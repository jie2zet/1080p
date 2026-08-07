using CMP.Services.Log;
using CMP.Services.Tracking;
using System.IO;
using System.Text;

namespace CMP.Models.Operation.LoadPort;

public partial class LoadPortData
{
    private const int TrackingBaseAddress = 200000;
    private const int TrackingWordsPerWafer = 2000;
    private const int TrackingStepCount = 400;
    private const int TrackingWordsPerStep = 5;
    private const int TrackingWaferCountPerCassette = 25;
    private const int TrackingTotalWafers = 125;

    private sealed class WaferTrackingActionRecord
    {
        public int StepIndex { get; init; }
        public string ActionDescription { get; init; } = string.Empty;
        public string ActionTime { get; init; } = string.Empty;
    }

    private void WriteTrackingLogWhenWaferReturnedToFoup(int cassetteId, int layerNumber)
    {
        _ = Task.Run(() => WriteTrackingLogWhenWaferReturnedToFoupAsync(cassetteId, layerNumber));
    }

    private async Task WriteTrackingLogWhenWaferReturnedToFoupAsync(int cassetteId, int layerNumber)
    {
        try
        {
            var waferName = ResolveTrackingWaferName(cassetteId, layerNumber);
            var records = ReadWaferTrackingActionRecords(cassetteId, layerNumber);

            var now = DateTime.Now;
            var logDirectory = Path.Combine(
                @"D:\CMP\Log",
                now.Year.ToString(),
                now.ToString("yyyy-MM"),
                now.ToString("yyyy-MM-dd"),
                "WaferTracking");

            Directory.CreateDirectory(logDirectory);

            var filePath = Path.Combine(logDirectory, $"WaferActionTracking_{now:yyyy-MM-dd}.csv");
            if (!File.Exists(filePath))
            {
                const string header = "WaferName,LoadPort,CassetteId,SlotId,StepIndex,ActionDescription,ActionTime";
                await File.WriteAllTextAsync(filePath, header + Environment.NewLine, Encoding.UTF8);
            }

            var loadPortTag = $"loadport{cassetteId + 1}";
            var cassetteNo = cassetteId + 1;
            var slotNo = layerNumber + 1;

            var builder = new StringBuilder(records.Count * 128);
            foreach (var record in records)
            {
                builder.Append(EscapeCsv(waferName)).Append(',')
                    .Append(EscapeCsv(loadPortTag)).Append(',')
                    .Append(cassetteNo).Append(',')
                    .Append(slotNo).Append(',')
                    .Append(record.StepIndex).Append(',')
                    .Append(EscapeCsv(record.ActionDescription)).Append(',')
                    .Append(EscapeCsv(record.ActionTime))
                    .AppendLine();
            }

            await File.AppendAllTextAsync(filePath, builder.ToString(), Encoding.UTF8);

            var filledCount = records.Count(x => !string.IsNullOrEmpty(x.ActionTime));
            AddLogMessage($"wafer {waferName} 回 FOUP 已记录 {records.Count} 步 Tracking（{filledCount} 步有时间）");
            Logger.Debug($"wafer {waferName} 回 FOUP 已记录 {records.Count} 步 Tracking（{filledCount} 步有时间）");
        }
        catch (Exception ex)
        {
            Logger.Error($"写入 wafer tracking 明细失败: {ex.Message}", ex, "loadport.txt", 0);
        }
    }

    private List<WaferTrackingActionRecord> ReadWaferTrackingActionRecords(int cassetteId, int layerNumber)
    {
        var stepTimes = ReadWaferTrackingStepDateTimes(cassetteId, layerNumber);
        var records = new List<WaferTrackingActionRecord>(TrackingStepCount);

        for (var step = 0; step < TrackingStepCount; step++)
        {
            var parsed = stepTimes[step];
            records.Add(new WaferTrackingActionRecord
            {
                StepIndex = step + 1,
                ActionDescription = WaferActionDescriptionProvider.GetDescription(step),
                ActionTime = parsed.HasValue ? FormatActionTime(parsed.Value) : string.Empty
            });
        }

        return records;
    }

