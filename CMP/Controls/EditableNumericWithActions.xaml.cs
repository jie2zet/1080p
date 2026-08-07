using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace CMP.Controls;

public partial class EditableNumericWithActions : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(EditableNumericWithActions),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(EditableNumericWithActions),
            new PropertyMetadata(-90000d));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(EditableNumericWithActions),
            new PropertyMetadata(90000d));

    public static readonly DependencyProperty DecimalPlacesProperty =
        DependencyProperty.Register(
            nameof(DecimalPlaces),
            typeof(int),
            typeof(EditableNumericWithActions),
            new PropertyMetadata(3));

    public static readonly DependencyProperty IncrementProperty =
        DependencyProperty.Register(
            nameof(Increment),
            typeof(double),
            typeof(EditableNumericWithActions),
            new PropertyMetadata(0.001d));

    public static readonly DependencyProperty IsDirtyProperty =
        DependencyProperty.Register(
            nameof(IsDirty),
            typeof(bool),
            typeof(EditableNumericWithActions),
            new PropertyMetadata(false));

    public static readonly DependencyProperty SetCommandProperty =
        DependencyProperty.Register(
            nameof(SetCommand),
            typeof(ICommand),
            typeof(EditableNumericWithActions),
            new PropertyMetadata(null));

    public static readonly DependencyProperty CancelCommandProperty =
        DependencyProperty.Register(
            nameof(CancelCommand),
            typeof(ICommand),
            typeof(EditableNumericWithActions),
            new PropertyMetadata(null));

    public static readonly DependencyProperty IsActionPopupOpenProperty =
        DependencyProperty.Register(
            nameof(IsActionPopupOpen),
            typeof(bool),
            typeof(EditableNumericWithActions),
            new PropertyMetadata(false));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public int DecimalPlaces
    {
        get => (int)GetValue(DecimalPlacesProperty);
        set => SetValue(DecimalPlacesProperty, value);
    }

    public double Increment
    {
        get => (double)GetValue(IncrementProperty);
        set => SetValue(IncrementProperty, value);
    }

    public bool IsDirty
    {
        get => (bool)GetValue(IsDirtyProperty);
        set => SetValue(IsDirtyProperty, value);
    }

    public ICommand? SetCommand
    {
        get => (ICommand?)GetValue(SetCommandProperty);
        set => SetValue(SetCommandProperty, value);
    }

    public ICommand? CancelCommand
    {
        get => (ICommand?)GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    public bool IsActionPopupOpen
    {
        get => (bool)GetValue(IsActionPopupOpenProperty);
        set => SetValue(IsActionPopupOpenProperty, value);
    }

    public EditableNumericWithActions()
    {
        InitializeComponent();
        NumericInput.GotKeyboardFocus += NumericInput_GotKeyboardFocus;
        NumericInput.LostKeyboardFocus += NumericInput_LostKeyboardFocus;
    }

    private void NumericInput_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        IsActionPopupOpen = true;
    }

    private void NumericInput_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.Background, new System.Action(() =>
        {
            var popup = FindName("ActionPopup") as FrameworkElement;

            if (NumericInput.IsKeyboardFocusWithin) return;
            if (popup?.IsKeyboardFocusWithin == true) return;

            if (IsDirty && CancelCommand?.CanExecute(null) == true)
            {
                CancelCommand.Execute(null);
            }

            IsActionPopupOpen = false;
        }));
    }

    private void ActionButton_Click(object sender, RoutedEventArgs e)
    {
        IsActionPopupOpen = false;
    }
}
