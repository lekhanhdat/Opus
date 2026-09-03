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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using AvePoint.GCommon;
using AvePoint.Hybrid.Contract;
using AvePoint.Media.Storage;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Common.Utils.ProtoBuf;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.FileSystem.Utils;
using Azure.Storage.Blobs;
using Microsoft.Azure.Amqp.Framing;

namespace RAFileSystem.FileSystem.BulkProcessing
{
    public class BatchController<T> : IDisposable where T : class
    {
        private readonly AveLogger logger = AveLogger.GetInstance(typeof(BatchController<T>));

        // Config
        private const int MAX_ITEMS_PER_BATCH = 1000;
        private const long MAX_SIZE_BYTES_PER_BATCH = 1024 * 1024 * 250; // 250MB
        private const int MAX_CONCURRENT_UPLOADS = 4;

        // State
        private readonly string _jobId;
        private readonly JobType _jobType;
        private readonly Func<T, long> _sizeCalculator;
        private readonly SemaphoreSlim _uploadSemaphore;
        private readonly CancellationTokenSource _cts;

        private Channel<T> _itemInputChannel;
        private Channel<string> _uploadedBatchIdChannel;
        private Task _processingLoopTask;

        private volatile bool _disposed;
        private FSBatchOperationType operationType;

        public ChannelReader<string> UploadedBatchIdReader => _uploadedBatchIdChannel.Reader;

        private readonly ConcurrentDictionary<Task, bool> _activeUploadTasks = new ConcurrentDictionary<Task, bool>();

        public BatchController(string jobId, JobType jobType, FSBatchOperationType operationType)
            : this(jobId, jobType, operationType, null) {}

        public BatchController(string jobId, JobType jobType, FSBatchOperationType operationType, Func<T, long> sizeCalculator)
        {
            _jobId = jobId;
            _jobType = jobType;
            this.operationType = operationType;
            _sizeCalculator = sizeCalculator;
            _cts = new CancellationTokenSource();
            _uploadSemaphore = new SemaphoreSlim(MAX_CONCURRENT_UPLOADS);
        }

        public void Start()
        {
            ProtobufRuntimeHelper.EnsureTypeRegistered<T>();
            _itemInputChannel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
            _uploadedBatchIdChannel = Channel.CreateUnbounded<string>();
            _processingLoopTask = Task.Run(() => ProcessingLoopAsync(_cts.Token));
        }

        public void AddItem(T item)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(BatchController<T>));

