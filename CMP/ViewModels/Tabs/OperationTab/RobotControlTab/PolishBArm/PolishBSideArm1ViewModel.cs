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
using CMP.Services.Auth;

namespace CMP.ViewModels.Tabs.OperationTab.RobotControlTab.PolishBArm
{
    public class PolishBSideArm1ViewModel : StationOpViewModelBase
    {
        private bool _isReading;
        private readonly EditableFloatItem _standbyPosition1xItem = new(PolishBSideArm1Address.StandbyPosition_1x, nameof(StandbyPosition_1x), nameof(IsStandbyPosition_1xDirty));
        private readonly EditableFloatItem _transferArmPosition1xItem = new(PolishBSideArm1Address.TransferArmPosition_1x, nameof(TransferArmPosition_1x), nameof(IsTransferArmPosition_1xDirty));
        private readonly EditableFloatItem _polishCPosition1xItem = new(PolishBSideArm1Address.PolishCPosition_1x, nameof(PolishCPosition_1x), nameof(IsPolishCPosition_1xDirty));
        private readonly EditableFloatItem _polishDPosition1xItem = new(PolishBSideArm1Address.PolishDPosition_1x, nameof(PolishDPosition_1x), nameof(IsPolishDPosition_1xDirty));
        private readonly EditableFloatItem _jogSpeed1xItem = new(PolishBSideArm1Address.JogSpeed_1x, nameof(JogSpeed_1x), nameof(IsJogSpeed_1xDirty));
        private readonly EditableFloatItem _autoSpeed1xItem = new(PolishBSideArm1Address.AutoSpeed_1x, nameof(AutoSpeed_1x), nameof(IsAutoSpeed_1xDirty));
        private readonly EditableFloatItem _accelerationTime1xItem = new(PolishBSideArm1Address.AccelerationTime_1x, nameof(AccelerationTime_1x), nameof(IsAccelerationTime_1xDirty));
        private readonly EditableFloatItem _deaccelerationTime1xItem = new(PolishBSideArm1Address.DeaccelerationTime_1x, nameof(DeaccelerationTime_1x), nameof(IsDeaccelerationTime_1xDirty));

        private readonly EditableFloatItem _standbyPosition1zItem = new(PolishBSideArm1Address.StandbyPosition_1z, nameof(StandbyPosition_1z), nameof(IsStandbyPosition_1zDirty));
        private readonly EditableFloatItem _transferArmPosition1zItem = new(PolishBSideArm1Address.TransferArmPosition_1z, nameof(TransferArmPosition_1z), nameof(IsTransferArmPosition_1zDirty));
        private readonly EditableFloatItem _polishCPosition1zItem = new(PolishBSideArm1Address.PolishCPosition_1z, nameof(PolishCPosition_1z), nameof(IsPolishCPosition_1zDirty));
        private readonly EditableFloatItem _polishDPosition1zItem = new(PolishBSideArm1Address.PolishDPosition_1z, nameof(PolishDPosition_1z), nameof(IsPolishDPosition_1zDirty));
        private readonly EditableFloatItem _jogSpeed1zItem = new(PolishBSideArm1Address.JogSpeed_1z, nameof(JogSpeed_1z), nameof(IsJogSpeed_1zDirty));
        private readonly EditableFloatItem _autoSpeed1zItem = new(PolishBSideArm1Address.AutoSpeed_1z, nameof(AutoSpeed_1z), nameof(IsAutoSpeed_1zDirty));
        private readonly EditableFloatItem _speedWhenHasWaferItem = new(PolishBSideArm1Address.SpeedWhenHasWafer, nameof(SpeedWhenHasWafer), nameof(IsSpeedWhenHasWaferDirty));
        private readonly EditableFloatItem _accelerationTime1zItem = new(PolishBSideArm1Address.AccelerationTime1z, nameof(AccelerationTime_1z), nameof(IsAccelerationTime_1zDirty));
        private readonly EditableFloatItem _deaccelerationTime1zItem = new(PolishBSideArm1Address.DeaccelerationTime1z, nameof(DeaccelerationTime_1z), nameof(IsDeaccelerationTime_1zDirty));

        private readonly List<EditableFloatItem> _editableItems;

        public double PositionMinimum { get; set; } = -90000;
        public double PositionMaximum { get; set; } = 90000;

        #region AxisPositionOfSideBArm-1x

        public bool IsStandbyPosition_1xDirty => _standbyPosition1xItem.IsDirty;
        public double StandbyPosition_1x
        {
            get => _standbyPosition1xItem.Value;
            set => EditableFloatItemHelper.SetValue(_standbyPosition1xItem, value, RaisePropertyChanged);
        }

        public bool IsTransferArmPosition_1xDirty => _transferArmPosition1xItem.IsDirty;
        public double TransferArmPosition_1x
        {
            get => _transferArmPosition1xItem.Value;
            set => EditableFloatItemHelper.SetValue(_transferArmPosition1xItem, value, RaisePropertyChanged);
        }

