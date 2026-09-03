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
using Aspose.Pdf;
using AvePoint.GCommon.Utility;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Calculator.Duplicate
{
    public class RMDiscoveryOffice365DuplicateCalculateCacheManager : IDisposable
    {
        private readonly string _dbPath;

        private readonly string _connectionString;

        public RMDiscoveryOffice365DuplicateCalculateCacheManager()
        {
            _dbPath = SecurityUtils.SafeCombinePath(Environment.CurrentDirectory, "duplicateData.db");
            _connectionString = $"Data Source={_dbPath};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            if (!File.Exists(_dbPath))
            {
                SQLiteConnection.CreateFile(_dbPath);
            }

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            string createHashCodeTableQuery = @"
                CREATE TABLE IF NOT EXISTS HashCodeData (
                    HashCode INTEGER PRIMARY KEY,
                    ItemCount INTEGER
                )";
            using (var command = new SQLiteCommand(createHashCodeTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            string createItemUniqueIdsTableQuery = @"
                CREATE TABLE IF NOT EXISTS ItemUniqueIdsData (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    HashCode INTEGER,
                    ItemUniqueId TEXT
                )";
            using (var command = new SQLiteCommand(createItemUniqueIdsTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public void InsertOrUpdateDuplicateData(int hashCode, string itemUniqueId)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            string selectHashCodeQuery = "SELECT ItemCount FROM HashCodeData WHERE HashCode = @HashCode";
            using var selectCommand = new SQLiteCommand(selectHashCodeQuery, connection);
            selectCommand.Parameters.AddWithValue("@HashCode", hashCode);
            var result = selectCommand.ExecuteScalar();
            if (result == null)
            {
                string insertHashCodeQuery = "INSERT INTO HashCodeData (HashCode, ItemCount) VALUES (@HashCode, 1)";
                using var insertCommand = new SQLiteCommand(insertHashCodeQuery, connection);
                insertCommand.Parameters.AddWithValue("@HashCode", hashCode);
                insertCommand.ExecuteNonQuery();
            }
            else
            {
                int itemCount = Convert.ToInt32(result) + 1;
                string updateHashCodeQuery = "UPDATE HashCodeData SET ItemCount = @ItemCount WHERE HashCode = @HashCode";
                using var updateCommand = new SQLiteCommand(updateHashCodeQuery, connection);
                updateCommand.Parameters.AddWithValue("@ItemCount", itemCount);
                updateCommand.Parameters.AddWithValue("@HashCode", hashCode);
                updateCommand.ExecuteNonQuery();
            }

            string insertItemUniqueIdQuery = "INSERT INTO ItemUniqueIdsData (HashCode, ItemUniqueId) VALUES (@HashCode, @ItemUniqueId)";
            using (var insertCommand = new SQLiteCommand(insertItemUniqueIdQuery, connection))
            {
                insertCommand.Parameters.AddWithValue("@HashCode", hashCode);
                insertCommand.Parameters.AddWithValue("@ItemUniqueId", itemUniqueId);
                insertCommand.ExecuteNonQuery();
            }
        }

        public IEnumerable<List<string>> GetAllDuplicateItemUniqueIds()
        {
            const int pageSize = 100;
            var latestId = 0;
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            string selectQuery = @"
            SELECT i.Id, i.ItemUniqueId 
            FROM ItemUniqueIdsData i
            JOIN HashCodeData h ON i.HashCode = h.HashCode
            WHERE h.ItemCount > 1 AND i.Id > @LatestId
            ORDER BY i.Id
            LIMIT @PageSize OFFSET 0";
            while(true)
            {
                using (var selectCommand = new SQLiteCommand(selectQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@PageSize", pageSize);
                    selectCommand.Parameters.AddWithValue("@LatestId", latestId);
                    using var reader = selectCommand.ExecuteReader();
                    var res = new List<string>();
                    while (reader.Read())
                    {
                        latestId = Convert.ToInt32(reader["Id"]);
                        res.Add(reader["ItemUniqueId"].ToString());
                    }
                    yield return res;

                    if(res.Count < pageSize)
                    {
                        break;
                    }
                }
            }
        }

        public void Dispose()
        {
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }
    }
}