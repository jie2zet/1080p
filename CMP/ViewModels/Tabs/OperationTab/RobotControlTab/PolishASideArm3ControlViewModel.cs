using System.Windows.Threading;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;
using CMP.Constants.Plc.RobotControl;
using CMP.Services.Log;
using CMP.Services.Plc;
using CMP.ViewModels.Common;
using WPFLocalizeExtension.Extensions;
using CMP.Services.Auth;

namespace CMP.ViewModels.Tabs.OperationTab.RobotControlTab;

public class PolishASideArm3ControlViewModel : TabContentViewModelBase
{
    private readonly MitsubishiPlcService _plc;
    private readonly DispatcherTimer _readTimer;
    private bool _isReading;
    private readonly List<EditableFloatItem> _editableItems;

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

    public double PositionMinimum { get; set; } = -90000;
    public double PositionMaximum { get; set; } = 90000;

    private readonly EditableFloatItem _xStandbyPositionItem = new(PolishASideArm3Address.XStandbyPosition, nameof(XStandbyPosition), nameof(IsXStandbyPositionDirty));
    private readonly EditableFloatItem _xTransferArmPositionItem = new(PolishASideArm3Address.XTransferArmPosition, nameof(XTransferArmPosition), nameof(IsXTransferArmPositionDirty));
    private readonly EditableFloatItem _xPolishAPositionItem = new(PolishASideArm3Address.XPolishAPosition, nameof(XPolishAPosition), nameof(IsXPolishAPositionDirty));
    private readonly EditableFloatItem _xPolishBPositionItem = new(PolishASideArm3Address.XPolishBPosition, nameof(XPolishBPosition), nameof(IsXPolishBPositionDirty));
    private readonly EditableFloatItem _xJogSpeedItem = new(PolishASideArm3Address.XJogSpeed, nameof(XJogSpeed), nameof(IsXJogSpeedDirty));
    private readonly EditableFloatItem _xAutoSpeedItem = new(PolishASideArm3Address.XAutoSpeed, nameof(XAutoSpeed), nameof(IsXAutoSpeedDirty));
    private readonly EditableFloatItem _zStandbyPositionItem = new(PolishASideArm3Address.ZStandbyPosition, nameof(ZStandbyPosition), nameof(IsZStandbyPositionDirty));
    private readonly EditableFloatItem _zTransferArmPositionItem = new(PolishASideArm3Address.ZTransferArmPosition, nameof(ZTransferArmPosition), nameof(IsZTransferArmPositionDirty));
    private readonly EditableFloatItem _zPolishAPositionItem = new(PolishASideArm3Address.ZPolishAPosition, nameof(ZPolishAPosition), nameof(IsZPolishAPositionDirty));
    private readonly EditableFloatItem _zPolishBPositionItem = new(PolishASideArm3Address.ZPolishBPosition, nameof(ZPolishBPosition), nameof(IsZPolishBPositionDirty));
    private readonly EditableFloatItem _zJogSpeedItem = new(PolishASideArm3Address.ZJogSpeed, nameof(ZJogSpeed), nameof(IsZJogSpeedDirty));
    private readonly EditableFloatItem _zAutoSpeedItem = new(PolishASideArm3Address.ZAutoSpeed, nameof(ZAutoSpeed), nameof(IsZAutoSpeedDirty));

    #region X-Axis Position Properties

    public bool IsXStandbyPositionDirty => _xStandbyPositionItem.IsDirty;

