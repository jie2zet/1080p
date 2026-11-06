namespace CMP.Constants.Plc;

/// <summary>
/// PLC Handle 类 - CmdRollA1 相关定义
/// </summary>
public static partial class PlcHandle
{
    /// <summary>
    /// CmdRollA1 命令操作
    /// </summary>
    public static class CmdRollA1
    {
        /// <summary>
        /// 所有 Objects (Roll A1)
        /// </summary>
        public static class All
        {
            /// <summary>
            /// Mode 模式
            /// </summary>
            public static class Mode
            {
                [PlcHandle(Handle.CmdRollA1, "自动模式")]
                public const int Auto = 1000;
                
                [PlcHandle(Handle.CmdRollA1, "手动模式")]
                public const int Manual = 1001;
            }
            
            /// <summary>
            /// 初始化
            /// </summary>
            [PlcHandle(Handle.CmdRollA1, "初始化")]
            public const int Initialize = 1004;
            
            /// <summary>
            /// Test Recipe
            /// </summary>
            public static class TestRecipe
            {
                [PlcHandle(Handle.CmdRollA1, "Test Recipe")]
                public const int Test = 1010;
                
                [PlcHandle(Handle.CmdRollA1, "Start")]
                public const int Start = 1012;
                
                [PlcHandle(Handle.CmdRollA1, "Stop")]
                public const int Stop = 1013;
            }
        }
        
        /// <summary>
        /// Motor 电机 (Roll A1)
        /// </summary>
        public static class Motor
        {
            /// <summary>
            /// WF Left Pillar Rotate Motor 左支柱旋转电机
            /// </summary>
            public static class WfLeftPillarRotate
            {
                [PlcHandle(Handle.CmdRollA1, "运行")]
                public const int Run = 1020;
                
                [PlcHandle(Handle.CmdRollA1, "停止")]
                public const int Stop = 1021;
                
                [PlcHandle(Handle.CmdRollA1, "Servo On", 0)]
                public const int ServoOn = 534;

                [PlcHandle(Handle.CmdRollA1, "Servo Off", 0)]
                public const int ServoOff = 534;

                public static class Param
                {
                    [Param(18000, 60, 0, 200, "WF Left Pillar Rotate Motor速度", "速度", "r/min")]
                    public static int Velocity = 60;
                }
            }
            
            /// <summary>
            /// WF Right Pillar Rotate Motor 右支柱旋转电机
            /// </summary>
            public static class WfRightPillarRotate
            {
                [PlcHandle(Handle.CmdRollA1, "运行")]
                public const int Run = 1030;
                
                [PlcHandle(Handle.CmdRollA1, "停止")]
                public const int Stop = 1031;
                
                [PlcHandle(Handle.CmdRollA1, "Servo On", 1)]
                public const int ServoOn = 534;

                [PlcHandle(Handle.CmdRollA1, "Servo Off", 1)]
                public const int ServoOff = 534;

                public static class Param
                {
                    [Param(18020, 60, 0, 200, "WF Right Pillar Rotate Motor速度", "速度", "r/min")]
                    public static int Velocity = 60;
                }
            }
            
            /// <summary>
            /// Upper Roller Rotate Motor 上辊旋转电机
            /// </summary>
            public static class UpperRollerRotate
            {
                [PlcHandle(Handle.CmdRollA1, "运行")]
                public const int Run = 1040;
                
                [PlcHandle(Handle.CmdRollA1, "停止")]
                public const int Stop = 1041;
                
                [PlcHandle(Handle.CmdRollA1, "Servo On", 2)]
                public const int ServoOn = 534;

                [PlcHandle(Handle.CmdRollA1, "Servo Off", 2)]
                public const int ServoOff = 534;

                public static class Param
                {
                    [Param(18040, 60, 0, 200, "Upper Roller Rotate Motor速度", "速度", "r/min")]
                    public static int Velocity = 60;
                }
            }
            
            /// <summary>
            /// Lower Roller Rotate Motor 下辊旋转电机
            /// </summary>
            public static class LowerRollerRotate
            {
                [PlcHandle(Handle.CmdRollA1, "运行")]
                public const int Run = 1050;
                
