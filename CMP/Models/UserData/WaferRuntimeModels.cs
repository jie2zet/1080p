using SqlSugar;

namespace CMP.Models.UserData;

[SugarTable("WaferSnapshot")]
public class WaferSnapshotModel
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    public int LoadportId { get; set; }

    public int SlotId { get; set; }

    [SugarColumn(IsNullable = true)]
    public string WaferName { get; set; } = string.Empty;

    [SugarColumn(IsNullable = true)]
    public string LotID { get; set; } = string.Empty;

    public int WaferStatus { get; set; }

    public int CurrentStation { get; set; }

    [SugarColumn(IsNullable = true, ColumnDataType = "TEXT")]
    public string SequenceRecipe { get; set; } = string.Empty;

    public int SequenceTrackSide { get; set; }

    public bool IsSelected { get; set; }

    [SugarColumn(IsNullable = true, ColumnDataType = "TEXT")]
    public string SequenceValueJson { get; set; } = string.Empty;

    [SugarColumn(IsNullable = true, ColumnDataType = "TEXT")]
    public string SequenceModelsJson { get; set; } = string.Empty;

    public string UpdatedAt { get; set; } = string.Empty;
}

[SugarTable("WaferPickQueueItem")]
public class WaferPickQueueItemModel
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    public int OrderIndex { get; set; }

    public int LoadportId { get; set; }

    public int SlotId { get; set; }
}

[SugarTable("LoadPortRuntimeState")]
public class LoadPortRuntimeStateModel
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    public int LoadportId { get; set; }

    [SugarColumn(Length = 16)]
    public string PortNumber { get; set; } = string.Empty;

    public bool IsCycle { get; set; }

    public int CycleNum { get; set; }

    public int CurrentCycleCount { get; set; }

    public bool IsStopLoad { get; set; }

    public int FoupWorkflowState { get; set; }

    /// <summary>Cycle 用的 last wafer dictionary keys（index = lp*25+slot+1），JSON int 数组。</summary>
    [SugarColumn(IsNullable = true, ColumnDataType = "TEXT")]
    public string LastWaferDictionaryKeysJson { get; set; } = "[]";

    [SugarColumn(IsNullable = true)]
    public int? ProcessingLoadportId { get; set; }

    [SugarColumn(IsNullable = true)]
    public int? ProcessingSlotId { get; set; }

    public string UpdatedAt { get; set; } = string.Empty;
}
