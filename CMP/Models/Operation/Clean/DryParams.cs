using System.Reflection;
using static CMP.Constants.Plc.PlcHandle;

namespace CMP.Models.Operation.Clean;

/// <summary>
/// Dry 参数访问类，提供类似 PlcHandle.CmdDryA 的访问方式
/// </summary>
public class DryParams
{
    public DryParams(Type plcHandleType)
    {
        Motor = new MotorParams(plcHandleType);
        Gas = new GasParams(plcHandleType);
        Liquid = new LiquidParams(plcHandleType);
    }

    public MotorParams Motor { get; }
    public GasParams Gas { get; }
    public LiquidParams Liquid { get; }

    /// <summary>
    /// Motor 参数访问类
    /// </summary>
    public class MotorParams
    {
        public MotorParams(Type plcHandleType)
        {
            var motorType = plcHandleType.GetNestedType("Motor")!;
            ChuckRotation = new MotorVelocityParam(motorType, "ChuckRotation");
            ArmSwing = new MotorVelocityPositionParam(motorType, "ArmSwing");
        }

        public MotorVelocityParam ChuckRotation { get; }
        public MotorVelocityPositionParam ArmSwing { get; }
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
            N2FlowWafer = new FlowrateSetParam(gasType, "N2FlowWafer");
            N2FlowLowerFace = new FlowrateSetParam(gasType, "N2FlowLowerFace");
        }

        public FlowrateSetParam N2Flow { get; }
        public FlowrateSetParam N2FlowWafer { get; }
        public FlowrateSetParam N2FlowLowerFace { get; }
    }

    /// <summary>
    /// Liquid 参数访问类
    /// </summary>
    public class LiquidParams
    {
        public LiquidParams(Type plcHandleType)
        {
            var liquidType = plcHandleType.GetNestedType("Liquid")!;
            WFUpperFaceDIWWashFlow2 = new FlowrateSetParam(liquidType, "WFUpperFaceDIWWashFlow2");
        }

        public FlowrateSetParam WFUpperFaceDIWWashFlow2 { get; }
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
}


