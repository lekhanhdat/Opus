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



using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server19
{
    class AveProcessIdentity : AvePersistedObject, IAveProcessIdentity
    {
        private SPProcessIdentity mProcessIdentity;

        public AveProcessIdentity(SPProcessIdentity processIdEntity)
            : base(processIdEntity)
        {
            mProcessIdentity = processIdEntity;
        }

        #region IAveProcessIdentity Members

        public bool IsCredentialDeploymentEnabled
        {
            get
            {
                return mProcessIdentity.IsCredentialDeploymentEnabled;
            }
            set
            {
                mProcessIdentity.IsCredentialDeploymentEnabled = value;
            }
        }

        public bool IsCredentialUpdateEnabled
        {
            get
            {
                return mProcessIdentity.IsCredentialUpdateEnabled;
            }
            set
            {
                mProcessIdentity.IsCredentialUpdateEnabled = value;
            }
        }

        public string Username
        {
            get
            {
                return mProcessIdentity.Username;
            }
            set
            {
                mProcessIdentity.Username = value;
            }
        }

        public AveIdentityType CurrentIdentityType
        {
            get
            {
                return (AveIdentityType)mProcessIdentity.CurrentIdentityType;
            }
            set
            {
                mProcessIdentity.CurrentIdentityType = (IdentityType)value;
            }
        }

        public IAveManagedAccount ManagedAccount
        {
            get
            {
                return (mProcessIdentity.ManagedAccount == null) ? null : new AveManagedAccount(mProcessIdentity.ManagedAccount);
            }
            set
            {
                AveManagedAccount account = value as AveManagedAccount;
                mProcessIdentity.ManagedAccount = (account == null) ? null : account.ManagedAccount;
            }
        }

        public override void Update()
        {
            mProcessIdentity.Update();
        }

        public void Deploy()
        {
            mProcessIdentity.Deploy();
        }

        #endregion
    }
}
