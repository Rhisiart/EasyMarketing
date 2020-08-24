using CrmEarlyBound;
using EasyMarketing.Api;
using EasyMarketing.Class;
using EasyMarketing.Class.Campaigns;
using EasyMarketing.Class.Interests;
using EasyMarketing.Class.Members;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Services.Description;

namespace EasyMarketing.Workflow
{
    public class SyncCrmToMailchimp : CodeActivity
    {
        [RequiredArgument]
        [Input("Campaign")]
        [ReferenceTarget("campaign")]
        public InArgument<EntityReference> Campaign { get; set; }

        [RequiredArgument]
        [Input("MemberId")]
        public InArgument<string> MemberId { get; set; }

        [RequiredArgument]
        [Input("MemberType")]
        public InArgument<string> MemberType { get; set; }

        [RequiredArgument]
        [Input("MarketingList")]
        [ReferenceTarget("list")]
        public InArgument<EntityReference> MarketingList { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            ITracingService tracing = context.GetExtension<ITracingService>();
            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
            IOrganizationService service = context.GetExtension<IOrganizationServiceFactory>().CreateOrganizationService(workflowContext.InitiatingUserId);

            List list = service.Retrieve(MarketingList.Get(context).LogicalName, MarketingList.Get(context).Id,new ColumnSet("find_mailchimplistid")).ToEntity<List>();
            Campaign campaign = service.Retrieve(Campaign.Get(context).LogicalName, Campaign.Get(context).Id, new ColumnSet("name", "find_mailchimpcampaignid")).ToEntity<Campaign>();
            EntityReference memberReference = new EntityReference(MemberType.Get(context), new Guid(MemberId.Get(context)));
            find_metric metric = GetMetricByCampaign(service, campaign);

            FuncLibrary aux = new FuncLibrary();
            find_marketingautomationintegration mailChimpAccount = aux.GetMailchimpInfo(service);
            Apihelper.InitializeClient(mailChimpAccount.find_MailChimpIntegration);
            ApiRoot apiRoot = new ApiRoot();
            string[] server = mailChimpAccount.find_MailChimpIntegration.Split('-'); 
            
            if (HasMetricDetailsToMember(service,metric, memberReference))
            {
                return;
            } 
            
            CreateMetricDetails(service, metric, memberReference);
            SyncMemberTypeToMailChimp(service, memberReference, list, apiRoot, server[1], mailChimpAccount,aux);
        }

        private bool HasMetricDetailsToMember(IOrganizationService service,find_metric metric, EntityReference memberReference)
        {
            QueryExpression QEfind_metricdetails = new QueryExpression("find_metricdetails");
            QEfind_metricdetails.ColumnSet.AddColumns("find_name");
            QEfind_metricdetails.Criteria.AddCondition("find_metric", "find_metricid", ConditionOperator.Equal, metric.Id);
            QEfind_metricdetails.AddLink("find_metric", "find_lookupmetric", "find_metricid");
            
            switch (memberReference.LogicalName)
            {
                case "account":
                    QEfind_metricdetails.Criteria.AddCondition("account", "accountid", ConditionOperator.Equal, memberReference.Id);
                    QEfind_metricdetails.AddLink("account", "find_lookupaccount", "accountid");
                    break;
                case "contact":
                    QEfind_metricdetails.Criteria.AddCondition("contact", "contactid", ConditionOperator.Equal, memberReference.Id);
                    QEfind_metricdetails.AddLink("contact", "find_lookupcontact", "contactid");
                    break;
                case "lead":
                    QEfind_metricdetails.Criteria.AddCondition("lead", "leadid", ConditionOperator.Equal, memberReference.Id);
                    QEfind_metricdetails.AddLink("contact", "find_lookuplead", "leadid");
                    break;
            }
            return service.RetrieveMultiple(QEfind_metricdetails).Entities.Count != 0;
        }

