using CMP;
using CMP.Constants.Plc.RobotControl;
using CMP.Models.DataCenter;
using CMP.Services.LoadPort;
using CMP.Services.Log;
using CMP.Services.Plc;
using Dm.util;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using HandyControl.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace CMP.Models.Resource;

public class RoboticArm : BindableBase
{
    private string _headColor = "#2C66A7";
    private ChamberStatus _robotStatus;
    private ChamberStatus _preRobotStatus;
    private string _name;
    private STCtrl? _linkedStCtrl;
    private int _err;
    private MitsubishiPlcService mcComm = MitsubishiPlcService.Instance;
    private readonly MitsubishiPlcService _robotPlc = MitsubishiPlcService.RobotCtrlInstance;
    private DBitAddress? _autoModeDBit;
    private string _startPermitionVarName;
    private string _readyVarName;
    private string _autoStartVarName;
    private string _resetErrorVarName;
    private string _initialStatusVarName;
    private string _alarmStatusVarName;

    private string _initCommandValue;
    private string _startCommandValue;
    private string _autoModeCommandValue;
    private string _manualModeCommandValue;
    private readonly object _statusLock = new();
    private readonly object _modeSwitchLock = new();
    //private ushort _abortCommandValue;

    private string _waferInfo = "";
    public string WaferInfo
    {
        get => _waferInfo;
        set => SetProperty(ref _waferInfo, value);
    }
    private bool _isVisibleWafer;
    public bool IsVisibleWafer
    {
        get => _isVisibleWafer;
        set => SetProperty(ref _isVisibleWafer, value);
    }

    private string _upperArmWaferInfo = "";
    public string UpperArmWaferInfo
    {
        get => _upperArmWaferInfo;
        set => SetProperty(ref _upperArmWaferInfo, value);
    }

    private bool _isVisibleUpperArmWafer;
    public bool IsVisibleUpperArmWafer
    {
        get => _isVisibleUpperArmWafer;
        set => SetProperty(ref _isVisibleUpperArmWafer, value);
    }

    private string _lowerArmWaferInfo = "";
    public string LowerArmWaferInfo
    {
        get => _lowerArmWaferInfo;
        set => SetProperty(ref _lowerArmWaferInfo, value);
    }

    private bool _isVisibleLowerArmWafer;
    public bool IsVisibleLowerArmWafer
    {
        get => _isVisibleLowerArmWafer;
        set => SetProperty(ref _isVisibleLowerArmWafer, value);
    }
    private List<string> _waferTrackingAddresses = new List<string>();
    public List<string> WaferTrackingAddresses
    {
        get => _waferTrackingAddresses;
        set => SetProperty(ref _waferTrackingAddresses, value);
    }
    private byte[] _writeBuf = new byte[2048];
    private byte[] _readBuf = new byte[2048];

    private bool _isResetErrorNeedClear;
    private int _resetErrorDelayCycle;
    private bool _isAutoRunNeedClear;
    private int _resetAutoRunCycle;

    /// <summary>与 SV_STCtrl.csv 中 Module 列一致（如 EFEMRobot）。</summary>
    public string RobotName => _name;

    /// <summary>主界面状态行：来自 DataCenter / SV_STCtrl.csv 的实时点位，由 DataCenter 采集任务刷新。</summary>
    public ObservableCollection<SV_PLC>? StCtrlStatusPoints => _linkedStCtrl?.listDataInSingleStCtrl;

    /// <summary>将本机械手与 DataCenter.listSTCtrl 中同 Module 名的 STCtrl 关联。</summary>
    public void AttachStCtrl(STCtrl? stCtrl)
    {
        _linkedStCtrl = stCtrl;
        RaisePropertyChanged(nameof(StCtrlStatusPoints));
    }

    /// <summary>绑定与触摸屏一致的自动模式 D 位（机器人 PLC 7005）。</summary>
    public void AttachAutoModeDBit(DBitAddress autoModeDBit)
    {
        _autoModeDBit = autoModeDBit;
    }

