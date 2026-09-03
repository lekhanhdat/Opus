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
using System.Globalization;
using System.IO;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Media.Storage;

namespace RAFileSystem.FileSystem.Disposal.NewLogic.V3.Services
{
    /// <summary>
    /// Extracts metadata properties from file system objects as XML.
    /// </summary>
    public class FileMetadataExtractor
    {
        private static readonly AveLogger Logger = AveLogger.GetInstance(typeof(FileMetadataExtractor));

        private static readonly HashSet<string> Office07Extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "docx", "docm", "dotx", "dotm",
            "xlsx", "xlsm", "xltx", "xltm", "xlsb", "xlam",
            "pptx", "pptm", "ppsx", "ppsm", "potx", "potm", "ppam"
        };

        public string GetMetaData(XFileInfoEx xObj)
        {
            try
            {
                var doc = new XmlDocument();
                var xe = doc.CreateElement("Property");

                AppendColumn(doc, xe, "CreatedBy", xObj.Owner);
                AppendColumn(doc, xe, "ModifiedBy", GetOfficeLastModifiedBy(xObj));

                return xe.OuterXml;
            }
            catch (Exception ex)
            {
                Logger.Debug("GetMetaData failed, path: {0}, details: {1}", xObj.Name, ex.ToString());
                return string.Empty;
            }
        }

        private static void AppendColumn(XmlDocument doc, XmlElement parent, string name, string value)
        {
            var element = doc.CreateElement("Column");
            element.SetAttribute("Name", name);
            element.SetAttribute("Value", value);
            parent.AppendChild(element);
        }

        private static string GetOfficeLastModifiedBy(XFileInfoEx xObj)
        {
            if (IsOffice07(xObj.Name))
            {
                // Intentionally not reading package metadata here
            }

            return string.Empty;
        }

        private static bool IsOffice07(string fileName)
        {
            try
            {
                var extension = Path.GetExtension(fileName);
                if (string.IsNullOrEmpty(extension))
                {
                    return false;
                }

                var ext = extension.Substring(1).ToLower(CultureInfo.InvariantCulture);
                return Office07Extensions.Contains(ext);
            }
            catch (Exception ex)
            {
                Logger.Debug("Check office file type failed, details: {0}", ex.ToString());
                return false;
            }
        }
    }
}

