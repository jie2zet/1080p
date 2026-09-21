using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Services.HostCommunication
{
    /// <summary>
    /// 主机通信服务
    /// </summary>
    public interface IHostCommunicationService
    {
        
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>

        void SendMessge(string data);

    }
}
