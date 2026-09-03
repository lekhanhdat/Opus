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
using AvePoint.Wrapper.Common;
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon.Contract.CommonFilter;

namespace AvePoint.Wrapper.Discovery
{
    internal class AveDiscoverFilterUtility : IAveDiscoverFilterUtility
    {
        public SiteCollectionInfo GetSiteFilterInfo(List<FilterPolicy> policies, IAveSite site)
        {
            return FilterAnalyser.GetSiteFilterInfo(policies, site) as SiteCollectionInfo;
        }

        public SiteInfo GetWebFilterInfo(List<FilterPolicy> policies, IAveWeb web)
        {
            return FilterAnalyser.GetWebFilterInfo(policies, web) as SiteInfo;
        }

        public ListInfo GetListFilterInfo(List<FilterPolicy> policies, IAveList list)
        {
            return FilterAnalyser.GetListFilterInfo(policies, list) as ListInfo;
        }

        public FolderInfo GetFolderFilterInfo(List<FilterPolicy> policies, IAveFolder folder)
        {
            return FilterAnalyser.GetFolderFilterInfo(policies, folder) as FolderInfo;
        }

        public DocumentInfo GetDocumentFilterInfo(List<FilterPolicy> policies, IAveFile file, IAveListItem listItem)
        {
            return FilterAnalyser.GetDocumentFilterInfo(policies, file, listItem) as DocumentInfo;
        }

        public DocumentInfo GetDocumentFilterInfo(List<FilterPolicy> policies, IAveListItem listItem, int uiVersion, IAveFile file)
        {
            return FilterAnalyser.GetDocumentFilterInfo(policies, listItem, uiVersion, file) as DocumentInfo;
        }

        public DocumentInfo GetDocumentFilterInfo(List<FilterPolicy> policies, IAveListItem listItem, int uiVersion, IAveFile file,IAveDiscoveryQuery query = null)
        {
            return FilterAnalyser.GetDocumentFilterInfo(policies, listItem, uiVersion, file,query) as DocumentInfo;
        }

        public ItemInfo GetItemFilterInfo(List<FilterPolicy> policies, IAveListItem listItem)
        {
            return FilterAnalyser.GetItemFilterInfo(policies, listItem) as ItemInfo;
        }

        public ItemInfo GetItemFilterInfo(List<FilterPolicy> policies, IAveListItem listItem, int uiVersion, IAveDiscoveryQuery query = null)
        {
            return FilterAnalyser.GetItemFilterInfo(policies, listItem, uiVersion, query) as ItemInfo;
        }

        public AttachmentInfo GetAttachmentFilterInfo(List<FilterPolicy> policies, IAveFile attachment, IAveListItem listItem)
        {
            return FilterAnalyser.GetAttachmentFilterInfo(policies, attachment, listItem) as AttachmentInfo;
        }

    }
}
