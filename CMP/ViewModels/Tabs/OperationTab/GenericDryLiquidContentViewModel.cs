using CMP.Constants.Plc;
using CMP.Models.kvsPlc;
using CMP.Models.Operation.Clean;
using HandyControl.Controls;
using KHL = CMP.Models.kvsPlc.KvHostLink;

namespace CMP.ViewModels.Tabs.OperationTab;

public sealed class GenericDryLiquidContentViewModel : LiquidChamberContentViewModel
{
    public GenericDryLiquidContentViewModel(GenericDryCleanConfig config) : this(
        config,
        () => (GlobalService.kvHostLinkInit.kvLinkDryA, GlobalService.kvHostLinkInit.kvLinkDryB),
        KHL.KHLWriteDevicesAsWords,
        (success, message) =>
        {
            if (success) Growl.SuccessGlobal(message);
            else Growl.ErrorGlobal(message);
        })
    {
    }

    internal GenericDryLiquidContentViewModel(
        GenericDryCleanConfig config,
        Func<(KVLink DryA, KVLink DryB)> getPlcLinks,
        Func<int, KHLDevType, uint, uint, byte[], int> writeDevices,
        Action<bool, string> showResult) : base(
            () =>
            {
                var links = getPlcLinks();
                return config.PlcHandleName switch
                {
                    Handle.CmdDryA => links.DryA.sock,
                    Handle.CmdDryB => links.DryB.sock,
                    _ => throw new InvalidOperationException($"Unsupported Dry Handle: {config.PlcHandleName:X}")
                };
            }, writeDevices, showResult)
    {
        AddParameter($"{config.PlcHandleTypeName}.Liquid.WFUpperFaceDIWWashFlow2.Param.FlowrateSet");
        AddCommandGroups(PlcHandleHelper.GetLiquidCommands(config.PlcHandleTypeName));
    }
}
