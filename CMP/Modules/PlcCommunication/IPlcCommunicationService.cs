using System;
using System.Threading.Tasks;

namespace CMP.Modules.PlcCommunication
{
    /// <summary>
    /// PLC通讯服务接口
    /// </summary>
    public interface IPlcCommunicationService
    {
        /// <summary>
        /// 是否已连接
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 连接状态变化事件
        /// </summary>
        event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        /// <summary>
        /// 异步连接PLC
        /// </summary>
        Task<bool> ConnectAsync(string ipAddress, int port);

        /// <summary>
        /// 异步断开连接
        /// </summary>
        Task<bool> DisconnectAsync();

        /// <summary>
        /// 异步读取数据
        /// </summary>
        Task<T> ReadAsync<T>(string address);

        /// <summary>
        /// 异步写入数据
        /// </summary>
        Task<bool> WriteAsync<T>(string address, T value);

        /// <summary>
        /// 读取Bool变量
        /// </summary>
        Task<bool> ReadBoolAsync(string address);

        /// <summary>
        /// 写入Bool变量
        /// </summary>
        Task<bool> WriteBoolAsync(string address, bool value);

        /// <summary>
        /// 读取Short变量
        /// </summary>
        Task<short> ReadShortAsync(string address);

        /// <summary>
        /// 写入Short变量
        /// </summary>
        Task<bool> WriteShortAsync(string address, short value);

        /// <summary>
        /// 读取Int变量
        /// </summary>
        Task<int> ReadIntAsync(string address);

        /// <summary>
        /// 写入Int变量
        /// </summary>
        Task<bool> WriteIntAsync(string address, int value);

        /// <summary>
        /// 写入字节数组
        /// </summary>
        Task<bool> WriteByteArrayAsync(string address, byte[] data);
    }

    /// <summary>
    /// 连接状态变化事件参数
    /// </summary>
    public class ConnectionStatusChangedEventArgs : EventArgs
    {
        public bool IsConnected { get; set; }
        public string Message { get; set; }
    }
}

