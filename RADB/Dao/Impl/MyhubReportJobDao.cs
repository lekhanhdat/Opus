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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Myhub.Items.Views;
using AvePoint.RA.Contract.MyHub.Model.QueryRequest.Views;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class MyhubReportJobDao : BaseDao<RMMyhubReportJob>, IMyhubReportJobDao
    {
        public List<RMMyhubReportJob> GetAllReportJobByUserName(string userId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMMyhubReportJobs
                          .Where(j => j.UserId == userId)
                          .GroupBy(j => j.JobId)
                          .Select(g => g.FirstOrDefault())
                          .ToList();
            }
        }

        public List<RMMyhubReportJob> GetJobByScopeId(List<string> scopeIds, string userId)
        {
            if (scopeIds == null || !scopeIds.Any()) return new List<RMMyhubReportJob>();

            using (var ctx = GetNewContext())
            {
                return ctx.RMMyhubReportJobs
                          .Where(j => scopeIds.Contains(j.ScopeId) && j.UserId == userId)
                          .ToList();
            }
        }

        public async Task UpdateStatusByJobId(string jobId, MyhubReportJobStatus status)
        {
            using (var ctx = GetNewContext())
            {
                var jobs = ctx.RMMyhubReportJobs.Where(j => j.JobId == jobId).ToList();
                if (jobs != null && jobs.Count > 0)
                {
                    foreach (var job in jobs)
                    {
                        job.Status = status;
                    }
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteReportJobByJobId(string jobId)
        {
            using (var ctx = GetNewContext())
            {
                var jobs = ctx.RMMyhubReportJobs.Where(j => j.JobId == jobId).ToList();
                if (jobs != null && jobs.Count > 0)
                {
                    ctx.RMMyhubReportJobs.RemoveRange(jobs);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public async Task CreateJobReports(RMDownloadDataInfo downloadInfo)
        {
            MyhubReportJobStatus mappedStatus = (MyhubReportJobStatus)downloadInfo.JobStatus;

            MyhubReportJobType mappedJobType;
            if (downloadInfo.DownloadType == DownloadContentType.DownloadRCCReport)
            {
                mappedJobType = MyhubReportJobType.DownloadRCCReport;

                var rccReports = JsonConvert.DeserializeObject<List<RCCReportContentDto>>(downloadInfo.ExtendString1) ?? new List<RCCReportContentDto>();

                using (var ctx = GetNewContext())
                {
                    foreach (var rccReport in rccReports)
                    {
                        var job = new RMMyhubReportJob
                        {
                            JobId = downloadInfo.JobId,
                            UserId = downloadInfo.UserId,
                            ScopeId = rccReport.NodeId,
                            Status = mappedStatus,
                            JobType = mappedJobType,
                            ExecutedTime = downloadInfo.FileDownloadTime
                        };
                        ctx.RMMyhubReportJobs.Add(job);
                    }

                    await ctx.SaveChangesAsync();
                }
            }
            else if (downloadInfo.DownloadType == DownloadContentType.HistoryContent)
            {
                mappedJobType = MyhubReportJobType.HistoryContent;

                var historyReports = JsonConvert.DeserializeObject<ManualApprovalHistoryOption>(downloadInfo.ExtendString1) ?? new ManualApprovalHistoryOption();

                using (var ctx = GetNewContext())
                {
                    var job = new RMMyhubReportJob
                    {
                        JobId = downloadInfo.JobId,
                        UserId = downloadInfo.UserId,
                        ScopeId = historyReports.Id,
                        Status = mappedStatus,
                        JobType = mappedJobType,
                        ExecutedTime = downloadInfo.FileDownloadTime
                    };
                    ctx.RMMyhubReportJobs.Add(job);
                    await ctx.SaveChangesAsync();
                }
            }
            else
            {
                return;
            }
            
        }

        public List<RMMyhubReportJob> GetAllMyhubReportJobByJobType(int jobType, int pageIndex, int pageSize, out int totalRecord)
        {
            string userId = TenantLocalValue.LogonUserId;

            using (var ctx = GetNewContext())
            {
                var query = ctx.RMMyhubReportJobs.AsNoTracking()
                       .Where(d => d.UserId == userId && d.JobType == (MyhubReportJobType)jobType)
                       .GroupBy(d => d.JobId)
                       .Select(g => g.FirstOrDefault())
                       .OrderByDescending(d => d.ExecutedTime);

                return query.Paging(pageIndex, pageSize, out totalRecord).ToList();
            }
        }

        public List<RMMyhubReportJob> GetMyhubReportByScopeIds(List<string> scopeIds, int jobType, int downloadType, int pageIndex, int pageSize, out int totalRecord)
        {
            if (scopeIds == null || !scopeIds.Any())
            {
                totalRecord = 0;
                return new List<RMMyhubReportJob>();
            }
            string userId = TenantLocalValue.LogonUserId;

            using (var ctx = GetNewContext())
            {
                var query = ctx.RMMyhubReportJobs.AsNoTracking()
                       .Where(d => d.UserId == userId
                                && d.JobType == (MyhubReportJobType)jobType
                                && scopeIds.Contains(d.ScopeId))
                       .GroupBy(d => d.JobId)
                       .Select(g => g.FirstOrDefault())
                       .OrderByDescending(d => d.ExecutedTime);

                return query.Paging(pageIndex, pageSize, out totalRecord).ToList();
            }
        }
    }
}