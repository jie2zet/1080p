using CMP.Services.Plc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Constants.Plc.RobotControl
{
    public static class CleanRobotA1Address
    {
        #region 位置设定 (ZR)
        /// <summary>
        /// 清洗A侧机器人1 rollerA1/A2
        /// </summary>
        public const string RollerA1orA2 = "ZR11718";
        /// <summary>
        /// 清洗A侧机器人1 RollerA3
        /// </summary>
        public const string RollerA3 = "ZR11720";
        /// <summary>
        /// 清洗A侧机器人1 PenA1
        /// </summary>
        public const string PenA1 = "ZR11722";
        #endregion

        #region 移动命令 (M)
        /// <summary>
        /// rollerA1/A2移动
        /// </summary>
        public const string RollerA1orA2Move = "M1560";
        /// <summary>
        /// 清洗A侧机器人1 RollerA3移动
        /// </summary>
        public const string RollerA3Move = "M1561";
        /// <summary>
        /// 清洗A侧机器人1 PenA1移动
        /// </summary>
        public const string PenA1Move = "M1562";
        #endregion

        #region 速度设定 (D/ZR)
        /// <summary>
        /// 清洗A侧机器人1 Jog速度
        /// </summary>
        public const string JogSpeed = "D1342";
        /// <summary>
        /// 清洗A侧机器人1 自动速度
        /// </summary>
        public const string AutoSpeed = "ZR11700";
        #endregion

        #region 当前坐标 (D)
        /// <summary>
        /// 清洗A侧机器人1 当前坐标
        /// </summary>
        public const string CurrentCoordinate = "D5340";
        #endregion

        #region 使能状态 (M)
        /// <summary>
        /// 清洗A侧机器人1 使能
        /// </summary>
        public const string Enable = "M1544";
        /// <summary>
        /// 清洗A侧机器人1 使能状态
        /// </summary>
        public const string EnableStatus = "M5816";
        #endregion

        #region 报警状态 (D/M)
        /// <summary>
        /// 清洗A侧机器人1 报警码
        /// </summary>
        public const string AlarmCode = "D5346";
        /// <summary>
        /// 清洗A侧机器人1 驱动器报警
        /// </summary>
        public const string DriverAlarm = "M5825";
        /// <summary>
        /// 清洗A侧机器人1 限位报警
        /// </summary>
        public const string LimitAlarm = "M5823";
        #endregion

        #region JOG控制 (M)
        /// <summary>
        /// 清洗A侧机器人1 JOG 上移
        /// </summary>
        public const string JogUp = "M1545";
        /// <summary>
        /// 清洗A侧机器人1 JOG 上移激活状态
        /// </summary>
        public const string JogUpStatus = "M5820";
        /// <summary>
        /// 清洗A侧机器人1 JOG 中间激活状态
        /// </summary>
        public const string JogMiddleStatus = "M5822";
        /// <summary>
        /// 清洗A侧机器人1 JOG 下移
        /// </summary>
        public const string JogDown = "M1546";
        /// <summary>
        /// 清洗A侧机器人1 JOG 下移激活状态
        /// </summary>
        public const string JogDownStatus = "M5821";
        #endregion

        #region 原点 (M)

        /// <summary>
        /// 清洗A侧机器人1 回原点命令
        /// </summary>
        public const string Origin = "M1547";

        /// <summary>
        /// 清洗A侧机器人1 原点状态
        /// </summary>
        public const string OriginStatus = "M5817";
        #endregion

        #region 机器人控制 - 程序控制 (D/M)

        /// <summary>
        /// 清洗A侧机器人1 程序号
        /// </summary>
        public const string ProgramNumber = "D1440";

        /// <summary>
        /// 清洗A侧机器人1 启动
        /// </summary>
        public const string Start = "M1704";

        /// <summary>
        /// 清洗A侧机器人1 运行中状态
        /// </summary>
        public const string RunningStatus = "M6024";

        /// <summary>
        /// 清洗A侧机器人1 暂停
        /// </summary>
        public const string Pause = "M1705";

        /// <summary>
        /// 清洗A侧机器人1 暂停状态
        /// </summary>
        public const string PausedStatus = "M6025";

        /// <summary>
        /// 清洗A侧机器人1 停止
        /// </summary>
        public const string Stop = "M1706";

        /// <summary>
        /// 清洗A侧机器人1 停止状态
        /// </summary>
        public const string StoppedStatus = "M6026";

        /// <summary>
        /// 清洗A侧机器人1 复位
        /// </summary>
        public const string Reset = "M1707";

        /// <summary>
        /// 清洗A侧机器人1 复位状态
        /// </summary>
        public const string ResetStatus = "M6027";

        #endregion

        #region 机器人控制 - 工位操作 (D/M)

        /// <summary>
        /// 清洗A侧机器人1 工位号
        /// </summary>
        public const string WorkAreaNumber = "D5442";

        /// <summary>
        /// 清洗A侧机器人1 RollA1 常量
        /// </summary>
        public const string RollA1_Const = "D1442";
        /// <summary>
        /// 清洗A侧机器人1 RollA3 常量
        /// </summary>
        public const string RollA3_Const = "D1442";
        /// <summary>
        /// 清洗A侧机器人1 PneA1 常量
        /// </summary>
        public const string PenA1_Const = "D1442";
        /// <summary>
        /// 清洗A侧机器人1 工作区取料
        /// </summary>
        public const string WorkAreaPickup = "M1718";
        /// <summary>
        /// 清洗A侧机器人1 工作区取料状态
        /// </summary>
        public const string WorkAreaPickupStatus = "M6037";
        /// <summary>
        /// 清洗A侧机器人1 工作区放料
        /// </summary>
        public const string WorkAreaPlace = "M1719";
        /// <summary>
        /// 清洗A侧机器人1 工作区放料状态
        /// </summary>
        public const string WorkAreaPlaceStatus = "M6038";
        /// <summary>
        /// 清洗A侧机器人1 等待确认
        /// </summary>
        public const string WaitConfirm = "M1722";
        /// <summary>
        /// 清洗A侧机器人1 等待确认状态
        /// </summary>
        public const string WaitConfirmStatus = "M6042";

        #endregion

        #region 机器人控制 - 模式选择 (M/D_Bit)

        /// <summary>
        /// 清洗A侧机器人1 自动模式
        /// </summary>
        public const string AutoMode = "M1709";

        /// <summary>
        /// 清洗A侧机器人1 自动模式状态
        /// </summary>
        public static readonly DBitAddress AutoModeStatus = new("D50400", 0x05);

        /// <summary>
        /// 清洗A侧机器人1 使用上臂
        /// </summary>
        public const string UseUpperArm = "M1710";

        /// <summary>
        /// 清洗A侧机器人1 使用上臂状态
        /// </summary>
        public static readonly DBitAddress UseUpperArmStatus = new("D55401", 0x0);

        /// <summary>
        /// 清洗A侧机器人1 使用下臂
        /// </summary>
        public const string UseLowerArm = "M1711";

        /// <summary>
        /// 清洗A侧机器人1 使用下臂状态
        /// </summary>
        public static readonly DBitAddress UseLowerArmStatus = new("D55401", 0x01);

        /// <summary>
        /// 清洗A侧机器人1 确认开关
        /// </summary>
        public const string ConfirmSwitch = "M1723";

        /// <summary>
        /// 清洗A侧机器人1 确认切换状态
        /// </summary>
        public static readonly DBitAddress ConfirmSwitchStatus = new("D50403", 0x0a);

        #endregion

        #region 机器人控制 - 传感器状态 (D_Bit)

        /// <summary>
        /// 清洗A侧机器人1 上臂传感器
        /// </summary>
        public static readonly DBitAddress UpperArmSensor = new("D50401", 0x0);

        /// <summary>
        /// 清洗A侧机器人1 下臂传感器
        /// </summary>
        public static readonly DBitAddress LowerArmSensor = new("D50401", 0x1);

        /// <summary>
        /// 清洗A侧机器人1 命令接受 
        /// </summary>
        public static readonly DBitAddress CommandAccept = new("D50401", 0x04);

        /// <summary>
        /// 清洗A侧机器人1 机器人安全位1
        /// </summary>
        public static readonly DBitAddress RobotSafePosition1 = new("D50404", 0x0);

        /// <summary>
        /// 清洗A侧机器人1 机器人安全位
        /// </summary>
        public static readonly DBitAddress RobotSafePosition = new("D50404", 0x0);

        #endregion

        #region 急停、报警复位、保存

        /// <summary>
        /// 急停
        /// </summary>
        public const string EmergencyStop = "M99";

        /// <summary>
        /// 报警复位
        /// </summary>
        public const string AlarmReset = "B0B";

        /// <summary>
        /// 保存
        /// </summary>
        public const string Save = "M98";

        #endregion

        #region 工位操作 - Station Operation

        /// <summary>
        /// 工位操作 - 复位
        /// </summary>
        public const string StationReset = "M2360";

        /// <summary>
        /// 工位操作 - 复位状态
        /// </summary>
        public const string StationResetStatus = "M6681";

        /// <summary>
        /// 工位操作 - 启动
        /// </summary>
        public const string StationStart = "M2363";

        /// <summary>
        /// 工位操作 - 运行状态
        /// </summary>
        public const string StationRunningStatus = "M6683";

        /// <summary>
        /// 工位操作 - 停止
        /// </summary>
        public const string StationStop = "M2365";

        /// <summary>
        /// 工位操作 - 停止状态
        /// </summary>
        public const string StationStoppedStatus = "M6685";

        /// <summary>
        /// 从Roller1取料
        /// </summary>
        public const string PickFromRoller1 = "M2376";

        /// <summary>
        /// 从Roller1取料激活状态
        /// </summary>
        public const string PickFromRoller1ActiveStatus = "M6696";

        /// <summary>
        /// 从Roller1取料完成状态
        /// </summary>
        public const string PickFromRoller1DoneStatus = "M6712";

        /// <summary>
        /// 从Roller2取料
        /// </summary>
        public const string PickFromRoller2 = "M2377";

        /// <summary>
        /// 从Roller2取料激活状态
        /// </summary>
        public const string PickFromRoller2ActiveStatus = "M6697";

        /// <summary>
        /// 从Roller2取料完成状态
        /// </summary>
        public const string PickFromRoller2DoneStatus = "M6713";

        /// <summary>
        /// 放入Roller3
        /// </summary>
        public const string PlaceToRoller3 = "M2378";

        /// <summary>
        /// 放入Roller3激活状态
        /// </summary>
        public const string PlaceToRoller3ActiveStatus = "M6698";

        /// <summary>
        /// 放入Roller3完成状态
        /// </summary>
        public const string PlaceToRoller3DoneStatus = "M6714";

        /// <summary>
        /// 从Roller3取料
        /// </summary>
        public const string PickFromRoller3 = "M2379";

        /// <summary>
        /// 从Roller3取料激活状态
        /// </summary>
        public const string PickFromRoller3ActiveStatus = "M6699";

        /// <summary>
        /// 从Roller3取料完成状态
        /// </summary>
        public const string PickFromRoller3DoneStatus = "M6715";

        /// <summary>
        /// 放入Pen1
        /// </summary>
        public const string PlaceToPen1 = "M2380";

        /// <summary>
        /// 放入Pen1激活状态
        /// </summary>
        public const string PlaceToPen1ActiveStatus = "M6700";

        /// <summary>
        /// 放入Pen1完成状态
        /// </summary>
        public const string PlaceToPen1DoneStatus = "M6716";


        /// <summary>
        /// 放入Roller2
        /// </summary>
        public const string PlaceToRoller2 = "M2381";

        /// <summary>
        /// 放入Roller2激活状态
        /// </summary>
        public const string PlaceToRoller2ActiveStatus = "M6701";

        /// <summary>
        /// 放入Roller2完成状态
        /// </summary>
        public const string PlaceToRoller2DoneStatus = "M6717";

        /// <summary>
        /// 模块报警
        /// </summary>
        public const string StationModuleAlarm = "M6689";

        /// <summary>
        /// 全局报警
        /// </summary>
        public const string StationGlobalAlarm = "M89";

        /// <summary>
        /// 工位报警复位
        /// </summary>
        public const string StationAlarmReset = "B0B";

        /// <summary>
        /// 手动模式
        /// </summary>
        public const string StationManualMode = "M2369";

        /// <summary>
        /// 自动模式
        /// </summary>
        public const string StationAutoMode = "M2368";
        #endregion
    }
}
