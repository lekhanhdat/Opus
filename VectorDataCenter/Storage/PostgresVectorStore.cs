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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Impl;
using Npgsql;
using Pgvector;
using Pgvector.Npgsql;
using AvePoint.GCommon.Utility;

namespace AvePoint.RA.VectorDataCenter.Storage
{
    public class PostgresVectorStore : IVectorStore
    {
        public string Name => "PostgreSQL";

        private static readonly RALogger _logger = RALogger.GetInstance(typeof(PostgresVectorStore));
        private readonly RMTenantVectorPostgreMappingDao _mappingDao = new RMTenantVectorPostgreMappingDao();
        private readonly string _baseConnectionString;
        private static readonly object _lockObject = new object();
        private static readonly Dictionary<string, NpgsqlDataSource> _dataSources = new Dictionary<string, NpgsqlDataSource>();

        public PostgresVectorStore(bool Init = true)
        {
            _baseConnectionString = RMGlobalConfiguration.EncryptConfig[RMCommonSettingKey.VECTOR_DB_CONNECTION_STRING];
            if (Init)
            {
                var (dbName, schemaName) = GetTenantContext();
                InitializeDatabaseIfNeededAsync(dbName, schemaName, 768).GetAwaiter().GetResult();
            }
        }

        #region Private Helper Methods

        private (string dbName, string schemaName) GetTenantContext()
        {
            var tenantId = TenantLocalValue.LogonGroupId;
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new InvalidOperationException("Tenant ID is not available in the current context.");
            }

            var dbName = _mappingDao.GetOrCreateDatabaseName(tenantId);
            var schemaName = SanitizeIdentifier(tenantId);

            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(dbName);

            return (dbName, "s_"+schemaName);
        }