        public bool IsPolishCPosition_1xDirty => _polishCPosition1xItem.IsDirty;
        public double PolishCPosition_1x
        {
            get => _polishCPosition1xItem.Value;
            set => EditableFloatItemHelper.SetValue(_polishCPosition1xItem, value, RaisePropertyChanged);
        }

        public bool IsPolishDPosition_1xDirty => _polishDPosition1xItem.IsDirty;
        public double PolishDPosition_1x
        {
            get => _polishDPosition1xItem.Value;
            set => EditableFloatItemHelper.SetValue(_polishDPosition1xItem, value, RaisePropertyChanged);
        }

        private bool _standbyPosMoveToggle_1x;
        public bool StandbyPosMoveToggle_1x
        {
            get => _standbyPosMoveToggle_1x;
            set => SetProperty(ref _standbyPosMoveToggle_1x, value);
        }

        private bool _transferArmMoveToggle_1x;
        public bool TransferArmMoveToggle_1x
        {
            get => _transferArmMoveToggle_1x;
            set => SetProperty(ref _transferArmMoveToggle_1x, value);
        }

        private bool _polishCPosMoveToggle_1x;
        public bool PolishCPosMoveToggle_1x
        {
            get => _polishCPosMoveToggle_1x;
            set => SetProperty(ref _polishCPosMoveToggle_1x, value);
        }

        private bool _polishDPosMoveToggle_1x;
        public bool PolishDPosMoveToggle_1x
        {
            get => _polishDPosMoveToggle_1x;
            set => SetProperty(ref _polishDPosMoveToggle_1x, value);
        }

        public bool IsJogSpeed_1xDirty => _jogSpeed1xItem.IsDirty;
        public double JogSpeed_1x
        {
            get => _jogSpeed1xItem.Value;
            set => EditableFloatItemHelper.SetValue(_jogSpeed1xItem, value, RaisePropertyChanged);
        }

        public bool IsAutoSpeed_1xDirty => _autoSpeed1xItem.IsDirty;
        public double AutoSpeed_1x
        {
            get => _autoSpeed1xItem.Value;
            set => EditableFloatItemHelper.SetValue(_autoSpeed1xItem, value, RaisePropertyChanged);
        }

        public bool IsAccelerationTime_1xDirty => _accelerationTime1xItem.IsDirty;
        public double AccelerationTime_1x
        {
            get => _accelerationTime1xItem.Value;
            set => EditableFloatItemHelper.SetValue(_accelerationTime1xItem, value, RaisePropertyChanged);
        }

        public bool IsDeaccelerationTime_1xDirty => _deaccelerationTime1xItem.IsDirty;
        public double DeaccelerationTime_1x
        {
            get => _deaccelerationTime1xItem.Value;
            set => EditableFloatItemHelper.SetValue(_deaccelerationTime1xItem, value, RaisePropertyChanged);
        }

        private double _currentCoordinate_1x;
        public double CurrentCoordinate_1x
        {
            get => _currentCoordinate_1x;
            set => SetProperty(ref _currentCoordinate_1x, value);
        }

        private bool _isEnabled_1x;
        public bool IsEnabled_1x
        {
            get => _isEnabled_1x;
            set => SetProperty(ref _isEnabled_1x, value);
        }

        private bool _driverAlarm_1x;
        public bool DriverAlarm_1x
        {
            get => _driverAlarm_1x;
            set => SetProperty(ref _driverAlarm_1x, value);
        }

        private bool _limitAlarm_1x;
        public bool LimitAlarm_1x
        {
            get => _limitAlarm_1x;
            set => SetProperty(ref _limitAlarm_1x, value);
        }

        private bool _isAtOrigin_1x;
        public bool IsAtOrigin_1x
        {
            get => _isAtOrigin_1x;
            set => SetProperty(ref _isAtOrigin_1x, value);
        }

        private int _alarmCode_1x;
        public int AlarmCode_1x
        {
            get => _alarmCode_1x;
            set => SetProperty(ref _alarmCode_1x, value);
        }

        private bool _jogUpActive_1x;
        public bool JogUpActive_1x
        {
            get => _jogUpActive_1x;
            set => SetProperty(ref _jogUpActive_1x, value);
        }

        private bool _jogMiddleActive_1x;
        public bool JogMiddleActive_1x
        {
            get => _jogMiddleActive_1x;
            set => SetProperty(ref _jogMiddleActive_1x, value);
        }

        private bool _jogDownActive_1x;
        public bool JogDownActive_1x
        {
            get => _jogDownActive_1x;
            set => SetProperty(ref _jogDownActive_1x, value);
        }

        private bool _originToggle_1x;
        public bool OriginToggle_1x
        {
            get => _originToggle_1x;
            set => SetProperty(ref _originToggle_1x, value);
        }

        #endregion

        #region AxisPositionOfSideBArm-1z

        public bool IsStandbyPosition_1zDirty => _standbyPosition1zItem.IsDirty;
        public double StandbyPosition_1z
        {
            get => _standbyPosition1zItem.Value;
            set => EditableFloatItemHelper.SetValue(_standbyPosition1zItem, value, RaisePropertyChanged);
        }

