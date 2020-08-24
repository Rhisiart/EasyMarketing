using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.Class.SendGridSingleSend
{
    public class Emails
    {
        [JsonProperty("emails", NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        [JsonProperty("template_id", NullValueHandling = NullValueHandling.Ignore)]
        public static int TemplateID = 1111;

        [JsonProperty("from_address", NullValueHandling = NullValueHandling.Ignore)]
        public string FromAddress { get; set; }

        public Emails(string emails, string fromAddress)
        {
            this.Email = emails;
            this.FromAddress = fromAddress;
        }
    }
}
