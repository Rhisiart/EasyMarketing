using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.Class.SendgridMember
{
    public class Contacts
    {
        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public string EmailAddress { get; set; }

        public Contacts(string email_address)
        {
            this.EmailAddress = email_address;
        }
    }
}
