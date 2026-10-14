using System.Collections.Generic;

namespace CMP.Models.Recipe.RecipeEdit;

/// <summary>
/// 参数可操作性规则：当 <see cref="SourceParameter"/> 在同一个步骤上的取值属于
/// <see cref="AllowValues"/> 时，本参数在该步骤上可操作。
/// 规则真源是 Recipe 规格 Excel 的「说明」列，由模板承载，不落盘。
/// </summary>
/// <param name="SourceParameter">规则来源参数名，例："Process Type"。</param>
/// <param name="AllowValues">允许的选项数字前缀集合，例：["1", "3"]。</param>
/// <param name="Reason">「说明」列原文，仅在不可操作时作为 ToolTip 显示。</param>
/// <param name="AllOf">预留：全部子规则都满足时才可操作（Pen A1 的 AND 组合条件）。本期不参与求值。</param>
public sealed record OperableRule(
    string SourceParameter,
    IReadOnlyList<string> AllowValues,
    string? Reason = null,
    IReadOnlyList<OperableRule>? AllOf = null);

/// <summary>
/// Recipe 选项取值的统一解析工具。
/// 选项在配方中以 "1: Dry With Arm" 或 "1" 两种形态存在，本类只负责取出数字前缀。
/// </summary>
public static class RecipeOptionValue
{
    /// <summary>
    /// 取出选项的数字前缀；无法解析时返回 null。
    /// "1: Dry With Arm" -> "1"；"1" -> "1"；"-1: Recipe End" -> "-1"；"" -> null；"abc" -> null。
    /// </summary>
    public static string? NumericPrefix(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var span = value.AsSpan().Trim();

        int i = 0;
        if (span[0] == '-') i = 1;

        int start = i;
        while (i < span.Length && char.IsDigit(span[i])) i++;

        if (i == start) return null;                          // 没有数字
        if (i < span.Length && span[i] != ':') return null;   // 数字后面既不是 ':' 也不是结尾

        return span[..i].ToString();
    }
}
