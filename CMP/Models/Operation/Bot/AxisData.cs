namespace CMP.Models.Operation.Bot;

public class AxisData : BindableBase
{
    private int _velocity;
    public int Velocity
    {
        get => _velocity;
        set => SetProperty(ref _velocity, value);
    }

    private int _position = 1;
    public int Position
    {
        get => _position;
        set => SetProperty(ref _position, value);
    }

    private State _serverState = State.Off;
    public State ServerState
    {
        get => _serverState;
        set => SetProperty(ref _serverState, value);
    }

    private int _currentPosition;
    public int CurrentPosition
    {
        get => _currentPosition;
        set => SetProperty(ref _currentPosition, value);
    }

    private int _selectedStationPosition;
    public int SelectedStationPosition
    {
        get => _selectedStationPosition==0?_currentPosition:_selectedStationPosition;
        set => SetProperty(ref _selectedStationPosition, value);
    }
    
    private string _moduleName = string.Empty;
    public string ModuleName
    {
        get => _moduleName;
        set => SetProperty(ref _moduleName, value);
    }
}