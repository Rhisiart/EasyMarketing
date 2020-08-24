using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.Class.Campaigns
{
    public class Schedule
    {
        [JsonProperty("schedule_time", NullValueHandling = NullValueHandling.Ignore)]
        public string ScheduleTime { get; set; }
        public Schedule(string scheduleTime)
        {
            this.ScheduleTime = scheduleTime;
        }
    }
}
