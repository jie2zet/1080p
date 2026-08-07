using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Services.McBinaryComm
{
    /// <summary>
    /// PLC地址常量定义类
    /// 包含所有与Loadport通信相关的PLC地址
    /// </summary>
    public static class PLCAddress
    {

        public const string IP_ADDRESS = "192.168.2.50";

        public const int PORT = 7002;
        /// <summary>
        /// Loadport准备状态地址 - 当Loadport完成扫描并准备好时置位
        /// </summary>
        public const string LOADPORT_READY = "M6879";

        /// <summary>
        /// 取料请求地址 - PLC请求从Loadport取料时置位
        /// </summary>
        public const string PICK_REQUEST = "M2560";

        /// <summary>
        /// 允许取料地址 - Loadport允许PLC取料时置位
        /// </summary>
        public const string PICK_ALLOW = "M6880";

        /// <summary>
        /// 取料中地址 - Loadport允许PLC取料时置位
        /// </summary>
        public const string PICKING = "M2561";

        /// <summary>
        /// 取料完成地址 - PLC完成取料操作后置位
        /// </summary>
        public const string PICK_COMPLETE = "M2563";

        /// <summary>
        /// 放料请求地址 - PLC请求向Loadport放料时置位
        /// </summary>
        public const string PLACE_REQUEST = "M2564";

        /// <summary>
        /// 允许放料地址 - Loadport允许PLC放料时置位
        /// </summary>
        public const string PLACE_ALLOW = "M6884";

        /// <summary>
        /// 放料中地址 - Loadport允许PLC放料时置位
        /// </summary>
        public const string PLACEING = "M2565";


        /// <summary>
        /// 放料完成地址 - PLC完成放料操作后置位
        /// </summary>
        public const string PLACE_COMPLETE = "M2567";

        /// <summary>
        /// Loadport错误状态地址 - Loadport发生错误时置位
        /// </summary>
        public const string LOADPORT_ERROR = "M6886";

        /// <summary>
        /// FOUP门开状态地址 - FOUP门完全打开时置位
        /// </summary>
        public const string FOUP_DOOR_OPEN = "M6887";

        /// <summary>
        /// FOUP门关状态地址 - FOUP门完全关闭时置位
        /// </summary>
        public const string FOUP_DOOR_CLOSE = "M6888";

        /// <summary>
        /// 天车到达信号地址 - 天车到达Loadport位置时置位
        /// </summary>
        public const string OVERHEAD_CRANE_ARRIVED = "M6889";

        /// <summary>
        /// FOUP夹取状态地址 - FOUP被夹取时置位
        /// </summary>
        public const string FOUP_CLAMPED = "M6890";

        /// <summary>
        /// FOUP放置状态地址 - FOUP被放置时置位
        /// </summary>
        public const string FOUP_PLACED = "M6891";

        /// <summary>
        /// 料盒号地址 
        /// </summary>
        public const string BOX_NUMBER = "D1800";

        /// <summary>
        /// 料盒层数地址 
        /// </summary>
        public const string BOX_TIER = "D1801";
    }
}
