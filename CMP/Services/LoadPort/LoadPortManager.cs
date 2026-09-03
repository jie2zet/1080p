using CMP;
using CMP.Models.Operation.LoadPort;
using CMP.Models.Resource;
using CMP.Services.LoadPort;
using HandyControl.Controls;
using System.Collections.Specialized;

public class LoadPortManager
{
    private static readonly Lazy<LoadPortManager> _instance =
        new Lazy<LoadPortManager>(() => new LoadPortManager());

    private Dictionary<string, LoadPortData> _loadPorts;
    private ILoadPortService _loadPortService;
   // private MitsubishiPlcService _plcService = MitsubishiPlcService.Main;
    private bool _isInitialized = false;

    private LoadPortManager()
    {

    }

    public static LoadPortManager Instance => _instance.Value;
    public event Action<LoadPortData, List<Wafer>> AllSelectedWafersCompleted;
    private bool _isMonitoring = false;
    private Dictionary<string, bool> _portMonitoringStatus; // 记录每个端口的监控状态
    private Dictionary<string, List<Wafer>> _lastCheckedSelectedWafers; // 记录上次检查时选中的 wafers

    public void StartWaferMonitoring()
    {
        if (_isMonitoring || !_isInitialized) return;

        _isMonitoring = true;

        // 为每个 LoadPort 设置监控
        foreach (var kvp in _loadPorts)
        {
            var loadPort = kvp.Value;

            // 监控 WaferList 变化，并为新增的wafer注册监听器
            if (loadPort.WaferList is INotifyCollectionChanged notifyCollection)
            {
                notifyCollection.CollectionChanged += (s, e) => {
                    // 为新添加的wafer注册属性监听器
                    if (e.NewItems != null)
                    {
                        foreach (Wafer newWafer in e.NewItems)
                        {
                            RegisterWaferPropertyListener(loadPort, newWafer);
                        }
                    }
                    Task.Run(() => CheckUnloadCondition(loadPort));
                };
            }

            // 监控现有的每个 Wafer 属性变化
            foreach (var wafer in loadPort.WaferList)
            {
                RegisterWaferPropertyListener(loadPort, wafer);
            }
        }
    }

    private void RegisterWaferPropertyListener(LoadPortData loadPort, Wafer wafer)
    {
        wafer.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(Wafer.SequenceRecipe) ||
                e.PropertyName == nameof(Wafer.WaferStatus))
            {
                Task.Run(() => CheckUnloadCondition(loadPort));
            }
        };
    }

    private void CheckUnloadCondition(LoadPortData loadPort)
    {
        try
        {
            // 获取所有下发Sequence的 wafers
            var selectedWafers = loadPort.WaferList.Where(w => !string.IsNullOrEmpty(w.SequenceRecipe)).ToList();

            // 如果有选中的 wafers 且都已完成
            if (selectedWafers.Count > 0 &&
                selectedWafers.All(w => w.WaferStatus==WaferStatus.ProcessDone))
            {
                // 检查是否已经处理过这些wafer，避免重复执行
                bool shouldExecuteUnload = true;
                if (_lastCheckedSelectedWafers != null && _lastCheckedSelectedWafers.ContainsKey(loadPort.PortNumber))
                {
                    var lastWafers = _lastCheckedSelectedWafers[loadPort.PortNumber];
                    // 如果上次检查的wafer和当前一样，且状态都是Complete，则不重复执行
                    if (lastWafers.Count == selectedWafers.Count &&
                        lastWafers.All(lw => selectedWafers.Any(sw => sw.SlotID == lw.SlotID)) &&
                        lastWafers.All(lw => lw.WaferStatus==WaferStatus.ProcessDone))
                    {
                        shouldExecuteUnload = false;
                    }
                }

                if (shouldExecuteUnload)
                {
                    // 更新记录
                    if (_lastCheckedSelectedWafers == null)
                        _lastCheckedSelectedWafers = new Dictionary<string, List<Wafer>>();

                    _lastCheckedSelectedWafers[loadPort.PortNumber] = new List<Wafer>(selectedWafers.Cast<Wafer>());

                    // 执行unload命令
                   // ExecuteUnloadCommand(loadPort);

                    // 触发事件
                    Task.Run(() => {
                        AllSelectedWafersCompleted?.Invoke(loadPort, selectedWafers);
                    });
                }
            }
            else
            {
                // 如果没有选中的wafer或者不是所有wafer都完成，清除记录
                if (_lastCheckedSelectedWafers != null && _lastCheckedSelectedWafers.ContainsKey(loadPort.PortNumber))
                {
                    _lastCheckedSelectedWafers.Remove(loadPort.PortNumber);
                }
            }
        }
        catch (Exception ex)
        {
            // 记录异常但不中断程序
            Growl.ErrorGlobal($"检查Unload条件时出错: {ex.Message}");
        }
    }

    // 执行unload命令的方法
    private void ExecuteUnloadCommand(LoadPortData loadPort)
    {
        try
        {
            // 方法1: 直接调用LoadPortData的Unload命令
            if (loadPort.MOVCommand != null && loadPort.MOVCommand.CanExecute(null))
            {
                loadPort.MOVCommand.Execute(null);
                
                Growl.InfoGlobal($"LoadPort {loadPort.PortNumber} 执行Unload命令成功");
            }
           
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"执行Unload命令时出错: {ex.Message}");
        }
    }

    public async Task InitializeAsync(ILoadPortService loadPortService)
    {
        if (!_isInitialized)
        {
            lock (_instance)
            {
               
                _loadPortService = loadPortService;
                _loadPorts = new Dictionary<string, LoadPortData>();

                // 初始化5个LoadPort
                for (int i = 12; i < 16; i++)
                {
                    string portName = $"COM{i}";
                    var loadPort = new LoadPortData(portName, loadPortService);
                    _loadPorts.Add(portName, loadPort);
                }

                _isInitialized = true;
               
            }
        }
    }

    // 其余方法保持不变...
    public async Task<Dictionary<string, bool>> ConnectAllLoadPortsAsync()
    {
        var connectionResults = new Dictionary<string, bool>();

        if (!_isInitialized || _loadPorts == null)
        {
            return connectionResults;
        }

        foreach (var kvp in _loadPorts)
        {
            try
            {
                kvp.Value.ConnectCommand.Execute(null);
                connectionResults.Add(kvp.Key, kvp.Value.IsConnected);

                if (kvp.Value.IsConnected)
                {
                    kvp.Value.StartAutoUpdate();
                    kvp.Value.StartPlcOperationProcessing();
                }
            }
            catch (Exception ex)
            {
                connectionResults.Add(kvp.Key, false);
            }
        }

        return connectionResults;
    }

    public LoadPortData GetLoadPort(string portName)
    {
        if (!_isInitialized || _loadPorts == null || !_loadPorts.ContainsKey(portName))
            return null;

        return _loadPorts[portName];
    }

    public LoadPortData GetLoadPort(int index)
    {
        if (!_isInitialized || _loadPorts == null || index < 0 || index >= _loadPorts.Count)
            return null;

        var ports = new List<LoadPortData>(_loadPorts.Values);
        return ports[index];
    }

    public Dictionary<string, LoadPortData> GetAllLoadPorts()
    {
        if (!_isInitialized || _loadPorts == null)
            return new Dictionary<string, LoadPortData>();

        return new Dictionary<string, LoadPortData>(_loadPorts);
    }

    public IEnumerable<string> GetPortNames()
    {
        if (!_isInitialized || _loadPorts == null)
            return new List<string>();

        return _loadPorts.Keys;
    }

    public bool IsInitialized => _isInitialized;
}