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
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using AvePoint.Wrapper.Common;

namespace AvePoint.Item.Restore
{
    public static class AveXmlUtility
    {
        private static readonly Dictionary<Type, XmlSerializer> mSerializers = new Dictionary<Type, XmlSerializer>();
        private static readonly Queue<MemoryStream> mStreams = new Queue<MemoryStream>();
        private static readonly XmlSerializerNamespaces NAMESPACE = new XmlSerializerNamespaces();
        private static readonly XmlWriterSettings SETTINGS = new XmlWriterSettings();
        private const int DEFAULT_CACHE_SITE = 10;
        private static bool mIsCache;

        static AveXmlUtility()
        {
            NAMESPACE.Add("", "");
            SETTINGS.Indent = false;
            SETTINGS.OmitXmlDeclaration = true;
            SETTINGS.Encoding = new UTF8Encoding(false);
            SETTINGS.CheckCharacters = false;
        }

        /// <summary>
        /// Set whether use cache mode.
        /// If you set this value to true, then we will cache memory that we use.
        /// We will reuse it when we serialize or deserialize object.
        /// If you serialize or deserialize objects frequently, set this value to true.
        /// Default value is false.
        /// </summary>
        public static bool IsCache
        {
            get { return mIsCache; }
            set { mIsCache = value; }
        }

        public static string Serialize(object obj)
        {
            MemoryStream stream = null;
            try
            {
                XmlSerializer serializer;
                Type type = obj.GetType();
                if (mIsCache)
                {
                    lock (mSerializers)
                    {
                        if (!mSerializers.TryGetValue(type, out serializer))
                        {
                            serializer = new XmlSerializer(type);
                            mSerializers[type] = serializer;
                        }
                    }
                    lock (mStreams)
                    {
                        stream = mStreams.Count == 0 ? new MemoryStream() : mStreams.Dequeue();
                    }
                }
                else
                {
                    serializer = new XmlSerializer(type);
                    stream = new MemoryStream();
                }
                stream.Position = 0;
                stream.SetLength(0);
                XmlWriter writer = XmlWriter.Create(stream, SETTINGS);
                serializer.Serialize(writer, obj, NAMESPACE);
                return Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int) stream.Length);
            }
            catch (Exception e)
            {
                throw new AveXmlException(e, "Serializate object to xml string error. obj:{0}", obj);
            }
            finally
            {
                // We do not cache large stream
                if (mIsCache && stream != null && stream.Capacity < 2048 && mStreams.Count < DEFAULT_CACHE_SITE)
                {
                    lock (mStreams)
                    {
                        mStreams.Enqueue(stream);
                    }
                }
            }
        }

        public static T Deserialize<T>(string xml)
        {
            MemoryStream stream = null;
            Type type = typeof (T);
            try
            {
                XmlSerializer serializer;
                if (mIsCache)
                {
                    lock (mSerializers)
                    {
                        if (!mSerializers.TryGetValue(type, out serializer))
                        {
                            serializer = new XmlSerializer(type);
                            mSerializers[type] = serializer;
                        }
                    }
                    lock (mStreams)
                    {
                        stream = mStreams.Count == 0 ? new MemoryStream() : mStreams.Dequeue();
                    }
                }
                else
                {
                    serializer = new XmlSerializer(type);
                    stream = new MemoryStream();
                }
                byte[] buf = Encoding.UTF8.GetBytes(xml);
                stream.Position = 0;
                stream.SetLength(0);
                stream.Write(buf, 0, buf.Length);
                stream.Position = 0;
                return (T) serializer.Deserialize(stream);
            }
            catch (Exception e)
            {
                throw new AveXmlException(e, "Deserialize xml string to object error. xml:{0}, type:{1}", xml, type);
            }
            finally
            {
                // We do not cache large stream
                if (mIsCache && stream != null && stream.Capacity < 2048 && mStreams.Count < DEFAULT_CACHE_SITE)
                {
                    lock (mStreams)
                    {
                        mStreams.Enqueue(stream);
                    }
                }
            }
        }
    }
}
