using Microsoft.Xrm.Sdk.Workflow;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.Class.Interests
{
    public class Group
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? isInList { get; set; }

        public Group(string name)
        {
            this.Name = name;
        }

        public Group(bool inList)
        {
            this.isInList = inList;
        }
    }
}