        private find_metric GetMetricByCampaign(IOrganizationService service, Campaign campaign)
        {
            QueryExpression QEfind_metric = new QueryExpression("find_metric");
            QEfind_metric.ColumnSet.AddColumns("find_name");
            QEfind_metric.Criteria.AddCondition("campaign", "campaignid", ConditionOperator.Equal, campaign.Id);
            QEfind_metric.AddLink("campaign", "find_lookupcampaign", "campaignid");

            return service.RetrieveMultiple(QEfind_metric).Entities[0].ToEntity<find_metric>();
        }

        private void CreateMetricDetails(IOrganizationService service, find_metric metric, EntityReference memberReference)
        {
            find_metricdetails metricDetails = new find_metricdetails(Guid.NewGuid());
            metricDetails.find_LookUpMetric = metric.ToEntityReference();
            metricDetails.find_NumberClick = 0;
            metricDetails.find_NumberOpens = 0;
            metricDetails.find_name = "metricDetail " + memberReference.Id;

            switch (memberReference.LogicalName)
            {
                case "account":
                    metricDetails.find_LookUpAccount = memberReference;
                    break;
                case "contact":
                    metricDetails.find_LookUpContact = memberReference;
                    break;
                case "lead":
                    metricDetails.find_LookUpLead = memberReference;
                    break;
            }

            service.Create(metricDetails);
        }

        private void SyncMemberTypeToMailChimp(IOrganizationService service, EntityReference memberId, List list, ApiRoot root,string server, find_marketingautomationintegration mailChimpAccount, FuncLibrary aux)
        {
            string mail = "" , sub = "";
 
            switch (memberId.LogicalName)
            {
                case "account":
                    Account account = service.Retrieve(memberId.LogicalName, memberId.Id, new ColumnSet("emailaddress1", "find_sub")).ToEntity<Account>();
                    mail = account.EMailAddress1;
                    sub = account.find_Sub.ToString();
                    break;
                case "contact":
                    Contact contact = service.Retrieve(memberId.LogicalName, memberId.Id, new ColumnSet("emailaddress1", "find_sub")).ToEntity<Contact>();
                    mail = contact.EMailAddress1;
                    sub = contact.find_Sub.ToString();
                    break;
                case "lead":
                    Lead lead = service.Retrieve(memberId.LogicalName, memberId.Id, new ColumnSet("emailaddress1", "find_sub")).ToEntity<Lead>();
                    mail = lead.EMailAddress1;
                    sub = lead.find_Sub.ToString();
                    break;
            }
            if (CheckIfMemberIsMailChimp(root, server, mailChimpAccount.find_MailchimpListId,aux.GetMailMd(mail)))
            {
                PutMemberToMailChimp(root, server, mailChimpAccount.find_MailchimpListId, aux.GetMailMd(mail),list,mail);
            }
            else
            {
                PostMemberToMailChimp(root, server, mailChimpAccount.find_MailchimpListId, mail,sub,list);
            }
        }

        private bool CheckIfMemberIsMailChimp(ApiRoot root, string server, string listId, string mailMd5)
        {
            Task<bool> t = Task.Run(() => root.GetMember(server, listId, mailMd5));
            t.Wait();
            if (t.IsCompleted)
            {
                return t.Result;
            }
            else throw new Exception();
        }

        private void PostMemberToMailChimp(ApiRoot root, string server, string listId, string mail, string sub, List list)
        {
            Group group = new Group(true);
            Member member = new Member(mail, sub, group);

            Task task = Task.Run(() => root.PostMember(member, server, listId, list.find_MailChimpListId));
            task.Wait();
        }

        private void PutMemberToMailChimp(ApiRoot root, string server, string listId, string mailMd5, List list, string mail)
        {
            Group group = new Group(true);
            Member member = new Member(mail, group);

            Task task = Task.Run(() => root.PutMember(member, server, listId, list.find_MailChimpListId, mailMd5));
            task.Wait();
        }
    }
}
