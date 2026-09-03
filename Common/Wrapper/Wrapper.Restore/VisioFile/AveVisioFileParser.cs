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
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Restore
{
    public class AveVisioFileParser
    {
        public static readonly AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private Stream fileStream;
        private Stream fixFileStream;
        private AveSPSite mAveParentSite;

        public Stream ConvertFileStreamToMemoryStream(Stream fileStream)
        {
            var memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }

        public AveVisioFileParser(Stream fileStream, AveSPSite mAveParentSite)
        {
            this.fileStream = fileStream;
            this.mAveParentSite = mAveParentSite;
        }

        public Stream FixBrokenLinks()
        {
            try
            {
                fixFileStream = ConvertFileStreamToMemoryStream(fileStream);
                using (Package fpackage = Package.Open(fixFileStream, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    PackagePart connectionsPart = ExtractPackage(fpackage);
                    if (connectionsPart != null)
                    {
                        XmlDocument connectionsXmlDoc = UpdateXmlDocument(connectionsPart);
                        SavePackage(connectionsPart, connectionsXmlDoc);
                    }
                }
                fixFileStream.Position = 0;
            }
            catch (Exception e)
            {
                log.Error("Fix Visio File Failed :{0}", e.Message);
                fixFileStream.Position = 0;
                return fixFileStream;
            }
            return fixFileStream;
        }

        public PackagePart ExtractPackage(Package package)
        {
            PackagePart connectionsPart = null;
            PackagePartCollection parts = package.GetParts();
            foreach (PackagePart part in parts)
            {
                if (part.Uri.ToString().EndsWith("connections.xml"))
                {
                    connectionsPart = part;
                    break;
                }
            }
            return connectionsPart;
        }

        public XmlDocument UpdateXmlDocument(PackagePart part)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(part.GetStream());
            XmlElement rootEle = xmlDoc.DocumentElement;
            XmlNamespaceManager xmlManger = new XmlNamespaceManager(xmlDoc.NameTable);
            xmlManger.AddNamespace("DocAve", "http://schemas.microsoft.com/office/visio/2012/main");
            XmlNodeList nodes = xmlDoc.SelectNodes("//DocAve:DataConnection", xmlManger);
            foreach (XmlNode subNode in nodes)
            {
                if (subNode is XmlElement)
                {
                    XmlElement xmlEle = subNode as XmlElement;
                    string oldUrl = xmlEle.GetAttribute("FileName");
                    string newUrl = AveReplaceProcessor.UrlReplace(oldUrl, mAveParentSite.MappingManager.SiteMappingManager.AbsoluteUrlMapping, new ReplaceOption(true, true), mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
                    NewOleDbConnectionStringBuilder builder = NewOleDbConnectionStringBuilder.Parse(xmlEle.GetAttribute("ConnectionString"));
                    builder.Set("Data Source", newUrl);
                    xmlEle.SetAttribute("FileName", newUrl);
                    xmlEle.SetAttribute("ConnectionString", builder.ToString().TrimEnd(';'));
                }
            }
            return xmlDoc;
        }

        public void SavePackage(PackagePart part, XmlDocument xmlDoc)
        {
            XmlWriterSettings partWriterSettings = new XmlWriterSettings();
            partWriterSettings.Encoding = Encoding.UTF8;
            XmlWriter partWriter = XmlWriter.Create(part.GetStream(FileMode.Create, FileAccess.Write), partWriterSettings);
            XElement xElement = XElement.Parse(xmlDoc.InnerXml, LoadOptions.PreserveWhitespace);
            xElement.WriteTo(partWriter);
            partWriter.Flush();
            partWriter.Close();
        }
    }
}
