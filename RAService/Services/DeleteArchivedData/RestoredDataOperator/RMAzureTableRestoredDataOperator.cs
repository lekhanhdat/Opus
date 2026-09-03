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
using AvePoint.RA.DB.AzureTable;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.DeleteArchivedData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DeleteArchivedData.RestoredDataOperator
{
    public class RMAzureTableRestoredDataOperator : IRMRestoredDataOperator
    {
        public string Sign => "AzureTable";

        private readonly RestoredSitesInfo _restoredSiteInfo;

        private readonly RMDeleteArchivedDataSettingManager _settingManager;

        private readonly string _sitePartitionKey;

        public RMAzureTableRestoredDataOperator(
            RestoredSitesInfo restoredSiteInfo, 
            RMDeleteArchivedDataSettingManager settingManager)
        {
            _restoredSiteInfo = restoredSiteInfo;
            _settingManager = settingManager;
            _sitePartitionKey = restoredSiteInfo.SiteUrl.ToLower().Replace('/', '_').Replace('\\', '_').Replace('#', '_').Replace('?', '_').Replace('&', '_').Replace('=', '_').Replace('+', '_');
        }

        public IEnumerable<RMRestoredItem> ReadItems()
        {
            string continuationToken = null;
            do
            {
                var (token, items) = RMRecordStorageAzureTableContext.NeedDeleteArchivedDataList
                    .QueryWithPagination(item => item.PartitionKey == _sitePartitionKey, 100, continuationToken)
                    .GetAwaiter().GetResult();
                continuationToken = token;
                foreach(var item in items)
                {
                    if(!_settingManager.HasTheDeletionTimeBeenReached(item.RestoredTicks))
                    {
                        yield break;
                    }

                    yield return RMRestoredItem.FromContract(item);
                }
            } while (continuationToken != null);
        }

        public void DeleteItem(RMRestoredItem item)
        {
            RMRecordStorageAzureTableContext.NeedDeleteArchivedDataList
                .Delete(_sitePartitionKey, item.AzureTableItemId)
                .GetAwaiter().GetResult();
        }

        public bool HasRemaingItems()
        {
            return RMRecordStorageAzureTableContext.NeedDeleteArchivedDataList
                .FirstOrDefault(item => item.PartitionKey == _sitePartitionKey)
                .GetAwaiter().GetResult() != null;
        }

        public void Close()
        {
            // Do nothing
        }
    }
}
