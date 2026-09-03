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




namespace AvePoint.GCommon.Utility.Exceptions
{
    #region using directives
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    #endregion

    /// <summary>
    /// Method May Lead To Stack Overflow Exception will be thrown in a recursive method
    /// </summary>
    [Serializable]
    public class MethodMayLeadToStackOverflowException : Exception
    {
        public Int32 CurrentFrameCount { get { return new StackTrace().FrameCount; } }

        public MethodMayLeadToStackOverflowException() { }

        public MethodMayLeadToStackOverflowException(String message)
            : base(message) { }

        public MethodMayLeadToStackOverflowException(String message, Exception inner)
            : base(message, inner) { }

        protected MethodMayLeadToStackOverflowException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }

        public override string ToString()
        {
            return String.Format(@"The current method {0} frame count is {1}, it's a so big number that may 
                lead to a StackOverflowException and the current method should be terminated ",
                new StackTrace().GetFrame(1).GetMethod().Name, this.CurrentFrameCount);
        }
    }
}
