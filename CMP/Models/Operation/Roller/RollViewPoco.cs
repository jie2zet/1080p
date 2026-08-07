namespace CMP.Models.Operation.Roller;

public class RollViewPoco
{
    // Motors
    public Motor WfLeftPillarRotateMotor { get; set; } = new();
    public Motor WfRightPillarRotateMotor { get; set; } = new();
    public Motor UpperRollerRotateMotor { get; set; } = new();
    public Motor LowerRollerRotateMotor { get; set; } = new();

    // Cylinders
    public Cylinder LowerRollerCylinder { get; set; } = new();
    public Cylinder UpperRollerCylinder { get; set; } = new();
    public Cylinder ArmUpDownCylinder { get; set; } = new();
    public Cylinder ArmLateralMovementCylinder { get; set; } = new();
    public Cylinder ClampCylinder { get; set; } = new();
    public Cylinder UltrasonicNozzleCylinder { get; set; } = new();
    public Cylinder WaferInletDoorCylinder { get; set; } = new();
    public Cylinder ArmDoorCylinder { get; set; } = new();
    public Cylinder WaferOutletDoorCylinder { get; set; } = new();

    // Pressures
    public Pressure LowerRollerPressure { get; set; } = new();
    public Pressure UpperRollerPressure { get; set; } = new();
    public Pressure ClampPressure { get; set; } = new();

    // Liquids
    public LiquidControl ChemicalEnterMainValve { get; set; } = new();
    public LiquidFlow ChemicalEnterFlow { get; set; } = new();
    public LiquidControl PipeDiwWash { get; set; } = new();
    public LiquidControl DiwEnterMainValve { get; set; } = new();
    public LiquidFlow DiwEnterFlow { get; set; } = new();
    public LiquidControl WfLowerFaceChemicalWash { get; set; } = new();
    public LiquidControl WfUpperFaceChemicalWash { get; set; } = new();
    public LiquidControl QuartzChemicalWash { get; set; } = new();
    public LiquidControl WfUpperFaceDiwWashInlet { get; set; } = new();
    public LiquidControl WfUpperFaceDiwWashOutlet { get; set; } = new();
    public LiquidFlow WfUpperFaceWashDiwFlow { get; set; } = new();
    public LiquidControl WfLowerFaceDiwWash { get; set; } = new();
    public LiquidFlow WfLowerFaceDiwWashFlow { get; set; } = new();
    public LiquidControl UpperRollerDiwWash { get; set; } = new();
    public LiquidControl QuartzDiwWash { get; set; } = new();
    public LiquidControl UpperRollerInsideDiwWash { get; set; } = new();
    public LiquidControl LowerRollerInsideDiwWash { get; set; } = new();
    public LiquidControl MageSonicDiwWash { get; set; } = new();
    public LiquidControl ChamberDiwWash { get; set; } = new();
    public LiquidControl PillarWash { get; set; } = new();
    public LiquidControl CoolingWaterCirculation { get; set; } = new();


    public class Cylinder
    {
        public string Name { get; set; } = string.Empty;
        public bool IsUp { get; set; }
        public bool IsDown { get; set; }
        public bool IsOpen { get; set; }
        public bool IsClosed { get; set; }
        public bool IsWaiting { get; set; }
        public bool IsCleaning { get; set; }
        public bool IsReleased { get; set; }
        public bool IsClamped { get; set; }
    }

    public class Pressure
    {
        public string Name { get; set; } = string.Empty;
        public double SetPressure { get; set; }
        public double ActPressure { get; set; }
        public bool IsPressed { get; set; }
        public bool IsReset { get; set; }
        public bool IsCalibrated { get; set; }
    }

    public class LiquidControl
    {
        public string Name { get; set; } = string.Empty;
        public bool IsOpen { get; set; }
        public bool IsClosed { get; set; } = true;
    }

    public class LiquidFlow : LiquidControl
    {
        public double SetFlowrate { get; set; }
        public double ActFlowrate { get; set; }
        public bool IsReset { get; set; }
        public bool IsCalibrated { get; set; }
    }
}
