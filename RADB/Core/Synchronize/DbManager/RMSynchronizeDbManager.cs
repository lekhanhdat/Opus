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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core.Synchronize.DbContext;
using AvePoint.RA.DB.Core.Synchronize.DbContext.Base;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao.SynchronizeDao;
using AvePoint.RA.DB.Dao.SynchronizeRemoteNodeDao.Imp;
using AvePoint.RA.DB.Model;
using Azure.Storage.Blobs.Models;
using Cloud.Sdk.AosModern;
using Cloud.Sdk.Data.Aos.Tenant;
using Cloud.Sdk.Data.AosModern;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Util.MSAzure;

namespace AvePoint.RA.DB.Core.Synchronize.DbManager;

public class RMSynchronizeDbManager
{
    private static readonly RALogger _logger = RALogger.GetInstance(typeof(RMSynchronizeDbManager));

    private const string STORAGE_CONTAINER_NAME = "opus-sqlite-database-container";

    private static string SQLITE_DB_NAME = "synchronization_remote_node.db";

    private static readonly string STORAGE_CONNECTION_STRING =
        RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];

    private static string TenantId => TenantLocalValue.LogonGroupId;

    private static ISynchronizeDbContext s_DbContext = new SqliteSynchronizeDbContext();

    private static IRMRemoteNodeDao s_RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
    private static IRMKeyValueDao s_KeyValueSqlSeverDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

    private static IKeyValueSqliteDao s_KeyValueSqliterDao = new KeyValueSqliteDao();

    public static readonly string LastSyncTimeKey = "LastSyncRemoteNodeTime";

    public static void UpdateSqliteDbName(string dbName)
    {
        SQLITE_DB_NAME = dbName;
    }

    public static ISynchronizeDbContext GetContext()
    {
        var dbPath = GetDbPath();

        s_DbContext.OpenAsync(dbPath).GetAwaiter().GetResult();

        return s_DbContext;
    }

    private static void CreateDatabase()
    {
        var dbPath = GetDbPath();

        EnsureDBFolderPath();

        s_DbContext.CreateDatabase(dbPath);
    }

    public static async Task DownloadDatabaseAsync()
    {
        var dbPath = GetDbPath();

        EnsureDBFolderPath();

        var containerClient = StorageUtil.GetContainerClient(STORAGE_CONNECTION_STRING, STORAGE_CONTAINER_NAME);
        await containerClient.CreateIfNotExistsAsync();
        var blobClient =
            containerClient.GetBlobClient(SecurityUtils.SafeCombinePath(TenantId.ToLower(), SQLITE_DB_NAME));
        var exists = await blobClient.ExistsAsync();

        if (!exists.Value)
        {
            _logger.Info(
                $"No database [{SecurityUtils.SafeCombinePath(TenantId.ToLower(), SQLITE_DB_NAME)}] found in storage.");
            CreateDatabase();
            await InitTablesAsync();
            await WriteToSqliteDatabaseAsync();
        }
        else
        {
            _logger.Info(
                $"Downloaded database [{SecurityUtils.SafeCombinePath(TenantId.ToLower(), SQLITE_DB_NAME)}] found in storage.");
            await blobClient.DownloadToAsync(dbPath);
            await CheckLastSyncTimeAsync();
        }
    }

    private static async Task WriteToSqliteDatabaseAsync()
    {
        using var performance = new PerformanceScope("Write To Sqlite");


        await foreach (var remoteNodes in s_RMRemoteNodeDao.GetAllRemoteNodesAsync())
        {
            await using var context = GetContext();
            await context.ExecuteInsertAsync(remoteNodes);
        }
    }

    private static async Task CheckLastSyncTimeAsync()
    {
        var lastSyncTimeInSqlite = await s_KeyValueSqliterDao.GetValueByKeyAsync(LastSyncTimeKey);
        var lastSyncTimeInSqlServer = s_KeyValueSqlSeverDao.GetValueByKey(LastSyncTimeKey)?.Value;
        if (lastSyncTimeInSqlite != lastSyncTimeInSqlServer)
        {
            _logger.Info("Last sync time in sqlite and sqlserver not same");
            File.Delete(GetDbPath());
            CreateDatabase();
            await InitTablesAsync();
            await WriteToSqliteDatabaseAsync();
        }
    }

    private static async Task InitTablesAsync()
    {
        await using var context = GetContext();

        List<ISynchronizeDbTableSet> tableSets = s_DbContext.GetTableSets(GetSchemaName());

        foreach (var tableSet in tableSets)
        {
            var existsSql = tableSet.GetExistsTableSql();
            var exists = await context.ExecuteScalarAsync<long>(existsSql);
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

    public static async Task<int> SyncDatabaseToStorageAsync()
    {
        var dbPath = GetDbPath();
        var containerClient = StorageUtil.GetContainerClient(STORAGE_CONNECTION_STRING, STORAGE_CONTAINER_NAME);
        await containerClient.CreateIfNotExistsAsync();
        var blobClient =
            containerClient.GetBlobClient(SecurityUtils.SafeCombinePath(TenantId.ToLower(), SQLITE_DB_NAME));
        await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
        try
        {
            await using (var fileStream = File.OpenRead(dbPath))
            {
                await containerClient.UploadBlobAsync(
                    SecurityUtils.SafeCombinePath(TenantId.ToLower(), SQLITE_DB_NAME), fileStream);
            }
            _logger.Info(
                $"Updated database [{SecurityUtils.SafeCombinePath(TenantId.ToLower(), SQLITE_DB_NAME)}] in storage.");
            File.Delete(dbPath);
            return (int)HttpStatusCode.Created;
        }
        catch (Exception ex)
        {
            _logger.Error($"Sync database to storage failed: {ex}");
            throw;
        }
    }

    private static void EnsureDBFolderPath()
    {
        var dbFolderPath = SecurityUtils.SafeCombinePath(Environment.CurrentDirectory, STORAGE_CONTAINER_NAME);
        if (!Directory.Exists(dbFolderPath))
        {
            Directory.CreateDirectory(dbFolderPath);
        }

        dbFolderPath = SecurityUtils.SafeCombinePath(dbFolderPath, TenantId.ToLower());
        if (!Directory.Exists(dbFolderPath))
        {
            Directory.CreateDirectory(dbFolderPath);
        }
    }

    public static string GetSchemaName()
    {
        return "s_" + TenantId.ToLower().Replace("-", "");
    }

    public static string GetDbPath()
    {
        return SecurityUtils.SafeCombinePath(Environment.CurrentDirectory, STORAGE_CONTAINER_NAME,
            TenantId.ToLower(), SQLITE_DB_NAME);
    }
}