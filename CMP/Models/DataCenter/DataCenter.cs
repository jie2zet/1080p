using CMP.Models.kvsPlc;
using CMP.Models.Resource;
using CMP.Models.Warn;
using CMP.Services.Log;
using CMP.Services.Plc;
using CMP.Services.Warn;
using CsvHelper;
using Dm.util;
using System.Globalization;
using System.IO;

namespace CMP.Models.DataCenter
{
    public class DataCenter
    {
        private const int StCtrlFastRefreshIntervalMs = 500;
        private const int StCtrlSlowRefreshIntervalMs = 2000;

        Task th_CollectData;
        Task th_CollectAlarm;
        Task th_CollectAxisData;
        Task th_CollectStCtrlData;
        public List<SV> listSV;
        public Dictionary<string, Axis> dicAxis;
        public List<Robot> listRobot;
        public List<STCtrl> listSTCtrl;
        public List<Polish> listPolish;
        public List<Roller> listRoller;
        public List<Pen> listPen;
        public List<Dry> listDry;

        PolishAlarmManage alarmManagePolishA;
        PolishAlarmManage alarmManagePolishB;
        PolishAlarmManage alarmManagePolishC;
        PolishAlarmManage alarmManagePolishD;
        RollerAlarmManage alarmManageRollerA;
        RollerAlarmManage alarmManageRollerB;
        PenAlarmManage alarmManagePenA;
        PenAlarmManage alarmManagePenB;
        DryAlarmManage alarmManageDryA;
        DryAlarmManage alarmManageDryB;
        RobotAlarmManage alarmManageRobot;
        public List<AlarmManage> listAlarmManage;
        IAlarmService alarmService;
        // 存储最新报警信息的集合，最新的报警显示在最上面
        public List<Alarm> listAlarms = new List<Alarm>();
        // 用于线程安全的锁对象
        private readonly object _alarmsLock = new object();

        static string SVBasePath = "D:\\CMP\\Configuration\\SV\\";
        static string AxisSVPath = SVBasePath + "SV_Axis.csv";
        string RobotSVPath = SVBasePath + "SV_Robot.csv";
        string STCtrlSVPath = SVBasePath + "SV_STCtrl.csv";
        string SVPolishAPath = SVBasePath + "SV_PolishA.csv";
        string SVPolishBPath = SVBasePath + "SV_PolishB.csv";
        string SVPolishCPath = SVBasePath + "SV_PolishC.csv";
        string SVPolishDPath = SVBasePath + "SV_PolishD.csv";
        string SVRollerAPath = SVBasePath + "SV_RollerA.csv";
        string SVRollerBPath = SVBasePath + "SV_RollerB.csv";
        string SVPenAPath = SVBasePath + "SV_PenA.csv";
        string SVPenBPath = SVBasePath + "SV_PenB.csv";
        string SVDryAPath = SVBasePath + "SV_DryA.csv";
        string SVDryBPath = SVBasePath + "SV_DryB.csv";

