using System.Windows;

namespace CMP.Services.Auth;

public interface ISessionIdleService
{
    event EventHandler? SessionTimedOut;

    void Start(Window mainWindow);

    void Stop();

    void Reset();
}
