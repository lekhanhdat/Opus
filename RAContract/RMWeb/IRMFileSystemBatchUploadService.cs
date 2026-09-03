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
using System.Threading.Tasks;
using AvePoint.Hybrid.Contract;
using AvePoint.RA.Contract.FileSystem;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IRMFileSystemBatchUploadService
    {
        /// <summary>
        ///  Return SAS URI of Azure Table by job
        /// </summary>
        bool StartQueueListenerAsync(string jobId, JobType jobType);

        /// <summary>
        ///  Return SAS URI of Blob for batch data upload
        /// </summary>
        string GetBlobSasUriAsync(string jobId, string blobName);

        /// <summary>
        ///  Return Message ID for batch processing
        /// </summary>
        string NotifyUploadCompleteAsync(FSBatchUploadNotification notification);

        /// <summary>
        ///  Return batch report record
        /// </summary>
        FSBatchReportTableEntityDto GetBatchReportResponseAsync(string jobId, string messageId);

        bool DisposeQueueListenerAsync(string jobId);
    }
}
