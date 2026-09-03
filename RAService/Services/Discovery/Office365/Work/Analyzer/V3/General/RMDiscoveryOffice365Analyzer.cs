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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Core.Discovery.DBManager.SQLite;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V3.General.Inactive;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V3.General.Rot;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Telemetry;
using RACloudFS.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V3.General
{
    public class RMDiscoveryOffice365Analyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365Analyzer));

        private readonly IRMDiscoveryOffice365JobDao _jobDao;

        private readonly IRMDiscoveryOffice365NodeDao _nodeDao;

        private readonly RMDiscoveryOffice365Temeleter _temeleter;

        private readonly IRMReportManager _reportManager;

        private readonly RMDiscoveryOffice365MainJob _mainJob;

        private readonly List<int> _sizeRangeIds;

        private readonly List<int> _dateRangeIds;

        private readonly List<RMDiscoveryOffice365RuleInfo> _rotRules;

        private readonly List<RMDiscoveryOffice365RuleInfo> _inactiveRules;

        private readonly bool _enableExpandQueryTest;

        public RMDiscoveryOffice365Analyzer(
                RMDiscoveryOffice365Temeleter temeleter,
                IRMReportManager reportManager,
                RMDiscoveryOffice365MainJob mainJob,
                List<int> sizeRangeIds,
                List<int> dateRangeIds,
                List<RMDiscoveryOffice365RuleInfo> rules,
                bool enableExpandQueryTest
            )
        {
            _jobDao = new RMDiscoveryOffice365JobDao();
            _nodeDao = new RMDiscoveryOffice365NodeDao();
            _temeleter = temeleter;
            _reportManager = reportManager;
            _mainJob = mainJob;
            _sizeRangeIds = sizeRangeIds;
            _dateRangeIds = dateRangeIds;
            _rotRules = rules.Where(item => item.DefinitionKind == RMDiscoveryRuleDefinitionKind.ROT).ToList();
            _inactiveRules = rules.Where(item => item.DefinitionKind == RMDiscoveryRuleDefinitionKind.Inactive).ToList();
            _enableExpandQueryTest = enableExpandQueryTest;
        }

        public async Task AnalysisAsync()
        {
            try
            {
                _reportManager.IncreaseBase(_mainJob.SitesCount);
                _reportManager.StartUpdateJobProgress();

                var jobType = _mainJob.Type;

                _logger.Info($"Start analysis [{_mainJob.Id}] [{jobType}] main job.");

                if (jobType == RMDiscoveryJobType.Newly)
                {
                    RMDiscoveryOffice365SQLiteDBManager.CreateDatabase();
                    _logger.Info($"Successful create sqlite database.");
                }
                else
                {
                    await RMDiscoveryOffice365SQLiteDBManager.DownloadDatabaseAsync();
                    _logger.Info($"Successful download sqlite database from storage.");
                }

                var discoveryJobs = await _jobDao.GetDiscoveryJobsAsync(_mainJob.Id, RMDiscoveryJobStatus.Completing);
                var tenantGroupedDiscoveryJobs = discoveryJobs.GroupBy(item => item.O365TenantId).ToDictionary(item => item.Key, item => item.ToList());
                foreach (var tenantDiscoveryJobs in tenantGroupedDiscoveryJobs)
                {
                    var o365TenantId = tenantDiscoveryJobs.Key;

                    await RMDiscoveryOffice365SQLiteDBManager.InitInactiveTablesAsync(o365TenantId, _inactiveRules.ConvertAll(item => item.ToCustomColumn()));
                    await RMDiscoveryOffice365SQLiteDBManager.InitRotTablesAsync(o365TenantId);
                    _logger.Info($"Successful create o365 tenant [{o365TenantId}] inactive & rot table.");

                    var fileExtensionAnalyzer = new RMDiscoveryOffice365FileExtensionAnalysisManager(o365TenantId);
                    await fileExtensionAnalyzer.InitAsync();

                    var contentSourceGroupedDiscoveryJobs = tenantDiscoveryJobs.Value.GroupBy(item => item.ContentSource).ToDictionary(item => item.Key, item => item.ToList());
                    foreach (var contentSourceDiscoveryJobs in contentSourceGroupedDiscoveryJobs)
                    {
                        var contentSource = contentSourceDiscoveryJobs.Key;

                        _logger.Info($"Start analysis tenant [{o365TenantId}] [{contentSource}] discovery jobs.");

                        var aggregateTotalDataAnalyer = new RMDiscoveryOffice365AggregateTotalDataAnalyzer(o365TenantId, _mainJob.Type, contentSource, _sizeRangeIds, _dateRangeIds, _enableExpandQueryTest);
                        var basicInactiveDataAnalyzer = new RMDiscoveryOffice365BasicInactiveDataAnalyzer(jobType, o365TenantId, contentSource, _inactiveRules);
                        var basicRotDataAnalyzer = new RMDiscoveryOffice365BasicRotDataAnalyzer(jobType, o365TenantId, contentSource);

                        var containerLevelCompletedStatus = new HashSet<bool>();

                        foreach (var discoveryJob in contentSourceDiscoveryJobs.Value)
                        {
                            _logger.Info($"Start analysis [{discoveryJob.Id}] [{discoveryJob.ContainerName}] discovery job.");

                            var containerDataAnalyzer = new RMDiscoveryOffice365ContainerDataAnalyzer(_mainJob.Type, discoveryJob);
                            var (initSucceed, containerInfo) = await containerDataAnalyzer.InitAsync();
                            if (!initSucceed)
                            {
                                await _jobDao.ChangeAnalysisJobsStatusAsync(RMDiscoveryJobStatus.Failed, RMDiscoveryJobFailedCause.AnalysisFailed, discoveryJob.Id);
                                continue;
                            }

                            var containerInactiveDataAnalyzer = new RMDiscoveryOffice365ContainerInactiveDataAnalyzer(jobType, o365TenantId, containerInfo.Id, _inactiveRules);
                            var containerRotDataAnalyzer = new RMDiscoveryOffice365ContainerRotDataAnalyzer(jobType, o365TenantId, containerInfo.Id);

                            var enumerableAnalysisJobs = _jobDao.GetAnalysisJobsByDiscoveryJobWithPaginationAsync(discoveryJob.Id, 1000, Contract.Discovery.Job.RMDiscoveryJobStatus.Pending);
                            var siteLevelCompletedStatus = new HashSet<bool>();

                            aggregateTotalDataAnalyer.Memeory();

                            await foreach (var analysisJob in enumerableAnalysisJobs)
                            {
                                if (analysisJob.FailedCause == RMDiscoveryJobFailedCause.SiteNotFound)
                                {

                                    _logger.Warn($"The site [{analysisJob.SiteId}] discovery job failed cause deleted, skip analysis.");

                                    await CleanupDeletedSiteDataAsync(o365TenantId, analysisJob.SiteId);

                                    await ChangeDiscoveryFailedAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Skipped);
                                    continue;
                                }
                                if (analysisJob.FailedCause == RMDiscoveryJobFailedCause.DiscoveryFailed)
                                {
                                    _logger.Warn($"The site [{analysisJob.SiteId}] discovery job failed, skip analysis.");
                                    await ChangeDiscoveryFailedAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Failed);
                                    continue;
                                }
                                if (_mainJob.Type == RMDiscoveryJobType.Append)
                                {
                                    var existsSitesInfo = await _nodeDao.GetDiscoverySiteInfoAsync(o365TenantId, analysisJob.SiteId);
                                    if (existsSitesInfo != null)
                                    {
                                        _logger.Warn($"The site [{analysisJob.SiteId}] has been analyzed before.");
                                        await ChangeAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Finished);
                                        continue;
                                    }
                                }

                                var res = await AnalysisAsync(
                                    fileExtensionAnalyzer,
                                    aggregateTotalDataAnalyer,
                                    containerDataAnalyzer,
                                    basicInactiveDataAnalyzer,
                                    basicRotDataAnalyzer,
                                    containerInactiveDataAnalyzer,
                                    containerRotDataAnalyzer,
                                    _mainJob,
                                    discoveryJob,
                                    analysisJob
                                             );
                                siteLevelCompletedStatus.Add(res);
                            }

                            var containerIactiveSaveSucceed = await containerInactiveDataAnalyzer.SaveAsync();
                            var containerRotSaveSucceed = await containerRotDataAnalyzer.SaveAsync();
                            var saveSucceed = await containerDataAnalyzer.SaveAsync();

                            if (!saveSucceed)
                            {
                                _logger.Info($"Due to container [{discoveryJob.Id}] [{discoveryJob.ContainerName}] data save failure, all analysis jobs will be set as failed.");
                                aggregateTotalDataAnalyer.Fallback();
                                await _jobDao.ChangeAnalysisJobsStatusAsync(RMDiscoveryJobStatus.Failed, RMDiscoveryJobFailedCause.AnalysisFailed, discoveryJob.Id);
                            }

                            containerLevelCompletedStatus.Add(saveSucceed);

                            var containerLevelStatus = siteLevelCompletedStatus.All(item => item)
                                  && containerIactiveSaveSucceed && containerRotSaveSucceed && saveSucceed;

                            _logger.Info($"End analysis [{discoveryJob.Id}] [{discoveryJob.ContainerName}] discovery job. Status: [{containerLevelStatus}] Save status: [{saveSucceed}].");
                        }

                        var basicInactiveSaveSucceed = await basicInactiveDataAnalyzer.SaveAsync();
                        var basicRotSaveSucceed = await basicRotDataAnalyzer.SaveAsync();

                        var basicLevelStatus = basicInactiveSaveSucceed && basicRotSaveSucceed;
                        if (basicInactiveSaveSucceed && basicRotSaveSucceed)
                        {
                            basicLevelStatus &= await aggregateTotalDataAnalyer.SaveAsync();
                        }

                        if (!basicLevelStatus)
                        {
                            _logger.Info($"Due to tenant [{o365TenantId}] [{contentSource}] data save failure, all analysis jobs will be set as failed.");
                            await _jobDao.ChangeAnalysisJobsStatusAsync(RMDiscoveryJobStatus.Failed, RMDiscoveryJobFailedCause.AnalysisFailed, contentSourceDiscoveryJobs.Value.Select(item => item.Id).ToArray());
                        }

                        basicLevelStatus &= containerLevelCompletedStatus.All(item => item);

                        _logger.Info($"End analysis tenant [{o365TenantId}] [{contentSource}] discovery jobs. Status: [{basicLevelStatus}]. Baisc inactive save status: [{basicInactiveSaveSucceed}]. Basic rot save status: [{basicRotSaveSucceed}].");
                    }
                }

                await RMDiscoveryOffice365SQLiteDBManager.SyncDatabaseToStorageAsync();
                _logger.Info($"Successful sync sqlite db to storage.");

                _logger.Info($"End analysis [{_mainJob.Id}] [{jobType}] main job.");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while run analysis job. Error: {e}");
            }
        }

        private async Task<bool> AnalysisAsync(
            RMDiscoveryOffice365FileExtensionAnalysisManager fileExtensionAnalyzer,
            RMDiscoveryOffice365AggregateTotalDataAnalyzer aggregateTotalDataAnalyer,
            RMDiscoveryOffice365ContainerDataAnalyzer containerDataAnalyer,
            RMDiscoveryOffice365BasicInactiveDataAnalyzer basicInactiveDataAnalyer,
            RMDiscoveryOffice365BasicRotDataAnalyzer basicRotDataAnalyer,
            RMDiscoveryOffice365ContainerInactiveDataAnalyzer containerInactiveDataAnalyer,
            RMDiscoveryOffice365ContainerRotDataAnalyzer containerRotDataAnalyer,
            RMDiscoveryOffice365MainJob mainJob,
            RMDiscoveryOffice365DiscoveryJob discoveryJob,
            RMDiscoveryOffice365AnalysisJob analysisJob
        )
        {
            try
            {
                var res = true;

                await ChangeAnalysisJobToRunningAsync(analysisJob);

                _logger.Info($"Start analysis [{analysisJob.Id}] [{analysisJob.Url}] job");

                var jobType = mainJob.Type;
                var contentSource = discoveryJob.ContentSource;
                var o365TenantId = discoveryJob.O365TenantId;
                var containerId = containerDataAnalyer.ContainerInfo.Id;
                var siteId = analysisJob.SiteId;

                var listManager = new RMDiscoveryOffice365ListManager(o365TenantId, siteId);
                var listIds = await listManager.GetListsAsync();

                var (succeed, aggregateInfo) = await aggregateTotalDataAnalyer.AnalysisAsync(siteId, listIds);
                if (!succeed)
                {
                    await ChangeAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Failed);
                    _reportManager.Increase();
                    return false;
                }

                _temeleter.Increse(aggregateInfo.FileSumCount, aggregateInfo.FileTotalSize);

                var siteDataAnalyzer = new RMDiscoveryOffice365SiteDataAnalyzer(jobType, contentSource, containerId, analysisJob);
                var (initSucceed, siteInfo) = await siteDataAnalyzer.InitAsync(aggregateInfo);
                if (!initSucceed)
                {
                    await ChangeAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Failed);
                    _reportManager.Increase();
                    return false;
                }


                var siteInactiveDataAnalyzer = new RMDiscoveryOffice365SiteInactiveDataAnalyzer(
                                                                    jobType,
                                                                    o365TenantId,
                                                                    contentSource,
                                                                    containerId,
                                                                    siteInfo.Id,
                                                                    siteInfo.SiteId,
                                                                    _sizeRangeIds,
                                                                    _dateRangeIds,
                                                                    listIds,
                                                                    _inactiveRules,
                                                                    fileExtensionAnalyzer,
                                                                    _enableExpandQueryTest);
                var (inactiveSucceed, inactiveDataList) = await siteInactiveDataAnalyzer.AnalysisAsync();
                res &= inactiveSucceed;

                if (inactiveSucceed)
                {
                    containerInactiveDataAnalyer.Increse(inactiveDataList);
                    basicInactiveDataAnalyer.Increse(inactiveDataList);
                }

                var siteRotDataAnalyzer = new RMDiscoveryOffice365SiteRotDataAnalyzer(
                                                         jobType,
                                                         o365TenantId,
                                                         contentSource,
                                                         containerId,
                                                         siteInfo.Id,
                                                         siteInfo.SiteId,
                                                         _sizeRangeIds,
                                                         _dateRangeIds,
                                                         listIds,
                                                         _rotRules,
                                                         fileExtensionAnalyzer,
                                                         _enableExpandQueryTest);
                var (rotRuleLevelSucceed, rotRuleLevelDataList) = await siteRotDataAnalyzer.AnalysisRuleLevelAsync();
                res &= rotRuleLevelSucceed;

                if (rotRuleLevelSucceed)
                {
                    containerRotDataAnalyer.Increse(rotRuleLevelDataList);
                    basicRotDataAnalyer.Increse(rotRuleLevelDataList);
                }

                var (rotCategoryLevelSucceed, rotCategoryLevelDataList) = await siteRotDataAnalyzer.AnalysisCategoryLevelAsync();
                res &= rotCategoryLevelSucceed;

                if (rotCategoryLevelSucceed)
                {
                    containerRotDataAnalyer.Increse(rotCategoryLevelDataList);
                    basicRotDataAnalyer.Increse(rotCategoryLevelDataList);
                }

                var (rotRootLevelSucceed, rotRootLevelDataList) = await siteRotDataAnalyzer.AnalysisRootLevelAsync();
                res &= rotRootLevelSucceed;

                if (rotRootLevelSucceed)
                {
                    containerRotDataAnalyer.Increse(rotRootLevelDataList);
                    basicRotDataAnalyer.Increse(rotRootLevelDataList);
                }

                if (res)
                {
                    containerDataAnalyer.Increse(aggregateInfo);
                    aggregateTotalDataAnalyer.Increse(aggregateInfo);
                }

                await ChangeAnalysisJobToEndAsync(analysisJob, res ? RMDiscoveryJobStatus.Finished : RMDiscoveryJobStatus.Failed);

                _reportManager.Increase();

                _logger.Info($"End analysis [{analysisJob.Id}] [{analysisJob.Url}] job. Status: [{res}]");

                return res;
            }
            catch (Exception e)
            {
                _reportManager.Increase();
                await ChangeAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Failed);
                _logger.Error($"An error occurred while analysis [{analysisJob.Id}] [{analysisJob.Url}] job. Error: {e}");
                return false;
            }
        }

        private async Task ChangeAnalysisJobToEndAsync(RMDiscoveryOffice365AnalysisJob analysisJob, RMDiscoveryJobStatus jobStatus)
        {
            analysisJob.Status = jobStatus;
            analysisJob.FailedCause = jobStatus == RMDiscoveryJobStatus.Finished ? RMDiscoveryJobFailedCause.None : RMDiscoveryJobFailedCause.AnalysisFailed;
            analysisJob.EndTime = DateTime.UtcNow.Ticks;
            await _jobDao.AddOrUpdateAnalysisJobAsync(analysisJob);
        }

        private async Task ChangeDiscoveryFailedAnalysisJobToEndAsync(RMDiscoveryOffice365AnalysisJob analysisJob, RMDiscoveryJobStatus jobStatus)
        {
            analysisJob.Status = jobStatus;
            analysisJob.EndTime = DateTime.UtcNow.Ticks;
            await _jobDao.AddOrUpdateAnalysisJobAsync(analysisJob);
        }

        private async Task ChangeAnalysisJobToRunningAsync(RMDiscoveryOffice365AnalysisJob analysisJob)
        {
            analysisJob.Status = RMDiscoveryJobStatus.Running;
            analysisJob.StartTime = DateTime.UtcNow.Ticks;
            await _jobDao.AddOrUpdateAnalysisJobAsync(analysisJob);
        }

        private async Task CleanupDeletedSiteDataAsync(Guid o365TenantId, Guid siteId)
        {
            try
            {
                _logger.Info($"Starting cleanup for deleted site [{siteId}] in tenant [{o365TenantId}].");

                // Cleanup from SQL Server (Discovery DB) - includes SiteInfo 
                var siteIdInt = await _nodeDao.DeleteSiteDataBySiteIdAsync(o365TenantId, siteId);
                _logger.Info($"Successfully cleaned up SQL Server data for site [{siteId}].");

                if (siteIdInt < 0)
                {
                    _logger.Warn($"Site ID [{siteId}] is invalid for SQLite cleanup. Skipping SQLite cleanup.");
                    return;
                }
                // Cleanup from SQLite DB 
                await RMDiscoveryOffice365SQLiteDBManager.DeleteSiteDataBySiteIdAsync(o365TenantId, siteIdInt);
                _logger.Info($"Successfully cleaned up SQLite data for site [{siteId}].");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while cleaning up data for deleted site [{siteId}]. Error: {e}");
            }
        }
    }
}
