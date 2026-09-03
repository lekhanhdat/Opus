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
using AvePoint.GCommon;
using Native13NinTexWorkflowEntity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace LS.SPWorkflowProcessor
{
    static class SerializerHelper
    {

        public static ExportedWorkflow DeserializeExportedWorkflow(byte[] exportedWorkflow)
        {
            var exportedWorkflowSeralizedData = GetExportedWorkflowSeralizedXml(Encoding.UTF8.GetString(exportedWorkflow));
            using (var stream = new MemoryStream(exportedWorkflowSeralizedData))
            using (var reader = new XmlTextReader(stream))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ExportedWorkflow));
                return serializer.Deserialize(reader) as ExportedWorkflow;
            }
        }

        public static ListReferenceCollection DeserializeListReferenceCollection(byte[] exportedWorkflow)
        {
            var listReferenceCollectionSeralizedData = GetListReferenceCollectionSeralizedXml(Encoding.UTF8.GetString(exportedWorkflow));
            return DeserializeObjectFromString<ListReferenceCollection>(listReferenceCollectionSeralizedData);
        }

        private static string GetListReferenceCollectionSeralizedXml(string exportedWorkflowData)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(exportedWorkflowData);
            XmlNodeList elementsByTagName = document.GetElementsByTagName("ListReferences");
            return string.Format("<ArrayOfListReference>{0}</ArrayOfListReference>", elementsByTagName[0].InnerXml);
        }

        private static byte[] GetExportedWorkflowSeralizedXml(string exportedWorkflowData)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(exportedWorkflowData);
            XmlNodeList elementsByTagName = document.GetElementsByTagName("ExportedWorkflowSeralized");
            return Encoding.UTF8.GetBytes(elementsByTagName[0].InnerText);
        }



        public static T DeserializeObjectFromString<T>(string input) where T : class
        {
            using (var stream = new StringReader(input))
            using (var reader = new XmlTextReader(stream))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return serializer.Deserialize(reader) as T;
            }
        }

        public static T DeserializeObjectFromStream<T>(Stream input) where T : class
        {
            using (var reader = new XmlTextReader(input))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return serializer.Deserialize(reader) as T;
            }
        }

        public static byte[] SerializeObjectToBytes<T>(T targetOject) where T : class
        {
            using (var stream = new MemoryStream())
            using (var writer = new XmlTextWriter(stream, Encoding.UTF8))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(stream, targetOject);
                var bytes = stream.ToArray();
                return bytes;
            }
        }

        public static string SerializeObjectToString<T>(T targetOject) where T : class
        {
            var bytes = SerializeObjectToBytes(targetOject);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

    }
}
