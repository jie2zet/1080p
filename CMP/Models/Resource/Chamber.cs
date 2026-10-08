using CMP.Models.kvsPlc;
using CMP.Models.DataCenter;
using CMP.Services.Log;
using Dm.util;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using HandyControl.Controls;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using DRYA = CMP.Models.Resource.GlobalChambers.DryACommandAndHandle;
using DRYB = CMP.Models.Resource.GlobalChambers.DryBCommandAndHandle;
using HANDLE = CMP.Constants.Plc.Handle;
using KHL = CMP.Models.kvsPlc.KvHostLink;
using KST = CMP.Models.kvsPlc.Polish.KvStruct;
using PENA1 = CMP.Models.Resource.GlobalChambers.PenA1CommandAndHandle;
using PENA2 = CMP.Models.Resource.GlobalChambers.PenA2CommandAndHandle;
using PENB1 = CMP.Models.Resource.GlobalChambers.PenB1CommandAndHandle;
using PENB2 = CMP.Models.Resource.GlobalChambers.PenB2CommandAndHandle;
using POLISH = CMP.Models.Resource.GlobalChambers.PolishCommandAndHandle;
using ROLLERA1 = CMP.Models.Resource.GlobalChambers.RollerA1CommandAndHandle;
using ROLLERA2 = CMP.Models.Resource.GlobalChambers.RollerA2CommandAndHandle;
using ROLLERA3 = CMP.Models.Resource.GlobalChambers.RollerA3CommandAndHandle;
using ROLLERB1 = CMP.Models.Resource.GlobalChambers.RollerB1CommandAndHandle;
using ROLLERB2 = CMP.Models.Resource.GlobalChambers.RollerB2CommandAndHandle;
using ROLLERB3 = CMP.Models.Resource.GlobalChambers.RollerB3CommandAndHandle;


namespace CMP.Models.Resource;

public class Chamber : BindableBase
{

    private CollectDataChamber _collectDataChamber;
    public CollectDataChamber CollectDataChamber
    {
        get => _collectDataChamber;
        set => SetProperty(ref _collectDataChamber, value);
    }
    private ChamberStatus _chamberStatus;
    private ChamberStatus _preChamberStatus;
    private string _name;
    private string _headColor = "#2C66A7";
    private KVLink _kvLink;
    private int _err;

    public bool IsEnabled { get; }

    private string _startPermitionVarName;
    private string _readyVarName;
    private string _autoStartVarName;
    private string _resetErrorVarName;

    private string _initialStatusVarName;

    private string _alarmStatusVarName;

    private uint _initCommandValue;
    private uint _autoModeCommandValue;
    private uint _manualModeCommandValue;
    private uint _abortCommandValue;

    private uint _commandHandle;
    private uint _autoModeHandle;

    private byte[] _writeBuf = new byte[2048];
    private byte[] _readBuf = new byte[2048];

    private Thread _thMonitorStatus;
    private Thread _thMonitorLogger;
    
    private bool _isResetErrorNeedClear;
    private int _resetErrorDelayCycle;
    private bool _isAutoRunNeedClear;
    private int _resetAutoRunCycle;
    private int _mainStatusReadCycle;
    private int _polishLogSession;
    private static readonly TimeSpan WarnLogInterval = TimeSpan.FromSeconds(5);
    private const int ExsituStopDebounceMilliseconds = 2000;
    private const int WaferNameRetryIntervalMilliseconds = 1000;
    private DateTime _nextMainStepWarnLogUtc = DateTime.MinValue;
    private DateTime _nextMain2StepWarnLogUtc = DateTime.MinValue;
    private DateTime _nextLogStatusWarnLogUtc = DateTime.MinValue;

    private DataLogger _dataLogger;

    private CollectDataChamber? _processDisplaySource;
    private string _processDisplayStationPrefix = string.Empty;
    private string _recipeTimeDisplay = "Recipe Time: --";
    private string _processStepDisplay = "Step: --";

    public string RecipeTimeDisplay
    {
        get => _recipeTimeDisplay;
        private set => SetProperty(ref _recipeTimeDisplay, value);
    }

    public string ProcessStepDisplay
    {
        get => _processStepDisplay;
        private set => SetProperty(ref _processStepDisplay, value);
    }

    internal void ConfigureProcessDataDisplay(CollectDataChamber source, string stationPrefix = "")
    {
        ArgumentNullException.ThrowIfNull(source);

        if (_processDisplaySource != null)
        {
            _processDisplaySource.ProcessDataUpdated -= OnProcessDataUpdated;
        }

        _processDisplaySource = source;
        _processDisplayStationPrefix = stationPrefix;
        _processDisplaySource.ProcessDataUpdated += OnProcessDataUpdated;
        UpdateProcessDataDisplay();
    }

