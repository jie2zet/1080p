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
                if (!SetProperty(ref _waferStatus, value))
                {
                    return;
                }

                UpdateWaferColor();
                RaisePropertyChanged(nameof(CanSelectForAssign));
                ClearSelectionIfNotAssignable();
            }
        }

        /// <summary>只有槽位里正常存在、且尚未下发配方的晶圆可以勾选。</summary>
        public bool CanSelectForAssign =>
            WaferStatus == WaferStatus.NormalExistence && string.IsNullOrEmpty(SequenceRecipe);
        public CurrentStation CurrentStation
        {
            get => _currentStation;
            set => SetProperty(ref _currentStation, value);
        }
        private CurrentStation _currentStation;
        private string _sequenceRecipe;
        public string SequenceRecipe {
            get => _sequenceRecipe;
            set
            {
                if (!SetProperty(ref _sequenceRecipe, value))
                {
                    return;
                }

                RaisePropertyChanged(nameof(CanSelectForAssign));
                ClearSelectionIfNotAssignable();
            }
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
            set
            {
                if (value && !CanSelectForAssign)
                {
                    if (_isSelected)
                    {
                        SetProperty(ref _isSelected, false);
                    }
                    else
                    {
                        RaisePropertyChanged(nameof(IsSelected));
                    }

                    return;
                }

                SetProperty(ref _isSelected, value);
            }
        }

        private void ClearSelectionIfNotAssignable()
        {
            if (!CanSelectForAssign && _isSelected)
            {
                IsSelected = false;
            }
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
        /// 按配方展开后的腔体判断可走侧别：仅 A 返回 A，仅 B 返回 B，两侧都能走返回 <see cref="WaferSequenceTrackSide.Unknown"/>（取片时再定侧）。
        /// 离线 / 断连腔体不参与判断。
        /// </summary>
        public static WaferSequenceTrackSide ResolveSequenceTrackSide(IReadOnlyList<WaferSequenceModel>? models)
        {
            if (models == null || models.Count == 0)
                return WaferSequenceTrackSide.Unknown;

            var canA = false;
            var canB = false;
            foreach (var m in models)
            {
                var n = m.ChamberName ?? string.Empty;
                if (IsChamberUnavailable(n))
                    continue;

                var side = ClassifyChamberTrackSide(n);
                if (side == WaferSequenceTrackSide.A)
                    canA = true;
                else if (side == WaferSequenceTrackSide.B)
                    canB = true;
            }

            if (canA && canB)
                return WaferSequenceTrackSide.Unknown;
            if (canA)
                return WaferSequenceTrackSide.A;
            if (canB)
                return WaferSequenceTrackSide.B;
            return WaferSequenceTrackSide.Unknown;
        }

        public static WaferSequenceTrackSide ClassifyChamberTrackSide(string? chamberName)
        {
            var n = chamberName ?? string.Empty;
            if (n.StartsWith("PolishA", StringComparison.Ordinal) || n.StartsWith("PolishB", StringComparison.Ordinal)
                || n.Contains("RollerA", StringComparison.Ordinal) || n.Contains("PenA", StringComparison.Ordinal)
                || string.Equals(n, "DryA", StringComparison.Ordinal))
                return WaferSequenceTrackSide.A;
            if (n.StartsWith("PolishC", StringComparison.Ordinal) || n.StartsWith("PolishD", StringComparison.Ordinal)
                || n.Contains("RollerB", StringComparison.Ordinal) || n.Contains("PenB", StringComparison.Ordinal)
                || string.Equals(n, "DryB", StringComparison.Ordinal))
                return WaferSequenceTrackSide.B;
            return WaferSequenceTrackSide.Unknown;
        }

        private static bool IsChamberUnavailable(string chamberName)
        {
            var matches = ResolveChambers(chamberName);
            if (matches.Count == 0)
                return false;

            return matches.All(c =>
                c.ChamberStatus is ChamberStatus.Offline or ChamberStatus.Disconnect);
        }

        private static List<Chamber> ResolveChambers(string chamberName)
        {
            var result = new List<Chamber>();
            var g = GlobalService.globalChambers;
            if (g == null || string.IsNullOrEmpty(chamberName))
                return result;

            void Add(Chamber? chamber)
            {
                if (chamber != null)
                    result.Add(chamber);
            }

            if (chamberName.StartsWith("PolishA", StringComparison.Ordinal)) Add(g.PolishA);
            else if (chamberName.StartsWith("PolishB", StringComparison.Ordinal)) Add(g.PolishB);
            else if (chamberName.StartsWith("PolishC", StringComparison.Ordinal)) Add(g.PolishC);
            else if (chamberName.StartsWith("PolishD", StringComparison.Ordinal)) Add(g.PolishD);
            else if (chamberName.Contains("RollerA1", StringComparison.Ordinal)) Add(g.RollerA1);
            else if (chamberName.Contains("RollerA2", StringComparison.Ordinal)) Add(g.RollerA2);
            else if (chamberName.Contains("RollerA3", StringComparison.Ordinal)) Add(g.RollerA3);
            else if (chamberName.Contains("RollerB1", StringComparison.Ordinal)) Add(g.RollerB1);
            else if (chamberName.Contains("RollerB2", StringComparison.Ordinal)) Add(g.RollerB2);
            else if (chamberName.Contains("RollerB3", StringComparison.Ordinal)) Add(g.RollerB3);
            else if (chamberName.Contains("PenA", StringComparison.Ordinal))
            {
                Add(g.PenA1);
                Add(g.PenA2);
            }
            else if (chamberName.Contains("PenB", StringComparison.Ordinal))
            {
                Add(g.PenB1);
                Add(g.PenB2);
            }
            else if (string.Equals(chamberName, "DryA", StringComparison.Ordinal)) Add(g.DryA);
            else if (string.Equals(chamberName, "DryB", StringComparison.Ordinal)) Add(g.DryB);

            return result;
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
