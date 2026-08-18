using CMP.Services.Auth;
using CMP.Constants.Plc;
using CMP.Extensions;
using CMP.Lang;
using CMP.Models.kvsPlc;
using CMP.Models.kvsPlc.Roll;
using CMP.Models.Operation.Polish;
using CMP.Models.Recipe.RecipeEdit;
using CMP.Models.Recipe.Sequence;
using CMP.Models.Resource;
using CMP.Services.Log;
using CMP.Utils;
using CMP.ViewModels.Common;
using Dm.util;
using DocumentFormat.OpenXml.Wordprocessing;
using HandyControl.Controls;
using NetTaste;
using Newtonsoft.Json;
using Prism.Navigation.Regions.Behaviors;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows;
using System.Windows.Threading;
using System.Xaml;
using static CMP.Models.kvsPlc.SV.KvStruct;
using KHL = CMP.Models.kvsPlc.KvHostLink;
using KST = CMP.Models.kvsPlc.Polish.KvStruct;
using PlcHandle = CMP.Constants.Plc.PlcHandle;
using RecipeData = CMP.Models.Recipe.RecipeEdit.Recipe.Data;

namespace CMP.ViewModels.Tabs.OperationTab;

public class PolishUnitTabContentViewModel : TabContentViewModelBase
{
    private readonly IEventAggregator _eventAggregator;
    private string? _tabTitle;
    private string? _baseChamberAddress;
    public new string? TabTitle
    {
        get => _tabTitle;
        set => SetProperty(ref _tabTitle, value);
    }

    private int _tabId;

    public int TabId
    {
        get => _tabId;
        set => SetProperty(ref _tabId, value);
    }

    /// <summary>
    /// 运行模式 0表示自动模式，1表示手动模式，默认自动模式
    /// </summary>
    private int _runMode = 0;

    private PolishUnit _polishUnit = new();

    public PolishUnit PolishUnit
    {
        get => _polishUnit;
        set => SetProperty(ref _polishUnit, value);
    }

    // Recipe lists for ComboBoxes 
    private ObservableCollection<string> _loadRecipes = new();

    public ObservableCollection<string> LoadRecipes
    {
        get => _loadRecipes;
        set => SetProperty(ref _loadRecipes, value);
    }

    private ObservableCollection<string> _conditionRecipes = new();

    public ObservableCollection<string> ConditionRecipes
    {
        get => _conditionRecipes;
        set => SetProperty(ref _conditionRecipes, value);
    }

    private ObservableCollection<string> _dechuckRecipes = new();

    public ObservableCollection<string> DechuckRecipes
    {
        get => _dechuckRecipes;
        set => SetProperty(ref _dechuckRecipes, value);
    }

    private ObservableCollection<string> _headRecipes = new();

    public ObservableCollection<string> HeadRecipes
    {
        get => _headRecipes;
        set => SetProperty(ref _headRecipes, value);
    }

    private ObservableCollection<string> _polishRecipes = new();

    public ObservableCollection<string> PolishRecipes
    {
        get => _polishRecipes;
        set => SetProperty(ref _polishRecipes, value);
    }

    private ObservableCollection<string> _unloadRecipes = new();

    public ObservableCollection<string> UnloadRecipes
    {
        get => _unloadRecipes;
        set => SetProperty(ref _unloadRecipes, value);
    }

    private ObservableCollection<string> _loadRetryRecipes = new();

    public ObservableCollection<string> LoadRetryRecipes
    {
        get => _loadRetryRecipes;
        set => SetProperty(ref _loadRetryRecipes, value);
    }

    private ObservableCollection<string> _dechuckRetryRecipes = new();

    public ObservableCollection<string> DechuckRetryRecipes
    {
        get => _dechuckRetryRecipes;
        set => SetProperty(ref _dechuckRetryRecipes, value);
    }

    private ObservableCollection<string> _unloadRetryRecipes = new();

    public ObservableCollection<string> UnloadRetryRecipes
    {
        get => _unloadRetryRecipes;
        set => SetProperty(ref _unloadRetryRecipes, value);
    }

    // Selected recipes
    private string? _selectedLoadRecipe;

    public string? SelectedLoadRecipe
    {
        get => _selectedLoadRecipe;
        set => SetProperty(ref _selectedLoadRecipe, value);
    }

    private string? _selectedPreCondRecipe;

    public string? SelectedPreCondRecipe
    {
        get => _selectedPreCondRecipe;
        set => SetProperty(ref _selectedPreCondRecipe, value);
    }

    private string? _selectedConditionRecipe;

    public string? SelectedConditionRecipe
    {
        get => _selectedConditionRecipe;
        set => SetProperty(ref _selectedConditionRecipe, value);
    }

    private string? _selectedDechuckRecipe;

    public string? SelectedDechuckRecipe
    {
        get => _selectedDechuckRecipe;
        set => SetProperty(ref _selectedDechuckRecipe, value);
    }

    private string? _selectedPolishRecipe;

    public string? SelectedPolishRecipe
    {
        get => _selectedPolishRecipe;
        set => SetProperty(ref _selectedPolishRecipe, value);
    }

    private string? _selectedUnloadRecipe;

    public string? SelectedUnloadRecipe
    {
        get => _selectedUnloadRecipe;
        set => SetProperty(ref _selectedUnloadRecipe, value);
    }

    private string? _selectedLoadRetryRecipe;

    public string? SelectedLoadRetryRecipe
    {
        get => _selectedLoadRetryRecipe;
        set => SetProperty(ref _selectedLoadRetryRecipe, value);
    }

    private string? _selectedDechuckRetryRecipe;

    public string? SelectedDechuckRetryRecipe
    {
        get => _selectedDechuckRetryRecipe;
        set => SetProperty(ref _selectedDechuckRetryRecipe, value);
    }

    private string? _selectedUnloadRetryRecipe;

    public string? SelectedUnloadRetryRecipe
    {
        get => _selectedUnloadRetryRecipe;
        set => SetProperty(ref _selectedUnloadRetryRecipe, value);
    }

    // 电机位置选择（Combobox 可编辑：可选择预设位置，也可输入自定义数值）
    public List<string> HeadSweepMotorPositionOptions { get; } = ["Home", "PolishNear", "PolishFar", "Dechuck"];

    private string _selectedHeadSweepMotorPosition = "Home";

    public string SelectedHeadSweepMotorPosition
    {
        get => _selectedHeadSweepMotorPosition;
        set => SetProperty(ref _selectedHeadSweepMotorPosition, value);
    }

    public List<string> HeadLiftMotorPositionOptions { get; } = ["High", "LoadCup", "Polish", "Clean"];

    private string _selectedHeadLiftMotorPosition = "High";

    public string SelectedHeadLiftMotorPosition
    {
        get => _selectedHeadLiftMotorPosition;
        set => SetProperty(ref _selectedHeadLiftMotorPosition, value);
    }

    public List<string> ConditionSweepMotorPositionOptions { get; } = ["Home", "Start", "End"];

    private string _selectedConditionSweepMotorPosition = "Home";

    public string SelectedConditionSweepMotorPosition
    {
        get => _selectedConditionSweepMotorPosition;
        set => SetProperty(ref _selectedConditionSweepMotorPosition, value);
    }

    public List<string> SlurrySweepMotorPositionOptions { get; } = ["Home", "Slurry"];

    private string _selectedSlurrySweepMotorPosition = "Home";

    public string SelectedSlurrySweepMotorPosition
    {
        get => _selectedSlurrySweepMotorPosition;
        set => SetProperty(ref _selectedSlurrySweepMotorPosition, value);
    }

    private string _configLoadRecipe = "";

    private string _configUnLoadRecipe = "";

    private string _configDechukRecipe = "";

    private string _configLoadRetryRecipe = "";

    private string _configDechuckRetryRecipe = "";

    private string _configUnloadRetryRecipe = "";

    private ObservableCollection<SlurryDiw> _slurryDiwOptions =
    [
        new() { Diw = false, Slurry = false },
        new() { Diw = false, Slurry = false },
        new() { Diw = false, Slurry = false }
    ];

    public ObservableCollection<SlurryDiw> SlurryDiwOptions
    {
        get => _slurryDiwOptions;
        set => SetProperty(ref _slurryDiwOptions, value);
    }

    public DelegateCommand<string> AutoCommand { get; set; }
    public DelegateCommand<string> AbortUnitCommand { get; set; }
    public DelegateCommand<string> AbortCommand { get; set; }
    public DelegateCommand<string> StartCommand { get; set; }
    public DelegateCommand<string> RunCommand { get; set; }
    public DelegateCommand<string> PressCommand { get; set; }
    public DelegateCommand<string> VentCommand { get; set; }
    public DelegateCommand<string> VacuumCommand { get; set; }
    public DelegateCommand<string> UpCommand { get; set; }
    public DelegateCommand<string> DownCommand { get; set; }
    public DelegateCommand<string> SetCommand { get; set; }
    public DelegateCommand<string> ResetCommand { get; set; }
    public DelegateCommand<string> ServoCommand { get; set; }
    public DelegateCommand<string>? LoadCommand { get; set; }
    public DelegateCommand<string>? PolishCommand { get; set; }
    public DelegateCommand<string>? InitializeCommand { get; set; }
    public DelegateCommand<string>? AutoModeCommand { get; set; }
    public DelegateCommand<string>? ManualModeCommand { get; set; }
    public DelegateCommand<string>? AutoRunCommand { get; set; }
    public DelegateCommand<string>? ConditionCommand { get; set; }
    public DelegateCommand<string>? UnloadCommand { get; set; }
    public DelegateCommand<string>? DechuckCommand { get; set; }
    public DelegateCommand<string>? SlurrySwitchCommand { get; set; }
    public DelegateCommand<string>? PositionCommand { get; set; }

    // Servo button text properties
    public string PlatenMotorServoText => PolishUnit.PlatenMotor.IsServoOn ? Resources.Off : Resources.On;
    public string HeadRotateMotorServoText => PolishUnit.HeadRotateMotor.IsServoOn ? Resources.Off : Resources.On;
    public string HeadSweepMotorServoText => PolishUnit.HeadSweepMotor.IsServoOn ? Resources.Off : Resources.On;
    public string HeadLiftMotorServoText => PolishUnit.HeadLiftMotor.IsServoOn ? Resources.Off : Resources.On;
    public string DiskRotateMotorServoText => PolishUnit.DiskRotateMotor.IsServoOn ? Resources.Off : Resources.On;

    public string ConditionSweepMotorServoText =>
        PolishUnit.ConditionSweepMotor.IsServoOn ? Resources.Off : Resources.On;

    public string SlurrySweepMotorServoText =>
        PolishUnit.SlurrySweepMotor.IsServoOn ? Resources.Off : Resources.On; // Servo control not supported


    int err = (int)KHLUserError.KHL_ERR_NONE;
    KVLink kvLink;
    byte[] writeBuf = new byte[65535];
    byte[] readBuf = new byte[65535];
    byte[] writeTrue = { 1 };
    byte[] writeFalse = { 0 };

    int delayCycle = 0;
    bool isPLCAutoStartNeedFalse = false;

    /// <summary>
    /// 是否重置Recipe状态
    /// </summary>
    bool isResetRecipeState = false;

    //private string _plcAddr;
    //private ushort _plcPort;
    private string _recipeBasePath = "D:\\CMP\\Configuration\\Recipes\\UnitRecipe\\";

    Task thMonitor;

    Polish polishData;

    int waferIndex;
    //public PolishUnitTabContentViewModel(int id, string tabTitle, IEventAggregator eventAggregator, 
    //    string plcAddr = "192.168.2.20", ushort plcPort = 8500) : base(id)
    //{
    //    _eventAggregator = eventAggregator;
    //    _plcAddr = plcAddr;
    //    _plcPort = plcPort;
    //    TabId = id;
    //    TabTitle = tabTitle;

    //    AutoCommand = new DelegateCommand<string>(AutoCommandExecute);
    //    AbortCommand = new DelegateCommand<string>(AbortCommandExecute);
    //    StartCommand = new DelegateCommand<string>(StartCommandExecute);
    //    RunCommand = new DelegateCommand<string>(RunCommandExecute);
    //    PressCommand = new DelegateCommand<string>(PressCommandExecute);
    //    VentCommand = new DelegateCommand<string>(VentCommandExecute);
    //    VacuumCommand = new DelegateCommand<string>(VacuumCommandExecute);
    //    ServoCommand = new DelegateCommand<string>(ServoCommandExecute);
    //    LoadCommand = new DelegateCommand<string>(LoadCommandExecute);
    //    PolishCommand = new DelegateCommand<string>(PolishCommandExecute);
    //    InitializeCommand = new DelegateCommand<string>(InitializeCommandExecute);
    //    AutoModeCommand = new DelegateCommand<string>(AutoModeCommandExecute);
    //    ManualModeCommand = new DelegateCommand<string>(ManualModeCommandExecute);
    //    ConditionCommand = new DelegateCommand<string>(ConditionCommandExecute);
    //    UnloadCommand = new DelegateCommand<string>(UnloadCommandExecute);
    //    DechuckCommand = new DelegateCommand<string>(DechuckCommandExecute);

    //    err = (int)KHLUserError.KHL_ERR_NONE;
    //    sock = 0;
    //        err = KHL.KHLInit();
    //        if (err != (int)KHLUserError.KHL_ERR_NONE)
    //        {
    //        }
    //        err = KHL.KHLConnect(_plcAddr, _plcPort, 3000, KHLSockType.SOCK_TCP, ref sock);
    //        if (err != (int)KHLUserError.KHL_ERR_NONE)
    //        {
    //        }
    //    byte[] writeTrue ={ 1 };
    //    err = KHL.KHLWriteVariable(sock, "PLCStartPermition", writeTrue, true);
    //    err = KHL.KHLWriteVariable(sock, "PLCReady", writeTrue, true);
    //    err = KHL.KHLWriteVariable(sock, "PLCAutoStart", writeTrue, true);
    //    isPLCAutoStartNeedFalse = true;
    //    //err = KHL.KHLConnect(_plcAddr, _plcPort, 3000, KHLSockType.SOCK_TCP, ref sock);
    //    //if (err != (int)KHLUserError.KHL_ERR_NONE)
    //    //{
    //    //}

    //    // Load recipes
    //    LoadRecipesData();

    //    // 订阅Recipe保存事件，当Recipe编辑器保存时自动重新加载
    //    _eventAggregator.GetEvent<RecipeSavedEvent>().Subscribe(OnRecipeSaved, ThreadOption.UIThread);

    //        thMonitor = new Thread(Monitor);
    //        thMonitor.Start();
    //        thMonitor.IsBackground = true;
    //}

    public PolishUnitTabContentViewModel(int id, KVLink link, string tabTitle, IEventAggregator eventAggregator) :
        base(id)
    {
        _eventAggregator = eventAggregator;
        TabId = id;
        TabTitle = tabTitle;
        isPLCAutoStartNeedFalse = true;
        _baseChamberAddress = "ZF89000";
        // 订阅Recipe保存事件，当Recipe编辑器保存时自动重新加载
        _eventAggregator.GetEvent<RecipeSavedEvent>().Subscribe(OnRecipeSaved, ThreadOption.UIThread);
        _eventAggregator.GetEvent<GetRecipeConfigEvent>().Subscribe(OnGetRecipeConfig, ThreadOption.UIThread);

        //_eventAggregator.GetEvent<PubSubEvent>().Subscribe(OnPolishSocketsChanged);

        AutoCommand = OperationAudit.Create<string>("Auto", AutoCommandExecute, () => TabTitle ?? string.Empty);
        AbortUnitCommand = OperationAudit.Create<string>("AbortUnit", AbortUnitCommandExecute, () => TabTitle ?? string.Empty);
        AbortCommand = OperationAudit.Create<string>("Abort", AbortCommandExecute, () => TabTitle ?? string.Empty);
        StartCommand = OperationAudit.Create<string>("Start", StartCommandExecute, () => TabTitle ?? string.Empty);
        RunCommand = OperationAudit.Create<string>("Run", RunCommandExecute, () => TabTitle ?? string.Empty);
        PressCommand = OperationAudit.Create<string>("Press", PressCommandExecute, () => TabTitle ?? string.Empty);
        VentCommand = OperationAudit.Create<string>("Vent", VentCommandExecute, () => TabTitle ?? string.Empty);
        VacuumCommand = OperationAudit.Create<string>("Vacuum", VacuumCommandExecute, () => TabTitle ?? string.Empty);
        UpCommand = OperationAudit.Create<string>("Up", UpCommandExecute, () => TabTitle ?? string.Empty);
        DownCommand = OperationAudit.Create<string>("Down", DownCommandExecute, () => TabTitle ?? string.Empty);
        SetCommand = OperationAudit.Create<string>("Set", SetCommandExecute, () => TabTitle ?? string.Empty);
        ResetCommand = OperationAudit.Create<string>("Reset", ResetCommandExecute, () => TabTitle ?? string.Empty);
        ServoCommand = OperationAudit.Create<string>("Servo", ServoCommandExecute, () => TabTitle ?? string.Empty);
        LoadCommand = OperationAudit.Create<string>("Load", LoadCommandExecute, () => TabTitle ?? string.Empty);
        PolishCommand = OperationAudit.Create<string>("Polish", PolishCommandExecute, () => TabTitle ?? string.Empty);
        InitializeCommand = OperationAudit.Create<string>("Initialize", InitializeCommandExecute, () => TabTitle ?? string.Empty);
        AutoModeCommand = OperationAudit.Create<string>("AutoMode", AutoModeCommandExecute, () => TabTitle ?? string.Empty);
        ManualModeCommand = OperationAudit.Create<string>("ManualMode", ManualModeCommandExecute, () => TabTitle ?? string.Empty);
        AutoRunCommand = OperationAudit.Create<string>("AutoRun", AutoRunCommandExecute, () => TabTitle ?? string.Empty);
        ConditionCommand = OperationAudit.Create<string>("BreakIn", BreakInCommandExecute, () => TabTitle ?? string.Empty);
        UnloadCommand = OperationAudit.Create<string>("Unload", UnloadCommandExecute, () => TabTitle ?? string.Empty);
        DechuckCommand = OperationAudit.Create<string>("Dechuck", DechuckCommandExecute, () => TabTitle ?? string.Empty);
        SlurrySwitchCommand = OperationAudit.Create<string>("SlurrySwitch", SlurrySwitchCommandExecute, () => TabTitle ?? string.Empty);
        PositionCommand = OperationAudit.Create<string>("Position", PositionCommandExecute, () => TabTitle ?? string.Empty);


        kvLink = link;
        err = KHL.KHLWriteVariable(kvLink.sock, "PLCStartPermition", writeTrue, true);
        err = KHL.KHLWriteVariable(kvLink.sock, "PLCReady", writeTrue, true);
        //err = KHL.KHLWriteVariable(sock, "PLCAutoStart", writeTrue, true);

        LoadRecipesData();

        thMonitor = new Task(Monitor);
        thMonitor.Start();

        polishData = GlobalService.DataCenter.listPolish[id];
    }

