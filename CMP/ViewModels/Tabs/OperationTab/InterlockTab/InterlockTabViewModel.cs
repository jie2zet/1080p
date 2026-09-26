using CMP.Constants.Plc;
using CMP.Controls;
using CMP.Services.Auth;
using CMP.Services.Log;
using CMP.Services.Plc;
using CMP.ViewModels.Common;
using Prism.Commands;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;

namespace CMP.ViewModels.Tabs.OperationTab.InterlockTab;

public sealed class InterlockSideViewModel
{
    public InterlockSideViewModel(string side, IEnumerable<InterlockItemViewModel> items)
    {
        Side = side;
        EmergencyStops = new ObservableCollection<InterlockItemViewModel>(
            items.Where(item => item.Group == InterlockGroup.EmergencyStop));
        UnlockItems = new ObservableCollection<InterlockItemViewModel>(
            items.Where(item => item.Group == InterlockGroup.Unlock));
        EquipmentStatus = new ObservableCollection<InterlockItemViewModel>(
            items.Where(item => item.Group == InterlockGroup.EquipmentStatus));
    }

    public string Side { get; }
    public ObservableCollection<InterlockItemViewModel> EmergencyStops { get; }
    public ObservableCollection<InterlockItemViewModel> UnlockItems { get; }
    public ObservableCollection<InterlockItemViewModel> EquipmentStatus { get; }
}

public sealed class InterlockItemViewModel : BindableBase
{
    private readonly Action<InterlockItemViewModel> _toggleAction;
    private bool _readState;
    private bool _hasReadState;

    internal InterlockItemViewModel(InterlockDefinition definition, Action<InterlockItemViewModel> toggleAction)
    {
        Definition = definition;
        _toggleAction = toggleAction;
        // The visual enabled state is controlled by IsSwitchEnabled. Keep the
        // command executable so HandyControl's switch style cannot disable the
        // ToggleButton because of a stale CanExecute snapshot.
        ToggleCommand = new DelegateCommand(() => _toggleAction(this));
    }

    public InterlockDefinition Definition { get; }
    public string Key => Definition.Key;
    public string DisplayName => Definition.DisplayName;
    public string Side => Definition.Side;
    public InterlockGroup Group => Definition.Group;
    public SignalLightColor LightColor => Definition.LightColor;
    public bool IsUnlock => Definition.IsUnlock;
    public string? ReadAddress => Definition.ReadAddress;
    public string? WriteAddress => Definition.WriteAddress;

    public bool ReadState
    {
        get => _readState;
        private set
        {
            if (SetProperty(ref _readState, value))
            {
                RaisePropertyChanged(nameof(CurrentLightColor));
            }
        }
    }

    /// <summary>
    /// The lamp is always lit: a PLC 1/true is healthy/green and 0/false is
    /// fault/not-ready/red. This is independent from the unlock switch.
    /// </summary>
    public SignalLightColor CurrentLightColor => ReadState
        ? SignalLightColor.Green
        : SignalLightColor.Red;

    public bool IsReadStateKnown
    {
        get => _hasReadState;
        private set => SetProperty(ref _hasReadState, value);
    }

    public bool IsConfigured => Group == InterlockGroup.Unlock
        ? !string.IsNullOrWhiteSpace(ReadAddress) && !string.IsNullOrWhiteSpace(WriteAddress)
        : !string.IsNullOrWhiteSpace(ReadAddress);

    // The switch is an operator control and is enabled as soon as the
    // interlock item is created. Address validation is handled when the
    // command executes, so an unknown PLC state never greys out the switch.
    public bool IsSwitchEnabled => IsUnlock;

    public DelegateCommand ToggleCommand { get; }

    internal void ApplyReadState(bool value)
    {
        ReadState = value;
        IsReadStateKnown = true;
    }

    internal void RefreshReadStateBinding()
    {
        RaisePropertyChanged(nameof(ReadState));
    }
}

public sealed class InterlockTabViewModel : TabContentViewModelBase
{
    private readonly MitsubishiPlcService _plc;
    private readonly DispatcherTimer _readTimer;
    private readonly HashSet<string> _missingAddressWarnings = [];
    private bool _isReading;

    private string? _tabTitle;
    public new string? TabTitle
    {
        get => _tabTitle ?? base.TabTitle;
        private set => SetProperty(ref _tabTitle, value);
    }

