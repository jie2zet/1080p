using CMP.Services.LoadPort;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.IO.Ports;
using System.Text;
using CMP.Services.Log;
using HandyControl.Controls;
using Azure;
using CMP.Models.Resource;
using CMP.Models.Operation.LoadPort;


namespace CMP.ViewModels.Tabs.OperationTab
{
    public class SerialPortContentViewModel : BindableBase
    {

        private ILoadPortService _serialService;
        private string _selectedPort;
        private string _statusMessage;
        private string _moveAction;
        private string _setParameter;
        private string _setValue;
        private string _getParameter;
        private string _getResult;
        private string _queryText;
        private string _queryResult;
        private ObservableCollection<Wafer> _wafers;
        private LoadPortData _loadPortData;
        // MOV 命令相关属性
        private ObservableCollection<string> _availableMoveActions;
        private string _selectedMoveAction;

        public ObservableCollection<string> AvailableMoveActions
        {
            get => _availableMoveActions;
            set => SetProperty(ref _availableMoveActions, value);
        }

        public ObservableCollection<Wafer> Wafers
        {
            get => _wafers;
            set => SetProperty(ref _wafers, value);
        }
        public string SelectedMoveAction
        {
            get => _selectedMoveAction;
            set => SetProperty(ref _selectedMoveAction, value);
        }
        public LoadPortData LoadPortData
        {
            get => _loadPortData;
            set => SetProperty(ref _loadPortData, value);
        }
        // SET 命令相关属性
        private ObservableCollection<string> _availableSetParameters;
        private string _selectedSetParameter;

        public ObservableCollection<string> AvailableSetParameters
        {
            get => _availableSetParameters;
            set => SetProperty(ref _availableSetParameters, value);
        }

        public string SelectedSetParameter
        {
            get => _selectedSetParameter;
            set => SetProperty(ref _selectedSetParameter, value);
        }

       

        // GET 命令相关属性
        private ObservableCollection<string> _availableGetParameters;
        private string _selectedGetParameter;

        public ObservableCollection<string> AvailableGetParameters
        {
            get => _availableGetParameters;
            set => SetProperty(ref _availableGetParameters, value);
        }

        public string SelectedGetParameter
        {
            get => _selectedGetParameter;
            set => SetProperty(ref _selectedGetParameter, value);
        }

        // Query 命令相关属性
        private ObservableCollection<string> _availableQueryCommands;
        private string _selectedQueryCommand;

        public ObservableCollection<string> AvailableQueryCommands
        {
            get => _availableQueryCommands;
            set => SetProperty(ref _availableQueryCommands, value);
        }

        public string SelectedQueryCommand
        {
            get => _selectedQueryCommand;
            set => SetProperty(ref _selectedQueryCommand, value);
        }

       


        private void InitializeCommandCollections()
        {
            AvailablePorts = new ObservableCollection<String>(SerialPort.GetPortNames());
            // 初始化 MOV 命令选项
            AvailableMoveActions = new ObservableCollection<string>
            {
                "ORGSH",
                "CLDMP",
                "CLOAD",
                "CLDDK",
                "CLDYD",
                "CLD0P",
                "CLMPO",
                "CULOD",
                "CUDMP",
                "CULDK",
                "CUMDK",
                "CULYD",
                "CUDNC",
                "CUDCL",
                "CULFC",
                "CUMFC",
                "REMAP",
                "MAPD1",
                "MAPD2",
                "MAPDO",
                "CUDCL",
                "CULFC",
                "CUMFC",
                // 添加其他 MOV 命令
                "STOP_",
                "PODCL",
                "PODOP",
                "YDOOR",
                "YWAIT",
                "VACON",
                "VACOF",
                "DOROP",
                "DORCL",
                "DORBK",
                "DORFW",
                "MAPOP",
                "MAPCL",
                "ZMPUP",
                "ZMPDW",
                "ZMPST",
                "ZMPED",
                "ZDRMP",
                "ZMDMP"
            };

            // 初始化 SET 参数选项
            AvailableSetParameters = new ObservableCollection<string>
            {
                "RESET",
                "SAVE1",
                "LPCON",
                "PARAM",
                // 添加其他 SET 参数
            };

            // 初始化 GET 参数选项
            AvailableGetParameters = new ObservableCollection<string>
            {
                "STATE",
                "VERSN",
                "PARAM",
                "MAPDT",
                "MAPRD",
                "WFCNT",
                "LEDST",
                "SELOP"
                // 添加其他 GET 参数
            };

            // 初始化 Query 命令选项
            AvailableQueryCommands = new ObservableCollection<string>
            {
                "SYSTEM_INFO",
                "VERSION",
                "CONFIG",
                "DIAGNOSTICS",
                // 添加其他 Query 命令
            };

            // 设置默认选中项
            if (AvailableMoveActions.Any())
                SelectedMoveAction = AvailableMoveActions[0];

            if (AvailableSetParameters.Any())
                SelectedSetParameter = AvailableSetParameters[0];

            if (AvailableGetParameters.Any())
                SelectedGetParameter = AvailableGetParameters[0];

            if (AvailableQueryCommands.Any())
                SelectedQueryCommand = AvailableQueryCommands[0];

            if (AvailablePorts.Any())
                SelectedPort = AvailablePorts[0];
        }

