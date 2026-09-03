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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using System;
using System.Collections.Generic;
using AOS = Cloud.Sdk.Data.Aos;
using CA = AvePoint.GCommon.Contract.CentralAdmin.Object;
using System.Linq;
using AvePoint.RA.Common.Encryption;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Service.Services.Tenant.Notification.Excutor;
using AvePoint.RA.Service.Services.Tenant.Notification;
using AvePoint.RA.Common.SyncNode.Compatible;
using PnP.Core.Model.SharePoint;

namespace AvePoint.RA.Service.Services.Tenant.SyncNodes
{
    public interface ISyncService
    {
        void Execute(SyncNodesSettings syncNodesSettings, List<RMCompatibleRemoteNode> aosNodes);
    }

    public abstract class AbstractSyncService<T> : ISyncService
    {
        protected SyncDataJobContext executorContext = null;
        private Dictionary<string, SyncRemoteNodePara> updateObjectsDict = new Dictionary<string, SyncRemoteNodePara>();
        private Dictionary<string, SyncRemoteNodePara> updateSecondParentIdDict = new Dictionary<string, SyncRemoteNodePara>();
        private List<string> deleteOjects = new List<string>();
        private List<string> deleteOneDriveObjects = new List<string>();
        private AveLogger logger = AveLogger.GetInstance(typeof(ISyncService));

        public AbstractSyncService(SyncDataJobContext context)
        {
            this.executorContext = context;
        }

        public void Execute(SyncNodesSettings syncNodesSettings, List<RMCompatibleRemoteNode> aosNodes)
        {
            if (aosNodes == null || aosNodes.Count == 0)
            {
                logger.Info("No nodes should be sync.");
                return;
            }
            try
            {
                InitCache(syncNodesSettings.TenantGroupId);
                var groups = new List<GroupInfo>();
                BatchExecute(aosNodes, (batchAosNodes) =>
                {
                    UpdateInternalListsForGroup(syncNodesSettings, batchAosNodes, groups);
                    UpdateInternalListsForNodeAsync(syncNodesSettings, batchAosNodes).Wait();
                });
                SyncNodesAndGroups(syncNodesSettings.TenantGroupId, updateObjectsDict, deleteOjects, deleteOneDriveObjects, updateSecondParentIdDict);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to sync. Exception is {0}.", ex.ToString());
                throw;
            }
        }

        #region GetNodesDict
        protected abstract Dictionary<string, SyncRemoteNodePara> GetNodesCache(string tenantGroupId, List<string> aosSyncNodes);
        protected abstract List<T> GetNodesFromDBByUrls(List<string> urls);
        protected abstract SyncRemoteNodePara ConvertDaoNodeModelToCacheModel(T node);
        protected abstract string FieldKeySelector(T node);
        protected abstract void AddNodesToCache(string tenantGroupId, Dictionary<string, SyncRemoteNodePara> newNodesDict);

        public Dictionary<string, SyncRemoteNodePara> GetNodesFromCacheInternal(string tenantGroupId, List<RMCompatibleRemoteNode> aosSyncNodes)
        {
            List<string> urls = aosSyncNodes.Select(s => s.Url.ToLower()).ToList();
            logger.Info("There are {0} AOS nodes.", urls.Count);
            Dictionary<string, SyncRemoteNodePara> urlToCacheNodesDict = GetNodesCache(tenantGroupId, urls);
            if (urlToCacheNodesDict.Count == 0)
            {
                logger.Info("Nodes can't be find in the cache.");
                List<T> nodes = GetNodesFromDBByUrls(urls);
                if (nodes != null && nodes.Count > 0)
                {
                    logger.Info("{0} urls exist in database.", nodes.Count);
                    var dict = new Dictionary<string, SyncRemoteNodePara>();
                    foreach (T node in nodes)
                    {
                        string key = FieldKeySelector(node);
                        if (!dict.ContainsKey(key))
                        {
                            dict.Add(key, ConvertDaoNodeModelToCacheModel(node));
                        }
                    }
                    AddNodesToCache(tenantGroupId, dict);
                    urlToCacheNodesDict = dict;
                }
            }
            else
            {
                // 有些节点在Cache中不存在，但是在DB中存在，将它们更新到Cache中
                List<string> notExistedUrlsInCache = urlToCacheNodesDict.Where(p => p.Value == null).Select(s => s.Key).ToList();
                if (notExistedUrlsInCache.Count == 0)
                {
                    return urlToCacheNodesDict;
                }
                logger.Info("{0} urls don't exist in cache.", notExistedUrlsInCache.Count);
                List<T> nodesInDB = GetNodesFromDBByUrls(notExistedUrlsInCache);
                logger.Info("{0} urls exist in database.", nodesInDB.Count);
                if (nodesInDB != null && nodesInDB.Count > 0)
                {
                    var dict = new Dictionary<string, SyncRemoteNodePara>();
                    foreach (T node in nodesInDB)
                    {
                        var cacheNode = ConvertDaoNodeModelToCacheModel(node);
                        string fieldKey = FieldKeySelector(node);
                        if (!dict.ContainsKey(fieldKey))
                        {
                            dict.Add(fieldKey, cacheNode);
                        }
                        if (urlToCacheNodesDict.ContainsKey(fieldKey))
                        {
                            urlToCacheNodesDict[fieldKey] = cacheNode;
                        }
                    }
                    AddNodesToCache(tenantGroupId, dict);
                }
            }
            return urlToCacheNodesDict;
        }
        #endregion

