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

public class PolishASideArm2ControlViewModel : TabContentViewModelBase
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

    private readonly EditableFloatItem _xStandbyPositionItem = new(PolishASideArm2Address.XStandbyPosition, nameof(XStandbyPosition), nameof(IsXStandbyPositionDirty));
    private readonly EditableFloatItem _xTransferArmPositionItem = new(PolishASideArm2Address.XTransferArmPosition, nameof(XTransferArmPosition), nameof(IsXTransferArmPositionDirty));
    private readonly EditableFloatItem _xPolishAPositionItem = new(PolishASideArm2Address.XPolishAPosition, nameof(XPolishAPosition), nameof(IsXPolishAPositionDirty));
    private readonly EditableFloatItem _xPolishBPositionItem = new(PolishASideArm2Address.XPolishBPosition, nameof(XPolishBPosition), nameof(IsXPolishBPositionDirty));
    private readonly EditableFloatItem _xJogSpeedItem = new(PolishASideArm2Address.XJogSpeed, nameof(XJogSpeed), nameof(IsXJogSpeedDirty));
    private readonly EditableFloatItem _xAutoSpeedItem = new(PolishASideArm2Address.XAutoSpeed, nameof(XAutoSpeed), nameof(IsXAutoSpeedDirty));
    private readonly EditableFloatItem _zStandbyPositionItem = new(PolishASideArm2Address.ZStandbyPosition, nameof(ZStandbyPosition), nameof(IsZStandbyPositionDirty));
    private readonly EditableFloatItem _zTransferArmPositionItem = new(PolishASideArm2Address.ZTransferArmPosition, nameof(ZTransferArmPosition), nameof(IsZTransferArmPositionDirty));
    private readonly EditableFloatItem _zPolishAUnloadPositionItem = new(PolishASideArm2Address.ZPolishAUnloadPosition, nameof(ZPolishAUnloadPosition), nameof(IsZPolishAUnloadPositionDirty));
    private readonly EditableFloatItem _zPolishBLoadPositionItem = new(PolishASideArm2Address.ZPolishBLoadPosition, nameof(ZPolishBLoadPosition), nameof(IsZPolishBLoadPositionDirty));
    private readonly EditableFloatItem _zPolishALoadPositionItem = new(PolishASideArm2Address.ZPolishALoadPosition, nameof(ZPolishALoadPosition), nameof(IsZPolishALoadPositionDirty));
    private readonly EditableFloatItem _zPolishBUnloadPositionItem = new(PolishASideArm2Address.ZPolishBUnloadPosition, nameof(ZPolishBUnloadPosition), nameof(IsZPolishBUnloadPositionDirty));
    private readonly EditableFloatItem _speedWhenHasWaferItem = new(PolishASideArm2Address.SpeedWhenHasWafer, nameof(SpeedWhenHasWafer), nameof(IsSpeedWhenHasWaferDirty));
    private readonly EditableFloatItem _accelerationTimeItem = new(PolishASideArm2Address.AccelerationTime, nameof(AccelerationTime), nameof(IsAccelerationTimeDirty));
    private readonly EditableFloatItem _decelerationTimeItem = new(PolishASideArm2Address.DeaccelerationTime, nameof(DecelerationTime), nameof(IsDecelerationTimeDirty));
    private readonly EditableFloatItem _zJogSpeedItem = new(PolishASideArm2Address.ZJogSpeed, nameof(ZJogSpeed), nameof(IsZJogSpeedDirty));
    private readonly EditableFloatItem _zAutoSpeedItem = new(PolishASideArm2Address.ZAutoSpeed, nameof(ZAutoSpeed), nameof(IsZAutoSpeedDirty));

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

    public bool IsZPolishAUnloadPositionDirty => _zPolishAUnloadPositionItem.IsDirty;

    public double ZPolishAUnloadPosition
    {
        get => _zPolishAUnloadPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_zPolishAUnloadPositionItem, value, RaisePropertyChanged);
    }

    public bool IsZPolishBLoadPositionDirty => _zPolishBLoadPositionItem.IsDirty;

    public double ZPolishBLoadPosition
    {
        get => _zPolishBLoadPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_zPolishBLoadPositionItem, value, RaisePropertyChanged);
    }

    public bool IsZPolishALoadPositionDirty => _zPolishALoadPositionItem.IsDirty;

    public double ZPolishALoadPosition
    {
        get => _zPolishALoadPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_zPolishALoadPositionItem, value, RaisePropertyChanged);
    }

    public bool IsZPolishBUnloadPositionDirty => _zPolishBUnloadPositionItem.IsDirty;

    public double ZPolishBUnloadPosition
    {
        get => _zPolishBUnloadPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_zPolishBUnloadPositionItem, value, RaisePropertyChanged);
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

    private bool _zPolishAUnloadPositionMoveToggle;
    public bool ZPolishAUnloadPositionMoveToggle
    {
        get => _zPolishAUnloadPositionMoveToggle;
        set => SetProperty(ref _zPolishAUnloadPositionMoveToggle, value);
    }

    private bool _zPolishBLoadPositionMoveToggle;
    public bool ZPolishBLoadPositionMoveToggle
    {
        get => _zPolishBLoadPositionMoveToggle;
        set => SetProperty(ref _zPolishBLoadPositionMoveToggle, value);
    }

    private bool _zPolishALoadPositionMoveToggle;
    public bool ZPolishALoadPositionMoveToggle
    {
        get => _zPolishALoadPositionMoveToggle;
        set => SetProperty(ref _zPolishALoadPositionMoveToggle, value);
    }

    private bool _zPolishBUnloadPositionMoveToggle;
    public bool ZPolishBUnloadPositionMoveToggle
    {
        get => _zPolishBUnloadPositionMoveToggle;
        set => SetProperty(ref _zPolishBUnloadPositionMoveToggle, value);
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

    private bool _zPolishAUnloadPositionArrived;
    public bool ZPolishAUnloadPositionArrived
    {
        get => _zPolishAUnloadPositionArrived;
        set => SetProperty(ref _zPolishAUnloadPositionArrived, value);
    }

    private bool _zPolishBLoadPositionArrived;
    public bool ZPolishBLoadPositionArrived
    {
        get => _zPolishBLoadPositionArrived;
        set => SetProperty(ref _zPolishBLoadPositionArrived, value);
    }

    private bool _zPolishALoadPositionArrived;
    public bool ZPolishALoadPositionArrived
    {
        get => _zPolishALoadPositionArrived;
        set => SetProperty(ref _zPolishALoadPositionArrived, value);
    }

    private bool _zPolishBUnloadPositionArrived;
    public bool ZPolishBUnloadPositionArrived
    {
        get => _zPolishBUnloadPositionArrived;
        set => SetProperty(ref _zPolishBUnloadPositionArrived, value);
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

    private bool _pickFromPolishBActive;
    public bool PickFromPolishBActive
    {
        get => _pickFromPolishBActive;
        set => SetProperty(ref _pickFromPolishBActive, value);
    }

    private bool _pickFromPolishBDone;
    public bool PickFromPolishBDone
    {
        get => _pickFromPolishBDone;
        set => SetProperty(ref _pickFromPolishBDone, value);
    }

    private bool _pickFromPolishBToggle;
    public bool PickFromPolishBToggle
    {
        get => _pickFromPolishBToggle;
        set => SetProperty(ref _pickFromPolishBToggle, value);
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

    private bool _pickFromPolishBMaterialActive;
    public bool PickFromPolishBMaterialActive
    {
        get => _pickFromPolishBMaterialActive;
        set => SetProperty(ref _pickFromPolishBMaterialActive, value);
    }

    private bool _pickFromPolishBMaterialDone;
    public bool PickFromPolishBMaterialDone
    {
        get => _pickFromPolishBMaterialDone;
        set => SetProperty(ref _pickFromPolishBMaterialDone, value);
    }

    private bool _pickFromPolishBMaterialToggle;
    public bool PickFromPolishBMaterialToggle
    {
        get => _pickFromPolishBMaterialToggle;
        set => SetProperty(ref _pickFromPolishBMaterialToggle, value);
    }

    private bool _polishBLoadUnloadActive;
    public bool PolishBLoadUnloadActive
    {
        get => _polishBLoadUnloadActive;
        set => SetProperty(ref _polishBLoadUnloadActive, value);
    }

    private bool _polishBLoadUnloadDone;
    public bool PolishBLoadUnloadDone
    {
        get => _polishBLoadUnloadDone;
        set => SetProperty(ref _polishBLoadUnloadDone, value);
    }

    private bool _polishBLoadUnloadToggle;
    public bool PolishBLoadUnloadToggle
    {
        get => _polishBLoadUnloadToggle;
        set => SetProperty(ref _polishBLoadUnloadToggle, value);
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
    private bool _arm2Fiber1;
    public bool Arm2Fiber1
    {
        get => _arm2Fiber1;
        set => SetProperty(ref _arm2Fiber1, value);
    }

    private bool _arm2Fiber2;
    public bool Arm2Fiber2
    {
        get => _arm2Fiber2;
        set => SetProperty(ref _arm2Fiber2, value);
    }

    private bool _arm2Fiber3;
    public bool Arm2Fiber3
    {
        get => _arm2Fiber3;
        set => SetProperty(ref _arm2Fiber3, value);
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
    public DelegateCommand ZPolishAUnloadPositionMoveCommand { get; }
    public DelegateCommand ZPolishBLoadPositionMoveCommand { get; }
    public DelegateCommand ZPolishALoadPositionMoveCommand { get; }
    public DelegateCommand ZPolishBUnloadPositionMoveCommand { get; }
    public DelegateCommand ZEnableCommand { get; }
    public DelegateCommand ZOriginCommand { get; }

    public DelegateCommand EmergencyStopCommand { get; }
    public DelegateCommand PowerOffCommand { get; }
    public DelegateCommand AlarmResetCommand { get; }
    public DelegateCommand SaveCommand { get; }

    public DelegateCommand PickFromTransferCommand { get; }
    public DelegateCommand PickFromPolishACommand { get; }
    public DelegateCommand PickFromPolishBCommand { get; }
    public DelegateCommand PlaceToTransferArmCommand { get; }
    public DelegateCommand PickFromPolishBMaterialCommand { get; }
    public DelegateCommand PolishBLoadUnloadCommand { get; }
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
    public DelegateCommand ZPolishAUnloadPositionSetCommand => _zPolishAUnloadPositionItem.SetCommand;
    public DelegateCommand ZPolishAUnloadPositionCancelCommand => _zPolishAUnloadPositionItem.CancelCommand;
    public DelegateCommand ZPolishBLoadPositionSetCommand => _zPolishBLoadPositionItem.SetCommand;
    public DelegateCommand ZPolishBLoadPositionCancelCommand => _zPolishBLoadPositionItem.CancelCommand;
    public DelegateCommand ZPolishALoadPositionSetCommand => _zPolishALoadPositionItem.SetCommand;
    public DelegateCommand ZPolishALoadPositionCancelCommand => _zPolishALoadPositionItem.CancelCommand;
    public DelegateCommand ZPolishBUnloadPositionSetCommand => _zPolishBUnloadPositionItem.SetCommand;
    public DelegateCommand ZPolishBUnloadPositionCancelCommand => _zPolishBUnloadPositionItem.CancelCommand;
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

    public PolishASideArm2ControlViewModel() : base(0, "A手臂2")
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
            _zPolishAUnloadPositionItem,
            _zPolishBLoadPositionItem,
            _zPolishALoadPositionItem,
            _zPolishBUnloadPositionItem,
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
            () => Logger.For("RobotControl").Debug("PolishASideArm2ControlViewModel activated, started PLC read timer."),
            () => Logger.For("RobotControl").Debug("PolishASideArm2ControlViewModel deactivated, stopped PLC read timer."));

        // Navigation
        StationOperationCommand = new DelegateCommand(() => CurrentViewIndex = 1);
        BackCommand = new DelegateCommand(() => CurrentViewIndex = 0);

        // Station Control
        StationResetCommand = OperationAudit.Create("StationReset", () => _plc.Write(PolishASideArm2Address.StationReset, true), () => "PolishASideArm2");
        StationStartCommand = OperationAudit.Create("StationStart", () => _plc.Write(PolishASideArm2Address.StationStart, true), () => "PolishASideArm2");
        StationStopCommand = OperationAudit.Create("StationStop", () => _plc.Write(PolishASideArm2Address.StationStop, true), () => "PolishASideArm2");
        StationAlarmResetCommand = OperationAudit.Create("StationAlarmReset", () => _plc.Write(PolishASideArm2Address.StationAlarmReset, true), () => "PolishASideArm2");

        // JOG Commands
        XPressJogUpCommand = OperationAudit.Create("XPressJogUp", () => _plc.Write(PolishASideArm2Address.XJogUp, true), () => "PolishASideArm2");
        XReleaseJogUpCommand = OperationAudit.Create("XReleaseJogUp", () => _plc.Write(PolishASideArm2Address.XJogUp, false), () => "PolishASideArm2");
        XPressJogDownCommand = OperationAudit.Create("XPressJogDown", () => _plc.Write(PolishASideArm2Address.XJogDown, true), () => "PolishASideArm2");
        XReleaseJogDownCommand = OperationAudit.Create("XReleaseJogDown", () => _plc.Write(PolishASideArm2Address.XJogDown, false), () => "PolishASideArm2");

        ZPressJogUpCommand = OperationAudit.Create("ZPressJogUp", () => _plc.Write(PolishASideArm2Address.ZJogUp, true), () => "PolishASideArm2");
        ZReleaseJogUpCommand = OperationAudit.Create("ZReleaseJogUp", () => _plc.Write(PolishASideArm2Address.ZJogUp, false), () => "PolishASideArm2");
        ZPressJogDownCommand = OperationAudit.Create("ZPressJogDown", () => _plc.Write(PolishASideArm2Address.ZJogDown, true), () => "PolishASideArm2");
        ZReleaseJogDownCommand = OperationAudit.Create("ZReleaseJogDown", () => _plc.Write(PolishASideArm2Address.ZJogDown, false), () => "PolishASideArm2");

        // Toggle Commands (Write Logic)
        XStandbyPositionMoveCommand = OperationAudit.Create("XStandbyPositionMove", () => _plc.Write(PolishASideArm2Address.XStandbyPositionMove, !XStandbyPositionMoveToggle), () => "PolishASideArm2");
        XTransferArmPositionMoveCommand = OperationAudit.Create("XTransferArmPositionMove", () => _plc.Write(PolishASideArm2Address.XTransferArmPositionMove, !XTransferArmPositionMoveToggle), () => "PolishASideArm2");
        XPolishAPositionMoveCommand = OperationAudit.Create("XPolishAPositionMove", () => _plc.Write(PolishASideArm2Address.XPolishAPositionMove, !XPolishAPositionMoveToggle), () => "PolishASideArm2");
        XPolishBPositionMoveCommand = OperationAudit.Create("XPolishBPositionMove", () => _plc.Write(PolishASideArm2Address.XPolishBPositionMove, !XPolishBPositionMoveToggle), () => "PolishASideArm2");
        XEnableCommand = OperationAudit.Create("XEnable", () => _plc.Write(PolishASideArm2Address.XEnable, !XIsEnabled), () => "PolishASideArm2");
        XOriginCommand = OperationAudit.Create("XOrigin", () =>
        {
            if (MessageBox.Show(Lang.Resources.ConfirmActionContent, Lang.Resources.ConfirmActionTips,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _plc.Write(PolishASideArm2Address.XOrigin, !XOriginToggle);
            }

            XOriginToggle = false;
        }, () => "PolishASideArm2");

        ZStandbyPositionMoveCommand = OperationAudit.Create("ZStandbyPositionMove", () => _plc.Write(PolishASideArm2Address.ZStandbyPositionMove, !ZStandbyPositionMoveToggle), () => "PolishASideArm2");
        ZTransferArmPositionMoveCommand = OperationAudit.Create("ZTransferArmPositionMove", () => _plc.Write(PolishASideArm2Address.ZTransferArmPositionMove, !ZTransferArmPositionMoveToggle), () => "PolishASideArm2");
        ZPolishAUnloadPositionMoveCommand = OperationAudit.Create("ZPolishAUnloadPositionMove", () => _plc.Write(PolishASideArm2Address.ZPolishAUnloadPositionMove, !ZPolishAUnloadPositionMoveToggle), () => "PolishASideArm2");
        ZPolishBLoadPositionMoveCommand = OperationAudit.Create("ZPolishBLoadPositionMove", () => _plc.Write(PolishASideArm2Address.ZPolishBLoadPositionMove, !ZPolishBLoadPositionMoveToggle), () => "PolishASideArm2");
        ZPolishALoadPositionMoveCommand = OperationAudit.Create("ZPolishALoadPositionMove", () => _plc.Write(PolishASideArm2Address.ZPolishALoadPositionMove, !ZPolishALoadPositionMoveToggle), () => "PolishASideArm2");
        ZPolishBUnloadPositionMoveCommand = OperationAudit.Create("ZPolishBUnloadPositionMove", () => _plc.Write(PolishASideArm2Address.ZPolishBUnloadPositionMove, !ZPolishBUnloadPositionMoveToggle), () => "PolishASideArm2");
        ZEnableCommand = OperationAudit.Create("ZEnable", () => _plc.Write(PolishASideArm2Address.ZEnable, !ZIsEnabled), () => "PolishASideArm2");
        ZOriginCommand = OperationAudit.Create("ZOrigin", () =>
        {
            if (MessageBox.Show(Lang.Resources.ConfirmActionContent, Lang.Resources.ConfirmActionTips,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _plc.Write(PolishASideArm2Address.ZOrigin, !ZOriginToggle);
            }

            ZOriginToggle = false;
        }, () => "PolishASideArm2");

        EmergencyStopCommand = OperationAudit.Create("EmergencyStop", () => _plc.Write(PolishASideArm2Address.EmergencyStop, !EmergencyStopToggle), () => "PolishASideArm2");
        PowerOffCommand = OperationAudit.Create("PowerOff", () => _plc.Write(PolishASideArm2Address.PowerOff, !PowerOffToggle), () => "PolishASideArm2");
        AlarmResetCommand = OperationAudit.Create("AlarmReset", () => _plc.Write(PolishASideArm2Address.AlarmReset, !AlarmResetToggle), () => "PolishASideArm2");
        SaveCommand = OperationAudit.Create("Save", () => _plc.Write(PolishASideArm2Address.Save, !SaveToggle), () => "PolishASideArm2");

        PickFromTransferCommand = OperationAudit.Create("PickFromTransfer", () =>
        {
            _plc.Write(PolishASideArm2Address.PickFromTransfer, true);
            Thread.Sleep(50);
            _plc.Write(PolishASideArm2Address.PickFromTransfer, false);
        }, () => "PolishASideArm2");
        PickFromPolishACommand = OperationAudit.Create("PickFromPolishA", () =>
        {
            _plc.Write(PolishASideArm2Address.PickFromPolishA, true);
            Thread.Sleep(50);
            _plc.Write(PolishASideArm2Address.PickFromPolishA, false);
        }, () => "PolishASideArm2");
        PickFromPolishBCommand = OperationAudit.Create("PickFromPolishB", () =>
        {
            _plc.Write(PolishASideArm2Address.PickFromPolishB, true);
            Thread.Sleep(50);
            _plc.Write(PolishASideArm2Address.PickFromPolishB, false);
        }, () => "PolishASideArm2");
        PlaceToTransferArmCommand = OperationAudit.Create("PlaceToTransferArm", () =>
        {
            _plc.Write(PolishASideArm2Address.PlaceToTransferArm, true);
            Thread.Sleep(50);
            _plc.Write(PolishASideArm2Address.PlaceToTransferArm, false);
        }, () => "PolishASideArm2");
        PickFromPolishBMaterialCommand = OperationAudit.Create("PickFromPolishBMaterial", () =>
        {
            _plc.Write(PolishASideArm2Address.PickFromPolishBMaterial, true);
            Thread.Sleep(50);
            _plc.Write(PolishASideArm2Address.PickFromPolishBMaterial, false);
        }, () => "PolishASideArm2");
        PolishBLoadUnloadCommand = OperationAudit.Create("PolishBLoadUnload", () =>
        {
            _plc.Write(PolishASideArm2Address.PolishBLoadUnload, true);
            Thread.Sleep(50);
            _plc.Write(PolishASideArm2Address.PolishBLoadUnload, false);
        }, () => "PolishASideArm2");
        ManualModeCommand = OperationAudit.Create("ManualMode", () => _plc.Write(PolishASideArm2Address.ManualMode, !ManualModeToggle), () => "PolishASideArm2");
        AutoModeCommand = OperationAudit.Create("AutoMode", () => _plc.Write(PolishASideArm2Address.AutoMode, !AutoModeToggle), () => "PolishASideArm2");
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
            new PlcBatchReadItem<bool>(PolishASideArm2Address.GlobalAlarm, assign: v => GlobalAlarm = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.Save, assign: v => SaveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.EmergencyStop, assign: v => EmergencyStopToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.PowerOff, assign: v => PowerOffToggle = v),

            new PlcBatchReadItem<bool>(PolishASideArm2Address.XOrigin, assign: v => XOriginToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.XStandbyPositionMove, assign: v => XStandbyPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.XTransferArmPositionMove, assign: v => XTransferArmPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.XPolishAPositionMove, assign: v => XPolishAPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.XPolishBPositionMove, assign: v => XPolishBPositionMoveToggle = v),

            new PlcBatchReadItem<bool>(PolishASideArm2Address.ZOrigin, assign: v => ZOriginToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.ZStandbyPositionMove, assign: v => ZStandbyPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.ZTransferArmPositionMove, assign: v => ZTransferArmPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.ZPolishAUnloadPositionMove, assign: v => ZPolishAUnloadPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.ZPolishBLoadPositionMove, assign: v => ZPolishBLoadPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.ZPolishALoadPositionMove, assign: v => ZPolishALoadPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.ZPolishBUnloadPositionMove, assign: v => ZPolishBUnloadPositionMoveToggle = v),

            new PlcBatchReadItem<bool>(PolishASideArm2Address.AutoMode, assign: v => AutoModeToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.ManualMode, assign: v => ManualModeToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.PickFromTransfer, assign: v => PickFromTransferToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.PickFromPolishA, assign: v => PickFromPolishAToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.PickFromPolishB, assign: v => PickFromPolishBToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.PlaceToTransferArm, assign: v => PlaceToTransferArmToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.PickFromPolishBMaterial, assign: v => PickFromPolishBMaterialToggle = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.PolishBLoadUnload, assign: v => PolishBLoadUnloadToggle = v),

            new PlcBatchReadItem<bool>(PolishASideArm2Address.XEnableStatus, assign: v => XIsEnabled = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.XOriginStatus, assign: v => XIsAtOrigin = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.XJogUpStatus, assign: v => XJogUpActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.XJogDownStatus, assign: v => XJogDownActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.XJogMiddleStatus, assign: v => XJogMiddleActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.XLimitAlarm, assign: v => XLimitAlarm = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.XDriverAlarm, assign: v => XDriverAlarm = v),

            new PlcBatchReadItem<bool>(PolishASideArm2Address.ZEnableStatus, assign: v => ZIsEnabled = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.ZOriginStatus, assign: v => ZIsAtOrigin = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.ZJogUpStatus, assign: v => ZJogUpActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.ZJogDownStatus, assign: v => ZJogDownActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.ZJogMiddleStatus, assign: v => ZJogMiddleActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.ZLimitAlarm, assign: v => ZLimitAlarm = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.ZDriverAlarm, assign: v => ZDriverAlarm = v),

            new PlcBatchReadItem<bool>(PolishASideArm2Address.ZStandbyPositionArrived, assign: v => ZStandbyPositionArrived = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.ZTransferArmPositionArrived, assign: v => ZTransferArmPositionArrived = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.ZPolishAUnloadPositionArrived, assign: v => ZPolishAUnloadPositionArrived = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.ZPolishBLoadPositionArrived, assign: v => ZPolishBLoadPositionArrived = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.ZPolishALoadPositionArrived, assign: v => ZPolishALoadPositionArrived = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.ZPolishBUnloadPositionArrived, assign: v => ZPolishBUnloadPositionArrived = v),

            new PlcBatchReadItem<bool>(PolishASideArm2Address.StationResetStatus, assign: v => StationIsReset = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.StationRunningStatus, assign: v => StationIsRunning = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.StationStoppedStatus, assign: v => StationIsStopped = v),

            new PlcBatchReadItem<bool>(PolishASideArm2Address.ModuleAlarm, assign: v => ModuleAlarm = v),

            new PlcBatchReadItem<bool>(PolishASideArm2Address.PickFromTransferActiveStatus, assign: v => PickFromTransferActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.PickFromPolishAActiveStatus, assign: v => PickFromPolishAActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.PickFromPolishBActiveStatus, assign: v => PickFromPolishBActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.PlaceToTransferArmActiveStatus, assign: v => PlaceToTransferArmActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.PickFromPolishBMaterialActiveStatus, assign: v => PickFromPolishBMaterialActive = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.PolishBLoadUnloadActiveStatus, assign: v => PolishBLoadUnloadActive = v),

            new PlcBatchReadItem<bool>(PolishASideArm2Address.PickFromTransferDoneStatus, assign: v => PickFromTransferDone = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.PickFromPolishADoneStatus, assign: v => PickFromPolishADone = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.PickFromPolishBDoneStatus, assign: v => PickFromPolishBDone = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.PlaceToTransferArmDoneStatus, assign: v => PlaceToTransferArmDone = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.PickFromPolishBMaterialDoneStatus, assign: v => PickFromPolishBMaterialDone = v),
            new PlcBatchReadItem<bool>(PolishASideArm2Address.PolishBLoadUnloadDoneStatus, assign: v => PolishBLoadUnloadDone = v)
        };

        var dStatusBatchItems = new List<PlcBatchReadItemBase>
        {
            new PlcBatchReadItem<int>(PolishASideArm2Address.XCurrentCoordinate, assign: v => XCurrentCoordinate = v / 1000.0),
            new PlcBatchReadItem<int>(PolishASideArm2Address.XAlarmCode, assign: v => XAlarmCode = v),
            new PlcBatchReadItem<int>(PolishASideArm2Address.ZCurrentCoordinate, assign: v => ZCurrentCoordinate = v / 1000.0),
            new PlcBatchReadItem<int>(PolishASideArm2Address.ZAlarmCode, assign: v => ZAlarmCode = v)
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
        var x1013Task = _plc.ReadAsync<bool[]>(PolishASideArm2Address.Arm2Fiber1, 3); // X1013-X1015
        var b0BTask = _plc.ReadAsync<bool>(PolishASideArm2Address.AlarmReset); // B0B
        var w1A20Task = _plc.ReadAsync<short[]>(PolishASideArm2Address.FoupSlot, 2); // W1A20-W1A21

        var allTasks = new List<Task>
        {
            mBatchTask,
            dStatusBatchTask,
            editableBatchTask,
            x1013Task,
            b0BTask,
            w1A20Task
        };

        await Task.WhenAll(allTasks);

        if (!mBatchTask.Result.IsSuccess)
            Logger.For("RobotControl").Warn($"[PolishASideArm2] M batch read failed: {mBatchTask.Result.Message}");

        if (!dStatusBatchTask.Result.IsSuccess)
            Logger.For("RobotControl").Warn($"[PolishASideArm2] D status batch read failed: {dStatusBatchTask.Result.Message}");

        if (!editableBatchTask.Result.IsSuccess)
            Logger.For("RobotControl").Warn($"[PolishASideArm2] D editable batch read failed: {editableBatchTask.Result.Message}");

        if (x1013Task.Result.IsSuccess)
        {
            var x = x1013Task.Result.Content;
            Arm2Fiber1 = x[0];
            Arm2Fiber2 = x[1];
            Arm2Fiber3 = x[2];
        }

        if (b0BTask.Result.IsSuccess) AlarmResetToggle = b0BTask.Result.Content;

        if (w1A20Task.Result.IsSuccess)
        {
            var w = w1A20Task.Result.Content;
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

