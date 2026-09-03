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
using AvePoint.GCommon.Utility;
using AvePoint.GCommon;
using System.Reflection;

namespace AvePoint.Wrapper.Restore
{
    class HSMLookupDataFormat : LookupDataFormat
    {
        protected static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public HSMLookupDataFormat(AveXmlField xmlField, IAveField destField, AveSPItem mItem, int originalVesrion)
            : base(xmlField, destField, mItem, originalVesrion)
        {
        }

        protected override object WrapperSingleResult(LookupItemValue itemValue)
        {
            return itemValue;
        }

        protected override object WrapperMultiValues(List<LookupItemValue> lookupItemVaues)
        {
            return lookupItemVaues;
        }

        protected override LookupItemValue GetDestItemValue(int sourceItemId, Guid itemGuid, string leafName, Guid lookupListId, Guid lookupWebId)
        {
            var result = new LookupItemValue();
            if (Guid.Empty != lookupListId && Guid.Empty != lookupWebId)
            {
                if (!mItem.ParentList.lookupItemUniqueIdCache.ContainsKey(lookupWebId))
                {
                    InitCache(mItem.ParentList.lookupItemUniqueIdCache, itemGuid, lookupListId, lookupWebId);
                }
                var listCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<int, Guid>>>>();
                if (mItem.ParentList.lookupItemUniqueIdCache.TryGetValue(lookupWebId, out listCache))
                {
                    var fieldCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<int, Guid>>>();
                    if (listCache.TryGetValue(lookupListId, out fieldCache))
                    {
                        var itemCache = new Dictionary<Guid, Dictionary<int, Guid>>();
                        if (fieldCache.TryGetValue(destField.ID, out itemCache))
                        {
                            var lookupItemInfo = new Dictionary<int, Guid>();
                            if (itemCache.TryGetValue(itemGuid, out lookupItemInfo))
                            {
                                result.ItemRowId = lookupItemInfo.Keys.First();
                                result.ItemUniqueId = lookupItemInfo.Values.First();
                            }
                        }
                    }
                }
            }
            return result;
        }

        private void InitCache(Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<int, Guid>>>>> lookupItemUniqueIdCache, Guid itemGuid, Guid lookupListId, Guid lookupWebId)
        {
            try
            {
                var lookupListCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<int, Guid>>>>();
                lookupItemUniqueIdCache[lookupWebId] = lookupListCache;
                IAveWeb lookupWeb = mItem.ParentWeb.AveWeb;
                if (lookupWebId != mItem.ParentWeb.AveWeb.ID)
                {
                    lookupWeb = mItem.ParentSite.AveSite.OpenWeb(lookupWebId);
                }
                var lookupList = lookupWeb.GetList(lookupListId);
                if (lookupList != null)
                {
                    var allitemsCache = new Dictionary<Guid, Dictionary<int, Guid>>();
                    lookupListCache[lookupListId] = new Dictionary<Guid, Dictionary<Guid, Dictionary<int, Guid>>>() { { destField.ID, allitemsCache } };
                    foreach (var item in lookupList.Items)
                    {
                        allitemsCache[item.GetTPGuid()] = new Dictionary<int, Guid> { { item.ID, item.UniqueId } };
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while init the lookup item cache. Error:{0}", e);
            }
        }
    }
}
