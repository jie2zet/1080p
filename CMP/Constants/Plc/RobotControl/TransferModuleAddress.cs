namespace CMP.Constants.Plc.RobotControl;

using CMP.Services.Plc;

/// <summary>
/// TransferModule PLC 地址定义
/// </summary>
public static class TransferModuleAddress
{
    #region 轴位置调整 - 位置设定 (D)

    /// <summary>
    /// Load 位置
    /// </summary>
    public const string LoadPosition = "ZR10118";

    /// <summary>
    /// Unload 位置
    /// </summary>
    public const string UnloadPosition = "ZR10120";

    /// <summary>
    /// 备用位置
    /// </summary>
    public const string SparePosition = "ZR10122";

    #endregion

    #region 轴位置调整 - 移动命令 (M)

    /// <summary>
    /// Load 位置移动
    /// </summary>
    public const string LoadPositionMove = "M1048";

    /// <summary>
    /// Unload 位置移动
    /// </summary>
    public const string UnloadPositionMove = "M1049";

    /// <summary>
    /// 备用位置移动
    /// </summary>
    public const string SparePositionMove = "M1050";

    #endregion

    #region 轴位置调整 - 速度设定 (D)

    /// <summary>
    /// JOG 速度
    /// </summary>
    public const string JogSpeed = "D1022";

    /// <summary>
    /// 自动速度
    /// </summary>
    public const string AutoSpeed = "ZR10100";

    #endregion

    #region 轴位置调整 - 状态 (D/M)

    /// <summary>
    /// 当前坐标
    /// </summary>
    public const string CurrentCoordinate = "D5020";

    /// <summary>
    /// 使能
    /// </summary>
    public const string Enable = "M1032";

    /// <summary>
    /// 使能状态
    /// </summary>
    public const string EnableStatus = "M5048";

    #endregion

    #region 轴位置调整 - JOG控制 (M)

    /// <summary>
    /// JOG 上移
    /// </summary>
    public const string JogUp = "M1033";

    /// <summary>
    /// JOG 上移激活状态
    /// </summary>
    public const string JogUpStatus = "M5052";

    /// <summary>
    /// JOG 中间激活状态
    /// </summary>
    public const string JogMiddleStatus = "M5054";

    /// <summary>
    /// JOG 下移
    /// </summary>
    public const string JogDown = "M1034";

    /// <summary>
    /// JOG 下移激活状态
    /// </summary>
    public const string JogDownStatus = "M5053";

    #endregion

    #region 轴位置调整 - 报警状态 (M)

    /// <summary>
    /// 驱动器报警
    /// </summary>
    public const string DriverAlarm = "M5057";

    /// <summary>
    /// 限位报警
    /// </summary>
    public const string LimitAlarm = "M5055";

    /// <summary>
    /// 报警代码
    /// </summary>
    public const string AlarmCode = "D5026";

    #endregion

    #region 轴位置调整 - 原点 (M)

    /// <summary>
    /// 回原点命令
    /// </summary>
    public const string Origin = "M1035";

    /// <summary>
    /// 原点状态
    /// </summary>
    public const string OriginStatus = "M5049";

    #endregion

    #region 轴位置调整 - 到位信号 (M)

    /// <summary>
    /// Load 位置到位
    /// </summary>
    public const string LoadPositionArrived = "M5080";

    /// <summary>
    /// Unload 位置到位
    /// </summary>
    public const string UnloadPositionArrived = "M5081";

    /// <summary>
    /// 备用位置到位
    /// </summary>
    public const string SparePositionArrived = "M5082";

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
    public const string StationReset = "M1880";

    /// <summary>
    /// 工位操作 - 复位状态
    /// </summary>
    public const string StationResetStatus = "M6201";

    /// <summary>
    /// 工位操作 - 启动
    /// </summary>
    public const string StationStart = "M1883";

    /// <summary>
    /// 工位操作 - 运行状态
    /// </summary>
    public const string StationRunningStatus = "M6203";

