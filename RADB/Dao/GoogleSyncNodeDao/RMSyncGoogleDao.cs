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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.Encryption;
using AvePoint.RA.Contract.SyncNode;
using AvePoint.RA.Contract.SyncNode.GoogleSyncNode;
using AvePoint.RA.DB.Core.Synchronize.DbContext.Base;
using AvePoint.RA.DB.Dao.SynchronizeDao;
using AvePoint.RA.DB.Dao.SynchronizeDao.Imp;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.DB.Dao.GoogleSyncNodeDao;

public class RMSyncGoogleDao : IRMGoogleSyncNodeDao
{
    private const int S_OPERATION_BATCH_COUNT = 1000;
    
    private readonly IRemoteNodeSynchronizeDao _remoteNodeSynchronizeDao = new SqliteRemoteNodeSynchronizeDao();
    
    private IRemoteNodeEvent _remoteNodeEvent;
    public async Task UpdateGoogleNodesAsync(IEnumerable<RMGoogleNodeAdaption> nodes)
    {
        var nodesDic = nodes.ToDictionary(item => item.Id, item => item);

        for (var i = 0; i < nodes.Count(); i += S_OPERATION_BATCH_COUNT)
        {
            var needUpdateIds = nodes.Skip(i).Take(S_OPERATION_BATCH_COUNT).Select(item => item.Id);

            var needUpdateNodes = await _remoteNodeSynchronizeDao.GetRemoteNodesAsync(needUpdateIds);
            
            foreach (var needUpdateNode in needUpdateNodes)
            {
                if (nodesDic.TryGetValue(needUpdateNode.Id, out var node))
                {
                    needUpdateNode.ObjectId = node.ObjectId;
                    needUpdateNode.Name = node.Name;
                    needUpdateNode.Url = node.Name;
                    needUpdateNode.AppType = (int)node.AppType;
                    needUpdateNode.ModifiedDate = DateTime.UtcNow.Ticks;
                    needUpdateNode.UserName = string.IsNullOrWhiteSpace(node.UserName) ? string.Empty : RMDatabaseDefaultEncryptor.EncryptToString(node.UserName);
                }
            }
            
            await _remoteNodeEvent.NotifyUpdateAsync(needUpdateNodes);
        }
    }

    public async Task DeleteGoogleNodesAsync(IEnumerable<RMGoogleNodeAdaption> nodes)
    {
        for (var i = 0; i < nodes.Count(); i += S_OPERATION_BATCH_COUNT)
        {
            var needDeleteNodes = nodes.Skip(i).Take(S_OPERATION_BATCH_COUNT).ConvertAll(item => item.Id);
            
            await _remoteNodeEvent.NotifyDeleteAsync(needDeleteNodes);
        }
    }

    public async Task AddGoogleNodesAsync(IEnumerable<RMGoogleNodeAdaption> nodes)
    {
        for (var i = 0; i < nodes.Count(); i += S_OPERATION_BATCH_COUNT)
        {
            var needAddNodes = nodes.Skip(i).Take(S_OPERATION_BATCH_COUNT).ConvertAll(item => new RMRemoteNode
            {
                Id = item.Id,
                ObjectId = item.ObjectId,
                TenantId = item.TenantId,
                ParentId = item.ContainerId,
                NodeLevel = (int)item.NodeLevel,
                AppType = (int)item.AppType,
                Url = item.Name,
                Name = item.Name,
                CreateTime = DateTime.UtcNow.Ticks,
                ModifiedDate = DateTime.UtcNow.Ticks,
                UserName = string.IsNullOrWhiteSpace(item.UserName)
                    ? string.Empty
                    : RMDatabaseDefaultEncryptor.EncryptToString(item.UserName),
            });

            await _remoteNodeEvent.NotifyAddAsync(needAddNodes);
        }
    }

    public async Task DeleteGoogleContainersAsync(IEnumerable<RMContainerInfoAdaption> containerInfos)
    {
        for (var i = 0; i < containerInfos.Count(); i += S_OPERATION_BATCH_COUNT)
        {
            var needDeleteContainers = containerInfos.Skip(i).Take(S_OPERATION_BATCH_COUNT).ConvertAll(item => new RMRemoteNode
            {
                Id = item.Id
            });
            
            
            await _remoteNodeEvent.NotifyDeleteContainerAsync(needDeleteContainers.Select(item => item.Id));
        }
    }

    public async Task UpdateGoogleContainersAsync(IEnumerable<RMContainerInfoAdaption> containerInfos)
    {
        for (var i = 0; i < containerInfos.Count(); i += S_OPERATION_BATCH_COUNT)
        {
            var needUpdateContainers = containerInfos.Skip(i).Take(S_OPERATION_BATCH_COUNT)
                .ToDictionary(item => item.Id, item => item);
            var ids = needUpdateContainers.Keys.ToHashSet();
            
            var existContainers = await _remoteNodeSynchronizeDao.GetRemoteNodesAsync(ids);
            
            foreach (var existContainer in existContainers)
            {
                var needUpdateContainer = needUpdateContainers[existContainer.Id];

                existContainer.Name = needUpdateContainer.Name;
                existContainer.Url = needUpdateContainer.Name;
            }
            
            await _remoteNodeEvent.NotifyAddAsync(existContainers);
        }
    }

    public async Task AddGoogleContainersAsync(IEnumerable<RMContainerInfoAdaption> containerInfos)
    {
        
        for (var i = 0; i < containerInfos.Count(); i += S_OPERATION_BATCH_COUNT)
        {
            var needAddContainers = containerInfos.Skip(i).Take(S_OPERATION_BATCH_COUNT).ConvertAll(item => new RMRemoteNode
            {
                Id = item.Id,
                Name = item.Name,
                Url = item.Name,
                NodeLevel = (int)item.NodeLevel,
                AosId = item.AosId,
                CreateTime = DateTime.UtcNow.Ticks,
                ModifiedDate = DateTime.UtcNow.Ticks
            });
            
            await _remoteNodeEvent.NotifyAddAsync(needAddContainers);
        }
    }

    public IAsyncEnumerable<RMGoogleNodeAdaption> GetGoogleNodesAsync(string containerId, string tenantId)
    {
        return _remoteNodeSynchronizeDao.GetRemoteNodesAsync(containerId, tenantId).Select(node => new RMGoogleNodeAdaption
        {
            Id = node.Id,
            ObjectId = node.ObjectId,
            TenantId = node.TenantId,
            ContainerId = node.ParentId,
            NodeLevel = (NodeLevel)node.NodeLevel,
            Name = node.Name,
            UserName = string.IsNullOrWhiteSpace(node.UserName) ? string.Empty : RMDatabaseDefaultEncryptor.DecryptToString(node.UserName),
        });
    }

    public async Task<List<RMContainerInfoAdaption>> GetGoogleContainersAsync(NodeLevel nodeLevel)
    {
        var containers = await _remoteNodeSynchronizeDao.GetContainerNodesAsync(nodeLevel);

        return containers.ToList().ConvertAll(item => new RMContainerInfoAdaption
        {
            Id = item.Id,
            AosId = item.AosId,
            Name = item.Name,
            NodeLevel = (NodeLevel)item.NodeLevel
        });
    }

    public void InjectRemoteNodeSynchronizeEvent(IRemoteNodeEvent remoteNodeEvent)
    {
        _remoteNodeEvent = remoteNodeEvent;
    }
}