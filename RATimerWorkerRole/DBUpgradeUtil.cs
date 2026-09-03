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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RATimerWorkerRole
{
    public class DBUpgradeUtil
    {
        private static readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string ExePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "RADBInitializationUpgrade.exe"); 
        private static readonly string ProcessName = "RADBInitializationUpgrade";
        private static readonly string DllPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "RADBInitializationUpgrade.dll");
        public void Execute()
        {
            try
            {
                if (!ProcessExist())
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        FileName = !RMGlobalConfiguration.EnvSetting.IsDevEnvironment ? "dotnet" : ExePath,
                        Arguments = !RMGlobalConfiguration.EnvSetting.IsDevEnvironment ? DllPath : null,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        Verb = "runas",
                        WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
                    };
                    Logger.Info($"begin to run db upgrade, {startInfo.FileName}, {DllPath}");
                    var process = Process.Start(startInfo);
                    
                    process.WaitForExit();

                }

            }
            catch (Exception ex)
            {
                Logger.Error($"error to run db upgrade, {ex.ToString()}");
            }
        }

        private bool ProcessExist()
        {
            if (!RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
            {
                var processes = Process.GetProcessesByName("dotnet");
                Logger.Debug($"[dotnent] count: {processes.Length}");
                return processes.Length > 1;
            }
            else
            {
                var processes = Process.GetProcessesByName(ProcessName);
                return processes.Length > 0;
            }
        }
    }
}
