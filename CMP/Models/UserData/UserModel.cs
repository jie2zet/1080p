using SqlSugar;

namespace CMP.Models.UserData;

[SugarTable("UserData")]
public class UserModel
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    /// <summary>旧版明文密码列，迁移后留空。</summary>
    public string Password { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>角色显示名，与 Level 同步。</summary>
    public string Group { get; set; } = string.Empty;

    public string ModifyTime { get; set; } = string.Empty;

    public PermissionEnum Level { get; set; } = PermissionEnum.Operator;

    public bool IsEnabled { get; set; } = true;
}
