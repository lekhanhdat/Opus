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
using AvePoint.GCommon.Contract.Server.CreateContainer.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.SyncNode;
using AvePoint.RA.SharePoint.Common;
using Cloud.Sdk.AosModern;
using Cloud.Sdk.Data.AosModern;
using Cloud.Sdk.Data.Cop.Insights;
using RMSynchronize.SyncNodeFromAOS.ChangeLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Retrying;

namespace RMSynchronize.SyncNodeFromAOS.Executors
{
    public abstract class RMSyncSiteNodeExecutor : RMSyncNodeExecutor
    {
        private const int PAGE_SIZE = 10_000;

        protected abstract IEnumerable<RMSiteNodeAdaption> ConvertAosNodesToAdaption(RMContainerInfoAdaption containerInfo, RemoteNodesResult queryResult);
        protected HashSet<string> ExistObjectId = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public RMSyncSiteNodeExecutor(AosModernApiTenantClient tenantClient, List<TenantConnectionInfo> tenantConnectionInfoes, RMSyncNodeAzureChangeLogger changeLogger) 
            : base(tenantClient, tenantConnectionInfoes, changeLogger)
        {
        }

        protected override async Task<IEnumerable<RMContainerInfoAdaption>> GetRecordContainers()
        {
            return await s_syncNodeDao.GetSiteContainersAsync(RecordContainerNodeLevel);
        }

        protected override Task AddContainers(IEnumerable<RMContainerInfoAdaption> containerInfoes)
        {
            return s_syncNodeDao.AddSiteContainersAsync(containerInfoes);
        }

        protected override Task DeleteContainers(IEnumerable<RMContainerInfoAdaption> containerInfoes)
        {
            return s_syncNodeDao.DeleteSiteContainersAsync(containerInfoes);
        }

        protected override Task UpdateContainers(IEnumerable<RMContainerInfoAdaption> containerInfoes)
        {
            return s_syncNodeDao.UpdateSiteContainerAsync(containerInfoes);
        }

