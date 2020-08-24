using CrmEarlyBound;
using EasyMarketing.Api;
using EasyMarketing.Class;
using EasyMarketing.Class.SendgridMember;
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
    public class SyncCrmToSendgrid : CodeActivity
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

            FuncLibrary aux = new FuncLibrary();
            find_marketingautomationintegration sendGridAccount = aux.GetMailchimpInfo(service);
            Apihelper.InitializeClient(sendGridAccount.find_SendGridIntegration);
            ApiRoot apiRoot = new ApiRoot();


            List list = service.Retrieve(MarketingList.Get(context).LogicalName, MarketingList.Get(context).Id, new ColumnSet("find_sendgridid")).ToEntity<List>();
            Campaign campaign = service.Retrieve(Campaign.Get(context).LogicalName, Campaign.Get(context).Id, new ColumnSet("name")).ToEntity<Campaign>();
            EntityReference memberReference = new EntityReference(MemberType.Get(context), new Guid(MemberId.Get(context)));
            find_metric metric = GetMetricByCampaign(service, campaign);


            if (HasMetricDetailsToMember(service, metric, memberReference))
            {
                return;
            }

            CreateMetricDetails(service, metric, memberReference);
            SyncMemberTypeToSendGrid(service, memberReference, list, apiRoot, tracing);

        }

        private bool HasMetricDetailsToMember(IOrganizationService service, find_metric metric, EntityReference memberReference)
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

        private void SyncMemberTypeToSendGrid(IOrganizationService service, EntityReference memberId, List list, ApiRoot root, ITracingService tracing)
        {
            string mail = "";

            switch (memberId.LogicalName)
            {
                case "account":
                    Account account = service.Retrieve(memberId.LogicalName, memberId.Id, new ColumnSet("emailaddress1")).ToEntity<Account>();
                    mail = account.EMailAddress1;
                    break;
                case "contact":
                    Contact contact = service.Retrieve(memberId.LogicalName, memberId.Id, new ColumnSet("emailaddress1")).ToEntity<Contact>();
                    mail = contact.EMailAddress1;
                    break;
                case "lead":
                    Lead lead = service.Retrieve(memberId.LogicalName, memberId.Id, new ColumnSet("emailaddress1")).ToEntity<Lead>();
                    mail = lead.EMailAddress1;
                    break;
            }
            PutMemberToSendGrid(mail, list, root, tracing);
        }

        private void PutMemberToSendGrid(string email,List list, ApiRoot root, ITracingService tracing)
        {
            List<string> listIds = new List<string>();
            listIds.Add(list.find_SendGridId);
            List<Contacts> listContacts = new List<Contacts>();
            Contacts contact = new Contacts(email);
            listContacts.Add(contact);
            MemberSendgrid member = new MemberSendgrid(listIds, listContacts);

            Task t = Task.Run(() => root.PostOrPutMember(member, tracing));
            t.Wait();
        }
    }
}
