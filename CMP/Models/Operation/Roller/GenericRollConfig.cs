using CMP.Constants.Plc;

namespace CMP.Models.Operation.Roller;

/// <summary>
/// 通用 Roll 配置实现 - 支持所有 Roller 类型
/// </summary>
public class GenericRollConfig : IRollerConfig
{
    public string RollType { get; }
    public uint PlcHandleName { get; }
    public string PlcAddress { get; }
    public ushort PlcPort { get; }
    public RollerParams Params { get; }

    private readonly Type _plcHandleType;

    public GenericRollConfig(string rollType, uint plcHandleName, string plcAddress = "192.168.2.20", ushort plcPort = 8500)
    {
        RollType = rollType;
        PlcHandleName = plcHandleName;
        PlcAddress = plcAddress;
        PlcPort = plcPort;

        // 通过反射获取对应的 PlcHandle 类型
        _plcHandleType = GetPlcHandleType(plcHandleName);
        
        // 初始化参数访问器
        Params = new RollerParams(_plcHandleType);
    }

    private static Type GetPlcHandleType(uint plcHandleName)
    {
        var handleTypeName = plcHandleName switch
        {
            Handle.CmdRollA1 => "CmdRollA1",
            Handle.CmdRollA2 => "CmdRollA2", 
            Handle.CmdRollA3 => "CmdRollA3",
            Handle.CmdRollB1 => "CmdRollB1",
            Handle.CmdRollB2 => "CmdRollB2",
            Handle.CmdRollB3 => "CmdRollB3",
            _ => throw new ArgumentException($"Unknown PLC handle name: {plcHandleName}")
        };

        return typeof(PlcHandle).GetNestedType(handleTypeName) 
            ?? throw new InvalidOperationException($"PlcHandle type not found: {handleTypeName}");
    }

    public (uint handle, byte[] value) GetInitializeCommand()
    {
        var allType = _plcHandleType.GetNestedType("All")!;
        var initializeField = allType.GetField("Initialize")!;
        var initializeValue = (int)initializeField.GetValue(null)!;
        
        return (PlcHandleName, initializeValue.Value());
    }

    public (uint handle, byte[] value) GetAutoModeCommand()
    {
        var modeType = _plcHandleType.GetNestedType("All")!.GetNestedType("Mode")!;
        var autoField = modeType.GetField("Auto")!;
        var autoValue = (int)autoField.GetValue(null)!;
        
        return (PlcHandleName, autoValue.Value());
    }

    public (uint handle, byte[] value) GetManualModeCommand()
    {
        var modeType = _plcHandleType.GetNestedType("All")!.GetNestedType("Mode")!;
        var manualField = modeType.GetField("Manual")!;
        var manualValue = (int)manualField.GetValue(null)!;
        
        return (PlcHandleName, manualValue.Value());
    }

    public (uint handle, byte[] value) GetTestRecipeCommand(string command)
    {
        var testRecipeType = _plcHandleType.GetNestedType("All")!.GetNestedType("TestRecipe")!;
        
        return command switch
        {
            "Test" => GetCommandValue(testRecipeType, "Test"),
            "Start" => GetCommandValue(testRecipeType, "Start"),
            "Stop" => GetCommandValue(testRecipeType, "Stop"),
            _ => throw new ArgumentException($"Unknown test recipe command: {command}")
        };
    }

    public (uint handle, byte[] value) GetMotorRunCommand(string motor)
    {
        var motorType = _plcHandleType.GetNestedType("Motor")!;
        
        return motor switch
        {
            "WfLeftPillarRotateMotor" => GetCommandValue(motorType.GetNestedType("WfLeftPillarRotate")!, "Run"),
            "WfRightPillarRotateMotor" => GetCommandValue(motorType.GetNestedType("WfRightPillarRotate")!, "Run"),
            "UpperRollerRotateMotor" => GetCommandValue(motorType.GetNestedType("UpperRollerRotate")!, "Run"),
            "LowerRollerRotateMotor" => GetCommandValue(motorType.GetNestedType("LowerRollerRotate")!, "Run"),
            _ => throw new ArgumentException($"Unknown motor: {motor}")
        };
    }

    public (uint handle, byte[] value) GetMotorStopCommand(string motor)
    {
        var motorType = _plcHandleType.GetNestedType("Motor")!;
        
        return motor switch
        {
            "WfLeftPillarRotateMotor" => GetCommandValue(motorType.GetNestedType("WfLeftPillarRotate")!, "Stop"),
            "WfRightPillarRotateMotor" => GetCommandValue(motorType.GetNestedType("WfRightPillarRotate")!, "Stop"),
            "UpperRollerRotateMotor" => GetCommandValue(motorType.GetNestedType("UpperRollerRotate")!, "Stop"),
            "LowerRollerRotateMotor" => GetCommandValue(motorType.GetNestedType("LowerRollerRotate")!, "Stop"),
            _ => throw new ArgumentException($"Unknown motor: {motor}")
        };
    }

