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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.SignalR;
using System;
using System.Reflection;

namespace RATimerWorkerRole
{
    class Program
    {
        private static AvePoint.RA.Contract.Services.IRALogger logger = null;

        static void Main(string[] args)
        {
#if DEBUG
            RALogger.ConfigFile = "TimerLog4net.dev.config";
#else
            RALogger.ConfigFile = "TimerLog4net.config";
#endif
            logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CrashHandler);
            while (System.IO.File.Exists(@"c:\\role.sleep"))
            {
                System.Threading.Thread.Sleep(3000);
            }
            try
            {
                logger.Info("start timer in main.");
                RMGlobalConfiguration.Init();
                StorageApiConfiguration.Setup();
                RALogger.SetCustomizedLogPostfix("V: " + RMGlobalConfiguration.EnvSetting.ProductVersion);
                LoggerInitializer.InitializeLogger(LogType.ServiceLog);
                var role = new WorkerRole();
                role.OnStart();
                InitSignalR();
                logger.Info("init timer in main end.");
                role.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine($"init:{e.ToString()}");
                logger.Error("Unhandled exception {0}", e);
                RALogger.WaitForAllLogsFlush();
            }
        }

        static void CrashHandler(object sender, UnhandledExceptionEventArgs args)
        {
            logger.Error("App crashed, {0}", args.ExceptionObject);
            RALogger.WaitForAllLogsFlush();
        }

        private static void InitSignalR()
        {
            try
            {
                ISignalRService signalrService = (ISignalRService)PlatformWindsorManager.GetService("AvePoint.RA.Service.Services.SignalR.SignalRService", typeof(ISignalRService));

                //string singalrServer = RMGlobalConfiguration.AppConfig[RMAppSettingKey.SignalRServerURL];
                //logger.Info("SignalR server url : " + singalrServer);

                signalrService.SignalRSetup();

                logger.Info("Successfully set up singalr server connection");
            }
            catch (Exception e)
            {
                logger.Error("Fail to setup singalr server.", e);
            }

        }
    }
}
