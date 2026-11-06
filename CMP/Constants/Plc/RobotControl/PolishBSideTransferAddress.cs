namespace CMP.Constants.Plc.RobotControl;

/// <summary>
/// PolishBSideTransfer PLC 地址定义
/// </summary>
public static class PolishBSideTransferAddress
{
    #region 轴位置调整 - 位置设定 (ZR)

    /// <summary>
    /// 待机位
    /// </summary>
    public const string StandbyPosition = "ZR11018";

    /// <summary>
    /// Robot上手臂
    /// </summary>
    public const string RobotUpperArmPosition = "ZR11020";

    /// <summary>
    /// LTP取料位
    /// </summary>
    public const string LtpPickPosition = "ZR11022";

    /// <summary>
    /// Robot下手臂
    /// </summary>
    public const string RobotLowerArmPosition = "ZR11024";

    /// <summary>
    /// LTP放料位
    /// </summary>
    public const string LtpPlacePosition = "ZR11026";

    #endregion

    #region 轴位置调整 - 移动命令 (M)

    /// <summary>
    /// 待机位移动
    /// </summary>
    public const string StandbyPositionMove = "M1336";

    /// <summary>
    /// Robot上手臂移动
    /// </summary>
    public const string RobotUpperArmPositionMove = "M1337";

    /// <summary>
    /// LTP取料位移动
    /// </summary>
    public const string LtpPickPositionMove = "M1338";

    /// <summary>
    /// Robot下手臂移动
    /// </summary>
    public const string RobotLowerArmPositionMove = "M1339";

    /// <summary>
    /// LTP放料位移动
    /// </summary>
    public const string LtpPlacePositionMove = "M1340";

    #endregion

    #region 轴位置调整 - 速度设定 (D/ZR)

    /// <summary>
    /// JOG 速度
    /// </summary>
    public const string JogSpeed = "D1202";

    /// <summary>
    /// 自动速度
    /// </summary>
    public const string AutoSpeed = "ZR11000";

    #endregion

    #region 轴位置调整 - 状态 (D/M)

    /// <summary>
    /// 当前坐标
    /// </summary>
    public const string CurrentCoordinate = "D5200";

    /// <summary>
    /// 使能
    /// </summary>
    public const string Enable = "M1320";

    /// <summary>
    /// 使能状态
    /// </summary>
    public const string EnableStatus = "M5480";

    #endregion

    #region 轴位置调整 - JOG控制 (M)

    /// <summary>
    /// JOG 上移
    /// </summary>
    public const string JogUp = "M1321";

    /// <summary>
    /// JOG 上移激活状态
    /// </summary>
    public const string JogUpStatus = "M5484";

    /// <summary>
    /// JOG 中间激活状态
    /// </summary>
    public const string JogMiddleStatus = "M5486";

    /// <summary>
    /// JOG 下移
    /// </summary>
    public const string JogDown = "M1322";

    /// <summary>
    /// JOG 下移激活状态
    /// </summary>
    public const string JogDownStatus = "M5485";

    #endregion

    #region 轴位置调整 - 报警状态 (M)

    /// <summary>
    /// 驱动器报警
    /// </summary>
    public const string DriverAlarm = "M5489";

    /// <summary>
    /// 限位报警
    /// </summary>
    public const string LimitAlarm = "M5487";

    /// <summary>
    /// 报警代码
    /// </summary>
    public const string AlarmCode = "D5206";

    #endregion

    #region 轴位置调整 - 原点 (M)

    /// <summary>
    /// 回原点命令
    /// </summary>
    public const string Origin = "M1323";

    /// <summary>
    /// 原点状态
    /// </summary>
    public const string OriginStatus = "M5481";

    #endregion

    #region 底部状态栏 (M)

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
    public const string StationReset = "M2168";

    /// <summary>
    /// 工位操作 - 复位状态
    /// </summary>
    public const string StationResetStatus = "M6489";

    /// <summary>
    /// 工位操作 - 启动
    /// </summary>
    public const string StationStart = "M2171";

    /// <summary>
    /// 工位操作 - 运行状态
    /// </summary>
    public const string StationRunningStatus = "M6491";

    /// <summary>
    /// 工位操作 - 停止
    /// </summary>
    public const string StationStop = "M2173";

    /// <summary>
    /// 工位操作 - 停止状态
    /// </summary>
    public const string StationStoppedStatus = "M6493";

    /// <summary>
    /// 模块报警
    /// </summary>
    public const string ModuleAlarm = "M6497";

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
    public const string ManualMode = "M2177";

