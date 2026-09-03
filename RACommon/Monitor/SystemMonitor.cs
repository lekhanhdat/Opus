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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace AvePoint.RA.Common.Monitor
{
    public class SystemMonitor : IDisposable
    {
        private IList<IPerformanceMetric> metrics = new List<IPerformanceMetric>() { 
            new CPUFastMonitorMetric(), new ProcessMetric(), new MemoryMetric(), new DriveMetric() };
        private RALogger mLogger = RALogger.GetInstance(typeof(PerformanceMonitor));
        private ulong mMemory;
        private int mCpuUsage;
        private bool isRunning = true;
        private Thread thread;
        public SystemMonitor()
        {
          
            thread = new Thread(CheckScaleIn);
            thread.IsBackground = true;
            thread.Start();
        }

        private void Check()
        {
            mMemory = OSInformation.GetLeftMemory();
            mCpuUsage = OSInformation.CPUUsage;
            mLogger.Info("current left memory:{0}, current cpu usage:{1}", mMemory, mCpuUsage);
            //ProcessMetric.DumpProcesses(true);
        }
        private void CheckScaleIn()
        {
            while (isRunning)
            {
                try
                {
                    Check();
                }
                catch (Exception ex)
                {
                    mLogger.Warn("CheckScaleIn process failed:{0}", ex);
                }

                Thread.Sleep(5 * 60 * 1000);
            }
            mLogger.Info("CheckScaleIn process info finished");
        }
        public void CheckFreeResources()
        {
            while (!IsHealthy())
            {
                mMemory = OSInformation.GetLeftMemory();
                mCpuUsage = OSInformation.CPUUsage;
                mLogger.Warn("current system is busy now, left memory:{0},cpu usage{1},", mMemory, mCpuUsage);
                Thread.Sleep(60 * 1000);
            }
        }

        private bool IsHealthy()
        {
            foreach (IPerformanceMetric performanceMethric in metrics)
            {
                if (!performanceMethric.IsHealthy)
                {
                    return false;
                }
            }
            return true;
        }

        public void Dispose()
        {
            mLogger.Info("start to dispose the system monitor instance.");
            isRunning = false;
            if (!thread.Join(5 * 60 * 1000))
            {
                thread.Abort();
            }
            mLogger.Info("end to dispose the system monitor instance.");
        }
    }

    interface IPerformanceMetric
    {
        bool IsHealthy { get; }
    }

    class CPUFastMonitorMetric : IPerformanceMetric
    {
        private static RALogger mlogger = RALogger.GetInstance(typeof(CPUFastMonitorMetric));
        private const int CPUThreshold = 90;

        public bool IsHealthy
        {
            get
            {
                var currentCPU = OSInformation.CPUUsage;

                if (currentCPU > CPUThreshold)
                {
                    var times = 3;
                    while (times > 0)
                    {
                        Thread.Sleep(5000);
                        currentCPU += OSInformation.CPUUsage;
                        times--;
                    }

                    var cpuUsage = currentCPU / 4;
                    mlogger.Info("The current CPU Usage:{0}", cpuUsage);
                    return cpuUsage < CPUThreshold;
                }

                return true;
            }
        }
    }

    class ProcessMetric : IPerformanceMetric
    {
        private static HashSet<string> ModuleNames = new HashSet<string>()
        { "RevIMScheduleJob"};
        private static RALogger mlogger = RALogger.GetInstance(typeof(ProcessMetric));
        private const int ProcessThreshold = 10;

        public bool IsHealthy
        {
            get
            {
                return CountJobProcess() <= ProcessThreshold;
            }
        }

        private int CountJobProcess()
        {
            int processNum = 0;
            try
            {
                foreach (Process process in Process.GetProcesses())
                {
                    if (ModuleNames.Contains(process.ProcessName))
                    {
                        processNum++;
                    }
                }
            }
            catch (Exception e)
            {
                mlogger.Error("failed to get process count due to: {0}", e.ToString());
            }
            return processNum;
        }

    }

    class MemoryMetric : IPerformanceMetric
    {
        private const ulong LeftMemoryThreshold = 200 * 1024 * 1024;
        private RALogger mlogger = RALogger.GetInstance(typeof(MemoryMetric));

        public bool IsHealthy
        {
            get
            {
                ulong leftMemory = OSInformation.GetLeftMemory();
                mlogger.Info("left memory: {0}", leftMemory);
                return leftMemory > LeftMemoryThreshold;
            }
        }
    }

    class DriveMetric : IPerformanceMetric
    {
        private static RALogger mlogger = RALogger.GetInstance(typeof(DriveMetric));
        public bool IsHealthy
        {
            get
            {
                return Usage > 95;
            }
        }

        public static int Usage
        {
            get
            {
                DriveInfo[] drives = DriveInfo.GetDrives();
                var dataDrive = Path.GetPathRoot(AppDomain.CurrentDomain.BaseDirectory);
                DriveInfo cInfo = drives.FirstOrDefault((info) => string.Equals(info.Name, dataDrive, StringComparison.InvariantCultureIgnoreCase));
                if (cInfo != null)
                {
                    mlogger.Info($"Disk free space:{cInfo.TotalFreeSpace}, total size: {cInfo.TotalSize}");
                    var freeDiskPer = Math.Round((double)cInfo.TotalFreeSpace / cInfo.TotalSize, 2);
                    return Convert.ToInt32((1 - freeDiskPer) * 100);
                }
                return 0;
            }
        }
    }
}
