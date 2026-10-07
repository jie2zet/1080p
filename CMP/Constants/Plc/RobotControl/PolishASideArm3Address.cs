namespace CMP.Constants.Plc.RobotControl;

/// <summary>
/// PolishASideArm3 PLC 地址定义
/// </summary>
public static class PolishASideArm3Address
{
    #region 轴位置调整 - X轴 (A手臂3x轴)

    /// <summary>
    /// 待机位
    /// </summary>
    public const string XStandbyPosition = "D10818";

    /// <summary>
    /// 待机位移动
    /// </summary>
    public const string XStandbyPositionMove = "M1272";

    /// <summary>
    /// 研磨A位
    /// </summary>
    public const string XPolishAPosition = "D10820";

    /// <summary>
    /// 研磨A位移动
    /// </summary>
    public const string XPolishAPositionMove = "M1273";

    /// <summary>
    /// 研磨B位
    /// </summary>
    public const string XPolishBPosition = "D10822";

    /// <summary>
    /// 研磨B位移动
    /// </summary>
    public const string XPolishBPositionMove = "M1274";

    /// <summary>
    /// 中转手臂位
    /// </summary>
    public const string XTransferArmPosition = "D10824";

    /// <summary>
    /// 中转手臂位移动
    /// </summary>
    public const string XTransferArmPositionMove = "M1275";

    /// <summary>
    /// JOG 速度
    /// </summary>
    public const string XJogSpeed = "D1162";

    /// <summary>
    /// 自动速度
    /// </summary>
    public const string XAutoSpeed = "D10800";

    /// <summary>
    /// 当前坐标
    /// </summary>
    public const string XCurrentCoordinate = "D5160";

    /// <summary>
    /// 使能
    /// </summary>
    public const string XEnable = "M1256";

    /// <summary>
    /// 使能状态
    /// </summary>
    public const string XEnableStatus = "M5384";

    /// <summary>
    /// JOG 上移
    /// </summary>
    public const string XJogUp = "M1257";

    /// <summary>
    /// JOG 上移激活状态
    /// </summary>
    public const string XJogUpStatus = "M5388";

    /// <summary>
    /// JOG 中间激活状态
    /// </summary>
    public const string XJogMiddleStatus = "M5390";

    /// <summary>
    /// JOG 下移
    /// </summary>
    public const string XJogDown = "M1258";

    /// <summary>
    /// JOG 下移激活状态
    /// </summary>
    public const string XJogDownStatus = "M5389";

    /// <summary>
    /// 驱动器报警
    /// </summary>
    public const string XDriverAlarm = "M5393";

    /// <summary>
    /// 限位报警
    /// </summary>
    public const string XLimitAlarm = "M5391";

    /// <summary>
    /// 报警代码
    /// </summary>
    public const string XAlarmCode = "D5166";

    /// <summary>
    /// 回原点命令
    /// </summary>
    public const string XOrigin = "M1259";

    /// <summary>
    /// 原点状态
    /// </summary>
    public const string XOriginStatus = "M5385";

    #endregion

    #region 轴位置调整 - Z轴 (A手臂3z轴)

    /// <summary>
    /// 待机位
    /// </summary>
    public const string ZStandbyPosition = "D10918";

    /// <summary>
    /// 待机位移动
    /// </summary>
    public const string ZStandbyPositionMove = "M1304";

    /// <summary>
    /// 研磨A位
    /// </summary>
    public const string ZPolishAPosition = "D10920";

    /// <summary>
    /// 研磨A位移动
    /// </summary>
    public const string ZPolishAPositionMove = "M1305";

    /// <summary>
    /// 研磨B位
    /// </summary>
    public const string ZPolishBPosition = "D10922";

    /// <summary>
    /// 研磨B位移动
    /// </summary>
    public const string ZPolishBPositionMove = "M1306";

    /// <summary>
    /// 中转手臂位
    /// </summary>
    public const string ZTransferArmPosition = "D10924";

    /// <summary>
    /// 中转手臂位移动
    /// </summary>
    public const string ZTransferArmPositionMove = "M1307";

    /// <summary>
    /// JOG 速度
    /// </summary>
    public const string ZJogSpeed = "D1182";

    /// <summary>
    /// 自动速度
    /// </summary>
    public const string ZAutoSpeed = "D10900";

    /// <summary>
    /// 当前坐标
    /// </summary>
    public const string ZCurrentCoordinate = "D5180";

    /// <summary>
    /// 使能
    /// </summary>
    public const string ZEnable = "M1288";

    /// <summary>
    /// 使能状态
    /// </summary>
    public const string ZEnableStatus = "M5432";

    /// <summary>
    /// JOG 上移
    /// </summary>
    public const string ZJogUp = "M1289";

    /// <summary>
    /// JOG 上移激活状态
    /// </summary>
    public const string ZJogUpStatus = "M5436";

    /// <summary>
    /// JOG 中间激活状态
    /// </summary>
    public const string ZJogMiddleStatus = "M5438";

