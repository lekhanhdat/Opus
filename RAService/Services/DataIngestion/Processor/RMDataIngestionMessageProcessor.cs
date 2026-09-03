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
using AvePoint.RA.DB.AzureTable;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.Model.DataIngestion;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DataIngestion.Processor
{
    public abstract class RMDataIngestionMessageProcessor
    {
        protected readonly RALogger _logger;

        private RMDataIngestionMessage _message;

        public abstract RMDataIngestionType IngestionType { get; }

        public RMDataIngestionMessageProcessor(RMDataIngestionMessage message, Type loggerType)
        {
            _message = message;
            _logger = RALogger.GetInstance(loggerType);
        }

        public async Task<bool> ProcessAsync()
        {
            try
            {
                _logger.Info($"Start processing data ingestion for {IngestionType} message: {_message.Id} ");
                await RMRecordStorageAzureTableContext.DataIngestionExecuteResultList.UpsertMerge(new RMDataIngestionExecutionResultTableEntity
                {
                    SourceBlobName = _message.SourceBlobName,
                    PartitionKey = _message.UniqueId,
                    RowKey = _message.Id.ToString(),
                    Status = (int)RMDataIngestionStatus.InProgress,
                    ModifiedTime = DateTime.UtcNow.Ticks,
                });

                (RMDataIngestionStatus status, string resultBlobName) = await ProcessMessageAsync(_message);

                await RMRecordStorageAzureTableContext.DataIngestionExecuteResultList.UpsertMerge(new RMDataIngestionExecutionResultTableEntity
                {
                    ResultBlobName = resultBlobName,
                    PartitionKey = _message.UniqueId,
                    RowKey = _message.Id.ToString(),
                    Status = (int)status,
                    ModifiedTime = DateTime.UtcNow.Ticks,
                });

                _logger.Info($"End processing data ingestion for {IngestionType} message: {_message.Id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while processing data ingestion for {IngestionType} message: {_message.Id}. Error: {ex}");
                return false;
            }
        }

        protected abstract Task<(RMDataIngestionStatus, string)> ProcessMessageAsync(RMDataIngestionMessage messageInfo);
    }
}
