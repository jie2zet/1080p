using CMP.Models.Recipe.Sequence;
using CMP.Models.Resource;
using CMP.Services.Log;
using CMP.Services.Tracking;
using Newtonsoft.Json;
using System.Globalization;
using System.IO;
using System.Text;

namespace CMP.Models.Operation.LoadPort;

public partial class LoadPortData
{
    private readonly object _cassetteLogSnapshotLock = new();
    private readonly Dictionary<int, CassetteLogSnapshot> _cassetteLogSnapshots = new();

    private sealed class CassetteLogSnapshot
    {
        public int SlotId { get; init; }
        public string WaferName { get; init; } = string.Empty;
        public string SequenceName { get; init; } = string.Empty;
        public IReadOnlyList<WaferChamberVisitRecord> ChamberVisits { get; init; } = [];
    }

    private static readonly string[] CassetteLogHeaders =
    [
        "CassetteID",
        "LoadPort",
        "SlotNo",
        "WaferName",
        "WaferID",
        "Sequence",
        "Chamber",
        "Recipe",
        "Planned",
        "EnterTime",
        "ExitTime",
        "DwellSec",
        "ProcessStartTime",
        "ProcessEndTime",
        "ProcessSec",
        "Status",
        "Remark"
    ];

    private static readonly IReadOnlyDictionary<string, string> CassetteLogRecipeSelectionMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["PolishA"] = "RecipePolishA",
            ["PolishB"] = "RecipePolishB",
            ["PolishC"] = "RecipePolishC",
            ["PolishD"] = "RecipePolishD",
            ["RollerA1"] = "RecipeRollerA1",
            ["RollerA2"] = "RecipeRollerA2",
            ["RollerA3"] = "RecipeRollerA3",
            ["RollerB1"] = "RecipeRollerB1",
            ["RollerB2"] = "RecipeRollerB2",
            ["RollerB3"] = "RecipeRollerB3",
            ["PenA"] = "RecipePenA1",           
            ["PenB"] = "RecipePenB1",           
            ["DryA"] = "RecipeDryA",
            ["DryB"] = "RecipeDryB"
        };

    private void CaptureCassetteLogSnapshot(Wafer wafer, int cassetteId, int layerNumber)
    {
        if (wafer == null || string.IsNullOrWhiteSpace(wafer.SequenceRecipe))
        {
            return;
        }

        var waferName = string.IsNullOrWhiteSpace(wafer.WaferName)
            ? $"C{cassetteId + 1}W{layerNumber + 1}"
            : wafer.WaferName;

        var stepTimes = ReadWaferTrackingStepDateTimes(cassetteId, layerNumber);
        var sequenceData = LoadSequenceRecipeData(wafer.SequenceRecipe);
        var recipeMap = BuildCassetteLogRecipeMap(sequenceData);
        var plannedChambers = BuildPlannedChambers(wafer);
        var visits = WaferChamberVisitBuilder.Build(stepTimes, recipeMap, plannedChambers);

        lock (_cassetteLogSnapshotLock)
        {
            _cassetteLogSnapshots[wafer.SlotID] = new CassetteLogSnapshot
            {
                SlotId = wafer.SlotID,
                WaferName = waferName,
                SequenceName = wafer.SequenceRecipe,
                ChamberVisits = visits
            };
        }
    }

    private void WriteCassetteLogWhenFoupUnload()
    {
        CassetteLogSnapshot[] snapshots;
        lock (_cassetteLogSnapshotLock)
        {
            snapshots = _cassetteLogSnapshots.Values
                .OrderByDescending(w => w.SlotId)
                .ToArray();
            _cassetteLogSnapshots.Clear();
        }

        if (snapshots.Length == 0)
        {
            AddLogMessage($"CassetteLog skipped for {PortNumber}: no completed wafer snapshots.");
            return;
        }

        var cassetteId = GetCassetteIdForLog();
        _ = Task.Run(() => WriteCassetteLogWhenFoupUnloadCore(cassetteId, snapshots));
    }

    private void WriteCassetteLogWhenFoupUnloadCore(string cassetteId, IReadOnlyCollection<CassetteLogSnapshot> snapshots)
    {
        try
        {
            var now = DateTime.Now;
            var logDirectory = Path.Combine(
                @"D:\CMP\Log",
                now.Year.ToString(),
                now.ToString("yyyy-MM"),
                now.ToString("yyyy-MM-dd"),
                "CassetteLog");

            Directory.CreateDirectory(logDirectory);

            var filePath = Path.Combine(
                logDirectory,
                $"CassetteLog_{cassetteId}_{now:yyyy-MM-dd_HH-mm-ss}.csv");

            var loadPort = GetLoadPortTagForLog();
            var lines = new List<string>(snapshots.Count * 8 + 1)
            {
                string.Join(",", CassetteLogHeaders)
            };

            foreach (var snapshot in snapshots.OrderByDescending(s => s.SlotId))
            {
                var slotNo = snapshot.SlotId + 1;
                var waferId = $"{cassetteId}W{slotNo}";

                foreach (var visit in snapshot.ChamberVisits)
                {
                    lines.Add(BuildCassetteLogDetailRow(
                        cassetteId,
                        loadPort,
                        slotNo,
                        snapshot.WaferName,
                        waferId,
                        snapshot.SequenceName,
                        visit));
                }
            }

            if (lines.Count <= 1)
            {
                AddLogMessage($"CassetteLog skipped for {PortNumber}: no chamber visit rows.");
                return;
            }

            File.WriteAllLines(filePath, lines, Encoding.UTF8);
            AddLogMessage($"CassetteLog generated: {filePath} ({lines.Count - 1} rows)");
        }
        catch (Exception ex)
        {
            Logger.Error($"写入 CassetteLog 失败: {ex.Message}", ex, "loadport.txt", 0);
        }
    }

    private static string BuildCassetteLogDetailRow(
        string cassetteId,
        string loadPort,
        int slotNo,
        string waferName,
        string waferId,
        string sequenceName,
        WaferChamberVisitRecord visit)
    {
        var shouldWriteTimes = visit.Planned;
        var hasDwellTimes = shouldWriteTimes && visit.EnterTime.HasValue && visit.ExitTime.HasValue;
        var hasProcessTimes = shouldWriteTimes && visit.ProcessStartTime.HasValue && visit.ProcessEndTime.HasValue;

        return string.Join(',',
        [
            EscapeCsv(cassetteId),
            EscapeCsv(loadPort),
            slotNo.ToString(CultureInfo.InvariantCulture),
            EscapeCsv(waferName),
            EscapeCsv(waferId),
            EscapeCsv(sequenceName),
            EscapeCsv(visit.ChamberName),
            EscapeCsv(visit.RecipeName),
            visit.Planned ? "Yes" : "No",
            EscapeCsv(shouldWriteTimes ? WaferTrackingTimeFormats.FormatTimestamp(visit.EnterTime) : string.Empty),
            EscapeCsv(shouldWriteTimes ? WaferTrackingTimeFormats.FormatTimestamp(visit.ExitTime) : string.Empty),
            EscapeCsv(hasDwellTimes ? WaferTrackingTimeFormats.FormatDurationSeconds(visit.DwellDuration) : string.Empty),
            EscapeCsv(hasProcessTimes ? WaferTrackingTimeFormats.FormatTimestamp(visit.ProcessStartTime) : string.Empty),
            EscapeCsv(hasProcessTimes ? WaferTrackingTimeFormats.FormatTimestamp(visit.ProcessEndTime) : string.Empty),
            EscapeCsv(hasProcessTimes ? WaferTrackingTimeFormats.FormatDurationSeconds(visit.ProcessDuration) : string.Empty),
            EscapeCsv(shouldWriteTimes ? visit.Status.ToString() : string.Empty),
            EscapeCsv(string.Empty)
        ]);
    }

    private static SequenceRecipeData? LoadSequenceRecipeData(string? sequenceName)
    {
        if (string.IsNullOrWhiteSpace(sequenceName))
        {
            return null;
        }

        var sequencePath = Path.Combine(@"D:\CMP\Configuration", "Recipes", "Sequence", $"{sequenceName}.json");
        if (!File.Exists(sequencePath))
        {
            return null;
        }

        try
        {
            var text = File.ReadAllText(sequencePath);
            return JsonConvert.DeserializeObject<SequenceRecipeData>(text);
        }
        catch (Exception ex)
        {
            Logger.Error($"读取 Sequence 配方 {sequenceName} 失败: {ex.Message}", ex, "loadport.txt", 0);
            return null;
        }
    }

    private static IReadOnlyDictionary<string, string> BuildCassetteLogRecipeMap(SequenceRecipeData? data)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        if (data?.RecipeSelections == null)
        {
            return map;
        }

        var selectionMap = data.RecipeSelections
            .Where(x => x != null && !string.IsNullOrWhiteSpace(x.Key) && x.Value != null)
            .ToDictionary(x => x!.Key!, x => x!.Value?.ToString() ?? string.Empty, StringComparer.Ordinal);

        foreach (var item in CassetteLogRecipeSelectionMap)
        {
            if (selectionMap.TryGetValue(item.Value, out var recipeName) && !string.IsNullOrWhiteSpace(recipeName))
            {
                map[item.Key] = Path.GetFileNameWithoutExtension(recipeName);
            }
        }

        return map;
    }

    private static HashSet<string> BuildPlannedChambers(Wafer wafer)
    {
        var planned = wafer.WaferSequenceModels?
            .Where(m => !string.IsNullOrWhiteSpace(m.ChamberName))
            .Select(m => m.ChamberName!)
            .ToHashSet(StringComparer.Ordinal);

        return planned is { Count: > 0 }
            ? planned
            : [];
    }

    private string GetCassetteIdForLog()
    {
        if (!string.IsNullOrWhiteSpace(CarrierId))
        {
            return CarrierId;
        }

        return PortNumber switch
        {
            "COM12" => "C1",
            "COM13" => "C2",
            "COM14" => "C3",
            "COM15" => "C4",
            "COM16" => "C5",
            _ => PortNumber
        };
    }

    private string GetLoadPortTagForLog()
    {
        return PortNumber switch
        {
            "COM12" => "loadport1",
            "COM13" => "loadport2",
            "COM14" => "loadport3",
            "COM15" => "loadport4",
            "COM16" => "loadport5",
            _ => PortNumber
        };
    }
}
