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
using System.Collections;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;
using AutoInstallation.Contract.WindowsService;
using LOGRESX = AutoInstallation.Records.App.Resources.LogResource;
using GUIRESX = AutoInstallation.Records.App.Resources.Resource;

namespace AutoInstallationCommon.Utility.Handler
{
    public class WindowsServiceHandler
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public static void StartService(string name)
        {
            try
            {
                var sc = new ServiceController(name);
                if (sc.Status.Equals(ServiceControllerStatus.Stopped)) sc.Start();
            }
            catch (Exception ex)
            {
                logger.Warn(LOGRESX.COMMONUTILITYLOG_STARTPLATFORMSERVICEERROR, name, ex.ToString());
            }
        }

        public static void StopService(string name)
        {
            if (isServiceIsExisted(name))
                try
                {
                    var sc = new ServiceController(name);
                    if (!sc.Status.Equals(ServiceControllerStatus.Stopped))
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    }

                    sc.Dispose();
                }
                catch (Exception ex)
                {
                    logger.Warn(LOGRESX.COMMONUTILITYLOG_STOPPLATFORMSERVICEERROR, name, ex.ToString());
                }
            else
                logger.Info(LOGRESX.COMMONUTILITYLOG_SERVICESTOPPED, name);
        }

        public static bool InstallService(ServiceInstallContext context)
        {
            if (!isServiceIsExisted(context.ServiceName))
            {
                InstallmyService(null, context);
                return true;
            }

            logger.Warn(LOGRESX.COMMONUTILITYLOG_SERVICEEXIST, context.ServiceName);
            return false;
        }

        /// <summary>
        ///     卸载服务
        /// </summary>
        public static void DeleteService(ServiceInstallContext context)
        {
            if (isServiceIsExisted(context.ServiceName))
                UnInstallmyService(context.ServiceFilePath);
            else
                logger.Warn(LOGRESX.COMMONUTILITYLOG_DONOTNEEDDELETESERVCIE, context.ServiceName);
        }

        /// <summary>
        ///     检查服务存在的存在性
        /// </summary>
        public static bool isServiceIsExisted(string NameService)
        {
            var services = ServiceController.GetServices();
            foreach (var s in services)
                if (s.ServiceName.ToLower() == NameService.ToLower())
                    return true;
            // s.Dispose();
            return false;
        }

        /// <summary>
        ///     安装Windows服务
        /// </summary>
        private static void InstallmyService(IDictionary stateSaver, ServiceInstallContext context)
        {
            try
            {
                var assInstaller = new AssemblyInstaller();
                assInstaller.UseNewContext = true;
                assInstaller.Path = context.ServiceFilePath;
                assInstaller.CommandLine = new[] {"username=" + context.UserName, "password=" + context.Password};
                assInstaller.Install(stateSaver);
                assInstaller.Commit(stateSaver);
                assInstaller.Dispose();
            }
            catch (Exception ex)
            {
                logger.Error(LOGRESX.COMMONUTILITYLOG_INSTALLSERVICEFAILED, context.ServiceName, ex.ToString());
                //throw new Exception(string.Format(GUIRESX.COMMONUTILITY_INSTALLSERVCIEERROR, context.ServiceName));
            }
        }

        /// <summary>
        ///     卸载Windows服务
        /// </summary>
        /// <param name="filepath">程序文件路径</param>
        private static void UnInstallmyService(string filepath)
        {
            try
            {
                //AssemblyInstaller assInstaller = new AssemblyInstaller();
                //assInstaller.UseNewContext = true;
                //assInstaller.Path = filepath;
                //assInstaller.Uninstall(null);
                //assInstaller.Dispose();
                using (var installer = new AssemblyInstaller())
                {
                    installer.UseNewContext = true;
                    installer.Path = filepath;
                    installer.Uninstall(null);
                    installer.Commit(null);
                }
            }
            catch (Exception ex)
            {
                logger.Error(LOGRESX.COMMONUTILITYLOG_UNINSTALLSERVICEFAILED, filepath, ex.ToString());
            }
        }
    }
}