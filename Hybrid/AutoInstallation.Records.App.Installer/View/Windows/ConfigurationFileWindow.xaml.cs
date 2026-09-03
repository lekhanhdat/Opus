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
using AvePoint.Hybrid.ClientLibrary.SDK;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.Hybrid.Utility.ConfigurationFile;
using CommonModel.MethodInfo;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using COMMRESX = AutoInstallation.Records.App.Resources.Resource;

namespace AutoInstallation.Records.App.Installer.View.Windows
{
    /// <summary>
    /// Interaction logic for ConfigurationFileWindow.xaml
    /// </summary>
    public partial class ConfigurationFileWindow : BaseWindow
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ConfigurationFileData data =
            ConfigurationFileData.GetInstance();

        public ConfigurationFileWindow()
        {
            InitializeComponent();
            AutoSetBackgroundImage(BackgroundImage);
            InitData();
            DataContext = data;
        }

        private void InitData()
        {
            if (data.IsInitialed) return;

            var setting = AgentConfigurationFileHelper.ReadFromRegistry();
            data.IsUsingExistingConfig = setting != null;
            data.IsInitialed = true;
        }

        private void BaseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            data.Reset();
            //data.IsUsingExistingConfig = ExistingConfig();
            ResetCtl();
        }

        private void ResetCtl()
        {
            CtlConfigFilePath.IsEnabled = !data.IsUsingExistingConfig;
            CtlInstallationCode.IsEnabled = !data.IsUsingExistingConfig;
            BtnSelectConfigFile.IsEnabled = !data.IsUsingExistingConfig;
        }

        private void SetLoading()
        {
            data.IsConfiguring = !data.IsConfiguring;
            this.NextBtn.IsEnabled = !data.IsConfiguring;
            this.BackBtn.IsEnabled = !data.IsConfiguring;

        }
        private void NextAction(object sender, RoutedEventArgs e)
        {
            
            SetLoading();
            var thread = new Thread(()=> {
                var s = Check();

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    SetLoading();
                    if (s)
                    {
                        ToWindow<ServiceAccountWindow>();
                    }
                })).Wait();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
            
        }

        private bool ExistingConfig()
        {
            return AgentConfigurationFileHelper.ReadFromRegistry() != null;
        }

        private bool Check()
        {
            data.Reset();
            //data.ConfigFilePathMsg = string.Empty;
            //data.InstallationCodeMsg = string.Empty;

            if (data.IsUsingExistingConfig)
            {
                if (ExistingConfig())
                {
                    logger.Info("Using existing configuration file");
                    return true;
                }
                data.InstallationCodeMsg = COMMRESX.ConfigurationTool_Key_GUI_Exisitng_Configuration_Invalid;
                return false;
            }

            var configFilePath = string.IsNullOrEmpty(data.ConfigFilePath) ? string.Empty : data.ConfigFilePath.Trim();
            if (string.IsNullOrEmpty(configFilePath))
            {
                //data.ConfigFilePathMsg = "Please select the configuration file.";
                data.ConfigFilePathMsg = COMMRESX.ConfigurationTool_Key_GUI_Select_Configuration_File;
                return false;
            }

            if (!File.Exists(configFilePath))
            {
                //data.ConfigFilePathMsg = "Configuration file does not exist.";
                data.ConfigFilePathMsg = COMMRESX.ConfigurationTool_Key_GUI_Configuration_File_Not_Exists;
                return false;
            }

            var installationCode = string.IsNullOrEmpty(data.InstallationCode) ? string.Empty : data.InstallationCode.Trim();
            if (string.IsNullOrEmpty(installationCode))
            {
                //data.InstallationCodeMsg = "Please enter the installation code.";
                data.InstallationCodeMsg = COMMRESX.ConfigurationTool_Key_GUI_Enter_Installation_Code;

                return false;
            }

            AgentConfigurtion config = AgentConfigurationFileHelper.ReadFromLocalPath(configFilePath, installationCode);
            if (config == null)
            {
                //data.InstallationCodeMsg = "Configuration file is invalid or installation code is wrong.";
                data.InstallationCodeMsg = COMMRESX.ConfigurationTool_Key_GUI_Installation_Code_Or_File_Invalid;
                logger.Error($"Failed to Validate configuration file.");
                return false;
            }

            if (!CheckScope(config))
            {
                return false;
            }

            if (CheckIfAlreadyUsed(config))
            {
                return false;
            }

            if (!WriteConfig(configFilePath, installationCode))
            {
                //data.InstallationCodeMsg = "Failed to save configuration file.";
                data.InstallationCodeMsg = COMMRESX.ConfigurationTool_Key_GUI_Configuration_File_Save_Failed;

                return false;
            }

            return true;
        }

        private bool CheckScope(AgentConfigurtion configurtion)
        {
            var result = true;
            try
            {
                result = ConfigurationFileChecker.ValidateScope(configurtion, ProxySettingData.GetInstance().Convert2Options());
            }
            catch (Exception e)
            {
                logger.Error($"Failed to check scope, error :{e.ToString()}");
                data.ConfigFilePathMsg = COMMRESX.ConfigurationTool_Key_GUI_Configuration_File_Check_Failed;
                return false;
            }

            if (!result)
            {
                logger.Warn($"Scope checking result : {result}. Customer Id: {configurtion.CustomerId}, Identity server : {configurtion.IdentityServiceUrl}");
                var scopes = $"\r\n{APIScope.Agent}" +
                    $"\r\n{HybridAgentPermissionScopes.ReadWrite_All}" +
                    $"\r\n{APIScope.Common}";
                //data.ConfigFilePathMsg = $"Invalid scope, please make sure the following scopes are enabled with Client Id '{configurtion.ClientId}' :" +
                //    scopes;
                data.ConfigFilePathMsg = string.Format(COMMRESX.ConfigurationTool_Key_GUI_Configuration_File_Scope_Invalid, configurtion.ClientId, scopes);
            }

            return result;
        }

        private bool IsLicenseExpired(Exception e)
        {
            return e.ToString().Contains("license is expired");
        }

        private bool CheckIfAlreadyUsed(AgentConfigurtion configurtion)
        {
            var isUsed = false;
            try
            {
                isUsed = !ConfigurationFileChecker.Validate(configurtion, ProxySettingData.GetInstance().Convert2Options());
            }
            catch (Exception e)
            {
                logger.Error($"Failed to check if configuration file is used, error : {e.ToString()}");
                data.ConfigFilePathMsg = IsLicenseExpired(e)? COMMRESX.ConfigurationTool_Key_GUI_License_Expired : COMMRESX.ConfigurationTool_Key_GUI_Configuration_File_Check_Failed;

                return true;
            }

            if (isUsed) data.ConfigFilePathMsg = COMMRESX.ConfigurationTool_Key_GUI_Configuration_File_Already_Used;

            return isUsed;
        }

        private bool WriteConfig(string filePath, string installationCode)
        {
            try
            {
                AgentConfigurationFileHelper.WriteInstallationCode(installationCode);
                logger.Info("Write installation code succussfully.");
                AgentConfigurationFileHelper.WriteConfig(filePath, installationCode);
                logger.Info("Write configuration file succussfully.");
            }
            catch (Exception e)
            {
                logger.Error($"Failed to save configuration file. error : {e.ToString()}");
                return false;
            }

            return true;
        }

        private void ClickExistingConfigFile(object sender, RoutedEventArgs e)
        {
            ResetCtl();
        }

        private void PreAction(object sender, RoutedEventArgs e)
        {
            ToWindow<ProxyWindow>();
        }
    }
}
