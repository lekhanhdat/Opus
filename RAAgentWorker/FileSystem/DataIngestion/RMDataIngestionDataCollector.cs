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
using AvePoint.GCommon.Utility.PerformanceScope;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.FileSystem.Collect;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ProtoBuf;
using RAFileSystem.FileSystem.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.DataIngestion
{
    public class RMDataIngestionDataCollector
    {
        private const int MAX_RECORDS_PER_BLOB = 10_000;

        private readonly AveLogger _logger = new AveLogger(typeof(RMDataIngestionDataCollector));

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private readonly string _jobId;

        private readonly RMDataIngestionOperationType _operationType;

        private readonly RMDataIngestionExecutionResultCollector _executionResultCollector;

        private int _counter = 0;

        private RMDataIngestionBlobReference _blobReference;

        private Stream _streamWriter;

        private RMDataIngestMessageExtensionManager _messageExtensionManager;

        private RMDataIngestionPersistor _ingestionPersistor;

        public RMDataIngestionDataCollector(RMDataIngestionExecutionResultCollector executionResultCollector, RMDataIngestionPersistor ingestionPersister)
            : this(executionResultCollector, null, ingestionPersister)
        {
        }

        public RMDataIngestionDataCollector(RMDataIngestionExecutionResultCollector executionResultCollector, RMDataIngestMessageExtensionManager messageExtensionManager, RMDataIngestionPersistor ingestionPersister)
        {
            if (executionResultCollector == null)
                throw new ArgumentNullException(nameof(executionResultCollector), "Please init the RMDataIngestionExecutionResultCollector first.");
            _jobId = executionResultCollector.JobId;
            _operationType = executionResultCollector.OperationType;
            _executionResultCollector = executionResultCollector;
            _messageExtensionManager = messageExtensionManager;
            _ingestionPersistor = ingestionPersister;
        }

        public async Task WriteDataAsync<TBlob, TLocal>(TBlob blobData, TLocal localData)
        {
            using (new AgentPerformanceScope("DataCollector.WriteData", addToStatistics: true))
            {
                await _lock.WaitAsync();
                try
                {
                    WriteDataToBlob(blobData);
                    WriteDataToLocalFile(localData);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error while writing data: {ex}");
                }
                finally
                {
                    _lock.Release();
                }
            }
        }

        public async Task WriteBatchAsync<TBlob, TLocal>(IReadOnlyList<TBlob> blobDataList, IReadOnlyList<TLocal> localDataList)
        {
            using (new AgentPerformanceScope("DataCollector.WriteBatch", addToStatistics: true))
            {
                if (blobDataList == null || localDataList == null) return;
                if (blobDataList.Count == 0 || blobDataList.Count != localDataList.Count)
                {
                    _logger.Warn($"The count of blob data list and local data list should be the same and greater than 0. BlobDataList count: {blobDataList.Count}, LocalDataList count: {localDataList.Count}");
                    return;
                }

                await _lock.WaitAsync();
                try
                {
                    for (int i = 0; i < blobDataList.Count; i++)
                    {
                        try
                        {
                            WriteDataToBlob(blobDataList[i]);
                            WriteDataToLocalFile(localDataList[i]);
                        }
                        catch (Exception ex)
                        {
                            var failureId = localDataList[i] as RMDataIngestionAgentWorkItemExecutionResult;
                            FSJobCache.Instance.CurrentJobFailedItemIds.Add(failureId.Id);
                            _logger.Error($"Error while writing record {failureId?.Id} in batch: {ex}");
                        }
                    }
                }
                finally
                {
                    _lock.Release();
                }
            }
        }

        private void WriteDataToBlob<T>(T data)
        {
            using (new AgentPerformanceScope("DataCollector.WriteDataToBlob", addToStatistics: true))
            {
                try
                {
                    if (data == null) return;
                    if (_streamWriter == null || _counter >= MAX_RECORDS_PER_BLOB)
                    {
                        SendMessage();
                        _blobReference = GenerateBlobReference();
                        if (_blobReference == null || string.IsNullOrEmpty(_blobReference.SasUri))
                            throw new InvalidOperationException("Failed to generate a valid blob reference: the API returned null or an empty SAS URI.");
                        var blobClient = new BlobClient(new Uri(_blobReference.SasUri));
                        var httpHeaders = new BlobHttpHeaders { ContentType = "application/octet-stream" };
                        
                        _streamWriter = blobClient.OpenWrite(true, new BlobOpenWriteOptions { HttpHeaders = httpHeaders });
                        _counter = 0;
                        _logger.Info($"Started writing to new blob {_blobReference.SasUri}");
                    }
                    Serializer.SerializeWithLengthPrefix(_streamWriter, data, PrefixStyle.Base128, 1);
                    _counter++;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error while writing data to blob: {ex}");
                    throw;
                }
            }
        }

        private void WriteDataToLocalFile<T>(T data) => _ingestionPersistor.WriteData(data);

        private RMDataIngestionBlobReference GenerateBlobReference()
        {
            var blobReference = HybridApiClient.Instance.DataIngestionGenerateBlobReference(new RMDataIngestionBlobNamingContext
            {
                UniqueId = _jobId,
                IngestionType = RMDataIngestionType.AgentWork,
                OperationType = _operationType,
                BlobType = RMDataIngestionBlobType.Source
            });
            return blobReference;
        }

        private void Flush()
        {
            if (_streamWriter == null) return;
            try
            {
                _streamWriter.Flush();
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to flush blob {_blobReference?.SasUri}. Ex: {ex.Message}");
            }
            try
            {
                _streamWriter.Dispose();
                _logger.Info($"Closed blob {_blobReference?.SasUri}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to dispose blob {_blobReference?.SasUri}. Ex: {ex.Message}");
            }
        }

        private void SendMessage()
        {
            using (new AgentPerformanceScope("DataCollector.SendMessage", addToStatistics: true))
            {
                if (_streamWriter == null || _counter == 0)
                {
                    return;
                }
                _logger.Info($"Current record counter blob {_counter}");
                Flush();

                var message = new RMDataIngestionMessageDto
                {
                    Id = Guid.NewGuid(),
                    UniqueId = _jobId,
                    IngestionType = RMDataIngestionType.AgentWork,
                    OperationType = _operationType,
                    SourceBlobName = _blobReference.BlobName,
                    CreatedTime = DateTime.UtcNow.Ticks
                };

                if (_messageExtensionManager != null)
                {
                    message.Extension = _messageExtensionManager.SerializeAndReset();
                }

                var receipt = HybridApiClient.Instance.DataIngestionSendMessage(message);

                if (receipt == null)
                {
                    _logger.Error($"Failed to send message for blob {_blobReference?.BlobName}");
                    return;
                }

                _executionResultCollector.AddMessageSendReceipt(receipt);

                _logger.Info($"Sent blob message for blob {_blobReference.SasUri}");

                _ingestionPersistor.SetCurrentMessageId(receipt.MessageId);
            }
        }

        public void Complete()
        {
            using (new AgentPerformanceScope("DataCollector.Complete", addToStatistics: true))
            {
                SendMessage();
                _executionResultCollector.SetMessageAddCompleted();
                _ingestionPersistor.Commit();
            }
        }
    }
}
