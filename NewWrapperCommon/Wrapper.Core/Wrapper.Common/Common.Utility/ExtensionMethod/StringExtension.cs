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

namespace AvePoint.Wrapper.Common
{
    static class StringExtension
    {
        public static T? ToNullableValueType<T>(this string value) where T : struct
        {
            if (value == null) return null;
            if (typeof(T) == typeof(int))
            {
                int result;
                return (int.TryParse(value, out result) ? result : (int?)null) as T?;
            }
            if (typeof(T) == typeof(bool))
            {
                bool result;
                return (bool.TryParse(value, out result) ? result : (bool?)null) as T?;
            }
            if (typeof(T) == typeof(Guid))
            {
                try
                {
                    return new Guid?(new Guid(value)) as T?;
                }
                catch (Exception ex)
                {
                    ex.EatException();
                    return null;
                }
            }
            if (typeof(T) == typeof(long))
            {
                long result;
                return (long.TryParse(value, out result) ? result : (long?)null) as T?;
            }
            if (typeof(T) == typeof(byte))
            {
                byte result;
                return (byte.TryParse(value, out result) ? result : (byte?)null) as T?;
            }
            if (typeof(T) == typeof(short))
            {
                short result;
                return (short.TryParse(value, out result) ? result : (short?)null) as T?;
            }
            throw new ArgumentException(typeof(T).FullName);
        }

        public static string Trim(this string source, bool trimStart, bool trimEnd, params string[] trims)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }

            string result = source.Trim();
            if (trims == null || trims.Length <= 0)
            {
                result = result.Trim();
                return source;
            }
            foreach (var trim in trims)
            {
                if (string.IsNullOrEmpty(trim))
                {
                    result = result.Trim();
                    return source;
                }
                if (trimStart)
                {
                    while (result.StartsWith(trim, System.StringComparison.OrdinalIgnoreCase))
                    {
                        result = result.Substring(trim.Length);
                    }
                }
                if (trimEnd)
                {
                    while (result.EndsWith(trim, System.StringComparison.OrdinalIgnoreCase))
                    {
                        result = result.Substring(0, result.Length - trim.Length);
                    }
                }
            }
            return result;
        }
    }
}
