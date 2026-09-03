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
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.SyncNode.Compatible;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Service.Services.Tenant.SyncNodes;
using AvePoint.RA.Service.Services.Tenant.SyncNodes.Cache;
using Cloud.Sdk.Data.Aos.Tenant;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Service.Services.Tenant.Notification.Excutor
{
    public class SyncNodesExecutor : AbstractSyncDataExecutor
    {
        private static RALogger logger = RALogger.GetInstance(typeof(SyncNodesExecutor));
        private Dictionary<string, SPTreeNodeDto> daoSPDataDic = new Dictionary<string, SPTreeNodeDto>();
        private Dictionary<string, ExchangeOnlineTreeNodeDto> daoEXODataDic = new Dictionary<string, ExchangeOnlineTreeNodeDto>();

        private static readonly IRMKeyValueDao s_keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private const string S_Need_Synced_Content_Source = "Need_Synced_Content_Source";

        public SyncNodesExecutor(SyncDataJobContext context) : base(context)
        {
            
        }

        private IRMRemoteNodeService RemoteNodeService
        {
            get
            {
                return PlatformWindsorManager.GetService<IRMRemoteNodeService>();
            }
        }
        private IRMMailboxService MailboxService
        {
            get
            {
                return PlatformWindsorManager.GetService<IRMMailboxService>();
            }
        }

        private ISyncService GetSyncService(Cloud.Sdk.Data.AosModern.RemoteNodeType nodeType)
        {
            switch (nodeType)
            {
                case Cloud.Sdk.Data.AosModern.RemoteNodeType.SiteCollection:
                case Cloud.Sdk.Data.AosModern.RemoteNodeType.OneDrive:
                case Cloud.Sdk.Data.AosModern.RemoteNodeType.Office365Group:
                    return new SyncRemoteNodesService(executorContext);
                case Cloud.Sdk.Data.AosModern.RemoteNodeType.Mailbox:
                //case Cloud.Sdk.Data.AosModern.RemoteNodeType.Office365Group:
                    return new SyncMailboxesService(executorContext);
                case Cloud.Sdk.Data.AosModern.RemoteNodeType.Channel:
                    return new SyncPrivateChannelService(executorContext);
                default:
                    return new NullSyncService(nodeType);
            }
        }

        public override bool SyncData(RMAosQueueMessage queueMessage)
        {
            try
            {
                logger.Info("Begin to execute sync message.");
                SyncNodesMessage syncMessage = queueMessage.SyncNodesMessage;
                if (!IsMessageValid(queueMessage))
                {
                    return true;
                }
                LogSyncNodesMessageInfo(queueMessage);
                //Cloud.Sdk.Data.AosModern.RemoteNode
                foreach(var aosNodes in InitNodes(queueMessage))
                {
                    ReportMangerFactory.Instance.ReportManager.IncreaseBase(aosNodes.Count);
                    /* Fortify Issue Type: Insecure Randomness 
                    * Sink Details:   AvePoint.RA.Service.Services.Tenant.Notification SyncDataJobProcessor ExecuteSync
                    * Ignore Reason: random用于job运行时参数，与用户无关
                    */
                    ReportMangerFactory.Instance.ReportManager.Increase(new Random().Next(2, 6));
                    if (aosNodes == null || aosNodes.Count == 0)
                    {
                        logger.Info("No notes to sync.");
                        continue;
                    }
                    TenantUtil.RunUnderTenant(queueMessage.TenantGroupId, null, () =>
                    {
                        var syncNodesSettings = new SyncNodesSettings(queueMessage);
                        try
                        {
                            logger.Info("Begin to sync remote nodes.");
                            SyncNodesInternal(syncNodesSettings, aosNodes);
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Failed to sync remote nodes. Exception is {0}.", ex.ToString());
                            throw;
                        }
                    });
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to sync node, exception is {ex.ToString()}");
                return false;
            }
        }

        private void SyncNodesInternal(SyncNodesSettings syncNodesSettings, List<RMCompatibleRemoteNode> aosNodes)
        {
            if (syncNodesSettings.IsLastSyncJob)
            {
                SyncAllAOSNodes(syncNodesSettings, aosNodes);
                //if(syncNodesSettings.MessageType != RMAosQueueMessageType.InitNodes)
                //{
                //    new CompareAllNodesService().Compare(aosNodes, syncNodesSettings.TenantGroupId, syncNodesSettings.O365TenantGroupId);
                //}
            }
            else
            {
                var currentType = aosNodes[0].NodeType;
                if (currentType == Cloud.Sdk.Data.AosModern.RemoteNodeType.Office365Group)
                {
                    SyncOffice365GroupNodes(syncNodesSettings, aosNodes);
                }
                else
                {
                    logger.Info("Current sync remote node type: {0}.", currentType.ToString());
                    ExecuteSyncService(currentType, syncNodesSettings, aosNodes);
                }
            }
        }

        private void SyncAllAOSNodes(SyncNodesSettings syncMessageSettings, List<RMCompatibleRemoteNode> aosNodes)
        {
            // RemoteNodes
            List<RMCompatibleRemoteNode> remoteNodes = GetRemoteNodes(aosNodes);
            ExecuteSyncService(Cloud.Sdk.Data.AosModern.RemoteNodeType.SiteCollection, syncMessageSettings, remoteNodes);
            ReportMangerFactory.Instance.ReportManager.Increase(remoteNodes.Count);
            // Mailbox
            List<RMCompatibleRemoteNode> mailboxes = GetMailboxes(aosNodes);
            ExecuteSyncService(Cloud.Sdk.Data.AosModern.RemoteNodeType.Mailbox, syncMessageSettings, mailboxes);
            // PrivateChannel
            List<RMCompatibleRemoteNode> privateChannels = CorrectGroupSiteNodes(GetPrivateChannels(aosNodes));
            ExecuteSyncService(Cloud.Sdk.Data.AosModern.RemoteNodeType.Channel, syncMessageSettings, privateChannels);
            // O365Group
            List<RMCompatibleRemoteNode> o365GroupNodes = GetO365GroupNodes(aosNodes);
            SyncOffice365GroupNodes(syncMessageSettings, o365GroupNodes);
        }

        private void ExecuteSyncService(Cloud.Sdk.Data.AosModern.RemoteNodeType nodeType, SyncNodesSettings syncMessageSettings, List<RMCompatibleRemoteNode> aosNodes)
        {
            logger.Info($"Sync Nodes: {nodeType}");
            GetSyncService(nodeType).Execute(syncMessageSettings, aosNodes);
            ReportMangerFactory.Instance.ReportManager.Increase(aosNodes.Count);
        }

        private List<RMCompatibleRemoteNode> GetRemoteNodes(List<RMCompatibleRemoteNode> aosNodes, bool isIncludeO365Group = false, bool isIncludePrivateChannel = false)
        {
            return aosNodes.Where(item =>
                item.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.SiteCollection ||
                item.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.OneDrive ||
                item.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.ProjectOnline).ToList();
        }

        private List<RMCompatibleRemoteNode> GetMailboxes(List<RMCompatibleRemoteNode> aosNodes)
        {
            List<RMCompatibleRemoteNode> mailboxes = new List<RMCompatibleRemoteNode>();
            foreach (var node in aosNodes)
            {
                if(node.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.Mailbox)
                {
                    node.ObjectId = GetMailboxObjectId(node);
                    mailboxes.Add(node);
                }
            }
            return mailboxes;
        }

        private List<RMCompatibleRemoteNode> GetO365GroupNodes(List<RMCompatibleRemoteNode> aosNodes)
        {
            return aosNodes.Where(n => n.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.Office365Group).ToList();
        }

        private List<RMCompatibleRemoteNode> GetPrivateChannels(List<RMCompatibleRemoteNode> aosNodes)
        {
            return aosNodes.Where(n => n.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.Channel).ToList();
        }

        private IEnumerable<List<RMCompatibleRemoteNode>> InitNodes(RMAosQueueMessage queueMessage)
        {

            var needSyncedContentSourceList = new List<SourceFlag>
            {
                SourceFlag.SharePoint,
                SourceFlag.OneDrive,
                SourceFlag.Exchange,
            };

            var setting = s_keyValueDao.GetValueByKey(S_Need_Synced_Content_Source);
            if(setting != null)
            {
                try
                {
                    if(!string.IsNullOrWhiteSpace(setting.Value)) {
                        var contentSource = JsonConvert.DeserializeObject<List<SourceFlag>>(setting.Value);
                        if(contentSource.Count > 0)
                        {
                            needSyncedContentSourceList = contentSource;
                        }
                    }
                }
                catch(Exception ex)
                {
                    logger.Error($"An error occurred while get need syned content source. Error: {ex}");
                }
            }

            if (!queueMessage.IsLastSyncJob)
            {
                var aosNodes = TryToGetNodesFromCloud(queueMessage.SyncNodesMessage.Content, needSyncedContentSourceList);
                CorrectAOSNodesInfo(aosNodes);
                logger.Info($"Get {aosNodes.Count} nodes.");
                yield return aosNodes;
            }
            else
            {
                var enumerableNodes = GetNodesFromAosByPage(queueMessage.TenantGroupId, queueMessage.SyncNodesMessage.Content.Office365TenantId, needSyncedContentSourceList);
                foreach(var nodes in enumerableNodes)
                {
                    if (queueMessage.MessageType == RMAosQueueMessageType.InitNodes
                    && this.executorContext.DependTypeForInitNode == RMDependTypeForInitNode.DAO)
                    {
                        SyncDataFromDAO(nodes);
                    }

                    CorrectAOSNodesInfo(nodes);

                    yield return nodes;
                }
            }
        }

        // 更正AOS Node Info
        private List<RMCompatibleRemoteNode> CorrectAOSNodesInfo(List<RMCompatibleRemoteNode> aosNodes)
        {
            aosNodes.ForEach(node =>
            {
                if(string.IsNullOrEmpty(node.Id))
                {
                    node.Id = Guid.NewGuid().ToString();
                }
                if (node.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.ProjectOnline)
                {
                    node.NodeType = Cloud.Sdk.Data.AosModern.RemoteNodeType.SiteCollection;
                }
                if (node.ParentName == RMConstants.DefaultProjectOnlineGroup)
                {
                    node.ParentName = RMConstants.DEFAULT_SPSITES_GROUP;
                }
            });
            return aosNodes;
        }

        private bool IsMessageValid(RMAosQueueMessage queueMessage)
        {
            bool isValid = true;
            if(queueMessage.MessageType == RMAosQueueMessageType.InitNodes)
            {
                return isValid;
            }
            if (queueMessage.SyncNodesMessage == null)
            {
                logger.Error("The content is null.");
                isValid = false;
            }
            if (string.IsNullOrEmpty(queueMessage.TenantGroupId))
            {
                logger.Error("Tenant group id is null.");
                isValid = false;
            }
            return isValid;
        }

        private void LogSyncNodesMessageInfo(RMAosQueueMessage queueMessage)
        {
            logger.Info("TenantGroupId is {0}.", queueMessage.TenantGroupId);
            var syncMessage = queueMessage.SyncNodesMessage;
            if (queueMessage.MessageType == RMAosQueueMessageType.InitNodes)
            {
                logger.Info($"Init Remote Nodes for Office365 Tenant: {syncMessage.Content.Office365TenantId}");
            }
            bool IsManualImport = syncMessage.Content.IsManualScan;
            if (IsManualImport)
            {
                logger.Info("Manual import from AOS.");
            }
            logger.Info("The license of this group is {0}", syncMessage.Content.DocAveLicenseInfo);
            logger.Info($"Last sync job: {queueMessage.IsLastSyncJob}");
        }







        private void SyncOffice365GroupNodes(SyncNodesSettings syncNodeSettings, List<RMCompatibleRemoteNode> groupNodes)
        {
            logger.Info("Sync office365 group nodes count: {0}.", groupNodes.Count);
            //ExecuteSyncService(Cloud.Sdk.Data.AosModern.RemoteNodeType.Mailbox, syncNodeSettings, CorrectO365GroupNodes(groupNodes));
            ExecuteSyncService(Cloud.Sdk.Data.AosModern.RemoteNodeType.SiteCollection, syncNodeSettings, CorrectGroupSiteNodes(groupNodes));
        }

        

        private List<RMCompatibleRemoteNode> CorrectGroupSiteNodes(List<RMCompatibleRemoteNode> groups)
        {
            groups.ForEach(group =>
            {
                if (string.Equals(group.ParentName, RMConstants.DEFAULT_O365_GROUP, StringComparison.InvariantCultureIgnoreCase))
                {
                    group.ParentName = RMConstants.DEFAULT_O365_SITES_GROUP;
                }
            });
            return groups;
        }

        private const int MaxAttempts = 3;
        private List<RMCompatibleRemoteNode> TryToGetNodesFromCloud(RemoteNodesMessage message, List<SourceFlag> needSyncedContentSources)
        {
            logger.Info("Begin to download message from cloud");
            int attempt = 1;
            var messageText = string.Empty;
            while (attempt <= MaxAttempts && string.IsNullOrEmpty(messageText))
            {
                try
                {
                    if (message.IsNewMessage)
                    {
                        messageText = RAStorageUtil.DownloadFileMessageFromStorageBySasTokenUrl(message.StorageSasUri, message.FileLowName);
                    }
                    else
                    {
                        messageText = RAStorageUtil.DownloadFileMessageFromStorageByXri(message.StorageXri, message.FileLowName);
                    }
                    logger.Info("{0} times to download message.", attempt);
                }
                catch(Exception ex)
                {
                    logger.Error($"An error occurred while get message from storage. Error: {ex}");
                    if(attempt == MaxAttempts) 
                    {
                        throw;
                    }
                }
                attempt++;
            }
            return ConvertToSyncNodes(messageText, needSyncedContentSources);
        }

        private List<RMCompatibleRemoteNode> ConvertToSyncNodes(string syncNodesStr, List<SourceFlag> needSyncedContentSources)
        {
            if (string.IsNullOrEmpty(syncNodesStr))
            {
                return new List<RMCompatibleRemoteNode>();
            }
            try
            {
                var nodes = SerializerHelper.DeserializeByJsonConvert<List<Cloud.Sdk.Data.Aos.Tenant.RemoteNode>>(syncNodesStr);
                var res = RMCompatibleRemoteNodeConverter.Convert(nodes);
                res = res.Where(item =>
                {
                    if(needSyncedContentSources.Contains(SourceFlag.SharePoint) && 
                    (item.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.Office365Group || 
                    item.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.Channel ||
                    item.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.SiteCollection ||
                    item.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.ProjectOnline))
                    {
                        return true;
                    }

                    if(needSyncedContentSources.Contains(SourceFlag.Exchange) && 
                    item.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.Mailbox)
                    {
                        return true;
                    }

                    if(needSyncedContentSources.Contains(SourceFlag.OneDrive) && 
                    item.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.OneDrive)
                    {
                        return true;
                    }

                    return false;
                }).ToList();

                return res;
                    
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to deserialize the content: {syncNodesStr}. Exception is {ex}");
                throw;
            }
        }

        private List<RemoteWebApplication> GetDefaultSPSitesGroups(Dictionary<string, string> groupIDs)
        {
            RemoteWebApplication remote = new RemoteWebApplication();
            remote.description = String.Empty;
            remote.modifiedDate = DateTime.UtcNow.Ticks;
            remote.url = RMConstants.DEFAULT_SPSITES_GROUP;
            remote.NodeType = RemoveNodeType.SiteCollection;
            remote.id = groupIDs[RMConstants.DEFAULT_SPSITES_GROUP];
            if (string.IsNullOrEmpty(remote.id))
            {
                remote.id = Guid.NewGuid().ToString();
            }
            else
            {
                remote.FromDAO = true;
            }

            RemoteWebApplication mySiteGroup = new RemoteWebApplication();
            mySiteGroup.id = groupIDs[RMConstants.DEFAULT_SKYDRIVEPROS_GROUP];
            mySiteGroup.url = RMConstants.DEFAULT_SKYDRIVEPROS_GROUP;
            mySiteGroup.description = String.Empty;
            mySiteGroup.modifiedDate = DateTime.UtcNow.Ticks;
            mySiteGroup.NodeType = RemoveNodeType.SkyDrivePro;
            if (string.IsNullOrEmpty(mySiteGroup.id))
            {
                mySiteGroup.id = Guid.NewGuid().ToString();
            }
            else
            {
                mySiteGroup.FromDAO = true;
            }

            RemoteWebApplication o365SitesGroup = new RemoteWebApplication();
            o365SitesGroup.id = groupIDs[RMConstants.DEFAULT_O365_SITES_GROUP];
            o365SitesGroup.url = RMConstants.DEFAULT_O365_SITES_GROUP;
            o365SitesGroup.description = String.Empty;
            o365SitesGroup.modifiedDate = DateTime.UtcNow.Ticks;
            o365SitesGroup.NodeType = RemoveNodeType.O365GroupSites;
            if (string.IsNullOrEmpty(o365SitesGroup.id))
            {
                o365SitesGroup.id = Guid.NewGuid().ToString();
            }
            else
            {
                o365SitesGroup.FromDAO = true;
            }

            return new List<RemoteWebApplication>()
            {
                remote, mySiteGroup, o365SitesGroup
            };
        }

        private List<EmailAccountGroupDto> GetDefaultMailboxGroups(Dictionary<string, string> groupIDs)
        {
            EmailAccountGroupDto emailGroup = new EmailAccountGroupDto();
            emailGroup.id = groupIDs[RMConstants.DEFAULT_MAILBOX_GROUP];
            if (string.IsNullOrEmpty(emailGroup.id))
            {
                emailGroup.id = Guid.NewGuid().ToString();
            }
            else
            {
                emailGroup.FromDAO = true;
            }
            emailGroup.Name = RMConstants.DEFAULT_MAILBOX_GROUP;
            emailGroup.NodeLevel = NodeLevel.ExchangeOnlineMailboxGroup;

            EmailAccountGroupDto o365group = new EmailAccountGroupDto();
            o365group.id = groupIDs[RMConstants.DEFAULT_O365_GROUPS_GROUP];
            if (string.IsNullOrEmpty(o365group.id))
            {
                o365group.id = Guid.NewGuid().ToString();
            }
            else
            {
                o365group.FromDAO = true;
            }
            o365group.Name = RMConstants.DEFAULT_O365_GROUPS_GROUP;
            o365group.NodeLevel = NodeLevel.ExchangeOnlineO365GroupGroup;

            return new List<EmailAccountGroupDto>() 
            {
                emailGroup, o365group
            };
        }

        private void InitRemoteNodeGroups(IEnumerable<SPTreeNodeDto> groupsFromDAO = null)
        {
            logger.Info($"Begin to init remote node groups.");
            var defaultGroupIDs = new Dictionary<string, string>() 
            {
                { RMConstants.DEFAULT_SPSITES_GROUP, null },
                { RMConstants.DEFAULT_SKYDRIVEPROS_GROUP, null },
                { RMConstants.DEFAULT_O365_SITES_GROUP, null },
            };

            var nodeGroups = new List<RemoteWebApplication>();
            if (groupsFromDAO != null && groupsFromDAO.Count() > 0)
            {
                foreach (var group in groupsFromDAO)
                {
                    if (defaultGroupIDs.ContainsKey(group.Name))
                    {
                        defaultGroupIDs[group.Name] = group.ID;
                        continue;
                    }

                    RemoveNodeType nodeType;
                    switch (group.Type)
                    {
                        case NodeType.PrivateChannelSitesGroup:
                            nodeType = RemoveNodeType.PrivateChannel;
                            break;
                        case NodeType.O365GroupSitesGroup:
                            nodeType = RemoveNodeType.O365GroupSites;
                            break;
                        case NodeType.SkyDriveProSitesGroup:
                            nodeType = RemoveNodeType.SkyDrivePro;
                            break;
                        default:
                            nodeType = RemoveNodeType.SiteCollection;
                            break;
                    }

                    nodeGroups.Add(new RemoteWebApplication()
                    {
                        id = group.ID,
                        url = group.Name,
                        NodeType = nodeType,
                        description = String.Empty,
                        modifiedDate = DateTime.UtcNow.Ticks,
                        FromDAO = true
                    });
                    
                }
            }

            nodeGroups.AddRange(GetDefaultSPSitesGroups(defaultGroupIDs));
            var existingGroups = RemoteNodeService.GetAllWebApplications();
            nodeGroups = nodeGroups.Where(n => !existingGroups.Exists(g => g.NodeType == n.NodeType && g.url.Equals(n.url, StringComparison.OrdinalIgnoreCase))).ToList();

            if(nodeGroups.Count > 0)
            {
                RemoteNodeService.CreateRemoteWebApplications(nodeGroups);
                SyncDataJobProcessor.AddJobDetails4ContainerAdded(RMRemoteNodeSourceType.SharePointOnline, nodeGroups.Select(n => n.url));
            }

            logger.Info($"Finish to init remote node groups.");
        }

        private void InitMailboxGroups(IEnumerable<ExchangeOnlineTreeNodeDto> groupsFromDAO = null)
        {
            logger.Info($"Begin to init mailbox groups.");
            var defaultGroupIDs = new Dictionary<string, string>()
            {
                { RMConstants.DEFAULT_MAILBOX_GROUP, null },
                { RMConstants.DEFAULT_O365_GROUPS_GROUP, null },
            };

            var mailboxGroups = new List<EmailAccountGroupDto>();
            if (groupsFromDAO != null && groupsFromDAO.Count() > 0)
            {
                foreach (var group in groupsFromDAO)
                {
                    if (defaultGroupIDs.ContainsKey(group.Name))
                    {
                        defaultGroupIDs[group.Name] = group.ID;
                        continue;
                    }
                    mailboxGroups.Add(new EmailAccountGroupDto()
                    {
                        id = group.ID,
                        Name = group.Name,
                        NodeLevel = group.Type == NodeType.EOO365GroupGroup ? NodeLevel.ExchangeOnlineO365GroupGroup : NodeLevel.ExchangeOnlineMailboxGroup,
                        FromDAO = true
                    });
                }
            }

            mailboxGroups.AddRange(GetDefaultMailboxGroups(defaultGroupIDs));
            var existingGroups = MailboxService.GetRemoteMailGroupNodes();
            mailboxGroups = mailboxGroups.Where(n => !existingGroups.Exists(g => g.NodeLevel == n.NodeLevel && g.NodeName.Equals(n.Name, StringComparison.OrdinalIgnoreCase))).ToList();
            if (mailboxGroups.Count > 0)
            {
                MailboxService.CreateMailboxGroups(mailboxGroups);
                SyncDataJobProcessor.AddJobDetails4ContainerAdded(
                    RMRemoteNodeSourceType.ExchangeOnline, 
                    mailboxGroups.Where(n => n.NodeLevel != NodeLevel.ExchangeOnlineO365GroupGroup).Select(n => n.Name));
            }
            logger.Info($"Finish to init mailbox groups.");
        }

        private IEnumerable<List<RMCompatibleRemoteNode>> GetNodesFromAosByPage(string tenantGroupId, string o365TenantGroupId, List<SourceFlag> needSyncedContentSources)
        {
            logger.Info("Begin to get nodes from AOS. Tenant group id is {0}.", tenantGroupId);
            var resultList = RMAosApiClient.GetTenantRemoteNodesByPage(tenantGroupId, o365TenantGroupId, needSyncedContentSources);

            foreach(var (result, container) in resultList)
            {
                var nodes = RMCompatibleRemoteNodeConverter.Convert(result);
                nodes.ForEach(item =>
                {
                    item.ParentId = container.Id;
                    item.ParentName = container.Name;
                });

                yield return nodes;
            }
        }

        public void InitDataForFirstJob()
        {
            if(executorContext.DependTypeForInitNode == RMDependTypeForInitNode.DAO)
            {
                
            }
            else
            {
                InitRemoteNodeGroups();
                InitMailboxGroups();
            }
        }

        private List<RMCompatibleRemoteNode> SyncDataFromDAO(List<RMCompatibleRemoteNode> aosNodes)
        {
            List<RMCompatibleRemoteNode> daoNodes = new List<RMCompatibleRemoteNode>();
            SPTreeNodeDto tempSPNode = null;
            ExchangeOnlineTreeNodeDto tempEXONode = null;
            string tempKey = null;
            foreach (var node in aosNodes)
            {
                tempKey = node.Url?.ToLowerInvariant();
                if (!string.IsNullOrEmpty(tempKey) && daoSPDataDic.TryGetValue(tempKey, out tempSPNode))
                {
                    daoNodes.Add(node);
                    node.Id = tempSPNode.ID;
                    if (node.NodeType != Cloud.Sdk.Data.AosModern.RemoteNodeType.Channel)
                    {
                        node.ParentId = tempSPNode.ParentId;  //Channel 是固定的ParentId
                    }
                }

                if(node.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.Office365Group)
                {
                    tempKey = node.Name?.ToLowerInvariant();
                }
                if (!string.IsNullOrEmpty(tempKey) && daoEXODataDic.TryGetValue(tempKey, out tempEXONode))
                {
                    daoNodes.Add(node);
                    if (node.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.Office365Group)
                    {
                        executorContext.O365GroupMapToNodeInDAO[tempKey] = tempEXONode;
                    }
                    else
                    {
                        node.Id = tempEXONode.ID;
                        node.ParentId = tempEXONode.ParentId;
                    }
                }
            }

            return daoNodes;
        }

        //private SPTreeMessage BrowseSPTree(ref DAOAPIClientV1 client, SPTreeNodeDto curNode)
        //{
        //    try
        //    {
        //        return client.Browse(new SPTreeMessage() { TreeType = TreeType.SOArchiverTree, Node = curNode });
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Warn($"BrowserSPTree failed, Retry with a new Client: {ex}");
        //        client = new DAOAPIClientV1();
        //    }
        //    return client.Browse(new SPTreeMessage() { TreeType = TreeType.SOArchiverTree, Node = curNode });
        //}

        //private ExchangeOnlineTreeMessage BrowseEXOTree(ref DAOAPIClientV1 client, ExchangeOnlineTreeNodeDto curNode)
        //{
        //    try
        //    {
        //        return client.BrowseExchange(new ExchangeOnlineTreeMessage()
        //        {
        //            TreeType = TreeType.ExchangeOnlineArchiverTree,
        //            Node = curNode
        //        }, true);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Warn($"BrowserEXOTree failed, Retry with a new Client: {ex}");
        //        client = new DAOAPIClientV1();
        //    }
        //    return client.BrowseExchange(new ExchangeOnlineTreeMessage()
        //    {
        //        TreeType = TreeType.ExchangeOnlineArchiverTree,
        //        Node = curNode
        //    }, true);
        //}

        private string GetMailboxObjectId(RMCompatibleRemoteNode node)
        {
            var mailboxGuid = node.ObjectId;
            if(mailboxGuid == null)
            {
                logger.Warn($"Mailbox node object id is null. {node.Url} - {node.Name}");
            }
            else if (mailboxGuid.IndexOf("(Archive)") >= 0)
            {
                mailboxGuid = mailboxGuid.Substring(0, mailboxGuid.IndexOf("(Archive)"));
            }
            return mailboxGuid;
        }
    }

}

