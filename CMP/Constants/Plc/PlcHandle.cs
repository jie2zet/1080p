using System.Reflection;
using System.Runtime.CompilerServices;

namespace CMP.Constants.Plc;

/// <summary>
/// PLC Handle 类型常量
/// </summary>
public static class Handle
{
    public const uint CmdPolish = 0x50;
    public const uint CmdRollA1 = 0x60;
    public const uint CmdRollA2 = 0x63;
    public const uint CmdRollA3 = 0x66;
    public const uint CmdRollB1 = 0x69;
    public const uint CmdRollB2 = 0x6C;
    public const uint CmdRollB3 = 0x6F;
    public const uint CmdPenA1 = 0x72;
    public const uint CmdPenA2 = 0x75;
    public const uint CmdPenB1 = 0x78;
    public const uint CmdPenB2 = 0x7B;
    public const uint CmdDryA = 0x8E;
    public const uint CmdDryB = 0x81;
}

/// <summary>
/// PLC Handle 类型属性
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class PlcHandleAttribute : Attribute
{
    public uint Handle { get; }
    public string Description { get; }
    public ushort BitOffset { get; }

    public PlcHandleAttribute(uint handle, string description = "", ushort bitOffset = 0)
    {
        Handle = handle;
        Description = description;
        BitOffset = bitOffset;
    }
}

/// <summary>
/// PLC Handle 类 - 主文件
/// </summary>
public static partial class PlcHandle
{
    /// <summary>
    /// PLC 参数属性，用于标记需要读写的参数
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ParamAttribute : Attribute
    {
        /// <summary>
        /// Handle 值
        /// </summary>
        public uint Handle { get; }

        /// <summary>
        /// 初始值
        /// </summary>
        public int DefaultValue { get; }

        /// <summary>
        /// 最小值
        /// </summary>
        public int MinValue { get; }

        /// <summary>
        /// 最大值
        /// </summary>
        public int MaxValue { get; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// 标识
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; }

        /// <summary>
        /// 浮点参数的默认值。旧参数继续使用整数属性，新增参数可使用浮点元数据。
        /// </summary>
        public double NumericDefaultValue { get; }

        /// <summary>
        /// 浮点参数的最小值。
        /// </summary>
        public double NumericMinValue { get; }

        /// <summary>
        /// 浮点参数的最大值。
        /// </summary>
        public double NumericMaxValue { get; }

        /// <summary>
        /// 显示的小数位数。
        /// </summary>
        public int DecimalPlaces { get; set; }

        /// <summary>
        /// 图纸标识。
        /// </summary>
        public string DrawingTag { get; set; } = string.Empty;

        /// <summary>
        /// PLC 数据类型，例如 ZR 或 ZF。
        /// </summary>
        public string ZrZf { get; set; } = "ZF";

        /// <summary>
        /// PLC 写入字数。
        /// </summary>
        public int WordCount { get; set; } = 2;

        /// <summary>
        /// 参数显示名称资源键。
        /// </summary>
        public string DisplayNameKey { get; set; } = string.Empty;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="handle">Handle 值</param>
        /// <param name="defaultValue">初始值</param>
        /// <param name="minValue">最小值</param>
        /// <param name="maxValue">最大值</param>
        /// <param name="description">描述</param>
        /// <param name="label">标识</param>
        /// <param name="unit">单位</param>
        public ParamAttribute(uint handle, int defaultValue, int minValue, int maxValue,
            string description = "", string label = "", string unit = "")
        {
            Handle = handle;
            DefaultValue = defaultValue;
            MinValue = minValue;
            MaxValue = maxValue;
            NumericDefaultValue = defaultValue;
            NumericMinValue = minValue;
            NumericMaxValue = maxValue;
            Description = description;
            Label = label;
            Unit = unit;
        }

        /// <summary>
        /// 浮点参数构造函数，保留整数属性以兼容既有调用方。
        /// </summary>
        public ParamAttribute(uint handle, double defaultValue, double minValue, double maxValue,
            string description = "", string label = "", string unit = "")
        {
            Handle = handle;
            DefaultValue = (int)Math.Round(defaultValue);
            MinValue = (int)Math.Round(minValue);
            MaxValue = (int)Math.Round(maxValue);
            NumericDefaultValue = defaultValue;
            NumericMinValue = minValue;
            NumericMaxValue = maxValue;
            Description = description;
            Label = label;
            Unit = unit;
        }
    }
}

