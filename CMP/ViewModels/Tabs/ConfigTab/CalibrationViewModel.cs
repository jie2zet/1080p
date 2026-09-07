using CMP.Services.Auth;
using CMP.Extensions;
using CMP.Models.Config;
using CMP.Models.kvsPlc;
using CMP.Models.Operation.Bot;
using CMP.Models.Recipe.RecipeEdit;
using CMP.Models.Resource;
using CMP.Utils;
using CMP.ViewModels.Common.Dialog;
using CMP.ViewModels.Tabs.RecipeTab;
using CMP.Views.Common.Dialog;
using CMP.Views.Tabs.RecipeTab;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools.Extension;
using Newtonsoft.Json;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KHSTSV = CMP.Models.kvsPlc.SV.KvStruct;
using KHL = CMP.Models.kvsPlc.KvHostLink;
using KST = CMP.Models.kvsPlc.Polish.KvStruct;

namespace CMP.ViewModels.Tabs.ConfigTab
{
    public class CalibrationViewModel : BindableBase, INavigationAware
    {
        private readonly string _calibrationBaseDir = "D:\\CMP\\Configuration\\Configs\\Calibration\\";
        private string _calibrationFileName = "Calibration.json";
        private string _polishIdentifier = "";
        private bool _isInitialized = false;
        private KVLink kvLink = new("");

        /// <summary>
        /// 500000
        /// </summary>
        uint baseAddress = 500000;
        /// <summary>
        /// 100
        /// </summary>
        uint offsetType = 100;
        /// <summary>
        /// 500
        /// </summary>
        uint offsetZone = 500;

        private Dictionary<string, ObservableCollection<CalibrationModel>> _calibModelDictionary;
        private Dictionary<string, List<(float Set, float Act, float Read)>> _savedSnapshot = new();

        public Dictionary<string, ObservableCollection<CalibrationModel>> CalibModelDictionary
        {
            get { return _calibModelDictionary; }
            set { _calibModelDictionary = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<CalibrationDisplayInfo> _calibrationDisplayInfos;

        public ObservableCollection<CalibrationDisplayInfo> CalibrationDisplayInfos
        {
            get { return _calibrationDisplayInfos; }
            set { _calibrationDisplayInfos = value; RaisePropertyChanged(); }
        }


        public DelegateCommand SaveZoneCommand { get; set; }
        public DelegateCommand<string> CopyToZoneCommand { get; set; }
        public DelegateCommand AddDataCommand { get; set; }
        public DelegateCommand<string> AssignCommand { get; set; }


        //对话服务
        private IDialogService _dialogService;

        //事件聚合器
        private IEventAggregator _eventAggregator;

        public CalibrationViewModel(IDialogService dialogService, IEventAggregator eventAggregator)
        {
            _isInitialized = false;
            SaveZoneCommand = new DelegateCommand(OnSaveZone);
            CopyToZoneCommand = new DelegateCommand<string>(OnCopyToZone);
            AddDataCommand = new DelegateCommand(OnAddData);
            AssignCommand = new DelegateCommand<string>(AssignCommandExecute);
            _calibModelDictionary = new Dictionary<string, ObservableCollection<CalibrationModel>>();
            _calibrationDisplayInfos = [];
            _dialogService = dialogService;
            _eventAggregator = eventAggregator;
        }


        private void OnCopyToZone(string viewName)
        {
            ObservableCollection<string> zoneNames = [];
            foreach (var item in _calibModelDictionary.Keys)
            {
                zoneNames.Add(item);
            }
            DialogParameters param = new DialogParameters();
            param.Add("ZoneNames", zoneNames);
            _dialogService.ShowDialog(viewName, param, r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var selectedZoneName = r.Parameters.GetValue<string>("SelectedZoneName");
                    var zoneNames = r.Parameters.GetValue<List<CopyCalibrationModel>>("Result");
                    if (!CalibModelDictionary.Keys.Contains(selectedZoneName)) return;
                    if (zoneNames == null || zoneNames.Count <= 0) return;
                    for (int i = 0; i < zoneNames.Count; i++)
                    {
                        if (zoneNames[i].IsCopy && CalibModelDictionary.Keys.Contains(zoneNames[i].Name!))
                        {
                            var modelCollect = CalibModelDictionary[zoneNames[i].Name!];
                            var sourceModelCollect = CalibModelDictionary[selectedZoneName];
                            for (int j = 0; j < sourceModelCollect.Count; j++)
                            {
                                modelCollect[j].Set = sourceModelCollect[j].Set;
                            }
                        }
                    }
                    Growl.Success("Copy Filished.");
                }
            });
        }

