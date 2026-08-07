using System;
using System.Globalization;
using System.Windows.Data;

namespace Core.Controls.Converters
{
    public class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
                return intValue.ToString();
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue && int.TryParse(strValue, out int result))
                return result;
            return 0; // 返回默认值（或可根据业务返回 null，但模型中是 int 类型）
        }
    }
}