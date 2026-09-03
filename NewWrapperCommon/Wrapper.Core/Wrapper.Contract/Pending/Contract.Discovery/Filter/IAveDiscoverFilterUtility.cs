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
    public interface IAveDiscoverFilterUtility
    {
        SiteCollectionInfo GetSiteFilterInfo(List<FilterPolicy> policies, IAveSite site);

        SiteInfo GetWebFilterInfo(List<FilterPolicy> policies, IAveWeb web);

        ListInfo GetListFilterInfo(List<FilterPolicy> policies, IAveList list);

        FolderInfo GetFolderFilterInfo(List<FilterPolicy> policies, IAveFolder folder);

        DocumentInfo GetDocumentFilterInfo(List<FilterPolicy> tempPolices, IAveFile file, IAveListItem listItem);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="policies"></param>
        /// <param name="item"></param>
        /// <param name="uiVersion"></param>
        /// <param name="file"></param>
        /// <param name="query">当前仅支持Local，不支持Office365的Native方式，外围调用时，如果为365，请传null</param>
        /// <returns></returns>
        DocumentInfo GetDocumentFilterInfo(List<FilterPolicy> policies, IAveListItem listItem, int uiVersion, IAveFile file, IAveDiscoveryQuery query = null);

        ItemInfo GetItemFilterInfo(List<FilterPolicy> tempPolices, IAveListItem listItem);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="policies"></param>
        /// <param name="listItem"></param>
        /// <param name="uiVersion"></param>
        /// <param name="query">当前仅支持Local，不支持Office365的Native方式，外围调用时，如果为365，请传null</param>
        /// <returns></returns>
        ItemInfo GetItemFilterInfo(List<FilterPolicy> policies, IAveListItem listItem, int uiVersion, IAveDiscoveryQuery query = null);

        AttachmentInfo GetAttachmentFilterInfo(List<FilterPolicy> policies, IAveFile file, IAveListItem listItem);
    }
}
