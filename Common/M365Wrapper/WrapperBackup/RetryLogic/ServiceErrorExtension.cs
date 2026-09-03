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

namespace ExchangeUtility.Graph
{
    using Microsoft.Exchange.WebServices.Data;

    using AvePoint.RA.CommonUtil;

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 该类主要扩展ServiceError类, 将ServiceError值与重连等待时间做映射。
    /// 若需要对其他错误类型添加等待-重连逻辑, 只需要在waitTimeMapping中添加一组键值对即可。
    /// {ServiceError.RetryImmediately,0},
    /// {ServiceError.NeverRetry,-1},
    /// {ServiceError.RetryAfterXXms,XX},
    /// 线程安全
    /// </summary>
    static class ServiceErrorExtension
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(ServiceErrorExtension));
        public const int DefaultBackOffMilliseconds = 0;
        private static Dictionary<ServiceError, int> waitTimeMapping;
        private const int ONE_MINUTE = 60 * 1000;
        private const int ONE_SECOND = 1000;
        private const int NEVER_RETRY = -2;

        static ServiceErrorExtension()
        {
            //https://msdn.microsoft.com/en-us/library/exchangewebservices.responsecodetype(v=exchg.150).aspx
            waitTimeMapping = new Dictionary<ServiceError, int>
            {
                #region Example
                //{ServiceError.RetryImmediately,0},
                //{ServiceError.NeverRetry,-1},
                //{ServiceError.RetryAfterXXms,XX},
                #endregion

                #region WaitAndTryLaterError
                //Indicates that the mailbox is being moved to a different mailbox store or server. 
                //This can also indicate that the mailbox is on another server or mailbox database.
                { ServiceError.ErrorMailboxMoveInProgress, 2 * ONE_MINUTE },
				//ErrorServerBusy
                //https://msdn.microsoft.com/en-us/library/office/jj945066(v=exchg.150).aspx#bk_ThrottlingErrors
                //Against the following throttling policy 
                //EWSPercentTimeInMailboxRPC
                //EWSPercentTimeInCAS 
                //EWSPercentTimeInAD
                //Occurs when the server is busy. The BackOffMilliseconds value returned with ErrorServerBusy errors indicates to the client the amount of time it should wait until it should resubmit the request that caused the response that returned this error code.
                { ServiceError.ErrorServerBusy, 2 * ONE_MINUTE},
                //For Azure AD is unavailable
                { ServiceError.ErrorADUnavailable, 3 * ONE_MINUTE},
                // Exchange Web Services are not currently available for this request because none of the Client Access Servers in the destination site could process the request
                { ServiceError.ErrorNoRespondingCASInDestinationSite, 2 * ONE_MINUTE},
                //For mailbox store is unavailable
                { ServiceError.ErrorMailboxStoreUnavailable, 1 * ONE_MINUTE},
                //This error -iIndicates that there are more concurrent requests against the server than are allowed by a user's policy.
                { ServiceError.ErrorExceededConnectionCount, 30 * ONE_SECOND },
                #region EWS encountered an error:[Try again later., Cannot open mailbox...] Could be caused by TooManyMailboxOpen in exchange server.
                //Indicates that Exchange Web Services encountered an error that it could not recover from, 
                //and no more specific response code is associated with the error that occurred.
                { ServiceError.ErrorInternalServerError, 5 * ONE_SECOND },
                //Indicates that an internal server error occurred and that you should try your request again later.
                { ServiceError.ErrorInternalServerTransientError, 5 * ONE_SECOND },
                //This error occurs when the internal limit on open objects has been exceeded.
                {ServiceError.ErrorTooManyObjectsOpened, 1 * ONE_MINUTE },
                //Connection did not succeed. Try again later., Cannot open mailbox.
                {ServiceError.ErrorConnectionFailed, 1 * ONE_MINUTE },
                #endregion

                { ServiceError.ErrorImpersonateUserDenied, 10 * ONE_SECOND },
                //{ ServiceError.ErrorMailboxConfiguration, 30 * ONE_SECOND },
                #endregion

                #region ImmediatelyRetryError
                #endregion
                
                #region NeverRetryError
                {ServiceError.ErrorMessageSizeExceeded,NEVER_RETRY},
                {ServiceError.ErrorInvalidOperation,NEVER_RETRY},
                {ServiceError.ErrorAccessDenied,NEVER_RETRY},
                //{ServiceError.ErrorImpersonateUserDenied,NEVER_RETRY},
                {ServiceError.ErrorNoPublicFolderReplicaAvailable,NEVER_RETRY},
                {ServiceError.ErrorFolderNotFound,NEVER_RETRY},
                {ServiceError.ErrorNonExistentMailbox,NEVER_RETRY},
                {ServiceError.ErrorItemNotFound,NEVER_RETRY},
                {ServiceError.ErrorSearchFolderNotInitialized, NEVER_RETRY },
                #endregion

            };
        }
        public static bool IsWaitAndTryLaterError(this ServiceError errorCode)
        {
            return BackOffMilliseconds(errorCode) > 0;
        }

        public static bool IsImmediatelyRetryError(this ServiceError errorCode)
        {
            return BackOffMilliseconds(errorCode) == 0;
        }

        public static bool IsNeverRetryError(this ServiceError errorCode)
        {
            return BackOffMilliseconds(errorCode) < 0;
        }

        public static bool IsWaitAndTryServiceError(string errorCode)
        {
            try
            {
                var tempErrorCode = (ServiceError)Enum.Parse(typeof(ServiceError), errorCode);
                int waitTime;
                if (waitTimeMapping.TryGetValue(tempErrorCode, out waitTime)) return waitTime > 0;
                else return false;
            }
            catch (Exception ex)
            {
                logger.Warn($"An error occurred when to judge the error code. Reason: {ex.ToString()}. ");
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns>wait time in ms</returns>
        public static int BackOffMilliseconds(this ServiceError errorCode)
        {
            int waitTime;
            if (waitTimeMapping.TryGetValue(errorCode, out waitTime))
            {
                return waitTime;
            }
            return DefaultBackOffMilliseconds;
        }

    }
}