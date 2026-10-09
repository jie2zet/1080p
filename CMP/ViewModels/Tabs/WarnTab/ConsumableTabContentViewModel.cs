using CMP.Services.Auth;
using CMP.Constants.Enums;
using CMP.Models.kvsPlc;
using CMP.Models.Resource;
using CMP.Models.Warn;
using CMP.Services.Log;
using CMP.Utils;
using KHSTPolishCollectData = CMP.Models.kvsPlc.Polish.KvsStructPolishCollectData;
using KHSTRollerCollectData = CMP.Models.kvsPlc.Roll.KvsStructRollerCollectData;
using KHSTPenCollectData = CMP.Models.kvsPlc.Pen.KvsStructPenCollectData;
using DocumentFormat.OpenXml.EMMA;
using HandyControl.Controls;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using KHL = CMP.Models.kvsPlc.KvHostLink;
using KST = CMP.Models.kvsPlc.Clean.KvStruct;

namespace CMP.ViewModels.Tabs.WarnTab
{

    public class ConsumableTabContentViewModel : BindableBase
    {
        private readonly string _dateTimeFormate = "yyyy-MM-dd HH:mm:ss";

        private readonly string consumableFile = "D:\\CMP\\Configuration\\Consumable\\ConsumableInfo.json";
        private readonly string g3HeadFile = "D:\\CMP\\Configuration\\Consumable\\AlarmLimitInfo.json";
        private const uint LifetimeWaferCountAddress = 392000;

        private ObservableCollection<IgnoreConsumableProperty> _ignoreConsumableProperties;
        //<耗材名、耗材参数>
        private ObservableCollection<ConsumableModel> _consumableModels;
        private readonly Dictionary<string, (string? WaferCountSet, string? UsedTimeSet)> _lastWritten = new();

