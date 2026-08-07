using CMP.Services.Plc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Constants.Plc.RobotControl
{
    public static class CleanRobotB2Address
    {
        #region 位置设定 (D)
        /// <summary>
        /// 清洗B侧机器人2 rollerA1/A2
        /// </summary>
        public const string RollerB3 = "D11918";
        /// <summary>
        /// 清洗B侧机器人2 RollerA3
        /// </summary>
        public const string PenB2 = "D11920";
        /// <summary>
        /// 清洗B侧机器人2 PenA1
        /// </summary>
        public const string PenB1 = "D11922";
        /// <summary>
        /// 清洗B侧机器人2 DryA
        /// </summary>
        public const string Dry = "D11924";
        #endregion

        #region 移动命令 (M)
        /// <summary>
        /// 清洗B侧机器人2 rollerA3移动
        /// </summary>
        public const string RollerB3Move = "M1624";
        /// <summary>
        /// 清洗B侧机器人2 PenA2移动
        /// </summary>
        public const string PenB2Move = "M1625";
        /// <summary>
        /// 清洗B侧机器人2 PenA1移动
        /// </summary>
        public const string PenB1Move = "M1626";
        /// <summary>
        /// 清洗B侧机器人2 DryA移动
        /// </summary>
        public const string DryMove = "M1627";
        #endregion

        #region 速度设定 (D)
        /// <summary>
        /// 清洗B侧机器人2 Jog速度
        /// </summary>
        public const string JogSpeed = "D1382";
        /// <summary>
        /// 清洗B侧机器人2 自动速度
        /// </summary>
        public const string AutoSpeed = "D11900";
        #endregion

        #region 当前坐标 (D)
        /// <summary>
        /// 清洗B侧机器人2 当前坐标
        /// </summary>
        public const string CurrentCoordinate = "D5380";
        #endregion

        #region 使能状态 (M)
        /// <summary>
        /// 清洗B侧机器人2 使能
        /// </summary>
        public const string Enable = "M1608";
        /// <summary>
        /// 清洗B侧机器人2 使能状态
        /// </summary>
        public const string EnableStatus = "M5912";
        #endregion

        #region 报警状态 (D/M)
        /// <summary>
        /// 清洗B侧机器人2 报警码
        /// </summary>
        public const string AlarmCode = "D5386";
        /// <summary>
        /// 清洗B侧机器人2 驱动器报警
        /// </summary>
        public const string DriverAlarm = "M5921";
        /// <summary>
        /// 清洗B侧机器人2 限位报警
        /// </summary>
        public const string LimitAlarm = "M5919";
        #endregion

        #region JOG控制 (M)
        /// <summary>
        /// 清洗B侧机器人2 JOG 上移
        /// </summary>
        public const string JogUp = "M1609";
        /// <summary>
        /// 清洗B侧机器人2 JOG 上移激活状态
        /// </summary>
        public const string JogUpStatus = "M5916";
        /// <summary>
        /// 清洗B侧机器人2 JOG 中间激活状态
        /// </summary>
        public const string JogMiddleStatus = "M5918";
        /// <summary>
        /// 清洗B侧机器人2 JOG 下移
        /// </summary>
        public const string JogDown = "M1610";
        /// <summary>
        /// 清洗B侧机器人2 JOG 下移激活状态
        /// </summary>
        public const string JogDownStatus = "M5917";
        #endregion

        #region 原点 (M)

        /// <summary>
        /// 清洗B侧机器人2 回原点命令
        /// </summary>
        public const string Origin = "M1611";

        /// <summary>
        /// 清洗B侧机器人2 原点状态
        /// </summary>
        public const string OriginStatus = "M5913";
        #endregion

        #region 机器人控制 - 程序控制 (D/M)

        /// <summary>
        /// 清洗B侧机器人2 程序号
        /// </summary>
        public const string ProgramNumber = "D1440";

        /// <summary>
        /// 清洗B侧机器人2 启动
        /// </summary>
        public const string Start = "M1800";

        /// <summary>
        /// 清洗B侧机器人2 运行中状态
        /// </summary>
        public const string RunningStatus = "M6120";

        /// <summary>
        /// 清洗B侧机器人2 暂停
        /// </summary>
        public const string Pause = "M1801";

        /// <summary>
        /// 清洗B侧机器人2 暂停状态
        /// </summary>
        public const string PausedStatus = "M6121";

        /// <summary>
        /// 清洗B侧机器人2 停止
        /// </summary>
        public const string Stop = "M1802";

        /// <summary>
        /// 清洗B侧机器人2 停止状态
        /// </summary>
        public const string StoppedStatus = "M6122";

        /// <summary>
        /// 清洗B侧机器人2 复位
        /// </summary>
        public const string Reset = "M1803";

        /// <summary>
        /// 清洗B侧机器人2 复位状态
        /// </summary>
        public const string ResetStatus = "M6123";

        #endregion

        #region 机器人控制 - 工位操作 (D/M)

        /// <summary>
        /// 清洗B侧机器人2 工位号
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
        /// 清洗B侧机器人2 工作区取料
        /// </summary>
        public const string WorkAreaPickup = "M1814";
        /// <summary>
        /// 清洗B侧机器人2 工作区取料状态
        /// </summary>
        public const string WorkAreaPickupStatus = "M6133";
        /// <summary>
        /// 清洗B侧机器人2 工作区放料
        /// </summary>
        public const string WorkAreaPlace = "M1815";
        /// <summary>
        /// 清洗B侧机器人2 工作区放料状态
        /// </summary>
        public const string WorkAreaPlaceStatus = "M6134";
        /// <summary>
        /// 清洗B侧机器人2 等待确认
        /// </summary>
        public const string WaitConfirm = "M1818";
        /// <summary>
        /// 清洗B侧机器人2 等待确认状态
        /// </summary>
        public const string WaitConfirmStatus = "M6138";

        #endregion

        #region 机器人控制 - 模式选择 (M/D_Bit)

        /// <summary>
        /// 清洗B侧机器人2 自动模式
        /// </summary>
        public const string AutoMode = "M1805";

        /// <summary>
        /// 清洗B侧机器人2 自动模式状态
        /// </summary>
        public static readonly DBitAddress AutoModeStatus = new("D51000", 0x05);

        /// <summary>
        /// 清洗B侧机器人2 使用上臂
        /// </summary>
        public const string UseUpperArm = "M1806";

        /// <summary>
        /// 清洗B侧机器人2 使用上臂状态
        /// </summary>
        public static readonly DBitAddress UseUpperArmStatus = new("D56001", 0x0);

        /// <summary>
        /// 清洗B侧机器人2 使用下臂
        /// </summary>
        public const string UseLowerArm = "M1807";

        /// <summary>
        /// 清洗B侧机器人2 使用下臂状态
        /// </summary>
        public static readonly DBitAddress UseLowerArmStatus = new("D56001", 0x01);

        /// <summary>
        /// 清洗B侧机器人2 确认开关
        /// </summary>
        public const string ConfirmSwitch = "M1819";

        /// <summary>
        /// 清洗B侧机器人2 确认切换状态
        /// </summary>
        public static readonly DBitAddress ConfirmSwitchStatus = new("D51003", 0x0a);

        #endregion

        #region 机器人控制 - 传感器状态 (M/D_Bit)

        /// <summary>
        /// 清洗B侧机器人2 上臂传感器
        /// </summary>
        public static readonly DBitAddress UpperArmSensor = new("D51001", 0x0);

        /// <summary>
        /// 清洗B侧机器人2 下臂传感器
        /// </summary>
        public static readonly DBitAddress LowerArmSensor = new("D51001", 0x1);

        /// <summary>
        /// 清洗B侧机器人2 命令接受
        /// </summary>
        public static readonly DBitAddress CommandAccept = new("D51001", 0x04);

        /// <summary>
        /// 清洗B侧机器人2 机器人安全位1
        /// </summary>
        public static readonly DBitAddress RobotSafePosition1 = new("D51004", 0x0);

        /// <summary>
        /// 清洗B侧机器人2 机器人安全位
        /// </summary>
        public static readonly DBitAddress RobotSafePosition = new("D51004", 0x0);

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
        public const string StationReset = "M2504";

        /// <summary>
        /// 工位操作 - 复位状态
        /// </summary>
        public const string StationResetStatus = "M6825";

        /// <summary>
        /// 工位操作 - 启动
        /// </summary>
        public const string StationStart = "M2507";

        /// <summary>
        /// 工位操作 - 运行状态
        /// </summary>
        public const string StationRunningStatus = "M6827";

        /// <summary>
        /// 工位操作 - 停止
        /// </summary>
        public const string StationStop = "M2509";

        /// <summary>
        /// 工位操作 - 停止状态
        /// </summary>
        public const string StationStoppedStatus = "M6829";

        /// <summary>
        /// 从Roller3取料
        /// </summary>
        public const string PickFromRoller3 = "M2520";

        /// <summary>
        /// 从Roller3取料激活状态
        /// </summary>
        public const string PickFromRoller3ActiveStatus = "M6840";

        /// <summary>
        /// 从Roller3取料完成状态
        /// </summary>
        public const string PickFromRoller3DoneStatus = "M6856";

        /// <summary>
        /// 放入Pen2
        /// </summary>
        public const string PlaceToPen2 = "M2521";

        /// <summary>
        /// 放入Pen2激活状态
        /// </summary>
        public const string PlaceToPen2ActiveStatus = "M6841";

        /// <summary>
        /// 放入Pen2完成状态
        /// </summary>
        public const string PlaceToPen2DoneStatus = "M6857";

        /// <summary>
        /// 从Pen1取料
        /// </summary>
        public const string PickFromPen1 = "M2522";

        /// <summary>
        /// 从Pen1取料激活状态
        /// </summary>
        public const string PickFromPen1ActiveStatus = "M6842";

        /// <summary>
        /// 从Pen1取料完成状态
        /// </summary>
        public const string PickFromPen1DoneStatus = "M6858";

        /// <summary>
        /// 从Pen2取料
        /// </summary>
        public const string PickFromPen2 = "M2523";

        /// <summary>
        /// 从Pen2取料激活状态
        /// </summary>
        public const string PickFromPen2ActiveStatus = "M6843";

        /// <summary>
        /// 从Pen2取料完成状态
        /// </summary>
        public const string PickFromPen2DoneStatus = "M6859";

        /// <summary>
        /// 放入Dry
        /// </summary>
        public const string PlaceToDry = "M2524";

        /// <summary>
        /// 放入Dry激活状态
        /// </summary>
        public const string PlaceToDryActiveStatus = "M6844";

        /// <summary>
        /// 放入Dry完成状态
        /// </summary>
        public const string PlaceToDryDoneStatus = "M6860";

        /// <summary>
        /// 模块报警
        /// </summary>
        public const string StationModuleAlarm = "M6833";

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
        public const string StationManualMode = "M2513";

        /// <summary>
        /// 自动模式
        /// </summary>
        public const string StationAutoMode = "M2512";
        #endregion


    }
}
