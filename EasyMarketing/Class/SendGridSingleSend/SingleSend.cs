using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.Class.SendGridSingleSend
{
    public class SingleSend
    {
        [JsonProperty("send_at", NullValueHandling = NullValueHandling.Ignore)]
        public string Schedule { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
        [JsonProperty("send_to", NullValueHandling = NullValueHandling.Ignore)]
        public SendTo SendTo { get; set; }

        [JsonProperty("email_config", NullValueHandling = NullValueHandling.Ignore)]
        public EmailConfig EmailConfig { get; set; }

        public SingleSend(string schedule)
        {
            this.Schedule = schedule;
        }

        public SingleSend(SendTo send)
        {
            this.SendTo = send;
        }

        public SingleSend(string name, EmailConfig emailConfig)
        {
            this.Name = name;
            this.EmailConfig = emailConfig;
        }
    }
}
