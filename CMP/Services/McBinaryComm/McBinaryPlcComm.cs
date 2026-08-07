using System;
using HslCommunication;
using CMP.Services.Log;
using CMP.Services.Plc;

namespace CMP.Services.McBinaryComm
{
    /// <summary>
    /// 兼容旧入口：统一转发到 MitsubishiPlcService，避免维护两套三菱PLC连接实现。
    /// </summary>
    public class McBinaryPlcComm
    {
        private static readonly Lazy<McBinaryPlcComm> _instance = new(() => new McBinaryPlcComm());

        public bool IsPLCConnected = false;

        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static McBinaryPlcComm Instance => _instance.Value;

        private readonly MitsubishiPlcService _plcService;
        public static readonly string IpAddressInputWrong = "IpAddress input wrong";

        // 私有构造函数，防止外部实例化
        private McBinaryPlcComm()
        {
            _plcService = MitsubishiPlcService.Main;
            IsPLCConnected = _plcService.IsConnected;
            _plcService.ConnectionStateChanged += (_, connected) => { IsPLCConnected = connected; };
        }
        
    }
}