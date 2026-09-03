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
using HybridServer.EF;
using HybridServer.EF.Entity;
using HybridServer.Log;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HybridServer.Services
{
    public class HeartbeatProcessorService : BackgroundService
    {
        private IInMemoryHeartbeatQueue _heartbeatQueue;
        private readonly IServiceProvider _serviceProvider;
        private readonly AveLogger logger = AveLogger.GetInstance(typeof(HeartbeatProcessorService));
        private const int BATCH_SIZE = 500; 
        private readonly TimeSpan FLUSH_INTERVAL = TimeSpan.FromSeconds(30);
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public HeartbeatProcessorService(IServiceProvider serviceProvider, IInMemoryHeartbeatQueue queue)
        {
            _serviceProvider = serviceProvider;
            _heartbeatQueue = queue;
        }
        

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // read from queue
            var batch = new Dictionary<string, Agent>();
            var readTask = ReadFromQueue(batch, stoppingToken);
            var flushTask = PeriodicallyFlush(batch, stoppingToken);

            await Task.WhenAny(readTask, flushTask);
        }

        private async Task ReadFromQueue(Dictionary<string, Agent> batch, CancellationToken ct)
        {
            await foreach (var item in _heartbeatQueue.ReadAllAsync(ct))
            {
                await _lock.WaitAsync(ct);
                try
                {
                    batch[item.AgentId] = item;
                    if (batch.Count >= BATCH_SIZE)
                    {
                        await FlushToDbAsync(batch);
                    }
                }
                finally
                {
                    _lock.Release(); 
                }
            }

        }

        private async Task PeriodicallyFlush(Dictionary<string, Agent> batch, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(FLUSH_INTERVAL, ct);

                await _lock.WaitAsync(ct);
                try
                {
                    if (batch.Count > 0)
                    {
                        await FlushToDbAsync(batch);
                    }
                }
                finally
                {
                    _lock.Release(); 
                }
            }
        }

        private async Task FlushToDbAsync(Dictionary<string, Agent> batch)
        {
            var itemsToProcess = batch.Values.ToList();
            logger.Info($"Start to flush agents to db, item counts: {itemsToProcess.Count}");
            batch.Clear();

            using (var scope = _serviceProvider.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<SignalRRepository>();
                var dbContext = scope.ServiceProvider.GetRequiredService<SignalRRDBContext>();
                try
                {
                    await repository.BulkMergeAsync(itemsToProcess);
                }
                catch (Exception ex)
                {
                    logger.Error("Error flushing heartbeat: " + ex.ToString());
                }
            }
        }
    }
}
