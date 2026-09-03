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

namespace ExchangeUtility
{
    using ExchangeUtility.Graph;
    using Microsoft.Exchange.WebServices.Data;
    using System;
    using GraphUtility = ExchangeUtility.Graph;

    public abstract class AuthObject : GraphUtility.IAuthObject
    {
        public string UserName { get; private set; }
        public abstract AuthObjectType AuthType { get; }

        public string EWSServiceUrl { get; internal set; }

        public AuthObject(string username, string serviceUrl)
        {
            if (username == null) throw new ArgumentNullException("username");
            this.UserName = username;
            this.EWSServiceUrl = serviceUrl;
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

        GraphUtility.AuthObjectType GraphUtility.IAuthObject.AuthType => Enum.Parse<GraphUtility.AuthObjectType>(this.AuthType.ToString());

        public global::Util.MSAzure.AzureEnvironment Environment { get; }

        public abstract void BindToExchangeService(ExchangeService service);

        //public abstract void BindToExchangeServiceBinding(ExchangeServiceBinding serviceBinding, string xAnchorMailbox = null);

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
    }



    public enum AuthObjectType : byte
    {
        None = 0,
        UserPassword = 1,
        AccessToken = 2,
        PasswordAccessToken = 3
    }
}
