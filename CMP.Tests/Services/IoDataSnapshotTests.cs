#nullable enable

using CMP.Models.kvsPlc;
using CMP.Models.Resource;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PolishIoData = CMP.Models.kvsPlc.IO.KvStruct.stPolishIOData;
using RollerIoData = CMP.Models.kvsPlc.Roll.KvsStructRollerIOData.stRollerIOData;
using PenIoData = CMP.Models.kvsPlc.Pen.KvsStructPenIOData.stPenIOData;
using DryIoData = CMP.Models.kvsPlc.Dry.KvsStructDryIOData.stDryIOData;
using PolishIo = CMP.Models.kvsPlc.IO.KvStruct;
using RollerIo = CMP.Models.kvsPlc.Roll.KvsStructRollerIOData;
using PenIo = CMP.Models.kvsPlc.Pen.KvsStructPenIOData;

namespace CMP.Tests.Services;

[TestClass]
public class IoDataSnapshotTests
{
    [TestMethod]
    public void PolishIoSnapshot_IsIndependentAndUnavailableBeforeFirstRead()
    {
        var polish = new Polish("PolishA", new KVLink("127.0.0.1"));
        Assert.IsFalse(polish.TryGetIoDataSnapshot(out _));

        polish.ApplyIoData(new PolishIoData
        {
            PlatenBreakwaterOutsideUpLimitSensor = true,
            PlatenWashValveControl = true
        });

        Assert.IsTrue(polish.TryGetIoDataSnapshot(out var first));
        Assert.IsTrue(first.PlatenBreakwaterOutsideUpLimitSensor);
        Assert.IsTrue(first.PlatenWashValveControl);
        Assert.AreNotSame(first, polish.IoData);

        first.PlatenWashValveControl = false;
        Assert.IsTrue(polish.TryGetIoDataSnapshot(out var second));
        Assert.IsTrue(second.PlatenWashValveControl);
    }

    [TestMethod]
    public void RollerIoSnapshot_IsIndependentPerStation()
    {
        var roller = new Roller("RollerA", new KVLink("127.0.0.1"));
        Assert.IsFalse(roller.TryGetIoDataSnapshot(1, out _));

        ApplyRollerIo(roller, 1, lowerRollerUp: true, coolingWater: false);
        ApplyRollerIo(roller, 2, lowerRollerUp: false, coolingWater: true);

        Assert.IsTrue(roller.TryGetIoDataSnapshot(1, out var station1));
        Assert.IsTrue(roller.TryGetIoDataSnapshot(2, out var station2));
        Assert.IsTrue(station1.LowerRollerCylinderUpPositionSensor);
        Assert.IsFalse(station1.CoolingWaterValveControl);
        Assert.IsFalse(station2.LowerRollerCylinderUpPositionSensor);
        Assert.IsTrue(station2.CoolingWaterValveControl);

        station1.CoolingWaterValveControl = true;
        Assert.IsTrue(roller.TryGetIoDataSnapshot(1, out var station1Again));
        Assert.IsFalse(station1Again.CoolingWaterValveControl);
    }

    [TestMethod]
    public void PenIoSnapshot_IsIndependentAndUnavailableBeforeFirstRead()
    {
        var pen = new Pen("PenA", new KVLink("127.0.0.1"));
        Assert.IsFalse(pen.TryGetIoDataSnapshot(out _));

        var source = new PenIoData { WFClampPositionSensor = true };
        var buffer = new byte[2048];
        PenIo.stPenIOData_ToByte(ref buffer, source);
        pen.ApplyIoData(buffer, 1);

        Assert.IsTrue(pen.TryGetIoDataSnapshot(out var first));
        Assert.IsTrue(first.WFClampPositionSensor);
        first.WFClampPositionSensor = false;
        Assert.IsTrue(pen.TryGetIoDataSnapshot(out var second));
        Assert.IsTrue(second.WFClampPositionSensor);
    }

    [TestMethod]
    public void DryIoSnapshot_IsIndependentAndUnavailableBeforeFirstRead()
    {
        var dry = new Dry("DryA", new KVLink("127.0.0.1"));
        Assert.IsFalse(dry.TryGetIoDataSnapshot(out _));

        dry.ApplyIoData(new DryIoData
        {
            WFClampPositionSensor = true,
            OuterRinseInletValveControl = true
        });

        Assert.IsTrue(dry.TryGetIoDataSnapshot(out var first));
        Assert.IsTrue(first.WFClampPositionSensor);
        Assert.IsTrue(first.OuterRinseInletValveControl);
        first.OuterRinseInletValveControl = false;
        Assert.IsTrue(dry.TryGetIoDataSnapshot(out var second));
        Assert.IsTrue(second.OuterRinseInletValveControl);
    }

    private static void ApplyRollerIo(Roller roller, int station, bool lowerRollerUp, bool coolingWater)
    {
        var source = new RollerIoData
        {
            LowerRollerCylinderUpPositionSensor = lowerRollerUp,
            CoolingWaterValveControl = coolingWater
        };
        var buffer = new byte[2048];
        RollerIo.stRollerIOData_ToByte(ref buffer, source);
        roller.ApplyIoData(buffer, station);
    }
}
