using Prism.Mvvm;

namespace CMP.Models.Operation.Polish;

public class Slurry : BindableBase
{
    private float _slurry1Set;
    private float _slurry2Set;
    private float _slurry3Set;
    private float _slurry4Set;

    private float _slurry1Act;
    private float _slurry2Act;
    private float _slurry3Act;
    private float _slurry4Act;

    public float Slurry1Set
    {
        get => _slurry1Set;
        set => SetProperty(ref _slurry1Set, value);
    }

    public float Slurry2Set
    {
        get => _slurry2Set;
        set => SetProperty(ref _slurry2Set, value);
    }

    public float Slurry3Set
    {
        get => _slurry3Set;
        set => SetProperty(ref _slurry3Set, value);
    }

    public float Slurry4Set
    {
        get => _slurry4Set;
        set => SetProperty(ref _slurry4Set, value);
    }

    public float Slurry1Act
    {
        get => _slurry1Act;
        set => SetProperty(ref _slurry1Act, value);
    }

    public float Slurry2Act
    {
        get => _slurry2Act;
        set => SetProperty(ref _slurry2Act, value);
    }

    public float Slurry3Act
    {
        get => _slurry3Act;
        set => SetProperty(ref _slurry3Act, value);
    }

    public float Slurry4Act
    {
        get => _slurry4Act;
        set => SetProperty(ref _slurry4Act, value);
    }
}