using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Models.Resource
{
    /// <summary>
    /// 取片/下片调度用：与 PLC M183（A 线正常）、M184（B 线正常）对应；由配方展开后的腔体顺序推断。
    /// </summary>
    public enum WaferSequenceTrackSide
    {
        Unknown = 0,
        A = 1,
        B = 2
    }

    public class Wafer:BindableBase
    {
        public int LoadportID { get; set; }
        public int SlotID { get; set; }

        private string _waferName = string.Empty;
        public string WaferName
        {
            get => _waferName;
            set => SetProperty(ref _waferName, value);
        }

        public string LotID { get; set; }
        private WaferStatus _waferStatus;
        public WaferStatus WaferStatus { get=> _waferStatus; 
            set{
                SetProperty(ref _waferStatus, value);
                UpdateWaferColor(); 
            }
        }
        public CurrentStation CurrentStation
        {
            get => _currentStation;
            set => SetProperty(ref _currentStation, value);
        }
        private CurrentStation _currentStation;
        private string _sequenceRecipe;
        public string SequenceRecipe {
            get => _sequenceRecipe;
            set => SetProperty(ref _sequenceRecipe, value);
        }

        public int index;

        private string _waferColor;
        private bool _isSelected;

        public string WaferColor
        {
            get { return _waferColor; }
            set { SetProperty(ref _waferColor, value); }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public List<WaferSequenceModel> WaferSequenceModels { get; set; } = [];
        public int[] SequnceValue { get; set; } = new int[16];

        private WaferSequenceTrackSide _sequenceTrackSide;

        /// <summary>
        /// 抛光/清洗主流程侧别，用于 M183/M184 门控与 A/B 交替取片（与 PLC 约定一致）。
        /// </summary>
        public WaferSequenceTrackSide SequenceTrackSide
        {
            get => _sequenceTrackSide;
            set => SetProperty(ref _sequenceTrackSide, value);
        }     

        public Wafer() { }
        public Wafer(int loadportID, int slotID)
        {
            this.LoadportID = loadportID;
            SlotID = slotID;
            WaferStatus = WaferStatus.WaitAssignRecipe;
            CurrentStation = CurrentStation.Loadport;
            WaferName = string.Empty;

            index = loadportID * 25 + (slotID + 1);
            if (GlobalService.dicWaferIndex.ContainsKey(index))
            {
                GlobalService.dicWaferIndex[index] = this;
            }
            else 
            { 
                GlobalService.dicWaferIndex.Add(index, this); 
            }
           
            UpdateWaferColor();
        }

        public Wafer(int loadportID, int slotID,WaferStatus status)
        {
            this.LoadportID = loadportID;
            SlotID = slotID;
            WaferStatus = status;
            CurrentStation = CurrentStation.Loadport;
            WaferName = string.Empty;
            UpdateWaferColor();
        }

        /// <summary>
        /// 按 <see cref="WaferSequenceModels"/> 顺序：先出现的抛光步 PolishA/B 归为 A 线，PolishC/D 归为 B 线；
        /// 若无抛光步，则按先出现的 A 线 / B 线清洗腔体推断；否则 <see cref="WaferSequenceTrackSide.Unknown"/>。
        /// </summary>
        public static WaferSequenceTrackSide ResolveSequenceTrackSide(IReadOnlyList<WaferSequenceModel>? models)
        {
            if (models == null || models.Count == 0)
                return WaferSequenceTrackSide.Unknown;

            foreach (var m in models)
            {
                var n = m.ChamberName ?? string.Empty;
                if (n.StartsWith("PolishA", StringComparison.Ordinal) || n.StartsWith("PolishB", StringComparison.Ordinal))
                    return WaferSequenceTrackSide.A;
                if (n.StartsWith("PolishC", StringComparison.Ordinal) || n.StartsWith("PolishD", StringComparison.Ordinal))
                    return WaferSequenceTrackSide.B;
            }

            foreach (var m in models)
            {
                var n = m.ChamberName ?? string.Empty;
                if (n.Contains("RollerA", StringComparison.Ordinal) || n.Contains("PenA", StringComparison.Ordinal)
                    || string.Equals(n, "DryA", StringComparison.Ordinal))
                    return WaferSequenceTrackSide.A;
                if (n.Contains("RollerB", StringComparison.Ordinal) || n.Contains("PenB", StringComparison.Ordinal)
                    || string.Equals(n, "DryB", StringComparison.Ordinal))
                    return WaferSequenceTrackSide.B;
            }

            return WaferSequenceTrackSide.Unknown;
        }
        
        // 添加ToString方法便于调试
        public override string ToString()
        {
            return $"Wafer[{LoadportID}-{SlotID}]: {WaferName} at {CurrentStation} ({WaferStatus})";
        }
        private void UpdateWaferColor()
        {
            switch (WaferStatus)
            {
                case WaferStatus.NormalExistence:
                    WaferColor = "blue";
                    break;
                case WaferStatus.WaitAssignRecipe:
                    WaferColor = "pink";
                    break;
                case WaferStatus.WaitProcess:
                    WaferColor = "lightYellow";
                    break;
                case WaferStatus.Processing:
                    WaferColor = "Orange";
                    break;
                case WaferStatus.ProcessDone:
                    WaferColor = "green";
                    break;
                case WaferStatus.Error:
                    WaferColor = "red";
                    break;
                case WaferStatus.NonWafer:
                case WaferStatus.None:
                    WaferColor = "gray";
                    break;
                default:
                    WaferColor = "lightGray";
                    break;
            }
        }
    }

    public enum WaferStatus
    {
        None,
        WaitAssignRecipe,
        WaitProcess,
        Processing,
        ProcessDone,
        Error, 
        NonWafer,
        NormalExistence,
        CrossSlot,
        UpDownTile,
        ThicknessErrorThick,
        ThicknessErrorThin,
        Unknown // 用于处理未定义的状态代码
    }

    public enum CurrentStation
    {
        Loadport,
        PolishA,
        PolishB,
        PolishC,
        PolishD,
        CleanAR1,
        CleanAR2,
        CleanAR3,
        CleanAP1,
        CleanAP2,
        CleanADry,
        CleanBR1,
        CleanBR2,
        CleanBR3,
        CleanBP1,
        CleanBP2,
        CleanBDry,
        EFEMRobot,
        TransBuffer,
        TransRobot,
        FixABuffer,
        FixALTP1,
        FixALTP2,
        FixALTP3,
        FixBBuffer,
        FixBLTP1, 
        FixBLTP2, 
        FixBLTP3, 
        CleanARobot1,
        CleanARobot2,
        CleanBRobot1,
        CleanBRobot2,
    }
}