    private static string DeriveBitAddress(string address, int offset)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return null;
        }

        var match = Regex.Match(address.Trim(), @"^(?<prefix>[A-Za-z]+)(?<number>\d+)$");
        if (!match.Success || !int.TryParse(match.Groups["number"].Value, out var number))
        {
            return null;
        }

        return $"{match.Groups["prefix"].Value}{number + offset}";
    }

    private static bool TryReadPlcBool(MitsubishiPlcService plc, string address, out bool value)
    {
        value = false;
        if (plc == null || string.IsNullOrWhiteSpace(address) || !plc.IsPLCConnected)
        {
            return false;
        }

        try
        {
            var result = plc.Read<bool>(address);
            if (!result.IsSuccess)
            {
                return false;
            }

            value = result.Content;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryReadDBit(MitsubishiPlcService plc, DBitAddress dBit, out bool value)
    {
        value = false;
        if (plc == null || dBit == null || !plc.IsPLCConnected)
        {
            return false;
        }

        try
        {
            var result = plc.ReadDBit(dBit);
            if (!result.IsSuccess)
            {
                return false;
            }

            value = result.Content;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 机械手状态点在机器人 PLC（7005）。主站读成功但为 false 时不能挡住 7005。
    /// </summary>
    private bool TryReadRobotBool(string address, out bool value)
    {
        if (TryReadPlcBool(_robotPlc, address, out value))
        {
            return true;
        }

        return TryReadPlcBool(mcComm, address, out value);
    }

    private bool TryReadStartBit(out bool isStarted)
    {
        // running = initdone + 2（如 EFEM M6153→M6155），这是保持的启动状态。
        // 命令脉冲 init+3 只在按下期间为 true。
        var runningAddress = DeriveBitAddress(_initialStatusVarName, 2);
        if (TryReadRobotBool(runningAddress, out isStarted) && isStarted)
        {
            return true;
        }

        return TryReadRobotBool(_startCommandValue, out isStarted);
    }

    private bool TryReadInitialized(out bool isInitialized)
    {
        return TryReadRobotBool(_initialStatusVarName, out isInitialized);
    }

    private bool TryReadStationAuto(out bool isAuto)
    {
        // 有触摸屏 D 位的机械手先认 D 位。M 位读到 false 不能跳过 D 位。
        if (_autoModeDBit != null && TryReadDBit(_robotPlc, _autoModeDBit, out isAuto))
        {
            return true;
        }

        return TryReadRobotBool(_autoStartVarName, out isAuto);
    }

    private bool TryReadAlarm(out bool isAlarm)
    {
        return TryReadRobotBool(_alarmStatusVarName, out isAlarm);
    }

    private void SetRobotStatusFromPlc(ChamberStatus status)
    {
        lock (_statusLock)
        {
            if (RobotStatus == ChamberStatus.Offline || _robotStatus == status)
            {
                return;
            }

            RobotStatus = status;
        }
    }

    /// <summary>运行中显示 Run，自动模式显示 Auto。initdone 为 false 不能盖住已经 Auto/Run 的状态。Offline 由菜单保持。</summary>
    public void SyncStatusFromPlc()
    {
        if (RobotStatus == ChamberStatus.Offline)
        {
            return;
        }

        // 主界面「PLC 未连接」只看主站。7005 仍连着时也会读到 Manual/Auto，这里主站断开就显示 Disconnect。
        if (!mcComm.IsPLCConnected)
        {
            SetRobotStatusFromPlc(ChamberStatus.Disconnect);
            return;
        }

        if (TryReadAlarm(out var isAlarm) && isAlarm)
        {
            SetRobotStatusFromPlc(ChamberStatus.Alarm);
            return;
        }

        if (TryReadStartBit(out var started) && started)
        {
            SetRobotStatusFromPlc(ChamberStatus.Run);
            return;
        }

        if (TryReadStationAuto(out var isAuto) && isAuto)
        {
            SetRobotStatusFromPlc(ChamberStatus.Auto);
            return;
        }

        if (TryReadInitialized(out var isInitialized) && !isInitialized)
        {
            SetRobotStatusFromPlc(ChamberStatus.Uninitialized);
            return;
        }

        SetRobotStatusFromPlc(ChamberStatus.Manual);
    }

    public string HeadColor
    {
        get => _headColor;
        set => SetProperty(ref _headColor, value);
    }

    public ChamberStatus RobotStatus
    {
        get { return _robotStatus; }
        set
        {
            switch (value)
            {
                case ChamberStatus.Disconnect:
                case ChamberStatus.Offline:
                    HeadColor = "#7F7F7F";
                    break;
                case ChamberStatus.Uninitialized:
                case ChamberStatus.Manual:
                    HeadColor = "#6BA8D6";
                    break;
                case ChamberStatus.Auto:
                case ChamberStatus.Run:
                    HeadColor = "#2C66A7";
                    break;
                case ChamberStatus.Alarm:
                    HeadColor = "#DB3340";
                    break;
                default:
                    HeadColor = "#2C66A7";
                    break;
            }
            SetProperty(ref _robotStatus, value);
        }
    }

    public RoboticArm(string name, string initCommandValue, string autoModeCommandValue,
        string manualModeCommandValue,string initStatusValue,string autoModeStatusValue)
    {
        RobotStatus = ChamberStatus.Disconnect;
        _name = name;
        _startPermitionVarName = "PLCStartPermition";
        _readyVarName = "PLCReady";
        _resetErrorVarName = "PLCResetError";

        _initCommandValue = initCommandValue;
        _startCommandValue = DeriveBitAddress(initCommandValue, 3);
        _autoModeCommandValue = autoModeCommandValue;
        _manualModeCommandValue = manualModeCommandValue;

        _initialStatusVarName = initStatusValue;
        _autoStartVarName = autoModeStatusValue;
        _alarmStatusVarName = DeriveBitAddress(initStatusValue, 8);

        _isResetErrorNeedClear = false;
        _isAutoRunNeedClear = false;

        // 初始化晶圆追踪地址（根据机器人名称设置不同的地址）
        InitializeWaferTrackingAddresses();

    }
    private Dictionary<string, string> _handMapping = new Dictionary<string, string>();
    /// <summary>
    /// 根据机器人名称初始化晶圆追踪地址
    /// </summary>
    private void InitializeWaferTrackingAddresses()
    {
        WaferTrackingAddresses = _name switch
        {
            "EFEMRobot" => new List<string> { "W0E10", "W0E20" },
            "TransBuffer" => new List<string> { "W0AA0" },
            "TransRobot" => new List<string> { "W2610", "W2620" },
            "FixABuffer" => new List<string> { "W0ADO" },
            "FixALTP1" => new List<string> { "W1A10" },
            "FixALTP2" => new List<string> { "W1A20" },
            "FixALTP3" => new List<string> { "W1A30" },
            "FixBBuffer" => new List<string> { "W0B10" },
            "FixBLTP1" => new List<string> { "W1C10" },
            "FixBLTP2" => new List<string> { "W1C20" },
            "FixBLTP3" => new List<string> { "W1C30" },
            "CleanARobot1" => new List<string> { "W1E10", "W1E20" },
            "CleanARobot2" => new List<string> { "W2210", "W2220" },
            "CleanBRobot1" => new List<string> { "W2010", "W2020" },
            "CleanBRobot2" => new List<string> { "W2410", "W2420" },            
            _ => new List<string>() // 其他机器人默认不追踪
        };

        _handMapping.put("W0E10", "Upper");
        _handMapping.put("W0E20", "Lower");
        _handMapping.put("W2610", "Upper");
        _handMapping.put("W2620", "Lower"); 
        _handMapping.put("W1E10", "Upper");
        _handMapping.put("W1E20", "Lower");
        _handMapping.put("W2210", "Upper");
        _handMapping.put("W2220", "Lower");
        _handMapping.put("W2010", "Upper");
        _handMapping.put("W2020", "Lower");
        _handMapping.put("W2410", "Upper");
        _handMapping.put("W2420", "Lower");
    }

    /// <summary>
    /// 晶圆信息显示在本机械手上时，Wafer 应标记的 <see cref="CurrentStation"/>（与 Module 名一致）。
    /// </summary>
    private CurrentStation? MapRobotNameToCurrentStationForWaferOnArm()
    {
        return _name switch
        {
            "EFEMRobot" => CurrentStation.EFEMRobot,
            "TransBuffer" => CurrentStation.TransBuffer,
            "TransRobot" => CurrentStation.TransRobot,
            "FixABuffer" => CurrentStation.FixABuffer,
            "FixALTP1" => CurrentStation.FixALTP1,
            "FixALTP2" => CurrentStation.FixALTP2,
            "FixALTP3" => CurrentStation.FixALTP3,
            "FixBBuffer" => CurrentStation.FixBBuffer,
            "FixBLTP1" => CurrentStation.FixBLTP1,
            "FixBLTP2" => CurrentStation.FixBLTP2,
            "FixBLTP3" => CurrentStation.FixBLTP3,
            "CleanARobot1" => CurrentStation.CleanARobot1,
            "CleanARobot2" => CurrentStation.CleanARobot2,
            "CleanBRobot1" => CurrentStation.CleanBRobot1,
            "CleanBRobot2" => CurrentStation.CleanBRobot2,
            _ => null
        };
    }

    /// <summary>
    /// 追踪晶圆信息
    /// </summary>
    //public void TrackWaferInfo()
    //{


    //    try
    //    {
    //        // 遍历所有需要追踪的地址
    //        foreach (string address in WaferTrackingAddresses)
    //        {
    //            try
    //            {
    //                // 异步读取字地址的值
    //                short value =  mcComm.ReadShortVari(address);

    //                // 如果有值（非0），则处理晶圆信息
    //                if (value > 0)
    //                {
    //                    var waferInfo = DecodeWaferInfo(value);
    //                    if (!string.IsNullOrEmpty(waferInfo))
    //                    {
    //                        IsVisibleWafer = true;
    //                        WaferInfo = waferInfo;
    //                    }
    //                    break; // 找到第一个有效的晶圆信息就停止
    //                }
    //                else {
    //                    IsVisibleWafer = false;
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                Growl.WarningGlobal($"读取机器人{_name}地址{address}失败: {ex.Message}");
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Growl.WarningGlobal($"追踪机器人{_name}晶圆信息时发生错误: {ex.Message}");
    //    }


    //}
    private string _lastWaferInfo = string.Empty;
    private bool _lastIsVisibleWafer = false;
    private string _lastUpperArmWaferInfo = string.Empty;
    private bool _lastIsVisibleUpperArmWafer;
    private string _lastLowerArmWaferInfo = string.Empty;
    private bool _lastIsVisibleLowerArmWafer;
    private readonly object _trackingLock = new object();

    /// <summary>
    /// 追踪晶圆信息（双手臂各记一片，不再在第一片处中断）
    /// </summary>
    public void TrackWaferInfo()
    {
        try
        {
            string upperInfo = string.Empty;
            string lowerInfo = string.Empty;
            string singleInfo = string.Empty;
            bool upperVisible = false;
            bool lowerVisible = false;
            bool singleVisible = false;
            bool trackedUpper = false;
            bool trackedLower = false;
            bool trackedNone = false;

            foreach (string address in WaferTrackingAddresses)
            {
                try
                {
                    short value = mcComm.ReadShortVari(address);
                    var arm = _handMapping.ContainsKey(address) ? _handMapping[address] : "None";
                    var waferInfo = value > 0 ? DecodeWaferInfo(value) : string.Empty;
                    var hasWafer = !string.IsNullOrEmpty(waferInfo);

                    if (string.Equals(arm, "Upper", StringComparison.OrdinalIgnoreCase))
                    {
                        trackedUpper = true;
                        upperVisible = hasWafer;
                        upperInfo = hasWafer ? waferInfo : string.Empty;
                    }
                    else if (string.Equals(arm, "Lower", StringComparison.OrdinalIgnoreCase))
                    {
                        trackedLower = true;
                        lowerVisible = hasWafer;
                        lowerInfo = hasWafer ? waferInfo : string.Empty;
                    }
                    else
                    {
                        trackedNone = true;
                        singleVisible = hasWafer;
                        singleInfo = hasWafer ? waferInfo : string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    Growl.WarningGlobal($"读取机器人{_name}地址{address}失败: {ex.Message}");
                }
            }

            IsVisibleUpperArmWafer = upperVisible;
            UpperArmWaferInfo = upperInfo;
            IsVisibleLowerArmWafer = lowerVisible;
            LowerArmWaferInfo = lowerInfo;

            var combinedVisible = upperVisible || lowerVisible || singleVisible;
            string combinedInfo;
            if (upperVisible && lowerVisible)
            {
                combinedInfo = $"{upperInfo}/{lowerInfo}";
            }
            else if (upperVisible)
            {
                combinedInfo = upperInfo;
            }
            else if (lowerVisible)
            {
                combinedInfo = lowerInfo;
            }
            else
            {
                combinedInfo = singleInfo;
            }

            IsVisibleWafer = combinedVisible;
            WaferInfo = combinedInfo;

            if (trackedUpper)
            {
                CheckAndLogWaferStateChange(upperInfo, "Upper", upperVisible);
            }

            if (trackedLower)
            {
                CheckAndLogWaferStateChange(lowerInfo, "Lower", lowerVisible);
            }

            if (trackedNone)
            {
                CheckAndLogWaferStateChange(singleInfo, "None", singleVisible);
            }

            lock (_trackingLock)
            {
                _lastIsVisibleWafer = combinedVisible;
                _lastWaferInfo = combinedInfo;
            }
        }
        catch (Exception ex)
        {
            Growl.WarningGlobal($"追踪机器人{_name}晶圆信息时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据机器人名称获取对应的D地址
    /// </summary>
    private string GetDAddressByRobotName(string robotName)
    {
        return robotName switch
        {
            "EFEMRobot" => "D951",
            "TransBuffer" => "D952",
            "TransRobot" => "D953",
            "FixABuffer" => "D954",
            "FixALTP1" => "D955",
            "FixALTP2" => "D956",
            "FixALTP3" => "D957", 
            "FixBBuffer" => "D958",
            "FixBLTP1" => "D959",
            "FixBLTP2" => "D960", 
            "FixBLTP3" => "D961", 
            "CleanARobot1" => "D962",
            "CleanARobot2" => "D963",
            "CleanBRobot1" => "D964",
            "CleanBRobot2" => "D965",
            _ => "D951" // 默认使用D951
        };
    }
    /// <summary>
    /// 根据D地址和料盒号获取工位信息
    /// </summary>
    private string GetStationFromDAddressAndCassetteId(string dAddress)
    {
        try
        {
            // 读取D地址的值来确定工位前缀
            short dValue = 0;
            try
            {
                dValue = mcComm.ReadShortVari(dAddress);
            }
            catch
            {
                // 如果读取失败，默认返回未知
                return "未知工位";
            }

            // 根据您提供的表格信息解析工位
            switch (dAddress)
            {
                case "D951": // EFEMRobot
                    return dValue switch
                    {
                        100 => "Loadport=>EFEMRobot",
                        200 => "EFEMRobot=>TransferBuffer",
                        300 => "DryA=>EFEMRobot",
                        400 => "DryB=>EFEMRobot",
                        500 => "EFEMRobot=>Loadport",
                        _ => $"未知工位({dValue})"
                    };

                case "D952": // TransBuffer
                    return dValue switch
                    {
                        100 => "Loadport=>TransferBuffer",
                        200 => "TransferBuffer=>TransRobot",
                        _ => $"未知工位({dValue})"
                    };

                case "D953": // TransRobot
                    return dValue switch
                    {
                        100 => "TransferBuffer=>TransRobot",
                        200 => "TransRobot=>RollA1",
                        300 => "TransRobot=>FixABuffer",
                        400 => "FixABuffer=>TransRobot",
                        500 => "TransRobot=>FixBBuffer",
                        600 => "FixBBuffer=>TransRobot",
                        700 => "TransRobot=>RollA2",
                        800 => "TransRobot=>RollB1",
                        900 => "TransRobot=>RollB2",
                        _ => $"未知工位({dValue})"
                    };
                case "D954": // FixABuffer
                    return "TransRobot=>FixABuffer";

                case "D955": // FixABuffer
                    return dValue switch
                    {
                        100 => "FixABuffer=>FixALTP1",
                        200 => "FixALTP1=>PolishA",
                        300 => "FixALTP1=>PolishB",
                        _ => $"未知工位({dValue})"
                    };

                case "D956": // FixALTP1
                    return dValue switch
                    {
                        100 => "FixABuffer=>FixALTP2",
                        200 => "PolishA=>FixALTP2",
                        300 => "FixALTP2=>PolishB",
                        400 => "FixALTP2=>FixABuffer",
                        _ => $"未知工位({dValue})"
                    };

                case "D957": // FixALTP2/FixALTP3
                    return dValue switch
                    {
                        100 => "PolishA=>FixALTP3",
                        200 => "PolishB=>FixALTP3",
                        300 => "FixALTP3=>FixABuffer",
                        _ => $"未知工位({dValue})"
                    };
                case "D958": // FixBBuffer
                    return "TransRobot=>FixBBuffer";
                         
                    

                case "D959": // FixBLTP1
                    return dValue switch
                    {
                        100 => "FixBBuffer=>FixBLTP1",
                        200 => "FixBLTP1=>PolishC",
                        300 => "FixBLTP1=>PolishD",
                        _ => $"未知工位({dValue})"
                    };

                case "D960": // FixBLTP2
                    return dValue switch
                    {
                        100 => "FixBBuffer=>FixBLTP2",
                        200 => "PolishC=>FixBLTP2",
                        300 => "FixBLTP2=>PolishD",
                        400 => "FixBLTP2=>FixBBuffer",
                        _ => $"未知工位({dValue})"
                    };

                case "D961": //FixBLTP3
                    return dValue switch
                    {
                        100 => "PolishC=>FixBLTP3",
                        200 => "PolishD=>FixBLTP3",
                        300 => "FixBLTP3=>FixBBuffer",
                        _ => $"未知工位({dValue})"
                    };

                case "D962": // CleanARobot1
                    return dValue switch
                    {
                        100 => "RollA1=>CleanARobot1",
                        200 => "RollA2=>CleanARobot1",
                        300 => "CleanARobot1=>RollA3",
                        400 => "RollA3=>CleanARobot1",
                        500 => "CleanARobot1=>PenA1",
                        600 => "CleanARobot1=>RollA2",
                        _ => $"未知工位({dValue})"
                    };

                case "D963": //CleanARobot2
                    return dValue switch
                    {
                        100 => "RollA3=>CleanARobot2",
                        200 => "CleanARobot2=>PenA2",
                        300 => "PenA1=>CleanARobot2",
                        400 => "PenA2=>CleanARobot2",
                        500 => "CleanARobot2=>DryA",
                        600 => "CleanARobot2=>PenA1",
                        _ => $"未知工位({dValue})"
                    };
                case "D964": // CleanBRobot1
                    return dValue switch
                    {
                        100 => "RollB1=>CleanBRobot1",
                        200 => "RollB2=>CleanBRobot1",
                        300 => "CleanBRobot1=>RollB3",
                        400 => "RollB3=>CleanBRobot1",
                        500 => "CleanBRobot1=>PenB1",
                        600 => "CleanBRobot1=>RollB2",
                        _ => $"未知工位({dValue})"
                    };

                case "D965": //CleanBRobot2
                    return dValue switch
                    {
                        100 => "RollB3=>CleanBRobot2",
                        200 => "CleanBRobot2=>PenB2",
                        300 => "PenB1=>CleanBRobot2",
                        400 => "PenB2=>CleanBRobot2",
                        500 => "CleanBRobot2=>DryB",
                        600 => "CleanBRobot2=>PenB1",
                        _ => $"未知工位({dValue})"
                    };
                default:
                    return $"Unknow_{dAddress}";
            }
        }
        catch (Exception ex)
        {
           // Growl.WarningGlobal($"获取工位信息失败: {ex.Message}");
            return "未知工位";
        }
    }
    /// <summary>
    /// 检查并记录晶圆状态变化
    /// </summary>
    /// <param name="currentWaferInfo">当前晶圆信息</param>
    /// <param name="currentIsVisible">当前是否可见</param>
    private void CheckAndLogWaferStateChange(string currentWaferInfo, string arm, bool currentIsVisible)
    {
        bool prevState;
        string prevWaferInfo;

        lock (_trackingLock)
        {
            GetLastArmState(arm, out prevState, out prevWaferInfo);
            SetLastArmState(arm, currentIsVisible, currentWaferInfo ?? string.Empty);
        }

        string timestamp = DateTime.Now.ToString("HH:mm:ss.f");
        // 根据机器人名称确定要读取的D地址
        string dAddress = GetDAddressByRobotName(_name);

        // 读取对应的D地址值来确定工位
        string station = GetStationFromDAddressAndCassetteId(dAddress);
        try
        {
            // 检查状态变化
            if (!prevState && currentIsVisible)
            {
                // 晶圆被抓取 - START事件
                string logEntry = $"{timestamp},{currentWaferInfo},{_name} Action START,{arm},Pick,{station}";
                WriteToCsvFile(logEntry);
            }
            else if (prevState && !currentIsVisible)
            {
                // 晶圆被释放 - END事件
                string logEntry = $"{timestamp},{prevWaferInfo},{_name} Action END ,{arm},Place,{station}";
                WriteToCsvFile(logEntry);
            }
            else if (prevState && currentIsVisible && prevWaferInfo != currentWaferInfo)
            {
                // 晶圆更换 - 先记录END事件，再记录START事件
                string endLogEntry = $"{timestamp},{prevWaferInfo},{_name} Action END,{arm},Place,{station}";
                string startLogEntry = $"{timestamp},{currentWaferInfo},{_name} Action START,{arm},Pick,{station}";
                WriteToCsvFile(endLogEntry);
                WriteToCsvFile(startLogEntry);
            }
        }
        catch (Exception ex)
        {
            Growl.WarningGlobal($"记录机器人{_name}晶圆状态变化时发生错误: {ex.Message}");
        }
    }

    private void GetLastArmState(string arm, out bool visible, out string waferInfo)
    {
        switch (arm)
        {
            case "Upper":
                visible = _lastIsVisibleUpperArmWafer;
                waferInfo = _lastUpperArmWaferInfo;
                break;
            case "Lower":
                visible = _lastIsVisibleLowerArmWafer;
                waferInfo = _lastLowerArmWaferInfo;
                break;
            default:
                visible = _lastIsVisibleWafer;
                waferInfo = _lastWaferInfo;
                break;
        }
    }

    private void SetLastArmState(string arm, bool visible, string waferInfo)
    {
        switch (arm)
        {
            case "Upper":
                _lastIsVisibleUpperArmWafer = visible;
                _lastUpperArmWaferInfo = waferInfo;
                break;
            case "Lower":
                _lastIsVisibleLowerArmWafer = visible;
                _lastLowerArmWaferInfo = waferInfo;
                break;
            default:
                _lastIsVisibleWafer = visible;
                _lastWaferInfo = waferInfo;
                break;
        }
    }

    /// <summary>
    /// 写入CSV文件
    /// </summary>
    /// <param name="date">日期，用于生成文件名</param>
    /// <param name="logEntry">日志条目</param>
    private void WriteToCsvFile(string logEntry)
    {
        try
        {
            // 获取当前时间用于创建目录结构
            DateTime createTime = DateTime.Now;

            // 构建日志目录路径 D:\CMP\Log\year\month\day
            string logDirectory = Path.Combine(@"D:\CMP\Log",
                createTime.Year.ToString(),
                createTime.ToString("yyyy-MM"),
                createTime.ToString("yyyy-MM-dd"), "WaferTracking");

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // 生成文件名（每天一个文件）
            string fileName = $"WaferTracking_{createTime.ToString("yyyy-MM-dd")}.csv";
            string filePath = Path.Combine(logDirectory, fileName);

            // 如果文件不存在，先写入标题行
            bool fileExists = File.Exists(filePath);
            if (!fileExists)
            {
                string header = "Time,WaferInfo,Event,Arm,Action,Station";
                File.WriteAllText(filePath, header + Environment.NewLine, Encoding.UTF8);
            }

            // 追加日志条目
            File.AppendAllText(filePath, logEntry + Environment.NewLine, Encoding.UTF8);

            // 同时输出到原有日志系统
            Logger.Info(logEntry.Replace(",", "\t"), "wafer_tracking.log", 0);
        }
        catch (Exception ex)
        {
            // 如果写入文件失败，至少记录到原有日志系统
            Logger.Error($"写入CSV文件失败: {ex.Message}", ex, "wafer_tracking.log", 0);
        }
    }

    /// <summary>
    /// 解码晶圆信息（根据实际PLC数据格式实现）
    /// </summary>
    private string DecodeWaferInfo(short value)
    {
        try
        {
            // 假设高8位表示料盒号，低8位表示槽位号
            byte  slotByte = (byte)(value >> 8);
            byte cassetteByte = (byte)(value & 0xFF);

            if (cassetteByte >= 0 && slotByte >= 0)
            {
                string waferName = $"C{cassetteByte}W{slotByte}";

                Wafer wafer = null;
                var loadPort = LoadPortManager.Instance.GetLoadPort(cassetteByte - 1);
                if (loadPort?.WaferList != null)
                {
                    wafer = loadPort.WaferList.FirstOrDefault(w => w.WaferName == waferName);
                    if (wafer == null && cassetteByte > 0 && slotByte > 0)
                    {
                        int lp = cassetteByte - 1;
                        wafer = loadPort.WaferList.FirstOrDefault(w => w.LoadportID == lp && w.SlotID == slotByte - 1)
                                ?? loadPort.WaferList.FirstOrDefault(w => w.LoadportID == lp && w.SlotID == slotByte);
                    }
                }

                if (wafer == null && cassetteByte > 0 && slotByte > 0)
                {
                    int trackingIndex = (cassetteByte - 1) * 25 + slotByte;
                    lock (GlobalService.dicWaferIndex)
                    {
                        GlobalService.dicWaferIndex.TryGetValue(trackingIndex, out wafer);
                    }
                }

                var stationOnArm = MapRobotNameToCurrentStationForWaferOnArm();
                if (wafer != null && stationOnArm.HasValue)
                    wafer.CurrentStation = stationOnArm.Value;

                return waferName;

            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            Growl.WarningGlobal($"解码机器人{_name}晶圆信息失败: {ex.Message}");
            return null;
        }
    }

    public bool IsInitialized()
    {
        return TryReadInitialized(out var isInitialized) && isInitialized;
    }

    public bool IsAutoMode()
    {
        SyncStatusFromPlc();
        return RobotStatus is ChamberStatus.Auto or ChamberStatus.Run;
    }

    public bool IsConnected()
    {
        if (RobotStatus == ChamberStatus.Offline)
        {
            return false;
        }

        return mcComm.IsPLCConnected || _robotPlc.IsPLCConnected;
    }

    public void SwitchToOffline()
    {
        RobotStatus = ChamberStatus.Offline;
    }

    public void SwitchToOnline()
    {
        RobotStatus = ChamberStatus.Uninitialized;
    }

    public void InitializeCommand()
    {
        if (RobotStatus == ChamberStatus.Offline || RobotStatus == ChamberStatus.Disconnect)
        {
            return;
        }

        if (!IsInitialized())
        {
           var result= mcComm.WriteBoolVari(_initCommandValue,true);
            if (result.IsSuccess)
            {
                // 成功发送初始化命令
                Growl.InfoGlobal("成功发送初始化命令");
            }
            else
            {
                Growl.ErrorGlobal($"{_name} InitCommandExecuteError: {_err} ");
            }
        }
    }

    public void SwitchToAutoModeCommand()
    {
        if (RobotStatus == ChamberStatus.Offline || RobotStatus == ChamberStatus.Disconnect)
        {
            return;
        }

        if (!SwitchModeAfterStop(_autoModeCommandValue, _manualModeCommandValue))
        {
            Growl.ErrorGlobal($"{_name} AutoModeCommandExecuteError");
            return;
        }

        Growl.InfoGlobal("成功发送自动模式命令");
        SyncStatusFromPlc();
    }

    public void SwitchToManualModeCommand()
    {
        if (RobotStatus == ChamberStatus.Offline || RobotStatus == ChamberStatus.Disconnect)
        {
            return;
        }

        if (!SwitchModeAfterStop(_manualModeCommandValue, _autoModeCommandValue))
        {
            Growl.ErrorGlobal($"{_name} ManualModeCommandExecuteError");
            return;
        }

        Growl.InfoGlobal("成功发送手动模式命令");
        SyncStatusFromPlc();
    }

    private bool SwitchModeAfterStop(string modeAddress, string oppositeAddress)
    {
        lock (_modeSwitchLock)
        {
            var stopAddress = DeriveBitAddress(_initCommandValue, 5);
            if (!PulseBit(mcComm, stopAddress))
            {
                return false;
            }

            Thread.Sleep(100);
            if (!string.IsNullOrWhiteSpace(oppositeAddress))
            {
                mcComm.WriteBoolVari(oppositeAddress, false);
            }

            return PulseBit(mcComm, modeAddress);
        }
    }

    private static bool PulseBit(MitsubishiPlcService plc, string address)
    {
        if (plc == null || string.IsNullOrWhiteSpace(address))
        {
            return false;
        }

        var on = plc.WriteBoolVari(address, true);
        if (!on.IsSuccess)
        {
            return false;
        }

        Thread.Sleep(200);
        plc.WriteBoolVari(address, false);
        return true;
    }

    public void AbortCommand()
    {
        //_err = McProtocol.MCWriteWordDevice(_mcLink.sock, _commandDevice, _abortCommandValue);
        //if (_err == 0)
        //{
        //    // 成功发送终止命令
        //}
        //else
        //{
        //    Growl.ErrorGlobal($"{_name} AbortCommandExecuteError: {_err} ");
        //}
    }

    public void ResetError()
    {
        if (RobotStatus == ChamberStatus.Offline || RobotStatus == ChamberStatus.Disconnect)
        {
            return;
        }

        var result = mcComm.WriteBoolVari("B09", true);
        if (result.IsSuccess)
        {
            _isResetErrorNeedClear = true;
            RobotStatus = ChamberStatus.Uninitialized;
            
            GlobalService.plcComm.WriteBoolVari("B09", false);
        }
        else
        {
            Growl.ErrorGlobal($"{_name} ResetErrorCommandExecuteError: {_err} ");
        }
    }

    public void ResetErrorClear()
    {
        if (RobotStatus == ChamberStatus.Offline || RobotStatus == ChamberStatus.Disconnect)
        {
            return;
        }

        var result = mcComm.WriteBoolVari(_resetErrorVarName, true);
        if (result.IsSuccess)
        {
            
            _isResetErrorNeedClear = false;
        }
        else
        {
            Growl.ErrorGlobal($"{_name} ResetErrorClearCommandExecuteError: {_err} ");
        }
    }

    public void PlcStartAndReady()
    {
        if (RobotStatus == ChamberStatus.Offline || RobotStatus == ChamberStatus.Disconnect)
        {
            return;
        }

        //_err = McProtocol.MCWriteBitDevice(_mcLink.sock, _startPermitionVarName, true);
        //_err = McProtocol.MCWriteBitDevice(_mcLink.sock, _readyVarName, true);
    }

    public void PlcAutoStart()
    {
        if (RobotStatus != ChamberStatus.Auto)
        {
            return;
        }

        //_err = McProtocol.MCWriteBitDevice(_mcLink.sock, _autoStartVarName, true);
        if (_err == 0)
        {
            _isAutoRunNeedClear = true;
        }
        else
        {
            Growl.ErrorGlobal($"{_name} AutoStartCommandExecuteError: {_err} ");
        }
    }

    public void ResetAutoStart()
    {
        if (RobotStatus != ChamberStatus.Auto)
        {
            return;
        }

        //_err = McProtocol.MCWriteBitDevice(_mcLink.sock, _autoStartVarName, false);
        if (_err == 0)
        {
            _isAutoRunNeedClear = false;
        }
        else
        {
            Growl.ErrorGlobal($"{_name} AutoStartCommandExecuteError: {_err} ");
        }
    }
}

[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
public class GlobalRobots
{
    // 15个机器人对象
    public RoboticArm EFEMRobot { get; set; }
    public RoboticArm CleanARobot1 { get; set; }
    public RoboticArm CleanARobot2 { get; set; }
    public RoboticArm CleanBRobot1 { get; set; }
    public RoboticArm CleanBRobot2 { get; set; }
    public RoboticArm TransRobot { get; set; }
    public RoboticArm TransBuffer { get; set; }
    public RoboticArm FixABuffer { get; set; }
    public RoboticArm FixALTP1 { get; set; }
    public RoboticArm FixALTP2 { get; set; }
    public RoboticArm FixALTP3 { get; set; }
    public RoboticArm FixBBuffer { get; set; }
    public RoboticArm FixBLTP1 { get; set; }
    public RoboticArm FixBLTP2 { get; set; }
    public RoboticArm FixBLTP3 { get; set; }

    public List<RoboticArm> listRobot { get; set; }

    Thread thRobotMonitor;
    private volatile bool _isRobotMonitorRunning;

    private Thread _waferTrackingThread;
    private volatile bool _isWaferTrackingRunning = false;
    private const int TrackingInterval = 1000; // 1秒间隔


    // 设备地址定义
    public static class RobotDeviceAddresses
    {
        public static string EFEMRobotInitialize = "M1832";
        public static string EFEMRobotAutoMode = "M1840";
        public static string EFEMRobotManualMode = "M1841";
        public static string EFEMRobotInitializeStatus = "M6153";
        public static string EFEMRobotAutoModeStatus = "M6167";

        public static string TransBufferInitialize = "M1880";
        public static string TransBufferInitializeStatus = "M6201";
        public static string TransBufferAutoModeStatus = "M6215";
        public static string TransBufferAutoMode = "M1888";
        public static string TransBufferManualMode = "M1889";

        public static string TransRobotInitialize = "M1928";
        public static string TransRobotInitializeStatus = "M6249";
        public static string TransRobotAutoModeStatus = "M6263";
        public static string TransRobotAutoMode = "M1936";
        public static string TransRobotManualMode = "M1937";

        public static string FixABufferInitialize = "M1976";
        public static string FixABufferInitializeStatus = "M6297";
        public static string FixABufferAutoModeStatus = "M6311";
        public static string FixABufferAutoMode = "M1984";
        public static string FixABufferManualMode = "M1985";

        public static string FixALTP1Initialize = "M2024";
        public static string FixALTP1InitializeStatus = "M6345";
        public static string FixALTP1AutoModeStatus = "M6359";
        public static string FixALTP1AutoMode = "M2032";
        public static string FixALTP1ManualMode = "M2033";

        public static string FixALTP2Initialize = "M2072";
        public static string FixALTP2InitializeStatus = "M6393";
        public static string FixALTP2AutoModeStatus = "M6407";
        public static string FixALTP2AutoMode = "M2080";
        public static string FixALTP2ManualMode = "M2081";

        public static string FixALTP3Initialize = "M2120";
        public static string FixALTP3InitializeStatus = "M6441";
        public static string FixALTP3AutoModeStatus = "M6455";
        public static string FixALTP3AutoMode = "M2128";
        public static string FixALTP3ManualMode = "M2129";

        public static string FixBBufferInitialize = "M2168";
        public static string FixBBufferInitializeStatus = "M6489";
        public static string FixBBufferAutoModeStatus = "M6503";
        public static string FixBBufferManualMode = "M2176";
        public static string FixBBufferAutoMode = "M2177";

        public static string FixBLTP1Initialize = "M2216";
        public static string FixBLTP1InitializeStatus = "M6537";
        public static string FixBLTP1AutoModeStatus = "M6551";
        public static string FixBLTP1AutoMode = "M2224";
        public static string FixBLTP1ManualMode = "M2225";

        public static string FixBLTP2Initialize = "M2264";
        public static string FixBLTP2InitializeStatus = "M6585";
        public static string FixBLTP2AutoModeStatus = "M6599";
        public static string FixBLTP2AutoMode = "M2272";
        public static string FixBLTP2ManualMode = "M2273";

        public static string FixBLTP3Initialize = "M2312";
        public static string FixBLTP3InitializeStatus = "M6633";
        public static string FixBLTP3AutoModeStatus = "M6647";
        public static string FixBLTP3AutoMode = "M2320";
        public static string FixBLTP3ManualMode = "M2321";

        public static string CleanARobot1Initialize = "M2360";
        public static string CleanARobot1AutoMode = "M2368";
        public static string CleanARobot1ManualMode = "M2369";
        public static string CleanARobot1InitializeStatus = "M6681";
        public static string CleanARobot1AutoModeStatus = "M6695";

        public static string CleanARobot2Initialize = "M2408";
        public static string CleanARobot2InitializeStatus = "M6729";
        public static string CleanARobot2AutoModeStatus = "M6743";
        public static string CleanARobot2AutoMode = "M2416";
        public static string CleanARobot2ManualMode = "M2417";

        public static string CleanBRobot1Initialize = "M2456";
        public static string CleanBRobot1InitializeStatus = "M6777";
        public static string CleanBRobot1AutoModeStatus = "M6791";
        public static string CleanBRobot1AutoMode = "M2464";
        public static string CleanBRobot1ManualMode = "M2465";

        public static string CleanBRobot2Initialize = "M2504";
        public static string CleanBRobot2InitializeStatus = "M6825";
        public static string CleanBRobot2AutoModeStatus = "M6839";
        public static string CleanBRobot2AutoMode = "M2512";
        public static string CleanBRobot2ManualMode = "M2513";

       
       

       



    }

    public GlobalRobots()
    {
        // 初始化15个机器人对象
        EFEMRobot = new RoboticArm("EFEMRobot",RobotDeviceAddresses.EFEMRobotInitialize, RobotDeviceAddresses.EFEMRobotAutoMode,RobotDeviceAddresses.EFEMRobotManualMode, RobotDeviceAddresses.EFEMRobotInitializeStatus,RobotDeviceAddresses.EFEMRobotAutoModeStatus);

        TransBuffer = new RoboticArm("TransBuffer", RobotDeviceAddresses.TransBufferInitialize, RobotDeviceAddresses.TransBufferAutoMode, RobotDeviceAddresses.TransBufferManualMode, RobotDeviceAddresses.TransBufferInitializeStatus,RobotDeviceAddresses.TransBufferAutoModeStatus);

        TransRobot = new RoboticArm("TransRobot",RobotDeviceAddresses.TransRobotInitialize, RobotDeviceAddresses.TransRobotAutoMode,RobotDeviceAddresses.TransRobotManualMode, RobotDeviceAddresses.TransRobotInitializeStatus,RobotDeviceAddresses.TransRobotAutoModeStatus);

        FixABuffer = new RoboticArm("FixABuffer",RobotDeviceAddresses.FixABufferInitialize, RobotDeviceAddresses.FixABufferAutoMode,RobotDeviceAddresses.FixABufferManualMode, RobotDeviceAddresses.FixABufferInitializeStatus,RobotDeviceAddresses.FixABufferAutoModeStatus);

        FixALTP1 = new RoboticArm("FixALTP1", RobotDeviceAddresses.FixALTP1Initialize, RobotDeviceAddresses.FixALTP1AutoMode, RobotDeviceAddresses.FixALTP1ManualMode, RobotDeviceAddresses.FixALTP1InitializeStatus,RobotDeviceAddresses.FixALTP1AutoModeStatus);
        
        FixALTP2 = new RoboticArm("FixALTP2", RobotDeviceAddresses.FixALTP2Initialize, RobotDeviceAddresses.FixALTP2AutoMode, RobotDeviceAddresses.FixALTP2ManualMode, RobotDeviceAddresses.FixALTP2InitializeStatus,RobotDeviceAddresses.FixALTP2AutoModeStatus);
        
        FixALTP3 = new RoboticArm("FixALTP3", RobotDeviceAddresses.FixALTP3Initialize, RobotDeviceAddresses.FixALTP3AutoMode, RobotDeviceAddresses.FixALTP3ManualMode, RobotDeviceAddresses.FixALTP3InitializeStatus,RobotDeviceAddresses.FixALTP3AutoModeStatus); 
        
        FixBBuffer = new RoboticArm("FixBBuffer", RobotDeviceAddresses.FixBBufferInitialize, RobotDeviceAddresses.FixBBufferAutoMode, RobotDeviceAddresses.FixBBufferManualMode, RobotDeviceAddresses.FixBBufferInitializeStatus,RobotDeviceAddresses.FixBBufferAutoModeStatus);

        FixBLTP1 = new RoboticArm("FixBLTP1", RobotDeviceAddresses.FixBLTP1Initialize, RobotDeviceAddresses.FixBLTP1AutoMode, RobotDeviceAddresses.FixBLTP1ManualMode, RobotDeviceAddresses.FixBLTP1InitializeStatus,RobotDeviceAddresses.FixBLTP1AutoModeStatus);
        
        FixBLTP2 = new RoboticArm("FixBLTP2", RobotDeviceAddresses.FixBLTP2Initialize, RobotDeviceAddresses.FixBLTP2AutoMode, RobotDeviceAddresses.FixBLTP2ManualMode, RobotDeviceAddresses.FixBLTP2InitializeStatus,RobotDeviceAddresses.FixBLTP2AutoModeStatus);
        
        FixBLTP3 = new RoboticArm("FixBLTP3", RobotDeviceAddresses.FixBLTP3Initialize, RobotDeviceAddresses.FixBLTP3AutoMode, RobotDeviceAddresses.FixBLTP3ManualMode,RobotDeviceAddresses.FixBLTP3InitializeStatus,RobotDeviceAddresses.FixBLTP3AutoModeStatus);

        CleanARobot1 = new RoboticArm("CleanARobot1", RobotDeviceAddresses.CleanARobot1Initialize, RobotDeviceAddresses.CleanARobot1AutoMode, RobotDeviceAddresses.CleanARobot1ManualMode, RobotDeviceAddresses.CleanARobot1InitializeStatus,RobotDeviceAddresses.CleanARobot1AutoModeStatus);

        CleanARobot2 = new RoboticArm("CleanARobot2", RobotDeviceAddresses.CleanARobot2Initialize, RobotDeviceAddresses.CleanARobot2AutoMode, RobotDeviceAddresses.CleanARobot2ManualMode, RobotDeviceAddresses.CleanARobot2InitializeStatus,RobotDeviceAddresses.CleanARobot2AutoModeStatus);

        CleanBRobot1 = new RoboticArm("CleanBRobot1", RobotDeviceAddresses.CleanBRobot1Initialize, RobotDeviceAddresses.CleanBRobot1AutoMode, RobotDeviceAddresses.CleanBRobot1ManualMode, RobotDeviceAddresses.CleanBRobot1InitializeStatus,RobotDeviceAddresses.CleanBRobot1AutoModeStatus);

        CleanBRobot2 = new RoboticArm("CleanBRobot2", RobotDeviceAddresses.CleanBRobot2Initialize, RobotDeviceAddresses.CleanBRobot2AutoMode, RobotDeviceAddresses.CleanBRobot2ManualMode, RobotDeviceAddresses.CleanBRobot2InitializeStatus,RobotDeviceAddresses.CleanBRobot2AutoModeStatus);


        listRobot = new List<RoboticArm>
        {
            EFEMRobot,
            TransBuffer,
            TransRobot,
            FixABuffer,
            FixALTP1,
            FixALTP2,
            FixALTP3,
            FixBBuffer,
            FixBLTP1,
            FixBLTP2,
            FixBLTP3,
            CleanARobot1,
            CleanARobot2,
            CleanBRobot1,
            CleanBRobot2
        };

        BindHmiAutoModeBits();
        LinkStCtrlFromDataCenter();

        _isRobotMonitorRunning = true;
        thRobotMonitor = new Thread(RobotsMonitor)
        {
            IsBackground = true,
            Name = "RobotStatusMonitor"
        };
        thRobotMonitor.Start();

        // 启动晶圆追踪定时器
        StartWaferTracking();
    }

    /// <summary>绑定与触摸屏一致的自动模式位：有 D 位的读 Dxxxx.5，其余回退到控制页的 AutoMode M 位。</summary>
    private void BindHmiAutoModeBits()
    {
        MitsubishiPlcService.EnsureRobotCtrlConnected();

        EFEMRobot.AttachAutoModeDBit(FeedingRobotAddress.AutoModeStatus);
        TransRobot.AttachAutoModeDBit(TransferRobotAddress.AutoModeStatus);
        CleanARobot1.AttachAutoModeDBit(CleanRobotA1Address.AutoModeStatus);
        CleanARobot2.AttachAutoModeDBit(CleanRobotA2Address.AutoModeStatus);
        CleanBRobot1.AttachAutoModeDBit(CleanRobotB1Address.AutoModeStatus);
        CleanBRobot2.AttachAutoModeDBit(CleanRobotB2Address.AutoModeStatus);
    }

    /// <summary>将各机械手与 DataCenter 中 SV_STCtrl.csv 加载的 <see cref="STCtrl"/>（Module 与机械手名一致）关联。</summary>
    private void LinkStCtrlFromDataCenter()
    {
        var modules = GlobalService.DataCenter?.listSTCtrl;
        if (modules == null) return;
        foreach (var robot in listRobot)
        {
            var st = modules.FirstOrDefault(m =>
                string.Equals(m.ModuleName, robot.RobotName, StringComparison.Ordinal));
            robot.AttachStCtrl(st);
        }
    }

    /// <summary>
    /// 启动晶圆追踪
    /// </summary>
    private void StartWaferTracking()
    {
        if (_isWaferTrackingRunning)
            return;

        _isWaferTrackingRunning = true;
        _waferTrackingThread = new Thread(WaferTrackingLoop)
        {
            IsBackground = true,
            Name = "WaferTrackingThread"
        };
        _waferTrackingThread.Start();
    }
    /// <summary>
    /// 晶圆追踪主循环
    /// </summary>
    private void WaferTrackingLoop()
    {
        // 初始延迟
        Thread.Sleep(2000);

        while (_isWaferTrackingRunning)
        {
            try
            {
                TrackAllRobotsWaferInfo();

                // 等待下一周期
                Thread.Sleep(TrackingInterval);
            }
            catch (ThreadInterruptedException)
            {
                // 线程被中断，正常退出
                break;
            }
            catch (Exception ex)
            {
                Growl.WarningGlobal($"晶圆追踪循环发生错误: {ex.Message}");
                // 出错后继续尝试
                Thread.Sleep(TrackingInterval);
            }
        }
    }
    /// <summary>
    /// 追踪所有机器人的晶圆信息
    /// </summary>
    private void TrackAllRobotsWaferInfo()
    {
        try
        {
            // 使用并行处理所有机器人的晶圆信息追踪
            Parallel.ForEach(listRobot, robot => {
                try
                {
                    // 在线程池中同步执行追踪
                    robot.TrackWaferInfo();
                }
                catch (Exception ex)
                {
                    // 记录单个机器人的错误，但不影响其他机器人
                    Growl.WarningGlobal($"追踪机器人{robot.GetType().Name}晶圆信息时发生错误: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            Growl.WarningGlobal($"追踪所有机器人晶圆信息时发生错误: {ex.Message}");
        }
    }

    public void Stop()
    {
        _isRobotMonitorRunning = false;
        _isWaferTrackingRunning = false;
    }

    private void RobotsMonitor()
    {
        Thread.Sleep(2000);
        const int delay = 500;
        while (_isRobotMonitorRunning)
        {
            try
            {
                MitsubishiPlcService.EnsureConnected();
                foreach (var robot in listRobot)
                {
                    robot.SyncStatusFromPlc();
                }
            }
            catch
            {
                // 读失败保持上次状态，避免主界面闪烁
            }

            Thread.Sleep(delay);
        }
    }
    #region
    #endregion

}