    /// <summary>
    /// 从配置中获取已经设置好的RecipeName
    /// </summary>
    /// <param name="args"></param>
    private void OnGetRecipeConfig(GetRecipeConfigEventArgs args)
    {
        try
        {
            if (args == null || string.IsNullOrEmpty(args.RecipeType))
                return;

            string workStationName = "";
            if (args.ConfigFileName != null && args.ConfigFileName != "")
            {
                switch (TabId)
                {
                    case 0:
                        workStationName = "PolishA";
                        break;
                    case 1:
                        workStationName = "PolishB";
                        break;
                    case 2:
                        workStationName = "PolishC";
                        break;
                    case 3:
                        workStationName = "PolishD";
                        break;
                    default:
                        break;
                }
                if (!(bool)args.ConfigFileName.StartsWith(workStationName))
                {
                    //修改的不是当前polish工位就不需要更新
                    return;
                }
            }
            bool isSuccess = false;
            switch (args.RecipeType)
            {
                case "Load Recipe":
                    LoadRecipesFromFolder("Load Recipe", LoadRecipes);
                    if (!string.IsNullOrEmpty(args.RecipeName) && LoadRecipes.Contains(args.RecipeName))
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            _configLoadRecipe = args.RecipeName; 
                            SelectedLoadRecipe = args.RecipeName;
                        });
                        isSuccess = true;
                    }

