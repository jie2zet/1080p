using CMP.Constants.Plc;

namespace CMP.Models.Operation.Clean;

/// <summary>
/// 通用 PenClean 配置实现 - 支持所有 Pen 类型
/// </summary>
public class GenericPenCleanConfig : IPenCleanConfig
{
    public string PenType { get; }
    public uint PlcHandleName { get; }
    public string PlcHandleTypeName => _plcHandleType.Name;
    public string PlcAddress { get; }
    public ushort PlcPort { get; }
    public PenCleanParams Params { get; }

    private readonly Type _plcHandleType;

    public GenericPenCleanConfig(string penType, uint plcHandleName, string plcAddress = "192.168.2.20", ushort plcPort = 8500)
    {
        PenType = penType;
        PlcHandleName = plcHandleName;
        PlcAddress = plcAddress;
        PlcPort = plcPort;

        // 通过反射获取对应的 PlcHandle 类型
        _plcHandleType = GetPlcHandleType(plcHandleName);
        
        // 初始化参数访问器
        Params = new PenCleanParams(_plcHandleType);
    }

    private static Type GetPlcHandleType(uint plcHandleName)
    {
        var handleTypeName = plcHandleName switch
        {
            Handle.CmdPenA1 => "CmdPenA1",
            Handle.CmdPenA2 => "CmdPenA2",
            Handle.CmdPenB1 => "CmdPenB1",
            Handle.CmdPenB2 => "CmdPenB2",
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

    public (uint handle, byte[] value) GetMotorCommand(string motorCommand)
    {
        var motorType = _plcHandleType.GetNestedType("Motor")!;
        
        return motorCommand switch
        {
            // Chuck Rotation Motor
            "ChuckRotation_Run" => GetCommandValue(motorType.GetNestedType("ChuckRotation")!, "Run"),
            "ChuckRotation_Stop" => GetCommandValue(motorType.GetNestedType("ChuckRotation")!, "Stop"),
            "ChuckRotation_Home" => GetCommandValue(motorType.GetNestedType("ChuckRotation")!, "Home"),
            
            // Pen Arm Sweep Motor
            "PenArmSweep_AnyPosition" => GetCommandValue(motorType.GetNestedType("PenArmSweep")!, "AnyPosition"),
            "PenArmSweep_Stop" => GetCommandValue(motorType.GetNestedType("PenArmSweep")!, "Stop"),
            "PenArmSweep_Home" => GetCommandValue(motorType.GetNestedType("PenArmSweep")!, "Home"),
            "PenArmSweep_Edge1" => GetCommandValue(motorType.GetNestedType("PenArmSweep")!, "EdgePosition1"),
            "PenArmSweep_Edge2" => GetCommandValue(motorType.GetNestedType("PenArmSweep")!, "EdgePosition2"),
            "PenArmSweep_Trim" => GetCommandValue(motorType.GetNestedType("PenArmSweep")!, "TrimPosition"),
            "PenArmSweep_Center" => GetCommandValue(motorType.GetNestedType("PenArmSweep")!, "CenterPosition"),
            
            // Pen Rotation Motor
            "PenRotation_Run" => GetCommandValue(motorType.GetNestedType("PenRotation")!, "Run"),
            "PenRotation_Stop" => GetCommandValue(motorType.GetNestedType("PenRotation")!, "Stop"),
            
            _ => throw new ArgumentException($"Unknown motor command: {motorCommand}")
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

    public (uint handle, ushort value, ushort bitOffset) GetServoCommand(string motor, bool servoOn)
    {
        var motorType = _plcHandleType.GetNestedType("Motor")!;
        var commandName = servoOn ? "ServoOn" : "ServoOff";
        
        return motor switch
        {
            "ChuckRotationMotor" => GetCommandValueWithBitOffset(motorType.GetNestedType("ChuckRotation")!, commandName),
            "PenArmSweepMotor" => GetCommandValueWithBitOffset(motorType.GetNestedType("PenArmSweep")!, commandName),
            "PenRotationMotor" => GetCommandValueWithBitOffset(motorType.GetNestedType("PenRotation")!, commandName),
            _ => throw new ArgumentException($"Unknown motor: {motor}")
        };
    }

    public (uint handle, byte[] value) GetCylinderCommand(string cylinderCommand)
    {
        var cylinderType = _plcHandleType.GetNestedType("Cylinder")!;
        
        return cylinderCommand switch
        {
            "PenUpDownCylinder_Up" => GetCommandValue(cylinderType.GetNestedType("PenUpDownCylinder")!, "Up"),
            "PenUpDownCylinder_Down" => GetCommandValue(cylinderType.GetNestedType("PenUpDownCylinder")!, "Down"),
            "PenArmCylinder_Up" => GetCommandValue(cylinderType.GetNestedType("PenArmCylinder")!, "Up"),
            "PenArmCylinder_Down" => GetCommandValue(cylinderType.GetNestedType("PenArmCylinder")!, "Down"),
            "WFClampCylinder_Clamp" => GetCommandValue(cylinderType.GetNestedType("WFClampCylinder")!, "Clamp"),
            "WFClampCylinder_Release" => GetCommandValue(cylinderType.GetNestedType("WFClampCylinder")!, "Release"),
            "NozzleCylinder_Up" => GetCommandValue(cylinderType.GetNestedType("NozzleCylinder")!, "Up"),
            "NozzleCylinder_Down" => GetCommandValue(cylinderType.GetNestedType("NozzleCylinder")!, "Down"),
            "WaferInletDoor_Open" => GetCommandValue(cylinderType.GetNestedType("WaferInletDoor")!, "Open"),
            "WaferInletDoor_Close" => GetCommandValue(cylinderType.GetNestedType("WaferInletDoor")!, "Close"),
            "WaferOutletDoor_Open" => GetCommandValue(cylinderType.GetNestedType("WaferOutletDoor")!, "Open"),
            "WaferOutletDoor_Close" => GetCommandValue(cylinderType.GetNestedType("WaferOutletDoor")!, "Close"),
            _ => throw new ArgumentException($"Unknown cylinder command: {cylinderCommand}")
        };
    }

    public (uint handle, byte[] value) GetPressureCommand(string pressureCommand)
    {
        var cylinderType = _plcHandleType.GetNestedType("Cylinder")!;
        var gasType = _plcHandleType.GetNestedType("Gas")!;
        var liquidType = _plcHandleType.GetNestedType("Liquid")!;
        
        return pressureCommand switch
        {
            // Pen Pressure
            "PenPressure_Pressure" => GetCommandValue(cylinderType.GetNestedType("PenPressure")!, "Pressure"),
            "PenPressure_Reset" => GetCommandValue(cylinderType.GetNestedType("PenPressure")!, "PressureReset"),
            "PenPressure_Calibration" => GetCommandValue(cylinderType.GetNestedType("PenPressure")!, "EVCalibration"),
            
            // N2 Flow
            "N2Flow_Flowrate" => GetCommandValue(gasType.GetNestedType("N2Flow")!, "Flowrate"),
            "N2Flow_Reset" => GetCommandValue(gasType.GetNestedType("N2Flow")!, "FlowrateReset"),
            "N2Flow_Calibration" => GetCommandValue(gasType.GetNestedType("N2Flow")!, "MFCCalibration"),
            
            // CO2 Pressure
            "CO2Pressure_Pressure" => GetCommandValue(gasType.GetNestedType("CO2Pressure")!, "Pressure"),
            "CO2Pressure_Reset" => GetCommandValue(gasType.GetNestedType("CO2Pressure")!, "PressureReset"),
            "CO2Pressure_Calibration" => GetCommandValue(gasType.GetNestedType("CO2Pressure")!, "EVCalibration"),
            
            // DIW Pressure
            "DIWPressure_Pressure" => GetCommandValue(liquidType.GetNestedType("DIWPressure")!, "Pressure"),
            "DIWPressure_Reset" => GetCommandValue(liquidType.GetNestedType("DIWPressure")!, "PressureReset"),
            "DIWPressure_Calibration" => GetCommandValue(liquidType.GetNestedType("DIWPressure")!, "EVCalibration"),
            
            _ => throw new ArgumentException($"Unknown pressure command: {pressureCommand}")
        };
    }

    public (uint handle, byte[] value) GetLiquidCommand(string liquidCommand)
    {
        var liquidType = _plcHandleType.GetNestedType("Liquid")!;
        
        return liquidCommand switch
        {
            "ChemicalInletValve_Open" => GetCommandValue(liquidType.GetNestedType("ChemicalInletValve")!, "Open"),
            "ChemicalInletValve_Close" => GetCommandValue(liquidType.GetNestedType("ChemicalInletValve")!, "Close"),
            "ChemicalOutletValve_Open" => GetCommandValue(liquidType.GetNestedType("ChemicalOutletValve")!, "Open"),
            "ChemicalOutletValve_Close" => GetCommandValue(liquidType.GetNestedType("ChemicalOutletValve")!, "Close"),
            "DIWInletValve_Open" => GetCommandValue(liquidType.GetNestedType("DIWInletValve")!, "Open"),
            "DIWInletValve_Close" => GetCommandValue(liquidType.GetNestedType("DIWInletValve")!, "Close"),
            "DIWOutletValve_Open" => GetCommandValue(liquidType.GetNestedType("DIWOutletValve")!, "Open"),
            "DIWOutletValve_Close" => GetCommandValue(liquidType.GetNestedType("DIWOutletValve")!, "Close"),
            "ChemicalFlow_Flowrate" => GetCommandValue(liquidType.GetNestedType("ChemicalFlow")!, "Flowrate"),
            "ChemicalFlow_FlowrateReset" => GetCommandValue(liquidType.GetNestedType("ChemicalFlow")!, "FlowrateReset"),
            "ChemicalFlow_FCCalibration" => GetCommandValue(liquidType.GetNestedType("ChemicalFlow")!, "FCCalibration"),
            "DIWFlow_Flowrate" => GetCommandValue(liquidType.GetNestedType("DIWFlow")!, "Flowrate"),
            "DIWFlow_FlowrateReset" => GetCommandValue(liquidType.GetNestedType("DIWFlow")!, "FlowrateReset"),
            "DIWFlow_FCCalibration" => GetCommandValue(liquidType.GetNestedType("DIWFlow")!, "FCCalibration"),
            "WFUpperFaceDIWWashValve_Open" => GetCommandValue(liquidType.GetNestedType("WFUpperFaceDIWWashValve")!, "Open"),
            "WFUpperFaceDIWWashValve_Close" => GetCommandValue(liquidType.GetNestedType("WFUpperFaceDIWWashValve")!, "Close"),
            "WFUpperFaceDIWFlow_Flowrate" => GetCommandValue(liquidType.GetNestedType("WFUpperFaceDIWFlow")!, "Flowrate"),
            "WFUpperFaceDIWFlow_FlowrateReset" => GetCommandValue(liquidType.GetNestedType("WFUpperFaceDIWFlow")!, "FlowrateReset"),
            "WFUpperFaceDIWFlow_FCCalibration" => GetCommandValue(liquidType.GetNestedType("WFUpperFaceDIWFlow")!, "FCCalibration"),
            "WFLowerFaceDIWWashValve_Open" => GetCommandValue(liquidType.GetNestedType("WFLowerFaceDIWWashValve")!, "Open"),
            "WFLowerFaceDIWWashValve_Close" => GetCommandValue(liquidType.GetNestedType("WFLowerFaceDIWWashValve")!, "Close"),
            "WFLowerFaceDIWFlow_Flowrate" => GetCommandValue(liquidType.GetNestedType("WFLowerFaceDIWFlow")!, "Flowrate"),
            "WFLowerFaceDIWFlow_FlowrateReset" => GetCommandValue(liquidType.GetNestedType("WFLowerFaceDIWFlow")!, "FlowrateReset"),
            "WFLowerFaceDIWFlow_FCCalibration" => GetCommandValue(liquidType.GetNestedType("WFLowerFaceDIWFlow")!, "FCCalibration"),
            "DIWSupplyValve_Open" => GetCommandValue(liquidType.GetNestedType("DIWSupplyValve")!, "Open"),
            "DIWSupplyValve_Close" => GetCommandValue(liquidType.GetNestedType("DIWSupplyValve")!, "Close"),
            "DIWPressure_Pressure" => GetCommandValue(liquidType.GetNestedType("DIWPressure")!, "Pressure"),
            "DIWPressure_PressureReset" => GetCommandValue(liquidType.GetNestedType("DIWPressure")!, "PressureReset"),
            "DIWPressure_EVCalibration" => GetCommandValue(liquidType.GetNestedType("DIWPressure")!, "EVCalibration"),
            "ChemicalInletValve2_Open" => GetCommandValue(liquidType.GetNestedType("ChemicalInletValve2")!, "Open"),
            "ChemicalInletValve2_Close" => GetCommandValue(liquidType.GetNestedType("ChemicalInletValve2")!, "Close"),
            "ChemicalInletValve3_Open" => GetCommandValue(liquidType.GetNestedType("ChemicalInletValve3")!, "Open"),
            "ChemicalInletValve3_Close" => GetCommandValue(liquidType.GetNestedType("ChemicalInletValve3")!, "Close"),
            "PenDIWWashValve_Open" => GetCommandValue(liquidType.GetNestedType("PenDIWWashValve")!, "Open"),
            "PenDIWWashValve_Close" => GetCommandValue(liquidType.GetNestedType("PenDIWWashValve")!, "Close"),
            "QuartzDIWWashValve_Open" => GetCommandValue(liquidType.GetNestedType("QuartzDIWWashValve")!, "Open"),
            "QuartzDIWWashValve_Close" => GetCommandValue(liquidType.GetNestedType("QuartzDIWWashValve")!, "Close"),
            "ChemicalPipelineWashValve_Open" => GetCommandValue(liquidType.GetNestedType("ChemicalPipelineWashValve")!, "Open"),
            "ChemicalPipelineWashValve_Close" => GetCommandValue(liquidType.GetNestedType("ChemicalPipelineWashValve")!, "Close"),
            "QuartzChemicalWashValve_Open" => GetCommandValue(liquidType.GetNestedType("QuartzChemicalWashValve")!, "Open"),
            "QuartzChemicalWashValve_Close" => GetCommandValue(liquidType.GetNestedType("QuartzChemicalWashValve")!, "Close"),
            "WFUpperLowerFacesChemicalWashValve_Open" => GetCommandValue(liquidType.GetNestedType("WFUpperLowerFacesChemicalWashValve")!, "Open"),
            "WFUpperLowerFacesChemicalWashValve_Close" => GetCommandValue(liquidType.GetNestedType("WFUpperLowerFacesChemicalWashValve")!, "Close"),
            "WFUpperFacesChemicalWashValve_Open" => GetCommandValue(liquidType.GetNestedType("WFUpperFacesChemicalWashValve")!, "Open"),
            "WFUpperFacesChemicalWashValve_Close" => GetCommandValue(liquidType.GetNestedType("WFUpperFacesChemicalWashValve")!, "Close"),
            "DIWChamberValve_Open" => GetCommandValue(liquidType.GetNestedType("DIWChamberValve")!, "Open"),
            "DIWChamberValve_Close" => GetCommandValue(liquidType.GetNestedType("DIWChamberValve")!, "Close"),
            "WFUpperFaceDICO2WashValve_Open" => GetCommandValue(liquidType.GetNestedType("WFUpperFaceDICO2WashValve")!, "Open"),
            "WFUpperFaceDICO2WashValve_Close" => GetCommandValue(liquidType.GetNestedType("WFUpperFaceDICO2WashValve")!, "Close"),
            "Flush_Open" => GetFlushCommand("Open"),
            "Flush_Close" => GetFlushCommand("Close"),
            _ => throw new ArgumentException($"Unknown liquid command: {liquidCommand}")
        };
    }

    public (uint handle, byte[] value) GetFlushCommand(string flushCommand)
    {
        var flushType = _plcHandleType.GetNestedType("Flush")!;

        return flushCommand switch
        {
            "Open" => GetCommandValue(flushType, "Open"),
            "Close" => GetCommandValue(flushType, "Close"),
            _ => throw new ArgumentException($"Unknown flush command: {flushCommand}")
        };
    }

    public (uint handle, byte[] value) GetGasCommand(string gasCommand)
    {
        var gasType = _plcHandleType.GetNestedType("Gas")!;
        
        return gasCommand switch
        {
            "WFUpperN2OutletValve_Open" => GetCommandValue(gasType.GetNestedType("WFUpperN2OutletValve")!, "Open"),
            "WFUpperN2OutletValve_Close" => GetCommandValue(gasType.GetNestedType("WFUpperN2OutletValve")!, "Close"),
            "CO2SupplyValve_Open" => GetCommandValue(gasType.GetNestedType("CO2SupplyValve")!, "Open"),
            "CO2SupplyValve_Close" => GetCommandValue(gasType.GetNestedType("CO2SupplyValve")!, "Close"),
            "CUPPurgeValves_Open" => GetCommandValue(gasType.GetNestedType("CUPPurgeValves")!, "Open"),
            "CUPPurgeValves_Close" => GetCommandValue(gasType.GetNestedType("CUPPurgeValves")!, "Close"),
            "WFLowerFaceN2OutletValve_Open" => GetCommandValue(gasType.GetNestedType("WFLowerFaceN2OutletValve")!, "Open"),
            "WFLowerFaceN2OutletValve_Close" => GetCommandValue(gasType.GetNestedType("WFLowerFaceN2OutletValve")!, "Close"),
            _ => throw new ArgumentException($"Unknown gas command: {gasCommand}")
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
/// PenClean 配置工厂类
/// </summary>
public static class PenCleanConfigFactory
{
    public static GenericPenCleanConfig CreatePenA1Config(string plcAddress = "192.168.2.30", ushort plcPort = 8500)
        => new("Pen A1", Handle.CmdPenA1, plcAddress, plcPort);

    public static GenericPenCleanConfig CreatePenA2Config(string plcAddress = "192.168.2.20", ushort plcPort = 8500)
        => new("Pen A2", Handle.CmdPenA2, plcAddress, plcPort);

    public static GenericPenCleanConfig CreatePenB1Config(string plcAddress = "192.168.2.32", ushort plcPort = 8500)
        => new("Pen B1", Handle.CmdPenB1, plcAddress, plcPort);

    public static GenericPenCleanConfig CreatePenB2Config(string plcAddress = "192.168.2.20", ushort plcPort = 8500)
        => new("Pen B2", Handle.CmdPenB2, plcAddress, plcPort);
}
