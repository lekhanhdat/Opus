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
using System.Timers;
using System.Collections.Generic;
using System.Diagnostics;
using SysProcess = System.Diagnostics.Process;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.JobManagement.Modules.Dashboard.Obj;
using AvePoint.GCommon.JobManagement.Modules.Dashboard.Impl;
using AvePoint.GCommon.Contract.Server.GUI;

namespace AvePoint.Common
{
    public class AveResourceUsageMonitor
    {
        private static bool isCurrentProcess = true;
        private static double mInterval = 1000;
        private static long totleMemory = 0;
        private static int totleCount = 0;
        private static object mLock = new object();
        //private static Timer mTimer;
        private static PerformanceCounter mMemoryCounter;
        private static SysProcess mMonitorProcess;
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveResourceUsageMonitor));
        private static DateTime mStartTime;
        private static DateTime mFinishTime;
        private static int mInstanceId = -1;
        private static GUIModuleType mModuleType;


        public static string TenantId { get; set; }

        public static string JobId { get; set; }

        /// <summary>
        ///获取监控进程的cpu使用时间
        /// </summary>
        public static TimeSpan ProcessorTime
        {
            get
            {
                try
                {
                    return mMonitorProcess.TotalProcessorTime;
                }
                catch (Exception ex)
                {
                    logger.Warn("get process cpu process time failed.{0}", ex.ToString());
                    return new TimeSpan(0);
                }
            }
        }

        /// <summary>
        /// 获取进程使用的平均内存
        /// </summary>
        public static long AverageMemoryUsage
        {
            get
            {
                try
                {
                    return mMonitorProcess.WorkingSet64;
                }
                catch (Exception ex)
                {
                    logger.Warn("get process memory failed.{0}", ex.ToString());
                    return 0;
                }
            }
        }

        /// <summary>
        /// 获取当前机器总内存
        /// </summary>
        public static long TotalPhysicalMemory
        {
            get
            {
                return OSInformation.TotalVisibleMemorySize * 1024;
            }
        }

        /// <summary>
        /// 设置和获取每次进行统计的时间间隔，单位为毫秒，默认为1000
        /// </summary>
        public static double Interval
        {
            get { return mInterval; }
            set { mInterval = value; }
        }

        /// <summary>
        /// job开始时间
        /// </summary>
        public static DateTime StartTime
        {
            get { return mStartTime; }
        }

        /// <summary>
        /// job结束时间
        /// </summary>
        public static DateTime FinishTime
        {
            get { return mFinishTime; }
        }


        public static int InstanceId
        {
            get { return mInstanceId; }
            set { mInstanceId = value; }
        }

        /// <summary>
        /// 开始监控，若监控当进程结束后将监控数据进行保存
        /// </summary>
        public static bool StartMonitor(GUIModuleType moduleType)
        {
            mModuleType = moduleType;
            mStartTime = mFinishTime = DateTime.Now;
            if (mInstanceId == -1)
            {
                mMonitorProcess = SysProcess.GetCurrentProcess();
                isCurrentProcess = true;
            }
            else
            {
                try
                {
                    mMonitorProcess = SysProcess.GetProcessById(mInstanceId);
                }
                catch (Exception)
                {
                    throw;
                }
                isCurrentProcess = false;
            }
            try
            {
                mMemoryCounter = new PerformanceCounter("Process", "Working Set - Private", mMonitorProcess.ProcessName);
            }
            catch (Exception ex)
            {
                logger.Warn("create memory monitor failed.{0}", ex.ToString());
                return false;
            }
            //mTimer = new Timer(mInterval);
            //mTimer.Elapsed += ExecuteMonitor;
            //mTimer.AutoReset = true;
            //mTimer.Start();
            return true;
        }


        /// <summary>
        /// 停止监控，监控进程不为当前进程，监控进程结束后自动执行
        /// </summary>
        public static void StopMonitor()
        {
            try
            {
                //mTimer.Stop();
                mFinishTime = DateTime.Now;
                JobId = string.IsNullOrEmpty(JobId) ? string.Empty : JobId;
                TenantId = string.IsNullOrEmpty(TenantId) ? string.Empty : TenantId;
                SaveResult();
            }
            catch (Exception ex)
            {
                logger.Error("save resource usage info failed.{0}", ex.ToString());
            }
        }

        /// <summary>
        /// 监控job使用内存
        /// </summary>
        private static void ExecuteMonitor(object source, ElapsedEventArgs e)
        {
            try
            {
                lock (mLock)
                {
                    if (mMemoryCounter != null && (isCurrentProcess ||
                        PerformanceCounterCategory.InstanceExists(mMonitorProcess.ProcessName, "Process")))
                    {
                        totleMemory += (long)mMemoryCounter.NextValue();
                        totleCount++;
                    }
                    else
                    {
                        logger.Info("the process finished.");
                        //mTimer.Stop();
                    }
                }
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                logger.Warn("get process info failed. NativeErrorCode:{0}  error:{1}", ex.NativeErrorCode, ex.ToString());
            }
            catch (Exception ex)
            {
                logger.Warn("get process info failed.{0}", ex.ToString());
                //mTimer.Stop();
            }
        }

        private static void SaveResult()
        {
            string mainJobId = string.Empty;
            string subJobId = string.Empty;
            mainJobId = JobId;
            if (JobId.Contains("_"))
            {
                mainJobId = JobId.Substring(0, JobId.IndexOf('_'));
                subJobId = JobId;
            }
            AzureResourceDto resourceDto = new AzureResourceDto()
            {
                JobId = mainJobId,
                SubJobId = subJobId,
                TenantId = TenantId,
                CPU = ProcessorTime,
                Module = mModuleType,
                StartTime = StartTime,
                FinishTime = FinishTime,
                AgentHost = AveEnv.AgentName,
                Memery = AverageMemoryUsage,
                TotalMemery = TotalPhysicalMemory
            };
            AzureResourceStatisticService resourceService = new AzureResourceStatisticService();
            resourceService.SaveJobResourceStatistic(resourceDto);
        }
    }
}
