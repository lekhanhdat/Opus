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
using AvePoint.Hybrid.AgentService.Initiator;
using AvePoint.Hybrid.AgentService.Utils;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.Hybrid.Utility;
using AvePoint.Hybrid.Utility.Configuration;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using log4net.Config;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

namespace AvePoint.Hybrid.AgentService
{
    public class AgentService : ServiceBase
    {
        private static IIocContainerManager containerManager;
        private static AvePoint.GCommon.AveLogger logger = AvePoint.GCommon.AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        protected override void OnStart(string[] args)
        {
            ThreadPool.QueueUserWorkItem(state => Start());
        }

        public void Start()
        {
            try
            {
                logger.Info("Begin to init agent service.");
                ServiceInitializationUtil.InitServicePoint();   
                InitLogger();
                CommonConfiguration.InitAppSetting();
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CrashHandler);

                logger.Info("Begin to init container and castle  service.");
                containerManager = new AgentIocContainerManager();
                containerManager.LoadContainer();
                logger.Info("Finish to init container and castle  service.");

                InitiatorManager.StartInitiators();
            }
            catch (Exception e)
            {
                logger.Error("start up agent service fail, ",e);
            }
        }

        private static void InitLogger()
        {

            if (System.Configuration.ConfigurationManager.GetSection("log4net") != null)
            {
                XmlConfigurator.Configure();
            }
            else
            {
                string logCfgPath = AppDomain.CurrentDomain.BaseDirectory + "Config\\AgentLog4net.config";
                XmlConfigurator.ConfigureAndWatch(new FileInfo(logCfgPath));
            }

            logger = AvePoint.GCommon.AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
            logger.Info("Records Agent - Logger initialized.");
        }

        private static void CrashHandler(object sender, UnhandledExceptionEventArgs args)
        {
            logger.Error("Agent Service crashed, {0}", args.ExceptionObject);
        }

        protected override void OnStop()
        {
            try
            {
                containerManager.UnloadContainer();
                ProcessHelper.ExecuteAgentStopLogic();
            }
            catch(Exception e)
            {
                logger.Error("Stop service error : " ,e);
            }
            logger.Info("Agent Service stopped.");
        }
    }
}