            if (!_itemInputChannel.Writer.TryWrite(item))
            {
                logger.Warn("Failed to write item to channel. Channel might be closed.");
            }
        }

        private async Task ProcessingLoopAsync(CancellationToken token)
        {
            var currentBatch = new List<T>(MAX_ITEMS_PER_BATCH);
            long currentSize = 0;

            try
            {
                while (await _itemInputChannel.Reader.WaitToReadAsync(token).ConfigureAwait(false))
                {
                    while (_itemInputChannel.Reader.TryRead(out var item))
                    {
                        currentBatch.Add(item);
                        if (_sizeCalculator != null) currentSize += _sizeCalculator(item);

                        bool isCountLimitReached = currentBatch.Count >= MAX_ITEMS_PER_BATCH;
                        bool isSizeLimitReached = (_sizeCalculator != null) && (currentSize >= MAX_SIZE_BYTES_PER_BATCH);

                        if (isCountLimitReached || isSizeLimitReached)
                        {
                            await StartBatchUploadAsync(new List<T>(currentBatch), currentSize, token);
                            currentBatch.Clear();
                            currentSize = 0;
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                logger.Error($"Processing loop crashed: {ex}");
            }
            finally
            {
                if (currentBatch.Count > 0)
                {
                    await StartBatchUploadAsync(currentBatch, currentSize, CancellationToken.None);
                }
            }
        }

        private async Task StartBatchUploadAsync(List<T> itemsSnapshot, long currentSize, CancellationToken token = default)
        {
            await _uploadSemaphore.WaitAsync(token).ConfigureAwait(false);

            var uploadTask = Task.Run(async () =>
            {
                try
                {
                    var batchID = Guid.NewGuid().ToString();
                    var batchPackage = new BatchPackage<T>
                    {
                        BatchId = batchID,
                        BatchFileName = $"d_{batchID}.fsb",
                        BatchSize = currentSize,
                        Items = itemsSnapshot
                    };

                    string msgId = await ProcessBatchUploadAsync(batchPackage).ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(msgId))
                    {
                        _uploadedBatchIdChannel.Writer.TryWrite(msgId);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Upload failed: {ex.Message}");
                    // TODO: retry
                }
                finally
                {
                    _uploadSemaphore.Release();
                }
            }, CancellationToken.None);

            _activeUploadTasks.TryAdd(uploadTask, true);

            _ = uploadTask.ContinueWith(t =>
            {
                _activeUploadTasks.TryRemove(t, out _);
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Core Logic: SAS -> Serialize -> Upload -> Notify
        /// </summary>
        private async Task<string> ProcessBatchUploadAsync(BatchPackage<T> batchData)
        {
            // 1. Get SAS
            //batchData.BatchFileName = $"{_jobId}_{batchData.BatchId}.fsb"; // Example blob name
            string sasUri = HybridApiClient.Instance.GetBlobSasUriAsync(_jobId, batchData.BatchFileName);

            // 2. Serialize & Upload (retry,...)
            await SerializeAndUploadAsync(batchData, sasUri).ConfigureAwait(false);

            // 3. Notify
            var notification = new FSBatchUploadNotification
            {
                JobId = _jobId,
                BatchId = batchData.BatchId,
                BlobName = batchData.BatchFileName,
                ItemCount = batchData.Items.Count,
                OperationType = operationType,
                // ...
            };

            string messageId = HybridApiClient.Instance.NotifyUploadCompleteAsync(notification);

            logger.Info($"Batch {batchData.BatchId} uploaded. MsgID: {messageId}. Items: {batchData.Items.Count}");

            return messageId;
        }

        private async Task SerializeAndUploadAsync(BatchPackage<T> batchData, string sasUri)
        {
            var blobClient = new BlobClient(new Uri(sasUri));

            if (_sizeCalculator == null || batchData.BatchSize < FileSystemContractHelper.MEMORY_STREAM_THRESHOLD)
            {
                try
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        FileSystemContractHelper.SerializerProtoBuf(memoryStream, batchData);
                        memoryStream.Position = 0;
                        await blobClient.UploadAsync(memoryStream, true);
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"SerializeAndUploadAsync failed: {e.Message}");
                    throw;
                }
            }
            else
            {
                var cacheSystem = ExternalUtil.OpenXSystem(AppDomain.CurrentDomain.BaseDirectory);
                var tempFile = new StorageInfo { HighName = Path.Combine("BatchTempFolder", _jobId, "data"), LowName = batchData.BatchFileName };
                
                try
                {
                    using (var fileStream = cacheSystem.OpenStream(tempFile, FileMode.OpenOrCreate))
                    {
                        FileSystemContractHelper.SerializerProtoBuf(fileStream, batchData);
                    }

                    using (var readStream = cacheSystem.OpenStream(tempFile, FileMode.Open))
                    {
                        await blobClient.UploadAsync(readStream, true);
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"SerializeAndUploadAsync failed: {e.Message}");
                    throw;
                }
                finally
                {
                    cacheSystem.DeleteFile(tempFile);
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _itemInputChannel.Writer.TryComplete();

            try
            {
                _processingLoopTask?.Wait();

                var tasksToWait = _activeUploadTasks.Keys.ToArray();

                if (tasksToWait.Length > 0)
                {
                    Task.WaitAll(tasksToWait);
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"Dispose warning: {ex.Message}");
            }
            finally
            {
                _uploadedBatchIdChannel.Writer.TryComplete();
                _cts.Cancel();
                _cts.Dispose();
                _uploadSemaphore.Dispose();
            }
        }
    }
}
