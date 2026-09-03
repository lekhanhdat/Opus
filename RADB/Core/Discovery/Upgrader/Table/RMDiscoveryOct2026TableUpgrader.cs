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
using AvePoint.RA.DB.Model.Discovery.Plan;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Core.Discovery.Upgrader.Table
{
    public class RMDiscoveryOct2026TableUpgrader : IRMDiscoveryTableUpgrader
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOct2026TableUpgrader));

        public RMDiscoveryUpgradeVersion Version => RMDiscoveryUpgradeVersion.Oct2026;

        public async Task<bool> Office365UpgradeAsync()
        {
            var result = true;
            try
            {
                _logger.Info($"Start execute Office365 upgrade in {Version}.");

                var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryPlanProfile));
                SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);

                var sql = $@"IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = '{tableInfo.Name}' AND TABLE_SCHEMA = 'dbo' AND COLUMN_NAME = 'StubSettingId'
)
BEGIN
    ALTER TABLE [dbo].[{tableInfo.Name}] ADD StubSettingId NVARCHAR(MAX) NULL
END";
                await using var context = await RMDiscoveryDBManager.GetContextAsync();
                await context.ExecuteNonQueryAsync(sql);

                _logger.Info($"End execute Office365 upgrade in {Version} is [{result}].");
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while execute Office365 upgrade with plan profile table [{nameof(RMDiscoveryPlanProfile)}] in {Version}. Error: {ex}");
                result = false;
            }
            return result;
        }

        public Task<bool> CommonUpgradeAsync()
        {
            return Task.FromResult(true);
        }

        public Task<bool> GoogleUpgradeAsync()
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