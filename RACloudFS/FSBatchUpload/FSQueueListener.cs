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
using System.Threading;
using System.Threading.Tasks;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.FileSystem;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;

namespace RAFileSystem.FSBatchUpload
{
    public class FSQueueListener : IDisposable
    {
        private RALogger logger = RALogger.GetInstance(typeof(FSQueueListener));
        private readonly IFSBatchHandler _batchHandler;
        private volatile bool _isRunning;
        private QueueClient _queueClient;

        private const int MAX_CONCURRENT_JOBS = 10;
        private readonly SemaphoreSlim _concurrencySemaphore = new SemaphoreSlim(MAX_CONCURRENT_JOBS);
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly ConcurrentDictionary<string, string> _processingPopReceipts = [];

        public FSQueueListener(IFSBatchHandler batchHandler)
        {
            _batchHandler = batchHandler;
            InitializeAsync();
        }

        public IFSBatchHandler BatchHandler => _batchHandler;

        public void InitializeAsync()
        {
            _queueClient ??= _batchHandler.GetQueueClient();
            _queueClient.CreateIfNotExists();
        }

        public async Task RunAsync()
        {
            _isRunning = true;
            while (_isRunning && !_cts.IsCancellationRequested)
            {
                await _concurrencySemaphore.WaitAsync(_cts.Token);
                var messageId = string.Empty;
                try
                {
                    var retrievedMessage = await GetNextMessageAsync();

                    if (retrievedMessage != null)
                    {
                        messageId = retrievedMessage.MessageId;
                        _ = ProcessMessageAsync(retrievedMessage, _cts.Token);
                    }
                    else
                    {
                        _concurrencySemaphore.Release();
                        await Task.Delay(TimeSpan.FromSeconds(5), _cts.Token);
                    }
                }
                catch (Exception ex)
                {
                    _concurrencySemaphore.Release();
                    logger.Error($"[FSBDU] Error polling message. MessageId: {messageId}, Exception: {ex}");
                }
            }
        }

        private async Task ProcessMessageAsync(QueueMessage message, CancellationToken token)
        {
            if (!_processingPopReceipts.TryAdd(message.MessageId, message.PopReceipt))
            {
                return;
            }

            using (var renewCts = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                var renewTask = KeepMessageInvisibleAsync(message.MessageId, renewCts.Token);

                try
                {
                    // Deserialize
                    var batchJobMessage = JsonConvert.DeserializeObject<FSBatchUploadNotification>(message.Body.ToString());

                    // Process Logic
                    await _batchHandler.ProcessBatchJobAsync(message.MessageId, batchJobMessage);

                    // Stop Renewing
                    renewCts.Cancel();
                    try { await renewTask; } catch (OperationCanceledException) { }

                    // Delete Message
                    if (_processingPopReceipts.TryGetValue(message.MessageId, out string latestPopReceipt))
                    {
                        await _queueClient.DeleteMessageAsync(message.MessageId, latestPopReceipt, token);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Error processing message {message.MessageId}: {ex.Message}");
                }
                finally
                {
                    if (!renewCts.IsCancellationRequested)
                    {
                        renewCts.Cancel();
                        try { await renewTask; } catch (OperationCanceledException) { }
                    }

                    _processingPopReceipts.TryRemove(message.MessageId, out _);

                    _concurrencySemaphore.Release();
                }
            }
        }

        private async Task KeepMessageInvisibleAsync(string messageId, CancellationToken token)
        {
            var visibilityWindow = TimeSpan.FromMinutes(30);
            var renewInterval = TimeSpan.FromMinutes(25);

            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(renewInterval, token);

                    if (_processingPopReceipts.TryGetValue(messageId, out string currentPopReceipt))
                    {
                        var result = await _queueClient.UpdateMessageAsync(
                            messageId,
                            currentPopReceipt,
                            visibilityTimeout: visibilityWindow,
                            cancellationToken: token
                        );

                        if (result != null && result.Value != null)
                        {
                            _processingPopReceipts[messageId] = result.Value.PopReceipt;
                        }
                    }
                    else
                    {
                        logger.Warn($"Pop receipt not found for message {messageId}. Stopping renewals.");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"Exception occurred while processing renewals for message {messageId}: {ex.Message}");
            }
        }

        private async Task<QueueMessage> GetNextMessageAsync()
        {
            // get 1 message, hide 30 mins (VisibilityTimeout)
            QueueMessage[] retrievedMessages = await _queueClient.ReceiveMessagesAsync(
                maxMessages: 1,
                visibilityTimeout: TimeSpan.FromMinutes(30)
            );

            if (retrievedMessages.Length > 0)
            {
                return retrievedMessages[0];
            }

            return null;
        }

        public void Dispose()
        {
            _isRunning = false;

            while (_concurrencySemaphore.CurrentCount < MAX_CONCURRENT_JOBS)
            {
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }

            _cts.Cancel();
            _cts.Dispose();
            _concurrencySemaphore.Dispose();
            _batchHandler.Dispose();
        }
    }
}
