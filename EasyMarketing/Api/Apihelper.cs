using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CrmEarlyBound;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace EasyMarketing.Api
{
    public class Apihelper
    {
        public static HttpClient apiClient { get; set; }

        public static void InitializeClient(string api)
        { 
            apiClient = new HttpClient();
            apiClient.DefaultRequestHeaders.Authorization =
                     new AuthenticationHeaderValue("Bearer", api);
            apiClient.DefaultRequestHeaders.Accept.Clear();
            apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}
