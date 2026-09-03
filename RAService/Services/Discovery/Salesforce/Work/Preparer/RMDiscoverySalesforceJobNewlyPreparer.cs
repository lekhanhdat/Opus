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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Salesforce;
using AvePoint.RA.DB.Dao.Discovery.Salesforce;
using AvePoint.RA.DB.Model.Discovery.Salesforce;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.Salesforce.License;
using RASalesforce;
using RASalesforce.DataObject;
using RASalesforce.Util;

namespace AvePoint.RA.Service.Services.Discovery.Salesforce.Work.Preparer
{
    public class RMDiscoverySalesforceJobNewlyPreparer(List<string> organizationIds) : IRMDiscoverySalesforceJobPreparable
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoverySalesforceJobNewlyPreparer));

        private readonly IRMDiscoverySalesforceExecutionInfoDao _executionInfoDao = new RMDiscoverySalesforceExecutionInfoDao();
        
        private readonly IRMDiscoverySalesforceJobDao _jobDao = new RMDiscoverySalesforceJobDao();

        public async Task<(bool success, string errorMessage)> PrepareAsync()
        {
            try
            {
                var (has, mainJob) = await _jobDao.TryGetProcessingMainJobAsync();
                if (has)
                {
                    _logger.Error($"There is already job [{mainJob.Id}] begin executed.");
                    return (false, I18NEntity.GetString("RM_FA_DiscoveryJob_HasRunningJob"));
                }
                
                if(!await CheckObjectExistAsync(organizationIds))
                {
                    return (false, I18NEntity.GetString("RM_FA_DiscoveryJob_NoSite"));
                }
                
                var licenseType = await RMDiscoverySalesforceLicenseHelper.GetLicenseTypeAsync();

                int sObjectCount = 0;
                foreach (var sfOrganizationId in organizationIds)
                {
                    var customerId = TenantLocalValue.LogonGroupId;
                    var sfService = new SalesforceService(customerId, sfOrganizationId).Build();

                    sObjectCount += (await sfService.GetSalesforceObjectsAsync()).Count(RMSFObjectUtil.IsStorageObject);
                }

                mainJob = new RMDiscoverySalesforceMainJob
                {
                    Id = Guid.NewGuid(),
                    StartTime = DateTime.UtcNow.Ticks,
                    Status = RMDiscoveryJobStatus.Preparing,
                    ObjectsCount = sObjectCount,
                    Type = RMDiscoveryJobType.Newly,
                    Version = RMDiscoveryJobVersion.V1,
                };

                await _jobDao.AddOrUpdateMainJobAsync(mainJob);
                await RMDiscoverySalesforceLicenseHelper.IncreaseConsumedFrequencyPerYearAsync();
                await _executionInfoDao.GenerateByMainJobAsync(mainJob.Id, licenseType);

                _logger.Info($"Salesforce discovery [{RMDiscoveryJobType.Newly}] job [{mainJob.Id}] is prepared.");

                return (true, string.Empty);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while prepare discovery [{RMDiscoveryJobType.Newly}] job. Error: {e}");
                return (false, string.Empty);
            }
        }
        
        private async Task<bool> CheckObjectExistAsync(List<string> organizationIds)
        {
            foreach (var organizationId in organizationIds)
            {
                var sfService = new SalesforceService(TenantLocalValue.LogonGroupId, organizationId).Build();
                List<SFObjectProxy> sfObjects = await sfService.GetSalesforceObjectsAsync();
                if (sfObjects.Count > 0) return true;
            }
            return false;
        }
    }
}
