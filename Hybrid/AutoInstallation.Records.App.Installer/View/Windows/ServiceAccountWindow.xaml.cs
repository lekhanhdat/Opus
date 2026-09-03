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
using AvePoint.Hybrid.Utility.Configuration;
using AvePoint.Hybrid.Utility.ConfigurationFile;
using HybridCommonModel.Utils;
using System;
using System.Management;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows;
using COMMRESX = AutoInstallation.Records.App.Resources.Resource;


namespace AutoInstallation.Records.App.Installer.View.Windows
{
    /// <summary>
    /// Interaction logic for ServiceAccountWindow.xaml
    /// </summary>
    public partial class ServiceAccountWindow : BaseWindow
    {
        private static readonly string AgentServiceName = "AvePointCloudAgentService";

        private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly AgentServiceAccountData data =
            AgentServiceAccountData.GetInstance();
        private bool isGMSAAccount;
        private string AccountName;
        public ServiceAccountWindow()
        {
            InitializeComponent();
            AutoSetBackgroundImage(BackgroundImage);
            InitData();
            DataContext = data;
        }

        private void InitData()
        {
            if (data.IsInitialed) return;

            var setting = AgentAccountUtil.Get();
            if (setting != null)
            {
                data.AccountName = $"{setting.Domain}\\{setting.UserName}";
                data.AccountPassword = setting.Password;
                if (data.AccountPassword == null && data.AccountName.EndsWith("$"))
                {
                    isGMSAAccount = true;
                    AccountName = data.AccountName;
                }
            }
            data.IsInitialed = true;
        }

        private void BaseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            data.Reset();
        }

        private void SetLoading()
        {
            data.IsConfiguring = !data.IsConfiguring;
            this.NextBtn.IsEnabled = !data.IsConfiguring;
            this.BackBtn.IsEnabled = !data.IsConfiguring;
        }
        private void PreAction(object sender, RoutedEventArgs e)
        {
            ToWindow<ConfigurationFileWindow>();
        }

