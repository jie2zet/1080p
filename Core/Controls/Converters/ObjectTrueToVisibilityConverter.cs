using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Core.Controls.Converters;

/// <summary>
/// SV/PLC 等 object 值：仅在为逻辑 True 时 Visible（如 bool、TRUE 字符串、非 0 整数），否则 Collapsed。
/// </summary>
public sealed class ObjectTrueToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return IsLogicalTrue(value, culture) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();

    public static bool IsLogicalTrue(object? value, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;
        if (value == null)
            return false;
        if (value is bool b)
            return b;
        if (value is byte or sbyte or short or ushort or int or uint or long or ulong)
        {
            try
            {
                return System.Convert.ToInt64(value) != 0;
            }
            catch
            {
                return false;
            }
        }

        if (value is string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return false;
            if (bool.TryParse(s, out var pb))
                return pb;
            if (long.TryParse(s, NumberStyles.Integer, culture, out var n))
                return n != 0;
            return s.Equals("TRUE", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}
