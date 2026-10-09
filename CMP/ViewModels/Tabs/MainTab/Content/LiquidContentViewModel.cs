using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using CMP.Constants.Plc;
using CMP.Lang;
using CMP.Models.kvsPlc;
using CMP.Models.Operation.Roller;
using HandyControl.Controls;
using Prism.Commands;
using Prism.Mvvm;
using WPFLocalizeExtension.Engine;
using KHL = CMP.Models.kvsPlc.KvHostLink;

namespace CMP.ViewModels.Tabs.MainTab.Content;

/// <summary>
/// Operation 页面中的集中式液路控制页。
/// </summary>
public sealed class LiquidContentViewModel : BindableBase
{
    public ObservableCollection<LiquidCategoryViewModel> Tabs { get; } = [];

    public LiquidContentViewModel()
    {
        Tabs.Add(new LiquidCategoryViewModel("LiquidPolish"));
        Tabs.Add(new LiquidCategoryViewModel("LiquidRoller")
        {
            Chambers =
            [
                new("RollerA1", new GenericRollerLiquidContentViewModel(RollerConfigFactory.CreateRollA1Config())),
                new("RollerA2", new GenericRollerLiquidContentViewModel(RollerConfigFactory.CreateRollA2Config())),
                new("RollerA3", new GenericRollerLiquidContentViewModel(RollerConfigFactory.CreateRollA3Config())),
                new("RollerB1", new GenericRollerLiquidContentViewModel(RollerConfigFactory.CreateRollB1Config())),
                new("RollerB2", new GenericRollerLiquidContentViewModel(RollerConfigFactory.CreateRollB2Config())),
                new("RollerB3", new GenericRollerLiquidContentViewModel(RollerConfigFactory.CreateRollB3Config()))
            ]
        });
        Tabs.Add(new LiquidCategoryViewModel("LiquidPen"));
        Tabs.Add(new LiquidCategoryViewModel("LiquidDry"));
    }
}

public sealed class LiquidCategoryViewModel : BindableBase
{
    private string _header;
    private string _emptyState;
    private ObservableCollection<LiquidChamberViewModel> _chambers = [];

    public LiquidCategoryViewModel(string headerKey)
    {
        HeaderKey = headerKey;
        _header = GetText(headerKey);
        _emptyState = GetText("LiquidEmptyState");
        _chambers.CollectionChanged += ChambersChanged;
        PropertyChangedEventManager.AddHandler(LocalizeDictionary.Instance, OnCultureChanged,
            nameof(LocalizeDictionary.Instance.Culture));
    }

    public string HeaderKey { get; }

    public string Header
    {
        get => _header;
        private set => SetProperty(ref _header, value);
    }

    public string EmptyState
    {
        get => _emptyState;
        private set => SetProperty(ref _emptyState, value);
    }

    public ObservableCollection<LiquidChamberViewModel> Chambers
    {
        get => _chambers;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (ReferenceEquals(_chambers, value)) return;
            _chambers.CollectionChanged -= ChambersChanged;
            SetProperty(ref _chambers, value!);
            _chambers.CollectionChanged += ChambersChanged;
            RaisePropertyChanged(nameof(IsEmpty));
            RaisePropertyChanged(nameof(HasChambers));
        }
    }

    public bool IsEmpty => Chambers.Count == 0;
    public bool HasChambers => !IsEmpty;

    private void OnCultureChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args)
    {
        Header = GetText(HeaderKey);
        EmptyState = GetText("LiquidEmptyState");
    }

    private void ChambersChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        RaisePropertyChanged(nameof(IsEmpty));
        RaisePropertyChanged(nameof(HasChambers));
    }

    private static string GetText(string key) =>
        Resources.ResourceManager.GetString(key, LocalizeDictionary.Instance.Culture) ?? key;
}

public sealed class LiquidChamberViewModel : BindableBase
{
    private string _header;

