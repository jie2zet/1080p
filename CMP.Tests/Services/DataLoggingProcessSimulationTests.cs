#nullable enable

using System;
using System.IO;
using System.Linq;
using CMP.Models.DataCenter;
using CMP.Models.Resource;
using CMP.Services.Log;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using stDryCollectData = CMP.Models.kvsPlc.SV.KvStruct.stDryCollectData;
using stRollerCollectData = CMP.Models.kvsPlc.Roll.KvsStructRollerCollectData.stRollerCollectData;
using static CMP.Tests.Services.DataLoggingTestSupport;

namespace CMP.Tests.Services;

/// <summary>
/// **模拟一次完整的 DataLogging 过程**（不接 PLC、不用全局单例），用的全是生产代码：
///
///   采集端周期性地把结构体/IO 缓冲区写进运行期 SV（ApplyIoData / ApplyMotorServoStatus / SetSv）
///     → 日志端按周期组一行并追加到 CSV（DataLoggerHelper.BuildXRow + DataLogger.AppendData）
///     → 会话结束时把临时 Wafer 名改成真实 Wafer 名（Chamber.LogWithTemporaryWaferNameAsync 的 finally）
///
/// 覆盖：文件与表头生成、多行对齐、临时名改名、IO 读不到时段落为空、SV 表补变量后自动补列且旧行不丢。
/// </summary>
[TestClass]
public class DataLoggingProcessSimulationTests
{
    /// <summary>三个采集周期 + 改名：模拟一次 RollerA2 的数据记录会话。</summary>
    [TestMethod]
    public void RollerSession_SimulatesCollectCycles_ThenRenamesTemporaryFileToWaferName()
    {
        var roller = CreateRollerWithSv("RollerA", "SV_RollerA.csv");
        if (roller == null)
        {
            return;
        }

        var basePath = NewTempLogPath();
        var dataLogger = new DataLogger("RollerA2", basePath);
        var helper = new DataLoggerHelper(dataLogger, "RollerA2");
        // 与 Chamber.LogWithTemporaryWaferNameAsync 里一样的临时名，结束时再改成真实 Wafer 名
        var temporaryWaferName = $"Temporary_RollerA2_{Guid.NewGuid():N}";

        var cylinderUp = SvFrom("SV_RollerA.csv", "RollerA2 Lower Roller Cylinder Up Position Sensor");
        var processStep = SvFrom("SV_RollerA.csv", "RollerA2 Process Step Act");
        var leftPillarServo = SvFrom("SV_RollerA.csv", "RollerA2 WF Left Pillar Motor ServoOn");

        for (var tick = 0; tick < 3; tick++)
        {
            // 模拟 PLC 采集：第一周期 IO 还没读到（err != KERR_NONE，不写 SV），该段列应为空
            if (tick > 0)
            {
                var ioBuffer = new byte[64];
                SetBit(ioBuffer, word: 0, bit: 0); // LowerRollerCylinderUpPositionSensor = word0 bit0
                roller.ApplyIoData(ioBuffer, rollerIndex: 2);
            }

            // 模拟过程量：Process Step Act 每周期递增
            roller.SetSv(roller.BuildRuntimeKey(null, nameof(stRollerCollectData._2ProcessStepAct)), (short)(800 + tick));
            // 模拟 M534 bit8..11：最后一个周期 WF 左柱伺服 ON
            roller.ApplyMotorServoStatus([tick == 2, false, true, false], "_2");

            var row = helper.BuildRollerRow(roller, rollerIndex: 1, logTime: $"2026-09-16 10:12:1{tick}:000", timeSpan: tick.ToString("F3"));
            dataLogger.AppendData(temporaryWaferName, row);
        }

        // 会话结束：临时 Wafer 名 -> 真实 Wafer 名（Chamber 的 finally 里做的事）
        Assert.IsTrue(dataLogger.RenameWaferFile(temporaryWaferName, "(C3W3)"), "临时日志文件应改名为真实 Wafer 名");
        Assert.IsFalse(dataLogger.FileExists(temporaryWaferName), "临时名文件不该还在");

        var file = SingleCsvFile(basePath);
        StringAssert.Contains(file, "RollerA2", "日志目录应带腔体名");
        StringAssert.Contains(Path.GetFileName(file), "(C3W3)", "文件名应是真实 Wafer 名");

        var table = ReadCsvTable(basePath);
        Assert.AreEqual(3, table.Rows.Count, "三个采集周期应有三行");
        Assert.IsTrue(table.Rows.All(row => row.Length == table.Headers.Length), "所有行必须与表头同列数");

        // 每个周期的值都对得上（列对齐 + 值正确）
        AssertCell(table, 0, processStep, "800");
        AssertCell(table, 1, processStep, "801");
        AssertCell(table, 2, processStep, "802");

        // 第一周期 IO 没读到 -> 空单元格；之后有值；填充位（Member）不产生列
        AssertCell(table, 0, cylinderUp, "");
        AssertCell(table, 1, cylinderUp, "True");
        AssertCell(table, 2, cylinderUp, "True");
        Assert.IsFalse(table.Headers.Any(header => header.Contains("Member", StringComparison.Ordinal)), "CSV 里出现了 Member* 填充列");

        // 伺服位：最后一周期 WF 左柱 ON，其它周期 OFF
        AssertCell(table, 0, leftPillarServo, "False");
        AssertCell(table, 2, leftPillarServo, "True");

        // 时间/Time Span 列
        Assert.AreEqual("2026-09-16 10:12:10:000", table.Rows[0][0]);
        Assert.AreEqual("0.000", table.Rows[0][1]);
    }

