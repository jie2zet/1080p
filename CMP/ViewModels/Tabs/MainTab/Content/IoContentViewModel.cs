using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using CMP.Models.DataCenter;
using CMP.Models.kvsPlc;
using CMP.Models.Resource;
using CMP.Services.Io;
using CMP.Services.Log;
using Prism.Commands;
using Prism.Mvvm;
using HandyControl.Controls;

namespace CMP.ViewModels.Tabs.MainTab.Content;

public sealed class IoContentViewModel : BindableBase
{
    private readonly IoDataWriteService _writeService = new();
    private readonly CancellationTokenSource _stop = new();

    public ObservableCollection<IoGroupViewModel> Polish { get; } = [];
    public ObservableCollection<IoGroupViewModel> Roller { get; } = [];
    public ObservableCollection<IoGroupViewModel> Pen { get; } = [];
    public ObservableCollection<IoGroupViewModel> Dry { get; } = [];
    public ObservableCollection<IoCategoryViewModel> Tabs { get; } = [];

    public IoContentViewModel()
    {
        var dataCenter = GlobalService.DataCenter;
        BuildGroups(Polish, dataCenter.listPolish, string.Empty, "Polish");
        foreach (var chamber in dataCenter.listRoller)
        {
            for (var station = 1; station <= 3; station++)
                AddGroup(Roller, chamber, $"_{station}", $"Roller {chamber.Name["Roller".Length..]}{station}");
        }
        foreach (var chamber in dataCenter.listPen)
            AddGroup(Pen, chamber, "_1", $"{chamber.Name} 1");
        BuildGroups(Dry, dataCenter.listDry, string.Empty, "Dry");
        Tabs.Add(new IoCategoryViewModel("Polish", Polish));
        Tabs.Add(new IoCategoryViewModel("Roller", Roller));
        Tabs.Add(new IoCategoryViewModel("Pen", Pen));
        Tabs.Add(new IoCategoryViewModel("Dry", Dry));

        _ = Task.Run(PollAsync);
    }

    private static void BuildGroups(
        ObservableCollection<IoGroupViewModel> target,
        IEnumerable<CollectDataChamber> chambers,
        string stationPrefix,
        string fallbackPrefix)
    {
        foreach (var chamber in chambers)
            AddGroup(target, chamber, stationPrefix, chamber.Name.Replace(fallbackPrefix, fallbackPrefix + " "));
    }

    private static void AddGroup(ObservableCollection<IoGroupViewModel> target, CollectDataChamber chamber, string stationPrefix, string title)
    {
        var metadataIndex = DataLogFormatting.BuildMetadataIndex(chamber.ListSV.Values);
        var metadata = chamber.ListSV.Values
            .Where(sv => string.Equals(sv.Descripition?.Trim(), "DI", StringComparison.OrdinalIgnoreCase)
                      || string.Equals(sv.Descripition?.Trim(), "DO", StringComparison.OrdinalIgnoreCase))
            .GroupBy(sv => DataLogFormatting.NormalizeSvKey(sv.Name), StringComparer.Ordinal)
            .Select(group => group.First())
            .Where(sv => IsForStation(chamber, stationPrefix, sv.Name))
            .Select(sv => CreateItem(chamber, stationPrefix, sv, metadataIndex))
            .Where(item => item != null)
            .Cast<IoItemViewModel>()
            .ToList();

        var group = new IoGroupViewModel(title, metadata);
        target.Add(group);
    }

    private static bool IsForStation(CollectDataChamber chamber, string stationPrefix, string name)
    {
        // Polish CSV rows are already scoped to one chamber and omit the
        // chamber prefix (for example "Platen Breakwater ...").
        // Dry rows include "DryA/DryB"; Roller/Pen are additionally scoped
        // by the station prefix, so only those need a prefix check here.
        if (stationPrefix.Length == 0)
        {
            return true;
        }

        var normalized = DataLogFormatting.NormalizeSvKey(name);
        var prefix = DataLogFormatting.NormalizeSvKey(chamber.Name);
        if (!normalized.StartsWith(prefix, StringComparison.Ordinal)) return false;
        var station = stationPrefix.TrimStart('_');
        return normalized.StartsWith(prefix + station, StringComparison.Ordinal);
    }

