using CMP.Constants.Plc;
using CMP.Models.kvsPlc;
using CMP.Models.Operation.Roller;
using HandyControl.Controls;
using KHL = CMP.Models.kvsPlc.KvHostLink;

namespace CMP.ViewModels.Tabs.OperationTab;

public sealed class GenericRollerLiquidContentViewModel : LiquidChamberContentViewModel
{
    public GenericRollerLiquidContentViewModel(GenericRollConfig config) : this(
        config,
        () => (GlobalService.kvHostLinkInit.kvLinkRollerA, GlobalService.kvHostLinkInit.kvLinkRollerB),
        KHL.KHLWriteDevicesAsWords,
        (success, message) =>
        {
            if (success) Growl.SuccessGlobal(message);
            else Growl.ErrorGlobal(message);
        })
    {
    }

    internal GenericRollerLiquidContentViewModel(
        GenericRollConfig config,
        Func<(KVLink RollerA, KVLink RollerB)> getPlcLinks,
        Func<int, KHLDevType, uint, uint, byte[], int> writeDevices,
        Action<bool, string> showResult) : base(
            () =>
            {
                var links = getPlcLinks();
                return config.PlcHandleName switch
                {
                    Handle.CmdRollA1 or Handle.CmdRollA2 or Handle.CmdRollA3 => links.RollerA.sock,
                    Handle.CmdRollB1 or Handle.CmdRollB2 or Handle.CmdRollB3 => links.RollerB.sock,
                    _ => throw new InvalidOperationException($"Unsupported Roller Handle: {config.PlcHandleName:X}")
                };
            }, writeDevices, showResult)
    {
        // Shared offsets must still resolve metadata from the selected chamber's full path.
        AddParameter($"{config.PlcHandleTypeName}.Liquid.ChemicalEnterFlow.Param.FlowrateSet");
        AddParameter($"{config.PlcHandleTypeName}.Liquid.DiwEnterFlow.Param.FlowrateSet");
        AddParameter($"{config.PlcHandleTypeName}.Liquid.WfUpperFaceWashDiwFlow.Param.FlowrateSet");
        AddParameter($"{config.PlcHandleTypeName}.Liquid.WfLowerFaceDiwWashFlow.Param.FlowrateSet");
        AddCommandGroups(PlcHandleHelper.GetLiquidCommands(config.PlcHandleTypeName));
    }
}
