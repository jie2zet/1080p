using System.Windows;
using System.Windows.Controls;

namespace CMP.Views.Tabs.WarnTab;

public partial class AlarmTabContentView
{
    private const int RowsPerPage = 15;

    public AlarmTabContentView()
    {
        InitializeComponent();
        AlarmGrid.SizeChanged += (_, _) => UpdateRowHeight();
        Loaded += (_, _) => UpdateRowHeight();
    }

    private void UpdateRowHeight()
    {
        if (AlarmGrid.ActualHeight <= 0)
        {
            return;
        }

        var borderHeight = AlarmGrid.BorderThickness.Top + AlarmGrid.BorderThickness.Bottom;
        var availableHeight = AlarmGrid.ActualHeight - AlarmGrid.ColumnHeaderHeight - borderHeight;
        if (availableHeight <= 0)
        {
            return;
        }

        AlarmGrid.RowHeight = availableHeight / RowsPerPage;
    }

    private void Add_btn_Click(object sender, RoutedEventArgs e)
    {
    }
}
