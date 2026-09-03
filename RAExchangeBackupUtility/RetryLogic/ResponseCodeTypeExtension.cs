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

    /// <summary>
    /// 该类主要扩展ResponseCodeType类, 将ResponseCode值与重连等待时间做映射。
    /// 内部调用ServiceErrorExtension类, ServiceError和ResponseCodeType两个枚举代表相同的意义, 因此不要在这个类中加入其他业务逻辑。
    /// </summary>
    static class ResponseCodeTypeExtension
    {
        public static bool IsWaitAndTryLaterError(this ResponseCodeType errorCode)
        {
            return errorCode.ToServiceErrorCode().IsWaitAndTryLaterError();
        }

        public static bool IsImmediatelyRetryError(this ResponseCodeType errorCode)
        {
            return errorCode.ToServiceErrorCode().IsImmediatelyRetryError();
        }

        public static bool IsNeverRetryError(this ResponseCodeType errorCode)
        {
            return errorCode.ToServiceErrorCode().IsNeverRetryError();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns>wait time in ms</returns>
        public static int BackOffMilliseconds(this ResponseCodeType errorCode)
        {
            return errorCode.ToServiceErrorCode().BackOffMilliseconds();
        }

        public static ServiceError ToServiceErrorCode(this ResponseCodeType errorCode)
        {
            var serviceErrorCode = (ServiceError)(-1);
            Enum.TryParse(errorCode.ToString(), true, out serviceErrorCode);
            return serviceErrorCode;
        }
    }
}
