using CrmEarlyBound;
using EasyMarketing.Api;
using EasyMarketing.Class;
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
    public class SendCampaign : CodeActivity
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

            Campaign campaign = service.Retrieve(Campaign.Get(context).LogicalName, Campaign.Get(context).Id, new ColumnSet("find_mailchimpcampaignid", "find_mailchimp_integration_count", "find_platformmarketing","find_sendgridid")).ToEntity<Campaign>();

            if(campaign.find_mailchimp_integration_count == 0)
            {
                throw new Exception("Não existe nenhum dado sobre esta campanha");
            }

            SendCampaingInMailChimp(service, campaign);
        }

        private void SendCampaingInMailChimp(IOrganizationService service, Campaign campaign)
        {
            FuncLibrary aux = new FuncLibrary();
            find_marketingautomationintegration mailChimpObj = aux.GetMailchimpInfo(service);
            ApiRoot root = new ApiRoot();

            if (SendEmails(campaign, mailChimpObj,root))
            {
                campaign.find_mail_integration_started = true;
                service.Update(campaign);
            }
        }

        private bool SendEmails(Campaign campaign, find_marketingautomationintegration mailChimpObj, ApiRoot root)
        {
            if (campaign.find_PlatformMarketing == false)
            {
                return PostActionSendMail(campaign, mailChimpObj, root);
            }
            else
            {
                return PutScheduleNow(campaign, mailChimpObj, root);
            }
        }

        private bool PutScheduleNow(Campaign campaign,find_marketingautomationintegration mailChimpObj, ApiRoot root)
        {
            Apihelper.InitializeClient(mailChimpObj.find_SendGridIntegration);
            SingleSend singleSend = new SingleSend("now");

            Task<bool> t = Task.Run(() => root.PutSchedule(singleSend, campaign.find_SendGridId));
            t.Wait();
            if (t.IsCompleted)
            {
                return t.Result;
            }
            else throw new Exception();
        }

        private bool PostActionSendMail(Campaign campaign, find_marketingautomationintegration mailChimpObj, ApiRoot root)
        {
            Apihelper.InitializeClient(mailChimpObj.find_MailChimpIntegration);
            string[] server = mailChimpObj.find_MailChimpIntegration.Split('-');

            Task<bool> t = Task.Run(() => root.PostSendEmail(server[1], campaign.find_mailChimpCampaignID));
            t.Wait();
            if (t.IsCompleted)
            {
                return t.Result;
            }
            else throw new Exception();
        }
    }
}
