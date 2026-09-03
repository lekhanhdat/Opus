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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Utility
{
    public class RAWebLocalCacheReleaser
    {
        //private static readonly IAveLogger _logger = AveLogger.GetInstance(typeof(RAWebLocalCacheReleaser));
        private static  LocalCacheReleaseManagement _releaser;

        public static void Configure(long maxCacheLimit, TimeSpan cacheProtectedPeriod, TimeSpan timerInterval)
        {
            if (_releaser == null)
            {
                _releaser = new LocalCacheReleaseManagement(maxCacheLimit, cacheProtectedPeriod, timerInterval);
            }
        }

        public static void RecordCacheFile(string filePath)
        {
            _releaser?.Record(filePath);
        }


        private class LocalCacheReleaseManagement
        {
            private IAveLogger _logger = AveLogger.GetInstance(typeof(LocalCacheReleaseManagement));
            private ConcurrentDictionary<string, long> _cacheFiles = new(); // value: last access time

            private PeriodicTimer _timer;
            private TimeSpan _timerInterval;
            private long _maxCacheLimit;
            private TimeSpan _cacheProtectedPeriod;

            /// <param name="maxCacheLimit">最大的 cache 总 size，单位 Byte，cache file总 size 超过后需要开始 release cache</param>
            /// <param name="cacheProtectedPeriod">需要传入正值，只有保护期时间以内，未被访问的Cache File，才允许被Release; 比如1小时没被访问过的，才允许删除</param>
            /// <param name="timerInterval">内部 timer执行 release cache 操作的周期</param>
            internal LocalCacheReleaseManagement(long maxCacheLimit, TimeSpan cacheProtectedPeriod, TimeSpan timerInterval)
            {
                _logger.Info($"LocalCacheReleaseManagement maxCacheLimit: {maxCacheLimit}, protectedPeriod: {cacheProtectedPeriod.TotalSeconds}, interval: {timerInterval.TotalSeconds}");
                _maxCacheLimit = maxCacheLimit;
                _timerInterval = timerInterval;
                _cacheProtectedPeriod = cacheProtectedPeriod;
            }


            public void Record(string filePath)
            {
                try
                {
                    if (string.IsNullOrEmpty(filePath))
                    {
                        return;
                    }
                    if (!File.Exists(filePath))
                    {
                        _logger.Error($"The cache file not exists. {filePath}");
                    }

                    lock (_cacheFiles)
                    {
                        _cacheFiles[filePath] = DateTime.UtcNow.Ticks;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Manage cache file failed. {ex}");
                }

                StartAutoManagement();
            }

            private bool StartAutoManagement()
            {
                if (_timer != null)
                {
                    return false;
                }

                lock (this)
                {
                    if (_timer == null)
                    {
                        _timer = new PeriodicTimer(_timerInterval);
                        _ = ExecuteTaskAsync();
                        _logger.Warn($"start LocalCacheReleaseManagement");
                        return true;
                    }
                }

                return false;
            }

            private async Task ExecuteTaskAsync()
            {
                try
                {
                    while (_timer != null && await _timer.WaitForNextTickAsync())
                    {
                        try
                        {
                            ReleaseCache();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"Error occurred while releasing cache. {ex}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error occurred while running LocalCacheReleaseManagement. {ex}");
                }
            }

            private void ReleaseCache()
            {
                List<KeyValuePair<string, long>> managedFiles = null;
                lock (_cacheFiles)
                {
                    managedFiles = _cacheFiles.OrderByDescending(i => i.Value).ToList();
                }

                int manageLength = managedFiles.Count;
                string filePath = null;
                long accessTime = 0;
                bool isOverLimitSize = false;
                long totalSize = 0;
                if (manageLength > 0)
                {
                    long releasePoint = DateTime.UtcNow.Add(-_cacheProtectedPeriod).Ticks;
                    _logger.Info($"Managed cache files: {manageLength}");
                    for (int i = 0; i < manageLength; i++)
                    {
                        (filePath, accessTime) = managedFiles[i];

                        if (!isOverLimitSize)
                        {
                            var file = new FileInfo(filePath);
                            if (file.Exists)
                            {
                                totalSize += file.Length;
                                if (totalSize > _maxCacheLimit)
                                {
                                    isOverLimitSize = true;
                                    _logger.Info($"Cache files were over the max cache size: {_maxCacheLimit}");
                                }
                            }
                            else
                            {
                                _logger.Warn($"Cache file not exists: {filePath}");
                                lock (_cacheFiles)
                                {
                                    _cacheFiles.Remove(filePath, out _);
                                }
                            }
                        }

                        if (isOverLimitSize && accessTime < releasePoint)
                        {
                            try
                            {
                                lock (_cacheFiles)
                                {
                                    if (_cacheFiles.TryGetValue(filePath, out accessTime) && accessTime >= releasePoint)
                                    {
                                        _logger.Info($"The cache file could not be released. It was access at: {accessTime}");
                                        continue;
                                    }
                                }

                                _logger.Info($"Deleting cache file: {filePath}");
                                File.Delete(filePath);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error($"Error occurred while deleting cache file: {filePath}. {ex}");
                            }
                        }
                    }

                }
            }

        }
    }

    
}