    public double XStandbyPosition
    {
        get => _xStandbyPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_xStandbyPositionItem, value, RaisePropertyChanged);
    }

    public bool IsXTransferArmPositionDirty => _xTransferArmPositionItem.IsDirty;

    public double XTransferArmPosition
    {
        get => _xTransferArmPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_xTransferArmPositionItem, value, RaisePropertyChanged);
    }

    public bool IsXPolishAPositionDirty => _xPolishAPositionItem.IsDirty;

    public double XPolishAPosition
    {
        get => _xPolishAPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_xPolishAPositionItem, value, RaisePropertyChanged);
    }

    public bool IsXPolishBPositionDirty => _xPolishBPositionItem.IsDirty;

    public double XPolishBPosition
    {
        get => _xPolishBPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_xPolishBPositionItem, value, RaisePropertyChanged);
    }

    #endregion

    #region X-Axis Move Toggle Properties (Read-Only via PLC)

    private bool _xStandbyPositionMoveToggle;
    public bool XStandbyPositionMoveToggle
    {
        get => _xStandbyPositionMoveToggle;
        set => SetProperty(ref _xStandbyPositionMoveToggle, value);
    }

    private bool _xTransferArmPositionMoveToggle;
    public bool XTransferArmPositionMoveToggle
    {
        get => _xTransferArmPositionMoveToggle;
        set => SetProperty(ref _xTransferArmPositionMoveToggle, value);
    }

    private bool _xPolishAPositionMoveToggle;
    public bool XPolishAPositionMoveToggle
    {
        get => _xPolishAPositionMoveToggle;
        set => SetProperty(ref _xPolishAPositionMoveToggle, value);
    }

    private bool _xPolishBPositionMoveToggle;
    public bool XPolishBPositionMoveToggle
    {
        get => _xPolishBPositionMoveToggle;
        set => SetProperty(ref _xPolishBPositionMoveToggle, value);
    }

    #endregion

    #region X-Axis Speed and Status

    public bool IsXJogSpeedDirty => _xJogSpeedItem.IsDirty;

    public double XJogSpeed
    {
        get => _xJogSpeedItem.Value;
        set => EditableFloatItemHelper.SetValue(_xJogSpeedItem, value, RaisePropertyChanged);
    }

    public bool IsXAutoSpeedDirty => _xAutoSpeedItem.IsDirty;

    public double XAutoSpeed
    {
        get => _xAutoSpeedItem.Value;
        set => EditableFloatItemHelper.SetValue(_xAutoSpeedItem, value, RaisePropertyChanged);
    }

    private double _xCurrentCoordinate;
    public double XCurrentCoordinate
    {
        get => _xCurrentCoordinate;
        set => SetProperty(ref _xCurrentCoordinate, value);
    }

    private bool _xIsEnabled;
    public bool XIsEnabled
    {
        get => _xIsEnabled;
        set => SetProperty(ref _xIsEnabled, value);
    }

    private bool _xJogUpActive;
    public bool XJogUpActive
    {
        get => _xJogUpActive;
        set => SetProperty(ref _xJogUpActive, value);
    }

    private bool _xJogMiddleActive;
    public bool XJogMiddleActive
    {
        get => _xJogMiddleActive;
        set => SetProperty(ref _xJogMiddleActive, value);
    }

    private bool _xJogDownActive;
    public bool XJogDownActive
    {
        get => _xJogDownActive;
        set => SetProperty(ref _xJogDownActive, value);
    }

    private bool _xIsAtOrigin;
    public bool XIsAtOrigin
    {
        get => _xIsAtOrigin;
        set => SetProperty(ref _xIsAtOrigin, value);
    }

    private bool _xOriginToggle;
    public bool XOriginToggle
    {
        get => _xOriginToggle;
        set => SetProperty(ref _xOriginToggle, value);
    }

    private bool _xDriverAlarm;
    public bool XDriverAlarm
    {
        get => _xDriverAlarm;
        set => SetProperty(ref _xDriverAlarm, value);
    }

    private bool _xLimitAlarm;
    public bool XLimitAlarm
    {
        get => _xLimitAlarm;
        set => SetProperty(ref _xLimitAlarm, value);
    }

    private int? _xAlarmCode;
    public int? XAlarmCode
    {
        get => _xAlarmCode;
        set => SetProperty(ref _xAlarmCode, value);
    }

    #endregion

    #region Z-Axis Position Properties

    public bool IsZStandbyPositionDirty => _zStandbyPositionItem.IsDirty;

    public double ZStandbyPosition
    {
        get => _zStandbyPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_zStandbyPositionItem, value, RaisePropertyChanged);
    }

    public bool IsZTransferArmPositionDirty => _zTransferArmPositionItem.IsDirty;

    public double ZTransferArmPosition
    {
        get => _zTransferArmPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_zTransferArmPositionItem, value, RaisePropertyChanged);
    }

    public bool IsZPolishAPositionDirty => _zPolishAPositionItem.IsDirty;

    public double ZPolishAPosition
    {
        get => _zPolishAPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_zPolishAPositionItem, value, RaisePropertyChanged);
    }

    public bool IsZPolishBPositionDirty => _zPolishBPositionItem.IsDirty;

    public double ZPolishBPosition
    {
        get => _zPolishBPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_zPolishBPositionItem, value, RaisePropertyChanged);
    }

    #endregion

    #region Z-Axis Move Toggle Properties (Read-Only via PLC)

    private bool _zStandbyPositionMoveToggle;
    public bool ZStandbyPositionMoveToggle
    {
        get => _zStandbyPositionMoveToggle;
        set => SetProperty(ref _zStandbyPositionMoveToggle, value);
    }

    private bool _zTransferArmPositionMoveToggle;
    public bool ZTransferArmPositionMoveToggle
    {
        get => _zTransferArmPositionMoveToggle;
        set => SetProperty(ref _zTransferArmPositionMoveToggle, value);
    }

    private bool _zPolishAPositionMoveToggle;
    public bool ZPolishAPositionMoveToggle
    {
        get => _zPolishAPositionMoveToggle;
        set => SetProperty(ref _zPolishAPositionMoveToggle, value);
    }

    private bool _zPolishBPositionMoveToggle;
    public bool ZPolishBPositionMoveToggle
    {
        get => _zPolishBPositionMoveToggle;
        set => SetProperty(ref _zPolishBPositionMoveToggle, value);
    }

    private bool _zStandbyPositionArrived;
    public bool ZStandbyPositionArrived
    {
        get => _zStandbyPositionArrived;
        set => SetProperty(ref _zStandbyPositionArrived, value);
    }

    private bool _zTransferArmPositionArrived;
    public bool ZTransferArmPositionArrived
    {
        get => _zTransferArmPositionArrived;
        set => SetProperty(ref _zTransferArmPositionArrived, value);
    }

    private bool _zPolishAPositionArrived;
    public bool ZPolishAPositionArrived
    {
        get => _zPolishAPositionArrived;
        set => SetProperty(ref _zPolishAPositionArrived, value);
    }

    private bool _zPolishBPositionArrived;
    public bool ZPolishBPositionArrived
    {
        get => _zPolishBPositionArrived;
        set => SetProperty(ref _zPolishBPositionArrived, value);
    }

    #endregion

    #region Z-Axis Speed and Status

    public bool IsZJogSpeedDirty => _zJogSpeedItem.IsDirty;

    public double ZJogSpeed
    {
        get => _zJogSpeedItem.Value;
        set => EditableFloatItemHelper.SetValue(_zJogSpeedItem, value, RaisePropertyChanged);
    }

    public bool IsZAutoSpeedDirty => _zAutoSpeedItem.IsDirty;

    public double ZAutoSpeed
    {
        get => _zAutoSpeedItem.Value;
        set => EditableFloatItemHelper.SetValue(_zAutoSpeedItem, value, RaisePropertyChanged);
    }

    private double _zCurrentCoordinate;
    public double ZCurrentCoordinate
    {
        get => _zCurrentCoordinate;
        set => SetProperty(ref _zCurrentCoordinate, value);
    }

    private bool _zIsEnabled;
    public bool ZIsEnabled
    {
        get => _zIsEnabled;
        set => SetProperty(ref _zIsEnabled, value);
    }

    private bool _zJogUpActive;
    public bool ZJogUpActive
    {
        get => _zJogUpActive;
        set => SetProperty(ref _zJogUpActive, value);
    }

    private bool _zJogMiddleActive;
    public bool ZJogMiddleActive
    {
        get => _zJogMiddleActive;
        set => SetProperty(ref _zJogMiddleActive, value);
    }

    private bool _zJogDownActive;
    public bool ZJogDownActive
    {
        get => _zJogDownActive;
        set => SetProperty(ref _zJogDownActive, value);
    }

    private bool _zIsAtOrigin;
    public bool ZIsAtOrigin
    {
        get => _zIsAtOrigin;
        set => SetProperty(ref _zIsAtOrigin, value);
    }

    private bool _zOriginToggle;
    public bool ZOriginToggle
    {
        get => _zOriginToggle;
        set => SetProperty(ref _zOriginToggle, value);
    }

    private bool _zDriverAlarm;
    public bool ZDriverAlarm
    {
        get => _zDriverAlarm;
        set => SetProperty(ref _zDriverAlarm, value);
    }

    private bool _zLimitAlarm;
    public bool ZLimitAlarm
    {
        get => _zLimitAlarm;
        set => SetProperty(ref _zLimitAlarm, value);
    }

    private int? _zAlarmCode;
    public int? ZAlarmCode
    {
        get => _zAlarmCode;
        set => SetProperty(ref _zAlarmCode, value);
    }

    #endregion

    #region Bottom Bar Properties

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

    #region Station Operation Properties

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

    private bool _reserveActive;
    public bool ReserveActive
    {
        get => _reserveActive;
        set => SetProperty(ref _reserveActive, value);
    }

    private bool _reserveDone;
    public bool ReserveDone
    {
        get => _reserveDone;
        set => SetProperty(ref _reserveDone, value);
    }

    private bool _reserveToggle;
    public bool ReserveToggle
    {
        get => _reserveToggle;
        set => SetProperty(ref _reserveToggle, value);
    }

    private bool _pickFromPolishAActive;
    public bool PickFromPolishAActive
    {
        get => _pickFromPolishAActive;
        set => SetProperty(ref _pickFromPolishAActive, value);
    }

    private bool _pickFromPolishADone;
    public bool PickFromPolishADone
    {
        get => _pickFromPolishADone;
        set => SetProperty(ref _pickFromPolishADone, value);
    }

    private bool _pickFromPolishAToggle;
    public bool PickFromPolishAToggle
    {
        get => _pickFromPolishAToggle;
        set => SetProperty(ref _pickFromPolishAToggle, value);
    }

    private bool _pickFromPolishA2Active;
    public bool PickFromPolishA2Active
    {
        get => _pickFromPolishA2Active;
        set => SetProperty(ref _pickFromPolishA2Active, value);
    }

    private bool _pickFromPolishA2Done;
    public bool PickFromPolishA2Done
    {
        get => _pickFromPolishA2Done;
        set => SetProperty(ref _pickFromPolishA2Done, value);
    }

    private bool _pickFromPolishA2Toggle;
    public bool PickFromPolishA2Toggle
    {
        get => _pickFromPolishA2Toggle;
        set => SetProperty(ref _pickFromPolishA2Toggle, value);
    }

    private bool _placeToTransferArmActive;
    public bool PlaceToTransferArmActive
    {
        get => _placeToTransferArmActive;
        set => SetProperty(ref _placeToTransferArmActive, value);
    }

    private bool _placeToTransferArmDone;
    public bool PlaceToTransferArmDone
    {
        get => _placeToTransferArmDone;
        set => SetProperty(ref _placeToTransferArmDone, value);
    }

    private bool _placeToTransferArmToggle;
    public bool PlaceToTransferArmToggle
    {
        get => _placeToTransferArmToggle;
        set => SetProperty(ref _placeToTransferArmToggle, value);
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

    // Sensors
    private bool _arm3Fiber1;
    public bool Arm3Fiber1
    {
        get => _arm3Fiber1;
        set => SetProperty(ref _arm3Fiber1, value);
    }

    private bool _arm3Fiber2;
    public bool Arm3Fiber2
    {
        get => _arm3Fiber2;
        set => SetProperty(ref _arm3Fiber2, value);
    }

    private bool _arm3Fiber3;
    public bool Arm3Fiber3
    {
        get => _arm3Fiber3;
        set => SetProperty(ref _arm3Fiber3, value);
    }

    private int _foupSlot;
    public int FoupSlot
    {
        get => _foupSlot;
        set => SetProperty(ref _foupSlot, value);
    }

    private int _waferIndex;
    public int WaferIndex
    {
        get => _waferIndex;
        set => SetProperty(ref _waferIndex, value);
    }

    #endregion

    #region Commands

    public DelegateCommand StationOperationCommand { get; }
    public DelegateCommand BackCommand { get; }
    public DelegateCommand StationResetCommand { get; }
    public DelegateCommand StationStartCommand { get; }
    public DelegateCommand StationStopCommand { get; }
    public DelegateCommand StationAlarmResetCommand { get; }

    // Jog Commands
    public DelegateCommand XPressJogUpCommand { get; }
    public DelegateCommand XReleaseJogUpCommand { get; }
    public DelegateCommand XPressJogDownCommand { get; }
    public DelegateCommand XReleaseJogDownCommand { get; }
    public DelegateCommand ZPressJogUpCommand { get; }
    public DelegateCommand ZReleaseJogUpCommand { get; }
    public DelegateCommand ZPressJogDownCommand { get; }
    public DelegateCommand ZReleaseJogDownCommand { get; }

    // Toggle Button Commands
    public DelegateCommand XStandbyPositionMoveCommand { get; }
    public DelegateCommand XTransferArmPositionMoveCommand { get; }
    public DelegateCommand XPolishAPositionMoveCommand { get; }
    public DelegateCommand XPolishBPositionMoveCommand { get; }
    public DelegateCommand XEnableCommand { get; }
    public DelegateCommand XOriginCommand { get; }

    public DelegateCommand ZStandbyPositionMoveCommand { get; }
    public DelegateCommand ZTransferArmPositionMoveCommand { get; }
    public DelegateCommand ZPolishAPositionMoveCommand { get; }
    public DelegateCommand ZPolishBPositionMoveCommand { get; }
    public DelegateCommand ZEnableCommand { get; }
    public DelegateCommand ZOriginCommand { get; }

    public DelegateCommand EmergencyStopCommand { get; }
    public DelegateCommand AlarmResetCommand { get; }
    public DelegateCommand SaveCommand { get; }

    public DelegateCommand ReserveCommand { get; }
    public DelegateCommand PickFromPolishACommand { get; }
    public DelegateCommand PickFromPolishA2Command { get; }
    public DelegateCommand PlaceToTransferArmCommand { get; }
    public DelegateCommand ManualModeCommand { get; }
    public DelegateCommand AutoModeCommand { get; }

    public DelegateCommand XStandbyPositionSetCommand => _xStandbyPositionItem.SetCommand;
    public DelegateCommand XStandbyPositionCancelCommand => _xStandbyPositionItem.CancelCommand;
    public DelegateCommand XTransferArmPositionSetCommand => _xTransferArmPositionItem.SetCommand;
    public DelegateCommand XTransferArmPositionCancelCommand => _xTransferArmPositionItem.CancelCommand;
    public DelegateCommand XPolishAPositionSetCommand => _xPolishAPositionItem.SetCommand;
    public DelegateCommand XPolishAPositionCancelCommand => _xPolishAPositionItem.CancelCommand;
    public DelegateCommand XPolishBPositionSetCommand => _xPolishBPositionItem.SetCommand;
    public DelegateCommand XPolishBPositionCancelCommand => _xPolishBPositionItem.CancelCommand;
    public DelegateCommand XJogSpeedSetCommand => _xJogSpeedItem.SetCommand;
    public DelegateCommand XJogSpeedCancelCommand => _xJogSpeedItem.CancelCommand;
    public DelegateCommand XAutoSpeedSetCommand => _xAutoSpeedItem.SetCommand;
    public DelegateCommand XAutoSpeedCancelCommand => _xAutoSpeedItem.CancelCommand;

    public DelegateCommand ZStandbyPositionSetCommand => _zStandbyPositionItem.SetCommand;
    public DelegateCommand ZStandbyPositionCancelCommand => _zStandbyPositionItem.CancelCommand;
    public DelegateCommand ZTransferArmPositionSetCommand => _zTransferArmPositionItem.SetCommand;
    public DelegateCommand ZTransferArmPositionCancelCommand => _zTransferArmPositionItem.CancelCommand;
    public DelegateCommand ZPolishAPositionSetCommand => _zPolishAPositionItem.SetCommand;
    public DelegateCommand ZPolishAPositionCancelCommand => _zPolishAPositionItem.CancelCommand;
    public DelegateCommand ZPolishBPositionSetCommand => _zPolishBPositionItem.SetCommand;
    public DelegateCommand ZPolishBPositionCancelCommand => _zPolishBPositionItem.CancelCommand;
    public DelegateCommand ZJogSpeedSetCommand => _zJogSpeedItem.SetCommand;
    public DelegateCommand ZJogSpeedCancelCommand => _zJogSpeedItem.CancelCommand;
    public DelegateCommand ZAutoSpeedSetCommand => _zAutoSpeedItem.SetCommand;
    public DelegateCommand ZAutoSpeedCancelCommand => _zAutoSpeedItem.CancelCommand;

    #endregion

    public PolishASideArm3ControlViewModel() : base(0, "A手臂3")
    {
        _plc = GlobalService.RobotControlPlc;

        _editableItems = new List<EditableFloatItem>
        {
            _xStandbyPositionItem,
            _xTransferArmPositionItem,
            _xPolishAPositionItem,
            _xPolishBPositionItem,
            _xJogSpeedItem,
            _xAutoSpeedItem,
            _zStandbyPositionItem,
            _zTransferArmPositionItem,
            _zPolishAPositionItem,
            _zPolishBPositionItem,
            _zJogSpeedItem,
            _zAutoSpeedItem
        };
        EditableFloatItemHelper.InitializeCommands(_editableItems, _plc, RaisePropertyChanged);

        _readTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _readTimer.Tick += ReadTimer_Tick;
        RegisterLifecycleTimer(_readTimer);
        RegisterViewLifecycleAction(
            () => Logger.For("RobotControl").Debug("PolishASideArm3ControlViewModel activated, started PLC read timer."),
            () => Logger.For("RobotControl").Debug("PolishASideArm3ControlViewModel deactivated, stopped PLC read timer."));

        // Navigation
        StationOperationCommand = new DelegateCommand(() => CurrentViewIndex = 1);
        BackCommand = new DelegateCommand(() => CurrentViewIndex = 0);

        // Station Control
        StationResetCommand = OperationAudit.CreateAsync("StationReset", () => _plc.PulseAsync(PolishASideArm3Address.StationReset), () => "PolishASideArm3");
        StationStartCommand = OperationAudit.CreateAsync("StationStart", () => _plc.PulseAsync(PolishASideArm3Address.StationStart), () => "PolishASideArm3");
        StationStopCommand = OperationAudit.CreateAsync("StationStop", () => _plc.PulseAsync(PolishASideArm3Address.StationStop), () => "PolishASideArm3");
        StationAlarmResetCommand = OperationAudit.CreateAsync("StationAlarmReset", () => _plc.PulseAsync(PolishASideArm3Address.StationAlarmReset), () => "PolishASideArm3");

        // JOG Commands
        XPressJogUpCommand = OperationAudit.Create("XPressJogUp", () => _plc.Write(PolishASideArm3Address.XJogUp, true), () => "PolishASideArm3");
        XReleaseJogUpCommand = OperationAudit.Create("XReleaseJogUp", () => _plc.Write(PolishASideArm3Address.XJogUp, false), () => "PolishASideArm3");
        XPressJogDownCommand = OperationAudit.Create("XPressJogDown", () => _plc.Write(PolishASideArm3Address.XJogDown, true), () => "PolishASideArm3");
        XReleaseJogDownCommand = OperationAudit.Create("XReleaseJogDown", () => _plc.Write(PolishASideArm3Address.XJogDown, false), () => "PolishASideArm3");

        ZPressJogUpCommand = OperationAudit.Create("ZPressJogUp", () => _plc.Write(PolishASideArm3Address.ZJogUp, true), () => "PolishASideArm3");
        ZReleaseJogUpCommand = OperationAudit.Create("ZReleaseJogUp", () => _plc.Write(PolishASideArm3Address.ZJogUp, false), () => "PolishASideArm3");
        ZPressJogDownCommand = OperationAudit.Create("ZPressJogDown", () => _plc.Write(PolishASideArm3Address.ZJogDown, true), () => "PolishASideArm3");
        ZReleaseJogDownCommand = OperationAudit.Create("ZReleaseJogDown", () => _plc.Write(PolishASideArm3Address.ZJogDown, false), () => "PolishASideArm3");

        // Toggle Commands (Write Logic)
        XStandbyPositionMoveCommand = OperationAudit.Create("XStandbyPositionMove", () => _plc.Write(PolishASideArm3Address.XStandbyPositionMove, !XStandbyPositionMoveToggle), () => "PolishASideArm3");
        XTransferArmPositionMoveCommand = OperationAudit.Create("XTransferArmPositionMove", () => _plc.Write(PolishASideArm3Address.XTransferArmPositionMove, !XTransferArmPositionMoveToggle), () => "PolishASideArm3");
        XPolishAPositionMoveCommand = OperationAudit.Create("XPolishAPositionMove", () => _plc.Write(PolishASideArm3Address.XPolishAPositionMove, !XPolishAPositionMoveToggle), () => "PolishASideArm3");
        XPolishBPositionMoveCommand = OperationAudit.Create("XPolishBPositionMove", () => _plc.Write(PolishASideArm3Address.XPolishBPositionMove, !XPolishBPositionMoveToggle), () => "PolishASideArm3");
        XEnableCommand = OperationAudit.Create("XEnable", () => _plc.ToggleBool(PolishASideArm3Address.XEnable), () => "PolishASideArm3");
        XOriginCommand = OperationAudit.Create("XOrigin", () =>
        {
            if (MessageBox.Show(Lang.Resources.ConfirmActionContent, Lang.Resources.ConfirmActionTips,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _plc.Write(PolishASideArm3Address.XOrigin, !XOriginToggle);
            }

            XOriginToggle = false;
        }, () => "PolishASideArm3");

        ZStandbyPositionMoveCommand = OperationAudit.Create("ZStandbyPositionMove", () => _plc.Write(PolishASideArm3Address.ZStandbyPositionMove, !ZStandbyPositionMoveToggle), () => "PolishASideArm3");
        ZTransferArmPositionMoveCommand = OperationAudit.Create("ZTransferArmPositionMove", () => _plc.Write(PolishASideArm3Address.ZTransferArmPositionMove, !ZTransferArmPositionMoveToggle), () => "PolishASideArm3");
        ZPolishAPositionMoveCommand = OperationAudit.Create("ZPolishAPositionMove", () => _plc.Write(PolishASideArm3Address.ZPolishAPositionMove, !ZPolishAPositionMoveToggle), () => "PolishASideArm3");
        ZPolishBPositionMoveCommand = OperationAudit.Create("ZPolishBPositionMove", () => _plc.Write(PolishASideArm3Address.ZPolishBPositionMove, !ZPolishBPositionMoveToggle), () => "PolishASideArm3");
        ZEnableCommand = OperationAudit.Create("ZEnable", () => _plc.ToggleBool(PolishASideArm3Address.ZEnable), () => "PolishASideArm3");
        ZOriginCommand = OperationAudit.Create("ZOrigin", () =>
        {
            if (MessageBox.Show(Lang.Resources.ConfirmActionContent, Lang.Resources.ConfirmActionTips,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _plc.Write(PolishASideArm3Address.ZOrigin, !ZOriginToggle);
            }

            ZOriginToggle = false;
        }, () => "PolishASideArm3");

        EmergencyStopCommand = OperationAudit.Create("EmergencyStop", () => _plc.Write(PolishASideArm3Address.EmergencyStop, !EmergencyStopToggle), () => "PolishASideArm3");
        AlarmResetCommand = OperationAudit.CreateAsync("AlarmReset", () => _plc.PulseAsync(PolishASideArm3Address.AlarmReset), () => "PolishASideArm3");
        SaveCommand = OperationAudit.Create("Save", () => _plc.Write(PolishASideArm3Address.Save, !SaveToggle), () => "PolishASideArm3");

        ReserveCommand = OperationAudit.CreateAsync("Reserve", () => _plc.PulseAsync(PolishASideArm3Address.Reserve), () => "PolishASideArm3");
        PickFromPolishACommand = OperationAudit.CreateAsync("PickFromPolishA", () => _plc.PulseAsync(PolishASideArm3Address.PickFromPolishA), () => "PolishASideArm3");
        PickFromPolishA2Command = OperationAudit.CreateAsync("PickFromPolishA2", () => _plc.PulseAsync(PolishASideArm3Address.PickFromPolishA2), () => "PolishASideArm3");
        PlaceToTransferArmCommand = OperationAudit.CreateAsync("PlaceToTransferArm", () => _plc.PulseAsync(PolishASideArm3Address.PlaceToTransferArm), () => "PolishASideArm3");
        ManualModeCommand = OperationAudit.CreateAsync("ManualMode", () => _plc.PulseAsync(PolishASideArm3Address.ManualMode), () => "PolishASideArm3");
        AutoModeCommand = OperationAudit.CreateAsync("AutoMode", () => _plc.PulseAsync(PolishASideArm3Address.AutoMode), () => "PolishASideArm3");
    }

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
            new PlcBatchReadItem<bool>(PolishASideArm3Address.GlobalAlarm, assign: v => GlobalAlarm = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.Save, assign: v => SaveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.EmergencyStop, assign: v => EmergencyStopToggle = v),

            new PlcBatchReadItem<bool>(PolishASideArm3Address.XOrigin, assign: v => XOriginToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.XStandbyPositionMove, assign: v => XStandbyPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.XTransferArmPositionMove, assign: v => XTransferArmPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.XPolishAPositionMove, assign: v => XPolishAPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.XPolishBPositionMove, assign: v => XPolishBPositionMoveToggle = v),

            new PlcBatchReadItem<bool>(PolishASideArm3Address.ZOrigin, assign: v => ZOriginToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.ZStandbyPositionMove, assign: v => ZStandbyPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.ZTransferArmPositionMove, assign: v => ZTransferArmPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.ZPolishAPositionMove, assign: v => ZPolishAPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.ZPolishBPositionMove, assign: v => ZPolishBPositionMoveToggle = v),

            new PlcBatchReadItem<bool>(PolishASideArm3Address.AutoMode, assign: v => AutoModeToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.ManualMode, assign: v => ManualModeToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.Reserve, assign: v => ReserveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.PickFromPolishA, assign: v => PickFromPolishAToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.PickFromPolishA2, assign: v => PickFromPolishA2Toggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.PlaceToTransferArm, assign: v => PlaceToTransferArmToggle = v),

            new PlcBatchReadItem<bool>(PolishASideArm3Address.XEnableStatus, assign: v => XIsEnabled = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.XOriginStatus, assign: v => XIsAtOrigin = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.XJogUpStatus, assign: v => XJogUpActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.XJogDownStatus, assign: v => XJogDownActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.XJogMiddleStatus, assign: v => XJogMiddleActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.XLimitAlarm, assign: v => XLimitAlarm = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.XDriverAlarm, assign: v => XDriverAlarm = v),

            new PlcBatchReadItem<bool>(PolishASideArm3Address.ZEnableStatus, assign: v => ZIsEnabled = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.ZOriginStatus, assign: v => ZIsAtOrigin = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.ZJogUpStatus, assign: v => ZJogUpActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.ZJogDownStatus, assign: v => ZJogDownActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.ZJogMiddleStatus, assign: v => ZJogMiddleActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.ZLimitAlarm, assign: v => ZLimitAlarm = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.ZDriverAlarm, assign: v => ZDriverAlarm = v),

            new PlcBatchReadItem<bool>(PolishASideArm3Address.ZStandbyPositionArrived, assign: v => ZStandbyPositionArrived = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.ZTransferArmPositionArrived, assign: v => ZTransferArmPositionArrived = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.ZPolishAPositionArrived, assign: v => ZPolishAPositionArrived = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.ZPolishBPositionArrived, assign: v => ZPolishBPositionArrived = v),

            new PlcBatchReadItem<bool>(PolishASideArm3Address.StationResetStatus, assign: v => StationIsReset = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.StationRunningStatus, assign: v => StationIsRunning = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.StationStoppedStatus, assign: v => StationIsStopped = v),

            new PlcBatchReadItem<bool>(PolishASideArm3Address.ModuleAlarm, assign: v => ModuleAlarm = v),

            new PlcBatchReadItem<bool>(PolishASideArm3Address.ReserveActiveStatus, assign: v => ReserveActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.PickFromPolishAActiveStatus, assign: v => PickFromPolishAActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.PickFromPolishA2ActiveStatus, assign: v => PickFromPolishA2Active = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.PlaceToTransferArmActiveStatus, assign: v => PlaceToTransferArmActive = v),

            new PlcBatchReadItem<bool>(PolishASideArm3Address.ReserveDoneStatus, assign: v => ReserveDone = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.PickFromPolishADoneStatus, assign: v => PickFromPolishADone = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.PickFromPolishA2DoneStatus, assign: v => PickFromPolishA2Done = v),
            new PlcBatchReadItem<bool>(PolishASideArm3Address.PlaceToTransferArmDoneStatus, assign: v => PlaceToTransferArmDone = v)
        };

        var dStatusBatchItems = new List<PlcBatchReadItemBase>
        {
            new PlcBatchReadItem<int>(PolishASideArm3Address.XCurrentCoordinate, assign: v => XCurrentCoordinate = v / 1000.0),
            new PlcBatchReadItem<int>(PolishASideArm3Address.XAlarmCode, assign: v => XAlarmCode = v),
            new PlcBatchReadItem<int>(PolishASideArm3Address.ZCurrentCoordinate, assign: v => ZCurrentCoordinate = v / 1000.0),
            new PlcBatchReadItem<int>(PolishASideArm3Address.ZAlarmCode, assign: v => ZAlarmCode = v)
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
        var x1016Task = _plc.ReadAsync<bool[]>(PolishASideArm3Address.Arm3Fiber1, 3); // X1016-X1018
        var b0BTask = _plc.ReadAsync<bool>(PolishASideArm3Address.AlarmReset); // B0B
        var w1A30Task = _plc.ReadAsync<short[]>(PolishASideArm3Address.FoupSlot, 2); // W1A30-W1A31

        var allTasks = new List<Task>
        {
            mBatchTask,
            dStatusBatchTask,
            editableBatchTask,
            x1016Task,
            b0BTask,
            w1A30Task
        };

        await Task.WhenAll(allTasks);

        if (!mBatchTask.Result.IsSuccess)
            Logger.For("RobotControl").Warn($"[PolishASideArm3] M batch read failed: {mBatchTask.Result.Message}");

        if (!dStatusBatchTask.Result.IsSuccess)
            Logger.For("RobotControl").Warn($"[PolishASideArm3] D status batch read failed: {dStatusBatchTask.Result.Message}");

        if (!editableBatchTask.Result.IsSuccess)
            Logger.For("RobotControl").Warn($"[PolishASideArm3] D editable batch read failed: {editableBatchTask.Result.Message}");

        if (x1016Task.Result.IsSuccess)
        {
            var x = x1016Task.Result.Content;
            Arm3Fiber1 = x[0];
            Arm3Fiber2 = x[1];
            Arm3Fiber3 = x[2];
        }

        if (b0BTask.Result.IsSuccess) AlarmResetToggle = b0BTask.Result.Content;

        if (w1A30Task.Result.IsSuccess)
        {
            var w = w1A30Task.Result.Content;
            if (w.Length >= 2)
            {
                FoupSlot = w[0];
                WaferIndex = w[1];
            }
        }
    }

    public void StopReading()
    {
        OnViewDeactivated();
    }
}

