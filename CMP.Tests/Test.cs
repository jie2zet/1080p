using CMP.Constants.Plc.RobotControl;
using CMP.Services.Plc;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMP.Tests;

[TestClass]
public class WaferTest
{
    private float _d10418;
    private float _d10420;
    private float _d10422;
    private float _d10424;
    private bool _upperArmSensorBatch;
    private bool _lowerArmSensorBatch;
    private bool _commandAcceptBatch;
    private bool _robotSafePositionBatch;
    private bool _limitAlarmBatch;
    private bool _driverAlarmBatch;
    private int _alarmCodeBatch;


    public static MitsubishiPlcService RobotControlPlc = MitsubishiPlcService.RobotCtrlInstance;

    [TestMethod]
    public async Task BatchReadExample()
    {
        var plc = RobotControlPlc;
        plc.Connect("192.168.2.50", 7003);
        if (!plc.IsConnected)
        {
            Assert.Inconclusive("PLC 未连接，无法执行读取示例。请先连接 RobotCtrl PLC。");
        }

        var batchItems = new List<PlcBatchReadItemBase>
        {
            PlcBatchReadItem<float>.Bind(PolishASideArm1Address.XStandbyPosition, () => _d10418),
            PlcBatchReadItem<float>.Bind(PolishASideArm1Address.XTransferArmPosition, () => _d10420),
            PlcBatchReadItem<float>.Bind(PolishASideArm1Address.XPolishAPosition, () => _d10422),
            PlcBatchReadItem<float>.Bind(PolishASideArm1Address.XPolishBPosition, () => _d10424)
        };

        var result = await plc.BatchReadAsync(batchItems);
        Assert.IsTrue(result.IsSuccess, result.Message);

        foreach (var item in batchItems)
        {
            Assert.IsTrue(item.IsSuccess, $"{item.Address} 读取失败: {item.OriginResult.Message}");
        }
    }

    public static float ShortsToFloat_LowFirst(short word1, short word2)
    {
        byte[] bytes = new byte[4];
        // 将 word1 (低16位) 放在前面
        BitConverter.GetBytes(word1).CopyTo(bytes, 0);
        // 将 word2 (高16位) 放在后面
        BitConverter.GetBytes(word2).CopyTo(bytes, 2);

        return BitConverter.ToSingle(bytes, 0);
    }

    [TestMethod]
    public async Task BatchReadCompareWithSingleRead()
    {
        var plc = RobotControlPlc;
        plc.Connect("192.168.2.50", 7004);
        if (!plc.IsConnected)
        {
            Assert.Inconclusive("PLC 未连接，无法执行读取示例。请先连接 RobotCtrl PLC。");
        }
        if (!plc.IsConnected)
        {
            Assert.Inconclusive("PLC 未连接，无法执行对比校验。请先连接 RobotCtrl PLC。");
        }

        var result1 = plc.Write(FeedingRobotAddress.Enable, true);
        plc.Write(FeedingRobotAddress.Enable, false);
        // 写入 -948.154
        var test12 = plc.Write(PolishASideArm1Address.XStandbyPosition, (int)(-948.154 * 1000));
        var batchItems = new List<PlcBatchReadItemBase>
        {
            PlcBatchReadItem<int>.Bind(PolishASideArm1Address.XStandbyPosition, value =>
            {
                _d10418 = value / 1000.0f;
            }),
            PlcBatchReadItem<int>.Bind(PolishASideArm1Address.XTransferArmPosition, value => _d10420 = value / 1000.0f),
            PlcBatchReadItem<int>.Bind(PolishASideArm1Address.XPolishAPosition, value => _d10422 = value / 1000.0f),
            PlcBatchReadItem<int>.Bind(PolishASideArm1Address.XPolishBPosition, value => _d10424 = value / 1000.0f)
        };

        var batchResult = await plc.BatchReadAsync(batchItems);
        Assert.IsTrue(batchResult.IsSuccess, batchResult.Message);

        var d1000Single = await plc.ReadAsync<int>(PolishASideArm1Address.XStandbyPosition);
        var d1020Single = await plc.ReadAsync<float>(PolishASideArm1Address.XTransferArmPosition);
        var d1030Single = await plc.ReadAsync<float>(PolishASideArm1Address.XPolishAPosition);
        var d1040Single = await plc.ReadAsync<float>(PolishASideArm1Address.XPolishBPosition);

        // -948154 读出来 -948.154
        var test1 = d1000Single.Content / 1000.0;
        Assert.IsTrue(d1000Single.IsSuccess, d1000Single.Message);
        Assert.IsTrue(d1020Single.IsSuccess, d1020Single.Message);
        Assert.IsTrue(d1030Single.IsSuccess, d1030Single.Message);
        Assert.IsTrue(d1040Single.IsSuccess, d1040Single.Message);

        const double tolerance = 1e-4;
        Assert.AreEqual(d1000Single.Content, _d10418, tolerance, "D10418 批量读取与单独读取不一致");
        Assert.AreEqual(d1020Single.Content, _d10420, tolerance, "D10420 批量读取与单独读取不一致");
        Assert.AreEqual(d1030Single.Content, _d10422, tolerance, "D10422 批量读取与单独读取不一致");
        Assert.AreEqual(d1040Single.Content, _d10424, tolerance, "D10424 批量读取与单独读取不一致");
    }

