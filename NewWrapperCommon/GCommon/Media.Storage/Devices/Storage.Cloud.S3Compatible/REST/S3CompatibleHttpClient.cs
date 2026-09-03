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


namespace AvePoint.Media.Storage.S3Compatible.REST
{
    #region using directives
    using AvePoint.Media.Storage.Cloud.Common;
    using AvePoint.Media.Storage.Cloud.S3Compatible;
    using System;
    using System.Collections.Generic;
    using System.Net;
    #endregion
    class S3CompatibleHttpClient : AbstractHttpClient
    {
        public override HttpWebRequest GetHttpWebRequest(BasicRequest request)
        {
            S3CompatibleRequest s3CompatibleRequest = request as S3CompatibleRequest;
            HttpWebRequest webRequest = WebRequest.Create(s3CompatibleRequest.URI) as HttpWebRequest;
            webRequest.Method = s3CompatibleRequest.Method;
            AddHeaders(webRequest, s3CompatibleRequest.Headers);
            S3CompatibleOpenParameter s3CompatibleOpenParameter = OpenParam as S3CompatibleOpenParameter;
            S3CompatibleUtils.AddAuthorization(webRequest, s3CompatibleRequest.UserName, s3CompatibleRequest.Password);
            return webRequest;
        }

        public override void CombiningRequestWithHeaders(HttpWebRequest request, Dictionary<String, String> headers)
        {
            base.CombiningRequestWithHeaders(request, headers);
            S3CompatibleOpenParameter s3CompatibleOpenParameter = OpenParam as S3CompatibleOpenParameter;
            S3CompatibleUtils.AddAuthorization(request, s3CompatibleOpenParameter.UserName, s3CompatibleOpenParameter.Password);
            request.AllowWriteStreamBuffering = true;
            request.AllowAutoRedirect = true;
            request.Timeout = S3CompatibleConstants.DefaultHttpRequestTimeOut;
        }
    }
}
