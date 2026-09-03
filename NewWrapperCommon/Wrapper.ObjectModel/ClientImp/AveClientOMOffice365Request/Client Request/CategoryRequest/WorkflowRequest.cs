/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using AvePoint.ObjectModel.ClientOM;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Workflow;
using Microsoft.SharePoint.Client.WorkflowServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.ClientOM
{
    public partial class AveClientOMOffice365Request : AveClientOM2019Request
    {
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> CreateListAssociation(string webServerRelativeUrl, Guid hostListId, string workflowTemplateSource, IAveWorkflowAssociation asso)
        {
            return base.CreateListAssociation(webServerRelativeUrl, hostListId, workflowTemplateSource, asso);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> CreateWebAssociation(string webServerRelativeUrl, Guid webId, string workflowTemplateSource, IAveWorkflowAssociation asso)
        {
            return base.CreateWebAssociation(webServerRelativeUrl, webId, workflowTemplateSource, asso);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> CreateListContentTypeAssociation(string webServerRelativeUrl, Guid hostListId, IAveContentTypeId ctId, string workflowTemplateSource, IAveWorkflowAssociation asso)
        {
            return base.CreateListContentTypeAssociation(webServerRelativeUrl, hostListId, ctId, workflowTemplateSource, asso);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> CreatWebContentTypeAssociation(string webServerRelativeUrl, IAveContentTypeId ctId, string workflowTemplateSource, IAveWorkflowAssociation asso)
        {
            return base.CreatWebContentTypeAssociation(webServerRelativeUrl, ctId, workflowTemplateSource, asso);
        }

        [NoAPI]
        public override string AssociateWorkflowMarkup(string webServerRelativeUrl, string configUrl, string configVersion)
        {
            return mWebServiceRequest.AssociateWorkflowMarkup(webServerRelativeUrl, configUrl, configVersion);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetWorkflowTemplates(string webServerRelativeUrl, string webName, Guid webId, string workflowSource, Dictionary<string, object> contentTypeProp)
        {
            return base.GetWorkflowTemplates(webServerRelativeUrl, webName, webId, workflowSource, contentTypeProp);
        }

        [NoAPI]
        public override Dictionary<string, object> GetWorkflowAssociations(string webServerRelativeUrl, string listName, Guid listId, string workflowSource, Dictionary<string, object> contentTypeProp)
        {
            return base.GetWorkflowAssociations(webServerRelativeUrl, listName, listId, workflowSource, contentTypeProp);
        }

        [KeepOriginalWithAPI]
        public override void DeleteWorkflowAssociation(IAveWorkflowAssociation workflow, string source)
        {
            base.DeleteWorkflowAssociation(workflow, source);
        }

        [KeepOriginalWithAPI]
        public override void DeleteAllWorkflowAasociations(string webUrl, Guid listId, string contentTypeId, string source)
        {
            base.DeleteAllWorkflowAasociations(webUrl, listId, contentTypeId, source);
        }

        [NoAPI]
        public override void UpdateWorkflowAssociationsOnChildren(string webUrl, string contentTypeId)
        {
            base.UpdateWorkflowAssociationsOnChildren(webUrl, contentTypeId);
        }
        [KeepOriginalWithAPI]
        public override void UpdateWorkflowAssociation(string webServerRelativeUrl, string listName, Guid listId, string ctId, Guid workflowAssociationId, string workflowSource, Dictionary<string, object> needUpdateWorkflowProperties)
        {
            base.UpdateWorkflowAssociation(webServerRelativeUrl, listName, listId, ctId, workflowAssociationId, workflowSource, needUpdateWorkflowProperties);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> EnumWorkflowDefinition(string webServerRelativeUrl, bool publishedOnly)
        {
            return base.EnumWorkflowDefinition(webServerRelativeUrl, publishedOnly);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetSubscription(string webServerRelativeUrl, Guid subscriptionId)
        {
            return base.GetSubscription(webServerRelativeUrl, subscriptionId);
        }

        [KeepOriginalWithAPI]
        public override Guid PublishSubscription(string webServerRelativeUrl, IAveWorkflowSubscription subscription, Guid listId)
        {
            return base.PublishSubscription(webServerRelativeUrl, subscription, listId);
        }

        [KeepOriginalWithAPI]
        public override void PublishDefinition(string webServerRelativeUrl, Guid definitionId)
        {
            base.PublishDefinition(webServerRelativeUrl, definitionId);
        }

        [KeepOriginalWithAPI]
        public override Guid SaveDefinition(string webServerRelativeUrl, IAveWorkflowDefinition definition)
        {
            return base.SaveDefinition(webServerRelativeUrl, definition);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetWorkflowDefinitionById(string webServerRelativeUrl, Guid definitionId)
        {
            return base.GetWorkflowDefinitionById(webServerRelativeUrl, definitionId);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> EnumerateSubscriptionsByEventSource(string webServerRelativeUrl, Guid webId)
        {
            return base.EnumerateSubscriptionsByEventSource(webServerRelativeUrl, webId);

        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> EnumerateSubscriptionsByList(string webServerRelativeUrl, Guid listId)
        {
            return base.EnumerateSubscriptionsByList(webServerRelativeUrl, listId);
        }


        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetWorkflowServicesManager(string webServerRelativeUrl)
        {
            return base.GetWorkflowServicesManager(webServerRelativeUrl);
        }

        [ReplaceByAPI]
        public override WorkflowStartOptionCache BackupWorkflowStartOption(string url, Guid webId, Guid listId)
        {
            var cache = new WorkflowStartOptionCache();
            using (var context = CreateContext(url))
            {
                var list = context.Web.Lists.GetById(listId);
                context.Load(list);
                context.Load(list.WorkflowAssociations);
                context.Load(list.ContentTypes, cts => cts.IncludeWithDefaultProperties(ct => ct.StringId, ct => ct.WorkflowAssociations));
                context.ExecuteQuery();
                Backup10ModeStartOption(WorkflowStartOptionCache.ListWorkflow, cache, list.WorkflowAssociations);
                foreach (var ct in list.ContentTypes)
                {
                    Backup10ModeStartOption(ct.StringId, cache, ct.WorkflowAssociations);
                }

                var workflowServiceManager = new WorkflowServicesManager(context, context.Web);
                context.Load(workflowServiceManager);
                context.ExecuteQuery();
                if (workflowServiceManager.IsConnected)
                {
                    var subScriptionService = workflowServiceManager.GetWorkflowSubscriptionService();
                    var subscriptions = subScriptionService.EnumerateSubscriptionsByList(list.Id);
                    context.Load(subscriptions, sub => sub.IncludeWithDefaultProperties(subscription => subscription.EventTypes, subscription => subscription.Id, subscription => subscription.ParentContentTypeId));
                    context.ExecuteQuery();
                    Backup13ModeStartOption(context, cache, subScriptionService, subscriptions);
                    context.ExecuteQuery();
                }
            }
            return cache;
        }


        [ReplaceByAPI]
        public override void RestoreWorkflowStartOption(string url, Guid webId, Guid listId, WorkflowStartOptionCache cache)
        {
            using (var context = CreateContext(url))
            {
                var web = context.Site.OpenWebById(webId);
                var list = web.Lists.GetById(listId);
                context.Load(list.WorkflowAssociations);
                context.Load(list.ContentTypes, cts => cts.IncludeWithDefaultProperties(ct => ct.StringId, ct => ct.WorkflowAssociations));
                context.ExecuteQuery();
                if (cache.SP2010ModeWorkflowAutoStartCache.Count > 0)
                {
                    foreach (var item in cache.SP2010ModeWorkflowAutoStartCache)
                    {
                        var workflows = list.WorkflowAssociations;
                        if (!string.Equals(item.Key, WorkflowStartOptionCache.ListWorkflow, StringComparison.OrdinalIgnoreCase))
                        {
                            var contentType = list.ContentTypes.GetById(item.Key);
                            workflows = contentType.WorkflowAssociations;
                        }
                        foreach (var cacheItem in item.Value)
                        {
                            var workflow = workflows.GetById(cacheItem.DefinitionId);
                            context.Load(workflow);
                            context.ExecuteQuery();
                            workflow.AutoStartChange = cacheItem.ItemUpdated;
                            workflow.AutoStartCreate = cacheItem.ItemAdded;
                            workflow.Update();
                            mLogger.Debug("ChangeBack auto start option for 2010 mode workflow:{0}:{1},AutoStart:{2},AutoChange:{3}",
                              workflow.Name, workflow.Id, cacheItem.ItemAdded, cacheItem.ItemUpdated);
                        }
                    }
                    context.ExecuteQuery();
                }
                if (cache.SP2013ModeWorkflowAutoStartCache.Count > 0)
                {
                    WorkflowServicesManager manager = new WorkflowServicesManager(context, web);
                    var subscriptionService = manager.GetWorkflowSubscriptionService();

                    foreach (var item in cache.SP2013ModeWorkflowAutoStartCache)
                    {
                        foreach (var cacheItem in item.Value)
                        {
                            var oldSubscription = subscriptionService.GetSubscription(cacheItem.DefinitionId);
                            context.Load(oldSubscription);
                            context.ExecuteQuery();
                            var workflow = CloneSubscription(context, oldSubscription);
                            var eventList = workflow.EventTypes.ToList();
                            bool needChange = false;
                            if (!workflow.EventTypes.Contains("ItemAdded", StringComparer.OrdinalIgnoreCase) && cacheItem.ItemAdded)
                            {
                                eventList.Add("ItemAdded");
                                needChange = true;
                            }
                            if (!workflow.EventTypes.Contains("ItemUpdated", StringComparer.OrdinalIgnoreCase) && cacheItem.ItemUpdated)
                            {
                                eventList.Add("ItemUpdated");
                                needChange = true;
                            }
                            if (needChange)
                            {
                                workflow.EventTypes = eventList.ToArray();
                            }
                            var subscriptionId = subscriptionService.PublishSubscriptionForList(workflow, listId);
                            mLogger.Debug("ChangeBack auto start option for 2013 mode workflow:{0}:{1},AutoStart:{2},AutoChange:{3},FinalId:{4}",
                             workflow.Name, workflow.Id, cacheItem.ItemAdded, cacheItem.ItemUpdated, subscriptionId.Value);
                        }
                    }
                    context.ExecuteQuery();
                }
            }
        }

        [ReplaceByAPI]
        private void Backup10ModeStartOption(string cacheKeyName, WorkflowStartOptionCache cache, WorkflowAssociationCollection collection)
        {
            if (collection.Count == 0)
            {
                return;
            }
            cache.SP2010ModeWorkflowAutoStartCache.Add(cacheKeyName, new List<WorkflowStartOption>());
            var listCache = cache.SP2010ModeWorkflowAutoStartCache[cacheKeyName];
            foreach (var workflow in collection)
            {
                if (workflow.Enabled && (workflow.AutoStartChange || workflow.AutoStartCreate))
                {
                    var option = new WorkflowStartOption()
                    {
                        DefinitionId = workflow.Id,
                        ItemAdded = workflow.AutoStartCreate,
                        ItemUpdated = workflow.AutoStartChange
                    };
                    listCache.Add(option);
                    mLogger.Debug("Change auto start option for 2010 mode workflow:{0}:{1},AutoStart:{2} to {3},AutoChange:{4} to {5}",
                               workflow.Name, workflow.Id, option.ItemAdded, false, option.ItemUpdated, false);
                    workflow.AutoStartChange = false;
                    workflow.AutoStartCreate = false;
                    workflow.Update();
                }
            }
        }

    }
}
