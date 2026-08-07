using System.Windows;
using System.Windows.Media;

namespace CMP.Extensions;

public static class VisualTreeHelperExtensions
{
    /// <summary>
    /// 在可视化树中查找指定类型的子元素
    /// </summary>
    public static T? FindVisualChild<T>(this DependencyObject? parent) where T : DependencyObject
    {
        if (parent == null) return null;
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is T found)
            {
                return found;
            }

            var childOfChild = FindVisualChild<T>(child);
            if (childOfChild != null)
            {
                return childOfChild;
            }
        }

        return null;
    }
}