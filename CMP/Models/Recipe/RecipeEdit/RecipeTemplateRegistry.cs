using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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

    /// <summary>Pen Recipe 文件夹名。</summary>
    public const string PenRecipeFolder = "Pen Recipe";

    /// <summary>Roll Recipe 文件夹名。</summary>
    public const string RollRecipeFolder = "Roll Recipe";

    /// <summary>Polish Recipe 文件夹名。</summary>
    public const string PolishRecipeFolder = "Polish Recipe";

    /// <summary>Head Sweep Recipe 文件夹名。</summary>
    public const string HeadSweepRecipeFolder = "Head Sweep Recipe";

    /// <summary>Load Recipe 文件夹名。</summary>
    public const string LoadRecipeFolder = "Load Recipe";

    /// <summary>Load Retry Recipe 文件夹名。</summary>
    public const string LoadRetryRecipeFolder = "Load Retry Recipe";

    /// <summary>UnLoad Recipe 文件夹名。</summary>
    public const string UnloadRecipeFolder = "UnLoad Recipe";

    /// <summary>UnLoad Retry Recipe 文件夹名。</summary>
    public const string UnloadRetryRecipeFolder = "UnLoad Retry Recipe";

    /// <summary>Dechuck Recipe 文件夹名。</summary>
    public const string DechuckRecipeFolder = "Dechuck Recipe";

    /// <summary>Dechuck Retry Recipe 文件夹名。</summary>
    public const string DechuckRetryRecipeFolder = "Dechuck Retry Recipe";

    /// <summary>InsituCondition Recipe 文件夹名。</summary>
    public const string InsituConditionRecipeFolder = "InsituCondition Recipe";

    /// <summary>ExsituCondition Recipe 文件夹名（独立模板，模式参数仅 25:ExsituCondition 与 -1:Recipe End）。</summary>
    public const string ExsituConditionRecipeFolder = "ExsituCondition Recipe";

    /// <summary>InsituCondition Sweep Recipe 文件夹名（独立模板）。</summary>
    public const string InsituConditionSweepRecipeFolder = "InsituCondition Sweep Recipe";

    /// <summary>ExsituCondition Sweep Recipe 文件夹名（独立模板）。</summary>
    public const string ExsituConditionSweepRecipeFolder = "ExsituCondition Sweep Recipe";

    private static readonly Dictionary<string, Func<ObservableCollection<Recipe.Data>>> Factories =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [DryRecipeFolder] = () => DryRecipeTemplate.Template,
            [PenRecipeFolder] = () => PenA1RecipeTemplate.Template,
            [RollRecipeFolder] = () => RollRecipeTemplate.Template,
            [PolishRecipeFolder] = () => PolishRecipeTemplate.Template,
            [HeadSweepRecipeFolder] = () => HeadSweepRecipeTemplate.Template,
            [LoadRecipeFolder] = () => LoadRecipeTemplate.Template,
            [LoadRetryRecipeFolder] = () => LoadRetryRecipeTemplate.Template,
            [UnloadRecipeFolder] = () => UnloadRecipeTemplate.Template,
            [UnloadRetryRecipeFolder] = () => UnloadRetryRecipeTemplate.Template,
            [DechuckRecipeFolder] = () => DechuckRecipeTemplate.Template,
            [DechuckRetryRecipeFolder] = () => DechuckRetryRecipeTemplate.Template,
            [InsituConditionRecipeFolder] = () => InsituConditionRecipeTemplate.Template,
            [ExsituConditionRecipeFolder] = () => ExsituConditionRecipeTemplate.Template,
            [InsituConditionSweepRecipeFolder] = () => InsituContidionSweepRecipeTemplate.Template,
            [ExsituConditionSweepRecipeFolder] = () => ExsituContidionSweepRecipeTemplate.Template,
        };

    /// <summary>
    /// 已注册的全部 Recipe 文件夹名（只读快照）。
    /// 用于首次运行 / 新设备上创建缺失的目录 —— 这些机器初始状态下
    /// <c>D:\CMP\Configuration\Recipes\UnitRecipe\</c> 下可能一个子目录都没有。
    /// </summary>
    public static IReadOnlyList<string> RegisteredFolders { get; } = Factories.Keys.ToArray();

    /// <summary>
    /// 在 <paramref name="basePath"/> 下补齐全部已注册的 Recipe 文件夹（幂等，已存在的跳过）。
    /// </summary>
    /// <returns>本次**新建**的文件夹名；全部已存在时返回空集合。</returns>
    public static IReadOnlyList<string> EnsureFoldersExist(string? basePath)
    {
        var created = new List<string>();
        if (string.IsNullOrWhiteSpace(basePath)) return created;

        Directory.CreateDirectory(basePath);

        foreach (var folder in RegisteredFolders)
        {
            var path = Path.Combine(basePath, folder);
            if (Directory.Exists(path)) continue;

            Directory.CreateDirectory(path);
            created.Add(folder);
        }
        return created;
    }

    /// <summary>取文件夹对应的模板；未注册返回 null（调用方据此 fail-open）。</summary>
    public static ObservableCollection<Recipe.Data>? Resolve(string? folderName)
    {
        if (string.IsNullOrWhiteSpace(folderName)) return null;
        return Factories.TryGetValue(folderName.Trim(), out var factory) ? factory() : null;
    }

    /// <summary>
    /// 把模板上的规格元数据（<see cref="Recipe.Data.OperableRule"/> 与 <see cref="Recipe.Data.IsUsed"/>）
    /// 按归一化参数名 + CompatibleName 贴到已载入的 Recipe 行上。
    /// 存量 JSON 使用 legacy 参数名，靠 CompatibleName 命中。
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
            row.IsUsed = spec.IsUsed;
            decorated = true;
        }
        return decorated;
    }
}
