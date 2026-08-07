using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CMP.Constants.Plc.RobotControl;
using CMP.Services.Plc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CMP.Tests.Services;

[TestClass]
public class FeedingRobotPlcPerfTests
{
    private const string PlcIp = "192.168.2.50";
    private const int PlcPort = 7003;

    private bool _autoModeActive;
    private bool _upperArmSensor;
    private bool _lowerArmSensor;
    private bool _commandAccepted;
    private bool _confirmSwitchActive;
    private bool _robotSafePosition;
    private bool _useUpperArmActive;
    private bool _useLowerArmActive;

    private float _currentCoordinate;
    private int _alarmCode;
    private bool _alarmReset;

    public TestContext TestContext { get; set; } = null!;

    private static MitsubishiPlcService Plc => MitsubishiPlcService.RobotCtrlInstance;

    [TestMethod]
    public async Task ReadPlcDataAsync_Performance()
    {
        var plc = Plc;
        plc.Connect(PlcIp, PlcPort);
        if (!plc.IsConnected)
        {
            Assert.Inconclusive("PLC 未连接，无法执行 ReadPlcDataAsync 速度测试。");
        }

        const int iterations = 30;
        var samples = new List<long>(iterations);
        var failedRounds = new List<string>();

        var warmUp = await ReadPlcDataOnceForPerfAsync(plc);
        if (!warmUp.IsSuccess)
        {
            Assert.Fail("ReadPlcDataAsync 预热失败: " + string.Join(" | ", warmUp.Errors));
        }

        for (var i = 1; i <= iterations; i++)
        {
            var result = await ReadPlcDataOnceForPerfAsync(plc);
            samples.Add(result.ElapsedMilliseconds);

            if (!result.IsSuccess)
            {
                failedRounds.Add($"Round {i}: {string.Join(" | ", result.Errors)}");
            }
        }

        var avg = samples.Average();
        var min = samples.Min();
        var max = samples.Max();

        TestContext?.WriteLine($"ReadPlcDataAsync rounds={iterations}, avg={avg:F2} ms, min={min} ms, max={max} ms");

        for (var i = 0; i < samples.Count; i++)
        {
            TestContext?.WriteLine($"Read round {i + 1}: {samples[i]} ms");
        }

        if (failedRounds.Count > 0)
        {
            Assert.Fail("存在读取失败轮次: " + string.Join(" || ", failedRounds));
        }
    }