        protected override async Task SyncNodeAsync(RMContainerInfoAdaption containerInfo)
        {
            foreach(var tenantConnectionInfo in _tenantConnectionInfoes)
            {
                try
                {
                    if (ContentSource == SourceFlag.Teams)
                    {
                        _logger.Warn($"The content source [{ContentSource}] is supported.");
                    }
                    _logger.Info($"Start sync [O365 Tenant: {tenantConnectionInfo.Id} - {tenantConnectionInfo.Name} - container aos id: {containerInfo.AosId} - container name: {containerInfo.Name}] site nodes.");

                    var aosNodesTask = QueryAosNodesAsync(tenantConnectionInfo, containerInfo);
                    
                    var recordNodesTask = GetRecordNodes(containerInfo, tenantConnectionInfo);

                    await Task.WhenAll(aosNodesTask, recordNodesTask);
                    
                    var aosNodes = aosNodesTask.Result.Where(item => !string.IsNullOrWhiteSpace(item.ObjectId) && !string.IsNullOrWhiteSpace(item.Url)).ToDictionary(node => node.ObjectId, node => node, StringComparer.OrdinalIgnoreCase);
                
                    var recordNodes = recordNodesTask.Result;

                    _logger.Info($" container aos id: {containerInfo.AosId} - container name: {containerInfo.Name}: current aos nodes count: {aosNodes.Count}, db nodes count: {recordNodes.Count}");

                    var needDeleteNodes = recordNodes.Except(aosNodes.Values).ToHashSet();

                    if (this.ContentSource == SourceFlag.Teams && this.AosNodeType == RemoteNodeType.Office365Group)
                    {
                        foreach (var aosNode in aosNodes.Values)
                        {
                            RMSyncNodeJobManager.CacheTeamsNodes.Add(aosNode);
                        }
                        foreach (var deleteNode in needDeleteNodes)
                        {
                            RMSyncNodeJobManager.CacheTeamsNodes.Add(deleteNode);
                        }
                    }

                    _logger.Debug($"Need delete node count: [{needDeleteNodes.Count}].");
                    await s_syncNodeDao.DeleteSiteNodesAsync(needDeleteNodes);
                    RMSyncNodeJobManager.AddSucceedJobDetail(ContentSource, RMSyncNodeAction.Delete, containerInfo, needDeleteNodes);
                    needDeleteNodes.ForEach(item => item.ContainerName = containerInfo.Name);
                    // not sync permission of orphane onedrive
                    await _changeLogger.Record(needDeleteNodes.Where(node => !string.IsNullOrEmpty(node.Name)), ContentSource, RMSyncNodeChangeType.Delete);

                    var needAddNodes = aosNodes.Values.Except(recordNodes).ToHashSet();           

                    _logger.Debug($"Need add node count: [{needAddNodes.Count}].");
                    await s_syncNodeDao.AddSiteNodesAsync(needAddNodes);
                    RMSyncNodeJobManager.AddSucceedJobDetail(ContentSource, RMSyncNodeAction.Add, containerInfo, needAddNodes);
                    // not sync permission of orphane onedrive
                    await _changeLogger.Record(needAddNodes.Where(node => !string.IsNullOrEmpty(node.Name)), ContentSource, RMSyncNodeChangeType.Add);

                    var intersectNodes = recordNodes.Intersect(aosNodes.Values).ToHashSet();

                    var needUpdateNodes = await intersectNodes.ToAsyncEnumerable().WhereAwait(async intersectNode =>
                    {
                        var hasChange = false;

                        var existNode = aosNodes[intersectNode.ObjectId];

                        if (intersectNode.Name != existNode.Name)
                        {
                            _logger.Info($"The object [O365 tenant: {intersectNode.TenantId} - Object id: {intersectNode.ObjectId}] has been changed name.");
                            hasChange = true;
                        }

                        if(intersectNode.Url != existNode.Url)
                        {
                            _logger.Info($"The object [O365 tenant: {intersectNode.TenantId} - Object id: {intersectNode.ObjectId}] has been changed url.");
                            await _changeLogger.RecordChangeName(intersectNode, ContentSource, intersectNode.Name, existNode.Name);
                            hasChange = true;
                        }

                        if(intersectNode.AppType != existNode.AppType)
                        {
                            _logger.Info($"The object [O365 tenant: {intersectNode.TenantId} - Object id: {intersectNode.ObjectId}] has been changed app type.");
                            hasChange = true;
                        }

                        if(intersectNode.ConnectionType != existNode.ConnectionType)
                        {
                            _logger.Info($"The object [O365 tenant: {intersectNode.TenantId} - Object id: {intersectNode.ObjectId}] has been changed connection type.");
                            hasChange = true;
                        }

                        if(string.IsNullOrWhiteSpace(intersectNode.UserName) || !intersectNode.UserName.Equals(existNode.UserName, StringComparison.OrdinalIgnoreCase))
                        {
                            if(!string.IsNullOrWhiteSpace(existNode.UserName))
                            {
                                _logger.Info($"The object [O365 tenant: {intersectNode.TenantId} - Object id: {intersectNode.ObjectId}] has been changed user name.");
                                hasChange = true;
                            }
                        }

                        if (existNode.SiteCollectionType == AvePoint.GCommon.Contract.SharePointBrowser.SiteCollectionType.Teams 
                            || existNode.SiteCollectionType == AvePoint.GCommon.Contract.SharePointBrowser.SiteCollectionType.Group
                            || existNode.SiteCollectionType == AvePoint.GCommon.Contract.SharePointBrowser.SiteCollectionType.PrivateChannel)
                        {
                            if ((string.IsNullOrWhiteSpace(intersectNode.TeamId) || !intersectNode.TeamId.Equals(existNode.TeamId, StringComparison.OrdinalIgnoreCase)) 
                                && !string.IsNullOrWhiteSpace(existNode.TeamId))
                            {
                                _logger.Info($"The object [O365 tenant: {intersectNode.TenantId} - Object id: {intersectNode.ObjectId}] has been changed team id.");
                                hasChange = true;
                            }

                            if (intersectNode.SiteCollectionType != existNode.SiteCollectionType)
                            {
                                _logger.Info($"The object [O365 tenant: {intersectNode.TenantId} - Object id: {intersectNode.ObjectId}] has been changed site collection type.");
                                hasChange = true;
                            }

                            if ((string.IsNullOrWhiteSpace(intersectNode.DisplayName) || !intersectNode.DisplayName.Equals(existNode.DisplayName, StringComparison.OrdinalIgnoreCase)) 
                                && !string.IsNullOrWhiteSpace(existNode.DisplayName))
                            {
                                _logger.Info($"The object [O365 tenant: {intersectNode.TenantId} - Object id: {intersectNode.ObjectId}] has been changed display name.");
                                hasChange = true;
                            }
                        }

                        intersectNode.Name = existNode.Name;
                        intersectNode.Url = existNode.Url;
                        intersectNode.AppType = existNode.AppType;
                        intersectNode.ConnectionType = existNode.ConnectionType;
                        intersectNode.UserName = existNode.UserName;
                        intersectNode.TeamId = existNode.TeamId;
                        intersectNode.SiteCollectionType = existNode.SiteCollectionType;
                        intersectNode.DisplayName = existNode.DisplayName;

                        return hasChange;
                    }).ToHashSetAsync();
                    _logger.Debug($"Need update node count: [{needUpdateNodes.Count}].");
                    await s_syncNodeDao.UpdateSiteNodesAsync(needUpdateNodes);
                    RMSyncNodeJobManager.AddSucceedJobDetail(ContentSource, RMSyncNodeAction.Update, containerInfo, needUpdateNodes);

                    _logger.Info($"Successful sync [O365 Tenant: {tenantConnectionInfo.Id} - {tenantConnectionInfo.Name} - container aos id: {containerInfo.AosId} - container name: {containerInfo.Name}] site nodes.");
                }
                catch(Exception e)
                {
                    _logger.Error($"An error occurred while sync [O365 Tenant: {tenantConnectionInfo.Id} - {tenantConnectionInfo.Name} - container aos id: {containerInfo.AosId} - container name: {containerInfo.Name}] site nodes. Error: {e}");
                    RMSyncNodeJobManager.AddFailedJobDetail(ContentSource, RMSyncNodeAction.None, [containerInfo], e);
                }
            }
        }

