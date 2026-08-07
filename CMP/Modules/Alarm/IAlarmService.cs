using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CMP.Modules.Alarm.Models;

namespace CMP.Modules.Alarm
{
    /// <summary>
    /// 报警服务接口
    /// </summary>
    public interface IAlarmService
    {
        /// <summary>
        /// 触发报警
        /// </summary>
        void RaiseAlarm(CMP.Models.Warn.Alarm alarm);

        /// <summary>
        /// 确认报警
        /// </summary>
        Task<bool> AcknowledgeAlarmAsync(string alarmId);

        /// <summary>
        /// 获取活动报警列表
        /// </summary>
        IEnumerable<CMP.Models.Warn.Alarm> GetActiveAlarms();

        /// <summary>
        /// 获取报警历史
        /// </summary>
        Task<IEnumerable<CMP.Models.Warn.Alarm>> GetAlarmHistoryAsync(DateTime startTime, DateTime endTime);

        /// <summary>
        /// 清除报警
        /// </summary>
        Task<bool> ClearAlarmAsync(string alarmId);

        /// <summary>
        /// 报警触发事件
        /// </summary>
        event EventHandler<AlarmEventArgs> AlarmRaised;

        /// <summary>
        /// 报警确认事件
        /// </summary>
        event EventHandler<AlarmEventArgs> AlarmAcknowledged;
    }

    /// <summary>
    /// 报警事件参数
    /// </summary>
    public class AlarmEventArgs : EventArgs
    {
        public CMP.Models.Warn.Alarm Alarm { get; set; }
    }
}

