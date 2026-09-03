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



//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Xml.Serialization;
//using System.Xml;
//using System.Text;
//using System.Collections;
//using System.Runtime.Serialization;

//namespace AvePoint.Common
//{
//    /// <summary>
//    /// 序列化通用类。该类提供对象以及Dictionary的序列化、反序列化方法。
//    /// </summary>
//    public class XmlUtility
//    {
//        private static Dictionary<Type, XmlSerializer> mSerializers = new Dictionary<Type, XmlSerializer>();
//        private static Queue<MemoryStream> mStreams = new Queue<MemoryStream>();
//        private static XmlSerializerNamespaces NAMESPACE = new XmlSerializerNamespaces();
//        private static XmlWriterSettings SETTINGS = new XmlWriterSettings();
//        private const int DEFAULT_CACHE_SIZE = 10;
//        private static bool mIsCache = false;

//        static XmlUtility()
//        {
//            NAMESPACE.Add("", "");
//            SETTINGS.Indent = false;
//            SETTINGS.OmitXmlDeclaration = true;
//            SETTINGS.Encoding = new System.Text.UTF8Encoding(false);
//            SETTINGS.CheckCharacters = false;
//        }

//        /// <summary>
//        /// Set whether use cache mode.
//        /// If you set this value to true, then we will cache memory that we use.
//        /// We will reuse it when we serialize or deserialize object.
//        /// If you serialize or deserialize objects frequently, set this value to true.
//        /// Default value is false.
//        /// </summary>
//        public static bool IsCache
//        {
//            get { return mIsCache; }
//            set { mIsCache = value; }
//        }

//        /// <summary>
//        /// 用DataContractSerializer序列化IDictionary的方法
//        /// </summary>
//        /// <typeparam name="TKey">IDictionary泛型中的Key值</typeparam>
//        /// <typeparam name="TValue">IDictionary泛型中的Value值</typeparam>
//        /// <param name="obj">需要被序列化的IDictionary</param>
//        /// <returns>返回utf-8的xml字符串</returns>
//        public static string SerializeDictionary<TKey, TValue>(IDictionary<TKey, TValue> obj)
//        {
//            string res = "";

//            if (obj == null || obj.Count == 0)
//            {
//                return "";
//            }
//            else
//            {
//                using (MemoryStream stream = new MemoryStream())
//                {
//                    DataContractSerializer dataSerializer = new DataContractSerializer(typeof(IDictionary<TKey, TValue>));

//                    dataSerializer.WriteObject(stream, obj);

//                    res = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
//                }

//                return res;
//            }
//        }

//        /// <summary>
//        /// 将被DataContractSerializer序列化成xml的字符串, 反序列化成对象
//        /// </summary>
//        /// <typeparam name="TKey">IDictionary泛型中的Key值</typeparam>
//        /// <typeparam name="TValue">IDictionary泛型中的Value值</typeparam>
//        /// <param name="xml">需要被反序列化的字符串</param>
//        /// <returns>IDictionary对象</returns>
//        public static IDictionary<TKey, TValue> DeserializeDictionary<TKey, TValue>(string xml)
//        {
//            if (xml == null)
//            {
//                return null;
//            }
//            else
//            {
//                byte[] bytes = Encoding.Default.GetBytes(xml);

//                return DeserializeDictionary<TKey, TValue>(bytes);
//            }

//        }

//        /// <summary>
//        /// 将被DataContractSerializer序列化成xml的字符串, 反序列化成对象
//        /// </summary>
//        /// <typeparam name="TKey">IDictionary泛型中的Key值</typeparam>
//        /// <typeparam name="TValue">IDictionary泛型中的Value值</typeparam>
//        /// <param name="bytes">需要反序列化的数组</param>
//        /// <returns>IDictionary对象</returns>
//        public static IDictionary<TKey, TValue> DeserializeDictionary<TKey, TValue>(byte[] bytes)
//        {
//            IDictionary<TKey, TValue> deserializedPerson = null;


//            if (bytes == null)
//            {
//                return null;
//            }
//            else
//            {
//                using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(bytes, new XmlDictionaryReaderQuotas()))
//                {
//                    DataContractSerializer ser = new DataContractSerializer(typeof(IDictionary<TKey, TValue>));

//                    deserializedPerson = ser.ReadObject(reader, true) as IDictionary<TKey, TValue>;
//                }
//            }

//            return deserializedPerson;
//        }

//        /// <summary>
//        /// 将被DataContractSerializer序列化成xml的字符串, 反序列化成对象
//        /// </summary>
//        /// <typeparam name="TKey">IDictionary泛型中的Key值</typeparam>
//        /// <typeparam name="TValue">IDictionary泛型中的Value值</typeparam>
//        /// <param name="stream">需要反序列化的流</param>
//        /// <returns>IDictionary对象</returns>
//        public static IDictionary<TKey, TValue> DeserializeDictionary<TKey, TValue>(Stream stream)
//        {
//            IDictionary<TKey, TValue> deserializedPerson = null;

