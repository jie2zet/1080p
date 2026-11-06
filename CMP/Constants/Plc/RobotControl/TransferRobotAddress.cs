using CMP.Services.Plc;

namespace CMP.Constants.Plc.RobotControl;

/// <summary>
/// TransferRobot (中转机器人) PLC 地址定义
/// </summary>
public static class TransferRobotAddress
{
    #region 轴位置调整 - 位置设定 (ZR)

    /// <summary>
    /// 模组取料位
    /// </summary>
    public const string ModulePickPosition = "ZR10218";

    /// <summary>
    /// A侧中转位
    /// </summary>
    public const string TransferAPosition = "ZR10220";

    /// <summary>
    /// B侧中转位
    /// </summary>
    public const string TransferBPosition = "ZR10222";

    /// <summary>
    /// RollerA1位置
    /// </summary>
    public const string RollerA1Position = "ZR10224";

    /// <summary>
    /// RollerA2位置
    /// </summary>
    public const string RollerA2Position = "ZR10226";

    /// <summary>
    /// RollerB1位置
    /// </summary>
    public const string RollerB1Position = "ZR10228";

    /// <summary>
    /// RollerB2位置
    /// </summary>
    public const string RollerB2Position = "ZR10230";

    #endregion

    #region 轴位置调整 - 移动命令 (M)

    public const string ModulePickMove = "M1080";
    public const string TransferAMove = "M1081";
    public const string TransferBMove = "M1082";
    public const string RollerA1Move = "M1083";
    public const string RollerA2Move = "M1084";
    public const string RollerB1Move = "M1085";
    public const string RollerB2Move = "M1086";

    #endregion

    #region 轴位置调整 - 移动状态 (M) - 用于 ToggleButton IsChecked

    public const string ModulePickMoveStatus = "M5128";
    public const string TransferAMoveStatus = "M5129"; 
    public const string TransferBMoveStatus = "M5130"; 
    public const string RollerA1MoveStatus = "M5131";
    public const string RollerA2MoveStatus = "M5132";
    public const string RollerB1MoveStatus = "M5133";
    public const string RollerB2MoveStatus = "M5134";

    #endregion

    #region 轴位置调整 - 速度和坐标 (D/ZR)

    /// <summary>
    /// JOG 速度
    /// </summary>
    public const string JogSpeed = "D1042"; 

    /// <summary>
    /// 自动速度
    /// </summary>
    public const string AutoSpeed = "ZR10200";

    /// <summary>
    /// 当前坐标
    /// </summary>
    public const string CurrentCoordinate = "D5040";

    #endregion

    #region 轴位置调整 - 状态与控制 (M)

    /// <summary>
    /// 使能 (M-1064 Enable, M-5096 Indicator)
    /// </summary>
    public const string Enable = "M1064";
    public const string EnableStatus = "M5096";

    /// <summary>
    /// JOG + (M-1065, M-5100 Indicator)
    /// </summary>
    public const string JogUp = "M1065";
    public const string JogUpStatus = "M5100";

    /// <summary>
    /// Jog Middle (M-5102 Indicator)
    /// </summary>
    public const string JogMiddleStatus = "M5102";
    /// <summary>
    /// JOG - (M-1066, M-5101 Indicator)
    /// </summary>
    public const string JogDown = "M1066";
    public const string JogDownStatus = "M5101";

    /// <summary>
    /// 原点 (M-1067, M-5097 Indicator)
    /// </summary>
    public const string Origin = "M1067";
    public const string OriginStatus = "M5097";

    /// <summary>
    /// 驱动器报警
    /// </summary>
    public const string DriverAlarm = "M5105";

    /// <summary>
    /// 限位报警
    /// </summary>
    public const string LimitAlarm = "M5103";

    /// <summary>
    /// 报警复位 (B-0B)
    /// </summary>
    public const string AxisAlarmReset = "B0B";

    /// <summary>
    /// 报警代码
    /// </summary>
    public const string AlarmCode = "D5046";

    #endregion

    #region 机器人控制 - 状态与命令

    /// <summary>
    /// 程序编号
    /// </summary>
    public const string ProgramNo = "D1402";

