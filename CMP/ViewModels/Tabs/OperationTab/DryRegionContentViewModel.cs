using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CMP.Lang;
using CMP.Models.Operation.Clean;
using CMP.ViewModels.Common;

namespace CMP.ViewModels.Tabs.OperationTab;

public class DryRegionContentViewModel : BindableBase
{
    private readonly IEventAggregator _eventAggregator;

    private ObservableCollection<TabContentViewModelBase> _dryTabItems = [];
    public ObservableCollection<TabContentViewModelBase> DryTabItems
    {
        get => _dryTabItems;
        set => SetProperty(ref _dryTabItems, value);
    }

    private int _selectedIndex;
    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetProperty(ref _selectedIndex, value);
    }

    public DryRegionContentViewModel(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

        _ = Task.Run(() =>
        {
            var items = new ObservableCollection<TabContentViewModelBase>
            {
                new DryTabContentViewModel(0, $"{Resources.Dry} A", DryCleanConfigFactory.CreateDryAConfig(), _eventAggregator),
                new DryTabContentViewModel(1, $"{Resources.Dry} B", DryCleanConfigFactory.CreateDryBConfig(), _eventAggregator),
            };
            Application.Current?.Dispatcher.BeginInvoke(DispatcherPriority.Background, () => DryTabItems = items);
        });
    }
}
