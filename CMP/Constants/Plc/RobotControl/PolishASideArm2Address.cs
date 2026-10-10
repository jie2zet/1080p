namespace CMP.Constants.Plc.RobotControl;

/// <summary>
/// PolishASideArm2 PLC 地址定义
/// </summary>
public static class PolishASideArm2Address
{
    #region 轴位置调整 - X轴 (A手臂2x轴)

    /// <summary>
    /// 待机位
    /// </summary>
    public const string XStandbyPosition = "ZR10618";

    /// <summary>
    /// 待机位移动
    /// </summary>
    public const string XStandbyPositionMove = "M1208";

    /// <summary>
    /// 中转手臂位
    /// </summary>
    public const string XTransferArmPosition = "ZR10620";

    /// <summary>
    /// 中转手臂位移动
    /// </summary>
    public const string XTransferArmPositionMove = "M1209";

    /// <summary>
    /// 研磨A位
    /// </summary>
    public const string XPolishAPosition = "ZR10622";

    /// <summary>
    /// 研磨A位移动
    /// </summary>
    public const string XPolishAPositionMove = "M1210";

    /// <summary>
    /// 研磨B位
    /// </summary>
    public const string XPolishBPosition = "ZR10624";

    /// <summary>
    /// 研磨B位移动
    /// </summary>
    public const string XPolishBPositionMove = "M1211";

    /// <summary>
    /// JOG 速度
    /// </summary>
    public const string XJogSpeed = "D1122";

    /// <summary>
    /// 自动速度
    /// </summary>
    public const string XAutoSpeed = "ZR10600";

    /// <summary>
    /// 当前坐标
    /// </summary>
    public const string XCurrentCoordinate = "D5120";

    /// <summary>
    /// 使能
    /// </summary>
    public const string XEnable = "M1192";

    /// <summary>
    /// 使能状态
    /// </summary>
    public const string XEnableStatus = "M5288";

    /// <summary>
    /// JOG 上移
    /// </summary>
    public const string XJogUp = "M1193";

    /// <summary>
    /// JOG 上移激活状态
    /// </summary>
    public const string XJogUpStatus = "M5292";

    /// <summary>
    /// JOG 中间激活状态
    /// </summary>
    public const string XJogMiddleStatus = "M5294";

    /// <summary>
    /// JOG 下移
    /// </summary>
    public const string XJogDown = "M1194";

    /// <summary>
    /// JOG 下移激活状态
    /// </summary>
    public const string XJogDownStatus = "M5293";

    /// <summary>
    /// 驱动器报警
    /// </summary>
    public const string XDriverAlarm = "M5297";

    /// <summary>
    /// 限位报警
    /// </summary>
    public const string XLimitAlarm = "M5295";

    /// <summary>
    /// 报警代码
    /// </summary>
    public const string XAlarmCode = "D5126";

    /// <summary>
    /// 回原点命令
    /// </summary>
    public const string XOrigin = "M1195";

    /// <summary>
    /// 原点状态
    /// </summary>
    public const string XOriginStatus = "M5289";

    #endregion

    #region 轴位置调整 - Z轴 (A手臂2z轴)

    /// <summary>
    /// 待机位
    /// </summary>
    public const string ZStandbyPosition = "ZR10718";

    /// <summary>
    /// 待机位移动
    /// </summary>
    public const string ZStandbyPositionMove = "M1240";

    /// <summary>
    /// 中转手臂位
    /// </summary>
    public const string ZTransferArmPosition = "ZR10720";

    /// <summary>
    /// 中转手臂位移动
    /// </summary>
    public const string ZTransferArmPositionMove = "M1241";

    /// <summary>
    /// 研磨A下片位
    /// </summary>
    public const string ZPolishAUnloadPosition = "ZR10722";

    /// <summary>
    /// 研磨A下片位移动
    /// </summary>
    public const string ZPolishAUnloadPositionMove = "M1242";

