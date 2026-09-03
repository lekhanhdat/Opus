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
using AvePoint.RA.Contract.Explorer;
using AvePoint.GCommon;
using AvePoint.RA.FileSystem.Stubs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.DataSync.V2
{
    public class FSDataSyncChannelProvider
    {
        private static readonly AveLogger _logger = AveLogger.GetInstance(typeof(FSDataSyncChannelProvider));
        private const int DEFAULT_CAPACITY = 10_000;
        private readonly Task[] _readerCompletions;
        private int _discoverActiveItemCount = 0;

        public Channel<Stub> DiscoverChannel { get; }
        public Channel<Stub> AnalyzerChannel { get; }
        public Channel<FileSystemRecordDto> PersistChannel { get; }
        public ConcurrentStack<Stub> DiscoverOverflowStack { get; } = new ConcurrentStack<Stub>();
        public SemaphoreSlim PersistGate = new SemaphoreSlim(ConfigUtils.MAX_INFLIGHT_COUNT);
        public SemaphoreSlim ReportGate = new SemaphoreSlim(ConfigUtils.MAX_INFLIGHT_COUNT);

        public FSDataSyncChannelProvider()
        {
            DiscoverChannel = CreateChannel<Stub>();
            AnalyzerChannel = CreateChannel<Stub>();
            PersistChannel = CreateChannel<FileSystemRecordDto>();
            _readerCompletions = new[]
            {
            DiscoverChannel.Reader.Completion,
            AnalyzerChannel.Reader.Completion,
            PersistChannel.Reader.Completion
            };
        }

        private static Channel<T> CreateChannel<T>(int count = DEFAULT_CAPACITY)
        {
            return Channel.CreateBounded<T>(new BoundedChannelOptions(count)
            {
                SingleReader = false,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            });
        }

        public void IncreaseDiscoveryCount(int count = 1) => Interlocked.Add(ref _discoverActiveItemCount, count);

        public void DecreaseDiscoveryCount()
        {
            if (Interlocked.Decrement(ref _discoverActiveItemCount) == 0 && DiscoverOverflowStack.IsEmpty)
            {
                _logger.Info("All discovery items processed. Completing DiscoverChannel.");
                DiscoverChannel.Writer.TryComplete();
            }
        }

        public Task WriteToDiscoverAsync(Stub stub, CancellationToken token) => DiscoverChannel.Writer.WriteWithRetryAsync(stub, token);

        public Task WriteToAnalyzerAsync(Stub stub, CancellationToken token) => AnalyzerChannel.Writer.WriteWithRetryAsync(stub, token);

        public Task WriteToPersistAsync(FileSystemRecordDto dto, CancellationToken token) => PersistChannel.Writer.WriteWithRetryAsync(dto, token);

        public Task WriteBatchToDiscoverAsync(List<Stub> stubs, CancellationToken token) => DiscoverChannel.Writer.WriteBatchWithRetryAsync(stubs, ConfigUtils.WORKER_TRANSFER_DATA_COUNT, token);

        public Task WriteBatchToAnalyzerAsync(List<Stub> stubs, CancellationToken token) => AnalyzerChannel.Writer.WriteBatchWithRetryAsync(stubs, ConfigUtils.WORKER_TRANSFER_DATA_COUNT, token);

        public Task WriteBatchToPersistAsync(List<FileSystemRecordDto> dtos, CancellationToken token) => PersistChannel.Writer.WriteBatchWithRetryAsync(dtos, ConfigUtils.WORKER_TRANSFER_DATA_COUNT, token);

        public void SetCompleteAll()
        {
            DiscoverChannel.Writer.TryComplete();
            AnalyzerChannel.Writer.TryComplete();
            PersistChannel.Writer.TryComplete();
        }

        public async Task WaitToCompletePipelineAsync(IEnumerable<Task> discoverWorkers, IEnumerable<Task> analyzerWorkers, IEnumerable<Task> persistWorkers, IEnumerable<Task> reportWorkers, Action additionalAction)
        {
            await Task.WhenAll(discoverWorkers);
            AnalyzerChannel.Writer.TryComplete();

            await Task.WhenAll(analyzerWorkers);
            PersistChannel.Writer.TryComplete();

            await Task.WhenAll(persistWorkers);
            additionalAction();

            await Task.WhenAll(reportWorkers);
        }

        public Task WaitForAllReadersCompletedAsync() => Task.WhenAll(_readerCompletions);
    }
}