        private NpgsqlDataSource GetDataSource(string dbName)
        {
            if (!_dataSources.ContainsKey(dbName))
            {
                lock (_lockObject)
                {
                    if (!_dataSources.ContainsKey(dbName))
                    {
                        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_baseConnectionString)
                        {
                            Database = dbName
                        };
                        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionStringBuilder.ToString());
                        dataSourceBuilder.UseVector();
                        _dataSources[dbName] = dataSourceBuilder.Build();
                    }
                }
            }
            return _dataSources[dbName];
        }

        private string SanitizeIdentifier(string identifier)
        {
            var sanitized = identifier.ToLower().Replace("-", "_");
            return Regex.Replace(sanitized, @"[^a-z0-9_]", "");
        }

        private async Task InitializeDatabaseIfNeededAsync(string dbName, string schemaName, int vectorDimension)
        {
            try
            {
                SecurityUtils.SanitizeSQLSchemaName(dbName);
                SecurityUtils.SanitizeSQLSchemaName(schemaName);
                var systemConnectionStringBuilder = new NpgsqlConnectionStringBuilder(_baseConnectionString) { Database = "postgres" };
                await using var systemDataSource = new NpgsqlDataSourceBuilder(systemConnectionStringBuilder.ToString()).Build();
                await using (var systemConn = await systemDataSource.OpenConnectionAsync())
                {
                    var checkDbCmd = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @dbname", systemConn);
                    checkDbCmd.Parameters.AddWithValue("dbname", dbName);
                    var dbExists = await checkDbCmd.ExecuteScalarAsync() is not null;

                    if (!dbExists)
                    {
                        var createDbCmdText = $"CREATE DATABASE \"{dbName.Replace("\"", "\"\"")}\"";
                        var createDbCmd = new NpgsqlCommand(createDbCmdText, systemConn);
                        await createDbCmd.ExecuteNonQueryAsync();
                        _logger.Info($"Created PostgreSQL database: {dbName}");
                    }
                }

                await using var tenantConn = await GetDataSource(dbName).OpenConnectionAsync();

                var checkIndexCmd = new NpgsqlCommand("SELECT 1 FROM pg_indexes WHERE schemaname = @schema AND tablename = 'vectors' AND indexname = 'vectors_vector_idx'", tenantConn);
                checkIndexCmd.Parameters.AddWithValue("schema", schemaName);
                var indexExists = await checkIndexCmd.ExecuteScalarAsync() is not null;

                if (!indexExists)
                {
                    await using var transaction = await tenantConn.BeginTransactionAsync();

                    var setupCmdText = $@"
                        CREATE SCHEMA IF NOT EXISTS {schemaName};
                        CREATE EXTENSION IF NOT EXISTS vector;
                        CREATE TABLE IF NOT EXISTS {schemaName}.vectors (
                            id TEXT PRIMARY KEY,
                            name TEXT,
                            vector vector({vectorDimension}),
                            metadata TEXT
                        );";

                    var setupCmd = new NpgsqlCommand(setupCmdText, tenantConn, transaction);
                    await setupCmd.ExecuteNonQueryAsync();
                    await transaction.CommitAsync();
                    _logger.Info($"Successfully initialized schema, table and index in database: {dbName}, schema: {schemaName}");
                }
            }
            catch (Exception e)
            {
                if (e is PostgresException pe && (pe.SqlState == "42P04" || pe.SqlState == "42710" || pe.SqlState == "42P07"))
                {
                    _logger.Warn($"Initialization race condition ignored in database {dbName}: {e.Message}");
                }
                else
                {
                    _logger.Error($"Error during database initialization for {dbName}: {e.Message}", e);
                    throw;
                }
            }
        }

        public async Task DropVectorDbIfExist(string dbName, string schemaName)
        {
            try
            {
                SecurityUtils.SanitizeSQLSchemaName(schemaName);
                SecurityUtils.SanitizeSQLSchemaName(dbName);
                await using var tenantConn = await GetDataSource(dbName).OpenConnectionAsync();
                await using var transaction = await tenantConn.BeginTransactionAsync();
                var dropCmdText = $@"DROP SCHEMA IF EXISTS {schemaName} CASCADE;";
                var dropCmd = new NpgsqlCommand(dropCmdText, tenantConn, transaction);
                dropCmd.Parameters.AddWithValue("schema", schemaName);

                await dropCmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();

                _logger.Info($"Dropped schema '{schemaName}' and related objects from database '{dbName}'.");

            }
            catch (PostgresException pe)
            {
                _logger.Warn("PostgreSQL error while dropping schema {0} in DB {1}: {2} ({3})", schemaName, dbName, pe.MessageText, pe.SqlState);
            }
            catch (Exception e)
            {
                _logger.Error($"Error while dropping schema {schemaName} in database {dbName}: {e.Message}", e);
                throw;
            }
        }

        #endregion

        #region Public IVectorStore Methods

        public async Task StoreVectorAsync(Guid id, string name, float[] vector, string metadata)
        {
            try
            {
                var (dbName, schemaName) = GetTenantContext();

                await using var conn = await GetDataSource(dbName).OpenConnectionAsync();

                var cmdText = $@"
                    INSERT INTO {schemaName}.vectors (id, name, vector, metadata) 
                    VALUES (@id, @name, @vector, @metadata) 
                    ON CONFLICT (id) DO UPDATE SET 
                        name = excluded.name,
                        vector = excluded.vector, 
                        metadata = excluded.metadata;";

                await using var cmd = new NpgsqlCommand(cmdText, conn);

                cmd.Parameters.AddWithValue("id", id.ToString());
                cmd.Parameters.AddWithValue("name", (object)name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("vector", new Vector(vector));
                cmd.Parameters.AddWithValue("metadata", (object)metadata ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
                _logger.Info($"Stored vector for id: {id} in database: {dbName}, schema: {schemaName}");
            }
            catch (Exception e)
            {
                _logger.Error($"Error storing vector: {e.Message}", e);
                throw;
            }
        }

        public async Task<(string id, float? scoreFromStore)[]> QuerySimilarAsync(float[] vector, int topK = 5)
        {
            var (dbName, schemaName) = GetTenantContext();
            var results = new List<(string, float?)>();

            try
            {
                await using var conn = await GetDataSource(dbName).OpenConnectionAsync();

                var cmdText = $@"
                    SELECT id, vector, 1 - (vector <=> @query_vector) as similarity
                    FROM {schemaName}.vectors 
                    ORDER BY vector <=> @query_vector 
                    LIMIT @top_k";

                await using var cmd = new NpgsqlCommand(cmdText, conn);

                cmd.Parameters.AddWithValue("query_vector", new Vector(vector));
                cmd.Parameters.AddWithValue("top_k", topK);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var id = reader.GetString(0);
                    var similarity = reader.IsDBNull(2) ? (float?)null : Convert.ToSingle(reader.GetValue(2));
                    results.Add((id, similarity));
                }
                return results.ToArray();
            }
            catch (Exception e)
            {
                if (e is PostgresException pe && pe.SqlState == "42P01") // undefined_table
                {
                    _logger.Warn($"Table not found for query in schema '{schemaName}'. Returning empty results.");
                    return results.ToArray();
                }
                _logger.Error($"Error querying similar vectors from {dbName}, schema {schemaName}: {e.Message}", e);
                throw;
            }
        }

        public async Task<string> QueryMetaDataByTermId(Guid termId)
        {
            var (dbName, schemaName) = GetTenantContext();
            try
            {
                await using var conn = await GetDataSource(dbName).OpenConnectionAsync();

                var cmdText = $"SELECT metadata FROM {schemaName}.vectors WHERE id = @id";
                await using var cmd = new NpgsqlCommand(cmdText, conn);
                cmd.Parameters.AddWithValue("id", termId.ToString());

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? string.Empty;
            }
            catch (Exception e)
            {
                _logger.Error($"Error querying metadata from {dbName}, schema {schemaName}: {e.Message}", e);
                return string.Empty;
            }
        }

        public async Task DeleteVectorAsync(Guid id)
        {
            var (dbName, schemaName) = GetTenantContext();
            try
            {
                await using var conn = await GetDataSource(dbName).OpenConnectionAsync();

                var cmdText = $"DELETE FROM {schemaName}.vectors WHERE id = @id";
                await using var cmd = new NpgsqlCommand(cmdText, conn);
                cmd.Parameters.AddWithValue("id", id.ToString());

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.Error($"Error deleting vector from {dbName}, schema {schemaName}: {e.Message}", e);
                throw;
            }
        }

        public async Task DeleteVectorsByIdsAsync(List<Guid> ids)
        {
            if (ids == null || !ids.Any()) return;

            var (dbName, schemaName) = GetTenantContext();
            try
            {
                await using var conn = await GetDataSource(dbName).OpenConnectionAsync();

                var idStrings = ids.Select(id => id.ToString()).ToArray();
                var cmdText = $"DELETE FROM {schemaName}.vectors WHERE id = ANY(@ids)";
                await using var cmd = new NpgsqlCommand(cmdText, conn);
                cmd.Parameters.AddWithValue("ids", idStrings);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.Error($"Error deleting vectors from {dbName}, schema {schemaName}: {e.Message}", e);
                throw;
            }
        }

        #endregion
    }
}