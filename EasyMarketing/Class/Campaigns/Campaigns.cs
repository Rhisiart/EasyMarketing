using EasyMarketing.Class.Campaigns;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.Class.Campaigns
{
    public class Campaigns
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public static string Type = "regular";
        [JsonProperty("recipients", NullValueHandling = NullValueHandling.Ignore)]
        public Recipients Recipients  { get; set; }
        [JsonProperty("settings", NullValueHandling = NullValueHandling.Ignore)]
        public Settings Settings { get; set; }

        public Campaigns(Settings settings,Recipients recipients)
        {
            this.Settings = settings;
            this.Recipients = recipients;
        }

        public Campaigns( Recipients recipients)
        {
            this.Recipients = recipients;
        }
    }
}
