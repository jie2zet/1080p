using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;
using System.Windows.Threading;
using CMP.Constants.Plc.RobotControl;
using CMP.Services.Log;
using CMP.Services.Plc;
using CMP.ViewModels.Common;
using Prism.Commands;
using WPFLocalizeExtension.Extensions;
using CMP.Services.Auth;

namespace CMP.ViewModels.Tabs.OperationTab.RobotControlTab;

public class TransferModuleControlViewModel : TabContentViewModelBase
{
    private readonly MitsubishiPlcService _plc;
    private readonly DispatcherTimer _readTimer;
    private bool _isReading;

    private string? _tabTitleKey;
    public string? TabTitleKey
    {
        get => _tabTitleKey;
        set
        {
            if (SetProperty(ref _tabTitleKey, value))
                RefreshTabTitle();
        }
    }

    public void RefreshTabTitle()
    {
        if (!string.IsNullOrEmpty(TabTitleKey))
        {
            var loc = new LocExtension($"CMP:Resources:{TabTitleKey}");
            loc.ResolveLocalizedValue(out string? localizedValue);
            TabTitle = localizedValue ?? TabTitleKey;
        }
    }

    private string? _tabTitle;
    public new string? TabTitle
    {
        get => _tabTitle ?? base.TabTitle;
        set => SetProperty(ref _tabTitle, value);
    }

    #region 轴位置属性

    public double PositionMinimum { get; set; } = -90000;
    public double PositionMaximum { get; set; } = 90000;

    private readonly EditableFloatItem _loadPositionItem = new(TransferModuleAddress.LoadPosition, nameof(LoadPosition), nameof(IsLoadPositionDirty));
    private readonly EditableFloatItem _unloadPositionItem = new(TransferModuleAddress.UnloadPosition, nameof(UnloadPosition), nameof(IsUnloadPositionDirty));
    private readonly EditableFloatItem _sparePositionItem = new(TransferModuleAddress.SparePosition, nameof(SparePosition), nameof(IsSparePositionDirty));
    private readonly EditableFloatItem _jogSpeedItem = new(TransferModuleAddress.JogSpeed, nameof(JogSpeed), nameof(IsJogSpeedDirty));
    private readonly EditableFloatItem _autoSpeedItem = new(TransferModuleAddress.AutoSpeed, nameof(AutoSpeed), nameof(IsAutoSpeedDirty));
    private readonly List<EditableFloatItem> _editableItems;

    public bool IsLoadPositionDirty => _loadPositionItem.IsDirty;

