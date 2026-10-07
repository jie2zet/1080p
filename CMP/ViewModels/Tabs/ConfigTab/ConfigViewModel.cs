using CMP.Extensions;
using CMP.Models.Config;
using CMP.Models.Dialog;
using CMP.Models.kvsPlc;
using CMP.Models.Recipe.RecipeEdit;
using CMP.Services.Auth;
using CMP.Services.Log;
using CMP.Utils;
using CMP.ViewModels.Common.Dialog;
using CMP.Views.Common.Dialog;
using Dm.util;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools.Extension;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static CMP.Models.Recipe.RecipeEdit.Recipe;
using KHL = CMP.Models.kvsPlc.KvHostLink;
using KST = CMP.Models.kvsPlc.Clean.KvStruct;

namespace CMP.ViewModels.Tabs.ConfigTab
{
    public class ConfigViewModel : BindableBase
    {
        private readonly string _baseConfigPath = "D:\\CMP\\Configuration\\Configs\\";

        private ConfigModel selectedMode;

        /// <summary>
        /// 当前已选择的一个配置
        /// </summary>
        public ConfigModel SelectedMode
        {
            get => selectedMode; set => SetProperty(ref selectedMode, value);
        }

        private ConfigTreeNode selectedNode;

        public ConfigTreeNode SelectedNode
        {
            get => selectedNode; set => SetProperty(ref selectedNode, value);
        }

        private ConfigSheetModel _selectedSheet;

        /// <summary>
        /// 已选中的SheetName
        /// </summary>
        public ConfigSheetModel SelectedSheet
        {
            get { return _selectedSheet; }
            set 
            { 
                _selectedSheet = value; 
                RaisePropertyChanged();
                if (value != null) _lastSelectedSheetName = value.SheetName;
            }

        }
        /// <summary>
        /// 上一个页面中选中的Sheet名称
        /// </summary>
        private string _lastSelectedSheetName = string.Empty;

        private ObservableCollection<ConfigModel> _configModels;

