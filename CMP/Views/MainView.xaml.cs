using System.ComponentModel;
using System.Windows;
using CMP.Services.Auth;
using CMP.Services.Log;
using CMP.ViewModels;
using HandyControl.Controls;
using MessageBox = HandyControl.Controls.MessageBox;

namespace CMP.Views;

public partial class MainView
{
    private readonly IAuthService _authService;
    private readonly ISessionIdleService _idleService;
    private bool _isIdleServiceStarted;

    public MainView(IAuthService authService, ISessionIdleService idleService)
    {
        _authService = authService;
        _idleService = idleService;
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        DataContextChanged += OnDataContextChanged;
        _idleService.SessionTimedOut += OnSessionTimedOut;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is MainViewModel oldVm)
        {
            oldVm.UserAuthenticated -= OnUserAuthenticated;
        }

        if (e.NewValue is MainViewModel newVm)
        {
            newVm.UserAuthenticated += OnUserAuthenticated;
            if (SessionContext.Instance.IsAuthenticated)
            {
                OnUserAuthenticated();
            }
        }
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var app = (App)Application.Current;
        if (app.StartupStopwatch != null)
        {
            app.StartupStopwatch.Stop();
            Logger.Info($"{app.StartupStopwatch.ElapsedMilliseconds} ms", "startup.txt", 0);
        }

        if (DataContext is MainViewModel viewModel)
        {
            Task.Run(async () => await viewModel.InitializeAsync());
        }
    }

    private void OnUserAuthenticated()
    {
        if (_isIdleServiceStarted)
        {
            _idleService.Start(this);
            if (DataContext is MainViewModel viewModel)
            {
                Task.Run(async () => await viewModel.InitializeAsync());
            }

            return;
        }

        _isIdleServiceStarted = true;
        _idleService.Start(this);
    }

    private void OnSessionTimedOut(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            _authService.Logout(LogoutReason.Timeout);
            Growl.WarningGlobal("会话已超时，请重新登录");
            _idleService.Stop();
        });
    }

    internal void RequireReLogin(LogoutReason reason = LogoutReason.Timeout)
    {
        _authService.Logout(reason);

        if (reason == LogoutReason.Timeout)
        {
            Growl.WarningGlobal("会话已超时，请重新登录");
        }
    }

    private void MainView_OnClosing(object? sender, CancelEventArgs e)
    {
        if (MessageBox.Show(Lang.Resources.ExitConfirmation, Lang.Resources.Confirmation,
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
        {
            e.Cancel = true;
            return;
        }

        _idleService.SessionTimedOut -= OnSessionTimedOut;
        _idleService.Stop();
        _authService.Logout(LogoutReason.ApplicationExit);
    }
}
