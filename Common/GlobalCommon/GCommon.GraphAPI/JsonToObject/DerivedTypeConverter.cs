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
namespace AvePoint.GCommon.GraphAPI
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;

    using Microsoft.Graph;
    using Microsoft.Graph.Models.ODataErrors;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Handles resolving interfaces to the correct derived class during serialization/deserialization.
    /// https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/blob/1.25.1/src/Microsoft.Graph.Core/Serialization/DerivedTypeConverter.cs
    /// </summary>
    public class DerivedTypeConverter : JsonConverter
    {
        internal static readonly ConcurrentDictionary<string, Type> TypeMappingCache = new ConcurrentDictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Constructs a new DerivedTypeConverter.
        /// </summary>
        public DerivedTypeConverter()
            : base()
        {
        }

        /// <summary>
        /// Checks if the given object can be converted. In this instance, all object can be converted.
        /// </summary>
        /// <param name="objectType">The type of the object to convert.</param>
        /// <returns>True</returns>
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        /// <summary>
        /// Checks if the entity supports write. Currently no derived types support write.
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Deserializes the object to the correct type.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">The interface type.</param>
        /// <param name="existingValue">The existing value of the object being read.</param>
        /// <param name="serializer">The <see cref="JsonSerializer"/> for deserialization.</param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            var type = jObject.GetValue(CoreConstants.Serialization.ODataType);

            object instance = null;

            if (type != null)
            {
                var typeString = type.ToString();
                typeString = typeString.TrimStart('#');
                typeString = StringHelper.ConvertTypeToTitleCase(typeString);

                Type instanceType = null;

                if (DerivedTypeConverter.TypeMappingCache.TryGetValue(typeString, out instanceType))
                {
                    instance = this.Create(instanceType);
                }
                else
                {
                    var typeAssembly = objectType.GetTypeInfo().Assembly;
                    instance = this.Create(typeString, typeAssembly);
                }

                // If @odata.type is set but we aren't able to create an instance of it use the method-provided
                // object type instead. This means unknown types will be deserialized as a parent type.
                if (instance == null)
                {
                    instance = this.Create(objectType.AssemblyQualifiedName, /* typeAssembly */ null);
                }

                if (instance != null && instanceType == null)
                {
                    // Cache the type mapping resolution if we haven't pulled it from the cache already.
                    DerivedTypeConverter.TypeMappingCache.TryAdd(typeString, instance.GetType());
                }
            }
            else
            {
                instance = this.Create(objectType.AssemblyQualifiedName, /* typeAssembly */ null);
            }

            if (instance == null)
            {
                throw new ODataError
                {
                    Error = new MainError
                    {
                        Code = "generalException",
                        Message = string.Format("Unable to create an instance of type {0}.", objectType.AssemblyQualifiedName),
                    }
                };
            }

            using (var objectReader = this.GetObjectReader(reader, jObject))
            {
                serializer.Populate(objectReader, instance);
                return instance;
            }
        }

        /// <summary>
        /// Not yet implemented
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private object Create(string typeString, Assembly typeAssembly)
        {
            Type type = null;

            if (typeAssembly != null)
            {
                type = typeAssembly.GetType(typeString);
            }
            else
            {
                type = Type.GetType(typeString);
            }

            return this.Create(type);
        }

        private object Create(Type type)
        {
            if (type == null)
            {
                return null;
            }

            try
            {
                // Find the default constructor. Abstract entity classes use non-public constructors.
                var constructorInfo = type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(
                    constructor => !constructor.GetParameters().Any() && !constructor.IsStatic);

                if (constructorInfo == null)
                {
                    return null;
                }

                return constructorInfo.Invoke(new object[] { });
            }
            catch (Exception exception)
            {
                throw new ODataError {
                    Error = new MainError
                    {
                        Code = "generalException",
                        Message = string.Format("Unable to create an instance of type {0}.", type.FullName)
                    },
                };
            }
        }

        private JsonReader GetObjectReader(JsonReader originalReader, JObject jObject)
        {
            var objectReader = jObject.CreateReader();

            objectReader.Culture = originalReader.Culture;
            objectReader.DateParseHandling = originalReader.DateParseHandling;
            objectReader.DateTimeZoneHandling = originalReader.DateTimeZoneHandling;
            objectReader.FloatParseHandling = originalReader.FloatParseHandling;

            // This reader is only for a single token. Don't dispose the stream after.
            objectReader.CloseInput = false;

            return objectReader;
        }
    }
}