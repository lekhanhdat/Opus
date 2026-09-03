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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CloudRecordDownloadManager.Modules;
using CloudRecordDownloadManager.Properties;

namespace CloudRecordDownloadManager.Pages {

    public partial class ExaminationPage : BasePage {

        private bool _canDownload;

        public ExaminationPage() {
            InitializeComponent();
            AutoSetBackgroundImage(BackgroundImage);
            ResetSource();
        }

        private ObservableCollection<ExaminationItem> Items { get; set; }

        private bool CanDownload {
            get => _canDownload;
            set {
                _canDownload = value;
                DownloadButton.Content = _canDownload ? I18N.key_75584acc_6208_43b5_b8d9_8cfaab54f03e : I18N.key_f2d4f453_4434_4200_8525_4b839c30de2a;
            }
        }

        protected override void OnLoaded(object sender, RoutedEventArgs e) {
            base.OnLoaded(sender, e);
            StartExamination();
        }

        private void BackAction(object sender, RoutedEventArgs e) {
            ToPage<InstallPathPage>();
        }

        private void NextAction(object sender, RoutedEventArgs e) {
            if (CanDownload) {
                ToPage<DownloadPage>();
            } else {
                ResetSource();
                StartExamination();
            }
        }

        private void ResetSource() {
            var uiContext = TaskScheduler.FromCurrentSynchronizationContext();
            Items = new ObservableCollection<ExaminationItem> {
                new ExaminationItem(ExaminationType.Network, uiContext),
                new ExaminationItem(ExaminationType.PhysicalMemory, uiContext),
                new ExaminationItem(ExaminationType.DotNetFramework, uiContext),
                new ExaminationItem(ExaminationType.DiskSpace, uiContext)
            };
            ExaminationView.ItemsSource = Items;
        }

        private void StartExamination() {
            Log.Info($"[{ClassName}] start examination");
            DownloadButton.IsEnabled = false;
            var uiContext = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Factory.StartNew(async () => {
                await Task.Delay(1000);
                var tasks = Items.Select(i => i.Checker.CheckTask()).ToList();
                Parallel.ForEach(tasks, task => task.Start());
                var canDownload = tasks.All(t => t.Result == ExaminationStatus.Pass);
                await Task.Factory.StartNew(() => {
                    CanDownload = canDownload;
                    DownloadButton.IsEnabled = true;
                }, CancellationToken.None, TaskCreationOptions.None, uiContext);
                Log.Info($"[{ClassName}] examination end");
            });
            
        }

    }

}