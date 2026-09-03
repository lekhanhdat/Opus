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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CloudRecordDownloadManager.Cache;
using CloudRecordDownloadManager.Checkers;
using CloudRecordDownloadManager.Model;
using CloudRecordDownloadManager.Properties;
using CloudRecordDownloadManager.Utils.Other;
using CloudRecordDownloadManager.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using static CloudRecordDownloadManager.Utils.Http.HttpExecutor;

namespace CloudRecordDownloadManager.Pages {

    public partial class WelcomePage : BasePage {
        
        private const string PackageId = "{DF5D64B0-C0C8-99C3-8650-031A7B9ADE3A}";
        private const string RegistryUninstall = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

        private readonly bool ReinstallMode;

        private string installedVersion = null;

        private bool _onError;

        private readonly TaskScheduler _uiContext = TaskScheduler.FromCurrentSynchronizationContext();


        private RegistryKey GetProduct()
        {
            foreach (var packageId in ConstValue.PackageIds)
            {
                var uninstall = Registry.LocalMachine.OpenSubKey($@"{ConstValue.RegistryUninstall}\{packageId}");
                if (uninstall != null)
                {
                    return uninstall;
                }
            }
            return null;
        }
        public WelcomePage() {
            InitializeComponent();
            AutoSetBackgroundImage(BackgroundImage);

            //var uninstall = Registry.LocalMachine.OpenSubKey($@"{ConstValue.RegistryUninstall}\{ConstValue.PackageId}");
            //uninstall = uninstall??Registry.LocalMachine.OpenSubKey($@"{ConstValue.RegistryUninstall}\{ConstValue.OldPackageId}");
            var uninstall = GetProduct();
            if (uninstall == null) {
                ReinstallMode = false;
                Log.Info("[product] not found");
            } else {
                ReinstallMode = true;
                installedVersion = uninstall.GetValue(ConstValue.RegistryDisplayVersion).ToString();
                Log.Info("[product] found");
            }
            ToExaminationButton.IsEnabled = !ReinstallMode;
        }
        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            base.OnLoaded(sender, e);
            /*if (ReinstallMode) */DownloadConfig();

        }

        private bool OnError
        {
            get => _onError;
            set
            {
                _onError = value;
                //InstallButton.Content = _onError ? I18N.key_5c9f2d7f_aa92_4332_bed1_9e9910dbcb92 : I18N.key_f43a31c4_bdcb_4a47_8bfa_12e1db8d3de3;
                ToExaminationButton.IsEnabled = true;
            }
        }

        private void ToExaminationPage(object sender, RoutedEventArgs e) {

            if (OnError)
            {
                ToExaminationButton.IsEnabled = false;
                DownloadConfig();
            }
            
            if (ReinstallMode) {
                if (CanUpgrade())
                {
                    //ToPage<InstallUpdatePackagePage>();
                    if (!RuntimeCache.IsMajorUpdate && !RuntimeCache.IsMinorUpdate)
                    {
                        ToPage<DownloadPage>(page => page.IsUpgrade = true);
                    }
                    else
                    {
                        ToPage<LicensePage>();
                    }
                }
                else
                {
                    ToPage<ErrorPage>(page =>
                    {
                        page.TitleLabel.Text = I18N.key_f33a29d5_e169_46a5_8283_01adad651dc6;
                        page.MessageLabel.Text = I18N.key_79c2e46a_a05b_4bc4_bc83_93b15799b650;
                    });
                }
            } else {
                ToPage<LicensePage>();
            }
        }

        private bool CanUpgrade()
        {
            //DownloadConfig();
            //RuntimeCache.AgentInfoConfigPath = $"C:\\AgentUpdate\\CloudAgentInstaller_Info.json";
            if (string.IsNullOrEmpty(RuntimeCache.AgentInfoConfigPath)) return false;
            var latestVersion = new Version(GetLatestVersion());
            var currentVersion = new Version(installedVersion);
            RuntimeCache.IsMajorUpdate = latestVersion.Major > currentVersion.Major;
            RuntimeCache.IsMinorUpdate = latestVersion.Minor > currentVersion.Minor;
            Log.Info($"[{ClassName}] Installed version : [{currentVersion}], Latest version: [{latestVersion}], Is major update : [{RuntimeCache.IsMajorUpdate}], Is minor update : [{RuntimeCache.IsMinorUpdate}]");
            return latestVersion.CompareTo(currentVersion) > 0;
        }

        private string GetLatestVersion()
        {
            var json = File.ReadAllText(RuntimeCache.AgentInfoConfigPath).Replace(@"\r\n", string.Empty);
            var agentInfo = JsonConvert.DeserializeObject<AgentInfoConfig>(json);
            return agentInfo.Version;
        }

        private void DownloadConfig()
        {
            if (Processing)
            {
                return;
            }

            Processing = true;

            var fileName = RandomString.Generate() + ".json";

            var filePath = Path.Combine(RuntimeCache.DownloadPath, fileName);
            var file = new FileInfo(filePath);
            //return true;
            var t = Task.Factory.StartNew(async () =>
            {
                try
                {
                    var result = await Download(ConstValue.AgentInfoUrl, filePath, l => { });
                    if (result.Status == 1)
                    {
                        await Task.Factory.StartNew(() =>
                        {
                            OnError = false;
                            //Indicator.Value = Indicator.Maximum;
                        }, CancellationToken.None, TaskCreationOptions.None, _uiContext);
                        Log.Info($"[{ClassName}] agent info file: {filePath}");
                        RuntimeCache.AgentInfoConfigPath = filePath;
                    }
                    else
                    {
                        throw new Exception(result.Msg);
                    }
                }
                catch (Exception exception)
                {
                    Log.Error(exception, $"[{ClassName}] download file failed");
                    await Task.Factory.StartNew(() => { OnError = true; }, CancellationToken.None, TaskCreationOptions.None, _uiContext);
                    file.Refresh();
                    if (file.Exists)
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception e)
                        {
                            Log.Error(e, $"[{ClassName}] remove download file failed");
                            Log.Error(e);
                        }
                    }
                }

                Processing = false;
            });

            //t.Result.Wait();
        }

    }

}