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
    public void PolishGetBitOffset()
    {
        var handle = PlcHandle.CmdPolish.Motor.ConditionerSweep.Servo;
        var bitOffset = PlcHandle.CmdPolish.Motor.ConditionerSweep.Servo.BitOffset();
        var value = true.Value();
        
        Console.WriteLine($@"Handle: {handle}");
        Console.WriteLine($@"Bit Offset: {bitOffset}");
        Console.WriteLine($@"Value: {value}");
        Console.WriteLine(0xA);
    }

    [TestMethod]
    public void DryInterfaceGetBitOffsetTest()
    {
        var dry = new GenericDryCleanConfig("Dry B", Handle.CmdDryB);
        var (handle, value, offset) = dry.GetServoCommand("ChuckRotationMotor");
        Console.WriteLine($@"Handle: {handle}");
        Console.WriteLine($@"Value: {value}");
        Console.WriteLine($@"Offset: {offset}");
    }
    
    [TestMethod]
    public void PenInterfaceGetBitOffsetTest()
    {
        var pen = new GenericPenCleanConfig("Pen A", Handle.CmdPenA1);
        var (handle, value, offset) = pen.GetServoCommand("PenArmSweepMotor");
        Console.WriteLine($@"Handle: {handle}");
        Console.WriteLine($@"Value: {value}");
        Console.WriteLine($@"Offset: {offset}");
    }
    
    [TestMethod]
    public void RollerInterfaceGetBitOffsetTest()
    {
        var roller = new GenericRollConfig("Roller C", Handle.CmdRollB1);
        var (handle, value, offset) = roller.GetServoCommand("WfRightPillarRotateMotor");
        Console.WriteLine($@"Handle: {handle}");
        Console.WriteLine($@"Value: {value}");
        Console.WriteLine($@"Offset: {offset}");
    }
}