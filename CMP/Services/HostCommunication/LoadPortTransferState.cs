using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Services.HostCommunication
{
    public enum LoadPortTransferState
    {
        LoadPortInitialized,

        LoadPortInService,

        LoadPortOutOfService,

        TransferReadyStateEntered,

        TransferBlockedStateEntered,

        ReadyToLoadEntered,

        ReadyToUnloadEntered,

        TransferBlockedFromReadyToLoad,

        TransferBlockedFromReadyToUnload,

        ReadyToLoadEnteredFromTransferBlocked,

        ReadyToUnloadEnteredFromTransferBlocked,

        TransferReadyEnteredFromTransferBlocked
    }
}
