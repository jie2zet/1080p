using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Constants.Plc.RobotControl
{
    public static class PolishBSideArm3Address
    {
        #region B侧手臂3x轴位置设定
        #region 位置设定 (D)
        /// <summary>
        /// 3x轴待机位
        /// </summary>
        public const string StandbyPosition_3x = "ZR11518";
        /// <summary>
        /// 3x轴研磨C位
        /// </summary>
        public const string PolishCPosition_3x = "ZR11120";
        /// <summary>
        /// 3x轴研磨D位
        /// </summary>
        public const string PolishDPosition_3x = "ZR11122";
        /// <summary>
        /// 3x轴中转手臂位
        /// </summary>
        public const string TransferArmPosition_3x = "ZR11124";
        
        /// <summary>
        /// 3x轴有晶圆时速度
        /// </summary>
        public const string SpeedWhenHasWafer3x = "ZR11502";
        /// <summary>
        /// 3x轴加速时间
        /// </summary>
        public const string AccelerationTime3x = "ZR11512";
        /// <summary>
        /// 3x轴减速时间
        /// </summary>
        public const string DeaccelerationTime3x = "ZR11514";
        #endregion

        #region 移动命令 (M)
        /// <summary>
        /// 3x轴待机位移动
        /// </summary>
        public const string StandbyPosMove_3x = "M1496";
        /// <summary>
        /// 3x轴研磨C位移动
        /// </summary>
        public const string PolishCMove_3x = "M1497";
        /// <summary>
        /// 3x轴研磨D位移动
        /// </summary>
        public const string PolishDMove_3x = "M1498";
        /// <summary>
        /// 3x轴中转手臂位移动
        /// </summary>
        public const string TransferArmMove_3x = "M1499";
        #endregion

        #region 速度设定 (D)
        /// <summary>
        /// 3x轴Jog速度
        /// </summary>
        public const string JogSpeed_3x = "D1302";
        /// <summary>
        /// 3x轴自动速度
        /// </summary>
        public const string AutoSpeed_3x = "ZR11500";
        #endregion

        #region 当前坐标 (D)
        /// <summary>
        /// 3x轴当前坐标
        /// </summary>
        public const string CurrentCoordinate_3x = "D5300";
        #endregion

        #region 使能状态 (M)
        /// <summary>
        /// 3x轴使能
        /// </summary>
        public const string Enable_3x = "M1480";
        /// <summary>
        /// 3x轴使能状态
        /// </summary>
        public const string EnableStatus_3x = "M5720";
        #endregion

        #region 报警状态 (D/M)
        /// <summary>
        /// 3x轴报警码
        /// </summary>
        public const string AlarmCode_3x = "D5306";
        /// <summary>
        /// 3x轴驱动器报警
        /// </summary>
        public const string DriverAlarm_3x = "M5729";
        /// <summary>
        /// 3x轴限位报警
        /// </summary>
        public const string LimitAlarm_3x = "M5727";
        #endregion

        #region JOG控制 (M)
        /// <summary>
        ///  3x轴 JOG 上移
        /// </summary>
        public const string JogUp_3x = "M1481";
        /// <summary>
        /// 3x轴 JOG 上移激活状态
        /// </summary>
        public const string JogUpStatus_3x = "M5724";
        /// <summary>
        /// 3x轴 JOG 中间激活状态
        /// </summary>
        public const string JogMiddleStatus_3x = "M5726";
        /// <summary>
        /// 3x轴 JOG 下移
        /// </summary>
        public const string JogDown_3x = "M1482";
        /// <summary>
        /// 3x轴 JOG 下移激活状态
        /// </summary>
        public const string JogDownStatus_3x = "M5725";
        #endregion

        #region 原点 (M)

        /// <summary>
        /// 3x轴 回原点命令
        /// </summary>
        public const string Origin_3x = "M1483";

        /// <summary>
        /// 3x轴 原点状态
        /// </summary>
        public const string OriginStatus_3x = "M5721";
        #endregion
        #endregion

        #region B侧手臂3z轴位置设定
        #region 位置设定 (D)
        /// <summary>
        /// 3z轴待机位
        /// </summary>
        public const string StandbyPosition_3z = "ZR11618";
        /// <summary>
        /// 3z轴研磨C位
        /// </summary>
        public const string PolishCPosition_3z = "ZR11620";
        /// <summary>
        /// 3z轴研磨D位
        /// </summary>
        public const string PolishDPosition_3z = "ZR11622";
        /// <summary>
        /// 3z轴中转手臂位
        /// </summary>
        public const string TransferArmPosition_3z = "ZR11624";
        
        /// <summary>
        /// 3z轴有晶圆时速度
        /// </summary>
        public const string SpeedWhenHasWafer3z = "ZR11602";
        /// <summary>
        /// 3z轴加速时间
        /// </summary>
        public const string AccelerationTime3z = "ZR11612";
        /// <summary>
        /// 3z轴减速时间
        /// </summary>
        public const string DeaccelerationTime3z = "ZR11614";
        #endregion

        #region 移动命令 (M)
        /// <summary>
        /// 3z轴待机位移动
        /// </summary>
        public const string StandbyPosMove_3z = "M1528";
        /// <summary>
        /// 3z轴研磨C位移动
        /// </summary>
        public const string PolishCMove_3z = "M1529";
        /// <summary>
        /// 3z轴研磨D位移动
        /// </summary>
        public const string PolishDMove_3z = "M1530";
        /// <summary>
        /// 3z轴中转手臂位移动
        /// </summary>
        public const string TransferArmMove_3z = "M1531";
        /// <summary>
        /// 3z轴移动完成状态
        /// </summary>
        public const string StandbyPosMove_3zStatus = "M5800";
        /// <summary>
        /// 3z轴研磨C位移动完成状态
        /// </summary>
        public const string PolishCMove_3zStatus = "M5801";
        /// <summary>
        /// 3z轴研磨D位移动完成状态
        /// </summary>
        public const string PolishDMove_3zStatus = "M5802";
        /// <summary>
        /// 3z轴中转手臂位移动完成状态
        /// </summary>
        public const string TransferArmMove_3zStatus = "M5803";
        #endregion

        #region 速度设定 (D)
        /// <summary>
        /// 3z轴Jog速度
        /// </summary>
        public const string JogSpeed_3z = "D1322";
        /// <summary>
        /// 3z轴自动速度
        /// </summary>
        public const string AutoSpeed_3z = "ZR11600";
        #endregion

        #region 当前坐标 (D)
        /// <summary>
        /// 3z轴当前坐标
        /// </summary>
        public const string CurrentCoordinate_3z = "D5320";
        #endregion

        #region 使能状态 (M)
        /// <summary>
        /// 3z轴使能
        /// </summary>
        public const string Enable_3z = "M1512";
        /// <summary>
        /// 3z轴使能状态
        /// </summary>
        public const string EnableStatus_3z = "M5768";
        #endregion

        #region 报警状态 (D/M)
        /// <summary>
        /// 3z轴报警码
        /// </summary>
        public const string AlarmCode_3z = "D5326";
        /// <summary>
        /// 3z轴驱动器报警
        /// </summary>
        public const string DriverAlarm_3z = "M5777";
        /// <summary>
        /// 3z轴限位报警
        /// </summary>
        public const string LimitAlarm_3z = "M5775";
        #endregion

        #region JOG控制 (M)
        /// <summary>
        ///  3z轴 JOG 上移
        /// </summary>
        public const string JogUp_3z = "M1513";
        /// <summary>
        /// 3z轴 JOG 上移激活状态
        /// </summary>
        public const string JogUpStatus_3z = "M5772";
        /// <summary>
        /// 3z轴 JOG 中间激活状态
        /// </summary>
        public const string JogMiddleStatus_3z = "M5774";
        /// <summary>
        /// 3z轴 JOG 下移
        /// </summary>
        public const string JogDown_3z = "M1514";
        /// <summary>
        /// 3z轴 JOG 下移激活状态
        /// </summary>
        public const string JogDownStatus_3z = "M5773";
        #endregion

        #region 原点 (M)

        /// <summary>
        /// 3z轴 回原点命令
        /// </summary>
        public const string Origin_3z = "M1515";

        /// <summary>
        /// 3z轴 原点状态
        /// </summary>
        public const string OriginStatus_3z = "M5769";
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
        public const string StationReset = "M2312";

        /// <summary>
        /// 工位操作 - 复位状态
        /// </summary>
        public const string StationResetStatus = "M6633";

        /// <summary>
        /// 工位操作 - 启动
        /// </summary>
        public const string StationStart = "M2315";

        /// <summary>
        /// 工位操作 - 运行状态
        /// </summary>
        public const string StationRunningStatus = "M6635";

        /// <summary>
        /// 工位操作 - 停止
        /// </summary>
        public const string StationStop = "M2317";

        /// <summary>
        /// 工位操作 - 停止状态
        /// </summary>
        public const string StationStoppedStatus = "M6637";

        /// <summary>
        /// 从PolishC取料
        /// </summary>
        public const string PickFromPolishC = "M2328";

        /// <summary>
        /// 从PolishC取料激活状态
        /// </summary>
        public const string PickFromPolishCActiveStatus = "M6648";

        /// <summary>
        /// 从PolishC取料完成状态
        /// </summary>
        public const string PickFromPolishCDoneStatus = "M6664";

        /// <summary>
        /// 从PolishD取料
        /// </summary>
        public const string PickFromPolishD = "M2329";

        /// <summary>
        /// 从PolishD取料激活状态
        /// </summary>
        public const string PickFromPolishDActiveStatus = "M6649";

        /// <summary>
        /// 从PolishD取料完成状态
        /// </summary>
        public const string PickFromPolishDDoneStatus = "M6665";

        /// <summary>
        /// 放到中转臂
        /// </summary>
        public const string PlaceToTransferArm = "M2330";

        /// <summary>
        /// 放到中转臂激活状态
        /// </summary>
        public const string PlaceToTransferArmActiveStatus = "M6650";

        /// <summary>
        /// 放到中转臂完成状态
        /// </summary>
        public const string PlaceToTransferArmDoneStatus = "M6666";

        /// <summary>
        /// 备用1
        /// </summary>
        public const string Spare1 = "M2331";

        /// <summary>
        /// 备用1激活状态
        /// </summary>
        public const string Spare1ActiveStatus = "M6651";

        /// <summary>
        /// 备用1完成状态
        /// </summary>
        public const string Spare1DoneStatus = "M6667";

        /// <summary>
        /// 备用2
        /// </summary>
        public const string Spare2 = "M2332";

        /// <summary>
        /// 备用2激活状态
        /// </summary>
        public const string Spare2ActiveStatus = "M6652";

        /// <summary>
        /// 备用2完成状态
        /// </summary>
        public const string Spare2DoneStatus = "M6668";

        /// <summary>
        /// 模块报警
        /// </summary>
        public const string ModuleAlarm = "M6641";

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
        public const string ManualMode = "M2321";

        /// <summary>
        /// 自动模式
        /// </summary>
        public const string AutoMode = "M2320";

        /// <summary>
        /// 手臂3光纤1
        /// </summary>
        public const string Arm3Fiber1 = "X1026";

        /// <summary>
        /// 手臂3光纤2
        /// </summary>
        public const string Arm3Fiber2 = "X1027";

        /// <summary>
        /// 手臂3光纤3
        /// </summary>
        public const string Arm3Fiber3 = "X1028";
        /// <summary>
        /// 电源关闭
        /// </summary>
        public const string PowerOff = "M10405";
        /// <summary>
        /// 晶圆槽位
        /// </summary>
        public const string FoupSlot = "W1C30";
        /// <summary>
        /// 晶圆索引
        /// </summary>
        public const string WaferIndex = "W1C31";

        #endregion
    }
}
