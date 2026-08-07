using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Core.Controls.Converters;

/// <summary>
/// 将布尔值转换为颜色的转换器
/// true = 绿色 (表示激活/正常)
/// false = 深灰色 (表示未激活/关闭)
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue 
                ? new SolidColorBrush(Color.FromRgb(0, 200, 0))  // 绿色
                : new SolidColorBrush(Color.FromRgb(47, 79, 79)); // 深灰色
        }
        return new SolidColorBrush(Color.FromRgb(47, 79, 79));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
