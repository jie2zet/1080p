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
    public class CleanASideRobot2ViewModel : StationOpViewModelBase
    {
        private bool _isReading;
        private readonly EditableFloatItem _rollerA3PositionItem = new(CleanRobotA2Address.RollerA3, nameof(RollerA3Position), nameof(IsRollerA3PositionDirty));
        private readonly EditableFloatItem _penA2PositionItem = new(CleanRobotA2Address.PenA2, nameof(PenA2Position), nameof(IsPenA2PositionDirty));
        private readonly EditableFloatItem _penA1PositionItem = new(CleanRobotA2Address.PenA1, nameof(PenA1Position), nameof(IsPenA1PositionDirty));
        private readonly EditableFloatItem _dryAPositionItem = new(CleanRobotA2Address.DryA, nameof(DryAPosition), nameof(IsDryAPositionDirty));
        private readonly EditableFloatItem _jogSpeedItem = new(CleanRobotA2Address.JogSpeed, nameof(JogSpeed), nameof(IsJogSpeedDirty));
        private readonly EditableFloatItem _autoSpeedItem = new(CleanRobotA2Address.AutoSpeed, nameof(AutoSpeed), nameof(IsAutoSpeedDirty));
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
        public bool IsRollerA3PositionDirty => _rollerA3PositionItem.IsDirty;
        /// <summary>
        /// RollerA3
        /// </summary>
        public double RollerA3Position
        {
            get => _rollerA3PositionItem.Value;
            set => EditableFloatItemHelper.SetValue(_rollerA3PositionItem, value, RaisePropertyChanged);
        }

        public bool IsPenA2PositionDirty => _penA2PositionItem.IsDirty;

        /// <summary>
        /// PenA2
        /// </summary>
        public double PenA2Position
        {
            get => _penA2PositionItem.Value;
            set => EditableFloatItemHelper.SetValue(_penA2PositionItem, value, RaisePropertyChanged);
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

        public bool IsDryAPositionDirty => _dryAPositionItem.IsDirty;

        /// <summary>
        /// DryA
        /// </summary>
        public double DryAPosition
        {
            get => _dryAPositionItem.Value;
            set => EditableFloatItemHelper.SetValue(_dryAPositionItem, value, RaisePropertyChanged);
        }
        #endregion

        #region Move Toggle属性       
        private bool _rollerA3MoveToggle;
        /// <summary>
        /// RollerA3按钮
        /// </summary>
        public bool RollerA3MoveToggle
        {
            get => _rollerA3MoveToggle;
            set => SetProperty(ref _rollerA3MoveToggle, value);
        }

        private bool _penA2MoveToggle;
        /// <summary>
        /// PenA2按钮
        /// </summary>
        public bool PenA2MoveToggle
        {
            get => _penA2MoveToggle;
            set => SetProperty(ref _penA2MoveToggle, value);
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

        private bool _dryAMoveToggle;
        /// <summary>
        /// DryA按钮
        /// </summary>
        public bool DryAMoveToggle
        {
            get => _dryAMoveToggle;
            set => SetProperty(ref _dryAMoveToggle, value);
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

        // PickFromRoller3
        private bool _pickFromRoller3Active;
        public bool PickFromRoller3Active { get => _pickFromRoller3Active; set => SetProperty(ref _pickFromRoller3Active, value); }

        private bool _pickFromRoller3Toggle;
        public bool PickFromRoller3Toggle { get => _pickFromRoller3Toggle; set => SetProperty(ref _pickFromRoller3Toggle, value); }

        private bool _pickFromRoller3Done;
        public bool PickFromRoller3Done { get => _pickFromRoller3Done; set => SetProperty(ref _pickFromRoller3Done, value); }

        // PlaceToPen2
        private bool _placeToPen2Active;
        public bool PlaceToPen2Active { get => _placeToPen2Active; set => SetProperty(ref _placeToPen2Active, value); }

        private bool _placeToPen2Toggle;
        public bool PlaceToPen2Toggle { get => _placeToPen2Toggle; set => SetProperty(ref _placeToPen2Toggle, value); }

        private bool _placeToPen2Done;
        public bool PlaceToPen2Done { get => _placeToPen2Done; set => SetProperty(ref _placeToPen2Done, value); }

        // PickFromPen1
        private bool _pickFromPen1Active;
        public bool PickFromPen1Active { get => _pickFromPen1Active; set => SetProperty(ref _pickFromPen1Active, value); }

        private bool _pickFromPen1Toggle;
        public bool PickFromPen1Toggle { get => _pickFromPen1Toggle; set => SetProperty(ref _pickFromPen1Toggle, value); }

        private bool _pickFromPen1Done;
        public bool PickFromPen1Done { get => _pickFromPen1Done; set => SetProperty(ref _pickFromPen1Done, value); }

        // PickFromPen2
        private bool _pickFromPen2Active;
        public bool PickFromPen2Active { get => _pickFromPen2Active; set => SetProperty(ref _pickFromPen2Active, value); }

        private bool _pickFromPen2Toggle;
        public bool PickFromPen2Toggle { get => _pickFromPen2Toggle; set => SetProperty(ref _pickFromPen2Toggle, value); }

        private bool _pickFromPen2Done;
        public bool PickFromPen2Done { get => _pickFromPen2Done; set => SetProperty(ref _pickFromPen2Done, value); }

        // PlaceToDry
        private bool _placeToDryActive;
        public bool PlaceToDryActive { get => _placeToDryActive; set => SetProperty(ref _placeToDryActive, value); }

        private bool _placeToDryToggle;
        public bool PlaceToDryToggle { get => _placeToDryToggle; set => SetProperty(ref _placeToDryToggle, value); }

        private bool _placeToDryDone;
        public bool PlaceToDryDone { get => _placeToDryDone; set => SetProperty(ref _placeToDryDone, value); }

        // PlaceToPen1
        private bool _placeToPen1Active;
        public bool PlaceToPen1Active { get => _placeToPen1Active; set => SetProperty(ref _placeToPen1Active, value); }

        private bool _placeToPen1Toggle;
        public bool PlaceToPen1Toggle { get => _placeToPen1Toggle; set => SetProperty(ref _placeToPen1Toggle, value); }

        private bool _placeToPen1Done;
        public bool PlaceToPen1Done { get => _placeToPen1Done; set => SetProperty(ref _placeToPen1Done, value); }


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
        public DelegateCommand RollerA3MoveCommand { get; }
        public DelegateCommand PenA2MoveCommand { get; }
        public DelegateCommand PenA1MoveCommand { get; }
        public DelegateCommand DryAMoveCommand { get; }

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

        public DelegateCommand PickFromRoller3Command { get; }
        public DelegateCommand PlaceToPen2Command { get; }
        public DelegateCommand PickFromPen1Command { get; }
        public DelegateCommand PickFromPen2Command { get; }
        public DelegateCommand PlaceToDryCommand { get; }
        public DelegateCommand PlaceToPen1Command { get; }

        public DelegateCommand StationManualModeCommand { get; }
        public DelegateCommand StationAutoModeCommand { get; }

        public DelegateCommand RollerA3PositionSetCommand => _rollerA3PositionItem.SetCommand;
        public DelegateCommand RollerA3PositionCancelCommand => _rollerA3PositionItem.CancelCommand;
        public DelegateCommand PenA2PositionSetCommand => _penA2PositionItem.SetCommand;
        public DelegateCommand PenA2PositionCancelCommand => _penA2PositionItem.CancelCommand;
        public DelegateCommand PenA1PositionSetCommand => _penA1PositionItem.SetCommand;
        public DelegateCommand PenA1PositionCancelCommand => _penA1PositionItem.CancelCommand;
        public DelegateCommand DryAPositionSetCommand => _dryAPositionItem.SetCommand;
        public DelegateCommand DryAPositionCancelCommand => _dryAPositionItem.CancelCommand;
        public DelegateCommand JogSpeedSetCommand => _jogSpeedItem.SetCommand;
        public DelegateCommand JogSpeedCancelCommand => _jogSpeedItem.CancelCommand;
        public DelegateCommand AutoSpeedSetCommand => _autoSpeedItem.SetCommand;
        public DelegateCommand AutoSpeedCancelCommand => _autoSpeedItem.CancelCommand;
        #endregion        

        public CleanASideRobot2ViewModel() : base(12, "Clean A Side Robot2")
        {
            _editableItems = new List<EditableFloatItem>
            {
                _rollerA3PositionItem,
                _penA1PositionItem,
                _dryAPositionItem,
                _jogSpeedItem,
                _autoSpeedItem
            };
            EditableFloatItemHelper.InitializeCommands(_editableItems, _plc, RaisePropertyChanged);

            // 定时器
            _readTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _readTimer.Tick += ReadTimer_Tick;
            RegisterLifecycleTimer(_readTimer);
            RegisterViewLifecycleAction(
                () => Logger.For("RobotControl").Debug("CleanASideRobot2ViewModel activated, started PLC read timer."),
                () => Logger.For("RobotControl").Debug("CleanASideRobot2ViewModel deactivated, stopped PLC read timer."));

            //Station Button 
            StationOperationCommand = new DelegateCommand(() => CurrentViewIndex = 1);

            //Move Commands
            RollerA3MoveCommand = OperationAudit.Create("RollerA3Move", () => _plc.Write(CleanRobotA2Address.RollerA3Move, !RollerA3MoveToggle), () => "CleanASideRobot2");
            PenA2MoveCommand = OperationAudit.Create("PenA2Move", () => _plc.Write(CleanRobotA2Address.PenA2Move, !PenA2MoveToggle), () => "CleanASideRobot2");
            PenA1MoveCommand = OperationAudit.Create("PenA1Move", () => _plc.Write(CleanRobotA2Address.PenA1Move, !PenA1MoveToggle), () => "CleanASideRobot2");
            DryAMoveCommand = OperationAudit.Create("DryAMove", () => _plc.Write(CleanRobotA2Address.DryAMove, !DryAMoveToggle), () => "CleanASideRobot2");


            //Enable
            EnableCommand = OperationAudit.Create("Enable", () => _plc.ToggleBool(CleanRobotA2Address.Enable), () => "CleanASideRobot2");

            //Jog Control
            PressJogUpCommand = new DelegateCommand(() => _plc.Write(CleanRobotA2Address.JogUp, true));
            ReleaseJogUpCommand = new DelegateCommand(() => _plc.Write(CleanRobotA2Address.JogUp, false));
            PressJogDownCommand = new DelegateCommand(() => _plc.Write(CleanRobotA2Address.JogDown, true));
            ReleaseJogDownCommand = new DelegateCommand(() => _plc.Write(CleanRobotA2Address.JogDown, false));

            //Orign
            OrignCommand = OperationAudit.Create("Orign", () =>
            {
                if (MessageBox.Show(Lang.Resources.ConfirmActionContent, Lang.Resources.ConfirmActionTips,
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _plc.Write(CleanRobotA2Address.Origin, !OriginToggle);
                }

                OriginToggle = false;
            }, () => "CleanASideRobot2");

            //急停按下、报警复位、保存
            EmergencyCommand = OperationAudit.Create("Emergency", () => _plc.Write(CleanRobotA2Address.EmergencyStop, !EmergencyStopToggle), () => "CleanASideRobot2");
            AlarmResetCommand = OperationAudit.CreateAsync("AlarmReset", () => _plc.PulseAsync(CleanRobotA2Address.AlarmReset), () => "CleanASideRobot2");
            SaveCommand = OperationAudit.Create("Save", OnSaveCommand, () => "CleanASideRobot2");

            //RobotControl - 程序区
            StartCommand = OperationAudit.CreateAsync("Start", () => _plc.PulseAsync(CleanRobotA2Address.Start), () => "CleanASideRobot2");
            PauseCommand = OperationAudit.Create("Pause", () => _plc.Write(CleanRobotA2Address.Pause, !IsPaused), () => "CleanASideRobot2");
            StopCommand = OperationAudit.CreateAsync("Stop", () => _plc.PulseAsync(CleanRobotA2Address.Stop), () => "CleanASideRobot2");
            ResetCommand = OperationAudit.CreateAsync("Reset", () => _plc.PulseAsync(CleanRobotA2Address.Reset), () => "CleanASideRobot2");

            //RobotControl - 工作区
            RollerA1_1Command = OperationAudit.Create("RollerA1_1", () => _plc.Write(CleanRobotA2Address.RollA1_Const, 0), () => "CleanASideRobot2");
            RollerA1_2Command = OperationAudit.Create("RollerA1_2", () => _plc.Write(CleanRobotA2Address.RollA1_Const, 1), () => "CleanASideRobot2");
            RollerA3Command = OperationAudit.Create("RollerA3", () => _plc.Write(CleanRobotA2Address.RollA3_Const, 2), () => "CleanASideRobot2");
            PenA1Command = OperationAudit.Create("PenA1", () => _plc.Write(CleanRobotA2Address.PenA1_Const, 3), () => "CleanASideRobot2");

            WorkAreaPickupCommand = OperationAudit.Create("WorkAreaPickup", () => _plc.Write(CleanRobotA2Address.WorkAreaPickup, !WorkAreaPickupToggle), () => "CleanASideRobot2");
            WorkAreaPlaceCommand = OperationAudit.Create("WorkAreaPlace", () => _plc.Write(CleanRobotA2Address.WorkAreaPlace, !WorkAreaPlaceToggle), () => "CleanASideRobot2");
            WaitConfirmCommand = OperationAudit.Create("WaitConfirm", () => _plc.Write(CleanRobotA2Address.WaitConfirm, !WaitConfirmToggle), () => "CleanASideRobot2");

            AutoModeCommand = OperationAudit.Create("AutoMode", () => _plc.Write(CleanRobotA2Address.AutoMode, !AutoModeToggle), () => "CleanASideRobot2");
            UseUpperArmCommand = OperationAudit.Create("UseUpperArm", () => _plc.Write(CleanRobotA2Address.UseUpperArm, !UseUpperArmToggle), () => "CleanASideRobot2");
            UseLowerArmCommand = OperationAudit.Create("UseLowerArm", () => _plc.Write(CleanRobotA2Address.UseLowerArm, !UseLowerArmToggle), () => "CleanASideRobot2");
            ConfirmSwitchCommand = OperationAudit.Create("ConfirmSwitch", () => _plc.Write(CleanRobotA2Address.ConfirmSwitch, !ConfirmSwitchToggle), () => "CleanASideRobot2");
            //RobotControl - 感应区
            RobotSafePositionCommand = OperationAudit.Create("RobotSafePosition", () => _plc.WriteDBit(CleanRobotA2Address.RobotSafePosition, !RobotSafePositionToggle), () => "CleanASideRobot2");

            //Station Operation
            BackCommand = new DelegateCommand(() => CurrentViewIndex = 0);
            StationResetCommand = OperationAudit.CreateAsync("StationReset", () => _plc.PulseAsync(CleanRobotA2Address.StationReset), () => "CleanASideRobot2");
            StationStartCommand = OperationAudit.CreateAsync("StationStart", () => _plc.PulseAsync(CleanRobotA2Address.StationStart), () => "CleanASideRobot2");
            StationStopCommand = OperationAudit.CreateAsync("StationStop", () => _plc.PulseAsync(CleanRobotA2Address.StationStop), () => "CleanASideRobot2");
            StationAlarmResetCommand = OperationAudit.CreateAsync("StationAlarmReset", () => _plc.PulseAsync(CleanRobotA2Address.StationAlarmReset), () => "CleanASideRobot2");

            PickFromRoller3Command = OperationAudit.CreateAsync("PickFromRoller3", () => _plc.PulseAsync(CleanRobotA2Address.PickFromRoller3), () => "CleanASideRobot2");
            PlaceToPen2Command = OperationAudit.Create("PlaceToPen2", () =>
            {
                _plc.Write(CleanRobotA2Address.PlaceToPen2, true);
                Thread.Sleep(50);
                _plc.Write(CleanRobotA2Address.PlaceToPen2, false);
            }, () => "CleanASideRobot2");
            PickFromPen1Command = OperationAudit.CreateAsync("PickFromPen1", () => _plc.PulseAsync(CleanRobotA2Address.PickFromPen1), () => "CleanASideRobot2");
            PickFromPen2Command = OperationAudit.Create("PickFromPen2", () =>
            {
                _plc.Write(CleanRobotA2Address.PickFromPen2, true);
                Thread.Sleep(50);
                _plc.Write(CleanRobotA2Address.PickFromPen2, false);
            }, () => "CleanASideRobot2");
            PlaceToDryCommand = OperationAudit.CreateAsync("PlaceToDry", () => _plc.PulseAsync(CleanRobotA2Address.PlaceToDry), () => "CleanASideRobot2");
            PlaceToPen1Command = OperationAudit.CreateAsync("PlaceToPen1", () => _plc.PulseAsync(CleanRobotA2Address.PlaceToPen1), () => "CleanASideRobot2");

            StationManualModeCommand = OperationAudit.CreateAsync("StationManualMode", async () =>
            {
                StationManualModeToggle = true;
                StationAutoModeToggle = false;
                _plc.Write(CleanRobotA2Address.StationAutoMode, false);
                await _plc.PulseAsync(CleanRobotA2Address.StationManualMode);
            }, () => "CleanASideRobot2");
            StationAutoModeCommand = OperationAudit.CreateAsync("StationAutoMode", async () =>
            {
                StationManualModeToggle = false;
                StationAutoModeToggle = true;
                _plc.Write(CleanRobotA2Address.StationManualMode, false);
                await _plc.PulseAsync(CleanRobotA2Address.StationAutoMode);
            }, () => "CleanASideRobot2");
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
                new PlcBatchReadItem<bool>(CleanRobotA2Address.RollerA3Move, assign: v => RollerA3MoveToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.PenA1Move, assign: v => PenA1MoveToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.DryAMove, assign: v => DryAMoveToggle = v, addressType: PlcBatchAddressType.M)
            };

            var mProgramAndWorkBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<bool>(CleanRobotA2Address.Pause, assign: v => PauseToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.AutoMode, assign: v => AutoModeToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.UseUpperArm, assign: v => UseUpperArmToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.UseLowerArm, assign: v => UseLowerArmToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.WorkAreaPickup, assign: v => WorkAreaPickupToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.WorkAreaPlace, assign: v => WorkAreaPlaceToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.WaitConfirm, assign: v => WaitConfirmToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.ConfirmSwitch, assign: v => ConfirmSwitchToggle = v, addressType: PlcBatchAddressType.M)
            };

            var mAxisStatusBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<bool>(CleanRobotA2Address.EnableStatus, assign: v => IsEnabled = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.OriginStatus, assign: v => IsAtOrigin = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.JogUpStatus, assign: v => JogUpActive = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.JogDownStatus, assign: v => JogDownActive = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.JogMiddleStatus, assign: v => JogMiddleActive = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.LimitAlarm, assign: v => LimitAlarm = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.DriverAlarm, assign: v => DriverAlarm = v, addressType: PlcBatchAddressType.M)
            };

            var mProgramStatusBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<bool>(CleanRobotA2Address.RunningStatus, assign: v => IsRunning = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.PausedStatus, assign: v => IsPaused = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.StoppedStatus, assign: v => IsStopped = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.ResetStatus, assign: v => IsReset = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.WorkAreaPickupStatus, assign: v => WorkAreaPickupStatus = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.WorkAreaPlaceStatus, assign: v => WorkAreaPlaceStatus = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.WaitConfirmStatus, assign: v => WaitConfirmStatus = v, addressType: PlcBatchAddressType.M)
            };

            var mStationStatusBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<bool>(CleanRobotA2Address.StationResetStatus, assign: v => StationIsReset = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.StationRunningStatus, assign: v => StationIsRunning = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.StationStoppedStatus, assign: v => StationIsStopped = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.StationModuleAlarm, assign: v => ModuleAlarm = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.PickFromRoller3ActiveStatus, assign: v => PickFromRoller3Active = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.PickFromPen1ActiveStatus, assign: v => PickFromPen1Active = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.PlaceToDryActiveStatus, assign: v => PlaceToDryActive = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.PlaceToPen1ActiveStatus, assign: v => PlaceToPen1Active = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.PickFromRoller3DoneStatus, assign: v => PickFromRoller3Done = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.PickFromPen1DoneStatus, assign: v => PickFromPen1Done = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.PlaceToDryDoneStatus, assign: v => PlaceToDryDone = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.PlaceToPen1DoneStatus, assign: v => PlaceToPen1Done = v, addressType: PlcBatchAddressType.M)
            };

            var mBottomBarBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<bool>(CleanRobotA2Address.Save, assign: v => SaveToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotA2Address.EmergencyStop, assign: v => EmergencyStopToggle = v, addressType: PlcBatchAddressType.M)
            };

            var mGlobalAlarmBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<bool>(CleanRobotA2Address.StationGlobalAlarm, assign: v => GlobalAlarm = v, addressType: PlcBatchAddressType.M)
            };

            var dStatusBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<int>(CleanRobotA2Address.CurrentCoordinate, assign: v => CurrentCoordinate = v / 1000),
                new PlcBatchReadItem<int>(CleanRobotA2Address.AlarmCode, assign: v => AlarmCode = v),
                new PlcBatchReadItem<int>(CleanRobotA2Address.ProgramNumber, assign: v => ProgramNumber = v),
                new PlcBatchReadItem<int>(CleanRobotA2Address.WorkAreaNumber, assign: v => WorkAreaNumber = v)
            };

            var dBitBatchItems = new List<PlcBatchReadItemBase>
            {
                PlcBatchReadItem<bool>.Bind(CleanRobotA2Address.AutoModeStatus.Address, () => AutoModeStatus, converter: MitsubishiPlcService.ToBitConverter(CleanRobotA2Address.AutoModeStatus)),
                PlcBatchReadItem<bool>.Bind(CleanRobotA2Address.UseUpperArmStatus.Address, () => UseUpperArmStatus, converter: MitsubishiPlcService.ToBitConverter(CleanRobotA2Address.UseUpperArmStatus)),
                PlcBatchReadItem<bool>.Bind(CleanRobotA2Address.UseLowerArmStatus.Address, () => UseLowerArmStatus, converter: MitsubishiPlcService.ToBitConverter(CleanRobotA2Address.UseLowerArmStatus)),
                PlcBatchReadItem<bool>.Bind(CleanRobotA2Address.ConfirmSwitchStatus.Address, () => ConfirmSwitchStatus, converter: MitsubishiPlcService.ToBitConverter(CleanRobotA2Address.ConfirmSwitchStatus)),
                PlcBatchReadItem<bool>.Bind(CleanRobotA2Address.UpperArmSensor.Address, () => UpperArmSensor, converter: MitsubishiPlcService.ToBitConverter(CleanRobotA2Address.UpperArmSensor)),
                PlcBatchReadItem<bool>.Bind(CleanRobotA2Address.LowerArmSensor.Address, () => LowerArmSensor, converter: MitsubishiPlcService.ToBitConverter(CleanRobotA2Address.LowerArmSensor)),
                PlcBatchReadItem<bool>.Bind(CleanRobotA2Address.CommandAccept.Address, () => CommandAccepted, converter: MitsubishiPlcService.ToBitConverter(CleanRobotA2Address.CommandAccept)),
                PlcBatchReadItem<bool>.Bind(CleanRobotA2Address.RobotSafePosition1.Address, () => RobotSafePosition1, converter: MitsubishiPlcService.ToBitConverter(CleanRobotA2Address.RobotSafePosition1)),
                PlcBatchReadItem<bool>.Bind(CleanRobotA2Address.RobotSafePosition.Address, () => RobotSafePositionToggle, converter: MitsubishiPlcService.ToBitConverter(CleanRobotA2Address.RobotSafePosition))
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

            var b0bTask = _plc.ReadAsync<bool>(CleanRobotA2Address.AlarmReset);

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
                editableBatchTask);

            if (!mMoveCommandBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanASideRobot2] M move batch read failed: {mMoveCommandBatchTask.Result.Message}");

            if (!mProgramAndWorkBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanASideRobot2] M program/work batch read failed: {mProgramAndWorkBatchTask.Result.Message}");

            if (!mAxisStatusBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanASideRobot2] M axis status batch read failed: {mAxisStatusBatchTask.Result.Message}");

            if (!mProgramStatusBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanASideRobot2] M program status batch read failed: {mProgramStatusBatchTask.Result.Message}");

            if (!mStationStatusBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanASideRobot2] M station batch read failed: {mStationStatusBatchTask.Result.Message}");

            if (!mBottomBarBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanASideRobot2] M bottom bar batch read failed: {mBottomBarBatchTask.Result.Message}");

            if (!mGlobalAlarmBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanASideRobot2] M global alarm batch read failed: {mGlobalAlarmBatchTask.Result.Message}");

            if (!dStatusBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanASideRobot2] D status batch read failed: {dStatusBatchTask.Result.Message}");

            if (!dBitBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanASideRobot2] D bit batch read failed: {dBitBatchTask.Result.Message}");

            if (!editableBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanASideRobot2] D editable batch read failed: {editableBatchTask.Result.Message}");

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

            _plc.Write(CleanRobotA2Address.Save, true);
        }

        public void StopReading()
        {
            OnViewDeactivated();
        }
    }
}

