using System;

namespace CMP.Modules.Alarm.Models
{
    /// <summary>
    /// 报警模型
    /// </summary>
    public class Alarm
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public AlarmLevel Level { get; set; }
        public DateTime RaisedTime { get; set; }
        public DateTime? AcknowledgedTime { get; set; }
        public DateTime? ClearedTime { get; set; }
        public string Source { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// 报警级别
    /// </summary>
    public enum AlarmLevel
    {
        Info,
        Warning,
        Error,
        Critical
    }
}

