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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Telemetry;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.DisposalStubDao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.RACommonUtility.Telemetry;
using AvePoint.RA.Service.Services.JobMonitor.Detail;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.JobManagement.Handler
{
    public class DefaultJobStateHandler : IJobStateHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(DefaultJobStateHandler));
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IJobMonitorDao JobMonitorDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
        private IRATelemetryService RATelemetryService => PlatformWindsorManager.GetService<IRATelemetryService>();
        private IJobDetailService JobDetailService => PlatformWindsorManager.GetService<IJobDetailService>();
        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMReportService ReportService => PlatformWindsorManager.GetService<IRMReportService>();
        private IRMEXOLabelDao RetentionLabelDao => PlatformWindsorManager.GetService<IRMEXOLabelDao>();
        private IRMRunningJobRuleMappingDao RMRunningJobRuleMappingDao => PlatformWindsorManager.GetService<IRMRunningJobRuleMappingDao>();
        IRMRetentionSimulateInfosDao RetentionSimulateInfosDao = PlatformWindsorManager.GetService<IRMRetentionSimulateInfosDao>();
        private IRMStubFileRecordDao StubFileRecordDao => PlatformWindsorManager.GetService<IRMStubFileRecordDao>();
        private IFileSystemSettingDao FileSystemSettingDao => PlatformWindsorManager.GetService<IFileSystemSettingDao>();

        IFSConnectionRelatedJobInfoDao FSConnectionRelatedJobInfoDao = PlatformWindsorManager.GetService<IFSConnectionRelatedJobInfoDao>();
        /// <summary>
        /// 进到这个方法的state是Finial的, 但是真正的状态是Calculating.  完成Merge等操作之后强更主job成Final状态.   强更结束后会进入HanldeFinalState方法
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="jobType"></param>
        /// <param name="state"></param>
        public void BeforeHandleState(string jobId, int jobType, int state)
        {
            try
            {
                if (jobType == (int)JobType.BCSTermUsageReport
                        || jobType == (int)JobType.EXOTermUsageReport
                        || jobType == (int)JobType.CreateAndDestroyedFileReport
                        || jobType == (int)JobType.EXOCreateAndDestroyedFileReport
                        || jobType == (int)JobType.OneDriveCreateAndDestroyedFileReport
                        || jobType == (int)JobType.RetiredTermReport
                        || jobType == (int)JobType.EXORetiredTermUsageReport
                        || jobType == (int)JobType.OrphanedTermReport
                        || jobType == (int)JobType.EXOOrphanedTermUsageReport
                        || jobType == (int)JobType.TeamsCreateAndDestroyedFileReport
                        || jobType == (int)JobType.TeamsBCSTermUsageReport
                        || jobType == (int)JobType.TeamsOrphanedTermUsageReport
                        || jobType == (int)JobType.TeamsRetiredTermUsageReport)
                {
                    //merge report

                    //TODO
                    JobMonitorDao.UpdateJob(jobId, (JobStatus)state, "");
                }
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
            }
        }


        public async System.Threading.Tasks.Task HanldeFinalStateAsync(string jobId, int state)
        {
            logger.Info("Handler job {0}, state {1} in state handler, start to merge report and clear cache.", jobId, state);

            DB.Model.RMJobMonitor mainJob = JobMonitorDao.GetJob(jobId, false);

            try
            {
                logger.Info("Try to handle enforce retention label status, {0}", jobId);

                await DealWithEnforceRetentionLabelStatusAsync(mainJob);

                logger.Info("Try to merge job report, {0}", jobId);
                await MergeJobReportAsync(mainJob);

                await MergeJobSummaryAsync(mainJob);

                await MergeAndUpoladArchiveMainJobStatistic(mainJob);

                RemoveRunningJobRuleMappings(mainJob);

                RemoveStubRecords();

                await ResetApplyClassCodeToExistingOption(mainJob, state);
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
            }
            if (state != (int)JobStatus.Failed)
            {
                ClearJobContext(jobId);
            }

            TelemetryContext.SendToQueue(TelemetryModule.JobMonitor, TelemetryEventType.RunJob, new List<object> { jobId });
        }

        private void RemoveStubRecords()
        {
            StubFileRecordDao.FlushDeleteCache(TenantLocalValue.LogonGroupId);
        }

        private async Task MergeAndUpoladArchiveMainJobStatistic(RMJobMonitor mainJob)
        {
            try
            {
                await RATelemetryService.MergeAndUpoladArchiveMainJobStatistic(mainJob.Id);
            }
            catch (Exception e)
            {
                logger.Error(@$"fail Merge And Upolad Main Job Statistic,ex:{e}");
            }
        }

        private void RemoveRunningJobRuleMappings(RMJobMonitor job)
        {
            try
            {
                List<int> jobTypes = new List<int>()
                {
                    (int)JobType.EXORecordsDisposal,
                    (int)JobType.OneDriveRecordsDisposal,
                    (int)JobType.RecordsDisposal,
                    (int)JobType.RMArchiverBackup,
                    (int)JobType.PhysicalRecordsDisposal,
                    (int)JobType.FSDisposal,
                    (int)JobType.FSDisposalSchedule,
                    (int)JobType.FSDisposalByClassCode,
                    (int)JobType.SPOnPremEnforceRuleAction,
                    (int)JobType.SPOnPremEnforceRuleActionSchedule,
                    (int)JobType.SOPreScan,
                    (int)JobType.DiscoverOptimization,
                    (int)JobType.DiscoveryPlanProOptimization,
                    (int)JobType.DiscoveryAOSPOptimization,
                    (int)JobType.DiscoveryPreScan,
                    (int)JobType.DiscoveryPlanProScan,
                    (int)JobType.BoxRecordsDisposal,
                    (int)JobType.ApprovalProcessArchive,
                    (int)JobType.GoogleRecordsDisposal,
                    (int)JobType.TeamsRecordsDisposal,
                    (int)JobType.TeamsArchiverBackup,
                    (int)JobType.TeamsPreScan,
                    (int)JobType.ArchiverByHSMXml
                };
                if (jobTypes.Contains(job.JobType))
                {
                    RMRunningJobRuleMappingDao.RemoveJobRuleMappings(TenantLocalValue.LogonGroupId, job.Id);
                }
            }
            catch (Exception e)
            {
                logger.Warn($"Error occurred while removing running job rule mappings. JobId:{job.Id} Error:{e.ToString()}"); ;
            }
        }

        private async System.Threading.Tasks.Task DealWithEnforceRetentionLabelStatusAsync(RMJobMonitor mainJob)
        {
            RMRetentionSourceType retentionSourceType = RMRetentionSourceType.None;

            if (CheckRetentionJob((JobType)mainJob.JobType, ref retentionSourceType))
            {
                try
                {
                    var retentionType = (int)retentionSourceType;
                    var tempLabel = RetentionLabelDao.GetLabel(retentionType, (int)Contract.TaxonomyModel.RMRetentionLabelStatus.JobProcessing);
                    if (tempLabel != null)
                    {
                        //有中间状态, 说明该Job有内容生效, 清空其他状态数据, 并将中间状态2更新为Last Job Used状态1
                        //Delete
                        var previousUsedLabels = RetentionLabelDao.GetLabelByStatusAndType((int)Contract.TaxonomyModel.RMRetentionLabelStatus.Previous, retentionType);
                        if (previousUsedLabels != null && previousUsedLabels.Count > 0 && previousUsedLabels.Where(l => l.LabelName.Equals(tempLabel.LabelName, StringComparison.OrdinalIgnoreCase)).ToList().Count > 0)
                        {
                            var previousLabel = previousUsedLabels.Where(l => l.LabelName.Equals(tempLabel.LabelName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                            previousLabel.SavedTime = DateTime.UtcNow.Ticks;
                            if (tempLabel.LabelId != Guid.Empty)
                            {
                                previousLabel.LabelId = tempLabel.LabelId;
                            }
                            await RetentionLabelDao.UpdateAsync(previousLabel);
                        }
                        else
                        {
                            RMEXOLabel newLabel = new RMEXOLabel()
                            {
                                LabelName = tempLabel.LabelName,
                                Status = (int)Contract.TaxonomyModel.RMRetentionLabelStatus.Previous,
                                LabelId = tempLabel.LabelId == Guid.Empty ? Guid.Empty : tempLabel.LabelId,
                                SavedTime = DateTime.UtcNow.Ticks,
                                Type = retentionType
                            };
                            RetentionLabelDao.Create(newLabel);
                        }

                        await RetentionLabelDao.BatchDeleteAsync(r => r.Status == 2 && r.Type == retentionType);
                    }
                    if (mainJob.Status == (int)JobStatus.Finished)
                    {
                        await RetentionLabelDao.BatchDeleteAsync(r => r.Status == 3 && r.Type == retentionType);
                    }

                }
                catch (Exception e)
                {
                    logger.Warn(e.Message, e);
                }
            }

        }
        private bool CheckRetentionJob(JobType jobType, ref Contract.TaxonomyModel.RMRetentionSourceType retentionType)
        {
            var retentionJob = false;
            switch (jobType)
            {
                case JobType.EXOEnforceRetention:
                    retentionType = Contract.TaxonomyModel.RMRetentionSourceType.Exchange;
                    retentionJob = true;
                    break;
                case JobType.EnforceRetention:
                    retentionType = Contract.TaxonomyModel.RMRetentionSourceType.SharePoint;
                    retentionJob = true;
                    break;
                case JobType.TeamsEnforceRetention:
                    retentionType = Contract.TaxonomyModel.RMRetentionSourceType.Teams;
                    retentionJob = true;
                    break;
                case JobType.OneDriveEnforceRetention:
                    retentionType = Contract.TaxonomyModel.RMRetentionSourceType.OneDrive;
                    retentionJob = true;
                    break;
                case JobType.OneDriveDataSynchronisation:
                    retentionType = Contract.TaxonomyModel.RMRetentionSourceType.OneDrive;
                    break;
                default:
                    break;
            }
            return retentionJob;
        }
        private async System.Threading.Tasks.Task MergeJobSummaryAsync(RMJobMonitor mainJob, int limitCount = 5)
        {
            try
            {
                string defaultComment = "RM_SS_CommonErrorMessage";
                GCommon.Utility.ArgumentCheck.NotNull(mainJob, nameof(mainJob));
                //var successSubJobCount = SubJobDao.Count(sj => sj.ParentId.StartsWith(mainJob.Id) && sj.Status == (int)JobStatus.Finished);
                var summary = SubJobDao.GetErrorJobSummary(mainJob.Id, limitCount).ToHashSet();
                int subjobCount = SubJobDao.Count(subJob => subJob.ParentId == mainJob.Id);
                if (!summary.IsNullOrEmpty())
                {
                    bool isContainerNode = false;

                    if (subjobCount > 1) isContainerNode = true;
                    else if (subjobCount == 1)
                    {
                        var subjob = SubJobDao.GetOneSubJobByParentIds([mainJob.Id]).FirstOrDefault()
                            ?? throw new Exception($"Subjob not found by mainjobId: {mainJob.Id}");
                        if (!string.Equals(subjob.String1, mainJob.ScopeId)) isContainerNode = true;
                    }

                    var firstSummary = summary.FirstOrDefault();
                    if (!string.IsNullOrEmpty(firstSummary) && (firstSummary == "RM_Retention_MoveToAnotherLocationDisabled" || firstSummary == "RM_JS_JM_Status_Stopped"))
                    {
                        isContainerNode = false;
                    }

                    if (isContainerNode)
                    {
                        mainJob.Comment = defaultComment;
                    }
                    else
                    {
                        mainJob.Comment = firstSummary;
                    }
                }
                else
                {
                    mainJob.Comment = string.Empty;
                }
                await JobMonitorDao.UpdateAsync(mainJob);
            }
            catch (Exception ex)
            {
                logger.Error($"error occurred while merge job summary: {ex.ToString()}");
            }

        }
        private void ClearJobContext(string jobId)
        {
            try
            {
                logger.Info("Clear job context of job id {0}", jobId);
                SubJobDao.DeleteJobContext(jobId);
                //Job结束删除Finish状态的子job
                SubJobDao.DeleteSubJob(jobId, (int)JobStatus.Finished);
                //Job结束删除Stoped状态的子job
                SubJobDao.DeleteSubJob(jobId, (int)JobStatus.Stopped);
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
            }
        }
        #region Merge rpt file

        //private void MergeJobReport(string jobId)
        //{
        //    try
        //    {
        //        logger.Info("Try to merge job report, {0}", jobId);
        //        DB.Model.RMJobMonitor mainJob = JobMonitorDao.GetJob(jobId);

        //        //最后一个更新子job关Final状态的进程, 会执行这个方法去Merge report文件
        //        MergeJobReport(mainJob);
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Warn(e.Message, e);
        //    }
        //}

        private async Task MergeJobReportForRetentionSimulateAsync(DB.Model.RMJobMonitor mainJob)
        {
            if (mainJob.JobType == (int)JobType.ArchiverRetentionSimulate || mainJob.JobType == (int)JobType.FSRetainSimulate)
            {
                try
                {
                    var allSimulateJobs = RetentionSimulateInfosDao.GetAll();
                    var mainSimulateJob = allSimulateJobs.First(r => r.SourceFlag == (int)SourceFlag.All);
                    var currentSimulateJob = allSimulateJobs.First(r => r.SourceFlag == GetSourceFlagByJobType(mainJob.JobType));
                    if (currentSimulateJob != null && mainSimulateJob != null)
                    {
                        var nextRunTimeTicks = currentSimulateJob.NextRunJobDate;
                        var retentionSumJob = new Contract.JobMonitor.BaseJobDto() { Id = $"{mainSimulateJob.RetentionJobId}", JobType = mainJob.JobType };

                        ArchiverRetentionSimulateSumDetailWorker worker = new ArchiverRetentionSimulateSumDetailWorker();
                        worker.MergeJobDetails(new Contract.JobMonitor.BaseJobDto() { Id = mainJob.Id, JobType = mainJob.JobType },
                            retentionSumJob);

                        var sum = worker.GetSummary(new Contract.JobMonitor.BaseJobDto() { Id = mainJob.Id, JobType = mainJob.JobType });

                        currentSimulateJob.FileNumber = sum.Item1;
                        currentSimulateJob.DataSize = sum.Item2;

                        worker.UploadReports(retentionSumJob);

                        currentSimulateJob.JobStatus = mainJob.Status;
                        currentSimulateJob.MergeReportState = (int)MergeIndexState.Succeed;
                        RetentionSimulateInfosDao.AddOrUpdateRetentionInfo(currentSimulateJob);

                        allSimulateJobs = RetentionSimulateInfosDao.GetAll();
                        mainSimulateJob = allSimulateJobs.First(r => r.SourceFlag == (int)SourceFlag.All);
                        var subSimulateJobs = allSimulateJobs.Where(r => string.Equals(r.MainRetentionJobId, mainSimulateJob.RetentionJobId));
                        if (subSimulateJobs.All(r => r.MergeReportState == (int)MergeIndexState.Succeed))
                        {
                            mainSimulateJob.JobStatus = (int)JobStatus.Finished;
                            mainSimulateJob.MergeReportState = (int)MergeIndexState.Succeed;
                        }
                        else if (mainSimulateJob.JobStatus == (int)JobStatus.Wait)
                        {
                            mainSimulateJob.JobStatus = (int)JobStatus.InProgress;
                        }
                        mainSimulateJob.FileNumber += currentSimulateJob.FileNumber;
                        mainSimulateJob.DataSize += currentSimulateJob.DataSize;
                        RetentionSimulateInfosDao.AddOrUpdateRetentionInfo(mainSimulateJob);
                    }
                }
                catch (Exception e)
                {
                    logger.Warn($"Failed to merge job report for rentetion simulate sum. ex:{e}");
                    try
                    {
                        var mainSimulateJob = RetentionSimulateInfosDao.GetAll().First(r => r.SourceFlag == (int)SourceFlag.All);
                        mainSimulateJob.JobStatus = (int)JobStatus.FinishWithException;
                        RetentionSimulateInfosDao.AddOrUpdateRetentionInfo(mainSimulateJob);
                    }
                    catch (Exception e2)
                    {
                        logger.Warn($"Failed to update main retention job status. ex:{e2}");
                    }
                }
            }

            int GetSourceFlagByJobType(int jobType)
            {
                switch (jobType)
                {
                    case (int)JobType.ArchiverRetentionSimulate:
                        return (int)SourceFlag.SharePoint;
                    case (int)JobType.FSRetainSimulate:
                        return (int)SourceFlag.FileSystem;
                    default:
                        return (int)SourceFlag.None;
                }
            }
        }

        private async Task MergeJobReportAsync(DB.Model.RMJobMonitor mainJob)
        {
            var successFlag = true;

            var isNewJobDetailsJob = JobServiceUtility.NewJobDetailsJobs.Contains(mainJob.JobType);
            var isSkipMergeJobDetails = JobServiceUtility.SkipMergeDetailsJobs.Contains(mainJob.JobType);
            mainJob.JobVersion = isSkipMergeJobDetails ? JobVersion.UnMerged : JobVersion.Merged;
            logger.Info($"Job {mainJob.Id} has sub job count {mainJob.SubJobCount}," +
                $" is skip merge job details: {isSkipMergeJobDetails}," +
                $" is new job details job: {isNewJobDetailsJob}," +
                $" job version: {mainJob.JobVersion}");
            if (mainJob.JobType == (int)JobType.EXOEnforceRetention || mainJob.JobType == (int)JobType.OneDriveEnforceRetention || mainJob.JobType == (int)JobType.TeamsEnforceRetention)
            {
                mainJob.JobType = (int)JobType.EnforceRetention;
            }
            var mainJobInfo = new Contract.JobMonitor.BaseJobDto() { Id = mainJob.Id, JobType = mainJob.JobType };
            var mainJobReportPath = JobReportUtility.GetJobReportPath(mainJobInfo, ".rpt");

            var stateList = new List<int> { (int)JobStatus.Finished, (int)JobStatus.FinishWithException, (int)JobStatus.Failed };
            if (isNewJobDetailsJob && mainJob.Status == (int)JobStatus.Stopping)
            {
                logger.Info($"Job {mainJob.Id} is a new job details job and is stopping, include stopped sub jobs for merge.");
                stateList.Add((int)JobStatus.Stopped);
            }
            var subJobList = await SubJobDao.GetAllSubJobByMainJobIdAsync(mainJob.Id, stateList.ToArray());
            if (subJobList.Count == 0)
            {
                //没有子job成功的情况下不需要 Merge
                logger.Info("No finished sub job in {0}, skip merge rpt action.");
                return;
            }
            logger.Info("Start to merge sub job report file.");
            int totalCount = 0;
            int pageSize = 400;  //每次处理200条, change to 400 for debug
            int processingSubJobNum = 0;
            JMSOSummaryDetails soSummaryDetails = new JMSOSummaryDetails();
            JMRestoreSummaryDetails restoreSummaryDetails = new JMRestoreSummaryDetails();

            foreach (var subJob in subJobList)
            {
                processingSubJobNum++;
                var subJobId = subJob.Id;
                try
                {
                    logger.Debug("merge sub job {0}", subJobId);
                    var subJobInfo = new Contract.JobMonitor.BaseJobDto()
                    {
                        Id = subJobId,
                        JobType = mainJob.JobType,
                        Status = subJob.Status,
                        ScopeId = subJob.String1,
                        Comment = subJob.Comment,

                        StartTime = subJob.StartTime,
                        EndTime = subJob.EndTime,
                    };
                    if (!isSkipMergeJobDetails)
                    {
                        if (processingSubJobNum == 1 && mainJob.JobType != (int)JobType.FSCreateAndDestroyedFileReport)
                        {
                            JobDetailService.DownloadReports(subJobInfo);

                            var firstJobReportPath = JobReportUtility.GetJobReportTempPath(
                                subJobInfo,
                                ".rpt");

                            var jobReportDir = Path.GetDirectoryName(mainJobReportPath);
                            if (!Directory.Exists(jobReportDir))
                            {
                                Directory.CreateDirectory(jobReportDir);
                            }

                            if (subJobList.Count > 1)
                            {
                                var tempMainJobReportPath = JobReportUtility.GetJobReportTempPath(
                                    mainJobInfo,
                                    ".rpt");
                                File.Copy(firstJobReportPath, tempMainJobReportPath, true);
                                JobDetailService.ClearSOSummaryDetails(mainJobInfo);
                                File.Move(tempMainJobReportPath, mainJobReportPath, true);
                            }
                            else
                            {
                                File.Copy(firstJobReportPath, mainJobReportPath, true);
                            }
                            logger.Info("Copy first sub job report as main job report file.");
                        }
                        else
                        {
                            logger.Info(@$"Start merge for {subJobId}");
                            JobDetailService.MergeJobDetails(new Contract.JobMonitor.BaseJobDto() { Id = subJobId, JobType = mainJob.JobType },
                                new Contract.JobMonitor.BaseJobDto() { Id = mainJob.Id, JobType = mainJob.JobType });
                            logger.Info(@$"End merge for {subJobId}");
                        }
                    }
                    if (isSkipMergeJobDetails && isNewJobDetailsJob)
                    {
                        // Create job details for each sub job without merge when the sub job count exceeds the threshold,
                        // to make sure the details data is available for troubleshooting,
                        // even though the main job report is not merged and might be missing some details.
                        logger.Info($"Start to write job details for sub job {subJobId} with skipped merge.");
                        JobDetailService.InsertMainJobDetails(subJobInfo, mainJobInfo);
                        logger.Info($"End to write job details for sub job {subJobId} with skipped merge.");
                    }

                    if (subJobList.Count > 1 || (subJobList.Count == 1 && isSkipMergeJobDetails))
                    {
                        if (mainJob.JobType == (int)JobType.SOPreScan
                        || mainJob.JobType == (int)JobType.DiscoveryPreScan
                        || mainJob.JobType == (int)JobType.DiscoveryPlanProScan
                        || mainJob.JobType == (int)JobType.RMArchiverBackup
                        || mainJob.JobType == (int)JobType.RMEndUserArchiverBackup
                        || mainJob.JobType == (int)JobType.SpecifySitesArchiverBackup
                        || mainJob.JobType == (int)JobType.SpecifyTeamsArchiverBackup
                        || mainJob.JobType == (int)JobType.RecordsDisposal
                        || mainJob.JobType == (int)JobType.OneDriveRecordsDisposal
                        || mainJob.JobType == (int)JobType.DiscoverOptimization
                        || mainJob.JobType == (int)JobType.DiscoveryPlanProOptimization
                        || mainJob.JobType == (int)JobType.DiscoveryAOSPOptimization
                        || mainJob.JobType == (int)JobType.BoxRecordsDisposal
                        || mainJob.JobType == (int)JobType.ApprovalProcessArchive
                        || mainJob.JobType == (int)JobType.ArchiverDeduplication
                        || mainJob.JobType == (int)JobType.TeamsArchiverBackup
                        || mainJob.JobType == (int)JobType.TeamsRecordsDisposal
                        || mainJob.JobType == (int)JobType.TeamsPreScan
                        || mainJob.JobType == (int)JobType.GoogleRecordsDisposal
                        || mainJob.JobType == (int)JobType.ArchiverByHSMXml
                        || mainJob.JobType == (int)JobType.EXORecordsDisposal
                        || mainJob.JobType == (int)JobType.CleanUpDuplicateDatas
                        )
                        {
                            JMJobDetails tempSummaryDetails = JobDetailService.GetDataForSOSummaryDetails(null, new Contract.JobMonitor.BaseJobDto() { Id = subJobId, JobType = mainJob.JobType });  //子job
                            if (tempSummaryDetails != null)
                            {
                                logger.Debug($"Merge SOSummaryDetails");
                                MergeSOSummayDetails(ref soSummaryDetails, tempSummaryDetails as JMSOSummaryDetails);
                            }
                        }
                        else if (mainJob.JobType == (int)JobType.TeamsArchiverRestore || mainJob.JobType == (int)JobType.MailBoxArchiverRestore || mainJob.JobType == (int)JobType.TeamsOutPlaceRestore)
                        {
                            JMJobDetails tempSummaryDetails = JobDetailService.GetDataForRestoreSummaryDetails(null, new Contract.JobMonitor.BaseJobDto() { Id = subJobId, JobType = mainJob.JobType });  //子job
                            if (tempSummaryDetails != null)
                            {
                                logger.Debug($"Merge RestoreSummaryDetails");
                                MergeRestoreSummayDetails(restoreSummaryDetails, tempSummaryDetails as JMRestoreSummaryDetails);
                            }
                        }
                    }
                    if ((subJobList.Count > 1 && processingSubJobNum > 1) || mainJob.JobType == (int)JobType.FSCreateAndDestroyedFileReport)
                    {
                        if (mainJob.JobType == (int)JobType.BCSTermUsageReport
                                || mainJob.JobType == (int)JobType.EXOTermUsageReport
                                || mainJob.JobType == (int)JobType.RetiredTermReport
                                || mainJob.JobType == (int)JobType.EXORetiredTermUsageReport
                                || mainJob.JobType == (int)JobType.OrphanedTermReport
                                || mainJob.JobType == (int)JobType.EXOOrphanedTermUsageReport
                                || mainJob.JobType == (int)JobType.CreateAndDestroyedFileReport
                                || mainJob.JobType == (int)JobType.EXOCreateAndDestroyedFileReport
                                || mainJob.JobType == (int)JobType.ItemsFilesDueDisposal
                                || mainJob.JobType == (int)JobType.EXOItemsFilesDueDisposalReport
                                || mainJob.JobType == (int)JobType.OneDriveItemsFilesDueDisposalReport
                                || mainJob.JobType == (int)JobType.OneDriveTermUsageReport
                                || mainJob.JobType == (int)JobType.OneDriveCreateAndDestroyedFileReport
                                || mainJob.JobType == (int)JobType.FSItemsFilesDueDisposal
                                || mainJob.JobType == (int)JobType.FSCreateAndDestroyedFileReport
                                || mainJob.JobType == (int)JobType.BoxItemsFilesDueDisposalReport
                                || mainJob.JobType == (int)JobType.BoxCreateAndDestroyedFileReport
                                || mainJob.JobType == (int)JobType.GoogleCreateAndDestroyedFileReport
                                || mainJob.JobType == (int)JobType.GoogleItemsFilesDueDisposalReport
                                || mainJob.JobType == (int)JobType.TeamsCreateAndDestroyedFileReport
                                || mainJob.JobType == (int)JobType.TeamsItemsFilesDueDisposalReport
                                || mainJob.JobType == (int)JobType.TeamsBCSTermUsageReport
                                || mainJob.JobType == (int)JobType.TeamsOrphanedTermUsageReport
                                || mainJob.JobType == (int)JobType.TeamsRetiredTermUsageReport)
                        {
                            totalCount = 0;
                            var startIndex = 1;
                            List<BaseReport> tempReports = (await ReportService.GetReportJobDatasAsync(pageSize, startIndex, string.Empty, new Contract.JobMonitor.BaseJobDto() { Id = subJobId, JobType = mainJob.JobType })).Item1.ToList();  //子job
                            while (tempReports != null && tempReports.Count > 0)
                            {
                                logger.Debug("Merge report job result, index {0}, count {1}, total {2}", startIndex, tempReports.Count, totalCount);
                                startIndex++;
                                ReportService.SyncReportJobDatas(tempReports, new Contract.JobMonitor.BaseJobDto() { Id = mainJob.Id, JobType = mainJob.JobType });  //主job
                                (var r, totalCount) = await ReportService.GetReportJobDatasAsync(pageSize, startIndex, string.Empty, new Contract.JobMonitor.BaseJobDto() { Id = subJobId, JobType = mainJob.JobType });
                                tempReports = r.ToList();  //子job
                            }
                            if ((mainJob.JobType == (int)JobType.BCSTermUsageReport
                                  || mainJob.JobType == (int)JobType.EXOTermUsageReport
                                  || mainJob.JobType == (int)JobType.RetiredTermReport
                                  || mainJob.JobType == (int)JobType.EXORetiredTermUsageReport
                                  || mainJob.JobType == (int)JobType.OrphanedTermReport
                                  || mainJob.JobType == (int)JobType.EXOOrphanedTermUsageReport
                                  || mainJob.JobType == (int)JobType.OneDriveTermUsageReport
                                  || mainJob.JobType == (int)JobType.OneDriveOrphanedTermUsageReport
                                  || mainJob.JobType == (int)JobType.OneDriveRetiredTermUsageReport
                                  || mainJob.JobType == (int)JobType.TeamsBCSTermUsageReport
                                  || mainJob.JobType == (int)JobType.TeamsOrphanedTermUsageReport
                                  || mainJob.JobType == (int)JobType.TeamsRetiredTermUsageReport) && processingSubJobNum == 1)
                            {
                                totalCount = 0;
                                startIndex = 1;
                                List<JMJobDetails> tempTerms = JobDetailService.GetDataForTermSelection(pageSize, startIndex, ref totalCount, null, new Contract.JobMonitor.BaseJobDto() { Id = subJobId, JobType = mainJob.JobType }).ToList();  //子job
                                while (tempTerms != null && tempTerms.Count > 0)
                                {
                                    logger.Debug("Merge term selection, index {0}, count {1}, total {2}", startIndex, tempTerms.Count, totalCount);
                                    startIndex++;
                                    JobDetailService.SyncJobDetails(tempTerms, new Contract.JobMonitor.BaseJobDto() { Id = mainJob.Id, JobType = mainJob.JobType });  //主job
                                    tempTerms = JobDetailService.GetDataForTermSelection(pageSize, startIndex, ref totalCount, null, new Contract.JobMonitor.BaseJobDto() { Id = subJobId, JobType = mainJob.JobType }).ToList(); //子job
                                }
                            }
                        }
                    }


                }
                catch (Exception e)
                {
                    logger.Error("merge sub job error, job id: {0}; exception: {1}", subJobId, e.ToString());
                    successFlag = false;
                }
                finally
                {
                    DeleteJobReportFileFromLocal(subJobId, mainJob.JobType);
                }
            }
            if (soSummaryDetails != null && soSummaryDetails.ActionStatistics != null && soSummaryDetails.ActionStatistics.Count > 0)
            {
                using var _ = new PerformanceScope("SyncJobSummary");
                List<JMJobDetails> jobDetails = new List<JMJobDetails>() { soSummaryDetails };
                JobDetailService.SyncJobDetails(jobDetails, new Contract.JobMonitor.BaseJobDto() { Id = mainJob.Id, JobType = mainJob.JobType });  //主job
            }
            else if (restoreSummaryDetails != null && restoreSummaryDetails.ActionStatistics != null && restoreSummaryDetails.ActionStatistics.Count > 0)
            {
                using var _ = new PerformanceScope("SyncResotreJobSummary");
                List<JMJobDetails> jobDetails = new List<JMJobDetails>() { restoreSummaryDetails };
                JobDetailService.SyncJobDetails(jobDetails, new Contract.JobMonitor.BaseJobDto() { Id = mainJob.Id, JobType = mainJob.JobType });  //主job
            }

            if (isSkipMergeJobDetails && isNewJobDetailsJob)
            {
                if (mainJob.Status == (int)JobStatus.Stopping || mainJob.Status == (int)JobStatus.Stopped)
                {
                    var updateResult = await JobDetailService.UpdateRemainingSubJobStatusAsync(mainJob.Id, [(int)JobStatus.Wait], (int)JobStatus.Stopped);
                    logger.Info($"Update remaining sub job status for main job {mainJob.Id} from Wait to Stopped, result: {updateResult}");
                }
                await JobDetailService.MigrateToRptAndDeleteAsync(mainJob.Id, mainJob.JobType);
            }

            var mainJobReport = new Contract.JobMonitor.BaseJobDto() { Id = mainJob.Id, JobType = mainJob.JobType };
            UploadReportForImportArchiveDataJob(mainJobReport, mainJob.Id);
            JobDetailService.UploadJobDetailsAndReport(mainJobReport);   //主job 
            //全部Merge完成之后 上传文件
            if (successFlag && !isSkipMergeJobDetails)
            {
                // 修改逻辑为，主job成功，删除子job，主job不成功，不删除subjob
                // 最后删除子job的rpt文件
                DeleteJobReportFiles(mainJob, subJobList.Select(s => s.Id).ToList());
            }

            await MergeJobReportForRetentionSimulateAsync(mainJob);
        }
        private void UploadReportForImportArchiveDataJob(Contract.JobMonitor.BaseJobDto dto, string mainJobId)
        {
            try
            {
                if (dto.JobType == (int)JobType.ArchiverByHSMXml)
                {
                    string traceId = GetImportArchiveDataTraceId(mainJobId);
                    var reportFilePath = JobReportUtility.GetJobReportPath(dto, ".rpt");
                    string reportBlobPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}", RMConstants.GetImportArchiveDataFolderName(traceId), dto.Id, Path.GetFileName(reportFilePath));
                    RAStorageUtil.UploadReportBlobToSpecifyStorage(WrapperConfiguration.SpecifyReportStorageXRIString, reportBlobPath, reportFilePath);
                    logger.Info($"Success upload job report to special storage");
                }
                else
                {
                    logger.Info($"this job type is not ArchiverByHSMXml,so no need to copy a report to special storage,job type:{dto.JobType}");
                }
            }
            catch (Exception e)
            {
                logger.Error($"try upload the job report to special storage failed,error:{e}");
            }
        }

        public void BuildRunningJobReport(string jobId)
        {
            logger.Info($"Start to BuildRunningJobReport file.JobId:{jobId}.");
            DB.Model.RMJobMonitor mainJob = JobMonitorDao.GetJob(jobId, false);
            JMSOSummaryDetails soSummaryDetails = new JMSOSummaryDetails();
            if (mainJob.JobType == (int)JobType.TeamsArchiverBackup || mainJob.JobType == (int)JobType.TeamsRecordsDisposal || mainJob.JobType == (int)JobType.TeamsPreScan)
            {
                var customId = TenantLocalValue.LogonGroupId;
                string blobPrefixName = string.Empty;
                switch (mainJob.JobType)
                {
                    case (int)JobType.TeamsArchiverBackup:
                        blobPrefixName = SecurityUtils.SafeCombinePath(customId, "TeamsArchiverBackup", mainJob.Id);
                        break;
                    case (int)JobType.TeamsRecordsDisposal:
                        blobPrefixName = SecurityUtils.SafeCombinePath(customId, "Teams Enforce Rule Action", mainJob.Id);
                        break;
                    case (int)JobType.TeamsPreScan:
                        blobPrefixName = SecurityUtils.SafeCombinePath(customId, "TeamsPreScan", mainJob.Id);
                        break;
                    default:
                        break;
                }

                blobPrefixName = blobPrefixName.Replace("\\", "/").TrimEnd('/');
                logger.Info($"blobPrefixName:{blobPrefixName}");
                var allJobReportNames = RAStorageUtil.GetAllReportBlobNames(blobPrefixName);
                if (allJobReportNames != null && allJobReportNames.Count > 0)
                {
                    logger.Info($"BuildRunningJobReport.allJobReportNames Count:{allJobReportNames.Count}");
                    foreach (string jobreportName in allJobReportNames)
                    {
                        string reportName = jobreportName.Substring(jobreportName.LastIndexOf("/") + 1);
                        string teamsSubJobId = reportName.Substring(0, reportName.LastIndexOf("."));
                        logger.Info($"BuildRunningJobReport.jobreportPath:{jobreportName}.jobreportName:{reportName}.teamsSubJobId:{teamsSubJobId}.");
                        RealRebuildSOJobReport(mainJob.Id, teamsSubJobId, mainJob.JobType, ref soSummaryDetails);
                    }
                }
                else
                {
                    logger.Info($"BuildRunningJobReport.allJobReportNames Count is 0.");
                }
            }
            else
            {
                int subjobTotalCount = mainJob.SubJobCount;
                List<string> subJobIdList = new List<string>();
                for (int i = 0; i < subjobTotalCount; i++)
                {
                    subJobIdList.Add(string.Format(jobId + "_{0:D3}", i));
                }
                foreach (string subJobId in subJobIdList)
                {
                    RealRebuildSOJobReport(mainJob.Id, subJobId, mainJob.JobType, ref soSummaryDetails);
                }
            }
            if (soSummaryDetails != null && soSummaryDetails.ActionStatistics != null && soSummaryDetails.ActionStatistics.Count > 0)
            {
                List<JMJobDetails> jobDetails = new List<JMJobDetails>() { soSummaryDetails };
                JobDetailService.SyncJobDetails(jobDetails, new Contract.JobMonitor.BaseJobDto() { Id = mainJob.Id, JobType = mainJob.JobType });  //主job
            }
            JobDetailService.UploadJobDetailsAndReportToTempLocation(new Contract.JobMonitor.BaseJobDto() { Id = mainJob.Id, JobType = mainJob.JobType });   //主job 
            //Rebuild SO Report Job，不删除子RPT，避免merge出问题导致后续无法操作
            logger.Info($"Finished to BuildRunningJobReport file.JobId:{jobId}.");
        }


        private string GetImportArchiveDataTraceId(string mainJobId)
        {
            try
            {
                var jobContextSetting = SubJobDao.GetJobContextSettingByMainJobId(mainJobId);
                if (string.IsNullOrWhiteSpace(jobContextSetting))
                {
                    return string.Empty;
                }

                var backupNode = SerializerHelper.DeserializeByDataContractSerializer<RMHSMBackupNode>(jobContextSetting);
                return backupNode?.TraceId ?? string.Empty;
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to resolve traceId for import archive data job. JobId:{mainJobId}. Error:{ex}");
                return string.Empty;
            }
        }

        public void RebuildSOJobReport(string soJobId)
        {
            logger.Info($"Start to RebuildSOJobReport file.JobId:{soJobId}.");
            DB.Model.RMJobMonitor mainJob = JobMonitorDao.GetJob(soJobId, false);
            JMSOSummaryDetails soSummaryDetails = new JMSOSummaryDetails();
            if (mainJob.JobType == (int)JobType.TeamsArchiverBackup || mainJob.JobType == (int)JobType.TeamsRecordsDisposal || mainJob.JobType == (int)JobType.TeamsPreScan)
            {
                var customId = TenantLocalValue.LogonGroupId;
                string blobPrefixName = string.Empty;
                switch (mainJob.JobType)
                {
                    case (int)JobType.TeamsArchiverBackup:
                        blobPrefixName = SecurityUtils.SafeCombinePath(customId, "TeamsArchiverBackup", mainJob.Id);
                        break;
                    case (int)JobType.TeamsRecordsDisposal:
                        blobPrefixName = SecurityUtils.SafeCombinePath(customId, "Teams Enforce Rule Action", mainJob.Id);
                        break;
                    case (int)JobType.TeamsPreScan:
                        blobPrefixName = SecurityUtils.SafeCombinePath(customId, "TeamsPreScan", mainJob.Id);
                        break;
                    default:
                        break;
                }

                blobPrefixName = blobPrefixName.Replace("\\", "/").TrimEnd('/');
                logger.Info($"blobPrefixName:{blobPrefixName}");
                var allJobReportNames = RAStorageUtil.GetAllReportBlobNames(blobPrefixName);
                if (allJobReportNames != null && allJobReportNames.Count > 0)
                {
                    logger.Info($"RebuildSOJobReport.allJobReportNames Count:{allJobReportNames.Count}");
                    foreach (string jobreportName in allJobReportNames)
                    {
                        string reportName = jobreportName.Substring(jobreportName.LastIndexOf("/") + 1);
                        string teamsSubJobId = reportName.Substring(0, reportName.LastIndexOf("."));
                        logger.Info($"RebuildSOJobReport.jobreportPath:{jobreportName}.jobreportName:{reportName}.teamsSubJobId:{teamsSubJobId}.");
                        RealRebuildSOJobReport(mainJob.Id, teamsSubJobId, mainJob.JobType, ref soSummaryDetails);
                    }
                }
                else
                {
                    logger.Info($"RebuildSOJobReport.allJobReportNames Count is 0.");
                }
            }
            else
            {
                int subjobTotalCount = mainJob.SubJobCount;
                List<string> subJobIdList = new List<string>();
                for (int i = 0; i < subjobTotalCount; i++)
                {
                    subJobIdList.Add(string.Format(soJobId + "_{0:D3}", i));
                }
                foreach (string subJobId in subJobIdList)
                {
                    RealRebuildSOJobReport(mainJob.Id, subJobId, mainJob.JobType, ref soSummaryDetails);
                }
            }
            if (soSummaryDetails != null && soSummaryDetails.ActionStatistics != null && soSummaryDetails.ActionStatistics.Count > 0)
            {
                List<JMJobDetails> jobDetails = new List<JMJobDetails>() { soSummaryDetails };
                JobDetailService.SyncJobDetails(jobDetails, new Contract.JobMonitor.BaseJobDto() { Id = mainJob.Id, JobType = mainJob.JobType });  //主job
            }
            JobDetailService.UploadJobDetailsAndReport(new Contract.JobMonitor.BaseJobDto() { Id = mainJob.Id, JobType = mainJob.JobType });   //主job 
            //Rebuild SO Report Job，不删除子RPT，避免merge出问题导致后续无法操作
            //DeleteJobReportFiles(mainJob, subJobIdList);
            logger.Info($"Finished to RebuildSOJobReport file.JobId:{soJobId}.");
        }

        public async Task RebuildEncryptKeyValue(string jobId)
        {
            logger.Info($"Start to RebuildEncryptKeyValue file.JobId:{jobId}.");
            await RMDBInitializer.ReInitRMEncryptKeyValue();
            logger.Info($"Finished to RebuildEncryptKeyValue file.JobId:{jobId}.");
        }

        public void RealRebuildSOJobReport(string mainJobId, string subJobId, int jobType, ref JMSOSummaryDetails soSummaryDetails)
        {
            try
            {
                int pageSize = 400;  //每次处理200条, change to 400 for debug
                logger.Debug($"Begin rebuild sub job report.SubJobId:{subJobId}.JobType:{jobType}.");
                int startIndex = 1;
                IEnumerable<JMJobDetails> tempDetails = null;
                do
                {
                    tempDetails = JobDetailService.GetData(pageSize, startIndex, null, new Contract.JobMonitor.BaseJobDto() { Id = subJobId, JobType = jobType, IsMergeRpt = true });
                    if (tempDetails != null && tempDetails.Count() > 0)
                    {
                        startIndex++;
                        JobDetailService.SyncJobDetails(tempDetails, new Contract.JobMonitor.BaseJobDto() { Id = mainJobId, JobType = jobType });
                        if (tempDetails.Count() < pageSize)
                        {
                            logger.Debug($"Finished rebuild sub job report.SubJobId:{subJobId}.");
                            break;
                        }
                    }
                    else
                    {
                        logger.Info($"Rebuild sub job report.Sub job has no detail.SubJobId:{subJobId}.");
                        break;
                    }
                }
                while (true);
                if (jobType == (int)JobType.SOPreScan
                    || jobType == (int)JobType.DiscoveryPreScan
                    || jobType == (int)JobType.DiscoveryPlanProScan
                    || jobType == (int)JobType.RMArchiverBackup
                    || jobType == (int)JobType.SpecifySitesArchiverBackup
                    || jobType == (int)JobType.RecordsDisposal
                    || jobType == (int)JobType.OneDriveRecordsDisposal
                    || jobType == (int)JobType.DiscoverOptimization
                    || jobType == (int)JobType.DiscoveryPlanProOptimization
                    || jobType == (int)JobType.DiscoveryAOSPOptimization
                    || jobType == (int)JobType.BoxRecordsDisposal
                    || jobType == (int)JobType.ApprovalProcessArchive
                    || jobType == (int)JobType.ArchiverDeduplication
                    || jobType == (int)JobType.TeamsArchiverBackup
                    || jobType == (int)JobType.SpecifyTeamsArchiverBackup
                    || jobType == (int)JobType.TeamsRecordsDisposal
                    || jobType == (int)JobType.TeamsPreScan
                    || jobType == (int)JobType.ArchiverByHSMXml
                    || jobType == (int)JobType.EXORecordsDisposal
                    )
                {
                    JMJobDetails tempSummaryDetails = JobDetailService.GetDataForSOSummaryDetails(null, new Contract.JobMonitor.BaseJobDto() { Id = subJobId, JobType = jobType });  //子job
                    if (tempSummaryDetails != null)
                    {
                        logger.Debug($"Merge SOSummaryDetails, {subJobId}");
                        MergeSOSummayDetails(ref soSummaryDetails, tempSummaryDetails as JMSOSummaryDetails);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("merge sub job error, job id: {0}; exception: {1}", subJobId, e.ToString());
            }
        }

        private void MergeSOSummayDetails(ref JMSOSummaryDetails mainJob, JMSOSummaryDetails subJob)
        {
            try
            {
                string mainSummaryStr = mainJob == null ? "null object" : SerializerHelper.SerializeByJsonConvert(mainJob);
                string subSummaryStr = subJob == null ? "null object" : SerializerHelper.SerializeByJsonConvert(subJob);
                logger.Info($"Prepare Merge sosummary, main summary :{mainSummaryStr}, sub summary :{subSummaryStr}");
            }
            catch (Exception e)
            {
                logger.Error($"Fail log SOSummary details value, main job is null:{mainJob == null}, sub job is null:{subJob == null},e:{e}");
            }

            if (subJob != null && subJob.ActionStatistics != null && subJob.ActionStatistics.Count > 0)
            {
                if (mainJob == null)
                {
                    mainJob = new JMSOSummaryDetails();
                }
                if (mainJob.ActionStatistics == null)
                {
                    mainJob.ActionStatistics = new List<ActionStatistics>();
                }
                foreach (var subSta in subJob.ActionStatistics)
                {
                    var mainSta = mainJob.ActionStatistics.Find(s => s.ActionTab == subSta.ActionTab);
                    if (mainSta == null)
                    {
                        mainSta = subSta.Clone();
                        mainJob.ActionStatistics.Add(mainSta);
                    }
                    else
                    {
                        AddSubDetailsToMain(mainSta, subSta);
                    }
                }
            }
            else
            {
                logger.Info($"Not merge job summary,subJob is null:{subJob == null}, subjob ActionStatistics is null;{subJob?.ActionStatistics == null}, subjob ActionStatistics count:{subJob?.ActionStatistics?.Count()}");
            }
        }

        private void MergeRestoreSummayDetails(JMRestoreSummaryDetails mainJob, JMRestoreSummaryDetails subJob)
        {
            if (subJob != null && subJob.ActionStatistics != null && subJob.ActionStatistics.Count > 0)
            {
                if (mainJob == null)
                {
                    mainJob = new JMRestoreSummaryDetails();
                }
                if (mainJob.ActionStatistics == null)
                {
                    mainJob.ActionStatistics = new List<ActionStatistics>();
                }
                foreach (var subSta in subJob.ActionStatistics)
                {
                    var mainSta = mainJob.ActionStatistics.Find(s => s.ActionTab == subSta.ActionTab);
                    if (mainSta == null)
                    {
                        mainSta = subSta.Clone();
                        mainJob.ActionStatistics.Add(mainSta);
                    }
                    else
                    {
                        AddSubDetailsToMain(mainSta, subSta);
                    }
                }
            }
        }

        private void AddSubDetailsToMain(ActionStatistics mainStatistics, ActionStatistics subStatistics)
        {
            if (subStatistics != null)
            {
                if (mainStatistics == null)
                {
                    mainStatistics = new ActionStatistics();
                }
                mainStatistics.ActionTab = subStatistics.ActionTab;
                mainStatistics.Size += subStatistics.Size;
                mainStatistics.DeleteSize += subStatistics.DeleteSize;
                if (mainStatistics.SuccessfulObj == null)
                {
                    mainStatistics.SuccessfulObj = new ObjectStatistic();
                }
                mainStatistics.SuccessfulObj.SiteCollectionCount += subStatistics.SuccessfulObj.SiteCollectionCount;
                mainStatistics.SuccessfulObj.SiteCount += subStatistics.SuccessfulObj.SiteCount;
                mainStatistics.SuccessfulObj.ListCount += subStatistics.SuccessfulObj.ListCount;
                mainStatistics.SuccessfulObj.FolderCount += subStatistics.SuccessfulObj.FolderCount;
                mainStatistics.SuccessfulObj.ItemCount += subStatistics.SuccessfulObj.ItemCount;
                mainStatistics.SuccessfulObj.ConnectionCount += subStatistics.SuccessfulObj.ConnectionCount;
                mainStatistics.SuccessfulObj.UserCount += subStatistics.SuccessfulObj.UserCount;
                mainStatistics.SuccessfulObj.FileCount += subStatistics.SuccessfulObj.FileCount;
                #region Teams data count
                mainStatistics.SuccessfulObj.TeamsGroupCount += subStatistics.SuccessfulObj.TeamsGroupCount;
                mainStatistics.SuccessfulObj.ChannelCount += subStatistics.SuccessfulObj.ChannelCount;
                mainStatistics.SuccessfulObj.ChannelConversationCount += subStatistics.SuccessfulObj.ChannelConversationCount;
                mainStatistics.SuccessfulObj.GroupMailboxCount += subStatistics.SuccessfulObj.GroupMailboxCount;
                mainStatistics.SuccessfulObj.GroupMailboxItemCount += subStatistics.SuccessfulObj.GroupMailboxItemCount;
                mainStatistics.SuccessfulObj.GroupMailboxFolderCount += subStatistics.SuccessfulObj.GroupMailboxFolderCount;
                mainStatistics.SuccessfulObj.PlanCount += subStatistics.SuccessfulObj.PlanCount;
                mainStatistics.SuccessfulObj.TaskCount += subStatistics.SuccessfulObj.TaskCount;
                mainStatistics.SuccessfulObj.AttachmentCount += subStatistics.SuccessfulObj.AttachmentCount;
                #endregion
                #region Google
                mainStatistics.SuccessfulObj.DriveCount += subStatistics.SuccessfulObj.DriveCount;
                #endregion

                if (mainStatistics.FailedObj == null)
                {
                    mainStatistics.FailedObj = new ObjectStatistic();
                }
                mainStatistics.FailedObj.SiteCollectionCount += subStatistics.FailedObj.SiteCollectionCount;
                mainStatistics.FailedObj.SiteCount += subStatistics.FailedObj.SiteCount;
                mainStatistics.FailedObj.ListCount += subStatistics.FailedObj.ListCount;
                mainStatistics.FailedObj.FolderCount += subStatistics.FailedObj.FolderCount;
                mainStatistics.FailedObj.ItemCount += subStatistics.FailedObj.ItemCount;
                mainStatistics.FailedObj.ExceptionCount += subStatistics.FailedObj.ExceptionCount;
                mainStatistics.FailedObj.ConnectionCount += subStatistics.FailedObj.ConnectionCount;
                mainStatistics.FailedObj.UserCount += subStatistics.FailedObj.UserCount;
                mainStatistics.FailedObj.FileCount += subStatistics.FailedObj.FileCount;
                #region Teams data count
                mainStatistics.FailedObj.TeamsGroupCount += subStatistics.FailedObj.TeamsGroupCount;
                mainStatistics.FailedObj.ChannelCount += subStatistics.FailedObj.ChannelCount;
                mainStatistics.FailedObj.ChannelConversationCount += subStatistics.FailedObj.ChannelConversationCount;
                mainStatistics.FailedObj.GroupMailboxCount += subStatistics.FailedObj.GroupMailboxCount;
                mainStatistics.FailedObj.GroupMailboxItemCount += subStatistics.FailedObj.GroupMailboxItemCount;
                mainStatistics.FailedObj.GroupMailboxFolderCount += subStatistics.FailedObj.GroupMailboxFolderCount;
                mainStatistics.FailedObj.PlanCount += subStatistics.FailedObj.PlanCount;
                mainStatistics.FailedObj.TaskCount += subStatistics.FailedObj.TaskCount;
                mainStatistics.FailedObj.AttachmentCount += subStatistics.FailedObj.AttachmentCount;
                #endregion
                #region Google
                mainStatistics.FailedObj.DriveCount += subStatistics.FailedObj.DriveCount;
                #endregion

                if (mainStatistics.SkippedObj == null)
                {
                    mainStatistics.SkippedObj = new ObjectStatistic();
                }
                mainStatistics.SkippedObj.SiteCollectionCount += subStatistics.SkippedObj.SiteCollectionCount;
                mainStatistics.SkippedObj.SiteCount += subStatistics.SkippedObj.SiteCount;
                mainStatistics.SkippedObj.ListCount += subStatistics.SkippedObj.ListCount;
                mainStatistics.SkippedObj.FolderCount += subStatistics.SkippedObj.FolderCount;
                mainStatistics.SkippedObj.ItemCount += subStatistics.SkippedObj.ItemCount;
                mainStatistics.SkippedObj.ConnectionCount += subStatistics.SkippedObj.ConnectionCount;
                mainStatistics.SkippedObj.UserCount += subStatistics.SkippedObj.UserCount;
                mainStatistics.SkippedObj.FileCount += subStatistics.SkippedObj.FileCount;
                #region Teams data count
                mainStatistics.SkippedObj.TeamsGroupCount += subStatistics.SkippedObj.TeamsGroupCount;
                mainStatistics.SkippedObj.ChannelCount += subStatistics.SkippedObj.ChannelCount;
                mainStatistics.SkippedObj.ChannelConversationCount += subStatistics.SkippedObj.ChannelConversationCount;
                mainStatistics.SkippedObj.GroupMailboxCount += subStatistics.SkippedObj.GroupMailboxCount;
                mainStatistics.SkippedObj.GroupMailboxItemCount += subStatistics.SkippedObj.GroupMailboxItemCount;
                mainStatistics.SkippedObj.GroupMailboxFolderCount += subStatistics.SkippedObj.GroupMailboxFolderCount;
                mainStatistics.SkippedObj.PlanCount += subStatistics.SkippedObj.PlanCount;
                mainStatistics.SkippedObj.TaskCount += subStatistics.SkippedObj.TaskCount;
                mainStatistics.SkippedObj.AttachmentCount += subStatistics.SkippedObj.AttachmentCount;
                #endregion
                #region Google
                mainStatistics.SkippedObj.DriveCount += subStatistics.SkippedObj.DriveCount;
                #endregion

            }
        }

        private bool DeleteJobReportFileFromLocal(string jobId, int jobType, string expandedName = ".rpt")
        {
            string rptPath = null;
            try
            {
                rptPath = JobReportUtility.GetJobReportTempPath(
                    new Contract.JobMonitor.BaseJobDto() { Id = jobId, JobType = jobType },
                    expandedName);
                File.Delete(rptPath);
                return true;
            }
            catch (Exception e)
            {
                logger.Warn($"delete local report file {rptPath} error, message: {e}");
            }
            return false;
        }

        private int DeleteJobReportFiles(RMJobMonitor mainJob, List<string> subJobIdList)
        {
            logger.Debug("Delete temp job rpt file in azure blob storage");
            string expandedName = ".rpt";
            var successCount = 0;

            foreach (var subJobId in subJobIdList)
            {
                try
                {
                    var uri = JobReportUtility.GetJobReportUri(subJobId, mainJob.JobType, expandedName);
                    logger.Debug($"delete file uri is: {uri}");
                    RAStorageUtil.DeleteReportBlob(uri);
                    successCount++;
                }
                catch (Exception e)
                {
                    logger.Warn($"delete blob file error, message: {e}");
                }
            }
            return successCount;
        }

        #endregion

        #region JPMC
        private async Task ResetApplyClassCodeToExistingOption(RMJobMonitor mainJob, int mainjobState)
        {
            try
            {
                if (!FSHighPerformanceUtility.IsEnabledJPMCFileSystemFeature()) return;
                if (mainJob.JobType == (int)JobType.ApplyClassCode
                    && (mainjobState == (int)JobStatus.Finished || mainjobState == (int)JobStatus.FinishWithException))
                {
                    var groupId = Guid.Parse(mainJob.ScopeId);
                    var groupSetting = FileSystemSettingDao.LoadFSSetting(groupId, groupId);
                    if (groupSetting != null && groupSetting.ApplyExistDocument)
                    {
                        groupSetting.ApplyExistDocument = false;
                        await FileSystemSettingDao.AddOrUpdateFSSettingAsync(groupSetting);
                        logger.Info($"Reset ApplyExistDocument to false for group {groupId} after apply class code job finished. JobId: {mainJob.Id}");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"ResetApplyClassCodeToExistingOption failed for job {mainJob.Id}, exception: {e}");
            }
        }
        #endregion

    }
}
