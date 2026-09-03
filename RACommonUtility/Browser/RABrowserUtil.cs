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
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Pipe;
using AvePoint.RA.Common.SharePointBrowser;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Browser
{
    public class RABrowserUtil
    {

        private static readonly string RABrowserExePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "RecordsBrowser.exe");
        private static readonly string RABrowserDllPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "RecordsBrowser.dll");

        private static readonly string RABrowserProcessName = "RecordsBrowser";

        private static readonly string PipeName = "ranpcs";

        private static volatile RABrowserUtil RABrowserUtilInstance;

        private static readonly object InstanceLocker = new object();

        private static readonly object ExeLocker = new object();

        private static readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public static RABrowserUtil Instance
        {
            get
            {
                if (RABrowserUtilInstance == null)
                {
                    lock (InstanceLocker)
                    {
                        if (RABrowserUtilInstance == null)
                        {
                            RABrowserUtilInstance = new RABrowserUtil();
                            RABrowserUtilInstance.EnsureRABrowserExeStart();
                        }
                    }
                }
                return RABrowserUtilInstance;
            }
        }

        public T Browse<T>(AveTreeMessage currentNode, BrowserType type) where T: AveTreeMessage
        {
            EnsureRABrowserExeStart();
            var contract = new RABrowserContract(JsonConvert.SerializeObject(currentNode), type, TenantLocalValue.LogonUserEmail, TenantLocalValue.LogonGroupId, TenantLocalValue.LogonUserId);
            return Browse<T>(contract);
        }

        public T Browse<T>(RABrowserContract contract) where T : AveTreeMessage
        {
            EnsureRABrowserExeStart();
            using (var client = new RANamedPipeClientStream(PipeName))
            {
                client.SendMessage(contract);
                return client.ReadMessage<T>();
            }
        }
        public T SendBrowseMessage<T>(RABrowserContract contract) where T : BrowserMessage
        {
            EnsureRABrowserExeStart();
            using (var client = new RANamedPipeClientStream(PipeName))
            {
                client.SendMessage(contract);
                return client.ReadMessage<T>();
            }
        }
        private void EnsureRABrowserExeStart()
        {
            if (!ProcessExist())
            {
                lock (ExeLocker)
                {
                    if (!ProcessExist())
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo()
                        {
                            FileName = !RMGlobalConfiguration.EnvSetting.IsDevEnvironment ? "dotnet" : SecurityUtils.SanitizeCommandArgs(RABrowserExePath),
                            Arguments = !RMGlobalConfiguration.EnvSetting.IsDevEnvironment ? SecurityUtils.SanitizeCommandArgs(RABrowserDllPath) : null,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardError = true,
                            Verb = "runas"
                        };
                        Process.Start(startInfo);
                        while (!ProcessExist())
                        {
                            Task.Delay(1000).Wait();
                        }
                    }
                }
            }
        }

        private bool ProcessExist()
        {
            if(!RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
            {
                var processes = Process.GetProcessesByName("dotnet");
                Logger.Debug($"[dotnent] count: {processes.Length}");
                return processes.Length > 1;
            }
            else
            {
                var processes = Process.GetProcessesByName(RABrowserProcessName);
                return processes.Length > 0;
            }
        }
    }
}
