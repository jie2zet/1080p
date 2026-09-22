using System.Collections.ObjectModel;
using CMP.Services.LoadPort;
using Prism.Mvvm;

namespace CMP.Models.Operation.LoadPort;

public enum RfidReadStatus
{
    Idle,
    Reading,
    Ok,
    Failed
}

public class LoadPortControlLamp : BindableBase
{
    private string _imgPath = "/Assets/Images/gray.png";

    public LoadPortControlLamp(string status)
    {
        Status = status;
    }

    public string Status { get; }

    public string ImgPath
    {
        get => _imgPath;
        set => SetProperty(ref _imgPath, value);
    }
}

public partial class LoadPortData
{
    private const string LampOn = "/Assets/Images/blue.png";
    private const string LampOff = "/Assets/Images/gray.png";

    private string _carrierId = string.Empty;
    private RfidReadStatus _rfidReadStatus = RfidReadStatus.Idle;

    public ObservableCollection<LoadPortControlLamp> ControlStatusItems { get; } = new()
    {
        new("Present"),
        new("Placed"),
        new("Clamped"),
        new("ID verified"),
        new("Docked")
    };

    public string CarrierId
    {
        get => _carrierId;
        set
        {
            if (SetProperty(ref _carrierId, value ?? string.Empty))
            {
                RaisePropertyChanged(nameof(CarrierIdDisplay));
                RefreshControlStatusItems();
                PublishCarrierIdChanged();
            }
        }
    }

    public string CarrierIdDisplay =>
        string.IsNullOrWhiteSpace(CarrierId) ? "--" : CarrierId;

    public RfidReadStatus RfidReadStatus
    {
        get => _rfidReadStatus;
        set => SetProperty(ref _rfidReadStatus, value);
    }

    public bool IsFoupPresent =>
        LoadPortStatus != null && LoadPortStatus.foupPlacementStatus != FoupPlacementStatus.NoExistence;

    public bool IsFoupPlaced =>
        LoadPortStatus != null && LoadPortStatus.foupPlacementStatus == FoupPlacementStatus.Normal;

    internal void RefreshControlStatusItems()
    {
        void Apply()
        {
            var status = LoadPortStatus;
            SetLamp(0, status != null && status.foupPlacementStatus != FoupPlacementStatus.NoExistence);
            SetLamp(1, status != null && status.foupPlacementStatus == FoupPlacementStatus.Normal);
            SetLamp(2, status != null && status.FoupClampStatus == FoupClampStatus.Clamp);
            SetLamp(3, !string.IsNullOrWhiteSpace(CarrierId) && _rfidReadStatus == RfidReadStatus.Ok);
            SetLamp(4, status != null && status.FoupDockStatus == FoupDockStatus.Docking);
        }

        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.CheckAccess())
        {
            Apply();
        }
        else
        {
            dispatcher.BeginInvoke(Apply);
        }
    }

    private void SetLamp(int index, bool on)
    {
        if (index < 0 || index >= ControlStatusItems.Count)
        {
            return;
        }

        ControlStatusItems[index].ImgPath = on ? LampOn : LampOff;
    }
}
