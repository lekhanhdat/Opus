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
using AvePoint.RA.Common.AzureService;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.Upgrader;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Profile;
using AvePoint.RA.DB.Model.Discovery.Salesforce;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;


namespace AvePoint.RA.DB.Core.Discovery.DBManager
{
    public partial class RMDiscoveryDBManager
    {
        private static readonly RALogger _logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static string TenantId => TenantLocalValue.LogonGroupId;

        public static async Task<RMDiscoveryDBContext> GetContextAsync()
        {
            var connectionStr = await DatabaseUtility.GetDiscoveryDBConnectionStringAsync();
            var connection = AzureUtil.GetConnectionUseIdentityToken(connectionStr);
            var res = new RMDiscoveryDBContext(connection);
            return res;
        }

        public static Task<RMDiscoveryDBEFContext> GetEFContextAsync()
        {
            return GetEFContextAsync("dbo");
        }

        public static async Task<RMDiscoveryDBEFContext> GetEFContextAsync(string schemaName)
        {
            var connectionStr = await DatabaseUtility.GetDiscoveryDBConnectionStringAsync();
            var connection = AzureUtil.GetConnectionUseIdentityToken(connectionStr);
            var context = new RMDiscoveryDBEFContext(schemaName, connection);
            context.Database.CommandTimeout = 60 * 10;
            return context;
        }

        public static string GetDatabaseName() => $"reco_discovery_{TenantId.Replace("-", "")}_{DateTime.UtcNow:yyyyMMdd}";

        public static async Task<bool> HasDatabaseAsync()
        {
            using var context = RMDBContextManager.GetSystemDBContext();
            var dbInfo = await context.TenantDiscoveryDBInfoes.FirstOrDefaultAsync(item => item.Id == TenantId);
            return dbInfo != null;
        }

        public static async Task DeleteDataBaseAsync(string databaseName)
        {
            
            try
            {
                FailoverGroupService.DeleteDatabaseFromServerAndFog(databaseName);
                //await DropDatabaseAsync(databaseName);
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while delete database. Error: {e}");
                throw;
            }
        }

        public static async Task<bool> CheckDataBaseHaveOnlyAOSPDiscovery()
        {
            bool result = true;
            try
            {
                result &= !(await CheckFileSystemTablesExistsAsync());
                result &= !(await CheckGoogleTablesExistsAsync());
                result &= !(await CheckSalesforceTablesExistsAsync());
                result &= !(await CheckOffice365TablesExistsAsync());
            }
            catch(Exception ex)
            {
                _logger.Error($"An error occurred while CheckDataBaseHaveOnlyAOSPDiscovery. Error: {ex}");
                throw;
            }
            return result;
        }

        public static async Task InitDatabaseAsync(string databaseName, RMAzureDBPerformanceLevel dbLevel, int dbSize)
        {
            using var context = RMDBContextManager.GetSystemDBContext();
            var dbInfo = await context.TenantDiscoveryDBInfoes.FirstOrDefaultAsync(item => item.Id == TenantId);
            if (dbInfo != null)
            {
                return;
            }
            await CreateDatabaseAsync(databaseName, dbLevel == RMAzureDBPerformanceLevel.BASIC ? "BASIC" : "Standard", dbLevel.ToString(), dbSize);
            await AddDatabaseRole(databaseName);
            var isUseFog = FailoverGroupService.AddDatabasesToFailoverGroup(databaseName);

            context.TenantDiscoveryDBInfoes.Add(new()
            {
                Id = TenantId,
                DatabaseName = databaseName,
                PerformanceLevel = dbLevel,
                CreateTime = DateTime.UtcNow.Ticks,
                IsEnabled = true,
                IsRemoved = false,
                UseFailoverGroup = isUseFog,
            });

            await context.SaveChangesAsync();
        }
        
