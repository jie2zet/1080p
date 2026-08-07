using Prism.Mvvm;

namespace CMP.Models.Operation.Polish;

public class Chamber : BindableBase
{
    private string? _load;
    private string? _polish;
    private string? _condition;
    private string? _unload;

    public string? Load
    {
        get => _load;
        set => SetProperty(ref _load, value);
    }

    public string? Polish
    {
        get => _polish;
        set => SetProperty(ref _polish, value);
    }

    public string? Condition
    {
        get => _condition;
        set => SetProperty(ref _condition, value);
    }

    public string? Unload
    {
        get => _unload;
        set => SetProperty(ref _unload, value);
    }
}