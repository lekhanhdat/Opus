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




namespace AvePoint.GCommon.Utility
{
    #region using directives
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using StjJsonSerializer = System.Text.Json.JsonSerializer;
    using StjJsonSerializerOptions = System.Text.Json.JsonSerializerOptions;

    #endregion

    /// <summary>
    /// Support class for serializing and deserializing workflow data such
    /// as association or task data. The method must use in pair.
    /// </summary>
    public static class SerializerHelper
    {
        static AveLogger logger = AveLogger.GetInstance(typeof(SerializerHelper));

        /// <summary>
        /// Marker prefixed to every string produced by the <see cref="System.Text.Json"/>
        /// based Base64 serialization. It lets the deserializer distinguish new JSON data
        /// from legacy <c>BinaryFormatter</c> payloads. The '|' character is never part of
        /// the Base64 alphabet, so a legacy (pure Base64) string can never be mistaken for
        /// a marked one.
        /// </summary>
        private const string JsonFormatMarker = "JSONv1|";

        /// <summary>
        /// Separator placed between the embedded assembly-qualified type name and the
        /// Base64 encoded UTF-8 JSON value. It is not part of the Base64 alphabet and
        /// never appears in an assembly-qualified type name.
        /// </summary>
        private const char TypeSeparator = '|';

        /// <summary>
        /// Options used to replace <c>BinaryFormatter</c>-based serialization with
        /// <see cref="System.Text.Json"/>. Fields are included since the types previously
        /// serialized via <c>BinaryFormatter</c> may expose data through fields rather
        /// than properties.
        /// </summary>
        private static readonly StjJsonSerializerOptions BinaryReplacementJsonOptions = new StjJsonSerializerOptions
        {
            IncludeFields = true
        };

        /// <summary>
        /// Serializes an object using <see cref="System.Text.Json"/> 
        /// into a Base64 encoded string. The runtime type is embedded alongside the
        /// serialized value so it can be restored by
        /// <see cref="DeserializeFromBase64String{TData}(String)"/>, preserving the
        /// interchangeable, polymorphic behavior of the previous BinaryFormatter logic.
        /// </summary>
        /// <param name="data">The data to serialize.</param>
        /// <typeparam name="TData">The type of data to process.</typeparam>
        /// <returns>A Base64 encoded string containing serialized 
        /// data.</returns>
        public static String SerializeToBase64String<TData>(TData data)
        {
            if (data == null)
                return JsonFormatMarker + TypeSeparator.ToString();

            Type type = data.GetType();
            byte[] bytes = StjJsonSerializer.SerializeToUtf8Bytes(data, type, BinaryReplacementJsonOptions);
            return JsonFormatMarker + type.AssemblyQualifiedName + TypeSeparator + Convert.ToBase64String(bytes);
        }

        /// </summary>
        /// <param name="data">The data to serialize.</param>
        /// <typeparam name="TData">The type of data to process.</typeparam>
        /// <returns>A Base64 encoded string containing serialized 
        /// data.</returns>
        public static String SerializeToBase64StringBinaryFunction<TData>(TData data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, data);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static T Copy<T>(T data)
        {
            string json = SerializeByJsonConvert(data);
            return DeserializeByJsonConvert<T>(json);
        }

