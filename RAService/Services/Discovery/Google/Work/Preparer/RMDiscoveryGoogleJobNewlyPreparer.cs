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
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Salesforce;
using AvePoint.RA.DB.Dao.Discovery.Salesforce;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.Google.License;
using AvePoint.RA.Service.Services.Discovery.Salesforce.License;
using Cloud.Sdk.Data.AosModern;

namespace AvePoint.RA.Service.Services.Discovery.Google.Work.Preparer
{
    internal class RMDiscoveryGoogleJobNewlyPreparer() : RMDiscoveryGoogleWorker, IRMDiscoveryGoogleJobPreparable
    {
        private readonly RMDiscoveryGoogleExecutionInfoDao _executionInfoDao = new RMDiscoveryGoogleExecutionInfoDao();

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

                await InitIEDatabaseAsync();
                var licenseType = await RMDiscoveryGoogleLicenseHelper.GetLicenseTypeAsync();

                var (containersCount, driversCount) = licenseType == LicenseType.Trial ? await CalculateTrialNodesCountAsync() : await CalculateDriverNodesCountAsync();

                _logger.Info($"The number of containers to be executed for this Google [{RMDiscoveryJobType.Newly}] job is [{containersCount}], and the number of sites is [{driversCount}].");

                if (containersCount == 0 | driversCount == 0)
                {
                    return (false, I18NEntity.GetString("RM_JM_Report_Skip_NoAvailableDrives"));
                }

                mainJob = new RMDiscoveryGoogleMainJob
                {
                    Id = Guid.NewGuid(),
                    StartTime = DateTime.UtcNow.Ticks,
                    ContainersCount = containersCount,
                    DrivesCount = driversCount,
                    Status = RMDiscoveryJobStatus.Preparing,
                    ProfileJobInitStatus = RMDiscoveryJobStatus.Waiting,
                    Type = RMDiscoveryJobType.Newly,
                    Version = RMDiscoveryJobVersion.V1
                };

                await _jobDao.AddOrUpdateMainJobAsync(mainJob);
                await RMDiscoveryGoogleLicenseHelper.IncreaseConsumedFrequencyPerYearAsync();
                await _executionInfoDao.GenerateByMainJobAsync(mainJob.Id, licenseType);

                _logger.Info($"Discovery Google [{RMDiscoveryJobType.Newly}] job [{mainJob.Id}] is prepared.");

                return (true, string.Empty);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while prepare discovery Google [{RMDiscoveryJobType.Newly}] job.Error: {e}");
                return (false, string.Empty);
            }
        }

        #region Private methods

        private async Task InitIEDatabaseAsync()
        {
            try
            {
                var isInit = await _ieApiClient.SettingService.IsInitializedAsync();
                if (!isInit)
                {
                    await _ieApiClient.SettingService.InitAsync();
                    _logger.Info($"Successful init IE database.");
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while init IE database. Error: {e}");
            }
        }

        private async Task<(int containersCount, int sitesCount)> CalculateDriverNodesCountAsync()
        {
            var scopeInfo = await _configurationDao.GetAsync<RMDiscoveryGoogleScopeInfo>(RMDiscoveryConfigurationType.GoogleNewlyScope);

            _logger.Info($"The scope of this Google [{RMDiscoveryJobType.Newly}] job is [{scopeInfo.ScopeType}].");

            _logger.Info($"The containers affected by this Google [{RMDiscoveryJobType.Newly}] job are [{string.Join(",", scopeInfo.SpecifyContainerIds)}].");

            var needProcessDriverCount = await _nodeDao.CountOpusGoogleDrivesAsync(scopeInfo.SpecifyContainerIds);

            return (scopeInfo.SpecifyContainerIds.Count, needProcessDriverCount);
        }
        
        private async Task<(int containersCount, int sitesCount)> CalculateTrialNodesCountAsync()
        {
            var scopeInfo = await _configurationDao.GetAsync<RMDiscoveryGoogleScopeInfo>(RMDiscoveryConfigurationType.GoogleNewlyScope);

            _logger.Info($"The scope of this [{RMDiscoveryJobType.Newly}] job is [{scopeInfo.ScopeType}].");
            var googleNodes = new List<RMRemoteNode>();            
               var containerIds = scopeInfo.SpecifyContainerIds;
                googleNodes = await _nodeDao.GetOpusTopGoogleDrviesAsync(5, [.. containerIds]);
            var containerCount = googleNodes.Select(item => item.ParentId).ToHashSet().Count;
            var siteCount = googleNodes.Count;
            return (containerCount, siteCount);
        }

        #endregion
    }
}
