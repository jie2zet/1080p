using System.ComponentModel;
using System.Globalization;
using System.Windows;
using CMP.Constants.Plc;
using CMP.Lang;
using CMP.Models.kvsPlc;
using CMP.Services.Auth;
using HandyControl.Controls;
using Prism.Commands;
using Prism.Mvvm;
using WPFLocalizeExtension.Engine;
using KHL = CMP.Models.kvsPlc.KvHostLink;
using SlurrySwitchDefinition = CMP.Constants.Plc.PlcHandle.CmdPolish.Cylinder.SlurrySwitchDefinition;

namespace CMP.ViewModels.Tabs.OperationTab.PolishUnitTab;

/// <summary>仅由 Operation 的 PolishUnit 页面使用的 Slurry 控制。</summary>
public sealed class PolishSlurryContentViewModel : BindableBase
{
    private readonly Func<int> _getSocket;
    private readonly Func<int, KHLDevType, uint, uint, byte[], int> _writeDevices;
    private readonly Action<bool, string> _showResult;
    private readonly Func<string> _getUnit;

    public PolishSlurryContentViewModel(Func<int> getSocket, Func<string> getUnit) : this(
        getSocket, KHL.KHLWriteDevicesAsWords,
        (success, message) =>
        {
            if (success) Growl.SuccessGlobal(message);
            else Growl.ErrorGlobal(message);
        }, getUnit)
    {
    }

    internal PolishSlurryContentViewModel(
        Func<int> getSocket,
        Func<int, KHLDevType, uint, uint, byte[], int> writeDevices,
        Action<bool, string> showResult,
        Func<string> getUnit)
    {
        _getSocket = getSocket;
        _writeDevices = writeDevices;
        _showResult = showResult;
        _getUnit = getUnit;
        Parameters = Enumerable.Range(1, 3).Select(line => new PolishSlurryParameterViewModel(
            PlcHandleHelper.GetParamByPath($"CmdPolish.Liquid.Slurry{line}.Param.Flowrate")
            ?? throw new InvalidOperationException($"Missing Slurry{line} flowrate metadata."))).ToArray();
        var controls = PlcHandle.CmdPolish.Cylinder.GetSlurryControls()
            .Select(definition => new PolishSlurrySwitchViewModel(definition, ExecuteSwitch)).ToArray();
        TotalControl = controls[0];
        Lines = Enumerable.Range(0, 3).Select(index => new PolishSlurryLineViewModel(
            Parameters[index], controls.Skip(1 + index * 3).Take(3).ToArray(), SetParameter)).ToArray();
        PropertyChangedEventManager.AddHandler(LocalizeDictionary.Instance, OnCultureChanged,
            nameof(LocalizeDictionary.Instance.Culture));
    }

    public IReadOnlyList<PolishSlurryParameterViewModel> Parameters { get; }
    public PolishSlurrySwitchViewModel TotalControl { get; }
    public IReadOnlyList<PolishSlurryLineViewModel> Lines { get; }

    private void OnCultureChanged(object? sender, PropertyChangedEventArgs args)
    {
        TotalControl.RefreshLocalization();
        foreach (var line in Lines)
        {
            line.RefreshLocalization();
            foreach (var control in line.Controls) control.RefreshLocalization();
        }
    }

    private void SetParameter(PolishSlurryParameterViewModel parameter) =>
        ExecuteOperation($"SlurryFlowrate.Set.{parameter.Handle}", parameter.Name,
            "LiquidWriteSucceeded", "LiquidWriteFailed", () => WriteParameter(parameter, Validate(parameter)));

    private void ExecuteSwitch(PolishSlurrySwitchViewModel control, bool open)
    {
        var definition = control.Definition;
        var path = open ? definition.OpenPath : definition.ClosePath;
        var name = $"{control.Title}: {GetText(open ? "ActionOpen" : "ActionClose")}";
        ExecuteOperation(path, name, "LiquidCommandSucceeded", "LiquidCommandFailed", () =>
        {
            if (open)
            {
                var parameters = definition.ParameterHandle.HasValue
                    ? Parameters.Where(p => p.Handle == definition.ParameterHandle.Value).ToArray()
                    : Parameters.ToArray();
                // Validate every selected line before writing any setpoint for the combined action.
                var values = parameters.Select(Validate).ToArray();
                for (var i = 0; i < parameters.Length; i++) WriteParameter(parameters[i], values[i]);
            }
            var handle = PlcHandleHelper.GetHandleByPath(path)
                ?? throw new InvalidOperationException($"Missing command handle: {path}");
            var code = PlcHandleHelper.GetValueByPath(path)
                ?? throw new InvalidOperationException($"Missing command code: {path}");
            WriteDevices(KHLDevType.DEV_W, handle, 1, code.Value());
        });
    }

