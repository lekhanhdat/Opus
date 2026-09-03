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
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.General.Inactive;
using AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.General.Rot;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.General
{
    public class RMDiscoveryGoogleAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleAnalyzer));

        private readonly IRMDiscoveryGoogleJobDao _jobDao;

        private readonly IRMDiscoveryGoogleNodeDao _nodeDao;

        private readonly IRMReportManager _reportManager;

        private readonly RMDiscoveryGoogleMainJob _mainJob;

        private readonly List<int> _sizeRangeIds;

        private readonly List<int> _dateRangeIds;

        private readonly List<RMDiscoveryGoogleRuleInfo> _inactiveRules;

        private readonly List<RMDiscoveryGoogleRuleInfo> _rotRules;

        public RMDiscoveryGoogleAnalyzer(
                IRMReportManager reportManager,
                RMDiscoveryGoogleMainJob mainJob,
                List<int> sizeRangeIds,
                List<int> dateRangeIds,
                List<RMDiscoveryGoogleRuleInfo> rules
            )
        {
            _jobDao = new RMDiscoveryGoogleJobDao();
            _nodeDao = new RMDiscoveryGoogleNodeDao();
            _reportManager = reportManager;
            _mainJob = mainJob;
            _sizeRangeIds = sizeRangeIds;
            _dateRangeIds = dateRangeIds;
            _inactiveRules = [];
            _rotRules = rules.Where(item => item.DefinitionKind == RMDiscoveryRuleDefinitionKind.ROT).ToList();
        }

        public async Task AnalysisAsync()
        {
            try
            {
                _reportManager.IncreaseBase(_mainJob.DrivesCount);
                _reportManager.StartUpdateJobProgress();

                var jobType = _mainJob.Type;

                _logger.Info($"Start analysis [{_mainJob.Id}] [{jobType}] main job.");

                if (jobType == RMDiscoveryJobType.Newly)
                {
                    RMDiscoveryGoogleSQLiteDBManager.CreateDatabase();
                    _logger.Info($"Successful create sqlite database.");
                }
                else
                {
                    await RMDiscoveryGoogleSQLiteDBManager.DownloadDatabaseAsync();
                    _logger.Info($"Successful download sqlite database from storage.");
                }

                var discoveryJobs = await _jobDao.GetDiscoveryJobsAsync(_mainJob.Id, RMDiscoveryJobStatus.Completing);
                var organizationGroupedDiscoveryJobs = discoveryJobs.GroupBy(item => item.OrganizationId).ToDictionary(item => item.Key, item => item.ToList());
                if (!organizationGroupedDiscoveryJobs.Any())
                {
                    _logger.Info($"There is no discovery job has [{RMDiscoveryJobStatus.Completing}] status.");
                }
                foreach (var organizationDiscoveryJobs in organizationGroupedDiscoveryJobs)
                {
                    var organizationId = organizationDiscoveryJobs.Key;

                    await RMDiscoveryGoogleSQLiteDBManager.InitInactiveTablesAsync(organizationId, _inactiveRules.ConvertAll(item => item.ToCustomColumn()));
                    await RMDiscoveryGoogleSQLiteDBManager.InitRotTablesAsync(organizationId);
                    _logger.Info($"Successful create google organization [{organizationId}] inactive & rot table.");

                    var fileExtensionAnalyzer = new RMDiscoveryGoogleFileExtensionAnalysisManager(organizationId);
                    await fileExtensionAnalyzer.InitAsync();

                    _logger.Info($"Start analysis organization [{organizationId}] discovery jobs.");

                    var aggregateTotalDataAnalyer = new RMDiscoveryGoogleAggregateTotalDataAnalyzer(organizationId, _mainJob.Type, _sizeRangeIds, _dateRangeIds);
                    var basicInactiveDataAnalyzer = new RMDiscoveryGoogleBasicInactiveDataAnalyzer(jobType, organizationId, _inactiveRules);
                    var basicRotDataAnalyzer = new RMDiscoveryGoogleBasicRotDataAnalyzer(jobType, organizationId);

                    var containerLevelCompletedStatus = new HashSet<bool>();

                    foreach (var discoveryJob in organizationDiscoveryJobs.Value)
                    {
                        _logger.Info($"Start analysis [{discoveryJob.Id}] [{discoveryJob.ContainerName}] discovery job.");

                        var containerDataAnalyzer = new RMDiscoveryGoogleContainerDataAnalyzer(_mainJob.Type, discoveryJob);
                        var (initSucceed, containerInfo) = await containerDataAnalyzer.InitAsync();
                        if (!initSucceed)
                        {
                            await _jobDao.ChangeAnalysisJobsStatusAsync(RMDiscoveryJobStatus.Failed, RMDiscoveryJobFailedCause.AnalysisFailed, discoveryJob.Id);
                            continue;
                        }

                        var containerInactiveDataAnalyzer = new RMDiscoveryGoogleContainerInactiveDataAnalyzer(jobType, organizationId, containerInfo.Id, _inactiveRules);
                        var containerRotDataAnalyzer = new RMDiscoveryGoogleContainerRotDataAnalyzer(jobType, organizationId, containerInfo.Id);

                        var enumerableAnalysisJobs = _jobDao.GetAnalysisJobsByDiscoveryJobWithPaginationAsync(discoveryJob.Id, 1000, Contract.Discovery.Job.RMDiscoveryJobStatus.Pending);

                        var siteLevelCompletedStatus = new HashSet<bool>();

                        aggregateTotalDataAnalyer.Memeory();

                        await foreach (var analysisJob in enumerableAnalysisJobs)
                        {
                            if (_mainJob.Type == RMDiscoveryJobType.Append)
                            {
                                var existsSitesInfo = await _nodeDao.GetDiscoveryGoogleDriveInfoAsync(organizationId, analysisJob.DriveId);
                                if (existsSitesInfo != null)
                                {
                                    _logger.Warn($"The drive [{analysisJob.DriveId}] has been analyzed before.");
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
                        _logger.Info($"Due to tenant [{organizationId}]  data save failure, all analysis jobs will be set as failed.");
                        await _jobDao.ChangeAnalysisJobsStatusAsync(RMDiscoveryJobStatus.Failed, RMDiscoveryJobFailedCause.AnalysisFailed, organizationDiscoveryJobs.Value.Select(item => item.Id).ToArray());
                    }

                    basicLevelStatus &= containerLevelCompletedStatus.All(item => item);

                    _logger.Info($"End analysis tenant [{organizationId}] discovery jobs. Status: [{basicLevelStatus}]. Baisc inactive save status: [{basicInactiveSaveSucceed}]. Basic rot save status: [{basicRotSaveSucceed}].");
                }

                await RMDiscoveryGoogleSQLiteDBManager.SyncDatabaseToStorageAsync();
                _logger.Info($"Successful sync sqlite db to storage.");

                _logger.Info($"End analysis [{_mainJob.Id}] [{jobType}] main job.");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while run analysis job. Error: {e}");
            }
        }

        private async Task<bool> AnalysisAsync(
            RMDiscoveryGoogleFileExtensionAnalysisManager fileExtensionAnalyzer,
            RMDiscoveryGoogleAggregateTotalDataAnalyzer aggregateTotalDataAnalyer,
            RMDiscoveryGoogleContainerDataAnalyzer containerDataAnalyer,
            RMDiscoveryGoogleBasicInactiveDataAnalyzer basicInactiveDataAnalyer,
            RMDiscoveryGoogleBasicRotDataAnalyzer basicRotDataAnalyer,
            RMDiscoveryGoogleContainerInactiveDataAnalyzer containerInactiveDataAnalyer,
            RMDiscoveryGoogleContainerRotDataAnalyzer containerRotDataAnalyer,
            RMDiscoveryGoogleMainJob mainJob,
            RMDiscoveryGoogleDiscoveryJob discoveryJob,
            RMDiscoveryGoogleAnalysisJob analysisJob
        )
        {
            try
            {
                var res = true;

                await ChangeAnalysisJobToRunningAsync(analysisJob);

                _logger.Info($"Start analysis [{analysisJob.Id}] [{analysisJob.DriveName}] job");

                var jobType = mainJob.Type;
                var organizationId = discoveryJob.OrganizationId;
                var containerId = containerDataAnalyer.ContainerInfo.Id;
                var driveId = analysisJob.DriveId;

                var (succeed, aggregateInfo) = await aggregateTotalDataAnalyer.AnalysisAsync(driveId);
                if (!succeed)
                {
                    await ChangeAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Failed);
                    _reportManager.Increase();
                    return false;
                }

                var siteDataAnalyzer = new RMDiscoveryGoogleDriveDataAnalyzer(jobType, containerId, analysisJob);
                var (initSucceed, driveInfo) = await siteDataAnalyzer.InitAsync(aggregateInfo);
                if (!initSucceed)
                {
                    await ChangeAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Failed);
                    _reportManager.Increase();
                    return false;
                }


                var siteInactiveDataAnalyzer = new RMDiscoveryGoogleDriveInactiveDataAnalyzer(
                                                                    jobType,
                                                                    organizationId,
                                                                    containerId,
                                                                    driveInfo.Id,
                                                                    driveInfo.DriveId,
                                                                    _sizeRangeIds,
                                                                    _dateRangeIds,
                                                                    [],
                                                                    fileExtensionAnalyzer);
                var (inactiveSucceed, inactiveDataList) = await siteInactiveDataAnalyzer.AnalysisAsync();
                res &= inactiveSucceed;

                if (inactiveSucceed)
                {
                    containerInactiveDataAnalyer.Increse(inactiveDataList);
                    basicInactiveDataAnalyer.Increse(inactiveDataList);
                }

                var siteRotDataAnalyzer = new RMDiscoveryGoogleDriveRotDataAnalyzer(
                                                         jobType,
                                                         organizationId,
                                                         containerId,
                                                         driveInfo.Id,
                                                         driveInfo.DriveId,
                                                         _sizeRangeIds,
                                                         _dateRangeIds,
                                                         _rotRules,
                                                         fileExtensionAnalyzer);
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

                _logger.Info($"End analysis [{analysisJob.Id}] [{analysisJob.DriveName}] job. Status: [{res}]");

                return res;
            }
            catch (Exception e)
            {
                _reportManager.Increase();
                await ChangeAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Failed);
                _logger.Error($"An error occurred while analysis [{analysisJob.Id}] [{analysisJob.DriveName}] job. Error: {e}");
                return false;
            }
        }

        private async Task ChangeAnalysisJobToEndAsync(RMDiscoveryGoogleAnalysisJob analysisJob, RMDiscoveryJobStatus jobStatus)
        {
            analysisJob.Status = jobStatus;
            analysisJob.FailedCause = jobStatus == RMDiscoveryJobStatus.Finished ? RMDiscoveryJobFailedCause.None : RMDiscoveryJobFailedCause.AnalysisFailed;
            analysisJob.EndTime = DateTime.UtcNow.Ticks;
            await _jobDao.AddOrUpdateAnalysisJobAsync(analysisJob);
        }

        private async Task ChangeAnalysisJobToRunningAsync(RMDiscoveryGoogleAnalysisJob analysisJob)
        {
            analysisJob.Status = RMDiscoveryJobStatus.Running;
            analysisJob.StartTime = DateTime.UtcNow.Ticks;
            await _jobDao.AddOrUpdateAnalysisJobAsync(analysisJob);
        }
    }
}