        private static readonly Lazy<DataCenter> _instance = new(() => new DataCenter());
        public static DataCenter Instance => _instance.Value;
        public DataCenter()
        {
            listSV = new List<SV>();
            dicAxis = new Dictionary<string, Axis>();
            listRobot = new List<Robot>();
            listSTCtrl = new List<STCtrl>();
            listPolish = new List<Polish>();
            listRoller = new List<Roller>();
            listPen = new List<Pen>();
            listDry = new List<Dry>();
            listAlarmManage = new List<AlarmManage>();


            CreatePolishAlarmManage(out alarmManagePolishA, "PolishA", GlobalService.kvHostLinkInit.kvLinkPolishA);
            CreatePolishAlarmManage(out alarmManagePolishB, "PolishB", GlobalService.kvHostLinkInit.kvLinkPolishB);
            CreatePolishAlarmManage(out alarmManagePolishC, "PolishC", GlobalService.kvHostLinkInit.kvLinkPolishC);
            CreatePolishAlarmManage(out alarmManagePolishD, "PolishD", GlobalService.kvHostLinkInit.kvLinkPolishD);
            CreateRollerAlarmManage(out alarmManageRollerA, "RollerA", GlobalService.kvHostLinkInit.kvLinkRollerA);
            CreateRollerAlarmManage(out alarmManageRollerB, "RollerB", GlobalService.kvHostLinkInit.kvLinkRollerB);
            CreatePenAlarmManage(out alarmManagePenA, "PenA", GlobalService.kvHostLinkInit.kvLinkPenA);
            CreatePenAlarmManage(out alarmManagePenB, "PenB", GlobalService.kvHostLinkInit.kvLinkPenB);
            CreateDryAlarmManage(out alarmManageDryA, "DryA", GlobalService.kvHostLinkInit.kvLinkDryA);
            CreateDryAlarmManage(out alarmManageDryB, "DryB", GlobalService.kvHostLinkInit.kvLinkDryB);
            CreateRobotAlarmManage(out alarmManageRobot, "Robot", -1);
            alarmService = new AlarmService();

            InitAxisSV(AxisSVPath);
            InitRobotSV(RobotSVPath);
            InitSTCtrlSV(STCtrlSVPath);
            InitSVPolish(SVPolishAPath);
            InitSVPolish(SVPolishBPath);
            InitSVPolish(SVPolishCPath);
            InitSVPolish(SVPolishDPath);
            InitSVRoller(SVRollerAPath);
            InitSVRoller(SVRollerBPath);
            InitSVPen(SVPenAPath);
            InitSVPen(SVPenBPath);
            InitSVDry(SVDryAPath);
            InitSVDry(SVDryBPath);

            th_CollectData = Task.Run(() => CollectData());
            th_CollectAlarm = Task.Run(() => CollectAlarm());
            th_CollectAxisData = Task.Run(() => CollectAxisData());
            th_CollectStCtrlData = Task.Run(() => CollectStCtrlData());
        }

        private void CreateRobotAlarmManage(out RobotAlarmManage alarmManage, string name, int sock)
        {
            alarmManage = new RobotAlarmManage(name, null);
            listAlarmManage.add(alarmManage);
        }

        private void CreatePolishAlarmManage(out PolishAlarmManage alarmManage, string name, KVLink link)

        {
            alarmManage = new PolishAlarmManage(name, link);
            listAlarmManage.add(alarmManage);
        }

        private void CreateRollerAlarmManage(out RollerAlarmManage alarmManage, string name, KVLink link)
        {
            alarmManage = new RollerAlarmManage(name, link);
            listAlarmManage.add(alarmManage);
        }

        private void CreatePenAlarmManage(out PenAlarmManage alarmManage, string name, KVLink link)
        {
            alarmManage = new PenAlarmManage(name, link);
            listAlarmManage.add(alarmManage);
        }

        private void CreateDryAlarmManage(out DryAlarmManage alarmManage, string name, KVLink link)
        {
            alarmManage = new DryAlarmManage(name, link);
            listAlarmManage.add(alarmManage);
        }

        public async Task CollectData()
        {
            await Task.Delay(1000);
            while (true)
            {
                // 主 PLC 未连接时拉长周期，避免大量同步读阻塞线程池并间接拖慢 UI
                bool plcOk = MitsubishiPlcService.Main.IsPLCConnected;
                await Task.Delay(plcOk ? 100 : 2000);
                foreach (Polish polish in listPolish)
                {
                    polish.CollectData();
                }
                foreach (Roller roller in listRoller)
                {
                    roller.CollectData();
                }
                foreach (Pen pen in listPen)
                {
                    pen.CollectData();
                }
                foreach (Dry dry in listDry)
                {
                    dry.CollectData();
                }
            }
        }

