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
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Migrations;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;
using AvePoint.GCommon.Contract.AveModuleContract;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Google.GControlPlatform;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Monitor;
using AvePoint.RA.Contract.Object.ArchiverMigration;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2021.PowerPoint.Comment;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
//using Z.EntityFramework.Plus;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class JobMonitorDao : BaseDao<RMJobMonitor>, IJobMonitorDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(JobMonitorDao));

        private readonly static Object updateLocker = new object();

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
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public List<int> GetFilterList(Expression<Func<RMJobMonitor, int>> selectLambda)
        {
            try
            {
                if (selectLambda == null)
                {
                    return new List<int>();
                }

                var excludeQueryJobTypes = new List<int>
                {
                    (int)JobType.SharePointOnlineDeletionSyncUpgrade,
                    (int)JobType.TenantUpgrade,
                    (int)JobType.ManualHistoriesUpgrade,
                    (int)JobType.CosmosDBDirtyDataDeleteUpgrade,
                    (int)JobType.ManualFileSystemUpgrade,
                    (int)JobType.SendEmailJob,
                    (int)JobType.MoveDataTier,
                    (int)JobType.DiscoveryJob,
                    (int)JobType.DiscoveryOptimizationCalculate,
                    (int)JobType.DiscoveryAOSPOptimizationCalculate,
                    (int)JobType.DiscoveryReCalculate,
                    (int)JobType.RebuildStub,
                    (int)JobType.RebuildIndex,
                    (int)JobType.AdjustStorageSize,
                    (int)JobType.ApprovalProcessArchive,
                    (int)JobType.AOSPRestore,
                    (int)JobType.TeamsChannelSettingConflictCheck,

                    (int)JobType.BaseArchiveJobIdMultiRestore,
                    (int)JobType.MigrateDataCosmosDbForJPMC,
                    (int)JobType.ArchiverFullMoveRetention,
                    (int)JobType.APStorageCostEvaluation,
                };

                using (var context = GetNewContext())
                {
                    return context.JobMonitors.AsQueryable().Where(item => !excludeQueryJobTypes.Contains(item.JobType)).Select(selectLambda).Distinct().ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<RMJobMonitor> GetJobs(int pageIndex, int pageSize, out int totalRecord, string orderKey, bool isAsc, Expression<Func<RMJobMonitor, bool>> whereLambda = null)
        {
            string sortBy = "StartTime";
            string runBy = "AvePoint Cloud Records System";     //此处是兼容老数据逻辑，文字是与DB中的值对应的，不会直接显示在页面上，所以不能改成Opus
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

            string enumSortString = null;
            try
            {
                if (sortBy == "JobType")
                {
                    enumSortString = GetJobTypesEnumSortString();
                    logger.Debug("typeSortString is {0}", enumSortString);
                }
                else if (sortBy == "Status")
                {
                    enumSortString = GetJobStatusEnumSortString();
                    logger.Debug("statusSortString is {0}", enumSortString);
                }

                using (var context = GetNewContext())
                {
                    //context.Database.Log = (log) => logger.Debug(log);
                    IOrderedQueryable<RMJobMonitor> query = null;
                    var excludeQueryJobTypes = new List<int>
                    {
                        (int)JobType.SharePointOnlineDeletionSyncUpgrade,
                        (int)JobType.TenantUpgrade,
                        (int)JobType.ManualHistoriesUpgrade,
                        (int)JobType.CosmosDBDirtyDataDeleteUpgrade,
                        (int)JobType.ManualFileSystemUpgrade,
                        (int)JobType.SendEmailJob,
                        (int)JobType.MoveDataTier,
                        (int)JobType.DiscoveryJob,
                        (int)JobType.DiscoveryOptimizationCalculate,
                        (int)JobType.DiscoveryAOSPOptimizationCalculate,
                        (int)JobType.DiscoveryAOSPOptimization,
                        (int)JobType.DiscoveryAOSPJob,
                        (int)JobType.AOSPRestore,
                        (int)JobType.DiscoveryReCalculate,
                        (int)JobType.RebuildStub,
                        (int)JobType.RebuildIndex,
                        (int)JobType.AdjustStorageSize,
                        (int)JobType.ApprovalProcessArchive,
                        (int)JobType.DiscoveryFileSystemV1,
                        (int)JobType.TeamsChannelSettingConflictCheck,
                        (int)JobType.MigrateDataCosmosDbForJPMC,
                        (int)JobType.ArchiverFullMoveRetention,
                        (int)JobType.APStorageCostEvaluation,
                    };

                    var baseQuery = context.JobMonitors.AsQueryable().Where(item => !excludeQueryJobTypes.Contains(item.JobType));
                    if (whereLambda != null)
                    {
                        baseQuery = baseQuery.Where(whereLambda);
                    }

                    if (sortBy == "JobPriority")
                    {
                        query = baseQuery.SortBy(sortBy, sortDirection)
                                         .JMEnumThenSortBy(thenSortBy, thenSortDirection, enumSortString);
                    }
                    else
                    {
                        query = baseQuery.JMEnumSortBy(sortBy, sortDirection, enumSortString)
                                         .JMEnumThenSortBy(thenSortBy, thenSortDirection, enumSortString);
                    }

                    totalRecord = query.Count();
                    var results = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
                    foreach (var jm in results)
                    {
                        if (!string.IsNullOrEmpty(jm.Comment))
                        {
                            jm.Comment = I18NEntity.GetString(jm.Comment);
                        }
                        if (string.IsNullOrEmpty(jm.UserName) || jm.UserName == "RM_TS_RunSchedule" || jm.UserName.ToLowerInvariant().Equals(runBy.ToLowerInvariant()))
                        {
                            jm.UserName = I18NEntity.GetString("RM_TS_RunSchedule");
                        }
                        if (jm.UserName == "RM_JS_Common_Pending")
                        {
                            jm.UserName = I18NEntity.GetString("RM_JS_Common_Pending");
                        }
                    }
                    return results.ToList();
                }
            }
            catch (Exception e)
            {
                logger.Error("Get jobs error:{0}.", e.ToString());
                totalRecord = 0;
                return new List<RMJobMonitor>();
            }
        }

        private string GetJobTypesEnumSortString()
        {
            string enumSortString;
            var jobTypesDic = new List<KeyValuePair<string, int>>();
            foreach (int v in Enum.GetValues(typeof(JobType)))
            {
                string strName = Enum.GetName(typeof(JobType), v);
                jobTypesDic.Add(new KeyValuePair<string, int>(I18NEntity.GetString("RM_JS_JM_JobType_" + strName), v));
            }
            enumSortString = "|" + string.Join("", jobTypesDic.OrderBy(s => s.Key).ToList().Select(j => j.Value + "|"));
            return enumSortString;
        }

        private string GetJobStatusEnumSortString()
        {
            string enumSortString;
            var jobStatusDic = new Dictionary<string, int>();
            foreach (int v in Enum.GetValues(typeof(JobStatus)))
            {
                var statusStr = ConvertJobStatusToString((JobStatus)v);
                if (statusStr != null)
                {
                    jobStatusDic.Add(statusStr, v);
                }
            }

            var sortedKeys = jobStatusDic.Keys.OrderBy(j => j);
            var jobStatusIntArray = new List<int>();
            foreach (var k in sortedKeys)
            {
                jobStatusIntArray.Add(jobStatusDic[k]);
            }
            enumSortString = "|" + string.Join("", jobStatusIntArray.Select(k => k + "|"));
            return enumSortString;
        }

        private string ConvertJobStatusToString(JobStatus jobStatus)
        {
            string result = null;
            switch (jobStatus)
            {
                case JobStatus.Wait:
                    result = I18NEntity.GetString("RM_JS_JM_Status_Wait");
                    break;
                case JobStatus.InProgress:
                    result = I18NEntity.GetString("RM_JS_JM_Status_InProgerss");
                    break;
                case JobStatus.Finished:
                    result = I18NEntity.GetString("RM_JS_JM_Status_Finished");
                    break;
                case JobStatus.Failed:
                    result = I18NEntity.GetString("RM_JS_JM_Status_Failed");
                    break;
                case JobStatus.FinishWithException:
                    result = I18NEntity.GetString("RM_JS_JM_Status_FinishWithException");
                    break;
                case JobStatus.Stopped:
                    result = I18NEntity.GetString("RM_JS_JM_Status_Stopped");
                    break;
                case JobStatus.Skipped:
                    result = I18NEntity.GetString("RM_JS_JM_Status_Skipped");
                    break;
                case JobStatus.Stopping:
                    result = I18NEntity.GetString("RM_JS_JM_Status_Stopping");
                    break;
            }
            return result;
        }

        public List<RMJobMonitor> GetJobsByProfileId(int profileId)
        {
            using (var context = GetNewContext())
            {
                var runBy = "AvePoint Cloud Records System";    //此处是兼容老数据逻辑，文字是与DB中的值对应的，不会直接显示在页面上，所以不能改成Opus
                var results = context.JobMonitors.Where(c => c.ProfileId.HasValue && c.ProfileId.Value == profileId).SortBy("StartTime", SortDirectionEnum.Descending).ToList();
                foreach (var jm in results)
                {
                    if (!string.IsNullOrEmpty(jm.Comment))
                    {
                        jm.Comment = I18NEntity.GetString(jm.Comment);
                    }
                    if (jm.UserName == "RM_TS_RunSchedule" || jm.UserName.ToLowerInvariant().Equals(runBy.ToLowerInvariant()))
                    {
                        jm.UserName = I18NEntity.GetString("RM_TS_RunSchedule");
                    }
                    if (jm.UserName == "RM_JS_Common_Pending")
                    {
                        jm.UserName = I18NEntity.GetString("RM_JS_Common_Pending");
                    }
                }
                return results;
            }
        }

        public List<RMJobMonitor> GetJobsByProfileIds(List<int> profileIds)
        {
            using (var context = GetNewContext())
            {
                var runBy = "AvePoint Cloud Records System";    //此处是兼容老数据逻辑，文字是与DB中的值对应的，不会直接显示在页面上，所以不能改成Opus
                var results = context.JobMonitors.Where(c => c.ProfileId.HasValue && profileIds.Contains(c.ProfileId.Value)).SortBy("StartTime", SortDirectionEnum.Descending).ToList();
                foreach (var jm in results)
                {
                    if (!string.IsNullOrEmpty(jm.Comment))
                    {
                        jm.Comment = I18NEntity.GetString(jm.Comment);
                    }
                    if (jm.UserName == "RM_TS_RunSchedule" || jm.UserName.ToLowerInvariant().Equals(runBy.ToLowerInvariant()))
                    {
                        jm.UserName = I18NEntity.GetString("RM_TS_RunSchedule");
                    }
                    if (jm.UserName == "RM_JS_Common_Pending")
                    {
                        jm.UserName = I18NEntity.GetString("RM_JS_Common_Pending");
                    }
                }
                return results;
            }
        }

        public List<RMJobMonitor> GetJobsByJobType(JobType jobType)
        {
            using (var context = GetNewContext())
            {
                context.Database.CommandTimeout = 60 * 5;
                var results = context.JobMonitors.Where(c => c.JobType == (int)jobType).ToList();
                return results;
            }
        }

        public RMJobMonitor GetLastestJobByJobType(JobType jobType)
        {
            using (var context = GetNewContext())
            {
                context.Database.CommandTimeout = 60 * 5;
                return context.JobMonitors.Where(c => c.JobType == (int)jobType).OrderByDescending(c => c.StartTime).FirstOrDefault();
            }
        }

        public string GetProfileNameById(int id)
        {
            if (id <= 0)
            {
                return "";
            }
            using (var context = GetNewContext())
            {
                var result = context.Profile.Where(c => c.Id == id).Select(c => c.Name).FirstOrDefault();
                return result;
            }
        }

        public string CreateJob(string id, JobType jobType)
        {
            return CreateJob(id, jobType, "");
        }

        public string CreateJob(string id, JobType jobType, string jobRunBy, string containerId = null, string scopedId = null, string fullPath = null)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    context.JobMonitors.Add(new RMJobMonitor()
                    {
                        Id = id,
                        JobType = (int)jobType,
                        StartTime = DateTime.UtcNow.Ticks,
                        Progress = 0,
                        Status = (int)JobStatus.Wait,
                        UserName = jobRunBy,
                        LastUpdateTime = DateTime.UtcNow.Ticks,
                        ContainerId = containerId,
                        ScopeId = scopedId,
                        Extension = SerializerHelper.SerializeByJsonConvert(new JobExtension() { soSCProgress = new SOSCProgress() { fullPath = fullPath} })
                });
                    context.SaveChanges();
                }
                return id;
            }
            catch (Exception e)
            {
                logger.Error("Create job error:{0}.", e.ToString());
                return "";
            }
        }
        
        public string CreateJobWithGControlJobId(string id, string gControlJobId, JobType jobType, string jobRunBy, string containerId = null, string scopedId = null, string fullPath = null)
        {
            try
            {
                using var context = GetNewContext();
                context.JobMonitors.Add(new RMJobMonitor()
                {
                    Id = id,
                    JobType = (int)jobType,
                    StartTime = DateTime.UtcNow.Ticks,
                    Progress = 0,
                    Status = (int)JobStatus.Wait,
                    UserName = jobRunBy,
                    LastUpdateTime = DateTime.UtcNow.Ticks,
                    ContainerId = containerId,
                    ScopeId = scopedId,
                    AdditionalInformation = gControlJobId,
                    Extension = SerializerHelper.SerializeByJsonConvert(new JobExtension() { soSCProgress = new SOSCProgress() { fullPath = fullPath} })
                });
                context.SaveChanges();
                return id;
            }
            catch (Exception e)
            {
                logger.Error("Create job error:{0}.", e.ToString());
                return "";
            }
        }

        public async Task CreateDiscoveryJobAsync(string id, string jobRunBy, Guid mainJobId, Guid discoveryJobId, JobType discoveryJobType)
        {
            using var context = GetNewContext();
            context.JobMonitors.Add(new RMJobMonitor
            {
                Id = id,
                JobType = (int)discoveryJobType,
                StartTime = DateTime.UtcNow.Ticks,
                Progress = 0,
                Status = (int)JobStatus.Wait,
                UserName = jobRunBy,
                LastUpdateTime = DateTime.UtcNow.Ticks,
                DiscoveryMainJobId = mainJobId,
                DiscoveryJobId = discoveryJobId,
            });
            await context.SaveChangesAsync();
        }

        public string CreateJobWithScopeId(string id, JobType jobType, string jobRunBy, string scopeId, string containerId = null, JobStatus status = JobStatus.Wait, string comment = null ,string fullPath = null,string jobConflictExtension = null)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    context.JobMonitors.Add(new RMJobMonitor()
                    {
                        Id = id,
                        JobType = (int)jobType,
                        StartTime = DateTime.UtcNow.Ticks,
                        Progress = 0,
                        Status = (int)status,
                        ScopeId = scopeId,
                        UserName = jobRunBy,
                        LastUpdateTime = DateTime.UtcNow.Ticks,
                        ContainerId = containerId,
                        Comment = comment,
                        EndTime = (status == JobStatus.Failed || status == JobStatus.Skipped) ? DateTime.UtcNow.Ticks : 0L,
                        Extension =  fullPath ?? string.Empty,
                        JobConflictExtension = jobConflictExtension,
                    });
                    context.SaveChanges();
                    return id;
                }
            }
            catch (Exception e)
            {
                logger.Error("Create job error:{0}.", e.ToString());
                return "";
            }
        }
        
        public string CreateJobWithScopeIdAndWithGControlJobId(string id, string gControlJobId, JobType jobType, string jobRunBy, string scopeId, string containerId = null, JobStatus status = JobStatus.Wait, string comment = null ,string fullPath = null,string jobConflictExtension = null)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    context.JobMonitors.Add(new RMJobMonitor()
                    {
                        Id = id,
                        JobType = (int)jobType,
                        StartTime = DateTime.UtcNow.Ticks,
                        Progress = 0,
                        Status = (int)status,
                        ScopeId = scopeId,
                        UserName = jobRunBy,
                        LastUpdateTime = DateTime.UtcNow.Ticks,
                        ContainerId = containerId,
                        Comment = comment,
                        EndTime = (status == JobStatus.Failed || status == JobStatus.Skipped) ? DateTime.UtcNow.Ticks : 0L,
                        Extension =  fullPath ?? string.Empty,
                        JobConflictExtension = jobConflictExtension,
                        AdditionalInformation = gControlJobId
                    });
                    context.SaveChanges();
                    return id;
                }
            }
            catch (Exception e)
            {
                logger.Error("Create job error:{0}.", e.ToString());
                return "";
            }
        }

        public string CreateJobWithScopeIdForTeams(string id, JobType jobType, string jobRunBy, string scopeId,string additionalInformation, string containerId = null, JobStatus status = JobStatus.Wait, string comment = null, string fullPath = null, string jobConflictExtension = null)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    context.JobMonitors.Add(new RMJobMonitor()
                    {
                        Id = id,
                        JobType = (int)jobType,
                        StartTime = DateTime.UtcNow.Ticks,
                        Progress = 0,
                        Status = (int)status,
                        ScopeId = scopeId,
                        UserName = jobRunBy,
                        LastUpdateTime = DateTime.UtcNow.Ticks,
                        ContainerId = containerId,
                        Comment = comment,
                        EndTime = (status == JobStatus.Failed || status == JobStatus.Skipped) ? DateTime.UtcNow.Ticks : 0L,
                        Extension = fullPath ?? string.Empty,
                        JobConflictExtension = jobConflictExtension,
                        AdditionalInformation = additionalInformation,
                    });
                    context.SaveChanges();
                    return id;
                }
            }
            catch (Exception e)
            {
                logger.Error("Create job error:{0}.", e.ToString());
                return "";
            }
        }

        public string CreateJobWithScopeIdForRecenter(string id, JobType jobType, string jobRunBy, string scopeId, int nodeType, string realRunJobUser, string containerId = null, JobStatus status = JobStatus.Wait, string comment = null)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    var document = new XmlDocument();
                    var element = document.CreateElement("Info");
                    element.SetAttribute("ReCenterRunJobUser", realRunJobUser);
                    document.AppendChild(element);
                    context.JobMonitors.Add(new RMJobMonitor()
                    {
                        Id = id,
                        JobType = (int)jobType,
                        StartTime = DateTime.UtcNow.Ticks,
                        Progress = 0,
                        Status = (int)status,
                        ScopeId = scopeId,
                        UserName = realRunJobUser,
                        LastUpdateTime = DateTime.UtcNow.Ticks,
                        ContainerId = containerId,
                        Comment = comment,
                        EndTime = (status == JobStatus.Failed || status == JobStatus.Skipped) ? DateTime.UtcNow.Ticks : 0L,
                        NodeType = nodeType,
                        AdditionalInformation = document.InnerXml,
                    });
                    context.SaveChanges();
                    return id;
                }
            }
            catch (Exception e)
            {
                logger.Error("Create job error:{0}.", e.ToString());
                return "";
            }
        }
        public bool HasRunningArchiverJobOnScope(List<JobType> types, string scope)
        {
            using (RMDbContext context = GetNewContext())
            {
                int waitingState = (int)JobStatus.Wait;
                int runningState = (int)JobStatus.InProgress;
                List<int> jobTypeStates = types.Select(t => (int)t).ToList();
                var runningJobs = context.JobMonitors.Where(a => jobTypeStates.Contains(a.JobType) && a.ScopeId.Equals(scope) && (a.Status == waitingState || a.Status == runningState || a.Status == (int)JobStatus.Stopping)).ToList();
                return runningJobs != null && runningJobs.Count > 0;
            }
        }
        public List<string> GetRunningArchiverJobOnScope(List<JobType> types, string scope)
        {
            using (RMDbContext context = GetNewContext())
            {
                int waitingState = (int)JobStatus.Wait;
                int runningState = (int)JobStatus.InProgress;
                List<int> jobTypeStates = types.Select(t => (int)t).ToList();
                var runningJobs = context.JobMonitors.Where(a => jobTypeStates.Contains(a.JobType) && a.ScopeId.Equals(scope) && (a.Status == waitingState || a.Status == runningState || a.Status == (int)JobStatus.Stopping)).ToList();
                return runningJobs?.Select(a=>a.Id).ToList();
            }
        }
        public List<RMJobMonitor> HasRunningArchiverJob(List<JobType> types)
        {
            using (RMDbContext context = GetNewContext())
            {
                int waitingState = (int)JobStatus.Wait;
                int runningState = (int)JobStatus.InProgress;
                int stoppingState = (int)JobStatus.Stopping;
                List<int> jobTypeStates = types.Select(t => (int)t).ToList();
                var runningJobs = context.JobMonitors.Where(a => jobTypeStates.Contains(a.JobType) && (a.Status == waitingState || a.Status == runningState || a.Status == stoppingState)).ToList();
                return runningJobs;
            }
        }

        public bool HasStoppingArchiverJobOnScope(List<JobType> types, string scope)
        {
            using (RMDbContext context = GetNewContext())
            {
                int stoppingState = (int)JobStatus.Stopping;
                List<int> jobTypeStates = types.Select(t => (int)t).ToList();
                var runningJobs = context.JobMonitors.Where(a => jobTypeStates.Contains(a.JobType) && a.ScopeId.Equals(scope) && a.Status == stoppingState).ToList();
                return runningJobs != null && runningJobs.Count > 0;
            }
        }

        public string CreateJobWithProfileId(string id, JobType jobType, string jobRunBy, int profileId, string userId = null, int subJobCount = 0)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    context.JobMonitors.Add(new RMJobMonitor()
                    {
                        Id = id,
                        JobType = (int)jobType,
                        StartTime = DateTime.UtcNow.Ticks,
                        Progress = 0,
                        UserName = jobRunBy,
                        Status = (int)JobStatus.Wait,
                        ProfileId = profileId,
                        LastUpdateTime = DateTime.UtcNow.Ticks,
                        ContainerId = userId,
                        SubJobCount = subJobCount,
                    });
                    context.SaveChanges();
                }
                return id;
            }
            catch (Exception e)
            {
                logger.Error("Create job error:{0}.", e.ToString());
                return "";
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
                    if (job.Progress != progress)
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

        /// <summary>
        /// 此方法用于避免因数据量较大无法及时更新job进度，导致的job 超时失败。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> UpdateJobWithoutProgressChangeAsync(string id)
        {
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
                    //2min
                    long elapsedTicks = DateTime.UtcNow.Ticks - job.LastUpdateTime;
                    TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
                    if (elapsedSpan.Minutes > 2)
                    {
                        job.LastUpdateTime = DateTime.UtcNow.Ticks;
                        result = UpdateAsync(job).Result;
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("Fail to update job.JobId:[{0}] Error Message:{1}", id, e.ToString());
                    return false;
                }
            }
            return result;
        }

        /// <summary>
        /// 目前只有JOb超时会级联子job的状态,  如果有子job, 不建议直接更新主job状态.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="comment"></param>
        /// <param name="cascadeSubJob">是否级联子job状态, 默认false</param>
        /// <returns></returns>
        public bool UpdateJob(string id, JobStatus status, string comment, bool cascadeSubJob = false)
        {
            if (status == JobStatus.InProgress || status == JobStatus.Wait)
            {
                logger.Warn("Can not set Job Status to InProgress OR Wait.");
                return false;
            }
            lock (updateLocker)
            {
                bool isFinish = status == JobStatus.Finished || status == JobStatus.FinishWithException || status == JobStatus.Pending;
                bool hasComments = !string.IsNullOrEmpty(comment);
                logger.Info("Update job status to {0}, isfinish? {1}, has comments? {2}, cascade sub job? {3}; job id {4}", status, isFinish, hasComments, cascadeSubJob, id);

                string sqlUpdate = "Update {0}.RMJobMonitors set Status = @status, "
                    + (hasComments ? "Comment = @comment," : "")
                    + " EndTime = @datetime, LastUpdateTime = @datetime"
                        + (isFinish ? ", Progress = 100" : "") + " where Id = @id";

                if (status == JobStatus.Stopped)
                {
                    sqlUpdate = "Update {0}.RMJobMonitors set Status = @status, "
                    + (hasComments ? "Comment = @comment," : "")
                    + " EndTime = @datetime, LastUpdateTime = @datetime"
                        + (isFinish ? ", Progress = 100" : "") + " where Id = @id";
                }

                string sqlSubJob = null;
                if (cascadeSubJob)
                {
                    sqlSubJob = "Update {0}.RMSubJobs set Status = @status , LastUpdateTime = @datetime where ParentId = @id and Status < 2";
                }
                int row = 0;
                var dateTime = DateTime.UtcNow;
                using (var context = RMDBContextManager.GetNewDBContext())
                {
                    using (DbContextTransaction tran = context.Database.BeginTransaction())
                    {
                        if (hasComments)
                        {
                            row = context.Database.ExecuteSqlCommand(string.Format(sqlUpdate, context.SchemaName),
                                new SqlParameter("status", (int)status),
                                new SqlParameter("comment", comment),
                                new SqlParameter("datetime", dateTime.Ticks),
                                new SqlParameter("id", id));
                        }
                        else
                        {
                            row = context.Database.ExecuteSqlCommand(string.Format(sqlUpdate, context.SchemaName),
                                new SqlParameter("status", (int)status),
                                new SqlParameter("datetime", dateTime.Ticks),
                                new SqlParameter("id", id));
                        }
                        if (cascadeSubJob)
                        {
                            context.Database.ExecuteSqlCommand(string.Format(sqlSubJob, context.SchemaName),
                                new SqlParameter("status", (int)status),
                                new SqlParameter("datetime", dateTime.Ticks),
                                new SqlParameter("id", id));
                        }
                        tran.Commit();
                    }
                    //Update Special Job Status to GControl
                    UpdateGControlJobStatus(id, context, status, dateTime);
                }
                return row > 0;

                #region 
                //try
                //{
                //    var job = GetJobWithOutI18N(id);
                //    if (status == JobStatus.Finished || status == JobStatus.FinishWithException)
                //    {
                //        job.Progress = 100;
                //        job.EndTime = DateTime.UtcNow.Ticks;
                //    }
                //    job.Status = (int)status;
                //    if (!string.IsNullOrEmpty(comment))
                //    {
                //        job.Comment = comment;
                //    }
                //    job.EndTime = DateTime.UtcNow.Ticks;
                //    job.LastUpdateTime = DateTime.UtcNow.Ticks;
                //    var result = Update(job);
                //    logger.Info("Successfully update job status.JobId:[{0}] Status:[{1}]", id, status);
                //    return result;
                //}
                //catch (Exception e)
                //{
                //    logger.Warn("Fail to update job status.JobId:[{0}] Status:[{1}] Error Message:{2}", id, status, e.ToString());
                //    return false;
                //}
                #endregion
            }
        }

        private void UpdateGControlJobStatus(string id, RMDbContext context, JobStatus status, DateTime dateTime)
        {
            var jobId = JobServiceUtility.IsSubJob(id) ? id.Split('_')[0] : id;
            var result = context.JobMonitors.FirstOrDefault(job => job.Id == jobId)!;
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

        public bool AtomicityUpdateJobExtension(string jobId, string oldJobExtension, string newJobExtension)
        {
            string sql = "update {0}.RMJobMonitors set Extension = @newJobExtension where Id = @jobId and Extension = @oldJobExtension";
            int row = 0;
            using (RMDbContext context = GetNewContext())
            {
                using (DbContextTransaction tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        row = context.Database.ExecuteSqlCommand(string.Format(sql, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)),
                        new SqlParameter("jobId", jobId),
                        new SqlParameter("oldJobExtension", oldJobExtension?? (Object)DBNull.Value),
                        new SqlParameter("newJobExtension", newJobExtension?? (Object)DBNull.Value));
                        tran.Commit();
                    }
                    catch(Exception)
                    {
                        tran.Rollback();
                        throw;
                    }
                }
                return row > 0;
            }
        }

        public bool UpdateMigrationJob(string id, JobStatus status, string comment, string additionalInformation)
        {
            if (status == JobStatus.InProgress || status == JobStatus.Wait)
            {
                logger.Warn("Can not set Job Status to InProgress OR Wait.");
                return false;
            }
            lock (updateLocker)
            {
                bool isFinish = status == JobStatus.Finished || status == JobStatus.FinishWithException || status == JobStatus.Pending;
                bool hasComments = !string.IsNullOrEmpty(comment);

                logger.Info("Update job status to {0}, isfinish? {1}, has comments? {2},  job id {3}", status, isFinish, hasComments, id);

                string sqlUpdate = "Update {0}.RMJobMonitors set Status = @status, "
                    + (hasComments ? "Comment = @comment," : "")
                    + "AdditionalInformation = @additionalInformation,"
                    + " EndTime = @datetime, LastUpdateTime = @datetime"
                        + (isFinish ? ", Progress = 100" : "") + " where Id = @id";

                if (status == JobStatus.Stopped)
                {
                    sqlUpdate = "Update {0}.RMJobMonitors set Status = @status, "
                    + (hasComments ? "Comment = @comment," : "")
                    + "AdditionalInformation = @additionalInformation,"
                    + " EndTime = @datetime, LastUpdateTime = @datetime"
                        + (isFinish ? ", Progress = 100" : "") + " where Id = @id";
                }
                int row = 0;
                using (var context = RMDBContextManager.GetNewDBContext())
                {
                    using (DbContextTransaction tran = context.Database.BeginTransaction())
                    {
                        if (hasComments)
                        {
                            row = context.Database.ExecuteSqlCommand(string.Format(sqlUpdate, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)),
                                new SqlParameter("status", (int)status),
                                new SqlParameter("comment", comment),
                                new SqlParameter("additionalInformation", additionalInformation),
                                new SqlParameter("datetime", DateTime.UtcNow.Ticks),
                                new SqlParameter("id", id));
                        }
                        else
                        {
                            row = context.Database.ExecuteSqlCommand(string.Format(sqlUpdate, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)),
                                new SqlParameter("status", (int)status),
                                new SqlParameter("additionalInformation", additionalInformation),
                                new SqlParameter("datetime", DateTime.UtcNow.Ticks),
                                new SqlParameter("id", id));
                        }
                        tran.Commit();
                    }
                }
                return row > 0;
            }
        }

        public bool UpdateJobAdditionalInformation(string id, string additionalInformation)
        {
            string sqlUpdate = "Update {0}.RMJobMonitors set AdditionalInformation = @additionalInformation,"
                + " EndTime = @datetime, LastUpdateTime = @datetime"
                + " where Id = @id";
            int row = 0;
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                using (DbContextTransaction tran = context.Database.BeginTransaction())
                {
                    row = context.Database.ExecuteSqlCommand(string.Format(sqlUpdate, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)),
                        new SqlParameter("additionalInformation", additionalInformation),
                        new SqlParameter("datetime", DateTime.UtcNow.Ticks),
                        new SqlParameter("id", id));
                    tran.Commit();
                }
            }
            return row > 0;
        }

        public bool UpdateJobExtension(string id,string extension)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                var job = context.JobMonitors.AsQueryable().First(c => c.Id == id);
                job.JobConflictExtension = extension;
                context.JobMonitors.AddOrUpdate(job);
                return context.SaveChanges()>0;
            }
        }

        public bool UpdateJobExtensionById(string id, string extension)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                var job = context.JobMonitors.AsQueryable().First(c => c.Id == id);
                job.Extension = extension;
                context.JobMonitors.AddOrUpdate(job);
                return context.SaveChanges() > 0;
            }
        }

        public bool UpdateJob(string id, JobStatus status)
        {
            return UpdateJob(id, status, "");
        }

        public bool UpdateJob(string id, int progress, int status, long endTime, string comment = null)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                var job = context.JobMonitors.AsQueryable().First(c => c.Id == id);
                job.Id = id;
                job.Progress = progress;
                job.Status = status;
                job.EndTime = endTime;
                job.LastUpdateTime = DateTime.UtcNow.Ticks;
                if (comment != null)
                {
                    job.Comment = comment;
                }
                return ApplyCurrentValues(context, job);
            }
        }

        public int GetJobProgress(string id)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.JobMonitors.AsQueryable()
                    .Where(c => c.Id == id)
                    .Select(c => c.Progress)
                    .FirstOrDefault();
            }
        }

        /// <summary>
        /// 供更新DB时使用
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private RMJobMonitor GetJobWithOutI18N(string id)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                var jm = context.JobMonitors.AsQueryable().First(c => c.Id == id);
                return jm;
            }
        }
        /// <summary>
        /// RMJobMonitor.Comment
        /// </summary>
        public RMJobMonitor GetJob(string id, bool userNameNeedI18N = true)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                var runBy = "AvePoint Cloud Records System";    //此处是兼容老数据逻辑，文字是与DB中的值对应的，不会直接显示在页面上，所以不能改成Opus
                var jm = context.JobMonitors.AsNoTracking().FirstOrDefault(c => c.Id == id);
                if (jm != null)
                {
                    if (string.IsNullOrEmpty(jm.UserName) || (jm.UserName == "RM_TS_RunSchedule" || jm.UserName.ToLowerInvariant().Equals(runBy.ToLowerInvariant())) && userNameNeedI18N)
                    {
                        jm.UserName = I18NEntity.GetString("RM_TS_RunSchedule");
                    }
                    if (jm.UserName == "RM_JS_Common_Pending")
                    {
                        jm.UserName = I18NEntity.GetString("RM_JS_Common_Pending");
                    }
                    if (!string.IsNullOrEmpty(jm.Comment))
                    {
                        jm.Comment = I18NEntity.GetString(jm.Comment);
                    }

                    //for fs job, get last failed sub job's comment
                    if (jm.JobType == (int)JobType.FSDataSynchronization
                        || jm.JobType == (int)JobType.FSDataSynchronizationSchedule
                        || jm.JobType == (int)JobType.FSDisposal
                        || jm.JobType == (int)JobType.FSDisposalByClassCode
                        || jm.JobType == (int)JobType.FSDisposalSchedule
                        || jm.JobType == (int)JobType.ImportFSSetting
                        || jm.JobType == (int)JobType.ExportFSSetting
                        || jm.JobType == (int)JobType.DownloadRCCReport)
                    {
                        try
                        {
                            var subJob = context.RMSubJobs.AsNoTracking().Where(j => j.ParentId == id && j.Status == (int)JobStatus.Failed && j.Comment != null).OrderByDescending(j => j.EndTime).FirstOrDefault();
                            if (subJob != null && !string.IsNullOrWhiteSpace(subJob.Comment))
                            {
                                //get from 18N
                                jm.Comment = subJob.Comment;
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Warn("Failed to get job comment. JobId:{0} Error:{1}", id, e.ToString());
                        }
                    }

                }
                return jm;
            }
        }

        public List<string> GetRCCJobIds(List<string> scopeIds)
        {
            var jobIds = new List<string>();
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                foreach (var id in scopeIds)
                {
                    var jobs = context.JobMonitors.AsQueryable().Where(c => c.ScopeId == id && (c.JobType == (int)JobType.DownloadRCCReport)).Select(c => c.Id).ToList();
                    jobIds.AddRange(jobs);
                }
            }
            return jobIds;
        }

        public RMJobMonitor GetSpecialJob(string id, bool userNameNeedI18N = true)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                var runBy = "AvePoint Cloud Records System";    //此处是兼容老数据逻辑，文字是与DB中的值对应的，不会直接显示在页面上，所以不能改成Opus
                var jm = context.JobMonitors.AsQueryable().FirstOrDefault(c => c.Id == id);
                if (jm != null)
                {
                    if (string.IsNullOrEmpty(jm.UserName) || (jm.UserName == "RM_TS_RunSchedule" || jm.UserName.ToLowerInvariant().Equals(runBy.ToLowerInvariant())) && userNameNeedI18N)
                    {
                        jm.UserName = I18NEntity.GetString("RM_TS_RunSchedule");
                    }
                    if (jm.UserName == "RM_JS_Common_Pending")
                    {
                        jm.UserName = I18NEntity.GetString("RM_JS_Common_Pending");
                    }
                }
                return jm;
            }
        }

        public RMJobMonitor GetJobById(string id)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                var jm = context.JobMonitors.AsQueryable().FirstOrDefault(c => c.Id.StartsWith(id));
                return jm;
            }
        }

        /// <summary>
        /// RMJobMonitor.Comment
        /// </summary>
        public List<RMJobMonitor> GetJobs(List<string> idArray)
        {
            return GetJobsAsync(idArray).GetAwaiter().GetResult();
        }
        public async Task<List<RMJobMonitor>> GetJobsAsync(List<string> idArray)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var runBy = "AvePoint Cloud Records System";    //此处是兼容老数据逻辑，文字是与DB中的值对应的，不会直接显示在页面上，所以不能改成Opus
            var jms = await context.JobMonitors
                .Where(c => idArray.Contains(c.Id))
                .ToListAsync();
            foreach (var jm in jms)
            {
                if (string.IsNullOrEmpty(jm.UserName) || jm.UserName == "RM_TS_RunSchedule" || jm.UserName.ToLowerInvariant().Equals(runBy.ToLowerInvariant()))
                {
                    jm.UserName = I18NEntity.GetString("RM_TS_RunSchedule");
                }
                if (jm.UserName == "RM_JS_Common_Pending")
                {
                    jm.UserName = I18NEntity.GetString("RM_JS_Common_Pending");
                }
                if (!string.IsNullOrEmpty(jm.Comment))
                {
                    jm.Comment = I18NEntity.GetString(jm.Comment);
                }
            }
            return jms;
        }
        /// <summary>
        /// 删除job, 级联删除jobcontext, subjob, job
        /// </summary>
        /// <param name="idArray"></param>
        /// <returns></returns>
        public int DeleteJobs(List<string> idArray)
        {
            if (idArray == null || idArray.Count == 0)
            {
                return 0;
            }

            var batchJobSize = 50;
            var deletedJobCount = 0;

            try
            {
                using (var context = RMDBContextManager.GetNewDBContext())
                {
                    context.Database.CommandTimeout = 600; //seconds
                    var dbSchema = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                    foreach (var batch in ChunkList(idArray, batchJobSize))
                    {
                        if (batch.Count == 0)
                        {
                            continue;
                        }

                        logger.Info($"Start to delete job contexts: {string.Join(", ", batch)}");

                        var batchDeleteSize = 2000;
                        var jobIdsInClause = DatabaseUtility.BuildInClause(batch, out var jobIdsParams);
                        var removeContextSucceeded = true;
                        var sqlDelJobContextByJobId = $"delete top ({batchDeleteSize}) from {dbSchema}.RMJobContexts where JobId in {jobIdsInClause}";
                        var sqlDelJobContextByMainJobId = $"delete top ({batchDeleteSize}) from {dbSchema}.RMJobContexts where MainJobId in {jobIdsInClause}";

                        void DeleteJobContext(string sql)
                        {
                            int removedRows;
                            do
                            {
                                removedRows = context.Database.ExecuteSqlCommand(sql, jobIdsParams.ToArray());
                            }
                            while (removedRows >= batchDeleteSize);
                        };

                        try
                        {
                            DeleteJobContext(sqlDelJobContextByJobId);
                            DeleteJobContext(sqlDelJobContextByMainJobId);
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Failed to delete job context for Jobs: {string.Join(", ", batch)}. Error: {ex}.");
                            removeContextSucceeded = false;
                            continue;
                        }

                        var removeSubJobsSucceeded = true;
                        var sqlDelSubJobs = $"delete top ({batchDeleteSize}) from {dbSchema}.RMSubJobs where ParentId in {jobIdsInClause}";
                        logger.Info($"Start to delete job subJobs: {string.Join(", ", batch)}");
                        try
                        {
                            int removedRows;
                            do
                            {
                                removedRows = context.Database.ExecuteSqlCommand(sqlDelSubJobs, jobIdsParams.ToArray());
                            }
                            while (removedRows >= batchDeleteSize);
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Failed to delete sub jobs: {string.Join(", ", batch)}. Error: {ex}.");
                            removeSubJobsSucceeded = false;
                            continue;
                        }

                        if (!removeContextSucceeded || !removeSubJobsSucceeded)
                        {
                            continue;
                        }

                        logger.Info($"Start to delete RMJobMonitors: {string.Join(", ", batch)}");
                        var sqlDelJobs = $"delete from {dbSchema}.RMJobMonitors where Id in {jobIdsInClause}";
                        try
                        {
                            context.Database.ExecuteSqlCommand(sqlDelJobs, jobIdsParams.ToArray());
                            deletedJobCount += batch.Count;
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Failed to delete jobs. {string.Join(", ", batch)}. Error: {ex}.");
                        }

                    }
                }

                return deletedJobCount;
            }
            catch (Exception e)
            {
                logger.Error("Delete jobs error:{0}.", e.ToString());
                return deletedJobCount;
            }
        }

        private static IEnumerable<List<T>> ChunkList<T>(List<T> source, int chunkSize)
        {
            if (source == null || source.Count == 0)
            {
                yield break;
            }

            if (chunkSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chunkSize));
            }

            for (var index = 0; index < source.Count; index += chunkSize)
            {
                var length = Math.Min(chunkSize, source.Count - index);
                yield return source.GetRange(index, length);
            }
        }

        public async Task<int> DeleteJobByJobTypes(List<JobType> jobTypes)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var jobIds = await context.JobMonitors.AsQueryable().Where(item => jobTypes.Contains((JobType)item.JobType)).Select(item => item.Id).ToListAsync();
            return DeleteJobs(jobIds);
        }

        /// <summary>
        /// 主job更新成Stopping, 子job Wait状态的直接更新成Stopped, Waiting+Runable子job更成Stopping；   如果主子job全都Wait状态， 并且没发消息到Queue， 此方法有问题。
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public int StopJobs(List<string> ids)
        {
            try
            {
                logger.Info($"start stop jobs,ids:{string.Join(',', ids)}");
                //List<RMJobMonitor> needStoppingJobs = new List<RMJobMonitor>();
                List<SqlParameter> paras = null;
                var parameterizedStatement = DatabaseUtility.BuildInClause(ids, out paras);
                using (var context = RMDBContextManager.GetNewDBContext())
                {
                    string mainSql = "update {0}.RMJobMonitors set Status = 7 where Status = 1 and Id in " + parameterizedStatement;
                    logger.Info("start mainJobRow ExecuteSqlCommand");
                    int mainJobRow = context.Database.ExecuteSqlCommand(string.Format(mainSql, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)), paras.ToArray());
                    logger.Info("start subJobRowWaiting ExecuteSqlCommand");
                    int subJobRowWaiting = DeleteWaitingSubJobsInBatches(context, parameterizedStatement, paras.ToArray());
                    string subSqlRuning = "update {0}.RMSubJobs set status = 7 where (Status = 0 or Status = 1) and (Runable = 2 OR Runable = -1) and ParentId in " + parameterizedStatement;
                    string mainStoppedSql = "update {0}.RMJobMonitors set Status = 5 , EndTime = {1} where Status = 0 and Id in " + parameterizedStatement;
                    //string subSqlWaitingDelete = "DELETE FROM {0}.RMSubJobs WHERE Status = 5 AND ParentId IN " + parameterizedStatement;
                    using (DbContextTransaction tran = context.Database.BeginTransaction())
                    {
                        logger.Info("start mainJobStoppedRow ExecuteSqlCommand");
                        int mainJobStoppedRow = context.Database.ExecuteSqlCommand(string.Format(mainStoppedSql, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName), DateTime.UtcNow.Ticks), paras.ToArray());
                        logger.Info("start subJobRowRunning ExecuteSqlCommand");
                        int subJobRowRunning = context.Database.ExecuteSqlCommand(string.Format(subSqlRuning, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)), paras.ToArray());
                        //logger.Info("start subJobRowDeleting ExecuteSqlCommand");
                        //int subJobRowDeleting = context.Database.ExecuteSqlCommand(string.Format(subSqlWaitingDelete, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)), paras.ToArray());
                        logger.Info("start tran.Commit");
                        tran.Commit();
                        //logger.Info("Deleted {0} subjobs with status 5.", subJobRowDeleting);
                        logger.Info("Stop job, update {0} jobs to stopping;  update {1} waiting,{2} running subjobs to stopped, update {3} jobs to stopping.", mainJobRow, subJobRowWaiting, subJobRowRunning, mainJobStoppedRow);

                        foreach (var id in ids)
                        {
                            int subSqlInProgressRow = context.Database.SqlQuery<int>($"Select count(*) from {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.RMSubJobs where (Status = {(int)JobStatus.InProgress} or Status = 7) and ParentId = @id", new SqlParameter("@id", id)).FirstOrDefault();
                            if (subSqlInProgressRow <= 0)
                            {
                                var count = context.Database.ExecuteSqlCommand($"update {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.RMJobMonitors set Status = 5 , EndTime = {DateTime.UtcNow.Ticks} where Id = @id and SubJobCount >0 ", new SqlParameter("@id", id));
                                if (count > 0)
                                {
                                    logger.Info($"set {id} to stop.");
                                }
                            }
                            else
                            {
                                logger.Info($"there exsit subjob that is inprogress,count:{subSqlInProgressRow},id:{id}");
                            }
                        }
                        logger.Info($"job stopped finish,ids:{string.Join(",", ids)},mainJobRow:{mainJobRow},mainJobStoppedRow:{mainJobStoppedRow}");
                        return mainJobRow + mainJobStoppedRow;
                    }
                    //foreach (string id in ids)
                    //{
                    //    RMJobMonitor job = context.JobMonitors.AsQueryable().First(c => c.Id == id);
                    //    if (job != null && (job.Status == (int)JobStatus.InProgress || job.Status == (int)JobStatus.Wait))
                    //    {
                    //        job.Status = (int)JobStatus.Stopping;
                    //        needStoppingJobs.Add(job);
                    //    }
                    //}
                }
            }
            catch (Exception e)
            {
                logger.Error("Stop jobs error:{0}.", e.ToString());
                throw;
            }
            //try
            //{
            //    return BatchUpdate(needStoppingJobs);
            //}
            //catch (Exception e)
            //{
            //    logger.Error("Stop jobs error:{0}.", e.ToString());
            //    return 0;
            //}
        }

        private int DeleteWaitingSubJobsInBatches(RMDbContext context, string parentIdsStatement, object[] parameters)
        {
            const int batchSize = 5000;
            string sql = @"
                ;WITH SubJobsToDelete AS
                (
                    SELECT TOP (" + batchSize + @") Id
                    FROM {0}.RMSubJobs
                    WHERE (((Status = 0 AND Runable != 2) OR LastUpdateTime < " + DateTime.UtcNow.AddMinutes(-30).Ticks + @") OR Status = 5)
                        AND ParentId IN " + parentIdsStatement + @"
                    ORDER BY Id
                )
                DELETE FROM SubJobsToDelete";
            string sanitizedSchemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
            int totalDeletedRows = 0;
            int deletedRows;
            do
            {
                deletedRows = context.Database.ExecuteSqlCommand(string.Format(sql, sanitizedSchemaName), parameters);
                totalDeletedRows += deletedRows;
            }
            while (deletedRows == batchSize);

            return totalDeletedRows;
        }

        public RMJobMonitor GetLastFinishedJob(JobType jobType)
        {
            var jobTypeVal = (int)jobType;
            var finishStatus = (int)JobStatus.Finished;
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.JobMonitors
                    .Where(j => j.JobType == jobTypeVal && j.Status == finishStatus)
                    .OrderByDescending(j => j.Id)
                    .FirstOrDefault();
            }
        }

        public List<string> GetRunningJobs(JobType jobType)
        {
            List<string> runningJobs = new List<string>();
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                if (jobType == JobType.All)
                {
                    var tempList = context.JobMonitors.AsQueryable().Where(c => c.JobType != (int)JobType.ManualApprovalLocationTest && c.JobType != (int)JobType.DisposalActivityManagement && (c.Status == (int)JobStatus.InProgress || c.Status == (int)JobStatus.Wait)).ToList();
                    foreach (var job in tempList)
                    {
                        if ((job.JobType == (int)JobType.ArchiverRestore 
                            || job.JobType == (int)JobType.ArchiverOutPlaceRestore 
                            || job.JobType == (int)JobType.StubOopRestore 
                            || job.JobType == (int)JobType.AOSPRestore 
                            || job.JobType == (int)JobType.ExportAdvanceSeachResult 
                            || job.JobType == (int)JobType.ExportRestoreCenterSeachResult 
                            || job.JobType == (int)JobType.TeamsArchiverRestore 
                            || job.JobType == (int)JobType.GoogleArchiverRestore 
                            || job.JobType == (int)JobType.MailBoxArchiverRestore
                            || job.JobType == (int)JobType.ArchiverToSpoRestore
                            || job.JobType == (int)JobType.StubArchiverRestore
                            || job.JobType == (int)JobType.M365InPlaceArchiverRestore
                            ) && !string.IsNullOrEmpty(job.AdditionalInformation))
                        {
                            try
                            {
                                XmlDocument doc = new XmlDocument();
                                doc.LoadXml(job.AdditionalInformation);
                                bool isNotEndUserRestore = string.IsNullOrEmpty(doc.DocumentElement.HasAttribute("ReCenterRunJobUser") ? doc.DocumentElement.GetAttribute("ReCenterRunJobUser") : string.Empty);
                                if (isNotEndUserRestore)
                                {
                                    runningJobs.Add(job.Id);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Warn($"GetRunningJobs error {e}");
                                runningJobs.Add(job.Id);
                            }
                        }
                        else if (job.JobType == (int)JobType.MoveDataTier || job.JobType == (int)JobType.AdjustStorageSize || job.JobType == (int)JobType.ApprovalProcessArchive)
                        {
                            continue;
                        }
                        else
                        {
                            runningJobs.Add(job.Id);
                        }
                    }
                }
                else
                {
                    runningJobs = context.JobMonitors.AsQueryable().Where(c => c.JobType == (int)jobType && (c.Status == (int)JobStatus.InProgress || c.Status == (int)JobStatus.Wait)).Select(c => c.Id).ToList();
                }
            }
            return runningJobs;
        }

        public List<RMJobMonitor> GetRunningJobs(List<JobType> jobTypes, string scopeId)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                List<int> tempJobTypes = new List<int>();
                foreach (var item in jobTypes)
                {
                    tempJobTypes.Add((int)item);
                }
                return context.JobMonitors.AsQueryable().Where(c => tempJobTypes.Contains(c.JobType) && (c.Status == (int)JobStatus.InProgress || c.Status == (int)JobStatus.Wait || c.Status == (int)JobStatus.Stopping) && c.ScopeId == scopeId).ToList();
            }

        }

        public List<RMJobMonitor> GetRunningJobsBatch(List<JobType> jobTypes, List<string> scopeIds)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                List<int> tempJobTypes = new List<int>();
                foreach (var item in jobTypes)
                {
                    tempJobTypes.Add((int)item);
                }
                return context.JobMonitors.AsQueryable().Where(c => tempJobTypes.Contains(c.JobType) && 
                (c.Status == (int)JobStatus.InProgress || c.Status == (int)JobStatus.Wait || c.Status == (int)JobStatus.Stopping) &&
                scopeIds.Contains(c.ScopeId)).ToList();
            }
        }

        public List<RMJobMonitor> GetRunningJobs(List<JobType> jobTypes)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                List<int> tempJobTypes = new List<int>();
                foreach (var item in jobTypes)
                {
                    tempJobTypes.Add((int)item);
                }
                return context.JobMonitors.AsQueryable().Where(c => tempJobTypes.Contains(c.JobType) && (c.Status == (int)JobStatus.InProgress || c.Status == (int)JobStatus.Wait || c.Status == (int)JobStatus.Stopping)).ToList();
            }
        }

        public List<RMJobMonitor> GetRunningExpectStoppingJobs(List<string> jobIds)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.JobMonitors.AsQueryable().Where(c => jobIds.Contains(c.Id) && (c.Status == (int)JobStatus.InProgress || c.Status == (int)JobStatus.Wait)).ToList();
            }
        }


        public List<string> GetRunningJobsScopeId(JobType jobType)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.JobMonitors.AsQueryable().Where(c => c.JobType == (int)jobType && (c.Status == (int)JobStatus.InProgress || c.Status == (int)JobStatus.Wait || c.Status == (int)JobStatus.Stopping)).Select(c => c.ScopeId).ToList();
            }
        }

        public async Task<bool> IsHavingRunningJob()
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return await context.JobMonitors.AnyAsync(c =>
                c.Status == (int)JobStatus.InProgress || c.Status == (int)JobStatus.Wait);
        }

        public List<string> GetSharePointSettingJobs()
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.JobMonitors.AsQueryable().Where(c => (c.JobType == (int)JobType.ApplySharePointSettings || c.JobType == (int)JobType.SharePointScheduleSetting) && (c.Status == (int)JobStatus.InProgress || c.Status == (int)JobStatus.Wait || c.Status == (int)JobStatus.Stopping)).Select(c => c.Id).ToList();
            }

        }

        public List<string> GetTeamsSettingJobs()
        {
            using(var context = RMDBContextManager.GetNewDBContext())
            {
                return context.JobMonitors.AsQueryable().Where(c => (c.JobType == (int)JobType.ApplyTeamsSettings || c.JobType == (int)JobType.TeamsScheduleSetting) && (c.Status == (int)JobStatus.Wait || c.Status == (int)JobStatus.InProgress || c.Status == (int)JobStatus.Stopping)).Select(c=> c.Id).ToList();
            }
        }

        public List<RMJobMonitor> GetUnstatisticFinishRestoreJobsByTime(long startTimeTicks, long finishTimeTicks)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.JobMonitors.AsQueryable().Where(
                    c => (c.JobType == (int)JobType.ArchiverRestore 
                        || c.JobType == (int)JobType.ArchiverOutPlaceRestore 
                        || c.JobType == (int)JobType.StubOopRestore 
                        || c.JobType == (int)JobType.AOSPRestore 
                        || c.JobType == (int)JobType.TeamsArchiverRestore 
                        || c.JobType == (int)JobType.MailBoxArchiverRestore
                        || c.JobType == (int)JobType.ArchiverToSpoRestore
                        || c.JobType == (int)JobType.StubArchiverRestore
                        || c.JobType == (int)JobType.M365InPlaceArchiverRestore
                        )
                    && (c.Status == (int)JobStatus.Finished || c.Status == (int)JobStatus.FinishWithException)
                    && c.EndTime <= finishTimeTicks && c.StartTime >= startTimeTicks
                    && (c.RestoreStatisticStatus == (int)MonitorStatisticStatus.UnStatistic || c.RestoreStatisticStatus == (int)MonitorStatisticStatus.PossbileFail))
                    .ToList<RMJobMonitor>();
            }
        }
        public List<RMJobMonitor> GetUnstatisticFinishRestoreGoogleJobsByTime(long startTimeTicks, long finishTimeTicks)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.JobMonitors.AsQueryable().Where(
                    c => (c.JobType == (int)JobType.GoogleArchiverRestore)
                    && (c.Status == (int)JobStatus.Finished || c.Status == (int)JobStatus.FinishWithException)
                    && c.EndTime <= finishTimeTicks && c.StartTime >= startTimeTicks
                    && (c.RestoreStatisticStatus == (int)MonitorStatisticStatus.UnStatistic || c.RestoreStatisticStatus == (int)MonitorStatisticStatus.PossbileFail))
                    .ToList<RMJobMonitor>();
            }
        }

        public List<RMJobMonitor> GetUnstatisticFinishMigrationRestoreJobsByTime(long startTimeTicks, long finishTimeTicks)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.JobMonitors.AsQueryable().Where(
                    c => (c.JobType == (int)JobType.MigrationArchiverRestore)
                    && (c.Status == (int)JobStatus.Finished || c.Status == (int)JobStatus.FinishWithException)
                    && c.EndTime <= finishTimeTicks && c.StartTime >= startTimeTicks
                    && (c.RestoreStatisticStatus == (int)MonitorStatisticStatus.UnStatistic || c.RestoreStatisticStatus == (int)MonitorStatisticStatus.PossbileFail))
                    .ToList<RMJobMonitor>();
            }
        }


        public List<string> GetRunningEXOApplySettingJob()
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.JobMonitors.AsQueryable().Where(c => (c.JobType == (int)JobType.EXOApplySetting || c.JobType == (int)JobType.EXOApplySettingSchedule) && (c.Status == (int)JobStatus.InProgress || c.Status == (int)JobStatus.Wait || c.Status == (int)JobStatus.Stopping)).Select(c => c.Id).ToList();
            }
        }
        public List<string> GetSharePointOnPremiseSettingJobs()
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.JobMonitors.AsQueryable().Where(c => (c.JobType == (int)JobType.SPOnPremApplySetting || c.JobType == (int)JobType.SPOnPremApplySettingSchedule) && (c.Status == (int)JobStatus.InProgress || c.Status == (int)JobStatus.Wait)).Select(c => c.Id).ToList();
            }
        }
        /// <summary>
        /// Move up to maxRowsPerRun RMJobMonitor rows older than olderThanDays into RMJobMonitorArchive.
        /// Returns number of inserted archive rows. Duplicate-safe and transactional.
        /// Optimized: set-based INSERT…SELECT + DELETE in one roundtrip (no EF materialization/reflection).
        /// </summary>
        public int ArchiveDataBatch(int maxRowsPerRun, int olderThanDays, IReadOnlyCollection<int> archiveJobTypes)
        {
            if (archiveJobTypes == null || archiveJobTypes.Count == 0)
            {
                return 0;
            }

            var maxRows = Math.Max(1, maxRowsPerRun);
            var olderThanTicks = DateTime.UtcNow.AddDays(-Math.Max(0, olderThanDays)).Ticks;
            var finalStatuses = new[] { (int)JobStatus.Finished, (int)JobStatus.Failed, (int)JobStatus.FinishWithException, (int)JobStatus.Skipped };

            using (var ctx = RMDBContextManager.GetNewDBContext(10 * 60)) 
            using (var tx = ctx.Database.BeginTransaction())
            {
                try
                {
                    // Build IN clause for job types using parameters
                    List<SqlParameter> jobTypeParams;
                    var jobTypeInClause = DatabaseUtility.BuildInClause(archiveJobTypes, out jobTypeParams);
                    var schema = SecurityUtils.SanitizeSQLSchemaName(ctx.SchemaName);
                    var monitorsTable = $"[{schema}].[RMJobMonitors]";
                    var archivesTable = $"[{schema}].[RMJobMonitorArchives]";

                    // Compose one T-SQL batch: pick TOP(@maxRows) eligible rows, insert into archive with OUTPUT to a table variable,
                    // then delete the same Ids from the source table, and finally return the count inserted.
                    var sql = $@"
DECLARE @moved TABLE (Id nvarchar(1024) PRIMARY KEY);
WITH cte AS (
    SELECT TOP (@maxRows)
           m.Id, m.JobType, m.StartTime, m.EndTime, m.Status, m.Progress, m.DoubleProgress,
           m.ScopeId, m.ProfileId, m.Comment, m.UserName, m.LastUpdateTime, m.SubJobCount,
           m.ContainerId, m.NodeType, m.ExceptionType, m.AdditionalInformation,
           m.DiscoveryMainJobId, m.DiscoveryJobId, m.DAOMigrated, m.RestoreStatisticStatus,
           m.Extension, m.JobConflictExtension
        FROM {monitorsTable} AS m WITH (READPAST)
    WHERE m.EndTime > 0 AND m.EndTime < @olderThanTicks
      AND m.Status IN ({string.Join(",", finalStatuses)})
      AND m.JobType IN {jobTypeInClause}
            AND NOT EXISTS (SELECT 1 FROM {archivesTable} a WHERE a.Id = m.Id)
    ORDER BY m.EndTime
)
INSERT INTO {archivesTable}
    (Id, JobType, StartTime, EndTime, Status, Progress, DoubleProgress,
     ScopeId, ProfileId, Comment, UserName, LastUpdateTime, SubJobCount,
     ContainerId, NodeType, ExceptionType, AdditionalInformation,
     DiscoveryMainJobId, DiscoveryJobId, DAOMigrated, RestoreStatisticStatus,
     Extension, JobConflictExtension)
OUTPUT inserted.Id INTO @moved(Id)
SELECT Id, JobType, StartTime, EndTime, Status, Progress, DoubleProgress,
       ScopeId, ProfileId, Comment, UserName, LastUpdateTime, SubJobCount,
       ContainerId, NodeType, ExceptionType, AdditionalInformation,
       DiscoveryMainJobId, DiscoveryJobId, DAOMigrated, RestoreStatisticStatus,
       Extension, JobConflictExtension
FROM cte;

DELETE m
FROM {monitorsTable} AS m
INNER JOIN @moved x ON x.Id = m.Id;

SELECT COUNT(1) FROM @moved;";

                    var parameters = new List<SqlParameter>(jobTypeParams)
                    {
                        new SqlParameter("maxRows", maxRows),
                        new SqlParameter("olderThanTicks", olderThanTicks)
                    };

                    // Execute and get number of archived rows
                    var moved = ctx.Database.SqlQuery<int>(sql, parameters.ToArray()).FirstOrDefault();
                    tx.Commit();
                    return moved;
                }
                catch (Exception ex)
                {
                    try { tx.Rollback(); } catch { }
                    logger.Error($"ArchiveDataBatch failed: {ex}");
                    throw;
                }
            }
        }

        private static void CopyProperties(RMJobMonitor src, RMJobMonitorArchive dest)
        {
            var sProps = typeof(RMJobMonitor).GetProperties();
            var dProps = typeof(RMJobMonitorArchive).GetProperties().ToDictionary(p => p.Name);
            foreach (var sp in sProps)
            {
                if (!sp.CanRead) continue;
                if (dProps.TryGetValue(sp.Name, out var dp) && dp.CanWrite && dp.PropertyType == sp.PropertyType)
                {
                    var val = sp.GetValue(src);
                    dp.SetValue(dest, val);
                }
            }
        }

        public List<string> GetTimeOutJobIds(int timeoutMinutesForRecordsJobInProgress, int timeoutMinutesForRecordsJobWaiting)
        {
            long commontimespan = DateTime.UtcNow.AddMinutes(0 - timeoutMinutesForRecordsJobInProgress).Ticks;
            long commontimespanForWaiting = DateTime.UtcNow.AddMinutes(0 - timeoutMinutesForRecordsJobWaiting).Ticks;
            int progress = (int)JobStatus.InProgress;
            int subJobRunning = Common.RecordsConstants.SubJob_Runnable_Runing;
            //int waiting = (int)JobStatus.Wait;

            using (var context = GetNewContext())
            {
                List<string> mainJobs = context.JobMonitors.Where(a => (a.Status == progress || a.Status == (int)JobStatus.Stopping)
                && (!Enumerable.Contains(JobServiceUtility.JobTypesHasSubJobAndDisposal, a.JobType) && a.LastUpdateTime < commontimespan)).Select(s => s.Id).ToList();

                List<string> subJobs = context.RMSubJobs.Where(a => (a.Status == progress || a.Status == (int)JobStatus.Stopping) && a.JobType != (int)JobType.DiscoveryJob && Enumerable.Contains(JobServiceUtility.JobTypesHasSubJob, a.JobType) && a.LastUpdateTime < commontimespan).Select(a => a.Id).ToList();
                mainJobs.AddRange(subJobs);

                List<string> waitingSubJobs = context.RMSubJobs.Where(a => a.JobType != (int)JobType.DiscoveryJob && a.Status == (int)JobStatus.Wait && a.Runable == Common.RecordsConstants.SubJob_Runnable_Runing && Enumerable.Contains(JobServiceUtility.JobTypesHasSubJob, a.JobType) && a.LastUpdateTime < commontimespanForWaiting).Select(a => a.Id).ToList();
                mainJobs.AddRange(waitingSubJobs);
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var job in waitingSubJobs)
                {
                    stringBuilder.Append(job);
                    stringBuilder.Append(',');
                }
                if (!string.IsNullOrEmpty(stringBuilder.ToString()))
                {
                    logger.Info($"Set status to timeout for waitting subjob(Runnable is 2), ids: {stringBuilder.ToString()}");
                }
                var schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                //条件:有主Job和子Job,且主Job的状态和子Job的状态都为Waiting
                List<string> waitingJobs = context.Database.SqlQuery<string>($"select JM.ID FROM  {schemaName}.RMJobMonitors JM JOIN {schemaName}.RMSubJobs SJ ON JM.Id=SJ.ParentId WHERE JM.status={(int)JobStatus.Wait} and SJ.status={(int)JobStatus.Wait} and JM.LastUpdateTime<{commontimespanForWaiting}")
                    .Distinct().ToList();

                mainJobs.AddRange(waitingJobs);

                //条件:只有主Job没有子Job,且主Job状态为waiting
                List<string> waitingJobOfNoSubJob = context.Database.SqlQuery<string>($"SELECT JM.ID FROM {schemaName}.RMJobMonitors JM WHERE JM.Status = {(int)JobStatus.Wait} AND JM.LastUpdateTime<{commontimespanForWaiting} AND NOT EXISTS (SELECT 1 FROM {schemaName}.RMSubJobs SJ WHERE SJ.ParentId = JM.Id)")
                    .Distinct().ToList();
                mainJobs.AddRange(waitingJobOfNoSubJob);

                return mainJobs;
            }
        }

        public List<RMJobMonitor> GetPermittedJobByScopeId(int jobType, string scopeId, int[] securityGroupId, int[] status)
        {
            using (var context = GetNewContext())
            {
                List<SqlParameter> sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@JobType", jobType),
                    new SqlParameter("@ScopeId", scopeId),
                };

                var statusInParamName = DatabaseUtility.BuildInClause(status, out var paramList);
                sqlParams.AddRange(paramList);
                var securityGroupInParamName = DatabaseUtility.BuildInClause(securityGroupId, out var paramList2);
                sqlParams.AddRange(paramList2);

                var schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);

                string sql = $@"select * from {schemaName}.RMJobMonitors as j where j.JobType = @JobType and j.ScopeId = @ScopeId and j.Status in {statusInParamName} and
