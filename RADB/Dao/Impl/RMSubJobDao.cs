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
using AvePoint.GCommon.Contract.Replicator.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.COP;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Google.GControlPlatform;
using AvePoint.RA.Contract.Tenant;
using System.Data.Entity.Infrastructure;

//using Z.EntityFramework.Plus;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMSubJobDao : BaseDao<RMSubJob>, IRMSubJobDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMSubJobDao));
        private readonly static Object updateLocker = new object();
        private const int BulkInsertRetryCount = 3;
        private const int BulkInsertRetryDelaySeconds = 5;

        private sealed class WaitingSubJobInfo
        {
            public string Id { get; init; }
            public string ParentId { get; init; }
        }

        private sealed class ParentSubJobCountInfo
        {
            public string ParentId { get; set; }
            public int SubJobCount { get; set; }
        }

        private sealed class DirtyWaitingSubJobRow
        {
            public string Id { get; set; }
            public string String1 { get; set; }
            public string O365TenantId { get; set; }
        }

        private readonly List<JobType> _jobTypesAssociateWithGControl =
        [
            JobType.GoogleApplySettings,
            JobType.GoogleRecordsDisposal,
            JobType.GoogleDataSynchronization,
            JobType.TermSynchronization,
            JobType.ExplorerOfflineSearch,
            JobType.GoogleArchiverRestore,
            JobType.GoogleArchiverRetention,
            JobType.GlobalSearchAction,
            JobType.ManualApprovalOrRejectJob,
            JobType.SyncNodesFromAOS,
            JobType.SyncSecurityContainer,
            JobType.Dashboard,
            JobType.ManualApprovalEmailSchedule,
            JobType.MachineLearningReviewReclassify,
            JobType.MachineLearningReviewApprove,
            JobType.ImportTermStructure,
            JobType.DiscoveryGoogleJobV1,
            JobType.DiscoveryGoogleProfileJob,
        ];
        private IGControlPlatformJobService GControlPlatformJobService => PlatformWindsorManager.GetService<IGControlPlatformJobService>();
        
        private readonly ITenantService _tenantService = PlatformWindsorManager.GetService<ITenantService>();

        private IFSConnectionRelatedJobInfoDao FSConnectionRelatedJobInfoDao => PlatformWindsorManager.GetService<IFSConnectionRelatedJobInfoDao>();

        public async Task<List<RMSubJob>> GetDiscoveryAnalysisSubJobs(IEnumerable<Guid> discoveryAnalysisJobIds)
        {
            var ids = discoveryAnalysisJobIds.ToList();
            using RMDbContext context = GetNewContext();
            return await context.RMSubJobs.Where(item => ids.Contains(item.DiscoveryAnalysisJobId)).ToListAsync();
        }

        public async Task AddOrUpdateSubJobsAsync(params RMSubJob[] subJobs)
        {
            using RMDbContext context = GetNewContext();
            context.RMSubJobs.AddOrUpdate(subJobs);
            await context.SaveChangesAsync();
        }

        public RMSubJob GetSubJob(string id, bool withContext = false)
        {
            using (RMDbContext context = GetNewContext())
            {
                RMSubJob subJob = context.RMSubJobs.Find(id);
                if (withContext)
                {
                    subJob.JobContext = context.JobContexts.Find(id);
                }
                return subJob;
            }
        }

        public void UpdateContentOfSubJobContext(string subJobId, string context)
        {
            string sql = "update {0}.[RMJobContexts] set Content = @Context where JobId = @SubJobId";
            using (RMDbContext dbContext = GetNewContext())
            {
                int row = dbContext.Database.ExecuteSqlCommand(
                    string.Format(sql, dbContext.SchemaName),
                    new SqlParameter("Context", context),
                    new SqlParameter("SubJobId", subJobId));
            }
        }

        public async Task<JobStatus> GetSubJobStatusAsync(string id)
        {
            using (RMDbContext context = GetNewContext())
            {
                RMSubJob subJob = await context.RMSubJobs.FindAsync(id);
                if (subJob != null)
                {
                    return (JobStatus)subJob?.Status;
                }
                return JobStatus.None;
            }
        }

        public async Task<(JobStatus Status, string Comment)> GetSubJobStatusWithCommentAsync(string id)
        {
            using var context = GetNewContext();
            var subJob = await context.RMSubJobs.FindAsync(id);
            return subJob == null
                ? (JobStatus.None, string.Empty)
                : ((JobStatus)subJob.Status, subJob.Comment ?? string.Empty);
        }


        public List<RMSubJob> GetDirtyWaitingArchiverSubJob(List<JobType> jobTypes, List<string> existOffice365tenantIds)
        {
            List<RMSubJob> dirtySubjobs = new List<RMSubJob>();
            if (jobTypes == null || jobTypes.Count == 0 || existOffice365tenantIds == null || existOffice365tenantIds.Count == 0)
            {
                return dirtySubjobs;
            }

            using (RMDbContext context = GetNewContext())
            {
                int waitingState = (int)JobStatus.Wait;
                List<int> jobTypeStates = jobTypes.Select(t => (int)t).ToList();
                if (jobTypeStates.Count == 0)
                {
                    return dirtySubjobs;
                }

                var schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var jobTypeInClause = DatabaseUtility.BuildInClause(jobTypeStates, out var jobTypeParams);
                var tenantNotInClause = DatabaseUtility.BuildInClause(existOffice365tenantIds, out var tenantParams);

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@waitingState", waitingState)
                };
                sqlParams.AddRange(jobTypeParams);
                sqlParams.AddRange(tenantParams);

                var sql = $@"
                    SELECT Id, String1, O365TenantId
                    FROM {schemaName}.RMSubJobs
                    WHERE JobType IN {jobTypeInClause}
                    AND Status = @waitingState
                    AND O365TenantId IS NOT NULL
                    AND LEN(O365TenantId) > 0
                    AND O365TenantId NOT IN {tenantNotInClause}";

                var rows = context.Database.SqlQuery<DirtyWaitingSubJobRow>(sql, sqlParams.ToArray()).ToList();
                if (rows.Count > 0)
                {
                    dirtySubjobs.AddRange(rows.Select(row => new RMSubJob
                    {
                        Id = row.Id,
                        String1 = row.String1,
                        O365TenantId = row.O365TenantId,
                    }));
                }
            }
            return dirtySubjobs;
        }

        public async Task<List<RMSubJob>> GetOtherSubJobFinishedAsync(string jobId, string parentId)
        {
            using RMDbContext context = GetNewContext();
            return await context.RMSubJobs.Where(subJob => subJob.ParentId == parentId && subJob.Id != jobId).ToListAsync();
        }

        public async Task<List<RMSubJob>> GetSubJobsBySubJobIdsAsync(List<string> subJobIds)
        {
            using RMDbContext context = GetNewContext();
            return await context.RMSubJobs.Where(subJob => subJobIds.Contains(subJob.Id)).ToListAsync();
        }

        public async Task<bool> IsFinalSubJob(string jobId, string parentId)
        {
            using RMDbContext context = GetNewContext();
            var getLastSubJob = await context.RMSubJobs.Where(subJob => subJob.ParentId == parentId).OrderByDescending(subJob => subJob.Id).FirstAsync();
            if (jobId == getLastSubJob.Id)
            {
                return true;
            }

            return false;
        }

        public async Task<string> Get365TenantIdByMainJobId(string mainJobId)
        {
            using RMDbContext context = GetNewContext();
            return await context.RMSubJobs.Where(subJob => subJob.ParentId == mainJobId).Select(subJob => subJob.O365TenantId).FirstOrDefaultAsync();
        }

        public double GetSubJobWeight(string jobId)
        {
            using (RMDbContext context = GetNewContext())
            {
                return context.RMSubJobs.Where(job => job.Id.Equals(jobId)).Select(job => job.Weight).FirstOrDefault();
            }
        }

        public bool UpdateSubJobWeight(string jobId, double weight)
        {
            try
            {
                string sql = "update {0}.RMSubJobs set Weight = @weight where Id = @id";
                using (RMDbContext context = GetNewContext())
                {
                    int row = context.Database.ExecuteSqlCommand(
                        string.Format(sql, context.SchemaName),
                        new SqlParameter("weight", weight),
                        new SqlParameter("id", jobId));
                    return row > 0;
                }
            }
            catch (Exception)
            {
                logger.Error($"Fail update sub job weight, jobid:{jobId}, weight:{weight}");
                return false;
            }
        }

        public bool UpdateSubJobWeightByParentId(string parentId, double weight)
        {
            try
            {
                string sql = "update [{0}].RMSubJobs set Weight = @weight where ParentId = @parentId";
                using (RMDbContext context = GetNewContext())
                {
                    int row = context.Database.ExecuteSqlCommand(
                        string.Format(sql, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)),
                        new SqlParameter("weight", weight),
                        new SqlParameter("parentId", parentId));
                    return row > 0;
                }
            }
            catch (Exception)
            {
                logger.Error($"Fail update sub job weight by parent id, parentId:{parentId}, weight:{weight}");
                return false;
            }
        }

        public void DeleteSubJob(string mainJobId, int jobstate)
        {
            try
            {
                string sql = "delete from {0}.RMSubJobs where ParentId = @jobid and Status = @status";
                using (RMDbContext context = GetNewContext())
                {
                    context.Database.ExecuteSqlCommand(string.Format(sql, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)), new SqlParameter("jobid", mainJobId), new SqlParameter("status", jobstate));
                }
            }
            catch (Exception ex)
            {
                logger.Error($"DeleteSubJob exception1:{ex}");
            }
        }
        public void DeleteSubJobById(string subJobid)
        {
            string sql = "delete from {0}.RMSubJobs where Id = @jobid";
            using (RMDbContext context = GetNewContext())
            {
                context.Database.ExecuteSqlCommand(string.Format(sql, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)), new SqlParameter("jobid", subJobid));
            }
        }

        public List<HSMArchiverSubJobInfo> GetArchiverSubJobsByParentId(string parentId)
        {
            using (RMDbContext context = GetNewContext())
            {
                return context.RMSubJobs.Where(j => j.ParentId == parentId && j.JobType == (int)JobType.ArchiverByHSMXml)
                        .Select(j => new HSMArchiverSubJobInfo
                        {
                            SiteUrl = j.String1,
                            Status = j.Status
                        }).ToList();
            }
        }

        public void BatchUpdateBackupFailedStatusByIds(IEnumerable<string> ids, HasCheckedBackupStatus status)
        {
            if (ids == null)
            {
                return;
            }

            var idList = ids.ToList();
            if (!idList.Any())
            {
                return;
            }

            
            using (RMDbContext context = GetNewContext())
            {
                string sql = $"update {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.RMSubJobs " +
                $"set HasCheckedBackupFailed = @HasCheckedBackupFailed " +
                $"where Id in {DatabaseUtility.BuildInClause(idList, out var dbParams)}";

                dbParams.Add(new SqlParameter("@HasCheckedBackupFailed", (int)status));
                context.Database.ExecuteSqlCommand(sql, dbParams.ToArray());
            }
        }

        public void DeleteJobContext(string jobId)
        {
            try
            {
                bool isSubJob = Common.JobService.JobServiceUtility.IsSubJob(jobId);
                string sql = "delete from {0}.RMJobContexts where JobId = @jobid";
                if (!isSubJob)
                {
                    sql = "delete from {0}.RMJobContexts where JobId like @jobId";
                }
                using (RMDbContext context = GetNewContext())
                {
                    if (isSubJob)
                    {
                        context.Database.ExecuteSqlCommand(string.Format(sql, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)), new SqlParameter("jobid", jobId));
                    }
                    else
                    {
                        context.Database.ExecuteSqlCommand(string.Format(sql, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)), new SqlParameter("jobid", jobId + "%"));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"DeleteJobContext exception:{ex}");
            }
        }

        public void AddJobContext(string jobId, string setting)
        {
            using (RMDbContext context = GetNewContext())
            {
                context.JobContexts.Add(new RMJobContext()
                {
                    JobId = jobId,
                    Settings = setting
                });
                context.SaveChanges();
            }
        }

        /// <summary>
        ///更新进度和时间戳
        /// </summary>
        /// <param name="id"></param>
        /// <param name="progress"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public bool UpdateProgress(string id, double progress, long dateTime)
        {
            //string sql = "update RMSubJobs set Progress = @Progress, LastUpdateTime = @UpdateTime where Id = @Id";
            //SqlParameter _id = new SqlParameter("@Id", id);
            //SqlParameter _progress = new SqlParameter("@Progress", progress);
            //SqlParameter _time = new SqlParameter("@UpdateTime", dateTime);

            //using (RMDbContext context = GetNewContext())
            //{
            //    return context.Database.ExecuteSqlCommand(sql, _id, _progress, _time) > 0;
            //}
            string sql = "update {0}.RMSubJobs set Progress = @progress, LastUpdateTime = @dateTime where Id = @id";
            using (RMDbContext context = GetNewContext())
            {
                //return context.RMSubJobs.Where(a => a.Id == id).Update(c => new RMSubJob() { Progress = progress, LastUpdateTime = dateTime }) > 0;
                int row = context.Database.ExecuteSqlCommand(
                    string.Format(sql, context.SchemaName), 
                    new SqlParameter("progress", progress), 
                    new SqlParameter("dateTime", dateTime), 
                    new SqlParameter("id", id));
                return row > 0;
            }
        }

        public bool CascatMainJobProgress(string parentId, int progress, double doubleProgress)
        {
            string sql = "update {0}.RMJobMonitors set Progress = @progress, DoubleProgress = @doubleProgress, LastUpdateTime = @dateTime where Id = @parentId and Progress <= @progress";
            using (RMDbContext context = GetNewContext())
            {
                //return context.JobMonitors.Where(a => a.Id == parentId && a.Progress <= progress).Update(u => new RMJobMonitor() { Progress = progress, LastUpdateTime = DateTime.UtcNow.Ticks }) > 0;
                int row = context.Database.ExecuteSqlCommand(
                    string.Format(sql, context.SchemaName), 
                    new SqlParameter("progress", progress),
                    new SqlParameter("DoubleProgress", doubleProgress),
                    new SqlParameter("dateTime", DateTime.UtcNow.Ticks),
                    new SqlParameter("parentId", parentId));
                return row > 0;
            }
        }

        public bool CascatMainJoStatus(string parentId, int status)
        {
            int[] finalSatesWithCalc = Common.JobService.JobServiceUtility.JobFinalStatusAndCalculating;
            bool isFinish = status == (int)JobStatus.Finished || status == (int)JobStatus.FinishWithException || status == (int)JobStatus.Calculating || status == (int)JobStatus.Failed || status == (int)JobStatus.Stopped;
            int progress = status == (int)JobStatus.Calculating ? 99 : 100;
            string sqlProgress = string.Empty;
            if (status != (int)JobStatus.Stopped && status != (int)JobStatus.Failed)
            {
                sqlProgress = ("Progress = " + progress + ",");
            }

            using (RMDbContext context = GetNewContext())
            {
                var dateTime = DateTime.UtcNow;
                List<SqlParameter> parameters = new List<SqlParameter>
                {
                       new SqlParameter("status", status),
                        new SqlParameter("dateTime", dateTime.Ticks),
                        new SqlParameter("parentId", parentId)
                };
                var inClauseParamName = DatabaseUtility.BuildInClause(finalSatesWithCalc, out var paramList);
                paramList.AddRange(parameters);

                string sql = isFinish ? $"update {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.RMJobMonitors set Status = @status, " + sqlProgress + $"LastUpdateTime = @dateTime, EndTime = @dateTime where Id = @parentId and Status not in {inClauseParamName}"
                    : $"update {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.RMJobMonitors set Status = @status, LastUpdateTime = @dateTime  where Id = @parentId and Status not in {inClauseParamName}";
                logger.Info($"MainJob status updating, jobId: {parentId}, status: {status}, isFinish: {isFinish}");

                int row = context.Database.ExecuteSqlCommand(sql, paramList.ToArray());

                //Update Special Job Status to GControl
                UpdateGControlJobStatus(parentId, context, (JobStatus)status, dateTime);

                if(isFinish) UpdateFSRelatedJobExecution(parentId);

                return row > 0;
                //if (status == (int)JobStatus.Finished || status == (int)JobStatus.FinishWithException)
                //{
                //    return context.JobMonitors.Where(a => a.Id == parentId && !finalSates.Contains(a.Status)).Update(u => new RMJobMonitor() { Status = status, LastUpdateTime = DateTime.UtcNow.Ticks, Progress = 100 }) > 0;
                //}
                //return context.JobMonitors.Where(a => a.Id == parentId && !finalSates.Contains(a.Status)).Update(u => new RMJobMonitor() { Status = status, LastUpdateTime = DateTime.UtcNow.Ticks }) > 0;
            }
        }
        
        private void UpdateGControlJobStatus(string id, RMDbContext context, JobStatus status, DateTime dateTime)
        {
            var result = context.JobMonitors.FirstOrDefault(job => job.Id == id)!;
            var jobType = result.JobType;
            if (_jobTypesAssociateWithGControl.Contains((JobType)jobType) && _tenantService.HasInitGControlPlatForm().Result)
            {
                var gControlJobStatus = status.ConvertToGControlJobStatus();
                var gControlJobId = Guid.TryParse(result.AdditionalInformation, out var gControlJobGuid) ? gControlJobGuid : Guid.Empty;
                logger.Info($"Updated GControl job status for jobId: {gControlJobId}, status: {status}");
                GControlPlatformJobService
                    .UpdatePlatformJob(gControlJobId, gControlJobStatus, dateTime)
                    .GetAwaiter()
                    .GetResult();
            }

        }

        private void UpdateFSRelatedJobExecution(string jobId)
        {
            try
            {
                if (jobId.IsNullOrEmpty()) return;
                FSConnectionRelatedJobInfoDao.UpdateRelatedJobExecutionInfoAsync(jobId).GetAwaiter().GetResult();
                logger.Info($"Updated FS connection related job execution info, JobId: {jobId}.");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to update FS related job execution info for jobId: {jobId}, exception: {ex}");
            }
        }

        public bool UpdateStatus(string id, int status, long dateTime, string comments = null)
        {
            string sql = "update {0}.RMSubJobs set Status = @status, LastUpdateTime = @dateTime " + ( comments == null ? "" : " , Comment = @comment ") + " where Id = @id";
            int row = 0;
            using (RMDbContext context = GetNewContext())
            {
                //return context.RMSubJobs.Where(a => a.Id == id).Update(c => new RMSubJob() { Status = status, LastUpdateTime = dateTime }) > 0;
                if (comments != null)
                {
                    row = context.Database.ExecuteSqlCommand(string.Format(sql, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)),
                        new SqlParameter("status", status),
                        new SqlParameter("dateTime", dateTime),
                        new SqlParameter("comment", comments),
                        new SqlParameter("id", id));
                }
                else
                {
                    row = context.Database.ExecuteSqlCommand(string.Format(sql, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)),
                        new SqlParameter("status", status),
                        new SqlParameter("dateTime", dateTime),
                        new SqlParameter("id", id));
                }
                return row > 0;
            }
        }

        public bool UpdateSubJobCount(string parentId, int subJobCount)
        {
            string sql = "update {0}.RMJobMonitors set SubJobCount = @subJobCount where Id = @parentId";
            using (RMDbContext context = GetNewContext())
            {
                //return context.JobMonitors.Where(a => a.Id == parentId).Update(c => new RMJobMonitor() {  SubJobCount = subJobCount }) > 0;
                int row = context.Database.ExecuteSqlCommand(string.Format(sql, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)),
                     new SqlParameter("subJobCount", subJobCount),
                    new SqlParameter("parentId", parentId));
                return row > 0;
            }
        }

        public bool UpdateRunable(string id)
        {
            string sql = "update {0}.RMSubJobs set Runable = 1, LastUpdateTime = @dateTime where Id = @id";
            using (RMDbContext context = GetNewContext())
            {
                int row = context.Database.ExecuteSqlCommand(string.Format(sql, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)),
                    new SqlParameter("dateTime", DateTime.UtcNow.Ticks),
                    new SqlParameter("id", id));
                return row > 0;
            }
        }

        public bool UpdateRunable(string id, int runnable, bool updateState = false)
        {
            //更新到CanRun, 要求原始状态是Waiting;  更新到Running, 原始状态需要是CanRun.
            int oldRunable = RecordsConstants.SubJob_Runnable_Waiting;
            if (runnable == RecordsConstants.SubJob_Runnable_Runing)
            {
                oldRunable = RecordsConstants.SubJob_Runnable_CanRun;
            }
            string sql = "update {0}.RMSubJobs set Runable = @runable, LastUpdateTime = @dateTime where Id = @id and Runable = @oldRunable";
            if (updateState)
            {
                sql = "update {0}.RMSubJobs set Runable = @runable, LastUpdateTime = @dateTime, Status = 1  where Id = @id and Runable = @oldRunable";
            }
            using (RMDbContext context = GetNewContext())
            {
                int row = context.Database.ExecuteSqlCommand(string.Format(sql, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)),
                    new SqlParameter("runable", runnable),
                    new SqlParameter("dateTime", DateTime.UtcNow.Ticks),
                    new SqlParameter("oldRunable", oldRunable),
                    new SqlParameter("id", id));
                return row > 0;
            }
        }
        /// <summary>
        /// 获取可以发送的子job, runnable状态为CanRun
        /// </summary>
        /// <returns></returns>
        public List<RMSubJob> GetRunableSubJobList()
        {
            using (RMDbContext context = GetNewContext())
            {
                int waitingState = (int)JobStatus.Wait;
                int progressState = (int)JobStatus.InProgress;
                List<RMSubJob> idList = context.RMSubJobs.Where(a => a.Runable == RecordsConstants.SubJob_Runnable_CanRun && (a.Status == waitingState || a.Status == progressState)).OrderByDescending(a => a.LastUpdateTime).ToList();
                return idList;
            }
        }

        public async Task<List<RMSubJob>> GetRunningAndRunnableSubJobListAsync(string o365TenantId, params JobType[] jobTypes)
        {
            using (RMDbContext context = GetNewContext())
            {
                var intJobTypes = jobTypes.ConvertAll(item => (int)item);
                List<RMSubJob> idList = await context.RMSubJobs.Where(item => item.O365TenantId == o365TenantId
                && intJobTypes.Contains(item.JobType)
                && (item.Status == (int)JobStatus.Wait || item.Status == (int)JobStatus.InProgress)
                && (item.Runable == RecordsConstants.SubJob_Runnable_CanRun || item.Runable == RecordsConstants.SubJob_Runnable_Runing)).ToListAsync();
                return idList;
            }
        }

        public async Task<List<RMSubJob>> GetRunningAndRunnableSubJobListAsync(params JobType[] jobTypes)
        {
            using (RMDbContext context = GetNewContext())
            {
                var intJobTypes = jobTypes.ConvertAll(item => (int)item);
                List<RMSubJob> idList = await context.RMSubJobs.Where(item => intJobTypes.Contains(item.JobType)
                && (item.Status == (int)JobStatus.Wait || item.Status == (int)JobStatus.InProgress)
                && (item.Runable == RecordsConstants.SubJob_Runnable_CanRun || item.Runable == RecordsConstants.SubJob_Runnable_Runing)).ToListAsync();
                return idList;
            }
        }



        public string GetJobContextSettingByJobId(string jobId)
        {
            using(var context = GetNewContext())
            {
                return context.JobContexts.FirstOrDefault(item => item.JobId == jobId)?.Settings;
            }
        }

        public string GetParentJobBySubJobId(string subJobId)
        {
            using (var context = GetNewContext())
            {
                return context.RMSubJobs.FirstOrDefault(item => item.Id == subJobId)?.ParentId;
            }
        }

        //Memory overflow problems may occur. A temporary solution will be released in June.
        //And it will be optimized in the next release.
        public List<RMJobContext> GetJobContextSettingByJobIds(List<string> jobIds)
        {
            using (var context = GetNewContext())
            {
                return context.JobContexts.Where(item => jobIds.Contains(item.JobId)).ToList();
            }
        }

        public async Task<List<RMJobContext>> PageQueryJobContextByMainJobId(string mainJobId, int page, int size)
        {
            using (var context = GetNewContext())
            {
                return await context.JobContexts
                    .Where(context => context.MainJobId == mainJobId)
                    .OrderBy(context => context.JobId)
                    .Skip((page -1) * size)
                    .Take(size)
                    .ToListAsync();
            }
        }

        public string GetJobContextSettingByMainJobId(string jobId)
        {
            using (var context = GetNewContext())
            {
                return context.JobContexts.FirstOrDefault(item => item.MainJobId == jobId)?.Settings;
            }
        }

        public Dictionary<JobType, int> GetRunningAndRunnableSubJobCount()
        {
            var result = new Dictionary<JobType, int>();
            using (RMDbContext context = GetNewContext())
            {
                int waitingState = (int)JobStatus.Wait;
                int progressState = (int)JobStatus.InProgress;
                var jobTypes = context.RMSubJobs
                    .Where(a => (a.Runable == RecordsConstants.SubJob_Runnable_Runing || a.Runable == RecordsConstants.SubJob_Runnable_CanRun) && (a.Status == waitingState || a.Status == progressState))
                    .Select(o => o.JobType).GroupBy(o => o).ToList();

                foreach (var jobtype in jobTypes)
                {
                    result[(JobType)jobtype.Key] = jobtype.Count();
                }
            }

            return result;
        }

        public Dictionary<string, int> GetRunningAndRunnableMainJobIdAndSubJobCountByJobType(params JobType[] jobTypes)
        {
            using var context = GetNewContext();
            var intJobTypes = jobTypes.ConvertAll(item => (int)item);
            return context.RMSubJobs.Where(item => intJobTypes.Contains(item.JobType)
            && (item.Status == (int)JobStatus.Wait || item.Status == (int)JobStatus.InProgress)
            && (item.Runable == RecordsConstants.SubJob_Runnable_CanRun || item.Runable == RecordsConstants.SubJob_Runnable_Runing))
            .GroupBy(item => item.ParentId).ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<int> GetRunningAndRunnableSubJobCountAsync(string o365TenantId, params JobType[] jobTypes)
        {
            using var context = GetNewContext();
            var intJobTypes = jobTypes.ConvertAll(item => (int)item);
            return await context.RMSubJobs.Where(item => item.O365TenantId == o365TenantId
            && intJobTypes.Contains(item.JobType)
            && (item.Status == (int)JobStatus.Wait || item.Status == (int)JobStatus.InProgress)
            && (item.Runable == RecordsConstants.SubJob_Runnable_CanRun || item.Runable == RecordsConstants.SubJob_Runnable_Runing)).CountAsync();
        }

        public async Task<Dictionary<string, int>> GetRunningAndRunnableMainJobIdAndSubJobCountAsync(string o365TenantId, params JobType[] jobTypes)
        {
            using var context = GetNewContext();
            var intJobTypes = (jobTypes ?? Array.Empty<JobType>()).Select(item => (int)item).Distinct().ToList();
            if (intJobTypes.Count == 0)
            {
                return new Dictionary<string, int>();
            }

            if (string.IsNullOrWhiteSpace(o365TenantId))
            {
                logger.Warn($"o365TenantId should not be null when calling GetRunningAndRunnableMainJobIdAndSubJobCountAsync. Returning empty dictionary.");
                return new Dictionary<string, int>();
            }

            var schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
            var statusValues = new List<int> { (int)JobStatus.Wait, (int)JobStatus.InProgress };
            var runnableValues = new List<int> { RecordsConstants.SubJob_Runnable_CanRun, RecordsConstants.SubJob_Runnable_Runing };

            var jobTypeInClause = DatabaseUtility.BuildInClause(intJobTypes, out var jobTypeParams);
            var statusInClause = DatabaseUtility.BuildInClause(statusValues, out var statusParams);
            var runnableInClause = DatabaseUtility.BuildInClause(runnableValues, out var runnableParams);

            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@o365TenantId", (object)o365TenantId)
            };
            sqlParams.AddRange(jobTypeParams);
            sqlParams.AddRange(statusParams);
            sqlParams.AddRange(runnableParams);

            var sql = $@"
                    SELECT ParentId, COUNT(1) AS SubJobCount
                    FROM {schemaName}.RMSubJobs
                    WHERE Runable IN {runnableInClause}
                    AND Status IN {statusInClause}
                    AND JobType IN {jobTypeInClause}
                    AND O365TenantId = @o365TenantId
                    GROUP BY ParentId";

            var countRows = await context.Database
                .SqlQuery<ParentSubJobCountInfo>(sql, sqlParams.ToArray())
                .ToListAsync();

            return countRows.ToDictionary(row => row.ParentId, row => row.SubJobCount);
        }

        public async Task<bool> HasWaitingSubJobCountAsync(params JobType[] jobTypes)
        {
            using var context = GetNewContext();
            var intJobTypes = jobTypes.ConvertAll(item => (int)item);
            return await context.RMSubJobs.AnyAsync(item => intJobTypes.Contains(item.JobType) && item.Runable == RecordsConstants.SubJob_Runnable_Waiting);
        }

        public async Task<bool> HasWaitingSubJobCountExpectJobTypesAsync(params JobType[] jobTypes)
        {
            using var context = GetNewContext();
            var intJobTypes = jobTypes.ConvertAll(item => (int)item);
            return await context.RMSubJobs.AnyAsync(item => !intJobTypes.Contains(item.JobType) && item.Runable == RecordsConstants.SubJob_Runnable_Waiting);
        }

        public async Task<Dictionary<JobType, List<RMSubJob>>> GetWaitingSubJobsGroups(List<int> expectJobTypes)
        {
            using var context = GetNewContext();
            var rows = await context.RMSubJobs
                .AsNoTracking()
                .Where(item => item.Runable == RecordsConstants.SubJob_Runnable_Waiting
                                && (item.Status == (int)JobStatus.Wait || item.Status == (int)JobStatus.InProgress)
                                && !expectJobTypes.Contains(item.JobType))
                .OrderBy(item => item.StartTime)
                .Select(item => new { item.Id, item.ParentId, item.JobType })
                .ToListAsync();

            return rows
                .GroupBy(item => (JobType)item.JobType)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => new RMSubJob { Id = x.Id, ParentId = x.ParentId, JobType = x.JobType }).ToList()
                );
        }

        public async Task<List<string>> GetSubJobsParentIdsAsync(int jobType)
        {
            using var context = GetNewContext();
            return await context.RMSubJobs
                .AsNoTracking()
                .Where(item => (item.Status == (int)JobStatus.Wait || item.Status == (int)JobStatus.InProgress) && item.JobType == jobType)
                .Select(item => item.ParentId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<bool> UpdateSubJobToRunnableByIdsAsync(List<string> subJobIds)
        {
            using var context = GetNewContext();
            var subJobs = await context.RMSubJobs.Where(subJob => subJobIds.Contains(subJob.Id)).ToListAsync();
            subJobs.ForEach(subJob =>
            {
                subJob.Runable = RecordsConstants.SubJob_Runnable_CanRun;
                subJob.LastUpdateTime = DateTime.UtcNow.Ticks;
            });
            context.RMSubJobs.AddOrUpdate([.. subJobs]);
            var affectedRows = await context.SaveChangesAsync();
            return affectedRows > 0;
        }

        public async Task<int> UpdateWaitingSubJobToRunnableAsync(string o365TenantId, int maxRunSubJobCount, int runnableSubJobCount, Dictionary<string, int> runningMainJobsAndSubJobCountDict, params JobType[] jobTypes)
        {
            using var context = GetNewContext();
            var intJobTypes = jobTypes.ConvertAll(item => (int)item);
            int perParentFetchSize = maxRunSubJobCount;

            // Query waiting parent ids first, then load at most 100 waiting sub jobs for each parent once.
            var waitingParentIds = context.RMSubJobs
                .AsNoTracking()
                .Where(subJob => subJob.Runable == RecordsConstants.SubJob_Runnable_Waiting
                                  && (subJob.Status == (int)JobStatus.Wait || subJob.Status == (int)JobStatus.InProgress)
                                  && intJobTypes.Contains(subJob.JobType)
                                  && subJob.O365TenantId == o365TenantId)
                .Select(subJob => subJob.ParentId)
                .Distinct()
                .ToList();

            // Only consider main jobs that actually have waiting sub jobs; order by priority desc then start time asc
            var orderedMainJobs = context.JobMonitors
                .Where(j => waitingParentIds.Contains(j.Id))
                .OrderByDescending(j => j.JobPriority)
                .ThenBy(j => j.StartTime)
                .Select(j => new { j.Id, j.JobPriority })
                .ToList();

            var parentQueueCache = new Dictionary<string, Queue<WaitingSubJobInfo>>(StringComparer.OrdinalIgnoreCase);
            var exhaustedParentIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            Queue<WaitingSubJobInfo> GetOrLoadQueue(string parentId)
            {
                if (string.IsNullOrWhiteSpace(parentId))
                {
                    return null;
                }

                if (parentQueueCache.TryGetValue(parentId, out var existingQueue))
                {
                    return existingQueue;
                }

                if (exhaustedParentIds.Contains(parentId))
                {
                    return null;
                }

                var waitingRows = context.RMSubJobs
                    .AsNoTracking()
                    .Where(subJob => subJob.ParentId == parentId
                                      && subJob.Runable == RecordsConstants.SubJob_Runnable_Waiting
                                      && (subJob.Status == (int)JobStatus.Wait || subJob.Status == (int)JobStatus.InProgress)
                                      && intJobTypes.Contains(subJob.JobType)
                                      && subJob.O365TenantId == o365TenantId)
                    .OrderBy(subJob => subJob.Id)
                    .Select(subJob => new WaitingSubJobInfo
                    {
                        Id = subJob.Id,
                        ParentId = subJob.ParentId
                    })
                    .Take(perParentFetchSize)
                    .ToList();

                if (waitingRows.Count == 0)
                {
                    exhaustedParentIds.Add(parentId);
                    return null;
                }

                var loadedQueue = new Queue<WaitingSubJobInfo>(waitingRows);
                parentQueueCache[parentId] = loadedQueue;
                return loadedQueue;
            }

            var readyUpdateSubJobIds = new List<string>();

            // 1. Ensure each main job has at least one running sub job; if not, promote one waiting sub job
            if (runnableSubJobCount > 0)
            {
                foreach (var mainJob in orderedMainJobs)
                {
                    if (runnableSubJobCount <= 0)
                    {
                        break;
                    }

                    var jobId = mainJob.Id;
                    var hasRunning = runningMainJobsAndSubJobCountDict.TryGetValue(jobId, out var runningCount) && runningCount > 0;
                    if (hasRunning)
                    {
                        continue;
                    }

                    var q = GetOrLoadQueue(jobId);
                    if (q != null && q.Count > 0)
                    {
                        readyUpdateSubJobIds.Add(q.Dequeue().Id);
                        runnableSubJobCount--;
                    }
                }
            }

            // 2/3/4. Distribute remaining capacity by priority group using queue-based round robin
            if (runnableSubJobCount > 0)
            {
                var groupedByPriority = orderedMainJobs
                    .GroupBy(j => j.JobPriority)
                    .OrderByDescending(g => g.Key)
                    .ToList();

                foreach (var priorityGroup in groupedByPriority)
                {
                    if (runnableSubJobCount <= 0)
                    {
                        break;
                    }

                    var groupJobIds = priorityGroup.Select(j => j.Id).ToList();
                    if (groupJobIds.Count == 0)
                    {
                        continue;
                    }

                    if (groupJobIds.Count == 1)
                    {
                        var jobId = groupJobIds[0];
                        var q = GetOrLoadQueue(jobId);
                        if (q != null && q.Count > 0)
                        {
                            var takeCount = Math.Min(runnableSubJobCount, q.Count);
                            for (var i = 0; i < takeCount; i++)
                            {
                                readyUpdateSubJobIds.Add(q.Dequeue().Id);
                            }
                            runnableSubJobCount -= takeCount;
                        }
                        continue;
                    }

                    var anyAvailable = true;
                    while (runnableSubJobCount > 0 && anyAvailable)
                    {
                        anyAvailable = false;
                        foreach (var jobId in groupJobIds)
                        {
                            if (runnableSubJobCount <= 0)
                            {
                                break;
                            }

                            var q = GetOrLoadQueue(jobId);
                            if (q != null && q.Count > 0)
                            {
                                readyUpdateSubJobIds.Add(q.Dequeue().Id);
                                runnableSubJobCount -= 1;
                                anyAvailable = true;
                            }
                        }
                    }
                }
            }

            var updated = await UpdateSubJobToRunnableByIdsAsync(readyUpdateSubJobIds);
            return updated ? readyUpdateSubJobIds.Count : 0;
        }

        public void ChangeRunnableWiating2CanRun(JobType jobType, int count)
        {
            using (RMDbContext context = GetNewContext())
            {
                int waitingState = (int)JobStatus.Wait;
                int jobTypeState = (int)jobType;

                // Order by corresponding main job priority (desc), then sub job start time (asc)
                var waitingList = (from sj in context.RMSubJobs
                                   join jm in context.JobMonitors on sj.ParentId equals jm.Id
                                   where sj.JobType == jobTypeState
                                         && sj.Runable == RecordsConstants.SubJob_Runnable_Waiting
                                         && sj.Status == waitingState
                                   orderby jm.JobPriority descending, sj.StartTime ascending
                                   select sj)
                                   .Take(count)
                                   .ToList();

                if (waitingList.Count > 0)
                {
                    waitingList.ForEach(o =>
                    {
                        o.Runable = RecordsConstants.SubJob_Runnable_CanRun;
                        o.LastUpdateTime = DateTime.UtcNow.Ticks;
                    });
                    context.SaveChanges();
                }
            }
        }
        public List<RMSubJob> GetFailedSubJobs(Expression<Func<RMSubJob, bool>> whereLambda = null)
        {
            using (RMDbContext context = GetNewContext())
            {
                List<RMSubJob> idList = null;
                int failed = (int)JobStatus.Failed;
                int finishWithException = (int)JobStatus.FinishWithException;
                if (whereLambda != null)
                {
                    idList = context.RMSubJobs.Where(a => (a.Status == failed || a.Status == finishWithException) && JobTypeConstants.SOBackupjobTypes.Contains((JobType)a.JobType) && a.HasCheckedBackupFailed == (int)HasCheckedBackupStatus.CheckedFit).Where(whereLambda).OrderByDescending(a => a.StartTime).ToList();
                }
                else
                {
                    idList = context.RMSubJobs.Where(a => (a.Status == failed || a.Status == finishWithException) && JobTypeConstants.SOBackupjobTypes.Contains((JobType)a.JobType) && a.HasCheckedBackupFailed == (int)HasCheckedBackupStatus.CheckedFit).OrderByDescending(a => a.StartTime).ToList();
                }
                return idList;
            }
        }
        public List<RMSubJob> GetNotCheckedFailedSubJobs()
        {
            using (RMDbContext context = GetNewContext())
            {
                int failed = (int)JobStatus.Failed;
                int finishWithException = (int)JobStatus.FinishWithException;
                List<RMSubJob> idList = context.RMSubJobs.Where(a => (a.Status == failed || a.Status == finishWithException) && JobTypeConstants.SOBackupjobTypes.Contains((JobType)a.JobType) && a.HasCheckedBackupFailed == (int)HasCheckedBackupStatus.None).OrderByDescending(a => a.StartTime).ToList();
                return idList;
            }
        }
        public List<RMSubJob> GetSubJobsByIds(List<string> subJobIds)
        {
            using (RMDbContext context = GetNewContext())
            {
                int failed = (int)JobStatus.Failed;
                int finishWithException = (int)JobStatus.FinishWithException;
                List<RMSubJob> idList = context.RMSubJobs.Where(a => (a.Status == failed || a.Status == finishWithException) && JobTypeConstants.SOBackupjobTypes.Contains((JobType)a.JobType) && a.HasCheckedBackupFailed == (int)HasCheckedBackupStatus.CheckedFit && subJobIds.Contains(a.Id)).OrderByDescending(a => a.StartTime).ToList();
                return idList;
            }
        }

        public List<string> GetSubJobScopesByMainJobId(string mainJobId, params string[] searchScope)
        {
            using (RMDbContext context = GetNewContext())
            {
                const int searchScopePageSize = 2000;
                var searchScopeSet = new HashSet<string>(
                    (searchScope ?? Array.Empty<string>()).Where(scope => !string.IsNullOrWhiteSpace(scope)),
                    StringComparer.OrdinalIgnoreCase);
                bool needScopeFilter = searchScopeSet.Count > 0;

                var matchedScopes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                string lastJobId = null;

                while (true)
                {
                    IQueryable<RMSubJob> query = context.RMSubJobs
                        .AsNoTracking()
                        .Where(a => a.ParentId == mainJobId);

                    if (!string.IsNullOrWhiteSpace(lastJobId))
                    {
                        query = query.Where(a => string.Compare(a.Id, lastJobId) > 0);
                    }

                    var batchResult = query
                        .OrderBy(a => a.Id)
                        .Select(a => new
                        {
                            a.Id,
                            a.String1
                        })
                        .Take(searchScopePageSize)
                        .ToList();

                    if (batchResult.Count == 0)
                    {
                        break;
                    }

                    foreach (var item in batchResult)
                    {
                        var scope = item.String1;
                        if (string.IsNullOrWhiteSpace(scope))
                        {
                            continue;
                        }

                        if (!needScopeFilter || searchScopeSet.Contains(scope))
                        {
                            matchedScopes.Add(scope);
                        }
                    }

                    lastJobId = batchResult[batchResult.Count - 1].Id;
                    if (batchResult.Count < searchScopePageSize)
                    {
                        break;
                    }
                }

                return matchedScopes.ToList();
            }
        }

        public List<string> GetRunningArchiverJobsScopes(List<JobType> types, params string[] excludeJobIds)
        {
            List<string> excludeJobIdList = excludeJobIds?.ToList() ?? new List<string>();
            List<string> jobScopes = new List<string>();
            using (RMDbContext context = GetNewContext())
            {
                int waitingState = (int)JobStatus.Wait;
                int runningState = (int)JobStatus.InProgress;
                int stoppingState = (int)JobStatus.Stopping;
                List<int> jobTypeStates = types.Select(t => (int)t).ToList();
                var scopes = context.RMSubJobs.Where(a => jobTypeStates.Contains(a.JobType) && !excludeJobIdList.Contains(a.ParentId) && (a.Status == waitingState || a.Status == runningState || a.Status == stoppingState)).Select(j => j.String1).ToList();
                if (scopes != null && scopes.Count > 0)
                {
                    jobScopes = scopes;
                }
            }
            return jobScopes;
        }

        private void LogSQL(string sql)
        {
            //logger.Debug(sql);
        }

        public List<string> GetRunningMoveSubJobByDest(string destUrl, bool includeWaiting)
        {
            int running = (int)JobStatus.InProgress;
            int moveJobType = (int)JobType.RecordsExplorerMove;
            List<string> idList = new List<string>();
            using (RMDbContext context = GetNewContext())
            {
                context.Database.Log = LogSQL;
                if (includeWaiting)
                {
                    idList = context.RMSubJobs.Where(a => (a.Status == running || a.Status == 0) && a.JobType == moveJobType && a.String1 == destUrl).Select(s => s.Id).ToList();
                }else
                {
                    idList = context.RMSubJobs.Where(a => a.Status == running && a.JobType == moveJobType && a.String1 == destUrl).Select(s=>s.Id).ToList();
                }
                logger.Info("Running include waiting ? {0}, result count is {1}", includeWaiting, idList.Count);
                return idList;
            }
        }

        public RMSubJob GetOneWaitingSubJob(string mainJobId)
        {
            using (RMDbContext context = GetNewContext())
            {
                int waitingState = (int)JobStatus.Wait;
                var res = context.RMSubJobs.Where(a => a.Status == waitingState && a.Runable == RecordsConstants.SubJob_Runnable_Waiting && a.ParentId == mainJobId).OrderBy(a => a.StartTime).FirstOrDefault();
                return res;
            }
        }

        public string GetOneWaitingSubJobId(string mainJobId)
        {
            using (RMDbContext context = GetNewContext())
            {
                int waitingState = (int)JobStatus.Wait;
                string jobId = context.RMSubJobs.Where(a => a.Status == waitingState && a.Runable == RecordsConstants.SubJob_Runnable_Waiting && a.ParentId == mainJobId).OrderBy(a => a.StartTime).Select(a=>a.Id).FirstOrDefault();
                return jobId;
            }
        }
        public string GetOtherOneWaitingPermissionSubJobId(string mainJobId)
        {
            using (RMDbContext context = GetNewContext())
            {
                int waitingState = (int)JobStatus.Wait;
                int jobType = (int)JobType.PhysicalSetPermission;
                string jobId = context.RMSubJobs.Where(a => a.Status == waitingState && a.Runable == RecordsConstants.SubJob_Runnable_Waiting && !string.IsNullOrEmpty(a.ParentId) && a.ParentId != mainJobId && a.JobType == jobType).OrderBy(a => a.StartTime).Select(a => a.Id).FirstOrDefault();
                return jobId;
            }
        }
        /// <summary>
        /// 获取所有子job的状态
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public bool HasInProgressSubJobByParent(string parentId)
        {
            using (RMDbContext context = GetNewContext())
            {
                int inProgressState = (int)JobStatus.InProgress;
                string subJobId = context.RMSubJobs
                    .Where(a => a.ParentId == parentId && a.Status == inProgressState)
                    .Select(a => a.Id)
                    .FirstOrDefault();

                return !string.IsNullOrEmpty(subJobId);
            }
        }

        public List<int> GetAllStatesByParent(string parentId)
        {
            using (RMDbContext context = GetNewContext())
            {
                return context.RMSubJobs.Where(a => a.ParentId == parentId).Select(s => s.Status).Distinct().ToList();
            }
        }

        public List<string> GetAllSubJobIds(string parentId, int[] states)
        {
            using (RMDbContext context = GetNewContext())
            {
                if (states == null || states.Length == 0)
                {
                    return context.RMSubJobs.Where(a => a.ParentId == parentId).Select(s => s.Id).ToList();
                }
                return context.RMSubJobs.Where(a => a.ParentId == parentId && Enumerable.Contains(states, a.Status)).Select(s => s.Id).ToList();
            }
        }
        public List<RMSubJob> GetAllSubJobByMainJobId(string parentId)
        {
            using (RMDbContext context = GetNewContext())
            {
                return context.RMSubJobs.Where(a => a.ParentId == parentId).ToList();
            }
        }

        public List<RMSubJob> QueryAllSubJobs(COPSubJobRequest request)
        {
            var searchKey = request.SearchKey;
            var statusJobs = request.SubJobStatusFilters;
            using (RMDbContext context = GetNewContext())
            {
                IQueryable<RMSubJob> query = context.RMSubJobs
                    .Where(x => x.ParentId == request.JobId);

                if (statusJobs?.Length > 0)
                {
                    query = query.Where(x => Enumerable.Contains(statusJobs, x.Status));
                }

                if (!string.IsNullOrWhiteSpace(searchKey))
                {
                    searchKey = searchKey.Trim();

                    query = query.Where(x => x.Id.Contains(searchKey));
                }

                return query.OrderBy(x => x.Id).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
            }
        }
        public async Task<List<RMSubJob>> GetAllSubJobByMainJobIdAsync(string parentId, int[] states)
        {
            using (RMDbContext context = GetNewContext())
            {
                if (states == null || states.Length == 0)
                {
                    return await context.RMSubJobs.Where(a => a.ParentId == parentId).ToListAsync();
                }
                return await context.RMSubJobs.Where(a => a.ParentId == parentId && Enumerable.Contains(states, a.Status)).ToListAsync();
            }
        }

        public List<string> GetAllExcludeSubJobIds(string parentId, int[] states)
        {
            using (RMDbContext context = GetNewContext())
            {
                if (states == null || states.Length == 0)
                {
                    return context.RMSubJobs
                        .Where(a => a.ParentId == parentId && a.Runable == RecordsConstants.SubJob_Runnable_Exclude)
                        .Select(s => s.Id).ToList();
                }
                return context.RMSubJobs
                    .Where(a => a.ParentId == parentId && Enumerable.Contains(states, a.Status) && a.Runable == RecordsConstants.SubJob_Runnable_Exclude)
                    .Select(s => s.Id).ToList();
            }
        }
        public List<string> GetAllSubJobString1sByParentId(string parentId)
        {
            using (RMDbContext context = GetNewContext())
            {
                return context.RMSubJobs.Where(a => a.ParentId == parentId).Select(s => s.String1).ToList();
            }
        }

        public Dictionary<string, string> GetAllSubJobSiteIdsByParentId(string parentId)
        {
            using (RMDbContext context = GetNewContext())
            {
                return context.RMSubJobs.Where(a => a.ParentId == parentId).GroupBy(a => a.Id).ToDictionary(s => s.Key, s => s.First().SiteId);
            }
        }


        public List<RMSubJob> GetOneSubJobByParentIds(List<string> parentIds)
        {
            using (RMDbContext context = GetNewContext())
            {
                if (parentIds == null || parentIds.Count == 0)
                {
                    return new List<RMSubJob>();
                }

                // Optimize: push ordering and grouping to the database, avoid loading full lists per group.
                var result = context.RMSubJobs
                    .AsNoTracking()
                    .Where(a => parentIds.Contains(a.ParentId))
                    .OrderBy(a => a.StartTime)
                    .GroupBy(a => a.ParentId)
                    .Select(g => g.FirstOrDefault())
                    .ToList();

                return result;
            }
        }

        public string CreateJob(RMSubJob sub)
        {
            using (RMDbContext context = GetNewContext())
            {
                context.Database.CommandTimeout = 600;
                sub.LastUpdateTime = DateTime.UtcNow.Ticks;  //防止没发送就被Check超时
                context.RMSubJobs.Add(sub);
                if (sub.JobContext != null)
                {
                    if (!string.IsNullOrEmpty(sub.JobContext.JobId) && sub.JobContext.JobId.Contains("_"))
                    {
                        sub.JobContext.MainJobId = sub.JobContext.JobId.Split('_')[0];
                    }
                    context.JobContexts.Add(sub.JobContext);
                }
                context.SaveChanges();
            }

            return sub.Id;
        }

        public void BulkCreateJobs(IEnumerable<RMSubJob> subJobs, int batchSize = 5000)
        {
            if (subJobs == null)
            {
                return;
            }

            batchSize = Math.Max(1, batchSize);

            var batch = new List<RMSubJob>(batchSize);
            var batchContexts = new List<RMJobContext>(batchSize);
            var totalProcessedCount = 0;
            var batchStartIndex = 0;

            foreach (var subJob in subJobs)
            {
                if (subJob == null)
                {
                    continue;
                }

                if (batch.Count == 0)
                {
                    batchStartIndex = totalProcessedCount;
                }

                batch.Add(subJob);
                if (subJob.JobContext != null)
                {
                    batchContexts.Add(subJob.JobContext);
                }
                totalProcessedCount++;

                if (batch.Count < batchSize)
                {
                    continue;
                }

                PrepareSubJobsForInsert(batch);
                ExecuteBulkInsertBatchWithRetry(batch, batchContexts, batchStartIndex, totalProcessedCount);
                batch.Clear();
                batchContexts.Clear();
            }

            if (batch.Count > 0)
            {
                PrepareSubJobsForInsert(batch);
                ExecuteBulkInsertBatchWithRetry(batch, batchContexts, batchStartIndex, totalProcessedCount);
            }
        }

        private void ExecuteBulkInsertBatchWithRetry(List<RMSubJob> batch, List<RMJobContext> contexts, int batchStartIndex, int totalCount)
        {
            using (RMDbContext context = GetNewContext())
            {
                context.Database.CommandTimeout = 600;
                var sqlConnection = context.Database.Connection as SqlConnection;
                if (sqlConnection == null)
                {
                    throw new InvalidOperationException("Database connection is not a SqlConnection.");
                }

                var shouldCloseConnection = sqlConnection.State != ConnectionState.Open;
                if (shouldCloseConnection)
                {
                    sqlConnection.Open();
                }

                var schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                try
                {
                    ExecuteBulkInsertSubJobsWithRetry(sqlConnection, schemaName, batch, batchStartIndex, totalCount);
                    ExecuteBulkInsertContextsWithRetry(sqlConnection, schemaName, contexts, batchStartIndex, totalCount);
                }
                finally
                {
                    if (shouldCloseConnection && sqlConnection.State != ConnectionState.Closed)
                    {
                        sqlConnection.Close();
                    }
                }
            }
        }

        private void ExecuteBulkInsertSubJobsWithRetry(SqlConnection sqlConnection, string schemaName, List<RMSubJob> batch, int batchStartIndex, int totalCount)
        {
            var attempt = 0;
            while (true)
            {
                attempt++;
                try
                {
                    ExecuteBulkInsertSubJobs(sqlConnection, schemaName, batch);
                    return;
                }
                catch (Exception ex) when (attempt < BulkInsertRetryCount && ShouldRetryBulkInsert(ex))
                {
                    var shouldReopenConnection = ShouldReopenConnectionForRetry(ex, sqlConnection);
                    if (shouldReopenConnection)
                    {
                        ReopenConnection(sqlConnection);
                    }

                    logger.Warn("BulkCreateJobs subjob insert failed and will retry. Attempt:{0}/{1}, BatchStart:{2}, BatchSize:{3}, TotalCount:{4}, Error:{5}",
                        attempt,
                        BulkInsertRetryCount,
                        batchStartIndex,
                        batch.Count,
                        totalCount,
                        ex.ToString());
                    Thread.Sleep(TimeSpan.FromSeconds(BulkInsertRetryDelaySeconds));
                }
                catch (Exception ex)
                {
                    logger.Error("BulkCreateJobs subjob insert failed. Attempt:{0}/{1}, BatchStart:{2}, BatchSize:{3}, TotalCount:{4}, Error:{5}",
                        attempt,
                        BulkInsertRetryCount,
                        batchStartIndex,
                        batch.Count,
                        totalCount,
                        ex.ToString());
                    throw;
                }
            }
        }

        private void ExecuteBulkInsertContextsWithRetry(SqlConnection sqlConnection, string schemaName, List<RMJobContext> contexts, int batchStartIndex, int totalCount)
        {
            if (contexts.Count == 0)
            {
                return;
            }

            var attempt = 0;
            while (true)
            {
                attempt++;
                try
                {
                    ExecuteBulkInsertContexts(sqlConnection, schemaName, contexts);
                    return;
                }
                catch (Exception ex) when (attempt < BulkInsertRetryCount && ShouldRetryBulkInsert(ex))
                {
                    var shouldReopenConnection = ShouldReopenConnectionForRetry(ex, sqlConnection);
                    if (shouldReopenConnection)
                    {
                        ReopenConnection(sqlConnection);
                    }

                    logger.Warn("BulkCreateJobs context insert failed and will retry. Attempt:{0}/{1}, BatchStart:{2}, ContextSize:{3}, TotalCount:{4}, Error:{5}",
                        attempt,
                        BulkInsertRetryCount,
                        batchStartIndex,
                        contexts.Count,
                        totalCount,
                        ex.ToString());
                    Thread.Sleep(TimeSpan.FromSeconds(BulkInsertRetryDelaySeconds));
                }
                catch (Exception ex)
                {
                    logger.Error("BulkCreateJobs context insert failed. Attempt:{0}/{1}, BatchStart:{2}, ContextSize:{3}, TotalCount:{4}, Error:{5}",
                        attempt,
                        BulkInsertRetryCount,
                        batchStartIndex,
                        contexts.Count,
                        totalCount,
                        ex.ToString());
                    throw;
                }
            }
        }

        private void ExecuteBulkInsertSubJobs(SqlConnection sqlConnection, string schemaName, List<RMSubJob> batch)
        {
            using (var subJobBulkCopy = new SqlBulkCopy(sqlConnection)
            {
                DestinationTableName = $"{schemaName}.RMSubJobs",
                BatchSize = batch.Count,
                BulkCopyTimeout = 600
            })
            {
                var subJobTable = BuildSubJobDataTable(batch);
                foreach (DataColumn column in subJobTable.Columns)
                {
                    subJobBulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }
                subJobBulkCopy.WriteToServer(subJobTable);
            }
        }

        private void ExecuteBulkInsertContexts(SqlConnection sqlConnection, string schemaName, List<RMJobContext> contexts)
        {
            if (contexts.Count == 0)
            {
                return;
            }

            using (var contextBulkCopy = new SqlBulkCopy(sqlConnection)
            {
                DestinationTableName = $"{schemaName}.RMJobContexts",
                BatchSize = contexts.Count,
                BulkCopyTimeout = 600
            })
            {
                var contextTable = BuildJobContextDataTable(contexts);
                foreach (DataColumn column in contextTable.Columns)
                {
                    contextBulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }
                contextBulkCopy.WriteToServer(contextTable);
            }
        }

        private static bool ShouldRetryBulkInsert(Exception ex)
        {
            if (ex == null)
            {
                return false;
            }

            if (ex is TimeoutException)
            {
                return true;
            }

            if (ex is SqlException sqlException)
            {
                foreach (SqlError error in sqlException.Errors)
                {
                    if (IsTransientSqlErrorCode(error.Number))
                    {
                        return true;
                    }
                }

                return false;
            }

            if (ex is AggregateException aggregateException)
            {
                return aggregateException.Flatten().InnerExceptions.Any(ShouldRetryBulkInsert);
            }

            return ShouldRetryBulkInsert(ex.InnerException);
        }

        private static bool ShouldReopenConnectionForRetry(Exception ex, SqlConnection sqlConnection)
        {
            if (sqlConnection == null)
            {
                return false;
            }

            if (sqlConnection.State == ConnectionState.Broken || sqlConnection.State == ConnectionState.Closed)
            {
                return true;
            }

            if (ex is SqlException sqlException)
            {
                foreach (SqlError error in sqlException.Errors)
                {
                    if (IsConnectionTransientSqlErrorCode(error.Number))
                    {
                        return true;
                    }
                }
            }

            if (ex is AggregateException aggregateException)
            {
                return aggregateException.Flatten().InnerExceptions.Any(inner => ShouldReopenConnectionForRetry(inner, sqlConnection));
            }

            return ex.InnerException != null && ShouldReopenConnectionForRetry(ex.InnerException, sqlConnection);
        }

        private static bool IsConnectionTransientSqlErrorCode(int errorCode)
        {
            switch (errorCode)
            {
                case 20:
                case 64:
                case 233:
                case 10053:
                case 10054:
                case 10060:
                case 11001:
                    return true;
                default:
                    return false;
            }
        }

        private static void ReopenConnection(SqlConnection sqlConnection)
        {
            if (sqlConnection.State != ConnectionState.Closed)
            {
                sqlConnection.Close();
            }

            sqlConnection.Open();
        }

        private static bool IsTransientSqlErrorCode(int errorCode)
        {
            switch (errorCode)
            {
                case -2:
                case 20:
                case 64:
                case 233:
                case 1205:
                case 4060:
                case 10928:
                case 10929:
                case 40197:
                case 40501:
                case 40613:
                case 10053:
                case 10054:
                case 10060:
                case 11001:
                    return true;
                default:
                    return false;
            }
        }

        private static void PrepareSubJobsForInsert(List<RMSubJob> subJobs)
        {
            var now = DateTime.UtcNow.Ticks;
            foreach (var subJob in subJobs)
            {
                subJob.LastUpdateTime = now;
                if (subJob.JobContext == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(subJob.JobContext.JobId) && subJob.JobContext.JobId.Contains("_"))
                {
                    subJob.JobContext.MainJobId = subJob.JobContext.JobId.Split('_')[0];
                }
            }
        }

        private static DataTable BuildSubJobDataTable(List<RMSubJob> subJobs)
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(string));
            table.Columns.Add("ParentId", typeof(string));
            table.Columns.Add("JobType", typeof(int));
            table.Columns.Add("StartTime", typeof(long));
            table.Columns.Add("EndTime", typeof(long));
            table.Columns.Add("Status", typeof(int));
            table.Columns.Add("Progress", typeof(double));
            table.Columns.Add("Weight", typeof(double));
            table.Columns.Add("Comment", typeof(string));
            table.Columns.Add("LastUpdateTime", typeof(long));
            table.Columns.Add("Runable", typeof(int));
            table.Columns.Add("AgentId", typeof(string));
            table.Columns.Add("FarmId", typeof(string));
            table.Columns.Add("String1", typeof(string));
            table.Columns.Add("O365TenantId", typeof(string));
            table.Columns.Add("SiteId", typeof(string));
            table.Columns.Add("DiscoveryAnalysisJobId", typeof(Guid));
            table.Columns.Add("HasCheckedBackupFailed", typeof(int));

            foreach (var job in subJobs)
            {
                table.Rows.Add(
                    job.Id,
                    (object)job.ParentId ?? DBNull.Value,
                    job.JobType,
                    job.StartTime,
                    job.EndTime,
                    job.Status,
                    job.Progress,
                    job.Weight,
                    (object)job.Comment ?? DBNull.Value,
                    job.LastUpdateTime,
                    job.Runable,
                    (object)job.AgentId ?? DBNull.Value,
                    (object)job.FarmId ?? DBNull.Value,
                    (object)job.String1 ?? DBNull.Value,
                    (object)job.O365TenantId ?? DBNull.Value,
                    (object)job.SiteId ?? DBNull.Value,
                    job.DiscoveryAnalysisJobId,
                    job.HasCheckedBackupFailed);
            }

            return table;
        }

        private static DataTable BuildJobContextDataTable(List<RMJobContext> contexts)
        {
            var table = new DataTable();
            table.Columns.Add("JobId", typeof(string));
            table.Columns.Add("Settings", typeof(string));
            table.Columns.Add("Content", typeof(string));
            table.Columns.Add("MainJobId", typeof(string));

            foreach (var context in contexts)
            {
                table.Rows.Add(
                    context.JobId,
                    (object)context.Settings ?? DBNull.Value,
                    (object)context.Content ?? DBNull.Value,
                    (object)context.MainJobId ?? DBNull.Value);
            }

            return table;
        }

        public bool UpdateJob(string id, JobStatus status, string comment)
        {
            if (status == JobStatus.InProgress || status == JobStatus.Wait)
            {
                logger.Warn("Can not set Job Status to InProgress OR Wait.");
                return false;
            }
            lock (updateLocker)
            {
                try
                {
                    var job = GetJobWithOutI18N(id);
                    if (status == JobStatus.Finished || status == JobStatus.FinishWithException)
                    {
                        job.Progress = 100;
                        job.EndTime = DateTime.UtcNow.Ticks;
                    }
                    job.Status = (int)status;
                    if (!string.IsNullOrEmpty(comment))
                    {
                        job.Comment = comment;
                    }
                    job.EndTime = DateTime.UtcNow.Ticks;
                    job.LastUpdateTime = DateTime.UtcNow.Ticks;
                    var result = UpdateAsync(job).Result;
                    logger.Info("Successfully update job status.JobId:[{0}] Status:[{1}]", id, status);
                    return result;
                }
                catch (Exception e)
                {
                    logger.Warn("Fail to update job status.JobId:[{0}] Status:[{1}] Error Message:{2}", id, status, e.ToString());
                    return false;
                }
            }
        }
        public bool UpdateJob(string id, int progress)
        {
            if (progress <= 0 || progress >= 100)
            {
                return false;
            }
            #region lock Db row
            //using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            //{
            //    IsolationLevel = IsolationLevel.RepeatableRead,
            //    Timeout = new TimeSpan(0, 2, 0)
            //}))
            #endregion
            var result = false;
            lock (updateLocker)
            {
                try
                {
                    var job = GetJobWithOutI18N(id);
                    if (job.Status == (int)JobStatus.Stopping)
                    {
                        return false;
                    }
                    if (job.Progress > progress)
                    {
                        return false;
                    }
                    bool isProgressChanged = false;
                    if (Convert.ToInt32(job.Progress) != progress) //Quality Issue
                    {
                        isProgressChanged = true;
                    }
                    //5min
                    long elapsedTicks = DateTime.UtcNow.Ticks - job.LastUpdateTime;
                    TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
                    if (elapsedSpan.Minutes > 5 || isProgressChanged)
                    {
                        job.Progress = progress;
                        job.Status = (int)JobStatus.InProgress;
                        job.LastUpdateTime = DateTime.UtcNow.Ticks;
                        result = UpdateAsync(job).Result;
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("Fail to update job progress.JobId:[{0}] Progress:[{1}] Error Message:{2}", id, progress, e.ToString());
                    return false;
                }
            }
            return result;
        }

        public bool UpdateJobTime(string id, bool isStartTime)
        {
            try
            {
                using var context = GetNewContext();
                var subJob = GetSubJob(id);
                if(subJob == null)
                {
                    logger.Warn($"Sub job not found: {id}");
                    return false; 
                }
                if (isStartTime)
                {
                    subJob.StartTime = DateTime.UtcNow.Ticks;
                }
                else
                {
                    subJob.EndTime = DateTime.UtcNow.Ticks;
                }
                context.RMSubJobs.AddOrUpdate(subJob);              
                return context.SaveChanges() > 0;
            }
            catch
            {
                logger.Error($"Fail to update job is start time : {isStartTime}");
                return false;
            }
        }

        public Dictionary<string, int> GetAgentJobCount(List<JobType> jobTypes)
        {
            Dictionary<string, int> agentJobCount = new Dictionary<string, int>();
            try
            {
                List<int> mJobTypes = new List<int>();
                foreach (var jobType in jobTypes)
                {
                    mJobTypes.Add((int)jobType);
                }
                using (var context = GetNewContext())
                {
                    var jobResult = context.RMSubJobs.Where(s => (s.Status == (int)JobStatus.InProgress || s.Status == (int)JobStatus.Wait)
                     && s.AgentId != null
                     && mJobTypes.Contains(s.JobType)).GroupBy(s => s.AgentId).ToList();                   
                    foreach (var j in jobResult)
                    {
                        agentJobCount.Add(j.Key, j.Count());
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while getting agent job count for fs. Error:{0}", e.ToString());
            }
            return agentJobCount;
        }

        public List<RMSubJob> GetRunningAgentJob(List<JobType> jobTypes)
        {
            List<RMSubJob> runningJobs = new List<RMSubJob>();
            try
            {
                List<int> mJobTypes = new List<int>();
                foreach (var jobType in jobTypes)
                {
                    mJobTypes.Add((int)jobType);
                }
                using (var context = GetNewContext())
                {
                    runningJobs = context.RMSubJobs.Where(s => (s.Status == (int)JobStatus.InProgress || s.Status == (int)JobStatus.Wait)
                     && mJobTypes.Contains(s.JobType) && s.String1 != null).ToList();
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting runing fs jobs, error:{0}", e.ToString());
            }
            return runningJobs;
        }

        public List<RMSubJob> GetInProgressAgentJob(List<JobType> jobTypes)
        {
            List<RMSubJob> runningJobs = new List<RMSubJob>();
            try
            {
                List<int> mJobTypes = new List<int>();
                foreach (var jobType in jobTypes)
                {
                    mJobTypes.Add((int)jobType);
                }
                using (var context = GetNewContext())
                {
                    runningJobs = context.RMSubJobs.Where(s => (s.Status == (int)JobStatus.InProgress)
                     && mJobTypes.Contains(s.JobType)).ToList();
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting runing fs jobs, error:{0}", e.ToString());
            }
            return runningJobs;
        }

        public async Task<bool> UpdateAgentIdAsync(string jobId, string agentId)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    var job = GetJobWithOutI18N(jobId);
                    job.AgentId = agentId;
                    await UpdateAsync(job);
                    return true;
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while updating agent id for job. JobId:{0} Agent Id:{1} Error:{2}", jobId, agentId, e.ToString());
                return false;
            }
        }
        private RMSubJob GetJobWithOutI18N(string id)
        {
            using (var context = GetNewContext())
            {
                RMSubJob jm = context.RMSubJobs.Find(id);
                return jm;
            }
        }

        public List<string> GetRunningSetPermissionJobIds(string exceptJobId = "")
        {
            int waitingState = (int)JobStatus.Wait;
            int progressState = (int)JobStatus.InProgress;
            int runnable_canRun = (int)RecordsConstants.SubJob_Runnable_CanRun;
            int runnable_running = (int)RecordsConstants.SubJob_Runnable_Runing;
            int type = (int)JobType.PhysicalSetPermission;
            using (var context = GetNewContext())
            {
                return context.RMSubJobs.Where(a => a.JobType == type && a.Id != exceptJobId && !string.IsNullOrEmpty(a.ParentId) && (a.Runable == runnable_canRun || a.Runable == runnable_running) && (a.Status == waitingState || a.Status == progressState)).Select(s => s.Id).ToList();
            }
        }

        public List<string> GetErrorJobSummary(string mainJobId, int limitCount)
        {
            int failedState = (int)JobStatus.Failed;
            int finishWithExceptionState = (int)JobStatus.FinishWithException;
            int stoppedState = (int)JobStatus.Stopped;
            using (var ctx = GetNewContext())
            {
                return ctx.RMSubJobs
                    .Where(s => s.ParentId == mainJobId 
                        && (s.Status == failedState || s.Status == finishWithExceptionState || s.Status == stoppedState) 
                        && !string.IsNullOrEmpty(s.Comment))
                    .Select(s => s.Comment).Take(limitCount).ToList();
            }
        }

        public List<RMSubJob> GetRunningAgentJob(List<JobType> jobTypes, List<string> agentIds)
        {
            List<RMSubJob> runningJobs = new List<RMSubJob>();
            try
            {
                List<int> mJobTypes = new List<int>();
                foreach (var jobType in jobTypes)
                {
                    mJobTypes.Add((int)jobType);
                }
                using (var context = GetNewContext())
                {
                    runningJobs = context.RMSubJobs.Where(s => (s.Status == (int)JobStatus.InProgress || s.Status == (int)JobStatus.Wait)
                     && mJobTypes.Contains(s.JobType) && s.String1 != null && agentIds.Contains(s.AgentId)).ToList();
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting runing fs jobs, error:{0}", e.ToString());
            }
            return runningJobs;
        }

        public bool TryReserveAgentSlot(string subJobId, string agentId,
            List<JobType> jobTypes, int maxConcurrent)
        {
            try
            {
                var mJobTypes = jobTypes.Select(t => (int)t).ToList();
                using (var context = GetNewContext())
                {
                    // Recount inside the same context/transaction to close the race window.
                    int liveCount = context.RMSubJobs.Count(s =>
                        (s.Status == (int)JobStatus.InProgress || s.Status == (int)JobStatus.Wait)
                        && s.AgentId == agentId
                        && mJobTypes.Contains(s.JobType));

                    if (liveCount >= maxConcurrent)
                    {
                        logger.Info("Agent {0} at capacity ({1}/{2}); deferring sub job {3}.", agentId, liveCount, maxConcurrent, subJobId);
                        return false;
                    }

                    var subJob = context.RMSubJobs.FirstOrDefault(s => s.Id == subJobId);
                    if (subJob == null) return false;

                    subJob.AgentId = agentId;
                    subJob.Runable = RecordsConstants.SubJob_Runnable_Runing;
                    context.SaveChanges();
                    return true;
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.Warn("Optimistic concurrency conflict reserving slot for agent {0}, sub job {1}: {2}",
                    agentId, subJobId, ex.Message);
                return false;
            }
            catch (Exception e)
            {
                logger.Error("Error reserving agent slot. Agent:{0} SubJob:{1} Error:{2}",
                    agentId, subJobId, e.ToString());
                return false;
            }
        }
    }
}