    /// <summary>
    /// 工位操作 - 停止
    /// </summary>
    public const string StationStop = "M1885";

    /// <summary>
    /// 工位操作 - 停止状态
    /// </summary>
    public const string StationStoppedStatus = "M6205";

    /// <summary>
    /// Load 操作
    /// </summary>
    public const string LoadOperation = "M1896";

    /// <summary>
    /// Load 操作激活状态
    /// </summary>
    public const string LoadOperationActiveStatus = "M6216";

    /// <summary>
    /// Load 操作完成状态
    /// </summary>
    public const string LoadOperationDoneStatus = "M6232";

    /// <summary>
    /// Unload 操作
    /// </summary>
    public const string UnloadOperation = "M1897";

    /// <summary>
    /// Unload 操作激活状态
    /// </summary>
    public const string UnloadOperationActiveStatus = "M6217";

    /// <summary>
    /// Unload 操作完成状态
    /// </summary>
    public const string UnloadOperationDoneStatus = "M6233";

    /// <summary>
    /// Load门开写入地址
    /// </summary>
    public const string LoadDoorOpenWrite = "M1912";

    /// <summary>
    /// Load门开读取地址
    /// </summary>
    public const string LoadDoorOpenRead = "Y10C0";

    /// <summary>
    /// Load门开完成状态
    /// </summary>
    public const string LoadDoorOpenDoneStatus = "X10B0";

    /// <summary>
    /// Load门关写入地址
    /// </summary>
    public const string LoadDoorCloseWrite = "M1913";

    /// <summary>
    /// Load门关读取地址
    /// </summary>
    public const string LoadDoorCloseRead = "Y10C1";

    /// <summary>
    /// Load门关完成状态
    /// </summary>
    public const string LoadDoorCloseDoneStatus = "X10B1";

    /// <summary>
    /// 侧门开写入地址
    /// </summary>
    public const string SideDoorOpenWrite = "M1914";

    /// <summary>
    /// 侧门开读取地址
    /// </summary>
    public const string SideDoorOpenRead = "Y10C2";

    /// <summary>
    /// 侧门开完成状态
    /// </summary>
    public const string SideDoorOpenDoneStatus = "X10B2";

    /// <summary>
    /// 侧门关写入地址
    /// </summary>
    public const string SideDoorCloseWrite = "M1915";

    /// <summary>
    /// 侧门关读取地址
    /// </summary>
    public const string SideDoorCloseRead = "Y10C3";

    /// <summary>
    /// 侧门关完成状态
    /// </summary>
    public const string SideDoorCloseDoneStatus = "X10B3";

    /// <summary>
    /// Unload门开写入地址
    /// </summary>
    public const string UnloadDoorOpenWrite = "M1916";

    /// <summary>
    /// Unload门开读取地址
    /// </summary>
    public const string UnloadDoorOpenRead = "Y10C4";

    /// <summary>
    /// Unload门开完成状态
    /// </summary>
    public const string UnloadDoorOpenDoneStatus = "X10B4";

    /// <summary>
    /// Unload门关写入地址
    /// </summary>
    public const string UnloadDoorCloseWrite = "M1917";

    /// <summary>
    /// Unload门关读取地址
    /// </summary>
    public const string UnloadDoorCloseRead = "Y10C5";

    /// <summary>
    /// Unload门关完成状态
    /// </summary>
    public const string UnloadDoorCloseDoneStatus = "X10B5";

    /// <summary>
    /// 模块报警
    /// </summary>
    public const string ModuleAlarm = "M6209";

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
    public const string ManualMode = "M1889";

    /// <summary>
    /// 自动模式
    /// </summary>
    public const string AutoMode = "M1888";

    /// <summary>
    /// Load感应
    /// </summary>
    public const string LoadInduction = "X102C";

    /// <summary>
    /// Unload感应
    /// </summary>
    public const string UnloadInduction = "X102D";

    #endregion
}
