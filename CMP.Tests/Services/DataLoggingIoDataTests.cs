#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CMP.Models.DataCenter;
using CMP.Models.kvsPlc;
using CMP.Models.Resource;
using static CMP.Tests.Services.DataLoggingTestSupport;
using CMP.Services.Log;
using CsvHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using stDryCollectData = CMP.Models.kvsPlc.SV.KvStruct.stDryCollectData;
using stPenCollectData = CMP.Models.kvsPlc.Pen.KvsStructPenCollectData.stPenCollectData;
using stPolishCollectData = CMP.Models.kvsPlc.SV.KvStruct.stPolishCollectData;
using stRollerCollectData = CMP.Models.kvsPlc.Roll.KvsStructRollerCollectData.stRollerCollectData;

namespace CMP.Tests.Services;

/// <summary>
/// 数据日志 IO 段的端到端验证。
///
/// 这里不经过 PLC/socket：直接把一段“PLC 结构体字节流”喂给采集端
/// （<see cref="Roller.ApplyIoData"/> / <see cref="Pen.ApplyIoData"/>，
/// 就是 CollectIOData 读变量后调用的同一个方法），
/// 然后用真实的日志代码（DataLoggerHelper.BuildXRollerRow / BuildPenRow + DataLogger）
/// 写出 CSV，检查：
///   1. SetSv 写的键与日志查找的键是否为同一个（列有没有值）；
///   2. SV 变量表里的每个 IO 变量是否都能在 CSV 里出现（列有没有少）；
///   3. 位序是否以拆分后的新结构体为准。
/// </summary>
[TestClass]
public class DataLoggingIoDataTests
{
    private const string SvBasePath = @"D:\CMP\Configuration\SV";

    // ---------------------------------------------------------------- Pen

    [TestMethod]
    public void Pen_IoDataBuffer_UsesTheSameKeyAsDataLogging_AndTheNewBitLayout()
    {
        var pen = CreatePenWithSv("PenA", "SV_PenA.csv");
        if (pen == null) return;

        var buffer = new byte[64];
        // 位序取自生成的 KvsStructPenIOData.ByteTo_stPenIOData（新单笔结构体）：
        //   WFClampPositionSensor   = word0 bit2
        //   ArmUpPositionSensor     = word0 bit4
        //   PlateDIWWashValveControl= word4 bit8
        //   WFLowerFacePurgeN2Control = word6 bit3
        SetBit(buffer, word: 0, bit: 2);
        SetBit(buffer, word: 0, bit: 4);
        SetBit(buffer, word: 4, bit: 8);
        SetBit(buffer, word: 6, bit: 3);

        pen.ApplyIoData(buffer, penIndex: 1);

        // 1) 采集端 SetSv 后的键，就是日志端查找的键（PenA1 -> "_1"）
        AssertSvBool(pen, "PenA_1WFClampPositionSensor", true);
        AssertSvBool(pen, "PenA_1ArmUpPositionSensor", true);
        AssertSvBool(pen, "PenA_1PlateDIWWashValveControl", true);
        AssertSvBool(pen, "PenA_1WFLowerFacePurgeN2Control", true);

        // 旧共享结构体（KHSTIO.stPenIOData）里 word0 bit2/bit4 是 ArmUp/WFClamp，与新结构体正好相反。
        // 位序以新结构体为准：bit2 只应点亮 WF Clamp，bit4 只应点亮 Arm Up。
        AssertSvBool(pen, "PenA_1WFReleasePositonSensor", false);
        AssertSvBool(pen, "PenA_1ArmDownPosionSensor", false);
        AssertSvBool(pen, "PenA_1PenUpPositionSensor", false);

        var row = WritePenRow(pen, penIndex: 0, chamberName: "PenA1", waferName: "(C1W1)_PenBitLayout");

        AssertCell(row, SvFrom("SV_PenA.csv", "PenA1 WF Clamp Position Sensor"), "True");
        AssertCell(row, SvFrom("SV_PenA.csv", "PenA1 Arm Up Position Sensor"), "True");
        AssertCell(row, SvFrom("SV_PenA.csv", "PenA1 WF Release Positon Sensor"), "False");
        AssertCell(row, SvFrom("SV_PenA.csv", "PenA1 Arm Down Posion Sensor"), "False");
        AssertCell(row, SvFrom("SV_PenA.csv", "PenA1 Plate DIW Wash Valve Control"), "True");
        AssertCell(row, SvFrom("SV_PenA.csv", "PenA1 WF Lower Face Purge N2 Control"), "True");
    }

