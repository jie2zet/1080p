namespace CMP.Constants.Plc.RobotControl;

/// <summary>
/// PolishASideArm1 PLC 地址定义
/// </summary>
public static class PolishASideArm1Address
{
    #region 轴位置调整 - X轴 (A手臂1x轴)

    /// <summary>
    /// 待机位
    /// </summary>
    public const string XStandbyPosition = "ZR10418";

    /// <summary>
    /// 待机位移动
    /// </summary>
    public const string XStandbyPositionMove = "M1144";

    /// <summary>
    /// 中转手臂位
    /// </summary>
    public const string XTransferArmPosition = "ZR10420";

    /// <summary>
    /// 中转手臂位移动
    /// </summary>
    public const string XTransferArmPositionMove = "M1145";

    /// <summary>
    /// 研磨A位
    /// </summary>
    public const string XPolishAPosition = "ZR10422";

    /// <summary>
    /// 研磨A位移动
    /// </summary>
    public const string XPolishAPositionMove = "M1146";

    /// <summary>
    /// 研磨B位
    /// </summary>
    public const string XPolishBPosition = "ZR10424";

    /// <summary>
    /// 研磨B位移动
    /// </summary>
    public const string XPolishBPositionMove = "M1147";

    /// <summary>
    /// JOG 速度
    /// </summary>
    public const string XJogSpeed = "D1082";

    /// <summary>
    /// 自动速度
    /// </summary>
    public const string XAutoSpeed = "ZR10400";

    /// <summary>
    /// 当前坐标
    /// </summary>
    public const string XCurrentCoordinate = "D5080";

    /// <summary>
    /// 使能
    /// </summary>
    public const string XEnable = "M1128";

    /// <summary>
    /// 使能状态
    /// </summary>
    public const string XEnableStatus = "M5192";

    /// <summary>
    /// JOG 上移
    /// </summary>
    public const string XJogUp = "M1129";

    /// <summary>
    /// JOG 上移激活状态
    /// </summary>
    public const string XJogUpStatus = "M5196";

    /// <summary>
    /// JOG 中间激活状态
    /// </summary>
    public const string XJogMiddleStatus = "M5198";

    /// <summary>
    /// JOG 下移
    /// </summary>
    public const string XJogDown = "M1130";

    /// <summary>
    /// JOG 下移激活状态
    /// </summary>
    public const string XJogDownStatus = "M5197";

    /// <summary>
    /// 驱动器报警
    /// </summary>
    public const string XDriverAlarm = "M5201";

    /// <summary>
    /// 限位报警
    /// </summary>
    public const string XLimitAlarm = "M5199";

    /// <summary>
    /// 报警代码
    /// </summary>
    public const string XAlarmCode = "D5086";

    /// <summary>
    /// 回原点命令
    /// </summary>
    public const string XOrigin = "M1131";

    /// <summary>
    /// 原点状态
    /// </summary>
    public const string XOriginStatus = "M5193";

    #endregion

    #region 轴位置调整 - Z轴 (A手臂1z轴)

    /// <summary>
    /// 待机位
    /// </summary>
    public const string ZStandbyPosition = "ZR10518";

    /// <summary>
    /// 待机位移动
    /// </summary>
    public const string ZStandbyPositionMove = "M1176";

    /// <summary>
    /// 中转手臂位
    /// </summary>
    public const string ZTransferArmPosition = "ZR10520";

    /// <summary>
    /// 中转手臂位移动
    /// </summary>
    public const string ZTransferArmPositionMove = "M1177";

    /// <summary>
    /// 研磨A位
    /// </summary>
    public const string ZPolishAPosition = "ZR10522";

    /// <summary>
    /// 研磨A位移动
    /// </summary>
    public const string ZPolishAPositionMove = "M1178";

    /// <summary>
    /// 研磨B位
    /// </summary>
    public const string ZPolishBPosition = "ZR10524";

    /// <summary>
    /// 研磨B位移动
    /// </summary>
    public const string ZPolishBPositionMove = "M1179";

    /// <summary>
    /// JOG 速度
    /// </summary>
    public const string ZJogSpeed = "D1102";

    /// <summary>
    /// 自动速度
    /// </summary>
    public const string ZAutoSpeed = "ZR10500";

    /// <summary>
    /// 当前坐标
    /// </summary>
    public const string ZCurrentCoordinate = "D5100";

    /// <summary>
    /// 使能
    /// </summary>
    public const string ZEnable = "M1160";

    /// <summary>
    /// 使能状态
    /// </summary>
    public const string ZEnableStatus = "M5240";

    /// <summary>
    /// JOG 上移
    /// </summary>
    public const string ZJogUp = "M1161";

    /// <summary>
    /// JOG 上移激活状态
    /// </summary>
    public const string ZJogUpStatus = "M5244";

    /// <summary>
    /// JOG 中间激活状态
    /// </summary>
    public const string ZJogMiddleStatus = "M5246";

    /// <summary>
    /// JOG 下移
    /// </summary>
    public const string ZJogDown = "M1162";

