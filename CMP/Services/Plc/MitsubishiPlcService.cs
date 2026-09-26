using System.Collections.Concurrent;
using CMP.Services.Log;
using CMP.Services.McBinaryComm;
using HslCommunication;
using HslCommunication.Core;
using HslCommunication.Profinet.Melsec;
using System.Linq.Expressions;
using System.Reflection;

namespace CMP.Services.Plc;

/// <summary>
/// D 寄存器位地址
/// </summary>
/// <param name="Address">D 地址，如 "D100"</param>
/// <param name="BitIndex">位索引 0-15</param>
public record DBitAddress(string Address, int BitIndex)
{
    public override string ToString() => $"{Address}.{BitIndex}";
}

internal static class PlcBatchAddressHelper
{
    public static bool TryParseDAddress(string address, out int addressNumber)
    {
        addressNumber = 0;
        if (string.IsNullOrWhiteSpace(address)) return false;

        var raw = address.Trim();
        if (!raw.StartsWith("D", StringComparison.OrdinalIgnoreCase)) return false;
        if (raw.Contains('.')) return false;

        return int.TryParse(raw[1..], out addressNumber) && addressNumber >= 0;
    }

    public static string ToDAddress(int addressNumber) => $"D{addressNumber}";

    public static bool TryParseMAddress(string address, out int addressNumber)
    {
        addressNumber = 0;
        if (string.IsNullOrWhiteSpace(address)) return false;

        var raw = address.Trim();
        if (!raw.StartsWith("M", StringComparison.OrdinalIgnoreCase)) return false;
        if (raw.Contains('.')) return false;

        return int.TryParse(raw[1..], out addressNumber) && addressNumber >= 0;
    }

    public static string ToMAddress(int addressNumber) => $"M{addressNumber}";
}

public enum PlcBatchAddressType
{
    Auto,
    D,
    M
}

public abstract class PlcBatchReadItemBase
{
    protected PlcBatchReadItemBase(string address, ushort wordLength, PlcBatchAddressType addressType)
    {
        Address = address;
        WordLength = wordLength;
        AddressType = addressType;
    }

    public string Address { get; }
    public ushort WordLength { get; }
    public PlcBatchAddressType AddressType { get; }
    public bool IsSuccess { get; protected set; }
    public OperateResult<short[]> OriginResult { get; protected set; } = new OperateResult<short[]>
    {
        IsSuccess = false,
        Message = "Not read yet"
    };

    internal abstract void ApplyBlockResult(OperateResult<short[]> blockResult, int startAddressNumber);
}

public sealed class PlcBatchReadItem<T> : PlcBatchReadItemBase
{
    private readonly Func<short[], T> _converter;
    private readonly Action<T>? _assign;

    public PlcBatchReadItem(
        string address,
        ushort wordLength = 0,
        Action<T>? assign = null,
        Func<short[], T>? converter = null,
        PlcBatchAddressType addressType = PlcBatchAddressType.Auto)
        : base(address, wordLength == 0 ? GetDefaultWordLength() : wordLength, addressType)
    {
        _assign = assign;
        _converter = converter ?? ConvertFromWords;
    }

    public T Result { get; private set; } = default!;

    /// <summary>
    /// 绑定 Action 回调 (支持直接传入赋值逻辑，替代复杂的表达式树)
    /// </summary>
    public static PlcBatchReadItem<T> Bind(
        string address,
        Action<T> updateAction, // 关键点：这里直接接收 Action<T>
        ushort wordLength = 0,
        Func<short[], T>? converter = null,
        PlcBatchAddressType addressType = PlcBatchAddressType.Auto)
    {
        // 因为 Action 已经包含了赋值逻辑，所以我们直接跳过所有繁琐的反射和表达式树解析，
        // 原封不动地传给底层的构造函数即可！性能甚至比用表达式树更好！
        return new PlcBatchReadItem<T>(
            address,
            wordLength,
            updateAction,
            converter,
            addressType);
    }

    /// <summary>
    /// 通过表达式绑定目标成员，避免手写 assign: PlcBatchReadItem&lt;float&gt;.Bind("D1000", () => SomeProperty)
    /// </summary>
    public static PlcBatchReadItem<T> Bind(
        string address,
        Expression<Func<T>> targetExpression,
        ushort wordLength = 0,
        Func<short[], T>? converter = null,
        PlcBatchAddressType addressType = PlcBatchAddressType.Auto)
    {
        if (targetExpression.Body is not MemberExpression memberExpression)
            throw new ArgumentException("targetExpression 必须是可写成员表达式", nameof(targetExpression));

        var member = memberExpression.Member;
        var targetGetter = BuildTargetGetter(memberExpression.Expression);

        return new PlcBatchReadItem<T>(
            address,
            wordLength,
            value =>
            {
                var target = targetGetter?.Invoke();

                switch (member)
                {
                    case PropertyInfo propertyInfo when propertyInfo.CanWrite:
                        propertyInfo.SetValue(target, value);
                        break;
                    case FieldInfo fieldInfo:
                        fieldInfo.SetValue(target, value);
                        break;
                    default:
                        throw new NotSupportedException($"表达式成员不可写: {member.Name}");
                }
            },
            converter,
            addressType);
    }

    private static Func<object?>? BuildTargetGetter(Expression? expression)
    {
        if (expression is null)
        {
            return null;
        }

        var convert = Expression.Convert(expression, typeof(object));
        var lambda = Expression.Lambda<Func<object?>>(convert);
        return lambda.Compile();
    }

