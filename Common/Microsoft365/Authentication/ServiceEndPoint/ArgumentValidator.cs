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
using System.Collections;
using System.Globalization;

namespace Microsoft365.Authentication.ServiceEndPoint
{
    internal static class ArgumentValidator
    {
        public static void ThrowIfEmpty(Guid argValue, string argName)
        {
            if (argValue == Guid.Empty)
            {
                throw new ArgumentException("Value can not be empty.", argName);
            }
        }

        public static void ThrowIfEmpty(IEnumerable argValue, string argName)
        {
            if (argValue != null && !argValue.GetEnumerator().MoveNext())
            {
                throw new ArgumentException("Expected at least one item in the collection.", argName);
            }
        }

        public static void ThrowIfNotEnum(Type argValue)
        {
            if (!argValue.IsEnum)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Type {0} is not an Enum.", new object[]
                {
                    argValue
                }));
            }
        }

        public static void ThrowIfNull(object argValue, string argName)
        {
            if (argValue == null)
            {
                throw new ArgumentNullException(argName);
            }
        }

        public static void ThrowIfNullOrEmpty(IEnumerable argValue, string argName)
        {
            if (argValue == null)
            {
                throw new ArgumentNullException(argName);
            }
            ThrowIfEmpty(argValue, argName);
        }
    }
}