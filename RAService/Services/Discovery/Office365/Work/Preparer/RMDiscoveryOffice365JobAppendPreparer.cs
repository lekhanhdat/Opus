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
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.Office365.License;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Preparer
{
    public class RMDiscoveryOffice365JobAppendPreparer : RMDiscoveryOffice365Worker, IRMDiscoveryOffice365JobPreparable
    {
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

                var licenseType = await RMDiscoveryOffice365LicenseHelper.GetLicenseTypeAsync();
                if(licenseType == Cloud.Sdk.Data.AosModern.LicenseType.Trial)
                {
                    _logger.Error($"Trial license does not support [{RMDiscoveryJobType.Append}] job.");
                    return (false, null);
                }

                var (containersCount, sitesCount) = await CalculateNodesCountAsync();
                if(containersCount == 0 || sitesCount == 0)
                {
                    return (false, I18NEntity.GetString("RM_FA_DiscoveryJob_NoSite"));
                }

                (has, mainJob) = await _jobDao.TryGetLatestMainJobAsync(RMDiscoveryJobType.Newly);
                if(!has)
                {
                    _logger.Error($"No [{RMDiscoveryJobType.Newly}] job found.");
                    return (false, null);
                }

                var appendJob = new RMDiscoveryOffice365MainJob
                {
                    Id = Guid.NewGuid(),
                    StartTime = DateTime.UtcNow.Ticks,
                    ContainersCount = containersCount,
                    SitesCount = sitesCount,
                    NeedToReRegisterTags = true,
                    Status = RMDiscoveryJobStatus.Preparing,
                    ProfileJobInitStatus = mainJob.Version.ToOffice365ProfileJobInitStatus(),
                    Type = RMDiscoveryJobType.Append,
                    ParentId = mainJob.Id,
                    MainJobId = mainJob.Id,
                    Version = mainJob.Version.ToOffice365AppendJobVersion(),
                };

                await _jobDao.AddOrUpdateMainJobAsync(appendJob);
                await _executionInfoDao.GenerateByMainJobAsync(appendJob.Id, licenseType);
                await RMDiscoveryOffice365LicenseHelper.IncreaseConsumedFrequencyPreMonthAsync();
                _logger.Info($"Discovery [{RMDiscoveryJobType.Append}] job [{mainJob.Id}] is prepared.");

                return (true, string.Empty);
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while prepare discovery [{RMDiscoveryJobType.Append}] job. Error: {e}");
                return (false, string.Empty);
            }
        }

        private async Task<(int containersCount, int sitesCount)> CalculateNodesCountAsync()
        {
            var scopeInfo = await _configurationDao.GetAsync<RMDiscoveryOffice365ScopeInfo>(RMDiscoveryConfigurationType.Office365AppendScope);

            _logger.Info($"The containers affected by this [{RMDiscoveryJobType.Append}] job are [{string.Join(",", scopeInfo.SpecifyContainerIds)}].");

            var needProcessSiteCount = await _nodeDao.CountOpusSitesAsync(scopeInfo.SpecifyContainerIds);
            return (scopeInfo.SpecifyContainerIds.Count, needProcessSiteCount);
        }
    }
}
