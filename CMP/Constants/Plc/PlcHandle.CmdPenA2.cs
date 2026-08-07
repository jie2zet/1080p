namespace CMP.Constants.Plc;

/// <summary>
/// PLC Handle 类 - CmdPenA2 相关定义
/// </summary>
public static partial class PlcHandle
{
    /// <summary>
    /// CmdPenA2 命令操作
    /// </summary>
    public static class CmdPenA2
    {
        /// <summary>
        /// 所有 Objects (Pen A2)
        /// </summary>
        public static class All
        {
            /// <summary>
            /// Mode 模式
            /// </summary>
            public static class Mode
            {
                [PlcHandle(Handle.CmdPenA2, "自动模式")]
                public const int Auto = 1000;
                
                [PlcHandle(Handle.CmdPenA2, "手动模式")]
                public const int Manual = 1001;
            }
            
            /// <summary>
            /// 初始化
            /// </summary>
            [PlcHandle(Handle.CmdPenA2, "初始化")]
            public const int Initialize = 1004;
            
            /// <summary>
            /// Test Recipe
            /// </summary>
            public static class TestRecipe
            {
                [PlcHandle(Handle.CmdPenA2, "Test Recipe")]
                public const int Test = 1010;
                
                [PlcHandle(Handle.CmdPenA2, "Start")]
                public const int Start = 1012;
                
                [PlcHandle(Handle.CmdPenA2, "Stop")]
                public const int Stop = 1013;
            }
        }
        
        /// <summary>
        /// Motor 电机 (Pen A2)
        /// </summary>
        public static class Motor
        {
            /// <summary>
            /// Chuck Rotation Motor 晶圆旋转电机
            /// </summary>
            public static class ChuckRotation
            {
                [PlcHandle(Handle.CmdPenA2, "运行")]
                public const int Run = 1020;
                
                [PlcHandle(Handle.CmdPenA2, "停止")]
                public const int Stop = 1021;
                
                [PlcHandle(Handle.CmdPenA2, "回原点")]
                public const int Home = 1023;
                
                [PlcHandle(Handle.CmdPenA2, "伺服", 8)]
                public const int Servo = 537;
                
                public static class Param
                {
                    [Param(33000, 60, 0, 18000, "Chuck Rotation Motor速度", "速度", "deg/s")]
                    public static int Velocity = 60;
                }
            }
            
            /// <summary>
            /// Pen Arm Sweep Motor 清洗手臂摆动电机
            /// </summary>
            public static class PenArmSweep
            {
                [PlcHandle(Handle.CmdPenA2, "任意位置")]
                public const int AnyPosition = 1030;
                
                [PlcHandle(Handle.CmdPenA2, "停止")]
                public const int Stop = 1031;
                
                [PlcHandle(Handle.CmdPenA2, "原点位置")]
                public const int Home = 1033;
                
                [PlcHandle(Handle.CmdPenA2, "边缘位置1")]
                public const int EdgePosition1 = 1034;
                
                [PlcHandle(Handle.CmdPenA2, "边缘位置2")]
                public const int EdgePosition2 = 1035;
                
                [PlcHandle(Handle.CmdPenA2, "修整位置")]
                public const int TrimPosition = 1036;
                
                [PlcHandle(Handle.CmdPenA2, "中心位置")]
                public const int CenterPosition = 1037;
                
                [PlcHandle(Handle.CmdPenA2, "伺服", 9)]
                public const int Servo = 537;
                
                public static class Param
                {
                    [Param(33020, 60, 0, 18000, "Pen Arm Sweep Motor速度", "速度", "deg/s")]
                    public static int Velocity = 60;
                    
                    [Param(33030, 0, 0, 180, "Pen Arm Sweep Motor位置", "位置", "deg")]
                    public static int Position = 0;
                }
            }
            
            /// <summary>
            /// Pen Rotation Motor 笔头旋转电机
            /// </summary>
            public static class PenRotation
            {
                [PlcHandle(Handle.CmdPenA2, "运行")]
                public const int Run = 1040;
                
