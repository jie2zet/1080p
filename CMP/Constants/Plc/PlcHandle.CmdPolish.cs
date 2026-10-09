namespace CMP.Constants.Plc;

/// <summary>
/// PLC Handle 类 - Command 相关定义
/// </summary>
public static partial class PlcHandle
{
    /// <summary>
    /// 基础命令操作
    /// </summary>
    public static class CmdPolish
    {
        /// <summary>
        /// 所有 Objects
        /// </summary>
        public static class All
        {
            /// <summary>
            /// 初始化
            /// </summary>
            [PlcHandle(Handle.CmdPolish, "初始化")]
            public const int Initialize = 1004;

            /// <summary>
            /// 急停
            /// </summary>
            [PlcHandle(Handle.CmdPolish, "急停")]
            public const int Abort = 1005;

            /// <summary>
            /// Mode 模式
            /// </summary>
            public static class Mode
            {
                [PlcHandle(Handle.CmdPolish, "自动模式")]
                public const int Auto = 1000;

                [PlcHandle(Handle.CmdPolish, "手动模式")]
                public const int Manual = 1001;
            }
        }

        /// <summary>
        /// Motor 电机
        /// </summary>
        public static class Motor
        {
            /// <summary>
            /// Platen Rotation Motor 转盘电机
            /// </summary>
            public static class PlatenRotation
            {
                [PlcHandle(Handle.CmdPolish, "运行")]
                public const int Run = 1020;

                [PlcHandle(Handle.CmdPolish, "停止")]
                public const int Stop = 1021;

                [PlcHandle(Handle.CmdPolish, "寸动")]
                public const int Inching = 1022;

                [PlcHandle(Handle.CmdPolish, "伺服", 0)]
                public const int Servo = 530;

                public static class Param
                {
                    [Param(18000, 300, 0, 1000, "大盘旋转电机速度", "速度", "r/min")]
                    public static int Velocity = 300;
                }
            }

            /// <summary>
            /// Head Rotation Motor 头部旋转电机
            /// </summary>
            public static class HeadRotation
            {
                [PlcHandle(Handle.CmdPolish, "运行")]
                public const int Run = 1030;

                [PlcHandle(Handle.CmdPolish, "停止")]
                public const int Stop = 1031;

                [PlcHandle(Handle.CmdPolish, "寸动")]
                public const int Inching = 1032;

                [PlcHandle(Handle.CmdPolish, "伺服", 1)]
                public const int Servo = 530;
                public static class Param
                {
                    [Param(18010, 300, 0, 1000, "头旋转电机速度", "速度", "r/min")]
                    public static int Velocity = 300;
                }
            }

            /// <summary>
            /// Head Sweep Motor 头部摆动电机
            /// </summary>
            public static class HeadSweep
            {
                [PlcHandle(Handle.CmdPolish, "任意位置")]
                public const int AnyPosition = 1040;

                [PlcHandle(Handle.CmdPolish, "停止")]
                public const int Stop = 1041;

                [PlcHandle(Handle.CmdPolish, "寸动")]
                public const int Inching = 1042;

                [PlcHandle(Handle.CmdPolish, "原点位置")]
                public const int HomePosition = 1043;

                [PlcHandle(Handle.CmdPolish, "抛光近位置")]
                public const int PolishNearPosition = 1044;

                [PlcHandle(Handle.CmdPolish, "抛光远位置")]
                public const int PolishFarPosition = 1045;

                [PlcHandle(Handle.CmdPolish, "卸片位置")]
                public const int DechuckPosition = 1046;

                [PlcHandle(Handle.CmdPolish, "伺服", 2)]
                public const int Servo = 530;

                public static class Param
                {
                    [Param(18020, 300, 0, 1000, "头摆动电机速度", "速度", "mm/s")]
                    public static int Velocity = 300;

                    [Param(18030, 100, 0, 1000, "头摆动电机位置", "位置", "mm")]
                    public static int Position = 100;
                }
            }

            /// <summary>
            /// Head Up/Down Motor 头部升降电机
            /// </summary>
            public static class HeadUpDown
            {
                [PlcHandle(Handle.CmdPolish, "任意位置")]
                public const int AnyPosition = 1050;

