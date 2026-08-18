//using CMP.Models.Operation.Brush;
using CMP.Services.Auth;
using CMP.Models.Resource;
using CMP.Models.Operation.LoadPort;
using CMP.Services.LoadPort;
using HandyControl.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CMP.Models.Warn;
using Microsoft.Identity.Client;
using static CMP.Models.Resource.RoboticArm;
using System.Windows;
using CMP.Extensions;
using CMP.Models.Recipe.Sequence;
using CMP.Views.Common.Dialog;
using System.Linq;
using CMP.ViewModels.Common.Dialog;
using CMP.Services.Log;
using CMP;
using Logger = CMP.Services.Log.Logger;
using System.Threading.Tasks;

namespace CMP.ViewModels.Tabs.OperationTab
{
   
    public class MainContentViewModel : BindableBase
    {
        Chamber _polishA;
        Chamber _polishB;
        Chamber _polishC;
        Chamber _polishD;
        Chamber _rollerA1;
        Chamber _rollerA2;
        Chamber _rollerA3;
        Chamber _rollerB1;
        Chamber _rollerB2;
        Chamber _rollerB3;
        Chamber _penA1;
        Chamber _penA2;
        Chamber _penB1;
        Chamber _penB2;
        Chamber _dryA;
        Chamber _dryB;

        public Chamber PolishA
        {
            get => _polishA;
            set => SetProperty(ref _polishA, value);
        }
        public Chamber PolishB
        {
            get => _polishB;
            set => SetProperty(ref _polishB, value);
        }
        public Chamber PolishC
        {
            get => _polishC;
            set => SetProperty(ref _polishC, value);
        }
        public Chamber PolishD
        {
            get => _polishD;
            set => SetProperty(ref _polishD, value);
        }
        public Chamber RollerA1
        {
            get => _rollerA1;
            set => SetProperty(ref _rollerA1, value);
        }
        public Chamber RollerA2
        {
            get => _rollerA2;
            set => SetProperty(ref _rollerA2, value);
        }
        public Chamber RollerA3
        {
            get => _rollerA3;
            set => SetProperty(ref _rollerA3, value);
        }
        public Chamber RollerB1
        {
            get => _rollerB1;
            set => SetProperty(ref _rollerB1, value);
        }
        public Chamber RollerB2
        {
            get => _rollerB2;
            set => SetProperty(ref _rollerB2, value);
        }
        public Chamber RollerB3
        {
            get => _rollerB3;
            set => SetProperty(ref _rollerB3, value);
        }
        public Chamber PenA1
        {
            get => _penA1;
            set => SetProperty(ref _penA1, value);
        }
        public Chamber PenA2
        {
            get => _penA2;
            set => SetProperty(ref _penA2, value);
        }
        public Chamber PenB1
        {
            get => _penB1;
            set => SetProperty(ref _penB1, value);
        }
        public Chamber PenB2
        {
            get => _penB2;
            set => SetProperty(ref _penB2, value);
        }
        public Chamber DryA
        {
            get => _dryA;
            set => SetProperty(ref _dryA, value);
        }
        public Chamber DryB
        {
            get => _dryB;
            set => SetProperty(ref _dryB, value);
        }
        private RoboticArm _eFEMRobot;
        private RoboticArm _transBuffer;
        private RoboticArm _transRobot;
        private RoboticArm _fixABuffer;
        private RoboticArm _fixALTP1;
        private RoboticArm _fixALTP2;
        private RoboticArm _fixALTP3;
        private RoboticArm _fixBBuffer;
        private RoboticArm _fixBLTP1;
        private RoboticArm _fixBLTP2;
        private RoboticArm _fixBLTP3;
        private RoboticArm _cleanARobot1;
        private RoboticArm _cleanARobot2;
        private RoboticArm _cleanBRobot1;
        private RoboticArm _cleanBRobot2;

        public RoboticArm EFEMRobot
        {
            get => _eFEMRobot;
            set => SetProperty(ref _eFEMRobot, value);
        }

        public RoboticArm TransBuffer{
            get => _transBuffer;
            set => SetProperty(ref _transBuffer, value);
        }

