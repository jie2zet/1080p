using CMP.Models.Operation.LoadPort;
using CMP.Models.Resource;
using CMP.Models.UserData;
using CMP.Services.LoadPort;
using CMP.Services.Log;
using CMP.Utils;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace CMP.Services;

/// <summary>
/// 晶圆运行态落盘/恢复：WaferList、dicWaferIndex、WaferPickQueue 与 LoadPort 运行参数。
/// 策略 C：重启后一律恢复快照；Mapping 在成功 Load（CLDMP）前不覆盖该口快照。
/// </summary>
public sealed class WaferRuntimeStore
{
    private static readonly Lazy<WaferRuntimeStore> Lazy = new(() => new WaferRuntimeStore());
    public static WaferRuntimeStore Instance => Lazy.Value;

    private readonly object _timerLock = new();
    private CancellationTokenSource? _debounceCts;
    private readonly ConcurrentDictionary<string, bool> _protectMapping = new();

    /// <summary>恢复过程中禁止落盘，避免把半成品写回。</summary>
    public bool IsRestoring { get; private set; }

    private WaferRuntimeStore()
    {
    }

    /// <summary>该口是否处于「恢复后保护」：禁止 Mapping 覆盖，直到本口成功 Load。</summary>
    public bool IsMappingProtected(string portNumber) =>
        _protectMapping.TryGetValue(portNumber, out var v) && v;

    public void SetMappingProtected(string portNumber, bool value) =>
        _protectMapping[portNumber] = value;