    public LiquidChamberViewModel(string headerKey, object content)
    {
        HeaderKey = headerKey;
        _header = GetText(headerKey);
        Content = content;
        PropertyChangedEventManager.AddHandler(LocalizeDictionary.Instance, OnCultureChanged,
            nameof(LocalizeDictionary.Instance.Culture));
    }

    public string HeaderKey { get; }
    public object Content { get; }

    public string Header
    {
        get => _header;
        private set => SetProperty(ref _header, value);
    }

    private void OnCultureChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args) =>
        Header = GetText(HeaderKey);

    private static string GetText(string key) =>
        Resources.ResourceManager.GetString(key, LocalizeDictionary.Instance.Culture) ?? key;
}

public sealed class GenericRollerLiquidContentViewModel : BindableBase
{
    private readonly GenericRollConfig _config;
    private readonly Func<(KVLink RollerA, KVLink RollerB)> _getPlcLinks;
    private readonly Func<int, KHLDevType, uint, uint, byte[], int> _writeDevices;
    private readonly Action<bool, string> _showResult;

    public ObservableCollection<LiquidParameterViewModel> Parameters { get; } = [];
    public ObservableCollection<LiquidCommandGroupViewModel> CommandGroups { get; } = [];

    public GenericRollerLiquidContentViewModel(GenericRollConfig config) : this(
        config,
        () => (GlobalService.kvHostLinkInit.kvLinkRollerA, GlobalService.kvHostLinkInit.kvLinkRollerB),
        KHL.KHLWriteDevicesAsWords,
        (success, message) =>
        {
            if (success) Growl.SuccessGlobal(message);
            else Growl.ErrorGlobal(message);
        })
    {
    }

    internal GenericRollerLiquidContentViewModel(
        GenericRollConfig config,
        Func<(KVLink RollerA, KVLink RollerB)> getPlcLinks,
        Func<int, KHLDevType, uint, uint, byte[], int> writeDevices,
        Action<bool, string> showResult)
    {
        _config = config;
        _getPlcLinks = getPlcLinks;
        _writeDevices = writeDevices;
        _showResult = showResult;
        // Shared offsets must still resolve metadata from the selected chamber's full path.
        AddParameter($"{_config.PlcHandleTypeName}.Liquid.ChemicalEnterFlow.Param.FlowrateSet");
        AddParameter($"{_config.PlcHandleTypeName}.Liquid.DiwEnterFlow.Param.FlowrateSet");
        AddParameter($"{_config.PlcHandleTypeName}.Liquid.WfUpperFaceWashDiwFlow.Param.FlowrateSet");
        AddParameter($"{_config.PlcHandleTypeName}.Liquid.WfLowerFaceDiwWashFlow.Param.FlowrateSet");

        var definitions = CreateCommandDefinitions();
        foreach (var group in definitions.GroupBy(d => d.ProjectKey))
            CommandGroups.Add(new LiquidCommandGroupViewModel(group.Key,
                group.Select(d => new LiquidCommandViewModel(d, _config.PlcHandleName, ExecuteCommand))));

        PropertyChangedEventManager.AddHandler(LocalizeDictionary.Instance, OnCultureChanged,
            nameof(LocalizeDictionary.Instance.Culture));
    }

