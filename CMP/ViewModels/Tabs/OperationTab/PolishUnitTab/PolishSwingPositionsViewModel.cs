using System.ComponentModel;
using System.Threading;
using System.Windows;
using CMP.Constants.Plc;
using CMP.Lang;
using CMP.Models.kvsPlc;
using CMP.Models.Operation;
using CMP.Models.Operation.Polish;
using CMP.Services.Auth;
using HandyControl.Controls;
using Prism.Commands;
using Prism.Mvvm;
using WPFLocalizeExtension.Engine;
using static CMP.ViewModels.Tabs.OperationTab.PolishUnitTab.PolishSwingPositionsViewModel;
using Cmd = CMP.Constants.Plc.PlcHandle.CmdPolish;
using KHL = CMP.Models.kvsPlc.KvHostLink;

namespace CMP.ViewModels.Tabs.OperationTab.PolishUnitTab;

/// <summary>摆动电机2；由所属 Polish 页的监控循环读取，不创建额外定时器。</summary>
public sealed class PolishSwingPositionsViewModel
{
    private readonly Func<int> _getSocket;
    private readonly Func<string> _getUnit;
    private readonly Func<int, KHLDevType, uint, uint, byte[], int> _readWords;
    private readonly Func<int, KHLDevType, uint, ushort, uint, byte[], int> _readBits;
    private readonly Func<int, KHLDevType, uint, uint, byte[], int> _writeWords;
    private readonly Func<int, KHLDevType, uint, ushort, uint, byte[], int> _writeBits;
    private readonly Action<string> _showError;
    private readonly Action<string, string, string, string> _audit;
    private readonly IReadOnlyList<PolishSwingParameterViewModel> _parameters;
    private readonly IReadOnlyList<PolishSwingPositionViewModel> _positions;
    private bool _isActive;

    public PolishSwingPositionsViewModel(Func<int> getSocket, Func<string> getUnit, PolishUnit unit)
        : this(getSocket, getUnit, unit, KHL.KHLReadDevicesAsWords, KHL.KHLReadDevicesAsBits,
            KHL.KHLWriteDevicesAsWords, KHL.KHLWriteDevicesAsBits, message => Growl.ErrorGlobal(message))
    {
    }

    internal PolishSwingPositionsViewModel(Func<int> getSocket, Func<string> getUnit, PolishUnit unit,
        Func<int, KHLDevType, uint, uint, byte[], int> readWords,
        Func<int, KHLDevType, uint, ushort, uint, byte[], int> readBits,
        Func<int, KHLDevType, uint, uint, byte[], int> writeWords,
        Func<int, KHLDevType, uint, ushort, uint, byte[], int> writeBits,
        Action<string> showError,
        Action<string, string, string, string>? audit = null)
    {
        _getSocket = getSocket;
        _getUnit = getUnit;
        _readWords = readWords;
        _readBits = readBits;
        _writeWords = writeWords;
        _writeBits = writeBits;
        _showError = showError;
        _audit = audit ?? ((action, result, detail, target) =>
            AuditLogService.WriteCurrentUser(action, result, detail, AuditModules.Operation, target));
        Motors = Cmd.GetSwingPositionMotors().Select(definition =>
        {
            var motor = definition.Key switch
            {
                "HeadSweep" => unit.HeadSweepMotor,
                "HeadUpDown" => unit.HeadLiftMotor,
                "ConditionerSweep" => unit.ConditionSweepMotor,
                "SlurryArmSweep" => unit.SlurrySweepMotor,
                _ => throw new InvalidOperationException($"Unknown swing motor: {definition.Key}")
            };
            var positionMetadata = PlcHandleHelper.GetParamByPath($"CmdPolish.Motor.{definition.Key}.Param.Position")
                ?? throw new InvalidOperationException($"Missing position metadata: {definition.Key}");
            var velocityMetadata = PlcHandleHelper.GetParamByPath($"CmdPolish.Motor.{definition.Key}.Param.Velocity")
                ?? throw new InvalidOperationException($"Missing velocity metadata: {definition.Key}");
            return new PolishSwingPositionMotorViewModel(definition, motor,
                definition.Key == "SlurryArmSweep" ? unit.HeadRotateMotor : null,
                positionMetadata, velocityMetadata, SetParameter, ExecuteCommand, ExecuteServoCommand);
        }).ToArray();
        _positions = Motors.SelectMany(motor => motor.Positions).ToArray();
        _parameters = Motors.SelectMany(motor => motor.Positions.Cast<PolishSwingParameterViewModel>()
            .Concat(motor.Velocities)).ToArray();
        PropertyChangedEventManager.AddHandler(LocalizeDictionary.Instance, OnCultureChanged,
            nameof(LocalizeDictionary.Instance.Culture));
    }

