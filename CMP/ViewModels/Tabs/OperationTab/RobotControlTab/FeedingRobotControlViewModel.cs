using System.Windows;
using System.Windows.Threading;
using CMP.Constants.Plc.RobotControl;
using CMP.Services.Log;
using CMP.Services.Plc;
using CMP.ViewModels.Common;
using HandyControl.Controls;
using Prism.Commands;
using WPFLocalizeExtension.Extensions;
using MessageBox = HandyControl.Controls.MessageBox;
using CMP.Services.Auth;

namespace CMP.ViewModels.Tabs.OperationTab.RobotControlTab;

/// <summary>
/// 上料机器人控制 ViewModel
/// </summary>
public class FeedingRobotControlViewModel : TabContentViewModelBase
{
    private readonly MitsubishiPlcService _plc;
    private readonly DispatcherTimer _readTimer;
    private bool _isReading;

    private string? _tabTitleKey;

    public string? TabTitleKey
    {
        get => _tabTitleKey;
        set
        {
            if (SetProperty(ref _tabTitleKey, value))
            {
                RefreshTabTitle();
            }
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

    #region 轴位置属性

    /// <summary>
    /// 轴位置最小值
    /// </summary>
    public double PositionMinimum { get; set; } = -90000;

    /// <summary>
    /// 轴位置最大值
    /// </summary>
    public double PositionMaximum { get; set; } = 90000;

    private readonly EditableFloatItem _loadPort1PositionItem = new(FeedingRobotAddress.LoadPort1Position, nameof(LoadPort1Position), nameof(IsLoadPort1PositionDirty));
    private readonly EditableFloatItem _loadPort2PositionItem = new(FeedingRobotAddress.LoadPort2Position, nameof(LoadPort2Position), nameof(IsLoadPort2PositionDirty));
    private readonly EditableFloatItem _loadPort3PositionItem = new(FeedingRobotAddress.LoadPort3Position, nameof(LoadPort3Position), nameof(IsLoadPort3PositionDirty));
    private readonly EditableFloatItem _loadPort4PositionItem = new(FeedingRobotAddress.LoadPort4Position, nameof(LoadPort4Position), nameof(IsLoadPort4PositionDirty));
    private readonly EditableFloatItem _loadPort5PositionItem = new(FeedingRobotAddress.LoadPort5Position, nameof(LoadPort5Position), nameof(IsLoadPort5PositionDirty));
    private readonly EditableFloatItem _middleModulePositionItem = new(FeedingRobotAddress.MiddleModulePosition, nameof(MiddleModulePosition), nameof(IsMiddleModulePositionDirty));
    private readonly EditableFloatItem _dryAPositionItem = new(FeedingRobotAddress.DryAPosition, nameof(DryAPosition), nameof(IsDryAPositionDirty));
    private readonly EditableFloatItem _dryBPositionItem = new(FeedingRobotAddress.DryBPosition, nameof(DryBPosition), nameof(IsDryBPositionDirty));
    private readonly EditableFloatItem _jogSpeedItem = new(FeedingRobotAddress.JogSpeed, nameof(JogSpeed), nameof(IsJogSpeedDirty));
    private readonly EditableFloatItem _autoSpeedItem = new(FeedingRobotAddress.AutoSpeed, nameof(AutoSpeed), nameof(IsAutoSpeedDirty));
    private readonly List<EditableFloatItem> _editableItems;
    private readonly EditableIntItem _sourceBoxNoItem = new(FeedingRobotAddress.SourceBoxNo, nameof(SourceBoxNo), nameof(IsSourceBoxNoDirty));
    private readonly EditableIntItem _sourceLayerItem = new(FeedingRobotAddress.SourceLayer, nameof(SourceLayer), nameof(IsSourceLayerDirty));
    private readonly EditableIntItem _targetBoxNoItem = new(FeedingRobotAddress.TargetBoxNo, nameof(TargetBoxNo), nameof(IsTargetBoxNoDirty));
    private readonly EditableIntItem _targetLayerItem = new(FeedingRobotAddress.TargetLayer, nameof(TargetLayer), nameof(IsTargetLayerDirty));
    private readonly List<EditableIntItem> _editableIntItems;

    public bool IsLoadPort1PositionDirty => _loadPort1PositionItem.IsDirty;

    public double LoadPort1Position
    {
        get => _loadPort1PositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_loadPort1PositionItem, value, RaisePropertyChanged);
    }

    public bool IsLoadPort2PositionDirty => _loadPort2PositionItem.IsDirty;

    public double LoadPort2Position
    {
        get => _loadPort2PositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_loadPort2PositionItem, value, RaisePropertyChanged);
    }

    public bool IsLoadPort3PositionDirty => _loadPort3PositionItem.IsDirty;

