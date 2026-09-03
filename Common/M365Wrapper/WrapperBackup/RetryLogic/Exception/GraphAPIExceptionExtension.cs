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

namespace ExchangeUtility.Graph
{
    static class GraphAPIExceptionExtension
    {
        private const int ONE_MINUTE = 60 * 1000;
        private const int ONE_SECOND = 1000;
        private static Dictionary<int, int> waitTimeMapping = new Dictionary<int, int>()
        {
            { 503, 15*ONE_SECOND},
            { 401, 1*ONE_SECOND},
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
            {"AuthenticationError", 3*ONE_SECOND},
        };
        private static Dictionary<string, int> errorMessageWaitTimeMapping = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            {"Access token validation failure.",10*ONE_SECOND},
            {"Unsupported token. Unable to initialize the authorization context.",10*ONE_SECOND},
            {"404 page not found\n",10*ONE_SECOND},
        };

        private static bool TryGetWaitTime(this GraphAPIException ex, out int waitTime)
        {
            if ((int)ex.HttpStatusCode == 429 && ex.Tag.Equals("GetTeam"))
            {
                waitTime = 100 * ONE_SECOND;
                return true;
            }
            if (((int)ex.HttpStatusCode == 400 || (int)ex.HttpStatusCode == 403) && (ex.Tag.Equals("GetChats") || ex.Tag.Equals("GetChat")))
            {
                waitTime = 15 * ONE_SECOND;
                return true;
            }
            if ((int)ex.HttpStatusCode != 403 && ex.Tag.Equals("GetChatMessagesInChat"))
            {
                waitTime = 15 * ONE_SECOND;
                return true;
            }
            if ((int)ex.HttpStatusCode == 401 && ex.Tag.Equals("GetHostedContentAsByte"))
            {
                waitTime = -2;
                return false;
            }
            if (ex.HttpStatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (ex.Message.Contains("Caller does not have the required permissions for accessing this API"))
                {
                    waitTime = -2;
                    return false;
                }
            }
            if (ex.Error.Code.Contains("ErrorMailboxMoveInProgress") && ex.Tag.Equals("ListPlannerTaskComments"))
            {
                waitTime = -2;
                return false;
            }
            if (GetRetryAfterDelta(ex.RetryAfter?.Delta, out int retryAfter))
            {
                waitTime = retryAfter;
                if (waitTime < 60 * ONE_SECOND)
                    waitTime = 60 * ONE_SECOND;
                return true;
            }
            if ((int)ex.HttpStatusCode == 504
                && (ex.Tag.Equals("ListChannelMessageAllReplies") || ex.Tag.Equals("GetChatMessages") || ex.Tag.Equals("GetChatMessagesInChat") || ex.Tag.Equals("GetChats")
                 || ex.Tag.Equals("ListChannelMessages")))
            {
                waitTime = 100 * ONE_SECOND;
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
                if (delta.HasValue) waitTime = (int)delta.Value.TotalMilliseconds;
            }
            catch (Exception ex)
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

}