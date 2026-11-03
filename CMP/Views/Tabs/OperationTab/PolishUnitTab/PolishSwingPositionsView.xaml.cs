using System.Windows;
using System.Windows.Controls;
using CMP.ViewModels.Tabs.OperationTab.PolishUnitTab;

namespace CMP.Views.Tabs.OperationTab.PolishUnitTab;

public partial class PolishSwingPositionsView : UserControl
{
    public PolishSwingPositionsView()
    {
        InitializeComponent();
    }

    private void UpdateActivity()
    {
        if (DataContext is PolishSwingPositionsViewModel viewModel) viewModel.IsActive = IsVisible && IsLoaded;
    }

    private void OnVisibilityChanged(object sender, RoutedEventArgs e) => UpdateActivity();
    private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) => UpdateActivity();

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is PolishSwingPositionsViewModel previous) previous.IsActive = false;
        UpdateActivity();
    }
}
