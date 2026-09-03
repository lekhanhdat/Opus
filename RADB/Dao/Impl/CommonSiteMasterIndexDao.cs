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
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class CommonSiteMasterIndexDao : BaseDao<CommonSiteMasterIndex>, ICommonSiteMasterIndexDao
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(CommonSiteMasterIndexDao));

        private IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao = PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();

        public async Task<long> GetMaxArchiverTimeAsync()
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var index = await context.CommonSiteMasterIndexes.AsNoTracking().OrderByDescending(item => item.ArchiverTime).FirstOrDefaultAsync();
            return index == null ? 0 : index.ArchiverTime;
        }

        public async Task<(bool Has, ArchiverSiteMasterIndexContract indexContract)> TryGetSiteMasterIndexAsync(string jobId)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var index = await context.CommonSiteMasterIndexes.AsNoTracking().FirstOrDefaultAsync(item => item.JobId == jobId);
            return (index != null, ConvertToDto(index));
        }

        public ArchiverSiteMasterIndexContract GetTeamsInfo(ArchiverSiteMasterIndexContract site)
        {
            ArchiverSiteMasterIndexContract contract = null;
            this.ClearNullValue(site);
            List<CommonSiteMasterIndex> domains = null;
            using (var context = GetNewContext())
            {
                domains = context.CommonSiteMasterIndexes.AsNoTracking()
                    .Where(s => s.SiteURL == site.SiteURL && s.MergeIndexState == (int)MergeIndexState.Succeed && s.DataType == site.SourceFlag)
                    .OrderByDescending(s => s.ArchiverTime)
                    .ToList();
            }
            if (domains != null && domains.Count > 0)
            {
                contract = ConvertToDto(domains[0]);
            }
            return contract;
        }

        public void UpdateStateByJobId(int status, string jobId)
        {
            using (var context = GetNewContext())
            {
                var entities = context.CommonSiteMasterIndexes.AsQueryable().Where(s => s.JobId == jobId).ToList();
                foreach (var entity in entities)
                {
                    entity.JobState = status;
                }

                this.BatchUpdate(entities);
            }
        }

        private ArchiverSiteMasterIndexContract ConvertToDto(CommonSiteMasterIndex domain)
        {
            ArchiverSiteMasterIndexContract contract = null;
            if (domain != null)
            {
                contract = new ArchiverSiteMasterIndexContract();
                contract.ArchiverTime = domain.ArchiverTime;
                contract.Id = domain.Id;
                //contract.IndexDeviceId = domain.IndexDeviceId;
                contract.JobId = domain.JobId;
                contract.JobState = domain.JobState;
                contract.SiteId = domain.SiteId;
                contract.SiteURL = domain.SiteURL;
                contract.SPVersion = domain.SPVersion;
                //contract.StoragePolicyId = domain.StoragePolicyId;
                contract.WebId = domain.SiteGroupId;
                contract.MergeIndexState = (MergeIndexState)domain.MergeIndexState;
                contract.StorageInfo = domain.StorageInfo;
                contract.BackupFileType = domain.BackupFileType;
                contract.DuplicateStatus = domain.DuplicateStatus;
                contract.SourceFlag = domain.DataType;
                contract.TeamsId = domain.TeamId;
                if (domain.Extension != null)
                {
                    contract.Extension = SerializerHelper.DeserializeByDataContractSerializer<ArchiverGroupSiteMasterIndexExtension>(domain.Extension);
                }
            }
            return contract;
        }

        private CommonSiteMasterIndex ConvertToCommonSiteMasterIndex(ArchiverSiteMasterIndexContract contract)
        {
            CommonSiteMasterIndex domain = null;
            if (contract != null)
            {
                domain = new CommonSiteMasterIndex();
                domain.ArchiverTime = contract.ArchiverTime;
                domain.Id = contract.Id;
                //contract.IndexDeviceId = domain.IndexDeviceId;
                domain.JobId = contract.JobId;
                domain.JobState = contract.JobState;
                domain.SiteId = contract.SiteId;
                domain.SiteURL = contract.SiteURL;
                domain.SPVersion = contract.SPVersion;
                //contract.StoragePolicyId = domain.StoragePolicyId;
                domain.SiteGroupId = contract.WebId;
                domain.MergeIndexState = (int)contract.MergeIndexState;
                domain.StorageInfo = contract.StorageInfo;
                domain.BackupFileType = contract.BackupFileType;
                domain.DuplicateStatus = contract.DuplicateStatus;
                domain.DataType = contract.SourceFlag;
                domain.TeamId = contract.TeamsId;
                domain.O365TenantId = contract.O365TenantId;
                if (contract.Extension != null)
                {
                    domain.Extension = SerializerHelper.SerializeByDataContractSerializer(contract.Extension);
                }
            }
            return domain;
        }

        private void ClearNullValue(ArchiverSiteMasterIndexContract site)
        {
            if (site != null)
            {
                if (site.FarmName == null)
                {
                    site.FarmName = string.Empty;
                }
                if (site.WebId == null)
                {
                    site.WebId = string.Empty;
                }
                if (site.WebURL == null)
                {
                    site.WebURL = string.Empty;
                }
                if (site.SiteId == null)
                {
                    site.SiteId = string.Empty;
                }
                if (site.SiteURL == null)
                {
                    site.SiteURL = string.Empty;
                }
            }
        }

        public List<CommonSiteMasterIndex> GetAllSiteCollectionNodsInfoByUrl(string url)
        {
            List<CommonSiteMasterIndex> domains = new List<CommonSiteMasterIndex>();
            using (var context = GetNewContext())
            {
                domains = context.CommonSiteMasterIndexes.AsQueryable().Where(n => n.SiteURL == url && n.MergeIndexState == (int)MergeIndexState.Succeed).OrderByDescending(n => n.ArchiverTime).ToList();
            }
            return domains;
        }

        public (Dictionary<string, double>, Dictionary<string, string>) GetAllTeamsArchivedSizeAndSiteURLs((long, long)? archivedTimeRange = null)
        {
            using (var context = GetNewContext())
            {
                int pageSize = 5000;
                int pageIndex = 0;
                var siteAndMailboxMapping = new Dictionary<string, string>();
                var teamsAndSizeMapping = new Dictionary<string, double>();
                do
                {
                    IQueryable<CommonSiteMasterIndex> query = context.CommonSiteMasterIndexes;
                    if(archivedTimeRange != null)
                    {
                        var (startTime, endTime) = archivedTimeRange.Value;
                        query = query.Where(s => s.MergeIndexState == (int)MergeIndexState.Succeed && s.ArchiverTime >= startTime && s.ArchiverTime <= endTime);
                    }
                    else
                    {
                        query = query.Where(s => s.MergeIndexState == (int)MergeIndexState.Succeed);
                    }

                    var results = query
                        .OrderBy(s => s.Id)
                        .Select(s => new { s.JobId, s.SiteURL, s.Extension })
                        .Skip(pageIndex * pageSize)
                        .Take(pageSize)
                        .ToList();

                    foreach (var teamsMasterIdx in results)
                    {
                        try
                        {
                            if(!teamsAndSizeMapping.TryGetValue(teamsMasterIdx.JobId, out var totalSize))
                            {
                                totalSize = 0;
                            }
                            teamsAndSizeMapping[teamsMasterIdx.SiteURL] = totalSize + ArchiverIndexSubInfoDao.GetAllArchivedSizeBySubJobIdInGB(teamsMasterIdx.JobId);

                            var extObj = SerializerHelper.DeserializeByDataContractSerializer<ArchiverGroupSiteMasterIndexExtension>(teamsMasterIdx.Extension);
                            if (!string.IsNullOrEmpty(extObj?.SPGroupSiteURL))
                            {
                                siteAndMailboxMapping[extObj.SPGroupSiteURL] = teamsMasterIdx.SiteURL;
                                var rootSiteUrl = new Uri(extObj.SPGroupSiteURL).GetLeftPart(UriPartial.Authority);

                                if(extObj.ChannelSiteRelativeURLs != null)
                                {
                                    foreach (var relativeUrl in extObj.ChannelSiteRelativeURLs)
                                    {
                                        siteAndMailboxMapping[rootSiteUrl + relativeUrl] = teamsMasterIdx.SiteURL;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Deserialize ArchiverGroupSiteMasterIndexExtension failed, {teamsMasterIdx.Extension}, {ex.Message}");
                        }
                    }

                    if (results.Count < pageSize)
                    {
                        break;
                    }

                    pageIndex++;
                } while (true);
                
                return (teamsAndSizeMapping, siteAndMailboxMapping);
            }
            
        }

        public Dictionary<string, List<string>> GetAllBackupTeamsDistinctJobIdMappings(List<string> mailboxAddress)
        {
            var SiteUrlJobIds = new Dictionary<string, List<string>>();
            using (var context = GetNewContext())
            {
                foreach (var address in mailboxAddress)
                {
                    var jobIds = context.CommonSiteMasterIndexes.Where(s => s.SiteURL == address && s.MergeIndexState == (int)MergeIndexState.Succeed).Select(s => s.JobId).ToList();
                    logger.Info($"Current site url is {address}, contains job count is {jobIds.Count}");
                    SiteUrlJobIds.Add(address, jobIds);
                }
            }
            return SiteUrlJobIds;
        }

        public Dictionary<string, List<string>> GetTeamsGroupWithRelatedSitesUrlMappings(List<string> mailboxAddress)
        {
            var teamsWithRelatedSites = new Dictionary<string, List<string>>();
            using (var context = GetNewContext())
            {
                foreach (var address in mailboxAddress)
                {
                    var relatedSites = GetRelatedSitesUrlByMailboxAddress(address);
                    logger.Info($"Current mailbox address is {address}, related sites: {string.Join(", \n", relatedSites)}");
                    teamsWithRelatedSites.Add(address, relatedSites);
                }
            }
            return teamsWithRelatedSites;
        }

        public List<ArchiverSiteMasterIndexContract> GetSiteCollectionStorageInfo(ArchiverSiteMasterIndexContract site)
        {
            List<ArchiverSiteMasterIndexContract> contract = null;
            List<CommonSiteMasterIndex> domains = null;
            this.ClearNullValue(site);
            using (var context = GetNewContext())
            {
                domains = context.CommonSiteMasterIndexes.AsQueryable().Where(s => s.SiteURL == site.SiteURL && s.MergeIndexState == (int)MergeIndexState.Succeed)
                    .OrderByDescending(s => s.ArchiverTime).ToList();
            }
            if (domains != null && domains.Count > 0)
            {
                contract = new List<ArchiverSiteMasterIndexContract>();
                domains.ForEach(a => contract.Add(ConvertToDto(a)));
            }
            return contract;
        }

        private List<string> GetRelatedSitesUrlByMailboxAddress(string mailboxAddress)
        {
            using (var context = GetNewContext())
            {
                var allIndex = GetAllSiteCollectionNodsInfoByUrl(mailboxAddress);
                var groupSiteUrl = string.Empty;
                HashSet<string> relatedSiteUrls = new HashSet<string>();
                string rootSiteUrl = null;
                foreach (var index in allIndex)
                {
                    var extObj = SerializerHelper.DeserializeByDataContractSerializer<ArchiverGroupSiteMasterIndexExtension>(index.Extension);
                    if (string.IsNullOrEmpty(extObj?.SPGroupSiteURL))
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(groupSiteUrl))
                    {
                        groupSiteUrl = extObj.SPGroupSiteURL;
                        rootSiteUrl = new Uri(extObj.SPGroupSiteURL).GetLeftPart(UriPartial.Authority);
                    }

                    relatedSiteUrls.Add(extObj.SPGroupSiteURL);

                    if (extObj.ChannelSiteRelativeURLs != null)
                    {
                        foreach (var relativeUrl in extObj.ChannelSiteRelativeURLs)
                        {
                            relatedSiteUrls.Add(rootSiteUrl + relativeUrl);
                        }
                    }
                }
                return relatedSiteUrls.ToList();
            }
        }

        public List<ArchiverSiteMasterIndexContract> GetIndexByJobId(string jobId)
        {
            List<ArchiverSiteMasterIndexContract> contract = null;
            List<CommonSiteMasterIndex> domains = null;
            using (var context = GetNewContext())
            {
                domains = context.CommonSiteMasterIndexes.AsQueryable().Where(s => s.JobId == jobId).OrderByDescending(s => s.ArchiverTime).ToList();
            }
            if (domains != null && domains.Count > 0)
            {
                contract = new List<ArchiverSiteMasterIndexContract>();
                domains.ForEach(a => contract.Add(ConvertToDto(a)));
            }
            return contract;
        }

        public string InsertIntoCommonSiteMasterIndex(ArchiverSiteMasterIndexContract indexDto)
        {
            string id = null;
            try
            {
                logger.Info("Insert into archiver site master index info from media site collection: {0}, job Id: {1}.", indexDto.SiteURL, indexDto.JobId);
                using (var context = GetNewContext())
                {
                    var existInfo = context.CommonSiteMasterIndexes.AsQueryable().Where(s => s.JobId == indexDto.JobId).ToList();
                    if (existInfo == null || existInfo.Count < 1)
                    {
                        logger.Info("Archiver site master Index with job Id {0} does not exist, create one.", indexDto.JobId);
                        var index = context.CommonSiteMasterIndexes.Add(ConvertToCommonSiteMasterIndex(indexDto));
                        id = index.Id;
                    }
                    else
                    {
                        logger.Info("Archiver site master index with job Id {0} already exists.", indexDto.JobId);
                        id = existInfo[0].Id;
                    }
                    if (indexDto.SubInfo != null)
                    {
                        foreach (ArchiverIndexSubInfoContract subInfo in indexDto.SubInfo)
                        {

                            //if (subInfo.StoragePolicyId != null && subInfo.StoragePolicyId != string.Empty)
                            //{
                            //    StoragePolicyDto dto = GetStoragePolicyInfo(subInfo.StoragePolicyId);
                            //    subInfo.ArchiverSubInfoExtension = new ArchiverSubInfoExtension();
                            //    if (dto != null && dto.RetentionOption != null)
                            //    {
                            //        logger.Info("Save archiver retention settings of storage policy {0} to index.", dto.Name);
                            //        subInfo.ArchiverSubInfoExtension.RetentionOption = dto.RetentionOption;
                            //        subInfo.ArchiverSubInfoExtension.RetentionOption.Schedule = null;
                            //        subInfo.ArchiverSubInfoExtension.PrimaryLogicalId = dto.PrimaryLogicalId;
                            //    }
                            //    subInfo.ArchiverSubInfoExtension.DataEncryptionInfo = subInfo.DataEncryptionInfo;
                            //}
                            //CreateSiteMasterSubIndex(subInfo);
                            ArchiverIndexSubInfo archiverIndexSubInfo = new ArchiverIndexSubInfo();
                            archiverIndexSubInfo.Id = subInfo.Id;
                            archiverIndexSubInfo.SubSubJobId = subInfo.JobId;
                            archiverIndexSubInfo.SubJobId = indexDto.JobId;
                            archiverIndexSubInfo.CurrentStorageId = subInfo.PhysicalDeviceId;
                            archiverIndexSubInfo.StorageId = subInfo.PhysicalDeviceId;
                            archiverIndexSubInfo.StorageInfo = subInfo.StorageInfo;
                            archiverIndexSubInfo.AgentDataSize = subInfo.AgentDataSize;
                            archiverIndexSubInfo.RetentionTime = indexDto.ArchiverTime;
                            archiverIndexSubInfo.RuleId = indexDto.RuleId;
                            archiverIndexSubInfo.SourceFlag = indexDto.SourceFlag;
                            archiverIndexSubInfo.DataFlag = indexDto.DataFlag;
                            archiverIndexSubInfo.RetentionCount = 1;
                            subInfo.ArchiverSubInfoExtension = new ArchiverSubInfoExtension();
                            if (!string.IsNullOrEmpty(subInfo.StoragePolicyId))
                            {
                                var storageInfo = context.RMStorageInfos.Where(s => s.Id.Equals(new Guid(subInfo.StoragePolicyId))).FirstOrDefault();
                                if (storageInfo != null && storageInfo.Retention != null)
                                {
                                    List<RetentionRule> rules = SerializerHelper.DeserializeByDataContractSerializer<List<RetentionRule>>(storageInfo.Retention);
                                    subInfo.ArchiverSubInfoExtension.RetentionOption = StorageDeviceConvert.ConvertToRetentionRuleOption(rules);
                                }
                            }
                            subInfo.ArchiverSubInfoExtension.DataEncryptionInfo = subInfo.DataEncryptionInfo;

                            archiverIndexSubInfo.Extension = SerializerHelper.SerializeByDataContractSerializer(subInfo.ArchiverSubInfoExtension);
                            context.ArchiverIndexSubInfos.Add(archiverIndexSubInfo);
                        }
                    }
                    else
                    {
                        logger.Info("Update site master index failed for there is no storage info in contract jobId: {0}", indexDto.JobId);
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw;
            }
            return id;
        }
        public async Task CreateByBulkCopyAsync(IEnumerable<ArchiverSiteMasterIndexContract> items)
        {
            if (items.Count() == 0)
            {
                return;
            }
            logger.Debug("Total add site master indexes: {0}", items.Count());
            using (new PerformanceScope("Batch site master indexes"))
            {
                var tableName = GetFullTableName();
                using (var table = ConvertToDataTable(items))
                {
                    table.TableName = tableName;
                    await BatchAddAsync(table, tableName);
                }
            }
        }

        private string GetFullTableName()
        {
            return $"[{SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}].[CommonSiteMasterIndexes]";
        }
        private DataTable ConvertToDataTable(IEnumerable<ArchiverSiteMasterIndexContract> items)
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(String));
            table.Columns.Add("ArchiverTime", typeof(Int64));
            table.Columns.Add("JobId", typeof(String));
            table.Columns.Add("SiteURL", typeof(String));
            table.Columns.Add("StorageId", typeof(String));
            table.Columns.Add("IndexStorageId", typeof(String));
            table.Columns.Add("SiteGroupId", typeof(String));
            table.Columns.Add("SiteId", typeof(String));
            table.Columns.Add("SPVersion", typeof(Int32));
            table.Columns.Add("MergeIndexState", typeof(Int32));
            table.Columns.Add("JobState", typeof(Int32));
            table.Columns.Add("StorageInfo", typeof(String));
            table.Columns.Add("Extension", typeof(String));
            table.Columns.Add("Flag", typeof(Int32));
            table.Columns.Add("DAOMigrated", typeof(Boolean));
            table.Columns.Add("BackupFileType", typeof(Int32));
            table.Columns.Add("DuplicateStatus", typeof(Int32));

            foreach (var item in items)
            {
                var row = table.NewRow();
                row["Id"] = item.Id;
                row["ArchiverTime"] = item.ArchiverTime;
                row["JobId"] = item.JobId;
                row["SiteURL"] = item.SiteURL;
                row["StorageId"] = item.StoragePolicyId;
                row["IndexStorageId"] = item.IndexDeviceId;
                row["SiteGroupId"] = item.WebId;
                row["SiteId"] = item.SiteId;
                row["SPVersion"] = item.SPVersion;
                row["MergeIndexState"] = (int)item.MergeIndexState;
                row["JobState"] = item.JobState;
                row["StorageInfo"] = item.StorageInfo;
                if (item.Extension != null)
                {
                    row["Extension"] = SerializerHelper.SerializeByDataContractSerializer(item.Extension);
                }
                row["Flag"] = 0;
                row["DAOMigrated"] = item.DAOMigrated;
                row["BackupFileType"] = item.BackupFileType;
                row["DuplicateStatus"] = item.DuplicateStatus;
                table.Rows.Add(row);
            }
            return table;
        }

        public async Task<int> DeleteMigratedSiteMasterIndexesAsync()
        {
            var sql = $"DELETE FROM {GetFullTableName()} WHERE DAOMigrated=1";

            using (var context = GetNewContext())
            {
                return await context.Database.ExecuteSqlCommandAsync(sql);
            }
        }
        public List<CommonSiteMasterIndex> GetAllTeamIndexInfoes()
        {
            List<CommonSiteMasterIndex> domains = new List<CommonSiteMasterIndex>();
            using (var context = GetNewContext())
            {
                domains = context.CommonSiteMasterIndexes.AsQueryable().Where(a => a.MergeIndexState == (int)MergeIndexState.Succeed && a.DataType == (int)SourceFlag.Teams).ToList();
            }
            return domains;
        }

        public bool ExistsTeamsGroupIndex()
        {
            using (var context = GetNewContext())
            {
                return context.CommonSiteMasterIndexes.Any(a =>
                    a.MergeIndexState == (int)MergeIndexState.Succeed
                    && a.DataType == (int)SourceFlag.Teams);
            }
        }

        public List<CommonSiteMasterIndex> GetAllCommonSiteMasterIndexes()
        {
            List<CommonSiteMasterIndex> domains = new List<CommonSiteMasterIndex>();
            using (var context = GetNewContext())
            {
                domains = context.CommonSiteMasterIndexes.AsQueryable().Where(a => (a.MergeIndexState == (int)MergeIndexState.Succeed || a.MergeIndexState == (int)MergeIndexState.DAOMigrated) && (a.JobId.StartsWith("SEA") || a.JobId.StartsWith("OEA") || a.JobId.StartsWith("AR") || a.JobId.StartsWith("SO") || a.JobId.StartsWith("DSO") || a.JobId.StartsWith("TEA"))).ToList();
            }
            return domains;
        }

        public async Task<(string, string)> GetExtensionAsync(string jobId)
        {
            using (var context = GetNewContext())
            {
                var result = await context.CommonSiteMasterIndexes
                    .Where(s => s.JobId == jobId)
                    .Select(s => new { s.Extension, s.Id })
                    .FirstOrDefaultAsync();
                return (result?.Extension, result?.Id);
            }
        }

        public async Task<bool> UpdateExtensionAsync(string indexId, string extension)
        {
            using (var context = GetNewContext())
            {
                var entity = context.CommonSiteMasterIndexes.FirstOrDefault(s => s.Id == indexId);
                entity.Extension = extension;
                var result = await context.SaveChangesAsync();
                return result > 0;
            }
        }

        public Dictionary<string, List<string>> GetAllBackupTeamsDistinctJobIdMappings(List<string> mailboxAddress, long startTime, long endTime)
        {
            var SiteUrlJobIds = new Dictionary<string, List<string>>();
            using (var context = GetNewContext())
            {
                foreach (var address in mailboxAddress)
                {
                    var jobIds = context.CommonSiteMasterIndexes.Where(s => s.SiteURL == address && s.MergeIndexState == (int)MergeIndexState.Succeed && s.ArchiverTime <= endTime && s.ArchiverTime >= startTime).Select(s => s.JobId).ToList();
                    logger.Info($"Current site url is {address}, contains job count is {jobIds.Count}");
                    SiteUrlJobIds.Add(address, jobIds);
                }
            }
            return SiteUrlJobIds;
        }

        public List<CommonSiteMasterIndex> GetTeamIndexInfoesByTimeRange(long startTime, long endTime)
        {
            List<CommonSiteMasterIndex> domains = new List<CommonSiteMasterIndex>();
            using (var context = GetNewContext())
            {
                domains = context.CommonSiteMasterIndexes.AsQueryable().Where(a => a.MergeIndexState == (int)MergeIndexState.Succeed && a.DataType == (int)SourceFlag.Teams && a.ArchiverTime <= endTime && a.ArchiverTime >= startTime).ToList();
            }
            return domains;
        }

        public async Task UpdateMergeIndexStateAsync(string jobId)
        {
            try
            {
                logger.Info($"Update merge index state for job id: {jobId}");
                var subInfoes = await ArchiverIndexSubInfoDao.FindListAsync(i => i.SubJobId == jobId);
                var mergeIndexState = (int)MergeIndexState.Succeed;

                if (!subInfoes.All(i => i.MergeIndexState == (int)MergeIndexState.Succeed))
                {
                    logger.Info($"Not all index sub info are Succeed");
                    mergeIndexState = (int)MergeIndexState.Failed;
                }
                var commonIndex = Find(i => i.JobId == jobId);
                if (commonIndex != null)
                {
                    commonIndex.MergeIndexState = (int)mergeIndexState;
                    await UpdateAsync(commonIndex);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Update merge index state failed: {ex}, sub job id:{jobId}");
                throw;
            }
        }

        public async Task<List<string>> GetAllRelatedSPSiteUrls(List<string> teamsIds)
        {
            List<string> relatedSites = new List<string>();
            using (var context = GetNewContext())
            {
                var emailAddresses = context.CommonSiteMasterIndexes.AsQueryable()
                    .Where(a => a.MergeIndexState == (int)MergeIndexState.Succeed && a.DataType == (int)SourceFlag.Teams && teamsIds.Contains(a.TeamId))
                    .Select(a => a.SiteURL)
                    .Distinct()
                    .ToList();
                emailAddresses.ForEach(e => relatedSites.AddRange(GetRelatedSitesUrlByMailboxAddress(e)));
            }
            return relatedSites;
        }
    }
}
