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
using AutoInstallation.Records.App.Installation.ViewModel.binding;
using AutoInstallation.Records.App.Installer.ViewModel;
using AutoInstallationCommon.Utility;
using AvePoint.Hybrid.Utility.Net;
using HybridCommonModel.Utils;
using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using COMMRESX = AutoInstallation.Records.App.Resources.Resource;


namespace AutoInstallation.Records.App.Installer.View.Windows
{
    /// <summary>
    /// Interaction logic for ProxyWindow.xaml
    /// </summary>
    public partial class ProxyWindow : BaseWindow
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ProxySettingData data = ProxySettingData.GetInstance();
        public ProxyWindow()
        {
            InitializeComponent();
            AutoSetBackgroundImage(BackgroundImage);
            InitData();
            DataContext = data;
        }

        private void InitData()
        {
            if (data.IsInitialed) return;
            try
            {
                var setting = AveWebProxyUtil.ReadProxySetting();
                if (setting != null)
                {
                    data.EnableProxy = setting.Enabled;
                    data.ProxyHost = setting.Host;
                    data.ProxyPort = setting.Port.ToString();
                    data.UserName = setting.UserName;
                    data.Password = setting.Password;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to init data setting, error : {ex.ToString()}");
                throw;
            }
            
            data.IsInitialed = true;
        }

        private void SaveData()
        {
            if (!data.EnableProxy)
            {
                AveWebProxyUtil.RemoveProxySetting();
                return;
            }
            var proxyOptions = data.Convert2Options();
            AveWebProxyUtil.WriteProxySetting(proxyOptions);
        }

        private void BaseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            data.Reset();
            ResetCtl();
        }

        private void ResetCtl()
        {
            NameProxyHost.IsEnabled = data.EnableProxy;
            NameProxyPort.IsEnabled = data.EnableProxy;
            NameProxyUserName.IsEnabled = data.EnableProxy;
            NameProxyPassword.IsEnabled = data.EnableProxy;

        }

        private void SetLoading()
        {
            data.IsConfiguring = !data.IsConfiguring;
            this.NextBtn.IsEnabled = !data.IsConfiguring;
        }

        private void NextAction(object sender, RoutedEventArgs e)
        {

            SetLoading();
            var thread = new Thread(() =>
            {
                var s = Check();

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    SetLoading();
                    if (s)
                    {
                        ToWindow<ConfigurationFileWindow>();
                    }
                })).Wait();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }
        private bool Check()
        {
            data.ProxyHostMsg = string.Empty;
            data.ProxyPortMsg = string.Empty;
            data.ErrorMsg = string.Empty;
            data.UserNameMsg = string.Empty;
            if (data.EnableProxy)
            {
                if (string.IsNullOrEmpty(data.ProxyHost))
                {
                    DisplayErrorMsg(COMMRESX.ConfigurationTool_Key_GUI_Enter_Valid_Proxy_Host);
                    return false;
                }

                if (string.IsNullOrEmpty(data.ProxyPort) || !Int32.TryParse(data.ProxyPort, out int oPort))
                {
                    DisplayErrorMsg(COMMRESX.ConfigurationTool_Key_GUI_Enter_Valid_Proxy_Port);
                    return false;
                }

                if (!TestProxy())
                {
                    DisplayErrorMsg(COMMRESX.ConfigurationTool_Key_GUI_Test_Proxy_Failed);
                    return false;
                }
            }
            try
            {
                SaveData();
            }
            catch (Exception e)
            {
                logger.Error($"Failed to save proxy setting, error : {e.ToString()}");
                DisplayErrorMsg(COMMRESX.ConfigurationTool_Key_GUI_Test_Proxy_Failed);
                return false;
            }


            return true;
        }

        private void DisplayErrorMsg(string errorMsg)
        {
            data.ErrorMsg = errorMsg;
            data.ErrorMsgVis = System.Windows.Visibility.Visible;
        }

        private bool TestProxy()
        {
            var proxyOptions = data.Convert2Options();

            try
            {
                var result = AveHttpConnectionUtil.TestWebProxyAsync(proxyOptions).Result;
                return result;
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while test proxy connection. error : {e.ToString()}");
                return false;
            }
        }

        private void ClickEnableProxy(object sender, RoutedEventArgs e)
        {
            ResetCtl();
        }

    }
}
