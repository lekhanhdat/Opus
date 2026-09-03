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
using System.Text;
using System.Reflection;

namespace AvePoint.ObjectModel.ClientOM
{
    public class TypeHelper
    {
        private static HashSet<Type> BASIC_TYPES = new HashSet<Type>();

        static TypeHelper()
        {
            BASIC_TYPES.Add(typeof(bool));
            BASIC_TYPES.Add(typeof(string));
            BASIC_TYPES.Add(typeof(bool));
            BASIC_TYPES.Add(typeof(byte));
            BASIC_TYPES.Add(typeof(char));
            BASIC_TYPES.Add(typeof(short));
            BASIC_TYPES.Add(typeof(int));
            BASIC_TYPES.Add(typeof(long));
            BASIC_TYPES.Add(typeof(float));
            BASIC_TYPES.Add(typeof(double));
            BASIC_TYPES.Add(typeof(decimal));
            BASIC_TYPES.Add(typeof(sbyte));
            BASIC_TYPES.Add(typeof(ushort));
            BASIC_TYPES.Add(typeof(uint));
            BASIC_TYPES.Add(typeof(ulong));
            BASIC_TYPES.Add(typeof(Uri));
            BASIC_TYPES.Add(typeof(Guid));
            BASIC_TYPES.Add(typeof(DateTime));
            BASIC_TYPES.Add(typeof(System.Globalization.CultureInfo));
            BASIC_TYPES.Add(typeof(System.Collections.Specialized.StringCollection));
        }

        public static bool IsBasicType(object obj)
        {
            if (obj != null)
            {
                return IsBasicType(obj.GetType()) || IsBasicCollection(obj) || IsBasicDictionary(obj);
            }
            return false;
        }

        public static bool IsBasicType(Type type)
        {
            if (type != null)
            {
                return BASIC_TYPES.Contains(type) || type.IsEnum || IsBasicArray(type);
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

        public static bool IsBasicDictionary(object obj)
        {
            if (obj != null && typeof(IDictionary).IsAssignableFrom(obj.GetType()))
            {
                IDictionary dic = obj as IDictionary;
                foreach (DictionaryEntry entry in dic)
                {
                    return IsBasicType(entry.Key) && IsBasicType(entry.Value);
                }
            }
            return false;
        }

        public static bool IsBasicCollection(object obj)
        {
            if (obj != null && typeof(ICollection).IsAssignableFrom(obj.GetType()))
            {
                ICollection list = obj as ICollection;
                foreach (object ele in list)
                {
                    return IsBasicType(ele);
                }
            }
            return false;
        }

        public static object CastEnumValue(object value)
        {
            Type underlyingType = Enum.GetUnderlyingType(value.GetType());
            return Convert.ChangeType(value, underlyingType);
        }

        public static object CreatGenericList(object array)
        {
            Type listType = Type.GetType("System.Collections.Generic.List`1[[" + array.GetType().GetElementType().FullName + "]]");
            //object list = listType.TypeInitializer.Invoke(null);
            object list = Activator.CreateInstance(listType, new object[] { });
            listType.InvokeMember("AddRange", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, list, new object[] { array }, System.Globalization.CultureInfo.InvariantCulture);
            return list;
        }
    }
}
