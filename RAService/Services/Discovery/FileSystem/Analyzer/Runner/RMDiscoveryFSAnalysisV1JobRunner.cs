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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Work;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Work.Analyzer.V1.General;
using AvePoint.RA.Service.Services.Discovery.FileSystem.License;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Analyzer.Runner
{
    public class RMDiscoveryFSAnalysisV1JobRunner : RMDiscoveryFSWorker
    {
        private readonly IJobMonitorDao _jobMonitorDao = PlatformWindsorManager.GetService<IJobMonitorDao>();

        private readonly IRMReportManager _reportManager;

        private readonly string _jobId;

        private readonly IRMDiscoveryFSDataQueryDao _dataQueryDao = new RMDiscoveryFSDataQueryDao();

        public RMDiscoveryFSAnalysisV1JobRunner(string jobId) : base()
        {
            ReportMangerFactory.Instance.Init(jobId, JobType.DiscoveryAnalysisFileSystemV1);
            _reportManager = ReportMangerFactory.Instance.ReportManager;
            _jobId = jobId;
        }

        public async Task RunAsync()
        {
            try
            {
                var mainJobId = _jobMonitorDao.GetJob(_jobId).DiscoveryMainJobId;
                var (has, mainJob) = await _jobDao.TryGetMainJobAsync(mainJobId);
                var rules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.Inactive, RMDiscoveryRuleDefinitionKind.ROT);
                var analyzer = new RMDiscoveryFSAnalyzer(_reportManager, mainJob, rules);
                await analyzer.AnalysisAsync();
                await AddJobDetailsAsync(mainJobId);
                var jobStatus = await CalculateJobStatusAsync(mainJobId);
                _reportManager.SetJobFinished(jobStatus, jobStatus == JobStatus.Failed ? "RM_HS_Criteria_View_Msg_ValidOtherError" : "");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while run FS analysis job. Error: {e}");
                _reportManager.SetJobFinished(JobStatus.Failed, "RM_HS_Criteria_View_Msg_ValidOtherError");
            }
        }

        private async Task<JobStatus> CalculateJobStatusAsync(Guid mainJobId)
        {
            try
            {
                var (has, mainJob) = await _jobDao.TryGetMainJobAsync(mainJobId);
                var completingDiscoveryJobs = await _jobDao.GetDiscoveryJobsAsync(mainJob.Id, RMDiscoveryJobStatus.Completing);
                {
                    foreach (var completingDiscoveryJob in completingDiscoveryJobs)
                    {
                        if (await _jobDao.HasProcessingAnalysisJobAsync(completingDiscoveryJob.Id))
                        {
                            continue;
                        }
                        var analysisJobStatusDic = await _jobDao.GetAnalysisCompletedStatusAsync(completingDiscoveryJob.Id);
                        _ = analysisJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Finished, out var finishedCount);
                        _ = analysisJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Failed, out var failedCount);
                        _ = analysisJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Timeout, out var timeoutCount);
                        completingDiscoveryJob.EndTime = DateTime.UtcNow.Ticks;
                        completingDiscoveryJob.Status = RMDiscoveryJobStatus.Finished;
                        if (finishedCount > 0 && completingDiscoveryJob.ConnectionCount - finishedCount > 0)
                        {
                            completingDiscoveryJob.Status = RMDiscoveryJobStatus.Exception;
                        }
                        else if (failedCount + timeoutCount > 0)
                        {
                            completingDiscoveryJob.Status = RMDiscoveryJobStatus.Failed;
                        }
                        await _jobDao.AddOrUpdateDiscoveryJobAsync(completingDiscoveryJob);
                        _logger.Info($"The discovery job [{completingDiscoveryJob.Id}] in main job [{mainJob.Id}] [{completingDiscoveryJob.Status}].");
                    }
                }
                {
                    var discoveryJobStatusDic = await _jobDao.GetDiscoveryCompletedStatusAsync(mainJob.Id);
                    _ = discoveryJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Finished, out var finishedCount);
                    _ = discoveryJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Failed, out var failedCount);
                    _ = discoveryJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Exception, out var exceptionCount);
                    mainJob.EndTime = DateTime.UtcNow.Ticks;
                    mainJob.Status = RMDiscoveryJobStatus.Finished;

                    if (finishedCount > 0 && failedCount + exceptionCount > 0 || exceptionCount > 0)
                    {
                        mainJob.Status = RMDiscoveryJobStatus.Exception;
                    }
                    else if (failedCount + exceptionCount > 0)
                    {
                        mainJob.Status = RMDiscoveryJobStatus.Failed;
                    }
                    await _jobDao.AddOrUpdateMainJobAsync(mainJob);

                    if (mainJob.Type != RMDiscoveryJobType.Retry)
                    {
                        if (mainJob.Status == RMDiscoveryJobStatus.Finished || mainJob.Status == RMDiscoveryJobStatus.Exception)
                        {
                            var aggregateList = await _dataQueryDao.GetAggregateTotalDataListAsync();
                            var fileTotalSize = aggregateList.Sum(item => item.FileTotalSize);
                            await _executionInfoDao.UpdateFileSizeByMainJobAsync(mainJob.Id, fileTotalSize);
                            _logger.Info($"Updated FileTotalSize [{fileTotalSize}] for main job [{mainJob.Id}].");
                        }
                        else if (mainJob.Status == RMDiscoveryJobStatus.Failed)
                        {
                            await RMDiscoveryFSLicenseHelper.DecreaseConsumedFrequencyPerYearAsync();
                            await _executionInfoDao.DeleteByMainJobIdAsync(mainJob.Id);
                        }
                    }
                    _logger.Info($"Successful calculate job status [{mainJob.Status}].");
                    return mainJob.Status switch
                    {
                        RMDiscoveryJobStatus.Finished => JobStatus.Finished,
                        RMDiscoveryJobStatus.Exception => JobStatus.FinishWithException,
                        _ => JobStatus.Failed,
                    };
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while calculate job status. Error: {e}");
                throw;
            }
        }

        private async Task AddJobDetailsAsync(Guid mainJobId)
        {
            try
            {
                var enumerableJobs = _jobDao.GetAnalysisJobsWithPaginationAsync(mainJobId, 1000);
                await foreach (var analysisJob in enumerableJobs)
                {
                    _reportManager.SendJobDetail(new JMDiscoveryFileSystemJobDetails
                    {
                        ConnectionName = analysisJob.ConnectionName,
                        Status = analysisJob.Status == RMDiscoveryJobStatus.Failed ? JobDetailsStatus.Failed : JobDetailsStatus.Successful,
                        Comment = analysisJob.Status == RMDiscoveryJobStatus.Failed ? "RM_HS_Criteria_View_Msg_ValidOtherError" : "",
                    });
                }
                _logger.Info($"Successful add job details.");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while add job details. Error: {e}");
                throw;
            }
        }
    }
}