/// <summary>
/// PLC Handle 帮助类
/// </summary>
public static class PlcHandleHelper
{
    private static Dictionary<int, (string Name, PlcHandleAttribute Attr)>? _handleCache;
    private static Dictionary<uint, PlcHandle.ParamAttribute>? _paramCacheByHandle;
    private static Dictionary<int, PlcHandle.ParamAttribute>? _paramCacheByValue;
    private static Dictionary<uint, string>? _paramNameCache;

    private static Dictionary<string, (int Value, PlcHandleAttribute Attr)>? _handleCacheByPath;
    private static Dictionary<string, (int Value, PlcHandle.ParamAttribute Attr)>? _paramCacheByPath;

    static PlcHandleHelper()
    {
        BuildHandleCache();
        BuildParamCache();
    }

    /// <summary>
    /// 获取指定 Handle 类型的所有值
    /// </summary>
    /// <param name="handle"> Handle 类型，使用 PlcHandle 常量</param>
    /// <returns>字典：键为字段名，值为 Handle 值</returns>
    public static Dictionary<string, int> GetValuesByHandle(uint handle)
    {
        var result = new Dictionary<string, int>();
        GetValuesRecursively(typeof(PlcHandle), handle, result, "");
        return result;
    }

    /// <summary>
    /// 获取所有Command类型的 Handle 值
    /// </summary>
    public static Dictionary<string, int> GetAllCommandValues()
    {
        return GetValuesByHandle(Handle.CmdPolish);
    }

    /// <summary>
    /// 根据 Handle 值获取 Handle 类型
    /// </summary>
    /// <param name="value"> Handle 值，如1004</param>
    /// <returns> Handle 类型，如PlcHandle.Command</returns>
    public static uint? GetHandle(int value)
    {
        return _handleCache!.TryGetValue(value, out var info) ? info.Attr.Handle : null;
    }

    /// <summary>
    /// 根据 Handle 值获取描述
    /// </summary>
    /// <param name="value"> Handle 值，如1004</param>
    /// <returns>描述，如"初始化"</returns>
    public static string? GetDescription(int value)
    {
        return _handleCache!.TryGetValue(value, out var info) ? info.Attr.Description : null;
    }

    /// <summary>
    /// 根据 Handle 值获取字段名
    /// </summary>
    /// <param name="value"> Handle 值，如1004</param>
    /// <returns>字段名，如"All.Initialize"</returns>
    public static string? GetFieldName(int value)
    {
        return _handleCache!.TryGetValue(value, out var info) ? info.Name : null;
    }

    /// <summary>
    /// 根据 Handle 值获取 BitOffset
    /// </summary>
    /// <param name="value"> Handle 值</param>
    /// <returns>BitOffset 值，如果没有定义则返回 0</returns>
    public static ushort GetBitOffset(int value)
    {
        return _handleCache!.TryGetValue(value, out var info) ? info.Attr.BitOffset : (ushort)0;
    }

    /// <summary>
    /// 获取完整的 Handle 信息
    /// </summary>
    /// <param name="value"> Handle 值</param>
    /// <returns>完整信息</returns>
    public static PlcHandleAttribute? GetHandleInfo(int value)
    {
        return _handleCache!.TryGetValue(value, out var info) ? info.Attr : null;
    }

    /// <summary>
    /// 判断 Handle 值是否为Command类型
    /// </summary>
    public static bool IsCommand(int value)
    {
        return GetHandle(value) == Handle.CmdPolish;
    }

    /// <summary>
    /// 获取所有 Handle 的完整信息
    /// </summary>
    public static Dictionary<int, (string FieldName, uint Handle, string Description)> GetAllHandle()
    {
        return _handleCache!.ToDictionary(
            kvp => kvp.Key,
            kvp => (kvp.Value.Name, kvp.Value.Attr.Handle, kvp.Value.Attr.Description)
        );
    }

    /// <summary>
    /// 根据参数 Handle 获取参数特性
    /// </summary>
    /// <param name="paramHandle">参数的 Handle 值</param>
    /// <returns>参数特性</returns>
    public static PlcHandle.ParamAttribute? GetParamByHandle(uint paramHandle)
    {
        return _paramCacheByHandle!.TryGetValue(paramHandle, out var attr) ? attr : null;
    }

