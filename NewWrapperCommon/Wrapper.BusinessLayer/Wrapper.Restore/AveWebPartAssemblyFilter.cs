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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace AvePoint.Wrapper.Restore
{
    public class AveWebPartAssemblyFilter
    {
        private const string WebPartV2NameSpace = "http://schemas.microsoft.com/WebPart/v2";
        private const string WebPartV3NameSpace = "http://schemas.microsoft.com/WebPart/v3";
        private const string FilterMessage = "Encountered Office 365 environmental issue. Please contact your service provider.";
        protected static AveLogger log = AveLogger.GetInstance(typeof(AveWebPartAssemblyFilter));
        private int compareVersion;
        private string currentFileUrl;

        public ICollection<AveWrapperReportDto> FilteredWebParts = new List<AveWrapperReportDto>();

        public AveWebPartAssemblyFilter(string fileUrl, string spVersion)
        {
            currentFileUrl = fileUrl;
            compareVersion = new Version(spVersion).Major;
        }

        public IList<AveWebPartBaseInfo> FilterWebParts(IList<AveWebPartBaseInfo> webparts)
        {
            return webparts.Where(FilterWebPart).ToList();
        }

        private bool FilterWebPart(AveWebPartBaseInfo webpart)
        {
            if (string.IsNullOrEmpty(webpart.DefinitionXml))
            {
                return true;
            }
            bool noNeedFilter = true;
            try
            {
                noNeedFilter = GetWebPartTypeFullname(SelectWebPartNode(webpart.DefinitionXml)).Assembly.Version.Major <= compareVersion;
            }
            catch(Exception ex)
            {
                log.Warn("Error occurred when filter web part in documents , error : {0}", ex);
            }
            AddFilterWebpartReportDetail(webpart, noNeedFilter);
            return noNeedFilter;
        }

        private void AddFilterWebpartReportDetail(AveWebPartBaseInfo webpart,bool noNeedFilter)
        {
            if (noNeedFilter)
            {
                return;
            }
            AveWrapperReportDto webPartDto = new AveWrapperWebpartReportDto(currentFileUrl, "WebPart",webpart,string.Empty,string.Empty, AveStatus.Skipped, AveReportResource.Wrapper_Report_Office365EnvironmentIssue);
            FilteredWebParts.Add(webPartDto);
        }

        private XmlElement SelectWebPartNode(string webpartDefinitionXml)
        {
            XmlDocument webPartDoc = new XmlDocument();
            webPartDoc.LoadXml(webpartDefinitionXml);
            XmlNode webPartNode = webPartDoc.FirstChild;
            if (string.IsNullOrEmpty(webPartNode.NamespaceURI))
            {
                webPartNode = webPartNode.FirstChild;
            }
            return webPartNode as XmlElement;
        }

        private AveTypeName GetWebPartTypeFullname(XmlElement webpartNode)
        {
            AveTypeName typeName = null;

            if (WebPartV2NameSpace.Equals(webpartNode.NamespaceURI, StringComparison.OrdinalIgnoreCase))
            {
                typeName = new AveTypeName();
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(webpartNode.OwnerDocument.NameTable);
                nsmgr.AddNamespace("default", WebPartV2NameSpace);
                XmlNode assemblyNode = webpartNode.SelectSingleNode("default:Assembly", nsmgr);
                typeName.Assembly = assemblyNode != null ? new AssemblyName(assemblyNode.InnerText) : null;
                XmlNode typeNode = webpartNode.SelectSingleNode("default:TypeName", nsmgr);
                typeName.FullName = typeNode != null ? typeNode.InnerText : null;
            }
            else if (WebPartV3NameSpace.Equals(webpartNode.NamespaceURI, StringComparison.OrdinalIgnoreCase))
            {
                typeName = new AveTypeName();
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(webpartNode.OwnerDocument.NameTable);
                nsmgr.AddNamespace("default", WebPartV3NameSpace);
                XmlNode typeNode = webpartNode.SelectSingleNode("default:metaData/default:type/@name", nsmgr);
                string typefullname = typeNode != null ? typeNode.InnerText : null;
                if (!string.IsNullOrEmpty(typefullname))
                {
                    typeName.Assembly = new AssemblyName(typefullname.Substring(typefullname.IndexOf(',') + 1).Trim());
                    typeName.FullName = typefullname.Substring(0, typefullname.IndexOf(',')).Trim();
                }
            }
            return typeName;
        }
    }

    public class AveTypeName
    {
        public AssemblyName Assembly { get; set; }
        public string FullName { get; set; }
    }
}
