using CMP.Models.DataCenter;
using CMP.Models.Resource;

namespace CMP.Services.Log;

internal static class DataLogFormatting
{
    internal const string UnknownWaferLogName = "Unknown(Unknown)";

    internal static bool TryBuildWaferLogName(Wafer? wafer, out string waferLogName)
    {
        if (wafer == null)
        {
            waferLogName = UnknownWaferLogName;
            return false;
        }

        var waferId = $"C{wafer.LoadportID + 1}W{wafer.SlotID + 1}";
        waferLogName = string.IsNullOrWhiteSpace(wafer.WaferName)
            ? $"({waferId})"
            : $"{wafer.WaferName}({waferId})";
        return true;
    }

    internal static string FormatSvHeader(SV? sv)
    {
        return sv == null
            ? "_"
            : $"{sv.Comment}_{sv.Name}({sv.Unit})";
    }

    internal static Dictionary<string, SV> BuildMetadataIndex(IEnumerable<SV> svs)
    {
        var result = new Dictionary<string, SV>(StringComparer.Ordinal);
        foreach (var sv in svs)
        {
            if (sv.SVID == 0 || string.IsNullOrWhiteSpace(sv.Name))
            {
                continue;
            }

            result.TryAdd(NormalizeSvKey(sv.Name), sv);
        }

        return result;
    }

    internal static SV? ResolveSvMetadata(
        IReadOnlyDictionary<string, SV> metadataByName,
        string runtimeKey,
        string chamberName)
    {
        var normalizedRuntimeKey = NormalizeSvKey(runtimeKey);
        if (metadataByName.TryGetValue(normalizedRuntimeKey, out var exactMatch))
        {
            return exactMatch;
        }

        var normalizedChamberName = NormalizeSvKey(chamberName);
        if (normalizedRuntimeKey.StartsWith(normalizedChamberName, StringComparison.Ordinal))
        {
            var keyWithoutChamber = normalizedRuntimeKey[normalizedChamberName.Length..];
            if (metadataByName.TryGetValue(keyWithoutChamber, out var matchWithoutChamber))
            {
                return matchWithoutChamber;
            }
        }

        return null;
    }

    internal static string NormalizeSvKey(string value)
    {
        return string.Concat(value.Where(char.IsLetterOrDigit)).ToUpperInvariant();
    }
}
