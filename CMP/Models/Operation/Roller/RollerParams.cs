using System.Reflection;
using static CMP.Constants.Plc.PlcHandle;

namespace CMP.Models.Operation.Roller;

/// <summary>
/// Roller 参数访问类，提供类似 PlcHandle.CmdPolish 的访问方式
/// </summary>
public class RollerParams
{
    public RollerParams(Type plcHandleType)
    {
        Motor = new MotorParams(plcHandleType);
        Liquid = new LiquidParams(plcHandleType);
        Pressure = new PressureParams(plcHandleType);
        Flush = new FlushParams(plcHandleType);
    }

    public MotorParams Motor { get; }
    public LiquidParams Liquid { get; }
    public PressureParams Pressure { get; }
    public FlushParams Flush { get; }

    public class FlushParams
    {
        public FlushParams(Type plcHandleType)
        {
            var flushType = plcHandleType.GetNestedType("Flush")!;
            var paramType = flushType.GetNestedType("Param")!;
            var field = paramType.GetField("OpenDelay")!;
            var attr = field.GetCustomAttribute<ParamAttribute>()!;
            OpenDelay = new FloatParam(attr, "Flush.Param.OpenDelay");
        }

        public FloatParam OpenDelay { get; }
    }

    public class FloatParam
    {
        public FloatParam(ParamAttribute attr, string name)
        {
            Handle = attr.Handle;
            MinValue = attr.MinValue;
            MaxValue = attr.MaxValue;
            DefaultValue = attr.DefaultValue;
            Description = attr.Description;
            Label = attr.Label;
            Unit = attr.Unit;
            Name = name;
        }

        public uint Handle { get; }
        public int MinValue { get; }
        public int MaxValue { get; }
        public int DefaultValue { get; }
        public string Description { get; }
        public string Label { get; }
        public string Unit { get; }
        public string Name { get; }
    }

    /// <summary>
    /// Motor 参数访问类
    /// </summary>
    public class MotorParams
    {
        public MotorParams(Type plcHandleType)
        {
            var motorType = plcHandleType.GetNestedType("Motor")!;
            WfLeftPillarRotate = new MotorVelocityParam(motorType, "WfLeftPillarRotate");
            WfRightPillarRotate = new MotorVelocityParam(motorType, "WfRightPillarRotate");
            UpperRollerRotate = new MotorVelocityParam(motorType, "UpperRollerRotate");
            LowerRollerRotate = new MotorVelocityParam(motorType, "LowerRollerRotate");
        }

        public MotorVelocityParam WfLeftPillarRotate { get; }
        public MotorVelocityParam WfRightPillarRotate { get; }
        public MotorVelocityParam UpperRollerRotate { get; }
        public MotorVelocityParam LowerRollerRotate { get; }
    }

    /// <summary>
    /// Liquid 参数访问类
    /// </summary>
    public class LiquidParams
    {
        public LiquidParams(Type plcHandleType)
        {
            var liquidType = plcHandleType.GetNestedType("Liquid")!;
            ChemicalEnterFlow = new LiquidFlowrateParam(liquidType, "ChemicalEnterFlow");
            DiwEnterFlow = new LiquidFlowrateParam(liquidType, "DiwEnterFlow");
            WfUpperFaceWashDiwFlow = new LiquidFlowrateParam(liquidType, "WfUpperFaceWashDiwFlow");
            WfLowerFaceDiwWashFlow = new LiquidFlowrateParam(liquidType, "WfLowerFaceDiwWashFlow");
        }

        public LiquidFlowrateParam ChemicalEnterFlow { get; }
        public LiquidFlowrateParam DiwEnterFlow { get; }
        public LiquidFlowrateParam WfUpperFaceWashDiwFlow { get; }
        public LiquidFlowrateParam WfLowerFaceDiwWashFlow { get; }
    }

    /// <summary>
    /// Pressure 参数访问类
    /// </summary>
    public class PressureParams
    {
        public PressureParams(Type plcHandleType)
        {
            var cylinderType = plcHandleType.GetNestedType("Cylinder")!;
            LowerRollerPressure = new PressureSetParam(cylinderType, "LowerRollerPressure");
            UpperRollerPressure = new PressureSetParam(cylinderType, "UpperRollerPressure");
            ClampPressure = new PressureSetParam(cylinderType, "ClampPressure");
        }

        public PressureSetParam LowerRollerPressure { get; }
        public PressureSetParam UpperRollerPressure { get; }
        public PressureSetParam ClampPressure { get; }
    }

