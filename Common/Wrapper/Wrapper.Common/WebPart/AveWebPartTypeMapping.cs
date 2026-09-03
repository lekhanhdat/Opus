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

namespace AvePoint.Wrapper.Common
{
    public class AveWebPartTypeMapping
    {
        private static readonly Dictionary<TypeInfo, AveWebPartType> WebPartUpdaterMapping = new Dictionary<TypeInfo, AveWebPartType>(new TypeInfoIgnoreVersionEqualityComparer());

        static AveWebPartTypeMapping()
        {
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.Office.Server.Search.WebControls.ContentBySearchWebPart, Microsoft.Office.Server.Search, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.ContentBySearchWebPart;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.SharePoint.Publishing.WebControls.TableOfContentsWebPart, Microsoft.SharePoint.Publishing, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.TableOfContentsWebPart;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.SharePoint.WebPartPages.ContentEditorWebPart, Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.ContentEditorWebPart;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.Office.Excel.WebUI.ExcelWebRenderer, Microsoft.Office.Excel.WebUI, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.ExcelWebRendererWebPart;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.Office.Visio.Server.WebControls.VisioWebAccess, Microsoft.Office.Visio.Server, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.VisioWebAccessWebPart;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.SharePoint.Publishing.WebControls.ContentByQueryWebPart,Microsoft.SharePoint.Publishing,Version=16.0.0.0,Culture=neutral,PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.ContentByQueryWebPart;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.SharePoint.WebPartPages.XsltListViewWebPart, Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.XsltListViewWebPart;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.SharePoint.WebPartPages.ListViewWebPart, Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.ListViewWebPart;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.SharePoint.WebPartPages.ListFormWebPart, Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.ListFormWebPart;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.SharePoint.Publishing.WebControls.SummaryLinkWebPart,Microsoft.SharePoint.Publishing,Version=16.0.0.0,Culture=neutral,PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.SummaryLinkWebPart;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.SharePoint.Portal.WebControls.TagCloudWebPart, Microsoft.SharePoint.Portal, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.TagCloudWebPart;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.SharePoint.Portal.WebControls.SocialCommentWebPart, Microsoft.SharePoint.Portal, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.SocialCommentWebPart;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.SharePoint.WebPartPages.XmlWebPart, Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.XMLWebPart;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.SharePoint.Portal.WebControls.RSSAggregatorWebPart, Microsoft.SharePoint.Portal, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.RssWebPart;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.SharePoint.WebPartPages.PictureLibrarySlideshowWebPart, Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.SlideshowWebPart;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.SharePoint.WebPartPages.SPTimelineWebPart, Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.TimelineWebPart;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.Office.InfoPath.Server.Controls.WebUI.BrowserFormWebPart, Microsoft.Office.InfoPath.Server, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.BrowserFormWebPart;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.SharePoint.Portal.WebControls.CategoryResultsWebPart, Microsoft.SharePoint.Portal, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.SiteInCategory;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.SharePoint.Portal.WebControls.BusinessDataDetailsWebPart, Microsoft.SharePoint.Portal, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.BusinessDataDetails;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.SharePoint.Portal.WebControls.BusinessDataListWebPart, Microsoft.SharePoint.Portal, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.BusinessDataList;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.SharePoint.Portal.WebControls.BusinessDataAssociationWebPart, Microsoft.SharePoint.Portal, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.BusinessDataList;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.SharePoint.Portal.WebControls.ContactFieldControl,Microsoft.SharePoint.Portal, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.ContactDetailWebPart;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.SharePoint.WebPartPages.BlogLinksWebPart,Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.BlogLinksWebPart;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.SharePoint.WebPartPages.ScriptEditorWebPart, Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.ScriptEditorWebPart;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.SharePoint.Portal.WebControls.CategoryWebPart, Microsoft.SharePoint.Portal, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.CategoriesWebPart;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.SharePoint.WebPartPages.DataFormWebPart, Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.DataFormWebPart;
            WebPartUpdaterMapping[TypeInfo.Parse("Microsoft.Office.Project.PWA.WebParts.ProjectFieldPart, Microsoft.Office.Project.Server.PWA, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")] = AveWebPartType.ProjectFieldPart;
        }
        public static AveWebPartType GetWebPartType(string typeName)
        {
            if (WebPartUpdaterMapping.Keys?.Any(it => it.Name.StartsWith(typeName)) == true)
            {
                return WebPartUpdaterMapping[WebPartUpdaterMapping.Keys.FirstOrDefault(it => it.Name.StartsWith(typeName))];
            }
            return AveWebPartType.DefaultWebpartType;
        }

        public static AveWebPartType GetWebPartType(TypeInfo typeInfo)
        {
            if (WebPartUpdaterMapping.ContainsKey(typeInfo))
            {
                return WebPartUpdaterMapping[typeInfo];
            }
            return AveWebPartType.DefaultWebpartType;
        }
    }

    public enum AveWebPartType
    {
        DefaultWebpartType,
        ContactDetailWebPart,
        ContentByQueryWebPart,
        ContentEditorWebPart,
        ExcelWebRendererWebPart,
        TableOfContentsWebPart,
        VisioWebAccessWebPart,
        ListViewWebPart,
        ListFormWebPart,
        XsltListViewWebPart,
        SummaryLinkWebPart,
        TagCloudWebPart,
        SocialCommentWebPart,
        XMLWebPart,
        RssWebPart,
        SlideshowWebPart,
        TimelineWebPart,
        BrowserFormWebPart,
        SiteInCategory,
        BusinessDataDetails,
        BusinessDataList,
        BlogLinksWebPart,
        ScriptEditorWebPart,
        CategoriesWebPart,
        DataFormWebPart,
        ProjectFieldPart,
        ContentBySearchWebPart
    }
}
