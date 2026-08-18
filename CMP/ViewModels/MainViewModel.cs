using CMP.Extensions;
using CMP.Models.Operation.LoadPort;
using CMP.Models.Resource;
using CMP.Services;
using CMP.Services.Auth;
using CMP.Services.LoadPort;
using CMP.Services.Log;
using CMP.ViewModels.Login;
using CMP.Views;
using Prism.Mvvm;

namespace CMP.ViewModels;

public class MainViewModel : BindableBase
{
    private readonly IRegionManager _regionManager;
    private readonly ILoadPortService _loadPortService;
    private readonly NavigationGate _navigationGate;
    private readonly IEventAggregator _eventAggregator;
    private string _title = "Chemical mechanical polishing";
    private bool _isInitialized;
    private bool _isLoginOverlayVisible = true;

    public MainViewModel(
        IRegionManager regionManager,
        ILoadPortService loadPortService,
        LoginViewModel loginViewModel,
        NavigationGate navigationGate,
        IEventAggregator eventAggregator)
    {
        _regionManager = regionManager;
        _loadPortService = loadPortService;
        _navigationGate = navigationGate;
        _eventAggregator = eventAggregator;
        Login = loginViewModel;

        Login.LoginSucceeded += OnLoginSucceeded;
        SessionContext.Instance.PropertyChanged += OnSessionContextChanged;
        _isLoginOverlayVisible = !SessionContext.Instance.IsAuthenticated;

        _regionManager.RegisterViewWithRegion("HeaderRegion", typeof(HeaderView));
        _regionManager.RegisterViewWithRegion("ContentRegion", typeof(ContentView));
    }

    public LoginViewModel Login { get; }

    public bool IsLoginOverlayVisible
    {
        get => _isLoginOverlayVisible;
        set => SetProperty(ref _isLoginOverlayVisible, value);
    }

    public event Action? UserAuthenticated;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            _eventAggregator.GetEvent<LoadPortsReadyEvent>().Publish();
            return;
        }

        try
        {
            await LoadPortManager.Instance.InitializeAsync(_loadPortService);
            WaferRuntimeStore.Instance.RestoreAll();
            await LoadPortManager.Instance.ConnectAllLoadPortsAsync();
            LoadPortManager.Instance.AllSelectedWafersCompleted += OnAllSelectedWafersCompleted;
            LoadPortManager.Instance.StartWaferMonitoring();
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to initialize LoadPorts: {ex.Message}", ex, "loadPort.txt", 0);
        }
        finally
        {
            _eventAggregator.GetEvent<LoadPortsReadyEvent>().Publish();
        }
    }

    private void OnLoginSucceeded()
    {
        IsLoginOverlayVisible = false;
        _navigationGate.RefreshTabVisibility();
        UserAuthenticated?.Invoke();
    }

    private void OnSessionContextChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SessionContext.IsAuthenticated))
        {
            return;
        }

        IsLoginOverlayVisible = !SessionContext.Instance.IsAuthenticated;
        if (!SessionContext.Instance.IsAuthenticated)
        {
            Login.Reset();
        }

        _navigationGate.RefreshTabVisibility();
    }

    private void OnAllSelectedWafersCompleted(LoadPortData loadPort, List<Wafer> completedWafers)
    {
        Logger.Info($"LoadPort {loadPort.PortNumber} 上的 {completedWafers.Count} 个选中 wafer 都已完成，执行 unload 操作");
        ExecuteUnloadOperation(loadPort, completedWafers);
    }

    private void ExecuteUnloadOperation(LoadPortData loadPort, List<Wafer> wafers)
    {
        Logger.Info($"正在 unload Slot {loadPort.PortNumber}");
        loadPort.MOVCommand.Execute("CULOD");
    }
}
