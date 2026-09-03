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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.DataIngestion;
using AvePoint.RA.DB.Dao.DataIngestion.Impl;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.RACommonUtility.JobControl.JPMC;
using AvePoint.RA.Service.RMTasks.Discovery;
using AvePoint.RA.Service.Services.DataIngestion;
using AvePoint.RA.SharePoint.Common;
using System;
using System.Threading.Tasks;
using AvePoint.RA.Contract.RMWeb.JobMonitor;


namespace AvePoint.RA.Service.RMTasks.DataIngestion
{
    public class DataIngestionTaskExecutor : ITaskExecutor
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(DataIngestionTaskExecutor));
        private static IRMDataIngestionMessageDao DataIngestionMessageDao = new RMDataIngestionMessageDao();

        private readonly IRMDataIngestionJobDao _jobDao = new RMDataIngestionJobDao();

        private readonly RMDataIngestionService _dataIngestionService = new();

        public async Task ExecutorAsync(TaskBase task)
        {
            var tenantService = PlatformWindsorManager.GetService<ITenantService>();
            var tenants = tenantService.GetAllAvailableTenantInfo();
            foreach (var tenantInfo in tenants)
            {
                await TenantUtil.RunUnderTenantAsync(tenantInfo.TenantId, tenantInfo.RegisterEmail, async () =>
                {
                    try
                    {
                        var supportDataIngestion = _dataIngestionService.SupportsDataIngestion();
//#if DEBUG
//                        supportDataIngestion = true;
//#endif
                        if (supportDataIngestion)
                        {
                            var jobIds = await DataIngestionMessageDao.GetExecutableMessageAsync(Contract.DataIngestion.RMDataIngestionType.AgentWork);
                            s_logger.Info($"DataIngestionTaskExecutor found {jobIds.Count} jobs for TenantId: {tenantInfo.TenantId} ");
                            if (jobIds.Count > 0)
                            {
                                long timeoutTicks = DateTime.UtcNow.AddHours(-2).Ticks;
                                foreach (var jobId in jobIds)
                                {
                                    var jobInfo = await _dataIngestionService.GetExistingDataIngesionJob(jobId);
                                    if (jobInfo == null)
                                    {
                                        s_logger.Info($"DataIngestionTaskExecutor started for TenantId: {tenantInfo.TenantId}, JobId: {jobId} ");
                                        await _dataIngestionService.ExecuteJobAsync(Contract.DataIngestion.RMDataIngestionType.AgentWork, jobId);
                                        continue;
                                    }
                                    if(jobInfo.ModifiedTime < timeoutTicks)
                                    {
                                        s_logger.Info($"DataIngestionTaskExecutor found a running job for TenantId: {tenantInfo.TenantId}, JobId: {jobId} is running for more than 2 hours, will mark it as failed and start a new one.");
                                        await _jobDao.UpdateStatusAsync(jobId, JobStatus.Timeout, DateTime.UtcNow.Ticks);
                                    }
                                    else
                                    {
                                        s_logger.Info($"Job {jobId} is still running. Skip.");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        s_logger.Error($"DataIngestionTaskExecutor failed for TenantId: {tenantInfo.TenantId}, Error: {ex}");
                    }
                });
            }
        }
    }
}