        protected override async Task UpgradeNodeAsync(RMContainerInfoAdaption containerInfo)
        {
            foreach (var tenantConnectionInfo in _tenantConnectionInfoes)
            {
                try
                {
                    _logger.Info($"Start upgrade [O365 Tenant: {tenantConnectionInfo.Id} - {tenantConnectionInfo.Name} - container aos id: {containerInfo.AosId} - container name: {containerInfo.Name}] site nodes.");

                    var aosNodesTask = QueryAosNodesAsync(tenantConnectionInfo, containerInfo);
                    
                    var recordNodesTask = GetRecordNodes(containerInfo, tenantConnectionInfo);

                    await Task.WhenAll(aosNodesTask, recordNodesTask);
                    
                    var aosNodes = aosNodesTask.Result.Where(item => !string.IsNullOrWhiteSpace(item.ObjectId) && !string.IsNullOrWhiteSpace(item.Url)).ToHashSet();
                
                    var recordNodes = recordNodesTask.Result;

                    if (this.ContentSource == SourceFlag.Teams && this.AosNodeType == RemoteNodeType.Office365Group)
                    {
                        foreach (var aosNode in aosNodes)
                        {
                            RMSyncNodeJobManager.CacheTeamsNodes.Add(aosNode);
                        }
                    }

                    var needUpgradeNodes = new List<RMSiteNodeAdaption>();

                    recordNodes.ForEach(node =>
                    {
                        var needUpgradeNode = aosNodes.FirstOrDefault(aNode => aNode.Url.Equals(node.Url, StringComparison.OrdinalIgnoreCase));
                        if (needUpgradeNode != null && !node.ObjectId.Equals(needUpgradeNode.ObjectId))
                        {
                            node.ObjectId = needUpgradeNode.ObjectId;
                            needUpgradeNodes.Add(node);
                        }
                    });

                    if (needUpgradeNodes.Any())
                    {
                        await s_syncNodeDao.UpdateSiteNodesAsync(needUpgradeNodes);

                        RMSyncNodeJobManager.AddSucceedJobDetail(ContentSource, RMSyncNodeAction.Upgrade, containerInfo, needUpgradeNodes);
                    }

                    _logger.Info($"Successful upgrade [O365 Tenant: {tenantConnectionInfo.Id} - {tenantConnectionInfo.Name} - container aos id: {containerInfo.AosId} - container name: {containerInfo.Name}] site nodes.");
                }
                catch (Exception e)
                {
                    _logger.Error($"An error occurred while sync [O365 Tenant: {tenantConnectionInfo.Id} - {tenantConnectionInfo.Name} - container aos id: {containerInfo.AosId} - container name: {containerInfo.Name}] site nodes. Error: {e}");
                    RMSyncNodeJobManager.AddFailedJobDetail(ContentSource, RMSyncNodeAction.None, [containerInfo], e);
                }
            }
        }
        
