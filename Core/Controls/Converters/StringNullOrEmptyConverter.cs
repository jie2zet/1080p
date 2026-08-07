using System;
using System.Globalization;
using System.Windows.Data;

namespace Core.Controls.Converters
{
    /// <summary>
    /// 字符串是否为空或null的转换器
    /// </summary>
    public class StringNullOrEmptyConverter : IValueConverter
    {
        /// <summary>
        /// 将字符串转换为是否为空的布尔值
        /// </summary>
        /// <param name="value">输入值</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">转换参数</param>
        /// <param name="culture">文化信息</param>
        /// <returns>如果字符串为null或空则返回true，否则返回false</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return true;

            if (value is string stringValue)
                return string.IsNullOrEmpty(stringValue);

            return false;
        }

        /// <summary>
        /// 反向转换（未实现）
        /// </summary>
        /// <param name="value">输入值</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">转换参数</param>
        /// <param name="culture">文化信息</param>
        /// <returns>抛出 NotImplementedException</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }
}