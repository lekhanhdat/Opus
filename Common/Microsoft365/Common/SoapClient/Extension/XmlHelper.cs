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
namespace Microsoft365.Common.SoapClient
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    internal static class XmlExtensions
    {
        private static readonly XmlSerializerNamespaces DefaultNamespace;

        static XmlExtensions()
        {
            DefaultNamespace = new XmlSerializerNamespaces();
            DefaultNamespace.Add("", "");
        }
        public static string ToXmlString<T>(this T item, bool removeXmlDeclaration) where T:class
        {
            if (item == default)
            {
                return null;
            }
            using (var textWriter = new StringWriter())
            using (var xmlWriter = GetXmlWriter(removeXmlDeclaration, textWriter))
            {
                new XmlSerializer(item.GetType())
                    .Serialize(xmlWriter, item, DefaultNamespace);
                return textWriter.ToString();
            }
        }

        public static XElement ToXElement<T>(this T item, bool removeXmlDeclaration=true) where T : class
        {
            return item == default ? default : XElement.Parse(item.ToXmlString(removeXmlDeclaration));
        }


        public static T ToObject<T>(this string xml)
        {
            if (string.IsNullOrWhiteSpace(xml)) return default;

            using (var stringReader = new StringReader(xml))
            using (var xmlReader = XmlReader.Create(stringReader))
            {
                var result = (T)new XmlSerializer(typeof(T)).Deserialize(xmlReader);
                return result;
            }
        }

        public static T ToObject<T>(this XElement xml)
        {
            return xml == default ? default : xml.ToString().ToObject<T>();
        }


    private static XmlWriter GetXmlWriter(bool removeXmlDeclaration, StringWriter textWriter)
        {
            return XmlWriter.Create(textWriter, new XmlWriterSettings
            {
                OmitXmlDeclaration = removeXmlDeclaration,
                Indent = false,
                NamespaceHandling = NamespaceHandling.OmitDuplicates
            });
        }
    }
}
