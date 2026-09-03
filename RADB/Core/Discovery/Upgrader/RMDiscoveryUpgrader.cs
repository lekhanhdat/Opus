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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Core.Discovery.Upgrader.Table;
using AvePoint.RA.DB.Model.Discovery;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Core.Discovery.Upgrader
{
    public static class RMDiscoveryUpgrader
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMDiscoveryUpgrader));

        private static readonly IRMDiscoveryTableUpgrader TABLE_UPGRADER = new RMDiscoveryOct2026TableUpgrader();

        public const RMDiscoveryUpgradeVersion CURRENT_VERSION = RMDiscoveryUpgradeVersion.Oct2026;

        public static async Task UpgradeAsync()
        {
            if(CURRENT_VERSION != TABLE_UPGRADER.Version)
            {
                s_logger.Error($"The upgrader current version [{CURRENT_VERSION}] not match will execute upgrader version [{TABLE_UPGRADER.Version}]. Skipped");
                return;
            }

            RMTenantDiscoveryDBInfo dbInfo = null;
            using (var sysContext = RMDBContextManager.GetSystemDBContext())
            {
                dbInfo = await sysContext.TenantDiscoveryDBInfoes.FirstOrDefaultAsync(item => item.Id == TenantLocalValue.LogonGroupId);
                if (dbInfo == null)
                {
                    return;
                }
            }

            await InitUpgradeInfoTableAsync();

            var needUpgrade = await CheckIfUpgradeIsNeededAsync();
            if (!needUpgrade)
            {
                s_logger.Info($"The database [{dbInfo.DatabaseName}] is already upgraded to version [{CURRENT_VERSION}].");
                return;
            }

            var upgradeInfo = new RMDiscoveryUpgradeInfo
            {
                ReleaseVersion = CURRENT_VERSION,
                StartTime = DateTime.UtcNow.Ticks,
                CommonTableUpgradeSucceed = await TABLE_UPGRADER.CommonUpgradeAsync(),
                Office365TableUpgradeSucceed = await TABLE_UPGRADER.Office365UpgradeAsync(),
                GoogleTableUpgradeSucceed = await TABLE_UPGRADER.GoogleUpgradeAsync(),
                SalesforceTableUpgradeSucceed = await TABLE_UPGRADER.SalesforceUpgradeAsync(),
                AOSPTableUpgradeSucceed = await TABLE_UPGRADER.AOSPUpgradeAsync(),
            };

            upgradeInfo.Succeed = upgradeInfo.CommonTableUpgradeSucceed && upgradeInfo.Office365TableUpgradeSucceed && upgradeInfo.GoogleTableUpgradeSucceed && upgradeInfo.SalesforceTableUpgradeSucceed && upgradeInfo.AOSPTableUpgradeSucceed;
            upgradeInfo.EndTime = DateTime.UtcNow.Ticks;
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            efContext.UpgradeInfoes.Add(upgradeInfo);
            await efContext.SaveChangesAsync();
        }

        private static async Task InitUpgradeInfoTableAsync()
        {
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var tableSet = new RMDiscoveryDBTableSet(typeof(RMDiscoveryUpgradeInfo), "dbo");
            var existsSql = tableSet.GetExistsTableSql();
            var exists = await context.ExecuteScalarAsync<int>(existsSql);
            if (exists == 1)
            {
                await AddMissingColumnsAsync(context);
                return; 
            }

            var createSql = tableSet.GetCreateTableSql();
            await context.ExecuteNonQueryAsync(createSql);
        }

        private static async Task<bool> CheckIfUpgradeIsNeededAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return !(await efContext.UpgradeInfoes.AnyAsync(item => item.ReleaseVersion == CURRENT_VERSION && item.Succeed == true));
        }

        private static async Task AddMissingColumnsAsync(RMDiscoveryDBContext context)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryUpgradeInfo));

            foreach (var column in tableInfo.Columns)
            {
                try
                {
                    var columnExistsSql = $@"
                SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = '{tableInfo.Name}' AND COLUMN_NAME = '{column.Name}'";

                    var columnExists = await context.ExecuteScalarAsync<int>(columnExistsSql) == 1;

                    if (!columnExists)
                    {
                        var addColumnSql = $@"
                    ALTER TABLE [dbo].[{tableInfo.Name}] 
                    ADD [{column.Name}] {column.TypeName} 
                    {(column.DefaultValue != null ? $"NOT NULL DEFAULT {column.DefaultValue}" : "NULL")}";

                        await context.ExecuteNonQueryAsync(addColumnSql);
                        
                        if (column.DefaultValue != null)
                        {
                            var updateExistingDataSql = $@"
                        UPDATE [dbo].[{tableInfo.Name}] 
                        SET [{column.Name}] = {column.DefaultValue}
                        WHERE [{column.Name}] IS NULL";

                            await context.ExecuteNonQueryAsync(updateExistingDataSql);
                        }

                        s_logger.Info($"Added new column [{column.Name}] to table [{tableInfo.Name}] with default value: {column.DefaultValue ?? "NULL"}");
                    }
                }
                catch (Exception ex)
                {
                    s_logger.Error($"Upgrade [{tableInfo.Name}] column [{column.Name}] failed. Error: {ex}");
                }
            }
        }
    }
}
