using CMP.Services.Auth;
using CMP.Constants.Plc;
using CMP.Extensions;
using CMP.Lang;
using CMP.Models.DataCenter;
using CMP.Models.kvsPlc;
using CMP.Models.kvsPlc.SV;
using CMP.Models.Operation.Clean;
using CMP.Models.Operation.RoboticArm;
using CMP.Models.Recipe.RecipeEdit;
using CMP.Models.Recipe.Sequence;
using CMP.Models.Resource;
using CMP.Services.Log;
using CMP.Utils;
using CMP.ViewModels.Common;
using HandyControl.Controls;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Threading.Tasks;
using KHL = CMP.Models.kvsPlc.KvHostLink;
using KST = CMP.Models.kvsPlc.Clean.KvStruct;
using KSTPenCollectData = CMP.Models.kvsPlc.Pen.KvsStructPenCollectData;
using RecipeContent = CMP.Models.Recipe.RecipeEdit.Recipe.Content;

namespace CMP.ViewModels.Tabs.OperationTab;

/// <summary>
/// 通用 PenClean Tab Content ViewModel，支持不同的 Pen 配置
/// </summary>
public class PenCleanTabContentViewModel : TabContentViewModelBase
{
    private readonly IEventAggregator _eventAggregator;
    private string? _tabTitle;
    private int _tabId;
    private PenClean _penClean = new();
    private readonly IPenCleanConfig _penCleanConfig;
    private int _err = (int)KHLUserError.KHL_ERR_NONE;
    private KVLink kvLink;
    //private int _sock;
    private int _addressPlcRecipeReq;
    private int _addressPlcRecipeReset;
    private string? _baseChamberAddress;
    private byte[] _writeBuf = new byte[16384];
    private byte[] _readBuf = new byte[1024];
    private byte[] _writeTrue = [1];
    private byte[] _writeFalse = [0];

    private int _delayCycle;
    private bool _isPlcAutoStartNeedFalse;
    private bool _isResetRecipeState;

    private string _varNameRecipeStruct;
    private string _recipeBasePath = "D:\\CMP\\Configuration\\Recipes\\UnitRecipe\\";
    
    private Pen? penData;
    private string _penKeyPrefix; // "PenA1", "PenA2", "PenB1", or "PenB2"
    
    public new string? TabTitle
    {
        get => _tabTitle;
        set => SetProperty(ref _tabTitle, value);
    }

    public int TabId
    {
        get => _tabId;
        set => SetProperty(ref _tabId, value);
    }
    
    public PenClean PenClean
    {
        get => _penClean;
        set => SetProperty(ref _penClean, value);
    }

    /// <summary>
    /// PenClean 配置，用于获取不同 Pen 类型的 PLC 句柄
    /// </summary>
    public IPenCleanConfig PenCleanConfig => _penCleanConfig;

    // Recipe lists
    private ObservableCollection<string> _penRecipes = new();
    public ObservableCollection<string> PenRecipes
    {
        get => _penRecipes;
        set => SetProperty(ref _penRecipes, value);
    }

    private string? _selectedPenRecipe;
    public string? SelectedPenRecipe
    {
        get => _selectedPenRecipe;
        set => SetProperty(ref _selectedPenRecipe, value);
    }

    // Commands
    public DelegateCommand<string> InitializeCommand { get; private set; } = null!;
    public DelegateCommand<string> AutoModeCommand { get; private set; } = null!;
    public DelegateCommand<string> ManualModeCommand { get; private set; } = null!;
    public DelegateCommand<string> AutoRunCommand { get; set; } = null!;
    public DelegateCommand<string> ExecuteRecipeCommand { get; private set; } = null!;
    public DelegateCommand<string> StartCommand { get; private set; } = null!;
    public DelegateCommand<string> StopCommand { get; private set; } = null!;
    public DelegateCommand<string> AutoCommand { get; private set; } = null!;
    public DelegateCommand<string> AbortCommand { get; private set; } = null!;
    public DelegateCommand<string> CloseCommand { get; private set; } = null!;
    public DelegateCommand<string> MotorCommand { get; private set; } = null!;
    public DelegateCommand<string> ServoCommand { get; private set; } = null!;
    public DelegateCommand<string> CylinderCommand { get; private set; } = null!;
    public DelegateCommand<string> PressureCommand { get; private set; } = null!;
    public DelegateCommand<string> LiquidCommand { get; private set; } = null!;
    public DelegateCommand<string> GasCommand { get; private set; } = null!;

    // Servo button text properties
    public string ChuckRotationMotorServoText => PenClean.ChuckRotationMotor.IsServoOn ? Resources.Off : Resources.On;
    public string PenArmSweepMotorServoText => PenClean.PenArmSweepMotor.IsServoOn ? Resources.Off : Resources.On;
    public string PenRotationMotorServoText => PenClean.PenRotationMotor.IsServoOn ? Resources.Off : Resources.On;

