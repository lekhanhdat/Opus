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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
namespace AvePoint.ObjectModel.WebService
{
    class AveAuthentication : IDisposable
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveAuthentication));
        private readonly string mUrlPostfix = "/_vti_bin/authentication.asmx";
        private AveBPOSAccountInfo mAccount;
        private Authentication.Authentication mAuth;
        public AveAuthentication(string url, AveBPOSAccountInfo userAccount)
        {
            mAuth = new Authentication.Authentication();
            Uri uri = new Uri(url.TrimEnd('/'));
            mAuth.Url = uri.AbsoluteUri + mUrlPostfix;
            mAccount = userAccount;
            //mAuth.Timeout = ;
            mAuth.Credentials = new NetworkCredential(userAccount.UserName, userAccount.Password, userAccount.Domain);
        }

        public object Authentic()
        {
            try
            {
                Authentication.AuthenticationMode mode = this.mAuth.Mode();
                if ( mode == Authentication.AuthenticationMode.Windows )
                {
                    return mAuth.Credentials;
                } 
                else
                {
                    this.mAuth.CookieContainer = new CookieContainer();
                    this.mAuth.AllowAutoRedirect = true;
                    Authentication.LoginResult result = this.mAuth.Login(string.Format("{1}", this.mAccount.Domain, this.mAccount.UserName), this.mAccount.Password);
                    if (result.ErrorCode == Authentication.LoginErrorCode.NoError)
                    {
                        return this.mAuth.CookieContainer;
                    }
                    else
                    {
                        throw new Exception("Form UnAuthentic");
                    }
                }
            }
            catch(Exception ex)
            {
                //form authentic
                mLogger.Warn("Form UnAuthentic.Domain:{0},Username:{1}.Error Message:{3}",this.mAccount.Domain,this.mAccount.UserName,ex.ToString());
                this.mAuth.CookieContainer = new CookieContainer();
                this.mAuth.AllowAutoRedirect = true;
                Authentication.LoginResult result = this.mAuth.Login(string.Format("{1}", this.mAccount.Domain, this.mAccount.UserName), this.mAccount.Password);
                if (result.ErrorCode == Authentication.LoginErrorCode.NoError)
                {
                    return this.mAuth.CookieContainer;
                }
                else
                {
                    throw new Exception("Form UnAuthentic");
                }
            }
        }
        public void Dispose()
        {
            mAuth.Dispose();
            mAuth = null;
        }
    }
}
