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
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Salesforce;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Salesforce.Model;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery.Impl.Salesforce;
using AvePoint.RA.DB.Dao.Discovery.Salesforce;
using AvePoint.RA.DB.Model.Discovery.Salesforce;
using AvePoint.RA.Service.Services.Discovery.Cache;
using AvePoint.RA.Service.Services.Discovery.Salesforce.License;
using RASalesforce;
using RASalesforce.APIs;
using RASalesforce.DataObject;
using JobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus;

namespace AvePoint.RA.Service.Services.Discovery.Salesforce.Work.Analyzer;

public class RMSFSummary : RMSFBaseProcessor
{
    
    private readonly IRMSubJobDao _subJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();
    
    private SalesforceService _salesforceService;

    public override async Task RunAsync()
    {
        var (has, mainJob) = await _jobDao.TryGetMainJobAsync(RMDiscoveryJobStatus.Running);
        try
        {
            while (true)
            {
                var checkOtherSubJobRunning =
                    await _subJobDao.GetOtherSubJobFinishedAsync(SubJobInfo.Id, SubJobInfo.ParentId);
                if (checkOtherSubJobRunning.Any(subJob => subJob.Status is (int)JobStatus.Failed or (int)JobStatus.Stopped ))
                {
                    await SetDiscoveryJob(mainJob, RMDiscoveryJobStatus.Failed);
                    await RMDiscoverySalesforceLicenseHelper.DecreaseConsumedFrequencyPerYearAsync();
                    await ExecutionInfoDao.DeleteByMainJobIdAsync(mainJob.Id);
                    throw new ArgumentException("job stopped or failed ");
                }
                if (checkOtherSubJobRunning.All(subJob => subJob.EndTime != 0))
                {
                    break;
                }

                await Task.Delay(1000);
            }

            var sfScopeInfo =
                await _configurationDao.GetAsync<RMDiscoverySalesforceScopeInfo>(RMDiscoveryConfigurationType.SalesforceNewlyScope);

            long fileTotalSize = 0;
            long recordTotalSize = 0;
            
            foreach (var organization in sfScopeInfo.Organizations)
            {
                try
                {
                    var customerId = TenantLocalValue.LogonGroupId;

                    _salesforceService = new SalesforceService(customerId, organization.Id).Build();

                    var aggregateTotalData = await CreateSummaryTable(organization, await _salesforceService.GetStorageLimitProxyAsync());

                    fileTotalSize += aggregateTotalData?.FileTotalSize ?? 0;
                    recordTotalSize += aggregateTotalData?.DataTotalSize ?? 0;
                    
                    
                    var cacheManager = new RMDiscoveryCacheManager(organization.Id, RMDiscoveryCacheDataSource.Salesforce);
                    await cacheManager.ClearAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to create summary info {organization.Name}, exception:{ex}");
                    ReportCenter.RecordFailedCommon(ReportCenter.GenerateCommonJobDetail(JobType.SFDiscoveryJob, new RMDiscoverySalesforceObjectInfo{ DisplayName = organization.Name}, JobDetailsStatus.Failed, ex.Message));
                    throw;
                }
            }
            

            await ExecutionInfoDao.UpdateFileSizeByMainJobAsync(mainJob.Id, fileTotalSize,recordTotalSize);

            await SetDiscoveryJob(mainJob, RMDiscoveryJobStatus.Finished);
            ReportCenter.SetJobFinish(JobStatus.Finished);
        }
        catch (Exception ex)
        {
            await SetDiscoveryJob(mainJob, RMDiscoveryJobStatus.Failed);
            await RMDiscoverySalesforceLicenseHelper.DecreaseConsumedFrequencyPerYearAsync();
            await ExecutionInfoDao.DeleteByMainJobIdAsync(mainJob.Id);
            _logger.Error($"Operating SF summary exception : {ex.Message}");
            ReportCenter.SetJobFinish(JobStatus.Failed, ex.ToString());
        }
    }

    private async Task<RMDiscoverySalesforceAggregateTotalData> CreateSummaryTable(RMDiscoverySalesforceOrgnization organization, SFStorageLimitProxy storageLimitProxy)
    {
        try
        {
            if (storageLimitProxy != null)
            {
                var aggregateDataDto = await SalesforceDiscoveryJobDao.GetAggregateTotalDataAsync(organization.Id);
                RMDiscoverySalesforceAggregateTotalData aggregateTotalData = new()
                {
                    OrgId = organization.Id,
                    OrgName = organization.Name,
                    DataTotalSize = storageLimitProxy.GetDataStorageTotal() * 1024 * 1024,
                    FileTotalSize = storageLimitProxy.GetFileStorageTotal() * 1024 * 1024,
                    ObjectTotalCount = aggregateDataDto.ObjectTotalCount,
                    OldestRecordsCreatedTime = aggregateDataDto.OldestRecordsCreatedTime,
                    BiggestObjectByDataSize = aggregateDataDto.BiggestObjectByDataSize,
                    BiggestObjectByRecordCount = aggregateDataDto.BiggestObjectByRecordCount,
                    BiggestObjectByFileSize = aggregateDataDto.BiggestObjectByFileSize,
                    RecordsTotalCount = aggregateDataDto.RecordsTotalCount,
                    DataStorageLimit = storageLimitProxy.GetUsedDataStorage(),
                    FileStorageLimit = storageLimitProxy.GetUsedFileStorage()
                };
                await SalesforceDiscoveryJobDao.AddAggregateTotalDataAsync(organization.Id, aggregateTotalData);
                return aggregateTotalData;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex.ToString());
            throw;
        }

        return null;
    }
}