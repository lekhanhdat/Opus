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
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Profile;

namespace AvePoint.RA.DB.Core.Discovery.DBManager
{
    public partial class RMDiscoveryDBManager
    {
        public static Task<RMDiscoveryDBEFContext> GetGoogleEFContextAsync(string googleOrganizationId)
        {
            var schemaName = GetGoogleSchemaName(googleOrganizationId);
            return GetEFContextAsync(schemaName);
        }

        public static string GetGoogleSchemaName(string googleOrganizationId)
        {
            string prefix = "s_google_";
            if (string.IsNullOrEmpty(googleOrganizationId)) return string.Empty;
            if (googleOrganizationId.Contains("-"))
            {
                return prefix + googleOrganizationId.ToLower().Replace("-", "");
            }
            return prefix + googleOrganizationId.ToLower();
        }
        
        public static string GetGoogleSchemaName(string googleOrganizationId, Guid profileId)
        {
            string prefix = "s_google_";
            if (string.IsNullOrEmpty(googleOrganizationId)) return string.Empty;
            if (googleOrganizationId.Contains("-"))
            {
                return prefix + googleOrganizationId.ToLower().Replace("-", "");
            }
            return prefix + googleOrganizationId.ToLower() + "_" + profileId.ToString().ToLower().Replace("-", "");
        }

        public static async Task<bool> CheckGoogleTablesExistsAsync()
        {
            var exists = true;

            await using var context = await GetContextAsync();
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new(typeof(RMDiscoveryGoogleSizeRange), "dbo"),
                new(typeof(RMDiscoveryGoogleWithoutInDate), "dbo"),
                new(typeof(RMDiscoveryGoogleRuleInfo), "dbo"),
                new(typeof(RMDiscoveryGoogleOrganizationInfo), "dbo"),
                new(typeof(RMDiscoveryGoogleMainJob), "dbo"),
                new(typeof(RMDiscoveryGoogleDiscoveryJob), "dbo"),
                new(typeof(RMDiscoveryGoogleAnalysisJob), "dbo"),
                new (typeof(RMDiscoveryGoogleExecutionInfo), "dbo"),
            };

            foreach (var tableSet in tableSets)
            {
                var existsSql = tableSet.GetExistsTableSql();
                var tableExists = await context.ExecuteScalarAsync<int>(existsSql);
                exists &= (tableExists == 1);
            }

            return exists;
        }

