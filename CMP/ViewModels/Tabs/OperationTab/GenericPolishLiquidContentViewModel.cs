using CMP.Constants.Enums;
using CMP.Constants.Plc;
using CMP.Models.kvsPlc;
using HandyControl.Controls;
using KHL = CMP.Models.kvsPlc.KvHostLink;

namespace CMP.ViewModels.Tabs.OperationTab;

public sealed class GenericPolishLiquidContentViewModel : LiquidChamberContentViewModel
{
    public GenericPolishLiquidContentViewModel(WorkstationType chamber) : this(
        chamber,
        () => (GlobalService.kvHostLinkInit.kvLinkPolishA, GlobalService.kvHostLinkInit.kvLinkPolishB,
            GlobalService.kvHostLinkInit.kvLinkPolishC, GlobalService.kvHostLinkInit.kvLinkPolishD),
        KHL.KHLWriteDevicesAsWords,
        (success, message) =>
        {
            if (success) Growl.SuccessGlobal(message);
            else Growl.ErrorGlobal(message);
        })
    {
    }

    internal GenericPolishLiquidContentViewModel(
        WorkstationType chamber,
        Func<(KVLink PolishA, KVLink PolishB, KVLink PolishC, KVLink PolishD)> getPlcLinks,
        Func<int, KHLDevType, uint, uint, byte[], int> writeDevices,
        Action<bool, string> showResult) : base(
            () =>
            {
                var links = getPlcLinks();
                return chamber switch
                {
                    WorkstationType.PolishA => links.PolishA.sock,
                    WorkstationType.PolishB => links.PolishB.sock,
                    WorkstationType.PolishC => links.PolishC.sock,
                    WorkstationType.PolishD => links.PolishD.sock,
                    _ => throw new ArgumentOutOfRangeException(nameof(chamber), chamber, "Unsupported Polish chamber.")
                };
            }, writeDevices, showResult)
    {
        if (chamber is not (WorkstationType.PolishA or WorkstationType.PolishB
            or WorkstationType.PolishC or WorkstationType.PolishD))
            throw new ArgumentOutOfRangeException(nameof(chamber), chamber, "Unsupported Polish chamber.");

        AddParameter("CmdPolish.Liquid.Slurry1.Param.Flowrate");
        AddParameter("CmdPolish.Liquid.Slurry2.Param.Flowrate");
        AddParameter("CmdPolish.Liquid.Slurry3.Param.Flowrate");
        AddCommandGroups(PlcHandleHelper.GetLiquidCommands(nameof(PlcHandle.CmdPolish)));
    }
}
