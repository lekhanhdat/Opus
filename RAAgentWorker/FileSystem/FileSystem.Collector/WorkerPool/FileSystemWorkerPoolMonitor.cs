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
using AvePoint.GCommon;
using Hardware.Info;

namespace RAFileSystem.FileSystem.Collector
{
    public class FileSystemWorkerPoolMonitor
    {
        private readonly AveLogger logger = AveLogger.GetInstance(typeof(FileSystemFileCollector));
        private readonly HardwareInfo hwInfo = new HardwareInfo();
        private int lastDesired = -1;
        private int stableCounter;
        private int nextWorkerId;
        private readonly object sync = new object();

        // collect current metrics and worker states
        public PoolMetrics CollectMetrics(int pendingDirectoryCount, IEnumerable<WorkerState> workerStates)
        {
            try
            {
                hwInfo.RefreshCPUList();
                hwInfo.RefreshMemoryStatus();
            }
            catch
            {
                // what if hardware info fails ?
            }

            var states = workerStates.ToList();
            int busy = states.Count(s => s.IsBusy);
            double cpuLoad = hwInfo.CpuList.Count == 0 ? 0
                : hwInfo.CpuList.Sum(c => (double)c.PercentProcessorTime) / hwInfo.CpuList.Count;
            long availableMemMb = (long)hwInfo.MemoryStatus.AvailablePhysical / (1024 * 1024);

            return new PoolMetrics
            {
                WorkerTotal = states.Count,
                BusyWorkers = busy,
                IdleWorkers = states.Count - busy,
                CpuLoadPercent = cpuLoad,
                AvailableMemoryMb = availableMemMb,
                QueueLength = pendingDirectoryCount
            };
        }

        // decide desired worker count based on metrics
        // need optimize
        public int CalculateDesired(PoolMetrics m, int minWorkers, int maxWorkers)
        {
            if (m.WorkerTotal == 0) return minWorkers;

            logger.Debug("Pool Metrics: {0}", m.Summary());

            int desired = m.WorkerTotal;
            double busyRatio = (double)m.BusyWorkers / m.WorkerTotal;
            int pendingPerWorker = m.WorkerTotal == 0 ? 0 : m.QueueLength / m.WorkerTotal;

            if (pendingPerWorker >= 4 && busyRatio >= 0.7 && m.CpuLoadPercent < 70 && m.AvailableMemoryMb > 400)
            {
                int grow = Math.Max(2, m.WorkerTotal / 4);
                desired = Math.Min(maxWorkers, m.WorkerTotal + grow);
            }
            else if (pendingPerWorker >= 2 && busyRatio >= 0.6 && m.CpuLoadPercent < 80)
            {
                desired = Math.Min(maxWorkers, m.WorkerTotal + 1);
            }

            else if (m.QueueLength == 0 && busyRatio < 0.3 && m.WorkerTotal > minWorkers)
            {
                desired = Math.Max(minWorkers, m.WorkerTotal - 1);
            }

            if (desired == lastDesired)
            {
                stableCounter++;
            }
            else
            {
                stableCounter = 0;
            }

            lastDesired = desired;

            return stableCounter < 1 ? m.WorkerTotal : desired;
        }
        public int GetNextWorkerId()
        {
            int newValue = Interlocked.Increment(ref nextWorkerId);
            if (newValue == int.MaxValue)
            {
                // reset to 0
                lock (sync)
                {
                    if (nextWorkerId == int.MaxValue)
                        nextWorkerId = 0;
                }
                newValue = Interlocked.Increment(ref nextWorkerId);
            }
            return newValue;
        }
    }

    public sealed class PoolMetrics
    {
        public int WorkerTotal { get; set; }
        public int BusyWorkers { get; set; }
        public int IdleWorkers { get; set; }
        public int QueueLength { get; set; }
        public double CpuLoadPercent { get; set; }
        public long AvailableMemoryMb { get; set; }

        public string Summary()
        {
            return $"Workers: {WorkerTotal} (Busy: {BusyWorkers}, Idle: {IdleWorkers}), Queue: {QueueLength}, CPU Load: {CpuLoadPercent:F1}%, Available Mem: {AvailableMemoryMb} MB";
        }
    }
}
