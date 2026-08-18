using CMP.Services.Auth;
using CMP.Constants.Enums;
using CMP.Models.kvsPlc;
using CMP.Models.Warn;
using CMP.Services.Log;
using CMP.Utils;
using DocumentFormat.OpenXml.EMMA;
using HandyControl.Controls;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using KHL = CMP.Models.kvsPlc.KvHostLink;
using KST = CMP.Models.kvsPlc.Clean.KvStruct;

namespace CMP.ViewModels.Tabs.WarnTab
{

    public class ConsumableTabContentViewModel : BindableBase
    {
        private readonly string _dateTimeFormate = "yyyy-MM-dd HH:mm:ss";
        private readonly string _shortTimeFormate = "HH:mm:ss";

        private readonly string consumableFile = "D:\\CMP\\Configuration\\Consumable\\ConsumableInfo.json";

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


        public DelegateCommand<ConsumableModel> UpdateCommand { get; set; }

        public DelegateCommand<string> SelectionChangedCommand { get; set; }

        private readonly Task _readTask;

        public ConsumableTabContentViewModel()
        {
            _ignoreConsumableProperties = [];
            _consumableModels = new ObservableCollection<ConsumableModel>();
            _consumableGroups = new ObservableCollection<ConsumableGroup>();
            UpdateCommand = new DelegateCommand<ConsumableModel>(OnUpdate);
            SelectionChangedCommand = new DelegateCommand<string>(OnSelectionChanged);
            _readTask = Task.Factory.StartNew(() => { ReadConsumableInfo(); }, TaskCreationOptions.LongRunning);
            InitIgnoreConsumableProperties();
            InitConsumable();
        }

        private async void ReadConsumableInfo()
        {
            while (true)
            {
                await Task.Delay(1000);
                if (_consumableModels == null || _consumableModels.Count == 0) continue;
                //读取PLC地址 设置_consumableModels
                foreach (var item in _consumableModels)
                {
                    if (item == null) continue;
                    if (item.WaferCountAdress == default || item.UsedTimeAdress == default) continue;

                    int hSock = GetSock(item);
                    byte[] readWaferData = new byte[256];
                    byte[] readUsedTimeData = new byte[2560];
                    int errorReadWaferCount = item.IsWaferCountSetEditable ? KHL.KHLReadDevicesAsWords(hSock, KHLDevType.DEV_ZF, item.WaferCountAdress, 1, readWaferData) : (int)KHLUserError.KHL_ERR_NONE;
                    int errorReadUsedTime = item.IsUsedTimeSetEditable ? KHL.KHLReadDevicesAsWords(hSock, KHLDevType.DEV_ZF, item.UsedTimeAdress, 20, readUsedTimeData) : (int)KHLUserError.KHL_ERR_NONE;
                    if (errorReadWaferCount != (int)KHLUserError.KHL_ERR_NONE || errorReadUsedTime != (int)KHLUserError.KHL_ERR_NONE)
                    {
                        Logger.Info($"{item.Workstation}-{item.ConsumableName} read Failure.");
                        continue;
                    }
                    uint count = 0;
                    KST.ByteToUint(ref count, readWaferData);
                    item.WaferCount = count.ToString();

                    byte[] useTimeByte = new byte[1048];
                    KST.ByteToString(ref useTimeByte, readUsedTimeData);
                    string dateString = Encoding.UTF8.GetString(useTimeByte);
                    item.UsedTime = DateTime.TryParse(dateString, out var parsedDate) ? parsedDate.ToString(_shortTimeFormate) : string.Empty;
                }
            }
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
                    var penGroups = _consumableModels.Where(x => x.Workstation == WorkstationType.PenA1 || x.Workstation == WorkstationType.PenA2 || x.Workstation == WorkstationType.PenB1 || x.Workstation == WorkstationType.PenB2)
                        .GroupBy(x => x.Workstation)
                        .Select(group => new ConsumableGroup(group.Key) { Consumables = new ObservableCollection<ConsumableModel>(group) });
                    ConsumableGroups = new ObservableCollection<ConsumableGroup>(penGroups);
                    break;
                default:
                    break;
            }
        }

        private void OnUpdate(ConsumableModel model)
        {
            if (model == null) return;
            var SpecifiedConsumable = _consumableModels.Where(x => x.Workstation == model.Workstation && x.ConsumableName == model.ConsumableName).FirstOrDefault();
            if (SpecifiedConsumable == null) return;

            uint waferCountSetValue = uint.TryParse(SpecifiedConsumable.WaferCountSet, out var waferCount) ? waferCount : 0;
            if (SpecifiedConsumable.IsWaferCountSetEditable && waferCountSetValue <= 0)
            {
                Growl.Warning("WaferCountSet is not valid.");
                return;
            }
            if (SpecifiedConsumable.IsUsedTimeSetEditable && string.IsNullOrEmpty(SpecifiedConsumable.UsedTimeSet))
            {
                Growl.Warning("UsedTimeSet is not valid.");
                return;
            }
            byte[] usedTimeSetValue = Encoding.UTF8.GetBytes(SpecifiedConsumable.UsedTimeSet!);

            //Write To PLC           
            int hSock = GetSock(SpecifiedConsumable);
            byte[] writeByte1 = new byte[256];
            byte[] writeByte2 = new byte[1048];
            KST.UintToByte(ref writeByte1, waferCountSetValue);
            KST.StringToByte(ref writeByte2, usedTimeSetValue);
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
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "RatainerRing", WaferCountAdress = 392114, UsedTimeAdress = 392112 });
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "Membrance", WaferCountAdress = 392110, UsedTimeAdress = 392108 });
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "Pad", WaferCountAdress = 392130, UsedTimeAdress = 392128 });
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "Airbag", WaferCountAdress = 392106, UsedTimeAdress = 392104 });
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "Disk_Insitu", UsedTimeAdress = 392192 });
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "Disk_Exsitu", UsedTimeAdress = 392116 });
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
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "Pen", WaferCountAdress = 392054, UsedTimeAdress = 392052 });
                    }
                    if (value == WorkstationType.PenA2 || value == WorkstationType.PenB2)
                    {
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "Unit", WaferCountAdress = 392150 });
                        _consumableModels.Add(new () { Workstation = value, ConsumableName = "Pen", WaferCountAdress = 392154, UsedTimeAdress = 392152 });
                    }
                }                                
            }

            foreach (var model in _consumableModels)
            {
                _lastWritten[ConsumableKey(model)] = (model.WaferCountSet, model.UsedTimeSet);
                model.UsedTimeSet = DateTime.Now.ToString(_shortTimeFormate);
                model.WaferCountSet = "0";
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

        private static int GetSock(ConsumableModel model)
        {
            int hSock = 0;
            switch (model.Workstation)
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
                case WorkstationType.PenA2:
                    hSock = GlobalService.kvHostLinkInit.kvLinkPenA.sock;
                    break;
                case WorkstationType.PenB1:
                    hSock = GlobalService.kvHostLinkInit.kvLinkPenB.sock;
                    break;
                case WorkstationType.PenB2:
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
                    value == WorkstationType.PenA2 ||
                    value == WorkstationType.PenB1 ||
                    value == WorkstationType.PenB2)
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
