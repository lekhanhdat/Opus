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
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace AvePoint.RA.Common.CloudService
{
    public class PerformanceMonitor
    {

        private IList<IPerformanceMetric> metrics = new List<IPerformanceMetric>() { new ProcessMetric(), new MemoryMetric(), new DriveMetric() };
        private RALogger mlogger = RALogger.GetInstance(typeof(PerformanceMonitor));
        private ulong mMemory;
        private int mCpuUsage;

        public PerformanceMonitor()
        {
            System.Timers.Timer timer = new System.Timers.Timer(3 * 60 * 1000);
            timer.Elapsed += Check;
            timer.AutoReset = true;
            timer.Start();
        }

        private void Check(object sender, ElapsedEventArgs e)
        {
            mMemory = OSInformation.GetLeftMemory();
            mCpuUsage = OSInformation.CPUUsage;
            mlogger.Info("current left memory:{0}, current cpu usage:{1}", mMemory, mCpuUsage);
        }

        public void CheckFreeResources()
        {
            while (!IsHealthy())
            {
                mMemory = OSInformation.GetLeftMemory();
                mCpuUsage = OSInformation.CPUUsage;
                mlogger.Warn("current system is busy now, left memory:{0},cpu usage{1},", mMemory, mCpuUsage);
                Thread.Sleep(5 * 60 * 1000);
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
    }

    interface IPerformanceMetric
    {
        bool IsHealthy { get; }
    }

    class ProcessMetric : IPerformanceMetric
    {
        private static HashSet<string> ModuleNames = new HashSet<string>()
        { "RevIMScheduleJob"};
        private RALogger mlogger = RALogger.GetInstance(typeof(ProcessMetric));
        private const int ProcessThreshold = 16;

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
        private RALogger mlogger = RALogger.GetInstance(typeof(DriveMetric));
        public bool IsHealthy
        {
            get
            {
                DriveInfo[] drives = DriveInfo.GetDrives();
                DriveInfo cInfo = drives.FirstOrDefault((info) => string.Equals(info.Name, "C:\\"));
                if (cInfo != null)
                {
                    mlogger.Info("c drive free space:{0}", cInfo.TotalFreeSpace);
                    return (double)cInfo.TotalFreeSpace / (double)cInfo.TotalSize > 0.05;  //剩余磁盘空间大于5%
                }
                return false;
            }
        }
    }
}
