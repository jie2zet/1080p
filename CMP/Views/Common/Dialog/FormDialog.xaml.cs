using System.Windows;
using System.Windows.Controls;

namespace CMP.Views.Common.Dialog
{
    /// <summary>
    /// FormDialog.xaml 的交互逻辑
    /// </summary>
    public partial class FormDialog:UserControl
    {
        public FormDialog(IRegionManager regionManager)
        {
            InitializeComponent();

            if (regionManager.Regions.ContainsRegionWithName("RegionForm"))
            {
                var oldRegion = regionManager.Regions["RegionForm"];
                
                // 或者完全移除
                regionManager.Regions.Remove("RegionForm");
            }

           
           

            RegionManager.SetRegionManager(this, regionManager);
            RegionManager.UpdateRegions();
            this.Unloaded += FormDialog_Unloaded;
            this.Loaded += (s, e) =>
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.ResizeMode = ResizeMode.NoResize;
                }
            };
        }

        private void FormDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            var vm = RegionManager.GetRegionManager(this);
            vm.Regions.Remove("RegionForm");
        }
    }
}