        private void OnAddData()
        {
            if (_calibModelDictionary == null) return;
            foreach (var key in _calibModelDictionary.Keys)
            {
                _calibModelDictionary[key].Add(new CalibrationModel());
            }
        }

        private void InitZoneData()
        {
            var simpleFileName = Path.GetFileNameWithoutExtension(_calibrationFileName);
            if (!File.Exists(_calibrationFileName))
            {
                CalibModelDictionary = GetDefaultZoneDataDic(simpleFileName);
                BuildCalibrationDisplays();
                CaptureCalibrationSnapshot();
                return;
            }
            //if (_isInitialized) return;
            string json = FileUtil.ReadFile(_calibrationFileName);
            var dic = JsonConvert.DeserializeObject<Dictionary<string, ObservableCollection<CalibrationModel>>>(json)!;
            // This is for testing purposes only
            if (dic == null || dic.Count == 0)
            {
                dic = GetDefaultZoneDataDic(simpleFileName);
            }
            dic.Remove("PenA2");
            dic.Remove("PenB2");
            CalibModelDictionary = dic;
            BuildCalibrationDisplays();
            CaptureCalibrationSnapshot();
            _isInitialized = true;
        }

        private void OnSaveZone()
        {
            foreach (var kvp in CalibModelDictionary)
            {
                var models = kvp.Value;
                for (int i = 1; i < models.Count; i++)
                {
                    // 校验 Read 列是否递增，前面的数据不能比后面的数据大
                    if (models[i].Read < models[i - 1].Read)
                    {
                        Growl.Warning(new GrowlInfo()
                        {
                            Message = $"{kvp.Key}, Row: {i + 1}, Read 数据必须递增，前面的数据不能比后面的数据大，请修正后再保存。",
                            ShowCloseButton = true
                        });
                        return;
                    }
                }
            }
            var changeCount = WriteCalibrationDiff(AuditActions.CalibrationSave, "Success");
            var json = JsonConvert.SerializeObject(CalibModelDictionary, Formatting.Indented);
            FileUtil.WriteFile(_calibrationFileName, json);
            CaptureCalibrationSnapshot();
            if (changeCount == 0)
            {
                AuditLogService.WriteChange(
                    AuditModules.Calibration,
                    AuditActions.CalibrationSave,
                    _polishIdentifier,
                    string.Empty,
                    string.Empty,
                    "Success",
                    "No point value changed");
            }
            Growl.Success(new GrowlInfo() { Message = Lang.Resources.SaveSuccessful, ShowCloseButton = true });
        }

