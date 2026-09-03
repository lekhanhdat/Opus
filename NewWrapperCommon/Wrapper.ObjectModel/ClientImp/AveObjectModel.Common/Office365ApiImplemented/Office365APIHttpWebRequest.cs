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

namespace AvePoint.ObjectModel.Common
{
    using AvePoint.Office365.Api.Http;
    using AvePoint.Wrapper.Common;
    using System;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    public class Office365APIHttpWebRequest : ReconnectableHttpWebRequest, IHttpWebRequest
    {
        public Office365APIHttpWebRequest(Uri url) : base(url)
        {
        }
        public Office365APIHttpWebRequest(HttpWebRequest request) : base(request)
        {
        }
        new public static Office365APIHttpWebRequest CreateRequest(string url)
        {
            return new Office365APIHttpWebRequest(HttpWebRequest.Create(url) as HttpWebRequest);
        }

        new public static Office365APIHttpWebRequest CreateRequest(Uri uri)
        {
            return new Office365APIHttpWebRequest(HttpWebRequest.Create(uri) as HttpWebRequest);
        }

        new public static Office365APIHttpWebRequest CreateRequest(HttpWebRequest request)
        {
            return new Office365APIHttpWebRequest(request);
        }
        public X509CertificateCollection ClientCertificates { get; set; }
        public string Host { get; set; }
        protected override void RetrieveHost()
        {
            this.Host = mRequest.Host;
        }
        protected override void AssignHost()
        {
            if (string.Compare(mRequest.Host, this.Host, false) != 0)
            {
                mRequest.Host = this.Host;
            }
        }
        protected override void AssignClientCertificates()
        {
            mRequest.ClientCertificates = this.ClientCertificates;
        }
        protected override void RetrieveClientCertificates()
        {
            this.ClientCertificates = mRequest.ClientCertificates;
        }
    }
}
