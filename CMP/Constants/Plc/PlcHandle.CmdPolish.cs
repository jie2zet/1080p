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
            public static class Slurry1
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1260;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1261;

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
                    [Param(18172, 100, 0, 1000, "Slurry1/DIW流量", "Slurry1/DIW流量", "mL/min")]
                    public static int Flowrate = 100;
                }
            }

            /// <summary>
            /// Slurry2/DIW 研磨液2/去离子水
            /// </summary>
            public static class Slurry2
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1262;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1263;

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
                    [Param(18174, 100, 0, 1000, "Slurry2/DIW流量", "Slurry2/DIW流量", "mL/min")]
                    public static int Flowrate = 100;
                }
            }

            /// <summary>
            /// Slurry3/DIW 研磨液3/去离子水
            /// </summary>
            public static class Slurry3
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1264;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1265;

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
                    [Param(18176, 100, 0, 1000, "Slurry3/DIW流量", "Slurry3/DIW流量", "mL/min")]
                    public static int Flowrate = 100;
                }
            }

            /// <summary>
            /// DIW Outlet Valve 去离子水出口阀
            /// </summary>
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
            public static class PlatenMotorCoolWaterValve
            {
                [PlcHandle(Handle.CmdPolish, "打开")]
                public const int Open = 1302;

                [PlcHandle(Handle.CmdPolish, "关闭")]
                public const int Close = 1303;
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
