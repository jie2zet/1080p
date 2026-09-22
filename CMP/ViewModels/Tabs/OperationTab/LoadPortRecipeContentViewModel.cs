using CMP.Models.kvsPlc;
using CMP.Models.Operation.LoadPort;
using CMP.Models.Recipe.RecipeEdit;
using CMP.Models.Recipe.Sequence;
using CMP.Models.Resource;
using CMP.Services;
using CMP.Services.Auth;
using CMP.Services.LoadPort;
using CMP.Services.Log;
using CMP.Services.Plc;
using CMP.Services.Recipe;
using CMP.Utils;
using CMP.Validation;
using CMP.ViewModels.Tabs.RecipeTab.Content;
using DocumentFormat.OpenXml.Wordprocessing;
using HandyControl.Controls;
using Newtonsoft.Json;
using Prism.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using KHL = CMP.Models.kvsPlc.KvHostLink;
using KST = CMP.Models.kvsPlc.Clean.KvStruct;

namespace CMP.ViewModels.Tabs.OperationTab
{
    public class LoadPortRecipeContentViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private static readonly object _globalLock = new object();
        private static List<Wafer> _tempOrderedWafers = new List<Wafer>();
        private LoadPortData _loadPortData;
        private string _selectedRecipe;
        private ObservableCollection<Wafer> _waferList;
        private string _statusMessage = "Ready to assign recipe";
        private int _selectedWaferCount = 0;
        private string _batchWaferName = string.Empty;

        private SequenceRecipeData? _selectedSequence;

        // 不再使用单一实例，而是在需要时创建指定端口的实例
        private SquenceWithPLC sequenceWithPLC = new();

        private MitsubishiPlcService plcComm = MitsubishiPlcService.Main;
        private static readonly SemaphoreSlim PlcInitGate = new(1, 1);
        //Dictionary<int, MitsubishiPlcService> plcComms = MitsubishiPlcService.GetAllConnectedPortServices();

        public SequenceRecipeData? SelectedSequence
        {
            get => _selectedSequence;
            set
            {
                SetProperty(ref _selectedSequence, value);
                // 显式通知命令重新评估状态
                ((DelegateCommand)AssignRecipeCommand)?.RaiseCanExecuteChanged();
                // 同步到 SequenceEditorViewModel
                if (_sequenceEditorViewModel != null && value != null)
                {
                    _sequenceEditorViewModel.SelectedSequence = value;
                }
            }
        }


