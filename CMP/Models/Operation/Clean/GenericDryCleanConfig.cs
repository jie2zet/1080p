using CMP.Constants.Plc;

namespace CMP.Models.Operation.Clean;

/// <summary>
/// 通用 DryClean 配置实现 - 支持所有 Dry 类型
/// </summary>
public class GenericDryCleanConfig : IDryCleanConfig
{
    public string DryType { get; }
    public uint PlcHandleName { get; }
    public string PlcHandleTypeName => _plcHandleType.Name;
    public string PlcAddress { get; }
    public ushort PlcPort { get; }
    public DryParams Params { get; }

    private readonly Type _plcHandleType;

    public GenericDryCleanConfig(string dryType, uint plcHandleName, string plcAddress = "192.168.2.20", ushort plcPort = 8500)
    {
        DryType = dryType;
        PlcHandleName = plcHandleName;
        PlcAddress = plcAddress;
        PlcPort = plcPort;

        // 通过反射获取对应的 PlcHandle 类型
        _plcHandleType = GetPlcHandleType(plcHandleName);
        Params = new DryParams(_plcHandleType);
    }

    private static Type GetPlcHandleType(uint plcHandleName)
    {
        var handleTypeName = plcHandleName switch
        {
            Handle.CmdDryA => "CmdDryA",
            Handle.CmdDryB => "CmdDryB",
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

            // Arm Swing Motor
            "ArmSwing_AnyPosition" => GetCommandValue(motorType.GetNestedType("ArmSwing")!, "AnyPosition"),
            "ArmSwing_Stop" => GetCommandValue(motorType.GetNestedType("ArmSwing")!, "Stop"),
            "ArmSwing_Home" => GetCommandValue(motorType.GetNestedType("ArmSwing")!, "Home"),
            "ArmSwing_Center" => GetCommandValue(motorType.GetNestedType("ArmSwing")!, "CenterPosition"),
            "ArmSwing_Edge1" => GetCommandValue(motorType.GetNestedType("ArmSwing")!, "EdgePosition1"),
            "ArmSwing_Edge2" => GetCommandValue(motorType.GetNestedType("ArmSwing")!, "EdgePosition2"),

            _ => throw new ArgumentException($"Unknown motor command: {motorCommand}")
        };
    }

    public (uint handle, ushort value, ushort bitOffset) GetServoCommand(string motor)
    {
        var motorType = _plcHandleType.GetNestedType("Motor")!;
        var commandName = "Servo";

        return motor switch
        {
            "ChuckRotationMotor" => GetCommandValueWithBitOffset(motorType.GetNestedType("ChuckRotation")!, commandName),
            "ArmSwingMotor" => GetCommandValueWithBitOffset(motorType.GetNestedType("ArmSwing")!, commandName),
            _ => throw new ArgumentException($"Unknown motor: {motor}")
        };
    }

    public (uint handle, byte[] value) GetCylinderCommand(string cylinderCommand)
    {
        var cylinderType = _plcHandleType.GetNestedType("Cylinder")!;

        return cylinderCommand switch
        {
            "WFClampCylinder_Clamp" => GetCommandValue(cylinderType.GetNestedType("WFClampCylinder")!, "Clamp"),
            "WFClampCylinder_Release" => GetCommandValue(cylinderType.GetNestedType("WFClampCylinder")!, "Release"),
            "WaferInletDoor_Open" => GetCommandValue(cylinderType.GetNestedType("WaferInletDoor")!, "Open"),
            "WaferInletDoor_Close" => GetCommandValue(cylinderType.GetNestedType("WaferInletDoor")!, "Close"),
            "WaferOutletDoor_Open" => GetCommandValue(cylinderType.GetNestedType("WaferOutletDoor")!, "Open"),
            "WaferOutletDoor_Close" => GetCommandValue(cylinderType.GetNestedType("WaferOutletDoor")!, "Close"),
            _ => throw new ArgumentException($"Unknown cylinder command: {cylinderCommand}")
        };
    }