        public bool IsTransferArmPosition_1zDirty => _transferArmPosition1zItem.IsDirty;
        public double TransferArmPosition_1z
        {
            get => _transferArmPosition1zItem.Value;
            set => EditableFloatItemHelper.SetValue(_transferArmPosition1zItem, value, RaisePropertyChanged);
        }

        public bool IsPolishCPosition_1zDirty => _polishCPosition1zItem.IsDirty;
        public double PolishCPosition_1z
        {
            get => _polishCPosition1zItem.Value;
            set => EditableFloatItemHelper.SetValue(_polishCPosition1zItem, value, RaisePropertyChanged);
        }

        public bool IsPolishDPosition_1zDirty => _polishDPosition1zItem.IsDirty;
        public double PolishDPosition_1z
        {
            get => _polishDPosition1zItem.Value;
            set => EditableFloatItemHelper.SetValue(_polishDPosition1zItem, value, RaisePropertyChanged);
        }

        private bool _standbyPosMoveToggle_1z;
        public bool StandbyPosMoveToggle_1z
        {
            get => _standbyPosMoveToggle_1z;
            set => SetProperty(ref _standbyPosMoveToggle_1z, value);
        }

        private bool _transferArmMoveToggle_1z;
        public bool TransferArmMoveToggle_1z
        {
            get => _transferArmMoveToggle_1z;
            set => SetProperty(ref _transferArmMoveToggle_1z, value);
        }

        private bool _polishCPosMoveToggle_1z;
        public bool PolishCPosMoveToggle_1z
        {
            get => _polishCPosMoveToggle_1z;
            set => SetProperty(ref _polishCPosMoveToggle_1z, value);
        }

        private bool _polishDPosMoveToggle_1z;
        public bool PolishDPosMoveToggle_1z
        {
            get => _polishDPosMoveToggle_1z;
            set => SetProperty(ref _polishDPosMoveToggle_1z, value);
        }

        public bool IsJogSpeed_1zDirty => _jogSpeed1zItem.IsDirty;
        public double JogSpeed_1z
        {
            get => _jogSpeed1zItem.Value;
            set => EditableFloatItemHelper.SetValue(_jogSpeed1zItem, value, RaisePropertyChanged);
        }

        public bool IsAutoSpeed_1zDirty => _autoSpeed1zItem.IsDirty;
        public double AutoSpeed_1z
        {
            get => _autoSpeed1zItem.Value;
            set => EditableFloatItemHelper.SetValue(_autoSpeed1zItem, value, RaisePropertyChanged);
        }

        public bool IsSpeedWhenHasWaferDirty => _speedWhenHasWaferItem.IsDirty;
        public double SpeedWhenHasWafer
        {
            get => _speedWhenHasWaferItem.Value;
            set => EditableFloatItemHelper.SetValue(_speedWhenHasWaferItem, value, RaisePropertyChanged);
        }

        public bool IsAccelerationTime_1zDirty => _accelerationTime1zItem.IsDirty;
        public double AccelerationTime_1z
        {
            get => _accelerationTime1zItem.Value;
            set => EditableFloatItemHelper.SetValue(_accelerationTime1zItem, value, RaisePropertyChanged);
        }

        public bool IsDeaccelerationTime_1zDirty => _deaccelerationTime1zItem.IsDirty;
        public double DeaccelerationTime_1z
        {
            get => _deaccelerationTime1zItem.Value;
            set => EditableFloatItemHelper.SetValue(_deaccelerationTime1zItem, value, RaisePropertyChanged);
        }

        private double _currentCoordinate_1z;
        public double CurrentCoordinate_1z
        {
            get => _currentCoordinate_1z;
            set => SetProperty(ref _currentCoordinate_1z, value);
        }

        private bool _isEnabled_1z;
        public bool IsEnabled_1z
        {
            get => _isEnabled_1z;
            set => SetProperty(ref _isEnabled_1z, value);
        }

        private bool _driverAlarm_1z;
        public bool DriverAlarm_1z
        {
            get => _driverAlarm_1z;
            set => SetProperty(ref _driverAlarm_1z, value);
        }

        private bool _limitAlarm_1z;
        public bool LimitAlarm_1z
        {
            get => _limitAlarm_1z;
            set => SetProperty(ref _limitAlarm_1z, value);
        }

        private bool _isAtOrigin_1z;
        public bool IsAtOrigin_1z
        {
            get => _isAtOrigin_1z;
            set => SetProperty(ref _isAtOrigin_1z, value);
        }

        private int _alarmCode_1z;
        public int AlarmCode_1z
        {
            get => _alarmCode_1z;
            set => SetProperty(ref _alarmCode_1z, value);
        }

        private bool _jogUpActive_1z;
        public bool JogUpActive_1z
        {
            get => _jogUpActive_1z;
            set => SetProperty(ref _jogUpActive_1z, value);
        }

        private bool _jogMiddleActive_1z;
        public bool JogMiddleActive_1z
        {
            get => _jogMiddleActive_1z;
            set => SetProperty(ref _jogMiddleActive_1z, value);
        }

