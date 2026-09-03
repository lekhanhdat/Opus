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
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Salesforce;
using AvePoint.RA.DB.Dao.Discovery.Salesforce;

namespace AvePoint.RA.Service.Services.Discovery.Salesforce;

public class RMDiscoverySalesforceJobManagementService : IRMDiscoverySalesforceJobManagementService
{
    private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoverySalesforceJobManagementService));

    private readonly IRMDiscoverySalesforceJobDao _jobDao = new RMDiscoverySalesforceJobDao();

    private readonly IRMTenantDiscoveryDBInfoDao _tenantInfoDao = new RMTenantDiscoveryDBInfoDao();

    private readonly IGeneralSettingService _generalSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();
    
    private readonly IRMDiscoverySalesforceDataQueryService _dataQueryService = PlatformWindsorManager.GetService<IRMDiscoverySalesforceDataQueryService>();

    
    public async Task<RMDiscoveryLatestJobInfo> GetLatestAsync()
    {
        try
        {
            if (!await _tenantInfoDao.IsInitTenantDiscoveryDBInfoAsync() || !await RMDiscoveryDBManager.CheckSalesforceTablesExistsAsync())
            {
                return new()
                {
                    Status = RMDiscoveryJobStatus.None,
                    EnableRot = false,
                };
            }

            var (has, jobInfo) = await _jobDao.TryGetLatestMainJobAsync();
            if (!has)
            {
                return new()
                {
                    Status = RMDiscoveryJobStatus.None,
                    EnableRot = false,
                };
            }
            
            var jobStatus = jobInfo.Status switch
            {
                RMDiscoveryJobStatus.Finished or RMDiscoveryJobStatus.Failed or RMDiscoveryJobStatus.Exception or RMDiscoveryJobStatus.Stopped=>
                    jobInfo.Status,
                _ => RMDiscoveryJobStatus.Running
            };
            var finishedCount = jobInfo.Status switch
            {
                RMDiscoveryJobStatus.Preparing => 0,
                _ => await _dataQueryService.GetSalesforceObjects()
            };


            return new RMDiscoveryLatestJobInfo
            {
                HasJob = true,
                Status = jobStatus,
                JobType = jobInfo.Type,
                SiteProgressInfo = new RMDiscoveryJobSiteProgressInfo
                {
                    NeedProcessCount = jobInfo.ObjectsCount,
                    SucceedCount = finishedCount, 
                    DiscoveredCount =finishedCount,
                },
                StartTime =
                    (await _generalSettingService.ConvertTiksToDateTimeAsync(jobInfo.StartTime, true)).FormaTime,
                EndTime = jobInfo.EndTime == 0
                    ? "0"
                    : (await _generalSettingService.ConvertTiksToDateTimeAsync(jobInfo.EndTime, true))
                    .SimplifyFormatTime,
                EndTimeLong = jobInfo.EndTime,
                Version = jobInfo.Version,
                EnableRot = false,
            };
        }
        catch (Exception e)
        {
            _logger.Error($"An error occured while get salesforce job progress info. Error: {e}");
            return new()
            {
                Status = RMDiscoveryJobStatus.None,
                EnableRot = false,
            };
        }
    }
}