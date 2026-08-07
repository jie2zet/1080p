using CMP.Models.UserData;
using CMP.Utils;
using SqlSugar;

namespace CMP.Services.Auth;

public static class AuditActions
{
    public const string RecipeSave = "RecipeSave";
    public const string RecipeCreate = "RecipeCreate";
    public const string RecipeDelete = "RecipeDelete";
    public const string RecipeRename = "RecipeRename";
    public const string RecipeSaveAs = "RecipeSaveAs";
    public const string ConfigApply = "ConfigApply";
    public const string ConfigSyncPlc = "ConfigSyncPlc";
    public const string ConfigExport = "ConfigExport";
}

public static class AuditLogService
{
    public static void Write(string userName, PermissionEnum level, string action, string result, string detail = "")
    {
        try
        {
            SqlSugarHelper.Db.Insertable(new AuditLogModel
            {
                UserName = userName,
                Level = level.ToString(),
                Action = action,
                Result = result,
                Detail = detail,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
            }).ExecuteCommand();
        }
        catch
        {
            // 审计失败不阻断主流程
        }
    }

    public static void WriteCurrentUser(string action, string result, string detail = "")
    {
        if (!SessionContext.Instance.IsAuthenticated)
        {
            Write("System", PermissionEnum.Operator, action, result, detail);
            return;
        }

        Write(
            SessionContext.Instance.UserName,
            SessionContext.Instance.Level,
            action,
            result,
            detail);
    }

    public static List<AuditLogModel> Query(string? userName, string? action, DateTime? startTime, DateTime? endTime)
    {
        var query = SqlSugarHelper.Db.Queryable<AuditLogModel>();

        if (!string.IsNullOrWhiteSpace(userName))
        {
            var keyword = userName.Trim();
            query = query.Where(x => x.UserName.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(action) && action != "全部")
        {
            query = query.Where(x => x.Action == action);
        }

        if (startTime.HasValue)
        {
            var start = startTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
            query = query.Where(x => x.Timestamp.CompareTo(start) >= 0);
        }

        if (endTime.HasValue)
        {
            var end = endTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
            query = query.Where(x => x.Timestamp.CompareTo(end) <= 0);
        }

        return query.OrderBy(x => x.Id, OrderByType.Desc).ToList();
    }
}
