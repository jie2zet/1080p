using CMP.Extensions;
using CMP.Models.HostCommunication;
using CMP.Models.Resource;
using CMP.Services.LoadPort;
using Dm.util;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2013.WebExtension;
using DocumentFormat.OpenXml.Wordprocessing;
using FabConnect;
using FabConnect.SecsGemInterface.Application.MessageServices;
using FabConnect.SecsGemInterface.Common;
using FabConnect.SecsGemInterface.Common.ToolModel;
using FabConnect.SecsGemInterface.GemModel;
using SqlSugar.DistributedSystem.Snowflake;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using static CMP.Models.kvsPlc.SV.KvStruct;
using Logger = CMP.Services.Log.Logger;

namespace CMP.Services.HostCommunication
{
    public class HostCommunicationService : IHostCommunicationService
    {
        public string FaCommunicationState
        {
            get { return _gem.CommunicationState.ToString(); }
        }
        public string FaControlState
        {
            get { return _gem.ControlState.ToString(); }
        }

        public int IntControlState => (int)_gem.ControlState;
        public string FaControlSubState
        {
            get
            {
                return _gem.ControlRemoteSwitch.ToString();
            }
        }
        public bool IsConnected
        {
            get { return _gem.IsConnected; }
        }

        public int SpoolingState
        {
            get
            {
                return (int)_gem.SpoolingState;
            }
        }

        public string SpoolingActual
        {
            get
            {
                return

              _gem.GetAttribute(GEMVariables.SpoolCountActual, AttributeType.SV)
              ;
            }
        }

        public string SpoolingTotal
        {
            get { return _gem.GetAttribute(GEMVariables.SpoolCountTotal, AttributeType.SV); }
        }

        public string SpoolingFullTime
        {
            get { return _gem.GetAttribute(GEMVariables.SpoolFullTime, AttributeType.SV); }
        }

        public string SpoolingStartTime
        {
            get { return _gem.GetAttribute(GEMVariables.SpoolStartTime, AttributeType.SV); }
        }

        public bool IsSpoolingEnable
        {
            get
            {
                return false;// Convert.ToBoolean(_gem.GetAttribute(GEMVariables.SpoolEnabled, AttributeType.EC)); 
            }
        }

        protected readonly GEMController _gem = new GEMController();
        //public IHostCallBack _equipment;
        private HashSet<string> _systemBuildInEc = new HashSet<string>();
        private HashSet<string> _systemBuildInVariables = new HashSet<string>();

        private ControlState _preControlState = ControlState.Unknown;
        private const string EventControlModeChangeReport = "ControlMode_CHANGE_Report";
        private const string EventTerminalMessage = "TerminalMessage";
        private DateTime _lastSendMsgTime;
        private MessageServiceManager ServiceManager;
        private object _locker = new object();


        private static string modelFilePath = "D:\\CMP\\Configuration\\FA\\";

        private Stopwatch _monitorLinkTest = new Stopwatch();

        private static Timer _timer;

        private PeriodicJob _faMonitorThread;
        private readonly IEventAggregator _eventAggregator;

        private ConcurrentQueue<SendEventModel> queue = new ConcurrentQueue<SendEventModel>();
        private bool _isOnline = false;

        private LoadPortStatus status = new LoadPortStatus();

        Polish polishData;
        public HostCommunicationService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;


            try
            {

                _gem.CommunicationStateChanged += OnCommunicationStateChanged;
                // _gem.RemoteCommandS2F41In += _gem_RemoteCommandS2F41In;
                // _gem.RemoteCommandS2F49In += _gem_RemoteCommandS2F49In;

                _gem.ControlStateChanged += _gem_ControlStateChanged;
                _gem.ShowTrialMessageBox = false;

                _gem.PrimaryMessageIn += _gem_PrimaryMessageIn;
                _gem.SecondaryMessageIn += _gem_SecondaryMessageIn;

                _gem.AutoPPDataReply = true;


                _gem.Initialize(modelFilePath + "FabConnectGemModel.xml");


                //get system build in ec
                _systemBuildInEc = new HashSet<string>();
                _systemBuildInEc.Add("EstablishCommunicationsTimeout");
                _systemBuildInEc.Add("MaxSpoolTransmit");
                _systemBuildInEc.Add("OverWriteSpool");
                _systemBuildInEc.Add("MaxSpoolCapacity");
                _systemBuildInEc.Add("SpoolEnabled");
                _systemBuildInEc.Add("TimeFormat");

                //initial system build in variable
                _systemBuildInVariables.Add("AlarmsEnabled");
                _systemBuildInVariables.Add("AlarmsSet");
                _systemBuildInVariables.Add("Clock");
                _systemBuildInVariables.Add("ControlState");
                _systemBuildInVariables.Add("EventsEnabled");
                _systemBuildInVariables.Add("PPExecName");
                _systemBuildInVariables.Add("PreviousProcessState");
                _systemBuildInVariables.Add("ProcessState");
                _systemBuildInVariables.Add("SpoolCountActual");
                _systemBuildInVariables.Add("SpoolCountTotal");
                _systemBuildInVariables.Add("SpoolFullTime");
                _systemBuildInVariables.Add("SpoolStartTime");
                _systemBuildInVariables.Add("SpoolState");
                _systemBuildInVariables.Add("SpoolSubstate");
                InitialSystemBuildInVariable();
                ServiceManager = _gem.Services;
                _faMonitorThread = new PeriodicJob(100, MonitorFaTask, "Monitor FA Thread", true);


                if (!_gem.IsInitialized)
                {
                    Logger.Error($"HostCommunicationService Initialized Error: Failed to initialize FabConnect.Net ");
                }

                _lastSendMsgTime = DateTime.Now;

                SetFAOperativeMode();

                status.LoadportStatus = LoadportActionStatus.None;

                //监听LoadPort状态改变通知
                _eventAggregator.GetEvent<LoadPortStatusChangedEvent>().Subscribe(OnLoadPortStatusMessageReceived);

                //处理发送事件
                _timer = new Timer(
                callback: ProcessSendEventsTask,
                state: null,
                dueTime: 1000,
                period: 50);

            }
            catch (Exception ex)
            {

                Logger.Error($"HostCommunicationService Initialized Error:{ex.ToString()} ");
            }


        }


