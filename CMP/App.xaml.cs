using CMP.Adapters;
using CMP.Extensions;
using CMP.Modules;
using CMP.Services;
using CMP.Services.Auth;
using CMP.Services.HostCommunication;
using CMP.Services.LoadPort;
using CMP.Services.Log;
using CMP.Services.Plc;
using CMP.Services.Rfid;
using CMP.Services.Tracking;
using CMP.Utils;
using CMP.ViewModels.Common.Dialog;
using CMP.ViewModels.Login;
using CMP.ViewModels.Tabs.OperationTab;
using CMP.Views;
using CMP.Views.Common.Dialog;
using CMP.Views.Common.Message;
using CMP.Views.Tabs.OperationTab;
using CMP.Views.Tabs.WarnTab;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using MessageBox = HandyControl.Controls.MessageBox;

namespace CMP;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<HeaderView>();
        containerRegistry.RegisterForNavigation<ContentView>();
        containerRegistry.RegisterDialog<FormDialog>();
        containerRegistry.RegisterForNavigation<MessageGrowl>();
        containerRegistry.RegisterForNavigation<AlarmEditFormView>();
        containerRegistry.RegisterDialog<UserManageDialogView, UserManageDialogViewModel>();
        containerRegistry.RegisterDialog<ChangePasswordDialogView, ChangePasswordDialogViewModel>();
        containerRegistry.RegisterDialog<CopyColumnDialogView, CopyColumnDialogViewModel>();
        containerRegistry.RegisterSingleton<ISessionIdleService, SessionIdleService>();
        containerRegistry.RegisterSingleton<IAuthService, AuthService>();
        containerRegistry.RegisterSingleton<IPermissionService, PermissionService>();
        containerRegistry.RegisterSingleton<NavigationGate>();
        containerRegistry.Register<LoginViewModel>();
        containerRegistry.Register<MainView>();
        containerRegistry.RegisterForNavigation<MainContentView, MainContentViewModel>();
        containerRegistry.RegisterForNavigation<MainPageView, MainPageViewModel>();
        // register region manager
        containerRegistry.RegisterSingleton<IRegionManager, RegionManager>();
        // 在模块的 RegisterTypes 方法中
        containerRegistry.RegisterSingleton<ILoadPortService, LoadPortService>();
        //主机通信模块
        containerRegistry.RegisterSingleton<IHostCommunicationService, HostCommunicationService>();
        containerRegistry.RegisterSingleton<IRfidReaderService, RfidReaderService>();
    }

    protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
    {
        base.ConfigureRegionAdapterMappings(regionAdapterMappings);

        regionAdapterMappings.RegisterMapping(typeof(TabControl),
            Container.Resolve<TabControlRegionAdapter>());
    }

    protected override Window CreateShell()
    {
        return Container.Resolve<MainView>();
    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        base.ConfigureModuleCatalog(moduleCatalog);
        moduleCatalog.AddModule<MainModule>();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _ = Container.Resolve<ILoadPortService>();
        _ = Container.Resolve<IHostCommunicationService>();
    }

    private Stopwatch? _startupStopwatch;
    private Mutex? _singleInstanceMutex;

    public Stopwatch StartupStopwatch { get => _startupStopwatch!; set => _startupStopwatch = value; }

    protected override void OnStartup(StartupEventArgs e)
    {
        ViewLifecycleEventRouter.Initialize();

        _singleInstanceMutex = new Mutex(true, "CMP_App_Singleton", out var createdNew);
        if (!createdNew)
        {
            MessageBox.Show(Lang.Resources.AppAlreadyRunningTips, Lang.Resources.ApplicationAlreadyRunning, MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }
        // 初始化数据库（首次访问时会自动创建）
        var db = SqlSugarHelper.Db;
        // 应用启动时自动连接三菱PLC（统一入口）
        MitsubishiPlcService.EnsureConnected();
        WaferActionDescriptionProvider.Initialize();
        StartupStopwatch = Stopwatch.StartNew();
        Logger.Info("应用程序启动开始", "startup.txt", 0);

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            WaferRuntimeStore.Instance.Flush();
        }
        catch
        {
            // 退出时落盘失败不阻断关闭
        }

        try
        {
            GlobalService.globalRobots?.Stop();
        }
        catch
        {
            // 退出时停机器人监控失败不阻断关闭
        }

        try
        {
            GlobalService.DataCenter?.Stop();
        }
        catch
        {
            // 退出时停采集循环失败不阻断关闭
        }

        try
        {
            Container.Resolve<IRfidReaderService>().Stop();
        }
        catch
        {
            // 退出时关闭 RFID 串口失败不阻断关闭
        }

        try
        {
            MitsubishiPlcService.DisconnectAll();
        }
        catch
        {
            // 退出时断开 PLC 失败不阻断关闭
        }

        try
        {
            base.OnExit(e);
        }
        catch
        {
            // WPF 关闭异常不阻断进程退出
        }

        try
        {
            _singleInstanceMutex?.ReleaseMutex();
            _singleInstanceMutex?.Dispose();
        }
        catch
        {
            // 互斥量释放失败不阻断进程退出
        }

        Environment.Exit(0);
    }


}