    /// <summary>
    /// 自动模式
    /// </summary>
    public const string AutoMode = "M2176";

    #endregion

    #region 工位操作 - 步骤 (M)

    /// <summary>
    /// 从robot2取
    /// </summary>
    public const string PickFromRobot2 = "M2184";

    /// <summary>
    /// 从robot2取 - 激活状态
    /// </summary>
    public const string PickFromRobot2ActiveStatus = "M6504";

    /// <summary>
    /// 从robot2取 - 完成状态
    /// </summary>
    public const string PickFromRobot2DoneStatus = "M6520";

    /// <summary>
    /// 放料到手臂1
    /// </summary>
    public const string PlaceToArm1 = "M2185";

    /// <summary>
    /// 放料到手臂1 - 激活状态
    /// </summary>
    public const string PlaceToArm1ActiveStatus = "M6505";

    /// <summary>
    /// 放料到手臂1 - 完成状态
    /// </summary>
    public const string PlaceToArm1DoneStatus = "M6521";

    /// <summary>
    /// 放料到手臂2
    /// </summary>
    public const string PlaceToArm2 = "M2186";

    /// <summary>
    /// 放料到手臂2 - 激活状态
    /// </summary>
    public const string PlaceToArm2ActiveStatus = "M6506";

    /// <summary>
    /// 放料到手臂2 - 完成状态
    /// </summary>
    public const string PlaceToArm2DoneStatus = "M6522";

    /// <summary>
    /// 从手臂3取
    /// </summary>
    public const string PickFromArm3 = "M2187";

    /// <summary>
    /// 从手臂3取 - 激活状态
    /// </summary>
    public const string PickFromArm3ActiveStatus = "M6507";

    /// <summary>
    /// 从手臂3取 - 完成状态
    /// </summary>
    public const string PickFromArm3DoneStatus = "M6523";

    /// <summary>
    /// 放料到机器人
    /// </summary>
    public const string PlaceToRobot = "M2188";

    /// <summary>
    /// 放料到机器人 - 激活状态
    /// </summary>
    public const string PlaceToRobotActiveStatus = "M6508";

    /// <summary>
    /// 放料到机器人 - 完成状态
    /// </summary>
    public const string PlaceToRobotDoneStatus = "M6524";

    /// <summary>
    /// 从手臂2取
    /// </summary>
    public const string PickFromArm2 = "M2189";

    /// <summary>
    /// 从手臂2取 - 激活状态
    /// </summary>
    public const string PickFromArm2ActiveStatus = "M6509";

    /// <summary>
    /// 从手臂2取 - 完成状态
    /// </summary>
    public const string PickFromArm2DoneStatus = "M6525";

    #endregion

    #region 工位操作 - 右侧控制 (Y/X/M)

    /// <summary>
    /// 夹子夹紧 读取
    /// </summary>
    public const string ClampCloseRead = "Y1048";

    /// <summary>
    /// 夹子夹紧 写入
    /// </summary>
    public const string ClampCloseWrite = "M2200";

    /// <summary>
    /// 夹子夹紧 状态
    /// </summary>
    public const string ClampCloseStatus = "X1029";

    /// <summary>
    /// 夹子松开 读取
    /// </summary>
    public const string ClampOpenRead = "Y1049";

    /// <summary>
    /// 夹子松开 写入
    /// </summary>
    public const string ClampOpenWrite = "M2201";

    /// <summary>
    /// 夹子松开 状态
    /// </summary>
    public const string ClampOpenStatus = "X102A";

    /// <summary>
    /// 晶圆检测
    /// </summary>
    public const string WaferDetection = "X102B";

    /// <summary>
    /// 研磨A门开 (读取/状态)
    /// </summary>
    public const string PolishADoorOpenRead = "Y1046";

    /// <summary>
    /// 研磨A门开 写入
    /// </summary>
    public const string PolishADoorOpenWrite = "M2202";

    /// <summary>
    /// 研磨A门开 状态
    /// </summary>
    public const string PolishADoorOpenStatus = "X101C";

    /// <summary>
    /// 研磨A门关 读取
    /// </summary>
    public const string PolishADoorCloseRead = "Y1047";

    /// <summary>
    /// 研磨A门关 写入
    /// </summary>
    public const string PolishADoorCloseWrite = "M2203";

    /// <summary>
    /// 研磨A门关 状态
    /// </summary>
    public const string PolishADoorCloseStatus = "X101D";

    #endregion
}
