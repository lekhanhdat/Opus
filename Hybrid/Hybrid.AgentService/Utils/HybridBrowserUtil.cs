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
using AvePoint.Hybrid.Contract.Object;
using AvePoint.Hybrid.Utility;
using AvePoint.RA.Common.Pipe;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Hybrid.Browser.Contract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.AgentService.Utils
{
    public class HybridBrowserUtil
    {
        private static readonly string HybridBrowserExePaht = Constants.RecordsBrowserExe;

        private static readonly string HybridBrowserProcessName = "RecordsAgentBrowser";

        private static readonly string DefaultPipeName = "hranpcs";

        private static volatile HybridBrowserUtil HybridBrowserUtilInstance;

        private static readonly object InstanceLocker = new object();

        private static readonly object ExeLocker = new object();

        public static HybridBrowserUtil Instance
        {
            get
            {
                if(HybridBrowserUtilInstance == null)
                {
                    lock(InstanceLocker)
                    {
                        if(HybridBrowserUtilInstance == null)
                        {
                            HybridBrowserUtilInstance = new HybridBrowserUtil();
                            HybridBrowserUtilInstance.EnsureHybridBrowserExeStart();
                        }
                    }
                }
                return HybridBrowserUtilInstance;
            }
        }

        public string Browse(HybridBrowserType browserType, string message)
        {
            return Browse(TenantLocalValue.LogonGroupId, browserType, message);
        }

        public string Browse(string logonGroupId, HybridBrowserType browserType, string message)
        {
            EnsureHybridBrowserExeStart();
            var contract = new HybridBrowserContract(logonGroupId, browserType, message);
            using(var client = new RANamedPipeClientStream(DefaultPipeName))
            {
                client.SendMessage(contract);
                return client.ReadMessage();
            }
        }

        private void EnsureHybridBrowserExeStart()
        {
            if (!ProcessExist())
            {
                lock(ExeLocker)
                {
                    if(!ProcessExist())
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo(HybridBrowserExePaht)
                        {
                            WindowStyle = ProcessWindowStyle.Normal,
                            UseShellExecute = false,
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
            var processes = Process.GetProcessesByName(HybridBrowserProcessName);
            return processes.Length > 0;
        }
    }
}
