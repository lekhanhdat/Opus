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

using AvePoint.GCommon.GraphAPI;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AvePoint.Wrapper.Common.Graph
{
    static class GraphAPIExceptionExtension
    {
        private const int ONE_SECOND = 1000;
        private static Dictionary<int, int> waitTimeMapping = new Dictionary<int, int>()
        {
            { 503, 15*ONE_SECOND},
            { 401, 10*ONE_SECOND},
            { 429, 90*ONE_SECOND},
            { 504, 10*ONE_SECOND},
            { 502, 30*ONE_SECOND},
            { 500, 10*ONE_SECOND},
            { 408, 10*ONE_SECOND},
        };
        private static Dictionary<string, int> errorCodeWaitTimeMapping = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            {"ErrorInvalidGroup",10*ONE_SECOND},
            {"ErrorNonExistentMailbox",-2},//未发现此 errorCode 对结果的影响，暂时不retry。
            {"InvalidAuthenticationToken",10*ONE_SECOND},
        };
        private static Dictionary<string, int> errorMessageWaitTimeMapping = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            {"Access token validation failure.",10*ONE_SECOND},
            {"Unsupported token. Unable to initialize the authorization context.",10*ONE_SECOND},
            {"Sharepoint folder not found.",10*ONE_SECOND},
        };

        private static bool TryGetWaitTime(this GraphAPIException ex, out int waitTime)
        {
            if (GetRetryAfterDelta(ex.RetryAfter?.Delta, out int retryAfter))
            {
                waitTime = retryAfter;
                return true;
            }
            if (!waitTimeMapping.TryGetValue((int)ex.HttpStatusCode, out waitTime))
            {
                var errorCode = ex.Error?.Code ?? String.Empty;
                if (!errorCodeWaitTimeMapping.TryGetValue(errorCode, out waitTime))
                {
                    return errorMessageWaitTimeMapping.TryGetValue(ex.Error.Message, out waitTime);
                }
            }
            return true;
        }

        private static bool GetRetryAfterDelta(TimeSpan? delta, out int waitTime)
        {
            waitTime = 0;
            try
            {
                if (delta.HasValue) waitTime = ((int)delta.Value.TotalSeconds) * ONE_SECOND;
            }
            catch (Exception)
            {
                return false;
            }
            return delta.HasValue;
        }
        internal static bool WaitForNextRequest(this GraphAPIException gaEx)
        {
            var backOffMilliseconds = GetWaitTime(gaEx);
            if (backOffMilliseconds == 0) return true;
            if (backOffMilliseconds < 0) return false;
            Thread.Sleep(backOffMilliseconds);
            return true;
        }
        internal static int GetWaitTime(GraphAPIException ex)
        {
            int waitTime;
            if (ex.TryGetWaitTime(out waitTime))
            {
                return waitTime;
            }
            return -2;
        }
    }

    static class ExceptionExtension
    {
        public static bool IsConnectonForciblyClosedExceptioin(this Exception te)
        {
            if (te.InnerException != null && !string.IsNullOrEmpty(te.InnerException.Message) &&
               (te.InnerException.Message.Contains("An existing connection was forcibly closed by the remote host") ||
                te.InnerException.Message.Contains("The underlying connection was closed")))
            {
                return true;
            }
            else if (te.InnerException != null)
            {
                return IsConnectonForciblyClosedExceptioin(te.InnerException);
            }
            return false;
        }
        public static bool IsTaskCanceledExceptioin(this Exception te)
        {
            if (te.InnerException != null && !string.IsNullOrEmpty(te.InnerException.Message) && te.InnerException.Message.Contains("A task was canceled"))
            {
                return true;
            }
            else if (te.InnerException != null)
            {
                return IsTaskCanceledExceptioin(te.InnerException);
            }
            return false;
        }
        public static bool IsErrorRequestExceptioin(this Exception te)
        {
            if (te.InnerException != null && !string.IsNullOrEmpty(te.InnerException.Message) && te.InnerException.Message.Contains("The remote name could not be resolved"))
            {
                return true;
            }
            else if (te.InnerException != null)
            {
                return IsErrorRequestExceptioin(te.InnerException);
            }
            return false;
        }
    }

}