                    break;                               
                case "UnLoad Recipe":
                    LoadRecipesFromFolder("UnLoad Recipe", UnloadRecipes);
                    if (!string.IsNullOrEmpty(args.RecipeName) && UnloadRecipes.Contains(args.RecipeName))
                    {
                       Application.Current.Dispatcher.Invoke(() => {  
                           _configUnLoadRecipe = args.RecipeName; 
                           SelectedUnloadRecipe = args.RecipeName;
                       });
                        isSuccess = true;
                    }
                    break;
                case "Dechuck Recipe":
                    LoadRecipesFromFolder("Dechuck Recipe", DechuckRecipes);
                    if (!string.IsNullOrEmpty(args.RecipeName) && DechuckRecipes.Contains(args.RecipeName))
                    {
                        Application.Current.Dispatcher.Invoke(() => { 
                            _configDechukRecipe = args.RecipeName; 
                            SelectedDechuckRecipe = args.RecipeName; 
                        });
                        isSuccess = true;
                    }
                    break;
                case "Load Retry Recipe":
                    LoadRecipesFromFolder("Load Recipe", LoadRetryRecipes);
                    if (!string.IsNullOrEmpty(args.RecipeName) && LoadRetryRecipes.Contains(args.RecipeName))
                    {
                        Application.Current.Dispatcher.Invoke(() => { 
                            SelectedLoadRetryRecipe = args.RecipeName;
                        });
                        isSuccess = true;
                    }
                    break;
                case "Dechuck Retry Recipe":
                    LoadRecipesFromFolder("Dechuck Recipe", DechuckRetryRecipes);
                    if (!string.IsNullOrEmpty(args.RecipeName) && DechuckRetryRecipes.Contains(args.RecipeName))
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            SelectedDechuckRetryRecipe = args.RecipeName;
                        });
                        isSuccess = true;
                    }
                    break;
                case "UnLoad Retry Recipe":
                    LoadRecipesFromFolder("UnLoad Recipe", UnloadRetryRecipes);
                    if (!string.IsNullOrEmpty(args.RecipeName) && UnloadRetryRecipes.Contains(args.RecipeName))
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            SelectedUnloadRetryRecipe = args.RecipeName;
                        });
                        isSuccess = true;
                    }
                    break;
            }
            if (isSuccess)
            {
                if (args.IsNeedTip)
                {
                    Growl.SuccessGlobal($"获取到Recipe配置参数:成功更新到工作台{workStationName} - Recipe类型：{args.RecipeType} - 文件名：{args.RecipeName}");
                }                
            }
            else
            {
                Growl.WarningGlobal($"获取到Recipe配置参数:未能更新到工作台{workStationName} - Recipe类型：{args.RecipeType} - 文件名：{args.RecipeName}");
            }
            Logger.Info($"获取到Recipe配置参数:Recipe类型：{args.RecipeType},文件名：{args.RecipeName},工作台 {workStationName}, 是否完成更新: {isSuccess}，是否提示{args.IsNeedTip}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to get recipes config : {ex.Message}", ex);
        }
    }

    private void SlurrySwitchCommandExecute(string obj)
    {
        SlurryDiwOptions[0].FlowRate = PolishUnit.Slurry.Slurry1Set;
        SlurryDiwOptions[1].FlowRate = PolishUnit.Slurry.Slurry2Set;
        SlurryDiwOptions[2].FlowRate = PolishUnit.Slurry.Slurry3Set;

        for (var i = 0; i < SlurryDiwOptions.Count; i++)
        {
            var option = SlurryDiwOptions[i];
            if (option.FlowRate == 0) continue;
            byte[] writeBufTemp;
            uint plcHandle;

            if (obj.Equals("On"))
            {
                writeBufTemp = option.Slurry ? i == 0 ? PlcHandle.CmdPolish.Liquid.Slurry1.Open.Value() :
                    i == 1 ? PlcHandle.CmdPolish.Liquid.Slurry2.Open.Value() :
                    PlcHandle.CmdPolish.Liquid.Slurry3.Open.Value()
                    : i == 0 ? PlcHandle.CmdPolish.Liquid.Slurry1.Diw.Open.Value()
                    : i == 1 ? PlcHandle.CmdPolish.Liquid.Slurry2.Diw.Open.Value()
                    : PlcHandle.CmdPolish.Liquid.Slurry3.Diw.Open.Value();
                plcHandle = option.Slurry ? i == 0 ? PlcHandle.CmdPolish.Liquid.Slurry1.Open.Handle() :
                    i == 1 ? PlcHandle.CmdPolish.Liquid.Slurry2.Open.Handle() :
                    PlcHandle.CmdPolish.Liquid.Slurry3.Open.Handle()
                    : i == 0 ? PlcHandle.CmdPolish.Liquid.Slurry1.Diw.Open.Handle()
                    : i == 1 ? PlcHandle.CmdPolish.Liquid.Slurry2.Diw.Open.Handle()
                    : PlcHandle.CmdPolish.Liquid.Slurry3.Diw.Open.Handle();
            }
            else
            {
                writeBufTemp = option.Slurry ? i == 0 ? PlcHandle.CmdPolish.Liquid.Slurry1.Close.Value() :
                    i == 1 ? PlcHandle.CmdPolish.Liquid.Slurry2.Close.Value() :
                    PlcHandle.CmdPolish.Liquid.Slurry3.Close.Value()
                    : i == 0 ? PlcHandle.CmdPolish.Liquid.Slurry1.Diw.Close.Value()
                    : i == 1 ? PlcHandle.CmdPolish.Liquid.Slurry2.Diw.Close.Value()
                    : PlcHandle.CmdPolish.Liquid.Slurry3.Diw.Close.Value();
                plcHandle = option.Slurry ? i == 0 ? PlcHandle.CmdPolish.Liquid.Slurry1.Close.Handle() :
                    i == 1 ? PlcHandle.CmdPolish.Liquid.Slurry2.Close.Handle() :
                    PlcHandle.CmdPolish.Liquid.Slurry3.Close.Handle()
                    : i == 0 ? PlcHandle.CmdPolish.Liquid.Slurry1.Diw.Close.Handle()
                    : i == 1 ? PlcHandle.CmdPolish.Liquid.Slurry2.Diw.Close.Handle()
                    : PlcHandle.CmdPolish.Liquid.Slurry3.Diw.Close.Handle();
            }

            // set param
            var paramHandle = i switch
            {
                0 => PlcHandle.CmdPolish.Liquid.Slurry1.Param.Flowrate.ParamHandle(),
                1 => PlcHandle.CmdPolish.Liquid.Slurry2.Param.Flowrate.ParamHandle(),
                _ => PlcHandle.CmdPolish.Liquid.Slurry3.Param.Flowrate.ParamHandle()
            };
            var paramValue = BitConverter.GetBytes(option.FlowRate);
            var err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, paramHandle, 2, paramValue);
            if (err != (int)KHLUserError.KHL_ERR_NONE)
            {
                Growl.ErrorGlobal($"Set Slurry{i + 1} Flowrate Failed!");
            }
            // send command
            err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, plcHandle, 1, writeBufTemp);
            if (err != (int)KHLUserError.KHL_ERR_NONE)
            {
                Growl.ErrorGlobal($"Slurry{i + 1} {(obj.Equals("Open") ? "Open" : "Close")} Command Failed!");
            }
        }
    }


    private async void Monitor()
    {
        //Thread.Sleep(2000);
        //防止界面还未完全加载完成就开始和PLC通讯导致的异常，等界面完全加载后再开始通讯
        await Task.Delay(2000).ConfigureAwait(false);
        const int delay = 500;
        var disp = Application.Current?.Dispatcher;
        while (true)
        {
            try {
                //每次循环都先等待，避免过于频繁地与PLC通讯
                await Task.Delay(delay).ConfigureAwait(false);
                if (isPLCAutoStartNeedFalse)
                {
                    delayCycle++;
                    if (delayCycle >= 4)
                    {
                        err = KHL.KHLWriteVariable(kvLink.sock, "PLCAutoStart", writeFalse, true);
                        delayCycle = 0;
                        isPLCAutoStartNeedFalse = false;
                    }
                }
                IntereactionWithPLCForAssignRecipe();

                if (disp != null)
                    await disp.InvokeAsync(new System.Action(RefreshHMI));
                else
                    RefreshHMI();
            }
            catch (Exception ex)
            {
                Logger.Error($"Polish monitor loop error in {TabTitle}", ex);
            }

        }
    }

    /// <summary>
    /// 获取类型的默认失败值
    /// </summary>
    private static T GetDefaultFailureValue<T>()
    {
        return typeof(T) switch
        {
            { } t when t == typeof(double) => (T)(object)double.NaN,
            { } t when t == typeof(float) => (T)(object)float.NaN,
            { } t when t == typeof(int) => (T)(object)0,
            { } t when t == typeof(long) => (T)(object)0L,
            { } t when t == typeof(short) => (T)(object)(short)0,
            { } t when t == typeof(byte) => (T)(object)(byte)0,
            { } t when t == typeof(bool) => (T)(object)false,
            { } t when t == typeof(string) => (T)(object)string.Empty,
            _ => default!
        };
    }

    /// <summary>
    /// 获取 SV 的值并转换为指定类型，如果获取失败返回默认值
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="key">SV 的键</param>
    /// <param name="defaultValue">获取失败时的默认值，不指定则自动匹配类型默认值</param>
    /// <returns>转换后的值或默认值</returns>
    private T GetSvValue<T>(string key, T? defaultValue = default)
    {
        try
        {
            var sv = polishData.GetSv(key);
            if (sv?.Value == null)
                return EqualityComparer<T>.Default.Equals(defaultValue!, default!)
                    ? GetDefaultFailureValue<T>()
                    : defaultValue!;

            return (T)Convert.ChangeType(sv.Value, typeof(T));
        }
        catch
        {
            return EqualityComparer<T>.Default.Equals(defaultValue!, default!)
                ? GetDefaultFailureValue<T>()
                : defaultValue!;
        }
    }

    private void RefreshHMI()
    {
        stPolishCollectData stCollectData;
        PolishUnit.PlatenMotor.ActVelocity = GetSvValue<float>(polishData.polishName + nameof(stCollectData.PlatenRotationSpeedAct));
        PolishUnit.HeadRotateMotor.ActVelocity = GetSvValue<float>(polishData.polishName + nameof(stCollectData.HeadRotationSpeedAct));
        PolishUnit.HeadSweepMotor.ActVelocity = GetSvValue<float>(polishData.polishName + nameof(stCollectData.HeadSweepSpeedAct));
        PolishUnit.HeadSweepMotor.ActPosition = GetSvValue<float>(polishData.polishName + nameof(stCollectData.HeadSweepPositionAct));
        PolishUnit.HeadLiftMotor.ActVelocity = GetSvValue<float>(polishData.polishName + nameof(stCollectData.HeadZ_AxisSpeedAct));
        PolishUnit.HeadLiftMotor.ActPosition = GetSvValue<float>(polishData.polishName + nameof(stCollectData.HeadZ_AxisPositionAct));
        PolishUnit.DiskRotateMotor.ActVelocity = GetSvValue<float>(polishData.polishName + nameof(stCollectData.ConditionRotationSpeedAct));
        PolishUnit.ConditionSweepMotor.ActVelocity = GetSvValue<float>(polishData.polishName + nameof(stCollectData.ConditionArmSweepSpeedAct));
        PolishUnit.ConditionSweepMotor.ActPosition = GetSvValue<float>(polishData.polishName + nameof(stCollectData.ConditionArmSweepPositionAct));

        PolishUnit.Zone1.ActPress = GetSvValue<float>(polishData.polishName + nameof(stCollectData.Zone1PressureAct));
        PolishUnit.Zone2.ActPress = GetSvValue<float>(polishData.polishName + nameof(stCollectData.Zone2PressureAct));
        PolishUnit.Zone3.ActPress = GetSvValue<float>(polishData.polishName + nameof(stCollectData.Zone3PressureAct));
        PolishUnit.Zone4.ActPress = GetSvValue<float>(polishData.polishName + nameof(stCollectData.Zone4PressureAct));
        PolishUnit.Zone5.ActPress = GetSvValue<float>(polishData.polishName + nameof(stCollectData.Zone5PressureAct));
        PolishUnit.Zone6.ActPress = GetSvValue<float>(polishData.polishName + nameof(stCollectData.Zone6PressureAct));
        PolishUnit.Zone7.ActPress = GetSvValue<float>(polishData.polishName + nameof(stCollectData.Zone7PressureAct));
        PolishUnit.Zone8.ActPress = GetSvValue<float>(polishData.polishName + nameof(stCollectData.Zone8PressureAct));
        PolishUnit.Slurry.Slurry1Act = GetSvValue<float>(polishData.polishName + nameof(stCollectData.Slurry1OrDIWFlowrateAct));
        PolishUnit.Slurry.Slurry2Act = GetSvValue<float>(polishData.polishName + nameof(stCollectData.Slurry2OrDIWFlowrateAct));
        PolishUnit.Slurry.Slurry3Act = GetSvValue<float>(polishData.polishName + nameof(stCollectData.Slurry3OrDIWFlowrateAct));


        PolishUnit.PlatenMotor.IsServoOn = GetSvValue<bool>(polishData.polishName + nameof(stCollectData.PlatenRotationMotorServoOn));
        PolishUnit.HeadRotateMotor.IsServoOn = GetSvValue<bool>(polishData.polishName + nameof(stCollectData.HeadRotationMotorServoOn));
        PolishUnit.HeadSweepMotor.IsServoOn = GetSvValue<bool>(polishData.polishName + nameof(stCollectData.HeadSweepMotorServoOn));
        PolishUnit.HeadLiftMotor.IsServoOn = GetSvValue<bool>(polishData.polishName + nameof(stCollectData.HeadUpDownMotorServoOn));
        PolishUnit.DiskRotateMotor.IsServoOn = GetSvValue<bool>(polishData.polishName + nameof(stCollectData.ConditionHeadRotationMotorServoOn));
        PolishUnit.ConditionSweepMotor.IsServoOn = GetSvValue<bool>(polishData.polishName + nameof(stCollectData.ConditionSweepMotorServoOn));
        PolishUnit.SlurrySweepMotor.IsServoOn = GetSvValue<bool>(polishData.polishName + nameof(stCollectData.SlurrySweepMotorServoOn));


        RaisePropertyChanged(nameof(PlatenMotorServoText));
        RaisePropertyChanged(nameof(ConditionSweepMotorServoText));
        RaisePropertyChanged(nameof(HeadRotateMotorServoText));
        RaisePropertyChanged(nameof(HeadLiftMotorServoText));
        RaisePropertyChanged(nameof(HeadSweepMotorServoText));
        RaisePropertyChanged(nameof(DiskRotateMotorServoText));
        RaisePropertyChanged(nameof(SlurrySweepMotorServoText));
    }

    private void SetWaferIndex(int waferIndex)
    {
        this.waferIndex = waferIndex;
        switch (TabId)
        {
            case 0:
                GlobalService.globalChambers.PolishA.WaferIndex = waferIndex;
                break;
            case 1:
                GlobalService.globalChambers.PolishB.WaferIndex = waferIndex;
                break;
            case 2:
                GlobalService.globalChambers.PolishC.WaferIndex = waferIndex;
                break;
            case 3:
                GlobalService.globalChambers.PolishD.WaferIndex = waferIndex;
                break;
        }
    }

    private void IntereactionWithPLCForAssignRecipe()
    {
        err = KHL.KHLReadDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, Convert.ToUInt32(0x300C), 1, readBuf);
        if (err != (int)KHLUserError.KHL_ERR_NONE)
        {
            Logger.For("RecipeAssign").Warn($"Read recipe req failed: {TabTitle} address=0x300C, err={err}");
            return;
        }

        int recipeReq = Convert.ToInt32(readBuf[0]);

        if (recipeReq != 0)
        {
            Growl.SuccessGlobal($"Index: {recipeReq}");
            int waferIndex = recipeReq;
            byte w10Low = readBuf[0];
            SetWaferIndex(waferIndex);
            //byte[] writeBufTemp;
            try
            {
                var (polishRecipeName, conditionRecipeName) =
                    GetRecipe(GlobalService.dicWaferIndex[waferIndex], this.Id);

                AssignWaferToPLC(polishRecipeName, conditionRecipeName);

                //向地址中写入wafer的sequence数值
                var wafer = GlobalService.dicWaferIndex[waferIndex];
                for (int i = 0; i < wafer.SequnceValue.Length; i++)
                {
                    byte[] data = new byte[256];
                    uint devTopNo = (uint)(Convert.ToUInt32(_baseChamberAddress?.Replace("ZF", "")) + 10 + 5 * i);
                    KST.IntToByte(ref data, wafer.SequnceValue[i]);
                    int error = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, devTopNo, 2, data);
                    if (error != (int)KHLUserError.KHL_ERR_NONE)
                    {
                        Logger.For("SequenceWrite").Debug($"向{TabTitle}地址ZF{devTopNo}中写入数值{wafer.SequnceValue[i]}失败，错误码返回码{error}");
                    }
                    else
                    {
                        Logger.For("SequenceWrite").Debug($"向{TabTitle}地址ZF{devTopNo}中写入数值{wafer.SequnceValue[i]}成功！");
                    }
                }

                byte[] writeBufTemp = { w10Low, 0x02 };
                err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, Convert.ToUInt32(0x010), 1,
                    writeBufTemp);
            }
            catch(Exception ex)
            {
                byte[] writeBufTemp = { w10Low, 0x03 };
                err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, Convert.ToUInt32(0x010), 1, writeBufTemp);
                Growl.ErrorGlobal($"Assign Recipe Failed, Index: {waferIndex},Error Information: {ex.Message},{ex.StackTrace}");
                Logger.For("AssignPolishRecipe").Error($"Assign Recipe Failed, Index: {waferIndex},Error Information: {ex.Message},{ex.StackTrace}");
            }

            //err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, Convert.ToUInt32(0x010), 1, writeBufTemp);
            //Growl.SuccessGlobal("SET: W010");
            isResetRecipeState = true;
        }
        else
        {
            //if (isResetRecipeState)
            //{
            byte[] writeBufTemp = { 0, 0 };
            err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, Convert.ToUInt32(0x010), 1, writeBufTemp);
            //Growl.SuccessGlobal("CLEAR: W010");
            isResetRecipeState = false;
            //}
        }
    }

    private void AssignWaferToPLC(string polishRecipeName, string conditionRecipeName)
    {
        SelectedPolishRecipe = polishRecipeName;
        SelectedConditionRecipe = conditionRecipeName;

        LoadCommandExecute("Auto");

        PolishCommandExecute("Auto");

        UnloadCommandExecute("Auto");

        DechuckCommandExecute("Auto");

        BreakInCommandExecute("Auto");

    }

    private (string, string) GetRecipe(Wafer wafer, int id)
    {
        var path = Path.Combine("D:\\CMP\\Configuration", "Recipes", "Sequence", $"{wafer.SequenceRecipe}.json");
        var text = File.ReadAllText(path);
        var data = JsonConvert.DeserializeObject<SequenceRecipeData>(text);
        string polishRecipeName = string.Empty;
        string conditionRecipeName = string.Empty;
        if (id == 0)
        {
            polishRecipeName =
                GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipePolishA").Value.ToString());
            conditionRecipeName =
                GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipePolishAExsitu").Value.ToString());
            wafer.CurrentStation = CurrentStation.PolishA;
        }
        else if (id == 1)
        {
            polishRecipeName =
                GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipePolishB").Value.ToString());
            conditionRecipeName =
                GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipePolishBExsitu").Value.ToString());
            wafer.CurrentStation = CurrentStation.PolishB;
        }
        else if (id == 2)
        {
            polishRecipeName =
                GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipePolishC").Value.ToString());
            conditionRecipeName =
                GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipePolishCExsitu").Value.ToString());
            wafer.CurrentStation = CurrentStation.PolishC;
        }
        else if (id == 3)
        {
            polishRecipeName =
                GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipePolishD").Value.ToString());
            conditionRecipeName =
                GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipePolishDExsitu").Value.ToString());
            wafer.CurrentStation = CurrentStation.PolishD;
        }

        return (polishRecipeName, conditionRecipeName);
    }

    private string GetFileNameWithoutEx(string path)
    {
        string[] temp = path.Split('\\');
        string fileName = temp[temp.Length - 1];
        return fileName.Substring(0, fileName.Length - 5);
    }

    private int KHLConnectAndOperation(string handle, byte[] value)
    {
        return KHL.KHLWriteVariable(kvLink.sock, handle, value, true);
    }

    private void StartCommandExecute(string obj)
    {
        if (string.IsNullOrEmpty(obj))
            return;

        // 解析参数，格式为 "设备名_操作" (如 "DIWOutletValve_Open")
        string[] parts = obj.Split('_');
        if (parts.Length != 2)
        {
            Growl.WarningGlobal($"无效的命令参数: {obj}");
            return;
        }

        string deviceName = parts[0];
        string action = parts[1]; // "Open" 或 "Close"

        uint plcHandle = 0u;
        byte[] plcValue = new byte[2048];

        try
        {
            // 根据设备名称和操作类型获取对应的PLC Handle和Value
            switch (deviceName)
            {
                case "DIWOutletValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.DIWOutletValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.DIWOutletValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.DIWOutletValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.DIWOutletValve.Close.Value();
                    break;

                case "PlatenWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.PlatenWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.PlatenWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.PlatenWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.PlatenWashValve.Close.Value();
                    break;

                case "MovingHeadDIWWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.MovingHeadDIWWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.MovingHeadDIWWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.MovingHeadDIWWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.MovingHeadDIWWashValve.Close.Value();
                    break;

                case "MovingHeadExternalDIWWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.MovingHeadExternalDIWWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.MovingHeadExternalDIWWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.MovingHeadExternalDIWWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.MovingHeadExternalDIWWashValve.Close.Value();
                    break;

                case "HeadChemicalWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.HeadChemicalWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.HeadChemicalWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.HeadChemicalWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.HeadChemicalWashValve.Close.Value();
                    break;

                case "ConditionerDIWWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.ConditionerDIWWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.ConditionerDIWWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.ConditionerDIWWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.ConditionerDIWWashValve.Close.Value();
                    break;

                case "ConditionerArmDIWWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.ConditionerArmDIWWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.ConditionerArmDIWWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.ConditionerArmDIWWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.ConditionerArmDIWWashValve.Close.Value();
                    break;

                case "ChemicalArmDIWWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.ChemicalArmDIWWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.ChemicalArmDIWWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.ChemicalArmDIWWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.ChemicalArmDIWWashValve.Close.Value();
                    break;

                case "HeadMiddleWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.HeadMiddleWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.HeadMiddleWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.HeadMiddleWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.HeadMiddleWashValve.Close.Value();
                    break;

                case "HeadExternalWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.HeadExternalWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.HeadExternalWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.HeadExternalWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.HeadExternalWashValve.Close.Value();
                    break;

                case "WFUpperFaceChemicalWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.WFUpperFaceChemicalWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.WFUpperFaceChemicalWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.WFUpperFaceChemicalWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.WFUpperFaceChemicalWashValve.Close.Value();
                    break;

                case "WFUpperFaceDIWWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.WFUpperFaceDIWWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.WFUpperFaceDIWWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.WFUpperFaceDIWWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.WFUpperFaceDIWWashValve.Close.Value();
                    break;

                case "WFLowerFaceChemicalWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.WFLowerFaceChemicalWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.WFLowerFaceChemicalWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.WFLowerFaceChemicalWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.WFLowerFaceChemicalWashValve.Close.Value();
                    break;

                case "WFLowerFaceDIWWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.WFLowerFaceDIWWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.WFLowerFaceDIWWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.WFLowerFaceDIWWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.WFLowerFaceDIWWashValve.Close.Value();
                    break;

                case "PlatenCoolWaterValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.PlatenCoolWaterValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.PlatenCoolWaterValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.PlatenCoolWaterValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.PlatenCoolWaterValve.Close.Value();
                    break;

                case "PlatenMotorCoolWaterValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.PlatenMotorCoolWaterValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.PlatenMotorCoolWaterValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.PlatenMotorCoolWaterValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.PlatenMotorCoolWaterValve.Close.Value();
                    break;

                default:
                    Growl.WarningGlobal($"未知的设备: {deviceName}");
                    return;
            }

            // 发送PLC命令
            err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, plcHandle, 1, plcValue);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                string actionText = action == "Open" ? "On" : "Off";
                Growl.SuccessGlobal($"{deviceName} {actionText} command executed successfully.");
            }
            else
            {
                string actionText = action == "Open" ? "On" : "Off";
                Growl.ErrorGlobal($"{deviceName} {actionText} command execution failed with error code: {err}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"exception occurred while executing {deviceName} {action} command. {ex.Message}");
            Logger.Error($"Error executing liquid control command for {deviceName} {action}", ex);
        }
    }

    /// <summary>
    /// 解析位置输入值：必须是有限数字（拒绝 NaN/Infinity 等非法值）
    /// </summary>
    private static bool TryParsePositionValue(string? text, out float value)
    {
        if (float.TryParse(text, out value) && !float.IsNaN(value) && !float.IsInfinity(value))
        {
            return true;
        }

        value = 0;
        return false;
    }

    /// <summary>
    /// 写入位置参数后立即读回验证（用于诊断"写入值为0"的问题；定位后可移除）
    /// </summary>
    private void VerifyPositionWrite(string motorName, uint posHandle, float expected)
    {
        var readBackBuf = new byte[2048];
        var readErr = KHL.KHLReadDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, posHandle, 2, readBackBuf);
        if (readErr == 0)
        {
            Growl.InfoGlobal($"{motorName}: set {expected:F2}, read back {BitConverter.ToSingle(readBackBuf, 0):F2}");
        }
        else
        {
            Growl.WarningGlobal($"{motorName}: read back position failed, err={readErr}");
        }
    }

    /// <summary>
    /// 尝试解析预设位置命令（key 形如 "HeadSweepMotor_Home"）。
    /// 返回 true 时 plcHandle/plcValue 为预设位置命令，plcVelHandle/plcVelValue 为对应电机速度参数；
    /// 返回 false 表示不是预设位置，调用方应按自定义数值处理。
    /// </summary>
    private bool TryGetPresetPosition(string key, out uint plcHandle, out byte[] plcValue,
        out uint plcVelHandle, out byte[] plcVelValue)
    {
        plcHandle = 0;
        plcValue = new byte[2048];
        plcVelHandle = 0;
        plcVelValue = new byte[2048];

        switch (key)
        {
            case "HeadSweepMotor_Home":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadSweep.HomePosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadSweep.HomePosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.HeadSweep.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.HeadSweepMotor.SetVelocity.Value();
                break;
            case "HeadSweepMotor_PolishNear":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadSweep.PolishNearPosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadSweep.PolishNearPosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.HeadSweep.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.HeadSweepMotor.SetVelocity.Value();
                break;
            case "HeadSweepMotor_PolishFar":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadSweep.PolishFarPosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadSweep.PolishFarPosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.HeadSweep.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.HeadSweepMotor.SetVelocity.Value();
                break;
            case "HeadSweepMotor_Dechuck":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadSweep.DechuckPosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadSweep.DechuckPosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.HeadSweep.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.HeadSweepMotor.SetVelocity.Value();
                break;
            case "HeadLiftMotor_High":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.HighPosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadUpDown.HighPosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.HeadLiftMotor.SetVelocity.Value();
                break;
            case "HeadLiftMotor_LoadCup":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.LoadCupPosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadUpDown.LoadCupPosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.HeadLiftMotor.SetVelocity.Value();
                break;
            case "HeadLiftMotor_Polish":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.PolishPosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadUpDown.PolishPosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.HeadLiftMotor.SetVelocity.Value();
                break;
            case "HeadLiftMotor_Clean":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.CleanPosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadUpDown.CleanPosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.HeadLiftMotor.SetVelocity.Value();
                break;
            case "ConditionSweepMotor_Home":
                plcHandle = PlcHandle.CmdPolish.Motor.ConditionerSweep.HomePosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.ConditionerSweep.HomePosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.ConditionerSweep.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.ConditionSweepMotor.SetVelocity.Value();
                break;
            case "ConditionSweepMotor_Start":
                plcHandle = PlcHandle.CmdPolish.Motor.ConditionerSweep.StartPosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.ConditionerSweep.StartPosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.ConditionerSweep.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.ConditionSweepMotor.SetVelocity.Value();
                break;
            case "ConditionSweepMotor_End":
                plcHandle = PlcHandle.CmdPolish.Motor.ConditionerSweep.EndPosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.ConditionerSweep.EndPosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.ConditionerSweep.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.ConditionSweepMotor.SetVelocity.Value();
                break;
            case "SlurrySweepMotor_Home":
                plcHandle = PlcHandle.CmdPolish.Motor.SlurryArmSweep.HomePosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.SlurryArmSweep.HomePosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.SlurryArmSweep.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.SlurrySweepMotor.SetVelocity.Value();
                break;
            case "SlurrySweepMotor_Slurry":
                plcHandle = PlcHandle.CmdPolish.Motor.SlurryArmSweep.SlurryPosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.SlurryArmSweep.SlurryPosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.SlurryArmSweep.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.SlurrySweepMotor.SetVelocity.Value();
                break;
            default:
                return false;
        }

        return true;
    }

    private void PositionCommandExecute(string obj)
    {
        if (!TryGetPresetPosition(obj, out var plcHandle, out var plcValue, out var plcVelHandle, out var plcVelValue))
        {
            Growl.ErrorGlobal($"PositionCommandExecute: 未知位置参数 '{obj}'");
            return;
        }

        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
        if (err != 0)
        {
            Growl.ErrorGlobal($"PositionCommandExecute Velocity Error: {err}");
        }

        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, plcHandle, 1, plcValue);
        if (err != 0)
        {
            Growl.ErrorGlobal($"PositionCommandExecute Error: {err}");
        }
        else
        {
            Growl.SuccessGlobal($"PositionCommandExecute: {obj}");
        }
    }

    private void RunCommandExecute(string obj)
    {
        var plcHandle = 0u;
        var plcValue = new byte[2048];
        var plcVelHandle = 0u;
        var plcVelValue = new byte[2048];
        var plcPosHandle = 0u;
        var plcPosValue = new byte[2048];

        switch (obj)
        {
            case "PlatenMotor":
                // 使用新的结构，保持向后兼容
                plcHandle = PlcHandle.CmdPolish.Motor.PlatenRotation.Run.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.PlatenRotation.Run.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.PlatenRotation.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.PlatenMotor.SetVelocity.Value();
                err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                break;
            case "HeadRotateMotor":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadRotation.Run.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadRotation.Run.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.HeadRotation.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.HeadRotateMotor.SetVelocity.Value();
                err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                break;
            case "DiskRotateMotor":
                plcHandle = PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.Run.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.Run.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.DiskRotateMotor.SetVelocity.Value();
                err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                break;
            case "HeadLiftMotor":
                if (TryGetPresetPosition("HeadLiftMotor_" + SelectedHeadLiftMotorPosition,
                        out plcHandle, out plcValue, out plcVelHandle, out plcVelValue))
                {
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                }
                else if (TryParsePositionValue(SelectedHeadLiftMotorPosition, out var headLiftCustomPos))
                {
                    plcVelHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.Param.Velocity.ParamHandle();
                    plcVelValue = PolishUnit.HeadLiftMotor.SetVelocity.Value();
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                    plcHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.AnyPosition.Handle();
                    plcValue = PlcHandle.CmdPolish.Motor.HeadUpDown.AnyPosition.Value();
                    plcPosHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.Param.Position.ParamHandle();
                    PolishUnit.HeadLiftMotor.SetPosition = headLiftCustomPos;
                    plcPosValue = PolishUnit.HeadLiftMotor.SetPosition.Value();
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcPosHandle, 2, plcPosValue);
                    VerifyPositionWrite("HeadLiftMotor", plcPosHandle, headLiftCustomPos);
                }
                else
                {
                    Growl.ErrorGlobal($"HeadLiftMotor: Invalid position '{SelectedHeadLiftMotorPosition}'. Select a preset or enter a numeric value");
                    return;
                }
                break;
            case "SlurrySweepMotor":
                if (TryGetPresetPosition("SlurrySweepMotor_" + SelectedSlurrySweepMotorPosition,
                        out plcHandle, out plcValue, out plcVelHandle, out plcVelValue))
                {
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                }
                else if (TryParsePositionValue(SelectedSlurrySweepMotorPosition, out var slurryCustomPos))
                {
                    plcVelHandle = PlcHandle.CmdPolish.Motor.SlurryArmSweep.Param.Velocity.ParamHandle();
                    plcVelValue = PolishUnit.SlurrySweepMotor.SetVelocity.Value();
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                    plcHandle = PlcHandle.CmdPolish.Motor.SlurryArmSweep.AnyPosition.Handle();
                    plcValue = PlcHandle.CmdPolish.Motor.SlurryArmSweep.AnyPosition.Value();
                    plcPosHandle = PlcHandle.CmdPolish.Motor.SlurryArmSweep.Param.Position.ParamHandle();
                    PolishUnit.SlurrySweepMotor.SetPosition = slurryCustomPos;
                    plcPosValue = PolishUnit.SlurrySweepMotor.SetPosition.Value();
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcPosHandle, 2, plcPosValue);
                    VerifyPositionWrite("SlurrySweepMotor", plcPosHandle, slurryCustomPos);
                }
                else
                {
                    Growl.ErrorGlobal($"SlurrySweepMotor: Invalid position '{SelectedSlurrySweepMotorPosition}'. Select a preset or enter a numeric value");
                    return;
                }
                break;
            case "HeadSweepMotor":
                if (TryGetPresetPosition("HeadSweepMotor_" + SelectedHeadSweepMotorPosition,
                        out plcHandle, out plcValue, out plcVelHandle, out plcVelValue))
                {
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                }
                else if (TryParsePositionValue(SelectedHeadSweepMotorPosition, out var headSweepCustomPos))
                {
                    plcVelHandle = PlcHandle.CmdPolish.Motor.HeadSweep.Param.Velocity.ParamHandle();
                    plcVelValue = PolishUnit.HeadSweepMotor.SetVelocity.Value();
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                    plcHandle = PlcHandle.CmdPolish.Motor.HeadSweep.AnyPosition.Handle();
                    plcValue = PlcHandle.CmdPolish.Motor.HeadSweep.AnyPosition.Value();
                    plcPosHandle = PlcHandle.CmdPolish.Motor.HeadSweep.Param.Position.ParamHandle();
                    PolishUnit.HeadSweepMotor.SetPosition = headSweepCustomPos;
                    plcPosValue = PolishUnit.HeadSweepMotor.SetPosition.Value();
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcPosHandle, 2, plcPosValue);
                    VerifyPositionWrite("HeadSweepMotor", plcPosHandle, headSweepCustomPos);
                }
                else
                {
                    Growl.ErrorGlobal($"HeadSweepMotor: Invalid position '{SelectedHeadSweepMotorPosition}'. Select a preset or enter a numeric value");
                    return;
                }
                break;
            case "ConditionSweepMotor":
                if (TryGetPresetPosition("ConditionSweepMotor_" + SelectedConditionSweepMotorPosition,
                        out plcHandle, out plcValue, out plcVelHandle, out plcVelValue))
                {
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                }
                else if (TryParsePositionValue(SelectedConditionSweepMotorPosition, out var conditionCustomPos))
                {
                    plcVelHandle = PlcHandle.CmdPolish.Motor.ConditionerSweep.Param.Velocity.ParamHandle();
                    plcVelValue = PolishUnit.ConditionSweepMotor.SetVelocity.Value();
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                    plcHandle = PlcHandle.CmdPolish.Motor.ConditionerSweep.AnyPosition.Handle();
                    plcValue = PlcHandle.CmdPolish.Motor.ConditionerSweep.AnyPosition.Value();
                    plcPosHandle = PlcHandle.CmdPolish.Motor.ConditionerSweep.Param.Position.ParamHandle();
                    PolishUnit.ConditionSweepMotor.SetPosition = conditionCustomPos;
                    plcPosValue = PolishUnit.ConditionSweepMotor.SetPosition.Value();
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcPosHandle, 2, plcPosValue);
                    VerifyPositionWrite("ConditionSweepMotor", plcPosHandle, conditionCustomPos);
                }
                else
                {
                    Growl.ErrorGlobal($"ConditionSweepMotor: Invalid position '{SelectedConditionSweepMotorPosition}'. Select a preset or enter a numeric value");
                    return;
                }
                break;
        }

        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, plcHandle, 1, plcValue);
        if (err != 0)
        {
            Growl.ErrorGlobal($"RunCommandExecute Error: {err}");
        }
        else
        {
            Growl.SuccessGlobal($"RunCommandExecute: {obj}");
        }
    }

    private void AbortCommandExecute(string obj)
    {
        var plcHandle = 0u;
        var plcValue = new byte[2048];
        switch (obj)
        {
            case "PlatenMotor":
                plcHandle = PlcHandle.CmdPolish.Motor.PlatenRotation.Stop.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.PlatenRotation.Stop.Value();
                break;
            case "HeadRotateMotor":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadRotation.Stop.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadRotation.Stop.Value();
                break;
            case "DiskRotateMotor":
                plcHandle = PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.Stop.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.Stop.Value();
                break;
            case "HeadSweepMotor":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadSweep.Stop.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadSweep.Stop.Value();
                break;
            case "ConditionSweepMotor":
                plcHandle = PlcHandle.CmdPolish.Motor.ConditionerSweep.Stop.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.ConditionerSweep.Stop.Value();
                break;
            case "HeadLiftMotor":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.Stop.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadUpDown.Stop.Value();
                break;
            case "SlurrySweepMotor":
                plcHandle = PlcHandle.CmdPolish.Motor.SlurryArmSweep.Stop.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.SlurryArmSweep.Stop.Value();
                break;
        }

        //err = KHL.KHLConnect(_plcAddr, _plcPort, 3000, KHLSockType.SOCK_TCP, ref sock);
        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, plcHandle, 1, plcValue);
        if (err != 0)
        {
            Growl.ErrorGlobal($"AbortCommandExecute Error: {err}");
        }
        else
        {
            Growl.SuccessGlobal($"AbortCommandExecute: {obj}");
        }
    }

    private void ServoCommandExecute(string obj)
    {
        var plcHandle = 0u;
        ushort plcOffset = 0;
        bool currentServoState;
        bool newServoState = false;

        switch (obj)
        {
            case "PlatenMotor":
                currentServoState = PolishUnit.PlatenMotor.IsServoOn;
                newServoState = !currentServoState;

                plcHandle = PlcHandle.CmdPolish.Motor.PlatenRotation.Servo;
                plcOffset = PlcHandle.CmdPolish.Motor.PlatenRotation.Servo.BitOffset();

                PolishUnit.PlatenMotor.IsServoOn = newServoState;
                RaisePropertyChanged(nameof(PlatenMotorServoText));
                break;
            case "HeadRotateMotor":
                currentServoState = PolishUnit.HeadRotateMotor.IsServoOn;
                newServoState = !currentServoState;

                plcHandle = PlcHandle.CmdPolish.Motor.HeadRotation.Servo;
                plcOffset = PlcHandle.CmdPolish.Motor.PlatenRotation.Servo.BitOffset();

                PolishUnit.HeadRotateMotor.IsServoOn = newServoState;
                RaisePropertyChanged(nameof(HeadRotateMotorServoText));
                break;
            case "DiskRotateMotor":
                currentServoState = PolishUnit.DiskRotateMotor.IsServoOn;
                newServoState = !currentServoState;

                plcHandle = PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.Servo;
                plcOffset = PlcHandle.CmdPolish.Motor.PlatenRotation.Servo.BitOffset();

                PolishUnit.DiskRotateMotor.IsServoOn = newServoState;
                RaisePropertyChanged(nameof(DiskRotateMotorServoText));
                break;
            case "HeadSweepMotor":
                currentServoState = PolishUnit.HeadSweepMotor.IsServoOn;
                newServoState = !currentServoState;

                plcHandle = PlcHandle.CmdPolish.Motor.HeadSweep.Servo;
                plcOffset = PlcHandle.CmdPolish.Motor.HeadSweep.Servo.BitOffset();

                PolishUnit.HeadSweepMotor.IsServoOn = newServoState;
                RaisePropertyChanged(nameof(HeadSweepMotorServoText));
                break;
            case "ConditionSweepMotor":
                currentServoState = PolishUnit.ConditionSweepMotor.IsServoOn;
                newServoState = !currentServoState;

                plcHandle = PlcHandle.CmdPolish.Motor.ConditionerSweep.Servo;
                plcOffset = PlcHandle.CmdPolish.Motor.ConditionerSweep.Servo.BitOffset();

                PolishUnit.ConditionSweepMotor.IsServoOn = newServoState;
                RaisePropertyChanged(nameof(ConditionSweepMotorServoText));
                break;
            case "HeadLiftMotor":
                currentServoState = PolishUnit.HeadLiftMotor.IsServoOn;
                newServoState = !currentServoState;

                plcHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.Servo;
                plcOffset = PlcHandle.CmdPolish.Motor.HeadUpDown.Servo.BitOffset();

                PolishUnit.HeadLiftMotor.IsServoOn = newServoState;
                RaisePropertyChanged(nameof(HeadLiftMotorServoText));
                break;
            case "SlurrySweepMotor":
                // SlurrySweepMotor doesn't support servo control in PLC
                Growl.WarningGlobal("SlurrySweepMotor servo control is not supported");
                break;
        }

        err = KHL.KHLWriteDevicesAsBits(kvLink.sock, KHLDevType.DEV_W, plcHandle, plcOffset, 1, newServoState.Value());
        if (err != 0)
        {
            Growl.ErrorGlobal($"ServoCommandExecute Error: {err}");
        }
        else
        {
            string stateText = newServoState ? "ON" : "OFF";
            Growl.SuccessGlobal($"ServoCommandExecute: {obj} - Servo {stateText}");
        }
    }

    private void PressCommandExecute(string obj)
    {
        var zones = GetZonesPress();
        foreach (var (name, addr, plcValue, pressAddr, pressValue) in zones)
        {
            err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, pressAddr, 2, pressValue);
            if (err != 0)
            {
                Growl.ErrorGlobal($"PressCommandSetValueExecute {name} Error: {err}");
            }
            else
            {
                Growl.SuccessGlobal($"PressCommandSetValueExecute: {name}");
            }

            err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, addr, 1, plcValue);
            if (err != 0)
            {
                Growl.ErrorGlobal($"PressCommandExecute {name} Error: {err}");
            }
            else
            {
                Growl.SuccessGlobal($"PressCommandExecute: {name}");
            }
        }
    }

    private void VentCommandExecute(string obj)
    {
        var zones = GetZonesVent();
        foreach (var (name, addr, plcValue) in zones)
        {
            err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, addr, 1, plcValue);
            if (err != 0)
            {
                Growl.ErrorGlobal($"VentCommandExecute {name} Error: {err}");
            }
            else
            {
                Growl.SuccessGlobal($"VentCommandExecute: {name}");
            }
        }
    }

    private void VacuumCommandExecute(string obj)
    {
        var zones = GetZonesVacuum();
        foreach (var (name, addr, plcValue) in zones)
        {
            err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, addr, 1, plcValue);
            if (err != 0)
            {
                Growl.ErrorGlobal($"VacuumCommandExecute {name} Error: {err}");
            }
            else
            {
                Growl.SuccessGlobal($"VacuumCommandExecute: {name}");
            }
        }
    }

    private void UpCommandExecute(string obj)
    {
        var plcHandle = 0u;
        var plcValue = new byte[256];
        switch (obj)
        {
            case "PlatenBreakwater":
                plcHandle = PlcHandle.CmdPolish.Cylinder.PlatenBreakwater.Up.Handle();
                plcValue = PlcHandle.CmdPolish.Cylinder.PlatenBreakwater.Up.Value();
                break;
            case "ConditionHead":
                plcHandle = PlcHandle.CmdPolish.Cylinder.ConditionHead.Up.Handle();
                plcValue = PlcHandle.CmdPolish.Cylinder.ConditionHead.Up.Value();
                break;
        }

        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, plcHandle, 1, plcValue);
        if (err == (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.SuccessGlobal($"Cylinder up command execute: {obj}");
        }
        else
        {
            Growl.ErrorGlobal($"Cylinder up command execute {obj} Error: {err}");
        }
    }

    private void DownCommandExecute(string obj)
    {
        var plcHandle = 0u;
        var plcValue = new byte[256];
        switch (obj)
        {
            case "PlatenBreakwater":
                plcHandle = PlcHandle.CmdPolish.Cylinder.PlatenBreakwater.Down.Handle();
                plcValue = PlcHandle.CmdPolish.Cylinder.PlatenBreakwater.Down.Value();
                break;
            case "ConditionHead":
                plcHandle = PlcHandle.CmdPolish.Cylinder.ConditionHead.Down.Handle();
                plcValue = PlcHandle.CmdPolish.Cylinder.ConditionHead.Down.Value();
                break;
        }

        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, plcHandle, 1, plcValue);
        if (err == (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.SuccessGlobal($"Cylinder down command execute: {obj}");
        }
        else
        {
            Growl.ErrorGlobal($"Cylinder down command execute {obj} Error: {err}");
        }
    }

    private void SetCommandExecute(string obj)
    {
        uint pressureValueHandle = PlcHandle.CmdPolish.Cylinder.ConditionHeadPressure.Param.Pressure.ParamHandle();
        byte[] pressureValue = PolishUnit.ZoneCondition.SetPress.Value();
        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, pressureValueHandle, 2, pressureValue);
        if (err == (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.SuccessGlobal($"ConditionHeadPressure set pressure command execute");
        }
        else
        {
            Growl.ErrorGlobal($"ConditionHeadPressure set pressure command execute Error: {err}");
        }

        uint plcHandle = PlcHandle.CmdPolish.Cylinder.ConditionHeadPressure.Set.Handle();
        byte[] plcValue = PlcHandle.CmdPolish.Cylinder.ConditionHeadPressure.Set.Value();
        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, plcHandle, 1, plcValue);
        if (err == (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.SuccessGlobal($"ConditionHeadPressure set command execute");
        }
        else
        {
            Growl.ErrorGlobal($"ConditionHeadPressure set command execute Error: {err}");
        }
    }

    private void ResetCommandExecute(string obj)
    {
        uint plcHandle = PlcHandle.CmdPolish.Cylinder.ConditionHeadPressure.Reset.Handle();
        byte[] plcValue = PlcHandle.CmdPolish.Cylinder.ConditionHeadPressure.Reset.Value();
        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, plcHandle, 1, plcValue);
        if (err == (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.SuccessGlobal($"ConditionHeadPressure reset command execute");
        }
        else
        {
            Growl.ErrorGlobal($"ConditionHeadPressure reset command execute Error: {err}");
        }
    }

    private void AutoCommandExecute(string obj)
    {
        // TODO: Implement auto command logic
        //LoadCommandExecute(null);
        //PolishCommandExecute(null);
        //UnloadCommandExecute(null);
        //DechuckCommandExecute(null);
        //KvStruct.KvStruct.BoolToByte(ref writeBuf, true);
        //err = KHL.KHLConnect(_plcAddr, _plcPort, 3000, KHLSockType.SOCK_TCP, ref sock);
        //err = KHL.KHLWriteDevicesAsBits(sock, KHLDevType.DEV_ZF, 500000, 0, 1, writeBuf);
        //err = KHL.KHLWriteDevicesAsBits(sock, KHLDevType.DEV_B, 0, 100, 1, writeBuf);
        //Growl.InfoGlobal($"Auto command executed for: {obj}");
        //byte[] writeBufTemp = { 0x02, 25 };
        //KHL.KHLConnect(_plcAddr, _plcPort, 3000, KHLSockType.SOCK_TCP, ref sock);
        //KHL.KHLWriteDevicesAsWords(sock, KHLDevType.DEV_W, Convert.ToUInt32(0x010 + this.Id), 1, writeBufTemp);
    }

    private void AbortUnitCommandExecute(string obj)
    {
        uint addr = PlcHandle.CmdPolish.All.Abort.Handle();
        byte[] value = PlcHandle.CmdPolish.All.Abort.Value();
        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, addr, 1, value);
        if (err == (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.InfoGlobal("Abort command executed");
        }
        else
        {
            Growl.ErrorGlobal($"Failed to Abort: {err}");
        }
    }

    private short ConvertOptionToShort(string value)
    {
        short result = Convert.ToInt16(value.Split(":")[0]);
        return result;
    }

    private void InitializeCommandExecute(string obj)
    {
        uint addr = PlcHandle.CmdPolish.All.Initialize.Handle();
        byte[] value = PlcHandle.CmdPolish.All.Initialize.Value();
        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, addr, 1, value);
        if (err == (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.InfoGlobal("Initialize command executed");
        }
        else
        {
            Growl.ErrorGlobal($"Failed to Initialize: {err}");
        }
    }

    private void AutoModeCommandExecute(string obj)
    {
        uint addr = PlcHandle.CmdPolish.All.Mode.Auto.Handle();
        byte[] value = PlcHandle.CmdPolish.All.Mode.Auto.Value();
        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, addr, 1, value);
        if (err == (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.InfoGlobal("Auto mode command executed");
        }
        else
        {
            Growl.ErrorGlobal($"Failed to switch to auto mode: {err}");
        }

        err = KHL.KHLWriteVariable(kvLink.sock, "PLCAutoStart", writeTrue, true);
        if (err == (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.InfoGlobal("Auto start command executed");
            isPLCAutoStartNeedFalse = true;
        }
        else
        {
            Growl.ErrorGlobal($"Failed to auto start: {err}");
        }
    }

    private void ManualModeCommandExecute(string obj)
    {
        uint addr = PlcHandle.CmdPolish.All.Mode.Manual.Handle();
        byte[] value = PlcHandle.CmdPolish.All.Mode.Manual.Value();
        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, addr, 1, value);
        if (err == (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.InfoGlobal("Manual mode command executed");
        }
        else
        {
            Growl.ErrorGlobal($"Failed to switch to manual mode: {err}");
        }
    }

    private void AutoRunCommandExecute(string obj)
    {
        err = KHL.KHLWriteVariable(kvLink.sock, "PLCAutoStart", writeTrue, true);
        if (err == (int)KHLUserError.KHL_ERR_NONE)
        {

            Growl.InfoGlobal("Auto start command executed");
            isPLCAutoStartNeedFalse = true;
        }
        else
        {
            Growl.ErrorGlobal($"Failed to auto start: {err}");
        }
    }

    private void LoadCommandExecute(string obj)
    {
        if (obj == "Auto")
        {            
            SelectedLoadRecipe = _configLoadRecipe;
        }
        if (string.IsNullOrEmpty(SelectedLoadRecipe))
        {
            Growl.WarningGlobal("Please Select a Load Recipe First");
            return;
        }
        AssignLoadRetryRecipe();
        try
        {
            var recipeContent = LoadRecipeContent("Load Recipe", SelectedLoadRecipe);
            if (recipeContent != null)
            {
                // TODO: Use the recipe content to execute load operation
                KST.stLoadRecipeFormat loadRecipe = new KST.stLoadRecipeFormat();
                for (int i = 0; i < recipeContent.Data?[0].Steps.Count; i++)
                {
                    loadRecipe.ProcessType[i] = GetRealVal<short>(recipeContent, "ProcessType", i);
                    loadRecipe.StepControl[i] = GetRealVal<short>(recipeContent, "StepControl", i);
                    loadRecipe.ZoneMinPressure[i] = GetRealVal<float>(recipeContent, "ZoneMinPressure", i);
                    loadRecipe.ZoneMaxPressure[i] = GetRealVal<float>(recipeContent, "ZoneMaxPressure", i);
                    loadRecipe.MaxTime[i] = GetRealVal<float>(recipeContent, "MaxTime", i);
                    loadRecipe.Zone1Type[i] = GetRealVal<short>(recipeContent, "Zone1Type", i);
                    loadRecipe.Zone2Type[i] = GetRealVal<short>(recipeContent, "Zone2Type", i);
                    loadRecipe.Zone3Type[i] = GetRealVal<short>(recipeContent, "Zone3Type", i);
                    loadRecipe.Zone4Type[i] = GetRealVal<short>(recipeContent, "Zone4Type", i);
                    loadRecipe.Zone5Type[i] = GetRealVal<short>(recipeContent, "Zone5Type", i);
                    loadRecipe.Zone6Type[i] = GetRealVal<short>(recipeContent, "Zone6Type", i);
                    loadRecipe.Zone7Type[i] = GetRealVal<short>(recipeContent, "Zone7Type", i);
                    loadRecipe.Zone8Type[i] = GetRealVal<short>(recipeContent, "Zone8Type", i);
                    loadRecipe.RRType[i] = GetRealVal<short>(recipeContent, "RRType", i);
                    loadRecipe.Zone1Pressure[i] = GetRealVal<float>(recipeContent, "Zone1Pressure", i);
                    loadRecipe.Zone2Pressure[i] = GetRealVal<float>(recipeContent, "Zone2Pressure", i);
                    loadRecipe.Zone3Pressure[i] = GetRealVal<float>(recipeContent, "Zone3Pressure", i);
                    loadRecipe.Zone4Pressure[i] = GetRealVal<float>(recipeContent, "Zone4Pressure", i);
                    loadRecipe.Zone5Pressure[i] = GetRealVal<float>(recipeContent, "Zone5Pressure", i);
                    loadRecipe.Zone6Pressure[i] = GetRealVal<float>(recipeContent, "Zone6Pressure", i);
                    loadRecipe.Zone7Pressure[i] = GetRealVal<float>(recipeContent, "Zone7Pressure", i);
                    loadRecipe.Zone8Pressure[i] = GetRealVal<float>(recipeContent, "Zone8Pressure", i);
                    loadRecipe.ZoneRRPressure[i] = GetRealVal<float>(recipeContent, "ZoneRRPressure", i);
                    loadRecipe.HeadMiddleWash[i] = GetRealVal<short>(recipeContent, "HeadMiddleWash", i);
                    loadRecipe.HeadExternalWash[i] = GetRealVal<short>(recipeContent, "HeadExternalWash", i);
                }

                KST.stLoadRecipeFormat_ToByte(ref writeBuf, loadRecipe, 1, 0);
                if (KHLConnectAndOperation("structLoadRecipe", writeBuf) == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"Assign Load Recipe {SelectedLoadRecipe} Successful");
                    if (obj == "Chamber")
                    {
                        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W,
                            PlcHandle.CmdPolish.Process.SingleProcessStart.LoadStart.Handle(), 1,
                            PlcHandle.CmdPolish.Process.SingleProcessStart.LoadStart.Value());
                    }
                }
                else
                {
                    Growl.ErrorGlobal($"Assign Load Recipe {SelectedLoadRecipe} Failed:{err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find Load Recipe: {SelectedLoadRecipe}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error Assign Load Recipe: {ex.Message}");
            Logger.Error("Error Assign Load Recipe", ex);
        }
    }

    private void PolishCommandExecute(string obj)
    {
        List<string> listHeadSweepRecipe = new List<string>();
        listHeadSweepRecipe.add("None");
        List<string> listConditionRecipe = new List<string>();
        listConditionRecipe.add("None");

        if (string.IsNullOrEmpty(SelectedPolishRecipe))
        {
            Growl.WarningGlobal("Please select a Polish recipe first");
            return;
        }

        try
        {
            KST.stEPD stEPD = new KST.stEPD();
            err = KHL.KHLReadVariable(kvLink.sock, "structEPD", readBuf, true);
            KST.ByteTo_stEPD(ref stEPD, readBuf);
            stEPD.newWafer = true;
            stEPD.posCount = 200;
            stEPD.waferIndex = Convert.ToUInt16(this.waferIndex);
            byte[] polishRecipeName = new byte[202];
            polishRecipeName = Encoding.UTF8.GetBytes(SelectedPolishRecipe);
            for (int i = 0; i < stEPD.PolishRecipeName.Length; i++)
            {
                if (i < polishRecipeName.Length)
                {
                    stEPD.PolishRecipeName[i] = polishRecipeName[i];
                }
                else
                {
                    stEPD.PolishRecipeName[i] = 0x0;
                }
            }
            //KST.StringToByte(ref stEPD.PolishRecipeName, polishRecipeName, 1, 0, SelectedPolishRecipe.length());
            KST.stEPD_ToByte(ref writeBuf, stEPD);
            err = KHL.KHLWriteVariable(kvLink.sock, "structEPD", writeBuf, true);
            Thread.Sleep(50);

            var recipeContent = LoadRecipeContent("Polish Recipe", SelectedPolishRecipe);
            if (recipeContent != null)
            {
                KST.stPolishRecipeFormat PolishRecipe = new KST.stPolishRecipeFormat();
                for (int i = 0; i < recipeContent.Data?[0].Steps.Count; i++)
                {
                    PolishRecipe.ProcessType[i] = GetRealVal<short>(recipeContent, "ProcessType", i);
                    PolishRecipe.ProcessTime[i] = GetRealVal<float>(recipeContent, "ProcessTime", i);                    
                    PolishRecipe.JumpStepNo[i] = GetRealVal<short>(recipeContent, "JumpStepNo", i);
                    PolishRecipe.PlatenRotation[i] = GetRealVal<float>(recipeContent, "PlatenRotation", i);
                    PolishRecipe.HeadRotation[i] = GetRealVal<float>(recipeContent, "HeadRotation", i);
                    PolishRecipe.HeadSweepRecipe[i] = AssignHeadSweepRecipe(ref listHeadSweepRecipe, GetRealVal<string>(recipeContent, "HeadSweepRecipe", i)!);
                    PolishRecipe.Zone1Type[i] = GetRealVal<short>(recipeContent, "Zone1Type", i);
                    PolishRecipe.Zone2Type[i] = GetRealVal<short>(recipeContent, "Zone2Type", i);
                    PolishRecipe.Zone3Type[i] = GetRealVal<short>(recipeContent, "Zone3Type", i);
                    PolishRecipe.Zone4Type[i] = GetRealVal<short>(recipeContent, "Zone4Type", i);
                    PolishRecipe.Zone5Type[i] = GetRealVal<short>(recipeContent, "Zone5Type", i);
                    PolishRecipe.Zone6Type[i] = GetRealVal<short>(recipeContent, "Zone6Type", i);
                    PolishRecipe.Zone7Type[i] = GetRealVal<short>(recipeContent, "Zone7Type", i);
                    PolishRecipe.Zone8Type[i] = GetRealVal<short>(recipeContent, "Zone8Type", i);
                    PolishRecipe.TopRingType[i] = GetRealVal<short>(recipeContent, "TopRingType", i);
                    PolishRecipe.Zone1Pressure[i] = GetRealVal<float>(recipeContent, "Zone1Pressure", i);
                    PolishRecipe.Zone2Pressure[i] = GetRealVal<float>(recipeContent, "Zone2Pressure", i);
                    PolishRecipe.Zone3Pressure[i] = GetRealVal<float>(recipeContent, "Zone3Pressure", i);
                    PolishRecipe.Zone4Pressure[i] = GetRealVal<float>(recipeContent, "Zone4Pressure", i);
                    PolishRecipe.Zone5Pressure[i] = GetRealVal<float>(recipeContent, "Zone5Pressure", i);
                    PolishRecipe.Zone6Pressure[i] = GetRealVal<float>(recipeContent, "Zone6Pressure", i);
                    PolishRecipe.Zone7Pressure[i] = GetRealVal<float>(recipeContent, "Zone7Pressure", i);
                    PolishRecipe.Zone8Pressure[i] = GetRealVal<float>(recipeContent, "Zone8Pressure", i);
                    PolishRecipe.ZoneRRPressure[i] = GetRealVal<float>(recipeContent, "ZoneRRPressure", i);
                    PolishRecipe.ConditionRecipe[i] = AssignConditionRecipe(ref listConditionRecipe, GetRealVal<string>(recipeContent, "ConditionRecipe", i)!);
                    PolishRecipe.PlantWash[i] = GetRealVal<short>(recipeContent, "PlantWash", i);
                    PolishRecipe.SlurryLine1Type[i] = GetRealVal<short>(recipeContent, "SlurryLine1Type", i);
                    PolishRecipe.SlurryLine2Type[i] = GetRealVal<short>(recipeContent, "SlurryLine2Type", i);
                    PolishRecipe.SlurryLine3Type[i] = GetRealVal<short>(recipeContent, "SlurryLine3Type", i);
                    PolishRecipe.SlurryLine1Flow[i] = GetRealVal<float>(recipeContent, "SlurryLine1Flow", i);
                    PolishRecipe.SlurryLine2Flow[i] = GetRealVal<float>(recipeContent, "SlurryLine2Flow", i);
                    PolishRecipe.SlurryLine3Flow[i] = GetRealVal<float>(recipeContent, "SlurryLine3Flow", i);
                    PolishRecipe.DIW[i] = GetRealVal<short>(recipeContent, "DIW", i);
                    PolishRecipe.PolishHeight[i] = GetRealVal<float>(recipeContent, "PolishHeight", i);
                    PolishRecipe.SlurryStartPosition[i] = GetRealVal<float>(recipeContent, "SlurryStartPosition", i);
                    PolishRecipe.SlurryEndPosition[i] = GetRealVal<float>(recipeContent, "SlurryEndPosition", i);
                    PolishRecipe.SlurrySweepVelocity[i] = GetRealVal<float>(recipeContent, "SlurrySweepVelocity", i);
                }

                KST.stPolishRecipeFormat_ToByte(ref writeBuf, PolishRecipe, 1, 0);
                err = KHLConnectAndOperation("structPolishRecipe", writeBuf);
                if (err == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"Assign Polish Recipe {SelectedPolishRecipe} Successful");
                    if (obj == "Chamber")
                    {
                        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W,
                            PlcHandle.CmdPolish.Process.SingleProcessStart.PolishStart.Handle(), 1,
                            PlcHandle.CmdPolish.Process.SingleProcessStart.PolishStart.Value());
                    }
                }
                else
                {
                    Growl.ErrorGlobal($"Assign Polish Recipe {SelectedPolishRecipe} Failed:{err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find Polish Recipe: {SelectedPolishRecipe}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error Assign Polish Recipe: {ex.Message}");
            Logger.Error("Error Assign Polish Recipe", ex);
        }
    }

    private void BreakInCommandExecute(string obj)
    {
        List<string> listConditionSweepRecipe = new List<string>();
        listConditionSweepRecipe.add("None");

        if (string.IsNullOrEmpty(SelectedConditionRecipe))
        {
            Growl.WarningGlobal("Please select a Condition recipe first");
            return;
        }

        try
        {
            var recipeContent = LoadRecipeContent("Condition Recipe", SelectedConditionRecipe);
            if (recipeContent != null)
            {
                KST.stExsituRecipeFormat exsituConditionRecipe = new KST.stExsituRecipeFormat();
                for (int i = 0; i < recipeContent.Data?[0].Steps.Count; i++)
                {
                    exsituConditionRecipe.ProcessType[i] = GetRealVal<short>(recipeContent, "ProcessType", i);
                    exsituConditionRecipe.ProcessTime[i] = GetRealVal<float>(recipeContent, "ProcessTime", i);
                    exsituConditionRecipe.InSitu[i] = GetRealVal<short>(recipeContent, "InSitu", i);
                    exsituConditionRecipe.PlatenRotation[i] = GetRealVal<float>(recipeContent, "PlatenRotation", i);
                    exsituConditionRecipe.ConditionHeadRotation[i] = GetRealVal<float>(recipeContent, "ConditionHeadRotation", i);
                    exsituConditionRecipe.ConditionPressure[i] = GetRealVal<float>(recipeContent, "ConditionPressure", i);
                    exsituConditionRecipe.ConditionRecipe[i] = AssignConditionSweepRecipe(ref listConditionSweepRecipe, GetRealVal<string>(recipeContent, "ConditionSweepRecipe", i)!, "Exsitu");
                    exsituConditionRecipe.PlantWash[i] = GetRealVal<short>(recipeContent, "PlantWash", i);
                    exsituConditionRecipe.SlurryLine1Type[i] = GetRealVal<short>(recipeContent, "SlurryLine1Type", i);
                    exsituConditionRecipe.SlurryLine2Type[i] = GetRealVal<short>(recipeContent, "SlurryLine2Type", i);
                    exsituConditionRecipe.SlurryLine3Type[i] = GetRealVal<short>(recipeContent, "SlurryLine3Type", i);
                    exsituConditionRecipe.SlurryLine1Flow[i] = GetRealVal<float>(recipeContent, "SlurryLine1Flow", i);
                    exsituConditionRecipe.SlurryLine2Flow[i] = GetRealVal<float>(recipeContent, "SlurryLine2Flow", i);
                    exsituConditionRecipe.SlurryLine3Flow[i] = GetRealVal<float>(recipeContent, "SlurryLine3Flow", i);
                    exsituConditionRecipe.SweepStopTimeType[i] = GetRealVal<short>(recipeContent, "SweepStopTimeType", i);
                    exsituConditionRecipe.Ratio[i] = GetRealVal<float>(recipeContent, "Ratio", i);
                }

                KST.stExsituRecipeFormat_ToByte(ref writeBuf, exsituConditionRecipe, 1, 0);
                err = KHLConnectAndOperation("structExsituRecipe", writeBuf);
                if (err == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"Assign BreadIn Recipe {SelectedConditionRecipe} Successful");

                    if (obj == "Chamber")
                    {
                        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W,
                            PlcHandle.CmdPolish.Process.SingleProcessStart.ExsituStart.Handle(), 1,
                            PlcHandle.CmdPolish.Process.SingleProcessStart.ExsituStart.Value());
                    }
                }
                else
                {
                    Growl.ErrorGlobal($"Assign BreadIn Recipe {SelectedConditionRecipe} Successful:{err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find BreakIn Recipe: {SelectedConditionRecipe}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error Assign BreakIn Recipe: {ex.Message}");
            Logger.Error("Error Assign BreakIn Recipe", ex);
        }
    }

    private void UnloadCommandExecute(string obj)
    {
        if (obj == "Auto")
        {
            SelectedUnloadRecipe = _configUnLoadRecipe;
        }
        if (string.IsNullOrEmpty(SelectedUnloadRecipe))
        {
            Growl.WarningGlobal("Please select an Unload recipe first");
            return;
        }
        AssignUnloadRetryRecipe();
        try
        {
            var recipeContent = LoadRecipeContent("Unload Recipe", SelectedUnloadRecipe);
            if (recipeContent != null)
            {
                // TODO: Use the recipe content to execute load operation
                KST.stUnloadRecipeFormat unloadRecipe = new KST.stUnloadRecipeFormat();
                for (int i = 0; i < recipeContent.Data?[0].Steps.Count; i++)
                {
                    unloadRecipe.ProcessType[i] = GetRealVal<short>(recipeContent, "ProcessType", i);
                    unloadRecipe.StepControl[i] = GetRealVal<short>(recipeContent, "StepControl", i);
                    unloadRecipe.ZoneMinPressure[i] = GetRealVal<float>(recipeContent, "ZoneMinPressure", i);
                    unloadRecipe.ZoneMaxPressure[i] = GetRealVal<float>(recipeContent, "ZoneMaxPressure", i);
                    unloadRecipe.MaxTime[i] = GetRealVal<float>(recipeContent, "MaxTime", i);
                    unloadRecipe.Zone1Type[i] = GetRealVal<short>(recipeContent, "Zone1Type", i);
                    unloadRecipe.Zone2Type[i] = GetRealVal<short>(recipeContent, "Zone2Type", i);
                    unloadRecipe.Zone3Type[i] = GetRealVal<short>(recipeContent, "Zone3Type", i);
                    unloadRecipe.Zone4Type[i] = GetRealVal<short>(recipeContent, "Zone4Type", i);
                    unloadRecipe.Zone5Type[i] = GetRealVal<short>(recipeContent, "Zone5Type", i);
                    unloadRecipe.Zone6Type[i] = GetRealVal<short>(recipeContent, "Zone6Type", i);
                    unloadRecipe.Zone7Type[i] = GetRealVal<short>(recipeContent, "Zone7Type", i);
                    unloadRecipe.Zone8Type[i] = GetRealVal<short>(recipeContent, "Zone8Type", i);
                    unloadRecipe.RRType[i] = GetRealVal<short>(recipeContent, "RRType", i);
                    unloadRecipe.Zone1Pressure[i] = GetRealVal<float>(recipeContent, "Zone1Pressure", i);
                    unloadRecipe.Zone2Pressure[i] = GetRealVal<float>(recipeContent, "Zone2Pressure", i);
                    unloadRecipe.Zone3Pressure[i] = GetRealVal<float>(recipeContent, "Zone3Pressure", i);
                    unloadRecipe.Zone4Pressure[i] = GetRealVal<float>(recipeContent, "Zone4Pressure", i);
                    unloadRecipe.Zone5Pressure[i] = GetRealVal<float>(recipeContent, "Zone5Pressure", i);
                    unloadRecipe.Zone6Pressure[i] = GetRealVal<float>(recipeContent, "Zone6Pressure", i);
                    unloadRecipe.Zone7Pressure[i] = GetRealVal<float>(recipeContent, "Zone7Pressure", i);
                    unloadRecipe.Zone8Pressure[i] = GetRealVal<float>(recipeContent, "Zone8Pressure", i);
                    unloadRecipe.ZoneRRPressure[i] = GetRealVal<float>(recipeContent, "ZoneRRPressure", i);
                    unloadRecipe.HeadMiddleWash[i] = GetRealVal<short>(recipeContent, "HeadMiddleWash", i);
                    unloadRecipe.HeadExternalWash[i] = GetRealVal<short>(recipeContent, "HeadExternalWash", i);
                    unloadRecipe.HeadRotation[i] = GetRealVal<float>(recipeContent, "HeadRotation", i);
                    unloadRecipe.BlowWFSelect[i] = GetRealVal<short>(recipeContent, "BlowWFSelect", i);
                }

                //KST.stLoadRecipeFormat_ToByte(ref writeBuf, loadRecipe, 1, 0);
                KST.stUnloadRecipeFormat_ToByte(ref writeBuf, unloadRecipe, 1, 0);
                err = KHLConnectAndOperation("structUnloadRecipe", writeBuf);
                if (KHLConnectAndOperation("structUnloadRecipe", writeBuf) == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"Assign Unload Recipe Successful: {SelectedUnloadRecipe}");
                    if (obj == "Chamber")
                    {
                        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W,
                            PlcHandle.CmdPolish.Process.SingleProcessStart.UnloadStart.Handle(), 1,
                            PlcHandle.CmdPolish.Process.SingleProcessStart.UnloadStart.Value());
                    }
                }
                else
                {
                    Growl.ErrorGlobal($"Assign Unload Recipe Failed: {SelectedUnloadRecipe}:{err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find Unload Recipe: {SelectedUnloadRecipe}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error Assign Unload Recipe: {ex.Message}");
            Logger.Error("Error Assign Unload Recipe", ex);
        }
    }

    private void DechuckCommandExecute(string obj)
    {
        if (obj == "Auto")
        {
            SelectedDechuckRecipe = _configDechukRecipe;
        }
        if (string.IsNullOrEmpty(SelectedDechuckRecipe))
        {
            Growl.WarningGlobal("Please select an dechuck recipe first");
            return;
        }
        AssignDechuckRetryRecipe();
        try
        {
            var recipeContent = LoadRecipeContent("Dechuck Recipe", SelectedDechuckRecipe);
            if (recipeContent != null)
            {
                // TODO: Use the recipe content to execute load operation
                KST.stChuckRecipeFormat dechuckRecipe = new KST.stChuckRecipeFormat();
                for (int i = 0; i < recipeContent.Data?[0].Steps.Count; i++)
                {
                    dechuckRecipe.ProcessType[i] = GetRealVal<short>(recipeContent, "ProcessType", i);
                    dechuckRecipe.StepControl[i] = GetRealVal<short>(recipeContent, "StepControl", i);
                    dechuckRecipe.ZoneMinPressure[i] = GetRealVal<float>(recipeContent, "ZoneMinPressure", i);
                    dechuckRecipe.ZoneMaxPressure[i] = GetRealVal<float>(recipeContent, "ZoneMaxPressure", i);
                    dechuckRecipe.MaxTime[i] = GetRealVal<float>(recipeContent, "MaxTime", i);
                    dechuckRecipe.PlatenRotation[i] = GetRealVal<float>(recipeContent, "PlatenRotation", i);
                    dechuckRecipe.HeadRotation[i] = GetRealVal<float>(recipeContent, "HeadRotation", i);
                    dechuckRecipe.Zone1Type[i] = GetRealVal<short>(recipeContent, "Zone1Type", i);
                    dechuckRecipe.Zone2Type[i] = GetRealVal<short>(recipeContent, "Zone2Type", i);
                    dechuckRecipe.Zone3Type[i] = GetRealVal<short>(recipeContent, "Zone3Type", i);
                    dechuckRecipe.Zone4Type[i] = GetRealVal<short>(recipeContent, "Zone4Type", i);
                    dechuckRecipe.Zone5Type[i] = GetRealVal<short>(recipeContent, "Zone5Type", i);
                    dechuckRecipe.Zone6Type[i] = GetRealVal<short>(recipeContent, "Zone6Type", i);
                    dechuckRecipe.Zone7Type[i] = GetRealVal<short>(recipeContent, "Zone7Type", i);
                    dechuckRecipe.Zone8Type[i] = GetRealVal<short>(recipeContent, "Zone8Type", i);
                    dechuckRecipe.ZoneRRType[i] = GetRealVal<short>(recipeContent, "ZoneRRType", i);
                    dechuckRecipe.Zone1Pressure[i] = GetRealVal<float>(recipeContent, "Zone1Pressure", i);
                    dechuckRecipe.Zone2Pressure[i] = GetRealVal<float>(recipeContent, "Zone2Pressure", i);
                    dechuckRecipe.Zone3Pressure[i] = GetRealVal<float>(recipeContent, "Zone3Pressure", i);
                    dechuckRecipe.Zone4Pressure[i] = GetRealVal<float>(recipeContent, "Zone4Pressure", i);
                    dechuckRecipe.Zone5Pressure[i] = GetRealVal<float>(recipeContent, "Zone5Pressure", i);
                    dechuckRecipe.Zone6Pressure[i] = GetRealVal<float>(recipeContent, "Zone6Pressure", i);
                    dechuckRecipe.Zone7Pressure[i] = GetRealVal<float>(recipeContent, "Zone7Pressure", i);
                    dechuckRecipe.Zone8Pressure[i] = GetRealVal<float>(recipeContent, "Zone8Pressure", i);
                    dechuckRecipe.ZoneRRPressure[i] = GetRealVal<float>(recipeContent, "ZoneRRPressure", i);
                    dechuckRecipe.PlantWash[i] = GetRealVal<short>(recipeContent, "PlantWash", i);
                    dechuckRecipe.DechuckHeight[i] = GetRealVal<float>(recipeContent, "DechuckHeight", i);
                }

                KST.stChuckRecipeFormat_ToByte(ref writeBuf, dechuckRecipe, 1, 0);
                err = KHLConnectAndOperation("structDechuckRecipe", writeBuf);
                if (KHLConnectAndOperation("structDechuckRecipe", writeBuf) == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"Assign Dechuck Recipe Successful: {SelectedDechuckRecipe}");
                    if (obj == "Chamber")
                    {
                        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W,
                            PlcHandle.CmdPolish.Process.SingleProcessStart.DechuckStart.Handle(), 1,
                            PlcHandle.CmdPolish.Process.SingleProcessStart.DechuckStart.Value());
                    }
                }
                else
                {
                    Growl.ErrorGlobal($"Assign Dechuck Recipe Failed: {SelectedDechuckRecipe}:{err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find Dechuck Recipe: {SelectedDechuckRecipe}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error Assign Dechuck Recipe: {ex.Message}");
            Logger.Error("Error Assign Dechuck Recipe", ex);
        }
    }

    private void AssignLoadRetryRecipe()
    {
        if (string.IsNullOrEmpty(SelectedLoadRetryRecipe))
        {
            Growl.WarningGlobal("Please Select a Load Retry Recipe First");
            return;
        }
        try
        {
            var recipeContent = LoadRecipeContent("Load Recipe", SelectedLoadRetryRecipe);
            if (recipeContent != null)  
            {
                KST.stLoadRecipeFormat loadRecipe = new KST.stLoadRecipeFormat();
                for (int i = 0; i < recipeContent.Data?[0].Steps.Count; i++)
                {
                    loadRecipe.ProcessType[i] = GetRealVal<short>(recipeContent, "ProcessType", i);
                    loadRecipe.StepControl[i] = GetRealVal<short>(recipeContent, "StepControl", i);
                    loadRecipe.ZoneMinPressure[i] = GetRealVal<float>(recipeContent, "ZoneMinPressure", i);
                    loadRecipe.ZoneMaxPressure[i] = GetRealVal<float>(recipeContent, "ZoneMaxPressure", i);
                    loadRecipe.MaxTime[i] = GetRealVal<float>(recipeContent, "MaxTime", i);
                    loadRecipe.Zone1Type[i] = GetRealVal<short>(recipeContent, "Zone1Type", i);
                    loadRecipe.Zone2Type[i] = GetRealVal<short>(recipeContent, "Zone2Type", i);
                    loadRecipe.Zone3Type[i] = GetRealVal<short>(recipeContent, "Zone3Type", i);
                    loadRecipe.Zone4Type[i] = GetRealVal<short>(recipeContent, "Zone4Type", i);
                    loadRecipe.Zone5Type[i] = GetRealVal<short>(recipeContent, "Zone5Type", i);
                    loadRecipe.Zone6Type[i] = GetRealVal<short>(recipeContent, "Zone6Type", i);
                    loadRecipe.Zone7Type[i] = GetRealVal<short>(recipeContent, "Zone7Type", i);
                    loadRecipe.Zone8Type[i] = GetRealVal<short>(recipeContent, "Zone8Type", i);
                    loadRecipe.RRType[i] = GetRealVal<short>(recipeContent, "RRType", i);
                    loadRecipe.Zone1Pressure[i] = GetRealVal<float>(recipeContent, "Zone1Pressure", i);
                    loadRecipe.Zone2Pressure[i] = GetRealVal<float>(recipeContent, "Zone2Pressure", i);
                    loadRecipe.Zone3Pressure[i] = GetRealVal<float>(recipeContent, "Zone3Pressure", i);
                    loadRecipe.Zone4Pressure[i] = GetRealVal<float>(recipeContent, "Zone4Pressure", i);
                    loadRecipe.Zone5Pressure[i] = GetRealVal<float>(recipeContent, "Zone5Pressure", i);
                    loadRecipe.Zone6Pressure[i] = GetRealVal<float>(recipeContent, "Zone6Pressure", i);
                    loadRecipe.Zone7Pressure[i] = GetRealVal<float>(recipeContent, "Zone7Pressure", i);
                    loadRecipe.Zone8Pressure[i] = GetRealVal<float>(recipeContent, "Zone8Pressure", i);
                    loadRecipe.ZoneRRPressure[i] = GetRealVal<float>(recipeContent, "ZoneRRPressure", i);
                    loadRecipe.HeadMiddleWash[i] = GetRealVal<short>(recipeContent, "HeadMiddleWash", i);
                    loadRecipe.HeadExternalWash[i] = GetRealVal<short>(recipeContent, "HeadExternalWash", i);
                }

                KST.stLoadRecipeFormat_ToByte(ref writeBuf, loadRecipe, 1, 0);
                if (KHLConnectAndOperation("structLoadRetryRecipe", writeBuf) == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"Assign Load Retry Recipe {SelectedLoadRetryRecipe} Successful");
                }
                else
                {
                    Growl.ErrorGlobal($"Assign Load Retry Recipe {SelectedLoadRetryRecipe} Failed:{err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find Load Retry Recipe: {SelectedLoadRetryRecipe}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error Assign Load Retry Recipe: {ex.Message}");
            Logger.Error("Error Assign Load Retry Recipe", ex);
        }
    }

    private void AssignUnloadRetryRecipe()
    {
        if (string.IsNullOrEmpty(SelectedUnloadRetryRecipe))
        {
            Growl.WarningGlobal("Please Select an Unload Retry Recipe First");
            return;
        }
        try
        {
            var recipeContent = LoadRecipeContent("Unload Recipe", SelectedUnloadRetryRecipe);
            if (recipeContent != null)
            {
                KST.stUnloadRecipeFormat unloadRecipe = new KST.stUnloadRecipeFormat();
                for (int i = 0; i < recipeContent.Data?[0].Steps.Count; i++)
                {
                    unloadRecipe.ProcessType[i] = GetRealVal<short>(recipeContent, "ProcessType", i);
                    unloadRecipe.StepControl[i] = GetRealVal<short>(recipeContent, "StepControl", i);
                    unloadRecipe.ZoneMinPressure[i] = GetRealVal<float>(recipeContent, "ZoneMinPressure", i);
                    unloadRecipe.ZoneMaxPressure[i] = GetRealVal<float>(recipeContent, "ZoneMaxPressure", i);
                    unloadRecipe.MaxTime[i] = GetRealVal<float>(recipeContent, "MaxTime", i);
                    unloadRecipe.Zone1Type[i] = GetRealVal<short>(recipeContent, "Zone1Type", i);
                    unloadRecipe.Zone2Type[i] = GetRealVal<short>(recipeContent, "Zone2Type", i);
                    unloadRecipe.Zone3Type[i] = GetRealVal<short>(recipeContent, "Zone3Type", i);
                    unloadRecipe.Zone4Type[i] = GetRealVal<short>(recipeContent, "Zone4Type", i);
                    unloadRecipe.Zone5Type[i] = GetRealVal<short>(recipeContent, "Zone5Type", i);
                    unloadRecipe.Zone6Type[i] = GetRealVal<short>(recipeContent, "Zone6Type", i);
                    unloadRecipe.Zone7Type[i] = GetRealVal<short>(recipeContent, "Zone7Type", i);
                    unloadRecipe.Zone8Type[i] = GetRealVal<short>(recipeContent, "Zone8Type", i);
                    unloadRecipe.RRType[i] = GetRealVal<short>(recipeContent, "RRType", i);
                    unloadRecipe.Zone1Pressure[i] = GetRealVal<float>(recipeContent, "Zone1Pressure", i);
                    unloadRecipe.Zone2Pressure[i] = GetRealVal<float>(recipeContent, "Zone2Pressure", i);
                    unloadRecipe.Zone3Pressure[i] = GetRealVal<float>(recipeContent, "Zone3Pressure", i);
                    unloadRecipe.Zone4Pressure[i] = GetRealVal<float>(recipeContent, "Zone4Pressure", i);
                    unloadRecipe.Zone5Pressure[i] = GetRealVal<float>(recipeContent, "Zone5Pressure", i);
                    unloadRecipe.Zone6Pressure[i] = GetRealVal<float>(recipeContent, "Zone6Pressure", i);
                    unloadRecipe.Zone7Pressure[i] = GetRealVal<float>(recipeContent, "Zone7Pressure", i);
                    unloadRecipe.Zone8Pressure[i] = GetRealVal<float>(recipeContent, "Zone8Pressure", i);
                    unloadRecipe.ZoneRRPressure[i] = GetRealVal<float>(recipeContent, "ZoneRRPressure", i);
                    unloadRecipe.HeadMiddleWash[i] = GetRealVal<short>(recipeContent, "HeadMiddleWash", i);
                    unloadRecipe.HeadExternalWash[i] = GetRealVal<short>(recipeContent, "HeadExternalWash", i);
                    unloadRecipe.HeadRotation[i] = GetRealVal<float>(recipeContent, "HeadRotation", i);
                    unloadRecipe.BlowWFSelect[i] = GetRealVal<short>(recipeContent, "BlowWFSelect", i);
                }

                KST.stUnloadRecipeFormat_ToByte(ref writeBuf, unloadRecipe, 1, 0);
                if (KHLConnectAndOperation("structUnloadRetryRecipe", writeBuf) == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"Assign Unload Retry Recipe {SelectedUnloadRetryRecipe} Successful");
                }
                else
                {
                    Growl.ErrorGlobal($"Assign Unload Retry Recipe {SelectedUnloadRetryRecipe} Failed:{err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find Unload Retry Recipe: {SelectedUnloadRetryRecipe}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error Assign Unload Retry Recipe: {ex.Message}");
            Logger.Error("Error Assign Unload Retry Recipe", ex);
        }
    }

    private void AssignDechuckRetryRecipe()
    {
        if (string.IsNullOrEmpty(SelectedDechuckRetryRecipe))
        {
            Growl.WarningGlobal("Please Select a Dechuck Retry Recipe First");
            return;
        }
        try
        {
            var recipeContent = LoadRecipeContent("Dechuck Recipe", SelectedDechuckRetryRecipe);
            if (recipeContent != null)
            {
                KST.stChuckRecipeFormat dechuckRecipe = new KST.stChuckRecipeFormat();
                for (int i = 0; i < recipeContent.Data?[0].Steps.Count; i++)
                {
                    dechuckRecipe.ProcessType[i] = GetRealVal<short>(recipeContent, "ProcessType", i);
                    dechuckRecipe.StepControl[i] = GetRealVal<short>(recipeContent, "StepControl", i);
                    dechuckRecipe.ZoneMinPressure[i] = GetRealVal<float>(recipeContent, "ZoneMinPressure", i);
                    dechuckRecipe.ZoneMaxPressure[i] = GetRealVal<float>(recipeContent, "ZoneMaxPressure", i);
                    dechuckRecipe.MaxTime[i] = GetRealVal<float>(recipeContent, "MaxTime", i);
                    dechuckRecipe.PlatenRotation[i] = GetRealVal<float>(recipeContent, "PlatenRotation", i);
                    dechuckRecipe.HeadRotation[i] = GetRealVal<float>(recipeContent, "HeadRotation", i);
                    dechuckRecipe.Zone1Type[i] = GetRealVal<short>(recipeContent, "Zone1Type", i);
                    dechuckRecipe.Zone2Type[i] = GetRealVal<short>(recipeContent, "Zone2Type", i);
                    dechuckRecipe.Zone3Type[i] = GetRealVal<short>(recipeContent, "Zone3Type", i);
                    dechuckRecipe.Zone4Type[i] = GetRealVal<short>(recipeContent, "Zone4Type", i);
                    dechuckRecipe.Zone5Type[i] = GetRealVal<short>(recipeContent, "Zone5Type", i);
                    dechuckRecipe.Zone6Type[i] = GetRealVal<short>(recipeContent, "Zone6Type", i);
                    dechuckRecipe.Zone7Type[i] = GetRealVal<short>(recipeContent, "Zone7Type", i);
                    dechuckRecipe.Zone8Type[i] = GetRealVal<short>(recipeContent, "Zone8Type", i);
                    dechuckRecipe.ZoneRRType[i] = GetRealVal<short>(recipeContent, "ZoneRRType", i);
                    dechuckRecipe.Zone1Pressure[i] = GetRealVal<float>(recipeContent, "Zone1Pressure", i);
                    dechuckRecipe.Zone2Pressure[i] = GetRealVal<float>(recipeContent, "Zone2Pressure", i);
                    dechuckRecipe.Zone3Pressure[i] = GetRealVal<float>(recipeContent, "Zone3Pressure", i);
                    dechuckRecipe.Zone4Pressure[i] = GetRealVal<float>(recipeContent, "Zone4Pressure", i);
                    dechuckRecipe.Zone5Pressure[i] = GetRealVal<float>(recipeContent, "Zone5Pressure", i);
                    dechuckRecipe.Zone6Pressure[i] = GetRealVal<float>(recipeContent, "Zone6Pressure", i);
                    dechuckRecipe.Zone7Pressure[i] = GetRealVal<float>(recipeContent, "Zone7Pressure", i);
                    dechuckRecipe.Zone8Pressure[i] = GetRealVal<float>(recipeContent, "Zone8Pressure", i);
                    dechuckRecipe.ZoneRRPressure[i] = GetRealVal<float>(recipeContent, "ZoneRRPressure", i);
                    dechuckRecipe.PlantWash[i] = GetRealVal<short>(recipeContent, "PlantWash", i);
                    dechuckRecipe.DechuckHeight[i] = GetRealVal<float>(recipeContent, "DechuckHeight", i);
                }

                KST.stChuckRecipeFormat_ToByte(ref writeBuf, dechuckRecipe, 1, 0);
                if (KHLConnectAndOperation("structDeChuckRetryRecipe", writeBuf) == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"Assign Dechuck Retry Recipe {SelectedDechuckRetryRecipe} Successful");
                }
                else
                {
                    Growl.ErrorGlobal($"Assign Dechuck Retry Recipe {SelectedDechuckRetryRecipe} Failed:{err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find Dechuck Retry Recipe: {SelectedDechuckRetryRecipe}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error Assign Dechuck Retry Recipe: {ex.Message}");
            Logger.Error("Error Assign Dechuck Retry Recipe", ex);
        }
    }

    private short AssignHeadSweepRecipe(ref List<string> listHeadSweepRecipe, string headSweepRecipeName)
    {
        if (headSweepRecipeName == "None")
        {
            return 0;
        }

        if (!listHeadSweepRecipe.Contains(headSweepRecipeName))
        {
            listHeadSweepRecipe.Add(headSweepRecipeName);
            var recipeContent = LoadRecipeContent("Head Sweep Recipe", headSweepRecipeName);
            if (recipeContent != null)
            {
                KST.stHeadSweepRecipeFormat headSweepRecipe = new KST.stHeadSweepRecipeFormat();
                headSweepRecipe.HeadSweepType = GetRealVal<short>(recipeContent, "HeadSweepType", 0);
                headSweepRecipe.HeadSweepCount = GetRealVal<short>(recipeContent, "HeadSweepCount", 0);
                headSweepRecipe.StartPosition = GetRealVal<float>(recipeContent, "StartPosition", 0);
                headSweepRecipe.EndPosition = GetRealVal<float>(recipeContent, "EndPosition", 0);
                headSweepRecipe.ZoneCount = GetRealVal<short>(recipeContent, "ZoneCount", 0);
                headSweepRecipe.Zone1Ratio = GetRealVal<float>(recipeContent, "Zone1Ratio", 0);
                headSweepRecipe.Zone2Ratio = GetRealVal<float>(recipeContent, "Zone2Ratio", 0);
                headSweepRecipe.Zone3Ratio = GetRealVal<float>(recipeContent, "Zone3Ratio", 0);
                headSweepRecipe.Zone4Ratio = GetRealVal<float>(recipeContent, "Zone4Ratio", 0);
                headSweepRecipe.Zone5Ratio = GetRealVal<float>(recipeContent, "Zone5Ratio", 0);
                headSweepRecipe.Zone6Ratio = GetRealVal<float>(recipeContent, "Zone6Ratio", 0);
                headSweepRecipe.Zone7Ratio = GetRealVal<float>(recipeContent, "Zone7Ratio", 0);
                headSweepRecipe.Zone8Ratio = GetRealVal<float>(recipeContent, "Zone8Ratio", 0);
                headSweepRecipe.Zone9Ratio = GetRealVal<float>(recipeContent, "Zone9Ratio", 0);
                headSweepRecipe.Zone10Ratio = GetRealVal<float>(recipeContent, "Zone10Ratio", 0);
                headSweepRecipe.Zone11Ratio = GetRealVal<float>(recipeContent, "Zone11Ratio", 0);
                headSweepRecipe.Zone12Ratio = GetRealVal<float>(recipeContent, "Zone12Ratio", 0);
                headSweepRecipe.Zone13Ratio = GetRealVal<float>(recipeContent, "Zone13Ratio", 0);
                headSweepRecipe.Zone14Ratio = GetRealVal<float>(recipeContent, "Zone14Ratio", 0);
                headSweepRecipe.Zone15Ratio = GetRealVal<float>(recipeContent, "Zone15Ratio", 0);
                headSweepRecipe.Zone16Ratio = GetRealVal<float>(recipeContent, "Zone16Ratio", 0);
                headSweepRecipe.Zone17Ratio = GetRealVal<float>(recipeContent, "Zone17Ratio", 0);
                headSweepRecipe.Zone18Ratio = GetRealVal<float>(recipeContent, "Zone18Ratio", 0);
                headSweepRecipe.Zone19Ratio = GetRealVal<float>(recipeContent, "Zone19Ratio", 0);
                headSweepRecipe.Zone20Ratio = GetRealVal<float>(recipeContent, "Zone20Ratio", 0);
                KST.stHeadSweepRecipeFormat_ToByte(ref writeBuf, headSweepRecipe, 1, 0);
                string variableName = $"structHeadSweepRecipe{listHeadSweepRecipe.IndexOf(headSweepRecipeName)}";
                err = KHLConnectAndOperation(variableName, writeBuf);
                if (err == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"Assign HeadSweep Recipe {headSweepRecipeName} Successful");
                }
                else
                {
                    Growl.ErrorGlobal($"Assign HeadSweep Recipe {headSweepRecipeName} Failed:{err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find HeadSweep Recipe: {headSweepRecipeName}");
            }
        }

        return (short)listHeadSweepRecipe.IndexOf(headSweepRecipeName);
    }

    private short AssignConditionRecipe(ref List<string> listConditionRecipe, string conditionRecipeName)
    {
        if (conditionRecipeName == "None")
        {
            return 0;
        }

        List<string> listConditionSweepRecipe = new List<string>();
        listConditionSweepRecipe.add("None");

        if (!listConditionRecipe.Contains(conditionRecipeName))
        {
            listConditionRecipe.add(conditionRecipeName);
            var recipeContent = LoadRecipeContent("Condition Recipe", conditionRecipeName);
            if (recipeContent != null)
            {
                KST.stInsituConditionRecipeFormat conditionRecipe = new KST.stInsituConditionRecipeFormat();
                conditionRecipe.ProcessType = GetRealVal<short>(recipeContent, "ProcessType", 0);
                conditionRecipe.ProcessTime = GetRealVal<float>(recipeContent, "ProcessTime", 0);
                conditionRecipe.InSitu = GetRealVal<short>(recipeContent, "InSitu", 0);
                conditionRecipe.PlatenRotation = GetRealVal<float>(recipeContent, "PlatenRotation", 0);
                conditionRecipe.ConditionHeadRotation = GetRealVal<float>(recipeContent, "ConditionHeadRotation", 0);
                conditionRecipe.ConditionPressure = GetRealVal<float>(recipeContent, "ConditionPressure", 0);
                conditionRecipe.ConditionRecipe = AssignConditionSweepRecipe(ref listConditionSweepRecipe, GetRealVal<string>(recipeContent, "ConditionSweepRecipe", 0)!, "Insitu");
                conditionRecipe.PlatenWash = GetRealVal<short>(recipeContent, "PlatenWash", 0);
                conditionRecipe.SlurryLine1Type = GetRealVal<short>(recipeContent, "SlurryLine1Type", 0);
                conditionRecipe.SlurryLine2Type = GetRealVal<short>(recipeContent, "SlurryLine2Type", 0);
                conditionRecipe.SlurryLine3Type = GetRealVal<short>(recipeContent, "SlurryLine3Type", 0);
                conditionRecipe.SlurryLine1Flow = GetRealVal<float>(recipeContent, "SlurryLine1Flow", 0);
                conditionRecipe.SlurryLine2Flow = GetRealVal<float>(recipeContent, "SlurryLine2Flow", 0);
                conditionRecipe.SlurryLine3Flow = GetRealVal<float>(recipeContent, "SlurryLine3Flow", 0);
                conditionRecipe.SweepStopTimeType = GetRealVal<short>(recipeContent, "SweepStopTimeType", 0);
                conditionRecipe.Ratio = GetRealVal<float>(recipeContent, "Ratio", 0);
                KST.stInsituConditionRecipeFormat_ToByte(ref writeBuf, conditionRecipe, 1, 0);
                string variableName = $"structConditionRecipe{listConditionRecipe.IndexOf(conditionRecipeName)}";
                err = KHLConnectAndOperation(variableName, writeBuf);
                if (err == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"Assign Condition Recipe {conditionRecipeName} Successful");
                }
                else
                {
                    Growl.ErrorGlobal($"Assign Condition Recipe {conditionRecipeName} Failed:{err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find Condition Recipe: {conditionRecipeName}");
            }
        }

        return (short)listConditionRecipe.IndexOf(conditionRecipeName);
    }

    private short AssignConditionSweepRecipe(ref List<string> listConditionSweepRecipe, string conditionSweepRecipeName,
        string type)
    {
        if (conditionSweepRecipeName == "None")
        {
            return 0;
        }

        if (!listConditionSweepRecipe.Contains(conditionSweepRecipeName))
        {
            listConditionSweepRecipe.Add(conditionSweepRecipeName);
            var recipeContent = LoadRecipeContent("Condition Sweep Recipe", conditionSweepRecipeName);
            if (recipeContent != null)
            {
                KST.stConditionSweepRecipeFormat conditionSweepRecipe = new KST.stConditionSweepRecipeFormat();
                conditionSweepRecipe.ConditionSweepType = GetRealVal<short>(recipeContent, "ConditionSweepType", 0);
                conditionSweepRecipe.ConditionSweepCount = GetRealVal<short>(recipeContent, "ConditionSweepCount", 0);
                conditionSweepRecipe.StartPosition = GetRealVal<float>(recipeContent, "StartPosition", 0);
                conditionSweepRecipe.EndPosition = GetRealVal<float>(recipeContent, "EndPosition", 0);
                conditionSweepRecipe.ZoneCount = GetRealVal<short>(recipeContent, "ZoneCount", 0);
                conditionSweepRecipe.Zone1Ratio = GetRealVal<float>(recipeContent, "Zone1Ratio", 0);
                conditionSweepRecipe.Zone2Ratio = GetRealVal<float>(recipeContent, "Zone2Ratio", 0);
                conditionSweepRecipe.Zone3Ratio = GetRealVal<float>(recipeContent, "Zone3Ratio", 0);
                conditionSweepRecipe.Zone4Ratio = GetRealVal<float>(recipeContent, "Zone4Ratio", 0);
                conditionSweepRecipe.Zone5Ratio = GetRealVal<float>(recipeContent, "Zone5Ratio", 0);
                conditionSweepRecipe.Zone6Ratio = GetRealVal<float>(recipeContent, "Zone6Ratio", 0);
                conditionSweepRecipe.Zone7Ratio = GetRealVal<float>(recipeContent, "Zone7Ratio", 0);
                conditionSweepRecipe.Zone8Ratio = GetRealVal<float>(recipeContent, "Zone8Ratio", 0);
                conditionSweepRecipe.Zone9Ratio = GetRealVal<float>(recipeContent, "Zone9Ratio", 0);
                conditionSweepRecipe.Zone10Ratio = GetRealVal<float>(recipeContent, "Zone10Ratio", 0);
                conditionSweepRecipe.Zone11Ratio = GetRealVal<float>(recipeContent, "Zone11Ratio", 0);
                conditionSweepRecipe.Zone12Ratio = GetRealVal<float>(recipeContent, "Zone12Ratio", 0);
                conditionSweepRecipe.Zone13Ratio = GetRealVal<float>(recipeContent, "Zone13Ratio", 0);
                conditionSweepRecipe.Zone14Ratio = GetRealVal<float>(recipeContent, "Zone14Ratio", 0);
                conditionSweepRecipe.Zone15Ratio = GetRealVal<float>(recipeContent, "Zone15Ratio", 0);
                conditionSweepRecipe.Zone16Ratio = GetRealVal<float>(recipeContent, "Zone16Ratio", 0);
                conditionSweepRecipe.Zone17Ratio = GetRealVal<float>(recipeContent, "Zone17Ratio", 0);
                conditionSweepRecipe.Zone18Ratio = GetRealVal<float>(recipeContent, "Zone18Ratio", 0);
                conditionSweepRecipe.Zone19Ratio = GetRealVal<float>(recipeContent, "Zone19Ratio", 0);
                conditionSweepRecipe.Zone20Ratio = GetRealVal<float>(recipeContent, "Zone20Ratio", 0);
                KST.stConditionSweepRecipeFormat_ToByte(ref writeBuf, conditionSweepRecipe, 1, 0);
                string variableName = string.Empty;
                if (type == "Insitu")
                {
                    variableName =
                        $"structConditionSweepRecipe{listConditionSweepRecipe.IndexOf(conditionSweepRecipeName)}";
                }
                else
                {
                    variableName =
                        $"structExsituConditionSweepRecipe{listConditionSweepRecipe.IndexOf(conditionSweepRecipeName)}";
                }

                err = KHLConnectAndOperation(variableName, writeBuf);
                if (err == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"Assign ConditionSweep Recipe {conditionSweepRecipeName} Successful");
                }
                else
                {
                    Growl.ErrorGlobal($"Assign ConditionSweep Recipe {conditionSweepRecipeName} Failed:{err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find ConditionSweep Recipe: {conditionSweepRecipeName}");
            }
        }

        return (short)listConditionSweepRecipe.IndexOf(conditionSweepRecipeName);
    }

    private IEnumerable<(string Name, uint Addr, byte[] Value, uint PressAddr, byte[] PressValue)> GetZonesPress()
    {
        var zones = new[]
        {
            (Name: "Zone1", Addr: PlcHandle.CmdPolish.Gas.Zone1.Press.Handle(), Value: PolishUnit.Zone1.Press.Value(),
                PressAddr: PlcHandle.CmdPolish.Gas.Zone1.Param.Pressure.ParamHandle(),
                PressValue: PolishUnit.Zone1.SetPress.Value()),
            (Name: "Zone2", Addr: PlcHandle.CmdPolish.Gas.Zone2.Press.Handle(), Value: PolishUnit.Zone2.Press.Value(),
                PressAddr: PlcHandle.CmdPolish.Gas.Zone2.Param.Pressure.ParamHandle(),
                PressValue: PolishUnit.Zone2.SetPress.Value()),
            (Name: "Zone3", Addr: PlcHandle.CmdPolish.Gas.Zone3.Press.Handle(), Value: PolishUnit.Zone3.Press.Value(),
                PressAddr: PlcHandle.CmdPolish.Gas.Zone3.Param.Pressure.ParamHandle(),
                PressValue: PolishUnit.Zone3.SetPress.Value()),
            (Name: "Zone4", Addr: PlcHandle.CmdPolish.Gas.Zone4.Press.Handle(), Value: PolishUnit.Zone4.Press.Value(),
                PressAddr: PlcHandle.CmdPolish.Gas.Zone4.Param.Pressure.ParamHandle(),
                PressValue: PolishUnit.Zone4.SetPress.Value()),
            (Name: "Zone5", Addr: PlcHandle.CmdPolish.Gas.Zone5.Press.Handle(), Value: PolishUnit.Zone5.Press.Value(),
                PressAddr: PlcHandle.CmdPolish.Gas.Zone5.Param.Pressure.ParamHandle(),
                PressValue: PolishUnit.Zone5.SetPress.Value()),
            (Name: "Zone6", Addr: PlcHandle.CmdPolish.Gas.Zone6.Press.Handle(), Value: PolishUnit.Zone6.Press.Value(),
                PressAddr: PlcHandle.CmdPolish.Gas.Zone6.Param.Pressure.ParamHandle(),
                PressValue: PolishUnit.Zone6.SetPress.Value()),
            (Name: "Zone7", Addr: PlcHandle.CmdPolish.Gas.Zone7.Press.Handle(), Value: PolishUnit.Zone7.Press.Value(),
                PressAddr: PlcHandle.CmdPolish.Gas.Zone7.Param.Pressure.ParamHandle(),
                PressValue: PolishUnit.Zone7.SetPress.Value()),
            (Name: "Zone8", Addr: PlcHandle.CmdPolish.Gas.Zone8.Press.Handle(), Value: PolishUnit.Zone8.Press.Value(),
                PressAddr: PlcHandle.CmdPolish.Gas.Zone8.Param.Pressure.ParamHandle(),
                PressValue: PolishUnit.Zone8.SetPress.Value()),
            (Name: "Retain", Addr: PlcHandle.CmdPolish.Gas.Retain.Press.Handle(),
                Value: PolishUnit.Retain.Press.Value(),
                PressAddr: PlcHandle.CmdPolish.Gas.Retain.Param.Pressure.ParamHandle(),
                PressValue: PolishUnit.Retain.SetPress.Value()),
        };
        return zones;
    }

    private IEnumerable<(string Name, uint Addr, byte[] Value)> GetZonesVent()
    {
        var zones = new[]
        {
            (Name: "Zone1", Addr: PlcHandle.CmdPolish.Gas.Zone1.Vent.Handle(), Value: PolishUnit.Zone1.Vent.Value()),
            (Name: "Zone2", Addr: PlcHandle.CmdPolish.Gas.Zone2.Vent.Handle(), Value: PolishUnit.Zone2.Vent.Value()),
            (Name: "Zone3", Addr: PlcHandle.CmdPolish.Gas.Zone3.Vent.Handle(), Value: PolishUnit.Zone3.Vent.Value()),
            (Name: "Zone4", Addr: PlcHandle.CmdPolish.Gas.Zone4.Vent.Handle(), Value: PolishUnit.Zone4.Vent.Value()),
            (Name: "Zone5", Addr: PlcHandle.CmdPolish.Gas.Zone5.Vent.Handle(), Value: PolishUnit.Zone5.Vent.Value()),
            (Name: "Zone6", Addr: PlcHandle.CmdPolish.Gas.Zone6.Vent.Handle(), Value: PolishUnit.Zone6.Vent.Value()),
            (Name: "Zone7", Addr: PlcHandle.CmdPolish.Gas.Zone7.Vent.Handle(), Value: PolishUnit.Zone7.Vent.Value()),
            (Name: "Zone8", Addr: PlcHandle.CmdPolish.Gas.Zone8.Vent.Handle(), Value: PolishUnit.Zone8.Vent.Value()),
            (Name: "Retain", Addr: PlcHandle.CmdPolish.Gas.Retain.Vent.Handle(), Value: PolishUnit.Retain.Vent.Value()),
        };
        return zones;
    }

    private IEnumerable<(string Name, uint Addr, byte[] Value)> GetZonesVacuum()
    {
        var zones = new[]
        {
            (Name: "Zone1", Addr: PlcHandle.CmdPolish.Gas.Zone1.Vacuum.Handle(),
                Value: PolishUnit.Zone1.Vacuum.Value()),
            (Name: "Zone2", Addr: PlcHandle.CmdPolish.Gas.Zone2.Vacuum.Handle(),
                Value: PolishUnit.Zone2.Vacuum.Value()),
            (Name: "Zone3", Addr: PlcHandle.CmdPolish.Gas.Zone3.Vacuum.Handle(),
                Value: PolishUnit.Zone3.Vacuum.Value()),
            (Name: "Zone4", Addr: PlcHandle.CmdPolish.Gas.Zone4.Vacuum.Handle(),
                Value: PolishUnit.Zone4.Vacuum.Value()),
            (Name: "Zone5", Addr: PlcHandle.CmdPolish.Gas.Zone5.Vacuum.Handle(),
                Value: PolishUnit.Zone5.Vacuum.Value()),
            (Name: "Zone6", Addr: PlcHandle.CmdPolish.Gas.Zone6.Vacuum.Handle(),
                Value: PolishUnit.Zone6.Vacuum.Value()),
            (Name: "Zone7", Addr: PlcHandle.CmdPolish.Gas.Zone7.Vacuum.Handle(),
                Value: PolishUnit.Zone7.Vacuum.Value()),
            (Name: "Zone8", Addr: PlcHandle.CmdPolish.Gas.Zone8.Vacuum.Handle(),
                Value: PolishUnit.Zone8.Vacuum.Value()),
            (Name: "Retain", Addr: PlcHandle.CmdPolish.Gas.Retain.Vacuum.Handle(),
                Value: PolishUnit.Retain.Vacuum.Value()),
        };
        return zones;
    }

    /// <summary>
    /// Recipe保存事件处理方法
    /// </summary>
    /// <param name="args">事件参数</param>
    private void OnRecipeSaved(RecipeSavedEventArgs args)
    {
        try
        {
            if (args == null || string.IsNullOrEmpty(args.RecipeType))
                return;

            // 根据Recipe类型重新加载对应的列表
            switch (args.RecipeType)
            {
                case "Load Recipe":
                    LoadRecipesFromFolder("Load Recipe", LoadRecipes);
                    // 如果当前选择的Recipe被修改了，保持选择
                    if (!string.IsNullOrEmpty(args.RecipeName) && LoadRecipes.Contains(args.RecipeName))
                    {
                        SelectedLoadRecipe = args.RecipeName;
                    }

                    break;
                case "Condition Recipe":
                    LoadRecipesFromFolder("Condition Recipe", ConditionRecipes);
                    if (!string.IsNullOrEmpty(args.RecipeName) && ConditionRecipes.Contains(args.RecipeName))
                    {
                        // 如果保存的Recipe是当前Pre_Cond或Condition选中的，则保持选中
                        if (SelectedPreCondRecipe == args.RecipeName)
                            SelectedPreCondRecipe = args.RecipeName;
                        if (SelectedConditionRecipe == args.RecipeName)
                            SelectedConditionRecipe = args.RecipeName;
                    }

                    break;
                case "Polish Recipe":
                    LoadRecipesFromFolder("Polish Recipe", PolishRecipes);
                    if (!string.IsNullOrEmpty(args.RecipeName) && PolishRecipes.Contains(args.RecipeName))
                    {
                        SelectedPolishRecipe = args.RecipeName;
                    }

                    break;
                case "Unload Recipe":
                    LoadRecipesFromFolder("Unload Recipe", UnloadRecipes);
                    if (!string.IsNullOrEmpty(args.RecipeName) && UnloadRecipes.Contains(args.RecipeName))
                    {
                        SelectedUnloadRecipe = args.RecipeName;
                    }

                    break;
                case "Dechuck Recipe":
                    LoadRecipesFromFolder("Dechuck Recipe", DechuckRecipes);
                    if (!string.IsNullOrEmpty(args.RecipeName) && DechuckRecipes.Contains(args.RecipeName))
                    {
                        SelectedDechuckRecipe = args.RecipeName;
                    }

                    break;
            }

            Growl.InfoGlobal($"Recipe已更新: {args.RecipeType} - {args.RecipeName}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to reload recipes after save: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 全局变量polishSocks做出改变的通知时，订阅者需要做出变更
    /// </summary>
    private void OnPolishSocketsChanged()
    {
        //kvLink.sock = GlobalService.kvHostLinkInit.PolishSocks[_polishToken];
        //err = KHL.KHLWriteVariable(sock, "PLCStartPermition", writeTrue, true);
        //err = KHL.KHLWriteVariable(sock, "PLCReady", writeTrue, true);
        //err = KHL.KHLWriteVariable(sock, "PLCAutoStart", writeTrue, true);
    }

    /// <summary>
    /// Load recipes from the file system
    /// </summary>
    private void LoadRecipesData()
    {
        try
        {
            if (!Directory.Exists(_recipeBasePath))
            {
                Directory.CreateDirectory(_recipeBasePath);
                return;
            }

            // Load recipes from specific folders

            //LoadRecipesFromFolder("Load Recipe", LoadRecipes);
            //LoadRecipesFromFolder("Unload Recipe", UnloadRecipes);
            //LoadRecipesFromFolder("Dechuck Recipe", DechuckRecipes);

            LoadRecipesFromFolder("Condition Recipe", ConditionRecipes);
            LoadRecipesFromFolder("Polish Recipe", PolishRecipes);
            LoadRecipesFromFolder("Head Sweep Recipe", HeadRecipes);

            // Set default selections if available

            //if (LoadRecipes.Count > 0)
            //    SelectedLoadRecipe = LoadRecipes[0];
            //if (UnloadRecipes.Count > 0)
            //    SelectedUnloadRecipe = UnloadRecipes[0];
            //if (DechuckRecipes.Count > 0)
            //    SelectedDechuckRecipe = DechuckRecipes[0];

            if (ConditionRecipes.Count > 0)
            {
                SelectedPreCondRecipe = ConditionRecipes[0];
                SelectedConditionRecipe = ConditionRecipes[0];
            }
            if (PolishRecipes.Count > 0)
                SelectedPolishRecipe = PolishRecipes[0];
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Failed to load recipes: {ex.Message}");
        }
    }

    /// <summary>
    /// Load recipes from a specific folder
    /// </summary>
    /// <param name="folderName">Folder name to load from</param>
    /// <param name="recipeList">Target recipe list to populate</param>
    private void LoadRecipesFromFolder(string folderName, ObservableCollection<string> recipeList)
    {
        try
        {
            string folderPath = Path.Combine(_recipeBasePath, folderName);

            if (!Directory.Exists(folderPath))
            {
                return;
            }

            // 清空现有列表以避免重复
            recipeList.Clear();

            DirectoryInfo folderInfo = new DirectoryInfo(folderPath);
            var jsonFiles = folderInfo.GetFiles("*.json");

            foreach (var file in jsonFiles)
            {
                try
                {
                    var json = FileUtil.ReadFile(file.FullName);
                    Recipe.Content? content = JsonConvert.DeserializeObject<Recipe.Content>(json);

                    if (content != null && !string.IsNullOrEmpty(content.Name))
                    {
                        if (!recipeList.Contains(content.Name))
                        {
                            recipeList.Add(content.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue processing other files
                    Console.WriteLine($"Error loading recipe from {file.FullName}: {ex.Message}");
                    Logger.Error($"Error loading recipe from {file.FullName}", ex);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading recipes from folder {folderName}: {ex.Message}");
            Logger.Error($"Error loading recipes from folder {folderName}", ex);
        }
    }

    /// <summary>
    /// Load specific recipe content from a folder
    /// </summary>
    /// <param name="folderName">Folder name containing the recipe</param>
    /// <param name="recipeName">Recipe name to load</param>
    /// <returns>Recipe content or null if not found</returns>
    private Recipe.Content? LoadRecipeContent(string folderName, string recipeName)
    {
        try
        {
            string folderPath = Path.Combine(_recipeBasePath, folderName);

            if (!Directory.Exists(folderPath))
            {
                Growl.WarningGlobal($"Recipe folder not found: {folderName}");
                return null;
            }

            // Find the JSON file matching the recipe name
            string recipeFilePath = Path.Combine(folderPath, $"{recipeName}.json");

            if (!File.Exists(recipeFilePath))
            {
                Growl.WarningGlobal($"Recipe file not found: {recipeName}.json");
                return null;
            }

            // Read and deserialize the recipe file
            var json = FileUtil.ReadFile(recipeFilePath);
            Recipe.Content? content = JsonConvert.DeserializeObject<Recipe.Content>(json);

            if (content == null)
            {
                Growl.ErrorGlobal($"Failed to deserialize recipe: {recipeName}");
                return null;
            }

            return content;
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error reading recipe file: {ex.Message}");
            return null;
        }
    }

    private static T? GetRealVal<T>(Recipe.Content recipeModel, string name, int stepIndex)
    {
        var model = recipeModel.Data?
            .FirstOrDefault(item => {
                string itemName = item.Name!.Replace(" ", "")
                            .Replace("_", "")
                            .Replace("-", "")
                            .Replace("&", "");

                return itemName == name || (item.CompatibleName != null && item.CompatibleName.Contains(name));
            }
                );

        if (model == null) return default;
        if (stepIndex < 0 || stepIndex >= model.Steps.Count) return default;

        var value = model.Steps[stepIndex].Value;
        var type = model.Type?.ToLowerInvariant();
        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        if (type == "option" || type == "list")
        {
            var arr = value.Split(':');
            value = targetType == typeof(string) ? (arr.Length >= 2 ? arr[1] : arr[0]) : arr[0];
        }


        try
        {
            if (targetType == typeof(string))
                return (T)(object)value;

            if (targetType.IsEnum)
                return (T)Enum.Parse(targetType, value, ignoreCase: true);

            return (T)Convert.ChangeType(value, targetType);
        }
        catch
        {
            return default;
        }
    }
}

public class SlurryDiw : BindableBase
{
    private bool _diw;
    private bool _slurry;
    private float _flowRate;

    public bool Diw
    {
        get => _diw;
        set => SetProperty(ref _diw, value);
    }

    public bool Slurry
    {
        get => _slurry;
        set => SetProperty(ref _slurry, value);
    }

    public float FlowRate
    {
        get => _flowRate;
        set => SetProperty(ref _flowRate, value);
    }
}