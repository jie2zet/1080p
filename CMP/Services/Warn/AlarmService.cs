using CMP.Models.Warn;
using CMP.Utils;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Services.Warn
{
    
    public class AlarmService : IAlarmService
    {
        private readonly ISqlSugarClient _db;

        public AlarmService()
        {
            _db = SqlSugarHelper.Db;
        }

        public List<Alarm> GetAllAlarms()
        {
            var query = _db.Queryable<Alarm>();
            return query.OrderByPropertyName("AlarmTime", OrderByType.Desc).ToList();
        }

        public bool AddAlarm(Alarm alarm)
        {
            // 确保AlarmTime字段格式化为 yyyy-MM-dd HH:mm:ss，去除毫秒
            alarm.AlarmTime = alarm.AlarmTime.Date.AddHours(alarm.AlarmTime.Hour)
                             .AddMinutes(alarm.AlarmTime.Minute)
                             .AddSeconds(alarm.AlarmTime.Second);
            var result = _db.Insertable(alarm).ExecuteCommand();
            return result > 0;
        }
        public int AddAlarms(List<Alarm> alarms)
        {
            if (alarms == null || alarms.Count == 0)
                return 0;
            // 批量格式化AlarmTime字段，去除毫秒
            foreach (var alarm in alarms)
            {
                alarm.AlarmTime = alarm.AlarmTime.Date.AddHours(alarm.AlarmTime.Hour)
                                 .AddMinutes(alarm.AlarmTime.Minute)
                                 .AddSeconds(alarm.AlarmTime.Second);
            }
            var result = _db.Insertable(alarms).ExecuteCommand();
            return result;
        }
        public bool UpdateAlarm(Alarm alarm)
        {
            var result = _db.Updateable(alarm).ExecuteCommand();
            return result > 0;
        }

        public bool DeleteAlarms(List<Alarm> alarms)
        {
            var ids = alarms.Select(a => a.AlarmId).ToList();
            var result = _db.Deleteable<Alarm>().In(ids).ExecuteCommand();
            return result > 0;
        }

        public List<Alarm> GetAlarmsByCondition(Alarm alarm, DateTime? startTime = null, DateTime? endTime = null)
        {
            var query = _db.Queryable<Alarm>();

            if (!string.IsNullOrEmpty(alarm.AlarmName))
                query = query.Where(a => a.AlarmName.Contains(alarm.AlarmName));

            if (!string.IsNullOrEmpty(alarm.Type))
                query = query.Where(a => a.Type.Equals(alarm.Type));

            if (alarm.DefaultOperation != null)
                query = query.Where(a => a.DefaultOperation.Equals(alarm.DefaultOperation));

            // 添加时间范围查询
            if (startTime.HasValue)
            {
                var formattedStartTime = startTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                var parsedStartTime = DateTime.Parse(formattedStartTime);
                query = query.Where(a => a.AlarmTime >= startTime.Value);
            }
                

            if (endTime.HasValue)
            {
                // 格式化结束时间，精确到秒
                var formattedEndTime = endTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                var parsedEndTime = DateTime.Parse(formattedEndTime);
                query = query.Where(a => a.AlarmTime <= parsedEndTime);
            }
            //query = query.Where(a => a.AlarmTime <= endTime.Value);

            // 可继续添加其他字段...
            query.OrderByPropertyName("AlarmTime", OrderByType.Desc);
            return query.ToList();
        }
    }
}