    private static double Validate(PolishSlurryParameterViewModel parameter)
    {
        if (!parameter.TryGetValue(out var value))
            throw new InvalidOperationException(string.Format(GetText("LiquidInvalidNumber"), parameter.Name));
        if (value < parameter.MinValue || value > parameter.MaxValue)
            throw new InvalidOperationException(string.Format(GetText("LiquidValueOutOfRange"), parameter.Name,
                parameter.MinValue.ToString(parameter.Format, CultureInfo.CurrentCulture),
                parameter.MaxValue.ToString(parameter.Format, CultureInfo.CurrentCulture)));
        return value;
    }

    private void WriteParameter(PolishSlurryParameterViewModel parameter, double value)
    {
        WriteDevices(KHLDevType.DEV_ZF, parameter.Handle, (uint)parameter.WordCount, ((float)value).Value());
    }

    private void WriteDevices(KHLDevType device, uint address, uint words, byte[] data)
    {
        var result = _writeDevices(_getSocket(), device, address, words, data);
        if (result != (int)KHLUserError.KHL_ERR_NONE)
            throw new InvalidOperationException(result.ToString(CultureInfo.InvariantCulture));
    }

    private void ExecuteOperation(string action, string name, string successKey, string failureKey, Action execute)
    {
        try
        {
            execute();
            _showResult(true, string.Format(GetText(successKey), name));
            AuditLogService.WriteCurrentUser(action, "Success", string.Empty, AuditModules.Operation, _getUnit());
        }
        catch (Exception ex)
        {
            _showResult(false, string.Format(GetText(failureKey), name, ex.Message));
            AuditLogService.WriteCurrentUser(action, "Failed", ex.Message, AuditModules.Operation, _getUnit());
        }
    }

    internal static string GetText(string key) =>
        Resources.ResourceManager.GetString(key, LocalizeDictionary.Instance.Culture) ?? key;
}

public sealed class PolishSlurryParameterViewModel : BindableBase
{
    private string _inputValue;

    internal PolishSlurryParameterViewModel(PlcHandle.ParamAttribute metadata)
    {
        Metadata = metadata;
        _inputValue = metadata.NumericDefaultValue.ToString(Format, CultureInfo.CurrentCulture);
    }

    public PlcHandle.ParamAttribute Metadata { get; }
    public uint Handle => Metadata.Handle;
    public string Name => PolishSlurryContentViewModel.GetText(Metadata.DisplayNameKey);
    public double MinValue => Metadata.NumericMinValue;
    public double MaxValue => Metadata.NumericMaxValue;
    public int DecimalPlaces => Metadata.DecimalPlaces;
    public int WordCount => Metadata.WordCount;
    public string Unit => Metadata.Unit;
    public string Format => $"F{DecimalPlaces}";

    public string InputValue
    {
        get => _inputValue;
        set => SetProperty(ref _inputValue, value);
    }

    public bool TryGetValue(out double value) =>
        (double.TryParse(InputValue, NumberStyles.Float, CultureInfo.CurrentCulture, out value)
         || double.TryParse(InputValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        && double.IsFinite(value);
}

public sealed class PolishSlurryLineViewModel : BindableBase
{
    private float _actualFlowrate;

    internal PolishSlurryLineViewModel(PolishSlurryParameterViewModel parameter,
        IReadOnlyList<PolishSlurrySwitchViewModel> controls, Action<PolishSlurryParameterViewModel> setParameter)
    {
        Parameter = parameter;
        Controls = controls;
        SetCommand = new DelegateCommand(() => setParameter(parameter));
    }

    public PolishSlurryParameterViewModel Parameter { get; }
    public IReadOnlyList<PolishSlurrySwitchViewModel> Controls { get; }
    public string Title => Controls[2].Title;
    public DelegateCommand SetCommand { get; }

    public float ActualFlowrate
    {
        get => _actualFlowrate;
        set => SetProperty(ref _actualFlowrate, value);
    }

    internal void RefreshLocalization() => RaisePropertyChanged(nameof(Title));
}

public sealed class PolishSlurrySwitchViewModel : BindableBase
{
    internal PolishSlurrySwitchViewModel(SlurrySwitchDefinition definition, Action<PolishSlurrySwitchViewModel, bool> execute)
    {
        Definition = definition;
        OpenCommand = new DelegateCommand(() => execute(this, true));
        CloseCommand = new DelegateCommand(() => execute(this, false));
    }

    public SlurrySwitchDefinition Definition { get; }
    public string Title => PolishSlurryContentViewModel.GetText(Definition.DisplayNameKey);
    public DelegateCommand OpenCommand { get; }
    public DelegateCommand CloseCommand { get; }

    internal void RefreshLocalization() => RaisePropertyChanged(nameof(Title));
}