        /// <summary>
        /// Config集合
        /// </summary>
        public ObservableCollection<ConfigModel> ConfigModels
        {
            get { return _configModels; }
            set { _configModels = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<ConfigTreeNode> _configTreeNodes;

        /// <summary>
        /// 界面上的Node节点
        /// </summary>
        public ObservableCollection<ConfigTreeNode> ConfigTreeNodes
        {
            get { return _configTreeNodes; }
            set { _configTreeNodes = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<ConfigSheetModel> _sheetConfigModels;

        /// <summary>
        /// SheetName关联的Model集合，方便后续根据SheetName进行分组展示
        /// </summary>
        public ObservableCollection<ConfigSheetModel> SheetConfigModels
        {
            get { return _sheetConfigModels; }
            set { _sheetConfigModels = value; RaisePropertyChanged(); }
        }


        private List<string>? configFileNameList;

        private List<int> newValueChangedList;

        /// <summary>
        /// 选中行事件
        /// </summary>
        public DelegateCommand<object> SelecteRowCommand { get; private set; }

        public DelegateCommand<ConfigModel> NewValueChangeCommand { get; private set; }

        public DelegateCommand ApplyCommand { get; private set; }

        public DelegateCommand RestoreDefaultCommand { get; private set; }

        public DelegateCommand SynchronouPlcCommand { get; private set; }

        public DelegateCommand ExportCsvCommand { get; private set; }

        public DelegateCommand<ConfigTreeNode> PolishChangedCommand { get; private set; }

        private readonly IEventAggregator _eventAggregator;

        public ConfigViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            selectedMode = new ConfigModel();
            selectedNode = new ConfigTreeNode();
            _selectedSheet = new ConfigSheetModel();
            _configTreeNodes = [new() { Name = "Unit Params" }, new() { Name = "Application Params" }];
            _configModels = [];
            _sheetConfigModels = [];
            configFileNameList = [];
            newValueChangedList = [];
            SelecteRowCommand = new DelegateCommand<object>(SelectedConfigItem);
            NewValueChangeCommand = new DelegateCommand<ConfigModel>(OnNewValueChangeCommand);
            PolishChangedCommand = new DelegateCommand<ConfigTreeNode>(OnPolishChanged);
            ApplyCommand = new DelegateCommand(OnApplyCommand);
            RestoreDefaultCommand = new DelegateCommand(OnRestoreDefaultCommand);
            SynchronouPlcCommand = new DelegateCommand(OnSynchronouPlc);
            ExportCsvCommand = new DelegateCommand(OnExportCsvCommand);
            InitConfigData();
        }

        private async void OnExportCsvCommand()
        {
            // 1. 弹出保存对话框，获取用户选择的路径和文件名
            var result = await HandyControl.Controls.Dialog.Show<ExportCsvDialogView>()
                .Initialize<ExportCsvDialogViewModel>(vm => { })
                .GetResultAsync<ExportFileModel>();

            if (result == null) return;

            switch (result.SelectedExportWay)
            {
                case ExportWay.All:
                    {
                        if (string.IsNullOrEmpty(result.Directory)) return;
                        bool exportSuccess = true;
                        string exportInfo = "";
                        // 获取 ConfigModel 的所有公共实例属性，按 Sort 排序，过滤 Ignore = true
                        var properties = typeof(ConfigModel).GetProperties()
                            .Where(p => p.CanRead)
                            .Select(p => new { Prop = p, Attr = p.GetCustomAttribute<SortAndIgnoreAttribute>() })
                            .Where(x => x.Attr == null || !x.Attr.Ignore)
                            .OrderBy(x => x.Attr?.Sort)
                            .Select(x => x.Prop)
                            .ToList();

                        foreach (var fileName in configFileNameList!)
                        {
                            string configFile = Path.Combine(_baseConfigPath, $"{fileName}.json");
                            string configModelsJson = FileUtil.ReadFile(configFile);
                            ObservableCollection<ConfigModel> configModels = JsonConvert.DeserializeObject<ObservableCollection<ConfigModel>>(configModelsJson)!;
                            string fullFilePath = Path.Combine(result.Directory, $"{fileName}.csv");

                            var sb = new StringBuilder();
                            sb.AppendLine(string.Join(',', properties.Select(p => p.Name)));

                            foreach (var mode in configModels)
                            {
                                if (mode == null) continue;
                                List<string> row = [];
                                foreach (var property in properties)
                                {
                                    object value = property.GetValue(mode, null)!;
                                    string strRow = FormatCsvField(value);
                                    string escapRow = EscapeCsvField(strRow);
                                    row.add(escapRow);
                                }
                                sb.AppendLine(string.Join(',', row));
                            }

                            try
                            {
                                using FileStream fileStream = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write);
                                using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
                                streamWriter.Write(sb.ToString());
                                
                            }
                            catch (Exception ex)
                            {
                                exportSuccess = false;
                                exportInfo = $"{fullFilePath}" + "\t";
                                Logger.Error($"Eport {fullFilePath} Failed! cause: {ex.Message},{ex.StackTrace}");
                            }
                        }

                        if (exportSuccess) Growl.SuccessGlobal("Eport csv success!");
                        if (exportSuccess)
                        {
                            AuditLogService.WriteCurrentUser(
                                AuditActions.ConfigExport,
                                "Success",
                                result.Directory ?? string.Empty,
                                AuditModules.EC,
                                result.Directory ?? string.Empty);
                        }
                        else Growl.Error($"{exportInfo} Eport Failed!");

                    }
                    break;
                case ExportWay.Single:
                    {
                        if (string.IsNullOrEmpty(result.Directory) || string.IsNullOrEmpty(result.FileName)) return;
                        if (ConfigModels.Count == 0) { Growl.Warning("Please select a configuration item first."); return; }                        

                        string fullFilePath = Path.Combine(result.Directory, $"{result.FileName}.csv");

                        // 获取 ConfigModel 的所有公共实例属性，按 Sort 排序，过滤 Ignore = true
                        var properties = typeof(ConfigModel).GetProperties()
                            .Where(p => p.CanRead)
                            .Select(p => new { Prop = p, Attr = p.GetCustomAttribute<SortAndIgnoreAttribute>() })
                            .Where(x => x.Attr == null || !x.Attr.Ignore)          // 未标记或 Ignore = false 的保留
                            .OrderBy(x => x.Attr?.Sort)            // 按 Sort 升序，未标记的排在最后
                            .Select(x => x.Prop)
                            .ToList();

                        var sb = new StringBuilder();
                        sb.AppendLine(string.Join(',', properties.Select(p => p.Name)));

                        foreach (var mode in ConfigModels)
                        {
                            if (mode == null) continue;
                            List<string> row = [];
                            foreach (var property in properties)
                            {
                                object value = property.GetValue(mode, null)!;
                                string strRow = FormatCsvField(value);
                                string escapRow = EscapeCsvField(strRow);
                                row.add(escapRow);
                            }
                            sb.AppendLine(string.Join(',', row));
                        }

                        try
                        {
                            using FileStream fileStream = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write);
                            using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
                            streamWriter.Write(sb.ToString());
                            Growl.SuccessGlobal("Eport csv success!");
                            AuditLogService.WriteCurrentUser(
                                AuditActions.ConfigExport,
                                "Success",
                                fullFilePath,
                                AuditModules.EC,
                                fullFilePath);
                        }
                        catch (Exception ex)
                        {
                            Growl.Error("Eport csv Failed!");
                            Logger.Error($"Eport csv Failed! cause: {ex.Message},{ex.StackTrace}");
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        
        private void OnSynchronouPlc()
        {
            if (ConfigModels != null && ConfigModels.Count > 0)
            {
                int hSocket = 0;
                if (SelectedNode.Name!.StartsWith("PolishA"))
                {
                    hSocket = GlobalService.kvHostLinkInit.kvLinkPolishA.sock;
                }
                if (SelectedNode.Name.StartsWith("PolishB"))
                {
                    hSocket = GlobalService.kvHostLinkInit.kvLinkPolishB.sock;
                }
                if (SelectedNode.Name.StartsWith("PolishC"))
                {
                    hSocket = GlobalService.kvHostLinkInit.kvLinkPolishC.sock;
                }
                if (SelectedNode.Name.StartsWith("PolishD"))
                {
                    hSocket = GlobalService.kvHostLinkInit.kvLinkPolishD.sock;
                }                
                if (SelectedNode.Name.StartsWith("RollerA") || SelectedNode.Name.StartsWith("RollA"))
                {
                    hSocket = GlobalService.kvHostLinkInit.kvLinkRollerA.sock;
                }
                if (SelectedNode.Name.StartsWith("RollerB") || SelectedNode.Name.StartsWith("RollB"))
                {
                    hSocket = GlobalService.kvHostLinkInit.kvLinkRollerB.sock;
                }
                if (SelectedNode.Name.StartsWith("PenA"))
                {
                    hSocket = GlobalService.kvHostLinkInit.kvLinkPenA.sock;
                }
                if (SelectedNode.Name.StartsWith("PenB"))
                {
                    hSocket = GlobalService.kvHostLinkInit.kvLinkPenB.sock;
                }
                if (SelectedNode.Name.StartsWith("DryA"))
                {
                    hSocket = GlobalService.kvHostLinkInit.kvLinkDryA.sock;
                }
                if (SelectedNode.Name.StartsWith("DryB"))
                {
                    hSocket = GlobalService.kvHostLinkInit.kvLinkDryB.sock;
                }

                var oldValues = ConfigModels.Select(m => m.SetValue ?? string.Empty).ToList();

                foreach (var model in ConfigModels)
                {
                    byte[] readData = new byte[256];
                    if (model.PLCLink.startsWith("ZF"))
                    {
                        uint devTopNo = Convert.ToUInt32(model.PLCLink.Replace("ZF", ""));
                        if (model.Words <= 0 || model.Words >= 3)
                        {
                            model.Words = model.TemplateType switch
                            {
                                ConfigDataType.Enum => 1,
                                ConfigDataType.Int or ConfigDataType.Double => 2,
                                _ => 2,
                            };
                        }
                        int error = KHL.KHLReadDevicesAsWords(hSocket, KHLDevType.DEV_ZF, devTopNo, model.Words, readData);
                        if (error == (int)KHLUserError.KHL_ERR_NONE)
                        {
                            switch (model.TemplateType)
                            {
                                case ConfigDataType.Enum:
                                    {
                                        short val = 0;
                                        KST.ByteToShort(ref val, readData);
                                        model.SetValue = val.toString();
                                    }
                                    break;
                                case ConfigDataType.Int:
                                    {
                                        int val = 0;
                                        KST.ByteToInt(ref val, readData);
                                        model.SetValue = val.toString();
                                    }
                                    break;
                                case ConfigDataType.Double:
                                    {
                                        float val = 0;
                                        KST.ByteToFloat(ref val, readData);
                                        model.SetValue = val.toString();
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    if (model.PLCLink.startsWith("ZR"))
                    {
                        switch (model.TemplateType)
                        {
                            case ConfigDataType.Enum:
                                {
                                    var result = GlobalService.plcComm.ReadShortVari(model.PLCLink);
                                    model.SetValue = result.ToString();
                                }
                                break;
                            case ConfigDataType.Int:
                                {
                                    var result = GlobalService.plcComm.ReadIntVari(model.PLCLink);
                                    model.SetValue = result.IsSuccess ? result.Content.ToString() : model.SetValue;
                                }
                                break;
                            case ConfigDataType.Double:
                                {
                                    var result = GlobalService.plcComm.ReadFloatVari(model.PLCLink);
                                    model.SetValue = result.IsSuccess ? result.Content.ToString() : model.SetValue;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }

                // Save to JSON file
                string polisherJsonFile = $"D:\\CMP\\Configuration\\Configs\\{SelectedNode.Name}.json";
                foreach (var item in ConfigModels)
                {
                    if (item.TemplateType == ConfigDataType.Enum)
                    {
                        foreach (var val in item.Option)
                        {
                            if (val.startsWith(item.SetValue))
                            {
                                item.SetValue = val;
                                break;
                            }
                        }
                        continue;
                    }
                    item.NewValue = "";
                }
                var json = JsonConvert.SerializeObject(ConfigModels, Formatting.Indented);
                FileUtil.WriteFile(polisherJsonFile, json);
                Growl.SuccessGlobal(new GrowlInfo
                {
                    Message = Lang.Resources.SaveSuccessful,
                    ShowCloseButton = true,
                });

                var unitName = SelectedNode?.Name ?? string.Empty;
                for (int i = 0; i < ConfigModels.Count && i < oldValues.Count; i++)
                {
                    var item = ConfigModels[i];
                    var oldValue = oldValues[i];
                    var newValue = item.SetValue ?? string.Empty;
                    if (string.Equals(oldValue, newValue, StringComparison.Ordinal))
                        continue;

                    AuditLogService.WriteChange(
                        AuditModules.EC,
                        AuditActions.ConfigSyncPlc,
                        FormatEcTarget(unitName, item),
                        oldValue,
                        newValue,
                        "Success",
                        item.PLCLink);
                }

                AuditLogService.WriteCurrentUser(
                    AuditActions.ConfigSyncPlc,
                    "Success",
                    unitName,
                    AuditModules.EC,
                    unitName);
            }
        }

        private void OnPolishChanged(ConfigTreeNode node)
        {
            if (string.IsNullOrEmpty(node.Name)) return;
            if (configFileNameList == null || configFileNameList.Count == 0) return;
            // 保存上一个选中的Sheet名称，因为后续Clear/Add过程中WPF TabControl自动选中第一项会覆盖_lastSelectedSheetName
            var lastSheetName = _lastSelectedSheetName;
            if (configFileNameList.Contains(node.Name))
            {
                //ConfigModels = polishDic[node.Name];
                //改变为实时加载文件内容，而非内存数据
                string configFile = Path.Combine(_baseConfigPath, $"{node.Name}.json");
                string configModelsJson = FileUtil.ReadFile(configFile);
                ObservableCollection<ConfigModel> propertyModes = JsonConvert.DeserializeObject<ObservableCollection<ConfigModel>>(configModelsJson)!;
                foreach (var item in propertyModes)
                {
                    if (item.TemplateType == ConfigDataType.Enum)
                    {
                        if (item.Name == "Load Recipe" || item.Name == "UnLoad Recipe" || item.Name == "Dechuck Recipe" ||
                            item.Name == "Load Retry Recipe" || item.Name == "UnLoad Retry Recipe" || item.Name == "Dechuck Retry Recipe" )
                        {
                            item.Option = new List<string>();
                            string folderName = item.Name.Replace("Retry ", "");
                            var list = LoadRecipesFromFolder(folderName);
                            item.Option.AddRange(list);
                        }
                        if (string.IsNullOrEmpty(item.SetValue) || item.SetValue == "" || !item.Option.Contains(item.SetValue))
                        {
                            item.SetValue = item.Option.Count >= 1 ? item.Option[0] : "";
                            item.NewValue = item.Option.Count >= 1 ? item.SetValue : "";
                        }
                        if (!string.IsNullOrEmpty(item.SetValue) && item.Option.Contains(item.SetValue))
                        {
                            item.NewValue = item.SetValue;
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(item.SetValue) || item.SetValue == "") item.SetValue = item.DefaultValue;
                    }
                }
                ConfigModels = propertyModes;

                //将该文件按SheetName进行分组
                SheetConfigModels.Clear();
                var groups = ConfigModels.GroupBy(x => x.SheetName ?? "");
                foreach (var group in groups)
                {
                    SheetConfigModels.Add(new ConfigSheetModel()
                    {
                        SheetName = group.Key,
                        ConfigModes = new ObservableCollection<ConfigModel>(group)
                    });
                }
                //清除已变更的索引
                newValueChangedList.clear();
            }
            SelectedNode = node;
            var val = SheetConfigModels.FirstOrDefault(x => x.SheetName == lastSheetName);
            SelectedSheet =  val! ?? SheetConfigModels.FirstOrDefault()!;
        }

        /// <summary>
        /// Save the modified data to a file
        /// </summary>        
        private void OnApplyCommand()
        {
            ObservableCollection<ConfigModel> configModelsCopy = new();// deep copy
            foreach (var item in ConfigModels)
            {
                ConfigModel mode = new ConfigModel();
                mode.ECID = item.ECID;
                mode.Name = item.Name;
                mode.Unit = item.Unit;
                mode.SetValue = item.SetValue;
                mode.NewValue = item.NewValue;
                mode.Min = item.Min;
                mode.Max = item.Max;
                mode.Class = item.Class;
                mode.TemplateType = item.TemplateType;
                mode.Option = item.Option;
                mode.Discription = item.Discription;
                mode.PLCLink = item.PLCLink;
                configModelsCopy.Add(mode);
            }
            List<int> writeFailureModels = new List<int>();
            var pendingChanges = newValueChangedList
                .Where(index => index >= 0 && index < ConfigModels.Count && index < configModelsCopy.Count)
                .Select(index => (
                    Index: index,
                    Item: ConfigModels[index],
                    OldValue: configModelsCopy[index].SetValue ?? string.Empty,
                    NewValue: ConfigModels[index].NewValue ?? string.Empty))
                .ToList();

            foreach (var index in newValueChangedList)
            {
                ConfigModels[index].SetValue = ConfigModels[index].NewValue;
            }

            //Write to PLC
            PolishConfigWriteToPlc(out writeFailureModels);
            ScheduleConfigWriteToPlc(out writeFailureModels);
            if (writeFailureModels.Count > 0)
            {
                foreach (var index in writeFailureModels)
                {
                    ConfigModels[index].SetValue = configModelsCopy[index].SetValue; //If the changed value is written incorrectly, it will revert to the original value
                    Growl.Warning($"Failed to Write {ConfigModels[index].Class}:{ConfigModels[index].Name} to PLC");
                }
            }
            // Save to JSON file
            string polisherJsonFile = $"D:\\CMP\\Configuration\\Configs\\{SelectedNode.Name}.json";
            foreach (var item in ConfigModels)
            {
                if (item.TemplateType == ConfigDataType.Enum)
                {
                    foreach (var val in item.Option)
                    {
                        if (val.startsWith(item.SetValue))
                        {
                            item.SetValue = val;
                            break;
                        }
                    }
                    continue;
                }
                item.NewValue = "";
            }
            var json = JsonConvert.SerializeObject(ConfigModels, Formatting.Indented);
            FileUtil.WriteFile(polisherJsonFile, json);
            Growl.SuccessGlobal(new GrowlInfo
            {
                Message = Lang.Resources.SaveSuccessful,
                ShowCloseButton = true,
            });

            var unitName = SelectedNode?.Name ?? string.Empty;
            foreach (var change in pendingChanges)
            {
                if (string.Equals(change.OldValue, change.NewValue, StringComparison.Ordinal))
                    continue;

                var failed = writeFailureModels.Contains(change.Index);
                AuditLogService.WriteChange(
                    AuditModules.EC,
                    AuditActions.ConfigApply,
                    FormatEcTarget(unitName, change.Item),
                    change.OldValue,
                    change.NewValue,
                    failed ? "PlcFailed" : "Success",
                    change.Item.PLCLink);
            }

            AuditLogService.WriteCurrentUser(
                AuditActions.ConfigApply,
                "Success",
                unitName,
                AuditModules.EC,
                unitName);

            newValueChangedList.Clear();
            newValueChangedList = new List<int>();

            #region 保存时，并将配置参数中Recipe相关部分发给送工位
            foreach (var item in ConfigModels)
            {
                if (item.TemplateType == ConfigDataType.Enum && item.Name == "Load Recipe")
                {
                    _eventAggregator.GetEvent<GetRecipeConfigEvent>().Publish(new GetRecipeConfigEventArgs()
                    {
                        RecipeType = item.Name,
                        RecipeName = item.SetValue,
                        ConfigFileName = SelectedNode!.Name,
                        IsNeedTip = true,
                    });
                }
                if (item.TemplateType == ConfigDataType.Enum && item.Name == "Dechuck Recipe")
                {
                    _eventAggregator.GetEvent<GetRecipeConfigEvent>().Publish(new GetRecipeConfigEventArgs()
                    {
                        RecipeType = item.Name,
                        RecipeName = item.SetValue,
                        ConfigFileName = SelectedNode!.Name,
                        IsNeedTip = true,
                    });
                }
                if (item.TemplateType == ConfigDataType.Enum && item.Name == "UnLoad Recipe")
                {
                    _eventAggregator.GetEvent<GetRecipeConfigEvent>().Publish(new GetRecipeConfigEventArgs()
                    {
                        RecipeType = item.Name,
                        RecipeName = item.SetValue,
                        ConfigFileName = SelectedNode!.Name,
                        IsNeedTip = true,
                    });
                }
                if (item.TemplateType == ConfigDataType.Enum && item.Name == "Load Retry Recipe")
                {
                    _eventAggregator.GetEvent<GetRecipeConfigEvent>().Publish(new GetRecipeConfigEventArgs()
                    {
                        RecipeType = item.Name,
                        RecipeName = item.SetValue,
                        ConfigFileName = SelectedNode!.Name,
                        IsNeedTip = true,
                    });
                }
                if (item.TemplateType == ConfigDataType.Enum && item.Name == "Dechuck Retry Recipe")
                {
                    _eventAggregator.GetEvent<GetRecipeConfigEvent>().Publish(new GetRecipeConfigEventArgs()
                    {
                        RecipeType = item.Name,
                        RecipeName = item.SetValue,
                        ConfigFileName = SelectedNode!.Name,
                        IsNeedTip = true,
                    });
                }
                if (item.TemplateType == ConfigDataType.Enum && item.Name == "UnLoad Retry Recipe")
                {
                    _eventAggregator.GetEvent<GetRecipeConfigEvent>().Publish(new GetRecipeConfigEventArgs()
                    {
                        RecipeType = item.Name,
                        RecipeName = item.SetValue,
                        ConfigFileName = SelectedNode!.Name,
                        IsNeedTip = true,
                    });
                }
            }
            #endregion
        }

        private void OnRestoreDefaultCommand()
        {
            if (SelectedSheet?.ConfigModes == null)
                return;

            foreach (var model in SelectedSheet.ConfigModes)
            {
                if (model == null || string.IsNullOrWhiteSpace(model.DefaultValue))
                    continue;

                int index = ConfigModels.IndexOf(model);
                if (index < 0 || !TryGetRestoreDefaultValue(model, out var defaultValue))
                    continue;

                model.NewValue = defaultValue;
                if (!newValueChangedList.Contains(index))
                    newValueChangedList.Add(index);
            }
        }

        private static bool TryGetRestoreDefaultValue(ConfigModel model, out string defaultValue)
        {
            defaultValue = string.Empty;

            switch (model.TemplateType)
            {
                case ConfigDataType.Enum:
                    if (!short.TryParse(model.DefaultValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out short defaultCode) ||
                        model.Option == null || model.Option.Count == 0)
                    {
                        return false;
                    }

                    foreach (var option in model.Option)
                    {
                        if (string.IsNullOrWhiteSpace(option) || !TryGetEnumOptionCode(option, out short optionCode))
                            continue;

                        if (optionCode == defaultCode)
                        {
                            defaultValue = option;
                            return true;
                        }
                    }

                    return false;

                case ConfigDataType.Int:
                case ConfigDataType.Double:
                    // Restore-default only accepts a finite numeric value. The configured
                    // TemplateType is not used to reject a numeric default, because some
                    // existing configuration rows are labeled Int but contain decimals.
                    if (!double.TryParse(model.DefaultValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double numericValue) ||
                        !double.IsFinite(numericValue))
                    {
                        return false;
                    }

                    // The original type-specific validation is intentionally retained here:
                    // if (model.TemplateType == ConfigDataType.Int &&
                    //     !int.TryParse(model.DefaultValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                    //     return false;
                    // if (model.TemplateType == ConfigDataType.Double &&
                    //     !double.TryParse(model.DefaultValue, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                    //     return false;

                    // if (!IsWithinConfiguredRange(doubleValue, model.Min, model.Max))
                    //     return false;

                    defaultValue = numericValue.ToString(CultureInfo.CurrentCulture);
                    return true;

                default:
                    return false;
            }
        }

        private static bool TryGetEnumOptionCode(string option, out short code)
        {
            string normalizedOption = option.Replace('：', ':');
            int separatorIndex = normalizedOption.IndexOf(':');
            string codeText = separatorIndex >= 0
                ? normalizedOption[..separatorIndex].Trim()
                : normalizedOption.Trim();

            return short.TryParse(codeText, NumberStyles.Integer, CultureInfo.InvariantCulture, out code);
        }

        /*
        private static bool IsWithinConfiguredRange(double value, string? minText, string? maxText)
        {
            if (!string.IsNullOrWhiteSpace(minText))
            {
                if (!double.TryParse(minText, NumberStyles.Float, CultureInfo.InvariantCulture, out double min) ||
                    !double.IsFinite(min) || value < min)
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(maxText))
            {
                if (!double.TryParse(maxText, NumberStyles.Float, CultureInfo.InvariantCulture, out double max) ||
                    !double.IsFinite(max) || value > max)
                {
                    return false;
                }
            }

            return true;
        }
        */

        private static string FormatEcTarget(string unitName, ConfigModel item)
        {
            return $"{unitName}/ECID:{item.ECID}/{item.Class}/{item.Name}";
        }

        private void OnNewValueChangeCommand(ConfigModel model)
        {
            if (model == null) return;

            switch (model.TemplateType)
            {
                case ConfigDataType.Int:
                    int min = int.TryParse(model.Min, out min) ? min : 0;
                    int max = int.TryParse(model.Max, out max) ? max : 0;
                    int newValueInt = int.TryParse(model.NewValue, out newValueInt) ? newValueInt : 0;
                    if (newValueInt > max || newValueInt < min)
                    {
                        model.NewValue = model.SetValue;
                        return;
                    }
                    break;
                case ConfigDataType.Double:
                    double minD = double.TryParse(model.Min, out minD) ? minD : 0;
                    double maxD = double.TryParse(model.Max, out maxD) ? maxD : 0;
                    double newValueD = double.TryParse(model.NewValue, out newValueD) ? newValueD : 0;
                    if (newValueD > maxD || newValueD < minD)
                    {
                        model.NewValue = model.SetValue;
                        return;
                    }
                    break;
                default:
                    break;
            }
            
            int index = ConfigModels.IndexOf(model);
            if (index >= 0 && !newValueChangedList.Contains(index))
            {
                newValueChangedList.Add(index);
            }
        }

        private void SelectedConfigItem(object model)
        {
            if (model == null) return;
            if (model is not ConfigModel configModel) return;
            SelectedMode = configModel;
        }

        private void InitConfigData()
        {
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(_baseConfigPath);
                FileInfo[] configFiles = directoryInfo.GetFiles("*.json");
                _configTreeNodes[0].Children = new ObservableCollection<ConfigTreeNode>();
                foreach (FileInfo configFile in configFiles)
                {
                    string configName = Path.GetFileNameWithoutExtension(configFile.Name);
                    if (configName.StartsWith("PenA2", StringComparison.OrdinalIgnoreCase) ||
                        configName.StartsWith("PenB2", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    _configTreeNodes[0].Children?.Add(new ConfigTreeNode() { Name = configName });
                    configFileNameList!.Add(configName);

                    #region 初始化时读取配置，并将参数发给送工位
                    string configModelsJson = FileUtil.ReadFile(configFile.FullName);
                    ObservableCollection<ConfigModel> configModes = JsonConvert.DeserializeObject<ObservableCollection<ConfigModel>>(configModelsJson)!;
                    foreach (var item in configModes)
                    {
                        if (item.TemplateType == ConfigDataType.Enum)
                        {
                            if (item.Name == "Load Recipe" || item.Name == "UnLoad Recipe" || item.Name == "Dechuck Recipe" ||
                                item.Name == "Load Retry Recipe" || item.Name == "UnLoad Retry Recipe" || item.Name == "Dechuck Retry Recipe")
                            {
                                item.Option = new List<string>();
                                string folderName = item.Name.Replace("Retry ", "");
                                var list = LoadRecipesFromFolder(folderName);
                                item.Option.AddRange(list);
                                if (item.Option.Count > 0)
                                {
                                    if (!item.Option.Contains(item.SetValue))
                                    {
                                        item.SetValue = item.Option[0];
                                    }
                                }
                                else
                                {
                                    item.SetValue = item.DefaultValue;
                                }                                

                                _eventAggregator.GetEvent<GetRecipeConfigEvent>().Publish(new GetRecipeConfigEventArgs()
                                {
                                    RecipeType = item.Name,
                                    RecipeName = item.SetValue,
                                    ConfigFileName = configName,
                                    IsNeedTip = false,
                                });
                            }

                            if (!string.IsNullOrEmpty(item.SetValue) && item.Option.Contains(item.SetValue))
                            {
                                item.NewValue = item.SetValue;
                            }
                        }
                    }
                    #endregion

                }
            }
            catch (Exception ex)
            {
                Growl.Info($"加载配置时出错：{ex.Message}");
                Logger.Error($"加载配置时出错：{ex.Message},{ex.StackTrace}");
            }
        }

        private List<string> LoadRecipesFromFolder(string folderName)
        {
            try
            {
                List<string> recipeList = new List<string>();
                string folderPath = Path.Combine("D:\\CMP\\Configuration\\Recipes\\UnitRecipe\\", folderName);

                if (!Directory.Exists(folderPath))
                {
                    return null;
                }

                DirectoryInfo folderInfo = new DirectoryInfo(folderPath);
                var jsonFiles = folderInfo.GetFiles("*.json");

                foreach (var file in jsonFiles)
                {
                    try
                    {
                        var json = FileUtil.ReadFile(file.FullName);
                        Recipe.Content? content = JsonConvert.DeserializeObject<Recipe.Content>(json);

                        if (content != null && !string.IsNullOrEmpty(content.Name))
                        {
                            if (!recipeList.Contains(content.Name))
                            {
                                recipeList.Add(content.Name);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue processing other files
                        Console.WriteLine($"Error loading recipe from {file.FullName}: {ex.Message}");
                    }
                }
                return recipeList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading recipes from folder {folderName}: {ex.Message}");
            }
            return null;
        }

        private void PolishConfigWriteToPlc(out List<int> writeFailureModels)
        {
            writeFailureModels = new List<int>();
            bool allowWriteToPlc = !string.IsNullOrEmpty(SelectedNode.Name);
            if (!allowWriteToPlc) return;
            int hSocket = 0;
            int err = 0;
            if (SelectedNode.Name!.StartsWith("PolishA")) hSocket = GlobalService.kvHostLinkInit.kvLinkPolishA.sock;

            if (SelectedNode.Name.StartsWith("PolishB")) hSocket = GlobalService.kvHostLinkInit.kvLinkPolishB.sock;

            if (SelectedNode.Name.StartsWith("PolishC")) hSocket = GlobalService.kvHostLinkInit.kvLinkPolishC.sock;

            if (SelectedNode.Name.StartsWith("PolishD")) hSocket = GlobalService.kvHostLinkInit.kvLinkPolishC.sock;

            if (SelectedNode.Name.StartsWith("RollerA") || SelectedNode.Name.StartsWith("RollA")) hSocket = GlobalService.kvHostLinkInit.kvLinkRollerA.sock;
    
            if (SelectedNode.Name.StartsWith("RollerB") || SelectedNode.Name.StartsWith("RollB")) hSocket = GlobalService.kvHostLinkInit.kvLinkRollerB.sock;

            if (SelectedNode.Name.StartsWith("PenA")) hSocket = GlobalService.kvHostLinkInit.kvLinkPenA.sock;

            if (SelectedNode.Name.StartsWith("PenB")) hSocket = GlobalService.kvHostLinkInit.kvLinkPenB.sock;

            if (SelectedNode.Name.StartsWith("DryA")) hSocket = GlobalService.kvHostLinkInit.kvLinkDryA.sock;

            if (SelectedNode.Name.StartsWith("DryB")) hSocket = GlobalService.kvHostLinkInit.kvLinkDryA.sock;

            foreach (var index in newValueChangedList)
            {
                var model = ConfigModels[index];
                var plclink = model.PLCLink;
                if (!plclink.StartsWith("ZF")) continue;
                
                byte[] data = new byte[256];
                uint devTopNo = Convert.ToUInt32(model.PLCLink.Replace("ZF", ""));
                switch (model.TemplateType)
                {
                    case ConfigDataType.Enum:
                        {
                            string[] strArr = model.SetValue.Replace(" ", "").Replace("：", ":").split(":");//解析字符串
                            if (!short.TryParse(strArr[0], out short val))
                            {
                                HandyControl.Controls.MessageBox.Show($"Invalid setValue for {model.Name}. Please enter a valid number.");
                                err = -1;
                                break;
                            }
                            KST.ShortToByte(ref data, val);
                        }
                        break;
                    case ConfigDataType.Int:
                        {
                            if (!int.TryParse(model.SetValue, out int val))
                            {
                                HandyControl.Controls.MessageBox.Show($"Invalid setValue for {model.Name}. Please enter a valid number.");
                                err = -1;
                                break;
                            }
                            KST.IntToByte(ref data, val);
                        }
                        break;
                    case ConfigDataType.Double:
                        {
                            if (!float.TryParse(model.SetValue, out float val))
                            {
                                HandyControl.Controls.MessageBox.Show($"Invalid setValue for {model.Name}. Please enter a valid number.");
                                err = -1;
                                break;
                            }
                            KST.FloatToByte(ref data, val);
                        }
                        break;
                    default:
                        break;
                }
                if (model.Words <= 0 || model.Words >= 3)
                {
                    model.Words = model.TemplateType switch
                    {
                        ConfigDataType.Enum => 1,
                        ConfigDataType.Int or ConfigDataType.Double => 2,
                        _ => 2,
                    };
                }
                err = KHL.KHLWriteDevicesAsWords(hSocket, KHLDevType.DEV_ZF, devTopNo, model.Words, data);
                if (err != (int)KHLUserError.KHL_ERR_NONE)
                {
                    writeFailureModels.Add(index);
                }
            }
        }

        private void ScheduleConfigWriteToPlc(out List<int> writeFailureModels)
        {
            writeFailureModels = new List<int>();
            if (string.IsNullOrEmpty(SelectedNode.Name)) return;
            if (!SelectedNode.Name.StartsWith("Schedule", StringComparison.OrdinalIgnoreCase)) return;
            int err = 0;

            foreach (var index in newValueChangedList)
            {
                var model = ConfigModels[index];
                
                switch (model.TemplateType)
                {
                    case ConfigDataType.Enum:
                        {
                            string[] strArr = model.SetValue.Replace(" ", "").Replace("：", ":").split(":");//解析字符串
                            if (!short.TryParse(strArr[0], out short val))
                            {
                                HandyControl.Controls.MessageBox.Show($"Invalid setValue for {model.Name}. Please enter a valid number.");
                                err = -1;
                                break;
                            }
                            GlobalService.plcComm.WriteShortVari(model.PLCLink, val);
                        }
                        break;
                    case ConfigDataType.Int:
                        {
                            if (!int.TryParse(model.SetValue, out int val))
                            {
                                HandyControl.Controls.MessageBox.Show($"Invalid setValue for {model.Name}. Please enter a valid number.");
                                err = -1;
                                break;
                            }
                            GlobalService.plcComm.WriteIntVari(model.PLCLink, val);
                        }
                        break;
                    case ConfigDataType.Double:
                        {
                            if (!float.TryParse(model.SetValue, out float val))
                            {
                                HandyControl.Controls.MessageBox.Show($"Invalid setValue for {model.Name}. Please enter a valid number.");
                                err = -1;
                                break;
                            }
                            GlobalService.plcComm.WriteFloatVari(model.PLCLink, val);
                        }
                        break;
                    default: break;
                }

                if (err != 0) writeFailureModels.Add(index);

            }
        }

        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;

            return field.replace(",", " ");
        }

        private static string FormatCsvField(object value)
        {
            if (value == null)
                return string.Empty;

            if (value is List<string> list)
            {
                return string.Join(";", list);
            }

            if (value is Enum enumVal)
            {
                return enumVal.ToString(); // 输出枚举名称
            }

            return value.ToString()!;
        }
    }

    public class ConfigTemplateSelector : DataTemplateSelector
{
    public DataTemplate ComboBoxTemplate { get; set; }
    public DataTemplate TextBoxTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is ConfigModel config)
        {
            // 根据TemplateType选择模板
            return config.TemplateType == ConfigDataType.Enum
                ? ComboBoxTemplate
                : TextBoxTemplate;
        }

        return base.SelectTemplate(item, container);
    }
}

/// <summary>
/// 用于同步ComboBox选择到NewValue的附加行为
/// </summary>
public class ComboBoxSyncBehavior : DependencyObject
{
    // 声明同步命令依赖属性
    public static readonly DependencyProperty SyncCommandProperty =
        DependencyProperty.RegisterAttached(
            "SyncCommand",
            typeof(DelegateCommand<ConfigModel>),
            typeof(ComboBoxSyncBehavior),
            new PropertyMetadata(null, OnSyncCommandChanged));

    // 设置命令
    public static void SetSyncCommand(DependencyObject obj, DelegateCommand<ConfigModel> value)
    {
        obj.SetValue(SyncCommandProperty, value);
    }

    // 获取命令
    public static DelegateCommand<ConfigModel> GetSyncCommand(DependencyObject obj)
    {
        return (DelegateCommand<ConfigModel>)obj.GetValue(SyncCommandProperty);
    }

    // 命令变化时绑定事件
    private static void OnSyncCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is System.Windows.Controls.ComboBox comboBox)
        {
            comboBox.SelectionChanged -= ComboBox_SelectionChanged;
            if (e.NewValue is DelegateCommand<ConfigModel> command)
            {
                comboBox.SelectionChanged += ComboBox_SelectionChanged;
            }
        }

        if (d is System.Windows.Controls.TextBox textBox)
        {
            textBox.TextChanged -= TextBox_TextChanged;
            if (e.NewValue is DelegateCommand<ConfigModel> command)
            {
                textBox.TextChanged += TextBox_TextChanged;
            }
        }
    }

    private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBox textBox &&
            textBox.DataContext is ConfigModel config &&
            textBox.IsFocused &&
            GetSyncCommand(textBox) is DelegateCommand<ConfigModel> command)
            {
                if (string.IsNullOrEmpty(config.NewValue))
                {
                    return;
                }
                if (config.NewValue == "-")
                {
                    return;
                }
                if (config.TemplateType == ConfigDataType.Int && !int.TryParse(config.NewValue, out int i))
                {
                    config.NewValue = config.DefaultValue;
                    return;
                }
                if (config.TemplateType == ConfigDataType.Double && !double.TryParse(config.NewValue, out double d))
                {
                    config.NewValue = config.DefaultValue;
                    return;
                }
                if (double.TryParse(config.Min, out double min) && min > 0 && config.NewValue.Length < config.Min.Length)
                {
                    return;
                }
                // 执行命令并传递当前数据模型
                command.Execute(config);
            }
        }

    // 处理选择变化事件
    private static void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is System.Windows.Controls.ComboBox comboBox &&
            comboBox.DataContext is ConfigModel config &&
            comboBox.IsMouseCaptured &&
            GetSyncCommand(comboBox) is DelegateCommand<ConfigModel> command)
        {
            // 执行命令并传递当前数据模型
            command.Execute(config);
        }
    }
}
}
