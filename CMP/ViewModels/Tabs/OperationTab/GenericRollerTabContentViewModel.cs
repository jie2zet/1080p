using CMP.Services.Auth;
using CMP.Lang;
using CMP.Models.Operation.Roller;
using CMP.ViewModels.Common;
using HandyControl.Controls;
using CMP.Models.kvsPlc;
using System.Collections.ObjectModel;
using System.IO;
using CMP.Constants.Plc;
using CMP.Extensions;
using Newtonsoft.Json;
using CMP.Utils;
using CMP.Services.Log;
using KHL = CMP.Models.kvsPlc.KvHostLink;
using KSTRecipe = CMP.Models.kvsPlc.Roll.KvsStructRollerRecipe;
using RecipeContent = CMP.Models.Recipe.RecipeEdit.Recipe.Content;
using CMP.Models.Recipe.RecipeEdit;
using CMP.Models.Recipe.Sequence;
using CMP.Models.Resource;
using DryIoc.FastExpressionCompiler.LightExpression;
using CMP.Models.DataCenter;
using CMP.Models.kvsPlc.Roll;
using System.Windows;
using System.Threading.Tasks;

namespace CMP.ViewModels.Tabs.OperationTab;

/// <summary>
/// 通用 Roller Tab Content ViewModel，支持不同的 Roll 配置
/// </summary>
public class GenericRollerTabContentViewModel : TabContentViewModelBase
{
    private readonly IEventAggregator _eventAggregator;
    private string? _tabTitle;
    private int _tabId;
    private RollViewPoco _rollerViewPoco = new();
    private readonly IRollerConfig _rollerConfig;
    private int _err;
    //private int _sock;
    KVLink kvLink;
    private string? _baseChamberAddress;
    private int _addressPlcRecipeReq;
    private int _addressPlcRecipeReset;

    private byte[] _writeBuf = new byte[16384];
    private byte[] _readBuf = new byte[1024];
    private byte[] _writeTrue = [1];
    private byte[] _writeFalse = [0];

    private int _delayCycle;
    private bool _isPlcAutoStartNeedFalse;
    private bool _isResetRecipeState;

    private string _veriableName = "";
    private string _recipeBasePath = "D:\\CMP\\Configuration\\Recipes\\UnitRecipe\\";

    private Roller? rollerData;
    private int _rollerIndex;

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
    
    public RollViewPoco RollerViewPoco
    {
        get => _rollerViewPoco;
        set => SetProperty(ref _rollerViewPoco, value);
    }

    /// <summary>
    /// Roller 配置，用于获取不同 Roll 类型的 PLC 句柄
    /// </summary>
    public IRollerConfig RollerConfig => _rollerConfig;

    // Recipe lists
    private ObservableCollection<string> _rollRecipes = new();
    public ObservableCollection<string> RollRecipes
    {
        get => _rollRecipes;
        set => SetProperty(ref _rollRecipes, value);
    }

    private string? _selectedRollRecipe;
    public string? SelectedRollRecipe
    {
        get => _selectedRollRecipe;
        set => SetProperty(ref _selectedRollRecipe, value);
    }

    // Commands
    public DelegateCommand<string> InitializeCommand { get; private set; } = null!;
    public DelegateCommand<string> AutoModeCommand { get; private set; } = null!;
    public DelegateCommand<string> ManualModeCommand { get; private set; } = null!;
    public DelegateCommand<string> AutoRunCommand { get; private set; } = null!;
    public DelegateCommand<string> ExecuteRecipeCommand { get; private set; } = null!;
    public DelegateCommand<string> StartCommand { get; private set; } = null!;
    public DelegateCommand<string> StopCommand { get; private set; } = null!;
    public DelegateCommand<string> RunCommand { get; private set; } = null!;
    public DelegateCommand<string> AbortCommand { get; private set; } = null!;
    public DelegateCommand<string> ServoCommand { get; private set; } = null!;
    public DelegateCommand<string> CylinderCommand { get; private set; } = null!;
    public DelegateCommand<string> PressureCommand { get; private set; } = null!;
    public DelegateCommand<string> LiquidCommand { get; private set; } = null!;
    public DelegateCommand<string> FlushCommand { get; private set; } = null!;
    public DelegateCommand FlushDelaySetCommand { get; private set; } = null!;

    // Servo button text properties
    public string WfLeftPillarRotateMotorServoText => RollerViewPoco.WfLeftPillarRotateMotor.IsServoOn ? Resources.Off : Resources.On;
    public string WfRightPillarRotateMotorServoText => RollerViewPoco.WfRightPillarRotateMotor.IsServoOn ? Resources.Off : Resources.On;
    public string UpperRollerRotateMotorServoText => RollerViewPoco.UpperRollerRotateMotor.IsServoOn ? Resources.Off : Resources.On;
    public string LowerRollerRotateMotorServoText => RollerViewPoco.LowerRollerRotateMotor.IsServoOn ? Resources.Off : Resources.On;