    [TestMethod]
    public void Pen_EveryConfiguredIoVariable_IsWrittenToTheCsvWithAValue()
    {
        var pen = CreatePenWithSv("PenA", "SV_PenA.csv");
        if (pen == null) return;

        var buffer = new byte[64];
        Array.Fill(buffer, (byte)0xFF);
        pen.ApplyIoData(buffer, penIndex: 1);

        var row = WritePenRow(pen, penIndex: 0, chamberName: "PenA1", waferName: "(C1W1)_PenAllIo");

        var ioVariables = ReadChamberSvs("SV_PenA.csv", "PenA1", firstSvid: 15300);
        Assert.AreEqual(41, ioVariables.Count, "SV_PenA.csv 里 PenA1 的 IO 变量个数（15600 段之前的 153xx 段）");

        foreach (var sv in ioVariables)
        {
            AssertCell(row, sv, "True");
        }
    }

    // ------------------------------------------------------------- Roller

    [TestMethod]
    public void Roller_IoDataBuffer_UsesTheSameKeyAsDataLogging_AndSkipsPaddingFields()
    {
        var roller = CreateRollerWithSv("RollerA", "SV_RollerA.csv");
        if (roller == null) return;

        var buffer = new byte[64];
        // 位序取自生成的 KvsStructRollerIOData.ByteTo_stRollerIOData：
        //   LowerRollerCylinderUpPositionSensor = word0 bit0
        //   LowerRollerCylinderDownPositionSensor = word0 bit1
        //   Member3[0]                          = word2 bit0（填充位，不该出现在日志里）
        //   CoolingWaterValveControl            = word8 bit0
        SetBit(buffer, word: 0, bit: 0);
        SetBit(buffer, word: 2, bit: 0);
        SetBit(buffer, word: 8, bit: 0);

        roller.ApplyIoData(buffer, rollerIndex: 2);

        AssertSvBool(roller, "RollerA_2LowerRollerCylinderUpPositionSensor", true);
        AssertSvBool(roller, "RollerA_2CoolingWaterValveControl", true);
        AssertSvBool(roller, "RollerA_2LowerRollerCylinderDownPositionSensor", false);
        // 填充位不写 SV
        Assert.IsNull(roller.GetSv("RollerA_2Member3"), "Member*/填充字段不应写入 SV");

        var row = WriteRollerRow(roller, rollerIndex: 1, chamberName: "RollerA2", waferName: "(C3W3)_RollerBitLayout");

        AssertCell(row, SvFrom("SV_RollerA.csv", "RollerA2 Lower Roller Cylinder Up Position Sensor"), "True");
        AssertCell(row, SvFrom("SV_RollerA.csv", "RollerA2 Cooling Water Valve Control"), "True");
        AssertCell(row, SvFrom("SV_RollerA.csv", "RollerA2 Lower Roller Cylinder Down Position Sensor"), "False");
        Assert.IsFalse(row.Keys.Any(h => h.Contains("Member", StringComparison.Ordinal)), "CSV 里出现了 Member* 填充列");
    }

    [TestMethod]
    public void Roller_EveryConfiguredIoVariable_IsWrittenToTheCsvWithAValue()
    {
        var roller = CreateRollerWithSv("RollerA", "SV_RollerA.csv");
        if (roller == null) return;

        var buffer = new byte[64];
        Array.Fill(buffer, (byte)0xFF);
        roller.ApplyIoData(buffer, rollerIndex: 2);

        var row = WriteRollerRow(roller, rollerIndex: 1, chamberName: "RollerA2", waferName: "(C3W3)_RollerAllIo");

        var ioVariables = ReadChamberSvs("SV_RollerA.csv", "RollerA2", firstSvid: 12800);
        Assert.AreEqual(52, ioVariables.Count, "SV_RollerA.csv 里 RollerA2 的 IO 变量个数");

        foreach (var sv in ioVariables)
        {
            AssertCell(row, sv, "True");
        }
    }

