using CMP.Models.Recipe.Sequence;
using CMP.Models.Resource;
using CMP.Services.Log;
using HandyControl.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace CMP.ViewModels.Common.Dialog
{
    public class AddWaferDialogViewModel : BindableBase
    {
        private string _title = "Add Wafer";
        private string _loadportID = string.Empty;
        private string _slotID = string.Empty;
        private string _waferName = string.Empty;
        private string _sequenceRecipe = string.Empty;
        private string _currentChamberName = string.Empty;
        public string CurrentChamberName
        {
            get { return _currentChamberName; }
            set { SetProperty(ref _currentChamberName, value); }
        }
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public Wafer ResultWafer { get; private set; }

        public string LoadportID
        {
            get { return _loadportID; }
            set { SetProperty(ref _loadportID, value); }
        }

        public string SlotID
        {
            get { return _slotID; }
            set { SetProperty(ref _slotID, value); }
        }

        public string WaferName
        {
            get { return _waferName; }
            set { SetProperty(ref _waferName, value); }
        }

        public string SequenceRecipe
        {
            get { return _sequenceRecipe; }
            set { SetProperty(ref _sequenceRecipe, value); }
        }

        private ObservableCollection<string> _sequenceRecipeList = new ObservableCollection<string>();
        public ObservableCollection<string> SequenceRecipeList
        {
            get { return _sequenceRecipeList; }
            set { SetProperty(ref _sequenceRecipeList, value); }
        }

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand OKCommand { get; set; }

        public Action<bool?> CloseWindowAction { get; set; }

        public AddWaferDialogViewModel(string chamberName = "")
        {
            CurrentChamberName = chamberName ?? string.Empty;
            CancelCommand = new DelegateCommand(CancelClick);
            OKCommand = new DelegateCommand(OKClick);
            LoadSequenceRecipes();
        }

        private void LoadSequenceRecipes()
        {
            try
            {
                var sequenceDirectory = Path.Combine("D:\\CMP\\Configuration", "Recipes", "Sequence");
                
                if (!Directory.Exists(sequenceDirectory))
                {
                    Growl.WarningGlobal($"Sequence 目录不存在: {sequenceDirectory}");
                    return;
                }

                var jsonFiles = Directory.GetFiles(sequenceDirectory, "*.json");
                SequenceRecipeList.Clear();

                foreach (var file in jsonFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        SequenceRecipeList.Add(fileName);
                    }
                }

                if (SequenceRecipeList.Count == 0)
                {
                    Growl.InfoGlobal("未找到 Sequence Recipe 文件");
                }
            }
            catch (Exception ex)
            {
                Growl.ErrorGlobal($"加载 Sequence Recipe 列表时发生错误: {ex.Message}");
            }
        }

        private void OKClick()
        {
            if (!ValidateInput())
            {
                return;
            }

            Wafer wafer = new Wafer();

            if (int.TryParse(LoadportID, out int loadportId))
            {
                wafer.LoadportID = loadportId-1;
            }

            if (int.TryParse(SlotID, out int slotId))
            {
                wafer.SlotID = slotId-1;
            }

            wafer.WaferName = string.IsNullOrWhiteSpace(WaferName)
                ? string.Empty
                : WaferName;
            wafer.SequenceRecipe = SequenceRecipe;
            wafer.WaferStatus = WaferStatus.Processing;
            wafer.CurrentStation = CurrentStation.Loadport;
            SetSequenceToWafer(wafer);
            // 可以根据需要使用 Chamber 名称
            if (!string.IsNullOrEmpty(CurrentChamberName))
            {
                // 根据 Chamber 名称设置相关属性
                // 根据 ChamberName 设置 CurrentStation
                if (!string.IsNullOrEmpty(CurrentChamberName))
                {
                    wafer.CurrentStation = GetStationFromChamberName(CurrentChamberName);
                }
                else
                {
                    wafer.CurrentStation = CurrentStation.Loadport; // 默认值
                }

                ResultWafer = wafer;
                CloseWindowAction?.Invoke(true);
            }
        }
        #region Wafer的Sequnce逻辑算法
        private void SetSequenceToWafer(Wafer wafer)
        {
            var path = Path.Combine("D:\\CMP\\Configuration", "Recipes", "Sequence", $"{wafer.SequenceRecipe}.json");
            try
            {
                wafer.WaferSequenceModels = [];
                var text = File.ReadAllText(path);
                var data = JsonConvert.DeserializeObject<SequenceRecipeData>(text);
                ApplySequenceDataToWafer(wafer, data);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 使用已加载的配方数据填充 Wafer 的 sequence 模型（批量下发时与 UI 选中项共用，避免每片读盘）。
        /// </summary>
        private void ApplySequenceDataToWafer(Wafer wafer, SequenceRecipeData? data)
        {
            try
            {
                wafer.WaferSequenceModels = [];
                wafer.SequnceValue = new int[16];

                if (data != null && !string.IsNullOrWhiteSpace(data.Name))
                {
                    #region 确定每个chamber的执行顺序的步骤
                    int currentMaxStep = -1;
                    //Polish
                    if (data.PolishMainType == SequenceType.Parallel)
                    {
                        if (data.PolishMainASideChecked)
                        {
                            if (data.PolishASideOperationType == SequenceType.Parallel)
                            {
                                if (data.PolishASideOperationBChecked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishB", ChamberSequnceStep = 0 });
                                    SetCurrentStep(ref currentMaxStep, 0);
                                }
                                if (data.PolishASideOperationAChecked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishA", ChamberSequnceStep = 0 });
                                    SetCurrentStep(ref currentMaxStep, 0);
                                }
                            }
                            else if (data.PolishASideOperationType == SequenceType.Serial)
                            {
                                int currentStep = -1;
                                if (data.PolishASideOperationBChecked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishB", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.PolishASideOperationAChecked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishA", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                        }
                        if (data.PolishMainBSideChecked)
                        {
                            if (data.PolishBSideOperationType == SequenceType.Parallel)
                            {
                                if (data.PolishBSideOperationDChecked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishD", ChamberSequnceStep = 0 });
                                    SetCurrentStep(ref currentMaxStep, 0);
                                }
                                if (data.PolishBSideOperationCChecked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishC", ChamberSequnceStep = 0 });
                                    SetCurrentStep(ref currentMaxStep, 0);
                                }
                            }
                            else if (data.PolishBSideOperationType == SequenceType.Serial)
                            {
                                int currentStep = -1;
                                if (data.PolishBSideOperationDChecked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishD", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.PolishBSideOperationCChecked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishC", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);

                                }
                            }
                        }
                    }
                    else if (data.PolishMainType == SequenceType.Serial)
                    {
                        if (data.PolishMainASideChecked)
                        {
                            if (data.PolishASideOperationType == SequenceType.Parallel)
                            {
                                if (data.PolishASideOperationBChecked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishB", ChamberSequnceStep = 0 });
                                    SetCurrentStep(ref currentMaxStep, 0);
                                }
                                if (data.PolishASideOperationAChecked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishA", ChamberSequnceStep = 0 });
                                    SetCurrentStep(ref currentMaxStep, 0);
                                }
                            }
                            else if (data.PolishASideOperationType == SequenceType.Serial)
                            {
                                int currentStep = -1;
                                if (data.PolishASideOperationBChecked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishB", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.PolishASideOperationAChecked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishA", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                        }
                        if (data.PolishMainBSideChecked)
                        {
                            if (data.PolishBSideOperationType == SequenceType.Parallel)
                            {
                                int currentStep = currentMaxStep + 1;
                                if (data.PolishBSideOperationDChecked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishD", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.PolishBSideOperationCChecked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishC", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                            else if (data.PolishBSideOperationType == SequenceType.Serial)
                            {
                                int currentStep = currentMaxStep;
                                if (data.PolishBSideOperationDChecked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishD", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.PolishBSideOperationCChecked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishC", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                        }
                    }
                    //Roller
                    if (data.CleanMainType == SequenceType.Parallel)
                    {
                        int tempCurrentMaxStep = currentMaxStep;
                        if (data.CleanMainASideChecked)
                        {
                            if (data.CleanASideRollerOp1Type == SequenceType.Parallel && data.CleanASideRollerOp2Type == SequenceType.Parallel)
                            {
                                int currentStep = tempCurrentMaxStep + 1;
                                if (data.CleanASideRollerOp1Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA1", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanASideRollerOp2Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA2", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanASideRollerOp3Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA3", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                            if (data.CleanASideRollerOp1Type == SequenceType.Serial && data.CleanASideRollerOp2Type == SequenceType.Serial)
                            {
                                int currentStep = tempCurrentMaxStep;
                                if (data.CleanASideRollerOp1Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA1", ChamberSequnceStep = currentStep });
                                }
                                if (data.CleanASideRollerOp2Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA2", ChamberSequnceStep = currentStep });
                                }
                                if (data.CleanASideRollerOp3Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA3", ChamberSequnceStep = currentStep });
                                }
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanASideRollerOp1Type == SequenceType.Parallel && data.CleanASideRollerOp2Type == SequenceType.Serial)
                            {
                                int currentStep = tempCurrentMaxStep + 1;
                                if (data.CleanASideRollerOp1Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA1", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanASideRollerOp2Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA2", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanASideRollerOp3Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA3", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                            if (data.CleanASideRollerOp1Type == SequenceType.Serial && data.CleanASideRollerOp2Type == SequenceType.Parallel)
                            {
                                int currentStep = tempCurrentMaxStep;
                                if (data.CleanASideRollerOp1Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA1", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanASideRollerOp2Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA2", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanASideRollerOp3Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA3", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                        }
                        if (data.CleanMainBSideChecked)
                        {
                            if (data.CleanBSideRollerOp1Type == SequenceType.Parallel && data.CleanBSideRollerOp2Type == SequenceType.Parallel)
                            {
                                int currentStep = tempCurrentMaxStep + 1;
                                if (data.CleanBSideRollerOp1Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB1", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanBSideRollerOp2Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB2", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanBSideRollerOp3Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB3", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                            if (data.CleanBSideRollerOp1Type == SequenceType.Serial && data.CleanBSideRollerOp2Type == SequenceType.Serial)
                            {
                                int currentStep = tempCurrentMaxStep;
                                if (data.CleanBSideRollerOp1Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB1", ChamberSequnceStep = currentStep });
                                }
                                if (data.CleanBSideRollerOp2Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB2", ChamberSequnceStep = currentStep });
                                }
                                if (data.CleanBSideRollerOp3Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB3", ChamberSequnceStep = currentStep });
                                }
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanBSideRollerOp1Type == SequenceType.Parallel && data.CleanBSideRollerOp2Type == SequenceType.Serial)
                            {
                                int currentStep = tempCurrentMaxStep + 1;
                                if (data.CleanBSideRollerOp1Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB1", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanBSideRollerOp2Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB2", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanBSideRollerOp3Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB3", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                            if (data.CleanBSideRollerOp1Type == SequenceType.Serial && data.CleanBSideRollerOp2Type == SequenceType.Parallel)
                            {
                                int currentStep = tempCurrentMaxStep;
                                if (data.CleanBSideRollerOp1Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB1", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanBSideRollerOp2Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB2", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanBSideRollerOp3Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB3", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                        }
                    }
                    else if (data.CleanMainType == SequenceType.Serial)
                    {
                        int tempCurrentMaxStep = currentMaxStep;
                        if (data.CleanMainASideChecked)
                        {
                            if (data.CleanASideRollerOp1Type == SequenceType.Parallel && data.CleanASideRollerOp2Type == SequenceType.Parallel)
                            {
                                int currentStep = tempCurrentMaxStep + 1;
                                if (data.CleanASideRollerOp1Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA1", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanASideRollerOp2Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA2", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanASideRollerOp3Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA3", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                            if (data.CleanASideRollerOp1Type == SequenceType.Serial && data.CleanASideRollerOp2Type == SequenceType.Serial)
                            {
                                int currentStep = tempCurrentMaxStep;
                                if (data.CleanASideRollerOp1Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA1", ChamberSequnceStep = currentStep });
                                }
                                if (data.CleanASideRollerOp2Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA2", ChamberSequnceStep = currentStep });
                                }
                                if (data.CleanASideRollerOp3Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA3", ChamberSequnceStep = currentStep });
                                }
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanASideRollerOp1Type == SequenceType.Parallel && data.CleanASideRollerOp2Type == SequenceType.Serial)
                            {
                                int currentStep = tempCurrentMaxStep + 1;
                                if (data.CleanASideRollerOp1Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA1", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanASideRollerOp2Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA2", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanASideRollerOp3Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA3", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                            if (data.CleanASideRollerOp1Type == SequenceType.Serial && data.CleanASideRollerOp2Type == SequenceType.Parallel)
                            {
                                int currentStep = tempCurrentMaxStep;
                                if (data.CleanASideRollerOp1Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA1", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanASideRollerOp2Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA2", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanASideRollerOp3Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA3", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                        }
                        if (data.CleanMainBSideChecked)
                        {
                            int temCurrentMaxStep2 = currentMaxStep;
                            if (data.CleanBSideRollerOp1Type == SequenceType.Parallel && data.CleanBSideRollerOp2Type == SequenceType.Parallel)
                            {
                                int currentStep = temCurrentMaxStep2 + 1;
                                if (data.CleanBSideRollerOp1Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB1", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanBSideRollerOp2Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB2", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanBSideRollerOp3Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB3", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                            if (data.CleanBSideRollerOp1Type == SequenceType.Serial && data.CleanBSideRollerOp2Type == SequenceType.Serial)
                            {
                                int currentStep = temCurrentMaxStep2;
                                if (data.CleanBSideRollerOp1Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB1", ChamberSequnceStep = currentStep });
                                }
                                if (data.CleanBSideRollerOp2Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB2", ChamberSequnceStep = currentStep });
                                }
                                if (data.CleanBSideRollerOp3Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB3", ChamberSequnceStep = currentStep });
                                }
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanBSideRollerOp1Type == SequenceType.Parallel && data.CleanBSideRollerOp2Type == SequenceType.Serial)
                            {
                                int currentStep = temCurrentMaxStep2 + 1;
                                if (data.CleanBSideRollerOp1Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB1", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanBSideRollerOp2Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB2", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanBSideRollerOp3Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB3", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                            if (data.CleanBSideRollerOp1Type == SequenceType.Serial && data.CleanBSideRollerOp2Type == SequenceType.Parallel)
                            {
                                int currentStep = temCurrentMaxStep2;
                                if (data.CleanBSideRollerOp1Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB1", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanBSideRollerOp2Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB2", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanBSideRollerOp3Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB3", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                        }
                    }
                    //Pen
                    if (data.CleanMainType == SequenceType.Parallel)
                    {
                        int tempCurrentMaxStep = currentMaxStep;
                        if (data.CleanMainASideChecked)
                        {
                            if (data.CleanASidePenOp1Type == SequenceType.Parallel)
                            {
                                int currentStep = tempCurrentMaxStep + 1;
                                if (data.CleanASidePenOp1Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenA1", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanASidePenOp2Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenA2", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                            else if (data.CleanASidePenOp1Type == SequenceType.Serial)
                            {
                                int currentStep = tempCurrentMaxStep;
                                if (data.CleanASidePenOp1Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenA1", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanASidePenOp2Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenA2", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                        }
                        if (data.CleanMainBSideChecked)
                        {
                            if (data.CleanBSidePenOp1Type == SequenceType.Parallel)
                            {
                                int currentStep = tempCurrentMaxStep + 1;
                                if (data.CleanBSidePenOp1Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenB1", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanBSidePenOp2Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenB2", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                            else if (data.CleanBSidePenOp1Type == SequenceType.Serial)
                            {
                                int currentStep = tempCurrentMaxStep;
                                if (data.CleanBSidePenOp1Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenB1", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanBSidePenOp2Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenB2", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                        }
                    }
                    else if (data.CleanMainType == SequenceType.Serial)
                    {
                        int tempCurrentMaxStep = currentMaxStep;
                        if (data.CleanMainASideChecked)
                        {
                            if (data.CleanASidePenOp1Type == SequenceType.Parallel)
                            {
                                int currentStep = tempCurrentMaxStep + 1;
                                if (data.CleanASidePenOp1Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenA1", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanASidePenOp2Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenA2", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                            else if (data.CleanASidePenOp1Type == SequenceType.Serial)
                            {
                                int currentStep = tempCurrentMaxStep;
                                if (data.CleanASidePenOp1Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenA1", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanASidePenOp2Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenA2", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                        }
                        if (data.CleanMainBSideChecked)
                        {
                            int tempCurrentMaxStep2 = currentMaxStep;
                            if (data.CleanBSidePenOp1Type == SequenceType.Parallel)
                            {
                                int currentStep = tempCurrentMaxStep2 + 1;
                                if (data.CleanBSidePenOp1Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenB1", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanBSidePenOp2Checked)
                                {
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenB2", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                            else if (data.CleanBSidePenOp1Type == SequenceType.Serial)
                            {
                                int currentStep = tempCurrentMaxStep2;
                                if (data.CleanBSidePenOp1Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenB1", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                                if (data.CleanBSidePenOp2Checked)
                                {
                                    currentStep += 1;
                                    wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenB2", ChamberSequnceStep = currentStep });
                                    SetCurrentStep(ref currentMaxStep, currentStep);
                                }
                            }
                        }
                    }
                    //Dry
                    if (data.CleanMainType == SequenceType.Parallel)
                    {
                        int tempCurrentMaxStep = currentMaxStep;
                        if (data.CleanMainASideChecked)
                        {
                            if (data.CleanASideDryOpChecked)
                            {
                                int currentStep = tempCurrentMaxStep + 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "DryA", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                        if (data.CleanMainBSideChecked)
                        {
                            if (data.CleanBSideDryOpChecked)
                            {
                                int currentStep = tempCurrentMaxStep + 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "DryB", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                    }
                    else if (data.CleanMainType == SequenceType.Serial)
                    {
                        int tempCurrentMaxStep = currentMaxStep;
                        if (data.CleanMainASideChecked)
                        {
                            if (data.CleanASideDryOpChecked)
                            {
                                int currentStep = tempCurrentMaxStep + 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "DryA", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                        if (data.CleanMainBSideChecked)
                        {
                            int tempCurrentMaxStep2 = currentMaxStep;
                            if (data.CleanBSideDryOpChecked)
                            {
                                int currentStep = tempCurrentMaxStep2 + 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "DryB", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                    }
                    #endregion

                    #region 确定每个chamber步骤需要写入的值
                    for (int i = 0; i < wafer.SequnceValue.Length; i++)
                    {
                        var list = wafer.WaferSequenceModels.Where(x => x.ChamberSequnceStep == i).ToList();
                        if (list.Count == 0)
                        {
                            wafer.SequnceValue[i] = 0;
                            continue;
                        }
                        //多个值表示并行，值相加
                        int val = 0;
                        foreach (var item in list)
                        {
                            val += item.ChamberStepValue;
                        }
                        wafer.SequnceValue[i] = val;
                    }
                    #endregion

                    wafer.SequenceTrackSide = Wafer.ResolveSequenceTrackSide(wafer.WaferSequenceModels);
                }
                else
                {
                    wafer.SequenceTrackSide = WaferSequenceTrackSide.Unknown;
                    Growl.ErrorGlobal("sequence Data is null ");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }
        private void SetCurrentStep(ref int current, int step)
        {
            if (step > current)
            {
                current = step;
            }
        }
        // 辅助方法：根据 ChamberName 获取对应的 CurrentStation
        public static CurrentStation GetStationFromChamberName(string chamberName)
        {
           // 根据 ChamberName 的前缀或关键词匹配对应的 Station
            switch (chamberName.ToUpper())
            {
                case "POLISHA":
                    return CurrentStation.PolishA;
                case "POLISHB":
                    return CurrentStation.PolishB;
                case "POLISHC":
                    return CurrentStation.PolishC;
                case "POLISHD":
                    return CurrentStation.PolishD;
                case "DRYA":
                    return CurrentStation.CleanADry;
                case "DRYB":
                    return CurrentStation.CleanBDry;
                case "PENA1":
                    return CurrentStation.CleanAP1;
                case "PENA2":
                    return CurrentStation.CleanAP2;
                case "PENB1":
                    return CurrentStation.CleanBP1;
                case "PENB2":
                    return CurrentStation.CleanBP2;
                case "ROLLERA1":
                    return CurrentStation.CleanAR1;
                case "ROLLERA2":
                    return CurrentStation.CleanAR2;
                case "ROLLERA3":
                    return CurrentStation.CleanAR3;
                case "ROLLERB1":
                    return CurrentStation.CleanBR1;
                case "ROLLERB2":
                    return CurrentStation.CleanBR2;
                case "ROLLERB3":
                    return CurrentStation.CleanBR3;
                default:
                    return CurrentStation.Loadport; // 默认值
            }
        }
        #endregion

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(LoadportID))
            {
                Growl.WarningGlobal("请输入 Loadport ID");
                return false;
            }

            if (!int.TryParse(LoadportID, out int loadportId) || loadportId < 1)
            {
                Growl.WarningGlobal("Loadport ID 必须是有效的数字");
                return false;
            }

            if (string.IsNullOrWhiteSpace(SlotID))
            {
                Growl.WarningGlobal("请输入 Slot ID");
                return false;
            }

            if (!int.TryParse(SlotID, out int slotId) || slotId < 1 || slotId > 25)
            {
                Growl.WarningGlobal("Slot ID 必须是 1-25 之间的有效数字");
                return false;
            }

            if (string.IsNullOrWhiteSpace(SequenceRecipe))
            {
                Growl.WarningGlobal("请选择 Sequence Recipe");
                return false;
            }

            return true;
        }

        private void CancelClick()
        {
            CloseWindowAction?.Invoke(false);
        }
    }
}