    internal override void ApplyBlockResult(OperateResult<short[]> blockResult, int startAddressNumber)
    {
        if (!blockResult.IsSuccess)
        {
            IsSuccess = false;
            OriginResult = new OperateResult<short[]>
            {
                IsSuccess = false,
                Message = blockResult.Message,
                Content = []
            };
            return;
        }

        if (!PlcBatchAddressHelper.TryParseDAddress(Address, out var addressNumber))
        {
            IsSuccess = false;
            OriginResult = new OperateResult<short[]>
            {
                IsSuccess = false,
                Message = $"不支持的地址格式: {Address}",
                Content = []
            };
            return;
        }

        var offset = addressNumber - startAddressNumber;
        if (offset < 0 || offset + WordLength > blockResult.Content.Length)
        {
            IsSuccess = false;
            OriginResult = new OperateResult<short[]>
            {
                IsSuccess = false,
                Message = $"地址 {Address} 超出批量读取范围",
                Content = []
            };
            return;
        }

        var words = new short[WordLength];
        Array.Copy(blockResult.Content, offset, words, 0, WordLength);

        try
        {
            Result = _converter(words);
            _assign?.Invoke(Result);
            IsSuccess = true;
            OriginResult = OperateResult.CreateSuccessResult(words);
        }
        catch (Exception ex)
        {
            IsSuccess = false;
            OriginResult = new OperateResult<short[]>
            {
                IsSuccess = false,
                Message = ex.Message,
                Content = words
            };
        }
    }

    internal void ApplyBoolBlockResult(OperateResult<bool[]> blockResult, int startAddressNumber)
    {
        if (typeof(T) != typeof(bool))
        {
            throw new NotSupportedException("ApplyBoolBlockResult 仅支持 T=bool");
        }

        if (!blockResult.IsSuccess)
        {
            IsSuccess = false;
            OriginResult = new OperateResult<short[]>
            {
                IsSuccess = false,
                Message = blockResult.Message,
                Content = []
            };
            return;
        }

        if (!PlcBatchAddressHelper.TryParseMAddress(Address, out var addressNumber))
        {
            IsSuccess = false;
            OriginResult = new OperateResult<short[]>
            {
                IsSuccess = false,
                Message = $"不支持的 M 地址格式: {Address}",
                Content = []
            };
            return;
        }

        var offset = addressNumber - startAddressNumber;
        if (offset < 0 || offset >= blockResult.Content.Length)
        {
            IsSuccess = false;
            OriginResult = new OperateResult<short[]>
            {
                IsSuccess = false,
                Message = $"地址 {Address} 超出批量读取范围",
                Content = []
            };
            return;
        }

        Result = (T)(object)blockResult.Content[offset];
        _assign?.Invoke(Result);
        IsSuccess = true;
        OriginResult = OperateResult.CreateSuccessResult(new[] { (short)(((bool)(object)Result) ? 1 : 0) });
    }

    private static ushort GetDefaultWordLength()
    {
        var t = typeof(T);

        if (t == typeof(bool) || t == typeof(short) || t == typeof(ushort)) return 1;
        if (t == typeof(int) || t == typeof(uint) || t == typeof(float)) return 2;
        if (t == typeof(long) || t == typeof(ulong) || t == typeof(double)) return 4;
        if (t == typeof(byte[])) return 1;
        if (t == typeof(short[])) return 1;

        throw new NotSupportedException($"类型 {t} 需要显式指定 wordLength 或 converter");
    }

    private static T ConvertFromWords(short[] words)
    {
        var t = typeof(T);

        if (t == typeof(bool)) return (T)(object)(words[0] != 0);
        if (t == typeof(short)) return (T)(object)words[0];
        if (t == typeof(ushort)) return (T)(object)unchecked((ushort)words[0]);
        if (t == typeof(short[])) return (T)(object)words;

        var bytes = ToBytes(words);

        if (t == typeof(byte[])) return (T)(object)bytes;
        if (t == typeof(int)) return (T)(object)BitConverter.ToInt32(bytes, 0);
        if (t == typeof(uint)) return (T)(object)BitConverter.ToUInt32(bytes, 0);
        if (t == typeof(float)) return (T)(object)BitConverter.ToSingle(bytes, 0);
        if (t == typeof(long)) return (T)(object)BitConverter.ToInt64(bytes, 0);
        if (t == typeof(ulong)) return (T)(object)BitConverter.ToUInt64(bytes, 0);
        if (t == typeof(double)) return (T)(object)BitConverter.ToDouble(bytes, 0);

        throw new NotSupportedException($"不支持的批量转换类型: {t}");
    }

    private static byte[] ToBytes(short[] words)
    {
        var bytes = new byte[words.Length * 2];
        for (var i = 0; i < words.Length; i++)
        {
            var wordBytes = BitConverter.GetBytes(words[i]);
            bytes[i * 2] = wordBytes[0];
            bytes[i * 2 + 1] = wordBytes[1];
        }

        return bytes;
    }
}

/// <summary>
/// 三菱PLC通信服务
/// </summary>
public class MitsubishiPlcService : IDisposable
{
    /// <summary>
    /// 构造 D 寄存器单词位转换器，适用于 PlcBatchReadItem 的 converter 参数。
    /// </summary>
    /// <param name="bitIndex">位索引，范围 0-15</param>
    public static Func<short[], bool> ToBitConverter(int bitIndex)
    {
        if (bitIndex is < 0 or > 15)
            throw new ArgumentOutOfRangeException(nameof(bitIndex), bitIndex, "bitIndex 必须在 0-15 范围内");

        return words => words.Length > 0 && (unchecked((ushort)words[0]) & (1 << bitIndex)) != 0;
    }

    /// <summary>
    /// 根据 D 位地址构造位转换器。
    /// </summary>
    public static Func<short[], bool> ToBitConverter(DBitAddress dBit) => ToBitConverter(dBit.BitIndex);

    #region 调试模式

    /// <summary>
    /// 调试模式开关，开启后读写操作返回 mock 值
    /// </summary>
    public static bool DebugMode { get; set; } = false;

    private readonly Random _random = new();

