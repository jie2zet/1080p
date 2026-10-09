using System.Collections.ObjectModel;
using System.Globalization;
using CMP.Lang;
using Prism.Mvvm;
using WPFLocalizeExtension.Engine;

namespace CMP.ViewModels.Tabs.OperationTab;

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
