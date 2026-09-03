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
using AvePoint.GCommon.Contract.Server.ControlPanel.PlanGroup.Object;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Utility.PerformanceScope
{
    public class ScopeFrame
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private readonly JobMetrics _startMetrics;
        public string ModuleName { get; }
        public string ActionDetail { get; }
        public ScopeFrame Parent { get; }
        public string HierarchyPath { get; }
        public bool CollectAggregate { get; }
        public PerformanceTelemetryOptions Options { get; }


        public ScopeFrame(string moduleName, string actionDetail, ScopeFrame parent, bool collectAggregate, PerformanceTelemetryOptions options)
        {
            ModuleName = moduleName;
            ActionDetail = actionDetail;
            Parent = parent;
            CollectAggregate = collectAggregate;
            HierarchyPath = BuildHierarchy(moduleName, actionDetail, parent);
            Options = options;
            _startMetrics = new JobMetrics();
        }

        public ScopeFrame(string moduleName, string actionDetail, ScopeFrame parent, bool collectAggregate, PerformanceTelemetryOptions options, JobMetrics startMetrics)
        {
            ModuleName = moduleName;
            ActionDetail = actionDetail;
            Parent = parent;
            CollectAggregate = collectAggregate;
            HierarchyPath = BuildHierarchy(moduleName, actionDetail, parent);
            Options = options;
            _startMetrics = startMetrics ?? new JobMetrics();
        }

        public PerformanceMetric Complete(double cpuUsage, long memoryUsage)
        {
            _stopwatch.Stop();
            var durationMs = _stopwatch.Elapsed.TotalMilliseconds;

            return new PerformanceMetric(
                    ModuleName,
                    ActionDetail,
                    HierarchyPath,
                    Parent?.ActionDetail,
                    durationMs,
                    memoryUsage,
                    cpuUsage
                );
        }
        
        public PerformanceMetric Complete(JobMetrics jobMetrics)
        {
            _stopwatch.Stop();
            var durationMs = _stopwatch.Elapsed.TotalMilliseconds;
            ApplyExecutionMetrics(jobMetrics);

            return new PerformanceMetric(
                ModuleName,
                ActionDetail,
                HierarchyPath,
                Parent?.ActionDetail,
                durationMs,
                jobMetrics
            );
        }

        private void ApplyExecutionMetrics(JobMetrics jobMetrics)
        {
            if (jobMetrics == null)
            {
                return;
            }

            jobMetrics.ExecutionMemoryUsageDeltaBytes = CalculateMemoryDelta(jobMetrics.ProcessMemoryUsageDeltaBytes, _startMetrics.ProcessMemoryUsageDeltaBytes);
            jobMetrics.ExecutionCpuUsagePercent = CalculateCpuDelta(jobMetrics.ProcessCpuUsagePercent, _startMetrics.ProcessCpuUsagePercent);
        }

        private static long CalculateMemoryDelta(long currentValue, long startValue)
        {
            var delta = currentValue - startValue;
            return delta > 0 ? delta : 0;
        }

        private static double CalculateCpuDelta(double currentValue, double startValue)
        {
            var delta = currentValue - startValue;
            return delta > 0d ? delta : 0d;
        }

        private static string BuildHierarchy(string moduleName, string actionDetail, ScopeFrame parent)
        {
            if (parent == null)
            {
                return $"{moduleName}:{actionDetail}";
            }

            return $"{parent.HierarchyPath} > {actionDetail}";
        }

        public class MetricAccumulator
        {
            private readonly object _sync = new object();
            private long _count;
            private double _totalDuration;
            private double _maxDuration;

            public MetricSnapshot Update(PerformanceMetric metric)
            {
                lock (_sync)
                {
                    var memoryDeltaBytes = metric.MemoryDeltaBytes;
                    var durationMs = metric.DurationMilliseconds;
                    if (!string.IsNullOrEmpty(metric.ParentAction))
                    {
                        _count++;
                        _totalDuration += durationMs;
                        _maxDuration = Math.Max(_maxDuration, durationMs);
                    }
                    else
                    {
                        _totalDuration = durationMs;
                    }
                    return new MetricSnapshot(_count, _totalDuration, _maxDuration, memoryDeltaBytes);
                }
            }
        }
    }
}
