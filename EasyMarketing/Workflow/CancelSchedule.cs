using CrmEarlyBound;
using EasyMarketing.Api;
using EasyMarketing.Class;
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
    public class CancelSchedule : CodeActivity
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

            Campaign campaign = service.Retrieve(Campaign.Get(context).LogicalName, Campaign.Get(context).Id, new ColumnSet("find_platformmarketing", "find_schedule", "find_mail_integration_started", "find_mailchimpcampaignid", "find_sendgridid")).ToEntity<Campaign>();
            CancelScheduleCampaign(service, campaign);
        }

        private void CancelScheduleCampaign(IOrganizationService service, Campaign campaign)
        {
            FuncLibrary aux = new FuncLibrary();
            find_marketingautomationintegration mailChimpObj = aux.GetMailchimpInfo(service);
            ApiRoot root = new ApiRoot();

            if (campaign.find_PlatformMarketing == false)
            {
                PostActionCancelScheduleMail(service, campaign, mailChimpObj, root);
            }
            else
            {
                PutScheduleNow(service, campaign, mailChimpObj, root);
            }

        }

        private void PutScheduleNow(IOrganizationService service, Campaign campaign, find_marketingautomationintegration mailChimpObj, ApiRoot root)
        {
            Apihelper.InitializeClient(mailChimpObj.find_SendGridIntegration);

            Task<bool> t = Task.Run(() => root.DeleteScheduleCampaign(campaign.find_SendGridId));
            t.Wait();
            if (t.IsCompleted && t.Result)
            {
                campaign.find_mail_integration_started = false;
                campaign.find_Schedule = null;
                service.Update(campaign);
            }
            else
            {
                throw new Exception();
            }
        }

        private void PostActionCancelScheduleMail(IOrganizationService service, Campaign campaign, find_marketingautomationintegration mailChimpObj, ApiRoot root)
        {
            Apihelper.InitializeClient(mailChimpObj.find_MailChimpIntegration);
            string[] server = mailChimpObj.find_MailChimpIntegration.Split('-');

            Task<bool> t = Task.Run(() => root.PostCancelScheduleCampaign(server[1], campaign.find_mailChimpCampaignID));
            t.Wait();
            if (t.IsCompleted && t.Result)
            {
                campaign.find_mail_integration_started = false;
                campaign.find_Schedule = null;
                service.Update(campaign);
            }
            else
            {
                throw new Exception();
            }
        }

    }
}