    public void RequestSave()
    {
        if (IsRestoring)
        {
            return;
        }

        lock (_timerLock)
        {
            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(300, token).ConfigureAwait(false);
                    SaveAll();
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    Logger.Warn($"WaferRuntimeStore.RequestSave failed: {ex.Message}");
                }
            }, token);
        }
    }

    public void Flush()
    {
        lock (_timerLock)
        {
            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
            _debounceCts = null;
        }

        try
        {
            SaveAll();
        }
        catch (Exception ex)
        {
            Logger.Error($"WaferRuntimeStore.Flush failed: {ex.Message}", ex, "wafer_runtime.txt", 0);
        }
    }

    public void SaveAll()
    {
        var ports = LoadPortManager.Instance.GetAllLoadPorts().Values.ToList();
        if (ports.Count == 0)
        {
            return;
        }

        var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var waferRows = new List<WaferSnapshotModel>();
        var portRows = new List<LoadPortRuntimeStateModel>();

        foreach (var lp in ports)
        {
            var loadportId = lp.LoadportId;
            if (loadportId < 0)
            {
                continue;
            }

            foreach (var wafer in lp.WaferList)
            {
                if (wafer.WaferStatus is WaferStatus.NonWafer or WaferStatus.None)
                {
                    continue;
                }

                waferRows.Add(ToSnapshot(wafer, loadportId, now));
            }

            portRows.Add(lp.CaptureRuntimeState(now));
        }

        var queueRows = WaferPickQueue.Snapshot()
            .Select((w, i) => new WaferPickQueueItemModel
            {
                OrderIndex = i,
                LoadportId = w.LoadportID,
                SlotId = w.SlotID
            })
            .ToList();

        var db = SqlSugarHelper.Db;
        try
        {
            db.Ado.BeginTran();
            db.Deleteable<WaferSnapshotModel>().ExecuteCommand();
            db.Deleteable<WaferPickQueueItemModel>().ExecuteCommand();
            db.Deleteable<LoadPortRuntimeStateModel>().ExecuteCommand();

            if (waferRows.Count > 0)
            {
                db.Insertable(waferRows).ExecuteCommand();
            }

            if (queueRows.Count > 0)
            {
                db.Insertable(queueRows).ExecuteCommand();
            }

            if (portRows.Count > 0)
            {
                db.Insertable(portRows).ExecuteCommand();
            }

            db.Ado.CommitTran();
        }
        catch (Exception ex)
        {
            try
            {
                db.Ado.RollbackTran();
            }
            catch
            {
                // ignore
            }

            Logger.Error($"WaferRuntimeStore.SaveAll failed: {ex.Message}", ex, "wafer_runtime.txt", 0);
        }
    }

    public void RestoreAll()
    {
        IsRestoring = true;
        try
        {
            var db = SqlSugarHelper.Db;
            var waferRows = db.Queryable<WaferSnapshotModel>().ToList();
            var queueRows = db.Queryable<WaferPickQueueItemModel>().OrderBy(x => x.OrderIndex).ToList();
            var portRows = db.Queryable<LoadPortRuntimeStateModel>().ToList();

            if (waferRows.Count == 0 && queueRows.Count == 0 && portRows.Count == 0)
            {
                Logger.Info("WaferRuntimeStore: no snapshot to restore.", "wafer_runtime.txt", 0);
                return;
            }

            var ports = LoadPortManager.Instance.GetAllLoadPorts();
            var protectedIds = waferRows.Select(r => r.LoadportId).ToHashSet();
            foreach (var lp in ports.Values)
            {
                SetMappingProtected(lp.PortNumber, protectedIds.Contains(lp.LoadportId));
            }

            // 1) 恢复 WaferList + dicWaferIndex
            foreach (var row in waferRows)
            {
                var lp = ports.Values.FirstOrDefault(p => p.LoadportId == row.LoadportId);
                if (lp == null)
                {
                    continue;
                }

                var wafer = FindSlotWafer(lp, row.SlotId);
                if (wafer == null)
                {
                    continue;
                }

                ApplySnapshot(wafer, row);
                var index = row.LoadportId * 25 + (row.SlotId + 1);
                wafer.index = index;
                lock (GlobalService.dicWaferIndex)
                {
                    GlobalService.dicWaferIndex[index] = wafer;
                }
            }

            // 2) LoadPort 运行态
            foreach (var row in portRows)
            {
                var lp = ports.Values.FirstOrDefault(p => p.LoadportId == row.LoadportId)
                         ?? ports.Values.FirstOrDefault(p => p.PortNumber == row.PortNumber);
                lp?.ApplyRuntimeState(row);
            }

            // 3) 取片队列（引用同一 Wafer 实例）
            var restoredQueue = new List<Wafer>();
            foreach (var item in queueRows)
            {
                var lp = ports.Values.FirstOrDefault(p => p.LoadportId == item.LoadportId);
                var wafer = lp == null ? null : FindSlotWafer(lp, item.SlotId);
                if (wafer != null)
                {
                    restoredQueue.Add(wafer);
                }
            }

            WaferPickQueue.ReplaceAll(restoredQueue);

            // 4) PLC 跟踪字校正 CurrentStation
            ApplyPlcStations(ports.Values);

            foreach (var lp in ports.Values)
            {
                lp.NotifyMappingProtectionChanged();
            }

            Logger.Info(
                $"WaferRuntimeStore restored wafers={waferRows.Count}, queue={restoredQueue.Count}, ports={portRows.Count}",
                "wafer_runtime.txt",
                0);
        }
        catch (Exception ex)
        {
            Logger.Error($"WaferRuntimeStore.RestoreAll failed: {ex.Message}", ex, "wafer_runtime.txt", 0);
        }
        finally
        {
            IsRestoring = false;
        }
    }

    /// <summary>本口已正常 Load：允许后续 Mapping 覆盖该口快照。</summary>
    public void ReleaseMappingProtection(LoadPortData loadPort)
    {
        if (loadPort == null)
        {
            return;
        }

        SetMappingProtected(loadPort.PortNumber, false);
        loadPort.NotifyMappingProtectionChanged();
    }

    private static Wafer? FindSlotWafer(LoadPortData lp, int slotId)
    {
        return lp.WaferList.FirstOrDefault(w => w.SlotID == slotId)
               ?? (slotId >= 0 && slotId < 25
                   ? lp.WaferList.ElementAtOrDefault(24 - slotId)
                   : null);
    }

    private static WaferSnapshotModel ToSnapshot(Wafer wafer, int loadportId, string now)
    {
        var models = wafer.WaferSequenceModels?
            .Select(m => new { m.ChamberName, m.ChamberSequnceStep })
            .ToList();

        return new WaferSnapshotModel
        {
            LoadportId = loadportId,
            SlotId = wafer.SlotID,
            WaferName = wafer.WaferName ?? string.Empty,
            LotID = wafer.LotID ?? string.Empty,
            WaferStatus = (int)wafer.WaferStatus,
            CurrentStation = (int)wafer.CurrentStation,
            SequenceRecipe = wafer.SequenceRecipe ?? string.Empty,
            SequenceTrackSide = (int)wafer.SequenceTrackSide,
            IsSelected = wafer.IsSelected,
            SequenceValueJson = JsonConvert.SerializeObject(wafer.SequnceValue ?? Array.Empty<int>()),
            SequenceModelsJson = JsonConvert.SerializeObject(models ?? []),
            UpdatedAt = now
        };
    }

    private static void ApplySnapshot(Wafer wafer, WaferSnapshotModel row)
    {
        wafer.LoadportID = row.LoadportId;
        wafer.SlotID = row.SlotId;
        wafer.WaferName = row.WaferName ?? string.Empty;
        wafer.LotID = row.LotID ?? string.Empty;
        wafer.WaferStatus = (WaferStatus)row.WaferStatus;
        wafer.CurrentStation = (CurrentStation)row.CurrentStation;
        wafer.SequenceRecipe = row.SequenceRecipe ?? string.Empty;
        wafer.SequenceTrackSide = (WaferSequenceTrackSide)row.SequenceTrackSide;
        wafer.IsSelected = row.IsSelected;

        try
        {
            wafer.SequnceValue = string.IsNullOrWhiteSpace(row.SequenceValueJson)
                ? []
                : JsonConvert.DeserializeObject<int[]>(row.SequenceValueJson) ?? [];
        }
        catch
        {
            wafer.SequnceValue = [];
        }

        try
        {
            var models = string.IsNullOrWhiteSpace(row.SequenceModelsJson)
                ? null
                : JsonConvert.DeserializeObject<List<WaferSequenceModelDto>>(row.SequenceModelsJson);
            if (models != null)
            {
                wafer.WaferSequenceModels = models
                    .Select(m => new WaferSequenceModel
                    {
                        ChamberName = m.ChamberName,
                        ChamberSequnceStep = m.ChamberSequnceStep
                    })
                    .ToList();
            }
        }
        catch
        {
            // keep existing
        }
    }

    private static void ApplyPlcStations(IEnumerable<LoadPortData> ports)
    {
        if (GlobalService.plcComm is not { IsPLCConnected: true })
        {
            return;
        }

        foreach (var (address, station) in StationTrackingAddresses)
        {
            try
            {
                var value = GlobalService.plcComm.ReadShortVari(address);
                if (value <= 0)
                {
                    continue;
                }

                byte slotByte = (byte)(value >> 8);
                byte cassetteByte = (byte)(value & 0xFF);
                if (cassetteByte == 0 || slotByte == 0)
                {
                    continue;
                }

                var loadportId = cassetteByte - 1;
                var slotId = slotByte - 1;
                var lp = ports.FirstOrDefault(p => p.LoadportId == loadportId);
                var wafer = lp == null ? null : FindSlotWafer(lp, slotId);
                if (wafer == null)
                {
                    continue;
                }

                if (wafer.CurrentStation != station)
                {
                    Logger.Info(
                        $"WaferRuntimeStore station from PLC: L{loadportId}S{slotId} {wafer.CurrentStation}->{station} @ {address}",
                        "wafer_runtime.txt",
                        0);
                    wafer.CurrentStation = station;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"WaferRuntimeStore PLC read {address} failed: {ex.Message}");
            }
        }

        // 双手臂：上/下臂同属一个 CurrentStation 枚举
        foreach (var (upper, lower, station) in DualArmTrackingAddresses)
        {
            ApplyArmAddress(ports, upper, station);
            ApplyArmAddress(ports, lower, station);
        }
    }

    private static void ApplyArmAddress(IEnumerable<LoadPortData> ports, string address, CurrentStation station)
    {
        try
        {
            var value = GlobalService.plcComm.ReadShortVari(address);
            if (value <= 0)
            {
                return;
            }

            byte slotByte = (byte)(value >> 8);
            byte cassetteByte = (byte)(value & 0xFF);
            if (cassetteByte == 0 || slotByte == 0)
            {
                return;
            }

            var loadportId = cassetteByte - 1;
            var slotId = slotByte - 1;
            var lp = ports.FirstOrDefault(p => p.LoadportId == loadportId);
            var wafer = lp == null ? null : FindSlotWafer(lp, slotId);
            if (wafer != null)
            {
                wafer.CurrentStation = station;
            }
        }
        catch
        {
            // ignore single address
        }
    }

    private static readonly Dictionary<string, CurrentStation> StationTrackingAddresses = new()
    {
        ["W3010"] = CurrentStation.PolishA,
        ["W3650"] = CurrentStation.PolishB,
        ["W3C90"] = CurrentStation.PolishC,
        ["W42D0"] = CurrentStation.PolishD,
        ["W4910"] = CurrentStation.CleanAR1,
        ["W4C10"] = CurrentStation.CleanAR2,
        ["W4F10"] = CurrentStation.CleanAR3,
        ["W5210"] = CurrentStation.CleanAP1,
        ["W5510"] = CurrentStation.CleanAP2,
        ["W5810"] = CurrentStation.CleanADry,
        ["W5B10"] = CurrentStation.CleanBR1,
        ["W5E10"] = CurrentStation.CleanBR2,
        ["W6110"] = CurrentStation.CleanBR3,
        ["W6410"] = CurrentStation.CleanBP1,
        ["W6710"] = CurrentStation.CleanBP2,
        ["W6A10"] = CurrentStation.CleanBDry,
        ["W0AA0"] = CurrentStation.TransBuffer,
        ["W0ADO"] = CurrentStation.FixABuffer,
        ["W1A10"] = CurrentStation.FixALTP1,
        ["W1A20"] = CurrentStation.FixALTP2,
        ["W1A30"] = CurrentStation.FixALTP3,
        ["W0B10"] = CurrentStation.FixBBuffer,
        ["W1C10"] = CurrentStation.FixBLTP1,
        ["W1C20"] = CurrentStation.FixBLTP2,
        ["W1C30"] = CurrentStation.FixBLTP3,
    };

    private static readonly (string Upper, string Lower, CurrentStation Station)[] DualArmTrackingAddresses =
    [
        ("W0E10", "W0E20", CurrentStation.EFEMRobot),
        ("W2610", "W2620", CurrentStation.TransRobot),
        ("W1E10", "W1E20", CurrentStation.CleanARobot1),
        ("W2210", "W2220", CurrentStation.CleanARobot2),
        ("W2010", "W2020", CurrentStation.CleanBRobot1),
        ("W2410", "W2420", CurrentStation.CleanBRobot2),
    ];

    private sealed class WaferSequenceModelDto
    {
        public string? ChamberName { get; set; }
        public int ChamberSequnceStep { get; set; }
    }
}
