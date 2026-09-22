using CMP.Extensions;
using CMP.Models.Operation.LoadPort;
using CMP.Models.Warn;
using CMP.Services.LoadPort;
using CMP.Services.Log;
using CMP.Services.Warn;
using Prism.Events;
using System.Windows;

namespace CMP.Services.Rfid;

public sealed class RfidReaderService : IRfidReaderService, IDisposable
{
    private const int AlarmPort = 800;
    private const int AlarmTimeout = 801;
    private const int AlarmFormat = 802;
    private const int AlarmComm = 803;
    private const int AlarmNoTag = 804;
    private const int AlarmIdSystem = 805;
    private const int AlarmEmpty = 806;
    private const int AlarmPlace = 807;

    private readonly IEventAggregator _eventAggregator;
    private readonly object _gate = new();
    private readonly Dictionary<int, ReaderContext> _readers = [];
    private readonly LoggerInstance _log = Logger.For("RFID");

    private RfidConfig? _config;
    private bool _started;
    private bool _disposed;
    private SubscriptionToken? _statusSubscription;

    public RfidReaderService(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
    }

    public bool IsStarted
    {
        get
        {
            lock (_gate)
            {
                return _started;
            }
        }
    }

    public void Start()
    {
        lock (_gate)
        {
            if (_disposed || _started)
            {
                return;
            }

            RfidConfig config;
            try
            {
                config = RfidConfig.Load();
            }
            catch (Exception ex)
            {
                _log.Error($"加载 RFID 配置失败: {ex.Message}", ex);
                return;
            }

            if (!config.Enabled)
            {
                _log.Info("RFID 已在配置中关闭。");
                return;
            }

            _config = config;
            foreach (var binding in config.Readers)
            {
                if (string.IsNullOrWhiteSpace(binding.RfidPort))
                {
                    continue;
                }

                var context = new ReaderContext(binding);
                _readers[binding.LoadportId] = context;
                TryOpenReader(context, config);
            }

            _statusSubscription = _eventAggregator
                .GetEvent<LoadPortStatusChangedEvent>()
                .Subscribe(OnLoadPortStatusChanged, ThreadOption.BackgroundThread, true);

            _started = true;
        }

        ScanExistingPlacement();
        _log.Info($"RFID 服务已启动，读者数 {_readers.Count}。");
    }

    public void Stop()
    {
        lock (_gate)
        {
            if (!_started)
            {
                DisposeReaders();
                return;
            }

            if (_statusSubscription != null)
            {
                _eventAggregator.GetEvent<LoadPortStatusChangedEvent>().Unsubscribe(_statusSubscription);
                _statusSubscription = null;
            }

            DisposeReaders();
            _started = false;
        }

        _log.Info("RFID 服务已停止。");
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }

