using CMP.Extensions;
using CMP.Models.Operation.LoadPort;
using CMP.Services.Log;
using CMP.Services.LoadPort;
using Prism.Events;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace CMP.ViewModels.Tabs.OperationTab;

public class LPControlStatus
{
    public string ImgPath { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class MainPageViewModel : BindableBase
{
    private ObservableCollection<LoadPortData> _allLoadPorts = new();

    public ObservableCollection<LoadPortData> AllLoadPorts
    {
        get => _allLoadPorts;
        set => SetProperty(ref _allLoadPorts, value);
    }

    private readonly List<LPControlStatus> _statusList =
    [
        new LPControlStatus { ImgPath = "/Assets/Images/blue.png", Status = "Present" },
        new LPControlStatus { ImgPath = "/Assets/Images/blue.png", Status = "Placed" },
        new LPControlStatus { ImgPath = "/Assets/Images/gray.png", Status = "Clamped" },
        new LPControlStatus { ImgPath = "/Assets/Images/blue.png", Status = "ID verified" },
        new LPControlStatus { ImgPath = "/Assets/Images/blue.png", Status = "Docked" },
    ];

    public List<LPControlStatus> StatusList => _statusList;

    private string _imgPath = string.Empty;

    public string ImgPath
    {
        get => _imgPath;
        set => SetProperty(ref _imgPath, value);
    }

    private string _status = string.Empty;

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public MainPageViewModel(ILoadPortService loadPortService, IEventAggregator eventAggregator)
    {
        eventAggregator.GetEvent<LoadPortsReadyEvent>()
            .Subscribe(RefreshLoadPorts, ThreadOption.UIThread);
        RefreshLoadPorts();
    }

    private void RefreshLoadPorts()
    {
        var loadPortDict = LoadPortManager.Instance.GetAllLoadPorts();
        if (loadPortDict == null || loadPortDict.Count == 0)
        {
            Logger.Info("MainPageViewModel: LoadPort 数据尚未就绪");
            return;
        }

        AllLoadPorts = new ObservableCollection<LoadPortData>(loadPortDict.Values);
        Logger.Info($"MainPageViewModel: 已加载 {AllLoadPorts.Count} 个 LoadPort");
    }
}
