using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EasyMarketing.Class;
using Microsoft.Xrm.Sdk;
using System;
using EasyMarketing.Class.Members;
using EasyMarketing.Class.Campaigns;
using EasyMarketing.Class.Interests;
using EasyMarketing.Class.Reports;
using EasyMarketing.Class.SendgridMember;
using System.Net.Http.Headers;
using EasyMarketing.Class.SendGridSingleSend;
using EasyMarketing.Class.SingleSendStats;

namespace EasyMarketing.Api
{
    public class ApiRoot
    {
        public async Task<MailChimpCampaign> PostInterest(Group interests, string server, string listId)
        {
            string json = JsonConvert.SerializeObject(interests);
            StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
            using (HttpResponseMessage response = await Apihelper.apiClient.PostAsync($"https://{server}.api.mailchimp.com/3.0/lists/{listId}/interest-categories/53afadfa03/interests", data))
            {
                if (response.IsSuccessStatusCode)
                {
                    string objResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<MailChimpCampaign>(objResponse);
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }

            }
        }

        public async Task<bool> GetMember(string server, string listId,string mail)
        {
            using (HttpResponseMessage response = await Apihelper.apiClient.GetAsync($"https://{server}.api.mailchimp.com/3.0/lists/{listId}/members/{mail}")) 
            {   
                if(response.IsSuccessStatusCode)
                {
                    string objResponse = await response.Content.ReadAsStringAsync();
                    Members member = JsonConvert.DeserializeObject<Members>(objResponse);
                    if(member.Status.Equals("archived"))
                    {
                        return false;
                    }
                    return true;
                }
                if(response.StatusCode.ToString().Equals("NotFound"))
                {
                    return false;
                }
                throw new Exception(response.ReasonPhrase);
            }
        }

        public async Task PostMember(Member member, string server, string listId, string idInterests)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            DynamicPropertyNameJson memberInterestsJson = new DynamicPropertyNameJson(idInterests);
            settings.ContractResolver = memberInterestsJson;
            string json = JsonConvert.SerializeObject(member, settings);
            StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
            using (HttpResponseMessage response = await Apihelper.apiClient.PostAsync($"https://{server}.api.mailchimp.com/3.0/lists/{listId}/members", data)) 
            {
                //if (response.IsSuccessStatusCode)
                //    result = response.Content.ReadAsStringAsync().Result;

            }
        }

