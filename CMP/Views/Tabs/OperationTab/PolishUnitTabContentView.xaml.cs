using System.Windows.Controls;
using System.Windows.Input;
using CMP.ViewModels.Tabs.OperationTab;

namespace CMP.Views.Tabs.OperationTab;

public partial class PolishUnitTabContentView : UserControl
{
    public PolishUnitTabContentView()
    {
        InitializeComponent();
    }

    private PolishUnitTabContentViewModel? ViewModel => DataContext as PolishUnitTabContentViewModel;
    private readonly HashSet<Button> _heldJogButtons = new();

    private void JogButton_OnPressed(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string motorName)
            return;

        if (!button.CaptureMouse())
            return;

        _heldJogButtons.Add(button);
        var direction = string.Equals(button.Content?.ToString(), "JOG-", StringComparison.Ordinal) ? "-" : "+";
        var parameter = $"{motorName}:{direction}";
        if (ViewModel?.JogPressCommand.CanExecute(parameter) == true)
            ViewModel.JogPressCommand.Execute(parameter);
        e.Handled = true;
    }

    private void JogButton_OnReleased(object sender, MouseEventArgs e)
    {
        if (sender is not Button button)
            return;

        ReleaseJog(button);
        if (button.IsMouseCaptured)
            button.ReleaseMouseCapture();

        e.Handled = true;
    }

    private void JogButton_OnCaptureLost(object sender, MouseEventArgs e)
    {
        if (sender is not Button { Tag: string motorName })
            return;

        if (sender is Button button)
            ReleaseJog(button);
        e.Handled = true;
    }

    private void ReleaseJog(Button button)
    {
        if (!_heldJogButtons.Remove(button) || button.Tag is not string motorName)
            return;

        if (ViewModel?.JogReleaseCommand.CanExecute(motorName) == true)
            ViewModel.JogReleaseCommand.Execute(motorName);
    }
}