        public ObservableCollection<ConsumableModel> ConsumableModels
        {
            get { return _consumableModels; }
            set { _consumableModels = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<ConsumableGroup> _consumableGroups;

        public ObservableCollection<ConsumableGroup> ConsumableGroups
        {
            get => _consumableGroups;
            set => SetProperty(ref _consumableGroups, value);
        }

        private ObservableCollection<G3HeadModel> _g3HeadModels = [];
        private ObservableCollection<G3HeadGroup> _g3HeadGroups = [];

        public ObservableCollection<G3HeadGroup> G3HeadGroups
        {
            get => _g3HeadGroups;
            set => SetProperty(ref _g3HeadGroups, value);
        }

        private uint _polisherALifetimeCount;
        private uint _polisherBLifetimeCount;
        private uint _polisherCLifetimeCount;
        private uint _polisherDLifetimeCount;

        public string PolisherALifetimeCount => _polisherALifetimeCount.ToString();
        public string PolisherBLifetimeCount => _polisherBLifetimeCount.ToString();
        public string PolisherCLifetimeCount => _polisherCLifetimeCount.ToString();
        public string PolisherDLifetimeCount => _polisherDLifetimeCount.ToString();
        public string EquipmentLifetimeCount =>
            (_polisherALifetimeCount + _polisherBLifetimeCount + _polisherCLifetimeCount + _polisherDLifetimeCount).ToString();

        public DelegateCommand<ConsumableModel> UpdateCommand { get; set; }

        public DelegateCommand<G3HeadModel> G3HeadUpdateCommand { get; set; }

        public DelegateCommand<string> SelectionChangedCommand { get; set; }

        private readonly Task _readTask;

        public ConsumableTabContentViewModel()
        {
            _ignoreConsumableProperties = [];
            _consumableModels = new ObservableCollection<ConsumableModel>();
            _consumableGroups = new ObservableCollection<ConsumableGroup>();
            UpdateCommand = new DelegateCommand<ConsumableModel>(OnUpdate);
            G3HeadUpdateCommand = new DelegateCommand<G3HeadModel>(OnG3HeadUpdate);
            SelectionChangedCommand = new DelegateCommand<string>(OnSelectionChanged);
            _readTask = Task.Factory.StartNew(() => { ReadConsumableInfo(); }, TaskCreationOptions.LongRunning);
            InitIgnoreConsumableProperties();
            InitConsumable();
            InitG3Head();
        }

        private async void ReadConsumableInfo()
        {
            while (true)
            {
                await Task.Delay(1000);
                if (_consumableModels == null || _consumableModels.Count == 0) continue;
                foreach (var item in _consumableModels)
                {
                    if (item == null) continue;
                    if (IsPolishWorkstation(item.Workstation))
                    {
                        ApplyPolishFromDataCenter(item);
                    }
                    else if (IsRollerWorkstation(item.Workstation))
                    {
                        ApplyRollerFromDataCenter(item);
                    }
                    else if (IsPenWorkstation(item.Workstation))
                    {
                        ApplyPenFromDataCenter(item);
                    }
                }

                foreach (var item in _g3HeadModels)
                {
                    ReadG3HeadRow(item);
                }

                RefreshPolishLifetimeCounts();
            }
        }

        private static bool IsPolishWorkstation(WorkstationType workstation) =>
            workstation is WorkstationType.PolishA
                or WorkstationType.PolishB
                or WorkstationType.PolishC
                or WorkstationType.PolishD;

        private static void ApplyPolishFromDataCenter(ConsumableModel item)
        {
            if (!TryGetPolishCollectData(item.Workstation, out var data))
            {
                return;
            }

            switch (item.ConsumableName)
            {
                case "Unit":
                    item.WaferCount = data.UnitWaferCount.ToString();
                    break;
                case "Airbag":
                    item.WaferCount = data.AirBagWaferCount.ToString();
                    item.UsedTime = data.AirBagUesTime.ToString();
                    break;
                case "Membrance":
                    item.WaferCount = data.MembranceWaferCount.ToString();
                    item.UsedTime = data.MembranceUesTime.ToString();
                    break;
                case "RetainerRing":
                    item.WaferCount = data.RatainerRingWaferCount.ToString();
                    item.UsedTime = data.RatainerRingUesTime.ToString();
                    break;
                case "Pad":
                    item.WaferCount = data.PadWaferCount.ToString();
                    item.UsedTime = data.PadUesTime.ToString();
                    break;
                case "Disk_Insitu":
                    item.UsedTime = data.ConditionInsiUesTime.ToString();
                    break;
                case "Disk_Exsitu":
                    item.UsedTime = data.ConditionExsiUesTime.ToString();
                    break;
            }
        }

        private static bool TryGetPolishCollectData(WorkstationType workstation, out KHSTPolishCollectData.stPolishCollectData data)
        {
            data = null!;
            var polish = GlobalService.DataCenter?.listPolish?
                .FirstOrDefault(item => item.Name == workstation.ToString());
            return polish != null && polish.TryGetProcessDataSnapshot(out data);
        }

        private static bool IsRollerWorkstation(WorkstationType workstation) =>
            workstation is WorkstationType.RollerA1
                or WorkstationType.RollerA2
                or WorkstationType.RollerA3
                or WorkstationType.RollerB1
                or WorkstationType.RollerB2
                or WorkstationType.RollerB3;

        private static void ApplyRollerFromDataCenter(ConsumableModel item)
        {
            if (!TryGetRollerCollectData(item.Workstation, out var data, out var station))
            {
                return;
            }

            switch (station)
            {
                case 1:
                    ApplyRollerStation(item, data._1UnitWaferCount, data._1LowerRollerWaferCount, data._1LowerRollerCumlativeTime,
                        data._1UpperRollerWaferCount, data._1UpperRollerCumlativeTime, data._1WFPillarRotationWaferCount, data._1WFPillarRotationCumlativeTime);
                    break;
                case 2:
                    ApplyRollerStation(item, data._2UnitWaferCount, data._2LowerRollerWaferCount, data._2LowerRollerCumlativeTime,
                        data._2UpperRollerWaferCount, data._2UpperRollerCumlativeTime, data._2WFPillarRotationWaferCount, data._2WFPillarRotationCumlativeTime);
                    break;
                case 3:
                    ApplyRollerStation(item, data._3UnitWaferCount, data._3LowerRollerWaferCount, data._3LowerRollerCumlativeTime,
                        data._3UpperRollerWaferCount, data._3UpperRollerCumlativeTime, data._3WFPillarRotationWaferCount, data._3WFPillarRotationCumlativeTime);
                    break;
            }
        }

        private static void ApplyRollerStation(
            ConsumableModel item,
            int unitCount,
            int lowerCount, int lowerTime,
            int upperCount, int upperTime,
            int pillarCount, int pillarTime)
        {
            switch (item.ConsumableName)
            {
                case "Unit":
                    item.WaferCount = ToIntText(unitCount);
                    break;
                case "LowerRoller":
                    item.WaferCount = ToIntText(lowerCount);
                    item.UsedTime = ToIntText(lowerTime);
                    break;
                case "UpperRoller":
                    item.WaferCount = ToIntText(upperCount);
                    item.UsedTime = ToIntText(upperTime);
                    break;
                case "Pillar":
                    item.WaferCount = ToIntText(pillarCount);
                    item.UsedTime = ToIntText(pillarTime);
                    break;
            }
        }

        private static bool TryGetRollerCollectData(
            WorkstationType workstation,
            out KHSTRollerCollectData.stRollerCollectData data,
            out int station)
        {
            data = null!;
            station = workstation switch
            {
                WorkstationType.RollerA1 or WorkstationType.RollerB1 => 1,
                WorkstationType.RollerA2 or WorkstationType.RollerB2 => 2,
                WorkstationType.RollerA3 or WorkstationType.RollerB3 => 3,
                _ => 0
            };
            if (station == 0)
            {
                return false;
            }

            var chamberName = workstation is WorkstationType.RollerA1 or WorkstationType.RollerA2 or WorkstationType.RollerA3
                ? "RollerA"
                : "RollerB";
            var roller = GlobalService.DataCenter?.listRoller?
                .FirstOrDefault(item => item.Name == chamberName);
            return roller != null && roller.TryGetProcessDataSnapshot(out data);
        }

        private static string ToIntText(int value) => value.ToString();

        private static bool IsPenWorkstation(WorkstationType workstation) =>
            workstation is WorkstationType.PenA1 or WorkstationType.PenB1;

        private static void ApplyPenFromDataCenter(ConsumableModel item)
        {
            if (!TryGetPenCollectData(item.Workstation, out var data))
            {
                return;
            }

            switch (item.ConsumableName)
            {
                case "Unit":
                    item.WaferCount = ToIntText(data._1UnitWaferCount);
                    break;
                case "Brush":
                case "Pen":
                    item.WaferCount = ToIntText(data._1BrushWaferCount);
                    item.UsedTime = ToIntText(data._1BrushCumlativeTime);
                    break;
            }
        }

        private static bool TryGetPenCollectData(WorkstationType workstation, out KHSTPenCollectData.stPenCollectData data)
        {
            data = null!;
            var chamberName = workstation == WorkstationType.PenA1 ? "PenA" : "PenB";
            var pen = GlobalService.DataCenter?.listPen?
                .FirstOrDefault(item => item.Name == chamberName);
            return pen != null && pen.TryGetProcessDataSnapshot(out data);
        }

        private void OnSelectionChanged(string header)
        {
            if (string.IsNullOrEmpty(header)) return;

            switch (header)
            {
                case "Polish":
                    var polishGroups = _consumableModels.Where(x => x.Workstation == WorkstationType.PolishA || x.Workstation == WorkstationType.PolishB || x.Workstation == WorkstationType.PolishC || x.Workstation == WorkstationType.PolishD)
                        .GroupBy(x => x.Workstation)
                        .Select(group => new ConsumableGroup(group.Key) { Consumables = new ObservableCollection<ConsumableModel>(group) });
                    ConsumableGroups = new ObservableCollection<ConsumableGroup>(polishGroups);
                    break;
                case "Roller":
                    var rollerGroups = _consumableModels.Where(x => x.Workstation == WorkstationType.RollerA1 || x.Workstation == WorkstationType.RollerA2 || x.Workstation == WorkstationType.RollerA3 || x.Workstation == WorkstationType.RollerB1 || x.Workstation == WorkstationType.RollerB2 || x.Workstation == WorkstationType.RollerB3)
                        .GroupBy(x => x.Workstation)
                        .Select(group => new ConsumableGroup(group.Key) { Consumables = new ObservableCollection<ConsumableModel>(group) });
                    ConsumableGroups = new ObservableCollection<ConsumableGroup>(rollerGroups);
                    break;
                case "Pen":
                    var penGroups = _consumableModels.Where(x => x.Workstation == WorkstationType.PenA1 || x.Workstation == WorkstationType.PenB1)
                        .GroupBy(x => x.Workstation)
                        .Select(group => new ConsumableGroup(group.Key) { Consumables = new ObservableCollection<ConsumableModel>(group) });
                    ConsumableGroups = new ObservableCollection<ConsumableGroup>(penGroups);
                    break;
                case "Alarm Limit":
                    break;
                default:
                    break;
            }
        }

        private void InitG3Head()
        {
            if (!Directory.Exists(Path.GetDirectoryName(g3HeadFile)!))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(g3HeadFile)!);
            }

            if (!File.Exists(g3HeadFile))
            {
                File.Create(g3HeadFile).Close();
            }

            var json = FileUtil.ReadFile(g3HeadFile);
            var list = JsonConvert.DeserializeObject<ObservableCollection<G3HeadModel>>(json);
            _g3HeadModels = list is { Count: > 0 } ? list : CreateDefaultG3HeadModels();
            foreach (var item in _g3HeadModels)
            {
                item.PropertyChanged += OnG3HeadPropertyChanged;
            }

            RebuildG3HeadGroups();
            SaveG3Head();
        }

