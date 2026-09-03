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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.SyncNode.Compatible;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Service.Services.Tenant.Notification;
using AvePoint.RA.Service.Services.Tenant.Notification.Excutor;
using AvePoint.RA.Service.Services.Tenant.SyncNodes.Cache;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common;
using Cloud.Sdk.Data.Aos.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using AOS = Cloud.Sdk.Data.Aos;
using CA = AvePoint.GCommon.Contract.CentralAdmin.Object;

namespace AvePoint.RA.Service.Services.Tenant.SyncNodes
{
    public class SyncPrivateChannelService : AbstractSyncService<RemoteSiteCollection>, ISyncService
    {
        private static RALogger logger = RALogger.GetInstance(typeof(SyncRemoteNodesService));
        private Dictionary<string, SyncRemoteNodePara> updateObjectsDict = new Dictionary<string, SyncRemoteNodePara>();
        private List<string> deleteOjects = new List<string>();
        private Dictionary<string, RemoteSiteCollection> urlToRemoteNodesDict = new Dictionary<string, RemoteSiteCollection>();
        private const string defaultPrivateChannelSiteContainerId = "41cfe969-e07b-45cb-a7d0-b022f967e929";

        public SyncPrivateChannelService(SyncDataJobContext context) : base(context)
        {
        }

        private ISyncChannelRedisService SyncChannelRedisService
        {
            get
            {
                return PlatformWindsorManager.GetService<ISyncChannelRedisService>();
            }
        }

        private IRMRemoteNodeService RemoteNodeService
        {
            get
            {
                return PlatformWindsorManager.GetService<IRMRemoteNodeService>();
            }
        }

        private IRMDeleteRemoteSiteAspect DeleteSiteAspect
        {
            get
            {
                return PlatformWindsorManager.GetService<IRMDeleteRemoteSiteAspect>();
            }
        }

        protected override void AddToListOfNewGroup(RMCompatibleRemoteNode node)
        {
            throw new NotImplementedException();
        }

        protected override string FieldKeySelector(RemoteSiteCollection node)
        {
            return node.url.ToLower();
        }

        protected override NodeLevel ConvertNodeLevel(RMCompatibleRemoteNode node)
        {
            return node.ChannelType switch
            {
                Cloud.Sdk.Data.AosModern.ChannelType.Private => NodeLevel.PrivateChannel,
                Cloud.Sdk.Data.AosModern.ChannelType.Shared => NodeLevel.SharedChannel,
                _ => throw new ArgumentOutOfRangeException("Current node type {0} not supported sc sync."),
            };
        }

        protected override void AddGroupsToCache(string tenantGroupId, Dictionary<string, RemoteNodePara> newGroupsCache)
        {
            throw new NotImplementedException();
        }

        protected override void AddNodesToCache(string tenantGroupId, Dictionary<string, SyncRemoteNodePara> newNodesDict)
        {
            SyncChannelRedisService.AddNodesToCache(tenantGroupId, newNodesDict);
        }

        protected override async System.Threading.Tasks.Task AddToListsForNodesAsync(RMCompatibleRemoteNode node)
        {
            if (!urlToRemoteNodesDict.ContainsKey(node.Url.ToLower()))
            {
                var remoteSiteCollection = new RemoteSiteCollection()
                {
                    CreateTime = DateTime.UtcNow.Ticks,
                    domain = node.DomainName,
                    id = node.Id,
                    Name = node.Name,
                    //ObjectId = node.ObjectId,
                    IsPublicWebSite = node.IsPublicWebSite,
                    NodeType = RemoveNodeType.PrivateChannel,
                    parentId = defaultPrivateChannelSiteContainerId,
                    ChannelType = (TeamsChannelType)node.ChannelType,
                    SiteCollectionType = GCommon.Contract.SharePointBrowser.SiteCollectionType.PrivateChannel,
                    state = SiteCollectionState.AccessAll,
                    SPVersion = string.IsNullOrEmpty(node.SPVersion) ? "15.0.0.0" : node.SPVersion,
                    TemplateName = node.TemplateName,
                    TemplateTitle = node.TemplateTitle,
                    url = node.Url,
                    username = node.UserName,
                    password = string.Empty,
                    AdminUrl = node.AdminUrl,
                    TenantId = string.IsNullOrEmpty(node.TenantId) ? string.Empty : node.TenantId,
                    AuthType = (CA.BposConnectionType)node.ConnectionType,
                    AppType = ConvertIdentityTypeToAppType(node.AppProfileType), // AOS AppToken方式Scan才有意义
                    ScanSource = RemoteNodeScanSource.AOS,
                    ServiceAccountId = GetServiceAccountId(node),
                    TeamId = node.ParentId
                };

                var siteUrl = remoteSiteCollection.url;
                try
                {
                    var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(remoteSiteCollection);
                    var factory = MultiAppUtil.CreateAveObjectModelFactory(siteUrl, bposInfo, AveContextKind.ClientObjectModel);
                    var aveSite = factory.CreateSite(siteUrl);
                    remoteSiteCollection.ObjectId = aveSite.ID.ToString();
                }
                catch (Exception e)
                {
                    logger.Error($"Get site id failed for: {siteUrl}. {e}");
                }

                urlToRemoteNodesDict.Add(node.Url.ToLower(), remoteSiteCollection);
            }
        }

