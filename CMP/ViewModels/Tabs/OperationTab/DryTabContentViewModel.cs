using CMP.Services.Auth;
using CMP.Constants.Plc;
using CMP.Extensions;
using CMP.Lang;
using CMP.Models.DataCenter;
using CMP.Models.kvsPlc;
using CMP.Models.Operation.Clean;
using CMP.Models.Operation.Roller;
using CMP.Models.Recipe.RecipeEdit;
using CMP.Models.Recipe.Sequence;
using CMP.Models.Resource;
using CMP.Services.Log;
using CMP.Utils;
using CMP.ViewModels.Common;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DryIoc.ImTools;
using HandyControl.Controls;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Threading.Tasks;
using KHL = CMP.Models.kvsPlc.KvHostLink;
//using KST = CMP.Models.kvsPlc.Dry.KvStructDry;
using KST = CMP.Models.kvsPlc.Clean.KvStruct;
using RecipeContent = CMP.Models.Recipe.RecipeEdit.Recipe.Content;

namespace CMP.ViewModels.Tabs.OperationTab;

/// <summary>
/// 通用 Dry Tab Content ViewModel，支持不同的 Dry 配置
/// </summary>
public class DryTabContentViewModel : TabContentViewModelBase
{
    private readonly IEventAggregator _eventAggregator;
    private string? _tabTitle;
    private int _tabId;
    private DryClean _dryClean = new();
    private readonly IDryCleanConfig _dryCleanConfig;
    private int _err = (int)KHLUserError.KHL_ERR_NONE;
    private KVLink kvLink;
    private int _addressPlcRecipeReq = 0;
    private int _addressPlcRecipeReset = 0;
    private string? _baseChamberAddress;
    private byte[] _writeBuf = new byte[16384];
    private byte[] _readBuf = new byte[1024];
    private byte[] _writeTrue = [1];
    private byte[] _writeFalse = [0];

    private int _delayCycle = 0;
    private bool _isPlcAutoStartNeedFalse = false;
    private bool _isResetRecipeState = false;

    private string _varNameRecipeStruct = string.Empty;
    private string _recipeBasePath = "D:\\CMP\\Configuration\\Recipes\\UnitRecipe\\";

    private Dry? dryData;

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

    public DryClean DryClean
    {
        get => _dryClean;
        set => SetProperty(ref _dryClean, value);
    }

    /// <summary>
    /// DryClean 配置，用于获取不同 Dry 类型的 PLC 句柄
    /// </summary>
    public IDryCleanConfig DryCleanConfig => _dryCleanConfig;

    // Recipe lists
    private ObservableCollection<string> _dryRecipes = new();
    public ObservableCollection<string> DryRecipes
    {
        get => _dryRecipes;
        set => SetProperty(ref _dryRecipes, value);
    }

    private string? _selectedDryRecipe;
    public string? SelectedDryRecipe
    {
        get => _selectedDryRecipe;
        set => SetProperty(ref _selectedDryRecipe, value);
    }

    // Commands
    public DelegateCommand<string> InitializeCommand { get; private set; } = null!;
    public DelegateCommand<string> AutoModeCommand { get; private set; } = null!;
    public DelegateCommand<string> ManualModeCommand { get; private set; } = null!;
    public DelegateCommand<string> AutoRunCommand { get; private set; } = null!;
    public DelegateCommand<string> ExecuteRecipeCommand { get; private set; } = null!;
    public DelegateCommand<string> StartCommand { get; private set; } = null!;
    public DelegateCommand<string> StopCommand { get; private set; } = null!;
    public DelegateCommand<string> MotorCommand { get; private set; } = null!;
    public DelegateCommand<string> ServoCommand { get; private set; } = null!;
    public DelegateCommand<string> CylinderCommand { get; private set; } = null!;
    public DelegateCommand<string> GasCommand { get; private set; } = null!;
    public DelegateCommand<string> LiquidCommand { get; private set; } = null!;

    // Servo button text properties
    public string ChuckRotationMotorServoText => DryClean.ChuckRotationMotor.IsServoOn ? Resources.Off : Resources.On;
    public string ArmSwingMotorServoText => DryClean.ArmSwingMotor.IsServoOn ? Resources.Off : Resources.On;