        public RoboticArm TransRobot
        {
            get => _transRobot;
            set => SetProperty(ref _transRobot, value);
        }
        public RoboticArm FixABuffer
        {
            get => _fixABuffer;
            set => SetProperty(ref _fixABuffer,value);
        }
        public RoboticArm FixALTP1
        {
            get => _fixALTP1;
            set => SetProperty(ref _fixALTP1, value);
        }
        public RoboticArm FixALTP2
        {
            get => _fixALTP2;
            set => SetProperty(ref _fixALTP2, value);
        }
        public RoboticArm FixALTP3
        {
            get => _fixALTP3;
            set => SetProperty(ref _fixALTP3, value);
        }
        public RoboticArm FixBBuffer
        {
            get => _fixBBuffer;
            set => SetProperty(ref _fixBBuffer, value);
        }
        public RoboticArm FixBLTP1
        {
            get => _fixBLTP1;
            set => SetProperty(ref _fixBLTP1, value);
        }
        public RoboticArm FixBLTP2
        {
            get => _fixBLTP2;
            set => SetProperty(ref _fixBLTP2, value);
        }
        public RoboticArm FixBLTP3
        {
            get => _fixBLTP3;
            set => SetProperty(ref _fixBLTP3, value);
        }
        public RoboticArm CleanARobot1
        {
            get => _cleanARobot1;
            set => SetProperty(ref _cleanARobot1,value);
        }
        public RoboticArm CleanARobot2
        {
            get => _cleanARobot2;
            set => SetProperty(ref _cleanARobot2, value);
        }
        public RoboticArm CleanBRobot1
        {
            get => _cleanBRobot1;
            set => SetProperty(ref _cleanBRobot1, value);
        }
        public RoboticArm CleanBRobot2
        {
            get => _cleanBRobot2;
            set => SetProperty(ref _cleanBRobot2, value);
        }
        private ILoadPortService _loadPortService;
        private SquenceWithPLC _sequenceWithPLC;

        private ObservableCollection<LoadPortData> _allLoadPorts = new ObservableCollection<LoadPortData>();
        public ObservableCollection<LoadPortData> AllLoadPorts
        {
            get { return _allLoadPorts; }
            set { SetProperty(ref _allLoadPorts, value); }
        }
        private async Task InitializeLoadPortsAsync()
        {
            try
            {
                var loadPortDict = LoadPortManager.Instance.GetAllLoadPorts();
                if (loadPortDict == null || loadPortDict.Count == 0)
                {
                    return;
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    AllLoadPorts.Clear();
                    foreach (var loadPort in loadPortDict.Values)
                    {
                        AllLoadPorts.Add(loadPort);
                    }

                    RaisePropertyChanged(nameof(AllLoadPorts));
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"LoadPorts refresh failed: {ex.Message}", ex);
            }
        }

        public MainPageViewModel MainPage { get; }

