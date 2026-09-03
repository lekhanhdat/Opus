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
using System.Xml;
using System.Linq;
using System.Collections;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AvePoint.Wrapper.Resource.Common;

namespace AvePoint.Wrapper.Common
{
    public class AveXmlSerializer
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly AveVolatileCache<Type, string> mTypeToStringCache = new AveVolatileCache<Type, string>();
        private static readonly AveVolatileCache<string, Assembly> mAssemblyCache = new AveVolatileCache<string, Assembly>();
        private static readonly AveVolatileCache<string, Type> mStringToTypeCache;
        private static readonly AveReadOnlyDictionary<string, object> mBasicTypes;
        private static readonly Encoding utf8EncodingWithReplacementFallback;

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "C# builtin type.")]
        static AveXmlSerializer()
        {
            Dictionary<string, Type> stringToTypeCache = new Dictionary<string, Type>();
            Dictionary<string, object> basicTypes = new Dictionary<string, object>();

            stringToTypeCache[AveWrapperConstants.TYPE_STRING] = typeof(string);
            stringToTypeCache[AveWrapperConstants.TYPE_BOOL] = typeof(bool);
            stringToTypeCache[AveWrapperConstants.TYPE_BYTE] = typeof(byte);
            stringToTypeCache[AveWrapperConstants.TYPE_CHAR] = typeof(char);
            stringToTypeCache[AveWrapperConstants.TYPE_SHORT] = typeof(short);
            stringToTypeCache[AveWrapperConstants.TYPE_INT] = typeof(int);
            stringToTypeCache[AveWrapperConstants.TYPE_LONG] = typeof(long);
            stringToTypeCache[AveWrapperConstants.TYPE_FLOAT] = typeof(float);
            stringToTypeCache[AveWrapperConstants.TYPE_DOUBLE] = typeof(double);
            stringToTypeCache[AveWrapperConstants.TYPE_DECIMAL] = typeof(decimal);
            stringToTypeCache[AveWrapperConstants.TYPE_URI] = typeof(Uri);
            stringToTypeCache[AveWrapperConstants.TYPE_GUID] = typeof(Guid);
            stringToTypeCache[AveWrapperConstants.TYPE_DATETIME] = typeof(DateTime);
            stringToTypeCache[AveWrapperConstants.TYPE_BINARY] = typeof(byte[]);
            stringToTypeCache[AveWrapperConstants.TYPE_SBYTE] = typeof(sbyte);
            stringToTypeCache[AveWrapperConstants.TYPE_USHORT] = typeof(ushort);
            stringToTypeCache[AveWrapperConstants.TYPE_UINT] = typeof(uint);
            stringToTypeCache[AveWrapperConstants.TYPE_ULONG] = typeof(ulong);

            basicTypes.Add(AveWrapperConstants.TYPE_STRING, null);
            basicTypes.Add(AveWrapperConstants.TYPE_BOOL, null);
            basicTypes.Add(AveWrapperConstants.TYPE_BYTE, null);
            basicTypes.Add(AveWrapperConstants.TYPE_CHAR, null);
            basicTypes.Add(AveWrapperConstants.TYPE_SHORT, null);
            basicTypes.Add(AveWrapperConstants.TYPE_INT, null);
            basicTypes.Add(AveWrapperConstants.TYPE_LONG, null);
            basicTypes.Add(AveWrapperConstants.TYPE_FLOAT, null);
            basicTypes.Add(AveWrapperConstants.TYPE_DOUBLE, null);
            basicTypes.Add(AveWrapperConstants.TYPE_DECIMAL, null);
            basicTypes.Add(AveWrapperConstants.TYPE_GUID, null);
            basicTypes.Add(AveWrapperConstants.TYPE_DATETIME, null);
            basicTypes.Add(AveWrapperConstants.TYPE_BINARY, null);
            basicTypes.Add(AveWrapperConstants.TYPE_SBYTE, null);
            basicTypes.Add(AveWrapperConstants.TYPE_USHORT, null);
            basicTypes.Add(AveWrapperConstants.TYPE_UINT, null);
            basicTypes.Add(AveWrapperConstants.TYPE_ULONG, null);
            basicTypes.Add(AveWrapperConstants.TYPE_URI, null);

            mStringToTypeCache = new AveVolatileCache<string, Type>();
            foreach (KeyValuePair<string, Type> kv in stringToTypeCache)
            {
                mStringToTypeCache[kv.Key] = kv.Value;
            }
            mBasicTypes = new AveReadOnlyDictionary<string, object>(basicTypes);
            utf8EncodingWithReplacementFallback = UTF8Encoding.GetEncoding("UTF-8", new EncoderReplacementFallback(string.Empty), new DecoderReplacementFallback(string.Empty));
            
        }
        /// <summary>
        /// Serialize a object to a xml string.
        /// NOTE: If the object or its descendants contains IDictionary, please
        /// make sure the key of IDictionary is a basic type.
        /// The basic type includes following type:
        /// 1. All primitive types
        /// 2. string, decimal, Uri, Guid, DateTime, byte[]
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="name"></param>
        /// <param name="obj"></param>
        public static void Serialize(XmlWriter writer, string name, object obj)
        {
            try
            {
                SerializeInternal(writer, name, obj, WrapperConfiguration.ZipLevel > 0);
            }
            finally
            {
                writer.Flush();
            }
        }

        private static void SerializeInternal(XmlWriter writer, object name, object obj, bool needCompress, object defaultValue = null)
        {
            if (obj == null)
            {
                return;
            }
            if (defaultValue != null && obj.Equals(defaultValue))
            {
                return;
            }
            if (obj is ICollection && defaultValue is ICollection)
            {
                if ((obj as ICollection).Count == 0 && (defaultValue as ICollection).Count == 0)
                {
                    return;
                }
            }

            obj = TryGetAveRestorablePropertyValue(obj);
            Type type = obj.GetType();
            if (IsBasicType(type) || type.IsEnum)
            {
                WriteBasicType(writer, name, obj, needCompress);
                return;
            }
            writer.WriteStartElement(AveWrapperConstants.COLUMN_ELEMENT);
            try
            {
                WriteKey(writer, name);
                writer.WriteAttributeString(AveWrapperConstants.COLUMN_TYPE, TypeToString(type));
                IEnumerable enumerable = obj as IEnumerable;
                if (enumerable != null)
                {
                    if (needCompress && NeedCompressed(enumerable))
                    {
                        var content = GetCompressedSerializedData(enumerable);
                        writer.WriteString(content);
                    }
                    else
                    {
                        var dictionary = enumerable as IDictionary;
                        if (dictionary != null)
                        {
                            foreach (DictionaryEntry de in dictionary)
                            {
                                SerializeInternal(writer, de.Key, de.Value, needCompress);
                            }
                        }
                        else
                        {
                            foreach (object value in enumerable)
                            {
                                SerializeInternal(writer, null, value, needCompress);
                            }
                        }
                    }
                    return;
                }
                if (defaultValue == null)
                {
                    defaultValue = CreateInstanceByType(type);
                }
                FieldInfo[] fieldInfos = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                foreach (FieldInfo fieldInfo in fieldInfos)
                {
                    SerializeInternal(writer, fieldInfo.Name, fieldInfo.GetValue(obj), needCompress, fieldInfo.GetValue(defaultValue));
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, "An error occurred while writing {0} data. Error:{1}", name.ToString(), ex.ToString());
                throw;
            }
            finally
            {
                writer.WriteEndElement();
            }
        }

        private static void WriteKey(XmlWriter writer, object key)
        {
            if (key != null)
            {
                if (key is string)
                {
                    writer.WriteAttributeString(AveWrapperConstants.COLUMN_NAME, (string)key);
                }
                else
                {
                    string sname;
                    string stype;
                    GetTypeAndValue(key, out sname, out stype);
                    writer.WriteAttributeString(AveWrapperConstants.COLUMN_NAME, sname);
                    writer.WriteAttributeString(AveWrapperConstants.COLUMN_KEY_TYPE, stype);
                }
            }
        }

        private static void WriteBasicType(XmlWriter writer, object key, object value, bool needCompress)
        {
            if (value == null)
            {
                return;
            }
            string svalue;
            string stype;
            GetTypeAndValue(value, out svalue, out stype, needCompress);
            WriteOneElement(writer, key, svalue, stype);
        }

        private static string RemoveSurrogateCharacters(string input, object key)
        {
            using (new AvePerformanceScope("AvePoint.Wrapper.Common.AveXmlSerializer.RemoveSurrogateCharacters"))
            {
                if (!WrapperConfiguration.ReplaceSurrogateChar || string.IsNullOrEmpty(input))
                {
                    return input;
                }
                //如果input string过大, 可能会有内存问题
                string output = utf8EncodingWithReplacementFallback.GetString(utf8EncodingWithReplacementFallback.GetBytes(input));
                if (!string.Equals(input, output))
                {
                    log.Log(AveLogLevel.WARN, "Replace surrogate in char, key:{0}, from:{1}, to:{2}", key, input, output);
                }
                return output;
            }
        }

        private static void WriteOneElement(XmlWriter writer, object key, string value, string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                throw new AveException("Cannot serialize object to xml string without type. Name:{0}, Value:{1}", key, value);
            }
            writer.WriteStartElement(AveWrapperConstants.COLUMN_ELEMENT);
            try
            {
                WriteKey(writer, key);
                writer.WriteAttributeString(AveWrapperConstants.COLUMN_TYPE, type);
                value = RemoveSurrogateCharacters(value, key);
                writer.WriteString(value);
            }
            catch (ArgumentException e)
            {
                //todo:如果可以Log，更多这个key,value关联的信息，对解决问题会更有帮助，但不好实现。
                log.Warn("An error occurred while writing string to xmlWriter, name:{0}, value:{1}, error: {2}", key, value, e.ToString());
                FieldInfo fi = writer.GetType().GetField("currentState", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fi == null)
                {
                    throw;
                }
                FieldInfo encoderFi = writer.GetType().GetField("xmlEncoder", BindingFlags.Instance | BindingFlags.NonPublic);
                if (encoderFi == null)
                {
                    throw;
                }
                MethodInfo mi = encoderFi.FieldType.GetMethod("Write",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(string) },
                    null);
                if (mi == null)
                {
                    throw;
                }
                char[] chars = value.ToCharArray();
                int beginPos = 0;
                for (int i = 0; i < chars.Length; ++i)
                {
                    if (chars[i] >= 0xD800 && chars[i] <= 0xDBFF)
                    {
                        if (chars[i + 1] >= 0xDC00 && chars[i] <= 0xDFFF)
                        {
                            ++i;
                        }
                        else
                        {
                            chars[i] = ' ';
                            if (beginPos == 0)
                            {
                                beginPos = i;
                            }
                        }
                    }
                }
                value = new string(chars, beginPos, chars.Length - beginPos);
                mi.Invoke(encoderFi.GetValue(writer), new object[] { value });
                fi.SetValue(writer, Enum.Parse(fi.FieldType, "Content"));
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, "An error occurred while writing {0} element. Error:{1}", key, ex.ToString());
                throw;
            }
            finally
            {
                writer.WriteEndElement();
            }
        }

        private static void GetTypeAndValue(object value, out string svalue, out string stype, bool needCompress = false)
        {
            Type vtype = value.GetType();
            if (vtype == typeof(string))
            {
                stype = AveWrapperConstants.TYPE_STRING;
                svalue = (string)value;
                if (needCompress && svalue.Length > WrapperConfiguration.AutoZipTriggerSize)
                {
                    var tmpValue = AveWrapperConstants.AUTO_COMPRESSED_STRING_LABEL + Convert.ToBase64String(ZlibUtil.ZipString(svalue, WrapperConfiguration.ZipLevel));
                    if (tmpValue.Length < svalue.Length)
                    {
                        svalue = tmpValue;
                    }
                }
            }
            else if (vtype == typeof(bool))
            {
                stype = AveWrapperConstants.TYPE_BOOL;
                svalue = value.ToString();
            }
            else if (vtype == typeof(byte))
            {
                stype = AveWrapperConstants.TYPE_BYTE;
                svalue = value.ToString();
            }
            else if (vtype == typeof(sbyte))
            {
                stype = AveWrapperConstants.TYPE_SBYTE;
                svalue = value.ToString();
            }
            else if (vtype == typeof(char))
            {
                stype = AveWrapperConstants.TYPE_CHAR;
                svalue = ((int)((char)value)).ToString();
            }
            else if (vtype == typeof(short))
            {
                stype = AveWrapperConstants.TYPE_SHORT;
                svalue = value.ToString();
            }
            else if (vtype == typeof(int))
            {
                stype = AveWrapperConstants.TYPE_INT;
                svalue = value.ToString();
            }
            else if (vtype == typeof(long))
            {
                stype = AveWrapperConstants.TYPE_LONG;
                svalue = value.ToString();
            }
            else if (vtype == typeof(ushort))
            {
                stype = AveWrapperConstants.TYPE_USHORT;
                svalue = value.ToString();
            }
            else if (vtype == typeof(uint))
            {
                stype = AveWrapperConstants.TYPE_UINT;
                svalue = value.ToString();
            }
            else if (vtype == typeof(ulong))
            {
                stype = AveWrapperConstants.TYPE_ULONG;
                svalue = value.ToString();
            }
            else if (vtype == typeof(float))
            {
                stype = AveWrapperConstants.TYPE_FLOAT;
                svalue = ((float)value).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (vtype == typeof(double))
            {
                stype = AveWrapperConstants.TYPE_DOUBLE;
                svalue = ((double)value).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (vtype == typeof(decimal))
            {
                stype = AveWrapperConstants.TYPE_DECIMAL;
                svalue = value.ToString();
            }
            else if (vtype == typeof(Uri))
            {
                stype = AveWrapperConstants.TYPE_URI;
                svalue = ((Uri)value).OriginalString;
            }
            else if (vtype == typeof(Guid))
            {
                stype = AveWrapperConstants.TYPE_GUID;
                svalue = value.ToString();
            }
            else if (vtype == typeof(DateTime))
            {
                stype = AveWrapperConstants.TYPE_DATETIME;
                svalue = ((DateTime)value).Ticks.ToString();
            }
            else if (vtype == typeof(byte[]))
            {
                stype = AveWrapperConstants.TYPE_BINARY;
                svalue = Convert.ToBase64String((byte[])value);
                if (needCompress && ((byte[])value).Length > WrapperConfiguration.AutoZipTriggerSize)
                {
                    var tmpValue = AveWrapperConstants.AUTO_COMPRESSED_BYTES_LABEL + Convert.ToBase64String(ZlibUtil.ZipBytes((byte[])value, WrapperConfiguration.ZipLevel));
                    if (tmpValue.Length < svalue.Length)
                    {
                        svalue = tmpValue;
                    }
                }
            }
            else if (vtype.IsEnum)
            {
                stype = vtype.FullName;
                svalue = value.ToString();
            }
            else
            {
                throw new AveException("The type '{0}' is not a basic type. " +
                    "The basic type includes following type:\r\n1. All primitive types\r\n" +
                    "2. string, decimal, Uri, Guid, DateTime, byte[]", vtype.FullName);
            }
        }

        public static object Deserialize(XmlElement xmlEle)
        {
            return DeserializeInternal(xmlEle, null);
        }

        public static object Deserialize(XmlElement xmlEle, Type type)
        {
            return DeserializeInternal(xmlEle, type);
        }

        public static void Deserialize(XmlElement xmlEle, object value)
        {
            if (value == null)
            {
                return;
            }
            Type type = value.GetType();
            foreach (XmlElement childEle in xmlEle.ChildElements())
            {
                string childName = childEle.GetAttribute(AveWrapperConstants.COLUMN_NAME);
                if (string.IsNullOrEmpty(childName))
                {
                    continue;
                }
                FieldInfo childField = type.GetField(childName);
                if (childField == null)
                {
                    continue;
                }

                childField.SetValue(value, DeserializeInternal(childEle, childField.FieldType));
            }
        }

        /// <summary>
        /// This method is created for performance purpose. It is faster than method
        /// 'object Deserialize(XmlElement xmlEle)'.
        /// NOTE: This method only deserialize basic type. Please also see
        /// 'void Serialize(XmlWriter writer, string name, IDictionary dictionary)'
        /// for reference.
        /// </summary>
        /// <param name="xmlEle"></param>
        /// <param name="dictionary"></param>
        public static void Deserialize(XmlElement xmlEle, IDictionary dictionary)
        {
            DecompressedChildren(xmlEle);
            foreach (XmlElement childEle in xmlEle.ChildNodes.OfType<XmlElement>())
            {
                string keyname = childEle.GetAttribute(AveWrapperConstants.COLUMN_NAME);
                string keytype = childEle.GetAttribute(AveWrapperConstants.COLUMN_KEY_TYPE);
                if (keyname == null)
                {
                    continue;
                }
                object key;
                if (string.IsNullOrEmpty(keytype))
                {
                    key = keyname;
                }
                else
                {
                    key = GetValueFromType(keytype, keyname);
                }
                dictionary[key] = DeserializeInternal(childEle, null);
            }
        }

        private static object GetNullableObject(Type type, XmlElement xmlEle)
        {
            try
            {
                string valueType = xmlEle.Attributes["type"].Value.ToLower(CultureInfo.InvariantCulture);
                object value = xmlEle.InnerText;

                switch (valueType)
                {
                    case "int":
                        return new Nullable<int>(Convert.ToInt32(value));
                    case "long":
                        return new Nullable<long>(Convert.ToInt64(value));
                    case "double":
                        return new Nullable<double>(Convert.ToDouble(value));
                    case "short":
                        return new Nullable<short>(Convert.ToInt16(value));
                    case "datetime":
                        return new Nullable<DateTime>(Convert.ToDateTime(value));
                    case "guid":
                        return new Nullable<Guid>(new Guid(value.ToString()));
                    case "bool":
                        return new Nullable<bool>(Convert.ToBoolean(value));
                    case "uint":
                        return new Nullable<uint>(Convert.ToUInt32(value));
                    default:
                        return null;
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetNullableObjectError, e.ToString());
                return null;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Enqueue: Method name.")]
        private static object DeserializeInternal(XmlElement xmlEle, Type type)
        {
            string stype = xmlEle.GetAttribute(AveWrapperConstants.COLUMN_TYPE);
            if (type == null || (type.Name.Contains("AveRestorableProperty") && !stype.Contains("AveRestorableProperty")))
            {
                type = StringToType(stype);
            }
            if (IsBasicType(type))
            {
                string svalue = xmlEle.InnerText;
                return GetValueFromType(type, svalue);
            }
            if (type.IsEnum)
            {
                string svalue = xmlEle.InnerText;
                return Enum.Parse(type, svalue);
            }
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                DecompressedChildren(xmlEle);
            }
            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                int elementCount = xmlEle.ChildNodes.Count;
                Array array = Array.CreateInstance(elementType, elementCount);
                for (int i = 0; i < elementCount; ++i)
                {
                    array.SetValue(DeserializeInternal((XmlElement)xmlEle.ChildNodes[i], elementType), i);
                }
                return array;
            }
            if (type.IsGenericType)
            {
                if (type.FullName.StartsWith("System.Nullable`1", StringComparison.OrdinalIgnoreCase))
                {
                    return GetNullableObject(type, xmlEle);
                }
                if (type.FullName.StartsWith("System.Collections.Generic.Dictionary`2", StringComparison.OrdinalIgnoreCase))
                {
                    IDictionary dictionary = (IDictionary)CreateInstanceByType(type);
                    Deserialize(xmlEle, dictionary);
                    return dictionary;
                }
                if (type.FullName.StartsWith("System.Collections.Generic.List`1", StringComparison.OrdinalIgnoreCase))
                {
                    IList list = (IList)CreateInstanceByType(type);
                    Type[] argumentTypes = type.GetGenericArguments();
                    foreach (XmlElement childEle in xmlEle.ChildNodes.OfType<XmlElement>())
                    {
                        list.Add(DeserializeInternal(childEle, argumentTypes[0]));
                    }
                    return list;
                }
                if (type.FullName.StartsWith("System.Collections.Generic.Queue`1", StringComparison.OrdinalIgnoreCase))
                {
                    object queue = CreateInstanceByType(type);
                    Type[] argumentTypes = type.GetGenericArguments();
                    MethodInfo enqueueMethod = type.GetMethod("Enqueue", type.GetGenericArguments());
                    if (enqueueMethod != null)
                    {
                        foreach (XmlElement childEle in xmlEle.ChildNodes.OfType<XmlElement>())
                        {
                            enqueueMethod.Invoke(queue, new object[] { DeserializeInternal(childEle, argumentTypes[0]) });
                        }
                    }
                    return queue;
                }
                if (type.FullName.StartsWith("System.Collections.Generic.Stack`1", StringComparison.OrdinalIgnoreCase))
                {
                    object stack = CreateInstanceByType(type);
                    Type[] argumentTypes = type.GetGenericArguments();
                    MethodInfo pushMethod = type.GetMethod("Push", argumentTypes);
                    int childCount = xmlEle.ChildNodes.Count;
                    for (int i = childCount - 1; i >= 0; --i)
                    {
                        pushMethod.Invoke(stack, new object[] { DeserializeInternal((XmlElement)xmlEle.ChildNodes[i], argumentTypes[0]) });
                    }
                    return stack;
                }
                //兼容D5
                if (!string.Equals(xmlEle.GetAttribute(AveWrapperConstants.COLUMN_TYPE).Replace("[", "").Replace("]", ""),
                type.ToString().Replace("[", "").Replace("]", ""), StringComparison.OrdinalIgnoreCase))
                {
                    type = type.GetGenericArguments()[0];
                }
            }
            if (type.GetInterface("System.Collections.IDictionary") != null)
            {
                IDictionary dictionary = (IDictionary)CreateInstanceByType(type);
                Deserialize(xmlEle, dictionary);
                return dictionary;
            }
            if (type.GetInterface("System.Collections.IList") != null)
            {
                IList list = (IList)CreateInstanceByType(type);
                foreach (XmlElement childEle in xmlEle.ChildNodes.OfType<XmlElement>())
                {
                    list.Add(DeserializeInternal(childEle, null));
                }
                return list;
            }
            if (type == typeof(Queue))
            {
                Queue queue = new Queue();
                foreach (XmlElement childEle in xmlEle.ChildNodes.OfType<XmlElement>())
                {
                    queue.Enqueue(DeserializeInternal(childEle, null));
                }
                return queue;
            }
            if (type == typeof(Stack))
            {
                Stack stack = new Stack();
                int childCount = xmlEle.ChildNodes.Count;
                for (int i = childCount - 1; i >= 0; --i)
                {
                    stack.Push(DeserializeInternal((XmlElement)xmlEle.ChildNodes[i], null));
                }
            }
            object childObj = CreateInstanceByType(type);
            foreach (XmlElement childEle in xmlEle.ChildNodes.OfType<XmlElement>())
            {

                string childFieldName = childEle.GetAttribute(AveWrapperConstants.COLUMN_NAME);
                if (string.IsNullOrEmpty(childFieldName))
                {
                    continue;
                }
                FieldInfo fieldInfo = type.GetField(childFieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (fieldInfo == null)
                {
                    continue;
                }
                try
                {
                    string keytype = childEle.GetAttribute(AveWrapperConstants.COLUMN_TYPE);
                    object value = null;
                    if (IsBasicType(keytype))
                    {
                        value = DeserializeInternal(childEle, null);
                    }
                    else
                    {
                        value = DeserializeInternal(childEle, fieldInfo.FieldType);
                    }
                    value = GetValue(fieldInfo.FieldType, value);
                    fieldInfo.SetValue(childObj, value);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCDeSerializingInteralError, e.ToString());
                }
            }
            return childObj;
        }

        private static object GetValue(Type type, object value)
        {
            if (value == null || type.IsAssignableFrom(value.GetType()))
            {
                return value;
            }
            if (type.IsGenericType)
            {
                Type innerType = type.GetGenericArguments()[0];
                value = GetValue(innerType, value);

                var methodInfo = type.GetMethod("op_Implicit", BindingFlags.Static | BindingFlags.Public, null, new Type[] { value.GetType() }, null);
                if (methodInfo == null)
                {
                    methodInfo = type.GetMethod("op_Explicit", BindingFlags.Static | BindingFlags.Public, null, new Type[] { value.GetType() }, null);
                }
                if (methodInfo != null)
                {
                    return methodInfo.Invoke(null, new object[] { value });
                }
            }
            return value;//maybe should throw here.
        }

        private static string TypeToString(Type type)
        {
            if (!mTypeToStringCache.ContainsKey(type))
            {
                mTypeToStringCache[type] = TypeToStringInternal(type);
            }
            return mTypeToStringCache[type];
        }

        private static string TypeToStringInternal(Type type)
        {
            if (!type.IsGenericType)
            {
                return type.FullName;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(type.Namespace).Append(".");
            sb.Append(type.Name);
            Type[] genericArguments = type.GetGenericArguments();
            if (genericArguments != null && genericArguments.Length > 0)
            {
                sb.Append("[");
                for (int i = 0; i < genericArguments.Length; ++i)
                {
                    sb.Append("[");
                    sb.Append(TypeToStringInternal(genericArguments[i]));
                    if (i == genericArguments.Length - 1)
                    {
                        sb.Append("]");
                    }
                    else
                    {
                        sb.Append("],");
                    }
                }
                sb.Append("]");
            }
            return sb.ToString();
        }

        private static Type StringToType(string typeString)
        {
            if (!mStringToTypeCache.ContainsKey(typeString))
            {
                mStringToTypeCache[typeString] = StringToTypeInternal(typeString);
            }
            return mStringToTypeCache[typeString];
        }

        private static Type StringToTypeInternal(string typeName)
        {
            Type type = Type.GetType(typeName);
            if (type != null)
            {
                return type;
            }
            if (mAssemblyCache.Count == 0)
            {
                mAssemblyCache["System"] = typeof(List<string>).Assembly;
                mAssemblyCache["System.Core"] = typeof(Queue<string>).Assembly;
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (!mAssemblyCache.ContainsKey(assembly.GetName().Name))
                    {
                        mAssemblyCache[assembly.GetName().Name] = assembly;
                    }
                }
            }
            if (typeName.Contains("["))
            {
                int paramIndex = typeName.IndexOf('`');
                int startPos = typeName.IndexOf('[');
                int paramCount = int.Parse(typeName.Substring(paramIndex + 1, startPos - paramIndex - 1).Trim());

                int paramStart = startPos + 1;
                StringBuilder sb = new StringBuilder();
                sb.Append(typeName.Substring(0, paramStart));
                while (true)
                {
                    --paramCount;
                    int paramEnd = typeName.IndexOf(']', paramStart + 1);
                    int count = 0;
                    int i = paramStart;
                    while (true)
                    {
                        i = typeName.IndexOf('[', i + 1);
                        if (i < 0 || i > paramEnd)
                        {
                            break;
                        }
                        ++count;
                    }
                    int j = paramEnd;
                    while (count > 0)
                    {
                        j = typeName.IndexOf(']', j + 1);
                        --count;
                    }
                    string subTypeName = typeName.Substring(paramStart + 1, j - paramStart - 1);
                    Type subType = StringToType(subTypeName);
                    sb.Append("[").Append(subType.AssemblyQualifiedName).Append("]");
                    if (paramCount != 0)
                    {
                        sb.Append(",");
                        paramStart = typeName.IndexOf('[', j + 1);
                    }
                    else
                    {
                        sb.Append("]");
                        break;
                    }
                }
                typeName = sb.ToString();
                type = Type.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }
            foreach (Assembly assmbly in mAssemblyCache.Values)
            {
                type = assmbly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }
            throw new AveException("Could not load type '{0}' from assembly.", typeName);
        }

        private static bool SetFieldValue(object obj, string fieldName, object fieldValue)
        {
            FieldInfo fieldInfo = obj.GetType().GetField(fieldName);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, fieldValue);
                return true;
            }
            return false;
        }

        private static object CreateInstanceByType(Type type)
        {
            ConstructorInfo[] ss = type.GetConstructors(); //is ss usefull??
            ConstructorInfo constrInfo = type.GetConstructor(new Type[0]);
            if (constrInfo == null)
            {
                throw new AveException("Cannot find default constructor for type:{0}", type.FullName);
            }
            return constrInfo.Invoke(new object[0]);
        }

        private static object GetValueFromType(string stype, string svalue)
        {
            switch (stype)
            {
                case AveWrapperConstants.TYPE_STRING: return svalue;
                case AveWrapperConstants.TYPE_BOOL: return bool.Parse(svalue);
                case AveWrapperConstants.TYPE_BYTE: return byte.Parse(svalue);
                case AveWrapperConstants.TYPE_CHAR: return (char)int.Parse(svalue);
                case AveWrapperConstants.TYPE_SHORT: return short.Parse(svalue);
                case AveWrapperConstants.TYPE_INT: return int.Parse(svalue);
                case AveWrapperConstants.TYPE_LONG: return long.Parse(svalue);
                case AveWrapperConstants.TYPE_FLOAT:
                    try
                    {
                        return float.Parse(svalue, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, "An error occurred while getting value for the float type.Error:{0}", e.ToString());
                        return TryParseWithCurrentInfoForFloat(svalue);
                    }
                case AveWrapperConstants.TYPE_DOUBLE:
                    try
                    {
                        return double.Parse(svalue, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, "An error occurred while getting value for the double type.Error:{0}", e.ToString());
                        return TryParseWithCurrentInfoForDouble(svalue);
                    }
                case AveWrapperConstants.TYPE_DECIMAL: return decimal.Parse(svalue);
                case AveWrapperConstants.TYPE_GUID: return new Guid(svalue);
                case AveWrapperConstants.TYPE_DATETIME: return new DateTime(long.Parse(svalue));
                case AveWrapperConstants.TYPE_BINARY: return Convert.FromBase64String(svalue);
                case AveWrapperConstants.TYPE_SBYTE: return sbyte.Parse(svalue);
                case AveWrapperConstants.TYPE_USHORT: return ushort.Parse(svalue);
                case AveWrapperConstants.TYPE_UINT: return uint.Parse(svalue);
                case AveWrapperConstants.TYPE_ULONG: return ulong.Parse(svalue);
                case AveWrapperConstants.TYPE_URI: return new Uri(svalue);
                default: throw new AveException("Unknown data type:{0}", stype);
            }
        }

        private static bool IsBasicType(string type)
        {
            return mBasicTypes.ContainsKey(type);
        }

        private static object GetValueFromType(Type vtype, string svalue)
        {
            if (vtype == typeof(string))
            {
                if (svalue != null && svalue.StartsWith(AveWrapperConstants.AUTO_COMPRESSED_STRING_LABEL, StringComparison.OrdinalIgnoreCase))
                {
                    svalue = ZlibUtil.UnZipString(Convert.FromBase64String(svalue.Substring(AveWrapperConstants.AUTO_COMPRESSED_STRING_LABEL.Length)));
                }
                return svalue;
            }
            else if (vtype == typeof(bool))
            {
                return bool.Parse(svalue);
            }
            else if (vtype == typeof(byte))
            {
                return byte.Parse(svalue);
            }
            else if (vtype == typeof(sbyte))
            {
                return sbyte.Parse(svalue);
            }
            else if (vtype == typeof(char))
            {
                return (char)int.Parse(svalue);
            }
            else if (vtype == typeof(short))
            {
                return short.Parse(svalue);
            }
            else if (vtype == typeof(int))
            {
                return int.Parse(svalue);
            }
            else if (vtype == typeof(long))
            {
                return long.Parse(svalue);
            }
            else if (vtype == typeof(ushort))
            {
                return ushort.Parse(svalue);
            }
            else if (vtype == typeof(uint))
            {
                return uint.Parse(svalue);
            }
            else if (vtype == typeof(ulong))
            {
                return ulong.Parse(svalue);
            }
            else if (vtype == typeof(float))
            {
                try
                {
                    return float.Parse(svalue, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "An error occurred while getting value for the float type.Error:{0}", e.ToString());
                    return TryParseWithCurrentInfoForFloat(svalue);
                }
            }
            else if (vtype == typeof(double))
            {
                try
                {
                    return double.Parse(svalue, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "An error occurred while getting value for the double type.Error:{0}", e.ToString());
                    return TryParseWithCurrentInfoForDouble(svalue);
                }
            }
            else if (vtype == typeof(decimal))
            {
                return decimal.Parse(svalue);
            }
            else if (vtype == typeof(Uri))
            {
                return new Uri(svalue);
            }
            else if (vtype == typeof(Guid))
            {
                return new Guid(svalue);
            }
            else if (vtype == typeof(DateTime))
            {
                return new DateTime(long.Parse(svalue));
            }
            else if (vtype == typeof(byte[]))
            {
                if (svalue != null && svalue.StartsWith(AveWrapperConstants.AUTO_COMPRESSED_BYTES_LABEL, StringComparison.OrdinalIgnoreCase))
                {
                    return ZlibUtil.UnZipBytes(Convert.FromBase64String(svalue.Substring(AveWrapperConstants.AUTO_COMPRESSED_BYTES_LABEL.Length)));
                }
                return Convert.FromBase64String(svalue);
            }
            else
            {
                throw new AveException("The type '{0}' is not a basic type. " +
                    "The basic type includes following type:\r\n1. All primitive types\r\n" +
                    "2. string, decimal, Uri, Guid, DateTime, byte[]", vtype.FullName);
            }
        }

        private static object TryParseWithCurrentInfoForDouble(string svalue)
        {
            //1036  为法语，法语中‘，’为小数点。此处为了兼容老数据，老数据没有指定语言
            if (svalue.Contains(","))
            {
                return double.Parse(svalue, new System.Globalization.CultureInfo(1036));
            }
            else
            {
                return double.Parse(svalue, System.Globalization.CultureInfo.CurrentCulture);
            }

        }

        private static object TryParseWithCurrentInfoForFloat(string svalue)
        {
            //1036  为法语，法语中‘，’为小数点。此处为了兼容老数据，老数据没有指定语言
            if (svalue.Contains(","))
            {
                return float.Parse(svalue, new System.Globalization.CultureInfo(1036));
            }
            else
            {
                return float.Parse(svalue, System.Globalization.CultureInfo.CurrentCulture);
            }
        }

        private static bool IsBasicType(Type type)
        {
            return (type.IsPrimitive ||
                type == typeof(string) ||
                type == typeof(decimal) ||
                type == typeof(Guid) ||
                type == typeof(DateTime) ||
                type == typeof(byte[]) ||
                type == typeof(Uri));
        }

        private static bool NeedCompressed(IEnumerable enumerable)
        {
            if (enumerable is List<int>)
            {
                return (enumerable as ICollection).Count > WrapperConfiguration.AutoZipTriggerSize;
            }
            return true;
        }

        private static string GetCompressedSerializedData(IEnumerable list)
        {
            int size = 1;
            if (list is ICollection)
            {
                size = (list as ICollection).Count;
            }
            if (size == 0) return null;
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(size * (1 << 8)))
            using (XmlTextWriter writer = new XmlTextWriter(stream, new UTF8Encoding(false)))
            {
                writer.WriteStartElement(AveWrapperConstants.COLUMN_ELEMENT);
                try
                {
                    var dictionary = list as IDictionary;
                    if (dictionary != null)
                    {
                        foreach (DictionaryEntry de in dictionary)
                        {
                            //只需要最后压缩一次即可，内部不用压缩了
                            SerializeInternal(writer, de.Key, de.Value, false);
                        }
                    }
                    else
                    {
                        foreach (object value in list)
                        {
                            SerializeInternal(writer, null, value, false);
                        }
                    }
                }
                finally
                {
                    writer.WriteEndElement();
                    writer.Flush();
                }
                var tmpBuffer = stream.GetBuffer();
                var emptyRoot = Encoding.UTF8.GetBytes(string.Format("<{0}></{0}>", AveWrapperConstants.COLUMN_ELEMENT));
                var rootHeader = Encoding.UTF8.GetBytes(string.Format("<{0}>", AveWrapperConstants.COLUMN_ELEMENT));
                if (stream.Length <= emptyRoot.Length)
                {
                    return null;
                }
                var buffer = new byte[stream.Length - emptyRoot.Length];
                Array.Copy(tmpBuffer, rootHeader.Length, buffer, 0, buffer.Length);
                return AveWrapperConstants.AUTO_COMPRESSED_CHILDREN_LABEL + Convert.ToBase64String(ZlibUtil.ZipBytes(buffer, WrapperConfiguration.ZipLevel));
            }
        }

        private static object TryGetAveRestorablePropertyValue(object obj)
        {
            if (obj != null)
            {
                var type = obj.GetType();
                if (type.FullName.StartsWith("AvePoint.Wrapper.Common.AveRestorableProperty", StringComparison.OrdinalIgnoreCase))
                {
                    var availableField = type.GetField("mIsAvailable", BindingFlags.Instance | BindingFlags.NonPublic);
                    if ((bool)availableField.GetValue(obj))
                    {
                        var valueField = type.GetField("mValue", BindingFlags.Instance | BindingFlags.NonPublic);
                        var realValue = valueField.GetValue(obj);
                        if (realValue != null)
                        {
                            return realValue;
                        }
                    }
                }

            }
            return obj;
        }

        public static void DecompressedChildren(XmlElement element)
        {
            var innerText = element.InnerText;
            if (innerText.StartsWith(AveWrapperConstants.AUTO_COMPRESSED_CHILDREN_LABEL, StringComparison.OrdinalIgnoreCase))
            {
                var content = ZlibUtil.UnZipString(Convert.FromBase64String(innerText.Substring(AveWrapperConstants.AUTO_COMPRESSED_CHILDREN_LABEL.Length)));
                element.InnerXml = content;
            }
        }
    }
}
