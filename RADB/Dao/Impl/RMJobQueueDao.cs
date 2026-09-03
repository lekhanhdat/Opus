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
using AvePoint.RA.Common.ClientRequest;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using Newtonsoft.Json;
using PnP.Framework.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMJobQueueDao : BaseDao<RMJobQueue>, IRMJobQueueDao
    {
        public string AddToJobQueue(RMJobQueue jobInfo)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                jobInfo.MessageId = Guid.NewGuid().ToString();
                jobInfo.CreateTime = DateTime.UtcNow.Ticks;
                jobInfo.UpdateTime = DateTime.UtcNow.Ticks;
                jobInfo.ClientIP = ClientRequestLocalValue.ClientIP;
                var job = ctx.JobQueue.Add(jobInfo);
                ctx.SaveChanges();
                return jobInfo.MessageId;
            }
        }

        public void DeleteQueueMessage(string Id, string tenantId)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                var job = ctx.JobQueue.AsQueryable().Where(j => j.MessageId == Id && j.TenantId == tenantId).FirstOrDefault();
                if (job != null)
                {
                    ctx.JobQueue.Remove(job);
                    ctx.SaveChanges();
                }
                
            }
        }

        public async Task<int> DeleteQueueMessageBatchAsync(List<string> idList)
        {
            using var ctx = RMDBContextManager.GetSystemDBContext();
            var idInClause = DatabaseUtility.BuildInClause(idList, out var idParams);
            string sql = $"DELETE FROM RMJobQueues WHERE MessageId IN {idInClause}";

            return await ctx.Database.ExecuteSqlCommandAsync(
                sql,
                idParams.ToArray());
        }

        public void ReEnterQueueMessage(string Id, string tenantId)
        {
            using var ctx = RMDBContextManager.GetSystemDBContext();
            string sql = $"UPDATE RMJobQueues SET Status=0, UpdateTime={DateTime.UtcNow.Ticks} WHERE MessageId=@Id AND TenantId=@TenantId AND Status=1";
            ctx.Database.ExecuteSqlCommand(
                sql,
                new SqlParameter("Id", Id),
                new SqlParameter("TenantId", tenantId));
        }

        public async Task<int> ReEnterQueueMessageBatchAsync(List<string> idList)
        {
            using var ctx = RMDBContextManager.GetSystemDBContext();
            var idInClause = DatabaseUtility.BuildInClause(idList, out var idParams);
            string sql = $"UPDATE RMJobQueues SET Status=0, UpdateTime={DateTime.UtcNow.Ticks} WHERE MessageId IN {idInClause} AND Status=1";

            return await ctx.Database.ExecuteSqlCommandAsync(
                sql,
                idParams.ToArray());
        }

        public RMJobQueue GetQueue(string id, string tenantid)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                return ctx.JobQueue.AsNoTracking().Where(jq => jq.MessageId == id && jq.TenantId == tenantid).FirstOrDefault();
            }
        }
        public List<RMJobQueue> GetDBJobQueueMessage(string tenantId, string useEmail, JobType jobType)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                return ctx.JobQueue.AsQueryable().Where(message => message.TenantId == tenantId && message.JobRunBy == useEmail && message.JobType == (int)jobType).ToList();
            }
        }
        public List<RMJobQueue> GetQueueMessage(string productionId)
        {
            List<RMJobQueue> jobs = new List<RMJobQueue>();
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                //jobs = ctx.JobQueue.AsNoTracking().AsQueryable().Where(j => j.ProductVersion.Equals(productionId, StringComparison.OrdinalIgnoreCase)).OrderBy(j => j.CreateTime).ToList();
                jobs = ctx.JobQueue.AsNoTracking().AsQueryable().OrderBy(j => j.CreateTime).ToList();
            }
            return jobs;
        }

        public int GetMessagesCount(string tenantId, JobType jobType)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                return ctx.JobQueue.AsNoTracking()
                    .Count(j => j.TenantId == tenantId && j.JobType == (int)jobType);
            }
        }

        public List<RMJobQueue> GetMessages(string tenantId, params JobType[] jobTypes)
        {
            int[] jobTypeArr = jobTypes.Select(jt => (int)jt).ToArray();
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                return ctx.JobQueue.AsNoTracking().Where(j => j.TenantId == tenantId && Enumerable.Contains(jobTypeArr, j.JobType)).ToList();
            }
        }

        public int GetTenantJobQueueCount(string tenantId)
        {
            using var ctx = RMDBContextManager.GetSystemDBContext();
            const string sql = "SELECT COUNT(1) FROM RMJobQueues WHERE TenantId=@TenantId";
            return ctx.Database.SqlQuery<int>(sql, new SqlParameter("TenantId", tenantId)).FirstOrDefault();
        }

        public List<RMJobQueue> GetQueues(int pageIndex, int pageSize, out int totalRecord, string orderKey, bool isAsc, Expression<Func<RMJobQueue, bool>> whereLambda = null)
        {
            string sortBy = "CreateTime";
            SortDirectionEnum sortDirection = SortDirectionEnum.Ascending;
            string thenSortBy = "";
            SortDirectionEnum thenSortDirection = SortDirectionEnum.None;
            if (orderKey == sortBy && isAsc)
            {
            }
            else
            {
                thenSortBy = sortBy;
                thenSortDirection = sortDirection;
                sortBy = orderKey;
                sortDirection = isAsc ? SortDirectionEnum.Ascending : SortDirectionEnum.Descending;
            }

            //var context = SharedDbContext;
            try
            {
                var excludeQueryJobTypes = new List<int>
                {
                    (int)JobType.SharePointOnlineDeletionSyncUpgrade,
                    (int)JobType.TenantUpgrade,
                    (int)JobType.ManualHistoriesUpgrade,
                    (int)JobType.CosmosDBDirtyDataDeleteUpgrade,
                    (int)JobType.ManualFileSystemUpgrade,
                    (int)JobType.SendEmailJob,
                    (int)JobType.DiscoveryJob,
                    (int)JobType.DiscoveryOptimizationCalculate,
                    (int)JobType.DiscoveryAOSPOptimizationCalculate,
                    (int)JobType.DiscoveryReCalculate,
                    (int)JobType.RebuildStub,
                    (int)JobType.RebuildIndex,
                    (int)JobType.ApprovalProcessArchive,
                    (int)JobType.RebuildSOJobReport,
                    (int)JobType.RebuildEncryptKeyValue,
                    (int)JobType.DispatchedJob,
                    (int)JobType.BuildRunningJobReport,
                    (int)JobType.ExportDecryptIndexDB,
                    (int)JobType.MultiSiteCollectionRestore,
                    (int)JobType.SimulateRestore,
                    (int)JobType.RebuildDeDupForWPPMigration,
                    (int)JobType.TeamsChannelSettingConflictCheck,
                    (int)JobType.BaseArchiveJobIdMultiRestore,
                    (int)JobType.MigrateDataCosmosDbForJPMC,
                    (int)JobType.ArchiverFullMoveRetention,
                    (int)JobType.APStorageCostEvaluation,
                    (int)JobType.PreviewRestore,
                };
                using (var context = RMDBContextManager.GetSystemDBContext())
                {
                    IOrderedQueryable<RMJobQueue> query = null;

                    if (whereLambda != null)
                    {
                        query = context.JobQueue.AsQueryable().Where(whereLambda).Where(q=>q.TenantId == TenantLocalValue.LogonGroupId && !excludeQueryJobTypes.Contains(q.JobType)).SortBy(sortBy, sortDirection).ThenSortBy(thenSortBy, thenSortDirection);
                    }
                    else
                    {
                        query = context.JobQueue.AsQueryable().Where(q => q.TenantId == TenantLocalValue.LogonGroupId && !excludeQueryJobTypes.Contains(q.JobType)).SortBy(sortBy, sortDirection).ThenSortBy(thenSortBy, thenSortDirection);
                    }
                    totalRecord = query.Count();
                    var results = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
                    return results.ToList();
                }
            }
            catch { totalRecord = 0; return null; }

        }

        public List<RMJobQueue> GetRCCDBJobQueueByLoginName(string loginName, List<string> scopeIds)
        {
            using (var context = RMDBContextManager.GetSystemDBContext())
            {
                var query = context.JobQueue.AsNoTracking().Where(q => q.JobRunBy == loginName && q.JobType == (int)JobType.DownloadRCCReport).OrderBy(q => q.CreateTime);

                var userJobs = query.ToList();

                if (scopeIds != null && scopeIds.Count > 0)
                {
                    userJobs = userJobs.Where(q =>
                    {
                        if (string.IsNullOrEmpty(q.Parameters)) return false;

                        try
                        {
                            var paramObj = JsonConvert.DeserializeObject<RCCReportRequest>(q.Parameters);
                            return paramObj?.Nodes?.Any(n => scopeIds.Contains(n.Id.ToString())) ?? false;
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }).ToList();
                }

                return userJobs;
            }
        }

        public List<RMJobQueue> GetDisposalHistoryDBJobQueueByLoginName(string loginName, string scopeId)
        {
            using (var context = RMDBContextManager.GetSystemDBContext())
            {
                var query = context.JobQueue.AsNoTracking().Where(q => q.JobRunBy == loginName && q.JobType == (int)JobType.ManualExportHistoryDatasJob).OrderBy(q => q.CreateTime);

                var userJobs = query.ToList();

                if (scopeId != null)
                {
                    userJobs = userJobs.Where(q =>
                    {
                        if (string.IsNullOrEmpty(q.Parameters)) return false;

                        try
                        {
                            var paramObj = JsonConvert.DeserializeObject<ManualApprovalHistoryOption>(q.Parameters);
                            var nodeId = paramObj?.Id.ToString();

                            return !string.IsNullOrEmpty(nodeId) && (scopeId.EqualsIgnoreCase(nodeId));
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }).ToList();
                }

                return userJobs;
            }
        }

        public List<RMJobQueue> GetAllDBJobQueueByLoginName(string loginName, int jobType)
        {
            using (var context = RMDBContextManager.GetSystemDBContext())
            {
                var query = context.JobQueue.AsNoTracking().Where(q => q.JobRunBy == loginName && q.JobType == jobType).OrderBy(q => q.CreateTime);
                return query.ToList();
            }
        }

        public Dictionary<string, List<RMJobQueue>> GetDBJobMessageGroupByTenant(int top)
        {
            using (var context = RMDBContextManager.GetSystemDBContext())
            {
                var allJobQueueItems = context.JobQueue.Where(q => q.Status == 0).ToList();
                Dictionary<string, List<RMJobQueue>> result = (
                    from j in allJobQueueItems
                    group j by j.TenantId into g
                    select new
                    {
                        Key = g.Key,
                        Value = g.OrderByDescending(a => a.JobPriority).ThenBy(m => m.CreateTime).Take(top).Union(g.Where(m => m.Parameters != null && m.Parameters.Contains("<IsEndUserJob>true</IsEndUserJob>") || m.JobType == (int)JobType.SyncNodesFromAOS)).ToList()
                    }).ToDictionary(k => k.Key, v => v.Value);
                foreach (var list in result.Values)
                {
                    foreach (RMJobQueue queue in list)
                    {
                        queue.Status = 1;
                        queue.UpdateTime = DateTime.UtcNow.Ticks;
                        var entry = context.Entry(queue);
                        if (entry.State == EntityState.Modified)
                        {
                            context.SaveChanges();
                        }
                        else if (entry.State == EntityState.Detached)
                        {
                            context.DetachLocalObject<RMJobQueue>(queue);
                            context.Set<RMJobQueue>().Attach(queue);
                            entry.State = EntityState.Modified;
                            context.SaveChanges();
                        }
                    }
                }
                return result;
            }
        }
        
        public bool UpdateJobPriority(string messageId, JobPriority newPriority, string tenantId)
        {
            using var ctx = RMDBContextManager.GetSystemDBContext();
            var job = ctx.JobQueue.AsQueryable().Where(jq => jq.MessageId == messageId && jq.TenantId == tenantId).FirstOrDefault();
            if (job != null)
            {
                job.JobPriority = newPriority;
                job.UpdateTime = DateTime.UtcNow.Ticks;
                ctx.JobQueue.AddOrUpdate(job);
                ctx.SaveChanges();
                return true;
            }
            return false;
        }

        public List<RMJobQueue> GetTimeoutProcessingMessages(long timeoutPeriod, string anchorMessageId, int top)
        {
            using var ctx = RMDBContextManager.GetSystemDBContext();
            if (string.IsNullOrEmpty(anchorMessageId))
            {
                string sql = $"SELECT TOP {top} * FROM RMJobQueues WHERE Status=1 AND UpdateTime<{timeoutPeriod} ORDER BY MessageId";
                return ctx.Database.SqlQuery<RMJobQueue>(sql).ToList();
            }
            else
            {
                string sql = $"SELECT TOP {top} * FROM RMJobQueues WHERE MessageId>@AnchorId AND Status=1 AND UpdateTime<{timeoutPeriod} ORDER BY MessageId";
                return ctx.Database.SqlQuery<RMJobQueue>(sql, new SqlParameter("AnchorId", anchorMessageId)).ToList();
            }
        }

    }
}
