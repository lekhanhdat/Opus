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

using System.Collections.Concurrent;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.SyncNode;
using AvePoint.RA.Contract.SyncNode.GoogleSyncNode;
using Cloud.Sdk.AosModern;
using Cloud.Sdk.Data.AosModern;
using RMSynchronize.SyncNodeFromAOS.ChangeLog;

namespace RMSynchronize.SyncNodeFromAOS.Executors;

public abstract class RMSyncGoogleNodeExecutor(
    AosModernApiTenantClient tenantClient,
    List<TenantConnectionInfo> tenantConnectionInfos,
    RMSyncNodeAzureChangeLogger changeLogger)
    : RMSyncNodeExecutor(tenantClient, tenantConnectionInfos, changeLogger)
{
    private const int PAGE_SIZE = 10_000;
    protected abstract IEnumerable<RMGoogleNodeAdaption> ConvertAosNodesToAdaption(RMContainerInfoAdaption containerInfo, RemoteNodesResult queryResult);
    protected override async Task SyncNodeAsync(RMContainerInfoAdaption containerInfo)
    {
        foreach (var tenantConnectionInfo in _tenantConnectionInfoes)
        {
            try
            {
                _logger.Info(
                    $"Start sync [Google Tenant: {tenantConnectionInfo.Id} - {tenantConnectionInfo.Name} - container aos id: {containerInfo.AosId} - container name: {containerInfo.Name}] nodes.");
                
                var queryAosNodesTask = QueryAosNodesAsync(tenantConnectionInfo, containerInfo);
                
                var queryDbNodesTask = GetRecordNodes(containerInfo, tenantConnectionInfo);

                await Task.WhenAll(queryAosNodesTask, queryDbNodesTask);
                
                var aosNodes = queryAosNodesTask.Result.Where(item => !string.IsNullOrWhiteSpace(item.ObjectId)).ToHashSet();
                
                var recordNodes = queryDbNodesTask.Result;

                var needDeleteNodes = recordNodes.Except(aosNodes).ToHashSet();

                await HandleNeedDeleteNodes(needDeleteNodes, containerInfo);

                var needAddNodes = aosNodes.Except(recordNodes).ToHashSet();

                await HandleNeedAddNodes(needAddNodes, containerInfo);

                var needUpdateNodes = recordNodes.Intersect(aosNodes).ToHashSet();

                await HandleNeedUpdateNodes(needUpdateNodes, aosNodes, containerInfo);

                _logger.Info(
                    $"Successful sync [Google Tenant: {tenantConnectionInfo.Id} - {tenantConnectionInfo.Name} - container aos id: {containerInfo.AosId} - container name: {containerInfo.Name}] nodes.");
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while sync [Google Tenant: {tenantConnectionInfo.Id} - {tenantConnectionInfo.Name} - container aos id: {containerInfo.AosId} - container name: {containerInfo.Name}] nodes. Error: {ex}");
                RMSyncNodeJobManager.AddFailedJobDetail(ContentSource, RMSyncNodeAction.None, [containerInfo], ex);
            }
            
        }
    }

    private async Task<List<RMGoogleNodeAdaption>> QueryAosNodesAsync(TenantConnectionInfo tenantConnectionInfo, RMContainerInfoAdaption containerInfo)
    {
        using var performance = new PerformanceScope("Query AOS Nodes");

        List<RMGoogleNodeAdaption> aosNodes = [];

        var pageIndex = 1;
        
        while (true)
        {
            var queryResult = await GetRemoteNodesAsync(tenantConnectionInfo, containerInfo, pageIndex);
            var convertedNodes = ConvertAosNodesToAdaption(containerInfo, queryResult);
            aosNodes.AddRange(convertedNodes);
            _logger.Debug($"Aos node count: [{aosNodes.Count}].");
            if (convertedNodes.Count() < PAGE_SIZE)
            {
                break;
            }

            pageIndex++;
        }

        return aosNodes;
    }

    private async Task HandleNeedUpdateNodes(HashSet<RMGoogleNodeAdaption> intersectNodes, HashSet<RMGoogleNodeAdaption> aosNodes , RMContainerInfoAdaption containerInfo)
    {
        var needUpdateNodes = await intersectNodes.ToAsyncEnumerable().WhereAwait(async intersectNode =>
                    {
                        var existNode = aosNodes.First(aosContainer => intersectNode.ObjectId.Equals(aosContainer.ObjectId, StringComparison.OrdinalIgnoreCase));

                        var isChanged = await CheckChangeProperties(intersectNode, existNode);
                        
                        intersectNode.UpdateData(existNode.Name, existNode.UserName);

                        return isChanged;
                    }).ToHashSetAsync();
        _logger.Debug($"Need update node count: [{needUpdateNodes.Count}].");
        await s_syncGoogleNodeDao.UpdateGoogleNodesAsync(needUpdateNodes);
        RMSyncNodeJobManager.AddSucceedJobDetail(ContentSource, RMSyncNodeAction.Update, containerInfo, needUpdateNodes);
    }

    private async Task<bool> CheckChangeProperties(RMGoogleNodeAdaption intersectNode, RMGoogleNodeAdaption existNode)
    {
        var hasChange = false;
        if (intersectNode.Name != existNode.Name)
        {
            _logger.Info(
                $"The object [Google tenant: {intersectNode.TenantId} - Object id: {intersectNode.ObjectId}] has been changed name.");
            await _changeLogger.RecordChangeName(intersectNode, ContentSource, intersectNode.Name,
                existNode.Name);
            hasChange = true;
        }
        return hasChange;
    }
    
    private async Task HandleNeedAddNodes(HashSet<RMGoogleNodeAdaption> needAddNodes, RMContainerInfoAdaption containerInfo)
    {
        _logger.Debug($"Need add node count: [{needAddNodes.Count}].");
        await s_syncGoogleNodeDao.AddGoogleNodesAsync(needAddNodes);
        RMSyncNodeJobManager.AddSucceedJobDetail(ContentSource, RMSyncNodeAction.Add, containerInfo, needAddNodes);
        await _changeLogger.Record(needAddNodes, ContentSource, RMSyncNodeChangeType.Add);
    }

    private async Task HandleNeedDeleteNodes(HashSet<RMGoogleNodeAdaption> needDeleteNodes, RMContainerInfoAdaption containerInfo)
    {
        _logger.Debug($"Need delete node count: [{needDeleteNodes.Count}].");
        await s_syncGoogleNodeDao.DeleteGoogleNodesAsync(needDeleteNodes);
        RMSyncNodeJobManager.AddSucceedJobDetail(ContentSource, RMSyncNodeAction.Delete, containerInfo, needDeleteNodes);
        needDeleteNodes.ForEach(item => item.ContainerName = containerInfo.Name);
        await _changeLogger.Record(needDeleteNodes, ContentSource, RMSyncNodeChangeType.Delete);
    }
    
    private async Task<HashSet<RMGoogleNodeAdaption>> GetRecordNodes(RMContainerInfoAdaption containerInfo, TenantConnectionInfo tenantConnectionInfo)
    {
        using var performance = new PerformanceScope("Query Record Nodes");

        var allRecordNodes = s_syncGoogleNodeDao.GetGoogleNodesAsync(containerInfo.Id, tenantConnectionInfo.Id);

        List<RMGoogleNodeAdaption> objectIdIsNullYetRecordNodes = [];
        HashSet<RMGoogleNodeAdaption> dbNodes = [];
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
            await s_syncGoogleNodeDao.DeleteGoogleNodesAsync(objectIdIsNullYetRecordNodes);
        }

        return dbNodes;
    }

    protected override async Task UpgradeNodeAsync(RMContainerInfoAdaption containerInfo)
    {
        foreach (var tenantConnectionInfo in _tenantConnectionInfoes)
        {
                try
                {
                    _logger.Info($"Start upgrade [Google Tenant: {tenantConnectionInfo.Id} - {tenantConnectionInfo.Name} - container aos id: {containerInfo.AosId} - container name: {containerInfo.Name}] nodes.");

                    var queryAosNodesTask = QueryAosNodesAsync(tenantConnectionInfo, containerInfo);
                
                    var queryDbNodesTask = GetRecordNodes(containerInfo, tenantConnectionInfo);

                    await Task.WhenAll(queryAosNodesTask, queryDbNodesTask);
                    
                    var aosNodes = queryAosNodesTask.Result.Where(item => !string.IsNullOrWhiteSpace(item.ObjectId)).ToHashSet();
                    
                    var recordNodes = queryDbNodesTask.Result;

                    List<RMGoogleNodeAdaption> needUpgradeNodes = [];

                    recordNodes.ForEach(node =>
                    {
                        var needUpgradeNode = aosNodes.FirstOrDefault(aNode => aNode.Name.Equals(node.Name, StringComparison.OrdinalIgnoreCase));
                        if (needUpgradeNode != null && !node.ObjectId.Equals(needUpgradeNode.ObjectId))
                        {
                            node.ObjectId = needUpgradeNode.ObjectId;
                            needUpgradeNodes.Add(node);
                        }
                    });

                    if (needUpgradeNodes.Any())
                    {
                        await s_syncGoogleNodeDao.UpdateGoogleNodesAsync(needUpgradeNodes);

                        RMSyncNodeJobManager.AddSucceedJobDetail(ContentSource, RMSyncNodeAction.Upgrade, containerInfo, needUpgradeNodes);
                    }

                    _logger.Info($"Successful upgrade [Google Tenant: {tenantConnectionInfo.Id} - {tenantConnectionInfo.Name} - container aos id: {containerInfo.AosId} - container name: {containerInfo.Name}] site nodes.");
                }
                catch (Exception e)
                {
                    _logger.Error($"An error occurred while sync [Google Tenant: {tenantConnectionInfo.Id} - {tenantConnectionInfo.Name} - container aos id: {containerInfo.AosId} - container name: {containerInfo.Name}] site nodes. Error: {e}");
                    RMSyncNodeJobManager.AddFailedJobDetail(ContentSource, RMSyncNodeAction.None, [containerInfo], e);
                }
        }
    }

    private async Task<RemoteNodesResult> GetRemoteNodesAsync(TenantConnectionInfo tenantConnectionInfo, RMContainerInfoAdaption containerInfo, int pageIndex)
    {
        var retryer = RMRetryerBuilder.CreateBuilder().WithStopStrategy(new RMRetryStopAfterAttemptStrategy(5)).Build();
        return await retryer.RetryAsync(async () => await _tenantClient.RemoteNodeService.GetRemoteNodesByPageAsync(AosNodeType, tenantConnectionInfo.Id, pageIndex, PAGE_SIZE, containerId: containerInfo.AosId));
    }
    
    protected override  Task AddContainers(IEnumerable<RMContainerInfoAdaption> containerInfos)
    {
        return s_syncGoogleNodeDao.AddGoogleContainersAsync(containerInfos);
    }

    protected override Task DeleteContainers(IEnumerable<RMContainerInfoAdaption> containerInfos)
    {
        return s_syncGoogleNodeDao.DeleteGoogleContainersAsync(containerInfos);
    }

    protected override Task UpdateContainers(IEnumerable<RMContainerInfoAdaption> containerInfos)
    {
        return s_syncGoogleNodeDao.UpdateGoogleContainersAsync(containerInfos);
    }

    protected override async Task<IEnumerable<RMContainerInfoAdaption>> GetRecordContainers()
    {
        return await s_syncGoogleNodeDao.GetGoogleContainersAsync(RecordContainerNodeLevel);
    }
}