    /// <summary>
    /// 根据参数值获取参数特性（如果有多个相同值的参数，返回第一个）
    /// </summary>
    /// <param name="value">参数的值</param>
    /// <returns>参数特性</returns>
    public static PlcHandle.ParamAttribute? GetParamByValue(int value)
    {
        return _paramCacheByValue!.TryGetValue(value, out var attr) ? attr : null;
    }

    /// <summary>
    /// 获取所有参数特性
    /// </summary>
    /// <returns>参数 Handle 到参数特性的字典</returns>
    public static Dictionary<uint, PlcHandle.ParamAttribute> GetAllParams()
    {
        return new Dictionary<uint, PlcHandle.ParamAttribute>(_paramCacheByHandle!);
    }

    /// <summary>
    /// 获取所有参数特性及其名称
    /// </summary>
    /// <returns>参数 Handle、名称和参数特性的字典</returns>
    public static Dictionary<uint, (string Name, PlcHandle.ParamAttribute Attr)> GetAllParamsWithNames()
    {
        var result = new Dictionary<uint, (string, PlcHandle.ParamAttribute)>();
        foreach (var kvp in _paramCacheByHandle!)
        {
            var name = _paramNameCache!.TryGetValue(kvp.Key, out var n) ? n : "";
            result[kvp.Key] = (name, kvp.Value);
        }
        return result;
    }

    /// <summary>
    /// 根据参数 Handle 获取参数名称
    /// </summary>
    /// <param name="paramHandle">参数的 Handle 值</param>
    /// <returns>参数名称</returns>
    public static string? GetParamName(uint paramHandle)
    {
        return _paramNameCache!.TryGetValue(paramHandle, out var name) ? name : null;
    }

    /// <summary>
    /// 根据路径获取 Handle 类型
    /// </summary>
    /// <param name="path">字段路径，如 "CmdPolish.Motor.PlatenRotation.Run"</param>
    /// <returns>Handle 类型</returns>
    public static uint? GetHandleByPath(string path)
    {
        // 移除 "PlcHandle." 前缀（如果存在）
        var normalizedPath = path.StartsWith("PlcHandle.") ? path.Substring(10) : path;
        return _handleCacheByPath!.TryGetValue(normalizedPath, out var info) ? info.Attr.Handle : null;
    }

    /// <summary>
    /// 根据路径获取字段值
    /// </summary>
    /// <param name="path">字段路径，如 "CmdPolish.Motor.PlatenRotation.Run"</param>
    /// <returns>字段值</returns>
    public static int? GetValueByPath(string path)
    {
        var normalizedPath = path.StartsWith("PlcHandle.") ? path.Substring(10) : path;
        return _handleCacheByPath!.TryGetValue(normalizedPath, out var info) ? info.Value : null;
    }

    /// <summary>
    /// 根据路径获取 Handle 信息
    /// </summary>
    /// <param name="path">字段路径</param>
    /// <returns>Handle 信息</returns>
    public static PlcHandleAttribute? GetHandleInfoByPath(string path)
    { 
        var normalizedPath = path.StartsWith("PlcHandle.") ? path.Substring(10) : path;
        return _handleCacheByPath!.TryGetValue(normalizedPath, out var info) ? info.Attr : null;
    }

    /// <summary>
    /// 根据路径获取参数特性
    /// </summary>
    /// <param name="path">参数路径，如 "CmdPolish.Motor.PlatenRotation.Param.Velocity"</param>
    /// <returns>参数特性</returns>
    public static PlcHandle.ParamAttribute? GetParamByPath(string path)
    {
        var normalizedPath = path.StartsWith("PlcHandle.") ? path.Substring(10) : path;
        return _paramCacheByPath!.TryGetValue(normalizedPath, out var info) ? info.Attr : null;
    }

    private static void BuildHandleCache()
    {
        _handleCache = new Dictionary<int, (string, PlcHandleAttribute)>();
        _handleCacheByPath = new Dictionary<string, (int, PlcHandleAttribute)>();
        BuildCacheRecursively(typeof(PlcHandle), "");
    }

