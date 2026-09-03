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
using System.Windows;

namespace AutoInstallation.Records.App.Installation.ViewModel.binding
{
    public class ProxySettingData : NotifyPropertyChanged
    {
        private static readonly ProxySettingData thisInstance = new ProxySettingData();

        private bool enableProxy = default;

        private string proxyHost = string.Empty;

        private string proxyHostMsg = string.Empty;

        private string proxyPort = string.Empty;

        private string proxyPortMsg = string.Empty;

        private string userName = string.Empty;

        private string userNameMsg = string.Empty;

        private string password = string.Empty;

        private string errorMsg = string.Empty;

        private Visibility errorMsgVis = Visibility.Collapsed;
        private bool isConfiguring = false;

        public bool IsInitialed { get; set; }

        public bool IsConfiguring
        {
            get { return isConfiguring; }
            set
            {
                isConfiguring = value;
                OnPropertyChanged("IsConfiguring");
            }
        }

        public bool EnableProxy
        {
            get
            {
                return enableProxy;
            }
            set
            {
                enableProxy = value;
                OnPropertyChanged("EnableProxy");
            }
        }

        public string ProxyHost
        {
            get { return proxyHost; }
            set
            {
                proxyHost = value;
                OnPropertyChanged("ProxyHost");
            }
        }
        public string ProxyHostMsg
        {
            get { return proxyHostMsg; }
            set
            {
                proxyHostMsg = value;
                OnPropertyChanged("ProxyHostMsg");
            }
        }

        public string ProxyPort
        {
            get { return proxyPort; }
            set
            {
                proxyPort = value;
                OnPropertyChanged("ProxyPort");
            }
        }
        public string ProxyPortMsg
        {
            get { return proxyPortMsg; }
            set
            {
                proxyPortMsg = value;
                OnPropertyChanged("ProxyPortMsg");
            }
        }

        public string UserName
        {
            get { return userName; }
            set
            {
                userName = value;
                OnPropertyChanged("UserName");
            }
        }
        public string UserNameMsg
        {
            get { return userNameMsg; }
            set
            {
                userNameMsg = value;
                OnPropertyChanged("UserNameMsg");
            }
        }

        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                OnPropertyChanged("Password");
            }
        }
        public string ErrorMsg
        {
            get { return errorMsg; }
            set
            {
                errorMsg = value;
                OnPropertyChanged("ErrorMsg");
            }
        }
        public Visibility ErrorMsgVis
        {
            get { return errorMsgVis; }
            set
            {
                errorMsgVis = value;
                OnPropertyChanged("ErrorMsgVis");
            }
        }


        public static ProxySettingData GetInstance()
        {
            return thisInstance;
        }

        public void Reset()
        {
            ProxyHostMsg = string.Empty;
            ProxyPortMsg = string.Empty;
            UserNameMsg = string.Empty; 
            ErrorMsg = string.Empty;
            ErrorMsgVis = Visibility.Collapsed;
        }

    }
}