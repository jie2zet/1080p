using CMP.Models.UserData;
using CMP.Services.Auth;
using CMP.Utils;

namespace CMP.ViewModels.Tabs.MainTab.Content;

public class AuditLogContentViewModel : BindableBase
{
    private const int PageSizeValue = 15;

    private List<AuditLogModel> _allAuditLogs = new();
    private List<AuditLogModel> _auditLogs = new();
    private string _userNameFilter = string.Empty;
    private string _selectedActionFilter = "全部";
    private DateTime _startTime = DateTime.Today;
    private DateTime _endTime = DateTime.Today.AddDays(1).AddSeconds(-1);
    private int _page = 1;
    private int _totalPages = 1;
    private bool _suppressPageChange;

    public List<AuditLogModel> AuditLogs
    {
        get => _auditLogs;
        set => SetProperty(ref _auditLogs, value);
    }

    public int PageSize => PageSizeValue;

    public int Page
    {
        get => _page;
        set => SetProperty(ref _page, value);
    }

    public int TotalPages
    {
        get => _totalPages;
        set => SetProperty(ref _totalPages, value);
    }

    public string UserNameFilter
    {
        get => _userNameFilter;
        set => SetProperty(ref _userNameFilter, value);
    }

    public List<string> ActionFilters { get; } =
    [
        "全部",
        "Login",
        "Logout",
        "SessionTimeout",
        "ChangePassword",
        AuditActions.RecipeSave,
        AuditActions.RecipeCreate,
        AuditActions.RecipeDelete,
        AuditActions.RecipeRename,
        AuditActions.RecipeSaveAs,
        AuditActions.ConfigApply,
        AuditActions.ConfigSyncPlc,
        AuditActions.ConfigExport
    ];

    public string SelectedActionFilter
    {
        get => _selectedActionFilter;
        set => SetProperty(ref _selectedActionFilter, value);
    }

    public DateTime StartTime
    {
        get => _startTime;
        set => SetProperty(ref _startTime, value);
    }

    public DateTime EndTime
    {
        get => _endTime;
        set => SetProperty(ref _endTime, value);
    }

    public DelegateCommand RefreshCommand { get; }

    public DelegateCommand PageChangeCommand { get; }

    public AuditLogContentViewModel()
    {
        RefreshCommand = new DelegateCommand(() =>
        {
            Page = 1;
            QueryInfo();
        });
        PageChangeCommand = new DelegateCommand(OnPageChange);
        LoadData();
    }

    private void QueryInfo()
    {
        var data = AuditLogService.Query(UserNameFilter, SelectedActionFilter, StartTime, EndTime);
        SetAuditLogSource(data);
    }

    private void LoadData()
    {
        var data = AuditLogService.Query(null, "全部", StartTime, EndTime);
        SetAuditLogSource(data);
    }

    private void SetAuditLogSource(List<AuditLogModel> data)
    {
        _suppressPageChange = true;
        try
        {
            _allAuditLogs = data ?? new List<AuditLogModel>();
            TotalPages = _allAuditLogs.Count == 0
                ? 1
                : (int)Math.Ceiling(_allAuditLogs.Count / (double)PageSizeValue);

            var targetPage = Math.Clamp(Page, 1, TotalPages);
            if (Page != targetPage)
            {
                Page = targetPage;
            }

            ApplyPagination();
        }
        finally
        {
            _suppressPageChange = false;
        }
    }

    private void OnPageChange()
    {
        if (_suppressPageChange)
        {
            return;
        }

        ApplyPagination();
    }

    private void ApplyPagination()
    {
        if (_allAuditLogs.Count == 0)
        {
            AuditLogs = new List<AuditLogModel>();
            return;
        }

        var page = Math.Clamp(Page, 1, TotalPages);
        if (page != Page)
        {
            Page = page;
        }

        AuditLogs = _allAuditLogs
            .Skip((page - 1) * PageSizeValue)
            .Take(PageSizeValue)
            .ToList();
    }
}
