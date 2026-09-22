using CMP.Models.Recipe.Sequence;
using CMP.Models.Resource;
using CMP.Services.Log;
using CMP.Services.Recipe;
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
            SetSequenceToWafer(ref wafer);
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
        #endregion

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

