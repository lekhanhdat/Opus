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




namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    [Serializable]
    public class AveWrapperException : AveWrapperI18NException
    {
        public AveWrapperErrorCode ErrorCode { get; private set; }

        public AveWrapperException() { }
        public AveWrapperException(string message) : base(message) { }
        public AveWrapperException(string message, Exception inner) : base(message, inner) { }
        protected AveWrapperException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
        public AveWrapperException(AveWrapperErrorCode errorCode, string message)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }
        public AveWrapperException(AveWrapperErrorCode errorCode, string message,Exception inner)
            : base(message, inner)
        {
            this.ErrorCode = errorCode;
        }

        public AveWrapperException(string key, string defaultValue)
            : base(key, defaultValue)
        {
        }

        public AveWrapperException(string key ,string defaultValue, params object[] args)
            : base(key, defaultValue, args) 
        { 
        }
        public override string ToString()
        {
            if (this.ErrorCode == AveWrapperErrorCode.UnKnown)
            {
                return base.ToString();
            }
            return new StringBuilder(this.ErrorCode.ToString()).AppendLine(base.ToString()).ToString();
        }
    }
}
