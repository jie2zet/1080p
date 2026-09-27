#nullable enable

using CMP.Models.kvsPlc;
using CMP.Models.Resource;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PenCollectData = CMP.Models.kvsPlc.Pen.KvsStructPenCollectData.stPenCollectData;

namespace CMP.Tests.Services;

[TestClass]
public class PenProcessDataSnapshotTests
{
    [TestMethod]
    public void Snapshot_IsUnavailableUntilProcessDataIsPublished()
    {
        var pen = CreatePen();

        Assert.IsFalse(pen.TryGetProcessDataSnapshot(out _));
    }

    [TestMethod]
    public void Snapshot_CopiesTheCompleteProcessData_AndIsDetachedFromTheCache()
    {
        var pen = CreatePen();
        var processData = new PenCollectData
        {
            _1ProcessStepAct = 17,
            _2PenArmSweepSpeedAct = 12.5f,
            _2DIWAllOpenTimeAct = 8.25f
        };

        pen.ApplyProcessData(processData);

        Assert.IsTrue(pen.TryGetProcessDataSnapshot(out var firstSnapshot));
        Assert.AreNotSame(processData, firstSnapshot);
        Assert.AreEqual((short)17, firstSnapshot._1ProcessStepAct);
        Assert.AreEqual(12.5f, firstSnapshot._2PenArmSweepSpeedAct);
        Assert.AreEqual(8.25f, firstSnapshot._2DIWAllOpenTimeAct);

        firstSnapshot._2PenArmSweepSpeedAct = 999f;
        firstSnapshot._2DIWAllOpenTimeAct = 0f;

        Assert.IsTrue(pen.TryGetProcessDataSnapshot(out var secondSnapshot));
        Assert.AreEqual(12.5f, secondSnapshot._2PenArmSweepSpeedAct);
        Assert.AreEqual(8.25f, secondSnapshot._2DIWAllOpenTimeAct);
        Assert.AreEqual(
            12.5f,
            pen.GetSv(pen.BuildRuntimeKey(null, nameof(PenCollectData._2PenArmSweepSpeedAct)))?.Value);
    }

    [TestMethod]
    public void Snapshot_UsesTheLatestW537Values_ForEachStation()
    {
        var pen = CreatePen();
        pen.ApplyProcessData(CreateProcessDataWithServoValues(false));

        pen.ApplyMotorServoStatus([true, false, true], "_1");
        pen.ApplyMotorServoStatus([false, true, false], "_2");

        Assert.IsTrue(pen.TryGetProcessDataSnapshot(out var snapshot));
        AssertStation1(snapshot, true, false, true);
        AssertStation2(snapshot, false, true, false);

        var newerProcessData = CreateProcessDataWithServoValues(false);
        newerProcessData._2PenArmSweepAngleAct = 88f;
        pen.ApplyProcessData(newerProcessData);

        Assert.IsTrue(pen.TryGetProcessDataSnapshot(out var mergedSnapshot));
        Assert.AreEqual(88f, mergedSnapshot._2PenArmSweepAngleAct);
        AssertStation1(mergedSnapshot, true, false, true);
        AssertStation2(mergedSnapshot, false, true, false);
    }

    [TestMethod]
    public void Snapshot_MergesW537ValuesThatArriveBeforeProcessData_PerStation()
    {
        var pen = CreatePen();
        pen.ApplyMotorServoStatus([false, true, false], "_2");

        Assert.IsFalse(pen.TryGetProcessDataSnapshot(out _));

        pen.ApplyProcessData(CreateProcessDataWithServoValues(true));

        Assert.IsTrue(pen.TryGetProcessDataSnapshot(out var snapshot));
        AssertStation1(snapshot, true, true, true);
        AssertStation2(snapshot, false, true, false);
    }

    private static Pen CreatePen()
    {
        return new Pen("PenA", new KVLink("127.0.0.1"));
    }

    private static PenCollectData CreateProcessDataWithServoValues(bool value)
    {
        return new PenCollectData
        {
            _1ChuckMotorServoOn = value,
            _1PenArmSweepMotorServoOn = value,
            _1PenRotationMotorServoOn = value,
            _2ChuckMotorServoOn = value,
            _2PenArmSweepMotorServoOn = value,
            _2PenRotationMotorServoOn = value
        };
    }

    private static void AssertStation1(PenCollectData snapshot, bool chuck, bool sweep, bool rotation)
    {
        Assert.AreEqual(chuck, snapshot._1ChuckMotorServoOn);
        Assert.AreEqual(sweep, snapshot._1PenArmSweepMotorServoOn);
        Assert.AreEqual(rotation, snapshot._1PenRotationMotorServoOn);
    }

    private static void AssertStation2(PenCollectData snapshot, bool chuck, bool sweep, bool rotation)
    {
        Assert.AreEqual(chuck, snapshot._2ChuckMotorServoOn);
        Assert.AreEqual(sweep, snapshot._2PenArmSweepMotorServoOn);
        Assert.AreEqual(rotation, snapshot._2PenRotationMotorServoOn);
    }
}
