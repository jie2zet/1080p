namespace CMP.Constants.Plc.RobotControl;

using CMP.Services.Plc;

/// <summary>
/// PolishASideTransfer PLC 地址定义
/// </summary>
public static class PolishASideTransferAddress
{
    #region 轴位置调整 - 位置设定 (D)

    /// <summary>
    /// 待机位
    /// </summary>
    public const string StandbyPosition = "D10318";

    /// <summary>
    /// Robot上手臂
    /// </summary>
    public const string RobotUpperArmPosition = "D10320";

    /// <summary>
    /// LTP取料位
    /// </summary>
    public const string LtpPickPosition = "D10322";

    /// <summary>
    /// Robot下手臂
    /// </summary>
    public const string RobotLowerArmPosition = "D10324";

    /// <summary>
    /// LTP放料位
    /// </summary>
    public const string LtpPlacePosition = "D10326";

    #endregion

    #region 轴位置调整 - 移动命令 (M)

    /// <summary>
    /// 待机位移动
    /// </summary>
    public const string StandbyPositionMove = "M1112";

    /// <summary>
    /// Robot上手臂移动
    /// </summary>
    public const string RobotUpperArmPositionMove = "M1113";

    /// <summary>
    /// LTP取料位移动
    /// </summary>
    public const string LtpPickPositionMove = "M1114";

    /// <summary>
    /// Robot下手臂移动
    /// </summary>
    public const string RobotLowerArmPositionMove = "M1115";

    /// <summary>
    /// LTP放料位移动
    /// </summary>
    public const string LtpPlacePositionMove = "M1116";

    #endregion

    #region 轴位置调整 - 速度设定 (D)

    /// <summary>
    /// JOG 速度
    /// </summary>
    public const string JogSpeed = "D1062";

    /// <summary>
    /// 自动速度
    /// </summary>
    public const string AutoSpeed = "D10300";

    #endregion

    #region 轴位置调整 - 状态 (D/M)

    /// <summary>
    /// 当前坐标
    /// </summary>
    public const string CurrentCoordinate = "D5060";

    /// <summary>
    /// 使能
    /// </summary>
    public const string Enable = "M1096";

    /// <summary>
    /// 使能状态
    /// </summary>
    public const string EnableStatus = "M5144";

    #endregion

    #region 轴位置调整 - JOG控制 (M)

    /// <summary>
    /// JOG 上移
    /// </summary>
    public const string JogUp = "M1097";

    /// <summary>
    /// JOG 上移激活状态
    /// </summary>
    public const string JogUpStatus = "M5148";

    /// <summary>
    /// JOG 中间激活状态
    /// </summary>
    public const string JogMiddleStatus = "M5150";

    /// <summary>
    /// JOG 下移
    /// </summary>
    public const string JogDown = "M1098";

    /// <summary>
    /// JOG 下移激活状态
    /// </summary>
    public const string JogDownStatus = "M5149";

    #endregion

    #region 轴位置调整 - 报警状态 (M)

    /// <summary>
    /// 驱动器报警
    /// </summary>
    public const string DriverAlarm = "M5153";

    /// <summary>
    /// 限位报警
    /// </summary>
    public const string LimitAlarm = "M5151";

    /// <summary>
    /// 报警代码
    /// </summary>
    public const string AlarmCode = "D5066";

    #endregion

    #region 轴位置调整 - 原点 (M)

    /// <summary>
    /// 回原点命令
    /// </summary>
    public const string Origin = "M1099";

    /// <summary>
    /// 原点状态
    /// </summary>
    public const string OriginStatus = "M5145";

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
    public const string StationReset = "M1976";

    /// <summary>
    /// 工位操作 - 复位状态
    /// </summary>
    public const string StationResetStatus = "M6297";

    /// <summary>
    /// 工位操作 - 启动
    /// </summary>
    public const string StationStart = "M1979";

    /// <summary>
    /// 工位操作 - 运行状态
    /// </summary>
    public const string StationRunningStatus = "M6299";

    /// <summary>
    /// 工位操作 - 停止
    /// </summary>
    public const string StationStop = "M1981";

    /// <summary>
    /// 工位操作 - 停止状态
    /// </summary>
    public const string StationStoppedStatus = "M6301";

    /// <summary>
    /// 模块报警
    /// </summary>
    public const string ModuleAlarm = "M6305";

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
    public const string ManualMode = "M1985";

