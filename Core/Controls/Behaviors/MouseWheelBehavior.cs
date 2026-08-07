using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Core.Controls.Behaviors
{
    /// <summary>
    /// 旨在解决容器类控件中子控件无法滚动的问题
    /// </summary>
    public static class MouseWheelBehavior
    {
        public static readonly DependencyProperty ForwardMouseWheelToParentProperty =
            DependencyProperty.RegisterAttached(
                "ForwardMouseWheelToParent",
                typeof(bool),
                typeof(MouseWheelBehavior),
                new PropertyMetadata(false, OnForwardMouseWheelToParentChanged));

        public static void SetForwardMouseWheelToParent(DependencyObject element, bool value)
        {
            element.SetValue(ForwardMouseWheelToParentProperty, value);
        }

        public static bool GetForwardMouseWheelToParent(DependencyObject element)
        {
            return (bool)element.GetValue(ForwardMouseWheelToParentProperty);
        }

        private static void OnForwardMouseWheelToParentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement uiElement)
            {
                if ((bool)e.NewValue)
                {
                    uiElement.PreviewMouseWheel += UiElement_PreviewMouseWheel;
                }
                else
                {
                    uiElement.PreviewMouseWheel -= UiElement_PreviewMouseWheel;
                }
            }
        }

        private static void UiElement_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var dep = sender as DependencyObject;
            var scrollParent = FindAncestor<ScrollViewer>(dep);
            if (scrollParent != null)
            {
                scrollParent.ScrollToVerticalOffset(scrollParent.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T t) return t;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
