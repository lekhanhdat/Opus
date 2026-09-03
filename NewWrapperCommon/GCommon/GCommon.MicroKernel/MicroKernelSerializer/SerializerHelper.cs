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



namespace AvePoint.GCommon.MicroKernel
{
    #region using directives
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Xml;
    #endregion

    #region Attributes
    /// <summary>
    /// Support class for serializing and deserializing workflow data such
    /// as association or task data. The method must use in pair.
    ///
    /// Changing list
    ///
    /// *********************************************************************************
    ///  2011-12-01   Baron
    ///  To avoid the memory issue of large object, we add a improvement of changing the
    ///  default way of data contract serializer, we remove the base64 convert and change
    ///  the serializer format from xml text writer(which is the default behavior of data contract
    ///  serializer) toe xml binary writer, via the binary writer, we almost save half of memory
    ///  use of large object.
    ///  <code>
    ///     var xmlBinaryWriter = XmlDictionaryWriter.CreateBinaryWriter(stream);
    ///     var dataContractSerializer = new DataContractSerializer(graph.GetType());
    ///     dataContractSerializer.WriteObject(xmlBinaryWriter, graph);
    ///     xmlBinaryWriter.Close();
    ///  </code>
    ///
    /// </summary>
    [DebuggerNonUserCode]
    #endregion 

    internal static class SerializerHelper
    {
        /// <summary>
        /// Serialize an object graph to an string of base64 encoding
        /// </summary>
        /// <param name="graph">the object graph is an object instance</param>
        /// <returns>the base64 string of the object graph</returns>
        public static String SerializeToBase64StringByDataContractSerializer(Object graph)
        {
            using (var stream = new MemoryStream())
            {
                var dataContractSerializer = new DataContractSerializer(graph.GetType());
                dataContractSerializer.WriteObject(stream, graph);
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        /// <summary>
        /// Deserialize the data instance which represented by buffer to an object
        /// </summary>
        /// <param name="data">the object in byte String format</param>
        /// <param name="type">the object data type</param>
        /// <returns>the object instance</returns>
        public static Object DeserializeFromBase64StringByDataContractSerializer(String data, Type type)
        {
            var content = Convert.FromBase64String(data);
            using (var stream = new MemoryStream(content))
            {
                var dataContractSerializer = new DataContractSerializer(type);
                return dataContractSerializer.ReadObject(stream);
            }
        }

        /// <summary>
        /// Serialize an object to byte array, this method use the xml binary writer
        /// to compress data size which obey the docave's standard way
        /// </summary>
        /// <param name="graph">the object graph instance which to be serialized</param>
        /// <returns>the </returns>
        public static Byte[] SerializeToBytesByDataContractSerializer(Object graph)
        {
            using (var stream = new MemoryStream())
            {
                var xmlBinaryWriter = XmlDictionaryWriter.CreateBinaryWriter(stream);
                var dataContractSerializer = new DataContractSerializer(graph.GetType());
                dataContractSerializer.WriteObject(xmlBinaryWriter, graph);
                xmlBinaryWriter.Close();
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Deserialize the data instance which represented by buffer to an object
        /// </summary>
        /// <param name="buffer">the object in byte array format</param>
        /// <param name="type">the object data type</param>
        /// <returns>the object instance</returns>
        public static Object DeserializeFromBytesByDataContractSerializer(Byte[] buffer, Type type)
        {
            var xmlBinaryReader = XmlDictionaryReader.CreateBinaryReader(buffer, XmlDictionaryReaderQuotas.Max);
            var dataContractSerializer = new DataContractSerializer(type);
            var result = dataContractSerializer.ReadObject(xmlBinaryReader);
            xmlBinaryReader.Close();
            return result;
        }
    }
}