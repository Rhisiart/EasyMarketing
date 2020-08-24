using CrmEarlyBound;
using EasyMarketing.Api;
using EasyMarketing.Class;
using EasyMarketing.Class.Campaigns;
using EasyMarketing.Class.Interests;
using EasyMarketing.Class.SendGridSingleSend;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.Plugin
{
    public class Campaign : IPlugin
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
                    CreateAsync(context, service);
                }
            }
            else if (context.MessageName == "Update")
            {
                if (context.Mode == 1)
                {
                    UpdateAsync(context, service, tracingService);
                }
            }
        }

        private void CreateAsync(IPluginExecutionContext context, IOrganizationService service)
        {
            CrmEarlyBound.Campaign newCampaign = ((Entity)context.InputParameters["Target"]).ToEntity<CrmEarlyBound.Campaign>();

            CreateMetric(service, newCampaign);
            newCampaign.find_mailchimp_integration_count = 0;
            newCampaign.find_crm_integration_count = 0;
            newCampaign.find_createcampaign = 0;

            service.Update(newCampaign);
        }

        private void UpdateAsync(IPluginExecutionContext context, IOrganizationService service, ITracingService tracingService)
        {
            CrmEarlyBound.Campaign target = ((Entity)context.InputParameters["Target"]).ToEntity<CrmEarlyBound.Campaign>();
            CrmEarlyBound.Campaign campaign = service.Retrieve(target.LogicalName, target.Id, new ColumnSet("name", "find_platformmarketing", "find_fromname","find_subject", "find_mailchimpcampaignid", "find_sendgridid")).ToEntity<CrmEarlyBound.Campaign>();


            if (target.Attributes.ContainsKey("find_crm_integration_count")) 
            {   
                
                if(campaign.find_PlatformMarketing == false)
                {
                    tracingService.Trace("a");
                    OrganizationRequest request = new OrganizationRequest("find_SyncMailchimpToCrm");
                    request.Parameters["Campaign"] = target.ToEntityReference(); 
                    service.Execute(request);
                }
                else
                {
                    OrganizationRequest request = new OrganizationRequest("find_SyncSendgridToCrm");
                    request.Parameters["Campaign"] = target.ToEntityReference(); 
                    service.Execute(request);
                }
               
            }
            else if (target.Attributes.ContainsKey("find_mailchimp_integration_count")) 
            {
                List<List> listsToUpdate = GetAllMarketingListsAssociatedToCampaign(service, target);

                if(listsToUpdate.Count == 0) 
                {
                    target.find_mailchimp_integration_count = 0;
                    return;
                }

                FuncLibrary aux = new FuncLibrary();
                find_marketingautomationintegration mailChimpAccount = aux.GetMailchimpInfo(service);
                ApiRoot root = new ApiRoot();

                if(campaign.find_PlatformMarketing == false)
                {
                    ProcessMailChimpListsAndCampaign(service,campaign, mailChimpAccount,root, listsToUpdate);
                }
                else
                {
                    ProcessSendGridLists(service, campaign, mailChimpAccount, root, listsToUpdate);
                }

                foreach (List list in listsToUpdate)
                {
                    list.find_mailchimp_integration_count++;
                    service.Update(list);
                }
            }
            else if (target.Attributes.ContainsKey("find_mail_integration_started"))
            {
                target.find_crm_integration_count = 1;

                service.Update(target);
            }
        }

        private void ProcessMailChimpListsAndCampaign(IOrganizationService service, CrmEarlyBound.Campaign campaign, find_marketingautomationintegration mailChimpAccount, ApiRoot root, List<List> listsToUpdate)
        {
            Apihelper.InitializeClient(mailChimpAccount.find_MailChimpIntegration);
            string[] server = mailChimpAccount.find_MailChimpIntegration.Split('-');

            if (campaign.find_mailChimpCampaignID == null)
            {
                PostCampaignToMailchimp(service, server[1], mailChimpAccount, root, campaign);
            }

            foreach (List list in listsToUpdate)
            {
                if (list.find_MailChimpListId == null)
                {
                    CreateAsyncInterest(service, server[1], root, mailChimpAccount, list);
                }
            }

            AssociateMarkeitngListToMailchimpCampaign(root, listsToUpdate, server[1], campaign);

        }

        private void ProcessSendGridLists(IOrganizationService service, CrmEarlyBound.Campaign campaign, find_marketingautomationintegration mailChimpAccount, ApiRoot root, List<List> listsToUpdate)
        {
            Apihelper.InitializeClient(mailChimpAccount.find_SendGridIntegration);

            if(campaign.find_SendGridId == null)
            {
                CreateSingleSend(service,campaign,root);
            }

            foreach (List list in listsToUpdate)
            {
                if (list.find_SendGridId == null)
                {
                    CreateListSendGrid(service, list, root);
                }
            }

            AssociateMarketingListToSendGrid(root, listsToUpdate,campaign);
        }
 
        private void CreateAsyncInterest(IOrganizationService service, string server, ApiRoot root, find_marketingautomationintegration mailChimpAccount, List list)
        {
            Group newInterests = new Group(list.ListName);
            Task<MailChimpCampaign> t = Task.Run(() => root.PostInterest(newInterests, server, mailChimpAccount.find_MailchimpListId));
            t.Wait();
            if (t.IsCompleted)
            {
                list.find_MailChimpListId = t.Result.Id;
                service.Update(list);
            }
        }

        private void CreateSingleSend(IOrganizationService service, CrmEarlyBound.Campaign campaign, ApiRoot root)
        {
            EmailConfig emailConfig = new EmailConfig(campaign.find_Subject);
            SingleSend singleSend = new SingleSend(campaign.Name, emailConfig);
            
            Task<MailChimpCampaign> t = Task.Run(() => root.PostSingleSend(singleSend));
            t.Wait();
            if (t.IsCompleted)
            {
                campaign.find_SendGridId = t.Result.Id;
                service.Update(campaign);
            }
        }

        private void CreateListSendGrid(IOrganizationService service, List newList, ApiRoot root)
        {
            Group listSend = new Group(newList.ListName);
            Task<MailChimpCampaign> t = Task.Run(() => root.PostList(listSend));
            t.Wait();
            if (t.IsCompleted)
            {
                newList.find_SendGridId = t.Result.Id;
                service.Update(newList);
            }
        }

        private void PostCampaignToMailchimp(IOrganizationService service, string server, find_marketingautomationintegration mailChimpAccount, ApiRoot root, CrmEarlyBound.Campaign newCampaign)
        {
            Settings settings = new Settings(newCampaign.find_FromName, newCampaign.Name, newCampaign.find_Subject, mailChimpAccount.find_MailAccount);
            Recipients recipients = new Recipients(mailChimpAccount.find_MailchimpListId);
            Campaigns campaign = new Campaigns(settings, recipients);

            Task<MailChimpCampaign> t = Task.Run(() => root.PostCampaign(campaign, server));
            t.Wait();
            if (t.IsCompleted)
            {
                newCampaign.find_mailChimpCampaignID = t.Result.Id;
                service.Update(newCampaign);
            }
        }

        private void AssociateMarkeitngListToMailchimpCampaign(ApiRoot root, List<List> listLists, string server, CrmEarlyBound.Campaign campaign)
        {
            List<string> listMailchimpId = new List<string>();
            List<Conditions> listConditions = new List<Conditions>();

            foreach (List lists in listLists)
            {
                listMailchimpId.Add(lists.find_MailChimpListId);
            }

            Conditions conditions = new Conditions("53afadfa03", listMailchimpId);
            listConditions.Add(conditions);
            SegementOps segement = new SegementOps(listConditions);
            Recipients recipients = new Recipients(segement);
            Campaigns campaigns = new Campaigns(recipients);

            Task t = Task.Run(() => root.PathCampaign(campaigns, server, campaign.find_mailChimpCampaignID));
            t.Wait();
        }

        private void AssociateMarketingListToSendGrid(ApiRoot root, List<List> listLists, CrmEarlyBound.Campaign campaign)
        {
            List<string> listSendGridId = new List<string>();
            foreach (List lists in listLists)
            {
                listSendGridId.Add(lists.find_SendGridId);
            }

            SendTo sendTo = new SendTo(listSendGridId);
            SingleSend singleSend = new SingleSend(sendTo);

            Task t = Task.Run(() => root.PathSingleSend(singleSend, campaign.find_SendGridId));
            t.Wait();
        }

        private List<List> GetAllMarketingListsAssociatedToCampaign(IOrganizationService service, CrmEarlyBound.Campaign campaign)
        {
            QueryExpression QElist = new QueryExpression("list"); 
            QElist.ColumnSet.AddColumns("listname" ,"find_mailchimp_integration_count", "find_mailchimplistid","find_sendgridid");
            QElist.Criteria.AddCondition("campaignitem", "campaignid", ConditionOperator.Equal, campaign.Id);
            QElist.AddLink("campaignitem", "listid", "entityid");

            return service.RetrieveMultiple(QElist).Entities.Select(entity => entity.ToEntity<List>()).ToList();
        }

        private void CreateMetric(IOrganizationService service, CrmEarlyBound.Campaign campaign)
        {
            find_metric metric = new find_metric(Guid.NewGuid());
            metric.find_lookupCampaign = campaign.ToEntityReference();
            metric.find_name = campaign.Name + " metric";
            service.Create(metric);
        }

    }
}
