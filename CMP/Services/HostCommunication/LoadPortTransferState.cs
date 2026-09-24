using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Services.HostCommunication
{
    public enum LoadPortTransferState
    {
        LoadPortInitialized, //87001 初始化

        LoadPortInService, //87002 LoadPort和主机通信正常 

        LoadPortOutOfService,//87003 LoadPort和主机通信异常

        TransferReadyStateEntered,//87004 LoadPort状态正常，可进行装载作业

        TransferBlockedStateEntered,//87005 LoadPort状态异常，不可进行装载作业

        ReadyToLoadEntered,//87006 LoadPort状态正常且没有Foup

        ReadyToUnloadEntered,//87007 LoadPort可以使用且有Foup

        TransferBlockedFromReadyToLoad,//87008 正在进行LoadPort操作

        TransferBlockedFromReadyToUnload, //87009 LoadPort正在进行卸载操作

        ReadyToLoadEnteredFromTransferBlocked, //87010 LoadPort卸载完成，LoadPort已准备就绪

        ReadyToUnloadEnteredFromTransferBlocked, //87011 加工完成，可以进行LoadPort卸载操作

        TransferReadyEnteredFromTransferBlocked  //87012 LoadPort卸载失败
    }
}