                [PlcHandle(Handle.CmdPenA2, "停止")]
                public const int Stop = 1041;
                
                [PlcHandle(Handle.CmdPenA2, "伺服", 0xA)]
                public const int Servo = 537;
                
                public static class Param
                {
                    [Param(33040, 15, 0, 18000, "Pen Rotation Motor速度", "速度", "deg/s")]
                    public static int Velocity = 15;
                }
            }
        }
        
        /// <summary>
        /// Cylinder 气缸控制 (Pen A2)
        /// </summary>
        public static class Cylinder
        {
            /// <summary>
            /// Pen Up Down Cylinder 笔头升降气缸
            /// </summary>
            public static class PenUpDownCylinder
            {
                [PlcHandle(Handle.CmdPenA2, "上升")]
                public const int Up = 1092;
                
                [PlcHandle(Handle.CmdPenA2, "下降")]
                public const int Down = 1093;
            }
            
            /// <summary>
            /// Pen Pressure 笔头压力控制
            /// </summary>
            public static class PenPressure
            {
                [PlcHandle(Handle.CmdPenA2, "加压")]
                public const int Pressure = 1094;
                
                [PlcHandle(Handle.CmdPenA2, "压力重置")]
                public const int PressureReset = 1095;
                
                [PlcHandle(Handle.CmdPenA2, "EV校准")]
                public const int EVCalibration = 1096;
                
                public static class Param
                {
                    [Param(33198, 200, 0, 500, "笔头下降压力设定", "笔头下降压力设定", "kPa")]
                    public static int PressureSet = 200;
                }
            }
            
            /// <summary>
            /// Pen Arm Cylinder 摆臂气缸
            /// </summary>
            public static class PenArmCylinder
            {
                [PlcHandle(Handle.CmdPenA2, "上升")]
                public const int Up = 1113;
                
                [PlcHandle(Handle.CmdPenA2, "下降")]
                public const int Down = 1114;
            }
            
            /// <summary>
            /// WF Clamp Cylinder 晶圆夹子气缸
            /// </summary>
            public static class WFClampCylinder
            {
                [PlcHandle(Handle.CmdPenA2, "夹紧")]
                public const int Clamp = 1118;
                
                [PlcHandle(Handle.CmdPenA2, "释放")]
                public const int Release = 1119;
            }
            
            /// <summary>
            /// Nozzle Cylinder 喷嘴升降气缸
            /// </summary>
            public static class NozzleCylinder
            {
                [PlcHandle(Handle.CmdPenA2, "上升")]
                public const int Up = 1123;
                
                [PlcHandle(Handle.CmdPenA2, "下降")]
                public const int Down = 1124;
            }
            
            /// <summary>
            /// Wafer Inlet Door 入晶圆门
            /// </summary>
            public static class WaferInletDoor
            {
                [PlcHandle(Handle.CmdPenA2, "打开")]
                public const int Open = 1128;
                
                [PlcHandle(Handle.CmdPenA2, "关闭")]
                public const int Close = 1129;
            }
            
            /// <summary>
            /// Wafer Outlet Door 出晶圆门
            /// </summary>
            public static class WaferOutletDoor
            {
                [PlcHandle(Handle.CmdPenA2, "打开")]
                public const int Open = 1133;
                
                [PlcHandle(Handle.CmdPenA2, "关闭")]
                public const int Close = 1134;
            }
        }
        
        /// <summary>
        /// Gas 气体控制 (Pen A2)
        /// </summary>
        public static class Gas
        {
            /// <summary>
            /// WF Upper N2 Outlet Valve 晶圆上表面单元供应开关阀
            /// </summary>
            public static class WFUpperN2OutletValve
            {
                [PlcHandle(Handle.CmdPenA2, "打开")]
                public const int Open = 1163;
                
                [PlcHandle(Handle.CmdPenA2, "关闭")]
                public const int Close = 1164;
            }
            
            /// <summary>
            /// N2 Flow 晶圆上表面氮气流量控制
            /// </summary>
            public static class N2Flow
            {
                [PlcHandle(Handle.CmdPenA2, "流量")]
                public const int Flowrate = 1165;
                
