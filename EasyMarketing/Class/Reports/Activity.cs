using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.Class.Reports
{
    public class Activity
    {
        public string Action { get; set; }
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
        public DateTime TimeStamp { get; set; }
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }
    }
}