    private void OnProcessDataUpdated(object? sender, EventArgs e)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher != null && !dispatcher.CheckAccess())
        {
            dispatcher.BeginInvoke(DispatcherPriority.DataBind, UpdateProcessDataDisplay);
            return;
        }

        UpdateProcessDataDisplay();
    }

    private void UpdateProcessDataDisplay()
    {
        if (_processDisplaySource == null
            || !_processDisplaySource.TryGetProcessDataDisplay(
                _processDisplayStationPrefix,
                out var processStepAct,
                out var recipeTimeAct))
        {
            return;
        }

        ProcessStepDisplay = $"Step: {processStepAct}";
        RecipeTimeDisplay = $"Recipe Time: {recipeTimeAct:0.##}";
    }

    private bool _isVisibleWafer;
    public bool IsVisibleWafer
    {
        get => _isVisibleWafer;
        set => SetProperty(ref _isVisibleWafer, value);
    }
    private string _waferInfo;

    public string WaferInfo
    {
        get => _waferInfo;
        set => SetProperty(ref _waferInfo, value);
    }
    // Remove events; we'll use async methods instead
    // public event EventHandler<LogEventArgs>? LogEvent;
    // public event EventHandler<LogEventArgs>? LogExsituEvent;

    // Cancellation and task handles for async logging

    private CancellationTokenSource? _logDataCts;
    private Task? _logDataTask;
    private CancellationTokenSource? _logStatusCts;
    private Task? _logStatusTask;

    private CancellationTokenSource? _logExsituCts;
    private Task? _logExsituTask;

    private struct LogConfig
    {
        public uint Address;
        public Enum RunState;
        public Enum InitState;
        public uint PlcWaferIndexAddress; // Add address for reading Wafer Index directly from PLC
    }
    private LogConfig? _logConfig;

    private int _waferIndex;

    public int WaferIndex
    {
        get => _waferIndex;
        set => SetProperty(ref _waferIndex, value);
    }

    public string HeadColor
    {
        get => _headColor;
        set => SetProperty(ref _headColor, value);
    }

    public ChamberStatus ChamberStatus
    {
        get { return _chamberStatus; }
        set
        {
            switch (value)
            {
                case ChamberStatus.Disconnect:
                case ChamberStatus.Offline:
                    HeadColor = "#7F7F7F";
                    break;
                case ChamberStatus.Uninitialized:
                case ChamberStatus.Manual:
                    HeadColor = "#6BA8D6";
                    break;
                case ChamberStatus.Auto:
                case ChamberStatus.Run:
                    HeadColor = "#2C66A7";
                    break;
                case ChamberStatus.Alarm:
                    HeadColor = "#DB3340";
                    break;
                default:
                    HeadColor = "#2C66A7";
                    break;
            }
            SetProperty(ref _chamberStatus, value);
        }
    }

    public Chamber(string name, KVLink link, uint initCommandValue, uint autoModeCommandValue,
        uint manualModeCommandValue,
        uint abortCommandValue, uint commandHandle, uint autoModeHandle)
    {
        _chamberStatus = ChamberStatus.Disconnect;
        _name = name;
        IsEnabled = ChamberAvailability.IsEnabled(name);
        _kvLink = link;
        _startPermitionVarName = "PLCStartPermition";
        _readyVarName = "PLCReady";
        _autoStartVarName = "PLCAutoStart";
        _resetErrorVarName = "PLCResetError";


        if (name.Contains("Polish") || name.Contains("Dry"))
        {
            _initialStatusVarName = "PLCInitStatus";
        }
        else
        {
            _initialStatusVarName = "PLCInitStatus" + name;
        }

        _alarmStatusVarName = "PLCAlarmStatus";
        //autoModeVarName = "PLCAutoModeStatus";

        _initCommandValue = initCommandValue;
        _autoModeCommandValue = autoModeCommandValue;
        _manualModeCommandValue = manualModeCommandValue;
        _abortCommandValue = abortCommandValue;
        _autoModeHandle = autoModeHandle;
        _commandHandle = commandHandle;

        if (_kvLink.sock != -1)
        {
            _chamberStatus = ChamberStatus.Uninitialized;
        }

        _isResetErrorNeedClear = false;
        _isAutoRunNeedClear = false;

        if (IsEnabled)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await MonitorStatusLoopAsync(CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Chamber {_name} MonitorStatusLoopAsync 异常", ex);
                }
            });
            _ = Task.Run(async () =>
            {
                try
                {
                    await MonitorLoggerStatusLoopAsync(CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Chamber {_name} MonitorLoggerStatusLoopAsync 异常", ex);
                }
            });
        }

        // Initialize LogConfig based on chamber name
        InitLogConfig();

        // 启动 Wafer 位置监控
        if (IsEnabled)
        {
            StartWaferMonitoring();
        }
    }

    private void InitLogConfig()
    {
        switch (_name)
        {
            // RollerA Series
            case "RollerA1": _logConfig = new LogConfig { Address = 0x4978, RunState = RollerLogState.Run, InitState = RollerLogState.Initialized, PlcWaferIndexAddress = 0x490C }; break;
            case "RollerA2": _logConfig = new LogConfig { Address = 0x4C78, RunState = RollerLogState.Run, InitState = RollerLogState.Initialized, PlcWaferIndexAddress = 0x4C0C }; break;
            case "RollerA3": _logConfig = new LogConfig { Address = 0x4F78, RunState = RollerLogState.Run, InitState = RollerLogState.Initialized, PlcWaferIndexAddress = 0x4F0C }; break;
            // RollerB Series
            case "RollerB1": _logConfig = new LogConfig { Address = 0x5B78, RunState = RollerLogState.Run, InitState = RollerLogState.Initialized, PlcWaferIndexAddress = 0x5B0C }; break;
            case "RollerB2": _logConfig = new LogConfig { Address = 0x5E78, RunState = RollerLogState.Run, InitState = RollerLogState.Initialized, PlcWaferIndexAddress = 0x5E0C }; break;
            case "RollerB3": _logConfig = new LogConfig { Address = 0x6178, RunState = RollerLogState.Run, InitState = RollerLogState.Initialized, PlcWaferIndexAddress = 0x610C }; break;
            // Pen Series
            case "PenA1": _logConfig = new LogConfig { Address = 0x5278, RunState = PenLogState.Run, InitState = PenLogState.Initialized, PlcWaferIndexAddress = 0x520C }; break;
            case "PenA2": _logConfig = new LogConfig { Address = 0x5578, RunState = PenLogState.Run, InitState = PenLogState.Initialized, PlcWaferIndexAddress = 0x550C }; break;
            case "PenB1": _logConfig = new LogConfig { Address = 0x6478, RunState = PenLogState.Run, InitState = PenLogState.Initialized, PlcWaferIndexAddress = 0x640C }; break;
            case "PenB2": _logConfig = new LogConfig { Address = 0x6778, RunState = PenLogState.Run, InitState = PenLogState.Initialized, PlcWaferIndexAddress = 0x670C }; break;
            // Dry Series
            case "DryA": _logConfig = new LogConfig { Address = 0x5878, RunState = DryLogState.Run, InitState = DryLogState.Initialized, PlcWaferIndexAddress = 0x580C }; break;
            case "DryB": _logConfig = new LogConfig { Address = 0x6A78, RunState = DryLogState.Run, InitState = DryLogState.Initialized, PlcWaferIndexAddress = 0x6A0C }; break;
            // Polish Series (They all share 0x300C for their respective PLC sockets/links)
            case "PolishA": _logConfig = new LogConfig { PlcWaferIndexAddress = 0x300C }; break;
            case "PolishB": _logConfig = new LogConfig { PlcWaferIndexAddress = 0x300C }; break;
            case "PolishC": _logConfig = new LogConfig { PlcWaferIndexAddress = 0x300C }; break;
            case "PolishD": _logConfig = new LogConfig { PlcWaferIndexAddress = 0x300C }; break;
            default: _logConfig = null; break;
        }
    }
    #region 查找每个chamber中的wafer运行记录
    // 启动 Wafer 位置监控
    private void StartWaferMonitoring()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    UpdateWaferDisplay();
                    await Task.Delay(1000); // 每秒检查一次
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"监控 Chamber {_name} 的 Wafer 时出错: {ex.Message}");
                    await Task.Delay(5000);
                }
            }
        });
    }
    // 更新 Wafer 显示
    private void UpdateWaferDisplay()
    {
        string? trackingAddress = _name switch
        {
            "PolishA" => "W3010",
            "PolishB" => "W3650",
            "PolishC" => "W3C90",
            "PolishD" => "W42D0",
            "RollerA1" => "W4910",
            "RollerA2" => "W4C10",
            "RollerA3" => "W4F10",
            "PenA1" => "W5210",
            "PenA2" => "W5510",
            "DryA" => "W5810",
            "RollerB1" => "W5B10",
            "RollerB2" => "W5E10",
            "RollerB3" => "W6110",
            "PenB1" => "W6410",
            "PenB2" => "W6710",
            "DryB" => "W6A10",
            _ => null
        };

        string? waferInfo = null;

        if (!string.IsNullOrWhiteSpace(trackingAddress) && GlobalService.plcComm.IsPLCConnected)
        {
            try
            {
                var value = GlobalService.plcComm.ReadShortVari(trackingAddress);
                if (value > 0)
                {
                    byte slotByte = (byte)(value >> 8);
                    byte cassetteByte = (byte)(value & 0xFF);
                    waferInfo = $"C{cassetteByte}W{slotByte}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取 Chamber {_name} 的 PLC 地址 {trackingAddress} 时出错: {ex.Message}");
            }
        }
        SetWaferDisplay(waferInfo);
    }
    private void SetWaferDisplay(string? waferInfo)
    {
        void UpdateDisplay()
        {
            IsVisibleWafer = !string.IsNullOrEmpty(waferInfo);
            WaferInfo = waferInfo ?? string.Empty;
        }

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher != null && !dispatcher.CheckAccess())
        {
            dispatcher.BeginInvoke(DispatcherPriority.Normal, UpdateDisplay);
            return;
        }

        UpdateDisplay();
    }
    #endregion
    #region Logging Region

   
    /// <summary>
    /// Get Main Step Type
    /// </summary>
    /// <returns></returns>
    private short GetMainStepType()
    {
        var readBuf = new byte[2048];
        short status = -1;
        var err = KHL.KHLReadDevicesAsWords(_kvLink.sock, KHLDevType.DEV_W, Convert.ToUInt32(0x3078), 1, readBuf);
        if (err != (int)KHLUserError.KHL_ERR_NONE)
        {
            if (ShouldLogWarn(ref _nextMainStepWarnLogUtc))
            {
                Logger.Warn("Failed to get Main Step Type, Error: " + err);
            }
            return status;
        }
        KST.ByteToShort(ref status, readBuf);
        return status;
    }

    /// <summary>
    /// Get Main2 Step Type
    /// </summary>
    /// <returns></returns>
    private short GetMain2StepType()
    {
        var readBuf = new byte[2048];
        short status = -1;
        var err = KHL.KHLReadDevicesAsWords(_kvLink.sock, KHLDevType.DEV_W, Convert.ToUInt32(0x307C), 1, readBuf);
        if (err != (int)KHLUserError.KHL_ERR_NONE)
        {
            if (ShouldLogWarn(ref _nextMain2StepWarnLogUtc))
            {
                Logger.Warn("Failed to get Main 2 Step Type, Error: " + err);
            }
            return status;
        }
        KST.ByteToShort(ref status, readBuf);
        return status;
    }

    private static bool ShouldLogWarn(ref DateTime nextLogUtc)
    {
        var now = DateTime.UtcNow;
        if (now < nextLogUtc)
        {
            return false;
        }

        nextLogUtc = now.Add(WarnLogInterval);
        return true;
    }

    /// <summary>
    /// Logging Exsitu Data asynchronously
    /// </summary>
    private async Task LogExsituDataAsync(LogContext context, CancellationToken token)
    {
        await LogWithTemporaryWaferNameAsync(
            context,
            "Exsitu",
            static (helper, waferName, logTime, timeSpan) => helper.LogExsituData(waferName, logTime, timeSpan),
            token);
    }

    private async Task LogDataAsync(LogContext context, CancellationToken token, string? subcategory = null)
    {
        await LogWithTemporaryWaferNameAsync(
            context,
            subcategory,
            static (helper, waferName, logTime, timeSpan) => helper.LogData(waferName, logTime, timeSpan),
            token);
    }

    private async Task LogWithTemporaryWaferNameAsync(
        LogContext context,
        string? subcategory,
        Action<DataLoggerHelper, string, string, string> logAction,
        CancellationToken token)
    {
        var temporaryWaferName = $"Temporary_{_name}_{Guid.NewGuid():N}";
        var resolvedWaferLogName = IsKnownWaferLogName(context.WaferLogName) ? context.WaferLogName : null;
        var nextWaferNameRetryUtc = DateTime.UtcNow;
        var dataLogger = new DataLogger(_name);
        var dataLoggerHelper = new DataLoggerHelper(dataLogger, _name)
        {
            PolishLogSubcategory = string.Equals(subcategory, "Exsitu", StringComparison.OrdinalIgnoreCase)
                ? null
                : subcategory
        };
        DateTime? last = DateTime.Now;
        var nextRowErrorWarnUtc = DateTime.MinValue;

        try
        {
            while (!token.IsCancellationRequested)
            {
                if (resolvedWaferLogName == null && DateTime.UtcNow >= nextWaferNameRetryUtc)
                {
                    var waferLogName = GetWaferLogName();
                    if (IsKnownWaferLogName(waferLogName))
                    {
                        resolvedWaferLogName = waferLogName;
                    }
                    else
                    {
                        nextWaferNameRetryUtc = DateTime.UtcNow.AddMilliseconds(WaferNameRetryIntervalMilliseconds);
                    }
                }

                var now = DateTime.Now;
                var timeSpan = last.HasValue ? (now - last.Value).TotalSeconds : 0.0;
                try
                {
                    logAction(dataLoggerHelper, temporaryWaferName, now.ToString("yyyy-MM-dd HH:mm:ss:fff"), timeSpan.ToString("F3"));
                }
                catch (Exception ex)
                {
                    // 一行写失败（例如键冲突、文件被占用）不能让整个腔体的数据记录任务退出。
                    if (DateTime.UtcNow >= nextRowErrorWarnUtc)
                    {
                        nextRowErrorWarnUtc = DateTime.UtcNow.AddSeconds(10);
                        Logger.Warn($"Failed to append a data log row for {_name}: {ex.Message}");
                    }
                }

                try
                {
                    await Task.Delay(100, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
        finally
        {
            if (resolvedWaferLogName == null)
            {
                var waferLogName = GetWaferLogName();
                resolvedWaferLogName = IsKnownWaferLogName(waferLogName)
                    ? waferLogName
                    : DataLogFormatting.UnknownWaferLogName;
            }

            try
            {
                dataLogger.RenameWaferFile(
                    temporaryWaferName,
                    resolvedWaferLogName,
                    subcategory,
                    context.RecipeName);
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to rename {_name} log file to wafer {resolvedWaferLogName}: {ex.Message}");
            }
        }
    }

    private static bool IsKnownWaferLogName(string? waferLogName)
    {
        return !string.IsNullOrWhiteSpace(waferLogName)
            && !string.Equals(
                waferLogName,
                DataLogFormatting.UnknownWaferLogName,
                StringComparison.OrdinalIgnoreCase);
    }
  

    private int GetWaferIndexFromPlc()
    {
        if (_logConfig.HasValue && _logConfig.Value.PlcWaferIndexAddress != 0)
        {
            var readBuf = new byte[2048];
            short waferIndex = -1;
            var err = KHL.KHLReadDevicesAsWords(_kvLink.sock, KHLDevType.DEV_W, _logConfig.Value.PlcWaferIndexAddress, 1, readBuf);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KST.ByteToShort(ref waferIndex, readBuf);
                if (waferIndex > 0)
                {
                    return waferIndex;
                }
            }
        }
        return -1;
    }

    private string GetWaferLogName()
    {
        var wafer = GetCurrentWafer();
        if (wafer != null)
        {
            DataLogFormatting.TryBuildWaferLogName(wafer, out var waferLogName);
            return waferLogName;
        }

        return DataLogFormatting.UnknownWaferLogName;
    }

    private Wafer? GetCurrentWafer()
    {
        int plcWaferIndex = GetWaferIndexFromPlc();
        int currentIndex = plcWaferIndex != -1 ? plcWaferIndex : WaferIndex;

        return currentIndex != -1 && GlobalService.dicWaferIndex.TryGetValue(currentIndex, out Wafer? wafer)
            ? wafer
            : null;
    }

    private string? GetPolishRecipeName()
    {
        return DataLogFormatting.TryGetPolishRecipeName(
            GetCurrentWafer(),
            _name,
            out var recipeName)
            ? recipeName
            : null;
    }

    private bool IsPolishChamber() =>
        _name.StartsWith("Polish", StringComparison.Ordinal);

    private static bool TryGetPolishLogFolder(short mainStatus, out string folder)
    {
        folder = mainStatus switch
        {
            (short)PolishLogState.Polish => "Polish",
            (short)PolishLogState.PolishLoad => "Load",
            (short)PolishLogState.PolishUnload => "Unload",
            (short)PolishLogState.PolishDechuck => "Dechuck",
            _ => string.Empty
        };
        return folder.Length > 0;
    }

    private static PolishLogState PolishLogStateFromFolder(string folder) => folder switch
    {
        "Load" => PolishLogState.PolishLoad,
        "Unload" => PolishLogState.PolishUnload,
        "Dechuck" => PolishLogState.PolishDechuck,
        _ => PolishLogState.Polish
    };

   

    /// <summary>
    /// Monitoring the logger status and logging data periodically.
    /// </summary>
    private async Task MonitorLoggerStatusLoopAsync(CancellationToken cancellationToken)
    {
        const int loggerLoopDelayMs = 250;
        const int mainStatusReadIntervalCycle = 2;
        short lastLogStatus = -1000;
        short lastMainStatus = -1000;
        short lastMain2Status = -1000;

        bool isExsituLogging = false;
        bool isDataLogging = false;
        string? activePolishLogFolder = null;
        bool isOtherLogging = false;
        DateTime? exsituZeroDetectedUtc = null;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(loggerLoopDelayMs, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            // Refactored logic using _logConfig
            // Only process general log address if the chamber has a valid non-zero address (e.g. Roller, Pen, Dry)
            if (_logConfig.HasValue && _logConfig.Value.Address != 0)
            {
                var config = _logConfig.Value;
                short status = GetLogStatus(config.Address);

                if (status != lastLogStatus)
                {
                    Logger.For(Logger.Category.Logging).Debug($"Chamber {_name}: Read log status {status} from address {config.Address:X}");
                    lastLogStatus = status;
                }

                // -1 is typically returned on network error, -200 from GetLogStatus default error
                if (status != -1)
                {
                    bool startLogging = false;
                    bool stopLogging = false;

                    // Ensure RunState is not null to prevent conversion exception
                    if (config.RunState != null && status == Convert.ToInt32(config.RunState))
                    {
                        startLogging = true;
                    }
                    else
                    {
                        startLogging = false;
                        stopLogging = true;
                    }

                    if (startLogging)
                    {
                        if (!isOtherLogging)
                        {
                            _logStatusCts?.Cancel();
                            _logStatusCts = new CancellationTokenSource();
                            // Pass the specific Enum state and the current WaferId
                            var context = new LogContext(GetWaferLogName(), config.RunState, _name);

                            isOtherLogging = true;
                            _logStatusTask = Task.Run(async () =>
                            {
                                try
                                {
                                    await LogOtherChamberDataAsync(context, _logStatusCts.Token);
                                }
                                catch (Exception ex)
                                {
                                    Logger.Warn($"Other Logging Task for {_name} crashed: {ex.Message}");
                                }
                                finally
                                {
                                    isOtherLogging = false;
                                }
                            });
                        }
                    }
                    else if (stopLogging)
                    {
                        if (isOtherLogging)
                        {
                            _logStatusCts?.Cancel();
                        }
                    }
                }
            }

            if (++_mainStatusReadCycle >= mainStatusReadIntervalCycle)
            {
                _mainStatusReadCycle = 0;
                var mainStatus = GetMainStepType();
                var main2Status = GetMain2StepType();

                if (mainStatus != lastMainStatus || main2Status != lastMain2Status)
                {
                    Logger.For(Logger.Category.Logging).Debug($"Chamber {_name}: Main Step Type {mainStatus}, Main2 Step Type {main2Status}");
                    lastMainStatus = mainStatus;
                    lastMain2Status = main2Status;
                }

                // Exsitu Logging via Main2Step (For Polish Chambers mainly)
                if (main2Status != 0 && main2Status != -1)
                {
                    exsituZeroDetectedUtc = null;
                    if (!isExsituLogging)
                    {
                        // Start async exsitu logging
                        _logExsituCts?.Cancel();
                        _logExsituCts = new CancellationTokenSource();
                        string waferLogName = GetWaferLogName();
                        var exsituContext = new LogContext(
                            waferLogName,
                            PolishLogState.ExsituCondition,
                            _name,
                            GetPolishRecipeName());

                        isExsituLogging = true;
                        // Launch in background to prevent blocking the while loop
                        _logExsituTask = Task.Run(async () =>
                        {
                            try
                            {
                                await LogExsituDataAsync(exsituContext, _logExsituCts.Token);
                            }
                            catch (Exception ex)
                            {
                                Logger.Warn($"Exsitu Logging Task for {_name} crashed: {ex.Message}");
                            }
                            finally
                            {
                                isExsituLogging = false;
                            }
                        });
                    }
                }
                else if (main2Status == 0)
                {
                    if (isExsituLogging)
                    {
                        exsituZeroDetectedUtc ??= DateTime.UtcNow;
                        if (DateTime.UtcNow - exsituZeroDetectedUtc.Value >= TimeSpan.FromMilliseconds(ExsituStopDebounceMilliseconds))
                        {
                            _logExsituCts?.Cancel();
                            exsituZeroDetectedUtc = null;
                        }
                    }
                }
                else
                {
                    exsituZeroDetectedUtc = null;
                    if (isExsituLogging)
                    {
                        _logExsituCts?.Cancel();
                    }
                }

                // Polish 按主步拆文件：101 Polish、103 Load、104 Unload、105 Dechuck。
                // 其它腔体仍是主步非 0 就记一份。
                if (IsPolishChamber())
                {
                    if (TryGetPolishLogFolder(mainStatus, out var folder))
                    {
                        if (!isDataLogging || !string.Equals(activePolishLogFolder, folder, StringComparison.Ordinal))
                        {
                            var session = Interlocked.Increment(ref _polishLogSession);
                            _logDataCts?.Cancel();
                            _logDataCts = new CancellationTokenSource();
                            activePolishLogFolder = folder;
                            isDataLogging = true;
                            var context = new LogContext(
                                GetWaferLogName(),
                                PolishLogStateFromFolder(folder),
                                _name,
                                GetPolishRecipeName());
                            var token = _logDataCts.Token;
                            _logDataTask = Task.Run(async () =>
                            {
                                try
                                {
                                    await LogDataAsync(context, token, folder).ConfigureAwait(false);
                                }
                                catch (Exception ex)
                                {
                                    Logger.Warn($"Data Logging Task for {_name} crashed: {ex.Message}");
                                }
                                finally
                                {
                                    if (session == Volatile.Read(ref _polishLogSession))
                                    {
                                        isDataLogging = false;
                                        activePolishLogFolder = null;
                                    }
                                }
                            });
                        }
                    }
                    else if (isDataLogging)
                    {
                        Interlocked.Increment(ref _polishLogSession);
                        isDataLogging = false;
                        activePolishLogFolder = null;
                        _logDataCts?.Cancel();
                    }
                }
                else if (mainStatus != 0 && mainStatus != -1)
                {
                    if (!isDataLogging)
                    {
                        _logDataCts?.Cancel();
                        _logDataCts = new CancellationTokenSource();
                        string waferLogName = GetWaferLogName();
                        var context = new LogContext(
                            waferLogName,
                            PolishLogState.PolishLoad,
                            _name,
                            GetPolishRecipeName());

                        isDataLogging = true;
                        _logDataTask = Task.Run(async () =>
                        {
                            try
                            {
                                await LogDataAsync(context, _logDataCts.Token);
                            }
                            catch (Exception ex)
                            {
                                Logger.Warn($"Data Logging Task for {_name} crashed: {ex.Message}");
                            }
                            finally
                            {
                                isDataLogging = false;
                            }
                        });
                    }
                }
                else if (isDataLogging)
                {
                    _logDataCts?.Cancel();
                }
            }
        }
    }

    private short GetLogStatus(uint address)
    {
        var readBuf = new byte[2048];
        short status = -1;
        var err = KHL.KHLReadDevicesAsWords(_kvLink.sock, KHLDevType.DEV_W, address, 1, readBuf);
        if (err != (int)KHLUserError.KHL_ERR_NONE)
        {
            if (ShouldLogWarn(ref _nextLogStatusWarnLogUtc))
            {
                Logger.Warn("Failed to get Main Step Type, Error: " + err);
            }
            return status;
        }
        KST.ByteToShort(ref status, readBuf);
        return status;
    }

    private async Task LogOtherChamberDataAsync(LogContext context, CancellationToken token)
    {
        await LogWithTemporaryWaferNameAsync(
            context,
            null,
            static (helper, waferName, logTime, timeSpan) => helper.LogData(waferName, logTime, timeSpan),
            token);
    }

    #endregion

    public bool IsInitialized()
    {
        if (!IsEnabled)
        {
            return false;
        }

        if (_chamberStatus == ChamberStatus.Offline || _chamberStatus == ChamberStatus.Disconnect)
        {
            return false;
        }

        bool isInitialized = false;
        _err = KHL.KHLReadVariable(_kvLink.sock, _initialStatusVarName, _readBuf, true);
        if (_err == (int)KHLUserError.KHL_ERR_NONE)
        {
            KST.ByteToBool(ref isInitialized, _readBuf);
        }

        if (isInitialized)
        {
            //ChamberStatus = ChamberStatus.Manual;
            IsAutoMode();
        }
        else
        {
            ChamberStatus = ChamberStatus.Uninitialized;
        }

        return isInitialized;
    }

    public bool IsAutoMode()
    {
        if (!IsEnabled)
        {
            return false;
        }

        if (_chamberStatus == ChamberStatus.Offline || _chamberStatus == ChamberStatus.Disconnect)
        {
            return false;
        }

        bool isAutoMode = false;
        short result = -1;
        _err = KHL.KHLReadDevicesAsWords(_kvLink.sock, KHLDevType.DEV_W, _autoModeHandle, 1, _readBuf);
        if (_err == (int)KHLUserError.KHL_ERR_NONE)
        {
            KST.ByteToShort(ref result, _readBuf);
        }

        if (result == 0)
        {
            isAutoMode = true;
        }
        else
        {
            isAutoMode = false;
        }

        if (isAutoMode)
        {
            ChamberStatus = ChamberStatus.Auto;
        }
        else
        {
            ChamberStatus = ChamberStatus.Manual;
        }

        return isAutoMode;
    }

    /// <summary>
    /// 通过读取初始化状态的错误码来确定链接状态，可能有更好的方式，暂实行
    /// </summary>
    /// <returns></returns>
    public bool IsConnected()
    {
        if (!IsEnabled)
        {
            return false;
        }

        if (_chamberStatus == ChamberStatus.Offline)
        {
            return false;
        }

        bool isConnected = false;
        _err = KHL.KHLReadVariable(_kvLink.sock, _initialStatusVarName, _readBuf, true);
        if (_err == (int)KHLUserError.KHL_ERR_NONE)
        {
            isConnected = true;
        }
        else
        {
            isConnected = false;
            ChamberStatus = ChamberStatus.Disconnect;
        }

        return isConnected;
    }

    public void SwitchToOffline()
    {
        if (!IsEnabled)
        {
            return;
        }

        ChamberStatus = ChamberStatus.Offline;
    }

    public void SwitchToOnline()
    {
        if (!IsEnabled)
        {
            return;
        }

        ChamberStatus = ChamberStatus.Uninitialized;
    }

    public void InitializeCommand()
    {
        if (!IsEnabled)
        {
            return;
        }

        if (_chamberStatus == ChamberStatus.Offline || _chamberStatus == ChamberStatus.Disconnect)
        {
            return;
        }

        if (!IsInitialized())
        {
            KST.UintToByte(ref _writeBuf, _initCommandValue);
            _err = KHL.KHLWriteDevicesAsWords(_kvLink.sock, KHLDevType.DEV_W, _commandHandle, 1, _writeBuf);
            if (_err == (int)KHLUserError.KHL_ERR_NONE)
            {
            }
            else
            {
                Growl.ErrorGlobal($"{_name} InitCommandExecuteError: {_err} ");
            }
        }
    }

    public void SwitchToAutoModeCommand()
    {
        if (!IsEnabled)
        {
            return;
        }

        if (_chamberStatus == ChamberStatus.Offline || _chamberStatus == ChamberStatus.Disconnect)
        {
            return;
        }

        if (_chamberStatus == ChamberStatus.Manual)
        {
            KST.UintToByte(ref _writeBuf, _autoModeCommandValue);
            _err = KHL.KHLWriteDevicesAsWords(_kvLink.sock, KHLDevType.DEV_W, _commandHandle, 1, _writeBuf);
            if (_err == (int)KHLUserError.KHL_ERR_NONE)
            {
            }
            else
            {
                Growl.ErrorGlobal($"{_name} AutoModeCommandExecuteError: {_err} ");
            }
        }
    }

    public void SwitchToManualModeCommand()
    {
        if (!IsEnabled)
        {
            return;
        }

        if (_chamberStatus == ChamberStatus.Offline || _chamberStatus == ChamberStatus.Disconnect)
        {
            return;
        }

        if (_chamberStatus == ChamberStatus.Auto)
        {
            KST.UintToByte(ref _writeBuf, _manualModeCommandValue);
            _err = KHL.KHLWriteDevicesAsWords(_kvLink.sock, KHLDevType.DEV_W, _commandHandle, 1, _writeBuf);
            if (_err == (int)KHLUserError.KHL_ERR_NONE)
            {
            }
            else
            {
                Growl.ErrorGlobal($"{_name} ManulModeCommandExecuteError: {_err} ");
            }
        }
    }

    public void AbortCommand()
    {
        if (!IsEnabled)
        {
            return;
        }

        //if (_chamberStatus == ChamberStatus.Offline || _chamberStatus == ChamberStatus.Disconnect)
        //{
        //    return;
        //}
        KST.UintToByte(ref _writeBuf, _abortCommandValue);
        _err = KHL.KHLWriteDevicesAsWords(_kvLink.sock, KHLDevType.DEV_W, _commandHandle, 1, _writeBuf);
        if (_err == (int)KHLUserError.KHL_ERR_NONE)
        {
        }
        else
        {
            Growl.ErrorGlobal($"{_name} AbortCommandExecuteError: {_err} ");
        }
    }

    public void ResetError()
    {
        if (!IsEnabled)
        {
            return;
        }

        if (_chamberStatus == ChamberStatus.Offline || _chamberStatus == ChamberStatus.Disconnect)
        {
            return;
        }

        KST.BoolToByte(ref _writeBuf, true);
        _err = KHL.KHLWriteVariable(_kvLink.sock, _resetErrorVarName, _writeBuf, true);
        if (_err == (int)KHLUserError.KHL_ERR_NONE)
        {
            _isResetErrorNeedClear = true;
            ChamberStatus = ChamberStatus.Uninitialized;
        }
        else
        {
            Growl.ErrorGlobal($"{_name} ResetErrorCommandExecuteError: {_err} ");
        }
    }

    public void ResetErrorClear()
    {
        if (!IsEnabled)
        {
            return;
        }

        if (_chamberStatus == ChamberStatus.Offline || _chamberStatus == ChamberStatus.Disconnect)
        {
            return;
        }

        KST.BoolToByte(ref _writeBuf, false);
        _err = KHL.KHLWriteVariable(_kvLink.sock, _resetErrorVarName, _writeBuf, true);
        if (_err == (int)KHLUserError.KHL_ERR_NONE)
        {
            _isResetErrorNeedClear = false;
        }
        else
        {
            Growl.ErrorGlobal($"{_name} ResetErrorClearCommandExecuteError: {_err} ");
        }
    }

    public void PlcStartAndReady()
    {
        if (!IsEnabled)
        {
            return;
        }

        if (_chamberStatus == ChamberStatus.Offline || _chamberStatus == ChamberStatus.Disconnect)
        {
            return;
        }

        KST.BoolToByte(ref _writeBuf, true);
        _err = KHL.KHLWriteVariable(_kvLink.sock, _startPermitionVarName, _writeBuf, true);
        _err = KHL.KHLWriteVariable(_kvLink.sock, _readyVarName, _writeBuf, true);
    }

    public void PlcAutoStart()
    {
        if (!IsEnabled)
        {
            return;
        }

        if (_chamberStatus != ChamberStatus.Auto)
        {
            return;
        }

        KST.BoolToByte(ref _writeBuf, true);
        _err = KHL.KHLWriteVariable(_kvLink.sock, _autoStartVarName, _writeBuf, true);
        if (_err == (int)KHLUserError.KHL_ERR_NONE)
        {
            _isAutoRunNeedClear = true;
        }
        else
        {
            Growl.ErrorGlobal($"{_name} AutoStartCommandExecuteError: {_err} ");
        }
    }

    public void ResetAutoStart()
    {
        if (!IsEnabled)
        {
            return;
        }

        if (_chamberStatus != ChamberStatus.Auto)
        {
            return;
        }

        KST.BoolToByte(ref _writeBuf, false);
        _err = KHL.KHLWriteVariable(_kvLink.sock, _autoStartVarName, _writeBuf, true);
        if (_err == (int)KHLUserError.KHL_ERR_NONE)
        {
            _isAutoRunNeedClear = false;
        }
        else
        {
            Growl.ErrorGlobal($"{_name} AutoStartCommandExecuteError: {_err} ");
        }
    }

    private async Task MonitorStatusLoopAsync(CancellationToken cancellationToken)
    {
        const int delay = 500;
        try
        {
            await Task.Delay(2000, cancellationToken).ConfigureAwait(false);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (_isResetErrorNeedClear)
                {
                    _resetErrorDelayCycle++;
                    if (_resetErrorDelayCycle >= 4)
                    {
                        ResetErrorClear();
                        _resetErrorDelayCycle = 0;
                    }
                }

                try
                {
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (_isAutoRunNeedClear)
                {
                    _resetAutoRunCycle++;
                    if (_resetAutoRunCycle >= 4)
                    {
                        ResetAutoStart();
                        _resetAutoRunCycle = 0;
                    }
                }

                switch (_chamberStatus)
                {
                    case ChamberStatus.Disconnect:
                    case ChamberStatus.Uninitialized:
                    case ChamberStatus.Manual:
                    case ChamberStatus.Auto:
                    case ChamberStatus.Alarm:
                    case ChamberStatus.Offline:
                    default:
                        break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 正常退出
        }
    }
    // 添加静态字典来映射 Chamber 名称到 CurrentStation
    private static readonly Dictionary<string, CurrentStation> ChamberToStationMap = new Dictionary<string, CurrentStation>
    {
        { "PenB1", CurrentStation.CleanBP1 },
        { "PenB2", CurrentStation.CleanBP2 },
        { "PenA1", CurrentStation.CleanAP1 },
        { "PenA2", CurrentStation.CleanAP2 },
        { "PolishA", CurrentStation.PolishA },
        { "PolishB", CurrentStation.PolishB },
        { "PolishC", CurrentStation.PolishC },
        { "PolishD", CurrentStation.PolishD },
        { "RollerA1", CurrentStation.CleanAR1 },
        { "RollerA2", CurrentStation.CleanAR2 },
        { "RollerA3", CurrentStation.CleanAR3 },
        { "RollerB1", CurrentStation.CleanBR1 },
        { "RollerB2", CurrentStation.CleanBR2 },
        { "RollerB3", CurrentStation.CleanBR3 },
        { "DryA", CurrentStation.CleanADry },
        { "DryB", CurrentStation.CleanBDry }
    };
}

[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
public class GlobalChambers
{
    public Chamber PolishA;
    public Chamber PolishB;
    public Chamber PolishC;
    public Chamber PolishD;
    public Chamber RollerA1;
    public Chamber RollerA2;
    public Chamber RollerA3;
    public Chamber RollerB1;
    public Chamber RollerB2;
    public Chamber RollerB3;
    public Chamber PenA1;
    public Chamber PenA2;
    public Chamber PenB1;
    public Chamber PenB2;
    public Chamber DryA;
    public Chamber DryB;

    public List<Chamber> listChamber;

    public static class PolishCommandAndHandle
    {
        public static uint AutoMode = 1000;
        public static uint ManualMode = 1001;
        public static uint Initialize = 1004;
        public static uint Abort = 1017;
        public static uint Handle = HANDLE.CmdPolish;
        public static uint AutoModeHandle = 0x3004;
    }

    public static class RollerA1CommandAndHandle
    {
        public static uint AutoMode = 1000;
        public static uint ManualMode = 1001;
        public static uint Initialize = 1004;
        public static uint Abort = 1013;
        public static uint Handle = HANDLE.CmdRollA1;
        public static uint AutoModeHandle = 0x4904;
    }

    public static class RollerA2CommandAndHandle
    {
        public static uint AutoMode = 1000;
        public static uint ManualMode = 1001;
        public static uint Initialize = 1004;
        public static uint Abort = 1013;
        public static uint Handle = HANDLE.CmdRollA2;
        public static uint AutoModeHandle = 0x4C04;
    }

    public static class RollerA3CommandAndHandle
    {
        public static uint AutoMode = 1000;
        public static uint ManualMode = 1001;
        public static uint Initialize = 1004;
        public static uint Abort = 1013;
        public static uint Handle = HANDLE.CmdRollA3;
        public static uint AutoModeHandle = 0x4F04;
    }

    public static class RollerB1CommandAndHandle
    {
        public static uint AutoMode = 1000;
        public static uint ManualMode = 1001;
        public static uint Initialize = 1004;
        public static uint Abort = 1013;
        public static uint Handle = HANDLE.CmdRollB1;
        public static uint AutoModeHandle = 0x5B04;
    }

    public static class RollerB2CommandAndHandle
    {
        public static uint AutoMode = 1000;
        public static uint ManualMode = 1001;
        public static uint Initialize = 1004;
        public static uint Abort = 1013;
        public static uint Handle = HANDLE.CmdRollB2;
        public static uint AutoModeHandle = 0x5E04;
    }

    public static class RollerB3CommandAndHandle
    {
        public static uint AutoMode = 1000;
        public static uint ManualMode = 1001;
        public static uint Initialize = 1004;
        public static uint Abort = 1013;
        public static uint Handle = HANDLE.CmdRollB3;
        public static uint AutoModeHandle = 0x6104;
    }

    public static class PenA1CommandAndHandle
    {
        public static uint AutoMode = 1000;
        public static uint ManualMode = 1001;
        public static uint Initialize = 1004;
        public static uint Abort = 1013;
        public static uint Handle = HANDLE.CmdPenA1;
        public static uint AutoModeHandle = 0x5204;
    }

    public static class PenA2CommandAndHandle
    {
        public static uint AutoMode = 1000;
        public static uint ManualMode = 1001;
        public static uint Initialize = 1004;
        public static uint Abort = 1013;
        public static uint Handle = HANDLE.CmdPenA2;
        public static uint AutoModeHandle = 0x5504;
    }

    public static class PenB1CommandAndHandle
    {
        public static uint AutoMode = 1000;
        public static uint ManualMode = 1001;
        public static uint Initialize = 1004;
        public static uint Abort = 1013;
        public static uint Handle = HANDLE.CmdPenB1;
        public static uint AutoModeHandle = 0x6404;
    }

    public static class PenB2CommandAndHandle
    {
        public static uint AutoMode = 1000;
        public static uint ManualMode = 1001;
        public static uint Initialize = 1004;
        public static uint Abort = 1013;
        public static uint Handle = HANDLE.CmdPenB2;
        public static uint AutoModeHandle = 0x6704;
    }

    public static class DryACommandAndHandle
    {
        public static uint AutoMode = 1000;
        public static uint ManualMode = 1001;
        public static uint Initialize = 1004;
        public static uint Abort = 1013;
        public static uint Handle = HANDLE.CmdDryA;
        public static uint AutoModeHandle = 0x5804;
    }

    public static class DryBCommandAndHandle
    {
        public static uint AutoMode = 1000;
        public static uint ManualMode = 1001;
        public static uint Initialize = 1004;
        public static uint Abort = 1013;
        public static uint Handle = HANDLE.CmdDryB;
        public static uint AutoModeHandle = 0x6A04;
    }

    public GlobalChambers()
    {
        PolishA = new Chamber("PolishA", GlobalService.kvHostLinkInit.kvLinkPolishA, POLISH.Initialize,
            POLISH.AutoMode, POLISH.ManualMode, POLISH.Abort, POLISH.Handle, POLISH.AutoModeHandle);
        PolishB = new Chamber("PolishB", GlobalService.kvHostLinkInit.kvLinkPolishB, POLISH.Initialize,
            POLISH.AutoMode, POLISH.ManualMode, POLISH.Abort, POLISH.Handle, POLISH.AutoModeHandle);
        PolishC = new Chamber("PolishC", GlobalService.kvHostLinkInit.kvLinkPolishC, POLISH.Initialize,
            POLISH.AutoMode, POLISH.ManualMode, POLISH.Abort, POLISH.Handle, POLISH.AutoModeHandle);
        PolishD = new Chamber("PolishD", GlobalService.kvHostLinkInit.kvLinkPolishD, POLISH.Initialize,
            POLISH.AutoMode, POLISH.ManualMode, POLISH.Abort, POLISH.Handle, POLISH.AutoModeHandle);
        RollerA1 = new Chamber("RollerA1", GlobalService.kvHostLinkInit.kvLinkRollerA, ROLLERA1.Initialize,
            ROLLERA1.AutoMode, ROLLERA1.ManualMode, ROLLERA1.Abort, ROLLERA1.Handle, ROLLERA1.AutoModeHandle);
        RollerA2 = new Chamber("RollerA2", GlobalService.kvHostLinkInit.kvLinkRollerA, ROLLERA2.Initialize,
            ROLLERA2.AutoMode, ROLLERA2.ManualMode, ROLLERA2.Abort, ROLLERA2.Handle, ROLLERA2.AutoModeHandle);
        RollerA3 = new Chamber("RollerA3", GlobalService.kvHostLinkInit.kvLinkRollerA, ROLLERA3.Initialize,
            ROLLERA3.AutoMode, ROLLERA3.ManualMode, ROLLERA3.Abort, ROLLERA3.Handle, ROLLERA3.AutoModeHandle);
        RollerB1 = new Chamber("RollerB1", GlobalService.kvHostLinkInit.kvLinkRollerB, ROLLERB1.Initialize,
            ROLLERB1.AutoMode, ROLLERB1.ManualMode, ROLLERB1.Abort, ROLLERB1.Handle, ROLLERB1.AutoModeHandle);
        RollerB2 = new Chamber("RollerB2", GlobalService.kvHostLinkInit.kvLinkRollerB, ROLLERB2.Initialize,
            ROLLERB2.AutoMode, ROLLERB2.ManualMode, ROLLERB2.Abort, ROLLERB2.Handle, ROLLERB2.AutoModeHandle);
        RollerB3 = new Chamber("RollerB3", GlobalService.kvHostLinkInit.kvLinkRollerB, ROLLERB3.Initialize,
            ROLLERB3.AutoMode, ROLLERB3.ManualMode, ROLLERB3.Abort, ROLLERB3.Handle, ROLLERB3.AutoModeHandle);
        PenA1 = new Chamber("PenA1", GlobalService.kvHostLinkInit.kvLinkPenA, PENA1.Initialize,
            PENA1.AutoMode, PENA1.ManualMode, PENA1.Abort, PENA1.Handle, PENA1.AutoModeHandle);
        PenA2 = new Chamber("PenA2", GlobalService.kvHostLinkInit.kvLinkPenA, PENA2.Initialize,
            PENA2.AutoMode, PENA2.ManualMode, PENA2.Abort, PENA2.Handle, PENA2.AutoModeHandle);
        PenB1 = new Chamber("PenB1", GlobalService.kvHostLinkInit.kvLinkPenB, PENB1.Initialize,
            PENB1.AutoMode, PENB1.ManualMode, PENB1.Abort, PENB1.Handle, PENB1.AutoModeHandle);
        PenB2 = new Chamber("PenB2", GlobalService.kvHostLinkInit.kvLinkPenB, PENB2.Initialize,
            PENB2.AutoMode, PENB2.ManualMode, PENB2.Abort, PENB2.Handle, PENB2.AutoModeHandle);
        DryA = new Chamber("DryA", GlobalService.kvHostLinkInit.kvLinkDryA, DRYA.Initialize,
            DRYA.AutoMode, DRYA.ManualMode, DRYA.Abort, DRYA.Handle, DRYA.AutoModeHandle);
        DryB = new Chamber("DryB", GlobalService.kvHostLinkInit.kvLinkDryB, DRYB.Initialize,
            DRYB.AutoMode, DRYB.ManualMode, DRYB.Abort, DRYB.Handle, DRYB.AutoModeHandle);

        listChamber = new List<Chamber>();
        listChamber.Add(PolishA);
        listChamber.Add(PolishB);
        listChamber.Add(PolishC);
        listChamber.Add(PolishD);
        listChamber.Add(RollerA1);
        listChamber.Add(RollerA2);
        listChamber.Add(RollerA3);
        listChamber.Add(RollerB1);
        listChamber.Add(RollerB2);
        listChamber.Add(RollerB3);
        listChamber.Add(PenA1);
        listChamber.Add(PenA2);
        listChamber.Add(PenB1);
        listChamber.Add(PenB2);
        listChamber.Add(DryA);
        listChamber.Add(DryB);

        ConfigureProcessDataDisplays();

        _ = Task.Run(async () =>
        {
            try
            {
                await ChambersMonitorLoopAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error("GlobalChambers.ChambersMonitorLoopAsync 异常", ex);
            }
        });
    }

    private void ConfigureProcessDataDisplays()
    {
        var dataCenter = GlobalService.DataCenter;
        PolishA.ConfigureProcessDataDisplay(dataCenter.listPolish[0]);
        PolishB.ConfigureProcessDataDisplay(dataCenter.listPolish[1]);
        PolishC.ConfigureProcessDataDisplay(dataCenter.listPolish[2]);
        PolishD.ConfigureProcessDataDisplay(dataCenter.listPolish[3]);

        RollerA1.ConfigureProcessDataDisplay(dataCenter.listRoller[0], "_1");
        RollerA2.ConfigureProcessDataDisplay(dataCenter.listRoller[0], "_2");
        RollerA3.ConfigureProcessDataDisplay(dataCenter.listRoller[0], "_3");
        RollerB1.ConfigureProcessDataDisplay(dataCenter.listRoller[1], "_1");
        RollerB2.ConfigureProcessDataDisplay(dataCenter.listRoller[1], "_2");
        RollerB3.ConfigureProcessDataDisplay(dataCenter.listRoller[1], "_3");

        PenA1.ConfigureProcessDataDisplay(dataCenter.listPen[0], "_1");
        PenA2.ConfigureProcessDataDisplay(dataCenter.listPen[0], "_2");
        PenB1.ConfigureProcessDataDisplay(dataCenter.listPen[1], "_1");
        PenB2.ConfigureProcessDataDisplay(dataCenter.listPen[1], "_2");

        DryA.ConfigureProcessDataDisplay(dataCenter.listDry[0]);
        DryB.ConfigureProcessDataDisplay(dataCenter.listDry[1]);
    }

    private async Task ChambersMonitorLoopAsync(CancellationToken cancellationToken)
    {
        const int delay = 500;
        try
        {
            await Task.Delay(2000, cancellationToken).ConfigureAwait(false);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                foreach (var chamber in listChamber)
                {
                    if (chamber.IsEnabled && chamber.IsConnected() && chamber.ChamberStatus != ChamberStatus.Alarm)
                        chamber.IsInitialized();
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 正常退出
        }
    }
}

public enum ChamberStatus
{
    Disconnect = -1,//灰色
    Uninitialized = 0,//浅蓝色
    Manual,//浅蓝色
    Auto,//蓝色
    Alarm,//红色
    Offline,//灰色
    Run,//运行中
}

public class DataLoggerHelper
{
    private readonly DataLogger _dataLogger;
    private readonly string _name;
    private readonly Dictionary<CollectDataChamber, Dictionary<string, SV>> _metadataIndexes = new();

    // 每个键只告警一次，避免 100ms 一行刷屏。
    private readonly HashSet<string> _unresolvedKeys = new(StringComparer.Ordinal);
    private readonly HashSet<string> _missingValueKeys = new(StringComparer.Ordinal);

    public DataLoggerHelper(DataLogger dataLogger, string name)
    {
        _dataLogger = dataLogger;
        _name = name;
    }

    /// <summary>
    /// Polish 主步对应的日志子目录：Polish、Load、Unload、Dechuck。
    /// </summary>
    internal string? PolishLogSubcategory { get; set; }

    private static readonly string[] PolishASideLtpModules =
    [
        "ASide_Feed_Arm1",
        "ASide_Feed_Arm2",
        "ASide_Discharge_Arm3"
    ];

    private static readonly string[] PolishBSideLtpModules =
    [
        "BSide_Feed_Arm1",
        "BSide_Feed_Arm2",
        "BSide_Discharge_Arm3"
    ];

    private static List<DataLogField> CreateDataLogRow(string logTime, string timeSpan)
    {
        return
        [
            new DataLogField("Time", "Time", logTime),
            new DataLogField("Time Span", "Time Span", timeSpan)
        ];
    }

    private void AddSvField(List<DataLogField> fields, CollectDataChamber chamber, string runtimeKey)
    {
        var sv = chamber.GetSv(runtimeKey);
        var metadata = ResolveSvMetadata(chamber, runtimeKey);
        if (metadata == null)
        {
            // SV 变量表里没有这个运行期键 -> 该列被丢掉。以前是静默丢弃，很难发现。
            WarnOnce(_unresolvedKeys, runtimeKey, "has no matching SV row and is dropped from the CSV log");
            return;
        }

        if (sv?.Value == null)
        {
            // 有 SV 变量但采集端从未 SetSv -> CSV 里是一列空白。
            WarnOnce(_missingValueKeys, runtimeKey, "has an SV row but no collected value (the CSV cell will be empty)");
        }

        fields.Add(new DataLogField(runtimeKey, DataLogFormatting.FormatSvHeader(metadata), sv?.Value.toString()));
    }

    private void WarnOnce(HashSet<string> warnedKeys, string runtimeKey, string reason)
    {
        if (!warnedKeys.Add(runtimeKey))
        {
            return;
        }

        Logger.For(Logger.Category.Logging).Warn($"Chamber {_name}: runtime key '{runtimeKey}' {reason}.");
    }

    private static void AddUnresolvedField(List<DataLogField> fields, string runtimeKey)
    {
        // Unconfigured structure members are omitted from the CSV log.
    }

    private SV? ResolveSvMetadata(CollectDataChamber chamber, string runtimeKey)
    {
        if (!_metadataIndexes.TryGetValue(chamber, out var metadataIndex))
        {
            metadataIndex = DataLogFormatting.BuildMetadataIndex(chamber.ListSV.Values);
            _metadataIndexes[chamber] = metadataIndex;
        }

        return DataLogFormatting.ResolveSvMetadata(metadataIndex, runtimeKey, _name);
    }

    public void LogData(string waferName, string logTime, string timeSpan)
    {
        switch (_name)
        {
            case "PolishA":
                LogPolishAData(waferName, logTime, timeSpan);
                break;
            case "PolishB":
                LogPolishBData(waferName, logTime, timeSpan);
                break;
            case "PolishC":
                LogPolishCData(waferName, logTime, timeSpan);
                break;
            case "PolishD":
                LogPolishDData(waferName, logTime, timeSpan);
                break;
            case "RollerA1":
                LogRollerA1Data(waferName, logTime, timeSpan);
                break;
            case "RollerA2":
                LogRollerA2Data(waferName, logTime, timeSpan);
                break;
            case "RollerA3":
                LogRollerA3Data(waferName, logTime, timeSpan);
                break;
            case "RollerB1":
                LogRollerB1Data(waferName, logTime, timeSpan);
                break;
            case "RollerB2":
                LogRollerB2Data(waferName, logTime, timeSpan);
                break;
            case "RollerB3":
                LogRollerB3Data(waferName, logTime, timeSpan);
                break;
            case "PenA1":
                LogPenA1Data(waferName, logTime, timeSpan);
                break;
            case "PenA2":
                LogPenA2Data(waferName, logTime, timeSpan);
                break;
            case "PenB1":
                LogPenB1Data(waferName, logTime, timeSpan);
                break;
            case "PenB2":
                LogPenB2Data(waferName, logTime, timeSpan);
                break;
            case "DryA":
                LogDryAData(waferName, logTime, timeSpan);
                break;
            case "DryB":
                LogDryBData(waferName, logTime, timeSpan);
                break;
            default:
                break;
        }
    }

    public void LogExsituData(string waferName, string logTime, string timeSpan)
    {
        int index = _name switch
        {
            "PolishA" => 0,
            "PolishB" => 1,
            "PolishC" => 2,
            "PolishD" => 3,
            _ => 0
        };
        var polishData = GlobalService.DataCenter.listPolish[index];
        var data = CreateDataLogRow(logTime, timeSpan);

        // collectData
        foreach (var field in polishData.collectDataFields)
        {
            var propertyName = polishData.BuildRuntimeKey(null, field.Name);
            try
            {
                if (field.Name.Contains("Void")) continue;
                AddSvField(data, polishData, propertyName);
            }
            catch
            {
                AddUnresolvedField(data, propertyName);
            }
        }

        // ioData
        foreach (var field in polishData.ioDataFields)
        {
            var propertyName = polishData.BuildRuntimeKey(null, field.Name);
            try
            {
                if (field.Name.Contains("Void")) continue;
                AddSvField(data, polishData, propertyName);
            }
            catch
            {
                AddUnresolvedField(data, propertyName);
            }
        }
        _dataLogger.AppendExsituData(waferName, data);
    }

    private void LogPolishData(string waferName, int index, string logTime, string timeSpan)
    {
        var polishData = GlobalService.DataCenter.listPolish[index];
        var row = BuildPolishRow(polishData, logTime, timeSpan);
        AppendLtpFields(row, polishData.Name);
        _dataLogger.AppendData(waferName, row, PolishLogSubcategory);
    }

    /// <summary>
    /// 组装一行 Polish 数据（键与 Polish.CollectData 的 SetSv 键一致，测试可直接喂一个采集过的对象）。
    /// </summary>
    internal List<DataLogField> BuildPolishRow(Polish polishData, string logTime, string timeSpan)
    {
        var data = CreateDataLogRow(logTime, timeSpan);

        // collectData
        foreach (var field in polishData.collectDataFields)
        {
            var propertyName = polishData.BuildRuntimeKey(null, field.Name);
            try
            {
                if (field.Name.Contains("Void")) continue;
                if (field.Name.Contains("Exsitu")) continue;
                AddSvField(data, polishData, propertyName);
            }
            catch
            {
                AddUnresolvedField(data, propertyName);
            }
        }

        // ioData
        foreach (var field in polishData.ioDataFields)
        {
            var propertyName = polishData.BuildRuntimeKey(null, field.Name);
            try
            {
                if (field.Name.Contains("Void")) continue;
                if (field.Name.Contains("Exsitu")) continue;
                AddSvField(data, polishData, propertyName);
            }
            catch
            {
                AddUnresolvedField(data, propertyName);
            }
        }

        return data;
    }

    private void AppendLtpFields(List<DataLogField> fields, string polishName)
    {
        var moduleNames = polishName switch
        {
            "PolishA" or "PolishB" => PolishASideLtpModules,
            "PolishC" or "PolishD" => PolishBSideLtpModules,
            _ => null
        };
        if (moduleNames == null)
        {
            return;
        }

        var stCtrls = GlobalService.DataCenter.listSTCtrl;
        if (stCtrls == null || stCtrls.Count == 0)
        {
            return;
        }

        var usedKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var moduleName in moduleNames)
        {
            STCtrl? stCtrl = null;
            for (var i = 0; i < stCtrls.Count; i++)
            {
                if (string.Equals(stCtrls[i].ModuleName, moduleName, StringComparison.Ordinal))
                {
                    stCtrl = stCtrls[i];
                    break;
                }
            }

            if (stCtrl == null)
            {
                continue;
            }

            foreach (var item in stCtrl.listDataInSingleStCtrl)
            {
                if (string.IsNullOrWhiteSpace(item.Name))
                {
                    continue;
                }

                var key = $"{moduleName}.{item.Name}";
                if (!usedKeys.Add(key))
                {
                    continue;
                }

                fields.Add(new DataLogField(key, key, item.Value?.ToString()));
            }
        }
    }

    private void LogRollerData(string waferName, int rollerIndex, string logTime, string timeSpan)
    {
        var rollerData = rollerIndex switch
        {
            0 or 1 or 2 => GlobalService.DataCenter.listRoller[0],
            3 or 4 or 5 => GlobalService.DataCenter.listRoller[1],
            _ => null
        };

        if (rollerData == null)
        {
            return;
        }

        _dataLogger.AppendData(waferName, BuildRollerRow(rollerData, rollerIndex, logTime, timeSpan));
    }

    /// <summary>
    /// 组装一行 Roller 数据。键与 Roller.CollectData 的 SetSv 键必须一致；
    /// 这里拆出来是为了让测试可以直接喂一个采集过的 Roller 对象验证“键能不能对上”。
    /// </summary>
    internal List<DataLogField> BuildRollerRow(Roller rollerData, int rollerIndex, string logTime, string timeSpan)
    {
        var keyPrefix = rollerIndex switch
        {
            0 => "_1",
            1 => "_2",
            2 => "_3",
            3 => "_1",
            4 => "_2",
            5 => "_3",
            _ => "_1"
        };

        var data = CreateDataLogRow(logTime, timeSpan);

        // collectData
        foreach (var field in rollerData.collectDataFields)
        {
            var propertyName = rollerData.BuildRuntimeKey(null, field.Name);
            try
            {
                if (field.Name.Contains("Void")) continue;
                if (!field.Name.StartsWith(keyPrefix, StringComparison.Ordinal)) continue;
                AddSvField(data, rollerData, propertyName);
            }
            catch
            {
                AddUnresolvedField(data, propertyName);
            }
        }

        // ioData
        foreach (var field in rollerData.ioDataFields)
        {
            // Roller IO data is read from the shared IOData1/2/3 structures.
            // The structure fields themselves do not contain the roller index;
            // CollectData prefixes the runtime key with _1, _2 or _3 when it
            // stores the value.  Keep the same index-specific lookup here.
            var propertyName = rollerData.BuildRuntimeKey(keyPrefix, field.Name);
            try
            {
                if (field.Name.Contains("Void")) continue;
                // Member* fields are padding placeholders in the shared IO
                // structure and do not have SV rows.
                if (CollectDataChamber.IsPaddingField(field.Name)) continue;
                AddSvField(data, rollerData, propertyName);
            }
            catch
            {
                AddUnresolvedField(data, propertyName);
            }
        }

        return data;
    }

    private void LogPenData(string waferName, int penIndex, string logTime, string timeSpan)
    {
        var penData = penIndex switch
        {
            0 or 1 => GlobalService.DataCenter.listPen[0],
            2 or 3 => GlobalService.DataCenter.listPen[1],
            _ => null
        };

        if (penData == null)
        {
            return;
        }

        _dataLogger.AppendData(waferName, BuildPenRow(penData, penIndex, logTime, timeSpan));
    }

    /// <summary>
    /// 组装一行 Pen 数据。键与 Pen.CollectData 的 SetSv 键必须一致；
    /// 这里拆出来是为了让测试可以直接喂一个采集过的 Pen 对象验证“键能不能对上”。
    /// </summary>
    internal List<DataLogField> BuildPenRow(Pen penData, int penIndex, string logTime, string timeSpan)
    {
        var keyPrefix = penIndex switch
        {
            0 => "_1",
            1 => "_2",
            2 => "_1",
            3 => "_2",
            _ => "_1"
        };

        var data = CreateDataLogRow(logTime, timeSpan);

        // collectData
        foreach (var field in penData.collectDataFields)
        {
            var propertyName = penData.BuildRuntimeKey(null, field.Name);
            try
            {
                if (field.Name.Contains("Void")) continue;
                if (!field.Name.StartsWith(keyPrefix, StringComparison.Ordinal)) continue;
                AddSvField(data, penData, propertyName);
            }
            catch
            {
                AddUnresolvedField(data, propertyName);
            }
        }

        // ioData
        foreach (var field in penData.ioDataFields)
        {
            // 单支笔的 IO 结构体（KHSTPenIOData.stPenIOData）字段名不带 _1/_2 序号，
            // 工位号由键补上；这与 Pen.CollectIOData 里 SetSv 的键完全一致。
            var propertyName = penData.BuildRuntimeKey(keyPrefix, field.Name);
            try
            {
                if (field.Name.Contains("Void")) continue;
                if (CollectDataChamber.IsPaddingField(field.Name)) continue;
                AddSvField(data, penData, propertyName);
            }
            catch
            {
                AddUnresolvedField(data, propertyName);
            }
        }

        return data;
    }

    private void LogDryData(string waferName, int dryIndex, string logTime, string timeSpan)
    {
        var dryData = dryIndex switch
        {
            0 => GlobalService.DataCenter.listDry[0],
            1 => GlobalService.DataCenter.listDry[1],
            _ => null
        };

        if (dryData == null)
        {
            return;
        }

        _dataLogger.AppendData(waferName, BuildDryRow(dryData, logTime, timeSpan));
    }

    /// <summary>
    /// 组装一行 Dry 数据（键与 Dry.CollectData 的 SetSv 键一致，测试可直接喂一个采集过的对象）。
    /// </summary>
    internal List<DataLogField> BuildDryRow(Dry dryData, string logTime, string timeSpan)
    {
        var data = CreateDataLogRow(logTime, timeSpan);

        // collectData
        foreach (var field in dryData.collectDataFields)
        {
            var propertyName = dryData.BuildRuntimeKey(null, field.Name);
            try
            {
                if (field.Name.Contains("Void")) continue;
                AddSvField(data, dryData, propertyName);
            }
            catch
            {
                AddUnresolvedField(data, propertyName);
            }
        }

        // ioData
        foreach (var field in dryData.ioDataFields)
        {
            var propertyName = dryData.BuildRuntimeKey(null, field.Name);
            try
            {
                if (field.Name.Contains("Void")) continue;
                AddSvField(data, dryData, propertyName);
            }
            catch
            {
                AddUnresolvedField(data, propertyName);
            }
        }

        return data;
    }

    private void LogPolishAData(string waferName, string logTime, string timeSpan)
    {
        LogPolishData(waferName, 0, logTime, timeSpan);
    }

    private void LogPolishBData(string waferName, string logTime, string timeSpan)
    {
        LogPolishData(waferName, 1, logTime, timeSpan);
    }

    private void LogPolishCData(string waferName, string logTime, string timeSpan)
    {
        LogPolishData(waferName, 2, logTime, timeSpan);
    }

    private void LogPolishDData(string waferName, string logTime, string timeSpan)
    {
        LogPolishData(waferName, 3, logTime, timeSpan);
    }


    private void LogRollerA1Data(string waferName, string logTime, string timeSpan)
    {
        LogRollerData(waferName, 0, logTime, timeSpan);
    }

    private void LogRollerA2Data(string waferName, string logTime, string timeSpan)
    {
        LogRollerData(waferName, 1, logTime, timeSpan);
    }

    private void LogRollerA3Data(string waferName, string logTime, string timeSpan)
    {
        LogRollerData(waferName, 2, logTime, timeSpan);
    }

    private void LogRollerB1Data(string waferName, string logTime, string timeSpan)
    {
        LogRollerData(waferName, 3, logTime, timeSpan);
    }

    private void LogRollerB2Data(string waferName, string logTime, string timeSpan)
    {
        LogRollerData(waferName, 4, logTime, timeSpan);
    }

    private void LogRollerB3Data(string waferName, string logTime, string timeSpan)
    {
        LogRollerData(waferName, 5, logTime, timeSpan);
    }

    private void LogPenA1Data(string waferName, string logTime, string timeSpan)
    {
        LogPenData(waferName, 0, logTime, timeSpan);
    }

    private void LogPenA2Data(string waferName, string logTime, string timeSpan)
    {
        LogPenData(waferName, 1, logTime, timeSpan);
    }

    private void LogPenB1Data(string waferName, string logTime, string timeSpan)
    {
        LogPenData(waferName, 2, logTime, timeSpan);
    }

    private void LogPenB2Data(string waferName, string logTime, string timeSpan)
    {
        LogPenData(waferName, 3, logTime, timeSpan);
    }

    private void LogDryAData(string waferName, string logTime, string timeSpan)
    {
        LogDryData(waferName, 0, logTime, timeSpan);
    }

    private void LogDryBData(string waferName, string logTime, string timeSpan)
    {
        LogDryData(waferName, 1, logTime, timeSpan);
    }
}

public enum PolishLogState
{
    #region Polish Part

    PolishInitialized = 100,
    Polish = 101,
    PolishLoad = 103,
    PolishUnload = 104,
    PolishDechuck = 105,
    PolishFailure = 108,
    ExsituCondition = 240,
    #endregion
}

public enum RollerLogState
{
    Initialized = 100,
    Run = 101,
}

public enum PenLogState
{
    Initialized = 100,
    Run = 101,
}

public enum DryLogState
{
    Initialized = 100,
    Run = 101,
}


public class LogContext
{
    public string WaferLogName { get; }
    public Enum Status { get; }
    public DateTime Timestamp { get; }
    public string ChamberName { get; }
    public string? RecipeName { get; }

    public LogContext(string waferLogName, Enum status, string chamberName, string? recipeName = null)
    {
        WaferLogName = waferLogName;
        Status = status;
        ChamberName = chamberName;
        RecipeName = recipeName;
        Timestamp = DateTime.UtcNow;
    }
}
