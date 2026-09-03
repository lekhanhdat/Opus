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
namespace Microsoft365.Common.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Logger;
    [Obsolete]
    public class RetryHandler : DelegatingHandler
    {
        protected IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(RetryHandler));
        protected int MaxRetries { get; set; }

        public RetryHandler(HttpMessageHandler innerHandler,int maxRetries)
            : base(innerHandler)
        {
            MaxRetries= maxRetries;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            Exception error = null;
            for (int i = 0; i < MaxRetries; i++)
            {
                try
                {
                    response = await base.SendAsync(request, cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        return response;
                    }
                }
                catch (Exception ex)
                {
                    error = ex;
                    logger.Warn($"Execute request failed.Error:{ex}");
                }
            }
            if (error != null)
            {
                throw error;
            }
            return response;
        }
    }

}
