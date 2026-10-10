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
    public class PolishBSideArm3ViewModel : StationOpViewModelBase
    {
        private bool _isReading;
        private readonly EditableFloatItem _standbyPosition3xItem = new(PolishBSideArm3Address.StandbyPosition_3x, nameof(StandbyPosition_3x), nameof(IsStandbyPosition_3xDirty));
        private readonly EditableFloatItem _polishCPosition3xItem = new(PolishBSideArm3Address.PolishCPosition_3x, nameof(PolishCPosition_3x), nameof(IsPolishCPosition_3xDirty));
        private readonly EditableFloatItem _polishDPosition3xItem = new(PolishBSideArm3Address.PolishDPosition_3x, nameof(PolishDPosition_3x), nameof(IsPolishDPosition_3xDirty));
        private readonly EditableFloatItem _transferArmPosition3xItem = new(PolishBSideArm3Address.TransferArmPosition_3x, nameof(TransferArmPosition_3x), nameof(IsTransferArmPosition_3xDirty));
        private readonly EditableFloatItem _jogSpeed3xItem = new(PolishBSideArm3Address.JogSpeed_3x, nameof(JogSpeed_3x), nameof(IsJogSpeed_3xDirty));
        private readonly EditableFloatItem _autoSpeed3xItem = new(PolishBSideArm3Address.AutoSpeed_3x, nameof(AutoSpeed_3x), nameof(IsAutoSpeed_3xDirty));
        private readonly EditableFloatItem _speedWhenHasWafer3xItem = new(PolishBSideArm3Address.SpeedWhenHasWafer3x, nameof(SpeedWhenHasWafer3x), nameof(IsSpeedWhenHasWafer3xDirty));
        private readonly EditableFloatItem _accelerationTime3xItem = new(PolishBSideArm3Address.AccelerationTime3x, nameof(AccelerationTime3x), nameof(IsAccelerationTime3xDirty));
        private readonly EditableFloatItem _deaccelerationTime3xItem = new(PolishBSideArm3Address.DeaccelerationTime3x, nameof(DeaccelerationTime3x), nameof(IsDeaccelerationTime3xDirty));

        private readonly EditableFloatItem _standbyPosition3zItem = new(PolishBSideArm3Address.StandbyPosition_3z, nameof(StandbyPosition_3z), nameof(IsStandbyPosition_3zDirty));
        private readonly EditableFloatItem _polishCPosition3zItem = new(PolishBSideArm3Address.PolishCPosition_3z, nameof(PolishCPosition_3z), nameof(IsPolishCPosition_3zDirty));
        private readonly EditableFloatItem _polishDPosition3zItem = new(PolishBSideArm3Address.PolishDPosition_3z, nameof(PolishDPosition_3z), nameof(IsPolishDPosition_3zDirty));
        private readonly EditableFloatItem _transferArmPosition3zItem = new(PolishBSideArm3Address.TransferArmPosition_3z, nameof(TransferArmPosition_3z), nameof(IsTransferArmPosition_3zDirty));
        private readonly EditableFloatItem _jogSpeed3zItem = new(PolishBSideArm3Address.JogSpeed_3z, nameof(JogSpeed_3z), nameof(IsJogSpeed_3zDirty));
        private readonly EditableFloatItem _autoSpeed3zItem = new(PolishBSideArm3Address.AutoSpeed_3z, nameof(AutoSpeed_3z), nameof(IsAutoSpeed_3zDirty));
        private readonly EditableFloatItem _speedWhenHasWafer3zItem = new(PolishBSideArm3Address.SpeedWhenHasWafer3z, nameof(SpeedWhenHasWafer3z), nameof(IsSpeedWhenHasWafer3zDirty));
        private readonly EditableFloatItem _accelerationTime3zItem = new(PolishBSideArm3Address.AccelerationTime3z, nameof(AccelerationTime3z), nameof(IsAccelerationTime3zDirty));
        private readonly EditableFloatItem _deaccelerationTime3zItem = new(PolishBSideArm3Address.DeaccelerationTime3z, nameof(DeaccelerationTime3z), nameof(IsDeaccelerationTime3zDirty));

        private readonly List<EditableFloatItem> _editableItems;

        /// <summary>
        /// 杞翠綅缃渶灏忓€?
        /// </summary>
        public double PositionMinimum { get; set; } = -90000;

        /// <summary>
        /// 杞翠綅缃渶澶у€?
        /// </summary>
        public double PositionMaximum { get; set; } = 90000;

        #region AxisPositionOfSideBArm-3x 灞炴€?

        #region 杞翠綅缃拰灞炴€?
        public bool IsStandbyPosition_3xDirty => _standbyPosition3xItem.IsDirty;
        /// <summary>
        /// 寰呮満浣?
        /// </summary>
        public double StandbyPosition_3x
        {
            get => _standbyPosition3xItem.Value;
            set => EditableFloatItemHelper.SetValue(_standbyPosition3xItem, value, RaisePropertyChanged);
        }

        public bool IsPolishCPosition_3xDirty => _polishCPosition3xItem.IsDirty;
        /// <summary>
        /// PolishC浣嶇疆 X1鏈烘鑷?
        /// </summary>
        public double PolishCPosition_3x
        {
            get => _polishCPosition3xItem.Value;
            set => EditableFloatItemHelper.SetValue(_polishCPosition3xItem, value, RaisePropertyChanged);
        }

        public bool IsPolishDPosition_3xDirty => _polishDPosition3xItem.IsDirty;
        /// <summary>
        /// PolishD浣嶇疆 X1鏈烘鑷?
        /// </summary>
        public double PolishDPosition_3x
        {
            get => _polishDPosition3xItem.Value;
            set => EditableFloatItemHelper.SetValue(_polishDPosition3xItem, value, RaisePropertyChanged);
        }

        public bool IsTransferArmPosition_3xDirty => _transferArmPosition3xItem.IsDirty;
        /// <summary>
        /// 涓浆鎵嬭噦浣?
        /// </summary>
        public double TransferArmPosition_3x
        {
            get => _transferArmPosition3xItem.Value;
            set => EditableFloatItemHelper.SetValue(_transferArmPosition3xItem, value, RaisePropertyChanged);
        }
        #endregion

        #region Move Toggle灞炴€?
        private bool _standbyPosMoveToggle_3x;
        public bool StandbyPosMoveToggle_3x
        {
            get => _standbyPosMoveToggle_3x;
            set => SetProperty(ref _standbyPosMoveToggle_3x, value);
        }

        private bool _polishCPosMoveToggle_3x;
        public bool PolishCPosMoveToggle_3x
        {
            get => _polishCPosMoveToggle_3x;
            set => SetProperty(ref _polishCPosMoveToggle_3x, value);
        }

        private bool _polishDPosMoveToggle_3x;
        public bool PolishDPosMoveToggle_3x
        {
            get => _polishDPosMoveToggle_3x;
            set => SetProperty(ref _polishDPosMoveToggle_3x, value);
        }

        private bool _transferArmMoveToggle_3x;
        public bool TransferArmMoveToggle_3x
        {
            get => _transferArmMoveToggle_3x;
            set => SetProperty(ref _transferArmMoveToggle_3x, value);
        }
        #endregion

        #region 閫熷害鍜屽潗鏍囧睘鎬?

        public bool IsJogSpeed_3xDirty => _jogSpeed3xItem.IsDirty;
        public double JogSpeed_3x
        {
            get => _jogSpeed3xItem.Value;
            set => EditableFloatItemHelper.SetValue(_jogSpeed3xItem, value, RaisePropertyChanged);
        }

        public bool IsAutoSpeed_3xDirty => _autoSpeed3xItem.IsDirty;
        public double AutoSpeed_3x
        {
            get => _autoSpeed3xItem.Value;
            set => EditableFloatItemHelper.SetValue(_autoSpeed3xItem, value, RaisePropertyChanged);
        }
        public bool IsSpeedWhenHasWafer3xDirty => _speedWhenHasWafer3xItem.IsDirty;
        public double SpeedWhenHasWafer3x { get => _speedWhenHasWafer3xItem.Value; set => EditableFloatItemHelper.SetValue(_speedWhenHasWafer3xItem, value, RaisePropertyChanged); }
        public bool IsAccelerationTime3xDirty => _accelerationTime3xItem.IsDirty;
        public double AccelerationTime3x { get => _accelerationTime3xItem.Value; set => EditableFloatItemHelper.SetValue(_accelerationTime3xItem, value, RaisePropertyChanged); }
        public bool IsDeaccelerationTime3xDirty => _deaccelerationTime3xItem.IsDirty;
        public double DeaccelerationTime3x { get => _deaccelerationTime3xItem.Value; set => EditableFloatItemHelper.SetValue(_deaccelerationTime3xItem, value, RaisePropertyChanged); }

        private double _currentCoordinate_3x;
        public double CurrentCoordinate_3x
        {
            get => _currentCoordinate_3x;
            set => SetProperty(ref _currentCoordinate_3x, value);
        }

        #endregion

        #region 鐘舵€佸睘鎬?

        private bool _isEnabled_3x;
        public bool IsEnabled_3x
        {
            get => _isEnabled_3x;
            set => SetProperty(ref _isEnabled_3x, value);
        }

        private bool _driverAlarm_3x;
        public bool DriverAlarm_3x
        {
            get => _driverAlarm_3x;
            set => SetProperty(ref _driverAlarm_3x, value);
        }

        private bool _limitAlarm_3x;
        public bool LimitAlarm_3x
        {
            get => _limitAlarm_3x;
            set => SetProperty(ref _limitAlarm_3x, value);
        }

        private bool _isAtOrigin_3x;
        public bool IsAtOrigin_3x
        {
            get => _isAtOrigin_3x;
            set => SetProperty(ref _isAtOrigin_3x, value);
        }

        private int _alarmCode_3x;
        public int AlarmCode_3x
        {
            get => _alarmCode_3x;
            set => SetProperty(ref _alarmCode_3x, value);
        }

        #endregion

        #region Jog 鎺у埗灞炴€?

        private bool _jogUpActive_3x;
        public bool JogUpActive_3x
        {
            get => _jogUpActive_3x;
            set => SetProperty(ref _jogUpActive_3x, value);
        }

        private bool _jogMiddleActive_3x;
        public bool JogMiddleActive_3x
        {
            get => _jogMiddleActive_3x;
            set => SetProperty(ref _jogMiddleActive_3x, value);
        }

        private bool _jogDownActive_3x;
        public bool JogDownActive_3x
        {
            get => _jogDownActive_3x;
            set => SetProperty(ref _jogDownActive_3x, value);
        }

        private bool _originToggle_3x;
        public bool OriginToggle_3x
        {
            get => _originToggle_3x;
            set => SetProperty(ref _originToggle_3x, value);
        }
        #endregion

        #endregion

        #region AxisPositionOfSideBArm-3z 灞炴€?

        #region 杞翠綅缃拰灞炴€?
        public bool IsStandbyPosition_3zDirty => _standbyPosition3zItem.IsDirty;
        /// <summary>
        /// 寰呮満浣?
        /// </summary>
        public double StandbyPosition_3z
        {
            get => _standbyPosition3zItem.Value;
            set => EditableFloatItemHelper.SetValue(_standbyPosition3zItem, value, RaisePropertyChanged);
        }

        public bool IsPolishCPosition_3zDirty => _polishCPosition3zItem.IsDirty;
        /// <summary>
        /// PolishC浣嶇疆 X1鏈烘鑷?
        /// </summary>
        public double PolishCPosition_3z
        {
            get => _polishCPosition3zItem.Value;
            set => EditableFloatItemHelper.SetValue(_polishCPosition3zItem, value, RaisePropertyChanged);
        }

        public bool IsPolishDPosition_3zDirty => _polishDPosition3zItem.IsDirty;
        /// <summary>
        /// PolishD浣嶇疆 X1鏈烘鑷?
        /// </summary>
        public double PolishDPosition_3z
        {
            get => _polishDPosition3zItem.Value;
            set => EditableFloatItemHelper.SetValue(_polishDPosition3zItem, value, RaisePropertyChanged);
        }

        public bool IsTransferArmPosition_3zDirty => _transferArmPosition3zItem.IsDirty;
        /// <summary>
        /// 涓浆鎵嬭噦浣?
        /// </summary>
        public double TransferArmPosition_3z
        {
            get => _transferArmPosition3zItem.Value;
            set => EditableFloatItemHelper.SetValue(_transferArmPosition3zItem, value, RaisePropertyChanged);
        }
        #endregion

        #region Move Toggle灞炴€?
        private bool _standbyPosMoveToggle_3z;
        public bool StandbyPosMoveToggle_3z
        {
            get => _standbyPosMoveToggle_3z;
            set => SetProperty(ref _standbyPosMoveToggle_3z, value);
        }

        private bool _polishCPosMoveToggle_3z;
        public bool PolishCPosMoveToggle_3z
        {
            get => _polishCPosMoveToggle_3z;
            set => SetProperty(ref _polishCPosMoveToggle_3z, value);
        }

        private bool _polishDPosMoveToggle_3z;
        public bool PolishDPosMoveToggle_3z
        {
            get => _polishDPosMoveToggle_3z;
            set => SetProperty(ref _polishDPosMoveToggle_3z, value);
        }

        private bool _transferArmMoveToggle_3z;
        public bool TransferArmMoveToggle_3z
        {
            get => _transferArmMoveToggle_3z;
            set => SetProperty(ref _transferArmMoveToggle_3z, value);
        }
        #endregion

        #region 閫熷害鍜屽潗鏍囧睘鎬?

        public bool IsJogSpeed_3zDirty => _jogSpeed3zItem.IsDirty;
        public double JogSpeed_3z
        {
            get => _jogSpeed3zItem.Value;
            set => EditableFloatItemHelper.SetValue(_jogSpeed3zItem, value, RaisePropertyChanged);
        }

        public bool IsAutoSpeed_3zDirty => _autoSpeed3zItem.IsDirty;
        public double AutoSpeed_3z
        {
            get => _autoSpeed3zItem.Value;
            set => EditableFloatItemHelper.SetValue(_autoSpeed3zItem, value, RaisePropertyChanged);
        }
        public bool IsSpeedWhenHasWafer3zDirty => _speedWhenHasWafer3zItem.IsDirty;
        public double SpeedWhenHasWafer3z { get => _speedWhenHasWafer3zItem.Value; set => EditableFloatItemHelper.SetValue(_speedWhenHasWafer3zItem, value, RaisePropertyChanged); }
        public bool IsAccelerationTime3zDirty => _accelerationTime3zItem.IsDirty;
        public double AccelerationTime3z { get => _accelerationTime3zItem.Value; set => EditableFloatItemHelper.SetValue(_accelerationTime3zItem, value, RaisePropertyChanged); }
        public bool IsDeaccelerationTime3zDirty => _deaccelerationTime3zItem.IsDirty;
        public double DeaccelerationTime3z { get => _deaccelerationTime3zItem.Value; set => EditableFloatItemHelper.SetValue(_deaccelerationTime3zItem, value, RaisePropertyChanged); }

        private double _currentCoordinate_3z;
        public double CurrentCoordinate_3z
        {
            get => _currentCoordinate_3z;
            set => SetProperty(ref _currentCoordinate_3z, value);
        }

        #endregion

        #region 鐘舵€佸睘鎬?

        private bool _isEnabled_3z;
        public bool IsEnabled_3z
        {
            get => _isEnabled_3z;
            set => SetProperty(ref _isEnabled_3z, value);
        }

        private bool _driverAlarm_3z;
        public bool DriverAlarm_3z
        {
            get => _driverAlarm_3z;
            set => SetProperty(ref _driverAlarm_3z, value);
        }

        private bool _limitAlarm_3z;
        public bool LimitAlarm_3z
        {
            get => _limitAlarm_3z;
            set => SetProperty(ref _limitAlarm_3z, value);
        }

        private bool _isAtOrigin_3z;
        public bool IsAtOrigin_3z
        {
            get => _isAtOrigin_3z;
            set => SetProperty(ref _isAtOrigin_3z, value);
        }

        private int _alarmCode_3z;
        public int AlarmCode_3z
        {
            get => _alarmCode_3z;
            set => SetProperty(ref _alarmCode_3z, value);
        }

        #endregion

        #region Jog 鎺у埗灞炴€?

        private bool _jogUpActive_3z;
        public bool JogUpActive_3z
        {
            get => _jogUpActive_3z;
            set => SetProperty(ref _jogUpActive_3z, value);
        }

        private bool _jogMiddleActive_3z;
        public bool JogMiddleActive_3z
        {
            get => _jogMiddleActive_3z;
            set => SetProperty(ref _jogMiddleActive_3z, value);
        }

        private bool _jogDownActive_3z;
        public bool JogDownActive_3z
        {
            get => _jogDownActive_3z;
            set => SetProperty(ref _jogDownActive_3z, value);
        }

        private bool _originToggle_3z;
        public bool OriginToggle_3z
        {
            get => _originToggle_3z;
            set => SetProperty(ref _originToggle_3z, value);
        }
        #endregion

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

        private bool _pickFromPolishCActive;
        public bool PickFromPolishCActive { get => _pickFromPolishCActive; set => SetProperty(ref _pickFromPolishCActive, value); }
        private bool _pickFromPolishCToggle;
        public bool PickFromPolishCToggle { get => _pickFromPolishCToggle; set => SetProperty(ref _pickFromPolishCToggle, value); }
        private bool _pickFromPolishCDone;
        public bool PickFromPolishCDone { get => _pickFromPolishCDone; set => SetProperty(ref _pickFromPolishCDone, value); }

        private bool _pickFromPolishDActive;
        public bool PickFromPolishDActive { get => _pickFromPolishDActive; set => SetProperty(ref _pickFromPolishDActive, value); }
        private bool _pickFromPolishDToggle;
        public bool PickFromPolishDToggle { get => _pickFromPolishDToggle; set => SetProperty(ref _pickFromPolishDToggle, value); }
        private bool _pickFromPolishDDone;
        public bool PickFromPolishDDone { get => _pickFromPolishDDone; set => SetProperty(ref _pickFromPolishDDone, value); }

        private bool _placeToTransferActive;
        public bool PlaceToTransferActive { get => _placeToTransferActive; set => SetProperty(ref _placeToTransferActive, value); }
        private bool _placeToTransferToggle;
        public bool PlaceToTransferToggle { get => _placeToTransferToggle; set => SetProperty(ref _placeToTransferToggle, value); }
        private bool _placeToTransferDone;
        public bool PlaceToTransferDone { get => _placeToTransferDone; set => SetProperty(ref _placeToTransferDone, value); }

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

        private bool _arm3Fiber1;
        public bool Arm3Fiber1 { get => _arm3Fiber1; set => SetProperty(ref _arm3Fiber1, value); }

        private bool _arm3Fiber2;
        public bool Arm3Fiber2 { get => _arm3Fiber2; set => SetProperty(ref _arm3Fiber2, value); }

        private bool _arm3Fiber3;
        public bool Arm3Fiber3 { get => _arm3Fiber3; set => SetProperty(ref _arm3Fiber3, value); }
        private bool _powerOffToggle;
        public bool PowerOffToggle { get => _powerOffToggle; set => SetProperty(ref _powerOffToggle, value); }
        private int _foupSlot;
        public int FoupSlot { get => _foupSlot; set => SetProperty(ref _foupSlot, value); }
        private int _waferIndex;
        public int WaferIndex { get => _waferIndex; set => SetProperty(ref _waferIndex, value); }

        #endregion

        #region Commands
        public DelegateCommand StationOperationCommand { get; }

        public DelegateCommand StandbyMoveX3Command { get; }
        public DelegateCommand PolishCMoveX3Command { get; }
        public DelegateCommand PolishDMoveX3Command { get; }
        public DelegateCommand TransferMoveX3Command { get; }
        public DelegateCommand StandbyMoveZ3Command { get; }
        public DelegateCommand PolishCMoveZ3Command { get; }
        public DelegateCommand PolishDMoveZ3Command { get; }
        public DelegateCommand TransferMoveZ3Command { get; }

        public DelegateCommand EnableCommandX3Command { get; }
        public DelegateCommand EnableCommandZ3Command { get; }

        public DelegateCommand PressJogUpX3Command { get; }
        public DelegateCommand ReleaseJogUpX3Command { get; }
        public DelegateCommand PressJogDownX3Command { get; }
        public DelegateCommand ReleaseJogDownX3Command { get; }
        public DelegateCommand PressJogUpZ3Command { get; }
        public DelegateCommand ReleaseJogUpZ3Command { get; }
        public DelegateCommand PressJogDownZ3Command { get; }
        public DelegateCommand ReleaseJogDownZ3Command { get; }

        public DelegateCommand OrignX3Command { get; }
        public DelegateCommand OrignZ3Command { get; }

        public DelegateCommand EmergencyCommand { get; }
        public DelegateCommand AlarmResetCommand { get; }
        public DelegateCommand SaveCommand { get; }

        public DelegateCommand BackCommand { get; }
        public DelegateCommand StationResetCommand { get; }
        public DelegateCommand StationStartCommand { get; }
        public DelegateCommand StationStopCommand { get; }
        public DelegateCommand StationAlarmResetCommand { get; }

        public DelegateCommand PickFromPolishCCommand { get; }
        public DelegateCommand PickFromPolishDCommand { get; }
        public DelegateCommand PlaceToTransferArmCommand { get; }
        public DelegateCommand Spare1Command { get; }
        public DelegateCommand Spare2Command { get; }

        public DelegateCommand ManualModeCommand { get; }
        public DelegateCommand AutoModeCommand { get; }
        public DelegateCommand PowerOffCommand { get; }

        public DelegateCommand StandbyPosition_3xSetCommand => _standbyPosition3xItem.SetCommand;
        public DelegateCommand StandbyPosition_3xCancelCommand => _standbyPosition3xItem.CancelCommand;
        public DelegateCommand PolishCPosition_3xSetCommand => _polishCPosition3xItem.SetCommand;
        public DelegateCommand PolishCPosition_3xCancelCommand => _polishCPosition3xItem.CancelCommand;
        public DelegateCommand PolishDPosition_3xSetCommand => _polishDPosition3xItem.SetCommand;
        public DelegateCommand PolishDPosition_3xCancelCommand => _polishDPosition3xItem.CancelCommand;
        public DelegateCommand TransferArmPosition_3xSetCommand => _transferArmPosition3xItem.SetCommand;
        public DelegateCommand TransferArmPosition_3xCancelCommand => _transferArmPosition3xItem.CancelCommand;
        public DelegateCommand JogSpeed_3xSetCommand => _jogSpeed3xItem.SetCommand;
        public DelegateCommand JogSpeed_3xCancelCommand => _jogSpeed3xItem.CancelCommand;
        public DelegateCommand AutoSpeed_3xSetCommand => _autoSpeed3xItem.SetCommand;
        public DelegateCommand AutoSpeed_3xCancelCommand => _autoSpeed3xItem.CancelCommand;
        public DelegateCommand SpeedWhenHasWafer3xSetCommand => _speedWhenHasWafer3xItem.SetCommand;
        public DelegateCommand SpeedWhenHasWafer3xCancelCommand => _speedWhenHasWafer3xItem.CancelCommand;
        public DelegateCommand AccelerationTime3xSetCommand => _accelerationTime3xItem.SetCommand;
        public DelegateCommand AccelerationTime3xCancelCommand => _accelerationTime3xItem.CancelCommand;
        public DelegateCommand DeaccelerationTime3xSetCommand => _deaccelerationTime3xItem.SetCommand;
        public DelegateCommand DeaccelerationTime3xCancelCommand => _deaccelerationTime3xItem.CancelCommand;

        public DelegateCommand StandbyPosition_3zSetCommand => _standbyPosition3zItem.SetCommand;
        public DelegateCommand StandbyPosition_3zCancelCommand => _standbyPosition3zItem.CancelCommand;
        public DelegateCommand PolishCPosition_3zSetCommand => _polishCPosition3zItem.SetCommand;
        public DelegateCommand PolishCPosition_3zCancelCommand => _polishCPosition3zItem.CancelCommand;
        public DelegateCommand PolishDPosition_3zSetCommand => _polishDPosition3zItem.SetCommand;
        public DelegateCommand PolishDPosition_3zCancelCommand => _polishDPosition3zItem.CancelCommand;
        public DelegateCommand TransferArmPosition_3zSetCommand => _transferArmPosition3zItem.SetCommand;
        public DelegateCommand TransferArmPosition_3zCancelCommand => _transferArmPosition3zItem.CancelCommand;
        public DelegateCommand JogSpeed_3zSetCommand => _jogSpeed3zItem.SetCommand;
        public DelegateCommand JogSpeed_3zCancelCommand => _jogSpeed3zItem.CancelCommand;
        public DelegateCommand AutoSpeed_3zSetCommand => _autoSpeed3zItem.SetCommand;
        public DelegateCommand AutoSpeed_3zCancelCommand => _autoSpeed3zItem.CancelCommand;
        public DelegateCommand SpeedWhenHasWafer3zSetCommand => _speedWhenHasWafer3zItem.SetCommand;
        public DelegateCommand SpeedWhenHasWafer3zCancelCommand => _speedWhenHasWafer3zItem.CancelCommand;
        public DelegateCommand AccelerationTime3zSetCommand => _accelerationTime3zItem.SetCommand;
        public DelegateCommand AccelerationTime3zCancelCommand => _accelerationTime3zItem.CancelCommand;
        public DelegateCommand DeaccelerationTime3zSetCommand => _deaccelerationTime3zItem.SetCommand;
        public DelegateCommand DeaccelerationTime3zCancelCommand => _deaccelerationTime3zItem.CancelCommand;
        #endregion

        public PolishBSideArm3ViewModel() : base(10, "Polish B Side Arm3")
        {
            _editableItems = new List<EditableFloatItem>
            {
                _standbyPosition3xItem,
                _polishCPosition3xItem,
                _polishDPosition3xItem,
                _transferArmPosition3xItem,
                _jogSpeed3xItem,
                _autoSpeed3xItem,
                _speedWhenHasWafer3xItem,
                _accelerationTime3xItem,
                _deaccelerationTime3xItem,
                _standbyPosition3zItem,
                _polishCPosition3zItem,
                _polishDPosition3zItem,
                _transferArmPosition3zItem,
                _jogSpeed3zItem,
                _autoSpeed3zItem,
                _speedWhenHasWafer3zItem,
                _accelerationTime3zItem,
                _deaccelerationTime3zItem
            };
            EditableFloatItemHelper.InitializeCommands(_editableItems, _plc, RaisePropertyChanged);

            _readTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            _readTimer.Tick += ReadTimer_Tick;
            RegisterLifecycleTimer(_readTimer);
            RegisterViewLifecycleAction(
                () => Logger.For("RobotControl").Debug("PolishBSideArm3ViewModel activated, started PLC read timer."),
                () => Logger.For("RobotControl").Debug("PolishBSideArm3ViewModel deactivated, stopped PLC read timer."));

            StationOperationCommand = new DelegateCommand(() => CurrentViewIndex = 1);

            StandbyMoveX3Command = OperationAudit.Create("StandbyMoveX3", () => _plc.Write(PolishBSideArm3Address.StandbyPosMove_3x, !StandbyPosMoveToggle_3x), () => "PolishBSideArm3");
            PolishCMoveX3Command = OperationAudit.Create("PolishCMoveX3", () => _plc.Write(PolishBSideArm3Address.PolishCMove_3x, !PolishCPosMoveToggle_3x), () => "PolishBSideArm3");
            PolishDMoveX3Command = OperationAudit.Create("PolishDMoveX3", () => _plc.Write(PolishBSideArm3Address.PolishDMove_3x, !PolishDPosMoveToggle_3x), () => "PolishBSideArm3");
            TransferMoveX3Command = OperationAudit.Create("TransferMoveX3", () => _plc.Write(PolishBSideArm3Address.TransferArmMove_3x, !TransferArmMoveToggle_3x), () => "PolishBSideArm3");

            StandbyMoveZ3Command = OperationAudit.Create("StandbyMoveZ3", () => _plc.Write(PolishBSideArm3Address.StandbyPosMove_3z, !StandbyPosMoveToggle_3z), () => "PolishBSideArm3");
            PolishCMoveZ3Command = OperationAudit.Create("PolishCMoveZ3", () => _plc.Write(PolishBSideArm3Address.PolishCMove_3z, !PolishCPosMoveToggle_3z), () => "PolishBSideArm3");
            PolishDMoveZ3Command = OperationAudit.Create("PolishDMoveZ3", () => _plc.Write(PolishBSideArm3Address.PolishDMove_3z, !PolishDPosMoveToggle_3z), () => "PolishBSideArm3");
            TransferMoveZ3Command = OperationAudit.Create("TransferMoveZ3", () => _plc.Write(PolishBSideArm3Address.TransferArmMove_3z, !TransferArmMoveToggle_3z), () => "PolishBSideArm3");

            EnableCommandX3Command = OperationAudit.Create("EnableCommandX3", () => _plc.Write(PolishBSideArm3Address.Enable_3x, !IsEnabled_3x), () => "PolishBSideArm3");
            EnableCommandZ3Command = OperationAudit.Create("EnableCommandZ3", () => _plc.Write(PolishBSideArm3Address.Enable_3z, !IsEnabled_3z), () => "PolishBSideArm3");

            PressJogUpX3Command = OperationAudit.Create("PressJogUpX3", () => _plc.Write(PolishBSideArm3Address.JogUp_3x, true), () => "PolishBSideArm3");
            ReleaseJogUpX3Command = OperationAudit.Create("ReleaseJogUpX3", () => _plc.Write(PolishBSideArm3Address.JogUp_3x, false), () => "PolishBSideArm3");
            PressJogDownX3Command = OperationAudit.Create("PressJogDownX3", () => _plc.Write(PolishBSideArm3Address.JogDown_3x, true), () => "PolishBSideArm3");
            ReleaseJogDownX3Command = OperationAudit.Create("ReleaseJogDownX3", () => _plc.Write(PolishBSideArm3Address.JogDown_3x, false), () => "PolishBSideArm3");

            PressJogUpZ3Command = OperationAudit.Create("PressJogUpZ3", () => _plc.Write(PolishBSideArm3Address.JogUp_3z, true), () => "PolishBSideArm3");
            ReleaseJogUpZ3Command = OperationAudit.Create("ReleaseJogUpZ3", () => _plc.Write(PolishBSideArm3Address.JogUp_3z, false), () => "PolishBSideArm3");
            PressJogDownZ3Command = OperationAudit.Create("PressJogDownZ3", () => _plc.Write(PolishBSideArm3Address.JogDown_3z, true), () => "PolishBSideArm3");
            ReleaseJogDownZ3Command = OperationAudit.Create("ReleaseJogDownZ3", () => _plc.Write(PolishBSideArm3Address.JogDown_3z, false), () => "PolishBSideArm3");

            OrignX3Command = OperationAudit.Create("OrignX3", () =>
            {
                if (MessageBox.Show(Lang.Resources.ConfirmActionContent, Lang.Resources.ConfirmActionTips,
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _plc.Write(PolishBSideArm3Address.Origin_3x, !OriginToggle_3x);
                }

                OriginToggle_3x = false;
            }, () => "PolishBSideArm3");
            OrignZ3Command = OperationAudit.Create("OrignZ3", () =>
            {
                if (MessageBox.Show(Lang.Resources.ConfirmActionContent, Lang.Resources.ConfirmActionTips,
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _plc.Write(PolishBSideArm3Address.Origin_3z, !OriginToggle_3z);
                }

                OriginToggle_3z = false;
            }, () => "PolishBSideArm3");

            EmergencyCommand = OperationAudit.Create("Emergency", () => _plc.Write(PolishBSideArm3Address.EmergencyStop, !EmergencyStopToggle), () => "PolishBSideArm3");
            AlarmResetCommand = OperationAudit.CreateAsync("AlarmReset", () => _plc.PulseAsync(PolishBSideArm3Address.AlarmReset), () => "PolishBSideArm3");
            SaveCommand = OperationAudit.Create("Save", () => _plc.Write(PolishBSideArm3Address.Save, !SaveToggle), () => "PolishBSideArm3");

            BackCommand = new DelegateCommand(() => CurrentViewIndex = 0);
            StationResetCommand = OperationAudit.CreateAsync("StationReset", () => _plc.PulseAsync(PolishBSideArm3Address.StationReset), () => "PolishBSideArm3");
            StationStartCommand = OperationAudit.CreateAsync("StationStart", () => _plc.PulseAsync(PolishBSideArm3Address.StationStart), () => "PolishBSideArm3");
            StationStopCommand = OperationAudit.CreateAsync("StationStop", () => _plc.PulseAsync(PolishBSideArm3Address.StationStop), () => "PolishBSideArm3");
            StationAlarmResetCommand = OperationAudit.CreateAsync("StationAlarmReset", () => _plc.PulseAsync(PolishBSideArm3Address.StationAlarmReset), () => "PolishBSideArm3");

            PickFromPolishCCommand = OperationAudit.CreateAsync("PickFromPolishC", () => _plc.PulseAsync(PolishBSideArm3Address.PickFromPolishC), () => "PolishBSideArm3");
            PickFromPolishDCommand = OperationAudit.CreateAsync("PickFromPolishD", () => _plc.PulseAsync(PolishBSideArm3Address.PickFromPolishD), () => "PolishBSideArm3");
            PlaceToTransferArmCommand = OperationAudit.CreateAsync("PlaceToTransferArm", () => _plc.PulseAsync(PolishBSideArm3Address.PlaceToTransferArm), () => "PolishBSideArm3");
            Spare1Command = OperationAudit.CreateAsync("Spare1", () => _plc.PulseAsync(PolishBSideArm3Address.Spare1), () => "PolishBSideArm3");
            Spare2Command = OperationAudit.CreateAsync("Spare2", () => _plc.PulseAsync(PolishBSideArm3Address.Spare2), () => "PolishBSideArm3");

            ManualModeCommand = OperationAudit.CreateAsync("ManualMode", () => _plc.PulseAsync(PolishBSideArm3Address.ManualMode), () => "PolishBSideArm3");
            AutoModeCommand = OperationAudit.CreateAsync("AutoMode", () => _plc.PulseAsync(PolishBSideArm3Address.AutoMode), () => "PolishBSideArm3");
            PowerOffCommand = OperationAudit.CreateAsync("PowerOff", () => _plc.PulseAsync(PolishBSideArm3Address.PowerOff), () => "PolishBSideArm3");
        }

        private async void ReadTimer_Tick(object? sender, EventArgs e)
        {
            if (_isReading || (!_plc.IsConnected && !MitsubishiPlcService.DebugMode)) return;
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

        /// <summary>
        /// 璇诲彇PLC鍦板潃鐨勬暟鎹?
        /// </summary>
        private async Task ReadPlcDataAsync()
        {
            var mBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.GlobalAlarm, assign: v => GlobalAlarm = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.Save, assign: v => SaveToggle = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.EmergencyStop, assign: v => EmergencyStopToggle = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.PowerOff, assign: v => PowerOffToggle = v),

                new PlcBatchReadItem<bool>(PolishBSideArm3Address.Origin_3x, assign: v => OriginToggle_3x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.StandbyPosMove_3x, assign: v => StandbyPosMoveToggle_3x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.PolishCMove_3x, assign: v => PolishCPosMoveToggle_3x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.PolishDMove_3x, assign: v => PolishDPosMoveToggle_3x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.TransferArmMove_3x, assign: v => TransferArmMoveToggle_3x = v),

                new PlcBatchReadItem<bool>(PolishBSideArm3Address.Origin_3z, assign: v => OriginToggle_3z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.StandbyPosMove_3z, assign: v => StandbyPosMoveToggle_3z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.PolishCMove_3z, assign: v => PolishCPosMoveToggle_3z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.PolishDMove_3z, assign: v => PolishDPosMoveToggle_3z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.TransferArmMove_3z, assign: v => TransferArmMoveToggle_3z = v),

                new PlcBatchReadItem<bool>(PolishBSideArm3Address.AutoMode, assign: v => AutoModeToggle = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.ManualMode, assign: v => ManualModeToggle = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.PickFromPolishC, assign: v => PickFromPolishCToggle = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.PickFromPolishD, assign: v => PickFromPolishDToggle = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.PlaceToTransferArm, assign: v => PlaceToTransferToggle = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.Spare1, assign: v => Spare1Toggle = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.Spare2, assign: v => Spare2Toggle = v),

                new PlcBatchReadItem<bool>(PolishBSideArm3Address.EnableStatus_3x, assign: v => IsEnabled_3x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.OriginStatus_3x, assign: v => IsAtOrigin_3x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.JogUpStatus_3x, assign: v => JogUpActive_3x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.JogDownStatus_3x, assign: v => JogDownActive_3x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.JogMiddleStatus_3x, assign: v => JogMiddleActive_3x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.LimitAlarm_3x, assign: v => LimitAlarm_3x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.DriverAlarm_3x, assign: v => DriverAlarm_3x = v),

                new PlcBatchReadItem<bool>(PolishBSideArm3Address.EnableStatus_3z, assign: v => IsEnabled_3z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.OriginStatus_3z, assign: v => IsAtOrigin_3z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.JogUpStatus_3z, assign: v => JogUpActive_3z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.JogDownStatus_3z, assign: v => JogDownActive_3z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.JogMiddleStatus_3z, assign: v => JogMiddleActive_3z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.LimitAlarm_3z, assign: v => LimitAlarm_3z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.DriverAlarm_3z, assign: v => DriverAlarm_3z = v),

                new PlcBatchReadItem<bool>(PolishBSideArm3Address.StationResetStatus, assign: v => StationIsReset = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.StationRunningStatus, assign: v => StationIsRunning = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.StationStoppedStatus, assign: v => StationIsStopped = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.ModuleAlarm, assign: v => ModuleAlarm = v),

                new PlcBatchReadItem<bool>(PolishBSideArm3Address.PickFromPolishCActiveStatus, assign: v => PickFromPolishCActive = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.PickFromPolishDActiveStatus, assign: v => PickFromPolishDActive = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.PlaceToTransferArmActiveStatus, assign: v => PlaceToTransferActive = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.Spare1ActiveStatus, assign: v => Spare1Active = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.Spare2ActiveStatus, assign: v => Spare2Active = v),

                new PlcBatchReadItem<bool>(PolishBSideArm3Address.PickFromPolishCDoneStatus, assign: v => PickFromPolishCDone = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.PickFromPolishDDoneStatus, assign: v => PickFromPolishDDone = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.PlaceToTransferArmDoneStatus, assign: v => PlaceToTransferDone = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.Spare1DoneStatus, assign: v => Spare1Done = v),
                new PlcBatchReadItem<bool>(PolishBSideArm3Address.Spare2DoneStatus, assign: v => Spare2Done = v)
            };

            var dStatusBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<int>(PolishBSideArm3Address.CurrentCoordinate_3x, assign: v => CurrentCoordinate_3x = v / 1000.0),
                new PlcBatchReadItem<int>(PolishBSideArm3Address.AlarmCode_3x, assign: v => AlarmCode_3x = v),
                new PlcBatchReadItem<int>(PolishBSideArm3Address.CurrentCoordinate_3z, assign: v => CurrentCoordinate_3z = v / 1000.0),
                new PlcBatchReadItem<int>(PolishBSideArm3Address.AlarmCode_3z, assign: v => AlarmCode_3z = v)
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
            var x1026Task = _plc.ReadAsync<bool[]>(PolishBSideArm3Address.Arm3Fiber1, 3);
            var b0bTask = _plc.ReadAsync<bool>(PolishBSideArm3Address.AlarmReset);
            var waferTask = _plc.ReadAsync<short[]>(PolishBSideArm3Address.FoupSlot, 2);

            await Task.WhenAll(mBatchTask, dStatusBatchTask, editableBatchTask, x1026Task, b0bTask, waferTask);

            if (!mBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[PolishBSideArm3] M batch read failed: {mBatchTask.Result.Message}");

            if (!dStatusBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[PolishBSideArm3] D status batch read failed: {dStatusBatchTask.Result.Message}");

            if (!editableBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[PolishBSideArm3] D editable batch read failed: {editableBatchTask.Result.Message}");

            if (x1026Task.Result.IsSuccess)
            {
                var x = x1026Task.Result.Content;
                if (x.Length >= 3)
                {
                    Arm3Fiber1 = x[0];
                    Arm3Fiber2 = x[1];
                    Arm3Fiber3 = x[2];
                }
            }
            else
            {
                Logger.For("RobotControl").Warn($"[PolishBSideArm3] X1026-X1028 read failed: {x1026Task.Result.Message}");
            }

            if (b0bTask.Result.IsSuccess)
            {
                AlarmResetToggle = b0bTask.Result.Content;
            }
            if (waferTask.Result.IsSuccess && waferTask.Result.Content.Length >= 2)
            {
                FoupSlot = waferTask.Result.Content[0];
                WaferIndex = waferTask.Result.Content[1];
            }
        }

        public void StopReading()
        {
            OnViewDeactivated();
        }
    }
}
