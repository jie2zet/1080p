using CMP.ViewModels.Tabs.OperationTab;
using CMP.Views.Tabs.OperationTab;
using System.Collections.ObjectModel;
using CMP.Lang;
using CMP.ViewModels.Common;
using System.Windows;
using System.Windows.Threading;
using CMP.Models.Operation.Polish;

namespace CMP.ViewModels.Tabs.MainTab.Content;


public class OperationContentViewModel:BindableBase
{
    private ObservableCollection<TabContentViewModelBase> _polishUnitTabItems = [];
    public ObservableCollection<TabContentViewModelBase> PolishUnitTabItems
    {
        get => _polishUnitTabItems;
        set => SetProperty(ref _polishUnitTabItems, value);
    }

    private int _selectedIndex;
    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetProperty(ref _selectedIndex, value);
    }

    public OperationContentViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
    {
        Task.Run(() =>
        {
            ObservableCollection<TabContentViewModelBase> tabContentViewModelBaseList = new ObservableCollection<TabContentViewModelBase>();
            var plishUnitA = new PolishUnitTabContentViewModel(0, GlobalService.kvHostLinkInit.kvLinkPolishA, $"{Resources.Polish} A", eventAggregator);
            var plishUnitB = new PolishUnitTabContentViewModel(1, GlobalService.kvHostLinkInit.kvLinkPolishB, $"{Resources.Polish} B", eventAggregator);
            var plishUnitC = new PolishUnitTabContentViewModel(2, GlobalService.kvHostLinkInit.kvLinkPolishC, $"{Resources.Polish} C", eventAggregator);
            var plishUnitD = new PolishUnitTabContentViewModel(3, GlobalService.kvHostLinkInit.kvLinkPolishD, $"{Resources.Polish} D", eventAggregator);
            tabContentViewModelBaseList.Add(plishUnitA);
            tabContentViewModelBaseList.Add(plishUnitB);
            tabContentViewModelBaseList.Add(plishUnitC);
            tabContentViewModelBaseList.Add(plishUnitD);
            tabContentViewModelBaseList.Add(new BotTabContentViewModel(0, "Bot A"));
            tabContentViewModelBaseList.Add(new BotTabContentViewModel(10, "Bot B", 10));
            tabContentViewModelBaseList.Add(new RoboticArmTabContentViewModel(5, "Robotic Arm"));
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
            {
                PolishUnitTabItems = tabContentViewModelBaseList;
            });
        });
        regionManager.RegisterViewWithRegion("RollerRegion", typeof(RollerRegionContentView));
        regionManager.RegisterViewWithRegion("PenCleanRegion", typeof(PenCleanRegionContentView));
        regionManager.RegisterViewWithRegion("DryRegion", typeof(DryRegionContentView));
        regionManager.RegisterViewWithRegion("LoadPortRegion", typeof(LoadPortContentView));
        regionManager.RegisterViewWithRegion("SerialPortRegion", typeof(SerialPortContentView));
        //regionManager.RegisterViewWithRegion("RollerRegion", typeof(RollerRegionContentView));
    }
}