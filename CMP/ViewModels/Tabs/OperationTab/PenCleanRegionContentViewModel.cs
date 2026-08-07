using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CMP.Lang;
using CMP.Models.Operation.Clean;
using CMP.ViewModels.Common;

namespace CMP.ViewModels.Tabs.OperationTab;

public class PenCleanRegionContentViewModel : BindableBase
{
    private readonly IEventAggregator _eventAggregator;

    private ObservableCollection<TabContentViewModelBase> _penCleanTabItems = [];
    public ObservableCollection<TabContentViewModelBase> PenCleanTabItems
    {
        get => _penCleanTabItems;
        set => SetProperty(ref _penCleanTabItems, value);
    }

    private int _selectedIndex;
    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetProperty(ref _selectedIndex, value);
    }

    public PenCleanRegionContentViewModel(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

        _ = Task.Run(() =>
        {
            var items = new ObservableCollection<TabContentViewModelBase>
            {
                new PenCleanTabContentViewModel(0, $"{Resources.PenClean} A1", PenCleanConfigFactory.CreatePenA1Config(), _eventAggregator),
                new PenCleanTabContentViewModel(1, $"{Resources.PenClean} A2", PenCleanConfigFactory.CreatePenA2Config(), _eventAggregator),
                new PenCleanTabContentViewModel(2, $"{Resources.PenClean} B1", PenCleanConfigFactory.CreatePenB1Config(), _eventAggregator),
                new PenCleanTabContentViewModel(3, $"{Resources.PenClean} B2", PenCleanConfigFactory.CreatePenB2Config(), _eventAggregator),
            };
            Application.Current?.Dispatcher.BeginInvoke(DispatcherPriority.Background, () => PenCleanTabItems = items);
        });
    }
}
