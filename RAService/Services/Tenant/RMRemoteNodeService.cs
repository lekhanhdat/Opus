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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.TransientFault;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Discovery.Model.PlanProfile;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.PlanProfile;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.AccountManager;
using Cloud.Sdk.Data.Aos;
using Cloud.Sdk.Data.Aos.Tenant;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JobType = AvePoint.RA.Contract.JobMonitor.JobType;

namespace AvePoint.RA.Service.Services.Tenant
{
    public class RMRemoteNodeService : RMServiceBase, IRMRemoteNodeService
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMRemoteNodeService));
        private static readonly AveRetryPolicy RetryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(3, TimeSpan.FromSeconds(10)));
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();

        private IRMAOSNotificationService AOSNotificationService => PlatformWindsorManager.GetService<IRMAOSNotificationService>();
        private IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private ITenantInfoDao TenantInfoDao => PlatformWindsorManager.GetService<ITenantInfoDao>();
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private IMultiGeoDataCenterService _multiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        private IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMFunctionSettingDao _functionSettingDao = PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
        private IRMDiscoveryPlanSiteMappingDao _planSiteMappingDao = PlatformWindsorManager.GetService<IRMDiscoveryPlanSiteMappingDao>();
        #region Sync Nodes Job
        public string CreateSyncAllNodesJob()
        {
            if (_functionSettingDao.IsEnableMultiGeoFeature(_keyValueDao).Result && !_multiGeoDataCenterService.IsMainDC())
            {
                logger.Info($"Multi-geo feature is enabled and this is not the main DC, skipping sync node jobs.");
                return string.Empty;
            }
            var tenantGroupId = TenantLocalValue.LogonGroupId;
            logger.Info($"Begin create init remote nodes job for tenant: {tenantGroupId}.");
            try
            {
                logger.Info($"Clear existing incremental sync node messages.");
                AOSNotificationService.DeleteAll(tenantGroupId);
                var o365TenantGroupIds = RMAosApiClient.GetO365TenantIds(tenantGroupId);
                if (o365TenantGroupIds == null || o365TenantGroupIds.Count == 0)
                {
                    logger.Info($"No o365 tenant info in tenant group: {tenantGroupId}.");
                }
                else
                {
                    logger.Info($"Add init sync node messages for O365 tenants: {string.Join(", ", o365TenantGroupIds)}");
                    foreach (string o365TenantGroupId in o365TenantGroupIds)
                    {
                        AOSNotificationService.Add(GenerateFakeQueueMessage(o365TenantGroupId, tenantGroupId));
                    }
                }

                var result = CreateSyncNodesJob(true);
                TenantInfoDao.UpdateSyncNodeState(tenantGroupId, RMInitNodeState.Syncing);
                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while creating sync nodes job for tenant: {tenantGroupId}. {ex}");
            }
            return null;
        }

        private RMAosQueueMessage GenerateFakeQueueMessage(string o365TenantGroupId, string tenantGroupId)
        {
            var licenseInfo = LicenseUnitType.None | LicenseUnitType.Exchange | LicenseUnitType.AvePointRecords;
            return new RMAosQueueMessage()
            {
                TenantGroupId = tenantGroupId,
                QueueMessageId = Guid.NewGuid().ToString(),
                SyncNodesMessage = new SyncNodesMessage()
                {
                    Content = new RemoteNodesMessage()
                    {
                        Office365TenantId = o365TenantGroupId,
                        DocAveLicenseInfo = (long)licenseInfo,
                        IsManualScan = false,
                    },
                },
                MessageType = RMAosQueueMessageType.InitNodes
            };
        }

        public string CreateSyncNodesJob(bool isInitJob = false)
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var jobRunBy = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.SyncNodesFromAOS,
                    JobRunType = JobRunBy.Schedule,
                    TenantGroupId = groupId,
                    JobRunByUser = isInitJob ? jobRunBy : "RM_TS_RunSchedule",
                    Parameters = isInitJob.ToString()
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while create sync nodes job, ERROR: {0}", ex.ToString());
            }

            return id;
        }

        public string RealRunSyncNodesJob(JobQueueDto jqDto)
        {
            string jobId = JobMonitorService.CreateJob(JobType.SyncNodesFromAOS, "RM_TS_RunSchedule");
            logger.Info($"Real create sync nodes job: {jobId}");

            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = jobId,
                JobType = JobType.SyncNodesFromAOS,
                CommandLine = string.Format("{0} {1}", JobType.SyncNodesFromAOS, jobId),
                Extension = jqDto.Parameters
            });

            //AOSNotificationService.IncrementRunningSRNJobCount(TenantLocalValue.LogonGroupId);
            return jobId;
        }
        #endregion

        public List<RemoteSiteCollection> GetRemoteSiteCollectionByParam(List<string> param, bool isUrl = true)
        {
            return RemoteNodeDao.GetRemoteSiteCollectionByParam(param, isUrl);
        }
        public void DeleteRemoteWebApplication(List<string> ids)
        {
            RemoteNodeDao.DeleteRemoteWebApplication(ids);
        }

        public void DeleteRemoteSiteCollectionsByUrl(List<string> urls)
        {
            Action<IEnumerable<string>> action = this.RemoteNodeDao.DeleteRemoteSiteCollectionsByUrl;
            DatabaseUtility.BatchOperation(urls, action);
        }

        public void DeleteRemoteSiteCollectionByParentId(List<string> parentIds)
        {
            RemoteNodeDao.DeleteRemoteSiteCollectionByParentId(parentIds);
        }

        public List<string> GetO365GroupSiteUrlsByNames(List<string> names)
        {
            return RemoteNodeDao.GetO365GroupSiteUrlsByNames(names);
        }

        public void SyncRemoteSiteCollections(List<RemoteSiteCollection> siteCollections)
        {
            // AOS中的ServiceAccount会单独同步到DAO
            //DatabaseEncrypt(siteCollections);
            RemoteNodeDao.CreateRemoteSiteCollectionsByCurrentGroupId(siteCollections);
        }

        public void CreateRemoteWebApplications(List<RemoteWebApplication> webApps)
        {
            this.RemoteNodeDao.CreateRemoteWebApplications(webApps);
        }

        public void UpdateRemoteWebApplications(List<RemoteWebApplication> webApplications)
        {
            RemoteNodeDao.UpdateRemoteWebApplications(webApplications);
        }

        public List<RemoteNodePara> GetRemoteWebApplicationNodes()
        {
            return this.RemoteNodeDao.GetRemoteWebApplicationNodes();
        }

        public Dictionary<string, string> GetContainerNameBySiteUrls(IEnumerable<string> urls)
        {
            Dictionary<string, string> results = new Dictionary<string, string>();
            DatabaseUtility.BatchOperation(urls, (batchUrls) =>
            {
                results.AddRangeInternal(RemoteNodeDao.GetContainerNameBySiteUrls(batchUrls), true);
            });
            return results;
        }

        public List<SyncRemoteNodePara> GetAllSiteCollectionNodesByPage(int pageIndex, int pageSize)
        {
            return this.RemoteNodeDao.GetAllSiteCollectionNodesByPage(pageIndex, pageSize);
        }

        public int GetRemoteNodesCount()
        {
            return this.RemoteNodeDao.GetRemoteNodesCount();
        }

        public void UpdateSyncSiteCollections(List<SyncRemoteNodePara> siteCollections)
        {
            RemoteNodeDao.UpdateSyncSiteCollections(siteCollections);
        }

        public List<RemoteSiteCollection> GetRemoteSiteCollectionBySiteUrls(List<string> urls)
        {
            if (urls == null || urls.Count == 0)
            {
                return new List<RemoteSiteCollection>();
            }

            Func<List<string>, List<RemoteSiteCollection>> func = RemoteNodeDao.GetRemoteSiteCollectionBySiteUrls;
            var siteCollections = DatabaseUtility.ExecuteOnBatch(urls, func);
            InitAccount(siteCollections);//RECO-11012
            return siteCollections;
        }

        private void DatabaseDecrypt(RemoteSiteCollection site)
        {
            if (site == null)
            {
                return;
            }
            DatabaseDecrypt(new List<RemoteSiteCollection> { site });
        }
        private void InitAccount(List<RemoteSiteCollection> sites)
        {
            if (sites == null || sites.Count == 0)
            {
                return;
            }
            var accounts = RMAosApiClient.GetServiceAccounts(TenantLocalValue.LogonGroupId);
            logger.Info($"The tenant: [{TenantLocalValue.LogonGroupId}] get service accounts: [{accounts.Count}] from aos.");
            foreach (var site in sites)
            {
                if (site == null)
                {
                    logger.Warn($"Can't find site.");
                    continue;
                }
                if (string.IsNullOrEmpty(site.ServiceAccountId))
                {
                    logger.Warn($"The [{site.url}] service account id is null or empty.");
                    continue;
                }
                var account = accounts.FirstOrDefault(a => HashCodeHelper.ToMD5HashCode(a.UserName.ToLowerInvariant()) == site.ServiceAccountId);

                if (account != null)
                {
                    var adminUrl = RMAosApiClient.GetO365TenantInfoByIdAsync(account.TenantId).GetAwaiter().GetResult().AdminUrl;

                    site.username = account.UserName;
                    site.TenantId = account.TenantId;
                    site.AdminUrl = adminUrl;
                }
            }
        }
        /// <summary>
        /// TO DO Review why use this pwd,need discuss how to remove get pwd by default
        /// </summary>
        /// <param name="sites"></param>
        private void DatabaseDecrypt(List<RemoteSiteCollection> sites)
        {     
            if (sites == null)
            {
                return;
            }
            sites = sites.Where(site => site.AuthType == GCommon.Contract.CentralAdmin.Object.BposConnectionType.ServiceAccount).ToList();
            if(sites.Count == 0)
            {
                return;
            }
            List<Cloud.Sdk.Data.AosModern.ServiceAccount> accounts = new List<Cloud.Sdk.Data.AosModern.ServiceAccount>();
            try
            {
                accounts = RMAosApiClient.GetServiceAccountsWithPassword(TenantLocalValue.LogonGroupId);
            }
            catch (Exception e)
            {
                logger.Info($"Skip Exception get service account failed,default option is Token service  {e} ");
                RetryPolicy.ExecuteAction(() =>
                {
                    accounts = RMAosApiClient.GetServiceAccounts(TenantLocalValue.LogonGroupId);
                });
            }
            logger.Info($"The tenant: [{TenantLocalValue.LogonGroupId}] get service accounts: [{accounts.Count}] from aos.");
            foreach (var site in sites)
            {
                if (site == null)
                {
                    logger.Warn($"Can't find site.");
                    continue;
                }
                if (string.IsNullOrEmpty(site.ServiceAccountId))
                {
                    logger.Warn($"The [{site.username}] service account id is null or empty.");
                    continue;
                }
                var account = accounts.FirstOrDefault(a => a.UserName.ToLowerInvariant() == site.username.ToLowerInvariant());


                if (account != null)
                {
                    var adminUrl = RMAosApiClient.GetO365TenantInfoByIdAsync(account.TenantId).GetAwaiter().GetResult().AdminUrl;

                    site.username = account.UserName;
                    site.password = account.Password;
                    site.TenantId = account.TenantId;
                    site.AdminUrl = adminUrl;
                }
            }
        }

        public Dictionary<string, string> GetTeamId2TeamNameDicByTeamIds(List<string> teamIds)
        {
            return RemoteNodeDao.GetTeamId2TeamNameDicByTeamIds(teamIds);
        }

        public RemoteNodePara GetGroupByNameAndNodeLevel(string name, int nodeLevel)
        {
            return RemoteNodeDao.GetGroupByNameAndNodeLevel(name, nodeLevel);
        }

        public RemoteNodePara GetGroupByAosIdAndNodeLevel(string aosId, int nodeLevel)
        {
            return RemoteNodeDao.GetGroupByAosIdAndNodeLevel(aosId, nodeLevel);
        }

        public HashSet<string> GetO365GroupSiteByUrls(List<string> urls)
        {
            return RemoteNodeDao.GetO365GroupSiteByUrls(urls);
        }

        public Dictionary<string, List<string>> GetO365GroupSiteName2UrlDicByNames(List<string> names)
        {
            return RemoteNodeDao.GetO365GroupSiteName2UrlDicByNames(names);
        }

        public void UpdateO365GroupSiteByUrls(List<RemoteSiteCollection> o365GroupSiteCollections)
        {
            RemoteNodeDao.UpdateO365GroupSiteByUrls(o365GroupSiteCollections);
        }

        public void UpdateSiteCollectionSecondParentId(List<SyncRemoteNodePara> siteCollections)
        {
            RemoteNodeDao.UpdateSiteCollectionSecondParentId(siteCollections);
        }

        public Dictionary<string, List<NodeCollection>> GetSiteCollectionByParentIds(List<string> ids)
        {
            Func<List<string>, Dictionary<string, List<NodeCollection>>> func = RemoteNodeDao.GetSiteCollectionByParentIds;
            return DatabaseUtility.ExecuteOnBatch(ids, func);
        }

        public List<RemoteSiteCollection> GetRemoteSiteCollectionsByParentId(string parentId, SiteCollectionState[] states)
        {
            return RemoteNodeDao.GetRemoteSiteCollectionsByParentId(parentId, states);
        }

        public List<RemoteSiteCollection> GetRemoteSiteCollectionsByParentId(string parentId, SiteCollectionState[] states, AvePoint.GCommon.Contract.SharePointBrowser.SiteCollectionType[] types, string[] names = null)
        {
            return RemoteNodeDao.GetRemoteSiteCollectionsByParentId(parentId, states, types, names);
        }

        public async Task<RMSPSampleTreeNode> GetSiteCollectionsAsync(RMSPSampleTreeNode node, bool checkPermission, bool includeOrphanNode = false)
        {
            if (string.IsNullOrEmpty(node.SearchKey))
            {
                return RemoteNodeDao.GetSiteCollections(node, checkPermission && (!(await IsSPOAdminAsync()) && !(await IsOpusSOSPOAdminAsync())), includeOrphanNode);
            }

            if (node.SourceType == (int)SourceFlag.Teams)
            {
                return RemoteNodeDao.GetTeamsBySearch(node, checkPermission && (!(await IsSPOAdminAsync()) && !(await IsOpusSOSPOAdminAsync())), node.SearchKey, includeOrphanNode);
            }

            return RemoteNodeDao.GetSiteCollectionBySearch(node, checkPermission && (!(await IsSPOAdminAsync()) && !(await IsOpusSOSPOAdminAsync())), node.SearchKey, includeOrphanNode);
        }

        public RMSPSampleTreeNode GetSiteCollectionsUnderTeamsAsync(RMSPSampleTreeNode node)
        {
            if (node.Parent == null)
            {
                return node;
            }
            return RemoteNodeDao.GetSiteCollectionsUnderTeams(node);
        }

        public List<SyncRemoteNodePara> GetAllPrivateChannelByPage(int pageIndex, int pageSize)
        {
            return RemoteNodeDao.GetAllPrivateChannelByPage(pageIndex, pageSize);
        }

        public List<SyncRemoteNodePara> GetAllPrivateChannel()
        {
            return RemoteNodeDao.GetAllPrivateChannel();
        }

        public bool IsPrivateChannelGroupExist()
        {
            return RemoteNodeDao.IsPrivateChannelGroupExist();
        }
        public List<Guid> GetOrphanedODIds()
        {
            return RemoteNodeDao.GetOrphanedODIds();
        }

        public List<string> GetPrivateChannelByGroupTeamSiteContainerIds(List<string> groupTeamSiteContainerIds)
        {
            return RemoteNodeDao.GetPrivateChannelByGroupTeamSiteContainerIds(groupTeamSiteContainerIds);
        }

        public RemoteSiteCollection GetRemoteSiteCollectionById(string id)
        {
            var result = RemoteNodeDao.GetRemoteSiteCollectionById(id);
            DatabaseDecrypt(result);
            var siteCollection2appProfileDic = RMAosApiClient.GetRemoteNodeUrlToAppProfileDict(new List<RemoteSiteCollection>() { result });
            result.password = string.Empty;
            if (siteCollection2appProfileDic.TryGetValue(result.url, out var appProfile) && appProfile != null)
            {
                result.AADEnvironment = appProfile.AADEnvironment;
            }
            return result;
        }

        public RemoteSiteCollection GetRemoteSiteCollectionByObjectId(string id)
        {
            var result = RemoteNodeDao.GetRemoteSiteCollectionByObjectId(id);
            DatabaseDecrypt(result);
            var siteCollection2appProfileDic = RMAosApiClient.GetRemoteNodeUrlToAppProfileDict(new List<RemoteSiteCollection>() { result });
            result.password = string.Empty;
            if (siteCollection2appProfileDic.TryGetValue(result.url, out var appProfile) && appProfile != null)
            {
                result.AADEnvironment = appProfile.AADEnvironment;
            }
            return result;
        }

        public List<RemoteSiteCollection> GetRemoteSiteCollectionByIds(List<string> ids)
        {
            var sites = RemoteNodeDao.GetRemoteSiteCollectionByIds(ids);
            DatabaseDecrypt(sites);
            var siteCollection2appProfileDic = RMAosApiClient.GetRemoteNodeUrlToAppProfileDict(sites);
            foreach (var site in sites)
            {
                site.password = string.Empty;
                if (siteCollection2appProfileDic.TryGetValue(site.url, out var appProfile) && appProfile != null)
                {
                    site.AADEnvironment = appProfile.AADEnvironment;
                }
            }
            return sites;
        }

        public RemoteSiteCollection GetRemoteSiteCollectionByUrl(string url)
        {
            var site = RemoteNodeDao.GetRemoteSiteCollectionByUrl(url);
            DatabaseDecrypt(site);
            return site;
        }

        public RemoteSiteCollection GetRemoteSiteCollectionByListUrl(string listUrl)
        {
            var site = RemoteNodeDao.GetRemoteSiteCollectionByListUrl(listUrl);
            DatabaseDecrypt(site);
            return site;
        }

        public List<RemoteWebApplication> GetWebApplications()
        {
            return RemoteNodeDao.GetAuthorisedAllSiteGroups(true);
        }
        public List<RemoteSiteCollection> GetRemoteSiteCollectionsByNodeLevel(int type)
        {
            return RemoteNodeDao.GetRemoteSiteCollectionsByNodeLevel(type);
        }
        public RemoteWebApplication GetWebApplicationById(string id)
        {
            return RemoteNodeDao.GetWebApplicationById(id);
        }

        public string GetContainerIdByName(string containerName, int nodeLevel)
        {
            return RemoteNodeDao.GetContainerIdByName(containerName, nodeLevel);
        }

        public List<RemoteSiteCollection> GetAllRemoteSiteCollections()
        {
            return GetAllRemoteSiteCollections(1, int.MaxValue, null).Items;
        }

        public PagedSiteCollectionResponse GetAllRemoteSiteCollections(int pageIndex, int pageSize, string key)
        {
            if (pageIndex < 1 || pageSize < 1)
            {
                throw new ArgumentException("Invalid pagination parameters.");
            }

            var pagedSiteCollections = RemoteNodeDao.GetAllRemoteSiteCollections(pageIndex, pageSize, key, out var totalCount);
            DatabaseDecrypt(pagedSiteCollections);
            var siteCollection2appProfileDic = RMAosApiClient.GetRemoteNodeUrlToAppProfileDict(pagedSiteCollections);
            foreach (var item in pagedSiteCollections)
            {
                item.password = null;
                if (siteCollection2appProfileDic.TryGetValue(item.url, out var appProfile) && appProfile != null)
                {
                    item.AADEnvironment = appProfile.AADEnvironment;
                }
                logger.Info($"Add [{item.url}] to paged remote site collections");
            }
            logger.Info($"Paged remote site collections are : {string.Join(',', pagedSiteCollections.Select(item => item.url))}");

            return new PagedSiteCollectionResponse
            {
                Items = pagedSiteCollections,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<RMRemoteSiteCollectionPageInfo> GetMappedSitesPagedAsync(RMRemoteSiteCollectionPageRequest request)
        {
            try
            {
                List<string> selectedNodeIds = await _planSiteMappingDao.GetNodeIdsByPlanProfileIdAsync(request.PlanProfileId);

                int totalCount;
                var items = RemoteNodeDao.GetMappedRemoteSitesPaged(
                    request.PageIndex,
                    request.PageSize,
                    request.Key,
                    selectedNodeIds,
                    out totalCount);

                return new RMRemoteSiteCollectionPageInfo
                {
                    TotalCount = totalCount,
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize,
                    Items = items
                };
            }
            catch (Exception ex)
            {
                logger.Error($"Error in GetMappedSitesPagedAsync for PlanProfileId: {request?.PlanProfileId}, Error: {ex}");
                throw;
            }
        }

        public bool IsRemoteSiteExist() 
        {
            using PerformanceScope scope = new("RMRemoteNodeService.IsRemoteSiteExist");
            return RemoteNodeDao.IsRemoteSiteExist();
        }

        public List<RemoteWebApplication> GetAllWebApplications(RMBrowseTreeNodeSourceType type)
        {
            return RemoteNodeDao.GetAllWebApplications(type);
        }

        public async Task<RMSPSampleTreeNode> GetWebApplicationsAsync(RMSPSampleTreeNode node, RMBrowseTreeNodeSourceType type, bool checkPermission)
        {
            return RemoteNodeDao.GetWebApplications(node, type, checkPermission && (!(await IsSPOAdminAsync()) && !(await IsOpusSOSPOAdminAsync())));
        }

        public async Task<RMSPSampleTreeNode> GetWebApplicationsForSearchAsync(RMSPSampleTreeNode node, RMBrowseTreeNodeSourceType type, bool checkPermission)
        {
            return await RemoteNodeDao.GetWebApplicationsForSearchAsync(node, type, checkPermission && (!(await IsSPOAdminAsync()) && !(await IsOpusSOSPOAdminAsync())));
        }

        public async Task<RMSPSampleTreeNode> GetWebApplicationsOnlyForSearchAsync(RMSPSampleTreeNode node, RMBrowseTreeNodeSourceType type, bool checkPermission)
        {
            return await RemoteNodeDao.GetWebApplicationsOnlyForSearchAsync(node, type, checkPermission && (!(await IsSPOAdminAsync()) && !(await IsOpusSOSPOAdminAsync())));
        }

        public async Task<RMSPSampleTreeNode> GetWebApplicationsForExactlySearchAsync(RMSPSampleTreeNode node, RMBrowseTreeNodeSourceType type, bool checkPermission, bool includeOrphanNode)
        {
            return await RemoteNodeDao.GetWebApplicationsForExactlySearchAsync(node, type, checkPermission && (!(await IsSPOAdminAsync()) && !(await IsOpusSOSPOAdminAsync())), includeOrphanNode);
        }

        public async Task<RMSPSampleTreeNode> GetWebApplicationsOnlyForExactlySearchAsync(RMSPSampleTreeNode node, RMBrowseTreeNodeSourceType type, bool checkPermission, bool includeOrphanNode)
        {
            return await RemoteNodeDao.GetWebApplicationsOnlyForExactlySearchAsync(node, type, checkPermission && (!(await IsSPOAdminAsync()) && !(await IsOpusSOSPOAdminAsync())), includeOrphanNode);
        }

        private System.Threading.Tasks.Task<bool> IsSPOAdminAsync()
        {
            return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOAdmin);
        }

        private System.Threading.Tasks.Task<bool> IsOpusSOSPOAdminAsync()
        {
            return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.SPOAdmin);
        }

        public List<RemoteWebApplication> GetRemoteWebApplications()
        {
            return RemoteNodeDao.GetRemoteWebApplications();
        }

        public List<RemoteWebApplication> GetAuthorisedSkyDriveProGroups()
        {
            return RemoteNodeDao.GetAuthorisedSkyDriveProGroups();
        }

        public List<RemoteWebApplication> GetAuthorisedOffice365GroupSitesGroups()
        {
            return RemoteNodeDao.GetAuthorisedOffice365GroupSitesGroups();
        }

        public List<RemoteWebApplication> GetAuthorisedPrivateChannelSitesGroups()
        {
            return RemoteNodeDao.GetAuthorisedPrivateChannelSitesGroups();
        }

        public List<RemoteWebApplication> GetAuthorisedAllSiteGroups(bool includeO365 = false, bool includePrivateChannel = false)
        {
            return RemoteNodeDao.GetAuthorisedAllSiteGroups(includeO365, includePrivateChannel);
        }

        public bool ValidOrphenSiteCollection(RMSPTreeNode siteCollectionNode)
        {
            try
            {
                if (siteCollectionNode.NodeType != (int)AvePoint.GCommon.Contract.Tree.Object.NodeType.SkyDriveProSitesGroup
                || siteCollectionNode.Level != (int)AvePoint.GCommon.Contract.Tree.Object.NodeLevel.SiteCollection)
                {
                    return false;
                }
                RemoteSiteCollection remoteNode = RemoteNodeDao.GetRemoteSiteCollectionById(siteCollectionNode.Id);
                if (remoteNode != null && remoteNode.Name == null && remoteNode.NodeType == RemoveNodeType.SkyDrivePro)
                {
                    return true;
                }else
                {
                    logger.Error(@$"fail check ValidOrphenSiteCollection, node : {siteCollectionNode.FullPath}");
                    return false;
                }
            }
            catch (Exception e)
            {
                logger.Error(@$"have exception when ValidOrphenSiteCollection,ex:{e}");
                return false;
            }
        }

        public List<RemoteSiteCollection> GetRemoteSiteCollectionsByTeamsId(string teamsId, SiteCollectionState[] states)
        {
            return RemoteNodeDao.GetRemoteSiteCollectionsByTeamsId(teamsId, states);
        }

        public RemoteSiteCollection GetTeamsNodeByTeamsAddress(string teamsAddress)
        {
            if (string.IsNullOrEmpty(teamsAddress))
            {
                return null;
            }
            return RemoteNodeDao.GetTeamsNodeByTeamsAddress(teamsAddress);
        }

        public RMSPTreeNode GetTeamsNodeByTeamsId(string teamsId)
        {
            if (string.IsNullOrEmpty(teamsId))
            {
                return null;
            }
            return RemoteNodeDao.GetTeamsNodeByTeamsId(teamsId);
        }

        public (RemoteSiteCollection, List<RemoteSiteCollection>) GetTeamsGroupAndChannelsCollectionByTeamsId(string teamsId, bool needChannel = false)
        {
            if (string.IsNullOrEmpty(teamsId))
            {
                return (null, null);
            }
            return RemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(teamsId, needChannel);
        }

        public async Task<SearchSiteCollectionLazyLoadResponse> SearchSiteCollectionLazyLoad(SearchSiteCollectionLazyLoadRequest condition, bool checkPermission)
        {
            return await Task.FromResult(RemoteNodeDao.SearchSiteCollectionLazyLoad(condition, checkPermission && (!(await IsSPOAdminAsync()) && !(await IsOpusSOSPOAdminAsync()))));
        }

        public async Task<(string, string, string)> GetChannelSiteInfoAsync(string siteCollectionUrl)
        {
            using var _ = new PerformanceScope("RMRemoteNodeService.GetChannelSiteInfoAsync");
            return await RemoteNodeDao.GetChannelSiteInfoAsync(siteCollectionUrl);
        }
    }
}
