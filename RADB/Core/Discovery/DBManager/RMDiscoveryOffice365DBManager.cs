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
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Plan;
using AvePoint.RA.DB.Model.Discovery.Profile;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Core.Discovery.DBManager
{
    public partial class RMDiscoveryDBManager
    {
        public static Task<RMDiscoveryDBEFContext> GetOffice365EFContextAsync(Guid o365TenantId)
        {
            var schemaName = GetOffice365SchemaName(o365TenantId);
            return GetEFContextAsync(schemaName);
        }

        public static string GetOffice365SchemaName(Guid o365TenantId)
        {
            return "s_" + o365TenantId.ToString().ToLower().Replace("-", "");
        }

        public static string GetOffice365SchemaName(Guid o365TenantId, Guid profileId)
        {
            return "s_" + o365TenantId.ToString().ToLower().Replace("-", "") + "_" + profileId.ToString().ToLower().Replace("-", "");
        }

        public static async Task<bool> CheckOffice365TablesExistsAsync()
        {
            var exists = true;

            await using var context = await GetContextAsync();
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new(typeof(RMDiscoveryOffice365RuleInfo), "dbo"),
                new(typeof(RMDiscoveryOffice365SizeRange), "dbo"),
                new(typeof(RMDiscoveryOffice365TenantInfo), "dbo"),
                new(typeof(RMDiscoveryOffice365WithoutInDate), "dbo"),
                new (typeof(RMDiscoveryOffice365TenantInfo), "dbo"),
                new (typeof(RMDiscoveryOffice365AnalysisJob), "dbo"),
                new (typeof(RMDiscoveryOffice365MainJob), "dbo"),
                new (typeof(RMDiscoveryOffice365DiscoveryJob), "dbo"),
                new (typeof(RMDiscoveryOffice365ExecutionInfo), "dbo"),
            };

            foreach (var tableSet in tableSets)
            {
                var existsSql = tableSet.GetExistsTableSql();
                var tableExists = await context.ExecuteScalarAsync<int>(existsSql);
                exists &= (tableExists == 1);
            }

            return exists;
        }

        public static async Task InitOffice365DatabaseAsync()
        {
            var dbName = GetDatabaseName();
            var hasDatabase = await HasDatabaseAsync();
            try
            {
                if(!hasDatabase)
                {
                    var (dbLevel, dbSize) = await CalculateOffice365DBInfoAsync();
                    await InitDatabaseAsync(dbName, dbLevel, dbSize);
                }
                await InitBasicTablesAsync();
                await InitOffice365BasicTablesAsync();
                await InitOffice365BuildInDataListAsync();
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while init office 365 database info. has database [{hasDatabase}]. Error: {e}");
                if(!hasDatabase)
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

        public static async Task InitOffice365BasicTablesAsync()
        {
            await using var context = await GetContextAsync();

            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new(typeof(RMDiscoveryOffice365RuleInfo), "dbo"),
                new(typeof(RMDiscoveryOffice365SizeRange), "dbo"),
                new(typeof(RMDiscoveryOffice365TenantInfo), "dbo"),
                new(typeof(RMDiscoveryOffice365WithoutInDate), "dbo"),
                new (typeof(RMDiscoveryOffice365TenantInfo), "dbo"),
                new (typeof(RMDiscoveryOffice365AnalysisJob), "dbo"),
                new (typeof(RMDiscoveryOffice365MainJob), "dbo"),
                new (typeof(RMDiscoveryOffice365DiscoveryJob), "dbo"),
                new (typeof(RMDiscoveryOffice365ExecutionInfo), "dbo"),
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

        public static async Task InitPlanTablesAsync()
        {

            var dbName = GetDatabaseName();
            var hasDatabase = await HasDatabaseAsync();
            try
            {
                if (!hasDatabase)
                {
                    var (dbLevel, dbSize) = await CalculateOffice365DBInfoAsync();
                    await InitDatabaseAsync(dbName, dbLevel, dbSize);
                }
                await using var context = await GetContextAsync();

                var tableSets = new List<RMDiscoveryDBTableSet>
                    {
                        new(typeof(RMDiscoveryPlanProfile), "dbo"),
                        new(typeof(RMDiscoveryPlanSiteMapping), "dbo"),
                        new(typeof(RMDiscoveryPlanDalJob), "dbo"),
                        new(typeof(RMDiscoveryDalJobConfiguration), "dbo"),
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

        public static async Task InitOffice365BuildInDataListAsync()
        {
            var sizeRangeDao = new RMDiscoveryOffice365SizeRangeDao();
            var withoutDateDao = new RMDiscoveryOffice365WithoutInDateDao();
            await sizeRangeDao.InitBuildInDataAsync();
            await withoutDateDao.InitBuildInDataAsync();
        }

        public static async Task InitOffice365BasicTablesAsync(Guid o365TenantId)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetOffice365SchemaName(o365TenantId);
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365ContainerInfo), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365SiteInfo), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365FileExtension), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365AggregateTotalData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365TenantConfiguration), schemaName),
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

        public static async Task InitOffice365BasicTablesV3Async(Guid o365TenantId)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetOffice365SchemaName(o365TenantId);
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365ContainerInfo), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365SiteInfo), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365FileExtension), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365AggregateTotalData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365TenantConfiguration), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryProfileFailedInfo), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365ProfileInfo), schemaName),
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

            await InitOffice365BuildInDataListAsync(o365TenantId);
        }

        public static async Task InitOffice365DataOptimizationTablesAsync(Guid o365TenantId)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetOffice365SchemaName(o365TenantId);
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365OptimizationSettingsInfo), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365SiteOptimizationMappingInfo), schemaName),
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

        public static async Task<bool> CheckOffice365OptimizationSettingsTableExistsAsync(Guid o365TenantId)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetOffice365SchemaName(o365TenantId);
            var tableSet = new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365OptimizationSettingsInfo), schemaName);
            var existsSql = tableSet.GetExistsTableSql();
            var exists = await context.ExecuteScalarAsync<int>(existsSql);
            return exists == 1;
        }

        public static async Task InitOffice365ProgressReportTablesAsync(Guid o365TenantId)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetOffice365SchemaName(o365TenantId);
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365ContainerOptimizedInfo), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365SiteOptimizedInfo), schemaName),
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

        public static async Task InitOffice365RotTablesAsync(Guid o365TenantId)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetOffice365SchemaName(o365TenantId);
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365BasicRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365ContainerRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365SiteRotData), schemaName),
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

        public static async Task InitOffice365RotTablesV3Async(Guid o365TenantId)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetOffice365SchemaName(o365TenantId);
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365BasicRuleLevelRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365BasicCategoryLevelRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365BasicRootLevelRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365ContainerRuleLevelRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365ContainerCategoryLevelRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365ContainerRootLevelRotData), schemaName),
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

        public static async Task InitOffice365InactiveTablesAsync(Guid o365TenantId, List<RMDiscoveryCustomColumn> inactiveDataCustomColumns)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetOffice365SchemaName(o365TenantId);
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365BasicInactiveData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365ContainerInactiveData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365SiteInactiveData), schemaName),
            };

            foreach (var tableSet in tableSets)
            {
                var existsSql = tableSet.GetExistsTableSql();
                var exists = await context.ExecuteScalarAsync<int>(existsSql);
                if (exists == 1)
                {
                    continue;
                }

                var createSql = tableSet.GetCreateTableSql(inactiveDataCustomColumns);
                await context.ExecuteNonQueryAsync(createSql);

                foreach (var indexSql in tableSet.GetAddIndexSql())
                {
                    await context.ExecuteNonQueryAsync(indexSql);
                }
            }
        }

        public static async Task InitOffice365InactiveTablesV3Async(Guid o365TenantId, List<RMDiscoveryCustomColumn> inactiveDataCustomColumns)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetOffice365SchemaName(o365TenantId);
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365BasicInactiveData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryOffice365ContainerInactiveData), schemaName),
            };

            foreach (var tableSet in tableSets)
            {
                var existsSql = tableSet.GetExistsTableSql();
                var exists = await context.ExecuteScalarAsync<int>(existsSql);
                if (exists == 1)
                {
                    continue;
                }

                var createSql = tableSet.GetCreateTableSql(inactiveDataCustomColumns);
                await context.ExecuteNonQueryAsync(createSql);

                foreach (var indexSql in tableSet.GetAddIndexSql())
                {
                    await context.ExecuteNonQueryAsync(indexSql);
                }
            }
        }

        public static async Task InitOffice365BuildInDataListAsync(Guid o365TenantId)
        {
            var profileDao = new RMDiscoveryOffice365ProfileDao();
            await profileDao.InitBuildInDataAsync(o365TenantId);
        }

        public static async Task InitOffice365InactiveProfileTabls(Guid o365TenantId, Guid profileId, List<RMDiscoveryCustomColumn> inactiveDataCustomColumns)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetOffice365SchemaName(o365TenantId, profileId);
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryProfileBasicInactiveData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryProfileContainerInactiveData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryProfileSiteInactiveData), schemaName),
            };

            foreach (var tableSet in tableSets)
            {
                var existsSql = tableSet.GetExistsTableSql();
                var exists = await context.ExecuteScalarAsync<int>(existsSql);
                if (exists == 1)
                {
                    continue;
                }

                var createSql = tableSet.GetCreateTableSql(inactiveDataCustomColumns);
                await context.ExecuteNonQueryAsync(createSql);

                foreach (var indexSql in tableSet.GetAddIndexSql())
                {
                    await context.ExecuteNonQueryAsync(indexSql);
                }
            }
        }

        public static async Task InitOffice365RotProfileTabls(Guid o365TenantId, Guid profileId, List<RMDiscoveryCustomColumn> rotDataCustomColumns)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetOffice365SchemaName(o365TenantId, profileId);
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryProfileBasicRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryProfileContainerRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryProfileSiteRotData), schemaName),
            };

            foreach (var tableSet in tableSets)
            {
                var existsSql = tableSet.GetExistsTableSql();
                var exists = await context.ExecuteScalarAsync<int>(existsSql);
                if (exists == 1)
                {
                    continue;
                }

                var createSql = tableSet.GetCreateTableSql(rotDataCustomColumns);
                await context.ExecuteNonQueryAsync(createSql);

                foreach (var indexSql in tableSet.GetAddIndexSql())
                {
                    await context.ExecuteNonQueryAsync(indexSql);
                }
            }
        }

        public static async Task DropOffice365TablesAsync(Guid o365TenantId)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var queryProfileTableSql = "SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @SchemaName AND TABLE_NAME = @TableName";
            var exists = await context.ExecuteScalarAsync<int>(queryProfileTableSql, new SqlParameter("@SchemaName", schemaName), new SqlParameter("@TableName", "RMProfileInfoes"));

            if (exists == 1)
            {
                var queryProfileSql = $"SELECT Id FROM [{schemaName}].RMProfileInfoes";
                var dataCollection = await context.ExecuteQueryAsync(queryProfileSql);
                var profileIds = dataCollection.ToList<Guid>();
                foreach (var profileId in profileIds)
                {
                    var profileSchema = GetOffice365SchemaName(o365TenantId, profileId);
                    var profileTableSets = new List<RMDiscoveryDBTableSet>
                {
                    new (typeof(RMDiscoveryProfileBasicInactiveData), profileSchema),
                    new (typeof(RMDiscoveryProfileContainerInactiveData), profileSchema),
                    new (typeof(RMDiscoveryProfileSiteInactiveData), profileSchema),
                    new (typeof(RMDiscoveryProfileBasicRotData), profileSchema),
                    new (typeof(RMDiscoveryProfileContainerRotData), profileSchema),
                    new (typeof(RMDiscoveryProfileSiteRotData), profileSchema),
                };
                    foreach (var profileTableSet in profileTableSets)
                    {
                        var sql = profileTableSet.GetDropTableSql();
                        await context.ExecuteNonQueryAsync(sql);
                    }
                }
            }

            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new (typeof(RMDiscoveryOffice365ContainerInfo), schemaName),
                new (typeof(RMDiscoveryOffice365SiteInfo), schemaName),
                new (typeof(RMDiscoveryOffice365FileExtension), schemaName),
                new (typeof(RMDiscoveryOffice365AggregateTotalData), schemaName),
                new (typeof(RMDiscoveryOffice365BasicRotData), schemaName),
                new (typeof(RMDiscoveryOffice365ContainerRotData), schemaName),
                new (typeof(RMDiscoveryOffice365SiteRotData), schemaName),
                new (typeof(RMDiscoveryOffice365BasicInactiveData), schemaName),
                new (typeof(RMDiscoveryOffice365ContainerInactiveData), schemaName),
                new (typeof(RMDiscoveryOffice365SiteInactiveData), schemaName),
                new (typeof(RMDiscoveryOffice365OptimizationSettingsInfo), schemaName),
                new (typeof(RMDiscoveryOffice365SiteOptimizationMappingInfo), schemaName),
                new (typeof(RMDiscoveryOffice365ContainerOptimizedInfo), schemaName),
                new (typeof(RMDiscoveryOffice365SiteOptimizedInfo), schemaName),
                new (typeof(RMDiscoveryOffice365ContainerOptimizedInfo), schemaName),
                new (typeof(RMDiscoveryOffice365TenantConfiguration), schemaName),
                new (typeof(RMDiscoveryOffice365ProfileInfo), schemaName),
                new (typeof(RMDiscoveryProfileFailedInfo), schemaName),
                new (typeof(RMDiscoveryOffice365BasicRuleLevelRotData), schemaName),
                new (typeof(RMDiscoveryOffice365BasicCategoryLevelRotData), schemaName),
                new (typeof(RMDiscoveryOffice365BasicRootLevelRotData), schemaName),
                new (typeof(RMDiscoveryOffice365ContainerRuleLevelRotData), schemaName),
                new (typeof(RMDiscoveryOffice365ContainerCategoryLevelRotData), schemaName),
                new (typeof(RMDiscoveryOffice365ContainerRootLevelRotData), schemaName),
            };
            foreach (var tableSet in tableSets)
            {
                var sql = tableSet.GetDropTableSql();
                await context.ExecuteNonQueryAsync(sql);
            }
        }

        public static async Task DropOffice365InactiveProfileTablsAsync(Guid o365TenantId, Guid profileId)
        {
            await using var context = await GetContextAsync();
            var profileSchema = GetOffice365SchemaName(o365TenantId, profileId);
            var profileTableSets = new List<RMDiscoveryDBTableSet>
                {
                    new (typeof(RMDiscoveryProfileBasicInactiveData), profileSchema),
                    new (typeof(RMDiscoveryProfileContainerInactiveData), profileSchema),
                    new (typeof(RMDiscoveryProfileSiteInactiveData), profileSchema),
                };
            foreach (var profileTableSet in profileTableSets)
            {
                var sql = profileTableSet.GetDropTableSql();
                await context.ExecuteNonQueryAsync(sql);
            }
        }

        public static async Task DropOffice365RotProfileTablsAsync(Guid o365TenantId, Guid profileId)
        {
            await using var context = await GetContextAsync();
            var profileSchema = GetOffice365SchemaName(o365TenantId, profileId);
            var profileTableSets = new List<RMDiscoveryDBTableSet>
                {
                    new (typeof(RMDiscoveryProfileBasicRotData), profileSchema),
                    new (typeof(RMDiscoveryProfileContainerRotData), profileSchema),
                    new (typeof(RMDiscoveryProfileSiteRotData), profileSchema),
                };
            foreach (var profileTableSet in profileTableSets)
            {
                var sql = profileTableSet.GetDropTableSql();
                await context.ExecuteNonQueryAsync(sql);
            }
        }

        private static async Task<(RMAzureDBPerformanceLevel performanceLevel, int dbSize)> CalculateOffice365DBInfoAsync()
        {
            var totalStorageSize = await CalculateOffice365TenantStorageUsageSize();
            if (totalStorageSize <= 10)
            {
                return (RMAzureDBPerformanceLevel.BASIC, 2);
            }
            return (RMAzureDBPerformanceLevel.S0, 250);
        }

        private static async Task<long> CalculateOffice365TenantStorageUsageSize()
        {
            const string customStorageUsageSizeKey = "CUSTOM_O365_TENANT_STROAGE_USAGE_SIZE";

            var totalSize = 0L;

            var keyValueDao = new RMKeyValueDao();
            var setting = keyValueDao.GetValueByKey(customStorageUsageSizeKey);
            if (setting != null && !string.IsNullOrWhiteSpace(setting.Value) && long.TryParse(setting.Value, out var size))
            {
                totalSize = size;
            }
            else
            {
                var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
                var o365TenantUsageStorages = await client.Office365TenantService.GetSharePointSiteUsageStorageAsync();
                foreach (var o365TenantUsageStorage in o365TenantUsageStorages)
                {
                    var usageReport = o365TenantUsageStorage.UsageStorage;
                    if (string.IsNullOrWhiteSpace(usageReport))
                    {
                        continue;
                    }

                    var reports = usageReport.Split("\r\n");
                    if (reports.Length < 2)
                    {
                        continue;
                    }

                    var latestReport = reports[1];
                    var reportInfo = latestReport.Split(",");
                    if (reportInfo.Length < 3)
                    {
                        continue;
                    }

                    if (!long.TryParse(reportInfo[2], out var usageBytes))
                    {
                        continue;
                    }

                    totalSize += usageBytes;
                }
            }

            var totalSizeTB = totalSize / 1024 / 1024 / 1024 / 1024;
            _logger.Info($"The customer [{TenantLocalValue.LogonGroupId}] total size [{totalSizeTB}] TB.");

            return totalSizeTB;
        }
        

    }
}
