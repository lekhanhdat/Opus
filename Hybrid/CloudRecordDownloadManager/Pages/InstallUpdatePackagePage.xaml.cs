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
using CloudRecordDownloadManager.Cache;
using CloudRecordDownloadManager.Checkers;
using CloudRecordDownloadManager.Properties;
using CloudRecordDownloadManager.Utils;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CloudRecordDownloadManager.Pages
{

    public partial class InstallUpdatePackagePage : BasePage
    {

        private bool _installed;
        private string AgentServiceName = ConstValue.AgentServiceName;

        public InstallUpdatePackagePage()
        {
            InitializeComponent();
            AutoSetBackgroundImage(BackgroundImage);
            ConfigButton.IsEnabled = false;
        }

        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            base.OnLoaded(sender, e);
            InstallPackage();
        }

        private bool Installed
        {
            get => _installed;
            set
            {
                _installed = value;
                ConfigButton.Content = _installed ? I18N.key_0505679f_b4a8_41b2_8b69_414a6a91292e : I18N.key_75aaabed_5b63_4d43_a5a6_1b4dabab0693;
                ConfigButton.IsEnabled = true;
                if (Indicator.IsIndeterminate)
                {
                    Indicator.IsIndeterminate = false;
                }

                if (_installed)
                {
                    Log.Info($"[{ClassName}] update product succeed");
                }
                else
                {
                    Log.Error($"[{ClassName}] update product failed");
                }

                Processing = false;
            }
        }

        private void InstallPackage()
        {
            if (Processing)
            {
                return;
            }

            Processing = true;
            Indicator.Value = 0;
            //var dir = new DirectoryInfo(RuntimeCache.InstallPath);
            var logFilePath = Path.Combine(RuntimeCache.DownloadPath, "msp.log");

            ///https://www.advancedinstaller.com/user-guide/msiexec.html#:~:text=The%20Windows%20Installer%20technology%20uses%20Msiexec.exe%20for%20installing,can%20set%20the%20install%20type%20through%20these%20options%3A
            var args = $"/p \"{RuntimeCache.PackagePath}\" /qb REINSTALLMODE=\"ecmus\" REINSTALL=\"ALL\" /l* \"{logFilePath}\" /promptrestart";
            //var db = new Database(RuntimeCache.PackagePath);
            // var fileNames = db.ExecuteQuery("SELECT FileName FROM File");
            //var fileCount = db.ExecuteQuery("SELECT File FROM File").Count;
            //Log.Info($"[{ClassName}] msp file count: {fileCount}");
            //Indicator.Maximum = fileCount;

            var uiContext = TaskScheduler.FromCurrentSynchronizationContext();

            Task.Factory.StartNew(() =>
            {

                StopAgentService();

                //var info = new ProcessStartInfo
                //{
                //    FileName = "msiexec.exe",
                //    Arguments = args,
                //    UseShellExecute = false,
                //    CreateNoWindow = true,
                //    Verb = "RunAs"
                //};
                //var process = Process.Start(info);
                //process?.WaitForExit();
                //var logFile = new FileInfo(logFilePath);
                var result = RealInstallPackage(args, logFilePath);
                //if (logFile.Exists)
                //{
                //    var log = File.ReadAllText(logFilePath);
                //    Log.Info($"[msp] =================================\n{log}\n=================================");
                //    result = log.Contains("Installation completed successfully") || log.Contains("Configuration completed successfully");
                //}

                if (result)
                {
                    StartAgentService();
                }

                Task.Factory.StartNew(() =>
                {
                    Indicator.Value = Indicator.Maximum;
                    Installed = result;
                }, CancellationToken.None, TaskCreationOptions.None, uiContext);
            });

            UpdateProgressBar(uiContext);
        }

        private bool RealInstallPackage(string args, string logFilePath)
        {
            var result = false;
            var info = new ProcessStartInfo
            {
                FileName = "msiexec.exe",
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                Verb = "RunAs"
            };
            var process = Process.Start(info);
            process?.WaitForExit();
            var logFile = new FileInfo(logFilePath);
            if (logFile.Exists)
            {
                var log = File.ReadAllText(logFilePath);
                Log.Info($"[msp] =================================\n{log}\n=================================");
                result = log.Contains("Installation completed successfully") || log.Contains("Configuration completed successfully");
            }

            return result;
        }

        private void UpdateProgressBar(TaskScheduler uiContext)
        {
            Task.Factory.StartNew(async () =>
            {
                while (Processing)
                {
                    await Task.Factory.StartNew(() =>
                    {
                        if (Indicator.Value < 95)
                            Indicator.Value = Indicator.Value + 1;
                    }, CancellationToken.None, TaskCreationOptions.None, uiContext);

                    await Task.Delay(500);
                }
            });
        }

        private bool StopAgentService()
        {
            return ServiceUtil.StopService(AgentServiceName);
        }

        private void StartAgentService()
        {
            ServiceUtil.StartService(AgentServiceName);
        }

        private void ConfigAction(object sender, RoutedEventArgs e)
        {
            MainWindow.Close();

            //if (Installed)
            //{
            //    var dir = new DirectoryInfo(RuntimeCache.InstallPath);
            //    var files = dir.GetFiles("CloudAgentConfigurationTool.exe", SearchOption.AllDirectories);
            //    if (files.Length > 0)
            //    {
            //        var tool = files[0];
            //        var info = new ProcessStartInfo
            //        {
            //            FileName = tool.FullName,
            //            UseShellExecute = false,
            //            CreateNoWindow = true,
            //            Verb = "RunAs"
            //        };
            //        var process = Process.Start(info);
            //        Log.Error($"[tool] launched({process?.Id ?? -1}): {tool.FullName}");
            //    }
            //    else
            //    {
            //        Log.Error("[tool] where is it???");
            //    }
            //    MainWindow.Close();
            //}
            //else
            //{
            //    ConfigButton.IsEnabled = false;
            //    InstallPackage();
            //}
        }

    }

}