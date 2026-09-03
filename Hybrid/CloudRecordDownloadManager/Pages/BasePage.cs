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
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CloudRecordDownloadManager.Properties;
using CloudRecordDownloadManager.Windows;
using NLog;

namespace CloudRecordDownloadManager.Pages {

    public class BasePage : Page {

        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();

        protected BasePage() {
            Title = I18N.key_257e3086_b755_4a8d_a694_451b6ab02aa7;
            Background = new SolidColorBrush(Color.FromRgb(248, 248, 248));
            ClassName = GetType().Name;
            Loaded += OnLoaded;
        }
        
        protected virtual void OnLoaded(object sender, RoutedEventArgs e) {
            Log.Info($"[{ClassName}] load");
        }

        public BaseWindow MainWindow { get; set; }
        protected string ClassName { get; }
        protected bool Processing {
            get => MainWindow.Processing;
            set => MainWindow.Processing = value;
        }

        protected void CloseAction(object sender, RoutedEventArgs e) {
            MainWindow.Close();
        }

        protected void AutoSetBackgroundImage(ImageBrush brush) {
            var dpi = 96;
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            if (dpiXProperty != null) {
                var dpiX = (int) dpiXProperty.GetValue(null, null);
                dpi = dpiX;
            }

            var dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);
            if (dpiYProperty != null) {
                var dpiY = (int) dpiYProperty.GetValue(null, null);
                if (dpiY > dpi) dpi = dpiY;
            }

            dpi *= 100;
            dpi /= 96;
            if (dpi < 125)
                brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CloudAgentDownloader;component/Resources/Images/bg.png"));
            else if (dpi < 150)
                brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CloudAgentDownloader;component/Resources/Images/bg@1.25x.png"));
            else if (dpi <= 175)
                brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CloudAgentDownloader;component/Resources/Images/bg@1.5x.png"));
            else if (dpi <= 200)
                brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CloudAgentDownloader;component/Resources/Images/bg@1.75x.png"));
            else if (dpi <= 300)
                brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CloudAgentDownloader;component/Resources/Images/bg@2x.png"));
            else
                brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CloudAgentDownloader;component/Resources/Images/bg@3x.png"));
        }
        
        protected void ToPage<T>(Action<T> customPage = null) where T : BasePage, new() {
            var page = new T {MainWindow = MainWindow};
            customPage?.Invoke(page);
            MainWindow.Content = page;
            Log.Info($"[{ClassName}] closed");
        }

    }

}