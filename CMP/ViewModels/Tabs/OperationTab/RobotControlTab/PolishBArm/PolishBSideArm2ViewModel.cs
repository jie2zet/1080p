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

namespace CMP.ViewModels.Tabs.OperationTab.RobotControlTab.PolishBArm
{
    public class PolishBSideArm2ViewModel : StationOpViewModelBase
    {
        private bool _isReading;
        private readonly EditableFloatItem _standbyPosition2xItem = new(PolishBSideArm2Address.StandbyPosition_2x, nameof(StandbyPosition_2x), nameof(IsStandbyPosition_2xDirty));
        private readonly EditableFloatItem _transferArmPosition2xItem = new(PolishBSideArm2Address.TransferArmPosition_2x, nameof(TransferArmPosition_2x), nameof(IsTransferArmPosition_2xDirty));
        private readonly EditableFloatItem _polishCPosition2xItem = new(PolishBSideArm2Address.PolishCPosition_2x, nameof(PolishCPosition_2x), nameof(IsPolishCPosition_2xDirty));
        private readonly EditableFloatItem _polishDPosition2xItem = new(PolishBSideArm2Address.PolishDPosition_2x, nameof(PolishDPosition_2x), nameof(IsPolishDPosition_2xDirty));
        private readonly EditableFloatItem _jogSpeed2xItem = new(PolishBSideArm2Address.JogSpeed_2x, nameof(JogSpeed_2x), nameof(IsJogSpeed_2xDirty));
        private readonly EditableFloatItem _autoSpeed2xItem = new(PolishBSideArm2Address.AutoSpeed_2x, nameof(AutoSpeed_2x), nameof(IsAutoSpeed_2xDirty));

        private readonly EditableFloatItem _standbyPosition2zItem = new(PolishBSideArm2Address.StandbyPosition_2z, nameof(StandbyPosition_2z), nameof(IsStandbyPosition_2zDirty));
        private readonly EditableFloatItem _transferArmPosition2zItem = new(PolishBSideArm2Address.TransferArmPosition_2z, nameof(TransferArmPosition_2z), nameof(IsTransferArmPosition_2zDirty));
        private readonly EditableFloatItem _polishCDownPieceItem = new(PolishBSideArm2Address.PolishCDownPiece, nameof(PolishCDownPiece), nameof(IsPolishCDownPieceDirty));
        private readonly EditableFloatItem _polishCUpPieceItem = new(PolishBSideArm2Address.PolishCUpPiece, nameof(PolishCUpPiece), nameof(IsPolishCUpPieceDirty));
        private readonly EditableFloatItem _polishDDownPieceItem = new(PolishBSideArm2Address.PolishDDownPiece, nameof(PolishDDownPiece), nameof(IsPolishDDownPieceDirty));
        private readonly EditableFloatItem _polishDUpPieceItem = new(PolishBSideArm2Address.PolishDUpPiece, nameof(PolishDUpPiece), nameof(IsPolishDUpPieceDirty));
        private readonly EditableFloatItem _jogSpeed2zItem = new(PolishBSideArm2Address.JogSpeed_2z, nameof(JogSpeed_2z), nameof(IsJogSpeed_2zDirty));
        private readonly EditableFloatItem _autoSpeed2zItem = new(PolishBSideArm2Address.AutoSpeed_2z, nameof(AutoSpeed_2z), nameof(IsAutoSpeed_2zDirty));

        private readonly List<EditableFloatItem> _editableItems;

        /// <summary>
        /// 杞翠綅缃渶灏忓€?
        /// </summary>
        public double PositionMinimum { get; set; } = -90000;

        /// <summary>
        /// 杞翠綅缃渶澶у€?
        /// </summary>
        public double PositionMaximum { get; set; } = 90000;

        #region AxisPositionOfSideBArm-2x 灞炴€?

        #region 杞翠綅缃拰灞炴€?

        public bool IsStandbyPosition_2xDirty => _standbyPosition2xItem.IsDirty;
        /// <summary>
        /// 寰呮満浣?
        /// </summary>
        public double StandbyPosition_2x
        {
            get => _standbyPosition2xItem.Value;
            set => EditableFloatItemHelper.SetValue(_standbyPosition2xItem, value, RaisePropertyChanged);
        }

        public bool IsTransferArmPosition_2xDirty => _transferArmPosition2xItem.IsDirty;
        /// <summary>
        /// 涓浆鎵嬭噦浣?
        /// </summary>
        public double TransferArmPosition_2x
        {
            get => _transferArmPosition2xItem.Value;
            set => EditableFloatItemHelper.SetValue(_transferArmPosition2xItem, value, RaisePropertyChanged);
        }

