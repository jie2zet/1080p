using HandyControl.Tools.Extension;
using SqlSugar;

namespace CMP.Controls.Basic.InteractiveDialog;

public class InteractiveDialogViewModel : BindableBase, IDialogResultable<string>
{
    public Action? CloseAction { get; set; }

    private string? _result;

    public string? Result
    {
        get => _result;
#if NET40
        set => Set(nameof(Result), ref _result, value);
#else
        set => SetProperty(ref _result, value);
#endif
    }

    private string? _message;

    public string? Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public DelegateCommand OkCmd => new(() => CloseAction?.Invoke());
    public DelegateCommand CancelCmd => new(() => { 
        Result = null; CloseAction?.Invoke(); 
    });
}