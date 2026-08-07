using System.Windows;
using System.Windows.Controls;

namespace Core.Controls.Extensions;

public class StackPanelHelpers
{
    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.RegisterAttached(
            "Spacing",
            typeof(double),
            typeof(StackPanelHelpers),
            new PropertyMetadata(0.0, OnSpacingChanged));

    // 保存原始边距的附加属性
    private static readonly DependencyProperty OriginalMarginProperty =
        DependencyProperty.RegisterAttached(
            "OriginalMargin",
            typeof(Thickness),
            typeof(StackPanelHelpers),
            new PropertyMetadata(new Thickness()));

    public static double GetSpacing(DependencyObject obj)
    {
        return (double)obj.GetValue(SpacingProperty);
    }

    public static void SetSpacing(DependencyObject obj, double value)
    {
        obj.SetValue(SpacingProperty, value);
    }

    private static Thickness GetOriginalMargin(DependencyObject obj)
    {
        return (Thickness)obj.GetValue(OriginalMarginProperty);
    }

    private static void SetOriginalMargin(DependencyObject obj, Thickness value)
    {
        obj.SetValue(OriginalMarginProperty, value);
    }

    private static void OnSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StackPanel panel)
        {
            panel.Loaded += (_, _) => ApplySpacing(panel);
            panel.LayoutUpdated += (_, _) => ApplySpacing(panel);
            panel.SizeChanged += (_, _) => ApplySpacing(panel);
        }
    }

    private static void ApplySpacing(StackPanel panel)
    {
        double spacing = GetSpacing(panel);
        if (spacing == 0) return;

        bool isHorizontal = panel.Orientation == Orientation.Horizontal;

        for (int i = 0; i < panel.Children.Count; i++)
        {
            UIElement child = panel.Children[i];

            // 存储原始边距（如果尚未存储）
            if (!(child.ReadLocalValue(OriginalMarginProperty) is Thickness))
            {
                SetOriginalMargin(child, (Thickness)child.GetValue(FrameworkElement.MarginProperty));
            }

            Thickness originalMargin = GetOriginalMargin(child);
            Thickness newMargin = originalMargin;

            // 根据方向和位置应用间距
            if (isHorizontal)
            {
                if (i < panel.Children.Count - 1) // 不是最后一个元素
                    newMargin = new Thickness(originalMargin.Left, originalMargin.Top,
                        spacing, originalMargin.Bottom);
            }
            else // 垂直方向
            {
                if (i < panel.Children.Count - 1) // 不是最后一个元素
                    newMargin = new Thickness(originalMargin.Left, originalMargin.Top,
                        originalMargin.Right, spacing);
            }

            child.SetValue(FrameworkElement.MarginProperty, newMargin);
        }
    }
}