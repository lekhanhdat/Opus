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
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.Office365.License;
using Microsoft.InformationProtection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Preparer
{
    public class RMDiscoveryOffice365JobNewlyPreparer(bool needToReregisterTags) : RMDiscoveryOffice365Worker, IRMDiscoveryOffice365JobPreparable
    {
        private const string S_DISCOVERY_JOB_VERSION_KEY = "DISCOVERY_JOB_VERSION";

        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private readonly IGlobalKeyValueService GlobalKeyValueService = PlatformWindsorManager.GetService<IGlobalKeyValueService>();

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

                await InitIeDatabaseAsync();

                var licenseType = await RMDiscoveryOffice365LicenseHelper.GetLicenseTypeAsync();

                var (containersCount, siteCount) = licenseType == Cloud.Sdk.Data.AosModern.LicenseType.Trial ? await CalculateTrialNodesCountAsync() : await CalculateNodesCountAsync();
                _logger.Info($"The number of containers to be executed for this [{RMDiscoveryJobType.Newly}] job is [{containersCount}], and the number of sites is [{siteCount}].");

                if (containersCount == 0 | siteCount == 0)
                {
                    return (false, I18NEntity.GetString("RM_FA_DiscoveryJob_NoSite"));
                }

                var version = RMDiscoveryJobVersion.V4;

                bool isUseTenantKey = false;
                var setting = _keyValueDao.GetValueByKey(S_DISCOVERY_JOB_VERSION_KEY);
                if (setting != null && !string.IsNullOrWhiteSpace(setting.Value) && int.TryParse(setting.Value, out var versionInt))
                {
                    _logger.Info($"The discovery job version is [{versionInt}] from tenant key.");
                    version = (RMDiscoveryJobVersion)versionInt;
                    isUseTenantKey = true;
                }

                if (!isUseTenantKey)
                {
                    var key = $"{GlobalValueKey.CUSTOM_SETTING}{RMGlobalNameValueDto.Seprator}{RMGlobalNameValueType.GlobalCustomSetting}";

                    var customSettingValue = GlobalKeyValueService.Get(key);
                    if (customSettingValue != null)
                    {
                        var globalConfigs = JsonConvert.DeserializeObject<List<RMGlobalConfigDto>>(customSettingValue?.Value);
                        var globalDiscoveryVersion = globalConfigs?.FirstOrDefault(x => x.Key == GlobalValueKey.DISCOVERY_VERSION_KEY);
                        if (globalDiscoveryVersion != null && int.TryParse(globalDiscoveryVersion.Value, out var globalDiscoveryVersionInt))
                        {
                            _logger.Info($"The discovery job version is [{globalDiscoveryVersionInt}] from global key.");
                            version = (RMDiscoveryJobVersion)globalDiscoveryVersionInt;
                        }
                    }
                }
                
                mainJob = new RMDiscoveryOffice365MainJob
                {
                    Id = Guid.NewGuid(),
                    StartTime = DateTime.UtcNow.Ticks,
                    ContainersCount = containersCount,
                    SitesCount = siteCount,
                    NeedToReRegisterTags = needToReregisterTags,
                    Status = RMDiscoveryJobStatus.Preparing,
                    ProfileJobInitStatus = version.ToOffice365ProfileJobInitStatus(),
                    Type = RMDiscoveryJobType.Newly,
                    Version = version,
                };

                await _jobDao.AddOrUpdateMainJobAsync(mainJob);
                await _executionInfoDao.GenerateByMainJobAsync(mainJob.Id, licenseType);
                await RMDiscoveryOffice365LicenseHelper.IncreaseConsumedFrequencyPreMonthAsync();
                _logger.Info($"Discovery [{RMDiscoveryJobType.Newly}] job [{mainJob.Id}] is prepared.");

                return (true, string.Empty);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while prepare discovery [{RMDiscoveryJobType.Newly}] job. Error: {e}");
                return (false, string.Empty);
            }
        }

        private async Task<(int containersCount, int sitesCount)> CalculateNodesCountAsync()
        {
            var scopeInfo = await _configurationDao.GetAsync<RMDiscoveryOffice365ScopeInfo>(RMDiscoveryConfigurationType.Office365NewlyScope);
            scopeInfo = scopeInfo.CompatibleConvert();

            _logger.Info($"The scope of this [{RMDiscoveryJobType.Newly}] job is [{scopeInfo.ScopeType}].");

            if (scopeInfo.ScopeType == RMDiscoveryOffice365ScopeType.DataSource)
            {
                var containerCount = await _nodeDao.CountOpusContainersAsync([..scopeInfo.ContentSources]);
                var siteCount = await _nodeDao.CountOpusSitesAsync([.. scopeInfo.ContentSources]);
                return (containerCount, siteCount);
            }

            _logger.Info($"The containers affected by this [{RMDiscoveryJobType.Newly}] job are [{string.Join(",", scopeInfo.SpecifyContainerIds)}].");

            var needProcessSiteCount = await _nodeDao.CountOpusSitesAsync(scopeInfo.SpecifyContainerIds);
            return (scopeInfo.SpecifyContainerIds.Count, needProcessSiteCount);
        }

        private async Task<(int containersCount, int sitesCount)> CalculateTrialNodesCountAsync()
        {
            var scopeInfo = await _configurationDao.GetAsync<RMDiscoveryOffice365ScopeInfo>(RMDiscoveryConfigurationType.Office365NewlyScope);
            scopeInfo = scopeInfo.CompatibleConvert();

            _logger.Info($"The scope of this [{RMDiscoveryJobType.Newly}] job is [{scopeInfo.ScopeType}].");
            var siteNodes = new List<RMRemoteNode>();
            if (scopeInfo.ScopeType == RMDiscoveryOffice365ScopeType.Specify)
            {
                var containerIds = scopeInfo.SpecifyContainerIds;
                siteNodes = await _nodeDao.GetOpusTopSitesAsync(5, [.. containerIds]);
            }
            else
            {
                siteNodes = await _nodeDao.GetOpusTopSitesAsync(5, [.. scopeInfo.ContentSources]);
            }

            var containerCount = siteNodes.Select(item => item.ParentId).ToHashSet().Count;
            var siteCount = siteNodes.Count;
            return (containerCount, siteCount);
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
    }
}
