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
namespace Microsoft365.SharePoint.CSOM
{
    using System;
    using Microsoft.ProjectServer.Client;
    using Microsoft.SharePoint.Client;

    public class RetryableProjectClientContext : ProjectContext
    {
        internal Action<ClientContext> RefreshTokenAction { get; set; }

        protected int RETRYCOUNT { get; set; }
        protected int RETRYINTERVAL { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webFullUrl"></param>
        /// <param name="retryCount">every request retry times,default is 3</param>
        /// <param name="retryInterval">this is a base retry wait interval,server exception will wait for this interval,WebException will wait double times.Default is 5s</param>
        public RetryableProjectClientContext(string webFullUrl, int retryCount, int retryInterval, bool useHttpClient = true)
            : base(webFullUrl)
        {
            if (useHttpClient)
            {
                WebRequestExecutorFactory = new SharePointHttpClientWebRequestExecutorFactory();
            }
            RETRYCOUNT = retryCount;
            RETRYINTERVAL = retryInterval;
        }

        public void SetRefreshToken(Action<ClientContext> refreshToken)
        {
            this.RefreshTokenAction = refreshToken;
        }    
    }
}