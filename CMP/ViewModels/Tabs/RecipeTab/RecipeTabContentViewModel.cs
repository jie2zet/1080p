using CMP.Views.Tabs.RecipeTab;

namespace CMP.ViewModels.Tabs.RecipeTab;

public class RecipeTabContentViewModel : BindableBase
{
    public RecipeTabContentViewModel(IRegionManager regionManager)
    {
        regionManager.RegisterViewWithRegion("RecipeInsideEditorRegion", typeof(RecipeEditorView));
    }
}