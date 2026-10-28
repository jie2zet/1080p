using System;
using System.Collections.Generic;
using System.Text;

namespace CMP.Constants
{
    public static class ChamberExponentMap
    {
        /// <summary>
        /// 与PLC工程师约定的Chamber所对应的指数
        /// </summary>
        public static readonly Dictionary<string, int> ChamberExponentMapping = new ()
        {
            { "PolishA", 0 },
            { "PolishB", 1 },
            { "PolishC", 2 },
            { "PolishD", 3 },
            { "RollerA1", 4 },
            { "RollerA2", 5 },
            { "RollerA3", 6 },
            { "RollerB1", 7 },
            { "RollerB2", 8 },
            { "RollerB3", 9 },
            { "PenA", 10 },
            { "PenB", 12 },
            { "DryA", 14 },
            { "DryB", 15 }         
        };

        /// <summary>
        /// 自动校准使用的 DPI 压力计串口设置。
        /// 原第一项目 appsettings.json 中的 "DpiSerialPort" 节点迁移到此，
        /// 修改串口或波特率时只需改这里，无需再改配置文件。
        /// </summary>
        public static class DpiSerialPort
        {
            /// <summary>
            /// 串口名称，例如 COM20。
            /// </summary>
            public const string PortName = "COM20";

            /// <summary>
            /// 串口波特率。
            /// </summary>
            public const int BaudRate = 9600;
        }

        /// <summary>
        /// 压力校准公式开关（PLC 的 ZF 设备号）。
        /// 自动校准开始前写 0 关闭 PLC 侧压力校准公式，结束后写 1 恢复，
        /// 保证采集到的是“原始设定压力 → 实际压力”的关系。
        ///
        /// 设备号约定（与第一项目 CalibrationPressure.GetTarget 保持一致）：
        ///   - 抛光：PolishA~PolishD 各自 PLC 的 ZF10003（第一项目同地址；
        ///           ZF10004 在第二项目已用于 ProcessType 标志，故开关仍取 10003）；
        ///   - 滚轮：按“腔体系统参数基址 + 上/下辊偏移”取值，
        ///           上辊 = 基址 + 1、下辊 = 基址 + 4；
        ///           各腔体基址：1 号腔 10000、2 号腔 25000、3 号腔 40000
        ///           （由现有常量 Param(10014/25014/40014) 可确认基址），
        ///           即 1 腔上/下辊 = 10001/10004，2 腔 = 25001/25004，3 腔 = 40001/40004；
        ///   - 笔：PenA1 / PenB1 均为 1 号腔，ZF10001。
        ///
        /// Roller的：
        ///   “奇数腔体上辊使用 ZF10001、下辊使用 ZF10004；
        ///     偶数腔体上辊使用 ZF25001、下辊使用 ZF25004。”
        ///
        /// 如现场 PLC 版本的开关地址不同，只需修改这里，不需要改自动校准代码。
        /// </summary>
        public static readonly Dictionary<string, uint> PressureCalibrationSwitchHandles = new()
        {
            // 抛光：同一工位所有 Zone / RRing 共用一个公式开关
            { "PolishA", 10003 },
            { "PolishB", 10003 },
            { "PolishC", 10003 },
            { "PolishD", 10003 },

            // 滚轮：上辊 = 腔体基址 + 1，下辊 = 腔体基址 + 4（上下辊地址不同）
            { "RollerA1Upper", 10001 },
            { "RollerA1Lower", 10004 },
            { "RollerA2Upper", 25001 },
            { "RollerA2Lower", 25004 },
            { "RollerA3Upper", 40001 },
            { "RollerA3Lower", 40004 },
            { "RollerB1Upper", 10001 },
            { "RollerB1Lower", 10004 },
            { "RollerB2Upper", 25001 },
            { "RollerB2Lower", 25004 },
            { "RollerB3Upper", 40001 },
            { "RollerB3Lower", 40004 },

            // 笔：本项目只使用 1 号腔
            { "PenA1", 10001 },
            { "PenB1", 10001 }
        };
    }
}