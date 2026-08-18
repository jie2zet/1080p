using CMP.Models.UserData;
using CMP.Utils;
using SqlSugar;

namespace CMP.Services.Auth;

public static class AuditModules
{
    public const string Auth = "Auth";
    public const string Recipe = "Recipe";
    public const string EC = "EC";
    public const string Calibration = "Calibration";
    public const string Consumable = "Consumable";
    public const string Operation = "Operation";
}

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
    public const string CalibrationSave = "CalibrationSave";
    public const string CalibrationAssign = "CalibrationAssign";
    public const string ConsumableUpdate = "ConsumableUpdate";
}

public static class AuditLogService
{
    public static void Write(
        string userName,
        PermissionEnum level,
        string action,
        string result,
        string detail = "",
        string module = "",
        string target = "",
        string oldValue = "",
        string newValue = "")
    {
        try
        {
            SqlSugarHelper.Db.Insertable(new AuditLogModel
            {
                UserName = userName,
                Level = level.ToString(),
                Module = module ?? string.Empty,
                Action = action,
                Target = target ?? string.Empty,
                OldValue = oldValue ?? string.Empty,
                NewValue = newValue ?? string.Empty,
                Result = result,
                Detail = detail ?? string.Empty,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
            }).ExecuteCommand();
        }
        catch
        {
            // 审计失败不阻断主流程
        }
    }

    public static void WriteCurrentUser(
        string action,
        string result,
        string detail = "",
        string module = "",
        string target = "")
    {
        WriteForCurrentUser(module, action, target, string.Empty, string.Empty, result, detail);
    }

    /// <summary>
    /// 记录一条参数级变更（原值/新值）。供 EC、校准、耗材等共用。
    /// </summary>
    public static void WriteChange(
        string module,
        string action,
        string target,
        string? oldValue,
        string? newValue,
        string result,
        string detail = "")
    {
        WriteForCurrentUser(module, action, target, oldValue, newValue, result, detail);
    }

    private static void WriteForCurrentUser(
        string module,
        string action,
        string target,
        string? oldValue,
        string? newValue,
        string result,
        string detail)
    {
        if (!SessionContext.Instance.IsAuthenticated)
        {
            Write(
                "System",
                PermissionEnum.Operator,
                action,
                result,
                detail,
                module,
                target,
                oldValue ?? string.Empty,
                newValue ?? string.Empty);
            return;
        }

        Write(
            SessionContext.Instance.UserName,
            SessionContext.Instance.Level,
            action,
            result,
            detail,
            module,
            target,
            oldValue ?? string.Empty,
            newValue ?? string.Empty);
    }

    public static List<AuditLogModel> Query(
        string? userName,
        string? action,
        DateTime? startTime,
        DateTime? endTime,
        string? module = null,
        string? target = null)
    {
        var query = SqlSugarHelper.Db.Queryable<AuditLogModel>();

        if (!string.IsNullOrWhiteSpace(userName))
        {
            var keyword = userName.Trim();
            query = query.Where(x => x.UserName.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(action) && !IsAllFilter(action))
        {
            query = query.Where(x => x.Action == action);
        }

        if (!string.IsNullOrWhiteSpace(module) && !IsAllFilter(module))
        {
            query = query.Where(x => x.Module == module);
        }

        if (!string.IsNullOrWhiteSpace(target))
        {
            var keyword = target.Trim();
            query = query.Where(x => x.Target.Contains(keyword));
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

    public static bool IsAllFilter(string? value) =>
        string.IsNullOrWhiteSpace(value) || value is "全部" or "All";
}
