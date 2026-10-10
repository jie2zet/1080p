using System.Globalization;
using CMP.Constants.Plc;
using CMP.Lang;
using Prism.Commands;
using Prism.Mvvm;
using WPFLocalizeExtension.Engine;

namespace CMP.ViewModels.Tabs.OperationTab;

public sealed class LiquidParameterViewModel : BindableBase
{
    private readonly Action<LiquidParameterViewModel> _write;
    private string _inputValue;
    private string _name;

    public LiquidParameterViewModel(PlcHandle.ParamAttribute metadata, Action<LiquidParameterViewModel> write)
    {
        Metadata = metadata;
        _write = write;
        _name = GetText(metadata.DisplayNameKey, metadata.Label);
        _inputValue = metadata.NumericDefaultValue.ToString(Format, CultureInfo.CurrentCulture);
        SetCommand = new DelegateCommand(() => _write(this));
    }

    public PlcHandle.ParamAttribute Metadata { get; }
    public uint Handle => Metadata.Handle;
    public double MinValue => Metadata.NumericMinValue;
    public double MaxValue => Metadata.NumericMaxValue;
    public int DecimalPlaces => Metadata.DecimalPlaces;
    public string Format => $"F{DecimalPlaces}";
    public string DrawingTag => Metadata.DrawingTag;
    public string Unit => Metadata.Unit;
    public string Address => $"{Metadata.ZrZf}{Metadata.Handle}";
    public int WordCount => Metadata.WordCount;
    public string EnglishName => GetText(Metadata.DisplayNameKey, Metadata.Label, CultureInfo.GetCultureInfo("en"));
    public string ChineseName => GetText(Metadata.DisplayNameKey, Metadata.Label, CultureInfo.GetCultureInfo("zh"));
    public string DefaultValueText => Metadata.NumericDefaultValue.ToString(Format, CultureInfo.CurrentCulture);
    public string RangeText => $"{MinValue.ToString(Format, CultureInfo.CurrentCulture)} – {MaxValue.ToString(Format, CultureInfo.CurrentCulture)}";

    public string Name
    {
        get => _name;
        private set => SetProperty(ref _name, value);
    }

    public string InputValue
    {
        get => _inputValue;
        set => SetProperty(ref _inputValue, value);
    }

    public DelegateCommand SetCommand { get; }

    public bool TryGetValue(out double value) =>
        (double.TryParse(InputValue, NumberStyles.Float, CultureInfo.CurrentCulture, out value)
         || double.TryParse(InputValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        && double.IsFinite(value);

    public void RefreshLocalization()
    {
        Name = GetText(Metadata.DisplayNameKey, Metadata.Label);
        RaisePropertyChanged(nameof(DefaultValueText));
        RaisePropertyChanged(nameof(RangeText));
    }

    private static string GetText(string key, string fallback) =>
        GetText(key, fallback, LocalizeDictionary.Instance.Culture);

    private static string GetText(string key, string fallback, CultureInfo culture) =>
        string.IsNullOrWhiteSpace(key)
            ? fallback
            : Resources.ResourceManager.GetString(key, culture) ?? fallback;
}
