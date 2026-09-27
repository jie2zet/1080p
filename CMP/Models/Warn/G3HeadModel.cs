using CMP.Constants.Enums;
using Newtonsoft.Json;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Globalization;

namespace CMP.Models.Warn
{
    public class G3HeadModel : BindableBase
    {
        private string? _lightFault;
        private string? _processInterlock;
        private string? _present;
        private string? _note;
        private string? _updateTime;

        public WorkstationType Workstation { get; set; }
        public string ModuleName { get; set; } = string.Empty;

        [JsonIgnore]
        public string? LightFault { get => _lightFault; set => SetProperty(ref _lightFault, value); }
        [JsonIgnore]
        public string? ProcessInterlock { get => _processInterlock; set => SetProperty(ref _processInterlock, value); }
        [JsonIgnore]
        public string? Present { get => _present; set => SetProperty(ref _present, value); }
        [JsonIgnore]
        public string? LastLightFaultFromPlc { get; set; }
        [JsonIgnore]
        public string? LastProcessInterlockFromPlc { get; set; }

        public string? Note { get => _note; set => SetProperty(ref _note, value); }
        public string? UpdateTime { get => _updateTime; set => SetProperty(ref _updateTime, value); }

        public bool IsLightFaultEditable => LightFaultAdress != 0;
        public bool IsProcessInterlockEditable => ProcessInterlockAdress != 0;
        public bool CanUpdate => LightFaultAdress != 0 || ProcessInterlockAdress != 0;

        public uint LightFaultAdress { get; set; }
        public uint ProcessInterlockAdress { get; set; }
        public uint PresentAdress { get; set; }
        public uint PresentDAdress { get; set; }
        public string? PresentWAdress { get; set; }

        public bool ShouldSerializePresentDAdress() => PresentDAdress != 0;
        public bool ShouldSerializePresentWAdress() => !string.IsNullOrWhiteSpace(PresentWAdress);

        public static bool TryParseW(string? text, out uint number)
        {
            number = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var value = text.Trim();
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                value = value[2..];
            }
            else if (value.StartsWith('W') || value.StartsWith('w'))
            {
                value = value[1..];
            }

            return uint.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out number) && number != 0;
        }
    }

    public class G3HeadGroup(WorkstationType workstation)
    {
        public WorkstationType Workstation { get; set; } = workstation;
        public ObservableCollection<G3HeadModel> Parts { get; set; } = [];
    }
}
