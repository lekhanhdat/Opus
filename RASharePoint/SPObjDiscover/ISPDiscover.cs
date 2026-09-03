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
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.SPObjDiscover
{
    public interface ISPDiscover
    {
        IEnumerable<AveDiscoverWeb> GetWebs(AveDiscoverSite site);

        IEnumerable<AveDiscoverList> GetLists(AveDiscoverWeb web, bool skipSystemList = true);

        IEnumerable<AveDiscoverFolder> GetSubFolders(AveDiscoverList list);
        IEnumerable<AveDiscoverFolder> GetSubFolders(AveDiscoverFolder folder);
        IEnumerable<AveDiscoverItem> GetItems(IAveList list, AveDiscoverFolder folder);
        IEnumerable<AveDiscoverItem> GetItems(IAveList list, AveDiscoverFolder folder, ref string pagerInfo);
        IEnumerable<AveDiscoverItem> GetItems(AveDiscoverList list, IAveList aveList);
        IEnumerable<AveDiscoverItem> GetItems(AveDiscoverList list, IAveList aveList, ref string pagerInfo);
        /// <summary>
        /// get all items under the list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="aveCamlQueries"></param>
        /// <returns></returns>
        IEnumerable<IAveListItem> GetAllItems(AveDiscoverList list, out long totalCount, List<AveCamlQuery> aveCamlQueries = null);
        AveDiscoverFolder GetRootFolder(AveDiscoverList list);
    }
}
