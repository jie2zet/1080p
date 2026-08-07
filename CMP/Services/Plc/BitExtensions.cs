namespace CMP.Services.Plc;

/// <summary>
/// 位操作扩展方法
/// </summary>
public static class BitExtensions
{
    /// <summary>
    /// 获取 short 的第 bitIndex 位的值 (0-15)
    /// </summary>
    public static bool GetBoolByIndex(this short value, int bitIndex) => (value & (1 << bitIndex)) != 0;

    /// <summary>
    /// 设置 short 的第 bitIndex 位 (0-15)
    /// </summary>
    public static short SetBoolByIndex(this short value, int bitIndex, bool boolValue)
        => boolValue ? (short)(value | (1 << bitIndex)) : (short)(value & ~(1 << bitIndex));
}