    /// <summary>
    /// 读取单片 wafer 的 400 步 Tracking 时间（0-based 索引，元素为 null 表示该步无记录）。
    /// </summary>
    internal DateTime?[] ReadWaferTrackingStepDateTimes(int cassetteId, int layerNumber)
    {
        var stepTimes = new DateTime?[TrackingStepCount];
        var trackingIndex = GetTrackingWaferIndex(cassetteId, layerNumber);
        if (!trackingIndex.HasValue)
        {
            return stepTimes;
        }

        var waferBase = GetTrackingWaferBaseAddress(trackingIndex.Value);
        short[]? block = ReadTrackingBlock(waferBase);
        if (block == null || block.Length < TrackingWordsPerWafer)
        {
            return stepTimes;
        }

        for (var step = 0; step < TrackingStepCount; step++)
        {
            var offset = step * TrackingWordsPerStep;
            var day = block[offset];
            var hour = block[offset + 1];
            var minute = block[offset + 2];
            var second = block[offset + 3];
            var millisecond = block[offset + 4];

            if (IsEmptyTrackingStep(day, hour, minute, second, millisecond))
            {
                continue;
            }

            var parsed = BuildTrackingTime(day, hour, minute, second, millisecond);
            if (parsed.HasValue)
            {
                stepTimes[step] = parsed.Value;
            }
            else
            {
                Logger.Warn(
                    $"Tracking 步 {step + 1} 时间无效: {day},{hour},{minute},{second},{millisecond} (D{waferBase + offset})",
                    "loadport.txt",
                    0);
            }
        }

        return stepTimes;
    }

    private short[]? ReadTrackingBlock(int waferBase)
    {
        try
        {
            if (plcService == null || !plcService.IsPLCConnected)
            {
                Logger.Warn($"PLC 未连接，无法读取 Tracking 块 D{waferBase}", "loadport.txt", 0);
                return null;
            }

            var read = plcService.ReadInt16Continuous($"D{waferBase}", (ushort)TrackingWordsPerWafer);
            if (!read.IsSuccess || read.Content == null || read.Content.Length < TrackingWordsPerWafer)
            {
                Logger.Error($"读取 Tracking 块 D{waferBase} 失败: {read.Message}", null, "loadport.txt", 0);
                return null;
            }

            return read.Content;
        }
        catch (Exception ex)
        {
            Logger.Error($"读取 Tracking 块 D{waferBase} 异常: {ex.Message}", ex, "loadport.txt", 0);
            return null;
        }
    }

    private static int? GetTrackingWaferIndex(int cassetteId, int layerNumber)
    {
        if (cassetteId < 0 || layerNumber < 0 || layerNumber >= TrackingWaferCountPerCassette)
        {
            return null;
        }

        var trackingIndex = cassetteId * TrackingWaferCountPerCassette + layerNumber;
        return trackingIndex < TrackingTotalWafers ? trackingIndex : null;
    }

    private static int GetTrackingWaferBaseAddress(int trackingIndex) =>
        TrackingBaseAddress + trackingIndex * TrackingWordsPerWafer;

    private string ResolveTrackingWaferName(int cassetteId, int layerNumber)
    {
        try
        {
            var loadport = LoadPortManager.Instance.GetLoadPort($"COM{cassetteId + 12}");
            if (loadport?.WaferList != null)
            {
                var waferIndex = loadport.WaferList.Count - (layerNumber + 1);
                if (waferIndex >= 0 && waferIndex < loadport.WaferList.Count)
                {
                    var name = loadport.WaferList[waferIndex].WaferName;
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        return name;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Warn($"解析 Tracking 晶圆名失败: {ex.Message}", "loadport.txt", 0);
        }

        return $"C{cassetteId + 1}W{layerNumber + 1}";
    }

    private static bool IsEmptyTrackingStep(int day, int hour, int minute, int second, int millisecond) =>
        day == 0 && hour == 0 && minute == 0 && second == 0 && millisecond == 0;

    private static DateTime? BuildTrackingTime(int day, int hour, int minute, int second, int millisecond)
    {
        if (day is < 1 or > 31 ||
            hour is < 0 or > 23 ||
            minute is < 0 or > 59 ||
            second is < 0 or > 59 ||
            millisecond is < 0 or > 999)
        {
            return null;
        }

        var today = DateTime.Today;
        return new DateTime(today.Year, today.Month, day, hour, minute, second, millisecond);
    }

    private static string FormatActionTime(DateTime time) =>
        $"{time.Day}日{time.Hour}时{time.Minute}分{time.Second}秒{time.Millisecond}毫秒";

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
