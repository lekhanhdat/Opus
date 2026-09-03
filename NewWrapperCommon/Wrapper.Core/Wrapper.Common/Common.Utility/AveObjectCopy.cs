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
using System.Collections;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.Common;

namespace AvePoint.Wrapper.Common
{
    public class AveObjectCopy
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

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
                            if (destInfo[j].FieldType.IsArray)
                            {
                                object tmpTarget = Activator.CreateInstance(targetValue.GetType(), (src as IList).Count);
                                int i = 0;
                                foreach (object srcElement in srcValue as IEnumerable)
                                {
                                    object destElement = Activator.CreateInstance(targetValue.GetType().GetElementType(), null);
                                    CopyObject(destElement, srcElement, null);
                                    (tmpTarget as IList)[i++] = destElement;
                                }
                                targetValue = tmpTarget;
                            }
                            else
                            {
                                CopyObject(ref targetValue, dest);
                            }
                        }
                        else if (destInfo[j].FieldType != srcInfo.FieldType && targetValue == null && srcValue.GetType().GetGenericArguments().Length != 0)
                        {
                            object tmpTarget = Activator.CreateInstance(destInfo[j].FieldType, (srcValue as IList).Count);
                            foreach (object srcElement in srcValue as IEnumerable)
                            {
                                if (destInfo[j].FieldType.GetGenericArguments().Length != 0)
                                {
                                    object destElement = Activator.CreateInstance(destInfo[j].FieldType.GetGenericArguments()[0], null);
                                    CopyObject(destElement, srcElement, null);
                                    (tmpTarget as IList).Add(destElement);
                                }
                            }
                            targetValue = tmpTarget;
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

        public static void CopyObjectProperties(object dest, object src, params string[] escapeFields)
        {
            if (null == src) { throw new ArgumentNullException("src can't be null"); }
            if (null == dest) { throw new ArgumentNullException("dest can't be null"); }

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

                        destInfo[j].GetSetMethod().Invoke(dest, new object[] { targetValue });
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
            FieldInfo destInfo = destType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
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

        public static void GetObjectBasicProperties(Dictionary<string, object> properitesDic, object obj, params string[] escapeProperties)
        {
            if (obj != null && properitesDic != null)
            {
                escapeProperties = escapeProperties == null ? new string[] { } : escapeProperties;
                PropertyInfo[] propertyInfos = obj.GetType().GetProperties();
                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    try
                    {
                        if (!ContainsKeyFromProperties(escapeProperties, propertyInfo.Name) && AveTypeHelper.IsBasicType(propertyInfo.PropertyType))
                        {
                            MethodInfo getMethod = propertyInfo.GetGetMethod();
                            if (getMethod != null)
                            {
                                object propValue = getMethod.Invoke(obj, null);
                                if (getMethod.ReturnType.IsEnum)
                                {
                                    properitesDic[propertyInfo.Name] = AveTypeHelper.CastEnumValue(propValue);
                                }
                                else if (AveTypeHelper.IsBasicArray(getMethod.ReturnType) && propValue != null)
                                {
                                    properitesDic[propertyInfo.Name] = AveTypeHelper.CreatGenericList(propValue);
                                }
                                else if (AveTypeHelper.IsBasicType(propValue))
                                {
                                    properitesDic[propertyInfo.Name] = propValue;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!ex.GetType().ToString().Equals("Microsoft.SharePoint.Client.PropertyOrFieldNotInitializedException")
                            && ex.InnerException != null && !ex.InnerException.GetType().ToString().Equals("Microsoft.SharePoint.Client.PropertyOrFieldNotInitializedException"))
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetPropertyError, propertyInfo.Name, ex.ToString());
                        }
                    }
                }
            }
        }

        public static void UpdateObjectBasicProperties(Dictionary<string, object> propertiesDic, object infoObj, string[] properties)
        {
            foreach (string property in properties)
            {
                if (propertiesDic.ContainsKey(property))
                {
                    AveAssemblyUtility.SetFieldValue(infoObj, property, propertiesDic[property]);
                }
            }
        }

        public static void UpdateObjectBasicPropertiesWithEscape(Dictionary<string, object> props, object obj, params string[] escapeProperties)
        {
            int validUpdateCount = 0;
            if (obj != null && props != null)
            {
                Type objType = obj.GetType();

                foreach (KeyValuePair<string, object> kv in props)
                {
                    try
                    {
                        if (!ContainsKeyFromProperties(escapeProperties, kv.Key))
                        {
                            PropertyInfo propertyInfo = objType.GetProperty(kv.Key);
                            if (propertyInfo != null && AveTypeHelper.IsBasicType(propertyInfo.PropertyType))
                            {
                                MethodInfo setMethod = propertyInfo.GetSetMethod();
                                if (setMethod != null)
                                {
                                    object propValue = setMethod.Invoke(obj, new object[] { kv.Value });
                                    ++validUpdateCount;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCUpdateObjectBasicPropertiesError, kv.Key, kv.Value, ex);
                        continue;
                    }
                }
            }
            //props.Add("ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix, validUpdateCount);
            props["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix] = validUpdateCount;
        }

        public static void UpdateObjectBasicProperties(Dictionary<string, object> props, object obj)
        {
            UpdateObjectBasicPropertiesWithEscape(props, obj, new string[] { });
        }

        public static int GetPropertiesWithSetAccessorCount(Dictionary<string, object> props, object obj)
        {
            int count = 0;
            if (obj != null && props != null)
            {
                Type objType = obj.GetType();

                foreach (KeyValuePair<string, object> kv in props)
                {
                    try
                    {
                        PropertyInfo propertyInfo = objType.GetProperty(kv.Key);
                        if (propertyInfo != null && AveTypeHelper.IsBasicType(propertyInfo.PropertyType))
                        {
                            MethodInfo setMethod = propertyInfo.GetSetMethod();
                            if (setMethod == null)
                            {
                                count++;
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetPropertiesError, e.ToString());
                        continue;
                    }
                }
            }
            return count;
        }

        private static bool ContainsKeyFromProperties(string[] properties, string key)
        {
            foreach (string property in properties)
            {
                if (key.Equals(property))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