        private void AssignCommandExecute(string name)
        {
            float[] arraySet = new float[50];
            float[] arrayAct = new float[50];
            float[] arrayRead = new float[50];
            for (int i = 0; i < arraySet.Length; i++)
            {
                if (i < CalibModelDictionary[name].Count)
                {
                    arraySet[i] = CalibModelDictionary[name][i].Set;
                    arrayAct[i] = CalibModelDictionary[name][i].Act;
                    arrayRead[i] = CalibModelDictionary[name][i].Read;
                }
                else
                {
                    arraySet[i] = 0;
                    arrayRead[i] = 0;
                    arrayAct[i] = 0;
                }
            }

            bool assignSuccess = false;
            switch (name)
            {
                case "Zone1":
                    assignSuccess = AssignCaliValueToPLC(0, baseAddress, arraySet, arrayRead, arrayAct, name);
                    break;
                case "Zone2":
                    assignSuccess = AssignCaliValueToPLC(1, baseAddress, arraySet, arrayRead, arrayAct, name);
                    break;
                case "Zone3":
                    assignSuccess = AssignCaliValueToPLC(2, baseAddress, arraySet, arrayRead, arrayAct, name);
                    break;
                case "Zone4":
                    assignSuccess = AssignCaliValueToPLC(3, baseAddress, arraySet, arrayRead, arrayAct, name);
                    break;
                case "Zone5":
                    assignSuccess = AssignCaliValueToPLC(4, baseAddress, arraySet, arrayRead, arrayAct, name);
                    break;
                case "Zone6":
                    assignSuccess = AssignCaliValueToPLC(5, baseAddress, arraySet, arrayRead, arrayAct, name);
                    break;
                case "Zone7":
                    assignSuccess = AssignCaliValueToPLC(6, baseAddress, arraySet, arrayRead, arrayAct, name);
                    break;
                case "Zone8":
                    assignSuccess = AssignCaliValueToPLC(7, baseAddress, arraySet, arrayRead, arrayAct, name);
                    break;
                case "RRing":
                    assignSuccess = AssignCaliValueToPLC(8, baseAddress, arraySet, arrayRead, arrayAct, name);
                    break;
                case "RollerA1Lower":
                case "RollerB1Lower":
                    assignSuccess = AssignCaliValueToPLC(8, baseAddress, arraySet, arrayRead, arrayAct, name);
                    break;
                case "RollerA1Upper":
                case "RollerB1Upper":
                    assignSuccess = AssignCaliValueToPLC(9, baseAddress, arraySet, arrayRead, arrayAct, name);
                    break;
                case "RollerA2Lower":
                case "RollerB2Lower":
                    assignSuccess = AssignCaliValueToPLC(14, baseAddress, arraySet, arrayRead, arrayAct, name);
                    break;
                case "RollerA2Upper":
                case "RollerB2Upper":
                    assignSuccess = AssignCaliValueToPLC(15, baseAddress, arraySet, arrayRead, arrayAct, name);
                    break;
                case "RollerA3Lower":
                case "RollerB3Lower":
                    assignSuccess = AssignCaliValueToPLC(20, baseAddress, arraySet, arrayRead, arrayAct, name);
                    break;
                case "RollerA3Upper":
                case "RollerB3Upper":
                    assignSuccess = AssignCaliValueToPLC(21, baseAddress, arraySet, arrayRead, arrayAct, name);
                    break;
                case "PenA1":
                    assignSuccess = AssignCaliValueToPLC(8, baseAddress, arraySet, arrayRead, arrayAct, name);
                    break;
                //case "PenA2":
                //    AssignCaliValueToPLC(14, baseAddress, arraySet, arrayRead, arrayAct, name);
                //    break;
                case "PenB1":
                    assignSuccess = AssignCaliValueToPLC(8, baseAddress, arraySet, arrayRead, arrayAct, name);
                    break;
                case "Disk"://Disk
                    uint setBaseAddress = 518000;
                    assignSuccess = AssignCaliValueToPLC(0, setBaseAddress, arraySet, arrayRead, arrayAct, name);
                    break;
            }

            var result = assignSuccess ? "Success" : "Failed";
            WriteCalibrationDiff(AuditActions.CalibrationAssign, result, name);
            AuditLogService.WriteChange(
                AuditModules.Calibration,
                AuditActions.CalibrationAssign,
                $"{_polishIdentifier}/{name}",
                string.Empty,
                string.Empty,
                result,
                "Assign to PLC");
        }

        /// <summary>
        /// 分配校准值到PLC
        /// </summary>
        /// <param name="coefficient">规律系数</param>
        /// <param name="baseAddress">基础地址</param>
        /// <param name="set">设定值数组</param>
        /// <param name="read">反馈值数组</param>
        /// <param name="act">真实值数组</param>
        /// <param name="caliName">校准项目名称</param>
        private bool AssignCaliValueToPLC(uint coefficient, uint baseAddress, float[] set, float[] read, float[] act, string caliName)
        {
            uint setBaseAddress = baseAddress + coefficient * offsetZone;//设定值
            uint actBaseAddress = setBaseAddress + offsetType;//真实值
            uint readBaseAddress = setBaseAddress + 2 * offsetType;//反馈值

            byte[] writeBuf = new byte[256];
            int err;
            var success = true;

            KST.FloatToByte(ref writeBuf, set, 50, 0);
            err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, setBaseAddress, 100, writeBuf);
            if (err == (int)KHLUserError.KHL_ERR_NONE) Growl.InfoGlobal($"Assign {caliName} Cali SetValue Successful");
            else
            {
                success = false;
                Growl.Error($"Assign {caliName} adress:{setBaseAddress} Cali SetValue Error:{err}");
            }

