using CMP.Constants.Plc.RobotControl;
using CMP.Services.Plc;
using CMP.ViewModels.Common;
using Prism.Commands;
using System;
using System.Windows.Threading;
using WPFLocalizeExtension.Extensions;
using CMP.Services.Auth;

namespace CMP.ViewModels.Tabs.OperationTab.RobotControlTab;

public class CacheBodyViewModel : TabContentViewModelBase
{
    private readonly MitsubishiPlcService _plc;
    private readonly DispatcherTimer _readTimer;
    private bool _isReading;

    private string? _tabTitleKey;
    public string? TabTitleKey
    {
        get => _tabTitleKey;
        set
        {
            if (SetProperty(ref _tabTitleKey, value))
            {
                RefreshTabTitle();
            }
        }
    }

    public void RefreshTabTitle()
    {
        if (!string.IsNullOrEmpty(TabTitleKey))
        {
            var loc = new LocExtension($"CMP:Resources:{TabTitleKey}");
            loc.ResolveLocalizedValue(out string? localizedValue);
            TabTitle = localizedValue ?? TabTitleKey;
        }
    }

    #region Cache Body Properties

    private bool _cacheADoorOpenStatus;
    public bool CacheADoorOpenStatus
    {
        get => _cacheADoorOpenStatus;
        set => SetProperty(ref _cacheADoorOpenStatus, value);
    }

    private bool _cacheADoorCloseStatus;
    public bool CacheADoorCloseStatus
    {
        get => _cacheADoorCloseStatus;
        set => SetProperty(ref _cacheADoorCloseStatus, value);
    }

    private bool _cacheAFiber1Status;
    public bool CacheAFiber1Status
    {
        get => _cacheAFiber1Status;
        set => SetProperty(ref _cacheAFiber1Status, value);
    }

    private bool _cacheALiquidLevelStatus;
    public bool CacheALiquidLevelStatus
    {
        get => _cacheALiquidLevelStatus;
        set => SetProperty(ref _cacheALiquidLevelStatus, value);
    }

    private bool _cacheADoorSwitchStatus;
    public bool CacheADoorSwitchStatus
    {
        get => _cacheADoorSwitchStatus;
        set => SetProperty(ref _cacheADoorSwitchStatus, value);
    }

    private bool _cacheBDoorOpenStatus;
    public bool CacheBDoorOpenStatus
    {
        get => _cacheBDoorOpenStatus;
        set => SetProperty(ref _cacheBDoorOpenStatus, value);
    }

    private bool _cacheBDoorCloseStatus;
    public bool CacheBDoorCloseStatus
    {
        get => _cacheBDoorCloseStatus;
        set => SetProperty(ref _cacheBDoorCloseStatus, value);
    }

    private bool _cacheBFiber1Status;
    public bool CacheBFiber1Status
    {
        get => _cacheBFiber1Status;
        set => SetProperty(ref _cacheBFiber1Status, value);
    }

    private bool _cacheBLiquidLevelStatus;
    public bool CacheBLiquidLevelStatus
    {
        get => _cacheBLiquidLevelStatus;
        set => SetProperty(ref _cacheBLiquidLevelStatus, value);
    }

    private bool _cacheBDoorSwitchStatus;
    public bool CacheBDoorSwitchStatus
    {
        get => _cacheBDoorSwitchStatus;
        set => SetProperty(ref _cacheBDoorSwitchStatus, value);
    }

    #endregion

    #region Output State Properties (Read-Only)

    private bool _cacheADoorOpenRead;
    public bool CacheADoorOpenRead
    {
        get => _cacheADoorOpenRead;
        set => SetProperty(ref _cacheADoorOpenRead, value);
    }

    private bool _cacheADoorCloseRead;
    public bool CacheADoorCloseRead
    {
        get => _cacheADoorCloseRead;
        set => SetProperty(ref _cacheADoorCloseRead, value);
    }

    private bool _cacheBDoorOpenRead;
    public bool CacheBDoorOpenRead
    {
        get => _cacheBDoorOpenRead;
        set => SetProperty(ref _cacheBDoorOpenRead, value);
    }

    private bool _cacheBDoorCloseRead;
    public bool CacheBDoorCloseRead
    {
        get => _cacheBDoorCloseRead;
        set => SetProperty(ref _cacheBDoorCloseRead, value);
    }

    #endregion

    #region Commands

    public DelegateCommand CacheADoorOpenCommand { get; }
    public DelegateCommand CacheADoorCloseCommand { get; }
    public DelegateCommand CacheBDoorOpenCommand { get; }
    public DelegateCommand CacheBDoorCloseCommand { get; }

    #endregion

    private string? _tabTitle;
    public new string? TabTitle
    {
        get => _tabTitle ?? base.TabTitle;
        set => SetProperty(ref _tabTitle, value);
    }

    public CacheBodyViewModel() : base(0, "缓存腔体")
    {
        _plc = GlobalService.RobotControlPlc;

        // 初始化定时器
        _readTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _readTimer.Tick += ReadTimer_Tick;
        _readTimer.Start();

        CacheADoorOpenCommand = OperationAudit.Create("CacheADoorOpen", ExecuteCacheADoorOpen, () => "CacheBody");
        CacheADoorCloseCommand = OperationAudit.Create("CacheADoorClose", ExecuteCacheADoorClose, () => "CacheBody");
        CacheBDoorOpenCommand = OperationAudit.Create("CacheBDoorOpen", ExecuteCacheBDoorOpen, () => "CacheBody");
        CacheBDoorCloseCommand = OperationAudit.Create("CacheBDoorClose", ExecuteCacheBDoorClose, () => "CacheBody");
    }

