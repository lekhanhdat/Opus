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
using AvePoint.Media.StorageApi;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using System;
using System.Reflection;

namespace AvePoint.RA.ScheduleJobWorkerRole
{
    class Program
    {
        private static IRALogger logger = null;

        static void Main(string[] args)
        {
#if DEBUG
            RALogger.ConfigFile = "WorkerLog4net.dev.config";
#else
            RALogger.ConfigFile = "WorkerLog4net.config";
#endif
            try
            {
                logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
                RMServiceManagerUtil.Init();
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CrashHandler);
                RMGlobalConfiguration.Init();
                StorageApiConfiguration.Setup();
                InitLogger();
                var role = new WorkerRole();
                role.OnStart();
                role.Run();
            }
            catch (Exception e)
            {
                logger.Error("Unhandled exception {0}", e.ToString());
                RALogger.WaitForAllLogsFlush();
            }
        }

        static void InitLogger()
        {
            //LoggerInitializer.Initialize();

        }

        static void CrashHandler(object sender, UnhandledExceptionEventArgs args)
        {
            logger.Error("App crashed, {0}", args.ExceptionObject);
            RALogger.WaitForAllLogsFlush();
        }
    }
}
