using CMP.Models.UserData;
using Prism.Mvvm;

namespace CMP.Services.Auth;

/// <summary>
/// 当前登录会话（单例，供 Header 绑定与空闲检测使用）。
/// </summary>
public sealed class SessionContext : BindableBase
{
    public static SessionContext Instance { get; } = new();

    private SessionContext()
    {
    }

    private bool _isAuthenticated;
    private string _userName = string.Empty;
    private PermissionEnum _level = PermissionEnum.Operator;
    private DateTime _loginTime;
    private DateTime _lastActivityTime = DateTime.UtcNow;

    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        private set => SetProperty(ref _isAuthenticated, value);
    }

    public string UserName
    {
        get => _userName;
        private set => SetProperty(ref _userName, value);
    }

    public PermissionEnum Level
    {
        get => _level;
        private set
        {
            if (SetProperty(ref _level, value))
            {
                RaisePropertyChanged(nameof(LevelDisplay));
            }
        }
    }

    public string LevelDisplay => Level.ToString();

    public DateTime LoginTime
    {
        get => _loginTime;
        private set => SetProperty(ref _loginTime, value);
    }

    public DateTime LastActivityTime
    {
        get => _lastActivityTime;
        private set => SetProperty(ref _lastActivityTime, value);
    }

    public void SetUser(UserModel user)
    {
        UserName = user.UserName;
        Level = user.Level;
        LoginTime = DateTime.Now;
        TouchActivity();
        IsAuthenticated = true;
    }

    public void Clear()
    {
        IsAuthenticated = false;
        UserName = string.Empty;
        Level = PermissionEnum.Operator;
        LoginTime = default;
        LastActivityTime = DateTime.UtcNow;
    }

    public void TouchActivity()
    {
        LastActivityTime = DateTime.UtcNow;
    }
}
