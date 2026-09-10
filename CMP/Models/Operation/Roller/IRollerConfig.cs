namespace CMP.Models.Operation.Roller;

/// <summary>
/// Roller 配置接口，定义不同 Roll 类型的 PLC 句柄映射
/// </summary>
public interface IRollerConfig
{
    /// <summary>
    /// Roll 类型名称，如 "Roll A1", "Roll A2"
    /// </summary>
    string RollType { get; }
    
    /// <summary>
    /// PLC Handle 名称，如 "CMD_Roll A1", "CMD_Roll A2"
    /// </summary>
    uint PlcHandleName { get; }
    
    /// <summary>
    /// PLC 地址，默认为 "192.168.2.20"
    /// </summary>
    string PlcAddress { get; }
    
    /// <summary>
    /// PLC 端口，默认为 8500
    /// </summary>
    ushort PlcPort { get; }
    
    /// <summary>
    /// Roller 参数访问器
    /// </summary>
    RollerParams Params { get; }
    
    /// <summary>
    /// 获取初始化命令的 PLC 句柄和值
    /// </summary>
    (uint handle, byte[] value) GetInitializeCommand();
    
    /// <summary>
    /// 获取自动模式命令的 PLC 句柄和值
    /// </summary>
    (uint handle, byte[] value) GetAutoModeCommand();
    
    /// <summary>
    /// 获取手动模式命令的 PLC 句柄和值
    /// </summary>
    (uint handle, byte[] value) GetManualModeCommand();
    
    /// <summary>
    /// 获取测试配方命令的 PLC 句柄和值
    /// </summary>
    /// <param name="command">Test, Start, Stop</param>
    (uint handle, byte[] value) GetTestRecipeCommand(string command);
    
    /// <summary>
    /// 获取电机运行命令的 PLC 句柄和值
    /// </summary>
    /// <param name="motor">电机名称</param>
    (uint handle, byte[] value) GetMotorRunCommand(string motor);
    
    /// <summary>
    /// 获取电机停止命令的 PLC 句柄和值
    /// </summary>
    /// <param name="motor">电机名称</param>
    (uint handle, byte[] value) GetMotorStopCommand(string motor);
    
    /// <summary>
    /// 获取伺服控制命令的 PLC 句柄和值
    /// </summary>
    /// <param name="motor">电机名称</param>
    /// <param name="isOn">是否打开伺服</param>
    (uint handle, ushort value, ushort bitOffset) GetServoCommand(string motor);
    
    /// <summary>
    /// 获取气缸控制命令的 PLC 句柄和值
    /// </summary>
    /// <param name="cylinderCommand">气缸命令</param>
    (uint handle, byte[] value) GetCylinderCommand(string cylinderCommand);
    
    /// <summary>
    /// 获取压力控制命令的 PLC 句柄和值
    /// </summary>
    /// <param name="pressureCommand">压力命令</param>
    (uint handle, byte[] value) GetPressureCommand(string pressureCommand);
    
    /// <summary>
    /// 获取液体控制命令的 PLC 句柄和值
    /// </summary>
    /// <param name="liquidCommand">液体命令</param>
    (uint handle, byte[] value) GetLiquidCommand(string liquidCommand);

    /// <summary>
    /// 获取全纯水阀 Flash 控制命令
    /// </summary>
    (uint handle, byte[] value) GetFlashCommand(string flashCommand);
}