        public MainContentViewModel(
            ILoadPortService loadPortService,
            IEventAggregator eventAggregator,
            MainPageViewModel mainPage)
        {
            _loadPortService = loadPortService;
            _sequenceWithPLC = new SquenceWithPLC();
            AllLoadPorts = new ObservableCollection<LoadPortData>();
            MainPage = mainPage;

            eventAggregator.GetEvent<LoadPortsReadyEvent>()
                .Subscribe(() => _ = InitializeLoadPortsAsync(), ThreadOption.UIThread);
            _ = InitializeLoadPortsAsync();

            InitCommand = OperationAudit.CreateAsync("Initialize", ExecuteInitCommandAsync, () => "Main");
            AutoModeCommand = OperationAudit.CreateAsync("AutoMode", ExecuteAutoModeCommandAsync, () => "Main");
            ManualModeCommand = OperationAudit.CreateAsync("ManualMode", ExecuteManualModelCommandAsync, () => "Main");
            AutoRunCommand = OperationAudit.CreateAsync("AutoRun", ExecuteAutoRunCommandAsync, () => "Main");
            ResetAlarmCommand = OperationAudit.CreateAsync("ResetAlarm", ExecuteResetAlarmCommandAsync, () => "Main");
            AbortCommand = OperationAudit.Create("Abort", ExecuteAbortCommand, () => "Main");
            StopCommand = OperationAudit.CreateAsync("Stop", ExecuteStopCommandAsync, () => "Main");
            AutoReturnCommand = OperationAudit.Create("StopLoad", ExecuteAutoReturnCommand, () => "Main");
            SingleChamberOfflineCommand = new RelayCommand<string>(ExecuteSingleChamberOffline);
            SingleChamberOnlineCommand = new RelayCommand<string>(ExecuteSingleChamberOnline);
            SingleChamberConnectCommand = new RelayCommand<string>(ExecuteSingleChamberConnect);
            SingleChamberInitializeCommand = new RelayCommand<string>(ExecuteSingleChamberInitialize);
            SingleChamberResetAlarmCommand = new RelayCommand<string>(ExecuteSingleChamberResetAlarm);
            SingleChamberAutoModeCommand = new RelayCommand<string>(ExecuteSingleChamberAutoMode);
            SingleChamberManualModeCommand = new RelayCommand<string>(ExecuteSingleChamberManualMode);
            AddWaferCommand = new RelayCommand<string>(ExecuteAddWafer);
            DeleteWaferCommand = new RelayCommand<string>(ExecuteDeleteWafer);

            _polishA = GlobalService.globalChambers.PolishA;
            _polishB = GlobalService.globalChambers.PolishB;
            _polishC = GlobalService.globalChambers.PolishC;
            _polishD = GlobalService.globalChambers.PolishD;
            _rollerA1 = GlobalService.globalChambers.RollerA1;
            _rollerA2 = GlobalService.globalChambers.RollerA2;
            _rollerA3 = GlobalService.globalChambers.RollerA3;
            _rollerB1 = GlobalService.globalChambers.RollerB1;
            _rollerB2 = GlobalService.globalChambers.RollerB2;
            _rollerB3 = GlobalService.globalChambers.RollerB3;
            _penA1 = GlobalService.globalChambers.PenA1;
            _penA2 = GlobalService.globalChambers.PenA2;
            _penB1 = GlobalService.globalChambers.PenB1;
            _penB2 = GlobalService.globalChambers.PenB2;
            _dryA = GlobalService.globalChambers.DryA;
            _dryB = GlobalService.globalChambers.DryB;

            _eFEMRobot = GlobalService.globalRobots.EFEMRobot;
            _transBuffer = GlobalService.globalRobots.TransBuffer;
            _transRobot = GlobalService.globalRobots.TransRobot;
            _fixABuffer = GlobalService.globalRobots.FixABuffer;
            _fixALTP1 = GlobalService.globalRobots.FixALTP1;
            _fixALTP2 = GlobalService.globalRobots.FixALTP2;
            _fixALTP3 = GlobalService.globalRobots.FixALTP3;
            _fixBBuffer = GlobalService.globalRobots.FixBBuffer;
            _fixBLTP1 = GlobalService.globalRobots.FixBLTP1;
            _fixBLTP2 = GlobalService.globalRobots.FixBLTP2;
            _fixBLTP3 = GlobalService.globalRobots.FixBLTP3;
            _cleanARobot1 = GlobalService.globalRobots.CleanARobot1;
            _cleanARobot2 = GlobalService.globalRobots.CleanARobot2;
            _cleanBRobot1 = GlobalService.globalRobots.CleanBRobot1;
            _cleanBRobot2 = GlobalService.globalRobots.CleanBRobot2;

          
        }


        public ICommand InitCommand { get; }
        public ICommand AutoModeCommand { get; }
        public ICommand ManualModeCommand { get; }
        public ICommand AutoRunCommand { get; }
        public ICommand ResetAlarmCommand { get; }
        public ICommand AbortCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand SingleChamberOfflineCommand { get; }
        public ICommand SingleChamberOnlineCommand { get; }
        public ICommand SingleChamberConnectCommand { get; }
        public ICommand SingleChamberInitializeCommand { get; }
        public ICommand SingleChamberResetAlarmCommand { get; }
        public ICommand SingleChamberAutoModeCommand { get; }
        public ICommand SingleChamberManualModeCommand { get; }
        public ICommand AddWaferCommand { get; }
        public ICommand DeleteWaferCommand { get; }

        public ICommand AutoReturnCommand { get; }