        public bool IsPolishCPosition_2xDirty => _polishCPosition2xItem.IsDirty;
        /// <summary>
        /// PolishC浣嶇疆 X1鏈烘鑷?
        /// </summary>
        public double PolishCPosition_2x
        {
            get => _polishCPosition2xItem.Value;
            set => EditableFloatItemHelper.SetValue(_polishCPosition2xItem, value, RaisePropertyChanged);
        }

        public bool IsPolishDPosition_2xDirty => _polishDPosition2xItem.IsDirty;
        /// <summary>
        /// PolishD浣嶇疆 X1鏈烘鑷?
        /// </summary>
        public double PolishDPosition_2x
        {
            get => _polishDPosition2xItem.Value;
            set => EditableFloatItemHelper.SetValue(_polishDPosition2xItem, value, RaisePropertyChanged);
        }
        #endregion

        #region Move Toggle灞炴€?
        private bool _standbyPosMoveToggle_2x;
        /// <summary>
        /// 寰呮満浣嶇Щ鍔?
        /// </summary>
        public bool StandbyPosMoveToggle_2x
        {
            get => _standbyPosMoveToggle_2x;
            set => SetProperty(ref _standbyPosMoveToggle_2x, value);
        }

        private bool _transferArmMoveToggle_2x;
        /// <summary>
        /// 涓浆鑷傜Щ鍔?
        /// </summary>
        public bool TransferArmMoveToggle_2x
        {
            get => _transferArmMoveToggle_2x;
            set => SetProperty(ref _transferArmMoveToggle_2x, value);
        }

        private bool _polishCPosMoveToggle_2x;
        /// <summary>
        /// 鐮旂（C浣?
        /// </summary>
        public bool PolishCPosMoveToggle_2x
        {
            get => _polishCPosMoveToggle_2x;
            set => SetProperty(ref _polishCPosMoveToggle_2x, value);
        }

        private bool _polishDPosMoveToggle_2x;
        /// <summary>
        /// 鐮旂（D浣?
        /// </summary>
        public bool PolishDPosMoveToggle_2x
        {
            get => _polishDPosMoveToggle_2x;
            set => SetProperty(ref _polishDPosMoveToggle_2x, value);
        }
        #endregion

        #region 閫熷害鍜屽潗鏍囧睘鎬?

        public bool IsJogSpeed_2xDirty => _jogSpeed2xItem.IsDirty;
        public double JogSpeed_2x
        {
            get => _jogSpeed2xItem.Value;
            set => EditableFloatItemHelper.SetValue(_jogSpeed2xItem, value, RaisePropertyChanged);
        }

        public bool IsAutoSpeed_2xDirty => _autoSpeed2xItem.IsDirty;
        public double AutoSpeed_2x
        {
            get => _autoSpeed2xItem.Value;
            set => EditableFloatItemHelper.SetValue(_autoSpeed2xItem, value, RaisePropertyChanged);
        }

        private double _currentCoordinate_2x;
        public double CurrentCoordinate_2x
        {
            get => _currentCoordinate_2x;
            set => SetProperty(ref _currentCoordinate_2x, value);
        }

        #endregion

        #region 鐘舵€佸睘鎬?

        private bool _isEnabled_2x;
        public bool IsEnabled_2x
        {
            get => _isEnabled_2x;
            set => SetProperty(ref _isEnabled_2x, value);
        }

        private bool _driverAlarm_2x;
        public bool DriverAlarm_2x
        {
            get => _driverAlarm_2x;
            set => SetProperty(ref _driverAlarm_2x, value);
        }

        private bool _limitAlarm_2x;
        public bool LimitAlarm_2x
        {
            get => _limitAlarm_2x;
            set => SetProperty(ref _limitAlarm_2x, value);
        }

        private bool _isAtOrigin_2x;
        public bool IsAtOrigin_2x
        {
            get => _isAtOrigin_2x;
            set => SetProperty(ref _isAtOrigin_2x, value);
        }

        private int _alarmCode_2x;
        public int AlarmCode_2x
        {
            get => _alarmCode_2x;
            set => SetProperty(ref _alarmCode_2x, value);
        }

        #endregion

        #region Jog 鎺у埗灞炴€?

        private bool _jogUpActive_2x;
        public bool JogUpActive_2x
        {
            get => _jogUpActive_2x;
            set => SetProperty(ref _jogUpActive_2x, value);
        }

        private bool _jogMiddleActive_2x;
        public bool JogMiddleActive_2x
        {
            get => _jogMiddleActive_2x;
            set => SetProperty(ref _jogMiddleActive_2x, value);
        }

