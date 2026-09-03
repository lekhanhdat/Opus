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
using System.Threading;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;

namespace AvePoint.RA.Service.Services.FileSystem;

public class RMFSUpgradeDataService
{
    private readonly RALogger _logger = new (typeof(RMFSUpgradeDataService));
    private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
    
    private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
    
    private IJobMonitorDao JobMonitorDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
    
    private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
    
    private const string JpmcUpgradeStatusKey = "JPMC_UPGRADE_STATUS";
    
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(10);

    public string RealRunMigrateDataForJPMC()
    {
        var jobId = string.Empty;
        try
        {
            CheckMigrationJobRunning();

            jobId = JobMonitorService.CreateJob(JobType.MigrateDataCosmosDbForJPMC, "RM_TS_RunSchedule");
            JobQueueService.HandleMessage(new Contract.CloudService.JobQueueMessage()
            {
                JobId = jobId,
                JobType = JobType.MigrateDataCosmosDbForJPMC,
                CommandLine = $"{JobType.MigrateDataCosmosDbForJPMC} {jobId}",
            });
            _logger.Info($"Created migration job for JPMC successfully, job id:{jobId}");
        }
        catch(Exception e)
        {
            _logger.Error($"Failed to create migration job for JPMC, exception: {e}");
        }
       
        return jobId;
    }

    private void CheckMigrationJobRunning()
    {
        var hasRunningJob = JobMonitorService.GetRunningJobsCount(JobType.MigrateDataCosmosDbForJPMC);
        if (hasRunningJob > 0)
        {
            _logger.Info($"There is already a running job for JPMC Cosmos DB migration, skip creating new job. Job count: {hasRunningJob}.");
            throw new Exception($"There is already a running job for JPMC Cosmos DB migration, skip creating new job. Job count: {hasRunningJob}.");
        }
    }

    public async Task EnsureMigrationJobAsync(string tenantId, string registerEmail)
    {
        try
        {
            CheckMigrationJobRunning();
            using var timer = new PeriodicTimer(PollInterval);
            do
            {
                var isHavingJob = await JobMonitorDao.IsHavingRunningJob();
                _logger.Info($"Is Having Running Job: {isHavingJob}");
                if (!isHavingJob)
                {
                    if (TryStartMigrationJob(tenantId, registerEmail))
                    {
                        _logger.Info($"Created JPMC Cosmos DB migration job for tenant: {tenantId}.");
                    }
                    return;

                }

                _logger.Info(
                    $"Skip JPMC Cosmos DB migration polling because active jobs still exist. Tenant: {tenantId}.");
            } while (await timer.WaitForNextTickAsync());
        }
        catch (Exception ex)
        {
            _logger.Error($"Exception in EnsureMigrationJobAsync: {ex}");
        }
    }

    private bool TryStartMigrationJob(string tenantId, string registerEmail)
    {
        var status = GetUpgradeStatus();
        if (status.HasValue && status.Value != 0)
        {
            _logger.Info($"Skip JPMC Cosmos DB migration because status is {status.Value}. Tenant: {tenantId}.");
            return false;
        }

        KeyValueDao.UpsertAsync(JpmcUpgradeStatusKey, "1").GetAwaiter().GetResult();

        RealRunMigrateDataForJPMC();
        return true;
    }

    public int? GetUpgradeStatus()
    {
        var keyValue = KeyValueDao.GetValueByKey(JpmcUpgradeStatusKey);
        if (keyValue == null || string.IsNullOrWhiteSpace(keyValue.Value))
        {
            return null;
        }

        return int.TryParse(keyValue.Value, out var status) ? status : null;
    }
}