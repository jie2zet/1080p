using CMP.Views.Tabs.ConfigTab;
using Core.Common.RegionNames;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.ViewModels.Tabs.MainTab.Content
{
    public class ConfigurationContentViewModel : BindableBase
    {
        public ConfigurationContentViewModel(IRegionManager regionManager)
        {
            regionManager.RegisterViewWithRegion(Global.ConfigRegion, typeof(ConfigView));
            regionManager.RegisterViewWithRegion(Global.CalibrationTabRegion, typeof(CalibrationConfigTabView));
        }
    }
}
