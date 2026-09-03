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
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Core.Discovery.Upgrader.Table
{
    public class RMDiscoveryOct2025TableUpgrader : IRMDiscoveryTableUpgrader
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOct2025TableUpgrader));

        public RMDiscoveryUpgradeVersion Version => RMDiscoveryUpgradeVersion.Oct2025;

        private readonly IRMDiscoveryAOSPTenantDao _aospTenantInfoDao = new RMDiscoveryAOSPTenantDao();

        public async Task<bool> AOSPUpgradeAsync()
        {
            async Task<bool> OptimizationTableUpgradeAsync()
            {
                try
                {
                    var result = true;
                    var analysisJobTableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryAOSPAnalysisJob));
                    result &= await UpdateTableAsync(analysisJobTableInfo);
                    var mainJobTableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryAOSPMainJob));
                    result &= await UpdateTableAsync(mainJobTableInfo);
                    var discoveryJobTableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryAOSPDiscoveryJob));
                    result &= await UpdateTableAsync(discoveryJobTableInfo);

                    return result;
                }
                catch (Exception e)
                {
                    _logger.Error($"An error occurred while execute common upgrade with configuration table in {Version}. Error: {e}");
                    return false;
                }
            }

            async Task<bool> UpdateTableAsync(RMDiscoveryTableInfo tableInfo)
            {
                SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
                var result = true;
                try
                {
                    var sql = $@"IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = '{tableInfo.Name}' AND TABLE_SCHEMA = 'dbo' AND COLUMN_NAME = 'Comment'
)
BEGIN
    ALTER TABLE [dbo].[{tableInfo.Name}] ADD Comment NVARCHAR(MAX) NOT NULL DEFAULT ('')
END";
                    await using var context = await RMDiscoveryDBManager.GetContextAsync();
                    await context.ExecuteNonQueryAsync(sql);
                }
                catch (Exception ex)
                {
                    _logger.Error($"An error occurred while execute common upgrade with configuration table [{tableInfo.Name}] in {Version}. Error: {ex}");
                    result = false;
                }
                return result;
            }

            try
            {
                _logger.Info($"Start execute common upgrade in {Version}.");

                var result = true;

                result &= await OptimizationTableUpgradeAsync();

                _logger.Info($"End execute common upgrade in {Version} is [{result}].");

                return result;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while execute common upgrade in {Version}. Error: {e}");
                return false;
            }
        }

        public Task<bool> CommonUpgradeAsync()
        {
            return Task.FromResult(true);
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
    }
}
