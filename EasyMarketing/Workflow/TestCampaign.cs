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
    public class TestCampaign : CodeActivity
    {
        [RequiredArgument]
        [Input("Campaign")]
        [ReferenceTarget("campaign")]
        public InArgument<EntityReference> Campaign { get; set; }

        [RequiredArgument]
        [Input("Emails")]
        public InArgument<string> Emails { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            ITracingService tracing = context.GetExtension<ITracingService>();
            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
            IOrganizationService service = context.GetExtension<IOrganizationServiceFactory>().CreateOrganizationService(workflowContext.InitiatingUserId);

            tracing.Trace("aqui");
            Campaign campaign = service.Retrieve(Campaign.Get(context).LogicalName, Campaign.Get(context).Id, new ColumnSet("find_platformmarketing", "find_mailchimpcampaignid", "find_sendgridid")).ToEntity<Campaign>();
            TestActionCampaign(service, campaign, Emails.Get(context), tracing);
        }

        private void TestActionCampaign(IOrganizationService service, Campaign campaign, string emails, ITracingService tracing)
        {
            FuncLibrary aux = new FuncLibrary();
            find_marketingautomationintegration mailChimpObj = aux.GetMailchimpInfo(service);
            ApiRoot root = new ApiRoot();

            if (campaign.find_PlatformMarketing == false)
            {
                tracing.Trace("aq");
                PostActionSendTestEmailsMailchimp(campaign, mailChimpObj, root, emails, tracing);
            }
            else
            {
                PostActionSendTestEmailsSendGrid(mailChimpObj, root, emails);
            }

        }

        private void PostActionSendTestEmailsSendGrid(find_marketingautomationintegration mailChimpObj, ApiRoot root, string emails)
        {
            Apihelper.InitializeClient(mailChimpObj.find_SendGridIntegration);
            Emails emailObj = new Emails(emails, mailChimpObj.find_MailAccount);

            Task t = Task.Run(() => root.PostTestsEmailsInSendGrid(emailObj));
            t.Wait();
        }

        private void PostActionSendTestEmailsMailchimp(Campaign campaign, find_marketingautomationintegration mailChimpObj, ApiRoot root, string emails, ITracingService tracing)
        {
            Apihelper.InitializeClient(mailChimpObj.find_MailChimpIntegration);
            string[] server = mailChimpObj.find_MailChimpIntegration.Split('-');

            List<string> listEmalis = emails.Split(',').ToList();
            TestEmails testEmails = new TestEmails(listEmalis);

            Task t = Task.Run(() => root.PostTestsEmails(testEmails, server[1], campaign.find_mailChimpCampaignID));
            t.Wait();
        }
    }
}
