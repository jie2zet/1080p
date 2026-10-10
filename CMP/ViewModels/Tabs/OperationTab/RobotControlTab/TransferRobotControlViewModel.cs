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

public class TransferRobotControlViewModel : TabContentViewModelBase
{
    private readonly MitsubishiPlcService _plc;
    private readonly DispatcherTimer _readTimer;
    private bool _isReading;

    public double PositionMinimum { get; set; } = -90000;
    public double PositionMaximum { get; set; } = 90000;

    private readonly EditableFloatItem _modulePickPositionItem = new(TransferRobotAddress.ModulePickPosition, nameof(ModulePickPosition), nameof(IsModulePickPositionDirty));
    private readonly EditableFloatItem _transferAPositionItem = new(TransferRobotAddress.TransferAPosition, nameof(TransferAPosition), nameof(IsTransferAPositionDirty));
    private readonly EditableFloatItem _transferBPositionItem = new(TransferRobotAddress.TransferBPosition, nameof(TransferBPosition), nameof(IsTransferBPositionDirty));
    private readonly EditableFloatItem _rollerA1PositionItem = new(TransferRobotAddress.RollerA1Position, nameof(RollerA1Position), nameof(IsRollerA1PositionDirty));
    private readonly EditableFloatItem _rollerA2PositionItem = new(TransferRobotAddress.RollerA2Position, nameof(RollerA2Position), nameof(IsRollerA2PositionDirty));
    private readonly EditableFloatItem _rollerB1PositionItem = new(TransferRobotAddress.RollerB1Position, nameof(RollerB1Position), nameof(IsRollerB1PositionDirty));
    private readonly EditableFloatItem _rollerB2PositionItem = new(TransferRobotAddress.RollerB2Position, nameof(RollerB2Position), nameof(IsRollerB2PositionDirty));
    private readonly EditableFloatItem _jogSpeedItem = new(TransferRobotAddress.JogSpeed, nameof(JogSpeed), nameof(IsJogSpeedDirty));
    private readonly EditableFloatItem _autoSpeedItem = new(TransferRobotAddress.AutoSpeed, nameof(AutoSpeed), nameof(IsAutoSpeedDirty));
    private readonly List<EditableFloatItem> _editableItems;
    private readonly EditableIntItem _programNoItem = new(TransferRobotAddress.ProgramNo, nameof(ProgramNo), nameof(IsProgramNoDirty));
    private readonly EditableIntItem _workAreaNoItem = new(TransferRobotAddress.WorkAreaNo, nameof(WorkAreaNo), nameof(IsWorkAreaNoDirty));
    private readonly List<EditableIntItem> _editableIntItems;

    #region Tab Title
    private string? _tabTitleKey;
    public string? TabTitleKey
    {
        get => _tabTitleKey;
        set { if (SetProperty(ref _tabTitleKey, value)) RefreshTabTitle(); }
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
    #endregion

    #region View Navigation
    private int _currentViewIndex;
    public int CurrentViewIndex { get => _currentViewIndex; set => SetProperty(ref _currentViewIndex, value); }
    #endregion

    #region 轴位置调整 - 位置设定 (D)

    public bool IsModulePickPositionDirty => _modulePickPositionItem.IsDirty;

    public double ModulePickPosition
    {
        get => _modulePickPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_modulePickPositionItem, value, RaisePropertyChanged);
    }

    public bool IsTransferAPositionDirty => _transferAPositionItem.IsDirty;

