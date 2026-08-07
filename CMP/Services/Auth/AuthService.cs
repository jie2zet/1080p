using System.Windows;
using CMP.Models.UserData;
using CMP.Utils;
using CMP.ViewModels.Login;
using CMP.Views.Login;
using SqlSugar;

namespace CMP.Services.Auth;

public class AuthService : IAuthService
{
    private readonly ISessionIdleService _idleService;

    public AuthService(ISessionIdleService idleService)
    {
        _idleService = idleService;
    }

    public bool Login(string userName, string password, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrEmpty(password))
        {
            errorMessage = "请输入用户名和密码";
            return false;
        }

        var user = SqlSugarHelper.Db.Queryable<UserModel>()
            .First(u => u.UserName == userName.Trim());

        if (user == null || !user.IsEnabled)
        {
            errorMessage = "用户名或密码错误";
            AuditLogService.Write(userName.Trim(), PermissionEnum.Operator, "Login", "Failed", "UserNotFoundOrDisabled");
            return false;
        }

        SyncLegacyRole(user);

        if (!VerifyPassword(user, password))
        {
            errorMessage = "用户名或密码错误";
            AuditLogService.Write(userName.Trim(), user.Level, "Login", "Failed", "InvalidPassword");
            return false;
        }

        MigrateLegacyPasswordIfNeeded(user, password);
        SessionContext.Instance.SetUser(user);
        _idleService.Reset();
        AuditLogService.Write(user.UserName, user.Level, "Login", "Success");
        return true;
    }

    public void Logout(LogoutReason reason)
    {
        if (!SessionContext.Instance.IsAuthenticated)
        {
            return;
        }

        var userName = SessionContext.Instance.UserName;
        var level = SessionContext.Instance.Level;
        SessionContext.Instance.Clear();
        _idleService.Stop();

        var action = reason switch
        {
            LogoutReason.Timeout => "SessionTimeout",
            LogoutReason.Manual => "Logout",
            _ => "Logout"
        };
        AuditLogService.Write(userName, level, action, "Success", reason.ToString());
    }

    public bool ChangePassword(string oldPassword, string newPassword, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (!SessionContext.Instance.IsAuthenticated)
        {
            errorMessage = "请先登录";
            return false;
        }

        if (string.IsNullOrEmpty(oldPassword))
        {
            errorMessage = "请输入原密码";
            return false;
        }

        if (string.IsNullOrEmpty(newPassword))
        {
            errorMessage = "请输入新密码";
            return false;
        }

        if (newPassword.Length < 4)
        {
            errorMessage = "新密码长度不能少于 4 位";
            return false;
        }

        if (oldPassword == newPassword)
        {
            errorMessage = "新密码不能与原密码相同";
            return false;
        }

        var userName = SessionContext.Instance.UserName;
        var user = SqlSugarHelper.Db.Queryable<UserModel>()
            .First(u => u.UserName == userName);

        if (user == null || !user.IsEnabled)
        {
            errorMessage = "用户不存在或已禁用";
            return false;
        }

        if (!VerifyPassword(user, oldPassword))
        {
            errorMessage = "原密码错误";
            AuditLogService.Write(userName, user.Level, "ChangePassword", "Failed", "InvalidOldPassword");
            return false;
        }

        user.PasswordHash = PasswordHasher.Hash(newPassword);
        user.Password = string.Empty;
        user.ModifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        SqlSugarHelper.Db.Updateable(user)
            .UpdateColumns(u => new { u.PasswordHash, u.Password, u.ModifyTime })
            .ExecuteCommand();

        AuditLogService.Write(userName, user.Level, "ChangePassword", "Success");
        return true;
    }

    public bool ShowLoginDialog(Window? owner = null)
    {
        var viewModel = new LoginViewModel(this);
        var window = new LoginWindow
        {
            DataContext = viewModel,
            Owner = owner
        };

        var result = window.ShowDialog();
        return result == true && SessionContext.Instance.IsAuthenticated;
    }

    private static void SyncLegacyRole(UserModel user)
    {
        if (!string.IsNullOrEmpty(user.Group)
            && Enum.TryParse<PermissionEnum>(user.Group, true, out var parsedLevel)
            && user.Level != parsedLevel)
        {
            user.Level = parsedLevel;
        }
        else if (string.IsNullOrEmpty(user.Group))
        {
            user.Group = user.Level.ToString();
        }
    }

    private static bool VerifyPassword(UserModel user, string password)
    {
        if (!string.IsNullOrEmpty(user.PasswordHash))
        {
            return PasswordHasher.Verify(password, user.PasswordHash);
        }

        if (!string.IsNullOrEmpty(user.Password))
        {
            return PasswordHasher.Verify(password, user.Password);
        }

        return false;
    }

    private static void MigrateLegacyPasswordIfNeeded(UserModel user, string plainPassword)
    {
        if (PasswordHasher.IsHashed(user.PasswordHash))
        {
            return;
        }

        user.PasswordHash = PasswordHasher.Hash(plainPassword);
        user.Password = string.Empty;
        user.ModifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        SqlSugarHelper.Db.Updateable(user)
            .UpdateColumns(u => new { u.PasswordHash, u.Password, u.ModifyTime })
            .ExecuteCommand();
    }
}
