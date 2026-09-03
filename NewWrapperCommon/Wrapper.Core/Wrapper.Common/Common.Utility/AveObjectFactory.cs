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
using System.Reflection;

namespace AvePoint.Wrapper.Common
{
    public class AveObjectFactory
    {
        public static void CopyObject(ref object dest, object src, params string[] escapeFields)
        {
            if (null == src) { throw new ArgumentNullException("The argument src can't be null"); }
            if (null == dest) { throw new ArgumentNullException("The argument dest can't be null"); }
            
            Type srcType = src.GetType();
            Type destType = dest.GetType();

            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
            FieldInfo[] destInfo = destType.GetFields(bindingFlags);

            object srcValue = null;
            object targetValue = null;

            for (int j = 0; j < destInfo.Length; j++)
            {
                string targetName = destInfo[j].Name;                
                if (!ContainsInEscapes(escapeFields, targetName))
                {
                    FieldInfo srcInfo = srcType.GetField(targetName, bindingFlags);
                    if (srcInfo != null)
                    {
                        srcValue = srcInfo.GetValue(src);
                        targetValue = destInfo[j].GetValue(dest);
                        if (destInfo[j].FieldType.IsEnum)
                        {
                            ConvertEnum(out targetValue, destInfo[j].FieldType, srcValue);
                        }
                        else if (destInfo[j].FieldType != srcInfo.FieldType && targetValue != null && srcValue != null)
                        {
                            CopyObject(ref targetValue, srcValue);
                        }
                        else
                        {
                            targetValue = srcValue;
                        }

                        destInfo[j].SetValue(dest, targetValue);
                    }    
                }                
            }                      
        }

        private static bool ContainsInEscapes(string[] escapes, string fieldName)
        {            
            return escapes != null && Array.Exists(escapes, new Predicate<string>(e => e.Equals(fieldName)));
        }      

        public static void CopyObject(object destObj, object srcObj, params string[] escapeFields)
        {
            CopyObject(ref destObj, srcObj, escapeFields);
        }

        public static void CopyObjectProperties(object dest, object src, params string[] escapeFields )
        {
            if (null == src) { throw new ArgumentNullException("The argument src can't be null"); }
            if (null == dest) { throw new ArgumentNullException("The argument dest can't be null"); }

            Type srcType = src.GetType();
            Type destType = dest.GetType();

            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
            PropertyInfo[] destInfo = destType.GetProperties(bindingFlags);

            object srcValue = null;
            object targetValue = null;

            for (int j = 0; j < destInfo.Length; j++)
            {
                string targetName = destInfo[j].Name;
                if (!ContainsInEscapes(escapeFields, targetName))
                {
                    PropertyInfo srcInfo = srcType.GetProperty(targetName, bindingFlags);
                    if (srcInfo != null)
                    {
                        srcValue = srcInfo.GetGetMethod().Invoke(src, null);
                        targetValue = destInfo[j].GetGetMethod().Invoke(dest, null);
                        if (destInfo[j].PropertyType.IsEnum)
                        {
                            ConvertEnum(out targetValue, destInfo[j].PropertyType, srcValue);
                        }
                        else if (destInfo[j].PropertyType != srcInfo.PropertyType && targetValue != null && srcValue != null)
                        {
                            CopyObject(ref targetValue, srcValue);
                        }
                        else
                        {
                            targetValue = srcValue;
                        }

                        destInfo[j].GetSetMethod().Invoke(dest, new object[]{targetValue});
                    }
                }
            }      
        }

        public static void CopyProperty(ref object dest, object src, string fieldName)
        {
            if (null == src || null == dest || null == fieldName)
            {
                throw new ArgumentNullException("One argument is null!");
            }
            Type srcType = src.GetType();
            Type destType = dest.GetType();
            FieldInfo srcInfo = srcType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            FieldInfo destInfo = destType.GetField(fieldName,BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);           
            object val = srcInfo.GetValue(src);
            destInfo.SetValue(dest, val);
        }

        public static void ConvertEnum(out object dest, Type destType, object src)
        {
            if (null == src)
            {
                throw new ArgumentNullException("One argument is null!");
            }

            dest = Enum.Parse(destType, Enum.Format(src.GetType(), src, "d"));
        }

        public static void setProperty(ref object dest, string fieldName, object value)
        {
            if (null == dest || null == fieldName || null == value)
            {
                throw new ArgumentNullException("One argument is null!");
            }
            Type t = dest.GetType();
            FieldInfo f = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            f.SetValue(dest, value);
        }
    }
}
