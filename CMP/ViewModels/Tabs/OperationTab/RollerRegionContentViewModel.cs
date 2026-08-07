using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CMP.Models.Operation.Roller;
using CMP.ViewModels.Common;

namespace CMP.ViewModels.Tabs.OperationTab;

public class RollerRegionContentViewModel : BindableBase
{
    private readonly IEventAggregator _eventAggregator;

    private ObservableCollection<TabContentViewModelBase> _rollerTabItems = [];
    public ObservableCollection<TabContentViewModelBase> RollerTabItems
    {
        get => _rollerTabItems;
        set => SetProperty(ref _rollerTabItems, value);
    }

    private int _selectedIndex;
    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetProperty(ref _selectedIndex, value);
    }

    public RollerRegionContentViewModel(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

        // 子 VM 构造内含 KHL 同步调用，放到后台线程避免首次点开 Roller 页卡死 UI
        _ = Task.Run(() =>
        {
            var items = new ObservableCollection<TabContentViewModelBase>
            {
                new GenericRollerTabContentViewModel(0, "Roller A1", RollerConfigFactory.CreateRollA1Config(), _eventAggregator),
                new GenericRollerTabContentViewModel(1, "Roller A2", RollerConfigFactory.CreateRollA2Config(), _eventAggregator),
                new GenericRollerTabContentViewModel(2, "Roller A3", RollerConfigFactory.CreateRollA3Config(), _eventAggregator),
                new GenericRollerTabContentViewModel(3, "Roller B1", RollerConfigFactory.CreateRollB1Config(), _eventAggregator),
                new GenericRollerTabContentViewModel(4, "Roller B2", RollerConfigFactory.CreateRollB2Config(), _eventAggregator),
                new GenericRollerTabContentViewModel(5, "Roller B3", RollerConfigFactory.CreateRollB3Config(), _eventAggregator),
            };
            Application.Current?.Dispatcher.BeginInvoke(DispatcherPriority.Background, () => RollerTabItems = items);
        });
    }
}
