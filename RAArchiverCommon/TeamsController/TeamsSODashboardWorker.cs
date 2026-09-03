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
using AvePoint.RA.Common.Aos;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;

namespace RAArchiverCommon.TeamsController
{
    public class TeamsSODashboardWorker
    {
        private static IRALogger Logger = RALogger.GetInstance(typeof(TeamsSODashboardWorker));
        public static IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao => PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
        public static ICommonSiteMasterIndexDao CommonSiteMasterIndexDao => PlatformWindsorManager.GetService<ICommonSiteMasterIndexDao>();
        public static IRMRemoteNodeDao RMNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        public static IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        public static IEXOArchiverIndexSubInfoDao EXOArchiverIndexSubInfoDao => PlatformWindsorManager.GetService<IEXOArchiverIndexSubInfoDao>();
        public static IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        public static IRMArchiveTeamsGroupInfoDao ArchiveTeamsGroupInfoDao => PlatformWindsorManager.GetService<IRMArchiveTeamsGroupInfoDao>();

        private bool _isSODashBoardSync = false;

        public TeamsSODashboardWorker(bool IsSODashBoardSync = false)
        {
            _isSODashBoardSync = IsSODashBoardSync;
        }

        public async Task SyncTeamsForArchiverSiteMasterIndexes()
        {
            try
            {
                var teamsIdMailboxMapping = RMNodeDao.GetAllTeamId2TeamNameMapping();
                Logger.Info($"Found TeamsId and Mailbox mapping count {teamsIdMailboxMapping.Count}.");

                await foreach (var siteUrlBatch in ArchiverSiteMasterIndexDao.GetAllSiteDistinctUrlAsync())
                {
                    Logger.Info($"Batch with {siteUrlBatch.Count()} sites.");
                    var addressSiteUrlsdic = RMNodeDao.GetGroupAddressAndRelatedSiteUrlsDic(siteUrlBatch, teamsIdMailboxMapping) ?? [];

                    foreach (var item in addressSiteUrlsdic)
                    {
                        if (string.IsNullOrEmpty(item.Key))
                        {
                            Logger.Info($"Cannot find GroupMailBoxAddresses for sites: {string.Join(", ", item.Value)}");
                            continue;
                        }
                        Logger.Info($"Update address for related sites. Address: {item.Key}, sites: {string.Join(", ", item.Value)}");
                        ArchiverSiteMasterIndexDao.UpdateGroupMailboxAddressBySiteURL(item.Value, item.Key);
                    }
                }

                await RMKeyValueDao.SaveOrUpdateAsync(new() { Key = KeyNameCollection.HasUpdateEmail4ArchivedSite, Value = "True" });
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while SyncTeamsForArchiverSiteMasterIndexes. Ex:{ex.Message}.");
                throw;
            }
        }

