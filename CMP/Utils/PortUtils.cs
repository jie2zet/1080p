using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Uitls
{
    public class PortUtils
    {
        public static List<int> BaudrateList;

        public static List<int> DatabitsList;

        public static List<string> ParityList = Enum.GetNames(typeof(Parity)).ToList();

        public static List<string> StopBitsList = Enum.GetNames(typeof(StopBits)).ToList();

        public enum TextEncode
        {
            ASCII   = 0,
            HEX     = 1,
        }


        static PortUtils()
        {
            BaudrateList = new List<int>(){  
                2400,4800,9600,19200,14400,38400,56000,57600,115200, 128000,230400,256000,460800,500000,512000 
            };

            DatabitsList = new List<int>()
            {
                5,6,7,8
            };
        }

        /// <summary>
        /// 获取系统可连接串口
        /// 不带驱动设备名称
        /// </summary>
        /// <returns>string[]</returns>
        public static string[] GetSystemPorts()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// 获取系统可连接串口列表
        /// 不带驱动设备名称
        /// </summary>
        /// <returns>list[]</returns>
        public static List<string> GetSystemPortsList()
        {
            List<string> ports = new List<string>();
            foreach (string com in GetSystemPorts())
            {
                ports.Add(com);
            }
            return ports;
        }

        /// <summary>
        /// 获取系统可连接串口列表
        /// 带驱动设备名称
        /// </summary>
        /// <returns>list[]</returns>
        public static List<string> GetSystemDevicePorts()
        {
            List<string> ports = new List<string>();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_PnPEntity where Name like '%(COM%'"))
            {
                var hardInfos = searcher.Get();
                foreach (var hardInfo in hardInfos)
                {
                    if (hardInfo.Properties["Name"].Value != null)
                    {
                        // ELTIMA Virtual Serial Port (COM11->COM21)

                        string deviceName = (string)hardInfo.Properties["Name"].Value;
                        int startIndex = deviceName.IndexOf("(");
                        int endIndex = deviceName.IndexOf(")");
                        string key = deviceName.Substring(startIndex + 1, deviceName.Length - startIndex - 2);
                        string name = deviceName.Substring(0, startIndex - 1);
                        //Console.WriteLine("key:" + key + ",name:" + name + ",deviceName:" + deviceName);
                        //ports.Add(key, name);
                        ports.Add($"{key} ({name})");
                    }
                }
            }

            return ports;

        }

        /// <summary>
        /// 获取系统可连接串口列表
        /// 带驱动设备名称
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSystemDevicePorts(ref Dictionary<string, string> dic)
        {
            dic.Clear();
            List<string> ports = new List<string>();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_PnPEntity where Name like '%(COM%'"))
            {
                var hardInfos = searcher.Get();
                foreach (var hardInfo in hardInfos)
                {
                    if (hardInfo.Properties["Name"].Value != null)
                    {
                        // ELTIMA Virtual Serial Port (COM11->COM21)

                        string deviceName = (string)hardInfo.Properties["Name"].Value;
                        int startIndex = deviceName.IndexOf("(");
                        int endIndex = deviceName.IndexOf(")");
                        string key = deviceName.Substring(startIndex + 1, deviceName.Length - startIndex - 2);
                        string name = deviceName.Substring(0, startIndex - 1);
                        //Console.WriteLine("key:" + key + ",name:" + name + ",deviceName:" + deviceName);
                        dic.Add(key, name);
                        ports.Add($"{key} ({name})");
                    }
                }
            }

            return ports;

        }
        public static ushort GetCheckCode(byte[] buf, int nEnd)
        {
            ushort crc = 0xffff;
            int i, j;
            for (i = 0; i < nEnd; i++)
            {
                crc ^= buf[i];
                for (j = 0; j < 8; j++)
                {
                    if ((crc & 1) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                        crc >>= 1;
                }
            }
            return crc;
        }

    }
}
