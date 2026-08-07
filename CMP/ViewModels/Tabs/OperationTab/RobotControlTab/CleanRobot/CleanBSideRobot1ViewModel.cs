using CMP.Constants.Plc.RobotControl;
using CMP.Services.Log;
using CMP.Services.Plc;
using CMP.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CMP.ViewModels.Tabs.OperationTab.RobotControlTab.CleanRobot
{
    public class CleanBSideRobot1ViewModel : StationOpViewModelBase
    {
        private bool _isReading;

        /// <summary>
        /// 轴位置最小值
        /// </summary>
        public double PositionMinimum { get; set; } = -90000;

        /// <summary>
        /// 轴位置最大值
        /// </summary>
        public double PositionMaximum { get; set; } = 90000;

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

        // PlaceToRollerB2
        private bool _placeToRollerB2Active;
        public bool PlaceToRollerB2Active { get => _placeToRollerB2Active; set => SetProperty(ref _placeToRollerB2Active, value); }

        private bool _placeToRollerB2Toggle;
        public bool PlaceToRollerB2Toggle { get => _placeToRollerB2Toggle; set => SetProperty(ref _placeToRollerB2Toggle, value); }

        private bool _placeToRollerB2Done;
        public bool PlaceToRollerB2Done { get => _placeToRollerB2Done; set => SetProperty(ref _placeToRollerB2Done, value); }


        // Manual/Auto
        private bool _stationManualModeToggle;
        public bool StationManualModeToggle { get => _stationManualModeToggle; set => SetProperty(ref _stationManualModeToggle, value); }

        private bool _stationAutoModeToggle;
        public bool StationAutoModeToggle { get => _stationAutoModeToggle; set => SetProperty(ref _stationAutoModeToggle, value); }

        #endregion

        #region Commands
        //Station
        public DelegateCommand StationOperationCommand { get; }

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
        //NoneCommands

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
        public DelegateCommand PlaceToRollerB2Command { get; }

        public DelegateCommand StationManualModeCommand { get; }
        public DelegateCommand StationAutoModeCommand { get; }
        #endregion        

        public CleanBSideRobot1ViewModel() : base(13, "Clean B Side Robot1")
        {
            // 定时器
            _readTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _readTimer.Tick += ReadTimer_Tick;
            RegisterLifecycleTimer(_readTimer);
            RegisterViewLifecycleAction(
                () => Logger.For("RobotControl").Debug("CleanBSideRobot1ViewModel activated, started PLC read timer."),
                () => Logger.For("RobotControl").Debug("CleanBSideRobot1ViewModel deactivated, stopped PLC read timer."));

            //Station Button 
            StationOperationCommand = new DelegateCommand(() => CurrentViewIndex = 1);

            //急停按下、报警复位、保存
            EmergencyCommand = new DelegateCommand(() => _plc.Write(CleanRobotB1Address.EmergencyStop, !EmergencyStopToggle));
            AlarmResetCommand = new DelegateCommand(() => _plc.Write(CleanRobotB1Address.AlarmReset, true));
            SaveCommand = new DelegateCommand(OnSaveCommand);

            //RobotControl - 程序区
            StartCommand = new DelegateCommand(() => _plc.Write(CleanRobotB1Address.Start, true));
            PauseCommand = new DelegateCommand(() => _plc.Write(CleanRobotB1Address.Pause, !IsPaused));
            StopCommand = new DelegateCommand(() => _plc.Write(CleanRobotB1Address.Stop, !IsStopped));
            ResetCommand = new DelegateCommand(() => _plc.Write(CleanRobotB1Address.Reset, true));

            //RobotControl - 工作区
            RollerA1_1Command = new DelegateCommand(() => _plc.Write(CleanRobotB1Address.RollA1_Const, 0));
            RollerA1_2Command = new DelegateCommand(() => _plc.Write(CleanRobotB1Address.RollA1_Const, 1));
            RollerA3Command = new DelegateCommand(() => _plc.Write(CleanRobotB1Address.RollA3_Const, 2));
            PenA1Command = new DelegateCommand(() => _plc.Write(CleanRobotB1Address.PenA1_Const, 3));

            WorkAreaPickupCommand = new DelegateCommand(() => _plc.Write(CleanRobotB1Address.WorkAreaPickup, !WorkAreaPickupToggle));
            WorkAreaPlaceCommand = new DelegateCommand(() => _plc.Write(CleanRobotB1Address.WorkAreaPlace, !WorkAreaPlaceToggle));
            WaitConfirmCommand = new DelegateCommand(() => _plc.Write(CleanRobotB1Address.WaitConfirm, !WaitConfirmToggle));

            AutoModeCommand = new DelegateCommand(() => _plc.Write(CleanRobotB1Address.AutoMode, !AutoModeToggle));
            UseUpperArmCommand = new DelegateCommand(() => _plc.Write(CleanRobotB1Address.UseUpperArm, !UseUpperArmToggle));
            UseLowerArmCommand = new DelegateCommand(() => _plc.Write(CleanRobotB1Address.UseLowerArm, !UseLowerArmToggle));
            ConfirmSwitchCommand = new DelegateCommand(() => _plc.Write(CleanRobotB1Address.ConfirmSwitch, !ConfirmSwitchToggle));
            //RobotControl - 感应区
            //none commands

            //Station Operation
            BackCommand = new DelegateCommand(() => CurrentViewIndex = 0);
            StationResetCommand = new DelegateCommand(() => { _plc.Write(CleanRobotB1Address.StationReset, true); });
            StationStartCommand = new DelegateCommand(() => { _plc.Write(CleanRobotB1Address.StationStart, true); });
            StationStopCommand = new DelegateCommand(() => { _plc.Write(CleanRobotB1Address.StationStop, true); });
            StationAlarmResetCommand = new DelegateCommand(() => { _plc.Write(CleanRobotB1Address.StationAlarmReset, true); });

            PickFromRoller1Command = new DelegateCommand(() =>
            {
                _plc.Write(CleanRobotB1Address.PickFromRoller1, true);
                Thread.Sleep(50);
                _plc.Write(CleanRobotB1Address.PickFromRoller1, false);
            });
            PickFromRoller2Command = new DelegateCommand(() =>
            {
                _plc.Write(CleanRobotB1Address.PickFromRoller2, true);
                Thread.Sleep(50);
                _plc.Write(CleanRobotB1Address.PickFromRoller2, false);
            });
            PlaceToRoller3Command = new DelegateCommand(() =>
            {
                _plc.Write(CleanRobotB1Address.PlaceToRoller3, true);
                Thread.Sleep(50);
                _plc.Write(CleanRobotB1Address.PlaceToRoller3, false);
            });
            PickFromRoller3Command = new DelegateCommand(() =>
            {
                _plc.Write(CleanRobotB1Address.PickFromRoller3, true);
                Thread.Sleep(50);
                _plc.Write(CleanRobotB1Address.PickFromRoller3, false);
            });
            PlaceToPen1Command = new DelegateCommand(() =>
            {
                _plc.Write(CleanRobotB1Address.PlaceToPen1, true);
                Thread.Sleep(50);
                _plc.Write(CleanRobotB1Address.PlaceToPen1, false);
            });
            PlaceToRollerB2Command = new DelegateCommand(() =>
            {
                _plc.Write(CleanRobotB1Address.PlaceToRollerB2, true);
                Thread.Sleep(50);
                _plc.Write(CleanRobotB1Address.PlaceToRollerB2, false);
            });

            StationManualModeCommand = new DelegateCommand(() =>
            {
                StationManualModeToggle = true;
                StationAutoModeToggle = false;
                _plc.Write(CleanRobotB1Address.StationManualMode, true);
                _plc.Write(CleanRobotB1Address.StationAutoMode, false);
            });
            StationAutoModeCommand = new DelegateCommand(() =>
            {
                StationManualModeToggle = false;
                StationAutoModeToggle = true;
                _plc.Write(CleanRobotB1Address.StationManualMode, false);
                _plc.Write(CleanRobotB1Address.StationAutoMode, true);
            });
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
            var mProgramAndWorkBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<bool>(CleanRobotB1Address.Pause, assign: v => PauseToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.AutoMode, assign: v => AutoModeToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.UseUpperArm, assign: v => UseUpperArmToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.UseLowerArm, assign: v => UseLowerArmToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.WorkAreaPickup, assign: v => WorkAreaPickupToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.WorkAreaPlace, assign: v => WorkAreaPlaceToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.WaitConfirm, assign: v => WaitConfirmToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.ConfirmSwitch, assign: v => ConfirmSwitchToggle = v, addressType: PlcBatchAddressType.M)
            };

            var mProgramStatusBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<bool>(CleanRobotB1Address.RunningStatus, assign: v => IsRunning = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.PausedStatus, assign: v => IsPaused = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.StoppedStatus, assign: v => IsStopped = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.ResetStatus, assign: v => IsReset = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.WorkAreaPickupStatus, assign: v => WorkAreaPickupStatus = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.WorkAreaPlaceStatus, assign: v => WorkAreaPlaceStatus = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.WaitConfirmStatus, assign: v => WaitConfirmStatus = v, addressType: PlcBatchAddressType.M)
            };

            var mStationStatusBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<bool>(CleanRobotB1Address.StationResetStatus, assign: v => StationIsReset = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.StationRunningStatus, assign: v => StationIsRunning = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.StationStoppedStatus, assign: v => StationIsStopped = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.StationModuleAlarm, assign: v => ModuleAlarm = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.PickFromRoller1ActiveStatus, assign: v => PickFromRoller1Active = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.PickFromRoller2ActiveStatus, assign: v => PickFromRoller2Active = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.PlaceToRoller3ActiveStatus, assign: v => PlaceToRoller3Active = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.PickFromRoller3ActiveStatus, assign: v => PickFromRoller3Active = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.PlaceToPen1ActiveStatus, assign: v => PlaceToPen1Active = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.PlaceToRollerB2ActiveStatus, assign: v => PlaceToRollerB2Active = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.PickFromRoller1DoneStatus, assign: v => PickFromRoller1Done = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.PickFromRoller2DoneStatus, assign: v => PickFromRoller2Done = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.PlaceToRoller3DoneStatus, assign: v => PlaceToRoller3Done = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.PickFromRoller3DoneStatus, assign: v => PickFromRoller3Done = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.PlaceToPen1DoneStatus, assign: v => PlaceToPen1Done = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.PlaceToRollerB2DoneStatus, assign: v => PlaceToRollerB2Done = v, addressType: PlcBatchAddressType.M)
            };

            var mBottomBarBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<bool>(CleanRobotB1Address.Save, assign: v => SaveToggle = v, addressType: PlcBatchAddressType.M),
                new PlcBatchReadItem<bool>(CleanRobotB1Address.EmergencyStop, assign: v => EmergencyStopToggle = v, addressType: PlcBatchAddressType.M)
            };

            var mGlobalAlarmBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<bool>(CleanRobotB1Address.StationGlobalAlarm, assign: v => GlobalAlarm = v, addressType: PlcBatchAddressType.M)
            };

            var dStatusBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<int>(CleanRobotB1Address.ProgramNumber, assign: v => ProgramNumber = v),
                new PlcBatchReadItem<int>(CleanRobotB1Address.WorkAreaNumber, assign: v => WorkAreaNumber = v)
            };

            var dBitBatchItems = new List<PlcBatchReadItemBase>
            {
                PlcBatchReadItem<bool>.Bind(CleanRobotB1Address.AutoModeStatus.Address, () => AutoModeStatus, converter: MitsubishiPlcService.ToBitConverter(CleanRobotB1Address.AutoModeStatus)),
                PlcBatchReadItem<bool>.Bind(CleanRobotB1Address.UseUpperArmStatus.Address, () => UseUpperArmStatus, converter: MitsubishiPlcService.ToBitConverter(CleanRobotB1Address.UseUpperArmStatus)),
                PlcBatchReadItem<bool>.Bind(CleanRobotB1Address.UseLowerArmStatus.Address, () => UseLowerArmStatus, converter: MitsubishiPlcService.ToBitConverter(CleanRobotB1Address.UseLowerArmStatus)),
                PlcBatchReadItem<bool>.Bind(CleanRobotB1Address.ConfirmSwitchStatus.Address, () => ConfirmSwitchStatus, converter: MitsubishiPlcService.ToBitConverter(CleanRobotB1Address.ConfirmSwitchStatus)),
                PlcBatchReadItem<bool>.Bind(CleanRobotB1Address.UpperArmSensor.Address, () => UpperArmSensor, converter: MitsubishiPlcService.ToBitConverter(CleanRobotB1Address.UpperArmSensor)),
                PlcBatchReadItem<bool>.Bind(CleanRobotB1Address.LowerArmSensor.Address, () => LowerArmSensor, converter: MitsubishiPlcService.ToBitConverter(CleanRobotB1Address.LowerArmSensor)),
                PlcBatchReadItem<bool>.Bind(CleanRobotB1Address.CommandAccept.Address, () => CommandAccepted, converter: MitsubishiPlcService.ToBitConverter(CleanRobotB1Address.CommandAccept)),
                PlcBatchReadItem<bool>.Bind(CleanRobotB1Address.RobotSafePosition1.Address, () => RobotSafePosition1, converter: MitsubishiPlcService.ToBitConverter(CleanRobotB1Address.RobotSafePosition1))
            };

            var mProgramAndWorkBatchTask = _plc.BatchReadAsync(mProgramAndWorkBatchItems);
            var mProgramStatusBatchTask = _plc.BatchReadAsync(mProgramStatusBatchItems);
            var mStationStatusBatchTask = _plc.BatchReadAsync(mStationStatusBatchItems);
            var mBottomBarBatchTask = _plc.BatchReadAsync(mBottomBarBatchItems);
            var mGlobalAlarmBatchTask = _plc.BatchReadAsync(mGlobalAlarmBatchItems);

            var dStatusBatchTask = _plc.BatchReadAsync(dStatusBatchItems);
            var dBitBatchTask = _plc.BatchReadAsync(dBitBatchItems);

            var b0bTask = _plc.ReadAsync<bool>(CleanRobotB1Address.AlarmReset);

            await Task.WhenAll(
                mProgramAndWorkBatchTask,
                mProgramStatusBatchTask,
                mStationStatusBatchTask,
                mBottomBarBatchTask,
                mGlobalAlarmBatchTask,
                dStatusBatchTask,
                dBitBatchTask,
                b0bTask);

            if (!mProgramAndWorkBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanBSideRobot1] M program/work batch read failed: {mProgramAndWorkBatchTask.Result.Message}");

            if (!mProgramStatusBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanBSideRobot1] M program status batch read failed: {mProgramStatusBatchTask.Result.Message}");

            if (!mStationStatusBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanBSideRobot1] M station batch read failed: {mStationStatusBatchTask.Result.Message}");

            if (!mBottomBarBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanBSideRobot1] M bottom bar batch read failed: {mBottomBarBatchTask.Result.Message}");

            if (!mGlobalAlarmBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanBSideRobot1] M global alarm batch read failed: {mGlobalAlarmBatchTask.Result.Message}");

            if (!dStatusBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanBSideRobot1] D status batch read failed: {dStatusBatchTask.Result.Message}");

            if (!dBitBatchTask.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[CleanBSideRobot1] D bit batch read failed: {dBitBatchTask.Result.Message}");

            if (b0bTask.Result.IsSuccess)
            {
                AlarmResetToggle = b0bTask.Result.Content;
            }

        }

        private void OnSaveCommand()
        {
            _plc.Write(CleanRobotB1Address.Save, true);
        }

        public void StopReading()
        {
            OnViewDeactivated();
        }
    }
}
