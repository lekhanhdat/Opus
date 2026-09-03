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
using AvePoint.Hybrid.Contract;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Myhub.Items.Views;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class DownloadDataInfoDao : BaseDao<RMDownloadDataInfo>, IDownloadDataInfoDao
    {

        private static IAveLogger logger = AveLogger.GetInstance(typeof(DownloadDataInfoDao));
        public bool CreateDownloadDataInfo(RMDownloadDataInfo downloadDataInfo)
        {
            try
            {
                string userId = TenantLocalValue.LogonUserId;
                using (var context = GetNewContext())
                {
                    var exist = context.DownloadDataInfo.Any(d => d.UserId == userId && d.RecordsId == downloadDataInfo.RecordsId && d.JobStatus == (int)DownloadContentJobStatus.Finished);
                    if (!exist)
                    {
                        context.DownloadDataInfo.Add(downloadDataInfo);
                        return context.SaveChanges() > 0;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void BatchDeleteDownloadDataInfoByIds(List<Guid> ids)
        {
            try
            {
                string userId = TenantLocalValue.LogonUserId;
                using (var ctx = GetNewContext())
                {
                    var downloadDataInfos = ctx.DownloadDataInfo.Where(a => a.UserId == userId && ids.Contains(a.RecordsId)).ToList();
                    foreach (RMDownloadDataInfo entity in downloadDataInfos)
                    {
                        ctx.Set<RMDownloadDataInfo>().Attach(entity);
                        ctx.Entry(entity).State = System.Data.Entity.EntityState.Deleted;
                    }
                    ctx.SaveChanges();
                }
            }
            catch (DbEntityValidationException dbex)
            {
                string message = string.Join("; ", dbex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<RMDownloadDataInfo> QueryDownloadDataInfoById(string searchKey, int pageIndex, int pageSize, out int totalRecord)
        {
            IQueryable<RMDownloadDataInfo> query = null;
            string userId = TenantLocalValue.LogonUserId;
            using (var ctx = GetNewContext())
            {
                if (string.IsNullOrWhiteSpace(searchKey))
                {
                    query = ctx.DownloadDataInfo.Where(d => d.UserId == userId).OrderByDescending(d => d.FileDownloadTime);
                }
                else
                {
                    query = ctx.DownloadDataInfo.Where(d => d.UserId == userId && d.Name.Contains(searchKey)).OrderByDescending(d => d.FileDownloadTime);
                }
                return query.Paging(pageIndex, pageSize, out totalRecord).ToList();
            }
        }

        public List<RMDownloadDataInfo> QueryDownloadReportInfoByScopeIds(List<string> scopeIds, int jobType, int pageIndex, int pageSize, out int totalRecord, string orderBy = null, bool isDesc = false)
        {
            if (scopeIds == null || scopeIds.Count == 0)
            {
                totalRecord = 0;
                return new List<RMDownloadDataInfo>();
            }

            string userId = TenantLocalValue.LogonUserId;
            var typedJobType = (MyhubReportJobType)jobType;
            var downloadContentType = MapToDownloadContentType(typedJobType);

            using (var ctx = GetNewContext())
            {
                var query = ctx.DownloadDataInfo.AsNoTracking()
                    .Where(d => d.UserId == userId)
                    .Where(d => ctx.Set<RMMyhubReportJob>().Any(m =>
                        m.JobId == d.JobId &&
                        m.UserId == userId &&
                        m.JobType == typedJobType &&
                        scopeIds.Contains(m.ScopeId)));

                totalRecord = query.Count();

                if (CanSortAtDatabaseLevel(orderBy))
                {
                    return ApplyDatabaseSorting(query, orderBy, isDesc)
                        .Skip((pageIndex - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
                }

                return FetchSortedByJsonField(query, (int)downloadContentType, ctx.DownloadDataInfo.AsNoTracking(), orderBy, isDesc, pageIndex, pageSize);
            }
        }

        private static DownloadContentType MapToDownloadContentType(MyhubReportJobType jobType)
        {
            return jobType switch
            {
                MyhubReportJobType.DownloadRCCReport => DownloadContentType.DownloadRCCReport,
                MyhubReportJobType.HistoryContent => DownloadContentType.HistoryContent,
                _ => DownloadContentType.DownloadRCCReport
            };
        }

        public List<RMDownloadDataInfo> QueryAllDownloadReportInfo(int jobType, int pageIndex, int pageSize, out int totalRecord, string orderBy = null, bool isDesc = false)
        {
            string userId = TenantLocalValue.LogonUserId;
            using (var ctx = GetNewContext())
            {
                var query = ctx.DownloadDataInfo.AsNoTracking()
                    .Where(d => d.UserId == userId)
                    .Where(d => d.DownloadType == (DownloadContentType)jobType);

                totalRecord = query.Count();

                if (CanSortAtDatabaseLevel(orderBy))
                {
                    return ApplyDatabaseSorting(query, orderBy, isDesc)
                        .Skip((pageIndex - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
                }

                return FetchSortedByJsonField(query, jobType, ctx.DownloadDataInfo.AsNoTracking(), orderBy, isDesc, pageIndex, pageSize);
            }
        }

        private bool CanSortAtDatabaseLevel(string orderBy)
        {
            return string.IsNullOrEmpty(orderBy) ||
                   orderBy.Equals("downloadtime", StringComparison.OrdinalIgnoreCase);
        }

        private IOrderedQueryable<RMDownloadDataInfo> ApplyDatabaseSorting(IQueryable<RMDownloadDataInfo> query, string orderBy, bool isDesc)
        {
            if (string.IsNullOrEmpty(orderBy) || isDesc)
                return query.OrderByDescending(d => d.FileDownloadTime);
            return query.OrderBy(d => d.FileDownloadTime);
        }

        private List<RMDownloadDataInfo> FetchSortedByJsonField(IQueryable<RMDownloadDataInfo> filteredQuery, int jobType, IQueryable<RMDownloadDataInfo> fullTableQuery, string orderBy, bool isDesc, int pageIndex, int pageSize)
        {
            var projections = filteredQuery.Select(d => new { d.Id, d.ExtendString1 }).ToList();

            var sorted = isDesc ? projections.OrderByDescending(p => GetJsonSortKey(p.ExtendString1, jobType, orderBy)) : projections.OrderBy(p => GetJsonSortKey(p.ExtendString1, jobType, orderBy));

            var pagedIds = sorted.Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(p => p.Id).ToList();

            if (pagedIds.Count == 0)
                return new List<RMDownloadDataInfo>();

            var entityDict = fullTableQuery.Where(d => pagedIds.Contains(d.Id)).ToList().ToDictionary(e => e.Id);

            return pagedIds.Where(entityDict.ContainsKey).Select(id => entityDict[id]).ToList();
        }

        private string GetJsonSortKey(string extendString, int jobType, string orderBy)
        {
            try
            {
                if (string.IsNullOrEmpty(extendString))
                    return string.Empty;
                if (jobType == (int)DownloadContentType.DownloadRCCReport)
                {
                    var content = JsonConvert.DeserializeObject<List<RCCReportContentDto>>(extendString)?.FirstOrDefault();
                    if (content == null)
                        return string.Empty;

                    return orderBy.ToLower() switch
                    {
                        "displayname" => content.DisplayName ?? string.Empty,
                        "enddatewithin" => content.TimeRange.PresetType.ToString() ?? string.Empty,
                        _ => string.Empty
                    };
                }
                else
                {
                    var content = JsonConvert.DeserializeObject<ManualApprovalHistoryOption>(extendString);
                    if (content == null)
                        return string.Empty;

                    return orderBy.ToLower() switch
                    {
                        "displayname" => content.DisplayName ?? string.Empty,
                        "enddatewithin" => content.LatestExportType.ToString() ?? string.Empty,
                        _ => string.Empty
                    };
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to parse ExtendString1 for sorting: {ex.Message}");
                return string.Empty;
            }
        }

        public bool IsHasInprogressRCCReport(List<string> jobIds)
        {
            string userId = TenantLocalValue.LogonUserId;

            using (var ctx = GetNewContext())
            {
                return ctx.DownloadDataInfo.Any(d =>
                    d.UserId == userId &&
                    d.JobStatus == (int)DownloadContentJobStatus.InProgress ||
                    d.JobStatus == (int)DownloadContentJobStatus.Wait &&
                    d.DownloadType == DownloadContentType.DownloadRCCReport &&
                    jobIds.Contains(d.JobId));
            }
        }

        public List<RMDownloadDataInfo> GetDownloadDataInfoByRetentionTime(long retentionTime)
        {
            //List<int> availableJobStatus = new List<int>()
            //{
            //    (int)DownloadContentJobStatus.Finished,
            //    (int)DownloadContentJobStatus.Wait,
            //    (int)DownloadContentJobStatus.InProgress
            //};

            List<RMDownloadDataInfo> expiredData = new List<RMDownloadDataInfo>();
            List<string> userIds = new List<string>();
            using (var ctx = GetNewContext())
            {
                userIds = ctx.DownloadDataInfo.Select(d => d.UserId).Distinct().ToList();
            }
            foreach (var userId in userIds)
            {
                using (var ctx = GetNewContext())
                {
                    var expiredDateContent = ctx.DownloadDataInfo.Where(d => d.UserId == userId && d.FileDownloadTime <= retentionTime).ToList();
                    if (expiredDateContent != null && expiredDateContent.Count > 0)
                    {
                        expiredData = expiredData.Concat(expiredDateContent).ToList();
                    }
                    var expiredCountContent = ctx.DownloadDataInfo.Where(d => d.UserId == userId && d.JobStatus == (int)DownloadContentJobStatus.Finished && d.FileDownloadTime > retentionTime).OrderByDescending(d => d.FileDownloadTime).Skip(100).ToList();
                    if (expiredCountContent != null && expiredCountContent.Count > 0)
                    {
                        expiredData = expiredData.Concat(expiredCountContent).ToList();
                    }
                }
            }
            return expiredData;
        }

        public bool ExistAvailableJob(Guid recordsId)
        {
            bool exist = false;
            try
            {
                string userId = TenantLocalValue.LogonUserId;
                List<int> availableJobStatus = new List<int>()
                {
                    (int)DownloadContentJobStatus.Finished,
                    (int)DownloadContentJobStatus.Wait,
                    (int)DownloadContentJobStatus.InProgress
                };
                using (var context = GetNewContext())
                {
                    exist = context.DownloadDataInfo.Any(d => d.UserId == userId && d.RecordsId == recordsId && availableJobStatus.Contains(d.JobStatus));
                }
            }
            catch (Exception e)
            {
                logger.Error($"error occured when ExistAvailableJob,error:{e}");
            }
            return exist;
        }

        public List<RMDownloadDataInfo> GetDownloadDataInfos(List<Guid> ids, List<int> status = null)
        {
            using (var ctx = GetNewContext())
            {
                string userId = TenantLocalValue.LogonUserId;
                if (status != null && status.Count > 0)
                {
                    return ctx.DownloadDataInfo.AsNoTracking().Where(d => d.UserId == userId && ids.Contains(d.RecordsId) && status.Contains(d.JobStatus)).ToList();
                }
                else
                {
                    return ctx.DownloadDataInfo.AsNoTracking().Where(d => d.UserId == userId && ids.Contains(d.RecordsId)).ToList();
                }
            }
        }

        public List<RMDownloadDataInfo> GetDownloadDataInfos(List<string> jobIds, List<int> status = null)
        {
            using (var ctx = GetNewContext())
            {
                string userId = TenantLocalValue.LogonUserId;
                if (status != null && status.Count > 0)
                {
                    return ctx.DownloadDataInfo.Where(d => d.UserId == userId && jobIds.Contains(d.JobId) && status.Contains(d.JobStatus)).ToList();
                }
                else
                {
                    return ctx.DownloadDataInfo.Where(d => d.UserId == userId && jobIds.Contains(d.JobId)).ToList();
                }
            }
        }

        public List<RMDownloadDataInfo> GetDownloadDataInfosByStatus(List<int> status)
        {
            using (var ctx = GetNewContext())
            {
                if (status != null && status.Count > 0)
                {
                    return ctx.DownloadDataInfo.Where(d => status.Contains(d.JobStatus)).ToList();
                }
                else
                {
                    return new List<RMDownloadDataInfo>();
                }
            }
        }

        public bool ApplyCurrentValues(RMDownloadDataInfo downloadDataInfo)
        {
            using (var ctx = GetNewContext())
            {
                var oldData = ctx.DownloadDataInfo.Where(d => d.Id == downloadDataInfo.Id).FirstOrDefault();
                if (oldData == null)
                {
                    return false;
                }
                else
                {
                    oldData.JobStatus = downloadDataInfo.JobStatus;
                    return this.ApplyCurrentValues(ctx, oldData);
                }
            }
        }

        public bool CreateZipPasswordInfo(RMDownloadDataInfo downloadDataInfo)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    context.DownloadDataInfo.Add(downloadDataInfo);
                    return context.SaveChanges() > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<RMDownloadDataInfo> GetZipPasswordInfoByRetentionTime(long retentionTime)
        {
            List<RMDownloadDataInfo> expiredData = new List<RMDownloadDataInfo>();
            List<string> userIds = new List<string>();
            using (var ctx = GetNewContext())
            {
                expiredData = ctx.DownloadDataInfo.Where(d => d.FileDownloadTime <= retentionTime && d.DownloadType == DownloadContentType.ZipPasswordInfo).ToList();
            }
            return expiredData;
        }

        public bool UpdateBlobSasUriByJobId(string jobId , string blobSasUri)
        {
            using (var ctx = GetNewContext())
            {
                var result = ctx.DownloadDataInfo.Where(d => d.JobId.StartsWith(jobId)).FirstOrDefault();
                if (result != null)
                {
                    result.BlobSasUri = blobSasUri;
                    ctx.DownloadDataInfo.AddOrUpdate(result);
                    return ctx.SaveChanges() > 0;
                }
                else
                {
                    return false;
                }
            }
        }

        public string GetBlobSasUriByJobId(string jobId)
        {
            using (var ctx = GetNewContext())
            {
                var result = ctx.DownloadDataInfo.Where(d => d.JobId.StartsWith(jobId)).FirstOrDefault();
                if (result != null)
                {
                    return result.BlobSasUri;
                }
                else
                {
                    return null;
                }
            }
        }

        public string GetBlobSasUriByRecordId(Guid recordId)
        {
            using (var ctx = GetNewContext())
            {
                var result = ctx.DownloadDataInfo.Where(d => d.RecordsId == recordId).FirstOrDefault();
                if (result != null)
                {
                    return result.BlobSasUri;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool UpdateDownloadFileSizeByJobId(string jobId, long fileSize)
        {
            using (var ctx = GetNewContext())
            {
                var result = ctx.DownloadDataInfo.Where(d => d.JobId.StartsWith(jobId)).FirstOrDefault();
                if (result != null)
                {
                    result.FileSize = fileSize;
                    ctx.DownloadDataInfo.AddOrUpdate(result);
                    return ctx.SaveChanges() > 0;
                }
                else
                {
                    return false;
                }
            }
        }

        public long? GetDownloadFileSizeByJobId(string jobId)
        {
            using (var ctx = GetNewContext())
            {
                var result = ctx.DownloadDataInfo.Where(d => d.JobId.StartsWith(jobId)).FirstOrDefault();
                if (result != null)
                {
                    return result.FileSize;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool UpdateDownloadInfo(RMDownloadDataInfo downloadDataInfo)
        {
            using (var ctx = GetNewContext())
            {
                var oldData = ctx.DownloadDataInfo.Where(d => d.Id == downloadDataInfo.Id).FirstOrDefault();
                if (oldData == null)
                {
                    return false;
                }

                else
                {
                    oldData.JobStatus = downloadDataInfo.JobStatus;
                    if (!string.IsNullOrEmpty(downloadDataInfo.BlobSasUri))
                    {
                        oldData.BlobSasUri = downloadDataInfo.BlobSasUri;
                    }
                    if(downloadDataInfo.FileSize != null)
                    {
                        oldData.FileSize = downloadDataInfo.FileSize;
                    }
                    ctx.DownloadDataInfo.AddOrUpdate(oldData);
                    return ctx.SaveChanges() > 0;
                }
            }
        }
        public RMDownloadDataInfo GetDownloadDataInfosByJobId(string jobId)
        {
            using (var ctx = GetNewContext())
            {
                var result=ctx.DownloadDataInfo.Where(d=>d.JobId.StartsWith(jobId)).ToList();
                if (result != null&&result.Count>0)
                {
                    return result.FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }
        }

        public RMDownloadDataInfo GetDownloadDataInfoByJobId(string jobId)
        {
            using var ctx = GetNewContext();
            return ctx.DownloadDataInfo.FirstOrDefault(item => item.JobId == jobId);
        }
    }
}