        /// <summary>
        /// Serializes an object using <see cref="System.Text.Json"/> 
        /// into a Base64 encoded string. Since the runtime type is not known
        /// by the caller of <see cref="DeserializeFromBase64String(String)"/>, the
        /// object's runtime type name is embedded in the marker prefix so it can be
        /// restored on deserialization while the value itself is produced by
        /// <see cref="System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(Object, Type, StjJsonSerializerOptions)"/>.
        /// </summary>
        /// <param name="data">The data to serialize.</param>
        /// <typeparam name="TData">The type of data to process.</typeparam>
        /// <returns>A Base64 encoded string containing serialized 
        /// data.</returns>
        public static String SerializeToBase64String(Object data)
        {
            if (data == null)
                return JsonFormatMarker + TypeSeparator.ToString();

            Type type = data.GetType();
            byte[] bytes = StjJsonSerializer.SerializeToUtf8Bytes(data, type, BinaryReplacementJsonOptions);
            return JsonFormatMarker + type.AssemblyQualifiedName + TypeSeparator + Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Deserializes an object from a Base64 encoded string 
        /// using <see cref="System.Text.Json"/>.
        /// </summary>
        /// <param name="data">The Base64 encoded data to deserialize.</param>
        /// <typeparam name="TData">The type of data to process.</typeparam>
        /// <returns>The deserialized object.</returns>
        public static TData DeserializeFromBase64String<TData>(String data)
        {
            return (TData)DeserializeFromBase64String(data);
        }

        /// <summary>
        /// Deserializes an object from a Base64 encoded string 
        /// using <see cref="System.Text.Json"/>. The runtime type is restored from
        /// the type name embedded during <see cref="SerializeToBase64String(Object)"/>.
        /// Strings without the <see cref="JsonFormatMarker"/> are treated as legacy
        /// <c>BinaryFormatter</c> payloads and read through the legacy path so that data
        /// produced before this migration can still be loaded.
        /// </summary>
        /// <param name="data">The Base64 encoded data to deserialize.</param>
        /// <typeparam name="TData">The type of data to process.</typeparam>
        /// <returns>The deserialized object.</returns>
        public static Object DeserializeFromBase64String(String data)
        {
            if (data == null)
                return null;

            if (!data.StartsWith(JsonFormatMarker, StringComparison.Ordinal))
            {
                // Legacy data written by the old BinaryFormatter based build.
                return DeserializeLegacyBinaryFormatterString(data);
            }

            string payload = data.Substring(JsonFormatMarker.Length);
            int separatorIndex = payload.IndexOf(TypeSeparator);
            if (separatorIndex < 0)
                return null;

            string typeName = payload.Substring(0, separatorIndex);
            if (String.IsNullOrEmpty(typeName))
                return null;

            string base64Value = payload.Substring(separatorIndex + 1);
            byte[] content = Convert.FromBase64String(base64Value);
            Type type = Type.GetType(typeName, throwOnError: true);
            return StjJsonSerializer.Deserialize(content, type, BinaryReplacementJsonOptions);
        }

        /// <summary>
        /// Deserializes a legacy Base64 encoded string that was produced by the old
        /// <see cref="BinaryFormatter"/> based implementation. This path is read-only and
        /// exists solely for backward compatibility with data persisted before the
        /// migration to <see cref="System.Text.Json"/>. New data is never written in this
        /// format.
        /// </summary>
        /// <param name="data">The legacy Base64 encoded data to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        private static Object DeserializeLegacyBinaryFormatterString(String data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] content = Convert.FromBase64String(data);
                ms.Write(content, 0, content.Length);
                ms.Position = 0;
                BinaryFormatter formatter = new BinaryFormatter();
                return formatter.Deserialize(ms);
            }
        }