                [PlcHandle(Handle.CmdPolish, "停止")]
                public const int Stop = 1051;

                [PlcHandle(Handle.CmdPolish, "寸动")]
                public const int Inching = 1052;

                [PlcHandle(Handle.CmdPolish, "高位置")]
                public const int HighPosition = 1053;

                [PlcHandle(Handle.CmdPolish, "装杯位置")]
                public const int LoadCupPosition = 1054;

                [PlcHandle(Handle.CmdPolish, "抛光位置")]
                public const int PolishPosition = 1055;

                [PlcHandle(Handle.CmdPolish, "清洁位置")]
                public const int CleanPosition = 1056;

                [PlcHandle(Handle.CmdPolish, "伺服", 3)]
                public const int Servo = 530;

                public static class Param
                {
                    [Param(18040, 300, 0, 1000, "头升降电机速度", "速度", "mm/s")]
                    public static int Velocity = 300;

                    [Param(18050, 30, -180, 180, "头升降电机位置", "位置", "mm")]
                    public static int Position = 30;
                }
            }

            /// <summary>
            /// Conditioner Sweep Motor 研磨头摆动电机
            /// </summary>
            public static class ConditionerSweep
            {
                [PlcHandle(Handle.CmdPolish, "任意位置")]
                public const int AnyPosition = 1080;

                [PlcHandle(Handle.CmdPolish, "停止")]
                public const int Stop = 1081;

                [PlcHandle(Handle.CmdPolish, "原点位置")]
                public const int HomePosition = 1083;

                [PlcHandle(Handle.CmdPolish, "开始位置")]
                public const int StartPosition = 1084;

                [PlcHandle(Handle.CmdPolish, "结束位置")]
                public const int EndPosition = 1085;

                [PlcHandle(Handle.CmdPolish, "伺服", 5)]
                public const int Servo = 530;

                public static class Param
                {
                    [Param(18080, 300, 0, 1000, "修整器摆动电机速度", "速度", "deg/s")]
                    public static int Velocity = 300;

                    [Param(18090, 30, -180, 180, "修整器摆动电机位置", "位置", "deg")]
                    public static int Position = 30;
                }
            }

            /// <summary>
            /// Conditioner Head Rotation Motor 研磨头旋转电机
            /// DiskRotateMotor 又名
            /// </summary>
            public static class ConditionerHeadRotation
            {
                [PlcHandle(Handle.CmdPolish, "运行")]
                public const int Run = 1070;

                [PlcHandle(Handle.CmdPolish, "停止")]
                public const int Stop = 1071;

                [PlcHandle(Handle.CmdPolish, "寸动")]
                public const int Inching = 1072;

                [PlcHandle(Handle.CmdPolish, "伺服", 4)]
                public const int Servo = 530;

                public static class Param
                {
                    [Param(18070, 300, 0, 1000, "修整器头旋转电机速度", "速度", "r/min")]
                    public static int Velocity = 300;
                }
            }

            /// <summary>
            /// SlurryArm Sweep Motor 研磨液手臂摆动电机
            /// </summary>
            public static class SlurryArmSweep
            {
                [PlcHandle(Handle.CmdPolish, "任意位置")]
                public const int AnyPosition = 1100;

                [PlcHandle(Handle.CmdPolish, "停止")]
                public const int Stop = 1101;

                [PlcHandle(Handle.CmdPolish, "原点位置")]
                public const int HomePosition = 1103;

                [PlcHandle(Handle.CmdPolish, "液体位置")]
                public const int SlurryPosition = 1104;
                
                [PlcHandle(Handle.CmdPolish, "伺服", 6)]
                public const int Servo = 530;

                public static class Param
                {
                    [Param(18110, 300, 0, 1000, "研磨液手臂电机速度", "速度", "deg/s")]
                    public static int Velocity = 300;

                    [Param(18120, 30, -180, 180, "研磨液手臂电机位置", "位置", "deg")]
                    public static int Position = 30;
                }
            }
        }

