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
    public class AgentServiceAccountData : NotifyPropertyChanged
    {
        private static readonly AgentServiceAccountData thisInstance = new AgentServiceAccountData();

        private string accountName = string.Empty;

        private string accountNameMsg = string.Empty;

        private string accountPassword = string.Empty;

        private string accountPasswordMsg = string.Empty;

        private Visibility accountPasswordVis = Visibility.Collapsed;
        private bool isConfiguring = false;

        public bool IsConfiguring
        {
            get { return isConfiguring; }
            set
            {
                isConfiguring = value;
                OnPropertyChanged("IsConfiguring");
            }
        }

        public string AccountName
        {
            get { return accountName; }
            set
            {
                accountName = value;
                OnPropertyChanged("AccountName");
            }
        }

        public bool IsInitialed { get; set; }

        public string AccountNameMsg
        {
            get { return accountNameMsg; }
            set
            {
                accountNameMsg = value;
                OnPropertyChanged("AccountNameMsg");
            }
        }

        public string AccountPassword
        {
            get { return accountPassword; }
            set
            {
                accountPassword = value;
                OnPropertyChanged("AccountPassword");
            }
        }
        public string AccountPasswordMsg
        {
            get { return accountPasswordMsg; }
            set
            {
                accountPasswordMsg = value;
                OnPropertyChanged("AccountPasswordMsg");
            }
        }
        public Visibility AccountPasswordVis
        {
            get { return accountPasswordVis; }
            set
            {
                accountPasswordVis = value;
                OnPropertyChanged("AccountPasswordVis");
            }
        }


        public static AgentServiceAccountData GetInstance()
        {
            return thisInstance;
        }

        public void Reset()
        {
            AccountNameMsg = string.Empty; 
            AccountPasswordMsg = string.Empty;
            AccountPasswordVis = Visibility.Collapsed;
        }

    }
}