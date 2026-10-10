using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Constants.Plc.RobotControl
{
    public static class PolishBSideArm2Address
    {
        #region B侧手臂2x轴位置设定
        #region 位置设定 (D)
        /// <summary>
        /// 2x轴待机位
        /// </summary>
        public const string StandbyPosition_2x = "ZR11318";
        /// <summary>
        /// 2x轴中转手臂位
        /// </summary>
        public const string TransferArmPosition_2x = "ZR11320";
        /// <summary>
        /// 2x轴研磨C位
        /// </summary>
        public const string PolishCPosition_2x = "ZR11322";
        /// <summary>
        /// 2x轴研磨D位
        /// </summary>
        public const string PolishDPosition_2x = "ZR11324";
        
        /// <summary>
        /// 1x轴有晶圆时速度
        /// </summary>
        public const string SpeedWhenHasWafer1x = "ZR11302";
        /// <summary>
        /// 1x轴加速时间
        /// </summary>
        public const string AccelerationTime1x = "ZR11312";
        /// <summary>
        /// 1x轴减速时间
        /// </summary>
        public const string DeaccelerationTime1x = "ZR11314";
        #endregion

        #region 移动命令 (M)
        /// <summary>
        /// 2x轴待机位移动
        /// </summary>
        public const string StandbyPosMove_2x = "M1432";
        /// <summary>
        /// 2x轴待机位移动
        /// </summary>
        public const string TransferArmMove_2x = "M1433";
        /// <summary>
        /// 2x轴待机位移动
        /// </summary>
        public const string PolishCMove_2x = "M1434";
        /// <summary>
        /// 2x轴待机位移动
        /// </summary>
        public const string PolishDMove_2x = "M1435";
        #endregion

        #region 速度设定 (D)
        /// <summary>
        /// 2x轴Jog速度
        /// </summary>
        public const string JogSpeed_2x = "D1262";
        /// <summary>
        /// 2x轴Jog速度
        /// </summary>
        public const string AutoSpeed_2x = "ZR11300";
        #endregion

        #region 当前坐标 (D)
        /// <summary>
        /// 2x轴当前坐标
        /// </summary>
        public const string CurrentCoordinate_2x = "D5260";
        #endregion

        #region 使能状态 (M)
        /// <summary>
        /// 2x轴使能
        /// </summary>
        public const string Enable_2x = "M1416";
        /// <summary>
        /// 2x轴使能状态
        /// </summary>
        public const string EnableStatus_2x = "M5624";
        #endregion

        #region 报警状态 (D/M)
        /// <summary>
        /// 2x轴报警码
        /// </summary>
        public const string AlarmCode_2x = "D5266";
        /// <summary>
        /// 2x轴驱动器报警
        /// </summary>
        public const string DriverAlarm_2x = "M5633";
        /// <summary>
        /// 2x轴限位报警
        /// </summary>
        public const string LimitAlarm_2x = "M5631";
        #endregion

        #region JOG控制 (M)
        /// <summary>
        /// 2x轴 JOG 上移
        /// </summary>
        public const string JogUp_2x = "M1417";
        /// <summary>
        /// 2x轴 JOG 上移激活状态
        /// </summary>
        public const string JogUpStatus_2x = "M5628";
        /// <summary>
        /// 2x轴 JOG 中间激活状态
        /// </summary>
        public const string JogMiddleStatus_2x = "M5630";
        /// <summary>
        /// 2x轴 JOG 下移
        /// </summary>
        public const string JogDown_2x = "M1418";
        /// <summary>
        /// 2x轴 JOG 下移激活状态
        /// </summary>
        public const string JogDownStatus_2x = "M5629";
        #endregion

        #region 原点 (M)
        /// <summary>
        /// 2x轴 回原点命令
        /// </summary>
        public const string Origin_2x = "M1419";

        /// <summary>
        /// 2x轴 原点状态
        /// </summary>
        public const string OriginStatus_2x = "M5625";

        #endregion
        #endregion

        #region B侧手臂2z轴位置设定
        #region 位置设定 (D)
        /// <summary>
        /// 2z轴待机位
        /// </summary>
        public const string StandbyPosition_2z = "ZR11418";
        /// <summary>
        /// 2z轴中转手臂位
        /// </summary>
        public const string TransferArmPosition_2z = "ZR11420";
        /// <summary>
        /// 2z轴研磨C下片
        /// </summary>
        public const string PolishCDownPiece = "ZR11422";
        /// <summary>
        /// 2z轴研磨D上片
        /// </summary>
        public const string PolishDUpPiece = "ZR11424";
        /// <summary>
        /// 2z轴研磨C上片
        /// </summary>
        public const string PolishCUpPiece = "ZR11426";
        /// <summary>
        /// 2z轴研磨D下片
        /// </summary>
        public const string PolishDDownPiece = "ZR11428";
        /// <summary>
        /// 2z轴有晶圆时速度
        /// </summary>
        public const string SpeedWhenHasWafer = "ZR11402";
        /// <summary>
        /// 2z轴加速时间
        /// </summary>
        public const string AccelerationTime = "ZR11412";
        /// <summary>
        /// 2z轴减速时间
        /// </summary>
        public const string DeaccelerationTime = "ZR11414";
        #endregion

        #region 移动命令 (M)
        /// <summary>
        /// 2z轴待机位移动
        /// </summary>
        public const string StandbyPosMove_2z = "M1464";
        /// <summary>
        /// 2z轴待机位移动
        /// </summary>
        public const string TransferArmMove_2z = "M1465";
        /// <summary>
        /// 2z轴研磨C下片移动
        /// </summary>
        public const string PolishCDownPieceMove = "M1466";
        /// <summary>
        /// 2z轴研磨D上片移动
        /// </summary>
        public const string PolishDUpPieceMove = "M1467";
        /// <summary>
        /// 2z轴研磨C上片移动
        /// </summary>
        public const string PolishCUpPieceMove = "M1468";
        /// <summary>
        /// 2z轴研磨D下片移动
        /// </summary>
        public const string PolishDDownPieceMove = "M1469";
        /// <summary>
        /// 2z轴待机位移动状态
        /// </summary>
        public const string StandbyPosMove_2zStatus = "M5704";
        /// <summary>
        /// 2z轴中转手臂位移动状态
        /// </summary>
        public const string TransferArmMove_2zStatus = "M5705";
        /// <summary>
        /// 2z轴研磨C下片移动状态
        /// </summary>
        public const string PolishCDownPieceMoveStatus = "M5706";
        /// <summary>
        /// 2z轴研磨D上片移动状态
        /// </summary>
        public const string PolishDUpPieceMoveStatus = "M5707";
        /// <summary>
        /// 2z轴研磨C上片移动状态
        /// </summary>
        public const string PolishCUpPieceMoveStatus = "M5708";
        /// <summary>
        /// 2z轴研磨D下片移动状态
        /// </summary>
        public const string PolishDDownPieceMoveStatus = "M5709";

        #endregion

        #region 速度设定 (D)
        /// <summary>
        /// 2z轴Jog速度
        /// </summary>
        public const string JogSpeed_2z = "D1282";
        /// <summary>
        /// 2z轴Jog速度
        /// </summary>
        public const string AutoSpeed_2z = "ZR11400";
        #endregion

        #region 当前坐标 (D)
        /// <summary>
        /// 2z轴当前坐标
        /// </summary>
        public const string CurrentCoordinate_2z = "D5280";
        #endregion

        #region 使能状态 (M)
        /// <summary>
        /// 2z轴使能
        /// </summary>
        public const string Enable_2z = "M1448";
        /// <summary>
        /// 2z轴使能状态
        /// </summary>
        public const string EnableStatus_2z = "M5672";
        #endregion

        #region 报警状态 (D/M)
        /// <summary>
        /// 2z轴报警码
        /// </summary>
        public const string AlarmCode_2z = "D5286";
        /// <summary>
        /// 2z轴驱动器报警
        /// </summary>
        public const string DriverAlarm_2z = "M5681";
        /// <summary>
        /// 2z轴限位报警
        /// </summary>
        public const string LimitAlarm_2z = "M5679";
        #endregion

        #region JOG控制 (M)
        /// <summary>
        /// 2z轴 JOG 上移
        /// </summary>
        public const string JogUp_2z = "M1449";
        /// <summary>
        /// 2z轴 JOG 上移激活状态
        /// </summary>
        public const string JogUpStatus_2z = "M5676";
        /// <summary>
        /// 2z轴 JOG 中间激活状态
        /// </summary>
        public const string JogMiddleStatus_2z = "M5678";
        /// <summary>
        /// 2z轴 JOG 下移
        /// </summary>
        public const string JogDown_2z = "M1450";
        /// <summary>
        /// 2z轴 JOG 下移激活状态
        /// </summary>
        public const string JogDownStatus_2z = "M5677";
        #endregion

        #region 原点 (M)
        /// <summary>
        /// 2z轴 回原点命令
        /// </summary>
        public const string Origin_2z = "M1451";

        /// <summary>
        /// 2z轴 原点状态
        /// </summary>
        public const string OriginStatus_2z = "M5673";

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
        public const string StationReset = "M2264";

        /// <summary>
        /// 工位操作 - 复位状态
        /// </summary>
        public const string StationResetStatus = "M6585";

        /// <summary>
        /// 工位操作 - 启动
        /// </summary>
        public const string StationStart = "M2267";

        /// <summary>
        /// 工位操作 - 运行状态
        /// </summary>
        public const string StationRunningStatus = "M6587";

        /// <summary>
        /// 工位操作 - 停止
        /// </summary>
        public const string StationStop = "M2269";

        /// <summary>
        /// 工位操作 - 停止状态
        /// </summary>
        public const string StationStoppedStatus = "M6589";

        /// <summary>
        /// 从中转取料
        /// </summary>
        public const string PickFromTransfer = "M2280";

        /// <summary>
        /// 从中转取料激活状态
        /// </summary>
        public const string PickFromTransferActiveStatus = "M6600";

        /// <summary>
        /// 从中转取料完成状态
        /// </summary>
        public const string PickFromTransferDoneStatus = "M6616";

        /// <summary>
        /// 从PolishC取料
        /// </summary>
        public const string PickFromPolishC = "M2281";

        /// <summary>
        /// 从PolishC取料激活状态
        /// </summary>
        public const string PickFromPolishCActiveStatus = "M6601";

        /// <summary>
        /// 从PolishC取料完成状态
        /// </summary>
        public const string PickFromPolishCDoneStatus = "M6617";

        /// <summary>
        /// 放入PolishD
        /// </summary>
        public const string PlaceToPolishD = "M2282";

        /// <summary>
        /// 放入PolishD激活状态
        /// </summary>
        public const string PlaceToPolishDActiveStatus = "M6602";

        /// <summary>
        /// 放入PolishD完成状态
        /// </summary>
        public const string PlaceToPolishDDoneStatus = "M6618";

        /// <summary>
        /// 放到中转
        /// </summary>
        public const string PlaceToTransfer = "M2283";

        /// <summary>
        /// 放到中转激活状态
        /// </summary>
        public const string PlaceToTransferActiveStatus = "M6603";

        /// <summary>
        /// 放到中转完成状态
        /// </summary>
        public const string PlaceToTransferDoneStatus = "M6619";

        /// <summary>
        /// 备用1
        /// </summary>
        public const string Spare1 = "M2284";

        /// <summary>
        /// 备用1激活状态
        /// </summary>
        public const string Spare1ActiveStatus = "M6604";

        /// <summary>
        /// 备用1完成状态
        /// </summary>
        public const string Spare1DoneStatus = "M6620";

        /// <summary>
        /// 模块报警
        /// </summary>
        public const string ModuleAlarm = "M6593";

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
        public const string ManualMode = "M2273";

        /// <summary>
        /// 自动模式
        /// </summary>
        public const string AutoMode = "M2272";

        /// <summary>
        /// 手臂1光纤1
        /// </summary>
        public const string Arm2Fiber1 = "X1023";

        /// <summary>
        /// 手臂1光纤2
        /// </summary>
        public const string Arm2Fiber2 = "X1024";

        /// <summary>
        /// 手臂1光纤3
        /// </summary>
        public const string Arm2Fiber3 = "X1025";
        /// <summary>
        /// 电源关闭
        /// </summary>
        public const string PowerOff = "M10404";
        /// <summary>
        /// 晶圆槽位
        /// </summary>
        public const string FoupSlot = "W1C20";
        /// <summary>
        /// 晶圆索引
        /// </summary>
        public const string WaferIndex = "W1C21";

        #endregion
    }
}
