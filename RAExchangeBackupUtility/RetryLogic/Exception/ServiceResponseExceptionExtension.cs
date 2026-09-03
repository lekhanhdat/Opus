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


namespace ExchangeUtility
{
    using Microsoft.Exchange.WebServices.Data;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// 扩展ServiceResponseException类, 提供重连等待的基础方法
    /// </summary>
    static class ServiceResponseExceptionExtension
    {
        /// <summary>
        /// 执行下一次Request之前需要等待的时间。
        /// </summary>
        /// <param name="exception"></param>
        /// <returns>
        /// 单位为ms
        /// time大于0: [time] ms之后执行下一次request
        /// time等于0: 立即执行下一次request
        /// time小于0: 不需要重连
        /// </returns>
        public static int BackOffMilliseconds(this ServiceResponseException exception)
        {
            var serverBusyException = exception as ServerBusyException;
            if (serverBusyException != null) return serverBusyException.BackOffMilliseconds;

            int backOffMilliseconds;
            if (exception.IsWellKnownInternalServerError(out backOffMilliseconds)) return backOffMilliseconds;

            return exception.ErrorCode.BackOffMilliseconds();
        }
        /// <summary>
        /// 阻塞当前线程一段时间, 使Exchange Server从错误中恢复过来
        /// </summary>
        /// <param name="srEx"></param>
        /// <returns>
        /// true: 需要重连
        /// false: 不需要重连
        /// </returns>
        public static bool WaitForNextRequest(this ServiceResponseException srEx)
        {
            var backOffMilliseconds = srEx.BackOffMilliseconds();
            return WaitForNextRequest(backOffMilliseconds);
        }

        internal static bool WaitForNextRequest(int backOffMilliseconds)
        {
            if (backOffMilliseconds == 0) return true;
            if (backOffMilliseconds < 0) return false;
            Thread.Sleep(backOffMilliseconds);
            return true;
        }

        internal static void ThrowIfWellknownError(this ServiceResponseException srEx, FormattedMessageException.Context context)
        {
            var errorCode = srEx.ErrorCode;
            switch (errorCode)
            {
                case ServiceError.ErrorInternalServerError:
                    srEx.ErrorDetails().ThrowIfWellknownError();
                    break;
                //Please move case after this to ServiceErrorExtension if there is more cases
                case ServiceError.ErrorImpersonateUserDenied:
                    throw new ImpersonateFailedException(context, srEx);
                case ServiceError.ErrorNoPublicFolderReplicaAvailable:
                    throw new NoPublicFolderReplicaAvailableException(context, srEx);
                default:
                    break;
            }
        }

        internal static bool IsWellKnownInternalServerError(this ServiceResponseException srEx, out int backOffMilliseconds)
        {
            backOffMilliseconds = 0;
            if (srEx.ErrorCode == ServiceError.ErrorInternalServerError)
            {
                //var error 
                var boms = srEx.ErrorDetails().BackOffMilliseconds;
                if (boms != null)
                {
                    backOffMilliseconds = boms.Value;
                    return true;
                }
            }
            return false;
        }
        internal static ServiceResponseErrorDetails ErrorDetails(this ServiceResponseException srEx)
        {
            if (srEx.Response == null) return null;
            return new ServiceResponseErrorDetails(srEx);
        }

        internal class ServiceResponseErrorDetails
        {
            public const string ErrorCannotAccessDeletedPublicFolder = "ErrorCannotAccessDeletedPublicFolder";
            public const string ErrorApiQuarantined = "ErrorApiQuarantined";

            private IDictionary<string, string> innerErrorDetails;
            private static Dictionary<string, int> waitTimeMapping = new Dictionary<string, int>()
            {
                #region Example
                //{RetryImmediately,0},
                //{NeverRetry,-1},
                //{RetryAfterXXms,XX},
                #endregion
               
                { ErrorCannotAccessDeletedPublicFolder,-1},
                { ErrorApiQuarantined,-1}
            };


            public ServiceResponseErrorDetails(ServiceResponseException srEx)
            {
                this.innerErrorDetails = srEx.Response.ErrorDetails ?? new Dictionary<string, string>();
            }
            public string InnerErrorResponseCode
            {
                get
                {
                    string error = string.Empty;
                    this.innerErrorDetails.TryGetValue("InnerErrorResponseCode", out error);
                    return error;
                }
            }

            public string InnerErrorMessageText
            {
                get
                {
                    string error = string.Empty;
                    this.innerErrorDetails.TryGetValue("InnerErrorMessageText", out error);
                    return error;
                }
            }

            public int? BackOffMilliseconds
            {
                get
                {
                    var error = this.InnerErrorResponseCode;
                    if (error != null)
                    {
                        int waitTime;
                        if (waitTimeMapping.TryGetValue(error, out waitTime))
                        {
                            return waitTime;
                        }
                    }
                    return null;
                }
            }

            internal void ThrowIfWellknownError()
            {
                switch (this.InnerErrorResponseCode)
                {
                    case ErrorCannotAccessDeletedPublicFolder:
                        throw new CannotAccessDeletedPFException(this.InnerErrorMessageText);
                    case ErrorApiQuarantined:
                        throw new ErrorApiQuarantinedException(this.InnerErrorResponseCode);
                    default:
                        break;
                }
            }

            public override string ToString()
            {
                return string.Join(Environment.NewLine,
                    innerErrorDetails.Select(kv => string.Format("{0}:{1}", kv.Key, kv.Value)));
            }

        }

    }

}