    /// <summary>
    /// JOG 下移激活状态
    /// </summary>
    public const string ZJogDownStatus = "M5245";

    /// <summary>
    /// 驱动器报警
    /// </summary>
    public const string ZDriverAlarm = "M5249";

    /// <summary>
    /// 限位报警
    /// </summary>
    public const string ZLimitAlarm = "M5247";

    /// <summary>
    /// 报警代码
    /// </summary>
    public const string ZAlarmCode = "D5106";

    /// <summary>
    /// 回原点命令
    /// </summary>
    public const string ZOrigin = "M1163";

    /// <summary>
    /// 原点状态
    /// </summary>
    public const string ZOriginStatus = "M5241";

    /// <summary>
    /// 待机位到位
    /// </summary>
    public const string ZStandbyPositionArrived = "M5272";

    /// <summary>
    /// 中转手臂位到位
    /// </summary>
    public const string ZTransferArmPositionArrived = "M5273";

    /// <summary>
    /// 研磨A位到位
    /// </summary>
    public const string ZPolishAPositionArrived = "M5274";

    /// <summary>
    /// 研磨B位到位
    /// </summary>
    public const string ZPolishBPositionArrived = "M5275";
    /// <summary>
    /// 速度(有晶圆)
    /// </summary>
    public const string SpeedWhenHasWafer = "ZR10502";
    /// <summary>
    /// 加速时间
    /// </summary>
    public const string AccelerationTime = "ZR10512";
    /// <summary>
    /// 减速时间
    /// </summary>
    public const string DecelerationTime = "ZR10514";

    #endregion

    #region 底部状态栏

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
    public const string StationReset = "M2024";

    /// <summary>
    /// 工位操作 - 复位状态
    /// </summary>
    public const string StationResetStatus = "M6345";

    /// <summary>
    /// 工位操作 - 启动
    /// </summary>
    public const string StationStart = "M2027";

    /// <summary>
    /// 工位操作 - 运行状态
    /// </summary>
    public const string StationRunningStatus = "M6347";

    /// <summary>
    /// 工位操作 - 停止
    /// </summary>
    public const string StationStop = "M2029";

    /// <summary>
    /// 工位操作 - 停止状态
    /// </summary>
    public const string StationStoppedStatus = "M6349";

    /// <summary>
    /// 从中转取料
    /// </summary>
    public const string PickFromTransfer = "M2040";

    /// <summary>
    /// 从中转取料激活状态
    /// </summary>
    public const string PickFromTransferActiveStatus = "M6360";

    /// <summary>
    /// 从中转取料完成状态
    /// </summary>
    public const string PickFromTransferDoneStatus = "M6376";

    /// <summary>
    /// 放入PolishA
    /// </summary>
    public const string PlaceToPolishA = "M2041";

    /// <summary>
    /// 放入PolishA激活状态
    /// </summary>
    public const string PlaceToPolishAActiveStatus = "M6361";

    /// <summary>
    /// 放入PolishA完成状态
    /// </summary>
    public const string PlaceToPolishADoneStatus = "M6377";

    /// <summary>
    /// 放入PolishB
    /// </summary>
    public const string PlaceToPolishB = "M2042";

    /// <summary>
    /// 放入PolishB激活状态
    /// </summary>
    public const string PlaceToPolishBActiveStatus = "M6362";

    /// <summary>
    /// 放入PolishB完成状态
    /// </summary>
    public const string PlaceToPolishBDoneStatus = "M6378";

    /// <summary>
    /// 备用1
    /// </summary>
    public const string Spare1 = "M2043";

    /// <summary>
    /// 备用1激活状态
    /// </summary>
    public const string Spare1ActiveStatus = "M6363";

    /// <summary>
    /// 备用1完成状态
    /// </summary>
    public const string Spare1DoneStatus = "M6379";

    /// <summary>
    /// 备用2
    /// </summary>
    public const string Spare2 = "M2044";

    /// <summary>
    /// 备用2激活状态
    /// </summary>
    public const string Spare2ActiveStatus = "M6364";

    /// <summary>
    /// 备用2完成状态
    /// </summary>
    public const string Spare2DoneStatus = "M6380";

    /// <summary>
    /// 模块报警
    /// </summary>
    public const string ModuleAlarm = "M6353";

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
    public const string ManualMode = "M2033";

    /// <summary>
    /// 自动模式
    /// </summary>
    public const string AutoMode = "M2032";

    /// <summary>
    /// 手臂1光纤1
    /// </summary>
    public const string Arm1Fiber1 = "X1010";

    /// <summary>
    /// 手臂1光纤2
    /// </summary>
    public const string Arm1Fiber2 = "X1011";

    /// <summary>
    /// 手臂1光纤3
    /// </summary>
    public const string Arm1Fiber3 = "X1012";

    /// <summary>
    /// Foup/Slot
    /// </summary>
    public const string FoupSlot = "W1A10";

    /// <summary>
    /// Index
    /// </summary>
    public const string WaferIndex = "W1A11";
    /// <summary>
    /// Power Off
    /// </summary>
    public const string PowerOff = "M10400";

    #endregion
}
