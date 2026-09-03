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
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Core.Discovery.Upgrader.Table
{
    public class RMDiscoveryApr2025TableUpgrader : IRMDiscoveryTableUpgrader
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryApr2025TableUpgrader));

        public RMDiscoveryUpgradeVersion Version => RMDiscoveryUpgradeVersion.Apr2025;

        public async Task<bool> CommonUpgradeAsync()
        {

            async Task<bool> ConfigurationTableUpgradeAsync()
            {
                try
                {
                    var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryConfiguration));
                    SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
                    var sql = $@"IF EXISTS (
SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableInfo.Name}' AND COLUMN_NAME = 'ScopeSource'
) 
BEGIN
ALTER TABLE [dbo].[{tableInfo.Name}] DROP COLUMN ScopeSource
END";
                    await using var context = await RMDiscoveryDBManager.GetContextAsync();
                    await context.ExecuteNonQueryAsync(sql);
                    return true;
                }
                catch(Exception e)
                {
                    _logger.Error($"An error occurred while execute common upgrade with configuration table in {Version}. Error: {e}");
                    return false;
                }
            }

            #region Obsolete
            //            async Task<bool> MainJobTableUpgradeAsync()
            //            {
            //                try
            //                {
            //                    var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365MainJob));
            //                    var addColumnSql = $@"IF NOT EXISTS (
            //SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableInfo.Name}' AND COLUMN_NAME = 'DataSource'
            //)
            //BEGIN
            //ALTER TABLE [dbo].[{tableInfo.Name}] ADD DataSource INT
            //END";
            //                    await using var context = await RMDiscoveryDBManager.GetContextAsync();
            //                    await context.ExecuteNonQueryAsync(addColumnSql);

            //                    var setValueSql = $"UPDATE [dbo].[{tableInfo.Name}] SET DataSource = 1";
            //                    await context.ExecuteNonQueryAsync(setValueSql);

            //                    return true;
            //                }
            //                catch(Exception e)
            //                {
            //                    _logger.Error($"An error occurred while execute common upgrade with main job table in {Version}. Error: {e}");
            //                    return false;
            //                }
            //            }

            #endregion

            try
            {
                _logger.Info($"Start execute common upgrade in {Version}.");

                var result = true;

                result &= await ConfigurationTableUpgradeAsync();

                _logger.Info($"End execute common upgrade in {Version} is [{result}].");

                return result;
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while execute common upgrade in {Version}. Error: {e}");
                return false;
            }
        }

        public Task<bool> GoogleUpgradeAsync()
        {
            return Task.FromResult(true);
        }

        public Task<bool> Office365UpgradeAsync()
        {
            return Task.FromResult(true);
        }

        public Task<bool> SalesforceUpgradeAsync()
        {
            return Task.FromResult(true);
        }

        public Task<bool> AOSPUpgradeAsync()
        {
            return Task.FromResult(true);
        }
    }
}
