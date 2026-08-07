//using System.Threading.Tasks;
//using CMP.Modules.Command.Models;

//namespace CMP.Modules.Command
//{
//    /// <summary>
//    /// 命令服务接口
//    /// </summary>
//    public interface ICommandService
//    {
//        /// <summary>
//        /// 执行命令
//        /// </summary>
//        Task<CommandResult> ExecuteCommandAsync(Command command);

//        /// <summary>
//        /// 执行设备命令
//        /// </summary>
//        Task<CommandResult> ExecuteDeviceCommandAsync<T>(T device, string commandName, object parameters = null);

//        /// <summary>
//        /// 将命令加入队列
//        /// </summary>
//        void EnqueueCommand(Command command);

//        /// <summary>
//        /// 取消命令
//        /// </summary>
//        Task<bool> CancelCommandAsync(string commandId);
//    }
//}

