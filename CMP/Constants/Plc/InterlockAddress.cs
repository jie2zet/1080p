using CMP.Controls;

namespace CMP.Constants.Plc;

/// <summary>
/// Interlock item definition used by the Operation/Interlock page.
/// Address values are kept separate because the PLC readback and write command
/// for an unlock switch are not necessarily the same Mitsubishi device.
/// </summary>
public sealed record InterlockDefinition(
    string Key,
    string DisplayName,
    string Side,
    InterlockGroup Group,
    SignalLightColor LightColor,
    string? ReadAddress = null,
    string? WriteAddress = null)
{
    public bool IsUnlock => WriteAddress is not null;
}

public enum InterlockGroup
{
    EmergencyStop,
    Unlock,
    EquipmentStatus
}

public static class InterlockAddressCatalog
{
    public static IReadOnlyList<InterlockDefinition> Definitions { get; } =
    [
        // B side emergency-stop lamps.
        new("EmergencyStopB1", "Emergency Stop", "B", InterlockGroup.EmergencyStop, SignalLightColor.Red, "B1080"),
        new("EmergencyStopB2", "Emergency Stop", "B", InterlockGroup.EmergencyStop, SignalLightColor.Red, "B10C0"),

        // A side emergency-stop lamps.
        new("EmergencyStopA1", "Emergency Stop", "A", InterlockGroup.EmergencyStop, SignalLightColor.Red, "B1000"),
        new("EmergencyStopA2", "Emergency Stop", "A", InterlockGroup.EmergencyStop, SignalLightColor.Red, "B1040"),
        new("EmergencyStopA3", "Emergency Stop", "A", InterlockGroup.EmergencyStop, SignalLightColor.Red, "B1100"),
        
        new("UnlockB1", "Efem Side B", "B", InterlockGroup.Unlock, SignalLightColor.Green, "B1202", "M11202"),
        new("UnlockB2", "Dry B", "B", InterlockGroup.Unlock, SignalLightColor.Green, "B1288", "M11288"),
        new("UnlockB3", "PenB1/RollB3", "B", InterlockGroup.Unlock, SignalLightColor.Green, "B1286", "M11286"),
        new("UnlockB4", "RollB1/RollB2", "B", InterlockGroup.Unlock, SignalLightColor.Green, "B1283", "M11283"),
        new("UnlockB5", "CleanB robot1", "B", InterlockGroup.Unlock, SignalLightColor.Green, "B1282", "M11282"),
        new("UnlockB6", "PolishC", "B", InterlockGroup.Unlock, SignalLightColor.Green, "B12C2", "M11322"),
        new("UnlockB7", "PolishD", "B", InterlockGroup.Unlock, SignalLightColor.Green, "B12C3", "M11323"),
        
        new("UnlockA1", "Efem Side A", "A", InterlockGroup.Unlock, SignalLightColor.Green, "B1203", "M11203"),
        new("UnlockA2", "DryA", "A", InterlockGroup.Unlock, SignalLightColor.Green,"B1248", "M11248"),
        new("UnlockA3", "PenA1/RollA3", "A", InterlockGroup.Unlock, SignalLightColor.Green, "B1246", "M11246"),
        new("UnlockA4", "RollA1/RollA2", "A", InterlockGroup.Unlock, SignalLightColor.Green, "B1243", "M11243"),
        new("UnlockA5", "Transfer Robot", "A", InterlockGroup.Unlock, SignalLightColor.Green, "B1242", "M11242"),
        new("UnlockA6", "PolishA", "A", InterlockGroup.Unlock, SignalLightColor.Green, "B1302", "M11362"),
        new("UnlockA7", "PolishB", "A", InterlockGroup.Unlock, SignalLightColor.Green, "B1303", "M11363"),

        // B side equipment status lamps.
        new("Roll2B3", "RollB3", "B", InterlockGroup.EquipmentStatus, SignalLightColor.Red, "B108E"),
        new("Roll2B2", "RollB2", "B", InterlockGroup.EquipmentStatus, SignalLightColor.Red, "B108D"),
        new("DryB3", "DryB", "B", InterlockGroup.EquipmentStatus, SignalLightColor.Red, "B1091"),
        new("PenB2", "PenB", "B", InterlockGroup.EquipmentStatus, SignalLightColor.Red, "B108F"),
        new("Roll1B1", "RollB1", "B", InterlockGroup.EquipmentStatus, SignalLightColor.Red, "B108C"),

        // A side equipment status lamps.
        new("DryA3", "DryA", "A", InterlockGroup.EquipmentStatus, SignalLightColor.Red, "B1051"),
        new("PenA1", "PenA", "A", InterlockGroup.EquipmentStatus, SignalLightColor.Red, "B104F"),
        new("Roll1A1", "RollA1", "A", InterlockGroup.EquipmentStatus, SignalLightColor.Red, "B104C"),
        new("Roll1A3", "RollA3", "A", InterlockGroup.EquipmentStatus, SignalLightColor.Red, "B104E"),
        new("Roll1A2", "RollA2", "A", InterlockGroup.EquipmentStatus, SignalLightColor.Red, "B104D")
    ];
}
