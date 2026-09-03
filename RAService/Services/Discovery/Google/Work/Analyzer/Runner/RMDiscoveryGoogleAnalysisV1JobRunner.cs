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
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Cache;
using AvePoint.RA.Service.Services.Discovery.Office365.License;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Calculator.Duplicate;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Calculator;
using AvePoint.RA.Service.Services.Discovery.Office365.Work;
using Cloud.Sdk.Data.IE;
using Cloud.Sdk.IE;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Service.Services.Discovery.Google.License;
using AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.General;

namespace AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.Runner
{
    public class RMDiscoveryGoogleAnalysisV1JobRunner : RMDiscoveryGoogleWorker
    {

        private const string S_ENABLE_EXPAND_QUERY_TEST = "ENABLE_DISCOVERY_EXPAND_QUERY_TEST";

        private readonly IJobMonitorDao _jobMonitorDao = PlatformWindsorManager.GetService<IJobMonitorDao>();

        private readonly IRMDiscoveryGoogleDataQueryDao _dataQueryDao = new RMDiscoveryGoogleDataQueryDao();

        private readonly IRMDiscoveryGoogleSizeRangeDao _sizeRangeDao = new RMDiscoveryGoogleSizeRangeDao();

        private readonly IRMDiscoveryGoogleWithoutInDateDao _dateRangeDao = new RMDiscoveryGoogleWithoutInDateDao();

        private readonly IRMReportManager _reportManager;
        
        private readonly IRMDiscoveryGoogleProfileService _profileService = PlatformWindsorManager.GetService<IRMDiscoveryGoogleProfileService>();

        private readonly IRMDiscoveryGoogleExecutionInfoDao _executionInfoDao = new RMDiscoveryGoogleExecutionInfoDao();

        private readonly string _jobId;

        public RMDiscoveryGoogleAnalysisV1JobRunner(string jobId) : base()
        {
            ReportMangerFactory.Instance.Init(jobId, JobType.DiscoveryGoogleJobV1);
            _reportManager = ReportMangerFactory.Instance.ReportManager;
            _jobId = jobId;
        }

        public async Task RunAsync()
        {
            try
            {
                var mainJobId = _jobMonitorDao.GetJob(_jobId).DiscoveryMainJobId;
                var (has, mainJob) = await _jobDao.TryGetMainJobAsync(mainJobId);
                var discoveryJobs = await _jobDao.GetDiscoveryJobsAsync(mainJobId, RMDiscoveryJobStatus.Completing);

                var sizeRangeIds = (await _sizeRangeDao.GetAllAsync()).Select(item => item.Id).ToList();
                var dateRangeIds = (await _dateRangeDao.GetAllAsync()).Select(item => item.Id).Concat(new List<int> { -1, 999 }).ToList();
                var rules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.Inactive, RMDiscoveryRuleDefinitionKind.ROT);

                var analyzer = new RMDiscoveryGoogleAnalyzer(
                        _reportManager,
                        mainJob,
                        sizeRangeIds,
                        dateRangeIds,
                        rules
                    );
                await analyzer.AnalysisAsync();

                await ClearRedisCache();

                await AddJobDetailsAsync(mainJobId);
                var jobStatus = await CalculateJobStatusAsync(mainJobId);

                _reportManager.SetJobFinished(jobStatus, jobStatus == Contract.RMWeb.JobMonitor.JobStatus.Failed ? "RM_HS_Criteria_View_Msg_ValidOtherError" : "");
                
                _profileService.SendProfileJob(JobRunBy.Schedule, new RMDiscoveryGoogleProfileJobDefinition 
                { 
                    RunMode = RMDiscoveryJobRunMode.All,
                    JobType = mainJob.Type,
                    MainJobId = mainJobId
                });

            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while run job. Error: {e}");
                _reportManager.SetJobFinished(AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Failed, "RM_HS_Criteria_View_Msg_ValidOtherError");
            }
        }

        private async Task ClearRedisCache()
        {
            var organizations = await _organizationDao.GetAllAsync();
            foreach (var organization in organizations)
            {
                var cacheManager = new RMDiscoveryCacheManager(organization.OrganizationId, RMDiscoveryCacheDataSource.Google);
                await cacheManager.ClearAsync();
                _logger.Info($"The tenant [{organization.OrganizationId}] cache cleared.");
            }
        }

        private async Task<AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus> CalculateJobStatusAsync(Guid mainJobId)
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
                        if (finishedCount > 0 && completingDiscoveryJob.DrivesCount - finishedCount > 0)
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

                    if ((finishedCount > 0 && failedCount + exceptionCount > 0) || exceptionCount > 0)
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
                            var fileTotalSize = 0L;
                            var organizations = await _organizationDao.GetAllAsync();
                            foreach (var organization in organizations)
                            {
                                var aggregateInfo = await _dataQueryDao.GetAggregateTotalDataListAsync(organization.OrganizationId);
                                fileTotalSize += aggregateInfo.Sum(item => item.FileTotalSize);
                            }
                            await _executionInfoDao.UpdateFileSizeByMainJobAsync(mainJob.Id, fileTotalSize);
                        }
                        else if (mainJob.Status == RMDiscoveryJobStatus.Failed)
                        {
                            await RMDiscoveryGoogleLicenseHelper.DecreaseConsumedFrequencyPerYearAsync();
                            await _executionInfoDao.DeleteByMainJobIdAsync(mainJob.Id);
                        }
                    }
                }

                _logger.Info($"Successful calculate job status [{mainJob.Status}].");
                return mainJob.Status switch
                {
                    RMDiscoveryJobStatus.Finished => AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Finished,
                    RMDiscoveryJobStatus.Exception => AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.FinishWithException,
                    _ => AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Failed,
                };
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
                    _reportManager.SendJobDetail(new JMDiscoveryGoogleJobDetails
                    {
                        DriveName = analysisJob.DriveName,
                        Status = analysisJob.Status == RMDiscoveryJobStatus.Failed ? JobDetailsStatus.Failed : JobDetailsStatus.Successful,
                        Comment = analysisJob.Status == RMDiscoveryJobStatus.Failed ? "RM_HS_Criteria_View_Msg_ValidOtherError" : "",
                    });
                    _logger.Info($"Discovery google job details, Name [{analysisJob.DriveName}], Status [{analysisJob.Status}].");
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
