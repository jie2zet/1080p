using System.Globalization;

namespace CMP.Services.Tracking;

/// <summary>
/// CassetteLog / Tracking 汇总使用的时间格式约定。
/// </summary>
public static class WaferTrackingTimeFormats
{
    /// <summary>CSV 列内时间（Enter/Exit/Process），例：11:25:45</summary>
    public const string TimestampFormat = "HH:mm:ss";

    /// <summary>日志文件生成时刻，例：2026-06-08 11:30:00</summary>
    public const string FileTimeFormat = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// 输出 Excel 友好的 CSV 时间文本。
    /// 使用 ="HH:mm:ss" 公式，避免 Excel 自动识别为时间后套成 mm:ss 只显示分秒。
    /// </summary>
    public static string FormatTimestamp(DateTime? time)
    {
        if (!time.HasValue)
        {
            return string.Empty;
        }

        var text = time.Value.ToString(TimestampFormat, CultureInfo.InvariantCulture);
        return $"=\"{text}\"";
    }

    public static string FormatDurationSeconds(TimeSpan? duration) =>
        duration.HasValue
            ? duration.Value.TotalSeconds.ToString("F3", CultureInfo.InvariantCulture)
            : string.Empty;
}
