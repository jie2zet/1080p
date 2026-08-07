using System.Windows.Controls;

namespace CMP.Views.Tabs.MainTab.Content;

public partial class AuditLogContentView
{
    private const int RowsPerPage = 15;

    public AuditLogContentView()
    {
        InitializeComponent();
        AuditLogGrid.SizeChanged += (_, _) => UpdateRowHeight();
        Loaded += (_, _) => UpdateRowHeight();
    }

    private void UpdateRowHeight()
    {
        if (AuditLogGrid.ActualHeight <= 0)
        {
            return;
        }

        var borderHeight = AuditLogGrid.BorderThickness.Top + AuditLogGrid.BorderThickness.Bottom;
        var availableHeight = AuditLogGrid.ActualHeight - AuditLogGrid.ColumnHeaderHeight - borderHeight;
        if (availableHeight <= 0)
        {
            return;
        }

        AuditLogGrid.RowHeight = availableHeight / RowsPerPage;
    }
}
