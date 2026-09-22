namespace CMP.Models.Operation.Clean;

public class PenClean
{
    public Chamber Chamber { get; set; } = new();
    
    // 电机控制
    public Motor ChuckRotationMotor { get; set; } = new();
    public SweepMotor PenArmSweepMotor { get; set; } = new();
    public Motor PenRotationMotor { get; set; } = new();
    
    // 气缸控制
    public PenCylinder PenCylinder { get; set; } = new();
    
    // 压力控制
    public PressureControl PenPressure { get; set; } = new();
    public FlowControl N2Flow { get; set; } = new();
    public PressureControl CO2Pressure { get; set; } = new();
    public PressureControl DIWPressure { get; set; } = new();
    
    // 液体流量控制
    public FlowControl ChemicalFlow { get; set; } = new();
    public FlowControl DIWFlow { get; set; } = new();
    public FlowControl WFUpperDIWFlow { get; set; } = new();
    public FlowControl WFLowerDIWFlow { get; set; } = new();
    
    // 气体控制
    public PenGas Gas { get; set; } = new();
    
    // 液体控制
    public PenLiquid Liquid { get; set; } = new();

    public FlushControl Flush { get; set; } = new();
}

/// <summary>
/// 气缸控制
/// </summary>
public class PenCylinder
{
    public bool PenUpDownUp { get; set; }
    public bool PenUpDownDown { get; set; }
    public double PenPressure { get; set; }
    public bool PenArmUp { get; set; }
    public bool PenArmDown { get; set; }
    public bool WFClamp { get; set; }
    public bool WFRelease { get; set; }
    public bool NozzleUp { get; set; }
    public bool NozzleDown { get; set; }
    public bool InletDoorOpen { get; set; }
    public bool OutletDoorOpen { get; set; }
}

/// <summary>
/// 气体控制
/// </summary>
public class PenGas
{
    public bool WFUpperN2OutletOpen { get; set; }
    public double N2Flowrate { get; set; }
    public bool CO2SupplyOpen { get; set; }
    public double CO2Pressure { get; set; }
    public bool CUPPurgeOpen { get; set; }
    public bool WFLowerN2OutletOpen { get; set; }
}

/// <summary>
/// 液体控制
/// </summary>
public class PenLiquid
{
    public bool ChemicalInletOpen { get; set; }
    public bool ChemicalOutletOpen { get; set; }
    public double ChemicalFlowrate { get; set; }
    public bool DIWInletOpen { get; set; }
    public bool DIWOutletOpen { get; set; }
    public double DIWFlowrate { get; set; }
    public bool WFUpperFaceDIWWashOpen { get; set; }
    public double WFUpperFaceDIWFlowrate { get; set; }
    public bool WFLowerFaceDIWWashOpen { get; set; }
    public double WFLowerFaceDIWFlowrate { get; set; }
    public bool DIWSupplyOpen { get; set; }
    public double DIWPressure { get; set; }
    public bool PenDIWWashOpen { get; set; }
    public bool QuartzDIWWashOpen { get; set; }
    public bool ChemicalPipelineWashOpen { get; set; }
    public bool QuartzChemicalWashOpen { get; set; }
    public bool WFUpperLowerChemicalWashOpen { get; set; }
    public bool WFUpperChemicalWashOpen { get; set; }
    public bool DIWChamberOpen { get; set; }
    public bool WFDI_CO2WashOpen { get; set; }
}

/// <summary>
/// 压力控制
/// </summary>
public class PressureControl : BindableBase
{
    private float _setPress;
    private float _actPress;
    
    public float SetPress
    {
        get => _setPress;
        set => SetProperty(ref _setPress, value);
    }
    
    public float ActPress
    {
        get => _actPress;
        set => SetProperty(ref _actPress, value);
    }
}

/// <summary>
/// 流量控制
/// </summary>
public class FlowControl : BindableBase
{
    private float _setFlowrate;
    private float _actFlowrate;
    
    public float SetFlowrate
    {
        get => _setFlowrate;
        set => SetProperty(ref _setFlowrate, value);
    }
    
    public float ActFlowrate
    {
        get => _actFlowrate;
        set => SetProperty(ref _actFlowrate, value);
    }
}

/// <summary>
/// 全纯水阀 Flush 控制
/// </summary>
public class FlushControl : BindableBase
{
    private float _openDelaySet;
    private float _openDelayAct;

    public float OpenDelaySet
    {
        get => _openDelaySet;
        set => SetProperty(ref _openDelaySet, value);
    }

    public float OpenDelayAct
    {
        get => _openDelayAct;
        set => SetProperty(ref _openDelayAct, value);
    }
}







