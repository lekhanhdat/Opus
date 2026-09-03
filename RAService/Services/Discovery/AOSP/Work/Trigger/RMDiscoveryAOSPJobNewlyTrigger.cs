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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Office365.License;
using Cloud.Sdk.AosModern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery.Model.Configuration.AOSP;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using Cloud.Sdk.Data.AosModern;
using AvePoint.RA.Contract.Tenant;

namespace AvePoint.RA.Service.Services.Discovery.AOSP.Work.Trigger
{
    internal class RMDiscoveryAOSPJobNewlyTrigger : RMDiscoveryAOSPWorker, IRMDiscoveryAOSPJobTriggerible
    {
        private readonly AosModernApiTenantClient _aosApiClient;

        private readonly RMDiscoveryAOSPMainJob _jobInfo;

        internal RMDiscoveryAOSPJobNewlyTrigger(RMDiscoveryAOSPMainJob jobInfo) : base()
        {
            _jobInfo = jobInfo;
            _aosApiClient = AosApiUtility.GetAosModerClient();
        }

        public async Task<(bool succeed, string errorMessage)> InitTablesAsync(List<Guid> o365TenantIds)
        {
            try
            {
                var aosTenantInfoes = await _aosApiClient.TenantManagementService.GetByTypeAsync(PlatformType.Office365);

                var discoveredTenants = await _aospTenantDao.GetAllAsync();
                var discoveredTenantIds = discoveredTenants.Select(item => item.UniqueId).ToHashSet();
                var needDeleteTenantIds = new List<Guid>();

                foreach (var discoveredTenantId in discoveredTenantIds)
                {
                    if (discoveredTenantId == new Guid(_jobInfo.O365TenantId))
                    {
                        await RMDiscoveryDBManager.DropAOSPTablesAsync(discoveredTenantId);
                        needDeleteTenantIds.Add(discoveredTenantId);
                    }
                    else if (!aosTenantInfoes.Any(item => new Guid(item.Id) == discoveredTenantId))
                    {
                        _logger.Info($"The tenant has been deleted from aos, tenant id: [{discoveredTenantId}]");
                        await RMDiscoveryDBManager.DropAOSPTablesAsync(discoveredTenantId);
                        needDeleteTenantIds.Add(discoveredTenantId);
                    }
                }

                foreach (var o365TenantId in o365TenantIds)
                {
                    var inactiveRules = await _ruleInfoDao.GetRuleInfoesAsync(true, o365TenantId.ToString(), RMDiscoveryRuleDefinitionKind.Inactive);
                    var inactiveColumns = inactiveRules.ConvertAll(item => item.ToCustomColumn());

                    await RMDiscoveryDBManager.InitAOSPBasicTablesAsync(o365TenantId);
                    await RMDiscoveryDBManager.InitAOSPRotTablesAsync(o365TenantId);
                    await RMDiscoveryDBManager.InitAOSPInactiveTablesAsync(o365TenantId, inactiveColumns);
                    await RMDiscoveryDBManager.InitAOSPDataOptimizationTablesAsync(o365TenantId);
                    await RMDiscoveryDBManager.InitAOSPProgressReportTablesAsync(o365TenantId);
                }

                await _aospTenantDao.DeleteAsync(needDeleteTenantIds.ConvertAll(id => discoveredTenants.First(item => item.UniqueId == id)).ToArray());

                var needAddOrUpdateTenants = discoveredTenantIds
                    .Except(needDeleteTenantIds)
                    .Union(o365TenantIds)
                    .ToHashSet().ConvertAll(id =>
                    {
                        var discoveredTenant = discoveredTenants.FirstOrDefault(item => item.UniqueId == id);
                        var aosTenantInfo = aosTenantInfoes.FirstOrDefault(item => new Guid(item.Id) == id);
                        if(aosTenantInfo == null)
                        {
                            _logger.Info($"The tenant has been deleted from aos, tenant id: [{id}");
                            return null;
                        }
                        if (discoveredTenant == null)
                        {
                            return new RMDiscoveryAOSPTenantInfo
                            {
                                UniqueId = id,
                                Name = aosTenantInfo.Name,
                                AdminUrl = aosTenantInfo.AdminUrl,
                                Environment = (RMAADEnvironment)aosTenantInfo.AadEnvironment,
                                CreatedTime = DateTime.UtcNow.Ticks,
                                ModifiedTime = DateTime.UtcNow.Ticks
                            };
                        }

                        discoveredTenant.Name = aosTenantInfo.Name;
                        discoveredTenant.AdminUrl = aosTenantInfo.AdminUrl;
                        discoveredTenant.ModifiedTime = DateTime.UtcNow.Ticks;
                        return discoveredTenant;
                    }).Where(item => item != null).ToList();
                    
                await _aospTenantDao.AddOrUpdateAsync(needAddOrUpdateTenants.ToArray());

                var availableO365TenantIds = needAddOrUpdateTenants.Select(item => item.UniqueId).ToList();

                _logger.Info($"The available tenants for this job are [{string.Join(", ", availableO365TenantIds)}]");

                return (true, string.Empty);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while init tables async. Error: {e}");
                return (false, e.Message);
            }
        }

