using CMP.Models.UserData;
using CMP.Services.Auth;
using Prism.Commands;
using Prism.Mvvm;

namespace CMP.ViewModels.Login;

public class LoginViewModel : BindableBase
{
    private readonly IAuthService _authService;
    private string _userName = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        LoginCommand = new DelegateCommand(ExecuteLogin, CanExecuteLogin)
            .ObservesProperty(() => UserName)
            .ObservesProperty(() => Password)
            .ObservesProperty(() => IsBusy);
    }

    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public DelegateCommand LoginCommand { get; }

    public event Action? LoginSucceeded;

    /// <summary>
    /// 通知视图清空 PasswordBox（PasswordBox 无法双向绑定）。
    /// </summary>
    public event Action? LoginFormReset;

    public void Reset()
    {
        ErrorMessage = string.Empty;
        ClearPassword();
    }

    private void ClearPassword()
    {
        Password = string.Empty;
        LoginFormReset?.Invoke();
    }

    private bool CanExecuteLogin() =>
        !IsBusy && !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrEmpty(Password);

    private void ExecuteLogin()
    {
        ErrorMessage = string.Empty;
        IsBusy = true;
        try
        {
            if (_authService.Login(UserName, Password, out var error))
            {
                ClearPassword();
                LoginSucceeded?.Invoke();
            }
            else
            {
                ErrorMessage = error;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
