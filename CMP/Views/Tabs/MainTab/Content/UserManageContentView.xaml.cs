using System.Windows.Controls;

namespace CMP.Views.Tabs.MainTab.Content;

public partial class UserManageContentView
{
    private const int RowsPerPage = 15;

    public UserManageContentView()
    {
        InitializeComponent();
        UsersGrid.SizeChanged += (_, _) => UpdateRowHeight();
        Loaded += (_, _) => UpdateRowHeight();
    }

    private void UpdateRowHeight()
    {
        if (UsersGrid.ActualHeight <= 0)
        {
            return;
        }

        var borderHeight = UsersGrid.BorderThickness.Top + UsersGrid.BorderThickness.Bottom;
        var availableHeight = UsersGrid.ActualHeight - UsersGrid.ColumnHeaderHeight - borderHeight;
        if (availableHeight <= 0)
        {
            return;
        }

        UsersGrid.RowHeight = availableHeight / RowsPerPage;
    }
}
