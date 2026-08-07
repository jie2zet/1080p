using CMP.Models.Resource;
using CMP.Models.Warn;
using CMP.Services.LoadPort;
using CMP.Services.Log;
using CMP.Services.Plc;
using CMP.Services.Warn;
using CMP.ViewModels.Tabs.OperationTab;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using HandyControl.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CMP.Models.Operation.LoadPort
{

    public enum PlcOperationType
    {
        PickRequest,
        NotifyPickAllowed,
        UpdateWaferState,
        PlaceRequest,
        NotifyPlaceAllowed,
        PlaceComplete
    }

    public enum LoadportFoupStatus
    {
        WaitPlace,
        Present,
        WaitLoad,
        Load,
        WaitUnload,
        Unload,
        WaitRemove,
    }

    public class PlcOperation
    {
        public PlcOperationType Type { get; set; }
        public object Data { get; set; }
        public DateTime Timestamp { get; set; }
    }
    public partial class LoadPortData : BindableBase
    {
        //// 机械手位置
        //private Point _robotPosition = new Point(90, 10);
        //public Point RobotPosition
        //{
        //    get => _robotPosition;
        //    set => SetProperty(ref _robotPosition, value);
        //}

        //// 机械手是否可见
        //private bool _isRobotVisible = false;
        //public bool IsRobotVisible
        //{
        //    get => _isRobotVisible;
        //    set => SetProperty(ref _isRobotVisible, value);
        //}

        //// 机械手信息
        //private RobotInfo _robotInfo = new RobotInfo();
        //public RobotInfo RobotInfo
        //{
        //    get => _robotInfo;
        //    set => SetProperty(ref _robotInfo, value);
        //}


        // 机械手信息类
        private string _tabName;
        private string _portName;
        private String _foupImageSource;
        private ObservableCollection<Wafer> _waferList;
        private ObservableCollection<string> _commandList;
        private string _selectedCommand;
        private string _commandParameter;
        private ObservableCollection<string> _logMessages;
        private SerialPort _serialPort;
        private bool _isConnected;
        private ILoadPortService _loadPortService;
        private LoadPortStatus _loadPortStatus;
        private readonly object _logLock = new object();
        private readonly object _waferLock = new object();
        private string _selectedGetCommand;
        private Visibility  _isVisibility = Visibility.Hidden;
        private string _dockValue ="15";

        // 在 LoadPortData 类中添加字段控制MOV命令执行时不允许get:state
        private bool _waitingForInfResponse = false;
        private string _lastSentCommand = string.Empty;
        private readonly object _commandLock = new object();

        //PLC相关操作
        private readonly ConcurrentQueue<PlcOperation> _plcOperationQueue;
        private readonly SemaphoreSlim _plcSemaphore;
        private Task _plcProcessingTask;
        private CancellationTokenSource _cancellationTokenSource;
        private MitsubishiPlcService plcService = null;
        private CancellationTokenSource _plcPickMonitoringTokenSource;
        private Task _plcPickMonitoringTask;
        //放料
        private CancellationTokenSource _plcPlaceMonitoringTokenSource;
        private Task _plcPlaceMonitoringTask;

        /// <summary>由 <see cref="LoadPortFoupStatusStateMachine"/> 根据 STATE 快照更新，供界面与流程使用。</summary>
        private LoadportFoupStatus _foupWorkflowState;
        public LoadportFoupStatus FoupWorkflowState
        {
            get => _foupWorkflowState;
            private set => SetProperty(ref _foupWorkflowState, value);
        }

        // 启动PLC取料监控
        public void  StartPlcPickMonitoring()
        {
            

            _plcPickMonitoringTokenSource = new CancellationTokenSource();
            var token = _plcPickMonitoringTokenSource.Token;

            // 单次轮询：M180–M182 块读，合并取料请求与完成检测，减少 PLC 往返
            _plcPickMonitoringTask = Task.Run(() => PollPlcPickMonitorAsync(token), token);

            AddLogMessage($"Started PLC pick monitoring for LoadPort {PortNumber}");
        }
        // 停止PLC取料监控
        public void StopPlcPickMonitoring()
        {
            if (_plcPickMonitoringTokenSource != null)
            {
                _plcPickMonitoringTokenSource.Cancel();

                // 等待任务完成（设置超时避免无限等待）
                try
                {
                    Task.WaitAll(
                        _plcPickMonitoringTask ?? Task.CompletedTask,
                        Task.Delay(5000)
                    );
                }
                catch (AggregateException ex)
                {
                    // 忽略等待异常
                    Logger.Error("轮询取料任务异常",ex,"loadport.txt",0);
                }

                _plcPickMonitoringTokenSource.Dispose();
                _plcPickMonitoringTokenSource = null;

                AddLogMessage($"Stopped PLC pick monitoring for LoadPort {PortNumber}");
            }
        }
        // 在 StartPlcPickMonitoring 方法附近添加新的放料监控方法
        // 启动PLC放料监控
        public void StartPlcPlaceMonitoring()
        {
            _plcPlaceMonitoringTokenSource = new CancellationTokenSource();
            var token = _plcPlaceMonitoringTokenSource.Token;

            // 单次轮询：各 LoadPort M 区连续 4 位块读，合并放料请求与完成检测
            _plcPlaceMonitoringTask = Task.Run(() => PollPlcPlaceMonitorAsync(token), token);

            AddLogMessage($"Started PLC place monitoring for LoadPort {PortNumber}");
        }

        // 停止PLC放料监控
        public void StopPlcPlaceMonitoring()
        {
            if (_plcPlaceMonitoringTokenSource != null)
            {
                _plcPlaceMonitoringTokenSource.Cancel();

                // 等待任务完成（设置超时避免无限等待）
                try
                {
                    Task.WaitAll(
                        _plcPlaceMonitoringTask ?? Task.CompletedTask,
                        Task.Delay(5000)
                    );
                }
                catch (AggregateException ex)
                {
                    Logger.Error("轮询放料任务异常", ex, "loadport.txt", 0);
                }

                _plcPlaceMonitoringTokenSource.Dispose();
                _plcPlaceMonitoringTokenSource = null;

                AddLogMessage($"Stopped PLC place monitoring for LoadPort {PortNumber}");
            }
        }
        private async Task PollPlcPlaceMonitorAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var operation = new PlcOperation
                    {
                        Type = PlcOperationType.PlaceRequest,
                        Data = "PlaceMonitorCheck",
                        Timestamp = DateTime.UtcNow
                    };

                    EnqueuePlcOperation(operation);

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error polling place monitor for LoadPort {PortNumber}: {ex.Message}", ex, "loadport.txt", 0);

                    try
                    {
                        await Task.Delay(1000, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
        }

        private async Task PollPlcPickMonitorAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var operation = new PlcOperation
                    {
                        Type = PlcOperationType.PickRequest,
                        Data = "PickMonitorCheck",
                        Timestamp = DateTime.UtcNow
                    };

                    EnqueuePlcOperation(operation);

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error polling pick monitor for LoadPort {PortNumber}: {ex.Message}", ex, "loadport.txt", 0);

                    try
                    {
                        await Task.Delay(1000, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
        }

        // 修改 ProcessPlcOperation 方法，添加对新操作类型的支持
        private async Task ProcessPlcOperation(PlcOperation operation)
        {
            await _plcSemaphore.WaitAsync();
            try
            {
                switch (operation.Type)
                {
                    case PlcOperationType.PickRequest:
                        await ProcessCustomPlcOperation(operation.Data as string);
                        break;
                    case PlcOperationType.PlaceRequest:
                        await ProcessCustomPlcOperation(operation.Data as string);
                        break;
                    case PlcOperationType.PlaceComplete:
                        await ProcessCustomPlcOperation(operation.Data as string);
                        break;
                }
            }
            finally
            {
                _plcSemaphore.Release();
            }
        }

        // 添加处理自定义PLC操作的方法
        private Wafer _processingWafer = null;
        private object _processingLock = new object();

        /// <summary>全局 M180 取片调度互斥（多 LoadPort 共用同一 PLC 地址与 waferQueue）。</summary>
        private static readonly SemaphoreSlim GlobalPickPlcGate = new(1, 1);

        private int GetLoadportId() => PortNumber switch
        {
            "COM12" => 0,
            "COM13" => 1,
            "COM14" => 2,
            "COM15" => 3,
            "COM16" => 4,
            _ => -1
        };

        /// <summary>上一次完成取片的侧别，用于 A/B 交替；与 PLC M183/M184 及 <see cref="Wafer.SequenceTrackSide"/> 一致。</summary>
        private static WaferSequenceTrackSide? _lastCompletedPickTrackSide;

        private const string PlcTrackSideAReadyAddress = "M183";
        private const string PlcTrackSideBReadyAddress = "M184";

        // 添加处理自定义PLC操作的方法
        private async Task ProcessCustomPlcOperation(string operationType)
        {
            switch (operationType)
            {
                case "PickMonitorCheck":
                    await GlobalPickPlcGate.WaitAsync();
                    try
                    {
                    if (!plcService.IsPLCConnected)
                        break;

                    bool trackAReadyPm;
                    bool trackBReadyPm;
                    if (!TryReadPlcTrackSideReadyFlags(out trackAReadyPm, out trackBReadyPm))
                    {
                        plcService.WriteBoolVari("M180", false);
                        break;
                    }

                    var pickSnapshotForM180 = WaferPickQueue.Snapshot();

                    var queueHasPick = pickSnapshotForM180.Count > 0;
                    var pickEnvOk = queueHasPick && pickSnapshotForM180.Any(w =>
                        CanAllowPlcPickForWafer(w) &&
                        IsTrackReadyForPick(w.SequenceTrackSide, trackAReadyPm, trackBReadyPm));

                    if (pickEnvOk)
                        plcService.WriteBoolVari("M180", true);
                    else
                        plcService.WriteBoolVari("M180", false);

                    var pickBits = plcService.ReadBoolContinuous("M180", 3);
                    if (!pickBits.IsSuccess || pickBits.Content == null || pickBits.Content.Length < 3)
                        break;

                    bool pickRequestedBatch = pickBits.Content[1];
                    bool pickCompletedBatch = pickBits.Content[2];

                    if (pickRequestedBatch && WaferPickQueue.Count > 0)
                    {
                        var orderedForPick = WaferPickQueue.Snapshot();
                        var wPick = SelectNextPickWaferFromOrderedList(orderedForPick, trackAReadyPm, trackBReadyPm);
                        if (wPick != null)
                            WaferPickQueue.RotateToHead(wPick);

                        if (wPick != null &&
                            WaferPickQueue.HeadMatches(wPick) &&
                            CanAllowPlcPickForWafer(wPick) &&
                            IsTrackReadyForPick(wPick.SequenceTrackSide, trackAReadyPm, trackBReadyPm))
                        {
                            lock (_processingLock)
                            {
                                _processingWafer = wPick;
                            }

                            plcService.WriteInt16Continuous("D47500", new short[] { (short)wPick.LoadportID, (short)wPick.SlotID });
                            plcService.WriteBoolVari("M181", false);

                            await UpdateWaferStatusAsync("Pick", wPick.LoadportID, wPick.SlotID);
                            AddLogMessage($"Pick request detected for LoadPort {PortNumber}, allowed pick for wafer L{wPick.LoadportID}S{wPick.SlotID} track {wPick.SequenceTrackSide}");
                        }
                    }

                    if (pickCompletedBatch)
                    {
                        Wafer? processingWafer;
                        lock (_processingLock)
                        {
                            processingWafer = _processingWafer;
                        }

                        if (processingWafer != null &&
                            WaferPickQueue.TryDequeueHeadIfMatches(processingWafer, out var completedWafer) &&
                            completedWafer != null)
                        {
                            lock (_processingLock)
                            {
                                if (_processingWafer != null &&
                                    _processingWafer.LoadportID == completedWafer.LoadportID &&
                                    _processingWafer.SlotID == completedWafer.SlotID)
                                {
                                    _processingWafer = null;
                                }
                            }

                            OnPickCompletedUpdateLastTrackSide(completedWafer);
                            plcService.WriteBoolVari("M182", false);
                            AddLogMessage($"Pick completed for wafer from LoadPort {completedWafer.LoadportID}, slot {completedWafer.SlotID}");
                        }
                    }
                    }
                    finally
                    {
                        GlobalPickPlcGate.Release();
                    }
                    break;

                case "PlaceMonitorCheck":
                    if (!plcService.IsPLCConnected)
                        break;
                    if (!TryGetPlaceMonitorMBase(out int placeMBase))
                        break;

                    var placeBits = plcService.ReadBoolContinuous($"M{placeMBase}", 4);
                    if (!placeBits.IsSuccess || placeBits.Content == null || placeBits.Content.Length < 4)
                        break;

                    bool placeRequestedBatch = placeBits.Content[0];
                    bool placeCompletedBatch = placeBits.Content[3];

                    if (placeRequestedBatch)
                    {
                        SetPlcAllowPlaceStatus(true);
                        AddLogMessage($"Place request detected for LoadPort {PortNumber}, allowed place");
                    }

                    if (placeCompletedBatch)
                    {
                        SetPlcAllowPlaceStatus(false);
                        var (cassetteId, layerNumber) = ReadCassettePlaceInfo();
                        await UpdateWaferStatusAsync("Place", cassetteId, layerNumber);
                        AddLogMessage($"Place completed for LoadPort {PortNumber}, cassette: {cassetteId}, layer: {layerNumber}");
                    }
                    break;

                case "PickRequestCheck":
                    await GlobalPickPlcGate.WaitAsync();
                    try
                    {
                    if (!plcService.IsPLCConnected)
                        break;

                    bool trackAReadyPr;
                    bool trackBReadyPr;
                    if (!TryReadPlcTrackSideReadyFlags(out trackAReadyPr, out trackBReadyPr))
                    {
                        plcService.WriteBoolVari("M180", false);
                        break;
                    }

                    var pickSnapshotPr = WaferPickQueue.Snapshot();

                    var queueHasPickReq = pickSnapshotPr.Count > 0;
                    var pickReqEnvOk = queueHasPickReq && pickSnapshotPr.Any(w =>
                        CanAllowPlcPickForWafer(w) &&
                        IsTrackReadyForPick(w.SequenceTrackSide, trackAReadyPr, trackBReadyPr));

                    if (pickReqEnvOk)
                        plcService.WriteBoolVari("M180", true);
                    else
                        plcService.WriteBoolVari("M180", false);

                    bool pickRequested = ReadPlcPickRequestStatus();

                    if (pickRequested && WaferPickQueue.Count > 0)
                    {
                        var orderedPr = WaferPickQueue.Snapshot();
                        var wSel = SelectNextPickWaferFromOrderedList(orderedPr, trackAReadyPr, trackBReadyPr);
                        if (wSel != null)
                            WaferPickQueue.RotateToHead(wSel);

                        if (wSel != null &&
                            WaferPickQueue.HeadMatches(wSel) &&
                            CanAllowPlcPickForWafer(wSel) &&
                            IsTrackReadyForPick(wSel.SequenceTrackSide, trackAReadyPr, trackBReadyPr))
                        {
                            lock (_processingLock)
                            {
                                _processingWafer = wSel;
                            }

                            plcService.WriteInt16Continuous("D47500", new short[] { (short)wSel.LoadportID, (short)wSel.SlotID });
                            plcService.WriteBoolVari("M181", false);

                            await UpdateWaferStatusAsync("Pick", wSel.LoadportID, wSel.SlotID);
                            AddLogMessage($"Pick request detected for LoadPort {PortNumber}, allowed pick for wafer L{wSel.LoadportID}S{wSel.SlotID} track {wSel.SequenceTrackSide}");
                        }
                    }
                    }
                    finally
                    {
                        GlobalPickPlcGate.Release();
                    }
                    break;

                case "PickCompleteCheck":
                    await GlobalPickPlcGate.WaitAsync();
                    try
                    {
                    bool pickCompleted = ReadPlcPickCompleteStatusForMonitoring();

                    if (pickCompleted)
                    {
                        Wafer? processingWafer;
                        lock (_processingLock)
                        {
                            processingWafer = _processingWafer;
                        }

                        if (processingWafer != null &&
                            WaferPickQueue.TryDequeueHeadIfMatches(processingWafer, out var completedWafer) &&
                            completedWafer != null)
                        {
                            lock (_processingLock)
                            {
                                if (_processingWafer != null &&
                                    _processingWafer.LoadportID == completedWafer.LoadportID &&
                                    _processingWafer.SlotID == completedWafer.SlotID)
                                {
                                    _processingWafer = null;
                                }
                            }

                            OnPickCompletedUpdateLastTrackSide(completedWafer);
                            plcService.WriteBoolVari("M182", false);
                            AddLogMessage($"Pick completed for wafer from LoadPort {completedWafer.LoadportID}, slot {completedWafer.SlotID}");
                        }
                    }
                    }
                    finally
                    {
                        GlobalPickPlcGate.Release();
                    }
                    break;
                // 添加放料请求检查
                case "PlaceRequestCheck":
                    // 轮询PLC放料请求状态
                    bool placeRequested = ReadPlcPlaceRequestStatus();

                    if (placeRequested)
                    {
                        // 当放料请求为true时，将PLC允许放料地址位置为true
                        SetPlcAllowPlaceStatus(true);
                        AddLogMessage($"Place request detected for LoadPort {PortNumber}, allowed place");
                    }
                    break;

                // 添加放料完成检查
                case "PlaceCompleteCheck":
                    // 读取PLC放料完成状态
                    bool placeCompleted = ReadPlcPlaceCompleteStatusForMonitoring();

                    if (placeCompleted)
                    {
                        // 当放料完成时，将对应的允许放料地址位置为false
                        SetPlcAllowPlaceStatus(false);

                        // 读取料盒号与层数
                        var (cassetteId, layerNumber) = ReadCassettePlaceInfo();

                        // 更新wafer状态
                        await UpdateWaferStatusAsync("Place",cassetteId, layerNumber);

                        AddLogMessage($"Place completed for LoadPort {PortNumber}, cassette: {cassetteId}, layer: {layerNumber}");
                    }
                    break;
            }

            await Task.CompletedTask;
        }
        // 添加读取PLC放料请求状态方法
        private bool ReadPlcPlaceRequestStatus()
        {
            try
            {
                if (!plcService.IsPLCConnected) return false;

                switch (PortNumber)
                {
                    case "COM12":
                        return plcService.ReadBoolVari("M2564");
                    case "COM13":
                        return plcService.ReadBoolVari("M2580");
                    case "COM14":
                        return plcService.ReadBoolVari("M2596");
                    case "COM15":
                        return plcService.ReadBoolVari("M2612");
                    case "COM16":
                        return plcService.ReadBoolVari("M2628");
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error reading place request status for LoadPort {PortNumber}: {ex.Message}", ex, "loadport.txt", 0);
                return false;
            }
        }

        // 添加读取PLC放料完成状态方法
        private bool ReadPlcPlaceCompleteStatusForMonitoring()
        {
            try
            {
                if (!plcService.IsPLCConnected) return false;
                switch (PortNumber)
                {
                    case "COM12":
                        return plcService.ReadBoolVari("M2567");
                    case "COM13":
                        return plcService.ReadBoolVari("M2583");
                    case "COM14":
                        return plcService.ReadBoolVari("M2599");
                    case "COM15":
                        return plcService.ReadBoolVari("M2615");
                    case "COM16":
                        return plcService.ReadBoolVari("M2631");
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error reading place complete status for LoadPort {PortNumber}: {ex.Message}", ex, "loadport.txt", 0);
                return false;
            }
        }

        // 添加设置PLC允许放料状态方法
        private void SetPlcAllowPlaceStatus(bool allowPlace)
        {
            try
            { 
                switch (PortNumber)
                {
                    case "COM12":
                        plcService.WriteBoolVari("M6884", allowPlace);
                        break;
                    case "COM13":
                        plcService.WriteBoolVari("M6900", allowPlace);
                        break;
                    case "COM14":
                        plcService.WriteBoolVari("M6916", allowPlace);
                        break;
                    case "COM15":
                        plcService.WriteBoolVari("M6932", allowPlace);
                        break;
                    case "COM16":
                        plcService.WriteBoolVari("M6948", allowPlace);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error setting allow place status for LoadPort {PortNumber}: {ex.Message}", ex, "loadport.txt", 0);
            }
        }

        private bool TryGetPlaceMonitorMBase(out int mBase)
        {
            mBase = 0;
            switch (PortNumber)
            {
                case "COM12": mBase = 2564; return true;
                case "COM13": mBase = 2580; return true;
                case "COM14": mBase = 2596; return true;
                case "COM15": mBase = 2612; return true;
                case "COM16": mBase = 2628; return true;
                default: return false;
            }
        }

        // 添加读取放料盒号与层数方法
        private (int cassetteId, int layerNumber) ReadCassettePlaceInfo()
        {
            try
            {
                if (!plcService.IsPLCConnected)
                    return (-1, 0);

                var batch = plcService.ReadInt16Continuous("D1804", 2);
                if (!batch.IsSuccess || batch.Content == null || batch.Content.Length < 2)
                    return (-1, 0);

                return (batch.Content[0], batch.Content[1]);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error reading cassette place info for LoadPort {PortNumber}: {ex.Message}", ex, "loadport.txt", 0);
                return (-1, 0);
            }
        }

        
        
        // 修改 EnqueuePlcOperation 方法以支持新的操作类型
        private void EnqueuePlcOperation(PlcOperation operation)
        {
            _plcOperationQueue.Enqueue(operation);
        }

        // 添加重载方法以保持向后兼容
        private void EnqueuePlcOperation(PlcOperationType type, object data)
        {
            _plcOperationQueue.Enqueue(new PlcOperation
            {
                Type = type,
                Data = data,
                Timestamp = DateTime.UtcNow
            });
        }
        // 读取PLC取料请求状态
        private bool ReadPlcPickRequestStatus()
        {
            try
            {
                if (!plcService.IsPLCConnected) return false;
                return plcService.ReadBoolVari("M181");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error reading pick request status for LoadPort {PortNumber}: {ex.Message}", ex, "loadport.txt", 0);
                return false;
            }
        }

        // 读取PLC取料完成状态（用于监控）
        private bool ReadPlcPickCompleteStatusForMonitoring()
        {
            try
            {
                if (!plcService.IsPLCConnected) return false;
               
                return plcService.ReadBoolVari("M182");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error reading pick complete status for LoadPort {PortNumber}: {ex.Message}", ex, "loadport.txt", 0);
                return false;
            }
        }

        /// <summary>
        /// 队列头片所对应 LoadPort：FOUP 在位且 FIMS 门打开时才允许向 PLC 发取料相关允许（M180/M181 流程）。
        /// </summary>
        private static bool CanAllowPlcPickForWafer(Wafer wafer)
        {
            if (wafer == null) return false;
            var portName = $"COM{wafer.LoadportID + 12}";
            var lp = LoadPortManager.Instance.GetLoadPort(portName);
            if (lp?.LoadPortStatus == null) return false;
            if (lp.LoadPortStatus.foupPlacementStatus != FoupPlacementStatus.Normal) return false;
            //if(lp.LoadPortStatus.FoupDockStatus != FoupDockStatus.Docking) return false;
            //if (lp.LoadPortStatus.FIMSDoorStatus != FIMSDoorStatus.Open) return false;
            return true;
        }

        private bool TryReadPlcTrackSideReadyFlags(out bool trackAReady, out bool trackBReady)
        {
            trackAReady = false;
            trackBReady = false;
            try
            {
                if (plcService == null || !plcService.IsPLCConnected)
                    return false;
                trackAReady = plcService.ReadBoolVari(PlcTrackSideAReadyAddress);
                trackBReady = plcService.ReadBoolVari(PlcTrackSideBReadyAddress);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"读取 {PlcTrackSideAReadyAddress}/{PlcTrackSideBReadyAddress} 失败 {PortNumber}: {ex.Message}", ex, "loadport.txt", 0);
                return false;
            }
        }

        private static bool IsTrackReadyForPick(WaferSequenceTrackSide side, bool trackAReady, bool trackBReady) =>
            side switch
            {
                WaferSequenceTrackSide.A => trackAReady,
                WaferSequenceTrackSide.B => trackBReady,
                _ => trackAReady && trackBReady
            };

        /// <summary>
        /// 在队列顺序下选出下一片：须满足 FOUP 条件且该侧 PLC 就绪；在有多片可选时优先与上一片侧别交替。
        /// </summary>
        private static Wafer? SelectNextPickWaferFromOrderedList(IReadOnlyList<Wafer> ordered, bool trackAReady, bool trackBReady)
        {
            var candidates = ordered
                .Where(w => CanAllowPlcPickForWafer(w) && IsTrackReadyForPick(w.SequenceTrackSide, trackAReady, trackBReady))
                .ToList();
            if (candidates.Count == 0)
                return null;

            if (_lastCompletedPickTrackSide == WaferSequenceTrackSide.A)
            {
                var preferB = candidates.FirstOrDefault(w => w.SequenceTrackSide == WaferSequenceTrackSide.B);
                if (preferB != null)
                    return preferB;
                var onlyA = candidates.FirstOrDefault(w => w.SequenceTrackSide == WaferSequenceTrackSide.A);
                if (onlyA != null)
                    return onlyA;
                return candidates[0];
            }

            if (_lastCompletedPickTrackSide == WaferSequenceTrackSide.B)
            {
                var preferA = candidates.FirstOrDefault(w => w.SequenceTrackSide == WaferSequenceTrackSide.A);
                if (preferA != null)
                    return preferA;
                var onlyB = candidates.FirstOrDefault(w => w.SequenceTrackSide == WaferSequenceTrackSide.B);
                if (onlyB != null)
                    return onlyB;
                return candidates[0];
            }

            return candidates[0];
        }

        private static void OnPickCompletedUpdateLastTrackSide(Wafer w)
        {
            if (w == null)
                return;
            _lastCompletedPickTrackSide = w.SequenceTrackSide == WaferSequenceTrackSide.Unknown
                ? null
                : w.SequenceTrackSide;
        }

        /// <summary>
        /// FOUP Unload（对接状态进入 Undocking）时，清除 PLC 取料队列允许位 M180。
        /// </summary>
        private void OnFoupUnloadClearPlcPickAllow()
        {
            try
            {
                if (plcService != null && plcService.IsPLCConnected)
                {
                    var loadportId = GetLoadportId();
                    var removed = loadportId >= 0
                        ? WaferPickQueue.RemoveByLoadportId(loadportId)
                        : 0;
                    plcService.WriteBoolVari("M180", false);

                    AddLogMessage($"FOUP Unload（Undocking）{PortNumber}，已移除本端口 {removed} 片待取队列，M180 已置 false");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"FOUP Unload 时清除 M180 失败 {PortNumber}: {ex.Message}", ex, "loadport.txt", 0);
            }
        }

        /// <summary>
        /// 上位或脚本在卸料前调用，将 <see cref="FoupWorkflowState"/> 置为 <see cref="LoadportFoupStatus.WaitUnload"/>，
        /// 直至硬件进入 Undocking 后由状态机转入 <see cref="LoadportFoupStatus.Unload"/>。
        /// </summary>
        public void NotifyHostUnloadRequested()
        {
            if (FoupWorkflowState == LoadportFoupStatus.Load)
                FoupWorkflowState = LoadportFoupStatus.WaitUnload;
        }

        // 更新wafer状态
        private async Task UpdateWaferStatusAsync(string PickOrPlace ,int cassetteId, int layerNumber)
        {
            
            if(cassetteId <0 || layerNumber < 0)
            {
                AddLogMessage($"loadport:{PortNumber}的{PickOrPlace}料异常，料盒号{cassetteId} ，层数{layerNumber}");
                return;
            }
            try
            {
                //根据料盒号和层数更新wafer状态
                switch (cassetteId)
                {

                    case 0:
                        //RobotPosition = new Point(90,10);
                        //IsRobotVisible = true;
                        //RobotInfo = new RobotInfo()
                        //{
                        //    Title = "上下料机器人",
                        //    Position = $"loadport:{PortNumber}的，第层数{layerNumber+1}"

                        //};
                        if (PickOrPlace.Equals("Place"))
                        {
                            //RobotInfo.Operation = "放料";   
                            plcService.WriteShortVari($"D{5650 + layerNumber}", 0x43);
                        }
                        else
                        {
                            //RobotInfo.Operation = "取料";
                            plcService.WriteShortVari($"D{5650 + layerNumber}", 0x45);
                        }
                        break;
                    case 1:
                        //RobotPosition = new Point(90, 86);
                        //IsRobotVisible = true;
                        //RobotInfo = new RobotInfo()
                        //{
                        //    Title = "上下料机器人",
                        //    Position = $"loadport:{PortNumber}的，第层数{layerNumber+1}"

                        //};
                        if (PickOrPlace.Equals("Place"))
                        {
                            //RobotInfo.Operation = "放料";
                            plcService.WriteShortVari($"D{5700  +layerNumber}", 0x43);
                        }
                        else
                        {
                            //RobotInfo.Operation = "取料";
                            plcService.WriteShortVari($"D{5700 + layerNumber}", 0x45);
                        }
                        break;
                    case 2:
                        //RobotPosition = new Point(90, 163);
                        //IsRobotVisible = true;
                        //RobotInfo = new RobotInfo()
                        //{
                        //    Title = "上下料机器人",
                        //    Position = $"loadport:{PortNumber}的，第层数{layerNumber + 1}"

                        //};
                        if (PickOrPlace.Equals("Place"))
                        {
                            //RobotInfo.Operation = "放料";
                            plcService.WriteShortVari($"D{5750 + layerNumber}", 0x43);
                        }
                        else
                        {
                            //RobotInfo.Operation = "取料";
                            plcService.WriteShortVari($"D{5750 + layerNumber}", 0x45);
                        }
                        break;
                    case 3:
                        //RobotPosition = new Point(90, 238);
                        //IsRobotVisible = true;
                        //RobotInfo = new RobotInfo()
                        //{
                        //    Title = "上下料机器人",
                        //    Position = $"loadport:{PortNumber}的，第层数{layerNumber + 1}"

                        //};
                        if (PickOrPlace.Equals("Place"))
                        {
                            //RobotInfo.Operation = "放料";
                            plcService.WriteShortVari($"D{5800 + layerNumber}", 0x43);
                        }
                        else
                        {
                            //RobotInfo.Operation = "取料";
                            plcService.WriteShortVari($"D{5800 + layerNumber}", 0x45);
                        }
                        break;
                    case 4:
                        //RobotPosition = new Point(90, 313);
                        //IsRobotVisible = true;
                        //RobotInfo = new RobotInfo()
                        //{
                        //    Title = "上下料机器人",
                        //    Position = $"loadport:{PortNumber}的，第层数{layerNumber + 1}"

                        //};
                        if (PickOrPlace.Equals("Place"))
                        {
                            //RobotInfo.Operation = "放料";
                            plcService.WriteShortVari($"D{5850 + layerNumber}", 0x43);
                        }
                        else
                        {
                            //RobotInfo.Operation = "取料";
                            plcService.WriteShortVari($"D{5850 + layerNumber}", 0x45);
                        }
                        break;
                }
            
                var loadport = LoadPortManager.Instance.GetLoadPort($"COM{cassetteId + 12}");

                //FIEXD LOG: 当放料完成时记录追踪日志开始

                if ("Place".Equals(PickOrPlace))
                {
                    WriteTrackingLogWhenWaferReturnedToFoup(cassetteId, layerNumber);
                }
                //FIEXD LOG: 当放料完成时记录追踪日志结束
                Application.Current.Dispatcher?.BeginInvoke(new Action(() =>
                {
                    // 确保层号在有效范围内
                    if (layerNumber >=0 && layerNumber <= loadport.WaferList.Count)
                    {
                        // 由于WaferList是倒序的，需要转换索引
                        int waferIndex = loadport.WaferList.Count - (layerNumber+1);
                        if (waferIndex >= 0 && waferIndex < loadport.WaferList.Count)
                        {
                            var wafer = loadport.WaferList[waferIndex];
                            // 更新wafer状态为已取走
                            if ("Pick".Equals(PickOrPlace))
                            {
                                wafer.WaferStatus = WaferStatus.Processing; // 或其他表示已取走的颜色
                            }
                            else {
                                wafer.WaferStatus = WaferStatus.ProcessDone;
                                CaptureCassetteLogSnapshot(wafer, cassetteId, layerNumber);
               
                                int  index = cassetteId * 25 + (layerNumber + 1);
                                if (GlobalService.dicWaferIndex.ContainsKey(index))
                                {
                                    if (IsCycle)
                                    {
                                        //wafer.IsSelected = true;
                                        wafer.WaferStatus = WaferStatus.WaitAssignRecipe;
                                        Logger.Info("join lastWaferDic");
                                        //处理完成记录上一次处理的wafer信息
                                        if (_lastWaferDictionary.ContainsKey(index))
                                        {

                                            _lastWaferDictionary[index] = wafer;
                                        }
                                        else
                                        {
                                            _lastWaferDictionary.Add(index, wafer);
                                        }
                                    }                                    
                                    GlobalService.dicWaferIndex.Remove(index);
                                }
               

                            } 
                        }
                    }
                }));

                AddLogMessage($"Updated wafer status for cassette {cassetteId}-{PortNumber}, layer {layerNumber}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating wafer status for LoadPort {PortNumber}: {ex.Message}", ex, "loadport.txt", 0);
            }

            await Task.CompletedTask;
        }

        // 当接收到 inf:cldmp 反馈时调用
        private void OnInfCldmpReceived(bool readySignal)
        {  //wafer准备好
            switch (PortNumber) {

                case "COM12":
                    plcService.WriteBoolVari("M6879", readySignal);                   
                    break;
                case "COM13":
                    plcService.WriteBoolVari("M6895", readySignal);
                    break;
                case "COM14":
                    plcService.WriteBoolVari("M6911", readySignal);
                    break;
                case "COM15":
                    plcService.WriteBoolVari("M6927", readySignal);
                    break;
                case "COM16":
                    plcService.WriteBoolVari("M6943", readySignal);
                    break;
            }
           
                var maprd = _loadPortService.SendGetCommandAsync("MAPRD", _serialPort);

        }

        private void OnInfCulodReceived(bool readySignal)
        {  //wafer准备好
            switch (PortNumber)
            {

                case "COM12":
                    plcService.WriteBoolVari("M6879", readySignal);
                    break;
                case "COM13":
                    plcService.WriteBoolVari("M6895", readySignal);
                    break;
                case "COM14":
                    plcService.WriteBoolVari("M6911", readySignal);
                    break;
                case "COM15":
                    plcService.WriteBoolVari("M6927", readySignal);
                    break;
                case "COM16":
                    plcService.WriteBoolVari("M6943", readySignal);
                    break;
            }
            
                plcService.WriteBoolVari("M170", false);
                plcService.WriteBoolVari("M180", false);
            //GlobalService.waferQueue.Clear();
            var wafers = LoadPortManager.Instance.GetLoadPort(PortNumber).WaferList.ToList();

            WriteCassetteLogWhenFoupUnload();

            if (IsCycle)
            {
                _ = OnInfCulodCycleDelayAndContinueAsync(wafers);
                return;
            }

            ResetWafersToInitialState(wafers);
            //else
            //{
            //    ResetWafersToInitialState();
            //}
        }

        /// <summary>
        /// Cycle 模式下原逻辑在串口回调线程 Sleep 5s，会阻塞接收与 UI；改为异步延迟后在 UI 线程继续。
        /// </summary>
        private async Task OnInfCulodCycleDelayAndContinueAsync(List<Wafer> wafers)
        {
            try
            {
                await Task.Delay(5000).ConfigureAwait(false);
                var dispatcher = Application.Current?.Dispatcher;
                if (dispatcher == null)
                    return;
                await dispatcher.InvokeAsync(() =>
                {
                    Logger.Info("@ cycle status execute cldmp command@");
                    if (CurrentCycleCount == 0)
                    {
                        IsCycle = false;
                        CycleNum = 0;
                        AddLogMessage($"Cycle completed. Reached {CycleNum} cycles. Cycle mode disabled.");
                    }
                    else if (CurrentCycleCount <= CycleNum)
                    {
                        ExecuteMOVCommand("CLDMP");
                    }

                    ResetWafersToInitialState(wafers);
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"OnInfCulodCycleDelayAndContinueAsync failed for {PortNumber}: {ex.Message}", ex, "loadport.txt", 0);
            }
        }

        // 添加重置wafer到初始状态的方法
        private void ResetWafersToInitialState(List<Wafer> wafers)
        {
            try
            {
                Application.Current.Dispatcher?.BeginInvoke(new Action(() =>
                {
                    foreach (var wafer in wafers)
                    {
                        // 将wafer状态重置为初始状态
                        if (wafer.WaferStatus != WaferStatus.NonWafer)
                        {
                            //wafer.WaferStatus = WaferStatus.NonWafer;
                            if (!IsCycle)
                            {
                                wafer.WaferStatus = WaferStatus.NonWafer;
                                wafer.WaferName = string.Empty;
                                wafer.SequenceRecipe = string.Empty;
                                wafer.IsSelected = false;
                            }
                            wafer.CurrentStation = CurrentStation.Loadport;
                            // 重置其他相关属性到初始状态
                            
                            var index = wafer.LoadportID * 25 + (wafer.SlotID + 1);
                            if (GlobalService.dicWaferIndex.ContainsKey(index))
                            {
                                GlobalService.dicWaferIndex.Remove(index);
                            }

                            WaferPickQueue.RemoveWafer(wafer);
                        }
                    }

                    AddLogMessage($"Reset all wafers to initial state for LoadPort {PortNumber}");
                }));
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error resetting wafers to initial state: {ex.Message}");
                Logger.Error($"Error resetting wafers to initial state for LoadPort {PortNumber}: {ex.Message}", ex, "loadport.txt", 0);
            }
        }
        
        public string DockValue
        {
            get { return _dockValue; }
            set { SetProperty(ref _dockValue, value); }
        }
        public Visibility IsVisibility
        {
            get { return _isVisibility; }
            set { SetProperty(ref _isVisibility, value); }
        }
        public string FoupImageSource
        {
            get { return _foupImageSource; }
            set { SetProperty(ref _foupImageSource, value); }
        }

        public LoadPortStatus LoadPortStatus
        {
            get { return _loadPortStatus; }
            set { SetProperty(ref _loadPortStatus, value); }
        }

        public ObservableCollection<Wafer> WaferList
        {
            get { return _waferList; }
            set { SetProperty(ref _waferList, value); }
        }

        public ObservableCollection<string> CommandList
        {
            get { return _commandList; }
            set { SetProperty(ref _commandList, value); }
        }

        public string SelectedCommand
        {
            get { return _selectedCommand; }
            set { SetProperty(ref _selectedCommand, value); }
        }

        public string SelectedGetCommand
        {
            get { return _selectedGetCommand; }
            set { SetProperty(ref _selectedGetCommand, value); }
        }
        public string CommandParameter
        {
            get { return _commandParameter; }
            set { SetProperty(ref _commandParameter, value); }
        }

        public ObservableCollection<string> LogMessages
        {
            get { return _logMessages; }
            set { SetProperty(ref _logMessages, value); }
        }
        public bool IsConnected
        {
            get { return _isConnected; }
            set { SetProperty(ref _isConnected, value); }
        }
        public string PortNumber => _portName;

        public String TabName
        {
            get => _tabName;
            set => SetProperty(ref _tabName, value);
        }

            // public bool PlcIsConnected =false;
            // 控制命令
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand MOVCommand { get; }
        public ICommand SETCommand { get; }

        public ICommand GETCommand { get; }
        public LoadPortData(String portName, ILoadPortService loadPortService)
        {
            _portName = portName;
            _isConnected = false; // 初始状态为未连接
            _loadPortService = loadPortService;
            plcService = GlobalService.plcComm;
            // 初始化属性
            FoupImageSource = "/Assets/Images/foup.png"; // 示例路径
            WaferList = new ObservableCollection<Wafer>();
            LogMessages = new ObservableCollection<string>();
            CommandList = new ObservableCollection<string> { "RESET", "SAVE1", "LPCON", "PARAM" };
            int loadportID = -1;
            switch (portName)
            {
                case "COM12":
                    loadportID = 0;
                    TabName = "LPort1";
                    break;
                case "COM13":
                    loadportID = 1;
                    TabName = "LPort2";
                    break;
                case "COM14":
                    loadportID = 2;
                    TabName = "LPort3";
                    break;
                case "COM15":
                    loadportID = 3;
                    TabName = "LPort4";
                    break;
                case "COM16":
                    loadportID = 4;
                    TabName = "LPort5";
                    break;
            }
            FoupWorkflowState = LoadportFoupStatus.WaitPlace;

            // 初始化晶圆列表示例数据
            for (int i = 24; i >= 0; i--)
            {
                WaferList.Add(new Wafer
                {   LoadportID = loadportID,
                    SlotID = i,
                    WaferStatus =WaferStatus.NonWafer,
                    CurrentStation = CurrentStation.Loadport
                    
                });
            }
            // 初始化命令
            ConnectCommand = new DelegateCommand<string>(ExecuteConnectCommand);
            DisconnectCommand = new DelegateCommand<string>(ExecuteDisconnectCommand);
            MOVCommand = new DelegateCommand<string>(ExecuteMOVCommand, CanExecuteMOVCommand).ObservesProperty(() => IsConnected);
            SETCommand = new DelegateCommand<string>(ExecuteSETCommand, CanExecuteSETCommand).ObservesProperty(() => IsConnected);
            GETCommand = new DelegateCommand<string>(ExecuteGETCommand, CanExecuteGETCommand).ObservesProperty(() => IsConnected);
            _serialPort = new SerialPort
            {
                PortName = portName,
                BaudRate = 9600,
                DataBits = 8,
                StopBits = StopBits.One,
                Parity = Parity.None,
                Encoding = Encoding.ASCII,
                ReadTimeout = 5000,
                WriteTimeout = 2000
            };
            //plcService = _plcService;
            _serialPort.DataReceived += (sender, e) => SerialPort_DataReceived(sender, e);

            _plcOperationQueue = new ConcurrentQueue<PlcOperation>();
            _plcSemaphore = new SemaphoreSlim(1, 1); // 保证同一时间只有一个PLC操作
            _cancellationTokenSource = new CancellationTokenSource();


            initLoadPortRecipe();

            //PlcIsConnected = _plcService.IsConnected;
            // 启动PLC操作处理任务
            // StartPlcOperationProcessing();
        }

        // 启动PLC操作处理任务
        public void StartPlcOperationProcessing()
        {
            // 启动PLC取料监控任务
            StartPlcPickMonitoring();

            // 启动PLC放料监控任务
            StartPlcPlaceMonitoring();

            try
            {
                // 确保 cancellationTokenSource 已初始化
                if (_cancellationTokenSource == null)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                }

                // 如果任务已经在运行，避免重复启动
                if (_plcProcessingTask != null && !_plcProcessingTask.IsCompleted)
                {
                    return;
                }

                _plcProcessingTask = Task.Run(async () =>
                {
                    while (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        if (_plcOperationQueue.TryDequeue(out var operation))
                        {
                            await ProcessPlcOperation(operation);
                        }
                        else
                        {
                            try
                            {
                                await Task.Delay(10, _cancellationTokenSource.Token);
                            }
                            catch (OperationCanceledException)
                            {
                                // 任务被取消，正常退出循环
                                break;
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"Error starting PLC operation processing: {ex.Message}", ex, "loadport.txt", 0);
            }
        }

        private string _lastReceivedMessage = string.Empty; // 存储上一次接收到的消息

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {

                byte[] buf = new byte[1024];
                int read = 0;
                if ((read = _serialPort.BytesToRead) != -1)
                {

                    if (read > buf.Length)
                    {
                        read = buf.Length;
                    }
                    int read_count = _serialPort.Read(buf, 0, read);
                    if (read_count > 0)
                    {
                        byte[] data = new byte[read_count];
                        Array.Copy(buf, 0, data, 0, read);                        
                        string msg = System.Text.ASCIIEncoding.ASCII.GetString(data);
                        // 只有当消息与上一次不同时才添加到日志
                        if (msg != _lastReceivedMessage)
                        {
                            _lastReceivedMessage = msg; // 更新上一次的消息
                            AddLogMessage($"Received from LoadPort {_portName}: {msg}");
                        }
                        Logger.Info($" Received from LoadPort:{msg} ", "loadport.txt", 0);
                        var result = ParseResponse(data, msg);

                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error receiving data from LoadPort  {ex.Message}", ex, "loadport.txt", 0);
            }
        }
        // NAK错误代码描述字典
        private static readonly Dictionary<string, string> NAKOrABSErrorDic = new Dictionary<string, string>
        {
            { "CKSUM", "Checksum error - 校验和错误" },
            { "LENGT", "Length error - 长度错误" },
            { "CMDER", "Undefined command - 未定义的命令" },
            { "NORDY", "Initialization not executed - 初始化未执行" },
            { "ERROR", "Error status - 错误状态" },
            { "BUSY", "Busy - 设备忙" },
            { "NOMAP", "Wafer mapping not completed - 晶圆映射未完成" },
            { "PAUSE", "Pausing - 暂停中" },
            { "NOPSE", "Not paused - 未暂停" },
            { "OUTRG", "Parameter out of range - 参数超出范围" },
            { "NOMOV", "Not under operation - 不在操作中" },
            { "OFLIN", "Under manual operation - 手动操作模式" },
            { "SFSER", "Detect hand-caught/head-caught sensor - 检测到机械手/传感器故障" },
            { "KPIFL", "Kernel initialization fail - 内核初始化失败" },
            { "STATS", "Precondition incompatible, reference precondition table - 前提条件不兼容" },
            { "UMPCE", "Incorrect Placement - 放置错误" },
            { "UMIFP", "Incorrect InfoPad status - 信息面板状态错误" },
            { "MMGSF", "Motion mode group save fail - 运动模式组保存失败" },
            { "EVTSF", "Event enable/disable data save fail - 事件使能/禁用数据保存失败" },
            { "CDAER", "Pressure sensor detects positive pressure below 0.5Mpa - 压力感应器侦测正压低于0.5Mpa" },
            { "VACCS", "Vacuum ON timeout - 负压开启时，操作超时" },
            { "VACOS", "Vacuum OFF timeout - 负压关闭时，操作超时" },
            { "ZLMIT", "Z-axis motion timeout - Z轴动作超时" },
            { "MLMIT", "Mapping axis motion timeout - Mapping轴动作超时" },
            { "YLMIT", "Dock axis motion timeout - Dock轴动作超时" },
            { "YVSFL", "Dock axis motion timeout - Dock轴动作超时" },
            { "PROTS", "Detected protrusion - 侦测到凸片" },
            { "DLMIT", "Door axis motion timeout - Door轴动作超时" },
            { "MPZLM", "Z-axis move to upper scan position motion timeout - Z轴移动到上扫片位置动作超时" },
            { "MPEDL", "Z-axis move to lower scan position motion timeout - Z轴移动到下扫片位置动作超时" },
            { "CLOPS", "Clamp axis UnClamp motion timeout - Clamp轴UnClamp动作超时" },
            { "CLCLS", "Clamp axis Clamp motion timeout - Clamp轴Clamp动作超时" },
            { "DROPS", "Latch axis UnLatch motion timeout - Latch轴UnLatch动作超时" },
            { "DRCLS", "Latch axis Latch motion timeout - Latch轴Latch动作超时" },
            { "UNPCE", "Foup not properly placed - Foup未正确放置" },
            { "UNPRS", "Foup not properly placed - Foup未正确放置" },
            { "SAFTY", "Safety sensor triggered - 触发防夹手安全感应器" },
            { "EMOTR", "Emergency stop button triggered - 触发紧急停止按钮" }
        };
        private SerialResponseEventArgs ParseResponse(byte[] data, string msg)
        {
            try
            {
                // 解析命令和响应内容
                string command = "";
                string response = "";
                MessageType messageType = MessageType.Unknown;
                bool isSuccess = false;

                // 判断消息类型
                if (msg.Contains("ACK:"))
                {
                    messageType = MessageType.ACK;
                    isSuccess = true;
                }
                else if (msg.Contains("NAK:"))
                {
                    messageType = MessageType.NAK;
                    isSuccess = true;
                }
                else if (msg.Contains("INF:"))
                {
                    messageType = MessageType.INF;
                    isSuccess = true; // INF通常表示信息而非错误
                                      // 收到INF响应，清除等待状态
                    lock (_commandLock)
                    {
                        if (_waitingForInfResponse && _lastSentCommand == "MOV")
                        {
                            _waitingForInfResponse = false;
                            _lastSentCommand = string.Empty;
                        }
                    }
                }
                else if (msg.Contains("ABS:"))
                {
                    messageType = MessageType.ABS;
                    isSuccess = true; // 根据实际协议确定
                }

                // 根据实际协议，响应格式为：ACK:COMMAND/RESPONSE; 或 NAK:COMMAND/ERROR;
                // 查找冒号位置
                int colonIndex = msg.IndexOf(':');
                if (colonIndex > 0)
                {
                    // 查找斜杠位置
                    int slashIndex = msg.IndexOf('/', colonIndex);
                    // 查找分号位置
                    int semicolonIndex = msg.IndexOf(';', colonIndex);
                    if (semicolonIndex == -1) semicolonIndex = msg.Length;

                    // 提取命令部分
                    command = msg.Substring(colonIndex + 1,
                        (slashIndex > colonIndex && slashIndex < semicolonIndex ?
                         slashIndex : semicolonIndex) - colonIndex - 1);

                    // 提取响应部分
                    if (slashIndex > colonIndex && slashIndex < semicolonIndex)
                    {
                        response = msg.Substring(slashIndex + 1, semicolonIndex - slashIndex - 1);
                    }
                }

                // 创建响应事件参数
                var result = new SerialResponseEventArgs
                {
                    Command = command,
                    Response = response,
                    MessageType = messageType,
                    IsSuccess = isSuccess
                };

                // 根据命令类型解析并更新LoadPort状态
                if (messageType == MessageType.ABS || messageType == MessageType.NAK)
                {
                    //Fixed 收集loadport报警
                    if (NAKOrABSErrorDic.ContainsKey(response)) {
                        // 获取错误代码在字典中的索引
                        int errorIndex = NAKOrABSErrorDic.Keys.ToList().IndexOf(response);
                        int alarmId = 500 + errorIndex; // AlarmId以500+dic第一个元素CKSUM，以此类推到最后一个EMOTR

                        // 创建Alarm对象
                        var alarm = new Alarm
                        {
                            AlarmId = alarmId,
                            AlarmCode = response,
                            AlarmName = response,
                            Description = NAKOrABSErrorDic.ContainsKey(response) ? NAKOrABSErrorDic[response] : "Unknown error",
                            AlarmTime = DateTime.Now,
                            Type = messageType.ToString(),
                            IsAlarmed = true
                        };
                        //写到alarm表中
                        IAlarmService alarmService = new AlarmService();
                        alarmService.AddAlarm(alarm);
                        // 添加到DataCenter.AlarmList
                        GlobalService.DataCenter.listAlarms.Insert(0,alarm);

                    }


                }
                else
                {
                    ParseAndApplyLoadPortState(result);
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error parsing response: {ex.Message}", ex, "loadport.txt", 0);
                return null;
            }
        }
        private void ParseAndApplyLoadPortState(SerialResponseEventArgs response)
        {
            try
            {
                switch (response.Command)
                {
                    case "STATE":
                        ParseStateResponse(response.Response);
                        break;
                    case "MAPDT":
                        ParseMappingResponse(response.Response);
                        break;
                    // 可以添加其他命令的解析...
                    case "MAPRD":
                        ParseMappingResponse(response.Response);
                        break;
                    case "CLDMP":
                        // 仅在 INF 时处理，避免 ACK/INF 双触发
                        if (response.MessageType == MessageType.INF)
                        {
                            OnInfCldmpReceived(true);
                        }
                        break;
                    case "CULOD":
                        // 仅在 INF 时处理，避免 ACK/INF 双触发
                        if (response.MessageType == MessageType.INF)
                        {
                            OnInfCulodReceived(false);
                        }
                        break;
                    
                        // 可以添加其他命令的解析...
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error applying LoadPort state: {ex.Message}", ex, "loadport.txt", 0);
            }
        }        
        private void ParseMappingResponse(string response)
        {
            try
            {
                // 根据实际协议解析Mapping响应
                // 假设响应格式为: Slot1Data;Slot2Data;...;Slot25Data
                String[] slotData = response.Select(c => c.ToString()).ToArray(); ;
                //WaferList.Clear();
                if(slotData.Count()==25)UpdateWaferList(slotData);
            }
            catch (Exception ex)
            {
                Growl.ErrorGlobal($"Error parsing mapping response: {ex.Message}");
                Logger.Error($"Error parsing mapping response: {ex.Message}", ex, "loadport.txt", 0);
            }
        }
        private void ParseStateResponse(string response)
        {
            LoadPortStatus = new LoadPortStatus();
            try
            {
                // 根据实际协议解析状态响应 20位ASCII码
                // 示例格式：ACK:STATE/A1100010......010;
                var parts = response.Select(c => c.ToString()).ToArray();
                if (parts.Length >= 20)
                {
                    // 更新LoadPort状态（根据实际协议调整）
                    byte highByte = 0;
                    byte lowByte = 0;

                    if (!string.IsNullOrEmpty(parts[4]))
                    {
                        // 将ASCII字符转换为对应的字节值
                        if (parts[4].Length > 0)
                        {
                            highByte = (byte)parts[4][0];  // 取字符串的第一个字符的ASCII值
                        }
                    }

                    if (!string.IsNullOrEmpty(parts[5]))
                    {
                        // 将ASCII字符转换为对应的字节值
                        if (parts[5].Length > 0)
                        {
                            lowByte = (byte)parts[5][0];   // 取字符串的第一个字符的ASCII值
                        }
                    }

                    // 组合高低8位
                    LoadPortStatus.ErrorCode = (short)((highByte << 8) | lowByte);
                    //LoadPortStatus.ErrorCode = short.Parse(parts[5]);//parts[4],parts[5]大小写errorcode
                                                                     // 使用安全的枚举解析
                    if ("A".Equals(parts[0]))
                    {
                        LoadPortStatus.EquipmentStatus = EquipmentStatus.Error;
                    }
                    else
                    {
                        LoadPortStatus.EquipmentStatus = EquipmentStatus.NoError;
                    }

                    if ("0".Equals(parts[1]))
                    {
                        LoadPortStatus.Mode = Mode.HostMode;
                    }
                    else
                    {
                        LoadPortStatus.Mode = Mode.ManualMode;
                    }


                    if ("0".Equals(parts[2]))
                    {
                        LoadPortStatus.positionShift = InitialPositionShift.NotExecuted;
                    }
                    else
                    {
                        LoadPortStatus.positionShift = InitialPositionShift.Executed;
                    }
                    if ("0".Equals(parts[3]))
                    {
                        LoadPortStatus.operationStatus = OperationStatus.StoppedStatus;
                    }
                    else
                    {
                        LoadPortStatus.operationStatus = OperationStatus.Busy;
                    }

                    if ("0".Equals(parts[6]))
                    {
                        LoadPortStatus.foupPlacementStatus = FoupPlacementStatus.NoExistence;
                        IsVisibility = Visibility.Hidden;
                    }
                    else if ("1".Equals(parts[6]))
                    {
                        LoadPortStatus.foupPlacementStatus = FoupPlacementStatus.Normal;
                        IsVisibility = Visibility.Visible;
                    }
                    else
                    {
                        LoadPortStatus.foupPlacementStatus = FoupPlacementStatus.Error;
                    }

                    if ("0".Equals(parts[7]))
                    {

                        LoadPortStatus.FoupClampStatus = FoupClampStatus.Unclamp;
                    }
                    else if ("1".Equals(parts[7]))
                    {

                        LoadPortStatus.FoupClampStatus = FoupClampStatus.Clamp;
                    }
                    else
                    {

                        LoadPortStatus.FoupClampStatus = FoupClampStatus.Unknown;
                    }

                    if ("0".Equals(parts[8]))
                    {

                        LoadPortStatus.DoorLatchStatus = DoorLatchStatus.Unlatch;
                    }
                    else if ("1".Equals(parts[8]))
                    {

                        LoadPortStatus.DoorLatchStatus = DoorLatchStatus.Latch;
                    }
                    else
                    {

                        LoadPortStatus.DoorLatchStatus = DoorLatchStatus.Unknown;
                    }

                    if ("0".Equals(parts[9]))
                    {
                        LoadPortStatus.Vacuum = Vacuum.NoVacuum;
                    }
                    else
                    {
                        LoadPortStatus.Vacuum = Vacuum.Vacuum;
                    }

                    if ("0".Equals(parts[10]))
                    {
                        LoadPortStatus.FIMSDoorStatus = FIMSDoorStatus.Close;
                    }
                    else if ("1".Equals(parts[10]))
                    {
                        LoadPortStatus.FIMSDoorStatus = FIMSDoorStatus.Open;
                    }
                    else
                    {
                        LoadPortStatus.FIMSDoorStatus = FIMSDoorStatus.Unknown;
                    }


                    if ("0".Equals(parts[11]))
                    {
                        LoadPortStatus.WaferOutSensor = WaferOutSensor.WaferOutDetected;
                    }
                    else
                    {
                        LoadPortStatus.WaferOutSensor = WaferOutSensor.NotDetected;
                    }

                    LoadPortStatus.Reserve = short.Parse(parts[12]);//13,17,18,19

                    if ("0".Equals(parts[13]))
                    {
                        LoadPortStatus.FoupDockStatus = FoupDockStatus.Undocking;
                        DockValue = "15";
                    }
                    else if ("1".Equals(parts[13]))
                    {
                        LoadPortStatus.FoupDockStatus = FoupDockStatus.Docking;
                        DockValue = "30";
                    }
                    else
                    {
                        LoadPortStatus.FoupDockStatus = FoupDockStatus.Unknown;
                    }


                    if ("0".Equals(parts[14]))
                    {
                        LoadPortStatus.MapperArmPosition = MapperArmPosition.Retract;
                    }
                    else if ("1".Equals(parts[14]))
                    {
                        LoadPortStatus.MapperArmPosition = MapperArmPosition.Extend;
                    }
                    else
                    {
                        LoadPortStatus.MapperArmPosition = MapperArmPosition.Unknown;
                    }

                    if ("0".Equals(parts[15]))
                    {
                        LoadPortStatus.ZAxisPosition = ZAxisPosition.UpperEnd;
                    }
                    else if ("1".Equals(parts[15]))
                    {
                        LoadPortStatus.ZAxisPosition = ZAxisPosition.LowerEnd;
                    }
                    else if ("2".Equals(parts[15]))
                    {
                        LoadPortStatus.ZAxisPosition = ZAxisPosition.MPUpperEnd;
                    }
                    else if ("3".Equals(parts[15]))
                    {
                        LoadPortStatus.ZAxisPosition = ZAxisPosition.MPLowerEnd;
                    }
                    else
                    {
                        LoadPortStatus.ZAxisPosition = ZAxisPosition.Unknown;
                    }

                    if ("0".Equals(parts[19]))
                    {
                        LoadPortStatus.InfoPadStatus = InfoPadStatus.Null;
                    }
                    else if ("1".Equals(parts[19]))
                    {
                        LoadPortStatus.InfoPadStatus = InfoPadStatus.PadA;
                    }
                    else if ("2".Equals(parts[19]))
                    {
                        LoadPortStatus.InfoPadStatus = InfoPadStatus.PadB;
                    }
                    else if ("3".Equals(parts[19]))
                    {
                        LoadPortStatus.InfoPadStatus = InfoPadStatus.PadAB;
                    }
                    else if ("4".Equals(parts[19]))
                    {
                        LoadPortStatus.InfoPadStatus = InfoPadStatus.PadC;
                    }
                    else if ("5".Equals(parts[19]))
                    {
                        LoadPortStatus.InfoPadStatus = InfoPadStatus.PadAC;
                    }
                    else if ("6".Equals(parts[19]))
                    {
                        LoadPortStatus.InfoPadStatus = InfoPadStatus.PadBC;
                    }
                    else if ("7".Equals(parts[19]))
                    {
                        LoadPortStatus.InfoPadStatus = InfoPadStatus.PadABC;
                    }
                    else if ("8".Equals(parts[19]))
                    {
                        LoadPortStatus.InfoPadStatus = InfoPadStatus.PadD;
                    }
                    else if ("9".Equals(parts[19]))
                    {
                        LoadPortStatus.InfoPadStatus = InfoPadStatus.PadAD;
                    }
                    else if ("A".Equals(parts[19]))
                    {
                        LoadPortStatus.InfoPadStatus = InfoPadStatus.PadBD;
                    }
                    else if ("B".Equals(parts[19]))
                    {
                        LoadPortStatus.InfoPadStatus = InfoPadStatus.PadABD;
                    }
                    else if ("C".Equals(parts[19]))
                    {
                        LoadPortStatus.InfoPadStatus = InfoPadStatus.PadCD;
                    }
                    else if ("D".Equals(parts[19]))
                    {
                        LoadPortStatus.InfoPadStatus = InfoPadStatus.PadACD;
                    }
                    else if ("E".Equals(parts[19]))
                    {
                        LoadPortStatus.InfoPadStatus = InfoPadStatus.PadBCD;
                    }
                    else
                    {
                        LoadPortStatus.InfoPadStatus = InfoPadStatus.PadABCD;
                    }

                    var prevWorkflow = _foupWorkflowState;
                    var (nextWorkflow, enteredUnload) = LoadPortFoupStatusStateMachine.Transition(LoadPortStatus, prevWorkflow);
                    if (nextWorkflow != prevWorkflow)
                        FoupWorkflowState = nextWorkflow;
                    if (enteredUnload)
                        OnFoupUnloadClearPlcPickAllow();

                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error parsing state response: {ex.Message}", ex, "loadport.txt", 0);
            }
        }
        private bool CanExecuteMOVCommand(string parameter)
        {
            return IsConnected; // 只有在连接状态下才能执行MOV命令
        }

        private bool CanExecuteSETCommand(string parameter)
        {
            return IsConnected; // 只有在连接状态下才能执行SET命令
        }

        private bool CanExecuteGETCommand(string arg)
        {
            lock (_commandLock)
            {
                return IsConnected && !_waitingForInfResponse;
            }
            
        }

        private void ExecuteConnectCommand(string COMNumber)
        {
            try
            {
                bool isOpen = _serialPort.IsOpen;
                if (isOpen)
                {
                    _serialPort.Close();
                }

                _serialPort.Open();  // 设置为已连接状态                
                IsConnected = true;
               // StartAutoUpdate();//连接成功后自动获取loadport状态与wafer映射
                AddLogMessage($"Connect command executed for LoadPort {_portName} Success");
                Logger.Info($"Connect command executed for LoadPort {_portName} Success", "loadport.txt", 0);
              
            }
            catch (Exception ex)
            {
                AddLogMessage($"Connect command executed for LoadPort {_portName} Error");
                Logger.Error($"Connect command executed for LoadPort {_portName} Error", ex, "loadport.txt", 0);
               // Growl.ErrorGlobal($"Connect command executed for LoadPort {_portName} Error");
            }


        }

        private void ExecuteDisconnectCommand(string COMNumber)
        {
            try
            {
                bool isOpen = _serialPort.IsOpen;
                if (isOpen)
                {
                    _serialPort.Close();
                }

                IsConnected = false;  // 设置为已连接状态

                AddLogMessage($"Disconnect command executed for LoadPort {_portName} Success");
                Logger.Info($"DissConnect command executed for LoadPort {_portName} Success", "loadport.txt", 0);
            }
            catch (Exception ex)
            {
                AddLogMessage($"DissConnect command executed for LoadPort {_portName} Error");
                Logger.Error($"DissConnect command executed for LoadPort {_portName} Error", ex, "loadport.txt", 0);
            }

        }

        private void ExecuteMOVCommand(string parameter)
        {
            try
            {
                if (_serialPort != null)
                {
                    lock (_commandLock)
                    {
                        _waitingForInfResponse = true;
                        _lastSentCommand = "MOV";
                    }
                    _loadPortService.SendMoveCommandAsync(parameter, _serialPort);
                    AddLogMessage($"MOV command '{parameter}' executed for LoadPort {_portName}");

                    // 当执行 CLDMP 命令时增加 Cycle 计数
                    if (parameter == "CLDMP" && IsCycle)
                    {

                        CurrentCycleCount += 1;
                        // 检查是否达到设定的 Cycle 次数
                        if (CurrentCycleCount >= CycleNum && CycleNum > 0)
                        {
                            // 自动结束 Cycle 模式
                            IsCycle = false;
                           
                            AddLogMessage($"Cycle completed. Reached {CycleNum} cycles. Cycle mode disabled.");
                        }
                        else
                        {
                            
                        }
                            
                        AddLogMessage($"Cycle count incremented to {CurrentCycleCount}");
                    }
                    // 启动超时检查，防止无限等待INF响应
                    Task.Run(async () => {
                        await Task.Delay(TimeSpan.FromSeconds(10)); // 10秒超时
                        lock (_commandLock)
                        {
                            if (_waitingForInfResponse && _lastSentCommand == "MOV")
                            {
                                _waitingForInfResponse = false;
                                _lastSentCommand = string.Empty;
                                AddLogMessage($"Timeout waiting for INF response for MOV command on LoadPort {_portName}");
                            }
                        }
                    });
                }
                else
                {
                    AddLogMessage("Loadport is not available, please connect first");
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"Failed to send command to LoadPort-{PortNumber} : {ex.Message}";
                AddLogMessage(errorMsg);
                Logger.Error(errorMsg, ex, "loadport.txt", 0);
                // 发生异常时清除等待状态
                lock (_commandLock)
                {
                    _waitingForInfResponse = false;
                    _lastSentCommand = string.Empty;
                }
            }

        }
        private void ExecuteGETCommand(string obj)
        {
            try
            {
                if (_serialPort != null)
                {
                    _loadPortService.SendGetCommandAsync(SelectedGetCommand, _serialPort);
                    AddLogMessage($"Set command :{SelectedGetCommand} executed for LoadPort {_portName}");
                }
                else
                {
                    AddLogMessage("Loadport is not available, please connect first");
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"Failed to send command to LoadPort-{PortNumber} : {ex.Message}";
                AddLogMessage(errorMsg);
                Logger.Error(errorMsg, ex, "loadport.txt", 0);
            }
        }

        private void ExecuteSETCommand(string parameter)
        {
            try
            {
                if (_serialPort != null)
                {
                    if(!string.IsNullOrEmpty(parameter)) SelectedCommand = parameter;
                    _loadPortService.SendSetCommandAsync(SelectedCommand, CommandParameter, _serialPort);
                    AddLogMessage($"Set command '{SelectedCommand}'parameter ：{CommandParameter} executed for LoadPort {_portName}");
                }
                else
                {
                    AddLogMessage("Loadport is not available, please connect first");
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"Failed to send command to LoadPort-{PortNumber} : {ex.Message}";
                AddLogMessage(errorMsg);
                Logger.Error(errorMsg, ex, "loadport.txt", 0);
            }

            //AddLogMessage($"SET command executed for LoadPort {_portNumber} with parameter: {SelectedCommand}/{CommandParameter}");
            // 在这里添加实际的SET命令逻辑
        }

        public void AddLogMessage(string message)
        {

            lock (_logLock)
            {
                // 使用Dispatcher确保在UI线程上执行
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    LogMessages.Insert(0,$"[{DateTime.Now:HH:mm:ss}] {message}");
                    // 保持日志数量在合理范围内
                    if (LogMessages.Count > 100)
                    {
                        LogMessages.RemoveAt(LogMessages.Count-1);
                    }
                }));
            }
           
        }
        private void UpdateWaferList(string[] slotData)
        {
            lock (_waferLock)
            {
               var loadport =  LoadPortManager.Instance.GetLoadPort(PortNumber);
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    for (int i = 0; i < slotData.Length; i++)
                    {
                        char statusChar = Convert.ToChar(slotData[i]);
                        string slotNumber = (i).ToString();
                            //倒序对应
                            int waferIndex = 24 - i;
                            if (waferIndex >= 0 && waferIndex < loadport.WaferList.Count)
                            {
                                //load wafer init isSelected，sequenceName,dicWafer,WaferQueue
                                var wafer = loadport.WaferList[waferIndex];                                
                                if (string.IsNullOrEmpty(wafer.SequenceRecipe))
                                {
                                    wafer.WaferStatus = _loadPortService.MapCharToWaferStatus(statusChar);
                                    wafer.IsSelected = false;
                                }
                                else
                                {
                                    if (IsCycle)
                                    {
                                        wafer.WaferStatus = WaferStatus.WaitAssignRecipe;
                                        wafer.IsSelected = true;
                                    }
                                   
                                }



                            }
                        
                    }
                }));

                if (IsCycle)
                {
                    // 在下一个UI循环中恢复WaferQueue，避免阻塞当前处理
                    Task.Run(() =>
                    {
                        Task.Delay(100); // 短暂延迟确保当前处理完成
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            
                            SyncWaferQueueWithLastDictionary();
                            // 同步完成后初始化_lastWaferDictionary
                            _lastWaferDictionary.Clear();
                        }));
                    });
                }
            }
           
        }

        // 新增：启动自动更新的方法
        public void StartAutoUpdate()
        {
            // 如果已有正在运行的任务，先取消它
            StopAutoUpdate();

            // 创建新的 CancellationTokenSource
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            // 启动后台任务
            Task.Run(async () => await AutoUpdateLoopAsync(token), token);
            
          
        }

        // 新增：停止自动更新的方法
        public void StopAutoUpdate()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }

            // 停止PLC放料监控
            StopPlcPlaceMonitoring();
            //停止取料
            StopPlcPickMonitoring();
        }

        // 新增：自动更新循环
        private async Task AutoUpdateLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    bool shouldUpdate = true;
                    lock (_commandLock)
                    {
                        if (_waitingForInfResponse) shouldUpdate = false;
                    }
                    // 执行状态和映射更新
                    if (shouldUpdate) await UpdateStatusAndMappingAsync(cancellationToken);
                    
                    // 等待30秒或直到取消
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // 这是预期的取消异常，可以忽略
                Logger.Info($"Auto update cancelled for LoadPort {_portName}", "loadport.txt", 0);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in auto update loop for LoadPort {_portName}: {ex.Message}", ex, "loadport.txt", 0);
            }
            finally
            {
                // 清理资源
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        // 修改：更新状态和映射的方法，支持取消
        private async Task UpdateStatusAndMappingAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (IsConnected && _serialPort != null && !cancellationToken.IsCancellationRequested)
                {
                    // 发送获取状态命令
                    await _loadPortService.SendGetCommandAsync("STATE",_serialPort);

                    // 检查取消状态
                    cancellationToken.ThrowIfCancellationRequested();

                    // 发送获取映射命令
                   // await _loadPortService.SendGetCommandAsync("MAPRD", _serialPort);
                }
            }
            catch (OperationCanceledException)
            {
                // 重新抛出取消异常
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating status and mapping for LoadPort {_portName}: {ex.Message}", ex, "loadport.txt", 0);
            }
        }

        // 修改：在析构或清理时确保取消所有任务
        public void Dispose()
        {
            StopAutoUpdate();

            // 其他清理代码...
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
            _serialPort?.Dispose();
        }

        #region cycle功能
        // 上一次执行的Wafer字典（在LoadPortData类中维护）
        private Dictionary<int, Wafer> _lastWaferDictionary = new Dictionary<int, Wafer>();
        private bool _isCycle;
        private int _cycleNum;
        public int CycleNum
        {
            get => _cycleNum;
            set => SetProperty(ref _cycleNum, value);
        }

        // 添加 Cycle 计数器
        private int _currentCycleCount = 0;
        public int CurrentCycleCount
        {
            get => _currentCycleCount;
            set => SetProperty(ref _currentCycleCount, value);
        }
        public bool IsCycle
        {
            get { return _isCycle; }
            set
            {
                if (SetProperty(ref _isCycle, value))
                {
                    OnIsCycleChanged();
                }
            }
        }
        /// <summary>
        /// 当IsCycle属性改变时触发的处理方法
        /// </summary>
        /// <returns></returns>
        private void OnIsCycleChanged()
        {
            _lastWaferDictionary.Clear();
            // 当 Cycle 模式关闭时重置当前 Cycle 计数
            if (!_isCycle)
            {
                CurrentCycleCount = 0;
                AddLogMessage("Cycle mode disabled. Cycle counter reset to 0.");
            }
            else
            {
                CurrentCycleCount = 0;
                AddLogMessage($"Cycle mode enabled. Will run for {CycleNum} cycles.");
            }
        }
        
        private void SyncWaferQueueWithLastDictionary()
        {
            try
            {
                if (!IsCycle)
                {
                    return;
                }

                var loadportId = GetLoadportId();
                if (loadportId < 0)
                {
                    return;
                }

                var wafersToSync = new List<Wafer>();

                foreach (var kvp in _lastWaferDictionary.OrderBy(k => k.Key))
                {
                    int key = kvp.Key;
                    Wafer lastWafer = kvp.Value;

                    int keyLoadportId = key / 25;
                    int slotID = (key % 25) - 1;

                    if (keyLoadportId != loadportId || slotID < 0 || slotID >= 25)
                    {
                        continue;
                    }

                    int waferIndex = 24 - slotID;
                    if (waferIndex < 0 || waferIndex >= WaferList.Count)
                    {
                        continue;
                    }

                    var slotWafer = WaferList[waferIndex];
                    if (slotWafer.LoadportID != keyLoadportId || slotWafer.SlotID != slotID)
                    {
                        continue;
                    }

                    lastWafer.CurrentStation = CurrentStation.Loadport;
                    var index = keyLoadportId * 25 + (slotID + 1);
                    if (GlobalService.dicWaferIndex.ContainsKey(index))
                    {
                        GlobalService.dicWaferIndex[index] = lastWafer;
                    }
                    else
                    {
                        GlobalService.dicWaferIndex.Add(index, lastWafer);
                    }

                    wafersToSync.Add(lastWafer);
                    Logger.Info($"Updated wafer L{lastWafer.LoadportID}S{lastWafer.SlotID} status to WaitAssignRecipe");
                }

                WaferPickQueue.ReplaceLoadportWafers(loadportId, wafersToSync);

                AddLogMessage($"Sync completed for LoadPort {PortNumber}. {wafersToSync.Count} wafers in queue from last cycle. Total queue: {WaferPickQueue.Count}. Current cycle: {CurrentCycleCount}/{CycleNum}");
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error syncing wafer queue with last dictionary: {ex.Message}");
                Logger.Error($"Error syncing wafer queue with last dictionary for LoadPort {PortNumber}: {ex.Message}", ex, "loadport.txt", 0);
            }
        }
        #endregion
    }
}
