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
using AutoInstallationCommon.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AutoInstallation.Records.App.Installer.View.Windows
{
    public class BaseWindow : Window
    {

        //protected static readonly Logger Log = LogManager.GetCurrentClassLogger();
        protected static readonly AveLogger Log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);


        protected BaseWindow()
        {
            ResizeMode = ResizeMode.CanMinimize;
            Height = 395;
            Width = 495;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //Title = I18N.key_257e3086_b755_4a8d_a694_451b6ab02aa7;
            Title = "Agent Configuration Tool";

            Background = new SolidColorBrush(Color.FromRgb(248, 248, 248));
            ClassName = GetType().Name;
            // ResizeMode="CanMinimize"
            // Height="395"
            // Width="495"
            // WindowStartupLocation="CenterScreen"
            // Title="{x:Static res:I18N.key_257e3086_b755_4a8d_a694_451b6ab02aa7}"
            // Background="#F8F8F8"
            Closing += OnClosing;
            Log.Info($"[{ClassName}] load");
        }

        protected string ClassName { get; }

        protected bool Processing { get; set; } = false;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        protected void OnClosing(object sender, CancelEventArgs e)
        {
            if (Processing)
            {
                var result = MessageBox.Show("We are under processing, and are you sure to quit?",
                    "Avepoint Cloud Record Agent Configuration Tool",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No);
                if (result != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }

                Log.Info($"[{ClassName}] user cancel processing");
            }

            e.Cancel = false;
            Log.Info($"[{ClassName}] closing");
        }

        protected void CloseAction(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected void ToWindow<T>(Action<T> customWindow = null) where T : Window, new()
        {
            var window = new T { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner };
            customWindow?.Invoke(window);
            window.Show();
            window.Owner = null;
            Close();
        }

        protected void AutoSetBackgroundImage(ImageBrush brush)
        {
            var dpi = 96;
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            if (dpiXProperty != null)
            {
                var dpiX = (int)dpiXProperty.GetValue(null, null);
                dpi = dpiX;
            }

            var dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);
            if (dpiYProperty != null)
            {
                var dpiY = (int)dpiYProperty.GetValue(null, null);
                if (dpiY > dpi) dpi = dpiY;
            }

            dpi *= 100;
            dpi /= 96;
            if (dpi < 125)
                brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CloudAgentConfigurationTool;component/Images/bg.png"));
            else if (dpi < 150)
                brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CloudAgentConfigurationTool;component/Images/bg@1.25x.png"));
            else if (dpi <= 175)
                brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CloudAgentConfigurationTool;component/Images/bg@1.5x.png"));
            else if (dpi <= 200)
                brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CloudAgentConfigurationTool;component/Images/bg@1.75x.png"));
            else if (dpi <= 300)
                brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CloudAgentConfigurationTool;component/Images/bg@2x.png"));
            else
                brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CloudAgentConfigurationTool;component/Images/bg@3x.png"));
        }

    }
}
