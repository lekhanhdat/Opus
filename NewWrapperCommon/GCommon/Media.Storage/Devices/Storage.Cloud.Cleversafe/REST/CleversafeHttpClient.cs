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
namespace AvePoint.Media.Storage.Cloud.Cleversafe
{
    #region using directives
    using AvePoint.Media.Storage.Cloud.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    #endregion
    class CleversafeHttpClient : AbstractHttpClient
    {

        public override HttpWebRequest GetHttpWebRequest(BasicRequest request)
        {
            CleversafeRequest cleversafeRequest = request as CleversafeRequest;
            HttpWebRequest webRequest = WebRequest.Create(cleversafeRequest.URI) as HttpWebRequest;
            webRequest.Method = cleversafeRequest.Method;
            AddHeaders(webRequest, cleversafeRequest.Headers);
            CleversafeOpenParameter cleversafeOpenParameter = OpenParam as CleversafeOpenParameter;
            CleversafeUtils.AddAuthorization(webRequest, cleversafeRequest.UserName, cleversafeRequest.Password);
            return webRequest;
        }

        public void CombiningRequestWithHeaders(HttpWebRequest request, Dictionary<String, String> headers)
        {
            base.CombiningRequestWithHeaders(request, headers);
            CleversafeOpenParameter cleversafeOpenParameter = OpenParam as CleversafeOpenParameter;
            CleversafeUtils.AddAuthorization(request, cleversafeOpenParameter.UserName, cleversafeOpenParameter.Password);
            request.AllowWriteStreamBuffering = true;
            request.AllowAutoRedirect = true;
            request.Timeout = 0x7ffffffe;
        }
    }
}
