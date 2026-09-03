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
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb;
using Microsoft.AspNetCore.Mvc;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [Route("api/FSBatchDataUpload/[action]")]
    [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridAgentScope)]
    [RMAgentApiPerformanceLogger]
    public class FSBatchDataUploadController : RAWebApiBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(FSBatchDataUploadController));

        private IRMFileSystemBatchUploadService FSBatchUploadService => PlatformWindsorManager.GetService<IRMFileSystemBatchUploadService>();

        [HttpPost]
        public bool StartQueueListener([FromBody] JobInfo jobInfo)
        {
            return FSBatchUploadService.StartQueueListenerAsync(jobInfo.JobId, jobInfo.JobType);
        }

        [HttpGet]
        public string GetBlobSasUri(string jobId, string blobName)
        {
            return FSBatchUploadService.GetBlobSasUriAsync(jobId, blobName);
        }

        [HttpPost]
        public string NotifyUploadComplete([FromBody] FSBatchUploadNotification request)
        {
            return FSBatchUploadService.NotifyUploadCompleteAsync(request);
        }

        [HttpGet]
        public FSBatchReportTableEntityDto GetBatchReportResponse(string jobId, string messageId)
        {
            return FSBatchUploadService.GetBatchReportResponseAsync(jobId, messageId);
        }

        [HttpPost]
        public bool DisposeQueueListener([FromBody] string jobId)
        {
            return FSBatchUploadService.DisposeQueueListenerAsync(jobId);
        }
    }
}
