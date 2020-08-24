using EasyMarketing.Class.Campaigns;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.Class.Automations
{
    public class Automation
    {
        [JsonProperty("recipents", NullValueHandling = NullValueHandling.Ignore)]
        public Recipents Recipents { get; set; }
        [JsonProperty("settings", NullValueHandling = NullValueHandling.Ignore)]
        public Settings Settings { get; set; }
        [JsonProperty("trigger_settings", NullValueHandling = NullValueHandling.Ignore)]
        public TriggerSettings TriggerSettings { get; set; }
        public Automation(Recipents recipents,Settings settings,TriggerSettings triggerSettings)
        {
            this.Recipents = recipents;
            this.Settings = settings;
            this.TriggerSettings = triggerSettings;
        }
    }
}