        private async Task ExecuteInitCommandAsync()
        {
            await PulseMainPlcCommandAsync("B08");
            foreach (Chamber chamber in GlobalService.globalChambers.listChamber)
            {
                chamber.InitializeCommand();
            }
        }
        private async Task ExecuteAutoRunCommandAsync()
        {
            await PulseMainPlcCommandAsync("B02");
            foreach (Chamber chamber in GlobalService.globalChambers.listChamber)
            {
                chamber.PlcAutoStart();
            }
        }
        private async Task ExecuteManualModelCommandAsync()
        {
            await PulseMainPlcCommandAsync("B0A");
            foreach (Chamber chamber in GlobalService.globalChambers.listChamber)
            {
                chamber.SwitchToManualModeCommand();
            }
        }
        private async Task ExecuteAutoModeCommandAsync()
        {
            await PulseMainPlcCommandAsync("B09");
            foreach (Chamber chamber in GlobalService.globalChambers.listChamber)
            {
                chamber.SwitchToAutoModeCommand();
            }
        }
        private async Task ExecuteResetAlarmCommandAsync()
        {
            await PulseMainPlcCommandAsync("B0B");
            foreach (Chamber chamber in GlobalService.globalChambers.listChamber)
            {
                chamber.ResetError();
                //chamber.ChamberStatus = ChamberStatus.Uninitialized;
            }
            foreach(AlarmManage alarmManage in GlobalService.DataCenter.listAlarmManage)
            {
                foreach(KeyValuePair<string,Alarm> dic in alarmManage.dicAlarm)
                {
                    dic.Value.IsAlarmed = false;
                }
            }
            GlobalService.DataCenter.listAlarms.Clear();
        }
        private async Task ExecuteStopCommandAsync()
        {
            await PulseMainPlcCommandAsync("B04");
        }

        private void ExecuteAutoReturnCommand()
        {
            var plc = GlobalService.plcComm;
            if (!plc.IsPLCConnected)
            {
                Growl.WarningGlobal("PLC 未连接");
                return;
            }

            _ = Task.Run(() =>
            {
                try
                {
                    plc.WriteBoolVari("M170", true);
                }
                catch (Exception ex)
                {
                    Logger.Error($"AutoReturn PLC: {ex.Message}");
                }
            });
        }