        public static async Task InitBasicTablesAsync()
        {
            await using var context = await GetContextAsync();

            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new (typeof(RMDiscoveryConfiguration), "dbo"),
                new (typeof(RMDiscoveryUpgradeInfo), "dbo")
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

        private static async Task DropDatabaseAsync(string databaseName)
        {
            try
            {
                SecurityUtils.SanitizeSQLSchemaName(databaseName);
                var dbServer = GetDatabaseServer();
                using var azureConnection = AzureUtil.GetConnection(dbServer, "master");
                var dropCommand = azureConnection.CreateCommand();
                dropCommand.CommandTimeout = DatabaseUtility.DefaultCommandTimeout;
                dropCommand.CommandText = $"DROP DATABASE {databaseName}";
                await dropCommand.ExecuteNonQueryAsync();
                await Task.Delay(5000);
            }
            catch
            {
                throw;
            }
        }

        private static async Task CreateDatabaseAsync(string databaseName, string edition, string dbLevel, int dbSize)
        {
            var dbServer = GetDatabaseServer();
            using var azureConnection = AzureUtil.GetConnection(dbServer, "master");
            var command = azureConnection.CreateCommand();
            command.CommandTimeout = DatabaseUtility.DefaultCommandTimeout;
            command.CommandText = @"SELECT CAST(CASE 
                                                WHEN SERVERPROPERTY('EditionId')=1674378470
                                                    THEN 1
                                                ELSE 0
                                             END AS BIT
                                            )";
            var isAzureDB = await command.ExecuteScalarAsync();
            databaseName = SecurityUtils.SanitizeSQLSchemaName(databaseName);
            command.CommandText = $"CREATE DATABASE {databaseName} CONTAINMENT = PARTIAL COLLATE Latin1_General_CI_AS_KS_WS";
            if ((bool)isAzureDB)
            {
                command.CommandText = $"CREATE DATABASE {databaseName} COLLATE Latin1_General_CI_AS_KS_WS (edition='{edition}',SERVICE_OBJECTIVE ='{dbLevel}', MAXSIZE={dbSize}GB)";
            }
            await command.ExecuteNonQueryAsync();
            await Task.Delay(5000);
        }

        private static async Task AddDatabaseRole(string databaseName)
        {
            var dbServer = GetDatabaseServer();
            using var connection = AzureUtil.GetConnection(dbServer, databaseName);
            var command = connection.CreateCommand();
            command.CommandTimeout = DatabaseUtility.DefaultCommandTimeout;
            command.CommandText = "CREATE ROLE tenantuser";
            await command.ExecuteNonQueryAsync();

            command.CommandText = "GRANT VIEW DATABASE STATE, CREATE TABLE TO tenantuser";
            await command.ExecuteNonQueryAsync();

            if (RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
            {
                return;
            }

            var fullConnStr = RMGlobalConfiguration.DBConfig[RMDatabaseSettingKey.RECO_CONTROL_SQL_CONNECTION_STRING_FULL];
            var sqlBuilder = new SqlConnectionStringBuilder(fullConnStr);
            if (string.IsNullOrWhiteSpace(sqlBuilder.UserID) || string.IsNullOrWhiteSpace(sqlBuilder.Password))
            {
                return;
            }
            command.CommandText = $"CREATE USER {sqlBuilder.UserID} WITH PASSWORD='{sqlBuilder.Password}';";
            await command.ExecuteNonQueryAsync();

            command.CommandText = $"ALTER ROLE db_owner ADD member {sqlBuilder.UserID};";
            await command.ExecuteNonQueryAsync();
        }

        private static string GetDatabaseServer()
        {
            var dbServer = FailoverGroupService.GetPrimaryDBServerName();
            var configedDBServer = RMGlobalConfiguration.DBConfig.ConfigDatabaseInstance;
            if (string.IsNullOrWhiteSpace(dbServer))
            {
                return configedDBServer;
            }
            var domain = configedDBServer.Split(".");
            domain[0] = dbServer;
            return string.Join(".", domain);
        }

        private static async Task CreateSchema(string schemaName)
        {
            await using var context = await GetContextAsync();
            var existsSql = $"SELECT COUNT(1) FROM sys.schemas WHERE name = '{SecurityUtils.SanitizeSQLSchemaName(schemaName)}'";
            var exists = await context.ExecuteScalarAsync<int>(existsSql);
            if (exists == 1)
            {
                return;
            }

            var createSql = $"CREATE SCHEMA [{SecurityUtils.SanitizeSQLSchemaName(schemaName)}] AUTHORIZATION tenantuser";
            await context.ExecuteNonQueryAsync(createSql);
        }
    }
}
