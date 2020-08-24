using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace EasyMarketing.Class.Automations
{
    public class TriggerSettings
    {
        [JsonProperty("workflow_type", NullValueHandling = NullValueHandling.Ignore)]
        public string WorkflowType { get; set; }
        [JsonProperty("workflow_title", NullValueHandling = NullValueHandling.Ignore)]
        public string WorkflowTitle { get; set; }
        [JsonProperty("runtime", NullValueHandling = NullValueHandling.Ignore)]
        public Runtime Runtime { get; set; }
        [JsonProperty("hours", NullValueHandling = NullValueHandling.Ignore)]
        public Hours Hours { get; set; }
    }
}
