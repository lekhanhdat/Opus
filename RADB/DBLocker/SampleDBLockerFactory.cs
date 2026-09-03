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
using AvePoint.RA.Contract.JobMonitor;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.DBLocker
{
    public class SampleDBLockerFactory : IAsyncDisposable, IDisposable
    {
        private readonly ConcurrentDictionary<string, SampleDBLocker> _lockers = new(StringComparer.OrdinalIgnoreCase);
        private readonly static RALogger _Logger = RALogger.GetInstance(typeof(SampleDBLockerFactory));

        private readonly string _jobId;
        private readonly JobType _jobType;

        public SampleDBLockerFactory(string jobId) : this(jobId, JobType.None) { }

        public SampleDBLockerFactory(string jobId, JobType jobType)
        {
            _jobId = jobId;
            _jobType = jobType;
        }

        #region Lock Methods

        /// <summary>
        /// register and keep the lock for site collection.
        /// siteId is optional.
        /// </summary>
        public async Task<bool> TryAcquire4IndexDBUpdaterAsync(string siteUrl, string siteId, TimeSpan? timeout = null)
        {
            if (string.IsNullOrWhiteSpace(siteUrl)) return false;
            if (_lockers.ContainsKey(siteUrl)) return true;

            try
            {
                var locker = await SampleDBLocker.Get4IndexDBUpdater(siteUrl, siteId, _jobId, timeout);
                if (locker != null)
                {
                    _Logger.Info($"Successfully acquired locker for site [{siteUrl}] with jobId [{_jobId}]");
                    return _lockers.TryAdd(siteUrl, locker);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"Failed to acquire site locker for [{siteUrl}]. {ex}");
            }

            return false;
        }

        /// <summary>
        /// register and keep the lock for email
        /// </summary>
        public async Task<bool> TryAcquire4IndexDBEmailAsync(string email, string siteId, JobType jobType, TimeSpan? timeout = null)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            if (_lockers.ContainsKey(email)) return true;

            try
            {
                var locker = await SampleDBLocker.Get4IndexDBEmail(email, siteId, _jobId, jobType, timeout);
                if (locker != null)
                {
                    return _lockers.TryAdd(email, locker);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"Failed to acquire email locker for [{email}]. {ex}");
            }

            return false;
        }

        public async Task<bool> TryAcquire4IndexDBEmailAsync(string email, string siteId, TimeSpan? timeout = null)
            => await TryAcquire4IndexDBEmailAsync(email, siteId, _jobType, timeout);

        #endregion

        #region Release Methods

        /// <summary>
        /// Release the lock for the specific resource (site collection or email) based on the locker key (siteUrl or email).
        /// </summary>
        public async Task ReleaseAsync(string lockerKey)
        {
            if (string.IsNullOrWhiteSpace(lockerKey)) return;

            if (_lockers.TryRemove(lockerKey, out var locker))
            {
                await locker.DisposeAsync();
                _Logger.Info($"Successfully released locker for [{lockerKey}] with jobId [{_jobId}]");
            }
        }

        /// <summary>
        /// Release all locks that have been acquired and registered in the _lockers dictionary.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            foreach (var key in _lockers.Keys)
            {
                await ReleaseAsync(key);
            }
        }

        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }

        #endregion
    }
}
