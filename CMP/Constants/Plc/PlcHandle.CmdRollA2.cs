namespace CMP.Constants.Plc;

/// <summary>
/// PLC Handle 类 - CmdRollA2 相关定义
/// </summary>
public static partial class PlcHandle
{
    /// <summary>
    /// CmdRollA2 命令操作
    /// </summary>
    public static class CmdRollA2
    {
        /// <summary>
        /// 所有 Objects (Roll A2)
        /// </summary>
        public static class All
        {
            /// <summary>
            /// Mode 模式
            /// </summary>
            public static class Mode
            {
                [PlcHandle(Handle.CmdRollA2, "自动模式")]
                public const int Auto = 1000;
                
                [PlcHandle(Handle.CmdRollA2, "手动模式")]
                public const int Manual = 1001;
            }
            
            /// <summary>
            /// 初始化
            /// </summary>
            [PlcHandle(Handle.CmdRollA2, "初始化")]
            public const int Initialize = 1004;
            
            /// <summary>
            /// Test Recipe
            /// </summary>
            public static class TestRecipe
            {
                [PlcHandle(Handle.CmdRollA2, "Test Recipe")]
                public const int Test = 1010;
                
                [PlcHandle(Handle.CmdRollA2, "Start")]
                public const int Start = 1012;
                
                [PlcHandle(Handle.CmdRollA2, "Stop")]
                public const int Stop = 1013;
            }
        }
        
        /// <summary>
        /// Motor 电机 (Roll A2)
        /// </summary>
        public static class Motor
        {
            /// <summary>
            /// WF Left Pillar Rotate Motor 左支柱旋转电机
            /// </summary>
            public static class WfLeftPillarRotate
            {
                [PlcHandle(Handle.CmdRollA2, "运行")]
                public const int Run = 1020;
                
                [PlcHandle(Handle.CmdRollA2, "停止")]
                public const int Stop = 1021;
                
                [PlcHandle(Handle.CmdRollA2, "伺服", 8)]
                public const int Servo = 534;

                public static class Param
                {
                    [Param(33000, 60, 0, 200, "WF Left Pillar Rotate Motor速度", "速度", "r/min")]
                    public static int Velocity = 60;
                }
            }
            
            /// <summary>
            /// WF Right Pillar Rotate Motor 右支柱旋转电机
            /// </summary>
            public static class WfRightPillarRotate
            {
                [PlcHandle(Handle.CmdRollA2, "运行")]
                public const int Run = 1030;
                
                [PlcHandle(Handle.CmdRollA2, "停止")]
                public const int Stop = 1031;
                
                [PlcHandle(Handle.CmdRollA2, "伺服", 9)]
                public const int Servo = 534;

                public static class Param
                {
                    [Param(33020, 60, 0, 200, "WF Right Pillar Rotate Motor速度", "速度", "r/min")]
                    public static int Velocity = 60;
                }
            }
            
            /// <summary>
            /// Upper Roller Rotate Motor 上辊旋转电机
            /// </summary>
            public static class UpperRollerRotate
            {
                [PlcHandle(Handle.CmdRollA2, "运行")]
                public const int Run = 1040;
                
                [PlcHandle(Handle.CmdRollA2, "停止")]
                public const int Stop = 1041;
                
                [PlcHandle(Handle.CmdRollA2, "伺服", 0xA)]
                public const int Servo = 534;

                public static class Param
                {
                    [Param(33040, 60, 0, 200, "Upper Roller Rotate Motor速度", "速度", "r/min")]
                    public static int Velocity = 60;
                }
            }
            
            /// <summary>
            /// Lower Roller Rotate Motor 下辊旋转电机
            /// </summary>
            public static class LowerRollerRotate
            {
                [PlcHandle(Handle.CmdRollA2, "运行")]
                public const int Run = 1050;
                
                [PlcHandle(Handle.CmdRollA2, "停止")]
                public const int Stop = 1051;
                