    [TestMethod]
    public async Task BatchReadCompareWithSingleRead_ForSensorBits()
    {
        var plc = RobotControlPlc;
        plc.Connect("192.168.2.50", 7005);
        if (!plc.IsConnected)
        {
            Assert.Inconclusive("PLC 未连接，无法执行传感器状态对比。请先连接 RobotCtrl PLC。");
        }

        static Func<short[], bool> ToBitConverter(int bitIndex)
        {
            return words => (unchecked((ushort)words[0]) & (1 << bitIndex)) != 0;
        }

        var upper = FeedingRobotAddress.UpperArmSensor;
        var lower = FeedingRobotAddress.LowerArmSensor;
        var cmdAccept = FeedingRobotAddress.CommandAccept;
        var safe = FeedingRobotAddress.RobotSafePosition;

        var batchItems = new List<PlcBatchReadItemBase>
        {
            PlcBatchReadItem<bool>.Bind(upper.Address, () => _upperArmSensorBatch, converter: ToBitConverter(upper.BitIndex)),
            PlcBatchReadItem<bool>.Bind(lower.Address, () => _lowerArmSensorBatch, converter: ToBitConverter(lower.BitIndex)),
            PlcBatchReadItem<bool>.Bind(cmdAccept.Address, () => _commandAcceptBatch, converter: ToBitConverter(cmdAccept.BitIndex)),
            PlcBatchReadItem<bool>.Bind(safe.Address, () => _robotSafePositionBatch, converter: ToBitConverter(safe.BitIndex))
        };

        var batchResult = await plc.BatchReadAsync(batchItems);
        Assert.IsTrue(batchResult.IsSuccess, batchResult.Message);
        foreach (var item in batchItems)
        {
            Assert.IsTrue(item.IsSuccess, $"{item.Address} 批量位读取失败: {item.OriginResult.Message}");
        }

        var upperSingle = await plc.ReadDBitAsync(upper);
        var lowerSingle = await plc.ReadDBitAsync(lower);
        var cmdAcceptSingle = await plc.ReadDBitAsync(cmdAccept);
        var safeSingle = await plc.ReadDBitAsync(safe);

        Assert.IsTrue(upperSingle.IsSuccess, upperSingle.Message);
        Assert.IsTrue(lowerSingle.IsSuccess, lowerSingle.Message);
        Assert.IsTrue(cmdAcceptSingle.IsSuccess, cmdAcceptSingle.Message);
        Assert.IsTrue(safeSingle.IsSuccess, safeSingle.Message);

        Assert.AreEqual(upperSingle.Content, _upperArmSensorBatch, "UpperArmSensor 批量读取与单独读取不一致");
        Assert.AreEqual(lowerSingle.Content, _lowerArmSensorBatch, "LowerArmSensor 批量读取与单独读取不一致");
        Assert.AreEqual(cmdAcceptSingle.Content, _commandAcceptBatch, "CommandAccept 批量读取与单独读取不一致");
        Assert.AreEqual(safeSingle.Content, _robotSafePositionBatch, "RobotSafePosition 批量读取与单独读取不一致");
    }

