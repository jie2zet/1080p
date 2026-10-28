using CMP.Constants;
using CMP.Constants.Plc;
using CMP.Models.kvsPlc;
using System.Collections.Concurrent;
using KHL = CMP.Models.kvsPlc.KvHostLink;
using PenSV = CMP.Models.kvsPlc.Pen.KvsStructPenCollectData;
using PolishSV = CMP.Models.kvsPlc.Polish.KvsStructPolishCollectData;
using RollerSV = CMP.Models.kvsPlc.Roll.KvsStructRollerCollectData;

namespace CMP.Services.Calibration
{
    /// <summary>
    /// 自动校准使用的压力通道操作。
    /// 负责把“工位 + 分区名”映射为 PLC 的压力设定参数、加压/排气命令以及过程数据回读字段，
    /// 屏蔽 Polish / Roller / Pen 三类设备在句柄和结构体上的差异。
    /// </summary>
    public static class CalibrationPressure
    {
        /// <summary>过程数据变量名（与本项目现有采集代码保持一致）。</summary>
        private const string ProcessDataVariable = "dataCollection";

        /// <summary>过程数据读取缓冲区大小（字节）。</summary>
        private const int ProcessDataBufferSize = 2048;

        /// <summary>
        /// 通道映射缓存。Target 不可变，可被界面 CanExecute 与后台采集线程共享。
        /// </summary>
        private static readonly ConcurrentDictionary<(string Station, string Channel), Target> TargetCache = new();

        /// <summary>通道类型，决定过程数据的解码方式。</summary>
        public enum ChannelKind
        {
            Polish,
            Roller,
            Pen
        }

        /// <summary>
        /// 单个校准通道的 PLC 操作描述。
        /// </summary>
        public sealed class Target
        {
            public ChannelKind Kind { get; init; }

            /// <summary>分区名，例如 Zone1、RollerA1Upper。</summary>
            public string Channel { get; init; } = string.Empty;

            /// <summary>压力设定参数句柄（DEV_ZF，2 words）。</summary>
            public uint PressureParamHandle { get; init; }

            /// <summary>加压命令句柄（DEV_W，1 word）。</summary>
            public uint PressCommandHandle { get; init; }

            /// <summary>加压命令码。</summary>
            public byte[] PressCommandValue { get; init; } = [];

            /// <summary>排气（压力重置）命令句柄（DEV_W，1 word）。</summary>
            public uint ResetCommandHandle { get; init; }

            /// <summary>排气（压力重置）命令码。</summary>
            public byte[] ResetCommandValue { get; init; } = [];

            /// <summary>压力校准公式开关句柄（DEV_ZF，1 word）；为空表示该通道没有此开关。</summary>
            public uint? CalibrationSwitchHandle { get; init; }

            /// <summary>参数允许的最小值（kPa）。</summary>
            public float MinValue { get; init; }

            /// <summary>参数允许的最大值（kPa）。</summary>
            public float MaxValue { get; init; }

            /// <summary>从过程数据缓冲区中解出该通道的实际压力。</summary>
            public Func<byte[], float> Decode { get; init; } = _ => throw new NotSupportedException();
        }

        /// <summary>
        /// 本项目支持自动校准的工位（与 CalibrationConfigTabViewModel 的 Tab 保持一致）。
        /// </summary>
        public static bool SupportsStation(string station) => station switch
        {
            "PolishA" or "PolishB" or "PolishC" or "PolishD" => true,
            "RollerA" or "RollerB" => true,
            "PenA" or "PenB" => true,
            _ => false
        };

        /// <summary>
        /// 获取“工位 + 分区”对应的压力通道；不支持的分区返回 null。
        /// </summary>
        public static Target? GetTarget(string station, string channel)
        {
            if (string.IsNullOrWhiteSpace(channel)) return null;

            var key = (station, channel);
            if (TargetCache.TryGetValue(key, out var cached)) return cached;

            var target = BuildTarget(station, channel);
            if (target != null) TargetCache[key] = target;
            return target;
        }