//            if (stream == null)
//            {
//                return null;
//            }
//            else
//            {
//                using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas()))
//                {
//                    DataContractSerializer ser = new DataContractSerializer(typeof(IDictionary<TKey, TValue>));

//                    deserializedPerson = ser.ReadObject(reader, true) as IDictionary<TKey, TValue>;
//                }

//                return deserializedPerson;
//            }
//        }

//        /// <summary>
//        /// 序列化一个对象。
//        /// </summary>
//        /// <param name="obj">要序列化的对象。</param>
//        /// <returns>序列化后的xml。</returns>
//        public static String Serialize(Object obj)
//        {
//            String result = "";
//            MemoryStream stream = null;
//            try
//            {
//                XmlSerializer serializer;
//                Type type = obj.GetType();
//                if (mIsCache)
//                {
//                    lock (mSerializers)
//                    {
//                        if (!mSerializers.TryGetValue(type, out serializer))
//                        {
//                            serializer = new XmlSerializer(type);
//                            mSerializers[type] = serializer;
//                        }
//                    }
//                    lock (mStreams)
//                    {
//                        if (mStreams.Count == 0)
//                        {
//                            stream = new MemoryStream();
//                        }
//                        else
//                        {
//                            stream = mStreams.Dequeue();
//                        }
//                    }
//                }
//                else
//                {
//                    serializer = new XmlSerializer(type);
//                    stream = new MemoryStream();
//                }
//                stream.Position = 0;
//                stream.SetLength(0);
//                XmlWriter writer = XmlWriter.Create(stream, SETTINGS);
//                serializer.Serialize(writer, obj, NAMESPACE);
//                result = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
//            }
//            catch (Exception e)
//            {
//                throw new Exception(string.Format("Serialize object to xml string error. obj:{0}", obj), e);
//            }
//            finally
//            {
//                // We do not cache large stream
//                //
//                //HACK: Close the Memory Stream accordingly and use the DCL design pattern to ENQueue
//                //      the stream queue 
//                if (stream != null)
//                {
//                    if (mIsCache
//                        && stream.Capacity < 2048
//                        && mStreams.Count < DEFAULT_CACHE_SIZE)
//                    {
//                        lock (mStreams)
//                        {
//                            if (mStreams.Count < DEFAULT_CACHE_SIZE)
//                                mStreams.Enqueue(stream);
//                            else stream.Close();
//                        }
//                    }
//                    else stream.Close();
//                }
//            }
//            return result;
//        }

//        /// <summary>
//        /// 反序列化一个对象。
//        /// </summary>
//        /// <param name="xml">要反序列化的xml。</param>
//        /// <param name="type">对象类型。</param>
//        /// <returns>反序列化后的对象。</returns>
//        public static Object Deserialize(String xml, Type type)
//        {
//            if (String.IsNullOrEmpty(xml))
//            {
//                return null;
//            }
//            var result = default(Object);
//            MemoryStream stream = null;
//            try
//            {
//                XmlSerializer serializer;
//                if (mIsCache)
//                {
//                    lock (mSerializers)
//                    {
//                        if (!mSerializers.TryGetValue(type, out serializer))
//                        {
//                            serializer = new XmlSerializer(type);
//                            mSerializers[type] = serializer;
//                        }
//                    }
//                    lock (mStreams)
//                    {
//                        if (mStreams.Count == 0)
//                        {
//                            stream = new MemoryStream();
//                        }
//                        else
//                        {
//                            stream = mStreams.Dequeue();
//                        }
//                    }
//                }
//                else
//                {
//                    serializer = new XmlSerializer(type);
//                    stream = new MemoryStream();
//                }
//                byte[] buf = System.Text.Encoding.UTF8.GetBytes(xml);
//                stream.Position = 0;
//                stream.SetLength(0);
//                stream.Write(buf, 0, buf.Length);
//                stream.Position = 0;
//                result = serializer.Deserialize(stream);
//            }
//            catch (Exception e)
//            {
//                throw new Exception(string.Format("Deserialize xml string to object error. xml:{0}, type:{1}", xml, type), e);
//            }
//            finally
//            {
//                // We do not cache large stream
//                //
//                //HACK: Close the Memory Stream accordingly and use the DCL design pattern to ENQueue
//                //      the stream queue 
//                if (stream != null)
//                {
//                    if (mIsCache
//                        && stream.Capacity < 2048
//                        && mStreams.Count < DEFAULT_CACHE_SIZE)
//                    {
//                        lock (mStreams)
//                        {
//                            if (mStreams.Count < DEFAULT_CACHE_SIZE)
//                                mStreams.Enqueue(stream);
//                            else stream.Close();
//                        }
//                    }
//                    else stream.Close();
//                }
//            }


//            return result;
//        }

//        /// <summary>
//        /// 反序列化一个对象。
//        /// </summary>
//        /// <typeparam name="T">反序列化后的类型。</typeparam>
//        /// <param name="xml">要序列化的xml。</param>
//        /// <returns>反序列化后的对象。</returns>
//        public static T Deserialize<T>(string xml)
//        {
//            return (T)Deserialize(xml, typeof(T));
//        }
//    }
//}
