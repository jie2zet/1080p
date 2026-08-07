namespace CMP.Models.Operation.Clean;

/// <summary>
/// PenClean 配置接口
/// </summary>
public interface IPenCleanConfig
{
    string PenType { get; }
    uint PlcHandleName { get; }
    string PlcAddress { get; }
    ushort PlcPort { get; }
    
    /// <summary>
    /// PenClean 参数访问器
    /// </summary>
    PenCleanParams Params { get; }

    // 基础控制命令
    (uint handle, byte[] value) GetInitializeCommand();
    (uint handle, byte[] value) GetAutoModeCommand();
    (uint handle, byte[] value) GetManualModeCommand();
    (uint handle, byte[] value) GetTestRecipeCommand(string command);

    // 电机控制命令
    (uint handle, byte[] value) GetMotorCommand(string motorCommand);
    (uint handle, ushort value, ushort bitOffset) GetServoCommand(string motor);

    // 气缸控制命令
    (uint handle, byte[] value) GetCylinderCommand(string cylinderCommand);

    // 压力控制命令
    (uint handle, byte[] value) GetPressureCommand(string pressureCommand);

    // 液体控制命令
    (uint handle, byte[] value) GetLiquidCommand(string liquidCommand);

    // 气体控制命令
    (uint handle, byte[] value) GetGasCommand(string gasCommand);
}