    /// <summary>
    /// 生成 mock 读取结果
    /// </summary>
    private OperateResult<T> MockRead<T>(ushort length = 1)
    {
        var shortArray = new short[length];
        for (var i = 0; i < shortArray.Length; i++)
            shortArray[i] = (short)_random.Next(-100, 100);

        var boolArray = new bool[length];
        for (var i = 0; i < boolArray.Length; i++)
            boolArray[i] = _random.Next(2) == 1;

        var byteArray = new byte[length];
        _random.NextBytes(byteArray);

        object value = typeof(T) switch
        {
            var t when t == typeof(bool) => _random.Next(2) == 1,
            var t when t == typeof(short) => (short)_random.Next(-100, 100),
            var t when t == typeof(short[]) => shortArray,
            var t when t == typeof(ushort) => (ushort)_random.Next(0, 200),
            var t when t == typeof(int) => _random.Next(-1000, 1000),
            var t when t == typeof(uint) => (uint)_random.Next(0, 2000),
            var t when t == typeof(long) => (long)_random.Next(-10000, 10000),
            var t when t == typeof(ulong) => (ulong)_random.Next(0, 20000),
            var t when t == typeof(float) => (float)(_random.NextDouble() * 200 - 100),
            var t when t == typeof(double) => _random.NextDouble() * 200 - 100,
            var t when t == typeof(string) => "MOCK",
            var t when t == typeof(bool[]) => boolArray,
            var t when t == typeof(byte[]) => byteArray,
            _ => default(T)!
        };
        return OperateResult.CreateSuccessResult((T)value);
    }

    /// <summary>
    /// 生成 mock 写入结果
    /// </summary>
    private static OperateResult MockWrite() => OperateResult.CreateSuccessResult();

    #endregion

    #region 单例与多端口入口（三菱 PLC 统一实现）

    public const string IpAddressInputWrong = "IpAddress input wrong";

    private static readonly Lazy<MitsubishiPlcService> MainLazy = new(() =>
        new MitsubishiPlcService(PLCAddress.IP_ADDRESS, PLCAddress.PORT));

    private static readonly ConcurrentDictionary<int, Lazy<MitsubishiPlcService>> PortLazy = new();

    /// <summary>主站（端口与 <see cref="PLCAddress.PORT"/> 一致）。应用启动后默认自动连接。</summary>
    public static MitsubishiPlcService Main => MainLazy.Value;

    /// <summary>兼容旧代码中 McBinaryPlcComm.Instance 的命名。</summary>
    public static MitsubishiPlcService Instance => Main;

    /// <summary>按 TCP 端口获取或创建连接（同 IP：<see cref="PLCAddress.IP_ADDRESS"/>）。主端口返回 <see cref="Main"/>。</summary>
    public static MitsubishiPlcService GetOrCreateForPort(int port)
    {
        if (port == PLCAddress.PORT)
            return Main;

        return PortLazy.GetOrAdd(port, _ => new Lazy<MitsubishiPlcService>(() =>
            new MitsubishiPlcService(PLCAddress.IP_ADDRESS, port))).Value;
    }

    /// <summary>机器人控制 PLC（端口 7005），与预连接端口表共用同一实例。</summary>
    public static MitsubishiPlcService RobotCtrlInstance => GetOrCreateForPort(7005);


    #endregion

    private MelsecMcNet _plc = new();
    private bool _disposed;
    private readonly object _connectLock = new();

    public string IpAddress { get; private set; } = string.Empty;
    public int Port { get; private set; }
    public bool IsConnected { get; private set; }

    public event EventHandler<bool>? ConnectionStateChanged;

    public MitsubishiPlcService()
    {
        DebugMode = true;
    }

    public MitsubishiPlcService(string ipAddress, int port, string? logPath = null)
    {
        _plc = new MelsecMcNet(ipAddress, port);
        if (!string.IsNullOrEmpty(logPath))
            _plc.LogNet = new HslCommunication.LogNet.LogNetSingle(logPath);
        if (!DebugMode)
            Connect(ipAddress, port);
    }

    /// <summary>预热主 PLC 与机器人控制（7005）连接。</summary>
    public static void EnsureConnected()
    {
        _ = Main;
        _ = RobotCtrlInstance;
    }

    /// <summary>预热主连接。</summary>
    public static void EnsureMainConnected()
    {
        _ = Main;
    }

    /// <summary>预热机器人控制连接（端口 7005）。</summary>
    public static void EnsureRobotCtrlConnected()
    {
        _ = RobotCtrlInstance;
    }

    /// <summary>关闭主 PLC 与各端口连接，避免退出后套接字线程拖住进程。</summary>
    public static void DisconnectAll()
    {
        try
        {
            if (MainLazy.IsValueCreated)
            {
                Main.Disconnect();
            }
        }
        catch
        {
            // 退出时断开失败不阻断关闭
        }

        foreach (var item in PortLazy.Values)
        {
            try
            {
                if (item.IsValueCreated)
                {
                    item.Value.Disconnect();
                }
            }
            catch
            {
                // 退出时断开失败不阻断关闭
            }
        }
    }


    #region 连接管理

    public OperateResult Connect(string ipAddress, int port)
    {
        lock (_connectLock)
        {
        if (DebugMode)
        {
            IsConnected = true;
            ConnectionStateChanged?.Invoke(this, true);
            return OperateResult.CreateSuccessResult();
        }

        if (!System.Net.IPAddress.TryParse(ipAddress, out _))
            return new OperateResult { IsSuccess = false, Message = "无效的IP地址格式" };

        IpAddress = ipAddress;
        Port = port;
        _plc.IpAddress = ipAddress;
        _plc.Port = port;
        // 统一策略：连接前先关闭旧连接
        _plc.ConnectClose();

        try
        {
            var result = _plc.ConnectServer();
            IsConnected = result.IsSuccess;

            if (IsConnected)
                Logger.Info($"PLC连接成功: {ipAddress}:{port}");
            else
                Logger.Error($"PLC连接失败: {ipAddress}:{port}, {result.Message}");

            ConnectionStateChanged?.Invoke(this, IsConnected);
            return result;
        }
        catch (Exception ex)
        {
            Logger.Error($"PLC连接异常: {ex.Message}", ex);
            IsConnected = false;
            ConnectionStateChanged?.Invoke(this, false);
            return new OperateResult { IsSuccess = false, Message = ex.Message };
        }
        }
    }

    public async Task<OperateResult> ConnectAsync(string ipAddress, int port) 
        => await Task.Run(() => Connect(ipAddress, port));

    public void Disconnect()
    {
        lock (_connectLock)
        {
            if (!DebugMode)
            {
                _plc.ConnectClose();
            }

            IsConnected = false;
        }

        ConnectionStateChanged?.Invoke(this, false);
    }

