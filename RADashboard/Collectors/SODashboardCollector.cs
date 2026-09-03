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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;
using RAArchiverCommon.TeamsController;

namespace RADashboard.Collectors
{
    public class SODashboardCollector
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(SODashboardCollector));

        private static readonly IArchiverIndexSubInfoDao s_archiverIndexSubInfoDao = PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();

        private static readonly IArchiverSiteMasterIndexDao s_archiverSiteMasterIndexDao = PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();

        private static readonly IRestoreSearchService s_restoreSearchService = PlatformWindsorManager.GetService<IRestoreSearchService>();

        private static readonly IRMArchiveSiteInfoDao s_archiveSiteInfoDao = PlatformWindsorManager.GetService<IRMArchiveSiteInfoDao>();

        private static readonly IRMArchiveGDriveInfoDao _archiveGDriveInfoDao = PlatformWindsorManager.GetService<IRMArchiveGDriveInfoDao>();

        private static readonly IRMKeyValueDao s_keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        
        private static IRMSiteDeletedSizeInfoDao siteDeletedSizeInfoDao => PlatformWindsorManager.GetService<IRMSiteDeletedSizeInfoDao>();
        private static IRMGDriveDeletedSizeInfoDao _gdriveDeletedSizeInfoDao => PlatformWindsorManager.GetService<IRMGDriveDeletedSizeInfoDao>();

        private static readonly IRMArchiveTeamsGroupInfoDao s_archiveTeamsGroupInfoDao = PlatformWindsorManager.GetService<IRMArchiveTeamsGroupInfoDao>();

        public static async Task Collect()
        {
            await SyncArchivedSiteInfoes();
            await SyncArchivedTeamsGroupInfoes();
        }

        private static async Task CollectTop50SiteArchivedAsync()
        {

            try
            {
                var allSiteCollectionInfos = s_archiverSiteMasterIndexDao.GetAllSiteCollectionNodsInfo(new List<int>() { (int)SourceFlag.Google }).DistinctBy(item => item.SiteURL).ToList();
                Logger.Info($"Get distinct archived site collection info success, count is: {allSiteCollectionInfos.Count}");
                var siteUrlAndJobIdMapping = s_archiverSiteMasterIndexDao.GetAllBackupSiteCollectionDistinctJobIdMappings(allSiteCollectionInfos.Select(site => site.SiteURL).ToList());
                var siteUrlAndSizeMapping = s_archiverIndexSubInfoDao.GetAllArchiverIndexSubInfoBySiteUrls(siteUrlAndJobIdMapping);
                var allSiteCollectionContract = allSiteCollectionInfos.ConvertAll(site => ConvertToDto(site));
                var allSiteCollectionNodesInfo = allSiteCollectionContract.ConvertAll(site => new SiteCollectionNodesInfo() { SiteGroupId = site.WebId, SiteUrl = site.SiteURL, SPObjectId = site.SiteId }).ToList();
                var filterPolicy = new ArchiverRestoreResult()
                {
                    SerchContract = new BackupDataSearchContract() { FilterPolicy = new ArchiverRestoreFilter() { Level = PolicyLevel.Document } }
                };
                var allArchivedSiteInfoStr = await s_restoreSearchService.GetSearchTreeResultForJobAsync(allSiteCollectionContract, filterPolicy, allSiteCollectionNodesInfo);
                var allArchivedSiteInfos = SerializerHelper.DeserializeByDataContractSerializer<List<RMArchiveSiteInfo>>(allArchivedSiteInfoStr);
                Logger.Info($"Get distinct archived site info from index success, count is: {allArchivedSiteInfos.Count}");
                var deletedSizeInfos = siteDeletedSizeInfoDao.GetSiteDeleteSizeInfoWithSiteId();

                var allProfiles = RMAosApiClient.GetAllProfiles(TenantLocalValue.LogonGroupId).DistinctBy(profile => profile.DomainName);
                var tenantIdDomainMap = allProfiles.ToDictionary(profile => profile.DomainName.ToLowerInvariant(), profile => profile.TenantId);

                if (deletedSizeInfos != null)
                {
                    allArchivedSiteInfos.ForEach(site =>
                    {
                        site.ArchivedSize = siteUrlAndSizeMapping[site.SiteUrl];
                        site.DeletedSize = deletedSizeInfos.ContainsKey(site.SiteUrl) ? (deletedSizeInfos[site.SiteUrl]?.Item2 ?? 0.00f) / ContractConstants.GBSizeInterval : 0;
                        var domain = tenantIdDomainMap.Keys.Where(key => site.SiteUrl.StartsWith("https://" + key + ".", StringComparison.InvariantCultureIgnoreCase)
                                                                    || site.SiteUrl.StartsWith("https://" + key + "-", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                        if (!string.IsNullOrEmpty(domain) && tenantIdDomainMap.TryGetValue(domain, out var tenantId))
                        {
                            site.O365TenantId = tenantId;
                        }
                        else
                        {
                            Logger.Warn($"CollectTop50SiteArchivedAsync.Current Site:{site.SiteUrl} does not have APP Profile.");
                        }
                    });
                }
                else
                {
                    allArchivedSiteInfos.ForEach(site =>
                    {
                        site.ArchivedSize = siteUrlAndSizeMapping[site.SiteUrl];
                        site.DeletedSize = 0;
                        var domain = tenantIdDomainMap.Keys.Where(key => site.SiteUrl.StartsWith("https://" + key + ".", StringComparison.InvariantCultureIgnoreCase)
                                                                    || site.SiteUrl.StartsWith("https://" + key + "-", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                        if (!string.IsNullOrEmpty(domain) && tenantIdDomainMap.TryGetValue(domain, out var tenantId))
                        {
                            site.O365TenantId = tenantId;
                        }
                        else
                        {
                            Logger.Warn($"CollectTop50SiteArchivedAsync.Current Site:{site.SiteUrl} does not have APP Profile.");
                        }
                    });
                }
                var siteUrlList = allArchivedSiteInfos?.Select(site => site.SiteUrl).ToList();
                foreach (var info in deletedSizeInfos)
                {
                    if (!siteUrlList.Contains(info.Key))
                    {
                        var domain = tenantIdDomainMap.Keys.Where(key => info.Key.StartsWith("https://" + key + ".", StringComparison.InvariantCultureIgnoreCase)
                                                                   || info.Key.StartsWith("https://" + key + "-", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                        string tenantId = string.Empty;
                        if (!string.IsNullOrEmpty(domain) && tenantIdDomainMap.TryGetValue(domain, out var tempTenantId))
                        {
                            tenantId = tempTenantId;
                        }
                        else
                        {
                            Logger.Warn($"CollectTop50SiteArchivedAsync.Current Site:{info.Key} does not have APP Profile.");
                        }
                        allArchivedSiteInfos?.Add(new RMArchiveSiteInfo()
                        {
                            Id = Guid.NewGuid().ToString(),
                            SiteUrl = info.Key,
                            SiteId = info.Value.Item1,
                            ArchivedSize = 0,
                            DeletedSize = (double)info.Value.Item2 / ContractConstants.GBSizeInterval,
                            VersionNumber = 0,
                            FileNumber = 0,
                            O365TenantId = tenantId ?? string.Empty,
                        });
                    }
                }
                var successCount = s_archiveSiteInfoDao.BatchCreate(allArchivedSiteInfos);
                Logger.Info($"Batch insert archive site info to db count is : {successCount}");
                await s_keyValueDao.SaveOrUpdateAsync(new RMKeyValue() { Key = "SyncArchivedSiteInfo", Value = "True" });
                DashboardCollectorJobManager.AddSuccessJobDetail(CollectorEventType.CollectArchivedSiteInfo, string.Empty);
            }
            catch
            {
                throw;
            }
        }

        private static ArchiverSiteMasterIndexContract ConvertToDto(AvePoint.RA.DB.Model.ArchiverSiteMasterIndex domain)
        {
            ArchiverSiteMasterIndexContract contract = null;
            if (domain != null)
            {
                contract = new ArchiverSiteMasterIndexContract();
                contract.ArchiverTime = domain.ArchiverTime;
                contract.Id = domain.Id;
                contract.JobId = domain.JobId;
                contract.JobState = domain.JobState;
                contract.SiteId = domain.SiteId;
                contract.SiteURL = domain.SiteURL;
                contract.SPVersion = domain.SPVersion;
                contract.WebId = domain.SiteGroupId;
                contract.MergeIndexState = (MergeIndexState)domain.MergeIndexState;
                contract.StorageInfo = domain.StorageInfo;
                if (!string.IsNullOrWhiteSpace(domain.Extension))
                {
                    contract.Extension = SerializerHelper.DeserializeByDataContractSerializer<ArchiverSiteMasterIndexExtension>(domain.Extension);
                }
            }
            return contract;
        }

        private static async Task SyncArchivedSiteInfoes()
        {
            try
            {
                if (s_keyValueDao.GetValueByKey("SyncArchivedSiteInfo") == null || !bool.TryParse(s_keyValueDao.GetValueByKey("SyncArchivedSiteInfo").Value, out var result))
                {
                    Logger.Info($"Start execute so dashboard collect.");
                    using (var scope = new PerformanceScope("Collect SO Dashboard sites info"))
                    {
                        var deleteCount = await s_archiveSiteInfoDao.DeleteAllAsync();
                        Logger.Info($"Empty RMArchiveSiteInfoes table successful, delete count is : {deleteCount} ,begin to collect new site info by full, current tenant id is : {TenantLocalValue.LogonGroupId}");
                        await CollectTop50SiteArchivedAsync();
                    }
                    Logger.Info($"Successfule execute so dashboard collect.");
                }
                else
                {
                    Logger.Info("Current tenant has collected archived site infos, so skip");
                    DashboardCollectorJobManager.HasSuccess = true;
                }
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while execute so dashboard collect. Error: {e}");
                DashboardCollectorJobManager.AddSOFailedJobDetail(CollectorEventType.CollectArchivedSiteInfo, e.Message);
            }
        }

        #region Teams Group
        private static async Task SyncArchivedTeamsGroupInfoes()
        {
            try
            {
                var isSendJobDetail = false;
                var worker = new TeamsSODashboardWorker(true);

                #region Update GroupMailboxAddress to ArchiveSiteMasterIndexes
                if (s_keyValueDao.HasUpgradeTeams() && !s_keyValueDao.HasUpdateEmail4ArchivedSite())
                {
                    Logger.Info($"Start updating group mailbox address for ArchiveSiteMasterIndex. TenantId [{TenantLocalValue.LogonGroupId}]");
                    using var scope = new PerformanceScope("Update group mailbox address for ArchiveSiteMasterIndex.");
                    await worker.SyncTeamsForArchiverSiteMasterIndexes();
                    Logger.Info($"Successful updating group mailbox address for ArchiveSiteMasterIndex.");
                    isSendJobDetail = true;
                }
                else
                {
                    Logger.Info("Current tenant has updated group mailbox address for ArchiveSiteMasterIndex, so skip.");
                    DashboardCollectorJobManager.HasSuccess = true;
                }
                #endregion

                #region SO dashboard collect for Teams&Group
                if (s_keyValueDao.HasUpgradeTeams() && !s_keyValueDao.HasSyncArchivedTeamsGroup() 
                    && s_keyValueDao.HasUpdateEmail4ArchivedSite())
                {
                    Logger.Info($"Start execute SO dashboard collect for Teams&Group.");
                    using (var scope = new PerformanceScope("Collect SO Dashboard Teams&Group infoes"))
                    {
                        var deleteCount = await s_archiveTeamsGroupInfoDao.DeleteAllAsync();
                        Logger.Info($"Clear RMArchiveTeamsGroupInfoes table successful, Count [{deleteCount}] , TenantId [{TenantLocalValue.LogonGroupId}].");
                        await worker.CollectTeamsGroupArchivedAsync();
                        Logger.Info($"Successful execute so dashboard collect for Teams&Group.");
                    }
                    isSendJobDetail = true;
                }
                else
                {
                    Logger.Info("Current tenant has collected archived Teams&Group infoes, so skip.");
                    DashboardCollectorJobManager.HasSuccess = true;
                }
                #endregion

                if (isSendJobDetail)
                {
                    DashboardCollectorJobManager.AddSuccessJobDetail(CollectorEventType.CollectArchivedTeamsGroupInfo, string.Empty);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while execute so dashboard collect. Error: {ex}");
                DashboardCollectorJobManager.AddSOFailedJobDetail(CollectorEventType.CollectArchivedTeamsGroupInfo, ex.Message);
            }
        }
        #endregion
    }
}
