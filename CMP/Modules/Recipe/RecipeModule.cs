using Prism.Ioc;
using Prism.Modularity;

namespace CMP.Modules.Recipe
{
    /// <summary>
    /// Recipe模块
    /// </summary>
    public class RecipeModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 注册Recipe服务
            //containerRegistry.RegisterSingleton<IRecipeService, RecipeService>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            // 模块初始化逻辑
        }
    }
}

