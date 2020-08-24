using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.Class.Campaigns
{
  
    public class SegementOps
    {
        [JsonProperty("match")]
        public static string Match = "all"; // all is AND any is OR

        [JsonProperty("conditions")]
        public List<Conditions> Conditions { get; set; }

        public SegementOps(List<Conditions> conditions)
        {
            this.Conditions = conditions;
        }
    }
}
