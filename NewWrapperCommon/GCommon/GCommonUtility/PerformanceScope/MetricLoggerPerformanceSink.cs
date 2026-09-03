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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using static AvePoint.GCommon.Utility.PerformanceScope.PerformanceTelemetryContext;

namespace AvePoint.GCommon.Utility.PerformanceScope
{
    public class MetricLoggerPerformanceSink : IPerformanceMetricSink
    {
        private readonly AveLogger _logger = AveLogger.GetInstance(typeof(MetricLoggerPerformanceSink));

        public void Publish(PerformanceMetric metric, MetricSnapshot aggregate)
        {
            var aggregatePart = aggregate.HasValue
                ? $"count={aggregate.InvocationCount}, totalMs={aggregate.TotalDurationMs:F2}, maxMs={aggregate.MaxDurationMs:F2}"
                : "count=1, totalMs=N/A, maxMs=N/A";

            _logger.Metric(
                "PerformanceScope module={0}, action={1}, parent={2}, hierarchy={3}, durationMs={4:F2}, executionMemoryUsageBytes={5}, executionCpuUsagePercent={6:F2}, processMemoryUsageBytes={7}, processCpuUsagePercent={8:F2}, machineMemoryUsageBytes={9}, machineCpuUsagePercent={10:F2}, threadCount={11}, timestamp={12}, {13}",
                metric.ModuleName,
                metric.ActionDetail,
                metric.ParentAction,
                metric.HierarchyPath,
                metric.DurationMilliseconds,
                metric.ExecutionMemoryUsageDeltaBytes,
                metric.ExecutionCpuUsagePercent,
                metric.ProcessMemoryUsageDeltaBytes,
                metric.ProcessCpuUsagePercent,
                metric.MachineMemoryUsageDeltaBytes,
                metric.MachineCpuUsagePercent,
                metric.ThreadCount,
                metric.Timestamp.ToString("MM-dd HH:mm:ss,fff"),
                aggregatePart);
        }
    }
}