    [TestMethod]
    public async Task BatchReadCompareWithSingleRead_ForMStatusAndAlarmCode()
    {
        var plc = RobotControlPlc;
        plc.Connect("192.168.2.50", 7003);
        if (!plc.IsConnected)
        {
            Assert.Inconclusive("PLC 未连接，无法执行 M 地址与报警码对比。请先连接 RobotCtrl PLC。");
        }

        _limitAlarmBatch = false;
        _driverAlarmBatch = false;
        _alarmCodeBatch = 0;

        // 1) 单独读取（基准值）
        var limitSingle = await plc.ReadAsync<bool>(FeedingRobotAddress.LimitAlarm);
        var driverSingle = await plc.ReadAsync<bool>(FeedingRobotAddress.DriverAlarm);
        var alarmCodeSingle = await plc.ReadAsync<int>(FeedingRobotAddress.AlarmCode);

        Assert.IsTrue(limitSingle.IsSuccess, limitSingle.Message);
        Assert.IsTrue(driverSingle.IsSuccess, driverSingle.Message);
        Assert.IsTrue(alarmCodeSingle.IsSuccess, alarmCodeSingle.Message);

            var mBatchItems = new List<PlcBatchReadItemBase>
        {
                PlcBatchReadItem<bool>.Bind(FeedingRobotAddress.LimitAlarm, () => _limitAlarmBatch, addressType: PlcBatchAddressType.M),
                PlcBatchReadItem<bool>.Bind(FeedingRobotAddress.DriverAlarm, () => _driverAlarmBatch, addressType: PlcBatchAddressType.M)
        };

        // 2) 批量读取（待验证）
            var mBatchResult = await plc.BatchReadAsync(mBatchItems);
        Assert.IsTrue(mBatchResult.IsSuccess, mBatchResult.Message);
        foreach (var item in mBatchItems)
        {
            Assert.IsTrue(item.IsSuccess, $"{item.Address} 批量 M 读取失败: {item.OriginResult.Message}");
        }

        var alarmCodeBatchItems = new List<PlcBatchReadItemBase>
        {
            PlcBatchReadItem<int>.Bind(FeedingRobotAddress.AlarmCode, () => _alarmCodeBatch)
        };

        var alarmCodeBatchResult = await plc.BatchReadAsync(alarmCodeBatchItems);
        Assert.IsTrue(alarmCodeBatchResult.IsSuccess, alarmCodeBatchResult.Message);
        Assert.IsTrue(alarmCodeBatchItems[0].IsSuccess, $"{FeedingRobotAddress.AlarmCode} 批量读取失败: {alarmCodeBatchItems[0].OriginResult.Message}");

        // 3) 对比
        Assert.AreEqual(limitSingle.Content, _limitAlarmBatch, "LimitAlarm(M5007) 批量读取与单独读取不一致");
        Assert.AreEqual(driverSingle.Content, _driverAlarmBatch, "DriverAlarm(M5009) 批量读取与单独读取不一致");
        Assert.AreEqual(alarmCodeSingle.Content, _alarmCodeBatch, "AlarmCode(D5006) 批量读取与单独读取不一致");
    }