    internal static string GetText(string key) =>
        Resources.ResourceManager.GetString(key, LocalizeDictionary.Instance.Culture) ?? key;

    private static string Format(string key, params object[] args) =>
        string.Format(LocalizeDictionary.Instance.Culture, GetText(key), args);

    public IReadOnlyList<PolishSwingPositionMotorViewModel> Motors { get; }

    private void OnCultureChanged(object? sender, PropertyChangedEventArgs args)
    {
        foreach (var motor in Motors) motor.RefreshLocalization();
        foreach (var parameter in _parameters) parameter.RefreshLocalization();
    }

    // 视图不可见时（包括切换 Polish A/B/C/D）停止新增参数的读取。
    public bool IsActive
    {
        get => Volatile.Read(ref _isActive);
        set => Volatile.Write(ref _isActive, value);
    }

    internal sealed record ParameterFeedback(PolishSwingParameterViewModel Parameter, float? Value, long Version);
    internal sealed record PositionFeedback(PolishSwingPositionViewModel Position, bool? IsReached);
    internal sealed record Feedback(ParameterFeedback[] Parameters, PositionFeedback[] Positions);

    internal Feedback? ReadFeedback()
    {
        if (!IsActive) return null;
        var socket = _getSocket();
        if (socket <= 0)
            return new Feedback(_parameters.Select(parameter => new ParameterFeedback(parameter, null, parameter.WriteVersion)).ToArray(),
                _positions.Select(position => new PositionFeedback(position, null)).ToArray());
        var parameters = _parameters.Select(parameter =>
        {
            var version = parameter.WriteVersion;
            var data = new byte[4];
            var result = _readWords(socket, KHLDevType.DEV_ZF, parameter.AddressZf, 2, data);
            var value = BitConverter.ToSingle(data, 0);
            return new ParameterFeedback(parameter,
                result == (int)KHLUserError.KHL_ERR_NONE && float.IsFinite(value) ? value : null, version);
        }).ToArray();
        var positions = _positions.Select(position =>
        {
            var data = new byte[1];
            var result = _readBits(socket, KHLDevType.DEV_MR, position.ArrivedMr, 0, 1, data);
            return new PositionFeedback(position,
                result == (int)KHLUserError.KHL_ERR_NONE ? data[0] != 0 : null);
        }).ToArray();
        return new Feedback(parameters, positions);
    }

    internal void ApplyFeedback(Feedback? feedback)
    {
        if (feedback == null) return;
        foreach (var item in feedback.Parameters) item.Parameter.ApplyFeedback(item.Value, item.Version);
        foreach (var item in feedback.Positions) item.Position.ApplyArrival(item.IsReached);
    }

    private void SetParameter(PolishSwingParameterViewModel parameter)
    {
        ExecuteOperation("SetValue", $"{parameter.MotorTitle}/{parameter.Title}/ZF{parameter.AddressZf}", () =>
        {
            var value = parameter.Value;
            if (!double.IsFinite(value) || value < parameter.Minimum || value > parameter.Maximum)
                throw new InvalidOperationException(Format("PolishMotorValueOutOfRange", parameter.Minimum, parameter.Maximum));
            // 防止本次写入之前或写入过程中的采样覆盖刚提交的数值。
            parameter.BeginWrite();
            try
            {
                Write(KHLDevType.DEV_ZF, parameter.AddressZf, 2, BitConverter.GetBytes((float)value));
                parameter.AcceptValue((float)value);
            }
            finally { parameter.EndWrite(); }
        });
    }