                [PlcHandle(Handle.CmdRollA2, "伺服", 0xB)]
                public const int Servo = 534;

                public static class Param
                {
                    [Param(33060, 60, 0, 200, "Lower Roller Rotate Motor速度", "速度", "r/min")]
                    public static int Velocity = 60;
                }
            }
        }
        
        /// <summary>
        /// Cylinder 气缸控制 (Roll A2)
        /// </summary>
        public static class Cylinder
        {
            /// <summary>
            /// Lower Roller Cylinder 下辊气缸
            /// </summary>
            public static class LowerRollerCylinder
            {
                [PlcHandle(Handle.CmdRollA2, "上升")]
                public const int Up = 1100;
                
                [PlcHandle(Handle.CmdRollA2, "下降")]
                public const int Down = 1101;
            }
            
            /// <summary>
            /// Lower Roller Pressure 下辊压力
            /// </summary>
            public static class LowerRollerPressure
            {
                [PlcHandle(Handle.CmdRollA2, "加压")]
                public const int Pressure = 1102;
                
                [PlcHandle(Handle.CmdRollA2, "压力重置")]
                public const int PressureReset = 1103;
                
                [PlcHandle(Handle.CmdRollA2, "压力校准")]
                public const int PressureCalibration = 1104;

                public static class Param
                {
                    [Param(33198, 300, 0, 500, "下辊压力设定", "下辊压力设定", "kPa")]
                    public static int PressureSet = 300;
                }
            }
            
            /// <summary>
            /// Upper Roller Cylinder 上辊气缸
            /// </summary>
            public static class UpperRollerCylinder
            {
                [PlcHandle(Handle.CmdRollA2, "上升")]
                public const int Up = 1105;
                
                [PlcHandle(Handle.CmdRollA2, "下降")]
                public const int Down = 1106;
            }
            
            /// <summary>
            /// Upper Roller Pressure 上辊压力
            /// </summary>
            public static class UpperRollerPressure
            {
                [PlcHandle(Handle.CmdRollA2, "加压")]
                public const int Pressure = 1107;
                
                [PlcHandle(Handle.CmdRollA2, "压力重置")]
                public const int PressureReset = 1108;
                
                [PlcHandle(Handle.CmdRollA2, "压力校准")]
                public const int PressureCalibration = 1109;

                public static class Param
                {
                    [Param(33200, 300, 0, 500, "上辊压力设定", "上辊压力设定", "kPa")]
                    public static int PressureSet = 300;
                }
            }
            
            /// <summary>
            /// Arm Up And Down 机械臂升降
            /// </summary>
            public static class ArmUpDown
            {
                [PlcHandle(Handle.CmdRollA2, "上升")]
                public const int Up = 1110;
                
                [PlcHandle(Handle.CmdRollA2, "下降")]
                public const int Down = 1111;
            }
            
            /// <summary>
            /// Arm Lateral Movement 机械臂横向移动
            /// </summary>
            public static class ArmLateralMovement
            {
                [PlcHandle(Handle.CmdRollA2, "等待")]
                public const int Waiting = 1115;
                
                [PlcHandle(Handle.CmdRollA2, "清洁")]
                public const int Cleaning = 1116;
            }
            
            /// <summary>
            /// Clamp Cylinder 夹紧气缸
            /// </summary>
            public static class ClampCylinder
            {
                [PlcHandle(Handle.CmdRollA2, "释放")]
                public const int Release = 1120;
                
                [PlcHandle(Handle.CmdRollA2, "夹紧")]
                public const int Clamp = 1121;
            }
            
            /// <summary>
            /// Clamp Pressure 夹紧压力
            /// </summary>
            public static class ClampPressure
            {
                [PlcHandle(Handle.CmdRollA2, "加压")]
                public const int Pressure = 1122;
                
                [PlcHandle(Handle.CmdRollA2, "压力重置")]
                public const int PressureReset = 1123;
                
