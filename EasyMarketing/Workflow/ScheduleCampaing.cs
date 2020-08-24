using CrmEarlyBound;
using EasyMarketing.Api;
using EasyMarketing.Class;
using EasyMarketing.Class.Campaigns;
using EasyMarketing.Class.SendGridSingleSend;
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
    public class ScheduleCampaing : CodeActivity
    {
        [RequiredArgument]
        [Input("Campaign")]
        [ReferenceTarget("campaign")]
        public InArgument<EntityReference> Campaign { get; set; }

        [RequiredArgument]
        [Input("Schedule")]
        public InArgument<string> Schedule { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            ITracingService tracing = context.GetExtension<ITracingService>();
            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
            IOrganizationService service = context.GetExtension<IOrganizationServiceFactory>().CreateOrganizationService(workflowContext.InitiatingUserId);

            Campaign campaign = service.Retrieve(Campaign.Get(context).LogicalName, Campaign.Get(context).Id, new ColumnSet("find_platformmarketing", "find_mailchimp_integration_count", "find_schedule", "find_mail_integration_started", "find_mailchimpcampaignid", "find_sendgridid")).ToEntity<Campaign>();

            if (campaign.find_mailchimp_integration_count == 0)
            {
                throw new Exception("Não existe nenhum dado sobre esta campanha");
            }

            ScheduleCampaign(service, campaign, Schedule.Get(context));
        }

        private void ScheduleCampaign(IOrganizationService service, Campaign campaign, string dateTime)
        {
            FuncLibrary aux = new FuncLibrary();
            find_marketingautomationintegration mailChimpObj = aux.GetMailchimpInfo(service);
            ApiRoot root = new ApiRoot();

            if (campaign.find_PlatformMarketing == false)
            {
                PostActionScheduleMail(service, campaign, mailChimpObj, root, dateTime);
            }
            else
            {
                PutScheduleNow(service,campaign, mailChimpObj, root, dateTime);
            }

        }

        private void PutScheduleNow(IOrganizationService service,Campaign campaign, find_marketingautomationintegration mailChimpObj, ApiRoot root, string dateTime)
        {
            Apihelper.InitializeClient(mailChimpObj.find_SendGridIntegration);
            SingleSend singleSend = new SingleSend(dateTime);

            Task<bool> t = Task.Run(() => root.PutSchedule(singleSend, campaign.find_SendGridId));
            t.Wait();
            if(t.IsCompleted && t.Result)
            {
                campaign.find_mail_integration_started = true;
                campaign.find_Schedule = DateTime.Parse(dateTime);
                service.Update(campaign);
            }
            else
            {
                throw new Exception();
            }
        }

        private void PostActionScheduleMail(IOrganizationService service, Campaign campaign, find_marketingautomationintegration mailChimpObj, ApiRoot root, string dateTime)
        {
            Apihelper.InitializeClient(mailChimpObj.find_MailChimpIntegration);
            string[] server = mailChimpObj.find_MailChimpIntegration.Split('-');
            Schedule schedule = new Schedule(dateTime);

            Task<bool> t = Task.Run(() => root.PostScheduleCampaign(schedule, server[1], campaign.find_mailChimpCampaignID));
            t.Wait();
            if (t.IsCompleted && t.Result)
            {
                campaign.find_mail_integration_started = true;
                campaign.find_Schedule = DateTime.Parse(dateTime);
                service.Update(campaign);
            }
            else
            {
                throw new Exception();
            }
        }
    }
}
