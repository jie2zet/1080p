using CMP.Models.Operation.RoboticArm;
using CMP.Utils;
using CMP.ViewModels.Common;
using HandyControl.Controls;
using NetTaste;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Action = CMP.Models.Operation.RoboticArm.Action;

namespace CMP.ViewModels.Tabs.OperationTab;

public class RoboticArmTabContentViewModel : TabContentViewModelBase
{
    private string? _tabTitle;
    public string? TabTitle
    {
        get => _tabTitle;
        set => SetProperty(ref _tabTitle, value);
    }
        
    private int _tabId;
    public int TabId
    {
        get => _tabId;
        set => SetProperty(ref _tabId, value);
    }
    
    private ObservableCollection<RoboticArm> _roboticArm = [];
    public ObservableCollection<RoboticArm> RoboticArm
    {
        get => _roboticArm;
        set => SetProperty(ref _roboticArm, value);
    }

    /// <summary>?? GlobalService.globalRobots.listRobot ???????EFEMRobot???????????υτ RobotStatus / SV_STCtrl ??????</summary>
    public List<CMP.Models.Resource.RoboticArm>? EquipRobots => GlobalService.globalRobots?.listRobot;
    
    public List<string> ArmSource { get; } = Enum.GetNames<Arm>().ToList();
    
    public List<string> ActionSource { get; } = Enum.GetNames<Action>().ToList();
    public List<string> PositionSource { get; } = Enum.GetNames<Position>().ToList();
    public List<string> SlotSource { get; } = Enumerable.Range(1, 25).Select(i => i.ToString()).ToList();
    
    #region Selected Properties
    
    public ObservableCollection<string?> SelectedArms { get; }
    public ObservableCollection<string?> SelectedActions { get; }
    public ObservableCollection<string?> SelectedPositions { get; }
    public ObservableCollection<string?> SelectedSlot { get; }
    
    #endregion
    
    public DelegateCommand<string> AutoModeOnCommand { get; set; }
    public DelegateCommand<RoboticArm> RunCommand { get; set; }
    public DelegateCommand<RoboticArm> PauseCommand { get; set; }
    public DelegateCommand<RoboticArm> StopCommand { get; set; }
    public DelegateCommand<RoboticArm> AbortCommand { get; set; }
    public DelegateCommand<RoboticArm> ResetCommand { get; set; }

    private const int RoboticArmCount = 15;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isScanning = false;

    public bool IsScanning
    {
        get => _isScanning;
        set => SetProperty(ref _isScanning, value);
    }

