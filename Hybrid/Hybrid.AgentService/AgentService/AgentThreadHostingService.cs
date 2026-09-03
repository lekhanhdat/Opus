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



namespace AvePoint.Hybrid.AgentService
{
    using AvePoint.GCommon;
    #region using directives
    using AvePoint.Hybrid.Contract.SignalR;
    using AvePoint.Hybrid.I18N.Resource;
    using AvePoint.Hybrid.Utility;
    using AvePoint.Hybrid.Utility.OperationSystem;
    using AvePoint.RA.Common.TransientFault;
    using AvePoint.RA.CommonUtil;
    using HybirdProxy.Implement;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    #endregion

    public class AgentThreadHostingService : IStartable
    {
        static AvePoint.GCommon.AveLogger logger = new AvePoint.GCommon.AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static Boolean quitAllThread = false;
        List<ThreadStart> hostThreads = new List<ThreadStart>() { new ThreadStart(RetentionLogAndTempFileByDays), new ThreadStart(UpdateAgentServiceThread) };
        List<Thread> runningThreads = new List<Thread>();

        private static AveRetryPolicy retryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(10, TimeSpan.FromSeconds(6)));

        public void Start()
        {
            this.runningThreads.Clear();
            this.hostThreads.ForEach(threadStart =>
            {
                try
                {
                    logger.Info(CommonResource.HostingThreadsStartHostingStartThread, threadStart.Method.Name);
                    var thread = new Thread(threadStart);
                    thread.IsBackground = true;
                    thread.Name = threadStart.Method.Name;
                    thread.Start();
                    runningThreads.Add(thread);
                    logger.Info(CommonResource.HostingThreadsStartHostingStartThreadSucceed, threadStart.Method.Name);
                }
                catch (Exception ex)
                {
                    logger.Error(CommonResource.HostingThreadsStartHostingStartThreadErrorOccurred, threadStart.Method.Name, ex.ToString());
                }
            });
        }

        public void Stop()
        {
            quitAllThread = true;
            this.runningThreads.ForEach(runningThread =>
            {
                try
                {
                    logger.Info(CommonResource.HostingThreadsStopHostingStopThread, runningThread.Name);
                    if (runningThread.Join(1000 * 5) == false)
                    {
                        try
                        {
                            logger.Warn(CommonResource.HostingThreadsStopHostingAbortThread, runningThread.Name);
                            runningThread.Abort();
                        }
                        catch (Exception ex)
                        {
                            logger.Error(CommonResource.HostingThreadsStopHostingAbortThreadErrorOccurred, runningThread.Name, ex.ToString());
                        }
                    }
                    logger.Info(CommonResource.HostingThreadsStopHostingStopThreadSucceed, runningThread.Name);
                }
                catch (Exception ex)
                {
                    logger.Error(CommonResource.HostingThreadsStopHostingStopThreadErrorOccurred, runningThread.Name, ex.ToString());
                }
            });
        }


