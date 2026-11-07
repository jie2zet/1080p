using CMP.Constants.Plc.RobotControl;
using CMP.Services.Log;
using CMP.Services.Plc;
using CMP.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;
using System.Windows.Threading;
using CMP.Services.Auth;

namespace CMP.ViewModels.Tabs.OperationTab.RobotControlTab.CleanRobot
{
    public class CleanASideRobot1ViewModel : StationOpViewModelBase
    {
        private bool _isReading;
        private readonly EditableFloatItem _rollerA1orA2PositionItem = new(CleanRobotA1Address.RollerA1orA2, nameof(RollerA1orA2Position), nameof(IsRollerA1orA2PositionDirty));
        private readonly EditableFloatItem _rollerA3PositionItem = new(CleanRobotA1Address.RollerA3, nameof(RollerA3Position), nameof(IsRollerA3PositionDirty));
        private readonly EditableFloatItem _penA1PositionItem = new(CleanRobotA1Address.PenA1, nameof(PenA1Position), nameof(IsPenA1PositionDirty));
        private readonly EditableFloatItem _jogSpeedItem = new(CleanRobotA1Address.JogSpeed, nameof(JogSpeed), nameof(IsJogSpeedDirty));
        private readonly EditableFloatItem _autoSpeedItem = new(CleanRobotA1Address.AutoSpeed, nameof(AutoSpeed), nameof(IsAutoSpeedDirty));
        private readonly List<EditableFloatItem> _editableItems;


        /// <summary>
        /// 轴位置最小值
        /// </summary>
        public double PositionMinimum { get; set; } = -90000;

        /// <summary>
        /// 轴位置最大值
        /// </summary>
        public double PositionMaximum { get; set; } = 90000;

        #region AxisPositionSetting 属性

        #region 轴位置和属性
        public bool IsRollerA1orA2PositionDirty => _rollerA1orA2PositionItem.IsDirty;

        /// <summary>
        /// RollerA1orA2
        /// </summary>
        public double RollerA1orA2Position
        {
            get => _rollerA1orA2PositionItem.Value;
            set => EditableFloatItemHelper.SetValue(_rollerA1orA2PositionItem, value, RaisePropertyChanged);
        }

        public bool IsRollerA3PositionDirty => _rollerA3PositionItem.IsDirty;

        /// <summary>
        /// RollerA3
        /// </summary>
        public double RollerA3Position
        {
            get => _rollerA3PositionItem.Value;
            set => EditableFloatItemHelper.SetValue(_rollerA3PositionItem, value, RaisePropertyChanged);
        }

        public bool IsPenA1PositionDirty => _penA1PositionItem.IsDirty;

        /// <summary>
        /// PenA1
        /// </summary>
        public double PenA1Position
        {
            get => _penA1PositionItem.Value;
            set => EditableFloatItemHelper.SetValue(_penA1PositionItem, value, RaisePropertyChanged);
        }
        #endregion