    private void OnCultureChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args)
    {
        foreach (var parameter in Parameters) parameter.RefreshLocalization();
        foreach (var group in CommandGroups) group.RefreshLocalization();
    }

    private void AddParameter(string path)
    {
        var metadata = PlcHandleHelper.GetParamByPath(path)
                       ?? throw new InvalidOperationException($"Roller liquid parameter {path} is not defined.");
        Parameters.Add(new LiquidParameterViewModel(metadata, WriteParameter));
    }

    private int WriteDevices(KHLDevType device, uint address, uint words, byte[] data)
    {
        // Resolve at execution time so reconnecting a PLC updates both parameter and command writes.
        var links = _getPlcLinks();
        var link = _config.PlcHandleName switch
        {
            Handle.CmdRollA1 or Handle.CmdRollA2 or Handle.CmdRollA3 => links.RollerA,
            Handle.CmdRollB1 or Handle.CmdRollB2 or Handle.CmdRollB3 => links.RollerB,
            _ => throw new InvalidOperationException($"Unsupported Roller Handle: {_config.PlcHandleName:X}")
        };
        return _writeDevices(link.sock, device, address, words, data);
    }

    private bool TryWriteParameter(LiquidParameterViewModel parameter)
    {
        if (!parameter.TryGetValue(out var value))
        {
            _showResult(false, string.Format(GetText("LiquidInvalidNumber"), parameter.Name));
            return false;
        }

        if (value < parameter.MinValue || value > parameter.MaxValue)
        {
            _showResult(false, string.Format(
                GetText("LiquidValueOutOfRange"),
                parameter.Name,
                parameter.MinValue.ToString(parameter.Format, CultureInfo.CurrentCulture),
                parameter.MaxValue.ToString(parameter.Format, CultureInfo.CurrentCulture)));
            return false;
        }

        value = Math.Round(value, parameter.DecimalPlaces, MidpointRounding.AwayFromZero);
        var result = WriteDevices(
            KHLDevType.DEV_ZF,
            parameter.Handle,
            (uint)parameter.WordCount,
            ((float)value).Value());

        if (result != (int)KHLUserError.KHL_ERR_NONE)
        {
            _showResult(false, string.Format(GetText("LiquidWriteFailed"), parameter.Name, result));
            return false;
        }

        parameter.InputValue = value.ToString(parameter.Format, CultureInfo.CurrentCulture);
        return true;
    }

    private void WriteParameter(LiquidParameterViewModel parameter)
    {
        try
        {
            if (TryWriteParameter(parameter))
                _showResult(true, string.Format(GetText("LiquidWriteSucceeded"), parameter.Name));
        }
        catch (Exception ex)
        {
            _showResult(false, string.Format(GetText("LiquidWriteFailed"), parameter.Name, ex.Message));
        }
    }

    private void ExecuteCommand(LiquidCommandViewModel command)
    {
        var commandName = $"{GetText(command.ProjectKey)}: {command.ActionName}";
        try
        {
            if (command.FlowParameterHandle.HasValue)
            {
                var parameter = Parameters.First(p => p.Handle == command.FlowParameterHandle.Value);
                if (!TryWriteParameter(parameter)) return;
            }

            var (handle, value) = _config.GetLiquidCommand(command.CommandKey);
            var result = WriteDevices(KHLDevType.DEV_W, handle, 1u, value);
            if (result != (int)KHLUserError.KHL_ERR_NONE)
            {
                _showResult(false, string.Format(GetText("LiquidCommandFailed"), commandName, result));
                return;
            }

            _showResult(true, string.Format(GetText("LiquidCommandSucceeded"), commandName));
        }
        catch (Exception ex)
        {
            _showResult(false, string.Format(GetText("LiquidCommandFailed"), commandName, ex.Message));
        }
    }

    private uint GetFlowParameterHandle(string projectKey) =>
        (PlcHandleHelper.GetParamByPath($"{_config.PlcHandleTypeName}.Liquid.{projectKey}.Param.FlowrateSet")
         ?? throw new InvalidOperationException($"Roller flow parameter {projectKey} is not defined.")).Handle;

    private IReadOnlyList<LiquidCommandDefinition> CreateCommandDefinitions() =>
    [
        new("ChemicalEnterMainValve_Open", "ChemicalEnterMainValve", "ActionOpen", "FV-ROAC100A", "", 1210),
        new("ChemicalEnterMainValve_Close", "ChemicalEnterMainValve", "ActionClose", "FV-ROAC100A", "", 1211),
        new("ChemicalEnterFlow_Flowrate", "ChemicalEnterFlow", "ActionFlowrate", "FC-ROAC100", "", 1212, GetFlowParameterHandle("ChemicalEnterFlow")),
        new("ChemicalEnterFlow_FlowrateReset", "ChemicalEnterFlow", "ActionFlowrateReset", "FC-ROAC100", "", 1213),
        new("ChemicalEnterFlow_ClfcCalibration", "ChemicalEnterFlow", "ActionClfcCalibration", "FC-ROAC100", "", 1214),
        new("PipeDiwWash_Open", "PipeDiwWash", "ActionOpen", "SV-ROAC100B", "", 1215),
        new("PipeDiwWash_Close", "PipeDiwWash", "ActionClose", "SV-ROAC100B", "", 1216),
        new("DiwEnterMainValve_Open", "DiwEnterMainValve", "ActionOpen", "FV-ROAC101", "", 1217),
        new("DiwEnterMainValve_Close", "DiwEnterMainValve", "ActionClose", "FV-ROAC101", "", 1218),
        new("DiwEnterFlow_Flowrate", "DiwEnterFlow", "ActionFlowrate", "FC-ROAC101", "", 1219, GetFlowParameterHandle("DiwEnterFlow")),
        new("DiwEnterFlow_FlowrateReset", "DiwEnterFlow", "ActionFlowrateReset", "FC-ROAC101", "", 1220),
        new("DiwEnterFlow_ClfcCalibration", "DiwEnterFlow", "ActionClfcCalibration", "FC-ROAC101", "", 1221),
        new("WfLowerFaceChemicalWash_Open", "WfLowerFaceChemicalWash", "ActionOpen", "FV-ROAC100B", "WfLowerFace", 1222),
        new("WfLowerFaceChemicalWash_Close", "WfLowerFaceChemicalWash", "ActionClose", "FV-ROAC100B", "WfLowerFace", 1223),
        new("WfUpperFaceChemicalWash_Open", "WfUpperFaceChemicalWash", "ActionOpen", "SFV-ROAC100C", "WfUpperFace", 1224),
        new("WfUpperFaceChemicalWash_Close", "WfUpperFaceChemicalWash", "ActionClose", "SFV-ROAC100C", "WfUpperFace", 1225),
        new("QuartzChemicalWash_Open", "QuartzChemicalWash", "ActionOpen", "FV-ROAC100D", "QuartzWash", 1226),
        new("QuartzChemicalWash_Close", "QuartzChemicalWash", "ActionClose", "FV-ROAC100D", "QuartzWash", 1227),
        new("ChemicalInletMainValve1_Open", "ChemicalInletMainValve1", "ActionOpen", "", "ChemicalInletMainValve1", 1228),
        new("ChemicalInletMainValve1_Close", "ChemicalInletMainValve1", "ActionClose", "", "ChemicalInletMainValve1", 1229),
        new("ChemicalInletMainValve2_Open", "ChemicalInletMainValve2", "ActionOpen", "", "ChemicalInletMainValve2", 1230),
        new("WfUpperFaceDiwWashInlet_Open", "WfUpperFaceDiwWashInlet", "ActionOpen", "FV-ROAC105", "WfUpperFace", 1242),
        new("WfUpperFaceDiwWashInlet_Close", "WfUpperFaceDiwWashInlet", "ActionClose", "FV-ROAC105", "WfUpperFace", 1243),
        new("WfUpperFaceDiwWashOutlet_Open", "WfUpperFaceDiwWashOutlet", "ActionOpen", "SFV-ROAC105", "", 1244),
        new("WfUpperFaceDiwWashOutlet_Close", "WfUpperFaceDiwWashOutlet", "ActionClose", "SFV-ROAC105", "", 1245),
        new("WfUpperFaceWashDiwFlow_Flowrate", "WfUpperFaceWashDiwFlow", "ActionFlowrate", "FC-ROAC105", "", 1246, GetFlowParameterHandle("WfUpperFaceWashDiwFlow")),
        new("WfUpperFaceWashDiwFlow_FlowrateReset", "WfUpperFaceWashDiwFlow", "ActionFlowrateReset", "FC-ROAC105", "", 1247),
        new("WfUpperFaceWashDiwFlow_ClfcCalibration", "WfUpperFaceWashDiwFlow", "ActionClfcCalibration", "FC-ROAC105", "", 1248),
        new("WfLowerFaceDiwWash_Open", "WfLowerFaceDiwWash", "ActionOpen", "", "WfLowerFace", 1263),
        new("WfLowerFaceDiwWash_Close", "WfLowerFaceDiwWash", "ActionClose", "", "WfLowerFace", 1264),
        new("WfLowerFaceDiwWashFlow_Flowrate", "WfLowerFaceDiwWashFlow", "ActionFlowrate", "", "WfLowerFace", 1265, GetFlowParameterHandle("WfLowerFaceDiwWashFlow")),
        new("WfLowerFaceDiwWashFlow_FlowrateReset", "WfLowerFaceDiwWashFlow", "ActionFlowrateReset", "", "WfLowerFace", 1266),
        new("WfLowerFaceDiwWashFlow_ClfcCalibration", "WfLowerFaceDiwWashFlow", "ActionClfcCalibration", "", "WfLowerFace", 1267),
        new("UpperRollerDiwWash_Open", "UpperRollerDiwWash", "ActionOpen", "FV-ROAC102", "LiquidUpperRollerWash", 1293),
        new("UpperRollerDiwWash_Close", "UpperRollerDiwWash", "ActionClose", "FV-ROAC102", "LiquidUpperRollerWash", 1294),
        new("QuartzDiwWash_Open", "QuartzDiwWash", "ActionOpen", "FV-ROAC103", "QuartzWash", 1295),
        new("QuartzDiwWash_Close", "QuartzDiwWash", "ActionClose", "FV-ROAC103", "QuartzWash", 1296),
        new("UpperRollerInsideDiwWash_Open", "UpperRollerInsideDiwWash", "ActionOpen", "FV-ROAC104", "UpperRollerInside", 1297),
        new("UpperRollerInsideDiwWash_Close", "UpperRollerInsideDiwWash", "ActionClose", "FV-ROAC104", "UpperRollerInside", 1298),
        new("LowerRollerInsideDiwWash_Open", "LowerRollerInsideDiwWash", "ActionOpen", "FV-ROAC107", "LowerRollerInside", 1299),
        new("LowerRollerInsideDiwWash_Close", "LowerRollerInsideDiwWash", "ActionClose", "FV-ROAC107", "LowerRollerInside", 1300),
        new("MageSonicDiwWash_Open", "MageSonicDiwWash", "ActionOpen", "FV-MJROAC130", "Ultrasonic", 1301),
        new("MageSonicDiwWash_Close", "MageSonicDiwWash", "ActionClose", "FV-MJROAC130", "Ultrasonic", 1302),
        new("ChamberDiwWash_Open", "ChamberDiwWash", "ActionOpen", "FV-ROAC120", "ChamberWash", 1303),
        new("ChamberDiwWash_Close", "ChamberDiwWash", "ActionClose", "FV-ROAC120", "ChamberWash", 1304),
        new("PillarWash_Open", "PillarWash", "ActionOpen", "FV-ROAC121", "PillarWash", 1305),
        new("PillarWash_Close", "PillarWash", "ActionClose", "FV-ROAC121", "PillarWash", 1306),
        new("CoolingWaterCirculation_Open", "CoolingWaterCirculation", "ActionOpen", "FV-ROAC122", "CoolingWaterCirculation", 1307),
        new("CoolingWaterCirculation_Close", "CoolingWaterCirculation", "ActionClose", "FV-ROAC122", "CoolingWaterCirculation", 1308),
        new("UltrasonicVibrationStartup_Open", "UltrasonicVibrationStartup", "ActionOpen", "", "LiquidUltrasonicStart", 1309),
        new("UltrasonicVibrationStartup_Close", "UltrasonicVibrationStartup", "ActionClose", "", "LiquidUltrasonicStart", 1310),
        new("UltrasonicVibrationStartup_Power", "UltrasonicVibrationStartup", "ActionPower", "", "UltrasonicPower", 1311),
        new("UltrasonicVibration_Open", "UltrasonicVibration", "ActionOpen", "", "LiquidUltrasonicStart", 1400),
        new("UltrasonicVibration_Close", "UltrasonicVibration", "ActionClose", "", "LiquidUltrasonicStop", 1401),
        new("RollerManualDiwWash_Open", "RollerManualDiwWash", "ActionOpen", "", "LiquidRollerManualWashOpen", 1404),
        new("RollerManualDiwWash_Close", "RollerManualDiwWash", "ActionClose", "", "LiquidRollerManualWashClose", 1405),
        new("CylinderOneTouchAction_Open", "CylinderOneTouchAction", "ActionOpen", "", "LiquidCylinderOpen", 1406),
        new("CylinderOneTouchAction_Close", "CylinderOneTouchAction", "ActionClose", "", "LiquidCylinderClose", 1407)
    ];

    private static string GetText(string key) =>
        Resources.ResourceManager.GetString(key, LocalizeDictionary.Instance.Culture) ?? key;
}

