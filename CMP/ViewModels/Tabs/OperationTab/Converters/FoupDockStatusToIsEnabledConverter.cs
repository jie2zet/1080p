using System;
using System.Globalization;
using System.Windows.Data;
using CMP.Services.LoadPort;

namespace CMP.ViewModels.Tabs.OperationTab.Converters
{
    /// <summary>
    /// 根据 FoupDockStatus 来确定按钮是否启用
    /// 如果 FoupDockStatus 为 Docking，则禁用；否则启用
    /// </summary>
    public class FoupDockStatusToIsEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FoupDockStatus dockStatus)
            {
                // 如果 FoupDockStatus 为 Docking，则禁用
                return dockStatus != FoupDockStatus.Docking;
            }

            // 如果值不是 FoupDockStatus 类型，默认启用
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

