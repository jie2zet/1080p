using System;
using System.Timers;
using CMP.Constants.Plc;
using CMP.Models.kvsPlc;
using CMP.Models.Operation.Clean;
using CMP.Models.Operation.Roller;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CMP.Tests.Constans;

[TestClass]
[TestSubject(typeof(ConstansTest))]
public class ConstansTest
{

    [TestMethod]
    public void GetParameter()
    {
        var name = PlcHandle.CmdPolish.Motor.PlatenRotation.Param.Velocity.ParamName();
        var handle = PlcHandle.CmdPolish.Motor.PlatenRotation.Param.Velocity.ParamHandle();
        var minValue = PlcHandle.CmdPolish.Motor.PlatenRotation.Param.Velocity.MinValue();
        var maxValue = PlcHandle.CmdPolish.Motor.PlatenRotation.Param.Velocity.MaxValue();
        var defaultValue = PlcHandle.CmdPolish.Motor.PlatenRotation.Param.Velocity.DefaultValue();
        var desc = PlcHandle.CmdPolish.Motor.PlatenRotation.Param.Velocity.ParamDescription();
        var label = PlcHandle.CmdPolish.Motor.PlatenRotation.Param.Velocity.ParamLabel();
        var unit = PlcHandle.CmdPolish.Motor.PlatenRotation.Param.Velocity.ParamUnit();
        var testInput = 100;
        var isTestInputInRange = PlcHandle.CmdPolish.Motor.PlatenRotation.Param.Velocity.IsInRange(testInput);
        
        // Plc 
        // KvHostLink.KHLWriteDevicesAsWords(0, KHLDevType.DEV_ZF, handle, 1, 
        //     isTestInputInRange ? testInput.Value() : 
        //         throw new ArgumentOutOfRangeException(nameof(testInput), $@"Value {testInput} is out of range [{minValue}, {maxValue}]"));
        
        Console.WriteLine($@"Name: {name}");
        Console.WriteLine($@"Handle: {handle}");
        Console.WriteLine($@"Min Value: {minValue}");
        Console.WriteLine($@"Max Value: {maxValue}");
        Console.WriteLine($@"Default Value: {defaultValue}");
        Console.WriteLine($@"Description: {desc}");
        Console.WriteLine($@"Label: {label}");
        Console.WriteLine($@"Unit: {unit}");
        Console.WriteLine($@"Is {testInput} in range: {isTestInputInRange}");
    }
    
    [TestMethod]
    public void GetParameters()
    {
        var allParams = PlcHandleHelper.GetAllParamsWithNames();
        
        Console.WriteLine($@"Total Parameters: {allParams.Count}");
        Console.WriteLine(new string('=', 100));
        
        foreach (var param in allParams)
        {
            var handle = param.Key;
            var name = param.Value.Name;
            var attr = param.Value.Attr;
            
            Console.WriteLine($@"Name: {name}");
            Console.WriteLine($@"  Handle: {handle}");
            Console.WriteLine($@"  Default Value: {attr.DefaultValue}");
            Console.WriteLine($@"  Min Value: {attr.MinValue}");
            Console.WriteLine($@"  Max Value: {attr.MaxValue}");
            Console.WriteLine($@"  Description: {attr.Description}");
            Console.WriteLine($@"  Label: {attr.Label}");
            Console.WriteLine($@"  Unit: {attr.Unit}");
            Console.WriteLine(new string('-', 100));
        }
    }

    [TestMethod]
    public void PolishServoCommands()
    {
        var servoOn = PlcHandle.CmdPolish.Motor.ConditionerSweep.ServoOn;
        var servoOff = PlcHandle.CmdPolish.Motor.ConditionerSweep.ServoOff;

        Assert.AreEqual(530, servoOn);
        Assert.AreEqual(530, servoOff);
        Assert.AreEqual(0x530u, servoOn.HexWAddress());
        Assert.AreEqual((ushort)5, PlcHandle.CmdPolish.Motor.ConditionerSweep.ServoOn.BitOffset());
        
        Console.WriteLine($@"Servo On: {servoOn}");
        Console.WriteLine($@"Servo Off: {servoOff}");
    }

    [TestMethod]
    public void DryInterfaceGetServoCommandTest()
    {
        var dry = new GenericDryCleanConfig("Dry B", Handle.CmdDryB);
        var (handle, value, offset) = dry.GetServoCommand("ChuckRotationMotor", true);

        Assert.AreEqual(539, value);
        Assert.AreEqual(0x539u, value.HexWAddress());
        Assert.AreEqual(8, offset);

        Console.WriteLine($@"Handle: {handle}");
        Console.WriteLine($@"Value: {value}");
        Console.WriteLine($@"Offset: {offset}");
    }
    
    [TestMethod]
    public void PenInterfaceGetServoCommandTest()
    {
        var pen = new GenericPenCleanConfig("Pen A", Handle.CmdPenA1);
        var (handle, value, offset) = pen.GetServoCommand("PenArmSweepMotor", true);
        Assert.AreEqual(537, value);
        Assert.AreEqual(0x537u, value.HexWAddress());
        Assert.AreEqual(1, offset);
        Console.WriteLine($@"Handle: {handle}");
        Console.WriteLine($@"Value: {value}");
        Console.WriteLine($@"Offset: {offset}");
    }
    
    [TestMethod]
    public void RollerInterfaceGetServoCommandTest()
    {
        var roller = new GenericRollConfig("Roller C", Handle.CmdRollB1);
        var (handle, value, offset) = roller.GetServoCommand("WfRightPillarRotateMotor", true);
        Assert.AreEqual(535, value);
        Assert.AreEqual(0x535u, value.HexWAddress());
        Assert.AreEqual(9, offset);
        Console.WriteLine($@"Handle: {handle}");
        Console.WriteLine($@"Value: {value}");
        Console.WriteLine($@"Offset: {offset}");
    }
}
