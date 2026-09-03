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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core.Upgrade;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.TenantMigrations.Upgrade.Impl
{
    public class RMBoardUpgradeDao : BaseDao<RMManualApprove>, IDbUpgradeDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMBoardUpgradeDao));
        public async Task UpgradeAsync(Core.RMDbContext context)
        {
            List<string> tableNames = new List<string> { "RMCPExportSettings", "BoardTotals", "RMDataOfDays", "RMTermUsages" };
            var index = 0;
            try
            {
            foreach (var tableName in tableNames)
            {
                if (tableName.Equals("RMCPExportSettings"))
                {
                    var sql = "update {0}." + SecurityUtils.SanitizeSQLSchemaName(tableName) + " set SourceFlag = 1 where SourceFlag = 0";
                    var newSql = string.Format(sql, context.SchemaName);
                    int row = context.Database.ExecuteSqlCommand(newSql);
                }
                else
                {
                    var sql = "Delete From {0}." + SecurityUtils.SanitizeSQLSchemaName(tableName) + " where SourceFlag = 0";
                    var newSql = string.Format(sql, context.SchemaName);
                    int row = context.Database.ExecuteSqlCommand(newSql);
                    if (row > 0 && index == 0)
                    {
                        var sql1 = "Delete From {0}.RMWaitingApprovalAssignees";
                        var sql2 = "Delete From {0}.RMSiteCollectionSizes";
                        var newSql1 = string.Format(sql1, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName));
                        var newSql2 = string.Format(sql2, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName));
                        int row1 = context.Database.ExecuteSqlCommand(newSql1);
                        int row2 = context.Database.ExecuteSqlCommand(newSql2);
                        index++;
                    }
                }
            }

        }
            catch (Exception ex)
            {
                logger.Error($"error occurred while upgrade board:{ ex.ToString() }");
    }
            
}
    }
}
