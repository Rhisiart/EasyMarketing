using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.Class.Members
{
    public class DynamicPropertyNameJson : DefaultContractResolver
    {
        private string propertyName;

        public DynamicPropertyNameJson()
        {
        }
        public DynamicPropertyNameJson(string name)
        {
            this.propertyName = name;
        }
        public static readonly DynamicPropertyNameJson Instance = new DynamicPropertyNameJson();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyName == "isInList")
            {
                property.PropertyName = propertyName;
            }
            return property;
        }
    }
}
