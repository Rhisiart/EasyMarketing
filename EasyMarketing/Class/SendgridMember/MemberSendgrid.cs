using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.Class.SendgridMember
{
    public class MemberSendgrid
    {  
        [JsonProperty("list_ids", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> ListIds { get; set; }
        [JsonProperty("contacts", NullValueHandling = NullValueHandling.Ignore)]
        public List<Contacts> ListContacts { get; set; }

        public MemberSendgrid( List<string> listIds, List<Contacts> ListContact)
        {
            this.ListContacts = ListContact;
            this.ListIds = listIds;
        }
    }
}
