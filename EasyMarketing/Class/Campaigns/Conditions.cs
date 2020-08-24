using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.Class.Campaigns
{
    public class Conditions
    {
        [JsonProperty("condition_type")]
        public static string Type = "Interests";

        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("op")]
        public static string Op = "interestcontains";

        [JsonProperty("value")]
        public List<string> Value { get; set; }

        public Conditions(string field, List<string> value)
        {
            this.Field = "interests-" + field;
            this.Value = value;
        }
    }
}
