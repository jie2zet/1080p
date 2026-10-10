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

public class PolishASideArm1ControlViewModel : TabContentViewModelBase
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

    public double PositionMinimum { get; set; } = -90000;
    public double PositionMaximum { get; set; } = 90000;

    private readonly EditableFloatItem _xStandbyPositionItem = new(PolishASideArm1Address.XStandbyPosition, nameof(XStandbyPosition), nameof(IsXStandbyPositionDirty));
    private readonly EditableFloatItem _xTransferArmPositionItem = new(PolishASideArm1Address.XTransferArmPosition, nameof(XTransferArmPosition), nameof(IsXTransferArmPositionDirty));
    private readonly EditableFloatItem _xPolishAPositionItem = new(PolishASideArm1Address.XPolishAPosition, nameof(XPolishAPosition), nameof(IsXPolishAPositionDirty));
    private readonly EditableFloatItem _xPolishBPositionItem = new(PolishASideArm1Address.XPolishBPosition, nameof(XPolishBPosition), nameof(IsXPolishBPositionDirty));
    private readonly EditableFloatItem _xJogSpeedItem = new(PolishASideArm1Address.XJogSpeed, nameof(XJogSpeed), nameof(IsXJogSpeedDirty));
    private readonly EditableFloatItem _xAutoSpeedItem = new(PolishASideArm1Address.XAutoSpeed, nameof(XAutoSpeed), nameof(IsXAutoSpeedDirty));
    private readonly EditableFloatItem _zStandbyPositionItem = new(PolishASideArm1Address.ZStandbyPosition, nameof(ZStandbyPosition), nameof(IsZStandbyPositionDirty));
    private readonly EditableFloatItem _zTransferArmPositionItem = new(PolishASideArm1Address.ZTransferArmPosition, nameof(ZTransferArmPosition), nameof(IsZTransferArmPositionDirty));
    private readonly EditableFloatItem _zPolishAPositionItem = new(PolishASideArm1Address.ZPolishAPosition, nameof(ZPolishAPosition), nameof(IsZPolishAPositionDirty));
    private readonly EditableFloatItem _zPolishBPositionItem = new(PolishASideArm1Address.ZPolishBPosition, nameof(ZPolishBPosition), nameof(IsZPolishBPositionDirty));
    private readonly EditableFloatItem _speedWhenHasWaferItem = new(PolishASideArm1Address.SpeedWhenHasWafer, nameof(SpeedWhenHasWafer), nameof(IsSpeedWhenHasWaferDirty));
    private readonly EditableFloatItem _accelerationTimeItem = new(PolishASideArm1Address.AccelerationTime, nameof(AccelerationTime), nameof(IsAccelerationTimeDirty));
    private readonly EditableFloatItem _decelerationTimeItem = new(PolishASideArm1Address.DecelerationTime, nameof(DecelerationTime), nameof(IsDecelerationTimeDirty));
    private readonly EditableFloatItem _zJogSpeedItem = new(PolishASideArm1Address.ZJogSpeed, nameof(ZJogSpeed), nameof(IsZJogSpeedDirty));
    private readonly EditableFloatItem _zAutoSpeedItem = new(PolishASideArm1Address.ZAutoSpeed, nameof(ZAutoSpeed), nameof(IsZAutoSpeedDirty));
    private readonly List<EditableFloatItem> _editableItems;

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

    public bool IsSpeedWhenHasWaferDirty => _speedWhenHasWaferItem.IsDirty;

    public double SpeedWhenHasWafer
    {
        get => _speedWhenHasWaferItem.Value;
        set => EditableFloatItemHelper.SetValue(_speedWhenHasWaferItem, value, RaisePropertyChanged);
    }

    public bool IsAccelerationTimeDirty => _accelerationTimeItem.IsDirty;

    public double AccelerationTime
    {
        get => _accelerationTimeItem.Value;
        set => EditableFloatItemHelper.SetValue(_accelerationTimeItem, value, RaisePropertyChanged);
    }

    public bool IsDecelerationTimeDirty => _decelerationTimeItem.IsDirty;

    public double DecelerationTime
    {
        get => _decelerationTimeItem.Value;
        set => EditableFloatItemHelper.SetValue(_decelerationTimeItem, value, RaisePropertyChanged);
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

    private bool _powerOffToggle;
    public bool PowerOffToggle
    {
        get => _powerOffToggle;
        set => SetProperty(ref _powerOffToggle, value);
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

    private bool _pickFromTransferActive;
    public bool PickFromTransferActive
    {
        get => _pickFromTransferActive;
        set => SetProperty(ref _pickFromTransferActive, value);
    }

    private bool _pickFromTransferDone;
    public bool PickFromTransferDone
    {
        get => _pickFromTransferDone;
        set => SetProperty(ref _pickFromTransferDone, value);
    }

    private bool _pickFromTransferToggle;
    public bool PickFromTransferToggle
    {
        get => _pickFromTransferToggle;
        set => SetProperty(ref _pickFromTransferToggle, value);
    }

    private bool _placeToPolishAActive;
    public bool PlaceToPolishAActive
    {
        get => _placeToPolishAActive;
        set => SetProperty(ref _placeToPolishAActive, value);
    }

    private bool _placeToPolishADone;
    public bool PlaceToPolishADone
    {
        get => _placeToPolishADone;
        set => SetProperty(ref _placeToPolishADone, value);
    }

    private bool _placeToPolishAToggle;
    public bool PlaceToPolishAToggle
    {
        get => _placeToPolishAToggle;
        set => SetProperty(ref _placeToPolishAToggle, value);
    }

    private bool _placeToPolishBActive;
    public bool PlaceToPolishBActive
    {
        get => _placeToPolishBActive;
        set => SetProperty(ref _placeToPolishBActive, value);
    }

    private bool _placeToPolishBDone;
    public bool PlaceToPolishBDone
    {
        get => _placeToPolishBDone;
        set => SetProperty(ref _placeToPolishBDone, value);
    }

    private bool _placeToPolishBToggle;
    public bool PlaceToPolishBToggle
    {
        get => _placeToPolishBToggle;
        set => SetProperty(ref _placeToPolishBToggle, value);
    }

    private bool _spare1Active;
    public bool Spare1Active
    {
        get => _spare1Active;
        set => SetProperty(ref _spare1Active, value);
    }

    private bool _spare1Done;
    public bool Spare1Done
    {
        get => _spare1Done;
        set => SetProperty(ref _spare1Done, value);
    }

    private bool _spare1Toggle;
    public bool Spare1Toggle
    {
        get => _spare1Toggle;
        set => SetProperty(ref _spare1Toggle, value);
    }

    private bool _spare2Active;
    public bool Spare2Active
    {
        get => _spare2Active;
        set => SetProperty(ref _spare2Active, value);
    }

    private bool _spare2Done;
    public bool Spare2Done
    {
        get => _spare2Done;
        set => SetProperty(ref _spare2Done, value);
    }

    private bool _spare2Toggle;
    public bool Spare2Toggle
    {
        get => _spare2Toggle;
        set => SetProperty(ref _spare2Toggle, value);
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

    private bool _arm1Fiber1;
    public bool Arm1Fiber1
    {
        get => _arm1Fiber1;
        set => SetProperty(ref _arm1Fiber1, value);
    }

    private bool _arm1Fiber2;
    public bool Arm1Fiber2
    {
        get => _arm1Fiber2;
        set => SetProperty(ref _arm1Fiber2, value);
    }

    private bool _arm1Fiber3;
    public bool Arm1Fiber3
    {
        get => _arm1Fiber3;
        set => SetProperty(ref _arm1Fiber3, value);
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

    public DelegateCommand XPressJogUpCommand { get; }
    public DelegateCommand XReleaseJogUpCommand { get; }
    public DelegateCommand XPressJogDownCommand { get; }
    public DelegateCommand XReleaseJogDownCommand { get; }
    public DelegateCommand ZPressJogUpCommand { get; }
    public DelegateCommand ZReleaseJogUpCommand { get; }
    public DelegateCommand ZPressJogDownCommand { get; }
    public DelegateCommand ZReleaseJogDownCommand { get; }

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
    public DelegateCommand PowerOffCommand { get; }
    public DelegateCommand AlarmResetCommand { get; }
    public DelegateCommand SaveCommand { get; }

    public DelegateCommand PickFromTransferCommand { get; }
    public DelegateCommand PlaceToPolishACommand { get; }
    public DelegateCommand PlaceToPolishBCommand { get; }
    public DelegateCommand Spare1Command { get; }
    public DelegateCommand Spare2Command { get; }
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
    public DelegateCommand SpeedWhenHasWaferSetCommand => _speedWhenHasWaferItem.SetCommand;
    public DelegateCommand SpeedWhenHasWaferCancelCommand => _speedWhenHasWaferItem.CancelCommand;
    public DelegateCommand AccelerationTimeSetCommand => _accelerationTimeItem.SetCommand;
    public DelegateCommand AccelerationTimeCancelCommand => _accelerationTimeItem.CancelCommand;
    public DelegateCommand DecelerationTimeSetCommand => _decelerationTimeItem.SetCommand;
    public DelegateCommand DecelerationTimeCancelCommand => _decelerationTimeItem.CancelCommand;
    public DelegateCommand ZJogSpeedSetCommand => _zJogSpeedItem.SetCommand;
    public DelegateCommand ZJogSpeedCancelCommand => _zJogSpeedItem.CancelCommand;
    public DelegateCommand ZAutoSpeedSetCommand => _zAutoSpeedItem.SetCommand;
    public DelegateCommand ZAutoSpeedCancelCommand => _zAutoSpeedItem.CancelCommand;

    #endregion

    public PolishASideArm1ControlViewModel() : base(0, "A鎵嬭噦1")
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
            _speedWhenHasWaferItem,
            _accelerationTimeItem,
            _decelerationTimeItem,
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
            () => Logger.For("RobotControl").Debug("PolishASideArm1ControlViewModel activated, started PLC read timer."),
            () => Logger.For("RobotControl").Debug("PolishASideArm1ControlViewModel deactivated, stopped PLC read timer."));
        

        StationOperationCommand = new DelegateCommand(() => CurrentViewIndex = 1);
        BackCommand = new DelegateCommand(() => CurrentViewIndex = 0);

        StationResetCommand = OperationAudit.CreateAsync("StationReset", () => _plc.PulseAsync(PolishASideArm1Address.StationReset), () => "PolishASideArm1");
        StationStartCommand = OperationAudit.CreateAsync("StationStart", () => _plc.PulseAsync(PolishASideArm1Address.StationStart), () => "PolishASideArm1");
        StationStopCommand = OperationAudit.CreateAsync("StationStop", () => _plc.PulseAsync(PolishASideArm1Address.StationStop), () => "PolishASideArm1");
        StationAlarmResetCommand = OperationAudit.CreateAsync("StationAlarmReset", () => _plc.PulseAsync(PolishASideArm1Address.StationAlarmReset), () => "PolishASideArm1");

        XPressJogUpCommand = OperationAudit.Create("XPressJogUp", () => _plc.Write(PolishASideArm1Address.XJogUp, true), () => "PolishASideArm1");
        XReleaseJogUpCommand = OperationAudit.Create("XReleaseJogUp", () => _plc.Write(PolishASideArm1Address.XJogUp, false), () => "PolishASideArm1");
        XPressJogDownCommand = OperationAudit.Create("XPressJogDown", () => _plc.Write(PolishASideArm1Address.XJogDown, true), () => "PolishASideArm1");
        XReleaseJogDownCommand = OperationAudit.Create("XReleaseJogDown", () => _plc.Write(PolishASideArm1Address.XJogDown, false), () => "PolishASideArm1");

        ZPressJogUpCommand = OperationAudit.Create("ZPressJogUp", () => _plc.Write(PolishASideArm1Address.ZJogUp, true), () => "PolishASideArm1");
        ZReleaseJogUpCommand = OperationAudit.Create("ZReleaseJogUp", () => _plc.Write(PolishASideArm1Address.ZJogUp, false), () => "PolishASideArm1");
        ZPressJogDownCommand = OperationAudit.Create("ZPressJogDown", () => _plc.Write(PolishASideArm1Address.ZJogDown, true), () => "PolishASideArm1");
        ZReleaseJogDownCommand = OperationAudit.Create("ZReleaseJogDown", () => _plc.Write(PolishASideArm1Address.ZJogDown, false), () => "PolishASideArm1");

        XStandbyPositionMoveCommand = OperationAudit.Create("XStandbyPositionMove", () => _plc.Write(PolishASideArm1Address.XStandbyPositionMove, !XStandbyPositionMoveToggle), () => "PolishASideArm1");
        XTransferArmPositionMoveCommand = OperationAudit.Create("XTransferArmPositionMove", () => _plc.Write(PolishASideArm1Address.XTransferArmPositionMove, !XTransferArmPositionMoveToggle), () => "PolishASideArm1");
        XPolishAPositionMoveCommand = OperationAudit.Create("XPolishAPositionMove", () => _plc.Write(PolishASideArm1Address.XPolishAPositionMove, !XPolishAPositionMoveToggle), () => "PolishASideArm1");
        XPolishBPositionMoveCommand = OperationAudit.Create("XPolishBPositionMove", () => _plc.Write(PolishASideArm1Address.XPolishBPositionMove, !XPolishBPositionMoveToggle), () => "PolishASideArm1");
        XEnableCommand = OperationAudit.Create("XEnable", () => _plc.Write(PolishASideArm1Address.XEnable, !XIsEnabled), () => "PolishASideArm1");
        XOriginCommand = OperationAudit.Create("XOrigin", () =>
        {
            if (MessageBox.Show(Lang.Resources.ConfirmActionContent, Lang.Resources.ConfirmActionTips,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _plc.Write(PolishASideArm1Address.XOrigin, !XOriginToggle);
            }

            XOriginToggle = false;
        }, () => "PolishASideArm1");

        ZStandbyPositionMoveCommand = OperationAudit.Create("ZStandbyPositionMove", () => _plc.Write(PolishASideArm1Address.ZStandbyPositionMove, !ZStandbyPositionMoveToggle), () => "PolishASideArm1");
        ZTransferArmPositionMoveCommand = OperationAudit.Create("ZTransferArmPositionMove", () => _plc.Write(PolishASideArm1Address.ZTransferArmPositionMove, !ZTransferArmPositionMoveToggle), () => "PolishASideArm1");
        ZPolishAPositionMoveCommand = OperationAudit.Create("ZPolishAPositionMove", () => _plc.Write(PolishASideArm1Address.ZPolishAPositionMove, !ZPolishAPositionMoveToggle), () => "PolishASideArm1");
        ZPolishBPositionMoveCommand = OperationAudit.Create("ZPolishBPositionMove", () => _plc.Write(PolishASideArm1Address.ZPolishBPositionMove, !ZPolishBPositionMoveToggle), () => "PolishASideArm1");
        ZEnableCommand = OperationAudit.Create("ZEnable", () => _plc.Write(PolishASideArm1Address.ZEnable, !ZIsEnabled), () => "PolishASideArm1");
        ZOriginCommand = OperationAudit.Create("ZOrigin", () =>
        {
            if (MessageBox.Show(Lang.Resources.ConfirmActionContent, Lang.Resources.ConfirmActionTips,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _plc.Write(PolishASideArm1Address.ZOrigin, !ZOriginToggle);
            }

            ZOriginToggle = false;
        }, () => "PolishASideArm1");

        EmergencyStopCommand = OperationAudit.Create("EmergencyStop", () => _plc.Write(PolishASideArm1Address.EmergencyStop, !EmergencyStopToggle), () => "PolishASideArm1");
        PowerOffCommand = OperationAudit.CreateAsync("PowerOff", () => _plc.PulseAsync(PolishASideArm1Address.PowerOff), () => "PolishASideArm1");
        AlarmResetCommand = OperationAudit.CreateAsync("AlarmReset", () => _plc.PulseAsync(PolishASideArm1Address.AlarmReset), () => "PolishASideArm1");
        SaveCommand = OperationAudit.Create("Save", () => _plc.Write(PolishASideArm1Address.Save, !SaveToggle), () => "PolishASideArm1");

        PickFromTransferCommand = OperationAudit.CreateAsync("PickFromTransfer", () => _plc.PulseAsync(PolishASideArm1Address.PickFromTransfer), () => "PolishASideArm1");
        PlaceToPolishACommand = OperationAudit.CreateAsync("PlaceToPolishA", () => _plc.PulseAsync(PolishASideArm1Address.PlaceToPolishA), () => "PolishASideArm1");
        PlaceToPolishBCommand = OperationAudit.CreateAsync("PlaceToPolishB", () => _plc.PulseAsync(PolishASideArm1Address.PlaceToPolishB), () => "PolishASideArm1");
        Spare1Command = OperationAudit.CreateAsync("Spare1", () => _plc.PulseAsync(PolishASideArm1Address.Spare1), () => "PolishASideArm1");
        Spare2Command = OperationAudit.CreateAsync("Spare2", () => _plc.PulseAsync(PolishASideArm1Address.Spare2), () => "PolishASideArm1");
        ManualModeCommand = OperationAudit.CreateAsync("ManualMode", () => _plc.PulseAsync(PolishASideArm1Address.ManualMode), () => "PolishASideArm1");
        AutoModeCommand = OperationAudit.CreateAsync("AutoMode", () => _plc.PulseAsync(PolishASideArm1Address.AutoMode), () => "PolishASideArm1");

    }

    private async void ReadTimer_Tick(object? sender, EventArgs e)
    {
        if (_isReading)
        {
            return;
        }

        if (!_plc.IsConnected)
        {
            _plc.Connect("192.168.2.50", 7003);
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
            new PlcBatchReadItem<bool>(PolishASideArm1Address.GlobalAlarm, assign: v => GlobalAlarm = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.Save, assign: v => SaveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.EmergencyStop, assign: v => EmergencyStopToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.PowerOff, assign: v => PowerOffToggle = v),

            new PlcBatchReadItem<bool>(PolishASideArm1Address.XOrigin, assign: v => XOriginToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.XStandbyPositionMove, assign: v => XStandbyPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.XTransferArmPositionMove, assign: v => XTransferArmPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.XPolishAPositionMove, assign: v => XPolishAPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.XPolishBPositionMove, assign: v => XPolishBPositionMoveToggle = v),

            new PlcBatchReadItem<bool>(PolishASideArm1Address.ZOrigin, assign: v => ZOriginToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.ZStandbyPositionMove, assign: v => ZStandbyPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.ZTransferArmPositionMove, assign: v => ZTransferArmPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.ZPolishAPositionMove, assign: v => ZPolishAPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.ZPolishBPositionMove, assign: v => ZPolishBPositionMoveToggle = v),

            new PlcBatchReadItem<bool>(PolishASideArm1Address.AutoMode, assign: v => AutoModeToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.ManualMode, assign: v => ManualModeToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.PickFromTransfer, assign: v => PickFromTransferToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.PlaceToPolishA, assign: v => PlaceToPolishAToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.PlaceToPolishB, assign: v => PlaceToPolishBToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.Spare1, assign: v => Spare1Toggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.Spare2, assign: v => Spare2Toggle = v),

            new PlcBatchReadItem<bool>(PolishASideArm1Address.XEnableStatus, assign: v => XIsEnabled = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.XOriginStatus, assign: v => XIsAtOrigin = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.XJogUpStatus, assign: v => XJogUpActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.XJogDownStatus, assign: v => XJogDownActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.XJogMiddleStatus, assign: v => XJogMiddleActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.XLimitAlarm, assign: v => XLimitAlarm = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.XDriverAlarm, assign: v => XDriverAlarm = v),

            new PlcBatchReadItem<bool>(PolishASideArm1Address.ZEnableStatus, assign: v => ZIsEnabled = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.ZOriginStatus, assign: v => ZIsAtOrigin = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.ZJogUpStatus, assign: v => ZJogUpActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.ZJogDownStatus, assign: v => ZJogDownActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.ZJogMiddleStatus, assign: v => ZJogMiddleActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.ZLimitAlarm, assign: v => ZLimitAlarm = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.ZDriverAlarm, assign: v => ZDriverAlarm = v),

            new PlcBatchReadItem<bool>(PolishASideArm1Address.ZStandbyPositionArrived, assign: v => ZStandbyPositionArrived = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.ZTransferArmPositionArrived, assign: v => ZTransferArmPositionArrived = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.ZPolishAPositionArrived, assign: v => ZPolishAPositionArrived = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.ZPolishBPositionArrived, assign: v => ZPolishBPositionArrived = v),

            new PlcBatchReadItem<bool>(PolishASideArm1Address.StationResetStatus, assign: v => StationIsReset = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.StationRunningStatus, assign: v => StationIsRunning = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.StationStoppedStatus, assign: v => StationIsStopped = v),

            new PlcBatchReadItem<bool>(PolishASideArm1Address.ModuleAlarm, assign: v => ModuleAlarm = v),

            new PlcBatchReadItem<bool>(PolishASideArm1Address.PickFromTransferActiveStatus, assign: v => PickFromTransferActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.PlaceToPolishAActiveStatus, assign: v => PlaceToPolishAActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.PlaceToPolishBActiveStatus, assign: v => PlaceToPolishBActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.Spare1ActiveStatus, assign: v => Spare1Active = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.Spare2ActiveStatus, assign: v => Spare2Active = v),

            new PlcBatchReadItem<bool>(PolishASideArm1Address.PickFromTransferDoneStatus, assign: v => PickFromTransferDone = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.PlaceToPolishADoneStatus, assign: v => PlaceToPolishADone = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.PlaceToPolishBDoneStatus, assign: v => PlaceToPolishBDone = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.Spare1DoneStatus, assign: v => Spare1Done = v),
            new PlcBatchReadItem<bool>(PolishASideArm1Address.Spare2DoneStatus, assign: v => Spare2Done = v)
        };

        var dStatusBatchItems = new List<PlcBatchReadItemBase>
        {
            new PlcBatchReadItem<int>(PolishASideArm1Address.XCurrentCoordinate, assign: v => XCurrentCoordinate = v / 1000.0),
            new PlcBatchReadItem<int>(PolishASideArm1Address.XAlarmCode, assign: v => XAlarmCode = v),
            new PlcBatchReadItem<int>(PolishASideArm1Address.ZCurrentCoordinate, assign: v => ZCurrentCoordinate = v / 1000.0),
            new PlcBatchReadItem<int>(PolishASideArm1Address.ZAlarmCode, assign: v => ZAlarmCode = v)
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
        var x1010Task = _plc.ReadAsync<bool[]>(PolishASideArm1Address.Arm1Fiber1, 3); // X1010-X1012
        var b0BTask = _plc.ReadAsync<bool>(PolishASideArm1Address.AlarmReset); // B0B
        var w1A10Task = _plc.ReadAsync<short[]>(PolishASideArm1Address.FoupSlot, 2); // W1A10-W1A11

        var allTasks = new List<Task>
        {
            mBatchTask,
            dStatusBatchTask,
            editableBatchTask,
            x1010Task,
            b0BTask,
            w1A10Task
        };

        await Task.WhenAll(allTasks);

        if (!mBatchTask.Result.IsSuccess)
            Logger.For("RobotControl").Warn($"[PolishASideArm1] M batch read failed: {mBatchTask.Result.Message}");

        if (!dStatusBatchTask.Result.IsSuccess)
            Logger.For("RobotControl").Warn($"[PolishASideArm1] D status batch read failed: {dStatusBatchTask.Result.Message}");

        if (!editableBatchTask.Result.IsSuccess)
            Logger.For("RobotControl").Warn($"[PolishASideArm1] D editable batch read failed: {editableBatchTask.Result.Message}");

        if (x1010Task.Result.IsSuccess)
        {
            var x = x1010Task.Result.Content;
            Arm1Fiber1 = x[0];
            Arm1Fiber2 = x[1];
            Arm1Fiber3 = x[2];
        }

        if (b0BTask.Result.IsSuccess) AlarmResetToggle = b0BTask.Result.Content;

        if (w1A10Task.Result.IsSuccess)
        {
            var w = w1A10Task.Result.Content;
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

