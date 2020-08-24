using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.Api
{
    public class HttpClientPath
    {
        public HttpClient httpClient { get; set; }

        public HttpClientPath(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        public async Task<HttpResponseMessage> PatchAsync(string requestUri, HttpContent content)
        {
            HttpMethod method = new HttpMethod("PATCH");

            HttpRequestMessage request = new HttpRequestMessage(method, requestUri)
            {
                Content = content
            };

            return await httpClient.SendAsync(request);
        }
    }
}