        public async Task PutMember(Member member, string server, string listId, string idInterests, string mail)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            DynamicPropertyNameJson memberInterestsJson = new DynamicPropertyNameJson(idInterests);
            settings.ContractResolver = memberInterestsJson;
            string json = JsonConvert.SerializeObject(member, settings);
            StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
            using (HttpResponseMessage response = await Apihelper.apiClient.PutAsync($"https://{server}.api.mailchimp.com/3.0/lists/{listId}/members/{mail}", data)) 
            {
                //if (response.IsSuccessStatusCode)
                //    result = response.Content.ReadAsStringAsync().Result;
            }
        }

        public async Task DeleteInterest(string interestId, string server, string listId)
        {
            using (HttpResponseMessage response = await Apihelper.apiClient.DeleteAsync($"https://{server}.api.mailchimp.com/3.0/lists/{listId}/interest-categories/53afadfa03/interests/{interestId}"))
            {
                //if (response.IsSuccessStatusCode)
                //    result = response.Content.ReadAsStringAsync().Result;
            }
        }

        public async Task PathInterest(Group interest, string interestId, string server, string listId)
        {
            HttpClientPath PathAsync = new HttpClientPath(Apihelper.apiClient);
            string json = JsonConvert.SerializeObject(interest);
            StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
            using (HttpResponseMessage response = await PathAsync.PatchAsync($"https://{server}.api.mailchimp.com/3.0/lists/{listId}/interest-categories/53afadfa03/interests/{interestId}", data))
            {
                //if (response.IsSuccessStatusCode)
                //    result = response.Content.ReadAsStringAsync().Result;
            }
        }

        public async Task<MailChimpCampaign> PostCampaign(Campaigns campaigns, string server)
        {
            string json = JsonConvert.SerializeObject(campaigns);
            StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
            using (HttpResponseMessage response = await Apihelper.apiClient.PostAsync($"https://{server}.api.mailchimp.com/3.0/campaigns", data)) 
            {
                if (response.IsSuccessStatusCode)
                {
                    string objResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<MailChimpCampaign>(objResponse);
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public async Task PathCampaign(Campaigns campaigns, string server, string campaignId)
        {
            HttpClientPath PathAsync = new HttpClientPath(Apihelper.apiClient);
            string json = JsonConvert.SerializeObject(campaigns);
            StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
            using (HttpResponseMessage response = await PathAsync.PatchAsync($"https://{server}.api.mailchimp.com/3.0/campaigns/{campaignId}", data)) 
            {
                //if (response.IsSuccessStatusCode)
                //    result = response.Content.ReadAsStringAsync().Result;
            }
        }

        public async Task<bool> PostSendEmail(string server, string campaignId)
        {
            using (HttpResponseMessage response = await Apihelper.apiClient.PostAsync($"https://{server}.api.mailchimp.com/3.0/campaigns/{campaignId}/actions/send",null)) 
            {
                return response.IsSuccessStatusCode;
            }
        }

        public async Task<Report> GetReportsEmail(string server, string campaignId)
        {
            using (HttpResponseMessage response = await Apihelper.apiClient.GetAsync($"https://{server}.api.mailchimp.com/3.0/reports/{campaignId}")) 
            {
                if (response.IsSuccessStatusCode)
                {
                    string objResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Report>(objResponse);
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public async Task<EmailActivity> GetEmailActivity(string server, string campaignId,string md5Mail)
        {
            using (HttpResponseMessage response = await Apihelper.apiClient.GetAsync($"https://{server}.api.mailchimp.com/3.0/reports/{campaignId}/email-activity/{md5Mail}"))
            {
                if (response.IsSuccessStatusCode)
                {
                    string objResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<EmailActivity>(objResponse);
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public async Task<MailChimpCampaign> PostCopyCampaign( string server,string campaignID)
        {
            using (HttpResponseMessage response = await Apihelper.apiClient.PostAsync($"https://{server}.api.mailchimp.com/3.0/campaigns/{campaignID}/actions/replicate", null))
            {
                if (response.IsSuccessStatusCode)
                {
                    string objResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<MailChimpCampaign>(objResponse);
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public async Task<bool> PostScheduleCampaign(Schedule dateTime, string server, string campaignID)
        {
            string json = JsonConvert.SerializeObject(dateTime);
            StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
            using (HttpResponseMessage response = await Apihelper.apiClient.PostAsync($"https://{server}.api.mailchimp.com/3.0/campaigns/{campaignID}/actions/schedule", data))
            {
                return response.IsSuccessStatusCode;
            }
        }

        public async Task<bool> PostCancelScheduleCampaign( string server, string campaignID)
        {
            using (HttpResponseMessage response = await Apihelper.apiClient.PostAsync($"https://{server}.api.mailchimp.com/3.0/campaigns/{campaignID}/actions/unschedule", null))
            {
                return response.IsSuccessStatusCode;
            }
        }

        public async Task PostTestsEmails(TestEmails testsEmails, string server, string campaignID)
        {
            string json = JsonConvert.SerializeObject(testsEmails);
            StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
            using (HttpResponseMessage response = await Apihelper.apiClient.PostAsync($"https://{server}.api.mailchimp.com/3.0/campaigns/{campaignID}/actions/test", data))
            {
                if (response.IsSuccessStatusCode)
                {

                }
            }
        }

        /*---------------------------------------------------------SendGrid Endpoints------------------------------------------------------------------------*/

        public async Task<MailChimpCampaign> PostList(Group interests)
        {
            string json = JsonConvert.SerializeObject(interests);
            StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
            using (HttpResponseMessage response = await Apihelper.apiClient.PostAsync("https://api.sendgrid.com/v3/marketing/lists", data))
            {
                if (response.IsSuccessStatusCode)
                {
                    string objResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<MailChimpCampaign>(objResponse);
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }

            }
        }

        public async Task PostOrPutMember(MemberSendgrid member, ITracingService tracing)
        {
            string json = JsonConvert.SerializeObject(member);
            StringContent data = new StringContent(json); // By Default application/json encode/charset is UTF-8 
            data.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            using (HttpResponseMessage response = await Apihelper.apiClient.PutAsync("https://api.sendgrid.com/v3/marketing/contacts",data))
            {
                if (response.IsSuccessStatusCode)
                {
                    tracing.Trace("aqui");
                }
            }
        }

        public async Task<bool> PutSchedule(SingleSend singleSend, string singleSendId)
        {
            string json = JsonConvert.SerializeObject(singleSend);
            StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
            using (HttpResponseMessage response = await Apihelper.apiClient.PutAsync($"https://api.sendgrid.com/v3/marketing/singlesends/{singleSendId}/schedule", data))
            {
                return response.IsSuccessStatusCode;
            }
        }

        public async Task<MailChimpCampaign> PostSingleSend(SingleSend singleSend)
        {
            string json = JsonConvert.SerializeObject(singleSend);
            StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
            using (HttpResponseMessage response = await Apihelper.apiClient.PostAsync("https://api.sendgrid.com/v3/marketing/singlesends", data))
            {
                if (response.IsSuccessStatusCode)
                {
                    string objResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<MailChimpCampaign>(objResponse);
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public async Task PathSingleSend(SingleSend singleSend, string singleSendId)
        {
            HttpClientPath PathAsync = new HttpClientPath(Apihelper.apiClient);
            string json = JsonConvert.SerializeObject(singleSend);
            StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
            using (HttpResponseMessage response = await PathAsync.PatchAsync($"https://api.sendgrid.com/v3/marketing/singlesends/{singleSendId}", data))
            {
                //if (response.IsSuccessStatusCode)
                //    result = response.Content.ReadAsStringAsync().Result;
            }
        }

        public async Task<Reports> GetMetricsFromSingleSend(string singleSendId, ITracingService tracing)
        {
            using (HttpResponseMessage response = await Apihelper.apiClient.GetAsync($"https://api.sendgrid.com/v3/marketing/stats/singlesends/{singleSendId}"))
            {
                if (response.IsSuccessStatusCode)
                {
                    string objResponse = await response.Content.ReadAsStringAsync();
                    tracing.Trace(objResponse);
                    return JsonConvert.DeserializeObject<Reports>(objResponse);
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public async Task<Reports> GetEmailAtivities(string singleSendId)
        {
            using (HttpResponseMessage response = await Apihelper.apiClient.GetAsync($"https://api.sendgrid.com/v3/messages?query=%28unique_args%5B%27singlesend_id%27%5D%3D%22{singleSendId}%22%29&limit=10"))
            {
                if (response.IsSuccessStatusCode)
                {
                    string objResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Reports>(objResponse);
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public async Task<MailChimpCampaign> PostCopySingleSend(string campaignID)
        {
            using (HttpResponseMessage response = await Apihelper.apiClient.PostAsync($"https://api.sendgrid.com/v3/marketing/singlesends/{campaignID}", null))
            {
                if (response.IsSuccessStatusCode)
                {
                    string objResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<MailChimpCampaign>(objResponse);
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public async Task PostTestsEmailsInSendGrid(Emails testsEmails)
        {
            string json = JsonConvert.SerializeObject(testsEmails);
            StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
            using (HttpResponseMessage response = await Apihelper.apiClient.PostAsync("https://api.sendgrid.com/v3/marketing/test/send_email", data))
            {
                if (response.IsSuccessStatusCode)
                {

                }
            }
        }

        public async Task<bool> DeleteScheduleCampaign(string singleSendId)
        {
            using (HttpResponseMessage response = await Apihelper.apiClient.DeleteAsync($"https://api.sendgrid.com/v3/marketing/singlesends/{singleSendId}/schedule"))
            {
                return response.IsSuccessStatusCode;
            }
        }
    }
}
