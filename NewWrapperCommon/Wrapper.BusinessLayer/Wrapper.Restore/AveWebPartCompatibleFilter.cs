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
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using AvePoint.Wrapper.Common;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.Wrapper.Restore
{
    //public class AveWebPartCompatibleFilter
    //{
    //    private const string WebPartV2NameSpace = "http://schemas.microsoft.com/WebPart/v2";
    //    private const string WebPartV3NameSpace = "http://schemas.microsoft.com/WebPart/v3";
    //    private static IList<AveTypeName> uncompatibleWebParts = new List<AveTypeName>();
    //    [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are the part of namespace. ")]
    //    static AveWebPartCompatibleFilter()
    //    {            
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.SharePoint.Portal, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.SharePoint.Portal.WebControls.OWACalendarPart" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.SharePoint.Portal, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.SharePoint.Portal.WebControls.OWAContactsPart" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.SharePoint.Portal, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.SharePoint.Portal.WebControls.OWAInboxPart" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.SharePoint.Portal, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.SharePoint.Portal.WebControls.OWAPart" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.SharePoint.Portal, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.SharePoint.Portal.WebControls.OWATasksPart" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.SharePoint.Portal, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.SharePoint.Portal.WebControls.PeopleSearchBoxEx" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.SharePoint.Portal, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.SharePoint.Portal.WebControls.ProfileBrowser" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.SharePoint.Portal, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.SharePoint.Portal.WebControls.ScorecardFilterWebPart" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.SharePoint.Portal, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.SharePoint.Portal.WebControls.BusinessDataDetailsWebPart" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.Office.Server.Search, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.Office.Server.Search.WebControls.AdvancedSearchBox" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.Office.Server.Search, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.SharePoint.Portal.WebControls.DualChineseWebpart" });            
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.Office.Server.Search, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.Office.Server.Search.WebControls.SearchPagingWebPart" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.Office.Server.Search, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.Office.Server.Search.WebControls.SearchStatsWebPart" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.Office.Server.Search, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.Office.Server.Search.WebControls.SearchSummaryWebPart" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.Office.Server.Search, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.Office.Server.Search.WebControls.VisualBestBetWebPart" });                        
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.Office.Server.Search, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.Office.Server.Search.WebControls.FederatedResultsWebPart" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.Office.Server.Search, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.Office.Server.Search.WebControls.PeopleRefinementWebPart" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.Office.Server.Search, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.Office.Server.Search.WebControls.PeopleCoreResultsWebPart" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.Office.Server.Search, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.Office.Server.Search.WebControls.RefinementWebPart" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.Office.Server.Search, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.Office.Server.Search.WebControls.QuerySuggestionsWebPart" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.Office.Server.Search, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.Office.Server.Search.WebControls.CoreResultsWebPart" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.Office.Server.Search, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.Office.Server.Search.WebControls.HighConfidenceWebPart" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.Office.Server.Search, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.Office.Server.Search.WebControls.CoreResultsWebPart" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.Office.Server.WebAnalytics.UI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.Office.Server.WebAnalytics.Reporting.WhatsPopularWebPart" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.Office.Server.Chart, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.Office.Server.WebControls.ChartWebPart" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.Office.Server.FilterControls, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.SharePoint.Portal.WebControls.PageContextFilterWebPart" });
    //        uncompatibleWebParts.Add(new AveTypeName() { Assembly = new AssemblyName("Microsoft.Office.Server.WebAnalytics.UI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"), FullName = "Microsoft.Office.Server.WebAnalytics.Reporting.WhatsPopularWebPart" });                        
    //    }

    //    public IList<AveWebPartBaseInfo> FilterWebParts(IList<AveWebPartBaseInfo> webparts)
    //    {            
    //        return webparts.Where(FilterWebPart).ToList();            
    //    }

    //    private bool FilterWebPart(AveWebPartBaseInfo webpart)
    //    {
    //        if (string.IsNullOrEmpty(webpart.DefinitionXml))
    //        {
    //            return false;
    //        }
    //        else
    //        {
    //            return !uncompatibleWebParts.Contains(GetWebPartTypeFullname(SelectWebPartNode(webpart.DefinitionXml)), new AveTypeNameEqualityComparer(true, true));
    //        }
    //    }

    //    private XmlElement SelectWebPartNode(string webpartDefinitionXml)
    //    {
    //        XmlElement webpartNode = TrySelectV2WebPartNode(webpartDefinitionXml);
    //        if (webpartNode == null)
    //        {
    //            webpartNode = TrySelectV3WebPartNode(webpartDefinitionXml);
    //        }
    //        return webpartNode;
    //    }

    //    private XmlElement TrySelectV2WebPartNode(string webpartDefinitionXml)
    //    {
    //        XmlDocument doc = new XmlDocument();
    //        doc.LoadXml(webpartDefinitionXml);

    //        XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
    //        nsmgr.AddNamespace("default", WebPartV2NameSpace);
    //        return doc.SelectSingleNode("default:WebPart", nsmgr) as XmlElement;            
    //    }

    //    private XmlElement TrySelectV3WebPartNode(string webpartDefinitionXml)
    //    {
    //        XmlDocument doc = new XmlDocument();
    //        doc.LoadXml(webpartDefinitionXml);

    //        XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
    //        nsmgr.AddNamespace("default", WebPartV3NameSpace);
    //        return doc.SelectSingleNode("webParts/default:webPart", nsmgr) as XmlElement;
    //    }

    //    private AveTypeName GetWebPartTypeFullname(XmlElement webpartNode)
    //    {
    //        AveTypeName typeName = null;

    //        if (WebPartV2NameSpace.Equals(webpartNode.NamespaceURI, StringComparison.OrdinalIgnoreCase))
    //        {                
    //            typeName = new AveTypeName();
    //            XmlNamespaceManager nsmgr = new XmlNamespaceManager(webpartNode.OwnerDocument.NameTable);
    //            nsmgr.AddNamespace("default", WebPartV2NameSpace);
    //            XmlNode assemblyNode = webpartNode.SelectSingleNode("default:Assembly", nsmgr);                
    //            typeName.Assembly = assemblyNode != null ? new AssemblyName(assemblyNode.InnerText) : null;
    //            XmlNode typeNode = webpartNode.SelectSingleNode("default:TypeName", nsmgr);
    //            typeName.FullName = typeNode != null ? typeNode.InnerText : null;
    //        }
    //        else if (WebPartV3NameSpace.Equals(webpartNode.NamespaceURI, StringComparison.OrdinalIgnoreCase))
    //        {
    //            typeName = new AveTypeName();
    //            XmlNamespaceManager nsmgr = new XmlNamespaceManager(webpartNode.OwnerDocument.NameTable);
    //            nsmgr.AddNamespace("default", WebPartV3NameSpace);
    //            XmlNode typeNode = webpartNode.SelectSingleNode("default:metaData/default:type/@name", nsmgr);
    //            string typefullname = typeNode != null ? typeNode.InnerText : null;
    //            if (!string.IsNullOrEmpty(typefullname))
    //            {                    
    //                typeName.Assembly = new AssemblyName(typefullname.Substring(typefullname.IndexOf(',') + 1).Trim());
    //                typeName.FullName = typefullname.Substring(0, typefullname.IndexOf(',')).Trim();
    //            }
    //        }
    //        return typeName;
    //    }
    //}   


    //public class AveTypeNameEqualityComparer : IEqualityComparer<AveTypeName>
    //{
    //    private bool ignoreVersion;
    //    private bool ignoreCase;

    //    public AveTypeNameEqualityComparer(bool ignoreVersion, bool ignoreCase)
    //    {
    //        this.ignoreVersion = ignoreVersion;
    //        this.ignoreCase = ignoreCase;
    //    }

    //    public bool Equals(AveTypeName x, AveTypeName y)
    //    {            
    //        if (x == null || y == null || x.Assembly == null || y.Assembly == null)
    //        {
    //            return false;
    //        }
    //        StringComparison comparision = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
    //        return x.Assembly.Name.Equals(y.Assembly.Name, comparision) && x.FullName.Equals(y.FullName, comparision);
    //    }

    //    public int GetHashCode(AveTypeName obj)
    //    {
    //        return obj.GetHashCode();
    //    }
    //}
}
