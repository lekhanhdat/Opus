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
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Common.Util;
using System.Data.SqlClient;
using AvePoint.RA.DB.Core;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class StorageAccountDao : IStorageAccountDao
    {
        public StorageInfo GetAvailableStorageAccount(int requiredSize)
        {
            var totalSizeQuotaClause =
                       " (CASE                                                                                            "
                     + "	WHEN (select sum(g.StorageQuota) from TenantInfo as g where g.StorageAccountName = d.AccountName) IS NULL THEN 0 "
                     + "	ELSE (select sum(g.StorageQuota) from TenantInfo as g where g.StorageAccountName = d.AccountName)                "
                     + " END)                                                                                             ";
            var getAvaliableStorageSql = string.Format("select top 1 d.AccountName, d.AccessKey, d.AccountType from StorageAccount as d"
                + " where (d.MaxSize - {0}) >= @RequiredSize"
                + " order by CreateTime asc", totalSizeQuotaClause);

            return DatabaseUtility.RetryPolicy.ExecuteAction<StorageInfo>(() =>
            {
                var sizeParam = new SqlParameter("RequiredSize", requiredSize);
                StorageInfo result = null;
                using (var ctx = RMDBContextManager.GetSystemSQLContext())
                {
                    //Fortify fix: Unreleased Resource: Database
                    using (var reader = ctx.ExecuteQuery(getAvaliableStorageSql, sizeParam))
                    {
                        if (reader.Read())
                        {
                            var name = reader.GetString(0);
                            var key = reader.GetString(1);
                            var ap = (StorageAccountType)reader.GetInt32(2);
                            result = new StorageInfo
                            {
                                AccountName = name,
                                AccessKey = key,
                                AccountType = ap
                            };
                        }
                    }
                }
                return result;
            });
        }
    }
}