    public InterlockSideViewModel ASide { get; }
    public InterlockSideViewModel BSide { get; }
    public ObservableCollection<InterlockItemViewModel> Items { get; }

    public InterlockTabViewModel()
        : base(0, "Interlock")
    {
        _plc = GlobalService.RobotControlPlc;

        Items = new ObservableCollection<InterlockItemViewModel>(
            InterlockAddressCatalog.Definitions.Select(definition =>
                new InterlockItemViewModel(definition, ExecuteUnlockCommand)));

        ASide = new InterlockSideViewModel("A", Items.Where(item => item.Side == "A"));
        BSide = new InterlockSideViewModel("B", Items.Where(item => item.Side == "B"));

        _readTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _readTimer.Tick += ReadTimer_Tick;
        RegisterLifecycleTimer(_readTimer);
        RegisterViewLifecycleAction(
            () => Logger.For("Interlock").Debug("InterlockTabViewModel activated, started PLC read timer."),
            () => Logger.For("Interlock").Debug("InterlockTabViewModel deactivated, stopped PLC read timer."));

        UpdateTabTitle();
        LocalizeDictionary.Instance.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(LocalizeDictionary.Instance.Culture))
            {
                UpdateTabTitle();
            }
        };
    }

    private void UpdateTabTitle()
    {
        var localization = new LocExtension("CMP:Resources:Interlock");
        localization.ResolveLocalizedValue(out string? localizedValue);
        TabTitle = localizedValue ?? "Interlock";
    }

    private async void ReadTimer_Tick(object? sender, EventArgs e)
    {
        if (_isReading)
        {
            return;
        }

        await ReadPlcDataOnceAsync();
    }

    public override void OnViewActivated()
    {
        base.OnViewActivated();

        // Do not wait for the first timer tick. This also makes activation
        // observable when the PLC is disconnected or running in debug mode.
        _ = ReadPlcDataOnceAsync();
    }

    private async Task ReadPlcDataOnceAsync()
    {
        if (_isReading)
        {
            return;
        }

        _isReading = true;
        try
        {
            await ReadPlcDataAsync();
        }
        finally
        {
            _isReading = false;
        }
    }

    private async Task ReadPlcDataAsync()
    {
        foreach (var item in Items.Where(item => item.IsConfigured))
        {
            var address = item.ReadAddress;
            if (string.IsNullOrWhiteSpace(address))
            {
                continue;
            }

            try
            {
                var result = await _plc.ReadAsync<bool>(NormalizeAddress(address));
                if (result.IsSuccess)
                {
                    item.ApplyReadState(result.Content);
                }
                else
                {
                    Logger.For("Interlock").Warn($"[{item.Key}] PLC read failed: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.For("Interlock").Error($"[{item.Key}] PLC read failed.", ex);
            }
        }
    }

    private void ExecuteUnlockCommand(InterlockItemViewModel item)
    {
        // Mitsubishi writes are synchronous in the current service. Keep the
        // command on the UI thread only long enough to schedule the write.
        _ = ExecuteUnlockCommandAsync(item);
    }

    private async Task ExecuteUnlockCommandAsync(InterlockItemViewModel item)
    {
        if (string.IsNullOrWhiteSpace(item.WriteAddress))
        {
            WarnMissingAddress(item, "write");
            item.RefreshReadStateBinding();
            return;
        }

        var targetState = !item.ReadState;
        try
        {
            var result = await Task.Run(() =>
                _plc.WriteBoolVari(NormalizeAddress(item.WriteAddress), targetState));
            if (!result.IsSuccess)
            {
                Logger.For("Interlock").Warn($"[{item.Key}] PLC write failed: {result.Message}");
                item.RefreshReadStateBinding();
            }
        }
        catch (Exception ex)
        {
            Logger.For("Interlock").Error($"[{item.Key}] PLC write failed.", ex);
            item.RefreshReadStateBinding();
        }
    }

    private void WarnMissingAddress(InterlockItemViewModel item, string direction)
    {
        var warningKey = $"{item.Key}:{direction}";
        if (_missingAddressWarnings.Add(warningKey))
        {
            Logger.For("Interlock").Warn($"[{item.Key}] PLC {direction} address is not configured.");
        }
    }

    private static string NormalizeAddress(string address)
    {
        return address.Replace("-", string.Empty, StringComparison.Ordinal).Trim();
    }
}
