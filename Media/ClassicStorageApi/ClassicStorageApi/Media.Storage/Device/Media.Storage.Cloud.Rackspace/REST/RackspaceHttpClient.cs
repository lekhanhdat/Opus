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




namespace AvePoint.Media.ClassicStorage.Cloud.Rackspace.REST
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.Media.ClassicStorage.Cloud.Common.HttpHelper;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using AvePoint.Media.ClassicStorage.Cloud.Common.Request;
    using System.Net.Security;
    using AvePoint.GCommon.Utility;
    #endregion

    class RackspaceHttpClient : AbstractHttpClient
    {
        public override HttpWebRequest GetHttpWebRequest(BasicRequest request)
        {
            //ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult); //解决 peer not authenticated
            RackspaceHttpRequest rackSpaceRequest = request as RackspaceHttpRequest;
            HttpWebRequest webRequest = WebRequest.Create(SecurityUtils.SanitizeRequestUrl(rackSpaceRequest.URI)) as HttpWebRequest;
            webRequest.Method = request.Method;
            AddHeaders(webRequest, rackSpaceRequest.Headers);
            return webRequest;
        }

        public override void CombiningRequestWithHeaders(HttpWebRequest request, Dictionary<string, string> headers)
        {
            base.CombiningRequestWithHeaders(request, headers);
            request.AllowWriteStreamBuffering = false;
            request.AllowAutoRedirect = false;
            request.Timeout = 0x7ffffffe; //never timeout
        }

        /*
         *只要是为了解决peer not authenticated error， 一般不会用到
         */
        #region 废弃方法
        [Obsolete("Not used any more", true)]
        public bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {   // 总是接受  
            return true;
        }
        #endregion
    }
}
