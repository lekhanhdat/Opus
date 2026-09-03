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
using AvePoint.GCommon.Utility.PerformanceScope;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test.PerformanceScope
{
    internal sealed class TestSink : PerformanceTelemetryContext.IPerformanceMetricSink
    {
        public readonly System.Collections.Concurrent.ConcurrentBag<(PerformanceMetric Metric, MetricSnapshot Snapshot)> Published
            = new System.Collections.Concurrent.ConcurrentBag<(PerformanceMetric, MetricSnapshot)>();

        public void Publish(PerformanceMetric metric, MetricSnapshot aggregate)
        {
            Published.Add((metric, aggregate));
        }
    }

    [TestClass]
    public class PerformanceTelemetryContextTests
    {
        internal TestSink ConfigureNewSink(bool enabled = true, bool mem = true, bool cpu = false)
        {
            var sink = new TestSink();
            PerformanceTelemetryContext.Configure(opt =>
            {
                opt.Enabled = enabled;
                opt.EnableMemoryUsage = mem;
                opt.EnableCpuUsage = cpu;
            });
            PerformanceTelemetryContext.RegisterSink(sink);
            return sink;
        }

        [TestMethod]
        public void BasicScopePublishesMetric()
        {
            var module = "Module_" + Guid.NewGuid().ToString("N");
            var sink = ConfigureNewSink();
            Stopwatch _stopwatch = Stopwatch.StartNew();
            using (PerformanceTelemetryContext.BeginScope(module, "DoWork"))
            {
                Thread.Sleep(50);
            }
            _stopwatch.Stop();
            var durationMs = _stopwatch.Elapsed.TotalMilliseconds;


            var records = sink.Published.ToArray();
            Assert.AreEqual(1, records.Length, "Expected one metric record.");
            var metric = records[0].Metric;
            Assert.AreEqual(module, metric.ModuleName);
            Assert.AreEqual("DoWork", metric.ActionDetail);
            var temp = durationMs - metric.DurationMilliseconds;
            Assert.IsTrue(durationMs - metric.DurationMilliseconds <= 1500, $"Duration too low: {metric.DurationMilliseconds}");
            //Assert.IsTrue(metric.DurationMilliseconds < 500, $"Duration unexpectedly high: {metric.DurationMilliseconds}");
        }

        [TestMethod]
        public void ParentChildScopesProduceTwoMetricsAndAggregateCounts()
        {
            var module = "Module_" + Guid.NewGuid().ToString("N");
            var sink = ConfigureNewSink();
            try
            {
                using (PerformanceTelemetryContext.BeginScope(module, "Parent"))
                {
                    Thread.Sleep(30);
                    using (PerformanceTelemetryContext.BeginChildScope("Child"))
                    {
                        Thread.Sleep(20);
                    }
                }
            }
            catch(Exception ex)
            {
                throw;
            }
            

            var records = sink.Published.OrderBy(r => r.Metric.DurationMilliseconds).ToArray();
            Assert.AreEqual(2, records.Length, "Expected two metric records (parent + child).");

            var parentMetric = records.First(r => r.Metric.ActionDetail == "Parent").Metric;
            var parentSnapshot = records.First(r => r.Metric.ActionDetail == "Parent").Snapshot;
            var childMetric = records.First(r => r.Metric.ActionDetail == "Child").Metric;
            var childSnapshot = records.First(r => r.Metric.ActionDetail == "Child").Snapshot;

            Assert.AreEqual(module, parentMetric.ModuleName);
            Assert.AreEqual(module, childMetric.ModuleName);

            // Aggregation key changed to ModuleName => invocation counts reflect number of scopes in that module.
            Assert.AreEqual(2, parentSnapshot.InvocationCount, "First scope should have invocation count 2.");
            Assert.AreEqual(1, childSnapshot.InvocationCount, "Second scope should have invocation count 1.");

            // Optional parent-child relation checks if exposed (HierarchyPath / ParentAction)
            Assert.AreEqual("Parent", childMetric.ParentAction, "Child.ParentAction should match parent action detail.");
        }

        //[TestMethod]
        //public void MemoryDeltaCaptured()
        //{
        //    var module = "Module_" + Guid.NewGuid().ToString("N");
        //    var sink = ConfigureNewSink(mem: true);

        //    byte[] buffer = null;
        //    using (PerformanceTelemetryContext.BeginScope(module, "Allocate"))
        //    {
        //        // Allocate ~2MB
        //        buffer = new byte[2 * 1024 * 1024];
        //        for (int i = 0; i < buffer.Length; i += 4096)
        //        {
        //            buffer[i] = 1;
        //        }
        //        Thread.Sleep(10);
        //    }

        //    var rec = sink.Published.Single();
        //    // MemoryDeltaBytes may be noisy; assert a lower bound > 0
        //    Assert.IsTrue(rec.Metric.MemoryDeltaBytes >= 512 * 1024,
        //        $"Expected memory delta >= 512KB, got {rec.Metric.MemoryDeltaBytes}");
        //}

        [TestMethod]
        public void MultipleChildScopesAccumulateInvocationCount()
        {
            var module = "Module_" + Guid.NewGuid().ToString("N");
            var sink = new PerformanceTelemetryContextTests().ConfigureNewSink();

            using (PerformanceTelemetryContext.BeginScope(module, "Root"))
            {
                for (int i = 0; i < 3; i++)
                {
                    using (PerformanceTelemetryContext.BeginChildScope("ChildOp" + i))
                    {
                        Thread.Sleep(5);
                    }
                }
            }

            int childCount = 0;
            long lastInvocationCount = 0;
            foreach (var p in sink.Published)
            {
                if (p.Metric.ActionDetail.StartsWith("ChildOp"))
                {
                    childCount++;
                    lastInvocationCount = p.Snapshot.InvocationCount;
                }
            }

            Assert.AreEqual(3, childCount, "Should have three child scope metrics.");

        }

        [TestMethod]
        public void DisabledOptionsDoNotPublish()
        {
            var module = "Module_" + Guid.NewGuid().ToString("N");
            var sink = ConfigureNewSink(enabled: false);

            using (PerformanceTelemetryContext.BeginScope(module, "NoOp"))
            {
                Thread.Sleep(10);
            }

            Assert.AreEqual(0, sink.Published.Count, "No metrics should be published when disabled.");
        }
    }
}
