using CMP.Views.Tabs.WarnTab;

namespace CMP.ViewModels.Tabs.MainTab.Content;

public class WarnContentViewModel : BindableBase
{
    public WarnContentViewModel(IRegionManager regionManager)
    {
        regionManager.RegisterViewWithRegion("AlarmTabRegion", typeof(AlarmTabContentView));
        regionManager.RegisterViewWithRegion("ConsumableTabRegion", typeof(ConsumableTabContentView));
    }
}