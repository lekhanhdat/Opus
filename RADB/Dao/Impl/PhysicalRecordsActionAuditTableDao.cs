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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class PhysicalRecordsActionAuditTableDao : IPhysicalRecordsActionAuditTableDao
    {
        protected static readonly RALogger logger = RALogger.GetInstance(typeof(PhysicalRecordsActionAuditTableDao));

        private const string TablePrefix = "RECOPhysicalRecordsActionAudit";

        private string ConnectionString = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];

        private string GetTableName(string tenantGroupId)
        {
            return string.Concat(TablePrefix, tenantGroupId.Replace("-", string.Empty));
        }
        public IEnumerable<PhysicalRecordActionAudit> AddPhysicalRecordsAudits(string tenantGroupId, List<PhysicalRecordActionAudit> entities)
        {
            string tableName = GetTableName(tenantGroupId);
            var mEntities = AzureTableStorageUtility.AddAzureTableEntities<PhysicalRecordActionAudit>(ConnectionString, tableName, entities);
            return mEntities;
        }

        public IEnumerable<PhysicalRecordActionAudit> GetPhysicalRecordsAudits(string tenantGroupId, string recordsId)
        {
            string tableName = GetTableName(tenantGroupId);
            string partionCondition = new AzureTableQueryConditionBuilder(recordsId).ToString();
            var result = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<PhysicalRecordActionAudit>(ConnectionString, tableName, partionCondition.ToString())
               .OrderByDescending(e => e.ExecuteOn).Take(20);
            return result;
        }
    }
}
