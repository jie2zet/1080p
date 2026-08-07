using CMP.Models.UserData;

namespace CMP.Services.Auth;

public interface IPermissionService
{
    bool CanAccessMenu(MenuKey menuKey, PermissionEnum? role = null);
}
