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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Security;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using System;
using System.Reflection;
using System.Text;
using System.Threading;
using AvePoint.RA.Timer.Task;
using RADBInitializationUpgrade;
using System.Data.SqlClient;
using System.IO;
using Util.Upgrade;
using Aos.Sdk.Models;
using System.Net;
using AvePoint.RA.Contract.Configurations;

namespace AvePoint.RA.RADBInitializationUpgrade
{
    public class Program
    {
        private static RALogger Logger = null;

        public static void Main(string[] args)
        {
            try
            {
#if DEBUG
                while (File.Exists("c:/RADBInitializationUpgrade.sleep"))
                {
                    Thread.Sleep(1000);
                }
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development", EnvironmentVariableTarget.Process);
#endif

#if DEBUG
                RALogger.ConfigFile = "TimerLog4net.dev.config";
#else
                RALogger.ConfigFile = "TimerLog4net.config";
#endif
                Logger = RALogger.GetInstance(typeof(Program));
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CrashHandler);
                Logger.Info("Starting RADBInitialization process");
                InitEnv();
                RunUpgrade();
            }
            catch (Exception ex)
            {
                if (Logger != null)
                {
                    Logger?.Error($"Start RADBInitialization process failed. {ex}");
                }
                else
                {
                    throw;
                }
            }
            finally 
            {
                RALogger.WaitForAllLogsFlush();
            }
        }

        private static void RunUpgrade()
        {
            var upgradeConfiguration = new UpgradeConfiguration
            {
                IsUpgradeMode = true,
                Version = RMGlobalConfiguration.EnvSetting.ProductVersion,
                DataCenter = RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_DATA_CENTER],
                MonitorUrl = RMGlobalConfiguration.AppConfig[RMAppSettingKey.UPGRADE_MONITOR_URL],
                Product = "AvePoint Opus",
                IsHotfix = true,
            };

            Logger.Info("Start check version.");

            if(!string.IsNullOrWhiteSpace(RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME]))
            {
                if (RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME].Equals("test", StringComparison.OrdinalIgnoreCase))
                {
                    upgradeConfiguration.Version = $"TEST.{DateTime.UtcNow.Ticks}";
                }

                if (RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME].Equals("Gov Virginia", StringComparison.OrdinalIgnoreCase))
                {
                    upgradeConfiguration.Version = $"GCC.{DateTime.UtcNow.Ticks}";
                }

                if (RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME].Equals("gcp test", StringComparison.OrdinalIgnoreCase))
                {
                    upgradeConfiguration.Version = $"GCP_Test.{DateTime.UtcNow.Ticks}";
                }
            }
#if DEBUG
            upgradeConfiguration.SpecificWorker = Dns.GetHostName();
            upgradeConfiguration.Version = $"DEV.{DateTime.UtcNow.Ticks}";
            upgradeConfiguration.IsUpgradeMode = true;
#endif

            Logger.Info($"Start execute upgrade task, version [{upgradeConfiguration.Version}].");

            var upgradeResult = UpgradeHelper.StartUpgradeAsync(upgradeConfiguration, new RMControlDBUpgrader(), new RMTenantUpgrader()).GetAwaiter().GetResult();

            Logger.Info("Successful execute upgrade task.");
        }
       
        private static void CrashHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Logger?.Error("App crashed, {0}", args.ExceptionObject);
            RALogger.WaitForAllLogsFlush();
        }

        private static void InitEnv()
        {
            try
            {
                Logger.Info("Begin initial env.");
                RMServiceManagerUtil.Init();
                RMGlobalConfiguration.Init();
                InitCastle();
                RALogger.SetCustomizedLogPostfix("V: " + RMGlobalConfiguration.EnvSetting.ProductVersion);
                AosApiUtility.Init(RMCertificateHelper.GetCertificate(RMCertNames.AvePointRecords));

                Logger.Info("Initial env successful.");
            }
            catch (Exception e)
            {
                Logger.Error("An error occur while Initial env. error: {0}", e);
                throw;
            }
        }


        private static void InitCastle()
        {
            try
            {
                Logger.Info("Begin initial castle.");
                string installPath = AppDomain.CurrentDomain.BaseDirectory;
                WindsorContainer windsorContainer = new WindsorContainer();
                windsorContainer.Register(
                    Component.For<IWindsorContainer>().Instance(windsorContainer)
                );
                windsorContainer.Install(Castle.Windsor.Installer.Configuration.FromXmlFile(
                    Path.Combine(installPath, "Config/Castle/ServiceCastle.config")));
                AppDomain.CurrentDomain.SetData("CoreIOCContainerIdentifier", windsorContainer);
                PlatformWindsorManager.SetUp(windsorContainer);
                Logger.Info("Initial castle successful.");
            }
            catch (Exception e)
            {
                Logger.Error("An error occur while initial castle. error: {0}", e);
                throw;
            }
        }

    }
}
