using CMP.Services.Auth;
using CMP.Constants.Plc;
using CMP.Extensions;
using CMP.Lang;
using CMP.Models.kvsPlc;
using CMP.Models.kvsPlc.Roll;
using CMP.Models.Operation.Polish;
using CMP.Models.Operation;
using CMP.Models.Recipe.RecipeEdit;
using CMP.Models.Recipe.Sequence;
using CMP.Models.Resource;
using CMP.Services.Log;
using CMP.Utils;
using CMP.ViewModels.Common;
using CMP.ViewModels.Tabs.OperationTab.PolishUnitTab;
using Dm.util;
using DocumentFormat.OpenXml.Wordprocessing;
using HandyControl.Controls;
using NetTaste;
using Newtonsoft.Json;
using Prism.Navigation.Regions.Behaviors;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Core.Common.CommandTrees;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows;
using System.Windows.Threading;
using System.Xaml;
using WPFLocalizeExtension.Engine;
using static CMP.Models.kvsPlc.SV.KvStruct;
using KHL = CMP.Models.kvsPlc.KvHostLink;
using KST = CMP.Models.kvsPlc.Polish.KvStruct;
using PlcHandle = CMP.Constants.Plc.PlcHandle;
using RecipeData = CMP.Models.Recipe.RecipeEdit.Recipe.Data;

namespace CMP.ViewModels.Tabs.OperationTab;

public class PolishUnitTabContentViewModel : TabContentViewModelBase
{
    private const ushort PolishMotorCommandValue = 10;

    private readonly record struct RotaryMotorPlcAddresses(
        uint StartCommand,
        uint StopCommand,
        uint SetVelocity,
        uint ConfirmedVelocity,
        uint ActualVelocity,
        uint? ActualPosition,
        uint JogVelocity,
        uint JogBitWord,
        ushort JogPlusOffset,
        ushort JogMinusOffset);

    private sealed record RotaryMotorFeedback(
        float? PlatenSetVelocity,
        float? PlatenConfirmedVelocity,
        float? PlatenActualVelocity,
        float? PlatenActualPosition,
        float? PlatenJogVelocity,
        float? HeadSetVelocity,
        float? HeadConfirmedVelocity,
        float? HeadActualVelocity,
        float? HeadJogVelocity,
        float? ConditionerSetVelocity,
        float? ConditionerConfirmedVelocity,
        float? ConditionerActualVelocity,
        float? ConditionerJogVelocity);

    private static readonly IReadOnlyDictionary<string, RotaryMotorPlcAddresses> RotaryMotorPlcAddressMap =
        new Dictionary<string, RotaryMotorPlcAddresses>(StringComparer.Ordinal)
        {
            ["PlatenMotor"] = new(
                PlcHandle.CmdPolish.Motor.PlatenRotation.Run,
                PlcHandle.CmdPolish.Motor.PlatenRotation.Stop,
                PlcHandle.CmdPolish.Motor.PlatenRotation.Param.Velocity.ParamHandle(),
                PlcHandle.CmdPolish.Motor.PlatenRotation.Address.ConfirmedVelocityW,
                PlcHandle.CmdPolish.Motor.PlatenRotation.Address.ActualVelocityW,
                PlcHandle.CmdPolish.Motor.PlatenRotation.Address.ActualPositionW,
                PlcHandle.CmdPolish.Motor.PlatenRotation.Address.JogVelocityZf,
                PlcHandle.CmdPolish.Motor.PlatenRotation.Address.JogBitWordZf,
                PlcHandle.CmdPolish.Motor.PlatenRotation.Address.JogPlusBitOffset,
                PlcHandle.CmdPolish.Motor.PlatenRotation.Address.JogMinusBitOffset),
            ["HeadRotateMotor"] = new(
                PlcHandle.CmdPolish.Motor.HeadRotation.Run,
                PlcHandle.CmdPolish.Motor.HeadRotation.Stop,
                PlcHandle.CmdPolish.Motor.HeadRotation.Param.Velocity.ParamHandle(),
                PlcHandle.CmdPolish.Motor.HeadRotation.Address.ConfirmedVelocityW,
                PlcHandle.CmdPolish.Motor.HeadRotation.Address.ActualVelocityW,
                null,
                PlcHandle.CmdPolish.Motor.HeadRotation.Address.JogVelocityZf,
                PlcHandle.CmdPolish.Motor.HeadRotation.Address.JogBitWordZf,
                PlcHandle.CmdPolish.Motor.HeadRotation.Address.JogPlusBitOffset,
                PlcHandle.CmdPolish.Motor.HeadRotation.Address.JogMinusBitOffset),
            ["DiskRotateMotor"] = new(
                PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.Run,
                PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.Stop,
                PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.Param.Velocity.ParamHandle(),
                PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.Address.ConfirmedVelocityW,
                PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.Address.ActualVelocityW,
                null,
                PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.Address.JogVelocityZf,
                PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.Address.JogBitWordZf,
                PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.Address.JogPlusBitOffset,
                PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.Address.JogMinusBitOffset)
        };

    private readonly record struct SwingMotorPlcAddresses(
        uint StartCommand,
        uint StopCommand,
        uint SetVelocity,
        uint SetPosition,
        uint ActualVelocity,
        uint ActualPosition,
        uint JogVelocity,
        uint JogBitWord,
        ushort JogPlusOffset,
        ushort JogMinusOffset);

    private sealed record SwingMotorFeedback(
        float? HeadSweepSetVelocity,
        float? HeadSweepSetPosition,
        float? HeadSweepActualVelocity,
        float? HeadSweepActualPosition,
        float? HeadSweepJogVelocity,
        float? HeadLiftSetVelocity,
        float? HeadLiftSetPosition,
        float? HeadLiftActualVelocity,
        float? HeadLiftActualPosition,
        float? HeadLiftJogVelocity,
        float? ConditionerSweepSetVelocity,
        float? ConditionerSweepSetPosition,
        float? ConditionerSweepActualVelocity,
        float? ConditionerSweepActualPosition,
        float? ConditionerSweepJogVelocity,
        float? SlurrySweepSetVelocity,
        float? SlurrySweepSetPosition,
        float? SlurrySweepActualVelocity,
        float? SlurrySweepActualPosition,
        float? SlurrySweepJogVelocity);

    private static readonly IReadOnlyDictionary<string, SwingMotorPlcAddresses> SwingMotorPlcAddressMap =
        new Dictionary<string, SwingMotorPlcAddresses>(StringComparer.Ordinal)
        {
            ["HeadSweepMotor"] = new(
                PlcHandle.CmdPolish.Motor.HeadSweep.AnyPosition,
                PlcHandle.CmdPolish.Motor.HeadSweep.Stop,
                PlcHandle.CmdPolish.Motor.HeadSweep.Param.Velocity.ParamHandle(),
                PlcHandle.CmdPolish.Motor.HeadSweep.Param.Position.ParamHandle(),
                PlcHandle.CmdPolish.Motor.HeadSweep.Address.ActualVelocityW,
                PlcHandle.CmdPolish.Motor.HeadSweep.Address.ActualPositionW,
                PlcHandle.CmdPolish.Motor.HeadSweep.Address.JogVelocityZf,
                PlcHandle.CmdPolish.Motor.HeadSweep.Address.JogBitWordZf,
                PlcHandle.CmdPolish.Motor.HeadSweep.Address.JogPlusBitOffset,
                PlcHandle.CmdPolish.Motor.HeadSweep.Address.JogMinusBitOffset),
            ["HeadLiftMotor"] = new(
                PlcHandle.CmdPolish.Motor.HeadUpDown.AnyPosition,
                PlcHandle.CmdPolish.Motor.HeadUpDown.Stop,
                PlcHandle.CmdPolish.Motor.HeadUpDown.Param.Velocity.ParamHandle(),
                PlcHandle.CmdPolish.Motor.HeadUpDown.Param.Position.ParamHandle(),
                PlcHandle.CmdPolish.Motor.HeadUpDown.Address.ActualVelocityW,
                PlcHandle.CmdPolish.Motor.HeadUpDown.Address.ActualPositionW,
                PlcHandle.CmdPolish.Motor.HeadUpDown.Address.JogVelocityZf,
                PlcHandle.CmdPolish.Motor.HeadUpDown.Address.JogBitWordZf,
                PlcHandle.CmdPolish.Motor.HeadUpDown.Address.JogPlusBitOffset,
                PlcHandle.CmdPolish.Motor.HeadUpDown.Address.JogMinusBitOffset),
            ["ConditionSweepMotor"] = new(
                PlcHandle.CmdPolish.Motor.ConditionerSweep.AnyPosition,
                PlcHandle.CmdPolish.Motor.ConditionerSweep.Stop,
                PlcHandle.CmdPolish.Motor.ConditionerSweep.Param.Velocity.ParamHandle(),
                PlcHandle.CmdPolish.Motor.ConditionerSweep.Param.Position.ParamHandle(),
                PlcHandle.CmdPolish.Motor.ConditionerSweep.Address.ActualVelocityW,
                PlcHandle.CmdPolish.Motor.ConditionerSweep.Address.ActualPositionW,
                PlcHandle.CmdPolish.Motor.ConditionerSweep.Address.JogVelocityZf,
                PlcHandle.CmdPolish.Motor.ConditionerSweep.Address.JogBitWordZf,
                PlcHandle.CmdPolish.Motor.ConditionerSweep.Address.JogPlusBitOffset,
                PlcHandle.CmdPolish.Motor.ConditionerSweep.Address.JogMinusBitOffset),
            ["SlurrySweepMotor"] = new(
                PlcHandle.CmdPolish.Motor.SlurryArmSweep.AnyPosition,
                PlcHandle.CmdPolish.Motor.SlurryArmSweep.Stop,
                PlcHandle.CmdPolish.Motor.SlurryArmSweep.Param.Velocity.ParamHandle(),
                PlcHandle.CmdPolish.Motor.SlurryArmSweep.Param.Position.ParamHandle(),
                PlcHandle.CmdPolish.Motor.SlurryArmSweep.Address.ActualVelocityW,
                PlcHandle.CmdPolish.Motor.SlurryArmSweep.Address.ActualPositionW,
                PlcHandle.CmdPolish.Motor.SlurryArmSweep.Address.JogVelocityZf,
                PlcHandle.CmdPolish.Motor.SlurryArmSweep.Address.JogBitWordZf,
                PlcHandle.CmdPolish.Motor.SlurryArmSweep.Address.JogPlusBitOffset,
                PlcHandle.CmdPolish.Motor.SlurryArmSweep.Address.JogMinusBitOffset)
        };

    private readonly IEventAggregator _eventAggregator;
    private string? _tabTitle;
    private string? _baseChamberAddress;
    private readonly EditableFloatItem _platenSetVelocityItem = new($"ZF{PlcHandle.CmdPolish.Motor.PlatenRotation.Param.Velocity.ParamHandle()}", nameof(PlatenSetVelocity), nameof(IsPlatenSetVelocityDirty));
    private readonly EditableFloatItem _platenJogVelocityItem = new($"ZF{PlcHandle.CmdPolish.Motor.PlatenRotation.Address.JogVelocityZf}", nameof(PlatenJogVelocity), nameof(IsPlatenJogVelocityDirty));
    private readonly EditableFloatItem _headSetVelocityItem = new($"ZF{PlcHandle.CmdPolish.Motor.HeadRotation.Param.Velocity.ParamHandle()}", nameof(HeadSetVelocity), nameof(IsHeadSetVelocityDirty));
    private readonly EditableFloatItem _headJogVelocityItem = new($"ZF{PlcHandle.CmdPolish.Motor.HeadRotation.Address.JogVelocityZf}", nameof(HeadJogVelocity), nameof(IsHeadJogVelocityDirty));
    private readonly EditableFloatItem _conditionerSetVelocityItem = new($"ZF{PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.Param.Velocity.ParamHandle()}", nameof(ConditionerSetVelocity), nameof(IsConditionerSetVelocityDirty));
    private readonly EditableFloatItem _conditionerJogVelocityItem = new($"ZF{PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.Address.JogVelocityZf}", nameof(ConditionerJogVelocity), nameof(IsConditionerJogVelocityDirty));
    private readonly EditableFloatItem _headSweepSetVelocityItem = new($"ZF{PlcHandle.CmdPolish.Motor.HeadSweep.Param.Velocity.ParamHandle()}", nameof(HeadSweepSetVelocity), nameof(IsHeadSweepSetVelocityDirty));
    private readonly EditableFloatItem _headSweepSetPositionItem = new($"ZF{PlcHandle.CmdPolish.Motor.HeadSweep.Param.Position.ParamHandle()}", nameof(HeadSweepSetPosition), nameof(IsHeadSweepSetPositionDirty));
    private readonly EditableFloatItem _headSweepJogVelocityItem = new($"ZF{PlcHandle.CmdPolish.Motor.HeadSweep.Address.JogVelocityZf}", nameof(HeadSweepJogVelocity), nameof(IsHeadSweepJogVelocityDirty));
    private readonly EditableFloatItem _headLiftSetVelocityItem = new($"ZF{PlcHandle.CmdPolish.Motor.HeadUpDown.Param.Velocity.ParamHandle()}", nameof(HeadLiftSetVelocity), nameof(IsHeadLiftSetVelocityDirty));
    private readonly EditableFloatItem _headLiftSetPositionItem = new($"ZF{PlcHandle.CmdPolish.Motor.HeadUpDown.Param.Position.ParamHandle()}", nameof(HeadLiftSetPosition), nameof(IsHeadLiftSetPositionDirty));
    private readonly EditableFloatItem _headLiftJogVelocityItem = new($"ZF{PlcHandle.CmdPolish.Motor.HeadUpDown.Address.JogVelocityZf}", nameof(HeadLiftJogVelocity), nameof(IsHeadLiftJogVelocityDirty));
    private readonly EditableFloatItem _conditionerSweepSetVelocityItem = new($"ZF{PlcHandle.CmdPolish.Motor.ConditionerSweep.Param.Velocity.ParamHandle()}", nameof(ConditionerSweepSetVelocity), nameof(IsConditionerSweepSetVelocityDirty));
    private readonly EditableFloatItem _conditionerSweepSetPositionItem = new($"ZF{PlcHandle.CmdPolish.Motor.ConditionerSweep.Param.Position.ParamHandle()}", nameof(ConditionerSweepSetPosition), nameof(IsConditionerSweepSetPositionDirty));
    private readonly EditableFloatItem _conditionerSweepJogVelocityItem = new($"ZF{PlcHandle.CmdPolish.Motor.ConditionerSweep.Address.JogVelocityZf}", nameof(ConditionerSweepJogVelocity), nameof(IsConditionerSweepJogVelocityDirty));
    private readonly EditableFloatItem _slurrySweepSetVelocityItem = new($"ZF{PlcHandle.CmdPolish.Motor.SlurryArmSweep.Param.Velocity.ParamHandle()}", nameof(SlurrySweepSetVelocity), nameof(IsSlurrySweepSetVelocityDirty));
    private readonly EditableFloatItem _slurrySweepSetPositionItem = new($"ZF{PlcHandle.CmdPolish.Motor.SlurryArmSweep.Param.Position.ParamHandle()}", nameof(SlurrySweepSetPosition), nameof(IsSlurrySweepSetPositionDirty));
    private readonly EditableFloatItem _slurrySweepJogVelocityItem = new($"ZF{PlcHandle.CmdPolish.Motor.SlurryArmSweep.Address.JogVelocityZf}", nameof(SlurrySweepJogVelocity), nameof(IsSlurrySweepJogVelocityDirty));
    public new string? TabTitle
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

    /// <summary>
    /// 运行模式 0表示自动模式，1表示手动模式，默认自动模式
    /// </summary>
    private int _runMode = 0;

    private PolishUnit _polishUnit = new();

    public PolishUnit PolishUnit
    {
        get => _polishUnit;
        set => SetProperty(ref _polishUnit, value);
    }

    public double PlatenSetVelocity
    {
        get => _platenSetVelocityItem.Value;
        set => EditableFloatItemHelper.SetValue(_platenSetVelocityItem, value, RaisePropertyChanged);
    }

    public bool IsPlatenSetVelocityDirty => _platenSetVelocityItem.IsDirty;

    public double PlatenJogVelocity
    {
        get => _platenJogVelocityItem.Value;
        set => EditableFloatItemHelper.SetValue(_platenJogVelocityItem, value, RaisePropertyChanged);
    }

    public bool IsPlatenJogVelocityDirty => _platenJogVelocityItem.IsDirty;

    public double HeadSetVelocity
    {
        get => _headSetVelocityItem.Value;
        set => EditableFloatItemHelper.SetValue(_headSetVelocityItem, value, RaisePropertyChanged);
    }

    public bool IsHeadSetVelocityDirty => _headSetVelocityItem.IsDirty;

    public double HeadJogVelocity
    {
        get => _headJogVelocityItem.Value;
        set => EditableFloatItemHelper.SetValue(_headJogVelocityItem, value, RaisePropertyChanged);
    }

    public bool IsHeadJogVelocityDirty => _headJogVelocityItem.IsDirty;

    public double ConditionerSetVelocity
    {
        get => _conditionerSetVelocityItem.Value;
        set => EditableFloatItemHelper.SetValue(_conditionerSetVelocityItem, value, RaisePropertyChanged);
    }

    public bool IsConditionerSetVelocityDirty => _conditionerSetVelocityItem.IsDirty;

    public double ConditionerJogVelocity
    {
        get => _conditionerJogVelocityItem.Value;
        set => EditableFloatItemHelper.SetValue(_conditionerJogVelocityItem, value, RaisePropertyChanged);
    }

    public bool IsConditionerJogVelocityDirty => _conditionerJogVelocityItem.IsDirty;

    public double HeadSweepSetVelocity
    {
        get => _headSweepSetVelocityItem.Value;
        set => EditableFloatItemHelper.SetValue(_headSweepSetVelocityItem, value, RaisePropertyChanged);
    }

    public bool IsHeadSweepSetVelocityDirty => _headSweepSetVelocityItem.IsDirty;

