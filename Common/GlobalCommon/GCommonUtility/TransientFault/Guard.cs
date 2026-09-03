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
using System.Globalization;

namespace AvePoint.GCommon.Utility.TransientFault
{
    internal static class Guard
    {
        public static bool ArgumentNotNullOrEmptyString(string argumentValue, string argumentName)
        {
            Guard.ArgumentNotNull(argumentValue, argumentName);
            if (argumentValue.Length == 0)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The specified string argument {0} must not be empty.", new object[]
				{
					argumentName
				}));
            }
            return true;
        }
        public static bool ArgumentNotNull(object argumentValue, string argumentName)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(argumentName);
            }
            return true;
        }
        public static void ArgumentNotNegativeValue(int argumentValue, string argumentName)
        {
            if (argumentValue < 0)
            {
                throw new ArgumentOutOfRangeException(argumentName, argumentValue, string.Format(CultureInfo.CurrentCulture, "The specified argument {0} cannot be initialized with a negative value.", new object[]
				{
					argumentName
				}));
            }
        }
        public static void ArgumentNotNegativeValue(long argumentValue, string argumentName)
        {
            if (argumentValue < 0L)
            {
                throw new ArgumentOutOfRangeException(argumentName, argumentValue, string.Format(CultureInfo.CurrentCulture, "The specified argument {0} cannot be initialized with a negative value.", new object[]
				{
					argumentName
				}));
            }
        }
        public static void ArgumentNotGreaterThan(double argumentValue, double ceilingValue, string argumentName)
        {
            if (argumentValue > ceilingValue)
            {
                throw new ArgumentOutOfRangeException(argumentName, argumentValue, string.Format(CultureInfo.CurrentCulture, "The specified argument {0} cannot be greater than its ceiling value of {1}.", new object[]
				{
					argumentName,
					ceilingValue
				}));
            }
        }
    }
}
