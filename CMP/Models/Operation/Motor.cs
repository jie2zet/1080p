namespace CMP.Models.Operation;

public class Motor : BindableBase
{

    private float _setVelocity;
    private float _actVelocity;
    private float _confirmedVelocity;
    private float _jogVelocity;
    private float _actPosition;
    private float _setTorque;
    private float _actTorque;
    private bool _isServoOn = false;
    public float ActVelocity
    {
        get => _actVelocity;
        set => SetProperty(ref _actVelocity, value);
    }
    public float SetVelocity
    {
        get => _setVelocity;
        set => SetProperty(ref _setVelocity, value);
    }
    public float ConfirmedVelocity
    {
        get => _confirmedVelocity;
        set => SetProperty(ref _confirmedVelocity, value);
    }
    public float JogVelocity
    {
        get => _jogVelocity;
        set => SetProperty(ref _jogVelocity, value);
    }
    public float ActPosition
    {
        get => _actPosition;
        set => SetProperty(ref _actPosition, value);
    }
    public float SetTorque
    {
        get => _setTorque;
        set => SetProperty(ref _setTorque, value);
    }
    public float ActTorque
    {
        get => _actTorque;
        set => SetProperty(ref _actTorque, value);
    }
    public bool IsServoOn
    {
        get => _isServoOn;
        set => SetProperty(ref _isServoOn, value);
    }
}
    
public class SweepMotor : Motor
{
    private float _setPosition;
    public float SetPosition
    {
        get => _setPosition;
        set => SetProperty(ref _setPosition, value);
    }
}
