using CMP.Constants.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Models.Warn
{
    public class ConsumableModel:BindableBase
    {       
        private WorkstationType workstation;
        private string? _consumableName;
        private string? _waferCount;
        private string? _usedTime;
        private string? _waferCountSet;
        private string? _usedTimeSet;
        private string? _updateTime;

        public WorkstationType Workstation { get => workstation; set => SetProperty(ref workstation, value); }
        public string? ConsumableName { get => _consumableName; set => SetProperty(ref _consumableName, value); }
        public string? WaferCount { get => _waferCount; set => SetProperty(ref _waferCount, value); }
        public string? UsedTime { get => _usedTime; set => SetProperty(ref _usedTime, value); }
        public string? WaferCountSet { get => _waferCountSet; set=>SetProperty(ref _waferCountSet, value); }
        public string? UsedTimeSet { get => _usedTimeSet; set => SetProperty(ref _usedTimeSet, value); }
        public string? UpdateTime { get => _updateTime; set => SetProperty(ref _updateTime, value); }

        private bool _isWaferCountSetEditable = true;
        public bool IsWaferCountSetEditable
        {
            get => _isWaferCountSetEditable;
            set => SetProperty(ref _isWaferCountSetEditable, value);
        }

        private bool _isUsedTimeSetEditable = true;
        public bool IsUsedTimeSetEditable
        {
            get => _isUsedTimeSetEditable;
            set => SetProperty(ref _isUsedTimeSetEditable, value);
        }

        public uint WaferCountAdress { get; set; }
        public uint UsedTimeAdress { get; set; }
        public uint UpdateTimeAdress { get; set; }
    }
}
