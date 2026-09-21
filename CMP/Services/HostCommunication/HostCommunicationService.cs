using CMP.Extensions;
using CMP.Models.Operation.LoadPort;
using CMP.Services.Auth;
using CMP.Services.LoadPort;
using FabConnect;
using FabConnect.SecsGemInterface.Application.MessageServices;
using FabConnect.SecsGemInterface.Application.MessageServices;
using FabConnect.SecsGemInterface.Common;
using FabConnect.SecsGemInterface.Common;
using FabConnect.SecsGemInterface.Common.ToolModel;
using FabConnect.SecsGemInterface.Common.ToolModel;
using FabConnect.SecsGemInterface.GemModel;
using HslCommunication.Secs;
//using FACore;
//using FACore.E87FA;
//using MECF.Framework.Common.Equipment;
//using MECF.Framework.Common.Log;
//using MECF.Framework.FA.Core.E40FA;
//using MECF.Framework.FA.Core.E87FA;
//using MECF.Framework.FA.Core.E94FA;
//using MECF.Framework.FA.Core.FAInterface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CMP.Services.Log;
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
        //private PeriodicJob _faMonitorThread;

        //private FixSizeQueue<FAEventItemDemo> _lstEvent = new FixSizeQueue<FAEventItemDemo>(30000);

        //private FixSizeQueue<SECsMessage> _lstToBeSendMsg = new FixSizeQueue<SECsMessage>(10000);

        //LogCleaner _logFabConnectCleaner = new LogCleaner(60);




        private ControlState _preControlState = ControlState.Unknown;
        private const string EventControlModeChangeReport = "ControlMode_CHANGE_Report";
        private const string EventTerminalMessage = "TerminalMessage";
        private DateTime _lastSendMsgTime;
        private  MessageServiceManager ServiceManager;
        private object _locker = new object();


        private static string modelFilePath = "D:\\CMP\\Configuration\\FA\\FabConnectGemModel.xml";

        private Stopwatch _monitorLinkTest = new Stopwatch();

        private   PeriodicJob _faMonitorThread;

        private readonly IEventAggregator _eventAggregator;
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


                _gem.Initialize(modelFilePath);


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

                ServiceManager = _gem.Services;
                _faMonitorThread = new PeriodicJob(200, MonitorFaTask, "Monitor FA Thread", true);


                if (!_gem.IsInitialized)
                {
                    Logger.Error($"HostCommunicationService Initialized Error: Failed to initialize FabConnect.Net ");
                }

                _lastSendMsgTime = DateTime.Now;

                //todo: 延时发送FA工作模式
                Thread.Sleep(500);
                SetFAOperativeMode();



            }
            catch (Exception ex)
            {
               
                Logger.Error($"HostCommunicationService Initialized Error:{ex.ToString()} ");
            }



            //接收LoadPort状态改变
            _eventAggregator.GetEvent<LoadPortStatusChangedEvent>().Subscribe(OnLoadPortStatusMessageReceived);


        }

        private void SetFAOperativeMode()
        {
            _gem.SetEnable();
            _gem.SetOnline();
            _gem.SetRemote();
        }


        /// <summary>
        /// 接收到LoadPortStatus改变消息
        /// </summary>
        /// <param name="message"></param>
        private void OnLoadPortStatusMessageReceived(LoadPortStatusChangedEventArgs  message)
        {
          
            Logger.Info($"HostCommunicationService Receive {message}");
            if (message.HardwareStatus == null) return;

            switch (message.HardwareStatus.LoadportStatus)
            {
                case LoadportActionStatus.None:
                    SendEventToHost("LoadPortInitialized",0,0,0);
                    break;
                case LoadportActionStatus.WaitFoupArrive:
                    SendEventToHost("LoadPortInService", 0, 0, 0);
                    break;
                case LoadportActionStatus.FoupArrived:
                    SendEventToHost("LoadPortOutOfService", 0, 0, 0);
                    break;
                case LoadportActionStatus.Clamp:
                    SendEventToHost("TransferReadyStateEntered", 0, 0, 0);
                    break;
                case LoadportActionStatus.Dock:
                    SendEventToHost("TransferBlockedStateEntered", 0, 0, 0);
                    break;
                case LoadportActionStatus.Load:
                    SendEventToHost("ReadyToLoadEntered", 0, 0, 0);
                    break;
                case LoadportActionStatus.DoorOpen:
                    SendEventToHost("ReadyToUnloadEntered", 0, 0, 0);
                    break;
                case LoadportActionStatus.Scan:
                    SendEventToHost("TransferBlockedFromReadyToLoad", 0, 0, 0);
                    break;
                case LoadportActionStatus.Undock:
                    SendEventToHost("TransferBlockedFromReadyToUnload", 0, 0, 0);
                    break;
                case LoadportActionStatus.Unload:
                    SendEventToHost("ReadyToLoadEnteredFromTransferBlocked", 0, 0, 0);
                    break;
                case LoadportActionStatus.Release:
                    SendEventToHost("ReadyToUnloadEnteredFromTransferBlocked", 0, 0, 0);
                    break;
                case LoadportActionStatus.WaitFoupLeave:
                    SendEventToHost("TransferReadyEnteredFromTransferBlocked", 0, 0, 0);
                    break;
            }


        }

        /// <summary>
        /// 发送事件到主机
        /// </summary>
        /// <param name="eventName"></param>
        private void SendEventToHost(string eventName, int loadportId, int portTransFerState, int carrierID)
        {

            var allEvents = _gem.GetAllEnabledEvents();
            if (!allEvents.Contains(eventName)) return;

            _gem.SetAttribute("PortID", AttributeType.DV, Convert.ToString(loadportId));
            _gem.SetAttribute("PortTransFerState", AttributeType.DV, Convert.ToString(portTransFerState));
            if (carrierID > 0)
            {
                _gem.SetAttribute("CarrierID", AttributeType.DV, Convert.ToString(carrierID));
            }


            SECsMessage message = ServiceManager.DataCollection.EventReportSend(eventName);
            SECsMessage reply = _gem.SendAndWait(message);
            if (reply != null)
            {
                if (reply.DataItem[0].ToInt() == 0)
                {
                    Logger.Info($"HostCommunicationService SendEventToHost EventName:{eventName}  LoadportId:{loadportId} PortTransFerState:{portTransFerState} CarrierID:{carrierID}  successfully");
                }
                else
                {
                   
                    Logger.Error($"HostCommunicationService SendEventToHost EventName:{eventName}  LoadportId:{loadportId} PortTransFerState:{portTransFerState} CarrierID:{carrierID}  is not sent successfully");
                }
            }
            else
            {
                Logger.Error($"HostCommunicationService SendEventToHost EventName:{eventName}  LoadportId:{loadportId} PortTransFerState:{portTransFerState} CarrierID:{carrierID} No reply received!");
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
            //try
            //{
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(14, 4, false);
            //    string reason = string.Empty;
            //   // LOG.Write("Handle_S14F1, Get Attribute");
            //    var dataItem = e.Inputs.DataItem;
            //    string objType = dataItem[0][1].Value.ToString().ToLower();
            //    List<string> objIds = new List<string>();

            //    for (int n = 0; n < dataItem[0][2].Count; n++)
            //    {
            //        objIds.Add(dataItem[0][2][n].Value.ToString());
            //    }

            //    var dataAttrData = dataItem[0][3];

            //    reply.DataItem.AddList();
            //    reply.DataItem[0].AddList();

            //    bool ret = true;

            //    switch (objType)
            //    {
            //        case "controljob":
            //            foreach (var cjName in objIds)
            //            {
            //                if (!_equipment.GetFAControlJobs().ContainsKey(cjName))
            //                {
            //                    reply.DataItem[0].AddList();
            //                    reply.DataItem[0][1].Add("ACK", 1, SECsFormat.U4);
            //                    reply.DataItem[0][1].AddList();
            //                    reply.DataItem[0][1][1].Add("ERRCODE", 1, SECsFormat.U4);
            //                    reply.DataItem[0][1][1].Add("ERRTEXT", "NotFound", SECsFormat.Ascii);

            //                    _gem.SendReply(reply, e.TransactionID);
            //                    return;
            //                }
            //                if (_equipment.GetFAControlJobs()[cjName].state == ControlJobState.EXECUTING)
            //                {
            //                    reply.DataItem[0].AddList();
            //                    reply.DataItem[0][1].Add("ACK", 1, SECsFormat.U4);
            //                    reply.DataItem[0][1].AddList();
            //                    reply.DataItem[0][1][1].Add("ERRCODE", 2, SECsFormat.U4);
            //                    reply.DataItem[0][1][1].Add("ERRTEXT", "JobExecuting", SECsFormat.Ascii);
            //                    _gem.SendReply(reply, e.TransactionID);
            //                    return;
            //                }
            //            }
            //            foreach (var cjName in objIds)
            //            {
            //                var cj = _equipment.GetFAControlJobs()[cjName];

            //                for (int i = 0; i < dataItem[0][3].Count; i++)
            //                {
            //                    string attrName = dataItem[0][3][i][0].Value.ToString().ToLower();
            //                    switch (attrName)
            //                    {
            //                        case "mtrloutspec":
            //                            SECsDataItem outSpecItem = dataItem[0][3][i][1];

            //                            List<MtrlOutSpecPair> newMtrlOutSpec = new List<MtrlOutSpecPair>();
            //                            for (int j = 0; j < outSpecItem.Count; j++)
            //                            {
            //                                MtrlOutSpecPair mtrl = new MtrlOutSpecPair()
            //                                {
            //                                    sourceCarID = outSpecItem[j][0][0].Value.ToString(),
            //                                    DestCarID = outSpecItem[j][1][0].Value.ToString(),
            //                                    sourceSlots = new List<int>(),
            //                                    destSlots = new List<int>(),
            //                                };

            //                                for (int k = 0; k < outSpecItem[j][0][1].Count; k++)
            //                                {
            //                                    mtrl.sourceSlots.Add(outSpecItem[j][0][1][k].ToInt());
            //                                }
            //                                for (int k = 0; k < outSpecItem[j][1][1].Count; k++)
            //                                {
            //                                    mtrl.destSlots.Add(outSpecItem[j][1][1][k].ToInt());
            //                                }
            //                                newMtrlOutSpec.Add(mtrl);

            //                            }

            //                            cj.MtrlOutSpec = newMtrlOutSpec;
            //                            break;
            //                    }
            //                }


            //                SECsDataItem JobReturnItem = new SECsDataItem(SECsFormat.List);
            //                JobReturnItem.Add("ObjID", cjName, SECsFormat.Ascii);
            //                JobReturnItem.Add(dataItem[0][3]);
            //                reply.DataItem[0][0].Add(JobReturnItem);

            //            }

            //            reply.DataItem[0].AddList();
            //            reply.DataItem[0][1].AddList();
            //            reply.DataItem[0][1][0].Add("ACK", 0, SECsFormat.U4);
            //            reply.DataItem[0][1][0].AddList();


            //            break;



            //    }




            //    _gem.SendReply(reply, e.TransactionID);
            //}
            //catch (Exception ex)
            //{
            //    //LOG.Write("Handle_S14F9 exception:" + ex.Message);
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(14, 0, false);
            //    _gem.SendReply(reply, e.TransactionID);
            //}
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
            //try
            //{
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(14, 2, false);
            //    string reason = string.Empty;
            //    LOG.Write("Handle_S14F1, Get Attribute");
            //    var dataitem = e.Inputs.DataItem;
            //    string objtype = e.Inputs.DataItem[0][1].ToString().ToLower();
            //    List<string> objIds = new List<string>();
            //    for (int n = 0; n < dataitem[0][2].Count; n++)
            //    {
            //        objIds.Add(dataitem[0][2][n].ToString());
            //    }


            //    bool ret = false;
            //    int i = 0;
            //    switch (objtype)
            //    {
            //        case "controljob":
            //            reply.DataItem.AddList();
            //            reply.DataItem[0].AddList();


            //            if (objIds == null || objIds.Count == 0)
            //            {
            //                foreach (var cj in _equipment.GetFAControlJobs().Values)
            //                {

            //                    reply.DataItem[0][0].AddList();
            //                    reply.DataItem[0][0][i].Add("ObjID", cj.ObjtID, SECsFormat.Ascii);
            //                    reply.DataItem[0][0][i].AddList();


            //                    SECsDataItem itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "ObjID", SECsFormat.Ascii);
            //                    itemAttribute.Add("AttributeValue", cj.ObjtID, SECsFormat.Ascii);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);


            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "CarrierInputSpec", SECsFormat.Ascii);
            //                    SECsDataItem attrValue = new SECsDataItem(SECsFormat.List);
            //                    foreach (var cid in cj.CarrierIputSpec)
            //                    {
            //                        attrValue.Add("AttributeValue", cid, SECsFormat.Ascii);
            //                    }
            //                    itemAttribute.Add(attrValue);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);


            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "MtrlOutSpec", SECsFormat.Ascii);
            //                    attrValue = new SECsDataItem(SECsFormat.List);
            //                    if (cj.MtrlOutSpec != null)
            //                    {
            //                        foreach (var mtrl in cj.MtrlOutSpec)
            //                        {
            //                            SECsDataItem item = new SECsDataItem(SECsFormat.List);

            //                            SECsDataItem itemSouce = new SECsDataItem(SECsFormat.List);
            //                            SECsDataItem itemDest = new SECsDataItem(SECsFormat.List);
            //                            SECsDataItem itemSouceslots = new SECsDataItem(SECsFormat.List);
            //                            SECsDataItem itemDestSlots = new SECsDataItem(SECsFormat.List);

            //                            itemSouce.Add("SourceCarrier", mtrl.sourceCarID, SECsFormat.Ascii);
            //                            foreach (var slot in mtrl.sourceSlots)
            //                            {
            //                                itemSouceslots.Add("SourceSlot", slot, SECsFormat.U1);
            //                            }
            //                            itemSouce.Add(itemSouceslots);

            //                            foreach (var slot in mtrl.destSlots)
            //                            {
            //                                itemDestSlots.Add("DestSlot", slot, SECsFormat.U1);
            //                            }
            //                            itemDest.Add(itemSouceslots);

            //                            item.Add(itemSouce);
            //                            item.Add(itemDest);

            //                            attrValue.Add(item);
            //                        }
            //                    }
            //                    itemAttribute.Add(attrValue);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);


            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "ProcessOrderMgmt", SECsFormat.Ascii);
            //                    itemAttribute.Add("AttributeValue", (int)cj.ProcessingOrderMgmt, SECsFormat.U1);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);


            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "StartMethod", SECsFormat.Ascii);
            //                    itemAttribute.Add("AttributeValue", cj.StartMethod, SECsFormat.Boolean);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);

            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "ProcessingCtrlSpec", SECsFormat.Ascii);
            //                    attrValue = new SECsDataItem(SECsFormat.List);
            //                    foreach (var pCtrl in cj.ProcessingCtrlSpec)
            //                    {
            //                        SECsDataItem item = new SECsDataItem(SECsFormat.List);
            //                        item.Add("PJID", pCtrl, SECsFormat.Ascii);
            //                        item.AddList();
            //                        item.AddList();
            //                        attrValue.Add(item);
            //                    }
            //                    itemAttribute.Add(attrValue);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);

            //                    i++;
            //                }

            //            }
            //            else
            //            {
            //                foreach (var cjName in objIds)
            //                {
            //                    if (!_equipment.GetFAControlJobs().ContainsKey(cjName))
            //                    {
            //                        reply.DataItem[0].AddList();
            //                        reply.DataItem[0][1].Add("ACK", 1, SECsFormat.U4);
            //                        reply.DataItem[0][1].AddList();
            //                        reply.DataItem[0][1][1].Add("ERRCODE", 1, SECsFormat.U4);
            //                        reply.DataItem[0][1][1].Add("ERRTEXT", "NotFound", SECsFormat.Ascii);
            //                        _gem.SendReply(reply, e.TransactionID);
            //                        return;
            //                    }
            //                    var cj = _equipment.GetFAControlJobs()[cjName];
            //                    reply.DataItem[0][0].AddList();
            //                    reply.DataItem[0][0][i].Add("ObjID", cj.ObjtID, SECsFormat.Ascii);
            //                    reply.DataItem[0][0][i].AddList();


            //                    SECsDataItem itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "ObjID", SECsFormat.Ascii);
            //                    itemAttribute.Add("AttributeValue", cj.ObjtID, SECsFormat.Ascii);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);


            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "CarrierInputSpec", SECsFormat.Ascii);
            //                    SECsDataItem attrValue = new SECsDataItem(SECsFormat.List);
            //                    foreach (var cid in cj.CarrierIputSpec)
            //                    {
            //                        attrValue.Add("AttributeValue", cid, SECsFormat.Ascii);
            //                    }
            //                    itemAttribute.Add(attrValue);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);


            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "MtrlOutSpec", SECsFormat.Ascii);
            //                    attrValue = new SECsDataItem(SECsFormat.List);
            //                    if (cj.MtrlOutSpec != null)
            //                    {
            //                        foreach (var mtrl in cj.MtrlOutSpec)
            //                        {
            //                            SECsDataItem item = new SECsDataItem(SECsFormat.List);

            //                            SECsDataItem itemSouce = new SECsDataItem(SECsFormat.List);
            //                            SECsDataItem itemDest = new SECsDataItem(SECsFormat.List);
            //                            SECsDataItem itemSouceslots = new SECsDataItem(SECsFormat.List);
            //                            SECsDataItem itemDestSlots = new SECsDataItem(SECsFormat.List);

            //                            itemSouce.Add("SourceCarrier", mtrl.sourceCarID, SECsFormat.Ascii);
            //                            foreach (var slot in mtrl.sourceSlots)
            //                            {
            //                                itemSouceslots.Add("SourceSlot", slot, SECsFormat.U1);
            //                            }
            //                            itemSouce.Add(itemSouceslots);

            //                            foreach (var slot in mtrl.destSlots)
            //                            {
            //                                itemDestSlots.Add("DestSlot", slot, SECsFormat.U1);
            //                            }
            //                            itemDest.Add(itemSouceslots);

            //                            item.Add(itemSouce);
            //                            item.Add(itemDest);

            //                            attrValue.Add(item);
            //                        }
            //                    }
            //                    itemAttribute.Add(attrValue);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);


            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "ProcessOrderMgmt", SECsFormat.Ascii);
            //                    itemAttribute.Add("AttributeValue", (int)cj.ProcessingOrderMgmt, SECsFormat.U1);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);


            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "StartMethod", SECsFormat.Ascii);
            //                    itemAttribute.Add("AttributeValue", cj.StartMethod, SECsFormat.Boolean);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);

            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "ProcessingCtrlSpec", SECsFormat.Ascii);
            //                    attrValue = new SECsDataItem(SECsFormat.List);
            //                    foreach (var pCtrl in cj.ProcessingCtrlSpec)
            //                    {
            //                        SECsDataItem item = new SECsDataItem(SECsFormat.List);
            //                        item.Add("PJID", pCtrl, SECsFormat.Ascii);
            //                        item.AddList();
            //                        item.AddList();
            //                        attrValue.Add(item);
            //                    }
            //                    itemAttribute.Add(attrValue);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);

            //                    i++;
            //                }
            //            }


            //            reply.DataItem[0].AddList();
            //            reply.DataItem[0][1].Add("ACK", 0, SECsFormat.U4);
            //            reply.DataItem[0][1].AddList();

            //            ret = true;


            //            break;

            //        case "processjob":
            //            reply.DataItem.AddList();
            //            reply.DataItem[0].AddList();
            //            i = 0;

            //            if (objIds == null || objIds.Count == 0)
            //            {
            //                foreach (var pj in _equipment.GetFAProcessJobs().Values)
            //                {
            //                    reply.DataItem[0][0].AddList();
            //                    reply.DataItem[0][0][i].Add("ObjID", pj.ObjtID, SECsFormat.Ascii);
            //                    reply.DataItem[0][0][i].AddList();


            //                    SECsDataItem itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "ObjType", SECsFormat.Ascii);
            //                    itemAttribute.Add("AttributeValue", "PROCESSJOB", SECsFormat.Ascii);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);


            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "ObjID", SECsFormat.Ascii);
            //                    itemAttribute.Add("AttributeValue", pj.ObjtID, SECsFormat.Ascii);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);


            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "PauseEvent", SECsFormat.Ascii);
            //                    SECsDataItem attrValue = new SECsDataItem(SECsFormat.List);
            //                    foreach (var pevt in pj.PauseEvent)
            //                    {
            //                        attrValue.Add("PEVT", pevt, SECsFormat.U4);

            //                    }
            //                    itemAttribute.Add(attrValue);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);

            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "PRJobState", SECsFormat.Ascii);
            //                    itemAttribute.Add("AttributeValue", (int)pj.PJstate, SECsFormat.U1);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);

            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "PRMtlNameList", SECsFormat.Ascii);
            //                    attrValue = new SECsDataItem(SECsFormat.List);
            //                    foreach (var mtrl in pj.PRMtlNameList)
            //                    {
            //                        SECsDataItem item = new SECsDataItem(SECsFormat.List);
            //                        item.Add("CarrierID", mtrl.CarrierID, SECsFormat.Ascii);
            //                        SECsDataItem itemSlots = new SECsDataItem(SECsFormat.List);
            //                        foreach (var slot in mtrl.SlotID)
            //                        {
            //                            itemSlots.Add("Slot", slot, SECsFormat.U1);
            //                        }
            //                        item.Add(itemSlots);
            //                        attrValue.Add(item);
            //                    }
            //                    itemAttribute.Add(attrValue);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);


            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "PRMtlType", SECsFormat.Ascii);
            //                    itemAttribute.Add("AttributeValue", (int)pj.PRMtlType, SECsFormat.U1);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);

            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "PRProcessStart", SECsFormat.Ascii);
            //                    itemAttribute.Add("AttributeValue", pj.PRProcessStart, SECsFormat.Boolean);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);

            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "PRRecipeMethod", SECsFormat.Ascii);
            //                    itemAttribute.Add("AttributeValue", (int)pj.PRRecipeMethod, SECsFormat.U1);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);

            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "RecID", SECsFormat.Ascii);
            //                    itemAttribute.Add("AttributeValue", pj.RecID, SECsFormat.Ascii);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);


            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "RecVariableList", SECsFormat.Ascii);
            //                    attrValue = new SECsDataItem(SECsFormat.List);
            //                    foreach (var variable in pj.RecVariableList)
            //                    {
            //                        SECsDataItem item = new SECsDataItem(SECsFormat.List);
            //                        item.Add("ParaName", variable.Key, SECsFormat.Ascii);
            //                        item.Add("ParaValue", variable.Value);

            //                    }
            //                    itemAttribute.Add(attrValue);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);


            //                }





            //            }
            //            else
            //            {
            //                foreach (var pjName in objIds)
            //                {
            //                    if (!_equipment.GetFAProcessJobs().ContainsKey(pjName))
            //                    {
            //                        reply.DataItem[0].AddList();
            //                        reply.DataItem[0][1].Add("ACK", 1, SECsFormat.U4);
            //                        reply.DataItem[0][1].AddList();
            //                        reply.DataItem[0][1][1].Add("ERRCODE", 1, SECsFormat.U4);
            //                        reply.DataItem[0][1][1].Add("ERRTEXT", "NotFound", SECsFormat.Ascii);
            //                        _gem.SendReply(reply, e.TransactionID);
            //                        return;
            //                    }
            //                    var pj = _equipment.GetFAProcessJobs()[pjName];

            //                    reply.DataItem[0][0].AddList();
            //                    reply.DataItem[0][0][i].Add("ObjID", pj.ObjtID, SECsFormat.Ascii);
            //                    reply.DataItem[0][0][i].AddList();


            //                    SECsDataItem itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "ObjType", SECsFormat.Ascii);
            //                    itemAttribute.Add("AttributeValue", "PROCESSJOB", SECsFormat.Ascii);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);


            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "ObjID", SECsFormat.Ascii);
            //                    itemAttribute.Add("AttributeValue", pj.ObjtID, SECsFormat.Ascii);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);


            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "PauseEvent", SECsFormat.Ascii);
            //                    SECsDataItem attrValue = new SECsDataItem(SECsFormat.List);
            //                    foreach (var pevt in pj.PauseEvent)
            //                    {
            //                        attrValue.Add("PEVT", pevt, SECsFormat.U4);

            //                    }
            //                    itemAttribute.Add(attrValue);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);

            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "PRJobState", SECsFormat.Ascii);
            //                    itemAttribute.Add("AttributeValue", (int)pj.PJstate, SECsFormat.U1);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);

            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "PRMtlNameList", SECsFormat.Ascii);
            //                    attrValue = new SECsDataItem(SECsFormat.List);
            //                    foreach (var mtrl in pj.PRMtlNameList)
            //                    {
            //                        SECsDataItem item = new SECsDataItem(SECsFormat.List);
            //                        item.Add("CarrierID", mtrl.CarrierID, SECsFormat.Ascii);
            //                        SECsDataItem itemSlots = new SECsDataItem(SECsFormat.List);
            //                        foreach (var slot in mtrl.SlotID)
            //                        {
            //                            itemSlots.Add("Slot", slot, SECsFormat.U1);
            //                        }
            //                        item.Add(itemSlots);
            //                        attrValue.Add(item);
            //                    }
            //                    itemAttribute.Add(attrValue);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);


            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "PRMtlType", SECsFormat.Ascii);
            //                    itemAttribute.Add("AttributeValue", (int)pj.PRMtlType, SECsFormat.U1);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);

            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "PRProcessStart", SECsFormat.Ascii);
            //                    itemAttribute.Add("AttributeValue", pj.PRProcessStart, SECsFormat.Boolean);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);

            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "PRRecipeMethod", SECsFormat.Ascii);
            //                    itemAttribute.Add("AttributeValue", (int)pj.PRRecipeMethod, SECsFormat.U1);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);

            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "RecID", SECsFormat.Ascii);
            //                    itemAttribute.Add("AttributeValue", pj.RecID, SECsFormat.Ascii);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);


            //                    itemAttribute = new SECsDataItem(SECsFormat.List);
            //                    itemAttribute.Add("AttributeName", "RecVariableList", SECsFormat.Ascii);
            //                    attrValue = new SECsDataItem(SECsFormat.List);
            //                    foreach (var variable in pj.RecVariableList)
            //                    {
            //                        SECsDataItem item = new SECsDataItem(SECsFormat.List);
            //                        item.Add("ParaName", variable.Key, SECsFormat.Ascii);
            //                        item.Add("ParaValue", variable.Value);

            //                    }
            //                    itemAttribute.Add(attrValue);
            //                    reply.DataItem[0][0][i][1].Add(itemAttribute);





            //                }





            //            }


            //            reply.DataItem[0].AddList();
            //            reply.DataItem[0][1].Add("ACK", 0, SECsFormat.U4);
            //            reply.DataItem[0][1].AddList();

            //            break;


            //        default:
            //            reply.DataItem.AddList();
            //            reply.DataItem[0].AddList();
            //            reply.DataItem[0].AddList();
            //            reply.DataItem[0][1].Add("ACK", 1, SECsFormat.U4);
            //            reply.DataItem[0][1].AddList();
            //            reply.DataItem[0][1][1].Add("ERRCODE", 1, SECsFormat.U4);
            //            reply.DataItem[0][1][1].Add("ERRTEXT", "NotFound", SECsFormat.Ascii);

            //            break;
            //    }



            //    _gem.SendReply(reply, e.TransactionID);
            //}
            //catch (Exception ex)
            //{
            //    LOG.Write("Handle_S14F1 exception:" + ex.Message);
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(14, 0, false);
            //    _gem.SendReply(reply, e.TransactionID);
            //}
        }
        private void HandleS3F29_ReadTag(SECsPrimaryInEventArgs e)
        {
            //try
            //{
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(3, 30, false);
            //    string reason = string.Empty;
            //    int portid = Convert.ToInt16(e.Inputs.DataItem[0][0].ToString().Replace("LP", ""));
            //    string tagid = e.Inputs.DataItem[0][1].ToString();
            //    int offset = Convert.ToInt16(e.Inputs.DataItem[0][2].ToString());
            //    int length = e.Inputs.DataItem[0][3].ToInt();
            //    //cid = e.Inputs.DataItem[0][4].ToString();


            //    bool result = _equipment.GetCarrierID(portid, offset, length, tagid, out string carrierId, out reason);

            //    reply.DataItem.AddList();

            //    reply.DataItem[0].Add("DATA", carrierId, SECsFormat.Ascii);

            //    reply.DataItem[0].AddList();
            //    if (result)
            //        reply.DataItem[0][1].Add("CAACK", 0, SECsFormat.U4);
            //    else
            //        reply.DataItem[0][1].Add("CAACK", 2, SECsFormat.U4);


            //    reply.DataItem[0][1].AddList();

            //    _gem.SendReply(reply, e.TransactionID);
            //}
            //catch (Exception ex)
            //{
            //    LOG.Write("Handle_S3F31 exception:" + ex.Message);
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(3, 0, false);
            //    _gem.SendReply(reply, e.TransactionID);
            //}
        }

        private void HandleS3F31_WriteTag(SECsPrimaryInEventArgs e)
        {
            //try
            //{
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(3, 32, false);
            //    string reason = string.Empty;
            //    int portid = Convert.ToInt16(e.Inputs.DataItem[0][0].ToString().Replace("LP", ""));
            //    string tagid = e.Inputs.DataItem[0][1].ToString();
            //    int offset = Convert.ToInt16(e.Inputs.DataItem[0][2].ToString());
            //    int length = e.Inputs.DataItem[0][3].ToInt();
            //    string cid = e.Inputs.DataItem[0][4].ToString();


            //    bool result = _equipment.WriteCarrierID(portid, cid, offset, length, tagid, out _);

            //    reply.DataItem.AddList();
            //    if (result)
            //        reply.DataItem[0].Add("CAACK", 0, SECsFormat.U4);
            //    else
            //    {
            //        reply.DataItem[0].Add("CAACK", 2, SECsFormat.U4);
            //    }


            //    reply.DataItem[0].AddList();

            //    _gem.SendReply(reply, e.TransactionID);
            //}
            //catch (Exception ex)
            //{
            //    LOG.Write("Handle_S3F31 exception:" + ex.Message);
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(3, 0, false);
            //    _gem.SendReply(reply, e.TransactionID);
            //}
        }
        private void E87_CancelAllCarrier(SECsPrimaryInEventArgs e)
        {

        }

        private void E87_PortAction(SECsPrimaryInEventArgs e)
        {
            //try
            //{
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(3, 26, false);
            //    string reason = string.Empty;
            //    LOG.Write("Handle_S3F26, E87 Action: Port Action");

            //    string portaction = e.Inputs.DataItem[0][0].ToString();
            //    int portID = e.Inputs.DataItem[0][1].ToInt();
            //    bool ret = false;
            //    switch (portaction.ToLower())
            //    {
            //        case "cancelreservationatport":

            //            break;
            //        case "changeaccess":
            //            if (e.Inputs.DataItem[0][2][0][0].ToString().ToLower() != "accessmode") break;
            //            if (e.Inputs.DataItem[0][2][0][1].ToInt() == 0)
            //                Singleton<FAJobController>.Instance.FAloadports[((ModuleName)portID).ToString()].SetAccessMode(false);
            //            else
            //                Singleton<FAJobController>.Instance.FAloadports[((ModuleName)portID).ToString()].SetAccessMode(true); ;
            //            ret = true;
            //            break;
            //        case "changeservicestatus":
            //            if (e.Inputs.DataItem[0][2][0][0].ToString().ToLower() != "servicestatus") break;
            //            if (e.Inputs.DataItem[0][2][0][1].ToInt() == 0)
            //                Singleton<FAJobController>.Instance.FAloadports[((ModuleName)portID).ToString()].SetLPtransferInService(false);
            //            else
            //                Singleton<FAJobController>.Instance.FAloadports[((ModuleName)portID).ToString()].SetLPtransferInService(true);
            //            ret = true;
            //            break;
            //        case "reserveatport":
            //            break;
            //        default:
            //            break;

            //    }



            //    reply.DataItem.AddList();
            //    if (ret)
            //        reply.DataItem[0].Add("CAACK", 0, SECsFormat.U4);
            //    else reply.DataItem[0].Add("CAACK", 1, SECsFormat.U4);
            //    reply.DataItem[0].Add("REASON", "ACK", SECsFormat.Ascii);
            //    _gem.SendReply(reply, e.TransactionID);
            //}
            //catch (Exception ex)
            //{
            //    LOG.Write("Handle_S14F9 exception:" + ex.Message);
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(3, 0, false);
            //    _gem.SendReply(reply, e.TransactionID);
            //}
        }
        private void E87_ChangeAccessModeList(SECsPrimaryInEventArgs e)
        {
            //try
            //{
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(3, 28, false);
            //    string reason = string.Empty;
            //    bool ret = true;
            //    LOG.Write("Handle_S3F28, E87 Action: Change access list");
            //    int portaction = e.Inputs.DataItem[0][0].ToInt();
            //    if (portaction != 0 && portaction != 1)
            //    {
            //        ret = false;
            //        reason = "Manual=0,Auto=1,Valid Data Ether";
            //    }
            //    List<int> ports = new List<int>();
            //    for (int i = 0; i < e.Inputs.DataItem[0][1].Count; i++)
            //    {
            //        int portID = e.Inputs.DataItem[0][1][i].ToInt();
            //        ports.Add(portID);
            //        //if (portID != 1 && portID != 2 && portID != 3 && portID != 4)
            //        //{
            //        //    ret = false;
            //        //    reason = "port need in range 1-4";
            //        //}
            //    }
            //    if (ret)
            //    {
            //        for (int i = 0; i < ports.Count; i++)
            //        {
            //            if (portaction == 0)
            //            {
            //                Singleton<FAJobController>.Instance.FAloadports[((ModuleName)ports[i]).ToString()].SetAccessMode(false);
            //            }
            //            else if (portaction == 1)
            //            {
            //                Singleton<FAJobController>.Instance.FAloadports[((ModuleName)ports[i]).ToString()].SetAccessMode(true);
            //            }
            //        }
            //    }
            //    reply.DataItem.AddList();
            //    reply.DataItem[0].Add("CAACK", 0, SECsFormat.U4);
            //    reply.DataItem[0].Add("REASON", "ACK", SECsFormat.Ascii);
            //    _gem.SendReply(reply, e.TransactionID);
            //}
            //catch (Exception ex)
            //{
            //    LOG.Write("Handle_S3F27 exception:" + ex.Message);
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(3, 0, false);
            //    _gem.SendReply(reply, e.TransactionID);
            //}
        }


        private void E94_Handle_CtrJobCreate(SECsPrimaryInEventArgs e)
        {
            //try
            //{
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(14, 10, false);
            //    string reason = string.Empty;
            //    LOG.Write("Handle_S14F9, E94 Action: Create Job");
            //    E94ControlJob facj;
            //    if (!ParseControlJobObj(e, out facj))
            //    {
            //        reply.DataItem.AddList();
            //        reply.DataItem[0].Add("OBJSPEC", facj.ObjtID, SECsFormat.Ascii);
            //        reply.DataItem[0].AddList();
            //        reply.DataItem[0].AddList();


            //        reply.DataItem[0][2].Add("OBJACK", 1, SECsFormat.U4);
            //        reply.DataItem[0][2].AddList();

            //        reply.DataItem[0][2][1].AddList();
            //        reply.DataItem[0][2][1][0].Add("ERRORCODE", 1, SECsFormat.U4);
            //        reply.DataItem[0][2][1][0].Add("ERRTEXT", "CJ format error", SECsFormat.Ascii);
            //        _gem.SendReply(reply, e.TransactionID);
            //        return;
            //    }

            //    if (!Singleton<FAJobController>.Instance.CJCreate(facj))
            //    {
            //        reply.DataItem.AddList();
            //        reply.DataItem[0].Add("OBJSPEC", facj.ObjtID, SECsFormat.Ascii);
            //        reply.DataItem[0].AddList();

            //        reply.DataItem[0][1].AddList();
            //        reply.DataItem[0][1][0].Add("ATTRID", "CarrierInputSpec", SECsFormat.Ascii);
            //        reply.DataItem[0][1][0].AddList();
            //        foreach (string strcid in facj.CarrierIputSpec)
            //        {
            //            reply.DataItem[0][1][0][1].Add("CarrierID", strcid, SECsFormat.Ascii);
            //        }

            //        reply.DataItem[0][1].AddList();
            //        reply.DataItem[0][1][1].Add("ATTRID", "MtrlOutSpec", SECsFormat.Ascii);
            //        for (int i = 0; i < facj.MtrlOutSpec.Count; i++)
            //        {
            //            reply.DataItem[0][1][1].AddList();

            //            reply.DataItem[0][1][1][i].AddList();
            //            reply.DataItem[0][1][1][i][0].Add("FromCarrier", facj.MtrlOutSpec[i].sourceCarID, SECsFormat.Ascii);
            //            reply.DataItem[0][1][1][i][0].AddList();
            //            foreach (int fromslot in facj.MtrlOutSpec[i].sourceSlots)
            //                reply.DataItem[0][1][1][i][0][1].Add("FromSlot", fromslot, SECsFormat.U1);

            //            reply.DataItem[0][1][1][i].AddList();
            //            reply.DataItem[0][1][1][i][1].Add("ToCarrier", facj.MtrlOutSpec[i].DestCarID, SECsFormat.Ascii);
            //            reply.DataItem[0][1][1][i][1].AddList();
            //            foreach (int toslot in facj.MtrlOutSpec[i].destSlots)
            //                reply.DataItem[0][1][1][i][1][1].Add("ToSlot", toslot, SECsFormat.U1);
            //        }

            //        reply.DataItem[0][1].AddList();
            //        reply.DataItem[0][1][2].Add("ATTRID", "ProcessOrderMgmt", SECsFormat.Ascii);
            //        reply.DataItem[0][1][2].Add("ATTDATA", (int)facj.ProcessingOrderMgmt, SECsFormat.U1);

            //        reply.DataItem[0][1].AddList();
            //        reply.DataItem[0][1][3].Add("ATTRID", "StartMethod", SECsFormat.Ascii);
            //        reply.DataItem[0][1][3].Add("ATTDATA", facj.StartMethod, SECsFormat.Boolean);

            //        reply.DataItem[0][1].AddList();
            //        reply.DataItem[0][1][4].Add("ATTRID", "ProcessingCtrlSpec", SECsFormat.Ascii);
            //        reply.DataItem[0][1][4].AddList();
            //        for (int i = 0; i < facj.ProcessingCtrlSpec.Count; i++)
            //        {
            //            reply.DataItem[0][1][4][1].AddList();
            //            reply.DataItem[0][1][4][1][i].Add("ProcessJobID", facj.ProcessingCtrlSpec[i], SECsFormat.Ascii);
            //            reply.DataItem[0][1][4][1][i].AddList();
            //        }


            //        reply.DataItem[0].AddList();


            //        reply.DataItem[0][2].Add("OBJACK", 1, SECsFormat.U4);
            //        reply.DataItem[0][2].AddList();

            //        reply.DataItem[0][2][1].AddList();
            //        reply.DataItem[0][2][1][0].Add("ERRORCODE", 2, SECsFormat.U4);
            //        reply.DataItem[0][2][1][0].Add("ERRTEXT", "CJ create fail", SECsFormat.Ascii);
            //        _gem.SendReply(reply, e.TransactionID);
            //        return;
            //    }

            //    reply.DataItem.AddList();
            //    reply.DataItem[0].Add("OBJPEC", facj.ObjtID, SECsFormat.Ascii);
            //    reply.DataItem[0].AddList();

            //    reply.DataItem[0][1].AddList();
            //    reply.DataItem[0][1][0].Add("ATTRID", "CarrierInputSpec", SECsFormat.Ascii);
            //    reply.DataItem[0][1][0].AddList();
            //    foreach (string strcid in facj.CarrierIputSpec)
            //    {
            //        reply.DataItem[0][1][0][1].Add("CarrierID", strcid, SECsFormat.Ascii);
            //    }

            //    reply.DataItem[0][1].AddList();
            //    reply.DataItem[0][1][1].Add("ATTRID", "MtrlOutSpec", SECsFormat.Ascii);
            //    reply.DataItem[0][1][1].AddList();
            //    if (facj.MtrlOutSpec != null)
            //        for (int i = 0; i < facj.MtrlOutSpec.Count; i++)
            //        {
            //            reply.DataItem[0][1][1][1].AddList();

            //            reply.DataItem[0][1][1][1][i].AddList();
            //            reply.DataItem[0][1][1][1][i][0].Add("FromCarrier", facj.MtrlOutSpec[i].sourceCarID, SECsFormat.Ascii);
            //            reply.DataItem[0][1][1][1][i][0].AddList();
            //            foreach (int fromslot in facj.MtrlOutSpec[i].sourceSlots)
            //                reply.DataItem[0][1][1][1][i][0][1].Add("FromSlot", fromslot, SECsFormat.U1);

            //            reply.DataItem[0][1][1][1][i].AddList();
            //            reply.DataItem[0][1][1][1][i][1].Add("ToCarrier", facj.MtrlOutSpec[i].DestCarID, SECsFormat.Ascii);
            //            reply.DataItem[0][1][1][1][i][1].AddList();
            //            foreach (int toslot in facj.MtrlOutSpec[i].destSlots)
            //                reply.DataItem[0][1][1][1][i][1][1].Add("ToSlot", toslot, SECsFormat.U1);
            //        }

            //    reply.DataItem[0][1].AddList();
            //    reply.DataItem[0][1][2].Add("ATTRID", "ProcessOrderMgmt", SECsFormat.Ascii);
            //    reply.DataItem[0][1][2].Add("ATTDATA", (int)facj.ProcessingOrderMgmt, SECsFormat.U1);

            //    reply.DataItem[0][1].AddList();
            //    reply.DataItem[0][1][3].Add("ATTRID", "StartMethod", SECsFormat.Ascii);
            //    reply.DataItem[0][1][3].Add("ATTDATA", facj.StartMethod, SECsFormat.Boolean);

            //    reply.DataItem[0][1].AddList();
            //    reply.DataItem[0][1][4].Add("ATTRID", "ProcessingCtrlSpec", SECsFormat.Ascii);
            //    reply.DataItem[0][1][4].AddList();
            //    for (int i = 0; i < facj.ProcessingCtrlSpec.Count; i++)
            //    {
            //        reply.DataItem[0][1][4][1].AddList();
            //        reply.DataItem[0][1][4][1][i].Add("ProcessJobID", facj.ProcessingCtrlSpec[i], SECsFormat.Ascii);
            //        reply.DataItem[0][1][4][1][i].AddList();
            //    }


            //    reply.DataItem[0].AddList();


            //    reply.DataItem[0][2].Add("OBJACK", 0, SECsFormat.U4);
            //    reply.DataItem[0][2].AddList();

            //    reply.DataItem[0][2][1].AddList();

            //    _gem.SendReply(reply, e.TransactionID);
            //}
            //catch (Exception ex)
            //{
            //    LOG.Write("Handle_S14F9 exception:" + ex.Message);
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(14, 0, false);
            //    _gem.SendReply(reply, e.TransactionID);
            //}
        }
        private void E94_Handle_CtrJobCMD(SECsPrimaryInEventArgs e)
        {
            //try
            //{
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(16, 28, false);
            //    string reason = string.Empty;
            //    LOG.Write("Handle_S16F27, E40 Action: CJ Command");
            //    string cjID = e.Inputs.DataItem[0][0].ToString();
            //    CJOperation cjop = (CJOperation)e.Inputs.DataItem[0][1].ToInt();


            //    if (!_equipment.GetFAControlJobs().ContainsKey(cjID))
            //    {
            //        LOG.Write("Handle_S16F6 Error JobID not Existed!");
            //        reply.DataItem.AddList();
            //        reply.DataItem[0].Add("ACKA", false, SECsFormat.Boolean);
            //        reply.DataItem[0].AddList();
            //        reply.DataItem[0][1].Add("ERRCODE", 1, SECsFormat.U4);
            //        reply.DataItem[0][1].Add("ERRTEXT", "NotFound", SECsFormat.Ascii);
            //        _gem.SendReply(reply, e.TransactionID);
            //        return;
            //    }
            //    var dvid = new SerializableDictionary<string, object>();
            //    dvid[DVIDName.CJID] = cjID;

            //    bool ret;
            //    switch (cjop)
            //    {
            //        case CJOperation.ABORT:
            //            ret = Singleton<FAJobController>.Instance.CJAbort(cjID);
            //            break;
            //        case CJOperation.CANCEL:
            //            ret = Singleton<FAJobController>.Instance.CJCancelled(cjID);
            //            break;

            //        case CJOperation.DESELECT:
            //            ret = Singleton<FAJobController>.Instance.CJDeselect(cjID);
            //            break;
            //        case CJOperation.HOQ:
            //            ret = true;
            //            break;
            //        case CJOperation.PAUSE:
            //            ret = Singleton<FAJobController>.Instance.CJPause(cjID);
            //            break;
            //        case CJOperation.RESUME:
            //            ret = Singleton<FAJobController>.Instance.CJResume(cjID);
            //            break;
            //        case CJOperation.START:
            //            ret = Singleton<FAJobController>.Instance.CJStart(cjID);
            //            break;
            //        case CJOperation.STOP:
            //            ret = Singleton<FAJobController>.Instance.CJStop(cjID);
            //            break;
            //        default:
            //            ret = false;
            //            return;
            //    }

            //    reply.DataItem.AddList();
            //    reply.DataItem[0].Add("ACKA", ret, SECsFormat.Boolean);

            //    reply.DataItem[0].AddList();
            //    if (!ret)
            //    {
            //        reply.DataItem[0][1].Add("ERRCODE", 2, SECsFormat.U4);
            //        reply.DataItem[0][1].Add("ERRTEXT", "ExecuteFailed", SECsFormat.Ascii);
            //    }

            //    _gem.SendReply(reply, e.TransactionID);
            //}
            //catch (Exception ex)
            //{
            //    LOG.Write("Handle_S14F27 exception:" + ex.Message);
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(14, 0, false);
            //    _gem.SendReply(reply, e.TransactionID);
            //}

        }
        private void E40_Handle_PJCMD(SECsPrimaryInEventArgs e)
        {
            //try
            //{
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(16, 6, false);
            //    string reason = string.Empty;
            //    LOG.Write("Handle_S16F5, E94 Action: PJ Command");
            //    string pjID = e.Inputs.DataItem[0][1].ToString();
            //    string cmd = e.Inputs.DataItem[0][2].ToString().ToUpper();
            //    bool ret = false;

            //    switch (cmd)
            //    {
            //        case "START":
            //            ret = Singleton<FAJobController>.Instance.PJStart(pjID);
            //            break;

            //        case "PAUSE":
            //            ret = Singleton<FAJobController>.Instance.PJPauseing(pjID);
            //            break;
            //        case "RESUME":
            //            ret = Singleton<FAJobController>.Instance.PJResume(pjID);
            //            break;
            //        case "STOP":
            //            ret = Singleton<FAJobController>.Instance.PJStopping(pjID);
            //            break;
            //        case "ABORT":
            //            ret = Singleton<FAJobController>.Instance.PJAbort(pjID);
            //            break;
            //        case "CANCEL":
            //            ret = Singleton<FAJobController>.Instance.PJCancel(pjID);
            //            break;
            //        default:
            //            ret = false;
            //            return;

            //    }
            //    reply.DataItem.AddList();
            //    reply.DataItem[0].Add("PRJOBID", pjID, SECsFormat.Ascii);
            //    reply.DataItem[0].AddList();
            //    if (ret)
            //        reply.DataItem[0][1].Add("CAACK", true, SECsFormat.Boolean);
            //    else
            //        reply.DataItem[0][1].Add("CAACK", false, SECsFormat.Boolean);
            //    reply.DataItem[0][1].AddList();
            //    reply.DataItem[0][1][1].Add("ERRORCODE", 0, SECsFormat.U4);
            //    reply.DataItem[0][1][1].Add("ERRTEXT", "", SECsFormat.Ascii);

            //    _gem.SendReply(reply, e.TransactionID);
            //}
            //catch (Exception ex)
            //{
            //    LOG.Write("Handle_S14F9 exception:" + ex.Message);
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(16, 0, false);
            //    _gem.SendReply(reply, e.TransactionID);
            //}
        }
        private void E40_Handle_PRJobCreateEnh(SECsPrimaryInEventArgs e)
        {
            //try
            //{
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(16, 12, false);
            //    string reason = string.Empty;
            //    LOG.Write("Handle_S16F11, E40 Action:");
            //    E40ProcessJob fapj;
            //    if (!ParseProcssJobObj(e, out fapj))
            //    {
            //        reply.DataItem.AddList();
            //        reply.DataItem[0].Add("PRJOBID", "", SECsFormat.Ascii);
            //        reply.DataItem[0].AddList();
            //        reply.DataItem[0][1].Add("ACKA", false, SECsFormat.Boolean);

            //        reply.DataItem[0][1].AddList();
            //        reply.DataItem[0][1][1].Add("ERRORCODE", 1, SECsFormat.U4);
            //        reply.DataItem[0][1][1].Add("REASON", "PJ Format Error", SECsFormat.Ascii);
            //        _gem.SendReply(reply, e.TransactionID);
            //        return;
            //    }
            //    if (fapj.RecID == "*null*") fapj.RecID = "";

            //    if (!GetRecipeList().Contains(fapj.RecID) || string.IsNullOrEmpty(fapj.RecID))
            //    {
            //        reply.DataItem.AddList();
            //        reply.DataItem[0].Add("PRJOBID", fapj.ObjtID, SECsFormat.Ascii);
            //        reply.DataItem[0].AddList();
            //        reply.DataItem[0][1].Add("ACKA", false, SECsFormat.Boolean);
            //        reply.DataItem[0][1].AddList();
            //        reply.DataItem[0][1][1].Add("ERRORCODE", 1, SECsFormat.U4);
            //        reply.DataItem[0][1][1].Add("REASON", "Invalid Recipe", SECsFormat.Ascii);
            //        _gem.SendReply(reply, e.TransactionID);
            //        return;
            //    }

            //    if (!Singleton<FAJobController>.Instance.PJEnhCreat(fapj))
            //    {
            //        reply.DataItem.AddList();
            //        reply.DataItem[0].Add("PRJOBID", fapj.ObjtID, SECsFormat.Ascii);
            //        reply.DataItem[0].AddList();
            //        reply.DataItem[0][1].Add("ACKA", false, SECsFormat.Boolean);

            //        reply.DataItem[0][1].AddList();
            //        reply.DataItem[0][1][1].Add("ERRORCODE", 2, SECsFormat.U4);
            //        reply.DataItem[0][1][1].Add("REASON", "PJ Create fail", SECsFormat.Ascii);
            //        _gem.SendReply(reply, e.TransactionID);
            //        return;
            //    }

            //    reply.DataItem.AddList();
            //    reply.DataItem[0].Add("PRJOBID", fapj.ObjtID, SECsFormat.Ascii);
            //    reply.DataItem[0].AddList();
            //    reply.DataItem[0][1].Add("ACKA", true, SECsFormat.Boolean);
            //    reply.DataItem[0][1].AddList();

            //    _gem.SendReply(reply, e.TransactionID);
            //}
            //catch (Exception ex)
            //{
            //    LOG.Write("Handle_S16F11 exception:" + ex.Message);
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(16, 0, false);
            //    _gem.SendReply(reply, e.TransactionID);
            //}
        }
        private void E40_Handle_PRJobDuplicateCreate(SECsPrimaryInEventArgs e)
        {
            //try
            //{
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(16, 16, false);
            //    string reason = string.Empty;
            //    LOG.Write("Handle_S16F15, E40 Action:");
            //    E40ProcessJob[] fapjs;
            //    if (!ParseMultiProcssJobObj(e, out fapjs))
            //    {
            //        for (int i = 0; i < fapjs.Length; i++)
            //        {
            //            reply.DataItem.AddList();
            //            reply.DataItem[i].Add("PRJOBID", "", SECsFormat.Ascii);
            //            reply.DataItem[i].AddList();
            //            reply.DataItem[i][1].Add("ACKA", false, SECsFormat.Boolean);

            //            reply.DataItem[i][1].AddList();
            //            reply.DataItem[i][1][1].Add("ERRORCODE", 1, SECsFormat.U4);
            //            reply.DataItem[i][1][1].Add("REASON", "PJ Format Error", SECsFormat.Ascii);
            //        }
            //        _gem.SendReply(reply, e.TransactionID);
            //        return;
            //    }
            //    object nobj;


            //    reply.DataItem.AddList();
            //    reply.DataItem[0].AddList();
            //    for (int i = 0; i < fapjs.Length; i++)
            //    {
            //        reply.DataItem[0][0].Add("PRJOBID", fapjs[i].ObjtID, SECsFormat.Ascii);
            //    }
            //    reply.DataItem[0].AddList();
            //    reply.DataItem[0][1].Add("ACKA", true, SECsFormat.Boolean);
            //    reply.DataItem[0][1].AddList();

            //    for (int i = 0; i < fapjs.Length; i++)
            //    {
            //        reply.DataItem[0][1][1].AddList();
            //        if (!Singleton<FAJobController>.Instance.PJEnhCreat(fapjs[i], out string innerReason))
            //        {
            //            reply.DataItem[0][1][0].Value = false;
            //            reply.DataItem[0][1][1][i].Add("ERRORCODE", 2, SECsFormat.U4);
            //            reply.DataItem[0][1][1][i].Add("REASON", "PJ Create fail . " + innerReason, SECsFormat.Ascii);
            //        }
            //    }

            //    _gem.SendReply(reply, e.TransactionID);
            //}
            //catch (Exception ex)
            //{
            //    LOG.Write("Handle_S16F15 exception:" + ex.Message);
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(16, 0, false);
            //    _gem.SendReply(reply, e.TransactionID);
            //}
        }
        private void E40_Handle_PRJobMultiCreate(SECsPrimaryInEventArgs e)
        {
            //try
            //{
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(16, 16, false);
            //    string reason = string.Empty;
            //    LOG.Write("Handle_S16F15, E40 Action:");
            //    E40ProcessJob[] fapjs;
            //    if (!ParseMultiProcssJobObj(e, out fapjs))
            //    {
            //        for (int i = 0; i < fapjs.Length; i++)
            //        {
            //            reply.DataItem.AddList();
            //            reply.DataItem[i].Add("PRJOBID", "", SECsFormat.Ascii);
            //            reply.DataItem[i].AddList();
            //            reply.DataItem[i][1].Add("ACKA", false, SECsFormat.Boolean);

            //            reply.DataItem[i][1].AddList();
            //            reply.DataItem[i][1][1].Add("ERRORCODE", 1, SECsFormat.U4);
            //            reply.DataItem[i][1][1].Add("REASON", "PJ Format Error", SECsFormat.Ascii);
            //        }
            //        _gem.SendReply(reply, e.TransactionID);
            //        return;
            //    }
            //    object nobj;


            //    reply.DataItem.AddList();
            //    reply.DataItem[0].AddList();
            //    for (int i = 0; i < fapjs.Length; i++)
            //    {
            //        reply.DataItem[0][0].Add("PRJOBID", fapjs[i].ObjtID, SECsFormat.Ascii);
            //    }
            //    reply.DataItem[0].AddList();
            //    reply.DataItem[0][1].Add("ACKA", true, SECsFormat.Boolean);
            //    reply.DataItem[0][1].AddList();

            //    for (int i = 0; i < fapjs.Length; i++)
            //    {
            //        reply.DataItem[0][1][1].AddList();
            //        if (!Singleton<FAJobController>.Instance.PJEnhCreat(fapjs[i], out string innerReason))
            //        {
            //            reply.DataItem[0][1][0].Value = false;
            //            reply.DataItem[0][1][1][i].Add("ERRORCODE", 2, SECsFormat.U4);
            //            reply.DataItem[0][1][1][i].Add("REASON", "PJ Create fail . " + innerReason, SECsFormat.Ascii);
            //        }
            //    }

            //    _gem.SendReply(reply, e.TransactionID);
            //}
            //catch (Exception ex)
            //{
            //    LOG.Write("Handle_S16F15 exception:" + ex.Message);
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(16, 0, false);
            //    _gem.SendReply(reply, e.TransactionID);
            //}
        }
        private void E40_Handle_PRJobDequeue(SECsPrimaryInEventArgs e)
        {
            //SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(16, 18, false);
            //reply.DataItem.AddList();
            //reply.DataItem[0].AddList();
            //reply.DataItem[0].AddList();

            //string reason = string.Empty;
            //LOG.Write("Handle_S16F17, Remove process jobs:");
            //bool ret = false;

            //for (int i = 0; i < e.Inputs.DataItem[0].Count; i++)
            //{
            //    ret = Singleton<FAJobController>.Instance.PJTerminate(e.Inputs.DataItem[0][i].ToString());
            //    if (ret) reply.DataItem[0][0].Add("PJob", e.Inputs.DataItem[0][i].ToString(), SECsFormat.Ascii);
            //}

            //if (ret)
            //{
            //    reply.DataItem[0][1].Add("ACK", true, SECsFormat.Boolean);
            //    reply.DataItem[0][1].AddList();
            //    reply.DataItem[0][1][1].Add("Code", 0, SECsFormat.I4);
            //    reply.DataItem[0][1][1].Add("Reason", "Success", SECsFormat.Ascii);
            //}
            //else
            //{
            //    reply.DataItem[0][1].Add("ACK", false, SECsFormat.Boolean);
            //    reply.DataItem[0][1].AddList();
            //    reply.DataItem[0][1][1].Add("Code", 3, SECsFormat.I4);
            //    reply.DataItem[0][1][1].Add("Reason", "Unknown process job", SECsFormat.Ascii);
            //}


            //_gem.SendReply(reply, e.TransactionID);
            //return;
        }
        private void E40_Handle_PRGetAllJobs(SECsPrimaryInEventArgs e)
        {
            //try
            //{
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(16, 20, false);
            //    string reason = string.Empty;
            //    int iPJ = 0;
            //    reply.DataItem.AddList();
            //    foreach (E40ProcessJob pj in Singleton<FAJobController>.Instance.FAProcessJobs.Values)
            //    {
            //        reply.DataItem[0].AddList();
            //        reply.DataItem[0][iPJ].Add("PJID", pj.ObjtID, SECsFormat.Ascii);
            //        reply.DataItem[0][iPJ].Add("PJ_State", (int)pj.PJstate, SECsFormat.I1);

            //    }
            //    LOG.Write("Handle_S16F20, E94 Action: Get all jobs.");

            //    _gem.SendReply(reply, e.TransactionID);
            //    return;

            //}
            //catch (Exception ex)
            //{
            //    LOG.Write("Handle_S16F19 exception:" + ex.Message);
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(16, 0, false);
            //    _gem.SendReply(reply, e.TransactionID);
            //}
        }
        private void E40_Handle_PRGetSpace(SECsPrimaryInEventArgs e)
        {
            //try
            //{
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(16, 22, false);
            //    string reason = string.Empty;

            //    int iPJSpace = 10 - Singleton<FAJobController>.Instance.FAProcessJobs.Values.Count;
            //    reply.DataItem.Add("PRJOBSPACE", iPJSpace >= 0 ? iPJSpace : 0, SECsFormat.I1);

            //    LOG.Write("Handle_S16F21, E94 Action: Get space.");

            //    _gem.SendReply(reply, e.TransactionID);
            //    return;

            //}
            //catch (Exception ex)
            //{
            //    LOG.Write("Handle_S16F21 exception:" + ex.Message);
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(16, 0, false);
            //    _gem.SendReply(reply, e.TransactionID);
            //}
        }
        private void E40_Handle_PRJobSetRecipeVariable(SECsPrimaryInEventArgs e)
        {

        }
        private void E40_Handle_PRJobSetStartMethod(SECsPrimaryInEventArgs e)
        {

        }
        private void E87_Handle_cms(SECsPrimaryInEventArgs e)
        {
            //try
            //{
            //    string reason = string.Empty;
            //    LOG.Write("Handle_S3F17, E87 Action:");
            //    string strAction = e.Inputs.DataItem[0][1].ToString();
            //    int PortID;
            //    FACarrier cobj;
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(3, 18, false);
            //    if (!ParseCarrierObj(e, out PortID, out cobj))
            //    {
            //        reply.DataItem.AddList();
            //        reply.DataItem[0].Add("CAACK", 1, SECsFormat.U1);
            //        reply.DataItem[0].AddList();
            //        reply.DataItem[0][1].Add("ERRCODE", 1, SECsFormat.U4);
            //        reply.DataItem[0][1].Add("REASON", "Invalid command.", SECsFormat.Ascii);
            //        _gem.SendReply(reply, e.TransactionID);

            //        return;
            //    }
            //    if (!_equipment.CarrierHandle(strAction, PortID, cobj, out reason))
            //    {
            //        reply.DataItem.AddList();
            //        reply.DataItem[0].Add("CAACK", 1, SECsFormat.U1);
            //        reply.DataItem[0].AddList();
            //        reply.DataItem[0][1].Add("ERRCODE", 2, SECsFormat.U4);
            //        reply.DataItem[0][1].Add("REASON", reason, SECsFormat.Ascii);
            //        _gem.SendReply(reply, e.TransactionID);
            //        return;
            //    }
            //    reply.DataItem.AddList();
            //    reply.DataItem[0].Add("CAACK", 0, SECsFormat.U1);
            //    reply.DataItem[0].AddList();
            //    _gem.SendReply(reply, e.TransactionID);
            //}
            //catch (Exception ex)
            //{
            //    LOG.Write("Handle_S3F17 exception:" + ex.Message);
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(3, 0, false);
            //    _gem.SendReply(reply, e.TransactionID);
            //}
        }
        //private bool ParseCarrierObj(SECsPrimaryInEventArgs e, out int PortID, out FACarrier carrierobj)
        //{
        //    bool ret = false;
        //    PortID = 0;
        //    carrierobj = new FACarrier();
        //    string[] paraname = { "ObjType","ObjID", "Capacity" , "CarrierAccessingStatus", "CarrierIDStatus", "ContentMap",
        //        "LocationID", "SlotMap" ,"SlotMapStatus","SubstrateCount","Usage"};
        //    try
        //    {
        //        carrierobj.ObjID = e.Inputs.DataItem[0][2].ToString();
        //        PortID = e.Inputs.DataItem[0][3].ToInt();
        //        for (int i = 0; i < e.Inputs.DataItem[0][4].Count; i++)
        //        {
        //            switch (e.Inputs.DataItem[0][4][i][0].ToString())
        //            {
        //                case "ObjType":
        //                    carrierobj.ObjectType = "Carrier";
        //                    break;
        //                case "ObjID":
        //                    //carrierobj.ObjID = e.Inputs.DataItem[0][4][i][1].ToString();
        //                    break;
        //                case "Capacity":
        //                    carrierobj.Capacity = e.Inputs.DataItem[0][4][i][1].ToUInt();
        //                    break;
        //                case "CarrierAccessingStatus":
        //                    carrierobj.Carrier_ACCESS_Status = (CarrierAccessingStatus)e.Inputs.DataItem[0][4][i][1].ToUInt();
        //                    break;
        //                case "CarrierIDStatus":
        //                    carrierobj.CarrierID_Satus = (CarrierIDStatus)e.Inputs.DataItem[0][4][i][1].ToUInt();
        //                    break;
        //                case "ContentMap":
        //                    carrierobj.Content_Map.Clear();
        //                    for (int j = 0; j < e.Inputs.DataItem[0][4][i][1].Count; j++)
        //                    {
        //                        carrierobj.Content_Map.Add(new ContentMap(e.Inputs.DataItem[0][4][i][1][j][0].ToString(),
        //                            e.Inputs.DataItem[0][4][i][1][j][1].ToString()));
        //                    }
        //                    break;
        //                case "LocationID":
        //                    carrierobj.Location_ID = e.Inputs.DataItem[0][4][i][1].ToString();
        //                    break;
        //                case "SlotMap":
        //                    carrierobj.SlotMap = new List<SlotSubstrateStatus>();
        //                    carrierobj.SlotMap.Clear();
        //                    for (int k = 0; k < e.Inputs.DataItem[0][4][i][1].Count; k++)
        //                    {
        //                        carrierobj.SlotMap.Add((SlotSubstrateStatus)e.Inputs.DataItem[0][4][i][1][k].ToUInt());
        //                    }
        //                    break;
        //                case "SubstrateCount":
        //                    carrierobj.Substrate_Count = e.Inputs.DataItem[0][4][i][1].ToUInt();
        //                    break;
        //                case "Usage":
        //                    carrierobj.Usage = e.Inputs.DataItem[0][4][i][1].ToString();
        //                    break;
        //            }
        //        }
        //        ret = true;

        //    }
        //    catch { return ret; }
        //    return ret;
        //}
        //private bool ParseProcssJobObj(SECsPrimaryInEventArgs e, out E40ProcessJob PJobj)
        //{
        //    bool ret = false;

        //    PJobj = new E40ProcessJob();

        //    try
        //    {
        //        PJobj.ObjtID = e.Inputs.DataItem[0][1].ToString().Trim();
        //        PJobj.PRMtlType = e.Inputs.DataItem[0][2].ToString() == "D" ? MtlType.CARRIER : MtlType.WAFER;   //13（D）= Carrier,E=Wafer
        //        PJobj.ObjType = "ProcessJob";
        //        PJobj.PauseEvent = new List<int>();
        //        PJobj.PRMtlNameList = new List<ProcessMaterialName>();
        //        for (int i = 0; i < e.Inputs.DataItem[0][3].Count; i++)
        //        {
        //            ProcessMaterialName promname = new ProcessMaterialName();
        //            promname.CarrierID = e.Inputs.DataItem[0][3][i][0].ToString();
        //            promname.SlotID = new List<int>();
        //            for (int j = 0; j < e.Inputs.DataItem[0][3][i][1].Count; j++) promname.SlotID.Add(e.Inputs.DataItem[0][3][i][1][j].ToInt());
        //            PJobj.PRMtlNameList.Add(promname);
        //        }
        //        PJobj.PRRecipeMethod = e.Inputs.DataItem[0][4][0].ToUInt() == 2 ? ProcessRecipeMethod.RECIPE_WITH_VAR_TUNING : ProcessRecipeMethod.RECIEP;
        //        PJobj.RecID = e.Inputs.DataItem[0][4][1].ToString();
        //        PJobj.RecVariableList = new Dictionary<string, object>();
        //        if (e.Inputs.DataItem[0][4].Count > 2 && e.Inputs.DataItem[0][4][2].Count >= 1)
        //        {
        //            for (int i = 0; i < e.Inputs.DataItem[0][4][2].Count; i++)
        //            {
        //                if (e.Inputs.DataItem[0][4][2][i].Count <= 1)
        //                    return false;
        //                if (!PJobj.RecVariableList.ContainsKey(e.Inputs.DataItem[0][4][2][i][0].ToString()))
        //                    PJobj.RecVariableList.Add(e.Inputs.DataItem[0][4][2][i][0].ToString(), e.Inputs.DataItem[0][4][2][i][1]);
        //            }
        //        }




        //        PJobj.PRProcessStart = e.Inputs.DataItem[0][5].ToBoolean();

        //        if (e.Inputs.DataItem[0][6].Count > 0)
        //        {
        //            for (int i = 0; i < e.Inputs.DataItem[0][6].Count; i++)
        //            {
        //                PJobj.PauseEvent.Add(e.Inputs.DataItem[0][6][i].ToInt32());
        //            }
        //        }



        //        ret = true;

        //    }
        //    catch { return ret; }
        //    return ret;
        //}
        //private bool ParseMultiProcssJobObj(SECsPrimaryInEventArgs e, out E40ProcessJob[] PJobjs)
        //{
        //    bool ret = true;
            //try
            //{
            //    PJobjs = new E40ProcessJob[e.Inputs.DataItem[0][1].Count];

            //    for (int pjIndex = 0; pjIndex < e.Inputs.DataItem[0][1].Count; pjIndex++)
            //    {
            //        var PJobj = new E40ProcessJob();


            //        PJobj.ObjtID = e.Inputs.DataItem[0][1][pjIndex][0].ToString().Trim();
            //        PJobj.PRMtlType = e.Inputs.DataItem[0][1][pjIndex][1].ToString() == "D" ? MtlType.CARRIER : MtlType.WAFER;   //13（D）= Carrier,E=Wafer
            //        PJobj.ObjType = "ProcessJob";
            //        PJobj.PauseEvent = new List<int>();
            //        PJobj.PRMtlNameList = new List<ProcessMaterialName>();
            //        for (int i = 0; i < e.Inputs.DataItem[0][1][pjIndex][2].Count; i++)
            //        {
            //            ProcessMaterialName promname = new ProcessMaterialName();
            //            promname.CarrierID = e.Inputs.DataItem[0][1][pjIndex][2][i][0].ToString();
            //            promname.SlotID = new List<int>();
            //            for (int j = 0; j < e.Inputs.DataItem[0][1][pjIndex][2][i][1].Count; j++) promname.SlotID.Add(e.Inputs.DataItem[0][1][pjIndex][2][i][1][j].ToInt());
            //            PJobj.PRMtlNameList.Add(promname);
            //        }
            //        PJobj.PRRecipeMethod = e.Inputs.DataItem[0][1][pjIndex][3][0].ToUInt() == 2 ? ProcessRecipeMethod.RECIPE_WITH_VAR_TUNING : ProcessRecipeMethod.RECIEP;
            //        PJobj.RecID = e.Inputs.DataItem[0][1][pjIndex][3][1].ToString().Split('\\').Last().Replace(".seq", "");
            //        PJobj.RecVariableList = new Dictionary<string, object>();
            //        for (int i = 0; i < e.Inputs.DataItem[0][1][pjIndex][3][2].Count; i++)
            //        {
            //            if (e.Inputs.DataItem[0][1][pjIndex][3][2][i].Count <= 1)
            //            {
            //                return false;
            //            }

            //            if (!PJobj.RecVariableList.ContainsKey(e.Inputs.DataItem[0][1][pjIndex][3][2][i][0].ToString()))
            //                PJobj.RecVariableList.Add(e.Inputs.DataItem[0][1][pjIndex][3][2][i][0].ToString(), e.Inputs.DataItem[0][1][pjIndex][3][2][i][1]);

            //        }
            //        PJobj.PRProcessStart = e.Inputs.DataItem[0][1][pjIndex][4].ToBoolean();
            //        PJobj.CreateTime = DateTime.Now;

            //        PJobjs[pjIndex] = PJobj;
            //        if (PJobj.RecID == "*null*") PJobj.RecID = "";
            //        string[] recipelist = GetRecipeList();
            //        if (!GetRecipeList().Contains(PJobj.RecID) || string.IsNullOrEmpty(PJobj.RecID))
            //            ret = false;
            //    }

            //}
            //catch
            //{
            //    PJobjs = new E40ProcessJob[0];
            //    return false;
            //}

        //    return ret;
        //}
        //private bool ParseControlJobObj(SECsPrimaryInEventArgs e, out E94ControlJob CJobj)
        //{
        //    bool ret = true;

        //    CJobj = new E94ControlJob();
        //    CJobj.CreateTime = DateTime.Now;

        //    try
        //    {
        //        if (e.Inputs.DataItem[0][1].ToString() != "ControlJob") return false;
        //        CJobj.ObjType = e.Inputs.DataItem[0][1].ToString();

        //        for (int nIndex = 0; nIndex < e.Inputs.DataItem[0][2].Count; nIndex++)
        //        {
        //            switch (e.Inputs.DataItem[0][2][nIndex][0].ToString())
        //            {
        //                case "ObjID":
        //                    CJobj.ObjtID = e.Inputs.DataItem[0][2][nIndex][1].ToString().Trim();
        //                    break;

        //                case "CarrierInputSpec":
        //                    CJobj.CarrierIputSpec = new List<string>();
        //                    for (int i = 0; i < e.Inputs.DataItem[0][2][nIndex][1].Count; i++)
        //                        CJobj.CarrierIputSpec.Add(e.Inputs.DataItem[0][2][nIndex][1][i].ToString());
        //                    break;
        //                case "MtrlOutSpec":
        //                    CJobj.MtrlOutSpec = new List<MtrlOutSpecPair>();
        //                    for (int i = 0; i < e.Inputs.DataItem[0][2][nIndex][1].Count; i++)
        //                    {
        //                        MtrlOutSpecPair moutpair = new MtrlOutSpecPair();
        //                        moutpair.sourceCarID = e.Inputs.DataItem[0][2][nIndex][1][i][0][0].ToString();
        //                        moutpair.DestCarID = e.Inputs.DataItem[0][2][nIndex][1][i][1][0].ToString();
        //                        moutpair.sourceSlots = new List<int>();
        //                        moutpair.destSlots = new List<int>();
        //                        for (int j = 0; j < e.Inputs.DataItem[0][2][nIndex][1][i][0][1].Count; j++)
        //                            moutpair.sourceSlots.Add(e.Inputs.DataItem[0][2][nIndex][1][i][0][1][j].ToInt());

        //                        for (int j = 0; j < e.Inputs.DataItem[0][2][nIndex][1][i][1][1].Count; j++)
        //                            moutpair.destSlots.Add(e.Inputs.DataItem[0][2][nIndex][1][i][1][1][j].ToInt());

        //                        CJobj.MtrlOutSpec.Add(moutpair);
        //                    }
        //                    break;
        //                case "ProcessOrderMgmt":
        //                    CJobj.ProcessingOrderMgmt = (ProcessOrderManagement)e.Inputs.DataItem[0][2][nIndex][1].ToInt();
        //                    break;
        //                case "StartMethod":
        //                    CJobj.StartMethod = e.Inputs.DataItem[0][2][nIndex][1].ToBoolean();
        //                    break;
        //                case "ProcessingCtrlSpec":
        //                    CJobj.ProcessingCtrlSpec = new List<string>();
        //                    for (int i = 0; i < e.Inputs.DataItem[0][2][nIndex][1].Count; i++)
        //                    {
        //                        CJobj.ProcessingCtrlSpec.Add(e.Inputs.DataItem[0][2][nIndex][1][i][0].ToString());
        //                    }
        //                    break;
        //                case "PauseEvent":

        //                    CJobj.PauseEvent = new List<int>();
        //                    for (int i = 0; i < e.Inputs.DataItem[0][2][nIndex][1].Count; i++)
        //                    {
        //                        CJobj.PauseEvent.Add(e.Inputs.DataItem[0][2][nIndex][1][i].ToInt32());
        //                    }
        //                    break;



        //                default:
        //                    ret = false;
        //                    break;
        //            }
        //        }
        //    }
        //    catch { ret = false; return ret; }
        //    return ret;
        //}
        private void Handle_S7F25FormattedPPRequest(SECsPrimaryInEventArgs e)
        {
            //try
            //{
            //    string reason = string.Empty;
            //    LOG.Write("Handle_S7F25, Request the transmission of the process program body");
            //    // send reply
            //    // S7F6 format
            //    // L,n (number of process programs in the directory)
            //    //      1. <PPID1>
            //    //      .
            //    //      .
            //    //      n. <PPIDn>
            //    string recipename = "";
            //    int moduleId = 0;
            //    if (e.Inputs.DataItem[0].Format == SECsFormat.Ascii)
            //    {
            //        recipename = e.Inputs.DataItem[0].ToString();
            //    }

            //    if (e.Inputs.DataItem[0].Format == SECsFormat.List)
            //    {
            //        recipename = e.Inputs.DataItem[0][0].ToString();
            //        if (e.Inputs.DataItem[0].Count > 1)
            //            moduleId = e.Inputs.DataItem[0][1].ToInt();
            //    }





            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(7, 26, false);



            //    bool ret = _equipment.GetFormatedRecipeBody(recipename, moduleId, out List<Tuple<uint, object[]>> paras);
            //    reply.DataItem.AddList();
            //    reply.DataItem[0].Add("PPID", recipename, SECsFormat.Ascii);
            //    reply.DataItem[0].Add("MDLN", _gem.Model, SECsFormat.Ascii);
            //    reply.DataItem[0].Add("SOFTREV", _gem.SoftwareRevision, SECsFormat.Ascii);

            //    SECsDataItem item1 = new SECsDataItem(SECsFormat.List);
            //    foreach (var recipePara in paras)
            //    {
            //        SECsDataItem item2 = new SECsDataItem(SECsFormat.List);
            //        item2.Add("CCode", recipePara.Item1, SECsFormat.U2);
            //        SECsDataItem item3 = new SECsDataItem(SECsFormat.List);
            //        foreach (var para in recipePara.Item2)
            //        {
            //            if (para.GetType() == typeof(bool))
            //            {
            //                item3.Add("Para", (Boolean)para, SECsFormat.Boolean);
            //            }
            //            else if (para.GetType() == typeof(int))
            //            {
            //                item3.Add("Para", (int)para, SECsFormat.I4);
            //            }
            //            else if (para.GetType() == typeof(Int16))
            //            {
            //                item3.Add("Para", (int)para, SECsFormat.I4);
            //            }
            //            else if (para.GetType() == typeof(uint))
            //            {
            //                item3.Add("Para", (uint)para, SECsFormat.U4);
            //            }
            //            else if (para.GetType() == typeof(UInt16))
            //            {
            //                item3.Add("Para", (uint)para, SECsFormat.U4);
            //            }
            //            else
            //            {
            //                item3.Add("Para", para.ToString(), SECsFormat.Ascii);
            //            }






            //        }
            //        item2.Add(item3);
            //        item1.Add(item2);
            //    }
            //    reply.DataItem[0].Add(item1);

            //    if (e.Inputs.NeedReply)
            //        _gem.SendReply(reply, e.TransactionID);


            //    //_gem.SendReply(reply, e.TransactionID);
            //}
            //catch (Exception ex)
            //{
            //    LOG.Write("Handle_S7F5 exception:" + ex.Message);
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(7, 0, false);
            //    _gem.SendReply(reply, e.TransactionID);
            //}
        }
        private void Handle_S7F23(SECsPrimaryInEventArgs e)
        {
            //try
            //{
            //    string reason = string.Empty;
            //    LOG.Write("Handle_S7F23, Process Program Send");
            //    // send reply
            //    // S7F6 format
            //    // L,n (number of process programs in the directory)
            //    //      1. <PPID1>
            //    //      .
            //    //      .
            //    //      n. <PPIDn>
            //    string recipename = e.Inputs.DataItem[0][0].ToString();

            //    string recipeBody = e.Inputs.DataItem[0][1].ToString();


            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(7, 24, false);

            //    bool resutl = _equipment.DownloadRecipe(recipename, recipeBody);
            //    reply.DataItem.Add("PPACKC7ID", resutl ? 0 : 1, SECsFormat.Binary);

            //    _gem.SendReply(reply, e.TransactionID);


            //    //_gem.SendReply(reply, e.TransactionID);
            //}
            //catch (Exception ex)
            //{
            //    LOG.Write("Handle_S7F5 exception:" + ex.Message);
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(7, 0, false);
            //    _gem.SendReply(reply, e.TransactionID);
            //}
        }
        private void Handle_S7F17(SECsPrimaryInEventArgs e)
        {
            //try
            //{
            //    string reason = string.Empty;
            //    LOG.Write("Handle_S7F17, Request the transmission of the Delete process program body");
            //    // send reply
            //    // S7F6 format
            //    // L,n (number of process programs in the directory)
            //    //      1. <PPID1>
            //    //      .
            //    //      .
            //    //      n. <PPIDn>
            //    List<string> recipeNames = new List<string>();

            //    for (int i = 0; i < e.Inputs.DataItem[0].Count; i++)
            //    {
            //        recipeNames.Add(e.Inputs.DataItem[0][i].ToString());
            //    }

            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(7, 18, false);
            //    if (DeleteRecipes(recipeNames.ToArray()))
            //    {
            //        reply.DataItem.Add("ACKC7", 0, SECsFormat.Binary);

            //    }
            //    else
            //    {
            //        reply.DataItem.Add("ACKC7", 1, SECsFormat.Binary);
            //    }
            //    _gem.SendReply(reply, e.TransactionID);


            //    //_gem.SendReply(reply, e.TransactionID);
            //}
            //catch (Exception ex)
            //{
            //    LOG.Write("Handle_S7F17 exception:" + ex.Message);
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(7, 0, false);
            //    _gem.SendReply(reply, e.TransactionID);
            //}
        }

        //private bool DeleteRecipes(string[] recs)
        //{
        //    //return _equipment.DeleteRecipes(recs);
        //}

        private void Handle_S7F5(SECsPrimaryInEventArgs e)
        {
            //try
            //{
            //    string reason = string.Empty;
            //    LOG.Write("Handle_S7F5, Request the transmission of the process program body");
            //    // send reply
            //    // S7F6 format
            //    // L,n (number of process programs in the directory)
            //    //      1. <PPID1>
            //    //      .
            //    //      .
            //    //      n. <PPIDn>
            //    string recipename = e.Inputs.DataItem[0].ToString();
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(7, 6, false);
            //    if (!GetRecipeList().Contains(recipename))
            //    {
            //        LOG.Write("Handle_S7F5 exception:" + "Recipe doesn't exist!");
            //        reply = _gem.Services.CustomMessage.CreateMessage(7, 0, false);
            //        _gem.SendReply(reply, e.TransactionID);
            //        return;
            //    }
            //    string recipebody = GetRecipeBody(recipename);
            //    reply.DataItem.AddList();
            //    reply.DataItem[0].Add("PPID", recipename, SECsFormat.Ascii);
            //    reply.DataItem[0].Add("PPBODY", recipebody, SECsFormat.Ascii);
            //    _gem.SendReply(reply, e.TransactionID);


            //    //_gem.SendReply(reply, e.TransactionID);
            //}
            //catch (Exception ex)
            //{
            //    LOG.Write("Handle_S7F5 exception:" + ex.Message);
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(7, 0, false);
            //    _gem.SendReply(reply, e.TransactionID);
            //}
        }
        //private string GetRecipeBody(string recipename)
        //{
        //    return _equipment.GetRecipeBody(recipename);
        //}

        /// <summary>
        /// Host requests the equipment to send the current EPPD
        /// S7F19 Format
        /// Head only
        /// </summary>
        /// <param name="e"></param>
        private void Handle_S7F19(SECsPrimaryInEventArgs e)
        {
            //try
            //{
            //    string reason = string.Empty;
            //    LOG.Write("Handle_S7F19, Request the transmission of the current equipment process program directory");
            //    // send reply
            //    // S7F20 format
            //    // L,n (number of process programs in the directory)
            //    //      1. <PPID1>
            //    //      .
            //    //      .
            //    //      n. <PPIDn>
            //    var recipeList = GetRecipeList();
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(7, 20, false);
            //    reply.DataItem.AddList();
            //    for (int rNum = 0; rNum < recipeList.Length; rNum++)
            //    {
            //        reply.DataItem[0].Add("PPID", recipeList[rNum], SECsFormat.Ascii);
            //    }
            //    _gem.SendReply(reply, e.TransactionID);
            //}
            //catch (Exception ex)
            //{
            //    LOG.Write("Handle_S7F19 exception:" + ex.Message);
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(7, 0, false);
            //    _gem.SendReply(reply, e.TransactionID);
            //}
        }

        //private string[] GetRecipeList()
        //{
        //    return _equipment.GetRecipeList();
        //}

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
            //try
            //{
            //    //get ec mapping
            //    Dictionary<long, string> ecMapping = new Dictionary<long, string>();
            //    foreach (ECID ec in _gem.EquipmentModel.EquipmentConstants.ECIDCollection)
            //    {
            //        ecMapping.Add(ec.id, ec.logicalName);
            //    }
            //    //send acknowledge
            //    //S2F14 Format
            //    //L,n
            //    //  1. <ECV1>
            //    //  2. <ECV2>
            //    //  .
            //    //  .
            //    //  n. <ECVn>
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(2, 14, false);
            //    reply.DataItem.AddList();
            //    if (e.Inputs.DataItem[0].Count != 0)
            //    {
            //        for (int i = 0; i < e.Inputs.DataItem[0].Count; i++)
            //        {
            //            long ecId = e.Inputs.DataItem[0][i].ToInt64();
            //            if (ecMapping.ContainsKey(ecId))
            //            {
            //                string ecName = ecMapping[ecId];
            //                if (_systemBuildInEc.Contains(ecName))
            //                {
            //                    string ecValue = _gem.GetAttribute(ecName, AttributeType.EC);
            //                    LOG.Write(string.Format("Host requests ECID:{0}, ECName:{1}, Value:{2}", ecId, ecName, ecValue));
            //                    reply.DataItem[0].Add("ECV", ecValue);
            //                }
            //                else
            //                {
            //                    var ecValue = GetECValue(ecName);
            //                    _gem.SetAttribute(ecName, AttributeType.EC, ecValue.ToString());
            //                    LOG.Write(string.Format("Host requests ECID:{0}, ECName:{1}, Value:{2}", ecId, ecName, ecValue));

            //                    SECsFormat format = ConvertObjFormat(ecValue);
            //                    reply.DataItem[0].Add("ECV", ecValue, format);
            //                }
            //            }
            //            else
            //            {
            //                LOG.Write(string.Format("Host requests ECID:{0}, Value:{1}", ecId, ""));
            //                reply.DataItem[0].Add("ECV", "");
            //            }
            //        }
            //    }
            //    else
            //    {
            //        foreach (ECID ec in _gem.EquipmentModel.EquipmentConstants.ECIDCollection)
            //        {

            //            if (_systemBuildInEc.Contains(ec.logicalName))
            //            {
            //                string ecValue = _gem.GetAttribute(ec.logicalName, AttributeType.EC);
            //                LOG.Write(string.Format("Host requests ECID:{0}, ECName:{1}, Value:{2}", ec.id, ec.logicalName, ecValue));
            //                reply.DataItem[0].Add("ECV", ecValue);
            //            }
            //            else
            //            {
            //                var ecValue = GetECValue(ec.logicalName);
            //                _gem.SetAttribute(ec.logicalName, AttributeType.EC, ecValue.ToString());
            //                LOG.Write(string.Format("Host requests ECID:{0}, ECName:{1}, Value:{2}", ec.id, ec.logicalName, ecValue));
            //                SECsFormat format = ConvertObjFormat(ecValue);
            //                reply.DataItem[0].Add("ECV", ecValue, format);
            //            }
            //        }
            //    }
            //    _gem.SendReply(reply, e.TransactionID);
            //}
            //catch (Exception ex)
            //{
            //    LOG.Write(string.Format("Handle_S2F13 Exception: {0}", ex.Message));
            //    SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(2, 0, false);
            //    _gem.SendReply(reply, e.TransactionID);
            //}
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



        //protected object GetECValue(string ecName)
        //{
        //    SCConfigItem sc;
        //    if (!SC.ContainsItem(ecName)) return null;
        //    sc = SC.GetConfigItem(ecName);
        //    return sc.Value;
        //}

        //protected void SetECValue(string ecName, string ecValue)
        //{
        //    if (SC.ContainsItem(ecName))
        //        SC.SetItemValueFromString(ecName, ecValue);
        //}

        /// <summary>
        /// Send terminal message to host
        /// </summary>
        /// <param name="message"></param>
        //public void SendTerminalMessageToHost(string message)
        //{
        //    if (!_gem.IsConnected || _gem.CommunicationState == CommunicationState.Disabled
        //        || _gem.CommunicationState == CommunicationState.WaitDelay
        //                          || _gem.CommunicationState == CommunicationState.EnabledNotCommunicating)
        //    {
        //        EV.PostWarningLog("FA", "Host not connected, send terminal message failed.");
        //        return;
        //    }
        //    LOG.Write("Send terminal message to host:" + message);
        //    Task.Factory.StartNew(() =>
        //    {
        //        SECsMessage secsMsg = _gem.Services.TerminalDisplay.TerminalRequest(message);
        //        _gem.Send(secsMsg);
        //    });
        //}
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

        //public virtual void Invoke(string method, params object[] args)
        //{
        //    switch (method)
        //    {
        //        case FACommandName.FAEnable:
        //            _gem.SetEnable();
        //            break;
        //        case FACommandName.FADisable:
        //            _gem.SetDisable();
        //            break;
        //        case FACommandName.FAOnline:
        //            if (_gem.CommunicationState != CommunicationState.EnabledCommunicating)
        //                break;
        //            _gem.SetOnline();
        //            break;
        //        case FACommandName.FAOffline:
        //            if (_gem.CommunicationState != CommunicationState.EnabledCommunicating)
        //                break;
        //            _gem.SetOffline();
        //            break;
        //        case FACommandName.FALocal:
        //            if (_gem.CommunicationState != CommunicationState.EnabledCommunicating)
        //                break;
        //            _gem.SetLocal();
        //            break;
        //        case FACommandName.FARemote:
        //            if (_gem.CommunicationState != CommunicationState.EnabledCommunicating)
        //                break;
        //            _gem.SetRemote();
        //            break;
        //        case FACommandName.FAEnableSpooling:
        //            SetEnableSpooling();
        //            break;
        //        case FACommandName.FADisableSpooling:
        //            SetDisableSpooling();
        //            break;
        //        case FACommandName.FASendTerminalMessage:
        //            if (args != null && args.Length == 1)
        //                SendTerminalMessageToHost(args[0].ToString());
        //            else
        //            {
        //                LOG.Write("FASendTerminalMessage should has 1 message parameter");
        //            }
        //            break;
        //    }
        //}

        //private bool _gem_RemoteCommandS2F41In(SECsTransaction trans)
        //{
            //lock (_locker)
            //{
            //    try
            //    {

            //        bool ret = true;
            //        string reason = string.Empty;
            //        SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(2, 42, false);
            //        string remoteCommandName = trans.Primary.DataItem[0][0].ToString().ToUpper();
            //        var paramters = new object[] { };
            //        int _port;
            //        switch (remoteCommandName)
            //        {
            //            case "PP_DOWNLOAD":
            //                if (trans.Primary.DataItem[0][1][0][0].ToString() != "RECIPE_NAME")
            //                {
            //                    reason = "PP_DOWNLOAD Command Format Not Correct";
            //                    ret = false;
            //                    break;
            //                }
            //                if (trans.Primary.DataItem[0][1][1][0].ToString() != "RECIPE_BODY")
            //                {
            //                    reason = "PP_DOWNLOAD Command Format Not Correct";
            //                    ret = false;
            //                    break;
            //                }
            //                string ppid = trans.Primary.DataItem[0][1][0][1].ToString();
            //                string ppbody = trans.Primary.DataItem[0][1][1][1].ToString();
            //                paramters = new object[] { ppid, ppbody };
            //                ret = _equipment.ExecuteFACommand(remoteCommandName, paramters, out reason);
            //                break;



            //            case "START":

            //                List<object> paras = new List<object>();
            //                for (int i = 0; i < trans.Primary.DataItem[0][1].Count; i++)
            //                {
            //                    paras.Add(trans.Primary.DataItem[0][1][i][1]);
            //                }

            //                ret = _equipment.ExecuteFACommand(remoteCommandName, paras.ToArray(), out reason);
            //                break;
            //            case "STARTCHECK":
            //                if (trans.Primary.DataItem[0][1][0][0].ToString().ToUpper() != "PORT_ID")
            //                {
            //                    reason = "Start Command Format Not Correct";
            //                    ret = false;
            //                    break;
            //                }
            //                if (trans.Primary.DataItem[0][1][1][0].ToString().ToUpper() != "SLOT")
            //                {
            //                    reason = "Start Command Format Not Correct";
            //                    ret = false;
            //                    break;
            //                }
            //                _port = trans.Primary.DataItem[0][1][0][1].ToInt();

            //                List<int> slots = new List<int>();
            //                for (int i = 0; i < trans.Primary.DataItem[0][1][1][1].Count; i++)
            //                {
            //                    int slot = trans.Primary.DataItem[0][1][1][1][i].ToInt();
            //                    slots.Add(slot);
            //                }


            //                paramters = new object[] { _port, slots };
            //                ret = _equipment.ExecuteFACommand(remoteCommandName, paramters, out reason);
            //                break;




            //            case "GOREMOTE":
            //                {
            //                    _gem.SetRemote();
            //                }
            //                break;
            //            case "GOLOCAL":
            //                {
            //                    _gem.SetLocal();
            //                }
            //                break;
            //            case "LOTCOMPLETE":
            //                if (trans.Primary.DataItem[0][1][0][0].ToString() != "LOT_ID")
            //                {
            //                    reason = "LotComplete Command Format Not Correct";
            //                    ret = false;
            //                }
            //                string lotId = trans.Primary.DataItem[0][1][0][1].ToString();
            //                paramters = new object[] { lotId };
            //                ret = _equipment.ExecuteFACommand(remoteCommandName, paramters, out reason); ;
            //                break;
            //            case "SETCARRIERINDEX":
            //                if (trans.Primary.DataItem[0][1][0][0].ToString() != "PORT_ID")
            //                {
            //                    reason = "SETCARRIERINDEX Command Format Not Correct";
            //                    ret = false;
            //                }
            //                _port = trans.Primary.DataItem[0][1][0][1].ToInt();
            //                if (trans.Primary.DataItem[0][1][1][0].ToString() != "INDEX")
            //                {
            //                    reason = "SETCARRIERINDEX Command Format Not Correct";
            //                    ret = false;
            //                }
            //                int index = trans.Primary.DataItem[0][1][1][1].ToInt();
            //                paramters = new object[] { _port, index };
            //                ret = _equipment.ExecuteFACommand(remoteCommandName, paramters, out reason); ;
            //                break;
            //            case "SETCARRIERNAME":
            //                if (trans.Primary.DataItem[0][1][0][0].ToString() != "PORT_ID")
            //                {
            //                    reason = "SETCARRIERINDEX Command Format Not Correct";
            //                    ret = false;
            //                }
            //                _port = trans.Primary.DataItem[0][1][0][1].ToInt();
            //                if (trans.Primary.DataItem[0][1][1][0].ToString() != "NAME")
            //                {
            //                    reason = "SETCARRIERINDEX Command Format Not Correct";
            //                    ret = false;
            //                }
            //                string carrierName = trans.Primary.DataItem[0][1][1][1].ToString();
            //                paramters = new object[] { _port, carrierName };
            //                ret = _equipment.ExecuteFACommand(remoteCommandName, paramters, out reason); ;
            //                break;

            //            case "UNCLAMP":
            //            case "READCARRIERID":
            //                if (trans.Primary.DataItem[0][1][0][0].ToString() != "PORT_ID")
            //                {
            //                    reason = "Read CarrierID Command Format Not Correct";
            //                    ret = false;
            //                    break;
            //                }
            //                if (trans.Primary.DataItem[0][1][1][0].ToString() != "INDEX")
            //                {
            //                    reason = "Read CarrierID Command Format Not Correct";
            //                    ret = false;
            //                    break;
            //                }
            //                if (trans.Primary.DataItem[0][1][2][0].ToString() != "OFFSET")
            //                {
            //                    reason = "Read CarrierID Command Format Not Correct";
            //                    ret = false;
            //                    break;
            //                }
            //                if (trans.Primary.DataItem[0][1][3][0].ToString() != "LENGTH")
            //                {
            //                    reason = "Read CarrierID Command Format Not Correct";
            //                    ret = false;
            //                    break;
            //                }

            //                _port = trans.Primary.DataItem[0][1][0][1].ToInt();
            //                paramters = new object[]
            //                {
            //                    _port,
            //                    trans.Primary.DataItem[0][1][1][1].ToInt(),
            //                    trans.Primary.DataItem[0][1][2][1].ToInt(),
            //                    trans.Primary.DataItem[0][1][3][1].ToInt(),
            //                };
            //                ret = _equipment.ExecuteFACommand(remoteCommandName, paramters, out reason);
            //                break;
            //            case "WRITECARRIERID":
            //                if (trans.Primary.DataItem[0][1][0][0].ToString() != "PORT_ID")
            //                {
            //                    reason = "Write CarrierID Command Format Not Correct";
            //                    ret = false;
            //                    break;
            //                }
            //                if (trans.Primary.DataItem[0][1][1][0].ToString() != "INDEX")
            //                {
            //                    reason = "Write CarrierID Command Format Not Correct";
            //                    ret = false;
            //                    break;
            //                }
            //                if (trans.Primary.DataItem[0][1][2][0].ToString() != "OFFSET")
            //                {
            //                    reason = "Write CarrierID Command Format Not Correct";
            //                    ret = false;
            //                    break;
            //                }
            //                if (trans.Primary.DataItem[0][1][3][0].ToString() != "LENGTH")
            //                {
            //                    reason = "Write CarrierID Command Format Not Correct";
            //                    ret = false;
            //                    break;
            //                }
            //                if (trans.Primary.DataItem[0][1][4][0].ToString() != "ID")
            //                {
            //                    reason = "Write CarrierID Command Format Not Correct";
            //                    ret = false;
            //                    break;
            //                }

            //                _port = trans.Primary.DataItem[0][1][0][1].ToInt();
            //                int idindex = trans.Primary.DataItem[0][1][1][1].ToInt();
            //                int idoffset = trans.Primary.DataItem[0][1][2][1].ToInt();
            //                int idlength = trans.Primary.DataItem[0][1][3][1].ToInt();
            //                string id = trans.Primary.DataItem[0][1][4][1].ToString();


            //                paramters = new object[]
            //                {
            //                    _port,
            //                    idindex,
            //                    idoffset,
            //                    idlength,
            //                    id,
            //                };
            //                ret = _equipment.ExecuteFACommand(remoteCommandName, paramters, out reason);
            //                break;
            //            case "READRFID":
            //            case "WRITERFID":
            //            case "LOAD":
            //            case "UNLOAD":
            //            case "SCANSLOT":
            //                if (trans.Primary.DataItem[0][1][0][0].ToString() != "PORT_ID")
            //                {
            //                    reason = "ScanSlot Command Format Not Correct";
            //                    ret = false;
            //                    break;
            //                }
            //                _port = trans.Primary.DataItem[0][1][0][1].ToInt();
            //                paramters = new object[] { _port };
            //                ret = _equipment.ExecuteFACommand(remoteCommandName, paramters, out reason);
            //                break;

            //            case "PP_SELECT":
            //                if (trans.Primary.DataItem[0][1][0][0].ToString() != "PORT_ID")
            //                {
            //                    reason = "PP_SELECT Command Format Not Correct";
            //                    ret = false;
            //                    break;
            //                }
            //                if (trans.Primary.DataItem[0][1][1][0].ToString() != "RECIPE_NAME")
            //                {
            //                    reason = "PP_SELECT Command Format Not Correct";
            //                    ret = false;
            //                    break;
            //                }
            //                _port = trans.Primary.DataItem[0][1][0][1].ToInt();
            //                string recipe = trans.Primary.DataItem[0][1][1][1].ToString();
            //                int start = trans.Primary.DataItem[0][1].Count > 2 ?
            //                    trans.Primary.DataItem[0][1][2][1].ToInt() : 1;
            //                int end = trans.Primary.DataItem[0][1].Count > 3 ?
            //                    trans.Primary.DataItem[0][1][3][1].ToInt() : 25;
            //                paramters = new object[] { _port, recipe, start, end };
            //                ret = _equipment.ExecuteFACommand(remoteCommandName, paramters, out reason);
            //                break;
            //            case "CREATE_WAFER":
            //                _port = trans.Primary.DataItem[0][1][0][1].ToInt();
            //                int startslot = trans.Primary.DataItem[0][1].Count > 1 ?
            //                    trans.Primary.DataItem[0][1][1][1].ToInt() : 1;
            //                int endslot = trans.Primary.DataItem[0][1].Count > 2 ?
            //                    trans.Primary.DataItem[0][1][2][1].ToInt() : 25;
            //                paramters = new object[] { _port, startslot, endslot };
            //                ret = _equipment.ExecuteFACommand(remoteCommandName, paramters, out reason);
            //                break;
            //            case "PP-SELECT":
            //                if (trans.Primary.DataItem[0][1][0][0].ToString() != "RECIPE_NAME" ||
            //                        trans.Primary.DataItem[0][1][0][1].ToString() == "")
            //                    throw new Exception("PP-SELECT Command Format Not Correct");
            //                string recipeId = trans.Primary.DataItem[0][1][0][1].ToString();
            //                ret = _equipment.ExecuteFACommand("PP-SELECT", new object[] { recipeId }, out _);

            //                break;
            //            case "PAUSE":
            //            case "ABORT":
            //            case "RESUME":

            //                ret = _equipment.ExecuteFACommand(remoteCommandName, null, out reason); ;
            //                break;
            //            default:
            //                Dictionary<string, SECsDataItem> parass = new Dictionary<string, SECsDataItem>();
            //                for (int i = 0; i < trans.Primary.DataItem[0][1].Count; i++)
            //                {
            //                    parass.Add(trans.Primary.DataItem[0][1][i][0].ToString(), trans.Primary.DataItem[0][1][i][1]);
            //                }
            //                ret = _equipment.ExecuteHostCommand(remoteCommandName, parass, out reason);
            //                break;
            //        }

            //        reply.DataItem.AddList();
            //        if (ret)
            //        {
            //            reply.DataItem[0].Add("HCACK", 0, SECsFormat.Binary);
            //        }
            //        else
            //        {
            //            reply.DataItem[0].Add("HCACK", 2/*Can not perform now*/, SECsFormat.Binary);

            //            reply.DataItem[0].Add("CPVALUE", reason, SECsFormat.Ascii);
            //        }

            //        _gem.SendReply(reply, trans.Id);
            //    }
            //    catch (Exception ex)
            //    {
            //        LOG.Write(string.Format("Handle_S2F41 Exception: {0}", ex.Message));
            //        SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(2, 42, false);
            //        reply.DataItem.AddList();
            //        reply.DataItem[0].Add("HCACK", 3/*At least one parameter is invalid*/, SECsFormat.Binary);
            //        reply.DataItem[0].AddList();
            //        reply.DataItem[0][1].AddList();
            //        reply.DataItem[0][1].Add("CPVALUE", ex.Message, SECsFormat.Ascii);
            //        _gem.SendReply(reply, trans.Id);
            //    }

            //    Thread.Sleep(200);
            //    return true;
            //}
        //}

        //private bool _gem_RemoteCommandS2F49In(SECsTransaction trans)
        //{
            //lock (_locker)
            //{
            //    try
            //    {
            //        bool ret = true;
            //        string reason = string.Empty;
            //        SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(2, 50, false);

            //        string remoteCommandName = trans.Primary.DataItem[0][2].ToString();
            //        switch (remoteCommandName.ToUpper())
            //        {
            //            case "START_TRANSFER":
            //            case "START_PACK":
            //                Dictionary<string, SECsDataItem> paras = new Dictionary<string, SECsDataItem>();
            //                for (int i = 0; i < trans.Primary.DataItem[0][3].Count; i++)
            //                {
            //                    paras.Add(trans.Primary.DataItem[0][3][i][0].ToString(), trans.Primary.DataItem[0][3][i][1]);
            //                }

            //                ret = _equipment.ExecuteHostCommand(remoteCommandName.ToUpper(), paras, out reason);
            //                break;



            //            case "START-TRANSFER":
            //                {
            //                    if (trans.Primary.DataItem[0][3][0][0].ToString() != "RECIPE_NAME" ||
            //                        trans.Primary.DataItem[0][3][1][0].ToString() != "CAR_ID" ||
            //                        trans.Primary.DataItem[0][3][2][0].ToString() != "SRC_PORT_ID" ||
            //                        trans.Primary.DataItem[0][3][3][0].ToString() != "SOURCE_SLOT" ||
            //                        trans.Primary.DataItem[0][3][4][0].ToString() != "DEST_PORT_ID"
            //                        )
            //                    {
            //                        ret = false;
            //                        reason = "START-TRANSFER Command format is not correct.";
            //                        break;
            //                    }

            //                    SortHostCommand htcmd = new SortHostCommand();

            //                    htcmd.REMOTE_RECIPE_NAME = trans.Primary.DataItem[0][3][0][1].ToString();
            //                    htcmd.CAR_ID = trans.Primary.DataItem[0][3][1][1].ToString();
            //                    htcmd.SRC_PORT_ID = trans.Primary.DataItem[0][3][2][1].ToString();
            //                    if (trans.Primary.DataItem[0][3].Count > 5 && trans.Primary.DataItem[0][3][5][0].ToString() == "ALIGN_ANGLE")
            //                    {
            //                        htcmd.ALIGN_ANGLE = trans.Primary.DataItem[0][3][5][1].ToString();
            //                    }

            //                    List<string> sourceSlot = new List<string>();
            //                    List<string> destPort = new List<string>();
            //                    List<string> lm1 = new List<string>();
            //                    List<string> lm2 = new List<string>();





            //                    int count = trans.Primary.DataItem[0][3][3][1].Count;
            //                    if (trans.Primary.DataItem[0][3][4][1].Count < count)
            //                    {
            //                        ret = false;
            //                        reason = "START-TRANSFER Command format is not correct.";
            //                        break;
            //                    }
            //                    for (int i = 0; i < count; i++)
            //                    {
            //                        sourceSlot.Add(trans.Primary.DataItem[0][3][3][1][i].ToString());
            //                        destPort.Add(trans.Primary.DataItem[0][3][4][1][i].ToString());

            //                        if (trans.Primary.DataItem[0][3].Count > 5)
            //                        {
            //                            lm1.Add(trans.Primary.DataItem[0][3][5][1][i].ToString());
            //                        }
            //                        else
            //                        {
            //                            lm1.Add("");
            //                        }
            //                        if (trans.Primary.DataItem[0][3].Count > 6)
            //                        {
            //                            lm2.Add(trans.Primary.DataItem[0][3][6][1][i].ToString());
            //                        }
            //                        else
            //                            lm2.Add("");
            //                    }
            //                    htcmd.SRC_SLOT = sourceSlot.ToArray();
            //                    htcmd.DEST_PORT_ID = destPort.ToArray();
            //                    htcmd.LASERMARK1 = lm1.ToArray();
            //                    htcmd.LASERMARK2 = lm2.ToArray();

            //                    ret = _equipment.ExecuteFACommand("START-TRANSFER", new object[] { htcmd }, out reason);
            //                }
            //                break;

            //            case "PP-SELECT":
            //                if (trans.Primary.DataItem[0][3][0][0].ToString() != "RECIPE_ID" ||
            //                        trans.Primary.DataItem[0][3][0][1].ToString() == "")
            //                    throw new Exception("PP-SELECT Command Format Not Correct");
            //                string recipeId = trans.Primary.DataItem[0][3][0][1].ToString();
            //                ret = _equipment.ExecuteFACommand("PP-SELECT", new object[] { recipeId }, out _);

            //                break;

            //            case "SORTBYHOST":
            //                if (trans.Primary.DataItem[0][3][0][0].ToString() != "RECIPE_NAME" ||
            //                        trans.Primary.DataItem[0][3][0][1].ToString() == "")
            //                    throw new Exception("SORTBYHOST Command Format Not Correct");
            //                if (trans.Primary.DataItem[0][3][1][0].ToString() != "SRC_PORT_ID" ||
            //                        trans.Primary.DataItem[0][3][1][1].ToString() == "")
            //                    throw new Exception("SORTBYHOST Command Format Not Correct");
            //                if (trans.Primary.DataItem[0][3][2][0].ToString() != "DEST_PORT_ID" ||
            //                        trans.Primary.DataItem[0][3][2][1].ToString() == "")
            //                    throw new Exception("SORTBYHOST Command Format Not Correct");
            //                if (trans.Primary.DataItem[0][3][3][0].ToString() != "SOURCE_SLOT" ||
            //                        trans.Primary.DataItem[0][3][3][1].Count == 0)
            //                    throw new Exception("SORTBYHOST Command Format Not Correct");
            //                if (trans.Primary.DataItem[0][3][4][0].ToString() != "DEST_SLOT" ||
            //                        trans.Primary.DataItem[0][3][4][1].Count == 0)
            //                    throw new Exception("SORTBYHOST Command Format Not Correct");
            //                if (trans.Primary.DataItem[0][3][5][0].ToString() != "LASERMARK1" ||
            //                        trans.Primary.DataItem[0][3][5][1].Count == 0)
            //                    throw new Exception("SORTBYHOST Command Format Not Correct");

            //                string recipeName = trans.Primary.DataItem[0][3][0][1].ToString();

            //                int sourcePort = trans.Primary.DataItem[0][3][1][1].ToInt();
            //                int destPort1 = trans.Primary.DataItem[0][3][2][1].ToInt();

            //                List<int> sourceSlots = new List<int>();
            //                List<Tuple<int, string>> destSlotWafer = new List<Tuple<int, string>>();

            //                int count1 = trans.Primary.DataItem[0][3][3][1].Count;

            //                for (int i = 0; i < count1; i++)
            //                {
            //                    sourceSlots.Add(trans.Primary.DataItem[0][3][3][1][i].ToInt());
            //                    Tuple<int, string> item = new Tuple<int, string>(trans.Primary.DataItem[0][3][4][1][i].ToInt(),
            //                        trans.Primary.DataItem[0][3][5][1][i].ToString());
            //                    destSlotWafer.Add(item);
            //                }

            //                ret = _equipment.ExecuteFACommand(remoteCommandName.ToUpper(),
            //                    new object[] { recipeName, sourcePort, destPort1, sourceSlots.ToArray(), destSlotWafer.ToArray() }, out _);
            //                break;

            //            case "SORTBYSLOT":
            //                if (trans.Primary.DataItem[0][3][0][0].ToString() != "RECIPE_NAME" ||
            //                        trans.Primary.DataItem[0][3][0][1].ToString() == "")
            //                    throw new Exception("SORTBYSLOT Command Format Not Correct");
            //                if (trans.Primary.DataItem[0][3][1][0].ToString() != "SRC_PORT_ID" ||
            //                        trans.Primary.DataItem[0][3][1][1].ToString() == "")
            //                    throw new Exception("SORTBYSLOT Command Format Not Correct");
            //                if (trans.Primary.DataItem[0][3][2][0].ToString() != "DEST_PORT_ID" ||
            //                        trans.Primary.DataItem[0][3][2][1].ToString() == "")
            //                    throw new Exception("SORTBYSLOT Command Format Not Correct");
            //                if (trans.Primary.DataItem[0][3][3][0].ToString() != "SLOT_INDEX_LM" ||
            //                        trans.Primary.DataItem[0][3][3][1].ToString() == "")
            //                    throw new Exception("SORTBYSLOT Command Format Not Correct");
            //                if (trans.Primary.DataItem[0][3][4][0].ToString() != "SOURCE_SLOT" ||
            //                        trans.Primary.DataItem[0][3][4][1].Count == 0)
            //                    throw new Exception("SORTBYSLOT Command Format Not Correct");

            //                recipeName = trans.Primary.DataItem[0][3][0][1].ToString();
            //                sourcePort = trans.Primary.DataItem[0][3][1][1].ToInt();
            //                destPort1 = trans.Primary.DataItem[0][3][2][1].ToInt();
            //                int slotStartIndex = trans.Primary.DataItem[0][3][3][1].ToInt();

            //                List<int> sourceSlots1 = new List<int>();

            //                int count2 = trans.Primary.DataItem[0][3][4][1].Count;

            //                for (int i = 0; i < count2; i++)
            //                {
            //                    sourceSlots1.Add(trans.Primary.DataItem[0][3][4][1][i].ToInt());
            //                }

            //                ret = _equipment.ExecuteFACommand(remoteCommandName.ToUpper(),
            //                    new object[] { recipeName, sourcePort, destPort1, slotStartIndex, sourceSlots1.ToArray() }, out _);

            //                break;
            //            case "DISPATCHBYID":
            //                if (trans.Primary.DataItem[0][3][0][0].ToString() != "RECIPE_NAME" ||
            //                        trans.Primary.DataItem[0][3][0][1].ToString() == "")
            //                    throw new Exception("DISPATCHBYID Command Format Not Correct");
            //                if (trans.Primary.DataItem[0][3][1][0].ToString() != "SRC_PORT_ID" ||
            //                        trans.Primary.DataItem[0][3][1][1].ToString() == "")
            //                    throw new Exception("DISPATCHBYID Command Format Not Correct");
            //                if (trans.Primary.DataItem[0][3][2][0].ToString() != "ID_START_POS" ||
            //                        trans.Primary.DataItem[0][3][2][1].ToString() == "")
            //                    throw new Exception("DISPATCHBYID Command Format Not Correct");
            //                if (trans.Primary.DataItem[0][3][3][0].ToString() != "LM_ID" ||
            //                        trans.Primary.DataItem[0][3][3][1].Count == 0)
            //                    throw new Exception("DISPATCHBYID Command Format Not Correct");
            //                if (trans.Primary.DataItem[0][3][4][0].ToString() != "DEST_PORT" ||
            //                        trans.Primary.DataItem[0][3][4][1].Count == 0)
            //                    throw new Exception("DISPATCHBYID Command Format Not Correct");
            //                if (trans.Primary.DataItem[0][3][5][0].ToString() != "DEST_SLOT" ||
            //                        trans.Primary.DataItem[0][3][5][1].Count == 0)
            //                    throw new Exception("DISPATCHBYID Command Format Not Correct");

            //                recipeName = trans.Primary.DataItem[0][3][0][1].ToString();
            //                sourcePort = trans.Primary.DataItem[0][3][1][1].ToInt();
            //                int idstartPos = trans.Primary.DataItem[0][3][2][1].ToInt();

            //                List<Tuple<string, int, int>> waferTransfers = new List<Tuple<string, int, int>>();

            //                count1 = trans.Primary.DataItem[0][3][3][1].Count;
            //                if (count1 != trans.Primary.DataItem[0][3][4][1].Count ||
            //                    count1 != trans.Primary.DataItem[0][3][5][1].Count)
            //                    throw new Exception("DISPATCHBYID Command Format Not Correct");

            //                for (int i = 0; i < count1; i++)
            //                {
            //                    Tuple<string, int, int> transfer = new Tuple<string, int, int>(
            //                        trans.Primary.DataItem[0][3][3][1][i].ToString(),
            //                        trans.Primary.DataItem[0][3][4][1][i].ToInt(),
            //                        trans.Primary.DataItem[0][3][5][1][i].ToInt()
            //                        );

            //                    waferTransfers.Add(transfer);
            //                }

            //                ret = _equipment.ExecuteFACommand(remoteCommandName.ToUpper(),
            //                    new object[] { recipeName, sourcePort, idstartPos, waferTransfers }, out reason);
            //                break;


            //        }

            //        reply.DataItem.AddList();
            //        if (ret)
            //        {
            //            reply.DataItem[0].Add("HCACK", 0, SECsFormat.Binary);
            //        }
            //        else
            //        {
            //            reply.DataItem[0].Add("HCACK", 2/*Can not perform now*/, SECsFormat.Binary);

            //            reply.DataItem[0].Add("CPVALUE", reason, SECsFormat.Ascii);
            //        }

            //        _gem.SendReply(reply, trans.Id);
            //    }
            //    catch (Exception ex)
            //    {
            //        //WriteLog(string.Format("Handle_S2F49 Exception: {0}", ex.Message));
            //        SECsMessage reply = _gem.Services.CustomMessage.CreateMessage(2, 50, false);
            //        reply.DataItem.AddList();
            //        reply.DataItem[0].Add("HCACK", 3/*At least one parameter is invalid*/, SECsFormat.Binary);
            //        reply.DataItem[0].AddList();
            //        reply.DataItem[0][1].AddList();
            //        reply.DataItem[0][1].Add("CPVALUE", ex.Message, SECsFormat.Ascii);
            //        _gem.SendReply(reply, trans.Id);
            //    }

            //    Thread.Sleep(200);
            //    return true;
            //}
        //}

        public void Terminate()
        {
            //try
            //{
            //    _faMonitorThread.Stop();
            //    _logFabConnectCleaner.Stop();
            //}
            //catch (Exception ex)
            //{
            //    LOG.Write(ex);
            //}

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
            //try
            //{
            //    SynchronizeECIDValue();

            //    FAEventItemDemo ev;
            //    if (_lstEvent.TryDequeue(out ev) &&
            //        _gem.CommunicationState == CommunicationState.EnabledCommunicating)
            //    {
            //        if (ev.IsAlarm && ev.objDvid != null && ev.objDvid.ContainsKey("AlarmDescription"))
            //        {
            //            if (_gem.EquipmentModel.Alarms.ALIDCollection[ev.EventName] != null)
            //                _gem.EquipmentModel.Alarms.ALIDCollection[ev.EventName].description = ev.objDvid["AlarmDescription"].ToString();
            //        }

            //        if (ev.objDvid != null)
            //        {
            //            foreach (var dvid in ev.objDvid)
            //            {
            //                SetDVIDValue(dvid.Key, dvid.Value);
            //            }
            //        }


            //        SetDVIDValue(DVIDName.EVENT_NAME, ev.EventName);

            //        List<Account> account = AccountManager.GetLoginUserList();
            //        if (account.Count > 0)
            //        {
            //            SetDVIDValue(DVIDName.OPERATOR_ID, account[0].AccountId);
            //        }

            //        SynchronizeSVIDValue();
            //        if (ev.IsAlarm)
            //        {
            //            SetAlarm(ev.EventName);
            //        }
            //        else
            //        {
            //            SendEventToHost(ev.EventName);
            //        }

            //    }

            //    SECsMessage sMsg;
            //    while (_lstToBeSendMsg.TryDequeue(out sMsg))
            //    {
            //        if (_gem.CommunicationState != CommunicationState.EnabledCommunicating ||
            //        _gem.ControlState == ControlState.HostOffline ||
            //        _gem.ControlState != ControlState.EquipmentOffline)
            //            continue;

            //        lock (_locker)
            //        {
            //            LOG.Write("FAMessage: " + sMsg.ToString() + " is start senting.");
            //            _gem.Send(sMsg);
            //            LOG.Write("FAMessage: " + sMsg.ToString() + " is sent successfully.");

            //        }
            //    }

            //    SynchronizeSVIDValue();

            //    if (_gem.CommunicationState == CommunicationState.EnabledCommunicating &&
            //        _gem.ControlState != ControlState.EquipmentOffline &&
            //        _gem.ControlState != ControlState.HostOffline)
            //    {
            //        if (_gem.EquipmentModel.GemConnection.HSMS.linktestInterval > 0)
            //        {
            //            if (_monitorLinkTest.IsRunning && _monitorLinkTest.ElapsedMilliseconds >= _gem.EquipmentModel.GemConnection.HSMS.linktestInterval * 1000)
            //            {
            //                _monitorLinkTest.Restart();
            //                //SendUDMsg();

            //            }




            //        }

            //    }


            //}
            //catch (Exception ex)
            //{
            //    System.Diagnostics.Trace.WriteLine(ex);
            //}

            return true;
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



        // 1.Job setup;2.Job Processing;3.Job Processing Complete;4.Job Complete;5.Job Waiting for start
        //public void NotifyPRJobMileStone(E40ProcessJob PRJob, PRJobMileStoneEnum mileStone)
        //{
        //    if (!_isPRJobAlert) return;
        //    SECsMessage mStoneMsg = _gem.Services.CustomMessage.CreateMessage(16, 7, false);

        //    SECsDataItem dataItem = new SECsDataItem(SECsFormat.List);

        //    string strTime = DateTime.Now.ToString("yyyyMMddHHmmssff");
        //    dataItem.Add("TIMESTAMP", strTime, SECsFormat.Ascii);

        //    dataItem.Add("PRJOBID", PRJob.ObjtID, SECsFormat.Ascii);

        //    dataItem.Add("PRJOBID", (int)mileStone, SECsFormat.U2);
        //    SECsDataItem itemAck = new SECsDataItem(SECsFormat.List);
        //    itemAck.Add("ACKA", true, SECsFormat.Boolean);
        //    itemAck.AddList();
        //    dataItem.Add(itemAck);


        //    mStoneMsg.DataItem.Add(dataItem);


        //    _lstToBeSendMsg.Enqueue(mStoneMsg);









        //}

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

            //if (HostManager.FAProcessJobs == null || HostManager.FAProcessJobs.Count == 0)
            //    return;
            //long ceid = ServiceManager.DataCollection.GetCeIdByLogicalName(eventName);
            //foreach (var pj in HostManager.FAProcessJobs.Values)
            //{
            //    if (pj.PauseEvent.Contains((int)ceid))
            //    {
            //        lock (_lockerPj)
            //        {
            //            pj.PausePJ();
            //            Thread.Sleep(200);
            //        }
            //    }
            //}



        }

        public void SendMessge(string data)
        {
            throw new NotImplementedException();
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
