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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.DBLocker
{
    public class SampleDBLocker : IAsyncDisposable, IDisposable
    {
        protected readonly static RALogger _Logger = RALogger.GetInstance(typeof(SampleLocker));
        protected static ISampleLockerDao _LockerDao => PlatformWindsorManager.GetService<ISampleLockerDao>();
        protected const int _MaxRetryIntervalInMs = 1000 * 60 * 10;
        protected const int _MinRetryIntervalInMs = 1000 * 30;

        #region Static Medthods


        public static async Task<SampleDBLocker> GetAsync(string key, string description, bool waitLocker, TimeSpan? waitLockerTimeout = null)
        {
            key = key?.Trim();
            ThrowUtil.ThrowIfNullOrEmpty(key, nameof(key));

            int retryIntervalInMs = _MinRetryIntervalInMs;
            DateTime? endTime = waitLockerTimeout == null ? null : DateTime.UtcNow.Add(waitLockerTimeout.Value);

            if(waitLockerTimeout != null)
            {
                if(waitLockerTimeout.Value.TotalMilliseconds < _MinRetryIntervalInMs && waitLockerTimeout.Value.TotalMilliseconds > 0)
                {
                    retryIntervalInMs = (int)waitLockerTimeout.Value.TotalMilliseconds;
                }
            }

            do
            {

                try
                {
                    var canTryCreateLocker = false;
                    var existsLocker = await _LockerDao.GetAsync(key);
                    if (existsLocker != null)
                    {
                        // if modified time was not update in 120 min, the program held the locker should be hang or crash.
                        if(existsLocker.Timestamp < DateTime.UtcNow.AddMinutes(-120).Ticks)
                        {
                            await _LockerDao.DeleteAsync(key);
                            _Logger.Info($"The existing DB locker was timeout, will delete it. Key: {existsLocker.Key}, Description: {existsLocker.Extension}, Created: {new DateTime(existsLocker.Created).ToString()}, Timestamp: {new DateTime(existsLocker.Created).ToString()}");
                            canTryCreateLocker = true;
                        }
                        else
                        {
                            _Logger.Info($"Please wait, the DB locker was held. Key: {existsLocker.Key}, Description: {existsLocker.Extension}, Created: {new DateTime(existsLocker.Created).ToString()}");
                        }
                    }
                    else
                    {
                        canTryCreateLocker = true;
                    }

                    if(canTryCreateLocker)
                    {
                        await _LockerDao.CreateAsync(
                            new SampleLocker()
                            {
                                Key = key,
                                Extension = description,
                                Created = DateTime.UtcNow.Ticks,
                                Timestamp = DateTime.UtcNow.Ticks,
                            });

                        return new SampleDBLocker(key, description);
                    }
                }
                catch (Exception ex)
                {
                    _Logger.Warn($"Error occurred while creating DB locker [{key}]. {ex}");
                }
                
                if(!waitLocker)
                {
                    return null;
                }

                await Task.Delay(retryIntervalInMs);
                if(retryIntervalInMs < _MaxRetryIntervalInMs)
                {
                    retryIntervalInMs += 1000 * 30;
                    retryIntervalInMs = Math.Min(retryIntervalInMs, _MaxRetryIntervalInMs);
                }

                if (endTime != null && endTime < DateTime.UtcNow)
                {
                    throw new SampleDBLockerTimeoutException($"Get DB locker [{key}] timeout.");
                }

            } while (true);
        }


        #region Get DB Locker for Index DB updatation
        /// <summary>
        /// siteId is optional, could be null
        /// </summary>
        public static Task<SampleDBLocker> Get4IndexDBUpdater(string siteUrl, string siteId, string jobId, TimeSpan? waitLockerTimeout = null)
        {
            return GetAsync(
                GetLockerKey4IndexDBUpdater(siteUrl),
                SerializerHelper.SerializeByJsonConvert(new List<string>() { siteUrl, siteId, jobId }),
                true,
                waitLockerTimeout);
        }
        /// <summary>
        /// siteId is optional, could be null
        /// </summary>
        public static async Task<(bool, SampleDBLocker)> TryGet4IndexDBUpdater(string siteUrl, string siteId, string jobId)
        {
            var dbLocker = await GetAsync(
                GetLockerKey4IndexDBUpdater(siteUrl),
                SerializerHelper.SerializeByJsonConvert(new List<string>() { siteUrl, siteId, jobId }),
                false);

            return (dbLocker != null, dbLocker);
        }
        public static async Task<(bool, SampleDBLocker)> TryGet4IndexDBUpdaterForGoogle(string siteUrl, string siteId, string jobId)
        {
            var dbLocker = await GetAsync(
                siteUrl,
                SerializerHelper.SerializeByJsonConvert(new List<string>() { siteUrl, siteId, jobId }),
                false);

            return (dbLocker != null, dbLocker);
        }
        private static string GetLockerKey4IndexDBUpdater(string siteCollectionUrl)
        {
            String webAppName, siteName;
            ParseSitePath(siteCollectionUrl, out webAppName, out siteName);
            return $"IndexDBLocker_{webAppName}{siteName}";
        }
        private static void ParseSitePath(String siteURL, out String webAppName, out String siteName)
        {
            int index = -1;
            StringBuilder tmp = new StringBuilder();
            index = siteURL.IndexOf(":", StringComparison.OrdinalIgnoreCase);
            tmp.Append(siteURL.Substring(0, index)).Append("#");
            string temp = siteURL.Substring(index + 3);
            index = -1;
            index = temp.IndexOf(":", StringComparison.OrdinalIgnoreCase);
            if (index == -1)
            {
                tmp.Append(80).Append("#");
                index = temp.IndexOf("/", StringComparison.OrdinalIgnoreCase);
                if (index != -1)
                {
                    tmp.Append(temp.Substring(0, index));
                    temp = temp.Substring(index + 1);
                }
                else
                {
                    tmp.Append(temp);
                    temp = "";
                }
            }
            else
            {
                String machineName = temp.Substring(0, index);
                temp = temp.Substring(index + 1);
                index = -1;
                index = temp.IndexOf("/", StringComparison.OrdinalIgnoreCase);
                if (index != -1)
                {
                    tmp.Append(temp.Substring(0, index));
                    temp = temp.Substring(index + 1);
                }
                else
                {
                    tmp.Append(temp);
                    temp = "";
                }
                tmp.Append("#").Append(machineName);
            }
            webAppName = tmp.ToString();
            tmp.Remove(0, tmp.Length);
            tmp.Append("#");
            if (temp.Length > 0)
            {
                temp = temp.Replace(';', '#');
                tmp.Append(temp.Replace('/', '#'));
            }
            siteName = tmp.ToString();
        }

        #endregion

        #region Get DB Locker for index DB with email address format
        public static Task<SampleDBLocker> Get4IndexDBEmail(string email, string siteId, string jobId, JobType jobType, TimeSpan? waitLockerTimeout = null)
        {
            return GetAsync(
                GetLockerKey4IndexDBEmail(email, jobType),
                SerializerHelper.SerializeByJsonConvert(new List<string>() { email, siteId, jobId }),
                true,
                waitLockerTimeout);
        }
        public static async Task<(bool, SampleDBLocker)> TryGet4IndexDBEmail(string email, string siteId, string jobId, JobType jobType)
        {
            var dbLocker = await GetAsync(
                GetLockerKey4IndexDBEmail(email, jobType),
                SerializerHelper.SerializeByJsonConvert(new List<string>() { email, siteId, jobId }),
                false);

            return (dbLocker != null, dbLocker);
        }
        protected static void ParseEmail(String email, out String domainName, out String name)
        {
            var emailSplit = email.Split("@");
            if (emailSplit.Length != 2) throw new Exception("The email is wrong format");
            domainName = emailSplit[0];
            name = emailSplit[1];
        }
        private static string GetLockerKey4IndexDBEmail(string email, JobType jobType)
        {
            String domainName, name;
            ParseEmail(email, out domainName, out name);
            return $"IndexDBLocker_{jobType}{domainName}{name}";
        }
        #endregion

        #endregion

        protected bool _IsReleased = false;
        protected readonly string _LockerKey;
        // update Locker record modified time per 30 min.
        protected readonly PeriodicTimer _Timer = new PeriodicTimer(TimeSpan.FromMinutes(30));

        protected SampleDBLocker(string key, string description, bool needLog = true)
        {
            this._LockerKey = key;
            this.StartUpdater();
            if (needLog) _Logger.Info($"DB locker was created. Key: {key}, Description: {description}");
        }

        protected void StartUpdater()
        {
            Task.Run(async () =>
            {
                while (!_IsReleased && await _Timer.WaitForNextTickAsync())
                {
                    try
                    {
                        await _LockerDao.UpdateTimestampAsync(this._LockerKey);
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error($"Error occurred while updating locker timestamp: {this._LockerKey}. {ex}");
                    }
                }
            });
        }

        public virtual async ValueTask DisposeAsync()
        {
            try
            {
                var result = await _LockerDao.DeleteAsync(this._LockerKey);
                _IsReleased = true;
                _Timer.Dispose();
                _Logger.Info($"Release locker [{this._LockerKey}]: {result}");
            }
            catch (Exception ex)
            {
                _Logger.Error($"Error occurred while releasing locker: {this._LockerKey}. {ex}");
            }
        }

        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }

        [Serializable]
        public class SampleDBLockerTimeoutException: TimeoutException
        {
            public SampleDBLockerTimeoutException(string? message)
                : base(message)
            {
            }
        }
    }
}