    public OperateResult Reconnect()
    {
        if (DebugMode)
            return OperateResult.CreateSuccessResult();
        if (string.IsNullOrEmpty(IpAddress) || Port == 0)
            return new OperateResult { IsSuccess = false, Message = "IP地址或端口未设置" };
        return Connect(IpAddress, Port);
    }

    #endregion

    #region 泛型读写

    /// <summary>
    /// 泛型读取
    /// </summary>
    public OperateResult<T> Read<T>(string address, ushort length = 1)
    {
        if (DebugMode) return MockRead<T>(length);
        
        return typeof(T) switch
        {
            var t when t == typeof(bool) => (OperateResult<T>)(object)_plc.ReadBool(address),
            var t when t == typeof(short) => (OperateResult<T>)(object)_plc.ReadInt16(address),
            var t when t == typeof(short[]) => (OperateResult<T>)(object)_plc.ReadInt16(address, length),
            var t when t == typeof(ushort) => (OperateResult<T>)(object)_plc.ReadUInt16(address),
            var t when t == typeof(int) => (OperateResult<T>)(object)_plc.ReadInt32(address),
            var t when t == typeof(uint) => (OperateResult<T>)(object)_plc.ReadUInt32(address),
            var t when t == typeof(long) => (OperateResult<T>)(object)_plc.ReadInt64(address),
            var t when t == typeof(ulong) => (OperateResult<T>)(object)_plc.ReadUInt64(address),
            var t when t == typeof(float) => (OperateResult<T>)(object)_plc.ReadFloat(address),
            var t when t == typeof(double) => (OperateResult<T>)(object)_plc.ReadDouble(address),
            var t when t == typeof(string) => (OperateResult<T>)(object)_plc.ReadString(address, length),
            var t when t == typeof(bool[]) => (OperateResult<T>)(object)_plc.ReadBool(address, length),
            var t when t == typeof(byte[]) => (OperateResult<T>)(object)_plc.Read(address, length),
            _ => throw new NotSupportedException($"不支持的类型: {typeof(T)}")
        };
    }

    /// <summary>
    /// 泛型异步读取
    /// </summary>
    public async Task<OperateResult<T>> ReadAsync<T>(string address, ushort length = 1)
    {
        if (DebugMode) return MockRead<T>(length);

        return typeof(T) switch
        {
            // Use sync bool read on thread pool to avoid unsupported/unstable async bit-read path.
            var t when t == typeof(bool) => (OperateResult<T>)(object)await Task.Run(() => _plc.ReadBool(address)),
            var t when t == typeof(short) => (OperateResult<T>)(object)await _plc.ReadInt16Async(address),
            var t when t == typeof(short[]) => (OperateResult<T>)(object)await _plc.ReadInt16Async(address, length),
            var t when t == typeof(ushort) => (OperateResult<T>)(object)await _plc.ReadUInt16Async(address),
            var t when t == typeof(int) => (OperateResult<T>)(object)await _plc.ReadInt32Async(address),
            var t when t == typeof(uint) => (OperateResult<T>)(object)await _plc.ReadUInt32Async(address),
            var t when t == typeof(long) => (OperateResult<T>)(object)await _plc.ReadInt64Async(address),
            var t when t == typeof(ulong) => (OperateResult<T>)(object)await _plc.ReadUInt64Async(address),
            var t when t == typeof(float) => (OperateResult<T>)(object)await _plc.ReadFloatAsync(address),
            var t when t == typeof(double) => (OperateResult<T>)(object)await _plc.ReadDoubleAsync(address),
            var t when t == typeof(string) => (OperateResult<T>)(object)await _plc.ReadStringAsync(address, length),
            var t when t == typeof(bool[]) => (OperateResult<T>)(object)await Task.Run(() => _plc.ReadBool(address, length)),
            var t when t == typeof(byte[]) => (OperateResult<T>)(object)await _plc.ReadAsync(address, length),
            _ => throw new NotSupportedException($"不支持的类型: {typeof(T)}")
        };
    }

    /// <summary>
    /// 批量读取地址，自动按最小地址到最大地址连续读取后再分发到各项
    /// </summary>
    public OperateResult<IList<PlcBatchReadItemBase>> BatchRead(IList<PlcBatchReadItemBase> batchItems)
    {
        if (batchItems is null)
            return new OperateResult<IList<PlcBatchReadItemBase>> { IsSuccess = false, Message = "batchItems 不能为空" };

        if (batchItems.Count == 0)
            return OperateResult.CreateSuccessResult(batchItems);

        if (!SplitBatchItemsByAddressType(batchItems, out var dItems, out var mBoolItems, out var splitError))
            return new OperateResult<IList<PlcBatchReadItemBase>> { IsSuccess = false, Message = splitError };

        if (dItems.Count > 0)
        {
            if (!TryBuildBatchRange(dItems, out var dStartAddress, out var dTotalLength, out var dError))
                return new OperateResult<IList<PlcBatchReadItemBase>> { IsSuccess = false, Message = dError };

            var dBlockResult = DebugMode
                ? MockRead<short[]>(dTotalLength)
                : Read<short[]>(PlcBatchAddressHelper.ToDAddress(dStartAddress), dTotalLength);

            ApplyBatchBlockResult(dItems, dBlockResult, dStartAddress);

            if (!dBlockResult.IsSuccess)
                return new OperateResult<IList<PlcBatchReadItemBase>> { IsSuccess = false, Message = dBlockResult.Message, Content = batchItems };
        }

        if (mBoolItems.Count > 0)
        {
            var mResult = BatchReadBool(mBoolItems);
            if (!mResult.IsSuccess)
                return new OperateResult<IList<PlcBatchReadItemBase>> { IsSuccess = false, Message = mResult.Message, Content = batchItems };
        }

        return OperateResult.CreateSuccessResult(batchItems);
    }

