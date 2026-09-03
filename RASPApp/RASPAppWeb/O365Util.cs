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
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Util;
using Newtonsoft.Json.Linq;
using System;

namespace RASPAppWeb
{
    public class O365Util
    {
        public static string GetO365TenantId(string o365DomainName)
        {
            Func<string> getObj = () =>
            {
                var wellKnownUrl = string.Format("https://login.windows.net/{0}.onmicrosoft.com/.well-known/openid-configuration", o365DomainName);
                var response = HttpHelper.HttpGet(null, wellKnownUrl);

                if (!string.IsNullOrEmpty(response))
                {
                    JToken token = JToken.Parse(response);
                    var authorizationEndpoint = token["authorization_endpoint"].ToString();
                    var uri = new Uri(authorizationEndpoint);
                    string tenantId = uri.Segments[1].TrimEnd('/');
                    return tenantId;
                }
                else
                {
                    return null;
                }
            };
            return CacheService.Get("O365Id", o365DomainName, getObj);
        }
    }
}