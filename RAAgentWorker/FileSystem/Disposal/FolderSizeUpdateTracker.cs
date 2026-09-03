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
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RAFileSystem.Disposal
{
    /// <summary>
    /// Thread-safe tracker that accumulates deleted file sizes per folder path,
    /// then propagates the size reduction up the folder hierarchy on flush.
    /// </summary>
    internal sealed class FolderSizeUpdateTracker
    {
        private static readonly AveLogger Logger = AveLogger.GetInstance(typeof(FolderSizeUpdateTracker));

        private readonly ConcurrentDictionary<string, long> _folderSizeDeltas =
            new ConcurrentDictionary<string, long>(StringComparer.OrdinalIgnoreCase);

        private readonly string _rootPath;
        private readonly int _flushThreshold;
        private readonly SemaphoreSlim _flushLock = new SemaphoreSlim(1, 1);
        private long _totalFlushedCount;
        private const int ApiBatchSize = 100;

        public FolderSizeUpdateTracker(string rootPath, int flushThreshold = 5000)
        {
            _rootPath = rootPath?.TrimEnd('\\') ?? string.Empty;
            _flushThreshold = flushThreshold;
        }

        /// <summary>
        /// Records a deleted file's size against its parent folder.
        /// Thread-safe: may be called concurrently from multiple worker threads.
        /// </summary>
        /// <param name="parentFolderRelativePath">
        /// The HighName of the deleted file (relative folder path under root).
        /// </param>
        /// <param name="fileSize">Size in bytes of the deleted file.</param>
        public async Task RecordDeletedFile(string parentFolderRelativePath, long fileSize)
        {
            if (fileSize <= 0)
            {
                return;
            }
            string folderKey = NormalizePath(parentFolderRelativePath);
            Logger.Debug("Recording deleted file of size {0} bytes for folder '{1}'.", fileSize, folderKey);
            _folderSizeDeltas.AddOrUpdate(folderKey, fileSize, (key, existing) => existing + fileSize);
            if (_folderSizeDeltas.Count >= _flushThreshold)
            {
                await FlushCurrentBatch().ConfigureAwait(false);
            }
        }

        public async Task FlushUpdates()
        {
            await FlushCurrentBatch().ConfigureAwait(false);

            Logger.Info(
                "Folder size tracking complete. Total folders flushed across all batches: {0}.",
                Interlocked.Read(ref _totalFlushedCount));
        }

        /// <summary>
        /// Drains the current dictionary contents into a local snapshot, 
        /// then sends the snapshot to the server. Uses a semaphore to 
        /// ensure only one flush operation runs at a time.
        /// </summary>
        private async Task FlushCurrentBatch()
        {
            if (_folderSizeDeltas.IsEmpty)
            {
                return;
            }

            // Ensure only one flush at a time. If another thread is already
            // flushing, this thread will wait rather than skip — we must not
            // lose data.
            await _flushLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_folderSizeDeltas.IsEmpty)
                {
                    return;
                }

                var snapshot = DrainDictionary();

                if (snapshot.Count == 0)
                {
                    return;
                }

                Logger.Info("Flushing batch of {0} folder size delta(s) to server.", snapshot.Count);
                await SendUpdatesInBatches(snapshot).ConfigureAwait(false);

                Interlocked.Add(ref _totalFlushedCount, snapshot.Count);
            }
            finally
            {
                _flushLock.Release();
            }
        }
        private Dictionary<string, long> DrainDictionary()
        {
            var snapshot = new Dictionary<string, long>(
                _folderSizeDeltas.Count, StringComparer.OrdinalIgnoreCase);

            foreach (var key in _folderSizeDeltas.Keys.ToArray())
            {
                if (_folderSizeDeltas.TryRemove(key, out long value))
                {
                    snapshot[key] = value;
                }
            }

            return snapshot;
        }
        private async Task SendUpdatesInBatches(Dictionary<string, long> folderDeltas)
        {
            var batch = new List<FolderSizeUpdateDto>(ApiBatchSize);

            foreach (var kv in folderDeltas)
            {
                if (kv.Value <= 0 || string.IsNullOrEmpty(kv.Key))
                {
                    continue;
                }

                // Build the DTO on-the-fly instead of materializing a full list
                batch.Add(new FolderSizeUpdateDto
                {
                    FolderPath = ExternalUtil.CombinePath(_rootPath, kv.Key),
                    FolderId = ExternalUtil.CombinePath(_rootPath, kv.Key)
                        .ToLowerInvariant().ToMd5(),
                    DeletedBytes = kv.Value,
                    RootFolderPath = _rootPath
                });

                if (batch.Count >= ApiBatchSize)
                {
                    await SendBatchAsync(batch).ConfigureAwait(false);
                    batch.Clear();
                }
            }
            if (batch.Count > 0)
            {
                await SendBatchAsync(batch).ConfigureAwait(false);
            }
        }

        private async Task SendBatchAsync(List<FolderSizeUpdateDto> batch)
        {
            try
            {
                using (new AgentPerformanceScope(
                    "DisposalWorker.UpdateFolderSizes",
                    $"DisposalWorker.UpdateFolderSizes.Count:{batch.Count}",
                    true))
                {
                    var success = await JobContext.Current.ApiClient
                        .UpdateFolderSizes(batch)
                        .ConfigureAwait(false);

                    if (success)
                    {
                        Logger.Info(
                            "Successfully sent folder size updates for {0} folder(s).",
                            batch.Count);
                    }
                    else
                    {
                        Logger.Warn(
                            "API returned failure for folder size batch of {0}.",
                            batch.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(
                    "Failed to send folder size batch of {0}. Error: {1}",
                    batch.Count, ex);
            }
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }
            return path.Trim('\\');
        }

    }
}