            KST.FloatToByte(ref writeBuf, act, 50, 0);
            err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, actBaseAddress, 100, writeBuf);
            if (err == (int)KHLUserError.KHL_ERR_NONE) Growl.InfoGlobal($"Assign {caliName} Cali ActValue Successful");
            else
            {
                success = false;
                Growl.Error($"Assign {caliName} adress:{actBaseAddress} Cali ActValue Error:{err}");
            }

            KST.FloatToByte(ref writeBuf, read, 50, 0);
            err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, readBaseAddress, 100, writeBuf);
            if (err == (int)KHLUserError.KHL_ERR_NONE) Growl.InfoGlobal($"Assign {caliName} Cali ReadValue Successful");
            else
            {
                success = false;
                Growl.Error($"Assign {caliName} adress:{readBaseAddress} Cali ReadValue Error:{err}");
            }

            return success;
        }

        private Dictionary<string, ObservableCollection<CalibrationModel>> GetDefaultZoneDataDic(string fileName)
        {
            var dic = new Dictionary<string, ObservableCollection<CalibrationModel>>();
            if (fileName.ToLower().StartsWith("rollera"))
            {
                var rollerA1UpperCalibModel = GetDefaultZoneData();
                var rollerA1LowerCalibModel = GetDefaultZoneData();
                var rollerA2UpperCalibModel = GetDefaultZoneData();
                var rollerA2LowerCalibModel = GetDefaultZoneData();
                var rollerA3UpperCalibModel = GetDefaultZoneData();
                var rollerA3LowerCalibModel = GetDefaultZoneData();
                dic.Add("RollerA1Upper", rollerA1UpperCalibModel);
                dic.Add("RollerA1Lower", rollerA1LowerCalibModel);
                dic.Add("RollerA2Upper", rollerA2UpperCalibModel);
                dic.Add("RollerA2Lower", rollerA2LowerCalibModel);
                dic.Add("RollerA3Upper", rollerA3UpperCalibModel);
                dic.Add("RollerA3Lower", rollerA3LowerCalibModel);
            }
            else if (fileName.ToLower().StartsWith("rollerb"))
            {
                var rollerB1UpperCalibModel = GetDefaultZoneData();
                var rollerB1LowerCalibModel = GetDefaultZoneData();
                var rollerB2UpperCalibModel = GetDefaultZoneData();
                var rollerB2LowerCalibModel = GetDefaultZoneData();
                var rollerB3UpperCalibModel = GetDefaultZoneData();
                var rollerB3LowerCalibModel = GetDefaultZoneData();
                dic.Add("RollerB1Upper", rollerB1UpperCalibModel);
                dic.Add("RollerB1Lower", rollerB1LowerCalibModel);
                dic.Add("RollerB2Upper", rollerB2UpperCalibModel);
                dic.Add("RollerB2Lower", rollerB2LowerCalibModel);
                dic.Add("RollerB3Upper", rollerB3UpperCalibModel);
                dic.Add("RollerB3Lower", rollerB3LowerCalibModel);
            }
            else if (fileName.ToLower().StartsWith("pena"))
            {
                var penA1CalibModel = GetDefaultZoneData();
                //var penA2CalibModel = GetDefaultZoneData();
                dic.Add("PenA1", penA1CalibModel);
                //dic.Add("PenA2", penA2CalibModel);
            }
            else if (fileName.ToLower().StartsWith("penb"))
            {
                var penB1CalibModel = GetDefaultZoneData();
                dic.Add("PenB1", penB1CalibModel);

            }
            else
            {
                var zoneCalibModel1 = GetDefaultZoneData();
                var zoneCalibModel2 = GetDefaultZoneData();
                var zoneCalibModel3 = GetDefaultZoneData();
                var zoneCalibModel4 = GetDefaultZoneData();
                var zoneCalibModel5 = GetDefaultZoneData();
                var zoneCalibModel6 = GetDefaultZoneData();
                var zoneCalibModel7 = GetDefaultZoneData();
                var zoneCalibModel8 = GetDefaultZoneData();
                var zoneCalibModel9 = GetDefaultZoneData();
                var zoneCalibModel10 = GetDefaultZoneData();

                dic.Add("Zone1", zoneCalibModel1);
                dic.Add("Zone2", zoneCalibModel2);
                dic.Add("Zone3", zoneCalibModel3);
                dic.Add("Zone4", zoneCalibModel4);
                dic.Add("Zone5", zoneCalibModel5);
                dic.Add("Zone6", zoneCalibModel6);
                dic.Add("Zone7", zoneCalibModel7);
                dic.Add("Zone8", zoneCalibModel8);
                dic.Add("RRing", zoneCalibModel9);
                dic.Add("Disk", zoneCalibModel10);
            }
            return dic;
        }

        private ObservableCollection<CalibrationModel> GetDefaultZoneData()
        {
            var zoneCalibModels = new ObservableCollection<CalibrationModel>() {
                new CalibrationModel()
                {
                    Set = 0,
                    Read = 0,
                    Act = 0
                }
            };
            return zoneCalibModels;
        }

        private void BuildCalibrationDisplays()
        {
            CalibrationDisplayInfos = new ObservableCollection<CalibrationDisplayInfo>(
                _calibModelDictionary.Select(kvp => new CalibrationDisplayInfo
                {
                    Name = kvp.Key,
                    Models = kvp.Value
                })
            );
            if (!_polishIdentifier.StartsWith("Polish")) return;
            new Task(() => {
                KHSTSV.stPolishCollectData stPolishCollectData = new();
                byte[] _readBuf = new byte[2048];
                var err = KHL.KHLReadVariable(kvLink.sock, "dataCollection", _readBuf, true);
                if (err == (int)KHLUserError.KHL_ERR_NONE)
                {
                    KHSTSV.ByteTo_stPolishCollectData(ref stPolishCollectData, _readBuf);
                    //读取Inline和Sensor的值
                    foreach (var displayInfo in CalibrationDisplayInfos)
                    {
                        switch (displayInfo.Name)
                        {
                            case "Zone1":
                                displayInfo.Inline = stPolishCollectData.Zone1PressureAct;
                                displayInfo.Sensor = stPolishCollectData.Zone1PressureSet;
                                break;
                            case "Zone2":
                                displayInfo.Inline = stPolishCollectData.Zone2PressureAct;
                                displayInfo.Sensor = stPolishCollectData.Zone2PressureSet;
                                break;
                            case "Zone3":
                                displayInfo.Inline = stPolishCollectData.Zone3PressureAct;
                                displayInfo.Sensor = stPolishCollectData.Zone3PressureSet;
                                break;
                            case "Zone4":
                                displayInfo.Inline = stPolishCollectData.Zone4PressureAct;
                                displayInfo.Sensor = stPolishCollectData.Zone4PressureSet;
                                break;
                            case "Zone5":
                                displayInfo.Inline = stPolishCollectData.Zone5PressureAct;
                                displayInfo.Sensor = stPolishCollectData.Zone5PressureSet;
                                break;
                            case "Zone6":
                                displayInfo.Inline = stPolishCollectData.Zone6PressureAct;
                                displayInfo.Sensor = stPolishCollectData.Zone6PressureSet;
                                break;
                            case "Zone7":
                                displayInfo.Inline = stPolishCollectData.Zone7PressureAct;
                                displayInfo.Sensor = stPolishCollectData.Zone7PressureSet;
                                break;
                            case "Zone8":
                                displayInfo.Inline = stPolishCollectData.Zone8PressureAct;
                                displayInfo.Sensor = stPolishCollectData.Zone8PressureSet;
                                break;
                            case "RRing":
                                displayInfo.Inline = stPolishCollectData.RetainerRingAirBagPressureAct;
                                displayInfo.Sensor = stPolishCollectData.RetainerRingAirBagPressureSet;
                                break;
                            case "Disk":
                                displayInfo.Inline = stPolishCollectData.ConditionDownForcePressureAct;
                                displayInfo.Sensor = stPolishCollectData.ConditionDownForcePressureSet;
                                break;
                        }
                    }
                }
            }).Start();
            
        }

        private void CaptureCalibrationSnapshot()
        {
            _savedSnapshot = CalibModelDictionary?.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Select(m => (m.Set, m.Act, m.Read)).ToList())
                ?? new Dictionary<string, List<(float Set, float Act, float Read)>>();
        }

        private int WriteCalibrationDiff(string action, string result, string? zoneName = null)
        {
            var count = 0;
            if (CalibModelDictionary == null)
                return count;

            foreach (var kvp in CalibModelDictionary)
            {
                if (!string.IsNullOrEmpty(zoneName) && kvp.Key != zoneName)
                    continue;

                _savedSnapshot.TryGetValue(kvp.Key, out var oldList);
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    var current = kvp.Value[i];
                    var hasOld = oldList != null && i < oldList.Count;
                    var oldSet = hasOld ? oldList![i].Set : (float?)null;
                    var oldAct = hasOld ? oldList![i].Act : (float?)null;
                    var oldRead = hasOld ? oldList![i].Read : (float?)null;

                    count += WriteIfChanged(action, result, kvp.Key, i, "Set", oldSet, current.Set);
                    count += WriteIfChanged(action, result, kvp.Key, i, "Act", oldAct, current.Act);
                    count += WriteIfChanged(action, result, kvp.Key, i, "Read", oldRead, current.Read);
                }
            }

            return count;
        }

        private int WriteIfChanged(string action, string result, string zone, int index, string field, float? oldValue, float newValue)
        {
            if (oldValue.HasValue && oldValue.Value.Equals(newValue))
                return 0;

            AuditLogService.WriteChange(
                AuditModules.Calibration,
                action,
                $"{_polishIdentifier}/{zone}[{index}].{field}",
                oldValue?.ToString() ?? string.Empty,
                newValue.ToString(),
                result);
            return 1;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            // 从导航参数中获取PolishIdentifier
            if (navigationContext.Parameters.TryGetValue("PolishIdentifier", out string? polishIdentifier))
            {
                if (string.IsNullOrEmpty(polishIdentifier)) return;
                _polishIdentifier = polishIdentifier;
                _calibrationFileName = Path.Combine(_calibrationBaseDir, $"{_polishIdentifier}_Calibration.json");
                InitZoneData();
                switch (_polishIdentifier)
                {
                    case "PolishA":
                        kvLink = GlobalService.kvHostLinkInit.kvLinkPolishA;
                        break;
                    case "PolishB":
                        kvLink = GlobalService.kvHostLinkInit.kvLinkPolishB;
                        break;
                    case "PolishC":
                        kvLink = GlobalService.kvHostLinkInit.kvLinkPolishC;
                        break;
                    case "PolishD":
                        kvLink = GlobalService.kvHostLinkInit.kvLinkPolishD;
                        break;
                    case "RollerA":
                        kvLink = GlobalService.kvHostLinkInit.kvLinkRollerA;
                        break;
                    case "RollerB":
                        kvLink = GlobalService.kvHostLinkInit.kvLinkRollerB;
                        break;
                    case "PenA":
                        kvLink = GlobalService.kvHostLinkInit.kvLinkPenA;
                        break;
                    case "PenB":
                        kvLink = GlobalService.kvHostLinkInit.kvLinkPenB;
                        break;
                }
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }

    public class CalibrationDisplayInfo
    {
        public string? Name { get; set; }

        public float Inline { get; set; }

        public float Sensor { get; set; }

        public ObservableCollection<CalibrationModel>? Models { get; set; }
    }
}