    public double HeadSweepSetPosition
    {
        get => _headSweepSetPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_headSweepSetPositionItem, value, RaisePropertyChanged);
    }

    public bool IsHeadSweepSetPositionDirty => _headSweepSetPositionItem.IsDirty;

    public double HeadSweepJogVelocity
    {
        get => _headSweepJogVelocityItem.Value;
        set => EditableFloatItemHelper.SetValue(_headSweepJogVelocityItem, value, RaisePropertyChanged);
    }

    public bool IsHeadSweepJogVelocityDirty => _headSweepJogVelocityItem.IsDirty;

    public double HeadLiftSetVelocity
    {
        get => _headLiftSetVelocityItem.Value;
        set => EditableFloatItemHelper.SetValue(_headLiftSetVelocityItem, value, RaisePropertyChanged);
    }

    public bool IsHeadLiftSetVelocityDirty => _headLiftSetVelocityItem.IsDirty;

    public double HeadLiftSetPosition
    {
        get => _headLiftSetPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_headLiftSetPositionItem, value, RaisePropertyChanged);
    }

    public bool IsHeadLiftSetPositionDirty => _headLiftSetPositionItem.IsDirty;

    public double HeadLiftJogVelocity
    {
        get => _headLiftJogVelocityItem.Value;
        set => EditableFloatItemHelper.SetValue(_headLiftJogVelocityItem, value, RaisePropertyChanged);
    }

    public bool IsHeadLiftJogVelocityDirty => _headLiftJogVelocityItem.IsDirty;

    public double ConditionerSweepSetVelocity
    {
        get => _conditionerSweepSetVelocityItem.Value;
        set => EditableFloatItemHelper.SetValue(_conditionerSweepSetVelocityItem, value, RaisePropertyChanged);
    }

    public bool IsConditionerSweepSetVelocityDirty => _conditionerSweepSetVelocityItem.IsDirty;

    public double ConditionerSweepSetPosition
    {
        get => _conditionerSweepSetPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_conditionerSweepSetPositionItem, value, RaisePropertyChanged);
    }

    public bool IsConditionerSweepSetPositionDirty => _conditionerSweepSetPositionItem.IsDirty;

    public double ConditionerSweepJogVelocity
    {
        get => _conditionerSweepJogVelocityItem.Value;
        set => EditableFloatItemHelper.SetValue(_conditionerSweepJogVelocityItem, value, RaisePropertyChanged);
    }

    public bool IsConditionerSweepJogVelocityDirty => _conditionerSweepJogVelocityItem.IsDirty;

    public double SlurrySweepSetVelocity
    {
        get => _slurrySweepSetVelocityItem.Value;
        set => EditableFloatItemHelper.SetValue(_slurrySweepSetVelocityItem, value, RaisePropertyChanged);
    }

    public bool IsSlurrySweepSetVelocityDirty => _slurrySweepSetVelocityItem.IsDirty;

    public double SlurrySweepSetPosition
    {
        get => _slurrySweepSetPositionItem.Value;
        set => EditableFloatItemHelper.SetValue(_slurrySweepSetPositionItem, value, RaisePropertyChanged);
    }

    public bool IsSlurrySweepSetPositionDirty => _slurrySweepSetPositionItem.IsDirty;

    public double SlurrySweepJogVelocity
    {
        get => _slurrySweepJogVelocityItem.Value;
        set => EditableFloatItemHelper.SetValue(_slurrySweepJogVelocityItem, value, RaisePropertyChanged);
    }

    public bool IsSlurrySweepJogVelocityDirty => _slurrySweepJogVelocityItem.IsDirty;

    // Recipe lists for ComboBoxes 
    private ObservableCollection<string> _loadRecipes = new();

    public ObservableCollection<string> LoadRecipes
    {
        get => _loadRecipes;
        set => SetProperty(ref _loadRecipes, value);
    }

    private ObservableCollection<string> _conditionRecipes = new();

    public ObservableCollection<string> ConditionRecipes
    {
        get => _conditionRecipes;
        set => SetProperty(ref _conditionRecipes, value);
    }

    private ObservableCollection<string> _exsituConditionRecipes = new();

    /// <summary>ExsituCondition Recipe 文件夹下的配方（与 In-situ 的 ConditionRecipes 分开）。</summary>
    public ObservableCollection<string> ExsituConditionRecipes
    {
        get => _exsituConditionRecipes;
        set => SetProperty(ref _exsituConditionRecipes, value);
    }

    private ObservableCollection<string> _dechuckRecipes = new();

    public ObservableCollection<string> DechuckRecipes
    {
        get => _dechuckRecipes;
        set => SetProperty(ref _dechuckRecipes, value);
    }

    private ObservableCollection<string> _headSweepRecipes = new();

    public ObservableCollection<string> HeadSweepRecipes
    {
        get => _headSweepRecipes;
        set => SetProperty(ref _headSweepRecipes, value);
    }

    private ObservableCollection<string> _polishRecipes = new();

    public ObservableCollection<string> PolishRecipes
    {
        get => _polishRecipes;
        set => SetProperty(ref _polishRecipes, value);
    }

    private ObservableCollection<string> _unloadRecipes = new();

    public ObservableCollection<string> UnloadRecipes
    {
        get => _unloadRecipes;
        set => SetProperty(ref _unloadRecipes, value);
    }

    private ObservableCollection<string> _loadRetryRecipes = new();

    public ObservableCollection<string> LoadRetryRecipes
    {
        get => _loadRetryRecipes;
        set => SetProperty(ref _loadRetryRecipes, value);
    }

    private ObservableCollection<string> _dechuckRetryRecipes = new();

    public ObservableCollection<string> DechuckRetryRecipes
    {
        get => _dechuckRetryRecipes;
        set => SetProperty(ref _dechuckRetryRecipes, value);
    }

    private ObservableCollection<string> _unloadRetryRecipes = new();

    public ObservableCollection<string> UnloadRetryRecipes
    {
        get => _unloadRetryRecipes;
        set => SetProperty(ref _unloadRetryRecipes, value);
    }

    // Selected recipes
    private string? _selectedLoadRecipe;

    public string? SelectedLoadRecipe
    {
        get => _selectedLoadRecipe;
        set => SetProperty(ref _selectedLoadRecipe, value);
    }

    private string? _selectedPreCondRecipe;

    public string? SelectedPreCondRecipe
    {
        get => _selectedPreCondRecipe;
        set => SetProperty(ref _selectedPreCondRecipe, value);
    }

    private string? _selectedExsituConditionRecipe;

    /// <summary>ExsituCondition Recipe 下拉框选中项（BreakIn 执行用）。</summary>
    public string? SelectedExsituConditionRecipe
    {
        get => _selectedExsituConditionRecipe;
        set => SetProperty(ref _selectedExsituConditionRecipe, value);
    }

    private string? _selectedDechuckRecipe;

    public string? SelectedDechuckRecipe
    {
        get => _selectedDechuckRecipe;
        set => SetProperty(ref _selectedDechuckRecipe, value);
    }

    private string? _selectedPolishRecipe;

    public string? SelectedPolishRecipe
    {
        get => _selectedPolishRecipe;
        set => SetProperty(ref _selectedPolishRecipe, value);
    }

    private string? _selectedUnloadRecipe;

    public string? SelectedUnloadRecipe
    {
        get => _selectedUnloadRecipe;
        set => SetProperty(ref _selectedUnloadRecipe, value);
    }

    private string? _selectedLoadRetryRecipe;

    public string? SelectedLoadRetryRecipe
    {
        get => _selectedLoadRetryRecipe;
        set => SetProperty(ref _selectedLoadRetryRecipe, value);
    }

    private string? _selectedDechuckRetryRecipe;

    public string? SelectedDechuckRetryRecipe
    {
        get => _selectedDechuckRetryRecipe;
        set => SetProperty(ref _selectedDechuckRetryRecipe, value);
    }

    private string? _selectedUnloadRetryRecipe;

    public string? SelectedUnloadRetryRecipe
    {
        get => _selectedUnloadRetryRecipe;
        set => SetProperty(ref _selectedUnloadRetryRecipe, value);
    }

    // 电机位置选择（Combobox 可编辑：可选择预设位置，也可输入自定义数值）
    public List<string> HeadSweepMotorPositionOptions { get; } = ["Home", "PolishNear", "PolishFar", "Dechuck"];

    private string _selectedHeadSweepMotorPosition = "Home";

    public string SelectedHeadSweepMotorPosition
    {
        get => _selectedHeadSweepMotorPosition;
        set => SetProperty(ref _selectedHeadSweepMotorPosition, value);
    }

    public List<string> HeadLiftMotorPositionOptions { get; } = ["High", "LoadCup", "Polish", "Clean"];

    private string _selectedHeadLiftMotorPosition = "High";

    public string SelectedHeadLiftMotorPosition
    {
        get => _selectedHeadLiftMotorPosition;
        set => SetProperty(ref _selectedHeadLiftMotorPosition, value);
    }

    public List<string> ConditionSweepMotorPositionOptions { get; } = ["Home", "Start", "End"];

    private string _selectedConditionSweepMotorPosition = "Home";

    public string SelectedConditionSweepMotorPosition
    {
        get => _selectedConditionSweepMotorPosition;
        set => SetProperty(ref _selectedConditionSweepMotorPosition, value);
    }

    public List<string> SlurrySweepMotorPositionOptions { get; } = ["Home", "Slurry"];

    private string _selectedSlurrySweepMotorPosition = "Home";

    public string SelectedSlurrySweepMotorPosition
    {
        get => _selectedSlurrySweepMotorPosition;
        set => SetProperty(ref _selectedSlurrySweepMotorPosition, value);
    }

    private string _configLoadRecipe = "";

    private string _configUnLoadRecipe = "";

    private string _configDechukRecipe = "";

    private string _configLoadRetryRecipe = "";

    private string _configDechuckRetryRecipe = "";

    private string _configUnloadRetryRecipe = "";

    public PolishSlurryContentViewModel SlurryControl { get; }
    public PolishSwingPositionsViewModel SwingPositions { get; }

    public DelegateCommand<string> AutoCommand { get; set; }
    public DelegateCommand<string> AbortUnitCommand { get; set; }
    public DelegateCommand<string> AbortCommand { get; set; }
    public DelegateCommand<string> StartCommand { get; set; }
    public DelegateCommand<string> RunCommand { get; set; }
    public DelegateCommand<string> RotaryMotorStartCommand { get; set; }
    public DelegateCommand<string> RotaryMotorStopCommand { get; set; }
    public DelegateCommand<string> SwingMotorStartCommand { get; set; }
    public DelegateCommand<string> SwingMotorStopCommand { get; set; }
    public DelegateCommand<string> JogPressCommand { get; set; }
    public DelegateCommand<string> JogReleaseCommand { get; set; }
    public DelegateCommand PlatenSetVelocitySetCommand => _platenSetVelocityItem.SetCommand;
    public DelegateCommand PlatenSetVelocityCancelCommand => _platenSetVelocityItem.CancelCommand;
    public DelegateCommand PlatenJogVelocitySetCommand => _platenJogVelocityItem.SetCommand;
    public DelegateCommand PlatenJogVelocityCancelCommand => _platenJogVelocityItem.CancelCommand;
    public DelegateCommand HeadSetVelocitySetCommand => _headSetVelocityItem.SetCommand;
    public DelegateCommand HeadSetVelocityCancelCommand => _headSetVelocityItem.CancelCommand;
    public DelegateCommand HeadJogVelocitySetCommand => _headJogVelocityItem.SetCommand;
    public DelegateCommand HeadJogVelocityCancelCommand => _headJogVelocityItem.CancelCommand;
    public DelegateCommand ConditionerSetVelocitySetCommand => _conditionerSetVelocityItem.SetCommand;
    public DelegateCommand ConditionerSetVelocityCancelCommand => _conditionerSetVelocityItem.CancelCommand;
    public DelegateCommand ConditionerJogVelocitySetCommand => _conditionerJogVelocityItem.SetCommand;
    public DelegateCommand ConditionerJogVelocityCancelCommand => _conditionerJogVelocityItem.CancelCommand;
    public DelegateCommand HeadSweepSetVelocitySetCommand => _headSweepSetVelocityItem.SetCommand;
    public DelegateCommand HeadSweepSetVelocityCancelCommand => _headSweepSetVelocityItem.CancelCommand;
    public DelegateCommand HeadSweepSetPositionSetCommand => _headSweepSetPositionItem.SetCommand;
    public DelegateCommand HeadSweepSetPositionCancelCommand => _headSweepSetPositionItem.CancelCommand;
    public DelegateCommand HeadSweepJogVelocitySetCommand => _headSweepJogVelocityItem.SetCommand;
    public DelegateCommand HeadSweepJogVelocityCancelCommand => _headSweepJogVelocityItem.CancelCommand;
    public DelegateCommand HeadLiftSetVelocitySetCommand => _headLiftSetVelocityItem.SetCommand;
    public DelegateCommand HeadLiftSetVelocityCancelCommand => _headLiftSetVelocityItem.CancelCommand;
    public DelegateCommand HeadLiftSetPositionSetCommand => _headLiftSetPositionItem.SetCommand;
    public DelegateCommand HeadLiftSetPositionCancelCommand => _headLiftSetPositionItem.CancelCommand;
    public DelegateCommand HeadLiftJogVelocitySetCommand => _headLiftJogVelocityItem.SetCommand;
    public DelegateCommand HeadLiftJogVelocityCancelCommand => _headLiftJogVelocityItem.CancelCommand;
    public DelegateCommand ConditionerSweepSetVelocitySetCommand => _conditionerSweepSetVelocityItem.SetCommand;
    public DelegateCommand ConditionerSweepSetVelocityCancelCommand => _conditionerSweepSetVelocityItem.CancelCommand;
    public DelegateCommand ConditionerSweepSetPositionSetCommand => _conditionerSweepSetPositionItem.SetCommand;
    public DelegateCommand ConditionerSweepSetPositionCancelCommand => _conditionerSweepSetPositionItem.CancelCommand;
    public DelegateCommand ConditionerSweepJogVelocitySetCommand => _conditionerSweepJogVelocityItem.SetCommand;
    public DelegateCommand ConditionerSweepJogVelocityCancelCommand => _conditionerSweepJogVelocityItem.CancelCommand;
    public DelegateCommand SlurrySweepSetVelocitySetCommand => _slurrySweepSetVelocityItem.SetCommand;
    public DelegateCommand SlurrySweepSetVelocityCancelCommand => _slurrySweepSetVelocityItem.CancelCommand;
    public DelegateCommand SlurrySweepSetPositionSetCommand => _slurrySweepSetPositionItem.SetCommand;
    public DelegateCommand SlurrySweepSetPositionCancelCommand => _slurrySweepSetPositionItem.CancelCommand;
    public DelegateCommand SlurrySweepJogVelocitySetCommand => _slurrySweepJogVelocityItem.SetCommand;
    public DelegateCommand SlurrySweepJogVelocityCancelCommand => _slurrySweepJogVelocityItem.CancelCommand;
    public DelegateCommand<string> PressCommand { get; set; }
    public DelegateCommand<string> VentCommand { get; set; }
    public DelegateCommand<string> VacuumCommand { get; set; }
    public DelegateCommand<string> UpCommand { get; set; }
    public DelegateCommand<string> DownCommand { get; set; }
    public DelegateCommand<string> SetCommand { get; set; }
    public DelegateCommand<string> ResetCommand { get; set; }
    public DelegateCommand<string> ServoCommand { get; set; }
    public DelegateCommand<string>? LoadCommand { get; set; }
    public DelegateCommand<string>? PolishCommand { get; set; }
    public DelegateCommand<string>? InitializeCommand { get; set; }
    public DelegateCommand<string>? AutoModeCommand { get; set; }
    public DelegateCommand<string>? ManualModeCommand { get; set; }
    public DelegateCommand<string>? AutoRunCommand { get; set; }
    public DelegateCommand<string>? ExsituConditionCommand { get; set; }
    public DelegateCommand<string>? UnloadCommand { get; set; }
    public DelegateCommand<string>? DechuckCommand { get; set; }
    public DelegateCommand<string>? PositionCommand { get; set; }

    // Servo button text properties
    public string PlatenMotorServoText => PolishUnit.PlatenMotor.IsServoOn ? GetMotorText("Off") : GetMotorText("On");
    public string HeadRotateMotorServoText => PolishUnit.HeadRotateMotor.IsServoOn ? GetMotorText("Off") : GetMotorText("On");
    public string HeadSweepMotorServoText => PolishUnit.HeadSweepMotor.IsServoOn ? GetMotorText("Off") : GetMotorText("On");
    public string HeadLiftMotorServoText => PolishUnit.HeadLiftMotor.IsServoOn ? GetMotorText("Off") : GetMotorText("On");
    public string DiskRotateMotorServoText => PolishUnit.DiskRotateMotor.IsServoOn ? GetMotorText("Off") : GetMotorText("On");

    public string ConditionSweepMotorServoText =>
        PolishUnit.ConditionSweepMotor.IsServoOn ? GetMotorText("Off") : GetMotorText("On");

    public string SlurrySweepMotorServoText =>
        PolishUnit.SlurrySweepMotor.IsServoOn ? GetMotorText("Off") : GetMotorText("On");


    int err = (int)KHLUserError.KHL_ERR_NONE;
    KVLink kvLink;
    byte[] writeBuf = new byte[65535];
    byte[] readBuf = new byte[65535];
    byte[] writeTrue = { 1 };
    byte[] writeFalse = { 0 };

    int delayCycle = 0;
    bool isPLCAutoStartNeedFalse = false;

    /// <summary>
    /// 是否重置Recipe状态
    /// </summary>
    bool isResetRecipeState = false;

    /// <summary>
    /// "D:\CMP\Configuration\Recipes\UnitRecipe\"
    /// </summary>
    private readonly string _recipeBasePath = "D:\\CMP\\Configuration\\Recipes\\UnitRecipe\\";

    Task thMonitor;

    Polish polishData;

    int waferIndex;
    //public PolishUnitTabContentViewModel(int id, string tabTitle, IEventAggregator eventAggregator, 
    //    string plcAddr = "192.168.2.20", ushort plcPort = 8500) : base(id)
    //{
    //    _eventAggregator = eventAggregator;
    //    _plcAddr = plcAddr;
    //    _plcPort = plcPort;
    //    TabId = id;
    //    TabTitle = tabTitle;

    //    AutoCommand = new DelegateCommand<string>(AutoCommandExecute);
    //    AbortCommand = new DelegateCommand<string>(AbortCommandExecute);
    //    StartCommand = new DelegateCommand<string>(StartCommandExecute);
    //    RunCommand = new DelegateCommand<string>(RunCommandExecute);
    //    PressCommand = new DelegateCommand<string>(PressCommandExecute);
    //    VentCommand = new DelegateCommand<string>(VentCommandExecute);
    //    VacuumCommand = new DelegateCommand<string>(VacuumCommandExecute);
    //    ServoCommand = new DelegateCommand<string>(ServoCommandExecute);
    //    LoadCommand = new DelegateCommand<string>(LoadCommandExecute);
    //    PolishCommand = new DelegateCommand<string>(PolishCommandExecute);
    //    InitializeCommand = new DelegateCommand<string>(InitializeCommandExecute);
    //    AutoModeCommand = new DelegateCommand<string>(AutoModeCommandExecute);
    //    ManualModeCommand = new DelegateCommand<string>(ManualModeCommandExecute);
    //    ConditionCommand = new DelegateCommand<string>(ConditionCommandExecute);
    //    UnloadCommand = new DelegateCommand<string>(UnloadCommandExecute);
    //    DechuckCommand = new DelegateCommand<string>(DechuckCommandExecute);

    //    err = (int)KHLUserError.KHL_ERR_NONE;
    //    sock = 0;
    //        err = KHL.KHLInit();
    //        if (err != (int)KHLUserError.KHL_ERR_NONE)
    //        {
    //        }
    //        err = KHL.KHLConnect(_plcAddr, _plcPort, 3000, KHLSockType.SOCK_TCP, ref sock);
    //        if (err != (int)KHLUserError.KHL_ERR_NONE)
    //        {
    //        }
    //    byte[] writeTrue ={ 1 };
    //    err = KHL.KHLWriteVariable(sock, "PLCStartPermition", writeTrue, true);
    //    err = KHL.KHLWriteVariable(sock, "PLCReady", writeTrue, true);
    //    err = KHL.KHLWriteVariable(sock, "PLCAutoStart", writeTrue, true);
    //    isPLCAutoStartNeedFalse = true;
    //    //err = KHL.KHLConnect(_plcAddr, _plcPort, 3000, KHLSockType.SOCK_TCP, ref sock);
    //    //if (err != (int)KHLUserError.KHL_ERR_NONE)
    //    //{
    //    //}

    //    // Load recipes
    //    LoadRecipesData();

    //    // 订阅Recipe保存事件，当Recipe编辑器保存时自动重新加载
    //    _eventAggregator.GetEvent<RecipeSavedEvent>().Subscribe(OnRecipeSaved, ThreadOption.UIThread);

    //        thMonitor = new Thread(Monitor);
    //        thMonitor.Start();
    //        thMonitor.IsBackground = true;
    //}

    public PolishUnitTabContentViewModel(int id, KVLink link, string tabTitle, IEventAggregator eventAggregator) :
        base(id)
    {
        _eventAggregator = eventAggregator;
        TabId = id;
        TabTitle = tabTitle;
        isPLCAutoStartNeedFalse = true;
        _baseChamberAddress = "ZF89000";
        // 订阅Recipe保存事件，当Recipe编辑器保存时自动重新加载
        _eventAggregator.GetEvent<RecipeSavedEvent>().Subscribe(OnRecipeSaved, ThreadOption.UIThread);
        _eventAggregator.GetEvent<GetRecipeConfigEvent>().Subscribe(OnGetRecipeConfig, ThreadOption.UIThread);

        //_eventAggregator.GetEvent<PubSubEvent>().Subscribe(OnPolishSocketsChanged);

        AutoCommand = OperationAudit.Create<string>("Auto", AutoCommandExecute, () => TabTitle ?? string.Empty);
        AbortUnitCommand = OperationAudit.Create<string>("AbortUnit", AbortUnitCommandExecute, () => TabTitle ?? string.Empty);
        AbortCommand = OperationAudit.Create<string>("Abort", AbortCommandExecute, () => TabTitle ?? string.Empty);
        StartCommand = OperationAudit.Create<string>("Start", StartCommandExecute, () => TabTitle ?? string.Empty);
        RunCommand = OperationAudit.Create<string>("Run", RunCommandExecute, () => TabTitle ?? string.Empty);
        RotaryMotorStartCommand = OperationAudit.Create<string>("Run", RotaryMotorStartCommandExecute, () => TabTitle ?? string.Empty);
        RotaryMotorStopCommand = OperationAudit.Create<string>("Abort", RotaryMotorStopCommandExecute, () => TabTitle ?? string.Empty);
        SwingMotorStartCommand = OperationAudit.Create<string>("Run", SwingMotorStartCommandExecute, () => TabTitle ?? string.Empty);
        SwingMotorStopCommand = OperationAudit.Create<string>("Abort", SwingMotorStopCommandExecute, () => TabTitle ?? string.Empty);
        InitializeRotaryMotorEditableItem(_platenSetVelocityItem, "PlatenMotor", "SetVelocity");
        InitializeRotaryMotorEditableItem(_platenJogVelocityItem, "PlatenMotor", "JogVelocity");
        InitializeRotaryMotorEditableItem(_headSetVelocityItem, "HeadRotateMotor", "SetVelocity");
        InitializeRotaryMotorEditableItem(_headJogVelocityItem, "HeadRotateMotor", "JogVelocity");
        InitializeRotaryMotorEditableItem(_conditionerSetVelocityItem, "DiskRotateMotor", "SetVelocity");
        InitializeRotaryMotorEditableItem(_conditionerJogVelocityItem, "DiskRotateMotor", "JogVelocity");
        InitializeSwingMotorEditableItem(_headSweepSetVelocityItem, "HeadSweepMotor", "SetVelocity");
        InitializeSwingMotorEditableItem(_headSweepSetPositionItem, "HeadSweepMotor", "SetPosition");
        InitializeSwingMotorEditableItem(_headSweepJogVelocityItem, "HeadSweepMotor", "JogVelocity");
        InitializeSwingMotorEditableItem(_headLiftSetVelocityItem, "HeadLiftMotor", "SetVelocity");
        InitializeSwingMotorEditableItem(_headLiftSetPositionItem, "HeadLiftMotor", "SetPosition");
        InitializeSwingMotorEditableItem(_headLiftJogVelocityItem, "HeadLiftMotor", "JogVelocity");
        InitializeSwingMotorEditableItem(_conditionerSweepSetVelocityItem, "ConditionSweepMotor", "SetVelocity");
        InitializeSwingMotorEditableItem(_conditionerSweepSetPositionItem, "ConditionSweepMotor", "SetPosition");
        InitializeSwingMotorEditableItem(_conditionerSweepJogVelocityItem, "ConditionSweepMotor", "JogVelocity");
        InitializeSwingMotorEditableItem(_slurrySweepSetVelocityItem, "SlurrySweepMotor", "SetVelocity");
        InitializeSwingMotorEditableItem(_slurrySweepSetPositionItem, "SlurrySweepMotor", "SetPosition");
        InitializeSwingMotorEditableItem(_slurrySweepJogVelocityItem, "SlurrySweepMotor", "JogVelocity");
        JogPressCommand = new DelegateCommand<string>(JogPressCommandExecute);
        JogReleaseCommand = new DelegateCommand<string>(JogReleaseCommandExecute);
        PressCommand = OperationAudit.Create<string>("Press", PressCommandExecute, () => TabTitle ?? string.Empty);
        VentCommand = OperationAudit.Create<string>("Vent", VentCommandExecute, () => TabTitle ?? string.Empty);
        VacuumCommand = OperationAudit.Create<string>("Vacuum", VacuumCommandExecute, () => TabTitle ?? string.Empty);
        UpCommand = OperationAudit.Create<string>("Up", UpCommandExecute, () => TabTitle ?? string.Empty);
        DownCommand = OperationAudit.Create<string>("Down", DownCommandExecute, () => TabTitle ?? string.Empty);
        SetCommand = OperationAudit.Create<string>("Set", SetCommandExecute, () => TabTitle ?? string.Empty);
        ResetCommand = OperationAudit.Create<string>("Reset", ResetCommandExecute, () => TabTitle ?? string.Empty);
        ServoCommand = OperationAudit.Create<string>("Servo", ServoCommandExecute, () => TabTitle ?? string.Empty);
        LoadCommand = OperationAudit.Create<string>("Load", LoadCommandExecute, () => TabTitle ?? string.Empty);
        PolishCommand = OperationAudit.Create<string>("Polish", PolishCommandExecute, () => TabTitle ?? string.Empty);
        InitializeCommand = OperationAudit.Create<string>("Initialize", InitializeCommandExecute, () => TabTitle ?? string.Empty);
        AutoModeCommand = OperationAudit.Create<string>("AutoMode", AutoModeCommandExecute, () => TabTitle ?? string.Empty);
        ManualModeCommand = OperationAudit.Create<string>("ManualMode", ManualModeCommandExecute, () => TabTitle ?? string.Empty);
        AutoRunCommand = OperationAudit.Create<string>("AutoRun", AutoRunCommandExecute, () => TabTitle ?? string.Empty);
        ExsituConditionCommand = OperationAudit.Create<string>("BreakIn", BreakInCommandExecute, () => TabTitle ?? string.Empty);
        UnloadCommand = OperationAudit.Create<string>("Unload", UnloadCommandExecute, () => TabTitle ?? string.Empty);
        DechuckCommand = OperationAudit.Create<string>("Dechuck", DechuckCommandExecute, () => TabTitle ?? string.Empty);
        PositionCommand = OperationAudit.Create<string>("Position", PositionCommandExecute, () => TabTitle ?? string.Empty);


        kvLink = link;
        SlurryControl = new PolishSlurryContentViewModel(() => kvLink.sock, () => TabTitle ?? string.Empty);
        SwingPositions = new PolishSwingPositionsViewModel(() => kvLink.sock, () => TabTitle ?? string.Empty, PolishUnit);
        PropertyChangedEventManager.AddHandler(LocalizeDictionary.Instance, OnCultureChanged,
            nameof(LocalizeDictionary.Instance.Culture));
        err = KHL.KHLWriteVariable(kvLink.sock, "PLCStartPermition", writeTrue, true);
        err = KHL.KHLWriteVariable(kvLink.sock, "PLCReady", writeTrue, true);
        //err = KHL.KHLWriteVariable(sock, "PLCAutoStart", writeTrue, true);

        LoadRecipesData();

        thMonitor = new Task(Monitor);
        thMonitor.Start();

        polishData = GlobalService.DataCenter.listPolish[id];
    }

    /// <summary>
    /// 从配置中获取已经设置好的RecipeName
    /// </summary>
    /// <param name="args"></param>
    private void OnGetRecipeConfig(GetRecipeConfigEventArgs args)
    {
        try
        {
            if (args == null || string.IsNullOrEmpty(args.RecipeType))
                return;

            string workStationName = "";
            if (args.ConfigFileName != null && args.ConfigFileName != "")
            {
                switch (TabId)
                {
                    case 0:
                        workStationName = "PolishA";
                        break;
                    case 1:
                        workStationName = "PolishB";
                        break;
                    case 2:
                        workStationName = "PolishC";
                        break;
                    case 3:
                        workStationName = "PolishD";
                        break;
                    default:
                        break;
                }
                if (!(bool)args.ConfigFileName.StartsWith(workStationName))
                {
                    //修改的不是当前polish工位就不需要更新
                    return;
                }
            }
            bool isSuccess = false;
            switch (args.RecipeType)
            {
                case RecipeTemplateRegistry.LoadRecipeFolder:
                    LoadRecipesFromFolder(RecipeTemplateRegistry.LoadRecipeFolder, LoadRecipes);
                    if (!string.IsNullOrEmpty(args.RecipeName) && LoadRecipes.Contains(args.RecipeName))
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            _configLoadRecipe = args.RecipeName; 
                            SelectedLoadRecipe = args.RecipeName;
                        });
                        isSuccess = true;
                    }

                    break;                               
                case RecipeTemplateRegistry.UnloadRecipeFolder:
                    LoadRecipesFromFolder(RecipeTemplateRegistry.UnloadRecipeFolder, UnloadRecipes);
                    if (!string.IsNullOrEmpty(args.RecipeName) && UnloadRecipes.Contains(args.RecipeName))
                    {
                       Application.Current.Dispatcher.Invoke(() => {  
                           _configUnLoadRecipe = args.RecipeName; 
                           SelectedUnloadRecipe = args.RecipeName;
                       });
                        isSuccess = true;
                    }
                    break;
                case RecipeTemplateRegistry.DechuckRecipeFolder:
                    LoadRecipesFromFolder(RecipeTemplateRegistry.DechuckRecipeFolder, DechuckRecipes);
                    if (!string.IsNullOrEmpty(args.RecipeName) && DechuckRecipes.Contains(args.RecipeName))
                    {
                        Application.Current.Dispatcher.Invoke(() => { 
                            _configDechukRecipe = args.RecipeName; 
                            SelectedDechuckRecipe = args.RecipeName; 
                        });
                        isSuccess = true;
                    }
                    break;
                case RecipeTemplateRegistry.LoadRetryRecipeFolder:
                    LoadRecipesFromFolder(RecipeTemplateRegistry.LoadRetryRecipeFolder, LoadRetryRecipes);
                    if (!string.IsNullOrEmpty(args.RecipeName) && LoadRetryRecipes.Contains(args.RecipeName))
                    {
                        Application.Current.Dispatcher.Invoke(() => { 
                            SelectedLoadRetryRecipe = args.RecipeName;
                        });
                        isSuccess = true;
                    }
                    break;
                case RecipeTemplateRegistry.DechuckRetryRecipeFolder:
                    LoadRecipesFromFolder(RecipeTemplateRegistry.DechuckRetryRecipeFolder, DechuckRetryRecipes);
                    if (!string.IsNullOrEmpty(args.RecipeName) && DechuckRetryRecipes.Contains(args.RecipeName))
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            SelectedDechuckRetryRecipe = args.RecipeName;
                        });
                        isSuccess = true;
                    }
                    break;
                case RecipeTemplateRegistry.UnloadRetryRecipeFolder:
                    LoadRecipesFromFolder(RecipeTemplateRegistry.UnloadRetryRecipeFolder, UnloadRetryRecipes);
                    if (!string.IsNullOrEmpty(args.RecipeName) && UnloadRetryRecipes.Contains(args.RecipeName))
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            SelectedUnloadRetryRecipe = args.RecipeName;
                        });
                        isSuccess = true;
                    }
                    break;
            }
            if (isSuccess)
            {
                if (args.IsNeedTip)
                {
                    Growl.SuccessGlobal($"获取到Recipe配置参数:成功更新到工作台{workStationName} - Recipe类型：{args.RecipeType} - 文件名：{args.RecipeName}");
                }                
            }
            else
            {
                Growl.WarningGlobal($"获取到Recipe配置参数:未能更新到工作台{workStationName} - Recipe类型：{args.RecipeType} - 文件名：{args.RecipeName}");
            }
            Logger.Info($"获取到Recipe配置参数:Recipe类型：{args.RecipeType},文件名：{args.RecipeName},工作台 {workStationName}, 是否完成更新: {isSuccess}，是否提示{args.IsNeedTip}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to get recipes config : {ex.Message}", ex);
        }
    }

    private async void Monitor()
    {
        //Thread.Sleep(2000);
        //防止界面还未完全加载完成就开始和PLC通讯导致的异常，等界面完全加载后再开始通讯
        await Task.Delay(2000).ConfigureAwait(false);
        const int delay = 500;
        var disp = Application.Current?.Dispatcher;
        while (true)
        {
            try {
                //每次循环都先等待，避免过于频繁地与PLC通讯
                await Task.Delay(delay).ConfigureAwait(false);
                if (isPLCAutoStartNeedFalse)
                {
                    delayCycle++;
                    if (delayCycle >= 4)
                    {
                        err = KHL.KHLWriteVariable(kvLink.sock, "PLCAutoStart", writeFalse, true);
                        delayCycle = 0;
                        isPLCAutoStartNeedFalse = false;
                    }
                }
                IntereactionWithPLCForAssignRecipe();
                var rotaryMotorFeedback = ReadRotaryMotorFeedback();
                var swingMotorFeedback = ReadSwingMotorFeedback();
                var swingPositionFeedback = SwingPositions.ReadFeedback();

                if (disp != null)
                    await disp.InvokeAsync(() =>
                    {
                        RefreshHMI();
                        ApplyRotaryMotorFeedback(rotaryMotorFeedback);
                        ApplySwingMotorFeedback(swingMotorFeedback);
                        SwingPositions.ApplyFeedback(swingPositionFeedback);
                    });
                else
                {
                    RefreshHMI();
                    ApplyRotaryMotorFeedback(rotaryMotorFeedback);
                    ApplySwingMotorFeedback(swingMotorFeedback);
                    SwingPositions.ApplyFeedback(swingPositionFeedback);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Polish monitor loop error in {TabTitle}", ex);
            }

        }
    }

    private RotaryMotorFeedback? ReadRotaryMotorFeedback()
    {
        if (kvLink.sock <= 0)
            return null;

        float? ReadFloat(KHLDevType device, uint address)
        {
            var data = new byte[4];
            var readError = KHL.KHLReadDevicesAsWords(kvLink.sock, device, address, 2, data);
            return readError == (int)KHLUserError.KHL_ERR_NONE ? BitConverter.ToSingle(data, 0) : null;
        }

        try
        {
            var platen = RotaryMotorPlcAddressMap["PlatenMotor"];
            var head = RotaryMotorPlcAddressMap["HeadRotateMotor"];
            var conditioner = RotaryMotorPlcAddressMap["DiskRotateMotor"];

            return new RotaryMotorFeedback(
                ReadFloat(KHLDevType.DEV_ZF, platen.SetVelocity),
                ReadFloat(KHLDevType.DEV_W, platen.ConfirmedVelocity),
                ReadFloat(KHLDevType.DEV_W, platen.ActualVelocity),
                platen.ActualPosition.HasValue ? ReadFloat(KHLDevType.DEV_W, platen.ActualPosition.Value) : null,
                ReadFloat(KHLDevType.DEV_ZF, platen.JogVelocity),
                ReadFloat(KHLDevType.DEV_ZF, head.SetVelocity),
                ReadFloat(KHLDevType.DEV_W, head.ConfirmedVelocity),
                ReadFloat(KHLDevType.DEV_W, head.ActualVelocity),
                ReadFloat(KHLDevType.DEV_ZF, head.JogVelocity),
                ReadFloat(KHLDevType.DEV_ZF, conditioner.SetVelocity),
                ReadFloat(KHLDevType.DEV_W, conditioner.ConfirmedVelocity),
                ReadFloat(KHLDevType.DEV_W, conditioner.ActualVelocity),
                ReadFloat(KHLDevType.DEV_ZF, conditioner.JogVelocity));
        }
        catch (Exception ex)
        {
            Logger.For("RotaryMotor").Debug($"Read rotary motor feedback failed: {ex.Message}");
            return null;
        }
    }

    private void ApplyRotaryMotorFeedback(RotaryMotorFeedback? feedback)
    {
        if (feedback == null)
            return;

        SyncRotaryMotorParameter(_platenSetVelocityItem, feedback.PlatenSetVelocity, PolishUnit.PlatenMotor, "SetVelocity");
        SyncRotaryMotorParameter(_platenJogVelocityItem, feedback.PlatenJogVelocity, PolishUnit.PlatenMotor, "JogVelocity");
        SyncRotaryMotorParameter(_headSetVelocityItem, feedback.HeadSetVelocity, PolishUnit.HeadRotateMotor, "SetVelocity");
        SyncRotaryMotorParameter(_headJogVelocityItem, feedback.HeadJogVelocity, PolishUnit.HeadRotateMotor, "JogVelocity");
        SyncRotaryMotorParameter(_conditionerSetVelocityItem, feedback.ConditionerSetVelocity, PolishUnit.DiskRotateMotor, "SetVelocity");
        SyncRotaryMotorParameter(_conditionerJogVelocityItem, feedback.ConditionerJogVelocity, PolishUnit.DiskRotateMotor, "JogVelocity");

        ApplyValue(feedback.PlatenConfirmedVelocity, value => PolishUnit.PlatenMotor.ConfirmedVelocity = value);
        ApplyValue(feedback.PlatenActualVelocity, value => PolishUnit.PlatenMotor.ActVelocity = value);
        ApplyValue(feedback.PlatenActualPosition, value => PolishUnit.PlatenMotor.ActPosition = value);
        ApplyValue(feedback.HeadConfirmedVelocity, value => PolishUnit.HeadRotateMotor.ConfirmedVelocity = value);
        ApplyValue(feedback.HeadActualVelocity, value => PolishUnit.HeadRotateMotor.ActVelocity = value);
        ApplyValue(feedback.ConditionerConfirmedVelocity, value => PolishUnit.DiskRotateMotor.ConfirmedVelocity = value);
        ApplyValue(feedback.ConditionerActualVelocity, value => PolishUnit.DiskRotateMotor.ActVelocity = value);
    }

    private SwingMotorFeedback? ReadSwingMotorFeedback()
    {
        if (kvLink.sock <= 0)
            return null;

        float? ReadFloat(KHLDevType device, uint address)
        {
            var data = new byte[4];
            var readError = KHL.KHLReadDevicesAsWords(kvLink.sock, device, address, 2, data);
            return readError == (int)KHLUserError.KHL_ERR_NONE ? BitConverter.ToSingle(data, 0) : null;
        }

        try
        {
            var headSweep = SwingMotorPlcAddressMap["HeadSweepMotor"];
            var headLift = SwingMotorPlcAddressMap["HeadLiftMotor"];
            var conditionerSweep = SwingMotorPlcAddressMap["ConditionSweepMotor"];
            var slurrySweep = SwingMotorPlcAddressMap["SlurrySweepMotor"];

            return new SwingMotorFeedback(
                ReadFloat(KHLDevType.DEV_ZF, headSweep.SetVelocity),
                ReadFloat(KHLDevType.DEV_ZF, headSweep.SetPosition),
                ReadFloat(KHLDevType.DEV_W, headSweep.ActualVelocity),
                ReadFloat(KHLDevType.DEV_W, headSweep.ActualPosition),
                ReadFloat(KHLDevType.DEV_ZF, headSweep.JogVelocity),
                ReadFloat(KHLDevType.DEV_ZF, headLift.SetVelocity),
                ReadFloat(KHLDevType.DEV_ZF, headLift.SetPosition),
                ReadFloat(KHLDevType.DEV_W, headLift.ActualVelocity),
                ReadFloat(KHLDevType.DEV_W, headLift.ActualPosition),
                ReadFloat(KHLDevType.DEV_ZF, headLift.JogVelocity),
                ReadFloat(KHLDevType.DEV_ZF, conditionerSweep.SetVelocity),
                ReadFloat(KHLDevType.DEV_ZF, conditionerSweep.SetPosition),
                ReadFloat(KHLDevType.DEV_W, conditionerSweep.ActualVelocity),
                ReadFloat(KHLDevType.DEV_W, conditionerSweep.ActualPosition),
                ReadFloat(KHLDevType.DEV_ZF, conditionerSweep.JogVelocity),
                ReadFloat(KHLDevType.DEV_ZF, slurrySweep.SetVelocity),
                ReadFloat(KHLDevType.DEV_ZF, slurrySweep.SetPosition),
                ReadFloat(KHLDevType.DEV_W, slurrySweep.ActualVelocity),
                ReadFloat(KHLDevType.DEV_W, slurrySweep.ActualPosition),
                ReadFloat(KHLDevType.DEV_ZF, slurrySweep.JogVelocity));
        }
        catch (Exception ex)
        {
            Logger.For("SwingMotor").Debug($"Read swing motor feedback failed: {ex.Message}");
            return null;
        }
    }

    private void ApplySwingMotorFeedback(SwingMotorFeedback? feedback)
    {
        if (feedback == null)
            return;

        SyncSwingMotorParameter(_headSweepSetVelocityItem, feedback.HeadSweepSetVelocity, PolishUnit.HeadSweepMotor, "SetVelocity");
        SyncSwingMotorParameter(_headSweepSetPositionItem, feedback.HeadSweepSetPosition, PolishUnit.HeadSweepMotor, "SetPosition");
        SyncSwingMotorParameter(_headSweepJogVelocityItem, feedback.HeadSweepJogVelocity, PolishUnit.HeadSweepMotor, "JogVelocity");
        ApplyValue(feedback.HeadSweepActualVelocity, value => PolishUnit.HeadSweepMotor.ActVelocity = value);
        ApplyValue(feedback.HeadSweepActualPosition, value => PolishUnit.HeadSweepMotor.ActPosition = value);

        SyncSwingMotorParameter(_headLiftSetVelocityItem, feedback.HeadLiftSetVelocity, PolishUnit.HeadLiftMotor, "SetVelocity");
        SyncSwingMotorParameter(_headLiftSetPositionItem, feedback.HeadLiftSetPosition, PolishUnit.HeadLiftMotor, "SetPosition");
        SyncSwingMotorParameter(_headLiftJogVelocityItem, feedback.HeadLiftJogVelocity, PolishUnit.HeadLiftMotor, "JogVelocity");
        ApplyValue(feedback.HeadLiftActualVelocity, value => PolishUnit.HeadLiftMotor.ActVelocity = value);
        ApplyValue(feedback.HeadLiftActualPosition, value => PolishUnit.HeadLiftMotor.ActPosition = value);

        SyncSwingMotorParameter(_conditionerSweepSetVelocityItem, feedback.ConditionerSweepSetVelocity, PolishUnit.ConditionSweepMotor, "SetVelocity");
        SyncSwingMotorParameter(_conditionerSweepSetPositionItem, feedback.ConditionerSweepSetPosition, PolishUnit.ConditionSweepMotor, "SetPosition");
        SyncSwingMotorParameter(_conditionerSweepJogVelocityItem, feedback.ConditionerSweepJogVelocity, PolishUnit.ConditionSweepMotor, "JogVelocity");
        ApplyValue(feedback.ConditionerSweepActualVelocity, value => PolishUnit.ConditionSweepMotor.ActVelocity = value);
        ApplyValue(feedback.ConditionerSweepActualPosition, value => PolishUnit.ConditionSweepMotor.ActPosition = value);

        SyncSwingMotorParameter(_slurrySweepSetVelocityItem, feedback.SlurrySweepSetVelocity, PolishUnit.SlurrySweepMotor, "SetVelocity");
        SyncSwingMotorParameter(_slurrySweepSetPositionItem, feedback.SlurrySweepSetPosition, PolishUnit.SlurrySweepMotor, "SetPosition");
        SyncSwingMotorParameter(_slurrySweepJogVelocityItem, feedback.SlurrySweepJogVelocity, PolishUnit.SlurrySweepMotor, "JogVelocity");
        ApplyValue(feedback.SlurrySweepActualVelocity, value => PolishUnit.SlurrySweepMotor.ActVelocity = value);
        ApplyValue(feedback.SlurrySweepActualPosition, value => PolishUnit.SlurrySweepMotor.ActPosition = value);
    }

    private void SyncSwingMotorParameter(EditableFloatItem item, float? plcValue, SweepMotor motor, string parameter)
    {
        if (!plcValue.HasValue || float.IsNaN(plcValue.Value) || float.IsInfinity(plcValue.Value))
            return;

        var value = (double)plcValue.Value;
        item.PlcValue = value;
        item.HasPlcValue = true;
        if (item.IsDirty)
            return;

        if (!item.Value.Equals(value))
        {
            item.Value = value;
            RaisePropertyChanged(item.ValuePropertyName);
        }

        SetSwingMotorParameterValue(motor, parameter, plcValue.Value);
    }

    private void SyncRotaryMotorParameter(EditableFloatItem item, float? plcValue, Motor motor, string parameter)
    {
        if (!plcValue.HasValue || float.IsNaN(plcValue.Value) || float.IsInfinity(plcValue.Value))
            return;

        var value = (double)plcValue.Value;
        item.PlcValue = value;
        item.HasPlcValue = true;
        if (item.IsDirty)
            return;

        if (!item.Value.Equals(value))
        {
            item.Value = value;
            RaisePropertyChanged(item.ValuePropertyName);
        }

        SetMotorParameterValue(motor, parameter, plcValue.Value);
    }

    private static void ApplyValue(float? value, Action<float> setter)
    {
        if (value.HasValue && !float.IsNaN(value.Value) && !float.IsInfinity(value.Value))
            setter(value.Value);
    }

    private void RotaryMotorStartCommandExecute(string motorName)
    {
        WriteRotaryMotorCommand(motorName, start: true);
    }

    private void RotaryMotorStopCommandExecute(string motorName)
    {
        WriteRotaryMotorCommand(motorName, start: false);
    }

    private static string GetMotorText(string key) =>
        Resources.ResourceManager.GetString(key, LocalizeDictionary.Instance.Culture) ?? key;

    private static string FormatMotorText(string key, params object[] args) =>
        string.Format(LocalizeDictionary.Instance.Culture, GetMotorText(key), args);

    private void OnCultureChanged(object? sender, PropertyChangedEventArgs args)
    {
        RaisePropertyChanged(nameof(PlatenMotorServoText));
        RaisePropertyChanged(nameof(HeadRotateMotorServoText));
        RaisePropertyChanged(nameof(HeadSweepMotorServoText));
        RaisePropertyChanged(nameof(HeadLiftMotorServoText));
        RaisePropertyChanged(nameof(DiskRotateMotorServoText));
        RaisePropertyChanged(nameof(ConditionSweepMotorServoText));
        RaisePropertyChanged(nameof(SlurrySweepMotorServoText));
    }

    private static string GetMotorTitle(string motorName) => GetMotorText(motorName switch
    {
        "PlatenMotor" => "PolishMotorPlatenRotation",
        "HeadRotateMotor" => "PolishMotorHeadRotation",
        "DiskRotateMotor" => "PolishMotorConditionerRotation",
        "HeadSweepMotor" => "PolishMotorHeadSwing",
        "HeadLiftMotor" => "PolishMotorHeadLift",
        "ConditionSweepMotor" => "PolishMotorConditionerSwing",
        "SlurrySweepMotor" => "PolishMotorSlurryArmSwing",
        _ => motorName
    });

    private static string GetMotorParameterTitle(string parameter) => GetMotorText(parameter switch
    {
        "SetVelocity" => "PolishMotorSetSpeed",
        "SetPosition" => "PolishMotorSetPosition",
        "JogVelocity" => "PolishMotorJogSpeed",
        _ => parameter
    });

    private void WriteRotaryMotorCommand(string motorName, bool start)
    {
        if (!RotaryMotorPlcAddressMap.TryGetValue(motorName, out var address))
        {
            Growl.ErrorGlobal(FormatMotorText("PolishMotorUnknown", motorName));
            return;
        }

        var commandAddress = start ? address.StartCommand : address.StopCommand;
        var result = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_DM, commandAddress, 1,
            BitConverter.GetBytes(PolishMotorCommandValue));
        if (result != (int)KHLUserError.KHL_ERR_NONE)
            Growl.ErrorGlobal(FormatMotorText("PolishMotorCommandFailed",
                GetMotorTitle(motorName),
                GetMotorText(start ? "PolishMotorStart" : "Stop"), result));
    }

    private void SwingMotorStartCommandExecute(string motorName)
    {
        WriteSwingMotorCommand(motorName, start: true);
    }

    private void SwingMotorStopCommandExecute(string motorName)
    {
        WriteSwingMotorCommand(motorName, start: false);
    }

    private void WriteSwingMotorCommand(string motorName, bool start)
    {
        if (!SwingMotorPlcAddressMap.TryGetValue(motorName, out var address))
        {
            Growl.ErrorGlobal(FormatMotorText("PolishMotorUnknown", motorName));
            return;
        }

        var commandAddress = start ? address.StartCommand : address.StopCommand;
        var result = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_DM, commandAddress, 1,
            BitConverter.GetBytes(PolishMotorCommandValue));
        if (result != (int)KHLUserError.KHL_ERR_NONE)
            Growl.ErrorGlobal(FormatMotorText("PolishMotorCommandFailed",
                GetMotorTitle(motorName),
                GetMotorText(start ? "PolishMotorStart" : "Stop"), result));
    }

    private void InitializeSwingMotorEditableItem(EditableFloatItem item, string motorName, string parameter)
    {
        item.SetCommand = new DelegateCommand(() => SetSwingMotorParameter(item, motorName, parameter));
        item.CancelCommand = new DelegateCommand(() => CancelSwingMotorParameter(item, motorName, parameter));
    }

    private void SetSwingMotorParameter(EditableFloatItem item, string motorName, string parameter)
    {
        var motor = GetSwingMotor(motorName);
        if (motor == null || !SwingMotorPlcAddressMap.TryGetValue(motorName, out var address))
            return;

        var value = (float)item.Value;
        var target = $"{TabTitle}/{motorName}:{parameter}";
        if (!WriteSwingMotorParameter(motorName, parameter, value, address))
        {
            AuditLogService.WriteCurrentUser("SetValue", "Failed", "PLC write failed", AuditModules.Operation, target);
            return;
        }

        item.Value = value;
        item.PlcValue = value;
        item.HasPlcValue = true;
        SetSwingMotorParameterValue(motor, parameter, value);
        RaisePropertyChanged(item.ValuePropertyName);
        if (PlcEditableValueHelper.SetDirty(ref item.IsDirty, false))
            RaisePropertyChanged(item.DirtyPropertyName);
        AuditLogService.WriteCurrentUser("SetValue", "Success", string.Empty, AuditModules.Operation, target);
    }

    private void CancelSwingMotorParameter(EditableFloatItem item, string motorName, string parameter)
    {
        var motor = GetSwingMotor(motorName);
        if (motor == null)
            return;

        var value = item.HasPlcValue ? item.PlcValue : GetSwingMotorParameterValue(motor, parameter);
        if (!item.Value.Equals(value))
        {
            item.Value = value;
            RaisePropertyChanged(item.ValuePropertyName);
        }

        SetSwingMotorParameterValue(motor, parameter, (float)value);
        if (PlcEditableValueHelper.SetDirty(ref item.IsDirty, false))
            RaisePropertyChanged(item.DirtyPropertyName);
    }

    private SweepMotor? GetSwingMotor(string motorName) => motorName switch
    {
        "HeadSweepMotor" => PolishUnit.HeadSweepMotor,
        "HeadLiftMotor" => PolishUnit.HeadLiftMotor,
        "ConditionSweepMotor" => PolishUnit.ConditionSweepMotor,
        "SlurrySweepMotor" => PolishUnit.SlurrySweepMotor,
        _ => null
    };

    private static double GetSwingMotorParameterValue(SweepMotor motor, string parameter) => parameter switch
    {
        "SetVelocity" => motor.SetVelocity,
        "SetPosition" => motor.SetPosition,
        "JogVelocity" => motor.JogVelocity,
        _ => 0d
    };

    private static void SetSwingMotorParameterValue(SweepMotor motor, string parameter, float value)
    {
        if (parameter == "SetVelocity")
            motor.SetVelocity = value;
        else if (parameter == "SetPosition")
            motor.SetPosition = value;
        else if (parameter == "JogVelocity")
            motor.JogVelocity = value;
    }

    private bool WriteSwingMotorParameter(string motorName, string parameter, float value,
        SwingMotorPlcAddresses address)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
            return false;

        var data = BitConverter.GetBytes(value);
        var wordAddress = parameter switch
        {
            "SetPosition" => address.SetPosition,
            "JogVelocity" => address.JogVelocity,
            _ => address.SetVelocity
        };
        var result = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, wordAddress, 2, data);
        if (result != (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.ErrorGlobal(FormatMotorText("PolishMotorParameterWriteFailed",
                GetMotorTitle(motorName),
                GetMotorParameterTitle(parameter), result));
            return false;
        }

        return true;
    }

    private void InitializeRotaryMotorEditableItem(EditableFloatItem item, string motorName, string parameter)
    {
        item.SetCommand = new DelegateCommand(() => SetRotaryMotorParameter(item, motorName, parameter));
        item.CancelCommand = new DelegateCommand(() => CancelRotaryMotorParameter(item, motorName, parameter));
    }

    private void SetRotaryMotorParameter(EditableFloatItem item, string motorName, string parameter)
    {
        var motor = GetRotaryMotor(motorName);
        if (motor == null || !RotaryMotorPlcAddressMap.TryGetValue(motorName, out var address))
            return;

        var value = (float)item.Value;
        var target = $"{TabTitle}/{motorName}:{parameter}";
        if (!WriteRotaryMotorParameter(motorName, parameter, value, address))
        {
            AuditLogService.WriteCurrentUser("SetValue", "Failed", "PLC write failed", AuditModules.Operation, target);
            return;
        }

        item.Value = value;
        item.PlcValue = value;
        item.HasPlcValue = true;
        SetMotorParameterValue(motor, parameter, value);
        RaisePropertyChanged(item.ValuePropertyName);
        if (PlcEditableValueHelper.SetDirty(ref item.IsDirty, false))
            RaisePropertyChanged(item.DirtyPropertyName);
        AuditLogService.WriteCurrentUser("SetValue", "Success", string.Empty, AuditModules.Operation, target);
    }

    private void CancelRotaryMotorParameter(EditableFloatItem item, string motorName, string parameter)
    {
        var motor = GetRotaryMotor(motorName);
        if (motor == null)
            return;

        var value = item.HasPlcValue ? item.PlcValue : GetMotorParameterValue(motor, parameter);
        if (!item.Value.Equals(value))
        {
            item.Value = value;
            RaisePropertyChanged(item.ValuePropertyName);
        }

        SetMotorParameterValue(motor, parameter, (float)value);
        if (PlcEditableValueHelper.SetDirty(ref item.IsDirty, false))
            RaisePropertyChanged(item.DirtyPropertyName);
    }

    private Motor? GetRotaryMotor(string motorName) => motorName switch
    {
        "PlatenMotor" => PolishUnit.PlatenMotor,
        "HeadRotateMotor" => PolishUnit.HeadRotateMotor,
        "DiskRotateMotor" => PolishUnit.DiskRotateMotor,
        _ => null
    };

    private static double GetMotorParameterValue(Motor motor, string parameter) => parameter switch
    {
        "SetVelocity" => motor.SetVelocity,
        "JogVelocity" => motor.JogVelocity,
        _ => 0d
    };

    private static void SetMotorParameterValue(Motor motor, string parameter, float value)
    {
        if (parameter == "SetVelocity")
            motor.SetVelocity = value;
        else if (parameter == "JogVelocity")
            motor.JogVelocity = value;
    }

    private bool WriteRotaryMotorParameter(string motorName, string parameter, float value,
        RotaryMotorPlcAddresses address)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
            return false;

        var data = BitConverter.GetBytes(value);
        var wordAddress = parameter == "JogVelocity" ? address.JogVelocity : address.SetVelocity;
        var result = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, wordAddress, 2, data);
        if (result != (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.ErrorGlobal(FormatMotorText("PolishMotorParameterWriteFailed",
                GetMotorTitle(motorName),
                GetMotorParameterTitle(parameter), result));
            return false;
        }

        return true;
    }

    private void JogPressCommandExecute(string motorName)
    {
        var parts = motorName?.Split(':', 2, StringSplitOptions.TrimEntries);
        if (parts is not { Length: 2 })
            return;

        var rotaryMotor = GetRotaryMotor(parts[0]);
        if (rotaryMotor != null && RotaryMotorPlcAddressMap.TryGetValue(parts[0], out var rotaryAddress))
        {
            if (!WriteRotaryMotorParameter(parts[0], "JogVelocity", rotaryMotor.JogVelocity, rotaryAddress))
                return;
        }
        else
        {
            var swingMotor = GetSwingMotor(parts[0]);
            if (swingMotor == null || !SwingMotorPlcAddressMap.TryGetValue(parts[0], out var swingAddress))
                return;

            if (!WriteSwingMotorParameter(parts[0], "JogVelocity", swingMotor.JogVelocity, swingAddress))
                return;
        }

        WriteJogBit(parts[0], string.Equals(parts[1], "+", StringComparison.Ordinal));
    }

    private void JogReleaseCommandExecute(string motorName)
    {
        WriteJogBit(motorName, plus: null);
    }

    private void WriteJogBit(string motorName, bool? plus)
    {
        var hasRotaryAddress = RotaryMotorPlcAddressMap.TryGetValue(motorName, out var rotaryAddress);
        var hasSwingAddress = SwingMotorPlcAddressMap.TryGetValue(motorName, out var swingAddress);
        if (!hasRotaryAddress && !hasSwingAddress)
            return;

        var wordAddress = hasRotaryAddress ? rotaryAddress.JogBitWord : swingAddress.JogBitWord;
        var plusOffset = hasRotaryAddress ? rotaryAddress.JogPlusOffset : swingAddress.JogPlusOffset;
        var minusOffset = hasRotaryAddress ? rotaryAddress.JogMinusOffset : swingAddress.JogMinusOffset;

        if (plus == null)
        {
            WriteJogBitValue(wordAddress, plusOffset, false);
            WriteJogBitValue(wordAddress, minusOffset, false);
            return;
        }

        var activeOffset = plus.Value ? plusOffset : minusOffset;
        var inactiveOffset = plus.Value ? minusOffset : plusOffset;
        if (WriteJogBitValue(wordAddress, inactiveOffset, false))
            WriteJogBitValue(wordAddress, activeOffset, true);
    }

    private bool WriteJogBitValue(uint wordAddress, ushort bitOffset, bool value)
    {
        var result = KHL.KHLWriteDevicesAsBits(kvLink.sock, KHLDevType.DEV_ZF, wordAddress, bitOffset, 1,
            new byte[] { value ? (byte)1 : (byte)0 });
        if (result != (int)KHLUserError.KHL_ERR_NONE)
        {
            Logger.For("Motor").Debug($"Write JOG bit ZF{wordAddress} bit {bitOffset} failed: {result}");
            return false;
        }

        return true;
    }

    private void RefreshHMI()
    {
        if (!polishData.TryGetProcessDataSnapshot(out var snapshot))
        {
            return;
        }

        PolishUnit.PlatenMotor.ActVelocity = snapshot.PlatenRotationSpeedAct;
        PolishUnit.HeadRotateMotor.ActVelocity = snapshot.HeadRotationSpeedAct;
        PolishUnit.HeadSweepMotor.ActVelocity = snapshot.HeadSweepSpeedAct;
        PolishUnit.HeadSweepMotor.ActPosition = snapshot.HeadSweepPositionAct;
        PolishUnit.HeadLiftMotor.ActVelocity = snapshot.HeadZ_AxisSpeedAct;
        PolishUnit.HeadLiftMotor.ActPosition = snapshot.HeadZ_AxisPositionAct;
        PolishUnit.DiskRotateMotor.ActVelocity = snapshot.ConditionRotationSpeedAct;
        PolishUnit.ConditionSweepMotor.ActVelocity = snapshot.ConditionArmSweepSpeedAct;
        PolishUnit.ConditionSweepMotor.ActPosition = snapshot.ConditionArmSweepPositionAct;

        PolishUnit.Zone1.ActPress = snapshot.Zone1PressureAct;
        PolishUnit.Zone2.ActPress = snapshot.Zone2PressureAct;
        PolishUnit.Zone3.ActPress = snapshot.Zone3PressureAct;
        PolishUnit.Zone4.ActPress = snapshot.Zone4PressureAct;
        
        PolishUnit.Zone4.ActVacuum = snapshot.VacuumPressAct;
        
        PolishUnit.Zone5.ActPress = snapshot.Zone5PressureAct;
        PolishUnit.Zone6.ActPress = snapshot.Zone6PressureAct;
        PolishUnit.Zone7.ActPress = snapshot.Zone7PressureAct;
        PolishUnit.Zone8.ActPress = snapshot.Zone8PressureAct;
        PolishUnit.Retain.ActPress = snapshot.RetainerRingPressureAct;
        PolishUnit.Slurry.Slurry1Act = snapshot.Slurry1OrDIWFlowrateAct;
        PolishUnit.Slurry.Slurry2Act = snapshot.Slurry2OrDIWFlowrateAct;
        PolishUnit.Slurry.Slurry3Act = snapshot.Slurry3OrDIWFlowrateAct;
        SlurryControl.Lines[0].ActualFlowrate = snapshot.Slurry1OrDIWFlowrateAct;
        SlurryControl.Lines[1].ActualFlowrate = snapshot.Slurry2OrDIWFlowrateAct;
        SlurryControl.Lines[2].ActualFlowrate = snapshot.Slurry3OrDIWFlowrateAct;

        PolishUnit.PlatenMotor.IsServoOn = snapshot.PlatenRotationMotorServoOn;
        PolishUnit.HeadRotateMotor.IsServoOn = snapshot.HeadRotationMotorServoOn;
        PolishUnit.HeadSweepMotor.IsServoOn = snapshot.HeadSweepMotorServoOn;
        PolishUnit.HeadLiftMotor.IsServoOn = snapshot.HeadUpDownMotorServoOn;
        PolishUnit.DiskRotateMotor.IsServoOn = snapshot.ConditionHeadRotationMotorServoOn;
        PolishUnit.ConditionSweepMotor.IsServoOn = snapshot.ConditionSweepMotorServoOn;
        PolishUnit.SlurrySweepMotor.IsServoOn = snapshot.SlurrySweepMotorServoOn;

        PolishUnit.PlatenMotor.ActTorque = snapshot.PlatenRotationMotorTorqueAct;
        PolishUnit.HeadRotateMotor.ActTorque = snapshot.HeadRotationMotorTorqueAct;
        PolishUnit.HeadSweepMotor.ActTorque = snapshot.HeadSweepMotorTorqueAct;
        PolishUnit.HeadLiftMotor.ActTorque = snapshot.HeadZ_AxisMotorTorqueAct;
        PolishUnit.DiskRotateMotor.ActTorque = snapshot.ConditionRotationMotorTorqueAct;
        PolishUnit.ConditionSweepMotor.ActTorque = snapshot.ConditionSweepMotorTorqueAct;
        PolishUnit.SlurrySweepMotor.ActTorque = snapshot.SlurryNozzleMotorTorqueAct;

        RaisePropertyChanged(nameof(PlatenMotorServoText));
        RaisePropertyChanged(nameof(ConditionSweepMotorServoText));
        RaisePropertyChanged(nameof(HeadRotateMotorServoText));
        RaisePropertyChanged(nameof(HeadLiftMotorServoText));
        RaisePropertyChanged(nameof(HeadSweepMotorServoText));
        RaisePropertyChanged(nameof(DiskRotateMotorServoText));
        RaisePropertyChanged(nameof(SlurrySweepMotorServoText));
    }

    private void SetWaferIndex(int waferIndex)
    {
        this.waferIndex = waferIndex;
        switch (TabId)
        {
            case 0:
                GlobalService.globalChambers.PolishA.WaferIndex = waferIndex;
                break;
            case 1:
                GlobalService.globalChambers.PolishB.WaferIndex = waferIndex;
                break;
            case 2:
                GlobalService.globalChambers.PolishC.WaferIndex = waferIndex;
                break;
            case 3:
                GlobalService.globalChambers.PolishD.WaferIndex = waferIndex;
                break;
        }
    }

    private void IntereactionWithPLCForAssignRecipe()
    {
        err = KHL.KHLReadDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, Convert.ToUInt32(0x300C), 1, readBuf);
        if (err != (int)KHLUserError.KHL_ERR_NONE)
        {
            Logger.For("RecipeAssign").Warn($"Read recipe req failed: {TabTitle} address=0x300C, err={err}");
            return;
        }

        int recipeReq = Convert.ToInt32(readBuf[0]);

        if (recipeReq != 0)
        {
            Growl.SuccessGlobal($"Index: {recipeReq}");
            int waferIndex = recipeReq;
            byte w10Low = readBuf[0];
            SetWaferIndex(waferIndex);
            //byte[] writeBufTemp;
            try
            {
                var (polishRecipeName, exsituConditionRecipeName) =
                    GetRecipe(GlobalService.dicWaferIndex[waferIndex], this.Id);

                AssignWaferToPLC(polishRecipeName, exsituConditionRecipeName);

                //向地址中写入wafer的sequence数值
                var wafer = GlobalService.dicWaferIndex[waferIndex];
                for (int i = 0; i < wafer.SequnceValue.Length; i++)
                {
                    byte[] data = new byte[256];
                    uint devTopNo = (uint)(Convert.ToUInt32(_baseChamberAddress?.Replace("ZF", "")) + 10 + 5 * i);
                    KST.IntToByte(ref data, wafer.SequnceValue[i]);
                    int error = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, devTopNo, 2, data);
                    if (error != (int)KHLUserError.KHL_ERR_NONE)
                    {
                        Logger.For("SequenceWrite").Debug($"向{TabTitle}地址ZF{devTopNo}中写入数值{wafer.SequnceValue[i]}失败，错误码返回码{error}");
                    }
                    else
                    {
                        Logger.For("SequenceWrite").Debug($"向{TabTitle}地址ZF{devTopNo}中写入数值{wafer.SequnceValue[i]}成功！");
                    }
                }

                byte[] writeBufTemp = { w10Low, 0x02 };
                err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, Convert.ToUInt32(0x010), 1,
                    writeBufTemp);
            }
            catch(Exception ex)
            {
                byte[] writeBufTemp = { w10Low, 0x03 };
                err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, Convert.ToUInt32(0x010), 1, writeBufTemp);
                Growl.ErrorGlobal($"Assign Recipe Failed, Index: {waferIndex},Error Information: {ex.Message},{ex.StackTrace}");
                Logger.For("AssignPolishRecipe").Error($"Assign Recipe Failed, Index: {waferIndex},Error Information: {ex.Message},{ex.StackTrace}");
            }

            //err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, Convert.ToUInt32(0x010), 1, writeBufTemp);
            //Growl.SuccessGlobal("SET: W010");
            isResetRecipeState = true;
        }
        else
        {
            //if (isResetRecipeState)
            //{
            byte[] writeBufTemp = { 0, 0 };
            err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, Convert.ToUInt32(0x010), 1, writeBufTemp);
            //Growl.SuccessGlobal("CLEAR: W010");
            isResetRecipeState = false;
            //}
        }
    }

    private void AssignWaferToPLC(string polishRecipeName, string exsituConditionRecipeName)
    {
        SelectedPolishRecipe = polishRecipeName;
        SelectedExsituConditionRecipe = exsituConditionRecipeName;

        LoadCommandExecute("Auto");

        PolishCommandExecute("Auto");

        UnloadCommandExecute("Auto");

        DechuckCommandExecute("Auto");

        BreakInCommandExecute("Auto");

    }

    private (string, string) GetRecipe(Wafer wafer, int id)
    {
        var path = Path.Combine("D:\\CMP\\Configuration", "Recipes", "Sequence", $"{wafer.SequenceRecipe}.json");
        var text = File.ReadAllText(path);
        var data = JsonConvert.DeserializeObject<SequenceRecipeData>(text);
        string polishRecipeName = string.Empty;
        string conditionRecipeName = string.Empty;
        if (id == 0)
        {
            polishRecipeName =
                GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipePolishA").Value.ToString());
            conditionRecipeName =
                GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipePolishAExsitu").Value.ToString());
            wafer.CurrentStation = CurrentStation.PolishA;
        }
        else if (id == 1)
        {
            polishRecipeName =
                GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipePolishB").Value.ToString());
            conditionRecipeName =
                GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipePolishBExsitu").Value.ToString());
            wafer.CurrentStation = CurrentStation.PolishB;
        }
        else if (id == 2)
        {
            polishRecipeName =
                GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipePolishC").Value.ToString());
            conditionRecipeName =
                GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipePolishCExsitu").Value.ToString());
            wafer.CurrentStation = CurrentStation.PolishC;
        }
        else if (id == 3)
        {
            polishRecipeName =
                GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipePolishD").Value.ToString());
            conditionRecipeName =
                GetFileNameWithoutEx(data.RecipeSelections.Find(x => x.Key == "RecipePolishDExsitu").Value.ToString());
            wafer.CurrentStation = CurrentStation.PolishD;
        }

        return (polishRecipeName, conditionRecipeName);
    }

    private string GetFileNameWithoutEx(string path)
    {
        string[] temp = path.Split('\\');
        string fileName = temp[temp.Length - 1];
        return fileName.Substring(0, fileName.Length - 5);
    }

    private int KHLConnectAndOperation(string handle, byte[] value)
    {
        return KHL.KHLWriteVariable(kvLink.sock, handle, value, true);
    }

    private void StartCommandExecute(string obj)
    {
        if (string.IsNullOrEmpty(obj))
            return;

        // 解析参数，格式为 "设备名_操作" (如 "DIWOutletValve_Open")
        string[] parts = obj.Split('_');
        if (parts.Length != 2)
        {
            Growl.WarningGlobal($"无效的命令参数: {obj}");
            return;
        }

        string deviceName = parts[0];
        string action = parts[1]; // "Open" 或 "Close"

        uint plcHandle = 0u;
        byte[] plcValue = new byte[2048];

        try
        {
            // 根据设备名称和操作类型获取对应的PLC Handle和Value
            switch (deviceName)
            {
                case "DIWOutletValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.DIWOutletValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.DIWOutletValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.DIWOutletValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.DIWOutletValve.Close.Value();
                    break;

                case "PlatenWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.PlatenWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.PlatenWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.PlatenWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.PlatenWashValve.Close.Value();
                    break;

                case "MovingHeadDIWWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.MovingHeadDIWWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.MovingHeadDIWWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.MovingHeadDIWWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.MovingHeadDIWWashValve.Close.Value();
                    break;

                case "MovingHeadExternalDIWWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.MovingHeadExternalDIWWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.MovingHeadExternalDIWWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.MovingHeadExternalDIWWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.MovingHeadExternalDIWWashValve.Close.Value();
                    break;

                case "HeadChemicalWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.HeadChemicalWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.HeadChemicalWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.HeadChemicalWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.HeadChemicalWashValve.Close.Value();
                    break;

                case "ConditionerDIWWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.ConditionerDIWWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.ConditionerDIWWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.ConditionerDIWWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.ConditionerDIWWashValve.Close.Value();
                    break;

                case "ConditionerArmDIWWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.ConditionerArmDIWWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.ConditionerArmDIWWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.ConditionerArmDIWWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.ConditionerArmDIWWashValve.Close.Value();
                    break;

                case "ChemicalArmDIWWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.ChemicalArmDIWWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.ChemicalArmDIWWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.ChemicalArmDIWWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.ChemicalArmDIWWashValve.Close.Value();
                    break;

                case "HeadMiddleWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.HeadMiddleWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.HeadMiddleWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.HeadMiddleWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.HeadMiddleWashValve.Close.Value();
                    break;

                case "HeadExternalWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.HeadExternalWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.HeadExternalWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.HeadExternalWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.HeadExternalWashValve.Close.Value();
                    break;

                case "WFUpperFaceChemicalWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.WFUpperFaceChemicalWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.WFUpperFaceChemicalWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.WFUpperFaceChemicalWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.WFUpperFaceChemicalWashValve.Close.Value();
                    break;

                case "WFUpperFaceDIWWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.WFUpperFaceDIWWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.WFUpperFaceDIWWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.WFUpperFaceDIWWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.WFUpperFaceDIWWashValve.Close.Value();
                    break;

                case "WFLowerFaceChemicalWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.WFLowerFaceChemicalWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.WFLowerFaceChemicalWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.WFLowerFaceChemicalWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.WFLowerFaceChemicalWashValve.Close.Value();
                    break;

                case "WFLowerFaceDIWWashValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.WFLowerFaceDIWWashValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.WFLowerFaceDIWWashValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.WFLowerFaceDIWWashValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.WFLowerFaceDIWWashValve.Close.Value();
                    break;

                case "PlatenCoolWaterValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.PlatenCoolWaterValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.PlatenCoolWaterValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.PlatenCoolWaterValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.PlatenCoolWaterValve.Close.Value();
                    break;

                case "PlatenMotorCoolWaterValve":
                    plcHandle = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.PlatenMotorCoolWaterValve.Open.Handle()
                        : PlcHandle.CmdPolish.Liquid.PlatenMotorCoolWaterValve.Close.Handle();
                    plcValue = action == "Open"
                        ? PlcHandle.CmdPolish.Liquid.PlatenMotorCoolWaterValve.Open.Value()
                        : PlcHandle.CmdPolish.Liquid.PlatenMotorCoolWaterValve.Close.Value();
                    break;

                default:
                    Growl.WarningGlobal($"未知的设备: {deviceName}");
                    return;
            }

            // 发送PLC命令
            err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, plcHandle, 1, plcValue);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                string actionText = action == "Open" ? "On" : "Off";
                Growl.SuccessGlobal($"{deviceName} {actionText} command executed successfully.");
            }
            else
            {
                string actionText = action == "Open" ? "On" : "Off";
                Growl.ErrorGlobal($"{deviceName} {actionText} command execution failed with error code: {err}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"exception occurred while executing {deviceName} {action} command. {ex.Message}");
            Logger.Error($"Error executing liquid control command for {deviceName} {action}", ex);
        }
    }

    /// <summary>
    /// 解析位置输入值：必须是有限数字（拒绝 NaN/Infinity 等非法值）
    /// </summary>
    private static bool TryParsePositionValue(string? text, out float value)
    {
        if (float.TryParse(text, out value) && !float.IsNaN(value) && !float.IsInfinity(value))
        {
            return true;
        }

        value = 0;
        return false;
    }

    /// <summary>
    /// 写入位置参数后立即读回验证（用于诊断"写入值为0"的问题；定位后可移除）
    /// </summary>
    private void VerifyPositionWrite(string motorName, uint posHandle, float expected)
    {
        var readBackBuf = new byte[2048];
        var readErr = KHL.KHLReadDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, posHandle, 2, readBackBuf);
        if (readErr == 0)
        {
            Growl.InfoGlobal($"{motorName}: set {expected:F2}, read back {BitConverter.ToSingle(readBackBuf, 0):F2}");
        }
        else
        {
            Growl.WarningGlobal($"{motorName}: read back position failed, err={readErr}");
        }
    }

    /// <summary>
    /// 尝试解析预设位置命令（key 形如 "HeadSweepMotor_Home"）。
    /// 返回 true 时 plcHandle/plcValue 为预设位置命令，plcVelHandle/plcVelValue 为对应电机速度参数；
    /// 返回 false 表示不是预设位置，调用方应按自定义数值处理。
    /// </summary>
    private bool TryGetPresetPosition(string key, out uint plcHandle, out byte[] plcValue,
        out uint plcVelHandle, out byte[] plcVelValue)
    {
        plcHandle = 0;
        plcValue = new byte[2048];
        plcVelHandle = 0;
        plcVelValue = new byte[2048];

        switch (key)
        {
            case "HeadSweepMotor_Home":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadSweep.HomePosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadSweep.HomePosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.HeadSweep.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.HeadSweepMotor.SetVelocity.Value();
                break;
            case "HeadSweepMotor_PolishNear":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadSweep.PolishNearPosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadSweep.PolishNearPosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.HeadSweep.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.HeadSweepMotor.SetVelocity.Value();
                break;
            case "HeadSweepMotor_PolishFar":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadSweep.PolishFarPosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadSweep.PolishFarPosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.HeadSweep.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.HeadSweepMotor.SetVelocity.Value();
                break;
            case "HeadSweepMotor_Dechuck":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadSweep.DechuckPosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadSweep.DechuckPosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.HeadSweep.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.HeadSweepMotor.SetVelocity.Value();
                break;
            case "HeadLiftMotor_High":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.HighPosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadUpDown.HighPosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.HeadLiftMotor.SetVelocity.Value();
                break;
            case "HeadLiftMotor_LoadCup":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.LoadCupPosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadUpDown.LoadCupPosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.HeadLiftMotor.SetVelocity.Value();
                break;
            case "HeadLiftMotor_Polish":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.PolishPosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadUpDown.PolishPosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.HeadLiftMotor.SetVelocity.Value();
                break;
            case "HeadLiftMotor_Clean":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.CleanPosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadUpDown.CleanPosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.HeadLiftMotor.SetVelocity.Value();
                break;
            case "ConditionSweepMotor_Home":
                plcHandle = PlcHandle.CmdPolish.Motor.ConditionerSweep.HomePosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.ConditionerSweep.HomePosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.ConditionerSweep.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.ConditionSweepMotor.SetVelocity.Value();
                break;
            case "ConditionSweepMotor_Start":
                plcHandle = PlcHandle.CmdPolish.Motor.ConditionerSweep.StartPosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.ConditionerSweep.StartPosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.ConditionerSweep.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.ConditionSweepMotor.SetVelocity.Value();
                break;
            case "ConditionSweepMotor_End":
                plcHandle = PlcHandle.CmdPolish.Motor.ConditionerSweep.EndPosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.ConditionerSweep.EndPosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.ConditionerSweep.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.ConditionSweepMotor.SetVelocity.Value();
                break;
            case "SlurrySweepMotor_Home":
                plcHandle = PlcHandle.CmdPolish.Motor.SlurryArmSweep.HomePosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.SlurryArmSweep.HomePosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.SlurryArmSweep.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.SlurrySweepMotor.SetVelocity.Value();
                break;
            case "SlurrySweepMotor_Slurry":
                plcHandle = PlcHandle.CmdPolish.Motor.SlurryArmSweep.SlurryPosition.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.SlurryArmSweep.SlurryPosition.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.SlurryArmSweep.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.SlurrySweepMotor.SetVelocity.Value();
                break;
            default:
                return false;
        }

        return true;
    }

    private void PositionCommandExecute(string obj)
    {
        if (!TryGetPresetPosition(obj, out var plcHandle, out var plcValue, out var plcVelHandle, out var plcVelValue))
        {
            Growl.ErrorGlobal($"PositionCommandExecute: 未知位置参数 '{obj}'");
            return;
        }

        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
        if (err != 0)
        {
            Growl.ErrorGlobal($"PositionCommandExecute Velocity Error: {err}");
        }

        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, plcHandle, 1, plcValue);
        if (err != 0)
        {
            Growl.ErrorGlobal($"PositionCommandExecute Error: {err}");
        }
        else
        {
            Growl.SuccessGlobal($"PositionCommandExecute: {obj}");
        }
    }

    private void RunCommandExecute(string obj)
    {
        var plcHandle = 0u;
        var plcValue = new byte[2048];
        var plcVelHandle = 0u;
        var plcVelValue = new byte[2048];
        var plcPosHandle = 0u;
        var plcPosValue = new byte[2048];

        switch (obj)
        {
            case "PlatenMotor":
                // 使用新的结构，保持向后兼容
                plcHandle = PlcHandle.CmdPolish.Motor.PlatenRotation.Run.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.PlatenRotation.Run.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.PlatenRotation.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.PlatenMotor.SetVelocity.Value();
                err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                break;
            case "HeadRotateMotor":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadRotation.Run.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadRotation.Run.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.HeadRotation.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.HeadRotateMotor.SetVelocity.Value();
                err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                break;
            case "DiskRotateMotor":
                plcHandle = PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.Run.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.Run.Value();
                plcVelHandle = PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.Param.Velocity.ParamHandle();
                plcVelValue = PolishUnit.DiskRotateMotor.SetVelocity.Value();
                err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                break;
            case "HeadLiftMotor":
                if (TryGetPresetPosition("HeadLiftMotor_" + SelectedHeadLiftMotorPosition,
                        out plcHandle, out plcValue, out plcVelHandle, out plcVelValue))
                {
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                }
                else if (TryParsePositionValue(SelectedHeadLiftMotorPosition, out var headLiftCustomPos))
                {
                    plcVelHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.Param.Velocity.ParamHandle();
                    plcVelValue = PolishUnit.HeadLiftMotor.SetVelocity.Value();
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                    plcHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.AnyPosition.Handle();
                    plcValue = PlcHandle.CmdPolish.Motor.HeadUpDown.AnyPosition.Value();
                    plcPosHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.Param.Position.ParamHandle();
                    PolishUnit.HeadLiftMotor.SetPosition = headLiftCustomPos;
                    plcPosValue = PolishUnit.HeadLiftMotor.SetPosition.Value();
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcPosHandle, 2, plcPosValue);
                    VerifyPositionWrite("HeadLiftMotor", plcPosHandle, headLiftCustomPos);
                }
                else
                {
                    Growl.ErrorGlobal($"HeadLiftMotor: Invalid position '{SelectedHeadLiftMotorPosition}'. Select a preset or enter a numeric value");
                    return;
                }
                break;
            case "SlurrySweepMotor":
                if (TryGetPresetPosition("SlurrySweepMotor_" + SelectedSlurrySweepMotorPosition,
                        out plcHandle, out plcValue, out plcVelHandle, out plcVelValue))
                {
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                }
                else if (TryParsePositionValue(SelectedSlurrySweepMotorPosition, out var slurryCustomPos))
                {
                    plcVelHandle = PlcHandle.CmdPolish.Motor.SlurryArmSweep.Param.Velocity.ParamHandle();
                    plcVelValue = PolishUnit.SlurrySweepMotor.SetVelocity.Value();
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                    plcHandle = PlcHandle.CmdPolish.Motor.SlurryArmSweep.AnyPosition.Handle();
                    plcValue = PlcHandle.CmdPolish.Motor.SlurryArmSweep.AnyPosition.Value();
                    plcPosHandle = PlcHandle.CmdPolish.Motor.SlurryArmSweep.Param.Position.ParamHandle();
                    PolishUnit.SlurrySweepMotor.SetPosition = slurryCustomPos;
                    plcPosValue = PolishUnit.SlurrySweepMotor.SetPosition.Value();
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcPosHandle, 2, plcPosValue);
                    VerifyPositionWrite("SlurrySweepMotor", plcPosHandle, slurryCustomPos);
                }
                else
                {
                    Growl.ErrorGlobal($"SlurrySweepMotor: Invalid position '{SelectedSlurrySweepMotorPosition}'. Select a preset or enter a numeric value");
                    return;
                }
                break;
            case "HeadSweepMotor":
                if (TryGetPresetPosition("HeadSweepMotor_" + SelectedHeadSweepMotorPosition,
                        out plcHandle, out plcValue, out plcVelHandle, out plcVelValue))
                {
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                }
                else if (TryParsePositionValue(SelectedHeadSweepMotorPosition, out var headSweepCustomPos))
                {
                    plcVelHandle = PlcHandle.CmdPolish.Motor.HeadSweep.Param.Velocity.ParamHandle();
                    plcVelValue = PolishUnit.HeadSweepMotor.SetVelocity.Value();
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                    plcHandle = PlcHandle.CmdPolish.Motor.HeadSweep.AnyPosition.Handle();
                    plcValue = PlcHandle.CmdPolish.Motor.HeadSweep.AnyPosition.Value();
                    plcPosHandle = PlcHandle.CmdPolish.Motor.HeadSweep.Param.Position.ParamHandle();
                    PolishUnit.HeadSweepMotor.SetPosition = headSweepCustomPos;
                    plcPosValue = PolishUnit.HeadSweepMotor.SetPosition.Value();
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcPosHandle, 2, plcPosValue);
                    VerifyPositionWrite("HeadSweepMotor", plcPosHandle, headSweepCustomPos);
                }
                else
                {
                    Growl.ErrorGlobal($"HeadSweepMotor: Invalid position '{SelectedHeadSweepMotorPosition}'. Select a preset or enter a numeric value");
                    return;
                }
                break;
            case "ConditionSweepMotor":
                if (TryGetPresetPosition("ConditionSweepMotor_" + SelectedConditionSweepMotorPosition,
                        out plcHandle, out plcValue, out plcVelHandle, out plcVelValue))
                {
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                }
                else if (TryParsePositionValue(SelectedConditionSweepMotorPosition, out var conditionCustomPos))
                {
                    plcVelHandle = PlcHandle.CmdPolish.Motor.ConditionerSweep.Param.Velocity.ParamHandle();
                    plcVelValue = PolishUnit.ConditionSweepMotor.SetVelocity.Value();
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcVelHandle, 2, plcVelValue);
                    plcHandle = PlcHandle.CmdPolish.Motor.ConditionerSweep.AnyPosition.Handle();
                    plcValue = PlcHandle.CmdPolish.Motor.ConditionerSweep.AnyPosition.Value();
                    plcPosHandle = PlcHandle.CmdPolish.Motor.ConditionerSweep.Param.Position.ParamHandle();
                    PolishUnit.ConditionSweepMotor.SetPosition = conditionCustomPos;
                    plcPosValue = PolishUnit.ConditionSweepMotor.SetPosition.Value();
                    err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, plcPosHandle, 2, plcPosValue);
                    VerifyPositionWrite("ConditionSweepMotor", plcPosHandle, conditionCustomPos);
                }
                else
                {
                    Growl.ErrorGlobal($"ConditionSweepMotor: Invalid position '{SelectedConditionSweepMotorPosition}'. Select a preset or enter a numeric value");
                    return;
                }
                break;
        }

        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, plcHandle, 1, plcValue);
        if (err != 0)
        {
            Growl.ErrorGlobal($"RunCommandExecute Error: {err}");
        }
        else
        {
            Growl.SuccessGlobal($"RunCommandExecute: {obj}");
        }
    }

    private void AbortCommandExecute(string obj)
    {
        var plcHandle = 0u;
        var plcValue = new byte[2048];
        switch (obj)
        {
            case "PlatenMotor":
                plcHandle = PlcHandle.CmdPolish.Motor.PlatenRotation.Stop.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.PlatenRotation.Stop.Value();
                break;
            case "HeadRotateMotor":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadRotation.Stop.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadRotation.Stop.Value();
                break;
            case "DiskRotateMotor":
                plcHandle = PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.Stop.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.Stop.Value();
                break;
            case "HeadSweepMotor":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadSweep.Stop.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadSweep.Stop.Value();
                break;
            case "ConditionSweepMotor":
                plcHandle = PlcHandle.CmdPolish.Motor.ConditionerSweep.Stop.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.ConditionerSweep.Stop.Value();
                break;
            case "HeadLiftMotor":
                plcHandle = PlcHandle.CmdPolish.Motor.HeadUpDown.Stop.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.HeadUpDown.Stop.Value();
                break;
            case "SlurrySweepMotor":
                plcHandle = PlcHandle.CmdPolish.Motor.SlurryArmSweep.Stop.Handle();
                plcValue = PlcHandle.CmdPolish.Motor.SlurryArmSweep.Stop.Value();
                break;
        }

        //err = KHL.KHLConnect(_plcAddr, _plcPort, 3000, KHLSockType.SOCK_TCP, ref sock);
        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, plcHandle, 1, plcValue);
        if (err != 0)
        {
            Growl.ErrorGlobal($"AbortCommandExecute Error: {err}");
        }
        else
        {
            Growl.SuccessGlobal($"AbortCommandExecute: {obj}");
        }
    }

    private void ServoCommandExecute(string obj)
    {
        var plcHandle = 0u;
        byte[] plcValue = [];
        var newServoState = false;

        void PrepareServoCommand(
            bool currentState,
            uint servoOnHandle,
            byte[] servoOnValue,
            uint servoOffHandle,
            byte[] servoOffValue,
            Action<bool> setState,
            string propertyName)
        {
            newServoState = !currentState;
            plcHandle = newServoState ? servoOnHandle : servoOffHandle;
            plcValue = newServoState ? servoOnValue : servoOffValue;
            setState(newServoState);
            RaisePropertyChanged(propertyName);
        }

        switch (obj)
        {
            case "PlatenMotor":
                PrepareServoCommand(
                    PolishUnit.PlatenMotor.IsServoOn,
                    PlcHandle.CmdPolish.Motor.PlatenRotation.ServoOn.Handle(),
                    PlcHandle.CmdPolish.Motor.PlatenRotation.ServoOn.Value(),
                    PlcHandle.CmdPolish.Motor.PlatenRotation.ServoOff.Handle(),
                    PlcHandle.CmdPolish.Motor.PlatenRotation.ServoOff.Value(),
                    state => PolishUnit.PlatenMotor.IsServoOn = state,
                    nameof(PlatenMotorServoText));
                break;
            case "HeadRotateMotor":
                PrepareServoCommand(
                    PolishUnit.HeadRotateMotor.IsServoOn,
                    PlcHandle.CmdPolish.Motor.HeadRotation.ServoOn.Handle(),
                    PlcHandle.CmdPolish.Motor.HeadRotation.ServoOn.Value(),
                    PlcHandle.CmdPolish.Motor.HeadRotation.ServoOff.Handle(),
                    PlcHandle.CmdPolish.Motor.HeadRotation.ServoOff.Value(),
                    state => PolishUnit.HeadRotateMotor.IsServoOn = state,
                    nameof(HeadRotateMotorServoText));
                break;
            case "DiskRotateMotor":
                PrepareServoCommand(
                    PolishUnit.DiskRotateMotor.IsServoOn,
                    PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.ServoOn.Handle(),
                    PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.ServoOn.Value(),
                    PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.ServoOff.Handle(),
                    PlcHandle.CmdPolish.Motor.ConditionerHeadRotation.ServoOff.Value(),
                    state => PolishUnit.DiskRotateMotor.IsServoOn = state,
                    nameof(DiskRotateMotorServoText));
                break;
            case "HeadSweepMotor":
                PrepareServoCommand(
                    PolishUnit.HeadSweepMotor.IsServoOn,
                    PlcHandle.CmdPolish.Motor.HeadSweep.ServoOn.Handle(),
                    PlcHandle.CmdPolish.Motor.HeadSweep.ServoOn.Value(),
                    PlcHandle.CmdPolish.Motor.HeadSweep.ServoOff.Handle(),
                    PlcHandle.CmdPolish.Motor.HeadSweep.ServoOff.Value(),
                    state => PolishUnit.HeadSweepMotor.IsServoOn = state,
                    nameof(HeadSweepMotorServoText));
                break;
            case "ConditionSweepMotor":
                PrepareServoCommand(
                    PolishUnit.ConditionSweepMotor.IsServoOn,
                    PlcHandle.CmdPolish.Motor.ConditionerSweep.ServoOn.Handle(),
                    PlcHandle.CmdPolish.Motor.ConditionerSweep.ServoOn.Value(),
                    PlcHandle.CmdPolish.Motor.ConditionerSweep.ServoOff.Handle(),
                    PlcHandle.CmdPolish.Motor.ConditionerSweep.ServoOff.Value(),
                    state => PolishUnit.ConditionSweepMotor.IsServoOn = state,
                    nameof(ConditionSweepMotorServoText));
                break;
            case "HeadLiftMotor":
                PrepareServoCommand(
                    PolishUnit.HeadLiftMotor.IsServoOn,
                    PlcHandle.CmdPolish.Motor.HeadUpDown.ServoOn.Handle(),
                    PlcHandle.CmdPolish.Motor.HeadUpDown.ServoOn.Value(),
                    PlcHandle.CmdPolish.Motor.HeadUpDown.ServoOff.Handle(),
                    PlcHandle.CmdPolish.Motor.HeadUpDown.ServoOff.Value(),
                    state => PolishUnit.HeadLiftMotor.IsServoOn = state,
                    nameof(HeadLiftMotorServoText));
                break;
            case "SlurrySweepMotor":
                PrepareServoCommand(
                    PolishUnit.SlurrySweepMotor.IsServoOn,
                    PlcHandle.CmdPolish.Motor.SlurryArmSweep.ServoOn.Handle(),
                    PlcHandle.CmdPolish.Motor.SlurryArmSweep.ServoOn.Value(),
                    PlcHandle.CmdPolish.Motor.SlurryArmSweep.ServoOff.Handle(),
                    PlcHandle.CmdPolish.Motor.SlurryArmSweep.ServoOff.Value(),
                    state => PolishUnit.SlurrySweepMotor.IsServoOn = state,
                    nameof(SlurrySweepMotorServoText));
                break;
            default:
                Growl.WarningGlobal($"Unknown servo motor: {obj}");
                return;
        }

        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, plcHandle, 1, plcValue);
        if (err != 0)
        {
            Growl.ErrorGlobal($"ServoCommandExecute Error: {err}");
        }
        else
        {
            string stateText = newServoState ? "ON" : "OFF";
            Growl.SuccessGlobal($"ServoCommandExecute: {obj} - Servo {stateText}");
        }
    }

    private void PressCommandExecute(string obj)
    {
        var zones = GetZonesPress();
        foreach (var (name, addr, plcValue, pressAddr, pressValue) in zones)
        {
            if (!string.IsNullOrWhiteSpace(obj) && !string.Equals(name, obj, StringComparison.OrdinalIgnoreCase))
                continue;

            err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, pressAddr, 2, pressValue);
            if (err != 0)
            {
                Growl.ErrorGlobal($"PressCommandSetValueExecute {name} Error: {err}");
            }
            else
            {
                Growl.SuccessGlobal($"PressCommandSetValueExecute: {name}");
            }

            err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, addr, 1, plcValue);
            if (err != 0)
            {
                Growl.ErrorGlobal($"PressCommandExecute {name} Error: {err}");
            }
            else
            {
                Growl.SuccessGlobal($"PressCommandExecute: {name}");
            }
            // sleep for a short duration to allow the press command to take effect before moving to the next zone
            Thread.Sleep(100); 
        }
    }

    private void VentCommandExecute(string obj)
    {
        var zones = GetZonesVent();
        foreach (var (name, addr, plcValue) in zones)
        {
            if (!string.IsNullOrWhiteSpace(obj) && !string.Equals(name, obj, StringComparison.OrdinalIgnoreCase))
                continue;

            err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, addr, 1, plcValue);
            if (err != 0)
            {
                Growl.ErrorGlobal($"VentCommandExecute {name} Error: {err}");
            }
            else
            {
                Growl.SuccessGlobal($"VentCommandExecute: {name}");
            }
            // sleep for 100ms to allow the vent command to take effect before moving on to the next zone
            Thread.Sleep(100);
        }
    }

    private void VacuumCommandExecute(string obj)
    {
        var zones = GetZonesVacuum();
        foreach (var (name, addr, plcValue) in zones)
        {
            if (!string.IsNullOrWhiteSpace(obj) && !string.Equals(name, obj, StringComparison.OrdinalIgnoreCase))
                continue;

            err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, addr, 1, plcValue);
            if (err != 0)
            {
                Growl.ErrorGlobal($"VacuumCommandExecute {name} Error: {err}");
            }
            else
            {
                Growl.SuccessGlobal($"VacuumCommandExecute: {name}");
            }
            // sleep for a short duration to allow the vacuum command to take effect before moving to the next zone
            Thread.Sleep(100);
        }
    }

    private void UpCommandExecute(string obj)
    {
        var plcHandle = 0u;
        var plcValue = new byte[256];
        switch (obj)
        {
            case "PlatenBreakwater":
                plcHandle = PlcHandle.CmdPolish.Cylinder.PlatenBreakwater.Up.Handle();
                plcValue = PlcHandle.CmdPolish.Cylinder.PlatenBreakwater.Up.Value();
                break;
            case "ConditionHead":
                plcHandle = PlcHandle.CmdPolish.Cylinder.ConditionHead.Up.Handle();
                plcValue = PlcHandle.CmdPolish.Cylinder.ConditionHead.Up.Value();
                break;
        }

        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, plcHandle, 1, plcValue);
        if (err == (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.SuccessGlobal($"Cylinder up command execute: {obj}");
        }
        else
        {
            Growl.ErrorGlobal($"Cylinder up command execute {obj} Error: {err}");
        }
    }

    private void DownCommandExecute(string obj)
    {
        var plcHandle = 0u;
        var plcValue = new byte[256];
        switch (obj)
        {
            case "PlatenBreakwater":
                plcHandle = PlcHandle.CmdPolish.Cylinder.PlatenBreakwater.Down.Handle();
                plcValue = PlcHandle.CmdPolish.Cylinder.PlatenBreakwater.Down.Value();
                break;
            case "ConditionHead":
                plcHandle = PlcHandle.CmdPolish.Cylinder.ConditionHead.Down.Handle();
                plcValue = PlcHandle.CmdPolish.Cylinder.ConditionHead.Down.Value();
                break;
        }

        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, plcHandle, 1, plcValue);
        if (err == (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.SuccessGlobal($"Cylinder down command execute: {obj}");
        }
        else
        {
            Growl.ErrorGlobal($"Cylinder down command execute {obj} Error: {err}");
        }
    }

    private void SetCommandExecute(string obj)
    {
        uint pressureValueHandle = PlcHandle.CmdPolish.Cylinder.ConditionHeadPressure.Param.Pressure.ParamHandle();
        byte[] pressureValue = PolishUnit.ZoneCondition.SetPress.Value();
        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, pressureValueHandle, 2, pressureValue);
        if (err == (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.SuccessGlobal($"ConditionHeadPressure set pressure command execute");
        }
        else
        {
            Growl.ErrorGlobal($"ConditionHeadPressure set pressure command execute Error: {err}");
        }

        uint plcHandle = PlcHandle.CmdPolish.Cylinder.ConditionHeadPressure.Set.Handle();
        byte[] plcValue = PlcHandle.CmdPolish.Cylinder.ConditionHeadPressure.Set.Value();
        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, plcHandle, 1, plcValue);
        if (err == (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.SuccessGlobal($"ConditionHeadPressure set command execute");
        }
        else
        {
            Growl.ErrorGlobal($"ConditionHeadPressure set command execute Error: {err}");
        }
    }

    private void ResetCommandExecute(string obj)
    {
        uint plcHandle = PlcHandle.CmdPolish.Cylinder.ConditionHeadPressure.Reset.Handle();
        byte[] plcValue = PlcHandle.CmdPolish.Cylinder.ConditionHeadPressure.Reset.Value();
        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, plcHandle, 1, plcValue);
        if (err == (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.SuccessGlobal($"ConditionHeadPressure reset command execute");
        }
        else
        {
            Growl.ErrorGlobal($"ConditionHeadPressure reset command execute Error: {err}");
        }
    }

    private void AutoCommandExecute(string obj)
    {
        // TODO: Implement auto command logic
        //LoadCommandExecute(null);
        //PolishCommandExecute(null);
        //UnloadCommandExecute(null);
        //DechuckCommandExecute(null);
        //KvStruct.KvStruct.BoolToByte(ref writeBuf, true);
        //err = KHL.KHLConnect(_plcAddr, _plcPort, 3000, KHLSockType.SOCK_TCP, ref sock);
        //err = KHL.KHLWriteDevicesAsBits(sock, KHLDevType.DEV_ZF, 500000, 0, 1, writeBuf);
        //err = KHL.KHLWriteDevicesAsBits(sock, KHLDevType.DEV_B, 0, 100, 1, writeBuf);
        //Growl.InfoGlobal($"Auto command executed for: {obj}");
        //byte[] writeBufTemp = { 0x02, 25 };
        //KHL.KHLConnect(_plcAddr, _plcPort, 3000, KHLSockType.SOCK_TCP, ref sock);
        //KHL.KHLWriteDevicesAsWords(sock, KHLDevType.DEV_W, Convert.ToUInt32(0x010 + this.Id), 1, writeBufTemp);
    }

    private void AbortUnitCommandExecute(string obj)
    {
        uint addr = PlcHandle.CmdPolish.All.Abort.Handle();
        byte[] value = PlcHandle.CmdPolish.All.Abort.Value();
        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, addr, 1, value);
        if (err == (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.InfoGlobal("Abort command executed");
        }
        else
        {
            Growl.ErrorGlobal($"Failed to Abort: {err}");
        }
    }

    private short ConvertOptionToShort(string value)
    {
        short result = Convert.ToInt16(value.Split(":")[0]);
        return result;
    }

    private void InitializeCommandExecute(string obj)
    {
        uint addr = PlcHandle.CmdPolish.All.Initialize.Handle();
        byte[] value = PlcHandle.CmdPolish.All.Initialize.Value();
        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, addr, 1, value);
        if (err == (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.InfoGlobal("Initialize command executed");
        }
        else
        {
            Growl.ErrorGlobal($"Failed to Initialize: {err}");
        }
    }

    private void AutoModeCommandExecute(string obj)
    {
        uint addr = PlcHandle.CmdPolish.All.Mode.Auto.Handle();
        byte[] value = PlcHandle.CmdPolish.All.Mode.Auto.Value();
        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, addr, 1, value);
        if (err == (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.InfoGlobal("Auto mode command executed");
        }
        else
        {
            Growl.ErrorGlobal($"Failed to switch to auto mode: {err}");
        }

        err = KHL.KHLWriteVariable(kvLink.sock, "PLCAutoStart", writeTrue, true);
        if (err == (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.InfoGlobal("Auto start command executed");
            isPLCAutoStartNeedFalse = true;
        }
        else
        {
            Growl.ErrorGlobal($"Failed to auto start: {err}");
        }
    }

    private void ManualModeCommandExecute(string obj)
    {
        uint addr = PlcHandle.CmdPolish.All.Mode.Manual.Handle();
        byte[] value = PlcHandle.CmdPolish.All.Mode.Manual.Value();
        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W, addr, 1, value);
        if (err == (int)KHLUserError.KHL_ERR_NONE)
        {
            Growl.InfoGlobal("Manual mode command executed");
        }
        else
        {
            Growl.ErrorGlobal($"Failed to switch to manual mode: {err}");
        }
    }

    private void AutoRunCommandExecute(string obj)
    {
        err = KHL.KHLWriteVariable(kvLink.sock, "PLCAutoStart", writeTrue, true);
        if (err == (int)KHLUserError.KHL_ERR_NONE)
        {

            Growl.InfoGlobal("Auto start command executed");
            isPLCAutoStartNeedFalse = true;
        }
        else
        {
            Growl.ErrorGlobal($"Failed to auto start: {err}");
        }
    }

    private void LoadCommandExecute(string obj)
    {
        if (obj == "Auto")
        {            
            SelectedLoadRecipe = _configLoadRecipe;
        }
        if (string.IsNullOrEmpty(SelectedLoadRecipe))
        {
            Growl.WarningGlobal("Please Select a Load Recipe First");
            return;
        }
        AssignLoadRetryRecipe();
        try
        {
            var recipeContent = LoadRecipeContent(RecipeTemplateRegistry.LoadRecipeFolder, SelectedLoadRecipe);
            if (recipeContent != null)
            {
                // TODO: Use the recipe content to execute load operation
                KST.stLoadRecipeFormat loadRecipe = new KST.stLoadRecipeFormat();
                for (int i = 0; i < recipeContent.Data?[0].Steps.Count; i++)
                {
                    loadRecipe.ProcessType[i] = GetRealVal<short>(recipeContent, "ProcessType", i);
                    loadRecipe.StepControl[i] = GetRealVal<short>(recipeContent, "StepControl", i);
                    loadRecipe.ZoneMinPressure[i] = GetRealVal<float>(recipeContent, "ZoneMinPressure", i);
                    loadRecipe.ZoneMaxPressure[i] = GetRealVal<float>(recipeContent, "ZoneMaxPressure", i);
                    loadRecipe.MaxTime[i] = GetRealVal<float>(recipeContent, "MaxTime", i);
                    loadRecipe.Zone1Type[i] = GetRealVal<short>(recipeContent, "Zone1Type", i);
                    loadRecipe.Zone2Type[i] = GetRealVal<short>(recipeContent, "Zone2Type", i);
                    loadRecipe.Zone3Type[i] = GetRealVal<short>(recipeContent, "Zone3Type", i);
                    loadRecipe.Zone4Type[i] = GetRealVal<short>(recipeContent, "Zone4Type", i);
                    loadRecipe.Zone5Type[i] = GetRealVal<short>(recipeContent, "Zone5Type", i);
                    loadRecipe.Zone6Type[i] = GetRealVal<short>(recipeContent, "Zone6Type", i);
                    loadRecipe.Zone7Type[i] = GetRealVal<short>(recipeContent, "Zone7Type", i);
                    loadRecipe.Zone8Type[i] = GetRealVal<short>(recipeContent, "Zone8Type", i);
                    loadRecipe.RRType[i] = GetRealVal<short>(recipeContent, "RRType", i);
                    loadRecipe.Zone1Pressure[i] = GetRealVal<float>(recipeContent, "Zone1Pressure", i);
                    loadRecipe.Zone2Pressure[i] = GetRealVal<float>(recipeContent, "Zone2Pressure", i);
                    loadRecipe.Zone3Pressure[i] = GetRealVal<float>(recipeContent, "Zone3Pressure", i);
                    loadRecipe.Zone4Pressure[i] = GetRealVal<float>(recipeContent, "Zone4Pressure", i);
                    loadRecipe.Zone5Pressure[i] = GetRealVal<float>(recipeContent, "Zone5Pressure", i);
                    loadRecipe.Zone6Pressure[i] = GetRealVal<float>(recipeContent, "Zone6Pressure", i);
                    loadRecipe.Zone7Pressure[i] = GetRealVal<float>(recipeContent, "Zone7Pressure", i);
                    loadRecipe.Zone8Pressure[i] = GetRealVal<float>(recipeContent, "Zone8Pressure", i);
                    loadRecipe.ZoneRRPressure[i] = GetRealVal<float>(recipeContent, "ZoneRRPressure", i);
                    loadRecipe.HeadMiddleWash[i] = GetRealVal<short>(recipeContent, "HeadMiddleWash", i);
                    loadRecipe.HeadExternalWash[i] = GetRealVal<short>(recipeContent, "HeadExternalWash", i);
                }

                KST.stLoadRecipeFormat_ToByte(ref writeBuf, loadRecipe, 1, 0);
                if (KHLConnectAndOperation("structLoadRecipe", writeBuf) == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"Assign Load Recipe {SelectedLoadRecipe} Successful");
                    if (obj == "Chamber")
                    {
                        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W,
                            PlcHandle.CmdPolish.Process.SingleProcessStart.LoadStart.Handle(), 1,
                            PlcHandle.CmdPolish.Process.SingleProcessStart.LoadStart.Value());
                    }
                }
                else
                {
                    Growl.ErrorGlobal($"Assign Load Recipe {SelectedLoadRecipe} Failed:{err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find Load Recipe: {SelectedLoadRecipe}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error Assign Load Recipe: {ex.Message}");
            Logger.Error("Error Assign Load Recipe", ex);
        }
    }

    private void PolishCommandExecute(string obj)
    {
        List<string> listHeadSweepRecipe = new List<string>();
        listHeadSweepRecipe.add("None");
        List<string> listConditionRecipe = new List<string>();
        listConditionRecipe.add("None");

        if (string.IsNullOrEmpty(SelectedPolishRecipe))
        {
            Growl.WarningGlobal("Please select a Polish recipe first");
            return;
        }

        try
        {
            KST.stEPD stEPD = new KST.stEPD();
            err = KHL.KHLReadVariable(kvLink.sock, "structEPD", readBuf, true);
            KST.ByteTo_stEPD(ref stEPD, readBuf);
            stEPD.newWafer = true;
            stEPD.posCount = 200;
            stEPD.waferIndex = Convert.ToUInt16(this.waferIndex);
            byte[] polishRecipeName = new byte[202];
            polishRecipeName = Encoding.UTF8.GetBytes(SelectedPolishRecipe);
            for (int i = 0; i < stEPD.PolishRecipeName.Length; i++)
            {
                if (i < polishRecipeName.Length)
                {
                    stEPD.PolishRecipeName[i] = polishRecipeName[i];
                }
                else
                {
                    stEPD.PolishRecipeName[i] = 0x0;
                }
            }
            //KST.StringToByte(ref stEPD.PolishRecipeName, polishRecipeName, 1, 0, SelectedPolishRecipe.length());
            KST.stEPD_ToByte(ref writeBuf, stEPD);
            err = KHL.KHLWriteVariable(kvLink.sock, "structEPD", writeBuf, true);
            Thread.Sleep(50);

            var recipeContent = LoadRecipeContent(RecipeTemplateRegistry.PolishRecipeFolder, SelectedPolishRecipe);


            if (recipeContent != null)
            {
                //下发Polish Recipe之前，根据Process Type的选项，给ZF10004写入0或者1，如果选择64，写1，其余写0
                short processType = GetRealVal<short>(recipeContent, "ProcessType", 0);
                byte[] data = new byte[256];
                if (processType == 64) KST.IntToByte(ref data, 1);
                else KST.IntToByte(ref data, 0);
                var err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_ZF, 10004, 1, data);

                KST.stPolishRecipeFormat PolishRecipe = new KST.stPolishRecipeFormat();
                for (int i = 0; i < recipeContent.Data?[0].Steps.Count; i++)
                {
                    PolishRecipe.ProcessType[i] = GetRealVal<short>(recipeContent, "ProcessType", i);
                    PolishRecipe.ProcessTime[i] = GetRealVal<float>(recipeContent, "ProcessTime", i);                    
                    PolishRecipe.JumpStepNo[i] = GetRealVal<short>(recipeContent, "JumpStepNo", i);
                    PolishRecipe.PlatenRotation[i] = GetRealVal<float>(recipeContent, "PlatenRotation", i);
                    PolishRecipe.HeadRotation[i] = GetRealVal<float>(recipeContent, "HeadRotation", i);
                    PolishRecipe.HeadSweepRecipe[i] = AssignHeadSweepRecipe(ref listHeadSweepRecipe, GetRealVal<string>(recipeContent, "HeadSweepRecipe", i)!);
                    PolishRecipe.Zone1Type[i] = GetRealVal<short>(recipeContent, "Zone1Type", i);
                    PolishRecipe.Zone2Type[i] = GetRealVal<short>(recipeContent, "Zone2Type", i);
                    PolishRecipe.Zone3Type[i] = GetRealVal<short>(recipeContent, "Zone3Type", i);
                    PolishRecipe.Zone4Type[i] = GetRealVal<short>(recipeContent, "Zone4Type", i);
                    PolishRecipe.Zone5Type[i] = GetRealVal<short>(recipeContent, "Zone5Type", i);
                    PolishRecipe.Zone6Type[i] = GetRealVal<short>(recipeContent, "Zone6Type", i);
                    PolishRecipe.Zone7Type[i] = GetRealVal<short>(recipeContent, "Zone7Type", i);
                    PolishRecipe.Zone8Type[i] = GetRealVal<short>(recipeContent, "Zone8Type", i);
                    PolishRecipe.TopRingType[i] = GetRealVal<short>(recipeContent, "TopRingType", i);
                    PolishRecipe.Zone1Pressure[i] = GetRealVal<float>(recipeContent, "Zone1Pressure", i);
                    PolishRecipe.Zone2Pressure[i] = GetRealVal<float>(recipeContent, "Zone2Pressure", i);
                    PolishRecipe.Zone3Pressure[i] = GetRealVal<float>(recipeContent, "Zone3Pressure", i);
                    PolishRecipe.Zone4Pressure[i] = GetRealVal<float>(recipeContent, "Zone4Pressure", i);
                    PolishRecipe.Zone5Pressure[i] = GetRealVal<float>(recipeContent, "Zone5Pressure", i);
                    PolishRecipe.Zone6Pressure[i] = GetRealVal<float>(recipeContent, "Zone6Pressure", i);
                    PolishRecipe.Zone7Pressure[i] = GetRealVal<float>(recipeContent, "Zone7Pressure", i);
                    PolishRecipe.Zone8Pressure[i] = GetRealVal<float>(recipeContent, "Zone8Pressure", i);
                    PolishRecipe.ZoneRRPressure[i] = GetRealVal<float>(recipeContent, "ZoneRRPressure", i);
                    PolishRecipe.ConditionRecipe[i] = AssignConditionRecipe(ref listConditionRecipe, GetRealVal<string>(recipeContent, "ConditionRecipe", i)!);
                    PolishRecipe.PlantWash[i] = GetRealVal<short>(recipeContent, "PlantWash", i);
                    PolishRecipe.SlurryLine1Type[i] = GetRealVal<short>(recipeContent, "SlurryLine1Type", i);
                    PolishRecipe.SlurryLine2Type[i] = GetRealVal<short>(recipeContent, "SlurryLine2Type", i);
                    PolishRecipe.SlurryLine3Type[i] = GetRealVal<short>(recipeContent, "SlurryLine3Type", i);
                    PolishRecipe.SlurryLine1Flow[i] = GetRealVal<float>(recipeContent, "SlurryLine1Flow", i);
                    PolishRecipe.SlurryLine2Flow[i] = GetRealVal<float>(recipeContent, "SlurryLine2Flow", i);
                    PolishRecipe.SlurryLine3Flow[i] = GetRealVal<float>(recipeContent, "SlurryLine3Flow", i);
                    PolishRecipe.DIW[i] = GetRealVal<short>(recipeContent, "DIW", i);
                    PolishRecipe.PolishHeight[i] = GetRealVal<float>(recipeContent, "PolishHeight", i);
                    PolishRecipe.SlurryStartPosition[i] = GetRealVal<float>(recipeContent, "SlurryStartPosition", i);
                    PolishRecipe.SlurryEndPosition[i] = GetRealVal<float>(recipeContent, "SlurryEndPosition", i);
                    PolishRecipe.SlurrySweepVelocity[i] = GetRealVal<float>(recipeContent, "SlurrySweepVelocity", i);
                }

                KST.stPolishRecipeFormat_ToByte(ref writeBuf, PolishRecipe, 1, 0);
                err = KHLConnectAndOperation("structPolishRecipe", writeBuf);
                if (err == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"Assign Polish Recipe {SelectedPolishRecipe} Successful");
                    if (obj == "Chamber")
                    {
                        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W,
                            PlcHandle.CmdPolish.Process.SingleProcessStart.PolishStart.Handle(), 1,
                            PlcHandle.CmdPolish.Process.SingleProcessStart.PolishStart.Value());
                    }
                }
                else
                {
                    Growl.ErrorGlobal($"Assign Polish Recipe {SelectedPolishRecipe} Failed:{err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find Polish Recipe: {SelectedPolishRecipe}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error Assign Polish Recipe: {ex.Message}");
            Logger.Error("Error Assign Polish Recipe", ex);
        }
    }

    private void BreakInCommandExecute(string obj)
    {
        List<string> listConditionSweepRecipe = new List<string>();
        listConditionSweepRecipe.add("None");

        if (string.IsNullOrEmpty(SelectedExsituConditionRecipe))
        {
            Growl.WarningGlobal("Please select a Condition recipe first");
            return;
        }

        try
        {
            var recipeContent = LoadRecipeContent(RecipeTemplateRegistry.ExsituConditionRecipeFolder, SelectedExsituConditionRecipe);
            if (recipeContent != null)
            {
                KST.stExsituConditionRecipeFormat exsituConditionRecipe = new KST.stExsituConditionRecipeFormat();
                for (int i = 0; i < recipeContent.Data?[0].Steps.Count; i++)
                {
                    exsituConditionRecipe.ProcessType[i] = GetRealVal<short>(recipeContent, "ProcessType", i);
                    exsituConditionRecipe.ProcessTime[i] = GetRealVal<float>(recipeContent, "ProcessTime", i);
                    exsituConditionRecipe.InSitu[i] = GetRealVal<short>(recipeContent, "InSitu", i);
                    exsituConditionRecipe.PlatenRotation[i] = GetRealVal<float>(recipeContent, "PlatenRotation", i);
                    exsituConditionRecipe.ConditionerHeadRotation[i] = GetRealVal<float>(recipeContent, "ConditionerHeadRotation", i);
                    exsituConditionRecipe.ConditionPressure[i] = GetRealVal<float>(recipeContent, "ConditionPressure", i);
                    exsituConditionRecipe.ConditionSweepRecipe[i] = AssignExsituConditionSweepRecipe(ref listConditionSweepRecipe, GetRealVal<string>(recipeContent, "ConditionSweepRecipe", i)!);
                    exsituConditionRecipe.PlatenWashSelection[i] = GetRealVal<short>(recipeContent, "PlatenWashSelection", i);
                    exsituConditionRecipe.SlurryLine1Type[i] = GetRealVal<short>(recipeContent, "SlurryLine1Type", i);
                    exsituConditionRecipe.SlurryLine2Type[i] = GetRealVal<short>(recipeContent, "SlurryLine2Type", i);
                    exsituConditionRecipe.SlurryLine3Type[i] = GetRealVal<short>(recipeContent, "SlurryLine3Type", i);
                    exsituConditionRecipe.SlurryLine1Flow[i] = GetRealVal<float>(recipeContent, "SlurryLine1Flow", i);
                    exsituConditionRecipe.SlurryLine2Flow[i] = GetRealVal<float>(recipeContent, "SlurryLine2Flow", i);
                    exsituConditionRecipe.SlurryLine3Flow[i] = GetRealVal<float>(recipeContent, "SlurryLine3Flow", i);
                    exsituConditionRecipe.SweepStopTimeType[i] = GetRealVal<short>(recipeContent, "SweepStopTimeType", i);
                    exsituConditionRecipe.Ratio[i] = GetRealVal<short>(recipeContent, "Ratio", i);
                }

                KST.stExsituConditionRecipeFormat_ToByte(ref writeBuf, exsituConditionRecipe, 1, 0);
                err = KHLConnectAndOperation("structExsituRecipe", writeBuf);
                if (err == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"Assign BreadIn Recipe {SelectedExsituConditionRecipe} Successful");

                    if (obj == "Chamber")
                    {
                        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W,
                            PlcHandle.CmdPolish.Process.SingleProcessStart.ExsituStart.Handle(), 1,
                            PlcHandle.CmdPolish.Process.SingleProcessStart.ExsituStart.Value());
                    }
                }
                else
                {
                    Growl.ErrorGlobal($"Assign BreadIn Recipe {SelectedExsituConditionRecipe} Successful:{err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find BreakIn Recipe: {SelectedExsituConditionRecipe}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error Assign BreakIn Recipe: {ex.Message}");
            Logger.Error("Error Assign BreakIn Recipe", ex);
        }
    }

    private void UnloadCommandExecute(string obj)
    {
        if (obj == "Auto")
        {
            SelectedUnloadRecipe = _configUnLoadRecipe;
        }
        if (string.IsNullOrEmpty(SelectedUnloadRecipe))
        {
            Growl.WarningGlobal("Please select an Unload recipe first");
            return;
        }
        AssignUnloadRetryRecipe();
        try
        {
            var recipeContent = LoadRecipeContent(RecipeTemplateRegistry.UnloadRecipeFolder, SelectedUnloadRecipe);
            if (recipeContent != null)
            {
                // TODO: Use the recipe content to execute load operation
                KST.stUnloadRecipeFormat unloadRecipe = new KST.stUnloadRecipeFormat();
                for (int i = 0; i < recipeContent.Data?[0].Steps.Count; i++)
                {
                    unloadRecipe.ProcessType[i] = GetRealVal<short>(recipeContent, "ProcessType", i);
                    unloadRecipe.StepControl[i] = GetRealVal<short>(recipeContent, "StepControl", i);
                    unloadRecipe.ZoneMinPressure[i] = GetRealVal<float>(recipeContent, "ZoneMinPressure", i);
                    unloadRecipe.ZoneMaxPressure[i] = GetRealVal<float>(recipeContent, "ZoneMaxPressure", i);
                    unloadRecipe.MaxTime[i] = GetRealVal<float>(recipeContent, "MaxTime", i);
                    unloadRecipe.Zone1Type[i] = GetRealVal<short>(recipeContent, "Zone1Type", i);
                    unloadRecipe.Zone2Type[i] = GetRealVal<short>(recipeContent, "Zone2Type", i);
                    unloadRecipe.Zone3Type[i] = GetRealVal<short>(recipeContent, "Zone3Type", i);
                    unloadRecipe.Zone4Type[i] = GetRealVal<short>(recipeContent, "Zone4Type", i);
                    unloadRecipe.Zone5Type[i] = GetRealVal<short>(recipeContent, "Zone5Type", i);
                    unloadRecipe.Zone6Type[i] = GetRealVal<short>(recipeContent, "Zone6Type", i);
                    unloadRecipe.Zone7Type[i] = GetRealVal<short>(recipeContent, "Zone7Type", i);
                    unloadRecipe.Zone8Type[i] = GetRealVal<short>(recipeContent, "Zone8Type", i);
                    unloadRecipe.RRType[i] = GetRealVal<short>(recipeContent, "RRType", i);
                    unloadRecipe.Zone1Pressure[i] = GetRealVal<float>(recipeContent, "Zone1Pressure", i);
                    unloadRecipe.Zone2Pressure[i] = GetRealVal<float>(recipeContent, "Zone2Pressure", i);
                    unloadRecipe.Zone3Pressure[i] = GetRealVal<float>(recipeContent, "Zone3Pressure", i);
                    unloadRecipe.Zone4Pressure[i] = GetRealVal<float>(recipeContent, "Zone4Pressure", i);
                    unloadRecipe.Zone5Pressure[i] = GetRealVal<float>(recipeContent, "Zone5Pressure", i);
                    unloadRecipe.Zone6Pressure[i] = GetRealVal<float>(recipeContent, "Zone6Pressure", i);
                    unloadRecipe.Zone7Pressure[i] = GetRealVal<float>(recipeContent, "Zone7Pressure", i);
                    unloadRecipe.Zone8Pressure[i] = GetRealVal<float>(recipeContent, "Zone8Pressure", i);
                    unloadRecipe.ZoneRRPressure[i] = GetRealVal<float>(recipeContent, "ZoneRRPressure", i);
                    unloadRecipe.HeadMiddleWash[i] = GetRealVal<short>(recipeContent, "HeadMiddleWash", i);
                    unloadRecipe.HeadExternalWash[i] = GetRealVal<short>(recipeContent, "HeadExternalWash", i);
                    unloadRecipe.HeadRotation[i] = GetRealVal<float>(recipeContent, "HeadRotation", i);
                    unloadRecipe.BlowWFSelect[i] = GetRealVal<short>(recipeContent, "BlowWFSelect", i);
                }

                //KST.stLoadRecipeFormat_ToByte(ref writeBuf, loadRecipe, 1, 0);
                KST.stUnloadRecipeFormat_ToByte(ref writeBuf, unloadRecipe, 1, 0);
                err = KHLConnectAndOperation("structUnloadRecipe", writeBuf);
                if (KHLConnectAndOperation("structUnloadRecipe", writeBuf) == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"Assign Unload Recipe Successful: {SelectedUnloadRecipe}");
                    if (obj == "Chamber")
                    {
                        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W,
                            PlcHandle.CmdPolish.Process.SingleProcessStart.UnloadStart.Handle(), 1,
                            PlcHandle.CmdPolish.Process.SingleProcessStart.UnloadStart.Value());
                    }
                }
                else
                {
                    Growl.ErrorGlobal($"Assign Unload Recipe Failed: {SelectedUnloadRecipe}:{err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find Unload Recipe: {SelectedUnloadRecipe}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error Assign Unload Recipe: {ex.Message}");
            Logger.Error("Error Assign Unload Recipe", ex);
        }
    }

    private void DechuckCommandExecute(string obj)
    {
        if (obj == "Auto")
        {
            SelectedDechuckRecipe = _configDechukRecipe;
        }
        if (string.IsNullOrEmpty(SelectedDechuckRecipe))
        {
            Growl.WarningGlobal("Please select an dechuck recipe first");
            return;
        }
        AssignDechuckRetryRecipe();
        try
        {
            var recipeContent = LoadRecipeContent(RecipeTemplateRegistry.DechuckRecipeFolder, SelectedDechuckRecipe);
            if (recipeContent != null)
            {
                // TODO: Use the recipe content to execute load operation
                KST.stChuckRecipeFormat dechuckRecipe = new KST.stChuckRecipeFormat();
                for (int i = 0; i < recipeContent.Data?[0].Steps.Count; i++)
                {
                    dechuckRecipe.ProcessType[i] = GetRealVal<short>(recipeContent, "ProcessType", i);
                    dechuckRecipe.StepControl[i] = GetRealVal<short>(recipeContent, "StepControl", i);
                    dechuckRecipe.ZoneMinPressure[i] = GetRealVal<float>(recipeContent, "ZoneMinPressure", i);
                    dechuckRecipe.ZoneMaxPressure[i] = GetRealVal<float>(recipeContent, "ZoneMaxPressure", i);
                    dechuckRecipe.MaxTime[i] = GetRealVal<float>(recipeContent, "MaxTime", i);
                    dechuckRecipe.PlatenRotation[i] = GetRealVal<float>(recipeContent, "PlatenRotation", i);
                    dechuckRecipe.HeadRotation[i] = GetRealVal<float>(recipeContent, "HeadRotation", i);
                    dechuckRecipe.Zone1Type[i] = GetRealVal<short>(recipeContent, "Zone1Type", i);
                    dechuckRecipe.Zone2Type[i] = GetRealVal<short>(recipeContent, "Zone2Type", i);
                    dechuckRecipe.Zone3Type[i] = GetRealVal<short>(recipeContent, "Zone3Type", i);
                    dechuckRecipe.Zone4Type[i] = GetRealVal<short>(recipeContent, "Zone4Type", i);
                    dechuckRecipe.Zone5Type[i] = GetRealVal<short>(recipeContent, "Zone5Type", i);
                    dechuckRecipe.Zone6Type[i] = GetRealVal<short>(recipeContent, "Zone6Type", i);
                    dechuckRecipe.Zone7Type[i] = GetRealVal<short>(recipeContent, "Zone7Type", i);
                    dechuckRecipe.Zone8Type[i] = GetRealVal<short>(recipeContent, "Zone8Type", i);
                    dechuckRecipe.ZoneRRType[i] = GetRealVal<short>(recipeContent, "ZoneRRType", i);
                    dechuckRecipe.Zone1Pressure[i] = GetRealVal<float>(recipeContent, "Zone1Pressure", i);
                    dechuckRecipe.Zone2Pressure[i] = GetRealVal<float>(recipeContent, "Zone2Pressure", i);
                    dechuckRecipe.Zone3Pressure[i] = GetRealVal<float>(recipeContent, "Zone3Pressure", i);
                    dechuckRecipe.Zone4Pressure[i] = GetRealVal<float>(recipeContent, "Zone4Pressure", i);
                    dechuckRecipe.Zone5Pressure[i] = GetRealVal<float>(recipeContent, "Zone5Pressure", i);
                    dechuckRecipe.Zone6Pressure[i] = GetRealVal<float>(recipeContent, "Zone6Pressure", i);
                    dechuckRecipe.Zone7Pressure[i] = GetRealVal<float>(recipeContent, "Zone7Pressure", i);
                    dechuckRecipe.Zone8Pressure[i] = GetRealVal<float>(recipeContent, "Zone8Pressure", i);
                    dechuckRecipe.ZoneRRPressure[i] = GetRealVal<float>(recipeContent, "ZoneRRPressure", i);
                    dechuckRecipe.PlantWash[i] = GetRealVal<short>(recipeContent, "PlantWash", i);
                    dechuckRecipe.DechuckHeight[i] = GetRealVal<float>(recipeContent, "DechuckHeight", i);
                }

                KST.stChuckRecipeFormat_ToByte(ref writeBuf, dechuckRecipe, 1, 0);
                err = KHLConnectAndOperation("structDechuckRecipe", writeBuf);
                if (KHLConnectAndOperation("structDechuckRecipe", writeBuf) == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"Assign Dechuck Recipe Successful: {SelectedDechuckRecipe}");
                    if (obj == "Chamber")
                    {
                        err = KHL.KHLWriteDevicesAsWords(kvLink.sock, KHLDevType.DEV_W,
                            PlcHandle.CmdPolish.Process.SingleProcessStart.DechuckStart.Handle(), 1,
                            PlcHandle.CmdPolish.Process.SingleProcessStart.DechuckStart.Value());
                    }
                }
                else
                {
                    Growl.ErrorGlobal($"Assign Dechuck Recipe Failed: {SelectedDechuckRecipe}:{err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find Dechuck Recipe: {SelectedDechuckRecipe}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error Assign Dechuck Recipe: {ex.Message}");
            Logger.Error("Error Assign Dechuck Recipe", ex);
        }
    }

    private void AssignLoadRetryRecipe()
    {
        if (string.IsNullOrEmpty(SelectedLoadRetryRecipe))
        {
            Growl.WarningGlobal("Please Select a Load Retry Recipe First");
            return;
        }
        try
        {
            var recipeContent = LoadRecipeContent(RecipeTemplateRegistry.LoadRetryRecipeFolder, SelectedLoadRetryRecipe);
            if (recipeContent != null)  
            {
                KST.stLoadRecipeFormat loadRecipe = new KST.stLoadRecipeFormat();
                for (int i = 0; i < recipeContent.Data?[0].Steps.Count; i++)
                {
                    loadRecipe.ProcessType[i] = GetRealVal<short>(recipeContent, "ProcessType", i);
                    loadRecipe.StepControl[i] = GetRealVal<short>(recipeContent, "StepControl", i);
                    loadRecipe.ZoneMinPressure[i] = GetRealVal<float>(recipeContent, "ZoneMinPressure", i);
                    loadRecipe.ZoneMaxPressure[i] = GetRealVal<float>(recipeContent, "ZoneMaxPressure", i);
                    loadRecipe.MaxTime[i] = GetRealVal<float>(recipeContent, "MaxTime", i);
                    loadRecipe.Zone1Type[i] = GetRealVal<short>(recipeContent, "Zone1Type", i);
                    loadRecipe.Zone2Type[i] = GetRealVal<short>(recipeContent, "Zone2Type", i);
                    loadRecipe.Zone3Type[i] = GetRealVal<short>(recipeContent, "Zone3Type", i);
                    loadRecipe.Zone4Type[i] = GetRealVal<short>(recipeContent, "Zone4Type", i);
                    loadRecipe.Zone5Type[i] = GetRealVal<short>(recipeContent, "Zone5Type", i);
                    loadRecipe.Zone6Type[i] = GetRealVal<short>(recipeContent, "Zone6Type", i);
                    loadRecipe.Zone7Type[i] = GetRealVal<short>(recipeContent, "Zone7Type", i);
                    loadRecipe.Zone8Type[i] = GetRealVal<short>(recipeContent, "Zone8Type", i);
                    loadRecipe.RRType[i] = GetRealVal<short>(recipeContent, "RRType", i);
                    loadRecipe.Zone1Pressure[i] = GetRealVal<float>(recipeContent, "Zone1Pressure", i);
                    loadRecipe.Zone2Pressure[i] = GetRealVal<float>(recipeContent, "Zone2Pressure", i);
                    loadRecipe.Zone3Pressure[i] = GetRealVal<float>(recipeContent, "Zone3Pressure", i);
                    loadRecipe.Zone4Pressure[i] = GetRealVal<float>(recipeContent, "Zone4Pressure", i);
                    loadRecipe.Zone5Pressure[i] = GetRealVal<float>(recipeContent, "Zone5Pressure", i);
                    loadRecipe.Zone6Pressure[i] = GetRealVal<float>(recipeContent, "Zone6Pressure", i);
                    loadRecipe.Zone7Pressure[i] = GetRealVal<float>(recipeContent, "Zone7Pressure", i);
                    loadRecipe.Zone8Pressure[i] = GetRealVal<float>(recipeContent, "Zone8Pressure", i);
                    loadRecipe.ZoneRRPressure[i] = GetRealVal<float>(recipeContent, "ZoneRRPressure", i);
                    loadRecipe.HeadMiddleWash[i] = GetRealVal<short>(recipeContent, "HeadMiddleWash", i);
                    loadRecipe.HeadExternalWash[i] = GetRealVal<short>(recipeContent, "HeadExternalWash", i);
                }

                KST.stLoadRecipeFormat_ToByte(ref writeBuf, loadRecipe, 1, 0);
                if (KHLConnectAndOperation("structLoadRetryRecipe", writeBuf) == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"Assign Load Retry Recipe {SelectedLoadRetryRecipe} Successful");
                }
                else
                {
                    Growl.ErrorGlobal($"Assign Load Retry Recipe {SelectedLoadRetryRecipe} Failed:{err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find Load Retry Recipe: {SelectedLoadRetryRecipe}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error Assign Load Retry Recipe: {ex.Message}");
            Logger.Error("Error Assign Load Retry Recipe", ex);
        }
    }

    private void AssignUnloadRetryRecipe()
    {
        if (string.IsNullOrEmpty(SelectedUnloadRetryRecipe))
        {
            Growl.WarningGlobal("Please Select an Unload Retry Recipe First");
            return;
        }
        try
        {
            var recipeContent = LoadRecipeContent(RecipeTemplateRegistry.UnloadRetryRecipeFolder, SelectedUnloadRetryRecipe);
            if (recipeContent != null)
            {
                KST.stUnloadRecipeFormat unloadRecipe = new KST.stUnloadRecipeFormat();
                for (int i = 0; i < recipeContent.Data?[0].Steps.Count; i++)
                {
                    unloadRecipe.ProcessType[i] = GetRealVal<short>(recipeContent, "ProcessType", i);
                    unloadRecipe.StepControl[i] = GetRealVal<short>(recipeContent, "StepControl", i);
                    unloadRecipe.ZoneMinPressure[i] = GetRealVal<float>(recipeContent, "ZoneMinPressure", i);
                    unloadRecipe.ZoneMaxPressure[i] = GetRealVal<float>(recipeContent, "ZoneMaxPressure", i);
                    unloadRecipe.MaxTime[i] = GetRealVal<float>(recipeContent, "MaxTime", i);
                    unloadRecipe.Zone1Type[i] = GetRealVal<short>(recipeContent, "Zone1Type", i);
                    unloadRecipe.Zone2Type[i] = GetRealVal<short>(recipeContent, "Zone2Type", i);
                    unloadRecipe.Zone3Type[i] = GetRealVal<short>(recipeContent, "Zone3Type", i);
                    unloadRecipe.Zone4Type[i] = GetRealVal<short>(recipeContent, "Zone4Type", i);
                    unloadRecipe.Zone5Type[i] = GetRealVal<short>(recipeContent, "Zone5Type", i);
                    unloadRecipe.Zone6Type[i] = GetRealVal<short>(recipeContent, "Zone6Type", i);
                    unloadRecipe.Zone7Type[i] = GetRealVal<short>(recipeContent, "Zone7Type", i);
                    unloadRecipe.Zone8Type[i] = GetRealVal<short>(recipeContent, "Zone8Type", i);
                    unloadRecipe.RRType[i] = GetRealVal<short>(recipeContent, "RRType", i);
                    unloadRecipe.Zone1Pressure[i] = GetRealVal<float>(recipeContent, "Zone1Pressure", i);
                    unloadRecipe.Zone2Pressure[i] = GetRealVal<float>(recipeContent, "Zone2Pressure", i);
                    unloadRecipe.Zone3Pressure[i] = GetRealVal<float>(recipeContent, "Zone3Pressure", i);
                    unloadRecipe.Zone4Pressure[i] = GetRealVal<float>(recipeContent, "Zone4Pressure", i);
                    unloadRecipe.Zone5Pressure[i] = GetRealVal<float>(recipeContent, "Zone5Pressure", i);
                    unloadRecipe.Zone6Pressure[i] = GetRealVal<float>(recipeContent, "Zone6Pressure", i);
                    unloadRecipe.Zone7Pressure[i] = GetRealVal<float>(recipeContent, "Zone7Pressure", i);
                    unloadRecipe.Zone8Pressure[i] = GetRealVal<float>(recipeContent, "Zone8Pressure", i);
                    unloadRecipe.ZoneRRPressure[i] = GetRealVal<float>(recipeContent, "ZoneRRPressure", i);
                    unloadRecipe.HeadMiddleWash[i] = GetRealVal<short>(recipeContent, "HeadMiddleWash", i);
                    unloadRecipe.HeadExternalWash[i] = GetRealVal<short>(recipeContent, "HeadExternalWash", i);
                    unloadRecipe.HeadRotation[i] = GetRealVal<float>(recipeContent, "HeadRotation", i);
                    unloadRecipe.BlowWFSelect[i] = GetRealVal<short>(recipeContent, "BlowWFSelect", i);
                }

                KST.stUnloadRecipeFormat_ToByte(ref writeBuf, unloadRecipe, 1, 0);
                if (KHLConnectAndOperation("structUnloadRetryRecipe", writeBuf) == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"Assign Unload Retry Recipe {SelectedUnloadRetryRecipe} Successful");
                }
                else
                {
                    Growl.ErrorGlobal($"Assign Unload Retry Recipe {SelectedUnloadRetryRecipe} Failed:{err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find Unload Retry Recipe: {SelectedUnloadRetryRecipe}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error Assign Unload Retry Recipe: {ex.Message}");
            Logger.Error("Error Assign Unload Retry Recipe", ex);
        }
    }

    private void AssignDechuckRetryRecipe()
    {
        if (string.IsNullOrEmpty(SelectedDechuckRetryRecipe))
        {
            Growl.WarningGlobal("Please Select a Dechuck Retry Recipe First");
            return;
        }
        try
        {
            var recipeContent = LoadRecipeContent(RecipeTemplateRegistry.DechuckRetryRecipeFolder, SelectedDechuckRetryRecipe);
            if (recipeContent != null)
            {
                KST.stChuckRecipeFormat dechuckRecipe = new KST.stChuckRecipeFormat();
                for (int i = 0; i < recipeContent.Data?[0].Steps.Count; i++)
                {
                    dechuckRecipe.ProcessType[i] = GetRealVal<short>(recipeContent, "ProcessType", i);
                    dechuckRecipe.StepControl[i] = GetRealVal<short>(recipeContent, "StepControl", i);
                    dechuckRecipe.ZoneMinPressure[i] = GetRealVal<float>(recipeContent, "ZoneMinPressure", i);
                    dechuckRecipe.ZoneMaxPressure[i] = GetRealVal<float>(recipeContent, "ZoneMaxPressure", i);
                    dechuckRecipe.MaxTime[i] = GetRealVal<float>(recipeContent, "MaxTime", i);
                    dechuckRecipe.PlatenRotation[i] = GetRealVal<float>(recipeContent, "PlatenRotation", i);
                    dechuckRecipe.HeadRotation[i] = GetRealVal<float>(recipeContent, "HeadRotation", i);
                    dechuckRecipe.Zone1Type[i] = GetRealVal<short>(recipeContent, "Zone1Type", i);
                    dechuckRecipe.Zone2Type[i] = GetRealVal<short>(recipeContent, "Zone2Type", i);
                    dechuckRecipe.Zone3Type[i] = GetRealVal<short>(recipeContent, "Zone3Type", i);
                    dechuckRecipe.Zone4Type[i] = GetRealVal<short>(recipeContent, "Zone4Type", i);
                    dechuckRecipe.Zone5Type[i] = GetRealVal<short>(recipeContent, "Zone5Type", i);
                    dechuckRecipe.Zone6Type[i] = GetRealVal<short>(recipeContent, "Zone6Type", i);
                    dechuckRecipe.Zone7Type[i] = GetRealVal<short>(recipeContent, "Zone7Type", i);
                    dechuckRecipe.Zone8Type[i] = GetRealVal<short>(recipeContent, "Zone8Type", i);
                    dechuckRecipe.ZoneRRType[i] = GetRealVal<short>(recipeContent, "ZoneRRType", i);
                    dechuckRecipe.Zone1Pressure[i] = GetRealVal<float>(recipeContent, "Zone1Pressure", i);
                    dechuckRecipe.Zone2Pressure[i] = GetRealVal<float>(recipeContent, "Zone2Pressure", i);
                    dechuckRecipe.Zone3Pressure[i] = GetRealVal<float>(recipeContent, "Zone3Pressure", i);
                    dechuckRecipe.Zone4Pressure[i] = GetRealVal<float>(recipeContent, "Zone4Pressure", i);
                    dechuckRecipe.Zone5Pressure[i] = GetRealVal<float>(recipeContent, "Zone5Pressure", i);
                    dechuckRecipe.Zone6Pressure[i] = GetRealVal<float>(recipeContent, "Zone6Pressure", i);
                    dechuckRecipe.Zone7Pressure[i] = GetRealVal<float>(recipeContent, "Zone7Pressure", i);
                    dechuckRecipe.Zone8Pressure[i] = GetRealVal<float>(recipeContent, "Zone8Pressure", i);
                    dechuckRecipe.ZoneRRPressure[i] = GetRealVal<float>(recipeContent, "ZoneRRPressure", i);
                    dechuckRecipe.PlantWash[i] = GetRealVal<short>(recipeContent, "PlantWash", i);
                    dechuckRecipe.DechuckHeight[i] = GetRealVal<float>(recipeContent, "DechuckHeight", i);
                }

                KST.stChuckRecipeFormat_ToByte(ref writeBuf, dechuckRecipe, 1, 0);
                if (KHLConnectAndOperation("structDeChuckRetryRecipe", writeBuf) == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"Assign Dechuck Retry Recipe {SelectedDechuckRetryRecipe} Successful");
                }
                else
                {
                    Growl.ErrorGlobal($"Assign Dechuck Retry Recipe {SelectedDechuckRetryRecipe} Failed:{err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find Dechuck Retry Recipe: {SelectedDechuckRetryRecipe}");
            }
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error Assign Dechuck Retry Recipe: {ex.Message}");
            Logger.Error("Error Assign Dechuck Retry Recipe", ex);
        }
    }

    private short AssignHeadSweepRecipe(ref List<string> listHeadSweepRecipe, string headSweepRecipeName)
    {
        if (headSweepRecipeName == "None") return 0;

        if (!listHeadSweepRecipe.Contains(headSweepRecipeName))
        {
            listHeadSweepRecipe.Add(headSweepRecipeName);
            var recipeContent = LoadRecipeContent(RecipeTemplateRegistry.HeadSweepRecipeFolder, headSweepRecipeName);
            if (recipeContent != null)
            {
                KST.stHeadSweepRecipeFormat headSweepRecipe = new KST.stHeadSweepRecipeFormat();
                headSweepRecipe.HeadSweepType = GetRealVal<short>(recipeContent, "HeadSweepType", 0);
                headSweepRecipe.HeadSweepCount = GetRealVal<float>(recipeContent, "HeadSweepCount", 0);
                headSweepRecipe.StartPosition = GetRealVal<float>(recipeContent, "StartPosition", 0);
                headSweepRecipe.EndPosition = GetRealVal<float>(recipeContent, "EndPosition", 0);
                headSweepRecipe.ZoneCount = GetRealVal<short>(recipeContent, "ZoneCount", 0);
                headSweepRecipe.Zone1Ratio = GetRealVal<float>(recipeContent, "Zone1Ratio", 0);
                headSweepRecipe.Zone2Ratio = GetRealVal<float>(recipeContent, "Zone2Ratio", 0);
                headSweepRecipe.Zone3Ratio = GetRealVal<float>(recipeContent, "Zone3Ratio", 0);
                headSweepRecipe.Zone4Ratio = GetRealVal<float>(recipeContent, "Zone4Ratio", 0);
                headSweepRecipe.Zone5Ratio = GetRealVal<float>(recipeContent, "Zone5Ratio", 0);
                headSweepRecipe.Zone6Ratio = GetRealVal<float>(recipeContent, "Zone6Ratio", 0);
                headSweepRecipe.Zone7Ratio = GetRealVal<float>(recipeContent, "Zone7Ratio", 0);
                headSweepRecipe.Zone8Ratio = GetRealVal<float>(recipeContent, "Zone8Ratio", 0);
                headSweepRecipe.Zone9Ratio = GetRealVal<float>(recipeContent, "Zone9Ratio", 0);
                headSweepRecipe.Zone10Ratio = GetRealVal<float>(recipeContent, "Zone10Ratio", 0);
                headSweepRecipe.Zone11Ratio = GetRealVal<float>(recipeContent, "Zone11Ratio", 0);
                headSweepRecipe.Zone12Ratio = GetRealVal<float>(recipeContent, "Zone12Ratio", 0);
                headSweepRecipe.Zone13Ratio = GetRealVal<float>(recipeContent, "Zone13Ratio", 0);
                headSweepRecipe.Zone14Ratio = GetRealVal<float>(recipeContent, "Zone14Ratio", 0);
                headSweepRecipe.Zone15Ratio = GetRealVal<float>(recipeContent, "Zone15Ratio", 0);
                headSweepRecipe.Zone16Ratio = GetRealVal<float>(recipeContent, "Zone16Ratio", 0);
                headSweepRecipe.Zone17Ratio = GetRealVal<float>(recipeContent, "Zone17Ratio", 0);
                headSweepRecipe.Zone18Ratio = GetRealVal<float>(recipeContent, "Zone18Ratio", 0);
                headSweepRecipe.Zone19Ratio = GetRealVal<float>(recipeContent, "Zone19Ratio", 0);
                headSweepRecipe.Zone20Ratio = GetRealVal<float>(recipeContent, "Zone20Ratio", 0);
                KST.stHeadSweepRecipeFormat_ToByte(ref writeBuf, headSweepRecipe, 1, 0);
                string variableName = $"structHeadSweepRecipe{listHeadSweepRecipe.IndexOf(headSweepRecipeName)}";
                err = KHLConnectAndOperation(variableName, writeBuf);
                if (err == (int)KHLUserError.KHL_ERR_NONE) Growl.SuccessGlobal($"Assign HeadSweep Recipe {headSweepRecipeName} Successful");
                else Growl.ErrorGlobal($"Assign HeadSweep Recipe {headSweepRecipeName} Failed:{err}");
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find HeadSweep Recipe: {headSweepRecipeName}");
            }
        }

        return (short)listHeadSweepRecipe.IndexOf(headSweepRecipeName);
    }

    private short AssignConditionRecipe(ref List<string> listConditionRecipe, string conditionRecipeName)
    {
        if (conditionRecipeName == "None") return 0;
        List<string> listConditionSweepRecipe = [];
        listConditionSweepRecipe.Add("None");

        if (!listConditionRecipe.Contains(conditionRecipeName))
        {
            listConditionRecipe.Add(conditionRecipeName);
            var recipeContent = LoadRecipeContent(RecipeTemplateRegistry.InsituConditionRecipeFolder, conditionRecipeName);
            if (recipeContent != null)
            {
                KST.stInsituConditionRecipeFormat conditionRecipe = new KST.stInsituConditionRecipeFormat();
                conditionRecipe.ProcessType = GetRealVal<short>(recipeContent, "ProcessType", 0);
                conditionRecipe.ProcessTime = GetRealVal<float>(recipeContent, "ProcessTime", 0);
                conditionRecipe.InSitu = GetRealVal<short>(recipeContent, "InSitu", 0);
                conditionRecipe.PlatenRotation = GetRealVal<float>(recipeContent, "PlatenRotation", 0);
                conditionRecipe.ConditionerHeadRotation = GetRealVal<float>(recipeContent, "ConditionerHeadRotation", 0);
                conditionRecipe.ConditionPressure = GetRealVal<float>(recipeContent, "ConditionPressure", 0);
                conditionRecipe.ConditionSweepRecipe = AssignInsituConditionSweepRecipe(ref listConditionSweepRecipe, GetRealVal<string>(recipeContent, "ConditionSweepRecipe", 0)!);
                conditionRecipe.PlatenWashSelection = GetRealVal<short>(recipeContent, "PlatenWashSelection", 0);
                conditionRecipe.SlurryLine1Type = GetRealVal<short>(recipeContent, "SlurryLine1Type", 0);
                conditionRecipe.SlurryLine2Type = GetRealVal<short>(recipeContent, "SlurryLine2Type", 0);
                conditionRecipe.SlurryLine3Type = GetRealVal<short>(recipeContent, "SlurryLine3Type", 0);
                conditionRecipe.SlurryLine1Flow = GetRealVal<float>(recipeContent, "SlurryLine1Flow", 0);
                conditionRecipe.SlurryLine2Flow = GetRealVal<float>(recipeContent, "SlurryLine2Flow", 0);
                conditionRecipe.SlurryLine3Flow = GetRealVal<float>(recipeContent, "SlurryLine3Flow", 0);
                conditionRecipe.SweepStopTimeType = GetRealVal<short>(recipeContent, "SweepStopTimeType", 0);
                conditionRecipe.Ratio = GetRealVal<float>(recipeContent, "Ratio", 0);
                KST.stInsituConditionRecipeFormat_ToByte(ref writeBuf, conditionRecipe, 1, 0);
                string variableName = $"structConditionRecipe{listConditionRecipe.IndexOf(conditionRecipeName)}";
                err = KHLConnectAndOperation(variableName, writeBuf);
                if (err == (int)KHLUserError.KHL_ERR_NONE) Growl.SuccessGlobal($"Assign Condition Recipe {conditionRecipeName} Successful");
                else Growl.ErrorGlobal($"Assign Condition Recipe {conditionRecipeName} Failed:{err}");
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find Condition Recipe: {conditionRecipeName}");
            }
        }

        return (short)listConditionRecipe.IndexOf(conditionRecipeName);
    }

    /// <summary>
    /// Insitu Condition 用 Sweep 配方下发（读 InsituCondition Sweep Recipe 文件夹，
    /// 组包成 <c>stInsituConditionSweepRecipeFormat</c>）。与 Exsitu 版本各自独立，不再靠 type 字符串分流。
    /// </summary>
    private short AssignInsituConditionSweepRecipe(ref List<string> listConditionSweepRecipe,
        string conditionSweepRecipeName)
    {
        if (conditionSweepRecipeName == "None") return 0;

        if (!listConditionSweepRecipe.Contains(conditionSweepRecipeName))
        {
            listConditionSweepRecipe.Add(conditionSweepRecipeName);
            var recipeContent = LoadRecipeContent(RecipeTemplateRegistry.InsituConditionSweepRecipeFolder, conditionSweepRecipeName);
            if (recipeContent != null)
            {
                KST.stInsituConditionSweepRecipeFormat insituConditionSweepRecipe = new KST.stInsituConditionSweepRecipeFormat();
                insituConditionSweepRecipe.ConditionSweepType = GetRealVal<short>(recipeContent, "ConditionSweepType", 0);
                insituConditionSweepRecipe.StartPosition = GetRealVal<float>(recipeContent, "StartPosition", 0);
                insituConditionSweepRecipe.EndPosition = GetRealVal<float>(recipeContent, "EndPosition", 0);
                insituConditionSweepRecipe.ZoneCount = GetRealVal<short>(recipeContent, "ZoneCount", 0);
                insituConditionSweepRecipe.Zone1Ratio = GetRealVal<float>(recipeContent, "Zone1Ratio", 0);
                insituConditionSweepRecipe.Zone2Ratio = GetRealVal<float>(recipeContent, "Zone2Ratio", 0);
                insituConditionSweepRecipe.Zone3Ratio = GetRealVal<float>(recipeContent, "Zone3Ratio", 0);
                insituConditionSweepRecipe.Zone4Ratio = GetRealVal<float>(recipeContent, "Zone4Ratio", 0);
                insituConditionSweepRecipe.Zone5Ratio = GetRealVal<float>(recipeContent, "Zone5Ratio", 0);
                insituConditionSweepRecipe.Zone6Ratio = GetRealVal<float>(recipeContent, "Zone6Ratio", 0);
                insituConditionSweepRecipe.Zone7Ratio = GetRealVal<float>(recipeContent, "Zone7Ratio", 0);
                insituConditionSweepRecipe.Zone8Ratio = GetRealVal<float>(recipeContent, "Zone8Ratio", 0);
                insituConditionSweepRecipe.Zone9Ratio = GetRealVal<float>(recipeContent, "Zone9Ratio", 0);
                insituConditionSweepRecipe.Zone10Ratio = GetRealVal<float>(recipeContent, "Zone10Ratio", 0);
                insituConditionSweepRecipe.Zone11Ratio = GetRealVal<float>(recipeContent, "Zone11Ratio", 0);
                insituConditionSweepRecipe.Zone12Ratio = GetRealVal<float>(recipeContent, "Zone12Ratio", 0);
                insituConditionSweepRecipe.Zone13Ratio = GetRealVal<float>(recipeContent, "Zone13Ratio", 0);
                insituConditionSweepRecipe.Zone14Ratio = GetRealVal<float>(recipeContent, "Zone14Ratio", 0);
                insituConditionSweepRecipe.Zone15Ratio = GetRealVal<float>(recipeContent, "Zone15Ratio", 0);
                insituConditionSweepRecipe.Zone16Ratio = GetRealVal<float>(recipeContent, "Zone16Ratio", 0);
                insituConditionSweepRecipe.Zone17Ratio = GetRealVal<float>(recipeContent, "Zone17Ratio", 0);
                insituConditionSweepRecipe.Zone18Ratio = GetRealVal<float>(recipeContent, "Zone18Ratio", 0);
                insituConditionSweepRecipe.Zone19Ratio = GetRealVal<float>(recipeContent, "Zone19Ratio", 0);
                insituConditionSweepRecipe.Zone20Ratio = GetRealVal<float>(recipeContent, "Zone20Ratio", 0);
                insituConditionSweepRecipe.ConditionSweepCount = GetRealVal<float>(recipeContent, "ConditionSweepCount", 0);
                KST.stInsituConditionSweepRecipeFormat_ToByte(ref writeBuf, insituConditionSweepRecipe, 1, 0);
                string variableName =
                    $"structConditionSweepRecipe{listConditionSweepRecipe.IndexOf(conditionSweepRecipeName)}";

                err = KHLConnectAndOperation(variableName, writeBuf);
                if (err == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"Assign Insitu ConditionSweep Recipe {conditionSweepRecipeName} Successful");
                }
                else
                {
                    Growl.ErrorGlobal($"Assign Insitu ConditionSweep Recipe {conditionSweepRecipeName} Failed:{err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find Insitu ConditionSweep Recipe: {conditionSweepRecipeName}");
            }
        }

        return (short)listConditionSweepRecipe.IndexOf(conditionSweepRecipeName);
    }

    /// <summary>
    /// Exsitu Condition 用 Sweep 配方下发（读 ExsituCondition Sweep Recipe 文件夹，
    /// 组包成 <c>stExsituConditionSweepRecipeFormat</c>）。
    /// </summary>
    private short AssignExsituConditionSweepRecipe(ref List<string> listConditionSweepRecipe,
        string conditionSweepRecipeName)
    {
        if (conditionSweepRecipeName == "None") return 0;

        if (!listConditionSweepRecipe.Contains(conditionSweepRecipeName))
        {
            listConditionSweepRecipe.Add(conditionSweepRecipeName);
            var recipeContent = LoadRecipeContent(RecipeTemplateRegistry.ExsituConditionSweepRecipeFolder, conditionSweepRecipeName);
            if (recipeContent != null)
            {
                KST.stExsituConditionSweepRecipeFormat exsituConditionSweepRecipe = new KST.stExsituConditionSweepRecipeFormat();
                exsituConditionSweepRecipe.ConditionSweepType = GetRealVal<short>(recipeContent, "ConditionSweepType", 0);
                exsituConditionSweepRecipe.ConditionSweepCount = GetRealVal<float>(recipeContent, "ConditionSweepCount", 0);
                exsituConditionSweepRecipe.StartPosition = GetRealVal<float>(recipeContent, "StartPosition", 0);
                exsituConditionSweepRecipe.EndPosition = GetRealVal<float>(recipeContent, "EndPosition", 0);
                exsituConditionSweepRecipe.ZoneCount = GetRealVal<short>(recipeContent, "ZoneCount", 0);
                exsituConditionSweepRecipe.Zone1Ratio = GetRealVal<float>(recipeContent, "Zone1Ratio", 0);
                exsituConditionSweepRecipe.Zone2Ratio = GetRealVal<float>(recipeContent, "Zone2Ratio", 0);
                exsituConditionSweepRecipe.Zone3Ratio = GetRealVal<float>(recipeContent, "Zone3Ratio", 0);
                exsituConditionSweepRecipe.Zone4Ratio = GetRealVal<float>(recipeContent, "Zone4Ratio", 0);
                exsituConditionSweepRecipe.Zone5Ratio = GetRealVal<float>(recipeContent, "Zone5Ratio", 0);
                exsituConditionSweepRecipe.Zone6Ratio = GetRealVal<float>(recipeContent, "Zone6Ratio", 0);
                exsituConditionSweepRecipe.Zone7Ratio = GetRealVal<float>(recipeContent, "Zone7Ratio", 0);
                exsituConditionSweepRecipe.Zone8Ratio = GetRealVal<float>(recipeContent, "Zone8Ratio", 0);
                exsituConditionSweepRecipe.Zone9Ratio = GetRealVal<float>(recipeContent, "Zone9Ratio", 0);
                exsituConditionSweepRecipe.Zone10Ratio = GetRealVal<float>(recipeContent, "Zone10Ratio", 0);
                exsituConditionSweepRecipe.Zone11Ratio = GetRealVal<float>(recipeContent, "Zone11Ratio", 0);
                exsituConditionSweepRecipe.Zone12Ratio = GetRealVal<float>(recipeContent, "Zone12Ratio", 0);
                exsituConditionSweepRecipe.Zone13Ratio = GetRealVal<float>(recipeContent, "Zone13Ratio", 0);
                exsituConditionSweepRecipe.Zone14Ratio = GetRealVal<float>(recipeContent, "Zone14Ratio", 0);
                exsituConditionSweepRecipe.Zone15Ratio = GetRealVal<float>(recipeContent, "Zone15Ratio", 0);
                exsituConditionSweepRecipe.Zone16Ratio = GetRealVal<float>(recipeContent, "Zone16Ratio", 0);
                exsituConditionSweepRecipe.Zone17Ratio = GetRealVal<float>(recipeContent, "Zone17Ratio", 0);
                exsituConditionSweepRecipe.Zone18Ratio = GetRealVal<float>(recipeContent, "Zone18Ratio", 0);
                exsituConditionSweepRecipe.Zone19Ratio = GetRealVal<float>(recipeContent, "Zone19Ratio", 0);
                exsituConditionSweepRecipe.Zone20Ratio = GetRealVal<float>(recipeContent, "Zone20Ratio", 0);
                KST.stExsituConditionSweepRecipeFormat_ToByte(ref writeBuf, exsituConditionSweepRecipe, 1, 0);
                string variableName =
                    $"structExsituConditionSweepRecipe{listConditionSweepRecipe.IndexOf(conditionSweepRecipeName)}";

                err = KHLConnectAndOperation(variableName, writeBuf);
                if (err == (int)KHLUserError.KHL_ERR_NONE)
                {
                    Growl.SuccessGlobal($"Assign Exsitu ConditionSweep Recipe {conditionSweepRecipeName} Successful");
                }
                else
                {
                    Growl.ErrorGlobal($"Assign Exsitu ConditionSweep Recipe {conditionSweepRecipeName} Failed:{err}");
                }
            }
            else
            {
                Growl.ErrorGlobal($"Cannot Find Exsitu ConditionSweep Recipe: {conditionSweepRecipeName}");
            }
        }

        return (short)listConditionSweepRecipe.IndexOf(conditionSweepRecipeName);
    }

    private IEnumerable<(string Name, uint Addr, byte[] Value, uint PressAddr, byte[] PressValue)> GetZonesPress()
    {
        var zones = new[]
        {
            (Name: "Zone1", Addr: PlcHandle.CmdPolish.Gas.Zone1.Press.Handle(), Value: PlcHandle.CmdPolish.Gas.Zone1.Press.Value(),
                PressAddr: PlcHandle.CmdPolish.Gas.Zone1.Param.Pressure.ParamHandle(),
                PressValue: PolishUnit.Zone1.SetPress.Value()),
            (Name: "Zone2", Addr: PlcHandle.CmdPolish.Gas.Zone2.Press.Handle(), Value: PlcHandle.CmdPolish.Gas.Zone2.Press.Value(),
                PressAddr: PlcHandle.CmdPolish.Gas.Zone2.Param.Pressure.ParamHandle(),
                PressValue: PolishUnit.Zone2.SetPress.Value()),
            (Name: "Zone3", Addr: PlcHandle.CmdPolish.Gas.Zone3.Press.Handle(), Value: PlcHandle.CmdPolish.Gas.Zone3.Press.Value(),
                PressAddr: PlcHandle.CmdPolish.Gas.Zone3.Param.Pressure.ParamHandle(),
                PressValue: PolishUnit.Zone3.SetPress.Value()),
            (Name: "Zone4", Addr: PlcHandle.CmdPolish.Gas.Zone4.Press.Handle(), Value: PlcHandle.CmdPolish.Gas.Zone4.Press.Value(),
                PressAddr: PlcHandle.CmdPolish.Gas.Zone4.Param.Pressure.ParamHandle(),
                PressValue: PolishUnit.Zone4.SetPress.Value()),
            (Name: "Zone5", Addr: PlcHandle.CmdPolish.Gas.Zone5.Press.Handle(), Value: PlcHandle.CmdPolish.Gas.Zone5.Press.Value(),
                PressAddr: PlcHandle.CmdPolish.Gas.Zone5.Param.Pressure.ParamHandle(),
                PressValue: PolishUnit.Zone5.SetPress.Value()),
            (Name: "Zone6", Addr: PlcHandle.CmdPolish.Gas.Zone6.Press.Handle(), Value: PlcHandle.CmdPolish.Gas.Zone6.Press.Value(),
                PressAddr: PlcHandle.CmdPolish.Gas.Zone6.Param.Pressure.ParamHandle(),
                PressValue: PolishUnit.Zone6.SetPress.Value()),
            (Name: "Zone7", Addr: PlcHandle.CmdPolish.Gas.Zone7.Press.Handle(), Value: PlcHandle.CmdPolish.Gas.Zone7.Press.Value(),
                PressAddr: PlcHandle.CmdPolish.Gas.Zone7.Param.Pressure.ParamHandle(),
                PressValue: PolishUnit.Zone7.SetPress.Value()),
            (Name: "Zone8", Addr: PlcHandle.CmdPolish.Gas.Zone8.Press.Handle(), Value: PlcHandle.CmdPolish.Gas.Zone8.Press.Value(),
                PressAddr: PlcHandle.CmdPolish.Gas.Zone8.Param.Pressure.ParamHandle(),
                PressValue: PolishUnit.Zone8.SetPress.Value()),
            (Name: "Retain", Addr: PlcHandle.CmdPolish.Gas.Retain.Press.Handle(),
                Value: PlcHandle.CmdPolish.Gas.Retain.Press.Value(),
                PressAddr: PlcHandle.CmdPolish.Gas.Retain.Param.Pressure.ParamHandle(),
                PressValue: PolishUnit.Retain.SetPress.Value()),
        };
        return zones;
    }

    private IEnumerable<(string Name, uint Addr, byte[] Value)> GetZonesVent()
    {
        var zones = new[]
        {
            (Name: "Zone1", Addr: PlcHandle.CmdPolish.Gas.Zone1.Vent.Handle(), Value: PlcHandle.CmdPolish.Gas.Zone1.Vent.Value()),
            (Name: "Zone2", Addr: PlcHandle.CmdPolish.Gas.Zone2.Vent.Handle(), Value: PlcHandle.CmdPolish.Gas.Zone2.Vent.Value()),
            (Name: "Zone3", Addr: PlcHandle.CmdPolish.Gas.Zone3.Vent.Handle(), Value: PlcHandle.CmdPolish.Gas.Zone3.Vent.Value()),
            (Name: "Zone4", Addr: PlcHandle.CmdPolish.Gas.Zone4.Vent.Handle(), Value: PlcHandle.CmdPolish.Gas.Zone4.Vent.Value()),
            (Name: "Zone5", Addr: PlcHandle.CmdPolish.Gas.Zone5.Vent.Handle(), Value: PlcHandle.CmdPolish.Gas.Zone5.Vent.Value()),
            (Name: "Zone6", Addr: PlcHandle.CmdPolish.Gas.Zone6.Vent.Handle(), Value: PlcHandle.CmdPolish.Gas.Zone6.Vent.Value()),
            (Name: "Zone7", Addr: PlcHandle.CmdPolish.Gas.Zone7.Vent.Handle(), Value: PlcHandle.CmdPolish.Gas.Zone7.Vent.Value()),
            (Name: "Zone8", Addr: PlcHandle.CmdPolish.Gas.Zone8.Vent.Handle(), Value: PlcHandle.CmdPolish.Gas.Zone8.Vent.Value()),
            (Name: "Retain", Addr: PlcHandle.CmdPolish.Gas.Retain.Vent.Handle(), Value: PlcHandle.CmdPolish.Gas.Retain.Vent.Value()),
        };
        return zones;
    }

    private IEnumerable<(string Name, uint Addr, byte[] Value)> GetZonesVacuum()
    {
        var zones = new[]
        {
            (Name: "Zone1", Addr: PlcHandle.CmdPolish.Gas.Zone1.Vacuum.Handle(),
                Value: PlcHandle.CmdPolish.Gas.Zone1.Vacuum.Value()),
            (Name: "Zone2", Addr: PlcHandle.CmdPolish.Gas.Zone2.Vacuum.Handle(),
                Value: PlcHandle.CmdPolish.Gas.Zone2.Vacuum.Value()),
            (Name: "Zone3", Addr: PlcHandle.CmdPolish.Gas.Zone3.Vacuum.Handle(),
                Value: PlcHandle.CmdPolish.Gas.Zone3.Vacuum.Value()),
            (Name: "Zone4", Addr: PlcHandle.CmdPolish.Gas.Zone4.Vacuum.Handle(),
                Value: PlcHandle.CmdPolish.Gas.Zone4.Vacuum.Value()),
            (Name: "Zone5", Addr: PlcHandle.CmdPolish.Gas.Zone5.Vacuum.Handle(),
                Value: PlcHandle.CmdPolish.Gas.Zone5.Vacuum.Value()),
            (Name: "Zone6", Addr: PlcHandle.CmdPolish.Gas.Zone6.Vacuum.Handle(),
                Value: PlcHandle.CmdPolish.Gas.Zone6.Vacuum.Value()),
            (Name: "Zone7", Addr: PlcHandle.CmdPolish.Gas.Zone7.Vacuum.Handle(),
                Value: PlcHandle.CmdPolish.Gas.Zone7.Vacuum.Value()),
            (Name: "Zone8", Addr: PlcHandle.CmdPolish.Gas.Zone8.Vacuum.Handle(),
                Value: PlcHandle.CmdPolish.Gas.Zone8.Vacuum.Value()),
            (Name: "Retain", Addr: PlcHandle.CmdPolish.Gas.Retain.Vacuum.Handle(),
                Value: PlcHandle.CmdPolish.Gas.Retain.Vacuum.Value()),
        };
        return zones;
    }

    /// <summary>
    /// Recipe保存事件处理方法
    /// </summary>
    /// <param name="args">事件参数</param>
    private void OnRecipeSaved(RecipeSavedEventArgs args)
    {
        try
        {
            if (args == null || string.IsNullOrEmpty(args.RecipeType))
                return;

            // 根据Recipe类型重新加载对应的列表
            switch (args.RecipeType)
            {
                case RecipeTemplateRegistry.LoadRecipeFolder:
                    LoadRecipesFromFolder(RecipeTemplateRegistry.LoadRecipeFolder, LoadRecipes);
                    // 如果当前选择的Recipe被修改了，保持选择
                    if (!string.IsNullOrEmpty(args.RecipeName) && LoadRecipes.Contains(args.RecipeName))
                    {
                        SelectedLoadRecipe = args.RecipeName;
                    }

                    break;
                case RecipeTemplateRegistry.InsituConditionRecipeFolder:
                    LoadRecipesFromFolder(RecipeTemplateRegistry.InsituConditionRecipeFolder, ConditionRecipes);
                    if (!string.IsNullOrEmpty(args.RecipeName) && ConditionRecipes.Contains(args.RecipeName))
                    {
                        // 如果保存的Recipe是当前 Pre_Cond 选中的，则保持选中
                        if (SelectedPreCondRecipe == args.RecipeName)
                            SelectedPreCondRecipe = args.RecipeName;
                    }

                    break;
                case RecipeTemplateRegistry.ExsituConditionRecipeFolder:
                    LoadRecipesFromFolder(RecipeTemplateRegistry.ExsituConditionRecipeFolder, ExsituConditionRecipes);
                    if (!string.IsNullOrEmpty(args.RecipeName) && ExsituConditionRecipes.Contains(args.RecipeName))
                    {
                        SelectedExsituConditionRecipe = args.RecipeName;
                    }

                    break;
                case RecipeTemplateRegistry.PolishRecipeFolder:
                    LoadRecipesFromFolder(RecipeTemplateRegistry.PolishRecipeFolder, PolishRecipes);
                    if (!string.IsNullOrEmpty(args.RecipeName) && PolishRecipes.Contains(args.RecipeName))
                    {
                        SelectedPolishRecipe = args.RecipeName;
                    }

                    break;
                case RecipeTemplateRegistry.UnloadRecipeFolder:
                    LoadRecipesFromFolder(RecipeTemplateRegistry.UnloadRecipeFolder, UnloadRecipes);
                    if (!string.IsNullOrEmpty(args.RecipeName) && UnloadRecipes.Contains(args.RecipeName))
                    {
                        SelectedUnloadRecipe = args.RecipeName;
                    }

                    break;
                case RecipeTemplateRegistry.DechuckRecipeFolder:
                    LoadRecipesFromFolder(RecipeTemplateRegistry.DechuckRecipeFolder, DechuckRecipes);
                    if (!string.IsNullOrEmpty(args.RecipeName) && DechuckRecipes.Contains(args.RecipeName))
                    {
                        SelectedDechuckRecipe = args.RecipeName;
                    }

                    break;
            }

            Growl.InfoGlobal($"Recipe已更新: {args.RecipeType} - {args.RecipeName}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to reload recipes after save: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 全局变量polishSocks做出改变的通知时，订阅者需要做出变更
    /// </summary>
    private void OnPolishSocketsChanged()
    {
        //kvLink.sock = GlobalService.kvHostLinkInit.PolishSocks[_polishToken];
        //err = KHL.KHLWriteVariable(sock, "PLCStartPermition", writeTrue, true);
        //err = KHL.KHLWriteVariable(sock, "PLCReady", writeTrue, true);
        //err = KHL.KHLWriteVariable(sock, "PLCAutoStart", writeTrue, true);
    }

    /// <summary>
    /// Load recipes from the file system
    /// </summary>
    private void LoadRecipesData()
    {
        try
        {
            if (!Directory.Exists(_recipeBasePath))
            {
                Directory.CreateDirectory(_recipeBasePath);
                return;
            }

            // Load recipes from specific folders

            LoadRecipesFromFolder(RecipeTemplateRegistry.InsituConditionRecipeFolder, ConditionRecipes);
            LoadRecipesFromFolder(RecipeTemplateRegistry.ExsituConditionRecipeFolder, ExsituConditionRecipes);
            LoadRecipesFromFolder(RecipeTemplateRegistry.PolishRecipeFolder, PolishRecipes);
            LoadRecipesFromFolder(RecipeTemplateRegistry.HeadSweepRecipeFolder, HeadSweepRecipes);

            // Set default selections if available

            //if (LoadRecipes.Count > 0)
            //    SelectedLoadRecipe = LoadRecipes[0];
            //if (UnloadRecipes.Count > 0)
            //    SelectedUnloadRecipe = UnloadRecipes[0];
            //if (DechuckRecipes.Count > 0)
            //    SelectedDechuckRecipe = DechuckRecipes[0];

            if (ConditionRecipes.Count > 0)
            {
                SelectedPreCondRecipe = ConditionRecipes[0];
            }
            if (ExsituConditionRecipes.Count > 0)
            {
                SelectedExsituConditionRecipe = ExsituConditionRecipes[0];
            }
            if (PolishRecipes.Count > 0)
                SelectedPolishRecipe = PolishRecipes[0];
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Failed to load recipes: {ex.Message}");
        }
    }

    /// <summary>
    /// Load recipes from a specific folder
    /// </summary>
    /// <param name="folderName">Folder name to load from</param>
    /// <param name="recipeList">Target recipe list to populate</param>
    private void LoadRecipesFromFolder(string folderName, ObservableCollection<string> recipeList)
    {
        try
        {
            string folderPath = Path.Combine(_recipeBasePath, folderName);

            if (!Directory.Exists(folderPath))
            {
                return;
            }

            // 清空现有列表以避免重复
            recipeList.Clear();

            DirectoryInfo folderInfo = new DirectoryInfo(folderPath);
            var jsonFiles = folderInfo.GetFiles("*.json");

            foreach (var file in jsonFiles)
            {
                try
                {
                    var json = FileUtil.ReadFile(file.FullName);
                    Recipe.Content? content = JsonConvert.DeserializeObject<Recipe.Content>(json);

                    if (content != null && !string.IsNullOrEmpty(content.Name))
                    {
                        if (!recipeList.Contains(content.Name))
                        {
                            recipeList.Add(content.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue processing other files
                    Console.WriteLine($"Error loading recipe from {file.FullName}: {ex.Message}");
                    Logger.Error($"Error loading recipe from {file.FullName}", ex);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading recipes from folder {folderName}: {ex.Message}");
            Logger.Error($"Error loading recipes from folder {folderName}", ex);
        }
    }

    /// <summary>
    /// Load specific recipe content from a folder
    /// </summary>
    /// <param name="folderName">Folder name containing the recipe</param>
    /// <param name="recipeName">Recipe name to load</param>
    /// <returns>Recipe content or null if not found</returns>
    private Recipe.Content? LoadRecipeContent(string folderName, string recipeName)
    {
        try
        {
            string folderPath = Path.Combine(_recipeBasePath, folderName);

            if (!Directory.Exists(folderPath))
            {
                Growl.WarningGlobal($"Recipe folder not found: {folderName}");
                return null;
            }

            // Find the JSON file matching the recipe name
            string recipeFilePath = Path.Combine(folderPath, $"{recipeName}.json");

            if (!File.Exists(recipeFilePath))
            {
                Growl.WarningGlobal($"Recipe file not found: {recipeName}.json");
                return null;
            }

            // Read and deserialize the recipe file
            var json = FileUtil.ReadFile(recipeFilePath);
            Recipe.Content? content = JsonConvert.DeserializeObject<Recipe.Content>(json);

            if (content == null)
            {
                Growl.ErrorGlobal($"Failed to deserialize recipe: {recipeName}");
                return null;
            }

            // 按文件夹名把模板上的规格元数据（IsUsed / OperableRule）贴回行上，
            // 供 GetRealVal 判定"不使用"参数：IsUsed=false 的参数按 0（default）下发
            RecipeTemplateRegistry.Decorate(content, folderName);

            return content;
        }
        catch (Exception ex)
        {
            Growl.ErrorGlobal($"Error reading recipe file: {ex.Message}");
            return null;
        }
    }

    private static T? GetRealVal<T>(Recipe.Content recipeModel, string name, int stepIndex)
    {
        var model = recipeModel.Data?
            .FirstOrDefault(item => {
                string itemName = item.Name!.Replace(" ", "")
                            .Replace("_", "")
                            .Replace("-", "")
                            .Replace("&", "");

                return itemName == name || (item.CompatibleName != null && item.CompatibleName.Contains(name));
            }
                );

        if (model == null || !model.IsUsed) return default;
        if (stepIndex < 0 || stepIndex >= model.Steps.Count) return default;

        var value = model.Steps[stepIndex].Value;
        var type = model.Type?.ToLowerInvariant();
        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        if (type == "option" || type == "list")
        {
            // 统一用 RecipeOptionValue 解析（兼容全角冒号）
            value = targetType == typeof(string)
                ? RecipeOptionValue.LabelPart(value)
                : RecipeOptionValue.NumericPart(value);
        }


        try
        {
            if (targetType == typeof(string))
                return (T)(object)value;

            if (targetType.IsEnum)
                return (T)Enum.Parse(targetType, value, ignoreCase: true);

            return (T)Convert.ChangeType(value, targetType);
        }
        catch
        {
            return default;
        }
    }
}
