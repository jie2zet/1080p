using Prism.Mvvm;

namespace CMP.Models.DataCenter
{
    public class SV : BindableBase
    {
        public int SVID { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        private object? _value;
        public object? Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public string Unit { get; set; } = string.Empty;

        public double Max { get; set; }

        public double Min { get; set; }

        public string Module { get; set; } = string.Empty;

        public string Descripition { get; set; } = string.Empty;

        public string Comment { get; set; } = string.Empty;
    }
}