        public async Task CollectTeamsGroupArchivedAsync()
        {
            try
            {
                Logger.Info("Start collect Teams group archived infoes.");

                var allTeamsGroupIndexes = CommonSiteMasterIndexDao.GetAllTeamIndexInfoes().DistinctBy(i => i.SiteURL).ToList();

                var mailboxAddresses = allTeamsGroupIndexes.Select(i => i.SiteURL).ToList();

                // Teams archived size including related SP sites
                var relatedSitesArchivedSizeMapping = new Dictionary<string, double>();
                var teamsWithRelatedSites = CommonSiteMasterIndexDao.GetTeamsGroupWithRelatedSitesUrlMappings(mailboxAddresses);

                var existingteamsWithRelatedSites = ArchiverSiteMasterIndexDao.GetAllBackSiteCollectionGroupMailboxMapping();

                foreach (var kvp in existingteamsWithRelatedSites)
                {
                    if (teamsWithRelatedSites.TryGetValue(kvp.Key, out var existingList))
                    {
                        var unionList = existingList.Union(kvp.Value).Distinct().ToList();
                        teamsWithRelatedSites[kvp.Key] = unionList;
                    }
                    else
                    {
                        Logger.Info($"Add sites that did not run teams rule to list for calculation. Mailbox addresses: {kvp.Key}. Sites: {string.Join(", ", kvp.Value)}");
                        teamsWithRelatedSites[kvp.Key] = kvp.Value;
                    }
                }

                Logger.Info($"Retrieved Teams group indexes. Mailbox addresses:\n{string.Join(",\n", teamsWithRelatedSites.Keys)}");

                var siteUrlAndSizeMapping = ArchiverSiteMasterIndexDao.GetSiteArchivedSizeInGB();

                foreach (var kvp in teamsWithRelatedSites)
                {
                    double totalSize = 0;
                    foreach (var siteUrl in kvp.Value)
                    {
                        if (siteUrlAndSizeMapping.TryGetValue(siteUrl, out var archivedSize))
                        {
                            totalSize += archivedSize;
                        }
                    }
                    relatedSitesArchivedSizeMapping[kvp.Key] = totalSize;
                    Logger.Info($"Calculated related SP site size for mailbox [{kvp.Key}]: {totalSize} GB");
                }

                // Teams + Mailbox size (not including related sites)
                var teamsAndJobIdMapping = CommonSiteMasterIndexDao.GetAllBackupTeamsDistinctJobIdMappings(mailboxAddresses);
                var teamsAndSizeMapping = ArchiverIndexSubInfoDao.GetAllArchiverIndexSubInfoBySiteUrls(teamsAndJobIdMapping);
                var mailboxAndSizeMapping = EXOArchiverIndexSubInfoDao.GetAllEXOArchiverIndexSubInfoByMailboxAddresses(teamsAndJobIdMapping.Keys.ToList());

                Logger.Info($"Retrieved Teams and mailbox archived sizes. Mailboxes:\n{string.Join(",\n", mailboxAndSizeMapping.Keys)}");
                Logger.Info($"Total archived size from mailbox only: {mailboxAndSizeMapping.Values.Sum()} GB");

                // Merge all into final result
                var archivedTeamsGroupInfoes = teamsWithRelatedSites.Keys.Select(GenerateArchiverTeamsGroupInfo).ToList();

                foreach (var teamsGroup in archivedTeamsGroupInfoes)
                {
                    var mailbox = teamsGroup.MailboxAddress;
                    double relatedSize = relatedSitesArchivedSizeMapping.TryGetValue(mailbox, out var rSize) ? rSize : 0;
                    double teamSize = teamsAndSizeMapping.TryGetValue(mailbox, out var tSize) ? tSize : 0;
                    double mailboxSize = mailboxAndSizeMapping.TryGetValue(mailbox, out var mSize) ? mSize : 0;

                    teamsGroup.ArchivedSize = relatedSize + teamSize + mailboxSize;
                    teamsGroup.ArchivedSizeWithoutRelatedSites = teamSize + mailboxSize;
                    teamsGroup.O365TenantId = GetO365TeamsTenantId(mailbox);
                }

                // Save ArchiveTeamsGroupInfo
                var successCount = await ArchiveTeamsGroupInfoDao.BatchUpsertAsync(archivedTeamsGroupInfoes);
                Logger.Info($"Batch upsert completed. Total inserted: {successCount}");

                // Update group mailbox address for related sites
                foreach (var kvp in teamsWithRelatedSites)
                {
                    ArchiverSiteMasterIndexDao.UpdateGroupMailboxAddressBySiteURL(kvp.Value, kvp.Key);
                    Logger.Info($"Update group mailbox address for related sites successfully, mailbox address [{kvp.Key}].");
                }

                if (_isSODashBoardSync)
                {
                    await RMKeyValueDao.SaveOrUpdateAsync(new() { Key = KeyNameCollection.HasSyncArchivedTeamsGroup, Value = "True" });
                }
            }
            catch (Exception ex)
            {
                if (_isSODashBoardSync) throw;
                Logger.Error($"An error occurred while collect Teams/Group archived infoes. Ex:{ex.Message}.");
            }
        }

