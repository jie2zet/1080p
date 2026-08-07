using System.Globalization;
using System.Windows.Data;

namespace Core.Controls.Converters
{
    public class DictionaryToItemsSourceConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Dictionary<string, string> dictionary)
            {
                return dictionary.Keys.ToList();
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}