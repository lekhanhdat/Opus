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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Google;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Service.Services.Discovery.Google.License;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common;
using Cloud.Sdk.AosModern;
using Cloud.Sdk.Data.AosModern;

namespace AvePoint.RA.Service.Services.Discovery.Google.Work.Trigger
{
    public class RMDiscoveryGoogleJobNewlyTrigger : RMDiscoveryGoogleWorker, IRMDiscoveryGoogleJobTriggerible
    {
        private readonly AosModernApiTenantClient _aosApiClient;

        private readonly RMDiscoveryGoogleMainJob _jobInfo;

        internal RMDiscoveryGoogleJobNewlyTrigger(RMDiscoveryGoogleMainJob jobInfo) : base()
        {
            _jobInfo = jobInfo;
            _aosApiClient = AosApiUtility.GetAosModerClient();
        }

        public async Task<(bool succeed, List<(string googleOrganizationId, RMRemoteNode container, List<RMRemoteNode> drives)> items)> GetWillTriggerJobsWrapperAsync()
        {
            var scopeInfo = await _configurationDao.GetAsync<RMDiscoveryGoogleScopeInfo>(RMDiscoveryConfigurationType.GoogleNewlyScope);
            
            var licenseType = await RMDiscoveryGoogleLicenseHelper.GetLicenseTypeAsync();
            
            return licenseType == LicenseType.Trial ? await GetTrialWillTriggerJobsAsync(scopeInfo) : await GetWillTriggerJobsAsync(scopeInfo);
        }

        public async Task<(bool succeed, List<(string googleOrganizationId, RMRemoteNode container, List<RMRemoteNode> drives)> items)> GetWillTriggerJobsAsync(RMDiscoveryGoogleScopeInfo scopeInfo)
        {
            try
            {
                var res = new List<(string googleOrganizationId, RMRemoteNode container, List<RMRemoteNode> drives)>();

                var willTriggerContainers = await GetWillTriggerJobContainers(scopeInfo.SpecifyContainerIds);
                _logger.Info($"This [{RMDiscoveryJobType.Newly}] job is will execute as containers [{string.Join(", ", willTriggerContainers.Select(item => item.Id))}] of scope [{scopeInfo.ScopeType}].");
                foreach (var willTriggerContainer in willTriggerContainers)
                {
                    var drives = await _nodeDao.GetOpusGoogleDrivesAsync(new Guid(willTriggerContainer.Id)).ToListAsync();
                    if (!drives.Any())
                    {
                        _logger.Info($"Container [{willTriggerContainer.Id}] has no drives that need to trigger [{RMDiscoveryJobType.Newly}] job.");
                        continue;
                    }
                    var tenantDrivesMapping = drives.GroupBy(item => item.TenantId).ToDictionary(item => item.Key, item => item.ToList());
                    foreach (var tenantDrives in tenantDrivesMapping)
                    {
                        var targetDrives = tenantDrives.Value.Where(item => !item.ObjectId.Equals(Guid.Empty.ToString())).ToList();
                        res.Add((tenantDrives.Key, willTriggerContainer, targetDrives));
                        _logger.Info($"Google tenant [{tenantDrives.Key}] has [{targetDrives.Count} drives.] ");
                    }
                }
                _logger.Info($"Successful allocate will trigger jobs: [{res.Count}].");
                return (true, res);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get will trigger jobs. Error: {e}");
                return (false, []);
            }
        }

