using CMP.Services.Auth;
using CMP.Views.Tabs.MainTab;
using CMP.Views.Tabs.MainTab.Content;
using CMP.Views.Tabs.OperationTab.RobotControlTab;

namespace CMP.Modules;

public class MainModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        containerProvider.Resolve<NavigationGate>();
        var regionManager = containerProvider.Resolve<IRegionManager>();

        regionManager.RegisterViewWithRegion("MainTabRegion", typeof(MainTabView));
        regionManager.RegisterViewWithRegion("MainTabRegion", typeof(OperationTabView));
        regionManager.RegisterViewWithRegion("MainTabRegion", typeof(RecipesTabView));
        regionManager.RegisterViewWithRegion("MainTabRegion", typeof(ConfigurationTabView));
        regionManager.RegisterViewWithRegion("MainTabRegion", typeof(SettingTabView));
        regionManager.RegisterViewWithRegion("MainTabRegion", typeof(WarnTabView));
        regionManager.RegisterViewWithRegion("MainTabRegion", typeof(UserManageView));

        regionManager.RegisterViewWithRegion("MainRegion", typeof(CMP.Views.Tabs.OperationTab.MainContentView));
        regionManager.RegisterViewWithRegion("OperationRegion", typeof(OperationContentView));
        regionManager.RegisterViewWithRegion("RecipesRegion", typeof(RecipesContentView));
        regionManager.RegisterViewWithRegion("ConfigurationRegion", typeof(ConfigurationContentView));
        regionManager.RegisterViewWithRegion("SettingRegion", typeof(SettingContentView));
        regionManager.RegisterViewWithRegion("WarnRegion", typeof(WarnContentView));
        regionManager.RegisterViewWithRegion("UserRegion", typeof(UserManageContentView));
        regionManager.RegisterViewWithRegion("RobotControlRegion", typeof(RobotControlTabView));
    }
}
