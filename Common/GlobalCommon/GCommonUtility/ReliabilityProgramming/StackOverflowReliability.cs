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




namespace AvePoint.GCommon.Utility.ReliabilityProgramming
{
    #region using directives
    using System;
    using System.Diagnostics;
    using AvePoint.GCommon.Utility.Exceptions;
    #endregion

    /// <summary>
    /// This class is mainly to check the frame count and decide a stack overflow  occurred or not.
    /// <remarks> 
    /// This class is should only be used is a recursive method
    /// </remarks>
    /// </summary>
    public sealed class StackOverflowReliability
    {
        /// <summary>
        /// Check the recursive method is a dead cycle or not
        /// <remarks> If the recursive method frame count is more the the frame count allowed, 
        /// a <see cref="MethodMayLeadToStackOverflowException"/>MethodMayLeadToStackOverflowException
        /// will be thrown
        /// </remarks>
        /// </summary>
        /// <param name="allowedFrameCount">the frame count is allowed, default count is 1000</param>
        public static void CheckStackFrameIfMayLeadToStackOverflowException(Int32 allowedFrameCount = 1000)
        {
            var currentFrameCount = new StackTrace().FrameCount;
            if (currentFrameCount > allowedFrameCount) throw new MethodMayLeadToStackOverflowException();
        }
    }
}
