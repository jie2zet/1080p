using CMP.ViewModels.Common;
using CMP.ViewModels.Tabs.OperationTab.RobotControlTab.CleanRobot;
using CMP.ViewModels.Tabs.OperationTab.RobotControlTab.PolishBArm;
using System.Collections.ObjectModel;
using WPFLocalizeExtension.Engine;

namespace CMP.ViewModels.Tabs.OperationTab.RobotControlTab;

/// <summary>
/// 机器人控制 Tab 主 ViewModel，包含多个机器人子 Tab
/// </summary>
public class RobotControlTabViewModel : BindableBase
{
    private ObservableCollection<TabContentViewModelBase> _robotTabs;
    public ObservableCollection<TabContentViewModelBase> RobotTabs
    {
        get => _robotTabs;
        set => SetProperty(ref _robotTabs, value);
    }

    private int _selectedTabIndex;
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetProperty(ref _selectedTabIndex, value);
    }

    public RobotControlTabViewModel()
    {
        InitializeRobotTabs();
        LocalizeDictionary.Instance.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(LocalizeDictionary.Instance.Culture))
            {
                UpdateTabTitles();
            }
        };
    }

    private void InitializeRobotTabs()
    {
        RobotTabs = new ObservableCollection<TabContentViewModelBase>
        {
            new FeedingRobotControlViewModel { TabTitleKey = "FeedingRobot" },
            new CacheBodyViewModel { TabTitleKey = "CacheBody" },
            new TransferModuleControlViewModel { TabTitleKey = "TransferModule" },
            new TransferRobotControlViewModel { TabTitleKey = "TransferRobot" },
            new PolishASideTransferControlViewModel { TabTitleKey = "PolishASideTransfer" },
            new PolishASideArm1ControlViewModel { TabTitleKey = "PolishASideArm1" },
            new PolishASideArm2ControlViewModel { TabTitleKey = "PolishASideArm2" },
            new PolishASideArm3ControlViewModel { TabTitleKey = "PolishASideArm3" },
            new PolishBSideTransferControlViewModel { TabTitleKey = "PolishBSideTransfer" },
            new PolishBSideArm1ViewModel { TabTitleKey = "PolishBSideArm1" },
            new PolishBSideArm2ViewModel { TabTitleKey = "PolishBSideArm2" },
            new PolishBSideArm3ViewModel { TabTitleKey = "PolishBSideArm3" },
            new CleanASideRobot1ViewModel { TabTitleKey = "CleanASideRobot1" },
            new CleanASideRobot2ViewModel { TabTitleKey = "CleanASideRobot2" },
            new CleanBSideRobot1ViewModel { TabTitleKey = "CleanBSideRobot1" },
            new CleanBSideRobot2ViewModel { TabTitleKey = "CleanBSideRobot2" },
            new WaferTransferInformationViewModel { TabTitleKey = "WaferTransferInformationMap" }
        };

        SelectedTabIndex = 0;
    }

    private void UpdateTabTitles()
    {
        foreach (var tab in RobotTabs)
        {
            switch (tab)
            {
                case FeedingRobotControlViewModel feedingVm when !string.IsNullOrEmpty(feedingVm.TabTitleKey):
                    feedingVm.RefreshTabTitle();
                    break;
                case TransferModuleControlViewModel transferVm when !string.IsNullOrEmpty(transferVm.TabTitleKey):
                    transferVm.RefreshTabTitle();
                    break;
                case TransferRobotControlViewModel transferVm when !string.IsNullOrEmpty(transferVm.TabTitleKey):
                    transferVm.RefreshTabTitle();
                    break;
                case PolishASideTransferControlViewModel polishVm when !string.IsNullOrEmpty(polishVm.TabTitleKey):
                    polishVm.RefreshTabTitle();
                    break;
                case PolishASideArm1ControlViewModel polishArm1Vm when !string.IsNullOrEmpty(polishArm1Vm.TabTitleKey):
                    polishArm1Vm.RefreshTabTitle();
                    break;
                case PolishASideArm2ControlViewModel polishArm2Vm when !string.IsNullOrEmpty(polishArm2Vm.TabTitleKey):
                    polishArm2Vm.RefreshTabTitle();
                    break;
                case PolishASideArm3ControlViewModel polishArm3Vm when !string.IsNullOrEmpty(polishArm3Vm.TabTitleKey):
                    polishArm3Vm.RefreshTabTitle();
                    break;
                case PolishBSideTransferControlViewModel polishBSideTransferVm when !string.IsNullOrEmpty(polishBSideTransferVm.TabTitleKey):
                    polishBSideTransferVm.RefreshTabTitle();
                    break;
            }
        }
    }
}
