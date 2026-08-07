using CMP.Models.Resource;
using CMP.Services.Log;
using CMP.Services.Plc;
using HandyControl.Controls;
using HandyControl.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CMP.Models.Recipe.Sequence
{
    /// <summary>
    /// 同一配方下发到多片时复用，避免重复读 JSON / 重复组包。
    /// </summary>
    public sealed class SequencePlcWritePayload
    {
        public ushort[] DataBlock { get; init; } = [];
    }

    public class SquenceWithPLC
    {
        private const int MaxWordsPerWrite = 960;
        /// <summary>
        /// 单 TCP 连接下多任务并发写会严重排队/超时；全局串行化 PLC 下发更安全。
        /// </summary>
        private static readonly SemaphoreSlim PlcSequenceWriteGate = new(1, 1);
        public const int PolishMainASideCheckedAddress = 40000;
        public const int PolishMainType = 40001;
        public const int PolishMainBSideCheckedAddress = 40002;
        public const int PolishASideOperationACheckedAddress = 40003;
        public const int PolishASideOperationTypeAddress = 40004;
        public const int PolishASideOperationBCheckedAddress = 40005;
        public const int PolishBSideOperationCCheckedAddress = 40006;
        public const int PolishBSideOperationTypeAddress = 40007;
        public const int PolishBSideOperationDCheckedAddress = 40008;
        public const int CleanMainASideCheckedAddress = 40015;
        public const int CleanMainTypeAddress = 40016;
        public const int CleanMainBSideCheckedAddress = 40017;
        public const int CleanASideRollerOp1CheckedAddress = 40030;
        public const int CleanASideRollerOp1TypeAddress = 40031;
        public const int CleanASideRollerOp2CheckedAddress = 40032;
        public const int CleanASideRollerOp2TypeAddress = 40033;
        public const int CleanASideRollerOp3CheckedAddress = 40034;
        public const int CleanASidePenOp1CheckedAddress = 40035;
        public const int CleanASidePenOp1TypeAddress = 40036;
        public const int CleanASidePenOp2CheckedAddress = 40037;
        public const int CleanASideDryOpCheckedAddress = 40038;
        public const int CleanBSideRollerOp1CheckedAddress = 40040;
        public const int CleanBSideRollerOp1TypeAddress = 40041;
        public const int CleanBSideRollerOp2CheckedAddress = 40042;
        public const int CleanBSideRollerOp2TypeAddress = 40043;
        public const int CleanBSideRollerOp3CheckedAddress = 40044;
        public const int CleanBSidePenOp1CheckedAddress = 40045;
        public const int CleanBSidePenOp1TypeAddress = 40046;
        public const int CleanBSidePenOp2CheckedAddress = 40047;
        public const int CleanBSideDryOpCheckedAddress = 40048;

        public const string Serial = "+";
        public const string Parallel = "*";
        public const string None = "/";
        public const string Selected = "Y"; //Y
        public const string Deselected = "N"; //N

        // 默认PLC通信实例
        private MitsubishiPlcService mcBinaryPlcComm = GlobalService.plcComm;

        public SquenceWithPLC()
        {
        }

        /// <summary>
        /// 由 <see cref="SequenceRecipeData"/> 预构建 PLC 一次写入缓冲（D40000~D40049），多片下发只构建一次。
        /// </summary>
        public SequencePlcWritePayload BuildPlcWritePayload(SequenceRecipeData data)
        {
            return new SequencePlcWritePayload
            {
                DataBlock = BuildDataBlock(data)
            };
        }

        /// <summary>
        /// 使用预构建 payload 一次下发；单连接内全局互斥，避免并发写挤爆同一 Socket。
        /// </summary>
        public async Task<bool> AssignWaferSequenceToPLCAsync(
            Wafer wafer,
            MitsubishiPlcService plcComm,
            SequencePlcWritePayload payload,
            CancellationToken cancellationToken = default)
        {
            await PlcSequenceWriteGate.WaitAsync(cancellationToken).ConfigureAwait(false);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (payload?.DataBlock == null || payload.DataBlock.Length == 0)
                {
                    Logger.Error($"Invalid PLC payload for wafer {wafer.WaferName}");
                    Growl.ErrorGlobal("Invalid PLC payload");
                    return false;
                }

                // 一次性写入 D40000~D40049
                var writeResult = plcComm.WriteUShortArray(CombineAddress(wafer, PolishMainASideCheckedAddress), payload.DataBlock);
                Logger.Info($"序列数据写入结果: {writeResult.IsSuccess}, 消息: {writeResult.Message}");

                stopwatch.Stop();
                Logger.Info($"PLC write operations for wafer {wafer.WaferName} took {stopwatch.ElapsedMilliseconds} ms");

                if (!writeResult.IsSuccess)
                {
                    Logger.Error($"Failed to write data block to PLC: {writeResult.Message}");
                    Growl.ErrorGlobal("Failed to write data block to PLC");
                    return false;
                }

                return true;
            }
            catch (OperationCanceledException)
            {
                Logger.Warn($"PLC write cancelled for wafer {wafer.WaferName}");
                return false;
            }
            finally
            {
                PlcSequenceWriteGate.Release();
            }
        }

        /// <summary>
        /// 将同一配方按 wafer 地址映射后一次性写入 PLC（单次 WriteUShortArray）。
        /// </summary>
        public async Task<bool> AssignWaferSequencesToPLCBatchAsync(
            IReadOnlyList<Wafer> wafers,
            MitsubishiPlcService plcComm,
            SequencePlcWritePayload payload,
            CancellationToken cancellationToken = default)
        {
            if (wafers == null || wafers.Count == 0)
            {
                return false;
            }

            await PlcSequenceWriteGate.WaitAsync(cancellationToken).ConfigureAwait(false);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (payload?.DataBlock == null || payload.DataBlock.Length == 0)
                {
                    Logger.Error("Invalid PLC payload for batch wafer write");
                    Growl.ErrorGlobal("Invalid PLC payload");
                    return false;
                }

                var addressedWafers = new List<(Wafer wafer, int startAddress)>(wafers.Count);
                foreach (var wafer in wafers)
                {
                    if (!TryParseDAddress(CombineAddress(wafer, PolishMainASideCheckedAddress), out var addr))
                    {
                        Logger.Error($"Invalid PLC address for wafer {wafer.WaferName}");
                        return false;
                    }

                    addressedWafers.Add((wafer, addr));
                }

                var payloadLength = payload.DataBlock.Length;
                var orderedWafers = addressedWafers
                    .OrderBy(x => x.startAddress)
                    .ToList();

                var segments = new List<List<(Wafer wafer, int startAddress)>>();
                var currentSegment = new List<(Wafer wafer, int startAddress)> { orderedWafers[0] };
                for (var i = 1; i < orderedWafers.Count; i++)
                {
                    var previous = orderedWafers[i - 1];
                    var current = orderedWafers[i];
                    if (current.startAddress == previous.startAddress + payloadLength)
                    {
                        currentSegment.Add(current);
                    }
                    else
                    {
                        segments.Add(currentSegment);
                        currentSegment = [current];
                    }
                }
                segments.Add(currentSegment);

                foreach (var segment in segments)
                {
                    var segmentStart = segment[0].startAddress;
                    var segmentEnd = segment[^1].startAddress;
                    var segmentTotalWords = (segmentEnd - segmentStart) + payloadLength;
                    var writeBuffer = new ushort[segmentTotalWords];

                    for (var i = 0; i < segment.Count; i++)
                    {
                        var offset = segment[i].startAddress - segmentStart;
                        Array.Copy(payload.DataBlock, 0, writeBuffer, offset, payloadLength);
                    }

                    for (var offset = 0; offset < writeBuffer.Length; offset += MaxWordsPerWrite)
                    {
                        var chunkLength = Math.Min(MaxWordsPerWrite, writeBuffer.Length - offset);
                        var chunk = new ushort[chunkLength];
                        Array.Copy(writeBuffer, offset, chunk, 0, chunkLength);

                        var chunkStartAddress = segmentStart + offset;
                        var writeResult = plcComm.WriteUShortArray($"D{chunkStartAddress}", chunk);
                        if (!writeResult.IsSuccess)
                        {
                            stopwatch.Stop();
                            Logger.Error($"Failed to batch write segment to PLC: start=D{chunkStartAddress}, words={chunkLength}, message={writeResult.Message}");
                            Growl.ErrorGlobal("Failed to batch write sequence to PLC");
                            return false;
                        }
                    }
                }

                stopwatch.Stop();
                Logger.Info($"Batch sequence segmented write success, wafers: {wafers.Count}, segments: {segments.Count}, elapsed: {stopwatch.ElapsedMilliseconds} ms");

                return true;
            }
            catch (OperationCanceledException)
            {
                Logger.Warn("Batch sequence write cancelled");
                return false;
            }
            finally
            {
                PlcSequenceWriteGate.Release();
            }
        }

        private ushort[] BuildDataBlock(SequenceRecipeData data)
        {
            var words = new ushort[50];

            void SetWord(int address, ushort value)
            {
                var index = address - PolishMainASideCheckedAddress;
                if (index >= 0 && index < words.Length)
                {
                    words[index] = value;
                }
            }

            SetWord(PolishMainASideCheckedAddress, data.PolishMainASideChecked ? ToUShort(Selected) : ToUShort(Deselected));
            SetWord(PolishMainType, ToUShort(ModeTypeConverter(data.PolishMainType)));
            SetWord(PolishMainBSideCheckedAddress, data.PolishMainBSideChecked ? ToUShort(Selected) : ToUShort(Deselected));
            SetWord(PolishASideOperationACheckedAddress, data.PolishASideOperationAChecked ? ToUShort(Selected) : ToUShort(Deselected));
            SetWord(PolishASideOperationTypeAddress, ToUShort(ModeTypeConverter(data.PolishASideOperationType)));
            SetWord(PolishASideOperationBCheckedAddress, data.PolishASideOperationBChecked ? ToUShort(Selected) : ToUShort(Deselected));
            SetWord(PolishBSideOperationCCheckedAddress, data.PolishBSideOperationCChecked ? ToUShort(Selected) : ToUShort(Deselected));
            SetWord(PolishBSideOperationTypeAddress, ToUShort(ModeTypeConverter(data.PolishBSideOperationType)));
            SetWord(PolishBSideOperationDCheckedAddress, data.PolishBSideOperationDChecked ? ToUShort(Selected) : ToUShort(Deselected));

            SetWord(CleanMainASideCheckedAddress, data.CleanMainASideChecked ? ToUShort(Selected) : ToUShort(Deselected));
            SetWord(CleanMainTypeAddress, ToUShort(ModeTypeConverter(data.CleanMainType)));
            SetWord(CleanMainBSideCheckedAddress, data.CleanMainBSideChecked ? ToUShort(Selected) : ToUShort(Deselected));

            SetWord(CleanASideRollerOp1CheckedAddress, data.CleanASideRollerOp1Checked ? ToUShort(Selected) : ToUShort(Deselected));
            SetWord(CleanASideRollerOp1TypeAddress, ToUShort(ModeTypeConverter(data.CleanASideRollerOp1Type)));
            SetWord(CleanASideRollerOp2CheckedAddress, data.CleanASideRollerOp2Checked ? ToUShort(Selected) : ToUShort(Deselected));
            SetWord(CleanASideRollerOp2TypeAddress, ToUShort(ModeTypeConverter(data.CleanASideRollerOp2Type)));
            SetWord(CleanASideRollerOp3CheckedAddress, data.CleanASideRollerOp3Checked ? ToUShort(Selected) : ToUShort(Deselected));
            SetWord(CleanASidePenOp1CheckedAddress, data.CleanASidePenOp1Checked ? ToUShort(Selected) : ToUShort(Deselected));
            SetWord(CleanASidePenOp1TypeAddress, ToUShort(ModeTypeConverter(data.CleanASidePenOp1Type)));
            SetWord(CleanASidePenOp2CheckedAddress, data.CleanASidePenOp2Checked ? ToUShort(Selected) : ToUShort(Deselected));
            SetWord(CleanASideDryOpCheckedAddress, data.CleanASideDryOpChecked ? ToUShort(Selected) : ToUShort(Deselected));

            SetWord(CleanBSideRollerOp1CheckedAddress, data.CleanBSideRollerOp1Checked ? ToUShort(Selected) : ToUShort(Deselected));
            SetWord(CleanBSideRollerOp1TypeAddress, ToUShort(ModeTypeConverter(data.CleanBSideRollerOp1Type)));
            SetWord(CleanBSideRollerOp2CheckedAddress, data.CleanBSideRollerOp2Checked ? ToUShort(Selected) : ToUShort(Deselected));
            SetWord(CleanBSideRollerOp2TypeAddress, ToUShort(ModeTypeConverter(data.CleanBSideRollerOp2Type)));
            SetWord(CleanBSideRollerOp3CheckedAddress, data.CleanBSideRollerOp3Checked ? ToUShort(Selected) : ToUShort(Deselected));
            SetWord(CleanBSidePenOp1CheckedAddress, data.CleanBSidePenOp1Checked ? ToUShort(Selected) : ToUShort(Deselected));
            SetWord(CleanBSidePenOp1TypeAddress, ToUShort(ModeTypeConverter(data.CleanBSidePenOp1Type)));
            SetWord(CleanBSidePenOp2CheckedAddress, data.CleanBSidePenOp2Checked ? ToUShort(Selected) : ToUShort(Deselected));
            SetWord(CleanBSideDryOpCheckedAddress, data.CleanBSideDryOpChecked ? ToUShort(Selected) : ToUShort(Deselected));

            return words;
        }

      
        // 将字符串转换为ushort
        private ushort ToUShort(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            // 根据您的实际需求进行转换
            switch (value)
            {
                case Selected:
                    return (ushort)'Y';
                case Deselected:
                    return (ushort)'N';
                case Serial:
                    return (ushort)'+';
                case Parallel:
                    return (ushort)'*';
                case None:
                    return (ushort)'/';
                default:
                    if (value.Length > 0)
                        return (ushort)value[0];
                    return 0;
            }
        }

        public string CombineAddress(Wafer wafer, int BaseAddress)
        {
            return "D" + ((wafer.LoadportID * 25 + wafer.SlotID) * 50 + BaseAddress);
        }

        private static bool TryParseDAddress(string address, out int value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(address) || !address.StartsWith("D", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return int.TryParse(address[1..], out value);
        }

        public string ModeTypeConverter(SequenceType sequenceType)
        {
            switch (sequenceType)
            {
                case SequenceType.Serial:
                    return Serial;
                case SequenceType.Parallel:
                    return Parallel;
                case SequenceType.None:
                    return None;
            }
            return string.Empty;
        }
    }
}