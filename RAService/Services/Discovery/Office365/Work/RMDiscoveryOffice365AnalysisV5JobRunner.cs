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
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Services.Discovery.Cache;
using AvePoint.RA.Service.Services.Discovery.Office365.License;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Calculator.Duplicate;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Calculator;
using Cloud.Sdk.Data.IE;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V5.General;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Office365.Common;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Telemetry;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work
{
    public class RMDiscoveryOffice365AnalysisV5JobRunner : RMDiscoveryOffice365Worker
    {

        private readonly IJobMonitorDao _jobMonitorDao = PlatformWindsorManager.GetService<IJobMonitorDao>();

        private readonly IRMDiscoveryOffice365DataQueryDao _dataQueryDao = new RMDiscoveryOffice365DataQueryDao();

        private readonly IRMDiscoveryOffice365ProfileService _profileService = PlatformWindsorManager.GetService<IRMDiscoveryOffice365ProfileService>();

        private readonly IJobMonitorService _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();

        private readonly IRMReportManager _reportManager;

        private readonly string _jobId;
        
        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private int _lastReportedProgress;

        public RMDiscoveryOffice365AnalysisV5JobRunner(string jobId) : base()
        {
            ReportMangerFactory.Instance.Init(jobId, JobType.DiscoveryJobV5);
            _reportManager = ReportMangerFactory.Instance.ReportManager;
            _jobId = jobId;
        }

        public async Task RunAsync()
        {
            try
            {
                var mainJobId = _jobMonitorDao.GetJob(_jobId).DiscoveryMainJobId;
                var (has, mainJob) = await _jobDao.TryGetMainJobAsync(mainJobId);
                if (!has || mainJob == null)
                {
                    _logger.Error($"Discovery main job [{mainJobId}] was not found. Mark V5 analysis job [{_jobId}] as failed.");
                    _reportManager.SetJobFinished(AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Failed, "RM_Job_JobConflictOrNotExistData");
                    return;
                }

                _reportManager.IncreaseBase(mainJob.SitesCount);
                _reportManager.StartUpdateJobProgress();

                var rules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.Inactive, RMDiscoveryRuleDefinitionKind.ROT);
                var temeleter = new RMDiscoveryOffice365Temeleter(mainJobId);
                var analyzer = new RMDiscoveryOffice365Analyzer(_jobId, _reportManager, temeleter, rules);
                await analyzer.InitializeAsync(mainJob);

                while (true)
                {
                    (has, mainJob) = await _jobDao.TryGetMainJobAsync(mainJobId);
                    if (!has)
                    {
                        _logger.Error($"Discovery main job [{mainJobId}] was not found during V5 analysis. Mark V5 analysis job [{_jobId}] as failed.");
                        _reportManager.SetJobFinished(AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Failed, "RM_Job_JobConflictOrNotExistData");
                        return;
                    }

                    var processed = await analyzer.TryProcessNextPendingSiteAsync(mainJob);
                    await analyzer.FinalizeCompletedDiscoveryJobsAsync(mainJob);

                    var hasProcessingDiscoveryJob = await _jobDao.HasProcessingDiscoveryJobAsync(mainJobId);
                    var hasProcessingAnalysisJob = await _jobDao.CountAnalysisJobsByMainJobAsync(mainJobId, RMDiscoveryJobStatus.Preparing, RMDiscoveryJobStatus.Waiting, RMDiscoveryJobStatus.Pending, RMDiscoveryJobStatus.Running) > 0;
                    _logger.Info($"V5 analysis loop for main job [{mainJobId}]: processed=[{processed}], hasProcessingDiscoveryJob=[{hasProcessingDiscoveryJob}], hasProcessingAnalysisJob=[{hasProcessingAnalysisJob}].");
                    if (!hasProcessingDiscoveryJob && !hasProcessingAnalysisJob)
                    {
                        _logger.Info($"V5 analysis loop for main job [{mainJobId}] is finishing because no processing discovery or analysis jobs remain.");
                        await analyzer.FinalizeMainAsync(mainJob);
                        await analyzer.SyncDatabaseAsync();
                        break;
                    }

                    if (!processed)
                    {
                        _logger.Info($"V5 analysis loop for main job [{mainJobId}] is waiting because no pending site was processed in this iteration.");
                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }
                }

                var projectionCalculator = new RMDiscoveryOffice365ProjectionCalculator();
                await projectionCalculator.CalculateAsync();

                var duplicateCalculator = new RMDiscoveryOffice365DuplicateCalculatorV5(mainJob);
                await duplicateCalculator.CalculateAsync();

                await ClearRedisCache();

                await AddJobDetailsAsync(mainJobId);
                var jobStatus = await CalculateJobStatusAsync(mainJobId);

                _reportManager.SetJobFinished(jobStatus, jobStatus == Contract.RMWeb.JobMonitor.JobStatus.Failed ? "RM_HS_Criteria_View_Msg_ValidOtherError" : "");

                _profileService.SendProfileJob(JobRunBy.Schedule, new RMDiscoveryProfileJobDefinition
                {
                    RunMode = RMDiscoveryJobRunMode.All,
                    JobType = mainJob.Type,
                    MainJobId = mainJobId
                });

                await _keyValueDao.UpsertAsync(DiscoveryConstants.EXPORT_ROW_DATA_JOB, "False");
                await temeleter.RecordAsync();
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while run job. Error: {e}");
                _reportManager.SetJobFinished(AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Failed, "RM_HS_Criteria_View_Msg_ValidOtherError");
            }
        }

        private async Task RegisterIndexAsync(List<RMDiscoveryOffice365DiscoveryJob> discoveryJobs)
        {
            var tenantContentSources = discoveryJobs.GroupBy(item => item.O365TenantId).ToDictionary(item => item.Key, item => item.Select(i => i.ContentSource).ToHashSet());
            foreach (var tenantContentSource in tenantContentSources)
            {
                var o365TenantId = tenantContentSource.Key;
                foreach (var contentSource in tenantContentSource.Value)
                {
                    try
                    {

                        var rules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.ROT);
                        var indexModels = rules.ConvertAll(item => new IndexModel
                        {
                            Name = item.ToTagColumn(),
                            Definition = JsonConvert.SerializeObject(new Dictionary<string, int>
                            {
                                {item.ToTagColumn(), 1 }
                            }),
                        });

                        indexModels.Add(new IndexModel
                        {
                            Name = "Compound_FileSize",
                            Definition = JsonConvert.SerializeObject(new Dictionary<string, int>
                            {
                                {"FileSize", 1 },
                                { "_id", 1 }
                            })
                        });

                        if (indexModels.Count > 0)
                        {
                            await _ieApiClient.DatabaseManagementService.CreateIndexAsync(new IndexCreationModel
                            {
                                DataType = contentSource == SourceFlag.SharePoint ? DataType.SPDocument : DataType.SPOneDriveDocument,
                                Office365TenantId = o365TenantId.ToString(),
                                Indexes = indexModels
                            });
                        }

                        _logger.Info($"Successful register index for o365 tenant [{o365TenantId}] content source [{contentSource}].");
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"An error occurred while register index for o365 tenant [{o365TenantId}] content source [{contentSource}]. Error: {e}");
                    }
                }
            }
        }

        private async Task ClearRedisCache()
        {
            var o365Tenants = await _o365TenantDao.GetAllAsync();
            foreach (var o365Tenant in o365Tenants)
            {
                var cacheManager = new RMDiscoveryCacheManager(o365Tenant.UniqueId, RMDiscoveryCacheDataSource.Office365);
                await cacheManager.ClearAsync();
                _logger.Info($"The tenant [{o365Tenant.UniqueId}] cache cleared.");
            }
        }

        private async Task<AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus> CalculateJobStatusAsync(Guid mainJobId)
        {
            try
            {
                var (has, mainJob) = await _jobDao.TryGetMainJobAsync(mainJobId);
                var completingDiscoveryJobs = await _jobDao.GetDiscoveryJobsAsync(mainJob.Id, RMDiscoveryJobStatus.Completing);
                _logger.Info($"Start calculating V5 job status for main job [{mainJobId}]. Completing discovery jobs count [{completingDiscoveryJobs.Count}].");

                {
                    foreach (var completingDiscoveryJob in completingDiscoveryJobs)
                    {
                        if (await _jobDao.HasProcessingAnalysisJobAsync(completingDiscoveryJob.Id))
                        {
                            _logger.Info($"Skip completing discovery job [{completingDiscoveryJob.Id}] in main job [{mainJob.Id}] because processing analysis jobs still exist.");
                            continue;
                        }

                        var analysisJobStatusDic = await _jobDao.GetAnalysisCompletedStatusAsync(completingDiscoveryJob.Id);
                        _ = analysisJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Finished, out var finishedCount);
                        _ = analysisJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Failed, out var failedCount);
                        _ = analysisJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Timeout, out var timeoutCount);
                        _ = analysisJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Skipped, out var skippedCount);
                        completingDiscoveryJob.EndTime = DateTime.UtcNow.Ticks;
                        completingDiscoveryJob.Status = RMDiscoveryJobStatus.Finished;
                        if (finishedCount > 0 && completingDiscoveryJob.SiteCount - finishedCount - skippedCount > 0)
                        {
                            completingDiscoveryJob.Status = RMDiscoveryJobStatus.Exception;
                        }
                        else if (failedCount + timeoutCount > 0)
                        {
                            completingDiscoveryJob.Status = RMDiscoveryJobStatus.Failed;
                        }
                        await _jobDao.AddOrUpdateDiscoveryJobAsync(completingDiscoveryJob);
                        _logger.Info($"The discovery job [{completingDiscoveryJob.Id}] in main job [{mainJob.Id}] finalized as [{completingDiscoveryJob.Status}]. Finished:[{finishedCount}], Failed:[{failedCount}], Timeout:[{timeoutCount}], Skipped:[{skippedCount}].");
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
                            var o365Tenants = await _o365TenantDao.GetAllAsync();
                            foreach (var o365Tenant in o365Tenants)
                            {
                                var aggregateInfo = await _dataQueryDao.GetAggregateTotalDataListAsync(o365Tenant.UniqueId);
                                fileTotalSize += aggregateInfo.Sum(item => item.FileTotalSize);
                            }

                            await _executionInfoDao.UpdateFileSizeByMainJobAsync(mainJob.Id, fileTotalSize);
                        }
                        else if (mainJob.Status == RMDiscoveryJobStatus.Failed)
                        {
                            await _executionInfoDao.DeleteByMainJobIdAsync(mainJob.Id);
                            await RMDiscoveryOffice365LicenseHelper.DecreaseConsumedFrequencyPreMonthAsync();
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
                    var (status, comment) = analysisJob.Status switch
                    {
                        RMDiscoveryJobStatus.Skipped when (analysisJob.FailedCause == RMDiscoveryJobFailedCause.SiteNotFound)
                            => (JobDetailsStatus.Skipped, "RM_JD_SiteIsNotExist"),

                        RMDiscoveryJobStatus.Skipped when (analysisJob.FailedCause == RMDiscoveryJobFailedCause.SkippedExcludedSite)
                            => (JobDetailsStatus.Skipped, "RM_JD_SiteIsExcluded"),

                        RMDiscoveryJobStatus.Failed
                            => (JobDetailsStatus.Failed, "RM_HS_Criteria_View_Msg_ValidOtherError"),

                        RMDiscoveryJobStatus.Skipped
                            => (JobDetailsStatus.Skipped, string.Empty),

                        _ => (JobDetailsStatus.Successful, string.Empty)
                    };

                    _reportManager.SendJobDetail(new JMDiscoveryJobV2Details
                    {
                        Url = analysisJob.Url,
                        Status = status,
                        Comment = comment
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
