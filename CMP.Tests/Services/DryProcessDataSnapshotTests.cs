#nullable enable

using CMP.Models.kvsPlc;
using CMP.Models.Resource;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DryCollectData = CMP.Models.kvsPlc.Dry.KvsStructDryCollectData.stDryCollectData;

namespace CMP.Tests.Services;

[TestClass]
public class DryProcessDataSnapshotTests
{
    [TestMethod]
    public void Snapshot_IsUnavailableUntilProcessDataIsPublished()
    {
        var dry = CreateDry();

        Assert.IsFalse(dry.TryGetProcessDataSnapshot(out _));
    }

    [TestMethod]
    public void Snapshot_CopiesTheCompleteProcessData_AndIsDetachedFromTheCache()
    {
        var dry = CreateDry();
        var processData = new DryCollectData
        {
            ProcessStepAct = 17,
            ArmSweepSpeedAct = 12.5f,
            IPATankPressureAct = 8.25f
        };

        dry.ApplyProcessData(processData);

        Assert.IsTrue(dry.TryGetProcessDataSnapshot(out var firstSnapshot));
        Assert.AreNotSame(processData, firstSnapshot);
        Assert.AreEqual((short)17, firstSnapshot.ProcessStepAct);
        Assert.AreEqual(12.5f, firstSnapshot.ArmSweepSpeedAct);
        Assert.AreEqual(8.25f, firstSnapshot.IPATankPressureAct);

        firstSnapshot.ArmSweepSpeedAct = 999f;
        firstSnapshot.IPATankPressureAct = 0f;

        Assert.IsTrue(dry.TryGetProcessDataSnapshot(out var secondSnapshot));
        Assert.AreEqual(12.5f, secondSnapshot.ArmSweepSpeedAct);
        Assert.AreEqual(8.25f, secondSnapshot.IPATankPressureAct);
        Assert.AreEqual(
            12.5f,
            dry.GetSv(dry.BuildRuntimeKey(null, nameof(DryCollectData.ArmSweepSpeedAct)))?.Value);
    }

    [TestMethod]
    public void Snapshot_UsesTheLatestW539Values_ForBothServoFields()
    {
        var dry = CreateDry();
        dry.ApplyProcessData(CreateProcessDataWithServoValues(false));

        dry.ApplyMotorServoStatus([true, false]);

        Assert.IsTrue(dry.TryGetProcessDataSnapshot(out var snapshot));
        Assert.IsTrue(snapshot.ChuckMotorServoOn);
        Assert.IsFalse(snapshot.ArmSweepMotorServoOn);

        var newerProcessData = CreateProcessDataWithServoValues(false);
        newerProcessData.ArmSweepAngleAct = 88f;
        dry.ApplyProcessData(newerProcessData);

        Assert.IsTrue(dry.TryGetProcessDataSnapshot(out var mergedSnapshot));
        Assert.AreEqual(88f, mergedSnapshot.ArmSweepAngleAct);
        Assert.IsTrue(mergedSnapshot.ChuckMotorServoOn);
        Assert.IsFalse(mergedSnapshot.ArmSweepMotorServoOn);
    }

    [TestMethod]
    public void Snapshot_MergesW539ValuesThatArriveBeforeProcessData()
    {
        var dry = CreateDry();
        dry.ApplyMotorServoStatus([false, true]);

        Assert.IsFalse(dry.TryGetProcessDataSnapshot(out _));

        dry.ApplyProcessData(CreateProcessDataWithServoValues(false));

        Assert.IsTrue(dry.TryGetProcessDataSnapshot(out var snapshot));
        Assert.IsFalse(snapshot.ChuckMotorServoOn);
        Assert.IsTrue(snapshot.ArmSweepMotorServoOn);
    }

    private static Dry CreateDry()
    {
        return new Dry("DryA", new KVLink("127.0.0.1"));
    }

    private static DryCollectData CreateProcessDataWithServoValues(bool value)
    {
        return new DryCollectData
        {
            ChuckMotorServoOn = value,
            ArmSweepMotorServoOn = value
        };
    }
}