                [PlcHandle(Handle.CmdPenA2, "流量重置")]
                public const int FlowrateReset = 1166;
                
                [PlcHandle(Handle.CmdPenA2, "MFC校准")]
                public const int MFCCalibration = 1167;
                
                public static class Param
                {
                    [Param(33152, 200, 0, 500, "晶圆上表面氮气流量设定", "晶圆上表面氮气流量设定", "L/min")]
                    public static int FlowrateSet = 200;
                }
            }
            
            /// <summary>
            /// CO2 Supply Valve CO2供应开关阀
            /// </summary>
            public static class CO2SupplyValve
            {
                [PlcHandle(Handle.CmdPenA2, "打开")]
                public const int Open = 1170;
                
                [PlcHandle(Handle.CmdPenA2, "关闭")]
                public const int Close = 1171;
            }
            
            /// <summary>
            /// CO2 Pressure CO2压力控制
            /// </summary>
            public static class CO2Pressure
            {
                [PlcHandle(Handle.CmdPenA2, "加压")]
                public const int Pressure = 1172;
                
                [PlcHandle(Handle.CmdPenA2, "压力重置")]
                public const int PressureReset = 1173;
                
                [PlcHandle(Handle.CmdPenA2, "EV校准")]
                public const int EVCalibration = 1174;
                
                public static class Param
                {
                    [Param(33150, 450, 5, 900, "CO2压力设定", "CO2压力设定", "kPa")]
                    public static int PressureSet = 450;
                }
            }
            
            /// <summary>
            /// CUP Purge Valves CUP冲洗开关阀
            /// </summary>
            public static class CUPPurgeValves
            {
                [PlcHandle(Handle.CmdPenA2, "打开")]
                public const int Open = 1217;
                
                [PlcHandle(Handle.CmdPenA2, "关闭")]
                public const int Close = 1218;
            }
            
            /// <summary>
            /// WF Lower Face N2 Outlet Valve 晶圆下表面单元供应开关阀
            /// </summary>
            public static class WFLowerFaceN2OutletValve
            {
                [PlcHandle(Handle.CmdPenA2, "打开")]
                public const int Open = 1219;
                
                [PlcHandle(Handle.CmdPenA2, "关闭")]
                public const int Close = 1220;
            }
        }
        
        /// <summary>
        /// Liquid 液体控制 (Pen A2)
        /// </summary>
        public static class Liquid
        {
            /// <summary>
            /// Chemical Inlet Valve 药液供应(流量控制器前)混合前阀
            /// </summary>
            public static class ChemicalInletValve
            {
                [PlcHandle(Handle.CmdPenA2, "打开")]
                public const int Open = 1279;
                
                [PlcHandle(Handle.CmdPenA2, "关闭")]
                public const int Close = 1280;
            }
            
            /// <summary>
            /// Chemical Outlet Valve 药液供应(流量控制器后)混合前阀
            /// </summary>
            public static class ChemicalOutletValve
            {
                [PlcHandle(Handle.CmdPenA2, "打开")]
                public const int Open = 1281;
                
                [PlcHandle(Handle.CmdPenA2, "关闭")]
                public const int Close = 1282;
            }
            
            /// <summary>
            /// Chemical Flow 药液流量控制(混合前)
            /// </summary>
            public static class ChemicalFlow
            {
                [PlcHandle(Handle.CmdPenA2, "流量")]
                public const int Flowrate = 1283;
                
                [PlcHandle(Handle.CmdPenA2, "流量重置")]
                public const int FlowrateReset = 1284;
                
                [PlcHandle(Handle.CmdPenA2, "FC校准")]
                public const int FCCalibration = 1285;
                
                public static class Param
                {
                    [Param(33128, 150, 15, 300, "药液流量设定(混合前)", "药液流量设定(混合前)", "mL/min")]
                    public static int FlowrateSet = 150;
                }
            }
            