        #region Move Toggle属性
        private bool _rollerA1orA2MoveToggle;
        /// <summary>
        /// RollerA1orA2按钮
        /// </summary>
        public bool RollerA1orA2MoveToggle
        {
            get => _rollerA1orA2MoveToggle;
            set => SetProperty(ref _rollerA1orA2MoveToggle, value);
        }
        private bool _rollerA3MoveToggle;
        /// <summary>
        /// RollerA3按钮
        /// </summary>
        public bool RollerA3MoveToggle
        {
            get => _rollerA3MoveToggle;
            set => SetProperty(ref _rollerA3MoveToggle, value);
        }
        private bool _penA1MoveToggle;
        /// <summary>
        /// PenA1按钮
        /// </summary>
        public bool PenA1MoveToggle
        {
            get => _penA1MoveToggle;
            set => SetProperty(ref _penA1MoveToggle, value);
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

        private int _alarmCode;
        public int AlarmCode
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

        #endregion

        #region RobotControl 属性

        #region 程序区

        private int _programNumber;
        /// <summary>
        /// 程序编号
        /// </summary>
        public int ProgramNumber
        {
            get => _programNumber;
            set => SetProperty(ref _programNumber, value);
        }

        private bool _isRunning;
        /// <summary>
        /// 启动
        /// </summary>
        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        private bool _isPaused;
        /// <summary>
        /// 暂停
        /// </summary>
        public bool IsPaused
        {
            get => _isPaused;
            set => SetProperty(ref _isPaused, value);
        }

        private bool _pauseToggle;
        /// <summary>
        /// 暂停
        /// </summary>
        public bool PauseToggle
        {
            get => _pauseToggle;
            set => SetProperty(ref _pauseToggle, value);
        }

        private bool _isStopped;
        /// <summary>
        /// 停止
        /// </summary>
        public bool IsStopped
        {
            get => _isStopped;
            set => SetProperty(ref _isStopped, value);
        }

        private bool _isReset;
        /// <summary>
        /// 复位
        /// </summary>
        public bool IsReset
        {
            get => _isReset;
            set => SetProperty(ref _isReset, value);
        }
        #endregion

        #region 工作区

        private int _workAreaNumber;
        /// <summary>
        /// 工作区编号
        /// </summary>
        public int WorkAreaNumber
        {
            get => _workAreaNumber;
            set => SetProperty(ref _workAreaNumber, value);
        }

        private bool _workAreaPickupToggle;
        /// <summary>
        /// 工作区取料按钮
        /// </summary>
        public bool WorkAreaPickupToggle
        {
            get => _workAreaPickupToggle;
            set => SetProperty(ref _workAreaPickupToggle, value);
        }

        private bool _workAreaPickupStatus;
        /// <summary>
        /// 工作区取料
        /// </summary>
        public bool WorkAreaPickupStatus
        {
            get => _workAreaPickupStatus;
            set => SetProperty(ref _workAreaPickupStatus, value);
        }

        private bool _workAreaPlaceToggle;
        /// <summary>
        /// 工作区放料按钮
        /// </summary>
        public bool WorkAreaPlaceToggle
        {
            get => _workAreaPlaceToggle;
            set => SetProperty(ref _workAreaPlaceToggle, value);
        }

        private bool _workAreaPlaceStatus;
        /// <summary>
        /// 工作区放料
        /// </summary>
        public bool WorkAreaPlaceStatus
        {
            get => _workAreaPlaceStatus;
            set => SetProperty(ref _workAreaPlaceStatus, value);
        }
        private bool _waitConfirmToggle;

        /// <summary>
        /// 等待确认按钮
        /// </summary>
        public bool WaitConfirmToggle
        {
            get => _waitConfirmToggle;
            set => SetProperty(ref _waitConfirmToggle, value);
        }

        private bool _waitConfirmStatus;
        /// <summary>
        /// 等待确认
        /// </summary>
        public bool WaitConfirmStatus
        {
            get => _waitConfirmStatus;
            set => SetProperty(ref _waitConfirmStatus, value);
        }

        private bool _autoModeToggle;
        /// <summary>
        /// 自动模式按钮
        /// </summary>
        public bool AutoModeToggle
        {
            get => _autoModeToggle;
            set => SetProperty(ref _autoModeToggle, value);
        }


        private bool _autoModeStatus;
        /// <summary>
        /// 自动模式激活指示灯
        /// </summary>
        public bool AutoModeStatus
        {
            get => _autoModeStatus;
            set => SetProperty(ref _autoModeStatus, value);
        }

        private bool _useUpperArmToggle;
        /// <summary>
        /// 使用上臂按钮
        /// </summary>
        public bool UseUpperArmToggle
        {
            get => _useUpperArmToggle;
            set => SetProperty(ref _useUpperArmToggle, value);
        }

        private bool _useUpperArmStatus;

        /// <summary>
        /// 使用上臂激活指示灯
        /// </summary>
        public bool UseUpperArmStatus
        {
            get => _useUpperArmStatus;
            set => SetProperty(ref _useUpperArmStatus, value);
        }

        private bool _useLowerArmToggle;
        /// <summary>
        /// 使用下臂按钮
        /// </summary>
        public bool UseLowerArmToggle
        {
            get => _useLowerArmToggle;
            set => SetProperty(ref _useLowerArmToggle, value);
        }

        private bool _useLowerArmStatus;
        /// <summary>
        /// 使用下臂激活指示灯
        /// </summary>
        public bool UseLowerArmStatus
        {
            get => _useLowerArmStatus;
            set => SetProperty(ref _useLowerArmStatus, value);
        }


        private bool _confirmSwitchToggle;

        /// <summary>
        /// 确认开关按钮
        /// </summary>
        public bool ConfirmSwitchToggle
        {
            get => _confirmSwitchToggle;
            set => SetProperty(ref _confirmSwitchToggle, value);
        }

        private bool _confirmSwitchStatus;
        /// <summary>
        /// 确认关闭激活指示灯
        /// </summary>
        public bool ConfirmSwitchStatus
        {
            get => _confirmSwitchStatus;
            set => SetProperty(ref _confirmSwitchStatus, value);
        }
        #endregion

        #region 感应区

        private bool _upperArmSensor;
        /// <summary>
        /// 上臂感应灯
        /// </summary>
        public bool UpperArmSensor
        {
            get => _upperArmSensor;
            set => SetProperty(ref _upperArmSensor, value);
        }

        private bool _lowerArmSensor;
        /// <summary>
        /// 下臂感应灯
        /// </summary>
        public bool LowerArmSensor
        {
            get => _lowerArmSensor;
            set => SetProperty(ref _lowerArmSensor, value);
        }

        private bool _commandAccepted;
        /// <summary>
        /// 命令接受
        /// </summary>
        public bool CommandAccepted
        {
            get => _commandAccepted;
            set => SetProperty(ref _commandAccepted, value);
        }

        private bool _robotSafePosition1;
        /// <summary>
        /// 机器人安全位1感应灯
        /// </summary>
        public bool RobotSafePosition1
        {
            get => _robotSafePosition1;
            set => SetProperty(ref _robotSafePosition1, value);
        }

        private bool _robotSafePositionToggle;
        /// <summary>
        /// 机器人安全位按钮
        /// </summary>
        public bool RobotSafePositionToggle
        {
            get => _robotSafePositionToggle;
            set => SetProperty(ref _robotSafePositionToggle, value);
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

        // PickFromRoller1
        private bool _pickFromRoller1Active;
        public bool PickFromRoller1Active { get => _pickFromRoller1Active; set => SetProperty(ref _pickFromRoller1Active, value); }
        private bool _pickFromRoller1Toggle;
        public bool PickFromRoller1Toggle { get => _pickFromRoller1Toggle; set => SetProperty(ref _pickFromRoller1Toggle, value); }
        private bool _pickFromRoller1Done;
        public bool PickFromRoller1Done { get => _pickFromRoller1Done; set => SetProperty(ref _pickFromRoller1Done, value); }

        // PickFromRoller2
        private bool _pickFromRoller2Active;
        public bool PickFromRoller2Active { get => _pickFromRoller2Active; set => SetProperty(ref _pickFromRoller2Active, value); }

        private bool _pickFromRoller2Toggle;
        public bool PickFromRoller2Toggle { get => _pickFromRoller2Toggle; set => SetProperty(ref _pickFromRoller2Toggle, value); }

        private bool _pickFromRoller2Done;
        public bool PickFromRoller2Done { get => _pickFromRoller2Done; set => SetProperty(ref _pickFromRoller2Done, value); }

        // PlaceToRoller3
        private bool _placeToRoller3Active;
        public bool PlaceToRoller3Active { get => _placeToRoller3Active; set => SetProperty(ref _placeToRoller3Active, value); }

        private bool _placeToRoller3Toggle;
        public bool PlaceToRoller3Toggle { get => _placeToRoller3Toggle; set => SetProperty(ref _placeToRoller3Toggle, value); }

        private bool _placeToRoller3Done;
        public bool PlaceToRoller3Done { get => _placeToRoller3Done; set => SetProperty(ref _placeToRoller3Done, value); }

        // PickFromRoller3
        private bool _pickFromRoller3Active;
        public bool PickFromRoller3Active { get => _pickFromRoller3Active; set => SetProperty(ref _pickFromRoller3Active, value); }

        private bool _pickFromRoller3Toggle;
        public bool PickFromRoller3Toggle { get => _pickFromRoller3Toggle; set => SetProperty(ref _pickFromRoller3Toggle, value); }

        private bool _pickFromRoller3Done;
        public bool PickFromRoller3Done { get => _pickFromRoller3Done; set => SetProperty(ref _pickFromRoller3Done, value); }

        // PlaceToPen1
        private bool _placeToPen1Active;
        public bool PlaceToPen1Active { get => _placeToPen1Active; set => SetProperty(ref _placeToPen1Active, value); }

        private bool _placeToPen1Toggle;
        public bool PlaceToPen1Toggle { get => _placeToPen1Toggle; set => SetProperty(ref _placeToPen1Toggle, value); }

        private bool _placeToPen1Done;
        public bool PlaceToPen1Done { get => _placeToPen1Done; set => SetProperty(ref _placeToPen1Done, value); }

        // PlaceToRoller2
        private bool _placeToRoller2Active;
        public bool PlaceToRoller2Active { get => _placeToRoller2Active; set => SetProperty(ref _placeToRoller2Active, value); }

        private bool _placeToRoller2Toggle;
        public bool PlaceToRoller2Toggle { get => _placeToRoller2Toggle; set => SetProperty(ref _placeToRoller2Toggle, value); }

        private bool _placeToRoller2Done;
        public bool PlaceToRoller2Done { get => _placeToRoller2Done; set => SetProperty(ref _placeToRoller2Done, value); }

        // Manual/Auto
        private bool _stationManualModeToggle;
        public bool StationManualModeToggle { get => _stationManualModeToggle; set => SetProperty(ref _stationManualModeToggle, value); }

        private bool _stationAutoModeToggle;
        public bool StationAutoModeToggle { get => _stationAutoModeToggle; set => SetProperty(ref _stationAutoModeToggle, value); }

        #endregion

        #region Commands
        //Station
        public DelegateCommand StationOperationCommand { get; }
        //Move
        public DelegateCommand RollerA1orA2MoveCommand { get; }
        public DelegateCommand RollerA3MoveCommand { get; }
        public DelegateCommand PenA1MoveCommand { get; }

        //Enable
        public DelegateCommand EnableCommand { get; }
        //Joge Control
        public DelegateCommand PressJogUpCommand { get; }
        public DelegateCommand ReleaseJogUpCommand { get; }
        public DelegateCommand PressJogDownCommand { get; }
        public DelegateCommand ReleaseJogDownCommand { get; }

        //Orign
        public DelegateCommand OrignCommand { get; }

        //RobotControl-程序区
        public DelegateCommand StartCommand { get; }
        public DelegateCommand PauseCommand { get; }
        public DelegateCommand StopCommand { get; }
        public DelegateCommand ResetCommand { get; }

        //RobotControl-工作区
        public DelegateCommand RollerA1_1Command { get; }
        public DelegateCommand RollerA1_2Command { get; }
        public DelegateCommand RollerA3Command { get; }
        public DelegateCommand PenA1Command { get; }
        public DelegateCommand WorkAreaPickupCommand { get; }
        public DelegateCommand WorkAreaPlaceCommand { get; }
        public DelegateCommand WaitConfirmCommand { get; }
        public DelegateCommand AutoModeCommand { get; }
        public DelegateCommand UseUpperArmCommand { get; }
        public DelegateCommand UseLowerArmCommand { get; }
        public DelegateCommand ConfirmSwitchCommand { get; }

        //RobotControl-感应区
        public DelegateCommand RobotSafePositionCommand { get; }

        //急停、报警复位、保存
        public DelegateCommand EmergencyCommand { get; }
        public DelegateCommand AlarmResetCommand { get; }
        public DelegateCommand SaveCommand { get; }

        //Station Operation
        public DelegateCommand BackCommand { get; }
        public DelegateCommand StationResetCommand { get; }
        public DelegateCommand StationStartCommand { get; }
        public DelegateCommand StationStopCommand { get; }
        public DelegateCommand StationAlarmResetCommand { get; }

        public DelegateCommand PickFromRoller1Command { get; }
        public DelegateCommand PickFromRoller2Command { get; }
        public DelegateCommand PlaceToRoller3Command { get; }
        public DelegateCommand PickFromRoller3Command { get; }
        public DelegateCommand PlaceToPen1Command { get; }
        public DelegateCommand PlaceToRoller2Command { get; }

        public DelegateCommand StationManualModeCommand { get; }
        public DelegateCommand StationAutoModeCommand { get; }

        public DelegateCommand RollerA1orA2PositionSetCommand => _rollerA1orA2PositionItem.SetCommand;
        public DelegateCommand RollerA1orA2PositionCancelCommand => _rollerA1orA2PositionItem.CancelCommand;
        public DelegateCommand RollerA3PositionSetCommand => _rollerA3PositionItem.SetCommand;
        public DelegateCommand RollerA3PositionCancelCommand => _rollerA3PositionItem.CancelCommand;
        public DelegateCommand PenA1PositionSetCommand => _penA1PositionItem.SetCommand;
        public DelegateCommand PenA1PositionCancelCommand => _penA1PositionItem.CancelCommand;
        public DelegateCommand JogSpeedSetCommand => _jogSpeedItem.SetCommand;
        public DelegateCommand JogSpeedCancelCommand => _jogSpeedItem.CancelCommand;
        public DelegateCommand AutoSpeedSetCommand => _autoSpeedItem.SetCommand;
        public DelegateCommand AutoSpeedCancelCommand => _autoSpeedItem.CancelCommand;
        #endregion        

        public CleanASideRobot1ViewModel() : base(11, "Clean A Side Robot1")
        {
            _editableItems = new List<EditableFloatItem>
            {
                _rollerA1orA2PositionItem,
                _rollerA3PositionItem,
                _penA1PositionItem,
                _jogSpeedItem,
                _autoSpeedItem
            };
            EditableFloatItemHelper.InitializeCommands(_editableItems, _plc, RaisePropertyChanged);

            // 定时器
            _readTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _readTimer.Tick += ReadTimer_Tick;
            RegisterLifecycleTimer(_readTimer);
            RegisterViewLifecycleAction(
                () => Logger.For("RobotControl").Debug("CleanASideRobot1ViewModel activated, started PLC read timer."),
                () => Logger.For("RobotControl").Debug("CleanASideRobot1ViewModel deactivated, stopped PLC read timer."));

            //Station Button 
            StationOperationCommand = new DelegateCommand(() => CurrentViewIndex = 1);

            //Move Commands
            RollerA1orA2MoveCommand = OperationAudit.Create("RollerA1orA2Move", () => _plc.Write(CleanRobotA1Address.RollerA1orA2Move, !RollerA1orA2MoveToggle), () => "CleanASideRobot1");
            RollerA3MoveCommand = OperationAudit.Create("RollerA3Move", () => _plc.Write(CleanRobotA1Address.RollerA3Move, !RollerA3MoveToggle), () => "CleanASideRobot1");
            PenA1MoveCommand = OperationAudit.Create("PenA1Move", () => _plc.Write(CleanRobotA1Address.PenA1Move, !PenA1MoveToggle), () => "CleanASideRobot1");


            //Enable
            EnableCommand = OperationAudit.Create("Enable", () => _plc.ToggleBool(CleanRobotA1Address.Enable), () => "CleanASideRobot1");

            //Jog Control
            PressJogUpCommand = new DelegateCommand(() => _plc.Write(CleanRobotA1Address.JogUp, true));
            ReleaseJogUpCommand = new DelegateCommand(() => _plc.Write(CleanRobotA1Address.JogUp, false));
            PressJogDownCommand = new DelegateCommand(() => _plc.Write(CleanRobotA1Address.JogDown, true));
            ReleaseJogDownCommand = new DelegateCommand(() => _plc.Write(CleanRobotA1Address.JogDown, false));

            //Orign
            OrignCommand = OperationAudit.Create("Orign", () =>
            {
                if (MessageBox.Show(Lang.Resources.ConfirmActionContent, Lang.Resources.ConfirmActionTips,
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _plc.Write(CleanRobotA1Address.Origin, !OriginToggle);
                }

                OriginToggle = false;
            }, () => "CleanASideRobot1");

            //急停按下、报警复位、保存
            EmergencyCommand = OperationAudit.Create("Emergency", () => _plc.Write(CleanRobotA1Address.EmergencyStop, !EmergencyStopToggle), () => "CleanASideRobot1");
            AlarmResetCommand = OperationAudit.CreateAsync("AlarmReset", () => _plc.PulseAsync(CleanRobotA1Address.AlarmReset), () => "CleanASideRobot1");
            SaveCommand = OperationAudit.Create("Save", OnSaveCommand, () => "CleanASideRobot1");

            //RobotControl - 程序区
            StartCommand = OperationAudit.CreateAsync("Start", () => _plc.PulseAsync(CleanRobotA1Address.Start), () => "CleanASideRobot1");
            PauseCommand = OperationAudit.Create("Pause", () => _plc.Write(CleanRobotA1Address.Pause, !IsPaused), () => "CleanASideRobot1");
            StopCommand = OperationAudit.CreateAsync("Stop", () => _plc.PulseAsync(CleanRobotA1Address.Stop), () => "CleanASideRobot1");
            ResetCommand = OperationAudit.CreateAsync("Reset", () => _plc.PulseAsync(CleanRobotA1Address.Reset), () => "CleanASideRobot1");

            //RobotControl - 工作区
            RollerA1_1Command = OperationAudit.Create("RollerA1_1", () => _plc.Write(CleanRobotA1Address.RollA1_Const, 0), () => "CleanASideRobot1");
            RollerA1_2Command = OperationAudit.Create("RollerA1_2", () => _plc.Write(CleanRobotA1Address.RollA1_Const, 1), () => "CleanASideRobot1");
            RollerA3Command = OperationAudit.Create("RollerA3", () => _plc.Write(CleanRobotA1Address.RollA3_Const, 2), () => "CleanASideRobot1");
            PenA1Command = OperationAudit.Create("PenA1", () => _plc.Write(CleanRobotA1Address.PenA1_Const, 3), () => "CleanASideRobot1");

            WorkAreaPickupCommand = OperationAudit.Create("WorkAreaPickup", () => _plc.Write(CleanRobotA1Address.WorkAreaPickup, !WorkAreaPickupToggle), () => "CleanASideRobot1");
            WorkAreaPlaceCommand = OperationAudit.Create("WorkAreaPlace", () => _plc.Write(CleanRobotA1Address.WorkAreaPlace, !WorkAreaPlaceToggle), () => "CleanASideRobot1");
            WaitConfirmCommand = OperationAudit.Create("WaitConfirm", () => _plc.Write(CleanRobotA1Address.WaitConfirm, !WaitConfirmToggle), () => "CleanASideRobot1");

            AutoModeCommand = OperationAudit.Create("AutoMode", () => _plc.Write(CleanRobotA1Address.AutoMode, !AutoModeToggle), () => "CleanASideRobot1");
            UseUpperArmCommand = OperationAudit.Create("UseUpperArm", () => _plc.Write(CleanRobotA1Address.UseUpperArm, !UseUpperArmToggle), () => "CleanASideRobot1");
            UseLowerArmCommand = OperationAudit.Create("UseLowerArm", () => _plc.Write(CleanRobotA1Address.UseLowerArm, !UseLowerArmToggle), () => "CleanASideRobot1");
            ConfirmSwitchCommand = OperationAudit.Create("ConfirmSwitch", () => _plc.Write(CleanRobotA1Address.ConfirmSwitch, !ConfirmSwitchToggle), () => "CleanASideRobot1");
            //RobotControl - 感应区
            RobotSafePositionCommand = OperationAudit.Create("RobotSafePosition", () => _plc.WriteDBit(CleanRobotA1Address.RobotSafePosition, !RobotSafePositionToggle), () => "CleanASideRobot1");

            //Station Operation
            BackCommand = new DelegateCommand(() => CurrentViewIndex = 0);
            StationResetCommand = OperationAudit.CreateAsync("StationReset", () => _plc.PulseAsync(CleanRobotA1Address.StationReset), () => "CleanASideRobot1");
            StationStartCommand = OperationAudit.CreateAsync("StationStart", () => _plc.PulseAsync(CleanRobotA1Address.StationStart), () => "CleanASideRobot1");
            StationStopCommand = OperationAudit.CreateAsync("StationStop", () => _plc.PulseAsync(CleanRobotA1Address.StationStop), () => "CleanASideRobot1");
            StationAlarmResetCommand = OperationAudit.CreateAsync("StationAlarmReset", () => _plc.PulseAsync(CleanRobotA1Address.StationAlarmReset), () => "CleanASideRobot1");

            PickFromRoller1Command = OperationAudit.CreateAsync("PickFromRoller1", () => _plc.PulseAsync(CleanRobotA1Address.PickFromRoller1), () => "CleanASideRobot1");
            PickFromRoller2Command = OperationAudit.CreateAsync("PickFromRoller2", () => _plc.PulseAsync(CleanRobotA1Address.PickFromRoller2), () => "CleanASideRobot1");
            PlaceToRoller3Command = OperationAudit.CreateAsync("PlaceToRoller3", () => _plc.PulseAsync(CleanRobotA1Address.PlaceToRoller3), () => "CleanASideRobot1");
            PickFromRoller3Command = OperationAudit.CreateAsync("PickFromRoller3", () => _plc.PulseAsync(CleanRobotA1Address.PickFromRoller3), () => "CleanASideRobot1");
            PlaceToPen1Command = OperationAudit.CreateAsync("PlaceToPen1", () => _plc.PulseAsync(CleanRobotA1Address.PlaceToPen1), () => "CleanASideRobot1");
            PlaceToRoller2Command = OperationAudit.CreateAsync("PlaceToRoller2", () => _plc.PulseAsync(CleanRobotA1Address.PlaceToRoller2), () => "CleanASideRobot1");

            StationManualModeCommand = OperationAudit.CreateAsync("StationManualMode", async () =>
            {
                StationManualModeToggle = true;
                StationAutoModeToggle = false;
                _plc.Write(CleanRobotA1Address.StationAutoMode, false);
                await _plc.PulseAsync(CleanRobotA1Address.StationManualMode);
            }, () => "CleanASideRobot1");
            StationAutoModeCommand = OperationAudit.CreateAsync("StationAutoMode", async () =>
            {
                StationManualModeToggle = false;
                StationAutoModeToggle = true;
                _plc.Write(CleanRobotA1Address.StationManualMode, false);
                await _plc.PulseAsync(CleanRobotA1Address.StationAutoMode);
            }, () => "CleanASideRobot1");
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

        /// <summary>
        /// 读取PLC地址的数据
        /// </summary>
        private async Task ReadPlcDataAsync()
        {
            var mMoveCommandBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<bool>(CleanRobotA1Address.RollerA1orA2Move, assign: v => RollerA1orA2MoveToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.RollerA3Move, assign: v => RollerA3MoveToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.PenA1Move, assign: v => PenA1MoveToggle = v, addressType: PlcBatchAddressType.M)
            };

            var mProgramAndWorkBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<bool>(CleanRobotA1Address.Pause, assign: v => PauseToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.AutoMode, assign: v => AutoModeToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.UseUpperArm, assign: v => UseUpperArmToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.UseLowerArm, assign: v => UseLowerArmToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.WorkAreaPickup, assign: v => WorkAreaPickupToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.WorkAreaPlace, assign: v => WorkAreaPlaceToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.WaitConfirm, assign: v => WaitConfirmToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.ConfirmSwitch, assign: v => ConfirmSwitchToggle = v, addressType: PlcBatchAddressType.M)
            };

            var mAxisStatusBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<bool>(CleanRobotA1Address.EnableStatus, assign: v => IsEnabled = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.OriginStatus, assign: v => IsAtOrigin = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.JogUpStatus, assign: v => JogUpActive = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.JogDownStatus, assign: v => JogDownActive = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.JogMiddleStatus, assign: v => JogMiddleActive = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.LimitAlarm, assign: v => LimitAlarm = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.DriverAlarm, assign: v => DriverAlarm = v, addressType: PlcBatchAddressType.M)
            };

            var mProgramStatusBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<bool>(CleanRobotA1Address.RunningStatus, assign: v => IsRunning = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.PausedStatus, assign: v => IsPaused = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.StoppedStatus, assign: v => IsStopped = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.ResetStatus, assign: v => IsReset = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.WorkAreaPickupStatus, assign: v => WorkAreaPickupStatus = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.WorkAreaPlaceStatus, assign: v => WorkAreaPlaceStatus = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.WaitConfirmStatus, assign: v => WaitConfirmStatus = v, addressType: PlcBatchAddressType.M)
            };

            var mStationStatusBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<bool>(CleanRobotA1Address.StationResetStatus, assign: v => StationIsReset = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.StationRunningStatus, assign: v => StationIsRunning = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.StationStoppedStatus, assign: v => StationIsStopped = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.StationModuleAlarm, assign: v => ModuleAlarm = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.PickFromRoller1ActiveStatus, assign: v => PickFromRoller1Active = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.PickFromRoller2ActiveStatus, assign: v => PickFromRoller2Active = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.PlaceToRoller3ActiveStatus, assign: v => PlaceToRoller3Active = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.PickFromRoller3ActiveStatus, assign: v => PickFromRoller3Active = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.PlaceToPen1ActiveStatus, assign: v => PlaceToPen1Active = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.PlaceToRoller2ActiveStatus, assign: v => PlaceToRoller2Active = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.PickFromRoller1DoneStatus, assign: v => PickFromRoller1Done = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.PickFromRoller2DoneStatus, assign: v => PickFromRoller2Done = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.PlaceToRoller3DoneStatus, assign: v => PlaceToRoller3Done = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.PickFromRoller3DoneStatus, assign: v => PickFromRoller3Done = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.PlaceToPen1DoneStatus, assign: v => PlaceToPen1Done = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.PlaceToRoller2DoneStatus, assign: v => PlaceToRoller2Done = v, addressType: PlcBatchAddressType.M)
            };

