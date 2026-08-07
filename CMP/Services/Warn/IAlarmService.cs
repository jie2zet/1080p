using CMP.Models.Warn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Services.Warn
{
    
    public interface IAlarmService
    {
        List<Alarm> GetAllAlarms();
        bool AddAlarm(Alarm alarm);
        int AddAlarms(List<Alarm> alarms);
        bool UpdateAlarm(Alarm alarm);
        bool DeleteAlarms(List<Alarm> alarms);
        List<Alarm> GetAlarmsByCondition(Alarm alarm,DateTime? startTime ,DateTime? endTime);
    }
    
}
