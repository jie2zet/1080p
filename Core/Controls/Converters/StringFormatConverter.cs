using System.Globalization;
using System.Windows.Data;

namespace Core.Controls.Converters;

public class StringFormatConverter : IMultiValueConverter
{
    public object Convert(object[]? values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2)
            return string.Empty;
            
        string? format = values[0].ToString();
        object[] args = values.Skip(1).ToArray();
        
        try
        {
            if (format != null) return string.Format(format, args);
        }
        catch
        {
            return string.Empty;
        }
        return string.Empty;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}