    private (uint handle, ushort value, ushort bitOffset) GetCommandValueWithBitOffset(Type type, string fieldName)
    {
        var field = type.GetField(fieldName) ?? throw new InvalidOperationException($"Field '{fieldName}' not found in type '{type.Name}'");
        var value = (int)field.GetValue(null)!;
        var path = $"{type.FullName!.Replace("+", ".")}.{fieldName}";
        path = $"PlcHandle.{path.Split("PlcHandle.").Last()}";
        return (PlcHandleName, (ushort)value, PlcHandleHelper.GetHandleInfoByPath(path)!.BitOffset);
    }
    
    public (uint handle, ushort value, ushort bitOffset) GetServoCommand(string motor)
    {
        var motorType = _plcHandleType.GetNestedType("Motor")!;
        var commandName = "Servo";
        
        return motor switch
        {
            "WfLeftPillarRotateMotor" => GetCommandValueWithBitOffset(motorType.GetNestedType("WfLeftPillarRotate")!, commandName),
            "WfRightPillarRotateMotor" => GetCommandValueWithBitOffset(motorType.GetNestedType("WfRightPillarRotate")!, commandName),
            "UpperRollerRotateMotor" => GetCommandValueWithBitOffset(motorType.GetNestedType("UpperRollerRotate")!, commandName),
            "LowerRollerRotateMotor" => GetCommandValueWithBitOffset(motorType.GetNestedType("LowerRollerRotate")!, commandName),
            _ => throw new ArgumentException($"Unknown motor: {motor}")
        };
    }

    public (uint handle, byte[] value) GetCylinderCommand(string cylinderCommand)
    {
        var cylinderType = _plcHandleType.GetNestedType("Cylinder")!;
        
        return cylinderCommand switch
        {
            "LowerRollerCylinder_Up" => GetCommandValue(cylinderType.GetNestedType("LowerRollerCylinder")!, "Up"),
            "LowerRollerCylinder_Down" => GetCommandValue(cylinderType.GetNestedType("LowerRollerCylinder")!, "Down"),
            "UpperRollerCylinder_Up" => GetCommandValue(cylinderType.GetNestedType("UpperRollerCylinder")!, "Up"),
            "UpperRollerCylinder_Down" => GetCommandValue(cylinderType.GetNestedType("UpperRollerCylinder")!, "Down"),
            "ArmUpDown_Up" => GetCommandValue(cylinderType.GetNestedType("ArmUpDown")!, "Up"),
            "ArmUpDown_Down" => GetCommandValue(cylinderType.GetNestedType("ArmUpDown")!, "Down"),
            "ArmLateralMovement_Waiting" => GetCommandValue(cylinderType.GetNestedType("ArmLateralMovement")!, "Waiting"),
            "ArmLateralMovement_Cleaning" => GetCommandValue(cylinderType.GetNestedType("ArmLateralMovement")!, "Cleaning"),
            "ClampCylinder_Release" => GetCommandValue(cylinderType.GetNestedType("ClampCylinder")!, "Release"),
            "ClampCylinder_Clamp" => GetCommandValue(cylinderType.GetNestedType("ClampCylinder")!, "Clamp"),
            "UltrasonicNozzle_Up" => GetCommandValue(cylinderType.GetNestedType("UltrasonicNozzle")!, "Up"),
            "UltrasonicNozzle_Down" => GetCommandValue(cylinderType.GetNestedType("UltrasonicNozzle")!, "Down"),
            "WaferInletDoor_Open" => GetCommandValue(cylinderType.GetNestedType("WaferInletDoor")!, "Open"),
            "WaferInletDoor_Close" => GetCommandValue(cylinderType.GetNestedType("WaferInletDoor")!, "Close"),
            "ArmDoor_Open" => GetCommandValue(cylinderType.GetNestedType("ArmDoor")!, "Open"),
            "ArmDoor_Close" => GetCommandValue(cylinderType.GetNestedType("ArmDoor")!, "Close"),
            "WaferOutletDoor_Open" => GetCommandValue(cylinderType.GetNestedType("WaferOutletDoor")!, "Open"),
            "WaferOutletDoor_Close" => GetCommandValue(cylinderType.GetNestedType("WaferOutletDoor")!, "Close"),
            _ => throw new ArgumentException($"Unknown cylinder command: {cylinderCommand}")
        };
    }