        private async Task<List<RMSiteNodeAdaption>> QueryAosNodesAsync(TenantConnectionInfo tenantConnectionInfo, RMContainerInfoAdaption containerInfo)
        {
            using var performance = new PerformanceScope("Query AOS Nodes");
            List<RMSiteNodeAdaption> aosNodes = [];

            var pageIndex = 1;

            var emptyIdNodes = new List<string>();
            var emptyUrlNodes = new List<string>();
            ExistObjectId.Clear();

            while (true)
            {
                var queryResult = await GetRemoteNodesAsync(tenantConnectionInfo, containerInfo, pageIndex);
                var rawNodeCount = GetRawNodeCount(queryResult);
                var convertedNodes = ConvertAosNodesToAdaption(containerInfo, queryResult).ToList();

                _logger.Info($"AOS page fetched [Tenant: {tenantConnectionInfo.Id} - {tenantConnectionInfo.Name} - container aos id: {containerInfo.AosId} - container name: {containerInfo.Name} - page index: {pageIndex}] raw node count: {rawNodeCount}, converted node count: {convertedNodes.Count}.");

                aosNodes.AddRange(convertedNodes);
                emptyIdNodes.AddRange(convertedNodes.Where(node => string.IsNullOrWhiteSpace(node.ObjectId)).Select(node => node.Url));
                emptyUrlNodes.AddRange(convertedNodes.Where(node => string.IsNullOrWhiteSpace(node.Url)).Select(node => node.Id));
                if (rawNodeCount < PAGE_SIZE)
                {
                    break;
                }

                pageIndex++;
            }

            if(emptyIdNodes.Count > 0 || emptyUrlNodes.Count > 0)
            {
                _logger.Warn($"The container AOS Id: [{containerInfo.AosId}], Name: [{containerInfo.Name}], object id empty nodes [{string.Join(", ", emptyIdNodes)}], url empty nodes [{string.Join(", ", emptyUrlNodes)}]");
            }

            return aosNodes;
        }

