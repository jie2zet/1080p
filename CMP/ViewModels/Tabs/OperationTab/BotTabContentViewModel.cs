using CMP.Services.Auth;
using CMP.Models.Operation.Bot;
using CMP.ViewModels.Common;
using HandyControl.Controls;
using System.Collections.ObjectModel;
using CMP.Models.DataCenter;
using CMP.Models.Resource;
using Prism.Commands;

namespace CMP.ViewModels.Tabs.OperationTab;

public class BotTabContentViewModel : TabContentViewModelBase
{
    public new string? TabTitle
    {
        get => _tabTitle;
        set => SetProperty(ref _tabTitle, value);
    }

    private string? _tabTitle;

    private int _tabId;

    public int TabId
    {
        get => _tabId;
        set => SetProperty(ref _tabId, value);
    }

    private ObservableCollection<AxisData> _botAxis = new();

    public ObservableCollection<AxisData> BotAxis
    {
        get => _botAxis;
        set => SetProperty(ref _botAxis, value);
    }

    public DelegateCommand<AxisData> ServerOnCommand { get; set; }
    public DelegateCommand<AxisData> RunCommand { get; set; }
    public DelegateCommand<AxisData> HomeCommand { get; set; }
    public DelegateCommand<AxisData> PressJogPlusCommand { get; set; }
    public DelegateCommand<AxisData> ReleaseJogPlusCommand { get; set; }
    public DelegateCommand<AxisData> PressJogMinusCommand { get; set; }
    public DelegateCommand<AxisData> ReleaseJogMinusCommand { get; set; }
    public DelegateCommand<AxisData> StopCommand { get; set; }
    public DelegateCommand<AxisData> SetPosCommand { get; set; }

    DataCenter _dataCenter;
    private readonly int _offset;
    private readonly int _size;

    public BotTabContentViewModel(int id, string title, int offset = 0, int size = 10) : base(id, title)
    {
        TabId = id;
        TabTitle = title;

        _offset = offset;
        _size = size;

        ServerOnCommand = OperationAudit.Create<AxisData>("Servo", ServerOnCommandExecute, () => TabTitle ?? string.Empty, formatTarget: a => a.ModuleName);
        RunCommand = OperationAudit.Create<AxisData>("Run", RunCommandExecute, () => TabTitle ?? string.Empty, formatTarget: a => a.ModuleName);
        HomeCommand = OperationAudit.Create<AxisData>("Home", HomeCommandExecute, () => TabTitle ?? string.Empty, formatTarget: a => a.ModuleName);
        PressJogPlusCommand = new DelegateCommand<AxisData>(axis => PressCommandExecute(axis, "JogPlus"));
        ReleaseJogPlusCommand = new DelegateCommand<AxisData>(axis => ReleaseCommandExecute(axis, "JogPlus"));
        PressJogMinusCommand = new DelegateCommand<AxisData>(axis => PressCommandExecute(axis, "JogMinus"));
        ReleaseJogMinusCommand = new DelegateCommand<AxisData>(axis => ReleaseCommandExecute(axis, "JogMinus"));
        StopCommand = OperationAudit.Create<AxisData>("Stop", StopCommandExecute, () => TabTitle ?? string.Empty, formatTarget: a => a.ModuleName);
        SetPosCommand = OperationAudit.Create<AxisData>("SetPos", SetPosCommandExecute, () => TabTitle ?? string.Empty, formatTarget: a => a.ModuleName);

        _dataCenter = GlobalService.DataCenter;

        InitBotAxisData();
    }

    private void InitBotAxisData()
    {
        BotAxis.Clear();
        foreach (var axis in _dataCenter.dicAxis.Values.Skip(_offset).Take(_size))
        {
            BotAxis.Add(new AxisData
            {
                ModuleName = axis.listDataInSingleAxis.FirstOrDefault()?.Module ?? string.Empty,
            });
            axis.AxisDataRefreshed += AxisDataRefreshedHandler;
        }
    }
    
    private void AxisDataRefreshedHandler(object? sender, AxisDataRefreshedEventArgs e)
    {
        var axis = _dataCenter.dicAxis[e.AxisName];
        var axisData = BotAxis.FirstOrDefault(a => a.ModuleName == axis.axisName);
        
        if (axisData != null)
        {       
            //??axisData???null,????????????????????????? 
            
            axisData.CurrentPosition = axis.GetCurrentPosition();
            
        }
    }

    private void ServerOnCommandExecute(AxisData axisData)
    {

        _dataCenter.dicAxis[axisData.ModuleName].ServerControl(axisData.ServerState == State.On);
        axisData.ServerState = axisData.ServerState == State.On ? State.Off : State.On;
        Growl.InfoGlobal($"Server state changed to {axisData.ServerState} for {axisData.ModuleName}");
    }

    private void RunCommandExecute(AxisData axisData)
    {
        
        _dataCenter.dicAxis[axisData.ModuleName].GoToStationPosition(axisData.Position);
        Growl.InfoGlobal(
            $"Run command executed for {axisData.ModuleName} to position {axisData.Position} with velocity {axisData.Velocity}");
    }

    private void HomeCommandExecute(AxisData axisData)
    {
        _dataCenter.dicAxis[axisData.ModuleName].GoHome();
        Growl.InfoGlobal($"Home command executed for {axisData.ModuleName} with velocity {axisData.Velocity}");
    }

    private void PressCommandExecute(AxisData axisData, string jogType)
    {
        _dataCenter.dicAxis[axisData.ModuleName].ModifyVelocity(axisData.Velocity);
        _dataCenter.dicAxis[axisData.ModuleName].StartJog(jogType);
        Growl.InfoGlobal($"Jog Plus command executed for {axisData.ModuleName} with {jogType}");
    }

    private void ReleaseCommandExecute(AxisData axisData, string jogType)
    {
        _dataCenter.dicAxis[axisData.ModuleName].ModifyVelocity(axisData.Velocity);
        _dataCenter.dicAxis[axisData.ModuleName].StopJog(jogType);
        Growl.InfoGlobal($"Jog Minus command executed for {axisData.ModuleName} with {jogType}");
    }


    private void StopCommandExecute(AxisData axisData)
    {
        _dataCenter.dicAxis[axisData.ModuleName].Stop();
        Growl.InfoGlobal($"Stop command executed for {axisData.ModuleName}");
    }

    private void SetPosCommandExecute(AxisData axisData)
    {
        _dataCenter.dicAxis[axisData.ModuleName]
            .SetStationPosition(axisData.Position, axisData.SelectedStationPosition);
        Growl.InfoGlobal($"Set position {axisData.Position} to {axisData.CurrentPosition} for {axisData.ModuleName}");
    }

}