namespace CMP.Constants.Plc;

/// <summary>
/// PLC Handle 类 - CmdDryA1 相关定义
/// </summary>
public static partial class PlcHandle
{
    /// <summary>
    /// CmdDryA1 命令操作
    /// </summary>
    public static class CmdDryA
    {
        /// <summary>
        /// 所有 Objects (Dry A1)
        /// </summary>
        public static class All
        {
            /// <summary>
            /// Mode 模式
            /// </summary>
            public static class Mode
            {
                [PlcHandle(Handle.CmdDryA, "自动模式")]
                public const int Auto = 1000;

                [PlcHandle(Handle.CmdDryA, "手动模式")]
                public const int Manual = 1001;
            }

            /// <summary>
            /// 初始化
            /// </summary>
            [PlcHandle(Handle.CmdDryA, "初始化")]
            public const int Initialize = 1004;

            /// <summary>
            /// Test Recipe
            /// </summary>
            public static class TestRecipe
            {
                [PlcHandle(Handle.CmdDryA, "Test Recipe")]
                public const int Test = 1010;

                [PlcHandle(Handle.CmdDryA, "Start")]
                public const int Start = 1012;

                [PlcHandle(Handle.CmdDryA, "Stop")]
                public const int Stop = 1013;
            }
        }

        /// <summary>
        /// Motor 电机 (Dry A1)
        /// </summary>
        public static class Motor
        {
            /// <summary>
            /// Chuck Rotation Motor 晶圆旋转电机
            /// </summary>
            public static class ChuckRotation
            {
                [PlcHandle(Handle.CmdDryA, "运行")]
                public const int Run = 1020;

                [PlcHandle(Handle.CmdDryA, "停止")]
                public const int Stop = 1021;

                [PlcHandle(Handle.CmdDryA, "回原点")]
                public const int Home = 1023;

                [PlcHandle(Handle.CmdDryA, "伺服", 0)]
                public const int Servo = 539;

                public static class Param
                {
                    [Param(18000, 60, 0, 18000, "Chuck Rotation Motor速度", "速度", "deg/s")]
                    public static int Velocity = 60;
                }
            }

            /// <summary>
            /// Arm Swing Motor 手臂摆动电机
            /// </summary>
            public static class ArmSwing
            {
                [PlcHandle(Handle.CmdDryA, "任意位置")]
                public const int AnyPosition = 1030;

                [PlcHandle(Handle.CmdDryA, "停止")]
                public const int Stop = 1031;

                [PlcHandle(Handle.CmdDryA, "原点位置")]
                public const int Home = 1033;

                [PlcHandle(Handle.CmdDryA, "中心位置")]
                public const int CenterPosition = 1034;

                [PlcHandle(Handle.CmdDryA, "边缘位置1")]
                public const int EdgePosition1 = 1035;

                [PlcHandle(Handle.CmdDryA, "边缘位置2")]
                public const int EdgePosition2 = 1036;

                [PlcHandle(Handle.CmdDryA, "伺服", 1)]
                public const int Servo = 539;

                public static class Param
                {
                    [Param(18020, 60, 0, 18000, "Arm Swing Motor速度", "速度", "deg/s")]
                    public static int Velocity = 60;

                    [Param(18030, 0, 0, 180, "Arm Swing Motor位置", "位置", "deg")]
                    public static int Position = 0;
                }
            }
        }

        /// <summary>
        /// Cylinder 气缸控制 (Dry A1)
        /// </summary>
        public static class Cylinder
        {
            /// <summary>
            /// WF Clamp Cylinder 晶圆夹子气缸
            /// </summary>
            public static class WFClampCylinder
            {
                [PlcHandle(Handle.CmdDryA, "夹紧")]
                public const int Clamp = 1080;

                [PlcHandle(Handle.CmdDryA, "释放")]
                public const int Release = 1081;
            }

            /// <summary>
            /// Wafer Inlet Door 入晶圆门
            /// </summary>
            public static class WaferInletDoor
            {
                [PlcHandle(Handle.CmdDryA, "打开")]
                public const int Open = 1085;

                [PlcHandle(Handle.CmdDryA, "关闭")]
                public const int Close = 1086;
            }