        public static class Process
        {
            public static class SingleProcessStart
            {
                [PlcHandle(Handle.CmdPolish, "LoadStart")]
                public const int LoadStart = 1013;
                [PlcHandle(Handle.CmdPolish, "PolishStart")]
                public const int PolishStart = 1012;
                [PlcHandle(Handle.CmdPolish, "UnloadStart")]
                public const int UnloadStart = 1014;
                [PlcHandle(Handle.CmdPolish, "DechuckStart")]
                public const int DechuckStart = 1015;
                [PlcHandle(Handle.CmdPolish, "ExsituStart")]
                public const int ExsituStart = 1016;
            }
        }

        /// <summary>
        /// Liquid 液体控制
        /// </summary>
        public static class Liquid
        {
            /// <summary>
            /// Slurry1/DIW 研磨液1/去离子水
            /// </summary>
            [LiquidControl("PolishSlurry1Slurry")]
            public static class Slurry1
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1260;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1261;

                [LiquidControl("PolishSlurry1Diw", "", CommandKeyPrefix = "Slurry1Diw")]
                public static class Diw
                {
                    [PlcHandle(Handle.CmdPolish, "打开")]
                    public const int Open = 1266;

                    [PlcHandle(Handle.CmdPolish, "关闭")]
                    public const int Close = 1267;
                }
                /// <summary>
                /// Slurry1 Three-way Total Inlet Valve 三通总进口阀
                /// </summary>
                [LiquidControl("PolishSlurry1ThreeWayTotalInletValve", "FV-POLA100A", CommandKeyPrefix = "Slurry1ThreeWayTotalInletValve")]
                public static class ThreeWayTotalInletValve
                {
                    [PlcHandle(Handle.CmdPolish, "打开")]
                    public const int Open = 1189;

                    [PlcHandle(Handle.CmdPolish, "关闭")]
                    public const int Close = 1190;
                }

                /// <summary>
                /// Slurry1 Three-way Return Valve 三通回流阀
                /// </summary>
                [LiquidControl("PolishSlurry1ThreeWayReturnValve", "FV-POLA100B", CommandKeyPrefix = "Slurry1ThreeWayReturnValve")]
                public static class ThreeWayReturnValve
                {
                    [PlcHandle(Handle.CmdPolish, "打开")]
                    public const int Open = 1191;

                    [PlcHandle(Handle.CmdPolish, "关闭")]
                    public const int Close = 1192;
                }

                /// <summary>
                /// Slurry1 Three-way Inlet Valve 三通进口阀
                /// </summary>
                [LiquidControl("PolishSlurry1ThreeWayInletValve", "FV-POLA100C", CommandKeyPrefix = "Slurry1ThreeWayInletValve")]
                public static class ThreeWayInletValve
                {
                    [PlcHandle(Handle.CmdPolish, "打开")]
                    public const int Open = 1193;

                    [PlcHandle(Handle.CmdPolish, "关闭")]
                    public const int Close = 1194;
                }

                /// <summary>
                /// Water Three-way Inlet Valve 水三通进口阀
                /// </summary>
                [LiquidControl("PolishSlurry1WaterThreeWayInletValve", "FV-POLA100D", CommandKeyPrefix = "Slurry1WaterThreeWayInletValve")]
                public static class WaterThreeWayInletValve
                {
                    [PlcHandle(Handle.CmdPolish, "打开")]
                    public const int Open = 1195;

                    [PlcHandle(Handle.CmdPolish, "关闭")]
                    public const int Close = 1196;
                }

                /// <summary>
                /// Slurry1/DIW Outlet Valve 出口阀
                /// </summary>
                [LiquidControl("PolishSlurry1OutletValve", "SBV-POLA100", CommandKeyPrefix = "Slurry1OutletValve")]
                public static class OutletValve
                {
                    [PlcHandle(Handle.CmdPolish, "打开")]
                    public const int Open = 1197;

                    [PlcHandle(Handle.CmdPolish, "关闭")]
                    public const int Close = 1198;
                }

