#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CMP.Models.DataCenter;
using CMP.Models.kvsPlc;
using CMP.Models.Resource;
using CMP.Services.Log;
using CsvHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CMP.Tests.Services;

/// <summary>
/// DataLogging 测试的公共夹具：加载 SV 变量表、造腔体对象、写/读 CSV、断言单元格。
/// 全部走生产代码（CollectDataChamber / DataLoggerHelper / DataLogger），不碰 PLC 与全局单例。
/// </summary>
internal static class DataLoggingTestSupport
{
    internal const string SvBasePath = @"D:\CMP\Configuration\SV";

    internal static string SvPath(string fileName)
    {
        return Path.Combine(SvBasePath, fileName);
    }

    internal static List<SV> ReadConfiguredSvs(string csvPath)
    {
        using var file = new FileStream(csvPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(file);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var configured = new List<SV>();
        while (csv.Read())
        {
            if (csv.Parser.Row == 1)
            {
                continue;
            }

            configured.Add(new SV
            {
                SVID = csv.GetField<int>(0),
                Name = csv.GetField<string>(1),
                Unit = csv.GetField<string>(4),
                Comment = csv.GetField<string>(11)
            });
        }

        return configured;
    }

    /// <summary>取某个工位的 IO 段 SV 变量（SVID >= firstSvid 且名字以工位号开头）。</summary>
    internal static List<SV> ReadChamberSvs(string fileName, string chamberId, int firstSvid)
    {
        return ReadConfiguredSvs(SvPath(fileName))
            .Where(sv => sv.SVID >= firstSvid && sv.Name.StartsWith(chamberId, StringComparison.Ordinal))
            .ToList();
    }

    /// <summary>按名字（忽略首尾空格）取 SV 变量表里的一行。</summary>
    internal static SV SvFrom(string fileName, string name)
    {
        var trimmed = name.Trim();
        var match = ReadConfiguredSvs(SvPath(fileName))
            .FirstOrDefault(sv => string.Equals(sv.Name.Trim(), trimmed, StringComparison.Ordinal));
        Assert.IsNotNull(match, $"SV 变量表 {fileName} 里找不到变量“{name}”");
        return match!;
    }

    internal static Pen? CreatePenWithSv(string penName, string fileName)
    {
        var svPath = SvPath(fileName);
        if (!File.Exists(svPath))
        {
            Assert.Inconclusive($"SV 变量表不存在：{svPath}");
            return null;
        }

        var pen = new Pen(penName, new KVLink("127.0.0.1"));
        LoadSv(pen, svPath);
        return pen;
    }

    internal static Roller? CreateRollerWithSv(string rollerName, string fileName)
    {
        var svPath = SvPath(fileName);
        if (!File.Exists(svPath))
        {
            Assert.Inconclusive($"SV 变量表不存在：{svPath}");
            return null;
        }

        var roller = new Roller(rollerName, new KVLink("127.0.0.1"));
        LoadSv(roller, svPath);
        return roller;
    }

    internal static Polish? CreatePolishWithSv(string polishName, string fileName)
    {
        var svPath = SvPath(fileName);
        if (!File.Exists(svPath))
        {
            Assert.Inconclusive($"SV 变量表不存在：{svPath}");
            return null;
        }

        var polish = new Polish(polishName, new KVLink("127.0.0.1"));
        LoadSv(polish, svPath);
        return polish;
    }

    internal static Dry? CreateDryWithSv(string dryName, string fileName)
    {
        var svPath = SvPath(fileName);
        if (!File.Exists(svPath))
        {
            Assert.Inconclusive($"SV 变量表不存在：{svPath}");
            return null;
        }

        var dry = new Dry(dryName, new KVLink("127.0.0.1"));
        LoadSv(dry, svPath);
        return dry;
    }

    /// <summary>像 DataCenter.InitSVXxx 那样把 SV 变量表装进腔体。</summary>
    internal static void LoadSv(CollectDataChamber chamber, string svPath)
    {
        foreach (var sv in ReadConfiguredSvs(svPath))
        {
            chamber.ListSV[sv.Name] = sv;
        }
    }

    internal static string NewTempLogPath()
    {
        return Path.Combine(Path.GetTempPath(), $"CMP_DataLoggingTests_{Guid.NewGuid():N}");
    }

    /// <summary>与生成的 ByteToBool 同一套位约定：byteOffset = word*2 + bit/8, mask = 1 &lt;&lt; (bit%8)。</summary>
    internal static void SetBit(byte[] buffer, int word, int bit)
    {
        buffer[(word * 2) + (bit / 8)] |= (byte)(1 << (bit % 8));
    }

    internal static void AssertSvBool(CollectDataChamber chamber, string key, bool expected)
    {
        var sv = chamber.GetSv(key);
        Assert.IsNotNull(sv, $"采集端没有写入运行期键 {key}");
        Assert.AreEqual(expected, Convert.ToBoolean(sv.Value), $"运行期键 {key} 的值");
    }

    // ------------------------------------------------------------ 写一行 CSV

    internal static Dictionary<string, string?> WritePenRow(Pen pen, int penIndex, string chamberName, string waferName)
    {
        var basePath = NewTempLogPath();
        var dataLogger = new DataLogger(chamberName, basePath);
        var helper = new DataLoggerHelper(dataLogger, chamberName);
        dataLogger.AppendData(waferName, helper.BuildPenRow(pen, penIndex, "2026-01-01 00:00:00:000", "0.000"));
        return ReadSingleLogRow(basePath);
    }

    internal static Dictionary<string, string?> WriteRollerRow(Roller roller, int rollerIndex, string chamberName, string waferName)
    {
        var basePath = NewTempLogPath();
        var dataLogger = new DataLogger(chamberName, basePath);
        var helper = new DataLoggerHelper(dataLogger, chamberName);
        dataLogger.AppendData(waferName, helper.BuildRollerRow(roller, rollerIndex, "2026-01-01 00:00:00:000", "0.000"));
        return ReadSingleLogRow(basePath);
    }

    internal static Dictionary<string, string?> WritePolishRow(Polish polish, string chamberName, string waferName)
    {
        var basePath = NewTempLogPath();
        var dataLogger = new DataLogger(chamberName, basePath);
        var helper = new DataLoggerHelper(dataLogger, chamberName);
        dataLogger.AppendData(waferName, helper.BuildPolishRow(polish, "2026-01-01 00:00:00:000", "0.000"));
        return ReadSingleLogRow(basePath);
    }

    internal static Dictionary<string, string?> WriteDryRow(Dry dry, string chamberName, string waferName)
    {
        var basePath = NewTempLogPath();
        var dataLogger = new DataLogger(chamberName, basePath);
        var helper = new DataLoggerHelper(dataLogger, chamberName);
        dataLogger.AppendData(waferName, helper.BuildDryRow(dry, "2026-01-01 00:00:00:000", "0.000"));
        return ReadSingleLogRow(basePath);
    }

    // ------------------------------------------------------------ 读回 CSV

    internal sealed class CsvTable
    {
        public string[] Headers { get; init; } = [];

        public List<string[]> Rows { get; init; } = [];
    }

    internal static string SingleCsvFile(string basePath)
    {
        var files = Directory.GetFiles(basePath, "*.csv", SearchOption.AllDirectories);
        Assert.AreEqual(1, files.Length, "应该只写出一个 CSV 文件");
        return files[0];
    }

    internal static CsvTable ReadCsvTable(string basePath)
    {
        var file = SingleCsvFile(basePath);
        using var reader = new StreamReader(file);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord ?? [];
        var rows = new List<string[]>();
        while (csv.Read())
        {
            var row = new string[headers.Length];
            for (var index = 0; index < headers.Length; index++)
            {
                row[index] = csv.GetField(index) ?? string.Empty;
            }

            rows.Add(row);
        }

        return new CsvTable { Headers = headers, Rows = rows };
    }

    internal static Dictionary<string, string?> ReadSingleLogRow(string basePath)
    {
        var table = ReadCsvTable(basePath);
        Assert.AreEqual(1, table.Rows.Count, "应该只有一行数据");
        var row = new Dictionary<string, string?>(StringComparer.Ordinal);
        for (var index = 0; index < table.Headers.Length; index++)
        {
            row[table.Headers[index]] = table.Rows[0][index];
        }

        return row;
    }

    /// <summary>取某行某列的值；列不存在返回 null。</summary>
    internal static string? Cell(CsvTable table, int rowIndex, SV sv)
    {
        var column = Array.IndexOf(table.Headers, DataLogFormatting.FormatSvHeader(sv));
        Assert.IsTrue(rowIndex >= 0 && rowIndex < table.Rows.Count, $"没有第 {rowIndex} 行");
        return column < 0 ? null : table.Rows[rowIndex][column];
    }

    /// <summary>
    /// 断言 CSV 里存在该 SV 变量对应的列（表头就是 DataLogFormatting.FormatSvHeader 的结果），并校验值。
    /// 不用子串匹配，避免 “Upper Roller Cylinder Up” 误配到 “Upper Roller Cylinder Up Control”。
    /// </summary>
    internal static void AssertCell(IReadOnlyDictionary<string, string?> row, SV sv, string expected)
    {
        var header = DataLogFormatting.FormatSvHeader(sv);
        Assert.IsTrue(
            row.ContainsKey(header),
            $"CSV 里没有变量“{sv.Name}”的列（期望表头“{header}”）。实际表头：{string.Join(" | ", row.Keys)}");
        Assert.AreEqual(expected, row[header], $"列 {header} 的值");
    }

    internal static void AssertCell(CsvTable table, int rowIndex, SV sv, string expected)
    {
        var header = DataLogFormatting.FormatSvHeader(sv);
        var column = Array.IndexOf(table.Headers, header);
        Assert.IsTrue(
            column >= 0,
            $"CSV 里没有变量“{sv.Name}”的列（期望表头“{header}”）。实际表头：{string.Join(" | ", table.Headers)}");
        Assert.AreEqual(expected, table.Rows[rowIndex][column], $"第 {rowIndex} 行列 {header} 的值");
    }
}
