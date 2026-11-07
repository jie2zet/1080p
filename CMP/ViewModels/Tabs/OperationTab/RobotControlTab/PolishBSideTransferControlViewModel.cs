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

public class PolishBSideTransferControlViewModel : TabContentViewModelBase
{
    private readonly MitsubishiPlcService _plc;
    private readonly DispatcherTimer _readTimer;
    private bool _isReading;
    private readonly EditableFloatItem _standbyPositionItem = new(PolishBSideTransferAddress.StandbyPosition, nameof(StandbyPosition), nameof(IsStandbyPositionDirty));
    private readonly EditableFloatItem _robotUpperArmPositionItem = new(PolishBSideTransferAddress.RobotUpperArmPosition, nameof(RobotUpperArmPosition), nameof(IsRobotUpperArmPositionDirty));
    private readonly EditableFloatItem _ltpPickPositionItem = new(PolishBSideTransferAddress.LtpPickPosition, nameof(LtpPickPosition), nameof(IsLtpPickPositionDirty));
    private readonly EditableFloatItem _robotLowerArmPositionItem = new(PolishBSideTransferAddress.RobotLowerArmPosition, nameof(RobotLowerArmPosition), nameof(IsRobotLowerArmPositionDirty));
    private readonly EditableFloatItem _ltpPlacePositionItem = new(PolishBSideTransferAddress.LtpPlacePosition, nameof(LtpPlacePosition), nameof(IsLtpPlacePositionDirty));
    private readonly EditableFloatItem _jogSpeedItem = new(PolishBSideTransferAddress.JogSpeed, nameof(JogSpeed), nameof(IsJogSpeedDirty));
    private readonly EditableFloatItem _autoSpeedItem = new(PolishBSideTransferAddress.AutoSpeed, nameof(AutoSpeed), nameof(IsAutoSpeedDirty));
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

    // 4. Polish B Door Open
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

    // 5. Polish B Door Close
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

    #endregion

