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
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using SD = System.Diagnostics;
    using System.Threading;
    using System.Text;
    using AvePoint.Hybrid.Utility;
    using AvePoint.RA.CommonUtil;
    using AvePoint.Hybrid.Utility.OperationSystem;
    using AvePoint.Hybrid.Utility.Cryptography;
    #endregion

    public class AgentProcessHostingService : IStartable
    {
        AvePoint.GCommon.AveLogger logger = new AvePoint.GCommon.AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        string AgentLazyStartProcess = string.Empty;

        /// <summary>
        /// 有些AgentType无法区分是10还是07的进程，所以需要区分开。
        /// </summary>
        public bool checkAgentLazyStartProcessChange()
        {
            bool change = true;
            if (!string.IsNullOrEmpty(AgentLazyStartProcess))
            {
                if (AgentLazyStartProcess.Equals(AveEnv.AgentLazyStartProcess, StringComparison.OrdinalIgnoreCase))
                {
                    change = false;
                }
            }
            AgentLazyStartProcess = AveEnv.AgentLazyStartProcess;
            return change;
        }

        internal List<HostingProcessInfo> hostProcessesForNonSPEnv = new List<HostingProcessInfo>();


        List<HostingProcessInfo> HostProcessesForCommon
        {

            get
            {
#if DEBUG
                while (File.Exists("C:\\sleep.txt"))
                {
                    Thread.Sleep(10 * 1000);
                }
#endif
                if (checkAgentLazyStartProcessChange())
                {

                }
                return hostProcessesForCommon;
            }
        }

        List<HostingProcessInfo> hostProcessesForCommon = new List<HostingProcessInfo> { };

        #region --don't modify this section--

        public void Start()
        {
            StartLazyStartProcess(false);
            //start monitor threads
            Thread monitorThread = new Thread(MonitorThread);
            monitorThread.Name = "Process Hosting Monitor Thread";
            monitorThread.IsBackground = true;
            monitorThread.Start();
        }

        void StartLazyStartProcess(bool judgeMonitor)
        {
            var domain = String.Empty;
            var username = String.Empty;
            var password = String.Empty;
            var availableHostProcessInfos = this.GetAvailableHostProcessInfos().FindAll(processInfo => this.CheckAgentType(processInfo.agentTypes) && !processInfo.LazyStart && (!judgeMonitor || processInfo.NeedMonitoring));
            try
            {
                if (CheckStart(availableHostProcessInfos))
                {
                    var credentials = AgentCredentialManager.GetAgentCredential();
                    domain = credentials.ItemA;
                    username = credentials.ItemB;
                    password = credentials.ItemC;
                }
                else
                {
                    logger.Info("All available lazyStart progresses has started.");
                    return;
                }
            }
            catch (Exception e)
            {
                this.logger.Error("get agent credential exceptions. Details:{0}", e.ToString());
            }
            if (String.IsNullOrEmpty(username))
            {
                this.logger.Error("Can not get credential to start processes.");
            }
            else
            {
                //.FindAll(processInfo => this.CheckAgentType(processInfo.agentTypes) && !processInfo.LazyStart)
                availableHostProcessInfos.ForEach(processInfo => TryToStartProcess(domain, username, password, processInfo));
            }
        }

        private bool CheckStart(List<HostingProcessInfo> availableHostProcessInfos)
        {
            foreach (var host in availableHostProcessInfos)
            {
                if (CheckNeedStartProcess(host))
                {
                    return true;
                }
            }
            return false;
        }

        public void Stop()
        {
            var availableHostProcessInfos = this.GetAvailableHostProcessInfos();
            availableHostProcessInfos.ForEach(processInfo =>
            {
                try
                {
                    var processes = SD.Process.GetProcessesByName(processInfo.exeName.Replace(".exe", ""));//stop service,get processes不能带exe；
                    if (processes.Length > 0)
                    {
                        processInfo.PeacefulStop();
                        Array.ForEach(processes, process => process.Kill());
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Hosting process error", processInfo.exeName, ex.ToString());
                }
            });
        }

        void MonitorThread()
        {
            logger.Info("Process hosting monitor thread started.");
            while (true)
            {
                try
                {
                    Thread.Sleep(5 * 60 * 1000);
                    StartLazyStartProcess(true);

                }
                catch (Exception e)
                {
                    logger.Error("An error occurred in monitor thread: {0}", e.ToString());
                }
            }
        }

        Boolean CheckAgentType(List<String> agentTypes)
        {
            var isValid = true;

            return isValid;
        }

        List<HostingProcessInfo> GetAvailableHostProcessInfos()
        {
            var processes = new List<HostingProcessInfo>();
            processes.AddRange(HostProcessesForCommon);

            return processes;
        }

        bool CheckNeedStartProcess(HostingProcessInfo processInfo)
        {
            bool needStartProcess = false;
            var processName = processInfo.exeName;
            if (processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                processName = processName.Substring(0, processName.Length - 4);
            var ps = SD.Process.GetProcessesByName(processName);
            if (ps.Length < 1)
            {
                string tempFile = string.Format("{0}/{1}.exe", processInfo.exePath, processName);
                if (File.Exists(tempFile))
                {
                    needStartProcess = true;
                }
            }
            return needStartProcess;
        }

        void TryToStartProcess(String domain, String username, String password, HostingProcessInfo processInfo)
        {
            try
            {
                var processName = processInfo.exeName;
                if (processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    processName = processName.Substring(0, processName.Length - 4);
                var ps = SD.Process.GetProcessesByName(processName);
                if (ps.Length < 1)
                {
                    string tempFile = string.Format("{0}/{1}.exe", processInfo.exePath, processName);
                    if (File.Exists(tempFile))
                    {
                        this.logger.Info(string.Format("Hosting process {0} {1}", processInfo.exeName, domain));
                        try
                        {
                            var sp = new StartProcess(domain, username, CryptoUtil.ConvertBytesToString(CspCommunicationWrapper.UnWrapKey(password)), processInfo.workingDir);
                            sp.Start(Path.Combine(processInfo.exePath, processInfo.exeName), processInfo.args);
                        }
                        catch (Exception ex)
                        {
                            if (ex is System.ComponentModel.Win32Exception || ex.InnerException is System.ComponentModel.Win32Exception)
                            {
                                var errorCode = ex is System.ComponentModel.Win32Exception ?
                                    (ex as System.ComponentModel.Win32Exception).NativeErrorCode :
                                    (ex.InnerException as System.ComponentModel.Win32Exception).NativeErrorCode;
                                if (errorCode == 1326)
                                {
                                    AgentCredentialManager.ClearAgentCredentialCache();
                                }
                            }
                            throw ex;
                        }
                    }
                    else
                    {
                        logger.Warn("The file doesn't exist, so skip to start it.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Hosting process error while starting the process : ", processInfo.exeName, ex.ToString());
            }
        }

        #endregion

        public event EventHandler OnStarting;
        public event EventHandler OnStarted;
        public event EventHandler OnStopping;
        public event EventHandler OnStopped;

    }
}