    /// <summary>
    /// 研磨B上片位
    /// </summary>
    public const string ZPolishBLoadPosition = "ZR10724";

    /// <summary>
    /// 研磨B上片位移动
    /// </summary>
    public const string ZPolishBLoadPositionMove = "M1243";

    /// <summary>
    /// 研磨A上片位
    /// </summary>
    public const string ZPolishALoadPosition = "ZR10726";

    /// <summary>
    /// 研磨A上片位移动
    /// </summary>
    public const string ZPolishALoadPositionMove = "M1244";

    /// <summary>
    /// 研磨B下片位
    /// </summary>
    public const string ZPolishBUnloadPosition = "ZR10728";

    /// <summary>
    /// 研磨B下片位移动
    /// </summary>
    public const string ZPolishBUnloadPositionMove = "M1245";

    /// <summary>
    /// JOG 速度
    /// </summary>
    public const string ZJogSpeed = "D1142";

    /// <summary>
    /// 自动速度
    /// </summary>
    public const string ZAutoSpeed = "ZR10700";

    /// <summary>
    /// 当前坐标
    /// </summary>
    public const string ZCurrentCoordinate = "D5140";

    /// <summary>
    /// 使能
    /// </summary>
    public const string ZEnable = "M1224";

    /// <summary>
    /// 使能状态
    /// </summary>
    public const string ZEnableStatus = "M5336";

    /// <summary>
    /// JOG 上移
    /// </summary>
    public const string ZJogUp = "M1225";

    /// <summary>
    /// JOG 上移激活状态
    /// </summary>
    public const string ZJogUpStatus = "M5340";

    /// <summary>
    /// JOG 中间激活状态
    /// </summary>
    public const string ZJogMiddleStatus = "M5342";

    /// <summary>
    /// JOG 下移
    /// </summary>
    public const string ZJogDown = "M1226";

    /// <summary>
    /// JOG 下移激活状态
    /// </summary>
    public const string ZJogDownStatus = "M5341";

    /// <summary>
    /// 驱动器报警
    /// </summary>
    public const string ZDriverAlarm = "M5345";

    /// <summary>
    /// 限位报警
    /// </summary>
    public const string ZLimitAlarm = "M5343";

    /// <summary>
    /// 报警代码
    /// </summary>
    public const string ZAlarmCode = "D5146";

    /// <summary>
    /// 回原点命令
    /// </summary>
    public const string ZOrigin = "M1227";

    /// <summary>
    /// 原点状态
    /// </summary>
    public const string ZOriginStatus = "M5337";

    /// <summary>
    /// 待机位到位
    /// </summary>
    public const string ZStandbyPositionArrived = "M5368";

    /// <summary>
    /// 中转手臂位到位
    /// </summary>
    public const string ZTransferArmPositionArrived = "M5369";

    /// <summary>
    /// 研磨A下片位到位
    /// </summary>
    public const string ZPolishAUnloadPositionArrived = "M5370";

    /// <summary>
    /// 研磨B上片位到位
    /// </summary>
    public const string ZPolishBLoadPositionArrived = "M5371";

    /// <summary>
    /// 研磨A上片位到位
    /// </summary>
    public const string ZPolishALoadPositionArrived = "M5372";

    /// <summary>
    /// 研磨B下片位到位
    /// </summary>
    public const string ZPolishBUnloadPositionArrived = "M5373";

    /// <summary>
    /// 加速度时间
    /// </summary>
    public const string AccelerationTime = "ZR10712";

    /// <summary>
    /// 减速度时间
    /// </summary>
    public const string DeaccelerationTime = "ZR10714";

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
    public const string StationReset = "M2072";

    /// <summary>
    /// 工位操作 - 复位状态
    /// </summary>
    public const string StationResetStatus = "M6393";

    /// <summary>
    /// 工位操作 - 启动
    /// </summary>
    public const string StationStart = "M2075";

