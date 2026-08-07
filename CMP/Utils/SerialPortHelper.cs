using CMP.Uitls;
using HandyControl.Controls;
using Nini.Config;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Windows.Interop;

namespace CMP.Utils
{
    public class SerialPortHelper:SerialPort   {
        private byte led_num_0 = 3;


        private byte slaveID = 1;

        private byte[] LedIndex;
        /// <summary>
        /// Led 数码管 可显示点位数量集合
        /// key: led_index, value: num
        /// </summary>
        private Dictionary<byte, byte> LedPointNumMap = new Dictionary<byte, byte>();

        private int LedPortNum = 0;


        public delegate void WriteHexHandler(byte[] code);
        public delegate void WriteStrHandler(string code);
        public delegate void SendHexHandler(byte[] code);
        public delegate void SendStrHandler(string code);
        public delegate void ErrorHandler(string str);

        public event WriteHexHandler WriteHexHandlerHelper;
        public event WriteStrHandler WriteStrHandlerHelper;
        public event SendHexHandler SendHexHandlerHelper;
        public event SendStrHandler SendStrHandlerHelper;
        public event ErrorHandler ErrorHandlerHelper;

        IConfigSource source = null;
        public SerialPortHelper(string InvokeName){
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets\\config.ini");
                source = new IniConfigSource(path);
                IConfig config = source.Configs[InvokeName];
                this.PortName = config.Get("PortName");
                this.BaudRate = config.GetInt("BaudRate");
                this.DataBits = config.GetInt("DataBits", 8);
                this.Parity = (System.IO.Ports.Parity)config.GetInt("Parity", 0);
                this.StopBits = (System.IO.Ports.StopBits)config.GetInt("StopBits", 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show("串口连接异常{}", ex.Message);
            }

            base.ReceivedBytesThreshold = 1;
            base.DataReceived += SerialPort_DataReceived;

        }
        public SerialPortHelper() 
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets\\config.ini");
                source = new IniConfigSource(path);
                IConfig config = source.Configs["SerialPort"];
                this.PortName = config.Get("PortName");
                this.BaudRate = config.GetInt("BaudRate");
                this.DataBits = config.GetInt("DataBits", 8);
                this.Parity = (System.IO.Ports.Parity)config.GetInt("Parity", 0);
                this.StopBits = (System.IO.Ports.StopBits)config.GetInt("StopBits", 1);
            }
            catch (Exception ex) {
               MessageBox.Show("串口连接异常{}",ex.Message);
            }         