    /// <summary>
    /// 电机速度参数
    /// </summary>
    public class MotorVelocityParam
    {
        private readonly int _velocityValue;
        private readonly uint _velocityHandle;
        private readonly int _minValue;
        private readonly int _maxValue;
        private readonly int _defaultValue;
        private readonly string _description;
        private readonly string _label;
        private readonly string _unit;
        private readonly string _name;

        public MotorVelocityParam(Type motorType, string motorName)
        {
            var motorSubType = motorType.GetNestedType(motorName)!;
            var paramType = motorSubType.GetNestedType("Param")!;
            var velocityField = paramType.GetField("Velocity")!;
            
            _velocityValue = (int)velocityField.GetValue(null)!;
            
            var attr = velocityField.GetCustomAttribute<ParamAttribute>()!;
            _velocityHandle = attr.Handle;
            _minValue = attr.MinValue;
            _maxValue = attr.MaxValue;
            _defaultValue = attr.DefaultValue;
            _description = attr.Description;
            _label = attr.Label;
            _unit = attr.Unit;
            _name = $"{motorName}.Param.Velocity";
        }

        public int Value => _velocityValue;
        public uint Handle => _velocityHandle;
        public int MinValue => _minValue;
        public int MaxValue => _maxValue;
        public int DefaultValue => _defaultValue;
        public string Description => _description;
        public string Label => _label;
        public string Unit => _unit;
        public string Name => _name;
    }

    /// <summary>
    /// 液体流量参数
    /// </summary>
    public class LiquidFlowrateParam
    {
        private readonly int _flowrateSetValue;
        private readonly uint _flowrateSetHandle;
        private readonly int _minValue;
        private readonly int _maxValue;
        private readonly int _defaultValue;
        private readonly string _description;
        private readonly string _label;
        private readonly string _unit;
        private readonly string _name;

        public LiquidFlowrateParam(Type liquidType, string liquidName)
        {
            var liquidSubType = liquidType.GetNestedType(liquidName)!;
            var paramType = liquidSubType.GetNestedType("Param")!;
            var flowrateSetField = paramType.GetField("FlowrateSet")!;
            
            _flowrateSetValue = (int)flowrateSetField.GetValue(null)!;
            
            var attr = flowrateSetField.GetCustomAttribute<ParamAttribute>()!;
            _flowrateSetHandle = attr.Handle;
            _minValue = attr.MinValue;
            _maxValue = attr.MaxValue;
            _defaultValue = attr.DefaultValue;
            _description = attr.Description;
            _label = attr.Label;
            _unit = attr.Unit;
            _name = $"{liquidName}.Param.FlowrateSet";
        }

        public int Value => _flowrateSetValue;
        public uint Handle => _flowrateSetHandle;
        public int MinValue => _minValue;
        public int MaxValue => _maxValue;
        public int DefaultValue => _defaultValue;
        public string Description => _description;
        public string Label => _label;
        public string Unit => _unit;
        public string Name => _name;
    }

    /// <summary>
    /// 压力设定参数
    /// </summary>
    public class PressureSetParam
    {
        private readonly int _pressureSetValue;
        private readonly uint _pressureSetHandle;
        private readonly int _minValue;
        private readonly int _maxValue;
        private readonly int _defaultValue;
        private readonly string _description;
        private readonly string _label;
        private readonly string _unit;
        private readonly string _name;

        public PressureSetParam(Type cylinderType, string pressureName)
        {
            var pressureSubType = cylinderType.GetNestedType(pressureName)!;
            var paramType = pressureSubType.GetNestedType("Param")!;
            var pressureSetField = paramType.GetField("PressureSet")!;
            
            _pressureSetValue = (int)pressureSetField.GetValue(null)!;
            
            var attr = pressureSetField.GetCustomAttribute<ParamAttribute>()!;
            _pressureSetHandle = attr.Handle;
            _minValue = attr.MinValue;
            _maxValue = attr.MaxValue;
            _defaultValue = attr.DefaultValue;
            _description = attr.Description;
            _label = attr.Label;
            _unit = attr.Unit;
            _name = $"{pressureName}.Param.PressureSet";
        }

        public int Value => _pressureSetValue;
        public uint Handle => _pressureSetHandle;
        public int MinValue => _minValue;
        public int MaxValue => _maxValue;
        public int DefaultValue => _defaultValue;
        public string Description => _description;
        public string Label => _label;
        public string Unit => _unit;
        public string Name => _name;
    }
}