    public RoboticArmTabContentViewModel(int id, string title) : base(id, title)
    {
        TabId = id;
        TabTitle = title;
        
        // Initialize selected collections with first enum values
        SelectedArms = new ObservableCollection<string?>(Enumerable.Repeat(ArmSource.FirstOrDefault(), RoboticArmCount));
        SelectedActions = new ObservableCollection<string?>(Enumerable.Repeat(ActionSource.FirstOrDefault(), RoboticArmCount));
        SelectedPositions = new ObservableCollection<string?>(Enumerable.Repeat(PositionSource.FirstOrDefault(), RoboticArmCount));
        SelectedSlot = new ObservableCollection<string?>(Enumerable.Repeat(SlotSource.FirstOrDefault(), RoboticArmCount));

        
        // Initialize Robotic Arms
        InitializeRoboticArms();
        
        AutoModeOnCommand = new DelegateCommand<string>(async s => await AutoModeOnCommandExecuteAsync(s));
        RunCommand = new DelegateCommand<RoboticArm>(async r => await RunCommandExecuteAsync(r));
        PauseCommand = new DelegateCommand<RoboticArm>(async r => await RunCommandExecuteAsync(r));
        StopCommand = new DelegateCommand<RoboticArm>(async r => await RunCommandExecuteAsync(r));
        AbortCommand = new DelegateCommand<RoboticArm>(async r => await RunCommandExecuteAsync(r));
        ResetCommand = new DelegateCommand<RoboticArm>(async r => await RunCommandExecuteAsync(r));
        // ??????????????????????????
       // StartAutomaticScanning();
    }
    /// <summary>
    /// ??????????????????????
    /// </summary>
    private void StartAutomaticScanning()
    {
        if (IsScanning) return;

        IsScanning = true;
        _cancellationTokenSource = new CancellationTokenSource();

        Task.Run(async () => {
            try
            {
                await AutomaticScanAllArmsAsync(_cancellationTokenSource.Token);
            }
            finally
            {
                IsScanning = false;
            }
        }, _cancellationTokenSource.Token);
    }
    /// <summary>
    /// ??????????????????
    /// ???1?????????????????????????M6152?????????????48?????
    /// ?????1,3,5,7,9?????Initialized,Running,Stopped,Empty,Alarm??
    /// </summary>
    /// <param name="cancellationToken">???????</param>
    private async Task AutomaticScanAllArmsAsync(CancellationToken cancellationToken)
    {
        Growl.InfoGlobal("?????????????????????...");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await ScanAllArmsOnceAsync(cancellationToken);

                // ???1?????????
                await Task.Delay(1000, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // ???????
                break;
            }
            catch (Exception ex)
            {
                Growl.WarningGlobal($"???????????????: {ex.Message}");

                // ???????????????????
                try
                {
                    await Task.Delay(1000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        IsScanning = false;
        if (!cancellationToken.IsCancellationRequested)
        {
            Growl.InfoGlobal("??????????????????????");
        }
        else
        {
            Growl.InfoGlobal("???????????????????????");
        }
    }
    /// <summary>
    /// ????????????????
    /// </summary>
    /// <param name="cancellationToken">???????</param>
    private async Task ScanAllArmsOnceAsync(CancellationToken cancellationToken)
    {
       
        const int addressOffsetPerArm = 48;
        // ?????: 1-Initialized, 3-Running, 5-Stopped, 7-Empty, 9-Alarm,15-Manual
        var offsetsToRead = new[] { 1, 3, 5, 7, 9 ,11,13,15};
        var statusNames = new[] { "Initialized", "Running", "Stopped", "Empty", "Alarm","","","Manual" };

        for (int armIndex = 0; armIndex < RoboticArmCount; armIndex++)
        {
            // ????????????
            if (cancellationToken.IsCancellationRequested)
                break;

            string detectedStatus = "Unknown";
            bool statusFound = false;

            for (int i = 0; i < offsetsToRead.Length; i++)
            {
                // ???????????????
                if (cancellationToken.IsCancellationRequested)
                    break;
                if (i == 5 || i == 6) continue;
                var offset = offsetsToRead[i];
                var addressOffset = addressOffsetPerArm * armIndex + offset;
                var address = $"M{6152 + addressOffset}";

                try
                {
                    bool value = await Task.Run(() => GlobalService.plcComm.ReadBoolVari(address), cancellationToken);

                    if (value)
                    {
                        detectedStatus = statusNames[i];
                        statusFound = true;
                        break; // ??????true????????????????????????
                    }
                }
                catch (OperationCanceledException)
                {
                    //throw; // ????????????
                }
                catch (Exception ex)
                {
                    Growl.WarningGlobal($"????????? {armIndex + 1} ??? {address} ???: {ex.Message}");
                }
            }

            // ?????????????ArmStatus???
            if (statusFound)
            {
                int armId = armIndex + 1;
                string finalStatus = detectedStatus;

                // ?????UI????????
                Application.Current?.Dispatcher?.BeginInvoke(() => {
                    if (armId >= 1 && armId <= RoboticArm.Count)
                    {
                        RoboticArm[armId - 1].ArmStatus = finalStatus;
                    }
                });
            }
        }
    }
    private void InitializeRoboticArms()
    {
        for (var i = 0; i < 16; i++)
        {
            RoboticArm.Add(new RoboticArm
            {
                AutoMode = AutoMode.Off,
                Arm = Arm.UpperArm,
                Action = Action.Get,
                Position = Position.Cassette,
                Slot = 1
            });
        }
    }

    private async Task RunCommandExecuteAsync(RoboticArm roboticArmParam)
    { 
        if(roboticArmParam != null)
        {
            // show the selected robotic arm's details
            if (roboticArmParam.ArmId >= 1 && roboticArmParam.ArmId <= 15)
            {
                var roboticArm = RoboticArm[roboticArmParam.ArmId - 1];

                roboticArm.Arm = Enum.Parse<Arm>(SelectedArms[roboticArmParam.ArmId - 1] ?? nameof(Arm.UpperArm));
                roboticArm.Action = Enum.Parse<Action>(SelectedActions[roboticArmParam.ArmId - 1] ?? nameof(Action.Get));
                roboticArm.Position = Enum.Parse<Position>(SelectedPositions[roboticArmParam.ArmId - 1] ?? nameof(Position.Cassette));
                roboticArm.Slot = int.TryParse(SelectedSlot[roboticArmParam.ArmId - 1], out var slot) ? slot : 1;
                await PlcPulse.BoolWritePulseAsync(GlobalService.plcComm, roboticArmParam.PLCAddress, 1000);
                Growl.InfoGlobal($"Robotic Arm {roboticArmParam.ArmId}:\n" +
                                 $"Arm: {roboticArm.Arm}\n" +
                                 $"Action: {roboticArm.Action}\n" +
                                 $"Position: {roboticArm.Position}\n" +
                                 $"Slot: {roboticArm.Slot}\n");
            }
            else
            {
                Growl.ErrorGlobal("Invalid Robotic Arm Index");
            }
        }
        
    }

    private async Task AutoModeOnCommandExecuteAsync(string obj)
    {
        if (int.TryParse(obj, out int armIndex) && armIndex >= 1 && armIndex <= 15)
        {
            var roboticArm = RoboticArm[armIndex - 1];
            roboticArm.AutoMode = roboticArm.AutoMode == AutoMode.On ? AutoMode.Off : AutoMode.On;
            string address=String.Empty;
            string modeStatusAddress = string.Empty;
            if (armIndex == 1)
            {
                address = roboticArm.AutoMode == AutoMode.On ? "M1841" : "M1840";
                modeStatusAddress = roboticArm.AutoMode == AutoMode.On ? "M6166" : "M6167";
            }
            else if (armIndex == 2)
            {
                address = roboticArm.AutoMode == AutoMode.On ? "M1889" : "M1888";
                modeStatusAddress = roboticArm.AutoMode == AutoMode.On ? "M6214" : "M6215";
            }
            else if (armIndex == 3)
            {
                address = roboticArm.AutoMode == AutoMode.On ? "M1937" : "M1936";
                modeStatusAddress = roboticArm.AutoMode == AutoMode.On ? "M6262" : "M6263";
            }
            else if (armIndex == 4)
            {
                address = roboticArm.AutoMode == AutoMode.On ? "M1984" : "M1983";
                modeStatusAddress = roboticArm.AutoMode == AutoMode.On ? "M6310" : "M6311";
            }
            else if (armIndex == 5)
            {
                address = roboticArm.AutoMode == AutoMode.On ? "M2033" : "M2032";
                modeStatusAddress = roboticArm.AutoMode == AutoMode.On ? "M6358" : "M6359";
            }
            else if (armIndex == 6)
            {
                address = roboticArm.AutoMode == AutoMode.On ? "M2081" : "M2080";
                modeStatusAddress = roboticArm.AutoMode == AutoMode.On ? "M6406" : "M6407";
            }
            else if (armIndex == 7)
            {
                address = roboticArm.AutoMode == AutoMode.On ? "M2129" : "M2128";
                modeStatusAddress = roboticArm.AutoMode == AutoMode.On ? "M6454" : "M6455";
            }
            else if (armIndex == 8)
            {
                address = roboticArm.AutoMode == AutoMode.On ? "M2177" : "M2176";
                modeStatusAddress = roboticArm.AutoMode == AutoMode.On ? "M6502" : "M6503";
            }
            else if (armIndex == 9)
            {
                address = roboticArm.AutoMode == AutoMode.On ? "M2225" : "M2224";
                modeStatusAddress = roboticArm.AutoMode == AutoMode.On ? "M6550" : "M6551";
            }
            else if (armIndex == 10)
            {
                address = roboticArm.AutoMode == AutoMode.On ? "M2273" : "M2272";
                modeStatusAddress = roboticArm.AutoMode == AutoMode.On ? "M6598" : "M6599";
            }
            else if (armIndex == 11)
            {
                address = roboticArm.AutoMode == AutoMode.On ? "M2321" : "M2320";
                modeStatusAddress = roboticArm.AutoMode == AutoMode.On ? "M6646" : "M6647";
            }
            else if (armIndex == 12)
            {
                address = roboticArm.AutoMode == AutoMode.On ? "M2369" : "M2368";
                modeStatusAddress = roboticArm.AutoMode == AutoMode.On ? "M6694" : "M6695";
            }
            else if (armIndex == 13)
            {
                address = roboticArm.AutoMode == AutoMode.On ? "M2417" : "M2416";
                modeStatusAddress = roboticArm.AutoMode == AutoMode.On ? "M6742" : "M6743";
            }
            else if (armIndex == 14)
            {
                address = roboticArm.AutoMode == AutoMode.On ? "M2465" : "M2464";
                modeStatusAddress = roboticArm.AutoMode == AutoMode.On ? "M6790" : "M6791";
            }
            else if (armIndex == 15)
            {
                address = roboticArm.AutoMode == AutoMode.On ? "M2513" : "M2512";
                modeStatusAddress = roboticArm.AutoMode == AutoMode.On ? "M6838" : "M6839";
            }
            await PlcPulse.BoolWritePulseAsync(GlobalService.plcComm, address, 1000);
            Growl.InfoGlobal($"Robotic Arm {armIndex}: AutoMode = {roboticArm.AutoMode}");
        }
    }
    /// <summary>
    /// ????????
    /// </summary>
    public void StopAutomaticScanning()
    {
        if (!IsScanning) return;

        _cancellationTokenSource?.Cancel();
        IsScanning = false;
        Growl.InfoGlobal("?????????????????");
    }
    public  void Dispose() {
        StopAutomaticScanning();
        _cancellationTokenSource?.Dispose();
        //base.Dispose();

    }
}