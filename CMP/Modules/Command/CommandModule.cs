using Prism.Ioc;
using Prism.Modularity;

namespace CMP.Modules.Command
{
    /// <summary>
    /// 命令模块
    /// </summary>
    public class CommandModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 注册命令服务
            //containerRegistry.RegisterSingleton<ICommandService, CommandService>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            // 模块初始化逻辑
        }
    }
}