        private bool _jogDownActive_2x;
        public bool JogDownActive_2x
        {
            get => _jogDownActive_2x;
            set => SetProperty(ref _jogDownActive_2x, value);
        }

        private bool _originToggle_2x;
        public bool OriginToggle_2x
        {
            get => _originToggle_2x;
            set => SetProperty(ref _originToggle_2x, value);
        }
        #endregion

        #endregion

        #region AxisPositionOfSideBArm-2z 灞炴€?

        #region 杞翠綅缃拰灞炴€?

        public bool IsStandbyPosition_2zDirty => _standbyPosition2zItem.IsDirty;
        /// <summary>
        /// 寰呮満浣?
        /// </summary>
        public double StandbyPosition_2z
        {
            get => _standbyPosition2zItem.Value;
            set => EditableFloatItemHelper.SetValue(_standbyPosition2zItem, value, RaisePropertyChanged);
        }

        public bool IsTransferArmPosition_2zDirty => _transferArmPosition2zItem.IsDirty;
        /// <summary>
        /// 涓浆鎵嬭噦浣?
        /// </summary>
        public double TransferArmPosition_2z
        {
            get => _transferArmPosition2zItem.Value;
            set => EditableFloatItemHelper.SetValue(_transferArmPosition2zItem, value, RaisePropertyChanged);
        }

        public bool IsPolishCDownPieceDirty => _polishCDownPieceItem.IsDirty;
        /// <summary>
        /// PolishC涓婄墖
        /// </summary>
        public double PolishCDownPiece
        {
            get => _polishCDownPieceItem.Value;
            set => EditableFloatItemHelper.SetValue(_polishCDownPieceItem, value, RaisePropertyChanged);
        }

        public bool IsPolishCUpPieceDirty => _polishCUpPieceItem.IsDirty;
        /// <summary>
        /// PolishC涓嬬墖
        /// </summary>
        public double PolishCUpPiece
        {
            get => _polishCUpPieceItem.Value;
            set => EditableFloatItemHelper.SetValue(_polishCUpPieceItem, value, RaisePropertyChanged);
        }

        public bool IsPolishDDownPieceDirty => _polishDDownPieceItem.IsDirty;
        /// <summary>
        /// PolishD涓婄墖
        /// </summary>
        public double PolishDDownPiece
        {
            get => _polishDDownPieceItem.Value;
            set => EditableFloatItemHelper.SetValue(_polishDDownPieceItem, value, RaisePropertyChanged);
        }

        public bool IsPolishDUpPieceDirty => _polishDUpPieceItem.IsDirty;
        /// <summary>
        /// PolishD涓婄墖
        /// </summary>
        public double PolishDUpPiece
        {
            get => _polishDUpPieceItem.Value;
            set => EditableFloatItemHelper.SetValue(_polishDUpPieceItem, value, RaisePropertyChanged);
        }
        #endregion

        #region Move Toggle灞炴€?
        private bool _standbyPosMoveToggle_2z;
        /// <summary>
        /// 寰呮満浣嶇Щ鍔?
        /// </summary>
        public bool StandbyPosMoveToggle_2z
        {
            get => _standbyPosMoveToggle_2z;
            set => SetProperty(ref _standbyPosMoveToggle_2z, value);
        }

        private bool _transferArmMoveToggle_2z;
        /// <summary>
        /// 涓浆鑷傜Щ鍔?
        /// </summary>
        public bool TransferArmMoveToggle_2z
        {
            get => _transferArmMoveToggle_2z;
            set => SetProperty(ref _transferArmMoveToggle_2z, value);
        }

        private bool _polishCDownPieceMove_2z;
        /// <summary>
        /// 鐮旂（C涓嬬墖 绉诲姩
        /// </summary>
        public bool PolishCDownPieceMove_2z
        {
            get => _polishCDownPieceMove_2z;
            set => SetProperty(ref _polishCDownPieceMove_2z, value);
        }

        private bool _polishCUpPieceMove_2z;
        /// <summary>
        /// 鐮旂（C涓婄墖 绉诲姩
        /// </summary>
        public bool PolishCUpPieceMove_2z
        {
            get => _polishCUpPieceMove_2z;
            set => SetProperty(ref _polishCUpPieceMove_2z, value);
        }

        private bool _polishDDownPieceMove_2z;
        /// <summary>
        /// 鐮旂（D涓嬬墖 绉诲姩
        /// </summary>
        public bool PolishDDownPieceMove_2z
        {
            get => _polishDDownPieceMove_2z;
            set => SetProperty(ref _polishDDownPieceMove_2z, value);
        }

