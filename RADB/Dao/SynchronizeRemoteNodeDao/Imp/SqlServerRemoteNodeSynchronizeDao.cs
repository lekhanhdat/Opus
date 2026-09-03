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
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Util;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.DB.Dao.SynchronizeDao.Imp;

public class SqlServerRemoteNodeSynchronizeDao  : IRemoteNodeSynchronizeDao
{
    private static RMDbContext GetDbContext()
    {
        var context = RMDBContextManager.GetNewDBContext();
        context.Database.CommandTimeout = 600;
        return context;
    }
    
    public async Task<IEnumerable<RMRemoteNode>> GetRemoteNodesAsync(IEnumerable<string> ids)
    {
        if(ids.IsNullOrEmpty())
        {
            return [];
        }
        using var context = GetDbContext();
        return await context.RMRemoteNodes.Where(node => ids.Contains(node.Id)).ToListAsync();
    }

    public async IAsyncEnumerable<RMRemoteNode> GetRemoteNodesAsync(string containerId, string tenantId)
    {
        var batchCount = 1000;

        using var context = GetDbContext();
        for(var i = 0; ; i+=batchCount)
        {
            var sites = await context.RMRemoteNodes.Where(item => item.ParentId == containerId.ToString()
                                                                  && item.NodeLevel == (int)NodeLevel.SiteCollection)
                .OrderBy(item => item.Id)
                .Skip(i).Take(batchCount)
                .ToListAsync();
            foreach(var site in sites)
            {
                yield return site;
            }
            if(sites.Count < batchCount)
            {
                yield break;
            }
        }
    }

    public async Task<IEnumerable<RMRemoteNode>> GetContainerNodesAsync(NodeLevel nodeLevel)
    {
        using var context = GetDbContext();

       return await context.RMRemoteNodes.Where(item => item.NodeLevel == (int)nodeLevel).ToListAsync();
    }

    public async Task DeleteNodesAsync(IEnumerable<string> nodeIds)
    {
        if(nodeIds.IsNullOrEmpty())
        {
            return;
        }
        using var context = GetDbContext();

        var nodeIdList = nodeIds.ToList();
        for(var i = 0; i < nodeIdList.Count; i += 1000)
        {
            var batchNodeIds = nodeIdList.Skip(i).Take(1000).ToList();
            if(batchNodeIds.IsNullOrEmpty())
            {
                continue;
            }

            var inClauseParamName = DatabaseUtility.BuildInClause(batchNodeIds, out var paramList);

            var sql = $"DELETE FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMRemoteNodes WHERE Id IN {inClauseParamName}";

            await context.Database.ExecuteSqlCommandAsync(sql, paramList.ToArray());
        }
    }

    public async Task DeleteNodesByParentIdsAsync(IEnumerable<string> parentIds)
    {
        if(parentIds.IsNullOrEmpty())
        {
            return;
        }
        using var context = GetDbContext();
        
        var needDeleteContainers = parentIds.ConvertAll(item => new RMRemoteNode
        {
            Id = item
        });

        var inClauseParamName = DatabaseUtility.BuildInClause(needDeleteContainers.Select(item => item.Id), out var paramList);

        var sql = $"DELETE FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMRemoteNodes WHERE ParentId IN {inClauseParamName}";

        await context.Database.ExecuteSqlCommandAsync(sql, paramList.ToArray());
        
        needDeleteContainers.ForEach(item => context.Entry<RMRemoteNode>(item).State = EntityState.Deleted);

        await context.SaveChangesAsync();
        
    }

    public async Task AddNodesAsync(IEnumerable<RMRemoteNode> nodes)
    {
        if(nodes.IsNullOrEmpty())
        {
            return;
        }
        using var context = GetDbContext();
        using var transaction = context.Database.BeginTransaction();
        try
        {
            var needDeleteNodes = nodes.Select(item => item.Id);
        
            var inClauseParamName = DatabaseUtility.BuildInClause(needDeleteNodes, out var paramList);

            var sql = $"DELETE FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].[RMRemoteNodes] WHERE Id IN {inClauseParamName}";

            await context.Database.ExecuteSqlCommandAsync(sql, paramList.ToArray());
        
            context.RMRemoteNodes.AddRange(nodes);
        
            await context.SaveChangesAsync();
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
        
    }

    public async Task UpdateNodesAsync(IEnumerable<RMRemoteNode> nodes)
    {
        using var context = GetDbContext();
        
        context.RMRemoteNodes.AddOrUpdate(nodes.ToArray());
        
        await context.SaveChangesAsync();
    }

    public async Task<bool> HasAnySites()
    {
        using var context = GetDbContext();
        return await context.RMRemoteNodes.AnyAsync();
    }
}