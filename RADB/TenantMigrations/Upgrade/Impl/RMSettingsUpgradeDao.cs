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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Core;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.TenantMigrations.Upgrade.Impl
{
    public class RMSettingsUpgradeDao
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RMSettingsUpgradeDao));

        private static readonly string RMRecordOwnersTableName = "RMRecordOwners";

        private static readonly List<KeyValuePair<string, int>> NeedUpgradeTableNames = new List<KeyValuePair<string, int>>
        {
            new KeyValuePair<string, int>("RMSharePointSettings", 0),
            new KeyValuePair<string, int>("RMExchangeOnlineSettings", 1),
            new KeyValuePair<string, int>("RMPhysicalRecordSettings", 2),
            new KeyValuePair<string, int>("RMFileSystemSettings", 3),
        };

        public void Upgrade(RMDbContext context)
        {
            Logger.Info("Begin upgrade setting tables.");
            foreach(var settingTableName in NeedUpgradeTableNames)
            {
                UpgradeSettingData(context, settingTableName.Key, settingTableName.Value);
            }
            Logger.Info("End upgrade setting tables.");
        }

        private void UpgradeSettingData(RMDbContext context, string settingTableName, int sourceType)
        {
            try
            {
                Logger.Info($"Begin upgrade {settingTableName}.");

                var sql = $"SELECT setting.Id FROM {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.{SecurityUtils.SanitizeSQLParameterName(settingTableName)} as setting RIGHT JOIN {context.SchemaName}.{RMRecordOwnersTableName} as owner ON setting.Id = owner.SPSettingId WHERE setting.ApprovalType = 0 and owner.SettingType = @p0 GROUP BY setting.Id HAVING COUNT(setting.Id) > 0";
                var needUpgradeSettingIds = context.Database.SqlQuery<int>(sql, sourceType);

                if(needUpgradeSettingIds.Count() == 0)
                {
                    Logger.Info($"[{settingTableName}] no setting need upgrade.");
                    return;
                }

                Logger.Info($"[{settingTableName}] need upgrade setting ids: [{string.Join(",", needUpgradeSettingIds)}]");
                /* Fortify Issue Type: SQL Injection
                 */

                var inClauseParamName = DatabaseUtility.BuildInClause(needUpgradeSettingIds, out var paramList);
                var upgradeSql = $"UPDATE {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.{SecurityUtils.SanitizeSQLParameterName(settingTableName)} SET ApprovalType = 2 WHERE Id in {inClauseParamName}";
                var upgradeCount = context.Database.ExecuteSqlCommand(upgradeSql, paramList.ToArray());

                Logger.Info($"End upgrade {settingTableName}. Upgrade count: [{upgradeCount}].");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while upgrade {settingTableName}. Error: {e}");
            }
        }
    }
}
