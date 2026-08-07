using CMP.Models.DataCenter;
using CMP.Models.Operation.LoadPort;
using CMP.Services.LoadPort;
using CMP.Services.Plc;
using CMP.Services.Tracking;
using Core.Framework;
using Microsoft.Identity.Client;
using CMP.Models.Resource;
using CMP.Models.kvsPlc;

namespace CMP;

public class GlobalService : Singleton<GlobalService>
{
    static GlobalService()
    {
        WaferActionDescriptionProvider.Initialize();
    }

    public static KVHostLinkInit kvHostLinkInit = new KVHostLinkInit();   
    public static MitsubishiPlcService plcComm => MitsubishiPlcService.Main;
    public static DataCenter DataCenter = DataCenter.Instance;
    public static GlobalChambers globalChambers = new GlobalChambers();
    public static Dictionary<int, Wafer> dicWaferIndex = new Dictionary<int, Wafer>();
    public static Queue<Wafer> waferQueue = new Queue<Wafer>();
    public static GlobalRobots globalRobots = new GlobalRobots();
    public static MitsubishiPlcService RobotControlPlc => MitsubishiPlcService.RobotCtrlInstance;
}
