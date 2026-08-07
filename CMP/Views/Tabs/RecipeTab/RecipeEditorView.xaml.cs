using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CMP.Models.Recipe;
using CMP.ViewModels.Tabs.RecipeTab;
using Prism.Ioc;

namespace CMP.Views.Tabs.RecipeTab;

public partial class RecipeEditorView
{
    public RecipeEditorView()
    {
        InitializeComponent();

        // 从容器中解析 IEventAggregator
        var eventAggregator = ContainerLocator.Container.Resolve<IEventAggregator>();
        DataContext = new RecipeEditorViewModel(MainDataGrid, eventAggregator);

        if (DataContext is RecipeEditorViewModel viewModel)
        {
            viewModel.RefreshEditor();
        }

    }
}