    /// <summary>
    /// 批量异步读取地址，自动按最小地址到最大地址连续读取后再分发到各项
    /// </summary>
    public async Task<OperateResult<IList<PlcBatchReadItemBase>>> BatchReadAsync(IList<PlcBatchReadItemBase> batchItems)
    {
        if (batchItems is null)
            return new OperateResult<IList<PlcBatchReadItemBase>> { IsSuccess = false, Message = "batchItems 不能为空" };

        if (batchItems.Count == 0)
            return OperateResult.CreateSuccessResult(batchItems);

        if (!SplitBatchItemsByAddressType(batchItems, out var dItems, out var mBoolItems, out var splitError))
            return new OperateResult<IList<PlcBatchReadItemBase>> { IsSuccess = false, Message = splitError };

        if (dItems.Count > 0)
        {
            if (!TryBuildBatchRange(dItems, out var dStartAddress, out var dTotalLength, out var dError))
                return new OperateResult<IList<PlcBatchReadItemBase>> { IsSuccess = false, Message = dError };

            var dBlockResult = DebugMode
                ? MockRead<short[]>(dTotalLength)
                : await ReadAsync<short[]>(PlcBatchAddressHelper.ToDAddress(dStartAddress), dTotalLength);

            ApplyBatchBlockResult(dItems, dBlockResult, dStartAddress);

            if (!dBlockResult.IsSuccess)
                return new OperateResult<IList<PlcBatchReadItemBase>> { IsSuccess = false, Message = dBlockResult.Message, Content = batchItems };
        }

        if (mBoolItems.Count > 0)
        {
            var mResult = await BatchReadBoolAsync(mBoolItems);
            if (!mResult.IsSuccess)
                return new OperateResult<IList<PlcBatchReadItemBase>> { IsSuccess = false, Message = mResult.Message, Content = batchItems };
        }

        return OperateResult.CreateSuccessResult(batchItems);
    }

    /// <summary>
    /// 批量读取 M 地址，自动按最小地址到最大地址连续读取后再分发到各项
    /// </summary>
    public OperateResult<IList<PlcBatchReadItem<bool>>> BatchReadBool(IList<PlcBatchReadItem<bool>> batchItems)
    {
        if (batchItems is null)
            return new OperateResult<IList<PlcBatchReadItem<bool>>> { IsSuccess = false, Message = "batchItems 不能为空" };

        if (batchItems.Count == 0)
            return OperateResult.CreateSuccessResult(batchItems);

        if (!TryBuildBoolBatchRange(batchItems, out var startAddress, out var totalLength, out var error))
            return new OperateResult<IList<PlcBatchReadItem<bool>>> { IsSuccess = false, Message = error };

        var blockResult = DebugMode
            ? MockRead<bool[]>(totalLength)
            : Read<bool[]>(PlcBatchAddressHelper.ToMAddress(startAddress), totalLength);

        foreach (var batchItem in batchItems)
        {
            batchItem.ApplyBoolBlockResult(blockResult, startAddress);
        }

        if (!blockResult.IsSuccess)
            return new OperateResult<IList<PlcBatchReadItem<bool>>> { IsSuccess = false, Message = blockResult.Message, Content = batchItems };

        return OperateResult.CreateSuccessResult(batchItems);
    }

    /// <summary>
    /// 批量异步读取 M 地址，自动按最小地址到最大地址连续读取后再分发到各项
    /// </summary>
    public async Task<OperateResult<IList<PlcBatchReadItem<bool>>>> BatchReadBoolAsync(IList<PlcBatchReadItem<bool>> batchItems)
    {
        if (batchItems is null)
            return new OperateResult<IList<PlcBatchReadItem<bool>>> { IsSuccess = false, Message = "batchItems 不能为空" };

        if (batchItems.Count == 0)
            return OperateResult.CreateSuccessResult(batchItems);

        if (!TryBuildBoolBatchRange(batchItems, out var startAddress, out var totalLength, out var error))
            return new OperateResult<IList<PlcBatchReadItem<bool>>> { IsSuccess = false, Message = error };

        var blockResult = DebugMode
            ? MockRead<bool[]>(totalLength)
            : await ReadAsync<bool[]>(PlcBatchAddressHelper.ToMAddress(startAddress), totalLength);

        foreach (var batchItem in batchItems)
        {
            batchItem.ApplyBoolBlockResult(blockResult, startAddress);
        }

        if (!blockResult.IsSuccess)
            return new OperateResult<IList<PlcBatchReadItem<bool>>> { IsSuccess = false, Message = blockResult.Message, Content = batchItems };

        return OperateResult.CreateSuccessResult(batchItems);
    }

    private static void ApplyBatchBlockResult(IList<PlcBatchReadItemBase> batchItems, OperateResult<short[]> blockResult, int startAddress)
    {
        foreach (var batchItem in batchItems)
        {
            batchItem.ApplyBlockResult(blockResult, startAddress);
        }
    }

    private static bool TryBuildBatchRange(
        IList<PlcBatchReadItemBase> batchItems,
        out int startAddress,
        out ushort totalLength,
        out string error)
    {
        startAddress = 0;
        totalLength = 0;
        error = string.Empty;

        var hasAny = false;
        var minAddress = int.MaxValue;
        var maxAddressExclusive = int.MinValue;

        foreach (var item in batchItems)
        {
            if (item.WordLength == 0)
            {
                error = $"地址 {item.Address} 的读取长度不能为 0";
                return false;
            }

            if (!PlcBatchAddressHelper.TryParseDAddress(item.Address, out var addressNumber))
            {
                error = $"仅支持 D 地址批量读取，当前地址无效: {item.Address}";
                return false;
            }

            hasAny = true;
            if (addressNumber < minAddress) minAddress = addressNumber;

            var endExclusive = addressNumber + item.WordLength;
            if (endExclusive > maxAddressExclusive) maxAddressExclusive = endExclusive;
        }

        if (!hasAny)
        {
            error = "batchItems 不能为空";
            return false;
        }

        var length = maxAddressExclusive - minAddress;
        if (length <= 0)
        {
            error = "批量读取长度无效";
            return false;
        }

        if (length > ushort.MaxValue)
        {
            error = $"批量读取长度超限: {length}";
            return false;
        }

        startAddress = minAddress;
        totalLength = (ushort)length;
        return true;
    }

