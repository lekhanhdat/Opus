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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Bulk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Dao;

namespace AvePoint.RA.SharePoint.ArchiverCommon
{
    public class CosmosDBManualDataUpdater
    {
        private static readonly IRALogger Logger = RALogger.GetInstance(typeof(CosmosDBManualDataUpdater));
        private static readonly ICosmosBulkOperator CosmosOperator;
        private static readonly IExplorerDao ExplorerDao = new ExplorerDao();
        private static readonly bool IsEnableBulkOperation;
        private static int BulkSize;
        static CosmosDBManualDataUpdater()
        {
            var keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
            IsEnableBulkOperation = keyValueDao.IsCosmosBulkOperationEnabled();
            Logger.Info($"Current tenant is enable bulk operation? [{IsEnableBulkOperation}].");
            if (!IsEnableBulkOperation)
            {
                return;
            }
            BulkSize = keyValueDao.GetCosmosBulkInsertOperationBufferSize();
            if (BulkSize <= 0)
            {
                BulkSize = CosmosBulkOperator.DefualtBufferSize;
            }

            CosmosOperator = CosmosBulkOperator.Instance;
            CosmosOperator.Start(BulkSize, SucceedProcessRecord, FailedProcessRecord);
            Logger.Info($"Succeed start cosmos db bulk operator. Bulk size: [{BulkSize}].");
        }

        public static void Add(Record item)
        {
            if (!IsEnableBulkOperation)
            {
                ExplorerDao.Upsert(item);
                //DataSyncJobManager.AddSucceedJobDetail(item);
                return;
            }
            CosmosOperator.Add(item);
        }

        public static void Commit()
        {
            if (!IsEnableBulkOperation)
            {
                return;
            }
            Logger.Info($"Start cosmos db bulk operator commit.");
            CosmosOperator.Complete();
            CosmosOperator.Reset();
            CosmosOperator.Start(BulkSize, SucceedProcessRecord, FailedProcessRecord);
            Logger.Info($"Succeed start cosmos db bulk operator. Bulk size: [{BulkSize}].");
            Logger.Info($"End cosmos db bulk operator commit.");
        }

        public static void WaitComplete()
        {
            if (!IsEnableBulkOperation)
            {
                return;
            }
            Logger.Info($"Waiting cosmos db bulk operator job complete.");
            CosmosOperator.Complete();
            Logger.Info($"The cosmos db bulk operator job complete.");
        }

        private static async Task SucceedProcessRecord(Record item)
        {
            //DataSyncJobManager.AddSucceedJobDetail(item);
            Logger.Info($"The item [{item.Id}] sync to cosmos db success.");
        }

        private static void FailedProcessRecord(Record item, Exception e)
        {
            Logger.Error($"The item [{item.Id}] sync to cosmos db failed. Error: {e}");
           // DataSyncFailedItemManager.AddFailedItem(item);
            //DataSyncJobManager.AddFailedJobDetail(item, e.Message);
        }
    }
}
