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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Explorer.Model;
using System.Linq.Expressions;
using AvePoint.RA.DB.Dao.Extension;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMManualApprovalDao : IRMManualApprovalDao
    {

        private IRMKeyValueDao keyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public IEnumerable<List<RMManualApprove>> GetHistoryDatas(int limit = 1000)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            context.Database.CommandTimeout = 600;
            var count = 0;
            var pageIndex = 0;
            do
            {
                var result = context.ManualApprove
                    .Where(item => item.ActionStatus == (int)ActionStatus.Archiverd)
                    .OrderBy(item => item.CollectionTime)
                    .Skip(limit * pageIndex++)
                    .Take(limit)
                    .ToList();
                count = result.Count;
                yield return result;
            } while (count == limit);
        }

        public IEnumerable<List<RMManualApprove>> GetUnArchiveDatas(SourceFlag source, int limit = 1000)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            context.Database.CommandTimeout = 600;
            var count = 0;
            var pageIndex = 0;
            do
            {
                var result = context.ManualApprove
                    .Where(item => item.SourceFlag == (int)source && item.ActionStatus != (int)ActionStatus.Archiverd)
                    .OrderBy(item => item.CollectionTime)
                    .Skip(limit * pageIndex++)
                    .Take(limit)
                    .ToList();
                count = result.Count;
                yield return result;
            } while (count == limit);
        }

        public IEnumerable<List<RMManualApprove>> GetNeedSyncToCosmosDbHistoryDatas(SourceFlag source, int limit = 1000)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            context.Database.CommandTimeout = 600;
            var count = 0;
            var pageIndex = 0;
            do
            {
                var result = context.ManualApprove
                    .Where(item => item.SourceFlag == (int)source &&
                    item.ActionStatus == (int)ActionStatus.Archiverd &&
                    item.ActionTime > 0)
                    .OrderBy(item => item.CollectionTime)
                    .Skip(limit * pageIndex++)
                    .Take(limit)
                    .ToList();
                count = result.Count;
                yield return result;
            } while (count == limit);
        }

        public RMWorkflowStatus GetWorkflowInstanceStatus(Guid workflowInstanceId)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return (RMWorkflowStatus)(context.WorkflowInstance.FirstOrDefault(item => item.Id == workflowInstanceId)?.Status);
        }

        public async Task<(List<ManualApprovalWorkspaceItem> Items, int Count, int SearchCount)> GetWorkspacesForSharePointOnline(int pageIndex, int pageSize, string searchValue)
        {
            searchValue = searchValue.Trim();
            var spNodeLevels = new HashSet<int> { (int)NodeLevel.SharedChannel, (int)NodeLevel.PrivateChannel, (int)NodeLevel.O365GroupSites, (int)NodeLevel.SiteCollection };

            using var context = RMDBContextManager.GetNewDBContext();
            if (keyValueDao.HasUpgradeTeams())
            {
                spNodeLevels = new HashSet<int> { (int)NodeLevel.SiteCollection };
            }
            context.Database.CommandTimeout = 900;
            var items = await context.RMRemoteNodes.Where(item => spNodeLevels.Contains(item.NodeLevel) && item.Url.Contains(searchValue))
                .OrderBy(item => item.Url)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(item => new ManualApprovalWorkspaceItem
                {
                    WorkspacePath = item.Url,
                    WorkspaceId =  string.IsNullOrEmpty(item.ObjectId)? Guid.Empty : new (item.ObjectId)
                })
                .ToListAsync();
            var count = 0;
            var searchCount = 0;
            if(pageIndex == 0)
            {
                count = await context.RMRemoteNodes.CountAsync(item => spNodeLevels.Contains(item.NodeLevel));
                searchCount = await context.RMRemoteNodes.CountAsync(item => spNodeLevels.Contains(item.NodeLevel) && item.Url.Contains(searchValue));
            }

            return (items, count, searchCount);
        }

        public async Task<(List<ManualApprovalWorkspaceItem> Items, int Count, int SearchCount)> GetWorkspacesForOneDrive(int pageIndex, int pageSize, string searchValue)
        {
            searchValue = searchValue.Trim();
            var oneDriveNodeLevel = (int)NodeLevel.SkyDrivePro;
            using var context = RMDBContextManager.GetNewDBContext();
            context.Database.CommandTimeout = 900;
            var items = await context.RMRemoteNodes.Where(item => oneDriveNodeLevel == item.NodeLevel && item.Url.Contains(searchValue))
                .OrderBy(item => item.Url)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(item => new ManualApprovalWorkspaceItem
                {
                    WorkspacePath = item.Url,
                    WorkspaceId = new(item.ObjectId)
                })
                .ToListAsync();
            var count = 0;
            var searchCount = 0;
            if (pageIndex == 0)
            {
                count = await context.RMRemoteNodes.CountAsync(item => oneDriveNodeLevel == item.NodeLevel);
                searchCount = await context.RMRemoteNodes.CountAsync(item => oneDriveNodeLevel == item.NodeLevel && item.Url.Contains(searchValue));
            }

            return (items, count, searchCount);
        }

        public async Task<(List<ManualApprovalWorkspaceItem> Items, int Count, int SearchCount)> GetWorkspacesForTeams(int pageIndex, int pageSize, string searchValue)
        {
            searchValue = searchValue.Trim();
            var spNodeLevels = new HashSet<int> { (int)NodeLevel.SharedChannel, (int)NodeLevel.PrivateChannel, (int)NodeLevel.O365GroupSites};
            using var context = RMDBContextManager.GetNewDBContext();
            context.Database.CommandTimeout = 900;
            var items = await context.RMRemoteNodes.Where(item => spNodeLevels.Contains(item.NodeLevel) && item.Url.Contains(searchValue))
                .OrderBy(item => item.Url)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(item => new ManualApprovalWorkspaceItem
                {
                    WorkspacePath = item.Url,
                    WorkspaceId = string.IsNullOrEmpty(item.ObjectId) ? Guid.Empty : new(item.ObjectId)
                })
                .ToListAsync();
            var count = 0;
            var searchCount = 0;
            if (pageIndex == 0)
            {
                count = await context.RMRemoteNodes.CountAsync(item => spNodeLevels.Contains(item.NodeLevel));
                searchCount = await context.RMRemoteNodes.CountAsync(item => spNodeLevels.Contains(item.NodeLevel) && item.Url.Contains(searchValue));
            }

            return (items, count, searchCount);
        }

        public  async Task<ManualApprovalFilterFolderPathResult> GetFolderPathResults(
                    ManualApprovalFilterFolderPathResult result, ManualApprovalRecordRepository repository,
                    Expression<Func<ManualApprovalRecord, bool>> predicate, Expression<Func<ManualApprovalRecord, bool>> notAdminpredicate,
                    int pageSize)
        {
            try
            {
                Expression<Func<ManualApprovalRecord, bool>> isSelectFolderPath = item => true;
                string continuationToken = result.Continuation;
                do
                { 
                    var folderPathItem = await repository.QueryItemsWithPaginationAsyncForFolderPath(predicate, notAdminpredicate, pageSize, continuationToken, isSelectFolderPath).ConfigureAwait(false);

                    result.FolderPathResults.UnionWith(folderPathItem.Items);

                    isSelectFolderPath = item => !result.FolderPathResults.Contains(item.ManualFolderPath);
                    
                    result.Continuation = continuationToken = folderPathItem.Continuation;

                    pageSize = 15 - result.FolderPathResults.Count;

                } while (pageSize > 0 && !string.IsNullOrEmpty(continuationToken));

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
          
        }

        public async Task<(List<ManualApprovalWorkspaceItem> Items, int Count, int SearchCount)> GetWorkspacesForGoogle(int pageIndex, int pageSize, string searchValue)
        {
            searchValue = searchValue.Trim();
            var googleNodeLevels = new HashSet<int> { (int)NodeLevel.GoogleMyDrive, (int)NodeLevel.GoogleSharedDrive };

            using var context = RMDBContextManager.GetNewDBContext();
            context.Database.CommandTimeout = 900;

            var baseQuery = context.RMRemoteNodes.Where(item => googleNodeLevels.Contains(item.NodeLevel));

            var searchQuery = baseQuery.Where(item => item.Url.Contains(searchValue));

            var itemsTask = await searchQuery
                .OrderBy(item => item.Url)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(item => new ManualApprovalWorkspaceItem
                {
                    WorkspacePath = item.Url,
                    Extention = item.ObjectId,
                })
                .ToListAsync();

            int totalCount = 0;
            int searchCount = 0;

            if (pageIndex == 0)
            {
                totalCount = baseQuery.Count();
                searchCount = searchQuery.Count();
            }

            return (itemsTask, totalCount, searchCount);
        }
    }
}
