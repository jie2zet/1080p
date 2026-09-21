#nullable enable

using System.Globalization;
using System.IO;
using System;
using System.Collections.Generic;
using CMP.Models.DataCenter;
using CMP.Models.kvsPlc;
using CMP.Models.Resource;
using CMP.Services.Log;
using CMP.Validation;
using CsvHelper;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CMP.Tests.Services;

[TestClass]
public class DataLogFormattingTests
{
    [TestMethod]
    public void TryBuildWaferLogName_UsesWaferNameAndOneBasedWaferId()
    {
        var wafer = new Wafer
        {
            WaferName = "LotA_2",
            LoadportID = 0,
            SlotID = 1
        };

        Assert.IsTrue(DataLogFormatting.TryBuildWaferLogName(wafer, out var waferLogName));
        Assert.AreEqual("LotA_2(C1W2)", waferLogName);

        wafer.WaferName = string.Empty;
        Assert.IsTrue(DataLogFormatting.TryBuildWaferLogName(wafer, out var waferIdOnlyLogName));
        Assert.AreEqual("(C1W2)", waferIdOnlyLogName);

        Assert.IsFalse(DataLogFormatting.TryBuildWaferLogName(null, out var unknownWaferLogName));
        Assert.AreEqual("Unknown(Unknown)", unknownWaferLogName);
    }

    [TestMethod]
    public void FormatSvHeader_UsesExactCommentNameAndUnitShape()
    {
        Assert.AreEqual(
            "FLC-POLA100_Slurry1 Or DIW Flowrate Act(mL/min)",
            DataLogFormatting.FormatSvHeader(new SV
            {
                Comment = "FLC-POLA100",
                Name = "Slurry1 Or DIW Flowrate Act",
                Unit = "mL/min"
            }));
        Assert.AreEqual(
            "_Recipe Time Act(s)",
            DataLogFormatting.FormatSvHeader(new SV { Name = "Recipe Time Act", Unit = "s" }));
        Assert.AreEqual(
            "Comment_Process Step Act()",
            DataLogFormatting.FormatSvHeader(new SV { Comment = "Comment", Name = "Process Step Act" }));
        Assert.AreEqual("_", DataLogFormatting.FormatSvHeader(null));
    }

    [TestMethod]
    public void ResolveSvMetadata_MatchesAllChamberNamingPatternsFromMemory()
    {
        var polish = new SV { SVID = 10004, Name = "Head Z-Axis Speed Act" };
        var roller = new SV { SVID = 12000, Name = "RollerA1 Process Step Act" };
        var pen = new SV { SVID = 15000, Name = "PenA1 Process Step Act" };
        var dry = new SV { SVID = 17000, Name = "DryA Process Step Act" };
        var index = DataLogFormatting.BuildMetadataIndex([polish, roller, pen, dry]);

        Assert.AreSame(polish, DataLogFormatting.ResolveSvMetadata(index, "PolishAHeadZ_AxisSpeedAct", "PolishA"));
        Assert.AreSame(roller, DataLogFormatting.ResolveSvMetadata(index, "RollerA_1ProcessStepAct", "RollerA1"));
        Assert.AreSame(pen, DataLogFormatting.ResolveSvMetadata(index, "PenA_1ProcessStepAct", "PenA1"));
        Assert.AreSame(dry, DataLogFormatting.ResolveSvMetadata(index, "DryAProcessStepAct", "DryA"));
        Assert.IsNull(DataLogFormatting.ResolveSvMetadata(index, "DryAUnconfiguredField", "DryA"));
    }

    [TestMethod]
    public void Polish_InitializesRuntimeKeysFromConfiguredValues()
    {
        var polish = new Polish("PolishD", new KVLink("127.0.0.1"));
        var configured = new SV
        {
            SVID = 11500,
            Name = "Process Step Act",
            Value = "0"
        };
        polish.ListSV[configured.Name] = configured;

        //polish.InitializeRuntimeValues();

        Assert.AreSame(configured, polish.GetSv("PolishDProcess Step Act"));
        Assert.AreEqual("0", polish.GetSv("PolishDProcess Step Act")?.Value);
    }

