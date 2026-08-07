using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMP.Models.kvsPlc;
using KHL = CMP.Models.kvsPlc.KvHostLink;
using System.Security.Cryptography.Xml;
using CMP.Services.Log;
using CMP.Modules;
using CMP.Models.Operation.Polish;
using CMP.Models.Warn;

namespace CMP.Models.kvsPlc
{
    public class KVHostLinkInit
    {
        int err = (int)KHLUserError.KHL_ERR_NONE;
        public string ipPolishA = "192.168.2.10";
        public string ipPolishB = "192.168.2.11";
        public string ipPolishC = "192.168.2.12";
        public string ipPolishD = "192.168.2.13";
        public string ipRollerA = "192.168.2.20";
        public string ipRollerB = "192.168.2.23";
        public string ipPenA = "192.168.2.30";
        public string ipPenB = "192.168.2.32";
        public string ipDryA = "192.168.2.40";
        public string ipDryB = "192.168.2.41";
        public KVLink kvLinkPolishA;
        public KVLink kvLinkPolishB;
        public KVLink kvLinkPolishC;
        public KVLink kvLinkPolishD;
        public KVLink kvLinkRollerA;
        public KVLink kvLinkRollerB;
        public KVLink kvLinkPenA;
        public KVLink kvLinkPenB;
        public KVLink kvLinkDryA;
        public KVLink kvLinkDryB;
        //public int sockPolishA;
        //public int sockPolishB;
        //public int sockPolishC;
        //public int sockPolishD;
        //public int sockRollerA;
        //public int sockRollerB;
        //public int sockPenA;
        //public int sockPenB;
        //public int sockDryA;
        //public int sockDryB;
        private Dictionary<Unit, KVLink> dicChamberNameToKvLink;
        //private Dictionary<string, int> dicIPToSock;

        //private IEventAggregator _eventAggregator;

        public KVHostLinkInit()
        {
            //_eventAggregator = GlobalAggregatorModule._eventAggregator!;
            if (err != (int)KHLUserError.KHL_ERR_NONE)
            {
                err = KHL.KHLInit();
                if (err != (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.ErrorGlobal($"KHL Init Fail");
                }
            }
            //Task.Run(() =>
            //{
                kvLinkPolishA = new KVLink(ipPolishA);
                AssignUnitSock(ref kvLinkPolishA);
                kvLinkPolishB = new KVLink(ipPolishB);
                AssignUnitSock(ref kvLinkPolishB);
                kvLinkPolishC = new KVLink(ipPolishC);
                AssignUnitSock(ref kvLinkPolishC);
                kvLinkPolishD = new KVLink(ipPolishD);
                AssignUnitSock(ref kvLinkPolishD);
                kvLinkRollerA = new KVLink(ipRollerA);
                AssignUnitSock(ref kvLinkRollerA);
                kvLinkRollerB = new KVLink(ipRollerB);
                AssignUnitSock(ref kvLinkRollerB);
                kvLinkPenA = new KVLink(ipPenA);
                AssignUnitSock(ref kvLinkPenA);
                kvLinkPenB = new KVLink(ipPenB);
                AssignUnitSock(ref kvLinkPenB);
                kvLinkDryA = new KVLink(ipDryA);
                AssignUnitSock(ref kvLinkDryA);
                kvLinkDryB = new KVLink(ipDryB);
                AssignUnitSock(ref kvLinkDryB);
            //});

            dicChamberNameToKvLink = new Dictionary<Unit, KVLink>();
            dicChamberNameToKvLink.Add(Unit.PolishA, kvLinkPolishA);
            dicChamberNameToKvLink.Add(Unit.PolishB, kvLinkPolishB);
            dicChamberNameToKvLink.Add(Unit.PolishC, kvLinkPolishC);
            dicChamberNameToKvLink.Add(Unit.PolishD, kvLinkPolishD);
            dicChamberNameToKvLink.Add(Unit.RollerA1, kvLinkRollerA);
            dicChamberNameToKvLink.Add(Unit.RollerA2, kvLinkRollerA);
            dicChamberNameToKvLink.Add(Unit.RollerA3, kvLinkRollerA);
            dicChamberNameToKvLink.Add(Unit.RollerB1, kvLinkRollerB);
            dicChamberNameToKvLink.Add(Unit.RollerB2, kvLinkRollerB);
            dicChamberNameToKvLink.Add(Unit.RollerB3, kvLinkRollerB);
            dicChamberNameToKvLink.Add(Unit.PenA1, kvLinkPenA);
            dicChamberNameToKvLink.Add(Unit.PenA2, kvLinkPenA);
            dicChamberNameToKvLink.Add(Unit.PenB1, kvLinkPenB);
            dicChamberNameToKvLink.Add(Unit.PenB2, kvLinkPenB);
            dicChamberNameToKvLink.Add(Unit.DryA, kvLinkDryA);
            dicChamberNameToKvLink.Add(Unit.DryB, kvLinkDryB);
        }

        private void AssignUnitSock(ref KVLink kvLink)
        {
            err = KHL.KHLConnect(kvLink.IP, 8500, 500, KHLSockType.SOCK_TCP, ref kvLink.sock);
            if (err != (int)KHLUserError.KHL_ERR_NONE)
            {
                kvLink.sock = -1;
            }
        }

        public void ConnectPLC(string name)
        {
            Unit unit = (Unit)Enum.Parse(typeof(Unit), name);
            KVLink link = dicChamberNameToKvLink[unit];
            AssignUnitSock(ref link);
        }
    }

    public class KVLink
    {
        public string IP;
        public int sock;

        public KVLink(string IP)
        {
            this.IP = IP;
            this.sock = -1;
        }
    }
}
