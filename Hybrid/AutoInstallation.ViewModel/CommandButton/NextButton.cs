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


using System.Windows;
using System.Windows.Input;
using AutoInstallation.Contract;
using AutoInstallation.Contract.Interface.Control;
using COMMONRESX = AutoInstallation.Records.App.Resources.Resource;

namespace AutoInstallation.ViewModel.CommandButton
{
    public class NextButton : NotifyPropertyChanged, IButton
    {
        private string content = " " /*+ COMMONRESX.COMMON_BTN_NEXT*/;
        private bool enabled = true;

        public string Content
        {
            get { return content; }
            set
            {
                content = value;
                OnPropertyChanged("Content");
            }
        }

        public ICommand Command { get; set; }

        public Visibility Vis { get; set; } = Visibility.Visible;

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                OnPropertyChanged("Enabled");
            }
        }
    }
}