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


namespace AveClientRequest.Common
{
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Net;
    using AvePoint.Wrapper.Common;
    using AvePoint.Office365.Api;

    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public class AveSoapHttpClientProtocol : SoapHttpClientProtocol
    {
        public ITokenProvider TokenProvider { get; set; }

        public object Credentials { get; set; }

        protected override System.Net.WebRequest GetWebRequest(System.Uri uri)
        {
            var webrequest = new ReconnectableHttpWebRequest(base.GetWebRequest(uri) as HttpWebRequest);
            if (TokenProvider != null)
            {
                webrequest.SetTokenProvider(uri.OriginalString, TokenProvider, false);
            }
            else if (Credentials != null) // For local windowns auth.
            {
                if (Credentials is CookieContainer)
                {
                    webrequest.CookieContainer = Credentials as CookieContainer;
                }
                else if (Credentials is NetworkCredential)
                {
                    webrequest.Credentials = Credentials as NetworkCredential;
                }
            }
            webrequest.Headers["X-FORMS_BASED_AUTH_ACCEPTED"] = "f";
            return webrequest;
        }
    }
}