    public (uint handle, byte[] value) GetGasCommand(string gasCommand)
    {
        var gasType = _plcHandleType.GetNestedType("Gas")!;

        return gasCommand switch
        {
            // N2 Mix Outlet Valve
            "N2MixOutletValve_Open" => GetCommandValue(gasType.GetNestedType("N2MixOutletValve")!, "Open"),
            "N2MixOutletValve_Close" => GetCommandValue(gasType.GetNestedType("N2MixOutletValve")!, "Close"),

            // N2 Flow
            "N2Flow_Flowrate" => GetCommandValue(gasType.GetNestedType("N2Flow")!, "Flowrate"),
            "N2Flow_Reset" => GetCommandValue(gasType.GetNestedType("N2Flow")!, "FlowrateReset"),
            "N2Flow_Calibration" => GetCommandValue(gasType.GetNestedType("N2Flow")!, "FCCalibration"),

            // WF Upper Face N2 Outlet Valve
            "WFUpperFaceN2OutletValve_Open" => GetCommandValue(gasType.GetNestedType("WFUpperFaceN2OutletValve")!, "Open"),
            "WFUpperFaceN2OutletValve_Close" => GetCommandValue(gasType.GetNestedType("WFUpperFaceN2OutletValve")!, "Close"),

            // N2 Flow Wafer
            "N2FlowWafer_Flowrate" => GetCommandValue(gasType.GetNestedType("N2FlowWafer")!, "Flowrate"),
            "N2FlowWafer_Reset" => GetCommandValue(gasType.GetNestedType("N2FlowWafer")!, "FlowrateReset"),
            "N2FlowWafer_Calibration" => GetCommandValue(gasType.GetNestedType("N2FlowWafer")!, "FCCalibration"),

            // WF Lower Face N2 Outlet Valve 1
            "WFLowerFaceN2OutletValve1_Open" => GetCommandValue(gasType.GetNestedType("WFLowerFaceN2OutletValve1")!, "Open"),
            "WFLowerFaceN2OutletValve1_Close" => GetCommandValue(gasType.GetNestedType("WFLowerFaceN2OutletValve1")!, "Close"),

            // N2 Flow Lower Face
            "N2FlowLowerFace_Flowrate" => GetCommandValue(gasType.GetNestedType("N2FlowLowerFace")!, "Flowrate"),
            "N2FlowLowerFace_Reset" => GetCommandValue(gasType.GetNestedType("N2FlowLowerFace")!, "FlowrateReset"),
            "N2FlowLowerFace_Calibration" => GetCommandValue(gasType.GetNestedType("N2FlowLowerFace")!, "MFCCalibration"),

            // N2 Release Valves
            "N2ReleaseValves_Open" => GetCommandValue(gasType.GetNestedType("N2ReleaseValves")!, "Open"),
            "N2ReleaseValves_Close" => GetCommandValue(gasType.GetNestedType("N2ReleaseValves")!, "Close"),

            // WF Upper Face IPA Outlet Valve
            "WFUpperFaceIPAOutletValve_Open" => GetCommandValue(gasType.GetNestedType("WFUpperFaceIPAOutletValve")!, "Open"),
            "WFUpperFaceIPAOutletValve_Close" => GetCommandValue(gasType.GetNestedType("WFUpperFaceIPAOutletValve")!, "Close"),

            // WF Lower Face N2 Outlet Valve 2
            "WFLowerFaceN2OutletValve2_Open" => GetCommandValue(gasType.GetNestedType("WFLowerFaceN2OutletValve2")!, "Open"),
            "WFLowerFaceN2OutletValve2_Close" => GetCommandValue(gasType.GetNestedType("WFLowerFaceN2OutletValve2")!, "Close"),

            _ => throw new ArgumentException($"Unknown gas command: {gasCommand}")
        };
    }