    public double TransferAPosition
    {
        get => _transferAPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_transferAPositionItem, value, RaisePropertyChanged);
    }

    public bool IsTransferBPositionDirty => _transferBPositionItem.IsDirty;

    public double TransferBPosition
    {
        get => _transferBPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_transferBPositionItem, value, RaisePropertyChanged);
    }

    public bool IsRollerA1PositionDirty => _rollerA1PositionItem.IsDirty;

    public double RollerA1Position
    {
        get => _rollerA1PositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_rollerA1PositionItem, value, RaisePropertyChanged);
    }

    public bool IsRollerA2PositionDirty => _rollerA2PositionItem.IsDirty;

    public double RollerA2Position
    {
        get => _rollerA2PositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_rollerA2PositionItem, value, RaisePropertyChanged);
    }

    public bool IsRollerB1PositionDirty => _rollerB1PositionItem.IsDirty;

    public double RollerB1Position
    {
        get => _rollerB1PositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_rollerB1PositionItem, value, RaisePropertyChanged);
    }

    public bool IsRollerB2PositionDirty => _rollerB2PositionItem.IsDirty;

    public double RollerB2Position
    {
        get => _rollerB2PositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_rollerB2PositionItem, value, RaisePropertyChanged);
    }

    #endregion

    #region 轴位置调整 - 移动状态 (M)

    // Toggle (Command)
    private bool _modulePickMoveToggle;
    public bool ModulePickMoveToggle { get => _modulePickMoveToggle; set => SetProperty(ref _modulePickMoveToggle, value); }

    private bool _transferAMoveToggle;
    public bool TransferAMoveToggle { get => _transferAMoveToggle; set => SetProperty(ref _transferAMoveToggle, value); }

    private bool _transferBMoveToggle;
    public bool TransferBMoveToggle { get => _transferBMoveToggle; set => SetProperty(ref _transferBMoveToggle, value); }

    private bool _rollerA1MoveToggle;
    public bool RollerA1MoveToggle { get => _rollerA1MoveToggle; set => SetProperty(ref _rollerA1MoveToggle, value); }

    private bool _rollerA2MoveToggle;
    public bool RollerA2MoveToggle { get => _rollerA2MoveToggle; set => SetProperty(ref _rollerA2MoveToggle, value); }

    private bool _rollerB1MoveToggle;
    public bool RollerB1MoveToggle { get => _rollerB1MoveToggle; set => SetProperty(ref _rollerB1MoveToggle, value); }

    private bool _rollerB2MoveToggle;
    public bool RollerB2MoveToggle { get => _rollerB2MoveToggle; set => SetProperty(ref _rollerB2MoveToggle, value); }

    // Status (Light)
    private bool _modulePickMoveStatus;
    public bool ModulePickMoveStatus { get => _modulePickMoveStatus; set => SetProperty(ref _modulePickMoveStatus, value); }

    private bool _transferAMoveStatus;
    public bool TransferAMoveStatus { get => _transferAMoveStatus; set => SetProperty(ref _transferAMoveStatus, value); }

    private bool _transferBMoveStatus;
    public bool TransferBMoveStatus { get => _transferBMoveStatus; set => SetProperty(ref _transferBMoveStatus, value); }

    private bool _rollerA1MoveStatus;
    public bool RollerA1MoveStatus { get => _rollerA1MoveStatus; set => SetProperty(ref _rollerA1MoveStatus, value); }

    private bool _rollerA2MoveStatus;
    public bool RollerA2MoveStatus { get => _rollerA2MoveStatus; set => SetProperty(ref _rollerA2MoveStatus, value); }

    private bool _rollerB1MoveStatus;
    public bool RollerB1MoveStatus { get => _rollerB1MoveStatus; set => SetProperty(ref _rollerB1MoveStatus, value); }

    private bool _rollerB2MoveStatus;
    public bool RollerB2MoveStatus { get => _rollerB2MoveStatus; set => SetProperty(ref _rollerB2MoveStatus, value); }

    #endregion

    #region 速度和坐标

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
    public double CurrentCoordinate { get => _currentCoordinate; set => SetProperty(ref _currentCoordinate, value); }

    #endregion

    #region 状态与控制 (M)

    // Enable
    private bool _isEnabled;
    public bool IsEnabled { get => _isEnabled; set => SetProperty(ref _isEnabled, value); }

    // Jog
    private bool _jogUpActive;
    public bool JogUpActive { get => _jogUpActive; set => SetProperty(ref _jogUpActive, value); }

    private bool _jogMiddleActive;
    public bool JogMiddleActive { get => _jogMiddleActive; set => SetProperty(ref _jogMiddleActive, value); }

    private bool _jogDownActive;
    public bool JogDownActive { get => _jogDownActive; set => SetProperty(ref _jogDownActive, value); }

    // Origin
    private bool _isAtOrigin;
    public bool IsAtOrigin { get => _isAtOrigin; set => SetProperty(ref _isAtOrigin, value); }

    private bool _originToggle;
    public bool OriginToggle { get => _originToggle; set => SetProperty(ref _originToggle, value); }

    // Alarms
    private bool _driverAlarm;
    public bool DriverAlarm { get => _driverAlarm; set => SetProperty(ref _driverAlarm, value); }

    private bool _limitAlarm;
    public bool LimitAlarm { get => _limitAlarm; set => SetProperty(ref _limitAlarm, value); }

    private int _alarmCode;
    public int AlarmCode { get => _alarmCode; set => SetProperty(ref _alarmCode, value); }

    #endregion

    #region 机器人控制 - 状态与命令

    public bool IsProgramNoDirty => _programNoItem.IsDirty;

    public int ProgramNo
    {
        get => _programNoItem.Value;
        set => EditableIntItemHelper.SetValue(_programNoItem, value, RaisePropertyChanged);
    }

    public bool IsWorkAreaNoDirty => _workAreaNoItem.IsDirty;

    public int WorkAreaNo
    {
        get => _workAreaNoItem.Value;
        set => EditableIntItemHelper.SetValue(_workAreaNoItem, value, RaisePropertyChanged);
    }

    // Robot Control Status
    private bool _robotStartStatus;
    public bool RobotStartStatus { get => _robotStartStatus; set => SetProperty(ref _robotStartStatus, value); }

    private bool _robotPauseStatus;
    public bool RobotPauseStatus { get => _robotPauseStatus; set => SetProperty(ref _robotPauseStatus, value); }

    // Toggle for Pause button state (if it behaves like a toggle)
    private bool _robotPauseToggle;
    public bool RobotPauseToggle { get => _robotPauseToggle; set => SetProperty(ref _robotPauseToggle, value); }

    private bool _robotStopStatus;
    public bool RobotStopStatus { get => _robotStopStatus; set => SetProperty(ref _robotStopStatus, value); }

    private bool _robotResetStatus;
    public bool RobotResetStatus { get => _robotResetStatus; set => SetProperty(ref _robotResetStatus, value); }

    // Operation Buttons Toggle/Status
    private bool _workAreaPickToggle;
    public bool WorkAreaPickToggle { get => _workAreaPickToggle; set => SetProperty(ref _workAreaPickToggle, value); }

    private bool _workAreaPlaceToggle;
    public bool WorkAreaPlaceToggle { get => _workAreaPlaceToggle; set => SetProperty(ref _workAreaPlaceToggle, value); }

    private bool _waitConfirmToggle;
    public bool WaitConfirmToggle { get => _waitConfirmToggle; set => SetProperty(ref _waitConfirmToggle, value); }

    private bool _autoModeToggle;
    public bool AutoModeToggle { get => _autoModeToggle; set => SetProperty(ref _autoModeToggle, value); } // Robot Auto Mode

    // Operation Status (Green Lights)
    private bool _workAreaPickStatus;
    public bool WorkAreaPickStatus { get => _workAreaPickStatus; set => SetProperty(ref _workAreaPickStatus, value); }

    private bool _workAreaPlaceStatus;
    public bool WorkAreaPlaceStatus { get => _workAreaPlaceStatus; set => SetProperty(ref _workAreaPlaceStatus, value); }

    private bool _waitConfirmStatus;
    public bool WaitConfirmStatus { get => _waitConfirmStatus; set => SetProperty(ref _waitConfirmStatus, value); }

    private bool _autoModeStatus;
    public bool AutoModeStatus { get => _autoModeStatus; set => SetProperty(ref _autoModeStatus, value); }

    // Arm Control
    private bool _useUpperArmToggle;
    public bool UseUpperArmToggle { get => _useUpperArmToggle; set => SetProperty(ref _useUpperArmToggle, value); }

    private bool _useLowerArmToggle;
    public bool UseLowerArmToggle { get => _useLowerArmToggle; set => SetProperty(ref _useLowerArmToggle, value); }

    // Confirm Switch
    private bool _confirmSwitchToggle;
    public bool ConfirmSwitchToggle { get => _confirmSwitchToggle; set => SetProperty(ref _confirmSwitchToggle, value); }

    // Turn
    private bool _turnTo0Toggle;
    public bool TurnTo0Toggle { get => _turnTo0Toggle; set => SetProperty(ref _turnTo0Toggle, value); }

    private bool _turnTo0Status;
    public bool TurnTo0Status { get => _turnTo0Status; set => SetProperty(ref _turnTo0Status, value); }

    private bool _turnTo180Toggle;
    public bool TurnTo180Toggle { get => _turnTo180Toggle; set => SetProperty(ref _turnTo180Toggle, value); }

    private bool _turnTo180Status;
    public bool TurnTo180Status { get => _turnTo180Status; set => SetProperty(ref _turnTo180Status, value); }


    // Sensors
    private bool _upperArmSensorOn;
    public bool UpperArmSensorOn { get => _upperArmSensorOn; set => SetProperty(ref _upperArmSensorOn, value); }

    private bool _lowerArmSensorOn;
    public bool LowerArmSensorOn { get => _lowerArmSensorOn; set => SetProperty(ref _lowerArmSensorOn, value); }

    private bool _commandReceivedOn;
    public bool CommandReceivedOn { get => _commandReceivedOn; set => SetProperty(ref _commandReceivedOn, value); }

    private bool _robotSafePositionOn;
    public bool RobotSafePositionOn { get => _robotSafePositionOn; set => SetProperty(ref _robotSafePositionOn, value); }

    #endregion

    #region 底部状态栏

    private bool _emergencyStopToggle;
    public bool EmergencyStopToggle { get => _emergencyStopToggle; set => SetProperty(ref _emergencyStopToggle, value); }

    private bool _alarmResetToggle;
    public bool AlarmResetToggle { get => _alarmResetToggle; set => SetProperty(ref _alarmResetToggle, value); }

    private bool _saveToggle;
    public bool SaveToggle { get => _saveToggle; set => SetProperty(ref _saveToggle, value); }

    #endregion

    #region 工位操作 (Station Operation)

    // Top Buttons Status
    private bool _stationIsReset;
    public bool StationIsReset { get => _stationIsReset; set => SetProperty(ref _stationIsReset, value); }

    private bool _stationIsRunning;
    public bool StationIsRunning { get => _stationIsRunning; set => SetProperty(ref _stationIsRunning, value); }

    private bool _stationIsStopped;
    public bool StationIsStopped { get => _stationIsStopped; set => SetProperty(ref _stationIsStopped, value); }

    // Alarms
    private bool _moduleAlarmOn;
    public bool ModuleAlarmOn { get => _moduleAlarmOn; set => SetProperty(ref _moduleAlarmOn, value); }

    private bool _globalAlarmOn;
    public bool GlobalAlarmOn { get => _globalAlarmOn; set => SetProperty(ref _globalAlarmOn, value); }


    // Operation Buttons
    // Module Pick
    private bool _stationModulePickToggle;
    public bool StationModulePickToggle { get => _stationModulePickToggle; set => SetProperty(ref _stationModulePickToggle, value); }
    private bool _stationModulePickActive;
    public bool StationModulePickActive { get => _stationModulePickActive; set => SetProperty(ref _stationModulePickActive, value); }
    private bool _stationModulePickDone;
    public bool StationModulePickDone { get => _stationModulePickDone; set => SetProperty(ref _stationModulePickDone, value); }

    // Transfer A Place
    private bool _stationTransferAPlaceToggle;
    public bool StationTransferAPlaceToggle { get => _stationTransferAPlaceToggle; set => SetProperty(ref _stationTransferAPlaceToggle, value); }
    private bool _stationTransferAPlaceActive;
    public bool StationTransferAPlaceActive { get => _stationTransferAPlaceActive; set => SetProperty(ref _stationTransferAPlaceActive, value); }
    private bool _stationTransferAPlaceDone;
    public bool StationTransferAPlaceDone { get => _stationTransferAPlaceDone; set => SetProperty(ref _stationTransferAPlaceDone, value); }

    // Transfer A Pick
    private bool _stationTransferAPickToggle;
    public bool StationTransferAPickToggle { get => _stationTransferAPickToggle; set => SetProperty(ref _stationTransferAPickToggle, value); }
    private bool _stationTransferAPickActive;
    public bool StationTransferAPickActive { get => _stationTransferAPickActive; set => SetProperty(ref _stationTransferAPickActive, value); }
    private bool _stationTransferAPickDone;
    public bool StationTransferAPickDone { get => _stationTransferAPickDone; set => SetProperty(ref _stationTransferAPickDone, value); }

    // Transfer B Place
    private bool _stationTransferBPlaceToggle;
    public bool StationTransferBPlaceToggle { get => _stationTransferBPlaceToggle; set => SetProperty(ref _stationTransferBPlaceToggle, value); }
    private bool _stationTransferBPlaceActive;
    public bool StationTransferBPlaceActive { get => _stationTransferBPlaceActive; set => SetProperty(ref _stationTransferBPlaceActive, value); }
    private bool _stationTransferBPlaceDone;
    public bool StationTransferBPlaceDone { get => _stationTransferBPlaceDone; set => SetProperty(ref _stationTransferBPlaceDone, value); }

    // Transfer B Pick
    private bool _stationTransferBPickToggle;
    public bool StationTransferBPickToggle { get => _stationTransferBPickToggle; set => SetProperty(ref _stationTransferBPickToggle, value); }
    private bool _stationTransferBPickActive;
    public bool StationTransferBPickActive { get => _stationTransferBPickActive; set => SetProperty(ref _stationTransferBPickActive, value); }
    private bool _stationTransferBPickDone;
    public bool StationTransferBPickDone { get => _stationTransferBPickDone; set => SetProperty(ref _stationTransferBPickDone, value); }

    // Roller A1 Place
    private bool _stationRollerA1PlaceToggle;
    public bool StationRollerA1PlaceToggle { get => _stationRollerA1PlaceToggle; set => SetProperty(ref _stationRollerA1PlaceToggle, value); }
    private bool _stationRollerA1PlaceActive;
    public bool StationRollerA1PlaceActive { get => _stationRollerA1PlaceActive; set => SetProperty(ref _stationRollerA1PlaceActive, value); }
    private bool _stationRollerA1PlaceDone;
    public bool StationRollerA1PlaceDone { get => _stationRollerA1PlaceDone; set => SetProperty(ref _stationRollerA1PlaceDone, value); }

    // Roller A2 Place
    private bool _stationRollerA2PlaceToggle;
    public bool StationRollerA2PlaceToggle { get => _stationRollerA2PlaceToggle; set => SetProperty(ref _stationRollerA2PlaceToggle, value); }
    private bool _stationRollerA2PlaceActive;
    public bool StationRollerA2PlaceActive { get => _stationRollerA2PlaceActive; set => SetProperty(ref _stationRollerA2PlaceActive, value); }
    private bool _stationRollerA2PlaceDone;
    public bool StationRollerA2PlaceDone { get => _stationRollerA2PlaceDone; set => SetProperty(ref _stationRollerA2PlaceDone, value); }

    // Roller B1 Place
    private bool _stationRollerB1PlaceToggle;
    public bool StationRollerB1PlaceToggle { get => _stationRollerB1PlaceToggle; set => SetProperty(ref _stationRollerB1PlaceToggle, value); }
    private bool _stationRollerB1PlaceActive;
    public bool StationRollerB1PlaceActive { get => _stationRollerB1PlaceActive; set => SetProperty(ref _stationRollerB1PlaceActive, value); }
    private bool _stationRollerB1PlaceDone;
    public bool StationRollerB1PlaceDone { get => _stationRollerB1PlaceDone; set => SetProperty(ref _stationRollerB1PlaceDone, value); }

    // Roller B2 Place
    private bool _stationRollerB2PlaceToggle;
    public bool StationRollerB2PlaceToggle { get => _stationRollerB2PlaceToggle; set => SetProperty(ref _stationRollerB2PlaceToggle, value); }
    private bool _stationRollerB2PlaceActive;
    public bool StationRollerB2PlaceActive { get => _stationRollerB2PlaceActive; set => SetProperty(ref _stationRollerB2PlaceActive, value); }
    private bool _stationRollerB2PlaceDone;
    public bool StationRollerB2PlaceDone { get => _stationRollerB2PlaceDone; set => SetProperty(ref _stationRollerB2PlaceDone, value); }

    // Manual / Auto
    private bool _manualModeStationToggle;
    public bool ManualModeStationToggle { get => _manualModeStationToggle; set => SetProperty(ref _manualModeStationToggle, value); }

    private bool _autoModeStationToggle;
    public bool AutoModeStationToggle { get => _autoModeStationToggle; set => SetProperty(ref _autoModeStationToggle, value); }

    #endregion

    #region Commands

    public DelegateCommand StationOperationCommand { get; }
    public DelegateCommand BackCommand { get; }

    // Move Commands
    public DelegateCommand ModulePickMoveCommand { get; }
    public DelegateCommand TransferAMoveCommand { get; }
    public DelegateCommand TransferBMoveCommand { get; }
    public DelegateCommand RollerA1MoveCommand { get; }
    public DelegateCommand RollerA2MoveCommand { get; }
    public DelegateCommand RollerB1MoveCommand { get; }
    public DelegateCommand RollerB2MoveCommand { get; }

    // Axis Control Commands
    public DelegateCommand EnableCommand { get; }
    public DelegateCommand OriginCommand { get; }
    public DelegateCommand PressJogUpCommand { get; }
    public DelegateCommand ReleaseJogUpCommand { get; }
    public DelegateCommand PressJogDownCommand { get; }
    public DelegateCommand ReleaseJogDownCommand { get; }

    // Robot Control Commands
    public DelegateCommand RobotStartCommand { get; }
    public DelegateCommand RobotPauseCommand { get; }
    public DelegateCommand RobotStopCommand { get; }
    public DelegateCommand RobotResetCommand { get; }

    public DelegateCommand WorkAreaPickCommand { get; }
    public DelegateCommand WorkAreaPlaceCommand { get; }
    public DelegateCommand WaitConfirmCommand { get; }
    public DelegateCommand AutoModeCommand { get; } // Robot Auto
    public DelegateCommand UseUpperArmCommand { get; }
    public DelegateCommand UseLowerArmCommand { get; }
    public DelegateCommand ConfirmSwitchCommand { get; }
    public DelegateCommand TurnTo0Command { get; }
    public DelegateCommand TurnTo180Command { get; }
    public DelegateCommand RobotSafePositionCommand { get; }

    // Bottom Bar Commands
    public DelegateCommand EmergencyStopCommand { get; }
    public DelegateCommand AlarmResetCommand { get; }
    public DelegateCommand SaveCommand { get; }

    // Station Commands
    public DelegateCommand StationResetCommand { get; }
    public DelegateCommand StationStartCommand { get; }
    public DelegateCommand StationStopCommand { get; }
    public DelegateCommand StationAlarmResetCommand { get; }

    public DelegateCommand StationModulePickCommand { get; }
    public DelegateCommand StationTransferAPlaceCommand { get; }
    public DelegateCommand StationTransferAPickCommand { get; }
    public DelegateCommand StationTransferBPlaceCommand { get; }
    public DelegateCommand StationTransferBPickCommand { get; }
    public DelegateCommand StationRollerA1PlaceCommand { get; }
    public DelegateCommand StationRollerA2PlaceCommand { get; }
    public DelegateCommand StationRollerB1PlaceCommand { get; }
    public DelegateCommand StationRollerB2PlaceCommand { get; }

    public DelegateCommand ManualModeStationCommand { get; }
    public DelegateCommand AutoModeStationCommand { get; }

    public DelegateCommand ModulePickPositionSetCommand => _modulePickPositionItem.SetCommand;
    public DelegateCommand ModulePickPositionCancelCommand => _modulePickPositionItem.CancelCommand;
    public DelegateCommand TransferAPositionSetCommand => _transferAPositionItem.SetCommand;
    public DelegateCommand TransferAPositionCancelCommand => _transferAPositionItem.CancelCommand;
    public DelegateCommand TransferBPositionSetCommand => _transferBPositionItem.SetCommand;
    public DelegateCommand TransferBPositionCancelCommand => _transferBPositionItem.CancelCommand;
    public DelegateCommand RollerA1PositionSetCommand => _rollerA1PositionItem.SetCommand;
    public DelegateCommand RollerA1PositionCancelCommand => _rollerA1PositionItem.CancelCommand;
    public DelegateCommand RollerA2PositionSetCommand => _rollerA2PositionItem.SetCommand;
    public DelegateCommand RollerA2PositionCancelCommand => _rollerA2PositionItem.CancelCommand;
    public DelegateCommand RollerB1PositionSetCommand => _rollerB1PositionItem.SetCommand;
    public DelegateCommand RollerB1PositionCancelCommand => _rollerB1PositionItem.CancelCommand;
    public DelegateCommand RollerB2PositionSetCommand => _rollerB2PositionItem.SetCommand;
    public DelegateCommand RollerB2PositionCancelCommand => _rollerB2PositionItem.CancelCommand;
    public DelegateCommand JogSpeedSetCommand => _jogSpeedItem.SetCommand;
    public DelegateCommand JogSpeedCancelCommand => _jogSpeedItem.CancelCommand;
    public DelegateCommand AutoSpeedSetCommand => _autoSpeedItem.SetCommand;
    public DelegateCommand AutoSpeedCancelCommand => _autoSpeedItem.CancelCommand;
    public DelegateCommand ProgramNoSetCommand => _programNoItem.SetCommand;
    public DelegateCommand ProgramNoCancelCommand => _programNoItem.CancelCommand;
    public DelegateCommand WorkAreaNoSetCommand => _workAreaNoItem.SetCommand;
    public DelegateCommand WorkAreaNoCancelCommand => _workAreaNoItem.CancelCommand;

    #endregion

    public TransferRobotControlViewModel() : base(0, "中转机器人")
    {
        _plc = GlobalService.RobotControlPlc;
        _editableItems = new List<EditableFloatItem>
        {
            _modulePickPositionItem,
            _transferAPositionItem,
            _transferBPositionItem,
            _rollerA1PositionItem,
            _rollerA2PositionItem,
            _rollerB1PositionItem,
            _rollerB2PositionItem,
            _jogSpeedItem,
            _autoSpeedItem
        };
        EditableFloatItemHelper.InitializeCommands(_editableItems, _plc, RaisePropertyChanged);
        _editableIntItems = new List<EditableIntItem>
        {
            _programNoItem,
            _workAreaNoItem
        };
        EditableIntItemHelper.InitializeCommands(
            _editableIntItems,
            (address, value) =>
            {
                _plc.Write(address, value * 1000);
            },
            RaisePropertyChanged);

        _readTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _readTimer.Tick += ReadTimer_Tick;
        RegisterLifecycleTimer(_readTimer);
        RegisterViewLifecycleAction(
            () => Logger.For("RobotControl").Debug("TransferRobotControlViewModel activated, started PLC read timer."),
            () => Logger.For("RobotControl").Debug("TransferRobotControlViewModel deactivated, stopped PLC read timer."));

        // Navigation
        StationOperationCommand = new DelegateCommand(() => CurrentViewIndex = 1);
        BackCommand = new DelegateCommand(() => CurrentViewIndex = 0);

        // Move Commands (Toggle)
        ModulePickMoveCommand = OperationAudit.Create("ModulePickMove", () => _plc.Write(TransferRobotAddress.ModulePickMove, !ModulePickMoveToggle), () => "TransferRobot");
        TransferAMoveCommand = OperationAudit.Create("TransferAMove", () => _plc.Write(TransferRobotAddress.TransferAMove, !TransferAMoveToggle), () => "TransferRobot");
        TransferBMoveCommand = OperationAudit.Create("TransferBMove", () => _plc.Write(TransferRobotAddress.TransferBMove, !TransferBMoveToggle), () => "TransferRobot");
        RollerA1MoveCommand = OperationAudit.Create("RollerA1Move", () => _plc.Write(TransferRobotAddress.RollerA1Move, !RollerA1MoveToggle), () => "TransferRobot");
        RollerA2MoveCommand = OperationAudit.Create("RollerA2Move", () => _plc.Write(TransferRobotAddress.RollerA2Move, !RollerA2MoveToggle), () => "TransferRobot");
        RollerB1MoveCommand = OperationAudit.Create("RollerB1Move", () => _plc.Write(TransferRobotAddress.RollerB1Move, !RollerB1MoveToggle), () => "TransferRobot");
        RollerB2MoveCommand = OperationAudit.Create("RollerB2Move", () => _plc.Write(TransferRobotAddress.RollerB2Move, !RollerB2MoveToggle), () => "TransferRobot");

        // Axis Control
        EnableCommand = OperationAudit.Create("Enable", () => _plc.Write(TransferRobotAddress.Enable, !IsEnabled), () => "TransferRobot");
        OriginCommand = OperationAudit.Create("Origin", () =>
        {
            if (MessageBox.Show(Lang.Resources.ConfirmActionContent, Lang.Resources.ConfirmActionTips,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _plc.Write(TransferRobotAddress.Origin, !OriginToggle);
            }

            OriginToggle = false;
        }, () => "TransferRobot");
        PressJogUpCommand = new DelegateCommand(() => _plc.Write(TransferRobotAddress.JogUp, true));
        ReleaseJogUpCommand = new DelegateCommand(() => _plc.Write(TransferRobotAddress.JogUp, false));
        PressJogDownCommand = new DelegateCommand(() => _plc.Write(TransferRobotAddress.JogDown, true));
        ReleaseJogDownCommand = new DelegateCommand(() => _plc.Write(TransferRobotAddress.JogDown, false));

        // Robot Control
        RobotStartCommand = OperationAudit.CreateAsync("RobotStart", () => _plc.PulseAsync(TransferRobotAddress.RobotStart), () => "TransferRobot");
        RobotPauseCommand = OperationAudit.Create("RobotPause", () => _plc.Write(TransferRobotAddress.RobotPause, !RobotPauseToggle), () => "TransferRobot");
        RobotStopCommand = OperationAudit.CreateAsync("RobotStop", () => _plc.PulseAsync(TransferRobotAddress.RobotStop), () => "TransferRobot");
        RobotResetCommand = OperationAudit.CreateAsync("RobotReset", () => _plc.PulseAsync(TransferRobotAddress.RobotReset), () => "TransferRobot");

        WorkAreaPickCommand = OperationAudit.Create("WorkAreaPick", () => _plc.Write(TransferRobotAddress.WorkAreaPick, !WorkAreaPickToggle), () => "TransferRobot");
        WorkAreaPlaceCommand = OperationAudit.Create("WorkAreaPlace", () => _plc.Write(TransferRobotAddress.WorkAreaPlace, !WorkAreaPlaceToggle), () => "TransferRobot");
        WaitConfirmCommand = OperationAudit.Create("WaitConfirm", () => _plc.Write(TransferRobotAddress.WaitConfirm, !WaitConfirmToggle), () => "TransferRobot");
        AutoModeCommand = OperationAudit.Create("AutoMode", () => _plc.Write(TransferRobotAddress.AutoMode, !AutoModeToggle), () => "TransferRobot");
        UseUpperArmCommand = OperationAudit.Create("UseUpperArm", () => _plc.Write(TransferRobotAddress.UseUpperArm, !UseUpperArmToggle), () => "TransferRobot");
        UseLowerArmCommand = OperationAudit.Create("UseLowerArm", () => _plc.Write(TransferRobotAddress.UseLowerArm, !UseLowerArmToggle), () => "TransferRobot");
        ConfirmSwitchCommand = OperationAudit.Create("ConfirmSwitch", () => _plc.Write(TransferRobotAddress.ConfirmSwitch, !ConfirmSwitchToggle), () => "TransferRobot");
        TurnTo0Command = OperationAudit.Create("TurnTo0", () => _plc.Write(TransferRobotAddress.TurnTo0, !TurnTo0Toggle), () => "TransferRobot");
        TurnTo180Command = OperationAudit.Create("TurnTo180", () => _plc.Write(TransferRobotAddress.TurnTo180, !TurnTo180Toggle), () => "TransferRobot");
        RobotSafePositionCommand = OperationAudit.Create("RobotSafePosition", () => { /* No direct write defined for safe pos button in address file or just read? Assuming standard toggle write pattern if address existed, but address definition only shows status. Checking logic... Address file defines RobotSafePosition as DBitAddress. If it's a command, it's missing in const string. Assuming it's just an indicator for now or use a placeholder if write is needed. Based on UI, it's a toggle button. But address list only has RobotSafePosition as DBitAddress (read). I will comment out write provided there's no address. */ }, () => "TransferRobot");

        // Bottom Bar
        EmergencyStopCommand = OperationAudit.Create("EmergencyStop", () => _plc.Write(TransferRobotAddress.EmergencyStop, !EmergencyStopToggle), () => "TransferRobot");
        AlarmResetCommand = OperationAudit.CreateAsync("AlarmReset", () => _plc.PulseAsync(TransferRobotAddress.AxisAlarmReset), () => "TransferRobot");
        SaveCommand = OperationAudit.Create("Save", () => _plc.Write(TransferRobotAddress.Save, !SaveToggle), () => "TransferRobot");

        // Station Operation
        StationResetCommand = OperationAudit.CreateAsync("StationReset", () => _plc.PulseAsync(TransferRobotAddress.StationReset), () => "TransferRobot");
        StationStartCommand = OperationAudit.CreateAsync("StationStart", () => _plc.PulseAsync(TransferRobotAddress.StationStart), () => "TransferRobot");
        StationStopCommand = OperationAudit.CreateAsync("StationStop", () => _plc.PulseAsync(TransferRobotAddress.StationStop), () => "TransferRobot");
        StationAlarmResetCommand = OperationAudit.CreateAsync("StationAlarmReset", () => _plc.PulseAsync(TransferRobotAddress.GlobalAlarmReset), () => "TransferRobot"); // Using GlobalAlarmReset (B0B)

        StationModulePickCommand = OperationAudit.CreateAsync("StationModulePick", () => _plc.PulseAsync(TransferRobotAddress.StationModulePick), () => "TransferRobot");
        StationTransferAPlaceCommand = OperationAudit.CreateAsync("StationTransferAPlace", () => _plc.PulseAsync(TransferRobotAddress.StationTransferAPlace), () => "TransferRobot");
        StationTransferAPickCommand = OperationAudit.CreateAsync("StationTransferAPick", () => _plc.PulseAsync(TransferRobotAddress.StationTransferAPick), () => "TransferRobot");
        StationTransferBPlaceCommand = OperationAudit.CreateAsync("StationTransferBPlace", () => _plc.PulseAsync(TransferRobotAddress.StationTransferBPlace), () => "TransferRobot");
        StationTransferBPickCommand = OperationAudit.CreateAsync("StationTransferBPick", () => _plc.PulseAsync(TransferRobotAddress.StationTransferBPick), () => "TransferRobot");
        StationRollerA1PlaceCommand = OperationAudit.CreateAsync("StationRollerA1Place", () => _plc.PulseAsync(TransferRobotAddress.StationRollerA1Place), () => "TransferRobot");
        StationRollerA2PlaceCommand = OperationAudit.CreateAsync("StationRollerA2Place", () => _plc.PulseAsync(TransferRobotAddress.StationRollerA2Place), () => "TransferRobot");
        StationRollerB1PlaceCommand = OperationAudit.CreateAsync("StationRollerB1Place", () => _plc.PulseAsync(TransferRobotAddress.StationRollerB1Place), () => "TransferRobot");
        StationRollerB2PlaceCommand = OperationAudit.CreateAsync("StationRollerB2Place", () => _plc.PulseAsync(TransferRobotAddress.StationRollerB2Place), () => "TransferRobot");

        ManualModeStationCommand = OperationAudit.CreateAsync("ManualModeStation", () => _plc.PulseAsync(TransferRobotAddress.ManualMode), () => "TransferRobot");
        AutoModeStationCommand = OperationAudit.CreateAsync("AutoModeStation", () => _plc.PulseAsync(TransferRobotAddress.AutoModeStation), () => "TransferRobot");
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
        var mBatchGroups = new List<List<PlcBatchReadItemBase>>
        {
            new()
            {
                new PlcBatchReadItem<bool>(TransferRobotAddress.EnableStatus, assign: v => IsEnabled = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.JogUpStatus, assign: v => JogUpActive = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.JogMiddleStatus, assign: v => JogMiddleActive = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.JogDownStatus, assign: v => JogDownActive = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.OriginStatus, assign: v => IsAtOrigin = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.Origin, assign: v => OriginToggle = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.DriverAlarm, assign: v => DriverAlarm = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.LimitAlarm, assign: v => LimitAlarm = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.EmergencyStop, assign: v => EmergencyStopToggle = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.Save, assign: v => SaveToggle = v)
            },
            new()
            {
                new PlcBatchReadItem<bool>(TransferRobotAddress.RobotStartStatus, assign: v => RobotStartStatus = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.RobotPause, assign: v => RobotPauseToggle = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.RobotPauseStatus, assign: v => RobotPauseStatus = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.RobotStopStatus, assign: v => RobotStopStatus = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.RobotResetStatus, assign: v => RobotResetStatus = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.WorkAreaPick, assign: v => WorkAreaPickToggle = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.WorkAreaPickStatus, assign: v => WorkAreaPickStatus = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.WorkAreaPlace, assign: v => WorkAreaPlaceToggle = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.WorkAreaPlaceStatus, assign: v => WorkAreaPlaceStatus = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.WaitConfirm, assign: v => WaitConfirmToggle = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.WaitConfirmStatus, assign: v => WaitConfirmStatus = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.AutoMode, assign: v => AutoModeToggle = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.UseUpperArm, assign: v => UseUpperArmToggle = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.UseLowerArm, assign: v => UseLowerArmToggle = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.ConfirmSwitch, assign: v => ConfirmSwitchToggle = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.TurnTo0, assign: v => TurnTo0Toggle = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.TurnTo0Status, assign: v => TurnTo0Status = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.TurnTo180, assign: v => TurnTo180Toggle = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.TurnTo180Status, assign: v => TurnTo180Status = v)
            },
            new()
            {
                new PlcBatchReadItem<bool>(TransferRobotAddress.StationResetStatus, assign: v => StationIsReset = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.StationStartStatus, assign: v => StationIsRunning = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.StationStopStatus, assign: v => StationIsStopped = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.ModuleAlarm, assign: v => ModuleAlarmOn = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.GlobalAlarm, assign: v => GlobalAlarmOn = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.ManualMode, assign: v => ManualModeStationToggle = v),
                new PlcBatchReadItem<bool>(TransferRobotAddress.AutoModeStation, assign: v => AutoModeStationToggle = v)
            }
        };

        var dStatusBatchItems = new List<PlcBatchReadItemBase>
        {
            new PlcBatchReadItem<int>(TransferRobotAddress.CurrentCoordinate, assign: v => CurrentCoordinate = v / 1000.0),
            new PlcBatchReadItem<int>(TransferRobotAddress.AlarmCode, assign: v => AlarmCode = v / 1000)
        };

        var dBitBatchItems = new List<PlcBatchReadItemBase>
        {
            PlcBatchReadItem<bool>.Bind(TransferRobotAddress.AutoModeStatus.Address, () => AutoModeStatus,
                converter: MitsubishiPlcService.ToBitConverter(TransferRobotAddress.AutoModeStatus)),
            PlcBatchReadItem<bool>.Bind(TransferRobotAddress.UpperArmSensor.Address, () => UpperArmSensorOn,
                converter: MitsubishiPlcService.ToBitConverter(TransferRobotAddress.UpperArmSensor)),
            PlcBatchReadItem<bool>.Bind(TransferRobotAddress.LowerArmSensor.Address, () => LowerArmSensorOn,
                converter: MitsubishiPlcService.ToBitConverter(TransferRobotAddress.LowerArmSensor)),
            PlcBatchReadItem<bool>.Bind(TransferRobotAddress.CommandReceived.Address, () => CommandReceivedOn,
                converter: MitsubishiPlcService.ToBitConverter(TransferRobotAddress.CommandReceived)),
            PlcBatchReadItem<bool>.Bind(TransferRobotAddress.RobotSafePosition.Address, () => RobotSafePositionOn,
                converter: MitsubishiPlcService.ToBitConverter(TransferRobotAddress.RobotSafePosition))
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

        var t_dStatus = _plc.BatchReadAsync(dStatusBatchItems);
        var t_dBits = _plc.BatchReadAsync(dBitBatchItems);
        var t_editable = _plc.BatchReadAsync(editableBatchItems);
        var t_b0B = _plc.ReadAsync<bool>(TransferRobotAddress.AxisAlarmReset);

        var editableIntBatchItems = _editableIntItems
            .Select(item => (PlcBatchReadItemBase)new PlcBatchReadItem<short>(item.Address, assign: v =>
            {
                item.PlcValue = v;
                item.HasPlcValue = true;

                if (item.IsDirty)
                {
                    return;
                }

                item.Value = v;
                RaisePropertyChanged(item.ValuePropertyName);
            }))
            .ToList();
        var t_editableInt = _plc.BatchReadAsync(editableIntBatchItems);

        var t_moveToggle = _plc.ReadAsync<bool[]>(TransferRobotAddress.ModulePickMove, 7); // M1080-M1086
        var t_moveStatus = _plc.ReadAsync<bool[]>(TransferRobotAddress.ModulePickMoveStatus, 7); // M5128-M5134
        var t_stationToggle = _plc.ReadAsync<bool[]>(TransferRobotAddress.StationModulePick, 10); // M1944-M1953
        var t_stationActive = _plc.ReadAsync<bool[]>(TransferRobotAddress.StationModulePickActive, 10); // M6264-M6273
        var t_stationDone = _plc.ReadAsync<bool[]>(TransferRobotAddress.StationModulePickDone, 10); // M6280-M6289

        var mBatchTasks = mBatchGroups.Select(group => _plc.BatchReadAsync(group)).ToList();
        var allTasks = new List<Task>
        {
            t_dStatus,
            t_dBits,
            t_editable,
            t_editableInt,
            t_b0B,
            t_moveToggle,
            t_moveStatus,
            t_stationToggle,
            t_stationActive,
            t_stationDone
        };
        allTasks.AddRange(mBatchTasks);

        await Task.WhenAll(allTasks);

        for (var i = 0; i < mBatchTasks.Count; i++)
        {
            if (!mBatchTasks[i].Result.IsSuccess)
            {
                Logger.For("RobotControl").Warn($"[TransferRobot] M batch read failed (group {i + 1}): {mBatchTasks[i].Result.Message}");
            }
        }

        if (!t_dStatus.Result.IsSuccess)
            Logger.For("RobotControl").Warn($"[TransferRobot] D status batch read failed: {t_dStatus.Result.Message}");

        if (!t_dBits.Result.IsSuccess)
            Logger.For("RobotControl").Warn($"[TransferRobot] D bit batch read failed: {t_dBits.Result.Message}");

        if (!t_editable.Result.IsSuccess)
            Logger.For("RobotControl").Warn($"[TransferRobot] D editable batch read failed: {t_editable.Result.Message}");

        if (!t_editableInt.Result.IsSuccess)
            Logger.For("RobotControl").Warn($"[TransferRobot] W editable batch read failed: {t_editableInt.Result.Message}");

        if (t_b0B.Result.IsSuccess)
            AlarmResetToggle = t_b0B.Result.Content;

        if (t_moveToggle.Result.IsSuccess)
        {
            var toggles = t_moveToggle.Result.Content;
            if (toggles.Length >= 7)
            {
                ModulePickMoveToggle = toggles[0];
                TransferAMoveToggle = toggles[1];
                TransferBMoveToggle = toggles[2];
                RollerA1MoveToggle = toggles[3];
                RollerA2MoveToggle = toggles[4];
                RollerB1MoveToggle = toggles[5];
                RollerB2MoveToggle = toggles[6];
            }
        }

        if (t_moveStatus.Result.IsSuccess)
        {
            var statuses = t_moveStatus.Result.Content;
            if (statuses.Length >= 7)
            {
                ModulePickMoveStatus = statuses[0];
                TransferAMoveStatus = statuses[1];
                TransferBMoveStatus = statuses[2];
                RollerA1MoveStatus = statuses[3];
                RollerA2MoveStatus = statuses[4];
                RollerB1MoveStatus = statuses[5];
                RollerB2MoveStatus = statuses[6];
            }
        }

        if (t_stationToggle.Result.IsSuccess)
        {
            var toggles = t_stationToggle.Result.Content;
            if (toggles.Length >= 10)
            {
                StationModulePickToggle = toggles[0];
                StationRollerA1PlaceToggle = toggles[1];
                StationTransferAPlaceToggle = toggles[3];
                StationTransferAPickToggle = toggles[4];
                StationTransferBPlaceToggle = toggles[5];
                StationTransferBPickToggle = toggles[6];
                StationRollerA2PlaceToggle = toggles[7];
                StationRollerB1PlaceToggle = toggles[8];
                StationRollerB2PlaceToggle = toggles[9];
            }
        }

        if (t_stationActive.Result.IsSuccess)
        {
            var active = t_stationActive.Result.Content;
            if (active.Length >= 10)
            {
                StationModulePickActive = active[0];
                StationRollerA1PlaceActive = active[1];
                StationTransferAPlaceActive = active[3];
                StationTransferAPickActive = active[4];
                StationTransferBPlaceActive = active[5];
                StationTransferBPickActive = active[6];
                StationRollerA2PlaceActive = active[7];
                StationRollerB1PlaceActive = active[8];
                StationRollerB2PlaceActive = active[9];
            }
        }

        if (t_stationDone.Result.IsSuccess)
        {
            var done = t_stationDone.Result.Content;
            if (done.Length >= 10)
            {
                StationModulePickDone = done[0];
                StationRollerA1PlaceDone = done[1];
                StationTransferAPlaceDone = done[3];
                StationTransferAPickDone = done[4];
                StationTransferBPlaceDone = done[5];
                StationTransferBPickDone = done[6];
                StationRollerA2PlaceDone = done[7];
                StationRollerB1PlaceDone = done[8];
                StationRollerB2PlaceDone = done[9];
            }
        }
    }

    public void StopReading()
    {
        OnViewDeactivated();
    }
}

