#nullable enable

using CMP.Models.kvsPlc;
using CMP.Models.Resource;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PolishCollectData = CMP.Models.kvsPlc.SV.KvStruct.stPolishCollectData;

namespace CMP.Tests.Services;

[TestClass]
public class PolishProcessDataSnapshotTests
{
    [TestMethod]
    public void Snapshot_IsUnavailableUntilProcessDataIsPublished()
    {
        var polish = CreatePolish();

        Assert.IsFalse(polish.TryGetProcessDataSnapshot(out _));
    }

    [TestMethod]
    public void Snapshot_CopiesTheCompleteProcessData_AndIsDetachedFromTheCache()
    {
        var polish = CreatePolish();
        var processData = new PolishCollectData
        {
            ProcessStepAct = 17,
            PlatenRotationSpeedAct = 12.5f,
            Zone8PressureAct = 8.25f,
            UnitIdleTimes = 59
        };

        polish.ApplyProcessData(processData);

        Assert.IsTrue(polish.TryGetProcessDataSnapshot(out var firstSnapshot));
        Assert.AreNotSame(processData, firstSnapshot);
        Assert.AreEqual((short)17, firstSnapshot.ProcessStepAct);
        Assert.AreEqual(12.5f, firstSnapshot.PlatenRotationSpeedAct);
        Assert.AreEqual(8.25f, firstSnapshot.Zone8PressureAct);
        Assert.AreEqual((short)59, firstSnapshot.UnitIdleTimes);

        firstSnapshot.PlatenRotationSpeedAct = 999f;
        firstSnapshot.UnitIdleTimes = 0;

        Assert.IsTrue(polish.TryGetProcessDataSnapshot(out var secondSnapshot));
        Assert.AreEqual(12.5f, secondSnapshot.PlatenRotationSpeedAct);
        Assert.AreEqual((short)59, secondSnapshot.UnitIdleTimes);
        Assert.AreEqual(
            12.5f,
            polish.GetSv(polish.BuildRuntimeKey(null, nameof(PolishCollectData.PlatenRotationSpeedAct)))?.Value);
    }

    [TestMethod]
    public void Snapshot_UsesTheLatestW530Values_ForAllServoFields()
    {
        var polish = CreatePolish();
        var processData = CreateProcessDataWithServoValues(false);
        polish.ApplyProcessData(processData);

        Assert.IsTrue(polish.TryGetProcessDataSnapshot(out var beforeW530));
        Assert.IsFalse(beforeW530.PlatenRotationMotorServoOn);

        polish.ApplyMotorServoStatus([true, false, true, false, true, false, true]);

        Assert.IsTrue(polish.TryGetProcessDataSnapshot(out var snapshot));
        Assert.IsTrue(snapshot.PlatenRotationMotorServoOn);
        Assert.IsFalse(snapshot.HeadRotationMotorServoOn);
        Assert.IsTrue(snapshot.HeadSweepMotorServoOn);
        Assert.IsFalse(snapshot.HeadUpDownMotorServoOn);
        Assert.IsTrue(snapshot.ConditionHeadRotationMotorServoOn);
        Assert.IsFalse(snapshot.ConditionSweepMotorServoOn);
        Assert.IsTrue(snapshot.SlurrySweepMotorServoOn);

        var newerProcessData = CreateProcessDataWithServoValues(false);
        newerProcessData.PlatenRotationSpeedAct = 88f;
        polish.ApplyProcessData(newerProcessData);

        Assert.IsTrue(polish.TryGetProcessDataSnapshot(out var mergedSnapshot));
        Assert.AreEqual(88f, mergedSnapshot.PlatenRotationSpeedAct);
        Assert.IsTrue(mergedSnapshot.PlatenRotationMotorServoOn);
        Assert.IsFalse(mergedSnapshot.HeadRotationMotorServoOn);
        Assert.IsTrue(mergedSnapshot.HeadSweepMotorServoOn);
        Assert.IsFalse(mergedSnapshot.HeadUpDownMotorServoOn);
        Assert.IsTrue(mergedSnapshot.ConditionHeadRotationMotorServoOn);
        Assert.IsFalse(mergedSnapshot.ConditionSweepMotorServoOn);
        Assert.IsTrue(mergedSnapshot.SlurrySweepMotorServoOn);
    }

    [TestMethod]
    public void Snapshot_MergesW530ValuesThatArriveBeforeProcessData()
    {
        var polish = CreatePolish();
        polish.ApplyMotorServoStatus([false, true, false, true, false, true, false]);

        Assert.IsFalse(polish.TryGetProcessDataSnapshot(out _));

        polish.ApplyProcessData(CreateProcessDataWithServoValues(true));

        Assert.IsTrue(polish.TryGetProcessDataSnapshot(out var snapshot));
        Assert.IsFalse(snapshot.PlatenRotationMotorServoOn);
        Assert.IsTrue(snapshot.HeadRotationMotorServoOn);
        Assert.IsFalse(snapshot.HeadSweepMotorServoOn);
        Assert.IsTrue(snapshot.HeadUpDownMotorServoOn);
        Assert.IsFalse(snapshot.ConditionHeadRotationMotorServoOn);
        Assert.IsTrue(snapshot.ConditionSweepMotorServoOn);
        Assert.IsFalse(snapshot.SlurrySweepMotorServoOn);
    }

    private static Polish CreatePolish()
    {
        return new Polish("PolishA", new KVLink("127.0.0.1"));
    }

    private static PolishCollectData CreateProcessDataWithServoValues(bool value)
    {
        return new PolishCollectData
        {
            PlatenRotationMotorServoOn = value,
            HeadRotationMotorServoOn = value,
            HeadSweepMotorServoOn = value,
            HeadUpDownMotorServoOn = value,
            ConditionHeadRotationMotorServoOn = value,
            ConditionSweepMotorServoOn = value,
            SlurrySweepMotorServoOn = value
        };
    }
}