    [TestMethod]
    public void RollerCsv_ResolvesEveryCurrentRuntimeKeyForAllRollerIndexes()
    {
        const string svBasePath = @"D:\CMP\Configuration\SV";
        foreach (var side in new[] { "A", "B" })
        {
            var csvPath = Path.Combine(svBasePath, $"SV_Roller{side}.csv");
            if (!File.Exists(csvPath))
                Assert.Inconclusive($"SV file is not available: {csvPath}");

            var roller = new Roller($"Roller{side}", new KVLink("127.0.0.1"));
            using var file = new FileStream(csvPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(file);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var configured = new List<SV>();
            while (csv.Read())
            {
                if (csv.Parser.Row == 1)
                    continue;

                configured.Add(new SV
                {
                    SVID = csv.GetField<int>(0),
                    Name = csv.GetField<string>(1),
                    Unit = csv.GetField<string>(4),
                    Comment = csv.GetField<string>(11)
                });
            }

            foreach (var item in configured)
                roller.ListSV[item.Name] = item;

            var metadata = DataLogFormatting.BuildMetadataIndex(configured);
            foreach (var index in Enumerable.Range(1, 3))
            {
                var prefix = $"_{index}";
                var runtimeKeys = roller.collectDataFields
                    .Where(field => !field.Name.Contains("Void") && field.Name.StartsWith(prefix, StringComparison.Ordinal))
                    .Select(field => roller.BuildRuntimeKey(null, field.Name))
                    .Concat(roller.ioDataFields
                        .Where(field => !field.Name.Contains("Void") && !CollectDataChamber.IsPaddingField(field.Name))
                        .Select(field => roller.BuildRuntimeKey(prefix, field.Name)))
                    .ToList();

                Assert.AreEqual(109, runtimeKeys.Count, $"Roller{side}{index} runtime field count");
                Assert.IsTrue(runtimeKeys.All(key =>
                    DataLogFormatting.ResolveSvMetadata(metadata, key, $"Roller{side}{index}") != null),
                    $"At least one Roller{side}{index} runtime key has no matching SV row.");

                foreach (var key in runtimeKeys)
                {
                    var sv = DataLogFormatting.ResolveSvMetadata(metadata, key, $"Roller{side}{index}");
                    Assert.AreNotEqual("_", DataLogFormatting.FormatSvHeader(sv), key);
                }
            }
        }
    }

    [TestMethod]
    public void WaferNameValidationRule_RejectsEveryInvalidFileNameCharacter()
    {
        var rule = new WaferNameValidationRule();
        Assert.IsTrue(rule.Validate("LotA_2", CultureInfo.InvariantCulture).IsValid);
        Assert.IsTrue(rule.Validate(string.Empty, CultureInfo.InvariantCulture).IsValid);

        foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
        {
            Assert.IsFalse(
                rule.Validate($"Lot{invalidCharacter}A", CultureInfo.InvariantCulture).IsValid,
                $"Expected character code {(int)invalidCharacter} to be rejected.");
        }
    }

    /// <summary>
    /// 覆盖率检测：SV 变量表必须能解析出日志实际会写出的每一个运行期键。
    /// 解析不到的键对应的列会被 DataLoggerHelper.AddSvField 静默丢弃，即 CSV 日志少列。
    /// </summary>
    [TestMethod]
    public void PolishCsv_ResolvesEveryCurrentRuntimeKeyForAllPolishIndexes()
    {
        const string svBasePath = @"D:\CMP\Configuration\SV";
        foreach (var side in new[] { "A", "B", "C", "D" })
        {
            var csvPath = Path.Combine(svBasePath, $"SV_Polish{side}.csv");
            if (!File.Exists(csvPath))
                Assert.Inconclusive($"SV file is not available: {csvPath}");

            var polish = new Polish($"Polish{side}", new KVLink("127.0.0.1"));
            var metadata = DataLogFormatting.BuildMetadataIndex(ReadConfiguredSvs(csvPath));

            var runtimeKeys = polish.collectDataFields
                .Where(field => !field.Name.Contains("Void") && !field.Name.Contains("Exsitu"))
                .Select(field => polish.BuildRuntimeKey(null, field.Name))
                .Concat(polish.ioDataFields
                    .Where(field => !field.Name.Contains("Void"))
                    .Select(field => polish.BuildRuntimeKey(null, field.Name)))
                .ToList();

            AssertNoUnresolvedRuntimeKeys(metadata, polish.polishName, runtimeKeys, $"Polish{side}");
        }
    }

    [TestMethod]
    public void PenCsv_ResolvesEveryCurrentRuntimeKeyForAllPenIndexes()
    {
        const string svBasePath = @"D:\CMP\Configuration\SV";
        foreach (var side in new[] { "A", "B" })
        {
            var csvPath = Path.Combine(svBasePath, $"SV_Pen{side}.csv");
            if (!File.Exists(csvPath))
                Assert.Inconclusive($"SV file is not available: {csvPath}");

            var pen = new Pen($"Pen{side}", new KVLink("127.0.0.1"));
            var metadata = DataLogFormatting.BuildMetadataIndex(ReadConfiguredSvs(csvPath));

            foreach (var (suffix, keyPrefix) in new[] { ("1", "_1"), ("2", "_2") })
            {
                var runtimeKeys = pen.collectDataFields
                    .Where(field => !field.Name.Contains("Void")
                        && field.Name.StartsWith(keyPrefix, StringComparison.Ordinal))
                    .Select(field => pen.BuildRuntimeKey(null, field.Name))
                    .Concat(pen.ioDataFields
                        .Where(field => !field.Name.Contains("Void")
                            && !CollectDataChamber.IsPaddingField(field.Name))
                        .Select(field => pen.BuildRuntimeKey(keyPrefix, field.Name)))
                    .ToList();

                // 55 个过程量 + 41 个 IO（KHSTPenIOData 是单支笔结构体，工位号在键里）
                Assert.AreEqual(96, runtimeKeys.Count, $"Pen{side}{suffix} runtime field count");
                AssertNoUnresolvedRuntimeKeys(metadata, $"Pen{side}{suffix}", runtimeKeys, $"Pen{side}{suffix}");
            }
        }
    }

    [TestMethod]
    public void DryCsv_ResolvesEveryCurrentRuntimeKeyForAllDryIndexes()
    {
        const string svBasePath = @"D:\CMP\Configuration\SV";
        foreach (var side in new[] { "A", "B" })
        {
            var csvPath = Path.Combine(svBasePath, $"SV_Dry{side}.csv");
            if (!File.Exists(csvPath))
                Assert.Inconclusive($"SV file is not available: {csvPath}");

            var dry = new Dry($"Dry{side}", new KVLink("127.0.0.1"));
            var metadata = DataLogFormatting.BuildMetadataIndex(ReadConfiguredSvs(csvPath));

            var runtimeKeys = dry.collectDataFields
                .Where(field => !field.Name.Contains("Void"))
                .Select(field => dry.BuildRuntimeKey(null, field.Name))
                .Concat(dry.ioDataFields
                    .Where(field => !field.Name.Contains("Void"))
                    .Select(field => dry.BuildRuntimeKey(null, field.Name)))
                .ToList();

            AssertNoUnresolvedRuntimeKeys(metadata, dry.dryName, runtimeKeys, $"Dry{side}");
        }
    }

    private static List<SV> ReadConfiguredSvs(string csvPath)
    {
        using var file = new FileStream(csvPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(file);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var configured = new List<SV>();
        while (csv.Read())
        {
            if (csv.Parser.Row == 1)
                continue;

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

    private static void AssertNoUnresolvedRuntimeKeys(
        IReadOnlyDictionary<string, SV> metadata,
        string chamberName,
        List<string> runtimeKeys,
        string scope)
    {
        var unresolved = runtimeKeys
            .Where(key => DataLogFormatting.ResolveSvMetadata(metadata, key, chamberName) == null)
            .ToList();

        Assert.AreEqual(
            0,
            unresolved.Count,
            $"{scope}: {unresolved.Count} of {runtimeKeys.Count} runtime keys have no SV row and would be dropped from the CSV log: {string.Join(", ", unresolved)}");
    }
}