public sealed class LiquidParameterViewModel : BindableBase
{
    private readonly Action<LiquidParameterViewModel> _write;
    private string _inputValue;
    private string _name;

    public LiquidParameterViewModel(PlcHandle.ParamAttribute metadata, Action<LiquidParameterViewModel> write)
    {
        Metadata = metadata;
        _write = write;
        _name = GetText(metadata.DisplayNameKey, metadata.Label);
        _inputValue = metadata.NumericDefaultValue.ToString(Format, CultureInfo.CurrentCulture);
        SetCommand = new DelegateCommand(() => _write(this));
    }

    public PlcHandle.ParamAttribute Metadata { get; }
    public uint Handle => Metadata.Handle;
    public double MinValue => Metadata.NumericMinValue;
    public double MaxValue => Metadata.NumericMaxValue;
    public int DecimalPlaces => Metadata.DecimalPlaces;
    public string Format => $"F{DecimalPlaces}";
    public string DrawingTag => Metadata.DrawingTag;
    public string Unit => Metadata.Unit;
    public string Address => $"{Metadata.ZrZf}{Metadata.Handle}";
    public int WordCount => Metadata.WordCount;
    public string EnglishName => GetText(Metadata.DisplayNameKey, Metadata.Label, CultureInfo.GetCultureInfo("en"));
    public string ChineseName => GetText(Metadata.DisplayNameKey, Metadata.Label, CultureInfo.GetCultureInfo("zh"));
    public string DefaultValueText => Metadata.NumericDefaultValue.ToString(Format, CultureInfo.CurrentCulture);
    public string RangeText => $"{MinValue.ToString(Format, CultureInfo.CurrentCulture)} – {MaxValue.ToString(Format, CultureInfo.CurrentCulture)}";