    private async Task<PerfRoundResult> ReadPlcDataOnceForPerfAsync(MitsubishiPlcService plc)
    {
        var sw = Stopwatch.StartNew();
        var errors = new List<string>();

        var mBatchGroups = new List<List<PlcBatchReadItemBase>>
        {
            new()
            {
                new PlcBatchReadItem<bool>(FeedingRobotAddress.GlobalAlarm, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.EmergencyStop, assign: _ => { })
            },
            new()
            {
                new PlcBatchReadItem<bool>(FeedingRobotAddress.Transfer, assign: _ => { })
            },
            new()
            {
                new PlcBatchReadItem<bool>(FeedingRobotAddress.Origin, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.LoadPort1Move, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.LoadPort2Move, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.LoadPort3Move, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.LoadPort4Move, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.LoadPort5Move, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.MiddleModuleMove, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.DryAMove, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.DryBMove, assign: _ => { })
            },
            new()
            {
                new PlcBatchReadItem<bool>(FeedingRobotAddress.Pause, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.AutoMode, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.UseUpperArm, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.UseLowerArm, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.BoxPickup, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.BoxPlace, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.WorkAreaPickup, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.WorkAreaPlace, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.FlipTo0, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.FlipTo180, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.WaitConfirm, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.ConfirmSwitch, assign: _ => { })
            },
            new()
            {
                new PlcBatchReadItem<bool>(FeedingRobotAddress.ManualMode, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromBox, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.PlaceToModule, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromDryA, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromDryB, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.PlaceToBox, assign: _ => { })
            },
            new()
            {
                new PlcBatchReadItem<bool>(FeedingRobotAddress.EnableStatus, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.OriginStatus, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.JogUpStatus, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.JogDownStatus, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.JogMiddleStatus, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.LimitAlarm, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.DriverAlarm, assign: _ => { })
            },
            new()
            {
                new PlcBatchReadItem<bool>(FeedingRobotAddress.RunningStatus, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.PausedStatus, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.StoppedStatus, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.ResetStatus, assign: _ => { })
            },
            new()
            {
                new PlcBatchReadItem<bool>(FeedingRobotAddress.BoxPickupStatus, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.BoxPlaceStatus, assign: _ => { })
            },
            new()
            {
                new PlcBatchReadItem<bool>(FeedingRobotAddress.WorkAreaPickupStatus, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.WorkAreaPlaceStatus, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.FlipTo0Status, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.FlipTo180Status, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.WaitConfirmStatus, assign: _ => { })
            },
            new()
            {
                new PlcBatchReadItem<bool>(FeedingRobotAddress.StationResetStatus, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.StationRunningStatus, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.StationStoppedStatus, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.ModuleAlarm, assign: _ => { })
            },
            new()
            {
                new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromBoxActiveStatus, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.PlaceToModuleActiveStatus, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromDryAActiveStatus, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromDryBActiveStatus, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.PlaceToBoxActiveStatus, assign: _ => { })
            },
            new()
            {
                new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromBoxDoneStatus, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.PlaceToModuleDoneStatus, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromDryADoneStatus, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromDryBDoneStatus, assign: _ => { }),
                new PlcBatchReadItem<bool>(FeedingRobotAddress.PlaceToBoxDoneStatus, assign: _ => { })
            }
        };

        var dStatusBatchItems = new List<PlcBatchReadItemBase>
        {
            new PlcBatchReadItem<float>(FeedingRobotAddress.CurrentCoordinate, assign: v => _currentCoordinate = v),
            new PlcBatchReadItem<int>(FeedingRobotAddress.AlarmCode, assign: v => _alarmCode = v)
        };

        var dBitBatchItems = new List<PlcBatchReadItemBase>
        {
            PlcBatchReadItem<bool>.Bind(FeedingRobotAddress.AutoModeStatus.Address, () => _autoModeActive,
                converter: MitsubishiPlcService.ToBitConverter(FeedingRobotAddress.AutoModeStatus)),
            PlcBatchReadItem<bool>.Bind(FeedingRobotAddress.UpperArmSensor.Address, () => _upperArmSensor,
                converter: MitsubishiPlcService.ToBitConverter(FeedingRobotAddress.UpperArmSensor)),
            PlcBatchReadItem<bool>.Bind(FeedingRobotAddress.LowerArmSensor.Address, () => _lowerArmSensor,
                converter: MitsubishiPlcService.ToBitConverter(FeedingRobotAddress.LowerArmSensor)),
            PlcBatchReadItem<bool>.Bind(FeedingRobotAddress.CommandAccept.Address, () => _commandAccepted,
                converter: MitsubishiPlcService.ToBitConverter(FeedingRobotAddress.CommandAccept)),
            PlcBatchReadItem<bool>.Bind(FeedingRobotAddress.ConfirmSwitchStatus.Address, () => _confirmSwitchActive,
                converter: MitsubishiPlcService.ToBitConverter(FeedingRobotAddress.ConfirmSwitchStatus)),
            PlcBatchReadItem<bool>.Bind(FeedingRobotAddress.RobotSafePosition.Address, () => _robotSafePosition,
                converter: MitsubishiPlcService.ToBitConverter(FeedingRobotAddress.RobotSafePosition)),
            PlcBatchReadItem<bool>.Bind(FeedingRobotAddress.UseUpperArmStatus.Address, () => _useUpperArmActive,
                converter: MitsubishiPlcService.ToBitConverter(FeedingRobotAddress.UseUpperArmStatus)),
            PlcBatchReadItem<bool>.Bind(FeedingRobotAddress.UseLowerArmStatus.Address, () => _useLowerArmActive,
                converter: MitsubishiPlcService.ToBitConverter(FeedingRobotAddress.UseLowerArmStatus))
        };

        var editableWordAddresses = new[]
        {
            FeedingRobotAddress.LoadPort1Position,
            FeedingRobotAddress.LoadPort2Position,
            FeedingRobotAddress.LoadPort3Position,
            FeedingRobotAddress.LoadPort4Position,
            FeedingRobotAddress.LoadPort5Position,
            FeedingRobotAddress.MiddleModulePosition,
            FeedingRobotAddress.DryAPosition,
            FeedingRobotAddress.DryBPosition,
            FeedingRobotAddress.JogSpeed,
            FeedingRobotAddress.AutoSpeed
        };

        var editableIntAddresses = new[]
        {
            FeedingRobotAddress.SourceBoxNo,
            FeedingRobotAddress.SourceLayer,
            FeedingRobotAddress.TargetBoxNo,
            FeedingRobotAddress.TargetLayer
        };

        var editableBatchItems = editableWordAddresses
            .Select(address => (PlcBatchReadItemBase)new PlcBatchReadItem<int>(address, assign: _ => { }))
            .ToList();

        var editableIntBatchItems = editableIntAddresses
            .Select(address => (PlcBatchReadItemBase)new PlcBatchReadItem<int>(address, assign: _ => { }))
            .ToList();

        var tB0B = plc.ReadAsync<bool>(FeedingRobotAddress.AlarmReset);
        var tDBits = plc.BatchReadAsync(dBitBatchItems);
        var tEditable = plc.BatchReadAsync(editableBatchItems);
        var tEditableInt = plc.BatchReadAsync(editableIntBatchItems);
        var tDStatus = plc.BatchReadAsync(dStatusBatchItems);
        var mBatchTasks = mBatchGroups.Select(group => plc.BatchReadAsync(group)).ToList();

        var allTasks = new List<Task>
        {
            tDStatus,
            tB0B,
            tDBits,
            tEditable,
            tEditableInt
        };
        allTasks.AddRange(mBatchTasks);

        await Task.WhenAll(allTasks);

        if (!tDStatus.Result.IsSuccess)
        {
            errors.Add("D status 批量读取失败: " + tDStatus.Result.Message);
        }

        if (!tB0B.Result.IsSuccess)
        {
            errors.Add("B0B 读取失败: " + tB0B.Result.Message);
        }
        else
        {
            _alarmReset = tB0B.Result.Content;
        }

        if (!tDBits.Result.IsSuccess)
        {
            errors.Add("D bit 批量读取失败: " + tDBits.Result.Message);
        }
        else
        {
            foreach (var item in dBitBatchItems.Where(item => !item.IsSuccess))
            {
                errors.Add(item.Address + " D bit 读取失败: " + item.OriginResult.Message);
            }
        }

        if (!tEditable.Result.IsSuccess)
        {
            errors.Add("D editable 批量读取失败: " + tEditable.Result.Message);
        }

        if (!tEditableInt.Result.IsSuccess)
        {
            errors.Add("W editable 批量读取失败: " + tEditableInt.Result.Message);
        }

        for (var i = 0; i < mBatchTasks.Count; i++)
        {
            if (!mBatchTasks[i].Result.IsSuccess)
            {
                errors.Add($"M 批次 {i + 1} 读取失败: {mBatchTasks[i].Result.Message}");
            }
        }

        sw.Stop();
        return new PerfRoundResult(sw.ElapsedMilliseconds, errors.Count == 0, errors);
    }

    private sealed record PerfRoundResult(long ElapsedMilliseconds, bool IsSuccess, List<string> Errors);
}
