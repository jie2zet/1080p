namespace CMP.Models.Operation.Clean;

public class DryClean : BindableBase
{
    private Chamber _chamber = new();
    private Motor _chuckRotationMotor = new();
    private SweepMotor _armSwingMotor = new();
    private DryCylinder _cylinder = new();
    private DryGas _gas = new();
    private DryLiquid _liquid = new();

    public Chamber Chamber
    {
        get => _chamber;
        set => SetProperty(ref _chamber, value);
    }
    
    // 电机控制
    public Motor ChuckRotationMotor
    {
        get => _chuckRotationMotor;
        set => SetProperty(ref _chuckRotationMotor, value);
    }
    
    public SweepMotor ArmSwingMotor
    {
        get => _armSwingMotor;
        set => SetProperty(ref _armSwingMotor, value);
    }
    
    // 气缸控制
    public DryCylinder Cylinder
    {
        get => _cylinder;
        set => SetProperty(ref _cylinder, value);
    }
    
    // 气体控制
    public DryGas Gas
    {
        get => _gas;
        set => SetProperty(ref _gas, value);
    }
    
    // 液体控制
    public DryLiquid Liquid
    {
        get => _liquid;
        set => SetProperty(ref _liquid, value);
    }
}

/// <summary>
/// 气缸控制
/// </summary>
public class DryCylinder : BindableBase
{
    private bool _wfClamp;
    private bool _wfRelease;
    private bool _inletDoorOpen;
    private bool _outletDoorOpen;

    public bool WFClamp
    {
        get => _wfClamp;
        set => SetProperty(ref _wfClamp, value);
    }
    
    public bool WFRelease
    {
        get => _wfRelease;
        set => SetProperty(ref _wfRelease, value);
    }
    
    public bool InletDoorOpen
    {
        get => _inletDoorOpen;
        set => SetProperty(ref _inletDoorOpen, value);
    }
    
    public bool OutletDoorOpen
    {
        get => _outletDoorOpen;
        set => SetProperty(ref _outletDoorOpen, value);
    }
}

/// <summary>
/// 气体控制
/// </summary>
public class DryGas : BindableBase
{
    private bool _n2MixOutletOpen;
    private double _n2Flowrate;
    private bool _wfUpperFaceN2OutletOpen;
    private double _n2FlowrateWafer;
    private bool _wfLowerFaceN2Outlet1Open;
    private double _n2FlowrateLowerFace;
    private bool _n2ReleaseOpen;
    private bool _wfUpperFaceIPAOutletOpen;
    private bool _wfLowerFaceN2Outlet2Open;

    public bool N2MixOutletOpen
    {
        get => _n2MixOutletOpen;
        set => SetProperty(ref _n2MixOutletOpen, value);
    }
    
    public double N2Flowrate
    {
        get => _n2Flowrate;
        set => SetProperty(ref _n2Flowrate, value);
    }
    
    public bool WFUpperFaceN2OutletOpen
    {
        get => _wfUpperFaceN2OutletOpen;
        set => SetProperty(ref _wfUpperFaceN2OutletOpen, value);
    }
    
    public double N2FlowrateWafer
    {
        get => _n2FlowrateWafer;
        set => SetProperty(ref _n2FlowrateWafer, value);
    }
    
    public bool WFLowerFaceN2Outlet1Open
    {
        get => _wfLowerFaceN2Outlet1Open;
        set => SetProperty(ref _wfLowerFaceN2Outlet1Open, value);
    }
    
    public double N2FlowrateLowerFace
    {
        get => _n2FlowrateLowerFace;
        set => SetProperty(ref _n2FlowrateLowerFace, value);
    }
    
    public bool N2ReleaseOpen
    {
        get => _n2ReleaseOpen;
        set => SetProperty(ref _n2ReleaseOpen, value);
    }
    
    public bool WFUpperFaceIPAOutletOpen
    {
        get => _wfUpperFaceIPAOutletOpen;
        set => SetProperty(ref _wfUpperFaceIPAOutletOpen, value);
    }
    
    public bool WFLowerFaceN2Outlet2Open
    {
        get => _wfLowerFaceN2Outlet2Open;
        set => SetProperty(ref _wfLowerFaceN2Outlet2Open, value);
    }
}

/// <summary>
/// 液体控制
/// </summary>
public class DryLiquid : BindableBase
{
    private bool _wfUpperFaceDIWWashInlet2Open;
    private bool _wfUpperFaceDIWWashOutlet2Open;
    private double _wfUpperFaceDIWWashFlow2;
    private bool _ipaSupplyOpen;
    private bool _wfUpperFaceDIWWashInlet1Open;
    private bool _wfUpperFaceDIWWashOutlet1Open;
    private bool _wfLowerFaceDIWWashOpen;

    public bool WFUpperFaceDIWWashInlet2Open
    {
        get => _wfUpperFaceDIWWashInlet2Open;
        set => SetProperty(ref _wfUpperFaceDIWWashInlet2Open, value);
    }
    
    public bool WFUpperFaceDIWWashOutlet2Open
    {
        get => _wfUpperFaceDIWWashOutlet2Open;
        set => SetProperty(ref _wfUpperFaceDIWWashOutlet2Open, value);
    }
    
    public double WFUpperFaceDIWWashFlow2
    {
        get => _wfUpperFaceDIWWashFlow2;
        set => SetProperty(ref _wfUpperFaceDIWWashFlow2, value);
    }
    
    public bool IPASupplyOpen
    {
        get => _ipaSupplyOpen;
        set => SetProperty(ref _ipaSupplyOpen, value);
    }
    
    public bool WFUpperFaceDIWWashInlet1Open
    {
        get => _wfUpperFaceDIWWashInlet1Open;
        set => SetProperty(ref _wfUpperFaceDIWWashInlet1Open, value);
    }
    
    public bool WFUpperFaceDIWWashOutlet1Open
    {
        get => _wfUpperFaceDIWWashOutlet1Open;
        set => SetProperty(ref _wfUpperFaceDIWWashOutlet1Open, value);
    }
    
    public bool WFLowerFaceDIWWashOpen
    {
        get => _wfLowerFaceDIWWashOpen;
        set => SetProperty(ref _wfLowerFaceDIWWashOpen, value);
    }
}

