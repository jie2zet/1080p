using ClosedXML.Excel;
using CMP.Models.Operation.LoadPort;
using CMP.Models.Resource;
using CMP.Services.Log;
using Dm.util;
using DocumentFormat.OpenXml.Spreadsheet;
using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using static CMP.ViewModels.Tabs.OperationTab.LoadPortContentViewModel;

namespace CMP.Services.LoadPort
{

    /// <summary>
    /// 串口服务实现
    /// </summary>
    public class LoadPortService : ILoadPortService
    {



        public event EventHandler<SerialResponseEventArgs> ResponseReceived;

        public LoadPortService()
        {



        }
        public SerialResponseEventArgs ParseResponse(byte[] data, string msg)
        {
            try
            {
                // 解析命令和响应内容
                string command = "";
                string response = "";
                bool isSuccess = msg.Contains("ACK");

                // 根据实际协议，响应格式为：ACK:COMMAND/RESPONSE; 或 NAK:COMMAND/ERROR;
                // 查找冒号位置
                int colonIndex = msg.IndexOf(':');
                if (colonIndex > 0)
                {
                    // 查找斜杠位置
                    int slashIndex = msg.IndexOf('/', colonIndex);
                    // 查找分号位置
                    int semicolonIndex = msg.IndexOf(';', colonIndex);
                    if (semicolonIndex == -1) semicolonIndex = msg.Length;

                    // 提取命令部分
                    command = msg.Substring(colonIndex + 1,
                        (slashIndex > colonIndex && slashIndex < semicolonIndex ?
                         slashIndex : semicolonIndex) - colonIndex - 1);

                    // 提取响应部分
                    if (slashIndex > colonIndex && slashIndex < semicolonIndex)
                    {
                        response = msg.Substring(slashIndex + 1, semicolonIndex - slashIndex - 1);
                    }
                }
                // 创建响应事件参数
                var result = new SerialResponseEventArgs
                {
                    Command = command,
                    Response = response,
                    IsSuccess = isSuccess
                };
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error parsing response: {ex.Message}", ex, "loadport.txt", 0);
                return null;
            }
        }
        // 新增方法：解析并应用LoadPort状态
        public void ParseAndApplyLoadPortState(SerialResponseEventArgs response)
        {
            try
            {
                switch (response.Command)
                {
                    case "STATE":
                        //ParseStateResponse(response.Response);
                        break;
                    case "MAPDT":
                        //ParseMappingResponse(response.Response);
                        break;
                    // 可以添加其他命令的解析...
                    case "MAPRD":
                        //ParseMappingResponse(response.Response);
                        break;
                        // 可以添加其他命令的解析...
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error applying LoadPort state: {ex.Message}", ex, "loadport.txt", 0);
            }
        }
        LoadPortStatus loadPort = null;

        Wafer waferData = null;
        // 新增方法：解析状态响应
        public LoadPortStatus ParseStateResponse(string response)
        {
            loadPort = new LoadPortStatus();
            try
            {
                // 根据实际协议解析状态响应 20位ASCII码
                // 示例格式：ACK:STATE/A1100010......010;
                var parts = response.Select(c => c.ToString()).ToArray(); ;

                if (parts.Length >= 20)
                {
                    // 更新LoadPort状态（根据实际协议调整）
                    loadPort.ErrorCode = short.Parse(parts[5]);//parts[4],parts[5]大小写errorcode

                    // 使用安全的枚举解析
                    if ("A".equals(parts[0]))
                    {
                        loadPort.EquipmentStatus = EquipmentStatus.Error;
                    }
                    else
                    {
                        loadPort.EquipmentStatus = EquipmentStatus.NoError;
                    }


                    if ("0".equals(parts[1]))
                    {
                        loadPort.Mode = Mode.HostMode;
                    }
                    else
                    {
                        loadPort.Mode = Mode.ManualMode;
                    }


                    if ("0".equals(parts[2]))
                    {
                        loadPort.positionShift = InitialPositionShift.NotExecuted;
                    }
                    else
                    {
                        loadPort.positionShift = InitialPositionShift.Executed;
                    }
                    if ("0".equals(parts[3]))
                    {
                        loadPort.operationStatus = OperationStatus.StoppedStatus;
                    }
                    else
                    {
                        loadPort.operationStatus = OperationStatus.Busy;
                    }

                    if ("0".equals(parts[6]))
                    {
                        loadPort.foupPlacementStatus = FoupPlacementStatus.NoExistence;
                    }
                    else if ("1".equals(parts[6]))
                    {
                        loadPort.foupPlacementStatus = FoupPlacementStatus.Normal;
                    }
                    else
                    {
                        loadPort.foupPlacementStatus = FoupPlacementStatus.Error;
                    }

                    if ("0".equals(parts[7]))
                    {

                        loadPort.FoupClampStatus = FoupClampStatus.Unclamp;
                    }
                    else if ("1".equals(parts[7]))
                    {

                        loadPort.FoupClampStatus = FoupClampStatus.Clamp;
                    }
                    else
                    {

                        loadPort.FoupClampStatus = FoupClampStatus.Unknown;
                    }

                    if ("0".equals(parts[8]))
                    {

                        loadPort.DoorLatchStatus = DoorLatchStatus.Unlatch;
                    }
                    else if ("1".equals(parts[8]))
                    {

                        loadPort.DoorLatchStatus = DoorLatchStatus.Latch;
                    }
                    else
                    {

                        loadPort.DoorLatchStatus = DoorLatchStatus.Unknown;
                    }

                    if ("0".equals(parts[9]))
                    {
                        loadPort.Vacuum = Vacuum.NoVacuum;
                    }
                    else
                    {
                        loadPort.Vacuum = Vacuum.Vacuum;
                    }

                    if ("0".equals(parts[10]))
                    {
                        loadPort.FIMSDoorStatus = FIMSDoorStatus.Close;
                    }
                    else if ("1".equals(parts[10]))
                    {
                        loadPort.FIMSDoorStatus = FIMSDoorStatus.Open;
                    }
                    else
                    {
                        loadPort.FIMSDoorStatus = FIMSDoorStatus.Unknown;
                    }


                    if ("0".equals(parts[11]))
                    {
                        loadPort.WaferOutSensor = WaferOutSensor.WaferOutDetected;
                    }
                    else
                    {
                        loadPort.WaferOutSensor = WaferOutSensor.NotDetected;
                    }

                    loadPort.Reserve = short.Parse(parts[12]);//13,17,18,19

                    if ("0".equals(parts[13]))
                    {
                        loadPort.FoupDockStatus = FoupDockStatus.Undocking;
                    }
                    else if ("1".equals(parts[13]))
                    {
                        loadPort.FoupDockStatus = FoupDockStatus.Docking;
                    }
                    else
                    {
                        loadPort.FoupDockStatus = FoupDockStatus.Unknown;
                    }


                    if ("0".equals(parts[14]))
                    {
                        loadPort.MapperArmPosition = MapperArmPosition.Retract;
                    }
                    else if ("1".equals(parts[14]))
                    {
                        loadPort.MapperArmPosition = MapperArmPosition.Extend;
                    }
                    else
                    {
                        loadPort.MapperArmPosition = MapperArmPosition.Unknown;
                    }

                    if ("0".equals(parts[15]))
                    {
                        loadPort.ZAxisPosition = ZAxisPosition.UpperEnd;
                    }
                    else if ("1".equals(parts[15]))
                    {
                        loadPort.ZAxisPosition = ZAxisPosition.LowerEnd;
                    }
                    else if ("2".equals(parts[15]))
                    {
                        loadPort.ZAxisPosition = ZAxisPosition.MPUpperEnd;
                    }
                    else if ("3".equals(parts[15]))
                    {
                        loadPort.ZAxisPosition = ZAxisPosition.MPLowerEnd;
                    }
                    else
                    {
                        loadPort.ZAxisPosition = ZAxisPosition.Unknown;
                    }

                    if ("0".equals(parts[19]))
                    {
                        loadPort.InfoPadStatus = InfoPadStatus.Null;
                    }
                    else if ("1".equals(parts[19]))
                    {
                        loadPort.InfoPadStatus = InfoPadStatus.PadA;
                    }
                    else if ("2".equals(parts[19]))
                    {
                        loadPort.InfoPadStatus = InfoPadStatus.PadB;
                    }
                    else if ("3".equals(parts[19]))
                    {
                        loadPort.InfoPadStatus = InfoPadStatus.PadAB;
                    }
                    else if ("4".equals(parts[19]))
                    {
                        loadPort.InfoPadStatus = InfoPadStatus.PadC;
                    }
                    else if ("5".equals(parts[19]))
                    {
                        loadPort.InfoPadStatus = InfoPadStatus.PadAC;
                    }
                    else if ("6".equals(parts[19]))
                    {
                        loadPort.InfoPadStatus = InfoPadStatus.PadBC;
                    }
                    else if ("7".equals(parts[19]))
                    {
                        loadPort.InfoPadStatus = InfoPadStatus.PadABC;
                    }
                    else if ("8".equals(parts[19]))
                    {
                        loadPort.InfoPadStatus = InfoPadStatus.PadD;
                    }
                    else if ("9".equals(parts[19]))
                    {
                        loadPort.InfoPadStatus = InfoPadStatus.PadAD;
                    }
                    else if ("A".equals(parts[19]))
                    {
                        loadPort.InfoPadStatus = InfoPadStatus.PadBD;
                    }
                    else if ("B".equals(parts[19]))
                    {
                        loadPort.InfoPadStatus = InfoPadStatus.PadABD;
                    }
                    else if ("C".equals(parts[19]))
                    {
                        loadPort.InfoPadStatus = InfoPadStatus.PadCD;
                    }
                    else if ("D".equals(parts[19]))
                    {
                        loadPort.InfoPadStatus = InfoPadStatus.PadACD;
                    }
                    else if ("E".equals(parts[19]))
                    {
                        loadPort.InfoPadStatus = InfoPadStatus.PadBCD;
                    }
                    else
                    {
                        loadPort.InfoPadStatus = InfoPadStatus.PadABCD;
                    }

                }

            }
            catch (Exception ex)
            {
                Logger.Error($"Error parsing state response: {ex.Message}", ex, "loadport.txt", 0);

            }
            return loadPort;
        }


        // 晶圆状态映射字典 - 行业标准映射

        // 添加字符到WaferStatus枚举的映射方法
        public WaferStatus MapCharToWaferStatus(char statusChar)
        {
            switch (char.ToUpper(statusChar))
            {
                case '0':
                    return WaferStatus.NonWafer;
                case '1':
                    return WaferStatus.NormalExistence;
                case '2':
                    return WaferStatus.CrossSlot;
                case '3':
                    return WaferStatus.Unknown; // 倾斜 (Tilted) - 没有直接对应的枚举值
                case '4':
                    return WaferStatus.UpDownTile;
                case '5':
                    return WaferStatus.Unknown; // 边缘损坏 (Edge damaged) - 没有直接对应的枚举值
                case 'W':
                    return WaferStatus.ThicknessErrorThick;
                case 'T':
                    return WaferStatus.ThicknessErrorThin;
                case 'S':
                    return WaferStatus.NormalExistence; // 厚度标准映射为正常存在
                case 'D':
                    return WaferStatus.Error; // 破损
                case 'C':
                    return WaferStatus.Error; // 裂纹
                case 'F':
                    return WaferStatus.Error; // 碎片
                case 'B':
                    return WaferStatus.Error; // 背面污染
                case 'P':
                    return WaferStatus.Error; // 正面污染
                case 'U':
                    return WaferStatus.Error; // 无法识别
                case 'M':
                    return WaferStatus.Error; // ID缺失
                case 'X':
                    return WaferStatus.Error; // ID读取错误
                case 'L':
                    return WaferStatus.Unknown; // 已锁定 - 没有直接对应的枚举值
                case 'R':
                    return WaferStatus.Unknown; // 已释放 - 没有直接对应的枚举值
                case 'Q':
                    return WaferStatus.Unknown; // 待检查 - 没有直接对应的枚举值
                default:
                    return WaferStatus.None;
            }
        }
        private readonly object _lockObj = new object();

        List<Wafer> wafers = null;
        public List<Wafer> ParseMappingResponse(string response)
        {
            
            wafers = new List<Wafer>();
            try
            {


                // 根据实际协议解析Mapping响应
                // 假设响应格式为: Slot1Data;Slot2Data;...;Slot25Data
                String[] slotData = response.Select(c => c.ToString()).ToArray(); ;

                for (int i = 0; i < slotData.Length; i++)
                {
                    waferData = new Wafer();
                    // 逐个解析状态字符

                    char statusChar = Convert.ToChar(slotData[i]);
                    waferData.SlotID = i+1;
                    // 设置枚举状态（用于程序逻辑）
                    waferData.WaferStatus = MapCharToWaferStatus(statusChar);
                    wafers.Add(waferData);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error parsing mapping response: {ex.Message}", ex, "loadport.txt", 0);
            }
            return wafers;
        }

        // 状态信息方法
        /*public LoadPortStatusInfo GetLoadPortStatus(int portIndex)
        {
            if (portIndex < 0 || portIndex >= _loadPorts.Count)
                throw new ArgumentOutOfRangeException(nameof(portIndex), "Invalid load port index");

            var loadPort = _loadPorts[portIndex];
            return new LoadPortStatusInfo
            {
                Name = loadPort.name,
                ErrorCode = loadPort.errorCode,
                Reserve = loadPort.reserve,
                EquipmentStatus = loadPort.equipmentStatus,
                Mode = loadPort.mode,
                InitialPositionShift = loadPort.positionShift,
                OperationStatus = loadPort.operationStatus,
                FoupPlacementStatus = loadPort.foupPlacementStatus,
                FoupClampStatus = loadPort.foupClampStatus,
                DoorLatchStatus = loadPort.doorLatchStatus,
                FIMSDoorStatus = loadPort.fIMSDoorStatus,
                Vacuum = loadPort.vacuum,
                WaferOutSensor = loadPort.waferOutSensor,
                FoupDockStatus = loadPort.foupDockStatus,
                ZAxisPosition = loadPort.zAxisPosition,
                MapperArmPosition = loadPort.mapperArmPosition,
                InfoPadStatus = loadPort.infoPadStatus,
                LoadportActionStatus = loadPort.loadportStatus
            };
        }*/

        // 获取LoadPort状态的实现
        /*public void GetLoadportState(string portName)
        {


            // 发送GET:STATE命令
            SendCommandWithResponseAsync("GET:STATE", portName,2);
        }*/

        // 解析LoadPort状态的实现
        /*public void ParseStatus(int portIndex)
        {
            if (portIndex < 0 || portIndex >= _loadPorts.Count)
                throw new ArgumentOutOfRangeException(nameof(portIndex), "Invalid load port index");

            var loadPort = _loadPorts[portIndex];
            // 解析从串口收到的状态数据

            // 这里需要根据实际的串口通信协议来实现
            // 示例解析逻辑（需要根据实际协议调整）:
            *//*
            if (!string.IsNullOrEmpty(response))
            {
                var parts = response.Split(',');
                if (parts.Length > 10)
                {
                    loadPort.errorCode = short.Parse(parts[0]);
                    loadPort.equipmentStatus = (EquipmentStatus)Enum.Parse(typeof(EquipmentStatus), parts[1]);
                    loadPort.mode = (Mode)Enum.Parse(typeof(Mode), parts[2]);
                    loadPort.operationStatus = (OperationStatus)Enum.Parse(typeof(OperationStatus), parts[3]);
                    loadPort.foupPlacementStatus = (FoupPlacementStatus)Enum.Parse(typeof(FoupPlacementStatus), parts[4]);
                    loadPort.foupClampStatus = (FoupClampStatus)Enum.Parse(typeof(FoupClampStatus), parts[5]);
                    loadPort.doorLatchStatus = (DoorLatchStatus)Enum.Parse(typeof(DoorLatchStatus), parts[6]);
                    loadPort.vacuum = (Vacuum)Enum.Parse(typeof(Vacuum), parts[7]);
                    loadPort.waferOutSensor = (WaferOutSensor)Enum.Parse(typeof(WaferOutSensor), parts[8]);
                    loadPort.foupDockStatus = (FoupDockStatus)Enum.Parse(typeof(FoupDockStatus), parts[9]);
                    loadPort.mapperArmPosition = (MapperArmPosition)Enum.Parse(typeof(MapperArmPosition), parts[10]);
                }
            }
            *//*
        }*/

        // 启动监控LoadPort状态
        /*public void StartMonitoring(int portIndex, String portName)
        {
            if (portIndex < 0 || portIndex >= _loadPorts.Count)
                throw new ArgumentOutOfRangeException(nameof(portIndex), "Invalid load port index");

            if (_monitoringStatus[portIndex])
                return; // 已经在监控中

            _monitoringStatus[portIndex] = true;

            var loadPort = _loadPorts[portIndex];
            int delay = 1000*30;

            // 在后台线程中运行状态监控
            Task.Run(() =>
            {
                while (_monitoringStatus[portIndex])
                {
                    Thread.Sleep(delay);
                    ParseStatus(portIndex);

                    // 更新前一个状态
                    loadPort.preLoadportStatus = loadPort.loadportStatus;

                    GetLoadportState(portName);
                }
            });
        }

        // 停止监控
        public void StopMonitoring(int portIndex)
        {
            if (portIndex < 0 || portIndex >= _monitoringStatus.Count)
                throw new ArgumentOutOfRangeException(nameof(portIndex), "Invalid load port index");

            _monitoringStatus[portIndex] = false;
        }*/

        // 串口命令发送方法
        private async Task<bool> SendCommandAsync(string command, SerialPort serialPort)
        {
            byte[] commandBytes = BuildCommandFrame($"{command}");
            bool sendSuccess = true;
            await Task.Run(() =>
            {
                lock (_lockObj)
                {
                    try
                    {
                        serialPort.Write(commandBytes, 0, commandBytes.Length);
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = $"Failed to send command to LoadPort : {ex.Message}";
                        //Growl.Error(errorMsg);
                        //Logger.Error(errorMsg, ex, "loadport.txt", 0);
                        sendSuccess = false;
                        return;
                    }
                }
            });
            return sendSuccess;
        }


        public async Task<bool> SendMoveCommandAsync(string action, SerialPort serialPort)
        {
            return await SendCommandAsync($"MOV:{action};", serialPort);
        }

        public async Task<bool> SendSetCommandAsync(string parameter, string value, SerialPort serialPort)
        {
            string command = "";
            if (string.IsNullOrEmpty(value))
            {
                command = $"SET:{parameter};";
            }
            else
            {
                command = $"SET:{parameter}/{value};";
            }
            return await SendCommandAsync(command, serialPort);
        }

        public async Task<bool> SendGetCommandAsync(string parameter, SerialPort serialPort)
        {
            return await SendCommandAsync($"GET:{parameter};", serialPort);
        }

        private byte[] BuildCommandFrame(string command)
        {
            // 构建符合协议的命令帧
            // 格式: SOH(1字节:0x01) + LEN(2字节) + ADR(2字节:0x30,0x30) + CMD + CS(2字节) + DEL(2字节:0x0d,0x0a)

            byte soh = 0x01;
            byte[] adrBytes = { 0x30, 0x30 }; // "00" 的 ASCII
            byte[] cmdBytes = Encoding.ASCII.GetBytes(command);

            // 计算 LEN：ADR(2字节) + CMD(命令长度) + CS(2字节) → 总长度为4 + 命令长度
            int lenValue = 4 + cmdBytes.Length;

            // 将长度值转换为16进制字节（高8位和低8位）
            byte[] lenBytes = new byte[2];
            lenBytes[0] = (byte)(lenValue / 0x100);  // 高8位
            lenBytes[1] = (byte)(lenValue % 0x100);  // 低8位

            // 计算校验和
            int sum = lenBytes[0] + lenBytes[1] + adrBytes[0] + adrBytes[1];
            foreach (byte b in cmdBytes)
                sum += b;

            // 校验和转换为两位十六进制ASCII码
            byte csValue = (byte)(sum & 0xFF);  // 取低8位
            byte[] csBytes = new byte[2];
            //取csValue高4位，小于10时加0x30(转数字字符)，否则加0x37(转A-F字符)
            csBytes[0] = (byte)((csValue >> 4) < 10 ? (csValue >> 4) + 0x30 : (csValue >> 4) + 0x37);//转换为十六进制字符
            csBytes[1] = (byte)((csValue & 0x0F) < 10 ? (csValue & 0x0F) + 0x30 : (csValue & 0x0F) + 0x37);//转换为十六进制字符

            // 计算总长度并初始化数组
            int totalLength = 1 + 2 + 2 + cmdBytes.Length + 2 + 2;
            byte[] fullCommand = new byte[totalLength];
            int index = 0;

            // 拼接完整指令
            fullCommand[index++] = soh;
            fullCommand[index++] = lenBytes[0];
            fullCommand[index++] = lenBytes[1];
            fullCommand[index++] = adrBytes[0];
            fullCommand[index++] = adrBytes[1];
            Array.Copy(cmdBytes, 0, fullCommand, index, cmdBytes.Length);
            index += cmdBytes.Length;
            fullCommand[index++] = csBytes[0];
            fullCommand[index++] = csBytes[1];
            fullCommand[index++] = 0x0d; // CR
            fullCommand[index++] = 0x0a; // LF

            return fullCommand;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
    //public class WaferData : BindableBase
    //{

    //   private int _slotNumber;
    //   private string _waferStatus;
    //   private string _waferColor;
    //   private bool isSelected;
    //   public bool IsSelected
    //    {
    //        get { return isSelected; }
    //        set { SetProperty(ref isSelected,value); }
    //   }
    //    public int SlotNumber
    //    {
    //        get { return _slotNumber; }
    //        set { SetProperty(ref _slotNumber, value); }
    //    }

    //    public string WaferStatus
    //    {
    //        get { return _waferStatus; }
    //        set
    //        {
    //            SetProperty(ref _waferStatus, value);
    //            UpdateWaferColor();
    //        }
    //    }

    //    public string WaferColor
    //    {
    //        get { return _waferColor; }
    //        set { SetProperty(ref _waferColor, value); }
    //    }

    //    private void UpdateWaferColor()
    //    {
    //        if (string.IsNullOrEmpty(_waferStatus) ||
    //            string.Equals(_waferStatus, "empty", StringComparison.OrdinalIgnoreCase))
    //        {
    //            WaferColor = "gray";
    //        }
    //        else if (string.Equals(_waferStatus, "present", StringComparison.OrdinalIgnoreCase))
    //        {
    //            WaferColor = "blue";
    //        }
    //        else if (string.Equals(_waferStatus, "processing", StringComparison.OrdinalIgnoreCase))
    //        {
    //            WaferColor = "yellow";
    //        }
    //        else if (string.Equals(_waferStatus, "preparation", StringComparison.OrdinalIgnoreCase))
    //        {
    //            WaferColor = "lightYellow";
    //        }
    //        else if (string.Equals(_waferStatus, "completed", StringComparison.OrdinalIgnoreCase))
    //        {
    //            WaferColor = "green";
    //        }
    //        else if (string.Equals(_waferStatus, "error", StringComparison.OrdinalIgnoreCase))
    //        {
    //            WaferColor = "red";
    //        }
            
    //    }

    //    public WaferData()
    //    {
    //        // 默认状态
    //        _waferStatus = "Empty";
    //        UpdateWaferColor(); // 确保颜色正确初始化
    //    }

    //}
    public class LoadPortStatus: BindableBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        private LoadportActionStatus _loadportStatus;
        public LoadportActionStatus LoadportStatus
        {
            get { return _loadportStatus; }
            set { SetProperty(ref _loadportStatus, value); }
        }
        private LoadportActionStatus _preLoadportStatus;
        public LoadportActionStatus PreLoadportStatus
        {
            get { return _preLoadportStatus; }
            set { SetProperty(ref _preLoadportStatus, value); }
        }
        private short _errorCode;
        public short ErrorCode
        {
            get { return _errorCode; }
            set { SetProperty(ref _errorCode, value); }
        }
        private short _reserve;
        public short Reserve
        {
            get { return _reserve; }
            set { SetProperty(ref _reserve, value); }
        }
        private EquipmentStatus _equipmentStatus;
        public EquipmentStatus EquipmentStatus
        {
            get { return _equipmentStatus; }
            set { SetProperty(ref _equipmentStatus,value); }
        }
        private Mode _mode;
        public Mode Mode
        {
            get { return _mode; }
            set { SetProperty(ref _mode, value); }
        }
        private InitialPositionShift _positionShift;
        public InitialPositionShift positionShift
        {
            get { return _positionShift; }
            set { SetProperty(ref _positionShift, value); }
        }
        private OperationStatus _operationStatus;
        public OperationStatus operationStatus
        {
            get { return _operationStatus; }
            set { SetProperty(ref _operationStatus,value); }
        }
        private FoupPlacementStatus _foupPlacementStatus;

        public FoupPlacementStatus foupPlacementStatus
        {
            get { return _foupPlacementStatus; }
            set { SetProperty(ref _foupPlacementStatus, value); }
        }
        private  FoupClampStatus _foupClampStatus;
        public FoupClampStatus FoupClampStatus
        {
            get { return _foupClampStatus; }
            set { SetProperty(ref _foupClampStatus, value); }
        }
        private DoorLatchStatus _doorLatchStatus;
        public DoorLatchStatus DoorLatchStatus
        {
            get { return _doorLatchStatus; }
            set { SetProperty(ref _doorLatchStatus, value); }
        }
        private FIMSDoorStatus _fIMSDoorStatus;
        public FIMSDoorStatus FIMSDoorStatus
        {
            get { return _fIMSDoorStatus; }
            set { SetProperty(ref _fIMSDoorStatus, value); }
        }
        private Vacuum _vacuum;
        public Vacuum Vacuum
        {
            get { return _vacuum; }
            set { SetProperty(ref _vacuum, value); }
        }
        private WaferOutSensor _waferOutSensor;
        public WaferOutSensor WaferOutSensor
        {
            get { return _waferOutSensor; }
            set { SetProperty(ref _waferOutSensor, value); }
        }
        private FoupDockStatus _foupDockStatus;
        public FoupDockStatus FoupDockStatus
        {
            get { return _foupDockStatus; }
            set { SetProperty(ref _foupDockStatus, value); }
        }
        private MapperArmPosition _mapperArmPosition;
        public MapperArmPosition MapperArmPosition
        {
            get { return _mapperArmPosition; }
            set { SetProperty(ref _mapperArmPosition, value); }
        }
        private ZAxisPosition _zAxisPosition;
        public ZAxisPosition ZAxisPosition
        {
            get { return _zAxisPosition; }
            set { SetProperty(ref _zAxisPosition, value); }
        }
        private InfoPadStatus _infoPadStatus;
        public InfoPadStatus InfoPadStatus
        {
            get { return _infoPadStatus; }
            set { SetProperty(ref _infoPadStatus, value); }
        }
    }
    public enum EquipmentStatus
    {

        NoError,
        Error,
    }

    public enum Mode
    {
        HostMode,
        ManualMode,
    }
    public enum InitialPositionShift
    {
        NotExecuted,
        Executed,
    }
    public enum OperationStatus
    {
        StoppedStatus,
        Busy,
    }

    public enum FoupPlacementStatus
    {
        NoExistence,
        Normal,
        Error,
    }

    public enum FoupClampStatus
    {
        Unclamp,
        Clamp,
        Unknown,
    }

    public enum DoorLatchStatus
    {
        Unlatch,
        Latch,
        Unknown,
    }

    public enum FIMSDoorStatus
    {
        Open,
        Close,
        Unknown,
    }
    public enum Vacuum
    {
        NoVacuum,
        Vacuum,
    }

    public enum WaferOutSensor
    {
        WaferOutDetected,
        NotDetected,
    }

    public enum FoupDockStatus
    {
        Undocking,
        Docking,
        Unknown,
    }

    public enum MapperArmPosition
    {
        Retract,
        Extend,
        Unknown,
    }
    public enum ZAxisPosition
    {
        UpperEnd,
        LowerEnd,
        MPUpperEnd,
        MPLowerEnd,
        Unknown,
    }
    public enum LoadportActionStatus
    {
        None,
        WaitFoupArrive,//等待foup到达
        FoupArrived,//foup到达
        Clamp,//foup 夹紧
        Dock,//锁住
        Load,//装载
        DoorOpen,//门打开
        Scan,//扫片
        Undock,//解锁
        Unload,//卸载
        Release,// 释放
        WaitFoupLeave,//等待离开
    }
    public enum InfoPadStatus
    {
        Null,
        PadA,
        PadB,
        PadAB,
        PadC,
        PadAC,
        PadBC,
        PadABC,
        PadD,
        PadAD,
        PadBD,
        PadABD,
        PadCD,
        PadACD,
        PadBCD,
        PadABCD,

    }

}
