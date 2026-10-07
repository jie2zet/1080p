namespace CMP.Constants.Plc.RobotControl;

using CMP.Services.Plc;

/// <summary>
/// FeedingRobot PLC 地址定义
/// </summary>
public static class FeedingRobotAddress
{
    #region 轴位置调整 - 位置设定 (D)

    /// <summary>
    /// LoadPort1# 位置
    /// </summary>
    public const string LoadPort1Position = "D10018";

    /// <summary>
    /// LoadPort2# 位置
    /// </summary>
    public const string LoadPort2Position = "D10020";

    /// <summary>
    /// LoadPort3# 位置
    /// </summary>
    public const string LoadPort3Position = "D10022";

    /// <summary>
    /// LoadPort4# 位置
    /// </summary>
    public const string LoadPort4Position = "D10024";

    /// <summary>
    /// LoadPort5# 位置
    /// </summary>
    public const string LoadPort5Position = "D10026";

    /// <summary>
    /// 中间模块位置
    /// </summary>
    public const string MiddleModulePosition = "D10028";

    /// <summary>
    /// DryA 位置
    /// </summary>
    public const string DryAPosition = "D10030";

    /// <summary>
    /// DryB 位置
    /// </summary>
    public const string DryBPosition = "D10032";

    #endregion

    #region 轴位置调整 - 移动命令 (M)

    /// <summary>
    /// LoadPort1# 移动
    /// </summary>
    public const string LoadPort1Move = "M1016";

    /// <summary>
    /// LoadPort2# 移动
    /// </summary>
    public const string LoadPort2Move = "M1017";

    /// <summary>
    /// LoadPort3# 移动
    /// </summary>
    public const string LoadPort3Move = "M1018";

    /// <summary>
    /// LoadPort4# 移动
    /// </summary>
    public const string LoadPort4Move = "M1019";

    /// <summary>
    /// LoadPort5# 移动
    /// </summary>
    public const string LoadPort5Move = "M1020";

    /// <summary>
    /// 中间模块移动
    /// </summary>
    public const string MiddleModuleMove = "M1021";

    /// <summary>
    /// DryA 移动
    /// </summary>
    public const string DryAMove = "M1022";

    /// <summary>
    /// DryB 移动
    /// </summary>
    public const string DryBMove = "M1023";

    #endregion

    #region 轴位置调整 - 速度设定 (D)

    /// <summary>
    /// JOG 速度
    /// </summary>
    public const string JogSpeed = "D1002";

    /// <summary>
    /// 自动速度
    /// </summary>
    public const string AutoSpeed = "D10000";

    #endregion

    #region 轴位置调整 - 状态 (D/M)

    /// <summary>
    /// 当前坐标
    /// </summary>
    public const string CurrentCoordinate = "D5000";

    /// <summary>
    /// 使能
    /// </summary>
    public const string Enable = "M1000";

    /// <summary>
    /// 使能状态
    /// </summary>
    public const string EnableStatus = "M5000";

    #endregion

    #region 轴位置调整 - JOG控制 (M)

    /// <summary>
    /// JOG 上移
    /// </summary>
    public const string JogUp = "M1001";

    /// <summary>
    /// JOG 上移激活状态
    /// </summary>
    public const string JogUpStatus = "M5004";

    /// <summary>
    /// JOG 中间激活状态
    /// </summary>
    public const string JogMiddleStatus = "M5006";

    /// <summary>
    /// JOG 下移
    /// </summary>
    public const string JogDown = "M1002";

    /// <summary>
    /// JOG 下移激活状态
    /// </summary>
    public const string JogDownStatus = "M5005";

    #endregion

    #region 轴位置调整 - 报警状态 (M)

    /// <summary>
    /// 驱动器报警
    /// </summary>
    public const string DriverAlarm = "M5009";

    /// <summary>
    /// 限位报警
    /// </summary>
    public const string LimitAlarm = "M5007";

    /// <summary>
    /// 报警代码
    /// </summary>
    public const string AlarmCode = "D5006";

    #endregion

    #region 轴位置调整 - 原点 (M)

    /// <summary>
    /// 回原点命令
    /// </summary>
    public const string Origin = "M1003";

    /// <summary>
    /// 原点状态
    /// </summary>
    public const string OriginStatus = "M5001";

    #endregion

    #region 机器人控制 - 程序控制 (D/M)

    /// <summary>
    /// 程序号
    /// </summary>
    public const string ProgramNumber = "D1400";

    /// <summary>
    /// 启动
    /// </summary>
    public const string Start = "M1640";

