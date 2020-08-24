using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.Class.Campaigns
{
    public class Settings
    {
        [JsonProperty("subject_line", NullValueHandling = NullValueHandling.Ignore)]
        public string Subject { get; set; }
        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }
        [JsonProperty("from_name", NullValueHandling = NullValueHandling.Ignore)]
        public string FromName { get; set; }
        [JsonProperty("reply_to", NullValueHandling = NullValueHandling.Ignore)]
        public string ReplayTo { get; set; }
        //[JsonProperty("template_id", NullValueHandling = NullValueHandling.Ignore)]
        //public static int TemplateId = 138;

        public Settings(string fromName,string title,string subject,string replay)
        {
            this.FromName = fromName;
            this.Title = title;
            this.Subject = subject;
            this.ReplayTo = replay;
        }
    }
}