    private static void BuildParamCache()
    {
        _paramCacheByHandle = new Dictionary<uint, PlcHandle.ParamAttribute>();
        _paramCacheByValue = new Dictionary<int, PlcHandle.ParamAttribute>();
        _paramNameCache = new Dictionary<uint, string>();
        _paramCacheByPath = new Dictionary<string, (int, PlcHandle.ParamAttribute)>();
        BuildParamCacheRecursively(typeof(PlcHandle), "");
    }

    private static void BuildParamCacheRecursively(Type type, string prefix)
    {
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = field.GetCustomAttribute<PlcHandle.ParamAttribute>();
            if (attr != null && field.FieldType == typeof(int))
            {
                var value = (int)(field.GetValue(null) ?? throw new InvalidOperationException("Field value is null"));

                // 构建参数名称，例如 "PlatenRotation.Param.Velocity"
                var name = string.IsNullOrEmpty(prefix) ? field.Name : $"{prefix}.{field.Name}";

                // 按 Handle 存储（唯一键）
                _paramCacheByHandle![attr.Handle] = attr;

                // 存储参数名称
                _paramNameCache![attr.Handle] = name;

                // 按值存储（如果值重复，后面的会覆盖前面的）
                if (!_paramCacheByValue!.ContainsKey(value))
                {
                    _paramCacheByValue[value] = attr;
                }

                // 按路径存储（支持完整路径查找）
                _paramCacheByPath![name] = (value, attr);
            }
        }

        foreach (var nestedType in type.GetNestedTypes())
        {
            var nestedPrefix = string.IsNullOrEmpty(prefix) ? nestedType.Name : $"{prefix}.{nestedType.Name}";
            BuildParamCacheRecursively(nestedType, nestedPrefix);
        }
    }

    private static void BuildCacheRecursively(Type type, string prefix)
    {
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = field.GetCustomAttribute<PlcHandleAttribute>();
            if (attr != null && field.FieldType == typeof(int))
            {
                var name = string.IsNullOrEmpty(prefix) ? field.Name : $"{prefix}.{field.Name}";
                var value = (int)(field.GetValue(null) ?? throw new InvalidOperationException("Field value is null"));
                _handleCache![value] = (name, attr);

                // 按路径存储（支持完整路径查找）
                _handleCacheByPath![name] = (value, attr);
            }
        }

        foreach (var nestedType in type.GetNestedTypes())
        {
            var nestedPrefix = string.IsNullOrEmpty(prefix) ? nestedType.Name : $"{prefix}.{nestedType.Name}";
            BuildCacheRecursively(nestedType, nestedPrefix);
        }
    }

    private static void GetValuesRecursively(Type type, uint targetHandle, Dictionary<string, int> result, string prefix)
    {
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = field.GetCustomAttribute<PlcHandleAttribute>();
            if (attr != null && attr.Handle == targetHandle)
            {
                var name = string.IsNullOrEmpty(prefix) ? field.Name : $"{prefix}.{field.Name}";
                result[name] = (int)(field.GetValue(null) ?? throw new InvalidOperationException("Field value is null"));
            }
        }

        foreach (var nestedType in type.GetNestedTypes())
        {
            var nestedPrefix = string.IsNullOrEmpty(prefix) ? nestedType.Name : $"{prefix}.{nestedType.Name}";
            GetValuesRecursively(nestedType, targetHandle, result, nestedPrefix);
        }
    }
}

/// <summary>
/// 扩展方法，便于使用
/// </summary>
public static class PlcHandleExtensions
{
    /// <summary>
    /// 获取 Handle 类型（通过路径查找，避免值重复问题）
    /// </summary>
    /// <param name="value">字段值</param>
    /// <param name="expression">自动捕获的调用表达式（无需手动传入）</param>
    /// <returns>Handle 类型</returns>
    public static uint Handle(this int value, [CallerArgumentExpression(nameof(value))] string? expression = null)
    {
        // 优先使用路径查找
        if (!string.IsNullOrEmpty(expression))
        {
            var handle = PlcHandleHelper.GetHandleByPath(expression);
            if (handle.HasValue)
                return handle.Value;
        }

        // 降级到值查找（保持向后兼容）
        return PlcHandleHelper.GetHandle(value) ?? throw new InvalidOperationException($"Invalid handle value: {value}, expression: {expression}");
    }