        private CancellationTokenSource _cancellationTokenSource;
        private Task _statusUpdateTask;
        private LoadPortStatus _currentStatus;
        private string _statusText;
        private string _detailedStatus;
        private string _statusColor;
        private bool _isAutoUpdateEnabled;
        private int _updateInterval = 5000; // 默认5秒更新一次





        // 修改为私有字段并添加公共属性
        private ObservableCollection<string> _availablePorts;
        private ObservableCollection<string> _communicationLog;

        // 公共属性
        public ObservableCollection<string> AvailablePorts
        {
            get => _availablePorts;
            set => SetProperty(ref _availablePorts, value);
        }

        public ObservableCollection<string> CommunicationLog
        {
            get => _communicationLog;
            set => SetProperty(ref _communicationLog, value);
        }

        public string SelectedPort
        {
            get => _selectedPort;
            set => SetProperty(ref _selectedPort, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }



        public string SetValue
        {
            get => _setValue;
            set => SetProperty(ref _setValue, value);
        }

        public String QueryText
        {
            get => _queryText;
            set => SetProperty(ref _queryText, value);
        }

        public string GetResult
        {
            get => _getResult;
            set => SetProperty(ref _getResult, value);
        }



        public string QueryResult
        {
            get => _queryResult;
            set => SetProperty(ref _queryResult, value);
        }


        // 新增状态相关属性
        public LoadPortStatus CurrentStatus
        {
            get => _currentStatus;
            set => SetProperty(ref _currentStatus, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public string DetailedStatus
        {
            get => _detailedStatus;
            set => SetProperty(ref _detailedStatus, value);
        }

        public string StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }

        

        public int UpdateInterval
        {
            get => _updateInterval;
            set => SetProperty(ref _updateInterval, value);
        }

        

        // 获取状态文本
        private string GetStatusText(LoadPortStatus status)
        {
            // 根据设备状态确定显示文本
            switch (status.EquipmentStatus)
            {
                case EquipmentStatus.NoError:
                    return "NoError";
                case EquipmentStatus.Error:
                    return "Error";
                default:
                    return "Unknown";
            }
        }

        // 获取详细状态描述
        private string GetDetailedStatus(LoadPortStatus status)
        {
            switch (status.EquipmentStatus)
            {
                case EquipmentStatus.NoError:
                   return $"LoadPort is ready. Mode: {status.Mode}, Operation: {status.operationStatus}";
                case EquipmentStatus.Error:
                    return $"Error occurred. Code: {status.ErrorCode}";
                default:
                    return "Unknown status";
            }
        }

        // 根据状态确定颜色
        private string GetStatusColor(LoadPortStatus status)
        {
            switch (status.EquipmentStatus)
            {
                case EquipmentStatus.NoError:
                    return "Green";
                case EquipmentStatus.Error:
                    return "Red";                    
                default:
                    return "Gray";
            }
        }

        // 手动更新状态的方法（可用于按钮点击等场景）
        public void UpdateStatusManually()
        {
            if (!string.IsNullOrEmpty(SelectedPort))
            {
                try
                {
                    // 假设我们使用第一个LoadPort (索引为0)
                    // var status = _serialService.GetLoadPortStatus(1);
                    //UpdateStatusProperties(status);
                }
                catch (Exception ex)
                {
                    StatusText = "Error";
                    DetailedStatus = $"Failed to get status: {ex.Message}";
                    StatusColor = "Red";
                }
            }
        }





        // 添加清空日志的命令属性
        public ICommand ClearLogCommand { get; }



        // 清空日志的方法
        private void ClearLog()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                LoadPortData.LogMessages.Clear();
            });
        }






        #region Commands

        public ICommand RefreshPortsCommand { get; }
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand SendMoveCommand { get; }
        public ICommand SendSetCommand { get; }
        public ICommand SendGetCommand { get; }
        public ICommand SendQueryCommand { get; }

        #endregion