        Stop();
    }

    private void ScanExistingPlacement()
    {
        foreach (var loadPort in LoadPortManager.Instance.GetAllLoadPorts().Values)
        {
            var status = loadPort.LoadPortStatus?.foupPlacementStatus ?? FoupPlacementStatus.NoExistence;
            HandlePlacement(loadPort, status);
        }
    }

    private void OnLoadPortStatusChanged(LoadPortStatusChangedEventArgs args)
    {
        try
        {
            var loadPort = LoadPortManager.Instance.GetLoadPort(args.PortName)
                           ?? LoadPortManager.Instance.GetLoadPort(args.LoadportId);
            if (loadPort == null)
            {
                return;
            }

            var placement = args.HardwareStatus?.foupPlacementStatus
                            ?? loadPort.LoadPortStatus?.foupPlacementStatus
                            ?? FoupPlacementStatus.NoExistence;
            HandlePlacement(loadPort, placement);
        }
        catch (Exception ex)
        {
            _log.Error($"处理 LoadPort 状态变化失败: {ex.Message}", ex);
        }
    }

    private void HandlePlacement(LoadPortData loadPort, FoupPlacementStatus placement)
    {
        ReaderContext? context;
        lock (_gate)
        {
            if (!_started || !_readers.TryGetValue(loadPort.LoadportId, out context))
            {
                return;
            }
        }

        lock (context.StateLock)
        {
            if (placement == FoupPlacementStatus.NoExistence)
            {
                context.HasReadThisPlacement = false;
                context.Phase = ReaderPhase.Idle;
                SetCarrierId(loadPort, string.Empty, RfidReadStatus.Idle);
                return;
            }

            if (placement == FoupPlacementStatus.Error)
            {
                if (context.Phase == ReaderPhase.Idle)
                {
                    context.Phase = ReaderPhase.Failed;
                    RaiseAlarm(
                        AlarmPlace,
                        "RFID_PLACE",
                        $"LP{loadPort.DisplayNumber} FOUP 放置错误，未读取 RFID。端口 {context.Binding.RfidPort}",
                        loadPort.DisplayNumber);
                }

                return;
            }

            if (placement != FoupPlacementStatus.Normal
                || context.HasReadThisPlacement
                || context.Phase == ReaderPhase.Reading)
            {
                return;
            }

            context.HasReadThisPlacement = true;
            context.Phase = ReaderPhase.Reading;
        }

        SetCarrierId(loadPort, loadPort.CarrierId, RfidReadStatus.Reading);
        _ = Task.Run(() => ReadOnce(loadPort, context));
    }

    private void ReadOnce(LoadPortData loadPort, ReaderContext context)
    {
        var config = _config;
        if (config == null)
        {
            return;
        }

        try
        {
            if (context.Client == null || !context.Client.IsOpen)
            {
                TryOpenReader(context, config);
            }

            if (context.Client == null || !context.Client.IsOpen)
            {
                FinishRead(loadPort, context, failed: true);
                RaiseAlarm(
                    AlarmPort,
                    "RFID_PORT",
                    $"LP{loadPort.DisplayNumber} RFID 串口未打开: {context.Binding.RfidPort}",
                    loadPort.DisplayNumber);
                return;
            }

            string line;
            lock (context.IoLock)
            {
                var command = V640OneToOneProtocol.BuildReadCommand(config.PageMask);
                _log.Info($"LP{loadPort.DisplayNumber} 发送 READ {command} @ {context.Binding.RfidPort}");
                line = context.Client.Transact(command, config.ReadTimeoutMs);
            }
            var response = V640OneToOneProtocol.Parse(line);
            if (!response.IsSuccess)
            {
                FinishRead(loadPort, context, failed: true);
                RaiseMappedResponseAlarm(loadPort, context, response.Code);
                return;
            }

            var carrierId = V640OneToOneProtocol.ParseCarrierId(response.PayloadHex, config.CarrierIdByteLength);
            if (string.IsNullOrWhiteSpace(carrierId))
            {
                FinishRead(loadPort, context, failed: true);
                RaiseAlarm(
                    AlarmEmpty,
                    "RFID_EMPTY",
                    $"LP{loadPort.DisplayNumber} RFID CarrierId 为空。端口 {context.Binding.RfidPort}",
                    loadPort.DisplayNumber);
                return;
            }

            FinishRead(loadPort, context, failed: false, carrierId);
            _log.Info($"LP{loadPort.DisplayNumber} CarrierId={carrierId}");
        }
        catch (TimeoutException ex)
        {
            FinishRead(loadPort, context, failed: true);
            RaiseAlarm(
                AlarmTimeout,
                "RFID_TIMEOUT",
                $"LP{loadPort.DisplayNumber} RFID 读取超时。端口 {context.Binding.RfidPort}。{ex.Message}",
                loadPort.DisplayNumber);
        }
        catch (Exception ex)
        {
            FinishRead(loadPort, context, failed: true);
            RaiseAlarm(
                AlarmComm,
                "70",
                $"LP{loadPort.DisplayNumber} RFID 通信失败。端口 {context.Binding.RfidPort}。{ex.Message}",
                loadPort.DisplayNumber);
            _log.Error($"LP{loadPort.DisplayNumber} RFID 读取异常: {ex.Message}", ex);
        }
    }

    private void FinishRead(LoadPortData loadPort, ReaderContext context, bool failed, string? carrierId = null)
    {
        lock (context.StateLock)
        {
            context.Phase = failed ? ReaderPhase.Failed : ReaderPhase.Bound;
        }

        SetCarrierId(loadPort, failed ? string.Empty : carrierId ?? string.Empty,
            failed ? RfidReadStatus.Failed : RfidReadStatus.Ok);
    }

    private void TryOpenReader(ReaderContext context, RfidConfig config)
    {
        lock (context.IoLock)
        {
            try
            {
                context.Client ??= new RfidSerialClient(context.Binding.RfidPort, config);
                context.Client.Open();
                if (config.ProbeOnConnect)
                {
                    var probe = context.Client.Transact(V640OneToOneProtocol.BuildTestCommand(), config.ReadTimeoutMs);
                    var parsed = V640OneToOneProtocol.Parse(probe);
                    if (!parsed.IsSuccess)
                    {
                        RaiseAlarm(
                            AlarmPort,
                            "RFID_PORT",
                            $"LP{context.Binding.LoadportId + 1} RFID TEST 失败，响应 {parsed.Code}。端口 {context.Binding.RfidPort}",
                            context.Binding.LoadportId + 1);
                    }
                }

                _log.Info($"RFID 串口已打开: {context.Binding.RfidPort} -> {context.Binding.LoadPortCom}");
            }
            catch (Exception ex)
            {
                RaiseAlarm(
                    AlarmPort,
                    "RFID_PORT",
                    $"LP{context.Binding.LoadportId + 1} 打开 RFID 串口失败: {context.Binding.RfidPort}。{ex.Message}",
                    context.Binding.LoadportId + 1);
                _log.Error($"打开 RFID 串口 {context.Binding.RfidPort} 失败: {ex.Message}", ex);
            }
        }
    }

    private static void SetCarrierId(LoadPortData loadPort, string carrierId, RfidReadStatus status)
    {
        void Apply()
        {
            loadPort.RfidReadStatus = status;
            loadPort.CarrierId = carrierId;
            loadPort.RefreshControlStatusItems();
        }

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.CheckAccess())
        {
            Apply();
        }
        else
        {
            dispatcher.BeginInvoke(Apply);
        }
    }

    private void RaiseMappedResponseAlarm(LoadPortData loadPort, ReaderContext context, string code)
    {
        var (alarmId, name, meaning) = code switch
        {
            V640OneToOneProtocol.FormatErrorCode => (AlarmFormat, "14", "格式错误"),
            V640OneToOneProtocol.CommunicationErrorCode => (AlarmComm, "70", "通信噪声/失败"),
            V640OneToOneProtocol.NoTagCode => (AlarmNoTag, "72", "无标签"),
            V640OneToOneProtocol.WriteProtectCode => (AlarmFormat, "7B", "外部写入区域错误"),
            V640OneToOneProtocol.IdSystemError1Code => (AlarmIdSystem, "7E", "ID 系统错误"),
            V640OneToOneProtocol.IdSystemError2Code => (AlarmIdSystem, "7F", "ID 系统错误"),
            _ => (AlarmComm, string.IsNullOrWhiteSpace(code) ? "RFID_COMM" : code, "未知响应")
        };

        RaiseAlarm(
            alarmId,
            name,
            $"LP{loadPort.DisplayNumber} RFID {meaning}（{name}）。端口 {context.Binding.RfidPort}",
            loadPort.DisplayNumber);
    }

    private void RaiseAlarm(int alarmId, string code, string description, int displayNumber)
    {
        _log.Warn(description);
        var alarm = new Alarm
        {
            AlarmId = alarmId,
            AlarmCode = code,
            AlarmName = code,
            Description = description,
            AlarmTime = DateTime.Now,
            Type = "RFID",
            IsAlarmed = true,
            Unit = Unit.None,
            Comment = $"LP{displayNumber}"
        };

        try
        {
            new AlarmService().AddAlarm(alarm);
        }
        catch (Exception ex)
        {
            _log.Error($"写入 RFID Alarm 失败: {ex.Message}", ex);
        }

        void Insert()
        {
            try
            {
                GlobalService.DataCenter.listAlarms.Insert(0, alarm);
            }
            catch (Exception ex)
            {
                _log.Error($"插入 RFID Alarm 列表失败: {ex.Message}", ex);
            }
        }

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.CheckAccess())
        {
            Insert();
        }
        else
        {
            dispatcher.BeginInvoke(Insert);
        }
    }

    private void DisposeReaders()
    {
        foreach (var context in _readers.Values)
        {
            try
            {
                context.Client?.Dispose();
            }
            catch
            {
                // ignore close errors
            }

            context.Client = null;
        }

        _readers.Clear();
    }

    private enum ReaderPhase
    {
        Idle,
        Reading,
        Bound,
        Failed
    }

    private sealed class ReaderContext
    {
        public ReaderContext(RfidReaderBinding binding)
        {
            Binding = binding;
        }

        public RfidReaderBinding Binding { get; }
        public RfidSerialClient? Client { get; set; }
        public object IoLock { get; } = new();
        public object StateLock { get; } = new();
        public bool HasReadThisPlacement { get; set; }
        public ReaderPhase Phase { get; set; } = ReaderPhase.Idle;
    }
}
