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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using Storage;
using System.Threading.Channels;

namespace RAArchiverCommon.Utility
{
    /// <summary>
    /// Generic, backpressure-aware writer that groups records of type <typeparamref name="T"/>
    /// by a caller-defined key and commits each group as partitioned CSV blobs via the
    /// <see cref="IXSystem"/> storage API.
    /// </summary>
    /// <typeparam name="T">
    /// The record type whose public properties are serialized as CSV columns.
    /// Properties decorated with <see cref="CsvColumnAttribute"/> use the attribute name as the header.
    /// </typeparam>
    /// <remarks>
    /// Records are enqueued via <see cref="EnqueueAsync"/> and processed on a background task.
    /// Each unique group key produces its own series of CSV files through a dedicated <see cref="SiteCsvWriter{T}"/>.
    /// Call <see cref="Complete"/> after all records have been enqueued to signal the end of input.
    /// </remarks>
    public class ChunkedCsvWriter<T> : IAsyncDisposable where T : class
    {
        private readonly IRALogger _logger;

        private readonly Channel<T> _channel;
        private readonly string _folderPath;
        private readonly Func<T, string> _groupKeySelector;
        private readonly Func<T, string> _jobIdSelector;
        private readonly int _channelCapacity;

        private readonly IXSystem _xSystem;
        
        private readonly Task _processingTask;

        private readonly int _maxRecordsPerFile;
        private readonly int _batchSize;

        public bool IsDisposed { get; private set; }

        public ChunkedCsvWriter(
            string xriString,
            string folderPath,
            Func<T, string> groupKeySelector,
            Func<T, string> jobIdSelector,
            int maxRecordsPerFile = 100_000, int batchSize = 2000,
            int channelCapacity = 10_000)
        {
            _logger = RALogger.GetInstance(typeof(ChunkedCsvWriter<T>));

            _folderPath = folderPath;
            _groupKeySelector = groupKeySelector;
            _jobIdSelector = jobIdSelector;
            _channelCapacity = channelCapacity;

            _xSystem = XFactory.InstanceSystem(xriString);

            _channel = Channel.CreateBounded<T>(new BoundedChannelOptions(_channelCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            });

            _maxRecordsPerFile = maxRecordsPerFile;
            _batchSize = batchSize;

            _processingTask = Task.Run(ProcessAsync);

            _logger.Info($"[ChunkedCsvWriter Initialized] FolderPath: {_folderPath}, ChannelCapacity: {_channelCapacity}, MaxRecordsPerFile: {_maxRecordsPerFile}, BatchSize: {_batchSize}");
        }

        public async Task EnqueueAsync(T record)
        {
            await _channel.Writer.WriteAsync(record);
        }

        public async Task CompleteAsync()
        {
            _channel.Writer.TryComplete();
            await _processingTask;
        }

        private async Task ProcessAsync()
        {
            var grouped = new Dictionary<string, SiteCsvWriter<T>>();

            await foreach (var record in _channel.Reader.ReadAllAsync())
            {
                var jobId = _jobIdSelector(record);

                if (!grouped.TryGetValue(jobId, out var writer))
                {
                    var site = SanitizeKey(_groupKeySelector(record));
                    writer = new SiteCsvWriter<T>(_xSystem, site, jobId, _folderPath, _maxRecordsPerFile, _batchSize);
                    grouped[jobId] = writer;
                }

                try
                {
                    await writer.WriteAsync(record);
                }
                catch (Exception ex)
                {
                    _logger.Error($"[ChunkedCsvWriter WriteAsync Error] Key: {jobId}, Exception: {ex}");
                }
            }

            foreach (var writer in grouped.Values)
            {
                try
                {
                    await writer.CompleteAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error($"[ChunkedCsvWriter CompleteAsync Error] Exception: {ex}");
                }
            }
        }

        private static string SanitizeKey(string key)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            var sep = Path.DirectorySeparatorChar;
            foreach (char c in invalidChars)
            {
                if (c != sep)
                {
                    key = key.Replace(c, '_');
                }
            }
            return key;
        }

        public async ValueTask DisposeAsync()
        {
            if (IsDisposed) return;

            _xSystem.Dispose();

            IsDisposed = true;
        }
    }
}