(
( exists ( select m.GroupId from {schemaName}.RMSecurityGroupMemberships as m  where m.UserId in (select a.UserId from {schemaName}.RMAccounts as a where a.UserPrincipalName = j.UserName ) and m.GroupId in {securityGroupInParamName}))
or 
(exists(select m.GroupId from {schemaName}.RMSecurityGroupMemberships as m join {schemaName}.RMLnkUserGroups as l on m.UserId = l.GroupId where l.UserId in (select a.UserId from {schemaName}.RMAccounts as a where a.UserPrincipalName = j.UserName )
and m.GroupId in {securityGroupInParamName}))
)
order by j.starttime desc";

                return context.Database.SqlQuery<RMJobMonitor>(sql, sqlParams.ToArray()).ToList();
            }
        }
        public List<RMJobMonitor> GetPermittedJobByScopeId(int jobType, string scopeId, string userId, int[] status)
        {
            using (var context = GetNewContext())
            {
                List<SqlParameter> sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@JobType", jobType),
                    new SqlParameter("@ScopeId", scopeId),
                    new SqlParameter("@UserId", userId)
                };

                var statusInParamName = DatabaseUtility.BuildInClause(status, out var paramList);
                sqlParams.AddRange(paramList);
                var schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);

                string sql = $@"select * from {schemaName}.RMJobMonitors as j where j.JobType = @JobType and j.ScopeId = @ScopeId and j.Status in {statusInParamName} and