    public (uint handle, byte[] value) GetPressureCommand(string pressureCommand)
    {
        var cylinderType = _plcHandleType.GetNestedType("Cylinder")!;
        
        return pressureCommand switch
        {
            "LowerRollerPressure_Pressure" => GetCommandValue(cylinderType.GetNestedType("LowerRollerPressure")!, "Pressure"),
            "LowerRollerPressure_Reset" => GetCommandValue(cylinderType.GetNestedType("LowerRollerPressure")!, "PressureReset"),
            "LowerRollerPressure_Calibration" => GetCommandValue(cylinderType.GetNestedType("LowerRollerPressure")!, "PressureCalibration"),
            "UpperRollerPressure_Pressure" => GetCommandValue(cylinderType.GetNestedType("UpperRollerPressure")!, "Pressure"),
            "UpperRollerPressure_Reset" => GetCommandValue(cylinderType.GetNestedType("UpperRollerPressure")!, "PressureReset"),
            "UpperRollerPressure_Calibration" => GetCommandValue(cylinderType.GetNestedType("UpperRollerPressure")!, "PressureCalibration"),
            "ClampPressure_Pressure" => GetCommandValue(cylinderType.GetNestedType("ClampPressure")!, "Pressure"),
            "ClampPressure_Reset" => GetCommandValue(cylinderType.GetNestedType("ClampPressure")!, "PressureReset"),
            "ClampPressure_Calibration" => GetCommandValue(cylinderType.GetNestedType("ClampPressure")!, "PressureCalibration"),
            _ => throw new ArgumentException($"Unknown pressure command: {pressureCommand}")
        };
    }

    public (uint handle, byte[] value) GetLiquidCommand(string liquidCommand)
    {
        var liquidType = _plcHandleType.GetNestedType("Liquid")!;
        
        return liquidCommand switch
        {
            "ChemicalEnterMainValve_Open" => GetCommandValue(liquidType.GetNestedType("ChemicalEnterMainValve")!, "Open"),
            "ChemicalEnterMainValve_Close" => GetCommandValue(liquidType.GetNestedType("ChemicalEnterMainValve")!, "Close"),
            "DiwEnterMainValve_Open" => GetCommandValue(liquidType.GetNestedType("DiwEnterMainValve")!, "Open"),
            "DiwEnterMainValve_Close" => GetCommandValue(liquidType.GetNestedType("DiwEnterMainValve")!, "Close"),
            "CoolingWaterCirculation_Open" => GetCommandValue(liquidType.GetNestedType("CoolingWaterCirculation")!, "Open"),
            "CoolingWaterCirculation_Close" => GetCommandValue(liquidType.GetNestedType("CoolingWaterCirculation")!, "Close"),
            _ => throw new ArgumentException($"Unknown liquid command: {liquidCommand}")
        };
    }

    public (uint handle, byte[] value) GetFlashCommand(string flashCommand)
    {
        var flashType = _plcHandleType.GetNestedType("Flash")!;

        return flashCommand switch
        {
            "Open" => GetCommandValue(flashType, "Open"),
            "Close" => GetCommandValue(flashType, "Close"),
            _ => throw new ArgumentException($"Unknown flash command: {flashCommand}")
        };
    }

    private (uint handle, byte[] value) GetCommandValue(Type type, string fieldName)
    {
        var field = type.GetField(fieldName) ?? throw new InvalidOperationException($"Field '{fieldName}' not found in type '{type.Name}'");
        var value = (int)field.GetValue(null)!;
        return (PlcHandleName, value.Value());
    }
}

/// <summary>
/// Roller 配置工厂类
/// </summary>
public static class RollerConfigFactory
{
    public static GenericRollConfig CreateRollA1Config(string plcAddress = "192.168.2.20", ushort plcPort = 8500)
        => new("Roll A1", Handle.CmdRollA1, plcAddress, plcPort);

    public static GenericRollConfig CreateRollA2Config(string plcAddress = "192.168.2.20", ushort plcPort = 8500)
        => new("Roll A2", Handle.CmdRollA2, plcAddress, plcPort);

    public static GenericRollConfig CreateRollA3Config(string plcAddress = "192.168.2.20", ushort plcPort = 8500)
        => new("Roll A3", Handle.CmdRollA3, plcAddress, plcPort);

    public static GenericRollConfig CreateRollB1Config(string plcAddress = "192.168.2.20", ushort plcPort = 8500)
        => new("Roll B1", Handle.CmdRollB1, plcAddress, plcPort);

    public static GenericRollConfig CreateRollB2Config(string plcAddress = "192.168.2.20", ushort plcPort = 8500)
        => new("Roll B2", Handle.CmdRollB2, plcAddress, plcPort);

    public static GenericRollConfig CreateRollB3Config(string plcAddress = "192.168.2.20", ushort plcPort = 8500)
        => new("Roll B3", Handle.CmdRollB3, plcAddress, plcPort);
}
