using CrmEarlyBound;
using EasyMarketing.Api;
using EasyMarketing.Class;
using EasyMarketing.Class.Reports;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.HtmlControls;

namespace EasyMarketing.Workflow
{
    public class SyncMailchimpToCrm : CodeActivity
    {
        [RequiredArgument]
        [Input("Campaign")]
        [ReferenceTarget("campaign")]
        public InArgument<EntityReference> Campaign { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            ITracingService tracing = context.GetExtension<ITracingService>();
            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
            IOrganizationService service = context.GetExtension<IOrganizationServiceFactory>().CreateOrganizationService(workflowContext.InitiatingUserId); 

            FuncLibrary aux = new FuncLibrary();
            find_marketingautomationintegration mailChimpObj = aux.GetMailchimpInfo(service);
            Apihelper.InitializeClient(mailChimpObj.find_MailChimpIntegration);
            ApiRoot root = new ApiRoot();
            string[] server = mailChimpObj.find_MailChimpIntegration.Split('-');

            Campaign campaign = service.Retrieve(Campaign.Get(context).LogicalName,Campaign.Get(context).Id, new ColumnSet("name", "find_mailchimpcampaignid")).ToEntity<Campaign>();
            find_metric metric = GetMetricByCampaign(service, campaign);
            List<find_metricdetails> metricDetailsList = GetMetricDetailsByMetric(service, metric);

            ProcessAtivitiesByMember(service, metricDetailsList,aux,root,server[1],campaign,metric, tracing);
        }

        private void ProcessAtivitiesByMember(IOrganizationService service, List<find_metricdetails> metricDetailsList, FuncLibrary aux, ApiRoot root,string server, Campaign campaign, find_metric metric, ITracingService tracing)
        {
            bool anyUpdate = false;
            foreach (find_metricdetails metricDetails in metricDetailsList)
            {
                int numOpens = 0, numClicks = 0;
                string md5mail = aux.GetMailMd(GetMailFromMember(service, metricDetails));
                DateTime? lastUpdate = GetLastUpdateByMetricDetail(service, metricDetails);
                List<Class.Reports.Activity> activitiesList = GetlastActivityByMetricDetail(root, server, campaign, md5mail).Activity;

                if (activitiesList.Count != 0 && lastUpdate == null)
                {
                    ProcessAtivities(service, metricDetails, numOpens, numClicks, activitiesList, tracing);
                    anyUpdate = true;
                }
                else if (activitiesList.Count != 0 && GetDateTimeFromLastActivity(activitiesList, (DateTime)lastUpdate).Count != 0)
                {
                    ProcessAtivities(service, metricDetails, numOpens, numClicks, GetDateTimeFromLastActivity(activitiesList, (DateTime)lastUpdate), tracing);
                    anyUpdate = true;
                }
                UpdateOpensAndClicksMetricDetails(service, metricDetails, numOpens, numClicks);
            }

            if (anyUpdate)
            {
                UpdateMetric(service, root, server, campaign, metric);
            }
        }

        private void ProcessAtivities(IOrganizationService service, find_metricdetails metricDetails, int numOpens, int numClicks, List<Class.Reports.Activity> activitiesList, ITracingService tracing)
        {
            foreach (Class.Reports.Activity activity in activitiesList)
            {
                if (activity.Action.Equals("open"))
                {
                    numOpens++;
                }
                else if (activity.Action.Equals("click"))
                {
                    numClicks++;
                }
                CreateActivities(service, activity, metricDetails);
            }
        }
        
        private void CreateActivities(IOrganizationService service, Class.Reports.Activity activity, find_metricdetails metricDetails)
        {
                find_emailactivity emailActivity = new find_emailactivity(Guid.NewGuid());
                emailActivity.find_Action = activity.Action;
                emailActivity.find_LookUpEmailActivityToMetricDetails = metricDetails.ToEntityReference();
                emailActivity.find_name = activity.Action + " " + activity.TimeStamp.ToString();
                emailActivity.find_Type = activity.Type;
                emailActivity.find_Url = activity.Url;
                emailActivity.find_TimeStamp = activity.TimeStamp;

                service.Create(emailActivity);
        }

        private void UpdateOpensAndClicksMetricDetails(IOrganizationService service,find_metricdetails metricDetails,int numOpens,int numClicks)
        {
            metricDetails.find_NumberOpens += numOpens;
            metricDetails.find_NumberClick += numClicks;

            service.Update(metricDetails);
        }

        private void UpdateMetric(IOrganizationService service,ApiRoot root ,string server, Campaign campaign, find_metric metric)
        {
            Report report = GetReportOfMetric(root, server, campaign);

            metric.find_Bounced = report.bounces.hard_bounces + report.bounces.soft_bounces;
            metric.find_PeopleWhoClicked = report.clicks.unique_clicks;
            metric.find_PeopleWhoOpened = report.opens.unique_opens;
            metric.find_SuccessfulDeliveries = report.emails_sent - (report.bounces.hard_bounces + report.bounces.soft_bounces);
            metric.find_Send = report.emails_sent;
            metric.find_TotalClicks = report.clicks.clicks_total;
            metric.find_TotalTimesOpened = report.opens.opens_total;
            metric.find_UnOpened = report.emails_sent - report.opens.unique_opens;
            metric.find_UnSubscribers = report.unsubscribed;
            metric.find_ForwardCount = report.forwards.forwards_count;
            metric.find_ForwardsOpens = report.forwards.forwards_opens;

            service.Update(metric);
        }

