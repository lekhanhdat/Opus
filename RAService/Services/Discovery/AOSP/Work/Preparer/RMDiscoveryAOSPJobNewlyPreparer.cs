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
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery.Model.Configuration.AOSP;
using AvePoint.RA.Contract.Explorer;

namespace AvePoint.RA.Service.Services.Discovery.AOSP.Work.Preparer
{
    public class RMDiscoveryAOSPJobNewlyPreparer(bool needToReregisterTags, RMDiscoveryAOSPJobParameter jobParamter) : RMDiscoveryAOSPWorker, IRMDiscoveryAOSPJobPreparable
    {
        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public async Task<(bool success, string errorMessage, Guid jobId)> PrepareAsync()
        {
            try
            {
                await InitIeDatabaseAsync();
                _logger.Info($"Current app profile id is {jobParamter.AppProfileId}");
                var mainJobId = Guid.Empty;
                foreach (var o365TenantId in jobParamter.Office365TenantIds)
                {

                    var (has, mainJob) = await _jobDao.TryGetProcessingMainJobAsync(o365TenantId);
                    if (has)
                    {
                        _logger.Error($"This tenant [{o365TenantId}] is already job [{mainJob.Id}] begin executed.");
                        continue;
                    }

                    var (containerCount, siteCount) = await CalculateAOSPNodesCountAsync(o365TenantId);
                    mainJob = new RMDiscoveryAOSPMainJob
                    {
                        Id = Guid.NewGuid(),
                        StartTime = DateTime.UtcNow.Ticks,
                        ContainersCount = containerCount,
                        SitesCount = siteCount,
                        NeedToReRegisterTags = needToReregisterTags,
                        Status = RMDiscoveryJobStatus.Preparing,
                        ProfileJobInitStatus = RMDiscoveryJobStatus.Waiting,
                        Type = RMDiscoveryJobType.Newly,
                        O365TenantId = o365TenantId,
                        AppProfileId = jobParamter.AppProfileId,
                        //Version = version,
                        Comment = string.Empty
                    };

                    await _jobDao.AddOrUpdateMainJobAsync(mainJob);
                    //await _executionInfoDao.GenerateByMainJobAsync(mainJob.Id, licenseType);
                    //await RMDiscoveryOffice365LicenseHelper.IncreaseConsumedFrequencyPreMonthAsync();
                    mainJobId = mainJob.Id;
                    _logger.Info($"Tenant [{o365TenantId}] discovery [{RMDiscoveryJobType.Newly}] job [{mainJob.Id}] is prepared.");
                }

                return (true, string.Empty, mainJobId);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while prepare discovery [{RMDiscoveryJobType.Newly}] job. Error: {e}");
                return (false, string.Empty, Guid.Empty);
            }
        }

        private async Task InitIeDatabaseAsync()
        {
            try
            {
                var isInit = await _ieApiClient.SettingService.IsInitializedAsync();
                if (!isInit)
                {
                    await _ieApiClient.SettingService.InitAsync();
                    _logger.Info($"Successful init ie database.");
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while init ie database. Error: {e}");
            }
        }

        private async Task<(int containerCount, int siteCount)> CalculateAOSPNodesCountAsync(string o365TenantId)
        {
            var scopeInfo = await _configurationDao.GetByO365TenantIdAsync<RMDiscoveryAOSPScopeInfo>(RMDiscoveryConfigurationType.AOSPNewlyScope, o365TenantId);
            scopeInfo = scopeInfo.CompatibleConvert();
            var availableContentSources = scopeInfo.ContentSources.Count != 0 ? new List<SourceFlag>(scopeInfo.ContentSources) : [SourceFlag.SharePoint, SourceFlag.OneDrive];
            var containerCount = await _nodeDao.CountAOSContainersAsync(o365TenantId, [.. availableContentSources]);
            var siteCount = await _nodeDao.CountAOSSitesAsync(o365TenantId, [.. availableContentSources]);
            _logger.Info($"The scope of this AOSP [{RMDiscoveryJobType.Newly}] job is [{scopeInfo.ScopeType}].");
            return (containerCount, siteCount);
        }
    }
}