    /// <summary>
    /// 获取 bytes 类型的值
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="value">要转换的值</param>
    /// <param name="wordOffset">字偏移量</param>
    /// <returns>字节数组</returns>
    public static byte[] Value<T>(this T value, int wordOffset = 0) where T : struct
    {
        var dst = new byte[2048];
        var bytes = value switch
        {
            bool b => BitConverter.GetBytes(b),
            byte b => [b],
            sbyte sb => [(byte)sb],
            short s => BitConverter.GetBytes(s),
            ushort us => BitConverter.GetBytes(us),
            int i => BitConverter.GetBytes(i),
            uint ui => BitConverter.GetBytes(ui),
            long l => BitConverter.GetBytes(l),
            ulong ul => BitConverter.GetBytes(ul),
            float f => BitConverter.GetBytes(f),
            double d => BitConverter.GetBytes(d),
            char c => BitConverter.GetBytes(c),
            _ => throw new NotSupportedException($"Type {typeof(T)} is not supported")
        };

        bytes.CopyTo(dst.AsSpan(wordOffset * 2, bytes.Length));
        return dst;
    }


    /// <summary>
    /// 获取 Handle 的描述信息（通过路径查找）
    /// </summary>
    /// <param name="value">字段值</param>
    /// <param name="expression">自动捕获的调用表达式</param>
    /// <returns>描述信息</returns>
    public static string HandleDescription(this int value, [CallerArgumentExpression(nameof(value))] string? expression = null)
    {
        // 优先使用路径查找
        if (!string.IsNullOrEmpty(expression))
        {
            var info = PlcHandleHelper.GetHandleInfoByPath(expression);
            if (info != null)
                return info.Description;
        }

        // 降级到值查找
        return PlcHandleHelper.GetDescription(value) ?? throw new InvalidOperationException($"Invalid handle value: {value}");
    }

    /// <summary>
    /// 获取 BitOffset（通过路径查找）
    /// </summary>
    /// <param name="value">字段值</param>
    /// <param name="expression">自动捕获的调用表达式</param>
    /// <returns>BitOffset</returns>
    public static ushort BitOffset(this int value, [CallerArgumentExpression(nameof(value))] string? expression = null)
    {
        // 优先使用路径查找
        if (!string.IsNullOrEmpty(expression))
        {
            var info = PlcHandleHelper.GetHandleInfoByPath(expression);
            if (info != null)
                return info.BitOffset;
        }

        // 降级到值查找
        return PlcHandleHelper.GetBitOffset(value);
    }

    /// <summary>
    /// 获取字段名（通过路径查找）
    /// </summary>
    /// <param name="value">字段值</param>
    /// <param name="expression">自动捕获的调用表达式</param>
    /// <returns>字段名</returns>
    public static string? FieldName(this int value, [CallerArgumentExpression(nameof(value))] string? expression = null)
    {
        // 如果有表达式，直接返回标准化后的路径
        if (!string.IsNullOrEmpty(expression))
        {
            return expression.StartsWith("PlcHandle.") ? expression.Substring(10) : expression;
        }

        // 降级到值查找
        return PlcHandleHelper.GetFieldName(value);
    }

    /// <summary>
    /// 判断 Handle 值是否为Command类型
    /// </summary>
    public static bool IsCommand(this int value)
    {
        return PlcHandleHelper.IsCommand(value);
    }

    /// <summary>
    /// 获取参数的 Handle 值（通过路径查找）
    /// </summary>
    /// <param name="paramValue">参数值</param>
    /// <param name="expression">自动捕获的调用表达式</param>
    /// <returns>参数的 Handle 值</returns>
    public static uint ParamHandle(this int paramValue, [CallerArgumentExpression(nameof(paramValue))] string? expression = null)
    {
        // 优先使用路径查找
        if (!string.IsNullOrEmpty(expression))
        {
            var attr = PlcHandleHelper.GetParamByPath(expression);
            if (attr != null)
                return attr.Handle;
        }

        // 降级到值查找
        var fallbackAttr = PlcHandleHelper.GetParamByValue(paramValue);
        if (fallbackAttr == null)
            throw new InvalidOperationException($"参数值 {paramValue} 未找到对应的 ParamAttribute, expression: {expression}");
        return fallbackAttr.Handle;
    }

