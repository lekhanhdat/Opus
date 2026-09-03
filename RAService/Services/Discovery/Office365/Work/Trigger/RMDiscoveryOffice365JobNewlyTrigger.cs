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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Service.Services.Discovery.Office365.License;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common;
using Cloud.Sdk.AosModern;
using Cloud.Sdk.Data.AosModern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Model.Discovery.Office365;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Trigger
{
    internal class RMDiscoveryOffice365JobNewlyTrigger : RMDiscoveryOffice365Worker, IRMDiscoveryOffice365JobTriggerible
    {

        private readonly AosModernApiTenantClient _aosApiClient;

        private readonly RMDiscoveryOffice365MainJob _jobInfo;

        internal RMDiscoveryOffice365JobNewlyTrigger(RMDiscoveryOffice365MainJob jobInfo) : base()
        {
            _jobInfo = jobInfo;
            _aosApiClient = AosApiUtility.GetAosModerClient();
        }

        public async Task<(bool succeed, List<(Guid o365TenantId, RMRemoteNode container, List<RMRemoteNode> sites)> items)> GetWillTriggerJobsAsync()
        {
            var scopeInfo = await _configurationDao.GetAsync<RMDiscoveryOffice365ScopeInfo>(RMDiscoveryConfigurationType.Office365NewlyScope);

            var licenseType = await RMDiscoveryOffice365LicenseHelper.GetLicenseTypeAsync();

            return licenseType == LicenseType.Trial ? await GetTrialWillTriggerJobsAsync(scopeInfo) : await GetWillTriggerJobsAsync(scopeInfo);
        }

        public async Task<bool> InitTablesAsync(List<Guid> o365TenantIds)
        {
            try
            {
                var inactiveEnable = (await _configurationDao.GetAsync<RMDiscoveryOffice365InactiveDefinition>(RMDiscoveryConfigurationType.Office365InactiveDefinition)).Enable;
                var rotEnable = (await _configurationDao.GetAsync<RMDiscoveryOffice365RotDefinition>(RMDiscoveryConfigurationType.Office365ROTDefinition)).Enable;

                var aosTenantInfoes = await _aosApiClient.TenantManagementService.GetByTypeAsync(PlatformType.Office365);

                var discoveredTenants = await _o365TenantDao.GetAllAsync();
                var discoveredTenantIds = discoveredTenants.Select(item => item.UniqueId).ToHashSet();

                foreach (var discoveredTenantId in discoveredTenantIds)
                {
                    await RMDiscoveryDBManager.DropOffice365TablesAsync(discoveredTenantId);
                }

                foreach (var o365TenantId in o365TenantIds)
                {
                    var inactiveRules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.Inactive);
                    var inactiveColumns = inactiveRules.ConvertAll(item => item.ToCustomColumn());

                    if(_jobInfo.Version.IsOffice365NewVersion())
                    {
                        await RMDiscoveryDBManager.InitOffice365BasicTablesV3Async(o365TenantId);
                        await RMDiscoveryDBManager.InitOffice365RotTablesV3Async(o365TenantId);
                        await RMDiscoveryDBManager.InitOffice365InactiveTablesV3Async(o365TenantId, inactiveColumns);
                    }
                    else
                    {
                        await RMDiscoveryDBManager.InitOffice365BasicTablesAsync(o365TenantId);
                        await RMDiscoveryDBManager.InitOffice365RotTablesAsync(o365TenantId);
                        await RMDiscoveryDBManager.InitOffice365InactiveTablesAsync(o365TenantId, inactiveColumns);
                    }

                    await RMDiscoveryDBManager.InitOffice365DataOptimizationTablesAsync(o365TenantId);
                    await RMDiscoveryDBManager.InitOffice365ProgressReportTablesAsync(o365TenantId);
                }

                var needDeleteTenantIds = discoveredTenantIds.Except(o365TenantIds);
                await _o365TenantDao.DeleteAsync(needDeleteTenantIds.ConvertAll(id => discoveredTenants.First(item => item.UniqueId == id)).ToArray());

                var needAddOrUpdateTenants = discoveredTenantIds
                    .Except(needDeleteTenantIds)
                    .Union(o365TenantIds)
                    .ToHashSet().ConvertAll(id =>
                    {
                        var discoveredTenant = discoveredTenants.FirstOrDefault(item => item.UniqueId == id);
                        var aosTenantInfo = aosTenantInfoes.First(item => new Guid(item.Id) == id);
                        if (discoveredTenant == null)
                        {
                            return new RMDiscoveryOffice365TenantInfo
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
                    });
                await _o365TenantDao.AddOrUpdateAsync(needAddOrUpdateTenants.ToArray());

                var availableO365TenantIds = needAddOrUpdateTenants.Select(item => item.UniqueId).ToList();

                _logger.Info($"The available tenants for this job are [{string.Join(", ", availableO365TenantIds)}]");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while init tables async. Error: {e}");
                return false;
            }
        }

        private async Task<(bool succeed, List<(Guid o365TenantId, RMRemoteNode container, List<RMRemoteNode> sites)> items)> GetWillTriggerJobsAsync(RMDiscoveryOffice365ScopeInfo scopeInfo)
        {
            try
            {
                var res = new List<(Guid o365TenantId, RMRemoteNode container, List<RMRemoteNode> sites)>();

                var willTriggerContainers = await GetWillTriggerJobContainers(scopeInfo);
                _logger.Info($"This [{RMDiscoveryJobType.Newly}] job is will execute as containers [{string.Join(", ", willTriggerContainers.Select(item => item.Id))}] of scope [{scopeInfo.ScopeType}].");
                foreach (var willTriggerContainer in willTriggerContainers)
                {
                    var sites = await _nodeDao.GetOpusSitesAsync(new Guid(willTriggerContainer.Id)).ToListAsync();
                    if (!sites.Any())
                    {
                        _logger.Info($"Container [{willTriggerContainer.Id}] has no sites that need to trigger [{RMDiscoveryJobType.Newly}] job.");
                        continue;
                    }
                    var tenantSitesMapping = sites.GroupBy(item => item.TenantId).ToDictionary(item => item.Key, item => item.ToList());
                    foreach (var tenantSites in tenantSitesMapping)
                    {
                        tenantSites.Value.ForEach(item =>
                        {
                            try
                            {
                                if (item.NodeLevel == (int)NodeLevel.O365GroupSites)
                                {
                                    var remoteNode = RABrowserClient.GetRemoteSiteCollectionWithBposByUrl(item.Url);
                                    var factory = MultiAppUtil.CreateAveObjectModelFactory(item.Url, PoolUserUtil.GetAveBPOSAccountInfo(remoteNode.Bpos, item.Url), AveContextKind.ClientObjectModel);
                                    using var site = factory.CreateSite();
                                    item.ObjectId = site.ID.ToString();
                                }
                            }
                            catch (AveSkipLockSiteException e)
                            {
                                _logger.Error($"An error occurred while accessing the site, The site is locked. URL:[{item.Url}] SiteState:[{e.SiteState}]");
                                item.ObjectId = Guid.Empty.ToString();
                            }
                            catch (Exception ex)
                            {
                                _logger.Error($"An error occurred while accessing the site, URL:[{item.Url}], message: {ex}");
                                item.ObjectId = Guid.Empty.ToString();
                            }
                        });

                        var targetSites = tenantSites.Value.Where(item => item.NodeLevel != (int)NodeLevel.O365GroupSites
                                                                    || (item.NodeLevel == (int)NodeLevel.O365GroupSites && !item.ObjectId.Equals(Guid.Empty.ToString()))).ToList();
                        res.Add((new Guid(tenantSites.Key), willTriggerContainer, targetSites));
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

        private async Task<(bool succeed, List<(Guid o365TenantId, RMRemoteNode container, List<RMRemoteNode> sites)> items)> GetTrialWillTriggerJobsAsync(RMDiscoveryOffice365ScopeInfo scopeInfo)
        {
            try
            {
                var res = new List<(Guid o365TenantId, RMRemoteNode container, List<RMRemoteNode> sites)>();

                var siteNodes = new List<RMRemoteNode>();
                if (scopeInfo.ScopeType == RMDiscoveryOffice365ScopeType.Specify)
                {
                    var containerIds = scopeInfo.SpecifyContainerIds;
                    siteNodes = await _nodeDao.GetOpusTopSitesAsync(5, containerIds.ToArray());
                }
                else
                {
                    siteNodes = await _nodeDao.GetOpusTopSitesAsync(5, [..scopeInfo.ContentSources]);
                }

                var willTriggerContainers = await _nodeDao.GetOpusContainersAsync(siteNodes.Select(item => new Guid(item.ParentId)).ToHashSet());
                foreach (var willTriggerContainer in willTriggerContainers)
                {
                    var willTriggerSites = siteNodes.Where(item => item.ParentId == willTriggerContainer.Id).ToList();
                    var tenantSitesMapping = willTriggerSites.GroupBy(item => item.TenantId).ToDictionary(item => item.Key, item => item.ToList());
                    foreach (var tenantSites in tenantSitesMapping)
                    {
                        tenantSites.Value.ForEach(item =>
                        {
                            try
                            {
                                if (item.NodeLevel == (int)NodeLevel.O365GroupSites)
                                {
                                    var remoteNode = RABrowserClient.GetRemoteSiteCollectionWithBposByUrl(item.Url);
                                    var factory = MultiAppUtil.CreateAveObjectModelFactory(item.Url, PoolUserUtil.GetAveBPOSAccountInfo(remoteNode.Bpos, item.Url), AveContextKind.ClientObjectModel);
                                    using var site = factory.CreateSite();
                                    item.ObjectId = site.ID.ToString();
                                }
                            }
                            catch (AveSkipLockSiteException e)
                            {
                                _logger.Error($"An error occurred while accessing the site, The site is locked. URL:[{item.Url}] SiteState:[{e.SiteState}]");
                                item.ObjectId = Guid.Empty.ToString();
                            }
                            catch (Exception ex)
                            {
                                _logger.Error($"An error occurred while accessing the site, URL:[{item.Url}], message: {ex}");
                                item.ObjectId = Guid.Empty.ToString();
                            }
                        });

                        var targetSites = tenantSites.Value.Where(item => item.NodeLevel != (int)NodeLevel.O365GroupSites
                                                                    || (item.NodeLevel == (int)NodeLevel.O365GroupSites && !item.ObjectId.Equals(Guid.Empty.ToString()))).ToList();
                        res.Add((new Guid(tenantSites.Key), willTriggerContainer, targetSites));
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

        private async Task<List<RMRemoteNode>> GetWillTriggerJobContainers(RMDiscoveryOffice365ScopeInfo scopeInfo)
        {
            if (scopeInfo.ScopeType == RMDiscoveryOffice365ScopeType.DataSource)
            {
                return await _nodeDao.GetOpusContainersAsync([..scopeInfo.ContentSources]);
            }

            return await _nodeDao.GetOpusContainersAsync(scopeInfo.SpecifyContainerIds);
        }
    }
}