        private bool _jogDownActive_1z;
        public bool JogDownActive_1z
        {
            get => _jogDownActive_1z;
            set => SetProperty(ref _jogDownActive_1z, value);
        }

        private bool _originToggle_1z;
        public bool OriginToggle_1z
        {
            get => _originToggle_1z;
            set => SetProperty(ref _originToggle_1z, value);
        }

        #endregion

        #region Station Operation View Properties

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

        private bool _pickFromTransferActive;
        public bool PickFromTransferActive { get => _pickFromTransferActive; set => SetProperty(ref _pickFromTransferActive, value); }
        private bool _pickFromTransferToggle;
        public bool PickFromTransferToggle { get => _pickFromTransferToggle; set => SetProperty(ref _pickFromTransferToggle, value); }
        private bool _pickFromTransferDone;
        public bool PickFromTransferDone { get => _pickFromTransferDone; set => SetProperty(ref _pickFromTransferDone, value); }

        private bool _placeToPolishCActive;
        public bool PlaceToPolishCActive { get => _placeToPolishCActive; set => SetProperty(ref _placeToPolishCActive, value); }
        private bool _placeToPolishCToggle;
        public bool PlaceToPolishCToggle { get => _placeToPolishCToggle; set => SetProperty(ref _placeToPolishCToggle, value); }
        private bool _placeToPolishCDone;
        public bool PlaceToPolishCDone { get => _placeToPolishCDone; set => SetProperty(ref _placeToPolishCDone, value); }

        private bool _placeToPolishDActive;
        public bool PlaceToPolishDActive { get => _placeToPolishDActive; set => SetProperty(ref _placeToPolishDActive, value); }
        private bool _placeToPolishDToggle;
        public bool PlaceToPolishDToggle { get => _placeToPolishDToggle; set => SetProperty(ref _placeToPolishDToggle, value); }
        private bool _placeToPolishDDone;
        public bool PlaceToPolishDDone { get => _placeToPolishDDone; set => SetProperty(ref _placeToPolishDDone, value); }

        private bool _spare1Active;
        public bool Spare1Active { get => _spare1Active; set => SetProperty(ref _spare1Active, value); }
        private bool _spare1Toggle;
        public bool Spare1Toggle { get => _spare1Toggle; set => SetProperty(ref _spare1Toggle, value); }
        private bool _spare1Done;
        public bool Spare1Done { get => _spare1Done; set => SetProperty(ref _spare1Done, value); }

        private bool _spare2Active;
        public bool Spare2Active { get => _spare2Active; set => SetProperty(ref _spare2Active, value); }
        private bool _spare2Toggle;
        public bool Spare2Toggle { get => _spare2Toggle; set => SetProperty(ref _spare2Toggle, value); }
        private bool _spare2Done;
        public bool Spare2Done { get => _spare2Done; set => SetProperty(ref _spare2Done, value); }

        private bool _manualModeToggle;
        public bool ManualModeToggle { get => _manualModeToggle; set => SetProperty(ref _manualModeToggle, value); }

        private bool _autoModeToggle;
        public bool AutoModeToggle { get => _autoModeToggle; set => SetProperty(ref _autoModeToggle, value); }

        private bool _arm1Fiber1;
        public bool Arm1Fiber1 { get => _arm1Fiber1; set => SetProperty(ref _arm1Fiber1, value); }

        private bool _arm1Fiber2;
        public bool Arm1Fiber2 { get => _arm1Fiber2; set => SetProperty(ref _arm1Fiber2, value); }

        private bool _arm1Fiber3;
        public bool Arm1Fiber3 { get => _arm1Fiber3; set => SetProperty(ref _arm1Fiber3, value); }

        #endregion

        #region Commands

        public DelegateCommand StationOperationCommand { get; }

        public DelegateCommand StandbyMoveX1Command { get; }
        public DelegateCommand TransferMoveX1Command { get; }
        public DelegateCommand PolishCMoveX1Command { get; }
        public DelegateCommand PolishDMoveX1Command { get; }
        public DelegateCommand StandbyMoveZ1Command { get; }
        public DelegateCommand TransferMoveZ1Command { get; }
        public DelegateCommand PolishCMoveZ1Command { get; }
        public DelegateCommand PolishDMoveZ1Command { get; }

        public DelegateCommand EnableCommandX1Command { get; }
        public DelegateCommand EnableCommandZ1Command { get; }

        public DelegateCommand PressJogUpX1Command { get; }
        public DelegateCommand ReleaseJogUpX1Command { get; }
        public DelegateCommand PressJogDownX1Command { get; }
        public DelegateCommand ReleaseJogDownX1Command { get; }
        public DelegateCommand PressJogUpZ1Command { get; }
        public DelegateCommand ReleaseJogUpZ1Command { get; }
        public DelegateCommand PressJogDownZ1Command { get; }
        public DelegateCommand ReleaseJogDownZ1Command { get; }

        public DelegateCommand OrignX1Command { get; }
        public DelegateCommand OrignZ1Command { get; }

        public DelegateCommand EmergencyCommand { get; }
        public DelegateCommand AlarmResetCommand { get; }
        public DelegateCommand SaveCommand { get; }

