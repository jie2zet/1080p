using System;

namespace CMP.Modules.Command.Models
{
    /// <summary>
    /// 命令模型
    /// </summary>
    public class Command
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string DeviceName { get; set; }
        public string CommandType { get; set; }
        public object Parameters { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public CommandPriority Priority { get; set; } = CommandPriority.Normal;
    }

    /// <summary>
    /// 命令结果
    /// </summary>
    public class CommandResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public DateTime ExecutedTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 命令优先级
    /// </summary>
    public enum CommandPriority
    {
        Low,
        Normal,
        High,
        Critical
    }
}

