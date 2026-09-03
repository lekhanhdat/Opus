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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Media.Service.ArchiverBackup
{
    public static class ArchiverBackupExtension
    {
        /// <summary>
        /// format string with instance
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static String FormatWith(this String format, params Object[] args)
        {
            return String.Format(format, args);
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        //public static bool EqualsIgnoreCase(this string currentValue, string compareValue)
        //{
        //    return currentValue?.Equals(compareValue, StringComparison.OrdinalIgnoreCase) ?? (compareValue == null);
        //}
        public static int IndexOfIgnoreCase(this string currentValue, string compareValue)
        {
            return currentValue.IndexOf(compareValue, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EndWithIgnoreCase(this string value, string arg)
        {
            return value.EndsWith(arg, StringComparison.OrdinalIgnoreCase);
        }

        public static int LastIndexOfIgnoreCase(this string currentValue, string compareValue)
        {
            return currentValue.LastIndexOf(compareValue, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Compute a MD5 hash value of the input string value
        /// </summary>
        /// <param name="value">input value</param>
        /// <returns>the result md5 of the input string value</returns>
        public static String ToMD5HashCode(this String value)
        {
            return HashCodeHelper.ToMD5HashCode(value);
        }

        public static NodeLevel ToNodeLevelByMediaDataTypeString(this String value)
        {
            return value.ToEnum<MediaDataType>().GetAttribute<DataTypeMapAttribute>().NodeLevel;
        }

        public static T ToEnum<T>(this string name)
        {
            if (false == typeof(T).IsEnum)
                throw new NotSupportedException(typeof(T).Name + " must be an Enum");

            if (false == Enum.IsDefined(typeof(T), name))
                throw new ArgumentException($"{name} is not defined in type of enum {typeof(T).Name}");

            return (T)Enum.Parse(typeof(T), name, true);
        }

        public static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            return Attribute.GetCustomAttribute(value.GetType().GetField(value.ToString()), typeof(T)) as T;
        }

        public static String ToMediaDataTypeStringBySPObjectLevelString(this String value)
        {
            return value.ToEnum<MediaSPObjectLevel>().GetAttribute<SPObjectMapAttribute>().DataType.ToString();
        }

        public static int CompareToIngnoreCase(this string currentValue, string compareValue)
        {
            return string.Compare(currentValue, compareValue, StringComparison.OrdinalIgnoreCase);
        }

        public static long JavaToDotNetTimeInLong(this long javaTimeInLong)
        {
            if (javaTimeInLong != 0L)
            {
                return javaTimeInLong * 10000 + new DateTime(1970, 1, 1, 0, 0, 0).Ticks;
            }

            return 0L;
        }
    }
}
