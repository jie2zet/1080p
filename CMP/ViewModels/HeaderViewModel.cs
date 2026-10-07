using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CMP;
using CMP.Models.DataCenter;
using CMP.Models.Operation.LoadPort;
using CMP.Models.Resource;
using CMP.Models.Warn;
using CMP.Services.Auth;
using CMP.Services.LoadPort;
using CMP.Services.Log;
using CMP.Services.Plc;
using CMP.ViewModels.Common.Dialog;
using CMP.Views;
using CMP.Views.Common.Dialog;
using Core.Common;
using HandyControl.Controls;
using WPFLocalizeExtension.Engine;

namespace CMP.ViewModels;

public class HeaderViewModel : BindableBase
{
    private readonly IAuthService _authService;
    private readonly IDialogService _dialogService;
    /// <summary>???? UI ???????? PLC?????????????????????????</summary>
    private static void WritePlcBoolFireAndForget(string address, bool value)
    {
        try
        {
            var plc = GlobalService.plcComm;
            if (plc is not { IsPLCConnected: true })
                return;
            _ = Task.Run(() =>
            {
                try
                {
                    plc.WriteBoolVari(address, value);
                }
                catch (Exception ex)
                {
                    Logger.Error($"PLC ?? {address} ???: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            Logger.Error($"WritePlcBoolFireAndForget: {ex.Message}");
        }
    }

    private string? _currentDate;
    private string? _currentTime;
    private Dictionary<string, string> _languageList = Languages.LanguageList;

    private object _selectedLanguage = Languages.LanguageList.FirstOrDefault().Key;
    private ObservableCollection<Alarm> _listAlarms = new();
    public ObservableCollection<Alarm> ListAlarm
    {
        get => _listAlarms;
        set => SetProperty(ref _listAlarms, value);
    }

    private string _currentUserName = "-";
    private string _currentGroup = "-";

    public HeaderViewModel(IAuthService authService, IDialogService dialogService)
    {
        _authService = authService;
        _dialogService = dialogService;
        SessionContext.Instance.PropertyChanged += OnSessionContextChanged;
        RefreshSessionInfo();
        RefreshDateTime();
        // UI ????????????? System.Timers.Timer ??????????????????
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (_, _) =>
        {
            RefreshDateTime();
            RefreshAlarmData();
            CheckAlarmStatus();
            UpdateTowerLights();
        };
        timer.Start();
        if (HasAlarm) IsSoundOnChecked = true;
    }

    public string CurrentUserName
    {
        get => _currentUserName;
        set => SetProperty(ref _currentUserName, value);
    }

    public string CurrentGroup
    {
        get => _currentGroup;
        set => SetProperty(ref _currentGroup, value);
    }

    public DelegateCommand LogoutCommand => new(ExecuteLogout);

    public DelegateCommand ChangePasswordCommand => new(ExecuteChangePassword);

    private void ExecuteChangePassword()
    {
        if (!SessionContext.Instance.IsAuthenticated)
        {
            return;
        }

        _dialogService.ShowDialog(nameof(ChangePasswordDialogView));
    }

    private void ExecuteLogout()
    {
        if (Application.Current.MainWindow is MainView mainView)
        {
            mainView.RequireReLogin(LogoutReason.Manual);
            RefreshSessionInfo();
        }
        else
        {
            _authService.Logout(LogoutReason.Manual);
            Application.Current.Shutdown();
        }
    }

    private void OnSessionContextChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SessionContext.UserName)
            or nameof(SessionContext.LevelDisplay)
            or nameof(SessionContext.IsAuthenticated))
        {
            RefreshSessionInfo();
        }
    }

    private void RefreshSessionInfo()
    {
        if (SessionContext.Instance.IsAuthenticated)
        {
            CurrentUserName = SessionContext.Instance.UserName;
            CurrentGroup = SessionContext.Instance.LevelDisplay;
        }
        else
        {
            CurrentUserName = "-";
            CurrentGroup = "-";
        }
    }

    public string? CurrentDate
    {
        get => _currentDate;
        set => SetProperty(ref _currentDate, value);
    }

    public string? CurrentTime
    {
        get => _currentTime;
        set => SetProperty(ref _currentTime, value);
    }

    public Dictionary<string, string> LanguageList
    {
        get => _languageList;
        set => SetProperty(ref _languageList, value);
    }

    public object SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (!SetProperty(ref _selectedLanguage, value)) return;
            // get lang code via dictionary
            var langCode = LanguageList.FirstOrDefault(x => x.Key == value.ToString()).Value;
            LocalizeDictionary.Instance.Culture =
                CultureInfo.GetCultureInfo(langCode ?? throw new InvalidOperationException());
        }
    }

    // ???????????
    private void RefreshAlarmData()
    {
        try
        {
            var dataCenter = GlobalService.DataCenter;
            // ???DataCenter?????????
            if (dataCenter == null)
            {
                Logger.Info("DataCenter????????????");
                return;
            }

            // DispatcherTimer ???? UI ????????????? CheckAlarmStatus ??????
            try
            {
                ListAlarm?.Clear();
                var latestAlarms = dataCenter.GetLatestAlarms();
                foreach (var alarm in latestAlarms)
                    ListAlarm?.Add(alarm);
            }
            catch (Exception ex)
            {
                Logger.Error($"?????????????????: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            // ????????
           Logger.Error($"????????????????: {ex.Message}");
        }
    }

    /// <summary>
    ///     Exit Command
    /// </summary>
    /// <returns></returns>
    public DelegateCommand ExitCommand => new(Exit);

    private void Exit()
    {
        // Exit Application
        Application.Current.Shutdown();
    }

    private void RefreshDateTime()
    {
        CurrentDate = $"{DateTime.Now:yyyy-MM-dd}";
        CurrentTime = $"{DateTime.Now:HH:mm:ss}";
    }

    private bool _isSoundOffChecked;
    private bool _isSoundOnChecked = true;
    // ????????????????
    private bool _wasAlarmCleared = false;
    private HashSet<string> _previousAlarmIds = new HashSet<string>();

    // ?????????????????????????ID?????
    private HashSet<string> _alarmsThatTriggeredBuzzer = new HashSet<string>();

    public bool IsSoundOffChecked
    {
        get => _isSoundOffChecked;
        set
        {
            if (SetProperty(ref _isSoundOffChecked, value))
            {
                if (value)
                {
                    // ??Off???????????On???????
                    IsSoundOnChecked = false;
                    // ???????????????
                    TurnOffSound();
                }
            }
        }
    }

    public bool IsSoundOnChecked
    {
        get => _isSoundOnChecked;
        set
        {
            if (SetProperty(ref _isSoundOnChecked, value))
            {
                if (value)
                {
                    // ??On???????????Off???????
                    IsSoundOffChecked = false;
                    // ??????????????
                    TurnOnSound();
                }
            }
        }
    }

    // ????????????
    private void TurnOffSound() => WritePlcBoolFireAndForget("M205", false);

    private void TurnOnSound() => WritePlcBoolFireAndForget("M205", true);

    private bool _hasAlarm;
    public bool HasAlarm
    {
        get => _hasAlarm;
        set => SetProperty(ref _hasAlarm, value);
    }

    // ??????????????????????????
    private int _previousAlarmCount = 0;

    // ????????????????????????????
    private bool _hasNewAlarm = false;
    public bool HasNewAlarm
    {
        get => _hasNewAlarm;
        set => SetProperty(ref _hasNewAlarm, value);
    }

    // ????????????????????????????
    private void CheckAlarmStatus()
    {
        try
        {
            // ??????????ID????
            var currentAlarmIds = new HashSet<string>();
            var alarmTypeAlarms = new List<Alarm>();
            if (ListAlarm != null)
            {
                // ????????Alarm???????????????RefreshAlarmData?????????????????????????
                alarmTypeAlarms = ListAlarm.Where(alarm => alarm.Type == "Alarm").ToList();
                foreach (var alarm in alarmTypeAlarms)
                {
                    currentAlarmIds.Add(alarm.AlarmId + "");
                }
            }

            // ????????????
            bool hasActiveAlarm = alarmTypeAlarms != null && alarmTypeAlarms.Count > 0;

            // ???????????????????????????????????
            bool hasNewAlarm = false;
            if (hasActiveAlarm)
            {
                foreach (var id in currentAlarmIds)
                {
                    if (!_previousAlarmIds.Contains(id))
                    {
                        hasNewAlarm = true;
                        break;
                    }
                }
            }

            // ??????????
            bool alarmStatusChanged = HasAlarm != hasActiveAlarm;
            HasAlarm = hasActiveAlarm;

            // ?????????
            if (hasActiveAlarm)
            {
                bool hasNewUntriggeredAlarm = false;
                if (hasActiveAlarm)
                {
                    foreach (var id in currentAlarmIds)
                    {
                        // ???????????????????????????
                        if (!_previousAlarmIds.Contains(id) && !_alarmsThatTriggeredBuzzer.Contains(id))
                        {
                            hasNewUntriggeredAlarm = true;
                            // ????????????????
                            _alarmsThatTriggeredBuzzer.Add(id);
                        }
                    }
                }

                // ??????????????????????????????????????????
                if (hasNewUntriggeredAlarm)
                {
                    TriggerBuzzer();
                    HasNewAlarm = true;
                }
                // ???????????????????????????
                else if (IsSoundOnChecked)
                {
                    WritePlcBoolFireAndForget("M205", true);
                }

            }
            else
            {
                WritePlcBoolFireAndForget("M205", false);
                // ?????????????????????
                HasNewAlarm = false;
                // ????????????????????
                _alarmsThatTriggeredBuzzer.Clear();

                // ????????????????????????????????
                if (HasAlarm && !hasActiveAlarm)
                {
                    _wasAlarmCleared = true;
                }
            }

            // ???????????ID????
            _previousAlarmIds = currentAlarmIds;
            // ???????????????
            _previousAlarmCount = ListAlarm?.Count ?? 0;
        }
        catch (Exception ex)
        {
            //Growl.ErrorGlobal($"???l?????????: {ex.Message}");
        }
    }

    // ???????????????
    private void TriggerBuzzer()
    {
        // ???????????????????????????
        //if (IsSoundOnChecked)
        //{
        WritePlcBoolFireAndForget("M205", true);
        IsSoundOnChecked = true;
        //}
    }

    private static readonly string[] RobotPauseStatusAddresses =
    [
        "M5961",
        "M5993",
        "M6025",
        "M6057",
        "M6089",
        "M6121",
    ];

    private int _pauseReadBusy;
    private bool _equipmentPaused;
    private int _towerWriteBusy;
    private int _lastTowerPlcMask = -1;

    private const int TowerRedBlinkBit = 1 << 0;
    private const int TowerYellowBlinkBit = 1 << 1;
    private const int TowerGreenOnBit = 1 << 2;
    private const int TowerGreenBlinkBit = 1 << 3;

    private string _redLightMode = "Off";
    private string _yellowLightMode = "Off";
    private string _greenLightMode = "Off";

    /// <summary>Off 或 Blink。红灯没有常亮。</summary>
    public string RedLightMode
    {
        get => _redLightMode;
        private set => SetProperty(ref _redLightMode, value);
    }

    /// <summary>Off 或 Blink。黄灯没有常亮。</summary>
    public string YellowLightMode
    {
        get => _yellowLightMode;
        private set => SetProperty(ref _yellowLightMode, value);
    }

    /// <summary>Off、On 或 Blink。</summary>
    public string GreenLightMode
    {
        get => _greenLightMode;
        private set => SetProperty(ref _greenLightMode, value);
    }

    private void UpdateTowerLights()
    {
        if (System.Threading.Interlocked.CompareExchange(ref _pauseReadBusy, 1, 0) == 0)
        {
            _ = Task.Run(() =>
            {
                var paused = ReadAnyRobotPaused();
                var dispatcher = Application.Current?.Dispatcher;
                if (dispatcher == null)
                {
                    _equipmentPaused = paused;
                    System.Threading.Interlocked.Exchange(ref _pauseReadBusy, 0);
                    return;
                }

                dispatcher.BeginInvoke(() =>
                {
                    _equipmentPaused = paused;
                    System.Threading.Interlocked.Exchange(ref _pauseReadBusy, 0);
                    ApplyCurrentTowerLights();
                });
            });
        }

        ApplyCurrentTowerLights();
    }

    private void ApplyCurrentTowerLights()
    {
        ApplyTowerLights(
            HasAlarm,
            HasFoupReadyToRemove(),
            IsEquipmentManual(),
            IsEquipmentRunning(),
            _equipmentPaused);
    }

    private void ApplyTowerLights(bool hasAlarm, bool readyToRemove, bool manual, bool running, bool paused)
    {
        var redBlink = hasAlarm;
        var yellowBlink = readyToRemove;
        var greenBlink = paused || manual;
        var greenOn = running && !greenBlink;

        RedLightMode = redBlink ? "Blink" : "Off";
        YellowLightMode = yellowBlink ? "Blink" : "Off";
        GreenLightMode = greenBlink ? "Blink" : greenOn ? "On" : "Off";

        SyncTowerLightsToPlc(redBlink, yellowBlink, greenOn, greenBlink);
    }

    private void SyncTowerLightsToPlc(bool redBlink, bool yellowBlink, bool greenOn, bool greenBlink)
    {
        var mask = 0;
        if (redBlink) mask |= TowerRedBlinkBit;
        if (yellowBlink) mask |= TowerYellowBlinkBit;
        if (greenOn) mask |= TowerGreenOnBit;
        if (greenBlink) mask |= TowerGreenBlinkBit;
        if (mask == _lastTowerPlcMask)
        {
            return;
        }

        if (System.Threading.Interlocked.CompareExchange(ref _towerWriteBusy, 1, 0) != 0)
        {
            return;
        }

        _ = Task.Run(() =>
        {
            try
            {
                var plc = GlobalService.plcComm;
                if (plc is not { IsPLCConnected: true })
                {
                    _lastTowerPlcMask = -1;
                    return;
                }

                plc.WriteBoolVariContinuous("M201", [false, false, greenOn, false]);
                plc.WriteBoolVariContinuous("M211", [redBlink, yellowBlink, greenBlink, false]);
                _lastTowerPlcMask = mask;
            }
            catch (Exception ex)
            {
                Logger.Error($"写入塔灯失败: {ex.Message}");
            }
            finally
            {
                System.Threading.Interlocked.Exchange(ref _towerWriteBusy, 0);
            }
        });
    }

    private static bool HasFoupReadyToRemove()
    {
        var ports = LoadPortManager.Instance.GetAllLoadPorts();
        if (ports == null)
        {
            return false;
        }

        foreach (var loadPort in ports.Values)
        {
            if (loadPort?.LoadPortStatus?.foupPlacementStatus != FoupPlacementStatus.Normal)
            {
                continue;
            }

            var assigned = loadPort.WaferList?
                .Where(wafer => !string.IsNullOrEmpty(wafer.SequenceRecipe))
                .ToList();
            if (assigned is { Count: > 0 } && assigned.All(wafer => wafer.WaferStatus == WaferStatus.ProcessDone))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsEquipmentRunning()
    {
        var ports = LoadPortManager.Instance.GetAllLoadPorts();
        if (ports == null)
        {
            return false;
        }

        return ports.Values.Any(loadPort =>
            loadPort?.WaferList?.Any(wafer => wafer.WaferStatus == WaferStatus.Processing) == true);
    }

    private static bool IsEquipmentManual()
    {
        var chambers = GlobalService.globalChambers?.listChamber;
        return chambers != null && chambers.Any(chamber =>
            chamber.IsEnabled && chamber.ChamberStatus == ChamberStatus.Manual);
    }

    private static bool ReadAnyRobotPaused()
    {
        var plc = MitsubishiPlcService.RobotCtrlInstance;
        if (!plc.IsPLCConnected)
        {
            return false;
        }

        foreach (var address in RobotPauseStatusAddresses)
        {
            try
            {
                if (plc.ReadBoolVari(address))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"读取机器人暂停状态 {address} 失败: {ex.Message}");
            }
        }

        return false;
    }
}