        public SerialPortContentViewModel(ILoadPortService serialService)
        {
            _serialService = serialService;

            // 初始化命令
            RefreshPortsCommand = new DelegateCommand(OnRefreshPorts);
            ConnectCommand = new DelegateCommand(OnConnect, CanConnect).ObservesProperty(() => SelectedPort);
            DisconnectCommand = new DelegateCommand(OnDisconnect, CanDisconnect);
            SendMoveCommand = new DelegateCommand(OnSendMoveCommand, CanSendCommands).ObservesProperty(() => SelectedMoveAction);
            SendSetCommand = new DelegateCommand(OnSendSetCommand, CanSendSetCommand);
            SendGetCommand = new DelegateCommand(OnSendGetCommand, CanSendGetCommand);
            SendQueryCommand = new DelegateCommand(OnSendQueryCommand, CanSendQueryCommand);
            // 初始化清空日志命令
            ClearLogCommand = new DelegateCommand(ClearLog);
            // 订阅响应事件
            _serialService.ResponseReceived += SerialService_ResponseReceived;
            // 初始化命令集合
            InitializeCommandCollections();

            CommunicationLog = new ObservableCollection<string>();

        }

        private void SerialService_ResponseReceived(object sender, SerialResponseEventArgs e)
        {
            UpdateStatusManually();
            // 更新状态消息
            StatusMessage = $"Received response for {e.Command}: {e.Response}";

            // 添加到通信日志
            //App.Current.Dispatcher.Invoke(() =>
            //{
            //    CommunicationLog.Add($"[{DateTime.Now:HH:mm:ss}] {e.Command}: {e.Response}");
            //});
        }

        private void OnRefreshPorts()
        {
            AvailablePorts = new ObservableCollection<String>(SerialPort.GetPortNames());
            StatusMessage = "Ports refreshed";
        }


        private bool CanConnect()
        {

            return true;
        }

        private void OnConnect()
        {
            try
            {
                LoadPortData = LoadPortManager.Instance.GetLoadPort(SelectedPort);

                //_loadPortData.DataReceived += (sender, e) => _loadPortData_DataReceived(sender, e);
                // _loadPortData.Open();
                bool success = LoadPortData.IsConnected;
                StatusMessage = success ? $"Connected to {SelectedPort}" : $"Failed to connect to {SelectedPort}";
                ((DelegateCommand)DisconnectCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)SendMoveCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)SendSetCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)SendGetCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)SendQueryCommand).RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to connect to {SelectedPort},Case:{ex.Message}";
            }

        }        
       
        
        
        private bool CanDisconnect()
        {

            return LoadPortData == null ? false: LoadPortData.IsConnected;
        }

        private void OnDisconnect()
        {
            if(LoadPortData != null) LoadPortData.DisconnectCommand.Execute(null);
            StatusMessage = "Disconnected";
            ((DelegateCommand)DisconnectCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)SendMoveCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)SendSetCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)SendGetCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)SendQueryCommand).RaiseCanExecuteChanged();
        }

        private bool CanSendCommands()
        {
            return LoadPortData == null ? false : LoadPortData.IsConnected;
        }

        private async void OnSendMoveCommand()
        {
            StatusMessage = "Sending move command...";
            LoadPortData.MOVCommand.Execute(SelectedMoveAction);
            //bool success = await _serialService.SendMoveCommandAsync(,.);
            CommunicationLog.Add($"Mov command sent: MOV:{SelectedMoveAction};");
            StatusMessage = "Mov command sent";
        }

        private bool CanSendSetCommand()
        {
            return LoadPortData == null ? false : LoadPortData.IsConnected;

        }

        private async void OnSendSetCommand()
        {
            StatusMessage = "Sending set command...";
            //bool success = await _serialService.SendSetCommandAsync(SelectedSetParameter, SetValue,serialPort);
            LoadPortData.SETCommand.Execute("");
            CommunicationLog.Add($"Set command sent: SET:{SelectedSetParameter}/{SetValue};");
            StatusMessage = "Set command sent" ;
        }

        private bool CanSendGetCommand()
        {
            return LoadPortData == null ? false : LoadPortData.IsConnected;
        }

        private async void OnSendGetCommand()
        {
            StatusMessage = "Sending get command...";
           // bool success = await _serialService.SendGetCommandAsync(SelectedGetParameter,serialPort);
            CommunicationLog.Add($"Get command sent: Get:{SelectedGetParameter};");
            LoadPortData.GETCommand.Execute(SelectedGetParameter);
            StatusMessage = "Get command completed" ;
        }

        private bool CanSendQueryCommand()
        {
            return LoadPortData == null ? false : LoadPortData.IsConnected;
        }

        private async void OnSendQueryCommand()
        {
            StatusMessage = "Sending query command...";
            // QueryResult = await _serialService.SendQueryCommandAsync(QueryText,SelectedPort,1);
            StatusMessage = QueryResult != null ? "Query command completed" : "Failed to get query response";
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            // _serialService.Initialize();
            StatusMessage = "Ready";
        }
    }
}
