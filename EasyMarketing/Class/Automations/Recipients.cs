using Microsoft.Crm.Sdk.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace EasyMarketing.Class.Campaigns
{
    public class Recipients
    {
        [JsonProperty("list_id", NullValueHandling = NullValueHandling.Ignore)]
        public string ListId { get; set; }

        [JsonProperty("segment_opts", NullValueHandling = NullValueHandling.Ignore)]
        public SegementOps Segement { get; set; }
        //[JsonProperty("segment_opts", NullValueHandling = NullValueHandling.Ignore)]
        //public MailChimpList SegmentId { get; set; }

        public Recipients(string listId)
        {
            this.ListId = listId;
        }

        public Recipients(SegementOps segementOps)
        {
            this.Segement = segementOps;
        }

        //public Recipients(string listId, MailChimpList segmentId)
        //{
        //    this.ListId = listId;
        //    this.SegmentId = segmentId;
        //}
    }
}
