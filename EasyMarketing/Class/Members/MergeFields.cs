using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace EasyMarketing.Class.Members
{
    public class MergeFields
    {
        [JsonProperty("FNAME", NullValueHandling = NullValueHandling.Ignore)]
        public string Fname{ get; set; }
        [JsonProperty("LNAME", NullValueHandling = NullValueHandling.Ignore)]
        public string Lname { get; set; }
        [JsonProperty("PHONE", NullValueHandling = NullValueHandling.Ignore)]
        public Nullable<int> Phone { get; set; }
        [JsonProperty("AGE", NullValueHandling = NullValueHandling.Ignore)]
        public Nullable<int> Age { get; set; }
        [JsonProperty("GENDER", NullValueHandling = NullValueHandling.Ignore,DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue("")]
        public string Gender { get; set; }
        [JsonProperty("MSTATUS", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue("")]
        public string MartialStatus { get; set; }
        [JsonProperty("COUNTRY", NullValueHandling = NullValueHandling.Ignore)]
        public string Country { get; set; }
        [JsonProperty("STATE", NullValueHandling = NullValueHandling.Ignore)]
        public string State { get; set; }
        [JsonProperty("CITY", NullValueHandling = NullValueHandling.Ignore)]
        public string City { get; set; }
        public MergeFields(string Fname, string Lname, Nullable<int> Phone, Nullable<int> Age, string Gender,string Mstatus,string Country,string State,string City)
        {
            this.Fname = Fname;
            this.Lname = Lname;
            this.Phone = Phone;
            this.Age = Age;
            this.Gender = Gender;
            this.MartialStatus = Mstatus;
            this.Country = Country;
            this.State = State;
            this.City = City;
        }

    }
}
