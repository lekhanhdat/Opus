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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualBasic.Devices;
using static AvePoint.GCommon.Utility.PerformanceScope.ScopeFrame;

namespace AvePoint.GCommon.Utility.PerformanceScope
{
    public class PerformanceTelemetryContext
    {
        private static readonly AveLogger sLogger = AveLogger.GetInstance(typeof(PerformanceTelemetryContext));
        private static PerformanceTelemetryOptions sOptions = PerformanceTelemetryOptions.CreateDefault();
        private static readonly AsyncLocal<ScopeFrame> sCurrentFrame = new AsyncLocal<ScopeFrame>();
        private static readonly ConcurrentDictionary<string, MetricAccumulator> sAggregates = new ConcurrentDictionary<string, MetricAccumulator>();
        private static readonly object sSinkLock = new object();
        private static CpuUsageCount sCpuSampler;
        private static readonly object sOptionLock = new object();
        private static readonly List<IPerformanceMetricSink> sSinks = new List<IPerformanceMetricSink>();
        private static readonly IPerformanceMetricSink sDefaultSink = CreateDefaultSink();

        static PerformanceTelemetryContext()
        {
            LogInitializationComputerInfo();

            if (sOptions.EnableCpuUsage)
            {
                sCpuSampler = new CpuUsageCount();
            }
            sSinks.Add(sDefaultSink);
        }

        private static IPerformanceMetricSink CreateDefaultSink()
        {
            try
            {
                return new MetricLoggerPerformanceSink();
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to create AveLoggerPerformanceSink. Falling back to NullPerformanceMetricSink. Error: {ex.Message}");
                return new NullPerformanceMetricSink();
            }
        }
        public static void Configure(Action<PerformanceTelemetryOptions> configure)
        {
            lock (sOptionLock)
            {
                var clone = sOptions.Clone();
                configure(clone);
                sOptions = clone;
            }
        }

        public static void RegisterSink(IPerformanceMetricSink performanceMetricSink)
        {
            lock (sSinks)
            {
                if (!sSinks.Contains(performanceMetricSink))
                {
                    sSinks.Add(performanceMetricSink);
                }
            }
        }

        public static IDisposable BeginScope(string moduleName, string actionDetail, bool collectAggregate = true)
        {
            return CreateScope(moduleName, actionDetail, null, collectAggregate);
        }

        public static IDisposable BeginChildScope(string actionDetail, bool collectAggregate = true)
        {
            var parent = sCurrentFrame.Value;
            return CreateScope(parent.ModuleName, actionDetail, parent, collectAggregate);
        }

        private static IDisposable CreateScope(string moduleName, string actionDetail, ScopeFrame parent, bool collectAggregate)
        {
            var startMetrics = GetCurrentJobMetrics();
            var scopeFrame = new ScopeFrame(moduleName, actionDetail, parent, collectAggregate, sOptions, startMetrics);
            sCurrentFrame.Value = scopeFrame;
            return new ScopeHandle(scopeFrame);
        }

        private static void CompleteScope(ScopeFrame frame)
        {
            sCurrentFrame.Value = frame.Parent;
            if (!frame.Options.Enabled)
            {
                return;
            }
            
            var jobMetrics = GetCurrentJobMetrics();
            var metric = frame.Complete(jobMetrics);
            var snapshot = frame.CollectAggregate ? UpdateAggregates(metric) : MetricSnapshot.Empty;
            Publish(metric, snapshot);
        }

        private static JobMetrics GetCurrentJobMetrics()
        {
            if (!sOptions.EnableCpuUsage && !sOptions.EnableMemoryUsage)
            {
                return new JobMetrics();
            }

            return sCpuSampler.GetMetrics();
        }

        private static MetricSnapshot UpdateAggregates(PerformanceMetric metric)
        {
            var key = metric.ModuleName;
            var accumulator = sAggregates.GetOrAdd(key, _ => new MetricAccumulator());
            return accumulator.Update(metric);
        }

        private static void Publish(PerformanceMetric metric, MetricSnapshot snapshot)
        {
            lock (sSinkLock)
            {
                foreach (var sink in sSinks)
                {
                    sink.Publish(metric, snapshot);
                }
            }
        }

        public interface IPerformanceMetricSink
        {
            void Publish(PerformanceMetric metric, MetricSnapshot aggregate);
        }

        private class NullPerformanceMetricSink : IPerformanceMetricSink
        {
            public void Publish(PerformanceMetric metric, MetricSnapshot aggregate)
            {
            }
        }

        public class ScopeHandle : IDisposable
        {
            public ScopeHandle() { }

            private readonly ScopeFrame _frame;
            private bool _disposed;

            public ScopeHandle(ScopeFrame frame)
            {
                _frame = frame;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                if (_frame == null)
                {
                    return;
                }

                CompleteScope(_frame);
            }
        }

        private static void LogInitializationComputerInfo()
        {
            try
            {
                var computerInfo = new ComputerInfo();
                sLogger.Info(
                    "PerformanceTelemetryContext initialized. MachineName={0}, OSVersion={1}, Is64BitOperatingSystem={2}, Is64BitProcess={3}, ProcessorCount={4}, TotalPhysicalMemoryMb={5}, AvailablePhysicalMemoryMb={6}",
                    Environment.MachineName,
                    Environment.OSVersion,
                    Environment.Is64BitOperatingSystem,
                    Environment.Is64BitProcess,
                    Environment.ProcessorCount,
                    ConvertBytesToMb((long)computerInfo.TotalPhysicalMemory),
                    ConvertBytesToMb((long)computerInfo.AvailablePhysicalMemory));
            }
            catch (Exception ex)
            {
                sLogger.Warn(
                    "PerformanceTelemetryContext initialized, but failed to read ComputerInfo. MachineName={0}, OSVersion={1}, ProcessorCount={2}, Error={3}",
                    Environment.MachineName,
                    Environment.OSVersion,
                    Environment.ProcessorCount,
                    ex.Message);
            }
        }

        private static long ConvertBytesToMb(long bytes)
        {
            return bytes <= 0 ? 0 : bytes / 1024 / 1024;
        }
    }
}
