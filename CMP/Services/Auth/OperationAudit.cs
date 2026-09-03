using Prism.Commands;

namespace CMP.Services.Auth;

/// <summary>
/// 命令切面：人点的 DelegateCommand 在执行时自动写入 Operation 审计。
/// </summary>
public static class OperationAudit
{
    public static DelegateCommand Create(
        string action,
        Action execute,
        Func<string> getUnit,
        Func<bool>? canExecute = null)
    {
        void Logged() => ExecuteLogged(action, getUnit(), execute);
        return canExecute is null
            ? new DelegateCommand(Logged)
            : new DelegateCommand(Logged, canExecute);
    }

    public static DelegateCommand CreateAsync(string action, Func<Task> execute, Func<string> getUnit)
    {
        return new DelegateCommand(async () =>
        {
            var target = getUnit() ?? string.Empty;
            try
            {
                await execute().ConfigureAwait(true);
                WriteSuccess(action, target);
            }
            catch (Exception ex)
            {
                WriteFailed(action, target, ex.Message);
                throw;
            }
        });
    }

    public static DelegateCommand<T> CreateAsync<T>(
        string action,
        Func<T, Task> execute,
        Func<string> getUnit,
        Func<T, string>? formatTarget = null)
    {
        return new DelegateCommand<T>(async param =>
        {
            var suffix = formatTarget != null ? formatTarget(param) : param?.ToString();
            var target = Combine(getUnit(), suffix);
            try
            {
                await execute(param).ConfigureAwait(true);
                WriteSuccess(action, target);
            }
            catch (Exception ex)
            {
                WriteFailed(action, target, ex.Message);
                throw;
            }
        });
    }

    public static DelegateCommand<T> Create<T>(
        string action,
        Action<T> execute,
        Func<string> getUnit,
        Func<T, bool>? canExecute = null,
        Func<T, string>? formatTarget = null)
    {
        void Logged(T param)
        {
            var suffix = formatTarget != null ? formatTarget(param) : param?.ToString();
            var target = Combine(getUnit(), suffix);
            ExecuteLogged(action, target, () => execute(param));
        }

        return canExecute is null
            ? new DelegateCommand<T>(Logged)
            : new DelegateCommand<T>(Logged, canExecute);
    }

    private static void ExecuteLogged(string action, string target, Action execute)
    {
        try
        {
            execute();
            WriteSuccess(action, target);
        }
        catch (Exception ex)
        {
            WriteFailed(action, target, ex.Message);
            throw;
        }
    }

    private static void WriteSuccess(string action, string target) =>
        AuditLogService.WriteCurrentUser(action, "Success", string.Empty, AuditModules.Operation, target);

    private static void WriteFailed(string action, string target, string detail) =>
        AuditLogService.WriteCurrentUser(action, "Failed", detail, AuditModules.Operation, target);

    private static string Combine(string? unit, string? suffix)
    {
        var left = unit?.Trim() ?? string.Empty;
        var right = suffix?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(left))
            return right;
        if (string.IsNullOrEmpty(right))
            return left;
        return $"{left}/{right}";
    }
}