    /// <summary>
    /// JOG 下移
    /// </summary>
    public const string ZJogDown = "M1290";

    /// <summary>
    /// JOG 下移激活状态
    /// </summary>
    public const string ZJogDownStatus = "M5437";

    /// <summary>
    /// 驱动器报警
    /// </summary>
    public const string ZDriverAlarm = "M5441";

    /// <summary>
    /// 限位报警
    /// </summary>
    public const string ZLimitAlarm = "M5439";

    /// <summary>
    /// 报警代码
    /// </summary>
    public const string ZAlarmCode = "D5186";

    /// <summary>
    /// 回原点命令
    /// </summary>
    public const string ZOrigin = "M1291";

    /// <summary>
    /// 原点状态
    /// </summary>
    public const string ZOriginStatus = "M5433";

    /// <summary>
    /// 待机位到位
    /// </summary>
    public const string ZStandbyPositionArrived = "M5464";

    /// <summary>
    /// 研磨A位到位
    /// </summary>
    public const string ZPolishAPositionArrived = "M5465";

    /// <summary>
    /// 研磨B位到位
    /// </summary>
    public const string ZPolishBPositionArrived = "M5466";

    /// <summary>
    /// 中转手臂位到位
    /// </summary>
    public const string ZTransferArmPositionArrived = "M5467";
    /// <summary>
    /// 有晶圆时的速度
    /// </summary>
    public const string SpeedWhenHasWafer = "D10902";
    /// <summary>
    /// 减速度时间
    /// </summary>
    public const string DeaccelerationTime = "D10914";
    /// <summary>
    /// 加速度时间
    /// </summary>
    public const string AccelerationTime = "D10912";    
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
    public const string StationReset = "M2120";

    /// <summary>
    /// 工位操作 - 复位状态
    /// </summary>
    public const string StationResetStatus = "M6441";

    /// <summary>
    /// 工位操作 - 启动
    /// </summary>
    public const string StationStart = "M2123";

    /// <summary>
    /// 工位操作 - 运行状态
    /// </summary>
    public const string StationRunningStatus = "M6443";

    /// <summary>
    /// 工位操作 - 停止
    /// </summary>
    public const string StationStop = "M2125";

    /// <summary>
    /// 工位操作 - 停止状态
    /// </summary>
    public const string StationStoppedStatus = "M6445";

    /// <summary>
    /// 从PolishA取
    /// </summary>
    public const string PickFromPolishA = "M2136";

    /// <summary>
    /// 从PolishA取激活状态
    /// </summary>
    public const string PickFromPolishAActiveStatus = "M6456";

    /// <summary>
    /// 从PolishA取完成状态
    /// </summary>
    public const string PickFromPolishADoneStatus = "M6472";

    /// <summary>
    /// 从PolishA取2
    /// </summary>
    public const string PickFromPolishA2 = "M2137";

    /// <summary>
    /// 从PolishA取2激活状态
    /// </summary>
    public const string PickFromPolishA2ActiveStatus = "M6457";

    /// <summary>
    /// 从PolishA取2完成状态
    /// </summary>
    public const string PickFromPolishA2DoneStatus = "M6473";

    /// <summary>
    /// 放回中转手臂
    /// </summary>
    public const string PlaceToTransferArm = "M2138";

    /// <summary>
    /// 放回中转手臂激活状态
    /// </summary>
    public const string PlaceToTransferArmActiveStatus = "M6458";

    /// <summary>
    /// 放回中转手臂完成状态
    /// </summary>
    public const string PlaceToTransferArmDoneStatus = "M6474";

    /// <summary>
    /// 备用
    /// </summary>
    public const string Reserve = "M2139";

    /// <summary>
    /// 备用激活状态
    /// </summary>
    public const string ReserveActiveStatus = "M6459";

    /// <summary>
    /// 备用完成状态
    /// </summary>
    public const string ReserveDoneStatus = "M6475";

    /// <summary>
    /// 模块报警
    /// </summary>
    public const string ModuleAlarm = "M6449";

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
    public const string ManualMode = "M2129";

    /// <summary>
    /// 自动模式
    /// </summary>
    public const string AutoMode = "M2128";

    /// <summary>
    /// 手臂3光纤1
    /// </summary>
    public const string Arm3Fiber1 = "X1016";

    /// <summary>
    /// 手臂3光纤2
    /// </summary>
    public const string Arm3Fiber2 = "X1017";

    /// <summary>
    /// 手臂3光纤3
    /// </summary>
    public const string Arm3Fiber3 = "X1018";

    /// <summary>
    /// Foup/Slot
    /// </summary>
    public const string FoupSlot = "W1A30";

    /// <summary>
    /// Index
    /// </summary>
    public const string WaferIndex = "W1A31";
    /// <summary>
    /// 电源关闭
    /// </summary>
    public const string PowerOff = "M10402";

    #endregion
}
