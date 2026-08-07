using Microsoft.Identity.Client;

namespace CMP.Models.Operation.RoboticArm;

public class RoboticArm : BindableBase
{
    private AutoMode _autoMode;
    private Arm _arm;
    private Action _action;
    private Position _position;
    private int _slot = 1;
    private string _modelStatus;
    private string _armStatus;
    public int ArmId { get; set; }
    public string PLCAddress { get; set; }

    public RoboticArm() { }
    public RoboticArm(int armId,string plcAddress)
    {
        ArmId = armId;
        PLCAddress = plcAddress;
    }
    public AutoMode AutoMode
    {
        get => _autoMode;
        set => SetProperty(ref _autoMode, value);
    }
    
    public Arm Arm
    {
        get => _arm;
        set => SetProperty(ref _arm, value);
    }
    
    public Action Action
    {
        get => _action;
        set => SetProperty(ref _action, value);
    }
    
    public Position Position
    {
        get => _position;
        set => SetProperty(ref _position, value);
    }
    
    public int Slot
    {
        get => _slot;
        set => SetProperty(ref _slot, value);
    }
    public string ModelStatus
    {
        get => _modelStatus;
        set => SetProperty(ref _modelStatus ,value);
    }
    public string ArmStatus
    {
        get => _armStatus;
        set => SetProperty(ref _armStatus, value);
    }
}