                [PlcHandle(Handle.CmdRollA2, "压力校准")]
                public const int PressureCalibration = 1124;

                public static class Param
                {
                    [Param(33202, 300, 0, 500, "夹紧压力设定", "夹紧压力设定", "kPa")]
                    public static int PressureSet = 300;
                }
            }
            
            /// <summary>
            /// Ultrasonic Nozzle 超声波喷嘴
            /// </summary>
            public static class UltrasonicNozzle
            {
                [PlcHandle(Handle.CmdRollA2, "上升")]
                public const int Up = 1125;
                
                [PlcHandle(Handle.CmdRollA2, "下降")]
                public const int Down = 1126;
            }
            
            /// <summary>
            /// Wafer Inlet Door 晶圆入口门
            /// </summary>
            public static class WaferInletDoor
            {
                [PlcHandle(Handle.CmdRollA2, "打开")]
                public const int Open = 1130;
                
                [PlcHandle(Handle.CmdRollA2, "关闭")]
                public const int Close = 1131;
            }
            
            /// <summary>
            /// Arm Door 机械臂门
            /// </summary>
            public static class ArmDoor
            {
                [PlcHandle(Handle.CmdRollA2, "打开")]
                public const int Open = 1135;
                
                [PlcHandle(Handle.CmdRollA2, "关闭")]
                public const int Close = 1136;
            }
            
            /// <summary>
            /// Wafer Outlet Door 晶圆出口门
            /// </summary>
            public static class WaferOutletDoor
            {
                [PlcHandle(Handle.CmdRollA2, "打开")]
                public const int Open = 1140;
                
                [PlcHandle(Handle.CmdRollA2, "关闭")]
                public const int Close = 1141;
            }
        }
        
        /// <summary>
        /// Liquid 液体控制 (Roll A2)
        /// </summary>
        public static class Liquid
        {
            /// <summary>
            /// Chemical Enter Main Valve 化学品进入主阀
            /// </summary>
            public static class ChemicalEnterMainValve
            {
                [PlcHandle(Handle.CmdRollA2, "打开")]
                public const int Open = 1210;
                
                [PlcHandle(Handle.CmdRollA2, "关闭")]
                public const int Close = 1211;
            }
            
            /// <summary>
            /// Chemical Enter Flow 化学品进入流量
            /// </summary>
            public static class ChemicalEnterFlow
            {
                [PlcHandle(Handle.CmdRollA2, "流量")]
                public const int Flowrate = 1212;
                
                [PlcHandle(Handle.CmdRollA2, "流量重置")]
                public const int FlowrateReset = 1213;
                
                [PlcHandle(Handle.CmdRollA2, "CLFC校准")]
                public const int ClfcCalibration = 1214;

                public static class Param
                {
                    [Param(33118, 100, 15, 300, "药液流量控制(混合前)", "药液流量控制(混合前)", "mL/min")]
                    public static int FlowrateSet = 100;
                }
            }
            
            /// <summary>
            /// Pipe DIW Wash 管道去离子水清洗
            /// </summary>
            public static class PipeDiwWash
            {
                [PlcHandle(Handle.CmdRollA2, "打开")]
                public const int Open = 1215;
                
                [PlcHandle(Handle.CmdRollA2, "关闭")]
                public const int Close = 1216;
            }
            
            /// <summary>
            /// DIW Enter Main Valve 去离子水进入主阀
            /// </summary>
            public static class DiwEnterMainValve
            {
                [PlcHandle(Handle.CmdRollA2, "打开")]
                public const int Open = 1217;
                
                [PlcHandle(Handle.CmdRollA2, "关闭")]
                public const int Close = 1218;
            }
            
            /// <summary>
            /// DIW Enter Flow 去离子水进入流量
            /// </summary>
            public static class DiwEnterFlow
            {
                [PlcHandle(Handle.CmdRollA2, "流量")]
                public const int Flowrate = 1219;
                