    /// <summary>
    /// 构造函数，通过注入 IPenCleanConfig 来支持不同的 Pen 类型
    /// </summary>
    /// <param name="id">Tab ID</param>
    /// <param name="tabTitle">Tab 标题</param>
    /// <param name="penCleanConfig">PenClean 配置</param>
    /// <param name="eventAggregator">事件聚合器</param>
    public PenCleanTabContentViewModel(int id, string tabTitle, IPenCleanConfig penCleanConfig, IEventAggregator eventAggregator) : base(id)
    {
        _penCleanConfig = penCleanConfig ?? throw new ArgumentNullException(nameof(penCleanConfig));
        _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        TabId = id;
        TabTitle = tabTitle;
        switch(id)
        {
            case 0:
                kvLink = GlobalService.kvHostLinkInit.kvLinkPenA;
                _varNameRecipeStruct = "structPenRecipe1";
                _addressPlcRecipeReq = 0x520C;
                _addressPlcRecipeReset = 0x1A;
                _baseChamberAddress = "ZF74500";
                break;
            case 1:
                kvLink = GlobalService.kvHostLinkInit.kvLinkPenA;
                _varNameRecipeStruct = "structPenRecipe2";
                _addressPlcRecipeReq = 0x550C;
                _addressPlcRecipeReset = 0x1B;
                _baseChamberAddress = "ZF94500";
                break;
            case 2:
                kvLink = GlobalService.kvHostLinkInit.kvLinkPenB;
                _varNameRecipeStruct = "structPenRecipe1";
                _addressPlcRecipeReq = 0x640C;
                _addressPlcRecipeReset = 0x1C;
                _baseChamberAddress = "ZF74500";
                break;
            default:
                kvLink = GlobalService.kvHostLinkInit.kvLinkPenB;
                _varNameRecipeStruct = "structPenRecipe2";
                _addressPlcRecipeReq = 0x670C;
                _addressPlcRecipeReset = 0x1D;
                _baseChamberAddress = "ZF94500";
                break;
        }

        if (id == 0 || id == 2)
        {
            _err = KHL.KHLWriteVariable(kvLink.sock, "PLCStartPermition", _writeTrue, true);
            _err = KHL.KHLWriteVariable(kvLink.sock, "PLCReady", _writeTrue, true);
            //_err = KHL.KHLWriteVariable(kvLink.sock, "PLCAutoStart", _writeTrue, true);
        }

        InitializeCommands();
        //InitializePlcConnection();
        LoadRecipesData();

        _ = MonitorLoopAsync();

        // penData = GlobalService.DataCenter.listPen.FirstOrDefault().ListSV;

        if (GlobalService.DataCenter.listPen.Count < 2)
        {
            throw new InvalidOperationException("DataCenter.listPen does not contain enough data.");
        }

        penData = id switch
        {
            0 or 1  => GlobalService.DataCenter.listPen[0],
            2 or 3  => GlobalService.DataCenter.listPen[1],
            _ => null
        };

        // 根据id确定键名前缀
        _penKeyPrefix = id switch
        {
            0 => "_1",
            1 => "_2",
            2 => "_1",
            3 => "_2",
            _ => "_1"
        };

        // 订阅Recipe保存事件
        _eventAggregator.GetEvent<RecipeSavedEvent>().Subscribe(OnRecipeSaved, ThreadOption.UIThread);
    }

    private void InitializeCommands()
    {
        InitializeCommand = OperationAudit.Create<string>("Initialize", InitializeCommandExecute, () => TabTitle ?? string.Empty);
        AutoModeCommand = OperationAudit.Create<string>("AutoMode", AutoModeCommandExecute, () => TabTitle ?? string.Empty);
        ManualModeCommand = OperationAudit.Create<string>("ManualMode", ManualModeCommandExecute, () => TabTitle ?? string.Empty);
        AutoRunCommand = OperationAudit.Create<string>("AutoRun", AutoRunCommandExecute, () => TabTitle ?? string.Empty);
        ExecuteRecipeCommand = OperationAudit.Create<string>("ExecuteRecipe", ExecuteRecipeCommandExecute, () => TabTitle ?? string.Empty);
        StartCommand = OperationAudit.Create<string>("Start", StartCommandExecute, () => TabTitle ?? string.Empty);
        StopCommand = OperationAudit.Create<string>("Stop", StopCommandExecute, () => TabTitle ?? string.Empty);
        AutoCommand = OperationAudit.Create<string>("Auto", AutoCommandExecute, () => TabTitle ?? string.Empty);
        AbortCommand = OperationAudit.Create<string>("Abort", AbortCommandExecute, () => TabTitle ?? string.Empty);
        CloseCommand = OperationAudit.Create<string>("Close", CloseCommandExecute, () => TabTitle ?? string.Empty);
        MotorCommand = OperationAudit.Create<string>("Motor", MotorCommandExecute, () => TabTitle ?? string.Empty);
        ServoCommand = OperationAudit.Create<string>("Servo", ServoCommandExecute, () => TabTitle ?? string.Empty);
        CylinderCommand = OperationAudit.Create<string>("Cylinder", CylinderCommandExecute, () => TabTitle ?? string.Empty);
        PressureCommand = OperationAudit.Create<string>("Pressure", PressureCommandExecute, () => TabTitle ?? string.Empty);
        LiquidCommand = OperationAudit.Create<string>("Liquid", LiquidCommandExecute, () => TabTitle ?? string.Empty);
        GasCommand = OperationAudit.Create<string>("Gas", GasCommandExecute, () => TabTitle ?? string.Empty);
    }

