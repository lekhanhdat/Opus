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
using AvePoint.RA.Common.Util;
using System;
using System.Linq;

namespace AvePoint.Records.Core.Utilities.Extensions
{
    /*
     * there should be NO business logic. 
     * 
     */

    /// <summary>
    /// 
    /// </summary>
    public static class Int32Extentions
    {
        #region BitWise validate helper

        public static bool HasFlag<T>(this int value, T flag) where T : Enum
        {
            int flagValue = (int)(object)flag;
            return value.HasFlag(flagValue);
        }

        public static bool HasFlags<T>(this int value, JoinTypes matchType, params T[] flags) where T : Enum
        {
            if (flags == null || flags.Length == 0) return false;

            var flagValues = flags.Select(f => (int)(object)f).ToArray();
            return value.HasFlags(matchType, flagValues);
        }

        public static bool HasFlag(this int value, int flagValue)
        {
            return (value & flagValue) == flagValue;
        }

        public static bool HasFlags(this int value, JoinTypes matchType, params int[] flags)
        {
            if (flags == null || flags.Length == 0) return false;

            return matchType switch
            {
                JoinTypes.And => flags.All(f => value.HasFlag(f)),
                //case JoinTypes.Or:
                _ => flags.Any(f => value.HasFlag(f)),
            };
        }

        #endregion
    }
}