            var mBottomBarBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<bool>(CleanRobotA1Address.Save, assign: v => SaveToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA1Address.EmergencyStop, assign: v => EmergencyStopToggle = v, addressType: PlcBatchAddressType.M)
            };

            var mGlobalAlarmBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<bool>(CleanRobotA1Address.StationGlobalAlarm, assign: v => GlobalAlarm = v, addressType: PlcBatchAddressType.M)
            };

            var dStatusBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<int>(CleanRobotA1Address.CurrentCoordinate, assign: v => CurrentCoordinate = v / 1000.0),
                new PlcBatchReadItem<int>(CleanRobotA1Address.AlarmCode, assign: v => AlarmCode = v),
                new PlcBatchReadItem<int>(CleanRobotA1Address.ProgramNumber, assign: v => ProgramNumber = v),
                new PlcBatchReadItem<int>(CleanRobotA1Address.WorkAreaNumber, assign: v => WorkAreaNumber = v)
            };

            var dBitBatchItems = new List<PlcBatchReadItemBase>
            {
                PlcBatchReadItem<bool>.Bind(CleanRobotA1Address.AutoModeStatus.Address, () => AutoModeStatus, converter: MitsubishiPlcService.ToBitConverter(CleanRobotA1Address.AutoModeStatus)),
                PlcBatchReadItem<bool>.Bind(CleanRobotA1Address.UseUpperArmStatus.Address, () => UseUpperArmStatus, converter: MitsubishiPlcService.ToBitConverter(CleanRobotA1Address.UseUpperArmStatus)),
                PlcBatchReadItem<bool>.Bind(CleanRobotA1Address.UseLowerArmStatus.Address, () => UseLowerArmStatus, converter: MitsubishiPlcService.ToBitConverter(CleanRobotA1Address.UseLowerArmStatus)),
                PlcBatchReadItem<bool>.Bind(CleanRobotA1Address.ConfirmSwitchStatus.Address, () => ConfirmSwitchStatus, converter: MitsubishiPlcService.ToBitConverter(CleanRobotA1Address.ConfirmSwitchStatus)),
                PlcBatchReadItem<bool>.Bind(CleanRobotA1Address.UpperArmSensor.Address, () => UpperArmSensor, converter: MitsubishiPlcService.ToBitConverter(CleanRobotA1Address.UpperArmSensor)),
                PlcBatchReadItem<bool>.Bind(CleanRobotA1Address.LowerArmSensor.Address, () => LowerArmSensor, converter: MitsubishiPlcService.ToBitConverter(CleanRobotA1Address.LowerArmSensor)),
                PlcBatchReadItem<bool>.Bind(CleanRobotA1Address.CommandAccept.Address, () => CommandAccepted, converter: MitsubishiPlcService.ToBitConverter(CleanRobotA1Address.CommandAccept)),
                PlcBatchReadItem<bool>.Bind(CleanRobotA1Address.RobotSafePosition1.Address, () => RobotSafePosition1, converter: MitsubishiPlcService.ToBitConverter(CleanRobotA1Address.RobotSafePosition1)),
                PlcBatchReadItem<bool>.Bind(CleanRobotA1Address.RobotSafePosition.Address, () => RobotSafePositionToggle, converter: MitsubishiPlcService.ToBitConverter(CleanRobotA1Address.RobotSafePosition))
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

            var mMoveCommandBatchTask = _plc.BatchReadAsync(mMoveCommandBatchItems);
            var mProgramAndWorkBatchTask = _plc.BatchReadAsync(mProgramAndWorkBatchItems);
            var mAxisStatusBatchTask = _plc.BatchReadAsync(mAxisStatusBatchItems);
            var mProgramStatusBatchTask = _plc.BatchReadAsync(mProgramStatusBatchItems);
            var mStationStatusBatchTask = _plc.BatchReadAsync(mStationStatusBatchItems);
            var mBottomBarBatchTask = _plc.BatchReadAsync(mBottomBarBatchItems);
            var mGlobalAlarmBatchTask = _plc.BatchReadAsync(mGlobalAlarmBatchItems);

            var dStatusBatchTask = _plc.BatchReadAsync(dStatusBatchItems);
            var dBitBatchTask = _plc.BatchReadAsync(dBitBatchItems);
            var editableBatchTask = _plc.BatchReadAsync(editableBatchItems);

            var b0bTask = _plc.ReadAsync<bool>(CleanRobotA1Address.AlarmReset);

            await Task.WhenAll(
                mMoveCommandBatchTask,
                mProgramAndWorkBatchTask,
                mAxisStatusBatchTask,
                mProgramStatusBatchTask,
                mStationStatusBatchTask,
                mBottomBarBatchTask,
                mGlobalAlarmBatchTask,
                b0bTask,
                dStatusBatchTask,
                dBitBatchTask,
                editableBatchTask,
                b0bTask);

            if (!mMoveCommandBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanASideRobot1] M move batch read failed: {mMoveCommandBatchTask.Result.Message}");

            if (!mProgramAndWorkBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanASideRobot1] M program/work batch read failed: {mProgramAndWorkBatchTask.Result.Message}");

            if (!mAxisStatusBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanASideRobot1] M axis status batch read failed: {mAxisStatusBatchTask.Result.Message}");

            if (!mProgramStatusBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanASideRobot1] M program status batch read failed: {mProgramStatusBatchTask.Result.Message}");

            if (!mStationStatusBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanASideRobot1] M station batch read failed: {mStationStatusBatchTask.Result.Message}");

            if (!mBottomBarBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanASideRobot1] M bottom bar batch read failed: {mBottomBarBatchTask.Result.Message}");

            if (!mGlobalAlarmBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanASideRobot1] M global alarm batch read failed: {mGlobalAlarmBatchTask.Result.Message}");

            if (!dStatusBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanASideRobot1] D status batch read failed: {dStatusBatchTask.Result.Message}");

            if (!dBitBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanASideRobot1] D bit batch read failed: {dBitBatchTask.Result.Message}");

            if (!editableBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanASideRobot1] D editable batch read failed: {editableBatchTask.Result.Message}");

            if (b0bTask.Result.IsSuccess)
            {
                AlarmResetToggle = b0bTask.Result.Content;
            }
        }

        private void OnSaveCommand()
        {
            foreach (var item in _editableItems)
            {
                _plc.Write(item.Address, (float)item.Value);
                item.PlcValue = item.Value;
                item.HasPlcValue = true;

                if (item.IsDirty)
                {
                    item.IsDirty = false;
                    RaisePropertyChanged(item.DirtyPropertyName);
                }
            }

            _plc.Write(CleanRobotA1Address.Save, true);
        }

        public void StopReading()
        {
            OnViewDeactivated();
        }
    }
}