    private static bool TryBuildBoolBatchRange(
        IList<PlcBatchReadItem<bool>> batchItems,
        out int startAddress,
        out ushort totalLength,
        out string error)
    {
        startAddress = 0;
        totalLength = 0;
        error = string.Empty;

        var hasAny = false;
        var minAddress = int.MaxValue;
        var maxAddressInclusive = int.MinValue;

        foreach (var item in batchItems)
        {
            if (item.AddressType == PlcBatchAddressType.D)
            {
                error = $"地址 {item.Address} 被标记为 D，不能用于 M 批量读取";
                return false;
            }

            if (!PlcBatchAddressHelper.TryParseMAddress(item.Address, out var addressNumber))
            {
                error = $"仅支持 M 地址批量读取，当前地址无效: {item.Address}";
                return false;
            }

            hasAny = true;
            if (addressNumber < minAddress) minAddress = addressNumber;
            if (addressNumber > maxAddressInclusive) maxAddressInclusive = addressNumber;
        }

        if (!hasAny)
        {
            error = "batchItems 不能为空";
            return false;
        }

        var length = maxAddressInclusive - minAddress + 1;
        if (length <= 0)
        {
            error = "批量读取长度无效";
            return false;
        }

        if (length > ushort.MaxValue)
        {
            error = $"批量读取长度超限: {length}";
            return false;
        }

        startAddress = minAddress;
        totalLength = (ushort)length;
        return true;
    }

    private static bool SplitBatchItemsByAddressType(
        IList<PlcBatchReadItemBase> batchItems,
        out List<PlcBatchReadItemBase> dItems,
        out List<PlcBatchReadItem<bool>> mBoolItems,
        out string error)
    {
        dItems = [];
        mBoolItems = [];
        error = string.Empty;

        foreach (var item in batchItems)
        {
            var addressType = ResolveAddressType(item);

            if (addressType == PlcBatchAddressType.D)
            {
                dItems.Add(item);
                continue;
            }

            if (addressType == PlcBatchAddressType.M)
            {
                if (item is PlcBatchReadItem<bool> boolItem)
                {
                    mBoolItems.Add(boolItem);
                    continue;
                }

                error = $"M 地址仅支持 bool 类型批量读取，当前地址: {item.Address}";
                return false;
            }

            error = $"无法识别地址类型: {item.Address}";
            return false;
        }

        return true;
    }

    private static PlcBatchAddressType ResolveAddressType(PlcBatchReadItemBase item)
    {
        if (item.AddressType != PlcBatchAddressType.Auto)
        {
            return item.AddressType;
        }

        if (PlcBatchAddressHelper.TryParseDAddress(item.Address, out _))
        {
            return PlcBatchAddressType.D;
        }

        if (PlcBatchAddressHelper.TryParseMAddress(item.Address, out _))
        {
            return PlcBatchAddressType.M;
        }

        return PlcBatchAddressType.Auto;
    }

    /// <summary>
    /// 泛型写入
    /// </summary>
    public OperateResult Write<T>(string address, T value)
    {
        if (DebugMode) return MockWrite();

        if (value is null)
            return new OperateResult { IsSuccess = false, Message = "值不能为空" };
        return _plc.Write(address, (dynamic)value);
    }

    /// <summary>
    /// 泛型异步写入
    /// </summary>
    public async Task<OperateResult> WriteAsync<T>(string address, T value)
    {
        if (DebugMode) return MockWrite();

        if (value is null)
            return new OperateResult { IsSuccess = false, Message = "值不能为空" };
        return await _plc.WriteAsync(address, (dynamic)value);
    }

    #endregion

    #region 便捷方法

    public OperateResult SetBit(string address)
    {
        if (DebugMode) return MockWrite();
        return _plc.Write(address, true);
    }

    public OperateResult ResetBit(string address)
    {
        if (DebugMode) return MockWrite();
        return _plc.Write(address, false);
    }

    public async Task<bool> PulseAsync(string address, int delayMs = 100)
    {
        if (DebugMode) return true;
        if (!SetBit(address).IsSuccess) return false;
        await Task.Delay(delayMs);
        return ResetBit(address).IsSuccess;
    }

    #endregion

    #region 等待方法

    /// <summary>
    /// 等待指定地址的值变为期望值
    /// </summary>
    /// <param name="address">PLC 地址</param>
    /// <param name="waitValue">期望的值</param>
    /// <param name="readInterval">轮询间隔（毫秒），默认 100ms</param>
    /// <param name="waitTimeout">超时时间（毫秒），-1 表示无限等待</param>
    /// <returns>等待耗时</returns>
    public OperateResult<TimeSpan> Wait<T>(string address, T waitValue, int readInterval = 100, int waitTimeout = -1)
    {
        if (DebugMode)
            return OperateResult.CreateSuccessResult(TimeSpan.FromMilliseconds(readInterval));

        var sw = System.Diagnostics.Stopwatch.StartNew();

        while (true)
        {
            var result = Read<T>(address);
            if (!result.IsSuccess)
                return new OperateResult<TimeSpan> { IsSuccess = false, Message = result.Message };

            if (EqualityComparer<T>.Default.Equals(result.Content, waitValue))
            {
                sw.Stop();
                return OperateResult.CreateSuccessResult(sw.Elapsed);
            }

            if (waitTimeout > 0 && sw.ElapsedMilliseconds >= waitTimeout)
            {
                sw.Stop();
                return new OperateResult<TimeSpan>
                {
                    IsSuccess = false,
                    Message = $"等待超时: {address} 未在 {waitTimeout}ms 内变为 {waitValue}",
                    Content = sw.Elapsed
                };
            }

            Thread.Sleep(readInterval);
        }
    }

    /// <summary>
    /// 异步等待指定地址的值变为期望值
    /// </summary>
    public async Task<OperateResult<TimeSpan>> WaitAsync<T>(string address, T waitValue, int readInterval = 100, int waitTimeout = -1, CancellationToken cancellationToken = default)
    {
        if (DebugMode)
            return OperateResult.CreateSuccessResult(TimeSpan.FromMilliseconds(readInterval));

        var sw = System.Diagnostics.Stopwatch.StartNew();

        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await ReadAsync<T>(address);
            if (!result.IsSuccess)
                return new OperateResult<TimeSpan> { IsSuccess = false, Message = result.Message };

            if (EqualityComparer<T>.Default.Equals(result.Content, waitValue))
            {
                sw.Stop();
                return OperateResult.CreateSuccessResult(sw.Elapsed);
            }

            if (waitTimeout > 0 && sw.ElapsedMilliseconds >= waitTimeout)
            {
                sw.Stop();
                return new OperateResult<TimeSpan>
                {
                    IsSuccess = false,
                    Message = $"等待超时: {address} 未在 {waitTimeout}ms 内变为 {waitValue}",
                    Content = sw.Elapsed
                };
            }

            await Task.Delay(readInterval, cancellationToken);
        }

