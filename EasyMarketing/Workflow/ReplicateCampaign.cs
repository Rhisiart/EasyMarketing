using CrmEarlyBound;
using EasyMarketing.Api;
using EasyMarketing.Class;
using EasyMarketing.Class.Campaigns;
using Microsoft.Crm.Sdk.Messages;
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
    public class ReplicateCampaign : CodeActivity
    {
        [RequiredArgument]
        [Input("Campaign")]
        [ReferenceTarget("campaign")]
        public InArgument<EntityReference> Campaign { get; set; }

        [RequiredArgument]
        [Output("CampaignId")]
        public OutArgument<string> CampaingId { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            ITracingService tracing = context.GetExtension<ITracingService>();
            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
            IOrganizationService service = context.GetExtension<IOrganizationServiceFactory>().CreateOrganizationService(workflowContext.InitiatingUserId);

            FuncLibrary aux = new FuncLibrary();
            find_marketingautomationintegration mailChimpAccount = aux.GetMailchimpInfo(service);
            ApiRoot root = new ApiRoot();

            Campaign campaign = service.Retrieve(Campaign.Get(context).LogicalName, Campaign.Get(context).Id, new ColumnSet("find_platformmarketing", "find_fromname", "find_subject", "name","find_mailchimpcampaignid","find_sendgridid")).ToEntity<Campaign>();
            Guid newGuidCampaign = CreateCopyFromCampaign(service, campaign, root, mailChimpAccount);
            CampaingId.Set(context, newGuidCampaign.ToString());
            tracing.Trace(CampaingId.ToString());
        }

        private Guid CreateCopyFromCampaign(IOrganizationService service, Campaign campaign, ApiRoot root, find_marketingautomationintegration mailChimpAccount)
        {
            List<List> listAssociate = GetAllMarketingListsAssociatedToCampaign(service, campaign);
            Guid newCampaignId = CreateNewCampaingCrm(service, campaign);
            Campaign newCampaign = service.Retrieve(campaign.LogicalName, newCampaignId, new ColumnSet("find_sendgridid", "find_mailchimpcampaignid")).ToEntity<Campaign>();
 
            if (listAssociate.Count != 0)
            {
                CreateAssociationToNewCampaign(service, listAssociate, newCampaign);
            }
            

            if (campaign.find_PlatformMarketing == false)
            {
                PostReplicateCampaignInMailchimp(service,root, mailChimpAccount, campaign, newCampaign);
            }
            else
            {
                PostReplicateCampaignInSendGrid(service, root, mailChimpAccount, campaign, newCampaign);
            }

            return newCampaignId;
        }

        private Guid CreateNewCampaingCrm(IOrganizationService service, Campaign campaign)
        {
            Campaign newCampaign = new Campaign();
            newCampaign.find_Subject = campaign.find_Subject;
            newCampaign.find_FromName = campaign.find_FromName;
            newCampaign.Name = "Copy " + campaign.Name;
            newCampaign.find_mail_integration_started = false;
            newCampaign.find_mailchimp_integration_count = 2;
            newCampaign.find_crm_integration_count = 0;
            newCampaign.find_PlatformMarketing = campaign.find_PlatformMarketing;

            return service.Create(newCampaign);
        }

        private void CreateAssociationToNewCampaign(IOrganizationService service, List<List> listAssociate, Campaign campaign)
        {
            foreach(List list in listAssociate)
            {
                CampaignItem campaignItem = new CampaignItem();
                campaignItem.EntityType = list.LogicalName;
                campaignItem.EntityId = list.Id;
                campaignItem.CampaignId = campaign.ToEntityReference();
                service.Create(campaignItem);
            }
        }

        private void PostReplicateCampaignInMailchimp(IOrganizationService service, ApiRoot root, find_marketingautomationintegration mailChimpAccount, Campaign campaign, Campaign newCampaign)
        {
            Apihelper.InitializeClient(mailChimpAccount.find_MailChimpIntegration);
            string[] server = mailChimpAccount.find_MailChimpIntegration.Split('-');

            Task<MailChimpCampaign> t = Task.Run(() => root.PostCopyCampaign(server[1], campaign.find_mailChimpCampaignID));
            t.Wait();
            if (t.IsCompleted)
            {
                newCampaign.find_mailChimpCampaignID = t.Result.Id;
                service.Update(newCampaign);
            }
        }

        private void PostReplicateCampaignInSendGrid(IOrganizationService service, ApiRoot root, find_marketingautomationintegration mailChimpAccount, Campaign campaign, Campaign newCampaign)
        {
            Apihelper.InitializeClient(mailChimpAccount.find_SendGridIntegration);

            Task<MailChimpCampaign> t = Task.Run(() => root.PostCopySingleSend(campaign.find_SendGridId));
            t.Wait();
            if (t.IsCompleted)
            {
                newCampaign.find_SendGridId =  t.Result.Id;
                service.Update(newCampaign);
            }
        }

        private List<List> GetAllMarketingListsAssociatedToCampaign(IOrganizationService service, Campaign campaign)
        { 
            QueryExpression QElist = new QueryExpression("list");
            QElist.ColumnSet.AddColumns("listname", "find_mailchimp_integration_count", "find_mailchimplistid", "find_sendgridid");
            QElist.Criteria.AddCondition("campaignitem", "campaignid", ConditionOperator.Equal, campaign.Id);
            var QElist_campaignitem = QElist.AddLink("campaignitem", "listid", "entityid");

            return service.RetrieveMultiple(QElist).Entities.Select(entity => entity.ToEntity<List>()).ToList();
        }
    }
}
