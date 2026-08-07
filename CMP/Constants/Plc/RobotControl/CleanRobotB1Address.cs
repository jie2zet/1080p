using CMP.Services.Plc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Constants.Plc.RobotControl
{
    public static class CleanRobotB1Address
    {
        #region 机器人控制 - 程序控制 (D/M)

        /// <summary>
        /// 清洗B侧机器人1 程序号
        /// </summary>
        public const string ProgramNumber = "D1440";

        /// <summary>
        /// 清洗B侧机器人1 启动
        /// </summary>
        public const string Start = "M1768";

        /// <summary>
        /// 清洗B侧机器人1 运行中状态
        /// </summary>
        public const string RunningStatus = "M6088";

        /// <summary>
        /// 清洗B侧机器人1 暂停
        /// </summary>
        public const string Pause = "M1769";

        /// <summary>
        /// 清洗B侧机器人1 暂停状态
        /// </summary>
        public const string PausedStatus = "M6089";

        /// <summary>
        /// 清洗B侧机器人1 停止
        /// </summary>
        public const string Stop = "M1770";

        /// <summary>
        /// 清洗B侧机器人1 停止状态
        /// </summary>
        public const string StoppedStatus = "M6090";

        /// <summary>
        /// 清洗B侧机器人1 复位
        /// </summary>
        public const string Reset = "M1771";

        /// <summary>
        /// 清洗B侧机器人1 复位状态
        /// </summary>
        public const string ResetStatus = "M6091";

        #endregion

        #region 机器人控制 - 工位操作 (D/M)

        /// <summary>
        /// 清洗B侧机器人1 工位号
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
        /// 清洗B侧机器人1 工作区取料
        /// </summary>
        public const string WorkAreaPickup = "M1782";
        /// <summary>
        /// 清洗B侧机器人1 工作区取料状态
        /// </summary>
        public const string WorkAreaPickupStatus = "M6101";
        /// <summary>
        /// 清洗B侧机器人1 工作区放料
        /// </summary>
        public const string WorkAreaPlace = "M1783";
        /// <summary>
        /// 清洗B侧机器人1 工作区放料状态
        /// </summary>
        public const string WorkAreaPlaceStatus = "M6102";
        /// <summary>
        /// 清洗B侧机器人1 等待确认
        /// </summary>
        public const string WaitConfirm = "M1786";
        /// <summary>
        /// 清洗B侧机器人1 等待确认状态
        /// </summary>
        public const string WaitConfirmStatus = "M6106";

        #endregion

        #region 机器人控制 - 模式选择 (M/D_Bit)

        /// <summary>
        /// 清洗B侧机器人1 自动模式
        /// </summary>
        public const string AutoMode = "M1773";

        /// <summary>
        /// 清洗B侧机器人1 自动模式状态
        /// </summary>
        public static readonly DBitAddress AutoModeStatus = new("D50800", 0x05);

        /// <summary>
        /// 清洗B侧机器人1 使用上臂
        /// </summary>
        public const string UseUpperArm = "M1774";

        /// <summary>
        /// 清洗B侧机器人1 使用上臂状态
        /// </summary>
        public static readonly DBitAddress UseUpperArmStatus = new("D55801", 0x0);

        /// <summary>
        /// 清洗B侧机器人1 使用下臂
        /// </summary>
        public const string UseLowerArm = "M1775";

        /// <summary>
        /// 清洗B侧机器人1 使用下臂状态
        /// </summary>
        public static readonly DBitAddress UseLowerArmStatus = new("D55801", 0x01);

        /// <summary>
        /// 清洗B侧机器人1 确认开关
        /// </summary>
        public const string ConfirmSwitch = "M1787";

        /// <summary>
        /// 清洗B侧机器人1 确认切换状态
        /// </summary>
        public static readonly DBitAddress ConfirmSwitchStatus = new("D50803", 0x0a);

        #endregion

        #region 机器人控制 - 传感器状态 (M/D_Bit)

        /// <summary>
        /// 清洗B侧机器人1 上臂传感器
        /// </summary>
        public static readonly DBitAddress UpperArmSensor = new("D50801", 0x0);

        /// <summary>
        /// 清洗B侧机器人1 下臂传感器
        /// </summary>
        public static readonly DBitAddress LowerArmSensor = new("D50801", 0x1);

        /// <summary>
        /// 清洗B侧机器人1 命令接受
        /// </summary>
        public static readonly DBitAddress CommandAccept = new("D50801", 0x04);

        /// <summary>
        /// 清洗B侧机器人1 机器人安全位1
        /// </summary>
        public static readonly DBitAddress RobotSafePosition1 = new("D50804", 0x0);

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
        public const string StationReset = "M2456";

        /// <summary>
        /// 工位操作 - 复位状态
        /// </summary>
        public const string StationResetStatus = "M6777";

        /// <summary>
        /// 工位操作 - 启动
        /// </summary>
        public const string StationStart = "M2459";

        /// <summary>
        /// 工位操作 - 运行状态
        /// </summary>
        public const string StationRunningStatus = "M6779";

        /// <summary>
        /// 工位操作 - 停止
        /// </summary>
        public const string StationStop = "M2461";

        /// <summary>
        /// 工位操作 - 停止状态
        /// </summary>
        public const string StationStoppedStatus = "M6781";

        /// <summary>
        /// 从Roller1取料
        /// </summary>
        public const string PickFromRoller1 = "M2472";

        /// <summary>
        /// 从Roller1取料激活状态
        /// </summary>
        public const string PickFromRoller1ActiveStatus = "M6792";

        /// <summary>
        /// 从Roller1取料完成状态
        /// </summary>
        public const string PickFromRoller1DoneStatus = "M6808";

        /// <summary>
        /// 从Roller2取料
        /// </summary>
        public const string PickFromRoller2 = "M2473";

        /// <summary>
        /// 从Roller2取料激活状态
        /// </summary>
        public const string PickFromRoller2ActiveStatus = "M6793";

        /// <summary>
        /// 从Roller2取料完成状态
        /// </summary>
        public const string PickFromRoller2DoneStatus = "M6809";

        /// <summary>
        /// 放入Roller3
        /// </summary>
        public const string PlaceToRoller3 = "M2474";

        /// <summary>
        /// 放入Roller3激活状态
        /// </summary>
        public const string PlaceToRoller3ActiveStatus = "M6794";

        /// <summary>
        /// 放入Roller3完成状态
        /// </summary>
        public const string PlaceToRoller3DoneStatus = "M6810";

        /// <summary>
        /// 从Roller3取料
        /// </summary>
        public const string PickFromRoller3 = "M2475";

        /// <summary>
        /// 从Roller3取料激活状态
        /// </summary>
        public const string PickFromRoller3ActiveStatus = "M6795";

        /// <summary>
        /// 从Roller3取料完成状态
        /// </summary>
        public const string PickFromRoller3DoneStatus = "M6811";

        /// <summary>
        /// 放入Pen1
        /// </summary>
        public const string PlaceToPen1 = "M2476";

        /// <summary>
        /// 放入Pen1激活状态
        /// </summary>
        public const string PlaceToPen1ActiveStatus = "M6796";

        /// <summary>
        /// 放入Pen1完成状态
        /// </summary>
        public const string PlaceToPen1DoneStatus = "M6812";

        /// <summary>
        /// 放入Roller3
        /// </summary>
        public const string PlaceToRollerB2 = "M2477";

        /// <summary>
        /// 放入Roller3激活状态
        /// </summary>
        public const string PlaceToRollerB2ActiveStatus = "M6797";

        /// <summary>
        /// 放入Roller3完成状态
        /// </summary>
        public const string PlaceToRollerB2DoneStatus = "M6813";

        /// <summary>
        /// 模块报警
        /// </summary>
        public const string StationModuleAlarm = "M6785";

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
        public const string StationManualMode = "M2465";

        /// <summary>
        /// 自动模式
        /// </summary>
        public const string StationAutoMode = "M2464";
        #endregion


    }
}