        public DelegateCommand BackCommand { get; }
        public DelegateCommand StationResetCommand { get; }
        public DelegateCommand StationStartCommand { get; }
        public DelegateCommand StationStopCommand { get; }
        public DelegateCommand StationAlarmResetCommand { get; }

        public DelegateCommand PickFromTransferCommand { get; }
        public DelegateCommand PlaceToPolishCCommand { get; }
        public DelegateCommand PlaceToPolishDCommand { get; }
        public DelegateCommand Spare1Command { get; }
        public DelegateCommand Spare2Command { get; }

        public DelegateCommand ManualModeCommand { get; }
        public DelegateCommand AutoModeCommand { get; }

        public DelegateCommand StandbyPosition_1xSetCommand => _standbyPosition1xItem.SetCommand;
        public DelegateCommand StandbyPosition_1xCancelCommand => _standbyPosition1xItem.CancelCommand;
        public DelegateCommand TransferArmPosition_1xSetCommand => _transferArmPosition1xItem.SetCommand;
        public DelegateCommand TransferArmPosition_1xCancelCommand => _transferArmPosition1xItem.CancelCommand;
        public DelegateCommand PolishCPosition_1xSetCommand => _polishCPosition1xItem.SetCommand;
        public DelegateCommand PolishCPosition_1xCancelCommand => _polishCPosition1xItem.CancelCommand;
        public DelegateCommand PolishDPosition_1xSetCommand => _polishDPosition1xItem.SetCommand;
        public DelegateCommand PolishDPosition_1xCancelCommand => _polishDPosition1xItem.CancelCommand;
        public DelegateCommand JogSpeed_1xSetCommand => _jogSpeed1xItem.SetCommand;
        public DelegateCommand JogSpeed_1xCancelCommand => _jogSpeed1xItem.CancelCommand;
        public DelegateCommand AutoSpeed_1xSetCommand => _autoSpeed1xItem.SetCommand;
        public DelegateCommand AutoSpeed_1xCancelCommand => _autoSpeed1xItem.CancelCommand;
        public DelegateCommand AccelerationTime_1xSetCommand => _accelerationTime1xItem.SetCommand;
        public DelegateCommand AccelerationTime_1xCancelCommand => _accelerationTime1xItem.CancelCommand;
        public DelegateCommand DeaccelerationTime_1xSetCommand => _deaccelerationTime1xItem.SetCommand;
        public DelegateCommand DeaccelerationTime_1xCancelCommand => _deaccelerationTime1xItem.CancelCommand;

        public DelegateCommand StandbyPosition_1zSetCommand => _standbyPosition1zItem.SetCommand;
        public DelegateCommand StandbyPosition_1zCancelCommand => _standbyPosition1zItem.CancelCommand;
        public DelegateCommand TransferArmPosition_1zSetCommand => _transferArmPosition1zItem.SetCommand;
        public DelegateCommand TransferArmPosition_1zCancelCommand => _transferArmPosition1zItem.CancelCommand;
        public DelegateCommand PolishCPosition_1zSetCommand => _polishCPosition1zItem.SetCommand;
        public DelegateCommand PolishCPosition_1zCancelCommand => _polishCPosition1zItem.CancelCommand;
        public DelegateCommand PolishDPosition_1zSetCommand => _polishDPosition1zItem.SetCommand;
        public DelegateCommand PolishDPosition_1zCancelCommand => _polishDPosition1zItem.CancelCommand;
        public DelegateCommand JogSpeed_1zSetCommand => _jogSpeed1zItem.SetCommand;
        public DelegateCommand JogSpeed_1zCancelCommand => _jogSpeed1zItem.CancelCommand;
        public DelegateCommand AutoSpeed_1zSetCommand => _autoSpeed1zItem.SetCommand;
        public DelegateCommand AutoSpeed_1zCancelCommand => _autoSpeed1zItem.CancelCommand;
        public DelegateCommand SpeedWhenHasWaferSetCommand => _speedWhenHasWaferItem.SetCommand;
        public DelegateCommand SpeedWhenHasWaferCancelCommand => _speedWhenHasWaferItem.CancelCommand;
        public DelegateCommand AccelerationTime_1zSetCommand => _accelerationTime1zItem.SetCommand;
        public DelegateCommand AccelerationTime_1zCancelCommand => _accelerationTime1zItem.CancelCommand;
        public DelegateCommand DeaccelerationTime_1zSetCommand => _deaccelerationTime1zItem.SetCommand;
        public DelegateCommand DeaccelerationTime_1zCancelCommand => _deaccelerationTime1zItem.CancelCommand;

        #endregion

