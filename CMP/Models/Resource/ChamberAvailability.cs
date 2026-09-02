using CMP.Models.Warn;

namespace CMP.Models.Resource;

/// <summary>
/// Centralized feature switches for chambers that are not installed or not in use.
/// </summary>
public static class ChamberAvailability
{
    private static readonly HashSet<string> DisabledChambers = new(StringComparer.OrdinalIgnoreCase)
    {
        "PenA2",
        "PenB2"
    };

    public static bool IsEnabled(string? chamberName)
    {
        return !string.IsNullOrWhiteSpace(chamberName) && !DisabledChambers.Contains(chamberName);
    }

    public static bool IsEnabled(Unit unit)
    {
        return unit is not (Unit.PenA2 or Unit.PenB2);
    }
}
