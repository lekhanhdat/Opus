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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.SPObjDiscover
{
    public interface ISPDiscoverHelper
    {
        BlockingCollection<IAveListItem> GetItems(AveDiscoverList list, List<AveCamlQuery> aveCamlQueries, bool scanALLIfNoQuery = false);
        BlockingCollection<IAveListItem> GetChangedItems(AveDiscoverList list, List<AveCamlQuery> aveCamlQueries, bool scanALLIfNoQuery = false);

        BlockingCollection<IAveListItem> GetItems(AveDiscoverList list, AveDiscoverFolder folder);
        BlockingCollection<IAveListItem> GetChangedItems(AveDiscoverList list, AveDiscoverFolder folder);
        //#region full discover
        //IEnumerable<AveDiscoverWeb> GetWebs(AveDiscoverSite site);
        //IEnumerable<AveDiscoverList> GetLists(AveDiscoverWeb web, bool skipSystemList = true);
        //IEnumerable<AveDiscoverFolder> GetFolders(AveDiscoverList list);
        //IEnumerable<AveDiscoverItem> GetItems(AveDiscoverFolder folder);
        //IEnumerable<AveDiscoverItem> GetItems(AveDiscoverList list);
        //#endregion

        //#region incremental
        //IEnumerable<AveDiscoverWeb> GetChangedtWebs(AveDiscoverSite site);
        //IEnumerable<AveDiscoverList> GetChangedLists(AveDiscoverWeb web, bool skipSystemList = true);
        //IEnumerable<AveDiscoverFolder> GetChangedFolders(AveDiscoverList list);
        //IEnumerable<AveDiscoverItem> GetChangedItems(AveDiscoverFolder folder);
        //IEnumerable<AveDiscoverItem> GetChangedItems(AveDiscoverList list);
        //#endregion
    }
}
