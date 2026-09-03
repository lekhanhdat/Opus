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
using AvePoint.RA.DB.Model.DataIngestion;
using AvePoint.RA.Service.Services.DataIngestion.DataStorage;
using AvePoint.RA.Service.Services.DataIngestion.Processor.AgentWork.Ingestor;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DataIngestion.Processor.AgentWork
{
    public class RMDataIngestionAgentWorkMessageProcessor : RMDataIngestionMessageProcessor
    {
        public override RMDataIngestionType IngestionType => RMDataIngestionType.AgentWork;

        public RMDataIngestionAgentWorkMessageProcessor(RMDataIngestionMessage message) : base(message, typeof(RMDataIngestionAgentWorkMessageProcessor))
        {
        }

        protected override async Task<(RMDataIngestionStatus, string)> ProcessMessageAsync(RMDataIngestionMessage messageInfo)
        {
            try
            {
                _logger.Info($"Start processing data ingestion message {messageInfo.SourceBlobName} for {IngestionType}");

                var blobHandler = RMDataIngestionAzureStorageBlobHandlerFactory.Create(IngestionType);
                var blobReference = await blobHandler.GenerateBlobReferenceAsync(new RMDataIngestionBlobNamingContext
                {
                    UniqueId = messageInfo.UniqueId,
                    IngestionType = IngestionType,
                    OperationType = messageInfo.OperationType,
                    BlobType = RMDataIngestionBlobType.Result
                });
                await using var reportWriter = await RMDataIngestionBlobDataWriter.CreateAsync(blobReference.BlobName, blobHandler);
                
                var reader = new RMDataIngestionBlobDataReader(messageInfo.SourceBlobName, blobHandler);

                var dataIngestor = RMDataIngestionAgentWorkIngestorFactory.Create(messageInfo.OperationType, reader, messageInfo.Extension);

                var ingestionTask = dataIngestor.IngestAsync();

                await foreach (var result in dataIngestor.ReadItemExecutionResultsAsync())
                {
                    reportWriter.WriteItem(result);
                }

                await ingestionTask;

                await reader.CompleteAsync();

                _logger.Info($"End processing data ingestion message {messageInfo.SourceBlobName} for {IngestionType}");

                return (RMDataIngestionStatus.Succeed, blobReference.BlobName);

            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to process data ingestion message {messageInfo.SourceBlobName} for {IngestionType}, Ex: {ex.Message}.");
                return (RMDataIngestionStatus.Failed, messageInfo.SourceBlobName);
            }
        }
    }
}