        #region GetGroupCache
        protected abstract Dictionary<string, RemoteNodePara> GetGroupsCache(string tenantGroupId, List<RMCompatibleRemoteNode> aosNodes);
        protected abstract string GetGroupFieldKey(RMCompatibleRemoteNode aosNode);
        protected abstract RemoteNodePara GetGroupCacheByNameAndNodeLevel(string parentName, RMCompatibleRemoteNode aosNode);

        protected abstract void AddGroupsToCache(string tenantGroupId, Dictionary<string, RemoteNodePara> newGroupsCache);
        protected abstract RemoteNodePara GetGroupFromDB(string groupFieldKey);

        private Dictionary<string, RemoteNodePara> GetGroupsFromCache(string tenantGroupId, List<RMCompatibleRemoteNode> aosNodes)
        {
            Dictionary<string, RemoteNodePara> result = GetGroupsCache(tenantGroupId, aosNodes);
            if (result == null || result.Count == 0)
            {
                logger.Error("Group nodes can't be find in the cache.");
                var newGroupCacheDict = new Dictionary<string, RemoteNodePara>();
                aosNodes.ForEach(aosNode =>
                {
                    string groupFieldKey = GetGroupFieldKey(aosNode);
                    if (!newGroupCacheDict.ContainsKey(groupFieldKey))
                    {
                        RemoteNodePara newGroup = GetGroupCacheByNameAndNodeLevel(aosNode.ParentName, aosNode);
                        if (newGroup != null)
                        {
                            newGroupCacheDict.Add(groupFieldKey, newGroup);
                        }
                    }
                });
                if (newGroupCacheDict.Count > 0)
                {
                    AddGroupsToCache(tenantGroupId, newGroupCacheDict);
                    result = newGroupCacheDict;
                }
            }
            else
            {
                List<string> notExistedRemoteNodeGroupStrs = result.Where(r => r.Value == null).Select(r => r.Key).ToList();
                if (notExistedRemoteNodeGroupStrs.Count == 0)
                {
                    return result;
                }
                var missingGroupsCache = new Dictionary<string, RemoteNodePara>();
                foreach (string notExistedGroupFieldKeyStr in notExistedRemoteNodeGroupStrs)
                {
                    RemoteNodePara groupInDB = GetGroupFromDB(notExistedGroupFieldKeyStr);
                    if (groupInDB != null)
                    {
                        result[notExistedGroupFieldKeyStr] = groupInDB;
                        missingGroupsCache.Add(notExistedGroupFieldKeyStr, groupInDB);
                    }
                }
                if (missingGroupsCache.Count > 0)
                {
                    AddGroupsToCache(tenantGroupId, missingGroupsCache);
                }
            }
            return result;
        }
        #endregion

