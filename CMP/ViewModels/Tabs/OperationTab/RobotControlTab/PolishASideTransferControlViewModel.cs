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

public class PolishASideTransferControlViewModel : TabContentViewModelBase
{
    private readonly MitsubishiPlcService _plc;
    private readonly DispatcherTimer _readTimer;
    private bool _isReading;
    private readonly EditableFloatItem _standbyPositionItem = new(PolishASideTransferAddress.StandbyPosition, nameof(StandbyPosition), nameof(IsStandbyPositionDirty));
    private readonly EditableFloatItem _robotUpperArmPositionItem = new(PolishASideTransferAddress.RobotUpperArmPosition, nameof(RobotUpperArmPosition), nameof(IsRobotUpperArmPositionDirty));
    private readonly EditableFloatItem _ltpPickPositionItem = new(PolishASideTransferAddress.LtpPickPosition, nameof(LtpPickPosition), nameof(IsLtpPickPositionDirty));
    private readonly EditableFloatItem _robotLowerArmPositionItem = new(PolishASideTransferAddress.RobotLowerArmPosition, nameof(RobotLowerArmPosition), nameof(IsRobotLowerArmPositionDirty));
    private readonly EditableFloatItem _ltpPlacePositionItem = new(PolishASideTransferAddress.LtpPlacePosition, nameof(LtpPlacePosition), nameof(IsLtpPlacePositionDirty));
    private readonly EditableFloatItem _speedWhenHasWaferItem = new(PolishASideTransferAddress.SpeedWhenHasWafer, nameof(SpeedWhenHasWafer), nameof(IsSpeedWhenHasWaferDirty));
    private readonly EditableFloatItem _jogSpeedItem = new(PolishASideTransferAddress.JogSpeed, nameof(JogSpeed), nameof(IsJogSpeedDirty));
    private readonly EditableFloatItem _autoSpeedItem = new(PolishASideTransferAddress.AutoSpeed, nameof(AutoSpeed), nameof(IsAutoSpeedDirty));
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

    #region 轴位置属性

    public double PositionMinimum { get; set; } = -90000;
    public double PositionMaximum { get; set; } = 90000;

    public bool IsStandbyPositionDirty => _standbyPositionItem.IsDirty;