            /// <summary>
            /// DIW Inlet Valve 纯水供应(流量控制器前)混合前阀
            /// </summary>
            public static class DIWInletValve
            {
                [PlcHandle(Handle.CmdPenA2, "打开")]
                public const int Open = 1288;
                
                [PlcHandle(Handle.CmdPenA2, "关闭")]
                public const int Close = 1289;
            }
            
            /// <summary>
            /// DIW Outlet Valve 纯水供应(流量控制器后)混合前阀
            /// </summary>
            public static class DIWOutletValve
            {
                [PlcHandle(Handle.CmdPenA2, "打开")]
                public const int Open = 1290;
                
                [PlcHandle(Handle.CmdPenA2, "关闭")]
                public const int Close = 1291;
            }
            
            /// <summary>
            /// DIW Flow 纯水流量控制(混合前)
            /// </summary>
            public static class DIWFlow
            {
                [PlcHandle(Handle.CmdPenA2, "流量")]
                public const int Flowrate = 1292;
                
                [PlcHandle(Handle.CmdPenA2, "流量重置")]
                public const int FlowrateReset = 1293;
                
                [PlcHandle(Handle.CmdPenA2, "FC校准")]
                public const int FCCalibration = 1294;
                
                public static class Param
                {
                    [Param(33120, 2000, 2000, 3500, "纯水流量设定(混合前)", "纯水流量设定(混合前)", "mL/min")]
                    public static int FlowrateSet = 2000;
                }
            }
            
            /// <summary>
            /// WF Upper Face DIW Wash Valve 晶圆上表面冲洗纯水阀
            /// </summary>
            public static class WFUpperFaceDIWWashValve
            {
                [PlcHandle(Handle.CmdPenA2, "打开")]
                public const int Open = 1297;
                
                [PlcHandle(Handle.CmdPenA2, "关闭")]
                public const int Close = 1298;
            }
            
            /// <summary>
            /// WF Upper Face DIW Flow 晶圆上表面纯水流量控制
            /// </summary>
            public static class WFUpperFaceDIWFlow
            {
                [PlcHandle(Handle.CmdPenA2, "流量")]
                public const int Flowrate = 1299;
                
                [PlcHandle(Handle.CmdPenA2, "流量重置")]
                public const int FlowrateReset = 1300;
                
                [PlcHandle(Handle.CmdPenA2, "FC校准")]
                public const int FCCalibration = 1301;
                
                public static class Param
                {
                    [Param(33122, 600, 50, 1000, "晶圆上表面纯水流量设定", "晶圆上表面纯水流量设定", "mL/min")]
                    public static int FlowrateSet = 600;
                }
            }
            
            /// <summary>
            /// WF Lower Face DIW Wash Valve 晶圆下表面纯水开关阀
            /// </summary>
            public static class WFLowerFaceDIWWashValve
            {
                [PlcHandle(Handle.CmdPenA2, "打开")]
                public const int Open = 1304;
                
                [PlcHandle(Handle.CmdPenA2, "关闭")]
                public const int Close = 1305;
            }
            
            /// <summary>
            /// WF Lower Face DIW Flow 晶圆下表面纯水流量控制
            /// </summary>
            public static class WFLowerFaceDIWFlow
            {
                [PlcHandle(Handle.CmdPenA2, "流量")]
                public const int Flowrate = 1306;
                
                [PlcHandle(Handle.CmdPenA2, "流量重置")]
                public const int FlowrateReset = 1307;
                
                [PlcHandle(Handle.CmdPenA2, "FC校准")]
                public const int FCCalibration = 1308;
                
                public static class Param
                {
                    [Param(33126, 500, 50, 1000, "晶圆下表面纯水流量设定", "晶圆下表面纯水流量设定", "mL/min")]
                    public static int FlowrateSet = 500;
                }
            }
            
            /// <summary>
            /// DIW Supply Valve 碳酸水混合前纯水供应阀
            /// </summary>
            public static class DIWSupplyValve
            {
                [PlcHandle(Handle.CmdPenA2, "打开")]
                public const int Open = 1311;
                