            /// <summary>
            /// Wafer Outlet Door 出晶圆门
            /// </summary>
            public static class WaferOutletDoor
            {
                [PlcHandle(Handle.CmdDryA, "打开")]
                public const int Open = 1090;

                [PlcHandle(Handle.CmdDryA, "关闭")]
                public const int Close = 1091;
            }
        }

        /// <summary>
        /// Gas 气体控制 (Dry A1)
        /// </summary>
        public static class Gas
        {
            /// <summary>
            /// N2 Mix Outlet Valve 氮气感应(入罐前阀)
            /// </summary>
            public static class N2MixOutletValve
            {
                [PlcHandle(Handle.CmdDryA, "打开")]
                public const int Open = 1123;

                [PlcHandle(Handle.CmdDryA, "关闭")]
                public const int Close = 1124;
            }

            /// <summary>
            /// N2 Flow 氮气流量控制(入罐前)
            /// </summary>
            public static class N2Flow
            {
                [PlcHandle(Handle.CmdDryA, "流量")]
                public const int Flowrate = 1125;

                [PlcHandle(Handle.CmdDryA, "流量重置")]
                public const int FlowrateReset = 1126;

                [PlcHandle(Handle.CmdDryA, "FC校准")]
                public const int FCCalibration = 1127;

                public static class Param
                {
                    [Param(18132, 8000, 0, 10000, "氮气流量控制(入罐前)", "氮气流量控制(入罐前)", "mL/min")]
                    public static int FlowrateSet = 8000;
                }
            }

            /// <summary>
            /// WF Upper Face N2 Outlet Valve 氮气供应吹晶圆上表面阀
            /// </summary>
            public static class WFUpperFaceN2OutletValve
            {
                [PlcHandle(Handle.CmdDryA, "打开")]
                public const int Open = 1130;

                [PlcHandle(Handle.CmdDryA, "关闭")]
                public const int Close = 1131;
            }

            /// <summary>
            /// N2 Flow 氮气流量控制吹晶圆
            /// </summary>
            public static class N2FlowWafer
            {
                [PlcHandle(Handle.CmdDryA, "流量")]
                public const int Flowrate = 1132;

                [PlcHandle(Handle.CmdDryA, "流量重置")]
                public const int FlowrateReset = 1133;

                [PlcHandle(Handle.CmdDryA, "FC校准")]
                public const int FCCalibration = 1134;

                public static class Param
                {
                    [Param(18134, 10000, 0, 10000, "氮气流量控制(吹晶圆)", "氮气流量控制(吹晶圆)", "mL/min")]
                    public static int FlowrateSet = 10000;
                }
            }

            /// <summary>
            /// WF Lower Face N2 Outlet Valve 1 氮气1路晶圆下表面吹氮气阀
            /// </summary>
            public static class WFLowerFaceN2OutletValve1
            {
                [PlcHandle(Handle.CmdDryA, "打开")]
                public const int Open = 1137;

                [PlcHandle(Handle.CmdDryA, "关闭")]
                public const int Close = 1138;
            }

            /// <summary>
            /// N2 Flow 晶圆下表面吹氮气流量控制
            /// </summary>
            public static class N2FlowLowerFace
            {
                [PlcHandle(Handle.CmdDryA, "流量")]
                public const int Flowrate = 1139;

                [PlcHandle(Handle.CmdDryA, "流量重置")]
                public const int FlowrateReset = 1140;

                [PlcHandle(Handle.CmdDryA, "MFC校准")]
                public const int MFCCalibration = 1141;

                public static class Param
                {
                    [Param(18136, 40000, 0, 100000, "晶圆下表面吹氮气流量控制", "晶圆下表面吹氮气流量控制", "mL/min")]
                    public static int FlowrateSet = 40000;
                }
            }

            /// <summary>
            /// N2 Release Valves 罐内充氮气排压阀
            /// </summary>
            public static class N2ReleaseValves
            {
                [PlcHandle(Handle.CmdDryA, "打开")]
                public const int Open = 1179;

                [PlcHandle(Handle.CmdDryA, "关闭")]
                public const int Close = 1180;
            }

