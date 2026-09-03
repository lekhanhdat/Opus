using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace AvePoint.RA.SharePoint.Archiver
{
    internal sealed class ApprovedDataSqliteRecord
    {
        public Guid SiteId { get; set; }
        public Guid WebId { get; set; }
        public Guid ListId { get; set; }
        public Guid ItemId { get; set; }
        public Guid TermId { get; set; }
        public Guid RuleId { get; set; }
        public int Status { get; set; }
        public int NodeLevel { get; set; }
    }
    public enum ProcessedStatus
    {
        None = 0,
        Success = 1,
        Failed = 2
    }
    internal static class ApprovedDatasSqliteHelper
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(ApprovedDatasSqliteHelper));
        private static readonly object mDbPathLock = new object();
        private const string mApprovedDataTableName = "ApprovedDatas";
        private const int mSqliteInsertBatchSize = 500;
        private static string mApprovedDataDbFilePath = null;

        public static void SaveApprovedDatasToSqlite(Guid siteId, List<DB.Explorer.Model.Record> approvedRecords, string archiveTemp, string jobId)
        {
            if (string.IsNullOrWhiteSpace(archiveTemp))
            {
                mLog.Warn($"Skip saving approved data to SQLite because approved data DB path is empty. SiteId:{siteId}.");
                return;
            }

            Directory.CreateDirectory(archiveTemp);
            var dbFilePath = GenerateApprovedDataDbFilePath(archiveTemp, jobId);
            if (!SecurityUtils.ValidateSQLiteConnectionWithBuilder(dbFilePath, out var builder))
            {
                mLog.Warn($"Skip saving approved data to SQLite because approved data DB connection is invalid. SiteId:{siteId}, DBPath:{dbFilePath}.");
                return;
            }

            using (var connection = new SQLiteConnection(builder.ConnectionString))
            {
                connection.Open();
                CreateApprovedDatasTable(connection);
                ClearApprovedDatasBySiteId(connection, siteId);
                InsertApprovedDatas(connection, siteId, approvedRecords ?? new());
            }

            SetApprovedDataDbFilePath(dbFilePath);
        }

        public static void DeleteApprovedDataDbFile()
        {
            var dbFilePath = GetApprovedDataDbFilePath();
            try
            {
                if (string.IsNullOrWhiteSpace(dbFilePath))
                {
                    return;
                }

                DeleteFileIfExists(dbFilePath);
                DeleteFileIfExists(dbFilePath + "-journal");
                DeleteFileIfExists(dbFilePath + "-wal");
                DeleteFileIfExists(dbFilePath + "-shm");
                SetApprovedDataDbFilePath(null);
            }
            catch (Exception e)
            {
                mLog.Error($"DeleteApprovedDataDbFile data SQLite DB failed. DBPath:{dbFilePath}.error:{e}");
            }
        }

        public static int UpdateStatus(Guid itemId, int status)
        {
            if (WrapperConfiguration.IsProcessApprovalDatasOnly)
            {
                if (!TryCreateConnection(out var connection))
                {
                    return 0;
                }

                var query = $@"UPDATE {mApprovedDataTableName}
                SET status = @Status
                WHERE itemid = @ItemId";

                using (connection)
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.Add("@Status", System.Data.DbType.Int32).Value = status;
                    command.Parameters.Add("@ItemId", System.Data.DbType.Guid).Value = itemId;
                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
            else
            {
                return 0; 
            }
        }

        public static List<ApprovedDataSqliteRecord> GetByItemId(Guid itemId)
        {
            List<ApprovedDataSqliteRecord> records = new List<ApprovedDataSqliteRecord>();
            if (!TryCreateConnection(out var connection))
            {
                return records;
            }

            var query = $@"SELECT siteid, webid, listid, itemid, termid, ruleid, status, NodeLevel
                FROM {mApprovedDataTableName}
                WHERE itemid = @ItemId";

            using (connection)
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.Add("@ItemId", System.Data.DbType.Guid).Value = itemId;
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        records.Add(ReadApprovedDataRecord(reader));
                    }
                }
            }

            return records;
        }

        public static List<int> GetProcessedNodeLevels()
        {
            List<int> nodeLevels = new List<int>();
            if (!TryCreateConnection(out var connection))
            {
                return nodeLevels;
            }

            var query = $@"SELECT DISTINCT NodeLevel
                FROM {mApprovedDataTableName}
                WHERE status <> @Status";

            using (connection)
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.Add("@Status", System.Data.DbType.Int32).Value = (int)ProcessedStatus.None;
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        nodeLevels.Add(Convert.ToInt32(reader["NodeLevel"]));
                    }
                }
            }

            return nodeLevels;
        }

        public static List<ApprovedDataSqliteRecord> GetPendingRecordsByNodeLevels(IEnumerable<int> nodeLevels)
        {
            List<ApprovedDataSqliteRecord> records = new List<ApprovedDataSqliteRecord>();
            var nodeLevelList = nodeLevels?.Distinct().ToList() ?? new List<int>();
            if (!TryCreateConnection(out var connection))
            {
                return records;
            }

            var nodeLevelParameters = nodeLevelList.Select((_, index) => $"@NodeLevel{index}").ToList();
            var nodeLevelCondition = nodeLevelList.Count > 0 ? $" AND NodeLevel IN ({string.Join(",", nodeLevelParameters)})" : string.Empty;
            var query = $@"SELECT siteid, itemid, NodeLevel
                FROM {mApprovedDataTableName}
                WHERE status = @Status{nodeLevelCondition}";

            using (connection)
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.Add("@Status", System.Data.DbType.Int32).Value = (int)ProcessedStatus.None;
                for (int index = 0; index < nodeLevelList.Count; index++)
                {
                    command.Parameters.Add(nodeLevelParameters[index], System.Data.DbType.Int32).Value = nodeLevelList[index];
                }

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        records.Add(new ApprovedDataSqliteRecord
                        {
                            SiteId = ReadGuid(reader, "siteid"),
                            ItemId = ReadGuid(reader, "itemid"),
                            NodeLevel = Convert.ToInt32(reader["NodeLevel"]),
                        });
                    }
                }
            }

            return records;
        }

        private static void SetApprovedDataDbFilePath(string dbFilePath)
        {
            lock (mDbPathLock)
            {
                mApprovedDataDbFilePath = dbFilePath;
            }
        }

        private static string GetApprovedDataDbFilePath()
        {
            lock (mDbPathLock)
            {
                return mApprovedDataDbFilePath;
            }
        }

        private static string GenerateApprovedDataDbFilePath(string archiveTemp, string jobId)
        {
            var validJobId = string.IsNullOrWhiteSpace(jobId) ? Guid.NewGuid().ToString() : jobId;
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            var dbName = $"approveddatas.{validJobId}.{timestamp}.db";
            return SecurityUtils.SafeCombinePath(archiveTemp, dbName);
        }

        private static void CreateApprovedDatasTable(SQLiteConnection connection)
        {
            var query = $@"CREATE TABLE IF NOT EXISTS {mApprovedDataTableName}(
                [siteid] [nvarchar](50) NOT NULL,
                [webid] [uniqueidentifier] NOT NULL,
                [listid] [uniqueidentifier] NOT NULL,
                [itemid] [uniqueidentifier] NOT NULL,
                [termid] [uniqueidentifier] NOT NULL,
                [ruleid] [uniqueidentifier] NOT NULL,
                [status] [int] NOT NULL,
                [NodeLevel] [int] NOT NULL
            );
            CREATE INDEX IF NOT EXISTS ApprovedDatasSiteIndex ON {mApprovedDataTableName}(siteid);
            CREATE INDEX IF NOT EXISTS ApprovedDatasListIndex ON {mApprovedDataTableName}(listid);
            CREATE INDEX IF NOT EXISTS ApprovedDatasRuleIndex ON {mApprovedDataTableName}(ruleid);
            CREATE INDEX IF NOT EXISTS ApprovedDatasStatusNodeLevelIndex ON {mApprovedDataTableName}(status, NodeLevel);";

            using (var command = new SQLiteCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void ClearApprovedDatasBySiteId(SQLiteConnection connection, Guid siteId)
        {
            var query = $"DELETE FROM {mApprovedDataTableName} WHERE siteid = @SiteId";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@SiteId", siteId.ToString());
                command.ExecuteNonQuery();
            }
        }

        private static void InsertApprovedDatas(SQLiteConnection connection, Guid siteId, List<DB.Explorer.Model.Record> approvedRecords)
        {
            if (approvedRecords == null || approvedRecords.Count == 0)
            {
                return;
            }

            for (int index = 0; index < approvedRecords.Count; index += mSqliteInsertBatchSize)
            {
                InsertApprovedDatasBatch(connection, siteId, approvedRecords.Skip(index).Take(mSqliteInsertBatchSize));
            }
        }

        private static void InsertApprovedDatasBatch(SQLiteConnection connection, Guid siteId, IEnumerable<DB.Explorer.Model.Record> approvedRecords)
        {
            var query = $@"INSERT INTO {mApprovedDataTableName}(siteid, webid, listid, itemid, termid, ruleid, status, NodeLevel)
                VALUES (@SiteId, @WebId, @ListId, @ItemId, @TermId, @RuleId, @Status, @NodeLevel)";

            using (var transaction = connection.BeginTransaction())
            using (var command = new SQLiteCommand(query, connection, transaction))
            {
                var siteIdParameter = command.Parameters.Add("@SiteId", System.Data.DbType.String);
                var webIdParameter = command.Parameters.Add("@WebId", System.Data.DbType.Guid);
                var listIdParameter = command.Parameters.Add("@ListId", System.Data.DbType.Guid);
                var itemIdParameter = command.Parameters.Add("@ItemId", System.Data.DbType.Guid);
                var termIdParameter = command.Parameters.Add("@TermId", System.Data.DbType.Guid);
                var ruleIdParameter = command.Parameters.Add("@RuleId", System.Data.DbType.Guid);
                var statusParameter = command.Parameters.Add("@Status", System.Data.DbType.Int32);
                var nodeLevelParameter = command.Parameters.Add("@NodeLevel", System.Data.DbType.Int32);

                foreach (var record in approvedRecords)
                {
                    if (record.ItemId == Guid.Empty)
                    {
                        mLog.Warn($"Skip approved data with invalid item id. SiteId:{siteId}, ListId:{record.ListId}, ItemId:{record.ItemId}.");
                        continue;
                    }

                    siteIdParameter.Value = siteId.ToString();
                    webIdParameter.Value = record.WebId;
                    listIdParameter.Value = record.ListId;
                    itemIdParameter.Value = record.ItemId;
                    termIdParameter.Value = record.TermId;
                    ruleIdParameter.Value = record.RuleId;
                    statusParameter.Value = (int)ProcessedStatus.None;
                    nodeLevelParameter.Value = record.NodeType;
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        private static bool TryCreateConnection(out SQLiteConnection connection)
        {
            connection = null;
            var dbFilePath = GetApprovedDataDbFilePath();
            if (string.IsNullOrWhiteSpace(dbFilePath) || !File.Exists(dbFilePath))
            {
                mLog.Warn($"Approved data SQLite DB does not exist. DBPath:{dbFilePath}.");
                return false;
            }

            if (!SecurityUtils.ValidateSQLiteConnectionWithBuilder(dbFilePath, out var builder))
            {
                mLog.Warn($"Approved data SQLite DB connection is invalid. DBPath:{dbFilePath}.");
                return false;
            }

            connection = new SQLiteConnection(builder.ConnectionString);
            return true;
        }

        private static ApprovedDataSqliteRecord ReadApprovedDataRecord(SQLiteDataReader reader)
        {
            return new ApprovedDataSqliteRecord
            {
                SiteId = ReadGuid(reader, "siteid"),
                WebId = ReadGuid(reader, "webid"),
                ListId = ReadGuid(reader, "listid"),
                ItemId = ReadGuid(reader, "itemid"),
                TermId = ReadGuid(reader, "termid"),
                RuleId = ReadGuid(reader, "ruleid"),
                Status = Convert.ToInt32(reader["status"]),
                NodeLevel = Convert.ToInt32(reader["NodeLevel"]),
            };
        }

        private static Guid ReadGuid(SQLiteDataReader reader, string columnName)
        {
            return Guid.TryParse(reader[columnName]?.ToString(), out var value) ? value : Guid.Empty;
        }

        private static void DeleteFileIfExists(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"Delete approved data SQLite file failed. FilePath:{filePath}, Error:{ex}.");
            }
        }
    }
}