    public string Name
    {
        get => _name;
        private set => SetProperty(ref _name, value);
    }

    public string InputValue
    {
        get => _inputValue;
        set => SetProperty(ref _inputValue, value);
    }

    public DelegateCommand SetCommand { get; }

    public bool TryGetValue(out double value) =>
        (double.TryParse(InputValue, NumberStyles.Float, CultureInfo.CurrentCulture, out value)
         || double.TryParse(InputValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        && double.IsFinite(value);

    public void RefreshLocalization()
    {
        Name = GetText(Metadata.DisplayNameKey, Metadata.Label);
        RaisePropertyChanged(nameof(DefaultValueText));
        RaisePropertyChanged(nameof(RangeText));
    }

    private static string GetText(string key, string fallback) =>
        GetText(key, fallback, LocalizeDictionary.Instance.Culture);

    private static string GetText(string key, string fallback, CultureInfo culture) =>
        string.IsNullOrWhiteSpace(key)
            ? fallback
            : Resources.ResourceManager.GetString(key, culture) ?? fallback;
}

public sealed record LiquidCommandDefinition(
    string CommandKey,
    string ProjectKey,
    string ActionKey,
    string PidTag,
    string DescriptionKey,
    int CommandCode,
    uint? FlowParameterHandle = null);

public sealed class LiquidCommandGroupViewModel : BindableBase
{
    private string _title;

    public LiquidCommandGroupViewModel(string projectKey, IEnumerable<LiquidCommandViewModel> commands)
    {
        ProjectKey = projectKey;
        _title = GetText(projectKey);
        Commands = new ObservableCollection<LiquidCommandViewModel>(commands);
    }

    public string ProjectKey { get; }
    public ObservableCollection<LiquidCommandViewModel> Commands { get; }
    public string EnglishTitle => Resources.ResourceManager.GetString(ProjectKey, CultureInfo.GetCultureInfo("en")) ?? ProjectKey;
    public string ChineseTitle => Resources.ResourceManager.GetString(ProjectKey, CultureInfo.GetCultureInfo("zh")) ?? ProjectKey;
    public string PidTag => string.Join(", ", Commands.Select(c => c.PidTag).Where(tag => !string.IsNullOrWhiteSpace(tag)).Distinct());

    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public void RefreshLocalization()
    {
        Title = GetText(ProjectKey);
        foreach (var command in Commands) command.RefreshLocalization();
    }

    private static string GetText(string key) =>
        Resources.ResourceManager.GetString(key, LocalizeDictionary.Instance.Culture) ?? key;
}

public sealed class LiquidCommandViewModel : BindableBase
{
    private readonly Action<LiquidCommandViewModel> _execute;
    private string _actionName;
    private string _description;

    public LiquidCommandViewModel(LiquidCommandDefinition definition, uint handle, Action<LiquidCommandViewModel> execute)
    {
        Definition = definition;
        Handle = handle;
        _execute = execute;
        _actionName = GetText(definition.ActionKey);
        _description = GetText(definition.DescriptionKey);
        ExecuteCommand = new DelegateCommand(() => _execute(this));
    }

    public LiquidCommandDefinition Definition { get; }
    public uint Handle { get; }
    public string CommandKey => Definition.CommandKey;
    public string ProjectKey => Definition.ProjectKey;
    public int CommandCode => Definition.CommandCode;
    public string PidTag => Definition.PidTag;
    public uint? FlowParameterHandle => Definition.FlowParameterHandle;
    public string Address => $"W{Handle:X}";
    public uint WordCount => 1u;
    public string EnglishActionName => GetText(Definition.ActionKey, CultureInfo.GetCultureInfo("en"));
    public string ChineseActionName => GetText(Definition.ActionKey, CultureInfo.GetCultureInfo("zh"));

    public string ActionName
    {
        get => _actionName;
        private set => SetProperty(ref _actionName, value);
    }

    public string Description
    {
        get => _description;
        private set => SetProperty(ref _description, value);
    }

    public DelegateCommand ExecuteCommand { get; }

    public void RefreshLocalization()
    {
        ActionName = GetText(Definition.ActionKey);
        Description = GetText(Definition.DescriptionKey);
    }

    private static string GetText(string key) =>
        GetText(key, LocalizeDictionary.Instance.Culture);

    private static string GetText(string key, CultureInfo culture) =>
        string.IsNullOrWhiteSpace(key)
            ? string.Empty
            : Resources.ResourceManager.GetString(key, culture) ?? key;
}
