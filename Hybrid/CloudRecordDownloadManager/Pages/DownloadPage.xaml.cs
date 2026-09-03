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
using CloudRecordDownloadManager.Properties;
using CloudRecordDownloadManager.Utils.Other;
using CloudRecordDownloadManager.Windows;
using static CloudRecordDownloadManager.Utils.Http.HttpExecutor;

namespace CloudRecordDownloadManager.Pages
{

    public partial class DownloadPage : BasePage
    {

        private bool _onError;

        private readonly TaskScheduler _uiContext = TaskScheduler.FromCurrentSynchronizationContext();

        public bool IsUpgrade { get; set; }

        public DownloadPage()
        {
            InitializeComponent();
            AutoSetBackgroundImage(BackgroundImage);
            InstallButton.IsEnabled = false;
        }

        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            base.OnLoaded(sender, e);
            DownloadPackage();
        }

        private bool OnError
        {
            get => _onError;
            set
            {
                _onError = value;
                InstallButton.Content = _onError ? I18N.key_5c9f2d7f_aa92_4332_bed1_9e9910dbcb92 : I18N.key_f43a31c4_bdcb_4a47_8bfa_12e1db8d3de3;
                InstallButton.IsEnabled = true;
            }
        }

        private void InstallAction(object sender, RoutedEventArgs e)
        {
            if (OnError)
            {
                InstallButton.IsEnabled = false;
                DownloadPackage();
            }
            else if (IsUpgrade)
            {
                ToPage<InstallUpdatePackagePage>();
            }
            else
            {
                ToPage<InstallMsiPage>();
            }

        }

        private void UpdateIndicatorMaximum(long contentLength)
        {
            Log.Info($"[{ClassName}] file size: {contentLength}");
            Task.Factory.StartNew(() => Indicator.Maximum = (double)contentLength, CancellationToken.None, TaskCreationOptions.None, _uiContext);
        }


        private void UpdateIndicator(long contentLength)
        {
            // Log.Info($"[{ClassName}] file size: {contentLength}");
            Task.Factory.StartNew(() => { Indicator.Value = contentLength; }, CancellationToken.None, TaskCreationOptions.None, _uiContext);
        }

        private void DownloadPackage()
        {
            var suffix = IsUpgrade ? ".msp" : ".msi";
            var fileName = RandomString.Generate() + suffix;
            var url = IsUpgrade ? ConstValue.PatchUrl : ConstValue.PackageUrl;
            DownloadFile(url, fileName);
        }

        private void DownloadFile(string sourceUrl, string fileName)
        {
            if (Processing)
            {
                return;
            }

            Processing = true;
            Indicator.Value = 0;
            //var uiContext = TaskScheduler.FromCurrentSynchronizationContext();
            // var fileFtp = ConstValue.MsiPackageUri;
            //var fileName = RandomString.Generate() + ".msi";
            var filePath = Path.Combine(RuntimeCache.DownloadPath, fileName);
            var file = new FileInfo(filePath);

            Task.Factory.StartNew(async () =>
            {
                try
                {
                    var updateMaxFileSize = new UpdateMaxFileSize(UpdateIndicatorMaximum);
                    var updateFileSize = new UpdateFileSize(UpdateIndicator);

                    var result = await Download(sourceUrl, filePath, updateMaxFileSize, updateFileSize);
                    if (result.Status == 1)
                    {
                        await Task.Factory.StartNew(() =>
                        {
                            OnError = false;
                            Indicator.Value = Indicator.Maximum;
                        }, CancellationToken.None, TaskCreationOptions.None, _uiContext);
                        Log.Info($"[{ClassName}] file: {filePath}");
                        RuntimeCache.PackagePath = filePath;
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
        }
    }
}