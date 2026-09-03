using AvePoint.GCommon.Utility;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.SQLiteDB.Reporting.DBContext
{
    public class RMReportSQLiteDBContext : IDisposable
    {
        private readonly SQLiteConnection _connection;

        private SQLiteCommand _preparedInsertCommand;
        private int _preparedColumnCount;

        public RMReportSQLiteDBContext(string dbPath)
        {
            _connection = new SQLiteConnection($"DataSource={dbPath};Version=3");
            _connection.Open();
        }

        public async Task EnsureDataTableAsync(string tableName, IReadOnlyList<string> columns)
        {
            var sanitizedTableName = SecurityUtils.SanitizeSQLSchemaName(tableName);
            var sanitizedColumns = MakeUniqueColumnNames(columns);

            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append($"CREATE TABLE IF NOT EXISTS {sanitizedTableName} (");
            for (var i = 0; i < sanitizedColumns.Count; i++)
            {
                sqlBuilder.Append($"{sanitizedColumns[i]} TEXT");
                if (i < sanitizedColumns.Count - 1)
                {
                    sqlBuilder.Append(", ");
                }
            }

            sqlBuilder.Append(");");

            using var command = _connection.CreateCommand();
            command.CommandText = sqlBuilder.ToString();
            await command.ExecuteNonQueryAsync();
        }

        public async Task CreateIndexAsync(string tableName, string columnName)
        {
            var table = SecurityUtils.SanitizeSQLSchemaName(tableName);
            var column = SanitizeIdentifier(columnName);
            var sql = $"CREATE INDEX IF NOT EXISTS IX_{table}_{column} ON {table}({column});";

            using var command = _connection.CreateCommand();
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();
        }

        public void PrepareInsertCommand(string tableName, IReadOnlyList<string> columns)
        {
            var table = SecurityUtils.SanitizeSQLSchemaName(tableName);
            var sanitizedColumns = MakeUniqueColumnNames(columns);
            var paramNames = Enumerable.Range(0, sanitizedColumns.Count).Select(i => "@c" + i).ToList();

            var columnsSql = string.Join(", ", sanitizedColumns);
            var valuesSql = string.Join(", ", paramNames);

            _preparedInsertCommand?.Dispose();
            _preparedInsertCommand = _connection.CreateCommand();
            _preparedInsertCommand.CommandText = $"INSERT INTO {table} ({columnsSql}) VALUES ({valuesSql});";

            foreach (var paramName in paramNames)
            {
                _preparedInsertCommand.Parameters.Add(new SQLiteParameter(paramName));
            }

            _preparedInsertCommand.Prepare();
            _preparedColumnCount = sanitizedColumns.Count;
        }

        public async Task InsertPreparedBatchAsync(IReadOnlyList<string[]> rows, int batchSize, CancellationToken cancellationToken)
        {
            if (_preparedInsertCommand == null)
            {
                throw new InvalidOperationException("Insert command has not been prepared. Call PrepareInsertCommand first.");
            }

            if (rows == null || rows.Count == 0)
            {
                return;
            }

            var parameters = _preparedInsertCommand.Parameters;

            for (var i = 0; i < rows.Count; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var end = Math.Min(i + batchSize, rows.Count);
                using var transaction = _connection.BeginTransaction();
                _preparedInsertCommand.Transaction = transaction;

                for (var j = i; j < end; j++)
                {
                    var row = rows[j];
                    var min = Math.Min(row.Length, _preparedColumnCount);
                    
                    for (var c = 0; c < min; c++)
                    {
                        parameters[c].Value = row[c] ?? string.Empty;
                    }

                    for (var c = min; c < _preparedColumnCount; c++)
                    {
                        parameters[c].Value = string.Empty;
                    }
                    
                    await _preparedInsertCommand.ExecuteNonQueryAsync();
                }

                transaction.Commit();
            }
        }

        public async Task EnsureExportedSitesTableAsync()
        {
            const string sql = @"
            CREATE TABLE IF NOT EXISTS ExportedSites
            (
                SiteCollectionId TEXT PRIMARY KEY,
                SiteCollectionUrl TEXT,
                ExportTime TEXT,
                RecordCount INTEGER
            );";

            using var command = _connection.CreateCommand();
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();
        }

        public async Task UpsertExportedSiteAsync(
            string siteCollectionId,
            string siteCollectionUrl,
            string tableName,
            DateTime exportTimeUtc,
            long recordCount)
        {
            const string sql = @"
            INSERT OR REPLACE INTO ExportedSites
            (
                SiteCollectionId,
                SiteCollectionUrl,
                ExportTime,
                RecordCount
            )
            VALUES
            (
                @SiteCollectionId,
                @SiteCollectionUrl,
                @ExportTime,
                @RecordCount
            );";

            using var command = _connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@SiteCollectionId", siteCollectionId ?? string.Empty);
            command.Parameters.AddWithValue("@SiteCollectionUrl", siteCollectionUrl ?? string.Empty);
            command.Parameters.AddWithValue("@ExportTime", exportTimeUtc.ToString("O"));
            command.Parameters.AddWithValue("@RecordCount", recordCount);
            await command.ExecuteNonQueryAsync();
        }

        private static List<string> MakeUniqueColumnNames(IReadOnlyList<string> columns)
        {
            var result = new List<string>(columns.Count);
            var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var col in columns)
            {
                var sanitized = SanitizeIdentifier(col);
                if (seen.TryGetValue(sanitized, out var count))
                {
                    count++;
                    seen[sanitized] = count;
                    sanitized = $"{sanitized}_{count}";
                }
                else
                {
                    seen[sanitized] = 0;
                }
                result.Add(sanitized);
            }
            return result;
        }

        private static string SanitizeIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "_";
            }

            var sanitized = new string(value.Where(ch => char.IsLetterOrDigit(ch) || ch == '_').ToArray());

            if (string.IsNullOrWhiteSpace(sanitized))
            {
                sanitized = "_";
            }

            if (char.IsDigit(sanitized[0]))
            {
                sanitized = "_" + sanitized;
            }

            return sanitized;
        }

        public void Dispose()
        {
            _preparedInsertCommand?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}