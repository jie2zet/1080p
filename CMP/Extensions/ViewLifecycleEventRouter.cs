using System.Windows;
using System.Windows.Controls;
using CMP.ViewModels.Common;

namespace CMP.Extensions;

public static class ViewLifecycleEventRouter
{
    private static bool _initialized;
    private static readonly HashSet<UserControl> HookedControls = [];

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        EventManager.RegisterClassHandler(typeof(UserControl), FrameworkElement.LoadedEvent,
            new RoutedEventHandler(OnLoaded), true);
        EventManager.RegisterClassHandler(typeof(UserControl), FrameworkElement.UnloadedEvent,
            new RoutedEventHandler(OnUnloaded), true);
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not UserControl control)
        {
            return;
        }

        HookControl(control);

        if (control.IsVisible)
        {
            Activate(control.DataContext);
        }
    }

    private static void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (sender is not UserControl control)
        {
            return;
        }

        Deactivate(control.DataContext);
        UnhookControl(control);
    }

    private static void HookControl(UserControl control)
    {
        if (!HookedControls.Add(control))
        {
            return;
        }

        control.IsVisibleChanged += OnIsVisibleChanged;
        control.DataContextChanged += OnDataContextChanged;
    }

    private static void UnhookControl(UserControl control)
    {
        if (!HookedControls.Remove(control))
        {
            return;
        }

        control.IsVisibleChanged -= OnIsVisibleChanged;
        control.DataContextChanged -= OnDataContextChanged;
    }

    private static void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        if (e.NewValue is true)
        {
            Activate(element.DataContext);
        }
        else if (e.NewValue is false)
        {
            Deactivate(element.DataContext);
        }
    }

    private static void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (ReferenceEquals(e.OldValue, e.NewValue))
        {
            return;
        }

        // DataContextChanged is not treated as a page leave signal.
        // It only replays activation when DataContext is assigned after the view is already visible.
        if (sender is FrameworkElement { IsVisible: true } && e.OldValue is null && e.NewValue is not null)
        {
            Activate(e.NewValue);
        }
    }

    private static void Activate(object? viewModel)
    {
        if (viewModel is IViewLifecycleAware lifecycleAware)
        {
            lifecycleAware.OnViewActivated();
        }
    }

    private static void Deactivate(object? viewModel)
    {
        if (viewModel is IViewLifecycleAware lifecycleAware)
        {
            lifecycleAware.OnViewDeactivated();
        }
    }
}