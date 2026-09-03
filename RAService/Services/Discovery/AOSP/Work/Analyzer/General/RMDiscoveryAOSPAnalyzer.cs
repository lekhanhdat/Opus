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
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Core.Discovery.DBManager.SQLite;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.AOSP.Work.Analyzer.General.Inactive;
using AvePoint.RA.Service.Services.Discovery.AOSP.Work.Analyzer.General.ROT;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V4.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.AOSP.Work.Analyzer.General
{
    public class RMDiscoveryAOSPAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryAOSPAnalyzer));

        private readonly IRMDiscoveryAOSPJobDao _jobDao;

        private readonly IRMDiscoveryAOSPNodeDao _nodeDao;

        private readonly IRMReportManager _reportManager;

        private readonly RMDiscoveryAOSPMainJob _mainJob;

        private readonly List<RMDiscoveryAOSPRuleInfo> _rotRules;

        private readonly List<RMDiscoveryAOSPRuleInfo> _inactiveRules;

        public RMDiscoveryAOSPAnalyzer(
                IRMReportManager reportManager,
                RMDiscoveryAOSPMainJob mainJob,
                List<RMDiscoveryAOSPRuleInfo> rules
            )
        {
            _jobDao = new RMDiscoveryAOSPJobDao();
            _nodeDao = new RMDiscoveryAOSPNodeDao();
            _reportManager = reportManager;
            _mainJob = mainJob;
            _rotRules = rules.Where(item => item.DefinitionKind == RMDiscoveryRuleDefinitionKind.ROT).ToList();
            _inactiveRules = rules.Where(item => item.DefinitionKind == RMDiscoveryRuleDefinitionKind.Inactive).ToList();
        }

        public async Task AnalysisAsync()
        {
            try
            {
                _reportManager.IncreaseBase(_mainJob.SitesCount);
                _reportManager.StartUpdateJobProgress();

                var jobType = _mainJob.Type;

                _logger.Info($"Start analysis [{_mainJob.Id}] [{jobType}] main job.");

                await RMDiscoveryAOSPSQLiteDBManager.DownloadDatabaseAsync();
                _logger.Info($"Successful download sqlite database from storage.");

                var discoveryJobs = await _jobDao.GetDiscoveryJobsAsync(_mainJob.Id, RMDiscoveryJobStatus.Completing);
                var tenantGroupedDiscoveryJobs = discoveryJobs.GroupBy(item => item.O365TenantId).ToDictionary(item => item.Key, item => item.ToList());
                foreach (var tenantDiscoveryJobs in tenantGroupedDiscoveryJobs)
                {
                    var o365TenantId = tenantDiscoveryJobs.Key;

                    _logger.Info($"O365 tenant [{o365TenantId}] inactive rule is {string.Join(",", _inactiveRules.Select(rule => rule.UniqueId))}.");
                    if (jobType != RMDiscoveryJobType.Rescan)
                    {
                        await RMDiscoveryAOSPSQLiteDBManager.InitInactiveTablesAsync(o365TenantId, _inactiveRules.ConvertAll(item => item.ToCustomColumn()));
                    }

                    _logger.Info($"O365 tenant [{o365TenantId}] inactive rule columns is {string.Join(",", _inactiveRules.ConvertAll(item => item.ToCustomColumn()).Select(item => item.Name))}");

                    if (jobType != RMDiscoveryJobType.Rescan)
                    {
                        await RMDiscoveryAOSPSQLiteDBManager.InitRotTablesAsync(o365TenantId);
                    }
                    _logger.Info($"Successful create o365 tenant [{o365TenantId}] inactive & rot table.");

                    var fileExtensionAnalyzer = new RMDiscoveryAOSPFileExtensionAnalysisManager(o365TenantId);
                    await fileExtensionAnalyzer.InitAsync();

                    var contentSourceGroupedDiscoveryJobs = tenantDiscoveryJobs.Value.GroupBy(item => item.ContentSource).ToDictionary(item => item.Key, item => item.ToList());
                    foreach (var contentSourceDiscoveryJobs in contentSourceGroupedDiscoveryJobs)
                    {
                        var contentSource = contentSourceDiscoveryJobs.Key;

                        _logger.Info($"Start analysis tenant [{o365TenantId}] [{contentSource}] discovery jobs.");

                        var aggregateTotalDataAnalyer = new RMDiscoveryAOSPAggregateTotalDataAnalyzer(o365TenantId, _mainJob.Type, contentSource);
                        var basicInactiveDataAnalyzer = new RMDiscoveryAOSPBasicInactiveDataAnalyzer(jobType, o365TenantId, contentSource, _inactiveRules);
                        var basicRotDataAnalyzer = new RMDiscoveryAOSPBasicRotDataAnalyzer(jobType, o365TenantId, contentSource);

                        var containerLevelCompletedStatus = new HashSet<bool>();

                        foreach (var discoveryJob in contentSourceDiscoveryJobs.Value)
                        {
                            _logger.Info($"Start analysis [{discoveryJob.Id}] [{discoveryJob.ContainerName}] discovery job.");

                            var containerDataAnalyzer = new RMDiscoveryAOSPContainerDataAnalyzer(_mainJob.Type, discoveryJob);
                            var (initSucceed, containerInfo) = await containerDataAnalyzer.InitAsync();
                            if (!initSucceed)
                            {
                                await _jobDao.ChangeAnalysisJobsStatusAsync(RMDiscoveryJobStatus.Failed, RMDiscoveryJobFailedCause.AnalysisFailed, "An error occurred during the initialization of container information", discoveryJob.Id);
                                continue;
                            }

                            var containerInactiveDataAnalyzer = new RMDiscoveryAOSPContainerInactiveDataAnalyzer(jobType, o365TenantId, containerInfo.Id, _inactiveRules);
                            var containerRotDataAnalyzer = new RMDiscoveryAOSPContainerRotDataAnalyzer(jobType, o365TenantId, containerInfo.Id);

                            var enumerableAnalysisJobs = _jobDao.GetAnalysisJobsByDiscoveryJobWithPaginationAsync(discoveryJob.Id, 1000, Contract.Discovery.Job.RMDiscoveryJobStatus.Pending);

                            var siteLevelCompletedStatus = new HashSet<bool>();

                            aggregateTotalDataAnalyer.Memeory();

                            await foreach (var analysisJob in enumerableAnalysisJobs)
                            {
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
                                    analysisJob);
                                siteLevelCompletedStatus.Add(res);
                            }

                            var containerIactiveSaveSucceed = await containerInactiveDataAnalyzer.SaveAsync();
                            var containerRotSaveSucceed = await containerRotDataAnalyzer.SaveAsync();
                            var saveSucceed = await containerDataAnalyzer.SaveAsync();

                            if (!saveSucceed)
                            {
                                _logger.Info($"Due to container [{discoveryJob.Id}] [{discoveryJob.ContainerName}] data save failure, all analysis jobs will be set as failed.");
                                aggregateTotalDataAnalyer.Fallback();
                                await _jobDao.ChangeAnalysisJobsStatusAsync(RMDiscoveryJobStatus.Failed, RMDiscoveryJobFailedCause.AnalysisFailed, "Failed to save container data", discoveryJob.Id);
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
                            await _jobDao.ChangeAnalysisJobsStatusAsync(RMDiscoveryJobStatus.Failed, RMDiscoveryJobFailedCause.AnalysisFailed, "Failed to save tenant data", contentSourceDiscoveryJobs.Value.Select(item => item.Id).ToArray());
                        }

                        basicLevelStatus &= containerLevelCompletedStatus.All(item => item);

                        _logger.Info($"End analysis tenant [{o365TenantId}] [{contentSource}] discovery jobs. Status: [{basicLevelStatus}]. Baisc inactive save status: [{basicInactiveSaveSucceed}]. Basic rot save status: [{basicRotSaveSucceed}].");
                    }
                }

                await RMDiscoveryAOSPSQLiteDBManager.SyncDatabaseToStorageAsync();
                _logger.Info($"Successful sync sqlite db to storage.");

                _logger.Info($"End analysis [{_mainJob.Id}] [{jobType}] main job.");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while run analysis job. Error: {e}");
            }
        }

        private async Task<bool> AnalysisAsync(
            RMDiscoveryAOSPFileExtensionAnalysisManager fileExtensionAnalyzer,
            RMDiscoveryAOSPAggregateTotalDataAnalyzer aggregateTotalDataAnalyer,
            RMDiscoveryAOSPContainerDataAnalyzer containerDataAnalyer,
            RMDiscoveryAOSPBasicInactiveDataAnalyzer basicInactiveDataAnalyer,
            RMDiscoveryAOSPBasicRotDataAnalyzer basicRotDataAnalyer,
            RMDiscoveryAOSPContainerInactiveDataAnalyzer containerInactiveDataAnalyer,
            RMDiscoveryAOSPContainerRotDataAnalyzer containerRotDataAnalyer,
            RMDiscoveryAOSPMainJob mainJob,
            RMDiscoveryAOSPDiscoveryJob discoveryJob,
            RMDiscoveryAOSPAnalysisJob analysisJob
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

                var analyzedDataManager = new RMDiscoveryAOSPAnalyzedDataManager(o365TenantId, siteId);

                var (succeed, aggregateInfo, message) = aggregateTotalDataAnalyer.Analysis(analyzedDataManager);
                if (!succeed)
                {
                    await ChangeAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Failed, message);
                    _reportManager.Increase();
                    return false;
                }

                var siteDataAnalyzer = new RMDiscoveryAOSPSiteDataAnalyzer(jobType, contentSource, containerId, analysisJob);
                var (initSucceed, siteInfo, errorsMessage) = await siteDataAnalyzer.InitAsync(aggregateInfo);
                if (!initSucceed)
                {
                    await ChangeAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Failed, errorsMessage);
                    _reportManager.Increase();
                    return false;
                }


                var siteInactiveDataAnalyzer = new RMDiscoveryAOSPSiteInactiveDataAnalyzer(
                                                                    jobType,
                                                                    o365TenantId,
                                                                    contentSource,
                                                                    containerId,
                                                                    siteInfo.Id,
                                                                    siteInfo.SiteId,
                                                                    _inactiveRules,
                                                                    fileExtensionAnalyzer);

                var siteRotDataAnalyzer = new RMDiscoveryAOSPSiteRotDataAnalyzer(
                                                         jobType,
                                                         o365TenantId,
                                                         contentSource,
                                                         containerId,
                                                         siteInfo.Id,
                                                         siteInfo.SiteId,
                                                         _rotRules,
                                                         fileExtensionAnalyzer);

                foreach (var analyzedDataInfo in analyzedDataManager.GetAnalyzedDataInfoes())
                {
                    siteInactiveDataAnalyzer.Increse(analyzedDataInfo);
                    siteRotDataAnalyzer.Increse(analyzedDataInfo);
                }

                var (inactiveSucceed, inactiveDataList, inactiveErrorMessage) = await siteInactiveDataAnalyzer.AnalysisAsync();
                res &= inactiveSucceed;
                var errorMessage = inactiveErrorMessage;

                if (inactiveSucceed)
                {
                    containerInactiveDataAnalyer.Increse(inactiveDataList);
                    basicInactiveDataAnalyer.Increse(inactiveDataList);
                }

                var (rotRuleLevelSucceed, rotRuleLevelDataList, rotRuleLevelErrorMessage) = await siteRotDataAnalyzer.AnalysisRuleLevelAsync();
                res &= rotRuleLevelSucceed;

                if (rotRuleLevelSucceed)
                {
                    containerRotDataAnalyer.Increse(rotRuleLevelDataList);
                    basicRotDataAnalyer.Increse(rotRuleLevelDataList);
                }
                else
                {
                    errorMessage = rotRuleLevelErrorMessage;
                }

                var (rotCategoryLevelSucceed, rotCategoryLevelDataList, rotCategoryLevelErrorMessage) = await siteRotDataAnalyzer.AnalysisCategoryLevelAsync();
                res &= rotCategoryLevelSucceed;

                if (rotCategoryLevelSucceed)
                {
                    containerRotDataAnalyer.Increse(rotCategoryLevelDataList);
                    basicRotDataAnalyer.Increse(rotCategoryLevelDataList);
                }
                else
                {
                    errorMessage = rotCategoryLevelErrorMessage;
                }

                var (rotRootLevelSucceed, rotRootLevelDataList, rotRootLevelErrorMessage) = await siteRotDataAnalyzer.AnalysisRootLevelAsync();
                res &= rotRootLevelSucceed;

                if (rotRootLevelSucceed)
                {
                    containerRotDataAnalyer.Increse(rotRootLevelDataList);
                    basicRotDataAnalyer.Increse(rotRootLevelDataList);
                }
                else
                {
                    errorMessage = rotRootLevelErrorMessage;
                }

                if (res)
                {
                    containerDataAnalyer.Increse(aggregateInfo);
                    aggregateTotalDataAnalyer.Increse(aggregateInfo);
                }

                await ChangeAnalysisJobToEndAsync(analysisJob, res ? RMDiscoveryJobStatus.Finished : RMDiscoveryJobStatus.Failed, res ? string.Empty : errorMessage);

                _reportManager.Increase();

                _logger.Info($"End analysis [{analysisJob.Id}] [{analysisJob.Url}] job. Status: [{res}]");

                return res;
            }
            catch (Exception e)
            {
                _reportManager.Increase();
                await ChangeAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Failed, e.Message);
                _logger.Error($"An error occurred while analysis [{analysisJob.Id}] [{analysisJob.Url}] job. Error: {e}");
                return false;
            }
        }

        private async Task ChangeAnalysisJobToRunningAsync(RMDiscoveryAOSPAnalysisJob analysisJob)
        {
            analysisJob.Status = RMDiscoveryJobStatus.Running;
            analysisJob.StartTime = DateTime.UtcNow.Ticks;
            analysisJob.Comment = string.Empty;
            await _jobDao.AddOrUpdateAnalysisJobAsync(analysisJob);
        }

        private async Task ChangeAnalysisJobToEndAsync(RMDiscoveryAOSPAnalysisJob analysisJob, RMDiscoveryJobStatus jobStatus, string comment = "")
        {
            analysisJob.Status = jobStatus;
            analysisJob.FailedCause = jobStatus == RMDiscoveryJobStatus.Finished ? RMDiscoveryJobFailedCause.None : RMDiscoveryJobFailedCause.AnalysisFailed;
            analysisJob.EndTime = DateTime.UtcNow.Ticks;
            analysisJob.Comment = comment ?? string.Empty;
            await _jobDao.AddOrUpdateAnalysisJobAsync(analysisJob);
        }
    }
}
