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

namespace AvePoint.Deployment.CommonGUI
{
    #region ---namespace---

    using System.ComponentModel;

    #endregion

    public enum SystemInfoCheckStatus
    {
        Waiting,
        Mismatch,
        Warning,
        Passed
    }

    public class ScanningRuleItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region

        #region content: displaying item name

        private string content;

        public string Content
        {
            get { return content; }
            set
            {
                content = value;
                OnPropertyChanged("Content");
            }
        }

        #endregion

        #region icon: icon image source

        private string icon;

        public string Icon
        {
            get { return icon; }
            set
            {
                icon = value;
                OnPropertyChanged("Icon");
            }
        }

        #endregion

        #region status: displaying systeminfo status

        private string status;

        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged("Status");
            }
        }

        #endregion

        #region isChecked: whether or not be checked

        private bool isChecked;

        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }

        #endregion

        #region isVisibility: whether or not be display

        private bool isVisibility;

        public bool IsVisibility
        {
            get { return isVisibility; }
            set
            {
                isVisibility = value;
                OnPropertyChanged("IsVisibility");
            }
        }

        #endregion

        #region checkState: checked item's state

        private SystemInfoCheckStatus checkState;

        public SystemInfoCheckStatus CheckState
        {
            get { return checkState; }
            set
            {
                checkState = value;
                OnPropertyChanged("CheckState");
            }
        }

        #endregion

        #region rightStatusFailed

        public string RightStatusFailedContent { get; set; }

        #endregion

        #region rightStatusWarning

        public string RightStatusWarningContent { get; set; }

        #endregion

        #region rightStatusPassed

        public string RightStatusPassedContent { get; set; }

        #endregion

        #endregion
    }
}