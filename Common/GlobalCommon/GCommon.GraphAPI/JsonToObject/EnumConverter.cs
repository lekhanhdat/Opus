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

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Handles resolving interfaces to the correct derived class during serialization/deserialization.
    /// </summary>
    public class EnumConverter : StringEnumConverter
    {
        /// <summary>
        /// Constructs a new EnumConverter.
        /// </summary>
        public EnumConverter()
            : base()
        {
            this.CamelCaseText = true;
        }

        /// <summary>
        /// Checks if the given type can be converted into an enum. All types
        /// can be converted.
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <returns>True.</returns>
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        /// <summary>
        /// Whether the object can be serialized to a request body.
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return true;
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
            try
            {
                return base.ReadJson(reader, objectType, existingValue, serializer);
            }
            catch (JsonSerializationException)
            {
                // The StringEnumConverter will throw a JsonSerializationException if the enum value doesn't exist.
                // Swallow the exception and return null in this case. The actual value will be in the additional
                // data property bag after deserialization.
            }

            return null;
        }

        /// <summary>
        /// Serializes the object into a JSON string.
        /// </summary>
        /// <param name="writer">The  <see cref="JsonWriter"/> to write with.</param>
        /// <param name="value">The object to write.</param>
        /// <param name="serializer">The <see cref="JsonSerializer"/> for serialization.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            base.WriteJson(writer, value, serializer);
        }
    }
}