    /// <summary>
    /// 构造函数，通过注入 IRollerConfig 来支持不同的 Roll 类型
    /// </summary>
    /// <param name="id">Tab ID</param>
    /// <param name="tabTitle">Tab 标题</param>
    /// <param name="rollerConfig">Roller 配置</param>
    /// <param name="eventAggregator">事件聚合器</param>
    public GenericRollerTabContentViewModel(int id, string tabTitle, IRollerConfig rollerConfig, IEventAggregator eventAggregator) : base(id)
    {
        _rollerConfig = rollerConfig ?? throw new ArgumentNullException(nameof(rollerConfig));
        _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        TabId = id;
        TabTitle = tabTitle;
        switch (id)
        {
            case 0:
                {
                    _veriableName = "structRollerRecipe1";
                    kvLink = GlobalService.kvHostLinkInit.kvLinkRollerA;
                    _addressPlcRecipeReq = 0x490C;
                    _addressPlcRecipeReset = 0x14;
                    _baseChamberAddress = "ZF74500";
                    break;                
                }
            case 1:
                {
                    _veriableName = "structRollerRecipe2";
                    kvLink = GlobalService.kvHostLinkInit.kvLinkRollerA;
                    _addressPlcRecipeReq = 0x4C0C;
                    _addressPlcRecipeReset = 0x15;
                    _baseChamberAddress = "ZF94500";
                    break;
                }
            case 2:
                {
                    _veriableName = "structRollerRecipe3";
                    kvLink = GlobalService.kvHostLinkInit.kvLinkRollerA;
                    _addressPlcRecipeReq = 0x4F0C;
                    _addressPlcRecipeReset = 0x16;
                    _baseChamberAddress = "ZF114500";
                    break;
                }
            case 3:
                {
                    _veriableName = "structRollerRecipe1";
                    kvLink = GlobalService.kvHostLinkInit.kvLinkRollerB;
                    _addressPlcRecipeReq = 0x5B0C;
                    _addressPlcRecipeReset = 0x17;
                    _baseChamberAddress = "ZF74500";
                    break;
                }
            case 4:
                {
                    _veriableName = "structRollerRecipe2";
                    kvLink = GlobalService.kvHostLinkInit.kvLinkRollerB;
                    _addressPlcRecipeReq = 0x5E0C;
                    _addressPlcRecipeReset = 0x18;
                    _baseChamberAddress = "ZF94500";
                    break;
                }
            case 5:
                {
                    _veriableName = "structRollerRecipe3";
                    kvLink = GlobalService.kvHostLinkInit.kvLinkRollerB;
                    _addressPlcRecipeReq = 0x610C;
                    _addressPlcRecipeReset = 0x19;
                    _baseChamberAddress = "ZF114500";
                    break;
                }
            default:
                kvLink = GlobalService.kvHostLinkInit.kvLinkRollerA;
                break;
        }

        if (id == 0 || id == 3)
        {
            _err = KHL.KHLWriteVariable(kvLink.sock, "PLCStartPermition", _writeTrue, true);
            _err = KHL.KHLWriteVariable(kvLink.sock, "PLCReady", _writeTrue, true);
            //_err = KHL.KHLWriteVariable(kvLink.sock, "PLCAutoStart", _writeTrue, true);
        }

        InitializeCommands();
        LoadRecipesData();

        _ = MonitorLoopAsync();

        // check if listRoller has no data
        if (GlobalService.DataCenter.listRoller.Count < 2)
        {
            throw new InvalidOperationException("DataCenter.listRoller does not contain enough data.");
        }

        rollerData = id switch
        {
            0 or 1 or 2 => GlobalService.DataCenter.listRoller[0],
            3 or 4 or 5 => GlobalService.DataCenter.listRoller[1],
            _ => null
        };

        _rollerIndex = id switch
        {
            0 or 3 => 1,
            1 or 4 => 2,
            2 or 5 => 3,
            _ => 1
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
        RunCommand = OperationAudit.Create<string>("Run", RunCommandExecute, () => TabTitle ?? string.Empty);
        AbortCommand = OperationAudit.Create<string>("Abort", AbortCommandExecute, () => TabTitle ?? string.Empty);
        ServoCommand = OperationAudit.Create<string>("Servo", ServoCommandExecute, () => TabTitle ?? string.Empty);
        CylinderCommand = OperationAudit.Create<string>("Cylinder", CylinderCommandExecute, () => TabTitle ?? string.Empty);
        PressureCommand = OperationAudit.Create<string>("Pressure", PressureCommandExecute, () => TabTitle ?? string.Empty);
        LiquidCommand = OperationAudit.Create<string>("Liquid", LiquidCommandExecute, () => TabTitle ?? string.Empty);
        FlushCommand = OperationAudit.Create<string>("Flush", FlushCommandExecute, () => TabTitle ?? string.Empty);
        FlushDelaySetCommand = OperationAudit.Create("FlushDelaySet", FlushDelaySetCommandExecute, () => TabTitle ?? string.Empty);
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
                    await disp.InvokeAsync((Action)RefreshHmi);
                else
                    RefreshHmi();
            }            
            catch (Exception ex)
            {
                Logger.Error($"Roller monitor loop error in {TabTitle}", ex);
            }
            
        }
    }
    private void SetWaferIndex(int waferIndex)
    {
        switch (TabId)
        {
            case 0:
                GlobalService.globalChambers.RollerA1.WaferIndex = waferIndex;
                break;
            case 1:
                GlobalService.globalChambers.RollerA2.WaferIndex = waferIndex;
                break;
            case 2:
                GlobalService.globalChambers.RollerA3.WaferIndex = waferIndex;
                break;
            case 3:
                GlobalService.globalChambers.RollerB1.WaferIndex = waferIndex;
                break;
            case 4:
                GlobalService.globalChambers.RollerB2.WaferIndex = waferIndex;
                break;
            case 5:
                GlobalService.globalChambers.RollerB3.WaferIndex = waferIndex;
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
                string penRecipeName = GetRollerRecipe(GlobalService.dicWaferIndex[waferIndex], this.Id);
                AssignWaferToPLC(penRecipeName);

                //向地址中写入wafer的sequence数值
                var wafer = GlobalService.dicWaferIndex[waferIndex];
                for (int i = 0; i < wafer.SequnceValue.Length; i++)
                {
                    byte[] data = new byte[256];
                    uint devTopNo = (uint)(Convert.ToUInt32(_baseChamberAddress?.Replace("ZF", "")) + 10 + 5 * i);
                    KSTRecipe.IntToByte(ref data, wafer.SequnceValue[i]);
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
        SelectedRollRecipe = recipeName;
        AssignRollerRecipe();
    }

    private void AssignRollerRecipe()
    {
        if (string.IsNullOrEmpty(SelectedRollRecipe))
        {
            Growl.WarningGlobal("Please select a Roll recipe first");
            return;
        }

        try
        {
            var recipeContent = LoadRecipeContent(RecipeTemplateRegistry.RollRecipeFolder, SelectedRollRecipe);
            if (recipeContent != null)
            {
                KSTRecipe.stRollerRecipeFormat plcRecipe = new KSTRecipe.stRollerRecipeFormat();
                //byte[] data = new byte[20480];
                for (int i = 0; i < recipeContent.Data?[0].Steps.Count; i++)
                {
                    plcRecipe.ProcessType[i] = GetRealVal<short>(recipeContent, "ProcessType", i);
                    plcRecipe.ProcessTime[i] = GetRealVal<float>(recipeContent, "ProcessTime", i);
                    plcRecipe.UpperRollerRotateVelocity[i] = GetRealVal<float>(recipeContent, "UpperRollerRotateVelocity", i);
                    plcRecipe.LowerRollerRotateVelocity[i] = GetRealVal<float>(recipeContent, "LowerRollerRotateVelocity", i);
                    plcRecipe.PillarRotateVelocity[i] = GetRealVal<float>(recipeContent, "PillarRotateVelocity", i);
                    plcRecipe.UpperRollerPressure[i] = GetRealVal<float>(recipeContent, "UpperRollerPressure", i);
                    plcRecipe.LowerRollerPressure[i] = GetRealVal<float>(recipeContent, "LowerRollerPressure", i);
                    plcRecipe.WFUpperFlowrate[i] = GetRealVal<float>(recipeContent, "WFUpperFaceDIWFlowrate", i);
                    plcRecipe.WFLowerFlowrate[i] = GetRealVal<float>(recipeContent, "WFLowerFaceDIWFlowrate", i);
                    plcRecipe.WFUpperChemicalSupply[i] = GetRealVal<short>(recipeContent, "WFUpperFaceChemicalSupply", i);
                    plcRecipe.WFLowerChemicalSupply[i] = GetRealVal<short>(recipeContent, "WFLowerFaceChemicalSupply", i);
                    plcRecipe.MegaSonicSupply[i] = GetRealVal<short>(recipeContent, "MegaSonicSupply", i);
                    plcRecipe.ChemicalMixedTotalFlowrate[i] = GetRealVal<float>(recipeContent, "ChemicalMixedTotalFlowrate", i);
                    plcRecipe.ChemicalRatio[i] = GetRealVal<float>(recipeContent, "ChemicalRatio", i);
                    plcRecipe.UpperRollInnerRinseSupply[i] = GetRealVal<short>(recipeContent, "UpperRollInnerRinseSupply", i);
                    plcRecipe.DownRollInnerRinseSupply[i] = GetRealVal<short>(recipeContent, "DownRollInnerRinseSupply", i);
                }
                KSTRecipe.stRollerRecipeFormat_ToByte(ref _writeBuf, plcRecipe, 1, 0);
                _err = KHL.KHLWriteVariable(kvLink.sock, _veriableName, _writeBuf, true);
                if (_err == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"{_rollerConfig.RollType} Recipe Executed: {SelectedRollRecipe}");
                }
                else
                {
                    Growl.ErrorGlobal($"Failed to Write recipe: {SelectedRollRecipe}:{_err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Failed to load recipe: {SelectedRollRecipe}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error executing recipe: {ex.Message}");
            Logger.Error("Error executing roll recipe", ex);
        }
    }

    private string GetRollerRecipe(Wafer wafer, int id)
    {
        var path = Path.Combine("D:\\CMP\\Configuration", "Recipes", "Sequence", $"{wafer.SequenceRecipe}.json");
        var text = File.ReadAllText(path);
        var data = JsonConvert.DeserializeObject<SequenceRecipeData>(text);
        string recipeName = string.Empty;
        if (id == 0)
        {
            recipeName = GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipeRollerA1").Value.ToString());
            wafer.CurrentStation = CurrentStation.CleanAR1;
        }
        else if (id == 1)
        {
            recipeName = GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipeRollerA2").Value.ToString());
            wafer.CurrentStation = CurrentStation.CleanAR2;
        }
        else if (id == 2)
        {
            recipeName = GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipeRollerA3").Value.ToString());
            wafer.CurrentStation = CurrentStation.CleanAR3;
        }
        else if (id == 3)
        {
            wafer.CurrentStation = CurrentStation.CleanBR1;
            recipeName = GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipeRollerB1").Value.ToString());           
        }
        else if (id == 4)
        {
            wafer.CurrentStation = CurrentStation.CleanBR2;
            recipeName = GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipeRollerB2").Value.ToString());           
        }
        else if (id == 5)
        {
            wafer.CurrentStation = CurrentStation.CleanBR3;
            recipeName = GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipeRollerB3").Value.ToString());            
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
    //    var (handle, value) = _rollerConfig.GetInitializeCommand();
        
    //    err = KHL.KHLWriteDevicesAsWords(sock, KHLDevType.DEV_W, handle, 1, value);
    //    if (err != 0)
    //    {
    //        Growl.ErrorGlobal($"Initialize Error: {err}");
    //    }
    //    else
    //    {
    //        Growl.SuccessGlobal($"{_rollerConfig.RollType} Initialize Command Executed");
    //    }
    //}

    private void InitializeCommandExecute(string obj)
    {
        //if (TabId == 0 || TabId == 3)
        //{
        //    byte[] writeTrue = { 1 };
        //    _err = KHL.KHLWriteVariable(_sock, "PLCStartPermition", writeTrue, true);
        //    _err = KHL.KHLWriteVariable(_sock, "PLCReady", writeTrue, true);
        //    _err = KHL.KHLWriteVariable(_sock, "PLCAutoStart", writeTrue, true);
        //    _isPlcAutoStartNeedFalse = true;
        //    if (_err != 0)
        //    {
        //        Growl.ErrorGlobal($"Initialize Error: {_err}");
        //    }
        //    else
        //    {
        //        Growl.SuccessGlobal($"{_rollerConfig.RollType} Initialize Command Executed");
        //    }
        //}

        var (handle, value) = _rollerConfig.GetInitializeCommand();
        _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
        if (_err != 0)
        {
            Growl.ErrorGlobal($"Initialize Error: {_err}");
        }
        else
        {
            Growl.SuccessGlobal($"{_rollerConfig.RollType} Initialize Command Executed");
        }
    }

    private void AutoModeCommandExecute(string obj)
    {
        var (handle, value) = _rollerConfig.GetAutoModeCommand();

        _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
        if (_err != 0)
        {
            Growl.ErrorGlobal($"Auto Mode Error: {_err}");
        }
        else
        {
            Growl.SuccessGlobal($"{_rollerConfig.RollType} Auto Mode Command Executed");
        }
    }

    private void ManualModeCommandExecute(string obj)
    {
        var (handle, value) = _rollerConfig.GetManualModeCommand();

        _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
        if (_err != 0)
        {
            Growl.ErrorGlobal($"Manual Mode Error: {_err}");
        }
        else
        {
            Growl.SuccessGlobal($"{_rollerConfig.RollType} Manual Mode Command Executed");
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
        if (string.IsNullOrEmpty(SelectedRollRecipe))
        {
            Growl.WarningGlobal("Please select a Roll recipe first");
            return;
        }
        if (obj.Equals("Manual", StringComparison.OrdinalIgnoreCase))
        {
            uint addr = TabId switch
            {
                0 => 1012,
                1 => 11012,
                2 => 21012,
                3 => 1012,
                4 => 11012,
                5 => 21012,
                _ => 1012
            };
            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_D, addr, 1, 10.Value());
            if (_err != (int)KHLUserError.KHL_ERR_NONE)
            {
                Growl.ErrorGlobal($"Failed to send run command for recipe execution: {_err}");
                return;
            }
        }
        try
        {
            var recipeContent = LoadRecipeContent(RecipeTemplateRegistry.RollRecipeFolder, SelectedRollRecipe);
            if (recipeContent != null)
            {
                KSTRecipe.stRollerRecipeFormat plcRecipe = new KSTRecipe.stRollerRecipeFormat();
                for (int i = 0; i < recipeContent.Data?[0].Steps.Count; i++)
                {
                    plcRecipe.ProcessType[i] = GetRealVal<short>(recipeContent, "ProcessType", i);
                    plcRecipe.ProcessTime[i] = GetRealVal<float>(recipeContent, "ProcessTime", i);
                    plcRecipe.UpperRollerRotateVelocity[i] = GetRealVal<float>(recipeContent, "UpperRollerRotateVelocity", i);
                    plcRecipe.LowerRollerRotateVelocity[i] = GetRealVal<float>(recipeContent, "LowerRollerRotateVelocity", i);
                    plcRecipe.PillarRotateVelocity[i] = GetRealVal<float>(recipeContent, "PillarRotateVelocity", i);
                    plcRecipe.UpperRollerPressure[i] = GetRealVal<float>(recipeContent, "UpperRollerPressure", i);
                    plcRecipe.LowerRollerPressure[i] = GetRealVal<float>(recipeContent, "LowerRollerPressure", i);
                    plcRecipe.WFUpperFlowrate[i] = GetRealVal<float>(recipeContent, "WFUpperFaceDIWFlowrate", i);
                    plcRecipe.WFLowerFlowrate[i] = GetRealVal<float>(recipeContent, "WFLowerFaceDIWFlowrate", i);
                    plcRecipe.WFUpperChemicalSupply[i] = GetRealVal<short>(recipeContent, "WFUpperFaceChemicalSupply", i);
                    plcRecipe.WFLowerChemicalSupply[i] = GetRealVal<short>(recipeContent, "WFLowerFaceChemicalSupply", i);
                    plcRecipe.MegaSonicSupply[i] = GetRealVal<short>(recipeContent, "MegaSonicSupply", i);
                    plcRecipe.ChemicalMixedTotalFlowrate[i] = GetRealVal<float>(recipeContent, "ChemicalMixedTotalFlowrate", i);
                    plcRecipe.ChemicalRatio[i] = GetRealVal<float>(recipeContent, "ChemicalRatio", i);
                    plcRecipe.UpperRollInnerRinseSupply[i] = GetRealVal<short>(recipeContent, "UpperRollInnerRinseSupply", i);
                    plcRecipe.DownRollInnerRinseSupply[i] = GetRealVal<short>(recipeContent, "DownRollInnerRinseSupply", i);
                }
                KSTRecipe.stRollerRecipeFormat_ToByte(ref _writeBuf, plcRecipe,  1, 0);
                _err = KHL.KHLWriteVariable(kvLink.sock, _veriableName, _writeBuf, true);
                if (_err == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"{_rollerConfig.RollType} Recipe Executed: {SelectedRollRecipe}");
                }
                else
                {
                    Growl.ErrorGlobal($"Failed to Write recipe: {SelectedRollRecipe}:{_err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Failed to load recipe: {SelectedRollRecipe}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error executing recipe: {ex.Message}");
            Logger.Error("Error executing roll recipe", ex);
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
            var (handle, value) = _rollerConfig.GetTestRecipeCommand("Stop");
            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
            if (_err != 0)
            {
                Growl.ErrorGlobal($"Stop Error: {_err}");
            }
            else
            {
                Growl.SuccessGlobal($"{_rollerConfig.RollType} Stopped");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Stop Command Error: {ex.Message}");
        }
    }

    private void RunCommandExecute(string obj)
    {
        try
        {
            var (handle, value) = _rollerConfig.GetMotorRunCommand(obj);
            
            uint velocityHandle;
            byte[] velocityValue;
            
            switch (obj)
            {
                case "WfLeftPillarRotateMotor":
                    velocityHandle = _rollerConfig.Params.Motor.WfLeftPillarRotate.Handle;
                    velocityValue = RollerViewPoco.WfLeftPillarRotateMotor.SetVelocity.Value();
                    break;
                    
                case "WfRightPillarRotateMotor":
                    velocityHandle = _rollerConfig.Params.Motor.WfRightPillarRotate.Handle;
                    velocityValue = RollerViewPoco.WfRightPillarRotateMotor.SetVelocity.Value();
                    break;
                    
                case "UpperRollerRotateMotor":
                    velocityHandle = _rollerConfig.Params.Motor.UpperRollerRotate.Handle;
                    velocityValue = RollerViewPoco.UpperRollerRotateMotor.SetVelocity.Value();
                    break;
                    
                case "LowerRollerRotateMotor":
                    velocityHandle = _rollerConfig.Params.Motor.LowerRollerRotate.Handle;
                    velocityValue = RollerViewPoco.LowerRollerRotateMotor.SetVelocity.Value();
                    break;
                    
                default:
                    throw new ArgumentException($"Unknown motor: {obj}");
            }
            
            // 写入参数
            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, velocityHandle, 2, velocityValue);
            if (_err != 0)
            {
                Growl.ErrorGlobal($"Write Velocity Error: {_err}");
                return;
            }
            
            // 发送命令
            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
            if (_err != 0)
            {
                Growl.ErrorGlobal($"RunCommand {obj} Error: {_err}");
            }
            else
            {
                Growl.SuccessGlobal($"{_rollerConfig.RollType} RunCommand: {obj}");
            }
        }
        catch (ArgumentException ex)
        {
            Growl.ErrorGlobal($"Run Command Error: {ex.Message}");
        }
    }

    private void AbortCommandExecute(string obj)
    {
        try
        {
            var (handle, value) = _rollerConfig.GetMotorStopCommand(obj);

            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
            if (_err != 0)
            {
                Growl.ErrorGlobal($"AbortCommand {obj} Error: {_err}");
            }
            else
            {
                Growl.SuccessGlobal($"{_rollerConfig.RollType} AbortCommand: {obj}");
            }
        }
        catch (ArgumentException ex)
        {
            Growl.ErrorGlobal($"Abort Command Error: {ex.Message}");
        }
    }

    private void ServoCommandExecute(string obj)
    {
        try
        {
            bool currentServoState = obj switch
            {
                "WfLeftPillarRotateMotor" => RollerViewPoco.WfLeftPillarRotateMotor.IsServoOn,
                "WfRightPillarRotateMotor" => RollerViewPoco.WfRightPillarRotateMotor.IsServoOn,
                "UpperRollerRotateMotor" => RollerViewPoco.UpperRollerRotateMotor.IsServoOn,
                "LowerRollerRotateMotor" => RollerViewPoco.LowerRollerRotateMotor.IsServoOn,
                _ => throw new ArgumentException($"Unknown motor: {obj}")
            };

            bool newServoState = !currentServoState;
            var (handle, value, offset) = _rollerConfig.GetServoCommand(obj, newServoState);

            // Update the motor state
            switch (obj)
            {
                case "WfLeftPillarRotateMotor":
                    RollerViewPoco.WfLeftPillarRotateMotor.IsServoOn = newServoState;
                    RaisePropertyChanged(nameof(WfLeftPillarRotateMotorServoText));
                    break;
                case "WfRightPillarRotateMotor":
                    RollerViewPoco.WfRightPillarRotateMotor.IsServoOn = newServoState;
                    RaisePropertyChanged(nameof(WfRightPillarRotateMotorServoText));
                    break;
                case "UpperRollerRotateMotor":
                    RollerViewPoco.UpperRollerRotateMotor.IsServoOn = newServoState;
                    RaisePropertyChanged(nameof(UpperRollerRotateMotorServoText));
                    break;
                case "LowerRollerRotateMotor":
                    RollerViewPoco.LowerRollerRotateMotor.IsServoOn = newServoState;
                    RaisePropertyChanged(nameof(LowerRollerRotateMotorServoText));
                    break;
            }

            _err = KHL.KHLWriteDevicesAsBits(kvLink.sock, KHLDevType.DEV_W, value, offset, 1, newServoState.Value());
            if (_err != 0)
            {
                Growl.ErrorGlobal($"ServoCommand {obj} Error: {_err}");
            }
            else
            {
                string stateText = newServoState ? "ON" : "OFF";
                Growl.SuccessGlobal($"{_rollerConfig.RollType} ServoCommand: {obj} - Servo {stateText}");
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
            var (handle, value) = _rollerConfig.GetCylinderCommand(obj);

            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
            if (_err != 0)
            {
                Growl.ErrorGlobal($"CylinderCommand {obj} Error: {_err}");
            }
            else
            {
                Growl.SuccessGlobal($"{_rollerConfig.RollType} CylinderCommand: {obj}");
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
            var (handle, value) = _rollerConfig.GetPressureCommand(obj);

            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
            if (_err != 0)
            {
                Growl.ErrorGlobal($"PressureCommand {obj} Error: {_err}");
            }
            else
            {
                Growl.SuccessGlobal($"{_rollerConfig.RollType} PressureCommand: {obj}");
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
            var (handle, value) = _rollerConfig.GetLiquidCommand(obj);

            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
            if (_err != 0)
            {
                Growl.ErrorGlobal($"LiquidCommand {obj} Error: {_err}");
            }
            else
            {
                Growl.SuccessGlobal($"{_rollerConfig.RollType} LiquidCommand: {obj}");
            }
        }
        catch (ArgumentException ex)
        {
            Growl.ErrorGlobal($"Liquid Command Error: {ex.Message}");
        }
    }

    private void FlushCommandExecute(string obj)
    {
        try
        {
            var (handle, value) = _rollerConfig.GetFlushCommand(obj);

            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
            if (_err != 0)
            {
                Growl.ErrorGlobal($"Flush Command {obj} Error: {_err}");
            }
            else
            {
                Growl.SuccessGlobal($"{_rollerConfig.RollType} Flush Command: {obj}");
            }
        }
        catch (ArgumentException ex)
        {
            Growl.ErrorGlobal($"Flush Command Error: {ex.Message}");
        }
    }

    private void FlushDelaySetCommandExecute()
    {
        var value = RollerViewPoco.Flush.OpenDelaySet;
        var parameter = _rollerConfig.Params.Flush.OpenDelay;
        if (float.IsNaN(value) || float.IsInfinity(value) || value < parameter.MinValue || value > parameter.MaxValue)
        {
            Growl.ErrorGlobal($"DIW(ALL) Open Delay 范围必须为 {parameter.MinValue:0.00}–{parameter.MaxValue:0.00}");
            return;
        }

        _err = KHL.KHLWriteDevicesAsWords(
            kvLink.sock,
            KHLDevType.DEV_ZF,
            parameter.Handle,
            2,
            value.Value());
        if (_err != 0)
        {
            Growl.ErrorGlobal($"设置 DIW(ALL) Open Delay 失败: {_err}");
        }
        else
        {
            Growl.SuccessGlobal($"{_rollerConfig.RollType} DIW(ALL) Open Delay 已设置为 {value:0.00}");
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

            LoadRecipesFromFolder(RecipeTemplateRegistry.RollRecipeFolder, RollRecipes);

            // 设置默认选择
            if (RollRecipes.Count > 0)
                SelectedRollRecipe = RollRecipes[0];
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Failed to load recipes: {ex.Message}");
            Logger.Error("Failed to load roll recipes", ex);
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

            // 按文件夹名把模板上的规格元数据（IsUsed / OperableRule）贴回行上，
            // 供 GetRealVal 判定"不使用"参数：IsUsed=false 的参数按 0（default）下发
            RecipeTemplateRegistry.Decorate(content, folderName);

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

            if (args.RecipeType == RecipeTemplateRegistry.RollRecipeFolder)
            {
                LoadRecipesFromFolder(RecipeTemplateRegistry.RollRecipeFolder, RollRecipes);
                if (!string.IsNullOrEmpty(args.RecipeName) && RollRecipes.Contains(args.RecipeName))
                {
                    SelectedRollRecipe = args.RecipeName;
                }
                Growl.InfoGlobal($"Roll Recipe 已更新: {args.RecipeName}");
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
    //    bool exists = false;
    //    foreach (var item in content.Data!)
    //    {
    //        if (item.Name?.Replace(" ", "").Replace("_", "").Replace("-", "").Replace("&", "") == name)
    //        {
    //            data = item;
    //            exists = true;
    //            break;
    //        }
    //    }
    //    if (!exists) return default;

    //    object result = new();
    //    string value = data.Steps[stepIndex].Value;
    //    data.Type = data.Type?.ToLower();
    //    if (data.Type == "option" || data.Type == "list")
    //    {
    //        result = Convert.ToInt16(value.Split(":")[0]);
    //    }
    //    else if (data.Type == "int")
    //    {
    //        result = Convert.ToInt16(value);
    //    }
    //    else if (data.Type == "float" || data.Type == "text")
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

        if (model == null || !model.IsUsed) return default;
        if (stepIndex < 0 || stepIndex >= model.Steps.Count) return default;

        var value = model.Steps[stepIndex].Value;
        var type = model.Type?.ToLowerInvariant();
        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        if (type == "option" || type == "list")
        {
            // 统一用 RecipeOptionValue 解析（兼容全角冒号）
            value = targetType == typeof(string)
                ? RecipeOptionValue.LabelPart(value)
                : RecipeOptionValue.NumericPart(value);
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

    /// <summary>
    /// 刷新HMI界面显示的实时数据
    /// </summary>
    private void RefreshHmi()
    {
        if (rollerData == null || !rollerData.TryGetProcessDataSnapshot(out var snapshot))
        {
            return;
        }

        (float WfLeftPillarSpeed,
            float WfRightPillarSpeed,
            float UpperRollerSpeed,
            float LowerRollerSpeed,
            float WfLeftPillarTorque,
            float WfRightPillarTorque,
            float UpperRollerTorque,
            float LowerRollerTorque,
            float DiwOpenTime,
            bool WfLeftPillarServoOn,
            bool WfRightPillarServoOn,
            bool UpperRollerServoOn,
            bool LowerRollerServoOn) stationData = _rollerIndex switch
        {
            1 => (
                snapshot._1WFLeftPillarRotationMotorSpeedAct,
                snapshot._1WFRightPillarRotationMotorSpeedAct,
                snapshot._1UpperRollerRotationSpeedAct,
                snapshot._1LowerRollerRotationSpeedAct,
                snapshot._1WFLeftPillarRotationMotorTorqueAct,
                snapshot._1WFRightPillarRotationMotorTorqueAct,
                snapshot._1UpperRollerRotationMotorTorqueAct,
                snapshot._1LowerRollerRotationMotorTorqueAct,
                snapshot._1DIWAllOpenTimeAct,
                snapshot._1WFLeftPillarMotorServoOn,
                snapshot._1WFRightPillarMotorServoOn,
                snapshot._1UpperRollerMotorServoOn,
                snapshot._1LowerRollerMotorServoOn),
            2 => (
                snapshot._2WFLeftPillarRotationMotorSpeedAct,
                snapshot._2WFRightPillarRotationMotorSpeedAct,
                snapshot._2UpperRollerRotationSpeedAct,
                snapshot._2LowerRollerRotationSpeedAct,
                snapshot._2WFLeftPillarRotationMotorTorqueAct,
                snapshot._2WFRightPillarRotationMotorTorqueAct,
                snapshot._2UpperRollerRotationMotorTorqueAct,
                snapshot._2LowerRollerRotationMotorTorqueAct,
                snapshot._2DIWAllOpenTimeAct,
                snapshot._2WFLeftPillarMotorServoOn,
                snapshot._2WFRightPillarMotorServoOn,
                snapshot._2UpperRollerMotorServoOn,
                snapshot._2LowerRollerMotorServoOn),
            _ => (
                snapshot._3WFLeftPillarRotationMotorSpeedAct,
                snapshot._3WFRightPillarRotationMotorSpeedAct,
                snapshot._3UpperRollerRotationSpeedAct,
                snapshot._3LowerRollerRotationSpeedAct,
                snapshot._3WFLeftPillarRotationMotorTorqueAct,
                snapshot._3WFRightPillarRotationMotorTorqueAct,
                snapshot._3UpperRollerRotationMotorTorqueAct,
                snapshot._3LowerRollerRotationMotorTorqueAct,
                snapshot._3DIWAllOpenTimeAct,
                snapshot._3WFLeftPillarMotorServoOn,
                snapshot._3WFRightPillarMotorServoOn,
                snapshot._3UpperRollerMotorServoOn,
                snapshot._3LowerRollerMotorServoOn)
        };

        RollerViewPoco.WfLeftPillarRotateMotor.ActVelocity = stationData.WfLeftPillarSpeed;
        RollerViewPoco.WfRightPillarRotateMotor.ActVelocity = stationData.WfRightPillarSpeed;
        RollerViewPoco.UpperRollerRotateMotor.ActVelocity = stationData.UpperRollerSpeed;
        RollerViewPoco.LowerRollerRotateMotor.ActVelocity = stationData.LowerRollerSpeed;
        RollerViewPoco.WfLeftPillarRotateMotor.ActTorque = stationData.WfLeftPillarTorque;
        RollerViewPoco.WfRightPillarRotateMotor.ActTorque = stationData.WfRightPillarTorque;
        RollerViewPoco.UpperRollerRotateMotor.ActTorque = stationData.UpperRollerTorque;
        RollerViewPoco.LowerRollerRotateMotor.ActTorque = stationData.LowerRollerTorque;
        RollerViewPoco.Flush.OpenDelayAct = stationData.DiwOpenTime;

        RollerViewPoco.WfLeftPillarRotateMotor.IsServoOn = stationData.WfLeftPillarServoOn;
        RollerViewPoco.WfRightPillarRotateMotor.IsServoOn = stationData.WfRightPillarServoOn;
        RollerViewPoco.UpperRollerRotateMotor.IsServoOn = stationData.UpperRollerServoOn;
        RollerViewPoco.LowerRollerRotateMotor.IsServoOn = stationData.LowerRollerServoOn;

        RaisePropertyChanged(nameof(WfLeftPillarRotateMotorServoText));
        RaisePropertyChanged(nameof(WfRightPillarRotateMotorServoText));
        RaisePropertyChanged(nameof(UpperRollerRotateMotorServoText));
        RaisePropertyChanged(nameof(LowerRollerRotateMotorServoText));
    }
}
