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




namespace AvePoint.GCommon.MicroKernel
{
    #region using directives
    using System;
    using System.Runtime.Serialization;
    #endregion

    /// <summary>
    /// This exception is to wrap the internal exception as a global message,
    /// Also, instead of using the wcf user defined fault exception and a
    /// httperrorhandler interface
    /// </summary>
    [Serializable]
    public class MicroKernelInternalInvocationException : Exception
    {
        public MicroKernelInternalInvocationException() { }

        public MicroKernelInternalInvocationException(String message)
            : base(message)
        { }

        public MicroKernelInternalInvocationException(String message, Exception inner)
            : base(message, inner)
        { }

        protected MicroKernelInternalInvocationException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context)
        { }

        public String ExceptionDetails { get; internal set; }
        public String ExceptionMessage { get; internal set; }
        public String ExceptionRawMessage { get; internal set; }

        public override String ToString()
        {
            return this.ExceptionDetails;
        }
    }
}