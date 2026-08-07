using CMP.Services.Auth;
using System.Windows;

namespace CMP.ViewModels.Common.Dialog;

public class ChangePasswordDialogViewModel : BindableBase, IDialogAware
{
    private readonly IAuthService _authService;
    private string _oldPassword = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _errorMessage = string.Empty;
    private string _currentUserName = string.Empty;

    public ChangePasswordDialogViewModel(IAuthService authService)
    {
        _authService = authService;
        ConfirmCommand = new DelegateCommand(OnConfirm);
        CancelCommand = new DelegateCommand(OnCancel);
    }

    public string CurrentUserName
    {
        get => _currentUserName;
        set => SetProperty(ref _currentUserName, value);
    }

    public string OldPassword
    {
        get => _oldPassword;
        set => SetProperty(ref _oldPassword, value);
    }

    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value);
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public DelegateCommand ConfirmCommand { get; }

    public DelegateCommand CancelCommand { get; }

    public DialogCloseListener RequestClose { get; }

    public bool CanCloseDialog() => true;

    public void OnDialogClosed()
    {
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        CurrentUserName = SessionContext.Instance.UserName;
        OldPassword = string.Empty;
        NewPassword = string.Empty;
        ConfirmPassword = string.Empty;
        ErrorMessage = string.Empty;
    }

    private void OnConfirm()
    {
        ErrorMessage = string.Empty;

        if (NewPassword != ConfirmPassword)
        {
            ErrorMessage = "两次输入的新密码不一致";
            return;
        }

        if (!_authService.ChangePassword(OldPassword, NewPassword, out var error))
        {
            ErrorMessage = error;
            return;
        }

        MessageBox.Show("密码修改成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        RequestClose.Invoke(new DialogResult(ButtonResult.OK));
    }

    private void OnCancel()
    {
        RequestClose.Invoke(new DialogResult(ButtonResult.Cancel));
    }
}