    private void ExecuteCommand(string title, uint command) =>
        ExecuteOperation("MotorCommand", $"{title}/W50={command}",
            () => Write(KHLDevType.DEV_W, Handle.CmdPolish, 1, BitConverter.GetBytes((ushort)command)));

    private void ExecuteServoCommand(PolishSwingPositionMotorViewModel motor)
    {
        var newState = !motor.Motor.IsServoOn;
        ExecuteOperation("Servo", $"{motor.Title}/{GetText("Servo")}", () =>
        {
            WriteBit(
                motor.ServoOn.HexWAddress(),
                motor.ServoBitOffset,
                newState);
            motor.Motor.IsServoOn = newState;
        });
    }

    private void WriteBit(uint address, ushort bitOffset, bool value)
    {
        var socket = _getSocket();
        if (socket <= 0) throw new InvalidOperationException(GetText("PolishMotorPlcDisconnected"));
        var result = _writeBits(socket, KHLDevType.DEV_W, address, bitOffset, 1, value.Value());
        if (result != (int)KHLUserError.KHL_ERR_NONE)
            throw new InvalidOperationException(Format("PolishMotorPlcWriteFailed", result));
    }

    private void Write(KHLDevType device, uint address, uint words, byte[] data)
    {
        var socket = _getSocket();
        if (socket <= 0) throw new InvalidOperationException(GetText("PolishMotorPlcDisconnected"));
        var result = _writeWords(socket, device, address, words, data);
        if (result != (int)KHLUserError.KHL_ERR_NONE)
            throw new InvalidOperationException(Format("PolishMotorPlcWriteFailed", result));
    }

    private void ExecuteOperation(string action, string target, Action execute)
    {
        target = $"{_getUnit()}/{target}";
        try
        {
            execute();
            _audit(action, "Success", string.Empty, target);
        }
        catch (Exception ex)
        {
            _audit(action, "Failed", ex.Message, target);
            _showError(ex.Message);
        }
    }
}

public class PolishSwingParameterViewModel : BindableBase
{
    private readonly string _motorTitleKey;
    private readonly string _titleKey;
    private double _value;
    private float _plcValue;
    private bool _isLoaded;
    private bool _isDirty;
    private long _writeVersion;

    internal PolishSwingParameterViewModel(string motorTitleKey, string titleKey, uint addressZf,
        double minimum, double maximum, Action<PolishSwingParameterViewModel> setParameter)
    {
        _motorTitleKey = motorTitleKey;
        _titleKey = titleKey;
        AddressZf = addressZf;
        Minimum = minimum;
        Maximum = maximum;
        SetCommand = new DelegateCommand(() => setParameter(this));
        CancelCommand = new DelegateCommand(() => AcceptValue(_plcValue));
    }

    public string MotorTitle => GetText(_motorTitleKey);
    public string Title => GetText(_titleKey);
    internal void RefreshLocalization()
    {
        RaisePropertyChanged(nameof(MotorTitle));
        RaisePropertyChanged(nameof(Title));
    }
    public uint AddressZf { get; }
    public double Minimum { get; }
    public double Maximum { get; }
    public DelegateCommand SetCommand { get; }
    public DelegateCommand CancelCommand { get; }
    public double Value
    {
        get => _value;
        set
        {
            if (SetProperty(ref _value, value)) IsDirty = !value.Equals((double)_plcValue);
        }
    }
    public bool IsDirty { get => _isDirty; private set => SetProperty(ref _isDirty, value); }
    public bool IsLoaded { get => _isLoaded; private set => SetProperty(ref _isLoaded, value); }
    internal long WriteVersion => Interlocked.Read(ref _writeVersion);
    internal void BeginWrite() => Interlocked.Increment(ref _writeVersion);
    internal void EndWrite() => Interlocked.Increment(ref _writeVersion);

    internal void ApplyFeedback(float? value, long version)
    {
        if (version != WriteVersion || (version & 1) != 0) return;
        IsLoaded = value.HasValue;
        if (!value.HasValue) return;
        _plcValue = value.Value;
        if (!IsDirty) SetProperty(ref _value, (double)value.Value, nameof(Value));
    }

