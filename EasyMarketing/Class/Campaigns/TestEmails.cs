using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.Class.Campaigns
{
    public class TestEmails
    {
        [JsonProperty("send_type", NullValueHandling = NullValueHandling.Ignore)]
        public static string Type = "plaintext";
        [JsonProperty("test_emails", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> ListEmails { get; set; }
        public TestEmails(List<string> listEmails)
        {
            this.ListEmails = listEmails;
        }
    }
}
