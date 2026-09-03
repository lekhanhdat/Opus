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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.GCommon;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.FileSystem.Disposal.NewLogic.V3.Services;

namespace RAFileSystem.FileSystem.Disposal.NewLogic.V3.Workers
{
    /// <summary>
    /// Wraps DisposalDiscoverV2.RunSendRecordsToCosmos() as a producer-consumer worker.
    /// Reads from DiscoveryToCosmos channel and sends records in batches to Cosmos DB.
    /// </summary>
    internal class DisposalCosmosSenderWorkerV3 : IFSDisposalWorker
    {
        private readonly FSDisposalChannelProvider _channel;
        private readonly DisposalReportService _reportService;

        private readonly AveLogger logger = AveLogger.GetInstance(typeof(DisposalCosmosSenderWorkerV3));

        public DisposalCosmosSenderWorkerV3(
            FSDisposalChannelProvider channel,
            DisposalReportService reportService)
        {
            _channel = channel;

            _reportService = reportService;
        }

        public async Task RunAsync()
        {
            var buffer = new List<FSAzureTableEntityDto>(ExternalUtil.TransferDataCount);

            while (await _channel.DiscoveryToCosmoChannel.Reader.WaitToReadAsync().ConfigureAwait(false))
            {
                while (_channel.DiscoveryToCosmoChannel.Reader.TryRead(out var dto))
                {
                    buffer.Add(dto);
                    if (buffer.Count >= ExternalUtil.TransferDataCount)
                    {
                        await FlushCosmosAsync(buffer).ConfigureAwait(false);
                        buffer.Clear();
                    }
                }
            }

            if (buffer.Count > 0)
            {
                await FlushCosmosAsync(buffer).ConfigureAwait(false);
            }

            JobContext.Current.SendDataToCosmosFinish = true;
            JobContext.Current.SendDataToAzureTableFinish = true;
        }

        private async Task FlushCosmosAsync(List<FSAzureTableEntityDto> batch)
        {
            if (batch == null || batch.Count == 0) return;

            var dtoInfo = new FSAzureTableEntityDtoWithJobId
            {
                JobId = JobContext.Current.JobId,
                EntityDtos = batch,
                IsFSHighPerformanceMode = true
            };

            List<Guid> failedIds;
            using (new AgentPerformanceScope("DisposalDiscover.AddScanData",
                       $"DisposalDiscover.AddScanData.Count:{batch.Count}", true))
            {
                failedIds = HybridApiClient.Instance.AddScanDataToCosmos(dtoInfo);

                var archived = batch.Where(a => a.Status == (int)SOApproveDBStatus.Archived).ToList();
                if (archived.Count > 0)
                {
                    HybridApiClient.Instance.AddScanData(archived);
                }
            }
            
            var sendable = batch.Where(e => !e.NoNeedSendReport).ToList();
            if (sendable.Count > 0)
            {
                _reportService.CommitCosmosSenderDetails(sendable, failedIds);
            }

            if (failedIds.Count > 0)
            {
                logger.Warn("Failed to add fs archived data to cosmos. File ids:{0}", string.Join(",", failedIds));
            }
        }
    }
}
