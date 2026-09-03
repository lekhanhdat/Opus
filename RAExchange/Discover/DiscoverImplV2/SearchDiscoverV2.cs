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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using ExchangeBackupUtility.Graph;
using Microsoft.Exchange.WebServices.Data;

namespace AvePoint.RA.RAExchange.Discover.DiscoverImplV2;

public class SearchDiscoverV2 : BaseDiscover, IBatchDiscoverV2
{
    private readonly RALogger _logger = RALogger.GetInstance(typeof(SearchDiscoverV2));

    
    private readonly SearchFilter _searchFilter;

    public SearchDiscoverV2( SearchFilter searchFilter = null)
    {
        _searchFilter = searchFilter;
    }
    public IEnumerable<IExchangeItemGroup> GetGroupedItems(IExchangeFolder folder, SearchFilter extraFilter = null)
    {
        SearchFilter filter = null;
        if (_searchFilter != null && extraFilter != null)
        {
            var tempFilter = new SearchFilter.SearchFilterCollection(LogicalOperator.And);
            tempFilter.Add(_searchFilter);
            tempFilter.Add(extraFilter);
            filter = tempFilter;
        }
        else if (_searchFilter == null && extraFilter != null)
        {
            filter = extraFilter;
        }
        else
        {
            filter = _searchFilter;
        }
        _logger.Info($"Begin to search folder:{folder.DisplayFolderPath} with filter. Original filter:{GetFilterString(_searchFilter)} Extra filter:{GetFilterString(extraFilter)}");
        return FindGroupedItems(folder, filter).GetConsumingEnumerable();
    }
    
    private string GetFilterString(SearchFilter filter)
    {
        try
        {
            return filter != null ? SerializerHelper.SerializeByJsonConvert(filter) : string.Empty;
        }
        catch (Exception e)
        {
            return filter?.ToString();
        }
    }
}