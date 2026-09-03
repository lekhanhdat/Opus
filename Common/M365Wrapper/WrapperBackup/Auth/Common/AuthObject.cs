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

namespace ExchangeUtility.Graph
{
    using Microsoft.Exchange.WebServices.Data;
    using System;
    using Util.MSAzure;
    using ExchangeUtility.Graph;

    public abstract class AuthObject : IEWSAuthObject, IDisposable
    {
        public string UserName { get; private set; }
        public abstract AuthObjectType AuthType { get; }

        public string EWSServiceUrl { get; set; }

        public string ImpersonateUser => ImpersonateUserInfo?.GetImpersonateUser();

        public ImpersonateUserInfo ImpersonateUserInfo { get; private set; }

        public AuthObject(string username, string serviceUrl, ImpersonateUserInfo impersonateUserInfo = null)
        {
            UserName = username;
            EWSServiceUrl = serviceUrl;
            ImpersonateUserInfo = impersonateUserInfo;
        }

        public string DomainName
        {
            get
            {
                if (!string.IsNullOrEmpty(this.UserName))
                {
                    int index = this.UserName.LastIndexOf('@');
                    if (index <= 0) throw new System.InvalidCastException(string.Format("Cannot get domain name from user name, user name: {0}", this.UserName));
                    return this.UserName.Substring(index + 1);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public AzureEnvironment Environment { get; set; }

        public abstract void BindToExchangeService(ExchangeService service);

        public abstract void BindToPOXAutoDiscoverService(POXAutodiscoverService poxAutodiscoverService);

        public virtual void SetImpersonatedUserId(ExchangeService service, string impersonatedUserAddress)
        {
            service.ImpersonatedUserId = new ImpersonatedUserId(ConnectingIdType.SmtpAddress, impersonatedUserAddress);
        }

        public virtual void RemoveImpersonatedUserId(ExchangeService service)
        {
            service.ImpersonatedUserId = null;
        }

        public virtual void AddImpersonationHeader(ExchangeService service, string mailbox)
        {
            service.HttpHeaders[ExchangeConstants.IMPERSONATION_HEADER_NAME] = mailbox;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~AuthObject() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }



    public enum AuthObjectType : byte
    {
        None = 0,
        UserPassword = 1,
        AccessToken = 2,
        PasswordAccessToken = 3,
        YammerAppToken = 4,
    }
}