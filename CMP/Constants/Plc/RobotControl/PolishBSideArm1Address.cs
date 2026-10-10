using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Constants.Plc.RobotControl
{
    public static class PolishBSideArm1Address
    {
        #region B侧手臂1x轴位置设定
        #region 位置设定 (D)
        /// <summary>
        /// 1x轴待机位
        /// </summary>
        public const string StandbyPosition_1x = "ZR11118";
        /// <summary>
        /// 1x轴中转手臂位
        /// </summary>
        public const string TransferArmPosition_1x = "ZR11120";
        /// <summary>
        /// 1x轴研磨C位
        /// </summary>
        public const string PolishCPosition_1x = "ZR11122";
        /// <summary>
        /// 1x轴研磨D位
        /// </summary>
        public const string PolishDPosition_1x = "ZR11124";
        /// <summary>
        /// 1x轴加速时间
        /// </summary>
        public const string AccelerationTime_1x = "ZR11112";
        /// <summary>
        /// 1x轴减速时间
        /// </summary>
        public const string DeaccelerationTime_1x = "ZR11114";
        #endregion

        #region 移动命令 (M)
        /// <summary>
        /// 1x轴待机位移动
        /// </summary>
        public const string StandbyPosMove_1x = "M1368";
        /// <summary>
        /// 1x轴中转手臂位移动
        /// </summary>
        public const string TransferArmMove_1x = "M1369";
        /// <summary>
        /// 1x轴研磨C位移动
        /// </summary>
        public const string PolishCMove_1x = "M1370";
        /// <summary>
        /// 1x轴研磨D位移动
        /// </summary>
        public const string PolishDMove_1x = "M1371";
        #endregion

        #region 速度设定 (D)
        /// <summary>
        /// 1x轴Jog速度
        /// </summary>
        public const string JogSpeed_1x = "D1222";
        /// <summary>
        /// 1x轴自动速度
        /// </summary>
        public const string AutoSpeed_1x = "ZR11100";
        #endregion

        #region 当前坐标 (D)
        /// <summary>
        /// 1x轴当前坐标
        /// </summary>
        public const string CurrentCoordinate_1x = "D5220";
        #endregion

        #region 使能状态 (M)
        /// <summary>
        /// 1x轴使能
        /// </summary>
        public const string Enable_1x = "M1352";
        /// <summary>
        /// 1x轴使能状态
        /// </summary>
        public const string EnableStatus_1x = "M5528";
        #endregion

        #region 报警状态 (D/M)
        /// <summary>
        /// 1x轴报警码
        /// </summary>
        public const string AlarmCode_1x = "D5226";
        /// <summary>
        /// 1x轴驱动器报警
        /// </summary>
        public const string DriverAlarm_1x = "M5537";
        /// <summary>
        /// 1x轴限位报警
        /// </summary>
        public const string LimitAlarm_1x = "M5535";
        #endregion

        #region JOG控制 (M)
        /// <summary>
        ///  1x轴 JOG 上移
        /// </summary>
        public const string JogUp_1x = "M1353";
        /// <summary>
        /// 1x轴 JOG 上移激活状态
        /// </summary>
        public const string JogUpStatus_1x = "M5532";
        /// <summary>
        /// 1x轴 JOG 中间激活状态
        /// </summary>
        public const string JogMiddleStatus_1x = "M5534";
        /// <summary>
        /// 1x轴 JOG 下移
        /// </summary>
        public const string JogDown_1x = "M1354";
        /// <summary>
        /// 1x轴 JOG 下移激活状态
        /// </summary>
        public const string JogDownStatus_1x = "M5533";
        #endregion

        #region 原点 (M)

        /// <summary>
        /// 1x轴 回原点命令
        /// </summary>
        public const string Origin_1x = "M1355";

        /// <summary>
        /// 1x轴 原点状态
        /// </summary>
        public const string OriginStatus_1x = "M5529";
        #endregion
        #endregion

        #region B侧手臂1z轴位置设定
        #region 位置设定 (D)
        /// <summary>
        /// 1z轴待机位
        /// </summary>
        public const string StandbyPosition_1z = "ZR11218";
        /// <summary>
        /// 1z轴中转手臂位
        /// </summary>
        public const string TransferArmPosition_1z = "ZR11220";
        /// <summary>
        /// 1z轴研磨C位
        /// </summary>
        public const string PolishCPosition_1z = "ZR11222";
        /// <summary>
        /// 1z轴研磨D位
        /// </summary>
        public const string PolishDPosition_1z = "ZR11224";
        /// <summary>
        /// 有晶圆时速度
        /// </summary>
        public const string SpeedWhenHasWafer = "ZR11202";
        /// <summary>
        /// 1z轴加速时间
        /// </summary>
        public const string AccelerationTime = "ZR11212";
        /// <summary>
        /// 1z轴减速时间
        /// </summary>
        public const string DeaccelerationTime = "ZR11214";
        #endregion

        #region 移动命令 (M)
        /// <summary>
        /// 1z轴待机位移动
        /// </summary>
        public const string StandbyPosMove_1z = "M1400";
        /// <summary>
        /// 1z轴待机位移动
        /// </summary>
        public const string TransferArmMove_1z = "M1401";
        /// <summary>
        /// 1z轴待机位移动
        /// </summary>
        public const string PolishCMove_1z = "M1402";
        /// <summary>
        /// 1z轴待机位移动
        /// </summary>
        public const string PolishDMove_1z = "M1403";
        #endregion

        #region 速度设定 (D)
        /// <summary>
        /// 1z轴Jog速度
        /// </summary>
        public const string JogSpeed_1z = "D1242";
        /// <summary>
        /// 1z轴Jog速度
        /// </summary>
        public const string AutoSpeed_1z = "ZR11200";
        #endregion

        #region 当前坐标 (D)
        /// <summary>
        /// 1z轴当前坐标
        /// </summary>
        public const string CurrentCoordinate_1z = "D5240";
        #endregion

        #region 使能状态 (M)
        /// <summary>
        /// 1z轴使能
        /// </summary>
        public const string Enable_1z = "M1384";
        /// <summary>
        /// 1z轴使能状态
        /// </summary>
        public const string EnableStatus_1z = "M5576";
        #endregion

        #region 报警状态 (D/M)
        /// <summary>
        /// 1z轴报警码
        /// </summary>
        public const string AlarmCode_1z = "D5246";
        /// <summary>
        /// 1z轴驱动器报警
        /// </summary>
        public const string DriverAlarm_1z = "M5585";
        /// <summary>
        /// 1z轴限位报警
        /// </summary>
        public const string LimitAlarm_1z = "M5583";
        #endregion

        #region JOG控制 (M)
        /// <summary>
        /// 1z轴 JOG 上移
        /// </summary>
        public const string JogUp_1z = "M1385";
        /// <summary>
        /// 1z轴 JOG 上移激活状态
        /// </summary>
        public const string JogUpStatus_1z = "M5580";
        /// <summary>
        /// 1z轴 JOG 中间激活状态
        /// </summary>
        public const string JogMiddleStatus_1z = "M5582";
        /// <summary>
        /// 1z轴 JOG 下移
        /// </summary>
        public const string JogDown_1z = "M1386";
        /// <summary>
        /// 1z轴 JOG 下移激活状态
        /// </summary>
        public const string JogDownStatus_1z = "M5581";
        #endregion

        #region 原点 (M)
        /// <summary>
        /// 1z轴 回原点命令
        /// </summary>
        public const string Origin_1z = "M1387";

        /// <summary>
        /// 1z轴 原点状态
        /// </summary>
        public const string OriginStatus_1z = "M5577";

        #endregion
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
        public const string StationReset = "M2216";

        /// <summary>
        /// 工位操作 - 复位状态
        /// </summary>
        public const string StationResetStatus = "M6537";

        /// <summary>
        /// 工位操作 - 启动
        /// </summary>
        public const string StationStart = "M2219";

        /// <summary>
        /// 工位操作 - 运行状态
        /// </summary>
        public const string StationRunningStatus = "M6539";

        /// <summary>
        /// 工位操作 - 停止
        /// </summary>
        public const string StationStop = "M2221";

        /// <summary>
        /// 工位操作 - 停止状态
        /// </summary>
        public const string StationStoppedStatus = "M6541";

        /// <summary>
        /// 从中转取料
        /// </summary>
        public const string PickFromTransfer = "M2232";

        /// <summary>
        /// 从中转取料激活状态
        /// </summary>
        public const string PickFromTransferActiveStatus = "M6552";

        /// <summary>
        /// 从中转取料完成状态
        /// </summary>
        public const string PickFromTransferDoneStatus = "M6568";

        /// <summary>
        /// 放入PolishC
        /// </summary>
        public const string PlaceToPolishC = "M2233";

        /// <summary>
        /// 放入PolishC激活状态
        /// </summary>
        public const string PlaceToPolishCActiveStatus = "M6553";

        /// <summary>
        /// 放入PolishC完成状态
        /// </summary>
        public const string PlaceToPolishCDoneStatus = "M6569";

        /// <summary>
        /// 放入PolishD
        /// </summary>
        public const string PlaceToPolishD = "M2234";

        /// <summary>
        /// 放入PolishD激活状态
        /// </summary>
        public const string PlaceToPolishDActiveStatus = "M6554";

        /// <summary>
        /// 放入PolishD完成状态
        /// </summary>
        public const string PlaceToPolishDDoneStatus = "M6570";

        /// <summary>
        /// 备用1
        /// </summary>
        public const string Spare1 = "M2235";

        /// <summary>
        /// 备用1激活状态
        /// </summary>
        public const string Spare1ActiveStatus = "M6555";

        /// <summary>
        /// 备用1完成状态
        /// </summary>
        public const string Spare1DoneStatus = "M6571";

        /// <summary>
        /// 备用2
        /// </summary>
        public const string Spare2 = "M2236";

        /// <summary>
        /// 备用2激活状态
        /// </summary>
        public const string Spare2ActiveStatus = "M6556";

        /// <summary>
        /// 备用2完成状态
        /// </summary>
        public const string Spare2DoneStatus = "M6572";

        /// <summary>
        /// 模块报警
        /// </summary>
        public const string ModuleAlarm = "M6545";

        /// <summary>
        /// 全局报警
        /// </summary>
        public const string GlobalAlarm = "M89";

        /// <summary>
        /// 工位报警复位
        /// </summary>
        public const string StationAlarmReset = "B0B";

        /// <summary>
        /// 手动模式
        /// </summary>
        public const string ManualMode = "M2225";

        /// <summary>
        /// 自动模式
        /// </summary>
        public const string AutoMode = "M2224";

        /// <summary>
        /// 手臂1光纤1
        /// </summary>
        public const string Arm1Fiber1 = "X1020";

        /// <summary>
        /// 手臂1光纤2
        /// </summary>
        public const string Arm1Fiber2 = "X1021";

        /// <summary>
        /// 手臂1光纤3
        /// </summary>
        public const string Arm1Fiber3 = "X1022";
        /// <summary>
        /// 电源关闭
        /// </summary>
        public const string PowerOff = "M10403";
        /// <summary>
        /// 晶圆槽位
        /// </summary>
        public const string FoupSlot = "W1C10";
        /// <summary>
        /// 晶圆索引
        /// </summary>
        public const string WaferIndex = "W1C11";
        

        #endregion
    }
}
