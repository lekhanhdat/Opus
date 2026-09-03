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
using AvePoint.Office365.Api;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.Common
{
    public class AuthenticationResult
    {
        public AuthenticationResult(AutheStatus status, AveAuthenticationMode autheMode, object credential = null, List<ITokenProvider> tokenProvider = null)
        {
            this.Status = status;
            this.AutheMode = autheMode;
            this.Credential = credential;
            this.tokenProviders = tokenProvider;
        }

        public List<ITokenProvider> tokenProviders { get; }

        public AutheStatus Status { get; private set; }

        public AveAuthenticationMode AutheMode { get; private set; }
        
        public object Credential { get; private set; }

        public override string ToString()
        {
            return string.Format("The auth result: status:{0}, autheMode:{1}", Status, AutheMode);
        }
    }

    public static class AuthenticationResultExtention
    {
        public static void SetCredential2Request(this AuthenticationResult result, string siteUrl, WebRequest request)
        {
            if ((result.AutheMode & AveAuthenticationMode.OnlineAppToken) != 0
                || (result.AutheMode & AveAuthenticationMode.OnlineServiceAccount) != 0)
            {
                ITokenProvider token = null;
                if (result.tokenProviders != null)
                {
                    foreach (var provider in result.tokenProviders)
                    {
                        if (provider.TokenType.Equals(TokenType.Bearer) || token == null)
                        {
                            token = provider;
                        }
                    }
                }
                request.SetTokenProvider(siteUrl, token, false);
            }
            else
            {
                if (result.Credential is CookieContainer)
                {
                    if (request is ReconnectableHttpWebRequest)
                    {
                        (request as ReconnectableHttpWebRequest).CookieContainer = result.Credential as CookieContainer;
                    }
                    else if(request is HttpWebRequest)
                    {
                        (request as HttpWebRequest).CookieContainer = result.Credential as CookieContainer;
                    }
                }
                else
                {
                    request.Credentials = result.Credential as NetworkCredential;
                }
            }
        }
    }
}
