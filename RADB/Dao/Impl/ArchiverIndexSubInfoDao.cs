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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static AvePoint.GCommon.Utility.I18N.EventIds.Configuration;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class ArchiverIndexSubInfoDao : BaseDao<ArchiverIndexSubInfo>, IArchiverIndexSubInfoDao
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(ArchiverIndexSubInfoDao));


        public void UpdateSubInfoes(params ArchiverIndexSubInfo[] subInfoes)
        {
            using var context = GetNewContext();
            context.ArchiverIndexSubInfos.AddOrUpdate(subInfoes);
            context.SaveChanges();
        }

        public List<ArchiverIndexSubInfoContract> GetSubInfoesBySubJobId(string subJobId)
        {
            List<ArchiverIndexSubInfoContract> result = new List<ArchiverIndexSubInfoContract>();
            List<ArchiverIndexSubInfo> domains = null;
            using (var context = GetNewContext())
            {
                domains = context.ArchiverIndexSubInfos.Where(s=>s.SubJobId == subJobId).ToList();
            }
            if (domains != null && domains.Count > 0)
            {
                foreach (ArchiverIndexSubInfo domain in domains)
                {
                    result.Add(this.ConvertToDto(domain));
                }
            }
            return result;
        }

        public async Task<(bool, ArchiverIndexSubInfoContract)> TryGetSubInfoByJobIdAsync(string subSubJobId)
        {
            using var context = GetNewContext();
            var domain = await context.ArchiverIndexSubInfos.FirstOrDefaultAsync(item => item.SubSubJobId == subSubJobId);
            if(domain == null)
            {
                return (false, null);
            }
            return (true, ConvertToDto(domain));
        }

        public async Task<(bool, ArchiverIndexSubInfo)> TryGetByJobIdAsync(string subSubJobId)
        {
            using var context = GetNewContext();
            var domain = await context.ArchiverIndexSubInfos.FirstOrDefaultAsync(item => item.SubSubJobId == subSubJobId);
            return (domain != null, domain);
        }
        public List<string> GetAllBackupOrMergeIndexFailedSubJobIds()
        {
            logger.Info("start GetAllMergeIndexFailedMainJobIds");
            List<string> subJobIds = new List<string>();
            List<ArchiverIndexSubInfo> domains = new List<ArchiverIndexSubInfo>();
            using (var context = GetNewContext())
            {
                domains = context.ArchiverIndexSubInfos.Where(a=>a.MergeIndexState != (int)MergeIndexState.Succeed && a.MergeIndexState != (int)MergeIndexState.DAOMigrated).ToList();
            }
            foreach (var temp in domains)
            {
                string subjobid = temp.SubSubJobId.Substring(0, temp.SubSubJobId.LastIndexOf('_'));
                if (!subJobIds.Contains(subjobid))
                {
                    subJobIds.Add(subjobid);
                }
            }
            logger.Info($"finish GetAllMergeIndexFailedMainJobIds,count:{subJobIds.Count}");
            return subJobIds;
        }

        public List<string> GetAllArchiverIndexSubSubJobIDs(string subJobId)
        {
            logger.Info("GetAllArchiverIndexSubInfo: job Id: {0}", subJobId);
            List<string> subJobIds = new List<string>();
            var subInfos = GetSubInfoesBySubJobId(subJobId);
            foreach (var subInfo in subInfos)
            {
                subJobIds.Add(subInfo.JobId);
            }
            logger.Info("GetAllArchiverIndexSubInfo: job Id: {0}, subJobIds count: {1}.", subJobId, subJobIds.Count);
            return subJobIds;
        }

        public async Task<List<(string, string)>> GetAllArchiverIndexSubInfoThatNoSubJobIdAsync()
        {
            logger.Info("GetAllArchiverIndexSubInfoThatNoSubJobId");
            using var context = GetNewContext();
            var domains = await context.ArchiverIndexSubInfos
                .Where(i => i.SubJobId == null)
                .Select(i => new { i.Id, i.SubSubJobId })
                .ToListAsync();

            List<(string, string)> results = new List<(string, string)>();
            foreach (var domain in domains)
            {
                results.Add((domain.Id, domain.SubSubJobId));
            }
            logger.Info($"GetAllArchiverIndexSubInfoThatNoSubJobId count: {results.Count}");
            return results;
        }

        public async Task<List<string>> GetAllDeviceIDsAsync()
        {
            using var context = GetNewContext();
            return await context.ArchiverIndexSubInfos.Select(i => i.CurrentStorageId).Distinct().ToListAsync();
        }

        public List<ArchiverIndexSubInfo> GetAllArchiverIndexSubInfoByStorageIdAndSourceFlag(string storageId, List<int> sourceFlag)
        {
            logger.Info("GetAllArchiverIndexSubInfoByStorageId: storageId: {0}", storageId);
            List<string> subJobIds = new List<string>();
            List<ArchiverIndexSubInfo> domains = new List<ArchiverIndexSubInfo>();
            long now = DateTime.UtcNow.Ticks;
            using (var context = GetNewContext())
            {
                int pageIndex = 0;
                int pageSize = 500;
                while (true)
                {
                    var temp = context.ArchiverIndexSubInfos.Where(s => s.StorageId.Equals(storageId, StringComparison.OrdinalIgnoreCase) && sourceFlag.Contains(s.DataFlag)).OrderBy(s => s.Id).Skip(pageIndex * pageSize).Take(pageSize).ToList();
                    if (temp == null || temp.Count != pageSize)
                    {
                        logger.Info($"this page is the last page,temp count:{temp?.Count},pageindex:{pageIndex}");
                        if (temp != null)
                        {
                            domains.AddRange(temp);
                        }
                        break;
                    }
                    else
                    {
                        pageIndex++;
                        domains.AddRange(temp);
                    }
                }
            }
            logger.Info("GetAllArchiverIndexSubInfoByStorageId: storageId: {0}, subIndexs count: {1}.", storageId, domains.Count);
            return domains;
        }

        public List<ArchiverIndexSubInfo> GetAllArchiverIndexSubInfoByStorageId(string storageId)
        {
            logger.Info("GetAllArchiverIndexSubInfoByStorageId: storageId: {0}", storageId);
            List<string> subJobIds = new List<string>();
            List<ArchiverIndexSubInfo> domains = new List<ArchiverIndexSubInfo>();
            long now = DateTime.UtcNow.Ticks;
            using (var context = GetNewContext())
            {
                int pageIndex = 0;
                int pageSize = 500;
                while (true)
                {
                    var temp = context.ArchiverIndexSubInfos.Where(s => s.StorageId.Equals(storageId, StringComparison.OrdinalIgnoreCase)).OrderBy(s => s.Id).Skip(pageIndex * pageSize).Take(pageSize).ToList();
                    if (temp == null || temp.Count != pageSize)
                    {
                        logger.Info($"this page is the last page,temp count:{temp?.Count},pageindex:{pageIndex}");
                        if (temp != null)
                        {
                            domains.AddRange(temp);
                        }
                        break;
                    }
                    else
                    {
                        pageIndex++;
                        domains.AddRange(temp);
                    }
                }
            }
            logger.Info("GetAllArchiverIndexSubInfoByStorageId: storageId: {0}, subIndexs count: {1}.", storageId, domains.Count);
            return domains;
        }

        public async Task<bool> CheckIfExistArchiverIndexSubInfoByStorageIdAndSourceFlag(string storageId, List<int> sourceFlag)
        {
            using var context = GetNewContext();
            return await context.ArchiverIndexSubInfos.AnyAsync(s => s.CurrentStorageId.Equals(storageId, StringComparison.OrdinalIgnoreCase) && sourceFlag.Contains(s.DataFlag));
        }

        //return GB
        public long GetArchiverStorageGBSize()
        {
            long sizeInMB = 0;
            using (var context = GetNewContext())
            {
                context.Database.CommandTimeout = 900;
                string lastId = null;
                long totalMediaDataSize = 0L;
                var tableName = GetFullTableName();
                const int pageSize = 2000;
                var pageSql = $@"
                    WITH Batch AS
                    (
                        SELECT TOP (@p0) [Id], [MediaDataSize], [SubJobId]
                        FROM {tableName}
                        WHERE (@p1 IS NULL OR [Id] > @p1)
                        ORDER BY [Id]
                    )
                    SELECT
                        MAX([Id]) AS [LastId],
                        ISNULL(SUM(CASE WHEN [SubJobId] IS NOT NULL AND [SubJobId] NOT LIKE 'DASO%' THEN [MediaDataSize] ELSE 0 END), 0) AS [PageMediaDataSize],
                        COUNT(1) AS [RowCount]
                    FROM Batch";

                while (true)
                {
                    using var performanceScope = new PerformanceScope("IsTrailLicenceAndExceedSizeLimit.GetArchiverStorageGBSize");
                    var pageResult = context.Database.SqlQuery<KeysetPageAggregationResult>(
                        pageSql,
                        pageSize,
                        (object)lastId ?? DBNull.Value
                    ).FirstOrDefault();

                    if (pageResult == null || pageResult.RowCount == 0)
                    {
                        break;
                    }

                    totalMediaDataSize += pageResult.PageMediaDataSize;
                    lastId = pageResult.LastId;

                    if (pageResult.RowCount < pageSize)
                    {
                        break;
                    }
                }

                sizeInMB = totalMediaDataSize / (1024 * 1024);
            }
            logger.Info($"GetArchiverStorageGBSizeAsync: {sizeInMB} MB");
            return sizeInMB / 1024;
        }

        private class KeysetPageAggregationResult
        {
            public string LastId { get; set; }

            public long PageMediaDataSize { get; set; }

            public int RowCount { get; set; }
        }

        public long GetAOSPArchiverStorageGBSize()
        {
            long sizeInMB = 0;
            using (var context = GetNewContext())
            {
                context.Database.CommandTimeout = 900;
                var totalMediaDataSize = context.ArchiverIndexSubInfos
                    .Where(i => i.SubJobId.StartsWith("DASO"))
                    .Sum(i => i.MediaDataSize);

                sizeInMB = totalMediaDataSize / (1024 * 1024);
            }
            logger.Info($"GetArchiverStorageGBSizeAsync: {sizeInMB} MB");
            return sizeInMB / 1024;
        }

        public double GetArchiverStorageDoubleGBSize()
        {
            //防止计数不那么准确，先用MB为单位统计
            double sizeInMB = 0.00;
            using (var context = GetNewContext())
            {
                var mediaDataSizeList = context.ArchiverIndexSubInfos.Select(i => i.MediaDataSize).ToList();

                if (mediaDataSizeList != null)
                {
                    foreach (var size in mediaDataSizeList)
                    {
                        sizeInMB += (double)size / (1024 * 1024);
                    }
                }
            }
            logger.Info($"GetArchiverStorageGBSizeAsync: {sizeInMB} MB");
            return sizeInMB / 1024;
        }

        public async Task<double> GetAllArchiverStorageGBSizeAsync(string storageId, IEnumerable<string> excludedJobPrefixes = null, CancellationToken cancellationToken = default)
        {
            long totalBytes = 0;
            using (var context = GetNewContext())
            {
                context.Database.CommandTimeout = 900;
                var query = context.ArchiverIndexSubInfos.Where(i => i.CurrentStorageId == storageId);
                if (excludedJobPrefixes is not null)
                {
                    foreach (var prefix in excludedJobPrefixes)
                    {
                        query = query.Where(i => !i.SubJobId.StartsWith(prefix));
                    }
                }
                totalBytes = await query.SumAsync(i => (long?)i.MediaDataSize, cancellationToken) ?? 0;
            }
            return totalBytes / 1024d / 1024 / 1024;
        }

        public async Task UpdateArchiverIndexSubInfoMergeIndexStatusAsync(string jobId, int status)
        {
            ArchiverIndexSubInfo domain = null;
            using (var context = GetNewContext())
            {
                domain = context.ArchiverIndexSubInfos.Where(s => s.SubSubJobId.Equals(jobId)).FirstOrDefault();

                if (domain != null)
                {
                    domain.MergeIndexState = status;
                    await this.UpdateAsync(domain);
                }
                else
                {
                    logger.Warn("UpdateArchiverIndexSubInfoMergeIndexStatus:cannot find job Id: {0} ", jobId);
                }
            }
        }
        public async Task UpdateArchiverIndexSubInfoMediaSizeAsync(string jobId, long size)
        {
            ArchiverIndexSubInfo domain = null;
            using (var context = GetNewContext())
            {
                domain = context.ArchiverIndexSubInfos.Where(s => s.SubSubJobId.Equals(jobId)).FirstOrDefault();

                if (domain != null)
                {
                    logger.Info($"Decrease media data size for {jobId}, original size: {domain.MediaDataSize}, decrease size: {size}");
                    domain.MediaDataSize -= size;
                    if (domain.MediaDataSize < 0)
                    {
                        domain.MediaDataSize = 0;
                    }
                    await this.UpdateAsync(domain);
                }
                else
                {
                    logger.Warn("UpdateArchiverIndexSubInfoMediaSizeAsync:cannot find job Id: {0} ", jobId);
                }
            }
        }
        public async Task UpdateArchiverIndexSubInfoMediaSizeForAdjustAsync(string jobId, long size)
        {
            ArchiverIndexSubInfo domain = null;
            using (var context = GetNewContext())
            {
                domain = context.ArchiverIndexSubInfos.Where(s => s.SubSubJobId.Equals(jobId)).FirstOrDefault();

                if (domain != null)
                {
                    domain.MediaDataSize = size;
                    if (size < 0)
                    {
                        domain.MediaDataSize = 0;
                    }
                    await this.UpdateAsync(domain);
                }
                else
                {
                    logger.Warn("UpdateArchiverIndexSubInfoMediaSizeForAdjustAsync:cannot find job Id: {0} ", jobId);
                }
            }
        }

        public int BatchUpdateSubJobId(List<ArchiverIndexSubInfo> items)
        {
            return this.BatchUpdate(items, p => p.SubJobId);
        }

        public Dictionary<string,double> GetAllArchiverIndexSubInfoBySiteUrls(Dictionary<string, List<string>> SiteUrlSubJobIds)
        {
            var result = new Dictionary<string, double>();
            using (var context = GetNewContext())
            {
                context.Database.CommandTimeout = 900;
                foreach (var key in SiteUrlSubJobIds.Keys)
                {
                    long totalSize = 0;
                    foreach (var jobId in SiteUrlSubJobIds[key])
                    {
                        var results = context.ArchiverIndexSubInfos.Where(s => s.SubJobId == jobId && s.SourceFlag != (int)SourceFlag.Google).Select(s => s.MediaDataSize);
                        var size = results.Sum();
                        totalSize += size;
                    }
                    var sizeGB = (double)totalSize / ContractConstants.GBSizeInterval;
                    result[key] = sizeGB;
                    logger.Info($"Current site url is {key}, total archived size is {sizeGB} GB");
                }
            }
            return result;
        }

        public double GetAllArchivedSizeBySubJobIdInGB(string subJobId)
        {
            using (var context = GetNewContext())
            {
                var results = context.ArchiverIndexSubInfos.Where(s => s.SubJobId == subJobId).Select(s => s.MediaDataSize);
                var totalSize = results.Sum();
                var sizeGB = (double)totalSize / ContractConstants.GBSizeInterval;
                logger.Info($"{subJobId} total archived size is {sizeGB} GB");
                return sizeGB;
            }
        }

        private ArchiverIndexSubInfoContract ConvertToDto(ArchiverIndexSubInfo domain)
        {
            if (domain == null)
            {
                return null;
            }
            ArchiverIndexSubInfoContract info = new ArchiverIndexSubInfoContract();
            info.Id = domain.Id;
            info.JobId = domain.SubSubJobId;
            //info.StoragePolicyId = domain.StoragePolicyId;
            //info.LogicalDeviceId = domain.LogicalDeviceId;
            //info.PhysicalDeviceId = domain.PhysicalDeviceId;
            info.RetentionTime = domain.RetentionTime;
            info.RetentionTimeSpanSeconds = domain.KeepTime;
            info.StorageInfo = domain.StorageId;
            info.CurrentStorageId = domain.CurrentStorageId;
            info.MediaDataSize = domain.MediaDataSize;
            info.AgentDataSize = domain.AgentDataSize;
            info.SourceFlag = domain.SourceFlag;
            info.DataFlag = domain.DataFlag;
            info.IsSoftDelete = domain.DeletedStatus == (int)DeletedStatus.SoftDelete;
            info.DAOMigrated = domain.DAOMigrated.HasValue && domain.DAOMigrated.Value;
            if (domain.Extension != null && domain.Extension != string.Empty)
            {
                info.ArchiverSubInfoExtension = SerializerHelper.DeserializeByDataContractSerializer<ArchiverSubInfoExtension>(domain.Extension);
                if (info.ArchiverSubInfoExtension != null)
                {
                    info.DataEncryptionInfo = info.ArchiverSubInfoExtension.DataEncryptionInfo;
                }
            }
            return info;
        }

        public async Task CreateByBulkCopyAsync(IEnumerable<ArchiverIndexSubInfoContract> items)
        {
            if (items.Count() == 0)
            {
                return;
            }
            logger.Debug("Total add index sub infoes: {0}", items.Count());
            using (new PerformanceScope("Batch index sub infoes"))
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
            return $"[{GetTenantSchemaName()}].[ArchiverIndexSubInfoes]";
        }

        private DataTable ConvertToDataTable(IEnumerable<ArchiverIndexSubInfoContract> items)
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(String));
            table.Columns.Add("RetentionTime", typeof(Int64));
            table.Columns.Add("JobId", typeof(String));
            table.Columns.Add("StorageId", typeof(String));
            table.Columns.Add("CurrentStorageId", typeof(String));
            table.Columns.Add("KeepTime", typeof(Int64));
            table.Columns.Add("Extension", typeof(String));
            table.Columns.Add("StorageInfo", typeof(String));
            table.Columns.Add("MediaDataSize", typeof(Int64));
            table.Columns.Add("AgentDataSize", typeof(Int64));
            table.Columns.Add("MergeIndexState", typeof(Int32));
            table.Columns.Add("RuleId", typeof(String));
            table.Columns.Add("SourceFlag", typeof(Int32));
            table.Columns.Add("DAOMigrated", typeof(Boolean));
            table.Columns.Add("SubJobId", typeof(String));
            table.Columns.Add("DeletedStatus", typeof(Int32));
            table.Columns.Add("SoftDeleteTime", typeof(Int64));
            table.Columns.Add("DataFlag", typeof(Int32));

            foreach (var item in items)
            {
                var row = table.NewRow();
                row["Id"] = item.Id;
                row["RetentionTime"] = item.RetentionTime;
                row["JobId"] = item.JobId;
                row["StorageId"] = item.StoragePolicyId;
                row["CurrentStorageId"] = item.CurrentStorageId;
                row["KeepTime"] = 0;
                row["Extension"] = SerializerHelper.SerializeByDataContractSerializer(item.ArchiverSubInfoExtension);
                row["StorageInfo"] = item.StorageInfo;
                row["MediaDataSize"] = item.MediaDataSize;
                row["AgentDataSize"] = item.AgentDataSize;
                row["MergeIndexState"] = (int)MergeIndexState.DAOMigrated;
                row["RuleId"] = null;
                row["SourceFlag"] = item.SourceFlag;
                row["DAOMigrated"] = item.DAOMigrated;
                row["SubJobId"] = item.JobId.Substring(0, item.JobId.LastIndexOf("_", StringComparison.CurrentCulture));
                row["DeletedStatus"] = 0;
                row["SoftDeleteTime"] = 0;
                row["DataFlag"] = item.DataFlag;
                table.Rows.Add(row);
            }

            return table;
        }

        public async Task<int> DeleteMigratedIndexSubInfoesAsync()
        {
            var sql = $"DELETE FROM {GetFullTableName()} WHERE DAOMigrated=1";

            using (var context = GetNewContext())
            {
                return await context.Database.ExecuteSqlCommandAsync(sql);
            }
        }

        public List<ArchiverIndexSubInfo> GetAllDisposalArchiverIndexSubInfo(long timeOlder)
        {
            List<ArchiverIndexSubInfo> domains = null;
            using (var context = GetNewContext())
            {
                domains = context.ArchiverIndexSubInfos.Where(s => (s.RetentionTime >= timeOlder) && (s.MergeIndexState == (int)MergeIndexState.Succeed || s.MergeIndexState == (int)MergeIndexState.DAOMigrated) && (s.SubSubJobId.StartsWith("SEA") || s.SubSubJobId.StartsWith("OEA") || s.SubSubJobId.StartsWith("AR") || s.SubSubJobId.StartsWith("SO") || s.SubSubJobId.StartsWith("DSO"))).ToList();
            }
            return domains;
        }

        public List<ArchiverIndexSubInfo> GetAllSubInfos()
        {
            using (var context = GetNewContext())
            {
                return context.ArchiverIndexSubInfos.AsQueryable().Where(a => (a.MergeIndexState == (int)MergeIndexState.Succeed || a.MergeIndexState == (int)MergeIndexState.DAOMigrated) && (a.SubSubJobId.StartsWith("SEA") || a.SubSubJobId.StartsWith("OEA") || a.SubSubJobId.StartsWith("AR") || a.SubSubJobId.StartsWith("SO") || a.SubSubJobId.StartsWith("DSO"))).ToList();
            }
        }
        public bool CheckExistSoftInfoAndUpdateThem(List<string> jobIds)
        {
            using (var context = GetNewContext())
            {
                var result = context.ArchiverIndexSubInfos.AsQueryable().Where(a => a.DeletedStatus == (int)DeletedStatus.SoftDelete && jobIds.Contains(a.SubSubJobId)).ToList();
                if (result != null)
                {
                    foreach (var temp in result)
                    {
                        temp.DeletedStatus = (int)DeletedStatus.Restored;
                    }
                    bool isExist = result.Count > 0;
                    context.ArchiverIndexSubInfos.AddOrUpdate(result.ToArray());
                    context.SaveChanges();
                    return isExist;
                }
                else
                {
                    return false;
                }
            }
        }
        public async Task<ArchiverIndexSubInfo> GetSubInfoBySubsubJobIdAsync(string subsubjobId)
        {
            using (var context = GetNewContext())
            {
                return await context.ArchiverIndexSubInfos.FirstOrDefaultAsync(item => subsubjobId == item.SubSubJobId);
            }
        }
        public async Task<ArchiverIndexSubInfo> GetSubInfoByJobIdAsync(string jobId)
        {
            using (var context = GetNewContext())
            {
                return await context.ArchiverIndexSubInfos.FirstOrDefaultAsync(item => item.SubSubJobId.StartsWith(jobId));
            }
        }
        public List<ArchiverIndexSubInfo> GetAllArchiverIndexSubInfoByMainJobId(string mainJobId)
        {
            using (var context = GetNewContext())
            {
                return context.ArchiverIndexSubInfos.AsQueryable().Where(a => a.SubJobId.Equals(mainJobId)).ToList();
            }
        }

        public async Task<bool> ExistsSubInfoAsync(string subsubJobId)
        {
            using var context = GetNewContext();
            var results = context.Database.SqlQuery<string>(
                $"SELECT TOP 1 Id FROM {GetFullTableName()} WHERE JobId=@SubsubJobId;",
                new SqlParameter("SubsubJobId", subsubJobId)
            );
            var count = await results.CountAsync();
            return count > 0;
        }

        public async Task<int> GetSubInfoCountAsync(string subJobId)
        {
            using var context = GetNewContext();
            var results = context.Database.SqlQuery<int>(
                $"SELECT COUNT(1) FROM {GetFullTableName()} WHERE SubJobId=@SubJobId;",
                new SqlParameter("SubJobId", subJobId)
            );
            return await results.FirstOrDefaultAsync();
        }

        public async Task UpdateGDriveArchiverIndexSubInfoMergeIndexStatusAsync(string jobId, int status)
        {
            using (var context = GetNewContext())
            {
                var archiverIndexSubInfo = context.ArchiverIndexSubInfos.Where(s => s.SubJobId.Equals(jobId)).FirstOrDefault();

                if (archiverIndexSubInfo != null)
                {
                    archiverIndexSubInfo.MergeIndexState = status;
                    await this.UpdateAsync(archiverIndexSubInfo);
                }
                else
                {
                    logger.Warn("UpdateGDriveArchiverIndexSubInfoMergeIndexStatusAsync:cannot find job Id: {0} ", jobId);
                }
            }
        }

        public Dictionary<string, double> GetAllGoogleArchiverIndexSubInfoByDriveIds(Dictionary<string, List<string>>driveIdsJobIds)
        {
            var result = new Dictionary<string, double>();
            using (var context = GetNewContext())
            {
                foreach (var key in driveIdsJobIds.Keys)
                {
                    long totalSize = 0;
                    foreach (var jobId in driveIdsJobIds[key])
                    {
                        var size = context.ArchiverIndexSubInfos.Where(s => s.SubJobId == jobId && s.SourceFlag == (int)SourceFlag.Google).Select(s => s.MediaDataSize).DefaultIfEmpty(0).Sum();
                        totalSize += size;

                    }
                    var sizeGB = (double)totalSize / ContractConstants.GBSizeInterval;
                    result[key] = sizeGB;
                    logger.Info($"Current drive id is {key}, total archived size is {sizeGB} GB");
                }
            }
            return result;
        }
    }
}
