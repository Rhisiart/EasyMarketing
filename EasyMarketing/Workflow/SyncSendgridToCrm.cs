using CrmEarlyBound;
using EasyMarketing.Api;
using EasyMarketing.Class;
using EasyMarketing.Class.SingleSendStats;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.Workflow
{
    public class SyncSendgridToCrm : CodeActivity
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
            find_marketingautomationintegration accountObj = aux.GetMailchimpInfo(service);
            Apihelper.InitializeClient(accountObj.find_SendGridIntegration);
            ApiRoot root = new ApiRoot();

            Campaign campaign = service.Retrieve(Campaign.Get(context).LogicalName, Campaign.Get(context).Id, new ColumnSet("name", "find_sendgridid")).ToEntity<Campaign>();
            find_metric metric = GetMetricByCampaign(service, campaign);
            
            UpdateMetric(service, root, campaign, metric, tracing);
 
        }

        private void UpdateMetric(IOrganizationService service, ApiRoot root, Campaign campaign, find_metric metric, ITracingService tracing)
        {
            Reports report = GetReportOfMetric(root, campaign, tracing); 

            metric.find_Bounced = report.results[0].stats.bounces;
            metric.find_PeopleWhoClicked = report.results[0].stats.unique_clicks;
            metric.find_PeopleWhoOpened = report.results[0].stats.unique_opens;
            metric.find_SuccessfulDeliveries = report.results[0].stats.delivered - report.results[0].stats.bounces;
            metric.find_Send = report.results[0].stats.delivered;
            metric.find_TotalClicks = report.results[0].stats.clicks;
            metric.find_TotalTimesOpened = report.results[0].stats.opens;
            metric.find_UnOpened = report.results[0].stats.delivered - report.results[0].stats.unique_opens;
            metric.find_UnSubscribers = report.results[0].stats.unsubscribes;
            metric.find_ForwardCount = 0;
            metric.find_ForwardsOpens = 0;

            service.Update(metric);
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

        private Reports GetReportOfMetric(ApiRoot root, Campaign campaign, ITracingService tracing)
        {
            Task<Reports> t = Task.Run(() => root.GetMetricsFromSingleSend(campaign.find_SendGridId, tracing));
            t.Wait();
            if (t.IsCompleted)
            {
                return t.Result;
            }
            else
            {
                throw new Exception();
            }
        }
    }
    
}
