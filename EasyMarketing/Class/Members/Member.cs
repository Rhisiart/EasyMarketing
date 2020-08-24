using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyMarketing.Class.Interests;

namespace EasyMarketing.Class.Members
{
    public class Member
    {
        [JsonProperty("email_address",NullValueHandling = NullValueHandling.Ignore)]
        public string EmailAddress { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue("")]
        public string Status { get; set; }

        [JsonProperty("interests",NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Group Interests { get; set; }

        //[JsonProperty("merge_fields", NullValueHandling = NullValueHandling.Ignore)]
        //public merge_fields MergeFields;

        //public Member(string email_address, string status, merge_fields merge_fields)
        //{
        //    this.EmailAddress = email_address;
        //    this.Status = status;
        //    this.MergeFields = merge_fields;
        //}

        public Member(string email_address, string status, Group interests)
        {
            this.EmailAddress = email_address;
            this.Status = status;
            this.Interests = interests;
        }

        public Member(string email_address, Group interests)
        {
            this.EmailAddress = email_address;
            this.Interests = interests;
        }

    }
}
