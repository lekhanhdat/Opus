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
using AvePoint.GCommon.Utility.PerformanceScope;
using AvePoint.RA.Contract.Tenant;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.DataIngestion
{
    public class RMDataIngestionPersistor
    {
        private readonly AveLogger _logger = AveLogger.GetInstance(typeof(RMDataIngestionPersistor));
        private readonly string _jobId;
        private const string FileExtension = ".bin";
        private const string FileNameFormat = "Persist_{0}_{1}" + FileExtension; //Persist_{JobId}_{MessageId}.bin
        private const int MaxRecordsPerFile = 10_000;
        private const int FlushIntervalRecords = 1000;
        private const int StreamBufferSize = 128 * 1024;
        private int _totalRecordsInFile = 0;
        private int _sinceLastFlushRecords = 0;
        private string _baseFolderPath;
        private string _currentTempPath;
        private string _currentMessageId;
        private FileStream _writeStream;
        private readonly object _writeLock = new object();

        public RMDataIngestionPersistor(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId)) throw new ArgumentException("JobId must be provided.");
            _jobId = jobId;
            Initialize();
        }

        private void Initialize()
        {
            _baseFolderPath = Path.Combine(Path.GetTempPath(), "AvePoint", $"DataIngestion_{Sanitize(TenantAgentInfo.AgentId)}");
            _currentTempPath = GenerateFilePath(true);
            EnsureDirectoryExists(_currentTempPath);
            OpenWriteStream();
        }

        public void SetCurrentMessageId(string messageId)
        {
            _currentMessageId = messageId;
            _logger.Info($"Set current message id to {_currentMessageId}");
        }

        #region Write

        public void WriteData<T>(T record)
        {
            lock (_writeLock)
            {
                using (new AgentPerformanceScope("Persistor.WriteRecordInternal", addToStatistics: true))
                {
                    if (record == null) return;
                    try
                    {
                        if (_totalRecordsInFile >= MaxRecordsPerFile)
                        {
                            _logger.Info("Max records per file reached. Rotating file.");
                            RotateFile();
                        }

                        Serializer.SerializeWithLengthPrefix(_writeStream, record, PrefixStyle.Base128, 1);
                        _totalRecordsInFile++;
                        _sinceLastFlushRecords++;

                        if (_sinceLastFlushRecords >= FlushIntervalRecords)
                        {
                            _writeStream.Flush(true);
                            _sinceLastFlushRecords = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to write record to stream: {ex}");
                    }
                }
            }
        }

        #endregion

        #region Read

        public async Task ReadAsync<T>(string messageId, Func<T, Task> onPersistCallback, bool deleteAfterRead = true)
        {
            using (new AgentPerformanceScope("Persistor.ReadAsync", addToStatistics: true))
            {
                if (onPersistCallback == null)
                    throw new ArgumentNullException(nameof(onPersistCallback));

                if (!Directory.Exists(_baseFolderPath))
                    return;

                var filePath = Path.Combine(_baseFolderPath, string.Format(FileNameFormat, Sanitize(_jobId), Sanitize(messageId)));

                if (!File.Exists(filePath))
                {
                    _logger.Warn($"File not found for messageId: {messageId}");
                    return;
                }

                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, StreamBufferSize))
                {
                    while (true)
                    {
                        T record;
                        try
                        {
                            record = Serializer.DeserializeWithLengthPrefix<T>(stream, PrefixStyle.Base128, 1);
                            if (record == null) break;
                        }
                        catch (EndOfStreamException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"Failed to read record: {ex}");
                            break;
                        }
                        await onPersistCallback(record).ConfigureAwait(false);
                    }
                }
                DropFiles(filePath);
            }
        }

        #endregion

        #region Delete

        private void DropFiles(params string[] paths)
        {
            if (paths == null || paths.Length == 0) return;
            try
            {
                foreach (var path in paths)
                {
                    if (File.Exists(path)) File.Delete(path);
                    _logger.Info("Successfully deleted local file.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to delete file. Exception: {ex}");
            }
        }

        #endregion

        #region Helpers

        private void OpenWriteStream()
        {
            _writeStream = new FileStream(_currentTempPath, FileMode.Append, FileAccess.Write, FileShare.Read, StreamBufferSize);
        }

        private void EnsureDirectoryExists(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        private string Sanitize(string input)
        {
            input = input.Replace("-", "");
            foreach (char c in Path.GetInvalidFileNameChars())
                input = input.Replace(c, '_');
            return input;
        }

        private void RotateFile()
        {
            Commit();
            _currentTempPath = GenerateFilePath(true);
            EnsureDirectoryExists(_currentTempPath);
            OpenWriteStream();
            _totalRecordsInFile = 0;
            _logger.Info($"Rotated to new file.");
        }

        private string GenerateFilePath(bool isTemp = false)
        {
            string fileName = "";
            if (isTemp)
            {
                fileName = string.Format(FileNameFormat, Sanitize(_jobId), DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
            }
            else
            {
                fileName = string.Format(FileNameFormat, Sanitize(_jobId), Sanitize(_currentMessageId));
            }
            return Path.Combine(_baseFolderPath, fileName);
        }
        #endregion

        public void Commit()
        {
            using (new AgentPerformanceScope("Persistor.Commit", addToStatistics: true))
            {
                try
                {
                    if (_writeStream == null) return;
                    _writeStream.Flush();
                    _writeStream.Dispose();

                    #region ATOMIC rename temp file to destination file
                    if (!string.IsNullOrWhiteSpace(_currentMessageId))
                    {
                        var destinationPath = GenerateFilePath(isTemp: false);
                        if (File.Exists(destinationPath))
                        {
                            File.Delete(destinationPath);
                        }
                        File.Move(_currentTempPath, destinationPath);
                        DropFiles(_currentTempPath);
                        _logger.Info($"Committed file for messageId {_currentMessageId}"); return;
                    }
                    else
                    {
                        _logger.Warn($"MessageId is null or empty. Skipping file commit.");
                        DropFiles(_currentTempPath);
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to commit file for messageId {_currentMessageId}. Exception: {ex}");
                }
                finally
                {
                    _writeStream = null;
                    _logger.Info($"Cleaned up write stream after commit.");
                }
            }
        }
    }
}