        private async System.Threading.Tasks.Task UpdateInternalListsForNodeAsync(SyncNodesSettings syncNodesSettings, List<RMCompatibleRemoteNode> aosSyncNodes)
        {
            if (aosSyncNodes == null || aosSyncNodes.Count == 0)
            {
                logger.Info("No AOS nodes should be handle.");
                return;
            }
            // GroupLevelNode不需要处理
            aosSyncNodes = aosSyncNodes.Where(n => !string.IsNullOrEmpty(n.Url)).ToList();
            if (aosSyncNodes.Count == 0)
            {
                logger.Warn("AOS nodes dont have site urls to be sync");
                return;
            }
            Dictionary<string, SyncRemoteNodePara> urlToCacheDict = GetNodesFromCacheInternal(syncNodesSettings.TenantGroupId, aosSyncNodes);
            var encryptedUserNameToUserNameDict = GetEncryptedUserNameToUserNameDict(urlToCacheDict);//获取旧站点username信息以便于判断是否需要更新
            foreach (var aosSyncNode in aosSyncNodes)
            {
                var aosSyncNodeUrl = aosSyncNode.Url.ToLower();
                if (!urlToCacheDict.ContainsKey(aosSyncNodeUrl))
                {
                    logger.Error("{0} should exist in dict but actually not.", aosSyncNode.Url);
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
                        logger.Info("Can not get out of policy {0} object: {1}.", aosSyncNode.NodeType, aosSyncNode.Url);
                        continue;
                    }
                    if (WhetherNodeUpdate(cachedNode, aosSyncNode, encryptedUserNameToUserNameDict))
                    {// AOS同步的节点已经存在于Reco中，但是Group发生了变化
                        if (string.IsNullOrEmpty(aosSyncNode.ParentName))
                        {// 在AOS中, Out of policy的Node（从AOS中被删除的Node）同样会同步到Reco，并在Reco中删除             
                            if (aosSyncNode.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.OneDrive)
                            {
                                deleteOneDriveObjects.Add(aosSyncNodeUrl);
                            }
                            else
                            {
                                deleteOjects.Add(aosSyncNodeUrl);
                            }
                            logger.Info("Delete out of policy {0} object: {1}.", aosSyncNode.NodeType, aosSyncNode.Url);
                        }
                        else
                        {
                            SyncRemoteNodePara newUpdateNode = new SyncRemoteNodePara();
                            if (IsBothTeamSiteAndGroupTeamSite(cachedNode, aosSyncNode))//当站点既是siteCollection又是Group team site时，需要进行更新secondparentid来满足GAO api获取逻辑
                            {
                                if (cachedNode.NodeLevel == NodeLevel.SiteCollection)
                                {
                                    var tempNode = ConvertRemoteNodeToCachedNode(aosSyncNode);
                                    tempNode.NodeLevel = NodeLevel.O365GroupSites;
                                    tempNode.SecondParentId = cachedNode.ParentId;
                                    newUpdateNode = tempNode;
                                }
                                else if (cachedNode.NodeLevel == NodeLevel.O365GroupSites)
                                {
                                    var tempNode = ConvertRemoteNodeToCachedNode(aosSyncNode);
                                    tempNode.NodeLevel = NodeLevel.O365GroupSites;
                                    tempNode.ParentId = cachedNode.ParentId;
                                    tempNode.SecondParentId = aosSyncNode.ParentId;
                                    if (cachedNode.SecondParentId != tempNode.SecondParentId && !updateSecondParentIdDict.ContainsKey(aosSyncNode.Url))
                                    {
                                        updateSecondParentIdDict.Add(aosSyncNode.Url, tempNode);
                                    }
                                    continue;
                                }
                            }
                            else
                            {
                                newUpdateNode = ConvertRemoteNodeToCachedNode(aosSyncNode);
                            }
                            if (!updateObjectsDict.ContainsKey(newUpdateNode.NodeName))
                            {
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
        }

        private Dictionary<string, string> GetEncryptedUserNameToUserNameDict(Dictionary<string, SyncRemoteNodePara> urlToCacheDict)
        {
            var encryptedUserNameToUserNameDict = new Dictionary<string, string>();
            foreach (var pair in urlToCacheDict)
            {
                SyncRemoteNodePara cachedNode = pair.Value;
                if (cachedNode == null || string.IsNullOrEmpty(cachedNode.UserName)
                    || encryptedUserNameToUserNameDict.ContainsKey(cachedNode.UserName)
                    || cachedNode.AuthType == CA.BposConnectionType.ServiceAccount)
                {
                    continue;
                }
                else
                {
                    var decryptedUserName = string.Empty;
                    try
                    {
                        decryptedUserName = RMDatabaseDefaultEncryptor.DecryptToString(cachedNode.UserName);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"GetEncryptedUserNameToUserNameDict:Failed to decrypt {pair.Key}, exception is {ex.ToString()}");
                    }
                    finally
                    {
                        encryptedUserNameToUserNameDict.Add(cachedNode.UserName, decryptedUserName);
                    }
                }
            }
            return encryptedUserNameToUserNameDict;
        }

        private void UpdateInternalListsForGroup(SyncNodesSettings syncNodesSettings, List<RMCompatibleRemoteNode> aosSyncNodes, List<GroupInfo> groups)
        {
            var tenantGroupId = syncNodesSettings.TenantGroupId;
            Dictionary<string, RemoteNodePara> groupsCacheDict = GetGroupsFromCache(tenantGroupId, aosSyncNodes);
            logger.Info("groups keys is {0}", string.Join(",", groupsCacheDict.Keys));
            foreach (var aosSyncNode in aosSyncNodes)
            {
                //if (!string.IsNullOrEmpty(aosSyncNode.ParentName))
                var groupAosId = aosSyncNode.ParentId;
                if (!string.IsNullOrEmpty(aosSyncNode.ParentId))
                {
                    //var group = groups.Find(g => { return g.Name == aosSyncNode.ParentName && g.Type == aosSyncNode.NodeType; });
                    var group = groups.Find(g => { return g.AosId == aosSyncNode.ParentId && g.Type == aosSyncNode.NodeType; });
                    if (group == null)
                    {
                        var groupKey = GetGroupFieldKey(aosSyncNode);
                        RemoteNodePara cachedGroup = null;
                        if (groupsCacheDict.TryGetValue(groupKey, out cachedGroup) && cachedGroup != null)
                        {
                            aosSyncNode.ParentId = cachedGroup.NodeId;
                            AddToListOfUpdateGroup(aosSyncNode, cachedGroup);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(aosSyncNode.ParentId))
                            {
                                aosSyncNode.ParentId = Guid.NewGuid().ToString();
                            }
                            AddToListOfNewGroup(aosSyncNode);
                        }
                        groups.Add(new GroupInfo()
                        {
                            Name = aosSyncNode.ParentName,
                            Type = aosSyncNode.NodeType,
                            Id = aosSyncNode.ParentId,
                            AosId = groupAosId,
                        });
                    }
                    else
                    {
                        aosSyncNode.ParentId = group.Id;
                    }
                }
            }
        }

        public void BatchExecute(List<RMCompatibleRemoteNode> collection, Action<List<RMCompatibleRemoteNode>> batchAction, int batch = 100)
        {
            if (collection == null || collection.Count == 0)
            {
                return;
            }
            var total = collection.Count;
            var iteration = (total - 1) / batch + 1;
            for (int i = 0; i < iteration; i++)
            {
                var source = collection.Skip(i * batch).Take(batch).ToList();
                batchAction(source);
            }
        }



        public bool IsNewNode(RMCompatibleRemoteNode aosSyncNode, SyncRemoteNodePara cachedNode)
        {
            return (cachedNode == null) &&
                    !string.IsNullOrEmpty(aosSyncNode.Url) &&
                    !string.IsNullOrEmpty(aosSyncNode.ParentId);
        }

        public SyncRemoteNodePara ConvertRemoteNodeToCachedNode(RMCompatibleRemoteNode aosSyncNode, bool isPrivateChannel = false)
        {
            return new SyncRemoteNodePara()
            {
                NodeName = aosSyncNode.Url,
                ParentId = aosSyncNode.ParentId,
                ParentName = aosSyncNode.ParentName,
                NodeLevel = ConvertNodeLevel(aosSyncNode),
                RelatedName = aosSyncNode.Name ?? string.Empty,
                AuthType = (CA.BposConnectionType)aosSyncNode.ConnectionType,
                AppType = ConvertIdentityTypeToAppType(aosSyncNode.AppProfileType),
                ServiceAccountId = GetServiceAccountId(aosSyncNode),
                ScanSource = RemoteNodeScanSource.AOS,
                TenantId = aosSyncNode.TenantId,
                UserName = GetUserName(aosSyncNode),
                TeamId = (isPrivateChannel ? aosSyncNode.ParentId : aosSyncNode.ExternalId) ?? string.Empty,
                SecondParentId = string.Empty,
                ObjectId = aosSyncNode.ObjectId
            };
        }

        protected string EncryUserName(string userName)
        {
            return string.IsNullOrEmpty(userName) ? string.Empty : RMDatabaseDefaultEncryptor.EncryptToString(userName);
        }

        protected void LogSyncDataInfo(List<string> names)
        {
            if (names == null || names.Count == 0)
            {
                return;
            }
            logger.Info("They are :");
            names.ForEach(name =>
            {
                logger.Info(name);
            });
        }

        #region 判断是否应该更新节点
        private bool WhetherNodeUpdate(SyncRemoteNodePara cachedNode, RMCompatibleRemoteNode syncNode, Dictionary<string, string> userNameDict)
        {
            return IsScanSourceChanged(cachedNode) ||
                IsGroupChanged(cachedNode, syncNode) ||
                IsTenantIdChanged(cachedNode, syncNode) ||
                IsServiceAccountIdChanged(cachedNode, syncNode) ||
                //IsUserNameChanged(cachedNode, syncNode, userNameDict) ||
                IsAuthTypeChanged(cachedNode, syncNode) ||
                IsAppTypeChanged(cachedNode, syncNode);
                //IsTeamIdChanged(cachedNode, syncNode);
        }

        private bool IsScanSourceChanged(SyncRemoteNodePara cachedNode)
        {
            return cachedNode.ScanSource != RemoteNodeScanSource.AOS;
        }

        private bool IsGroupChanged(SyncRemoteNodePara cachedNode, RMCompatibleRemoteNode syncNode)
        {
            cachedNode.ParentId = cachedNode.ParentId ?? string.Empty;
            syncNode.ParentId = syncNode.ParentId ?? string.Empty;
            logger.Debug($"[{syncNode.Url}] is group changed: [{!cachedNode.ParentId.Equals(syncNode.ParentId, StringComparison.Ordinal)}]");
            return !cachedNode.ParentId.Equals(syncNode.ParentId, StringComparison.Ordinal);
        }

        public bool IsTenantIdChanged(SyncRemoteNodePara cachedNode, RMCompatibleRemoteNode syncNode)
        {
            cachedNode.TenantId = cachedNode.TenantId ?? string.Empty;
            syncNode.TenantId = syncNode.TenantId ?? string.Empty;
            logger.Debug($"[{syncNode.Url}] is tenant id changed: [{!syncNode.TenantId.Equals(cachedNode.TenantId, StringComparison.Ordinal)}]");
            return !syncNode.TenantId.Equals(cachedNode.TenantId, StringComparison.Ordinal);
        }

        public bool IsServiceAccountIdChanged(SyncRemoteNodePara cachedNode, RMCompatibleRemoteNode syncNode)
        {
            string syncNodeServiceAccountId = GetServiceAccountId(syncNode);
            cachedNode.ServiceAccountId = cachedNode.ServiceAccountId ?? string.Empty;
            logger.Debug($"[{syncNode.Url}] is service account id changed: [{!cachedNode.ServiceAccountId.Equals(syncNodeServiceAccountId, StringComparison.OrdinalIgnoreCase)}]");
            return !cachedNode.ServiceAccountId.Equals(syncNodeServiceAccountId, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsAuthTypeChanged(SyncRemoteNodePara cachedNode, RMCompatibleRemoteNode syncNode)
        {
            logger.Debug($"[{syncNode.Url}] is auth type changed: [{cachedNode.AuthType != ConvertAOSAuthTypeToDAOConnectType(syncNode.ConnectionType)}]");
            return cachedNode.AuthType != ConvertAOSAuthTypeToDAOConnectType(syncNode.ConnectionType);
        }

        public bool IsAppTypeChanged(SyncRemoteNodePara cachedNode, RMCompatibleRemoteNode syncNode)
        {
            bool isChanged = false;
            if ((cachedNode.AuthType == CA.BposConnectionType.AppToken || cachedNode.AuthType == CA.BposConnectionType.Modern) &&
                (syncNode.ConnectionType == Cloud.Sdk.Data.AosModern.ConnectionType.AppToken || syncNode.ConnectionType == Cloud.Sdk.Data.AosModern.ConnectionType.Modern))
            {
                if ((int)cachedNode.AppType != (int)ConvertIdentityTypeToAppType(syncNode.AppProfileType))
                {
                    logger.Info("AppType of mailbox {0} changed, from {1} to {2}", cachedNode.NodeName, cachedNode.AppType.ToString(), ConvertIdentityTypeToAppType(syncNode.AppProfileType).ToString());
                    isChanged = true;
                }
            }

            logger.Debug($"[{syncNode.Url}] is app type changed: [{isChanged}]");
            return isChanged;
        }

        private CA.BposConnectionType ConvertAOSAuthTypeToDAOConnectType(Cloud.Sdk.Data.AosModern.ConnectionType aosConnectType)
        {
            CA.BposConnectionType daoConnectType = CA.BposConnectionType.AppToken;
            switch (aosConnectType)
            {
                case Cloud.Sdk.Data.AosModern.ConnectionType.AppToken:
                    daoConnectType = CA.BposConnectionType.AppToken;
                    break;
                case Cloud.Sdk.Data.AosModern.ConnectionType.ServiceAccount:
                    daoConnectType = CA.BposConnectionType.ServiceAccount;
                    break;
                case Cloud.Sdk.Data.AosModern.ConnectionType.Modern:
                    daoConnectType = CA.BposConnectionType.Modern;
                    break;
            }
            return daoConnectType;
        }
        #endregion

        #region 判断站点是否既是普通SiteCollection又是Group Team Site

        private bool IsBothTeamSiteAndGroupTeamSite(SyncRemoteNodePara cachedNode, RMCompatibleRemoteNode syncNode)
        {
            return (cachedNode.NodeLevel == NodeLevel.SiteCollection && (syncNode.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.Office365Group))
                || (cachedNode.NodeLevel == NodeLevel.O365GroupSites && syncNode.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.SiteCollection);
        }

        #endregion

        protected abstract void SyncNodesAndGroups(string tenantGroupId, Dictionary<string, SyncRemoteNodePara> updateObjectsDict, List<string> deleteOjects, List<string> deleteOneDriveObjects, Dictionary<string, SyncRemoteNodePara> updateSecondParentIdDict);
        protected abstract bool CheckExitedGroup(RMCompatibleRemoteNode node, RemoteNodePara group);
        protected abstract void AddToListOfNewGroup(RMCompatibleRemoteNode node);

        protected abstract void AddToListOfUpdateGroup(RMCompatibleRemoteNode node, RemoteNodePara existGroup);

        protected abstract System.Threading.Tasks.Task AddToListsForNodesAsync(RMCompatibleRemoteNode node);
        protected abstract string GetServiceAccountId(RMCompatibleRemoteNode node);
        protected abstract string GetUserName(RMCompatibleRemoteNode node);
        protected abstract NodeLevel ConvertNodeLevel(RMCompatibleRemoteNode node);

        #region Cache
        protected abstract void InitCache(string tenantGroupId);
        #endregion

        protected CA.AppType ConvertIdentityTypeToAppType(Cloud.Sdk.Data.AosModern.IdentityProviderType providerType)
        {
            var appType = CA.AppType.Office365;
            switch (providerType)
            {
                case Cloud.Sdk.Data.AosModern.IdentityProviderType.CloudRecords:
                    appType = CA.AppType.CloudRecords;
                    break;
                case Cloud.Sdk.Data.AosModern.IdentityProviderType.Office365:
                    appType = CA.AppType.Office365;
                    break;
                case Cloud.Sdk.Data.AosModern.IdentityProviderType.SharePoint:
                    appType = CA.AppType.SharePoint;
                    break;
                case Cloud.Sdk.Data.AosModern.IdentityProviderType.Exchange:
                    appType = CA.AppType.Exchange;
                    break;
                case Cloud.Sdk.Data.AosModern.IdentityProviderType.CustomAzureApp:
                    appType = CA.AppType.CustomAzureApp;
                    break;
                case Cloud.Sdk.Data.AosModern.IdentityProviderType.CustomDelegateApp:
                    appType = CA.AppType.CustomDelegateApp;
                    break;
            }
            return appType;
        }

        protected class GroupInfo
        {
            public string Name;
            public string Id;
            public Cloud.Sdk.Data.AosModern.RemoteNodeType Type;
            public string AosId;
        }
    }
}

