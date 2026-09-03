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

namespace Microsoft365.SharePoint.Rest
{
    using Microsoft365.Common.Logger;
    using Microsoft365.Common.RequestMonitor;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    //用于构建HttpClient,加强rest api调用的鲁棒性
    //此处的异常处理和retry，参考了legacy wrapper object model中的ReliableHttpWebRequest类。不过由于ReliableHttpWebRequest类严重耦合HttpWebRequest\Response，无法重用。
    //legacy wrapper中的retry逻辑太重太复杂，此处做了一定简化
    //此外改变了httpclient SendAsync的行为。IsSuccessStatusCode=false的情况，即便不调用EnsureSuccessStatusCode，也会抛异常。
    internal class HttpMessageHandlerWithRetry : HttpClientHandler
    {
        private static IMicrosoft365Logger Logger => Microsoft365LoggerManager.CreateLogger(typeof(HttpMessageHandlerWithRetry));
        private const int MaxRetryCount = 3;
        private const int Interval = 3;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    var response = await base.SendAsync(request, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        Microsoft365RequestMonitorService.Instance.AddRequest(ResponseStateType.OK);
                        return response;
                    }
                    else
                    {
                        try
                        {
                            var result = response.Content.ReadAsStringAsync().GetResultEx();
                            Logger.Error($@"Call SharePoint rest api failed, {request.Method} {request.RequestUri}
{result}");
                            if (retryCount >= MaxRetryCount || !ProcessErrorResponse(response))
                            {
                                throw new SPRestException($"{request.Method } {request.RequestUri}", response.StatusCode, response.ReasonPhrase, result, $"{response.Headers}{response.Content.Headers}");
                            }
                            retryCount++;
                        }
                        finally
                        {
                            response.Dispose();
                        }
                    }
                }
                catch (Exception ex) when (!(ex is SPRestException))
                {
                    if (retryCount >= MaxRetryCount || !ProcessErrorRequest(ex))
                    {
                        throw;
                    }
                    retryCount++;
                }
            }
        }

        private static bool ProcessErrorRequest(Exception ex)
        {
            Microsoft365RequestMonitorService.Instance.AddRequest(ResponseStateType.Failed);
            if (RequestExceptionHanddler.IsConnectonForciblyClosedExceptioin(ex))
            {
                Wait();
                return true; 
            }
            if (ProcessWebException(ex))
            {
                return true;
            }
            return false;
        }

        private static bool ProcessWebException(Exception ex)
        {
            var webEx = (ex as WebException) ?? (ex.InnerException as WebException);
            if (webEx != null)
            {
                int interval = Interval;
                if (RequestExceptionHanddler.IsUnstableNetworkException(webEx) ||
                    RequestExceptionHanddler.IsNameResolutionFailureException(webEx) ||
                    RequestExceptionHanddler.IsTimedoutException(webEx, ref interval)||
                    RequestExceptionHanddler.IsServerProtocolViolationError(webEx, ref interval))
                {
                    Wait();
                    return true;
                }
            }
            return false;
        }

        private static bool ProcessErrorResponse(HttpResponseMessage response)
        {
            if (ProcessTooManyRequestError(response))
            {
                Microsoft365RequestMonitorService.Instance.AddRequest(ResponseStateType.Throttled);
                return true;
            }
            Microsoft365RequestMonitorService.Instance.AddRequest(ResponseStateType.Failed);
            if (ProcessUnstableServiceError(response))
            {
                return true;
            }
            return false;
        }

        private static bool ProcessTooManyRequestError(HttpResponseMessage response)
        {
            var errorCode = response.StatusCode;
            var retryAfter = response.Headers.RetryAfter?.Delta;
            if (retryAfter.HasValue && retryAfter > TimeSpan.Zero)
            {
                Wait(retryAfter.Value);
                return true;
            }
            if ((int)errorCode == 429)
            {
                Wait(60);
                return true;
            }
            return false;
        }

        private static bool ProcessUnstableServiceError(HttpResponseMessage response)
        {
            var set = new HashSet<HttpStatusCode>() 
            {
                HttpStatusCode.ServiceUnavailable,
                HttpStatusCode.BadRequest,
                HttpStatusCode.BadGateway,
                HttpStatusCode.GatewayTimeout
            };
            var errorCode = response.StatusCode;
            if (set.Contains(errorCode))
            {
                Wait();
                return true;
            }
            return false;
        }

        private static void Wait(int second = Interval)
        {
            Thread.Sleep(second * 1000);
        }

        private static void Wait(TimeSpan t)
        {
            Thread.Sleep(t);
        }
    }
}