        public async Task CollectStCtrlData()
        {
            await Task.Delay(1000);
            var slowElapsedMs = 0;

            while (true)
            {
                bool plcOk = MitsubishiPlcService.Main.IsPLCConnected;
                var delayMs = plcOk ? StCtrlFastRefreshIntervalMs : 2000;
                await Task.Delay(delayMs);

                foreach (var st in listSTCtrl)
                {
                    st.RefreshStCtrlDataFast();
                }

                slowElapsedMs += delayMs;
                if (slowElapsedMs < StCtrlSlowRefreshIntervalMs)
                {
                    continue;
                }

                foreach (var st in listSTCtrl)
                {
                    st.RefreshStCtrlDataSlow();
                }

                slowElapsedMs = 0;
            }
        }

        public async Task CollectAlarm()
        {
            await Task.Delay(1000);
            while (true)
            {
                await Task.Delay(500);
                List<Alarm> resultAlarms = new List<Alarm>();
                // 并行收集所有设备的报警信息以提高性能
                var tasks = listAlarmManage.Select(am => Task.Run(() => am.CollectAlarm())).ToList();
                var results = await Task.WhenAll(tasks);
                foreach (var alarmList in results)
                {
                    var enabledAlarmList = alarmList
                        .Where(alarm => ChamberAvailability.IsEnabled(alarm.Unit))
                        .ToList();
                    if (enabledAlarmList.Count > 0)
                    {
                        if (enabledAlarmList[0].Unit == Unit.None)
                        {
                            resultAlarms.AddRange(enabledAlarmList);
                            continue;
                        }
                        foreach (Alarm alarm in enabledAlarmList)
                        {
                            if (alarm.Type == "Alarm")
                            {
                                int unitIndex = (int)alarm.Unit;

                                // 根据Unit编号范围确定设备类型
                                if (unitIndex >= 0 && unitIndex <= 15) // Chamber: 0-15
                                {
                                    if (unitIndex < GlobalService.globalChambers.listChamber.Count)
                                    {
                                        GlobalService.globalChambers.listChamber[unitIndex].ChamberStatus = ChamberStatus.Alarm;
                                    }
                                }
                                else if (unitIndex >= 16 && unitIndex <= 30) // Robot: 17-31 (对应0-14)
                                {
                                    int robotIndex = unitIndex - 16;
                                    if (robotIndex < GlobalService.globalRobots.listRobot.Count)
                                    {
                                        GlobalService.globalRobots.listRobot[robotIndex].RobotStatus = ChamberStatus.Alarm;
                                    }
                                }//FIXED
                            }
                        }
                    }
                    resultAlarms.AddRange(enabledAlarmList);
                }
                if (resultAlarms.Count > 0)
                {
                    Logger.Info($"收集到 {resultAlarms.Count} 条报警信息");
                    try
                    {
                        var insertResult = alarmService.AddAlarms(resultAlarms);
                        if (insertResult > 0)
                        {
                            Logger.Info($"成功插入 {insertResult} 条报警信息");
                        }
                        else
                        {
                            Logger.Warn("未能插入任何报警信息");
                        }
                        // 更新最新的报警列表，最新的报警显示在最上面
                        lock (_alarmsLock)
                        {
                            // 将新报警插入到列表开头
                            listAlarms.InsertRange(0, resultAlarms);
                            // 保持列表大小在合理范围内，例如只保留最近的100条报警
                            if (listAlarms.Count > 100)
                            {
                                listAlarms.RemoveRange(100, listAlarms.Count - 100);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"未能插入任何报警信息,原因{ex.Message}");
                    }

                }
            }
        }
        // 在 DataCenter 类中添加定时更新任务
        public async Task CollectAxisData()
        {
            await Task.Delay(1000);
            while (true)
            {
                try
                {
                    // 定时更新所有轴的数据
                    foreach (var axis in dicAxis.Values)
                    {
                        axis.RefreshAxisData();
                        await Task.Delay(100); // 为每个轴之间添加小延迟，避免PLC通信过载
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"更新轴数据时出错: {ex.Message}");
                }

                await Task.Delay(500); // 500ms更新一次，实现相对实时的显示
            }
        }
        public void InitAxisSV(string path)
        {
            string preModule = string.Empty;
            try
            {
                using (var reader = new StreamReader(path))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    // 跳过标题行
                    csv.Read();
                    csv.ReadHeader();

                    while (csv.Read())
                    {
                        var sv_PLC = new SV_PLC();
                        sv_PLC.SVID = csv.TryGetField<int>(0, out int svid) ? svid : 0;
                        sv_PLC.Name = csv.GetField<string>(1) ?? string.Empty;
                        sv_PLC.Type = csv.GetField<string>(2) ?? string.Empty;
                        sv_PLC.Value = csv.GetField<string>(3) ?? string.Empty;
                        sv_PLC.Unit = csv.GetField<string>(4) ?? string.Empty;
                        sv_PLC.PLCType = csv.GetField<string>(5) ?? string.Empty;
                        sv_PLC.PLCLink = csv.GetField<string>(6) ?? string.Empty;
                        sv_PLC.Max = csv.TryGetField<double>(7, out double max) ? max : 0.0;
                        sv_PLC.Min = csv.TryGetField<double>(8, out double min) ? min : 0.0;
                        sv_PLC.Module = csv.GetField<string>(9) ?? string.Empty;
                        sv_PLC.Descripition = csv.GetField<string>(10) ?? string.Empty;
                        sv_PLC.Comment = csv.GetField<string>(11) ?? string.Empty;

                        if (preModule != sv_PLC.Module)
                        {
                            Axis axis = new Axis(sv_PLC.Module, dicAxis.Count);
                            dicAxis.Add(axis.axisName, axis);
                            preModule = sv_PLC.Module;
                        }
                        dicAxis[sv_PLC.Module].listDataInSingleAxis.Add(sv_PLC);
                        listSV.Add(sv_PLC);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error initializing Axis SV from CSV file.", ex);
            }
        }

        public void InitRobotSV(string path)
        {
            string preModule = string.Empty;
            try
            {
                using (var reader = new StreamReader(path))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    // 跳过标题行
                    csv.Read();
                    csv.ReadHeader();

                    while (csv.Read())
                    {
                        var sv_PLC = new SV_PLC();
                        sv_PLC.SVID = csv.TryGetField<int>(0, out int svid) ? svid : 0;
                        sv_PLC.Name = csv.GetField<string>(1) ?? string.Empty;
                        sv_PLC.Type = csv.GetField<string>(2) ?? string.Empty;
                        sv_PLC.Value = csv.GetField<string>(3) ?? string.Empty;
                        sv_PLC.Unit = csv.GetField<string>(4) ?? string.Empty;
                        sv_PLC.PLCType = csv.GetField<string>(5) ?? string.Empty;
                        sv_PLC.PLCLink = csv.GetField<string>(6) ?? string.Empty;
                        sv_PLC.Max = csv.TryGetField<double>(7, out double max) ? max : 0.0;
                        sv_PLC.Min = csv.TryGetField<double>(8, out double min) ? min : 0.0;
                        sv_PLC.Module = csv.GetField<string>(9) ?? string.Empty;
                        sv_PLC.Descripition = csv.GetField<string>(10) ?? string.Empty;
                        sv_PLC.Comment = csv.GetField<string>(11) ?? string.Empty;

                        if (preModule != sv_PLC.Module)
                        {
                            Robot robot = new Robot(sv_PLC.Module);
                            listRobot.Add(robot);
                            preModule = sv_PLC.Module;
                        }
                        listRobot[listRobot.Count - 1].listDataInSingleRobot.Add(sv_PLC);
                        listSV.Add(sv_PLC);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error initializing Robot SV from CSV file.", ex);
            }
        }

        public void InitSTCtrlSV(string path)
        {
            string preModule = string.Empty;
            try
            {
                using (var reader = new StreamReader(path))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    // 跳过标题行
                    csv.Read();
                    csv.ReadHeader();

                    while (csv.Read())
                    {
                        var sv_PLC = new SV_PLC();
                        sv_PLC.SVID = csv.TryGetField<int>(0, out int svid) ? svid : 0;
                        sv_PLC.Name = csv.GetField<string>(1) ?? string.Empty;
                        sv_PLC.Type = csv.GetField<string>(2) ?? string.Empty;
                        sv_PLC.Value = csv.GetField<string>(3) ?? string.Empty;
                        sv_PLC.Unit = csv.GetField<string>(4) ?? string.Empty;
                        sv_PLC.PLCType = csv.GetField<string>(5) ?? string.Empty;
                        sv_PLC.PLCLink = csv.GetField<string>(6) ?? string.Empty;
                        sv_PLC.Max = csv.TryGetField<double>(7, out double max) ? max : 0.0;
                        sv_PLC.Min = csv.TryGetField<double>(8, out double min) ? min : 0.0;
                        sv_PLC.Module = csv.GetField<string>(9) ?? string.Empty;
                        sv_PLC.Descripition = csv.GetField<string>(10) ?? string.Empty;
                        sv_PLC.Comment = csv.GetField<string>(11) ?? string.Empty;

                        if (preModule != sv_PLC.Module)
                        {
                            STCtrl STCtrl = new STCtrl(sv_PLC.Module);
                            listSTCtrl.Add(STCtrl);
                            preModule = sv_PLC.Module;
                        }
                        listSTCtrl[listSTCtrl.Count - 1].listDataInSingleStCtrl.Add(sv_PLC);
                        listSV.Add(sv_PLC);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error initializing STCtrl SV from CSV file.", ex);
            }
        }

        public void InitSVPolish(string path)
        {
            Polish svPolish;
            if (path.contains("SV_PolishA"))
            {
                svPolish = new Polish("PolishA", GlobalService.kvHostLinkInit.kvLinkPolishA);
            }
            else if (path.contains("SV_PolishB"))
            {
                svPolish = new Polish("PolishB", GlobalService.kvHostLinkInit.kvLinkPolishB);
            }
            else if (path.contains("SV_PolishC"))
            {
                svPolish = new Polish("PolishC", GlobalService.kvHostLinkInit.kvLinkPolishC);
            }
            else
            {
                svPolish = new Polish("PolishD", GlobalService.kvHostLinkInit.kvLinkPolishD);
            }

            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // 跳过标题行
                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    var SV = new SV();
                    SV.SVID = csv.TryGetField<int>(0, out int svid) ? svid : 0;
                    SV.Name = csv.GetField<string>(1) ?? string.Empty;
                    SV.Type = csv.GetField<string>(2) ?? string.Empty;
                    SV.Value = csv.GetField<string>(3) ?? string.Empty;
                    SV.Unit = csv.GetField<string>(4) ?? string.Empty;
                    SV.Max = csv.TryGetField<double>(7, out double max) ? max : 0.0;
                    SV.Min = csv.TryGetField<double>(8, out double min) ? min : 0.0;
                    SV.Module = csv.GetField<string>(9) ?? string.Empty;
                    SV.Descripition = csv.GetField<string>(10) ?? string.Empty;
                    SV.Comment = csv.GetField<string>(11) ?? string.Empty;
                    if (!string.IsNullOrEmpty(SV.Name))
                        svPolish.ListSV[SV.Name] = SV;
                    listSV.Add(SV);
                }
                listPolish.Add(svPolish);
            }
        }

        public void InitSVRoller(string path)
        {
            Roller svRoller;
            if (path.contains("SV_RollerA"))
            {
                svRoller = new Roller("RollerA", GlobalService.kvHostLinkInit.kvLinkRollerA);
            }
            else
            {
                svRoller = new Roller("RollerB", GlobalService.kvHostLinkInit.kvLinkRollerB);
            }

            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // 跳过标题行
                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    var SV = new SV();
                    SV.SVID = csv.TryGetField<int>(0, out int svid) ? svid : 0;
                    SV.Name = csv.GetField<string>(1) ?? string.Empty;
                    SV.Type = csv.GetField<string>(2) ?? string.Empty;
                    SV.Value = csv.GetField<string>(3) ?? string.Empty;
                    SV.Unit = csv.GetField<string>(4) ?? string.Empty;
                    SV.Max = csv.TryGetField<double>(7, out double max) ? max : 0.0;
                    SV.Min = csv.TryGetField<double>(8, out double min) ? min : 0.0;
                    SV.Module = csv.GetField<string>(9) ?? string.Empty;
                    SV.Descripition = csv.GetField<string>(10) ?? string.Empty;
                    SV.Comment = csv.GetField<string>(11) ?? string.Empty;
                    if (!string.IsNullOrEmpty(SV.Name))
                        svRoller.ListSV[SV.Name] = SV;
                    listSV.Add(SV);
                }
                listRoller.Add(svRoller);
            }
        }

