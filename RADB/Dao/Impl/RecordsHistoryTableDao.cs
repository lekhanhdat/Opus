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
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Core;
using AvePoint.RA.CommonUtil;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RecordsHistoryTableDao : IRecordsHistoryTableDao
    {
        protected static readonly RALogger logger = RALogger.GetInstance(typeof(RecordsHistoryTableDao));
        private const string TablePrefix = "RECORecordsHistory";
        private string GetTableName(string tenantGroupId)
        {
            return string.Concat(TablePrefix, tenantGroupId.Replace("-", string.Empty));
        }
        public IEnumerable<RecordHistoryTableEntity> AddRecordsHistory(string connectString, string tenantGroupId, List<RecordHistoryTableEntity> entities)
        {
            string connectStr = connectString;
            string tableName = GetTableName(tenantGroupId);
            var mEntities = AzureTableStorageUtility.AddAzureTableEntities<RecordHistoryTableEntity>(connectStr, tableName, entities);
            return mEntities;
        }

        public IEnumerable<RecordHistoryTableEntity> GetRecordsHistory(string connectString, string tenantGroupId, string recordsId)
        {
            string connectStr = connectString;
            string tableName = GetTableName(tenantGroupId);
            string partionCondition = new AzureTableQueryConditionBuilder(recordsId).ToString();
            var result = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<RecordHistoryTableEntity>(connectStr, tableName, partionCondition.ToString())
               .OrderByDescending(e => e.ExecuteOn);
            return result;
        }
        
        public void CloneMoveHistoryRecords(string connectString, string tenantGroupId, Guid sourceId, Guid destId)
        {
            string connectStr = connectString;
            string tableName = GetTableName(tenantGroupId);
            string partionCondition = new AzureTableQueryConditionBuilder(sourceId.ToString()).ToString();
            var entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<RecordHistoryTableEntity>(connectStr, tableName, partionCondition.ToString())
               .OrderByDescending(e => e.ExecuteOn).ToList();
            entities.ForEach(e =>
            {
                e.PartitionKey = destId.ToString();
            });
            AzureTableStorageUtility.AddAzureTableEntities(connectStr, tableName, entities);
        }
    }
}
