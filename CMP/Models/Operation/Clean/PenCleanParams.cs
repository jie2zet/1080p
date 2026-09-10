using System.Reflection;
using static CMP.Constants.Plc.PlcHandle;

namespace CMP.Models.Operation.Clean;

/// <summary>
/// PenClean 参数访问类，提供类似 PlcHandle.CmdPenA1 的访问方式
/// </summary>
public class PenCleanParams
{
    public PenCleanParams(Type plcHandleType)
    {
        Motor = new MotorParams(plcHandleType);
        Cylinder = new CylinderParams(plcHandleType);
        Gas = new GasParams(plcHandleType);
        Liquid = new LiquidParams(plcHandleType);
        Flash = new FlashParams(plcHandleType);
    }

    public MotorParams Motor { get; }
    public CylinderParams Cylinder { get; }
    public GasParams Gas { get; }
    public LiquidParams Liquid { get; }
    public FlashParams Flash { get; }

    public class FlashParams
    {
        public FlashParams(Type plcHandleType)
        {
            var flashType = plcHandleType.GetNestedType("Flash")!;
            var paramType = flashType.GetNestedType("Param")!;
            var field = paramType.GetField("OpenDelay")!;
            var attr = field.GetCustomAttribute<ParamAttribute>()!;
            OpenDelay = new FloatParam(attr, "Flash.Param.OpenDelay");
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
            ChuckRotation = new MotorVelocityParam(motorType, "ChuckRotation");
            PenArmSweep = new MotorVelocityPositionParam(motorType, "PenArmSweep");
            PenRotation = new MotorVelocityParam(motorType, "PenRotation");
        }

        public MotorVelocityParam ChuckRotation { get; }
        public MotorVelocityPositionParam PenArmSweep { get; }
        public MotorVelocityParam PenRotation { get; }
    }

    /// <summary>
    /// Cylinder 参数访问类
    /// </summary>
    public class CylinderParams
    {
        public CylinderParams(Type plcHandleType)
        {
            var cylinderType = plcHandleType.GetNestedType("Cylinder")!;
            PenPressure = new PressureSetParam(cylinderType, "PenPressure");
        }

        public PressureSetParam PenPressure { get; }
    }

    /// <summary>
    /// Gas 参数访问类
    /// </summary>
    public class GasParams
    {
        public GasParams(Type plcHandleType)
        {
            var gasType = plcHandleType.GetNestedType("Gas")!;
            N2Flow = new FlowrateSetParam(gasType, "N2Flow");
            CO2Pressure = new PressureSetParam(gasType, "CO2Pressure");
        }

        public FlowrateSetParam N2Flow { get; }
        public PressureSetParam CO2Pressure { get; }
    }

    /// <summary>
    /// Liquid 参数访问类
    /// </summary>
    public class LiquidParams
    {
        public LiquidParams(Type plcHandleType)
        {
            var liquidType = plcHandleType.GetNestedType("Liquid")!;
            ChemicalFlow = new FlowrateSetParam(liquidType, "ChemicalFlow");
            DIWFlow = new FlowrateSetParam(liquidType, "DIWFlow");
            WFUpperFaceDIWFlow = new FlowrateSetParam(liquidType, "WFUpperFaceDIWFlow");
            WFLowerFaceDIWFlow = new FlowrateSetParam(liquidType, "WFLowerFaceDIWFlow");
            DIWPressure = new PressureSetParam(liquidType, "DIWPressure");
        }

        public FlowrateSetParam ChemicalFlow { get; }
        public FlowrateSetParam DIWFlow { get; }
        public FlowrateSetParam WFUpperFaceDIWFlow { get; }
        public FlowrateSetParam WFLowerFaceDIWFlow { get; }
        public PressureSetParam DIWPressure { get; }
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
    /// 电机速度+位置参数
    /// </summary>
    public class MotorVelocityPositionParam
    {
        private readonly uint _velocityHandle;
        private readonly uint _positionHandle;

        public MotorVelocityPositionParam(Type motorType, string motorName)
        {
            var motorSubType = motorType.GetNestedType(motorName)!;
            var paramType = motorSubType.GetNestedType("Param")!;
            
            var velocityField = paramType.GetField("Velocity")!;
            var velocityAttr = velocityField.GetCustomAttribute<ParamAttribute>()!;
            _velocityHandle = velocityAttr.Handle;
            
            var positionField = paramType.GetField("Position")!;
            var positionAttr = positionField.GetCustomAttribute<ParamAttribute>()!;
            _positionHandle = positionAttr.Handle;
        }

        public uint VelocityHandle => _velocityHandle;
        public uint PositionHandle => _positionHandle;
    }

    /// <summary>
    /// 流量设定参数
    /// </summary>
    public class FlowrateSetParam
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

        public FlowrateSetParam(Type parentType, string fieldName)
        {
            var subType = parentType.GetNestedType(fieldName)!;
            var paramType = subType.GetNestedType("Param")!;
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
            _name = $"{fieldName}.Param.FlowrateSet";
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

        public PressureSetParam(Type parentType, string fieldName)
        {
            var subType = parentType.GetNestedType(fieldName)!;
            var paramType = subType.GetNestedType("Param")!;
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
            _name = $"{fieldName}.Param.PressureSet";
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