    /// <summary>
    /// 获取参数的最大值（通过路径查找）
    /// </summary>
    public static int MaxValue(this int paramValue, [CallerArgumentExpression(nameof(paramValue))] string? expression = null)
    {
        var attr = GetParamAttributeInternal(paramValue, expression);
        return attr.MaxValue;
    }

    /// <summary>
    /// 获取参数的最小值（通过路径查找）
    /// </summary>
    public static int MinValue(this int paramValue, [CallerArgumentExpression(nameof(paramValue))] string? expression = null)
    {
        var attr = GetParamAttributeInternal(paramValue, expression);
        return attr.MinValue;
    }

    /// <summary>
    /// 获取参数的默认值（通过路径查找）
    /// </summary>
    public static int DefaultValue(this int paramValue, [CallerArgumentExpression(nameof(paramValue))] string? expression = null)
    {
        var attr = GetParamAttributeInternal(paramValue, expression);
        return attr.DefaultValue;
    }

    /// <summary>
    /// 获取参数的完整特性信息（通过路径查找）
    /// </summary>
    public static PlcHandle.ParamAttribute? GetParamAttribute(this int paramValue, [CallerArgumentExpression(nameof(paramValue))] string? expression = null)
    {
        // 优先使用路径查找
        if (!string.IsNullOrEmpty(expression))
        {
            var attr = PlcHandleHelper.GetParamByPath(expression);
            if (attr != null)
                return attr;
        }

        // 降级到值查找
        return PlcHandleHelper.GetParamByValue(paramValue);
    }

    /// <summary>
    /// 内部辅助方法：获取参数特性（带路径查找）
    /// </summary>
    private static PlcHandle.ParamAttribute GetParamAttributeInternal(int paramValue, string? expression)
    {
        // 优先使用路径查找
        if (!string.IsNullOrEmpty(expression))
        {
            var attr = PlcHandleHelper.GetParamByPath(expression);
            if (attr != null)
                return attr;
        }

        // 降级到值查找
        var fallbackAttr = PlcHandleHelper.GetParamByValue(paramValue);
        if (fallbackAttr == null)
            throw new InvalidOperationException($"参数值 {paramValue} 未找到对应的 ParamAttribute, expression: {expression}");
        return fallbackAttr;
    }

    /// <summary>
    /// 检查值是否在参数的有效范围内（通过路径查找）
    /// </summary>
    public static bool IsInRange(this int paramValue, int valueToCheck, [CallerArgumentExpression(nameof(paramValue))] string? expression = null)
    {
        var attr = GetParamAttributeInternal(paramValue, expression);
        return valueToCheck >= attr.MinValue && valueToCheck <= attr.MaxValue;
    }

    /// <summary>
    /// 获取参数的描述（通过路径查找）
    /// </summary>
    public static string ParamDescription(this int paramValue, [CallerArgumentExpression(nameof(paramValue))] string? expression = null)
    {
        var attr = GetParamAttributeInternal(paramValue, expression);
        return attr.Description;
    }

    /// <summary>
    /// 获取参数的标识（通过路径查找）
    /// </summary>
    public static string ParamLabel(this int paramValue, [CallerArgumentExpression(nameof(paramValue))] string? expression = null)
    {
        var attr = GetParamAttributeInternal(paramValue, expression);
        return attr.Label;
    }

    /// <summary>
    /// 获取参数的单位（通过路径查找）
    /// </summary>
    public static string ParamUnit(this int paramValue, [CallerArgumentExpression(nameof(paramValue))] string? expression = null)
    {
        var attr = GetParamAttributeInternal(paramValue, expression);
        return attr.Unit;
    }

    /// <summary>
    /// 获取参数的名称（通过路径查找）
    /// </summary>
    public static string? ParamName(this int paramValue, [CallerArgumentExpression(nameof(paramValue))] string? expression = null)
    {
        // 如果有表达式，直接返回标准化后的路径
        if (!string.IsNullOrEmpty(expression))
        {
            return expression.StartsWith("PlcHandle.") ? expression.Substring(10) : expression;
        }

        // 降级到值查找
        var attr = PlcHandleHelper.GetParamByValue(paramValue);
        if (attr == null)
            return null;
        return PlcHandleHelper.GetParamName(attr.Handle);
    }

}