        protected override bool CheckExitedGroup(RMCompatibleRemoteNode node, RemoteNodePara group)
        {
            throw new NotImplementedException();
        }

        protected override SyncRemoteNodePara ConvertDaoNodeModelToCacheModel(RemoteSiteCollection node)
        {
            return new SyncRemoteNodePara()
            {
                NodeName = node.url,
                AppType = node.AppType,
                AuthType = node.AuthType,
                ServiceAccountId = node.ServiceAccountId,
                TenantId = node.TenantId,
                UserName = node.username,
            };
        }

        protected override RemoteNodePara GetGroupCacheByNameAndNodeLevel(string parentName, RMCompatibleRemoteNode aosNode)
        {
            throw new NotImplementedException();
        }

        protected override string GetGroupFieldKey(RMCompatibleRemoteNode aosNode)
        {
            throw new NotImplementedException();
        }

        protected override RemoteNodePara GetGroupFromDB(string groupFieldKey)
        {
            throw new NotImplementedException();
        }

        protected override Dictionary<string, RemoteNodePara> GetGroupsCache(string tenantGroupId, List<RMCompatibleRemoteNode> aosNodes)
        {
            throw new NotImplementedException();
        }

        protected override List<RemoteSiteCollection> GetNodesFromDBByUrls(List<string> urls)
        {
            return RemoteNodeService.GetRemoteSiteCollectionBySiteUrls(urls);
        }

        protected override Dictionary<string, SyncRemoteNodePara> GetNodesCache(string tenantGroupId, List<string> aosSyncNodes)
        {
            return SyncChannelRedisService.GetNodesCache(tenantGroupId, aosSyncNodes);
        }

        protected override string GetServiceAccountId(RMCompatibleRemoteNode node)
        {
            var serviceAccountId = string.Empty;
            var authType = (CA.BposConnectionType)node.ConnectionType;
            switch (authType)
            {
                case CA.BposConnectionType.ServiceAccount:
                    serviceAccountId = HashCodeHelper.ToMD5HashCode(node.UserName.ToLowerInvariant());
                    break;
                case CA.BposConnectionType.AppToken:
                    if (string.IsNullOrEmpty(node.UserName))
                    { // AppProfile
                        serviceAccountId = string.Empty;
                    }
                    else
                    { // AppProfile + MFA
                        serviceAccountId = HashCodeHelper.ToMD5HashCode(node.UserName.ToLowerInvariant());
                    }
                    break;
                case CA.BposConnectionType.Modern:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("AuthType is {0} and out of range.", authType.ToString());
            }
            return serviceAccountId;
        }

        protected override string GetUserName(RMCompatibleRemoteNode node)
        {
            return string.Empty;
        }

        protected override void InitCache(string tenantGroupId)
        {
            SyncChannelRedisService.InitCache(tenantGroupId);
        }

