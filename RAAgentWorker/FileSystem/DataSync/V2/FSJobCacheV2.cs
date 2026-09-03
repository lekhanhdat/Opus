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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AvePoint.GCommon;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;

namespace AvePoint.RA.FileSystem.DataSync.V2
{
    /// <summary>
    /// Thread-safe cache for V2 multi-threaded workers.
    /// Provides efficient lock-free lookup with ConcurrentDictionary and thread-safe operations.
    /// Supports refresh from FSJobCache for dynamic data synchronization.
    /// All collections use lock-free concurrent data structures for maximum performance in high-concurrency scenarios.
    /// </summary>
    public class FSJobCacheV2 : SingletonBase<FSJobCacheV2>
    {
        private static readonly AveLogger _logger = AveLogger.GetInstance(typeof(FSJobCacheV2));

        #region Thread-Safe Collections for Fast Lookup

        // ConcurrentDictionary to simulate HashSet for O(1) lookup - Lock-free for maximum performance
        // Using byte as value since we only need the key
        private ConcurrentDictionary<Guid, byte> _lastJobFailedItemIds;
        private ConcurrentDictionary<string, byte> _runningJobNodeUrls;

        // ConcurrentDictionary for thread-safe read/write with fast lookup
        private ConcurrentDictionary<Guid, FSSettingDto> _scopeSettingCache;

        #endregion

        #region Thread-Safe Counters

        private int _failedCount;
        private int _successCount;

        /// <summary>
        /// Thread-safe read for FailedCount.
        /// </summary>
        public int FailedCount => _failedCount;

        /// <summary>
        /// Thread-safe read for SuccessCount.
        /// </summary>
        public int SuccessCount => _successCount;

        #endregion

        #region Singleton Pattern

