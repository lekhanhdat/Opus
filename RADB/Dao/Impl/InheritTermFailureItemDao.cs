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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class InheritTermFailureItemDao : IInheritTermFailureItemDao
    {
        protected static readonly RALogger logger = RALogger.GetInstance(typeof(InheritTermFailureItemDao));
        private const string TablePrefix = "RECOInheritTermFailure";
        private string connectionString => RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
        private string GetTableName(string tenantGroupId) => string.Concat(TablePrefix, tenantGroupId.Replace("-", string.Empty));

        public bool Add(string tenantGroupId, List<InheritTermFailureItemEntity> entities)
        {
            try
            {
                AzureTableStorageUtility.AddAzureTableEntities(connectionString, GetTableName(tenantGroupId), entities);
                return true;
            }
            catch (Exception ex)
            {
                logger.Warn($"[InheritTermFailureItemDao] Add failed: {ex.Message}");
                return false;
            }
        }

        public List<InheritTermFailureItemEntity> GetInheritFailed(string tenantGroupId, string siteId, string listId)
        {
            var builder = new AzureTableQueryConditionBuilder();
            builder.AppendAndQuery("PartitionKey", "eq", siteId);
            builder.AppendAndQuery("ListId", "eq", listId);
            builder.AppendAndQuery("IsInheritFailed", "eq", true);
            string filter = builder.ToString();
            return AzureTableStorageUtility.RetrieveTableEntitiesInCondition<InheritTermFailureItemEntity>(connectionString, GetTableName(tenantGroupId), filter).ToList();
        }

        public bool Remove(string tenantGroupId, IList<InheritTermFailureItemEntity> entities)
        {
            try
            {
                AzureTableStorageUtility.DeleteTableEntities(connectionString, GetTableName(tenantGroupId), entities);
                return true;
            }
            catch (Exception ex)
            {
                logger.Warn($"[InheritTermFailureItemDao] Remove failed: {ex.Message}");
                return false;
            }
        }

        public bool RemoveAll(string tenantGroupId, string siteId)
        {
            var builder = new AzureTableQueryConditionBuilder();
            builder.AppendAndQuery("PartitionKey", "eq", siteId);
            string filter = builder.ToString();
            var all = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<InheritTermFailureItemEntity>(connectionString, GetTableName(tenantGroupId), filter).ToList();
            if (all.Count == 0) return true;
            return Remove(tenantGroupId, all);
        }
    }
}
