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
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;

namespace AutoInstallationCommon.Utility.Handler
{
    public class InstallUtility
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public static void StopService(string serviceName)
        {
            using (var service = GetService(serviceName))
            {
                if (service == null) return;

                if (service.Status.Equals(ServiceControllerStatus.Stopped)) return;

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped);
            }
        }

        private static ServiceController GetService(string serviceName)
        {
            return ServiceController.GetServices()
                .FirstOrDefault(item => item.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
        }

        public static void DeleteService(string path)
        {
            using (var installer = new AssemblyInstaller())
            {
                installer.UseNewContext = true;
                installer.Path = path;
                installer.Uninstall(null);
                installer.Commit(null);
            }
        }

        public static void CMDRestartIIS()
        {
            try
            {
                var cmd = new Process();
                cmd.StartInfo.FileName = "cmd.exe";
                cmd.StartInfo.UseShellExecute = false;
                cmd.StartInfo.RedirectStandardInput = true;
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.CreateNoWindow = true;
                cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                cmd.Start();
                cmd.StandardInput.WriteLine("iisreset");
                cmd.StandardInput.WriteLine("exit");
                cmd.WaitForExit();
                var readProOutResult = cmd.StandardOutput.ReadToEnd();
                logger.Info("StartIISReset:", readProOutResult);
            }
            catch (Exception ex)
            {
                logger.Warn("RestartIIS failed.error:{0}", ex);
            }
        }
    }
}