    public PolishBSideTransferControlViewModel() : base(0, "研磨B侧中转")
    {
        _plc = GlobalService.RobotControlPlc;
        _editableItems = new List<EditableFloatItem>
        {
            _standbyPositionItem,
            _robotUpperArmPositionItem,
            _ltpPickPositionItem,
            _robotLowerArmPositionItem,
            _ltpPlacePositionItem,
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
            () => Logger.For("RobotControl").Debug("PolishBSideTransferControlViewModel activated, started PLC read timer."),
            () => Logger.For("RobotControl").Debug("PolishBSideTransferControlViewModel deactivated, stopped PLC read timer."));

        // Navigation Commands
        StationOperationCommand = new DelegateCommand(ExecuteStationOperation);
        BackCommand = new DelegateCommand(ExecuteBack);

        // Station Commands
        StationResetCommand = OperationAudit.CreateAsync("StationReset", ExecuteStationReset, () => "PolishBSideTransfer");
        StationStartCommand = OperationAudit.CreateAsync("StationStart", ExecuteStationStart, () => "PolishBSideTransfer");
        StationStopCommand = OperationAudit.CreateAsync("StationStop", ExecuteStationStop, () => "PolishBSideTransfer");
        StationAlarmResetCommand = OperationAudit.CreateAsync("StationAlarmReset", ExecuteStationAlarmReset, () => "PolishBSideTransfer");

        // Jog Commands
        PressJogUpCommand = new DelegateCommand(ExecutePressJogUp);
        ReleaseJogUpCommand = new DelegateCommand(ExecuteReleaseJogUp);
        PressJogDownCommand = new DelegateCommand(ExecutePressJogDown);
        ReleaseJogDownCommand = new DelegateCommand(ExecuteReleaseJogDown);

        // Toggle Button Commands
        StandbyPositionMoveCommand = OperationAudit.Create("StandbyPositionMove", () => _plc.Write(PolishBSideTransferAddress.StandbyPositionMove, !StandbyPositionMoveToggle), () => "PolishBSideTransfer");
        RobotUpperArmMoveCommand = OperationAudit.Create("RobotUpperArmMove", () => _plc.Write(PolishBSideTransferAddress.RobotUpperArmPositionMove, !RobotUpperArmMoveToggle), () => "PolishBSideTransfer");
        LtpPickPositionMoveCommand = OperationAudit.Create("LtpPickPositionMove", () => _plc.Write(PolishBSideTransferAddress.LtpPickPositionMove, !LtpPickPositionMoveToggle), () => "PolishBSideTransfer");
        RobotLowerArmMoveCommand = OperationAudit.Create("RobotLowerArmMove", () => _plc.Write(PolishBSideTransferAddress.RobotLowerArmPositionMove, !RobotLowerArmMoveToggle), () => "PolishBSideTransfer");
        LtpPlacePositionMoveCommand = OperationAudit.Create("LtpPlacePositionMove", () => _plc.Write(PolishBSideTransferAddress.LtpPlacePositionMove, !LtpPlacePositionMoveToggle), () => "PolishBSideTransfer");

        EnableCommand = OperationAudit.Create("Enable", () => _plc.ToggleBool(PolishBSideTransferAddress.Enable), () => "PolishBSideTransfer");
        OriginCommand = OperationAudit.Create("Origin", () =>
        {
            if (MessageBox.Show(Lang.Resources.ConfirmActionContent, Lang.Resources.ConfirmActionTips,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _plc.Write(PolishBSideTransferAddress.Origin, !OriginToggle);
            }

            OriginToggle = false;
        }, () => "PolishBSideTransfer");
        EmergencyStopCommand = OperationAudit.Create("EmergencyStop", () => _plc.Write(PolishBSideTransferAddress.EmergencyStop, !EmergencyStopToggle), () => "PolishBSideTransfer");
        AlarmResetCommand = OperationAudit.CreateAsync("AlarmReset", () => _plc.PulseAsync(PolishBSideTransferAddress.AlarmReset), () => "PolishBSideTransfer");
        SaveCommand = OperationAudit.Create("Save", () => _plc.Write(PolishBSideTransferAddress.Save, !SaveToggle), () => "PolishBSideTransfer");

        ManualModeCommand = OperationAudit.CreateAsync("ManualMode", () => _plc.PulseAsync(PolishBSideTransferAddress.ManualMode), () => "PolishBSideTransfer");
        AutoModeCommand = OperationAudit.CreateAsync("AutoMode", () => _plc.PulseAsync(PolishBSideTransferAddress.AutoMode), () => "PolishBSideTransfer");

        // New Left Operations Commands
        PickFromRobot2Command = OperationAudit.CreateAsync("PickFromRobot2", () => _plc.PulseAsync(PolishBSideTransferAddress.PickFromRobot2), () => "PolishBSideTransfer");
        PlaceToArm1Command = OperationAudit.CreateAsync("PlaceToArm1", () => _plc.PulseAsync(PolishBSideTransferAddress.PlaceToArm1), () => "PolishBSideTransfer");
        PlaceToArm2Command = OperationAudit.CreateAsync("PlaceToArm2", () => _plc.PulseAsync(PolishBSideTransferAddress.PlaceToArm2), () => "PolishBSideTransfer");
        PickFromArm3Command = OperationAudit.CreateAsync("PickFromArm3", () => _plc.PulseAsync(PolishBSideTransferAddress.PickFromArm3), () => "PolishBSideTransfer");
        PlaceToRobotCommand = OperationAudit.CreateAsync("PlaceToRobot", () => _plc.PulseAsync(PolishBSideTransferAddress.PlaceToRobot), () => "PolishBSideTransfer");
        PickFromArm2Command = OperationAudit.CreateAsync("PickFromArm2", () => _plc.PulseAsync(PolishBSideTransferAddress.PickFromArm2), () => "PolishBSideTransfer");

        // New Right Operations Commands
        ClampCloseCommand = OperationAudit.CreateAsync("ClampClose", () => _plc.PulseAsync(PolishBSideTransferAddress.ClampCloseWrite), () => "PolishBSideTransfer");
        ClampOpenCommand = OperationAudit.CreateAsync("ClampOpen", () => _plc.PulseAsync(PolishBSideTransferAddress.ClampOpenWrite), () => "PolishBSideTransfer");
        PolishADoorOpenCommand = OperationAudit.CreateAsync("PolishADoorOpen", () => _plc.PulseAsync(PolishBSideTransferAddress.PolishADoorOpenWrite), () => "PolishBSideTransfer");
        PolishADoorCloseCommand = OperationAudit.CreateAsync("PolishADoorClose", () => _plc.PulseAsync(PolishBSideTransferAddress.PolishADoorCloseWrite), () => "PolishBSideTransfer");
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
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.GlobalAlarm, assign: v => GlobalAlarm = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.Save, assign: v => SaveToggle = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.EmergencyStop, assign: v => EmergencyStopToggle = v),

            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.Origin, assign: v => OriginToggle = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.StandbyPositionMove, assign: v => StandbyPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.RobotUpperArmPositionMove, assign: v => RobotUpperArmMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.LtpPickPositionMove, assign: v => LtpPickPositionMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.RobotLowerArmPositionMove, assign: v => RobotLowerArmMoveToggle = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.LtpPlacePositionMove, assign: v => LtpPlacePositionMoveToggle = v),

            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.AutoMode, assign: v => AutoModeToggle = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.ManualMode, assign: v => ManualModeToggle = v),

            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.EnableStatus, assign: v => IsEnabled = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.OriginStatus, assign: v => IsAtOrigin = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.JogUpStatus, assign: v => JogUpActive = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.JogDownStatus, assign: v => JogDownActive = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.JogMiddleStatus, assign: v => JogMiddleActive = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.LimitAlarm, assign: v => LimitAlarm = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.DriverAlarm, assign: v => DriverAlarm = v),

            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.StationResetStatus, assign: v => StationIsReset = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.StationRunningStatus, assign: v => StationIsRunning = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.StationStoppedStatus, assign: v => StationIsStopped = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.ModuleAlarm, assign: v => ModuleAlarm = v),

            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.PickFromRobot2, assign: v => PickFromRobot2Toggle = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.PlaceToArm1, assign: v => PlaceToArm1Toggle = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.PlaceToArm2, assign: v => PlaceToArm2Toggle = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.PickFromArm3, assign: v => PickFromArm3Toggle = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.PlaceToRobot, assign: v => PlaceToRobotToggle = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.PickFromArm2, assign: v => PickFromArm2Toggle = v),

            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.PickFromRobot2ActiveStatus, assign: v => PickFromRobot2Active = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.PlaceToArm1ActiveStatus, assign: v => PlaceToArm1Active = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.PlaceToArm2ActiveStatus, assign: v => PlaceToArm2Active = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.PickFromArm3ActiveStatus, assign: v => PickFromArm3Active = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.PlaceToRobotActiveStatus, assign: v => PlaceToRobotActive = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.PickFromArm2ActiveStatus, assign: v => PickFromArm2Active = v),

            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.PickFromRobot2DoneStatus, assign: v => PickFromRobot2Done = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.PlaceToArm1DoneStatus, assign: v => PlaceToArm1Done = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.PlaceToArm2DoneStatus, assign: v => PlaceToArm2Done = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.PickFromArm3DoneStatus, assign: v => PickFromArm3Done = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.PlaceToRobotDoneStatus, assign: v => PlaceToRobotDone = v),
            new PlcBatchReadItem<bool>(PolishBSideTransferAddress.PickFromArm2DoneStatus, assign: v => PickFromArm2Done = v)
        };

        var dStatusBatchItems = new List<PlcBatchReadItemBase>
        {
            new PlcBatchReadItem<int>(PolishBSideTransferAddress.CurrentCoordinate, assign: v => CurrentCoordinate = v / 1000.0),
            new PlcBatchReadItem<int>(PolishBSideTransferAddress.AlarmCode, assign: v => AlarmCode = v)
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
        var y1046Task = _plc.ReadAsync<bool[]>(PolishBSideTransferAddress.PolishADoorOpenRead, 4); // Y1046-Y1049
        var x101cTask = _plc.ReadAsync<bool[]>(PolishBSideTransferAddress.PolishADoorOpenStatus, 2); // X101C-X101D
        var x1029Task = _plc.ReadAsync<bool[]>(PolishBSideTransferAddress.ClampCloseStatus, 3); // X1029-X102B
        var b0bTask = _plc.ReadAsync<bool>(PolishBSideTransferAddress.AlarmReset); // B0B

        await Task.WhenAll(mBatchTask, dStatusBatchTask, editableBatchTask, y1046Task, x101cTask, x1029Task, b0bTask);

        if (!mBatchTask.Result.IsSuccess)
            Logger.For("RobotControl").Warn($"[PolishBSideTransfer] M batch read failed: {mBatchTask.Result.Message}");

        if (!dStatusBatchTask.Result.IsSuccess)
            Logger.For("RobotControl").Warn($"[PolishBSideTransfer] D status batch read failed: {dStatusBatchTask.Result.Message}");

        if (!editableBatchTask.Result.IsSuccess)
            Logger.For("RobotControl").Warn($"[PolishBSideTransfer] D editable batch read failed: {editableBatchTask.Result.Message}");

        if (y1046Task.Result.IsSuccess)
        {
            var y = y1046Task.Result.Content;
            if (y.Length >= 4)
            {
                PolishADoorOpenToggle = y[0];
                PolishADoorCloseToggle = y[1];
                ClampCloseToggle = y[2];
                ClampOpenToggle = y[3];
            }
        }
        else
        {
            Logger.For("RobotControl").Warn($"[PolishBSideTransfer] Y1046-Y1049 read failed: {y1046Task.Result.Message}");
        }

        if (x101cTask.Result.IsSuccess)
        {
            var x = x101cTask.Result.Content;
            if (x.Length >= 2)
            {
                PolishADoorOpenStatus = x[0];
                PolishADoorCloseStatus = x[1];
            }
        }
        else
        {
            Logger.For("RobotControl").Warn($"[PolishBSideTransfer] X101C-X101D read failed: {x101cTask.Result.Message}");
        }

        if (x1029Task.Result.IsSuccess)
        {
            var x = x1029Task.Result.Content;
            if (x.Length >= 3)
            {
                ClampCloseStatus = x[0];
                ClampOpenStatus = x[1];
                WaferDetectionStatus = x[2];
            }
        }
        else
        {
            Logger.For("RobotControl").Warn($"[PolishBSideTransfer] X1029-X102B read failed: {x1029Task.Result.Message}");
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

    private Task ExecuteStationReset() => _plc.PulseAsync(PolishBSideTransferAddress.StationReset);

    private Task ExecuteStationStart() => _plc.PulseAsync(PolishBSideTransferAddress.StationStart);

    private Task ExecuteStationStop() => _plc.PulseAsync(PolishBSideTransferAddress.StationStop);

    private Task ExecuteStationAlarmReset() => _plc.PulseAsync(PolishBSideTransferAddress.StationAlarmReset);

    private void ExecutePressJogUp() => _plc.Write(PolishBSideTransferAddress.JogUp, true);

    private void ExecuteReleaseJogUp() => _plc.Write(PolishBSideTransferAddress.JogUp, false);

    private void ExecutePressJogDown() => _plc.Write(PolishBSideTransferAddress.JogDown, true);

    private void ExecuteReleaseJogDown() => _plc.Write(PolishBSideTransferAddress.JogDown, false);

    #endregion
}

