#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CMP.Models.DataCenter;
using CMP.Models.Resource;
using CMP.Services.Log;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static CMP.Tests.Services.DataLoggingTestSupport;

namespace CMP.Tests.Services;

/// <summary>
/// **一次性诊断用**：把每个腔体真实的 ListSV 内容、以及“结构体字段 → 采集键 → SV 行 → CSV 列头”的对照
/// 导出到工作区根目录的 ListSV_dump.txt，人工核对用。不修改任何生产代码。
/// </summary>
[TestClass]
public class ListSvDumpTests
{
    [TestMethod]
    public void Dump_ListSv_And_RuntimeKeyMapping()
    {
        var workspaceRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var dumpPath = Path.Combine(workspaceRoot, "ListSV_dump.txt");
        var lines = new List<string>
        {
            $"ListSV 导出（生成时间 {DateTime.Now:yyyy-MM-dd HH:mm:ss}）",
            "来源：DataLoggerTestSupport 按 DataCenter.InitSVXxx 的同一方式加载 D:\\CMP\\Configuration\\SV\\SV_*.csv",
            "说明：Key 就是 DataCenter 装进 ListSV 时用的键（= SV 表第 2 列 Name）；采集端 SetSv 用的是“腔体前缀+工位号+结构体字段名”，",
            "      两者往往不是同一个字符串 —— 这份文件就是用来核对它们能不能对上。",
            string.Empty
        };

        foreach (var (label, fileName, chamber) in CreateAllChambers())
        {
            DumpChamber(lines, label, fileName, chamber);
        }

        File.WriteAllLines(dumpPath, lines, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        // 让测试输出里也能看到路径（MSTest 会打印）
        Console.WriteLine($"ListSV dump -> {dumpPath}");
        Assert.IsTrue(File.Exists(dumpPath), $"没写出文件：{dumpPath}");
    }

    private static void DumpChamber(List<string> lines, string label, string fileName, CollectDataChamber chamber)
    {
        var prefix = chamber.KeyPrefix;
        var entries = chamber.ListSV.Values.ToList();
        var withPrefix = entries.Count(sv => sv.Name.StartsWith(prefix, StringComparison.Ordinal));
        var metadata = DataLogFormatting.BuildMetadataIndex(entries);

        lines.Add(new string('=', 150));
        lines.Add($"【{label}】ListSV 原始内容：共 {entries.Count} 条；名字以“{prefix}”开头的 {withPrefix} 条；"
            + $"不以它开头的 {entries.Count - withPrefix} 条");
        lines.Add(new string('=', 150));
        lines.Add($"{"Key(=SV.Name)",-70} {"SVID",6} {"Type",6} {"Value",-12} {"Unit",-8} Comment");
        foreach (var sv in entries.OrderBy(sv => sv.SVID).ThenBy(sv => sv.Name, StringComparer.Ordinal))
        {
            lines.Add($"{sv.Name,-70} {sv.SVID,6} {sv.Type,6} {(sv.Value?.ToString() ?? ""),-12} {sv.Unit,-8} {sv.Comment}");
        }

        // 归一化重名检查：BuildMetadataIndex 用 TryAdd，取的是第一条 → 后一条的 Comment/Unit 用不上
        var duplicates = entries
            .GroupBy(sv => DataLogFormatting.NormalizeSvKey(sv.Name), StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .ToList();
        lines.Add(string.Empty);
        lines.Add($"归一化后重名的 SV（BuildMetadataIndex 只保留第一条，列头会取第一条的 Comment/Unit）：{duplicates.Count} 组");
        foreach (var group in duplicates)
        {
            lines.Add($"    [{group.Key}] " + string.Join(" || ", group.Select(sv => $"{sv.SVID}:{sv.Name}[{sv.Comment}]")));
        }

        // 结构体字段 → 采集键 → SV 行 → CSV 列头
        lines.Add(string.Empty);
        lines.Add("--- 结构体字段 → 采集端 SetSv 的键 → 日志端查找的键 → 归一化命中的 SV 行 → CSV 列头 ---");
        foreach (var (kind, fields, stationPrefixes) in new (string, System.Reflection.FieldInfo[], string[])[]
                 {
                     ("collectData", chamber.collectDataFields, [string.Empty]),
                     ("ioData", chamber.ioDataFields, chamber.StationPrefixes.ToArray())
                 })
        {
            foreach (var stationPrefix in stationPrefixes)
            {
                var stationLabel = stationPrefix.Length == 0 ? "-" : stationPrefix;
                var shown = 0;
                foreach (var field in fields)
                {
                    // 与日志端同样的过滤规则
                    if (field.Name.Contains("Void")) continue;
                    if (kind == "ioData" && CollectDataChamber.IsPaddingField(field.Name)) continue;
                    if (stationPrefix.Length == 0 && kind == "collectData"
                        && chamber.StationPrefixes.Count > 1
                        && !field.Name.StartsWith(stationPrefix, StringComparison.Ordinal)
                        && chamber.StationPrefixes.Any(p => field.Name.StartsWith(p, StringComparison.Ordinal)))
                    {
                        // 带工位号的 collectData 字段由调用方按工位过滤，这里只在对应工位列出
                    }

                    var runtimeKey = chamber.BuildRuntimeKey(
                        kind == "ioData" && stationPrefix.Length > 0 ? stationPrefix : null,
                        field.Name);
                    // collectData 的工位过滤（Roller/Pen）：键里带的是字段名自带的 _N，这里按字段名过滤
                    if (kind == "collectData" && chamber.StationPrefixes.Count > 1)
                    {
                        var wanted = chamber.StationPrefixes.ToList().IndexOf(stationPrefix);
                        if (wanted >= 0)
                        {
                            var expected = chamber.StationPrefixes[wanted];
                            if (!field.Name.StartsWith(expected, StringComparison.Ordinal)) continue;
                        }
                    }

                    var exists = chamber.ListSV.ContainsKey(runtimeKey);
                    var matched = DataLogFormatting.ResolveSvMetadata(metadata, runtimeKey, label);
                    var header = matched == null ? "(无法解析→该列会被丢弃)" : DataLogFormatting.FormatSvHeader(matched);
                    var value = chamber.GetSv(runtimeKey)?.Value?.ToString() ?? "(未采集)";
                    lines.Add($"  {kind,-11} {stationLabel,-3} {field.Name,-58} key={runtimeKey,-58} 已在ListSV={exists,-5} "
                        + $"命中={matched?.Name ?? "(无)",-58} 列头={header,-70} 值={value}");
                    shown++;
                }

                if (shown == 0)
                {
                    lines.Add($"  {kind,-11} {stationLabel,-3} （无字段）");
                }
            }
        }

        lines.Add(string.Empty);
        lines.Add($"--- 采集一次（喂全 0 缓冲区）后 ListSV 的变化：新增运行期键（SVID=0） "
            + $"{chamber.ListSV.Values.Count(sv => sv.SVID == 0)} 条 ---");
        lines.Add(string.Empty);
    }

    private static List<(string Label, string FileName, CollectDataChamber Chamber)> CreateAllChambers()
    {
        var result = new List<(string, string, CollectDataChamber)>();
        foreach (var side in new[] { "A", "B", "C", "D" })
        {
            var polish = CreatePolishWithSv($"Polish{side}", $"SV_Polish{side}.csv");
            if (polish != null) result.Add(($"Polish{side}", $"SV_Polish{side}.csv", polish));
        }

        foreach (var side in new[] { "A", "B" })
        {
            var roller = CreateRollerWithSv($"Roller{side}", $"SV_Roller{side}.csv");
            if (roller != null) result.Add(($"Roller{side}", $"SV_Roller{side}.csv", roller));

            var pen = CreatePenWithSv($"Pen{side}", $"SV_Pen{side}.csv");
            if (pen != null) result.Add(($"Pen{side}", $"SV_Pen{side}.csv", pen));

            var dry = CreateDryWithSv($"Dry{side}", $"SV_Dry{side}.csv");
            if (dry != null) result.Add(($"Dry{side}", $"SV_Dry{side}.csv", dry));
        }

        return result;
    }
}