        public PolishBSideArm1ViewModel() : base(8, "Polish B Side Arm1")
        {
            _editableItems = new List<EditableFloatItem>
            {
                _standbyPosition1xItem,
                _transferArmPosition1xItem,
                _polishCPosition1xItem,
                _polishDPosition1xItem,
                _jogSpeed1xItem,
                _autoSpeed1xItem,
                _accelerationTime1xItem,
                _deaccelerationTime1xItem,
                _standbyPosition1zItem,
                _transferArmPosition1zItem,
                _polishCPosition1zItem,
                _polishDPosition1zItem,
                _jogSpeed1zItem,
                _autoSpeed1zItem,
                _speedWhenHasWaferItem,
                _accelerationTime1zItem,
                _deaccelerationTime1zItem
            };
            EditableFloatItemHelper.InitializeCommands(_editableItems, _plc, RaisePropertyChanged);

            _readTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            _readTimer.Tick += ReadTimer_Tick;
            RegisterLifecycleTimer(_readTimer);
            RegisterViewLifecycleAction(
                () => Logger.For("RobotControl").Debug("PolishBSideArm1ViewModel activated, started PLC read timer."),
                () => Logger.For("RobotControl").Debug("PolishBSideArm1ViewModel deactivated, stopped PLC read timer."));

            StationOperationCommand = new DelegateCommand(() => CurrentViewIndex = 1);

            StandbyMoveX1Command = OperationAudit.Create("StandbyMoveX1", () => _plc.Write(PolishBSideArm1Address.StandbyPosMove_1x, !StandbyPosMoveToggle_1x), () => "PolishBSideArm1");
            TransferMoveX1Command = OperationAudit.Create("TransferMoveX1", () => _plc.Write(PolishBSideArm1Address.TransferArmMove_1x, !TransferArmMoveToggle_1x), () => "PolishBSideArm1");
            PolishCMoveX1Command = OperationAudit.Create("PolishCMoveX1", () => _plc.Write(PolishBSideArm1Address.PolishCMove_1x, !PolishCPosMoveToggle_1x), () => "PolishBSideArm1");
            PolishDMoveX1Command = OperationAudit.Create("PolishDMoveX1", () => _plc.Write(PolishBSideArm1Address.PolishDMove_1x, !PolishDPosMoveToggle_1x), () => "PolishBSideArm1");

            StandbyMoveZ1Command = OperationAudit.Create("StandbyMoveZ1", () => _plc.Write(PolishBSideArm1Address.StandbyPosMove_1z, !StandbyPosMoveToggle_1z), () => "PolishBSideArm1");
            TransferMoveZ1Command = OperationAudit.Create("TransferMoveZ1", () => _plc.Write(PolishBSideArm1Address.TransferArmMove_1z, !TransferArmMoveToggle_1z), () => "PolishBSideArm1");
            PolishCMoveZ1Command = OperationAudit.Create("PolishCMoveZ1", () => _plc.Write(PolishBSideArm1Address.PolishCMove_1z, !PolishCPosMoveToggle_1z), () => "PolishBSideArm1");
            PolishDMoveZ1Command = OperationAudit.Create("PolishDMoveZ1", () => _plc.Write(PolishBSideArm1Address.PolishDMove_1z, !PolishDPosMoveToggle_1z), () => "PolishBSideArm1");

            EnableCommandX1Command = OperationAudit.Create("EnableCommandX1", () => _plc.Write(PolishBSideArm1Address.Enable_1x, !IsEnabled_1x), () => "PolishBSideArm1");
            EnableCommandZ1Command = OperationAudit.Create("EnableCommandZ1", () => _plc.Write(PolishBSideArm1Address.Enable_1z, !IsEnabled_1z), () => "PolishBSideArm1");

            PressJogUpX1Command = OperationAudit.Create("PressJogUpX1", () => _plc.Write(PolishBSideArm1Address.JogUp_1x, true), () => "PolishBSideArm1");
            ReleaseJogUpX1Command = OperationAudit.Create("ReleaseJogUpX1", () => _plc.Write(PolishBSideArm1Address.JogUp_1x, false), () => "PolishBSideArm1");
            PressJogDownX1Command = OperationAudit.Create("PressJogDownX1", () => _plc.Write(PolishBSideArm1Address.JogDown_1x, true), () => "PolishBSideArm1");
            ReleaseJogDownX1Command = OperationAudit.Create("ReleaseJogDownX1", () => _plc.Write(PolishBSideArm1Address.JogDown_1x, false), () => "PolishBSideArm1");

            PressJogUpZ1Command = OperationAudit.Create("PressJogUpZ1", () => _plc.Write(PolishBSideArm1Address.JogUp_1z, true), () => "PolishBSideArm1");
            ReleaseJogUpZ1Command = OperationAudit.Create("ReleaseJogUpZ1", () => _plc.Write(PolishBSideArm1Address.JogUp_1z, false), () => "PolishBSideArm1");
            PressJogDownZ1Command = OperationAudit.Create("PressJogDownZ1", () => _plc.Write(PolishBSideArm1Address.JogDown_1z, true), () => "PolishBSideArm1");
            ReleaseJogDownZ1Command = OperationAudit.Create("ReleaseJogDownZ1", () => _plc.Write(PolishBSideArm1Address.JogDown_1z, false), () => "PolishBSideArm1");

            OrignX1Command = OperationAudit.Create("OrignX1", () =>
            {
                if (MessageBox.Show(Lang.Resources.ConfirmActionContent, Lang.Resources.ConfirmActionTips,
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _plc.Write(PolishBSideArm1Address.Origin_1x, !OriginToggle_1x);
                }

                OriginToggle_1x = false;
            }, () => "PolishBSideArm1");
            OrignZ1Command = OperationAudit.Create("OrignZ1", () =>
            {
                if (MessageBox.Show(Lang.Resources.ConfirmActionContent, Lang.Resources.ConfirmActionTips,
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _plc.Write(PolishBSideArm1Address.Origin_1z, !OriginToggle_1z);
                }

                OriginToggle_1z = false;
            }, () => "PolishBSideArm1");

            EmergencyCommand = OperationAudit.Create("Emergency", () => _plc.Write(PolishBSideArm1Address.EmergencyStop, !EmergencyStopToggle), () => "PolishBSideArm1");
            AlarmResetCommand = OperationAudit.Create("AlarmReset", () => _plc.Write(PolishBSideArm1Address.AlarmReset, !AlarmResetToggle), () => "PolishBSideArm1");
            SaveCommand = OperationAudit.Create("Save", () => _plc.Write(PolishBSideArm1Address.Save, !SaveToggle), () => "PolishBSideArm1");

            BackCommand = new DelegateCommand(() => CurrentViewIndex = 0);
            StationResetCommand = OperationAudit.Create("StationReset", () => _plc.Write(PolishBSideArm1Address.StationReset, true), () => "PolishBSideArm1");
            StationStartCommand = OperationAudit.Create("StationStart", () => _plc.Write(PolishBSideArm1Address.StationStart, true), () => "PolishBSideArm1");
            StationStopCommand = OperationAudit.Create("StationStop", () => _plc.Write(PolishBSideArm1Address.StationStop, true), () => "PolishBSideArm1");
            StationAlarmResetCommand = OperationAudit.Create("StationAlarmReset", () => _plc.Write(PolishBSideArm1Address.StationAlarmReset, true), () => "PolishBSideArm1");

            PickFromTransferCommand = OperationAudit.Create("PickFromTransfer", () =>
            {
                _plc.Write(PolishBSideArm1Address.PickFromTransfer, true);
                Thread.Sleep(50);
                _plc.Write(PolishBSideArm1Address.PickFromTransfer, false);
            }, () => "PolishBSideArm1");
            PlaceToPolishCCommand = OperationAudit.Create("PlaceToPolishC", () =>
            {
                _plc.Write(PolishBSideArm1Address.PlaceToPolishC, true);
                Thread.Sleep(50);
                _plc.Write(PolishBSideArm1Address.PlaceToPolishC, false);
            }, () => "PolishBSideArm1");
            PlaceToPolishDCommand = OperationAudit.Create("PlaceToPolishD", () =>
            {
                _plc.Write(PolishBSideArm1Address.PlaceToPolishD, true);
                Thread.Sleep(50);
                _plc.Write(PolishBSideArm1Address.PlaceToPolishD, false);
            }, () => "PolishBSideArm1");
            Spare1Command = OperationAudit.Create("Spare1", () =>
            {
                _plc.Write(PolishBSideArm1Address.Spare1, true);
                Thread.Sleep(50);
                _plc.Write(PolishBSideArm1Address.Spare1, false);
            }, () => "PolishBSideArm1");
            Spare2Command = OperationAudit.Create("Spare2", () =>
            {
                _plc.Write(PolishBSideArm1Address.Spare2, true);
                Thread.Sleep(50);   
                _plc.Write(PolishBSideArm1Address.Spare2, false);
            }, () => "PolishBSideArm1");

            ManualModeCommand = OperationAudit.Create("ManualMode", () => _plc.Write(PolishBSideArm1Address.ManualMode, !ManualModeToggle), () => "PolishBSideArm1");
            AutoModeCommand = OperationAudit.Create("AutoMode", () => _plc.Write(PolishBSideArm1Address.AutoMode, !AutoModeToggle), () => "PolishBSideArm1");
        }

        private async void ReadTimer_Tick(object? sender, EventArgs e)
        {
            if (!_plc.IsConnected && !MitsubishiPlcService.DebugMode) return;

            try
            {
                await ReadPlcDataAsync();
            }
            catch (Exception ex)
            {
                Logger.For(Logger.Category.RobotControl).Error("Error reading PLC data", ex);
            }
        }

        private async Task ReadPlcDataAsync()
        {
            var mBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.GlobalAlarm, assign: v => GlobalAlarm = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.Save, assign: v => SaveToggle = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.EmergencyStop, assign: v => EmergencyStopToggle = v),

                new PlcBatchReadItem<bool>(PolishBSideArm1Address.Origin_1x, assign: v => OriginToggle_1x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.StandbyPosMove_1x, assign: v => StandbyPosMoveToggle_1x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.TransferArmMove_1x, assign: v => TransferArmMoveToggle_1x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.PolishCMove_1x, assign: v => PolishCPosMoveToggle_1x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.PolishDMove_1x, assign: v => PolishDPosMoveToggle_1x = v),

                new PlcBatchReadItem<bool>(PolishBSideArm1Address.Origin_1z, assign: v => OriginToggle_1z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.StandbyPosMove_1z, assign: v => StandbyPosMoveToggle_1z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.TransferArmMove_1z, assign: v => TransferArmMoveToggle_1z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.PolishCMove_1z, assign: v => PolishCPosMoveToggle_1z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.PolishDMove_1z, assign: v => PolishDPosMoveToggle_1z = v),

                new PlcBatchReadItem<bool>(PolishBSideArm1Address.AutoMode, assign: v => AutoModeToggle = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.ManualMode, assign: v => ManualModeToggle = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.PickFromTransfer, assign: v => PickFromTransferToggle = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.PlaceToPolishC, assign: v => PlaceToPolishCToggle = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.PlaceToPolishD, assign: v => PlaceToPolishDToggle = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.Spare1, assign: v => Spare1Toggle = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.Spare2, assign: v => Spare2Toggle = v),

                new PlcBatchReadItem<bool>(PolishBSideArm1Address.EnableStatus_1x, assign: v => IsEnabled_1x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.OriginStatus_1x, assign: v => IsAtOrigin_1x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.JogUpStatus_1x, assign: v => JogUpActive_1x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.JogDownStatus_1x, assign: v => JogDownActive_1x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.JogMiddleStatus_1x, assign: v => JogMiddleActive_1x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.LimitAlarm_1x, assign: v => LimitAlarm_1x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.DriverAlarm_1x, assign: v => DriverAlarm_1x = v),

                new PlcBatchReadItem<bool>(PolishBSideArm1Address.EnableStatus_1z, assign: v => IsEnabled_1z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.OriginStatus_1z, assign: v => IsAtOrigin_1z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.JogUpStatus_1z, assign: v => JogUpActive_1z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.JogDownStatus_1z, assign: v => JogDownActive_1z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.JogMiddleStatus_1z, assign: v => JogMiddleActive_1z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.LimitAlarm_1z, assign: v => LimitAlarm_1z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.DriverAlarm_1z, assign: v => DriverAlarm_1z = v),

                new PlcBatchReadItem<bool>(PolishBSideArm1Address.StationResetStatus, assign: v => StationIsReset = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.StationRunningStatus, assign: v => StationIsRunning = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.StationStoppedStatus, assign: v => StationIsStopped = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.ModuleAlarm, assign: v => ModuleAlarm = v),

                new PlcBatchReadItem<bool>(PolishBSideArm1Address.PickFromTransferActiveStatus, assign: v => PickFromTransferActive = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.PlaceToPolishCActiveStatus, assign: v => PlaceToPolishCActive = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.PlaceToPolishDActiveStatus, assign: v => PlaceToPolishDActive = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.Spare1ActiveStatus, assign: v => Spare1Active = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.Spare2ActiveStatus, assign: v => Spare2Active = v),

                new PlcBatchReadItem<bool>(PolishBSideArm1Address.PickFromTransferDoneStatus, assign: v => PickFromTransferDone = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.PlaceToPolishCDoneStatus, assign: v => PlaceToPolishCDone = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.PlaceToPolishDDoneStatus, assign: v => PlaceToPolishDDone = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.Spare1DoneStatus, assign: v => Spare1Done = v),
                new PlcBatchReadItem<bool>(PolishBSideArm1Address.Spare2DoneStatus, assign: v => Spare2Done = v)
            };

            var dStatusBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<int>(PolishBSideArm1Address.CurrentCoordinate_1x, assign: v => CurrentCoordinate_1x = v / 1000),
                new PlcBatchReadItem<int>(PolishBSideArm1Address.AlarmCode_1x, assign: v => AlarmCode_1x = v),
                new PlcBatchReadItem<int>(PolishBSideArm1Address.CurrentCoordinate_1z, assign: v => CurrentCoordinate_1z = v / 1000),
                new PlcBatchReadItem<int>(PolishBSideArm1Address.AlarmCode_1z, assign: v => AlarmCode_1z = v)
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
            var x1020Task = _plc.ReadAsync<bool[]>(PolishBSideArm1Address.Arm1Fiber1, 3);
            var b0bTask = _plc.ReadAsync<bool>(PolishBSideArm1Address.AlarmReset);

            await Task.WhenAll(mBatchTask, dStatusBatchTask, editableBatchTask, x1020Task, b0bTask);

            if (!mBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[PolishBSideArm1] M batch read failed: {mBatchTask.Result.Message}");

            if (!dStatusBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[PolishBSideArm1] D status batch read failed: {dStatusBatchTask.Result.Message}");

            if (!editableBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[PolishBSideArm1] D editable batch read failed: {editableBatchTask.Result.Message}");

            if (x1020Task.Result.IsSuccess)
            {
                var x = x1020Task.Result.Content;
                if (x.Length >= 3)
                {
                    Arm1Fiber1 = x[0];
                    Arm1Fiber2 = x[1];
                    Arm1Fiber3 = x[2];
                }
            }
            else
            {
                Logger.For("RobotControl").Warn($"[PolishBSideArm1] X1020-X1022 read failed: {x1020Task.Result.Message}");
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
    }
}