    /// <summary>
    /// 工作区编号
    /// </summary>
    public const string WorkAreaNo = "D5422";

    // 机器人控制按钮
    public const string RobotStart = "M1672";
    public const string RobotStartStatus = "M5992";
    public const string RobotPause = "M1673";
    public const string RobotPauseStatus = "M5993";
    public const string RobotStop = "M1674";
    public const string RobotStopStatus = "M5994";
    public const string RobotReset = "M1675";
    public const string RobotResetStatus = "M5995";

    // 操作按钮
    public const string WorkAreaPick = "M1686";
    public const string WorkAreaPickStatus = "M6005";

    public const string WorkAreaPlace = "M1687";
    public const string WorkAreaPlaceStatus = "M6006";

    public const string WaitConfirm = "M1090";
    public const string WaitConfirmStatus = "M6010";

    public const string AutoMode = "M1677";
    public static DBitAddress AutoModeStatus = new("D50200", 5);

    public const string UseUpperArm = "M1678";
    public static DBitAddress UseUpperArmStatus = new("D55201", 0);
    public const string UseLowerArm = "M1679";
    public static DBitAddress UseLowerArmStatus = new("D55201", 1);

    public const string ConfirmSwitch = "M1691";
    public static DBitAddress ConfirmSwitchStatus = new("D50203", 0x0A);

    public const string TurnTo0 = "M1688";
    public const string TurnTo0Status = "M6008";

    public const string TurnTo180 = "M1689";
    public const string TurnTo180Status = "M6009";

    // 传感器状态
    public static DBitAddress UpperArmSensor = new("D50201", 0);
    public static DBitAddress LowerArmSensor = new("D50201", 1);
    public static DBitAddress CommandReceived = new("D50201", 4);

    // 机器人安全位置
    public static DBitAddress RobotSafePosition = new("D50204", 0);

    #endregion

    #region 底部状态栏

    public const string EmergencyStop = "M99";
    public const string GlobalAlarmReset = "B0B";
    public const string Save = "M98";

    #endregion

    #region 工位操作 (Station Operation)

    // 顶部按钮
    public const string StationReset = "M1928";
    public const string StationResetStatus = "M6249";

    public const string StationStart = "M1931";
    public const string StationStartStatus = "M6251";

    public const string StationStop = "M1933";
    public const string StationStopStatus = "M6253";

    public const string ModuleAlarm = "M6257";
    public const string GlobalAlarm = "M89";

    public const string ManualMode = "M1937";
    public const string AutoModeStation = "M1936";

    // 动作按钮 (Write/Read/Done)
    public const string StationModulePick = "M1944";
    public const string StationModulePickActive = "M6264";
    public const string StationModulePickDone = "M6280";

    public const string StationTransferAPlace = "M1947";
    public const string StationTransferAPlaceActive = "M6267";
    public const string StationTransferAPlaceDone = "M6283";

    public const string StationTransferAPick = "M1948";
    public const string StationTransferAPickActive = "M6268";
    public const string StationTransferAPickDone = "M6284";

    public const string StationTransferBPlace = "M1949";
    public const string StationTransferBPlaceActive = "M6269";
    public const string StationTransferBPlaceDone = "M6285";

    public const string StationTransferBPick = "M1950";
    public const string StationTransferBPickActive = "M6270";
    public const string StationTransferBPickDone = "M6286";

    public const string StationRollerA1Place = "M1945";
    public const string StationRollerA1PlaceActive = "M6265";
    public const string StationRollerA1PlaceDone = "M6281";

    public const string StationRollerA2Place = "M1951";
    public const string StationRollerA2PlaceActive = "M6271";
    public const string StationRollerA2PlaceDone = "M6287";

    public const string StationRollerB1Place = "M1952";
    public const string StationRollerB1PlaceActive = "M6272";
    public const string StationRollerB1PlaceDone = "M6288";

    public const string StationRollerB2Place = "M1953";
    public const string StationRollerB2PlaceActive = "M6273";
    public const string StationRollerB2PlaceDone = "M6289";

    #endregion
}
