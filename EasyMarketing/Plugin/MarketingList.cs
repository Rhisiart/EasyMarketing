using CrmEarlyBound;
using EasyMarketing.Api;
using EasyMarketing.Class;
using EasyMarketing.Class.Campaigns;
using EasyMarketing.Class.Interests;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.Plugin
{
    public class MarketingList : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context =
                            (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.MessageName == "Create")
            {
                if (context.Mode == 1)
                {
                    CreateAsync(context, service, tracingService);
                }
            }
            else if (context.MessageName == "Update")
            {
                if (context.Mode == 1)
                {
                    UpdateAsync(context, service,tracingService);
                }
            }
        }

        private void CreateAsync(IPluginExecutionContext context, IOrganizationService service, ITracingService tracingService)
        {
            if(context.PrimaryEntityName.Equals("campaignitem"))
            {
                AddListMembersAsync(context, service, tracingService);
            }
        }

        private void AddListMembersAsync(IPluginExecutionContext context, IOrganizationService service, ITracingService tracingService)
        {
            CampaignItem campaignItem = (context.InputParameters["Target"] as Entity).ToEntity<CampaignItem>();
            CrmEarlyBound.Campaign campaign = service.Retrieve(campaignItem.CampaignId.LogicalName, campaignItem.CampaignId.Id, new ColumnSet("name", "find_mailchimp_integration_count", "find_createcampaign")).ToEntity<CrmEarlyBound.Campaign>();

            if (campaignItem.EntityType.Equals("list") && campaign.find_createcampaign == 0)
            {
                campaign.find_mailchimp_integration_count++;
                campaign.find_createcampaign++;
                service.Update(campaign);
            }
        }

        private void UpdateAsync(IPluginExecutionContext context, IOrganizationService service, ITracingService tracingService)
        {
            List target = (context.InputParameters["Target"] as Entity).ToEntity<List>();

            if (target.Attributes.ContainsKey("find_mailchimp_integration_count"))
            {
                List<CrmEarlyBound.Campaign> campaignsList = GetAllStartedCampaignsThatMarketingListIsAssociated(service, target);
                List list = (List)service.Retrieve(target.LogicalName, target.Id, new ColumnSet("listname","type", "query", "membertype", "find_mailchimplistid", "find_sendgridid"));

                foreach (CrmEarlyBound.Campaign campaign in campaignsList)
                {
                    ProcessMembersByList(service,campaign,list);
                }
            }
        }

        private void ProcessMembersByList(IOrganizationService service, CrmEarlyBound.Campaign campaign, List list)
        {
            switch (list.MemberType)
            {
                case 1:
                    List<Account> listAccounts = new List<Account>();

                    if (list.Type == true)
                    {
                        listAccounts = GetDynamicMarketingListMembers<Account>(service, new FetchExpression(list.Query));
                    }
                    else
                    {
                        listAccounts = GetStaticMarketingListMembers<Account>(service, list, "account");
                    }

                    if (listAccounts.Count != 0)
                    {
                        CallAction<Account>(service, campaign, listAccounts, list);
                    }
                    else
                    {
                        throw new Exception();
                    }
                    break;
                case 2:
                    List<Contact> listContact = new List<Contact>();
                    if (list.Type == true)
                    {
                        listContact = GetDynamicMarketingListMembers<Contact>(service, new FetchExpression(list.Query));
                    }
                    else
                    {
                        listContact = GetStaticMarketingListMembers<Contact>(service, list, "contact");
                    }

                    if (listContact.Count != 0)
                    {
                        CallAction<Contact>(service, campaign, listContact, list);
                    }
                    else
                    {
                        throw new Exception();
                    }
                    break;
                case 4:
                    List<Lead> listLead = new List<Lead>();

                    if (list.Type == true)
                    {
                        listLead = GetDynamicMarketingListMembers<Lead>(service, new FetchExpression(list.Query));
                    }
                    else
                    {
                        listLead = GetStaticMarketingListMembers<Lead>(service, list, "lead");
                    }

                    if(listLead.Count != 0)
                    {
                        CallAction<Lead>(service,campaign, listLead,list);
                    }
                    else
                    {
                        throw new Exception();
                    }
                    break;
            }
        }

        private void CallAction<T>(IOrganizationService service, CrmEarlyBound.Campaign campaign,List<T> listType, List list) where T : Entity
        {
            if (campaign.find_PlatformMarketing == false)
            {
                foreach (T type in listType)
                {
                    ExecuteMailChimpIntegration(service, campaign.ToEntityReference(), type.ToEntityReference(), list.ToEntityReference());
                }
            }
            else
            {
                foreach (T type in listType)
                {
                    ExecuteSendGridIntegration(service, campaign.ToEntityReference(), type.ToEntityReference(), list.ToEntityReference());
                }
            }
        }

        private void ExecuteMailChimpIntegration(IOrganizationService service, EntityReference campaign, EntityReference memberListId, EntityReference list)
        {
            OrganizationRequest request = new OrganizationRequest("find_SyncCrmToMailchimp");
            request.Parameters["Campaign"] = campaign; 
            request.Parameters["MemberId"] = memberListId.Id.ToString(); 
            request.Parameters["MemberType"] = memberListId.LogicalName; 
            request.Parameters["MarketingList"] = list;

            service.Execute(request);
        }

        private void ExecuteSendGridIntegration(IOrganizationService service, EntityReference campaign, EntityReference memberListId, EntityReference list)
        {
            OrganizationRequest request = new OrganizationRequest("find_find_SyncCrmToSendgrid");
            request.Parameters["Campaign"] = campaign; 
            request.Parameters["MemberId"] = memberListId.Id.ToString(); 
            request.Parameters["MemberType"] = memberListId.LogicalName; 
            request.Parameters["MarketingList"] = list;

            service.Execute(request);
        }

        public List<T> GetDynamicMarketingListMembers<T>(IOrganizationService service,FetchExpression query) where T : Entity
        {
            return service.RetrieveMultiple(query).Entities.Select(entity => entity.ToEntity<T>()).ToList();
        }

        public List<T> GetStaticMarketingListMembers<T>(IOrganizationService service,List listMarketing, string type) where T : Entity
        {
            Guid QEcontact_listmember_listid = (Guid)listMarketing.ListId;
            QueryExpression QEcontact = new QueryExpression(type);
            QEcontact.ColumnSet.AddColumns("emailaddress1", "find_sub");
            QEcontact.Criteria.AddCondition("listmember", "listid", ConditionOperator.Equal, QEcontact_listmember_listid);
            QEcontact.AddLink("listmember", type + "id", "entityid");

            return service.RetrieveMultiple(QEcontact).Entities.Select(entity => entity.ToEntity<T>()).ToList();
        }

        private List<CrmEarlyBound.Campaign> GetAllStartedCampaignsThatMarketingListIsAssociated(IOrganizationService service, List listId)
        {
            QueryExpression QEcampaign = new QueryExpression("campaign");
            QEcampaign.ColumnSet.AddColumns("find_mailchimpcampaignid","name", "find_platformmarketing");
            QEcampaign.Criteria.AddCondition("find_mail_integration_started", ConditionOperator.Equal, false);
            LinkEntity QEcampaign_campaignitem = QEcampaign.AddLink("campaignitem", "campaignid", "campaignid");
            LinkEntity QEcampaign_campaignitem_list = QEcampaign_campaignitem.AddLink("list", "entityid", "listid");
            QEcampaign_campaignitem_list.LinkCriteria.AddCondition("listid", ConditionOperator.Equal, listId.Id);

            return service.RetrieveMultiple(QEcampaign).Entities.Select(entity => entity.ToEntity<CrmEarlyBound.Campaign>()).ToList();
        }
    }
}