    // ------------------------------------------------- SV <-> 运行期键双向覆盖

    /// <summary>
    /// 覆盖检测（两个方向都测）：
    ///   正向：日志会写出的每个运行期键，都必须能在 SV 变量表里解析出变量，否则整列被丢掉；
    ///   反向：SV 变量表里属于该工位的每个变量，都必须能被日志写出，否则数据永远不落盘。
    /// 之前的 bug 就是这两个方向各错一半（Roller 少了前缀、Pen 多了一层前缀 + 结构体没换）。
    /// </summary>
    [TestMethod]
    public void LoggerKeys_AndConfiguredSvVariables_CoverEachOther_ForEveryStation()
    {
        foreach (var (chamberName, stationCount, fileName) in new[]
                 {
                     ("RollerA", 3, "SV_RollerA.csv"),
                     ("RollerB", 3, "SV_RollerB.csv"),
                     ("PenA", 2, "SV_PenA.csv"),
                     ("PenB", 2, "SV_PenB.csv")
                 })
        {
            var svPath = SvPath(fileName);
            if (!File.Exists(svPath))
            {
                Assert.Inconclusive($"SV 变量表不存在：{svPath}");
            }

            var configured = ReadConfiguredSvs(svPath);
            var metadata = DataLogFormatting.BuildMetadataIndex(configured);

            CollectDataChamber chamber;
            FieldInfo[] collectDataFields;
            FieldInfo[] ioDataFields;
            if (chamberName.StartsWith("Roller", StringComparison.Ordinal))
            {
                var roller = new Roller(chamberName, new KVLink("127.0.0.1"));
                chamber = roller;
                collectDataFields = roller.collectDataFields;
                ioDataFields = roller.ioDataFields;
            }
            else
            {
                var pen = new Pen(chamberName, new KVLink("127.0.0.1"));
                chamber = pen;
                collectDataFields = pen.collectDataFields;
                ioDataFields = pen.ioDataFields;
            }

            foreach (var sv in configured)
            {
                chamber.ListSV[sv.Name] = sv;
            }

            for (var station = 1; station <= stationCount; station++)
            {
                var chamberId = $"{chamberName}{station}";
                var keyPrefix = $"_{station}";

                var loggerKeys = collectDataFields
                    .Where(field => !field.Name.Contains("Void") && field.Name.StartsWith(keyPrefix, StringComparison.Ordinal))
                    .Select(field => chamber.BuildRuntimeKey(null, field.Name))
                    .Concat(ioDataFields
                        .Where(field => !field.Name.Contains("Void") && !CollectDataChamber.IsPaddingField(field.Name))
                        .Select(field => chamber.BuildRuntimeKey(keyPrefix, field.Name)))
                    .ToList();

                var unresolved = loggerKeys
                    .Where(key => DataLogFormatting.ResolveSvMetadata(metadata, key, chamberId) == null)
                    .ToList();
                Assert.AreEqual(
                    0,
                    unresolved.Count,
                    $"{chamberId}: 这些运行期键在 SV 表里没有对应变量，列会被丢掉：{string.Join(" | ", unresolved)}");

                var loggerKeySet = loggerKeys
                    .Select(DataLogFormatting.NormalizeSvKey)
                    .ToHashSet(StringComparer.Ordinal);
                var normalizedChamberId = DataLogFormatting.NormalizeSvKey(chamberId);
                var uncovered = configured
                    .Where(sv => DataLogFormatting.NormalizeSvKey(sv.Name).StartsWith(normalizedChamberId, StringComparison.Ordinal))
                    .Where(sv => !loggerKeySet.Contains(DataLogFormatting.NormalizeSvKey(sv.Name)))
                    .Select(sv => $"{sv.SVID} {sv.Name}")
                    .ToList();
                Assert.AreEqual(
                    0,
                    uncovered.Count,
                    $"{chamberId}: SV 表里有这些变量但日志不会写出对应列：{string.Join(" | ", uncovered)}");
            }
        }
    }

