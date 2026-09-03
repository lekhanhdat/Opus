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
using AvePoint.Common;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveIisWebServiceApplicationPool : AvePersistedObject, IAveIisWebServiceApplicationPool
    {
        private SPIisWebServiceApplicationPool mIisWebServiceApplicationPool;
        private AveManagedAccount mManagedAccount;
        private AveProcessAccount mProcessAccount;

        public AveIisWebServiceApplicationPool(SPPersistedObject iisWebServiceApplicationPool)
            : base(iisWebServiceApplicationPool)
        {
            mIisWebServiceApplicationPool = (SPIisWebServiceApplicationPool)iisWebServiceApplicationPool;
        }

        internal SPIisWebServiceApplicationPool IisWebServiceApplicationPool
        {
            get
            {
                return mIisWebServiceApplicationPool;
            }
        }

        #region IAveIisWebServiceApplicationPool Members

        public AveIdentityType CurrentIdentityType
        {
            get
            {
                return (AveIdentityType)AveAssemblyUtility.GetPropertyValue(mIisWebServiceApplicationPool, "CurrentIdentityType");
            }
        }

        public IAveManagedAccount ManagedAccount
        {
            get
            {
                SPManagedAccount managedAccount = (SPManagedAccount)AveAssemblyUtility.GetPropertyValue(mIisWebServiceApplicationPool, "ManagedAccount");
                if (managedAccount == null)
                {
                    return null;
                }
                if (mManagedAccount == null)
                {
                    mManagedAccount = new AveManagedAccount(managedAccount);
                }
                return mManagedAccount;
            }
        }

        public IAveProcessAccount ProcessAccount
        {
            get
            {
                if (mProcessAccount == null)
                {
                    mProcessAccount = new AveProcessAccount(mIisWebServiceApplicationPool.ProcessAccount);
                }
                return mProcessAccount;
            }
            set
            {
                mProcessAccount = value as AveProcessAccount;
                if (mProcessAccount != null)
                {
                    mIisWebServiceApplicationPool.ProcessAccount = mProcessAccount.ProcessAccount;
                }
                else
                {
                    mIisWebServiceApplicationPool.ProcessAccount = null;
                }
            }
        }

        public override void Update()
        {
            mIisWebServiceApplicationPool.Update();
        }

        public string IisObjectName
        {
            get
            {
                return (string)AveAssemblyUtility.GetPropertyValue(mIisWebServiceApplicationPool, "IisObjectName");
            }
        }

        public void BeginProvision(AveIisWebServiceApplicationPoolProvisioningOptions options)
        {
            AveAssemblyUtility.InvokeMethod(mIisWebServiceApplicationPool, "BeginProvision", (int)options);
        }

        #endregion
    }
}
