using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CMP.ViewModels.Login;

namespace CMP.Views.Login;

public partial class LoginPanel
{
    public LoginPanel()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        PasswordBox.Focus();
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is LoginViewModel oldVm)
        {
            oldVm.LoginFormReset -= OnLoginFormReset;
        }

        if (e.NewValue is LoginViewModel newVm)
        {
            newVm.LoginFormReset += OnLoginFormReset;
        }
    }

    private void OnLoginFormReset()
    {
        ClearPassword();
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            vm.Password = PasswordBox.Password;
        }
    }

    private void PasswordBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || DataContext is not LoginViewModel vm)
        {
            return;
        }

        if (vm.LoginCommand.CanExecute())
        {
            vm.LoginCommand.Execute();
        }
    }

    public void ClearPassword()
    {
        PasswordBox.Password = string.Empty;
        if (DataContext is LoginViewModel vm)
        {
            vm.Password = string.Empty;
        }
    }
}