    /// <summary>
    /// 模拟“SV 变量表后来补了一个变量”：日志文件自动补列，且已写的历史行不丢、不错位。
    /// 用 Dry 结构体里 SV 表原本没有的 WaferOutletDoorOpenControl_Copy1 做样本。
    /// </summary>
    [TestMethod]
    public void NewSvVariable_AddsColumnOnNextCycle_AndKeepsExistingRows()
    {
        var dry = CreateDryWithSv("DryA", "SV_DryA.csv");
        if (dry == null)
        {
            return;
        }

        const string missingField = "WaferOutletDoorOpenControl_Copy1";
        Assert.IsTrue(dry.ioDataFields.Any(field => field.Name == missingField), $"Dry 的 IO 结构体里应有字段 {missingField}");
        var addedSv = new SV { SVID = 17999, Name = $"DryA {missingField}", Unit = string.Empty, Comment = string.Empty };
        Assert.IsFalse(
            DataLoggingTestSupport.ReadConfiguredSvs(SvPath("SV_DryA.csv")).Any(sv => sv.SVID == addedSv.SVID),
            "这个变量本来不在 SV 表里（本用例就是要模拟后来补上）");

        var basePath = NewTempLogPath();
        var dataLogger = new DataLogger("DryA", basePath);
        var temporaryWaferName = $"Temporary_DryA_{Guid.NewGuid():N}";
        var velocity = SvFrom("SV_DryA.csv", "DryA Chuck Rotation Motor Velocity Valu Act");

        // 补变量之前：两个周期。老列有值，缺变量的那一列根本不存在
        var helperBefore = new DataLoggerHelper(dataLogger, "DryA");
        for (var tick = 0; tick < 2; tick++)
        {
            dry.SetSv(dry.BuildRuntimeKey(null, nameof(stDryCollectData.ChuckRotationMotorVelocityValuAct)), 12.5f + tick);
            dataLogger.AppendData(temporaryWaferName, helperBefore.BuildDryRow(dry, $"2026-09-16 11:00:0{tick}:000", tick.ToString("F3")));
        }

        var before = ReadCsvTable(basePath);
        Assert.AreEqual(2, before.Rows.Count);
        Assert.IsNull(Cell(before, 0, addedSv), "补变量之前不该有这一列");
        AssertCell(before, 0, velocity, "12.5");
        AssertCell(before, 1, velocity, "13.5");

        // 模拟 SV 变量表补上这个变量（新会话会用新的元数据索引）
        dry.ListSV[addedSv.Name] = addedSv;
        var helperAfter = new DataLoggerHelper(dataLogger, "DryA");
        dry.SetSv(dry.BuildRuntimeKey(null, nameof(stDryCollectData.ChuckRotationMotorVelocityValuAct)), 14.5f);
        dataLogger.AppendData(temporaryWaferName, helperAfter.BuildDryRow(dry, "2026-09-16 11:00:02:000", "2.000"));

        var after = ReadCsvTable(basePath);
        Assert.AreEqual(3, after.Rows.Count, "补列时历史行必须保留");
        Assert.IsNotNull(Cell(after, 0, addedSv), "补变量之后应有这一列（历史行补空）");
        AssertCell(after, 0, addedSv, "");
        AssertCell(after, 2, addedSv, "");
        // 历史行的老列没有错位
        AssertCell(after, 0, velocity, "12.5");
        AssertCell(after, 2, velocity, "14.5");
        Assert.IsTrue(after.Rows.All(row => row.Length == after.Headers.Length), "重建后所有行必须与表头同列数");
    }
}
