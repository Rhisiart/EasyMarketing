using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace EasyMarketing.Class.SendGridSingleSend
{
    public class SendTo
    {
        [JsonProperty("list_ids", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> List { get; set; }
        public SendTo(List<string> list)
        {
            this.List = list;
        }
    }
}
