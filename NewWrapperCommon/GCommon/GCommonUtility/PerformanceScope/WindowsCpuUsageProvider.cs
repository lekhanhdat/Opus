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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Utility.PerformanceScope
{
    public class WindowsCpuUsageProvider : ICpuUsageProvider
    {
        private PerformanceCounter _counter;
        private int _processId;
        private PerformanceCounter _counterMemoryUsage;

        public void SetProcessId(int processId)
        {
            if (_processId == processId && _counter != null)
            {
                return;
            }

            _counter?.Dispose();
            _processId = processId;
            var instance = ResolveProcessInstanceName(processId);
            if (string.IsNullOrEmpty(instance))
            {
                _counter = null;
                return;
            }

            _counter = new PerformanceCounter("Process", "% Processor Time", instance, true);
            _counterMemoryUsage = new PerformanceCounter("Process", "Private Bytes", instance);
            _counter.NextValue();
            _counterMemoryUsage.NextValue();
        }
        public long GetMemoryUsage()
        {
            if (_counterMemoryUsage == null)
            {
                return 0;
            }

            try
            {
                var memoryUsage = _counterMemoryUsage.NextValue();
                if (memoryUsage < 0)
                {
                    return 0;
                }
                return (long)memoryUsage;
            }
            catch
            {
                return 0;
            }
        }

        public JobMetrics GetMetrics()
        {
            return default(JobMetrics);
        }

        public double GetCpuUsage()
        {
            if (_counter == null)
            {
                return 0d;
            }

            try
            {
                var raw = _counter.NextValue();
                var memoryUsage = _counterMemoryUsage.NextValue();
                if (raw < 0d)
                {
                    return 0d;
                }

                var normalized = raw / Math.Max(Environment.ProcessorCount, 1);
                return Math.Min(normalized, 100d);
            }
            catch
            {
                return 0d;
            }
        }

        public void Dispose()
        {
            _counter?.Dispose();
            _counter = null;
        }

        private static string ResolveProcessInstanceName(int processId)
        {
            var category = new PerformanceCounterCategory("Process");
            foreach (var instance in category.GetInstanceNames())
            {
                try
                {
                    var idCounter = new PerformanceCounter("Process", "ID Process", instance, true);
                    if ((int)idCounter.RawValue == processId)
                    {
                        return instance;
                    }
                }
                catch (InvalidOperationException)
                {
                    continue;
                }
            }

            return null;
        }
    }
}
