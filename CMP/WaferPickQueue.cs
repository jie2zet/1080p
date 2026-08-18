using CMP.Models.Resource;
using CMP.Services;
using System.Collections.Generic;
using System.Linq;

namespace CMP;

/// <summary>
/// 线程安全的全局取片队列封装（多 LoadPort 共享 <see cref="GlobalService.waferQueue"/>）。
/// </summary>
public static class WaferPickQueue
{
    private static readonly object Sync = new();

    public static int Count
    {
        get
        {
            lock (Sync)
            {
                return GlobalService.waferQueue.Count;
            }
        }
    }

    public static IReadOnlyList<Wafer> Snapshot()
    {
        lock (Sync)
        {
            return GlobalService.waferQueue.ToList();
        }
    }

    public static void Enqueue(Wafer wafer)
    {
        lock (Sync)
        {
            GlobalService.waferQueue.Enqueue(wafer);
        }
        WaferRuntimeStore.Instance.RequestSave();
    }

    public static void EnqueueRange(IEnumerable<Wafer> wafers)
    {
        lock (Sync)
        {
            foreach (var wafer in wafers)
            {
                GlobalService.waferQueue.Enqueue(wafer);
            }
        }
        WaferRuntimeStore.Instance.RequestSave();
    }

    public static void ReplaceAll(IEnumerable<Wafer> wafers)
    {
        lock (Sync)
        {
            RebuildQueue(wafers.ToList());
        }
        WaferRuntimeStore.Instance.RequestSave();
    }

    public static bool HeadMatches(Wafer wafer)
    {
        lock (Sync)
        {
            return GlobalService.waferQueue.Count > 0 &&
                   MatchesWafer(GlobalService.waferQueue.Peek(), wafer);
        }
    }

    public static bool TryDequeueHeadIfMatches(Wafer expected, out Wafer? dequeued)
    {
        lock (Sync)
        {
            dequeued = null;
            if (GlobalService.waferQueue.Count == 0)
            {
                return false;
            }

            var head = GlobalService.waferQueue.Peek();
            if (!MatchesWafer(head, expected))
            {
                return false;
            }

            dequeued = GlobalService.waferQueue.Dequeue();
            WaferRuntimeStore.Instance.RequestSave();
            return true;
        }
    }

    public static void RemoveWafer(Wafer wafer)
    {
        lock (Sync)
        {
            if (GlobalService.waferQueue.Count == 0)
            {
                return;
            }

            var remaining = GlobalService.waferQueue
                .Where(w => !MatchesWafer(w, wafer))
                .ToList();
            RebuildQueue(remaining);
        }
        WaferRuntimeStore.Instance.RequestSave();
    }

    public static int RemoveByLoadportId(int loadportId)
    {
        return RemoveByLoadportIdExcept(loadportId, except: null);
    }

    /// <summary>
    /// 移除指定 LoadPort 的待取片，可保留正在取的那一片以免打断进行中的取片。
    /// </summary>
    public static int RemoveByLoadportIdExcept(int loadportId, Wafer? except)
    {
        lock (Sync)
        {
            var remaining = GlobalService.waferQueue
                .Where(w => w.LoadportID != loadportId || (except != null && MatchesWafer(w, except)))
                .ToList();
            var removed = GlobalService.waferQueue.Count - remaining.Count;
            RebuildQueue(remaining);
            WaferRuntimeStore.Instance.RequestSave();
            return removed;
        }
    }

    /// <summary>
    /// Cycle 同步：仅替换指定 LoadPort 的队列项，保留其他 LoadPort 待取片。
    /// </summary>
    public static void ReplaceLoadportWafers(int loadportId, IEnumerable<Wafer> wafers)
    {
        lock (Sync)
        {
            var others = GlobalService.waferQueue
                .Where(w => w.LoadportID != loadportId)
                .ToList();
            var merged = others.Concat(wafers).ToList();
            RebuildQueue(merged);
        }
        WaferRuntimeStore.Instance.RequestSave();
    }

    public static void RotateToHead(Wafer target)
    {
        lock (Sync)
        {
            var list = GlobalService.waferQueue.ToList();
            var idx = list.FindIndex(w => MatchesWafer(w, target));
            if (idx <= 0)
            {
                return;
            }

            var item = list[idx];
            list.RemoveAt(idx);
            list.Insert(0, item);
            RebuildQueue(list);
        }
        WaferRuntimeStore.Instance.RequestSave();
    }

    private static bool MatchesWafer(Wafer left, Wafer right) =>
        ReferenceEquals(left, right) ||
        (left.LoadportID == right.LoadportID && left.SlotID == right.SlotID);

    private static void RebuildQueue(IReadOnlyList<Wafer> wafers)
    {
        GlobalService.waferQueue.Clear();
        foreach (var wafer in wafers)
        {
            GlobalService.waferQueue.Enqueue(wafer);
        }
    }
}
