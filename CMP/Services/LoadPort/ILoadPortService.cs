using CMP.Models.Operation.LoadPort;
using CMP.Models.Resource;
using DocumentFormat.OpenXml.Spreadsheet;
using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CMP.ViewModels.Tabs.OperationTab.LoadPortContentViewModel;

namespace CMP.Services.LoadPort
{
    /// <summary>
    /// 串口服务接口
    /// </summary>
    public interface ILoadPortService : IDisposable
    {
        List<Wafer>ParseMappingResponse(string response);

        WaferStatus MapCharToWaferStatus(Char status);
        LoadPortStatus ParseStateResponse(string response);
        SerialResponseEventArgs ParseResponse(byte[] data, string msg);

        /// <summary>
        /// 发送MOV命令
        /// </summary>
        /// <param name="action">动作指令</param>
        /// <returns>是否成功发送</returns>
        Task<bool> SendMoveCommandAsync(string action,SerialPort serialPort);

        /// <summary>
        /// 发送SET命令
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <param name="value">值</param>
        /// <returns>是否成功发送</returns>
        Task<bool> SendSetCommandAsync(string parameter, string value, SerialPort serialPort);

        /// <summary>
        /// 发送GET命令
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <returns>返回的数据</returns>
        Task<bool> SendGetCommandAsync(string parameter, SerialPort serialPort);     

        /// <summary>
        /// 响应接收事件
        /// </summary>
        event EventHandler<SerialResponseEventArgs> ResponseReceived;  
         

       
        
    }
    
    /// <summary>
    /// 串口响应事件参数
    /// </summary>
    public class SerialResponseEventArgs : EventArgs
    {
        public string Command { get; set; }
        public string Response { get; set; }
        public bool IsSuccess { get; set; }
        public MessageType MessageType { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
    public enum MessageType
    {
        Unknown,
        ACK,
        NAK,
        INF,
        ABS
    }
}

