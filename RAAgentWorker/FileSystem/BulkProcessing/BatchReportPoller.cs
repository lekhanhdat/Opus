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
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using AvePoint.GCommon;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Common.Utils.ProtoBuf;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using Azure.Storage.Blobs;

namespace RAFileSystem.FileSystem.BulkProcessing
{
    public class BatchReportPoller : IDisposable
    {
        private readonly AveLogger logger = AveLogger.GetInstance(typeof(BatchReportPoller));
        private const int POLL_INTERVAL = 5000; // 5s
        private const int MAX_CONCURRENT_CHECKS = 10;

        private Channel<FSBatchReportDto> _reportChannel;

        private readonly ConcurrentDictionary<string, Task> _pendingTasks = new ConcurrentDictionary<string, Task>();
        private readonly SemaphoreSlim _checkSemaphore = new SemaphoreSlim(MAX_CONCURRENT_CHECKS);
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private Task _processingLoopTask;
        private volatile bool _disposed;

        public void InitiatePolling(ChannelReader<string> messageIdReader, string jobId)
        {
            _reportChannel = Channel.CreateUnbounded<FSBatchReportDto>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
            _processingLoopTask = Task.Run(() => PollingLoopAsync(messageIdReader, jobId));
        }

        private async Task PollingLoopAsync(ChannelReader<string> inputReader, string jobId)
        {
            try
            {
                while (await inputReader.WaitToReadAsync().ConfigureAwait(false))
                {
                    while (inputReader.TryRead(out string messageId))
                    {
                        var task = MonitorBatchAsync(jobId, messageId);
                        _pendingTasks.TryAdd(messageId, task);
                    }
                }

                await Task.WhenAll(_pendingTasks.Values);
            }
            catch (Exception ex)
            {
                logger.Error($"Polling Loop crashed: {ex}");
            }
            finally
            {
                _reportChannel.Writer.TryComplete();
            }
        }

        private async Task MonitorBatchAsync(string jobId, string messageId)
        {
            bool isCompleted = false;

            try
            {
                while (!isCompleted && !_cts.IsCancellationRequested)
                {
                    await _checkSemaphore.WaitAsync(_cts.Token).ConfigureAwait(false);

                    var batchReport = new FSBatchReportDto()
                    {
                        MessageId = messageId,
                    };

                    try
                    {
                        var response = HybridApiClient.Instance.GetBatchReportResponseAsync(jobId, messageId);

                        // Process based on status if needed
                        if (response == null || response.Status == JobDetailsStatus.Pending)
                        {
                            continue;
                        }

                        if (response.Status == JobDetailsStatus.Failed)
                        {
                            logger.Error($"Batch job failed for messageId: {messageId}, Error: {response.ErrorMessage}");
                            batchReport.BatchStatus = JobDetailsStatus.Failed;
                            batchReport.ErrorMessage = response.ErrorMessage;
                        }

                        if (!string.IsNullOrEmpty(response.SASURI))
                        {
                            batchReport = await DownloadBatchReportBlobAsync(response.SASURI).ConfigureAwait(false);
                            logger.Info($"Batch report downloaded for messageId: {messageId}, Items: {batchReport.Records.Count}");
                        }

                        if (batchReport != null)
                        {
                            logger.Info($"Batch report processed successfully for messageId: {messageId}");
                            _reportChannel.Writer.TryWrite(batchReport);
                            isCompleted = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Monitor error for {messageId}: {ex.Message}");
                    }
                    finally
                    {
                        _checkSemaphore.Release();
                        if (!isCompleted)
                        {
                            await Task.Delay(POLL_INTERVAL, _cts.Token).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                logger.Info($"Monitoring cancelled for messageId: {messageId}");
            }
            catch (Exception e)
            {
                logger.Error($"Unexpected error while monitoring batch for messageId: {messageId}. Error: {e.Message}");
            }
            finally
            {
                _pendingTasks.TryRemove(messageId, out _);
            }
        }

        private async Task<FSBatchReportDto> DownloadBatchReportBlobAsync(string sasUri)
        {
            try
            {
                var blobClient = new BlobClient(new Uri(sasUri));
                using (Stream blobStream = await blobClient.OpenReadAsync().ConfigureAwait(false))
                {
                    return FileSystemContractHelper.DeserializerProtoBuf<FSBatchReportDto>(blobStream);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to download/deserialize result from Blob. URI: {sasUri}. Error: {ex.Message}");
                throw;
            }
        }

        public IEnumerable<FSBatchReportDto> CollectBatchReports()
        {
            var spin = new SpinWait();
            int idleCycles = 0;

            while (true)
            {
                if (_reportChannel.Reader.TryRead(out var item))
                {
                    spin.Reset();
                    idleCycles = 0;
                    yield return item;
                    continue;
                }

                if (_reportChannel.Reader.Completion.IsCompleted)
                {
                    if (_reportChannel.Reader.TryRead(out item))
                    {
                        yield return item;
                        continue;
                    }
                    yield break;
                }

                spin.SpinOnce();

                if (spin.NextSpinWillYield)
                {
                    idleCycles++;
                    if (idleCycles > 25)
                    {
                        Thread.Sleep(1);
                        idleCycles = 0;
                    }
                }

                if (spin.Count > 5000)
                {
                    logger.Warn("SpinWait timeout reached...");
                    Thread.Sleep(5);
                    spin.Reset();
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                _processingLoopTask?.Wait();

                _cts.Cancel();
            }
            catch (Exception ex)
            {
                logger.Warn($"Dispose warning: {ex.Message}");
            }
            finally
            {
                _cts.Dispose();
                _checkSemaphore.Dispose();
            }
        }
    }
}
