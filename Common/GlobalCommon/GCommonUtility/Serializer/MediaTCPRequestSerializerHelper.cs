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
    using System.IO;
    using System.Runtime.Serialization;
    using System.Xml;
    #endregion

    /// <summary>
    /// To serialize the media request message by the DataContractSerializer which
    /// transfer by socket channel.
    /// </summary>
    /// <example>
    ///      var obj = new MediaFullTextIndexInfo();
    ///      var serializedString = MediaTCPRequestSerializerHelper.Serialize(obj);
    ///      var deserializedObj = MediaTCPRequestSerializerHelper.DeSerialize(serializedString);
    /// </example>
    public static class MediaTCPRequestSerializerHelper
    {
        /// <summary>
        /// serialize the object to string 
        /// </summary>
        /// <param name="value">the object value</param>
        /// <returns>the result string</returns>
        public static String Serialize(Object value)
        {
            using (var ms = new MemoryStream())
            {
                var dataContractSerializer = new DataContractSerializer(value.GetType());
                dataContractSerializer.WriteObject(ms, value);
                var doc = new XmlDocument();
                var element = doc.CreateElement("Request");
                element.SetAttribute("assemblyQualifiedName", value.GetType().AssemblyQualifiedName);
                element.SetAttribute("base64data", Convert.ToBase64String(ms.ToArray()));
                return element.OuterXml;
            }
        }

        /// <summary>
        /// deserialize the string to object 
        /// </summary>
        /// <param name="value">the string value of the serialized string</param>
        /// <returns>the deserialized object</returns>
        public static Object DeSerialize(String value)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var doc = new XmlDocument();
                doc.LoadXml(value);
                var typeFullName = doc.DocumentElement.GetAttribute("assemblyQualifiedName");
                var t = Type.GetType(typeFullName);
                var serializeString = doc.DocumentElement.GetAttribute("base64data");
                var content = Convert.FromBase64String(serializeString);
                ms.Write(content, 0, content.Length);
                ms.Position = 0;
                var dataContractSerializer = new DataContractSerializer(t);
                return dataContractSerializer.ReadObject(ms);
            }
        }
    }
}