    public double StandbyPosition
    {
        get => _standbyPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_standbyPositionItem, value, RaisePropertyChanged);
    }

    public bool IsRobotUpperArmPositionDirty => _robotUpperArmPositionItem.IsDirty;

    public double RobotUpperArmPosition
    {
        get => _robotUpperArmPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_robotUpperArmPositionItem, value, RaisePropertyChanged);
    }

    public bool IsLtpPickPositionDirty => _ltpPickPositionItem.IsDirty;

    public double LtpPickPosition
    {
        get => _ltpPickPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_ltpPickPositionItem, value, RaisePropertyChanged);
    }

    public bool IsRobotLowerArmPositionDirty => _robotLowerArmPositionItem.IsDirty;

    public double RobotLowerArmPosition
    {
        get => _robotLowerArmPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_robotLowerArmPositionItem, value, RaisePropertyChanged);
    }

    public bool IsLtpPlacePositionDirty => _ltpPlacePositionItem.IsDirty;

    public double LtpPlacePosition
    {
        get => _ltpPlacePositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_ltpPlacePositionItem, value, RaisePropertyChanged);
    }

    #endregion

    #region Move Toggle 属性 (只读，通过 Command 写入)

    private bool _standbyPositionMoveToggle;
    public bool StandbyPositionMoveToggle
    {
        get => _standbyPositionMoveToggle;
        set => SetProperty(ref _standbyPositionMoveToggle, value);
    }

    private bool _robotUpperArmMoveToggle;
    public bool RobotUpperArmMoveToggle
    {
        get => _robotUpperArmMoveToggle;
        set => SetProperty(ref _robotUpperArmMoveToggle, value);
    }

    private bool _ltpPickPositionMoveToggle;
    public bool LtpPickPositionMoveToggle
    {
        get => _ltpPickPositionMoveToggle;
        set => SetProperty(ref _ltpPickPositionMoveToggle, value);
    }

    private bool _robotLowerArmMoveToggle;
    public bool RobotLowerArmMoveToggle
    {
        get => _robotLowerArmMoveToggle;
        set => SetProperty(ref _robotLowerArmMoveToggle, value);
    }

    private bool _ltpPlacePositionMoveToggle;
    public bool LtpPlacePositionMoveToggle
    {
        get => _ltpPlacePositionMoveToggle;
        set => SetProperty(ref _ltpPlacePositionMoveToggle, value);
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

    public bool IsSpeedWhenHasWaferDirty => _speedWhenHasWaferItem.IsDirty;

    public double SpeedWhenHasWafer
    {
        get => _speedWhenHasWaferItem.Value;
        set => EditableFloatItemHelper.SetValue(_speedWhenHasWaferItem, value, RaisePropertyChanged);
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

    #region New Left Side Operations Properties (Dual Light)

    // Helper method to create property with backing field for brevity? No, keep standard pattern.

    // 1. Pick From Robot 2
    private bool _pickFromRobot2Toggle;
    public bool PickFromRobot2Toggle
    {
        get => _pickFromRobot2Toggle;
        set => SetProperty(ref _pickFromRobot2Toggle, value);
    }
    private bool _pickFromRobot2Active;
    public bool PickFromRobot2Active
    {
        get => _pickFromRobot2Active;
        set => SetProperty(ref _pickFromRobot2Active, value);
    }
    private bool _pickFromRobot2Done;
    public bool PickFromRobot2Done
    {
        get => _pickFromRobot2Done;
        set => SetProperty(ref _pickFromRobot2Done, value);
    }

    // 2. Place To Arm 1
    private bool _placeToArm1Toggle;
    public bool PlaceToArm1Toggle
    {
        get => _placeToArm1Toggle;
        set => SetProperty(ref _placeToArm1Toggle, value);
    }
    private bool _placeToArm1Active;
    public bool PlaceToArm1Active
    {
        get => _placeToArm1Active;
        set => SetProperty(ref _placeToArm1Active, value);
    }
    private bool _placeToArm1Done;
    public bool PlaceToArm1Done
    {
        get => _placeToArm1Done;
        set => SetProperty(ref _placeToArm1Done, value);
    }

    // 3. Place To Arm 2
    private bool _placeToArm2Toggle;
    public bool PlaceToArm2Toggle
    {
        get => _placeToArm2Toggle;
        set => SetProperty(ref _placeToArm2Toggle, value);
    }
    private bool _placeToArm2Active;
    public bool PlaceToArm2Active
    {
        get => _placeToArm2Active;
        set => SetProperty(ref _placeToArm2Active, value);
    }
    private bool _placeToArm2Done;
    public bool PlaceToArm2Done
    {
        get => _placeToArm2Done;
        set => SetProperty(ref _placeToArm2Done, value);
    }

    // 4. Pick From Arm 3
    private bool _pickFromArm3Toggle;
    public bool PickFromArm3Toggle
    {
        get => _pickFromArm3Toggle;
        set => SetProperty(ref _pickFromArm3Toggle, value);
    }
    private bool _pickFromArm3Active;
    public bool PickFromArm3Active
    {
        get => _pickFromArm3Active;
        set => SetProperty(ref _pickFromArm3Active, value);
    }
    private bool _pickFromArm3Done;
    public bool PickFromArm3Done
    {
        get => _pickFromArm3Done;
        set => SetProperty(ref _pickFromArm3Done, value);
    }

    // 5. Place To Robot
    private bool _placeToRobotToggle;
    public bool PlaceToRobotToggle
    {
        get => _placeToRobotToggle;
        set => SetProperty(ref _placeToRobotToggle, value);
    }
    private bool _placeToRobotActive;
    public bool PlaceToRobotActive
    {
        get => _placeToRobotActive;
        set => SetProperty(ref _placeToRobotActive, value);
    }
    private bool _placeToRobotDone;
    public bool PlaceToRobotDone
    {
        get => _placeToRobotDone;
        set => SetProperty(ref _placeToRobotDone, value);
    }

    // 6. Pick From Arm 2
    private bool _pickFromArm2Toggle;
    public bool PickFromArm2Toggle
    {
        get => _pickFromArm2Toggle;
        set => SetProperty(ref _pickFromArm2Toggle, value);
    }
    private bool _pickFromArm2Active;
    public bool PickFromArm2Active
    {
        get => _pickFromArm2Active;
        set => SetProperty(ref _pickFromArm2Active, value);
    }
    private bool _pickFromArm2Done;
    public bool PickFromArm2Done
    {
        get => _pickFromArm2Done;
        set => SetProperty(ref _pickFromArm2Done, value);
    }

    #endregion

    #region New Right Side Operations Properties (Single Light)

    // 1. Clamp Close
    private bool _clampCloseToggle;
    public bool ClampCloseToggle
    {
        get => _clampCloseToggle;
        set => SetProperty(ref _clampCloseToggle, value);
    }
    private bool _clampCloseStatus;
    public bool ClampCloseStatus
    {
        get => _clampCloseStatus;
        set => SetProperty(ref _clampCloseStatus, value);
    }

    // 2. Clamp Open
    private bool _clampOpenToggle;
    public bool ClampOpenToggle
    {
        get => _clampOpenToggle;
        set => SetProperty(ref _clampOpenToggle, value);
    }
    private bool _clampOpenStatus;
    public bool ClampOpenStatus
    {
        get => _clampOpenStatus;
        set => SetProperty(ref _clampOpenStatus, value);
    }

    // 3. Wafer Detection (Status only)
    private bool _waferDetectionStatus;
    public bool WaferDetectionStatus
    {
        get => _waferDetectionStatus;
        set => SetProperty(ref _waferDetectionStatus, value);
    }

    // 4. Polish A Door Open
    private bool _polishADoorOpenToggle;
    public bool PolishADoorOpenToggle
    {
        get => _polishADoorOpenToggle;
        set => SetProperty(ref _polishADoorOpenToggle, value);
    }
    private bool _polishADoorOpenStatus;
    public bool PolishADoorOpenStatus
    {
        get => _polishADoorOpenStatus;
        set => SetProperty(ref _polishADoorOpenStatus, value);
    }

    // 5. Polish A Door Close
    private bool _polishADoorCloseToggle;
    public bool PolishADoorCloseToggle
    {
        get => _polishADoorCloseToggle;
        set => SetProperty(ref _polishADoorCloseToggle, value);
    }
    private bool _polishADoorCloseStatus;
    public bool PolishADoorCloseStatus
    {
        get => _polishADoorCloseStatus;
        set => SetProperty(ref _polishADoorCloseStatus, value);
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

    // Toggle Button Commands (Axis Move)
    public DelegateCommand StandbyPositionMoveCommand { get; }
    public DelegateCommand RobotUpperArmMoveCommand { get; }
    public DelegateCommand LtpPickPositionMoveCommand { get; }
    public DelegateCommand RobotLowerArmMoveCommand { get; }
    public DelegateCommand LtpPlacePositionMoveCommand { get; }

    // Bottom Bar Commands
    public DelegateCommand EnableCommand { get; }
    public DelegateCommand OriginCommand { get; }
    public DelegateCommand EmergencyStopCommand { get; }
    public DelegateCommand AlarmResetCommand { get; }
    public DelegateCommand SaveCommand { get; }

    // Manual/Auto
    public DelegateCommand ManualModeCommand { get; }
    public DelegateCommand AutoModeCommand { get; }

    // New Left Operations Commands
    public DelegateCommand PickFromRobot2Command { get; }
    public DelegateCommand PlaceToArm1Command { get; }
    public DelegateCommand PlaceToArm2Command { get; }
    public DelegateCommand PickFromArm3Command { get; }
    public DelegateCommand PlaceToRobotCommand { get; }
    public DelegateCommand PickFromArm2Command { get; }

    // New Right Operations Commands
    public DelegateCommand ClampCloseCommand { get; }
    public DelegateCommand ClampOpenCommand { get; }
    public DelegateCommand PolishADoorOpenCommand { get; }
    public DelegateCommand PolishADoorCloseCommand { get; }

    public DelegateCommand StandbyPositionSetCommand => _standbyPositionItem.SetCommand;
    public DelegateCommand StandbyPositionCancelCommand => _standbyPositionItem.CancelCommand;
    public DelegateCommand RobotUpperArmPositionSetCommand => _robotUpperArmPositionItem.SetCommand;
    public DelegateCommand RobotUpperArmPositionCancelCommand => _robotUpperArmPositionItem.CancelCommand;
    public DelegateCommand LtpPickPositionSetCommand => _ltpPickPositionItem.SetCommand;
    public DelegateCommand LtpPickPositionCancelCommand => _ltpPickPositionItem.CancelCommand;
    public DelegateCommand RobotLowerArmPositionSetCommand => _robotLowerArmPositionItem.SetCommand;
    public DelegateCommand RobotLowerArmPositionCancelCommand => _robotLowerArmPositionItem.CancelCommand;
    public DelegateCommand LtpPlacePositionSetCommand => _ltpPlacePositionItem.SetCommand;
    public DelegateCommand LtpPlacePositionCancelCommand => _ltpPlacePositionItem.CancelCommand;
    public DelegateCommand JogSpeedSetCommand => _jogSpeedItem.SetCommand;
    public DelegateCommand JogSpeedCancelCommand => _jogSpeedItem.CancelCommand;
    public DelegateCommand AutoSpeedSetCommand => _autoSpeedItem.SetCommand;
    public DelegateCommand AutoSpeedCancelCommand => _autoSpeedItem.CancelCommand;
    public DelegateCommand SpeedWhenHasWaferSetCommand => _speedWhenHasWaferItem.SetCommand;
    public DelegateCommand SpeedWhenHasWaferCancelCommand => _speedWhenHasWaferItem.CancelCommand;

    #endregion

    public PolishASideTransferControlViewModel() : base(0, "研磨A测中转")
    {
        _plc = GlobalService.RobotControlPlc;
        _editableItems = new List<EditableFloatItem>
        {
            _standbyPositionItem,
            _robotUpperArmPositionItem,
            _ltpPickPositionItem,
            _robotLowerArmPositionItem,
            _ltpPlacePositionItem,
            _speedWhenHasWaferItem,
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
            () => Logger.For("RobotControl").Debug("PolishASideTransferControlViewModel activated, started PLC read timer."),
            () => Logger.For("RobotControl").Debug("PolishASideTransferControlViewModel deactivated, stopped PLC read timer."));

        // Navigation Commands
        StationOperationCommand = new DelegateCommand(ExecuteStationOperation);
        BackCommand = new DelegateCommand(ExecuteBack);

        // Station Commands
        StationResetCommand = OperationAudit.Create("StationReset", ExecuteStationReset, () => "PolishASideTransfer");
        StationStartCommand = OperationAudit.Create("StationStart", ExecuteStationStart, () => "PolishASideTransfer");
        StationStopCommand = OperationAudit.Create("StationStop", ExecuteStationStop, () => "PolishASideTransfer");
        StationAlarmResetCommand = OperationAudit.Create("StationAlarmReset", ExecuteStationAlarmReset, () => "PolishASideTransfer");

        // Jog Commands
        PressJogUpCommand = new DelegateCommand(ExecutePressJogUp);
        ReleaseJogUpCommand = new DelegateCommand(ExecuteReleaseJogUp);
        PressJogDownCommand = new DelegateCommand(ExecutePressJogDown);
        ReleaseJogDownCommand = new DelegateCommand(ExecuteReleaseJogDown);

        // Toggle Button Commands
        StandbyPositionMoveCommand = OperationAudit.Create("StandbyPositionMove", () => _plc.Write(PolishASideTransferAddress.StandbyPositionMove, !StandbyPositionMoveToggle), () => "PolishASideTransfer");
        RobotUpperArmMoveCommand = OperationAudit.Create("RobotUpperArmMove", () => _plc.Write(PolishASideTransferAddress.RobotUpperArmPositionMove, !RobotUpperArmMoveToggle), () => "PolishASideTransfer");
        LtpPickPositionMoveCommand = OperationAudit.Create("LtpPickPositionMove", () => _plc.Write(PolishASideTransferAddress.LtpPickPositionMove, !LtpPickPositionMoveToggle), () => "PolishASideTransfer");
        RobotLowerArmMoveCommand = OperationAudit.Create("RobotLowerArmMove", () => _plc.Write(PolishASideTransferAddress.RobotLowerArmPositionMove, !RobotLowerArmMoveToggle), () => "PolishASideTransfer");
        LtpPlacePositionMoveCommand = OperationAudit.Create("LtpPlacePositionMove", () => _plc.Write(PolishASideTransferAddress.LtpPlacePositionMove, !LtpPlacePositionMoveToggle), () => "PolishASideTransfer");

        EnableCommand = OperationAudit.Create("Enable", () => _plc.Write(PolishASideTransferAddress.Enable, !IsEnabled), () => "PolishASideTransfer");
        OriginCommand = OperationAudit.Create("Origin", () =>
        {
            if (MessageBox.Show(Lang.Resources.ConfirmActionContent, Lang.Resources.ConfirmActionTips,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _plc.Write(PolishASideTransferAddress.Origin, !OriginToggle);
            }

            OriginToggle = false;
        }, () => "PolishASideTransfer");
        EmergencyStopCommand = OperationAudit.Create("EmergencyStop", () => _plc.Write(PolishASideTransferAddress.EmergencyStop, !EmergencyStopToggle), () => "PolishASideTransfer");
        AlarmResetCommand = OperationAudit.Create("AlarmReset", () => _plc.Write(PolishASideTransferAddress.AlarmReset, !AlarmResetToggle), () => "PolishASideTransfer");
        SaveCommand = OperationAudit.Create("Save", () => _plc.Write(PolishASideTransferAddress.Save, !SaveToggle), () => "PolishASideTransfer");

        ManualModeCommand = OperationAudit.Create("ManualMode", () => _plc.Write(PolishASideTransferAddress.ManualMode, !ManualModeToggle), () => "PolishASideTransfer");
        AutoModeCommand = OperationAudit.Create("AutoMode", () => _plc.Write(PolishASideTransferAddress.AutoMode, !AutoModeToggle), () => "PolishASideTransfer");

        // New Left Operations Commands
        PickFromRobot2Command = OperationAudit.Create("PickFromRobot2", () => _plc.Write(PolishASideTransferAddress.PickFromRobot2, !PickFromRobot2Toggle), () => "PolishASideTransfer");
        PlaceToArm1Command = OperationAudit.Create("PlaceToArm1", () => _plc.Write(PolishASideTransferAddress.PlaceToArm1, !PlaceToArm1Toggle), () => "PolishASideTransfer");
        PlaceToArm2Command = OperationAudit.Create("PlaceToArm2", () => _plc.Write(PolishASideTransferAddress.PlaceToArm2, !PlaceToArm2Toggle), () => "PolishASideTransfer");
        PickFromArm3Command = OperationAudit.Create("PickFromArm3", () => _plc.Write(PolishASideTransferAddress.PickFromArm3, !PickFromArm3Toggle), () => "PolishASideTransfer");
        PlaceToRobotCommand = OperationAudit.Create("PlaceToRobot", () => _plc.Write(PolishASideTransferAddress.PlaceToRobot, !PlaceToRobotToggle), () => "PolishASideTransfer");
        PickFromArm2Command = OperationAudit.Create("PickFromArm2", () => _plc.Write(PolishASideTransferAddress.PickFromArm2, !PickFromArm2Toggle), () => "PolishASideTransfer");

        // New Right Operations Commands
        ClampCloseCommand = OperationAudit.Create("ClampClose", () => _plc.Write(PolishASideTransferAddress.ClampCloseWrite, !ClampCloseToggle), () => "PolishASideTransfer");
        ClampOpenCommand = OperationAudit.Create("ClampOpen", () => _plc.Write(PolishASideTransferAddress.ClampOpenWrite, !ClampOpenToggle), () => "PolishASideTransfer");
        PolishADoorOpenCommand = OperationAudit.Create("PolishADoorOpen", () => _plc.Write(PolishASideTransferAddress.PolishADoorWrite, !PolishADoorOpenToggle), () => "PolishASideTransfer");
        PolishADoorCloseCommand = OperationAudit.Create("PolishADoorClose", () => _plc.Write(PolishASideTransferAddress.PolishADoorCloseWrite, !PolishADoorCloseToggle), () => "PolishASideTransfer");
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
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.GlobalAlarm, assign: v => GlobalAlarm = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.Save, assign: v => SaveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.EmergencyStop, assign: v => EmergencyStopToggle = v),

            new PlcBatchReadItem<bool>(PolishASideTransferAddress.Origin, assign: v => OriginToggle = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.StandbyPositionMove, assign: v => StandbyPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.RobotUpperArmPositionMove, assign: v => RobotUpperArmMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.LtpPickPositionMove, assign: v => LtpPickPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.RobotLowerArmPositionMove, assign: v => RobotLowerArmMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.LtpPlacePositionMove, assign: v => LtpPlacePositionMoveToggle = v),

            new PlcBatchReadItem<bool>(PolishASideTransferAddress.AutoMode, assign: v => AutoModeToggle = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.ManualMode, assign: v => ManualModeToggle = v),

            new PlcBatchReadItem<bool>(PolishASideTransferAddress.EnableStatus, assign: v => IsEnabled = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.OriginStatus, assign: v => IsAtOrigin = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.JogUpStatus, assign: v => JogUpActive = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.JogDownStatus, assign: v => JogDownActive = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.JogMiddleStatus, assign: v => JogMiddleActive = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.LimitAlarm, assign: v => LimitAlarm = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.DriverAlarm, assign: v => DriverAlarm = v),

            new PlcBatchReadItem<bool>(PolishASideTransferAddress.StationResetStatus, assign: v => StationIsReset = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.StationRunningStatus, assign: v => StationIsRunning = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.StationStoppedStatus, assign: v => StationIsStopped = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.ModuleAlarm, assign: v => ModuleAlarm = v),

            new PlcBatchReadItem<bool>(PolishASideTransferAddress.PickFromRobot2, assign: v => PickFromRobot2Toggle = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.PlaceToArm1, assign: v => PlaceToArm1Toggle = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.PlaceToArm2, assign: v => PlaceToArm2Toggle = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.PickFromArm3, assign: v => PickFromArm3Toggle = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.PlaceToRobot, assign: v => PlaceToRobotToggle = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.PickFromArm2, assign: v => PickFromArm2Toggle = v),

            new PlcBatchReadItem<bool>(PolishASideTransferAddress.PickFromRobot2ActiveStatus, assign: v => PickFromRobot2Active = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.PlaceToArm1ActiveStatus, assign: v => PlaceToArm1Active = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.PlaceToArm2ActiveStatus, assign: v => PlaceToArm2Active = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.PickFromArm3ActiveStatus, assign: v => PickFromArm3Active = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.PlaceToRobotActiveStatus, assign: v => PlaceToRobotActive = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.PickFromArm2ActiveStatus, assign: v => PickFromArm2Active = v),

            new PlcBatchReadItem<bool>(PolishASideTransferAddress.PickFromRobot2DoneStatus, assign: v => PickFromRobot2Done = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.PlaceToArm1DoneStatus, assign: v => PlaceToArm1Done = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.PlaceToArm2DoneStatus, assign: v => PlaceToArm2Done = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.PickFromArm3DoneStatus, assign: v => PickFromArm3Done = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.PlaceToRobotDoneStatus, assign: v => PlaceToRobotDone = v),
            new PlcBatchReadItem<bool>(PolishASideTransferAddress.PickFromArm2DoneStatus, assign: v => PickFromArm2Done = v)
        };

        var dStatusBatchItems = new List<PlcBatchReadItemBase>
        {
            new PlcBatchReadItem<int>(PolishASideTransferAddress.CurrentCoordinate, assign: v => CurrentCoordinate = v / 1000),
            new PlcBatchReadItem<int>(PolishASideTransferAddress.AlarmCode, assign: v => AlarmCode = v)
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
        var y104aTask = _plc.ReadAsync<bool[]>(PolishASideTransferAddress.ClampCloseRead, 6); // Y104A-Y104F
        var x1019Task = _plc.ReadAsync<bool[]>(PolishASideTransferAddress.ClampCloseStatus, 7); // X1019-X101F
        var b0bTask = _plc.ReadAsync<bool>(PolishASideTransferAddress.AlarmReset); // B0B

        await Task.WhenAll(mBatchTask, dStatusBatchTask, editableBatchTask, y104aTask, x1019Task, b0bTask);

        if (!mBatchTask.Result.IsSuccess)
            Logger.For("RobotControl").Warn($"[PolishASideTransfer] M batch read failed: {mBatchTask.Result.Message}");

        if (!dStatusBatchTask.Result.IsSuccess)
            Logger.For("RobotControl").Warn($"[PolishASideTransfer] D status batch read failed: {dStatusBatchTask.Result.Message}");

        if (!editableBatchTask.Result.IsSuccess)
            Logger.For("RobotControl").Warn($"[PolishASideTransfer] D editable batch read failed: {editableBatchTask.Result.Message}");

        if (y104aTask.Result.IsSuccess)
        {
            var y = y104aTask.Result.Content;
            if (y.Length >= 6)
            {
                ClampCloseToggle = y[0];
                ClampOpenToggle = y[1];
                PolishADoorOpenToggle = y[4];
                PolishADoorCloseToggle = y[5];
            }
        }
        else
        {
            Logger.For("RobotControl").Warn($"[PolishASideTransfer] Y104A-Y104F read failed: {y104aTask.Result.Message}");
        }

        if (x1019Task.Result.IsSuccess)
        {
            var x = x1019Task.Result.Content;
            if (x.Length >= 7)
            {
                ClampCloseStatus = x[0];
                ClampOpenStatus = x[1];
                WaferDetectionStatus = x[2];
                PolishADoorOpenStatus = x[5];
                PolishADoorCloseStatus = x[6];
            }
        }
        else
        {
            Logger.For("RobotControl").Warn($"[PolishASideTransfer] X1019-X101F read failed: {x1019Task.Result.Message}");
        }

        if (b0bTask.Result.IsSuccess)
        {
            AlarmResetToggle = b0bTask.Result.Content;
        }
    }

    public void StopReading()
    {
        OnViewDeactivated();
    }

    #endregion

    #region Command 实现

    private void ExecuteStationOperation() => CurrentViewIndex = 1;

    private void ExecuteBack() => CurrentViewIndex = 0;

    private void ExecuteStationReset() => _plc.Write(PolishASideTransferAddress.StationReset, true);

    private void ExecuteStationStart() => _plc.Write(PolishASideTransferAddress.StationStart, true);

    private void ExecuteStationStop() => _plc.Write(PolishASideTransferAddress.StationStop, true);

    private void ExecuteStationAlarmReset() => _plc.Write(PolishASideTransferAddress.StationAlarmReset, true);

    private void ExecutePressJogUp() => _plc.Write(PolishASideTransferAddress.JogUp, true);

    private void ExecuteReleaseJogUp() => _plc.Write(PolishASideTransferAddress.JogUp, false);

    private void ExecutePressJogDown() => _plc.Write(PolishASideTransferAddress.JogDown, true);

    private void ExecuteReleaseJogDown() => _plc.Write(PolishASideTransferAddress.JogDown, false);

    #endregion
}