                /// <summary>
                /// Slurry1/DIW Flow 流量控制
                /// </summary>
                [LiquidControl("PolishSlurry1Flow", "FC-POLA100", CommandKeyPrefix = "Slurry1Flow", SetpointPath = "CmdPolish.Liquid.Slurry1.Param.Flowrate")]
                public static class Flow
                {
                    [PlcHandle(Handle.CmdPolish, "流量设定")]
                    public const int Flowrate = 1199;

                    [PlcHandle(Handle.CmdPolish, "流量复位")]
                    public const int FlowrateReset = 1200;

                    [PlcHandle(Handle.CmdPolish, "MFC校准")]
                    public const int MFCCalibration = 1201;
                }

                public static class Param
                {
                    [Param(18172, 100.00, 0.00, 1000.00, "Slurry Line1/DIW流量", "Slurry Line1/DIW流量", "mL/min",
                        DecimalPlaces = 2, ZrZf = "ZF", WordCount = 2, DisplayNameKey = "PolishSlurry1DiwFlowrateSet")]
                    public static int Flowrate = 100;
                }
            }

            /// <summary>
            /// Slurry2/DIW 研磨液2/去离子水
            /// </summary>
            [LiquidControl("PolishSlurry2Slurry")]
            public static class Slurry2
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1262;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1263;

                [LiquidControl("PolishSlurry2Diw", "", CommandKeyPrefix = "Slurry2Diw")]
                public static class Diw
                {
                    [PlcHandle(Handle.CmdPolish, "打开")]
                    public const int Open = 1268;

                    [PlcHandle(Handle.CmdPolish, "关闭")]
                    public const int Close = 1269;
                }
                /// <summary>
                /// Slurry2 Three-way Total Inlet Valve 三通总进口阀
                /// </summary>
                [LiquidControl("PolishSlurry2ThreeWayTotalInletValve", "FV-POLA102A", CommandKeyPrefix = "Slurry2ThreeWayTotalInletValve")]
                public static class ThreeWayTotalInletValve
                {
                    [PlcHandle(Handle.CmdPolish, "打开")]
                    public const int Open = 1204;

                    [PlcHandle(Handle.CmdPolish, "关闭")]
                    public const int Close = 1205;
                }

                /// <summary>
                /// Slurry2 Three-way Return Valve 三通回流阀
                /// </summary>
                [LiquidControl("PolishSlurry2ThreeWayReturnValve", "FV-POLA102B", CommandKeyPrefix = "Slurry2ThreeWayReturnValve")]
                public static class ThreeWayReturnValve
                {
                    [PlcHandle(Handle.CmdPolish, "打开")]
                    public const int Open = 1206;

                    [PlcHandle(Handle.CmdPolish, "关闭")]
                    public const int Close = 1207;
                }

                /// <summary>
                /// Slurry2 Three-way Inlet Valve 三通进口阀
                /// </summary>
                [LiquidControl("PolishSlurry2ThreeWayInletValve", "FV-POLA102C", CommandKeyPrefix = "Slurry2ThreeWayInletValve")]
                public static class ThreeWayInletValve
                {
                    [PlcHandle(Handle.CmdPolish, "打开")]
                    public const int Open = 1208;

                    [PlcHandle(Handle.CmdPolish, "关闭")]
                    public const int Close = 1209;
                }

                /// <summary>
                /// Water Three-way Inlet Valve 水三通进口阀
                /// </summary>
                [LiquidControl("PolishSlurry2WaterThreeWayInletValve", "FV-POLA102D", CommandKeyPrefix = "Slurry2WaterThreeWayInletValve")]
                public static class WaterThreeWayInletValve
                {
                    [PlcHandle(Handle.CmdPolish, "打开")]
                    public const int Open = 1210;

                    [PlcHandle(Handle.CmdPolish, "关闭")]
                    public const int Close = 1211;
                }

                /// <summary>
                /// Slurry2/DIW Outlet Valve 出口阀
                /// </summary>
                [LiquidControl("PolishSlurry2OutletValve", "SBV-POLA102", CommandKeyPrefix = "Slurry2OutletValve")]
                public static class OutletValve
                {
                    [PlcHandle(Handle.CmdPolish, "打开")]
                    public const int Open = 1212;