    /// <summary>
    /// 运行中状态
    /// </summary>
    public const string RunningStatus = "M5960";

    /// <summary>
    /// 暂停
    /// </summary>
    public const string Pause = "M1641";

    /// <summary>
    /// 暂停状态
    /// </summary>
    public const string PausedStatus = "M5961";

    /// <summary>
    /// 停止
    /// </summary>
    public const string Stop = "M1642";

    /// <summary>
    /// 停止状态
    /// </summary>
    public const string StoppedStatus = "M5962";

    /// <summary>
    /// 复位
    /// </summary>
    public const string Reset = "M1643";

    /// <summary>
    /// 复位状态
    /// </summary>
    public const string ResetStatus = "M5963";

    #endregion

    #region 机器人控制 - 工位操作 (D/M)

    /// <summary>
    /// 工位号
    /// </summary>
    public const string WorkAreaNumber = "D5402";

    /// <summary>
    /// 模块位置命令
    /// </summary>
    public const string ModulePosition = "D1402";

    /// <summary>
    /// DryA 命令
    /// </summary>
    public const string DryACommand = "D1402";

    /// <summary>
    /// DryB 命令
    /// </summary>
    public const string DryBCommand = "D1402";

    /// <summary>
    /// 工位取片
    /// </summary>
    public const string WorkAreaPickup = "M1654";

    /// <summary>
    /// 工位取片状态
    /// </summary>
    public const string WorkAreaPickupStatus = "M5973";

    /// <summary>
    /// 工位放片
    /// </summary>
    public const string WorkAreaPlace = "M1655";

    /// <summary>
    /// 工位放片状态
    /// </summary>
    public const string WorkAreaPlaceStatus = "M5974";

    /// <summary>
    /// 等待确认
    /// </summary>
    public const string WaitConfirm = "M1658";

    /// <summary>
    /// 等待确认状态
    /// </summary>
    public const string WaitConfirmStatus = "M5978";

    #endregion

    #region 机器人控制 - 模式选择 (M)

    /// <summary>
    /// 自动模式
    /// </summary>
    public const string AutoMode = "M1645";

    /// <summary>
    /// 自动模式状态
    /// </summary>
    public static readonly DBitAddress AutoModeStatus = new("D50000", 0x05);

    /// <summary>
    /// 使用上臂
    /// </summary>
    public const string UseUpperArm = "M1646";

    /// <summary>
    /// 使用上臂状态
    /// </summary>
    public static readonly DBitAddress UseUpperArmStatus = new("D55001", 0x0);

    /// <summary>
    /// 使用下臂
    /// </summary>
    public const string UseLowerArm = "M1647";

    /// <summary>
    /// 使用下臂状态
    /// </summary>
    public static readonly DBitAddress UseLowerArmStatus = new("D55001", 0x01);

    /// <summary>
    /// 确认开关
    /// </summary>
    public const string ConfirmSwitch = "M1659";

    /// <summary>
    /// 确认切换状态
    /// </summary>
    public static readonly DBitAddress ConfirmSwitchStatus = new("D50003", 0x0a);

    #endregion

    #region 机器人控制 - 料盒操作 (D/M)

    /// <summary>
    /// 料盒号
    /// </summary>
    public const string BoxNumber = "D1800";

    /// <summary>
    /// 层数
    /// </summary>
    public const string LayerCount = "D1802";

    /// <summary>
    /// 料盒取片
    /// </summary>
    public const string BoxPickup = "M1648";

    /// <summary>
    /// 料盒取片状态
    /// </summary>
    public const string BoxPickupStatus = "M5968";

    /// <summary>
    /// 料盒放片
    /// </summary>
    public const string BoxPlace = "M1649";

    /// <summary>
    /// 料盒放片状态
    /// </summary>
    public const string BoxPlaceStatus = "M5969";

    /// <summary>
    /// 翻转到0度
    /// </summary>
    public const string FlipTo0 = "M1656";

    /// <summary>
    /// 翻转到0度状态
    /// </summary>
    public const string FlipTo0Status = "M5976";

    /// <summary>
    /// 翻转到180度
    /// </summary>
    public const string FlipTo180 = "M1657";

    /// <summary>
    /// 翻转到180度状态
    /// </summary>
    public const string FlipTo180Status = "M5977";

    #endregion

    #region 机器人控制 - 传感器状态 (M)

    /// <summary>
    /// 上臂传感器
    /// </summary>
    public static readonly DBitAddress UpperArmSensor = new("D50001", 0x0);

    /// <summary>
    /// 下臂传感器
    /// </summary>
    public static readonly DBitAddress LowerArmSensor = new("D50001", 0x1);

