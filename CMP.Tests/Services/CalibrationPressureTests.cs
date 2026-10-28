using System;
using CMP.Services.Calibration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CMP.Tests.Services;

/// <summary>
/// 自动校准通道映射与 DPI 串口解析的回归测试。
/// 验证“工位 + 分区”到 PLC 句柄/命令码/量程/公式开关，以及过程数据解码没有因为迁移而错位。
/// 说明：测试只使用 PLC 常量表，不连接真实 PLC。
/// </summary>
[TestClass]
public class CalibrationPressureTests
{
    [DataTestMethod]
    // 抛光：Zone1~Zone8、RRing；公式开关为各工位 PLC 的 ZF10003
    [DataRow("PolishA", "Zone1", 18214u, 0x50u, -14f, 14f, 1802, 1800, 10003u)]
    [DataRow("PolishA", "Zone2", 18216u, 0x50u, -14f, 14f, 1805, 1803, 10003u)]
    [DataRow("PolishA", "Zone3", 18218u, 0x50u, -14f, 14f, 1808, 1806, 10003u)]
    [DataRow("PolishB", "Zone4", 18220u, 0x50u, -14f, 14f, 1811, 1809, 10003u)]
    [DataRow("PolishB", "Zone5", 18222u, 0x50u, -14f, 14f, 1814, 1812, 10003u)]
    [DataRow("PolishC", "Zone6", 18224u, 0x50u, -14f, 14f, 1817, 1815, 10003u)]
    [DataRow("PolishC", "Zone7", 18226u, 0x50u, -14f, 14f, 1820, 1818, 10003u)]
    [DataRow("PolishD", "Zone8", 18228u, 0x50u, -14f, 14f, 1823, 1821, 10003u)]
    [DataRow("PolishD", "RRing", 18230u, 0x50u, 0f, 130f, 1826, 1824, 10003u)]
    // 滚轮：上辊 = 腔体基址+1，下辊 = 腔体基址+4；1/2/3 号腔基址为 10000/25000/40000
    [DataRow("RollerA", "RollerA1Lower", 18198u, 0x60u, 0f, 500f, 1102, 1103, 10004u)]
    [DataRow("RollerA", "RollerA1Upper", 18200u, 0x60u, 0f, 500f, 1107, 1108, 10001u)]
    [DataRow("RollerA", "RollerA2Lower", 33198u, 0x63u, 0f, 500f, 1102, 1103, 25004u)]
    [DataRow("RollerA", "RollerA2Upper", 33200u, 0x63u, 0f, 500f, 1107, 1108, 25001u)]
    [DataRow("RollerA", "RollerA3Lower", 48198u, 0x66u, 0f, 500f, 1102, 1103, 40004u)]
    [DataRow("RollerA", "RollerA3Upper", 48200u, 0x66u, 0f, 500f, 1107, 1108, 40001u)]
    [DataRow("RollerB", "RollerB1Lower", 18198u, 0x69u, 0f, 500f, 1102, 1103, 10004u)]
    [DataRow("RollerB", "RollerB1Upper", 18200u, 0x69u, 0f, 500f, 1107, 1108, 10001u)]
    [DataRow("RollerB", "RollerB2Lower", 33198u, 0x6Cu, 0f, 500f, 1102, 1103, 25004u)]
    [DataRow("RollerB", "RollerB2Upper", 33200u, 0x6Cu, 0f, 500f, 1107, 1108, 25001u)]
    [DataRow("RollerB", "RollerB3Lower", 48198u, 0x6Fu, 0f, 500f, 1102, 1103, 40004u)]
    [DataRow("RollerB", "RollerB3Upper", 48200u, 0x6Fu, 0f, 500f, 1107, 1108, 40001u)]
    // 笔：本项目只校准 1 号腔，公式开关为 ZF10001
    [DataRow("PenA", "PenA1", 18198u, 0x72u, 0f, 500f, 1094, 1095, 10001u)]
    [DataRow("PenB", "PenB1", 18198u, 0x78u, 0f, 500f, 1094, 1095, 10001u)]
    public void GetTarget_MapsChannelToPlcHandles(string station, string channel, uint paramHandle,
        uint commandHandle, float minValue, float maxValue, int pressCode, int resetCode, uint switchHandle)
    {
        var target = CalibrationPressure.GetTarget(station, channel);

        Assert.IsNotNull(target, $"{station}/{channel} 应能映射到压力通道。");
        Assert.AreEqual(paramHandle, target.PressureParamHandle, "压力设定参数句柄不一致。");
        Assert.AreEqual(commandHandle, target.PressCommandHandle, "加压命令句柄不一致。");
        Assert.AreEqual(commandHandle, target.ResetCommandHandle, "排气命令句柄不一致。");
        Assert.AreEqual(minValue, target.MinValue);
        Assert.AreEqual(maxValue, target.MaxValue);
        Assert.AreEqual(pressCode, BitConverter.ToInt32(target.PressCommandValue, 0), "加压命令码不一致。");
        Assert.AreEqual(resetCode, BitConverter.ToInt32(target.ResetCommandValue, 0), "排气命令码不一致。");
        Assert.AreEqual(switchHandle, target.CalibrationSwitchHandle, "压力校准公式开关不一致。");
    }

    [DataTestMethod]
    [DataRow("PolishA", "Zone1", 176)]
    [DataRow("PolishA", "RRing", 240)]
    [DataRow("RollerA", "RollerA1Lower", 56)]
    [DataRow("RollerA", "RollerA2Upper", 256)]
    [DataRow("PenA", "PenA1", 52)]
    public void Target_DecodesProcessData(string station, string channel, int byteOffset)
    {
        var target = CalibrationPressure.GetTarget(station, channel);
        Assert.IsNotNull(target);

        var buffer = new byte[2048];
        BitConverter.GetBytes(12.5f).CopyTo(buffer, byteOffset);

        Assert.AreEqual(12.5f, target.Decode(buffer), 1e-4f);
    }

    [DataTestMethod]
    [DataRow("PolishA", "Disk")]
    [DataRow("PolishA", "RollerA1Upper")]
    [DataRow("RollerA", "Zone1")]
    [DataRow("PenA", "PenB1")]
    [DataRow("Unknown", "Zone1")]
    public void GetTarget_ReturnsNullForUnsupportedChannel(string station, string channel)
    {
        Assert.IsNull(CalibrationPressure.GetTarget(station, channel));
    }

    [DataTestMethod]
    [DataRow("PolishA", true)]
    [DataRow("PolishD", true)]
    [DataRow("RollerA", true)]
    [DataRow("RollerB", true)]
    [DataRow("PenA", true)]
    [DataRow("PenB", true)]
    [DataRow("DryA", false)]
    [DataRow("", false)]
    public void SupportsStation_MatchesCalibrationTabs(string station, bool expected)
    {
        Assert.AreEqual(expected, CalibrationPressure.SupportsStation(station));
    }

    [DataTestMethod]
    [DataRow(":SENS:PRES 12.345\r\n", 12.345f)]
    [DataRow(":sens:pres 0.5", 0.5f)]
    public void DpiSerialConnection_ParsesPressure(string response, float expected)
    {
        Assert.AreEqual(expected, DpiSerialConnection.ParsePressure(response), 1e-4f);
    }

    [TestMethod]
    public void DpiSerialConnection_RejectsInvalidResponse()
    {
        Assert.ThrowsException<FormatException>(() => DpiSerialConnection.ParsePressure(":SENS:PRES abc"));
        Assert.ThrowsException<FormatException>(() => DpiSerialConnection.ParsePressure("garbage"));
    }
}
