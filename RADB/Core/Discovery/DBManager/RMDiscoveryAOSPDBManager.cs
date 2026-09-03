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
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.AOSP;
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
        public static Task<RMDiscoveryDBEFContext> GetAOSPEFContextAsync(Guid o365TenantId)
        {
            var schemaName = GetAOSPSchemaName(o365TenantId);
            return GetEFContextAsync(schemaName);
        }

        public static string GetAOSPSchemaName(Guid o365TenantId)
        {
            return "s_" + "aosp_" + o365TenantId.ToString().ToLower().Replace("-", "");
        }

        public static string GetAOSPSchemaName(Guid o365TenantId, Guid profileId)
        {
            return "s_" + "aosp_" + o365TenantId.ToString().ToLower().Replace("-", "") + "_" + profileId.ToString().ToLower().Replace("-", "");
        }

        public static async Task InitAOSPDatabaseAsync(string o365TenantId)
        {
            var dbName = GetDatabaseName();
            var hasDatabase = await HasDatabaseAsync();
            try
            {
                if (!hasDatabase)
                {
                    await InitDatabaseAsync(dbName, RMAzureDBPerformanceLevel.BASIC, 2);
                }
                //await InitBasicTablesAsync();
                await InitAOSPBasicTablesAsync();
                await InitAOSPBuildInDataListAsync(o365TenantId);
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

        public static async Task InitAOSPBasicTablesAsync()
        {
            await using var context = await GetContextAsync();

            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new (typeof(RMDiscoveryAOSPConfiguration), "dbo"),
                new (typeof(RMDiscoveryUpgradeInfo), "dbo"),

                new(typeof(RMDiscoveryAOSPRuleInfo), "dbo"),
                new(typeof(RMDiscoveryAOSPSizeRange), "dbo"),
                new(typeof(RMDiscoveryAOSPTenantInfo), "dbo"),
                new(typeof(RMDiscoveryAOSPWithoutInDate), "dbo"),
                //new (typeof(RMDiscoveryAOSPTenantInfo), "dbo"),
                new (typeof(RMDiscoveryAOSPAnalysisJob), "dbo"),
                new (typeof(RMDiscoveryAOSPMainJob), "dbo"),
                new (typeof(RMDiscoveryAOSPDiscoveryJob), "dbo"),
                //new (typeof(RMDiscoveryAOSPExecutionInfo), "dbo"),
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

        public static async Task InitAOSPBasicTablesAsync(Guid o365TenantId)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetAOSPSchemaName(o365TenantId);
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryAOSPContainerInfo), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryAOSPSiteInfo), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryAOSPFileExtension), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryAOSPAggregateTotalData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryAOSPTenantConfiguration), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryProfileFailedInfo), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryAOSPProfileInfo), schemaName),
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

            //await InitAOSPBuildInDataListAsync(o365TenantId);
        }

        public static async Task InitAOSPRotTablesAsync(Guid o365TenantId)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetAOSPSchemaName(o365TenantId);
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryAOSPBasicRuleLevelRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryAOSPBasicCategoryLevelRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryAOSPBasicRootLevelRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryAOSPContainerRuleLevelRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryAOSPContainerCategoryLevelRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryAOSPContainerRootLevelRotData), schemaName),
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

        public static async Task InitAOSPInactiveTablesAsync(Guid o365TenantId, List<RMDiscoveryCustomColumn> inactiveDataCustomColumns)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetAOSPSchemaName(o365TenantId);
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryAOSPBasicInactiveData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryAOSPContainerInactiveData), schemaName),
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

        public static async Task InitAOSPDataOptimizationTablesAsync(Guid o365TenantId)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetAOSPSchemaName(o365TenantId);
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryAOSPOptimizationSettingsInfo), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryAOSPSiteOptimizationMappingInfo), schemaName),
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



        public static async Task<bool> CheckAOSPOptimizationSettingsTableExistsAsync(Guid o365TenantId)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetAOSPSchemaName(o365TenantId);
            var tableSet = new RMDiscoveryDBTableSet(typeof(RMDiscoveryAOSPOptimizationSettingsInfo), schemaName);
            var existsSql = tableSet.GetExistsTableSql();
            var exists = await context.ExecuteScalarAsync<int>(existsSql);
            return exists == 1;
        }

        public static async Task<bool> CheckAOSPTenantInfoTableExistsAsync()
        {
            await using var context = await GetContextAsync();
            var tableSet = new RMDiscoveryDBTableSet(typeof(RMDiscoveryAOSPTenantInfo), "dbo");
            var existsSql = tableSet.GetExistsTableSql();
            var exists = await context.ExecuteScalarAsync<int>(existsSql);
            return exists == 1;
        }

        public static async Task InitAOSPProgressReportTablesAsync(Guid o365TenantId)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetAOSPSchemaName(o365TenantId);
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryAOSPContainerOptimizedInfo), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryAOSPSiteOptimizedInfo), schemaName),
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

        public static async Task InitAOSPBuildInDataListAsync(string o365TenantId)
        {
            var sizeRangeDao = new RMDiscoveryAOSPSizeRangeDao();
            var withoutDateDao = new RMDiscoveryAOSPWithoutInDateDao();
            await sizeRangeDao.InitBuildInDataAsync(o365TenantId);
            await withoutDateDao.InitBuildInDataAsync(o365TenantId);
        }

        //public static async Task InitAOSPBuildInDataListAsync(Guid o365TenantId)
        //{
        //    var profileDao = new RMDiscoveryAOSPProfileDao();
        //    await profileDao.InitBuildInDataAsync(o365TenantId);
        //}

        public static async Task<bool> CheckAOSPTablesExistsAsync()
        {
            var exists = true;

            await using var context = await GetContextAsync();
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new(typeof(RMDiscoveryAOSPRuleInfo), "dbo"),
                new(typeof(RMDiscoveryAOSPSizeRange), "dbo"),
                new(typeof(RMDiscoveryAOSPTenantInfo), "dbo"),
                new(typeof(RMDiscoveryAOSPWithoutInDate), "dbo"),
                new (typeof(RMDiscoveryAOSPTenantInfo), "dbo"),
                new (typeof(RMDiscoveryAOSPAnalysisJob), "dbo"),
                new (typeof(RMDiscoveryAOSPMainJob), "dbo"),
                new (typeof(RMDiscoveryAOSPDiscoveryJob), "dbo"),
                //new (typeof(RMDiscoveryAOSPExecutionInfo), "dbo"),
            };

            foreach (var tableSet in tableSets)
            {
                var existsSql = tableSet.GetExistsTableSql();
                var tableExists = await context.ExecuteScalarAsync<int>(existsSql);
                exists &= (tableExists == 1);
            }

            return exists;
        }

        public static async Task InitAOSPInactiveProfileTabls(Guid o365TenantId, Guid profileId, List<RMDiscoveryCustomColumn> inactiveDataCustomColumns)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetAOSPSchemaName(o365TenantId, profileId);
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

        public static async Task InitAOSPRotProfileTabls(Guid o365TenantId, Guid profileId, List<RMDiscoveryCustomColumn> rotDataCustomColumns)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetAOSPSchemaName(o365TenantId, profileId);
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


        public static async Task DropAOSPBasicTablesAsync()
        {
            await using var context = await GetContextAsync();
            
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new (typeof(RMDiscoveryAOSPConfiguration), "dbo"),
                new (typeof(RMDiscoveryUpgradeInfo), "dbo"),

                new(typeof(RMDiscoveryAOSPRuleInfo), "dbo"),
                new(typeof(RMDiscoveryAOSPSizeRange), "dbo"),
                new(typeof(RMDiscoveryAOSPTenantInfo), "dbo"),
                new(typeof(RMDiscoveryAOSPWithoutInDate), "dbo"),
                new (typeof(RMDiscoveryAOSPAnalysisJob), "dbo"),
                new (typeof(RMDiscoveryAOSPMainJob), "dbo"),
                new (typeof(RMDiscoveryAOSPDiscoveryJob), "dbo"),
            };

            foreach (var tableSet in tableSets)
            {
                var sql = tableSet.GetDropTableSql();
                await context.ExecuteNonQueryAsync(sql);
            }

        }

        public static async Task DropAOSPTablesAsync(Guid o365TenantId)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetAOSPSchemaName(o365TenantId);
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
                    var profileSchema = GetAOSPSchemaName(o365TenantId, profileId);
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
                new (typeof(RMDiscoveryAOSPContainerInfo), schemaName),
                new (typeof(RMDiscoveryAOSPSiteInfo), schemaName),
                new (typeof(RMDiscoveryAOSPFileExtension), schemaName),
                new (typeof(RMDiscoveryAOSPAggregateTotalData), schemaName),
                new (typeof(RMDiscoveryAOSPBasicRotData), schemaName),
                new (typeof(RMDiscoveryAOSPContainerRotData), schemaName),
                new (typeof(RMDiscoveryAOSPSiteRotData), schemaName),
                new (typeof(RMDiscoveryAOSPBasicInactiveData), schemaName),
                new (typeof(RMDiscoveryAOSPContainerInactiveData), schemaName),
                new (typeof(RMDiscoveryAOSPSiteInactiveData), schemaName),
                new (typeof(RMDiscoveryAOSPOptimizationSettingsInfo), schemaName),
                new (typeof(RMDiscoveryAOSPSiteOptimizationMappingInfo), schemaName),
                new (typeof(RMDiscoveryAOSPContainerOptimizedInfo), schemaName),
                new (typeof(RMDiscoveryAOSPSiteOptimizedInfo), schemaName),
                new (typeof(RMDiscoveryAOSPContainerOptimizedInfo), schemaName),
                new (typeof(RMDiscoveryAOSPTenantConfiguration), schemaName),
                new (typeof(RMDiscoveryAOSPProfileInfo), schemaName),
                new (typeof(RMDiscoveryProfileFailedInfo), schemaName),
                new (typeof(RMDiscoveryAOSPBasicRuleLevelRotData), schemaName),
                new (typeof(RMDiscoveryAOSPBasicCategoryLevelRotData), schemaName),
                new (typeof(RMDiscoveryAOSPBasicRootLevelRotData), schemaName),
                new (typeof(RMDiscoveryAOSPContainerRuleLevelRotData), schemaName),
                new (typeof(RMDiscoveryAOSPContainerCategoryLevelRotData), schemaName),
                new (typeof(RMDiscoveryAOSPContainerRootLevelRotData), schemaName),
            };
            foreach (var tableSet in tableSets)
            {
                var sql = tableSet.GetDropTableSql();
                await context.ExecuteNonQueryAsync(sql);
            }
        }

        public static async Task DropAOSPInactiveProfileTablsAsync(Guid o365TenantId, Guid profileId)
        {
            await using var context = await GetContextAsync();
            var profileSchema = GetAOSPSchemaName(o365TenantId, profileId);
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

        public static async Task DropAOSPRotProfileTablsAsync(Guid o365TenantId, Guid profileId)
        {
            await using var context = await GetContextAsync();
            var profileSchema = GetAOSPSchemaName(o365TenantId, profileId);
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

        public static async Task<string> DeleteAOSPDatabaseAsync()
        {
            try
            {
                using var context = RMDBContextManager.GetSystemDBContext();
                var dbInfo = await context.TenantDiscoveryDBInfoes.FirstOrDefaultAsync(item => item.Id == TenantId);
                if (dbInfo == null)
                {
                    _logger.Error($"Current tenant don't have discovery db info");
                    return "Current tenant don't have discovery db info";
                }
                if(await CheckDataBaseHaveOnlyAOSPDiscovery())
                {
                    await DeleteDataBaseAsync(dbInfo.DatabaseName);
                    context.TenantDiscoveryDBInfoes.Remove(dbInfo);
                    await context.SaveChangesAsync();
                }
                else
                {
                    IRMDiscoveryAOSPTenantDao aospTenantDao = new RMDiscoveryAOSPTenantDao();
                    var discoveredTenants = await aospTenantDao.GetAllAsync();
                    var discoveredTenantIds = discoveredTenants.Select(item => item.UniqueId).ToHashSet();
                    foreach(var discoveredTenantID in discoveredTenantIds)
                    {
                        await DropAOSPTablesAsync(discoveredTenantID);
                    }
                    await DropAOSPBasicTablesAsync();
                }
                return "Delete AOSP DataBase success";

            }
            catch (Exception ex) 
            {
                _logger.Error($"Error occurred while DeleteAOSPDatabaseAsync {ex}");
                return ex.Message;
            }
        }
    }
}
