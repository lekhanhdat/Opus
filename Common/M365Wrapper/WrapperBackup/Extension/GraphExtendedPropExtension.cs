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
using Microsoft.Exchange.WebServices.Data;
using Microsoft365.Graph.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using EWSMapiPropertyType = Microsoft.Exchange.WebServices.Data.MapiPropertyType;

namespace ExchangeUtility.Graph
{
    public static class GraphExtendedPropExtension
    {
        /// <summary>
        /// Convert EWS ExtendedPropertyDefinition to Graph extended property id string.
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static string ToGraphExtendedPropId(this ExtendedPropertyDefinition ext)
        {
            string type = ext.MapiType switch
            {
                EWSMapiPropertyType.String => "String",
                EWSMapiPropertyType.Integer => "Integer",
                EWSMapiPropertyType.Boolean => "Boolean",
                EWSMapiPropertyType.SystemTime => "SystemTime",
                EWSMapiPropertyType.Double => "Double",
                EWSMapiPropertyType.Binary => "Binary",
                _ => throw new NotSupportedException($"Unsupported MAPI type: {ext.MapiType}")
            };

            // INTERNET HEADERS
            if (ext.PropertySet == DefaultExtendedPropertySet.InternetHeaders)
            {
                if (string.IsNullOrEmpty(ext.Name))
                    throw new NotSupportedException("Internet headers must have a Name.");

                // For sensitivity label
                if (ext.Name.Equals("msip_labels"))
                    return $"{type} {{{ext.PropertySetId ?? new Guid("00020386-0000-0000-C000-000000000046")}}} Name {ext.Name}";

                return $"{type} {{{ext.PropertySetId ?? Guid.Empty}}} Name {ext.Name}";
            }

            // CUSTOM PROPERTY: PropertySetId + Name
            if (ext.PropertySetId.HasValue && !string.IsNullOrEmpty(ext.Name))
            {
                return $"{type} {{{ext.PropertySetId.Value}}} Name {ext.Name}";
            }

            // CUSTOM PROPERTY: PropertySetId + Id
            if (ext.PropertySetId.HasValue && ext.Id.HasValue)
            {
                return $"{type} {{{ext.PropertySetId.Value}}} Id 0x{ext.Id.Value:x4}";
            }

            // PROPERTY TAG
            if (ext.Tag.HasValue)
            {
                return $"{type} 0x{ext.Tag.Value:x4}";
            }

            throw new NotSupportedException(
                "Extended property must have (PropertySetId + Name), (PropertySetId + Id), or Tag.");
        }

        /// <summary>
        /// Build Graph $expand string for a single extended property.
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string ToGraphSingleValueExpandString(this IMapiExtendedPropertyDefinition props)
        {
            if (props == null)
                throw new ArgumentNullException(nameof(props));

            string container = props.GetPropType().ToString()
                .Contains("Array") ? "multiValueExtendedProperties" : "singleValueExtendedProperties";

            return $"{container}($filter=id eq {props})";
        }

        /// <summary>
        /// Build Graph $expand string for multiple extended properties.
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string ToGraphSingleValueExpandString(this IEnumerable<IMapiExtendedPropertyDefinition> props)
        {
            if (props == null)
                throw new ArgumentNullException(nameof(props));

            return $"singleValueExtendedProperties($filter=id eq {string.Join(" or id eq ", props)})";
        }

        public static string ConvertFromGuidToBase64Id(this Guid guid)
        {
            if (guid == Guid.Empty) return string.Empty;
            byte[] guidBytes = guid.ToByteArray();
            return Convert.ToBase64String(guidBytes);
        }

        public static Guid ConvertFromBase64ToGuidId(this string base64Id)
        {
            if (string.IsNullOrEmpty(base64Id)) return Guid.Empty;
            byte[] guidBytes = Convert.FromBase64String(base64Id);
            return new Guid(guidBytes);
        }
    }
}