    /// <summary>
    /// 命令接受
    /// </summary>
    public static readonly DBitAddress CommandAccept = new("D50001", 0x04);

    /// <summary>
    /// 机器人安全位
    /// </summary>
    public static readonly DBitAddress RobotSafePosition = new("D50004", 0x0);

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
    public const string StationReset = "M1832";

    /// <summary>
    /// 工位操作 - 复位状态
    /// </summary>
    public const string StationResetStatus = "M6153";

    /// <summary>
    /// 工位操作 - 启动
    /// </summary>
    public const string StationStart = "M1835";

    /// <summary>
    /// 工位操作 - 运行状态
    /// </summary>
    public const string StationRunningStatus = "M6155";

    /// <summary>
    /// 工位操作 - 停止
    /// </summary>
    public const string StationStop = "M1837";

    /// <summary>
    /// 工位操作 - 停止状态
    /// </summary>
    public const string StationStoppedStatus = "M6157";

    /// <summary>
    /// 从料盒取片
    /// </summary>
    public const string PickFromBox = "M1848";

    /// <summary>
    /// 从料盒取片激活状态
    /// </summary>
    public const string PickFromBoxActiveStatus = "M6168";

    /// <summary>
    /// 从料盒取片完成状态
    /// </summary>
    public const string PickFromBoxDoneStatus = "M6184";

    /// <summary>
    /// 放片到模组
    /// </summary>
    public const string PlaceToModule = "M1849";

    /// <summary>
    /// 放片到模组激活状态
    /// </summary>
    public const string PlaceToModuleActiveStatus = "M6169";

    /// <summary>
    /// 放片到模组完成状态
    /// </summary>
    public const string PlaceToModuleDoneStatus = "M6185";

    /// <summary>
    /// 从DryA取片
    /// </summary>
    public const string PickFromDryA = "M1850";

    /// <summary>
    /// 从DryA取片激活状态
    /// </summary>
    public const string PickFromDryAActiveStatus = "M6170";

    /// <summary>
    /// 从DryA取片完成状态
    /// </summary>
    public const string PickFromDryADoneStatus = "M6186";

    /// <summary>
    /// 从DryB取片
    /// </summary>
    public const string PickFromDryB = "M1851";

    /// <summary>
    /// 从DryB取片激活状态
    /// </summary>
    public const string PickFromDryBActiveStatus = "M6171";

    /// <summary>
    /// 从DryB取片完成状态
    /// </summary>
    public const string PickFromDryBDoneStatus = "M6187";

    /// <summary>
    /// 放片到料盒
    /// </summary>
    public const string PlaceToBox = "M1852";

    /// <summary>
    /// 放片到料盒激活状态
    /// </summary>
    public const string PlaceToBoxActiveStatus = "M6172";

    /// <summary>
    /// 放片到料盒完成状态
    /// </summary>
    public const string PlaceToBoxDoneStatus = "M6188";

    /// <summary>
    /// 模块报警
    /// </summary>
    public const string ModuleAlarm = "M6161";

    /// <summary>
    /// 全局报警
    /// </summary>
    public const string GlobalAlarm = "M89";

    /// <summary>
    /// 报警复位
    /// </summary>
    public const string ResetAlarm = "B0B";

    /// <summary>
    /// 手动模式
    /// </summary>
    public const string ManualMode = "M1841";

    /// <summary>
    /// 源料盒号
    /// </summary>
    public const string SourceBoxNo = "D1830";

    /// <summary>
    /// 源层数
    /// </summary>
    public const string SourceLayer = "D1831";

    /// <summary>
    /// 目标料盒号
    /// </summary>
    public const string TargetBoxNo = "D1834";

    /// <summary>
    /// 目标层数
    /// </summary>
    public const string TargetLayer = "D1835";

    /// <summary>
    /// 传送命令
    /// </summary>
    public const string Transfer = "M185";

    /// <summary>
    /// 停止入料
    /// </summary>
    public const string StopLoad = "M170";

    /// <summary>
    /// 停止入料状态
    /// </summary>
    public const string StopLoadStatus = "M170";

    /// <summary>
    /// 恢复入料
    /// </summary>
    public const string ResumeLoad = "M171";

    /// <summary>
    /// 恢复入料状态
    /// </summary>
    public const string ResumeLoadStatus = "M171";

    /// <summary>
    /// A侧正常
    /// </summary>
    public const string SideAHealthy = "M183";

    /// <summary>
    /// B侧正常
    /// </summary>
    public const string SideBHealthy = "M184";

    #endregion
}