            base.ReceivedBytesThreshold = 1;
            base.DataReceived += SerialPort_DataReceived;

        }


        private bool stop_lock  = false;
        public void Stop()
        {
            this.stop_lock = true;
        }

        public bool DataReceived_Pause_lock = false;

        public PortUtils.TextEncode DataRecrived_Encode = PortUtils.TextEncode.HEX;
        public PortUtils.TextEncode DataSend_Encode = PortUtils.TextEncode.HEX;


        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

           
            byte[] buf = new byte[1024];
            int read = 0;
            if ((read = base.BytesToRead) != -1)
            {
                if (!this.IsOpen)
                {
                    return;
                }
                if (read > buf.Length)
                {
                    read = buf.Length;
                }
                int read_count = base.Read(buf, 0, read);
                if (read_count > 0)
                {
                    byte[] data = new byte[read_count];
                    Array.Copy(buf, 0, data, 0, read);
                    Console.WriteLine("####################3333" + data);
                    WriteHexHandlerHelper(data);
                    string msg = System.Text.ASCIIEncoding.ASCII.GetString(data);
                    Console.WriteLine("####################3333" + msg);
                    WriteStrHandlerHelper(msg);
                }
                
            }
        }

        public bool Send(String msg)
        {
            if (!base.IsOpen)
            {
                ErrorHandlerHelper("Port Close");
                return false;
            }
            if (msg?.Length == 0)
            {
                ErrorHandlerHelper("Text is null");
                return false;
            }
            if (this.DataSend_Encode.Equals(PortUtils.TextEncode.HEX))
            {
                try
                {
                    string[] buf = msg.Split(',', ' ', '-');
                    List<byte> data = new List<byte>();
                    if (buf.Length > 1)
                    {
                        foreach (string item in buf)
                        {
                            if (item?.Length == 0)
                            {
                                continue;
                            }
                            else
                            {
                                if (item.Length % 2 == 1)
                                {
                                    data.Add(Convert.ToByte(item.Substring(0, 1), 16));
                                    for (int i = 1; i < item.Length; i += 2)
                                    {
                                        data.Add(Convert.ToByte(item.Substring(i, 2), 16));
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < item.Length; i += 2)
                                    {
                                        data.Add(Convert.ToByte(item.Substring(i, 2), 16));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (buf[0].Length % 2 == 1)
                        {
                            data.Add(Convert.ToByte(buf[0].Substring(0, 1), 16));
                            for (int i = 1; i < buf[0].Length; i += 2)
                            {
                                data.Add(Convert.ToByte(buf[0].Substring(i, 2), 16));
                            }
                        }
                        else
                        {
                            for (int i = 0; i < buf[0].Length; i += 2)
                            {
                                data.Add(Convert.ToByte(buf[0].Substring(i, 2), 16));
                            }
                        }
                    }
                    base.Write(data.ToArray(), 0, data.Count);
                    this.SendHexHandlerHelper(data.ToArray());
                }
                catch (Exception ex)
                {
                    ErrorHandlerHelper(ex.Message);
                    return false;
                }
            }
            else
            {
                base.Write(msg);
            }
            return true;
        }
        /// <summary>
        /// 修改 LedPointNumMap
        /// </summary>
        /// <param name="led_index"></param>
        /// <returns></returns>
        public bool RefreshLEDMap(byte[] led_index, byte[] led_num)
        {
            Dictionary<byte, byte> buf = new Dictionary<byte, byte>();
            int num = 0;
            try
            {
                for (byte i = 0; i < led_index.Length; i++)
                {
                    if (led_num.Length - 1 <= i)
                    {
                        buf.Add(led_index[i], led_num[i]);
                    }
                    else
                    {
                        buf.Add(led_index[i], this.led_num_0);
                    }
                    num += buf[led_index[i]];
                }

                lock (this.LedPointNumMap) { 
                    this.LedPointNumMap.Clear();
                    this.LedPointNumMap = new Dictionary<byte, byte>(buf);
                    this.LedPortNum = num;
                }
                buf.Clear();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 设置自定义浮点数
        /// </summary>
        /// <param name="symbol">符号 1:负数，other：正数</param>
        /// <param name="point_index">小数位序号</param>
        /// <param name="value">值 <= 0xFFFFFF</param>
        /// <returns></returns>
        public bool WriteFloatValueOne(byte slaveId, byte symbol, byte point_index, int value)
        {

            byte[] code = new byte[13] {
                slaveId, 0x10, 
                0x00, 0x06, 0x00, 0x02, 0x04, 
                0x00, 0x00, 0x00, 0x00, 
                0x00, 0x00 };
            
            if (symbol == 1)
            {
                code[7] = (byte)(point_index % 10 + 10);
            }
            else
            {
                code[7] = (byte)(point_index % 10);
            }

            byte[] data = BitConverter.GetBytes(value & 0x00FFFFFF);
            code[8] = data[2];
            code[9] = data[1];
            code[10] = data[0];

            UInt16 crc = PortUtils.GetCheckCode(code, code.Length - 2);
            code[11] = (byte)(crc & 0xff);
            code[12] = (byte)(crc >> 8);

            bool ret = true;
            try
            {
                if (IsOpen)
                {
                    Write(code, 0, code.Length);
                    SendHexHandlerHelper(code);
                }
                else
                {
                    ret = false;
                }
            }
            catch (Exception e)
            {
                ret = false;
            }

            return ret;
        }

        /// <summary>
        /// 设置单个LED数码管自定义浮点数
        /// </summary>
        /// <param name="symbol">符号 1:负数，other：正数</param>
        /// <param name="point_index">小数位序号</param>
        /// <param name="value">值 <= 0xFFFFFF</param>
        /// <returns></returns>
        public bool WriteFloatValueOne(byte slaveId , byte symbol, byte point_index, byte[] value)
        {

            byte[] code = new byte[13] {
                slaveId, 0x10,
                0x00, 0x06, 0x00, 0x02, 0x04,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00 };

            if (symbol == 1)
            {
                code[7] = (byte)(point_index % 10 + 10);
            }
            else
            {
                code[7] = (byte)(point_index % 10);
            }

            //byte[] data = BitConverter.GetBytes(value & 0x00FFFFFF);
            code[8] = value[2];
            code[9] = value[1];
            code[10] = value[0];

            UInt16 crc = PortUtils.GetCheckCode(code, code.Length - 2);
            code[11] = (byte)(crc & 0xff);
            code[12] = (byte)(crc >> 8);

            bool ret = true;
            try
            {
                if (IsOpen)
                {
                    Write(code, 0, code.Length);
                    SendHexHandlerHelper(code);
                }
                else
                {
                    ret = false;
                }
            }
            catch (Exception e)
            {
                ret = false;
            }

            return ret;
        }

        /// <summary>
        /// 设置串联LED数码管自定义浮点数
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="point_index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteFloatValueOther(byte symbol, byte point_index, long value)
        {
            try
            {
                int int_num = value.ToString().Length;
                if (int_num > this.LedPortNum)
                {
                    return false;
                }

                byte[] bytData = BitConverter.GetBytes(value);

                int point_index_num = 0;
                int index_end = this.LedPortNum - 1;

                for (int i = this.LedIndex.Length - 1; i > 0; i--)
                {
                    point_index_num += this.LedPointNumMap[this.LedIndex[i]];
                    index_end -= this.LedPointNumMap[this.LedIndex[i]];

                    byte[] buf = new byte[this.LedPointNumMap[this.LedIndex[i]]];
                    //bytData.CopyTo(buf, bytData.Length - point_index_num);
                    //bytData.CopyTo(buf, bytData.Length - index_end);
                    Array.Copy(bytData, bytData.Length - point_index_num, buf, 0, buf.Length);
                    if (point_index_num >= this.LedPortNum)
                    {
                        if (point_index_num >= point_index)
                        {
                            this.WriteFloatValueOne(this.LedIndex[i], symbol, (byte)(point_index - this.LedPointNumMap[this.LedIndex[i]]), buf);
                        }
                        else
                        {
                            this.WriteFloatValueOne(this.LedIndex[i], symbol, 0, buf);
                        }
                    }
                    if (point_index_num >= point_index)
                    {
                        this.WriteFloatValueOne(this.LedIndex[i], 0, (byte)(point_index - this.LedPointNumMap[this.LedIndex[i]]), buf);
                    }
                    else
                    {
                        this.WriteFloatValueOne(this.LedIndex[i], 0, 0, buf);
                    }
                }

            }
            catch {
                return false;
            };
            return true;
        }

        /// <summary>
        /// 设置单个LED数码管显示数值
        /// </summary>
        /// <param name="slaveId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteStringValueOne(byte slaveId, string value)
        {
            byte[] bytData = ASCIIEncoding.ASCII.GetBytes(value);
            UInt16[] byt = new UInt16[bytData.Length];
            for (int i = 0; i<bytData.Length;i++)
            {
                byt[i] = bytData[i];
            }
            byte[] code = this.WriteMultipleRegisters(slaveId, 0x10, 0x0000, 3, byt);

            this.Write(code, 0, code.Length);
            SendHexHandlerHelper(code);
            return true;
        }

        /// <summary>
        /// 设置单个LED数码管显示数值
        /// </summary>
        /// <param name="slaveId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteStringValueOther(string value)
        {
            try
            {
                int int_num = value.ToString().Length;
                //if (int_num > this.LedPortNum)
                //{
                //    return false;
                //}

                byte[] bytData = ASCIIEncoding.ASCII.GetBytes(value);
                UInt16[] byt = new UInt16[bytData.Length];
                for (int i = 0; i < bytData.Length; i++)
                {
                    byt[i] = bytData[i];
                }

                int point_index_num = 0;
                int index_end = this.LedPortNum - 1;

                for (int i = this.LedIndex.Length - 1; i >= 0; i--)
                {
                    point_index_num += this.LedPointNumMap[this.LedIndex[i]];
                    index_end -= this.LedPointNumMap[this.LedIndex[i]];

                    UInt16[] buf = new UInt16[this.LedPointNumMap[this.LedIndex[i]]];
                    //byt.CopyTo(buf, byt.Length - index_end - 1);

                    Array.Copy(byt, byt.Length - point_index_num, buf, 0, buf.Length);
                    //byt.CopyTo(buf, byt.Length - index_end);
                    byte[] code = this.WriteMultipleRegisters(this.LedIndex[i], 0x10, 0x0000, (byte)(buf.Length), buf);

                    this.Write(code, 0, code.Length);
                    SendHexHandlerHelper(code);
                    Thread.Sleep(50);
                }

            }
            catch (Exception ex)
            {
                ErrorHandlerHelper(ex.Message);
                return false;
            };
            return true;
        }

        // 数字管操作
        // Modbus 寄存器连号写
        /// <summary>
        /// Modbus 寄存器连号写
        /// </summary>
        /// <param name="Addr">设备地址位</param>
        /// <param name="type">功能码</param>
        /// <param name="RegisterAddr">寄存器开始位</param>
        /// <param name="RegisterNum">寄存器数量</param>
        /// <param name="DataList">数据列表</param>
        /// <returns></returns>
        public byte[] WriteMultipleRegisters(byte slaveId, byte type, UInt16 RegisterAddr, UInt16 RegisterNum, UInt16[] DataList)
        {
            if (type != 0x10) return null;
            if (RegisterNum != DataList.Length) return null;

            byte[] bytData = new byte[9 + RegisterNum * 2];

            bytData[0] = slaveId;
            bytData[1] = (byte)type;
            bytData[2] = (byte)(RegisterAddr >> 8);           //数据个数高位
            bytData[3] = (byte)(RegisterAddr & 0xFF);         //数据个数低位;
            bytData[4] = (byte)(RegisterNum >> 8);           //数据个数高位
            bytData[5] = (byte)(RegisterNum & 0xFF);         //数据个数低位;
            bytData[6] = (byte)(DataList.Length * 2);
            for (int i = 0; i < RegisterNum; i++)
            {
                bytData[7 + 2 * i] = (byte)(DataList[i] >> 8);           //数据个数高位
                bytData[7 + 2 * i + 1] = (byte)(DataList[i] & 0xFF);         //数据个数低位;
            }

            UInt16 crc16 = PortUtils.GetCheckCode(bytData, bytData.Length - 2);

            bytData[bytData.Length - 1] = (byte)(crc16 >> 8);           //数据个数高位
            bytData[bytData.Length - 2] = (byte)(crc16 & 0xFF);         //数据个数低位;

            return bytData;

        }
    }
}