                    [PlcHandle(Handle.CmdPolish, "关闭")]
                    public const int Close = 1213;
                }

                /// <summary>
                /// Slurry2/DIW Flow 流量控制
                /// </summary>
                [LiquidControl("PolishSlurry2Flow", "FC-POLA102", CommandKeyPrefix = "Slurry2Flow", SetpointPath = "CmdPolish.Liquid.Slurry2.Param.Flowrate")]
                public static class Flow
                {
                    [PlcHandle(Handle.CmdPolish, "流量设定")]
                    public const int Flowrate = 1214;

                    [PlcHandle(Handle.CmdPolish, "流量复位")]
                    public const int FlowrateReset = 1215;

                    [PlcHandle(Handle.CmdPolish, "MFC校准")]
                    public const int MFCCalibration = 1216;
                }

                public static class Param
                {
                    [Param(18174, 100.00, 0.00, 1000.00, "Slurry Line2/DIW流量", "Slurry Line2/DIW流量", "mL/min",
                        DecimalPlaces = 2, ZrZf = "ZF", WordCount = 2, DisplayNameKey = "PolishSlurry2DiwFlowrateSet")]
                    public static int Flowrate = 100;
                }
            }

            /// <summary>
            /// Slurry3/DIW 研磨液3/去离子水
            /// </summary>
            [LiquidControl("PolishSlurry3Slurry")]
            public static class Slurry3
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1264;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1265;

                [LiquidControl("PolishSlurry3Diw", "", CommandKeyPrefix = "Slurry3Diw")]
                public static class Diw
                {
                    [PlcHandle(Handle.CmdPolish, "打开")]
                    public const int Open = 1270;

                    [PlcHandle(Handle.CmdPolish, "关闭")]
                    public const int Close = 1271;
                }
                /// <summary>
                /// Slurry3 Three-way Total Inlet Valve 三通总进口阀
                /// </summary>
                [LiquidControl("PolishSlurry3ThreeWayTotalInletValve", "FV-POLA104A", CommandKeyPrefix = "Slurry3ThreeWayTotalInletValve")]
                public static class ThreeWayTotalInletValve
                {
                    [PlcHandle(Handle.CmdPolish, "打开")]
                    public const int Open = 1219;

                    [PlcHandle(Handle.CmdPolish, "关闭")]
                    public const int Close = 1220;
                }

                /// <summary>
                /// Slurry3 Three-way Return Valve 三通回流阀
                /// </summary>
                [LiquidControl("PolishSlurry3ThreeWayReturnValve", "FV-POLA104B", CommandKeyPrefix = "Slurry3ThreeWayReturnValve")]
                public static class ThreeWayReturnValve
                {
                    [PlcHandle(Handle.CmdPolish, "打开")]
                    public const int Open = 1221;

                    [PlcHandle(Handle.CmdPolish, "关闭")]
                    public const int Close = 1222;
                }

                /// <summary>
                /// Slurry3 Three-way Inlet Valve 三通进口阀
                /// </summary>
                [LiquidControl("PolishSlurry3ThreeWayInletValve", "FV-POLA104C", CommandKeyPrefix = "Slurry3ThreeWayInletValve")]
                public static class ThreeWayInletValve
                {
                    [PlcHandle(Handle.CmdPolish, "打开")]
                    public const int Open = 1223;

                    [PlcHandle(Handle.CmdPolish, "关闭")]
                    public const int Close = 1224;
                }

                /// <summary>
                /// Water Three-way Inlet Valve 水三通进口阀
                /// </summary>
                [LiquidControl("PolishSlurry3WaterThreeWayInletValve", "FV-POLA104D", CommandKeyPrefix = "Slurry3WaterThreeWayInletValve")]
                public static class WaterThreeWayInletValve
                {
                    [PlcHandle(Handle.CmdPolish, "打开")]
                    public const int Open = 1225;

                    [PlcHandle(Handle.CmdPolish, "关闭")]
                    public const int Close = 1226;
                }

