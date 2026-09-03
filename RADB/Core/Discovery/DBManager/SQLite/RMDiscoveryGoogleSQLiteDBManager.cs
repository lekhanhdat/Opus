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
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core.Discovery.Context.SQLite;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Office365;
using Util.MSAzure;

namespace AvePoint.RA.DB.Core.Discovery.DBManager.SQLite
{
    public class RMDiscoveryGoogleSQLiteDBManager
    {

        private const string STORAGE_CONTAINER_NAME = "opus-sqlite-database-container";

        private const string SQLITE_DB_NAME = "discoverygoogle.db";

        private static readonly string STORAGE_CONNECTION_STRING = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];

        private static string TenantId => TenantLocalValue.LogonGroupId;

        public static RMDiscoveryGoogleSQLiteDBContext GetContext()
        {
            var dbPath = GetDBPath();
            return new(dbPath);
        }

        public static void CreateDatabase()
        {
            var dbPath = GetDBPath();
#if DEBUG
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
#endif
            if (File.Exists(dbPath))
            {
                return;
            }

            EnsureDBFolderPath();
            SQLiteConnection.CreateFile(dbPath);
        }

        public static async Task DownloadDatabaseAsync()
        {
            var dbPath = GetDBPath();

#if DEBUG
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
#endif

            if (File.Exists(dbPath))
            {
                return;
            }

            EnsureDBFolderPath();

            var containerClient = StorageUtil.GetContainerClient(STORAGE_CONNECTION_STRING, STORAGE_CONTAINER_NAME);
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(SecurityUtils.SafeCombinePath(TenantId.ToString().ToLower(), SQLITE_DB_NAME));
            var exists = await blobClient.ExistsAsync();

            if (!exists.Value)
            {
                throw new FileNotFoundException($"No database [{SecurityUtils.SafeCombinePath(TenantId.ToString().ToLower(), SQLITE_DB_NAME)}] found in storage.");
            }
            await blobClient.DownloadToAsync(dbPath);
            return;
        }

        public static async Task InitInactiveTablesAsync(string googleOrganizationId, IEnumerable<RMDiscoveryCustomColumn> customColumns)
        {
            var dbPath = GetDBPath();
            using var context = new RMDiscoveryGoogleSQLiteDBContext(dbPath);
            var schemaName = GetSchemaName(googleOrganizationId);

            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryGoogleDriveInactiveData), schemaName),
            };

            foreach (var tableSet in tableSets)
            {
                var existsSql = tableSet.GetSqliteExistsTableSql();
                var exists = await context.ExecuteScalarAsync<long>(existsSql);
                if (exists == 1)
                {
                    continue;
                }
                var createSql = tableSet.GetSqliteCreateTableSql(customColumns);
                await context.ExecuteNonQueryAsync(createSql);

                foreach (var indexSql in tableSet.GetSqliteAddIndexSql())
                {
                    await context.ExecuteNonQueryAsync(indexSql);
                }
            }
        }

        public static async Task InitRotTablesAsync(string googleOrganizationId)
        {
            var dbPath = GetDBPath();
            using var context = new RMDiscoveryGoogleSQLiteDBContext(dbPath);
            var schemaName = GetSchemaName(googleOrganizationId);

            var tableSets = new List<RMDiscoveryDBTableSet>
            {
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryGoogleDriveRuleLevelRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryGoogleDriveCategoryLevelRotData), schemaName),
                new RMDiscoveryDBTableSet(typeof(RMDiscoveryGoogleDriveRootLevelRotData), schemaName),
            };

            foreach (var tableSet in tableSets)
            {
                var existsSql = tableSet.GetSqliteExistsTableSql();
                var exists = await context.ExecuteScalarAsync<long>(existsSql);
                if (exists == 1)
                {
                    continue;
                }
                var createSql = tableSet.GetSqliteCreateTableSql();
                await context.ExecuteNonQueryAsync(createSql);

                foreach (var indexSql in tableSet.GetSqliteAddIndexSql())
                {
                    await context.ExecuteNonQueryAsync(indexSql);
                }
            }
        }

        public static async Task SyncDatabaseToStorageAsync()
        {
            var dbPath = GetDBPath();
            var containerClient = StorageUtil.GetContainerClient(STORAGE_CONNECTION_STRING, STORAGE_CONTAINER_NAME);
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(SecurityUtils.SafeCombinePath(TenantId.ToString().ToLower(), SQLITE_DB_NAME));
            await blobClient.DeleteIfExistsAsync();
            using (var fileStream = File.OpenRead(dbPath))
            {
                await containerClient.UploadBlobAsync(SecurityUtils.SafeCombinePath(TenantId.ToString().ToLower(), SQLITE_DB_NAME), fileStream);
            }
            File.Delete(dbPath);
        }

        private static string GetDBPath()
        {
            return SecurityUtils.SafeCombinePath(Environment.CurrentDirectory, STORAGE_CONTAINER_NAME, TenantId.ToString().ToLower(), SQLITE_DB_NAME);
        }

        private static void EnsureDBFolderPath()
        {
            var dbFolderPath = SecurityUtils.SafeCombinePath(Environment.CurrentDirectory, STORAGE_CONTAINER_NAME);
            if (!Directory.Exists(dbFolderPath))
            {
                Directory.CreateDirectory(dbFolderPath);
            }

            dbFolderPath = SecurityUtils.SafeCombinePath(dbFolderPath, TenantId.ToString().ToLower());
            if (!Directory.Exists(dbFolderPath))
            {
                Directory.CreateDirectory(dbFolderPath);
            }
        }

        public static string GetSchemaName(string googleOrganizationId)
        {
            return "s_" + SecurityUtils.SanitizeSQLSchemaName(googleOrganizationId.ToLower().Replace("-", ""));
        }
    }
}
