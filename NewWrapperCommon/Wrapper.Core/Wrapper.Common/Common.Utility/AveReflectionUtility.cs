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
using System.Text;
using System.Reflection;

namespace AvePoint.Wrapper.Common
{
    public static class AveReflectionUtility
    {
        //create instance if it don't exist in the Dictionary specified in params
        public static void CheckCacheCreateInstance(Dictionary<string, object> cache, string propertyName, out object result, Type instanceType, params object[] constructParam)
        {
            if (!cache.TryGetValue(propertyName, out result))
            {
                result = Activator.CreateInstance(instanceType, constructParam);
                cache.Add(propertyName, result);
            }
        }

        public static object GetFieldValue(string fieldName, object target)
        {
            ArgumentCheck.CheckNotNull(target, fieldName);
            return target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
        }

        public static void SetFieldValue(string fieldName, object target, object value)
        {
            ArgumentCheck.CheckNotNull(target, fieldName);
            target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }

        public static Type[] GetMethodParameterTypes(MethodInfo method)
        {
            ParameterInfo[] parameterInfos = method.GetParameters();
            Type[] parameterTypes = new Type[parameterInfos.Length];

            for (int i = 0; i < parameterInfos.Length; i++)
            {
                parameterTypes[i] = parameterInfos[i].ParameterType;
            }
            return parameterTypes;
        }

        public static Type[] GetMethodParameterTypes(object[] parameterInfos)
        {           
            Type[] parameterTypes = new Type[parameterInfos.Length];

            for (int i = 0; i < parameterInfos.Length; i++)
            {
                parameterTypes[i] = parameterInfos[i].GetType();
            }
            return parameterTypes;
        }

        public static MethodInfo GetMethodIncludingInherited(Type type, string methodName, BindingFlags bf, Type[] types)
        {
            MethodInfo retMethod = type.GetMethod(methodName, bf, null, types, null);
            if (retMethod == null) 
            {
                foreach(Type superType in type.GetInterfaces()) 
                {
                    retMethod = superType.GetMethod(methodName, bf, null, types, null);
                    if (retMethod != null) 
                    {
                        break;
                    }
                }
            }
            return retMethod;
        }

        public static bool IsGenericGetEnumeratorMethod(MethodInfo method)
        {
            return method != null && method.Name.EndsWith("GetEnumerator",StringComparison.OrdinalIgnoreCase) && method.ReturnType.Name.Equals("IEnumerator`1");
        }

        public static bool IsAssignableFromGenericIEnumerable(Type type)
        {
            return type != null && type.IsGenericType && type.Name.Equals("IEnumerator`1") && type.Namespace.Equals("System.Collections.Generic");
        }

        public static Type GetIEnumerableParameterType(Type type)
        {
            Type parameterType = null;
            if (IsAssignableFromGenericIEnumerable(type))
            {
                parameterType = type.GetGenericArguments()[0];
            }
            return parameterType;
        }

        public static Type MakeGenericIEnumerator(Type parameterType)
        {
            Type t = typeof(IEnumerator<>);
            return t.GetGenericTypeDefinition().MakeGenericType(parameterType);
        }

        public static bool ContainsProperty(Type type, string property)
        {
            return type.GetProperty(property) != null;
        }

        public static bool IsPropertyGetterMethod(Type type, string getterMethod)
        {
            string getPrefix = "get_";
            return getterMethod.StartsWith(getPrefix,StringComparison.OrdinalIgnoreCase) && ContainsProperty(type, getterMethod.Substring(getPrefix.Length));            
        }

        public static bool IsPropertySetterMethod(Type type, string setterMethdod)
        {
            string setPrefix = "set_";
            return setterMethdod.StartsWith(setPrefix,StringComparison.OrdinalIgnoreCase) && ContainsProperty(type, setterMethdod.Substring(setPrefix.Length));
        }
    }    
}