    private async Task MonitorLoopAsync()
    {
        await Task.Delay(2000).ConfigureAwait(false);
        const int delay = 500;
        var disp = Application.Current?.Dispatcher;
        while (true)
        {
            try
            {
                await Task.Delay(delay).ConfigureAwait(false);
                if (_isPlcAutoStartNeedFalse)
                {
                    _delayCycle++;
                    if (_delayCycle >= 4)
                    {
                        _err = KHL.KHLWriteVariable(kvLink.sock, "PLCAutoStart", _writeFalse, true);
                        _delayCycle = 0;
                        _isPlcAutoStartNeedFalse = false;
                    }
                }

                IntereactionWithPLCForAssignRecipe();

                if (disp != null)
                    await disp.InvokeAsync(new System.Action(RefreshHmi));
                else
                    RefreshHmi();
            }
            catch (Exception ex)
            {
                Logger.Error($"Pen monitor loop error in {TabTitle}", ex);
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
            var sv = penData.GetSv(key);
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

    /// <summary>
    /// 刷新HMI界面显示的实时数据
    /// </summary>
    private void RefreshHmi()
    {
        KSTPenCollectData.stPenCollectData data;
        // 刷新电机速度和位置的实际值
        PenClean.ChuckRotationMotor.ActVelocity = GetSvValue<float>(penData.penName + _penKeyPrefix + nameof(data._1ChuckRotationSpeedAct).Replace("_1", ""));

        PenClean.PenArmSweepMotor.ActVelocity = GetSvValue<float>(penData.penName + _penKeyPrefix + nameof(data._1PenArmSweepSpeedAct).Replace("_1", ""));
        PenClean.PenArmSweepMotor.ActPosition = GetSvValue<float>(penData.penName + _penKeyPrefix + nameof(data._1PenArmSweepAngleAct).Replace("_1", ""));

        PenClean.PenRotationMotor.ActVelocity = GetSvValue<float>(penData.penName + _penKeyPrefix + nameof(data._1PenHeadRotationSpeedAct).Replace("_1", ""));

        PenClean.ChuckRotationMotor.IsServoOn = GetSvValue<bool>(penData.penName + _penKeyPrefix + nameof(data._1ChuckMotorServoOn).Replace("_1", ""));
        PenClean.PenArmSweepMotor.IsServoOn = GetSvValue<bool>(penData.penName + _penKeyPrefix + nameof(data._1PenArmSweepMotorServoOn).Replace("_1", ""));
        PenClean.PenRotationMotor.IsServoOn = GetSvValue<bool>(penData.penName + _penKeyPrefix + nameof(data._1PenRotationMotorServoOn).Replace("_1", ""));
        
        RaisePropertyChanged(nameof(ChuckRotationMotorServoText));
        RaisePropertyChanged(nameof(PenArmSweepMotorServoText));
        RaisePropertyChanged(nameof(PenRotationMotorServoText));
        
    }
    
    private void SetWaferIndex(int waferIndex)
    {
        switch (TabId)
        {
            case 0:
                GlobalService.globalChambers.PenA1.WaferIndex = waferIndex;
                break;
            case 1:
                GlobalService.globalChambers.PenA2.WaferIndex = waferIndex;
                break;
            case 2:
                GlobalService.globalChambers.PenB1.WaferIndex = waferIndex;
                break;
            case 3:
                GlobalService.globalChambers.PenB2.WaferIndex = waferIndex;
                break;
        }
    }

    private void IntereactionWithPLCForAssignRecipe()
    {        
        _err = KHL.KHLReadDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, Convert.ToUInt32(_addressPlcRecipeReq), 1, _readBuf);
        if (_err != (int)KHLUserError.KHL_ERR_NONE)
        {
            Logger.For("RecipeAssign").Warn($"Read recipe req failed: {TabTitle} address={_addressPlcRecipeReq:X}, err={_err}");
            return;
        }

        int recipeReq = Convert.ToInt32(_readBuf[0]);

        if (recipeReq != 0)
        {
            Growl.SuccessGlobal($"Index: {recipeReq}");
            int waferIndex = recipeReq;
            try
            {
                SetWaferIndex(waferIndex);
                string penRecipeName = GetPenRecipe(GlobalService.dicWaferIndex[waferIndex], this.Id);
                AssignWaferToPLC(penRecipeName);

                //向地址中写入wafer的sequence数值
                if (!GlobalService.dicWaferIndex.TryGetValue(waferIndex, out var wafer))
                {
                    Logger.For("RecipeAssign").Warn($"Wafer index not found in dicWaferIndex: {waferIndex}, tab={TabTitle}");
                    return;
                }
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

                byte[] writeBufTemp = { _readBuf[0], 0x02 };
                _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, Convert.ToUInt32(_addressPlcRecipeReset), 1, writeBufTemp);
                Growl.SuccessGlobal($"SET: {_addressPlcRecipeReset}");
            }
            catch
            {

                byte[] writeBufTemp = { _readBuf[0], 0x03 };
                _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, Convert.ToUInt32(_addressPlcRecipeReset), 1, writeBufTemp);
                Growl.ErrorGlobal($"Get Sequence Failed, Index: {waferIndex}");

            }
           
            _isResetRecipeState = true;

        }
        else
        {
            if (_isResetRecipeState)
            {
                byte[] writeBufTemp = { 0, 0 };
                _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, Convert.ToUInt32(_addressPlcRecipeReset), 1, writeBufTemp);
                Growl.SuccessGlobal($"RESET: {_addressPlcRecipeReset}");
                _isResetRecipeState = false;
            }
        }
    }

    private void AssignWaferToPLC(string recipeName)
    {
        SelectedPenRecipe = recipeName;
        AssignPenRecipe();
    }

    private void AssignPenRecipe()
    {
        if (string.IsNullOrEmpty(SelectedPenRecipe))
        {
            Growl.WarningGlobal("Please select a Pen recipe first");
            return;
        }
        //byte[] data = new byte[20480];
        try
        {
            var recipeContent = LoadRecipeContent("Pen Recipe", SelectedPenRecipe);
            if (recipeContent != null)
            {
                KST.stPenRecipeFormat plcPenRecipe = new KST.stPenRecipeFormat();
                for (int i = 0; i < recipeContent.Data?[0].Steps.Count; i++)
                {
                    plcPenRecipe.ProcessType[i] = GetRealVal<short>(recipeContent, "ProcessType", i);
                    plcPenRecipe.ProcessTime[i] = GetRealVal<float>(recipeContent, "ProcessTime", i);
                    plcPenRecipe.ChuckRotateVelocity[i] = GetRealVal<float>(recipeContent, "ChuckRotateVelocity", i);
                    plcPenRecipe.PenRotateVelocity[i] = GetRealVal<float>(recipeContent, "PenRotateVelocity", i);
                    plcPenRecipe.PenArmSweepStartPosition[i] = GetRealVal<float>(recipeContent, "PenArmSweepStartPosition", i);
                    plcPenRecipe.PenArmSweepEndPosition[i] = GetRealVal<float>(recipeContent, "PenArmSweepEndPosition", i);
                    plcPenRecipe.PenArmSweepCenterVelocity[i] = GetRealVal<float>(recipeContent, "PenArmSweepCenterVelocity", i);
                    plcPenRecipe.PenArmSweepEdgeVelocity[i] = GetRealVal<float>(recipeContent, "PenArmSweepEdgeVelocity", i);
                    plcPenRecipe.PenArmCycle[i] = GetRealVal<short>(recipeContent, "PenArmCycle", i);
                    plcPenRecipe.PenPressure[i] = GetRealVal<float>(recipeContent, "PenPressure", i);
                    plcPenRecipe.WFUpperN2Flow[i] = GetRealVal<float>(recipeContent, "WFUpperFaceN2Flow", i);
                    plcPenRecipe.WFUpperDIWFlow[i] = GetRealVal<float>(recipeContent, "WFUpperFaceDIWFlow", i);
                    plcPenRecipe.WFLowerDIWFlow[i] = GetRealVal<float>(recipeContent, "WFLowerFaceDIWFlow", i);
                    plcPenRecipe.WFLowerN2Supply[i] = GetRealVal<short>(recipeContent, "WFLowerFaceN2Supply", i);
                    plcPenRecipe.WFUpperLowerChemicalSupply[i] = GetRealVal<short>(recipeContent, "WFUpperLowerFaceChemicalSupply", i);
                    plcPenRecipe.WFUpperChemicalSupply[i] = GetRealVal<short>(recipeContent, "WFUpperFaceChemicalSupply", i);
                    plcPenRecipe.WFUpperrDICO2Supply[i] = GetRealVal<short>(recipeContent, "WFUpperFaceDICO2Supply", i);
                    plcPenRecipe.WFChemicalMixedTotalFlow[i] = GetRealVal<float>(recipeContent, "WFChemicalMixedTotalFlow", i);
                    plcPenRecipe.WFChemicalRatio[i] = GetRealVal<float>(recipeContent, "WFChemicalRatio", i);
                }
                KST.stPenRecipeFormat_ToByte(ref _writeBuf, plcPenRecipe, 1, 0);
                _err = KHL.KHLWriteVariable(kvLink.sock, _varNameRecipeStruct, _writeBuf, true);      
                if (_err == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"{_penCleanConfig.PenType} Recipe Executed: {SelectedPenRecipe}");
                }
                else
                {
                    Growl.ErrorGlobal($"Failed to Write recipe: {SelectedPenRecipe}:{_err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Failed to load recipe: {SelectedPenRecipe}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error executing recipe: {ex.Message}");
            Logger.Error("Error executing pen recipe", ex);
        }
    }

    private string GetPenRecipe(Wafer wafer, int id)
    {
        var path = Path.Combine("D:\\CMP\\Configuration", "Recipes", "Sequence", $"{wafer.SequenceRecipe}.json");
        var text = File.ReadAllText(path);
        var data = JsonConvert.DeserializeObject<SequenceRecipeData>(text);
        string recipeName = string.Empty;
        if (id == 0)
        {
            recipeName = GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipePenA1").Value.ToString());
            wafer.CurrentStation = CurrentStation.CleanAP1;
        }
        else if (id == 1)
        {
            recipeName = GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipePenA2").Value.ToString());
            wafer.CurrentStation = CurrentStation.CleanAP2;
        }
        else if (id == 2)
        {
            recipeName = GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipePenB1").Value.ToString());
            wafer.CurrentStation = CurrentStation.CleanBP1;
        }
        else if (id == 3)
        {
            recipeName = GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipePenB2").Value.ToString());
            wafer.CurrentStation = CurrentStation.CleanBP2;
        }
        return recipeName;
    }

    private string GetFileNameWithoutEx(string path)
    {
        string[] temp = path.Split('\\');
        string fileName = temp[temp.Length - 1];
        return fileName.Substring(0, fileName.Length - 5);
    }

    //private void InitializeCommandExecute(string obj)
    //{
    //    var (handle, value) = _penCleanConfig.GetInitializeCommand();
        
    //    err = KHL.KHLWriteDevicesAsWords(sock, KHLDevType.DEV_W, handle, 1, value);
    //    if (err != 0)
    //    {
    //        Growl.ErrorGlobal($"Initialize Error: {err}");
    //    }
    //    else
    //    {
    //        Growl.SuccessGlobal($"{_penCleanConfig.PenType} Initialize Command Executed");
    //    }
    //}

    private void InitializeCommandExecute(string obj)
    {
        var (handle, value) = _penCleanConfig.GetInitializeCommand();

        _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
        if (_err == (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.InfoGlobal("Initialize command executed");
        }
        else
        {
            Growl.ErrorGlobal($"Failed to Initialize: {_err}");
        }
    }

    private void AutoModeCommandExecute(string obj)
    {
        var (handle, value) = _penCleanConfig.GetAutoModeCommand();

        _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
        if (_err != 0)
        {
            Growl.ErrorGlobal($"Auto Mode Error: {_err}");
        }
        else
        {
            Growl.SuccessGlobal($"{_penCleanConfig.PenType} Auto Mode Command Executed");
        }
    }

    private void ManualModeCommandExecute(string obj)
    {
        var (handle, value) = _penCleanConfig.GetManualModeCommand();

        _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
        if (_err != 0)
        {
            Growl.ErrorGlobal($"Manual Mode Error: {_err}");
        }
        else
        {
            Growl.SuccessGlobal($"{_penCleanConfig.PenType} Manual Mode Command Executed");
        }
    }

    private void AutoRunCommandExecute(string obj)
    {
        _err = KHL.KHLWriteVariable(kvLink.sock, "PLCAutoStart", _writeTrue, true);
        if (_err == (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.InfoGlobal("Auto start command executed");
            _isPlcAutoStartNeedFalse = true;
        }
        else
        {
            Growl.ErrorGlobal($"Failed to auto start: {_err}");
        }
    }

    private void ExecuteRecipeCommandExecute(string obj)
    {
        if (string.IsNullOrEmpty(SelectedPenRecipe))
        {
            Growl.WarningGlobal("Please select a Pen recipe first");
            return;
        }
        //byte[] data = new byte[20480];
        try
        {
            var recipeContent = LoadRecipeContent("Pen Recipe", SelectedPenRecipe);
            if (recipeContent != null)
            {
                KST.stPenRecipeFormat plcPenRecipe = new KST.stPenRecipeFormat();
                for (int i = 0; i < recipeContent.Data?[0].Steps.Count; i++)
                {
                    plcPenRecipe.ProcessType[i] = GetRealVal<short>(recipeContent, "ProcessType", i);
                    plcPenRecipe.ProcessTime[i] = GetRealVal<float>(recipeContent, "ProcessTime", i);
                    plcPenRecipe.ChuckRotateVelocity[i] = GetRealVal<float>(recipeContent, "ChuckRotateVelocity", i);
                    plcPenRecipe.PenRotateVelocity[i] = GetRealVal<float>(recipeContent, "PenRotateVelocity", i);
                    plcPenRecipe.PenArmSweepStartPosition[i] = GetRealVal<float>(recipeContent, "PenArmSweepStartPosition", i);
                    plcPenRecipe.PenArmSweepEndPosition[i] = GetRealVal<float>(recipeContent, "PenArmSweepEndPosition", i);
                    plcPenRecipe.PenArmSweepCenterVelocity[i] = GetRealVal<float>(recipeContent, "PenArmSweepCenterVelocity", i);
                    plcPenRecipe.PenArmSweepEdgeVelocity[i] = GetRealVal<float>(recipeContent, "PenArmSweepEdgeVelocity", i);
                    plcPenRecipe.PenArmCycle[i] = GetRealVal<short>(recipeContent, "PenArmCycle", i);
                    plcPenRecipe.PenPressure[i] = GetRealVal<float>(recipeContent, "PenPressure", i);
                    plcPenRecipe.WFUpperN2Flow[i] = GetRealVal<float>(recipeContent, "WFUpperFaceN2Flow", i);
                    plcPenRecipe.WFUpperDIWFlow[i] = GetRealVal<float>(recipeContent, "WFUpperFaceDIWFlow", i);
                    plcPenRecipe.WFLowerDIWFlow[i] = GetRealVal<float>(recipeContent, "WFLowerFaceDIWFlow", i);
                    plcPenRecipe.WFLowerN2Supply[i] = GetRealVal<short>(recipeContent, "WFLowerFaceN2Supply", i);
                    plcPenRecipe.WFUpperLowerChemicalSupply[i] = GetRealVal<short>(recipeContent, "WFUpperLowerFaceChemicalSupply", i);
                    plcPenRecipe.WFUpperChemicalSupply[i] = GetRealVal<short>(recipeContent, "WFUpperFaceChemicalSupply", i);
                    plcPenRecipe.WFUpperrDICO2Supply[i] = GetRealVal<short>(recipeContent, "WFUpperFaceDICO2Supply", i);
                    plcPenRecipe.WFChemicalMixedTotalFlow[i] = GetRealVal<float>(recipeContent, "WFChemicalMixedTotalFlow", i);
                    plcPenRecipe.WFChemicalRatio[i] = GetRealVal<float>(recipeContent, "WFChemicalRatio", i);
                }
                KST.stPenRecipeFormat_ToByte(ref _writeBuf, plcPenRecipe, 1, 0);
                _err = KHL.KHLWriteVariable(kvLink.sock, _varNameRecipeStruct, _writeBuf, true);
                if (_err == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"{_penCleanConfig.PenType} Recipe Executed: {SelectedPenRecipe}");
                }
                else
                {
                    Growl.ErrorGlobal($"Failed to Write recipe: {SelectedPenRecipe}:{_err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Failed to load recipe: {SelectedPenRecipe}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error executing recipe: {ex.Message}");
            Logger.Error("Error executing pen recipe", ex);
        }
    }

    private void StartCommandExecute(string obj)
    {
        ExecuteRecipeCommandExecute(obj);
    }

    private void StopCommandExecute(string obj)
    {
        try
        {
            var (handle, value) = _penCleanConfig.GetTestRecipeCommand("Stop");
            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
            if (_err != 0)
            {
                Growl.ErrorGlobal($"Stop Error: {_err}");
            }
            else
            {
                Growl.SuccessGlobal($"{_penCleanConfig.PenType} Stopped");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Stop Command Error: {ex.Message}");
        }
    }

    private void AutoCommandExecute(string obj)
    {
        Growl.InfoGlobal($"{_penCleanConfig.PenType} Auto Command: {obj}");
    }

    private void AbortCommandExecute(string obj)
    {
        Growl.InfoGlobal($"{_penCleanConfig.PenType} Abort Command: {obj}");
    }

    private void CloseCommandExecute(string obj)
    {
        try
        {
            var (handle, value) = _penCleanConfig.GetGasCommand(obj);

            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
            if (_err != 0)
            {
                Growl.ErrorGlobal($"Close Command {obj} Error: {_err}");
            }
            else
            {
                Growl.SuccessGlobal($"{_penCleanConfig.PenType} Close Command: {obj}");
            }
        }
        catch (ArgumentException ex)
        {
            Growl.ErrorGlobal($"Close Command Error: {ex.Message}");
        }
    }

    private void MotorCommandExecute(string obj)
    {
        try
        {
            uint plcVelHandle;
            byte[] plcVelValue;
            uint plcPosHandle;
            byte[] plcPosValue;
            
            // 在执行命令前先设置参数
            switch (obj)
            {
                case "ChuckRotation_Run":
                    plcVelHandle = _penCleanConfig.Params.Motor.ChuckRotation.Handle;
                    plcVelValue = PenClean.ChuckRotationMotor.SetVelocity.Value();
                    _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                    if (_err != 0)
                    {
                        Growl.ErrorGlobal($"设置 Chuck Rotation 速度失败: {_err}");
                        return;
                    }
                    break;
                    
                case "PenArmSweep_AnyPosition":
                    plcVelHandle = _penCleanConfig.Params.Motor.PenArmSweep.VelocityHandle;
                    plcVelValue = PenClean.PenArmSweepMotor.SetVelocity.Value();
                    _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                    if (_err != 0)
                    {
                        Growl.ErrorGlobal($"设置 Pen Arm Sweep 速度失败: {_err}");
                        return;
                    }
                    plcPosHandle = _penCleanConfig.Params.Motor.PenArmSweep.PositionHandle;
                    plcPosValue = PenClean.PenArmSweepMotor.SetPosition.Value();
                    _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcPosHandle, 2, plcPosValue);
                    if (_err != 0)
                    {
                        Growl.ErrorGlobal($"设置 Pen Arm Sweep 位置失败: {_err}");
                        return;
                    }
                    break;
                    
                case "PenRotation_Run":
                    plcVelHandle = _penCleanConfig.Params.Motor.PenRotation.Handle;
                    plcVelValue = PenClean.PenRotationMotor.SetVelocity.Value();
                    _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                    if (_err != 0)
                    {
                        Growl.ErrorGlobal($"设置 Pen Rotation 速度失败: {_err}");
                        return;
                    }
                    break;
            }
            
            var (handle, value) = _penCleanConfig.GetMotorCommand(obj);

            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
            if (_err != 0)
            {
                Growl.ErrorGlobal($"Motor Command {obj} Error: {_err}");
            }
            else
            {
                Growl.SuccessGlobal($"{_penCleanConfig.PenType} Motor Command: {obj}");
            }
        }
        catch (ArgumentException ex)
        {
            Growl.ErrorGlobal($"Motor Command Error: {ex.Message}");
        }
    }

    private void ServoCommandExecute(string obj)
    {
        try
        {
            bool currentServoState = obj switch
            {
                "ChuckRotationMotor" => PenClean.ChuckRotationMotor.IsServoOn,
                "PenArmSweepMotor" => PenClean.PenArmSweepMotor.IsServoOn,
                "PenRotationMotor" => PenClean.PenRotationMotor.IsServoOn,
                _ => throw new ArgumentException($"Unknown motor: {obj}")
            };

            bool newServoState = !currentServoState;
            var (handle, value, offset) = _penCleanConfig.GetServoCommand(obj);

            // Update the motor state
            switch (obj)
            {
                case "ChuckRotationMotor":
                    PenClean.ChuckRotationMotor.IsServoOn = newServoState;
                    RaisePropertyChanged(nameof(ChuckRotationMotorServoText));
                    break;
                case "PenArmSweepMotor":
                    PenClean.PenArmSweepMotor.IsServoOn = newServoState;
                    RaisePropertyChanged(nameof(PenArmSweepMotorServoText));
                    break;
                case "PenRotationMotor":
                    PenClean.PenRotationMotor.IsServoOn = newServoState;
                    RaisePropertyChanged(nameof(PenRotationMotorServoText));
                    break;
            }

            _err = KHL.KHLWriteDevicesAsBits(kvLink.sock, KHLDevType.DEV_W, value, offset, 1, newServoState.Value());
            if (_err != 0)
            {
                Growl.ErrorGlobal($"Servo Command {obj} Error: {_err}");
            }
            else
            {
                string stateText = newServoState ? "ON" : "OFF";
                Growl.SuccessGlobal($"{_penCleanConfig.PenType} Servo Command: {obj} - Servo {stateText}");
            }
        }
        catch (ArgumentException ex)
        {
            Growl.ErrorGlobal($"Servo Command Error: {ex.Message}");
        }
    }

    private void CylinderCommandExecute(string obj)
    {
        try
        {
            var (handle, value) = _penCleanConfig.GetCylinderCommand(obj);

            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
            if (_err != 0)
            {
                Growl.ErrorGlobal($"Cylinder Command {obj} Error: {_err}");
            }
            else
            {
                Growl.SuccessGlobal($"{_penCleanConfig.PenType} Cylinder Command: {obj}");
            }
        }
        catch (ArgumentException ex)
        {
            Growl.ErrorGlobal($"Cylinder Command Error: {ex.Message}");
        }
    }

    private void PressureCommandExecute(string obj)
    {
        try
        {
            uint plcParamHandle;
            byte[] plcParamValue;
            
            // 在执行压力/流量命令前先设置参数
            switch (obj)
            {
                case "PenPressure_Pressure":
                    plcParamHandle = _penCleanConfig.Params.Cylinder.PenPressure.Handle;
                    plcParamValue = PenClean.PenPressure.SetPress.Value();
                    _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcParamHandle, 2, plcParamValue);
                    if (_err != 0)
                    {
                        Growl.ErrorGlobal($"设置 Pen Pressure 参数失败: {_err}");
                        return;
                    }
                    break;
                    
                case "N2Flow_Flowrate":
                    plcParamHandle = _penCleanConfig.Params.Gas.N2Flow.Handle;
                    plcParamValue = PenClean.N2Flow.SetFlowrate.Value();
                    _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcParamHandle, 2, plcParamValue);
                    if (_err != 0)
                    {
                        Growl.ErrorGlobal($"设置 N2 Flow 参数失败: {_err}");
                        return;
                    }
                    break;
                    
                case "CO2Pressure_Pressure":
                    plcParamHandle = _penCleanConfig.Params.Gas.CO2Pressure.Handle;
                    plcParamValue = PenClean.CO2Pressure.SetPress.Value();
                    _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcParamHandle, 2, plcParamValue);
                    if (_err != 0)
                    {
                        Growl.ErrorGlobal($"设置 CO2 Pressure 参数失败: {_err}");
                        return;
                    }
                    break;
                    
                case "DIWPressure_Pressure":
                    plcParamHandle = _penCleanConfig.Params.Liquid.DIWPressure.Handle;
                    plcParamValue = PenClean.DIWPressure.SetPress.Value();
                    _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcParamHandle, 2, plcParamValue);
                    if (_err != 0)
                    {
                        Growl.ErrorGlobal($"设置 DIW Pressure 参数失败: {_err}");
                        return;
                    }
                    break;
            }
            
            var (handle, value) = _penCleanConfig.GetPressureCommand(obj);

            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
            if (_err != 0)
            {
                Growl.ErrorGlobal($"Pressure Command {obj} Error: {_err}");
            }
            else
            {
                Growl.SuccessGlobal($"{_penCleanConfig.PenType} Pressure Command: {obj}");
            }
        }
        catch (ArgumentException ex)
        {
            Growl.ErrorGlobal($"Pressure Command Error: {ex.Message}");
        }
    }

    private void LiquidCommandExecute(string obj)
    {
        try
        {
            uint plcParamHandle;
            byte[] plcParamValue;
            
            // 在执行液体命令前先设置流量参数
            switch (obj)
            {
                case "ChemicalInletValve_Open":
                case "ChemicalOutletValve_Open":
                    plcParamHandle = _penCleanConfig.Params.Liquid.ChemicalFlow.Handle;
                    plcParamValue = PenClean.ChemicalFlow.SetFlowrate.Value();
                    _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcParamHandle, 2, plcParamValue);
                    if (_err != 0)
                    {
                        Growl.ErrorGlobal($"设置 Chemical Flow 参数失败: {_err}");
                        return;
                    }
                    break;
                    
                case "DIWInletValve_Open":
                case "DIWOutletValve_Open":
                    plcParamHandle = _penCleanConfig.Params.Liquid.DIWFlow.Handle;
                    plcParamValue = PenClean.DIWFlow.SetFlowrate.Value();
                    _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcParamHandle, 2, plcParamValue);
                    if (_err != 0)
                    {
                        Growl.ErrorGlobal($"设置 DIW Flow 参数失败: {_err}");
                        return;
                    }
                    break;
                    
                case "WFUpperFaceDIWWashValve_Open":
                    plcParamHandle = _penCleanConfig.Params.Liquid.WFUpperFaceDIWFlow.Handle;
                    plcParamValue = PenClean.WFUpperDIWFlow.SetFlowrate.Value();
                    _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcParamHandle, 2, plcParamValue);
                    if (_err != 0)
                    {
                        Growl.ErrorGlobal($"设置 WF Upper DIW Flow 参数失败: {_err}");
                        return;
                    }
                    break;
                    
                case "WFLowerFaceDIWWashValve_Open":
                    plcParamHandle = _penCleanConfig.Params.Liquid.WFLowerFaceDIWFlow.Handle;
                    plcParamValue = PenClean.WFLowerDIWFlow.SetFlowrate.Value();
                    _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcParamHandle, 2, plcParamValue);
                    if (_err != 0)
                    {
                        Growl.ErrorGlobal($"设置 WF Lower DIW Flow 参数失败: {_err}");
                        return;
                    }
                    break;
            }
            
            var (handle, value) = _penCleanConfig.GetLiquidCommand(obj);

            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
            if (_err != 0)
            {
                Growl.ErrorGlobal($"Liquid Command {obj} Error: {_err}");
            }
            else
            {
                Growl.SuccessGlobal($"{_penCleanConfig.PenType} Liquid Command: {obj}");
            }
        }
        catch (ArgumentException ex)
        {
            Growl.ErrorGlobal($"Liquid Command Error: {ex.Message}");
        }
    }

    private void GasCommandExecute(string obj)
    {
        try
        {
            var (handle, value) = _penCleanConfig.GetGasCommand(obj);

            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
            if (_err != 0)
            {
                Growl.ErrorGlobal($"Gas Command {obj} Error: {_err}");
            }
            else
            {
                Growl.SuccessGlobal($"{_penCleanConfig.PenType} Gas Command: {obj}");
            }
        }
        catch (ArgumentException ex)
        {
            Growl.ErrorGlobal($"Gas Command Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 加载 Recipe 数据
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

            LoadRecipesFromFolder("Pen Recipe", PenRecipes);

            // 设置默认选择
            if (PenRecipes.Count > 0)
                SelectedPenRecipe = PenRecipes[0];
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Failed to load recipes: {ex.Message}");
            Logger.Error("Failed to load pen recipes", ex);
        }
    }

    /// <summary>
    /// 从特定文件夹加载 Recipe
    /// </summary>
    private void LoadRecipesFromFolder(string folderName, ObservableCollection<string> recipeList)
    {
        try
        {
            string folderPath = Path.Combine(_recipeBasePath, folderName);
            
            if (!Directory.Exists(folderPath))
            {
                return;
            }

            recipeList.Clear();

            DirectoryInfo folderInfo = new DirectoryInfo(folderPath);
            var jsonFiles = folderInfo.GetFiles("*.json");

            foreach (var file in jsonFiles)
            {
                try
                {
                    var json = FileUtil.ReadFile(file.FullName);
                    RecipeContent? content = JsonConvert.DeserializeObject<RecipeContent>(json);

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
                    Logger.Error($"Error loading recipe from {file.FullName}", ex);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error loading recipes from folder {folderName}", ex);
        }
    }

    /// <summary>
    /// 加载具体的 Recipe 内容
    /// </summary>
    private RecipeContent? LoadRecipeContent(string folderName, string recipeName)
    {
        try
        {
            string folderPath = Path.Combine(_recipeBasePath, folderName);
            
            if (!Directory.Exists(folderPath))
            {
                Growl.WarningGlobal($"Recipe folder not found: {folderName}");
                return null;
            }

            string recipeFilePath = Path.Combine(folderPath, $"{recipeName}.json");
            
            if (!File.Exists(recipeFilePath))
            {
                Growl.WarningGlobal($"Recipe file not found: {recipeName}.json");
                return null;
            }

            var json = FileUtil.ReadFile(recipeFilePath);
            RecipeContent? content = JsonConvert.DeserializeObject<RecipeContent>(json);

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
            Logger.Error($"Error reading recipe file: {recipeName}", ex);
            return null;
        }
    }

    /// <summary>
    /// Recipe 保存事件处理
    /// </summary>
    private void OnRecipeSaved(RecipeSavedEventArgs args)
    {
        try
        {
            if (args == null || string.IsNullOrEmpty(args.RecipeType))
                return;

            if (args.RecipeType == "Pen Recipe")
            {
                LoadRecipesFromFolder("Pen Recipe", PenRecipes);
                if (!string.IsNullOrEmpty(args.RecipeName) && PenRecipes.Contains(args.RecipeName))
                {
                    SelectedPenRecipe = args.RecipeName;
                }
                Growl.InfoGlobal($"Pen Recipe 已更新: {args.RecipeName}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to reload recipes after save: {ex.Message}", ex);
        }
    }

    //private static T? GetRealVal<T>(Recipe.Content content, string name, int stepIndex)
    //{
    //    Recipe.Data data = new Recipe.Data();
    //    foreach (var item in content.Data!)
    //    {
    //        if (item.Name?.Replace(" ", "").Replace("_","").Replace("-","").Replace("&","") == name)
    //        {
    //            data = item;
    //        }
    //    }
    //    object result = new();
    //    string value = data.Steps[stepIndex].Value;
    //    data.Type = data.Type?.ToLower();
    //    if (data.Type == "option" || data.Type == "list")
    //    {
    //        result = Convert.ToInt16(value.Split(":")[0]);
    //    }
    //    if (data.Type == "int")
    //    {
    //        result = Convert.ToInt16(value);
    //    }
    //    if (data.Type == "float")
    //    {
    //        result = Convert.ToSingle(value);
    //    }
    //    else
    //    {
    //        result = value;
    //    }
    //    T t = (T)result;
    //    if (t == null) return default;
    //    return t;
    //}

    private static T? GetRealVal<T>(Recipe.Content content, string name, int stepIndex)
    {
        Recipe.Data data = new Recipe.Data();
        bool exists = false;
        foreach (var item in content.Data!)
        {
            if (item.Name?.Replace(" ", "").Replace("_", "").Replace("-", "").Replace("&", "") == name)
            {
                data = item;
                exists = true;
                break;
            }
        }
        if (!exists) return default;

        object result = new();
        string value = data.Steps[stepIndex].Value;
        data.Type = data.Type?.ToLower();
        if (data.Type == "option" || data.Type == "list")
        {
            result = Convert.ToInt16(value.Split(":")[0]);
        }
        else if (data.Type == "int")
        {
            result = Convert.ToInt16(value);
        }
        else if (data.Type == "float" || data.Type == "text")

        {
            result = Convert.ToSingle(value);
        }
        else
        {
            result = value;
        }
        T t = (T)result;
        if (t == null) return default;
        return t;
    }
}
