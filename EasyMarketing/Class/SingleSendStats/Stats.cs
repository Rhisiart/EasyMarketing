using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.Class.SingleSendStats
{
    public class Stats
    {
        public int bounces { get; set; }
        public int clicks { get; set; }
        public int delivered { get; set; }
        public int opens { get; set; }
        public int requests { get; set; }
        public int unique_clicks { get; set; }
        public int unique_opens { get; set; }
        public int unsubscribes { get; set; }

    }
}