    internal void AcceptValue(float value)
    {
        _plcValue = value;
        SetProperty(ref _value, (double)value, nameof(Value));
        IsDirty = false;
    }
}

public sealed class PolishSwingPositionViewModel : PolishSwingParameterViewModel
{
    private bool _isReached;
    private bool _isArrivalKnown;

    internal PolishSwingPositionViewModel(string motorTitleKey, Cmd.SwingPositionDefinition definition,
        double minimum, double maximum, Action<PolishSwingParameterViewModel> setParameter,
        Action<string, uint> executeCommand)
        : base(motorTitleKey, definition.TitleKey, definition.PositionZf, minimum, maximum, setParameter)
    {
        ArrivedMr = definition.ArrivedMr;
        StartCommand = new DelegateCommand(() => executeCommand($"{MotorTitle}/{Title}/{GetText("PolishMotorStart")}", definition.StartDm),
                () => !IsDirty)
            .ObservesProperty(() => IsDirty);
    }

    public uint ArrivedMr { get; }
    public DelegateCommand StartCommand { get; }
    public bool IsReached { get => _isReached; private set => SetProperty(ref _isReached, value); }
    public bool IsArrivalKnown { get => _isArrivalKnown; private set => SetProperty(ref _isArrivalKnown, value); }

    internal void ApplyArrival(bool? value)
    {
        IsArrivalKnown = value.HasValue;
        IsReached = value == true;
    }
}

public sealed class PolishSwingPositionMotorViewModel : BindableBase
{
    private readonly string _titleKey;
    internal PolishSwingPositionMotorViewModel(Cmd.SwingPositionMotorDefinition definition, SweepMotor motor,
        Motor? extraTorqueMotor, PlcHandle.ParamAttribute positionMetadata, PlcHandle.ParamAttribute velocityMetadata,
        Action<PolishSwingParameterViewModel> setParameter, Action<string, uint> executeCommand,
        Action<PolishSwingPositionMotorViewModel> executeServoCommand)
    {
        _titleKey = definition.TitleKey;
        Motor = motor;
        ExtraTorqueMotor = extraTorqueMotor;
        ServoOn = definition.ServoOn;
        ServoOff = definition.ServoOff;
        ServoBitOffset = definition.ServoBitOffset;
        Positions = definition.Positions.Select(position => new PolishSwingPositionViewModel(_titleKey, position,
            positionMetadata.NumericMinValue, positionMetadata.NumericMaxValue, setParameter, executeCommand)).ToArray();
        Velocities = new[] { ("PolishMotorManualSpeed", definition.ManualVelocityZf), ("PolishMotorAutoSpeed", definition.AutoVelocityZf) }
            .Select(item => new PolishSwingParameterViewModel(_titleKey, item.Item1, item.Item2,
                velocityMetadata.NumericMinValue, velocityMetadata.NumericMaxValue, setParameter)).ToArray();
        StopCommand = new DelegateCommand(() => executeCommand($"{Title}/{GetText("Stop")}", definition.StopDm));
        ServoCommand = new DelegateCommand(() => executeServoCommand(this));
        Motor.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Motor.IsServoOn)) RaisePropertyChanged(nameof(ServoText));
        };
    }

    public string Title => GetText(_titleKey);
    internal void RefreshLocalization()
    {
        RaisePropertyChanged(nameof(Title));
        RaisePropertyChanged(nameof(ServoText));
    }
    public SweepMotor Motor { get; }
    public Motor? ExtraTorqueMotor { get; }
    public int ServoOn { get; }
    public int ServoOff { get; }
    public ushort ServoBitOffset { get; }
    public string ServoText => Motor.IsServoOn ? GetText("Off") : GetText("On");
    public bool HasExtraTorque => ExtraTorqueMotor != null;
    public IReadOnlyList<PolishSwingPositionViewModel> Positions { get; }
    public IReadOnlyList<PolishSwingParameterViewModel> Velocities { get; }
    public DelegateCommand StopCommand { get; }
    public DelegateCommand ServoCommand { get; }
}
