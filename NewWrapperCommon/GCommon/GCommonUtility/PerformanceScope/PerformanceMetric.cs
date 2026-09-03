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
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Utility.PerformanceScope
{
    public class PerformanceMetric
    {
        private readonly JobMetrics jobMetrics;

        public PerformanceMetric(
            string moduleName,
            string actionDetail,
            string hierarchyPath,
            string parentAction,
            double durationMilliseconds,
            long memoryDeltaBytes,
            double cpuExitPercent)
        {
            ModuleName = moduleName;
            ActionDetail = actionDetail;
            HierarchyPath = hierarchyPath;
            ParentAction = parentAction ?? string.Empty;
            DurationMilliseconds = durationMilliseconds;
            MemoryDeltaBytes = memoryDeltaBytes;
            CpuUsagePercent = cpuExitPercent;
        }

        public PerformanceMetric(
            string moduleName,
            string actionDetail,
            string hierarchyPath,
            string parentAction,
            double durationMilliseconds,
            JobMetrics jobMetrics)
        {
            ModuleName = moduleName;
            ActionDetail = actionDetail;
            MemoryDeltaBytes = jobMetrics.ExecutionMemoryUsageDeltaBytes;
            CpuUsagePercent = jobMetrics.ProcessCpuUsagePercent;
            ThreadCount = jobMetrics.ThreadCount;
            Timestamp = jobMetrics.Timestamp;
            HierarchyPath = hierarchyPath;
            ParentAction = parentAction ?? string.Empty;
            DurationMilliseconds = durationMilliseconds;
            ExecutionMemoryUsageDeltaBytes = jobMetrics.ExecutionMemoryUsageDeltaBytes;
            ExecutionCpuUsagePercent = jobMetrics.ExecutionCpuUsagePercent;
            ProcessMemoryUsageDeltaBytes = jobMetrics.ProcessMemoryUsageDeltaBytes;
            ProcessCpuUsagePercent = jobMetrics.ProcessCpuUsagePercent;
            MachineMemoryUsageDeltaBytes = jobMetrics.MachineMemoryUsageDeltaBytes;
            MachineCpuUsagePercent = jobMetrics.MachineCpuUsagePercent;
        }

        public string ModuleName { get; }
        public string ActionDetail { get; }
        public string HierarchyPath { get; }
        public string ParentAction { get; }
        public double DurationMilliseconds { get; }
        public long MemoryDeltaBytes { get; }
        public double CpuUsagePercent { get; }
        public int ThreadCount { get; }
        public DateTime Timestamp { get; }
        public long ExecutionMemoryUsageDeltaBytes { get; }
        public double ExecutionCpuUsagePercent { get; }
        public long ProcessMemoryUsageDeltaBytes { get; }
        public double ProcessCpuUsagePercent { get; }
        public long MachineMemoryUsageDeltaBytes { get; }
        public double MachineCpuUsagePercent { get; }
    }

    public readonly struct MetricSnapshot
    {
        public MetricSnapshot(long invocationCount, double totalDurationMs, double maxDurationMs, long latestMemoryDeltaBytes)
        {
            InvocationCount = invocationCount;
            TotalDurationMs = totalDurationMs;
            MaxDurationMs = maxDurationMs;
            LatestMemoryDeltaBytes = latestMemoryDeltaBytes;
        }

        public static MetricSnapshot Empty => new MetricSnapshot(0, 0d, 0d, 0L);

        public long InvocationCount { get; }
        public double TotalDurationMs { get; }
        public double MaxDurationMs { get; }
        public long LatestMemoryDeltaBytes { get; }
        public bool HasValue => InvocationCount > 0;
    }
}
