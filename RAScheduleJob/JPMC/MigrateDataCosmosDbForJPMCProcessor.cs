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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.AzureCosmosDB;
using AvePoint.RA.DB.AzureCosmosDB.Concurrent;
using AvePoint.RA.DB.AzureCosmosDB.Model;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RACommonUtility.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.ScheduleJob.JPMC
{
    internal class MigrateDataCosmosDbForJPMCProcessor
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(MigrateDataCosmosDbForJPMCProcessor));

        private readonly IRMReportManager s_reportManager = ReportMangerFactory.Instance.ReportManager;

        private const string JpmcUpgradeStatusKey = "JPMC_UPGRADE_STATUS";
        
        private readonly string _jobId;
        private readonly IRMKeyValueDao _keyValueDao;
        private readonly IExplorerDao _explorerDao;

        private int _batchSize;
        private RMAzureCosmosDBDelayConcurrentAction _concurrentAction;
        private JPMCUpgradeSetting _upgradeSetting;

        public MigrateDataCosmosDbForJPMCProcessor(string jobId)
        {
            _jobId = jobId;
            _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
            _keyValueDao.UpsertAsync("JPMC_UPGRADE_STATUS", "2").GetAwaiter().GetResult();
            _explorerDao = new ExplorerDao(true, false);
            SetUpTargetContainer();
            ReportMangerFactory.Instance.Init(jobId, AvePoint.RA.Contract.JobMonitor.JobType.MigrateDataCosmosDbForJPMC);
            s_reportManager.StartUpdateJobProgress();
            s_reportManager.IncreaseBase(10000);
        }


        private void SetUpTargetContainer()
        {
            _upgradeSetting = FSHighPerformanceUtility.LoadFSUpgradeConfig();
            if (_upgradeSetting != null)
            {
                _logger.Info($"Current retry times: {_upgradeSetting.RetryTimes}, parallelism: {_upgradeSetting.MaxDegreeOfParallelism}, batch size: {_upgradeSetting.BatchSize}");
            }
            _batchSize = _upgradeSetting?.BatchSize ?? 1000;

            var container = RMAzureCosmosDBContext.GetContainerAsync().GetAwaiter().GetResult();
            _concurrentAction = container
                .UseConcurrentAction()
                .WithMaxDegreeOfParallelism(_upgradeSetting?.MaxDegreeOfParallelism ?? 10)
                .WithRetryTimes(_upgradeSetting?.RetryTimes ?? 10)
                .WithInitialRetryDelayTime(500)
                .ToDelay();
        }

        public async Task RunAsync()
        {                
            var stopwatch = Stopwatch.StartNew();
            var migratedCount = 0;
            var continuationToken = string.Empty;
            await _concurrentAction.StartAsync(OnNotifyAsync).ConfigureAwait(false);
            try
            {
                while (true)
                {
                    var result = _explorerDao.QueryByPage(_ => true, pageCount: _batchSize,
                        continuation: continuationToken, convertCustomColumn2Metainfo: false);
                    var records = result.Item1?.ToList() ?? new List<Record>();
                    continuationToken = result.Item2;
                    if (records.Count == 0)
                    {
                        break;
                    }

                    foreach (var record in records)
                    {
                        await _concurrentAction.Upsert(record).ConfigureAwait(false);
                    }
                    s_reportManager.Increase();

                    migratedCount += records.Count;

                    LogProgress(migratedCount, continuationToken, stopwatch.Elapsed);
                    if (string.IsNullOrEmpty(continuationToken))
                    {
                        break;
                    }
                }

                await SetUpgradeStatusAsync("3");
                _concurrentAction.SetCompleteAdding();
                await _concurrentAction.WaitCompletedAsync().ConfigureAwait(false);
                _logger.Info(
                    $"Completed JPMC Cosmos DB migration. Tenant: {TenantLocalValue.LogonGroupId}, MigratedCount: {migratedCount}, Elapsed: {stopwatch.Elapsed}.");
                s_reportManager.SetJobFinished(JobStatus.Finished);
            }
            catch (Exception ex)
            {
                s_reportManager.SetJobFinished(JobStatus.Failed);
                _logger.Error(
                    $"JPMC Cosmos DB migration failed. Tenant: {TenantLocalValue.LogonGroupId}, JobId: {_jobId}, MigratedCount: {migratedCount}, Error: {ex}");
            }
            finally
            {
                await _concurrentAction.DisposeAsync().ConfigureAwait(false);
            }
        }

        private Task OnNotifyAsync(RMAzureCosmosDBDelayConcurrentActionResult arg)
        {
            if (!arg.IsSucceed)
            {
                _logger.Error("Failed to migrate record to Cosmos DB. RecordId: {0}, Error: {1}", arg.Item.NodeId, arg.Exception);
            }
            return Task.CompletedTask;
        }


        private void LogProgress(int migratedCount, string continuationToken, TimeSpan elapsed)
        {
            var hasMore = !string.IsNullOrEmpty(continuationToken);
            _logger.Info($"JPMC Cosmos DB migration progress. Tenant: {TenantLocalValue.LogonGroupId}, JobId: {_jobId}, MigratedCount: {migratedCount}, HasMore: {hasMore}, Elapsed: {elapsed}.");
        }

        private async Task SetUpgradeStatusAsync(string status)
        {
            await _keyValueDao.UpsertAsync(JpmcUpgradeStatusKey, status);
        }
    }
}
