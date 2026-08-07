using CMP.Models.UserData;

namespace CMP.Services.Auth;

public class PermissionService : IPermissionService
{
    public bool CanAccessMenu(MenuKey menuKey, PermissionEnum? role = null)
    {
        role ??= SessionContext.Instance.Level;

        if (!SessionContext.Instance.IsAuthenticated)
        {
            return false;
        }

        return menuKey switch
        {
            MenuKey.UserManage => role == PermissionEnum.Administrator,
            MenuKey.Recipes => role != PermissionEnum.Operator,
            _ => true
        };
    }
}
