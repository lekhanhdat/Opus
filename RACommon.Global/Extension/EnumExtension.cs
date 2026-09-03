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
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Extension
{
    public static class EnumExtension
    {
        public static List<T> Split<T>(this T enumObj) where T : struct, IConvertible
        {
            List<T> result = new List<T>();
            var objList = enumObj.ToString().Split(new String[1] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var obj in objList)
            {
                if (!Enum.TryParse<T>(obj, out T p))
                {
                    continue;
                }
                result.Add(p);

            }
            return result;
        }

        public static TEnum ToEnum<TEnum>(this string value, bool ignoreCase = true) where TEnum : struct, Enum
        {
            if (!Enum.TryParse<TEnum>(value, ignoreCase, out var result))
            {
                throw new ArgumentException($"Invalid value '{value}' for enum '{typeof(TEnum).Name}'");
            }
            return result;
        }
    }
}
