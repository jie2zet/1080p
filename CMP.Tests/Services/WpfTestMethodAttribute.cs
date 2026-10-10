using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CMP.Tests.Services;

/// <summary>
/// Keeps WPF resources and localization bindings on one live STA dispatcher across tests.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class WpfTestMethodAttribute : TestMethodAttribute
{
    private static readonly Lazy<Dispatcher> UiDispatcher = new(CreateDispatcher);

    public override TestResult[] Execute(ITestMethod testMethod) =>
        UiDispatcher.Value.Invoke(() => base.Execute(testMethod));

    private static Dispatcher CreateDispatcher()
    {
        var ready = new TaskCompletionSource<Dispatcher>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            ready.SetResult(Dispatcher.CurrentDispatcher);
            Dispatcher.Run();
        })
        {
            IsBackground = true,
            Name = "Offline WPF tests"
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return ready.Task.GetAwaiter().GetResult();
    }
}
