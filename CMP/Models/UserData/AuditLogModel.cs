using SqlSugar;

namespace CMP.Models.UserData;

[SugarTable("AuditLog")]
public class AuditLogModel
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Level { get; set; } = string.Empty;

    [SugarColumn(IsNullable = true)]
    public string Module { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    [SugarColumn(IsNullable = true)]
    public string Target { get; set; } = string.Empty;

    [SugarColumn(IsNullable = true)]
    public string OldValue { get; set; } = string.Empty;

    [SugarColumn(IsNullable = true)]
    public string NewValue { get; set; } = string.Empty;

    public string Result { get; set; } = string.Empty;

    public string Detail { get; set; } = string.Empty;

    public string Timestamp { get; set; } = string.Empty;
}
