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



using System.Security;
using Microsoft.SharePoint.MobileMessage;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server13
{
    class AveMobileMessagingAccount : IAveMobileMessagingAccount
    {
        private SPMobileMessagingAccount mMobileMessagingAccount;

        public AveMobileMessagingAccount()
        {
            mMobileMessagingAccount = new SPMobileMessagingAccount();
        }

        public AveMobileMessagingAccount(SPMobileMessagingAccount mobileMessagingAccount)
        {
            mMobileMessagingAccount = mobileMessagingAccount;
        }

        public AveMobileMessagingAccount(string serviceName, string serviceUrl, string userId, SecureString password)
        {
            mMobileMessagingAccount = new SPMobileMessagingAccount(serviceName, serviceUrl, userId, password);
        }

        public AveMobileMessagingAccount(string serviceName, string serviceUrl, string userId, SecureString password, IAveMobileMessageServiceProvider serviceProvider, IAveMobileMessageUserInfo userInfo)
        {
            mMobileMessagingAccount = new SPMobileMessagingAccount(serviceName, serviceUrl, userId, password, (serviceProvider as AveMobileMessageServiceProvider).MobileMessageServiceProvider, (userInfo as AveMobileMessageUserInfo).MobileMessageUserInfo);
        }

        internal SPMobileMessagingAccount MobileMessagingAccount
        {
            get
            {
                return mMobileMessagingAccount;
            }
        }

        #region IAveMobileMessagingAccount Members

        public bool IsValidAccount()
        {
            return mMobileMessagingAccount.IsValidAccount();
        }

        public string ServiceUrl
        {
            get
            {
                return mMobileMessagingAccount.ServiceUrl;
            }
            set
            {
                mMobileMessagingAccount.ServiceUrl = value;
            }
        }

        public string UserId
        {
            get
            {
                return mMobileMessagingAccount.UserId;
            }
            set
            {
                mMobileMessagingAccount.UserId = value;
            }
        }

        public IAveMobileMessageServiceProvider ServiceProvider
        {
            get
            {
                SPMobileMessageServiceProvider mobileMessageServiceProvider = mMobileMessagingAccount.ServiceProvider;
                if (mobileMessageServiceProvider == null)
                {
                    return null;
                }
                return new AveMobileMessageServiceProvider(mobileMessageServiceProvider);
            }
        }

        public bool UpdateServiceProvider()
        {
            return mMobileMessagingAccount.UpdateServiceProvider();
        }

        public SecureString Password
        {
            get
            {
                return mMobileMessagingAccount.Password;
            }
            set
            {
                mMobileMessagingAccount.Password = value;
            }
        }

        public bool UpdateUserInfo()
        {
            return mMobileMessagingAccount.UpdateUserInfo();
        }

        #endregion
    }
}
