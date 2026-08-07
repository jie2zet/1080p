using Prism.Ioc;
using Prism.Modularity;

namespace CMP.Modules.PlcCommunication
{
    /// <summary>
    /// PLC通讯模块
    /// </summary>
    public class PlcCommunicationModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 注册PLC通讯服务
           // containerRegistry.RegisterSingleton<IPlcCommunicationService, McBinaryPlcService>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            // 模块初始化逻辑
        }
    }
}

