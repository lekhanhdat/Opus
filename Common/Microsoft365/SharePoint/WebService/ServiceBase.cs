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
namespace Microsoft365.SharePoint.WebService
{
    using Microsoft365.Common.SoapClient;
    using System;
    public interface ISharePointWebService
    {
        
    }
    public abstract class ServiceBase: ISharePointWebService,IDisposable
    {
        protected virtual SoapHttpClient SoapClient { get; set; }
        protected abstract string ServiceEndPoint { get; }
        protected virtual Uri ServiceUrl { get; set; }
        protected virtual Func<string> CookieProvider { get; set; }
        public ServiceBase(string webUrl, Func<string> cookieProvider)
        {
            ServiceUrl = new Uri(webUrl.TrimEnd('/') + ServiceEndPoint);
            CookieProvider = cookieProvider;
            SoapClient = new SoapHttpClient(ServiceUrl,CookieProvider);
        }

        public void Dispose()
        {
            CookieProvider = null;
            ServiceUrl = null;
            SoapClient = null;
        }
    }
}