                [PlcHandle(Handle.CmdRollA2, "流量重置")]
                public const int FlowrateReset = 1220;
                
                [PlcHandle(Handle.CmdRollA2, "CLFC校准")]
                public const int ClfcCalibration = 1221;

                public static class Param
                {
                    [Param(33120, 1000, 200, 4000, "纯水流量控制(混合前)", "纯水流量控制(混合前)", "mL/min")]
                    public static int FlowrateSet = 1000;
                }
            }
            
            /// <summary>
            /// WF Lower Face Chemical Wash 晶圆下表面化学清洗
            /// </summary>
            public static class WfLowerFaceChemicalWash
            {
                [PlcHandle(Handle.CmdRollA2, "打开")]
                public const int Open = 1222;
                
                [PlcHandle(Handle.CmdRollA2, "关闭")]
                public const int Close = 1223;
            }
            
            /// <summary>
            /// WF Upper Face Chemical Wash 晶圆上表面化学清洗
            /// </summary>
            public static class WfUpperFaceChemicalWash
            {
                [PlcHandle(Handle.CmdRollA2, "打开")]
                public const int Open = 1224;
                
                [PlcHandle(Handle.CmdRollA2, "关闭")]
                public const int Close = 1225;
            }
            
            /// <summary>
            /// Quartz Chemical Wash 石英化学清洗
            /// </summary>
            public static class QuartzChemicalWash
            {
                [PlcHandle(Handle.CmdRollA2, "打开")]
                public const int Open = 1226;
                
                [PlcHandle(Handle.CmdRollA2, "关闭")]
                public const int Close = 1227;
            }
            
            /// <summary>
            /// WF Upper Face DIW Wash Inlet 晶圆上表面去离子水清洗入口
            /// </summary>
            public static class WfUpperFaceDiwWashInlet
            {
                [PlcHandle(Handle.CmdRollA2, "打开")]
                public const int Open = 1242;
                
                [PlcHandle(Handle.CmdRollA2, "关闭")]
                public const int Close = 1243;
            }
            
            /// <summary>
            /// WF Upper Face DIW Wash Outlet 晶圆上表面去离子水清洗出口
            /// </summary>
            public static class WfUpperFaceDiwWashOutlet
            {
                [PlcHandle(Handle.CmdRollA2, "打开")]
                public const int Open = 1244;
                
                [PlcHandle(Handle.CmdRollA2, "关闭")]
                public const int Close = 1245;
            }
            
            /// <summary>
            /// WF Upper Face Wash DIW Flow 晶圆上表面清洗去离子水流量
            /// </summary>
            public static class WfUpperFaceWashDiwFlow
            {
                [PlcHandle(Handle.CmdRollA2, "流量")]
                public const int Flowrate = 1246;
                
                [PlcHandle(Handle.CmdRollA2, "流量重置")]
                public const int FlowrateReset = 1247;
                
                [PlcHandle(Handle.CmdRollA2, "CLFC校准")]
                public const int ClfcCalibration = 1248;

                public static class Param
                {
                    [Param(33122, 1000, 200, 4000, "晶圆上表面清洁 流量控制", "晶圆上表面清洁 流量控制", "mL/min")]
                    public static int FlowrateSet = 1000;
                }
            }
            
            /// <summary>
            /// WF Lower Face DIW Wash 晶圆下表面去离子水清洗
            /// </summary>
            public static class WfLowerFaceDiwWash
            {
                [PlcHandle(Handle.CmdRollA2, "打开")]
                public const int Open = 1263;
                
                [PlcHandle(Handle.CmdRollA2, "关闭")]
                public const int Close = 1264;
            }
            
            /// <summary>
            /// WF Lower Face DIW Wash Flow 晶圆下表面去离子水清洗流量
            /// </summary>
            public static class WfLowerFaceDiwWashFlow
            {
                [PlcHandle(Handle.CmdRollA2, "流量")]
                public const int Flowrate = 1265;
                
