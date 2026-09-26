using CMP.ViewModels.Common;
using CMP.ViewModels.Tabs.OperationTab.InterlockTab;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace CMP.Views.Tabs.OperationTab.InterlockTab;

public partial class InterlockTabView : UserControl
{
    public InterlockTabView()
    {
        InitializeComponent();

        // Keep the tab functional even when the host region does not replay
        // the global lifecycle class handlers for a recycled TabControl item.
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        IsVisibleChanged += OnIsVisibleChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (IsVisible)
        {
            ActivateViewModel();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        DeactivateViewModel();
    }

    private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            ActivateViewModel();
        }
        else if (e.NewValue is false)
        {
            DeactivateViewModel();
        }
    }

    private void ActivateViewModel()
    {
        if (DataContext is IViewLifecycleAware lifecycleAware)
        {
            lifecycleAware.OnViewActivated();
        }
    }

    private void DeactivateViewModel()
    {
        if (DataContext is IViewLifecycleAware lifecycleAware)
        {
            lifecycleAware.OnViewDeactivated();
        }
    }

    private void UnlockToggleButton_Click(object sender, RoutedEventArgs e)
    {
        // Use the routed Click event as the final input path. HandyControl's
        // switch template can replace the command source while applying its
        // template, but the ToggleButton itself always raises Click.
        if (sender is ToggleButton { DataContext: InterlockItemViewModel item })
        {
            item.ToggleCommand.Execute();
        }
    }
}
