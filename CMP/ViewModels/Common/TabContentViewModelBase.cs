using CMP.Services.Plc;
using Prism.Commands;
using System.Windows.Threading;

namespace CMP.ViewModels.Common;

public class TabContentViewModelBase : BindableBase, IViewLifecycleAware
{
    public int Id { get; }
    public string? TabTitle { get; set; }
    private readonly List<(Action onActivated, Action onDeactivated)> _lifecycleActions = [];

    private bool _isViewActive;
    public bool IsViewActive
    {
        get => _isViewActive;
        protected set => SetProperty(ref _isViewActive, value);
    }

    protected TabContentViewModelBase(int id, string title = "")
    {
        Id = id;
        TabTitle = title;
    }

    protected void RegisterViewLifecycleAction(Action onActivated, Action onDeactivated)
    {
        _lifecycleActions.Add((onActivated, onDeactivated));
    }

    protected void RegisterLifecycleTimer(DispatcherTimer timer)
    {
        RegisterViewLifecycleAction(
            () =>
            {
                if (!timer.IsEnabled)
                {
                    timer.Start();
                }
            },
            () =>
            {
                if (timer.IsEnabled)
                {
                    timer.Stop();
                }
            });
    }

    public virtual void OnViewActivated()
    {
        if (IsViewActive)
        {
            return;
        }

        IsViewActive = true;

        foreach (var lifecycleAction in _lifecycleActions)
        {
            lifecycleAction.onActivated();
        }
    }

    public virtual void OnViewDeactivated()
    {
        if (!IsViewActive)
        {
            return;
        }

        foreach (var lifecycleAction in _lifecycleActions)
        {
            lifecycleAction.onDeactivated();
        }

        IsViewActive = false;
    }
}

public static class PlcEditableValueHelper
{
    public static bool ShouldMarkFloatDirty(double value, double plcValue, bool hasPlcValue, double tolerance = 0.0005d)
    {
        if (!hasPlcValue) return false;
        return Math.Abs(value - plcValue) >= tolerance;
    }

    public static bool ShouldMarkIntDirty(int value, int plcValue, bool hasPlcValue)
    {
        if (!hasPlcValue) return false;
        return value != plcValue;
    }

    public static bool SetDirty(ref bool dirtyField, bool value)
    {
        if (dirtyField == value) return false;
        dirtyField = value;
        return true;
    }

    public static void UpdateFloatDirtyAndNotify(
        double value,
        double plcValue,
        bool hasPlcValue,
        ref bool dirtyField,
        Action raiseDirtyChanged)
    {
        var shouldBeDirty = ShouldMarkFloatDirty(value, plcValue, hasPlcValue);
        if (SetDirty(ref dirtyField, shouldBeDirty))
        {
            raiseDirtyChanged();
        }
    }

    public static void ResetDirtyAndNotify(ref bool dirtyField, Action raiseDirtyChanged)
    {
        if (SetDirty(ref dirtyField, false))
        {
            raiseDirtyChanged();
        }
    }

    public static void SaveFloat(
        MitsubishiPlcService plc,
        string address,
        double value,
        ref double plcValueField,
        ref bool hasPlcField)
    {
        plc.Write(address, (int)(value * 1000));
        plcValueField = value;
        hasPlcField = true;
    }

    public static void SaveFloatAndResetDirty(
        MitsubishiPlcService plc,
        string address,
        double value,
        ref double plcValueField,
        ref bool hasPlcField,
        ref bool dirtyField,
        Action raiseDirtyChanged)
    {
        SaveFloat(plc, address, value, ref plcValueField, ref hasPlcField);
        ResetDirtyAndNotify(ref dirtyField, raiseDirtyChanged);
    }

    public static void CancelToPlcValue<T>(
        ref T valueField,
        T plcValue,
        bool hasPlcValue,
        Action raiseValueChanged)
    {
        if (hasPlcValue)
        {
            valueField = plcValue;
            raiseValueChanged();
        }
    }

    public static void CancelToPlcValueAndResetDirty<T>(
        ref T valueField,
        T plcValue,
        bool hasPlcValue,
        ref bool dirtyField,
        Action raiseValueChanged,
        Action raiseDirtyChanged)
    {
        CancelToPlcValue(ref valueField, plcValue, hasPlcValue, raiseValueChanged);
        ResetDirtyAndNotify(ref dirtyField, raiseDirtyChanged);
    }

    public static void ApplyFloatFromPlc(
        bool isSuccess,
        float value,
        ref double valueField,
        ref double plcValueField,
        ref bool hasPlcField,
        bool isDirty,
        Action raiseValueChanged)
    {
        if (!isSuccess) return;

        plcValueField = value;
        hasPlcField = true;

        if (isDirty) return;

        valueField = value;
        raiseValueChanged();
    }
}