        static void UpdateAgentServiceThread()
        {
            try
            {
                logger.Info("update Agent service thread start, start time:{0}", DateTime.Now.ToString());

                string agentId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerAgentId);
                string tenantId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerTenantId);
                string version = AveEnv.AgentDisplayVersion;
                ManagerProxy proxy = null;
                while (quitAllThread == false)
                {
                    if (!CommonConfiguration.InUpgradingProcess)
                    {
                        try
                        {
                            proxy = retryPolicy.ExecuteAction(() => RASignalRProxy.GetManagerProxy());

                            AgentManagementArgs args = new AgentManagementArgs();
                            args.Type = MessageType.KeepAlive;
                            args.AgentId = agentId;
                            args.TenantId = tenantId;
                            args.CPUHZ = OSInformation.CPUHz;
                            args.CPUUSage = OSInformation.CPUUsage;
                            args.AvailableMemeory = OSInformation.FreePhysicalMemory;
                            args.TotalMemory = OSInformation.TotalVisibleMemorySize;
                            args.OSName = OSInformation.OSName;
                            args.OSVersionNumber = OSInformation.OSVersionNumber;
                            args.HostName = OSInformation.HostName;
                            args.Version = version;
                            args.JobCounts = RecordJobsCount();
                            args.TimeStamp = System.DateTime.UtcNow.Ticks;
                            args.IsSupportUpgrade = true;

                            var result = System.Threading.Tasks.Task.Run(() => proxy.SendToManagerAsync<SAgentManagement>(new SAgentManagement() { MethodArgs = args }));
                            logger.Info("Update agent status to manager, agent id : " + agentId + ", timesatmp : " + args.TimeStamp);
                        }
                        catch (Exception e)
                        {
                            logger.Info("An error occurred while update agent service information, exception:{0}.", e.ToString());
                        }
                    }

                    int updateInterval = AveEnv.AgentServiceUpdataInterval;
                    DateTime cutoffTime = DateTime.Now.AddMinutes(updateInterval);
                    while (DateTime.Now < cutoffTime)
                    {
                        if (quitAllThread)
                        {
                            logger.Warn("Keep update thread need to be quit, agent id : " + agentId);
                            break;
                        }
                        else
                        {
                            Thread.Sleep(10 * 1000);
                        }
                    }
                }
                logger.Info("update Agent service thread stop, date time:{0}", DateTime.Now.ToString());
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred in log retention by size. Error: {0}.", ex.ToString());
            }
        }

        private static int RecordJobsCount()
        {
            int count = 0;
            Process[] processList = Process.GetProcesses();
            foreach (Process process in processList)
            {
                if (process.ProcessName.ToLower().Contains("recordsagentworker"))
                {
                    count++;
                }
            }

            return count;
        }

        public event EventHandler OnStarting;
        public event EventHandler OnStarted;
        public event EventHandler OnStopping;
        public event EventHandler OnStopped;


        static void RetentionLogAndTempFileByDays()
        {
            try
            {
                logger.Info("The log retention thread was started.");
                while (quitAllThread == false)
                {
                    LogRetentionByDays();
                    for (int i = 0; i < 30 * 60 * 24; i++)
                    {
                        if (quitAllThread) break;
                        Thread.Sleep(2000);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred in log retention.Error: {ex}");
            }
        }

        /// <summary>
        /// In March Release, Cloud Records only do retention log at first.
        /// </summary>
        static void LogRetentionByDays()
        {
            try
            {
                logger.Info($"Process Log folder {AveEnv.AgentLogFolder} With {AveEnv.AgentLogRetentionDays}");
                var workingFiles = Directory.GetFiles(AveEnv.AgentLogFolder, "*", SearchOption.AllDirectories);
                var filesToDelete = new List<FileInfo>();
                foreach (var logFile in workingFiles)
                {
                    var fileInfo = new FileInfo(logFile);

                    if (NeedRetention(fileInfo) == false)
                    {
                        continue;
                    }
                    filesToDelete.Add(fileInfo);
                }

                if (filesToDelete.Count > 0)
                {
                    DeleteLogs(filesToDelete);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred in log retention.Error: {ex}");
            }
        }



        static void DeleteLogs(List<FileInfo> logFiles)
        {
            logFiles.ForEach(DeleteFile);
        }

        static void DeleteFile(FileInfo file)
        {
            logger.Info($"Deleting the log file: {file.FullName}...");
            File.Delete(file.FullName);
        }

        static bool NeedRetention(FileInfo file)
        {
            if (file.FullName.EndsWith("CloudAgentService.log", StringComparison.OrdinalIgnoreCase)
                   || file.FullName.EndsWith("CloudAgentConfigurationTool.log", StringComparison.OrdinalIgnoreCase)
                   )
            {
                return false;
            }
            if (file.LastWriteTime.AddDays(AveEnv.AgentLogRetentionDays) < DateTime.Now)
            {
                return true;
            }
            return false;
        }
    }
    //will support in later release.Support retention log in March 2023
}
