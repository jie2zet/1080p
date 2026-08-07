using System.Collections.Generic;
using System.Threading.Tasks;
using CMP.Modules.Resource.Models;

namespace CMP.Modules.Resource
{
    /// <summary>
    /// 资源服务接口
    /// </summary>
    public interface IResourceService
    {
        /// <summary>
        /// 获取资源
        /// </summary>
        T GetResource<T>(string name) where T : IDevice;

        /// <summary>
        /// 获取所有资源
        /// </summary>
        IEnumerable<T> GetAllResources<T>() where T : IDevice;

        /// <summary>
        /// 异步初始化设备
        /// </summary>
        Task<bool> InitializeDeviceAsync(string deviceName);

        /// <summary>
        /// 获取设备状态
        /// </summary>
        Task<DeviceStatus> GetDeviceStatusAsync(string deviceName);

        /// <summary>
        /// 获取所有机器人
        /// </summary>
        IEnumerable<RoboticArm> GetAllRobots();

        /// <summary>
        /// 获取所有腔室
        /// </summary>
        IEnumerable<Chamber> GetAllChambers();
    }
}