        private bool _polishDUpPieceMove_2z;
        /// <summary>
        /// 鐮旂（D涓婄墖 绉诲姩
        /// </summary>
        public bool PolishDUpPieceMove_2z
        {
            get => _polishDUpPieceMove_2z;
            set => SetProperty(ref _polishDUpPieceMove_2z, value);
        }
        #endregion

        #region 閫熷害鍜屽潗鏍囧睘鎬?

        public bool IsJogSpeed_2zDirty => _jogSpeed2zItem.IsDirty;
        public double JogSpeed_2z
        {
            get => _jogSpeed2zItem.Value;
            set => EditableFloatItemHelper.SetValue(_jogSpeed2zItem, value, RaisePropertyChanged);
        }

        public bool IsAutoSpeed_2zDirty => _autoSpeed2zItem.IsDirty;
        public double AutoSpeed_2z
        {
            get => _autoSpeed2zItem.Value;
            set => EditableFloatItemHelper.SetValue(_autoSpeed2zItem, value, RaisePropertyChanged);
        }

        private double _currentCoordinate_2z;
        public double CurrentCoordinate_2z
        {
            get => _currentCoordinate_2z;
            set => SetProperty(ref _currentCoordinate_2z, value);
        }

        #endregion

        #region 鐘舵€佸睘鎬?

        private bool _isEnabled_2z;
        public bool IsEnabled_2z
        {
            get => _isEnabled_2z;
            set => SetProperty(ref _isEnabled_2z, value);
        }

        private bool _driverAlarm_2z;
        public bool DriverAlarm_2z
        {
            get => _driverAlarm_2z;
            set => SetProperty(ref _driverAlarm_2z, value);
        }

        private bool _limitAlarm_2z;
        public bool LimitAlarm_2z
        {
            get => _limitAlarm_2z;
            set => SetProperty(ref _limitAlarm_2z, value);
        }

        private bool _isAtOrigin_2z;
        public bool IsAtOrigin_2z
        {
            get => _isAtOrigin_2z;
            set => SetProperty(ref _isAtOrigin_2z, value);
        }

        private int _alarmCode_2z;
        public int AlarmCode_2z
        {
            get => _alarmCode_2z;
            set => SetProperty(ref _alarmCode_2z, value);
        }

        #endregion

        #region Jog 鎺у埗灞炴€?

        private bool _jogUpActive_2z;
        public bool JogUpActive_2z
        {
            get => _jogUpActive_2z;
            set => SetProperty(ref _jogUpActive_2z, value);
        }

        private bool _jogMiddleActive_2z;
        public bool JogMiddleActive_2z
        {
            get => _jogMiddleActive_2z;
            set => SetProperty(ref _jogMiddleActive_2z, value);
        }

        private bool _jogDownActive_2z;
        public bool JogDownActive_2z
        {
            get => _jogDownActive_2z;
            set => SetProperty(ref _jogDownActive_2z, value);
        }