    public double LoadPort3Position
    {
        get => _loadPort3PositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_loadPort3PositionItem, value, RaisePropertyChanged);
    }

    public bool IsLoadPort4PositionDirty => _loadPort4PositionItem.IsDirty;

    public double LoadPort4Position
    {
        get => _loadPort4PositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_loadPort4PositionItem, value, RaisePropertyChanged);
    }

    public bool IsLoadPort5PositionDirty => _loadPort5PositionItem.IsDirty;

    public double LoadPort5Position
    {
        get => _loadPort5PositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_loadPort5PositionItem, value, RaisePropertyChanged);
    }

    public bool IsMiddleModulePositionDirty => _middleModulePositionItem.IsDirty;

    public double MiddleModulePosition
    {
        get => _middleModulePositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_middleModulePositionItem, value, RaisePropertyChanged);
    }

    public bool IsDryAPositionDirty => _dryAPositionItem.IsDirty;

    public double DryAPosition
    {
        get => _dryAPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_dryAPositionItem, value, RaisePropertyChanged);
    }

    public bool IsDryBPositionDirty => _dryBPositionItem.IsDirty;

    public double DryBPosition
    {
        get => _dryBPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_dryBPositionItem, value, RaisePropertyChanged);
    }

    #endregion

    #region Move Toggle 属性

    private bool _loadPort1MoveToggle;

    public bool LoadPort1MoveToggle
    {
        get => _loadPort1MoveToggle;
        set => SetProperty(ref _loadPort1MoveToggle, value);
    }

    private bool _loadPort2MoveToggle;

    public bool LoadPort2MoveToggle
    {
        get => _loadPort2MoveToggle;
        set => SetProperty(ref _loadPort2MoveToggle, value);
    }

    private bool _loadPort3MoveToggle;

    public bool LoadPort3MoveToggle
    {
        get => _loadPort3MoveToggle;
        set => SetProperty(ref _loadPort3MoveToggle, value);
    }

    private bool _loadPort4MoveToggle;

    public bool LoadPort4MoveToggle
    {
        get => _loadPort4MoveToggle;
        set => SetProperty(ref _loadPort4MoveToggle, value);
    }

    private bool _loadPort5MoveToggle;

    public bool LoadPort5MoveToggle
    {
        get => _loadPort5MoveToggle;
        set => SetProperty(ref _loadPort5MoveToggle, value);
    }

    private bool _middleModuleMoveToggle;

    public bool MiddleModuleMoveToggle
    {
        get => _middleModuleMoveToggle;
        set => SetProperty(ref _middleModuleMoveToggle, value);
    }

    private bool _dryAMoveToggle;

    public bool DryAMoveToggle
    {
        get => _dryAMoveToggle;
        set => SetProperty(ref _dryAMoveToggle, value);
    }

    private bool _dryBMoveToggle;

    public bool DryBMoveToggle
    {
        get => _dryBMoveToggle;
        set => SetProperty(ref _dryBMoveToggle, value);
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

    #region LoadPort 激活状态

    private bool _loadPort1Active;

    public bool LoadPort1Active
    {
        get => _loadPort1Active;
        set => SetProperty(ref _loadPort1Active, value);
    }

    private bool _loadPort2Active;

    public bool LoadPort2Active
    {
        get => _loadPort2Active;
        set => SetProperty(ref _loadPort2Active, value);
    }

    private bool _loadPort3Active;

    public bool LoadPort3Active
    {
        get => _loadPort3Active;
        set => SetProperty(ref _loadPort3Active, value);
    }

    private bool _loadPort4Active;

    public bool LoadPort4Active
    {
        get => _loadPort4Active;
        set => SetProperty(ref _loadPort4Active, value);
    }

    private bool _loadPort5Active;

    public bool LoadPort5Active
    {
        get => _loadPort5Active;
        set => SetProperty(ref _loadPort5Active, value);
    }

    private bool _middleModuleActive;

    public bool MiddleModuleActive
    {
        get => _middleModuleActive;
        set => SetProperty(ref _middleModuleActive, value);
    }

    private bool _dryAActive;

    public bool DryAActive
    {
        get => _dryAActive;
        set => SetProperty(ref _dryAActive, value);
    }

    private bool _dryBActive;

    public bool DryBActive
    {
        get => _dryBActive;
        set => SetProperty(ref _dryBActive, value);
    }

    #endregion

    #region Jog 控制属性

    private bool _jogUpActive;

    public bool JogUpActive
    {
        get => _jogUpActive;
        set => SetProperty(ref _jogUpActive, value);
    }

    private bool _jogUpToggle;

    public bool JogUpToggle
    {
        get => _jogUpToggle;
        set => SetProperty(ref _jogUpToggle, value);
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

    private bool _jogDownToggle;

    public bool JogDownToggle
    {
        get => _jogDownToggle;
        set => SetProperty(ref _jogDownToggle, value);
    }

    private bool _originToggle;

    public bool OriginToggle
    {
        get => _originToggle;
        set => SetProperty(ref _originToggle, value);
    }

    #endregion


    #region 机器人控制属性

    private int _programNumber;

    public int ProgramNumber
    {
        get => _programNumber;
        set
        {
            if (SetProperty(ref _programNumber, value))
                _plc.Write(FeedingRobotAddress.ProgramNumber, (short)value);
        }
    }

    private int _workAreaNumber;

    public int WorkAreaNumber
    {
        get => _workAreaNumber;
        set
        {
            if (SetProperty(ref _workAreaNumber, value))
                _plc.Write(FeedingRobotAddress.WorkAreaNumber, (short)value);
        }
    }

    private int _boxNumber;

    public int BoxNumber
    {
        get => _boxNumber;
        set
        {
            if (SetProperty(ref _boxNumber, value))
                _plc.Write(FeedingRobotAddress.BoxNumber, (short)value);
        }
    }

    private int _layerCount;

    public int LayerCount
    {
        get => _layerCount;
        set
        {
            if (SetProperty(ref _layerCount, value))
                _plc.Write(FeedingRobotAddress.LayerCount, (short)value);
        }
    }

    private bool _isRunning;

    public bool IsRunning
    {
        get => _isRunning;
        set => SetProperty(ref _isRunning, value);
    }

    private bool _isPaused;

    public bool IsPaused
    {
        get => _isPaused;
        set => SetProperty(ref _isPaused, value);
    }

    private bool _isStopped;

    public bool IsStopped
    {
        get => _isStopped;
        set => SetProperty(ref _isStopped, value);
    }

    private bool _isReset;

    public bool IsReset
    {
        get => _isReset;
        set => SetProperty(ref _isReset, value);
    }

    private bool _workAreaPickup;

    public bool WorkAreaPickup
    {
        get => _workAreaPickup;
        set => SetProperty(ref _workAreaPickup, value);
    }

    private bool _workAreaPickupToggle;

    public bool WorkAreaPickupToggle
    {
        get => _workAreaPickupToggle;
        set => SetProperty(ref _workAreaPickupToggle, value);
    }

    private bool _workAreaPlace;

    public bool WorkAreaPlace
    {
        get => _workAreaPlace;
        set => SetProperty(ref _workAreaPlace, value);
    }

    private bool _workAreaPlaceToggle;

    public bool WorkAreaPlaceToggle
    {
        get => _workAreaPlaceToggle;
        set => SetProperty(ref _workAreaPlaceToggle, value);
    }

    private bool _waitConfirm;

    public bool WaitConfirm
    {
        get => _waitConfirm;
        set => SetProperty(ref _waitConfirm, value);
    }

    private bool _waitConfirmToggle;

    public bool WaitConfirmToggle
    {
        get => _waitConfirmToggle;
        set => SetProperty(ref _waitConfirmToggle, value);
    }

    private bool _autoModeToggle;

    public bool AutoModeToggle
    {
        get => _autoModeToggle;
        set => SetProperty(ref _autoModeToggle, value);
    }

    private bool _useUpperArmToggle;

    public bool UseUpperArmToggle
    {
        get => _useUpperArmToggle;
        set => SetProperty(ref _useUpperArmToggle, value);
    }

    private bool _useLowerArmToggle;

    public bool UseLowerArmToggle
    {
        get => _useLowerArmToggle;
        set => SetProperty(ref _useLowerArmToggle, value);
    }

    private bool _confirmSwitchToggle;

    public bool ConfirmSwitchToggle
    {
        get => _confirmSwitchToggle;
        set => SetProperty(ref _confirmSwitchToggle, value);
    }

    private bool _boxPickup;

    public bool BoxPickup
    {
        get => _boxPickup;
        set => SetProperty(ref _boxPickup, value);
    }

    private bool _boxPickupToggle;

    public bool BoxPickupToggle
    {
        get => _boxPickupToggle;
        set => SetProperty(ref _boxPickupToggle, value);
    }

    private bool _boxPlace;

    public bool BoxPlace
    {
        get => _boxPlace;
        set => SetProperty(ref _boxPlace, value);
    }

    private bool _boxPlaceToggle;

    public bool BoxPlaceToggle
    {
        get => _boxPlaceToggle;
        set => SetProperty(ref _boxPlaceToggle, value);
    }

    private bool _flipTo0;

    public bool FlipTo0
    {
        get => _flipTo0;
        set => SetProperty(ref _flipTo0, value);
    }

    private bool _flipTo0Toggle;

    public bool FlipTo0Toggle
    {
        get => _flipTo0Toggle;
        set => SetProperty(ref _flipTo0Toggle, value);
    }

    private bool _flipTo180;

    public bool FlipTo180
    {
        get => _flipTo180;
        set => SetProperty(ref _flipTo180, value);
    }

    private bool _flipTo180Toggle;

    public bool FlipTo180Toggle
    {
        get => _flipTo180Toggle;
        set => SetProperty(ref _flipTo180Toggle, value);
    }

    private bool _upperArmSensor;

    public bool UpperArmSensor
    {
        get => _upperArmSensor;
        set => SetProperty(ref _upperArmSensor, value);
    }

    private bool _lowerArmSensor;

    public bool LowerArmSensor
    {
        get => _lowerArmSensor;
        set => SetProperty(ref _lowerArmSensor, value);
    }

    private bool _commandAccepted;

    public bool CommandAccepted
    {
        get => _commandAccepted;
        set => SetProperty(ref _commandAccepted, value);
    }

    private bool _robotSafePosition;

    public bool RobotSafePosition
    {
        get => _robotSafePosition;
        set => SetProperty(ref _robotSafePosition, value);
    }

    private bool _robotSafePositionToggle;

    public bool RobotSafePositionToggle
    {
        get => _robotSafePositionToggle;
        set => SetProperty(ref _robotSafePositionToggle, value);
    }

    private bool _autoModeActive;

    public bool AutoModeActive
    {
        get => _autoModeActive;
        set => SetProperty(ref _autoModeActive, value);
    }

    private bool _useUpperArmActive;

    public bool UseUpperArmActive
    {
        get => _useUpperArmActive;
        set => SetProperty(ref _useUpperArmActive, value);
    }

    private bool _useLowerArmActive;

    public bool UseLowerArmActive
    {
        get => _useLowerArmActive;
        set => SetProperty(ref _useLowerArmActive, value);
    }

    private bool _confirmSwitchActive;

    public bool ConfirmSwitchActive
    {
        get => _confirmSwitchActive;
        set => SetProperty(ref _confirmSwitchActive, value);
    }

    #endregion

    #region Station Operation 属性

    private int _currentViewIndex;

    public int CurrentViewIndex
    {
        get => _currentViewIndex;
        set => SetProperty(ref _currentViewIndex, value);
    }

    private bool _isAutoMode;

    public bool IsAutoMode
    {
        get => _isAutoMode;
        set
        {
            if (SetProperty(ref _isAutoMode, value) && value)
            {
                IsManualMode = false;
                _plc.Write(FeedingRobotAddress.AutoMode, true);
            }
        }
    }

    private bool _isManualMode = true;

    public bool IsManualMode
    {
        get => _isManualMode;
        set
        {
            if (SetProperty(ref _isManualMode, value) && value)
            {
                IsAutoMode = false;
                _plc.Write(FeedingRobotAddress.ManualMode, true);
            }
        }
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

    private bool _pickFromBoxActive;

    public bool PickFromBoxActive
    {
        get => _pickFromBoxActive;
        set => SetProperty(ref _pickFromBoxActive, value);
    }

    private bool _pickFromBoxDone;

    public bool PickFromBoxDone
    {
        get => _pickFromBoxDone;
        set => SetProperty(ref _pickFromBoxDone, value);
    }

    private bool _placeToModuleActive;

    public bool PlaceToModuleActive
    {
        get => _placeToModuleActive;
        set => SetProperty(ref _placeToModuleActive, value);
    }

    private bool _placeToModuleDone;

    public bool PlaceToModuleDone
    {
        get => _placeToModuleDone;
        set => SetProperty(ref _placeToModuleDone, value);
    }

    private bool _pickFromDryAActive;

    public bool PickFromDryAActive
    {
        get => _pickFromDryAActive;
        set => SetProperty(ref _pickFromDryAActive, value);
    }

    private bool _pickFromDryADone;

    public bool PickFromDryADone
    {
        get => _pickFromDryADone;
        set => SetProperty(ref _pickFromDryADone, value);
    }

    private bool _pickFromDryBActive;

    public bool PickFromDryBActive
    {
        get => _pickFromDryBActive;
        set => SetProperty(ref _pickFromDryBActive, value);
    }

    private bool _pickFromDryBDone;

    public bool PickFromDryBDone
    {
        get => _pickFromDryBDone;
        set => SetProperty(ref _pickFromDryBDone, value);
    }

    private bool _placeToBoxActive;

    public bool PlaceToBoxActive
    {
        get => _placeToBoxActive;
        set => SetProperty(ref _placeToBoxActive, value);
    }

    private bool _placeToBoxDone;

    public bool PlaceToBoxDone
    {
        get => _placeToBoxDone;
        set => SetProperty(ref _placeToBoxDone, value);
    }

    public bool IsSourceBoxNoDirty => _sourceBoxNoItem.IsDirty;

    public int SourceBoxNo
    {
        get => _sourceBoxNoItem.Value;
        set => EditableIntItemHelper.SetValue(_sourceBoxNoItem, value, RaisePropertyChanged);
    }

    public bool IsSourceLayerDirty => _sourceLayerItem.IsDirty;

    public int SourceLayer
    {
        get => _sourceLayerItem.Value;
        set => EditableIntItemHelper.SetValue(_sourceLayerItem, value, RaisePropertyChanged);
    }

    public bool IsTargetBoxNoDirty => _targetBoxNoItem.IsDirty;

    public int TargetBoxNo
    {
        get => _targetBoxNoItem.Value;
        set => EditableIntItemHelper.SetValue(_targetBoxNoItem, value, RaisePropertyChanged);
    }

    public bool IsTargetLayerDirty => _targetLayerItem.IsDirty;

    public int TargetLayer
    {
        get => _targetLayerItem.Value;
        set => EditableIntItemHelper.SetValue(_targetLayerItem, value, RaisePropertyChanged);
    }

    // Station Operation 专用状态
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

    private bool _stopLoadToggle;

    public bool StopLoadToggle
    {
        get => _stopLoadToggle;
        set => SetProperty(ref _stopLoadToggle, value);
    }

    private bool _resumeLoadToggle;

    public bool ResumeLoadToggle
    {
        get => _resumeLoadToggle;
        set => SetProperty(ref _resumeLoadToggle, value);
    }

    private bool _sideAHealthy;

    public bool SideAHealthy
    {
        get => _sideAHealthy;
        set => SetProperty(ref _sideAHealthy, value);
    }

    private bool _sideBHealthy;

    public bool SideBHealthy
    {
        get => _sideBHealthy;
        set => SetProperty(ref _sideBHealthy, value);
    }

    // Station Operation Toggle 属性
    private bool _stationResetToggle;

    public bool StationResetToggle
    {
        get => _stationResetToggle;
        set
        {
            if (SetProperty(ref _stationResetToggle, value))
                _plc.Write(FeedingRobotAddress.StationReset, value);
        }
    }

    private bool _stationStartToggle;

    public bool StationStartToggle
    {
        get => _stationStartToggle;
        set
        {
            if (SetProperty(ref _stationStartToggle, value))
                _plc.Write(FeedingRobotAddress.StationStart, value);
        }
    }

    private bool _stationStopToggle;

    public bool StationStopToggle
    {
        get => _stationStopToggle;
        set
        {
            if (SetProperty(ref _stationStopToggle, value))
                _plc.Write(FeedingRobotAddress.StationStop, value);
        }
    }

    private bool _pickFromBoxToggle;

    public bool PickFromBoxToggle
    {
        get => _pickFromBoxToggle;
        set
        {
            if (SetProperty(ref _pickFromBoxToggle, value))
                _plc.Write(FeedingRobotAddress.PickFromBox, value);
        }
    }

    private bool _placeToModuleToggle;

    public bool PlaceToModuleToggle
    {
        get => _placeToModuleToggle;
        set
        {
            if (SetProperty(ref _placeToModuleToggle, value))
                _plc.Write(FeedingRobotAddress.PlaceToModule, value);
        }
    }

    private bool _pickFromDryAToggle;

    public bool PickFromDryAToggle
    {
        get => _pickFromDryAToggle;
        set
        {
            if (SetProperty(ref _pickFromDryAToggle, value))
                _plc.Write(FeedingRobotAddress.PickFromDryA, value);
        }
    }

    private bool _pickFromDryBToggle;

    public bool PickFromDryBToggle
    {
        get => _pickFromDryBToggle;
        set
        {
            if (SetProperty(ref _pickFromDryBToggle, value))
                _plc.Write(FeedingRobotAddress.PickFromDryB, value);
        }
    }

    private bool _placeToBoxToggle;

    public bool PlaceToBoxToggle
    {
        get => _placeToBoxToggle;
        set
        {
            if (SetProperty(ref _placeToBoxToggle, value))
                _plc.Write(FeedingRobotAddress.PlaceToBox, value);
        }
    }

    private bool _manualModeToggle;

    public bool ManualModeToggle
    {
        get => _manualModeToggle;
        set => SetProperty(ref _manualModeToggle, value);
    }

    private bool _transferToggle;

    public bool TransferToggle
    {
        get => _transferToggle;
        set => SetProperty(ref _transferToggle, value);
    }

    #endregion

    #region Robot Control Toggle 属性

    private bool _startToggle;

    public bool StartToggle
    {
        get => _startToggle;
        set => SetProperty(ref _startToggle, value);
    }

    private bool _pauseToggle;

    public bool PauseToggle
    {
        get => _pauseToggle;
        set => SetProperty(ref _pauseToggle, value);
    }

    private bool _stopToggle;

    public bool StopToggle
    {
        get => _stopToggle;
        set => SetProperty(ref _stopToggle, value);
    }

    private bool _resetToggle;

    public bool ResetToggle
    {
        get => _resetToggle;
        set => SetProperty(ref _resetToggle, value);
    }

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

    #region Cache Body Properties

    private bool _cacheADoorOpenStatus;

    public bool CacheADoorOpenStatus
    {
        get => _cacheADoorOpenStatus;
        set => SetProperty(ref _cacheADoorOpenStatus, value);
    }

    private bool _cacheADoorCloseStatus;

    public bool CacheADoorCloseStatus
    {
        get => _cacheADoorCloseStatus;
        set => SetProperty(ref _cacheADoorCloseStatus, value);
    }

    private bool _cacheAFiber1Status;

    public bool CacheAFiber1Status
    {
        get => _cacheAFiber1Status;
        set => SetProperty(ref _cacheAFiber1Status, value);
    }

    private bool _cacheALiquidLevelStatus;

    public bool CacheALiquidLevelStatus
    {
        get => _cacheALiquidLevelStatus;
        set => SetProperty(ref _cacheALiquidLevelStatus, value);
    }

    private bool _cacheADoorSwitchStatus;

    public bool CacheADoorSwitchStatus
    {
        get => _cacheADoorSwitchStatus;
        set => SetProperty(ref _cacheADoorSwitchStatus, value);
    }

    private bool _cacheBDoorOpenStatus;

    public bool CacheBDoorOpenStatus
    {
        get => _cacheBDoorOpenStatus;
        set => SetProperty(ref _cacheBDoorOpenStatus, value);
    }

    private bool _cacheBDoorCloseStatus;

    public bool CacheBDoorCloseStatus
    {
        get => _cacheBDoorCloseStatus;
        set => SetProperty(ref _cacheBDoorCloseStatus, value);
    }

    private bool _cacheBFiber1Status;

    public bool CacheBFiber1Status
    {
        get => _cacheBFiber1Status;
        set => SetProperty(ref _cacheBFiber1Status, value);
    }

    private bool _cacheBLiquidLevelStatus;

    public bool CacheBLiquidLevelStatus
    {
        get => _cacheBLiquidLevelStatus;
        set => SetProperty(ref _cacheBLiquidLevelStatus, value);
    }

    private bool _cacheBDoorSwitchStatus;

    public bool CacheBDoorSwitchStatus
    {
        get => _cacheBDoorSwitchStatus;
        set => SetProperty(ref _cacheBDoorSwitchStatus, value);
    }

    #endregion


    #region Commands

    public DelegateCommand StartCommand { get; }
    public DelegateCommand StopCommand { get; }
    public DelegateCommand ResetCommand { get; }
    public DelegateCommand ModulePositionCommand { get; }
    public DelegateCommand DryACommand { get; }
    public DelegateCommand DryBCommand { get; }
    public DelegateCommand CacheBodyCommand { get; }
    public DelegateCommand SaveCommand { get; }
    public DelegateCommand StationOperationCommand { get; }
    public DelegateCommand BackCommand { get; }
    public DelegateCommand AlarmResetCommand { get; }
    public DelegateCommand BackToRobotCommand { get; }
    public DelegateCommand CacheADoorOpenCommand { get; }
    public DelegateCommand CacheADoorCloseCommand { get; }
    public DelegateCommand CacheBDoorOpenCommand { get; }
    public DelegateCommand CacheBDoorCloseCommand { get; }
    public DelegateCommand BackFromCacheBodyCommand { get; }
    public DelegateCommand StationResetCommand { get; }
    public DelegateCommand StationStartCommand { get; }
    public DelegateCommand StationStopCommand { get; }
    public DelegateCommand StopLoadCommand { get; }
    public DelegateCommand ResumeLoadCommand { get; }
    public DelegateCommand TransferCommand { get; }
    public DelegateCommand PressJogUpCommand { get; }
    public DelegateCommand ReleaseJogUpCommand { get; }
    public DelegateCommand PressJogDownCommand { get; }
    public DelegateCommand ReleaseJogDownCommand { get; }
    public DelegateCommand LoadPort1SetCommand => _loadPort1PositionItem.SetCommand;
    public DelegateCommand LoadPort2SetCommand => _loadPort2PositionItem.SetCommand;
    public DelegateCommand LoadPort3SetCommand => _loadPort3PositionItem.SetCommand;
    public DelegateCommand LoadPort4SetCommand => _loadPort4PositionItem.SetCommand;
    public DelegateCommand LoadPort5SetCommand => _loadPort5PositionItem.SetCommand;
    public DelegateCommand LoadPort1CancelCommand => _loadPort1PositionItem.CancelCommand;
    public DelegateCommand LoadPort2CancelCommand => _loadPort2PositionItem.CancelCommand;
    public DelegateCommand LoadPort3CancelCommand => _loadPort3PositionItem.CancelCommand;
    public DelegateCommand LoadPort4CancelCommand => _loadPort4PositionItem.CancelCommand;
    public DelegateCommand LoadPort5CancelCommand => _loadPort5PositionItem.CancelCommand;
    public DelegateCommand MiddleModuleSetCommand => _middleModulePositionItem.SetCommand;
    public DelegateCommand MiddleModuleCancelCommand => _middleModulePositionItem.CancelCommand;
    public DelegateCommand DryASetCommand => _dryAPositionItem.SetCommand;
    public DelegateCommand DryACancelCommand => _dryAPositionItem.CancelCommand;
    public DelegateCommand DryBSetCommand => _dryBPositionItem.SetCommand;
    public DelegateCommand DryBCancelCommand => _dryBPositionItem.CancelCommand;
    public DelegateCommand JogSpeedSetCommand => _jogSpeedItem.SetCommand;
    public DelegateCommand JogSpeedCancelCommand => _jogSpeedItem.CancelCommand;
    public DelegateCommand AutoSpeedSetCommand => _autoSpeedItem.SetCommand;
    public DelegateCommand AutoSpeedCancelCommand => _autoSpeedItem.CancelCommand;
    public DelegateCommand SourceBoxNoSetCommand => _sourceBoxNoItem.SetCommand;
    public DelegateCommand SourceBoxNoCancelCommand => _sourceBoxNoItem.CancelCommand;
    public DelegateCommand SourceLayerSetCommand => _sourceLayerItem.SetCommand;
    public DelegateCommand SourceLayerCancelCommand => _sourceLayerItem.CancelCommand;
    public DelegateCommand TargetBoxNoSetCommand => _targetBoxNoItem.SetCommand;
    public DelegateCommand TargetBoxNoCancelCommand => _targetBoxNoItem.CancelCommand;
    public DelegateCommand TargetLayerSetCommand => _targetLayerItem.SetCommand;
    public DelegateCommand TargetLayerCancelCommand => _targetLayerItem.CancelCommand;

    // Toggle Commands for View Binding
    public DelegateCommand LoadPort1MoveCommand { get; }
    public DelegateCommand LoadPort2MoveCommand { get; }
    public DelegateCommand LoadPort3MoveCommand { get; }
    public DelegateCommand LoadPort4MoveCommand { get; }
    public DelegateCommand LoadPort5MoveCommand { get; }
    public DelegateCommand MiddleModuleMoveCommand { get; }
    public DelegateCommand DryAMoveCommand { get; }
    public DelegateCommand DryBMoveCommand { get; }
    public DelegateCommand EnableCommand { get; }
    public DelegateCommand OriginCommand { get; }
    public DelegateCommand EmergencyStopCommand { get; }
    public DelegateCommand RobotSafePositionCommand { get; }

    public DelegateCommand WorkAreaPickupCommand { get; }
    public DelegateCommand WorkAreaPlaceCommand { get; }
    public DelegateCommand WaitConfirmCommand { get; }

    public DelegateCommand PickFromBoxToggleCommand { get; }
    public DelegateCommand PlaceToModuleToggleCommand { get; }
    public DelegateCommand PickFromDryAToggleCommand { get; }
    public DelegateCommand PickFromDryBToggleCommand { get; }
    public DelegateCommand PlaceToBoxToggleCommand { get; }
    public DelegateCommand TransferToggleCommand { get; }

    public DelegateCommand StartToggleCommand { get; }
    public DelegateCommand PauseToggleCommand { get; }
    public DelegateCommand StopToggleCommand { get; }
    public DelegateCommand ResetToggleCommand { get; }

    public DelegateCommand AutoModeToggleCommand { get; }
    public DelegateCommand ManualModeToggleCommand { get; }

    public DelegateCommand UseUpperArmToggleCommand { get; }
    public DelegateCommand UseLowerArmToggleCommand { get; }
    public DelegateCommand ConfirmSwitchToggleCommand { get; }

    public DelegateCommand BoxPickupToggleCommand { get; }
    public DelegateCommand BoxPlaceToggleCommand { get; }
    public DelegateCommand FlipTo0ToggleCommand { get; }
    public DelegateCommand FlipTo180ToggleCommand { get; }

    #endregion

    private string? _tabTitle;

    public new string? TabTitle
    {
        get => _tabTitle ?? base.TabTitle;
        set => SetProperty(ref _tabTitle, value);
    }

    public FeedingRobotControlViewModel() : base(0, "上料机器人")
    {
        _plc = GlobalService.RobotControlPlc;
        _editableItems = new List<EditableFloatItem>
        {
            _loadPort1PositionItem,
            _loadPort2PositionItem,
            _loadPort3PositionItem,
            _loadPort4PositionItem,
            _loadPort5PositionItem,
            _middleModulePositionItem,
            _dryAPositionItem,
            _dryBPositionItem,
            _jogSpeedItem,
            _autoSpeedItem
        };
        EditableFloatItemHelper.InitializeCommands(_editableItems, _plc, RaisePropertyChanged);
        _editableIntItems = new List<EditableIntItem>
        {
            _sourceBoxNoItem,
            _sourceLayerItem,
            _targetBoxNoItem,
            _targetLayerItem
        };
        EditableIntItemHelper.InitializeCommands(
            _editableIntItems,
            (address, value) => _plc.Write(address, (int)(value * 1000)),
            RaisePropertyChanged);

        // 初始化定时器
        _readTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _readTimer.Tick += ReadTimer_Tick;
        RegisterLifecycleTimer(_readTimer);
        RegisterViewLifecycleAction(
            () => Logger.For("RobotControl").Debug("FeedingRobotControlViewModel activated, started PLC read timer."),
            () => Logger.For("RobotControl")
                .Debug("FeedingRobotControlViewModel deactivated, stopped PLC read timer."));

        StartCommand = OperationAudit.Create("Start", ExecuteStart, () => "FeedingRobot");
        StopCommand = OperationAudit.Create("Stop", ExecuteStop, () => "FeedingRobot");
        ResetCommand = OperationAudit.Create("Reset", ExecuteReset, () => "FeedingRobot");
        ModulePositionCommand = OperationAudit.Create("ModulePosition", ExecuteModulePosition, () => "FeedingRobot");
        DryACommand = OperationAudit.Create("DryA", ExecuteDryA, () => "FeedingRobot");
        DryBCommand = OperationAudit.Create("DryB", ExecuteDryB, () => "FeedingRobot");
        CacheBodyCommand = OperationAudit.Create("CacheBody", ExecuteCacheBody, () => "FeedingRobot");
        SaveCommand = OperationAudit.Create("Save", ExecuteSave, () => "FeedingRobot");
        StationOperationCommand = new DelegateCommand(ExecuteStationOperation);
        BackCommand = new DelegateCommand(ExecuteBack);
        AlarmResetCommand = OperationAudit.Create("AlarmReset", ExecuteAlarmReset, () => "FeedingRobot");
        BackToRobotCommand = OperationAudit.Create("BackToRobot", ExecuteBackToRobot, () => "FeedingRobot");
        CacheADoorOpenCommand = OperationAudit.Create("CacheADoorOpen", ExecuteCacheADoorOpen, () => "FeedingRobot");
        CacheADoorCloseCommand = OperationAudit.Create("CacheADoorClose", ExecuteCacheADoorClose, () => "FeedingRobot");
        CacheBDoorOpenCommand = OperationAudit.Create("CacheBDoorOpen", ExecuteCacheBDoorOpen, () => "FeedingRobot");
        CacheBDoorCloseCommand = OperationAudit.Create("CacheBDoorClose", ExecuteCacheBDoorClose, () => "FeedingRobot");
        BackFromCacheBodyCommand = OperationAudit.Create("BackFromCacheBody", ExecuteBackFromCacheBody, () => "FeedingRobot");
        StationResetCommand = OperationAudit.Create("StationReset", ExecuteStationReset, () => "FeedingRobot");
        StationStartCommand = OperationAudit.Create("StationStart", ExecuteStationStart, () => "FeedingRobot");
        StationStopCommand = OperationAudit.Create("StationStop", ExecuteStationStop, () => "FeedingRobot");
        StopLoadCommand = OperationAudit.Create("StopLoad", () => _plc.Write(FeedingRobotAddress.StopLoad, !StopLoadToggle), () => "FeedingRobot");
        ResumeLoadCommand = OperationAudit.Create("ResumeLoad", () => _plc.Write(FeedingRobotAddress.ResumeLoad, !ResumeLoadToggle), () => "FeedingRobot");
        TransferCommand = OperationAudit.Create("Transfer", ExecuteTransfer, () => "FeedingRobot");
        PressJogUpCommand = new DelegateCommand(ExecutePressJogUp);
        ReleaseJogUpCommand = new DelegateCommand(ExecuteReleaseJogUp);
        PressJogDownCommand = new DelegateCommand(ExecutePressJogDown);
        ReleaseJogDownCommand = new DelegateCommand(ExecuteReleaseJogDown);

        // Toggle Button Commands
        LoadPort1MoveCommand =
            OperationAudit.Create("LoadPort1Move", () => _plc.Write(FeedingRobotAddress.LoadPort1Move, !LoadPort1MoveToggle), () => "FeedingRobot");
        LoadPort2MoveCommand =
            OperationAudit.Create("LoadPort2Move", () => _plc.Write(FeedingRobotAddress.LoadPort2Move, !LoadPort2MoveToggle), () => "FeedingRobot");
        LoadPort3MoveCommand =
            OperationAudit.Create("LoadPort3Move", () => _plc.Write(FeedingRobotAddress.LoadPort3Move, !LoadPort3MoveToggle), () => "FeedingRobot");
        LoadPort4MoveCommand =
            OperationAudit.Create("LoadPort4Move", () => _plc.Write(FeedingRobotAddress.LoadPort4Move, !LoadPort4MoveToggle), () => "FeedingRobot");
        LoadPort5MoveCommand =
            OperationAudit.Create("LoadPort5Move", () => _plc.Write(FeedingRobotAddress.LoadPort5Move, !LoadPort5MoveToggle), () => "FeedingRobot");
        MiddleModuleMoveCommand =
            OperationAudit.Create("MiddleModuleMove", () => _plc.Write(FeedingRobotAddress.MiddleModuleMove, !MiddleModuleMoveToggle), () => "FeedingRobot");
        DryAMoveCommand = OperationAudit.Create("DryAMove", () => _plc.Write(FeedingRobotAddress.DryAMove, !DryAMoveToggle), () => "FeedingRobot");
        DryBMoveCommand = OperationAudit.Create("DryBMove", () => _plc.Write(FeedingRobotAddress.DryBMove, !DryBMoveToggle), () => "FeedingRobot");
        EnableCommand = OperationAudit.Create("Enable", () => _plc.Write(FeedingRobotAddress.Enable, !IsEnabled), () => "FeedingRobot");
        OriginCommand = OperationAudit.Create("Origin", () =>
        {
            if (MessageBox.Show(Lang.Resources.ConfirmActionContent, Lang.Resources.ConfirmActionTips,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _plc.Write(FeedingRobotAddress.Origin, !OriginToggle);
            }

            OriginToggle = false;
        }, () => "FeedingRobot");
        EmergencyStopCommand =
            OperationAudit.Create("EmergencyStop", () => _plc.Write(FeedingRobotAddress.EmergencyStop, !EmergencyStopToggle), () => "FeedingRobot");
        RobotSafePositionCommand = OperationAudit.Create("RobotSafePosition", () =>
            _plc.WriteDBit(FeedingRobotAddress.RobotSafePosition, !RobotSafePositionToggle), () => "FeedingRobot");

        WorkAreaPickupCommand =
            OperationAudit.Create("WorkAreaPickup", () => _plc.Write(FeedingRobotAddress.WorkAreaPickup, !WorkAreaPickupToggle), () => "FeedingRobot");
        WorkAreaPlaceCommand =
            OperationAudit.Create("WorkAreaPlace", () => _plc.Write(FeedingRobotAddress.WorkAreaPlace, !WorkAreaPlaceToggle), () => "FeedingRobot");
        WaitConfirmCommand = OperationAudit.Create("WaitConfirm", () => _plc.Write(FeedingRobotAddress.WaitConfirm, !WaitConfirmToggle), () => "FeedingRobot");

        // Station Operation Toggle Commands
        PickFromBoxToggleCommand =
            OperationAudit.Create("PickFromBoxToggle", () =>
            {
                _plc.Write(FeedingRobotAddress.PickFromBox, true);
                Thread.Sleep(50);
                _plc.Write(FeedingRobotAddress.PickFromBox, false);
            }, () => "FeedingRobot");
        PlaceToModuleToggleCommand =
            OperationAudit.Create("PlaceToModuleToggle", () =>
            {
                _plc.Write(FeedingRobotAddress.PlaceToModule, true);
                Thread.Sleep(50);
                _plc.Write(FeedingRobotAddress.PlaceToModule, false);
            }, () => "FeedingRobot");
        PickFromDryAToggleCommand =
            OperationAudit.Create("PickFromDryAToggle", () =>
            {
                _plc.Write(FeedingRobotAddress.PickFromDryA, true);
                Thread.Sleep(50);
                _plc.Write(FeedingRobotAddress.PickFromDryA, false);
            }, () => "FeedingRobot");
        PickFromDryBToggleCommand =
            OperationAudit.Create("PickFromDryBToggle", () =>
            {
                _plc.Write(FeedingRobotAddress.PickFromDryB, true);
                Thread.Sleep(50);
                _plc.Write(FeedingRobotAddress.PickFromDryB, false);
            }, () => "FeedingRobot");
        PlaceToBoxToggleCommand =
            OperationAudit.Create("PlaceToBoxToggle", () =>
            {
                _plc.Write(FeedingRobotAddress.PlaceToBox, true);
                Thread.Sleep(50);                
                _plc.Write(FeedingRobotAddress.PlaceToBox, false);
            }, () => "FeedingRobot");
        TransferToggleCommand = OperationAudit.Create("TransferToggle", () =>
        {
            _plc.Write(FeedingRobotAddress.Transfer, true);
            Thread.Sleep(50);
            _plc.Write(FeedingRobotAddress.Transfer, false);
        }, () => "FeedingRobot");

        // Robot Control Toggle Commands
        StartToggleCommand = OperationAudit.Create("StartToggle", () => _plc.Write(FeedingRobotAddress.Start, !StartToggle), () => "FeedingRobot");
        PauseToggleCommand = OperationAudit.Create("PauseToggle", () => _plc.Write(FeedingRobotAddress.Pause, !PauseToggle), () => "FeedingRobot");
        StopToggleCommand = OperationAudit.Create("StopToggle", () => _plc.Write(FeedingRobotAddress.Stop, !StopToggle), () => "FeedingRobot");
        ResetToggleCommand = OperationAudit.Create("ResetToggle", () => _plc.Write(FeedingRobotAddress.Reset, !ResetToggle), () => "FeedingRobot");

        // Auto/Manual are special cases, usually radio behavior but implemented as toggles often
        AutoModeToggleCommand = OperationAudit.Create("AutoModeToggle", () => _plc.Write(FeedingRobotAddress.AutoMode, !AutoModeToggle), () => "FeedingRobot");
        ManualModeToggleCommand =
            OperationAudit.Create("ManualModeToggle", () => _plc.Write(FeedingRobotAddress.ManualMode, !ManualModeToggle), () => "FeedingRobot");

        UseUpperArmToggleCommand =
            OperationAudit.Create("UseUpperArmToggle", () => _plc.Write(FeedingRobotAddress.UseUpperArm, !UseUpperArmToggle), () => "FeedingRobot");
        UseLowerArmToggleCommand =
            OperationAudit.Create("UseLowerArmToggle", () => _plc.Write(FeedingRobotAddress.UseLowerArm, !UseLowerArmToggle), () => "FeedingRobot");
        ConfirmSwitchToggleCommand =
            OperationAudit.Create("ConfirmSwitchToggle", () => _plc.Write(FeedingRobotAddress.ConfirmSwitch, !ConfirmSwitchToggle), () => "FeedingRobot");

        BoxPickupToggleCommand = OperationAudit.Create("BoxPickupToggle", () => _plc.Write(FeedingRobotAddress.BoxPickup, !BoxPickupToggle), () => "FeedingRobot");
        BoxPlaceToggleCommand = OperationAudit.Create("BoxPlaceToggle", () => _plc.Write(FeedingRobotAddress.BoxPlace, !BoxPlaceToggle), () => "FeedingRobot");
        FlipTo0ToggleCommand = OperationAudit.Create("FlipTo0Toggle", () => _plc.Write(FeedingRobotAddress.FlipTo0, !FlipTo0Toggle), () => "FeedingRobot");
        FlipTo180ToggleCommand = OperationAudit.Create("FlipTo180Toggle", () => _plc.Write(FeedingRobotAddress.FlipTo180, !FlipTo180Toggle), () => "FeedingRobot");
    }

    #region PLC 读取

    private async void ReadTimer_Tick(object? sender, EventArgs e)
    {
        if (_isReading)
        {
            return;
        }

        if (!_plc.IsConnected)
        {
            _plc.Connect("192.168.2.50", 7003);
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
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var mBatchGroups = new List<List<PlcBatchReadItemBase>>
            {
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.GlobalAlarm, assign: v => GlobalAlarm = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.StopLoadStatus, assign: v => StopLoadToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.ResumeLoadStatus, assign: v => ResumeLoadToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.SideAHealthy, assign: v => SideAHealthy = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.SideBHealthy, assign: v => SideBHealthy = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.EmergencyStop, assign: v => EmergencyStopToggle = v)
                },
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.Transfer, assign: v => TransferToggle = v)
                },
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.Origin, assign: v => OriginToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.LoadPort1Move, assign: v => LoadPort1MoveToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.LoadPort2Move, assign: v => LoadPort2MoveToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.LoadPort3Move, assign: v => LoadPort3MoveToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.LoadPort4Move, assign: v => LoadPort4MoveToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.LoadPort5Move, assign: v => LoadPort5MoveToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.MiddleModuleMove, assign: v => MiddleModuleMoveToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.DryAMove, assign: v => DryAMoveToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.DryBMove, assign: v => DryBMoveToggle = v)
                },
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.Pause, assign: v => PauseToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.AutoMode, assign: v => AutoModeToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.UseUpperArm, assign: v => UseUpperArmToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.UseLowerArm, assign: v => UseLowerArmToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.BoxPickup, assign: v => BoxPickupToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.BoxPlace, assign: v => BoxPlaceToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.WorkAreaPickup, assign: v => WorkAreaPickupToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.WorkAreaPlace, assign: v => WorkAreaPlaceToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.FlipTo0, assign: v => FlipTo0Toggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.FlipTo180, assign: v => FlipTo180Toggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.WaitConfirm, assign: v => WaitConfirmToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.ConfirmSwitch, assign: v => ConfirmSwitchToggle = v)
                },
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.ManualMode, assign: v => ManualModeToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromBox, assign: v => PickFromBoxToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PlaceToModule, assign: v => PlaceToModuleToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromDryA, assign: v => PickFromDryAToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromDryB, assign: v => PickFromDryBToggle = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PlaceToBox, assign: v => PlaceToBoxToggle = v)
                },
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.EnableStatus, assign: v => IsEnabled = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.OriginStatus, assign: v => IsAtOrigin = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.JogUpStatus, assign: v => JogUpActive = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.JogDownStatus, assign: v => JogDownActive = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.JogMiddleStatus, assign: v => JogMiddleActive = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.LimitAlarm, assign: v => LimitAlarm = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.DriverAlarm, assign: v => DriverAlarm = v)
                },
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.RunningStatus, assign: v => IsRunning = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PausedStatus, assign: v => IsPaused = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.StoppedStatus, assign: v => IsStopped = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.ResetStatus, assign: v => IsReset = v)
                },
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.BoxPickupStatus, assign: v => BoxPickup = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.BoxPlaceStatus, assign: v => BoxPlace = v)
                },
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.WorkAreaPickupStatus, assign: v => WorkAreaPickup = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.WorkAreaPlaceStatus, assign: v => WorkAreaPlace = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.FlipTo0Status, assign: v => FlipTo0 = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.FlipTo180Status, assign: v => FlipTo180 = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.WaitConfirmStatus, assign: v => WaitConfirm = v)
                },
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.StationResetStatus, assign: v => StationIsReset = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.StationRunningStatus, assign: v => StationIsRunning = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.StationStoppedStatus, assign: v => StationIsStopped = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.ModuleAlarm, assign: v => ModuleAlarm = v)
                },
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromBoxActiveStatus, assign: v => PickFromBoxActive = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PlaceToModuleActiveStatus, assign: v => PlaceToModuleActive = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromDryAActiveStatus, assign: v => PickFromDryAActive = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromDryBActiveStatus, assign: v => PickFromDryBActive = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PlaceToBoxActiveStatus, assign: v => PlaceToBoxActive = v)
                },
                new()
                {
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromBoxDoneStatus, assign: v => PickFromBoxDone = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PlaceToModuleDoneStatus, assign: v => PlaceToModuleDone = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromDryADoneStatus, assign: v => PickFromDryADone = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PickFromDryBDoneStatus, assign: v => PickFromDryBDone = v),
                    new PlcBatchReadItem<bool>(FeedingRobotAddress.PlaceToBoxDoneStatus, assign: v => PlaceToBoxDone = v)
                }
            };

            var dStatusBatchItems = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<int>(FeedingRobotAddress.CurrentCoordinate, assign: v => CurrentCoordinate = v / 1000.0),
                new PlcBatchReadItem<int>(FeedingRobotAddress.AlarmCode, assign: v => AlarmCode = v)
            };

            // B 设备（不能与 M 合并）
            var t_b0B = _plc.ReadAsync<bool>(FeedingRobotAddress.AlarmReset); // B0B

            var dBitBatchItems = new List<PlcBatchReadItemBase>
            {
                PlcBatchReadItem<bool>.Bind(FeedingRobotAddress.AutoModeStatus.Address, () => AutoModeActive,
                    converter: MitsubishiPlcService.ToBitConverter(FeedingRobotAddress.AutoModeStatus)),
                PlcBatchReadItem<bool>.Bind(FeedingRobotAddress.UpperArmSensor.Address, () => UpperArmSensor,
                    converter: MitsubishiPlcService.ToBitConverter(FeedingRobotAddress.UpperArmSensor)),
                PlcBatchReadItem<bool>.Bind(FeedingRobotAddress.LowerArmSensor.Address, () => LowerArmSensor,
                    converter: MitsubishiPlcService.ToBitConverter(FeedingRobotAddress.LowerArmSensor)),
                PlcBatchReadItem<bool>.Bind(FeedingRobotAddress.CommandAccept.Address, () => CommandAccepted,
                    converter: MitsubishiPlcService.ToBitConverter(FeedingRobotAddress.CommandAccept)),
                PlcBatchReadItem<bool>.Bind(FeedingRobotAddress.ConfirmSwitchStatus.Address, () => ConfirmSwitchActive,
                    converter: MitsubishiPlcService.ToBitConverter(FeedingRobotAddress.ConfirmSwitchStatus)),
                PlcBatchReadItem<bool>.Bind(FeedingRobotAddress.RobotSafePosition.Address, () => RobotSafePosition,
                    converter: MitsubishiPlcService.ToBitConverter(FeedingRobotAddress.RobotSafePosition)),
                PlcBatchReadItem<bool>.Bind(FeedingRobotAddress.UseUpperArmStatus.Address, () => UseUpperArmActive,
                    converter: MitsubishiPlcService.ToBitConverter(FeedingRobotAddress.UseUpperArmStatus)),
                PlcBatchReadItem<bool>.Bind(FeedingRobotAddress.UseLowerArmStatus.Address, () => UseLowerArmActive,
                    converter: MitsubishiPlcService.ToBitConverter(FeedingRobotAddress.UseLowerArmStatus))
            };
            var t_dBits = _plc.BatchReadAsync(dBitBatchItems);

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
            var t_editable = _plc.BatchReadAsync(editableBatchItems);
            var editableIntBatchItems = _editableIntItems
                .Select(item => (PlcBatchReadItemBase)new PlcBatchReadItem<int>(item.Address, assign: v =>
                {
                    item.PlcValue = v / 1000;
                    item.HasPlcValue = true;

                    if (item.IsDirty)
                    {
                        return;
                    }

                    item.Value = v / 1000;
                    RaisePropertyChanged(item.ValuePropertyName);
                }))
                .ToList();
            var t_editableInt = _plc.BatchReadAsync(editableIntBatchItems);
            var mBatchTasks = mBatchGroups.Select(group => _plc.BatchReadAsync(group)).ToList();
            var t_dStatus = _plc.BatchReadAsync(dStatusBatchItems);

            var allTasks = new List<Task>
            {
                t_dStatus,
                t_b0B,
                t_dBits,
                t_editable,
                t_editableInt
            };
            allTasks.AddRange(mBatchTasks);

            await Task.WhenAll(allTasks);

            for (var i = 0; i < mBatchTasks.Count; i++)
            {
                var task = mBatchTasks[i];
                if (!task.Result.IsSuccess)
                    Logger.For("RobotControl").Warn($"[FeedingRobot] M batch read failed (group {i + 1}): {task.Result.Message}");
            }

            if (!t_dStatus.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[FeedingRobot] D status batch read failed: {t_dStatus.Result.Message}");

            // B0B: AlarmReset
            if (t_b0B.Result.IsSuccess) AlarmResetToggle = t_b0B.Result.Content;

            if (!t_dBits.Result.IsSuccess)
            {
                Logger.For("RobotControl").Warn($"[FeedingRobot] D bit batch read failed: {t_dBits.Result.Message}");
            }
            else
            {
                foreach (var item in dBitBatchItems)
                {
                    if (!item.IsSuccess)
                        Logger.For("RobotControl").Warn($"[FeedingRobot] {item.Address} D bit read failed: {item.OriginResult.Message}");
                }

                RobotSafePositionToggle = RobotSafePosition;
            }

            if (!t_editable.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[FeedingRobot] D editable batch read failed: {t_editable.Result.Message}");

            if (!t_editableInt.Result.IsSuccess)
                Logger.For("RobotControl").Warn($"[FeedingRobot] W editable batch read failed: {t_editableInt.Result.Message}");
        }
        finally
        {
            sw.Stop();
            Logger.For("RobotControl").Info($"[FeedingRobot] ReadPlcDataAsync elapsed: {sw.ElapsedMilliseconds} ms");
        }
    }

    public void StopReading()
    {
        OnViewDeactivated();
    }

    #endregion

    #region Command 实现

    private void ExecuteStart()
    {
        _plc.Write(FeedingRobotAddress.Start, true);
    }

    private void ExecuteStop()
    {
        _plc.Write(FeedingRobotAddress.Stop, true);
    }

    private void ExecuteReset()
    {
        _plc.Write(FeedingRobotAddress.Reset, true);
    }

    private void ExecuteModulePosition()
    {
        _plc.Write(FeedingRobotAddress.ModulePosition, 0);
    }

    private void ExecuteDryA()
    {
        _plc.Write(FeedingRobotAddress.DryACommand, 1);
    }

    private void ExecuteDryB()
    {
        _plc.Write(FeedingRobotAddress.DryBCommand, 2);
    }


    private void ExecuteCacheBody()
    {
        CurrentViewIndex = 2;
    }

    private void ExecuteSave()
    {
        _plc.Write(FeedingRobotAddress.Save, true);
        Growl.InfoGlobal("保存配置");
    }

    private void ExecuteStationOperation()
    {
        CurrentViewIndex = 1;
    }

    private void ExecuteBack()
    {
        CurrentViewIndex = 0;
    }

    private void ExecuteBackToRobot()
    {
        CurrentViewIndex = 0;
    }

    private void ExecuteAlarmReset()
    {
        _plc.Write(FeedingRobotAddress.ResetAlarm, true);
        ModuleAlarm = false;
        GlobalAlarm = false;
    }


    private void ExecuteCacheADoorOpen()
    {
        CacheADoorOpenStatus = !CacheADoorOpenStatus;
    }

    private void ExecuteCacheADoorClose()
    {
        CacheADoorCloseStatus = !CacheADoorCloseStatus;
    }

    private void ExecuteCacheBDoorOpen()
    {
        CacheBDoorOpenStatus = !CacheBDoorOpenStatus;
    }

    private void ExecuteCacheBDoorClose()
    {
        CacheBDoorCloseStatus = !CacheBDoorCloseStatus;
    }

    private void ExecuteBackFromCacheBody()
    {
        CurrentViewIndex = 0;
    }

    private void ExecuteStationReset()
    {
        _plc.Write(FeedingRobotAddress.StationReset, true);
    }

    private void ExecuteStationStart()
    {
        _plc.Write(FeedingRobotAddress.StationStart, true);
    }

    private void ExecuteStationStop()
    {
        _plc.Write(FeedingRobotAddress.StationStop, true);
    }

    private void ExecuteTransfer()
    {
        _plc.Write(FeedingRobotAddress.Transfer, true);
    }

    private void ExecutePressJogUp()
    {
        _plc.Write(FeedingRobotAddress.JogUp, true);
    }

    private void ExecuteReleaseJogUp()
    {
        _plc.Write(FeedingRobotAddress.JogUp, false);
    }

    private void ExecutePressJogDown()
    {
        _plc.Write(FeedingRobotAddress.JogDown, true);
    }

    private void ExecuteReleaseJogDown()
    {
        _plc.Write(FeedingRobotAddress.JogDown, false);
    }

    #endregion
}
