using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using CMP.Constants.Plc;
using CMP.Lang;
using CMP.Models.kvsPlc;
using Prism.Mvvm;
using WPFLocalizeExtension.Engine;

namespace CMP.ViewModels.Tabs.OperationTab;

/// <summary>
/// 液路腔体共用的参数校验、写入及本地化；命令读取 PlcHandle 元数据，腔体配置负责 PLC 路由。
/// </summary>
public abstract class LiquidChamberContentViewModel : BindableBase
{
    private readonly Func<int> _getSocket;
    private readonly Func<int, KHLDevType, uint, uint, byte[], int> _writeDevices;
    private readonly Action<bool, string> _showResult;

    public ObservableCollection<LiquidParameterViewModel> Parameters { get; } = [];
    public ObservableCollection<LiquidCommandGroupViewModel> CommandGroups { get; } = [];

    protected LiquidChamberContentViewModel(
        Func<int> getSocket,
        Func<int, KHLDevType, uint, uint, byte[], int> writeDevices,
        Action<bool, string> showResult)
    {
        _getSocket = getSocket;
        _writeDevices = writeDevices;
        _showResult = showResult;
        PropertyChangedEventManager.AddHandler(LocalizeDictionary.Instance, OnCultureChanged,
            nameof(LocalizeDictionary.Instance.Culture));
    }

    protected void AddParameter(string path) =>
        Parameters.Add(new LiquidParameterViewModel(GetParameterMetadata(path), WriteParameter));

    private static PlcHandle.ParamAttribute GetParameterMetadata(string path) =>
        PlcHandleHelper.GetParamByPath(path)
        ?? throw new InvalidOperationException($"Liquid parameter {path} is not defined.");

    protected void AddCommandGroups(IEnumerable<LiquidCommandDefinition> definitions)
    {
        foreach (var group in definitions.GroupBy(d => d.ProjectKey))
            CommandGroups.Add(new LiquidCommandGroupViewModel(group.Key,
                group.Select(d => new LiquidCommandViewModel(d, ExecuteCommand))));
    }

    private void OnCultureChanged(object? sender, PropertyChangedEventArgs args)
    {
        foreach (var parameter in Parameters) parameter.RefreshLocalization();
        foreach (var group in CommandGroups) group.RefreshLocalization();
    }

    private int WriteDevices(KHLDevType device, uint address, uint words, byte[] data) =>
        // Resolve the current socket for every write, including after PLC reconnection.
        _writeDevices(_getSocket(), device, address, words, data);

    private bool TryWriteParameter(LiquidParameterViewModel parameter)
    {
        if (!parameter.TryGetValue(out var value))
        {
            _showResult(false, string.Format(GetText("LiquidInvalidNumber"), parameter.Name));
            return false;
        }

        if (value < parameter.MinValue || value > parameter.MaxValue)
        {
            _showResult(false, string.Format(GetText("LiquidValueOutOfRange"), parameter.Name,
                parameter.MinValue.ToString(parameter.Format, CultureInfo.CurrentCulture),
                parameter.MaxValue.ToString(parameter.Format, CultureInfo.CurrentCulture)));
            return false;
        }

        var result = WriteDevices(KHLDevType.DEV_ZF, parameter.Handle, (uint)parameter.WordCount,
            ((float)value).Value());
        if (result != (int)KHLUserError.KHL_ERR_NONE)
        {
            _showResult(false, string.Format(GetText("LiquidWriteFailed"), parameter.Name, result));
            return false;
        }

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
            if (command.ParameterHandle.HasValue)
            {
                var parameter = Parameters.First(p => p.Handle == command.ParameterHandle.Value);
                if (!TryWriteParameter(parameter)) return;
            }

            var result = WriteDevices(KHLDevType.DEV_W, command.Handle, command.WordCount, command.CommandCode.Value());
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

    private static string GetText(string key) =>
        Resources.ResourceManager.GetString(key, LocalizeDictionary.Instance.Culture) ?? key;
}
