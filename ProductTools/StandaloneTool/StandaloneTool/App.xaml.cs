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
using AvePoint.RA.CommonUtil;
using log4net;
using StandaloneTool.Common;
using StandaloneTool.Model.Common;
using System.IO;
using System.Windows;

namespace StandaloneTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly RALogger logger;
        private GlobalConfiguration globalConfig = GlobalConfiguration.Instance;

        public App()
        {
            InitLogger();
            logger = RALogger.GetInstance(typeof(App));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try { }
            catch (Exception ex)
            {
                logger.Error($"An error occurred when start app: {ex}");
                Current.Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                CleanExistingExportDBLocation();
                base.OnExit(e);
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred when exit app: {ex}");
                Current.Shutdown();
            }
        }

        private void InitLogger()
        {
#if DEBUG
                        RALogger.ConfigFile = globalConfig.GetSetting<string>(AppSettingKey.LOG_DEBUG_CONFIG_FILENAME);
#else
                        RALogger.ConfigFile = globalConfig.GetSetting<string>(AppSettingKey.LOG_CONFIG_FILENAME);
#endif

            GlobalContext.Properties["LogPath"] = AppDomain.CurrentDomain.BaseDirectory;
            //Environment.SetEnvironmentVariable("LogPath", Path.Combine(Environment.CurrentDirectory, "logs"), EnvironmentVariableTarget.Process);
        }

        private void CleanExistingExportDBLocation()
        {
            if (Directory.Exists(GlobalInfo.ExtractZipLocation))
            {
                Directory.Delete(GlobalInfo.ExtractZipLocation, true);
            }
        }
    }
}