    // ----------------------------------------------------- Motor Servo（M 位）

    /// <summary>
    /// M534/M535 读到的辊轮伺服 ON 位必须写到 CSV 真正会读的那一列。
    /// 旧代码写的是 "RollerA2 Left Pillar Motor Servo"（SV 表里叫 "RollerA2 WF Left Pillar Motor ServoOn"），
    /// 属于没人读的死键，CSV 里那列只能靠采集结构体里的值。
    /// </summary>
    [TestMethod]
    public void Roller_MotorServoBits_ReachTheCsvColumns()
    {
        var roller = CreateRollerWithSv("RollerA", "SV_RollerA.csv");
        if (roller == null) return;

        // M534 bit0..3 = RollerA1；bit8..11 = RollerA2；M535 bit0..3 = RollerA3
        // 位顺序：0 = WF 左柱、1 = WF 右柱、2 = 上辊、3 = 下辊
        roller.ApplyMotorServoStatus([true, false, true, false], "_2");

        AssertSvBool(roller, roller.BuildRuntimeKey(null, nameof(stRollerCollectData._2WFLeftPillarMotorServoOn)), true);
        AssertSvBool(roller, roller.BuildRuntimeKey(null, nameof(stRollerCollectData._2WFRightPillarMotorServoOn)), false);
        AssertSvBool(roller, roller.BuildRuntimeKey(null, nameof(stRollerCollectData._2UpperRollerMotorServoOn)), true);
        AssertSvBool(roller, roller.BuildRuntimeKey(null, nameof(stRollerCollectData._2LowerRollerMotorServoOn)), false);

        // 这些键必须就是日志会遍历的字段（否则列不会出现）
        foreach (var fieldName in new[]
                 {
                     nameof(stRollerCollectData._2WFLeftPillarMotorServoOn),
                     nameof(stRollerCollectData._2WFRightPillarMotorServoOn),
                     nameof(stRollerCollectData._2UpperRollerMotorServoOn),
                     nameof(stRollerCollectData._2LowerRollerMotorServoOn)
                 })
        {
            Assert.IsTrue(roller.collectDataFields.Any(f => f.Name == fieldName), $"采集结构体里没有字段 {fieldName}");
        }

        var row = WriteRollerRow(roller, rollerIndex: 1, chamberName: "RollerA2", waferName: "(C3W3)_Servo");
        AssertCell(row, SvFrom("SV_RollerA.csv", "RollerA2 WF Left Pillar Motor ServoOn"), "True");
        AssertCell(row, SvFrom("SV_RollerA.csv", "RollerA2 WF Right Pillar Motor ServoOn"), "False");
        AssertCell(row, SvFrom("SV_RollerA.csv", "RollerA2 Upper Roller Motor ServoOn"), "True");
        AssertCell(row, SvFrom("SV_RollerA.csv", "RollerA2 Lower Roller Motor ServoOn"), "False");
    }

    /// <summary>M537 读到的笔刷伺服 ON 位必须出现在 “PenA1 … Motor Servo On” 列里。</summary>
    [TestMethod]
    public void Pen_MotorServoBits_ReachTheCsvColumns()
    {
        var pen = CreatePenWithSv("PenA", "SV_PenA.csv");
        if (pen == null) return;

        // M537 bit0..2 = PenA1；bit8..10 = PenA2；位顺序 0 = Chuck、1 = Arm Sweep、2 = Rotation
        pen.ApplyMotorServoStatus([false, true, false], "_1");

        AssertSvBool(pen, pen.BuildRuntimeKey(null, nameof(stPenCollectData._1ChuckMotorServoOn)), false);
        AssertSvBool(pen, pen.BuildRuntimeKey(null, nameof(stPenCollectData._1PenArmSweepMotorServoOn)), true);
        AssertSvBool(pen, pen.BuildRuntimeKey(null, nameof(stPenCollectData._1PenRotationMotorServoOn)), false);

        var row = WritePenRow(pen, penIndex: 0, chamberName: "PenA1", waferName: "(C1W1)_Servo");
        AssertCell(row, SvFrom("SV_PenA.csv", "PenA1 Chuck Motor Servo On"), "False");
        AssertCell(row, SvFrom("SV_PenA.csv", "PenA1 Pen Arm Sweep Motor Servo On"), "True");
        AssertCell(row, SvFrom("SV_PenA.csv", "PenA1 Pen Rotation Motor Servo On"), "False");
    }

