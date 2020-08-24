using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.Class.Reports
{
    public class Report
    {
        public int emails_sent { get; set; }
        public int unsubscribed { get; set; }
        public int abuse_reports { get; set; }
        public Bounces bounces { get; set; }
        public Forwards forwards { get; set; }
        public Opens opens { get; set; }
        public Clicks clicks { get; set; }
        public ListStats list_stats { get; set; }

    }
}
