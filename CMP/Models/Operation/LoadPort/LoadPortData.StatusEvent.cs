using CMP.Extensions;
using CMP.Services.LoadPort;
using CMP.Services.Log;
using Prism.Events;
using Prism.Ioc;

namespace CMP.Models.Operation.LoadPort;

public partial class LoadPortData
{
    private HardwareStatusSnapshot? _lastHardwareSnapshot;

    private readonly record struct HardwareStatusSnapshot(
        EquipmentStatus EquipmentStatus,
        short ErrorCode,
        Mode Mode,
        OperationStatus OperationStatus,
        FoupPlacementStatus FoupPlacementStatus,
        FoupClampStatus FoupClampStatus,
        DoorLatchStatus DoorLatchStatus,
        FIMSDoorStatus FimsDoorStatus,
        Vacuum Vacuum,
        WaferOutSensor WaferOutSensor,
        FoupDockStatus FoupDockStatus,
        MapperArmPosition MapperArmPosition,
        ZAxisPosition ZAxisPosition,
        InfoPadStatus InfoPadStatus);

    private static HardwareStatusSnapshot CreateHardwareSnapshot(LoadPortStatus? status)
    {
        if (status == null)
        {
            return default;
        }

        return new HardwareStatusSnapshot(
            status.EquipmentStatus,
            status.ErrorCode,
            status.Mode,
            status.operationStatus,
            status.foupPlacementStatus,
            status.FoupClampStatus,
            status.DoorLatchStatus,
            status.FIMSDoorStatus,
            status.Vacuum,
            status.WaferOutSensor,
            status.FoupDockStatus,
            status.MapperArmPosition,
            status.ZAxisPosition,
            status.InfoPadStatus);
    }

    private static List<string> CollectChangedFields(
        HardwareStatusSnapshot? previous,
        HardwareStatusSnapshot current,
        LoadportFoupStatus previousWorkflow,
        LoadportFoupStatus currentWorkflow)
    {
        var changed = new List<string>();

        if (previous == null)
        {
            changed.Add(nameof(LoadPortStatus.EquipmentStatus));
            changed.Add(nameof(LoadPortStatus.ErrorCode));
            changed.Add(nameof(LoadPortStatus.Mode));
            changed.Add(nameof(LoadPortStatus.operationStatus));
            changed.Add(nameof(LoadPortStatus.foupPlacementStatus));
            changed.Add(nameof(LoadPortStatus.FoupClampStatus));
            changed.Add(nameof(LoadPortStatus.DoorLatchStatus));
            changed.Add(nameof(LoadPortStatus.FIMSDoorStatus));
            changed.Add(nameof(LoadPortStatus.Vacuum));
            changed.Add(nameof(LoadPortStatus.WaferOutSensor));
            changed.Add(nameof(LoadPortStatus.FoupDockStatus));
            changed.Add(nameof(LoadPortStatus.MapperArmPosition));
            changed.Add(nameof(LoadPortStatus.ZAxisPosition));
            changed.Add(nameof(LoadPortStatus.InfoPadStatus));
            changed.Add(nameof(FoupWorkflowState));
            return changed;
        }

        var prev = previous.Value;
        if (prev.EquipmentStatus != current.EquipmentStatus) changed.Add(nameof(LoadPortStatus.EquipmentStatus));
        if (prev.ErrorCode != current.ErrorCode) changed.Add(nameof(LoadPortStatus.ErrorCode));
        if (prev.Mode != current.Mode) changed.Add(nameof(LoadPortStatus.Mode));
        if (prev.OperationStatus != current.OperationStatus) changed.Add(nameof(LoadPortStatus.operationStatus));
        if (prev.FoupPlacementStatus != current.FoupPlacementStatus) changed.Add(nameof(LoadPortStatus.foupPlacementStatus));
        if (prev.FoupClampStatus != current.FoupClampStatus) changed.Add(nameof(LoadPortStatus.FoupClampStatus));
        if (prev.DoorLatchStatus != current.DoorLatchStatus) changed.Add(nameof(LoadPortStatus.DoorLatchStatus));
        if (prev.FimsDoorStatus != current.FimsDoorStatus) changed.Add(nameof(LoadPortStatus.FIMSDoorStatus));
        if (prev.Vacuum != current.Vacuum) changed.Add(nameof(LoadPortStatus.Vacuum));
        if (prev.WaferOutSensor != current.WaferOutSensor) changed.Add(nameof(LoadPortStatus.WaferOutSensor));
        if (prev.FoupDockStatus != current.FoupDockStatus) changed.Add(nameof(LoadPortStatus.FoupDockStatus));
        if (prev.MapperArmPosition != current.MapperArmPosition) changed.Add(nameof(LoadPortStatus.MapperArmPosition));
        if (prev.ZAxisPosition != current.ZAxisPosition) changed.Add(nameof(LoadPortStatus.ZAxisPosition));
        if (prev.InfoPadStatus != current.InfoPadStatus) changed.Add(nameof(LoadPortStatus.InfoPadStatus));
        if (previousWorkflow != currentWorkflow) changed.Add(nameof(FoupWorkflowState));

        return changed;
    }

    private void TryPublishStatusChanged(
        LoadPortStatus? hardwareStatus,
        LoadportFoupStatus previousWorkflow,
        LoadportFoupStatus currentWorkflow)
    {
        var currentSnapshot = CreateHardwareSnapshot(hardwareStatus);
        var changedFields = CollectChangedFields(
            _lastHardwareSnapshot,
            currentSnapshot,
            previousWorkflow,
            currentWorkflow);

        if (_lastHardwareSnapshot != null && changedFields.Count == 0)
        {
            return;
        }

        PublishStatusChanged(new LoadPortStatusChangedEventArgs
        {
            PortName = _portName,
            DisplayNumber = DisplayNumber,
            LoadportId = GetLoadportId(),
            PreviousWorkflow = previousWorkflow,
            CurrentWorkflow = currentWorkflow,
            HardwareStatus = hardwareStatus,
            ChangedFields = changedFields,
            Timestamp = DateTime.Now
        });

        _lastHardwareSnapshot = currentSnapshot;
    }

    private void PublishStatusChanged(LoadPortStatusChangedEventArgs args)
    {
        try
        {
            var aggregator = ContainerLocator.Container.Resolve<IEventAggregator>();
            aggregator.GetEvent<LoadPortStatusChangedEvent>().Publish(args);
        }
        catch (Exception ex)
        {
            Logger.Error($"Publish LoadPortStatusChanged failed: {ex.Message}", ex, "loadport.txt", 0);
        }
    }
}