    /// <summary>M530 读到的研磨伺服 ON 位必须出现在 “Platen/Head … Motor Servo On” 列里。</summary>
    [TestMethod]
    public void Polish_MotorServoBits_ReachTheCsvColumns()
    {
        var polish = CreatePolishWithSv("PolishA", "SV_PolishA.csv");
        if (polish == null) return;

        polish.ApplyMotorServoStatus([true, true, false, false, true, false, true]);

        AssertSvBool(polish, polish.BuildRuntimeKey(null, nameof(stPolishCollectData.PlatenRotationMotorServoOn)), true);
        AssertSvBool(polish, polish.BuildRuntimeKey(null, nameof(stPolishCollectData.HeadSweepMotorServoOn)), false);
        AssertSvBool(polish, polish.BuildRuntimeKey(null, nameof(stPolishCollectData.SlurrySweepMotorServoOn)), true);

        var row = WritePolishRow(polish, chamberName: "PolishA", waferName: "(C1W1)_Servo");
        AssertCell(row, SvFrom("SV_PolishA.csv", "Platen Rotation Motor Servo On"), "True");
        AssertCell(row, SvFrom("SV_PolishA.csv", "Head Sweep Motor Servo On"), "False");
        AssertCell(row, SvFrom("SV_PolishA.csv", "Slurry Sweep Motor Servo On"), "True");
    }

    /// <summary>M539 读到的 Dry 伺服 ON 位必须出现在 “DryA … Motor Servo On” 列里。</summary>
    [TestMethod]
    public void Dry_MotorServoBits_ReachTheCsvColumns()
    {
        var dry = CreateDryWithSv("DryA", "SV_DryA.csv");
        if (dry == null) return;

        dry.ApplyMotorServoStatus([true, false]);

        AssertSvBool(dry, dry.BuildRuntimeKey(null, nameof(stDryCollectData.ChuckMotorServoOn)), true);
        AssertSvBool(dry, dry.BuildRuntimeKey(null, nameof(stDryCollectData.ArmSweepMotorServoOn)), false);

        var row = WriteDryRow(dry, chamberName: "DryA", waferName: "(C1W1)_Servo");
        AssertCell(row, SvFrom("SV_DryA.csv", "DryA Chuck Motor Servo On"), "True");
        AssertCell(row, SvFrom("SV_DryA.csv", "DryA Arm Sweep Motor Servo On"), "False");
    }

    /// <summary>
    /// 结构体变更守护：<see cref="CollectDataChamber.MappedMotorServoFieldNames"/> 必须与采集结构体里
    /// 所有 …ServoOn 字段**一一对应**。
    ///   - 结构体“改名” → nameof 编译失败（这里不用管）；
    ///   - 结构体“新增/删除一个 …ServoOn 字段” → 本用例失败，提示必须同步更新映射表，
    ///     否则新位不会被记录、或位序整体错位。
    /// </summary>
    [TestMethod]
    public void MotorServoMapping_CoversExactlyTheServoFieldsOfTheStruct()
    {
        var chambers = new (string Label, CollectDataChamber Chamber)[]
        {
            ("PolishA", CreatePolishWithSv("PolishA", "SV_PolishA.csv")),
            ("RollerA", CreateRollerWithSv("RollerA", "SV_RollerA.csv")),
            ("PenA", CreatePenWithSv("PenA", "SV_PenA.csv")),
            ("DryA", CreateDryWithSv("DryA", "SV_DryA.csv"))
        };

        foreach (var (label, chamber) in chambers)
        {
            var structServoFields = chamber.collectDataFields
                .Select(field => field.Name)
                .Where(name => name.EndsWith("MotorServoOn", StringComparison.Ordinal))
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToList();
            var mapped = chamber.MappedMotorServoFieldNames().OrderBy(name => name, StringComparer.Ordinal).ToList();

            CollectionAssert.AreEqual(
                structServoFields,
                mapped,
                $"{label}: 结构体里的 …ServoOn 字段与伺服位映射表不一致（结构体新增/改名后必须同步更新 MotorServoSuffixes/StationPrefixes）");
            Assert.AreEqual(0, chamber.FindUnmappedMotorServoFields().Count, $"{label}: 有未映射的伺服字段");
        }
    }

