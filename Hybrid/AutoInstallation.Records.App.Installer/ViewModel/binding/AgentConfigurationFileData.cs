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


using AutoInstallation.Contract;

namespace AutoInstallation.Records.App.Installation.ViewModel.binding
{
    public class ConfigurationFileData : NotifyPropertyChanged
    {
        private static readonly ConfigurationFileData thisInstance = new ConfigurationFileData();

        private string configFilePath = string.Empty;

        private string configFilePathMsg = string.Empty;

        private string installationCode = string.Empty;

        private string installationCodeMsg = string.Empty;
        private bool isUsingExistingConfig = false;

        private SelectConfigurationFileOperator selectConfigurationFilePathOperatorin;
        public bool IsInitialed { get; set; }

        private bool isConfiguring;

        public bool IsConfiguring
        {
            get { return isConfiguring; }
            set
            {
                isConfiguring = value;
                OnPropertyChanged("IsConfiguring");
            }
        }

        public bool IsUsingExistingConfig
        {
            get { return isUsingExistingConfig; }
            set
            {
                isUsingExistingConfig = value;
                OnPropertyChanged("IsUsingExistingConfig");
            }
        }
        public string ConfigFilePath
        {
            get { return configFilePath; }
            set
            {
                configFilePath = value;
                OnPropertyChanged("ConfigFilePath");
            }
        }
        public string ConfigFilePathMsg
        {
            get { return configFilePathMsg; }
            set
            {
                configFilePathMsg = value;
                OnPropertyChanged("ConfigFilePathMsg");
            }
        }

        public string InstallationCode
        {
            get { return installationCode; }
            set
            {
                installationCode = value;
                OnPropertyChanged("InstallationCode");
            }
        }
        public string InstallationCodeMsg
        {
            get { return installationCodeMsg; }
            set
            {
                installationCodeMsg = value;
                OnPropertyChanged("InstallationCodeMsg");
            }
        }

        public SelectConfigurationFileOperator SelectConfigurationFilePathOperator
        {
            get
            {
                if (selectConfigurationFilePathOperatorin == null) selectConfigurationFilePathOperatorin = new SelectConfigurationFileOperator();
                return selectConfigurationFilePathOperatorin;
            }
            set { selectConfigurationFilePathOperatorin = value; }
        }

        public static ConfigurationFileData GetInstance()
        {
            return thisInstance;
        }

        public void Reset()
        {
            ConfigFilePathMsg = string.Empty; //import path error message
            InstallationCodeMsg = string.Empty;
        }

    }
}