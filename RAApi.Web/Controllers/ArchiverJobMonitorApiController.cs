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
using Aspose.Pdf.Operators;
using AvePoint.Api.Contract.Job;
using AvePoint.Api.Service.Implement;
using AvePoint.Api.Web.ApiControllers;
using AvePoint.Common.RemoteNode.Impl;
using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Cache.Services;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Cache;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.JobMonitor;
using AvePoint.RA.Web.Common.Utils;
using DocAveOnline.WebApi.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers
{
    [Route("api/archiverjobmonitorapi/[action]")]
    [ApiController]
    public class ArchiverJobMonitorApiController : RAWebApiBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(ArchiverJobMonitorApiController));
        private IJobMonitorService _JobMonitorService;
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService(ref _JobMonitorService);

        private IJobQueueService _JobQueueService;
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService(ref _JobQueueService);

        private IDownloadDataInfoDao _DownloadDataInfoDao;
        private IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService(ref _DownloadDataInfoDao);
        private AvePoint.Api.Service.Interface.IArchiverService ArchiverService { get { return new ArchiverService(); } }
        /// <summary>
        /// Get jobs by job ids
        /// </summary>
        /// <param name="ids">job ids</param>
        /// <returns>job</returns>
        /// <response code="500">An error occured.</response>
        /// <response code="200">Job info.</response>
        /// <response code="401">Authorize header failed.</response>
        [HttpPost]
        public async Task<JobResult> GetJobListByIds([FromBody] List<string> ids)
        {
            JobResult result = new JobResult() { LimitSize = 100 * 1024 * 1024 * 1024L, Jobs = new List<JobDto>() };
            List<JMItemInfo> temp = await JobMonitorService.GetJobsForRecenterAsync(ids);
            foreach (var job in temp)
            {
                result.Jobs.Add(ConvertToJobDto(job));
            }
            return result;
        }

        [HttpPost]
        public ArchiverExportJobDetailInfo RecenterJobDetails([FromBody] string jobId)
        {
            return JobMonitorService.RecenterJobDetailsAsync(jobId).GetAwaiter().GetResult();
        }

        [HttpPost]
        public bool CheckEndUserArchvierJobInJobQueue([FromBody] string jobId)
        {
            return JobQueueService.CheckEndUserArchvierJobInJobQueue(jobId);
        }

        [HttpPost]
        public AvePoint.Api.Contract.Job.JMJobSummary GetJobSummary([FromBody] string id)
        {
            return ArchiverService.GetJobSummary(id);
        }

        [HttpPost]
        public AvePoint.Api.Contract.Job.JMDetailsResult GetJobDetails([FromBody] AvePoint.Api.Contract.Job.JMDetailsQuery queryModel)
        {
            return ArchiverService.GetJobDetails(queryModel);
        }

        [HttpPost]
        public AvePoint.Api.Contract.Job.JMJobDetails GetJobSummaryStatistics([FromBody] string id)
        {
            return ArchiverService.GetJobSummaryStatistics(id);
        }

        [HttpPost]
        public List<JMJobInfo> GetOpusJobListByIds([FromBody] List<string> ids)
        {
            return ArchiverService.GetOpusJobListByIds(ids);
        }

        private JobDto ConvertToJobDto(JMItemInfo info)
        {
            JobDto jobDto = new JobDto();
            jobDto.Id = info.JobId;
            jobDto.Module = Module.ArchiverRestore;
            jobDto.Status = DataContractConvertUtil.ConvertToStatus(info.Status);
            jobDto.StartTime = ParseStringToDateTime(info.StartTime);
            if (info.EndTime != "Pending")
            {
                jobDto.FinishTime = ParseStringToDateTime(info.EndTime);
            }
            jobDto.Progress = info.Progress;
            jobDto.NodeType = (RemoveNodeType)info.NodeType;
            return jobDto;

        }
        private DateTime ParseStringToDateTime(string timeStr)
        {
            DateTime result = new DateTime(Convert.ToInt64(timeStr), DateTimeKind.Utc).ToLocalTime();
            return result;
        }
        /// <summary>
        /// Public API: query job list with pager and filters.
        /// Mirrors internal JMApiController.QueryPager for cross-product consumption.
        /// </summary>
        [HttpPost]
        public Task<JMPageResult> ListJobs([FromBody] JMPager pager)
        {
            using (new PerformanceScope("public api list jobs"))
            {
                return JobMonitorService.GetJobsListAsync(pager);
            }
        }

        /// <summary>
        /// Public API: queue a job to generate job report logs and wait until SAS URI is available (or timeout) then return it.
        /// </summary>
        [HttpPost]
        public async Task<string> DownloadJobReport([FromBody] List<string> jobIds)
        {
            try
            {
                logger.Info("Public API DownloadJobReport");

                // validate payload
                if (jobIds == null || jobIds.Count == 0 || jobIds.Exists(id => string.IsNullOrWhiteSpace(id)))
                {
                    logger.Warn("DownloadJobReport called with invalid payload");
                    return string.Empty;
                }

                // run generation immediately (no DB job queue)
                var param = AvePoint.RA.Common.Global.Utils.SerializerHelper.SerializeByDataContractSerializer(jobIds);
                var jobId = await JobMonitorService.RealRunDownloadJobReportJobForCOP(param);
                if (string.IsNullOrWhiteSpace(jobId))
                {
                    logger.Warn("DownloadJobReport failed to start: empty jobId returned");
                    return string.Empty;
                }
                logger.Debug($"DownloadJobReport started directly. jobId={jobId}, downloadJobIds:{string.Join(',', jobIds)}");

                // poll until the SAS is available, with a safety timeout to avoid hanging forever
                var pollIntervalMs = 1000; // 1 second
                var timeoutMs = 3600000;   // 1 hour max wait
                var stopAt = DateTime.UtcNow.AddMilliseconds(timeoutMs);
                List<int> finalJobStatus = new List<int>()
                {
                    (int)DownloadContentJobStatus.None,
                    (int)DownloadContentJobStatus.Calculating,
                    (int)DownloadContentJobStatus.Failed,
                    (int)DownloadContentJobStatus.Finished,
                    (int)DownloadContentJobStatus.FinishWithException,
                    (int)DownloadContentJobStatus.Skipped,
                    (int)DownloadContentJobStatus.Stopped,
                    (int)DownloadContentJobStatus.Stopping
                };

                while (DateTime.UtcNow < stopAt)
                {
                    try
                    {
                        var info = DownloadDataInfoDao.GetDownloadDataInfosByJobId(jobId);
                        if (info != null && finalJobStatus.Contains(info.JobStatus))
                        {
                            logger.Debug($"Download Info is final status:{info.JobStatus}, return SAS uri for jobId={jobId}");
                            return info.BlobSasUri;
                        }
                    }
                    catch (Exception pollEx)
                    {
                        // swallow and continue polling; transient DB/storage issues can resolve on retry
                        logger.Debug($"Polling SAS for jobId={jobId} threw, will retry.", pollEx);
                    }

                    await System.Threading.Tasks.Task.Delay(pollIntervalMs);
                }

                logger.Warn($"Timed out waiting for SAS for jobId={jobId} after {timeoutMs}ms");
                return string.Empty;
            }
            catch (Exception ex)
            {
                logger.Error("DownloadJobReport failed", ex);
                return string.Empty;
            }
        }
    }
}
