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
using Cloud.Sdk.Data.Aos.Tenant;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.Common.RemoteNode;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using CAObject = AvePoint.GCommon.Contract.CentralAdmin.Object;
using DAOSP = AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Service.Services.Tenant.SyncNodes.Cache;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Service.Services.Tenant.Notification.Excutor;
using AvePoint.RA.Service.Services.Tenant.Notification;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Common.SyncNode.Compatible;

namespace AvePoint.RA.Service.Services.Tenant.SyncNodes
{
    public class SyncRemoteNodesService : AbstractSyncService<RemoteSiteCollection>, ISyncService
    {
        private static RALogger logger = RALogger.GetInstance(typeof(SyncRemoteNodesService));
        private Dictionary<string, RemoteWebApplication> newGroupsDict = new Dictionary<string, RemoteWebApplication>();
        private Dictionary<string, RemoteWebApplication> updateGroupsDict = new Dictionary<string, RemoteWebApplication>();
        private Dictionary<string, RemoteSiteCollection> urlToRemoteNodesDict = new Dictionary<string, RemoteSiteCollection>();

        public SyncRemoteNodesService(SyncDataJobContext context) : base(context)
        {
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

        private ISyncRemoteNodeRedisService RemoteNodeCacheService
        {
            get
            {
                return PlatformWindsorManager.GetService<ISyncRemoteNodeRedisService>();
            }
        }

        #region Cache
        #region GetGroupsCache
        protected override Dictionary<string, RemoteNodePara> GetGroupsCache(string tenantGroupId, List<RMCompatibleRemoteNode> aosNodes)
        {
            return RemoteNodeCacheService.GetGroupsCache(tenantGroupId, aosNodes);
        }

        protected override string GetGroupFieldKey(RMCompatibleRemoteNode aosNode)
        {
            //return RedisFieldKeyUtil.GenerateRemoteNodeGroupFieldKey(aosNode.NodeType, aosNode.ParentName);
            return RedisFieldKeyUtil.GenerateRemoteNodeGroupFieldKeyByAosId(aosNode.NodeType, aosNode.ParentId);
        }

        protected override RemoteNodePara GetGroupCacheByNameAndNodeLevel(string parentName, RMCompatibleRemoteNode aosNode)
        {
            //return RemoteNodeService.GetGroupByNameAndNodeLevel(aosNode.ParentName, (int)ConvertGroupNodeLevel(aosNode.NodeType));
            return RemoteNodeService.GetGroupByAosIdAndNodeLevel(aosNode.ParentId, (int)ConvertGroupNodeLevel(aosNode.NodeType));
        }

        protected override void AddGroupsToCache(string tenantGroupId, Dictionary<string, RemoteNodePara> newGroupsCache)
        {
            RemoteNodeCacheService.AddGroupsToCache(tenantGroupId, newGroupsCache);
        }

        protected override RemoteNodePara GetGroupFromDB(string groupFieldKey)
        {
            RemoteNodeCachePair cachePair = RedisFieldKeyUtil.GenerateRemoteNodeCachePair(groupFieldKey);
            //return RemoteNodeService.GetGroupByNameAndNodeLevel(cachePair.GroupName, (int)(cachePair.NodeLevel));
            return RemoteNodeService.GetGroupByAosIdAndNodeLevel(cachePair.GroupName, (int)(cachePair.NodeLevel));
        }
        #endregion

        #region GetNodesCache
        protected override Dictionary<string, SyncRemoteNodePara> GetNodesCache(string tenantGroupId, List<string> urls)
        {
            return RemoteNodeCacheService.GetNodesCache(tenantGroupId, urls);
        }

        protected override List<RemoteSiteCollection> GetNodesFromDBByUrls(List<string> urls)
        {
            return RemoteNodeService.GetRemoteSiteCollectionBySiteUrls(urls);
        }

        protected override SyncRemoteNodePara ConvertDaoNodeModelToCacheModel(RemoteSiteCollection node)
        {
            return new SyncRemoteNodePara()
            {
                NodeName = node.url,
                ParentId = node.parentId,
                AppType = node.AppType,
                AuthType = node.AuthType,
                ServiceAccountId = node.ServiceAccountId,
                TenantId = node.TenantId,
                ScanSource = node.ScanSource,
                TeamId = node.TeamId,
                NodeLevel = ConvertRemoveNodeType2NodeLevel(node),
            };
        }

        protected override string FieldKeySelector(RemoteSiteCollection node)
        {
            return node.url.ToLower();
        }

        protected override void AddNodesToCache(string tenantGroupId, Dictionary<string, SyncRemoteNodePara> newNodesDict)
        {
            RemoteNodeCacheService.AddNodesToCache(tenantGroupId, newNodesDict);
        }
        #endregion

        protected override void InitCache(string tenantGroupId)
        {
            RemoteNodeCacheService.InitCache(tenantGroupId);
        }
        #endregion

        protected override void AddToListOfNewGroup(RMCompatibleRemoteNode node)
        {
            var nodeType = ConvertNodeType(node.NodeType);
            //string groupKey = RedisFieldKeyUtil.GenerateRemoteNodeGroupFieldKey(nodeType, node.ParentName);
            string groupKey = RedisFieldKeyUtil.GenerateRemoteNodeGroupFieldKey(nodeType, node.ParentId);
            if (!newGroupsDict.Keys.Contains(groupKey))
            {
                newGroupsDict.Add(groupKey, new RemoteWebApplication()
                {
                    id = node.ParentId,
                    url = node.ParentName,
                    NodeType = nodeType,
                    AosId = node.ParentId
                });
            }
        }

        protected override void AddToListOfUpdateGroup(RMCompatibleRemoteNode node, RemoteNodePara existGroup)
        {
            if(node.ParentName == existGroup.NodeName)
            {
                return;
            }

            logger.Debug($"Synced group name [{node.ParentName}], Cached group name [{existGroup.NodeName}]");

            var nodeType = ConvertNodeType(node.NodeType);
            //string groupKey = RedisFieldKeyUtil.GenerateRemoteNodeGroupFieldKey(nodeType, node.ParentName);
            string groupKey = RedisFieldKeyUtil.GenerateRemoteNodeGroupFieldKey(nodeType, node.ParentId);
            if (!updateGroupsDict.Keys.Contains(groupKey))
            {
                updateGroupsDict.Add(groupKey, new RemoteWebApplication()
                {
                    id = node.ParentId,
                    url = node.ParentName,
                    NodeType = nodeType,
                    AosId = node.ParentId
                });
            }
        }

        protected override async System.Threading.Tasks.Task AddToListsForNodesAsync(RMCompatibleRemoteNode node)
        {
            if (!urlToRemoteNodesDict.Keys.Contains(node.Url.ToLower()))
            {
                urlToRemoteNodesDict.Add(node.Url.ToLower(), new RemoteSiteCollection()
                {
                    CreateTime = DateTime.UtcNow.Ticks,
                    domain = node.DomainName,
                    id = node.Id,
                    ObjectId = node.ObjectId,
                    Name = node.Name,
                    parentName = node.ParentName,
                    IsPublicWebSite = node.IsPublicWebSite,
                    NodeType = ConvertNodeType(node.NodeType),
                    parentId = node.ParentId,
                    ChannelType = (TeamsChannelType)node.ChannelType,
                    SiteCollectionType = (node.GroupType == Cloud.Sdk.Data.AosModern.O365GroupType.TeamsGroup) ? DAOSP.SiteCollectionType.Teams : (DAOSP.SiteCollectionType)(int)node.SiteCollectionType,
                    state = SiteCollectionState.AccessAll,
                    SPVersion = node.SPVersion,
                    TemplateName = node.TemplateName,
                    TemplateTitle = node.TemplateTitle,
                    url = node.Url,
                    username = node.UserName,
                    password = string.Empty,
                    AdminUrl = node.AdminUrl,
                    TenantId = string.IsNullOrEmpty(node.TenantId) ? string.Empty : node.TenantId,
                    AuthType = (CAObject.BposConnectionType)node.ConnectionType,
                    AppType = ConvertIdentityTypeToAppType(node.AppProfileType), // AOS AppToken方式Scan才有意义
                    ScanSource = RemoteNodeScanSource.AOS,
                    ServiceAccountId = GetServiceAccountId(node),
                    TeamId = node.ExternalId ?? string.Empty
                });
            }
        }

        protected override string GetUserName(RMCompatibleRemoteNode node)
        {
            return string.Empty;
        }

        protected override string GetServiceAccountId(RMCompatibleRemoteNode node)
        {
            var serviceAccountId = string.Empty;
            //var authType = (CAObject.BposConnectionType)node.ConnectionType;
            //switch (authType)
            //{
            //    case CAObject.BposConnectionType.ServiceAccount:
            //        serviceAccountId = HashCodeHelper.ToMD5HashCode(node.UserName.ToLowerInvariant());
            //        break;
            //    case CAObject.BposConnectionType.AppToken:
            //        if (string.IsNullOrEmpty(node.UserName))
            //        { // AppProfile
            //            serviceAccountId = string.Empty;
            //        }
            //        else
            //        { // AppProfile + MFA
            //            serviceAccountId = HashCodeHelper.ToMD5HashCode(node.UserName.ToLowerInvariant());
            //        }
            //        break;
            //    case CAObject.BposConnectionType.Modern:
            //        break
            //    default:
            //        throw new ArgumentOutOfRangeException($"AuthType is {authType.ToString()} and out of range.");
            //}
            return serviceAccountId;
        }

        protected override bool CheckExitedGroup(RMCompatibleRemoteNode node, RemoteNodePara group)
        {
            return group.NodeType == ConvertNodeType(node.NodeType);
        }

        protected override void SyncNodesAndGroups(string tenantGroupId, Dictionary<string, SyncRemoteNodePara> updateObjectsDict, List<string> deleteUrls, List<string> deleteOneDriveUrls, Dictionary<string, SyncRemoteNodePara> updateSecondParentIdDict)
        {
            logger.Info("Begin to sync remote node containers and nodes.");
            if (newGroupsDict.Count > 0)
            {
                List<RemoteWebApplication> addedGroups = newGroupsDict.Values.ToList();
                Dictionary<string, RemoteNodePara> addedCachedGroups = ConvertDBGroupDictToCacheDict(newGroupsDict);
                RemoteNodeCacheService.AddGroupsToCache(tenantGroupId, addedCachedGroups,
                    () => { RemoteNodeService.CreateRemoteWebApplications(addedGroups); });
                logger.Info("Add {0} remotenode containers.", newGroupsDict.Count);
                LogSyncDataInfo(newGroupsDict.Keys.ToList());
                var addedOneDriveGroups = addedGroups.Where(item => item.NodeType == RemoveNodeType.SkyDrivePro);
                var addedOtherGroups = addedGroups.Where(item => item.NodeType != RemoveNodeType.SkyDrivePro);
                SyncDataJobProcessor.AddJobDetails4ContainerAdded(RMRemoteNodeSourceType.SharePointOnline, addedOtherGroups.Select(g => g.url));
                SyncDataJobProcessor.AddJobDetails4ContainerAdded(RMRemoteNodeSourceType.OneDrive, addedOneDriveGroups.Select(g => g.url));
            }
            if(updateGroupsDict.Count > 0)
            {
                List<RemoteWebApplication> updateGroups = updateGroupsDict.Values.ToList();
                Dictionary<string, RemoteNodePara> updateCachedGroups = ConvertDBGroupDictToCacheDict(updateGroupsDict);
                RemoteNodeCacheService.UpdateGroupToCache(tenantGroupId, updateCachedGroups,
                    () => { RemoteNodeService.UpdateRemoteWebApplications(updateGroups); });
                logger.Info("Update {0} remotenode containers.", updateGroupsDict.Count);
                LogSyncDataInfo(newGroupsDict.Keys.ToList());
                var updateOneDriveGroups = updateGroups.Where(item => item.NodeType == RemoveNodeType.SkyDrivePro);
                var updateOtherGroups = updateGroups.Where(item => item.NodeType != RemoveNodeType.SkyDrivePro);
                SyncDataJobProcessor.AddJobDetails4ContainerUpdate(RMRemoteNodeSourceType.SharePointOnline, updateOtherGroups.Select(g => g.url));
                SyncDataJobProcessor.AddJobDetails4ContainerUpdate(RMRemoteNodeSourceType.OneDrive, updateOneDriveGroups.Select(g => g.url));
            }
            if (urlToRemoteNodesDict.Count > 0)
            {
                List<RemoteSiteCollection> newSiteCollections = urlToRemoteNodesDict.Values.ToList();
                FilterGroupNodesIfExist(newSiteCollections);
                var cacheDict = ConvertRemoteNodeDictToCacheDict(urlToRemoteNodesDict);
                RemoteNodeCacheService.AddNodesToCache(tenantGroupId, cacheDict,
                    () => {
                        newSiteCollections.ForEach(s => s.FromDAO = executorContext.InitializedFromDAO);
                        RemoteNodeService.SyncRemoteSiteCollections(newSiteCollections); 
                    });
                logger.Info("Add {0} remotenode.", urlToRemoteNodesDict.Count);
                LogSyncDataInfo(urlToRemoteNodesDict.Keys.ToList());
                foreach (var item in urlToRemoteNodesDict)
                {
                    SyncDataJobProcessor.AddJobDetails4Added(item.Value.NodeType == RemoveNodeType.SkyDrivePro ? RMRemoteNodeSourceType.OneDrive : RMRemoteNodeSourceType.SharePointOnline, item.Value.parentName, item.Key);
                }
            }
            if (updateObjectsDict.Count > 0)
            {
                RemoteNodeCacheService.UpdateNodesToCache(tenantGroupId, updateObjectsDict,
                    () => {
                        RemoteNodeService.UpdateSyncSiteCollections(updateObjectsDict.Values.ToList());
                    });
                logger.Info("Update {0} remotenodes.", updateObjectsDict.Count);
                LogSyncDataInfo(updateObjectsDict.Keys.ToList());
                foreach (var item in updateObjectsDict)
                {
                    SyncDataJobProcessor.AddJobDetails4Updated(item.Value.NodeLevel == NodeLevel.SkyDrivePro ? RMRemoteNodeSourceType.OneDrive : RMRemoteNodeSourceType.SharePointOnline, item.Value.ParentName, item.Key);
                }
            }
            if (deleteUrls.Count > 0)
            {
                RemoteNodeCacheService.DeleteNodesFromCache(tenantGroupId, deleteUrls,
                    () =>
                    {
                        var parentNames = RemoteNodeService.GetContainerNameBySiteUrls(deleteUrls);
                        RemoteNodeService.DeleteRemoteSiteCollectionsByUrl(deleteUrls);
                        DeleteSiteAspect.DeleteRelatedDataByUrl(deleteUrls);
                        foreach (var item in parentNames)
                        {
                            SyncDataJobProcessor.AddJobDetails4Removed(RMRemoteNodeSourceType.SharePointOnline, item.Value, item.Key);
                        }
                    });
                logger.Info("Delete {0} remotenodes.", deleteUrls.Count);
                LogSyncDataInfo(deleteUrls);
            }
            if(deleteOneDriveUrls.Count > 0)
            {
                RemoteNodeCacheService.DeleteNodesFromCache(tenantGroupId, deleteOneDriveUrls,
                       () =>
                       {
                           var parentNames = RemoteNodeService.GetContainerNameBySiteUrls(deleteOneDriveUrls);
                           RemoteNodeService.DeleteRemoteSiteCollectionsByUrl(deleteOneDriveUrls);
                           DeleteSiteAspect.DeleteRelatedDataByUrl(deleteOneDriveUrls);
                           foreach (var item in parentNames)
                           {
                               SyncDataJobProcessor.AddJobDetails4Removed(RMRemoteNodeSourceType.OneDrive, item.Value, item.Key);
                           }
                       });
                logger.Info("Delete {0} remotenodes.", deleteUrls.Count);
                LogSyncDataInfo(deleteUrls);

            }
            if (updateSecondParentIdDict.Count > 0)
            {
                RemoteNodeCacheService.UpdateNodesToCache(tenantGroupId, updateSecondParentIdDict,
                    () => { RemoteNodeService.UpdateSiteCollectionSecondParentId(updateSecondParentIdDict.Values.ToList()); });
                logger.Info("Update SecondParentIdDict {0} remotenodes.", updateSecondParentIdDict.Count);
                LogSyncDataInfo(updateSecondParentIdDict.Keys.ToList());
                foreach (var item in updateSecondParentIdDict)
                {
                    SyncDataJobProcessor.AddJobDetails4Updated(item.Value.NodeLevel == NodeLevel.SkyDrivePro ? RMRemoteNodeSourceType.OneDrive : RMRemoteNodeSourceType.SharePointOnline, item.Value.ParentName, item.Value.NodeName);
                }
            }
            logger.Info("Sync remotenode containers and nodes successfully.");
        }

        private void FilterGroupNodesIfExist(List<RemoteSiteCollection> newSiteCollections)
        {
            if (newSiteCollections.FirstOrDefault()?.NodeType == RemoveNodeType.O365GroupSites)
            {
                var needUpdateO365GroupSites = new List<RemoteSiteCollection>();
                var urls = newSiteCollections.Select(item => item.url).Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
                var existsUrls = RemoteNodeService.GetO365GroupSiteByUrls(urls).Select(item => item.ToLower()).ToHashSet();
                newSiteCollections.ForEach(newSc =>
                {
                    if (!string.IsNullOrWhiteSpace(newSc.url) && existsUrls.Contains(newSc.url.ToLower()))
                    {
                        needUpdateO365GroupSites.Add(newSc);
                    }
                });
                if (needUpdateO365GroupSites.Count != 0)
                {
                    needUpdateO365GroupSites.ForEach(nUOGS =>
                    {
                        newSiteCollections.Remove(nUOGS);
                    });

                    RemoteNodeService.UpdateO365GroupSiteByUrls(needUpdateO365GroupSites);
                    var names = needUpdateO365GroupSites.Select(nUOGS => nUOGS.url).ToList();
                    logger.Info("update {0} remotenodes by name.", names.Count);
                    LogSyncDataInfo(names);
                }
            }
        }

        private Dictionary<string, RemoteNodePara> ConvertDBGroupDictToCacheDict(Dictionary<string, RemoteWebApplication> dbGroupDict)
        {
            if (dbGroupDict == null || dbGroupDict.Count == 0)
            {
                return new Dictionary<string, RemoteNodePara>();
            }
            var result = new Dictionary<string, RemoteNodePara>();
            foreach (var pair in dbGroupDict)
            {
                result.Add(pair.Key, ConvertDBGroupModelToCacheModel(pair.Value));
            }
            return result;
        }

        private RemoteNodePara ConvertDBGroupModelToCacheModel(RemoteWebApplication daoGroupModel)
        {
            if (daoGroupModel == null)
            {
                throw new ArgumentNullException("Dao group model is null.");
            }
            return new RemoteNodePara()
            {
                NodeId = daoGroupModel.id,
                NodeName = daoGroupModel.url,
                NodeType = daoGroupModel.NodeType,
                AosId = daoGroupModel.AosId
            };
        }

        private Dictionary<string, SyncRemoteNodePara> ConvertRemoteNodeDictToCacheDict(Dictionary<string, RemoteSiteCollection> dict)
        {
            var result = new Dictionary<string, SyncRemoteNodePara>();
            foreach (var pair in dict)
            {
                result.Add(pair.Key, ConvertDaoNodeModelToCacheModel(pair.Value));
            }
            return result;
        }

        private AvePoint.GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType ConvertNodeType(Cloud.Sdk.Data.AosModern.RemoteNodeType syncNodeType)
        {
            if (syncNodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.OneDrive)
            {
                return RemoveNodeType.SkyDrivePro;
            }
            else if (syncNodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.Office365Group)
            {
                return RemoveNodeType.O365GroupSites;
            }
            else
            {
                return RemoveNodeType.SiteCollection;
            }
        }

        protected override NodeLevel ConvertNodeLevel(RMCompatibleRemoteNode node)
        {
            switch (node.NodeType)
            {
                case Cloud.Sdk.Data.AosModern.RemoteNodeType.SiteCollection:
                    return NodeLevel.SiteCollection;
                case Cloud.Sdk.Data.AosModern.RemoteNodeType.OneDrive:
                    return NodeLevel.SkyDrivePro;
                case Cloud.Sdk.Data.AosModern.RemoteNodeType.Office365Group:
                    return NodeLevel.O365GroupSites;
                default:
                    throw new ArgumentOutOfRangeException("Current node type {0} not supported sc sync.");
            }
        }

        private NodeLevel ConvertRemoveNodeType2NodeLevel(RemoteSiteCollection node)
        {
            switch (node.NodeType)
            {
                case RemoveNodeType.SiteCollection:
                    return NodeLevel.SiteCollection;
                case RemoveNodeType.SkyDrivePro:
                    return NodeLevel.SkyDrivePro;
                case RemoveNodeType.O365GroupSites:
                    return NodeLevel.O365GroupSites;
                case RemoveNodeType.PrivateChannel:
                    return node.ChannelType == TeamsChannelType.Private? NodeLevel.PrivateChannel: NodeLevel.SharedChannel;
                default:
                    throw new ArgumentOutOfRangeException("Current node type {0} not supported sc sync.");
            }
        }

        private NodeLevel ConvertGroupNodeLevel(Cloud.Sdk.Data.AosModern.RemoteNodeType nodeType)
        {
            switch (nodeType)
            {
                case Cloud.Sdk.Data.AosModern.RemoteNodeType.SiteCollection:
                    return NodeLevel.WebApplication;
                case Cloud.Sdk.Data.AosModern.RemoteNodeType.OneDrive:
                    return NodeLevel.SkyDriveProGroup;
                case Cloud.Sdk.Data.AosModern.RemoteNodeType.Office365Group:
                    return NodeLevel.O365GroupSitesGroup;
                default:
                    throw new ArgumentOutOfRangeException("Current node type {0} not supported sc sync.");
            }
        }
    }
}