    /// <summary>
    /// 构造函数，通过注入 IDryCleanConfig 来支持不同的 Dry 类型
    /// </summary>
    /// <param name="id">Tab ID</param>
    /// <param name="tabTitle">Tab 标题</param>
    /// <param name="dryCleanConfig">DryClean 配置</param>
    /// <param name="eventAggregator">事件聚合器</param>
    public DryTabContentViewModel(int id, string tabTitle, IDryCleanConfig dryCleanConfig, IEventAggregator eventAggregator) : base(id)
    {
        _dryCleanConfig = dryCleanConfig ?? throw new ArgumentNullException(nameof(dryCleanConfig));
        _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        TabId = id;
        TabTitle = tabTitle;
        _baseChamberAddress = "ZF74500";
        switch (id)
        {
            case 0:
                kvLink = GlobalService.kvHostLinkInit.kvLinkDryA;
                _varNameRecipeStruct = "structDryRecipe";
                _addressPlcRecipeReq = 0x580C;
                _addressPlcRecipeReset = 0x1E;
                break;
            default:
                kvLink = GlobalService.kvHostLinkInit.kvLinkDryB;
                _varNameRecipeStruct = "structDryRecipe";
                _addressPlcRecipeReq = 0x6A0C;
                _addressPlcRecipeReset = 0x1F;
                break;
        }

        _err = KHL.KHLWriteVariable(kvLink.sock, "PLCStartPermition", _writeTrue, true);
        _err = KHL.KHLWriteVariable(kvLink.sock, "PLCReady", _writeTrue, true);
        _err = KHL.KHLWriteVariable(kvLink.sock, "PLCAutoStart", _writeTrue, true);

        InitializeCommands();
        LoadRecipesData();

        _ = MonitorLoopAsync();

        if (GlobalService.DataCenter.listDry.Count < 2)
        {
            throw new InvalidOperationException("DataCenter.listDry does not contain enough data.");
        }

        dryData = id switch
        {
            0 => GlobalService.DataCenter.listDry[0],
            1 => GlobalService.DataCenter.listDry[1],
            _ => null
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
        MotorCommand = OperationAudit.Create<string>("Motor", MotorCommandExecute, () => TabTitle ?? string.Empty);
        ServoCommand = OperationAudit.Create<string>("Servo", ServoCommandExecute, () => TabTitle ?? string.Empty);
        CylinderCommand = OperationAudit.Create<string>("Cylinder", CylinderCommandExecute, () => TabTitle ?? string.Empty);
        GasCommand = OperationAudit.Create<string>("Gas", GasCommandExecute, () => TabTitle ?? string.Empty);
        LiquidCommand = OperationAudit.Create<string>("Liquid", LiquidCommandExecute, () => TabTitle ?? string.Empty);
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

                IntereactionWithPlcForAssignRecipe();

                if (disp != null)
                    await disp.InvokeAsync((Action)RefreshHmi);
                else
                    RefreshHmi();
            }
            catch (Exception ex)
            {
                Logger.Error($"Dry monitor loop error in {TabTitle}", ex);
            }
        }
    }

    private void SetWaferIndex(int waferIndex)
    {
        switch (TabId)
        {
            case 0:
                GlobalService.globalChambers.DryA.WaferIndex = waferIndex;
                break;
            case 1:
                GlobalService.globalChambers.DryB.WaferIndex = waferIndex;
                break;
        }
    }

    private void IntereactionWithPlcForAssignRecipe()
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
                string dryRecipeName = GetDryRecipe(GlobalService.dicWaferIndex[waferIndex], this.Id);
                AssignWaferToPLC(dryRecipeName);

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
        SelectedDryRecipe = recipeName;
        AssignDryRecipe();
    }

    private void AssignDryRecipe()
    {
        if (string.IsNullOrEmpty(SelectedDryRecipe))
        {
            Growl.WarningGlobal("Please select a Dry recipe first");
            return;
        }

        try
        {
            var recipeContent = LoadRecipeContent(RecipeTemplateRegistry.DryRecipeFolder, SelectedDryRecipe);
            if (recipeContent != null)
            {
                KST.stDryRecipeFormat plcRecipe = new KST.stDryRecipeFormat();
                for (int i = 0; i < recipeContent.Data?[0].Steps.Count; i++)
                {
                    plcRecipe.ProcessType[i] = GetRealVal<short>(recipeContent, "ProcessType", i);
                    plcRecipe.ProcessTime[i] = GetRealVal<float>(recipeContent, "ProcessTime", i);
                    plcRecipe.WaferRotationSpeed[i] = GetRealVal<float>(recipeContent, "WaferRotationSpeed", i);
                    plcRecipe.IPAArmSweepStartPosition[i] = GetRealVal<short>(recipeContent, "IPAArmSweepStartPosition", i);
                    plcRecipe.IPAArmSweepEndPosition[i] = GetRealVal<short>(recipeContent, "IPAArmSweepEndPosition", i);
                    plcRecipe.IPAArmSweepCenterVelocity[i] = GetRealVal<float>(recipeContent, "IPAArmSweepCenterVelocity", i);
                    plcRecipe.IPAArmSweepEdgeVelocity[i] = GetRealVal<float>(recipeContent, "IPAArmSweepEdgeVelocity", i);
                    plcRecipe.IPAArmSweepSpeed[i] = GetRealVal<float>(recipeContent, "IPAArmSweepSpeed", i);
                    plcRecipe.IPAArmCycle[i] = GetRealVal<short>(recipeContent, "IPAArmCycle", i);
                    plcRecipe.DilutionN2GasFlow[i] = GetRealVal<float>(recipeContent, "DilutionN2GasFlow", i);
                    plcRecipe.N2ForWFBacksideFlow[i] = GetRealVal<float>(recipeContent, "N2ForWFBacksideFlow", i);
                    plcRecipe.OuterRinseFlow[i] = GetRealVal<float>(recipeContent, "OuterRinseFlow", i);
                    plcRecipe.BubblingN2GasFlow[i] = GetRealVal<float>(recipeContent, "BubblingN2GasFlow", i);
                    plcRecipe.UpperRinseSupply[i] = GetRealVal<short>(recipeContent, "UpperRinseSupply", i);
                    plcRecipe.LowerRinseSupply[i] = GetRealVal<short>(recipeContent, "LowerRinseSupply", i);
                    plcRecipe.WaferRotationAcceleration[i] = GetRealVal<float>(recipeContent, "WaferRotationAcceleration", i);

                }
                KST.stDryRecipeFormat_ToByte(ref _writeBuf, plcRecipe, 1, 0);
                _err = KHL.KHLWriteVariable(kvLink.sock, _varNameRecipeStruct, _writeBuf, true);
                if (_err == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"{_dryCleanConfig.DryType} Recipe Executed: {SelectedDryRecipe}");
                }
                else
                {
                    Growl.ErrorGlobal($"Failed to Write recipe: {SelectedDryRecipe}:{_err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Failed to load recipe: {SelectedDryRecipe}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error executing recipe: {ex.Message}");
            Logger.Error("Error executing dry recipe", ex);
        }
    }

    private string GetDryRecipe(Wafer wafer, int id)
    {
        var path = Path.Combine("D:\\CMP\\Configuration", "Recipes", "Sequence", $"{wafer.SequenceRecipe}.json");
        var text = File.ReadAllText(path);
        var data = JsonConvert.DeserializeObject<SequenceRecipeData>(text);
        string recipeName = string.Empty;
        if (id == 0)
        {
            recipeName = GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipeDryA")?.Value?.ToString() ?? "");
            wafer.CurrentStation = CurrentStation.CleanADry;
        }
        else if (id == 1)
        {
            recipeName = GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipeDryB")?.Value?.ToString() ?? "");
            wafer.CurrentStation = CurrentStation.CleanBDry;
        }
        return recipeName;
    }

    private string GetFileNameWithoutEx(string path)
    {
        if (string.IsNullOrEmpty(path)) return string.Empty;
        string[] temp = path.Split('\\');
        string fileName = temp[temp.Length - 1];
        return fileName.Substring(0, fileName.Length - 5);
    }

    //private void InitializeCommandExecute(string obj)
    //{
    //    var (handle, value) = _dryCleanConfig.GetInitializeCommand();

    //    _err = KHL.KHLWriteVariable(_sock, handle, value, true);
    //    if (_err != 0)
    //    {
    //        Growl.ErrorGlobal($"Initialize Error: {_err}");
    //    }
    //    else
    //    {
    //        Growl.SuccessGlobal($"{_dryCleanConfig.DryType} Initialize Command Executed");
    //    }
    //}

    private void InitializeCommandExecute(string obj)
    {
        // if (TabId == 0 || TabId == 3)
        // {
        //     byte[] writeTrue = { 1 };
        //     _err = KHL.KHLWriteVariable(_sock, "PLCStartPermition", writeTrue, true);
        //     _err = KHL.KHLWriteVariable(_sock, "PLCReady", writeTrue, true);
        //     _err = KHL.KHLWriteVariable(_sock, "PLCAutoStart", writeTrue, true);
        //     _isPlcAutoStartNeedFalse = true;
        //     if (_err != 0)
        //     {
        //         Growl.ErrorGlobal($"Initialize Error: {_err}");
        //     }
        //     else
        //     {
        //         Growl.SuccessGlobal($"{_dryCleanConfig.DryType} Initialize Command Executed");
        //     }
        // }
        var (handle, value) = _dryCleanConfig.GetInitializeCommand();
        _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
        if (_err != 0)
        {
            Growl.ErrorGlobal($"Initialize Error: {_err}");
        }
        else
        {
            Growl.SuccessGlobal($"{_dryCleanConfig.DryType} Initialize Command Executed");
        }
    }

    private void AutoModeCommandExecute(string obj)
    {
        var (handle, value) = _dryCleanConfig.GetAutoModeCommand();

        _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);

        if (_err != 0)
        {
            Growl.ErrorGlobal($"Auto Mode Error: {_err}");
        }
        else
        {
            Growl.SuccessGlobal($"{_dryCleanConfig.DryType} Auto Mode Command Executed");
        }
    }

    private void ManualModeCommandExecute(string obj)
    {
        var (handle, value) = _dryCleanConfig.GetManualModeCommand();

        _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
        if (_err != 0)
        {
            Growl.ErrorGlobal($"Manual Mode Error: {_err}");
        }
        else
        {
            Growl.SuccessGlobal($"{_dryCleanConfig.DryType} Manual Mode Command Executed");
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
        if (string.IsNullOrEmpty(SelectedDryRecipe))
        {
            Growl.WarningGlobal("Please select a Dry recipe first");
            return;
        }
        if (obj.Equals("Manual", StringComparison.OrdinalIgnoreCase))
        {
            uint addr = 1012;
            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_D, addr, 1, 10.Value());
            if (_err != (int)KHLUserError.KHL_ERR_NONE)
            {
                Growl.ErrorGlobal($"Failed to send run command for recipe execution: {_err}");
                return;
            }
        }
        try
        {
            var recipeContent = LoadRecipeContent(RecipeTemplateRegistry.DryRecipeFolder, SelectedDryRecipe);
            if (recipeContent != null)
            {
                KST.stDryRecipeFormat plcRecipe = new KST.stDryRecipeFormat();
                for (int i = 0; i < recipeContent.Data?[0].Steps.Count; i++)
                {
                    plcRecipe.ProcessType[i] = GetRealVal<short>(recipeContent, "ProcessType", i);
                    plcRecipe.ProcessTime[i] = GetRealVal<float>(recipeContent, "ProcessTime", i);
                    plcRecipe.WaferRotationSpeed[i] = GetRealVal<float>(recipeContent, "WaferRotationSpeed", i);
                    plcRecipe.IPAArmSweepStartPosition[i] = GetRealVal<short>(recipeContent, "IPAArmSweepStartPosition", i);
                    plcRecipe.IPAArmSweepEndPosition[i] = GetRealVal<short>(recipeContent, "IPAArmSweepEndPosition", i);
                    plcRecipe.IPAArmSweepCenterVelocity[i] = GetRealVal<float>(recipeContent, "IPAArmSweepCenterVelocity", i);
                    plcRecipe.IPAArmSweepEdgeVelocity[i] = GetRealVal<float>(recipeContent, "IPAArmSweepEdgeVelocity", i);
                    plcRecipe.IPAArmSweepSpeed[i] = GetRealVal<float>(recipeContent, "IPAArmSweepSpeed", i);
                    plcRecipe.IPAArmCycle[i] = GetRealVal<short>(recipeContent, "IPAArmCycle", i);
                    plcRecipe.DilutionN2GasFlow[i] = GetRealVal<float>(recipeContent, "DilutionN2GasFlow", i);
                    plcRecipe.N2ForWFBacksideFlow[i] = GetRealVal<float>(recipeContent, "N2ForWFBacksideFlow", i);
                    plcRecipe.OuterRinseFlow[i] = GetRealVal<float>(recipeContent, "OuterRinseFlow", i);
                    plcRecipe.BubblingN2GasFlow[i] = GetRealVal<float>(recipeContent, "BubblingN2GasFlow", i);
                    plcRecipe.UpperRinseSupply[i] = GetRealVal<short>(recipeContent, "UpperRinseSupply", i);
                    plcRecipe.LowerRinseSupply[i] = GetRealVal<short>(recipeContent, "LowerRinseSupply", i);
                    plcRecipe.WaferRotationAcceleration[i] = GetRealVal<float>(recipeContent, "WaferRotationAcceleration", i);
                }
                KST.stDryRecipeFormat_ToByte(ref _writeBuf, plcRecipe, 1, 0);
                _err = KHL.KHLWriteVariable(kvLink.sock, _varNameRecipeStruct, _writeBuf, true);
                if (_err == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"{_dryCleanConfig.DryType} Recipe Executed: {SelectedDryRecipe}");
                }
                else
                {
                    Growl.ErrorGlobal($"Failed to Write recipe: {SelectedDryRecipe}:{_err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Failed to load recipe: {SelectedDryRecipe}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error executing recipe: {ex.Message}");
            Logger.Error("Error executing dry recipe", ex);
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
            var (handle, value) = _dryCleanConfig.GetTestRecipeCommand("Stop");
            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
            if (_err != 0)
            {
                Growl.ErrorGlobal($"Stop Error: {_err}");
            }
            else
            {
                Growl.SuccessGlobal($"{_dryCleanConfig.DryType} Stopped");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Stop Command Error: {ex.Message}");
        }
    }

    private void MotorCommandExecute(string obj)
    {
        try
        {
            // 在执行命令前先设置参数
            uint velocityHandle;
            byte[] velocityValue;

            switch (obj)
            {
                case "ChuckRotation_Run":
                    velocityHandle = _dryCleanConfig.Params.Motor.ChuckRotation.Handle;
                    velocityValue = DryClean.ChuckRotationMotor.SetVelocity.Value();
                    _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, velocityHandle, 2, velocityValue);
                    if (_err != 0)
                    {
                        Growl.ErrorGlobal($"Write ChuckRotation Velocity Error: {_err}");
                        return;
                    }
                    break;

                case "ArmSwing_AnyPosition":
                    velocityHandle = _dryCleanConfig.Params.Motor.ArmSwing.VelocityHandle;
                    velocityValue = DryClean.ArmSwingMotor.SetVelocity.Value();
                    _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, velocityHandle, 2, velocityValue);
                    if (_err != 0)
                    {
                        Growl.ErrorGlobal($"Write ArmSwing Velocity Error: {_err}");
                        return;
                    }

                    var positionHandle = _dryCleanConfig.Params.Motor.ArmSwing.PositionHandle;
                    var positionValue = DryClean.ArmSwingMotor.SetPosition.Value();
                    _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, positionHandle, 2, positionValue);
                    if (_err != 0)
                    {
                        Growl.ErrorGlobal($"Write ArmSwing Position Error: {_err}");
                        return;
                    }
                    break;
            }

            var (handle, value) = _dryCleanConfig.GetMotorCommand(obj);

            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
            if (_err != 0)
            {
                Growl.ErrorGlobal($"Motor Command {obj} Error: {_err}");
            }
            else
            {
                Growl.SuccessGlobal($"{_dryCleanConfig.DryType} Motor Command: {obj}");
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
                "ChuckRotationMotor" => DryClean.ChuckRotationMotor.IsServoOn,
                "ArmSwingMotor" => DryClean.ArmSwingMotor.IsServoOn,
                _ => throw new ArgumentException($"Unknown motor: {obj}")
            };

            bool newServoState = !currentServoState;
            var (handle, value, _) = _dryCleanConfig.GetServoCommand(obj, newServoState);

            // Update the motor state
            switch (obj)
            {
                case "ChuckRotationMotor":
                    DryClean.ChuckRotationMotor.IsServoOn = newServoState;
                    RaisePropertyChanged(nameof(ChuckRotationMotorServoText));
                    break;
                case "ArmSwingMotor":
                    DryClean.ArmSwingMotor.IsServoOn = newServoState;
                    RaisePropertyChanged(nameof(ArmSwingMotorServoText));
                    break;
            }

            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value.Value());
            if (_err != 0)
            {
                Growl.ErrorGlobal($"Servo Command {obj} Error: {_err}");
            }
            else
            {
                string stateText = newServoState ? "ON" : "OFF";
                Growl.SuccessGlobal($"{_dryCleanConfig.DryType} Servo Command: {obj} - Servo {stateText}");
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
            var (handle, value) = _dryCleanConfig.GetCylinderCommand(obj);

            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
            if (_err != 0)
            {
                Growl.ErrorGlobal($"Cylinder Command {obj} Error: {_err}");
            }
            else
            {
                Growl.SuccessGlobal($"{_dryCleanConfig.DryType} Cylinder Command: {obj}");
            }
        }
        catch (ArgumentException ex)
        {
            Growl.ErrorGlobal($"Cylinder Command Error: {ex.Message}");
        }
    }

    private void GasCommandExecute(string obj)
    {
        try
        {
            // 在执行命令前先设置参数
            uint flowrateHandle;
            byte[] flowrateValue;

            switch (obj)
            {
                case "N2Flow_Flowrate":
                    flowrateHandle = _dryCleanConfig.Params.Gas.N2Flow.Handle;
                    flowrateValue = ((float)DryClean.Gas.N2Flowrate).Value();
                    _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, flowrateHandle, 2, flowrateValue);
                    if (_err != 0)
                    {
                        Growl.ErrorGlobal($"Write N2Flow Error: {_err}");
                        return;
                    }
                    break;

                case "N2FlowWafer_Flowrate":
                    flowrateHandle = _dryCleanConfig.Params.Gas.N2FlowWafer.Handle;
                    flowrateValue = ((float)DryClean.Gas.N2FlowrateWafer).Value();
                    _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, flowrateHandle, 2, flowrateValue);
                    if (_err != 0)
                    {
                        Growl.ErrorGlobal($"Write N2FlowWafer Error: {_err}");
                        return;
                    }
                    break;

                case "N2FlowLowerFace_Flowrate":
                    flowrateHandle = _dryCleanConfig.Params.Gas.N2FlowLowerFace.Handle;
                    flowrateValue = ((float)DryClean.Gas.N2FlowrateLowerFace).Value();
                    _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, flowrateHandle, 2, flowrateValue);
                    if (_err != 0)
                    {
                        Growl.ErrorGlobal($"Write N2FlowLowerFace Error: {_err}");
                        return;
                    }
                    break;
            }

            var (handle, value) = _dryCleanConfig.GetGasCommand(obj);

            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
            if (_err != 0)
            {
                Growl.ErrorGlobal($"Gas Command {obj} Error: {_err}");
            }
            else
            {
                Growl.SuccessGlobal($"{_dryCleanConfig.DryType} Gas Command: {obj}");
            }
        }
        catch (ArgumentException ex)
        {
            Growl.ErrorGlobal($"Gas Command Error: {ex.Message}");
        }
    }

    private void LiquidCommandExecute(string obj)
    {
        try
        {
            // 在执行命令前先设置参数
            uint flowrateHandle;
            byte[] flowrateValue;

            switch (obj)
            {
                case "WFUpperFaceDIWWashFlow2_Flowrate":
                    flowrateHandle = _dryCleanConfig.Params.Liquid.WFUpperFaceDIWWashFlow2.Handle;
                    flowrateValue = ((float)DryClean.Liquid.WFUpperFaceDIWWashFlow2).Value();
                    _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, flowrateHandle, 2, flowrateValue);
                    if (_err != 0)
                    {
                        Growl.ErrorGlobal($"Write WFUpperFaceDIWWashFlow2 Error: {_err}");
                        return;
                    }
                    break;
            }

            var (handle, value) = _dryCleanConfig.GetLiquidCommand(obj);

            _err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, handle, 1, value);
            if (_err != 0)
            {
                Growl.ErrorGlobal($"Liquid Command {obj} Error: {_err}");
            }
            else
            {
                Growl.SuccessGlobal($"{_dryCleanConfig.DryType} Liquid Command: {obj}");
            }
        }
        catch (ArgumentException ex)
        {
            Growl.ErrorGlobal($"Liquid Command Error: {ex.Message}");
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

            LoadRecipesFromFolder(RecipeTemplateRegistry.DryRecipeFolder, DryRecipes);

            // 设置默认选择
            if (DryRecipes.Count > 0)
                SelectedDryRecipe = DryRecipes[0];
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Failed to load recipes: {ex.Message}");
            Logger.Error("Failed to load dry recipes", ex);
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

            if (args.RecipeType == RecipeTemplateRegistry.DryRecipeFolder)
            {
                LoadRecipesFromFolder(RecipeTemplateRegistry.DryRecipeFolder, DryRecipes);
                if (!string.IsNullOrEmpty(args.RecipeName) && DryRecipes.Contains(args.RecipeName))
                {
                    SelectedDryRecipe = args.RecipeName;
                }
                Growl.InfoGlobal($"Dry Recipe 已更新: {args.RecipeName}");
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
    //        if (item.Name?.Replace(" ", "").Replace("_", "").Replace("-", "").Replace("&", "") == name)
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

    /// <summary>旋转电机当前位置：DryA 为 W5A10，DryB 为 W6C10。</summary>
    private const uint ChuckPositionAddressDryA = 0x5A10;
    private const uint ChuckPositionAddressDryB = 0x6C10;

    /// <summary>
    /// 刷新HMI界面显示的实时数据
    /// </summary>
    private void RefreshHmi()
    {
        RefreshChuckPosition();

        if (dryData == null || !dryData.TryGetProcessDataSnapshot(out var snapshot))
        {
            return;
        }

        DryClean.ChuckRotationMotor.ActVelocity = snapshot.ChuckRotationSpeedAct;
        DryClean.ArmSwingMotor.ActVelocity = snapshot.ArmSweepSpeedAct;
        DryClean.ArmSwingMotor.ActPosition = snapshot.ArmSweepAngleAct;
        DryClean.ChuckRotationMotor.ActTorque = snapshot.ChuckRotationTorqueAct;
        DryClean.ArmSwingMotor.ActTorque = snapshot.ArmSweepTorqueAct;
        
        DryClean.ChuckRotationMotor.IsServoOn = snapshot.ChuckMotorServoOn;
        DryClean.ArmSwingMotor.IsServoOn = snapshot.ArmSweepMotorServoOn;
        
        RaisePropertyChanged(nameof(ChuckRotationMotorServoText));
        RaisePropertyChanged(nameof(ArmSwingMotorServoText));
    }

    /// <summary>旋转电机当前位置按连续两个字解析成浮点数。</summary>
    private void RefreshChuckPosition()
    {
        if (kvLink.sock <= 0)
        {
            return;
        }

        var address = TabId == 0 ? ChuckPositionAddressDryA : ChuckPositionAddressDryB;
        var data = new byte[4];
        var readError = KHL.KHLReadDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, address, 2, data);
        if (readError == (int)KHLUserError.KHL_ERR_NONE)
        {
            DryClean.ChuckRotationMotor.ActPosition = BitConverter.ToSingle(data, 0);
        }
    }
}

