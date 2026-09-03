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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using CloudRecordDownloadManager.Cache;
using CloudRecordDownloadManager.Checkers;
using CloudRecordDownloadManager.Utils.Other;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace CloudRecordDownloadManager.Main {

    public partial class App : Application {

        private Mutex _mutex;

        public App() {
            Startup += App_Startup;
            Exit += App_Exit;
            InitServicePoint();
        }

        private void InitServicePoint(int connectionLimit = 12)
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = connectionLimit;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
        }

        private void App_Exit(object sender, ExitEventArgs args) {
            var log = LogManager.GetCurrentClassLogger();
            try {
                Directory.Delete(RuntimeCache.DownloadPath, true);
                log.Info($"[download] remove folder: {RuntimeCache.DownloadPath}");
            } catch (Exception e) {
                log.Error(e, $"[download] remove folder: {RuntimeCache.DownloadPath}");
            }

            // 防止文件被本身占用后程序本身无法删除
            var info = new ProcessStartInfo {
                Arguments = $"/C choice /C Y /N /D Y /T 3 & rmdir /S /Q \"{RuntimeCache.DownloadPath}\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = false,
                UseShellExecute = true,
                FileName = "cmd.exe"
            };
            Process.Start(info);
            
            log.Info("[app] shutdown");
        }

        private void App_Startup(object sender, StartupEventArgs args) {
            _mutex = new Mutex(true, "cloud_record_download_manager", out var createdNew);
            if (!createdNew) Current.Shutdown();
            var name = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
            var config = new LoggingConfiguration();
            var logFile = new FileTarget("file") {
                FileName = $"{name}_{DateTime.Now:yyyy-MM-dd_HH:mm:ss}.log",
                Layout = "${longdate} [${level}] ${message} ${exception}",
                Encoding = Encoding.UTF8
            };
            var logConsole = new ConsoleTarget("console") {
                Layout = @"${date:format=HH\:mm\:ss}[${level}] ${message} ${exception}"
            };
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logConsole);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logFile);
            LogManager.Configuration = config;
            var log = LogManager.GetCurrentClassLogger();
            log.Info($"[app] launch with arguments: {args.Args}");

            var folder = RandomString.Generate();
            var tmp = Path.GetTempPath();
            RuntimeCache.DownloadPath = Path.Combine(tmp, folder);
            try {
                Directory.CreateDirectory(RuntimeCache.DownloadPath);
                log.Info($"[download] create folder: {RuntimeCache.DownloadPath}");
            } catch (Exception e) {
                log.Error($"[download] create folder: {RuntimeCache.DownloadPath}");
                log.Error(e);
            }

            ConstValue.ReadConfigFile();
        }

    }

}