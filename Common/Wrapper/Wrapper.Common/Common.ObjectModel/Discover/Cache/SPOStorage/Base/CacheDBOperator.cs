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
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading;

namespace AvePoint.Wrapper.Common.Common.ObjectModel.Discover.Cache.SPOStorage.Base
{
    public class CacheDBOperator<T> : IDisposable where T : SPOItem, new()
    {
        private static readonly AveLogger _logger = AveLogger.GetInstance(typeof(CacheDBOperator<T>));

        private const string DB_PATH_PREFIX = "SPOFolder_";
        
        private const int DEFAULT_PAGE_SIZE = 500;
        private string _itemsDbPath = string.Empty;

        private readonly object _lock = new object();
        private bool _alreadyInit;
        private string CacheFolderPath => SecurityUtils.SafeCombinePath(Environment.CurrentDirectory, FolderName);
        protected virtual string TableName => "Items";
        protected virtual string FolderName => "ItemStorage";

        public void InsertItems(IEnumerable<T> items, SPOFolder parentFolder)
        {
            if (items == null)
            {
                return;
            }

            var batch = items.Where(item => item != null).ToList();
            if (batch.Count == 0)
            {
                return;
            }

            EnsureDatabase();

            for (int page = 0; page * DEFAULT_PAGE_SIZE < batch.Count; page++)
            {
                var currentBatch = batch.Skip(page * DEFAULT_PAGE_SIZE).Take(DEFAULT_PAGE_SIZE).ToList();
                if (TryBatchInsert(currentBatch, parentFolder) == null)
                {
                    TrySingleInsert(currentBatch, parentFolder);
                }
            }
        }

        public void UpdateItemId(int id, string name, string parentFolder)
        {
            ExecuteWithConnection(connection =>
            {
                string updateQuery = $"UPDATE {SecurityUtils.SanitizeSQLSchemaName(TableName)} SET Id = @Id WHERE Name = @Name COLLATE NOCASE AND ParentFolder = @ParentFolder COLLATE NOCASE ";
                using (var command = new SQLiteCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ParentFolder", parentFolder);
                    command.ExecuteNonQuery();
                }
            }, ensureDatabase: false);
        }