        /// <summary>
        /// 加载SV变量
        /// </summary>
        private void InitialSystemBuildInVariable()
        {
            string[] txtFiles = Directory.GetFiles(modelFilePath, "*.txt");
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
            foreach (string filePath in txtFiles)
            {
                if (filePath.Contains("SVVariable"))
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (!dic.ContainsKey(line))
                            {
                                dic[line] = line;
                            }
                        }
                    }

                }

            }

            if (dic.Count > 0)
            {
                foreach (string svVariable in dic.Keys)
                {
                    _systemBuildInVariables.Add(svVariable);
                }
            }

        }

        private void SetFAOperativeMode()
        {
            //todo: 延时发送FA工作模式
            Thread.Sleep(500);
            try
            {
                _gem.SetEnable();
            }
            catch (Exception)
            {

                throw;
            }


        }


        /// <summary>
        /// 接收到LoadPortStatus改变消息
        /// </summary>
        /// <param name="message"></param>
        private void OnLoadPortStatusMessageReceived(LoadPortStatusChangedEventArgs message)
        {

            Logger.Info($"HostCommunicationService Receive {message}");
            if (message.HardwareStatus == null) return;

            int portTransferStatus = (int)message.PortTransferStatus;
            int carrierId = 0;
            if (!string.IsNullOrEmpty(message.CarrierId))
            {
                carrierId = Convert.ToInt16(message.CarrierId);
            }

            switch (message.HardwareStatus.LoadportStatus)
            {
                case LoadportActionStatus.None:
                    queue.Enqueue(new SendEventModel(LoadPortTransferState.LoadPortInitialized.toString(), message.LoadportId, portTransferStatus, carrierId, false));
                    queue.Enqueue(new SendEventModel(LoadPortTransferState.LoadPortInService.toString(), message.LoadportId, portTransferStatus, carrierId, false));
                    break;
                case LoadportActionStatus.WaitFoupArrive:
                    queue.Enqueue(new SendEventModel(LoadPortTransferState.ReadyToLoadEntered.toString(), message.LoadportId, portTransferStatus, carrierId, false));
                    break;
                case LoadportActionStatus.FoupArrived: //LoadPort上有foup可以    
                    queue.Enqueue(new SendEventModel(LoadPortTransferState.TransferBlockedFromReadyToLoad.toString(), message.LoadportId, portTransferStatus, carrierId, true));

                    break;
                case LoadportActionStatus.Clamp:

                    break;
                case LoadportActionStatus.Dock:

                    break;
                case LoadportActionStatus.Load: //LoadPort 上有foup可以进行装载作业

                    queue.Enqueue(new SendEventModel(LoadPortTransferState.TransferBlockedFromReadyToLoad.toString(), message.LoadportId, portTransferStatus, carrierId, true));
                    break;
                case LoadportActionStatus.DoorOpen:

                    break;
                case LoadportActionStatus.Scan:

                    break;
                case LoadportActionStatus.Undock:

                    break;
                case LoadportActionStatus.Unload: //LoadPort 可以取走foup

                    queue.Enqueue(new SendEventModel(LoadPortTransferState.ReadyToLoadEnteredFromTransferBlocked.toString(), message.LoadportId, portTransferStatus, carrierId, false));
                    break;
                case LoadportActionStatus.Release:

                    break;
                case LoadportActionStatus.WaitFoupLeave: //作业完成等待取走foup

                    queue.Enqueue(new SendEventModel(LoadPortTransferState.ReadyToUnloadEnteredFromTransferBlocked.toString(), message.LoadportId, portTransferStatus, carrierId, false));
                    break;
            }


        }

        /// <summary>
        /// 通知主机事件
        /// </summary>
        /// <param name="model"></param>
        private void SendEventToHost(SendEventModel model)
        {

            var allEvents = _gem.GetAllEnabledEvents();
            if (!allEvents.Contains(model.EventName)) return;

            _gem.SetAttribute("PortID", AttributeType.DV, Convert.ToString(model.LoadportId));
            _gem.SetAttribute("PortTransFerState", AttributeType.DV, Convert.ToString(model.PortTransFerState));
            if (model.IsSendCarrierID)
            {
                _gem.SetAttribute("CarrierID", AttributeType.DV, Convert.ToString(model.CarrierID));
            }


            SECsMessage message = ServiceManager.DataCollection.EventReportSend(model.EventName);
            SECsMessage reply = _gem.SendAndWait(message);
            if (reply != null)
            {
                if (reply.DataItem[0].ToInt() == 0)
                {
                    Logger.Info($"HostCommunicationService SendEventToHost EventName:{model.EventName}  LoadportId:{model.LoadportId} PortTransFerState:{model.PortTransFerState} CarrierID:{model.CarrierID}  successfully");
                }
                else
                {

                    Logger.Error($"HostCommunicationService SendEventToHost EventName:{model.EventName}  LoadportId:{model.LoadportId} PortTransFerState:{model.PortTransFerState} CarrierID:{model.CarrierID}  is not sent successfully");
                }
            }
            else
            {
                Logger.Error($"HostCommunicationService SendEventToHost EventName:{model.EventName}  LoadportId:{model.LoadportId} PortTransFerState:{model.PortTransFerState} CarrierID:{model.CarrierID} No reply received!");
            }

        }


        private void ProcessSendEventsTask(object? state)
        {

            // PrintfSVValue();

            if (queue.TryDequeue(out SendEventModel? result))
            {
                Task.Run(() =>
                {
                    SendEventToHost(result);
                });
            }

        }




        private void _gem_SecondaryMessageIn(object sender, SECsSecondaryInEventArgs e)
        {
            _monitorLinkTest.Restart();
        }

        private void _gem_PrimaryMessageIn(object sender, SECsPrimaryInEventArgs e)
        {
            _monitorLinkTest.Restart();
            lock (_locker)
            {
                string SF = "S" + e.Inputs.Stream.ToString() + "F" + e.Inputs.Function.ToString();
                switch (SF)
                {
                    case "S2F15"://Equipment constant send
                        Handle_S2F15(e);
                        break;
                    case "S2F25":
                        Handle_S2F25(e);
                        break;
                    case "S10F3"://Single terminal message
                        Handle_S10F3(e);
                        break;
                    case "S10F5"://Multiple terminal message
                        Handle_S10F5(e);
                        break;

                    case "S7F19": //PPID list query
                        Handle_S7F19(e);
                        break;
                    case "S7F17": //Delete PPID
                        Handle_S7F17(e);
                        break;
                    case "S7F5":
                        Handle_S7F5(e);
                        break;
                    case "S7F1":
                        Handle_S7F1(e);
                        break;
                    case "S7F23":
                        Handle_S7F23(e);
                        break;
                    case "S7F25":
                        Handle_S7F25FormattedPPRequest(e);
                        break;

                    case "S3F17":
                        E87_Handle_cms(e);
                        break;
                    case "S3F19":
                        E87_CancelAllCarrier(e);
                        break;
                    case "S3F27":
                        E87_ChangeAccessModeList(e);
                        break;
                    case "S3F25":
                        E87_PortAction(e);
                        break;
                    case "S16F5":
                        E40_Handle_PJCMD(e);
                        break;
                    case "S16F11":
                        E40_Handle_PRJobCreateEnh(e);
                        break;
                    case "S16F13":
                        E40_Handle_PRJobDuplicateCreate(e);
                        break;
                    case "S16F15":
                        E40_Handle_PRJobMultiCreate(e);
                        break;
                    case "S16F17":
                        E40_Handle_PRJobDequeue(e);
                        break;
                    case "S16F19":
                        E40_Handle_PRGetAllJobs(e);
                        break;
                    case "S16F21":
                        E40_Handle_PRGetSpace(e);
                        break;
                    case "S16F23":
                        E40_Handle_PRJobSetRecipeVariable(e);
                        break;
                    case "S16F25":
                        E40_Handle_PRJobSetStartMethod(e);
                        break;
                    case "S16F27":
                        E94_Handle_CtrJobCMD(e);
                        break;
                    case "S14F9":
                        E94_Handle_CtrJobCreate(e);
                        break;
                    case "S14F1":
                        HandleS14F1_GetAttribute(e);
                        break;
                    case "S14F3":
                        HandleS14F3_SetAttribute(e);
                        break;
                    case "S3F31":
                        HandleS3F31_WriteTag(e);
                        break;
                    case "S3F29":
                        HandleS3F29_ReadTag(e);
                        break;

                }

                //Thread.Sleep(200);

            }

        }

        private void HandleS14F3_SetAttribute(SECsPrimaryInEventArgs e)
        {

        }

        private void Handle_S7F1(SECsPrimaryInEventArgs e)
        {
            try
            {
                string reason = string.Empty;
                // LOG.Write("Handle_S7F1, Request the permission of the process program body");


                SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(7, 2, false);
                reply.DataItem.AddList();
                reply.DataItem[0].Add("PPGNT", "1", SECsFormat.Ascii);


                _gem.SendReply(reply, e.TransactionID);


                //_gem.SendReply(reply, e.TransactionID);
            }
            catch (Exception ex)
            {
                // LOG.Write("Handle_S7F1 exception:" + ex.Message);
                SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(7, 0, false);
                _gem.SendReply(reply, e.TransactionID);
            }
        }

        private void HandleS14F1_GetAttribute(SECsPrimaryInEventArgs e)
        {

        }
        private void HandleS3F29_ReadTag(SECsPrimaryInEventArgs e)
        {

        }

        private void HandleS3F31_WriteTag(SECsPrimaryInEventArgs e)
        {

        }
        private void E87_CancelAllCarrier(SECsPrimaryInEventArgs e)
        {

        }

        private void E87_PortAction(SECsPrimaryInEventArgs e)
        {

        }
        private void E87_ChangeAccessModeList(SECsPrimaryInEventArgs e)
        {

        }


        private void E94_Handle_CtrJobCreate(SECsPrimaryInEventArgs e)
        {

        }
        private void E94_Handle_CtrJobCMD(SECsPrimaryInEventArgs e)
        {


        }
        private void E40_Handle_PJCMD(SECsPrimaryInEventArgs e)
        {


        }
        private void E40_Handle_PRJobCreateEnh(SECsPrimaryInEventArgs e)
        {


        }
        private void E40_Handle_PRJobDuplicateCreate(SECsPrimaryInEventArgs e)
        {

        }
        private void E40_Handle_PRJobMultiCreate(SECsPrimaryInEventArgs e)
        {

        }
        private void E40_Handle_PRJobDequeue(SECsPrimaryInEventArgs e)
        {

        }
        private void E40_Handle_PRGetAllJobs(SECsPrimaryInEventArgs e)
        {

        }
        private void E40_Handle_PRGetSpace(SECsPrimaryInEventArgs e)
        {

        }
        private void E40_Handle_PRJobSetRecipeVariable(SECsPrimaryInEventArgs e)
        {

        }
        private void E40_Handle_PRJobSetStartMethod(SECsPrimaryInEventArgs e)
        {

        }
        private void E87_Handle_cms(SECsPrimaryInEventArgs e)
        {

        }

        private void Handle_S7F25FormattedPPRequest(SECsPrimaryInEventArgs e)
        {

        }
        private void Handle_S7F23(SECsPrimaryInEventArgs e)
        {

        }
        private void Handle_S7F17(SECsPrimaryInEventArgs e)
        {

        }

        //private bool DeleteRecipes(string[] recs)
        //{
        //    //return _equipment.DeleteRecipes(recs);
        //}

        private void Handle_S7F5(SECsPrimaryInEventArgs e)
        {

        }


        /// <summary>
        /// Host requests the equipment to send the current EPPD
        /// S7F19 Format
        /// Head only
        /// </summary>
        /// <param name="e"></param>
        private void Handle_S7F19(SECsPrimaryInEventArgs e)
        {

        }


        /// <summary>
        /// Single terminal message process
        /// S10F3 Format
        /// L,2
        ///     1. <TID>
        ///     2. <TEXT>
        /// </summary>
        /// <param name="e"></param>
        private void Handle_S10F3(SECsPrimaryInEventArgs e)
        {
            try
            {
                // LOG.Write("Process host single terminal message");
                string terminalMessage = e.Inputs.DataItem["Ln"]["TEXT"].ToString();
                // notify equipment to show terminal message
                OnReceivedSingleTerminalMessage(terminalMessage);
                // acknowledge Host
                SECsMessage reply = _gem.Services.TerminalDisplay.TerminalDisplayAcknowledge(0);
                reply.Function = 4;
                _gem.SendReply(reply, e.TransactionID);
            }
            catch (Exception ex)
            {
                // LOG.Write("Handle_S10F3 Exception: " + ex.Message);
                SECsMessage reply = _gem.Services.TerminalDisplay.TerminalDisplayAcknowledge(1/*will not display*/);
                reply.Function = 4;
                _gem.SendReply(reply, e.TransactionID);
            }
        }

        /// <summary>
        /// Multiple terminal message process
        /// S10F5 Format
        /// L,2
        ///      1. <TID>
        ///      2. L,N
        ///         1. <TEXT1>
        ///         .
        ///         .
        ///         n.<TEXTn>
        /// </summary>
        /// <param name="e"></param>
        private void Handle_S10F5(SECsPrimaryInEventArgs e)
        {
            try
            {
                //  LOG.Write("Process host multiple terminal message");
                List<string> receivedMultipleMessages = new List<string>();
                string message = string.Empty;
                for (int i = 0; i < e.Inputs.DataItem[0][1].Count; i++)
                {
                    receivedMultipleMessages.Add((string)e.Inputs.DataItem[0][1][i].Value);
                }
                // notify equipment to show terminal message
                OnReceivedMultipleTerminalMessage(receivedMultipleMessages);
                // acknowledge Host
                SECsMessage reply = _gem.Services.TerminalDisplay.TerminalDisplayAcknowledge(0);
                reply.Function = 6;
                _gem.SendReply(reply, e.TransactionID);
            }
            catch (Exception ex)
            {
                //  LOG.Write("Handle_S10F5 Exception: " + ex.Message);
                SECsMessage reply = _gem.Services.TerminalDisplay.TerminalDisplayAcknowledge(1/*will not display*/);
                reply.Function = 6;
                _gem.SendReply(reply, e.TransactionID);
            }
        }

        /// <summary>
        /// Handle the equipment constant value change request by Host
        /// S2F13 Format
        /// L,n
        ///     1. <ECID1>
        ///     .
        ///     n. <ECIDn>
        /// </summary>
        /// <param name="e"></param>
        private void Handle_S2F13(SECsPrimaryInEventArgs e)
        {

        }

        private SECsFormat ConvertObjFormat(object value)
        {
            if (value.GetType().IsArray == true)
            {
                return SECsFormat.List;
            }
            SECsFormat format = SECsFormat.UNRECOGNIZE_FORMAT;
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Boolean:
                    format = SECsFormat.Boolean;
                    break;
                case TypeCode.Byte:
                    format = SECsFormat.U1;
                    break;
                case TypeCode.Char:
                    format = SECsFormat.Ascii;
                    break;
                case TypeCode.Double:
                    format = SECsFormat.F8;
                    break;
                case TypeCode.Int16:
                    format = SECsFormat.I2;
                    break;
                case TypeCode.Int32:
                    if (value.GetType().IsEnum)
                        format = SECsFormat.U1;
                    else
                        format = SECsFormat.I4;
                    break;
                case TypeCode.Int64:
                    format = SECsFormat.I8;
                    break;
                case TypeCode.String:
                    format = SECsFormat.Ascii;
                    break;
                case TypeCode.UInt16:
                    format = SECsFormat.U2;
                    break;
                case TypeCode.UInt32:
                    format = SECsFormat.U4;
                    break;
                case TypeCode.UInt64:
                    format = SECsFormat.U8;
                    break;
                case TypeCode.SByte:
                    format = SECsFormat.Binary;
                    break;
                case TypeCode.Single:
                    format = SECsFormat.F4;
                    break;
                case TypeCode.Decimal:
                    format = SECsFormat.I8;
                    break;
                case TypeCode.DateTime:
                    format = SECsFormat.I8;
                    break;
                case TypeCode.Object:
                    format = SECsFormat.List;
                    break;

            }

            return format;



        }

        /// <summary>
        /// Handle the equipment constant change
        /// S2F15 Format
        /// L,n
        /// 1. L,2
        ///     1. <ECID1>
        ///     2. <ECV1>
        /// 2. L,2
        /// .
        /// n. L,2
        ///     1. <ECIDn>
        ///     2. <ECVn>        
        /// </summary>
        /// <param name="e"></param>
        private void Handle_S2F15(SECsPrimaryInEventArgs e)
        {
            try
            {
                // LOG.Write("Handle_S2F15, Equipment Constant Send");
                //get ec mapping
                Dictionary<long, string> ecMapping = new Dictionary<long, string>();
                foreach (ECID ec in _gem.EquipmentModel.EquipmentConstants.ECIDCollection)
                {
                    ecMapping.Add(ec.id, ec.logicalName);
                }
                for (int i = 0; i < e.Inputs.DataItem[0].Count; i++)
                {
                    long ecid = e.Inputs.DataItem[0][i][0].ToInt64();
                    string ecVal = e.Inputs.DataItem[0][i][1].ToString();
                    if (ecMapping.ContainsKey(ecid))
                    {
                        string ecName = ecMapping[ecid];
                        if (!_systemBuildInEc.Contains(ecName))
                        {
                            //SetECValue(ecName, ecVal);
                        }
                        _gem.SetAttribute(ecName, AttributeType.EC, ecVal);
                    }
                }
                // Set reply to acknowledge
                // S2F16 Format
                // <EAC>
                SECsMessage reply = _gem.Services.ControlDiagnostic.NewEquipmentConstantAcknowledge(0);
                _gem.SendReply(reply, e.TransactionID);
            }
            catch (Exception ex)
            {
                //LOG.Write(string.Format("Handle_S2F15 Exception: {0}", ex.Message));
                SECsMessage reply = _gem.Services.ControlDiagnostic.NewEquipmentConstantAcknowledge(1);
                _gem.SendReply(reply, e.TransactionID);
            }
        }

        private void Handle_S2F25(SECsPrimaryInEventArgs e)
        {
            try
            {
                //LOG.Write("Handle_S2F25, Diagnostic loopback feedabck");

                string binvalue = e.Inputs.DataItem[0].ToString();
                //get ec mapping


                // Set reply to acknowledge
                // S2F16 Format
                // <EAC>
                SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(2, 26, false);
                reply.DataItem.Add("Echoes", binvalue,
                     SECsFormat.Binary);
                _gem.SendReply(reply, e.TransactionID);
            }
            catch (Exception ex)
            {
                // LOG.Write(string.Format("Handle_S2F25 Exception: {0}", ex.Message));
                SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(2, 0, false);
                _gem.SendReply(reply, e.TransactionID);
            }
        }
        /// <summary>
        /// On single terminal message received
        /// </summary>
        /// <param name="terminalMessage"></param>
        protected virtual void OnReceivedSingleTerminalMessage(string terminalMessage)
        {
            //  EV.Notify("Host", EventTerminalMessage, terminalMessage);
        }

        /// <summary>
        /// On multiple terminal message received
        /// </summary>
        /// <param name="terminalMessage"></param>
        protected virtual void OnReceivedMultipleTerminalMessage(List<string> terminalMessage)
        {
            foreach (var message in terminalMessage)
            {
                // EV.Notify("Host", EventTerminalMessage, message);
            }
        }


        private void _gem_ControlStateChanged(object sender, SECsEventArgs e)
        {
            if (_preControlState != _gem.ControlState)
            {
                // LOG.Write($"Control mode changed from {_preControlState} to {_gem.ControlState}");


            }
            if (_gem.ControlState == ControlState.OnlineLocal && _needchangetoremote)
            {
                _gem.SetRemote();
                _needchangetoremote = false;
            }
        }



        /// <summary>
        /// 释放资源
        /// </summary>
        public void Terminate()
        {
            try
            {
                _faMonitorThread.Stop();
                _timer.Dispose();
                queue.Clear();
                _gem.Dispose();

            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }

        }

        public void Enable()
        {
            _gem.SetEnable();
        }

        public void Disable()
        {
            _gem.SetDisable();
        }

        public void NotifyEvent(string eventName, Dictionary<string, object> objDvid)
        {
            var allEvents = _gem.GetAllEnabledEvents();
            if (!allEvents.Contains(eventName)) return;



            //HandlePausedEvent(eventName);

            //   _lstEvent.Enqueue(new FAEventItemDemo() { objDvid = objDvid, EventName = eventName, IsAlarm = false });
        }

        public void NotifyAlarm(string alarmName, Dictionary<string, string> dvid, Dictionary<string, object> objDvid)
        {
            //_lstEvent.Enqueue(new FAEventItemDemo() { objDvid = objDvid, EventName = alarmName, IsAlarm = true });
        }
        public void ClearAlarm()
        {
            var alarms = _gem.GetAllEnabledAlarms();
            foreach (string alarm in alarms)
            {
                if (_gem.IsAlarmSet(alarm))
                {
                    _gem.ClearAlarm(alarm);
                }
                //_gem.ClearAlarm(alarm);
            }
        }
        public void SetLocalControl()
        {
            Task.Factory.StartNew(() =>
            {
                _gem.SetLocal();
            });
        }

        public void SetRemoteControl()
        {
            Task.Factory.StartNew(() =>
            {
                _gem.SetRemote();
            });
        }

        public void SetEnableSpooling()
        {
            Task.Factory.StartNew(() =>
            {
                _gem.SetAttribute(GEMVariables.SpoolEnabled, AttributeType.EC, "true");
            });
        }
        public void SetDisableSpooling()
        {
            Task.Factory.StartNew(() =>
            {
                _gem.SetAttribute(GEMVariables.SpoolEnabled, AttributeType.EC, "false");
            });
        }

        public bool MonitorFaTask()
        {
            if (_gem.CommunicationState == CommunicationState.EnabledCommunicating && _isOnline == false)
            {
                _gem.SetOnline();
                _gem.SetRemote();
                _isOnline = true;
            }

            if (_isOnline)
            {

                SetPolishSVData();

                // SetPolishSVDataTest();

            }

            return true;



        }

        private void LoadPortStatusChangedEventTest()
        {
            LoadPortStatusChangedEventArgs arg = new LoadPortStatusChangedEventArgs()
            {
                LoadportId = 1,
                HardwareStatus = status,
                CarrierId = "100"

            };


            OnLoadPortStatusMessageReceived(arg);

            switch (status.LoadportStatus)
            {
                case LoadportActionStatus.None:
                    status.LoadportStatus = LoadportActionStatus.WaitFoupArrive;
                    break;
                case LoadportActionStatus.WaitFoupArrive:
                    status.LoadportStatus = LoadportActionStatus.FoupArrived;
                    break;
                case LoadportActionStatus.FoupArrived:
                    status.LoadportStatus = LoadportActionStatus.Clamp;
                    break;
                case LoadportActionStatus.Clamp:
                    status.LoadportStatus = LoadportActionStatus.Dock;
                    break;
                case LoadportActionStatus.Dock:
                    status.LoadportStatus = LoadportActionStatus.Load;
                    break;
                case LoadportActionStatus.Load:
                    status.LoadportStatus = LoadportActionStatus.DoorOpen;
                    break;
                case LoadportActionStatus.DoorOpen:
                    status.LoadportStatus = LoadportActionStatus.Scan;
                    break;
                case LoadportActionStatus.Scan:
                    status.LoadportStatus = LoadportActionStatus.Undock;
                    break;
                case LoadportActionStatus.Undock:
                    status.LoadportStatus = LoadportActionStatus.Unload;
                    break;
                case LoadportActionStatus.Unload:
                    status.LoadportStatus = LoadportActionStatus.Release;
                    break;
                case LoadportActionStatus.Release:
                    status.LoadportStatus = LoadportActionStatus.WaitFoupLeave;
                    break;
                case LoadportActionStatus.WaitFoupLeave:
                    status.LoadportStatus = LoadportActionStatus.None;
                    break;
            }

            Thread.Sleep(500);

        }


        private void SetPolishSVDataTest()
        {
            foreach (SVID sv in _gem.EquipmentModel.StatusVariables.SVIDCollection)
            {
                if (sv != null && _systemBuildInVariables.Contains(sv.logicalName) && sv.logicalName.contains("Polish"))
                {

                    SetSVIDValue(sv.logicalName, 99);

                }
            }

        }


        private void SetPolishSVData()
        {
            var stopwatch = Stopwatch.StartNew();

            for (int index = 0; index < 4; index++)
            {
                if (GlobalService.DataCenter.listPolish[index].TryGetProcessDataSnapshot(out stPolishCollectData snapshot))
                {

                    FieldInfo[] fields = typeof(stPolishCollectData).GetFields();

                    foreach (FieldInfo field in fields)
                    {
                        string name = field.Name;
                        switch (index)
                        {
                            case 0:
                                name = "PolishA" + name;
                                break;
                            case 1:
                                name = "PolishB" + name;
                                break;
                            case 2:
                                name = "PolishC" + name;
                                break;
                            case 3:
                                name = "PolishD" + name;
                                break;
                            default:
                                break;

                        }
                        object? value = field.GetValue(snapshot);
                        if (value != null)
                        {
                            SetSVIDValue(name, value);
                        }

                    }

                }
            }

            stopwatch.Stop();
            Logger.Info($"上报SVVariable耗时: {stopwatch.ElapsedMilliseconds} ms");

        }

        private bool SetSVIDValue(string localName, object value)
        {
            if (value == null || string.IsNullOrEmpty(localName)) return false;

            if (_gem.EquipmentModel == null || !_gem.EquipmentModel.StatusVariables.SVIDCollection.IsExistLogicalName(localName))
                return false;

            foreach (SVID sv in _gem.EquipmentModel.StatusVariables.SVIDCollection)
            {
                if (sv != null && _systemBuildInVariables.Contains(sv.logicalName) && localName.Contains("Polish", StringComparison.OrdinalIgnoreCase))
                {
                    _gem.SetAttribute(localName, AttributeType.SV, Convert.ToString(value));
                    return true;
                }
            }

            return false;
        }

        private static readonly object _fileLock = new object();

        //todo：测试参数写入是否成功 
        private void PrintfSVValue()
        {
            if (!_isOnline) return;


            foreach (SVID sv in _gem.EquipmentModel.StatusVariables.SVIDCollection)
            {
                if (sv != null && _systemBuildInVariables.Contains(sv.logicalName) && sv.logicalName.contains("Polish"))
                {
                    string str = _gem.GetAttribute(sv.logicalName, AttributeType.SV);
                    lock (_fileLock)
                    {
                        string path = $"{modelFilePath}{DateTime.Now.ToString("yyyy-MM-dd")}.txt";
                        File.AppendAllText(path, $"读取到的数据名称：{sv.logicalName} --{str}" + Environment.NewLine, Encoding.UTF8);
                    }

                }
            }

        }



        private void SendHello()
        {
            SECsMessage helloMsg = _gem.Services.CustomMessage.CreateMessage(1, 1, true);

            lock (_locker)
            {
                _gem.Send(helloMsg);
            }

        }

        private void SendUDMsg()
        {
            SECsMessage helloMsg = _gem.Services.CustomMessage.CreateMessage(6, 1, true);
            helloMsg.DataItem.AddList();
            SECsDataItem item1 = new SECsDataItem(SECsFormat.Ascii);
            item1.Value = "8";

            SECsDataItem item3 = new SECsDataItem(SECsFormat.Ascii);
            item3.Value = "1";

            SECsDataItem item4 = new SECsDataItem(SECsFormat.Ascii);
            item4.Value = "20260702101839";


            SECsDataItem item2 = new SECsDataItem(SECsFormat.List);
            item2.Add("value1", 1, SECsFormat.U4);
            item2.Add("value1", 2, SECsFormat.U4);
            item2.Add("value1", 3, SECsFormat.U4);
            item2.Add("value1", 4, SECsFormat.U4);
            item2.Add("value1", 5, SECsFormat.U4);
            item2.Add("value1", 1, SECsFormat.U4);
            item2.Add("value1", 1, SECsFormat.U4);
            item2.Add("value1", 1, SECsFormat.U4);
            item2.Add("value1", 1, SECsFormat.U4);
            item2.Add("value1", 1, SECsFormat.U4);
            item2.Add("value1", 1, SECsFormat.U4);
            item2.Add("value1", 1, SECsFormat.U4);
            item2.Add("value1", 1, SECsFormat.U4);
            item2.Add("value1", 1, SECsFormat.U4);
            item2.Add("value1", 1, SECsFormat.U4);
            item2.Add("value1", 1, SECsFormat.U4);


            helloMsg.DataItem[0].Add(item1);

            helloMsg.DataItem[0].Add(item3);
            helloMsg.DataItem[0].Add(item4);

            helloMsg.DataItem[0].Add(item2);



            helloMsg.NeedReply = false;

            lock (_locker)
            {
                _gem.Send(helloMsg);
            }

        }

        private bool _needchangetoremote = false;


        private void OnCommunicationStateChanged(object sender, SECsEventArgs e)
        {
            if (_gem.CommunicationState == CommunicationState.EnabledCommunicating)
            {
                _monitorLinkTest.Restart();
                _gem.SetOnline();
                _needchangetoremote = true;

            }
            else
            {
                _monitorLinkTest.Stop();
            }
            //  LOG.Write($"Communication state change to {_gem.CommunicationState.ToString()}");

        }



        private void SynchronizeSVIDValue()
        {
            //try
            //{
            //    foreach (SVID sv in _gem.EquipmentModel.StatusVariables.SVIDCollection)
            //    {
            //        if (sv != null && !_systemBuildInVariables.Contains(sv.logicalName))
            //        {
            //            if (sv.valueType == SECSFormats.List)
            //            {
            //                SECsDataItem item = (SECsDataItem)_equipment.GetListSvidValue(sv.logicalName);
            //                 _gem.SetListAttribute(sv.logicalName, AttributeType.SV, item);

            //            }
            //            else
            //            {
            //                string svDataValue = _equipment.GetSvidValue(sv.logicalName);

            //                if (!string.IsNullOrEmpty(svDataValue))
            //                {
            //                    if (sv.valueType == SECSFormats.Boolean) 
            //                        svDataValue = ConvertToBoolean(svDataValue).ToString();
            //                    _gem.SetAttribute(sv.logicalName, AttributeType.SV, svDataValue);
            //                }
            //                else
            //                {
            //                    if (sv.valueType == SECSFormats.Ascii)
            //                        _gem.SetAttribute(sv.logicalName, AttributeType.SV, "");
            //                }
            //            }
            //        }
            //    }

            //}
            //catch (Exception ex)
            //{
            //    LOG.Write("Synchronize FA Model Data exception:" + ex.Message);
            //}
        }

        private void SynchronizeECIDValue()
        {
            //try
            //{
            //    List<SCConfigItem> scitems = _equipment.GetSCItemList();


            //    foreach (var scitem in scitems)
            //    {
            //        if (_gem.EquipmentModel.EquipmentConstants.ECIDCollection.IsExistLogicalName(scitem.PathName))
            //        {
            //            string ecDataValue = "";
            //            ECID ec = _gem.EquipmentModel.EquipmentConstants.ECIDCollection[scitem.PathName];
            //            ecDataValue = scitem.Value.ToString();
            //            if (ec.valueType == SECSFormats.Boolean)
            //                ecDataValue = ConvertToBoolean(ecDataValue).ToString();


            //                _gem.SetAttribute(ec.logicalName, AttributeType.EC, ecDataValue);


            //        }



            //    }





            //}
            //catch (Exception ex)
            //{
            //    LOG.Write("Synchronize FA Model EC Data exception:" + ex.Message);
            //}
        }


        private bool SetDVIDValue(string localName, object value)
        {
            if (value == null) return false;

            if (_gem.EquipmentModel == null || !_gem.EquipmentModel.DataVariables.DVIDCollection.IsExistLogicalName(localName))
                return false;

            foreach (VariableType dv in _gem.EquipmentModel.DataVariables.DVIDCollection)
            {
                if (dv.logicalName == localName)
                {
                    if (dv.valueType == SECSFormats.List)
                    {
                        try
                        {
                            SECsDataItem itemvalue = (SECsDataItem)value;
                            _gem.SetListAttribute(localName, AttributeType.DV, itemvalue);
                        }
                        catch (Exception ex)
                        {
                            //  LOG.Write(ex);
                        }
                        return true;


                    }


                    if (dv.valueType == SECSFormats.Boolean)
                    {
                        value = ConvertToBoolean(Convert.ToString(value)).ToString();
                    }




                    break;
                }
            }

            _gem.SetAttribute(localName, AttributeType.DV, Convert.ToString(value));


            return true;
        }




        //public void SetProcessState(FAProcessingState value)
        //{
        //    int intvalue = (int)value;
        //    if (_gem.IsInitialized)
        //        _gem.ProcessingState = (ProcessingState)intvalue;
        //}

        private bool ConvertToBoolean(string value)
        {
            if (value == "0" || value.ToLower() == "false") return false;
            else return true;
        }


        //private void SendEvent(string eventName)
        //{
        //    try
        //    {
        //        if (_gem.EquipmentModel != null)
        //        {
        //            var allEvents = _gem.GetAllEnabledEvents();
        //            if (allEvents.Contains(eventName))
        //            {
        //                LOG.Write(string.Format("【FA2SendEvent--{0}】", eventName));
        //                _gem.SendCollectionEvent(eventName);                        
        //            }
        //            else
        //            {
        //                //LOG.Write(string.Format("sendEvent failed,not find:", eventName));
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LOG.Write("SendEvent Error:" + ex.Message);
        //    }
        //}
        public bool IsNeedS6F12ReplyAndRetry
        {
            get; set;
        }


        public void SetAlarm(string alarmTag)
        {
            //Task.Factory.StartNew(() =>
            //{
            //    //if (_gem.IsAlarmSet(alarmTag))
            //    //{
            //    //    _gem.ClearAlarm(alarmTag);
            //    //}
            var allAlarms = _gem.GetAllEnabledAlarms();
            if (allAlarms.Contains(alarmTag))
            {
                _gem.SetAlarm(alarmTag);
            }
            else
            {
                // LOG.Write(string.Format("SetAlarm failed,not find:", alarmTag));
            }
            // LOG.Write(string.Format("FA3SetAlarm--{0}.", alarmTag));
            Thread.Sleep(500);
            // });
        }

        private bool _isPRJobAlert
        {
            get
            {
                //if (SC.ContainsItem("FA.NeedS16F7PRJobAlert"))
                //    return SC.GetValue<bool>("FA.NeedS16F7PRJobAlert");
                return false;
            }
        }
        private object _lockerPj = new object();
        public void HandlePausedEvent(string eventName)
        {



        }

        public void SendMessge(string data)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

            Terminate();

        }

        public class FAEventItemDemo
        {
            public string EventName { get; set; }
            public Dictionary<string, object> objDvid { get; set; }
            public bool IsAlarm { get; set; }
        }



        //public void SendMessge(string data)
        //{
        //    throw new NotImplementedException();
        //}




    }
}
