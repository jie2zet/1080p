using CMP.Views.Tabs.RecipeTab;
using CMP.Views.Tabs.RecipeTab.Content;

namespace CMP.ViewModels.Tabs.MainTab.Content;

public class RecipesContentViewModel : BindableBase
{
    public RecipesContentViewModel(IRegionManager regionManager)
    {
        regionManager.RegisterViewWithRegion("RecipeEditorRegion", typeof(RecipeTabContentView));
        regionManager.RegisterViewWithRegion("SequenceRecipeEditor", typeof(SequenceRecipeEditorView));
    }
}