                [PlcHandle(Handle.CmdRollA2, "流量重置")]
                public const int FlowrateReset = 1266;
                
                [PlcHandle(Handle.CmdRollA2, "CLFC校准")]
                public const int ClfcCalibration = 1267;

                public static class Param
                {
                    [Param(33124, 1000, 200, 4000, "晶圆下表面清洁 流量控制", "晶圆下表面清洁 流量控制", "mL/min")]
                    public static int FlowrateSet = 1000;
                }
            }
            
            /// <summary>
            /// Upper Roller DIW Wash 上辊去离子水清洗
            /// </summary>
            public static class UpperRollerDiwWash
            {
                [PlcHandle(Handle.CmdRollA2, "打开")]
                public const int Open = 1293;
                
                [PlcHandle(Handle.CmdRollA2, "关闭")]
                public const int Close = 1294;
            }
            
            /// <summary>
            /// Quartz DIW Wash 石英去离子水清洗
            /// </summary>
            public static class QuartzDiwWash
            {
                [PlcHandle(Handle.CmdRollA2, "打开")]
                public const int Open = 1295;
                
                [PlcHandle(Handle.CmdRollA2, "关闭")]
                public const int Close = 1296;
            }
            
            /// <summary>
            /// Upper Roller Inside DIW Wash 上辊内部去离子水清洗
            /// </summary>
            public static class UpperRollerInsideDiwWash
            {
                [PlcHandle(Handle.CmdRollA2, "打开")]
                public const int Open = 1297;
                
                [PlcHandle(Handle.CmdRollA2, "关闭")]
                public const int Close = 1298;
            }
            
            /// <summary>
            /// Lower Roller Inside DIW Wash 下辊内部去离子水清洗
            /// </summary>
            public static class LowerRollerInsideDiwWash
            {
                [PlcHandle(Handle.CmdRollA2, "打开")]
                public const int Open = 1299;
                
                [PlcHandle(Handle.CmdRollA2, "关闭")]
                public const int Close = 1300;
            }
            
            /// <summary>
            /// Mage Sonic DIW Wash 超声波去离子水清洗
            /// </summary>
            public static class MageSonicDiwWash
            {
                [PlcHandle(Handle.CmdRollA2, "打开")]
                public const int Open = 1301;
                
                [PlcHandle(Handle.CmdRollA2, "关闭")]
                public const int Close = 1302;
            }
            
            /// <summary>
            /// Chamber DIW Wash 腔室去离子水清洗
            /// </summary>
            public static class ChamberDiwWash
            {
                [PlcHandle(Handle.CmdRollA2, "打开")]
                public const int Open = 1303;
                
                [PlcHandle(Handle.CmdRollA2, "关闭")]
                public const int Close = 1304;
            }
            
            /// <summary>
            /// Pillar Wash 支柱清洗
            /// </summary>
            public static class PillarWash
            {
                [PlcHandle(Handle.CmdRollA2, "打开")]
                public const int Open = 1305;
                
                [PlcHandle(Handle.CmdRollA2, "关闭")]
                public const int Close = 1306;
            }
            
            /// <summary>
            /// Cooling Water Circulation 冷却水循环
            /// </summary>
            public static class CoolingWaterCirculation
            {
                [PlcHandle(Handle.CmdRollA2, "打开")]
                public const int Open = 1307;
                
                [PlcHandle(Handle.CmdRollA2, "关闭")]
                public const int Close = 1308;
            }
        }

        public static class Flush
        {
            [PlcHandle(Handle.CmdRollA2, "打开全纯水阀")]
            public const int Open = 11404;

            [PlcHandle(Handle.CmdRollA2, "关闭全纯水阀")]
            public const int Close = 11405;

            public static class Param
            {
                [Param(25014, 0, 0, 999, "全纯水阀打开延时", "DIW(ALL) Open Delay", "s")]
                public static int OpenDelay = 0;
            }
        }
    }
}