    /// <summary>
    /// 自动模式
    /// </summary>
    public const string AutoMode = "M1984";

    #endregion

    #region 工位操作 - 步骤 (M)

    /// <summary>
    /// 从robot2取
    /// </summary>
    public const string PickFromRobot2 = "M1992";

    /// <summary>
    /// 从robot2取 - 激活状态
    /// </summary>
    public const string PickFromRobot2ActiveStatus = "M6312";

    /// <summary>
    /// 从robot2取 - 完成状态
    /// </summary>
    public const string PickFromRobot2DoneStatus = "M6328";

    /// <summary>
    /// 放料到手臂1
    /// </summary>
    public const string PlaceToArm1 = "M1993";

    /// <summary>
    /// 放料到手臂1 - 激活状态
    /// </summary>
    public const string PlaceToArm1ActiveStatus = "M6313";

    /// <summary>
    /// 放料到手臂1 - 完成状态
    /// </summary>
    public const string PlaceToArm1DoneStatus = "M6329";

    /// <summary>
    /// 放料到手臂2
    /// </summary>
    public const string PlaceToArm2 = "M1994";

    /// <summary>
    /// 放料到手臂2 - 激活状态
    /// </summary>
    public const string PlaceToArm2ActiveStatus = "M6314";

    /// <summary>
    /// 放料到手臂2 - 完成状态
    /// </summary>
    public const string PlaceToArm2DoneStatus = "M6330";

    /// <summary>
    /// 从手臂3取
    /// </summary>
    public const string PickFromArm3 = "M1995";

    /// <summary>
    /// 从手臂3取 - 激活状态
    /// </summary>
    public const string PickFromArm3ActiveStatus = "M6315";

    /// <summary>
    /// 从手臂3取 - 完成状态
    /// </summary>
    public const string PickFromArm3DoneStatus = "M6331";

    /// <summary>
    /// 放料到机器人
    /// </summary>
    public const string PlaceToRobot = "M1996";

    /// <summary>
    /// 放料到机器人 - 激活状态
    /// </summary>
    public const string PlaceToRobotActiveStatus = "M6316";

    /// <summary>
    /// 放料到机器人 - 完成状态
    /// </summary>
    public const string PlaceToRobotDoneStatus = "M6332";

    /// <summary>
    /// 从手臂2取
    /// </summary>
    public const string PickFromArm2 = "M1997";

    /// <summary>
    /// 从手臂2取 - 激活状态
    /// </summary>
    public const string PickFromArm2ActiveStatus = "M6317";

    /// <summary>
    /// 从手臂2取 - 完成状态
    /// </summary>
    public const string PickFromArm2DoneStatus = "M6333";

    #endregion

    #region 工位操作 - 右侧控制 (Y/X/M)

    /// <summary>
    /// 夹子夹紧 读取
    /// </summary>
    public const string ClampCloseWrite = "M2008";

    /// <summary>
    /// 夹子夹紧 写入
    /// </summary>
    public const string ClampCloseRead = "Y104A";

    /// <summary>
    /// 夹子夹紧 状态
    /// </summary>
    public const string ClampCloseStatus = "X1019";

    /// <summary>
    /// 夹子松开 读取
    /// </summary>
    public const string ClampOpenRead = "Y104B";

    /// <summary>
    /// 夹子松开 写入
    /// </summary>
    public const string ClampOpenWrite = "M2009";

    /// <summary>
    /// 夹子松开 状态
    /// </summary>
    public const string ClampOpenStatus = "X101A";

    /// <summary>
    /// 晶圆检测
    /// </summary>
    public const string WaferDetection = "X101B";

    /// <summary>
    /// 研磨A门开 读取
    /// </summary>
    public const string PolishADoorOpen = "Y104E";

    /// <summary>
    /// 研磨A门开 写入
    /// </summary>
    public const string PolishADoorWrite = "M2010";

    /// <summary>
    /// 研磨A门开 状态
    /// </summary>
    public const string PolishADoorOpenStatus = "X101E";

    /// <summary>
    /// 研磨A门关 读取
    /// </summary>
    public const string PolishADoorCloseRead = "Y104F";
    
    /// <summary>
    /// 研磨A门关 
    /// </summary>
    public const string PolishADoorCloseWrite = "M2011";

    /// <summary>
    /// 研磨A门关 状态
    /// </summary>
    public const string PolishADoorCloseStatus = "X101F";

    #endregion
}
