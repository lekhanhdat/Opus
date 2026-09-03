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



using System.Security.Principal;
using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server16
{
    public class AveProcessAccount : IAveProcessAccount
    {
        private SPProcessAccount mProcessAccount;
        private AveProcessAccount mNetWorkService;
        private AveProcessAccount mLocalService;
        private AveProcessAccount mLocalSystem;

        public AveProcessAccount(SPProcessAccount processAccount)
        {
            mProcessAccount = processAccount;
        }

        public AveProcessAccount()
        { }

        internal SPProcessAccount ProcessAccount
        {
            get
            {
                return mProcessAccount;
            }
        }

        public IAveProcessAccount NetworkService
        {
            get
            {
                if (mNetWorkService == null)
                {
                    SPProcessAccount processAccount = SPProcessAccount.NetworkService;
                    if (processAccount != null)
                    {
                        mNetWorkService = new AveProcessAccount(processAccount);
                    }
                }
                return mNetWorkService;
            }
        }

        public IAveProcessAccount LocalService
        {
            get
            {
                if (mLocalService == null)
                {
                    SPProcessAccount processAccount = SPProcessAccount.LocalService;
                    if (processAccount != null)
                    {
                        mLocalService = new AveProcessAccount(processAccount);
                    }
                }
                return mLocalService;
            }
        }

        public IAveProcessAccount LocalSystem
        {
            get
            {
                if (mLocalSystem == null)
                {
                    SPProcessAccount processAccount = SPProcessAccount.LocalSystem;
                    if (processAccount != null)
                    {
                        mLocalSystem = new AveProcessAccount(processAccount);
                    }
                }
                return mLocalSystem;
            }
        }

        public IAveProcessAccount LookupManagedAccount(SecurityIdentifier sid)
        {
            SPProcessAccount processAccount = SPProcessAccount.LookupManagedAccount(sid);
            if (processAccount == null)
            {
                return null;
            }
            return new AveProcessAccount(processAccount);
        }
        
        public string LookupName()
        {
            return mProcessAccount.LookupName();
        }
    }
}