    private async void ReadTimer_Tick(object? sender, EventArgs e)
    {
        if (_isReading)
        {
            return;
        }

        if (!_plc.IsConnected && !MitsubishiPlcService.DebugMode) return;

        _isReading = true;

        try
        {
            await ReadPlcDataAsync();
        }
        finally
        {
            _isReading = false;
        }
    }

    private async Task ReadPlcDataAsync()
    {
        // A缓存腔体状态
        // 读取 Y 地址 (Operation Read)
        var cacheADoorOpenReadResult = await _plc.ReadAsync<bool>(CacheBodyAddress.CacheADoorOpenRead);
        if (cacheADoorOpenReadResult.IsSuccess) CacheADoorOpenRead = cacheADoorOpenReadResult.Content;

        var cacheADoorCloseReadResult = await _plc.ReadAsync<bool>(CacheBodyAddress.CacheADoorCloseRead);
        if (cacheADoorCloseReadResult.IsSuccess) CacheADoorCloseRead = cacheADoorCloseReadResult.Content;
        
        // A缓存腔体 - 传感器状态 reading
        var cacheADoorOpenResult = await _plc.ReadAsync<bool>(CacheBodyAddress.CacheADoorOpenStatus);
        if (cacheADoorOpenResult.IsSuccess) CacheADoorOpenStatus = cacheADoorOpenResult.Content;

        var cacheADoorCloseResult = await _plc.ReadAsync<bool>(CacheBodyAddress.CacheADoorCloseStatus);
        if (cacheADoorCloseResult.IsSuccess) CacheADoorCloseStatus = cacheADoorCloseResult.Content;

        var cacheAFiber1Result = await _plc.ReadAsync<bool>(CacheBodyAddress.CacheAFiber1Status);
        if (cacheAFiber1Result.IsSuccess) CacheAFiber1Status = cacheAFiber1Result.Content;

        var cacheALiquidLevelResult = await _plc.ReadAsync<bool>(CacheBodyAddress.CacheALiquidLevelStatus);
        if (cacheALiquidLevelResult.IsSuccess) CacheALiquidLevelStatus = cacheALiquidLevelResult.Content;

        var cacheADoorSwitchResult = await _plc.ReadAsync<bool>(CacheBodyAddress.CacheADoorSwitchStatus);
        if (cacheADoorSwitchResult.IsSuccess) CacheADoorSwitchStatus = cacheADoorSwitchResult.Content;

        // B缓存腔体状态
        // 读取 Y 地址 (Operation Read)
        var cacheBDoorOpenReadResult = await _plc.ReadAsync<bool>(CacheBodyAddress.CacheBDoorOpenRead);
        if (cacheBDoorOpenReadResult.IsSuccess) CacheBDoorOpenRead = cacheBDoorOpenReadResult.Content;

        var cacheBDoorCloseReadResult = await _plc.ReadAsync<bool>(CacheBodyAddress.CacheBDoorCloseRead);
        if (cacheBDoorCloseReadResult.IsSuccess) CacheBDoorCloseRead = cacheBDoorCloseReadResult.Content;

        // B缓存腔体 - 传感器状态 reading
        var cacheBDoorOpenResult = await _plc.ReadAsync<bool>(CacheBodyAddress.CacheBDoorOpenStatus);
        if (cacheBDoorOpenResult.IsSuccess) CacheBDoorOpenStatus = cacheBDoorOpenResult.Content;

        var cacheBDoorCloseResult = await _plc.ReadAsync<bool>(CacheBodyAddress.CacheBDoorCloseStatus);
        if (cacheBDoorCloseResult.IsSuccess) CacheBDoorCloseStatus = cacheBDoorCloseResult.Content;

        var cacheBFiber1Result = await _plc.ReadAsync<bool>(CacheBodyAddress.CacheBFiber1Status);
        if (cacheBFiber1Result.IsSuccess) CacheBFiber1Status = cacheBFiber1Result.Content;

        var cacheBLiquidLevelResult = await _plc.ReadAsync<bool>(CacheBodyAddress.CacheBLiquidLevelStatus);
        if (cacheBLiquidLevelResult.IsSuccess) CacheBLiquidLevelStatus = cacheBLiquidLevelResult.Content;

        var cacheBDoorSwitchResult = await _plc.ReadAsync<bool>(CacheBodyAddress.CacheBDoorSwitchStatus);
        if (cacheBDoorSwitchResult.IsSuccess) CacheBDoorSwitchStatus = cacheBDoorSwitchResult.Content;
    }

    private void ExecuteCacheADoorOpen()
    {
        _plc.Write(CacheBodyAddress.CacheADoorOpenWrite, true);
    }

    private void ExecuteCacheADoorClose()
    {
        _plc.Write(CacheBodyAddress.CacheADoorCloseWrite, true);
    }

    private void ExecuteCacheBDoorOpen()
    {
        _plc.Write(CacheBodyAddress.CacheBDoorOpenWrite, true);
    }

    private void ExecuteCacheBDoorClose()
    {
        _plc.Write(CacheBodyAddress.CacheBDoorCloseWrite, true);
    }
}
