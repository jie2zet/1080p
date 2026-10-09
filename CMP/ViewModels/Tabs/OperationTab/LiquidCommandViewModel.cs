using System.Globalization;
using CMP.Constants.Plc;
using CMP.Lang;
using Prism.Commands;
using Prism.Mvvm;
using WPFLocalizeExtension.Engine;

namespace CMP.ViewModels.Tabs.OperationTab;

public sealed class LiquidCommandViewModel : BindableBase
{
    private readonly Action<LiquidCommandViewModel> _execute;
    private string _actionName;

    public LiquidCommandViewModel(LiquidCommandDefinition definition, Action<LiquidCommandViewModel> execute)
    {
        Definition = definition;
        _execute = execute;
        _actionName = GetText(definition.ActionKey);
        ExecuteCommand = new DelegateCommand(() => _execute(this));
    }

    public LiquidCommandDefinition Definition { get; }
    public uint Handle => Definition.Handle;
    public string CommandPath => Definition.CommandPath;
    public string CommandKey => Definition.CommandKey;
    public string ProjectKey => Definition.ProjectKey;
    public int CommandCode => Definition.CommandCode;
    public string PidTag => Definition.PidTag;
    public uint? ParameterHandle => Definition.ParameterHandle;
    public string Address => $"W{Handle:X}";
    public uint WordCount => 1u;
    public string EnglishActionName => GetText(Definition.ActionKey, CultureInfo.GetCultureInfo("en"));
    public string ChineseActionName => GetText(Definition.ActionKey, CultureInfo.GetCultureInfo("zh"));

    public string ActionName
    {
        get => _actionName;
        private set => SetProperty(ref _actionName, value);
    }

    public DelegateCommand ExecuteCommand { get; }

    public void RefreshLocalization()
    {
        ActionName = GetText(Definition.ActionKey);
    }

    private static string GetText(string key) =>
        GetText(key, LocalizeDictionary.Instance.Culture);

    private static string GetText(string key, CultureInfo culture) =>
        string.IsNullOrWhiteSpace(key)
            ? string.Empty
            : Resources.ResourceManager.GetString(key, culture) ?? key;
}