    public (uint handle, byte[] value) GetLiquidCommand(string liquidCommand)
    {
        var liquidType = _plcHandleType.GetNestedType("Liquid")!;

        return liquidCommand switch
        {
            // WF Upper Face DIW Wash Inlet Valve 2
            "WFUpperFaceDIWWashInletValve2_Open" => GetCommandValue(liquidType.GetNestedType("WFUpperFaceDIWWashInletValve2")!, "Open"),
            "WFUpperFaceDIWWashInletValve2_Close" => GetCommandValue(liquidType.GetNestedType("WFUpperFaceDIWWashInletValve2")!, "Close"),

            // WF Upper Face DIW Wash Outlet Valve 2
            "WFUpperFaceDIWWashOutletValve2_Open" => GetCommandValue(liquidType.GetNestedType("WFUpperFaceDIWWashOutletValve2")!, "Open"),
            "WFUpperFaceDIWWashOutletValve2_Close" => GetCommandValue(liquidType.GetNestedType("WFUpperFaceDIWWashOutletValve2")!, "Close"),

            // WF Upper Face DIW Wash Flow 2
            "WFUpperFaceDIWWashFlow2_Flowrate" => GetCommandValue(liquidType.GetNestedType("WFUpperFaceDIWWashFlow2")!, "Flowrate"),
            "WFUpperFaceDIWWashFlow2_Reset" => GetCommandValue(liquidType.GetNestedType("WFUpperFaceDIWWashFlow2")!, "FlowrateReset"),
            "WFUpperFaceDIWWashFlow2_Calibration" => GetCommandValue(liquidType.GetNestedType("WFUpperFaceDIWWashFlow2")!, "FCCalibration"),

            // IPA Supply Valve
            "IPASupplyValve_Open" => GetCommandValue(liquidType.GetNestedType("IPASupplyValve")!, "Open"),
            "IPASupplyValve_Close" => GetCommandValue(liquidType.GetNestedType("IPASupplyValve")!, "Close"),

            // WF Upper Face DIW Wash Inlet Valve 1
            "WFUpperFaceDIWWashInletValve1_Open" => GetCommandValue(liquidType.GetNestedType("WFUpperFaceDIWWashInletValve1")!, "Open"),
            "WFUpperFaceDIWWashInletValve1_Close" => GetCommandValue(liquidType.GetNestedType("WFUpperFaceDIWWashInletValve1")!, "Close"),

            // WF Upper Face DIW Wash Outlet Valve 1
            "WFUpperFaceDIWWashOutletValve1_Open" => GetCommandValue(liquidType.GetNestedType("WFUpperFaceDIWWashOutletValve1")!, "Open"),
            "WFUpperFaceDIWWashOutletValve1_Close" => GetCommandValue(liquidType.GetNestedType("WFUpperFaceDIWWashOutletValve1")!, "Close"),

            // WF Lower Face DIW Wash Valve
            "WFLowerFaceDIWWashValve_Open" => GetCommandValue(liquidType.GetNestedType("WFLowerFaceDIWWashValve")!, "Open"),
            "WFLowerFaceDIWWashValve_Close" => GetCommandValue(liquidType.GetNestedType("WFLowerFaceDIWWashValve")!, "Close"),

            _ => throw new ArgumentException($"Unknown liquid command: {liquidCommand}")
        };
    }

    private (uint handle, byte[] value) GetCommandValue(Type type, string fieldName)
    {
        var field = type.GetField(fieldName) ?? throw new InvalidOperationException($"Field '{fieldName}' not found in type '{type.Name}'");
        var value = (int)field.GetValue(null)!;
        return (PlcHandleName, value.Value());
    }

    private (uint handle, ushort value, ushort bitOffset) GetCommandValueWithBitOffset(Type type, string fieldName)
    {
        var field = type.GetField(fieldName) ?? throw new InvalidOperationException($"Field '{fieldName}' not found in type '{type.Name}'");
        var value = (int)field.GetValue(null)!;
        var path = $"{type.FullName!.Replace("+", ".")}.{fieldName}";
        path = $"PlcHandle.{path.Split("PlcHandle.").Last()}";
        return (PlcHandleName, (ushort)value, PlcHandleHelper.GetHandleInfoByPath(path)!.BitOffset);
    }
}

/// <summary>
/// DryClean 配置工厂类
/// </summary>
public static class DryCleanConfigFactory
{
    public static GenericDryCleanConfig CreateDryAConfig(string plcAddress = "192.168.2.20", ushort plcPort = 8500)
        => new("Dry A1", Handle.CmdDryA, plcAddress, plcPort);

    public static GenericDryCleanConfig CreateDryBConfig(string plcAddress = "192.168.2.20", ushort plcPort = 8500)
        => new("Dry B1", Handle.CmdDryB, plcAddress, plcPort);
}