    /// <summary>位顺序规范：同一工位内第 N 位必须对应映射表里第 N 个字段（也就是对应的 SV 变量）。</summary>
    [TestMethod]
    public void MotorServoBitOrder_MatchesTheDocumentedSignalOrder()
    {
        var roller = CreateRollerWithSv("RollerA", "SV_RollerA.csv");
        var pen = CreatePenWithSv("PenA", "SV_PenA.csv");
        var polish = CreatePolishWithSv("PolishA", "SV_PolishA.csv");
        var dry = CreateDryWithSv("DryA", "SV_DryA.csv");

        CollectionAssert.AreEqual(
            new[] { "WFLeftPillarMotorServoOn", "WFRightPillarMotorServoOn", "UpperRollerMotorServoOn", "LowerRollerMotorServoOn" },
            roller.MotorServoSuffixes.ToArray(),
            "Roller 位顺序");
        CollectionAssert.AreEqual(
            new[] { "ChuckMotorServoOn", "PenArmSweepMotorServoOn", "PenRotationMotorServoOn" },
            pen.MotorServoSuffixes.ToArray(),
            "Pen 位顺序");
        CollectionAssert.AreEqual(
            new[]
            {
                "PlatenRotationMotorServoOn", "HeadRotationMotorServoOn", "HeadSweepMotorServoOn", "HeadUpDownMotorServoOn",
                "ConditionHeadRotationMotorServoOn", "ConditionSweepMotorServoOn", "SlurrySweepMotorServoOn"
            },
            polish.MotorServoSuffixes.ToArray(),
            "Polish 位顺序");
        CollectionAssert.AreEqual(
            new[] { "ChuckMotorServoOn", "ArmSweepMotorServoOn" },
            dry.MotorServoSuffixes.ToArray(),
            "Dry 位顺序");
    }

    // ------------------------------------------------------------ 测试基建

    /// <summary>
    /// 回归保护：把键的拼装收敛到 <see cref="CollectDataChamber.BuildRuntimeKey"/> 是行为等价的，
    /// 没有改变任何一个既有运行期键（Dry/Polish 的失败用例与此无关）。
    /// </summary>
    [TestMethod]
    public void BuildRuntimeKey_IsIdenticalToThePreviousStringConcatenation()
    {
        var polish = new Polish("PolishA", new KVLink("127.0.0.1"));
        var roller = new Roller("RollerA", new KVLink("127.0.0.1"));
        var pen = new Pen("PenA", new KVLink("127.0.0.1"));
        var dry = new Dry("DryA", new KVLink("127.0.0.1"));

        Assert.AreEqual(polish.polishName + "Zone4AtmosphericValveControl", polish.BuildRuntimeKey(null, "Zone4AtmosphericValveControl"));
        Assert.AreEqual(dry.dryName + "WaferOutletDoorOpenControl_Copy1", dry.BuildRuntimeKey(null, "WaferOutletDoorOpenControl_Copy1"));
        Assert.AreEqual(roller.rollerName + "_2ProcessStepAct", roller.BuildRuntimeKey(null, "_2ProcessStepAct"));
        Assert.AreEqual(roller.rollerName + "_2CoolingWaterValveControl", roller.BuildRuntimeKey("_2", "CoolingWaterValveControl"));
        Assert.AreEqual(pen.penName + "_1PenUpPositionSensor", pen.BuildRuntimeKey("_1", "PenUpPositionSensor"));
    }

}
