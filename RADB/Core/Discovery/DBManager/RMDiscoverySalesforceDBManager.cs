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
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Salesforce;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Salesforce;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Core.Discovery.DBManager
{
    public partial class RMDiscoveryDBManager
    {
        public static Task<RMDiscoveryDBEFContext> GetSalesforceEFContextAsync(string organizationId)
        {
            var schemaName = GetSalesforceSchemaName(organizationId);
            return GetEFContextAsync(schemaName);
        }

        public static string GetSalesforceSchemaName(string organizationId)
        {
            return "s_salesforce" + organizationId.ToLower().Replace("-", "");
        }

        public static async Task<bool> CheckSalesforceTablesExistsAsync()
        {
            var exists = true;

            await using var context = await GetContextAsync();
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new(typeof(RMDiscoverySalesforceSizeRange), "dbo"),
                new(typeof(RMDiscoverySalesforceWithoutInDate), "dbo"),
                new(typeof(RMDiscoverySalesforceCreatedDateRange), "dbo"),
                new(typeof(RMDiscoverySalesforceMainJob), "dbo"),
                new (typeof(RMDiscoverySalesforceExecutionInfo), "dbo"),
            };

            foreach (var tableSet in tableSets)
            {
                var existsSql = tableSet.GetExistsTableSql();
                var tableExists = await context.ExecuteScalarAsync<int>(existsSql);
                exists &= (tableExists == 1);
            }

            return exists;
        }

        public static async Task InitSalesforceDatabaseAsync()
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
                await InitSalesforceBasicTablesAsync();
                await InitSalesforceBuildInDataListAsync();
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while init office 365 database info. has database [{hasDatabase}]. Error: {e}");
                if (!hasDatabase)
                {
                    using var context = RMDBContextManager.GetSystemDBContext();
                    var info = await context.TenantDiscoveryDBInfoes.FirstOrDefaultAsync(item => item.Id == TenantId);
                    context.TenantDiscoveryDBInfoes.Remove(info);
                    await context.SaveChangesAsync();
                    await DropDatabaseAsync(dbName);
                }
                throw;
            }
        }

        public static async Task InitSalesforceBasicTablesAsync()
        {
            await using var context = await GetContextAsync();

            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new(typeof(RMDiscoverySalesforceSizeRange), "dbo"),
                new(typeof(RMDiscoverySalesforceWithoutInDate), "dbo"),
                new(typeof(RMDiscoverySalesforceCreatedDateRange), "dbo"),
                new(typeof(RMDiscoverySalesforceMainJob), "dbo"),
                new (typeof(RMDiscoverySalesforceExecutionInfo), "dbo"),
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

        public static async Task InitSalesforceBuildInDataListAsync()
        {
            var sizeRangeDao = new RMDiscoverySalesforceSizeRangeDao();
            var withoutDateDao = new RMDiscoverySalesforceWithoutInDateDao();
            await sizeRangeDao.InitBuildInDataAsync();
            await withoutDateDao.InitBuildInDataAsync();
        }

        public static async Task InitSalesforceBasicTablesAsync(string organizationId)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetSalesforceSchemaName(organizationId);
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new (typeof(RMDiscoverySalesforceObjectInfo), schemaName),
                new (typeof(RMDiscoverySalesforceAggregateTotalData), schemaName),
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

        public static async Task InitSalesforceInactiveTablesAsync(string safesForceTenantId)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetSalesforceSchemaName(safesForceTenantId);
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new(typeof(RMDiscoverySalesforceRecordInactiveData), schemaName),
                new(typeof(RMDiscoverySalesforceFileInactiveData), schemaName),
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

        public static async Task DropSalesforceTablesAsync(string organizationId)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetSalesforceSchemaName(organizationId);

            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new (typeof(RMDiscoverySalesforceAggregateTotalData), schemaName),
                new (typeof(RMDiscoverySalesforceFileInactiveData), schemaName),
                new (typeof(RMDiscoverySalesforceObjectInfo), schemaName),
                new (typeof(RMDiscoverySalesforceRecordInactiveData), schemaName),
            };
            foreach (var tableSet in tableSets)
            {
                var sql = tableSet.GetDropTableSql();
                await context.ExecuteNonQueryAsync(sql);
            }
        }
    }
}
