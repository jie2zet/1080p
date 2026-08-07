using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Core.Controls.Converters;

/// <summary>
/// Converts a boolean value to HorizontalAlignment.
/// true = Right, false = Left (default)
/// Can be inverted using parameter "Invert"
/// </summary>
public class BoolToAlignmentConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var boolValue = value is true;
        var invert = parameter?.ToString()?.Equals("Invert", StringComparison.OrdinalIgnoreCase) == true;
        
        if (invert)
            boolValue = !boolValue;
            
        return boolValue ? HorizontalAlignment.Right : HorizontalAlignment.Left;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is HorizontalAlignment alignment)
        {
            var result = alignment == HorizontalAlignment.Right;
            var invert = parameter?.ToString()?.Equals("Invert", StringComparison.OrdinalIgnoreCase) == true;
            return invert ? !result : result;
        }
        return false;
    }
}
