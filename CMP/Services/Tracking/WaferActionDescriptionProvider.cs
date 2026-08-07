using CMP.Services.Log;
using System.IO;
using System.Text;

namespace CMP.Services.Tracking;

/// <summary>
/// 进程内缓存 400 步 Wafer Tracking 动作说明（启动时从 CSV 加载一次）。
/// </summary>
public static class WaferActionDescriptionProvider
{
    public const int StepCount = 400;

    private const string DefaultCsvPath = @"D:\CMP\Configuration\Tracking\WaferActionDescriptions.csv";
    private const string RelativeCsvPath = @"Configuration\Tracking\WaferActionDescriptions.csv";

    private static readonly object Sync = new();
    private static readonly string[] Descriptions = new string[StepCount];
    private static bool _initialized;
    private static int _loadedCount;

    public static bool IsInitialized => _initialized;

    public static int LoadedCount => _loadedCount;

    public static void Initialize(string? csvPath = null)
    {
        lock (Sync)
        {
            if (_initialized && _loadedCount > 0)
            {
                return;
            }

            _loadedCount = LoadFromCsv(ResolveCsvPath(csvPath));
            _initialized = _loadedCount > 0;

            if (!_initialized)
            {
                Logger.Error(
                    "WaferActionDescriptions 未成功加载，Tracking 日志将使用 Step_N 占位说明。请检查 CSV 路径。",
                    null,
                    "loadport.txt",
                    0);
            }
        }
    }

    /// <param name="stepIndex">0-based，对应 PLC 块内第 stepIndex 组 5 字。</param>
    public static string GetDescription(int stepIndex)
    {
        if (stepIndex < 0 || stepIndex >= StepCount)
        {
            return $"Step_{stepIndex + 1}";
        }

        EnsureInitialized();

        return Descriptions[stepIndex];
    }

    private static void EnsureInitialized()
    {
        if (_initialized && _loadedCount > 0)
        {
            return;
        }

        Initialize();
    }

    private static string ResolveCsvPath(string? csvPath)
    {
        if (!string.IsNullOrWhiteSpace(csvPath) && File.Exists(csvPath))
        {
            return csvPath;
        }

        var candidates = new[]
        {
            DefaultCsvPath,
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RelativeCsvPath),
            Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", RelativeCsvPath))
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return csvPath ?? DefaultCsvPath;
    }

    private static int LoadFromCsv(string csvPath)
    {
        for (var i = 0; i < StepCount; i++)
        {
            Descriptions[i] = $"Step_{i + 1}";
        }

        if (!File.Exists(csvPath))
        {
            Logger.Error($"WaferActionDescriptions.csv 不存在: {csvPath}", null, "loadport.txt", 0);
            return 0;
        }

        try
        {
            var lines = File.ReadAllLines(csvPath, Encoding.UTF8);
            var loadedCount = 0;

            foreach (var rawLine in lines)
            {
                if (string.IsNullOrWhiteSpace(rawLine))
                {
                    continue;
                }

                var line = rawLine.Trim().TrimStart('\uFEFF');
                if (IsHeaderLine(line))
                {
                    continue;
                }

                if (TryApplyDescriptionLine(line, out var applied) && applied)
                {
                    loadedCount++;
                }
            }

            if (loadedCount == 0)
            {
                Logger.Error($"WaferActionDescriptions.csv 未解析到有效数据: {csvPath}", null, "loadport.txt", 0);
            }
            else if (loadedCount < StepCount)
            {
                Logger.Warn(
                    $"WaferActionDescriptions.csv 仅加载 {loadedCount}/{StepCount} 步，其余使用占位说明",
                    "loadport.txt",
                    0);
            }
            else
            {
                Logger.Info($"WaferActionDescriptions 已加载 {loadedCount} 步: {csvPath}", "loadport.txt", 0);
            }

            return loadedCount;
        }
        catch (Exception ex)
        {
            Logger.Error($"加载 WaferActionDescriptions.csv 失败: {ex.Message}", ex, "loadport.txt", 0);
            return 0;
        }
    }

    private static bool TryApplyDescriptionLine(string line, out bool applied)
    {
        applied = false;
        var commaIndex = line.IndexOf(',');
        if (commaIndex < 0)
        {
            return false;
        }

        var stepText = line[..commaIndex].Trim();
        var description = line[(commaIndex + 1)..].Trim();
        if (string.IsNullOrWhiteSpace(description))
        {
            return false;
        }

        if (int.TryParse(stepText, out var stepNumber) &&
            stepNumber is >= 1 and <= StepCount)
        {
            Descriptions[stepNumber - 1] = description;
            applied = true;
            return true;
        }

        return false;
    }

    private static bool IsHeaderLine(string line)
    {
        if (line.StartsWith("Step", StringComparison.OrdinalIgnoreCase) &&
            line.Contains("Description", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return line.Equals("Description", StringComparison.OrdinalIgnoreCase);
    }
}
