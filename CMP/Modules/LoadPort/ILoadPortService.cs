using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CMP.Modules.LoadPort.Models;

namespace CMP.Modules.LoadPort
{
    /// <summary>
    /// LoadPort服务接口
    /// </summary>
    public interface ILoadPortService
    {
        /// <summary>
        /// 异步连接LoadPort
        /// </summary>
        Task<bool> ConnectAsync(string portName);

        /// <summary>
        /// 异步断开连接
        /// </summary>
        Task<bool> DisconnectAsync(string portName);

        /// <summary>
        /// 获取LoadPort状态
        /// </summary>
        Task<LoadPortStatus> GetStatusAsync(string portName);

        /// <summary>
        /// 获取所有LoadPort
        /// </summary>
        Dictionary<string, LoadPortData> GetAllLoadPorts();

        /// <summary>
        /// 获取指定LoadPort
        /// </summary>
        LoadPortData GetLoadPort(string portName);

        /// <summary>
        /// 异步初始化所有LoadPort
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// 异步连接所有LoadPort
        /// </summary>
        Task ConnectAllAsync();

        /// <summary>
        /// 分配Recipe到Wafer
        /// </summary>
        Task<bool> AssignRecipeAsync(Wafer wafer, string recipeName);

        /// <summary>
        /// 所有选中Wafer完成事件
        /// </summary>
        event Action<LoadPortData, List<Wafer>> AllSelectedWafersCompleted;

        /// <summary>
        /// 启动Wafer监控
        /// </summary>
        void StartWaferMonitoring();
    }
}

