#nullable enable

using CMP.Models.kvsPlc;
using CMP.Models.Resource;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RollerCollectData = CMP.Models.kvsPlc.Roll.KvsStructRollerCollectData.stRollerCollectData;

namespace CMP.Tests.Services;

[TestClass]
public class RollerProcessDataSnapshotTests
{
    [TestMethod]
    public void Snapshot_IsUnavailableUntilProcessDataIsPublished()
    {
        var roller = CreateRoller();

        Assert.IsFalse(roller.TryGetProcessDataSnapshot(out _));
    }

    [TestMethod]
    public void Snapshot_CopiesTheCompleteProcessData_AndIsDetachedFromTheCache()
    {
        var roller = CreateRoller();
        var processData = new RollerCollectData
        {
            _1ProcessStepAct = 17,
            _2WFLeftPillarRotationMotorSpeedAct = 12.5f,
            _3DIWAllOpenTimeAct = 8.25f
        };

        roller.ApplyProcessData(processData);

        Assert.IsTrue(roller.TryGetProcessDataSnapshot(out var firstSnapshot));
        Assert.AreNotSame(processData, firstSnapshot);
        Assert.AreEqual((short)17, firstSnapshot._1ProcessStepAct);
        Assert.AreEqual(12.5f, firstSnapshot._2WFLeftPillarRotationMotorSpeedAct);
        Assert.AreEqual(8.25f, firstSnapshot._3DIWAllOpenTimeAct);

        firstSnapshot._2WFLeftPillarRotationMotorSpeedAct = 999f;
        firstSnapshot._3DIWAllOpenTimeAct = 0f;

        Assert.IsTrue(roller.TryGetProcessDataSnapshot(out var secondSnapshot));
        Assert.AreEqual(12.5f, secondSnapshot._2WFLeftPillarRotationMotorSpeedAct);
        Assert.AreEqual(8.25f, secondSnapshot._3DIWAllOpenTimeAct);
        Assert.AreEqual(
            12.5f,
            roller.GetSv(roller.BuildRuntimeKey(null, nameof(RollerCollectData._2WFLeftPillarRotationMotorSpeedAct)))?.Value);
    }

    [TestMethod]
    public void Snapshot_UsesTheLatestDeviceBits_ForEachStation()
    {
        var roller = CreateRoller();
        roller.ApplyProcessData(CreateProcessDataWithServoValues(false));

        roller.ApplyMotorServoStatus([true, false, true, false], "_1");
        roller.ApplyMotorServoStatus([false, true, false, true], "_2");
        roller.ApplyMotorServoStatus([true, true, false, false], "_3");

        Assert.IsTrue(roller.TryGetProcessDataSnapshot(out var snapshot));
        AssertStation1(snapshot, true, false, true, false);
        AssertStation2(snapshot, false, true, false, true);
        AssertStation3(snapshot, true, true, false, false);

        var newerProcessData = CreateProcessDataWithServoValues(false);
        newerProcessData._3LowerRollerRotationSpeedAct = 88f;
        roller.ApplyProcessData(newerProcessData);

        Assert.IsTrue(roller.TryGetProcessDataSnapshot(out var mergedSnapshot));
        Assert.AreEqual(88f, mergedSnapshot._3LowerRollerRotationSpeedAct);
        AssertStation1(mergedSnapshot, true, false, true, false);
        AssertStation2(mergedSnapshot, false, true, false, true);
        AssertStation3(mergedSnapshot, true, true, false, false);
    }

    [TestMethod]
    public void Snapshot_MergesDeviceBitsThatArriveBeforeProcessData_PerStation()
    {
        var roller = CreateRoller();
        roller.ApplyMotorServoStatus([false, true, false, true], "_2");

        Assert.IsFalse(roller.TryGetProcessDataSnapshot(out _));

        roller.ApplyProcessData(CreateProcessDataWithServoValues(true));

        Assert.IsTrue(roller.TryGetProcessDataSnapshot(out var snapshot));
        AssertStation1(snapshot, true, true, true, true);
        AssertStation2(snapshot, false, true, false, true);
        AssertStation3(snapshot, true, true, true, true);
    }

    private static Roller CreateRoller()
    {
        return new Roller("RollerA", new KVLink("127.0.0.1"));
    }

    private static RollerCollectData CreateProcessDataWithServoValues(bool value)
    {
        return new RollerCollectData
        {
            _1WFLeftPillarMotorServoOn = value,
            _1WFRightPillarMotorServoOn = value,
            _1UpperRollerMotorServoOn = value,
            _1LowerRollerMotorServoOn = value,
            _2WFLeftPillarMotorServoOn = value,
            _2WFRightPillarMotorServoOn = value,
            _2UpperRollerMotorServoOn = value,
            _2LowerRollerMotorServoOn = value,
            _3WFLeftPillarMotorServoOn = value,
            _3WFRightPillarMotorServoOn = value,
            _3UpperRollerMotorServoOn = value,
            _3LowerRollerMotorServoOn = value
        };
    }

    private static void AssertStation1(
        RollerCollectData snapshot,
        bool left,
        bool right,
        bool upper,
        bool lower)
    {
        Assert.AreEqual(left, snapshot._1WFLeftPillarMotorServoOn);
        Assert.AreEqual(right, snapshot._1WFRightPillarMotorServoOn);
        Assert.AreEqual(upper, snapshot._1UpperRollerMotorServoOn);
        Assert.AreEqual(lower, snapshot._1LowerRollerMotorServoOn);
    }

    private static void AssertStation2(
        RollerCollectData snapshot,
        bool left,
        bool right,
        bool upper,
        bool lower)
    {
        Assert.AreEqual(left, snapshot._2WFLeftPillarMotorServoOn);
        Assert.AreEqual(right, snapshot._2WFRightPillarMotorServoOn);
        Assert.AreEqual(upper, snapshot._2UpperRollerMotorServoOn);
        Assert.AreEqual(lower, snapshot._2LowerRollerMotorServoOn);
    }

    private static void AssertStation3(
        RollerCollectData snapshot,
        bool left,
        bool right,
        bool upper,
        bool lower)
    {
        Assert.AreEqual(left, snapshot._3WFLeftPillarMotorServoOn);
        Assert.AreEqual(right, snapshot._3WFRightPillarMotorServoOn);
        Assert.AreEqual(upper, snapshot._3UpperRollerMotorServoOn);
        Assert.AreEqual(lower, snapshot._3LowerRollerMotorServoOn);
    }
}
