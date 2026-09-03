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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Salesforce;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Salesforce;
using AvePoint.RA.DB.Dao.Discovery.Salesforce;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.Salesforce;
using AvePoint.RA.DB.Model.Salesforce;
using AvePoint.RA.Service.Services.Discovery.Salesforce.License;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.Services.Discovery.Salesforce.Work.Trigger
{
    public class RMDiscoverySalesforceJobTrigger
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoverySalesforceJobTrigger));
        private readonly IRMDiscoverySalesforceJobDao _jobDao = new RMDiscoverySalesforceJobDao();
        private readonly IRMDiscoverySalesforceExecutionInfoDao _executionInfoDao= new RMDiscoverySalesforceExecutionInfoDao();
        private readonly IJobMonitorService _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();
        private readonly IRMSubJobDao _subJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();
        private readonly IRMTenantDiscoveryDBInfoDao _tenantDiscoveryDbDao = new RMTenantDiscoveryDBInfoDao();
        private readonly IJobQueueService _jobQueueService = PlatformWindsorManager.GetService<IJobQueueService>();
        private readonly IRMDiscoveryConfigurationDao _configurationDao = new RMDiscoveryConfigurationDao();

        public async Task TriggerAsync()
        {
            try
            {
                var (has, mainJob) = await _jobDao.TryGetMainJobAsync(RMDiscoveryJobStatus.Preparing);
                if (!has)
                {
                    return;
                }

                _logger.Info($"Start trigger [{mainJob.Type}] salesforce job [{mainJob.Id}].");

                mainJob.Status = RMDiscoveryJobStatus.Pending;
                await _jobDao.AddOrUpdateMainJobAsync(mainJob);

                _logger.Info($"The [{mainJob.Type}] salesforce job [{mainJob.Id}] is set to [{RMDiscoveryJobStatus.Pending}] status.");

                TenantLocalValue.LogonUserEmail = (await _configurationDao.GetAsync<RMDiscoverySalesforceScopeInfo>(RMDiscoveryConfigurationType.SalesforceNewlyScope)).RunJobBy;

                await HandleSalesForceJob(mainJob);

                TenantLocalValue.LogonUserEmail = "";

            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while trigger discovery job. Error: {e}");
            }
        }

        private async Task HandleSalesForceJob(RMDiscoverySalesforceMainJob mainJob)
        {
            IRMDiscoverySalesforceJobTriggerible trigger = mainJob.Type switch
            {
                RMDiscoveryJobType.Newly => new RMDiscoverySalesforceJobNewlyTrigger(),
                RMDiscoveryJobType.Append => new RMDiscoverySalesforceJobAppendTrigger(),
                RMDiscoveryJobType.Retry => new RMDiscoverySalesforceJobRetryTrigger(),
                _ => throw new NotSupportedException(mainJob.Type.ToString()),
            };
            
            var (succeed, objectsByOrganization) = await trigger.GetWillTriggerJobsAsync();
            if (!succeed)
            {
                await SetJobToFailedAsync(mainJob);
                return;
            }

            var initTableSucceed = await trigger.InitTablesAsync(objectsByOrganization.First().Key);
            if (!initTableSucceed)
            {
                await SetJobToFailedAsync(mainJob);
                return;
            }

            var triggerSucceed = await TriggerSalesforceJobAsync(objectsByOrganization, mainJob);
            if (!triggerSucceed)
            {
                await SetJobToFailedAsync(mainJob);
                return;
            }

            var objectsCount = objectsByOrganization.Sum(keyValue => keyValue.Value.Count);
            mainJob.Status = RMDiscoveryJobStatus.Running;
            mainJob.ObjectsCount = objectsCount;
            await _jobDao.AddOrUpdateMainJobAsync(mainJob);
            _logger.Info($"This [{mainJob.Type}] job [{mainJob.Id}] is set to [{RMDiscoveryJobStatus.Running}] status, ObjectsCount:[{mainJob.ObjectsCount}].");
        }
        
        private async Task SetJobToFailedAsync(RMDiscoverySalesforceMainJob mainJob)
        {
            _logger.Info($"Set job [{mainJob.Id}] to failed status due to failed tags registration");
            mainJob.Status = RMDiscoveryJobStatus.Failed;
            mainJob.EndTime = DateTime.UtcNow.Ticks;

            await _jobDao.AddOrUpdateMainJobAsync(mainJob);
            await RMDiscoverySalesforceLicenseHelper.DecreaseConsumedFrequencyPerYearAsync();

            await _executionInfoDao.DeleteByMainJobIdAsync(mainJob.Id);
        }
        
        private async Task<bool> TriggerSalesforceJobAsync(Dictionary<string, List<SfObjectJobDto>> objectsByOrganization, RMDiscoverySalesforceMainJob mainJob)
        {
            var mainJobId = mainJob.Id;
            var jobType = JobType.SFDiscoveryJob;
            try
            {
                var jobId = _jobMonitorService.CreateDiscoveryJobNextVersionAsync(TenantLocalValue.LogonUserEmail, mainJobId, jobType).GetAwaiter().GetResult();
                await SeparateSubJobForSalesforceJob(objectsByOrganization, jobId, jobType);
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while trigger jobs. Error: {e}");
                return false;
            }
        }

        private async Task SeparateSubJobForSalesforceJob(Dictionary<string, List<SfObjectJobDto>> objectsInOneOrganization, string jobId, JobType jobType)
        {
            int subJobRunnable = 2;
            
            var (counter, subJobObjectDic) = AssignObjectsToSubJob(objectsInOneOrganization.SelectMany(organization => organization.Value).ToList());
            
            _subJobDao.UpdateSubJobCount(jobId, counter);

            int currentSubJobIndex = 0;
            foreach (KeyValuePair<int, List<SfObjectJobDto>> dic in subJobObjectDic)
            {
                string subJobId = CreateSubJob(jobId, currentSubJobIndex, jobType, counter, dic.Value, currentSubJobIndex < subJobRunnable);
                _logger.Debug("Create and queue sub job {0}", subJobId);
                if (currentSubJobIndex < subJobRunnable)
                {
                    _logger.Debug("Start sub job {0}", subJobId);
                    _jobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = JobRunBy.Control,
                        JobType = jobType,
                        CommandLine = $"{jobType} {subJobId}",
                    });
                }

                currentSubJobIndex++;
            }
        }

        private static (int, Dictionary<int, List<SfObjectJobDto>>) AssignObjectsToSubJob(List<SfObjectJobDto> objectsInOneOrganization)
        {
            Dictionary<int, List<SfObjectJobDto>> subJobObjectDic = new();
            List<SfObjectJobDto> objectList = [];
            int counter = 0;
            double objectsInOneSubJob = Math.Round(Math.Sqrt(objectsInOneOrganization.Count));
            foreach (var sfObject in objectsInOneOrganization)
            {
                objectList.Add(sfObject);
                if (objectList.Count >= objectsInOneSubJob)
                {
                    counter++;
                    var temp = new List<SfObjectJobDto>();
                    temp.AddRange(objectList);
                    subJobObjectDic.Add(counter, temp);
                    objectList.Clear();
                }
            }
            if (objectList.Count > 0)
            {
                counter++;
                subJobObjectDic.Add(counter, objectList);
            }
            //Additional Job to calculate summary information
            counter++;
            subJobObjectDic.Add(counter, []);
            return (counter, subJobObjectDic);
        }

        private string CreateSubJob(string jobId, int currentSubJobIndex, JobType jobType, int subJobCount, List<SfObjectJobDto> objectList, bool sendNow)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubJobIndex);
            var subJob = new RMSubJob
            {
                Id = subJobId, 
                ParentId = jobId, 
                StartTime = DateTime.UtcNow.Ticks, 
                JobType = (int)jobType, 
                Progress = 0, 
                Status = (int)Contract.RMWeb.JobMonitor.JobStatus.Wait, 
                Weight = 100d / subJobCount,
                Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting,
                JobContext = new RMJobContext
                {
                    JobId = subJobId,
                }
            };
            if (objectList.IsNotNullOrEmpty())
            {
                subJob.JobContext.Content = JsonConvert.SerializeObject(objectList);
            }
            _subJobDao.CreateJob(subJob);
            _logger.Info("Create salesforce sub job {0} sucessfully, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }
    }
}
