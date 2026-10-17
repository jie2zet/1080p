using System.Collections.Generic;

namespace CMP.Models.Recipe.RecipeEdit;

/// <summary>
/// 参数可操作性规则：当 <see cref="SourceParameter"/> 在同一个步骤上的取值属于
/// <see cref="AllowValues"/>，或任一 <see cref="AnyOf"/> 子规则满足时，本参数在该步骤上可操作。
/// 规则真源是 Recipe 规格 Excel 的「说明」列，由模板承载，不落盘。
/// </summary>
/// <param name="SourceParameter">规则来源参数名，例："Process Type"。</param>
/// <param name="AllowValues">允许的选项数字前缀集合，例：["1", "3"]。</param>
/// <param name="Reason">「说明」列原文，仅在不可操作时作为 ToolTip 显示。</param>
/// <param name="AnyOf">备用条件（**或**语义）：本规则满足 = 自身条件满足 或 任一子规则满足。
/// 用于 Pen A1 的跨参数条件，例："WF Upper Face Chemical Supply=1 或 WF Lower Face Chemical Supply=1 时允许"。</param>
public sealed record OperableRule(
    string SourceParameter,
    IReadOnlyList<string> AllowValues,
    string? Reason = null,
    IReadOnlyList<OperableRule>? AnyOf = null);

/// <summary>
/// Recipe 选项取值的统一解析工具。
/// 选项在配方中以 "1: Dry With Arm" 或 "1" 两种形态存在，本类只负责取出数字前缀。
/// **兼容全角冒号**：Excel 由人工维护，中文输入法下极易把 `:` 打成 `：`(U+FF1A)，
/// 若不放宽会导致解析静默失败（门控失效 / PLC 取值落空）。
/// </summary>
public static class RecipeOptionValue
{
    /// <summary>
    /// 数字前缀与文字之间允许的分隔符：
    /// 半角冒号 `:`、全角冒号 `：`(U+FF1A)、小冒号 `﹕`(U+FE55)、比例号 `∶`(U+2236)。
    /// </summary>
    private static readonly char[] Separators = [':', '\uFF1A', '\uFE55', '\u2236'];

    /// <summary>
    /// 取出选项的数字前缀；无法解析时返回 null。
    /// "1: Dry With Arm" -> "1"；"1：Dry With Arm"（全角）-> "1"；"1" -> "1"；
    /// "-1: Recipe End" -> "-1"；"" -> null；"abc" -> null。
    /// </summary>
    public static string? NumericPrefix(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var span = value.AsSpan().Trim();

        int i = 0;
        if (span[0] == '-') i = 1;

        int start = i;
        while (i < span.Length && char.IsDigit(span[i])) i++;

        if (i == start) return null;                                      // 没有数字
        if (i < span.Length && !IsSeparator(span[i])) return null;         // 数字后面既不是分隔符也不是结尾

        return span[..i].ToString();
    }

    /// <summary>
    /// 若值中存在分隔符（半角或全角冒号），返回**第一个**分隔符之后的部分；否则原样返回。
    /// 用于把 "1:HeadSweep1" / "1：HeadSweep1" 显示为 "HeadSweep1"。
    /// </summary>
    public static string PartAfterSeparator(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        var index = value.IndexOfAny(Separators);
        return index >= 0 ? value[(index + 1)..] : value;
    }

    /// <summary>
    /// 取选项的**数字部分**（下发给 PLC 用）："1: DIW" / "1：DIW" -> "1"；无数字前缀时返回去空白后的原文。
    /// 供各 Operation Tab 的 <c>GetRealVal</c> 统一使用，避免各处重复且脆弱的 <c>Split(':')</c>。
    /// </summary>
    public static string NumericPart(string? value) => NumericPrefix(value) ?? (value ?? string.Empty).Trim();

    /// <summary>
    /// 取选项的**标签部分**（如子配方文件名）："1: HeadSweep1" / "1：HeadSweep1" -> "HeadSweep1"；
    /// 无分隔符时返回去空白后的原文。
    /// </summary>
    public static string LabelPart(string? value) => PartAfterSeparator(value).Trim();

    private static bool IsSeparator(char c) => Array.IndexOf(Separators, c) >= 0;
}
