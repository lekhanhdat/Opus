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
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using GUIRESX = AutoInstallation.Records.App.Resources.Resource;

namespace AutoInstallation.Records.App.Installer
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!IsAdmin())
            {
                var adminMessage = GUIRESX.COMMON_Key_ToolRunAdmin;
                var adminSummary = GUIRESX.ConfigurationTool_Key_GUI_App_Title;

                System.Windows.Forms.MessageBox.Show(adminMessage,
                    adminSummary,
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            logger.Info($"current culture: {CultureInfo.CurrentUICulture.ToString()}");
            InitServicePoint();
            //var data = ContentData.GetInstance();
            //PopupMessageBox.GetInstance(data.MainWindowViewModel.Data.IConImage);
            //if (!AveEnv.IsNonSPInstalled && AveEnv.IsSharePoint2010OrLower)
            //{
            //    PopupMessageBox.GetInstance()
            //        .ShowWarningMessageBox(GUIRESX.APPINSTALLATION_MESSAGE_SHAREPOINTDEPENDENCYERROR);
            //}
            //else if (AveEnv.IsNonSPInstalled)
            //{
            //    data.WelcomeViewModel.Data.IsShowInstall = Visibility.Collapsed;
            //    data.WelcomeViewModel.Data.InstallMode = Contract.InstallationModel.GeneratePackage;
            //}
            var message = GUIRESX.APPINSTALLATION_MESSAGE_SINGLEINSTANCEERROR;
            var summary = GUIRESX.ConfigurationTool_Key_GUI_App_Title;
            CommonCheckSystemInfo.VerifySingleInstance(message, summary);
            //logger.Info(LogEntity.LogResource.GetString("CommonInstallation_Key_Log_Info_InstallationStarted"));
            logger.Info("Agent Configuration Tool started");
            //thisSettingInfo = settingInfoHandler.GetSettingInfoHandler();
            //instanceInstallationInfo.CabFilePath = e.Args[0];
            //IInitializationManager secondaryInitialization = new BaseSecondaryInitialization();
            //secondaryInitialization.InstallInit = new InstallSecondaryInit(data);
            //secondaryInitialization.UpgradeInit = new UpgradeSecondaryInit(data);
            //secondaryInitialization.GeneratePackageInit = new GeneratePackageSecondaryInit(data);
            //data.WelcomeViewModel.Data.IsShowUpgrade = Visibility.Collapsed;
            //var ini = new InstallInit(data, secondaryInitialization);
            //ini.Init();
            //logger.Info(LogEntity.LogResource.GetString("CommonInstallation_Key_Log_Info_InitSetupInfoEnd"));
        }

        internal static bool IsAdmin()
        {
            var id = WindowsIdentity.GetCurrent();
            var p = new WindowsPrincipal(id);
            return p.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void InitServicePoint()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
        }
    }
}