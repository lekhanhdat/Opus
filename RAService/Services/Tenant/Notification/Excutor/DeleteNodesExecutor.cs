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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Services.Tenant.SyncNodes.Cache;
using Cloud.Sdk.Data.Aos.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Service.Services.Tenant.Notification.Excutor
{
    public class DeleteNodesExecutor : AbstractSyncDataExecutor
    {
        private static RALogger logger = RALogger.GetInstance(typeof(DeleteNodesExecutor));

        #region Services
        private IRMRemoteNodeService RemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();

        private IRMMailboxService EmailService => PlatformWindsorManager.GetService<IRMMailboxService>();
        private IRMDeleteRemoteSiteAspect DeleteRemoteSiteAspect => PlatformWindsorManager.GetService<IRMDeleteRemoteSiteAspect>();

        private ISyncRemoteNodeRedisService RemoteNodeRedisService => PlatformWindsorManager.GetService<ISyncRemoteNodeRedisService>();

        private ISyncMailboxRedisService MailboxRedisService => PlatformWindsorManager.GetService<ISyncMailboxRedisService>();

        private ISyncChannelRedisService ChannelRedisService => PlatformWindsorManager.GetService<ISyncChannelRedisService>();

        #endregion

        private List<RemoteNodeType> MailboxLevels = new List<RemoteNodeType> { RemoteNodeType.Mailbox, RemoteNodeType.Office365GroupMailboxes, RemoteNodeType.Office365Group };
        private List<RemoteNodeType> RemoteNodeLevels = new List<RemoteNodeType> { RemoteNodeType.SiteCollection };
        private List<RemoteNodeType> O365GroupSiteLevels = new List<RemoteNodeType> { RemoteNodeType.Office365GroupSites, RemoteNodeType.Office365Group };
        private List<RemoteNodeType> OneDriveNodeLevels = new List<RemoteNodeType> { RemoteNodeType.OneDrive };

        public DeleteNodesExecutor(SyncDataJobContext context) : base(context)
        {
        }

        public override bool SyncData(RMAosQueueMessage queueMessage)
        {
            logger.Info("Execute delete remote nodes task.");
            try
            {
                logger.Info("TenantGroupId is {0}.", queueMessage.TenantGroupId);
                return TenantUtil.RunUnderTenant(queueMessage.TenantGroupId, null, () =>
                {
                    try
                    {
                        logger.Info("Run under group {0}.", queueMessage.TenantGroupId);
                        var nodes = GetNodesFromByMessage(queueMessage);
                        if (nodes == null || nodes.Count == 0)
                        {
                            logger.Warn("Current message don't have any nodes");
                            return true;
                        }
                        ReportMangerFactory.Instance.ReportManager.IncreaseBase(nodes.Count);
                        /* Fortify Issue Type: Insecure Randomness 
                        * Sink Details: AvePoint.RA.Common.Report SubJobReportManager 474
                        *               AvePoint.RA.Common.Report RMReportManager  488
                        * Ignore Reason: random用于决定阻塞线程几秒，与安全性无关 
                        */
                        ReportMangerFactory.Instance.ReportManager.Increase(new Random().Next(2, 6));
                        UpdateNodeTypeForProjectOnline(nodes);
                        var containers = nodes.Where(n => n.NodeLevel == (int)RemoteNodeLevel.Group).ToList();
                        var objects = nodes.Where(n => n.NodeLevel == (int)RemoteNodeLevel.Sites).ToList();
                        if (containers.Count > 0)
                        {
                            // 删除整个 container 或者 container 下的所有 object
                            logger.Info("container count: {0} names: {1}.", containers.Count, DatabaseUtility.BuildInClause<string>(containers.Select(s => s.NodeName)));
                            DeleteContainers(containers);
                        }
                        if (objects.Count > 0)
                        {
                            // 删除某个 objects
                            logger.Info("object count: {0}", objects.Count);
                            DeleteObjects(objects);
                        }
                        logger.Debug("Finish delete remote nodes to database.");
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Delete remote nodes to database failed: {0}.", ex.ToString());
                        return false;
                    }
                    logger.Debug("Execute delete remote nodes complete");
                    return true;
                });
            }
            catch (Exception ex)
            {
                logger.Error("Execute delete remote nodes task error.", ex.ToString());
                return false;
            }
            finally
            {
                logger.Debug("Complete delete remote message.");
            }
        }

        #region 删除整个 container 或者 container 下的所有 object
        private void DeleteContainers(List<RemoteNodeMessage> nodes)
        {
            var mailboxContainers = nodes.Where(n => MailboxLevels.Contains((RemoteNodeType)n.NodeType)).ToList();
            var remoteNodeContainers = nodes.Where(n => RemoteNodeLevels.Contains((RemoteNodeType)n.NodeType) || O365GroupSiteLevels.Contains((RemoteNodeType)n.NodeType) || OneDriveNodeLevels.Contains((RemoteNodeType)n.NodeType)).ToList();
            if (mailboxContainers.Count > 0)
            {
                DeleteMailboxContainers(mailboxContainers);
                ReportMangerFactory.Instance.ReportManager.Increase(mailboxContainers.Count);
            }
            if (remoteNodeContainers.Count > 0)
            {
                DeleteRemoteNodeContainers(remoteNodeContainers);
                ReportMangerFactory.Instance.ReportManager.Increase(remoteNodeContainers.Count);
            }
        }
        private void DeleteMailboxContainers(List<RemoteNodeMessage> mailboxContainers)
        {
            var exchangeMailboxContainers = mailboxContainers.Where(m => m.NodeType == (int)RemoteNodeType.Mailbox).ToList();
            var o365GroupMailboxContainers = mailboxContainers.Where(m => m.NodeType == (int)RemoteNodeType.Office365GroupMailboxes || m.NodeType == (int)RemoteNodeType.Office365Group).ToList();
            // 从 DB 中取所有的 Mailbox Containers， 包含 Exchange Mailbox Containers 和 O365 Group Mailbox Containers
            var allMailboxContainers = EmailService.GetRemoteMailGroupNodes();
            var currentExchangeMailboxContainers = allMailboxContainers.Where(a => a.NodeLevel == NodeLevel.ExchangeOnlineMailboxGroup).ToList();
            var currentO365GroupMailboxContainers = allMailboxContainers.Where(a => a.NodeLevel == NodeLevel.ExchangeOnlineO365GroupGroup).ToList();
            logger.Info("current exchange mailbox containers count: {0}, o365 group mailbox containers count: {1}.", currentExchangeMailboxContainers.Count, currentO365GroupMailboxContainers.Count);
            var deleteMailboxContainers = new Dictionary<string, RemoteNodeMessage>();   //需要删除的 container id
            var deleteMailboxContainerRedisFieldKeys = new List<string>();   //需要从 Redis 里删除的 container key
            if (exchangeMailboxContainers.Count > 0)
            {
                deleteMailboxContainers.AddRangeInternal(GetDeleteContainerIds(exchangeMailboxContainers, currentExchangeMailboxContainers, ref deleteMailboxContainerRedisFieldKeys), true);
            }
            if (o365GroupMailboxContainers.Count > 0)
            {
                deleteMailboxContainers.AddRangeInternal(GetDeleteContainerIds(o365GroupMailboxContainers, currentO365GroupMailboxContainers, ref deleteMailboxContainerRedisFieldKeys), true);
            }
            logger.Info("delete mailbox container count: {0}", deleteMailboxContainers.Count);
            if (deleteMailboxContainers != null && deleteMailboxContainers.Count > 0)
            {
                var deleteMailboxContainerIDs = deleteMailboxContainers.Keys.ToList();
                var mailboxes = EmailService.GetMailboxNamesByParentIds(deleteMailboxContainerIDs);
                MailboxRedisService.DeleteMailboxes(mailboxes.Keys.ToList(), () =>
                {
                    logger.Info("start delete mailbox by parent id mailboxName: {0}.", DatabaseUtility.BuildInClause<string>(mailboxes.Keys));
                    this.EmailService.DeleteMailboxByParentIds(deleteMailboxContainerIDs);
                    foreach (var item in mailboxes)
                    {
                        var delMC = deleteMailboxContainers[item.Value];
                        if(delMC.NodeType != (int)RemoteNodeType.Office365GroupMailboxes && delMC.NodeType != (int)RemoteNodeType.Office365Group)
                        {
                            SyncDataJobProcessor.AddJobDetails4Removed(RMRemoteNodeSourceType.ExchangeOnline, delMC.NodeName, item.Key);
                        }
                    }
                });
                MailboxRedisService.DeleteMailboxes(deleteMailboxContainerRedisFieldKeys, () =>
                {
                    logger.Info("start delete mailbox containers.");
                    this.EmailService.DeleteMailboxGroup(deleteMailboxContainerIDs);
                    foreach (var item in deleteMailboxContainers)
                    {
                        if(item.Value.NodeType != (int)RemoteNodeType.Office365Group)
                        {
                            SyncDataJobProcessor.AddJobDetails4Removed(RMRemoteNodeSourceType.ExchangeOnline, item.Value.NodeName);
                        }
                    }
                }, false);
            }
        }
        private void DeleteRemoteNodeContainers(List<RemoteNodeMessage> remoteNodeContainers)
        {
            var scContainers = remoteNodeContainers.Where(r => r.NodeType == (int)RemoteNodeType.SiteCollection).ToList();
            var onedriveContainers = remoteNodeContainers.Where(r => r.NodeType == (int)RemoteNodeType.OneDrive).ToList();
            var o365GroupSiteContainers = remoteNodeContainers.Where(r => r.NodeType == (int)RemoteNodeType.Office365GroupSites || r.NodeType == (int)RemoteNodeType.Office365Group).ToList();
            // 从 DB 中取所有的 RemoteNode Containers， 包含 Sitecollection Containers， Onedrive Containers 和 O365 Group Site Containers
            var allRemoteNodeContainers = RemoteNodeService.GetRemoteWebApplicationNodes();
            var currentSCContainsers = allRemoteNodeContainers.Where(a => a.NodeType == RemoveNodeType.SiteCollection).ToList();
            var currentOnedroveContainers = allRemoteNodeContainers.Where(a => a.NodeType == RemoveNodeType.SkyDrivePro).ToList();
            var currentO365GroupSiteContainers = allRemoteNodeContainers.Where(a => a.NodeType == RemoveNodeType.O365GroupSites).ToList();
            logger.Info("current sitecollection container count: {0}, current onedrive container count: {1}, current o365 group site container count: {2}", currentSCContainsers.Count, currentOnedroveContainers.Count, currentO365GroupSiteContainers.Count);
            var deleteRemoteNodeContainers = new Dictionary<string, RemoteNodeMessage>();   //需要删除的 container id
            var deleteOneDriveContainers = new Dictionary<string, RemoteNodeMessage>();
            var deleteRemoteNodeContainerRedisFieldKeys = new List<string>();   //需要从 Redis 里删除的 container key
            var deleteOneDriveContainerRedisFieldKeys = new List<string>();

            if (scContainers.Count > 0)
            {
                deleteRemoteNodeContainers.AddRangeInternal(
                    GetDeleteContainerIds(scContainers, currentSCContainsers, ref deleteRemoteNodeContainerRedisFieldKeys, true),
                    true);
            }
            if (onedriveContainers.Count > 0)
            {
                deleteOneDriveContainers.AddRangeInternal(
                    GetDeleteContainerIds(onedriveContainers, currentOnedroveContainers, ref deleteOneDriveContainerRedisFieldKeys, true),
                    true);
            }
            if (o365GroupSiteContainers.Count > 0)
            {
                var delContainerDict = GetDeleteContainerIds(o365GroupSiteContainers, currentO365GroupSiteContainers, ref deleteRemoteNodeContainerRedisFieldKeys, true);
                var o365GroupSiteContainersIds = delContainerDict.Keys.ToList();
                if (o365GroupSiteContainersIds.Count > 0)
                {
                    deleteRemoteNodeContainers.AddRangeInternal(delContainerDict, true);
                    var privateChannelSiteCollections = RemoteNodeService.GetPrivateChannelByGroupTeamSiteContainerIds(o365GroupSiteContainersIds);
                    logger.Info("start delete private channels by urls:{0}.", DatabaseUtility.BuildInClause<string>(privateChannelSiteCollections));
                    DeletePrivateChannelObjects(privateChannelSiteCollections);
                }
            }
            logger.Info("delete remote node container count: {0} before filter", deleteRemoteNodeContainers.Count);

            #region Abondon
            //if (deleteRemoteNodeContainers != null && deleteRemoteNodeContainers.Count > 0)
            //{
            //    var scIds = new List<string>();
            //    var scUrls = new List<string>();
            //    var deleteRemoteNodeContainerIds = deleteRemoteNodeContainers.Keys.ToList();
            //    var childNodes = RemoteNodeService.GetSiteCollectionByParentIds(deleteRemoteNodeContainerIds);
            //    List<NodeCollection> tempNodes = null;
            //    foreach (var delContainer in deleteRemoteNodeContainers)
            //    {
            //        if (childNodes.TryGetValue(delContainer.Key, out tempNodes))
            //        {
            //            tempNodes.ForEach(sc =>
            //            {
            //                scIds.Add(sc.NodeId);
            //                scUrls.Add(sc.Scope);
            //                SyncDataJobProcessor.AddJobDetails4Removed(
            //                    RMRemoteNodeSourceType.SharePointOnline, delContainer.Value.NodeName, sc.Scope);
            //            });
            //        }
            //        SyncDataJobProcessor.AddJobDetails4Removed(
            //            RMRemoteNodeSourceType.SharePointOnline, delContainer.Value.NodeName);
            //    }

            //    logger.Info("start delete remote node by parent id urls:{0}.", DatabaseUtility.BuildInClause<string>(scUrls));
            //    RemoteNodeRedisService.DeleteRemoteNodes(scUrls, () =>
            //    {
            //        this.RemoteNodeService.DeleteRemoteSiteCollectionByParentId(deleteRemoteNodeContainerIds);
            //    });
            //    logger.Info("start delete related data for archiver by sitecollection ids.");
            //    DeleteRemoteSiteAspect.DeleteRelatedDataById(scIds);
            //    RemoteNodeRedisService.DeleteRemoteNodes(deleteRemoteNodeContainerRedisFieldKeys, () =>
            //    {
            //        logger.Info("start delete sitecollection group.");
            //        //deleteRemoteNodeContainerIds = FilterGroupWithArchivedSiteCollection(deleteRemoteNodeContainerIds);
            //        logger.Info("delete remote node container count: {0}", deleteRemoteNodeContainerIds.Count);
            //        this.RemoteNodeService.DeleteRemoteWebApplication(deleteRemoteNodeContainerIds);
            //    }, false);
            //}
            #endregion

            DeleteContainers(deleteRemoteNodeContainers, deleteRemoteNodeContainerRedisFieldKeys, RMRemoteNodeSourceType.SharePointOnline);
            DeleteContainers(deleteOneDriveContainers, deleteOneDriveContainerRedisFieldKeys, RMRemoteNodeSourceType.OneDrive);
        }
        
        public void DeleteContainers(Dictionary<string, RemoteNodeMessage> deleteContainers, List<string> deleteRemoteNodeContainerRedisFieldKeys, RMRemoteNodeSourceType sourceType)
        {
            if(deleteContainers == null || deleteContainers.Count == 0)
            {
                return;
            }
            var scIds = new List<string>();
            var scUrls = new List<string>();
            var deleteContainerIds = deleteContainers.Keys.ToList();
            var childNodes = RemoteNodeService.GetSiteCollectionByParentIds(deleteContainerIds);
            List<NodeCollection> tempNodes = null;
            foreach (var delContainer in deleteContainers)
            {
                if (childNodes.TryGetValue(delContainer.Key, out tempNodes))
                {
                    tempNodes.ForEach(sc => {
                        scIds.Add(sc.NodeId);
                        scUrls.Add(sc.Scope);
                        SyncDataJobProcessor.AddJobDetails4Removed(
                            sourceType, delContainer.Value.NodeName, sc.Scope);
                    });
                }
                SyncDataJobProcessor.AddJobDetails4Removed(
                    sourceType, delContainer.Value.NodeName);
            }
            logger.Info("start delete remote node by parent id urls:{0}.", DatabaseUtility.BuildInClause<string>(scUrls));
            RemoteNodeRedisService.DeleteRemoteNodes(scUrls, () =>
            {
                this.RemoteNodeService.DeleteRemoteSiteCollectionByParentId(deleteContainerIds);
            });
            logger.Info("start delete related data for archiver by sitecollection ids.");
            DeleteRemoteSiteAspect.DeleteRelatedDataById(scIds);
            RemoteNodeRedisService.DeleteRemoteNodes(deleteRemoteNodeContainerRedisFieldKeys, () =>
            {
                logger.Info("start delete sitecollection group.");
                logger.Info("delete remote node container count: {0}", deleteContainerIds.Count);
                this.RemoteNodeService.DeleteRemoteWebApplication(deleteContainerIds);
            }, false);
        }

        /// <summary>
        /// 获取需要删除节点的 parent id
        /// </summary>
        /// <param name="aosContainers">从 AOS 传过来的 Containers</param>
        /// <param name="currentContainers">当前在 DAO 数据库中的 Containers</param>
        /// <param name="deleteContainerRedisFieldKeys">需要从 Redis 里删除的 Containers 的 redis field key</param>
        /// <param name="deleteMailboxContainerIds">需要从 DAO 数据库中删除的 container id</param>
        /// <param name="isForRemoteNode">是否为 RemoteNode 表， 如果是 true， 则处理 RemoteNode 表里的数据， 否则处理 Mailbox 表里的数据</param>
        /// <returns></returns>
        private Dictionary<string, RemoteNodeMessage> GetDeleteContainerIds(List<RemoteNodeMessage> aosContainers, List<RemoteNodePara> currentContainers, ref List<string> deleteContainerRedisFieldKeys, bool isForRemoteNode = false)
        {
            var deleteContainerIds = new Dictionary<string, RemoteNodeMessage>();
            foreach (var item in aosContainers)
            {
                if (item.NodeName.Equals(RMConstants.DEFAULT_O365_GROUP, StringComparison.OrdinalIgnoreCase))
                {
                    item.NodeName = isForRemoteNode ? RMConstants.DEFAULT_O365_SITES_GROUP : RMConstants.DEFAULT_O365_GROUPS_GROUP;
                }
                var currentContainer = currentContainers.Find(c => c.NodeName == item.NodeName);
                if (currentContainer != null)
                {
                    deleteContainerIds[currentContainer.NodeId] = item;
                    var nodeLevel = isForRemoteNode ? ConvertRemoveNodeTypeToNodeLevel(currentContainer.NodeType) : currentContainer.NodeLevel;
                    //deleteContainerRedisFieldKeys.Add(RedisFieldKeyUtil.GenerateContainerFieldKey(nodeLevel, currentContainer.NodeName));
                    deleteContainerRedisFieldKeys.Add(RedisFieldKeyUtil.GenerateContainerFieldKey(nodeLevel, currentContainer.AosId));
                }
            }
            return deleteContainerIds;
        }
        private NodeLevel ConvertRemoveNodeTypeToNodeLevel(RemoveNodeType removeNodeType)
        {
            var nodeLevel = NodeLevel.Undefined;
            switch (removeNodeType)
            {
                case RemoveNodeType.O365GroupSites:
                    nodeLevel = NodeLevel.O365GroupSitesGroup;
                    break;
                case RemoveNodeType.SiteCollection:
                    nodeLevel = NodeLevel.WebApplication;
                    break;
                case RemoveNodeType.SkyDrivePro:
                    nodeLevel = NodeLevel.SkyDriveProGroup;
                    break;
                default:
                    break;
            }
            return nodeLevel;
        }
        #endregion

        #region 删除某个 objects
        private void DeleteObjects(List<RemoteNodeMessage> nodes)
        {
            var mailboxObjects = nodes.Where(n => MailboxLevels.Contains((RemoteNodeType)n.NodeType)).Select(n => n.NodeName).ToList();
            var remoteNodeObjects = nodes.Where(n => RemoteNodeLevels.Contains((RemoteNodeType)n.NodeType)).Select(n => n.NodeName).ToList();
            var onedriveNodeObjects = nodes.Where(n => OneDriveNodeLevels.Contains((RemoteNodeType)n.NodeType)).Select(n => n.NodeName).ToList();
            var o365GroupSites = nodes.Where(n => O365GroupSiteLevels.Contains((RemoteNodeType)n.NodeType)).Select(n => n.NodeName).ToList();
            var channelSites = nodes.Where(n => (RemoteNodeType)n.NodeType == RemoteNodeType.Channel).Select(n => n.NodeName).ToList();
            if (o365GroupSites.Count > 0)
            {
                remoteNodeObjects.AddRange(RemoteNodeService.GetO365GroupSiteUrlsByNames(o365GroupSites));
            }
            if (mailboxObjects.Count > 0)
            {
                DeleteMailboxObjects(mailboxObjects);
                ReportMangerFactory.Instance.ReportManager.Increase(mailboxObjects.Count);
            }
            if (remoteNodeObjects.Count > 0)
            {
                DeleteRemoteNodeObjects(remoteNodeObjects);
                ReportMangerFactory.Instance.ReportManager.Increase(remoteNodeObjects.Count);
            }
            if (channelSites.Count > 0)
            {
                DeletePrivateChannelObjects(channelSites);
                ReportMangerFactory.Instance.ReportManager.Increase(channelSites.Count);
            }
            if(onedriveNodeObjects.Count > 0)
            {
                DeleteOneDriveObjects(onedriveNodeObjects);
                ReportMangerFactory.Instance.ReportManager.Increase(onedriveNodeObjects.Count);
            }
        }
        private void DeleteMailboxObjects(List<string> nodes)
        {
            logger.Info("delete mailbox object count: {0}.", nodes.Count);
            DatabaseUtility.BatchOperation(nodes, (batchItems) => {
                var deleteItems = batchItems.ToList();
                MailboxRedisService.DeleteMailboxes(deleteItems, () =>
                {
                    var parentNames = EmailService.GetParentNamesByMailboxes(deleteItems, false);
                    EmailService.DeleteMailboxByNames(deleteItems);
                    foreach (var item in parentNames)
                    {
                        SyncDataJobProcessor.AddJobDetails4Removed(RMRemoteNodeSourceType.ExchangeOnline, item.Value, item.Key);
                    }
                });
            });
            logger.Info("delete mailbox objects successful.");
        }
        private void DeleteRemoteNodeObjects(List<string> nodes)
        {
            logger.Info("delete remotenode object count: {0}.", nodes.Count);
            DatabaseUtility.BatchOperation(nodes, (batchItems) => {
                var deleteItems = batchItems.ToList();
                RemoteNodeRedisService.DeleteRemoteNodes(deleteItems, () =>
                {
                    var parentNames = RemoteNodeService.GetContainerNameBySiteUrls(deleteItems);
                    RemoteNodeService.DeleteRemoteSiteCollectionsByUrl(deleteItems);
                    DeleteRemoteSiteAspect.DeleteRelatedDataByUrl(deleteItems);
                    foreach (var item in parentNames)
                    {
                        SyncDataJobProcessor.AddJobDetails4Removed(RMRemoteNodeSourceType.SharePointOnline, item.Value, item.Key);
                    }
                });
            });
            logger.Info("delete remotenode objects successful.");
        }

        private void DeleteOneDriveObjects(List<string> nodes)
        {
            logger.Info("delete onedrive object count: {0}.", nodes.Count);
            DatabaseUtility.BatchOperation(nodes, (batchItems) => {
                var deleteItems = batchItems.ToList();
                RemoteNodeRedisService.DeleteRemoteNodes(deleteItems, () =>
                {
                    var parentNames = RemoteNodeService.GetContainerNameBySiteUrls(deleteItems);
                    RemoteNodeService.DeleteRemoteSiteCollectionsByUrl(deleteItems);
                    DeleteRemoteSiteAspect.DeleteRelatedDataByUrl(deleteItems);
                    foreach (var item in parentNames)
                    {
                        SyncDataJobProcessor.AddJobDetails4Removed(RMRemoteNodeSourceType.OneDrive, item.Value, item.Key);
                    }
                });
            });
            logger.Info("delete remotenode objects successful.");
        }

        private void DeletePrivateChannelObjects(List<string> nodes)
        {
            logger.Info("delete private channel object count: {0}.", nodes.Count);
            DatabaseUtility.BatchOperation(nodes, (batchItems) => {
                var deleteItems = batchItems.ToList();
                ChannelRedisService.DeletePrivateChannels(deleteItems, () =>
                {
                    foreach (var item in deleteItems)
                    {
                        SyncDataJobProcessor.AddJobDetails4Removed(RMRemoteNodeSourceType.SharePointOnline, RMConstants.DefaultPrivateChannelSitesGroup, item);
                    }
                    this.RemoteNodeService.DeleteRemoteSiteCollectionsByUrl(deleteItems);
                    this.DeleteRemoteSiteAspect.DeleteRelatedDataByUrl(deleteItems);
                });
            });
            logger.Info("delete private channel objects successful.");
        }
        #endregion

        private static void UpdateNodeTypeForProjectOnline(List<RemoteNodeMessage> nodes)
        {
            nodes.ForEach(node =>
            {
                if (node.NodeType == (int)RemoteNodeType.ProjectOnline)
                {
                    node.NodeType = (int)RemoteNodeType.SiteCollection;
                }
            });
        }
        private static List<RemoteNodeMessage> GetNodesFromByMessage(RMAosQueueMessage message)
        {
            DeleteNodesMessage deleteNodesMessage = message.DeleteNodesMessage;
            logger.Info("begin to get nodes from delete-node message");
            var msg = string.Empty;
            if(deleteNodesMessage.Content.IsNewMessage)
            {
                msg = RAStorageUtil.DownloadFileMessageFromStorageBySasTokenUrl(deleteNodesMessage.Content.StorageSasUri, deleteNodesMessage.Content.FileLowName);
            }
            else
            {
                msg = RAStorageUtil.DownloadFileMessageFromStorageByXri(deleteNodesMessage.Content.StorageXri, deleteNodesMessage.Content.FileLowName);
            }
            if (string.IsNullOrEmpty(msg))
            {
                logger.Error("The message for downloading nodes is null.");
                return new List<RemoteNodeMessage>();
            }
            return SerializerHelper.DeserializeByDataContractJsonSerializer<List<RemoteNodeMessage>>(msg);
        }
    }
}

