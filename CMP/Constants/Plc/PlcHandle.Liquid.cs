using System.Reflection;

namespace CMP.Constants.Plc;

public static partial class PlcHandle
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class LiquidControlAttribute(string displayNameKey, string drawingTag = "") : Attribute
    {
        public string DisplayNameKey { get; } = displayNameKey;
        public string DrawingTag { get; } = drawingTag;
        public string CommandKeyPrefix { get; set; } = string.Empty;
        // Allows existing PLC setpoints outside the control project's Param class to be reused.
        public string SetpointPath { get; set; } = string.Empty;
    }
}

public sealed record LiquidCommandDefinition(
    string CommandPath,
    string CommandKey,
    string ProjectKey,
    string ActionKey,
    string PidTag,
    uint? ParameterHandle = null)
{
    public uint Handle => PlcHandleHelper.GetHandleByPath(CommandPath)
        ?? throw new InvalidOperationException($"Liquid command Handle is not defined: {CommandPath}");

    public int CommandCode => PlcHandleHelper.GetValueByPath(CommandPath)
        ?? throw new InvalidOperationException($"Liquid command value is not defined: {CommandPath}");
}

public static partial class PlcHandleHelper
{
    public static IReadOnlyList<LiquidCommandDefinition> GetLiquidCommands(string chamberName)
    {
        var chamberType = typeof(PlcHandle).GetNestedType(chamberName)
            ?? throw new ArgumentException($"Unknown PLC chamber: {chamberName}", nameof(chamberName));
        var commands = new List<LiquidCommandDefinition>();
        AddLiquidCommands(chamberType, chamberName, commands);
        return commands.OrderBy(command => command.CommandCode).ToArray();
    }

    private static void AddLiquidCommands(Type type, string path, List<LiquidCommandDefinition> commands)
    {
        var project = type.GetCustomAttribute<PlcHandle.LiquidControlAttribute>();
        if (project != null)
        {
            var commandKeyPrefix = string.IsNullOrEmpty(project.CommandKeyPrefix)
                ? type.Name : project.CommandKeyPrefix;
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (field.FieldType != typeof(int) || field.GetCustomAttribute<PlcHandleAttribute>() == null)
                    continue;

                var actionKey = field.Name switch
                {
                    "Open" => "ActionOpen",
                    "Close" => "ActionClose",
                    "Flowrate" => "ActionFlowrate",
                    "FlowrateReset" => "ActionFlowrateReset",
                    "ClfcCalibration" => "ActionClfcCalibration",
                    "FCCalibration" => "ActionFcCalibration",
                    "MFCCalibration" => "ActionMfcCalibration",
                    "Pressure" => "ActionPressure",
                    "PressureReset" => "ActionPressureReset",
                    "EVCalibration" => "ActionEvCalibration",
                    "Power" => "ActionPower",
                    _ => throw new InvalidOperationException($"Unknown liquid action: {path}.{field.Name}")
                };
                var parameterName = field.Name switch
                {
                    "Flowrate" => "FlowrateSet",
                    "Pressure" => "PressureSet",
                    _ => null
                };
                var parameterPath = string.IsNullOrEmpty(project.SetpointPath)
                    ? $"{path}.Param.{parameterName}" : project.SetpointPath;
                var parameterHandle = parameterName == null ? (uint?)null
                    : (GetParamByPath(parameterPath)
                       ?? throw new InvalidOperationException($"Liquid setpoint is not defined: {parameterPath}")).Handle;

                commands.Add(new LiquidCommandDefinition($"{path}.{field.Name}",
                    $"{commandKeyPrefix}_{field.Name}", project.DisplayNameKey, actionKey,
                    project.DrawingTag, parameterHandle));
            }
        }

        foreach (var nestedType in type.GetNestedTypes())
            AddLiquidCommands(nestedType, $"{path}.{nestedType.Name}", commands);
    }
}