                [PlcHandle(Handle.CmdPenA2, "关闭")]
                public const int Close = 1312;
            }
            
            /// <summary>
            /// DIW Pressure 纯水压力控制
            /// </summary>
            public static class DIWPressure
            {
                [PlcHandle(Handle.CmdPenA2, "加压")]
                public const int Pressure = 1313;
                
                [PlcHandle(Handle.CmdPenA2, "压力重置")]
                public const int PressureReset = 1314;
                
                [PlcHandle(Handle.CmdPenA2, "EV校准")]
                public const int EVCalibration = 1315;
                
                public static class Param
                {
                    [Param(33124, 300, 50, 500, "碳酸水混合前纯水压力设定", "碳酸水混合前纯水压力设定", "kPa")]
                    public static int PressureSet = 300;
                }
            }
            
            /// <summary>
            /// Pen DIW Wash Valve 笔头清洗纯水阀
            /// </summary>
            public static class PenDIWWashValve
            {
                [PlcHandle(Handle.CmdPenA2, "打开")]
                public const int Open = 1346;
                
                [PlcHandle(Handle.CmdPenA2, "关闭")]
                public const int Close = 1347;
            }
            
            /// <summary>
            /// Quartz DIW Wash Valve 石英冲洗纯水阀
            /// </summary>
            public static class QuartzDIWWashValve
            {
                [PlcHandle(Handle.CmdPenA2, "打开")]
                public const int Open = 1348;
                
                [PlcHandle(Handle.CmdPenA2, "关闭")]
                public const int Close = 1349;
            }
            
            /// <summary>
            /// Chemical Pipeline Wash Valve 药液管道冲洗阀
            /// </summary>
            public static class ChemicalPipelineWashValve
            {
                [PlcHandle(Handle.CmdPenA2, "打开")]
                public const int Open = 1350;
                
                [PlcHandle(Handle.CmdPenA2, "关闭")]
                public const int Close = 1351;
            }
            
            /// <summary>
            /// Quartz Chemical Wash Valve 石英板冲洗药液阀
            /// </summary>
            public static class QuartzChemicalWashValve
            {
                [PlcHandle(Handle.CmdPenA2, "打开")]
                public const int Open = 1352;
                
                [PlcHandle(Handle.CmdPenA2, "关闭")]
                public const int Close = 1353;
            }
            
            /// <summary>
            /// WF Upper&Lower Faces Chemical Wash Valve 晶圆上下表面冲洗药液总阀
            /// </summary>
            public static class WFUpperLowerFacesChemicalWashValve
            {
                [PlcHandle(Handle.CmdPenA2, "打开")]
                public const int Open = 1354;
                
                [PlcHandle(Handle.CmdPenA2, "关闭")]
                public const int Close = 1355;
            }
            
            /// <summary>
            /// WF Upper Faces Chemical Wash Valve 晶圆上表面冲洗药液阀
            /// </summary>
            public static class WFUpperFacesChemicalWashValve
            {
                [PlcHandle(Handle.CmdPenA2, "打开")]
                public const int Open = 1356;
                
                [PlcHandle(Handle.CmdPenA2, "关闭")]
                public const int Close = 1357;
            }
            
            /// <summary>
            /// DIW Chamber Valve PEN纯水槽内清洗阀
            /// </summary>
            public static class DIWChamberValve
            {
                [PlcHandle(Handle.CmdPenA2, "打开")]
                public const int Open = 1358;
                
                [PlcHandle(Handle.CmdPenA2, "关闭")]
                public const int Close = 1359;
            }
            
            /// <summary>
            /// WF Upper Face DI-CO2 Wash Valve 晶圆上表面碳酸水(混合后)冲洗阀
            /// </summary>
            public static class WFUpperFaceDICO2WashValve
            {
                [PlcHandle(Handle.CmdPenA2, "打开")]
                public const int Open = 1360;
                
                [PlcHandle(Handle.CmdPenA2, "关闭")]
                public const int Close = 1361;
            }
        }
    }
}

