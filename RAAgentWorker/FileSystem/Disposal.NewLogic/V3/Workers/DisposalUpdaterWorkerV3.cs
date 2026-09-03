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
using System.Threading;
using System.Threading.Tasks;
using AvePoint.GCommon;
using RAFileSystem.Disposal.NewLogic;

namespace RAFileSystem.FileSystem.Disposal.NewLogic.V3.Workers
{
    /// <summary>
    /// Wraps DisposalDataUpdaterV2.Run() as a producer-consumer worker.
    /// Reads from WorkerToUpdater channel, performs data ingestion updates, writes to DiscoveryToCosmos channel.
    /// </summary>
    public class DisposalUpdaterWorkerV3 : IFSDisposalWorker
    {
        private readonly AveLogger logger = AveLogger.GetInstance(typeof(DisposalUpdaterWorkerV3));
        private readonly CancellationToken token;

        public DisposalUpdaterWorkerV3(CancellationToken token)
        {
            this.token = token;
        }

        public async Task RunAsync()
        {
            try
            {
                logger.Info("Start disposal updater worker.");
                token.ThrowIfCancellationRequested();
                var updater = new DisposalDataUpdaterV2();
                await updater.Run().ConfigureAwait(false);
                logger.Info("Disposal updater worker completed.");
            }
            catch (OperationCanceledException)
            {
                logger.Warn("Disposal updater worker canceled.");
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred in disposal updater worker. Error: {0}", ex);
                throw;
            }
        }
    }
}
