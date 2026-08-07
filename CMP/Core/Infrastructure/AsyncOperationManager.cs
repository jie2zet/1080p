using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace CMP.Core.Infrastructure
{
    /// <summary>
    /// 异步操作管理器 - 统一管理异步操作和UI更新
    /// </summary>
    public static class AsyncOperationManager
    {
        /// <summary>
        /// 在后台线程执行同步操作
        /// </summary>
        public static async Task<T> ExecuteAsync<T>(Func<T> operation)
        {
            return await Task.Run(operation);
        }

        /// <summary>
        /// 在后台线程执行无返回值操作
        /// </summary>
        public static async Task ExecuteAsync(Action operation)
        {
            await Task.Run(operation);
        }

        /// <summary>
        /// 在UI线程上异步执行操作
        /// </summary>
        public static async Task InvokeOnUIAsync(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                action();
                return;
            }

            if (dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                await dispatcher.InvokeAsync(action);
            }
        }

        /// <summary>
        /// 在UI线程上异步执行操作并返回值
        /// </summary>
        public static async Task<T> InvokeOnUIAsync<T>(Func<T> func)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                return func();
            }

            if (dispatcher.CheckAccess())
            {
                return func();
            }
            else
            {
                return await dispatcher.InvokeAsync(func);
            }
        }

        /// <summary>
        /// 安全执行异步操作，捕获异常
        /// </summary>
        public static async Task<bool> SafeExecuteAsync(Func<Task> operation, Action<Exception> onError = null)
        {
            try
            {
                await operation();
                return true;
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                return false;
            }
        }
    }
}