        private void InitializeEditorViewModel()
        {
            _sequenceEditorViewModel = new SequenceRecipeEditorViewModel(_eventAggregator);
            // 监听 SelectedSequence 的变化
            _sequenceEditorViewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(SequenceRecipeEditorViewModel.SelectedSequence))
                {
                    SelectedSequence = _sequenceEditorViewModel.SelectedSequence;
                }
            };
        }

        // 在 LoadPortRecipeContentViewModel 中
        private SequenceRecipeEditorViewModel _sequenceEditorViewModel;
        public SequenceRecipeEditorViewModel SequenceEditorViewModel
        {
            get => _sequenceEditorViewModel;
            set
            {
                if (_sequenceEditorViewModel != null)
                {
                    _sequenceEditorViewModel.PropertyChanged -= SequenceEditorViewModel_PropertyChanged;
                }

                SetProperty(ref _sequenceEditorViewModel, value);

                if (_sequenceEditorViewModel != null)
                {
                    _sequenceEditorViewModel.PropertyChanged += SequenceEditorViewModel_PropertyChanged;
                    SelectedSequence = _sequenceEditorViewModel.SelectedSequence;
                }
            }
        }

        private void SequenceEditorViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SequenceRecipeEditorViewModel.SelectedSequence))
            {
                SelectedSequence = _sequenceEditorViewModel.SelectedSequence;
                // 显式通知命令重新评估状态
                ((DelegateCommand)AssignRecipeCommand)?.RaiseCanExecuteChanged();
            }
        }
        // 添加关闭窗口的回调Action
        public Action<bool?> CloseWindowAction { get; set; }

        // Commands
        public ICommand SelectAllCommand { get; private set; }
        public ICommand ClearSelectionCommand { get; private set; }
        public ICommand AssignRecipeCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public ObservableCollection<Wafer> WaferList
        {
            get => _waferList;
            set => SetProperty(ref _waferList, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public int SelectedWaferCount
        {
            get => _selectedWaferCount;
            set => SetProperty(ref _selectedWaferCount, value);
        }

        public string BatchWaferName
        {
            get => _batchWaferName;
            set => SetProperty(ref _batchWaferName, value);
        }

        public LoadPortRecipeContentViewModel(LoadPortData loadPortData, string recipe, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _loadPortData = loadPortData;
            _selectedRecipe = recipe;
            _waferList = loadPortData.WaferList;
            // 初始化命令
            InitializeCommands();
            InitializeEditorViewModel();


            // 初始化选中 Wafer 数量
            UpdateSelectedWaferCount();

            // 订阅 WaferList 的变化以更新选中数量
            if (_waferList is ObservableCollection<Wafer> observableList)
            {
                // 订阅集合变化
                observableList.CollectionChanged += OnWaferListCollectionChanged;

                // 订阅现有项的属性变化
                SubscribeToWaferPropertyChanges();
            }
        }

        private void OnWaferListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // 取消订阅旧项的属性变化
            if (e.OldItems != null)
            {
                foreach (Wafer item in e.OldItems)
                {
                    item.PropertyChanged -= OnWaferPropertyChanged;
                }
            }

            // 订阅新项的属性变化
            if (e.NewItems != null)
            {
                foreach (Wafer item in e.NewItems)
                {
                    item.PropertyChanged += OnWaferPropertyChanged;
                }
            }

            UpdateSelectedWaferCount();

        }

        private void SubscribeToWaferPropertyChanges()
        {
            foreach (var wafer in _waferList)
            {
                wafer.PropertyChanged += OnWaferPropertyChanged;
            }
        }

        private void OnWaferPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // 当Wafer的IsSelected属性更改时更新UI
            if (e.PropertyName == nameof(Wafer.IsSelected))
            {
                UpdateSelectedWaferCount();
                // 显式通知命令重新评估状态
                ((DelegateCommand)AssignRecipeCommand)?.RaiseCanExecuteChanged();
            }
            if (e.PropertyName == nameof(Wafer.SequenceRecipe))
            {
                var wafer = sender as Wafer;
                if (wafer != null && !string.IsNullOrEmpty(wafer.SequenceRecipe))
                {
                    // 如果分配了sequence，确保不会被选中
                    wafer.IsSelected = false;
                }
            }
        }

        private string RecipeAuditUnit() => _loadPortData?.TabName ?? "LoadPort";

        private void InitializeCommands()
        {
            SelectAllCommand = OperationAudit.Create("SelectAll", ExecuteSelectAll, RecipeAuditUnit, CanExecuteSelectAll);
            ClearSelectionCommand = OperationAudit.Create("ClearSelection", ExecuteClearSelection, RecipeAuditUnit, CanExecuteClearSelection);
            AssignRecipeCommand = OperationAudit.Create("AssignRecipe", ExecuteAssignRecipe, RecipeAuditUnit, CanExecuteAssignRecipe);
            CancelCommand = OperationAudit.Create("CancelAssign", ExecuteCancel, RecipeAuditUnit);
        }

        public async Task InitializeRuntimeStateAsync()
        {
            await PlcInitGate.WaitAsync().ConfigureAwait(false);
            try
            {
                await Task.Run(() => plcComm.Write("M170", false)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to initialize recipe runtime state: {ex.Message}");
            }
            finally
            {
                PlcInitGate.Release();
            }
        }

        #region 全选与反选功能

        private void ExecuteSelectAll()
        {
            if (_waferList != null)
            {
                foreach (var wafer in _waferList)
                {
                    // 只选择有 Wafer 的槽位（非空槽位）
                    if (wafer.WaferStatus == WaferStatus.NormalExistence && string.IsNullOrEmpty(wafer.SequenceRecipe))
                    {
                        wafer.IsSelected = true;
                    }
                }
                UpdateSelectedWaferCount();
                UpdateStatusMessage();
            }
        }

        private bool CanExecuteSelectAll()
        {
            return _waferList != null && _waferList.Count > 0;
        }

        private void ExecuteClearSelection()
        {
            if (_waferList != null)
            {
                foreach (var wafer in _waferList)
                {
                    if (wafer.IsSelected)
                    {
                        wafer.WaferName = string.Empty;
                    }

                    wafer.IsSelected = false;
                }
                UpdateSelectedWaferCount();
                UpdateStatusMessage();
            }
        }

        private bool CanExecuteClearSelection()
        {
            return _waferList != null && _waferList.Count > 0;
        }
        public void ApplyBatchWaferName(string? waferName)
        {
            if (string.IsNullOrWhiteSpace(waferName))
            {
                return;
            }

            if (!WaferNameRules.IsValid(waferName))
            {
                StatusMessage = WaferNameRules.ErrorMessage;
                return;
            }

            var selectedWafers = _waferList?
                .Where(w => w.IsSelected && w.WaferStatus == WaferStatus.NormalExistence && string.IsNullOrEmpty(w.SequenceRecipe))
                .OrderBy(w => w.SlotID)
                .ToList();

            if (selectedWafers == null || selectedWafers.Count == 0)
            {
                StatusMessage = "Please select wafers before naming";
                return;
            }

            if (selectedWafers.Count == 1)
            {
                StatusMessage = "Please edit the wafer name directly in the list for a single wafer";
                return;
            }

            var normalizedName = waferName.Trim();

            foreach (var wafer in selectedWafers)
            {
                wafer.WaferName = BuildBatchWaferName(normalizedName, wafer.SlotID);
            }

            BatchWaferName = normalizedName;

            StatusMessage = $"Updated wafer names for {selectedWafers.Count} wafer(s)";
        }
        #endregion

        #region Recipe 分配功能

        private async void ExecuteAssignRecipe()
        {
            var selectedWafers = _waferList?.Where(w => w.IsSelected && w.WaferStatus == WaferStatus.NormalExistence).OrderBy(w=>w.SlotID).ToList();

            if (selectedWafers == null || selectedWafers.Count == 0)
            {
                StatusMessage = "Please select at least one wafer";
                return;
            }

            if (SelectedSequence == null)
            {
                StatusMessage = "Please select a recipe";
                return;
            }
            // 清空临时列表
            lock (_globalLock)
            {
                _tempOrderedWafers.Clear();
            }
            int LoadportID = -1;
            switch (_loadPortData.PortNumber)
            {
                case "COM12":
                    LoadportID = 0;
                    break;
                case "COM13":
                    LoadportID = 1;
                    break;
                case "COM14":
                    LoadportID = 2;
                    break;
                case "COM15":
                    LoadportID = 3;
                    break;
                case "COM16":
                    LoadportID = 4;
                    break;
            }
           
            Dictionary<Wafer, int>? loadportSnapshots = null;
            var plcWriteSucceeded = false;
            try
            { 
                // 记录开始时间
                var stopwatch = Stopwatch.StartNew();

                ApplyBatchWaferNameForSelectedWafers();

                // 同一配方只组包一次；PLC 单连接在 SquenceWithPLC 内已串行化，此处按片顺序 await，避免多余线程空等
                var plcPayload = sequenceWithPLC.BuildPlcWritePayload(SelectedSequence);
                using var batchCts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

                // PLC 寻址仅需 LoadportID + SlotID，不下发成功前不更新 SequenceRecipe / WaferStatus 等 Slot Map 信息
                loadportSnapshots = selectedWafers.ToDictionary(w => w, w => w.LoadportID);
                foreach (var wafer in selectedWafers)
                {
                    wafer.LoadportID = LoadportID;
                }

                #region Assign防呆
                //在Assign Wafer的时候做一个防呆，先读B3003，如果读出来是True，则判断Sequen中的所有Polish，Process Type是否选择了64，选择了给B3003写入False，没选择64则提示不能Assign，重新选择PolishRecipe                
                foreach (var wafer in selectedWafers)
                {
                    var path = Path.Combine("D:\\CMP\\Configuration", "Recipes", "Sequence", $"{SelectedSequence.Name}.json");
                    var recipeData = JsonConvert.DeserializeObject<SequenceRecipeData>(File.ReadAllText(path))!;

                    if (recipeData.PolishASideOperationAChecked && !TryValidatePolishProcessType(recipeData, GlobalService.kvHostLinkInit.kvLinkPolishA.sock ,"RecipePolishA", "PolishA")) return;
                    if (recipeData.PolishASideOperationBChecked && !TryValidatePolishProcessType(recipeData, GlobalService.kvHostLinkInit.kvLinkPolishB.sock, "RecipePolishB", "PolishB")) return;
                    if (recipeData.PolishBSideOperationCChecked && !TryValidatePolishProcessType(recipeData, GlobalService.kvHostLinkInit.kvLinkPolishC.sock, "RecipePolishC", "PolishC")) return;
                    if (recipeData.PolishBSideOperationDChecked && !TryValidatePolishProcessType(recipeData, GlobalService.kvHostLinkInit.kvLinkPolishD.sock, "RecipePolishD", "PolishD")) return;
                }

                #endregion


                var batchWriteSuccess = await sequenceWithPLC.AssignWaferSequencesToPLCBatchAsync(selectedWafers, plcComm, plcPayload, batchCts.Token);
                if (!batchWriteSuccess)
                {
                    RestoreLoadportIds(selectedWafers, loadportSnapshots);
                    StatusMessage = "Failed to assign recipe to one or more wafers";
                    return;
                }

                plcWriteSucceeded = true;

                foreach (var wafer in selectedWafers)
                {
                    PrepareWaferForAssign(wafer, LoadportID);
                    FinalizeAssignedWafer(wafer, LoadportID);
                    _loadPortData.AddLogMessage($"Assigned recipe '{wafer.SequenceRecipe}' to wafer in slot {wafer.SlotID}");
                }

                bool allSuccess = true;
                if (allSuccess)
                {
                    // 重新 Assign 视为恢复本口入料
                    _loadPortData.ClearStopLoad();

                    // 按SlotID顺序将wafer添加到全局队列
                    lock (_globalLock)
                    {
                        var orderedWafers = _tempOrderedWafers.OrderBy(w => w.SlotID).ToList();
                        foreach (var wafer in orderedWafers)
                        {
                            WaferPickQueue.Enqueue(wafer);
                        }
                    }
                    // 停止计时并记录时间
                    stopwatch.Stop();
                    var elapsedTime = stopwatch.ElapsedMilliseconds;

                    _loadPortData.AddLogMessage($"Async parallel processing of {selectedWafers.Count} wafers with {WaferPickQueue.Count} tasks took {elapsedTime} ms");
                    WaferRuntimeStore.Instance.RequestSave();

                    SelectedWaferCount = selectedWafers.Count;
                    StatusMessage = $"Successfully assigned '{SelectedSequence.Name}' to {selectedWafers.Count} wafer(s)";
                }
                else {
                    StatusMessage = "Failed to assign recipe to one or more wafers";
                }
                // 发送sequence完成给设备恢复正常流程;
                GlobalService.plcComm.WriteBoolVari("M171", true);

                // 关闭对话框（如果需要）
                CloseWindowAction?.Invoke(true);
            }
            catch (Exception ex)
            {
                if (!plcWriteSucceeded && loadportSnapshots != null)
                {
                    RestoreLoadportIds(selectedWafers, loadportSnapshots);
                }
                StatusMessage = $"Error assigning recipe: {ex.Message}";
            }
        }

        private static void RestoreLoadportIds(IEnumerable<Wafer> wafers, Dictionary<Wafer, int> snapshots)
        {
            foreach (var wafer in wafers)
            {
                wafer.LoadportID = snapshots[wafer];
            }
        }

        /// <summary>
        /// Assign防呆：校验指定 Polish 操作所选配方的 ProcessType 是否为 64 (Pad Search)。
        /// 校验通过时将 B3003 复位为 False；失败时设置 StatusMessage 并返回 false，调用方应中止 Assign。
        /// </summary>
        private bool TryValidatePolishProcessType(SequenceRecipeData recipeData, int sock, string recipeKey, string operationName)
        {
            string variableName = "PadSearchFinished";
            byte[] data = new byte[256];
            //int err = KHL.KHLReadDevicesAsWords(sock, KHLDevType.DEV_B, 3003, 1, data);
            int err = KHL.KHLReadVariable(sock, variableName, data, true);
            bool b3003Value = false;
            KST.ByteToBool(ref b3003Value, data);

            if (err != (int)KHLUserError.KHL_ERR_NONE)
            {
                StatusMessage = $"PLC address B3003 read error. Please check the cause and then assign the Recipe!";
                return false;
            }

            if (b3003Value)
            {
                var selection = recipeData.RecipeSelections?.Find(x => x?.Key == recipeKey);
                if (selection?.Value == null)
                {
                    StatusMessage = $"{operationName} has no a recipe selecteds";
                    return false;
                }

                var recipe = JsonConvert.DeserializeObject<Recipe.Content>(FileUtil.ReadFile(selection.Value.ToString()!))!;
                if (GetRealVal<short>(recipe, "ProcessType", 0) != 64)
                {
                    StatusMessage = $"The ProcessType option for {operationName} must be set to \"64:Pad Search\"!";
                    return false;
                }

                //int v = KHL.KHLWriteDevicesAsWords(sock, KHLDevType.DEV_B, 3003, 1, [0]);
                int v = KHL.KHLWriteVariable(sock, variableName, [0], true);
                return true;
            }
            return true;
        }

        private void ApplyBatchWaferNameForSelectedWafers()
        {
            if (string.IsNullOrWhiteSpace(BatchWaferName) || _waferList == null)
            {
                return;
            }

            var normalizedName = BatchWaferName.Trim();
            foreach (var wafer in _waferList.Where(w => w.IsSelected && w.WaferStatus == WaferStatus.NormalExistence && string.IsNullOrEmpty(w.SequenceRecipe)))
            {
                wafer.WaferName = BuildBatchWaferName(normalizedName, wafer.SlotID);
            }
        }

        private static string BuildBatchWaferName(string waferName, int slotId)
        {
            return $"{waferName}_{slotId + 1}";
        }

        private void PrepareWaferForAssign(Wafer waferData, int loadportId)
        {
            // 仅在 PLC 下发成功后调用，更新 Slot Map 中的 wafer 状态与 sequence 信息
            waferData.LoadportID = loadportId;
            waferData.SequenceRecipe = SelectedSequence.Name;
            waferData.WaferStatus = WaferStatus.WaitAssignRecipe;
            waferData.CurrentStation = CurrentStation.Loadport;

            SetSequenceToWafer(ref waferData);
        }

        private void FinalizeAssignedWafer(Wafer waferData, int loadportId)
        {
            var index = loadportId * 25 + (waferData.SlotID + 1);
            lock (_globalLock)
            {
                if (GlobalService.dicWaferIndex.ContainsKey(index))
                {
                    GlobalService.dicWaferIndex[index] = waferData;
                }
                else
                {
                    GlobalService.dicWaferIndex.Add(index, waferData);
                }

                _tempOrderedWafers.Add(waferData);
            }
        }

        private async Task<bool> ProcessSingleWaferAsync(
            Wafer waferData,
            MitsubishiPlcService plcComm,
            int loadportId,
            SequencePlcWritePayload plcPayload,
            CancellationToken cancellationToken)
        {
            var originalLoadportId = waferData.LoadportID;
            waferData.LoadportID = loadportId;

            try
            {
                var success = await sequenceWithPLC.AssignWaferSequenceToPLCAsync(waferData, plcComm, plcPayload, cancellationToken);
                if (!success)
                {
                    waferData.LoadportID = originalLoadportId;
                    _loadPortData.AddLogMessage($"Failed to assign recipe to wafer in slot {waferData.SlotID}");
                    return false;
                }

                PrepareWaferForAssign(waferData, loadportId);

                var index = loadportId * 25 + (waferData.SlotID + 1);
                lock (_globalLock)
                {
                    if (GlobalService.dicWaferIndex.ContainsKey(index))
                    {
                        GlobalService.dicWaferIndex[index] = waferData;
                    }
                    else
                    {
                        GlobalService.dicWaferIndex.Add(index, waferData);
                    }

                    _tempOrderedWafers.Add(waferData);
                }

                _loadPortData.AddLogMessage($"Assigned recipe '{waferData.SequenceRecipe}' to wafer in slot {waferData.SlotID}");
                return true;
            }
            catch (Exception ex)
            {
                waferData.LoadportID = originalLoadportId;
                _loadPortData.AddLogMessage($"Error assigning recipe to wafer in slot {waferData.SlotID}: {ex.Message}");
                return false;
            }
        }

        private bool CanExecuteAssignRecipe()
        {
            return _waferList != null &&
                   _waferList.Any(w => w.IsSelected) && SelectedSequence != null;
        }

        private void ExecuteCancel()
        {
            // 可以在这里添加取消逻辑
            StatusMessage = "Operation cancelled";
            // 关闭窗口，返回 false 表示取消操作
            CloseWindowAction?.Invoke(false);
        }
        public void ResetAndInitialize()
        {
            // 重置所有wafer的选中状态
            if (_waferList != null)
            {
                foreach (var wafer in _waferList)
                {
                    wafer.IsSelected = false;
                }
            }

            // 重置选中计数
            SelectedWaferCount = 0;
            StatusMessage = "Ready to assign recipe";

            // 通知UI更新
            UpdateSelectedWaferCount();
        }
        #endregion

        #region 辅助方法

        private void UpdateSelectedWaferCount()
        {
            if (_waferList != null)
            {
                SelectedWaferCount = _waferList.Count(w => w.IsSelected && w.WaferStatus == WaferStatus.NormalExistence);
            }
            else
            {
                SelectedWaferCount = 0;
            }

            // 通知命令状态可能已改变
            ((DelegateCommand)AssignRecipeCommand)?.RaiseCanExecuteChanged();
        }

        private void UpdateStatusMessage()
        {
            if (SelectedSequence != null && SelectedWaferCount > 0)
            {
                StatusMessage = $"Ready to assign '{SelectedSequence.Name}' to {SelectedWaferCount} wafer(s)";
            }
            else if (SelectedSequence != null)
            {
                StatusMessage = $"Recipe '{SelectedSequence.Name}' selected. Please select wafers.";
            }
            else
            {
                StatusMessage = "Please select a recipe and wafers";
            }
        }

        #endregion

        #region Wafer的Sequnce逻辑算法
        private void SetSequenceToWafer(ref Wafer wafer)
        {
            var path = Path.Combine("D:\\CMP\\Configuration", "Recipes", "Sequence", $"{wafer.SequenceRecipe}.json");
            try
            {
                wafer.SequnceValue = new int[16];
                var text = File.ReadAllText(path);
                var data = JsonConvert.DeserializeObject<SequenceRecipeData>(text);
                if (data == null) return;
                var waferSequencePlan = WaferSequencePlanner.BuildPlan(data);
                Array.Copy(waferSequencePlan.SequnceValue, wafer.SequnceValue, waferSequencePlan.SequnceValue.Length);
                wafer.WaferSequenceModels = waferSequencePlan.WaferSequenceModels;
                wafer.SequenceTrackSide = Wafer.ResolveSequenceTrackSide(wafer.WaferSequenceModels);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        private static T? GetRealVal<T>(Recipe.Content recipeModel, string name, int stepIndex)
        {
            var model = recipeModel.Data?
                .FirstOrDefault(item => {
                    string itemName = item.Name!.Replace(" ", "")
                                .Replace("_", "")
                                .Replace("-", "")
                                .Replace("&", "");

                    return itemName == name || (item.CompatibleName != null && item.CompatibleName.Contains(name));
                }
                    );

            if (model == null) return default;
            if (stepIndex < 0 || stepIndex >= model.Steps.Count) return default;

            var value = model.Steps[stepIndex].Value;
            var type = model.Type?.ToLowerInvariant();
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            if (type == "option" || type == "list")
            {
                var arr = value.Split(':');
                value = targetType == typeof(string) ? (arr.Length >= 2 ? arr[1] : arr[0]) : arr[0];
            }


            try
            {
                if (targetType == typeof(string))
                    return (T)(object)value;

                if (targetType.IsEnum)
                    return (T)Enum.Parse(targetType, value, ignoreCase: true);

                return (T)Convert.ChangeType(value, targetType);
            }
            catch
            {
                return default;
            }
        }
        #endregion
    }
}