    /// <summary>
    /// 工位操作 - 运行状态
    /// </summary>
    public const string StationRunningStatus = "M6395";

    /// <summary>
    /// 工位操作 - 停止
    /// </summary>
    public const string StationStop = "M2077";

    /// <summary>
    /// 工位操作 - 停止状态
    /// </summary>
    public const string StationStoppedStatus = "M6397";

    /// <summary>
    /// 从中转取料
    /// </summary>
    public const string PickFromTransfer = "M2088";

    /// <summary>
    /// 从中转取料激活状态
    /// </summary>
    public const string PickFromTransferActiveStatus = "M6408";

    /// <summary>
    /// 从中转取料完成状态
    /// </summary>
    public const string PickFromTransferDoneStatus = "M6424";

    /// <summary>
    /// 放入PolishA (Revised: 从PolishA取)
    /// </summary>
    public const string PickFromPolishA = "M2089";

    /// <summary>
    /// 放入PolishA激活状态
    /// </summary>
    public const string PickFromPolishAActiveStatus = "M6409";

    /// <summary>
    /// 放入PolishA完成状态
    /// </summary>
    public const string PickFromPolishADoneStatus = "M6425";

    /// <summary>
    /// 放入PolishB (Revised: 放入PolishB)
    /// </summary>
    public const string PickFromPolishB = "M2090";

    /// <summary>
    /// 放入PolishB激活状态
    /// </summary>
    public const string PickFromPolishBActiveStatus = "M6410";

    /// <summary>
    /// 放入PolishB完成状态
    /// </summary>
    public const string PickFromPolishBDoneStatus = "M6426";

    /// <summary>
    /// 备用1 (Revised: 放回中转手臂)
    /// </summary>
    public const string PlaceToTransferArm = "M2091";

    /// <summary>
    /// 备用1激活状态
    /// </summary>
    public const string PlaceToTransferArmActiveStatus = "M6411";

    /// <summary>
    /// 备用1完成状态
    /// </summary>
    public const string PlaceToTransferArmDoneStatus = "M6427";

    /// <summary>
    /// 备用2 (Revised: 从PolishB取料)
    /// </summary>
    public const string PickFromPolishBMaterial = "M2092";

    /// <summary>
    /// 备用2激活状态
    /// </summary>
    public const string PickFromPolishBMaterialActiveStatus = "M6412";

    /// <summary>
    /// 备用2完成状态
    /// </summary>
    public const string PickFromPolishBMaterialDoneStatus = "M6428";

    /// <summary>
    /// PolishB上下料
    /// </summary>
    public const string PolishBLoadUnload = "M2103";

    /// <summary>
    /// PolishB上下料激活状态
    /// </summary>
    public const string PolishBLoadUnloadActiveStatus = "M6423";

    /// <summary>
    /// PolishB上下料完成状态
    /// </summary>
    public const string PolishBLoadUnloadDoneStatus = "M6439";

    /// <summary>
    /// 模块报警
    /// </summary>
    public const string ModuleAlarm = "M6401";

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
    public const string ManualMode = "M2081";

    /// <summary>
    /// 自动模式
    /// </summary>
    public const string AutoMode = "M2080";

    /// <summary>
    /// 手臂2光纤1
    /// </summary>
    public const string Arm2Fiber1 = "X1013";

    /// <summary>
    /// 手臂2光纤2
    /// </summary>
    public const string Arm2Fiber2 = "X1014";

    /// <summary>
    /// 手臂2光纤3
    /// </summary>
    public const string Arm2Fiber3 = "X1015";

    /// <summary>
    /// Foup/Slot
    /// </summary>
    public const string FoupSlot = "W1A20";

    /// <summary>
    /// Index
    /// </summary>
    public const string WaferIndex = "W1A21";
    /// <summary>
    /// Power Off
    /// </summary>
    public const string PowerOff = "M10401";

    #endregion
}
