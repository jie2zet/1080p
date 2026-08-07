using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace CMP.Services.Auth;

public class SessionIdleService : ISessionIdleService
{
    public const int IdleTimeoutMinutes = 10;

    private readonly TimeSpan _idleTimeout = TimeSpan.FromMinutes(IdleTimeoutMinutes);
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

    private Window? _mainWindow;
    private DispatcherTimer? _timer;
    private bool _isRunning;

    public event EventHandler? SessionTimedOut;

    public void Start(Window mainWindow)
    {
        Stop();
        _mainWindow = mainWindow;
        InputManager.Current.PreProcessInput += OnUserActivity;
        _timer = new DispatcherTimer { Interval = _checkInterval };
        _timer.Tick += OnTimerTick;
        _timer.Start();
        _isRunning = true;
        SessionContext.Instance.TouchActivity();
    }

    public void Stop()
    {
        InputManager.Current.PreProcessInput -= OnUserActivity;
        if (_timer != null)
        {
            _timer.Tick -= OnTimerTick;
            _timer.Stop();
            _timer = null;
        }

        _isRunning = false;
        _mainWindow = null;
    }

    public void Reset()
    {
        SessionContext.Instance.TouchActivity();
    }

    private void OnUserActivity(object sender, PreProcessInputEventArgs e)
    {
        if (!SessionContext.Instance.IsAuthenticated)
        {
            return;
        }

        if (e.StagingItem.Input is MouseEventArgs or KeyEventArgs or TextCompositionEventArgs or TouchEventArgs)
        {
            SessionContext.Instance.TouchActivity();
        }
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (!SessionContext.Instance.IsAuthenticated)
        {
            return;
        }

        var idle = DateTime.UtcNow - SessionContext.Instance.LastActivityTime;
        if (idle < _idleTimeout)
        {
            return;
        }

        _timer?.Stop();
        SessionTimedOut?.Invoke(this, EventArgs.Empty);
    }
}