        /// <summary>
        /// Protected constructor for singleton pattern.
        /// Initializes all lock-free concurrent collections from FSJobCache.
        /// </summary>
        public FSJobCacheV2()
        {
            RefreshFromFSJobCache();
            _logger.Info("FSJobCacheV2 initialized with lock-free concurrent collections");
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static FSJobCacheV2 Instance => GetInstance();

        #endregion

        #region Refresh Functionality

        /// <summary>
        /// Refreshes all data from FSJobCache.
        /// Call this method to synchronize with the source cache.
        /// Thread-safe operation - completely lock-free.
        /// </summary>
        public void RefreshFromFSJobCache()
        {
            _logger.Info("Refreshing FSJobCacheV2 from FSJobCache");

            try
            {
                // Refresh LastJobFailedItemIds - Convert List to ConcurrentDictionary for lock-free O(1) lookup
                var sourceList = FSJobCache.Instance.LastJobFailedItemIds;
                _lastJobFailedItemIds = sourceList != null 
                    ? new ConcurrentDictionary<Guid, byte>(sourceList.ToDictionary(id => id, id => (byte)0))
                    : new ConcurrentDictionary<Guid, byte>();
                _logger.Info($"Refreshed LastJobFailedItemIds: {_lastJobFailedItemIds.Count} items");

                // Refresh RunningJobNodeUrls - Convert List to ConcurrentDictionary with case-insensitive comparer
                var sourceUrlList = FSJobCache.Instance.RunningJobNodeUrls;
                _runningJobNodeUrls = sourceUrlList != null 
                    ? new ConcurrentDictionary<string, byte>(
                        sourceUrlList.ToDictionary(url => url, url => (byte)0), 
                        StringComparer.OrdinalIgnoreCase)
                    : new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
                _logger.Info($"Refreshed RunningJobNodeUrls: {_runningJobNodeUrls.Count} items");

                // Refresh ScopeSettingCache - Convert Dictionary to ConcurrentDictionary
                var sourceDict = FSJobCache.Instance.ScopeSettingCache;
                _scopeSettingCache = sourceDict != null 
                    ? new ConcurrentDictionary<Guid, FSSettingDto>(sourceDict) 
                    : new ConcurrentDictionary<Guid, FSSettingDto>();
                _logger.Info($"Refreshed ScopeSettingCache: {_scopeSettingCache.Count} items");

                // Refresh counters
                _failedCount = FSJobCache.Instance.FailedCount;
                _successCount = FSJobCache.Instance.SuccessCount;
                _logger.Info($"Refreshed counters - FailedCount: {_failedCount}, SuccessCount: {_successCount}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error refreshing FSJobCacheV2: {ex}");
                throw;
            }
        }

        #endregion

        #region Thread-Safe Properties and Methods

        /// <summary>
        /// Thread-safe lock-free lookup for LastJobFailedItemIds.
        /// O(1) complexity using ConcurrentDictionary - no locking overhead.
        /// Usage: FSJobCacheV2.Instance.LastJobFailedItemIdsContains(id)
        /// </summary>
        public bool LastJobFailedItemIdsContains(Guid id)
        {
            return _lastJobFailedItemIds.ContainsKey(id);
        }

        /// <summary>
        /// Thread-safe lock-free lookup for RunningJobNodeUrls.
        /// O(1) complexity using ConcurrentDictionary with case-insensitive comparison - no locking overhead.
        /// Usage: FSJobCacheV2.Instance.RunningJobNodeUrlsContains(url)
        /// </summary>
        public bool RunningJobNodeUrlsContains(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            return _runningJobNodeUrls.ContainsKey(url);
        }

        /// <summary>
        /// Thread-safe access to ScopeSettingCache.
        /// O(1) lookup using ConcurrentDictionary.
        /// Usage: FSJobCacheV2.Instance.ScopeSettingCache[id]
        /// </summary>
        public ConcurrentDictionary<Guid, FSSettingDto> ScopeSettingCache => _scopeSettingCache;

        /// <summary>
        /// Thread-safe atomic increment for FailedCount.
        /// Usage: FSJobCacheV2.Instance.IncrementFailedCount()
        /// </summary>
        public void IncrementFailedCount()
        {
            Interlocked.Increment(ref _failedCount);
        }

        /// <summary>
        /// Thread-safe atomic increment for SuccessCount.
        /// Usage: FSJobCacheV2.Instance.IncrementSuccessCount()
        /// </summary>
        public void IncrementSuccessCount()
        {
            Interlocked.Increment(ref _successCount);
        }

        /// <summary>
        /// Thread-safe atomic decrement for FailedCount.
        /// Usage: FSJobCacheV2.Instance.DecrementFailedCount()
        /// </summary>
        public void DecrementFailedCount()
        {
            Interlocked.Decrement(ref _failedCount);
        }

        /// <summary>
        /// Thread-safe atomic decrement for SuccessCount.
        /// Usage: FSJobCacheV2.Instance.DecrementSuccessCount()
        /// </summary>
        public void DecrementSuccessCount()
        {
            Interlocked.Decrement(ref _successCount);
        }

        #endregion

        #region Synchronization Back to FSJobCache

        /// <summary>
        /// Synchronizes counters back to FSJobCache.
        /// Call this method before job completion for final reporting.
        /// Thread-safe operation.
        /// </summary>
        public void SyncBackToFSJobCache()
        {
            try
            {
                FSJobCache.Instance.FailedCount = _failedCount;
                FSJobCache.Instance.SuccessCount = _successCount;
                _logger.Info($"Synced counters back to FSJobCache - FailedCount: {_failedCount}, SuccessCount: {_successCount}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error syncing back to FSJobCache: {ex}");
                throw;
            }
        }

        #endregion

        

        #region Dispose

        /// <summary>
        /// Releases resources used by FSJobCacheV2.
        /// </summary>
        public void Dispose()
        {
            // No resources to dispose - ConcurrentDictionary is lock-free
            _logger.Info("FSJobCacheV2 disposed");
        }

        #endregion
    }
}