(( exists ( select m.GroupId from {schemaName}.RMSecurityGroupMemberships as m  where m.UserId in (select a.UserId from {schemaName}.RMAccounts as a where a.UserPrincipalName = j.UserName ) 
and 
exists (select mm.GroupId from {schemaName}.RMSecurityGroupMemberships as mm where mm.GroupId = m.GroupId and mm.userId = @UserId))
)
or (
exists(select m.GroupId from {schemaName}.RMSecurityGroupMemberships as m join {schemaName}.RMLnkUserGroups as l on m.UserId = l.GroupId where l.UserId in (select a.UserId from {schemaName}.RMAccounts as a where a.UserPrincipalName = j.UserName )
and exists (select mm.GroupId from {schemaName}.RMSecurityGroupMemberships as mm where mm.GroupId = m.GroupId and mm.userId = l.GroupId and l.UserId = @UserId))
))
order by j.starttime desc";


                return context.Database.SqlQuery<RMJobMonitor>(sql, sqlParams.ToArray()).ToList();
            }
        }
        public List<RMJobMonitor> GetPermittedFinalJobByScopeId(int jobType, string scopeId, string userId)
        {
            string sql = @"select * from {0}.RMJobMonitors as j where j.JobType = @JobType and j.ScopeId = @ScopeId and j.Status not in (0, 1) and
(( exists ( select m.GroupId from {0}.RMSecurityGroupMemberships as m  where m.UserId in (select a.UserId from {0}.RMAccounts as a where a.UserPrincipalName = j.UserName ) 
and 
exists (select mm.GroupId from {0}.RMSecurityGroupMemberships as mm where mm.GroupId = m.GroupId and mm.userId = @UserId))
)
or (
exists(select m.GroupId from {0}.RMSecurityGroupMemberships as m join {0}.RMLnkUserGroups as l on m.UserId = l.GroupId where l.UserId in (select a.UserId from {0}.RMAccounts as a where a.UserPrincipalName = j.UserName )
and exists (select mm.GroupId from {0}.RMSecurityGroupMemberships as mm where mm.GroupId = m.GroupId and mm.userId = l.GroupId and l.UserId = @UserId))
))
order by j.starttime desc";
            var sqlParams = new SqlParameter[]
                {
                    new SqlParameter("@JobType", jobType),
                    new SqlParameter("@ScopeId", scopeId),
                    new SqlParameter("@UserId", userId)
                };
            using (var context = GetNewContext())
            {
                return context.Database.SqlQuery<RMJobMonitor>(string.Format(sql, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)), sqlParams).ToList();
            }
        }
        public List<RMJobMonitor> GetRunningAndWaitingJobs()
        {
            using (var context = GetNewContext())
            {
                return context.JobMonitors.Where(c => c.Status == (int)JobStatus.InProgress
                                                    || c.Status == (int)JobStatus.Wait).ToList();
            }
        }

        public List<RMJobMonitor> GetRunningJobsByProfileIds(List<int> profileIds)
        {
            using (var context = GetNewContext())
            {
                var results = context.JobMonitors.Where(c => c.Status == (int)JobStatus.InProgress && c.ProfileId.HasValue && profileIds.Contains(c.ProfileId.Value)).SortBy("StartTime", SortDirectionEnum.Descending).ToList();
                return results;
            }
        }

        public string GetJobFakeidByKey(string key)
        {
            using (var context = GetNewContext())
            {
                var result = context.JobMonitors.Where(c => c.ScopeId == key).FirstOrDefault();
                return result == null ? string.Empty : result.Id;
            }
        }

        public List<string> GetUniqueIDSettingJobs()
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.JobMonitors.AsQueryable().Where(c => (c.JobType == (int)JobType.UniqueIDSettingFullSchedule || c.JobType == (int)JobType.UniqueIDSettingIncrementalSchedule) && (c.Status == (int)JobStatus.InProgress || c.Status == (int)JobStatus.Wait)).Select(c => c.Id).ToList();
            }

        }
        
        public List<string> GetTeamsUniqueIDSettingJobs()
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.JobMonitors.AsQueryable().Where(c => (c.JobType == (int)JobType.TeamsUniqueIDSettingFullSchedule || c.JobType == (int)JobType.TeamsUniqueIDSettingIncrementalSchedule) && (c.Status == (int)JobStatus.InProgress || c.Status == (int)JobStatus.Wait)).Select(c => c.Id).ToList();
            }

        }

        public List<string> GetSPOnPremUniqueIDSettingJobs()
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.JobMonitors.AsQueryable().Where(c => (c.JobType == (int)JobType.SPOnPremUniqueIDSettingFullSchedule || c.JobType == (int)JobType.SPOnPremUniqueIDSettingIncrementalSchedule) && (c.Status == (int)JobStatus.InProgress || c.Status == (int)JobStatus.Wait)).Select(c => c.Id).ToList();
            }

        }

        public List<string> GetRunningSyncSecurityContainerJob()
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.JobMonitors.AsQueryable().Where(c => c.JobType == (int)JobType.SyncSecurityContainer && (c.Status == (int)JobStatus.InProgress || c.Status == (int)JobStatus.Wait)).Select(c => c.Id).ToList();
            }
        }

        public List<string> GetCollectionDataSettingJobs()
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.JobMonitors.AsQueryable().Where(c => (c.JobType == (int)JobType.DataSynchronisation) && (c.Status == (int)JobStatus.InProgress || c.Status == (int)JobStatus.Wait)).Select(c => c.Id).ToList();
            }

        }

        public List<RMJobMonitor> GetFailedJobInfoByTimeRange(TimeSpan timeRange, List<JobType> excludeJobTypes = null)
        {
            var ticket = DateTime.UtcNow.AddSeconds(-timeRange.TotalSeconds).Ticks;

            var jobStatus = new List<int>() { (int)JobStatus.Failed, (int)JobStatus.FinishWithException };
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                if (excludeJobTypes != null)
                {
                    var jobtypes = excludeJobTypes.ConvertAll(j => (int)j);
                    return context.JobMonitors.Where(j => !jobtypes.Contains(j.JobType) && j.LastUpdateTime >= ticket && jobStatus.Contains(j.Status)).ToList();
                }
                else
                {
                    return context.JobMonitors.Where(j => j.LastUpdateTime >= ticket && jobStatus.Contains(j.Status)).ToList();
                }
            }
        }       
        
        public List<RMJobMonitor> GetJobInfoByTimeRangeAndStatus(long startTime, long endTime, List<JobType> excludeJobTypes, List<JobStatus> excludeJobStatuses)
        {
            var jobStatus = excludeJobStatuses.Select(jobStatus => (int)jobStatus);
            using var context = RMDBContextManager.GetNewDBContext();
            if (excludeJobTypes != null)
            {
                var jobtypes = excludeJobTypes.ConvertAll(j => (int)j);
                return
                [
                    .. context.JobMonitors.Where(j => jobtypes.Contains(j.JobType) && j.LastUpdateTime >= startTime && j.LastUpdateTime <= endTime && jobStatus.Contains(j.Status)),
                ];
            }
            else
            {
                return [];
            }
        }
        /// <summary>
        /// //查询Job 更新时间在指定范围内timeRange, 正在运行或者完成的Job, 使用时间超过指定时间范围longRunningTimeRange.
        /// </summary>
        /// <param name="timeRange"></param>
        /// <param name="longRunningTimeRange"></param>
        /// <param name="excludeJobTypes"></param>
        /// <returns></returns>
        public List<RMJobMonitor> GetLongRunningJobInfoByTimeRange(TimeSpan timeRange, TimeSpan longRunningTimeRange, List<JobType> excludeJobTypes = null)
        {
            //查询 job 的范围, 最近多久更新的Job.
            var queryScopeTimeRangeTicket = DateTime.UtcNow.AddSeconds(-timeRange.TotalSeconds).Ticks;
            //job 运行时间
            var runningTimeTicket = longRunningTimeRange.Ticks;
            var nowTickets = DateTime.UtcNow.Ticks;
            var runningJobStatus = new List<int>() { (int)JobStatus.InProgress, (int)JobStatus.Wait };
            var jobFinishStatus = new List<int>() { (int)JobStatus.Finished, (int)JobStatus.FinishWithException, (int)JobStatus.Failed };
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                if (excludeJobTypes != null)
                {
                    var jobtypes = excludeJobTypes.ConvertAll(j => (int)j);
                    return context.JobMonitors.Where(j => !jobtypes.Contains(j.JobType) && j.LastUpdateTime >= queryScopeTimeRangeTicket && ((nowTickets - j.StartTime) >= runningTimeTicket && runningJobStatus.Contains(j.Status) || (jobFinishStatus.Contains(j.Status) && (j.EndTime - j.StartTime) >= runningTimeTicket))).ToList();
                }
                else
                {
                    return context.JobMonitors.Where(j => j.LastUpdateTime >= queryScopeTimeRangeTicket && ((nowTickets - j.StartTime) >= runningTimeTicket && runningJobStatus.Contains(j.Status) || (jobFinishStatus.Contains(j.Status) && (j.EndTime - j.StartTime) >= runningTimeTicket))).ToList();
                }
            }
        }

        public List<RMJobMonitor> GetSpecificJobExeptionInfoByTimeRange(TimeSpan timeRange, List<JobType> excludeJobTypes = null)
        {
            var queryTickets = DateTime.UtcNow.AddSeconds(-timeRange.TotalSeconds).Ticks;

            var jobStatus = new List<int>() { (int)JobStatus.Failed, (int)JobStatus.FinishWithException };
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                if (excludeJobTypes != null)
                {
                    var jobtypes = excludeJobTypes.ConvertAll(j => (int)j);
                    return context.JobMonitors.Where(j => !jobtypes.Contains(j.JobType) && j.StartTime >= queryTickets && jobStatus.Contains(j.Status) && j.ExceptionType != MonitorExceptionType.None).ToList();
                }
                else
                {
                    return context.JobMonitors.Where(j => j.StartTime >= queryTickets && jobStatus.Contains(j.Status) && j.ExceptionType != MonitorExceptionType.None).ToList();
                }
            }
        }

        public async Task UpdateJobWithMonitorExceptionAsync(string jobId, MonitorExceptionType exceptionType)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                var job = GetJobWithOutI18N(jobId);
                job.ExceptionType |= exceptionType;
                await UpdateAsync(job);
            }
        }

        public List<string> GetJobIdsByScopeId(List<string> scopeIds)
        {
            var jobIdList = new List<string>();
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                foreach (var scopeId in scopeIds)
                {
                    jobIdList.Add(context.JobMonitors.Where(job => job.ScopeId == scopeId).Select(job => job.Id).FirstOrDefault());
                }
                return jobIdList;
            }
        }
        public string GetJobIdByAdditional(string additional)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.JobMonitors.Where(job => job.AdditionalInformation == additional).Select(job => job.Id).FirstOrDefault() ?? string.Empty;
            }
        }
        public bool CheckHasRunningManualJob()
        {
            List<JobType> manualJobs = new()
            {
                JobType.ManualApprovalTimer,
                JobType.ManualApprovalOrRejectJob,
                JobType.ManualFolderViewActions,
                JobType.ManualApprovalEmailSchedule,
                JobType.DisposalActivityManagement,
                JobType.FSDisposal
            };
            using var context = RMDBContextManager.GetNewDBContext();
            return context.JobMonitors.Any(job => manualJobs.Contains((JobType)job.JobType) && (job.Status == (int)JobStatus.InProgress || job.Status == (int)JobStatus.Wait));
        }

        public bool CheckCurrentUserHasRunningJob(string containerId, string jobId)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return context.JobMonitors.Any(job => (JobType)job.JobType == JobType.ManualImportUnderReviewDatasJob && (job.Status == (int)JobStatus.InProgress || job.Status == (int)JobStatus.Wait) && job.ContainerId == containerId && job.Id != jobId);
        }

        public async Task<int> ClearOldArchiverJobsAsync()
        {
            var sql = $@"DELETE FROM {GetFullTableName()} WHERE JobType={(int)JobType.DisposalActivityManagement}; ";
            using (var context = GetNewContext())
            {
                return await context.Database.ExecuteSqlCommandAsync(sql);
            }
        }

        public async Task BulkMigrateJobsAsync(IEnumerable<ArchiverMigrationJobDto> jobs)
        {
            if (jobs.Count() == 0)
            {
                return;
            }
            var lfAllArchiverJobs = GetAllExistArchiverJobId();
            logger.Debug("Total migrate jobs: {0}", jobs.Count());
            using (new PerformanceScope("Batch migrate jobs"))
            {
                var tableName = GetFullTableName();
                using (var table = ConvertToDataTable(jobs, lfAllArchiverJobs))
                {
                    table.TableName = tableName;
                    await BatchAddAsync(table, tableName);
                }
            }
        }

        public async Task<bool> CheckStoppedJobByDiscoveryJobId(Guid mainJobId)
        {
            using var context = GetNewContext();
            List<int> failedJob = [(int)JobStatus.Stopping, (int)JobStatus.Stopped, (int)JobStatus.Failed];
            return await context.JobMonitors.AnyAsync(job => failedJob.Contains(job.Status) && job.DiscoveryMainJobId == mainJobId);
        }

        private string GetFullTableName()
        {
            return $"[{SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}].[RMJobMonitors]";
        }

        private List<string> GetAllExistArchiverJobId()
        {
            using (var context = GetNewContext())
            {
                return context.ArchiverJobs.AsQueryable().Where(j => j.DAOMigrated == null || !j.DAOMigrated.Value).Select(j => j.Id).ToList();
            }
        }

        private DataTable ConvertToDataTable(IEnumerable<ArchiverMigrationJobDto> items, List<string> lfAllArchiverJobs)
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(String));
            table.Columns.Add("JobType", typeof(Int32));
            table.Columns.Add("StartTime", typeof(Int64));
            table.Columns.Add("EndTime", typeof(Int64));
            table.Columns.Add("Status", typeof(Int32));
            table.Columns.Add("Progress", typeof(Int32));
            table.Columns.Add("DoubleProgress", typeof(Double));
            table.Columns.Add("ScopeId", typeof(String));
            table.Columns.Add("UserName", typeof(String));
            table.Columns.Add("LastUpdateTime", typeof(Int64));
            table.Columns.Add("SubJobCount", typeof(Int32));
            table.Columns.Add("ContainerId", typeof(String));
            table.Columns.Add("ExceptionType", typeof(Int32));
            table.Columns.Add("NodeType", typeof(Int32));
            table.Columns.Add("DAOMigrated", typeof(Boolean));
            table.Columns.Add("Comment", typeof(String));
            table.Columns.Add("AdditionalInformation", typeof(String));
            table.Columns.Add("DiscoveryMainJobId", typeof(Guid));
            table.Columns.Add("DiscoveryJobId", typeof(Guid));
            table.Columns.Add("RestoreStatisticStatus", typeof(Int32));
            table.Columns.Add("Extension", typeof(String));
            table.Columns.Add("JobConflictExtension", typeof(String));
            table.Columns.Add("JobPriority", typeof(Int32));
            table.Columns.Add("JobVersion", typeof(Int32));

            Regex regex = new("^AR|^PAR|^EAR");
            foreach (var item in items)
            {
                var row = table.NewRow();
                var isDaJob = JobTypeConstants.MigrationDisposalJobTypes.Contains(item.JobType);
                if (lfAllArchiverJobs.Any(j => j == item.Id))
                {
                    logger.Info($"{item.Id} exist, skip insert RMJobMonitors");
                    continue;
                }
                row["Id"] = isDaJob ? regex.Replace(item.Id, "DA").Replace("S", "") : item.Id;
                row["JobType"] = isDaJob ? (int)JobType.MigrationDisposalActivityManagement : item.JobType;
                row["StartTime"] = item.StartTime;
                row["EndTime"] = item.EndTime;
                row["Status"] = item.Status;
                row["Progress"] = item.Progress;
                row["DoubleProgress"] = 0;
                row["ScopeId"] = item.ScopeId;
                row["UserName"] = item.UserName;
                row["LastUpdateTime"] = item.LastUpdateTime;
                row["SubJobCount"] = 0;
                row["ContainerId"] = Guid.Empty.ToString();
                row["ExceptionType"] = 0;
                row["NodeType"] = 0;
                row["DAOMigrated"] = true;
                row["Comment"] = item.Comment;
                row["AdditionalInformation"] = item.AdditionalInformation;
                row["DiscoveryMainJobId"] = Guid.Empty;
                row["DiscoveryJobId"] = Guid.Empty;
                row["RestoreStatisticStatus"] = 0;
                row["Extension"] = string.Empty;
                row["JobConflictExtension"] = string.Empty;
                row["JobPriority"] = 0;
                row["JobVersion"] = 0;
                table.Rows.Add(row);
            }

            return table;
        }

        public async Task<string> CreateDiscoveryJobWithGControlJobId(string id, string gControlJobId, string jobRunBy, Guid mainJobId, Guid discoveryJobId, JobType jobType)
        {
            using var context = GetNewContext();
            context.JobMonitors.Add(new RMJobMonitor
            {
                Id = id,
                JobType = (int)jobType,
                StartTime = DateTime.UtcNow.Ticks,
                Progress = 0,
                Status = (int)JobStatus.Wait,
                UserName = jobRunBy,
                LastUpdateTime = DateTime.UtcNow.Ticks,
                AdditionalInformation = gControlJobId,
                DiscoveryMainJobId = mainJobId,
                DiscoveryJobId = discoveryJobId,
            });
            await context.SaveChangesAsync();
            return id;
        }

        public async Task<bool> UpdateJobPriorityAsync(List<string> jobIds, JobPriority jobPriority)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var jobs = context.JobMonitors.Where(j => jobIds.Contains(j.Id)).ToList();
            foreach (var job in jobs)
            {
                job.JobPriority = jobPriority;
            }
            context.JobMonitors.AddOrUpdate([.. jobs]);
            return (await context.SaveChangesAsync()) > 0;
        }

        public List<RMJobMonitor> GetWatingAndRunningJobsWithPriorityAndSubJob()
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var jobs = context.JobMonitors.Where(j => (j.Status == (int)JobStatus.Wait || j.Status == (int)JobStatus.InProgress) && j.SubJobCount > 0)
                        .OrderByDescending(j => j.JobPriority)
                        .ThenByDescending(j => j.LastUpdateTime)
                        .ToList();
            return jobs;
        }

        public List<RMJobMonitor> GetJobsByJobIds(List<string> jobIds)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var jobs = context.JobMonitors.Where(j => jobIds.Contains(j.Id)).ToList();
            return jobs;
        }

        public RMJobMonitor GetLastestJobByLocation(string location)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var job = context.JobMonitors.Where(j => j.JobType == (int)JobType.ArchiverByHSMXml && j.ScopeId.Equals(location, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(j => j.LastUpdateTime)
                        .FirstOrDefault();
            return job;
        }
        public async Task<(List<RMJobMonitor> Items, int TotalCount)> GetJobReportsAsync(Expression<Func<RMJobMonitor, bool>> predicate, int pageIndex, int pageSize)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var query = context.JobMonitors.Where(predicate);
            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.StartTime)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> UpdateJobVersion(string id, JobVersion version)
        {
            string sql = "UPDATE {0}.RMJobMonitors SET JobVersion = @version WHERE Id = @id";
            using RMDbContext context = GetNewContext();
            int row = await context.Database.ExecuteSqlCommandAsync(string.Format(sql, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)),
                 new SqlParameter("version", version), new SqlParameter("id", id));
            return row > 0;
        }

        public async Task<bool> AnyJobAsync(Expression<Func<RMJobMonitor, bool>> predicate)
        {
            using var context = GetNewContext();
            return await context.JobMonitors.AnyAsync(predicate);
        }
    }


    public static class JMEnumSortByQueryExtensions
    {
        /// <summary>
        /// Linq Sort Extensions Method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="sortPropertyName"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> JMEnumSortBy<T>(this IQueryable<T> source, string sortPropertyName, SortDirectionEnum sortDirection, string sortString = null)
        {
            string OrderBy = "OrderBy";
            string OrderByDescending = "OrderByDescending";
            return JMEnumBaseSort(source, sortPropertyName, sortDirection, OrderBy, OrderByDescending, sortString);
        }

        /// <summary>
        /// Linq Then Sort Extensions Method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="sortPropertyName"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> JMEnumThenSortBy<T>(this IOrderedQueryable<T> source, string sortPropertyName, SortDirectionEnum sortDirection, string sortString = null)
        {
            string OrderBy = "ThenBy";
            string OrderByDescending = "ThenByDescending";
            var iQuery = JMEnumBaseSort(source, sortPropertyName, sortDirection, OrderBy, OrderByDescending, sortString);
            return iQuery;
        }
        public static IOrderedQueryable<T> JMEnumBaseSort<T>(IQueryable<T> source, string sortPropertyName, SortDirectionEnum sortDirection, string OrderBy, string OrderByDescending, string sortString = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (String.IsNullOrEmpty(sortPropertyName) || sortPropertyName.Trim().Length == 0)
            {
                return (IOrderedQueryable<T>)source;
            }

            ParameterExpression parameter = Expression.Parameter(source.ElementType, String.Empty);
            MemberExpression property = Expression.Property(parameter, sortPropertyName);
            LambdaExpression lambda = Expression.Lambda(property, parameter);

            var propertyType = property.Type;
            if (sortPropertyName == "JobType")
            {
                Expression<Func<RMJobMonitor, int?>> lambdaExp = s => SqlFunctions.CharIndex("|" + s.JobType.ToString() + "|", sortString);
                lambda = lambdaExp;
                propertyType = typeof(int?);
            }
            else if (sortPropertyName == "Status")
            {
                Expression<Func<RMJobMonitor, int?>> lambdaExp = s => SqlFunctions.CharIndex("|" + s.Status.ToString() + "|", sortString);
                lambda = lambdaExp;
                propertyType = typeof(int?);
            }
            else if (sortPropertyName == "UserName")
            {
                var runBy = "AvePoint Cloud Records System";    //此处是兼容老数据逻辑，文字是与DB中的值对应的，不会直接显示在页面上，所以不能改成Opus
                var i18NRunBySchedule = I18NEntity.GetString("RM_TS_RunSchedule");
                Expression<Func<RMJobMonitor, string>> lambdaExp = s => s.UserName == "RM_TS_RunSchedule" || s.UserName.ToLower() == runBy.ToLower() ? i18NRunBySchedule : s.UserName;
                lambda = lambdaExp;
                propertyType = typeof(string);
            }

            string methodName = (sortDirection == SortDirectionEnum.Ascending) ? OrderBy : OrderByDescending;

            Expression methodCallExpression = Expression.Call(typeof(Queryable), methodName,
                                                new Type[] { source.ElementType, propertyType },
                                                source.Expression, Expression.Quote(lambda));

            return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(methodCallExpression);
        }
    }
}

