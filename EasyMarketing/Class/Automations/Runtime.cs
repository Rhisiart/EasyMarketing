using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.Class.Automations
{
    public class Runtime
    {
        List<string> Days = new List<string>();

        public Runtime()
        {
            Days.Add("sunday");
            Days.Add("monday");
            Days.Add("tuesday");
            Days.Add("wednesday");
            Days.Add("thursday");
            Days.Add("friday");
            Days.Add("saturday");
        }
    }
}
