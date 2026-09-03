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
namespace Microsoft365.Common.Http
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Cache;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;

    public interface IHttpWebRequest
    {
        HttpWebRequest WebRequest { get; }
        Stream GetRequestStream();

        WebResponse GetResponse();

        IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state);

        Stream EndGetRequestStream(IAsyncResult asyncResult);

        IAsyncResult BeginGetResponse(AsyncCallback callback, object state);

        WebResponse EndGetResponse(IAsyncResult asyncResult);

        CookieContainer CookieContainer { get; set; }

        int ReadWriteTimeout { get; set; }

        RequestCachePolicy CachePolicy { get; set; }

        string ConnectionGroupName { get; set; }

        long ContentLength { get; set; }

        string ContentType { get; set; }

        ICredentials Credentials { get; set; }

        WebHeaderCollection Headers { get; set; }

        string Method { get; set; }

        bool PreAuthenticate { get; set; }

        bool KeepAlive { get; set; }

        IWebProxy Proxy { get; set; }

        Uri RequestUri { get; }

        int Timeout { get; set; }

        bool UseDefaultCredentials { get; set; }

        string UserAgent { get; set; }

        bool AllowAutoRedirect { get; set; }

        string Accept { get; set; }

        X509CertificateCollection ClientCertificates { get; set; }

        AuthenticationLevel AuthenticationLevel { get; set; }

        string Host { get; set; }

        DecompressionMethods AutomaticDecompression { get; set; }
    }
}