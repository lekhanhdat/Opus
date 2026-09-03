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
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CloudRecordDownloadManager.Cache;
using CloudRecordDownloadManager.Checkers;
using CloudRecordDownloadManager.Properties;
using CloudRecordDownloadManager.Utils;
using Microsoft.Deployment.WindowsInstaller;

namespace CloudRecordDownloadManager.Pages {

    public partial class InstallMsiPage : BasePage {

        private bool _installed;

        public InstallMsiPage() {
            InitializeComponent();
            AutoSetBackgroundImage(BackgroundImage);
            ConfigButton.IsEnabled = false;
        }
        
        protected override void OnLoaded(object sender, RoutedEventArgs e) {
            base.OnLoaded(sender, e);
            InstallPackage(); 
        }

        private bool Installed {
            get => _installed;
            set {
                _installed = value;
                ConfigButton.Content = _installed ? I18N.key_51a906d9_7bad_4489_b99a_d4450dd177b4 : I18N.key_75aaabed_5b63_4d43_a5a6_1b4dabab0693;
                ConfigButton.IsEnabled = true;
                if (Indicator.IsIndeterminate) {
                    Indicator.IsIndeterminate = false;
                }

                if (_installed) {
                    Log.Info($"[{ClassName}] install product succeed");
                } else {
                    Log.Error($"[{ClassName}] install product failed");
                }

                Processing = false;
            }
        }

        private void InstallPackage() {
            if (Processing) {
                return;
            }

            Processing = true;
            Indicator.Value = 0;
            var dir = new DirectoryInfo(RuntimeCache.InstallPath);
            var logFilePath = Path.Combine(RuntimeCache.DownloadPath, "msi.log");
            var args = $"/i \"{RuntimeCache.PackagePath}\" INSTALLFOLDER=\"{RuntimeCache.InstallPath}\" /qn /l* \"{logFilePath}\"";
            var db = new Database(RuntimeCache.PackagePath);
            // var fileNames = db.ExecuteQuery("SELECT FileName FROM File");
            var fileCount = db.ExecuteQuery("SELECT File FROM File").Count;
            Log.Info($"[{ClassName}] msi file count: {fileCount}");
            Indicator.Maximum = fileCount;

            var uiContext = TaskScheduler.FromCurrentSynchronizationContext();

            Task.Factory.StartNew(() => {

                if (RuntimeCache.IsMajorUpdate) // if is major update , need to reinstall msi file instead of update msp patch file
                {
                    ServiceUtil.StopService(ConstValue.AgentServiceName);
                }

                var info = new ProcessStartInfo {
                    FileName = "msiexec.exe",
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "RunAs"
                };
                var process = Process.Start(info);
                process?.WaitForExit();
                var logFile = new FileInfo(logFilePath);
                var result = false;
                if (logFile.Exists) {
                    var log = File.ReadAllText(logFilePath);
                    Log.Info($"[msi] =================================\n{log}\n=================================");
                    result = log.Contains("Installation completed successfully") || log.Contains("Configuration completed successfully");
                }

                Task.Factory.StartNew(() => {
                    Indicator.Value = Indicator.Maximum;
                    Installed = result;
                }, CancellationToken.None, TaskCreationOptions.None, uiContext);
            });

            Task.Factory.StartNew(async () => {
                while (Processing) {
                    dir.Refresh();
                    if (dir.Exists) {
                        var files = dir.GetFiles("*", SearchOption.AllDirectories);
                        await Task.Factory.StartNew(() => { Indicator.Value = files.Length; }, CancellationToken.None, TaskCreationOptions.None, uiContext);
                    }
                    await Task.Delay(100);
                }
            });
        }

        private void ConfigAction(object sender, RoutedEventArgs e) {
            if (Installed) {
                var dir = new DirectoryInfo(RuntimeCache.InstallPath);
                var files = dir.GetFiles("CloudAgentConfigurationTool.exe", SearchOption.AllDirectories);
                if (files.Length > 0) {
                    var tool = files[0];
                    var info = new ProcessStartInfo {
                        FileName = tool.FullName,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Verb = "RunAs"
                    };
                    var process = Process.Start(info);
                    Log.Error($"[tool] launched({process?.Id ?? -1}): {tool.FullName}");
                } else  {
                    Log.Error("[tool] where is it???");
                }
                MainWindow.Close();
            } else {
                ConfigButton.IsEnabled = false;
                InstallPackage();
            }
        }

    }

}