        public static async Task InitGoogleDatabaseAsync()
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
                await InitGoogleBasicTablesAsync();
                await InitGoogleBuildInDataListAsync();
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while init discovery google database info. has database [{hasDatabase}]. Error: {e}");
                if (!hasDatabase)
                {
                    using var context = RMDBContextManager.GetSystemDBContext();
                    await DropDatabaseAsync(dbName);
                }
                throw;
            }
        }

        public static async Task InitGoogleBasicTablesAsync()
        {
            await using var context = await GetContextAsync();

            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new(typeof(RMDiscoveryGoogleSizeRange), "dbo"),
                new(typeof(RMDiscoveryGoogleWithoutInDate), "dbo"),
                new(typeof(RMDiscoveryGoogleRuleInfo), "dbo"),
                new(typeof(RMDiscoveryGoogleOrganizationInfo), "dbo"),
                new(typeof(RMDiscoveryGoogleMainJob), "dbo"),
                new(typeof(RMDiscoveryGoogleDiscoveryJob), "dbo"),
                new(typeof(RMDiscoveryGoogleAnalysisJob), "dbo"),
                new (typeof(RMDiscoveryGoogleExecutionInfo), "dbo"),
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

        public static async Task InitGoogleBuildInDataListAsync()
        {
            var sizeRangeDao = new RMDiscoveryGoogleSizeRangeDao();
            var withoutDateDao = new RMDiscoveryGoogleWithoutInDateDao();
            await sizeRangeDao.InitBuildInDataAsync();
            await withoutDateDao.InitBuildInDataAsync();
        }

        public static async Task InitGoogleBasicTablesAsync(string googleOrganizationId)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetGoogleSchemaName(googleOrganizationId);
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryGoogleContainerInfo), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryGoogleDriveInfo), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryGoogleFileExtension), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryGoogleAggregateTotalData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryGoogleProfileFailedInfo), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryGoogleProfileInfo), schemaName),
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

            await InitGoogleBuildInDataListAsync(googleOrganizationId);
        }

        public static async Task InitGoogleRotTablesAsync(string googleOrganizationId)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetGoogleSchemaName(googleOrganizationId);
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryGoogleBasicRuleLevelRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryGoogleBasicCategoryLevelRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryGoogleBasicRootLevelRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryGoogleContainerRuleLevelRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryGoogleContainerCategoryLevelRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryGoogleContainerRootLevelRotData), schemaName),
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

        public static async Task InitGoogleInactiveTablesAsync(string googleOrganizationId, List<RMDiscoveryCustomColumn> inactiveDataCustomColumns = null)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetGoogleSchemaName(googleOrganizationId);
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryGoogleBasicInactiveData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryGoogleContainerInactiveData), schemaName),
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

        public static async Task InitGoogleBuildInDataListAsync(string googleOrganizationId)
        {
            var profileDao = new RMDiscoveryGoogleProfileDao();
            await profileDao.InitBuildInDataAsync(googleOrganizationId);
        }

        public static async Task DropGoogleTablesAsync(string googleOrganizationId)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetGoogleSchemaName(googleOrganizationId);

            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var queryProfileTableSql = "SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @SchemaName AND TABLE_NAME = @TableName";
            var exists = await context.ExecuteScalarAsync<int>(queryProfileTableSql, new SqlParameter("@SchemaName", schemaName), new SqlParameter("@TableName", "RMGoogleProfileInfoes"));

            if (exists == 1)
            {
                var queryProfileSql = $"SELECT Id FROM [{schemaName}].RMGoogleProfileInfoes";
                var dataCollection = await context.ExecuteQueryAsync(queryProfileSql);
                var profileIds = dataCollection.ToList<Guid>();
                foreach (var profileId in profileIds)
                {
                    var profileSchema = GetGoogleSchemaName(googleOrganizationId, profileId);
                    var profileTableSets = new List<RMDiscoveryDBTableSet>
                    {
                        new (typeof(RMDiscoveryGoogleProfileBasicInactiveData), profileSchema),
                        new (typeof(RMDiscoveryGoogleProfileContainerInactiveData), profileSchema),
                        new (typeof(RMDiscoveryGoogleProfileDriveInactiveData), profileSchema),
                        new (typeof(RMDiscoveryGoogleProfileBasicRotData), profileSchema),
                        new (typeof(RMDiscoveryGoogleProfileContainerRotData), profileSchema),
                        new (typeof(RMDiscoveryGoogleProfileDriveRotData), profileSchema),
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
                new(typeof(RMDiscoveryGoogleContainerInfo), schemaName),
                new(typeof(RMDiscoveryGoogleDriveInfo), schemaName),
                new(typeof(RMDiscoveryGoogleFileExtension), schemaName),
                new(typeof(RMDiscoveryGoogleFileExtension), schemaName),
                new(typeof(RMDiscoveryGoogleAggregateTotalData), schemaName),
                new(typeof(RMDiscoveryGoogleFileExtension), schemaName),
                new(typeof(RMDiscoveryGoogleContainerInactiveData), schemaName),
                new(typeof(RMDiscoveryGoogleBasicInactiveData), schemaName),
                new(typeof(RMDiscoveryGoogleContainerRootLevelRotData), schemaName),
                new(typeof(RMDiscoveryGoogleContainerCategoryLevelRotData), schemaName),
                new(typeof(RMDiscoveryGoogleContainerRuleLevelRotData), schemaName),
                new(typeof(RMDiscoveryGoogleBasicRootLevelRotData), schemaName),
                new(typeof(RMDiscoveryGoogleBasicCategoryLevelRotData), schemaName),
                new(typeof(RMDiscoveryGoogleBasicRuleLevelRotData), schemaName),
                new(typeof(RMDiscoveryGoogleProfileInfo), schemaName),
                new(typeof(RMDiscoveryGoogleProfileFailedInfo), schemaName),
            };
            foreach (var tableSet in tableSets)
            {
                var sql = tableSet.GetDropTableSql();
                await context.ExecuteNonQueryAsync(sql);
            }
        }
        
        public static async Task DropGoogleInactiveProfileTablesAsync(string googleOrganizationid, Guid profileId)
        {
            await using var context = await GetContextAsync();
            var profileSchema = GetGoogleSchemaName(googleOrganizationid, profileId);
            var profileTableSets = new List<RMDiscoveryDBTableSet>
            {
                new (typeof(RMDiscoveryGoogleProfileBasicInactiveData), profileSchema),
                new (typeof(RMDiscoveryGoogleProfileContainerInactiveData), profileSchema),
                new (typeof(RMDiscoveryGoogleProfileDriveInactiveData), profileSchema),
            };
            foreach (var sql in profileTableSets.Select(profileTableSet => profileTableSet.GetDropTableSql()))
            {
                await context.ExecuteNonQueryAsync(sql);
            }
        }
        
        public static async Task InitGoogleInactiveProfileTables(string googleOrganizationId, Guid profileId, List<RMDiscoveryCustomColumn> inactiveDataCustomColumns)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetGoogleSchemaName(googleOrganizationId, profileId);
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new (typeof(RMDiscoveryGoogleProfileBasicInactiveData), schemaName),
                new (typeof(RMDiscoveryGoogleProfileContainerInactiveData), schemaName),
                new (typeof(RMDiscoveryGoogleProfileDriveInactiveData), schemaName),
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
        
        public static async Task DropGoogleRotProfileTablesAsync(string googleOrganizationId, Guid profileId)
        {
            await using var context = await GetContextAsync();
            var profileSchema = GetGoogleSchemaName(googleOrganizationId, profileId);
            var profileTableSets = new List<RMDiscoveryDBTableSet>
            {
                new (typeof(RMDiscoveryGoogleProfileBasicRotData), profileSchema),
                new (typeof(RMDiscoveryGoogleProfileContainerRotData), profileSchema),
                new (typeof(RMDiscoveryGoogleProfileDriveRotData), profileSchema),
            };
            foreach (var sql in profileTableSets.Select(profileTableSet => profileTableSet.GetDropTableSql()))
            {
                await context.ExecuteNonQueryAsync(sql);
            }
        }
        
        public static async Task InitGoogleRotProfileTables(string googleOrganizationId, Guid profileId, List<RMDiscoveryCustomColumn> rotDataCustomColumns)
        {
            await using var context = await GetContextAsync();
            var schemaName = GetGoogleSchemaName(googleOrganizationId, profileId);
            await CreateSchema(schemaName);
            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new (typeof(RMDiscoveryGoogleProfileBasicRotData), schemaName),
                new (typeof(RMDiscoveryGoogleProfileContainerRotData), schemaName),
                new (typeof(RMDiscoveryGoogleProfileDriveRotData), schemaName),
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
    }
}
