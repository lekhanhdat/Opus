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



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    using System;

    //如果O365中存在Name相同，类型不同的DomainGroup，在调用Web类的EnsureUser方法抛出ServerException时使用到此异常。
    [Serializable]
    public class AveWrapperUserNotFoundOrNotUniqueException : AveWrapperException
    {
        public AveWrapperUserNotFoundOrNotUniqueException() { }
        public AveWrapperUserNotFoundOrNotUniqueException(string message) : base(message) { }
        public AveWrapperUserNotFoundOrNotUniqueException(string message, Exception inner) : base(message, inner) { }
        protected AveWrapperUserNotFoundOrNotUniqueException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        public AveWrapperUserNotFoundOrNotUniqueException(string key, string defaultValue)
            : base(key, defaultValue)
        {
        }

        public AveWrapperUserNotFoundOrNotUniqueException(string key, string defaultValue, params object[] args)
            : base(key, defaultValue, args)
        {
        }
    }
}

