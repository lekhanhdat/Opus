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
using System.Threading;
using System.Threading.Tasks;
using AvePoint.GCommon;

namespace RAFileSystem.FileSystem.Collector
{
    public class FileSystemWorkerPoolController : IDisposable
    {
        private readonly AveLogger logger = AveLogger.GetInstance(typeof(FileSystemFileCollector));
        private Func<int, CancellationToken, Task> workerLoop;
        private readonly int minWorkers;
        private readonly int maxWorkers;
        private readonly TimeSpan pollInterval = TimeSpan.FromSeconds(2); // need control from Collector ?
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly Dictionary<int, WorkerState> workers = new Dictionary<int, WorkerState>();
        private readonly object sync = new object();
        private readonly FileSystemWorkerPoolMonitor monitor;
        private bool disposed;
        private readonly Func<int> pendingDirectoryCount;

        public FileSystemWorkerPoolController(
            Func<int, CancellationToken, Task> workerLoop,
            int minWorkers,
            int maxWorkers,
            Func<int> pendingDirectoryCount)
        {
            this.workerLoop = workerLoop;
            this.minWorkers = minWorkers;
            this.maxWorkers = maxWorkers;
            this.pendingDirectoryCount = pendingDirectoryCount ?? (() => 0);

            monitor = new FileSystemWorkerPoolMonitor();
        }

        public void UseNewWorkerLoop(Func<int, CancellationToken, Task> workerLoop)
        {
            this.workerLoop = workerLoop;
        }

        public void Start()
        {
            lock (sync)
            {
                if (disposed) return;
                for (int i = 0; i < minWorkers; i++)
                    AddWorker();
            }
            _ = Task.Run(ControlLoopAsync);
        }

        public void MarkBusy(int workerId, bool busy)
        {
            lock (sync)
            {
                if (workers.TryGetValue(workerId, out var state))
                {
                    state.IsBusy = busy;
                    if (busy) state.LastActiveUtc = DateTime.UtcNow;
                }
            }
        }

        private async Task ControlLoopAsync()
        {
            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(pollInterval, cts.Token).ConfigureAwait(false);

                PoolMetrics metrics;
                int desired;

                lock (sync)
                {
                    int pendingDirectoryCount = this.pendingDirectoryCount();
                    metrics = monitor.CollectMetrics(pendingDirectoryCount, workers.Values);
                    desired = monitor.CalculateDesired(metrics, minWorkers, maxWorkers);
                }

                ScaleTo(desired);
            }
        }

        private void ScaleTo(int target)
        {
            lock (sync)
            {
                if (disposed) return;
                target = Math.Max(minWorkers, Math.Min(maxWorkers, target));
                int current = workers.Count;
                if (current == target) return;

                logger.Debug("Scaling workers from {0} to {1}", current, target);

                if (target > current)
                {
                    for (int i = 0; i < target - current; i++)
                        AddWorker();
                }
                else
                {
                    // Remove oldest idle workers.
                    var removable = workers.Values
                        .Where(w => !w.IsBusy && (DateTime.UtcNow - w.LastActiveUtc) > TimeSpan.FromSeconds(10))
                        .OrderBy(w => w.LastActiveUtc)
                        .Take(current - target)
                        .ToList();

                    foreach (var r in removable)
                        RemoveWorker(r.Id);
                }
            }
        }

        private void AddWorker()
        {
            int id = monitor.GetNextWorkerId();
            var state = new WorkerState(id);
            workers[id] = state;
            state.Task = Task.Run(() => workerLoop(id, state.Cancellation.Token));
        }

        private void RemoveWorker(int id)
        {
            if (workers.TryGetValue(id, out var state))
            {
                state.Cancellation.Cancel();
                workers.Remove(id);
            }
        }

        public void Dispose()
        {
            lock (sync)
            {
                if (disposed) return;
                disposed = true;
                cts.Cancel();
                foreach (var w in workers.Values)
                    w.Cancellation.Cancel();
            }
        }
    }

    public sealed class WorkerState
    {
        public WorkerState(int id)
        {
            Id = id;
            LastActiveUtc = DateTime.UtcNow;
            Cancellation = new CancellationTokenSource();
        }
        public int Id { get; }
        public bool IsBusy;
        public DateTime LastActiveUtc;
        public CancellationTokenSource Cancellation;
        public Task Task;

        public string Summary()
        {
            return $"Worker {Id}, IsBusy={IsBusy}, LastActiveUtc={LastActiveUtc}";
        }
    }
}