        private List<T> TryBatchInsert(List<T> batch, SPOFolder parentFolder)
        {
            var insertedItems = new List<T>();
            bool batchInsertSuccess = false;
            ExecuteWithConnection(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var valueStrings = new List<string>();
                        var parameters = new List<SQLiteParameter>();
                        int idx = 0;
                        foreach (var item in batch)
                        {
                            string idParam = "@Id" + idx;
                            string nameParam = "@Name" + idx;
                            string parentParam = "@ParentFolder" + idx;
                            valueStrings.Add($"({idParam}, {nameParam}, {parentParam})");
                            parameters.Add(new SQLiteParameter(idParam, System.Data.DbType.Int32) { Value = item.Id });
                            parameters.Add(new SQLiteParameter(nameParam, System.Data.DbType.String) { Value = item.Name ?? string.Empty });
                            parameters.Add(new SQLiteParameter(parentParam, System.Data.DbType.String) { Value = parentFolder.FullPath });
                            idx++;
                        }
                        string insertQuery = $"INSERT INTO {SecurityUtils.SanitizeSQLSchemaName(TableName)} (Id, Name, ParentFolder) VALUES {string.Join(", ", valueStrings)}";
                        using (var command = new SQLiteCommand(insertQuery, connection, transaction))
                        {
                            command.Parameters.AddRange(parameters.ToArray());
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        insertedItems.AddRange(batch);
                        batchInsertSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn($"Batch insert failed, fallback to single insert. ex: {ex.Message}");
                        transaction.Rollback();
                    }
                }
            });
            return batchInsertSuccess ? insertedItems : null;
        }

        /// <summary>
        /// Insert items one by one, handling duplicates. Returns inserted items.
        /// </summary>
        private List<T> TrySingleInsert(List<T> batch, SPOFolder parentFolder)
        {
            var insertedItems = new List<T>();
            ExecuteWithConnection(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string insertQuery = $"INSERT INTO {SecurityUtils.SanitizeSQLSchemaName(TableName)} (Id, Name, ParentFolder) VALUES (@Id, @Name, @ParentFolder)";
                        using (var command = new SQLiteCommand(insertQuery, connection, transaction))
                        {
                            var idParam = command.Parameters.Add("@Id", System.Data.DbType.Int32);
                            var nameParam = command.Parameters.Add("@Name", System.Data.DbType.String);
                            var ParentFolderParam = command.Parameters.Add("@ParentFolder", System.Data.DbType.String);

                            foreach (var item in batch)
                            {
                                idParam.Value = item.Id;
                                nameParam.Value = item.Name ?? string.Empty;
                                ParentFolderParam.Value = parentFolder.FullPath;
                                try
                                {
                                    command.ExecuteNonQuery();
                                    insertedItems.Add(item);
                                }
                                catch (SQLiteException ex) when (ex.ResultCode == SQLiteErrorCode.Constraint)
                                {
                                    _logger.Warn($"Duplicate cache item ignored, id: {item.Id}, name :{item.Name}, parentPath:{parentFolder.FullPath}, ex: {ex.Message}");
                                }
                            }
                        }
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }                    
                }
            });
            return insertedItems;
        }

        public void RemoveItem(string name, string parentFolder)
        {
            ExecuteWithConnection(connection =>
            {
                string deleteQuery = $"DELETE FROM {SecurityUtils.SanitizeSQLSchemaName(TableName)} WHERE Name = @Name COLLATE NOCASE and ParentFolder = @ParentFolder COLLATE NOCASE ";
                using (var command = new SQLiteCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@ParentFolder", parentFolder);
                    command.Parameters.AddWithValue("@Name", name);
                    command.ExecuteNonQuery();
                }
            }, ensureDatabase: false);
        }

        public void Clear(string parentFolder)
        {
            ExecuteWithConnection(connection =>
            {
                string deleteQuery = $"DELETE FROM {SecurityUtils.SanitizeSQLSchemaName(TableName)} WHERE ParentFolder = @ParentFolder COLLATE NOCASE ";
                using (var command = new SQLiteCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@ParentFolder", parentFolder);
                    command.ExecuteNonQuery();
                }
            }, ensureDatabase: false);
        }

        public bool ContainsItem(string name, string parentFolder)
        {
            if (!_alreadyInit)
            {
                return false;
            }

            bool exists = false;
            ExecuteWithConnection(connection =>
            {
                string selectQuery = $"SELECT 1 FROM {TableName} WHERE Name = @Name COLLATE NOCASE AND ParentFolder = @ParentFolder COLLATE NOCASE LIMIT 1";
                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@ParentFolder", parentFolder);
                    using (var reader = command.ExecuteReader())
                    {
                        exists = reader.Read();
                    }
                }
            }, ensureDatabase: false);

            return exists;
        }

        public int CountItems(string parentFolder)
        {
            if (!_alreadyInit)
            {
                return 0;
            }

            int count = 0;
            ExecuteWithConnection(connection =>
            {
                string countQuery = $"SELECT COUNT(*) FROM {SecurityUtils.SanitizeSQLSchemaName(TableName)} WHERE ParentFolder = @ParentFolder COLLATE NOCASE ";
                using (var command = new SQLiteCommand(countQuery, connection))
                {
                    command.Parameters.AddWithValue("@ParentFolder", parentFolder);
                    var result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        count = Convert.ToInt32(result);
                    }
                }
            }, ensureDatabase: false);

            return count;
        }

        public virtual T QueryItemByName(string name, string parentFolder)
        {
            lock (_lock)
            {
                if (!_alreadyInit)
                {
                    return null;
                }
                T res = null;
                using (var connection = GetConnection())
                {
                    connection.Open();
                    string selectQuery = $"SELECT Id, Name FROM {SecurityUtils.SanitizeSQLSchemaName(SecurityUtils.SanitizeSQLSchemaName(TableName))} WHERE ParentFolder = @ParentFolder COLLATE NOCASE AND Name = @Name COLLATE NOCASE ORDER BY Id LIMIT 1 OFFSET 0";
                    using (var command = new SQLiteCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ParentFolder", parentFolder);
                        command.Parameters.AddWithValue("@Name", name);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                res = new T()
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                };
                            }                            
                        }
                    }
                }
                return res;
            }
        }

        public IReadOnlyList<T> QueryItems(int offset, string parentFolder, int pageSize = DEFAULT_PAGE_SIZE)
        {
            lock (_lock)
            {
                if (!_alreadyInit)
                {
                    return Array.Empty<T>();
                }

                var items = new List<T>();
                using (var connection = GetConnection())
                {
                    connection.Open();
                    string selectQuery = $"SELECT Id, Name FROM {SecurityUtils.SanitizeSQLSchemaName(TableName)} WHERE ParentFolder = @ParentFolder COLLATE NOCASE ORDER BY Id LIMIT @PageSize OFFSET @Offset";
                    using (var command = new SQLiteCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@PageSize", pageSize);
                        command.Parameters.AddWithValue("@Offset", offset);
                        command.Parameters.AddWithValue("@ParentFolder", parentFolder);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(new T()
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                });
                            }
                        }
                    }
                }
                return items;
            }
        }

        public void Dispose()
        {
            if (!_alreadyInit)
            {
                return;
            }

            try
            {
                lock (_lock)
                {
                    if (System.IO.File.Exists(_itemsDbPath))
                    {
                        System.IO.File.Delete(_itemsDbPath);
                        _itemsDbPath = string.Empty;
                    }
                    _alreadyInit = false;
                }                
            }
            catch (Exception ex)
            {
                _logger.Error($"Error deleting database file: {_itemsDbPath}, ex: {ex.Message}");
            }
        }

        ~CacheDBOperator()
        {
            try
            {
                if (_alreadyInit)
                {
                    ResourcesRecovery.Enqueue(_itemsDbPath);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Have ex when clean dbItem cache, ex: {ex}");
            }
        }

        private void EnsureDatabase()
        {
            if (_alreadyInit)
            {
                return;
            }

            lock (_lock)
            {
                if (_alreadyInit)
                {
                    return;
                }

                if (!System.IO.Directory.Exists(CacheFolderPath))
                {
                    System.IO.Directory.CreateDirectory(CacheFolderPath);
                }

                _itemsDbPath = $"{CacheFolderPath}/{DB_PATH_PREFIX}_{Guid.NewGuid()}.db";
                using (var connection = GetConnection())
                {
                    connection.Open();
                    CreateTable(connection);
                }
            }
        }

        private SQLiteConnection GetConnection()
        {
            var builder = new SQLiteConnectionStringBuilder
            {
                DataSource = _itemsDbPath
            };
            return new SQLiteConnection(builder.ToString());
        }

        private void ExecuteWithConnection(Action<SQLiteConnection> action, bool ensureDatabase = true)
        {
            lock (_lock)
            {
                if (ensureDatabase)
                {
                    EnsureDatabase();
                }
                else if (!_alreadyInit)
                {
                    return;
                }

                using (var connection = GetConnection())
                {
                    connection.Open();
                    action(connection);
                }
            }
        }

        private void CreateTable(SQLiteConnection connection)
        {
            ResourcesRecovery.Start();
            string createTableQuery = $"CREATE TABLE IF NOT EXISTS {SecurityUtils.SanitizeSQLSchemaName(TableName)} (Id INTEGER, Name TEXT COLLATE NOCASE, ParentFolder TEXT COLLATE NOCASE);" +
                $"CREATE INDEX parentFolder_name ON {SecurityUtils.SanitizeSQLSchemaName(TableName)}(ParentFolder COLLATE NOCASE, Name COLLATE NOCASE);" +
                $"CREATE INDEX idx ON {SecurityUtils.SanitizeSQLSchemaName(TableName)}(Id);";
            using (var command = new SQLiteCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
            _alreadyInit = true;
        }

        private static class ResourcesRecovery
        {
            private static readonly AveLogger _logger = AveLogger.GetInstance(typeof(ResourcesRecovery));
            private static Thread _executeThread;
            private static readonly Queue<string> _dbPathQueue = new Queue<string>();
            private static readonly object _lock = new object();
            private static readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

            public static void Start()
            {
                if (_executeThread != null)
                {
                    return;
                }

                lock (_lock)
                {
                    if (_executeThread == null)
                    {
                        _executeThread = new Thread(RealResourcesRecovery)
                        {
                            IsBackground = true
                        };
                        _executeThread.Start();
                    }
                }
            }

            public static void Enqueue(string dbPath)
            {
                if (string.IsNullOrWhiteSpace(dbPath))
                {
                    return;
                }

                lock (_dbPathQueue)
                {
                    _dbPathQueue.Enqueue(dbPath);
                }

                Start();
            }

            private static void RealResourcesRecovery()
            {
                while (true)
                {
                    try
                    {
                        if (_cancellationTokenSource != null && _cancellationTokenSource.IsCancellationRequested)
                        {
                            _logger.Info($"exit ResourcesRecovery thread ");
                            _cancellationTokenSource.Dispose();
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Have ex when check cancellation token, ex: {ex}");
                    }


                    string dbPath = string.Empty;

                    lock (_dbPathQueue)
                    {
                        if (_dbPathQueue.Count > 0)
                        {
                            dbPath = _dbPathQueue.Dequeue();
                        }
                    }

                    if (string.IsNullOrWhiteSpace(dbPath))
                    {
                        Thread.Sleep(TimeSpan.FromMinutes(1));
                        continue;
                    }

                    try
                    {
                        if (System.IO.File.Exists(dbPath))
                        {
                            System.IO.File.Delete(dbPath);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Fail delete cache item db, db: {dbPath}, ex: {e}");
                    }
                }
            }
        }
    }
}
