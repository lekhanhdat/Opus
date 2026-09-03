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
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public interface IAveView
    {
        void DeleteObject();
        void Update();

        string Aggregations { get; set; }
        string AggregationsStatus { get; set; }
        string BaseViewId { get; }
        IAveContentTypeId ContentTypeId { get; set; }
        string CalendarSettings { get; set; }
        bool DefaultView { get; set; }
        bool DefaultViewForContentType { get; set; }
        bool EditorModified { get; set; }
        string Formats { get; set; }
        bool Hidden { get; set; }
        string HtmlSchemaXml { get; }
        Guid ID { get; }
        string ImageUrl { get; }
        bool IncludeRootFolder { get; set; }
        string Method { get; set; }
        bool MobileDefaultView { get; set; }
        bool MobileView { get; set; }
        string ModerationType { get; }
        bool OrderedView { get; }
        bool Paged { get; set; }
        IAveList ParentList { get; }
        bool PersonalView { get; }
        bool ReadOnlyView { get; }
        bool RequiresClientIntegration { get; }
        uint RowLimit { get; set; }
        AveViewScope Scope { get; set; }
        string ServerRelativeUrl { get; }
        string StyleId { get; }
        bool Threaded { get; }
        string Title { get; set; }
        string Toolbar { get; set; }
        string ToolbarTemplateName { get; }
        string Url { get; }
        string ViewData { get; set; }
        IAveViewFieldCollection ViewFields { get; }
        string ViewJoins { get; set; }
        string ViewProjectedFields { get; set; }
        string Query { get; set; }
        string Type { get; }
        string RowLimitExceeded { get; set; }
        void ApplyStyle(IAveViewStyle viewStyles);
        string GroupByFooter { get; set; }
        string GroupByHeader { get; set; }
        string OpenApplicationExtension { get; set; }
        string ViewBody { get; set; }
        string ViewEmpty { get; set; }
        string ViewFooter { get; set; }
        string ViewHeader { get; set; }
        string ParameterBindings { get; set; }
        string Joins { get; set; }
        string InlineEdit { get; set; }
        string XslLink { get; set; }
        string Xsl { get; set; }
        string ListViewXml { get; set; }
    }

    public enum AveViewScope
    {
        DefaultValue,
        Recursive,
        RecursiveAll,
        FilesOnly
    }
}
