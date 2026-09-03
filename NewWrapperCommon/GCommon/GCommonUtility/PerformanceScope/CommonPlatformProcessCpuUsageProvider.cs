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
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;

namespace AvePoint.GCommon.Utility.PerformanceScope
{
    public class CommonPlatformProcessCpuUsageProvider : ICpuUsageProvider
    {
        private int _processId;
        private System.Diagnostics.Process _process;
        private TimeSpan _lastCpuTime = TimeSpan.Zero;
        private DateTime _lastSampleTime = DateTime.MinValue;

        public void SetProcessId(int processId)
        {
            if (_processId == processId)
            {
                return;
            }

            _processId = processId;
            AttachProcess();
        }

        public long GetMemoryUsage()
        {
            _process.Refresh();
            long workingSetBytes = _process.WorkingSet64;
            return workingSetBytes;
        }

        public JobMetrics GetMetrics()
        {
            if (!EnsureProcess())
            {
                return CreateEmptyMetrics();
            }

            _process.Refresh();
            return new JobMetrics
            {
                ProcessMemoryUsageDeltaBytes = _process.WorkingSet64,
                ProcessCpuUsagePercent = GetCpuUsage(),
                MachineMemoryUsageDeltaBytes = GetMachineMemoryUsageBytes(),
                MachineCpuUsagePercent = GetMachineCpuUsagePercent(),
                ThreadCount = _process.Threads.Count,
                Timestamp = DateTime.UtcNow
            };
        }

        public double GetCpuUsage()
        {
            if (!EnsureProcess())
            {
                return 0d;
            }

            var currentCpu = _process.TotalProcessorTime;
            var currentTime = DateTime.UtcNow;
            if (_lastSampleTime == DateTime.MinValue)
            {
                _lastCpuTime = currentCpu;
                _lastSampleTime = currentTime;
                return 0d;
            }

            var cpuDeltaMs = (currentCpu - _lastCpuTime).TotalMilliseconds;
            var wallDeltaMs = (currentTime - _lastSampleTime).TotalMilliseconds;
            if (wallDeltaMs <= 0d)
            {
                return 0d;
            }

            _lastCpuTime = currentCpu;
            _lastSampleTime = currentTime;

            var usage = (cpuDeltaMs / (wallDeltaMs * Math.Max(Environment.ProcessorCount, 1))) * 100d;
            if (usage < 0d)
            {
                return 0d;
            }

            return usage > 100d ? 100d : usage;
        }

        public void Dispose()
        {
            _process?.Dispose();
            _process = null;
        }

        private bool EnsureProcess()
        {
            if (_process != null && !_process.HasExited && _process.Id == _processId)
            {
                return true;
            }

            AttachProcess();
            return _process != null;
        }

        private void AttachProcess()
        {
            _process?.Dispose();
            try
            {
                _process = System.Diagnostics.Process.GetProcessById(_processId);
                _lastCpuTime = _process.TotalProcessorTime;
                _lastSampleTime = DateTime.UtcNow;
            }
            catch
            {
                _process = null;
                _lastCpuTime = TimeSpan.Zero;
                _lastSampleTime = DateTime.MinValue;
            }
        }

        private static JobMetrics CreateEmptyMetrics()
        {
            return new JobMetrics
            {
                Timestamp = DateTime.UtcNow
            };
        }

        private static long GetMachineMemoryUsageBytes()
        {
            try
            {
                var computerInfo = new ComputerInfo();
                return (long)(computerInfo.TotalPhysicalMemory - computerInfo.AvailablePhysicalMemory);
            }
            catch
            {
                return 0;
            }
        }

        private static double GetMachineCpuUsagePercent()
        {
            try
            {
                using (var counter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true))
                {
                    counter.NextValue();
                    System.Threading.Thread.Sleep(150);
                    var value = counter.NextValue();
                    return NormalizePercentage(value);
                }
            }
            catch
            {
                return 0d;
            }
        }

        private static double NormalizePercentage(float rawValue)
        {
            if (rawValue < 0d)
            {
                return 0d;
            }

            return rawValue > 100d ? 100d : rawValue;
        }
    }
}
