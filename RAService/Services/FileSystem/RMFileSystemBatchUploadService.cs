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
using System.Collections.Concurrent;
using AvePoint.Hybrid.Contract;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Model;
using Azure;
using Newtonsoft.Json;
using RAFileSystem.FSBatchUpload;

namespace AvePoint.RA.Service.Services.FileSystem
{
    public class RMFileSystemBatchUploadService : RMServiceBase, IRMFileSystemBatchUploadService
    {
        private static RALogger logger = RALogger.GetInstance(typeof(RMFileSystemBatchUploadService));
        private static readonly ConcurrentDictionary<string, FSQueueListener> _activeListeners = new();

        public bool StartQueueListenerAsync(string jobId, JobType jobType)
        {
            if (string.IsNullOrEmpty(jobId))
            {
                logger.Error($"[FSBDU] {nameof(StartQueueListenerAsync)} Params is invalid. jobId: {jobId}, jobType: {jobType}");
                throw new ArgumentNullException(nameof(jobId));
            }

            try
            {
                if (_activeListeners.TryGetValue(jobId, out _))
                {
                    logger.Warn($"[FSBDU] Queue Listener is already running for jobId: {jobId}");
                    return false;
                }

                IFSBatchHandler batchHandler = new FSBatchHandler(jobId, jobType);
                var queueListener = new FSQueueListener(batchHandler);

                if (!_activeListeners.TryAdd(jobId, queueListener))
                {
                    logger.Warn($"[FSBDU] Cannot add Queue Listener for jobId: {jobId}.");
                    return false;
                }
                _ = queueListener.RunAsync().ConfigureAwait(false);

                return true;
            }
            catch (Exception e)
            {
                logger.Error($"[FSBDU] Failed to start queue listener. jobId: {jobId}, jobType: {jobType}, Exception: {e}");
                throw;
            }
        }

        public string GetBlobSasUriAsync(string jobId, string blobName)
        {
            if (string.IsNullOrEmpty(jobId) || string.IsNullOrEmpty(blobName))
            {
                logger.Error($"[FSBDU] {nameof(GetBlobSasUriAsync)} Params is invalid. jobId: {jobId}, blobName: {blobName}");
                throw new ArgumentNullException(string.IsNullOrEmpty(jobId) ? nameof(jobId) : nameof(blobName));
            }

            if (!_activeListeners.TryGetValue(jobId, out var queueListener))
            {
                logger.Error($"[FSBDU] Queue listener not found for jobId: {jobId}");
                throw new Exception("Queue listener not found for the provided jobId.");
            }
            var batchHandler = queueListener.BatchHandler;

            var blobSasUri = batchHandler.GetBlobSasUriForWrite(blobName);
            if (string.IsNullOrEmpty(blobSasUri))
            {
                logger.Error($"[FSBDU] Failed to generate Blob SAS URI for jobId: {jobId}, blobName: {blobName}");
                throw new Exception("Failed to generate Blob SAS URI.");
            }
            return blobSasUri;
        }

        public string NotifyUploadCompleteAsync(FSBatchUploadNotification notification)
        {
            if (notification == null || string.IsNullOrEmpty(notification.JobId))
            {
                logger.Error($"[FSBDU] {nameof(NotifyUploadCompleteAsync)} Params is invalid. jobId: {notification?.JobId}");
                throw new ArgumentNullException(nameof(notification));
            }

            if (!_activeListeners.TryGetValue(notification.JobId, out var queueListener))
            {
                logger.Error($"[FSBDU] Queue listener not found for jobId: {notification.JobId}");
                throw new Exception("Queue listener not found for the provided jobId.");
            }

            var batchHandler = queueListener.BatchHandler;
            string messageBody = JsonConvert.SerializeObject(notification);
            try
            {
                var queueClient = batchHandler.GetQueueClient();

                var response = queueClient.SendMessageAsync(messageBody, timeToLive: TimeSpan.FromDays(7)).GetAwaiter().GetResult();

                return response.Value.MessageId;
            }
            catch (Exception ex)
            {
                //logger.Error
                throw new Exception($"Failed to enqueue batch {notification.BatchId}", ex);
            }
        }

        public FSBatchReportTableEntityDto GetBatchReportResponseAsync(string jobId, string messageId)
        {
            // use messageId to query batch report record from Azure Storage Table
            // it will contain the SAS URI of the items reports blob
            if (string.IsNullOrEmpty(jobId) || string.IsNullOrEmpty(messageId))
            {
                logger.Error($"[FSBDU] {nameof(GetBatchReportResponseAsync)} Params is invalid. jobId: {jobId}, messageId: {messageId}");
                throw new ArgumentNullException(string.IsNullOrEmpty(jobId) ? nameof(jobId) : nameof(messageId));
            }

            if (!_activeListeners.TryGetValue(jobId, out var queueListener))
            {
                logger.Error($"[FSBDU] Queue listener not found for jobId: {jobId}");
                throw new Exception("Queue listener not found for the provided jobId.");
            }
            var batchHandler = queueListener.BatchHandler;
            var recordDto = new FSBatchReportTableEntityDto
            {
                MessageId = messageId,
                JobId = jobId,
            };

            try
            {
                var tableClient = batchHandler.GetTableClient();
                var response = tableClient.GetEntityAsync<FSBatchUploadResultTableEntity>(jobId, messageId).GetAwaiter().GetResult();
                if (response != null)
                {
                    var batchReportRecordDto = response.Value;
                    recordDto.StartTime = batchReportRecordDto.StartTime;
                    recordDto.FinishTime = batchReportRecordDto.FinishTime;
                    recordDto.Status = batchReportRecordDto.Status;
                    recordDto.ErrorMessage = batchReportRecordDto.ErrorMessage;
                    recordDto.SASURI = batchReportRecordDto.SASURI;
                }
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                recordDto.Status = JobDetailsStatus.Pending;
            }
            catch (Exception ex)
            {
                logger.Error($"[FSBDU] Error retrieving batch report. Exception: {ex}");
                recordDto.Status = JobDetailsStatus.Failed;
                recordDto.ErrorMessage = ex.Message;
            }

            return recordDto;
        }

        public bool DisposeQueueListenerAsync(string jobId)
        {
            if (string.IsNullOrEmpty(jobId))
            {
                logger.Error($"[FSBDU] {nameof(DisposeQueueListenerAsync)} Params is invalid. jobId: {jobId}");
                throw new ArgumentNullException(nameof(jobId));
            }
            if (_activeListeners.TryRemove(jobId, out var queueListener))
            {
                queueListener.Dispose();
                return true;
            }
            return false;
        }
    }
}