        private bool _originToggle_2z;
        public bool OriginToggle_2z
        {
            get => _originToggle_2z;
            set => SetProperty(ref _originToggle_2z, value);
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

        private bool _pickFromTransferActive;
        public bool PickFromTransferActive { get => _pickFromTransferActive; set => SetProperty(ref _pickFromTransferActive, value); }
        private bool _pickFromTransferToggle;
        public bool PickFromTransferToggle { get => _pickFromTransferToggle; set => SetProperty(ref _pickFromTransferToggle, value); }
        private bool _pickFromTransferDone;
        public bool PickFromTransferDone { get => _pickFromTransferDone; set => SetProperty(ref _pickFromTransferDone, value); }

        private bool _pickFromPolishCActive;
        public bool PickFromPolishCActive { get => _pickFromPolishCActive; set => SetProperty(ref _pickFromPolishCActive, value); }
        private bool _pickFromPolishCToggle;
        public bool PickFromPolishCToggle { get => _pickFromPolishCToggle; set => SetProperty(ref _pickFromPolishCToggle, value); }
        private bool _pickFromPolishCDone;
        public bool PickFromPolishCDone { get => _pickFromPolishCDone; set => SetProperty(ref _pickFromPolishCDone, value); }

        private bool _placeToPolishDActive;
        public bool PlaceToPolishDActive { get => _placeToPolishDActive; set => SetProperty(ref _placeToPolishDActive, value); }
        private bool _placeToPolishDToggle;
        public bool PlaceToPolishDToggle { get => _placeToPolishDToggle; set => SetProperty(ref _placeToPolishDToggle, value); }
        private bool _placeToPolishDDone;
        public bool PlaceToPolishDDone { get => _placeToPolishDDone; set => SetProperty(ref _placeToPolishDDone, value); }

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

        private bool _manualModeToggle;
        public bool ManualModeToggle { get => _manualModeToggle; set => SetProperty(ref _manualModeToggle, value); }

        private bool _autoModeToggle;
        public bool AutoModeToggle { get => _autoModeToggle; set => SetProperty(ref _autoModeToggle, value); }

        private bool _arm2Fiber1;
        public bool Arm2Fiber1 { get => _arm2Fiber1; set => SetProperty(ref _arm2Fiber1, value); }

        private bool _arm2Fiber2;
        public bool Arm2Fiber2 { get => _arm2Fiber2; set => SetProperty(ref _arm2Fiber2, value); }

        private bool _arm2Fiber3;
        public bool Arm2Fiber3 { get => _arm2Fiber3; set => SetProperty(ref _arm2Fiber3, value); }

        #endregion

        #region Commands
        public DelegateCommand StationOperationCommand { get; }

        public DelegateCommand StandbyMoveX2Command { get; }
        public DelegateCommand TransferMoveX2Command { get; }
        public DelegateCommand PolishCMoveX2Command { get; }
        public DelegateCommand PolishDMoveX2Command { get; }

        public DelegateCommand StandbyMoveZ2Command { get; }
        public DelegateCommand TransferMoveZ2Command { get; }
        public DelegateCommand PolishCDownPieceMoveZ2Command { get; }
        public DelegateCommand PolishCUpPieceMoveZ2Command { get; }
        public DelegateCommand PolishDDownPieceMoveZ2Command { get; }
        public DelegateCommand PolishDUpPieceMoveZ2Command { get; }

        public DelegateCommand EnableCommandX2Command { get; }
        public DelegateCommand EnableCommandZ2Command { get; }

        public DelegateCommand PressJogUpX2Command { get; }
        public DelegateCommand ReleaseJogUpX2Command { get; }
        public DelegateCommand PressJogDownX2Command { get; }
        public DelegateCommand ReleaseJogDownX2Command { get; }
        public DelegateCommand PressJogUpZ2Command { get; }
        public DelegateCommand ReleaseJogUpZ2Command { get; }
        public DelegateCommand PressJogDownZ2Command { get; }
        public DelegateCommand ReleaseJogDownZ2Command { get; }

        public DelegateCommand OrignX2Command { get; }
        public DelegateCommand OrignZ2Command { get; }

        public DelegateCommand EmergencyCommand { get; }
        public DelegateCommand AlarmResetCommand { get; }
        public DelegateCommand SaveCommand { get; }

        public DelegateCommand BackCommand { get; }
        public DelegateCommand StationResetCommand { get; }
        public DelegateCommand StationStartCommand { get; }
        public DelegateCommand StationStopCommand { get; }
        public DelegateCommand StationAlarmResetCommand { get; }

        public DelegateCommand PickFromTransferCommand { get; }
        public DelegateCommand PickFromPolishCCommand { get; }
        public DelegateCommand PlaceToPolishDCommand { get; }
        public DelegateCommand PlaceToTransferCommand { get; }
        public DelegateCommand Spare1Command { get; }

        public DelegateCommand ManualModeCommand { get; }
        public DelegateCommand AutoModeCommand { get; }

        public DelegateCommand StandbyPosition_2xSetCommand => _standbyPosition2xItem.SetCommand;
        public DelegateCommand StandbyPosition_2xCancelCommand => _standbyPosition2xItem.CancelCommand;
        public DelegateCommand TransferArmPosition_2xSetCommand => _transferArmPosition2xItem.SetCommand;
        public DelegateCommand TransferArmPosition_2xCancelCommand => _transferArmPosition2xItem.CancelCommand;
        public DelegateCommand PolishCPosition_2xSetCommand => _polishCPosition2xItem.SetCommand;
        public DelegateCommand PolishCPosition_2xCancelCommand => _polishCPosition2xItem.CancelCommand;
        public DelegateCommand PolishDPosition_2xSetCommand => _polishDPosition2xItem.SetCommand;
        public DelegateCommand PolishDPosition_2xCancelCommand => _polishDPosition2xItem.CancelCommand;
        public DelegateCommand JogSpeed_2xSetCommand => _jogSpeed2xItem.SetCommand;
        public DelegateCommand JogSpeed_2xCancelCommand => _jogSpeed2xItem.CancelCommand;
        public DelegateCommand AutoSpeed_2xSetCommand => _autoSpeed2xItem.SetCommand;
        public DelegateCommand AutoSpeed_2xCancelCommand => _autoSpeed2xItem.CancelCommand;

        public DelegateCommand StandbyPosition_2zSetCommand => _standbyPosition2zItem.SetCommand;
        public DelegateCommand StandbyPosition_2zCancelCommand => _standbyPosition2zItem.CancelCommand;
        public DelegateCommand TransferArmPosition_2zSetCommand => _transferArmPosition2zItem.SetCommand;
        public DelegateCommand TransferArmPosition_2zCancelCommand => _transferArmPosition2zItem.CancelCommand;
        public DelegateCommand PolishCDownPieceSetCommand => _polishCDownPieceItem.SetCommand;
        public DelegateCommand PolishCDownPieceCancelCommand => _polishCDownPieceItem.CancelCommand;
        public DelegateCommand PolishCUpPieceSetCommand => _polishCUpPieceItem.SetCommand;
        public DelegateCommand PolishCUpPieceCancelCommand => _polishCUpPieceItem.CancelCommand;
        public DelegateCommand PolishDDownPieceSetCommand => _polishDDownPieceItem.SetCommand;
        public DelegateCommand PolishDDownPieceCancelCommand => _polishDDownPieceItem.CancelCommand;
        public DelegateCommand PolishDUpPieceSetCommand => _polishDUpPieceItem.SetCommand;
        public DelegateCommand PolishDUpPieceCancelCommand => _polishDUpPieceItem.CancelCommand;
        public DelegateCommand JogSpeed_2zSetCommand => _jogSpeed2zItem.SetCommand;
        public DelegateCommand JogSpeed_2zCancelCommand => _jogSpeed2zItem.CancelCommand;
        public DelegateCommand AutoSpeed_2zSetCommand => _autoSpeed2zItem.SetCommand;
        public DelegateCommand AutoSpeed_2zCancelCommand => _autoSpeed2zItem.CancelCommand;
        #endregion

        public PolishBSideArm2ViewModel() : base(9, "Polish B Side Arm2")
        {
            _editableItems = new List<EditableFloatItem>
            {
                _standbyPosition2xItem,
                _transferArmPosition2xItem,
                _polishCPosition2xItem,
                _polishDPosition2xItem,
                _jogSpeed2xItem,
                _autoSpeed2xItem,
                _standbyPosition2zItem,
                _transferArmPosition2zItem,
                _polishCDownPieceItem,
                _polishCUpPieceItem,
                _polishDDownPieceItem,
                _polishDUpPieceItem,
                _jogSpeed2zItem,
                _autoSpeed2zItem
            };
            EditableFloatItemHelper.InitializeCommands(_editableItems, _plc, RaisePropertyChanged);

            _readTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            _readTimer.Tick += ReadTimer_Tick;
            RegisterLifecycleTimer(_readTimer);
            RegisterViewLifecycleAction(
                () => Logger.For("RobotControl").Debug("PolishBSideArm2ViewModel activated, started PLC read timer."),
                () => Logger.For("RobotControl").Debug("PolishBSideArm2ViewModel deactivated, stopped PLC read timer."));

            StationOperationCommand = new DelegateCommand(() => CurrentViewIndex = 1);

            StandbyMoveX2Command = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.StandbyPosMove_2x, !StandbyPosMoveToggle_2x));
            TransferMoveX2Command = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.TransferArmMove_2x, !TransferArmMoveToggle_2x));
            PolishCMoveX2Command = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.PolishCMove_2x, !PolishCPosMoveToggle_2x));
            PolishDMoveX2Command = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.PolishDMove_2x, !PolishDPosMoveToggle_2x));

            StandbyMoveZ2Command = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.StandbyPosMove_2z, !StandbyPosMoveToggle_2z));
            TransferMoveZ2Command = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.TransferArmMove_2z, !TransferArmMoveToggle_2z));
            PolishCDownPieceMoveZ2Command = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.PolishCDownPieceMove, !PolishCDownPieceMove_2z));
            PolishCUpPieceMoveZ2Command = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.PolishCUpPieceMove, !PolishCUpPieceMove_2z));
            PolishDDownPieceMoveZ2Command = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.PolishDDownPieceMove, !PolishDDownPieceMove_2z));
            PolishDUpPieceMoveZ2Command = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.PolishDUpPieceMove, !PolishDUpPieceMove_2z));

            EnableCommandX2Command = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.Enable_2x, !IsEnabled_2x));
            EnableCommandZ2Command = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.Enable_2z, !IsEnabled_2z));

            PressJogUpX2Command = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.JogUp_2x, true));
            ReleaseJogUpX2Command = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.JogUp_2x, false));
            PressJogDownX2Command = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.JogDown_2x, true));
            ReleaseJogDownX2Command = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.JogDown_2x, false));

            PressJogUpZ2Command = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.JogUp_2z, true));
            ReleaseJogUpZ2Command = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.JogUp_2z, false));
            PressJogDownZ2Command = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.JogDown_2z, true));
            ReleaseJogDownZ2Command = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.JogDown_2z, false));

            OrignX2Command = new DelegateCommand(() =>
            {
                if (MessageBox.Show(Lang.Resources.ConfirmActionContent, Lang.Resources.ConfirmActionTips,
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _plc.Write(PolishBSideArm2Address.Origin_2x, !OriginToggle_2x);
                }

                OriginToggle_2x = false;
            });
            OrignZ2Command = new DelegateCommand(() =>
            {
                if (MessageBox.Show(Lang.Resources.ConfirmActionContent, Lang.Resources.ConfirmActionTips,
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _plc.Write(PolishBSideArm2Address.Origin_2z, !OriginToggle_2z);
                }

                OriginToggle_2z = false;
            });

            EmergencyCommand = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.EmergencyStop, !EmergencyStopToggle));
            AlarmResetCommand = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.AlarmReset, !AlarmResetToggle));
            SaveCommand = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.Save, !SaveToggle));

            BackCommand = new DelegateCommand(() => CurrentViewIndex = 0);
            StationResetCommand = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.StationReset, true));
            StationStartCommand = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.StationStart, true));
            StationStopCommand = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.StationStop, true));
            StationAlarmResetCommand = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.StationAlarmReset, true));

            PickFromTransferCommand = new DelegateCommand(() =>
            {
                _plc.Write(PolishBSideArm2Address.PickFromTransfer, true);
                Thread.Sleep(50);
                _plc.Write(PolishBSideArm2Address.PickFromTransfer, false);
            });
            PickFromPolishCCommand = new DelegateCommand(() =>
            {
                _plc.Write(PolishBSideArm2Address.PickFromPolishC, true);
                Thread.Sleep(50);
                _plc.Write(PolishBSideArm2Address.PickFromPolishC, false);
            });
            PlaceToPolishDCommand = new DelegateCommand(() =>
            {
                _plc.Write(PolishBSideArm2Address.PlaceToPolishD, true);
                Thread.Sleep(50);
                _plc.Write(PolishBSideArm2Address.PlaceToPolishD, false);
            });
            PlaceToTransferCommand = new DelegateCommand(() =>
            {
                _plc.Write(PolishBSideArm2Address.PlaceToTransfer, true);
                Thread.Sleep(50);
                _plc.Write(PolishBSideArm2Address.PlaceToTransfer, false);
            });
            Spare1Command = new DelegateCommand(() =>
            {
                _plc.Write(PolishBSideArm2Address.Spare1, true);
                Thread.Sleep(50);
                _plc.Write(PolishBSideArm2Address.Spare1, false);
            });

            ManualModeCommand = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.ManualMode, !ManualModeToggle));
            AutoModeCommand = new DelegateCommand(() => _plc.Write(PolishBSideArm2Address.AutoMode, !AutoModeToggle));
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
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.GlobalAlarm, assign: v => GlobalAlarm = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.Save, assign: v => SaveToggle = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.EmergencyStop, assign: v => EmergencyStopToggle = v),

                new PlcBatchReadItem<bool>(PolishBSideArm2Address.Origin_2x, assign: v => OriginToggle_2x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.StandbyPosMove_2x, assign: v => StandbyPosMoveToggle_2x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.TransferArmMove_2x, assign: v => TransferArmMoveToggle_2x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.PolishCMove_2x, assign: v => PolishCPosMoveToggle_2x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.PolishDMove_2x, assign: v => PolishDPosMoveToggle_2x = v),

                new PlcBatchReadItem<bool>(PolishBSideArm2Address.Origin_2z, assign: v => OriginToggle_2z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.StandbyPosMove_2z, assign: v => StandbyPosMoveToggle_2z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.TransferArmMove_2z, assign: v => TransferArmMoveToggle_2z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.PolishCDownPieceMove, assign: v => PolishCDownPieceMove_2z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.PolishCUpPieceMove, assign: v => PolishCUpPieceMove_2z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.PolishDDownPieceMove, assign: v => PolishDDownPieceMove_2z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.PolishDUpPieceMove, assign: v => PolishDUpPieceMove_2z = v),

                new PlcBatchReadItem<bool>(PolishBSideArm2Address.AutoMode, assign: v => AutoModeToggle = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.ManualMode, assign: v => ManualModeToggle = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.PickFromTransfer, assign: v => PickFromTransferToggle = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.PickFromPolishC, assign: v => PickFromPolishCToggle = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.PlaceToPolishD, assign: v => PlaceToPolishDToggle = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.PlaceToTransfer, assign: v => PlaceToTransferToggle = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.Spare1, assign: v => Spare1Toggle = v),

                new PlcBatchReadItem<bool>(PolishBSideArm2Address.EnableStatus_2x, assign: v => IsEnabled_2x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.OriginStatus_2x, assign: v => IsAtOrigin_2x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.JogUpStatus_2x, assign: v => JogUpActive_2x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.JogDownStatus_2x, assign: v => JogDownActive_2x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.JogMiddleStatus_2x, assign: v => JogMiddleActive_2x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.LimitAlarm_2x, assign: v => LimitAlarm_2x = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.DriverAlarm_2x, assign: v => DriverAlarm_2x = v),

                new PlcBatchReadItem<bool>(PolishBSideArm2Address.EnableStatus_2z, assign: v => IsEnabled_2z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.OriginStatus_2z, assign: v => IsAtOrigin_2z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.JogUpStatus_2z, assign: v => JogUpActive_2z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.JogDownStatus_2z, assign: v => JogDownActive_2z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.JogMiddleStatus_2z, assign: v => JogMiddleActive_2z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.LimitAlarm_2z, assign: v => LimitAlarm_2z = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.DriverAlarm_2z, assign: v => DriverAlarm_2z = v),

                new PlcBatchReadItem<bool>(PolishBSideArm2Address.StationResetStatus, assign: v => StationIsReset = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.StationRunningStatus, assign: v => StationIsRunning = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.StationStoppedStatus, assign: v => StationIsStopped = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.ModuleAlarm, assign: v => ModuleAlarm = v),

                new PlcBatchReadItem<bool>(PolishBSideArm2Address.PickFromTransferActiveStatus, assign: v => PickFromTransferActive = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.PickFromPolishCActiveStatus, assign: v => PickFromPolishCActive = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.PlaceToPolishDActiveStatus, assign: v => PlaceToPolishDActive = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.PlaceToTransferActiveStatus, assign: v => PlaceToTransferActive = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.Spare1ActiveStatus, assign: v => Spare1Active = v),

                new PlcBatchReadItem<bool>(PolishBSideArm2Address.PickFromTransferDoneStatus, assign: v => PickFromTransferDone = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.PickFromPolishCDoneStatus, assign: v => PickFromPolishCDone = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.PlaceToPolishDDoneStatus, assign: v => PlaceToPolishDDone = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.PlaceToTransferDoneStatus, assign: v => PlaceToTransferDone = v),
                new PlcBatchReadItem<bool>(PolishBSideArm2Address.Spare1DoneStatus, assign: v => Spare1Done = v)
            };

            var dStatusBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<int>(PolishBSideArm2Address.CurrentCoordinate_2x, assign: v => CurrentCoordinate_2x = v / 1000),
                new PlcBatchReadItem<int>(PolishBSideArm2Address.AlarmCode_2x, assign: v => AlarmCode_2x = v),
                new PlcBatchReadItem<int>(PolishBSideArm2Address.CurrentCoordinate_2z, assign: v => CurrentCoordinate_2z = v / 1000),
                new PlcBatchReadItem<int>(PolishBSideArm2Address.AlarmCode_2z, assign: v => AlarmCode_2z = v)
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
            var x1023Task = _plc.ReadAsync<bool[]>(PolishBSideArm2Address.Arm2Fiber1, 3);
            var b0bTask = _plc.ReadAsync<bool>(PolishBSideArm2Address.AlarmReset);

            await Task.WhenAll(mBatchTask, dStatusBatchTask, editableBatchTask, x1023Task, b0bTask);

            if (!mBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[PolishBSideArm2] M batch read failed: {mBatchTask.Result.Message}");

            if (!dStatusBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[PolishBSideArm2] D status batch read failed: {dStatusBatchTask.Result.Message}");

            if (!editableBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[PolishBSideArm2] D editable batch read failed: {editableBatchTask.Result.Message}");

            if (x1023Task.Result.IsSuccess)
            {
                var x = x1023Task.Result.Content;
                if (x.Length >= 3)
                {
                    Arm2Fiber1 = x[0];
                    Arm2Fiber2 = x[1];
                    Arm2Fiber3 = x[2];
                }
            }
            else
            {
                Logger.For("RobotControl").Warn($"[PolishBSideArm2] X1023-X1025 read failed: {x1023Task.Result.Message}");
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
