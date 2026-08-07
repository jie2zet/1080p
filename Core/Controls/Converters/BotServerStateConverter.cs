using System.Globalization;
using System.Windows.Data;

namespace Core.Controls.Converters;

public class BotServerStateConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value?.ToString()?.Equals("On", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "Off";
        }
        else if(value?.ToString()?.Equals("Off", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "On";
        }
        
        if (targetType.IsEnum)
        {
            if (value != null) return Enum.GetName(targetType, value) ?? "Off";
        }
        return "Off"; // Default value if conversion fails
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str && targetType.IsEnum)
        {
            return Enum.Parse(targetType, str);
        }
        return "Off"; // Default value if conversion fails
    }
}