        protected override void SyncNodesAndGroups(string tenantGroupId, Dictionary<string, SyncRemoteNodePara> updateObjectsDict, List<string> deleteUrls, List<string> deleteOneDriveUrls, Dictionary<string, SyncRemoteNodePara> updateSecondParentIdDict)
        {
            //当前逻辑中只有一个private Channel Group 若不存在 则创建
            if (!RemoteNodeService.IsPrivateChannelGroupExist())
            {
                RemoteNodeService.CreateRemoteWebApplications(new List<RemoteWebApplication>() {
                    new RemoteWebApplication() {
                         url = RMConstants.DefaultPrivateChannelSitesGroup,
                         id = defaultPrivateChannelSiteContainerId,
                         NodeType  = RemoveNodeType.PrivateChannel,
                         FromDAO = executorContext.InitializedFromDAO,
                         AosId = defaultPrivateChannelSiteContainerId
                    }
                });
                SyncDataJobProcessor.AddJobDetails4Added(RMRemoteNodeSourceType.SharePointOnline, RMConstants.DefaultPrivateChannelSitesGroup);
            }
            if (urlToRemoteNodesDict.Count > 0)
            {
                List<RemoteSiteCollection> newSiteCollections = urlToRemoteNodesDict.Values.ToList();
                var cacheDict = ConvertRemoteNodeDictToCacheDict(urlToRemoteNodesDict);
                SyncChannelRedisService.AddNodesToCache(tenantGroupId, cacheDict,
                    () => {
                        newSiteCollections.ForEach(s => s.FromDAO = executorContext.InitializedFromDAO);
                        RemoteNodeService.SyncRemoteSiteCollections(newSiteCollections);
                    });
                logger.Info("Add {0} private channels.", urlToRemoteNodesDict.Count);
                LogSyncDataInfo(urlToRemoteNodesDict.Keys.ToList());
                SyncDataJobProcessor.AddJobDetails4ObjectAdded(RMRemoteNodeSourceType.SharePointOnline, RMConstants.DefaultPrivateChannelSitesGroup, urlToRemoteNodesDict.Keys);
            }
            if (updateObjectsDict.Count > 0)
            {
                SyncChannelRedisService.UpdateNodesToCache(tenantGroupId, updateObjectsDict,
                    () => { RemoteNodeService.UpdateSyncSiteCollections(updateObjectsDict.Values.ToList()); });
                logger.Info("Update {0} remotenodes.", updateObjectsDict.Count);
                LogSyncDataInfo(updateObjectsDict.Keys.ToList());
                SyncDataJobProcessor.AddJobDetails4Updated(RMRemoteNodeSourceType.SharePointOnline, RMConstants.DefaultPrivateChannelSitesGroup, urlToRemoteNodesDict.Keys);
            }
            if (deleteUrls.Count > 0)
            {
                SyncChannelRedisService.DeleteNodesFromCache(tenantGroupId, deleteUrls, () =>
                {
                    RemoteNodeService.DeleteRemoteSiteCollectionsByUrl(deleteUrls);
                    DeleteSiteAspect.DeleteRelatedDataByUrl(deleteUrls);
                });
                logger.Info("Delete {0} remotenodes.", deleteUrls.Count);
                LogSyncDataInfo(deleteUrls);
                SyncDataJobProcessor.AddJobDetails4Removed(RMRemoteNodeSourceType.SharePointOnline, RMConstants.DefaultPrivateChannelSitesGroup, urlToRemoteNodesDict.Keys);
            }
            logger.Info("Sync remotenode containers and nodes successfully.");
        }

        public new void Execute(SyncNodesSettings syncNodesSettings, List<RMCompatibleRemoteNode> aosNodes)
        {
            if (aosNodes == null || aosNodes.Count == 0)
            {
                logger.Info("No channels should be sync.");
                return;
            }
            try
            {
                logger.Info($"Start to sync private channel. Count: [{aosNodes.Count}].");
                InitCache(syncNodesSettings.TenantGroupId);
                BatchExecute(aosNodes, (batchAosNodes) =>
                {
                    SyncPrivateChannelAsync(syncNodesSettings, batchAosNodes).Wait();
                    SyncNodesAndGroups(syncNodesSettings.TenantGroupId, updateObjectsDict, deleteOjects, null, null);
                    urlToRemoteNodesDict.Clear();
                    updateObjectsDict.Clear();
                });
            }
            catch (Exception e)
            {
                logger.Error("Failed to sync private channel. Exception is {0}.", e.ToString());
                throw;
            }
        }

