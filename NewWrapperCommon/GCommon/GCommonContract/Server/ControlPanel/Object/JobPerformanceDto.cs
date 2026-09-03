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

using System.ComponentModel;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.ExportLocation.Object;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.DataManager.Object;
using System;
using System.Text;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobPerformanceDto : INotifyPropertyChanged
    {
        private bool mIsConfigured;
        [DataMember]
        public bool IsConfigured
        {
            get
            {
                return mIsConfigured;
            }
            set
            {
                mIsConfigured = value;
                OnPropertyChanged("IsConfigured");
            }
        }

        [DataMember]
        public string ProfileId { get; set; }

        private string mProfileName;
        [DataMember]
        public string ProfileName
        {
            get
            {
                return mProfileName;
            }
            set
            {
                mProfileName = value;
                OnPropertyChanged("ProfileName");
            }
        }

        [DataMember]
        public PlanCategory PlanCategory { get; set; }

        private bool mIsChecked;
        [DataMember]
        public bool IsChecked
        {
            get { return mIsChecked; }
            set
            {
                mIsChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }

        private long mPendingTime;
        [DataMember]
        public long PendingTime
        {
            get
            {
                return mPendingTime;
            }
            set
            {
                mPendingTime = value;
                OnPropertyChanged("PendingTime");
            }
        }

        [DataMember]
        public bool Retry { get; set; }

        private bool mCollectLog;
        [DataMember]
        public bool CollectLog
        {
            get
            {
                return mCollectLog;
            }
            set
            {
                mCollectLog = value;
                OnPropertyChanged("CollectLog");
            }
        }

        [DataMember]
        public ExportLocationDto ExportLocation { get; set; }

        public ModuleGroupType Group { get; set; }

        private bool autoFailedJob;

        [DataMember]
        public bool AutoFailedJob
        {
            get { return autoFailedJob; }
            set
            {
                autoFailedJob = value;
                OnPropertyChanged("AutoFailedJob");
            }
        }

        private bool noActions;
        public bool NoActions
        {
            get { return noActions; }
            set
            {
                noActions = value;
                OnPropertyChanged("NoActions");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("Unresponsive for {0} Minutes", new TimeSpan(this.PendingTime).TotalMinutes);

            if (this.AutoFailedJob)
            {
                builder.Append(", Automatically fail");
            }
            if (this.CollectLog)
            {
                builder.Append(", Collect Logs");
            }
            if (!string.IsNullOrEmpty(this.ProfileName))
            {
                builder.AppendFormat(", Send E-mail to {0}", this.ProfileName);
            }
            return builder.ToString();
        }
    }
}
