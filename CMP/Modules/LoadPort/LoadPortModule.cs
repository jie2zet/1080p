using Prism.Ioc;
using Prism.Modularity;

namespace CMP.Modules.LoadPort
{
    /// <summary>
    /// LoadPort模块
    /// </summary>
    public class LoadPortModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 注册LoadPort服务
           // containerRegistry.RegisterSingleton<ILoadPortService, LoadPortService>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            // 模块初始化逻辑
        }
    }
}

