using Prism.Ioc;
using Prism.Modularity;

namespace CMP.Modules.Alarm
{
    /// <summary>
    /// 报警模块
    /// </summary>
    public class AlarmModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 注册报警服务
           // containerRegistry.RegisterSingleton<IAlarmService, AlarmService>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            // 模块初始化逻辑
        }
    }
}

