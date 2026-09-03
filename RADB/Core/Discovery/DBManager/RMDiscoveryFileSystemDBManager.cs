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
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem;
using AvePoint.RA.DB.Model.Discovery.FileSystem;

namespace AvePoint.RA.DB.Core.Discovery.DBManager
{
    public partial class RMDiscoveryDBManager
    {
        public static Task<RMDiscoveryDBEFContext> GetFileSystemEFContextAsync()
        {
            var schemaName = GetFileSystemSchemaName();
            return GetEFContextAsync(schemaName);
        }

        public static string GetFileSystemSchemaName()
        {
            return "s_fs_";
        }

        public static async Task<bool> CheckFileSystemTablesExistsAsync()
        {
            var exists = true;

            await using var context = await GetContextAsync();
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new(typeof(RMDiscoveryFSSizeRange), "dbo"),
                new(typeof(RMDiscoveryFSWithoutInDate), "dbo"),
                new(typeof(RMDiscoveryFSRuleInfo), "dbo"),
                new(typeof(RMDiscoveryFSMainJob), "dbo"),
                new(typeof(RMDiscoveryFSDiscoveryJob), "dbo"),
                new(typeof(RMDiscoveryFSAnalysisJob), "dbo"),
                new(typeof(RMDiscoveryFSAgentInfo), "dbo"),
                new (typeof(RMDiscoveryFSExecutionInfo), "dbo"),
                new (typeof(RMDiscoveryFSTagRuleInfo), "dbo"),
            };

            foreach (var tableSet in tableSets)
            {
                var existsSql = tableSet.GetExistsTableSql();
                var tableExists = await context.ExecuteScalarAsync<int>(existsSql);
                exists &= (tableExists == 1);
            }

            return exists;
        }

        public static async Task InitFileSytemDatabaseAsync()
        {
            var dbName = GetDatabaseName();
            var hasDatabase = await HasDatabaseAsync();
            try
            {
                if (!hasDatabase)
                {
                    await InitDatabaseAsync(dbName, RMAzureDBPerformanceLevel.BASIC, 2);
                }
                await InitBasicTablesAsync();
                await InitFileSytemBasicTablesAsync();
                await InitFileSystemBuildInDataListAsync();
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while init discovery filesystem database info. has database [{hasDatabase}]. Error: {e}");
                if (!hasDatabase)
                {
                    using var context = RMDBContextManager.GetSystemDBContext();
                    await DropDatabaseAsync(dbName);
                }
                throw;
            }
        }

        public static async Task InitFileSytemBasicTablesAsync()
        {
            await using var context = await GetContextAsync();

            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new(typeof(RMDiscoveryFSSizeRange), "dbo"),
                new(typeof(RMDiscoveryFSWithoutInDate), "dbo"),
                new(typeof(RMDiscoveryFSRuleInfo), "dbo"),
                new(typeof(RMDiscoveryFSAgentInfo), "dbo"),
                new(typeof(RMDiscoveryFSMainJob), "dbo"),
                new(typeof(RMDiscoveryFSDiscoveryJob), "dbo"),
                new(typeof(RMDiscoveryFSAnalysisJob), "dbo"),
                new (typeof(RMDiscoveryFSExecutionInfo), "dbo"),
                new (typeof(RMDiscoveryFSTagRuleInfo), "dbo"),
            };

