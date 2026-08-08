using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Core.Controls.Behaviors
{
    public class ZeroOrRangeValidationRule : ValidationRule
    {
        public double Min { get; set; }
        public double Max { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            // 处理 null 或空值（视业务需求，此处视为无效）
            if (value == null)
                return new ValidationResult(false, "值不能为空");

            // 尝试转换为 double
            if (!double.TryParse(value.ToString(), out double doubleValue))
                return new ValidationResult(false, "请输入有效数字");

            // 允许 0（特殊豁免值）
            if (doubleValue == 0)
                return ValidationResult.ValidResult; 

            // 非0值必须在 [Min, Max] 区间内
            if (doubleValue >= Min && doubleValue <= Max)
                return ValidationResult.ValidResult; 

            return new ValidationResult(false, $"值必须为 0 或在 {Min} 到 {Max} 之间");
        }
    }
}
