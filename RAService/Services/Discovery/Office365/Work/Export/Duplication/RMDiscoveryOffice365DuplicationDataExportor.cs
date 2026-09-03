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
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Export.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Export
{
    public class RMDiscoveryOffice365DuplicationDataExportor<T> : IDisposable
    {
        private readonly int MaxRecordsPerFile = 1_000_000;
        private readonly int FlushBatchSize = 1_000;
        private readonly string _baseFileName;
        private readonly string _folderPath;
        private readonly RMDiscoveryOffice365ReportHeaderDefinition _header;
        private readonly Func<T, IDictionary<string, object>> _rowMapper;
        private StreamWriter _writer;
        private bool _headerWritten;
        private int _currentRecordCount;
        private int _pendingWriteCount;
        private int _fileIndex;

        public RMDiscoveryOffice365DuplicationDataExportor(
            string folderPath,
            string baseFileName,
            RMDiscoveryOffice365ReportHeaderDefinition header,
            Func<T, IDictionary<string, object>> rowMapper)
        {
            _folderPath = folderPath;
            _baseFileName = baseFileName;
            _header = header;
            _rowMapper = rowMapper;
            OpenNewFile();
        }

        #region Public methods

        public void WriteData(T record) => WriteInternal(new[] { record });

        public void WriteData(IEnumerable<T> records)
        {
            if (records == null) return;

            const int batchSize = 100;
            var buffer = new List<T>(batchSize);

            foreach (var record in records)
            {
                if (record == null) continue;

                buffer.Add(record);

                if (buffer.Count == batchSize)
                {
                    WriteInternal(buffer);
                    buffer.Clear();
                }
            }

            if (buffer.Count > 0) 
            {
                WriteInternal(buffer);
            }
        }

        public void ForceExportWithHeaderOnly() => WriteHeaderIfNeeded();

        public void Dispose()
        {
            if (_writer != null)
            {
                _writer.Flush();
                _writer.Dispose();
                _writer = null;
            }
        }
        #endregion

        #region Limitation control

        private static void EnsureDirectory(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentException("Folder path is null or empty.", folderPath);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        private void EnsureRowCapacity(int incomingCount)
        {
            if (_currentRecordCount + incomingCount <= MaxRecordsPerFile) return;
            RotateFile();
        }

        private void RotateFile()
        {
            _writer.Flush();
            _writer.Dispose();
            _fileIndex++;
            OpenNewFile();
        }

        private void OpenNewFile()
        {
            EnsureDirectory(_folderPath);
            string destinationPath = GenerateFileName();
            _writer = new StreamWriter( new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.Read), Encoding.UTF8);
            _headerWritten = false;
            _currentRecordCount = 0;
        }

        private void EnsureReachedFlushBatch()
        {
            if (_pendingWriteCount >= FlushBatchSize)
            {
                _writer.Flush();
                _pendingWriteCount = 0;
            }
        }
        #endregion

        #region Internals

        private void WriteHeaderIfNeeded()
        {
            if (_headerWritten) return;
            _writer.WriteLine(string.Join(",", _header.OrderedColumns.Select(Escape)));
            _headerWritten = true;
        }

        private void WriteInternal(IReadOnlyList<T> records)
        {
            if (records == null || records.Count == 0) return;
            
            EnsureRowCapacity(records.Count);
            WriteHeaderIfNeeded();

            foreach (var record in records)
            {
                if (record == null) continue;
                var row = _rowMapper(record);
                var sb = new StringBuilder();

                for (int i = 0; i < _header.OrderedColumns.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    var col = _header.OrderedColumns[i];
                    row.TryGetValue(col, out var val);
                    sb.Append(Escape(ToInvariantString(val)));
                }

                _writer.WriteLine(sb.ToString());
                _currentRecordCount++;
                _pendingWriteCount++;
            }

            EnsureReachedFlushBatch();
        }

        private static string ToInvariantString(object value)
        {
            return value  switch
            {
                null => string.Empty,
                _ => Convert.ToString(value, CultureInfo.InvariantCulture)
            };
        }

        private static string Escape(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            if (input.Contains(",") || input.Contains("\"") || input.Contains("\n"))
                return $"\"{input.Replace("\"", "\"\"")}\"";

            return input;
        }

        private string GenerateFileName()
        {
            return Path.Combine(_folderPath, $"{_baseFileName}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}_{_fileIndex:D3}.csv");
        }
        #endregion
    }
}
