using CMP.Models.Warn;
using CMP.Services.Warn;
using DryIoc.ImTools;
using System.Security.Claims;
using CMP.Extensions;

namespace CMP.ViewModels.Tabs.WarnTab
{
    public enum AlarmType { 
        Alarm = 1,
        Warning = 2
    
    }
    public enum DefaultOperation
    {
        None = 0,
        Stop = 1,
        Skip = 2,
        Abort = 3

    }
    public class AlarmTabContentViewModel :BindableBase,INavigationAware
    {
        private const int PageSizeValue = 15;

        private List<Alarm> _allAlarmData = new();
        private List<Alarm> _alarmData = new();
        private int _page = 1;
        private int _totalPages = 1;
        private bool _suppressPageChange;
       
        public List<Alarm> AlarmData
        {
            get => _alarmData;
            set => SetProperty(ref _alarmData, value);
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

        private string _alarmName;
        public String AlarmName {

            get => _alarmName;
            set => SetProperty(ref _alarmName, value);
        }
        public List<DefaultOperation> DefaultOperations { get; }
        private DefaultOperation _selectedOperation;
        public DefaultOperation SelectedOperation
        {
            get => _selectedOperation;
            set => SetProperty(ref _selectedOperation, value);
            
        }

        public List<AlarmType> AlarmTypes { get; }
        private AlarmType _selectedAlarmType;
        public AlarmType SelectedAlarmType
        {
            get => _selectedAlarmType;
            set => SetProperty(ref _selectedAlarmType, value);

        }
        private DateTime _startTime = DateTime.Now;
        private DateTime _endTime = DateTime.Now.AddHours(1);

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
        private readonly IAlarmService _alarmService = new AlarmService();

        public DelegateCommand? RefreshCommand { get; set; }
        public DelegateCommand<string>? InsertCommand { get; set; }
        public DelegateCommand EditCommand { get; set; }
        public DelegateCommand PageChangeCommand { get; set; }

        private readonly IDialogService _dialogService;

        private readonly IEventAggregator _eventAggregator;
        public AlarmTabContentViewModel(IDialogService dialogService,IEventAggregator eventAggregator) {
            _dialogService = dialogService;
            _eventAggregator = eventAggregator;
            InsertCommand = new DelegateCommand<string>(OnAddStep);
            RefreshCommand = new DelegateCommand(Refresh);
            EditCommand = new DelegateCommand(OnEdit);
            PageChangeCommand = new DelegateCommand(OnPageChange);
            // 初始化枚举列表
            AlarmTypes = Enum.GetValues(typeof(AlarmType))
                             .Cast<AlarmType>()
                             .ToList();
            // 默认选中第一个
            SelectedAlarmType = AlarmTypes.FirstOrDefault();


            // 初始化枚举列表
            DefaultOperations = Enum.GetValues(typeof(DefaultOperation))
                             .Cast<DefaultOperation>()
                             .ToList();
            // 默认选中第一个
            SelectedOperation = DefaultOperations.FirstOrDefault();

            _eventAggregator.GetEvent<OperateEvent>().Subscribe(OnDialogClosed);
            LoadData();
        }

        

        private Alarm _selectedItem;
        private List<Alarm> _selectedItems;

        public Alarm SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
               

            }
        }

        private void OnAddStep(String obj)
        {
            DialogParameters dialogParameters2 = new DialogParameters();
            dialogParameters2.Add("Title", "新增");
            dialogParameters2.Add("FormName", "AlarmEditFormView");
            dialogParameters2.Add("Height", "500");
            dialogParameters2.Add("Width", "600");
            this._dialogService.ShowDialog(obj, dialogParameters2);
        }

        private void OnEdit()
        { 
            DialogParameters dialogParameters2 = new DialogParameters();
            dialogParameters2.Add("Title", "编辑");
            dialogParameters2.Add("Data", SelectedItem);
            dialogParameters2.Add("FormName", "AlarmEditFormView");
            dialogParameters2.Add("Height", "500");
            dialogParameters2.Add("Width", "600");
            this._dialogService.ShowDialog("FormDialog",  dialogParameters2);
        }
        private void Refresh()
        {
           
            Alarm alarm = new Alarm() { AlarmName = AlarmName ,Type= SelectedAlarmType.ToString(),DefaultOperation=SelectedOperation.ToString() };
            
            var data = _alarmService.GetAlarmsByCondition(alarm,StartTime,EndTime);
            SetAlarmDataSource(data);
        }

        private void OnDialogClosed(string obj)
        {
            LoadData();
        }

        private void LoadData()
        {
            var data = _alarmService.GetAllAlarms();
            SetAlarmDataSource(data);
        }

        private void SetAlarmDataSource(List<Alarm> data)
        {
            _suppressPageChange = true;
            try
            {
                _allAlarmData = data ?? new List<Alarm>();
                TotalPages = _allAlarmData.Count == 0
                    ? 1
                    : (int)Math.Ceiling(_allAlarmData.Count / (double)PageSizeValue);
                Page = 1;
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
            if (_allAlarmData.Count == 0)
            {
                AlarmData = new List<Alarm>();
                return;
            }

            var page = Math.Clamp(Page, 1, TotalPages);
            if (page != Page)
            {
                Page = page;
            }

            AlarmData = _allAlarmData
                .Skip((page - 1) * PageSizeValue)
                .Take(PageSizeValue)
                .ToList();
        }


       
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Refresh();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }
    }

}

