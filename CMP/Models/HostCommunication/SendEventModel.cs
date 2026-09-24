using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Models.HostCommunication
{
    public class SendEventModel
    {
        public string? EventName { get; set; }

        public int LoadportId { get; set; }

        public int PortTransFerState { get; set; }

        public int CarrierID { get; set; }

        public bool IsSendCarrierID { get; set; }

        public SendEventModel(string eventName, int loadportId, int portTransFerState, int carrierID, bool isSendCarrierID)
        {
            EventName = eventName;
            LoadportId = loadportId;
            PortTransFerState = portTransFerState;
            CarrierID = carrierID;
            IsSendCarrierID = isSendCarrierID;
        }

    }
}
