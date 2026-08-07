using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Core.Controls.Converters
{
    /// <summary>
    /// string 与 DateTime 互转（支持HandyControl DateTimePicker）
    /// </summary>
    [ValueConversion(typeof(string), typeof(DateTime?))]
    public class StringToDateTimeConverter : MarkupExtension, IValueConverter
    {
        private static StringToDateTimeConverter? _instance;

        private readonly string _format = "HH:mm:ss";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // string → DateTime?
            if (value is string str && !string.IsNullOrWhiteSpace(str))
            {
                if (DateTime.TryParse(str, out DateTime date))
                    return date;
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // DateTime? → string
            if (value is DateTime date)
            {
                return date.ToString(_format);
            }
            return string.Empty;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ??= new StringToDateTimeConverter();
        }
    }
}