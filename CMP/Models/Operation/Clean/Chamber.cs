namespace CMP.Models.Operation.Clean;

public class Chamber : BindableBase
{
    private string? _pen;
    
    public string? Pen
    {
        get => _pen;
        set => SetProperty(ref _pen, value);
    }
}