            foreach (var tableSet in tableSets)
            {
                var existsSql = tableSet.GetExistsTableSql();
                var exists = await context.ExecuteScalarAsync<int>(existsSql);
                if (exists == 1)
                {
                    continue;
                }
                var createSql = tableSet.GetCreateTableSql();
                await context.ExecuteNonQueryAsync(createSql);
            }
        }

        public static async Task InitFileSystemBuildInDataListAsync()
        {
            var sizeRangeDao = new RMDiscoveryFSSizeRangeDao();
            var withoutDateDao = new RMDiscoveryFSWithoutInDateDao();
            await sizeRangeDao.InitBuildInDataAsync();
            await withoutDateDao.InitBuildInDataAsync();
        }

        public static async Task InitFileSystemBasicTablesAsync()
        {
            await using var context = await GetContextAsync();
            var schemaName = GetFileSystemSchemaName();
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryFSContainerInfo), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryFSConnectionInfo), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryFSFileExtension), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryFSAggregateTotalData), schemaName),
            };

            foreach (var tableSet in tableSets)
            {
                var existsSql = tableSet.GetExistsTableSql();
                var exists = await context.ExecuteScalarAsync<int>(existsSql);
                if (exists == 1)
                {
                    continue;
                }

                var createSql = tableSet.GetCreateTableSql();
                await context.ExecuteNonQueryAsync(createSql);

                foreach (var indexSql in tableSet.GetAddIndexSql())
                {
                    await context.ExecuteNonQueryAsync(indexSql);
                }
            }
        }

        public static async Task InitFileSystemRotTablesAsync()
        {
            await using var context = await GetContextAsync();
            var schemaName = GetFileSystemSchemaName();
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryFSBasicRuleLevelRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryFSBasicCategoryLevelRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryFSBasicRootLevelRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryFSContainerRuleLevelRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryFSContainerCategoryLevelRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryFSContainerRootLevelRotData), schemaName),
            };

            foreach (var tableSet in tableSets)
            {
                var existsSql = tableSet.GetExistsTableSql();
                var exists = await context.ExecuteScalarAsync<int>(existsSql);
                if (exists == 1)
                {
                    continue;
                }

                var createSql = tableSet.GetCreateTableSql();
                await context.ExecuteNonQueryAsync(createSql);

                foreach (var indexSql in tableSet.GetAddIndexSql())
                {
                    await context.ExecuteNonQueryAsync(indexSql);
                }
            }
        }

        public static async Task InitFileSystemInactiveTablesAsync(List<RMDiscoveryCustomColumn> inactiveDataCustomColumns = null)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetFileSystemSchemaName();
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryFSBasicInactiveData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryFSContainerInactiveData), schemaName),
            };

            foreach (var tableSet in tableSets)
            {
                var existsSql = tableSet.GetExistsTableSql();
                var exists = await context.ExecuteScalarAsync<int>(existsSql);
                if (exists == 1)
                {
                    continue;
                }

                if (inactiveDataCustomColumns != null && inactiveDataCustomColumns.Any())
                {
                    var createSql = tableSet.GetCreateTableSql(inactiveDataCustomColumns);
                    await context.ExecuteNonQueryAsync(createSql);
                }
                else
                {
                    var createSql = tableSet.GetCreateTableSql();
                    await context.ExecuteNonQueryAsync(createSql);
                }

                foreach (var indexSql in tableSet.GetAddIndexSql())
                {
                    await context.ExecuteNonQueryAsync(indexSql);
                }
            }
        }

        public static async Task DropFileSystemTablesAsync()
        {
            await using var context = await GetContextAsync();
            var schemaName = GetFileSystemSchemaName();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new(typeof(RMDiscoveryFSContainerInfo), schemaName),
                new(typeof(RMDiscoveryFSConnectionInfo), schemaName),
                new(typeof(RMDiscoveryFSFileExtension), schemaName),
                new(typeof(RMDiscoveryFSAggregateTotalData), schemaName),
                new(typeof(RMDiscoveryFSContainerInactiveData), schemaName),
                new(typeof(RMDiscoveryFSBasicInactiveData), schemaName),
                new(typeof(RMDiscoveryFSContainerRootLevelRotData), schemaName),
                new(typeof(RMDiscoveryFSContainerCategoryLevelRotData), schemaName),
                new(typeof(RMDiscoveryFSContainerRuleLevelRotData), schemaName),
                new(typeof(RMDiscoveryFSBasicRootLevelRotData), schemaName),
                new(typeof(RMDiscoveryFSBasicCategoryLevelRotData), schemaName),
                new(typeof(RMDiscoveryFSBasicRuleLevelRotData), schemaName),
            };
            foreach (var tableSet in tableSets)
            {
                var sql = tableSet.GetDropTableSql();
                await context.ExecuteNonQueryAsync(sql);
            }
        }
    }
}
