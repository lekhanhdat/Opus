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
using AvePoint.GCommon;
using AvePoint.RA.Common;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAExchange.Disposal.Action
{
    public class ExchangeDeletionUtil
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public void DeleteExchangeItem(Item item)
        {
            try
            {
                using (var performance = new PerformanceScope("ExchangeDeletionUtil.DeleteExchangeItem", "", true))
                {
                    item.Delete(DeleteMode.HardDelete).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                logger.Error("Cannot delete the item, item Subject : {1}, reason : {0}.", ex.ToString(), item?.Id?.ToString() ?? string.Empty);
                throw;
            }
        }

        public void DeleteExchangeItem(ExchangeService service, IEnumerable<ItemId> itemIds)
        {
            try
            {
                service.DeleteItems(itemIds, DeleteMode.HardDelete, null, null);
            }
            catch (Exception ex)
            {
                logger.Error("Cannot delete the items, reason : {0}.", ex.ToString());
            }
        }

        public void DeleteExchangeItem(ExchangeService service, string itemId)
        {
            try
            {
                service.DeleteItems(GetItemId(itemId), DeleteMode.HardDelete, null, null);
            }
            catch (Exception ex)
            {
                logger.Error("Cannot delete the item, item id : {1}, reason : {0}.", ex.ToString(), itemId);
            }
        }

        public void DeleteExchangeItem(ExchangeService service, ItemId itemId)
        {
            try
            {
                service.DeleteItems(GetItemId(itemId), DeleteMode.HardDelete, null, null);
            }
            catch (Exception ex)
            {
                logger.Error("Cannot delete the item, item id : {1}, reason : {0}.", ex.ToString(), itemId.ToString());
            }
        }

        public IEnumerable<ItemId> GetItemId(string id)
        {
            yield return new ItemId(id);
        }

        public IEnumerable<ItemId> GetItemId(ItemId id)
        {
            yield return id;
        }
    }
}
