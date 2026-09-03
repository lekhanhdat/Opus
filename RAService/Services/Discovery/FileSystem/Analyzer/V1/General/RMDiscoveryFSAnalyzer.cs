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
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem;
using AvePoint.RA.DB.Model.Discovery.FileSystem;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Analyzer;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Analyzer.V1.General.Inactive;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Analyzer.V1.General.Rot;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Work.Analyzer.V1.General.Rot;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Work.Analyzer.V1.General
{
    public class RMDiscoveryFSAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryFSAnalyzer));

        private readonly IRMDiscoveryFSJobDao _jobDao;

        private readonly IRMDiscoveryFSNodeDao _nodeDao;

        private readonly IRMReportManager _reportManager;

        private readonly RMDiscoveryFSMainJob _mainJob;

        private readonly List<RMDiscoveryFSRuleInfo> _rotRules;

        private readonly List<RMDiscoveryFSRuleInfo> _inactiveRules;

        public RMDiscoveryFSAnalyzer(
                IRMReportManager reportManager,
                RMDiscoveryFSMainJob mainJob,
                List<RMDiscoveryFSRuleInfo> rules
            )
        {
            _jobDao = new RMDiscoveryFSJobDao();
            _nodeDao = new RMDiscoveryFSNodeDao();
            _reportManager = reportManager;
            _mainJob = mainJob;
            _rotRules = rules.Where(item => item.DefinitionKind == RMDiscoveryRuleDefinitionKind.ROT).ToList();
            _inactiveRules = rules.Where(item => item.DefinitionKind == RMDiscoveryRuleDefinitionKind.Inactive).ToList();
        }

        public async Task AnalysisAsync()
        {
            try
            {
                _reportManager.IncreaseBase(_mainJob.ConnectionCount);
                _reportManager.StartUpdateJobProgress();

                var jobType = _mainJob.Type;
                var containerLevelCompletedStatus = new HashSet<bool>();

                _logger.Info($"Start analysis [{_mainJob.Id}] [{jobType}] main job.");
                
                var aggregateTotalDataAnalyer = new RMDiscoveryFSAggregateTotalDataAnalyzer(_mainJob.Type);
                var basicInactiveDataAnalyzer = new RMDiscoveryFSBasicInactiveDataAnalyzer(jobType, _inactiveRules);
                var basicRotDataAnalyzer = new RMDiscoveryFSBasicRotDataAnalyzer(jobType);
              
                var fileExtensionAnalyzer = new RMDiscoveryFSFileExtensionAnalysisManager();
                await fileExtensionAnalyzer.InitAsync();

                var discoveryJobs = await _jobDao.GetDiscoveryJobsAsync(_mainJob.Id, RMDiscoveryJobStatus.Completing);
                var containerGroupedDiscoveryJobs = discoveryJobs.GroupBy(item => item.ContainerId).ToDictionary(item => item.Key, item => item.ToList());
               
                foreach (var containerDiscoveryJobs in containerGroupedDiscoveryJobs)
                {
                    var containerId = containerDiscoveryJobs.Key;
                    var connectionIds = await GetConnectionUnderContainer(containerId);
                    foreach (var connectionId in connectionIds)
                    {
                        _logger.Info($"Start analysis connection [{connectionId}] discovery jobs.");
                        foreach (var discoveryJob in containerDiscoveryJobs.Value)
                        {
                            _logger.Info($"Start analysis [{discoveryJob.Id}] [{discoveryJob.ContainerName}] discovery job.");

                            var containerDataAnalyzer = new RMDiscoveryFSContainerDataAnalyzer(_mainJob.Type, discoveryJob);
                            var (initSucceed, containerInfo) = await containerDataAnalyzer.InitAsync();
                            if (!initSucceed)
                            {
                                await _jobDao.ChangeAnalysisJobsStatusAsync(RMDiscoveryJobStatus.Failed, RMDiscoveryJobFailedCause.AnalysisFailed, discoveryJob.Id);
                                continue;
                            }

                            var containerInactiveDataAnalyzer = new RMDiscoveryFSContainerInactiveDataAnalyzer(jobType, connectionId, containerInfo.Id, _inactiveRules);
                            var containerRotDataAnalyzer = new RMDiscoveryFSContainerRotDataAnalyzer(jobType, connectionId, containerInfo.Id);

                            var enumerableAnalysisJobs = _jobDao.GetAnalysisJobsByDiscoveryJobWithPaginationAsync(discoveryJob.Id, 1000, RMDiscoveryJobStatus.Pending);

                            var siteLevelCompletedStatus = new HashSet<bool>();

                            aggregateTotalDataAnalyer.Memeory();

                            await foreach (var analysisJob in enumerableAnalysisJobs)
                            {
                                if (_mainJob.Type == RMDiscoveryJobType.Append)
                                {
                                    var existsSitesInfo = await _nodeDao.GetDiscoveryConnectionInfoAsync(analysisJob.ConnectionId);
                                    if (existsSitesInfo != null)
                                    {
                                        _logger.Warn($"The connection [{analysisJob.ConnectionId}] has been analyzed before.");
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
                    }
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
                    _logger.Info($"Due to data save failure, all analysis jobs will be set as failed.");
                    await _jobDao.ChangeAnalysisJobsStatusAsync(RMDiscoveryJobStatus.Failed, RMDiscoveryJobFailedCause.AnalysisFailed, containerGroupedDiscoveryJobs.Values.SelectMany(item => item).Select(item => item.Id).ToArray());
                }

                basicLevelStatus &= containerLevelCompletedStatus.All(item => item);
                
                _logger.Info($"End analysis discovery jobs. Status: [{basicLevelStatus}]. Baisc inactive save status: [{basicInactiveSaveSucceed}]. Basic rot save status: [{basicRotSaveSucceed}].");
               
                _logger.Info($"End analysis [{_mainJob.Id}] [{jobType}] main job.");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while run analysis job. Error: {e}");
            }
        }

        private async Task<List<string>> GetConnectionUnderContainer(Guid containerId)
        {
            try
            {
                var group = await _nodeDao.GetConnectionGroupsById(containerId);
                return group?.FSConnections.Select(item => item.Id.ToString()).ToList() ?? new List<string>();
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get connection under container [{containerId}]. Error: {e}");
                return new List<string>();
            }
        }

        private async Task<bool> AnalysisAsync(
            RMDiscoveryFSFileExtensionAnalysisManager fileExtensionAnalyzer,
            RMDiscoveryFSAggregateTotalDataAnalyzer aggregateTotalDataAnalyer,
            RMDiscoveryFSContainerDataAnalyzer containerDataAnalyer,
            RMDiscoveryFSBasicInactiveDataAnalyzer basicInactiveDataAnalyer,
            RMDiscoveryFSBasicRotDataAnalyzer basicRotDataAnalyer,
            RMDiscoveryFSContainerInactiveDataAnalyzer containerInactiveDataAnalyer,
            RMDiscoveryFSContainerRotDataAnalyzer containerRotDataAnalyer,
            RMDiscoveryFSMainJob mainJob,
            RMDiscoveryFSDiscoveryJob discoveryJob,
            RMDiscoveryFSAnalysisJob analysisJob
        )
        {
            try
            {
                var res = true;

                await ChangeAnalysisJobToRunningAsync(analysisJob);

                _logger.Info($"Start analysis [{analysisJob.Id}] [{analysisJob.UNCPath}] job");

                var jobType = mainJob.Type;
                var containerId = containerDataAnalyer.ContainerInfo.Id;
                var connectionId = analysisJob.ConnectionId;

                var analyzedDataManager = new RMDiscoveryFSAnalyzedDataManager(connectionId);
                await analyzedDataManager.Init();
                
                var (succeed, aggregateInfo) = aggregateTotalDataAnalyer.Analysis(analyzedDataManager);
                if (!succeed)
                {
                    await ChangeAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Failed);
                    _reportManager.Increase();
                    return false;
                }

                var connectionDataAnalyzer = new RMDiscoveryFSConnectionDataAnalyzer(jobType, containerId, analysisJob);
                var (initSucceed, connectionInfo) = await connectionDataAnalyzer.InitAsync(aggregateInfo);
                if (!initSucceed)
                {
                    await ChangeAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Failed);
                    _reportManager.Increase();
                    return false;
                }


                var connectionInactiveDataAnalyzer = new RMDiscoveryFSConnectionInactiveDataAnalyzer(
                                                                    jobType,
                                                                    containerId,
                                                                    connectionInfo.Id,
                                                                    connectionInfo.ConnectionId,
                                                                    _inactiveRules,
                                                                    fileExtensionAnalyzer);

                var connectionRotDataAnalyzer = new RMDiscoveryFSConnectionRotDataAnalyzer(
                                                         jobType,
                                                         containerId,
                                                         connectionInfo.Id,
                                                         connectionInfo.ConnectionId,
                                                         _rotRules,
                                                         fileExtensionAnalyzer);

                foreach (var analyzedDataInfo in analyzedDataManager.GetAnalyzedDataInfoes())
                {
                    connectionInactiveDataAnalyzer.Increse(analyzedDataInfo);
                    connectionRotDataAnalyzer.Increse(analyzedDataInfo);
                }

                var (inactiveSucceed, inactiveDataList) = await connectionInactiveDataAnalyzer.AnalysisAsync();
                res &= inactiveSucceed;

                if (inactiveSucceed)
                {
                    containerInactiveDataAnalyer.Increse(inactiveDataList);
                    basicInactiveDataAnalyer.Increse(inactiveDataList);
                }

                var (rotRuleLevelSucceed, rotRuleLevelDataList) = await connectionRotDataAnalyzer.AnalysisRuleLevelAsync();
                res &= rotRuleLevelSucceed;

                if (rotRuleLevelSucceed)
                {
                    containerRotDataAnalyer.Increse(rotRuleLevelDataList);
                    basicRotDataAnalyer.Increse(rotRuleLevelDataList);
                }

                var (rotCategoryLevelSucceed, rotCategoryLevelDataList) = await connectionRotDataAnalyzer.AnalysisCategoryLevelAsync();
                res &= rotCategoryLevelSucceed;

                if (rotCategoryLevelSucceed)
                {
                    containerRotDataAnalyer.Increse(rotCategoryLevelDataList);
                    basicRotDataAnalyer.Increse(rotCategoryLevelDataList);
                }

                var (rotRootLevelSucceed, rotRootLevelDataList) = await connectionRotDataAnalyzer.AnalysisRootLevelAsync();
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

                _logger.Info($"End analysis [{analysisJob.Id}] [{analysisJob.UNCPath}] job. Status: [{res}]");

                return res;
            }
            catch (Exception e)
            {
                _reportManager.Increase();
                await ChangeAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Failed);
                _logger.Error($"An error occurred while analysis [{analysisJob.Id}] [{analysisJob.UNCPath}] job. Error: {e}");
                return false;
            }
        }

        private async Task ChangeAnalysisJobToEndAsync(RMDiscoveryFSAnalysisJob analysisJob, RMDiscoveryJobStatus jobStatus)
        {
            analysisJob.Status = jobStatus;
            analysisJob.FailedCause = jobStatus == RMDiscoveryJobStatus.Finished ? RMDiscoveryJobFailedCause.None : RMDiscoveryJobFailedCause.AnalysisFailed;
            analysisJob.EndTime = DateTime.UtcNow.Ticks;
            await _jobDao.AddOrUpdateAnalysisJobAsync(analysisJob);
        }

        private async Task ChangeAnalysisJobToRunningAsync(RMDiscoveryFSAnalysisJob analysisJob)
        {
            analysisJob.Status = RMDiscoveryJobStatus.Running;
            analysisJob.StartTime = DateTime.UtcNow.Ticks;
            await _jobDao.AddOrUpdateAnalysisJobAsync(analysisJob);
        }
    }
}
