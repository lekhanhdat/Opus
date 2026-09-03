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
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Contract.DataIngestion.QueueMessageExtension;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Protobuf;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.AzureCosmosDB.Model;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.UniqueId;
using AvePoint.RA.Service.Services.DataIngestion.DataStorage;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DataIngestion.Processor.AgentWork.Ingestor
{
    public class RMDataIngestionFileSystemEnforceRuleActionIngestor : RMDataIngestionAgentWorkIngestor
    {
        public RMDataIngestionFileSystemEnforceRuleActionIngestor(RMDataIngestionBlobDataReader dataReader, string messageInfoExtension)
            : base(dataReader, typeof(RMDataIngestionFileSystemEnforceRuleActionIngestor))
        {
            //_disposalFileDetails = new ConcurrentDictionary<Guid, DisposalFileDetailDto>();
            _dataReader.RegisterProtobufModel<FileSystemRecordDto>();
            _messageInfoExtension = messageInfoExtension;
        }

        public override RMDataIngestionOperationType OperationType => RMDataIngestionOperationType.FileSystemEnforceRunAction;

        private readonly string _messageInfoExtension;

        protected override async IAsyncEnumerable<Record> ReadItemsAsync()
        {
            UniqueIdUtil idUtil = null;
            try
            {
                if (!string.IsNullOrEmpty(_messageInfoExtension))
                {
                    RMDataIngestMessageExtension messageExtension = JsonConvert.DeserializeObject<RMDataIngestMessageExtension>(_messageInfoExtension);
                    if (messageExtension != null && messageExtension.NewRecordIdsRange > 0)
                    {
                        idUtil = new UniqueIdUtil(TenantLocalValue.LogonGroupId, messageExtension.NewRecordIdsRange, UniqueIdType.FileSystem);
                    }
                    _logger.Info("Deserialize message extension successfully. Extension: {0}", _messageInfoExtension);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Failed to deserialize message extension. Exception: {0}", e);
            }
            var deduplicatedRecords = new Dictionary<Guid, Record>();
            await foreach (var item in _dataReader.ReadItemsAsync<FSDataIngestion<FileSystemRecordDto>>())
            {
                var recordDto = item.Item;
                if (recordDto == null) continue;

                var record = ConvertUtil.ConvertFSDtoToRMBaseRecord(recordDto);
                _logger.Info("Converted FS record DTO to RM base record for NodeId {0}. Record Id: {1}", recordDto.NodeId, record.RecordStatus);
                record.IsFsControlRecordJPMC = true;

                try
                {
                    if (idUtil != null && string.IsNullOrWhiteSpace(record.RecordsId))
                    {
                        record.RecordsId = idUtil.GenerateUniqueId();
                    }

                    record.AppendCustomColumns();
                }
                catch (Exception e)
                {
                    _logger.Error("Failed to convert FS record DTO to RM base record for NodeId {0}. Exception: {1}", recordDto.NodeId, e);
                    continue;
                }
                deduplicatedRecords[record.NodeId] = record;
               // yield return record;
            }
            foreach (var record in deduplicatedRecords.Values)
            {
                yield return record;
            }
        }

        protected override async Task OnNotifyAsync(RMAzureCosmosDBDelayConcurrentActionResult result)
        {
            var syncedItem = result?.Item;
            if (syncedItem == null)
            {
                _logger.Error(
                    "Cosmos ingestion returned null item. OptimisticConflict: {0}, CanRetry: {1}, Exception: {2}",
                    result?.IsOptimisticLockConflict,
                    result?.CanContinueRetry,
                    result?.Exception);
                return;
            }
            if (!result.IsSucceed)
            {
                await _resultChannel.Writer.WriteAsync(new RMDataIngestionAgentWorkItemExecutionResult
                {
                    Id = syncedItem.Id,
                    Message = BuildMessage(result),
                });

                _logger.Warn("Cosmos ingestion failed for NodeId {0}. OptimisticConflict: {1}, CanRetry: {2}, Exception: {3}",
                    syncedItem.NodeId,
                    result.IsOptimisticLockConflict,
                    result.CanContinueRetry,
                    result.Exception);
            }
            _logger.Info($"Upserted successfully item {syncedItem.NodeId}, , status: {syncedItem.RecordStatus}, destroyedTime: {syncedItem.DestroyedTime} with Action {result.ActionType}");
        }

        private static string BuildMessage(RMAzureCosmosDBDelayConcurrentActionResult result)
        {
            if (result.IsSucceed)
                return "Ingested successfully.";

            return $"Ingestion failed. " +
                   $"OptimisticConflict: {result.IsOptimisticLockConflict}, " +
                   $"CanRetry: {result.CanContinueRetry}, " +
                   $"Error: {result.Exception?.Message}";
        }
    }
}