        /// <summary>
        /// Serializes an object using the <see cref="XmlSerializer" /> 
        /// into an XML string.
        /// </summary>
        /// <param name="data">The data to serialize.</param>
        /// <typeparam name="TData">The type of data to process.</typeparam>
        /// <returns>An XML string containing serialized 
        /// data.
        /// </returns>
        public static String SerializeToXmlString<TData>(TData data)
        {
            StringBuilder serializedData = new StringBuilder();
            using (StringWriter writer = new EncodingStringWriter(Encoding.UTF8, serializedData, CultureInfo.InvariantCulture))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TData));
                serializer.Serialize(writer, data);
            }
            return serializedData.ToString();
        }

        /// <summary>
        /// Serializes an object using the <see cref="XmlSerializer" /> 
        /// into an XML string.
        /// </summary>
        /// <param name="data">The data to serialize.</param>
        /// <typeparam name="TData">The type of data to process.</typeparam>
        /// <returns>An XML string containing serialized 
        /// data.
        /// </returns>
        public static String SerializeToXmlString(Object data)
        {
            StringBuilder serializedData = new StringBuilder();
            using (StringWriter writer = new EncodingStringWriter(Encoding.UTF8, serializedData, CultureInfo.InvariantCulture))
            {
                XmlSerializer serializer = new XmlSerializer(data.GetType());
                serializer.Serialize(writer, data);
            }
            return serializedData.ToString();
        }

        /// <summary>
        /// Deserializes an object from an XML string  
        /// using the <see cref="XmlSerializer" />.
        /// </summary>
        /// <param name="data">The XML data to Deserializes.</param>
        /// <typeparam name="TData">The type of data to process.</typeparam>
        /// <returns>The deserialized object.</returns>        
        public static TData DeserializeFromXmlString<TData>(String data)
        {
            using (StringReader reader = new StringReader(data))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TData));
                return (TData)serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Deserializes an object from an XML string  
        /// using the <see cref="XmlSerializer" />.
        /// </summary>
        /// <param name="data">The XML data to Deserializes.</param>
        /// <param name="Type">The type of data to process.</typeparam>
        /// <returns>The deserialized object.</returns> 
        public static Object DeserializeFromXmlString(String data, Type type)
        {
            using (StringReader reader = new StringReader(data))
            {
                XmlSerializer serializer = new XmlSerializer(type);
                return serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Serializes an object using the <see cref="XmlSerializer" /> 
        /// into an XML string.
        /// </summary>
        /// <param name="data">The data to serialize.</param>
        /// <typeparam name="TData">The type of data to process.</typeparam>
        /// <returns>An XML string containing serialized 
        /// data.
        /// </returns>
        public static String SerializeToXmlStringWithoutDecalaring<TData>(TData data)
        {
            String result = SerializerHelper.SerializeToXmlString<TData>(data);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);
            return doc.DocumentElement.OuterXml;
        }

        /// <summary>
        /// Serializes an object using the <see cref="XmlSerializer" /> 
        /// into an XML string.
        /// </summary>
        /// <param name="data">The data to serialize.</param>
        /// <typeparam name="TData">The type of data to process.</typeparam>
        /// <returns>An XML string containing serialized 
        /// data.
        /// </returns>
        public static String SerializeToXmlStringWithoutDecalaring(Object data)
        {
            String result = SerializerHelper.SerializeToXmlString(data);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);
            return doc.DocumentElement.OuterXml;
        }

        /// <summary>
        /// Serializes an object using the <see cref="XmlSerializer" /> 
        /// into an XML string.
        /// </summary>
        /// <param name="data">The data to serialize.</param>
        /// <typeparam name="TData">The type of data to process.</typeparam>
        /// <returns>An XML string containing serialized 
        /// data.
        /// </returns>
        public static Object DeserializeToXmlStringWithoutDecalaring(String data, Type type)
        {
            return DeserializeFromXmlString(data, type);
        }

        /// <summary>
        /// Deserializes an object from an XML string  
        /// using the <see cref="XmlSerializer" />.
        /// </summary>
        /// <param name="data">The XML data to Deserializes.</param>
        /// <typeparam name="TData">The type of data to process.</typeparam>
        /// <returns>The deserialized object.</returns>        
        public static TData DeserializeFromXmlStringWithoutDecalaring<TData>(String data)
        {
            return SerializerHelper.DeserializeFromXmlString<TData>(data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static String SerializeToBase64StringByDataContractSerializer(Object data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var dataContractSerializer = new DataContractSerializer(data.GetType());
                dataContractSerializer.WriteObject(ms, data);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        /// <summary>
        /// make sure the correct type of the data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static String SerializeToBase64StringByDataContractSerializer(Object data, Type type)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var dataContractSerializer = new DataContractSerializer(type);
                dataContractSerializer.WriteObject(ms, data);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        /// <summary>
        /// 利用DataContractSerializer进行序列化
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static String SerializeByDataContractSerializer(Object data)
        {
            var serializedStringBuilder = new StringBuilder();
            using (var encodingStringWriter = new EncodingStringWriter(Encoding.UTF8, serializedStringBuilder, CultureInfo.InvariantCulture))
            {
                using (XmlTextWriter writer = new XmlTextWriter(encodingStringWriter))
                {
                    var dataContractSerializer = new DataContractSerializer(data.GetType());
                    dataContractSerializer.WriteObject(writer, data);
                    return serializedStringBuilder.ToString();
                }
            }
        }

        /// <summary>
        /// make the correct type of the data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static String SerializeByDataContractSerializer(Object data, Type type)
        {
            var serializedStringBuilder = new StringBuilder();
            using (var encodingStringWriter = new EncodingStringWriter(Encoding.UTF8, serializedStringBuilder, CultureInfo.InvariantCulture))
            {
                using (XmlTextWriter writer = new XmlTextWriter(encodingStringWriter))
                {
                    var dataContractSerializer = new DataContractSerializer(type);
                    dataContractSerializer.WriteObject(writer, data);
                    return serializedStringBuilder.ToString();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Object DeserializeFromBase64StringByDataContractSerializer(String data, Type type)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] content = Convert.FromBase64String(data);
                ms.Write(content, 0, content.Length);
                ms.Position = 0;
                var dataContractSerializer = new DataContractSerializer(type);
                return dataContractSerializer.ReadObject(ms);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T DeserializeFromBase64StringByDataContractSerializer<T>(String data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] content = Convert.FromBase64String(data);
                ms.Write(content, 0, content.Length);
                ms.Position = 0;
                var dataContractSerializer = new DataContractSerializer(typeof(T));
                return (T)dataContractSerializer.ReadObject(ms);
            }
        }

        /// <summary>
        /// 利用DataContractSerializer进行反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T DeserializeByDataContractSerializer<T>(string data)
        {
            using (var stringReader = new StringReader(data))
            {
                using (XmlTextReader reader = new XmlTextReader(stringReader))
                {
                    var dataContractSerializer = new DataContractSerializer(typeof(T));
                    return (T)dataContractSerializer.ReadObject(reader);
                }
            }
        }

        /// <summary>
        /// 利用DataContractSerializer进行反序列化
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Object DeserializeByDataContractSerializer(string data, Type type)
        {
            using (var stringReader = new StringReader(data))
            {
                using (var xmlTextReader = new XmlTextReader(stringReader))
                {
                    var dataContractSerializer = new DataContractSerializer(type);
                    return dataContractSerializer.ReadObject(xmlTextReader);
                }
            }
        }

        /// <summary>
        /// 利用JsonSerializer进行序列化
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreDataMember"></param>
        /// <returns></returns>
        public static string SerializeByJsonSerializer(Object data, bool serializeWithReference = false)
        {
            var settings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = serializeWithReference ? Newtonsoft.Json.ReferenceLoopHandling.Serialize : Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = serializeWithReference ? PreserveReferencesHandling.Objects : PreserveReferencesHandling.None,
                TypeNameHandling = TypeNameHandling.None,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            return JsonConvert.SerializeObject(data, settings);
        }

        /// <summary>
        /// 利用JsonSerializer进行反序列化
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreDataMember"></param>
        /// <returns></returns>
        public static T DeserializeByJsonSerializer<T>(string data/*, bool ignoreDataMember = true*/, bool serializeWithReference = false)
        {
            try
            {
                int intData = 0;
                if (data.StartsWith("{", StringComparison.Ordinal) || data.StartsWith("[", StringComparison.Ordinal)
                    || data.StartsWith("\"", StringComparison.Ordinal) || int.TryParse(data, out intData))
                {
                    var settings = new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = serializeWithReference ? Newtonsoft.Json.ReferenceLoopHandling.Serialize : Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                        PreserveReferencesHandling = serializeWithReference ? PreserveReferencesHandling.Objects : PreserveReferencesHandling.None,
                        TypeNameHandling = TypeNameHandling.None
                    };
                    //if (ignoreDataMember) { settings.ContractResolver = AveContractResolver.Instance; }
                    return JsonConvert.DeserializeObject<T>(data, settings);
                }
                else
                {
                    return DeserializeByDataContractSerializer<T>(data);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("An exception occurred on deserialize data: {0}", ex.Message);
                return DeserializeByDataContractSerializer<T>(data);
            }
        }

        /// <summary>
        /// 利用JsonSerializer进行序列化
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string SerializeByDataContractJsonSerializer(Object data)
        {
            try
            {
                DataContractJsonSerializer json = new DataContractJsonSerializer(data.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    json.WriteObject(stream, data);
                    return Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            catch (Exception ex)
            {
                logger.Warn("An exception occurred on datacontract json serialize data: {0}", ex.Message);
                return SerializeByDataContractSerializer(data);
            }
        }

        /// <summary>
        /// 利用JsonSerializer进行反序列化
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T DeserializeByDataContractJsonSerializer<T>(string data)
        {
            try
            {
                DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(T));
                using (MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data)))
                {
                    return (T)json.ReadObject(stream);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("An exception occurred on deserialize data: {0}", ex.Message);
                return DeserializeByDataContractSerializer<T>(data);
            }
        }

        /// <summary>
        /// 利用Newtonsoft.Json.dll中JsonConvert进行序列化
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreDataMember"></param>
        /// <returns></returns>
        public static string SerializeByJsonConvert(Object data, bool ignoreException = false)
        {
            try
            {
                var settings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };
                return JsonConvert.SerializeObject(data, settings);
            }
            catch(Exception ex)
            {
                if (ignoreException)
                {
                    logger.Warn("An exception occurred on json convert serialize data: {0}", ex.Message);
                    return string.Empty;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 利用Newtonsoft.Json.dll中JsonConvert进行反序列化
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreDataMember"></param>
        /// <returns></returns>
        public static T DeserializeByJsonConvert<T>(string data/*, bool ignoreDataMember = true*/)
        {
            try
            {
                var settings = new JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore };
                //if (ignoreDataMember) { settings.ContractResolver = AveContractResolver.Instance; }
                return JsonConvert.DeserializeObject<T>(data, settings);
            }
            catch (Exception ex)
            {
                logger.Warn("An exception occurred on json convert deserialize data: {0}", ex.Message);
                throw;
            }
        }
    }

    /// <summary>
    /// Use PropertyName to serialize or deserialize, not DataMemberAttribute's Name
    /// </summary>
    public class AveContractResolver : DefaultContractResolver
    {
        private static readonly IContractResolver _instance = new AveContractResolver();

        internal static IContractResolver Instance
        {
            get { return _instance; }
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            property.PropertyName = property.UnderlyingName;
            return property;
        }

        protected override JsonProperty CreatePropertyFromConstructorParameter(JsonProperty matchingMemberProperty, ParameterInfo parameterInfo)
        {
            var property = base.CreatePropertyFromConstructorParameter(matchingMemberProperty, parameterInfo);
            property.PropertyName = property.UnderlyingName;
            return property;
        }
    }
}