                /// <summary>
                /// Slurry3/DIW Outlet Valve 出口阀
                /// </summary>
                [LiquidControl("PolishSlurry3OutletValve", "SBV-POLA104", CommandKeyPrefix = "Slurry3OutletValve")]
                public static class OutletValve
                {
                    [PlcHandle(Handle.CmdPolish, "打开")]
                    public const int Open = 1227;

                    [PlcHandle(Handle.CmdPolish, "关闭")]
                    public const int Close = 1228;
                }

                /// <summary>
                /// Slurry3/DIW Flow 流量控制
                /// </summary>
                [LiquidControl("PolishSlurry3Flow", "FC-POLA104", CommandKeyPrefix = "Slurry3Flow", SetpointPath = "CmdPolish.Liquid.Slurry3.Param.Flowrate")]
                public static class Flow
                {
                    [PlcHandle(Handle.CmdPolish, "流量设定")]
                    public const int Flowrate = 1229;

                    [PlcHandle(Handle.CmdPolish, "流量复位")]
                    public const int FlowrateReset = 1230;

                    [PlcHandle(Handle.CmdPolish, "MFC校准")]
                    public const int MFCCalibration = 1231;
                }

                public static class Param
                {
                    [Param(18176, 100.00, 0.00, 1000.00, "Slurry Line3/DIW流量", "Slurry Line3/DIW流量", "mL/min",
                        DecimalPlaces = 2, ZrZf = "ZF", WordCount = 2, DisplayNameKey = "PolishSlurry3DiwFlowrateSet")]
                    public static int Flowrate = 100;
                }
            }

            /// <summary>
            /// DIW Outlet Valve 去离子水出口阀
            /// </summary>
            [LiquidControl("PolishDiwOutletValve", "SBV-POLA106")]
            public static class DIWOutletValve
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1272;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1273;
            }

            /// <summary>
            /// Platen Wash Valve 转盘清洗阀
            /// </summary>
            [LiquidControl("PolishPlatenWashValve", "FV-POLA120")]
            public static class PlatenWashValve
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1274;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1275;
            }

            /// <summary>
            /// Moving Head DIW Wash Valve 移动头去离子水清洗阀
            /// </summary>
            [LiquidControl("PolishMovingHeadDiwWashValve", "FV-POLA121A")]
            public static class MovingHeadDIWWashValve
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1276;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1277;
            }

            /// <summary>
            /// Moving Head External DIW Wash Valve 移动头外部去离子水清洗阀
            /// </summary>
            [LiquidControl("PolishMovingHeadExternalDiwWashValve", "FV-POLA121B")]
            public static class MovingHeadExternalDIWWashValve
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1278;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1279;
            }

            /// <summary>
            /// Head Chemical Wash Valve 头部化学清洗阀
            /// </summary>
            [LiquidControl("PolishHeadChemicalWashValve", "FV-POLA121C")]
            public static class HeadChemicalWashValve
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1280;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1281;
            }

            /// <summary>
            /// Conditioner DIW Wash Valve 修整器去离子水清洗阀
            /// </summary>
            [LiquidControl("PolishConditionerDiwWashValve", "FV-POLA122A")]
            public static class ConditionerDIWWashValve
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1282;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1283;
            }

            /// <summary>
            /// Conditioner Arm DIW Wash Valve 修整器臂去离子水清洗阀
            /// </summary>
            [LiquidControl("PolishConditionerArmDiwWashValve", "FV-POLA122B")]
            public static class ConditionerArmDIWWashValve
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1284;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1285;
            }

            /// <summary>
            /// Chemical Arm DIW Wash Valve 化学臂去离子水清洗阀
            /// </summary>
            [LiquidControl("PolishChemicalArmDiwWashValve", "FV-POLA123")]
            public static class ChemicalArmDIWWashValve
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1286;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1287;
            }

            /// <summary>
            /// Head Middle Wash Valve 头部中部清洗阀
            /// </summary>
            [LiquidControl("PolishRrCleaningValve", "FV-POLA124")]
            public static class HeadMiddleWashValve
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1288;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1289;
            }

            /// <summary>
            /// Head External Wash Valve 头部外部清洗阀
            /// </summary>
            [LiquidControl("PolishReleaseDiwWashValve", "FV-POLA126")]
            public static class HeadExternalWashValve
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1290;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1291;
            }

            /// <summary>
            /// WF Upper Face Chemical Wash Valve 晶圆上表面化学清洗阀
            /// </summary>
            [LiquidControl("PolishWfFaceChemicalWashValve", "FV-POLA127")]
            public static class WFUpperFaceChemicalWashValve
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1292;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1293;
            }

            /// <summary>
            /// WF Upper Face DIW Wash Valve 晶圆上表面去离子水清洗阀
            /// </summary>
            [LiquidControl("PolishWfFaceDiwWashValve", "FV-POLA128")]
            public static class WFUpperFaceDIWWashValve
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1294;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1295;
            }

            /// <summary>
            /// WF Lower Face Chemical Wash Valve 晶圆下表面化学清洗阀
            /// </summary>
            [LiquidControl("PolishGapCleaningValve", "FV-POLA130")]
            public static class WFLowerFaceChemicalWashValve
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1296;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1297;
            }

            /// <summary>
            /// WF Lower Face DIW Wash Valve 晶圆下表面去离子水清洗阀
            /// </summary>
            [LiquidControl("PolishTrCleaningValve", "FV-POLA129")]
            public static class WFLowerFaceDIWWashValve
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1298;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1299;
            }

            /// <summary>
            /// Platen Cool Water Valve 转盘冷却水阀
            /// </summary>
            [LiquidControl("PolishPlatenCoolWaterValve", "FV-POLA165")]
            public static class PlatenCoolWaterValve
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1300;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1301;
            }

            /// <summary>
            /// Platen Motor Cool Water Valve 转盘电机冷却水阀
            /// </summary>
            [LiquidControl("PolishPlatenMotorCoolWaterValve", "FV-POLA167")]
            public static class PlatenMotorCoolWaterValve
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1302;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1303;
            }
            /// <summary>
            /// Zone 4 DIW Valve Zone4 纯水冲洗阀
            /// </summary>
            [LiquidControl("PolishZone4DiwValve", "SV-POLA148")]
            public static class Zone4DIWValve
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1304;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1305;
            }

        }

        /// <summary>
        /// Gas 气体控制
        /// </summary>
        public static class Gas
        {
            public static class Zone1
            {
                [PlcHandle(Handle.CmdPolish, "排气")]
                public const int Vent = 1800;

                [PlcHandle(Handle.CmdPolish, "真空")]
                public const int Vacuum = 1801;

                [PlcHandle(Handle.CmdPolish, "加压")]
                public const int Press = 1802;

                public static class Param
                {
                    [Param(18214, 0, -14, 14, "Zone1压力", "Zone1压力", "kPa")]
                    public static int Pressure = 0;
                }
            }

            public static class Zone2
            {
                [PlcHandle(Handle.CmdPolish, "排气")]
                public const int Vent = 1803;

                [PlcHandle(Handle.CmdPolish, "真空")]
                public const int Vacuum = 1804;

                [PlcHandle(Handle.CmdPolish, "加压")]
                public const int Press = 1805;

                public static class Param
                {
                    [Param(18216, 0, -14, 14, "Zone2压力", "Zone2压力", "kPa")]
                    public static int Pressure = 0;
                }
            }

            public static class Zone3
            {
                [PlcHandle(Handle.CmdPolish, "排气")]
                public const int Vent = 1806;

                [PlcHandle(Handle.CmdPolish, "真空")]
                public const int Vacuum = 1807;

                [PlcHandle(Handle.CmdPolish, "加压")]
                public const int Press = 1808;

                public static class Param
                {
                    [Param(18218, 0, -14, 14, "Zone3压力", "Zone3压力", "kPa")]
                    public static int Pressure = 0;
                }
            }

            public static class Zone4
            {
                [PlcHandle(Handle.CmdPolish, "排气")]
                public const int Vent = 1809;

                [PlcHandle(Handle.CmdPolish, "真空")]
                public const int Vacuum = 1810;

                [PlcHandle(Handle.CmdPolish, "加压")]
                public const int Press = 1811;

                public static class Param
                {
                    [Param(18220, 0, -14, 14, "Zone4压力", "Zone4压力", "kPa")]
                    public static int Pressure = 0;
                }
            }

            public static class Zone5
            {
                [PlcHandle(Handle.CmdPolish, "排气")]
                public const int Vent = 1812;

                [PlcHandle(Handle.CmdPolish, "真空")]
                public const int Vacuum = 1813;

                [PlcHandle(Handle.CmdPolish, "加压")]
                public const int Press = 1814;

                public static class Param
                {
                    [Param(18222, 0, -14, 14, "Zone5压力", "Zone5压力", "kPa")]
                    public static int Pressure = 0;
                }
            }

            public static class Zone6
            {
                [PlcHandle(Handle.CmdPolish, "排气")]
                public const int Vent = 1815;

                [PlcHandle(Handle.CmdPolish, "真空")]
                public const int Vacuum = 1816;

                [PlcHandle(Handle.CmdPolish, "加压")]
                public const int Press = 1817;

                public static class Param
                {
                    [Param(18224, 0, -14, 14, "Zone6压力", "Zone6压力", "kPa")]
                    public static int Pressure = 0;
                }
            }

            public static class Zone7
            {
                [PlcHandle(Handle.CmdPolish, "排气")]
                public const int Vent = 1818;

                [PlcHandle(Handle.CmdPolish, "真空")]
                public const int Vacuum = 1819;

                [PlcHandle(Handle.CmdPolish, "加压")]
                public const int Press = 1820;

                public static class Param
                {
                    [Param(18226, 0, -14, 14, "Zone7压力", "Zone7压力", "kPa")]
                    public static int Pressure = 0;
                }
            }

            public static class Zone8
            {
                [PlcHandle(Handle.CmdPolish, "排气")]
                public const int Vent = 1821;

                [PlcHandle(Handle.CmdPolish, "真空")]
                public const int Vacuum = 1822;

                [PlcHandle(Handle.CmdPolish, "加压")]
                public const int Press = 1823;

                public static class Param
                {
                    [Param(18228, 0, -14, 14, "Zone8压力", "Zone8压力", "kPa")]
                    public static int Pressure = 0;
                }
            }

            public static class Retain
            {
                [PlcHandle(Handle.CmdPolish, "排气")]
                public const int Vent = 1824;

                [PlcHandle(Handle.CmdPolish, "真空")]
                public const int Vacuum = 1825;

                [PlcHandle(Handle.CmdPolish, "加压")]
                public const int Press = 1826;

                public static class Param
                {
                    [Param(18230, 0, 0, 130, "ZoneRR压力", "ZoneRR压力", "kPa")]
                    public static int Pressure = 0;
                }
            }

            /// <summary>
            /// Conditioner 修整器
            /// </summary>
            public static class Conditioner
            {
                public static class Param
                {
                    [Param(18232, 0, 0, 130, "修整器头下压力", "Condition头下压力", "kPa")]
                    public static int HeadDownPress = 0;
                }
            }
        }

        public static class Cylinder
        {
            public static class ConditionHead
            {
                [PlcHandle(Handle.CmdPolish, "气缸升")]
                public const int Up = 1140;

                [PlcHandle(Handle.CmdPolish, "气缸降")]
                public const int Down = 1141;
            }

            public static class ConditionHeadPressure
            {
                [PlcHandle(Handle.CmdPolish, "气缸设置")]
                public const int Set = 1142;

                [PlcHandle(Handle.CmdPolish, "气缸复位")]
                public const int Reset = 1143;

                public static class Param
                {
                    [Param(18232, 0, 0, 130, "ConditionPressure", "ConditionPressure", "kPa")]
                    public static int Pressure = 0;
                }
            }

            public static class PlatenBreakwater
            {
                [PlcHandle(Handle.CmdPolish, "气缸升")]
                public const int Up = 1144;

                [PlcHandle(Handle.CmdPolish, "气缸降")]
                public const int Down = 1145;
            }
        }
    }
}