        public async Task<bool> InitTablesAsync(List<string> googleOrganizationIds)
        {
            try
            {
                var rotEnable = (await _configurationDao.GetAsync<RMDiscoveryGoogleRotDefinition>(RMDiscoveryConfigurationType.GoogleROTDefinition)).Enable;
                var aosGoogleTenantInfoes = await _aosApiClient.TenantManagementService.GetByTypeAsync(PlatformType.Google);

                var discoveredTenants = await _organizationDao.GetAllAsync();
                var discoveredTenantIds = discoveredTenants.Select(item => item.OrganizationId).ToHashSet();

                foreach (var discoveredTenantId in discoveredTenantIds)
                {
                    await RMDiscoveryDBManager.DropGoogleTablesAsync(discoveredTenantId);
                }

                foreach (var organizationId in googleOrganizationIds)
                {
                    await RMDiscoveryDBManager.InitGoogleBasicTablesAsync(organizationId);
                    await RMDiscoveryDBManager.InitGoogleRotTablesAsync(organizationId);
                    await RMDiscoveryDBManager.InitGoogleInactiveTablesAsync(organizationId);
                }

                var needDeleteTenantIds = discoveredTenantIds.Except(googleOrganizationIds);
                await _organizationDao.DeleteAsync(needDeleteTenantIds.ConvertAll(id => discoveredTenants.First(item => item.OrganizationId.Equals(id))).ToArray());

                var needAddOrUpdateTenants = discoveredTenantIds
                    .Except(needDeleteTenantIds)
                    .Union(googleOrganizationIds)
                    .ToHashSet().ConvertAll(id =>
                    {
                        var discoveredTenant = discoveredTenants.FirstOrDefault(item => item.OrganizationId == id);
                        var aosTenantInfo = aosGoogleTenantInfoes.First(item => item.Id.Equals(id));
                        if (discoveredTenant == null)
                        {
                            return new RMDiscoveryGoogleOrganizationInfo
                            {
                                OrganizationId = id,
                                Name = aosTenantInfo.Name,
                                CreatedTime = DateTime.UtcNow.Ticks,
                                ModifiedTime = DateTime.UtcNow.Ticks
                            };
                        }

                        discoveredTenant.Name = aosTenantInfo.Name;
                        discoveredTenant.ModifiedTime = DateTime.UtcNow.Ticks;
                        return discoveredTenant;
                    });
                await _organizationDao.AddOrUpdateAsync(needAddOrUpdateTenants.ToArray());

                _logger.Info($"The newly google organizations for this job are [{string.Join(", ", googleOrganizationIds)}]");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while init tables async. Error: {e}");
                return false;
            }
        }

        private async Task<(bool succeed, List<(string googleOrganizationId, RMRemoteNode container, List<RMRemoteNode> drives)> items)> GetTrialWillTriggerJobsAsync(RMDiscoveryGoogleScopeInfo scopeInfo)
        {
            try
            {
                var res = new List<(string googleOrganizationId, RMRemoteNode container, List<RMRemoteNode> drives)>();

                var containerIds = scopeInfo.SpecifyContainerIds;
                var googleNodes = await _nodeDao.GetOpusTopGoogleDrviesAsync(5, containerIds.ToArray());

                var willTriggerContainers =
                    await _nodeDao.GetOpusGoogleContainersAsync(googleNodes.Select(item => new Guid(item.ParentId))
                        .ToHashSet());
                foreach (var willTriggerContainer in willTriggerContainers)
                {
                    var willTriggerDrives = googleNodes.Where(item => item.ParentId == willTriggerContainer.Id).ToList();
                    var tenantDrivesMapping = willTriggerDrives.GroupBy(item => item.TenantId)
                        .ToDictionary(item => item.Key, item => item.ToList());
                    foreach (var tenantDrives in tenantDrivesMapping)
                    {

                        var targetDrives = tenantDrives.Value.Where(item => !item.ObjectId.Equals(Guid.Empty.ToString())).ToList();
                        res.Add((tenantDrives.Key, willTriggerContainer, targetDrives));
                        _logger.Info($"Google tenant [{tenantDrives.Key}] has [{targetDrives.Count} drives.] ");
                    }
                }

                _logger.Info($"Successful allocate will trigger jobs: [{res.Count}].");
                return (true, res);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get will trigger jobs. Error: {e}");
                return (false, []);
            }
        }


        #region Private methods
        private async Task<List<RMRemoteNode>> GetWillTriggerJobContainers(List<Guid> containerIds)
        {
            return await _nodeDao.GetOpusGoogleContainersAsync(containerIds);
        }
        #endregion
    }
}
