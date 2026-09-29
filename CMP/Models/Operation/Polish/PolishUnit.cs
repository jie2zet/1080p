using System.Collections.ObjectModel;
using Prism.Mvvm;

namespace CMP.Models.Operation.Polish;

public class PolishUnit : BindableBase
{
    private Chamber _chamber = new();
    private Slurry _slurry = new();
    private Liquid _liquid = new();
    private Motor _platenMotor = new();
    private Motor _headRotateMotor = new();
    private Motor _diskRotateMotor = new();
    private SweepMotor _slurrySweepMotor = new();
    private SweepMotor _headSweepMotor = new();
    private SweepMotor _conditionSweepMotor = new();
    private SweepMotor _headLiftMotor = new();
    private ConditionZone _zoneCondition = new();

    public Chamber Chamber
    {
        get => _chamber;
        set => SetProperty(ref _chamber, value);
    }

    public Slurry Slurry
    {
        get => _slurry;
        set => SetProperty(ref _slurry, value);
    }

    public Liquid Liquid
    {
        get => _liquid;
        set => SetProperty(ref _liquid, value);
    }

    public Motor PlatenMotor
    {
        get => _platenMotor;
        set => SetProperty(ref _platenMotor, value);
    }

    public Motor HeadRotateMotor
    {
        get => _headRotateMotor;
        set => SetProperty(ref _headRotateMotor, value);
    }

    public Motor DiskRotateMotor
    {
        get => _diskRotateMotor;
        set => SetProperty(ref _diskRotateMotor, value);
    }

    public SweepMotor SlurrySweepMotor
    {
        get => _slurrySweepMotor;
        set => SetProperty(ref _slurrySweepMotor, value);
    }

    public SweepMotor HeadSweepMotor
    {
        get => _headSweepMotor;
        set => SetProperty(ref _headSweepMotor, value);
    }

    public SweepMotor ConditionSweepMotor
    {
        get => _conditionSweepMotor;
        set => SetProperty(ref _conditionSweepMotor, value);
    }

    public SweepMotor HeadLiftMotor
    {
        get => _headLiftMotor;
        set => SetProperty(ref _headLiftMotor, value);
    }

    public ConditionZone ZoneCondition
    {
        get => _zoneCondition;
        set => SetProperty(ref _zoneCondition, value);
    }

    public class ConditionZone : BindableBase
    {
        private float _actPress;
        private float _setPress;

        public float ActPress
        {
            get => _actPress;
            set => SetProperty(ref _actPress, value);
        }

        public float SetPress
        {
            get => _setPress;
            set => SetProperty(ref _setPress, value);
        }
    }

    public class ZoneGas : BindableBase
    {
        private string _name = string.Empty;
        private int _vent;
        private float _actVacuum;
        private int _press;
        private float _actPress;
        private float _setPress;
        private int _vacuum;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public int Vent
        {
            get => _vent;
            set => SetProperty(ref _vent, value);
        }

        public float ActVacuum
        {
            get => _actVacuum;
            set => SetProperty(ref _actVacuum, value);
        }
        
        public int Vacuum
        {
            get => _vacuum;
            set => SetProperty(ref _vacuum, value);
        }
        
        public int Press
        {
            get => _press;
            set => SetProperty(ref _press, value);
        }

        public float ActPress
        {
            get => _actPress;
            set => SetProperty(ref _actPress, value);
        }

        public float SetPress
        {
            get => _setPress;
            set => SetProperty(ref _setPress, value);
        }
    }

    private ObservableCollection<ZoneGas> _zones;
    private ZoneGas _zone1;
    private ZoneGas _zone2;
    private ZoneGas _zone3;
    private ZoneGas _zone4;
    private ZoneGas _zone5;
    private ZoneGas _zone6;
    private ZoneGas _zone7;
    private ZoneGas _zone8;
    private ZoneGas _retain;

    public ObservableCollection<ZoneGas> Zones
    {
        get => _zones;
        set => SetProperty(ref _zones, value);
    }

    public ZoneGas Zone1
    {
        get => _zone1;
        set => SetProperty(ref _zone1, value);
    }

    public ZoneGas Zone2
    {
        get => _zone2;
        set => SetProperty(ref _zone2, value);
    }

    public ZoneGas Zone3
    {
        get => _zone3;
        set => SetProperty(ref _zone3, value);
    }

    public ZoneGas Zone4
    {
        get => _zone4;
        set => SetProperty(ref _zone4, value);
    }

    public ZoneGas Zone5
    {
        get => _zone5;
        set => SetProperty(ref _zone5, value);
    }

    public ZoneGas Zone6
    {
        get => _zone6;
        set => SetProperty(ref _zone6, value);
    }

    public ZoneGas Zone7
    {
        get => _zone7;
        set => SetProperty(ref _zone7, value);
    }

    public ZoneGas Zone8
    {
        get => _zone8;
        set => SetProperty(ref _zone8, value);
    }

    public ZoneGas Retain
    {
        get => _retain;
        set => SetProperty(ref _retain, value);
    }

    public PolishUnit()
    {
        _zone1 = new ZoneGas { Name = "Zone1" };
        _zone2 = new ZoneGas { Name = "Zone2" };
        _zone3 = new ZoneGas { Name = "Zone3" };
        _zone4 = new ZoneGas { Name = "Zone4" };
        _zone5 = new ZoneGas { Name = "Zone5" };
        _zone6 = new ZoneGas { Name = "Zone6" };
        _zone7 = new ZoneGas { Name = "Zone7" };
        _zone8 = new ZoneGas { Name = "Zone8" };
        _retain = new ZoneGas { Name = "Retain" };
        _zones = new ObservableCollection<ZoneGas> { _zone1, _zone2, _zone3, _zone4, _zone5, _zone6, _zone7, _zone8, _retain };
    }
}