        public async Task UpdateTeamsGroupArchivedInfo(string groupMailbox)
        {
            if (string.IsNullOrEmpty(groupMailbox)) return;

            try
            {
                var teamsNode = RMNodeDao.GetTeamsNodeByTeamsAddress(groupMailbox);

                if (IsNeedUpdateTeamsGroupArchivedInfo())
                {
                    Logger.Info($"Begin collect Teams/Group archived data. URL [{groupMailbox}].");
                    if (teamsNode != null)
                    {
                        (var groupSite, var channels) = RMNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(teamsNode.TeamId, true);
                        var relatedSitesSet = new HashSet<string>(channels.Select(i => i.url)) { groupSite.url };
                        await UpdateArchivedTeamsGroupInfo(teamsNode.Name, relatedSitesSet.ToList(), teamsNode.TenantId);
                    }
                    else
                    {
                        var o365TenantId = GetO365TeamsTenantId(groupMailbox);
                        await UpdateArchivedTeamsGroupInfo(groupMailbox, new(), o365TenantId);
                    }
                }
                else if (teamsNode != null)
                {
                    Logger.Info($"Update only site mapping without collecting archived data. Mailbox: [{groupMailbox}]");

                    (var groupSite, var channels) = RMNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(teamsNode.TeamId, true);
                    var relatedSitesSet = new HashSet<string>(channels.Select(i => i.url)) { groupSite.url };
                    ArchiverSiteMasterIndexDao.UpdateGroupMailboxAddressBySiteURL(relatedSitesSet, groupMailbox);
                    Logger.Info($"Update group mailbox address for related sites successfully, mailbox address [{groupMailbox}].");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while updating Teams/Group archived info for [{groupMailbox}]. Exception: {ex}");
            }
        }

        public async Task UpdateTeamsGroupRelatedSiteArchivedInfo(string siteUrl)
        {
            try
            {
                if (!string.IsNullOrEmpty(siteUrl) && IsNeedUpdateTeamsGroupArchivedInfo())
                {
                    Logger.Info($"Begin collect Teams/Group related site archived data. URL [{siteUrl}].");
                    var siteNode = RMNodeDao.GetRemoteSiteCollectionByUrl(siteUrl);
                    if (siteNode != null)
                    {
                        (var groupSite, var channels) = RMNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(siteNode.TeamId, true);
                        if (groupSite == null) return;
                        var relatedSitesSet = new HashSet<string>(channels.Select(i => i.url)) { groupSite.url };
                        await UpdateArchivedTeamsGroupInfo(groupSite.Name, relatedSitesSet.ToList(), siteNode.TenantId, true);
                    }
                    else
                    {
                        Logger.Warn($"Get site node from RemoteNode failed, try to get from ArchiveSiteMasterIndexes. Site:{siteUrl}");
                        var siteMasterIndex = ArchiverSiteMasterIndexDao.GetRestoringSiteCollectionInfoByUrl(siteUrl);
                        if (siteMasterIndex == null) return;
                        if (!string.IsNullOrEmpty(siteMasterIndex.GroupMailboxAddress))
                        {
                            var o365TenantId = GetO365TeamsTenantId(siteMasterIndex.GroupMailboxAddress);
                            await UpdateArchivedTeamsGroupInfo(siteMasterIndex.GroupMailboxAddress, [siteUrl], o365TenantId, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while updating Teams/Group archived info for [{siteUrl}]. Exception: {ex}");
            }
        }

        private string GetO365TeamsTenantId(string teamsAddress)
        {
            string tenantId = string.Empty;
            try
            {
                Logger.Warn($"Get tenant id from teams failed,teamsAddress:{teamsAddress}");
                var domainName = teamsAddress.Split("@")?.LastOrDefault();
                tenantId = RMAosApiClient.GetO365TenantIdByFullDomain(domainName);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to get tenant id.e:{e}");
            }
            return tenantId;
        }

        private async Task UpdateArchivedTeamsGroupInfo(string groupMailbox, List<string> relatedSites, string o365TenantId = null, bool skipUpdateMailbox = false)
        {
            Logger.Info($"Start update Teams/Group archived info, URL [{groupMailbox}].");
            #region Teams archived size include related SP sites
            double totalArchivedSize = 0;
            var relatedSitesArchivedSizeMapping = new Dictionary<string, double>();
            var teamsWithRelatedSPSitesMapping = CommonSiteMasterIndexDao.GetTeamsGroupWithRelatedSitesUrlMappings(new() { groupMailbox }).FirstOrDefault();
            if (!string.IsNullOrEmpty(teamsWithRelatedSPSitesMapping.Key))
            {
                var updatedUrls = new HashSet<string>(teamsWithRelatedSPSitesMapping.Value ?? new List<string>());
                if (relatedSites.Any()) updatedUrls.UnionWith(relatedSites);
                teamsWithRelatedSPSitesMapping = new KeyValuePair<string, List<string>>(groupMailbox, updatedUrls.ToList());
            }
            var allSiteCollectionInfos = ArchiverSiteMasterIndexDao.GetAllSiteCollectionNodsInfo().Select(i => i.SiteURL).Distinct().ToList();
            var targetRelatedSites = teamsWithRelatedSPSitesMapping.Value.Intersect(allSiteCollectionInfos).ToList();
            var siteUrlAndJobIdMapping = ArchiverSiteMasterIndexDao.GetAllBackupSiteCollectionDistinctJobIdMappings(targetRelatedSites);
            var siteUrlAndSizeMapping = ArchiverIndexSubInfoDao.GetAllArchiverIndexSubInfoBySiteUrls(siteUrlAndJobIdMapping);
            teamsWithRelatedSPSitesMapping.Value.ForEach(site =>
            {
                if (siteUrlAndSizeMapping.TryGetValue(site, out double archivedSize))
                {
                    totalArchivedSize += archivedSize;
                }
            });
            relatedSitesArchivedSizeMapping.Add(teamsWithRelatedSPSitesMapping.Key, totalArchivedSize);
            Logger.Info($"Teams's related SP site archived total size: {totalArchivedSize}.");
            #endregion

            #region Teams archived size without related SP sites
            var teamsAndJobIdMapping = CommonSiteMasterIndexDao.GetAllBackupTeamsDistinctJobIdMappings(new() { groupMailbox });
            var teamsAndSizeMapping = ArchiverIndexSubInfoDao.GetAllArchiverIndexSubInfoBySiteUrls(teamsAndJobIdMapping);
            var mailboxAndSizeMapping = EXOArchiverIndexSubInfoDao.GetAllEXOArchiverIndexSubInfoByMailboxAddresses(teamsAndJobIdMapping.Keys.ToList());
            Logger.Info($"Teams and Mailbox archived total size, Teams/Group [{teamsAndSizeMapping.FirstOrDefault().Value}], Mailbox [{mailboxAndSizeMapping.FirstOrDefault().Value}].");
            #endregion

            var archiveTeamsGroupInfo = new RMArchiveTeamsGroupInfo
            {
                ArchivedSize = relatedSitesArchivedSizeMapping[groupMailbox] + teamsAndSizeMapping[groupMailbox] + mailboxAndSizeMapping[groupMailbox],
                ArchivedSizeWithoutRelatedSites = teamsAndSizeMapping[groupMailbox] + mailboxAndSizeMapping[groupMailbox],
                MailboxAddress = groupMailbox,
                O365TenantId = o365TenantId
            };
            await ArchiveTeamsGroupInfoDao.UpdateAchivedTeamsGroupInfo(archiveTeamsGroupInfo);

            if (!skipUpdateMailbox && !teamsWithRelatedSPSitesMapping.Value.IsNullOrEmpty())
            {
                ArchiverSiteMasterIndexDao.UpdateGroupMailboxAddressBySiteURL(teamsWithRelatedSPSitesMapping.Value, teamsWithRelatedSPSitesMapping.Key);
                Logger.Info($"Update group mailbox address for related sites successfully, mailbox address [{teamsWithRelatedSPSitesMapping.Key}].");
            }
            Logger.Info($"Update Teams/Group archived info successful. Group mailbox [{groupMailbox}].");
        }

        private RMArchiveTeamsGroupInfo GenerateArchiverTeamsGroupInfo(string mailboxAddress)
        {
            return new()
            {
                Id = Guid.NewGuid().ToString(),
                ArchivedSize = 0,
                ArchivedSizeWithoutRelatedSites = 0,
                MailboxAddress = mailboxAddress
            };
        }

        private bool IsNeedUpdateTeamsGroupArchivedInfo()
        {
            var syncTeamsArchivedSiteInfo = RMKeyValueDao.GetValueByKey(KeyNameCollection.HasSyncArchivedTeamsGroup);
            var updateEmail4ArchivedSite = RMKeyValueDao.GetValueByKey(KeyNameCollection.HasUpdateEmail4ArchivedSite);
            if (syncTeamsArchivedSiteInfo == null || (bool.TryParse(syncTeamsArchivedSiteInfo.Value, out bool result) && !result) ||
                updateEmail4ArchivedSite == null || bool.TryParse(updateEmail4ArchivedSite.Value, out bool hasUpdate) && !hasUpdate)
            {
                Logger.Warn($"Key HasSyncArchivedTeamsGroup or HasUpdateEmail4ArchivedSite is false or not formatted by true/false value.");
                return false;
            }
            return true;
        }
    }
}
