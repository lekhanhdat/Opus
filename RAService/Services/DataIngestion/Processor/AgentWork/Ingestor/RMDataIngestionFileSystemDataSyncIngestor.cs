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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Protobuf;
using AvePoint.RA.DB.AzureCosmosDB.Model;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.Service.Services.DataIngestion.DataStorage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DataIngestion.Processor.AgentWork.Ingestor
{
    public class RMDataIngestionFileSystemDataSyncIngestor : RMDataIngestionAgentWorkIngestor
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDataIngestionFileSystemDataSyncIngestor));

        public RMDataIngestionFileSystemDataSyncIngestor(RMDataIngestionBlobDataReader dataReader, string messageInfoExtension)
            : base(dataReader, typeof(RMDataIngestionFileSystemDataSyncIngestor))
        {
            _dataReader.RegisterProtobufModel<FileSystemRecordDto>();
        }

        public override RMDataIngestionOperationType OperationType => RMDataIngestionOperationType.FileSystemDataSync;

        protected override async Task OnNotifyAsync(RMAzureCosmosDBDelayConcurrentActionResult result)
        {
            var syncedItem = result?.Item;
            if (syncedItem == null)
            {
                _logger.Error("Synced item is null on Cosmos ingestion result notification.");
                return;
            }
            if (!result.IsSucceed)
            {
                try
                {
                    await _resultChannel.Writer.WriteAsync(new RMDataIngestionAgentWorkItemExecutionResult
                    {
                        Id = syncedItem.Id,
                    });
                    _logger.Warn("Cosmos ingestion failed for NodeId {0}. OptimisticConflict: {1}, CanRetry: {2}, Exception: {3}",
                        syncedItem.NodeId,
                        result.IsOptimisticLockConflict,
                        result.CanContinueRetry,
                        result.Exception?.Message);
                }
                catch (Exception e)
                {
                    _logger.Error("Failed to write ingestion result to channel for NodeId {0}. Exception: {1}", syncedItem.NodeId, e);
                }
            }
        }

        protected override async IAsyncEnumerable<Record> ReadItemsAsync()
        {
            await foreach (var item in _dataReader.ReadItemsAsync<FSDataIngestion<FileSystemRecordDto>>())
            {
                if (item.Item == null) continue;
                yield return ConvertUtil.ConvertFSDtoToRMBaseRecord(item.Item).AppendCustomerColumns4FsJPMCRecord();
            }
        }
    }
}
