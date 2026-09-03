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



using System;
using System.Security;
using System.Collections.Specialized;
using System.Security.Principal;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using AvePoint.Common;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveManagedAccount : AvePersistedObject, IAveManagedAccount
    {
        private const string mManagedAccount_ComponentsUsingThisAccount_Member = "ComponentsUsingThisAccount";
        private const string mManagedAccount_Domain_Member = "Domain";
        private const string mManagedAccount_NextChangeTimeHelper_Member = "NextChangeTimeHelper";
        private const string mManagedAccount_PasswordChangeJobName_Member = "PasswordChangeJobName";
        private const string mManagedAccount_PasswordChangeJob_Member = "PasswordChangeJob";
        private const string mManagedAccount_SplitName_Member = "SplitName";
        private const string mManagedAccount_SplitServer_Member = "SplitServer";
        private const string mManagedAccount_TimeToNotifyAboutChange_Member = "TimeToNotifyAboutChange";
        private const string mManagedAccount_TimeToNotifyAboutExpiry_Member = "TimeToNotifyAboutExpiry";
        private const string mManagedAccount_UPNName_Member = "UPNName";
        private const string mManagedAccount_UserAccountControl_Member = "UserAccountControl";
        private SPManagedAccount mManagedAccount;
        private AveSchedule mChangeSchedule;
        private AveGeneratePasswordJobDefinition mPasswordChangeJob;

        public AveManagedAccount(SPPersistedObject managedAccount)
            : base(managedAccount)
        {
            mManagedAccount = (SPManagedAccount)managedAccount;
        }

        public AveManagedAccount(object managedAccount)
            : base(managedAccount)
        {
            mManagedAccount = (SPManagedAccount)managedAccount;
        }

        internal SPManagedAccount ManagedAccount
        {
            get
            {
                return mManagedAccount;
            }
        }

        #region IAveManagedAccount Members

        public bool AutomaticChange
        {
            get
            {
                return mManagedAccount.AutomaticChange;
            }
            set
            {
                mManagedAccount.AutomaticChange = value;
            }
        }

        public bool CanChangePassword
        {
            get { return mManagedAccount.CanChangePassword; }
        }

        public IAveSchedule ChangeSchedule
        {
            get
            {
                if (mChangeSchedule == null)
                {
                    SPSchedule schedule = mManagedAccount.ChangeSchedule;
                    if (schedule != null)
                    {
                        mChangeSchedule = AveSchedule.InitSchedule(schedule);
                    }
                }
                return mChangeSchedule;
            }
            set
            {
                mChangeSchedule = value as AveSchedule;
                if (mChangeSchedule != null)
                {
                    mManagedAccount.ChangeSchedule = mChangeSchedule.Schedule;
                }
                else
                {
                    mManagedAccount.ChangeSchedule = null;
                }
            }
        }

        public StringCollection ComponentsUsingThisAccount
        {
            get
            {
                return (StringCollection)AveAssemblyUtility.GetPropertyValue(mManagedAccount, mManagedAccount_ComponentsUsingThisAccount_Member);
            }
        }

        public int DaysBeforeChangeToEmail
        {
            get
            {
                return mManagedAccount.DaysBeforeChangeToEmail;
            }
            set
            {
                mManagedAccount.DaysBeforeChangeToEmail = value;
            }
        }

        public int DaysBeforeExpiryToChange
        {
            get
            {
                return mManagedAccount.DaysBeforeExpiryToChange;
            }
            set
            {
                mManagedAccount.DaysBeforeExpiryToChange = value;
            }
        }

        public string DisplayName
        {
            get { return mManagedAccount.DisplayName; }
        }

        public string Domain
        {
            get { return (string)AveAssemblyUtility.GetPropertyValue(mManagedAccount, mManagedAccount_Domain_Member); }
        }

        public bool EnableEmailBeforePasswordChange
        {
            get
            {
                return mManagedAccount.EnableEmailBeforePasswordChange;
            }
            set
            {
                mManagedAccount.EnableEmailBeforePasswordChange = value;
            }
        }

        public int MinPasswordLen
        {
            get { return mManagedAccount.MinPasswordLen; }
        }

        public IAveGeneratePasswordJobDefinition PasswordChangeJob
        {
            get
            {
                if (mPasswordChangeJob == null)
                {
                    object generatePasswordJobDefinition = AveAssemblyUtility.GetPropertyValue(mManagedAccount, mManagedAccount_PasswordChangeJob_Member);
                    if (generatePasswordJobDefinition != null)
                    {
                        mPasswordChangeJob = new AveGeneratePasswordJobDefinition(generatePasswordJobDefinition);
                    }
                }
                return mPasswordChangeJob;
            }
        }

        public string PasswordChangeJobName
        {
            get { return (string)AveAssemblyUtility.GetPropertyValue(mManagedAccount, mManagedAccount_PasswordChangeJobName_Member); }
        }

        public DateTime PasswordExpiration
        {
            get { return mManagedAccount.PasswordLastChanged; }
        }

        public DateTime PasswordLastChanged
        {
            get { return mManagedAccount.PasswordLastChanged; }
        }

        public SecurityIdentifier Sid
        {
            get
            {
                return mManagedAccount.Sid;
            }
            set
            {
                mManagedAccount.Sid = value;
            }
        }

        public string SplitName
        {
            get { return (string)AveAssemblyUtility.GetPropertyValue(mManagedAccount, mManagedAccount_SplitName_Member); }
        }

        public string SplitServer
        {
            get { return (string)AveAssemblyUtility.GetPropertyValue(mManagedAccount, mManagedAccount_SplitServer_Member); }
        }

        public bool TimeToNotifyAboutChange
        {
            get { return (bool)AveAssemblyUtility.GetPropertyValue(mManagedAccount, mManagedAccount_TimeToNotifyAboutChange_Member); }
        }

        public bool TimeToNotifyAboutExpiry
        {
            get { return (bool)AveAssemblyUtility.GetPropertyValue(mManagedAccount, mManagedAccount_TimeToNotifyAboutExpiry_Member); }
        }

        public string TypeName
        {
            get { return mManagedAccount.TypeName; }
        }

        public string UPNName
        {
            get { return (string)AveAssemblyUtility.GetPropertyValue(mManagedAccount, mManagedAccount_UPNName_Member); }
        }

        public int UserAccountControl
        {
            get { return (int)AveAssemblyUtility.GetPropertyValue(mManagedAccount, mManagedAccount_UserAccountControl_Member); }
        }

        public string Username
        {
            get
            {
                return mManagedAccount.Username;
            }
            set
            {
                mManagedAccount.Username = value;
            }
        }

        public DateTime NextChangeTime
        {
            get { return (DateTime)AveAssemblyUtility.InvokeMethod(mManagedAccount, mManagedAccount_NextChangeTimeHelper_Member, new Type[] { typeof(DateTime) }, new object[] { DateTime.Now }); }
        }

        public void ChangePassword(SecureString newPassword, AveEventProcessingOptions eventFlags)
        {
            mManagedAccount.ChangePassword(newPassword, (SPManagedAccount.EventProcessingOptions)eventFlags);
        }

        public void Update()
        {
            mManagedAccount.Update();
        }

        public bool SetPassword(SecureString value)
        {
            return mManagedAccount.SetPassword(value);
        }

        public void GeneratePassword(AveEventProcessingOptions eventFlags)
        {
            mManagedAccount.GeneratePassword((SPManagedAccount.EventProcessingOptions)(eventFlags));
        }

        public void PropagatePassword(SecureString newPassword, AveEventProcessingOptions eventFlags)
        {
            mManagedAccount.PropagatePassword(newPassword, (SPManagedAccount.EventProcessingOptions)(eventFlags));
        }

        public void Delete()
        {
            mManagedAccount.Delete();
        }

        #endregion
    }
}