    private static IoItemViewModel? CreateItem(
        CollectDataChamber chamber,
        string stationPrefix,
        SV metadata,
        IReadOnlyDictionary<string, SV> metadataIndex)
    {
        string? fieldName = null;
        foreach (var field in chamber.ioDataFields.Where(f => f.FieldType == typeof(bool)))
        {
            var runtimeKey = chamber.BuildRuntimeKey(stationPrefix, field.Name);
            var resolved = DataLogFormatting.ResolveSvMetadata(
                metadataIndex, runtimeKey, chamber.Name);
            if (resolved != null && DataLogFormatting.NormalizeSvKey(resolved.Name) == DataLogFormatting.NormalizeSvKey(metadata.Name))
            {
                fieldName = field.Name;
                break;
            }
        }

        return new IoItemViewModel(chamber, stationPrefix, metadata, fieldName);
    }

    private async Task PollAsync()
    {
        while (!_stop.IsCancellationRequested)
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var group in Polish.Concat(Roller).Concat(Pen).Concat(Dry))
                        foreach (var item in group.Items)
                            item.Refresh();
                }, DispatcherPriority.Background);
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                Logger.Error("IO view refresh failed.", ex);
            }
            await Task.Delay(200, _stop.Token);
        }
    }
}

public sealed class IoCategoryViewModel
{
    public string Header { get; }
    public ObservableCollection<IoGroupViewModel> Groups { get; }

    public IoCategoryViewModel(string header, ObservableCollection<IoGroupViewModel> groups)
    {
        Header = header;
        Groups = groups;
    }
}

public sealed class IoGroupViewModel : BindableBase
{
    public string Title { get; }
    public ObservableCollection<IoItemViewModel> Items { get; }
    public IEnumerable<IoItemViewModel> DiItems => Items.Where(item => !item.IsOutput);
    public IEnumerable<IoItemViewModel> DoItems => Items.Where(item => item.IsOutput);

    public IoGroupViewModel(string title, IEnumerable<IoItemViewModel> items)
    {
        Title = title;
        Items = new ObservableCollection<IoItemViewModel>(items);
    }
}

public sealed class IoItemViewModel : BindableBase
{
    private readonly CollectDataChamber _chamber;
    private readonly IoDataWriteService _writeService = new();
    private readonly string _stationPrefix;
    private readonly string? _fieldName;
    private bool _value;

    public string Name { get; }
    public string Description { get; }
    public bool IsOutput { get; }
    public bool Value { get => _value; private set => SetProperty(ref _value, value); }
    public bool CanWrite => IsOutput && _fieldName != null;
    public DelegateCommand TrueCommand { get; }
    public DelegateCommand FalseCommand { get; }

    public IoItemViewModel(CollectDataChamber chamber, string stationPrefix, SV metadata, string? fieldName)
    {
        _chamber = chamber;
        _stationPrefix = stationPrefix;
        _fieldName = fieldName;
        Name = metadata.Name;
        Description = metadata.Descripition;
        IsOutput = string.Equals(metadata.Descripition?.Trim(), "DO", StringComparison.OrdinalIgnoreCase);
        TrueCommand = new DelegateCommand(() => Write(true), () => CanWrite);
        FalseCommand = new DelegateCommand(() => Write(false), () => CanWrite);
    }

    public void Refresh()
    {
        if (_fieldName != null && _chamber.TryGetIoValue(_stationPrefix, _fieldName, out var ioValue))
        {
            Value = ioValue;
            return;
        }

        var runtimeKey = _chamber.BuildRuntimeKey(_stationPrefix, _fieldName ?? Name);
        var normalized = DataLogFormatting.NormalizeSvKey(runtimeKey);
        var sv = _chamber.ListSV.Values.FirstOrDefault(item =>
            DataLogFormatting.NormalizeSvKey(item.Name) == normalized);
        if (sv?.Value is bool boolValue)
            Value = boolValue;
        else if (bool.TryParse(sv?.Value?.ToString(), out var parsed))
            Value = parsed;
    }

    private void Write(bool value)
    {
        if (!CanWrite) return;
        var result = _writeService.Write(_chamber, _stationPrefix, _fieldName!, value);
        if (result == (int)KHLUserError.KHL_ERR_NONE)
            Growl.SuccessGlobal($"{Name}: {(value ? "True" : "False")} written successfully");
        else
            Growl.ErrorGlobal($"{Name}: write failed ({result})");
    }
}
