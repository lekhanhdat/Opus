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
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using ExchangeBackupUtility;
using Microsoft.Exchange.WebServices.Data;

namespace AvePoint.RA.RAExchange.Discover.DiscoverImpl
{
    public class SearchDiscover : IBatchDiscover
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(SearchDiscover));
        private readonly RMEXODiscoverHelper discoverHelper = null;
        private SearchFilter searchFilter = null;
        public SearchDiscover(RMEXODiscoverHelper helper, SearchFilter searchFilter = null)
        {
            this.discoverHelper = helper;
            this.searchFilter = searchFilter;
        }

        public IEnumerable<ExchangeItemGroup> GetGroupedItems(ExchangeFolder folder, SearchFilter extraFilter = null)
        {
            SearchFilter filter = null;
            if (this.searchFilter != null && extraFilter != null)
            {
                var termpFilter = new SearchFilter.SearchFilterCollection(LogicalOperator.And);
                termpFilter.Add(this.searchFilter);
                termpFilter.Add(extraFilter);
                filter = termpFilter;
            }
            else if (this.searchFilter == null && extraFilter != null)
            {
                filter = extraFilter;
            }
            else
            {
                filter = this.searchFilter;
            }
            logger.Info($"Begin to search folder:{folder.DisplayFolderPath} with filter. Original filter:{GetFilterString(searchFilter)} Extra filter:{GetFilterString(extraFilter)}");
            return discoverHelper.FindGroupedItems(folder, filter).GetConsumingEnumerable();
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
}
