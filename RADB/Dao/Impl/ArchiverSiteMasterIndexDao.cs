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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.AveModuleContract;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class ArchiverSiteMasterIndexDao : BaseDao<ArchiverSiteMasterIndex>, IArchiverSiteMasterIndexDao
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(ArchiverSiteMasterIndexDao));

        private IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao = PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();

        public async Task<long> GetMaxArchiverTimeAsync()
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var index = await context.ArchiverSiteMasterIndexs.OrderByDescending(item => item.ArchiverTime).FirstOrDefaultAsync();
            return index == null ? 0 : index.ArchiverTime;
        }

        public async IAsyncEnumerable<ArchiverSiteMasterIndexContract> GetSiteMasterIndexesAsync(long minArchiverTime, long maxArchiverTime)
        {
            const int pageSize = 1000;
            using var context = RMDBContextManager.GetNewDBContext();
            for (var i = 0; ; i++)
            {
                var indexes = await context.ArchiverSiteMasterIndexs.Where(item => item.ArchiverTime > minArchiverTime && item.ArchiverTime <= maxArchiverTime)
                    .OrderBy(item => item.ArchiverTime)
                    .Skip(i * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                foreach (var index in indexes)
                {
                    yield return ConvertToDto(index);
                }

                if (indexes.Count < pageSize)
                {
                    break;
                }
            }
        }

        public async IAsyncEnumerable<ArchiverSiteMasterIndexContract> GetSiteMasterIndexesAsync(string siteUrl, long minArchiverTime, long maxArchiverTime)
        {
            const int pageSize = 1000;
            using var context = RMDBContextManager.GetNewDBContext();
            for (var i = 0; ; i++)
            {
                var indexes = await context.ArchiverSiteMasterIndexs.Where(item =>
                    item.SiteURL == siteUrl
                    && item.ArchiverTime > minArchiverTime 
                    && item.ArchiverTime <= maxArchiverTime)
                    .OrderBy(item => item.ArchiverTime)
                    .Skip(i * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                foreach (var index in indexes)
                {
                    yield return ConvertToDto(index);
                }

                if (indexes.Count < pageSize)
                {
                    break;
                }
            }
        }

        public async IAsyncEnumerable<ArchiverSiteMasterIndexContract> GetSiteMasterIndexesBySiteUrlsAsync(
            IEnumerable<string> siteUrls,
            long minArchiverTime,
            long maxArchiverTime)
        {
            const int pageSize = 1000;
            var urlList = siteUrls?.Where(url => !string.IsNullOrWhiteSpace(url)).Distinct().ToList() ?? new List<string>();
            if (urlList.Count == 0)
            {
                yield break;
            }

            foreach (var urlBatch in BatchStringList(urlList, 1000))
            {
                using var context = RMDBContextManager.GetNewDBContext();
                for (var i = 0; ; i++)
                {
                    var indexes = await context.ArchiverSiteMasterIndexs.Where(item =>
                            urlBatch.Contains(item.SiteURL)
                            && item.ArchiverTime > minArchiverTime
                            && item.ArchiverTime <= maxArchiverTime)
                        .OrderBy(item => item.ArchiverTime)
                        .Skip(i * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
                    foreach (var index in indexes)
                    {
                        yield return ConvertToDto(index);
                    }

                    if (indexes.Count < pageSize)
                    {
                        break;
                    }
                }
            }
        }

        private static IEnumerable<List<string>> BatchStringList(List<string> items, int batchSize)
        {
            for (var i = 0; i < items.Count; i += batchSize)
            {
                yield return items.Skip(i).Take(batchSize).ToList();
            }
        }

        public async Task<int> CountSiteMasterIndexesAsync(long minArchiverTime, long maxArchiverTime)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return await context.ArchiverSiteMasterIndexs
                .Where(item => item.ArchiverTime > minArchiverTime && item.ArchiverTime <= maxArchiverTime)
                .CountAsync();
        }

        public async Task<(bool Has, ArchiverSiteMasterIndexContract indexContract)> TryGetSiteMasterIndexAsync(string jobId)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var index = await context.ArchiverSiteMasterIndexs.FirstOrDefaultAsync(item => item.JobId == jobId);
            return (index != null, ConvertToDto(index));
        }

        public ArchiverSiteMasterIndexContract GetSiteCollectionInfo(ArchiverSiteMasterIndexContract site)
        {
            ArchiverSiteMasterIndexContract contract = null;
            this.ClearNullValue(site);
            List<ArchiverSiteMasterIndex> domains = null;
            using (var context = GetNewContext())
            {
                domains = context.ArchiverSiteMasterIndexs.AsQueryable().Where(s => s.SiteURL == site.SiteURL).ToList();
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
                var entities = context.ArchiverSiteMasterIndexs.AsQueryable().Where(s => s.JobId == jobId).ToList();
                foreach (var entity in entities)
                {
                    entity.JobState = status;
                }

                this.BatchUpdate(entities);
            }
        }
        private  ArchiverSiteMasterIndexContract ConvertToDto(ArchiverSiteMasterIndex domain)
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
                contract.GroupMailboxAddress = domain.GroupMailboxAddress;
                if (domain.Extension != null)
                {
                    contract.Extension = SerializerHelper.DeserializeByDataContractSerializer<ArchiverSiteMasterIndexExtension>(domain.Extension);
                }
            }
            return contract;
        }

        private ArchiverSiteMasterIndex ConvertToArchiverSiteMasterIndex(ArchiverSiteMasterIndexContract contract)
        {
            ArchiverSiteMasterIndex domain = null;
            if (contract != null)
            {
                domain = new ArchiverSiteMasterIndex();
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
                domain.GroupMailboxAddress = contract.GroupMailboxAddress;
                domain.O365TenantId = contract.O365TenantId;
                if (contract.Extension != null)
                {
                    domain.Extension = SerializerHelper.SerializeByDataContractSerializer(contract.Extension);
                }
            }

            if(contract.SourceFlag == (int)SourceFlag.Google) domain.Flag = (int)SourceFlag.Google;

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
                if (site.GroupMailboxAddress == null)
                {
                    site.GroupMailboxAddress = string.Empty;
                }
            }
        }

        public List<ArchiverSiteMasterIndex> GetAllSiteCollectionNodsInfo(List<int> flagIgnores = null)
        {
            List<ArchiverSiteMasterIndex> domains = new List<ArchiverSiteMasterIndex>();
            flagIgnores ??= [];
            using (var context = GetNewContext())
            {
                domains = context.ArchiverSiteMasterIndexs.AsQueryable().Where(a=>a.MergeIndexState == (int)MergeIndexState.Succeed && !flagIgnores.Contains(a.Flag)).ToList();
            }
            return domains;
        }
        public List<ArchiverSiteMasterIndex> GetAllSiteCollectionNodsInfoByUrl(string url)
        {
            List<ArchiverSiteMasterIndex> domains = new List<ArchiverSiteMasterIndex>();
            using (var context = GetNewContext())
            {
                domains = context.ArchiverSiteMasterIndexs.AsQueryable().Where(n=>n.SiteURL==url).OrderByDescending(n=>n.ArchiverTime).ToList();
            }
            return domains;
        }
        public async Task<bool> ExistsArchivedDataAsync(string siteURL)
        {
            using var context = GetNewContext();
            return await context.ArchiverSiteMasterIndexs.AnyAsync(n => n.SiteURL == siteURL);
        }

        public bool ExistsRestoringSiteCollectionByUrl(string url)
        {
            using (var context = GetNewContext())
            {
                return context.ArchiverSiteMasterIndexs.Any(n =>
                    n.SiteURL == url && n.MergeIndexState == (int)MergeIndexState.Succeed);
            }
        }

        public ArchiverSiteMasterIndex GetRestoringSiteCollectionInfoByUrl(string url)
        {
            using (var context = GetNewContext())
            {
                return context.ArchiverSiteMasterIndexs.AsQueryable()
                    .Where(n => n.SiteURL == url && n.MergeIndexState == (int)MergeIndexState.Succeed)
                    .ToList()
                    .OrderByDescending(n => n.ArchiverTime)
                    .FirstOrDefault();
            }
        }

        public string GetSiteIdByUrl(string url)
        {
            using (var context = GetNewContext())
            {
                var domain = context.ArchiverSiteMasterIndexs.FirstOrDefault(n => n.SiteURL == url);
                return domain?.SiteId;
            }
        }

        public List<string> GetAllBackupSiteCollectionDistinctUrl()
        {
            List<string> domains = new List<string>();
            using (var context = GetNewContext())
            {
                domains = context.ArchiverSiteMasterIndexs.Where(s =>  s.Flag == 0).AsQueryable().Select(s => s.SiteURL).Distinct().ToList();
            }
            return domains;
        }
        public List<ArchiverSiteMasterIndexContract> GetAllBackupGoogleDriveIndexs()
        {
            using (var context = GetNewContext())
            {
                return context.ArchiverSiteMasterIndexs.Where(s => s.Flag == 9).GroupBy(s => new { s.SiteId, s.SiteGroupId, s.SiteURL } ).Select(g => new ArchiverSiteMasterIndexContract() { WebId = g.Key.SiteGroupId, SiteId = g.Key.SiteId,SiteURL = g.Key.SiteURL }).ToList();
            }
        }
        public Dictionary<string, List<string>> GetAllBackupSiteCollectionDistinctJobIdMappings(List<string> siteUrls)
        {
            var SiteUrlJobIds = new Dictionary<string, List<string>>();
            using (var context = GetNewContext())
            {
                foreach(var siteUrl in siteUrls)
                {
                    var jobIds = context.ArchiverSiteMasterIndexs.Where(s => s.SiteURL == siteUrl && s.Flag != (int)SourceFlag.Google).Select(s => s.JobId).ToList();
                    logger.Info($"Current site url is {siteUrl}, contains job count is {jobIds.Count}");
                    SiteUrlJobIds.Add(siteUrl, jobIds);
                }
            }
            return SiteUrlJobIds;
        }
        public Dictionary<string, List<string>> GetAllBackupGDriveDistinctJobIdMappings(List<string> driveIds)
        {
            var driveIdWithJobId = new Dictionary<string, List<string>>();
            using (var context = GetNewContext())
            {
                foreach (var driveId in driveIds)
                {
                    var jobIds = context.ArchiverSiteMasterIndexs.Where(s => s.SiteId == driveId && s.Flag == (int)SourceFlag.Google).Select(s => s.JobId).ToList();
                    logger.Info($"Current drive is {driveId}, contains job count is {jobIds.Count}");
                    driveIdWithJobId.Add(driveId, jobIds);
                }
            }
            return driveIdWithJobId;
        }

        public Dictionary<string, List<string>> GetAllBackupSiteCollectionDistinctJobIdMappings(List<string> siteUrls, long startTime, long endTime)
        {
            var SiteUrlJobIds = new Dictionary<string, List<string>>();
            using (var context = GetNewContext())
            {
                foreach (var siteUrl in siteUrls)
                {
                    var jobIds = context.ArchiverSiteMasterIndexs.Where(s => s.SiteURL == siteUrl && s.ArchiverTime <= endTime && s.ArchiverTime >= startTime).Select(s => s.JobId).ToList();
                    logger.Info($"Current site url is {siteUrl}, contains job count is {jobIds.Count}");
                    SiteUrlJobIds.Add(siteUrl, jobIds);
                }
            }
            return SiteUrlJobIds;
        }

        public Dictionary<string, (double archivedSizeInGB, string groupMailboxAddress)> GetAllSiteArchivedSizeInGBAndGroupMailBox(long startTime, long endTime)
        {
            var siteArchivedSize = new Dictionary<string, (double archivedSizeInGB, string groupMailboxAddress)>();

            int pageIndex = 0;
            int pageSize = 5000;
            using var context = GetNewContext();
            do
            {

                var results = context.ArchiverSiteMasterIndexs
                    .Where(s => s.ArchiverTime >= startTime && s.ArchiverTime <= endTime)
                    .OrderBy(s => s.Id)
                    .Select(s => new { s.SiteURL, s.JobId, s.GroupMailboxAddress })
                    .Skip(pageIndex * pageSize)
                    .ToList();
                
                foreach (var item in results)
                {
                    siteArchivedSize.TryGetValue(item.SiteURL, out var existing);
                    var groupMailbox = string.IsNullOrEmpty(item.GroupMailboxAddress) ? string.Empty : item.GroupMailboxAddress;

                    siteArchivedSize[item.SiteURL] = (existing.archivedSizeInGB + ArchiverIndexSubInfoDao.GetAllArchivedSizeBySubJobIdInGB(item.JobId), groupMailbox);
                }

                if(results.Count < pageSize)
                {
                    break;
                }

                pageIndex++;

            } while (true);

            return siteArchivedSize;
        }

        public Dictionary<string, double> GetSiteArchivedSizeInGB()
        {
            var result = new Dictionary<string, double>();
            using var context = GetNewContext();

            const int pageSize = 2000;
            var siteBytes = new Dictionary<string, long>();

            var query = context.ArchiverSiteMasterIndexs
                .Where(i => i.MergeIndexState == (int)MergeIndexState.Succeed)
                .OrderBy(i => i.Id);

            int pageIndex = 0;

            while (true)
            {
                var pageRecords = query
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .Select(i => new { i.SiteURL, i.JobId })
                    .ToList();

                if (pageRecords.Count == 0)
                {
                    break;
                }

                var siteGroups = pageRecords
                    .GroupBy(i => i.SiteURL)
                    .ToList();

                var jobIds = siteGroups
                    .SelectMany(g => g.Select(x => x.JobId))
                    .Where(id => !string.IsNullOrEmpty(id))
                    .Distinct()
                    .ToList();

                var jobIdToSizeBytes = jobIds.Count == 0
                    ? new Dictionary<string, long>()
                    : context.ArchiverIndexSubInfos
                        .Where(i => jobIds.Contains(i.SubJobId))
                        .GroupBy(i => i.SubJobId)
                        .Select(g => new { JobId = g.Key, TotalSize = g.Sum(x => x.MediaDataSize) })
                        .ToList()
                        .ToDictionary(k => k.JobId, v => v.TotalSize);

                foreach (var group in siteGroups)
                {
                    long totalBytes = 0;
                    foreach (var record in group)
                    {
                        if (string.IsNullOrEmpty(record.JobId))
                        {
                            continue;
                        }

                        if (jobIdToSizeBytes.TryGetValue(record.JobId, out var size))
                        {
                            totalBytes += size;
                        }
                    }

                    siteBytes[group.Key] = siteBytes.TryGetValue(group.Key, out var existingBytes)
                        ? existingBytes + totalBytes
                        : totalBytes;
                }

                pageIndex++;
            }

            foreach (var siteAndArchiveSize in siteBytes)
            {
                double sizeGB = (double)siteAndArchiveSize.Value / ContractConstants.GBSizeInterval;
                result[siteAndArchiveSize.Key] = sizeGB;
                logger.Info($"Site URL [{siteAndArchiveSize.Key}], Size: {sizeGB} GB]");
            }

            return result;
        }



        public List<ArchiverSiteMasterIndexContract> GetSiteCollectionStorageInfo(ArchiverSiteMasterIndexContract site)
        {
            List<ArchiverSiteMasterIndexContract> contract = null;
            List<ArchiverSiteMasterIndex> domains = null;
            this.ClearNullValue(site);
            //string QueryString = "select value a from ArchiverSiteMasterIndex as a where (a.SiteURL=@siteUrl or (a.SiteId <> '' and a.SiteId=@siteId)) order by a.ArchiverTime desc";
            //ObjectParameter param_SiteURL = new ObjectParameter("siteUrl", site.SiteURL);
            //ObjectParameter param_SiteId = new ObjectParameter("siteId", site.SiteId);
            using (var context = GetNewContext())
            {
                domains = context.ArchiverSiteMasterIndexs.AsQueryable().Where(s => s.SiteURL == site.SiteURL || (!string.IsNullOrEmpty(s.SiteId) && s.SiteId == site.SiteId)).OrderByDescending(s=>s.ArchiverTime).ToList();///(QueryString, param_SiteURL, param_SiteId);
            }
            if (domains != null && domains.Count > 0)
            {
                contract = new List<ArchiverSiteMasterIndexContract>();
                domains.ForEach(a => contract.Add(ConvertToDto(a)));
            }
            return contract;
        }

        public List<ArchiverSiteMasterIndexContract> GetGDriveStorageInfo(ArchiverSiteMasterIndexContract site)
        {
            List<ArchiverSiteMasterIndexContract> contract = null;
            List<ArchiverSiteMasterIndex> domains = null;
            this.ClearNullValue(site);
            using (var context = GetNewContext())
            {
                domains = context.ArchiverSiteMasterIndexs.AsQueryable().Where(s =>  s.SiteId == site.SiteId && s.Flag == (int) SourceFlag.Google).OrderByDescending(s => s.ArchiverTime).ToList();
            }
            if (domains != null && domains.Count > 0)
            {
                contract = new List<ArchiverSiteMasterIndexContract>();
                domains.ForEach(a => contract.Add(ConvertToDto(a)));
            }
            return contract;
        }

        public bool IsFileLevelBlockBackup(string jobId)
        {
            using (var context = GetNewContext())
            {
                var result = context.ArchiverSiteMasterIndexs
                    .Where(s => s.JobId == jobId)
                    .Select(s => s.BackupFileType)
                    .FirstOrDefault();
                return result == (int)BackupFileType.File || result == (int)BackupFileType.RecordsFile;
            }
        }

        public List<ArchiverSiteMasterIndexContract> GetIndexByJobId(string jobId)
        {
            List<ArchiverSiteMasterIndexContract> contract = null;
            List<ArchiverSiteMasterIndex> domains = null;
            using (var context = GetNewContext())
            {
                domains = context.ArchiverSiteMasterIndexs.AsQueryable().Where(s => s.JobId == jobId).OrderByDescending(s => s.ArchiverTime).ToList();///(QueryString, param_SiteURL, param_SiteId);
            }
            if (domains != null && domains.Count > 0)
            {
                contract = new List<ArchiverSiteMasterIndexContract>();
                domains.ForEach(a => contract.Add(ConvertToDto(a)));
            }
            return contract;
        }

        public string InsertIntoArchiverSiteMasterIndex(ArchiverSiteMasterIndexContract indexDto)
        {
            string id = null;
            try
            {
                logger.Info("Insert into archiver site master index info from media site collection: {0}, job Id: {1}.", indexDto.SiteURL, indexDto.JobId);
                using (var context = GetNewContext())
                {
                    var existInfoId = context.ArchiverSiteMasterIndexs.AsNoTracking().AsQueryable()
                        .Where(s => s.JobId == indexDto.JobId).Select(s => s.Id).FirstOrDefault();
                    if (string.IsNullOrEmpty(existInfoId))
                    {
                        logger.Info("Archiver site master Index with job Id {0} does not exist, create one.", indexDto.JobId);
                        var index = context.ArchiverSiteMasterIndexs.Add(ConvertToArchiverSiteMasterIndex(indexDto));
                        id = index.Id;
                    }
                    else
                    {
                        logger.Info("Archiver site master index with job Id {0} already exists.", indexDto.JobId);
                        id = existInfoId;
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

        public List<ArchiverSiteMasterIndex> GetGetAllWithMoveDataTierFlagAndArchiverTime(int interval)
        {
            using (var context = GetNewContext())
            {
                var temp = TimeSpan.FromDays(interval).Ticks;
                var timeNow = DateTime.UtcNow.Ticks;
                var result = context.ArchiverSiteMasterIndexs.AsQueryable().Where(a => a.MergeIndexState == (int)MergeIndexState.Succeed && (a.ArchiverTime + temp < timeNow) && (a.Flag & (int)ArchiverSiteMasterIndexFlag.MoveDataTier) != (int)ArchiverSiteMasterIndexFlag.MoveDataTier).ToList();
                return result;
            }
        }

        public void SetMoveDateTierFlag(string jobId)
        {
            using (var context = GetNewContext())
            {
                var entities = context.ArchiverSiteMasterIndexs.AsQueryable().Where(s => s.JobId == jobId).ToList();
                foreach (var entity in entities)
                {
                    entity.Flag = entity.Flag+(int)ArchiverSiteMasterIndexFlag.MoveDataTier;
                }
                this.BatchUpdate(entities);
            }
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
            return $"[{SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}].[ArchiverSiteMasterIndexes]";
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

        public List<ArchiverSiteMasterIndex> GetAllDisposalJobNodsInfo(long timeOlder)
        {
            List<ArchiverSiteMasterIndex> domains = new List<ArchiverSiteMasterIndex>();
            using (var context = GetNewContext())
            {
                domains = context.ArchiverSiteMasterIndexs.AsQueryable().Where(a => (a.MergeIndexState == (int)MergeIndexState.Succeed || a.MergeIndexState == (int)MergeIndexState.DAOMigrated) && (a.ArchiverTime>= timeOlder) && (a.JobId.StartsWith("SEA") || a.JobId.StartsWith("OEA") || a.JobId.StartsWith("AR") || a.JobId.StartsWith("SO") || a.JobId.StartsWith("DSO"))).ToList();
            }
            return domains;
        }

        public void UpdateArchiverMasterIndexDeduplicatedState(IEnumerable<string> idList)
        {
            using (var context = GetNewContext())
            {
                DatabaseUtility.BatchOperation(idList, (batchIDs) =>
                {
                    string sql = $"UPDATE {GetFullTableName()} SET DuplicateStatus=@DeduplicatedStatus WHERE Id IN {DatabaseUtility.BuildInClause(batchIDs, out var sqlParameters)}";
                    sqlParameters.Add(new SqlParameter("@DeduplicatedStatus", (int)DeDuplicateStatus.FinishDeDuplicate));
                    context.Database.ExecuteSqlCommand(
                        sql,
                        sqlParameters.ToArray());
                }, 200);
            }
        }

        private class SiteMasterTempInfo
        {
            public string Id { get; set; }
            public string SiteURL { get; set; }
        }
        public async Task<Dictionary<string, List<string>>> GetAllUnDedupArchiverSiteMasterIndexesAsync()
        {
            var allSiteMasterIndexesMappings = new Dictionary<string, List<string>>();
            int resultsCount = 0;
            int skipCount = 0;
            int pagerSize = 5000;
            using (var context = GetNewContext())
            {
                do
                {
                    var results = await context.ArchiverSiteMasterIndexs
                        .Where(a => a.MergeIndexState == 2 && a.DuplicateStatus <= 1)
                        .OrderBy(a => a.Id)
                        .Skip(skipCount).Take(pagerSize)
                        .Select(a => new { a.SiteURL, a.Id })
                        .ToListAsync();
                    resultsCount = results.Count;
                    skipCount += resultsCount;

                    foreach (var item in results)
                    {
                        AddToSiteMasterIndexesMappings(allSiteMasterIndexesMappings, item.SiteURL, item.Id);
                    }

                } while (resultsCount == pagerSize);
            }
            return allSiteMasterIndexesMappings;
        }
        private void AddToSiteMasterIndexesMappings(Dictionary<string, List<string>> mappings, string siteUrl, string masterIndexId)
        {
            if (!mappings.TryGetValue(siteUrl, out var masterIndexIDs))
            {
                masterIndexIDs = new List<string>();
                mappings[siteUrl] = masterIndexIDs;
            }
            masterIndexIDs.Add(masterIndexId);
        }
        public Dictionary<string, List<string>> GetAllUnDedupArchiverSiteMasterIndexes(IEnumerable<string> siteURLs)
        {
            var allSiteMasterIndexesMappings = new Dictionary<string, List<string>>();
            using (var context = GetNewContext())
            {
                DatabaseUtility.BatchOperation(siteURLs, (batchURLs) =>
                {
                    string sql = 
@$"SELECT [Id],[SiteURL] FROM {GetFullTableName()} 
WHERE [MergeIndexState] = 2 AND [DuplicateStatus] <= 1 
  AND SiteURL IN {DatabaseUtility.BuildInClause(batchURLs, out var sqlParameters)}";
                    sqlParameters.Add(new SqlParameter("@DeduplicatedStatus", (int)DeDuplicateStatus.FinishDeDuplicate));
                    var data = context.Database.SqlQuery<SiteMasterTempInfo>(
                        sql,
                        sqlParameters.ToArray());

                    foreach (var item in data)
                    {
                        AddToSiteMasterIndexesMappings(allSiteMasterIndexesMappings, item.SiteURL, item.Id);
                    }
                }, 200);
            }
            return allSiteMasterIndexesMappings;
        }

        public string GetSiteId(string masterIndexId)
        {
            using (var context = GetNewContext())
            {
                return context.ArchiverSiteMasterIndexs
                    .Where(s => s.Id == masterIndexId)
                    .Select(s => s.SiteId)
                    .FirstOrDefault();
            }
        }

        public List<ArchiverSiteMasterIndex> GetAllSiteMastersInfo()
        {
            List<ArchiverSiteMasterIndex> domains = new List<ArchiverSiteMasterIndex>();
            using (var context = GetNewContext())
            {
                domains = context.ArchiverSiteMasterIndexs.AsQueryable().Where(a => (a.MergeIndexState == (int)MergeIndexState.Succeed || a.MergeIndexState == (int)MergeIndexState.DAOMigrated) && (a.JobId.StartsWith("SEA") || a.JobId.StartsWith("OEA") || a.JobId.StartsWith("AR") || a.JobId.StartsWith("SO") || a.JobId.StartsWith("DSO") || a.JobId.StartsWith("TEA"))).ToList();
            }
            return domains;
        }

        public List<ArchiverSiteMasterIndex> GetSiteMastersInfoByMainJobId(string mainJobId)
        {
            List<ArchiverSiteMasterIndex> domains = new List<ArchiverSiteMasterIndex>();
            using (var context = GetNewContext())
            {
                domains = context.ArchiverSiteMasterIndexs.AsQueryable().Where(a => a.JobId.StartsWith(mainJobId)).ToList();
            }
            return domains;
        }

        public List<ArchiverSiteMasterIndex> GetSiteMastersInfoByJobIds(List<string> jobIds)
        {
            if(jobIds == null || jobIds.Count ==0)
            {
                return new List<ArchiverSiteMasterIndex>();
            }

            List<ArchiverSiteMasterIndex> domains = new List<ArchiverSiteMasterIndex>();
            using (var context = GetNewContext())
            {
                foreach(var batch in jobIds.Batch(500))
                {
                    var tempDomains = context.ArchiverSiteMasterIndexs.AsQueryable().Where(a => batch.Contains(a.JobId)).ToList();
                    domains.AddRange(tempDomains);
                }
            }
            return domains;
        }

        public void UpdateGroupMailboxAddressBySiteURL(IEnumerable<string> siteURLs, string groupMailboxAddress)
        {
            if (string.IsNullOrEmpty(groupMailboxAddress) || siteURLs == null || !siteURLs.Any())
            {
                logger.Warn($"Invalid. GroupMailboxAddress: [{groupMailboxAddress}], siteURLs count: {siteURLs?.Count() ?? 0}");
                return;
            }

            using var context = GetNewContext();

            DatabaseUtility.BatchOperation(siteURLs, batch =>
            {
                var sql = $"UPDATE {GetFullTableName()} SET GroupMailboxAddress = @Address " +
                          $"WHERE SiteURL IN {DatabaseUtility.BuildInClause(batch, out var sqlParams)}";

                sqlParams.Add(new SqlParameter("@Address", groupMailboxAddress));

                context.Database.ExecuteSqlCommand(sql, sqlParams.ToArray());
            }, 200);
        }

        public Dictionary<string, List<string>> GetAllBackSiteCollectionGroupMailboxMapping()
        {
            var result = new Dictionary<string, List<string>>();

            using var context = GetNewContext();

            var records = context.ArchiverSiteMasterIndexs
                .Where(i => i.MergeIndexState == (int)MergeIndexState.Succeed 
                            && !string.IsNullOrEmpty(i.GroupMailboxAddress) 
                            && !string.IsNullOrEmpty(i.SiteURL))
                .Select(i => new { i.GroupMailboxAddress, i.SiteURL })
                .Distinct()
                .ToList();

            result = records
                .GroupBy(r => r.GroupMailboxAddress)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.SiteURL).Distinct().ToList()
                );

            return result;
        }

        public async IAsyncEnumerable<IEnumerable<string>> GetAllSiteDistinctUrlAsync()
        {
            using var context = GetNewContext();
            context.Database.CommandTimeout = 900;
            var batchCount = 500;
            string lastSiteUrl = null;

            while (true)
            {
                IQueryable<ArchiverSiteMasterIndex> query = context.ArchiverSiteMasterIndexs.AsNoTracking();
                if (!string.IsNullOrEmpty(lastSiteUrl))
                {
                    query = query.Where(x => string.Compare(x.SiteURL, lastSiteUrl) > 0);
                }

                var batch = await query
                    .OrderBy(x => x.SiteURL)
                    .Select(x => x.SiteURL)
                    .Take(batchCount)
                    .ToListAsync();

                if (batch.Count > 0) yield return batch.ToHashSet();

                if (batch.Count < batchCount) yield break;

                lastSiteUrl = batch.Last();
            }
        }

        public async Task<(List<ArchiverSiteMasterIndex> Items, int TotalCount)> GetSiteCollectionNodesByFilterAsync(
            IEnumerable<Guid> containerIds,
            string filterKeyword,
            int pageIndex,
            int pageSize,
            bool filterByContainers)
        {
            List<string> normalizedContainerIds = NormalizeContainerIds(containerIds);

            if (filterByContainers && normalizedContainerIds.Count == 0)
            {
                return (new List<ArchiverSiteMasterIndex>(), 0);
            }

            using var context = GetNewContext();

            IQueryable<string> distinctSiteQuery = BuildDistinctSiteQuery(
                context,
                normalizedContainerIds,
                filterByContainers,
                filterKeyword);

            int totalCount = await distinctSiteQuery.CountAsync();
            int skip = CalculateSkip(pageIndex, pageSize);
            if (skip >= totalCount)
            {
                return (new List<ArchiverSiteMasterIndex>(), totalCount);
            }

            List<string> pageSiteUrls = await GetPagedSiteUrlsAsync(distinctSiteQuery, skip, pageSize);

            if (pageSiteUrls.Count == 0)
            {
                return (new List<ArchiverSiteMasterIndex>(), totalCount);
            }

            List<ArchiverSiteMasterIndex> pageRecords = await FetchLatestRecordsForSitesAsync(context, pageSiteUrls);

            return (pageRecords.OrderBy(site => site.SiteURL).ToList(), totalCount);
        }

        private static List<string> NormalizeContainerIds(IEnumerable<Guid> containerIds)
        {
            if (containerIds == null)
            {
                return new List<string>();
            }

            return containerIds
                .Where(id => id != Guid.Empty)
                .Select(id => id.ToString())
                .Distinct()
                .ToList();
        }

        private IQueryable<ArchiverSiteMasterIndex> BuildFilteredSiteQuery(
            RMDbContext context,
            List<string> normalizedContainerIds,
            bool filterByContainers,
            string filterKeyword)
        {
            IQueryable<ArchiverSiteMasterIndex> query = context.ArchiverSiteMasterIndexs
                .AsNoTracking()
                .Where(x => x.MergeIndexState == (int)MergeIndexState.Succeed && x.Flag != (int)SourceFlag.Google);

            query = ApplyKeywordFilter(query, filterKeyword);
            query = ApplyContainerFilter(context, query, normalizedContainerIds, filterByContainers);

            return query;
        }

        private static IQueryable<ArchiverSiteMasterIndex> ApplyKeywordFilter(
            IQueryable<ArchiverSiteMasterIndex> query,
            string filterKeyword)
        {
            (string processedKeyword, bool useExactMatch) = NormalizeSiteFilterKeyword(filterKeyword);

            if (string.IsNullOrEmpty(processedKeyword))
            {
                return query;
            }

            if (useExactMatch)
            {
                return query.Where(x => x.SiteURL == processedKeyword);
            }

            return query.Where(x => System.Data.Entity.DbFunctions.Like(x.SiteURL, processedKeyword) == true);
        }

        private static IQueryable<ArchiverSiteMasterIndex> ApplyContainerFilter(
            RMDbContext context,
            IQueryable<ArchiverSiteMasterIndex> query,
            List<string> normalizedContainerIds,
            bool filterByContainers)
        {
            if (!filterByContainers || normalizedContainerIds.Count == 0)
            {
                return query;
            }

            IQueryable<string> accessibleSiteQuery = context.RMRemoteNodes
                .AsNoTracking()
                .Where(node => normalizedContainerIds.Contains(node.ParentId) && node.Url != null)
                .Select(node => node.Url)
                .Distinct();

            return query.Where(x => accessibleSiteQuery.Contains(x.SiteURL));
        }

        private IQueryable<string> BuildDistinctSiteQuery(
            RMDbContext context,
            List<string> normalizedContainerIds,
            bool filterByContainers,
            string filterKeyword)
        {
            IQueryable<ArchiverSiteMasterIndex> filteredQuery = BuildFilteredSiteQuery(
                context,
                normalizedContainerIds,
                filterByContainers,
                filterKeyword);

            return filteredQuery.Select(x => x.SiteURL).Distinct();
        }

        private static int CalculateSkip(int pageIndex, int pageSize)
        {
            return (pageIndex - 1) * pageSize;
        }

        private static async Task<List<string>> GetPagedSiteUrlsAsync(
            IQueryable<string> distinctSiteQuery,
            int skip,
            int pageSize)
        {
            return await distinctSiteQuery
                .OrderBy(url => url)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        private async Task<List<ArchiverSiteMasterIndex>> FetchLatestRecordsForSitesAsync(
            RMDbContext context,
            List<string> pageSiteUrls)
        {
            if (pageSiteUrls == null || pageSiteUrls.Count == 0)
            {
                return new List<ArchiverSiteMasterIndex>();
            }

            string sql = $@"
SELECT *
FROM (
    SELECT *,
           ROW_NUMBER() OVER (PARTITION BY [SiteURL] ORDER BY [ArchiverTime] DESC, [Id] DESC) AS [rn]
    FROM {GetFullTableName()}
    WHERE [MergeIndexState] = @SucceedState
      AND [Flag] <> @GoogleFlag
      AND [SiteURL] IN {DatabaseUtility.BuildInClause(pageSiteUrls, out var sqlParameters)}
) AS RankedSites
WHERE RankedSites.[rn] = 1";

            sqlParameters.Add(new("@SucceedState", (int)MergeIndexState.Succeed));
            sqlParameters.Add(new("@GoogleFlag", (int)SourceFlag.Google));
            var result = await context.Database
                .SqlQuery<ArchiverSiteMasterIndex>(sql, sqlParameters.ToArray())
                .ToListAsync();
            return result;
        }

        private static (string ProcessedKeyword, bool UseExactMatch) NormalizeSiteFilterKeyword(string filterKeyword)
        {
            if (string.IsNullOrEmpty(filterKeyword))
            {
                return (null, false);
            }

            if (filterKeyword.Length >= 2 && filterKeyword.StartsWith("\"") && filterKeyword.EndsWith("\""))
            {
                string exactKeyword = filterKeyword.Substring(1, filterKeyword.Length - 2);
                return (exactKeyword, true);
            }

            if (filterKeyword.Contains("*") || filterKeyword.Contains("?"))
            {
                filterKeyword = filterKeyword.Replace("*", "%").Replace("?", "_");
            }
            else
            {
                filterKeyword = $"%{filterKeyword}%";
            }

            return (filterKeyword, false);
        }
        public List<ArchiverSiteMasterIndex> GetAllGoogleNodesInfo()
        {
            List<ArchiverSiteMasterIndex> domains = new List<ArchiverSiteMasterIndex>();
            using (var context = GetNewContext())
            {
                domains = context.ArchiverSiteMasterIndexs.AsQueryable().Where(a => a.MergeIndexState == (int)MergeIndexState.Succeed && a.Flag == (int)SourceFlag.Google).ToList();
            }
            return domains;
        }

        public ArchiverSiteMasterIndexContract GetGoogleDriveInfo(ArchiverSiteMasterIndexContract site)
        {
            ArchiverSiteMasterIndexContract contract = null;
            this.ClearNullValue(site);
            List<ArchiverSiteMasterIndex> domains = null;
            using (var context = GetNewContext())
            {
                domains = context.ArchiverSiteMasterIndexs.AsQueryable().Where(s => s.SiteURL == site.SiteURL && s.SiteId == site.SiteId).ToList();
            }
            if (domains != null && domains.Count > 0)
            {
                contract = ConvertToDto(domains[0]);
            }
            return contract;
        }

        public Dictionary<string, List<string>> GetAllBackupGDriveCollectionDistinctJobIdMappings(List<string> direIds, long startTime, long endTime)
        {
            var diveIdsJobIds = new Dictionary<string, List<string>>();
            using (var context = GetNewContext())
            {
                foreach (var driveId in direIds)
                {
                    var jobIds = context.ArchiverSiteMasterIndexs.Where(s => s.SiteId == driveId && s.ArchiverTime <= endTime && s.ArchiverTime >= startTime).Select(s => s.JobId).ToList();
                    logger.Info($"Current driveId is {driveId}, contains job count is {jobIds.Count}");
                    diveIdsJobIds.Add(driveId, jobIds);
                }
            }
            return diveIdsJobIds;
        }

        public async Task<ArchiverSiteMasterIndex?> GetLatestSiteCollectionNodeInfoByUrlAsync(string url)
        {
            using var context = GetNewContext();
            var indexes = await context.ArchiverSiteMasterIndexs
                .Where(n => n.SiteURL == url)
                .ToListAsync();
            return indexes.OrderByDescending(i => i.ArchiverTime).FirstOrDefault();
        }

        public List<string> GetExistingSiteCollectionUrls(IEnumerable<string> siteUrls)
        {
            if (siteUrls == null)
            {
                return new List<string>();
            }

            var normalizedUrls = siteUrls
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Select(url => url.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!normalizedUrls.Any())
            {
                return new List<string>();
            }

            const int batchSize = 1000;
            var existingUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using var context = GetNewContext();
            for (var i = 0; i < normalizedUrls.Count; i += batchSize)
            {
                var batch = normalizedUrls.Skip(i).Take(batchSize).ToList();
                var matchedUrls = context.ArchiverSiteMasterIndexs
                    .Where(item => batch.Contains(item.SiteURL))
                    .Select(item => item.SiteURL)
                    .Distinct()
                    .ToList();

                foreach (var url in matchedUrls.Where(url => !string.IsNullOrWhiteSpace(url)))
                {
                    existingUrls.Add(url);
                }
            }

            return existingUrls.ToList();
        }

        public List<string> LoadSiteCollectionUrlsByJobIdOrTeamsGroup(string jobId, string teamsGroupAddress)
        {
            var context = GetNewContext();
            return context.ArchiverSiteMasterIndexs.Where(s => s.JobId.StartsWith(jobId) || s.GroupMailboxAddress.Equals(teamsGroupAddress)).Select(_ => _.SiteURL).ToList();
        }

        public List<ArchiverSiteMasterIndexContract> LoadSiteMasterIndexByJobIdOrTeamsGroup(string jobId, string teamsGroupAddress)
        {
            List<ArchiverSiteMasterIndexContract> contract = null;
            var context = GetNewContext();
            var domains = context.ArchiverSiteMasterIndexs.Where(s => s.JobId.StartsWith(jobId) || s.GroupMailboxAddress.Equals(teamsGroupAddress))
                .OrderByDescending(_ => _.ArchiverTime).ToList();
            if(domains != null && domains.Count > 0)
            {
                contract = new List<ArchiverSiteMasterIndexContract>();
                domains.ForEach(a => contract.Add(ConvertToDto(a)));
            }
            return contract;
        }

        public async Task<(string, string, string)> GetArchivedChannelSiteInfoAsync(string siteCollectionUrl)
        {
            using var context = GetNewContext();
            context.Database.CommandTimeout = 600;
            var groupMailboxAddress = await context.ArchiverSiteMasterIndexs
                .AsNoTracking()
                .Where(n => n.SiteURL == siteCollectionUrl)
                .OrderByDescending(i => i.ArchiverTime)
                .Select(i => i.GroupMailboxAddress)
                .FirstOrDefaultAsync();
            if (string.IsNullOrEmpty(groupMailboxAddress)) return (string.Empty, string.Empty, string.Empty);
            var result = await context.CommonSiteMasterIndexes
                .AsNoTracking()
                .Where(n => n.SiteURL == groupMailboxAddress)
                .OrderByDescending(i => i.ArchiverTime)
                .Select(i => new { i.Extension, i.O365TenantId })
                .FirstOrDefaultAsync();
            if (result is null || string.IsNullOrEmpty(result.Extension)) return (groupMailboxAddress, string.Empty, string.Empty);
            var extObj = SerializerHelper.DeserializeByDataContractSerializer<ArchiverGroupSiteMasterIndexExtension>(result.Extension);
            if (extObj is null
                || string.IsNullOrEmpty(extObj.SPGroupSiteURL)
                || extObj.ChannelSiteRelativeURLs is null
                || extObj.SPGroupSiteURL.EqualsIgnoreCase(siteCollectionUrl))
            {
                return (groupMailboxAddress, extObj?.SPGroupSiteURL ?? string.Empty, result.O365TenantId);
            }
            return (groupMailboxAddress, extObj.SPGroupSiteURL, result.O365TenantId);
        }

        public async Task<string> GetO365TenantIdBySiteCollectionAsync(string siteCollectionUrl)
        {
            using var context = GetNewContext();
            context.Database.CommandTimeout = 600;
            return await context.ArchiverSiteMasterIndexs
                .AsNoTracking()
                .Where(n => n.SiteURL == siteCollectionUrl)
                .OrderByDescending(i => i.ArchiverTime)
                .Select(i => i.O365TenantId)
                .FirstOrDefaultAsync();
        }
    }
    public enum ArchiverSiteMasterIndexFlag  //Binary system,if add new flag please Multiply by 2
    {
        None = 0,
        MoveDataTier = 1,
    }
}