        private int GetRawNodeCount(RemoteNodesResult queryResult)
        {
            if (queryResult == null)
            {
                return 0;
            }

            return AosNodeType switch
            {
                RemoteNodeType.SiteCollection => queryResult.SPSites?.Count ?? 0,
                RemoteNodeType.OneDrive => queryResult.OneDrives?.Count ?? 0,
                RemoteNodeType.Office365Group => queryResult.O365Groups?.Count ?? 0,
                RemoteNodeType.Channel => queryResult.Channels?.Count ?? 0,
                _ => throw new NotSupportedException($"Unsupported aos node type [{AosNodeType}] in {nameof(GetRawNodeCount)}."),
            };
        }


        private async Task<HashSet<RMSiteNodeAdaption>> GetRecordNodes(RMContainerInfoAdaption containerInfo, TenantConnectionInfo tenantConnectionInfo)
        {
            using var performance = new PerformanceScope("Query Record Nodes");
            var allRecordNodes = s_syncNodeDao.GetSiteNodesAsync(containerInfo.Id, tenantConnectionInfo.Id);

            List<RMSiteNodeAdaption> objectIdIsNullYetRecordNodes = [];
            HashSet<RMSiteNodeAdaption> dbNodes = [];
            await foreach (var recordNode in allRecordNodes)
            {
                if(string.IsNullOrWhiteSpace(recordNode.ObjectId))
                {
                    objectIdIsNullYetRecordNodes.Add(recordNode);
                }
                else
                {
                    dbNodes.Add(recordNode);
                }
            }
        
            if (objectIdIsNullYetRecordNodes.Any())
            {
                _logger.Debug($"The count of null children node object id still null for [{containerInfo.Name}] is [{objectIdIsNullYetRecordNodes.Count}].");
                await s_syncNodeDao.DeleteSiteNodesAsync(objectIdIsNullYetRecordNodes);
            }

            return dbNodes;
        }

        protected async Task SetRealSiteIdAndName(RMSiteNodeAdaption node, RemoteNodeType nodeType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(node.ObjectId) || string.IsNullOrWhiteSpace(node.Name))
                {
                    _logger.Debug($"Current node objectId is: [{node.ObjectId}], node name is [{node.Name}].");
                    if (!string.IsNullOrWhiteSpace(node.ObjectId) && nodeType == RemoteNodeType.OneDrive)
                    {
                        _logger.Debug($"The node object id is not empty and the node type is one drive, no need to get site id.");
                        return;
                    }

                    _logger.Info($"Start get [{node.Name}] objectId id.");
                    var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(new AvePoint.GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection
                    {
                        AdminUrl = node.AdminUrl,
                        url = node.Url,
                        TenantId = node.TenantId
                    });

                    var factory = MultiAppUtil.CreateAveObjectModelFactory(node.Url, bposInfo, AvePoint.Wrapper.Common.AveContextKind.ClientObjectModel);

                    using var site = factory.CreateSite(node.Url);

                    if (string.IsNullOrWhiteSpace(node.ObjectId))
                    {
                        node.ObjectId = site.ID.ToString();
                    }

                    
                    if (string.IsNullOrWhiteSpace(node.Name) && nodeType != RemoteNodeType.OneDrive)
                    {
                        _logger.Debug($"The node name is empty and the node type is {nodeType}, need to get site name.");
                        node.Name = site.RootWeb.Title;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get [{node.Url}] site id. Error: {e}");
            }
        }
        
        private async Task<RemoteNodesResult> GetRemoteNodesAsync(TenantConnectionInfo tenantConnectionInfo, RMContainerInfoAdaption containerInfo, int pageIndex)
        {
            var retryer = RMRetryerBuilder.CreateBuilder().WithStopStrategy(new RMRetryStopAfterAttemptStrategy(5)).Build();
            return await retryer.RetryAsync(async () => await _tenantClient.RemoteNodeService.GetRemoteNodesByPageAsync(AosNodeType, tenantConnectionInfo.Id, pageIndex, PAGE_SIZE, containerId: AosNodeType == RemoteNodeType.Channel ? null : containerInfo.AosId,filter: "", isSupportMultiGeoProduct: false, orderBy: OrderBy.Id));
        }
    }
}
