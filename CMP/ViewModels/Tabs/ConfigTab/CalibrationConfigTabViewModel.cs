using CMP.Views.Tabs.ConfigTab;
using Core.Common.RegionNames;
using Prism.Navigation.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.ViewModels.Tabs.ConfigTab
{
    public class CalibrationConfigTabViewModel : BindableBase
    {        
        public DelegateCommand<string> SelectionChangedCommand { get; set; }

        private IEventAggregator eventAggregator;

        private IRegionManager regionManager;

        public CalibrationConfigTabViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.regionManager = regionManager;
            regionManager.RegisterViewWithRegion(Global.CalibrationRegionPolishA, typeof(CalibrationView));
            regionManager.RegisterViewWithRegion(Global.CalibrationRegionPolishB, typeof(CalibrationView));
            regionManager.RegisterViewWithRegion(Global.CalibrationRegionPolishC, typeof(CalibrationView));
            regionManager.RegisterViewWithRegion(Global.CalibrationRegionPolishD, typeof(CalibrationView));
            regionManager.RegisterViewWithRegion(Global.CalibrationRegionRollerA, typeof(CalibrationView));
            regionManager.RegisterViewWithRegion(Global.CalibrationRegionRollerB, typeof(CalibrationView));
            regionManager.RegisterViewWithRegion(Global.CalibrationRegionPenA, typeof(CalibrationView));
            regionManager.RegisterViewWithRegion(Global.CalibrationRegionPenB, typeof(CalibrationView));
            SelectionChangedCommand = new DelegateCommand<string>(OnSelectionChanged);
        }

        private void OnSelectionChanged(string selectedTabHeader)
        {
            string regionName = GetRegionNameFromTabHeader(selectedTabHeader);
            if (!string.IsNullOrEmpty(regionName))
            {
                var parameters = new NavigationParameters();
                parameters.Add("PolishIdentifier", selectedTabHeader);
                regionManager.RequestNavigate(regionName, new Uri("CalibrationView", UriKind.Relative), parameters);
            }
        }

        private string GetRegionNameFromTabHeader(string tabHeader)
        {
            switch (tabHeader)
            {
                case "PolishA":
                    return Global.CalibrationRegionPolishA;
                case "PolishB":
                    return Global.CalibrationRegionPolishB;
                case "PolishC":
                    return Global.CalibrationRegionPolishC;
                case "PolishD":
                    return Global.CalibrationRegionPolishD;
                case "RollerA":
                    return Global.CalibrationRegionRollerA;
                case "RollerB":
                    return Global.CalibrationRegionRollerB;
                case "PenA":
                    return Global.CalibrationRegionPenA;
                case "PenB":
                    return Global.CalibrationRegionPenB;
                default:
                    return "";
            }
        }
    }
}