        private async System.Threading.Tasks.Task SyncPrivateChannelAsync(SyncNodesSettings syncNodesSettings, List<RMCompatibleRemoteNode> aosSyncNodes)
        {
            aosSyncNodes = aosSyncNodes.Where(n => !string.IsNullOrEmpty(n.Url)).ToList();
            if (aosSyncNodes.Count == 0)
            {
                return;
            }
            Dictionary<string, SyncRemoteNodePara> urlToCacheDict = GetNodesFromCacheInternal(syncNodesSettings.TenantGroupId, aosSyncNodes);
            foreach (var aosSyncNode in aosSyncNodes)
            {
                if (aosSyncNode.ChannelType == Cloud.Sdk.Data.AosModern.ChannelType.Shared)
                {
                    logger.Warn($"Not support shared type channel. Skipped it.");
                    //continue;
                }
                var aosSyncNodeUrl = aosSyncNode.Url.ToLower();
                if (!urlToCacheDict.ContainsKey(aosSyncNodeUrl))
                {
                    logger.Error("Private channel {0} should exist in dict but actually not.", aosSyncNode.Url);
                    continue;
                }
                SyncRemoteNodePara cachedNode = urlToCacheDict[aosSyncNodeUrl];
                if (IsNewNode(aosSyncNode, cachedNode))
                {
                    await AddToListsForNodesAsync(aosSyncNode);
                }
                else
                {
                    if (cachedNode == null)
                    {
                        logger.Info("Current private channel not exist in cache , parentId or url is null. object: {0}.", aosSyncNode.Url);
                        continue;
                    }
                    if (WhetherNodeUpdate(cachedNode, aosSyncNode))
                    {
                        SyncRemoteNodePara newUpdateNode = ConvertRemoteNodeToCachedNode(aosSyncNode, true);
                        if (!updateObjectsDict.ContainsKey(newUpdateNode.NodeName))
                        {
                            newUpdateNode.ParentId = defaultPrivateChannelSiteContainerId;
                            updateObjectsDict.Add(newUpdateNode.NodeName, newUpdateNode);
                        }
                        else
                        {
                            logger.Error("{0} has been added to update dict.", newUpdateNode.NodeName);
                        }
                        logger.Info("Update out of policy {0} object: {1}, node type:{2}, node name: {3}, app type: {4}", aosSyncNode.NodeType, aosSyncNode.Url, aosSyncNode.NodeType, aosSyncNode.Name ?? string.Empty, ConvertIdentityTypeToAppType(aosSyncNode.AppProfileType).ToString());
                    }
                }
            }
        }

        #region 判断是否应该更新节点
        private bool WhetherNodeUpdate(SyncRemoteNodePara cachedNode, RMCompatibleRemoteNode syncNode)
        {
            return IsTenantIdChanged(cachedNode, syncNode) ||
                IsServiceAccountIdChanged(cachedNode, syncNode) ||
                IsAuthTypeChanged(cachedNode, syncNode) ||
                IsAppTypeChanged(cachedNode, syncNode) ||
                IsTeamIdChanged(cachedNode, syncNode);
        }

        private bool IsTeamIdChanged(SyncRemoteNodePara cachedNode, RMCompatibleRemoteNode syncNode)
        {
            cachedNode.TeamId = cachedNode.TeamId ?? string.Empty;
            syncNode.ParentId = syncNode.ParentId ?? string.Empty;
            return !cachedNode.TeamId.Equals(syncNode.ParentId, StringComparison.Ordinal);
        }

        #endregion

        private Dictionary<string, SyncRemoteNodePara> ConvertRemoteNodeDictToCacheDict(Dictionary<string, RemoteSiteCollection> dict)
        {
            var result = new Dictionary<string, SyncRemoteNodePara>();
            foreach (var pair in dict)
            {
                result.Add(pair.Key, ConvertDaoNodeModelToCacheModel(pair.Value));
            }
            return result;
        }

        protected override void AddToListOfUpdateGroup(RMCompatibleRemoteNode node, RemoteNodePara existGroup)
        {
            throw new NotImplementedException();
        }
    }
}

