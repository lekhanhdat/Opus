using AvePoint.RA.DB.SQLiteDB.Reporting.DBContext;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.SQLiteDB.Reporting.Runtime
{
    public class RMSharePointSiteMetricsReportSQLiteWriter : IDisposable
    {
        private const int DefaultBatchSize = 2000;
        private const string SharedTableName = "SiteMetrics";

        private readonly RMReportSQLiteDBContext _context;
        private readonly int _batchSize;
        private bool _initialized;

        public RMSharePointSiteMetricsReportSQLiteWriter(string dbPath, int batchSize = DefaultBatchSize)
        {
            _context = new RMReportSQLiteDBContext(dbPath);
            _batchSize = batchSize;
        }

        public async Task InitializeAsync(IReadOnlyList<string> exportHeaders)
        {
            if (_initialized) return;
            await _context.EnsureDataTableAsync(SharedTableName, exportHeaders);
            _context.PrepareInsertCommand(SharedTableName, exportHeaders);
            _initialized = true;
        }

        public async Task WriteAsync(IReadOnlyList<string[]> rows, CancellationToken cancellationToken)
        {
            if (!_initialized)
                throw new InvalidOperationException("Writer is not initialized. Call InitializeAsync first.");

            if (rows == null || rows.Count == 0) return;

            await _context.InsertPreparedBatchAsync(rows, _batchSize, cancellationToken);
        }

        public async Task RecordSiteMetadataAsync(string siteCollectionId, string siteCollectionUrl, long recordCount)
        {
            await _context.UpsertExportedSiteAsync(
                siteCollectionId,
                siteCollectionUrl,
                SharedTableName,
                DateTime.UtcNow,
                recordCount);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}