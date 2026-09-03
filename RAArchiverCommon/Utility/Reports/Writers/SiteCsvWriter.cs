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
using AvePoint.Application.StorageApiModern;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using Storage;
using System.Reflection;
using System.Text;

namespace RAArchiverCommon.Utility
{
    /// <summary>
    /// Accumulates entries for a single site and commits them
    /// as partitioned CSV blobs via the <see cref="IXSystem"/> storage API.
    /// The in-memory buffer is flushed to a local temp file every <see cref="BatchSize"/> records,
    /// and files are automatically rotated after <see cref="_maxRecordsPerFile"/> records.
    /// </summary>
    internal class SiteCsvWriter<T> where T : class
    {
        private readonly IRALogger _logger;

        private readonly string _site;
        private readonly string _jobId;
        private readonly string _folderPath;

        private readonly IXSystem _xSystem;

        private int _fileIndex = 1;
        private int _recordCount = 0;

        private readonly List<string> _buffer = [];
        private string? _localFilePath;

        private readonly int _maxRecordsPerFile;
        private readonly int _batchSize;

        private static readonly PropertyInfo[] Props = typeof(T).GetProperties();
        private static readonly string Header = string.Join(",", Props.Select(p => p.GetCustomAttribute<CsvColumnAttribute>()?.Name ?? p.Name));

        public SiteCsvWriter(IXSystem xSystem, string site, string jobId, string folderPath, int maxRecordsPerFile = 100_000, int batchSize = 2000)
        {
            _logger = RALogger.GetInstance(typeof(ChunkedCsvWriter<T>));

            _site = site;
            _jobId = jobId;
            _folderPath = folderPath;

            _xSystem = xSystem;

            _maxRecordsPerFile = maxRecordsPerFile;
            _batchSize = batchSize;

            InitNewFile();
        }

        public async Task WriteAsync(T record)
        {
            _buffer.Add(ToCsv(record));
            _recordCount++;
            try
            {
                if (_buffer.Count >= _batchSize)
                {
                    _logger.Info($"[SiteCsvWriter WriteAsync] Site: {_site}, JobId: {_jobId}, RecordCount: {_recordCount}, BufferCount: {_buffer.Count} - flushing to local file.");
                    await FlushAsync();
                }

                if (_recordCount >= _maxRecordsPerFile)
                {
                    _logger.Info($"[SiteCsvWriter WriteAsync] Site: {_site}, JobId: {_jobId}, RecordCount: {_recordCount} - rotating file.");
                    await RotateFileAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"[SiteCsvWriter WriteAsync Error] Site: {_site}, JobId: {_jobId}, RecordCount: {_recordCount}, Exception: {ex}");
            }
        }

        public async Task CompleteAsync()
        {
            _logger.Info($"[SiteCsvWriter CompleteAsync] Site: {_site}, JobId: {_jobId}, FinalRecordCount: {_recordCount} - flushing remaining buffer and committing file.");
            try
            {
                await FlushAsync();
                await CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.Error($"[SiteCsvWriter CompleteAsync Error] Site: {_site}, JobId: {_jobId}, Exception: {ex}");
            }
        }

        private void InitNewFile()
        {
            _buffer.Clear();
            _recordCount = 0;
            _localFilePath = null;
            _buffer.Add(Header);
        }

        private async Task RotateFileAsync()
        {
            _logger.Info($"[SiteCsvWriter RotateFileAsync] Site: {_site}, JobId: {_jobId}, Rotating file at RecordCount: {_recordCount}.");
            try
            {
                await FlushAsync();
                await CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.Error($"[SiteCsvWriter RotateFileAsync Error] Site: {_site}, JobId: {_jobId}, FileIndex: {_fileIndex}, Exception: {ex}");
            }
            _fileIndex++;
            InitNewFile();
        }

        private async Task FlushAsync()
        {
            if (_buffer.Count == 0) return;

            _localFilePath ??= SecurityUtils.SafeCombinePath(Path.GetTempPath(), $"{Guid.NewGuid():N}_{GetFileName()}");

            var content = string.Join("\n", _buffer) + "\n";
            await File.AppendAllTextAsync(_localFilePath, content, Encoding.UTF8);

            _buffer.Clear();
        }

        private async Task CommitAsync()
        {
            if (string.IsNullOrEmpty(_localFilePath) || !File.Exists(_localFilePath))
                return;
            try
            {
                using var fs = new FileStream(_localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var storageInfo = new StorageInfo
                {
                    HighName = _folderPath,
                    LowName = GetFileName(),
                    Length = fs.Length
                };

                await _xSystem.UploadAsyncExt(fs, storageInfo, true, default);
            }
            catch
            {
                throw;
            }
            finally
            {
                CleanupLocalFile();
            }
        }

        private void CleanupLocalFile()
        {
            if (!string.IsNullOrEmpty(_localFilePath) && File.Exists(_localFilePath))
            {
                File.Delete(_localFilePath);
            }

            _localFilePath = null;
        }

        private string GetFileName() => $"{_jobId}_part_{_fileIndex:D3}.csv";

        private string ToCsv(T obj)
        {
            var values = new string[Props.Length];
            for (int i = 0; i < Props.Length; i++)
            {
                var val = Props[i].GetValue(obj);
                values[i] = Esc(val?.ToString());
            }
            return string.Join(",", values);
        }

        private string Esc(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            if (input.Contains('"') || input.Contains(',') || input.Contains('\n'))
                return $"\"{input.Replace("\"", "\"\"")}\"";
            return input;
        }
    }
}