using System.Collections.Concurrent;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Core.Controls.Converters;

/// <summary>
/// 字符串颜色 → SolidColorBrush。使用 Freeze + 缓存，避免在绑定路径中 Dispatcher.Invoke 导致死锁或阻塞。
/// </summary>
public class StringToBrushConverter : IValueConverter
{
    private static readonly ConcurrentDictionary<string, SolidColorBrush> Cache = new(StringComparer.OrdinalIgnoreCase);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            if (value is not string colorString || string.IsNullOrEmpty(colorString))
                return Brushes.Transparent;

            return Cache.GetOrAdd(colorString, static key =>
            {
                var color = (Color)ColorConverter.ConvertFromString(key);
                var brush = new SolidColorBrush(color);
                if (brush.CanFreeze)
                    brush.Freeze();
                return brush;
            });
        }
        catch
        {
            return Brushes.Transparent;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
