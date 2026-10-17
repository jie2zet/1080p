using CMP.Models.Recipe.RecipeEdit;

namespace CMP.ViewModels.Tabs.RecipeTab;

/// <summary>
/// Recipe 编辑器的行级可见性。
/// 语义：Excel「不使用」列 = 1 的参数行（<see cref="Recipe.Data.IsUsed"/> = false）**不显示**。
/// 实现方式：给 DataGrid 绑定集合的默认 CollectionView 设置过滤谓词 —— 只影响显示，
/// <c>RecipeData.Data</c> 本身不被改动，行数据仍保留、照常落盘与下发。
/// </summary>
public static class RecipeRowVisibility
{
    /// <summary>CollectionView 过滤谓词：保留启用（IsUsed）的行。</summary>
    public static bool Filter(object item) => item is not Recipe.Data data || data.IsUsed;
}