        public void InitSVPen(string path)
        {
            Pen svPen;
            if (path.contains("SV_PenA"))
            {
                svPen = new Pen("PenA", GlobalService.kvHostLinkInit.kvLinkPenA);
            }
            else
            {
                svPen = new Pen("PenB", GlobalService.kvHostLinkInit.kvLinkPenB);
            }

            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // 跳过标题行
                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    var SV = new SV();
                    SV.SVID = csv.TryGetField<int>(0, out int svid) ? svid : 0;
                    SV.Name = csv.GetField<string>(1) ?? string.Empty;
                    SV.Type = csv.GetField<string>(2) ?? string.Empty;
                    SV.Value = csv.GetField<string>(3) ?? string.Empty;
                    SV.Unit = csv.GetField<string>(4) ?? string.Empty;
                    SV.Max = csv.TryGetField<double>(7, out double max) ? max : 0.0;
                    SV.Min = csv.TryGetField<double>(8, out double min) ? min : 0.0;
                    SV.Module = csv.GetField<string>(9) ?? string.Empty;
                    SV.Descripition = csv.GetField<string>(10) ?? string.Empty;
                    SV.Comment = csv.GetField<string>(11) ?? string.Empty;
                    if (!string.IsNullOrEmpty(SV.Name))
                        svPen.ListSV[SV.Name] = SV;
                    listSV.Add(SV);
                }
                listPen.Add(svPen);
            }
        }

        public void InitSVDry(string path)
        {
            Dry svDry;
            if (path.contains("SV_DryA"))
            {
                svDry = new Dry("DryA", GlobalService.kvHostLinkInit.kvLinkDryA);
            }
            else
            {
                svDry = new Dry("DryB", GlobalService.kvHostLinkInit.kvLinkDryB);
            }

            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // 跳过标题行
                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    var SV = new SV();
                    SV.SVID = csv.TryGetField<int>(0, out int svid) ? svid : 0;
                    SV.Name = csv.GetField<string>(1) ?? string.Empty;
                    SV.Type = csv.GetField<string>(2) ?? string.Empty;
                    SV.Value = csv.GetField<string>(3) ?? string.Empty;
                    SV.Unit = csv.GetField<string>(4) ?? string.Empty;
                    SV.Max = csv.TryGetField<double>(7, out double max) ? max : 0.0;
                    SV.Min = csv.TryGetField<double>(8, out double min) ? min : 0.0;
                    SV.Module = csv.GetField<string>(9) ?? string.Empty;
                    SV.Descripition = csv.GetField<string>(10) ?? string.Empty;
                    SV.Comment = csv.GetField<string>(11) ?? string.Empty;
                    if (!string.IsNullOrEmpty(SV.Name))
                        svDry.ListSV[SV.Name] = SV;
                    listSV.Add(SV);
                }
                listDry.Add(svDry);
            }
        }

        // 提供获取最新报警列表的方法
        public List<Alarm> GetLatestAlarms(int count = 100)
        {
            lock (_alarmsLock)
            {
                return listAlarms.Take(count).ToList();
            }
        }

        // 提供获取所有报警列表的方法
        public List<Alarm> GetAllAlarms()
        {
            lock (_alarmsLock)
            {
                return new List<Alarm>(listAlarms);
            }
        }
    }
}