    public double LoadPosition
    {
        get => _loadPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_loadPositionItem, value, RaisePropertyChanged);
    }

    public bool IsUnloadPositionDirty => _unloadPositionItem.IsDirty;

    public double UnloadPosition
    {
        get => _unloadPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_unloadPositionItem, value, RaisePropertyChanged);
    }

    public bool IsSparePositionDirty => _sparePositionItem.IsDirty;

    public double SparePosition
    {
        get => _sparePositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_sparePositionItem, value, RaisePropertyChanged);
    }

    #endregion

    #region Move Toggle 属性 (只读，通过 Command 写入)

    private bool _loadPositionMoveToggle;
    public bool LoadPositionMoveToggle
    {
        get => _loadPositionMoveToggle;
        set => SetProperty(ref _loadPositionMoveToggle, value);
    }

    private bool _unloadPositionMoveToggle;
    public bool UnloadPositionMoveToggle
    {
        get => _unloadPositionMoveToggle;
        set => SetProperty(ref _unloadPositionMoveToggle, value);
    }

    private bool _sparePositionMoveToggle;
    public bool SparePositionMoveToggle
    {
        get => _sparePositionMoveToggle;
        set => SetProperty(ref _sparePositionMoveToggle, value);
    }

    #endregion

    #region 到位信号属性

    private bool _loadPositionArrived;
    public bool LoadPositionArrived
    {
        get => _loadPositionArrived;
        set => SetProperty(ref _loadPositionArrived, value);
    }

    private bool _unloadPositionArrived;
    public bool UnloadPositionArrived
    {
        get => _unloadPositionArrived;
        set => SetProperty(ref _unloadPositionArrived, value);
    }

    private bool _sparePositionArrived;
    public bool SparePositionArrived
    {
        get => _sparePositionArrived;
        set => SetProperty(ref _sparePositionArrived, value);
    }

    #endregion

    #region 速度和坐标属性

    public bool IsJogSpeedDirty => _jogSpeedItem.IsDirty;

    public double JogSpeed
    {
        get => _jogSpeedItem.Value;
        set => EditableFloatItemHelper.SetValue(_jogSpeedItem, value, RaisePropertyChanged);
    }

    public bool IsAutoSpeedDirty => _autoSpeedItem.IsDirty;

    public double AutoSpeed
    {
        get => _autoSpeedItem.Value;
        set => EditableFloatItemHelper.SetValue(_autoSpeedItem, value, RaisePropertyChanged);
    }

    private double _currentCoordinate;
    public double CurrentCoordinate
    {
        get => _currentCoordinate;
        set => SetProperty(ref _currentCoordinate, value);
    }

    #endregion

    #region 状态属性

    private bool _isEnabled;
    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }

    private bool _driverAlarm;
    public bool DriverAlarm
    {
        get => _driverAlarm;
        set => SetProperty(ref _driverAlarm, value);
    }

    private bool _limitAlarm;
    public bool LimitAlarm
    {
        get => _limitAlarm;
        set => SetProperty(ref _limitAlarm, value);
    }

    private bool _isAtOrigin;
    public bool IsAtOrigin
    {
        get => _isAtOrigin;
        set => SetProperty(ref _isAtOrigin, value);
    }

    private int? _alarmCode;
    public int? AlarmCode
    {
        get => _alarmCode;
        set => SetProperty(ref _alarmCode, value);
    }

    #endregion

    #region Jog 控制属性

    private bool _jogUpActive;
    public bool JogUpActive
    {
        get => _jogUpActive;
        set => SetProperty(ref _jogUpActive, value);
    }

    private bool _jogMiddleActive;
    public bool JogMiddleActive
    {
        get => _jogMiddleActive;
        set => SetProperty(ref _jogMiddleActive, value);
    }

    private bool _jogDownActive;
    public bool JogDownActive
    {
        get => _jogDownActive;
        set => SetProperty(ref _jogDownActive, value);
    }

    private bool _originToggle;
    public bool OriginToggle
    {
        get => _originToggle;
        set => SetProperty(ref _originToggle, value);
    }

    #endregion

    #region 底部按钮属性 (只读，通过 Command 写入)

    private bool _emergencyStopToggle;
    public bool EmergencyStopToggle
    {
        get => _emergencyStopToggle;
        set => SetProperty(ref _emergencyStopToggle, value);
    }

    private bool _alarmResetToggle;
    public bool AlarmResetToggle
    {
        get => _alarmResetToggle;
        set => SetProperty(ref _alarmResetToggle, value);
    }

    private bool _saveToggle;
    public bool SaveToggle
    {
        get => _saveToggle;
        set => SetProperty(ref _saveToggle, value);
    }

    #endregion

    #region Station Operation 属性

    private int _currentViewIndex;
    public int CurrentViewIndex
    {
        get => _currentViewIndex;
        set => SetProperty(ref _currentViewIndex, value);
    }

    private bool _stationIsReset;
    public bool StationIsReset
    {
        get => _stationIsReset;
        set => SetProperty(ref _stationIsReset, value);
    }

    private bool _stationIsRunning;
    public bool StationIsRunning
    {
        get => _stationIsRunning;
        set => SetProperty(ref _stationIsRunning, value);
    }

    private bool _stationIsStopped;
    public bool StationIsStopped
    {
        get => _stationIsStopped;
        set => SetProperty(ref _stationIsStopped, value);
    }

    private bool _loadInduction;
    public bool LoadInduction
    {
        get => _loadInduction;
        set => SetProperty(ref _loadInduction, value);
    }

    private bool _unloadInduction;
    public bool UnloadInduction
    {
        get => _unloadInduction;
        set => SetProperty(ref _unloadInduction, value);
    }

    private bool _loadOperationActive;
    public bool LoadOperationActive
    {
        get => _loadOperationActive;
        set => SetProperty(ref _loadOperationActive, value);
    }

    private bool _loadOperationDone;
    public bool LoadOperationDone
    {
        get => _loadOperationDone;
        set => SetProperty(ref _loadOperationDone, value);
    }

    private bool _loadOperationToggle;
    public bool LoadOperationToggle
    {
        get => _loadOperationToggle;
        set => SetProperty(ref _loadOperationToggle, value);
    }

    private bool _unloadOperationActive;
    public bool UnloadOperationActive
    {
        get => _unloadOperationActive;
        set => SetProperty(ref _unloadOperationActive, value);
    }

    private bool _unloadOperationDone;
    public bool UnloadOperationDone
    {
        get => _unloadOperationDone;
        set => SetProperty(ref _unloadOperationDone, value);
    }

    private bool _unloadOperationToggle;
    public bool UnloadOperationToggle
    {
        get => _unloadOperationToggle;
        set => SetProperty(ref _unloadOperationToggle, value);
    }

    private bool _moduleAlarm;
    public bool ModuleAlarm
    {
        get => _moduleAlarm;
        set => SetProperty(ref _moduleAlarm, value);
    }

    private bool _globalAlarm;
    public bool GlobalAlarm
    {
        get => _globalAlarm;
        set => SetProperty(ref _globalAlarm, value);
    }

    private bool _manualModeToggle;
    public bool ManualModeToggle
    {
        get => _manualModeToggle;
        set => SetProperty(ref _manualModeToggle, value);
    }

    private bool _autoModeToggle;
    public bool AutoModeToggle
    {
        get => _autoModeToggle;
        set => SetProperty(ref _autoModeToggle, value);
    }

    #endregion

    #region Door Control 属性 (只读，通过 Command 写入)

    private bool _loadDoorOpenToggle;
    public bool LoadDoorOpenToggle
    {
        get => _loadDoorOpenToggle;
        set => SetProperty(ref _loadDoorOpenToggle, value);
    }

    private bool _loadDoorOpenDone;
    public bool LoadDoorOpenDone
    {
        get => _loadDoorOpenDone;
        set => SetProperty(ref _loadDoorOpenDone, value);
    }

    private bool _loadDoorCloseToggle;
    public bool LoadDoorCloseToggle
    {
        get => _loadDoorCloseToggle;
        set => SetProperty(ref _loadDoorCloseToggle, value);
    }

    private bool _loadDoorCloseDone;
    public bool LoadDoorCloseDone
    {
        get => _loadDoorCloseDone;
        set => SetProperty(ref _loadDoorCloseDone, value);
    }

    private bool _sideDoorOpenToggle;
    public bool SideDoorOpenToggle
    {
        get => _sideDoorOpenToggle;
        set => SetProperty(ref _sideDoorOpenToggle, value);
    }

    private bool _sideDoorOpenDone;
    public bool SideDoorOpenDone
    {
        get => _sideDoorOpenDone;
        set => SetProperty(ref _sideDoorOpenDone, value);
    }

    private bool _sideDoorCloseToggle;
    public bool SideDoorCloseToggle
    {
        get => _sideDoorCloseToggle;
        set => SetProperty(ref _sideDoorCloseToggle, value);
    }

    private bool _sideDoorCloseDone;
    public bool SideDoorCloseDone
    {
        get => _sideDoorCloseDone;
        set => SetProperty(ref _sideDoorCloseDone, value);
    }

    private bool _unloadDoorOpenToggle;
    public bool UnloadDoorOpenToggle
    {
        get => _unloadDoorOpenToggle;
        set => SetProperty(ref _unloadDoorOpenToggle, value);
    }

    private bool _unloadDoorOpenDone;
    public bool UnloadDoorOpenDone
    {
        get => _unloadDoorOpenDone;
        set => SetProperty(ref _unloadDoorOpenDone, value);
    }

    private bool _unloadDoorCloseToggle;
    public bool UnloadDoorCloseToggle
    {
        get => _unloadDoorCloseToggle;
        set => SetProperty(ref _unloadDoorCloseToggle, value);
    }

    private bool _unloadDoorCloseDone;
    public bool UnloadDoorCloseDone
    {
        get => _unloadDoorCloseDone;
        set => SetProperty(ref _unloadDoorCloseDone, value);
    }

    #endregion

    #region Commands

    public DelegateCommand StationOperationCommand { get; }
    public DelegateCommand BackCommand { get; }
    public DelegateCommand StationResetCommand { get; }
    public DelegateCommand StationStartCommand { get; }
    public DelegateCommand StationStopCommand { get; }
    public DelegateCommand StationAlarmResetCommand { get; }
    public DelegateCommand PressJogUpCommand { get; }
    public DelegateCommand ReleaseJogUpCommand { get; }
    public DelegateCommand PressJogDownCommand { get; }
    public DelegateCommand ReleaseJogDownCommand { get; }

    // Toggle Button Commands
    public DelegateCommand LoadPositionMoveCommand { get; }
    public DelegateCommand UnloadPositionMoveCommand { get; }
    public DelegateCommand SparePositionMoveCommand { get; }
    public DelegateCommand EnableCommand { get; }
    public DelegateCommand OriginCommand { get; }
    public DelegateCommand EmergencyStopCommand { get; }
    public DelegateCommand AlarmResetCommand { get; }
    public DelegateCommand SaveCommand { get; }
    public DelegateCommand LoadOperationCommand { get; }
    public DelegateCommand UnloadOperationCommand { get; }
    public DelegateCommand ManualModeCommand { get; }
    public DelegateCommand AutoModeCommand { get; }
    public DelegateCommand LoadDoorOpenCommand { get; }
    public DelegateCommand LoadDoorCloseCommand { get; }
    public DelegateCommand SideDoorOpenCommand { get; }
    public DelegateCommand SideDoorCloseCommand { get; }
    public DelegateCommand UnloadDoorOpenCommand { get; }
    public DelegateCommand UnloadDoorCloseCommand { get; }

    public DelegateCommand LoadPositionSetCommand => _loadPositionItem.SetCommand;
    public DelegateCommand LoadPositionCancelCommand => _loadPositionItem.CancelCommand;
    public DelegateCommand UnloadPositionSetCommand => _unloadPositionItem.SetCommand;
    public DelegateCommand UnloadPositionCancelCommand => _unloadPositionItem.CancelCommand;
    public DelegateCommand SparePositionSetCommand => _sparePositionItem.SetCommand;
    public DelegateCommand SparePositionCancelCommand => _sparePositionItem.CancelCommand;
    public DelegateCommand JogSpeedSetCommand => _jogSpeedItem.SetCommand;
    public DelegateCommand JogSpeedCancelCommand => _jogSpeedItem.CancelCommand;
    public DelegateCommand AutoSpeedSetCommand => _autoSpeedItem.SetCommand;
    public DelegateCommand AutoSpeedCancelCommand => _autoSpeedItem.CancelCommand;

    #endregion

    public TransferModuleControlViewModel() : base(0, "传送模组")
    {
        _plc = GlobalService.RobotControlPlc;
        _editableItems = new List<EditableFloatItem>
        {
            _loadPositionItem,
            _unloadPositionItem,
            _sparePositionItem,
            _jogSpeedItem,
            _autoSpeedItem
        };
        EditableFloatItemHelper.InitializeCommands(_editableItems, _plc, RaisePropertyChanged);

        _readTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _readTimer.Tick += ReadTimer_Tick;
        RegisterLifecycleTimer(_readTimer);
        RegisterViewLifecycleAction(
            () => Logger.For("RobotControl").Debug("TransferModuleControlViewModel activated, started PLC read timer."),
            () => Logger.For("RobotControl").Debug("TransferModuleControlViewModel deactivated, stopped PLC read timer."));

        // Navigation Commands
        StationOperationCommand = new DelegateCommand(ExecuteStationOperation);
        BackCommand = new DelegateCommand(ExecuteBack);

        // Station Commands
        StationResetCommand = OperationAudit.CreateAsync("StationReset", ExecuteStationReset, () => "TransferModule");
        StationStartCommand = OperationAudit.CreateAsync("StationStart", ExecuteStationStart, () => "TransferModule");
        StationStopCommand = OperationAudit.CreateAsync("StationStop", ExecuteStationStop, () => "TransferModule");
        StationAlarmResetCommand = OperationAudit.CreateAsync("StationAlarmReset", ExecuteStationAlarmReset, () => "TransferModule");

        // Jog Commands
        PressJogUpCommand = new DelegateCommand(ExecutePressJogUp);
        ReleaseJogUpCommand = new DelegateCommand(ExecuteReleaseJogUp);
        PressJogDownCommand = new DelegateCommand(ExecutePressJogDown);
        ReleaseJogDownCommand = new DelegateCommand(ExecuteReleaseJogDown);

        // Toggle Button Commands - 写入当前状态的反值
        LoadPositionMoveCommand = OperationAudit.Create("LoadPositionMove", () => _plc.Write(TransferModuleAddress.LoadPositionMove, !LoadPositionMoveToggle), () => "TransferModule");
        UnloadPositionMoveCommand = OperationAudit.Create("UnloadPositionMove", () => _plc.Write(TransferModuleAddress.UnloadPositionMove, !UnloadPositionMoveToggle), () => "TransferModule");
        SparePositionMoveCommand = OperationAudit.Create("SparePositionMove", () => _plc.Write(TransferModuleAddress.SparePositionMove, !SparePositionMoveToggle), () => "TransferModule");
        EnableCommand = OperationAudit.Create("Enable", () => _plc.ToggleBool(TransferModuleAddress.Enable), () => "TransferModule");
        OriginCommand = OperationAudit.Create("Origin", () =>
        {
            if (MessageBox.Show(Lang.Resources.ConfirmActionContent, Lang.Resources.ConfirmActionTips,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _plc.Write(TransferModuleAddress.Origin, !OriginToggle);
            }

            OriginToggle = false;
        }, () => "TransferModule");
        EmergencyStopCommand = OperationAudit.Create("EmergencyStop", () => _plc.Write(TransferModuleAddress.EmergencyStop, !EmergencyStopToggle), () => "TransferModule");
        AlarmResetCommand = OperationAudit.CreateAsync("AlarmReset", () => _plc.PulseAsync(TransferModuleAddress.AlarmReset), () => "TransferModule");
        SaveCommand = OperationAudit.Create("Save", () => _plc.Write(TransferModuleAddress.Save, !SaveToggle), () => "TransferModule");
        LoadOperationCommand = OperationAudit.CreateAsync("LoadOperation", () => _plc.PulseAsync(TransferModuleAddress.LoadOperation), () => "TransferModule");
        UnloadOperationCommand = OperationAudit.CreateAsync("UnloadOperation", () => _plc.PulseAsync(TransferModuleAddress.UnloadOperation), () => "TransferModule");
        ManualModeCommand = OperationAudit.CreateAsync("ManualMode", () => _plc.PulseAsync(TransferModuleAddress.ManualMode), () => "TransferModule");
        AutoModeCommand = OperationAudit.CreateAsync("AutoMode", () => _plc.PulseAsync(TransferModuleAddress.AutoMode), () => "TransferModule");
        LoadDoorOpenCommand = OperationAudit.CreateAsync("LoadDoorOpen", () => _plc.PulseAsync(TransferModuleAddress.LoadDoorOpenWrite), () => "TransferModule");
        LoadDoorCloseCommand = OperationAudit.CreateAsync("LoadDoorClose", () => _plc.PulseAsync(TransferModuleAddress.LoadDoorCloseWrite), () => "TransferModule");
        SideDoorOpenCommand = OperationAudit.CreateAsync("SideDoorOpen", () => _plc.PulseAsync(TransferModuleAddress.SideDoorOpenWrite), () => "TransferModule");
        SideDoorCloseCommand = OperationAudit.CreateAsync("SideDoorClose", () => _plc.PulseAsync(TransferModuleAddress.SideDoorCloseWrite), () => "TransferModule");
        UnloadDoorOpenCommand = OperationAudit.CreateAsync("UnloadDoorOpen", () => _plc.PulseAsync(TransferModuleAddress.UnloadDoorOpenWrite), () => "TransferModule");
        UnloadDoorCloseCommand = OperationAudit.CreateAsync("UnloadDoorClose", () => _plc.PulseAsync(TransferModuleAddress.UnloadDoorCloseWrite), () => "TransferModule");
    }

    #region PLC 读取

    private async void ReadTimer_Tick(object? sender, EventArgs e)
    {
        if (_isReading)
        {
            return;
        }

        if (!_plc.IsConnected && !MitsubishiPlcService.DebugMode) return;

        _isReading = true;

        try
        {
            await ReadPlcDataAsync();
        }
        catch (Exception ex)
        {
            Logger.For(Logger.Category.RobotControl).Error("Error reading PLC data", ex);
        }
        finally
        {
            _isReading = false;
        }
    }

    private async Task ReadPlcDataAsync()
    {
        var mBatchItems = new List<PlcBatchReadItemBase>
        {
            new PlcBatchReadItem<bool>(TransferModuleAddress.LoadPositionMove, assign: v => LoadPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(TransferModuleAddress.UnloadPositionMove, assign: v => UnloadPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(TransferModuleAddress.SparePositionMove, assign: v => SparePositionMoveToggle = v),

            new PlcBatchReadItem<bool>(TransferModuleAddress.LoadPositionArrived, assign: v => LoadPositionArrived = v),
            new PlcBatchReadItem<bool>(TransferModuleAddress.UnloadPositionArrived, assign: v => UnloadPositionArrived = v),
            new PlcBatchReadItem<bool>(TransferModuleAddress.SparePositionArrived, assign: v => SparePositionArrived = v),

            new PlcBatchReadItem<bool>(TransferModuleAddress.EnableStatus, assign: v => IsEnabled = v),
            new PlcBatchReadItem<bool>(TransferModuleAddress.OriginStatus, assign: v => IsAtOrigin = v),
            new PlcBatchReadItem<bool>(TransferModuleAddress.Origin, assign: v => OriginToggle = v),

            new PlcBatchReadItem<bool>(TransferModuleAddress.JogUpStatus, assign: v => JogUpActive = v),
            new PlcBatchReadItem<bool>(TransferModuleAddress.JogMiddleStatus, assign: v => JogMiddleActive = v),
            new PlcBatchReadItem<bool>(TransferModuleAddress.JogDownStatus, assign: v => JogDownActive = v),

            new PlcBatchReadItem<bool>(TransferModuleAddress.DriverAlarm, assign: v => DriverAlarm = v),
            new PlcBatchReadItem<bool>(TransferModuleAddress.LimitAlarm, assign: v => LimitAlarm = v),
            new PlcBatchReadItem<bool>(TransferModuleAddress.EmergencyStop, assign: v => EmergencyStopToggle = v),
            new PlcBatchReadItem<bool>(TransferModuleAddress.Save, assign: v => SaveToggle = v),

            new PlcBatchReadItem<bool>(TransferModuleAddress.StationResetStatus, assign: v => StationIsReset = v),
            new PlcBatchReadItem<bool>(TransferModuleAddress.StationRunningStatus, assign: v => StationIsRunning = v),
            new PlcBatchReadItem<bool>(TransferModuleAddress.StationStoppedStatus, assign: v => StationIsStopped = v),

            new PlcBatchReadItem<bool>(TransferModuleAddress.LoadOperationActiveStatus, assign: v => LoadOperationActive = v),
            new PlcBatchReadItem<bool>(TransferModuleAddress.LoadOperationDoneStatus, assign: v => LoadOperationDone = v),
            new PlcBatchReadItem<bool>(TransferModuleAddress.LoadOperation, assign: v => LoadOperationToggle = v),
            new PlcBatchReadItem<bool>(TransferModuleAddress.UnloadOperationActiveStatus, assign: v => UnloadOperationActive = v),
            new PlcBatchReadItem<bool>(TransferModuleAddress.UnloadOperationDoneStatus, assign: v => UnloadOperationDone = v),
            new PlcBatchReadItem<bool>(TransferModuleAddress.UnloadOperation, assign: v => UnloadOperationToggle = v),

            new PlcBatchReadItem<bool>(TransferModuleAddress.ModuleAlarm, assign: v => ModuleAlarm = v),
            new PlcBatchReadItem<bool>(TransferModuleAddress.GlobalAlarm, assign: v => GlobalAlarm = v),
            new PlcBatchReadItem<bool>(TransferModuleAddress.ManualMode, assign: v => ManualModeToggle = v),
            new PlcBatchReadItem<bool>(TransferModuleAddress.AutoMode, assign: v => AutoModeToggle = v)
        };

        var dStatusBatchItems = new List<PlcBatchReadItemBase>
        {
            new PlcBatchReadItem<int>(TransferModuleAddress.CurrentCoordinate, assign: v => CurrentCoordinate = v / 1000.0),
            new PlcBatchReadItem<int>(TransferModuleAddress.AlarmCode, assign: v => AlarmCode = v)
        };

        var editableBatchItems = _editableItems
            .Select(item => (PlcBatchReadItemBase)new PlcBatchReadItem<int>(item.Address, assign: v =>
            {
                item.PlcValue = v / 1000.0;
                item.HasPlcValue = true;

                if (item.IsDirty)
                {
                    return;
                }

                item.Value = v / 1000.0;
                RaisePropertyChanged(item.ValuePropertyName);
            }))
            .ToList();

        var mBatchTask = _plc.BatchReadAsync(mBatchItems);
        var dStatusBatchTask = _plc.BatchReadAsync(dStatusBatchItems);
        var editableBatchTask = _plc.BatchReadAsync(editableBatchItems);
        var y10C0Task = _plc.ReadAsync<bool[]>(TransferModuleAddress.LoadDoorOpenRead, 6); // Y10C0-Y10C5
        var x10B0Task = _plc.ReadAsync<bool[]>(TransferModuleAddress.LoadDoorOpenDoneStatus, 6); // X10B0-X10B5
        var x102cTask = _plc.ReadAsync<bool[]>(TransferModuleAddress.LoadInduction, 2); // X102C-X102D
        var b0BTask = _plc.ReadAsync<bool>(TransferModuleAddress.AlarmReset); // B0B

        await Task.WhenAll(mBatchTask, dStatusBatchTask, editableBatchTask, y10C0Task, x10B0Task, x102cTask, b0BTask);

        if (!mBatchTask.Result.IsSuccess)
            Logger.For("RobotControl").Warn($"[TransferModule] M batch read failed: {mBatchTask.Result.Message}");

        if (!dStatusBatchTask.Result.IsSuccess)
            Logger.For("RobotControl").Warn($"[TransferModule] D status batch read failed: {dStatusBatchTask.Result.Message}");

        if (!editableBatchTask.Result.IsSuccess)
            Logger.For("RobotControl").Warn($"[TransferModule] D editable batch read failed: {editableBatchTask.Result.Message}");

        if (y10C0Task.Result.IsSuccess)
        {
            var y = y10C0Task.Result.Content;
            if (y.Length >= 6)
            {
                LoadDoorOpenToggle = y[0];
                LoadDoorCloseToggle = y[1];
                SideDoorOpenToggle = y[2];
                SideDoorCloseToggle = y[3];
                UnloadDoorOpenToggle = y[4];
                UnloadDoorCloseToggle = y[5];
            }
        }

        if (x10B0Task.Result.IsSuccess)
        {
            var x = x10B0Task.Result.Content;
            if (x.Length >= 6)
            {
                LoadDoorOpenDone = x[0];
                LoadDoorCloseDone = x[1];
                SideDoorOpenDone = x[2];
                SideDoorCloseDone = x[3];
                UnloadDoorOpenDone = x[4];
                UnloadDoorCloseDone = x[5];
            }
        }

        if (x102cTask.Result.IsSuccess && x102cTask.Result.Content.Length >= 2)
        {
            LoadInduction = x102cTask.Result.Content[0];
            UnloadInduction = x102cTask.Result.Content[1];
        }

        if (b0BTask.Result.IsSuccess)
            AlarmResetToggle = b0BTask.Result.Content;
    }

    public void StopReading()
    {
        OnViewDeactivated();
    }

    #endregion

    #region Command 实现

    private void ExecuteStationOperation() => CurrentViewIndex = 1;

    private void ExecuteBack() => CurrentViewIndex = 0;

    private Task ExecuteStationReset() => _plc.PulseAsync(TransferModuleAddress.StationReset);

    private Task ExecuteStationStart() => _plc.PulseAsync(TransferModuleAddress.StationStart);

    private Task ExecuteStationStop() => _plc.PulseAsync(TransferModuleAddress.StationStop);

    private Task ExecuteStationAlarmReset() => _plc.PulseAsync(TransferModuleAddress.StationAlarmReset);

    private void ExecutePressJogUp() => _plc.Write(TransferModuleAddress.JogUp, true);

    private void ExecuteReleaseJogUp() => _plc.Write(TransferModuleAddress.JogUp, false);

    private void ExecutePressJogDown() => _plc.Write(TransferModuleAddress.JogDown, true);

    private void ExecuteReleaseJogDown() => _plc.Write(TransferModuleAddress.JogDown, false);

    #endregion
}

