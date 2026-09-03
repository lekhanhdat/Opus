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

namespace AvePoint.Metadata
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    using AvePoint.RA.CommonUtil;

    public class AveXmlSerializer
    {
        private static RALogger logger = RALogger.GetInstance(typeof(AveXmlSerializer));

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

            stringToTypeCache[AveMetadataConstants.TYPE_STRING] = typeof(string);
            stringToTypeCache[AveMetadataConstants.TYPE_BOOL] = typeof(bool);
            stringToTypeCache[AveMetadataConstants.TYPE_BYTE] = typeof(byte);
            stringToTypeCache[AveMetadataConstants.TYPE_CHAR] = typeof(char);
            stringToTypeCache[AveMetadataConstants.TYPE_SHORT] = typeof(short);
            stringToTypeCache[AveMetadataConstants.TYPE_INT] = typeof(int);
            stringToTypeCache[AveMetadataConstants.TYPE_LONG] = typeof(long);
            stringToTypeCache[AveMetadataConstants.TYPE_FLOAT] = typeof(float);
            stringToTypeCache[AveMetadataConstants.TYPE_DOUBLE] = typeof(double);
            stringToTypeCache[AveMetadataConstants.TYPE_DECIMAL] = typeof(decimal);
            stringToTypeCache[AveMetadataConstants.TYPE_URI] = typeof(Uri);
            stringToTypeCache[AveMetadataConstants.TYPE_GUID] = typeof(Guid);
            stringToTypeCache[AveMetadataConstants.TYPE_DATETIME] = typeof(DateTime);
            stringToTypeCache[AveMetadataConstants.TYPE_BINARY] = typeof(byte[]);
            stringToTypeCache[AveMetadataConstants.TYPE_SBYTE] = typeof(sbyte);
            stringToTypeCache[AveMetadataConstants.TYPE_USHORT] = typeof(ushort);
            stringToTypeCache[AveMetadataConstants.TYPE_UINT] = typeof(uint);
            stringToTypeCache[AveMetadataConstants.TYPE_ULONG] = typeof(ulong);

            basicTypes.Add(AveMetadataConstants.TYPE_STRING, null);
            basicTypes.Add(AveMetadataConstants.TYPE_BOOL, null);
            basicTypes.Add(AveMetadataConstants.TYPE_BYTE, null);
            basicTypes.Add(AveMetadataConstants.TYPE_CHAR, null);
            basicTypes.Add(AveMetadataConstants.TYPE_SHORT, null);
            basicTypes.Add(AveMetadataConstants.TYPE_INT, null);
            basicTypes.Add(AveMetadataConstants.TYPE_LONG, null);
            basicTypes.Add(AveMetadataConstants.TYPE_FLOAT, null);
            basicTypes.Add(AveMetadataConstants.TYPE_DOUBLE, null);
            basicTypes.Add(AveMetadataConstants.TYPE_DECIMAL, null);
            basicTypes.Add(AveMetadataConstants.TYPE_GUID, null);
            basicTypes.Add(AveMetadataConstants.TYPE_DATETIME, null);
            basicTypes.Add(AveMetadataConstants.TYPE_BINARY, null);
            basicTypes.Add(AveMetadataConstants.TYPE_SBYTE, null);
            basicTypes.Add(AveMetadataConstants.TYPE_USHORT, null);
            basicTypes.Add(AveMetadataConstants.TYPE_UINT, null);
            basicTypes.Add(AveMetadataConstants.TYPE_ULONG, null);
            basicTypes.Add(AveMetadataConstants.TYPE_URI, null);

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
                SerializeInternal(writer, name, obj);
            }
            finally
            {
                writer.Flush();
            }
        }

        /// <summary>
        /// This method is created for performace purpose. Usually, we use reflection to
        /// serialize fileds and the performace is not very good. If it is required high
        /// performace serialize method, please use this method.
        /// NOTO: This method only serializes basic type and the key of the dictionary
        /// must be a basic type.
        /// The basic type includes following type:
        /// 1. All primitive types
        /// 2. string, decimal, Uri, Guid, DateTime, byte[]
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="name"></param>
        /// <param name="dictionary"></param>
        public static void Serialize(XmlWriter writer, string name, IDictionary dictionary)
        {
            Serialize(writer, name, dictionary as object);
        }

        private static void SerializeInternal(XmlWriter writer, object name, object obj)
        {
            if (obj == null)
            {
                return;
            }

            Type type = obj.GetType();
            if (IsBasicType(type) || type.IsEnum)
            {
                WriteBasicType(writer, name, obj);
                return;
            }
            writer.WriteStartElement(AveMetadataConstants.COLUMN_ELEMENT);
            try
            {
                if (name != null)
                {
                    if (name is string)
                    {
                        writer.WriteAttributeString(AveMetadataConstants.COLUMN_NAME, (string)name);
                    }
                    else
                    {
                        string sname;
                        string stype;
                        GetTypeAndValue(name, out sname, out stype);
                        writer.WriteAttributeString(AveMetadataConstants.COLUMN_NAME, sname);
                        writer.WriteAttributeString(AveMetadataConstants.COLUMN_KEY_TYPE, stype);
                    }
                }
                writer.WriteAttributeString(AveMetadataConstants.COLUMN_TYPE, TypeToString(type));
                IDictionary dictionary = obj as IDictionary;
                if (dictionary != null)
                {
                    foreach (DictionaryEntry entry in dictionary)
                    {
                        SerializeInternal(writer, entry.Key, entry.Value);
                    }
                    return;
                }
                IEnumerable enumerable = obj as IEnumerable;
                if (enumerable != null)
                {
                    foreach (object childValue in enumerable)
                    {
                        SerializeInternal(writer, null, childValue);
                    }
                    return;
                }

                FieldInfo[] fieldInfos = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                foreach (FieldInfo fieldInfo in fieldInfos)
                {
                    SerializeInternal(writer, fieldInfo.Name, fieldInfo.GetValue(obj));
                }
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while writing {0} data. Error:{1}", name?.ToString(), ex.ToString());
                throw;
            }
            finally
            {
                writer.WriteEndElement();
            }
        }

        private static void WriteBasicType(XmlWriter writer, object key, object value)
        {
            if (value == null)
            {
                return;
            }
            string svalue;
            string stype;
            GetTypeAndValue(value, out svalue, out stype);
            if (key == null)
            {
                WriteOneElement(writer, null, stype, svalue, null);
            }
            else if (key is string)
            {
                WriteOneElement(writer, (string)key, stype, svalue, null);
            }
            else
            {
                string skeyname;
                string skeytype;
                GetTypeAndValue(key, out skeyname, out skeytype);
                WriteOneElement(writer, skeyname, stype, svalue, skeytype);
            }
        }

        private static string RemoveSurrogateCharacters(string input, object key)
        {
            //if (!WrapperConfiguration.ReplaceSurrogateChar || string.IsNullOrEmpty(input))
            //{
            //    return input;
            //}
            //如果input string过大, 可能会有内存问题
            string output = utf8EncodingWithReplacementFallback.GetString(utf8EncodingWithReplacementFallback.GetBytes(input));
            if (!string.Equals(input, output))
            {
                logger.Warn("Replace surrogate in char, key:{0}, from:{1}, to:{2}", key.ToString(), input, output);
            }
            return output;
        }

        private static void WriteOneElement(XmlWriter writer, string key, string type, string value, string keyType)
        {
            if (string.IsNullOrEmpty(type))
            {
                throw new Exception($"Cannot serialize object to xml string without type. Name:{key}, Value:{value}");
            }
            writer.WriteStartElement(AveMetadataConstants.COLUMN_ELEMENT);
            try
            {
                if (key != null)
                {
                    writer.WriteAttributeString(AveMetadataConstants.COLUMN_NAME, key);
                }
                if (keyType != null)
                {
                    writer.WriteAttributeString(AveMetadataConstants.COLUMN_KEY_TYPE, keyType);
                }
                writer.WriteAttributeString(AveMetadataConstants.COLUMN_TYPE, type);
                value = RemoveSurrogateCharacters(value, key);
                if (value != null)
                {
                    writer.WriteString(value);
                }
            }
            catch (ArgumentException e)
            {
                logger.Warn("An error occurred while writing string to xmlWriter, name:{0}, value:{1}, error: {2}", key, value, e.ToString());
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
                if (mi == null || value == null)
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
            finally
            {
                writer.WriteEndElement();
            }
        }

        private static void GetTypeAndValue(object value, out string svalue, out string stype)
        {
            Type vtype = value.GetType();
            if (vtype == typeof(string))
            {
                stype = AveMetadataConstants.TYPE_STRING;
                svalue = (string)value;
            }
            else if (vtype == typeof(bool))
            {
                stype = AveMetadataConstants.TYPE_BOOL;
                svalue = value.ToString();
            }
            else if (vtype == typeof(byte))
            {
                stype = AveMetadataConstants.TYPE_BYTE;
                svalue = value.ToString();
            }
            else if (vtype == typeof(sbyte))
            {
                stype = AveMetadataConstants.TYPE_SBYTE;
                svalue = value.ToString();
            }
            else if (vtype == typeof(char))
            {
                stype = AveMetadataConstants.TYPE_CHAR;
                svalue = ((int)((char)value)).ToString();
            }
            else if (vtype == typeof(short))
            {
                stype = AveMetadataConstants.TYPE_SHORT;
                svalue = value.ToString();
            }
            else if (vtype == typeof(int))
            {
                stype = AveMetadataConstants.TYPE_INT;
                svalue = value.ToString();
            }
            else if (vtype == typeof(long))
            {
                stype = AveMetadataConstants.TYPE_LONG;
                svalue = value.ToString();
            }
            else if (vtype == typeof(ushort))
            {
                stype = AveMetadataConstants.TYPE_USHORT;
                svalue = value.ToString();
            }
            else if (vtype == typeof(uint))
            {
                stype = AveMetadataConstants.TYPE_UINT;
                svalue = value.ToString();
            }
            else if (vtype == typeof(ulong))
            {
                stype = AveMetadataConstants.TYPE_ULONG;
                svalue = value.ToString();
            }
            else if (vtype == typeof(float))
            {
                stype = AveMetadataConstants.TYPE_FLOAT;
                svalue = value.ToString();
            }
            else if (vtype == typeof(double))
            {
                stype = AveMetadataConstants.TYPE_DOUBLE;
                svalue = value.ToString();
            }
            else if (vtype == typeof(decimal))
            {
                stype = AveMetadataConstants.TYPE_DECIMAL;
                svalue = value.ToString();
            }
            else if (vtype == typeof(Uri))
            {
                stype = AveMetadataConstants.TYPE_GUID;
                svalue = ((Uri)value).OriginalString;
            }
            else if (vtype == typeof(Guid))
            {
                stype = AveMetadataConstants.TYPE_GUID;
                svalue = value.ToString();
            }
            else if (vtype == typeof(DateTime))
            {
                stype = AveMetadataConstants.TYPE_DATETIME;
                svalue = ((DateTime)value).Ticks.ToString();
            }
            else if (vtype == typeof(byte[]))
            {
                stype = AveMetadataConstants.TYPE_BINARY;
                svalue = Convert.ToBase64String((byte[])value);
            }
            else if (vtype.IsEnum)
            {
                stype = vtype.FullName;
                svalue = value.ToString();
            }
            else
            {
                throw new Exception($"The type '{vtype.FullName}' is not a basic type. " +
                    "The basic type includes following type:\r\n1. All primitive types\r\n" +
                    "2. string, decimal, Uri, Guid, DateTime, byte[]");
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

        public static object Deserialize(string path, Type type = null)
        {
            using (var stream = File.OpenRead(path))
            using (XmlReader reader = XmlReader.Create(stream))
            {
                reader.Read();
                return DeserializeInternal(reader, type);
            }
        }

        private static object DeserializeInternal(XmlReader reader, Type type)
        {
            bool isMovedToNext;
            return DeserializeInternal(reader, type, out isMovedToNext);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Enqueue: Method name.")]
        private static object DeserializeInternal(XmlReader reader, Type type, out bool isMovedToNext)
        {
            isMovedToNext = false;
            if (reader.NodeType != XmlNodeType.Element)
            {
                reader.ReadNextElement();
            }
            reader.ReadAttributeValue();
            if (type == null)
            {
                string stype = reader.GetAttribute(AveMetadataConstants.COLUMN_TYPE);
                type = StringToType(stype);
            }
            if (IsBasicType(type))
            {
                string svalue = reader.ReadElementContentAsString();
                isMovedToNext = true;
                return GetValueFromType(type, svalue);
            }
            if (type.IsEnum)
            {
                string svalue = reader.ReadElementContentAsString();
                isMovedToNext = true;
                return Enum.Parse(type, svalue);
            }
            if (type.IsArray)
            {
                /*neede consider*/
                Type elementType = type.GetElementType();
                var arraySubReader = reader.ReadSubtree();
                arraySubReader.Read();
                ArrayList list = new ArrayList();
                bool isArrayItemMoved = false;
                while (isArrayItemMoved || arraySubReader.ReadNextElement())
                {
                    if (!NeedContinue(arraySubReader))
                    {
                        isArrayItemMoved = false;
                        break;
                    }
                    var obj = DeserializeInternal(arraySubReader, elementType, out isArrayItemMoved);
                    list.Add(obj);
                }
                isMovedToNext = isArrayItemMoved;

                Array array = Array.CreateInstance(elementType, list.Count);
                for (int k = 0; k < list.Count; k++)
                {
                    array.SetValue(list[k], k);
                }
                list.Clear();
                list = null;
                return array;
            }
            if (type.IsGenericType)
            {
                if (type.FullName.StartsWith("System.Nullable`1", StringComparison.OrdinalIgnoreCase))
                {
                    isMovedToNext = true;
                    return GetNullableObject(type, reader);
                }
                if (type.FullName.StartsWith("System.Collections.Generic.Dictionary`2", StringComparison.OrdinalIgnoreCase))
                {
                    IDictionary dictionary = (IDictionary)CreateInstanceByType(type);
                    Type[] argumentTypes = type.GetGenericArguments();
                    for (int i = 0; i < argumentTypes.Length; ++i)
                    {
                        if (argumentTypes[i] == typeof(object))
                        {
                            argumentTypes[i] = null;
                        }
                    }

                    var dicSubReader = reader.ReadSubtree();
                    dicSubReader.Read();
                    bool isDicItemMovedNext = false;
                    while (isDicItemMovedNext || dicSubReader.ReadNextElement())
                    {
                        if (!NeedContinue(dicSubReader))
                        {
                            isDicItemMovedNext = false;
                            break;
                        }
                        string keyname = dicSubReader.GetAttribute(AveMetadataConstants.COLUMN_NAME);
                        if (keyname == null)
                        {
                            continue;
                        }
                        object key = GetValueFromType(argumentTypes[0], keyname);
                        var obj = DeserializeInternal(dicSubReader, argumentTypes[1], out isDicItemMovedNext);
                        dictionary[key] = obj;
                    }
                    isMovedToNext = isDicItemMovedNext;
                    return dictionary;
                }
                if (type.FullName.StartsWith("System.Collections.Generic.List`1", StringComparison.OrdinalIgnoreCase))
                {
                    IList list = (IList)CreateInstanceByType(type);
                    Type[] argumentTypes = type.GetGenericArguments();

                    var listSubReader = reader.ReadSubtree();
                    listSubReader.Read();
                    bool isListReadNext = false;
                    while (isListReadNext || listSubReader.ReadNextElement())
                    {
                        if (!NeedContinue(listSubReader))
                        {
                            isListReadNext = false;
                            break;
                        }
                        var obj = DeserializeInternal(listSubReader, argumentTypes[0], out isListReadNext);
                        list.Add(obj);
                    }
                    isMovedToNext = isListReadNext;
                    return list;
                }
                if (type.FullName.StartsWith("System.Collections.Generic.Queue`1", StringComparison.OrdinalIgnoreCase))
                {
                    object queue = CreateInstanceByType(type);
                    Type[] argumentTypes = type.GetGenericArguments();
                    MethodInfo enqueueMethod = type.GetMethod("Enqueue", type.GetGenericArguments());
                    if (enqueueMethod != null)
                    {
                        var queueSubReader = reader.ReadSubtree();
                        queueSubReader.Read();
                        bool isQueueMovedNext = false;
                        while (isQueueMovedNext || queueSubReader.ReadNextElement())
                        {
                            if (!NeedContinue(queueSubReader))
                            {
                                isQueueMovedNext = false;
                                break;
                            }
                            var obj = DeserializeInternal(queueSubReader, argumentTypes[0], out isQueueMovedNext);
                            enqueueMethod.Invoke(queue, new object[] { obj });
                        }
                        isMovedToNext = isQueueMovedNext;
                    }
                    return queue;
                }
                if (type.FullName.StartsWith("System.Collections.Generic.Stack`1", StringComparison.OrdinalIgnoreCase))
                {
                    object tempStack = CreateInstanceByType(type);
                    Type[] argumentTypes = type.GetGenericArguments();
                    MethodInfo pushMethod = type.GetMethod("Push", argumentTypes);
                    MethodInfo popMethod = type.GetMethod("Pop", argumentTypes);

                    var stackTSubReader = reader.ReadSubtree();
                    stackTSubReader.Read();
                    bool isStackMovedNext = false;
                    while (isStackMovedNext || stackTSubReader.ReadNextElement())
                    {
                        if (!NeedContinue(stackTSubReader))
                        {
                            isStackMovedNext = false;
                            break;
                        }
                        var obj = DeserializeInternal(stackTSubReader, argumentTypes[0], out isStackMovedNext);
                        pushMethod.Invoke(tempStack, new object[] { obj, argumentTypes[0] });
                    }
                    isMovedToNext = isStackMovedNext;
                    int count = (int)type.GetProperty("Count").GetValue(tempStack);
                    object stack = CreateInstanceByType(type);
                    for (int k = 0; k < count; k++)
                    {
                        var obj = popMethod.Invoke(tempStack, new object[] { });
                        pushMethod.Invoke(stack, new object[] { obj, argumentTypes[0] });
                    }

                    return stack;
                }
                //no need any more,兼容D5
                //if (!string.Equals(xmlEle.GetAttribute(AveWrapperConstants.COLUMN_TYPE).Replace("[", "").Replace("]", ""),
                //type.ToString().Replace("[", "").Replace("]", ""), StringComparison.OrdinalIgnoreCase))
                //{
                //    type = type.GetGenericArguments()[0];
                //}
            }
            if (type.GetInterface("System.Collections.IDictionary") != null)
            {
                IDictionary dictionary = (IDictionary)CreateInstanceByType(type);

                var idicSubReader = reader.ReadSubtree();
                idicSubReader.Read();
                bool isIdicMovedNext = false;
                while (isIdicMovedNext || idicSubReader.ReadNextElement())
                {
                    if (!NeedContinue(idicSubReader))
                    {
                        isIdicMovedNext = false;
                        break;
                    }
                    string keyname = idicSubReader.GetAttribute(AveMetadataConstants.COLUMN_NAME);
                    string keytype = idicSubReader.GetAttribute(AveMetadataConstants.COLUMN_KEY_TYPE);
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
                    var obj = DeserializeInternal(idicSubReader, null, out isIdicMovedNext);
                    dictionary[key] = obj;
                }
                isMovedToNext = isIdicMovedNext;
                return dictionary;
            }
            if (type.GetInterface("System.Collections.IList") != null)
            {
                IList list = (IList)CreateInstanceByType(type);
                var ilistSubReader = reader.ReadSubtree();
                ilistSubReader.Read();
                bool isListMovedNext = false;
                while (isListMovedNext || ilistSubReader.ReadNextElement())
                {
                    if (!NeedContinue(ilistSubReader))
                    {
                        isListMovedNext = false;
                        break;
                    }
                    var obj = DeserializeInternal(ilistSubReader, null, out isListMovedNext);
                    list.Add(obj);
                }
                isMovedToNext = isListMovedNext;
                return list;
            }
            if (type == typeof(Queue))
            {
                Queue queue = new Queue();
                var queueSubReader = reader.ReadSubtree();
                queueSubReader.Read();
                bool isQueueMovedNext = false;
                while (isQueueMovedNext || queueSubReader.ReadNextElement())
                {
                    if (!NeedContinue(queueSubReader))
                    {
                        isQueueMovedNext = false;
                        break;
                    }
                    var obj = DeserializeInternal(queueSubReader, null, out isQueueMovedNext);
                    queue.Enqueue(obj);
                }
                isMovedToNext = isQueueMovedNext;
                return queue;
            }
            if (type == typeof(Stack))
            {
                Stack tempStack = new Stack();
                var stackSubReader = reader.ReadSubtree();
                stackSubReader.Read();
                bool isStackMovedNext = false;
                while (isStackMovedNext || stackSubReader.ReadNextElement())
                {
                    if (!NeedContinue(stackSubReader))
                    {
                        isStackMovedNext = false;
                        break;
                    }
                    var obj = DeserializeInternal(stackSubReader, null, out isStackMovedNext);
                    tempStack.Push(obj);
                }
                isMovedToNext = isStackMovedNext;
                Stack stack = new Stack();
                while (tempStack.Count > 0)
                {
                    var obj = tempStack.Pop();
                    stack.Push(obj);
                }
                return stack;
            }
            object childObj = CreateInstanceByType(type);

            var subReader = reader.ReadSubtree();
            subReader.Read();
            bool isNextObjReaded = false;
            while (isNextObjReaded || subReader.ReadNextElement())
            {
                if (!NeedContinue(subReader))
                {
                    isNextObjReaded = false;
                    break;
                }
                if (subReader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                string childFieldName = subReader.GetAttribute(AveMetadataConstants.COLUMN_NAME);
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
                    string keytype = subReader.GetAttribute(AveMetadataConstants.COLUMN_TYPE);
                    object value = null;
                    if (IsBasicType(keytype))
                    {
                        value = DeserializeInternal(subReader, null, out isNextObjReaded);
                    }
                    else
                    {
                        value = DeserializeInternal(subReader, fieldInfo.FieldType, out isNextObjReaded);
                    }
                    value = GetValue(fieldInfo.FieldType, value);
                    fieldInfo.SetValue(childObj, value);
                }
                catch (Exception e)
                {
                    logger.Debug("An error occurred while deserializering to internal.Exception:{0}", e.ToString());
                }
            }
            isMovedToNext = isNextObjReaded;
            return childObj;
        }

        private static bool NeedContinue(XmlReader reader)
        {
            if (reader.NodeType == XmlNodeType.EndElement)
            {
                return false;
            }
            return true;
        }

        public static void Deserialize(XmlElement xmlEle, object value)
        {
            if (value == null)
            {
                return;
            }
            Type type = value.GetType();
            foreach (XmlElement childEle in xmlEle.ChildNodes)
            {
                string childName = childEle.GetAttribute(AveMetadataConstants.COLUMN_NAME);
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
            foreach (XmlElement childEle in xmlEle.ChildNodes)
            {
                string keyname = childEle.GetAttribute(AveMetadataConstants.COLUMN_NAME);
                string keytype = childEle.GetAttribute(AveMetadataConstants.COLUMN_KEY_TYPE);
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
                string svalue = childEle.InnerText;
                string stype = childEle.GetAttribute(AveMetadataConstants.COLUMN_TYPE);
                try
                {
                    dictionary[key] = GetValueFromType(stype, svalue);
                }
                catch (Exception) { }
            }
        }

        private static object GetNullableObject(Type type, XmlElement xmlEle)
        {
            try
            {
                string valueType = xmlEle.Attributes["type"].Value.ToLower();
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
                logger.Debug("An error occurred while getting nullable object.Exception:{0}", e.ToString());
                return null;
            }
        }

        private static object GetNullableObject(Type type, XmlReader xmlEle)
        {
            try
            {
                string valueType = xmlEle.GetAttribute("type");
                object value = xmlEle.ReadContentAsObject();

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
                logger.Debug("An error occurred while getting nullable object.Exception:{0}", e.ToString());
                return null;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Enqueue: Method name.")]
        private static object DeserializeInternal(XmlElement xmlEle, Type type)
        {
            if (type == null)
            {
                string stype = xmlEle.GetAttribute(AveMetadataConstants.COLUMN_TYPE);
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
            if (type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?))
            {
                if (xmlEle.ChildNodes?.Count == 2)
                {
                    var xmlNodes = xmlEle.ChildNodes.Cast<XmlNode>();
                    var ticks = long.Parse(xmlNodes.First(i => i.OuterXml.Contains("_dateTime")).InnerText);
                    var offset = new TimeSpan(0, int.Parse(xmlNodes.First(i => i.OuterXml.Contains("_offsetMinutes")).InnerText), 0);
                    return new DateTimeOffset(ticks, offset);
                }
                return null;
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
                    Type[] argumentTypes = type.GetGenericArguments();
                    for (int i = 0; i < argumentTypes.Length; ++i)
                    {
                        if (argumentTypes[i] == typeof(object))
                        {
                            argumentTypes[i] = null;
                        }
                    }
                    foreach (XmlElement childEle in xmlEle.ChildNodes.OfType<XmlElement>())
                    {
                        string keyname = childEle.GetAttribute(AveMetadataConstants.COLUMN_NAME);
                        if (keyname == null)
                        {
                            continue;
                        }
                        object key = GetValueFromType(argumentTypes[0], keyname);
                        dictionary[key] = DeserializeInternal(childEle, argumentTypes[1]);
                    }
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
                if (!string.Equals(xmlEle.GetAttribute(AveMetadataConstants.COLUMN_TYPE).Replace("[", "").Replace("]", ""),
                type.ToString().Replace("[", "").Replace("]", ""), StringComparison.OrdinalIgnoreCase))
                {
                    type = type.GetGenericArguments()[0];
                }
            }
            if (type.GetInterface("System.Collections.IDictionary") != null)
            {
                IDictionary dictionary = (IDictionary)CreateInstanceByType(type);
                foreach (XmlElement childEle in xmlEle.ChildNodes.OfType<XmlElement>())
                {
                    string keyname = childEle.GetAttribute(AveMetadataConstants.COLUMN_NAME);
                    string keytype = childEle.GetAttribute(AveMetadataConstants.COLUMN_KEY_TYPE);
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
                string childFieldName = childEle.GetAttribute(AveMetadataConstants.COLUMN_NAME);
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
                    string keytype = childEle.GetAttribute(AveMetadataConstants.COLUMN_TYPE);
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
                    logger.Debug("An error occurred while deserializering to internal.Exception:{0}", e.ToString());
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
            throw new Exception($"Could not load type '{typeName}' from assembly.");
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
                throw new Exception($"Cannot find default constructor for type:{type.FullName}");
            }
            return constrInfo.Invoke(new object[0]);
        }

        private static object GetValueFromType(string stype, string svalue)
        {
            switch (stype)
            {
                case AveMetadataConstants.TYPE_STRING:
                case AveMetadataConstants.TYPE_SYSTEM_STRING:
                    return svalue;

                case AveMetadataConstants.TYPE_BOOL: return bool.Parse(svalue);
                case AveMetadataConstants.TYPE_BYTE: return byte.Parse(svalue);
                case AveMetadataConstants.TYPE_CHAR: return (char)int.Parse(svalue);
                case AveMetadataConstants.TYPE_SHORT: return short.Parse(svalue);
                case AveMetadataConstants.TYPE_INT: return int.Parse(svalue);
                case AveMetadataConstants.TYPE_LONG: return long.Parse(svalue);
                case AveMetadataConstants.TYPE_FLOAT: return float.Parse(svalue);
                case AveMetadataConstants.TYPE_DOUBLE: return double.Parse(svalue);
                case AveMetadataConstants.TYPE_DECIMAL: return decimal.Parse(svalue);
                case AveMetadataConstants.TYPE_GUID: return new Guid(svalue);
                case AveMetadataConstants.TYPE_DATETIME: return new DateTime(long.Parse(svalue));
                case AveMetadataConstants.TYPE_BINARY: return Convert.FromBase64String(svalue);
                case AveMetadataConstants.TYPE_SBYTE: return sbyte.Parse(svalue);
                case AveMetadataConstants.TYPE_USHORT: return ushort.Parse(svalue);
                case AveMetadataConstants.TYPE_UINT: return uint.Parse(svalue);
                case AveMetadataConstants.TYPE_ULONG: return ulong.Parse(svalue);
                case AveMetadataConstants.TYPE_URI: return new Uri(svalue);
                default: throw new Exception($"Unknown data type:{stype}");
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
                return float.Parse(svalue);
            }
            else if (vtype == typeof(double))
            {
                return double.Parse(svalue);
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
                return new DateTime(long.Parse(svalue), DateTimeKind.Utc);
            }
            else if (vtype == typeof(byte[]))
            {
                return Convert.FromBase64String(svalue);
            }
            else
            {
                throw new Exception($"The type '{vtype.FullName}' is not a basic type. " +
                    "The basic type includes following type:\r\n1. All primitive types\r\n" +
                    "2. string, decimal, Uri, Guid, DateTime, byte[]");
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
    }
}