namespace CMP.Models.Operation.Clean;

/// <summary>
/// DryClean 配置接口
/// </summary>
public interface IDryCleanConfig
{
    string DryType { get; }
    uint PlcHandleName { get; }
    string PlcAddress { get; }
    ushort PlcPort { get; }

    /// <summary>
    /// DryClean 参数访问器
    /// </summary>
    DryParams Params { get; }

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

    // 气体控制命令
    (uint handle, byte[] value) GetGasCommand(string gasCommand);

    // 液体控制命令
    (uint handle, byte[] value) GetLiquidCommand(string liquidCommand);
}