        private async Task PulseMainPlcCommandAsync(string address)
        {
            var plc = GlobalService.plcComm;
            if (!plc.IsPLCConnected)
            {
                Growl.WarningGlobal("PLC 未连接");
                return;
            }

            await Task.Run(() => plc.WriteBoolVari(address, true)).ConfigureAwait(false);
            Growl.InfoGlobal("Execute Command Success");
            await Task.Delay(1000).ConfigureAwait(false);
            await Task.Run(() => plc.WriteBoolVari(address, false)).ConfigureAwait(false);
        }
        private void ExecuteAbortCommand()
        {
            foreach (Chamber chamber in GlobalService.globalChambers.listChamber)
            {
                chamber.AbortCommand();
            }
        }
        private void ExecuteSingleChamberOffline(string chamberName)
        {
            var unitIndex = ConvertChamberNameToIndex(chamberName);   
            // 根据Unit编号范围确定设备类型
            if (unitIndex >= 0 && unitIndex < 16) // Chamber: 0-15
            {
                if (unitIndex < GlobalService.globalChambers.listChamber.Count)
                {
                    GlobalService.globalChambers.listChamber[unitIndex].SwitchToOffline();
                }
            }
            else if (unitIndex >= 16 && unitIndex <= 31) // Robot: 16-31 
            {
                int robotIndex = unitIndex - 16;
                if (robotIndex < GlobalService.globalRobots.listRobot.Count)
                {
                    GlobalService.globalRobots.listRobot[robotIndex].SwitchToOffline();
                }
            }
           
        }
        private void ExecuteSingleChamberOnline(string chamberName)
        {
            var unitIndex = ConvertChamberNameToIndex(chamberName);
            // 根据Unit编号范围确定设备类型
            if (unitIndex >= 0 && unitIndex < 16) // Chamber: 0-16
            {
                if (unitIndex < GlobalService.globalChambers.listChamber.Count)
                {
                    GlobalService.globalChambers.listChamber[unitIndex].SwitchToOnline();
                }
            }
            else if (unitIndex >= 16 && unitIndex <= 31) // Robot: 17-31 (对应0-14)
            {
                int robotIndex = unitIndex - 16;
                if (robotIndex < GlobalService.globalRobots.listRobot.Count)
                {
                    GlobalService.globalRobots.listRobot[robotIndex].SwitchToOnline();
                }
            }
        }
        private void ExecuteSingleChamberConnect(string chamberName)
        {
            var unitIndex = ConvertChamberNameToIndex(chamberName);
            if (unitIndex >= 0 && unitIndex < 16)
            {
                GlobalService.kvHostLinkInit.ConnectPLC(chamberName);
            }
            else {
                int robotIndex = unitIndex - 16;
                GlobalService.globalRobots.listRobot[robotIndex].IsConnected();
            }
        }
        private void ExecuteSingleChamberInitialize(string chamberName)
        {
            var unitIndex = ConvertChamberNameToIndex(chamberName);
            // 根据Unit编号范围确定设备类型
            if (unitIndex >= 0 && unitIndex < 16) // Chamber: 0-16
            {
                if (unitIndex < GlobalService.globalChambers.listChamber.Count)
                {
                    GlobalService.globalChambers.listChamber[unitIndex].InitializeCommand();
                }
            }
            else if (unitIndex >= 16 && unitIndex <= 31) // Robot: 17-31 (对应0-14)
            {
                int robotIndex = unitIndex - 16;
                if (robotIndex < GlobalService.globalRobots.listRobot.Count)
                {
                    GlobalService.globalRobots.listRobot[robotIndex].InitializeCommand();
                }
            }
        }
        private void ExecuteSingleChamberResetAlarm(string chamberName)
        {
            var unitIndex = ConvertChamberNameToIndex(chamberName);
            // 根据Unit编号范围确定设备类型
            if (unitIndex >= 0 && unitIndex < 16) // Chamber: 0-16
            {
                if (unitIndex < GlobalService.globalChambers.listChamber.Count)
                {
                    GlobalService.globalChambers.listChamber[unitIndex].ResetError();
                }
            }
            else if (unitIndex >= 16 && unitIndex <= 31) // Robot: 17-31 (对应0-14)
            {
                int robotIndex = unitIndex - 16;
                if (robotIndex < GlobalService.globalRobots.listRobot.Count)
                {
                    GlobalService.globalRobots.listRobot[robotIndex].ResetError();
                }
            }
        }
        private void ExecuteSingleChamberAutoMode(string chamberName)
        {
            var unitIndex = ConvertChamberNameToIndex(chamberName);
            // 根据Unit编号范围确定设备类型
            if (unitIndex >= 0 && unitIndex < 16) // Chamber: 0-16
            {
                if (unitIndex < GlobalService.globalChambers.listChamber.Count)
                {
                    GlobalService.globalChambers.listChamber[unitIndex].SwitchToAutoModeCommand();
                }
            }
            else if (unitIndex >= 16 && unitIndex <= 31) // Robot: 17-31 (对应0-14)
            {
                int robotIndex = unitIndex - 16;
                if (robotIndex < GlobalService.globalRobots.listRobot.Count)
                {
                    GlobalService.globalRobots.listRobot[robotIndex].SwitchToAutoModeCommand();
                }
            }
        }
        private void ExecuteSingleChamberManualMode(string chamberName)
        {
            var unitIndex = ConvertChamberNameToIndex(chamberName);
            // 根据Unit编号范围确定设备类型
            if (unitIndex >= 0 && unitIndex < 16) // Chamber: 0-16
            {
                if (unitIndex < GlobalService.globalChambers.listChamber.Count)
                {
                    GlobalService.globalChambers.listChamber[unitIndex].SwitchToManualModeCommand();
                }
            }
            else if (unitIndex >= 16 && unitIndex <= 31) // Robot: 17-31 (对应0-14)
            {
                int robotIndex = unitIndex - 16;
                if (robotIndex < GlobalService.globalRobots.listRobot.Count)
                {
                    GlobalService.globalRobots.listRobot[robotIndex].SwitchToManualModeCommand();
                }
            }
        }
        private int ConvertChamberNameToIndex(string name)
        {
            return (int)Enum.Parse(typeof(Unit), name);//FIXED修改状态满足机器人与chamber单命令，待完善

        }