        private void NextAction(object sender, RoutedEventArgs e)
        {

            SetLoading();
            var thread = new Thread(() => {
                var s = Check();

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    SetLoading();
                    if (s)
                    {
                        ToWindow<CompleteWindow>();
                    }
                })).Wait();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }

        private bool SaveData()
        {
            try
            {
                var agentAccount = data.Convert();
                AgentAccountUtil.Save(agentAccount);
                //SaveProxyData();
                return true;
            }
            catch(Exception e)
            {
                logger.Error($"Faile to save agent account info. error : {e.ToString()}");
                return false;
            }
        }

        private void SaveProxyData()
        {
            try
            {
                var data = ProxySettingData.GetInstance();
                if (!data.EnableProxy)
                {
                    AveWebProxyUtil.RemoveProxySetting();
                    return;
                }
                var proxyOptions = data.Convert2Options();
                AveWebProxyUtil.WriteProxySetting(proxyOptions);
            }
            catch (Exception e)
            {
                logger.Error($"Failed to save proxy setting, error : {e.ToString()}");
            }
        }

        private bool Check()
        {
            data.AccountPasswordMsg = string.Empty;
            data.AccountNameMsg = string.Empty;
            var accountName = string.IsNullOrEmpty(data.AccountName) ? string.Empty : data.AccountName.Trim();
            if (string.IsNullOrEmpty(accountName) || accountName.Split('\\').Length != 2)
            {
                //data.AccountNameMsg = $"Please enter the service account with format : domain\\username.";
                data.AccountNameMsg = COMMRESX.ConfigurationTool_Key_GUI_Enter_Service_Account_Name;
                return false;
            }


            var pwd = string.IsNullOrEmpty(data.AccountPassword) ? string.Empty : data.AccountPassword.Trim();
            if (string.IsNullOrEmpty(pwd))
            {
                //data.AccountPasswordMsg = "Please enter the password.";
                if (isGMSAAccount && AccountName == accountName)
                {
                    logger.Info("this is gmsa account,no need check again");
                    return true;
                }
                data.AccountPasswordMsg = COMMRESX.ConfigurationTool_Key_GUI_Enter_Service_Account_Password;
                data.AccountPasswordVis = System.Windows.Visibility.Visible;
                return false;
            }

            try
            {
                var returnCode = ChangeServiceAccountInfobyWMI(AgentServiceName, accountName, pwd);
                if (returnCode == 0)
                {
                    logger.Info("Service account information changed successfully");
                }
                else
                {
                    var errorMsg = COMMRESX.ConfigurationTool_Key_GUI_Change_Service_Account_Failed;
                    //if (returnCode == 2) // access denied
                    //{

                    //}
                    //else if (returnCode == 15) // The service does not have the correct authentication to run on the system.
                    //{

                    //}
                    //else 
                    if (returnCode == 22) // Invalid Service Account
                    {
                        errorMsg = COMMRESX.ConfigurationTool_Key_GUI_Change_Service_Account_Invalid_Account;
                    }

                    var returnCodeUrl = "https://msdn.microsoft.com/en-us/library/aa393660(v=vs.85).aspx";
                    logger.Error($"Failed to change Service account information, Error code: {returnCode}, please refer to {returnCodeUrl} for details.");
                    // Support link to check the message for corresponding Return code:
                    // https://msdn.microsoft.com/en-us/library/aa393660(v=vs.85).aspx
                    data.AccountPasswordMsg = errorMsg;
                    data.AccountPasswordVis = System.Windows.Visibility.Visible;
                    return false;
                }
                
            }
            catch (Exception e)
            {
                logger.Error($"Failed to change Service account, error : {e.ToString()}");
                //data.AccountPasswordMsg = "Failed to change Service account, please see log for details.";
                data.AccountPasswordMsg = COMMRESX.ConfigurationTool_Key_GUI_Change_Service_Account_Failed;

                data.AccountPasswordVis = System.Windows.Visibility.Visible;
                return false;
            }

            if (!SaveData()) return false;

            if (!RestartService())
            {
                data.AccountPasswordMsg = COMMRESX.ConfigurationTool_Key_GUI_Start_Service_Failed;

                data.AccountPasswordVis = System.Windows.Visibility.Visible;
                return false;
            }


            UpdatePackageStatus();
            //if (!UpdatePackageStatus())
            //{
            //    data.AccountPasswordMsg = "Failed to update installation status to web api, please see log for details.";
            //    data.AccountPasswordVis = System.Windows.Visibility.Visible;
            //    return false;
            //}

            return true;
        }


        private static uint ChangeServiceAccountInfobyWMI(string serviceName, string username,
          string password)
        {
            string mgmntPath = string.Format("Win32_Service.Name='{0}'", serviceName);
            using (ManagementObject service = new ManagementObject(new ManagementPath(mgmntPath)))
            {
                object[] accountParams = new object[11];
                accountParams[6] = username;
                accountParams[7] = password;
                uint returnCode = (uint)service.InvokeMethod("Change", accountParams);
                //if (returnCode == 0)
                //{
                //    logger.Info("Service account information changed successfully");
                //}
                //else
                //{
                //    var returnCodeUrl = "https://msdn.microsoft.com/en-us/library/aa393660(v=vs.85).aspx";
                //    logger.Error($"Failed to change Service account information, Error code: {returnCode}, please refer to {returnCodeUrl} for details.");
                //    // Support link to check the message for corresponding Return code:
                //    // https://msdn.microsoft.com/en-us/library/aa393660(v=vs.85).aspx
                //}
                return returnCode;
            }
        }

        private bool UpdatePackageStatus()
        {
            if (ConfigurationFileData.GetInstance().IsUsingExistingConfig) return true;

            try
            {
                var config = AgentConfigurationFileHelper.ReadFromRegistry();
                logger.Info($"Read config from registry : {config != null}");
                return ConfigurationFileChecker.UpdateConfigFileStatus(config, ProxySettingData.GetInstance().Convert2Options());
            }
            catch (Exception e)
            {
                logger.Error($"Failed to update configuration file status to web api, {e.ToString()}");
                return false;
            }
        }

        private bool RestartService()
        {
            ServiceController service = new ServiceController(AgentServiceName);
            if (service.DependentServices.Length == 0)
            {
                TimeSpan timeout = TimeSpan.FromMinutes(2);
                try
                {
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        logger.Info($"start to stop agent service '{AgentServiceName}'");
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                        logger.Info($"agent service '{AgentServiceName}' stopped");
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"Failed to stop agent service '{AgentServiceName}', error : {e.ToString()}");
                    return false; ;
                }

                try
                {
                    logger.Info($"start to start agent service '{AgentServiceName}'");
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                    logger.Info($"Start agent service '{AgentServiceName}' successfully");
                }
                catch (Exception e)
                {
                    logger.Error($"Failed to start agent service '{AgentServiceName}', error : {e.ToString()}");
                    return false;
                }
            }
            else
            {
                StringBuilder eventLogString = new StringBuilder();
                eventLogString.AppendLine(String.Format("Restart service is {0}, display name is {1}", service.ServiceName, service.DisplayName));
                foreach (ServiceController dependentService in service.DependentServices)
                {
                    eventLogString.AppendLine(String.Format("Dependent services contain {0}, display name is {1}", dependentService.ServiceName, dependentService.DisplayName));
                }
                logger.Warn("We need to restart service and its dependent service manually:{0}", eventLogString.ToString());
                return false;
            }

            return true;
        }
    }
}
