namespace CMP.ViewModels.Common;

public interface IViewLifecycleAware
{
    void OnViewActivated();
    void OnViewDeactivated();
}