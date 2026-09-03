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
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Globalization;

namespace AvePoint.Wrapper.Common
{
    public class AveTypeHelper
    {
        //Oliver:只在静态构造方法中初始化
        private static readonly Dictionary<Type, object> BASIC_TYPES = new Dictionary<Type, object>();

        static AveTypeHelper()
        {
            BASIC_TYPES.Add(typeof(bool), null);
            BASIC_TYPES.Add(typeof(string), null);
            BASIC_TYPES.Add(typeof(byte), null);
            BASIC_TYPES.Add(typeof(char), null);
            BASIC_TYPES.Add(typeof(short), null);
            BASIC_TYPES.Add(typeof(int), null);
            BASIC_TYPES.Add(typeof(long), null);
            BASIC_TYPES.Add(typeof(float), null);
            BASIC_TYPES.Add(typeof(double), null);
            BASIC_TYPES.Add(typeof(decimal), null);
            BASIC_TYPES.Add(typeof(sbyte), null);
            BASIC_TYPES.Add(typeof(ushort), null);
            BASIC_TYPES.Add(typeof(uint), null);
            BASIC_TYPES.Add(typeof(ulong), null);
            BASIC_TYPES.Add(typeof(Uri), null);
            BASIC_TYPES.Add(typeof(Guid), null);
            BASIC_TYPES.Add(typeof(DateTime), null);
            BASIC_TYPES.Add(typeof(object), null);
            BASIC_TYPES.Add(typeof(System.Globalization.CultureInfo), null);
            BASIC_TYPES.Add(typeof(System.Collections.Specialized.StringCollection), null);
        }

        public static bool IsBasicType(object obj)
        {
            if (obj != null)
            {
                return IsBasicCollection(obj) || IsBasicDictionary(obj) || IsBasicType(obj.GetType());
            }
            return false;
        }

        public static bool IsBasicType(Type type)
        {
            if (type != null)
            {
                return BASIC_TYPES.ContainsKey(type) || type.IsEnum || IsBasicNullable(type) || IsBasicArray(type) || IsBasicDictionary(type) || IsBasicCollection(type);
            }
            return false;
        }

        public static bool IsBasicArray(Type type)
        {
            if (type != null && type.IsArray)
            {
                return IsBasicType(type.GetElementType());
            }
            return false;
        }

        public static bool IsBasicNullable(Type type)
        {
            if (type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return IsBasicType(type.GetGenericArguments()[0]);
            }
            return false;
        }

        public static bool IsSystemEnum(Type type)
        {
            return type.IsEnum && type.Assembly == typeof(int).Assembly;
        }

        public static bool IsBasicDictionary(object obj)
        {
            if (obj != null && typeof(IDictionary).IsAssignableFrom(obj.GetType()) && obj.GetType().Namespace.StartsWith(typeof(IDictionary).Namespace, StringComparison.OrdinalIgnoreCase))
            {
                IDictionary dic = obj as IDictionary;
                foreach (DictionaryEntry entry in dic)
                {
                    if (!(IsBasicType(entry.Key.GetType()) && IsBasicType(entry.Value.GetType())))
                    {
                        return false;
                    }
                    return IsBasicType(entry.Key) && IsBasicType(entry.Value);
                }
            }
            return false;
        }

        public static bool IsBasicCollection(object obj)
        {
            if (obj != null && typeof(ICollection).IsAssignableFrom(obj.GetType()) && obj.GetType().Namespace.StartsWith(typeof(ICollection).Namespace, StringComparison.OrdinalIgnoreCase))
            {
                ICollection list = obj as ICollection;
                foreach (object ele in list)
                {
                    return IsBasicType(ele);
                }
            }
            return false;
        }

        public static bool IsBasicDictionary(Type type)
        {
            return type != null && typeof(IDictionary).IsAssignableFrom(type) && type.Namespace.StartsWith(typeof(IDictionary).Namespace, StringComparison.OrdinalIgnoreCase) && IsBasicGenericType(type);
        }

        public static bool IsBasicCollection(Type type)
        {
            return type != null && typeof(ICollection).IsAssignableFrom(type) && type.Namespace.StartsWith(typeof(ICollection).Namespace, StringComparison.OrdinalIgnoreCase) && IsBasicGenericType(type);
        }

        public static bool IsBasicGenericType(Type type)
        {
            if (type.IsGenericType)
            {
                Type[] genericArguments = type.GetGenericArguments();
                foreach (Type genericArgument in genericArguments)
                {
                    if (!(BASIC_TYPES.ContainsKey(genericArgument) || IsSystemEnum(genericArgument) || IsBasicArray(genericArgument) || IsBasicDictionary(genericArgument) || IsBasicCollection(genericArgument)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static object CastEnumValue(object value)
        {
            Type underlyingType = Enum.GetUnderlyingType(value.GetType());
            return Convert.ChangeType(value, underlyingType);
        }

        public static T CastEnumValue<T>(object value) where T : struct
        {
            Type type = typeof(T).IsEnum ? Enum.GetUnderlyingType(typeof(T)) : typeof(T);
            return (T)Convert.ChangeType(value, type);
        }

        public static object CreatGenericList(object array)
        {
            Type listType = typeof(List<>).MakeGenericType(new Type[] { array.GetType().GetElementType() });
            object list = Activator.CreateInstance(listType, new object[] { });
            listType.InvokeMember("AddRange", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, list, new object[] { array }, CultureInfo.InvariantCulture);
            return list;
        }

        public static bool IsGuid(string strId)
        {
            if (string.IsNullOrEmpty(strId))
            {
                return false;
            }
            strId = strId.Trim();
            if (strId.Length < 0x20)
            {
                return false;
            }
            if (strId.Contains("x") || strId.Contains("X"))
            {
                strId = strId.Replace(" ", "");
                return Regex.IsMatch(strId, @"^\{0[x|X][a-fA-F\d]{8},(0[x|X][a-fA-F\d]{4},){2}\{(0[x|X][a-fA-F\d]{2},){7}0[x|X][a-fA-F\d]{2}\}\}$", RegexOptions.Compiled);
            }
            return Regex.IsMatch(strId, @"^([a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}|\([a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}\)|\{[a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}\}|[a-fA-F\d]{32})$", RegexOptions.Compiled);
        }

        public static T ParseEnum<T>(object enumValue) where T : struct
        {
            if (enumValue == null || !enumValue.GetType().IsEnum)
            {
                throw new ArgumentException("Enum value is not valid.");
            }

            return (T)Enum.Parse(typeof(T), enumValue.ToString(), true);
        }
    }
}