public class EditableValueItem<T>
{
    public EditableValueItem(string address, string valuePropertyName, string dirtyPropertyName)
    {
        Address = address;
        ValuePropertyName = valuePropertyName;
        DirtyPropertyName = dirtyPropertyName;
    }

    public string Address { get; }
    public string ValuePropertyName { get; }
    public string DirtyPropertyName { get; }
    public T Value = default!;
    public T PlcValue = default!;
    public bool HasPlcValue;
    public bool IsDirty;
    public DelegateCommand SetCommand = null!;
    public DelegateCommand CancelCommand = null!;
}

public static class EditableValueItemHelper
{
    public static void SetValue<T>(
        EditableValueItem<T> item,
        T value,
        Func<T, T, bool, bool> shouldMarkDirty,
        Action<string> raisePropertyChanged)
    {
        if (EqualityComparer<T>.Default.Equals(item.Value, value))
        {
            return;
        }

        item.Value = value;
        raisePropertyChanged(item.ValuePropertyName);

        var shouldBeDirty = shouldMarkDirty(item.Value, item.PlcValue, item.HasPlcValue);
        if (PlcEditableValueHelper.SetDirty(ref item.IsDirty, shouldBeDirty))
        {
            raisePropertyChanged(item.DirtyPropertyName);
        }
    }

    public static void InitializeCommands<T>(
        IEnumerable<EditableValueItem<T>> items,
        Action<string, T> writeAction,
        Action<string> raisePropertyChanged)
    {
        foreach (var item in items)
        {
            item.SetCommand = new DelegateCommand(() =>
            {
                writeAction(item.Address, item.Value);
                item.PlcValue = item.Value;
                item.HasPlcValue = true;

                if (PlcEditableValueHelper.SetDirty(ref item.IsDirty, false))
                {
                    raisePropertyChanged(item.DirtyPropertyName);
                }
            });

            item.CancelCommand = new DelegateCommand(() =>
            {
                if (item.HasPlcValue)
                {
                    item.Value = item.PlcValue;
                    raisePropertyChanged(item.ValuePropertyName);
                }

                if (PlcEditableValueHelper.SetDirty(ref item.IsDirty, false))
                {
                    raisePropertyChanged(item.DirtyPropertyName);
                }
            });
        }
    }
}

public sealed class EditableFloatItem : EditableValueItem<double>
{
    public EditableFloatItem(string address, string valuePropertyName, string dirtyPropertyName)
        : base(address, valuePropertyName, dirtyPropertyName)
    {
    }
}

public static class EditableFloatItemHelper
{
    public static void SetValue(EditableFloatItem item, double value, Action<string> raisePropertyChanged)
    {
        if (item.Value.Equals(value))
        {
            return;
        }

        item.Value = value;
        raisePropertyChanged(item.ValuePropertyName);
        PlcEditableValueHelper.UpdateFloatDirtyAndNotify(
            value,
            item.PlcValue,
            item.HasPlcValue,
            ref item.IsDirty,
            () => raisePropertyChanged(item.DirtyPropertyName));
    }

    public static void InitializeCommands(
        IEnumerable<EditableFloatItem> items,
        MitsubishiPlcService plc,
        Action<string> raisePropertyChanged)
    {
        foreach (var item in items)
        {
            item.SetCommand = new DelegateCommand(() =>
                PlcEditableValueHelper.SaveFloatAndResetDirty(
                    plc,
                    item.Address,
                    item.Value,
                    ref item.PlcValue,
                    ref item.HasPlcValue,
                    ref item.IsDirty,
                    () => raisePropertyChanged(item.DirtyPropertyName)));

            item.CancelCommand = new DelegateCommand(() =>
                PlcEditableValueHelper.CancelToPlcValueAndResetDirty(
                    ref item.Value,
                    item.PlcValue,
                    item.HasPlcValue,
                    ref item.IsDirty,
                    () => raisePropertyChanged(item.ValuePropertyName),
                    () => raisePropertyChanged(item.DirtyPropertyName)));
        }
    }
}

public sealed class EditableIntItem : EditableValueItem<int>
{
    public EditableIntItem(string address, string valuePropertyName, string dirtyPropertyName)
        : base(address, valuePropertyName, dirtyPropertyName)
    {
    }
}

public static class EditableIntItemHelper
{
    public static void SetValue(EditableIntItem item, int value, Action<string> raisePropertyChanged)
    {
        EditableValueItemHelper.SetValue(
            item,
            value,
            (current, plc, hasPlc) => PlcEditableValueHelper.ShouldMarkIntDirty(current, plc, hasPlc),
            raisePropertyChanged);
    }

    public static void InitializeCommands(
        IEnumerable<EditableIntItem> items,
        Action<string, int> writeAction,
        Action<string> raisePropertyChanged)
    {
        EditableValueItemHelper.InitializeCommands(items, writeAction, raisePropertyChanged);
    }
}