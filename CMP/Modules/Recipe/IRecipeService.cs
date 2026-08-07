using System.Collections.Generic;
using System.Threading.Tasks;
using CMP.Modules.Recipe.Models;

namespace CMP.Modules.Recipe
{
    /// <summary>
    /// Recipe服务接口
    /// </summary>
    public interface IRecipeService
    {
        /// <summary>
        /// 创建Recipe
        /// </summary>
        Task<RecipeModel> CreateRecipeAsync(RecipeTemplate template);

        /// <summary>
        /// 更新Recipe
        /// </summary>
        Task<bool> UpdateRecipeAsync(RecipeModel recipe);

        /// <summary>
        /// 删除Recipe
        /// </summary>
        Task<bool> DeleteRecipeAsync(string recipeName);

        /// <summary>
        /// 加载Recipe
        /// </summary>
        Task<RecipeModel> LoadRecipeAsync(string recipePath);

        /// <summary>
        /// 获取所有Recipe
        /// </summary>
        Task<IEnumerable<RecipeModel>> GetAllRecipesAsync();

        /// <summary>
        /// 分配Recipe到PLC
        /// </summary>
        Task<bool> AssignRecipeToPlcAsync(Wafer wafer, RecipeModel recipe);

        /// <summary>
        /// 获取Sequence Recipe
        /// </summary>
        Task<SequenceRecipeData> GetSequenceRecipeAsync(string recipeName);
    }
}

