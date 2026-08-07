using System.Windows.Controls;
using System.Windows.Input;
using CMP.ViewModels.Common.Dialog;

namespace CMP.Views.Common.Dialog;

public partial class ChangePasswordDialogView
{
    public ChangePasswordDialogView()
    {
        InitializeComponent();
        Loaded += (_, _) => OldPasswordBox.Focus();
    }

    private void OldPasswordBox_OnPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ChangePasswordDialogViewModel vm)
        {
            vm.OldPassword = OldPasswordBox.Password;
        }
    }

    private void NewPasswordBox_OnPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ChangePasswordDialogViewModel vm)
        {
            vm.NewPassword = NewPasswordBox.Password;
        }
    }

    private void ConfirmPasswordBox_OnPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ChangePasswordDialogViewModel vm)
        {
            vm.ConfirmPassword = ConfirmPasswordBox.Password;
        }
    }

    private void ConfirmPasswordBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || DataContext is not ChangePasswordDialogViewModel vm)
        {
            return;
        }

        if (vm.ConfirmCommand.CanExecute())
        {
            vm.ConfirmCommand.Execute();
        }
    }
}