        private void ExecuteAddWafer(string unitName)
        {
            try
            {
                var addWaferWindow = new AddWaferDialogView()
                {
                    Title = $"Add Wafer - {unitName}",
                    Width = 600,
                    Height = 400,
                    ResizeMode = ResizeMode.NoResize,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };

                var viewModel = new AddWaferDialogViewModel(unitName);
                viewModel.Title = $"Add Wafer - {unitName}";
                
                // 设置关闭窗口的回调
                viewModel.CloseWindowAction = (dialogResult) =>
                {
                    addWaferWindow.DialogResult = dialogResult;
                    addWaferWindow.Close();
                };

                addWaferWindow.DataContext = viewModel;

                // 设置所有者窗口
                var parentWindow = Application.Current.Windows.OfType<HandyControl.Controls.Window>()
                                             .FirstOrDefault(w => w.IsActive);
                if (parentWindow != null)
                {
                    addWaferWindow.Owner = parentWindow;
                }

                // 显示对话框
                var result = addWaferWindow.ShowDialog();
                if (result == true && viewModel.ResultWafer != null)
                {
                    AddWaferToSystem(viewModel.ResultWafer);
                }
            }
            catch (Exception ex)
            {
                Growl.ErrorGlobal($"显示添加 Wafer 窗口时发生错误: {ex.Message}");
            }
        }

        private void ExecuteDeleteWafer(string unitName)
        {
            try
            {
                var deleteWaferWindow = new DeleteWaferDialogView
                {
                    Title = $"Delete Wafer - {unitName}",
                    Width = 550,
                    Height = 350,
                    ResizeMode = ResizeMode.NoResize,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };

                var viewModel = new DeleteWaferDialogViewModel(unitName);
                viewModel.Title = $"Delete Wafer - {unitName}";
                
                // 设置关闭窗口的回调
                viewModel.CloseWindowAction = (dialogResult) =>
                {
                    deleteWaferWindow.DialogResult = dialogResult;
                    deleteWaferWindow.Close();
                };

                deleteWaferWindow.DataContext = viewModel;

                // 设置所有者窗口
                var parentWindow = Application.Current.Windows.OfType<HandyControl.Controls.Window>()
                                             .FirstOrDefault(w => w.IsActive);
                if (parentWindow != null)
                {
                    deleteWaferWindow.Owner = parentWindow;
                }

                // 显示对话框
                var result = deleteWaferWindow.ShowDialog();
                if (result == true)
                {
                    DeleteWaferFromSystem(viewModel.ResultLoadportID, viewModel.ResultSlotID);
                }
            }
            catch (Exception ex)
            {
                Growl.ErrorGlobal($"显示删除 Wafer 窗口时发生错误: {ex.Message}");
            }
        }

