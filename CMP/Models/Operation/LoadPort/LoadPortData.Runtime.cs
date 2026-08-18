using CMP.Models.Resource;
using CMP.Models.UserData;
using CMP.Services;
using Newtonsoft.Json;

namespace CMP.Models.Operation.LoadPort;

public partial class LoadPortData
{
    /// <summary>COM12–COM16 对应 0–4。</summary>
    public int LoadportId => GetLoadportId();

    /// <summary>恢复快照后为 true，直到本口成功 Load（CLDMP）。</summary>
    public bool IsMappingProtected => WaferRuntimeStore.Instance.IsMappingProtected(PortNumber);

    public void NotifyMappingProtectionChanged()
    {
        RaisePropertyChanged(nameof(IsMappingProtected));
    }

    internal LoadPortRuntimeStateModel CaptureRuntimeState(string now)
    {
        lock (_processingLock)
        {
            return new LoadPortRuntimeStateModel
            {
                LoadportId = LoadportId,
                PortNumber = PortNumber,
                IsCycle = IsCycle,
                CycleNum = CycleNum,
                CurrentCycleCount = CurrentCycleCount,
                IsStopLoad = IsStopLoad,
                FoupWorkflowState = (int)FoupWorkflowState,
                LastWaferDictionaryKeysJson = JsonConvert.SerializeObject(_lastWaferDictionary.Keys.ToList()),
                ProcessingLoadportId = _processingWafer?.LoadportID,
                ProcessingSlotId = _processingWafer?.SlotID,
                UpdatedAt = now
            };
        }
    }

    internal void ApplyRuntimeState(LoadPortRuntimeStateModel row)
    {
        CycleNum = row.CycleNum;
        _isCycle = row.IsCycle;
        RaisePropertyChanged(nameof(IsCycle));
        CurrentCycleCount = row.CurrentCycleCount;
        IsStopLoad = row.IsStopLoad;
        if (Enum.IsDefined(typeof(LoadportFoupStatus), row.FoupWorkflowState))
        {
            FoupWorkflowState = (LoadportFoupStatus)row.FoupWorkflowState;
        }

        _lastWaferDictionary.Clear();
        try
        {
            var keys = string.IsNullOrWhiteSpace(row.LastWaferDictionaryKeysJson)
                ? []
                : JsonConvert.DeserializeObject<List<int>>(row.LastWaferDictionaryKeysJson) ?? [];
            foreach (var key in keys)
            {
                var slotId = (key % 25) - 1;
                var wafer = WaferList.FirstOrDefault(w => w.SlotID == slotId);
                if (wafer != null)
                {
                    _lastWaferDictionary[key] = wafer;
                }
            }
        }
        catch
        {
            // keep empty dictionary
        }

        lock (_processingLock)
        {
            _processingWafer = row.ProcessingSlotId is int slot
                ? WaferList.FirstOrDefault(w => w.SlotID == slot)
                : null!;
        }
    }
}
