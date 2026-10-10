using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using CMP.Constants.Enums;
using CMP.Lang;
using CMP.Models.Operation.Clean;
using CMP.Models.Operation.Roller;
using CMP.ViewModels.Tabs.OperationTab;
using Prism.Mvvm;
using WPFLocalizeExtension.Engine;

namespace CMP.ViewModels.Tabs.MainTab.Content;

/// <summary>
/// Operation 页面中的集中式液路控制页。
/// </summary>
public sealed class LiquidContentViewModel : BindableBase
{
    public ObservableCollection<LiquidCategoryViewModel> Tabs { get; } = [];

    public LiquidContentViewModel()
    {
        Tabs.Add(new LiquidCategoryViewModel("LiquidPolish")
        {
            Chambers =
            [
                new("PolishA", new GenericPolishLiquidContentViewModel(WorkstationType.PolishA)),
                new("PolishB", new GenericPolishLiquidContentViewModel(WorkstationType.PolishB)),
                new("PolishC", new GenericPolishLiquidContentViewModel(WorkstationType.PolishC)),
                new("PolishD", new GenericPolishLiquidContentViewModel(WorkstationType.PolishD))
            ]
        });
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
        Tabs.Add(new LiquidCategoryViewModel("LiquidPen")
        {
            Chambers =
            [
                new("PenA1", new GenericPenLiquidContentViewModel(PenCleanConfigFactory.CreatePenA1Config())),
                new("PenB1", new GenericPenLiquidContentViewModel(PenCleanConfigFactory.CreatePenB1Config()))
            ]
        });
        Tabs.Add(new LiquidCategoryViewModel("LiquidDry")
        {
            Chambers =
            [
                new("DryA", new GenericDryLiquidContentViewModel(DryCleanConfigFactory.CreateDryAConfig())),
                new("DryB", new GenericDryLiquidContentViewModel(DryCleanConfigFactory.CreateDryBConfig()))
            ]
        });
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
