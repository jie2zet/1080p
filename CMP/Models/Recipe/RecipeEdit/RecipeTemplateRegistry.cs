using System.Collections.ObjectModel;

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

    /// <summary>取文件夹对应的模板；未注册返回 null（调用方据此 fail-open）。</summary>
    public static ObservableCollection<Recipe.Data>? Resolve(string? folderName) => null;

    /// <summary>
    /// 把模板上的规格元数据按归一化参数名贴到已载入的 Recipe 行上。
    /// </summary>
    /// <returns>是否至少贴中一行。</returns>
    public static bool Decorate(Recipe.Content? content, string? folderName) => false;
}