            /// <summary>
            /// WF Upper Face IPA Outlet Valve IPA气体供应阀(出罐)吹晶圆上表面
            /// </summary>
            public static class WFUpperFaceIPAOutletValve
            {
                [PlcHandle(Handle.CmdDryA, "打开")]
                public const int Open = 1181;

                [PlcHandle(Handle.CmdDryA, "关闭")]
                public const int Close = 1182;
            }

            /// <summary>
            /// WF Lower Face N2 Outlet Valve 2 氮气2路晶圆下表面吹氮气阀
            /// </summary>
            public static class WFLowerFaceN2OutletValve2
            {
                [PlcHandle(Handle.CmdDryA, "打开")]
                public const int Open = 1183;

                [PlcHandle(Handle.CmdDryA, "关闭")]
                public const int Close = 1184;
            }
        }

        /// <summary>
        /// Liquid 液体控制 (Dry A1)
        /// </summary>
        public static class Liquid
        {
            /// <summary>
            /// WF Upper Face DIW Wash Inlet Valve 2 纯水2路晶圆上表面冲洗阀流量控制器前
            /// </summary>
            public static class WFUpperFaceDIWWashInletValve2
            {
                [PlcHandle(Handle.CmdDryA, "打开")]
                public const int Open = 1279;

                [PlcHandle(Handle.CmdDryA, "关闭")]
                public const int Close = 1280;
            }

            /// <summary>
            /// WF Upper Face DIW Wash Outlet Valve 2 纯水2路晶圆上表面冲洗阀流量控制器后
            /// </summary>
            public static class WFUpperFaceDIWWashOutletValve2
            {
                [PlcHandle(Handle.CmdDryA, "打开")]
                public const int Open = 1281;

                [PlcHandle(Handle.CmdDryA, "关闭")]
                public const int Close = 1282;
            }

            /// <summary>
            /// WF Upper Face DIW Wash Flow 2 晶圆上表面冲洗流量控制
            /// </summary>
            public static class WFUpperFaceDIWWashFlow2
            {
                [PlcHandle(Handle.CmdDryA, "流量")]
                public const int Flowrate = 1283;

                [PlcHandle(Handle.CmdDryA, "流量重置")]
                public const int FlowrateReset = 1284;

                [PlcHandle(Handle.CmdDryA, "FC校准")]
                public const int FCCalibration = 1285;

                public static class Param
                {
                    [Param(18100, 600, 50, 1000, "纯水2路晶圆上表面冲洗流量设定", "纯水2路晶圆上表面冲洗流量设定", "mL/min")]
                    public static int FlowrateSet = 600;
                }
            }

            /// <summary>
            /// IPA Supply Valve IPA供应(入罐前)阀
            /// </summary>
            public static class IPASupplyValve
            {
                [PlcHandle(Handle.CmdDryA, "打开")]
                public const int Open = 1344;

                [PlcHandle(Handle.CmdDryA, "关闭")]
                public const int Close = 1345;
            }

            /// <summary>
            /// WF Upper Face DIW Wash Inlet Valve 1 纯水1路晶圆上表面冲洗阀流量控制器前
            /// </summary>
            public static class WFUpperFaceDIWWashInletValve1
            {
                [PlcHandle(Handle.CmdDryA, "打开")]
                public const int Open = 1346;

                [PlcHandle(Handle.CmdDryA, "关闭")]
                public const int Close = 1347;
            }

            /// <summary>
            /// WF Upper Face DIW Wash Outlet Valve 1 纯水1路晶圆上表面冲洗阀流量控制器后
            /// </summary>
            public static class WFUpperFaceDIWWashOutletValve1
            {
                [PlcHandle(Handle.CmdDryA, "打开")]
                public const int Open = 1348;

                [PlcHandle(Handle.CmdDryA, "关闭")]
                public const int Close = 1349;
            }

            /// <summary>
            /// WF Lower Face DIW Wash Valve 纯水3路晶圆下表面冲洗阀
            /// </summary>
            public static class WFLowerFaceDIWWashValve
            {
                [PlcHandle(Handle.CmdDryA, "打开")]
                public const int Open = 1350;

                [PlcHandle(Handle.CmdDryA, "关闭")]
                public const int Close = 1351;
            }
        }
    }
}


