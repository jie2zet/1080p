using CMP.Constants.Plc;
using CMP.Models.kvsPlc;
using CMP.Models.Operation.Clean;
using HandyControl.Controls;
using KHL = CMP.Models.kvsPlc.KvHostLink;

namespace CMP.ViewModels.Tabs.OperationTab;

public sealed class GenericPenLiquidContentViewModel : LiquidChamberContentViewModel
{
    public GenericPenLiquidContentViewModel(GenericPenCleanConfig config) : this(
        config,
        () => (GlobalService.kvHostLinkInit.kvLinkPenA, GlobalService.kvHostLinkInit.kvLinkPenB),
        KHL.KHLWriteDevicesAsWords,
        (success, message) =>
        {
            if (success) Growl.SuccessGlobal(message);
            else Growl.ErrorGlobal(message);
        })
    {
    }

    internal GenericPenLiquidContentViewModel(
        GenericPenCleanConfig config,
        Func<(KVLink PenA, KVLink PenB)> getPlcLinks,
        Func<int, KHLDevType, uint, uint, byte[], int> writeDevices,
        Action<bool, string> showResult) : base(
            () =>
            {
                var links = getPlcLinks();
                return config.PlcHandleName switch
                {
                    Handle.CmdPenA1 => links.PenA.sock,
                    Handle.CmdPenB1 => links.PenB.sock,
                    _ => throw new InvalidOperationException($"Unsupported Pen Handle: {config.PlcHandleName:X}")
                };
            }, writeDevices, showResult)
    {
        AddParameter($"{config.PlcHandleTypeName}.Liquid.DIWFlow.Param.FlowrateSet");
        AddParameter($"{config.PlcHandleTypeName}.Liquid.WFUpperFaceDIWFlow.Param.FlowrateSet");
        AddParameter($"{config.PlcHandleTypeName}.Liquid.DIWPressure.Param.PressureSet");
        AddParameter($"{config.PlcHandleTypeName}.Liquid.WFLowerFaceDIWFlow.Param.FlowrateSet");
        AddParameter($"{config.PlcHandleTypeName}.Liquid.ChemicalFlow.Param.FlowrateSet");
        AddParameter($"{config.PlcHandleTypeName}.Gas.CO2Pressure.Param.PressureSet");
        AddParameter($"{config.PlcHandleTypeName}.Gas.N2Flow.Param.FlowrateSet");
        AddCommandGroups(PlcHandleHelper.GetLiquidCommands(config.PlcHandleTypeName));
    }
}
