using CMP.Models.DataCenter;
using CMP.Models.Recipe.Sequence;
using CMP.Models.Resource;
using Newtonsoft.Json;
using System.IO;

namespace CMP.Services.Log;

internal static class DataLogFormatting
{
    internal const string UnknownWaferLogName = "Unknown(Unknown)";

    /// <summary>
    /// 运行期结构体字段名与 SV 变量表变量名不一致时的兜底别名表。
    /// Key 是 <see cref="NormalizeSvKey"/> 后的字段名，Value 是 <see cref="NormalizeSvKey"/> 后的 SV 变量名，
    /// 两者都不含 chamber 前缀；SV 表里带前缀的写法（PenA1 / DryA / RollerA1 …）由
    /// <see cref="ResolveSvMetadata"/> 拼回 chamber 前缀后再查一次。
    /// 缺少别名会让对应列在 CSV 日志中被静默丢弃（见 DataLoggerHelper.AddSvField）。
    /// </summary>
    private static readonly Dictionary<string, string> SvKeyAliases = new(StringComparer.Ordinal)
    {
        // KHSTIO.stPolishIOData 字段名 -> SV_PolishX.csv 变量名
        ["DIWOUTLETVALVECONTROL"] = "DRESSINGWATEROUTLETVALVECONTROL",
        ["CONDITIONERDIWWASHVALVECONTROL"] = "CONDITIONDIWWASHVALVECONTROL",
        ["CONDITIONERARMDIWWASHVALVECONTROL"] = "CONDITIONARMDIWWASHVALVECONTROL",
        ["PLATENCOOLWATERVALVECONTROL"] = "PLATENCOOLINGWATERVALVECONTROL",
        ["PLATENMOTORCOOLWATERVALVECONTROL"] = "PLATENMOTORCOOLINGWATERVALVECONTROL",
        ["TOPRINGPRESSVALVE1CONTROL"] = "RETAINERRINGPRESSVALVE1CONTROL",
        ["TOPRINGPRESSVALVE2CONTROL"] = "RETAINERRINGPRESSVALVE2CONTROL",
        ["TOPRINGVACUUMVALVECONTROL"] = "RETAINERRINGVACUUMVALVECONTROL",
        ["TOPRINGVENTVALVECONTROL"] = "RETAINERRINGVENTVALVECONTROL",
        ["ZONE4ATMOSPHERICVALVECONTROL"] = "ZONE4VENTVALVECONTROL",
        ["CATCHWATERBLOWVALVECONTROL"] = "WATERSEPARATORBLOWVALVECONTROL",
        ["CATCHWATERDRAINVALVECONTROL"] = "WATERSEPARATORWATERDRAINVALVECONTROL",
        ["HEADEXTERNALBLOWN2VALVECONTROL"] = "RELEASEN2PURGEVALVECONTROL"
    };

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

    /// <summary>
    /// Resolves the Unit Recipe that a Polish chamber is executing for a wafer.
    /// The sequence recipe stores the selected Polish recipe as a file path, so
    /// only the file name is returned for use in a log file name.
    /// </summary>
    internal static bool TryGetPolishRecipeName(
        Wafer? wafer,
        string chamberName,
        out string recipeName,
        string sequenceRecipeDirectory = @"D:\CMP\Configuration\Recipes\Sequence")
    {
        recipeName = string.Empty;

        var selectionKey = chamberName switch
        {
            "PolishA" => "RecipePolishA",
            "PolishB" => "RecipePolishB",
            "PolishC" => "RecipePolishC",
            "PolishD" => "RecipePolishD",
            _ => null
        };

        if (selectionKey == null || wafer == null || string.IsNullOrWhiteSpace(wafer.SequenceRecipe))
        {
            return false;
        }

        try
        {
            var sequenceName = Path.GetFileNameWithoutExtension(wafer.SequenceRecipe);
            if (string.IsNullOrWhiteSpace(sequenceName))
            {
                return false;
            }

            var sequencePath = Path.Combine(sequenceRecipeDirectory, $"{sequenceName}.json");
            if (!File.Exists(sequencePath))
            {
                return false;
            }

            var sequence = JsonConvert.DeserializeObject<SequenceRecipeData>(File.ReadAllText(sequencePath));
            var selectedRecipe = sequence?.RecipeSelections?
                .FirstOrDefault(selection => string.Equals(selection?.Key, selectionKey, StringComparison.Ordinal))
                ?.Value?.ToString();
            if (string.IsNullOrWhiteSpace(selectedRecipe))
            {
                return false;
            }

            recipeName = Path.GetFileNameWithoutExtension(selectedRecipe.Trim());
            return !string.IsNullOrWhiteSpace(recipeName);
        }
        catch
        {
            // A missing or malformed sequence recipe must not stop data logging.
            recipeName = string.Empty;
            return false;
        }
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
        var hasChamberPrefix = normalizedChamberName.Length > 0
            && normalizedRuntimeKey.StartsWith(normalizedChamberName, StringComparison.Ordinal);
        var keyWithoutChamber = hasChamberPrefix
            ? normalizedRuntimeKey[normalizedChamberName.Length..]
            : normalizedRuntimeKey;

        // 结构体字段名与 SV 变量表命名不一致时用别名兜底。
        if (SvKeyAliases.TryGetValue(keyWithoutChamber, out var aliasKey))
        {
            if (metadataByName.TryGetValue(aliasKey, out var aliasMatch))
            {
                return aliasMatch;
            }

            if (hasChamberPrefix
                && metadataByName.TryGetValue(normalizedChamberName + aliasKey, out var prefixedAliasMatch))
            {
                return prefixedAliasMatch;
            }
        }

        if (hasChamberPrefix && metadataByName.TryGetValue(keyWithoutChamber, out var matchWithoutChamber))
        {
            return matchWithoutChamber;
        }

        return null;
    }

    internal static string NormalizeSvKey(string value)
    {
        return string.Concat(value.Where(char.IsLetterOrDigit)).ToUpperInvariant();
    }
}
