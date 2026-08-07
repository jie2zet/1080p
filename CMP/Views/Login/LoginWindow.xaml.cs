using System.Windows;

using CMP.ViewModels.Login;



namespace CMP.Views.Login;



public partial class LoginWindow

{

    public LoginWindow()

    {

        InitializeComponent();

        DataContextChanged += OnDataContextChanged;

    }



    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is LoginViewModel oldVm)
        {
            oldVm.LoginSucceeded -= OnLoginSucceeded;
            oldVm.LoginFormReset -= OnLoginFormReset;
        }

        if (e.NewValue is LoginViewModel newVm)
        {
            newVm.LoginSucceeded += OnLoginSucceeded;
            newVm.LoginFormReset += OnLoginFormReset;
        }
    }

    private void OnLoginFormReset()
    {
        PasswordBox.Password = string.Empty;
    }



    private void OnLoginSucceeded()

    {

        DialogResult = true;

        Close();

    }



    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)

    {

        if (DataContext is LoginViewModel vm)

        {

            vm.Password = PasswordBox.Password;

        }

    }



    private void CancelButton_OnClick(object sender, RoutedEventArgs e)

    {

        DialogResult = false;

        Close();

    }

}


