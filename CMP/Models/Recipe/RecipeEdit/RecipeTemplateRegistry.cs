using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CMP.Models.Recipe.RecipeEdit.RecipeParameterTemplate;

namespace CMP.Models.Recipe.RecipeEdit;

/// <summary>
/// Recipe 文件夹名 -> 参数模板 的注册表。
/// 本工程的既有约定是「文件夹名 = Recipe 类型」，见 Chamber.ResolveUnitRecipePath
/// 与 SequenceRecipeEditorViewModel 的目录常量。
/// </summary>
public static class RecipeTemplateRegistry
{
    /// <summary>Dry Recipe 文件夹名。</summary>
    public const string DryRecipeFolder = "Dry Recipe";

    private static readonly Dictionary<string, Func<ObservableCollection<Recipe.Data>>> Factories =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [DryRecipeFolder] = () => DryRecipeTemplate.Template,
        };

    /// <summary>取文件夹对应的模板；未注册返回 null（调用方据此 fail-open）。</summary>
    public static ObservableCollection<Recipe.Data>? Resolve(string? folderName)
    {
        if (string.IsNullOrWhiteSpace(folderName)) return null;
        return Factories.TryGetValue(folderName.Trim(), out var factory) ? factory() : null;
    }

    /// <summary>
    /// 把模板上的规格元数据（当前只有 <see cref="Recipe.Data.OperableRule"/>）按归一化参数名 +
    /// CompatibleName 贴到已载入的 Recipe 行上。存量 JSON 使用 legacy 参数名，靠 CompatibleName 命中。
    /// 只读模板，不修改模板内容；未注册的文件夹不做任何事。
    /// </summary>
    /// <returns>是否至少贴中一行。</returns>
    public static bool Decorate(Recipe.Content? content, string? folderName)
    {
        var template = Resolve(folderName);
        if (template == null || content?.Data == null || content.Data.Count == 0) return false;

        var templateIndex = new Dictionary<string, Recipe.Data>(StringComparer.Ordinal);
        foreach (var item in template)
        {
            if (item == null) continue;

            var key = RecipeOperabilityResolver.Normalize(item.Name);
            if (key.Length > 0) templateIndex.TryAdd(key, item);

            if (item.CompatibleName == null) continue;
            foreach (var alias in item.CompatibleName)
            {
                var aliasKey = RecipeOperabilityResolver.Normalize(alias);
                if (aliasKey.Length > 0) templateIndex.TryAdd(aliasKey, item);
            }
        }

        var decorated = false;
        foreach (var row in content.Data)
        {
            if (row == null) continue;
            if (!templateIndex.TryGetValue(RecipeOperabilityResolver.Normalize(row.Name), out var spec)) continue;

            row.OperableRule = spec.OperableRule;
            decorated = true;
        }
        return decorated;
    }
}
