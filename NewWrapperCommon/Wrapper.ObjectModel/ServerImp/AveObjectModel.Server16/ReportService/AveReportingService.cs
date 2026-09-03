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
using System.Text;
using System.Xml;
using System.IO;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.Server16
{
    public class AveReportingService
    {
        internal static Stream ReplaceDataSourceStream(Stream stream, AveItem item)
        {
            return stream;
        }

        internal static Stream ReplaceReportStream(Stream stream, AveItem item)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveReportingService.ReplaceReportStream"))
            {

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                xmlDoc.Load(stream);
                XmlNodeList dsrNodeList = xmlDoc.GetElementsByTagName("DataSourceReference");
                if (dsrNodeList.Count > 0)
                {
                    for (int i = 0; i < dsrNodeList.Count; i++)
                    {
                        dsrNodeList[i].InnerText = AveReplaceProcessor.UrlReplace(dsrNodeList[i].InnerText, item.info.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), item.info.SourceSiteInfo, item.info.ParentSiteServerRelativeUrl);
                    }
                }
                XmlNodeList nodelist = xmlDoc.DocumentElement.GetElementsByTagName("rd:ReportServerUrl");
                if (nodelist.Count > 0)
                {
                    for (int i = 0; i < nodelist.Count; i++)
                    {
                        nodelist[i].InnerText = AveReplaceProcessor.UrlReplace(nodelist[i].InnerText, item.info.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), item.info.SourceSiteInfo, item.info.ParentSiteServerRelativeUrl);
                    }
                }
                Stream newStream = new MemoryStream();
                xmlDoc.Save(newStream);
                newStream.TryToResetStreamPosition();
                return newStream;

            }

        }
    }
}
