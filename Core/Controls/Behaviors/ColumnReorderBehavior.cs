using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Core.Controls.Behaviors;
public class ColumnReorderBehavior : Behavior<DataGrid>
{
    public static readonly DependencyProperty ColumnReorderedCommandProperty =
        DependencyProperty.Register(
            "ColumnReorderedCommand",
            typeof(ICommand),
            typeof(ColumnReorderBehavior),
            new PropertyMetadata(null));

    public ICommand ColumnReorderedCommand
    {
        get { return (ICommand)GetValue(ColumnReorderedCommandProperty); }
        set { SetValue(ColumnReorderedCommandProperty, value); }
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.ColumnReordered += OnColumnReordered;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.ColumnReordered -= OnColumnReordered;
    }

    private void OnColumnReordered(object? sender, DataGridColumnEventArgs e)
    {
        if (e.Column.DisplayIndex < AssociatedObject.FrozenColumnCount)
        {            
            return;
        }
        if (ColumnReorderedCommand?.CanExecute(AssociatedObject.Columns) == true)
        {
            ColumnReorderedCommand.Execute(AssociatedObject.Columns);
        }
    }
}