        private static Target? BuildTarget(string station, string channel)
        {
            return station switch
            {
                "PolishA" or "PolishB" or "PolishC" or "PolishD" => GetPolishTarget(station, channel),
                "RollerA" or "RollerB" => GetRollerTarget(station, channel),
                "PenA" or "PenB" => GetPenTarget(station, channel),
                _ => null
            };
        }

        /// <summary>
        /// 写入设定压力并发送加压命令。
        /// </summary>
        public static void Set(KVLink link, Target target, float pressure)
        {
            if (!float.IsFinite(pressure))
                throw new ArgumentException("Set must be a finite number.", nameof(pressure));

            if (pressure < target.MinValue || pressure > target.MaxValue)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(pressure),
                    pressure,
                    $"{target.Channel} 的设定压力超出允许范围 [{target.MinValue}, {target.MaxValue}] kPa。");
            }

            Check(KHL.KHLWriteDevicesAsWords(link.sock, KHLDevType.DEV_ZF, target.PressureParamHandle, 2,
                pressure.Value()), "写入设定压力");

            Check(KHL.KHLWriteDevicesAsWords(link.sock, KHLDevType.DEV_W, target.PressCommandHandle, 1,
                target.PressCommandValue), "发送加压命令");
        }

        /// <summary>
        /// 发送排气（压力重置）命令。
        /// </summary>
        public static void Vent(KVLink link, Target target)
        {
            Check(KHL.KHLWriteDevicesAsWords(link.sock, KHLDevType.DEV_W, target.ResetCommandHandle, 1,
                target.ResetCommandValue), "发送排气命令");
        }

        /// <summary>
        /// 读取过程数据并解出该通道的实际压力（PLC 侧反馈值）。
        /// </summary>
        public static float Read(KVLink link, Target target)
        {
            var buffer = new byte[ProcessDataBufferSize];
            Check(KHL.KHLReadVariable(link.sock, ProcessDataVariable, buffer, true), "读取过程数据");

            var pressure = target.Decode(buffer);
            if (!float.IsFinite(pressure))
                throw new InvalidOperationException($"{target.Channel} 的 PLC 压力值无效。");

            return pressure;
        }

        /// <summary>
        /// 写压力校准公式开关（0 关闭 / 1 恢复）。未配置开关句柄时不做任何操作。
        /// </summary>
        public static void SetCalibrationEnabled(KVLink link, Target target, bool enabled)
        {
            if (target.CalibrationSwitchHandle is not { } handle) return;

            var data = (enabled ? (ushort)1 : (ushort)0).Value();
            Check(KHL.KHLWriteDevicesAsWords(link.sock, KHLDevType.DEV_ZF, handle, 1, data),
                enabled ? "恢复压力校准公式" : "关闭压力校准公式");
        }

        #region 通道映射

        private static Target? GetPolishTarget(string station, string channel)
        {
            var calibrationSwitch = GetCalibrationSwitchHandle(station);

            return channel switch
            {
                "Zone1" => CreatePolish(channel,
                    PlcHandle.CmdPolish.Gas.Zone1.Param.Pressure.ParamHandle(),
                    PlcHandle.CmdPolish.Gas.Zone1.Press,
                    PlcHandle.CmdPolish.Gas.Zone1.Vent,
                    PlcHandle.CmdPolish.Gas.Zone1.Param.Pressure.MinValue(),
                    PlcHandle.CmdPolish.Gas.Zone1.Param.Pressure.MaxValue(),
                    1, calibrationSwitch),
                "Zone2" => CreatePolish(channel,
                    PlcHandle.CmdPolish.Gas.Zone2.Param.Pressure.ParamHandle(),
                    PlcHandle.CmdPolish.Gas.Zone2.Press,
                    PlcHandle.CmdPolish.Gas.Zone2.Vent,
                    PlcHandle.CmdPolish.Gas.Zone2.Param.Pressure.MinValue(),
                    PlcHandle.CmdPolish.Gas.Zone2.Param.Pressure.MaxValue(),
                    2, calibrationSwitch),
                "Zone3" => CreatePolish(channel,
                    PlcHandle.CmdPolish.Gas.Zone3.Param.Pressure.ParamHandle(),
                    PlcHandle.CmdPolish.Gas.Zone3.Press,
                    PlcHandle.CmdPolish.Gas.Zone3.Vent,
                    PlcHandle.CmdPolish.Gas.Zone3.Param.Pressure.MinValue(),
                    PlcHandle.CmdPolish.Gas.Zone3.Param.Pressure.MaxValue(),
                    3, calibrationSwitch),
                "Zone4" => CreatePolish(channel,
                    PlcHandle.CmdPolish.Gas.Zone4.Param.Pressure.ParamHandle(),
                    PlcHandle.CmdPolish.Gas.Zone4.Press,
                    PlcHandle.CmdPolish.Gas.Zone4.Vent,
                    PlcHandle.CmdPolish.Gas.Zone4.Param.Pressure.MinValue(),
                    PlcHandle.CmdPolish.Gas.Zone4.Param.Pressure.MaxValue(),
                    4, calibrationSwitch),
                "Zone5" => CreatePolish(channel,
                    PlcHandle.CmdPolish.Gas.Zone5.Param.Pressure.ParamHandle(),
                    PlcHandle.CmdPolish.Gas.Zone5.Press,
                    PlcHandle.CmdPolish.Gas.Zone5.Vent,
                    PlcHandle.CmdPolish.Gas.Zone5.Param.Pressure.MinValue(),
                    PlcHandle.CmdPolish.Gas.Zone5.Param.Pressure.MaxValue(),
                    5, calibrationSwitch),
                "Zone6" => CreatePolish(channel,
                    PlcHandle.CmdPolish.Gas.Zone6.Param.Pressure.ParamHandle(),
                    PlcHandle.CmdPolish.Gas.Zone6.Press,
                    PlcHandle.CmdPolish.Gas.Zone6.Vent,
                    PlcHandle.CmdPolish.Gas.Zone6.Param.Pressure.MinValue(),
                    PlcHandle.CmdPolish.Gas.Zone6.Param.Pressure.MaxValue(),
                    6, calibrationSwitch),
                "Zone7" => CreatePolish(channel,
                    PlcHandle.CmdPolish.Gas.Zone7.Param.Pressure.ParamHandle(),
                    PlcHandle.CmdPolish.Gas.Zone7.Press,
                    PlcHandle.CmdPolish.Gas.Zone7.Vent,
                    PlcHandle.CmdPolish.Gas.Zone7.Param.Pressure.MinValue(),
                    PlcHandle.CmdPolish.Gas.Zone7.Param.Pressure.MaxValue(),
                    7, calibrationSwitch),
                "Zone8" => CreatePolish(channel,
                    PlcHandle.CmdPolish.Gas.Zone8.Param.Pressure.ParamHandle(),
                    PlcHandle.CmdPolish.Gas.Zone8.Press,
                    PlcHandle.CmdPolish.Gas.Zone8.Vent,
                    PlcHandle.CmdPolish.Gas.Zone8.Param.Pressure.MinValue(),
                    PlcHandle.CmdPolish.Gas.Zone8.Param.Pressure.MaxValue(),
                    8, calibrationSwitch),
                "RRing" => CreatePolish(channel,
                    PlcHandle.CmdPolish.Gas.Retain.Param.Pressure.ParamHandle(),
                    PlcHandle.CmdPolish.Gas.Retain.Press,
                    PlcHandle.CmdPolish.Gas.Retain.Vent,
                    PlcHandle.CmdPolish.Gas.Retain.Param.Pressure.MinValue(),
                    PlcHandle.CmdPolish.Gas.Retain.Param.Pressure.MaxValue(),
                    9, calibrationSwitch),
                // Disk（修整器头下压力）与原项目保持一致，不纳入自动校准。
                _ => null
            };
        }

        private static Target? GetRollerTarget(string station, string channel)
        {
            // 滚轮上、下辊的公式开关地址不同，直接按完整分区名（含 Upper/Lower）取。
            var calibrationSwitch = GetCalibrationSwitchHandle(channel);

            return (station, channel) switch
            {
                ("RollerA", "RollerA1Lower") => CreateRoller(channel, Handle.CmdRollA1,
                    PlcHandle.CmdRollA1.Cylinder.LowerRollerPressure.Param.PressureSet.ParamHandle(),
                    PlcHandle.CmdRollA1.Cylinder.LowerRollerPressure.Pressure,
                    PlcHandle.CmdRollA1.Cylinder.LowerRollerPressure.PressureReset,
                    PlcHandle.CmdRollA1.Cylinder.LowerRollerPressure.Param.PressureSet.MinValue(),
                    PlcHandle.CmdRollA1.Cylinder.LowerRollerPressure.Param.PressureSet.MaxValue(),
                    1, false, calibrationSwitch),
                ("RollerA", "RollerA1Upper") => CreateRoller(channel, Handle.CmdRollA1,
                    PlcHandle.CmdRollA1.Cylinder.UpperRollerPressure.Param.PressureSet.ParamHandle(),
                    PlcHandle.CmdRollA1.Cylinder.UpperRollerPressure.Pressure,
                    PlcHandle.CmdRollA1.Cylinder.UpperRollerPressure.PressureReset,
                    PlcHandle.CmdRollA1.Cylinder.UpperRollerPressure.Param.PressureSet.MinValue(),
                    PlcHandle.CmdRollA1.Cylinder.UpperRollerPressure.Param.PressureSet.MaxValue(),
                    1, true, calibrationSwitch),
                ("RollerA", "RollerA2Lower") => CreateRoller(channel, Handle.CmdRollA2,
                    PlcHandle.CmdRollA2.Cylinder.LowerRollerPressure.Param.PressureSet.ParamHandle(),
                    PlcHandle.CmdRollA2.Cylinder.LowerRollerPressure.Pressure,
                    PlcHandle.CmdRollA2.Cylinder.LowerRollerPressure.PressureReset,
                    PlcHandle.CmdRollA2.Cylinder.LowerRollerPressure.Param.PressureSet.MinValue(),
                    PlcHandle.CmdRollA2.Cylinder.LowerRollerPressure.Param.PressureSet.MaxValue(),
                    2, false, calibrationSwitch),
                ("RollerA", "RollerA2Upper") => CreateRoller(channel, Handle.CmdRollA2,
                    PlcHandle.CmdRollA2.Cylinder.UpperRollerPressure.Param.PressureSet.ParamHandle(),
                    PlcHandle.CmdRollA2.Cylinder.UpperRollerPressure.Pressure,
                    PlcHandle.CmdRollA2.Cylinder.UpperRollerPressure.PressureReset,
                    PlcHandle.CmdRollA2.Cylinder.UpperRollerPressure.Param.PressureSet.MinValue(),
                    PlcHandle.CmdRollA2.Cylinder.UpperRollerPressure.Param.PressureSet.MaxValue(),
                    2, true, calibrationSwitch),
                ("RollerA", "RollerA3Lower") => CreateRoller(channel, Handle.CmdRollA3,
                    PlcHandle.CmdRollA3.Cylinder.LowerRollerPressure.Param.PressureSet.ParamHandle(),
                    PlcHandle.CmdRollA3.Cylinder.LowerRollerPressure.Pressure,
                    PlcHandle.CmdRollA3.Cylinder.LowerRollerPressure.PressureReset,
                    PlcHandle.CmdRollA3.Cylinder.LowerRollerPressure.Param.PressureSet.MinValue(),
                    PlcHandle.CmdRollA3.Cylinder.LowerRollerPressure.Param.PressureSet.MaxValue(),
                    3, false, calibrationSwitch),
                ("RollerA", "RollerA3Upper") => CreateRoller(channel, Handle.CmdRollA3,
                    PlcHandle.CmdRollA3.Cylinder.UpperRollerPressure.Param.PressureSet.ParamHandle(),
                    PlcHandle.CmdRollA3.Cylinder.UpperRollerPressure.Pressure,
                    PlcHandle.CmdRollA3.Cylinder.UpperRollerPressure.PressureReset,
                    PlcHandle.CmdRollA3.Cylinder.UpperRollerPressure.Param.PressureSet.MinValue(),
                    PlcHandle.CmdRollA3.Cylinder.UpperRollerPressure.Param.PressureSet.MaxValue(),
                    3, true, calibrationSwitch),
                ("RollerB", "RollerB1Lower") => CreateRoller(channel, Handle.CmdRollB1,
                    PlcHandle.CmdRollB1.Cylinder.LowerRollerPressure.Param.PressureSet.ParamHandle(),
                    PlcHandle.CmdRollB1.Cylinder.LowerRollerPressure.Pressure,
                    PlcHandle.CmdRollB1.Cylinder.LowerRollerPressure.PressureReset,
                    PlcHandle.CmdRollB1.Cylinder.LowerRollerPressure.Param.PressureSet.MinValue(),
                    PlcHandle.CmdRollB1.Cylinder.LowerRollerPressure.Param.PressureSet.MaxValue(),
                    1, false, calibrationSwitch),
                ("RollerB", "RollerB1Upper") => CreateRoller(channel, Handle.CmdRollB1,
                    PlcHandle.CmdRollB1.Cylinder.UpperRollerPressure.Param.PressureSet.ParamHandle(),
                    PlcHandle.CmdRollB1.Cylinder.UpperRollerPressure.Pressure,
                    PlcHandle.CmdRollB1.Cylinder.UpperRollerPressure.PressureReset,
                    PlcHandle.CmdRollB1.Cylinder.UpperRollerPressure.Param.PressureSet.MinValue(),
                    PlcHandle.CmdRollB1.Cylinder.UpperRollerPressure.Param.PressureSet.MaxValue(),
                    1, true, calibrationSwitch),
                ("RollerB", "RollerB2Lower") => CreateRoller(channel, Handle.CmdRollB2,
                    PlcHandle.CmdRollB2.Cylinder.LowerRollerPressure.Param.PressureSet.ParamHandle(),
                    PlcHandle.CmdRollB2.Cylinder.LowerRollerPressure.Pressure,
                    PlcHandle.CmdRollB2.Cylinder.LowerRollerPressure.PressureReset,
                    PlcHandle.CmdRollB2.Cylinder.LowerRollerPressure.Param.PressureSet.MinValue(),
                    PlcHandle.CmdRollB2.Cylinder.LowerRollerPressure.Param.PressureSet.MaxValue(),
                    2, false, calibrationSwitch),
                ("RollerB", "RollerB2Upper") => CreateRoller(channel, Handle.CmdRollB2,
                    PlcHandle.CmdRollB2.Cylinder.UpperRollerPressure.Param.PressureSet.ParamHandle(),
                    PlcHandle.CmdRollB2.Cylinder.UpperRollerPressure.Pressure,
                    PlcHandle.CmdRollB2.Cylinder.UpperRollerPressure.PressureReset,
                    PlcHandle.CmdRollB2.Cylinder.UpperRollerPressure.Param.PressureSet.MinValue(),
                    PlcHandle.CmdRollB2.Cylinder.UpperRollerPressure.Param.PressureSet.MaxValue(),
                    2, true, calibrationSwitch),
                ("RollerB", "RollerB3Lower") => CreateRoller(channel, Handle.CmdRollB3,
                    PlcHandle.CmdRollB3.Cylinder.LowerRollerPressure.Param.PressureSet.ParamHandle(),
                    PlcHandle.CmdRollB3.Cylinder.LowerRollerPressure.Pressure,
                    PlcHandle.CmdRollB3.Cylinder.LowerRollerPressure.PressureReset,
                    PlcHandle.CmdRollB3.Cylinder.LowerRollerPressure.Param.PressureSet.MinValue(),
                    PlcHandle.CmdRollB3.Cylinder.LowerRollerPressure.Param.PressureSet.MaxValue(),
                    3, false, calibrationSwitch),
                ("RollerB", "RollerB3Upper") => CreateRoller(channel, Handle.CmdRollB3,
                    PlcHandle.CmdRollB3.Cylinder.UpperRollerPressure.Param.PressureSet.ParamHandle(),
                    PlcHandle.CmdRollB3.Cylinder.UpperRollerPressure.Pressure,
                    PlcHandle.CmdRollB3.Cylinder.UpperRollerPressure.PressureReset,
                    PlcHandle.CmdRollB3.Cylinder.UpperRollerPressure.Param.PressureSet.MinValue(),
                    PlcHandle.CmdRollB3.Cylinder.UpperRollerPressure.Param.PressureSet.MaxValue(),
                    3, true, calibrationSwitch),
                _ => null
            };
        }

        private static Target? GetPenTarget(string station, string channel)
        {
            var calibrationSwitch = GetCalibrationSwitchHandle(channel);

            return (station, channel) switch
            {
                ("PenA", "PenA1") => CreatePen(channel, Handle.CmdPenA1,
                    PlcHandle.CmdPenA1.Cylinder.PenPressure.Param.PressureSet.ParamHandle(),
                    PlcHandle.CmdPenA1.Cylinder.PenPressure.Pressure,
                    PlcHandle.CmdPenA1.Cylinder.PenPressure.PressureReset,
                    PlcHandle.CmdPenA1.Cylinder.PenPressure.Param.PressureSet.MinValue(),
                    PlcHandle.CmdPenA1.Cylinder.PenPressure.Param.PressureSet.MaxValue(),
                    1, calibrationSwitch),
                ("PenB", "PenB1") => CreatePen(channel, Handle.CmdPenB1,
                    PlcHandle.CmdPenB1.Cylinder.PenPressure.Param.PressureSet.ParamHandle(),
                    PlcHandle.CmdPenB1.Cylinder.PenPressure.Pressure,
                    PlcHandle.CmdPenB1.Cylinder.PenPressure.PressureReset,
                    PlcHandle.CmdPenB1.Cylinder.PenPressure.Param.PressureSet.MinValue(),
                    PlcHandle.CmdPenB1.Cylinder.PenPressure.Param.PressureSet.MaxValue(),
                    1, calibrationSwitch),
                _ => null
            };
        }

        private static Target CreatePolish(string channel, uint pressureParamHandle, int pressCommand, int ventCommand,
            int minValue, int maxValue, int zoneIndex, uint? calibrationSwitch)
        {
            return Create(ChannelKind.Polish, channel,
                pressureParamHandle,
                Handle.CmdPolish,
                pressCommand.Value(),
                ventCommand.Value(),
                minValue,
                maxValue,
                calibrationSwitch,
                buffer => DecodePolish(buffer, zoneIndex));
        }

        private static Target CreateRoller(string channel, uint commandHandle, uint pressureParamHandle,
            int pressCommand, int resetCommand, int minValue, int maxValue, int cavity, bool upper,
            uint? calibrationSwitch)
        {
            return Create(ChannelKind.Roller, channel,
                pressureParamHandle,
                commandHandle,
                pressCommand.Value(),
                resetCommand.Value(),
                minValue,
                maxValue,
                calibrationSwitch,
                buffer => DecodeRoller(buffer, cavity, upper));
        }

        private static Target CreatePen(string channel, uint commandHandle, uint pressureParamHandle,
            int pressCommand, int resetCommand, int minValue, int maxValue, int cavity,
            uint? calibrationSwitch)
        {
            return Create(ChannelKind.Pen, channel,
                pressureParamHandle,
                commandHandle,
                pressCommand.Value(),
                resetCommand.Value(),
                minValue,
                maxValue,
                calibrationSwitch,
                buffer => DecodePen(buffer, cavity));
        }

        private static Target Create(ChannelKind kind, string channel, uint pressureParamHandle, uint commandHandle,
            byte[] pressCommandValue, byte[] resetCommandValue, float minValue, float maxValue,
            uint? calibrationSwitch, Func<byte[], float> decode)
        {
            return new Target
            {
                Kind = kind,
                Channel = channel,
                PressureParamHandle = pressureParamHandle,
                PressCommandHandle = commandHandle,
                PressCommandValue = pressCommandValue,
                ResetCommandHandle = commandHandle,
                ResetCommandValue = resetCommandValue,
                CalibrationSwitchHandle = calibrationSwitch,
                MinValue = minValue,
                MaxValue = maxValue,
                Decode = decode
            };
        }

        #endregion

        #region 过程数据解码

        private static float DecodePolish(byte[] buffer, int zoneIndex)
        {
            var data = new PolishSV.stPolishCollectData();
            PolishSV.ByteTo_stPolishCollectData(ref data, buffer);

            return zoneIndex switch
            {
                1 => data.Zone1PressureAct,
                2 => data.Zone2PressureAct,
                3 => data.Zone3PressureAct,
                4 => data.Zone4PressureAct,
                5 => data.Zone5PressureAct,
                6 => data.Zone6PressureAct,
                7 => data.Zone7PressureAct,
                8 => data.Zone8PressureAct,
                9 => data.RetainerRingAirBagPressureAct,
                _ => throw new ArgumentOutOfRangeException(nameof(zoneIndex), zoneIndex, "抛光分区索引无效。")
            };
        }

        private static float DecodeRoller(byte[] buffer, int cavity, bool upper)
        {
            var data = new RollerSV.stRollerCollectData();
            RollerSV.ByteTo_stRollerCollectData(ref data, buffer);

            return (cavity, upper) switch
            {
                (1, true) => data._1UpperRollerPressureAct,
                (1, false) => data._1LowerRollerPressureAct,
                (2, true) => data._2UpperRollerPressureAct,
                (2, false) => data._2LowerRollerPressureAct,
                (3, true) => data._3UpperRollerPressureAct,
                (3, false) => data._3LowerRollerPressureAct,
                _ => throw new ArgumentOutOfRangeException(nameof(cavity), cavity, "滚轮腔体索引无效。")
            };
        }

        private static float DecodePen(byte[] buffer, int cavity)
        {
            var data = new PenSV.stPenCollectData();
            PenSV.ByteTo_stPenCollectData(ref data, buffer);

            return cavity switch
            {
                1 => data._1PenHeadPressureAct,
                2 => data._2PenHeadPressureAct,
                _ => throw new ArgumentOutOfRangeException(nameof(cavity), cavity, "笔腔体索引无效。")
            };
        }

        #endregion

        #region 辅助方法

        private static uint? GetCalibrationSwitchHandle(string key)
        {
            return ChamberExponentMap.PressureCalibrationSwitchHandles.TryGetValue(key, out var handle)
                ? handle
                : null;
        }

        private static void Check(int error, string operation)
        {
            if (error != (int)KHLUserError.KHL_ERR_NONE)
                throw new InvalidOperationException($"{operation}失败，PLC 错误码：{error}。");
        }

        #endregion
    }
}
