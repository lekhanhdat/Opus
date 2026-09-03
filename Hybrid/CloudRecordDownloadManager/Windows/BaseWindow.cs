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
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CloudRecordDownloadManager.Properties;
using NLog;

namespace CloudRecordDownloadManager.Windows {

    public class BaseWindow : Window {

        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();

        protected BaseWindow() {
            ResizeMode = ResizeMode.CanMinimize;
            Height = 395;
            Width = 495;
            Title = I18N.key_257e3086_b755_4a8d_a694_451b6ab02aa7;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Closing += OnClosing;
        }

        public bool Processing { get; set; } = false;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void OnClosing(object sender, CancelEventArgs e) {
            if (Processing) {
                var result = MessageBox.Show(I18N.key_3e6b5d22_4b85_48aa_bb83_473eee71ff61,
                    I18N.key_257e3086_b755_4a8d_a694_451b6ab02aa7,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No);
                if (result != MessageBoxResult.Yes) {
                    e.Cancel = true;
                    return;
                }

                Log.Info($"[window] user cancel processing");
            }

            e.Cancel = false;
            Log.Info($"[window] closing");
        }

    }

}