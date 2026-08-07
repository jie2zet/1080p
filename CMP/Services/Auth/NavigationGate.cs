using System.Windows;
using CMP.Models.UserData;
using Prism.Mvvm;

namespace CMP.Services.Auth;

/// <summary>
/// 登录后按角色刷新主导航 Tab 可见性。
/// </summary>
public sealed class NavigationGate : BindableBase
{
    private static NavigationGate? _current;
    private readonly IPermissionService _permissionService;

    public NavigationGate(IPermissionService permissionService)
    {
        _permissionService = permissionService;
        _current = this;
        SessionContext.Instance.PropertyChanged += (_, _) => RefreshTabVisibility();
        RefreshTabVisibility();
    }

    public static NavigationGate Current =>
        _current ?? throw new InvalidOperationException("NavigationGate 尚未初始化");

    public Visibility RecipesTabVisibility
    {
        get => GetVisibility(MenuKey.Recipes);
    }

    public Visibility UserManageTabVisibility
    {
        get => GetVisibility(MenuKey.UserManage);
    }

    public void RefreshTabVisibility()
    {
        RaisePropertyChanged(nameof(RecipesTabVisibility));
        RaisePropertyChanged(nameof(UserManageTabVisibility));
    }

    private Visibility GetVisibility(MenuKey menuKey)
    {
        if (!SessionContext.Instance.IsAuthenticated)
        {
            return Visibility.Visible;
        }

        return _permissionService.CanAccessMenu(menuKey) ? Visibility.Visible : Visibility.Collapsed;
    }
}
