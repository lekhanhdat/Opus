using AvePoint.GCommon.Utility;
using System;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AvePoint.Item.Restore.ScanStaging
{
    internal sealed class OpusScanStagingDatabase : IDisposable
    {
        internal const string ScanResultsTable = "ScanResults";
        internal const string ScanContainersTable = "ScanContainers";

        private readonly string _directoryPath;
        private bool _disposed;

        internal OpusScanStagingDatabase(string archiveTempPath, string jobId)
        {
            if (string.IsNullOrWhiteSpace(archiveTempPath))
            {
                throw new ArgumentException("The archive temporary path is required.", nameof(archiveTempPath));
            }

            _directoryPath = SecurityUtils.SafeCombinePath(archiveTempPath, "OpusStubScan", Guid.CreateVersion7().ToString());
            Directory.CreateDirectory(_directoryPath);
            DatabasePath = SecurityUtils.SafeCombinePath(_directoryPath, "scan.db");
            Connection = CreateConnection(DatabasePath);
            try
            {
                Connection.Open();
                InitializeSchema(Connection);
            }
            catch
            {
                Connection.Dispose();
                TryDeleteDirectory();
                throw;
            }
        }

        internal string DatabasePath { get; }

        internal SQLiteConnection Connection { get; }

        internal long CountScanResults()
        {
            using SQLiteCommand command = Connection.CreateCommand();
            command.CommandText = "SELECT COUNT(1) FROM ScanResults;";
            return Convert.ToInt64(command.ExecuteScalar());
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Connection.Dispose();
            TryDeleteDirectory();
        }

        private static SQLiteConnection CreateConnection(string databasePath)
        {
            return new SQLiteConnection($"Data Source={databasePath};Version=3;foreign keys=True;");
        }

        private static void InitializeSchema(SQLiteConnection connection)
        {
            using SQLiteCommand command = connection.CreateCommand();
            command.CommandText = @"
                PRAGMA journal_mode = WAL;
                PRAGMA synchronous = NORMAL;
                PRAGMA busy_timeout = 30000;

                CREATE TABLE IF NOT EXISTS ScanContainers (
                    ContainerUrl TEXT PRIMARY KEY,
                    ParentUrl TEXT NULL,
                    SiteUrl TEXT NOT NULL,
                    WebUrl TEXT NOT NULL,
                    ContainerType INTEGER NOT NULL,
                    Name TEXT NOT NULL,
                    DisplayName TEXT NOT NULL,
                    FullPathForUI TEXT NULL
                );

                CREATE TABLE IF NOT EXISTS ScanResults (
                    RowId INTEGER PRIMARY KEY AUTOINCREMENT,
                    ItemId TEXT NOT NULL,
                    UniqueId TEXT NOT NULL,
                    SiteUrl TEXT NOT NULL,
                    WebUrl TEXT NOT NULL,
                    ListUrl TEXT NOT NULL,
                    ParentUrl TEXT NOT NULL,
                    FileUrl TEXT NOT NULL,
                    FileName TEXT NOT NULL,
                    Size BIGINT NOT NULL,
                    Extension TEXT NOT NULL,
                    UNIQUE (ListUrl, ItemId)
                );

                CREATE INDEX IF NOT EXISTS IX_ScanResults_Parent_RowId
                    ON ScanResults (ParentUrl, RowId);
                CREATE INDEX IF NOT EXISTS IX_ScanResults_List_RowId
                    ON ScanResults (ListUrl, RowId);
                CREATE INDEX IF NOT EXISTS IX_ScanResults_ItemId
                    ON ScanResults (ItemId);
                CREATE INDEX IF NOT EXISTS IX_ScanContainers_Parent
                    ON ScanContainers (ParentUrl, ContainerUrl);
                ";
            command.ExecuteNonQuery();
        }

        private void TryDeleteDirectory()
        {
            try
            {
                if (Directory.Exists(_directoryPath))
                {
                    Directory.Delete(_directoryPath, true);
                }
            }
            catch
            {
            }
        }
    }
}
