using CMP.Extensions;
using CMP.Models.UserData;
using CMP.Services.Auth;
using CMP.Utils;
using SqlSugar;
using System.Reflection;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace CMP.ViewModels.Tabs.MainTab.Content;

public class UserManageContentViewModel : BindableBase
{
    private const int PageSizeValue = 15;

    private readonly IDialogService _dialogService;
    private readonly IEventAggregator _eventAggregator;
    private List<UserModel> _allUserData = new();
    private List<UserModel> _userData = new();
    private UserModel? _selectedItem;
    private string _userNameFilter = string.Empty;
    private string _selectedLevelFilter = "全部";
    private int _page = 1;
    private int _totalPages = 1;
    private bool _suppressPageChange;

    public List<UserModel> UserData
    {
        get => _userData;
        set => SetProperty(ref _userData, value);
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

    public List<string> LevelFilters { get; } =
    [
        "全部",
        nameof(PermissionEnum.Administrator),
        nameof(PermissionEnum.Engineer),
        nameof(PermissionEnum.Operator)
    ];

    public string SelectedLevelFilter
    {
        get => _selectedLevelFilter;
        set => SetProperty(ref _selectedLevelFilter, value);
    }

    public UserModel? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public DelegateCommand<string> AddUserCommand { get; }
    public DelegateCommand DeleteCommand { get; }
    public DelegateCommand<object?> EditCommand { get; }
    public DelegateCommand RefreshCommand { get; }
    public DelegateCommand PageChangeCommand { get; }

    public UserManageContentViewModel(IDialogService dialog, IEventAggregator eventAggregator)
    {
        _dialogService = dialog;
        _eventAggregator = eventAggregator;

        AddUserCommand = new DelegateCommand<string>(AddUser);
        DeleteCommand = new DelegateCommand(DeleteSelectedItem);
        EditCommand = new DelegateCommand<object?>(EditModelCommand);
        RefreshCommand = new DelegateCommand(() => { Page = 1; QueryInfo(); });
        PageChangeCommand = new DelegateCommand(OnPageChange);

        _eventAggregator.GetEvent<AddGroupOrUserEvent>().Subscribe(OnUserChanged);
        LoadData();
    }

    private static ISqlSugarClient Db => SqlSugarHelper.Db;

    private void OnUserChanged(string _)
    {
        QueryInfo();
    }

    private void QueryInfo()
    {
        var query = Db.Queryable<UserModel>();

        if (!string.IsNullOrWhiteSpace(UserNameFilter))
        {
            var keyword = UserNameFilter.Trim();
            query = query.Where(u => u.UserName.Contains(keyword));
        }

        if (SelectedLevelFilter != "全部"
            && Enum.TryParse<PermissionEnum>(SelectedLevelFilter, true, out var level))
        {
            var levelName = level.ToString();
            query = query.Where(u => u.Level == level || u.Group == levelName);
        }

        SetUserDataSource(query.OrderBy(u => u.Id).ToList());
    }

    private void LoadData()
    {
        SetUserDataSource(Db.Queryable<UserModel>().OrderBy(u => u.Id).ToList());
    }

    private void SetUserDataSource(List<UserModel> data)
    {
        _suppressPageChange = true;
        try
        {
            _allUserData = data ?? new List<UserModel>();
            TotalPages = _allUserData.Count == 0
                ? 1
                : (int)Math.Ceiling(_allUserData.Count / (double)PageSizeValue);

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
        if (_allUserData.Count == 0)
        {
            UserData = new List<UserModel>();
            return;
        }

        var page = Math.Clamp(Page, 1, TotalPages);
        if (page != Page)
        {
            Page = page;
        }

        UserData = _allUserData
            .Skip((page - 1) * PageSizeValue)
            .Take(PageSizeValue)
            .ToList();
    }

    private void DeleteSelectedItem()
    {
        if (SelectedItem == null)
        {
            MessageBox.Show("请先选择要删除的用户。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (SessionContext.Instance.IsAuthenticated
            && SelectedItem.UserName.Equals(SessionContext.Instance.UserName, StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("不能删除当前登录用户。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (MessageBox.Show($"确定删除用户「{SelectedItem.UserName}」吗？", "确认删除",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
        {
            return;
        }

        var deleteResult = Db.Deleteable<UserModel>().Where(x => x.Id == SelectedItem.Id).ExecuteCommand();
        if (deleteResult > 0)
        {
            SelectedItem = null;
            QueryInfo();
            MessageBox.Show("删除成功。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show("删除失败。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void AddUser(string dialogName)
    {
        var param = new DialogParameters
        {
            { "Title", "Add user Data" },
            { "BlockName1", "UserName" },
            { "BlockName2", "Password" },
            { "BlockName3", "Group" }
        };

        _dialogService.ShowDialog(dialogName, param, r =>
        {
            if (r.Result != ButtonResult.OK)
            {
                return;
            }

            var user = r.Parameters.GetValue<UserModel>("AddUserResult");
            if (user == null)
            {
                return;
            }

            if (Db.Queryable<UserModel>().Any(u => u.UserName == user.UserName))
            {
                MessageBox.Show("用户名已存在。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ApplyPasswordHash(user, user.Password);
            user.IsEnabled = true;
            user.ModifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var insertResult = Db.Insertable(user).ExecuteCommand();
            if (insertResult > 0)
            {
                QueryInfo();
                MessageBox.Show("新增成功。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("新增失败。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        });
    }

    private void EditModelCommand(object? parameter)
    {
        var selectedUser = parameter as UserModel ?? SelectedItem;
        if (selectedUser == null)
        {
            MessageBox.Show("请先选择要编辑的用户。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        SelectedItem = selectedUser;

        var param = new DialogParameters
        {
            { "Title", "Edit user data" },
            { "BlockName1", "UserName" },
            { "BlockName2", "Password" },
            { "BlockName3", "Group" },
            { "TextBox1", selectedUser.UserName },
            { "TextBox2", string.Empty },
            { "TextBox3", selectedUser.Level.ToString() }
        };

        _dialogService.ShowDialog("UserManageDialogView", param, r =>
        {
            if (r.Result != ButtonResult.OK)
            {
                return;
            }

            var user = r.Parameters.GetValue<UserModel>("EditUserResult");
            if (user == null)
            {
                return;
            }

            user.Id = selectedUser.Id;
            user.IsEnabled = selectedUser.IsEnabled;
            user.ModifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (string.IsNullOrEmpty(user.Password))
            {
                user.PasswordHash = selectedUser.PasswordHash;
            }
            else
            {
                ApplyPasswordHash(user, user.Password);
            }

            var updateResult = Db.Updateable(user).ExecuteCommand();
            if (updateResult > 0)
            {
                QueryInfo();
                MessageBox.Show("编辑成功。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("编辑失败。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        });
    }

    private static void ApplyPasswordHash(UserModel user, string plainPassword)
    {
        user.PasswordHash = PasswordHasher.Hash(plainPassword);
        user.Password = string.Empty;
        user.Group = user.Level.ToString();
    }
}