                [PlcHandle(Handle.CmdRollA1, "停止")]
                public const int Stop = 1051;
                
                [PlcHandle(Handle.CmdRollA1, "Servo On", 3)]
                public const int ServoOn = 534;

                [PlcHandle(Handle.CmdRollA1, "Servo Off", 3)]
                public const int ServoOff = 534;

                public static class Param
                {
                    [Param(18060, 60, 0, 200, "Lower Roller Rotate Motor速度", "速度", "r/min")]
                    public static int Velocity = 60;
                }
            }
        }
        
        /// <summary>
        /// Cylinder 气缸控制 (Roll A1)
        /// </summary>
        public static class Cylinder
        {
            /// <summary>
            /// Lower Roller Cylinder 下辊气缸
            /// </summary>
            public static class LowerRollerCylinder
            {
                [PlcHandle(Handle.CmdRollA1, "上升")]
                public const int Up = 1100;
                
                [PlcHandle(Handle.CmdRollA1, "下降")]
                public const int Down = 1101;
            }
            
            /// <summary>
            /// Lower Roller Pressure 下辊压力
            /// </summary>
            public static class LowerRollerPressure
            {
                [PlcHandle(Handle.CmdRollA1, "加压")]
                public const int Pressure = 1102;
                
                [PlcHandle(Handle.CmdRollA1, "压力重置")]
                public const int PressureReset = 1103;
                
                [PlcHandle(Handle.CmdRollA1, "压力校准")]
                public const int PressureCalibration = 1104;

                public static class Param
                {
                    [Param(18198, 300, 0, 500, "下辊压力设定", "下辊压力设定", "kPa")]
                    public static int PressureSet = 300;
                }
            }
            
            /// <summary>
            /// Upper Roller Cylinder 上辊气缸
            /// </summary>
            public static class UpperRollerCylinder
            {
                [PlcHandle(Handle.CmdRollA1, "上升")]
                public const int Up = 1105;
                
                [PlcHandle(Handle.CmdRollA1, "下降")]
                public const int Down = 1106;
            }
            
            /// <summary>
            /// Upper Roller Pressure 上辊压力
            /// </summary>
            public static class UpperRollerPressure
            {
                [PlcHandle(Handle.CmdRollA1, "加压")]
                public const int Pressure = 1107;
                
                [PlcHandle(Handle.CmdRollA1, "压力重置")]
                public const int PressureReset = 1108;
                
                [PlcHandle(Handle.CmdRollA1, "压力校准")]
                public const int PressureCalibration = 1109;

                public static class Param
                {
                    [Param(18200, 300, 0, 500, "上辊压力设定", "上辊压力设定", "kPa")]
                    public static int PressureSet = 300;
                }
            }
            
            /// <summary>
            /// Arm Up And Down 机械臂升降
            /// </summary>
            public static class ArmUpDown
            {
                [PlcHandle(Handle.CmdRollA1, "上升")]
                public const int Up = 1110;
                
                [PlcHandle(Handle.CmdRollA1, "下降")]
                public const int Down = 1111;
            }
            
            /// <summary>
            /// Arm Lateral Movement 机械臂横向移动
            /// </summary>
            public static class ArmLateralMovement
            {
                [PlcHandle(Handle.CmdRollA1, "等待")]
                public const int Waiting = 1115;
                
                [PlcHandle(Handle.CmdRollA1, "清洁")]
                public const int Cleaning = 1116;
            }
            
            /// <summary>
            /// Clamp Cylinder 夹紧气缸
            /// </summary>
            public static class ClampCylinder
            {
                [PlcHandle(Handle.CmdRollA1, "释放")]
                public const int Release = 1120;
                
                [PlcHandle(Handle.CmdRollA1, "夹紧")]
                public const int Clamp = 1121;
            }
            
            /// <summary>
            /// Clamp Pressure 夹紧压力
            /// </summary>
            public static class ClampPressure
            {
                [PlcHandle(Handle.CmdRollA1, "加压")]
                public const int Pressure = 1122;
                
                [PlcHandle(Handle.CmdRollA1, "压力重置")]
                public const int PressureReset = 1123;
                