        sw.Stop();
        return new OperateResult<TimeSpan> { IsSuccess = false, Message = "等待被取消", Content = sw.Elapsed };
    }

    #endregion

    #region D寄存器位操作 (0-15)

    /// <summary>
    /// 读取 D 寄存器的某一位 (0-15)
    /// </summary>
    public OperateResult<bool> ReadDBit(string address, int bitIndex)
    {
        if (DebugMode) return OperateResult.CreateSuccessResult(_random.Next(2) == 1);

        if (bitIndex is < 0 or > 15)
            return new OperateResult<bool> { IsSuccess = false, Message = "位索引必须在 0-15 之间" };

        var result = Read<short>(address);
        if (!result.IsSuccess)
            return new OperateResult<bool> { IsSuccess = false, Message = result.Message };
        
        return OperateResult.CreateSuccessResult(result.Content.GetBoolByIndex(bitIndex));
    }

    /// <summary>
    /// 异步读取 D 寄存器的某一位 (0-15)
    /// </summary>
    public async Task<OperateResult<bool>> ReadDBitAsync(string address, int bitIndex)
    {
        if (DebugMode) return OperateResult.CreateSuccessResult(_random.Next(2) == 1);

        if (bitIndex is < 0 or > 15)
            return new OperateResult<bool> { IsSuccess = false, Message = "位索引必须在 0-15 之间" };

        var result = await ReadAsync<short>(address);
        if (!result.IsSuccess)
            return new OperateResult<bool> { IsSuccess = false, Message = result.Message };

        return OperateResult.CreateSuccessResult(result.Content.GetBoolByIndex(bitIndex));
    }

    /// <summary>
    /// 写入 D 寄存器的某一位 (0-15)
    /// </summary>
    public OperateResult WriteDBit(string address, int bitIndex, bool value)
    {
        if (DebugMode) return MockWrite();

        if (bitIndex is < 0 or > 15)
            return new OperateResult { IsSuccess = false, Message = "位索引必须在 0-15 之间" };

        var readResult = Read<short>(address);
        if (!readResult.IsSuccess) return readResult;

        return Write(address, readResult.Content.SetBoolByIndex(bitIndex, value));
    }

    /// <summary>
    /// 异步写入 D 寄存器的某一位 (0-15)
    /// </summary>
    public async Task<OperateResult> WriteDBitAsync(string address, int bitIndex, bool value)
    {
        if (DebugMode) return MockWrite();

        if (bitIndex is < 0 or > 15)
            return new OperateResult { IsSuccess = false, Message = "位索引必须在 0-15 之间" };

        var readResult = await ReadAsync<short>(address);
        if (!readResult.IsSuccess) return readResult;

        return await WriteAsync(address, readResult.Content.SetBoolByIndex(bitIndex, value));
    }

    // DBitAddress 重载
    public OperateResult<bool> ReadDBit(DBitAddress dBit) => ReadDBit(dBit.Address, dBit.BitIndex);
    public Task<OperateResult<bool>> ReadDBitAsync(DBitAddress dBit) => ReadDBitAsync(dBit.Address, dBit.BitIndex);
    public OperateResult WriteDBit(DBitAddress dBit, bool value) => WriteDBit(dBit.Address, dBit.BitIndex, value);
    public Task<OperateResult> WriteDBitAsync(DBitAddress dBit, bool value) => WriteDBitAsync(dBit.Address, dBit.BitIndex, value);

    // 置位/复位便捷方法
    public OperateResult SetDBit(string address, int bitIndex) => WriteDBit(address, bitIndex, true);
    public OperateResult SetDBit(DBitAddress dBit) => WriteDBit(dBit, true);
    public Task<OperateResult> SetDBitAsync(string address, int bitIndex) => WriteDBitAsync(address, bitIndex, true);
    public Task<OperateResult> SetDBitAsync(DBitAddress dBit) => WriteDBitAsync(dBit, true);
    public OperateResult ResetDBit(string address, int bitIndex) => WriteDBit(address, bitIndex, false);
    public OperateResult ResetDBit(DBitAddress dBit) => WriteDBit(dBit, false);
    public Task<OperateResult> ResetDBitAsync(string address, int bitIndex) => WriteDBitAsync(address, bitIndex, false);
    public Task<OperateResult> ResetDBitAsync(DBitAddress dBit) => WriteDBitAsync(dBit, false);

    #endregion

    #region 历史 McBinary 风格 API（统一走本类 _plc / DebugMode）

    /// <summary>与旧 McBinaryPlcComm.IsPLCConnected 一致。</summary>
    public bool IsPLCConnected => IsConnected;

    public bool ConnectPlc(string ipAddress, int port) => Connect(ipAddress, port).IsSuccess;

    public void CloseConnect() => Disconnect();

    public static string ReadResultRender<T>(OperateResult<T> result, string address)
    {
        if (result.IsSuccess)
            return DateTime.Now.ToString("[HH:mm:ss] ") + $"[{address}] {result.Content}{Environment.NewLine}";
        return DateTime.Now.ToString("[HH:mm:ss] ") + $"[{address}] Read Failed {Environment.NewLine}Reason：{result.ToMessageShowString()}";
    }

    public static string WriteResultRender(Func<OperateResult> write, string address)
    {
        try
        {
            OperateResult result = write();
            if (result.IsSuccess)
                return DateTime.Now.ToString("[HH:mm:ss] ") + $"[{address}] Write Success";
            return DateTime.Now.ToString("[HH:mm:ss] ") + $"[{address}] Write Failed {Environment.NewLine} Reason：{result.ToMessageShowString()}";
        }
        catch (Exception ex)
        {
            return "Data for writting is not corrent: " + ex.Message;
        }
    }

    public static string BulkReadRenderResult(IReadWriteNet readWrite, string addressValue, string lengthValue)
    {
        try
        {
            OperateResult<byte[]> read = readWrite.Read(addressValue, ushort.Parse(lengthValue));
            if (read.IsSuccess)
                return "Result：" + HslCommunication.BasicFramework.SoftBasic.ByteToHexString(read.Content);
            return "Read Failed：" + read.ToMessageShowString();
        }
        catch (Exception ex)
        {
            return "Read Failed：" + ex.Message;
        }
    }

    public OperateResult WriteByteArray(string address, byte[] data)
    {
        if (DebugMode) return MockWrite();
        return _plc.Write(address, data);
    }

    public OperateResult WriteUShortArray(string address, ushort[] data)
    {
        if (DebugMode) return MockWrite();
        return _plc.Write(address, data);
    }

    public byte[]? ReadBytes(string addressValue, int lengthValue)
    {
        var read = Read<byte[]>(addressValue, (ushort)lengthValue);
        return read.IsSuccess ? read.Content : null;
    }

    public OperateResult<bool[]> ReadBoolContinuous(string startAddress, ushort length) =>
        DebugMode ? MockRead<bool[]>(length) : _plc.ReadBool(startAddress, length);

    public OperateResult<short[]> ReadInt16Continuous(string startAddress, ushort length) =>
        DebugMode ? MockRead<short[]>(length) : _plc.ReadInt16(startAddress, length);

    public OperateResult<byte[]> ReadRawContinuous(string startAddress, ushort length) =>
        DebugMode ? MockRead<byte[]>(length) : _plc.Read(startAddress, length);

    public OperateResult WriteInt16Continuous(string startAddress, short[] values)
    {
        if (DebugMode) return MockWrite();
        return _plc.Write(startAddress, values);
    }

    public Task<OperateResult<bool[]>> ReadBoolContinuousAsync(string startAddress, ushort length) =>
        DebugMode ? Task.FromResult(MockRead<bool[]>(length)) : _plc.ReadBoolAsync(startAddress, length);

    public Task<OperateResult<short[]>> ReadInt16ContinuousAsync(string startAddress, ushort length) =>
        DebugMode ? Task.FromResult(MockRead<short[]>(length)) : _plc.ReadInt16Async(startAddress, length);

    public Task<OperateResult<byte[]>> ReadRawContinuousAsync(string startAddress, ushort length) =>
        DebugMode ? Task.FromResult(MockRead<byte[]>(length)) : _plc.ReadAsync(startAddress, length);

    public Task<OperateResult> WriteInt16ContinuousAsync(string startAddress, short[] values) =>
        DebugMode ? Task.FromResult(MockWrite()) : _plc.WriteAsync(startAddress, values);

    public bool ReadBoolVari(string address) => Read<bool>(address).Content;

    public short ReadShortVari(string address) => Read<short>(address).Content;

    public OperateResult<ushort> ReadUShortVari(string address) =>
        DebugMode ? OperateResult.CreateSuccessResult((ushort)_random.Next(0, 200)) : _plc.ReadUInt16(address);

    public OperateResult<int> ReadIntVari(string address) =>
        DebugMode ? OperateResult.CreateSuccessResult(_random.Next(-1000, 1000)) : _plc.ReadInt32(address);

    public OperateResult<uint> ReadUIntVari(string address) =>
        DebugMode ? OperateResult.CreateSuccessResult((uint)_random.Next(0, 2000)) : _plc.ReadUInt32(address);

    public OperateResult<long> ReadLongVari(string address) =>
        DebugMode ? OperateResult.CreateSuccessResult((long)_random.Next(-10000, 10000)) : _plc.ReadInt64(address);

    public OperateResult<ulong> ReadULongVari(string address) =>
        DebugMode ? OperateResult.CreateSuccessResult((ulong)_random.Next(0, 20000)) : _plc.ReadUInt64(address);

    public OperateResult<float> ReadFloatVari(string address) =>
        DebugMode ? OperateResult.CreateSuccessResult((float)(_random.NextDouble() * 200 - 100)) : _plc.ReadFloat(address);

    public OperateResult<double> ReadDoubleVari(string address) =>
        DebugMode ? OperateResult.CreateSuccessResult(_random.NextDouble() * 200 - 100) : _plc.ReadDouble(address);

    public OperateResult<string> ReadStringVari(string address, ushort length) =>
        DebugMode ? OperateResult.CreateSuccessResult("MOCK") : _plc.ReadString(address, length);

    public OperateResult WriteBoolVari(string address, bool writebool) => Write(address, writebool);

    public OperateResult WriteBoolVariContinuous(string startAddress, bool[] writebool)
    {
        if (DebugMode) return MockWrite();
        return _plc.Write(startAddress, writebool);
    }

    public OperateResult WriteShortVari(string address, short writeShort) => Write(address, writeShort);

    public OperateResult WriteUShortVari(string address, ushort writeUShort)
    {
        if (DebugMode) return MockWrite();
        return _plc.Write(address, writeUShort);
    }

    public OperateResult WriteUShortVari(string address, string writeUShort)
    {
        if (DebugMode) return MockWrite();
        return _plc.Write(address, writeUShort);
    }

    public OperateResult WriteIntVari(string address, int writeInt) => Write(address, writeInt);

    public OperateResult WriteUIntVari(string address, uint writeUInt) => Write(address, writeUInt);

    public OperateResult WriteLongVari(string address, long writeLong) => Write(address, writeLong);

    public OperateResult WriteULongVari(string address, ulong writeULong) => Write(address, writeULong);

    public OperateResult WriteFloatVari(string address, float writeFloat) => Write(address, writeFloat);

    public OperateResult WriteDoubleVari(string address, double writeDouble) => Write(address, writeDouble);

    public OperateResult WriteStringVari(string address, string writeString, int length = 10)
    {
        if (DebugMode) return MockWrite();
        return _plc.Write(address, writeString, length);
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_disposed) return;
        Disconnect();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~MitsubishiPlcService() => Dispose();

    #endregion
}