        private string GetMailFromMember(IOrganizationService service, find_metricdetails metricdetails)
        {
            if (metricdetails.find_LookUpAccount != null)
            {
                return service.Retrieve(metricdetails.find_LookUpAccount.LogicalName, metricdetails.find_LookUpAccount.Id, new ColumnSet("emailaddress1")).ToEntity<Account>().EMailAddress1;
            }else if(metricdetails.find_LookUpContact != null)
            {
                return service.Retrieve(metricdetails.find_LookUpContact.LogicalName, metricdetails.find_LookUpContact.Id, new ColumnSet("emailaddress1")).ToEntity<Contact>().EMailAddress1;
            }
            else if(metricdetails.find_LookUpLead != null)
            {
                return service.Retrieve(metricdetails.find_LookUpLead.LogicalName, metricdetails.find_LookUpLead.Id, new ColumnSet("emailaddress1")).ToEntity<Lead>().EMailAddress1;
            }else
            {
                throw new Exception();
            }
        }

        private find_metric GetMetricByCampaign(IOrganizationService service, Campaign campaign)
        {
            QueryExpression QEfind_metric = new QueryExpression("find_metric");
            QEfind_metric.ColumnSet.AddColumns("find_name");
            QEfind_metric.Criteria.AddCondition("campaign", "campaignid", ConditionOperator.Equal, campaign.Id);
            QEfind_metric.AddLink("campaign", "find_lookupcampaign", "campaignid");

            return service.RetrieveMultiple(QEfind_metric).Entities[0].ToEntity<find_metric>();
        }

        private List<find_metricdetails> GetMetricDetailsByMetric(IOrganizationService service, find_metric metric)
        {
            QueryExpression QEfind_metricdetails = new QueryExpression("find_metricdetails");
            QEfind_metricdetails.ColumnSet.AddColumns("find_name", "find_lookupcontact", "find_lookupaccount", "find_lookuplead", "find_numberclick", "find_numberopens");
            QEfind_metricdetails.Criteria.AddCondition("find_metric", "find_metricid", ConditionOperator.Equal, metric.Id);
            QEfind_metricdetails.AddLink("find_metric", "find_lookupmetric", "find_metricid");

            return service.RetrieveMultiple(QEfind_metricdetails).Entities.Select(entity => entity.ToEntity<find_metricdetails>()).ToList();
        }

        private EmailActivity GetlastActivityByMetricDetail(ApiRoot root,string server,Campaign campaign,string md5Mail)
        {
            Task<EmailActivity> t = Task.Run(() => root.GetEmailActivity(server, campaign.find_mailChimpCampaignID, md5Mail));
            t.Wait();

            return t.IsCompleted ? t.Result : throw new Exception();
        }

        private List<Class.Reports.Activity> GetDateTimeFromLastActivity(List<Class.Reports.Activity> activitiesList, DateTime lastUpdate)
        {
            List<Class.Reports.Activity> activitiesListBiggerLastUpdate = new List<Class.Reports.Activity>();

            foreach (Class.Reports.Activity activity in activitiesList)
            {
                if (activity.TimeStamp > lastUpdate)
                {
                    activitiesListBiggerLastUpdate.Add(activity);
                }
            }

            return activitiesListBiggerLastUpdate;
        }

        private DateTime? GetLastUpdateByMetricDetail(IOrganizationService service, find_metricdetails metricdetails)
        {
            QueryExpression QEfind_emailactivity = new QueryExpression("find_emailactivity");
            QEfind_emailactivity.ColumnSet.AddColumns("find_timestamp");
            QEfind_emailactivity.AddOrder("find_timestamp", OrderType.Descending);
            QEfind_emailactivity.Criteria.AddCondition("find_metricdetails", "find_metricdetailsid", ConditionOperator.Equal, metricdetails.Id);
            QEfind_emailactivity.AddLink("find_metricdetails", "find_lookupemailactivitytometricdetails", "find_metricdetailsid");

            return service.RetrieveMultiple(QEfind_emailactivity).Entities.Count != 0 ? service.RetrieveMultiple(QEfind_emailactivity).Entities[0].ToEntity<find_emailactivity>().find_TimeStamp : null;
        }

        private Report GetReportOfMetric(ApiRoot root,string server, Campaign campaign)
        {
            Task<Report> t = Task.Run(() => root.GetReportsEmail(server, campaign.find_mailChimpCampaignID));
            t.Wait();

            return t.IsCompleted ? t.Result : throw new Exception();
        }
    }
}
