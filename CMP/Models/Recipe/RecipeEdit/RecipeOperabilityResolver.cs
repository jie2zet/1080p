using System;
using System.Collections.Generic;

namespace CMP.Models.Recipe.RecipeEdit;

/// <summary>
/// 依据参数上的 <see cref="OperableRule"/> 计算每个步骤的可操作性。
/// 设计约束：
/// 1) 只按参数名匹配（归一化 + CompatibleName），不使用 Id / 下标；
/// 2) 任何"判不出来"的情况（源参数缺失 / 步数不齐 / 源值无法解析）一律 fail-open（视为可操作），
///    避免一条脏数据把整张表锁成只读。
/// </summary>
public static class RecipeOperabilityResolver
{
    /// <summary>
    /// 重算 Recipe 中所有带规则参数的每一步可操作性。
    /// 结构性变化（载入 / 增删步 / 粘贴 / 列重排）后必须调用。
    /// </summary>
    public static void Recalculate(Recipe.Content? content)
    {
        var dataList = content?.Data;
        if (dataList == null || dataList.Count == 0) return;

        var index = BuildIndex(dataList);

        foreach (var target in dataList)
        {
            var rule = target?.OperableRule;
            if (rule == null || target!.Steps == null) continue;

            for (int i = 0; i < target.Steps.Count; i++)
            {
                var step = target.Steps[i];
                var operable = IsSatisfied(rule, index, i);

                step.IsOperable = operable;
                step.OperableReason = operable ? null : rule.Reason;
            }
        }
    }

    /// <summary>
    /// 规则是否满足 =（源参数该步取值 ∈ AllowValues）或（任一 <see cref="OperableRule.AnyOf"/> 子规则满足）。
    /// 任何判不出来的情况返回 true（fail-open）。
    /// </summary>
    private static bool IsSatisfied(OperableRule rule, Dictionary<string, Recipe.Data> index, int stepIndex)
    {
        if (IsConditionSatisfied(rule.SourceParameter, rule.AllowValues, index, stepIndex)) return true;
        if (rule.AnyOf == null) return false;

        foreach (var sub in rule.AnyOf)
        {
            if (sub != null && IsSatisfied(sub, index, stepIndex)) return true;
        }
        return false;
    }

    /// <summary>单个"某参数该步取值 ∈ 允许值集合"条件；判不出来时 fail-open。</summary>
    private static bool IsConditionSatisfied(
        string sourceParameter,
        IReadOnlyList<string> allowValues,
        Dictionary<string, Recipe.Data> index,
        int stepIndex)
    {
        // 允许值集合为空/缺失视为无限制（fail-open，避免把整列锁死）
        if (allowValues == null || allowValues.Count == 0) return true;

        if (!index.TryGetValue(Normalize(sourceParameter), out var source) || source.Steps == null)
            return true;                                        // 源参数缺失 -> fail-open

        if (stepIndex < 0 || stepIndex >= source.Steps.Count)
            return true;                                        // 步数不齐 -> fail-open

        var value = RecipeOptionValue.NumericPrefix(source.Steps[stepIndex].Value);
        if (value == null)
            return true;                                        // 源值无法解析 -> fail-open

        return allowValues.Contains(value);
    }

    /// <summary>
    /// 归一化参数名：删除 空格 / _ / - / &amp;，与 PLC 侧 GetRealVal 的规则保持一致。
    /// </summary>
    public static string Normalize(string? name) =>
        (name ?? string.Empty).Replace(" ", "").Replace("_", "").Replace("-", "").Replace("&", "");

    /// <summary>
    /// 收集当前 Recipe 中所有规则的来源参数名（归一化后，含 AnyOf 子规则的来源），用于订阅变更。
    /// </summary>
    public static HashSet<string> GetSourceParameterNames(Recipe.Content? content)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        if (content?.Data == null) return names;

        foreach (var data in content.Data)
        {
            var rule = data?.OperableRule;
            if (rule == null) continue;

            CollectSourceNames(rule, names);
        }
        return names;
    }

    private static void CollectSourceNames(OperableRule rule, HashSet<string> names)
    {
        var key = Normalize(rule.SourceParameter);
        if (key.Length > 0) names.Add(key);

        if (rule.AnyOf == null) return;
        foreach (var sub in rule.AnyOf)
        {
            if (sub != null) CollectSourceNames(sub, names);
        }
    }

    /// <summary>参数名或任一 CompatibleName 命中给定名称集合。</summary>
    public static bool MatchesAnyName(Recipe.Data? data, IReadOnlySet<string> names)
    {
        if (data == null || names == null || names.Count == 0) return false;
        if (names.Contains(Normalize(data.Name))) return true;
        if (data.CompatibleName == null) return false;

        foreach (var alias in data.CompatibleName)
        {
            if (names.Contains(Normalize(alias))) return true;
        }
        return false;
    }

    private static Dictionary<string, Recipe.Data> BuildIndex(IEnumerable<Recipe.Data> dataList)
    {
        var index = new Dictionary<string, Recipe.Data>(StringComparer.Ordinal);

        foreach (var data in dataList)
        {
            if (data == null) continue;

            var key = Normalize(data.Name);
            if (key.Length > 0) index.TryAdd(key, data);

            if (data.CompatibleName == null) continue;
            foreach (var alias in data.CompatibleName)
            {
                var aliasKey = Normalize(alias);
                if (aliasKey.Length > 0) index.TryAdd(aliasKey, data);
            }
        }
        return index;
    }
}