    [TestMethod]
    public async Task BatchReadTest()
    {

        var plc = RobotControlPlc;
        plc.Connect("192.168.2.50", 7005);
        if (!plc.IsConnected)
        {
            Assert.Inconclusive("PLC 未连接，无法执行 M 地址与报警码对比。请先连接 RobotCtrl PLC。");
        }

        bool GlobalAlarm = false;
        bool EmergencyStopToggle = false;
        bool TransferToggle = false;
        bool OriginToggle = false;
        bool LoadPort1MoveToggle = false;
        bool LoadPort2MoveToggle = false;
        bool LoadPort3MoveToggle = false;
        bool LoadPort4MoveToggle = false;
        bool LoadPort5MoveToggle = false;
        bool MiddleModuleMoveToggle = false;
        bool DryAMoveToggle = false;
        bool DryBMoveToggle = false;
        bool PauseToggle = false;
        bool AutoModeToggle = false;
        bool UseUpperArmToggle = false;
        bool UseLowerArmToggle = false;
        bool BoxPickupToggle = false;
        bool BoxPlaceToggle = false;
        bool WorkAreaPickupToggle = false;
        bool WorkAreaPlaceToggle = false;
        bool FlipTo0Toggle = false;
        bool FlipTo180Toggle = false;
        bool WaitConfirmToggle = false;
        bool ConfirmSwitchToggle = false;
        bool ManualModeToggle = false;
        bool PickFromBoxToggle = false;
        bool PlaceToModuleToggle = false;
        bool PickFromDryAToggle = false;
        bool PickFromDryBToggle = false;
        bool PlaceToBoxToggle = false;
        bool IsEnabled = false;
        bool IsAtOrigin = false;
        bool JogUpActive = false;
        bool JogDownActive = false;
        bool JogMiddleActive = false;
        bool LimitAlarm = false;
        bool DriverAlarm = false;
        bool IsRunning = false;
        bool IsPaused = false;
        bool IsStopped = false;
        bool IsReset = false;
        bool BoxPickup = false;
        bool BoxPlace = false;
        bool WorkAreaPickup = false;
        bool WorkAreaPlace = false;
        bool FlipTo0 = false;
        bool FlipTo180 = false;
        bool WaitConfirm = false;
        bool StationIsReset = false;
        bool StationIsRunning = false;
        bool StationIsStopped = false;
        bool ModuleAlarm = false;
        bool PickFromBoxActive = false;
        bool PlaceToModuleActive = false;
        bool PickFromDryAActive = false;
        bool PickFromDryBActive = false;
        bool PlaceToBoxActive = false;
        bool PickFromBoxDone = false;
        bool PlaceToModuleDone = false;
        bool PickFromDryADone = false;
        bool PickFromDryBDone = false;
        bool PlaceToBoxDone = false;
        float CurrentCoordinate;

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var mBatchGroups = new List<List<PlcBatchReadItemBase>>
            {
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.GlobalAlarm, assign: value => GlobalAlarm = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.EmergencyStop, assign: value => EmergencyStopToggle = value)
                },
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.Transfer, assign: value => TransferToggle = value)
                },
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.Origin, assign: value => OriginToggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.LoadPort1Move, assign: value => LoadPort1MoveToggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.LoadPort2Move, assign: value => LoadPort2MoveToggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.LoadPort3Move, assign: value => LoadPort3MoveToggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.LoadPort4Move, assign: value => LoadPort4MoveToggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.LoadPort5Move, assign: value => LoadPort5MoveToggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.MiddleModuleMove, assign: value => MiddleModuleMoveToggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.DryAMove, assign: value => DryAMoveToggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.DryBMove, assign: value => DryBMoveToggle = value)
                },
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.Pause, assign: value => PauseToggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.AutoMode, assign: value => AutoModeToggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.UseUpperArm, assign: value => UseUpperArmToggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.UseLowerArm, assign: value => UseLowerArmToggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.BoxPickup, assign: value => BoxPickupToggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.BoxPlace, assign: value => BoxPlaceToggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.WorkAreaPickup, assign: value => WorkAreaPickupToggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.WorkAreaPlace, assign: value => WorkAreaPlaceToggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.FlipTo0, assign: value => FlipTo0Toggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.FlipTo180, assign: value => FlipTo180Toggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.WaitConfirm, assign: value => WaitConfirmToggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.ConfirmSwitch, assign: value => ConfirmSwitchToggle = value)
                },
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.ManualMode, assign: value => ManualModeToggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromBox, assign: value => PickFromBoxToggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PlaceToModule, assign: value => PlaceToModuleToggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromDryA, assign: value => PickFromDryAToggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromDryB, assign: value => PickFromDryBToggle = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PlaceToBox, assign: value => PlaceToBoxToggle = value)
                },
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.EnableStatus, assign: value => IsEnabled = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.OriginStatus, assign: value => IsAtOrigin = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.JogUpStatus, assign: value => JogUpActive = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.JogDownStatus, assign: value => JogDownActive = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.JogMiddleStatus, assign: value => JogMiddleActive = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.LimitAlarm, assign: value => LimitAlarm = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.DriverAlarm, assign: value => DriverAlarm = value)
                },
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.RunningStatus, assign: value => IsRunning = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PausedStatus, assign: value => IsPaused = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.StoppedStatus, assign: value => IsStopped = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.ResetStatus, assign: value => IsReset = value)
                },
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.BoxPickupStatus, assign: value => BoxPickup = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.BoxPlaceStatus, assign: value => BoxPlace = value)
                },
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.WorkAreaPickupStatus, assign: value => WorkAreaPickup = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.WorkAreaPlaceStatus, assign: value => WorkAreaPlace = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.FlipTo0Status, assign: value => FlipTo0 = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.FlipTo180Status, assign: value => FlipTo180 = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.WaitConfirmStatus, assign: value => WaitConfirm = value)
                },
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.StationResetStatus, assign: value => StationIsReset = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.StationRunningStatus, assign: value => StationIsRunning = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.StationStoppedStatus, assign: value => StationIsStopped = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.ModuleAlarm, assign: value => ModuleAlarm = value)
                },
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromBoxActiveStatus, assign: value => PickFromBoxActive = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PlaceToModuleActiveStatus, assign: value => PlaceToModuleActive = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromDryAActiveStatus, assign: value => PickFromDryAActive = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromDryBActiveStatus, assign: value => PickFromDryBActive = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PlaceToBoxActiveStatus, assign: value => PlaceToBoxActive = value)
                },
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromBoxDoneStatus, assign: value => PickFromBoxDone = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PlaceToModuleDoneStatus, assign: value => PlaceToModuleDone = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromDryADoneStatus, assign: value => PickFromDryADone = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromDryBDoneStatus, assign: value => PickFromDryBDone = value),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PlaceToBoxDoneStatus, assign: value => PlaceToBoxDone = value)
                }
            };
        var mBatchTasks = mBatchGroups.Select(group => plc.BatchReadAsync(group)).ToList();
        Task.WhenAll(mBatchTasks).Wait();
        sw.Stop();
        var elapsedTime = sw.ElapsedMilliseconds;
        
    }


    public (int, int) GetCurrentCarrierAndPosition(int WaferIndex)
    {
        if (WaferIndex == -1)
        {
            return (-1, -1);
        }
        var carrierCapacity = 25;
        var carrierNumber = (WaferIndex - 1) / carrierCapacity + 1;
        var positionInCarrier = (WaferIndex - 1) % carrierCapacity + 1;
        return (carrierNumber, positionInCarrier);
    }
}