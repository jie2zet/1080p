namespace CMP.Services.Auth;

public interface IAuthService
{
    bool Login(string userName, string password, out string errorMessage);

    void Logout(LogoutReason reason);

    bool ChangePassword(string oldPassword, string newPassword, out string errorMessage);

    bool ShowLoginDialog(System.Windows.Window? owner = null);
}