        public async Task<(bool succeed, List<(Guid o365TenantId, RMRemoteNode container, List<RMRemoteNode> sites)> items, string errorMessage)> GetWillTriggerJobsAsync()
        {
            var scopeInfo = await _configurationDao.GetByO365TenantIdAsync<RMDiscoveryAOSPScopeInfo>(RMDiscoveryConfigurationType.AOSPNewlyScope, _jobInfo.O365TenantId);
            return await GetWillTriggerJobsAsync(scopeInfo);
            //var licenseType = await RMDiscoveryOffice365LicenseHelper.GetLicenseTypeAsync();
            //return licenseType == LicenseType.Trial ? await GetTrialWillTriggerJobsAsync(scopeInfo) : await GetWillTriggerJobsAsync(scopeInfo);
        }

        private async Task<(bool succeed, List<(Guid o365TenantId, RMRemoteNode container, List<RMRemoteNode> sites)> items, string errorMessage)> GetWillTriggerJobsAsync(RMDiscoveryAOSPScopeInfo scopeInfo)
        {
            try
            {
                var res = new List<(Guid o365TenantId, RMRemoteNode container, List<RMRemoteNode> sites)>();

                var willTriggerContainers = await GetWillTriggerJobContainers(scopeInfo);
                _logger.Info($"This [{RMDiscoveryJobType.Newly}] job is will execute as containers [{string.Join(", ", willTriggerContainers.Select(item => item.Id))}] of scope [{scopeInfo.ScopeType}].");
                foreach (var willTriggerContainer in willTriggerContainers)
                {
                    var sites = await _nodeDao.GetAOSSitesAsync(_jobInfo.O365TenantId, willTriggerContainer.Id, willTriggerContainer.NodeLevel);
                    if (sites.Count == 0)
                    {
                        _logger.Info($"Container [{willTriggerContainer.Id}] has no sites that need to trigger [{RMDiscoveryJobType.Newly}] job.");
                        continue;
                    }
                    sites = sites.Where(item => !string.IsNullOrWhiteSpace(item.ObjectId)).ToList();
                    var tenantSitesMapping = sites.GroupBy(item => item.TenantId).ToDictionary(item => item.Key, item => item.ToList());
                    foreach (var tenantSites in tenantSitesMapping)
                    {
                        var targetSites = tenantSites.Value.Where(item => !string.IsNullOrEmpty(item.ObjectId) && !item.ObjectId.Equals(Guid.Empty.ToString()))
                                                                    .DistinctBy(item => item.ObjectId).DistinctBy(item => item.Url).ToList();
                        res.Add((new Guid(tenantSites.Key), willTriggerContainer, targetSites));
                    }
                }

                _logger.Info($"Successful allocate will trigger jobs: [{res.Count}].");
                return (true, res, string.Empty);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get will trigger jobs. Error: {e}");
                return (false, [], e.Message);
            }
        }

        private async Task<List<RMRemoteNode>> GetWillTriggerJobContainers(RMDiscoveryAOSPScopeInfo scopeInfo)
        {
            return await _nodeDao.GetAOSContainersAsync(_jobInfo.O365TenantId, [.. scopeInfo.ContentSources]);
        }
    }
}