        private static ObservableCollection<G3HeadModel> CreateDefaultG3HeadModels()
        {
            var list = new ObservableCollection<G3HeadModel>();
            foreach (var workstation in new[]
                     {
                         WorkstationType.PolishA, WorkstationType.PolishB,
                         WorkstationType.PolishC, WorkstationType.PolishD
                     })
            {
                list.Add(new() { Workstation = workstation, ModuleName = "Pad Wear Amount (mm)", LightFaultAdress = 22136, ProcessInterlockAdress = 22138, PresentDAdress = 10022, Note = "0" });
                list.Add(new() { Workstation = workstation, ModuleName = "Disk Cutting Rate (um/h)", PresentWAdress = "35A2", Note = "0" });
                list.Add(new() { Workstation = workstation, ModuleName = "Change Tolerance Min (um/h)", LightFaultAdress = 22150, ProcessInterlockAdress = 22152, Note = "0" });
                list.Add(new() { Workstation = workstation, ModuleName = "Change Tolerance Max (um/h)", LightFaultAdress = 22148, ProcessInterlockAdress = 22146, Note = "0" });
                list.Add(new() { Workstation = workstation, ModuleName = "Retainer Ring Height (at changed) (mm)", PresentWAdress = "35A4", Note = "0" });
                list.Add(new() { Workstation = workstation, ModuleName = "Retainer Ring Wear Amount (mm)", PresentWAdress = "35A6", Note = "0" });
                list.Add(new() { Workstation = workstation, ModuleName = "Retainer Ring Wear Limit (mm)", LightFaultAdress = 22164, ProcessInterlockAdress = 22166, Note = "0" });
            }

            return list;
        }