                [PlcHandle(Handle.CmdRollA1, "压力校准")]
                public const int PressureCalibration = 1124;

                public static class Param
                {
                    [Param(18202, 300, 0, 500, "夹紧压力设定", "夹紧压力设定", "kPa")]
                    public static int PressureSet = 300;
                }
            }
            
            /// <summary>
            /// Ultrasonic Nozzle 超声波喷嘴
            /// </summary>
            public static class UltrasonicNozzle
            {
                [PlcHandle(Handle.CmdRollA1, "上升")]
                public const int Up = 1125;
                
                [PlcHandle(Handle.CmdRollA1, "下降")]
                public const int Down = 1126;
            }
            
            /// <summary>
            /// Wafer Inlet Door 晶圆入口门
            /// </summary>
            public static class WaferInletDoor
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1130;
                
                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1131;
            }
            
            /// <summary>
            /// Arm Door 机械臂门
            /// </summary>
            public static class ArmDoor
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1135;
                
                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1136;
            }
            
            /// <summary>
            /// Wafer Outlet Door 晶圆出口门
            /// </summary>
            public static class WaferOutletDoor
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1140;
                
                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1141;
            }
        }
        
        /// <summary>
        /// Liquid 液体控制 (Roll A1)
        /// </summary>
        public static class Liquid
        {
            /// <summary>
            /// Chemical Enter Main Valve 化学品进入主阀
            /// </summary>
            [LiquidControl("ChemicalEnterMainValve", "FV-ROAC100A")]
            public static class ChemicalEnterMainValve
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1210;
                
                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1211;
            }
            
            /// <summary>
            /// Chemical Enter Flow 化学品进入流量
            /// </summary>
            [LiquidControl("ChemicalEnterFlow", "FC-ROAC100")]
            public static class ChemicalEnterFlow
            {
                [PlcHandle(Handle.CmdRollA1, "流量")]
                public const int Flowrate = 1212;
                
                [PlcHandle(Handle.CmdRollA1, "流量重置")]
                public const int FlowrateReset = 1213;
                
                [PlcHandle(Handle.CmdRollA1, "CLFC校准")]
                public const int ClfcCalibration = 1214;

                public static class Param
                {
                    [Param(18118, 100.00, 15.00, 500.00, "药液流量控制(混合前)", "药液流量控制(混合前)", "mL/min", DecimalPlaces = 2, DrawingTag = "FC-ROAC100", ZrZf = "ZF", WordCount = 2, DisplayNameKey = "ChemicalEnterFlowSet")]
                    public static int FlowrateSet = 100;
                }
            }
            
            /// <summary>
            /// Pipe DIW Wash 管道去离子水清洗
            /// </summary>
            [LiquidControl("PipeDiwWash", "SV-ROAC100B")]
            public static class PipeDiwWash
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1215;
                
                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1216;
            }
            
            /// <summary>
            /// DIW Enter Main Valve 去离子水进入主阀
            /// </summary>
            [LiquidControl("DiwEnterMainValve", "FV-ROAC101")]
            public static class DiwEnterMainValve
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1217;
                
                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1218;
            }
            
            /// <summary>
            /// DIW Enter Flow 去离子水进入流量
            /// </summary>
            [LiquidControl("DiwEnterFlow", "FC-ROAC101")]
            public static class DiwEnterFlow
            {
                [PlcHandle(Handle.CmdRollA1, "流量")]
                public const int Flowrate = 1219;
                
                [PlcHandle(Handle.CmdRollA1, "流量重置")]
                public const int FlowrateReset = 1220;
                
                [PlcHandle(Handle.CmdRollA1, "CLFC校准")]
                public const int ClfcCalibration = 1221;

                public static class Param
                {
                    [Param(18120, 1000.00, 200.00, 9999.00, "纯水流量控制(混合前)", "纯水流量控制(混合前)", "mL/min", DecimalPlaces = 2, DrawingTag = "FC-ROAC101", ZrZf = "ZF", WordCount = 2, DisplayNameKey = "DiwEnterFlowSet")]
                    public static int FlowrateSet = 1000;
                }
            }
            
            /// <summary>
            /// WF Lower Face Chemical Wash 晶圆下表面化学清洗
            /// </summary>
            [LiquidControl("WfLowerFaceChemicalWash", "FV-ROAC100B")]
            public static class WfLowerFaceChemicalWash
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1222;
                
                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1223;
            }
            
            /// <summary>
            /// WF Upper Face Chemical Wash 晶圆上表面化学清洗
            /// </summary>
            [LiquidControl("WfUpperFaceChemicalWash", "SFV-ROAC100C")]
            public static class WfUpperFaceChemicalWash
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1224;
                
                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1225;
            }
            
            /// <summary>
            /// Quartz Chemical Wash 石英化学清洗
            /// </summary>
            [LiquidControl("QuartzChemicalWash", "FV-ROAC100D")]
            public static class QuartzChemicalWash
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1226;
                
                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1227;
            }

            /// <summary>
            /// Chemical Inlet Main Valve 1 药水进入主阀1
            /// </summary>
            [LiquidControl("ChemicalInletMainValve1", "")]
            public static class ChemicalInletMainValve1
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1228;

                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1229;
            }

            /// <summary>
            /// Chemical Inlet Main Valve 2 药水进入主阀2
            /// </summary>
            [LiquidControl("ChemicalInletMainValve2", "")]
            public static class ChemicalInletMainValve2
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1230;
            }
            
            /// <summary>
            /// WF Upper Face DIW Wash Inlet 晶圆上表面去离子水清洗入口
            /// </summary>
            [LiquidControl("WfUpperFaceDiwWashInlet", "FV-ROAC105")]
            public static class WfUpperFaceDiwWashInlet
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1242;
                
                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1243;
            }
            
            /// <summary>
            /// WF Upper Face DIW Wash Outlet 晶圆上表面去离子水清洗出口
            /// </summary>
            [LiquidControl("WfUpperFaceDiwWashOutlet", "SFV-ROAC105")]
            public static class WfUpperFaceDiwWashOutlet
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1244;
                
                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1245;
            }
            
            /// <summary>
            /// WF Upper Face Wash DIW Flow 晶圆上表面清洗去离子水流量
            /// </summary>
            [LiquidControl("WfUpperFaceWashDiwFlow", "FC-ROAC105")]
            public static class WfUpperFaceWashDiwFlow
            {
                [PlcHandle(Handle.CmdRollA1, "流量")]
                public const int Flowrate = 1246;
                
                [PlcHandle(Handle.CmdRollA1, "流量重置")]
                public const int FlowrateReset = 1247;
                
                [PlcHandle(Handle.CmdRollA1, "CLFC校准")]
                public const int ClfcCalibration = 1248;

                public static class Param
                {
                    [Param(18122, 1000.00, 200.00, 9999.00, "晶圆上表面清洁 流量控制", "晶圆上表面清洁 流量控制", "mL/min", DecimalPlaces = 2, DrawingTag = "FC-RO1AC105", ZrZf = "ZF", WordCount = 2, DisplayNameKey = "UpperRinseDiwWash")]
                    public static int FlowrateSet = 1000;
                }
            }
            
            /// <summary>
            /// WF Lower Face DIW Wash 晶圆下表面去离子水清洗
            /// </summary>
            [LiquidControl("WfLowerFaceDiwWash", "")]
            public static class WfLowerFaceDiwWash
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1263;
                
                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1264;
            }
            
            /// <summary>
            /// WF Lower Face DIW Wash Flow 晶圆下表面去离子水清洗流量
            /// </summary>
            [LiquidControl("WfLowerFaceDiwWashFlow", "")]
            public static class WfLowerFaceDiwWashFlow
            {
                [PlcHandle(Handle.CmdRollA1, "流量")]
                public const int Flowrate = 1265;
                
                [PlcHandle(Handle.CmdRollA1, "流量重置")]
                public const int FlowrateReset = 1266;
                
                [PlcHandle(Handle.CmdRollA1, "CLFC校准")]
                public const int ClfcCalibration = 1267;

                public static class Param
                {
                    [Param(18124, 1000.00, 200.00, 9999.00, "晶圆下表面清洁 流量控制", "晶圆下表面清洁 流量控制", "mL/min", DecimalPlaces = 2, DrawingTag = "FC-RO1AC106", ZrZf = "ZF", WordCount = 2, DisplayNameKey = "LowerRinseDiwWash")]
                    public static int FlowrateSet = 1000;
                }
            }
            
            /// <summary>
            /// Upper Roller DIW Wash 上辊去离子水清洗
            /// </summary>
            [LiquidControl("UpperRollerDiwWash", "FV-ROAC102")]
            public static class UpperRollerDiwWash
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1293;
                
                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1294;
            }
            
            /// <summary>
            /// Quartz DIW Wash 石英去离子水清洗
            /// </summary>
            [LiquidControl("QuartzDiwWash", "FV-ROAC103")]
            public static class QuartzDiwWash
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1295;
                
                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1296;
            }
            
            /// <summary>
            /// Upper Roller Inside DIW Wash 上辊内部去离子水清洗
            /// </summary>
            [LiquidControl("UpperRollerInsideDiwWash", "FV-ROAC104")]
            public static class UpperRollerInsideDiwWash
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1297;
                
                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1298;
            }
            
            /// <summary>
            /// Lower Roller Inside DIW Wash 下辊内部去离子水清洗
            /// </summary>
            [LiquidControl("LowerRollerInsideDiwWash", "FV-ROAC107")]
            public static class LowerRollerInsideDiwWash
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1299;
                
                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1300;
            }
            
            /// <summary>
            /// Mage Sonic DIW Wash 超声波去离子水清洗
            /// </summary>
            [LiquidControl("MageSonicDiwWash", "FV-MJROAC130")]
            public static class MageSonicDiwWash
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1301;
                
                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1302;
            }
            
            /// <summary>
            /// Chamber DIW Wash 腔室去离子水清洗
            /// </summary>
            [LiquidControl("ChamberDiwWash", "FV-ROAC120")]
            public static class ChamberDiwWash
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1303;
                
                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1304;
            }
            
            /// <summary>
            /// Pillar Wash 支柱清洗
            /// </summary>
            [LiquidControl("PillarWash", "FV-ROAC121")]
            public static class PillarWash
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1305;
                
                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1306;
            }
            
            /// <summary>
            /// Cooling Water Circulation 冷却水循环
            /// </summary>
            [LiquidControl("CoolingWaterCirculation", "FV-ROAC122")]
            public static class CoolingWaterCirculation
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1307;
                
                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1308;
            }

            /// <summary>
            /// Ultrasonic Vibration Startup 超声波发振启动
            /// </summary>
            [LiquidControl("UltrasonicVibrationStartup", "")]
            public static class UltrasonicVibrationStartup
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1309;

                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1310;

                [PlcHandle(Handle.CmdRollA1, "功率")]
                public const int Power = 1311;
            }

            /// <summary>
            /// Ultrasonic Vibration 超声波发振
            /// </summary>
            [LiquidControl("UltrasonicVibration", "")]
            public static class UltrasonicVibration
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1400;

                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1401;
            }

            /// <summary>
            /// Cylinder One-Touch Action 气缸一键动作
            /// </summary>
            [LiquidControl("CylinderOneTouchAction", "")]
            public static class CylinderOneTouchAction
            {
                [PlcHandle(Handle.CmdRollA1, "打开")]
                public const int Open = 1406;

                [PlcHandle(Handle.CmdRollA1, "关闭")]
                public const int Close = 1407;
            }
        }

        [LiquidControl("RollerManualDiwWash", CommandKeyPrefix = "RollerManualDiwWash")]
        public static class Flush
        {
            [PlcHandle(Handle.CmdRollA1, "打开全纯水阀")]
            public const int Open = 1404;

            [PlcHandle(Handle.CmdRollA1, "关闭全纯水阀")]
            public const int Close = 1405;

            public static class Param
            {
                [Param(10014, 0, 0, 999, "全纯水阀打开延时", "DIW(ALL) Open Delay", "s")]
                public static int OpenDelay = 0;
            }
        }
    }
}
