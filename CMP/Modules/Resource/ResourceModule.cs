using Prism.Ioc;
using Prism.Modularity;

namespace CMP.Modules.Resource
{
    /// <summary>
    /// 资源模块
    /// </summary>
    public class ResourceModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 注册资源服务
           // containerRegistry.RegisterSingleton<IResourceService, ResourceService>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            // 模块初始化逻辑
        }
    }
}