        private void RebuildG3HeadGroups()
        {
            var groups = _g3HeadModels
                .GroupBy(x => x.Workstation)
                .Select(group => new G3HeadGroup(group.Key)
                {
                    Parts = new ObservableCollection<G3HeadModel>(group)
                });
            G3HeadGroups = new ObservableCollection<G3HeadGroup>(groups);
        }

        private void OnG3HeadPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(G3HeadModel.Note))
            {
                SaveG3Head();
            }
        }

        private void SaveG3Head()
        {
            FileUtil.WriteFile(g3HeadFile, JsonConvert.SerializeObject(_g3HeadModels, Formatting.Indented));
        }

        private void ReadG3HeadRow(G3HeadModel item)
        {
            var hSock = GetSock(item.Workstation);
            if (hSock == 0)
            {
                return;
            }

            if (TryReadFloat(hSock, KHLDevType.DEV_ZF, item.LightFaultAdress, out var lightFault))
            {
                var text = FormatFloat(lightFault);
                if (string.IsNullOrEmpty(item.LightFault) || item.LightFault == item.LastLightFaultFromPlc)
                {
                    item.LightFault = text;
                }
                item.LastLightFaultFromPlc = text;
            }

            if (TryReadFloat(hSock, KHLDevType.DEV_ZF, item.ProcessInterlockAdress, out var processInterlock))
            {
                var text = FormatFloat(processInterlock);
                if (string.IsNullOrEmpty(item.ProcessInterlock) || item.ProcessInterlock == item.LastProcessInterlockFromPlc)
                {
                    item.ProcessInterlock = text;
                }
                item.LastProcessInterlockFromPlc = text;
            }

            if (TryReadFloat(hSock, KHLDevType.DEV_ZF, item.PresentAdress, out var presentZf))
            {
                item.Present = FormatFloat(presentZf);
            }
            else if (TryReadFloat(hSock, KHLDevType.DEV_D, item.PresentDAdress, out var presentD))
            {
                item.Present = FormatFloat(presentD);
            }
            else if (G3HeadModel.TryParseW(item.PresentWAdress, out var presentW)
                     && TryReadFloat(hSock, KHLDevType.DEV_W, presentW, out var presentWValue))
            {
                item.Present = FormatFloat(presentWValue);
            }
        }

        private static bool TryReadFloat(int hSock, KHLDevType device, uint address, out float value)
        {
            value = 0;
            if (address == 0)
            {
                return false;
            }

            var buffer = new byte[8];
            var error = KHL.KHLReadDevicesAsWords(hSock, device, address, 2, buffer);
            if (error != (int)KHLUserError.KHL_ERR_NONE)
            {
                return false;
            }

            KST.ByteToFloat(ref value, buffer);
            return true;
        }

        private static string FormatFloat(float value) => value.ToString("0.####", CultureInfo.InvariantCulture);

        private void RefreshPolishLifetimeCounts()
        {
            TryReadLifetimeCount(WorkstationType.PolishA, ref _polisherALifetimeCount, nameof(PolisherALifetimeCount));
            TryReadLifetimeCount(WorkstationType.PolishB, ref _polisherBLifetimeCount, nameof(PolisherBLifetimeCount));
            TryReadLifetimeCount(WorkstationType.PolishC, ref _polisherCLifetimeCount, nameof(PolisherCLifetimeCount));
            TryReadLifetimeCount(WorkstationType.PolishD, ref _polisherDLifetimeCount, nameof(PolisherDLifetimeCount));
        }

        private void TryReadLifetimeCount(WorkstationType workstation, ref uint field, string propertyName)
        {
            var hSock = GetSock(workstation);
            if (hSock == 0)
            {
                return;
            }

            var buffer = new byte[8];
            var error = KHL.KHLReadDevicesAsWords(hSock, KHLDevType.DEV_ZF, LifetimeWaferCountAddress, 2, buffer);
            if (error != (int)KHLUserError.KHL_ERR_NONE)
            {
                return;
            }

            uint count = 0;
            KST.ByteToUint(ref count, buffer);
            if (field == count)
            {
                return;
            }

            field = count;
            RaisePropertyChanged(propertyName);
            RaisePropertyChanged(nameof(EquipmentLifetimeCount));
        }

        private void OnG3HeadUpdate(G3HeadModel item)
        {
            if (item == null)
            {
                return;
            }

            var hSock = GetSock(item.Workstation);
            if (hSock == 0)
            {
                Growl.Warning($"{item.Workstation}-{item.ModuleName} write Failure.");
                return;
            }

            if (item.IsLightFaultEditable)
            {
                if (!TryParseFloat(item.LightFault, out var lightFault))
                {
                    Growl.Warning("Light Fault is not valid.");
                    return;
                }

                if (!TryWriteFloat(hSock, KHLDevType.DEV_ZF, item.LightFaultAdress, lightFault))
                {
                    Logger.Info($"{item.Workstation}-{item.ModuleName} LightFault write Failure.");
                    Growl.Warning($"{item.Workstation}-{item.ModuleName} write Failure.");
                    return;
                }

                item.LightFault = FormatFloat(lightFault);
                item.LastLightFaultFromPlc = item.LightFault;
            }

            if (item.IsProcessInterlockEditable)
            {
                if (!TryParseFloat(item.ProcessInterlock, out var processInterlock))
                {
                    Growl.Warning("Process I/L is not valid.");
                    return;
                }

                if (!TryWriteFloat(hSock, KHLDevType.DEV_ZF, item.ProcessInterlockAdress, processInterlock))
                {
                    Logger.Info($"{item.Workstation}-{item.ModuleName} ProcessInterlock write Failure.");
                    Growl.Warning($"{item.Workstation}-{item.ModuleName} write Failure.");
                    return;
                }

                item.ProcessInterlock = FormatFloat(processInterlock);
                item.LastProcessInterlockFromPlc = item.ProcessInterlock;
            }

            item.UpdateTime = DateTime.Now.ToString(_dateTimeFormate);
            SaveG3Head();
        }

        private static bool TryParseFloat(string? text, out float value)
        {
            var parsed = float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                || float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
            return parsed && value >= 0;
        }

        private static bool TryWriteFloat(int hSock, KHLDevType device, uint address, float value)
        {
            var buffer = new byte[8];
            KST.FloatToByte(ref buffer, value);
            var error = KHL.KHLWriteDevicesAsWords(hSock, device, address, 2, buffer);
            return error == (int)KHLUserError.KHL_ERR_NONE;
        }

        private void OnUpdate(ConsumableModel model)
        {
            if (model == null) return;
            var SpecifiedConsumable = _consumableModels.Where(x => x.Workstation == model.Workstation && x.ConsumableName == model.ConsumableName).FirstOrDefault();
            if (SpecifiedConsumable == null) return;

            int waferCountSetValue = 0;
            int usedTimeSetValue = 0;
            if (SpecifiedConsumable.IsWaferCountSetEditable
                && !int.TryParse(SpecifiedConsumable.WaferCountSet, out waferCountSetValue))
            {
                Growl.Warning("WaferCountSet is not valid.");
                return;
            }

            if (SpecifiedConsumable.IsUsedTimeSetEditable
                && !int.TryParse(SpecifiedConsumable.UsedTimeSet, out usedTimeSetValue))
            {
                Growl.Warning("UsedTimeSet is not valid.");
                return;
            }

            //Write To PLC：ZF 有符号双字
            int hSock = GetSock(SpecifiedConsumable);
            byte[] writeByte1 = new byte[256];
            byte[] writeByte2 = new byte[256];
            KST.IntToByte(ref writeByte1, waferCountSetValue);
            KST.IntToByte(ref writeByte2, usedTimeSetValue);
            int errWaferWrite = SpecifiedConsumable.IsWaferCountSetEditable ? 
                                KHL.KHLWriteDevicesAsWords(hSock, KHLDevType.DEV_ZF, SpecifiedConsumable.WaferCountAdress, 2, writeByte1) : (int)KHLUserError.KHL_ERR_NONE;
            int errUsedTime = SpecifiedConsumable.IsUsedTimeSetEditable ? 
                                KHL.KHLWriteDevicesAsWords(hSock, KHLDevType.DEV_ZF, SpecifiedConsumable.UsedTimeAdress, 2, writeByte2) : (int)KHLUserError.KHL_ERR_NONE;
            if (errWaferWrite != (int)KHLUserError.KHL_ERR_NONE || errUsedTime != (int)KHLUserError.KHL_ERR_NONE)
            {
                Logger.Info($"{SpecifiedConsumable.Workstation}-{SpecifiedConsumable.ConsumableName} write Failure.");
                Growl.Warning($"{SpecifiedConsumable.Workstation}-{SpecifiedConsumable.ConsumableName} write Failure.");
                return;
            }

            var key = ConsumableKey(SpecifiedConsumable);
            _lastWritten.TryGetValue(key, out var lastWritten);
            var target = $"{SpecifiedConsumable.Workstation}/{SpecifiedConsumable.ConsumableName}";

            if (SpecifiedConsumable.IsWaferCountSetEditable &&
                !string.Equals(lastWritten.WaferCountSet, SpecifiedConsumable.WaferCountSet, StringComparison.Ordinal))
            {
                AuditLogService.WriteChange(
                    AuditModules.Consumable,
                    AuditActions.ConsumableUpdate,
                    $"{target}/WaferCountSet",
                    lastWritten.WaferCountSet,
                    SpecifiedConsumable.WaferCountSet,
                    "Success");
            }

            if (SpecifiedConsumable.IsUsedTimeSetEditable &&
                !string.Equals(lastWritten.UsedTimeSet, SpecifiedConsumable.UsedTimeSet, StringComparison.Ordinal))
            {
                AuditLogService.WriteChange(
                    AuditModules.Consumable,
                    AuditActions.ConsumableUpdate,
                    $"{target}/UsedTimeSet",
                    lastWritten.UsedTimeSet,
                    SpecifiedConsumable.UsedTimeSet,
                    "Success");
            }

            _lastWritten[key] = (SpecifiedConsumable.WaferCountSet, SpecifiedConsumable.UsedTimeSet);

            //Write To Json
            SpecifiedConsumable.UpdateTime = DateTime.Now.ToString(_dateTimeFormate);
            FileUtil.WriteFile(consumableFile, JsonConvert.SerializeObject(_consumableModels, Formatting.Indented));
        }

        private void InitConsumable()
        {
            if (_consumableModels == null) _consumableModels = new ObservableCollection<ConsumableModel>();
            if (!File.Exists(consumableFile)) File.Create(consumableFile).Close();
            var json = FileUtil.ReadFile(consumableFile);
            var list = JsonConvert.DeserializeObject<ObservableCollection<ConsumableModel>>(json);
            if (list != null)
            {
                _consumableModels = list;
            }
            else
            {
                //没有Json文件支撑时，初始化默认的ConsumableModels
                var enumValues = Enum.GetValues(typeof(WorkstationType)).Cast<WorkstationType>();
                foreach (var value in enumValues)
                {
                    if (value == WorkstationType.PolishA ||
                        value == WorkstationType.PolishB ||
                        value == WorkstationType.PolishC ||
                        value == WorkstationType.PolishD)
                    {
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "Unit", WaferCountAdress = 392100 });
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "RetainerRing", WaferCountAdress = 392114, UsedTimeAdress = 392112 });
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "Membrance", WaferCountAdress = 392110, UsedTimeAdress = 392108 });
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "Pad", WaferCountAdress = 392130, UsedTimeAdress = 392128 });
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "Airbag", WaferCountAdress = 392106, UsedTimeAdress = 392104 });
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "Disk_Insitu", UsedTimeAdress = 392192 });
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "Disk_Exsitu", UsedTimeAdress = 392196 });
                    }

                    if (value == WorkstationType.RollerA1 || value == WorkstationType.RollerB1)
                    {
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "Unit", WaferCountAdress = 392050 });
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "LowerRoller", WaferCountAdress = 392054, UsedTimeAdress = 392052 });
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "UpperRoller", WaferCountAdress = 392058, UsedTimeAdress = 392056 });
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "Pillar", WaferCountAdress = 392062, UsedTimeAdress = 392060 });
                    }

                    if (value == WorkstationType.RollerA2 || value == WorkstationType.RollerB2)
                    {
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "Unit", WaferCountAdress = 392150 });
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "LowerRoller", WaferCountAdress = 392154, UsedTimeAdress = 392152 });
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "UpperRoller", WaferCountAdress = 392158, UsedTimeAdress = 392156 });
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "Pillar", WaferCountAdress = 392162, UsedTimeAdress = 392160 });
                    }

                    if (value == WorkstationType.RollerA3 || value == WorkstationType.RollerB3)
                    {
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "Unit", WaferCountAdress = 392250 });
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "LowerRoller", WaferCountAdress = 392254, UsedTimeAdress = 392252 });
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "UpperRoller", WaferCountAdress = 392258, UsedTimeAdress = 392256 });
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "Pillar", WaferCountAdress = 392262, UsedTimeAdress = 392260 });
                    }

                    if (value == WorkstationType.PenA1 || value == WorkstationType.PenB1)
                    {
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "Unit", WaferCountAdress = 392050 });
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "Brush", WaferCountAdress = 392054, UsedTimeAdress = 392052 });
                    } 
                }                                
            }

            if (AlignPenConsumables())
            {
                FileUtil.WriteFile(consumableFile, JsonConvert.SerializeObject(_consumableModels, Formatting.Indented));
            }

            foreach (var model in _consumableModels)
            {
                _lastWritten[ConsumableKey(model)] = (model.WaferCountSet, model.UsedTimeSet);
                if (!int.TryParse(model.UsedTimeSet, out _))
                {
                    model.UsedTimeSet = "0";
                }
                if (!int.TryParse(model.WaferCountSet, out _))
                {
                    model.WaferCountSet = "0";
                }
            }

            foreach (var consumable in _consumableModels)
            {                
                var properties = consumable.GetType().GetProperties();
                var ignoreProperties = _ignoreConsumableProperties.Where(x => x.Workstation == consumable.Workstation &&
                                                                         x.ConsumableName == consumable.ConsumableName)
                                                                  .Select(x => x.IgnoreProperty);
                if (ignoreProperties == null || ignoreProperties.Count() == 0) continue;

                foreach (var ignoreProperty in ignoreProperties)
                {
                    foreach (var property in properties)
                    {
                        if (property.Name == ignoreProperty)
                        {
                            property.SetValue(consumable, string.Empty);
                            break;
                        }
                    }
                }
            }
            UpdateEditableStates();
            OnSelectionChanged("Polish");
        }

        private void UpdateEditableStates()
        {
            if (_consumableModels == null) return;
            foreach (var model in _consumableModels)
            {
                if (model == null) continue;
                model.IsWaferCountSetEditable = !_ignoreConsumableProperties.Any(x =>
                    x.Workstation == model.Workstation &&
                    x.ConsumableName == model.ConsumableName &&
                    x.IgnoreProperty == nameof(ConsumableModel.WaferCountSet));
                model.IsUsedTimeSetEditable = !_ignoreConsumableProperties.Any(x =>
                    x.Workstation == model.Workstation &&
                    x.ConsumableName == model.ConsumableName &&
                    x.IgnoreProperty == nameof(ConsumableModel.UsedTimeSet));
            }
        }

        private static string ConsumableKey(ConsumableModel model)
        {
            return $"{model.Workstation}/{model.ConsumableName}";
        }

        private bool AlignPenConsumables()
        {
            var changed = false;
            foreach (var item in _consumableModels)
            {
                if (!IsPenWorkstation(item.Workstation))
                {
                    continue;
                }

                if (item.ConsumableName == "Pen")
                {
                    item.ConsumableName = "Brush";
                    changed = true;
                }

                if (item.ConsumableName == "Unit")
                {
                    if (item.WaferCountAdress != 392050 || item.UsedTimeAdress != 0)
                    {
                        item.WaferCountAdress = 392050;
                        item.UsedTimeAdress = 0;
                        changed = true;
                    }
                }
                else if (item.ConsumableName == "Brush"
                    && (item.WaferCountAdress != 392054 || item.UsedTimeAdress != 392052))
                {
                    item.WaferCountAdress = 392054;
                    item.UsedTimeAdress = 392052;
                    changed = true;
                }
            }

            return changed;
        }

        private static int GetSock(ConsumableModel model) => GetSock(model.Workstation);

        private static int GetSock(WorkstationType workstation)
        {
            int hSock = 0;
            switch (workstation)
            {
                case WorkstationType.PolishA:
                    hSock = GlobalService.kvHostLinkInit.kvLinkPolishA.sock;
                    break;
                case WorkstationType.PolishB:
                    hSock = GlobalService.kvHostLinkInit.kvLinkPolishB.sock;
                    break;
                case WorkstationType.PolishC:
                    hSock = GlobalService.kvHostLinkInit.kvLinkPolishC.sock;
                    break;
                case WorkstationType.PolishD:
                    hSock = GlobalService.kvHostLinkInit.kvLinkPolishD.sock;
                    break;
                case WorkstationType.RollerA1:
                    hSock = GlobalService.kvHostLinkInit.kvLinkRollerA.sock;
                    break;
                case WorkstationType.RollerA2:
                    hSock = GlobalService.kvHostLinkInit.kvLinkRollerA.sock;
                    break;
                case WorkstationType.RollerA3:
                    hSock = GlobalService.kvHostLinkInit.kvLinkRollerA.sock;
                    break;
                case WorkstationType.RollerB1:
                    hSock = GlobalService.kvHostLinkInit.kvLinkRollerB.sock;
                    break;
                case WorkstationType.RollerB2:
                    hSock = GlobalService.kvHostLinkInit.kvLinkRollerB.sock;
                    break;
                case WorkstationType.RollerB3:
                    hSock = GlobalService.kvHostLinkInit.kvLinkRollerB.sock;
                    break;
                case WorkstationType.PenA1:
                    hSock = GlobalService.kvHostLinkInit.kvLinkPenA.sock;
                    break;
                case WorkstationType.PenB1:
                    hSock = GlobalService.kvHostLinkInit.kvLinkPenB.sock;
                    break;
                default:
                    break;
            }
            return hSock;
        }

        private void InitIgnoreConsumableProperties()
        {
            var enumValues = Enum.GetValues(typeof(WorkstationType)).Cast<WorkstationType>();
            foreach (var value in enumValues)
            {
                if (value == WorkstationType.PolishA ||
                    value == WorkstationType.PolishB ||
                    value == WorkstationType.PolishC ||
                    value == WorkstationType.PolishD)
                {
                    _ignoreConsumableProperties.Add(new() { Workstation = value, ConsumableName = "Unit", IgnoreProperty = "UsedTime" });
                    _ignoreConsumableProperties.Add(new() { Workstation = value, ConsumableName = "Unit", IgnoreProperty = "UsedTimeSet" });
                    _ignoreConsumableProperties.Add(new() { Workstation = value, ConsumableName = "Disk_Insitu", IgnoreProperty = "WaferCount" });
                    _ignoreConsumableProperties.Add(new() { Workstation = value, ConsumableName = "Disk_Insitu", IgnoreProperty = "WaferCountSet" });
                    _ignoreConsumableProperties.Add(new() { Workstation = value, ConsumableName = "Disk_Exsitu", IgnoreProperty = "WaferCount" });
                    _ignoreConsumableProperties.Add(new() { Workstation = value, ConsumableName = "Disk_Exsitu", IgnoreProperty = "WaferCountSet" });
                }

                if (value == WorkstationType.RollerA1 ||
                    value == WorkstationType.RollerA2 ||
                    value == WorkstationType.RollerA3 ||
                    value == WorkstationType.RollerB1 ||
                    value == WorkstationType.RollerB2 ||
                    value == WorkstationType.RollerB3)
                {
                    _ignoreConsumableProperties.Add(new() { Workstation = value, ConsumableName = "Unit", IgnoreProperty = "UsedTime" });
                    _ignoreConsumableProperties.Add(new() { Workstation = value, ConsumableName = "Unit", IgnoreProperty = "UsedTimeSet" });
                }

                if (value == WorkstationType.PenA1 ||
                    value == WorkstationType.PenB1)
                {
                    _ignoreConsumableProperties.Add(new() { Workstation = value, ConsumableName = "Unit", IgnoreProperty = "UsedTime" });
                    _ignoreConsumableProperties.Add(new() { Workstation = value, ConsumableName = "Unit", IgnoreProperty = "UsedTimeSet" });
                }
            }
        }

    }

    public class ConsumableGroup(WorkstationType workstation)
    {
        public WorkstationType Workstation { get; set; } = workstation;
        public ObservableCollection<ConsumableModel> Consumables { get; set; } = [];
    }

    public class IgnoreConsumableProperty
    {
        public WorkstationType Workstation { get; set; }
        public string? ConsumableName { get; set; }
        public string? IgnoreProperty { get; set; }
    }

}