        private void AddWaferToSystem(Wafer wafer)
        {
            try
            {
                var index = wafer.LoadportID * 25 + (wafer.SlotID + 1);
                
                // 如果指定了 SequenceRecipe，则下发到 三菱 PLC
                if (!string.IsNullOrWhiteSpace(wafer.SequenceRecipe))
                {
                    //var success = _sequenceWithPLC.AssignWaferSequenceToPLC(wafer, GlobalService.plcComm);
                    //if (!success)
                    //{
                    //    Growl.ErrorGlobal($"添加 Wafer 失败：无法下发 Recipe 到 PLC");
                    //    return;
                    //}
                }
                //TODO下发到基恩士 PLC 逻辑


                // 添加到全局字典
                lock (GlobalService.dicWaferIndex)
                {
                    if (GlobalService.dicWaferIndex.ContainsKey(index))
                    {
                        GlobalService.dicWaferIndex[index] = wafer;
                        Growl.InfoGlobal($"更新 Wafer: {wafer.WaferName} (Loadport: {wafer.LoadportID}, Slot: {wafer.SlotID})");
                    }
                    else
                    {
                        GlobalService.dicWaferIndex.Add(index, wafer);
                        Growl.SuccessGlobal($"添加 Wafer 成功: {wafer.WaferName} (Loadport: {wafer.LoadportID}, Slot: {wafer.SlotID})");
                    }
                }
                //更新LoadPort列表中的Wafer信息
                foreach (var loadPort in AllLoadPorts)
                {
                    // 找到对应的 loadPort（通过检查 WaferList 中的 wafer 是否匹配 LoadportID）
                    if (loadPort.WaferList != null && loadPort.WaferList.Count > 0)
                    {
                        // 检查第一个 wafer 的 LoadportID 是否匹配（所有 wafer 的 LoadportID 应该相同）
                        var firstWafer = loadPort.WaferList.FirstOrDefault();
                        if (firstWafer != null && firstWafer.LoadportID == wafer.LoadportID)
                        {
                            // 由于 WaferList 是倒序的，需要转换索引
                            int waferIndex = 24 - wafer.SlotID;
                            if (waferIndex >= 0 && waferIndex < loadPort.WaferList.Count)
                            {
                                var existingWafer = loadPort.WaferList[waferIndex];
                                // 确保是同一个 wafer
                                if (existingWafer.LoadportID == wafer.LoadportID && existingWafer.SlotID == wafer.SlotID)
                                {
                                    // 更新 wafer 信息
                                    existingWafer.WaferName = wafer.WaferName;
                                    existingWafer.SequenceRecipe = wafer.SequenceRecipe;
                                    existingWafer.WaferStatus = wafer.WaferStatus;
                                    existingWafer.CurrentStation = wafer.CurrentStation;
                                    
                                    Logger.Info($"Updated wafer in LoadPort {loadPort.PortNumber} WaferList: L{wafer.LoadportID+1}S{wafer.SlotID+1}");
                                }
                            }
                            break; // 找到对应的 loadPort 后退出循环
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Growl.ErrorGlobal($"添加 Wafer 时发生错误: {ex.Message}");
            }
        }

        private void DeleteWaferFromSystem(int loadportId, int slotId)
        {
            try
            {
                var index = loadportId * 25 + (slotId + 1);
                
                lock (GlobalService.dicWaferIndex)
                {
                    if (GlobalService.dicWaferIndex.ContainsKey(index))
                    {
                        var wafer = GlobalService.dicWaferIndex[index];
                        GlobalService.dicWaferIndex.Remove(index);

                        // 清除 PLC 中的 wafer 数据（将相关地址清零）
                        // ClearWaferFromPLC(wafer);


                        // TODO当前Wafer存在清除三菱 PLC信息

                        //var success = _sequenceWithPLC.AssignWaferSequenceToPLC(wafer, GlobalService.plcComm);
                        //if (!success)
                        //{
                        //    Growl.ErrorGlobal($"添加 Wafer 失败：无法下发 Recipe 到 PLC");
                        //    return;
                        //}

                        //TODO下发到基恩士 PLC 逻辑

                        //更新LoadPort列表中的Wafer信息
                        // 根据 SEMI E90 状态转换表，删除 wafer 时需要设置适当的状态
                        foreach (var loadPort in AllLoadPorts)
                        {
                            // 找到对应的 loadPort（通过检查 WaferList 中的 wafer 是否匹配 LoadportID）
                            if (loadPort.WaferList != null && loadPort.WaferList.Count > 0)
                            {
                                // 检查第一个 wafer 的 LoadportID 是否匹配（所有 wafer 的 LoadportID 应该相同）
                                var firstWafer = loadPort.WaferList.FirstOrDefault();
                                if (firstWafer != null && firstWafer.LoadportID == loadportId)
                                {
                                    // 由于 WaferList 是倒序的，需要转换索引
                                    int waferIndex = 24 - slotId;
                                    if (waferIndex >= 0 && waferIndex < loadPort.WaferList.Count)
                                    {
                                        var existingWafer = loadPort.WaferList[waferIndex];
                                        // 确保是同一个 wafer
                                        if (existingWafer.LoadportID == loadportId && existingWafer.SlotID == slotId)
                                        {
                                            // 根据 SEMI E90 状态转换表设置 wafer 状态
                                            // Transition 7/9: 从任何状态转换到 (Extinction)
                                            // 根据当前状态设置适当的终止状态
                                            var currentStatus = existingWafer.WaferStatus;
                                            
                                            // 如果正在处理中，设置为 Error（表示处理被中断）
                                            if (currentStatus == WaferStatus.Processing)
                                            {
                                                existingWafer.WaferStatus = WaferStatus.Error;
                                                Logger.Info($"Wafer processing aborted, status set to Error: L{loadportId + 1}S{slotId + 1}");
                                            }
                                            // 如果等待处理或已分配 Recipe，设置为 NonWafer（表示被移除/跳过）
                                            else if (currentStatus == WaferStatus.WaitProcess || 
                                                     currentStatus == WaferStatus.WaitAssignRecipe)
                                            {
                                                existingWafer.WaferStatus = WaferStatus.NonWafer;
                                                Logger.Info($"Wafer removed before processing, status set to NonWafer: L{loadportId + 1}S{slotId + 1}");
                                            }
                                            // 其他状态也设置为 NonWafer（对应 Extinction 状态）
                                            else
                                            {
                                                existingWafer.WaferStatus = WaferStatus.NonWafer;
                                                Logger.Info($"Wafer removed, status set to NonWafer: L{loadportId + 1}S{slotId + 1}");
                                            }
                                            
                                            // 清除 wafer 的 Recipe 信息（对应 Transition 7/9: substrate object is deleted）
                                            existingWafer.SequenceRecipe = string.Empty;
                                            
                                            // 重置位置信息为 Loadport（wafer 已从设备中移除）
                                            existingWafer.CurrentStation = CurrentStation.Loadport;
                                            
                                            // 清除选中状态
                                            existingWafer.IsSelected = false;
                                            
                                            // 保留 WaferName 以便追踪（可选：也可以清除）
                                            // existingWafer.WaferName = string.Empty;

                                            Logger.Info($"Deleted wafer in LoadPort {loadPort.PortNumber} WaferList: L{loadportId + 1}S{slotId + 1}, Status: {existingWafer.WaferStatus}");
                                        }
                                    }
                                    break; // 找到对应的 loadPort 后退出循环
                                }
                            }
                        }

                        Growl.SuccessGlobal($"删除 Wafer 成功 (Loadport: {loadportId}, Slot: {slotId})");

                    }
                    else
                    {
                        Growl.WarningGlobal($"未找到指定的 Wafer (Loadport: {loadportId}, Slot: {slotId})");
                    }
                }
            }
            catch (Exception ex)
            {
                Growl.ErrorGlobal($"删除 Wafer 时发生错误: {ex.Message}");
            }
        }

        private void ClearWaferFromPLC(Wafer wafer)
        {
            try
            {
                // 清除 PLC 中的 wafer 数据
                var baseAddress = wafer.LoadportID * 25 + wafer.SlotID;
                var addressOffset = baseAddress * 50;
                
                // 创建清零数据块（所有数据设为 0）
                var zeroData = new byte[200]; // 假设每个 wafer 需要 200 字节
                
                // 清除 Polish 数据块
                var polishAddress = $"D{addressOffset + SquenceWithPLC.PolishMainASideCheckedAddress}";
                GlobalService.plcComm.WriteByteArray(polishAddress, new byte[18]);
                
                // 清除 Clean Main 数据块
                var cleanMainAddress = $"D{addressOffset + SquenceWithPLC.CleanMainASideCheckedAddress}";
                GlobalService.plcComm.WriteByteArray(cleanMainAddress, new byte[6]);
                
                // 清除 Clean A Side 数据块
                var cleanAAddress = $"D{addressOffset + SquenceWithPLC.CleanASideRollerOp1CheckedAddress}";
                GlobalService.plcComm.WriteByteArray(cleanAAddress, new byte[18]);
                
                // 清除 Clean B Side 数据块
                var cleanBAddress = $"D{addressOffset + SquenceWithPLC.CleanBSideRollerOp1CheckedAddress}";
                GlobalService.plcComm.WriteByteArray(cleanBAddress, new byte[18]);
            }
            catch (Exception ex)
            {
                Growl.WarningGlobal($"清除 PLC 中的 Wafer 数据时发生错误: {ex.Message}");
            }
        }
        
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            if (parameter is T typedParameter)
                return _canExecute?.Invoke(typedParameter) ?? true;

            return _canExecute?.Invoke(default(T)) ?? true;
        }

        public void Execute(object parameter)
        {
            if (parameter is T typedParameter)
            {
                _execute(typedParameter);
            }
            else
            {
                _execute(default(T));
            }
        }
    }
}


