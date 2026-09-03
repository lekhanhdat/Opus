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
using Amazon.Runtime.Internal.Transform;
using AngleSharp.Common;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.Wrapper.Common;
using RAArchiverCommon.Sqlite;
using RAArchiverCommon.Sqlite.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAArchiverCommon.DisposalProgress.Impl
{
    public class ThrottlingStatistic : BaseThreadDisposalStatistic
    {
        #region properties
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private long AllRequestCount => _appThrottlingCache.Values.Sum(cache => cache.AllRequestCount);
        private long AllSuccessRequestCount => _appThrottlingCache.Values.Sum(cache => cache.AllSuccessRequestCount);
        private long ThrottlingRequestCount => _appThrottlingCache.Values.Sum(cache => cache.ThrottlingRequestCount);
        private long ThrottlingWaitingTime => _appThrottlingCache.Values.Sum(cache => cache.ThrottlingWaitingTime);
        #endregion

        #region Const and statistic
        private readonly static object _instanceLock = new object();

        private static ThrottlingStatistic _instance;

        private const String DEFAULT_APP_ID = "notappid-nota-ppid-nota-ppidnotap";
        #endregion

        #region Field
        private long _running429ReqeustCount;

        private DateTime _running429StartTime;

        private long _jobAllExist429Times;

        private string _scope;

        private string _msId;

        private DateTime _subJobStartTime;

        private List<string> _tooManyRequestErrors = new List<string>();

        private DateTime _lastUpdateThrottlingDateTime = DateTime.UtcNow;

        private Dictionary<string, ThrottlingStatisticCache> _appThrottlingCache = new ();
        #endregion

        #region instance
        public static ThrottlingStatistic Instance()
        {
            if (_instance == null)
            {
                lock (_instanceLock)
                {
                    if (_instance == null)
                    {
                        _mLog.Info("Create ThrottlingStatistic instance");
                        _instance = new ThrottlingStatistic();
                    }
                }
            }
            return _instance;
        }

        private ThrottlingStatistic()
        {
            _executeThreadAction = StatisticThrottlingAndUpToStorage;
        }
        #endregion

        #region Implement the interface
        public override void Init(DisposalStaticInitObject initObject)
        {
            if (_alreadyInit || _statisticState != ThreadState.Unstarted)
            {
                _mLog.Error($"CompoundStatistics unable init, init status:{_alreadyInit}, statistic state:{_statisticState}");
                return;
            }

            ActionWithLockAndCatch(() =>
            {
                this._mainJobId = initObject.MainJobId;
                this._subJobId = initObject.SubJobId;
                this._jobType = initObject.JobType;

                RMSubJob subJob = SubJobDao.GetSubJob(_subJobId, false);
                _subJobStartTime = new DateTime(subJob.StartTime, DateTimeKind.Utc);
                this._scope = subJob.String1;
                this._msId = subJob.O365TenantId;

                RegisterAction();

                _alreadyInit = true;
            }, "Fail init");
        }

        public override void StartStatistic()
        {
            base.StartStatistic();
        }

        public override void PrepareEndStatistic()
        {
            lock (_lockObject)
            {
                if (_statisticState == ThreadState.StopRequested ||  _statisticState == ThreadState.Stopped)
                {
                    _mLog.Error($"state: {_statisticState},already unable request stop");
                    return;
                }
                _statisticState = ThreadState.StopRequested;
            }

            EndAddTooManyRequestError();

            UnRegisterAction();
        }

        public override void WaitEndStatistic()
        {
            if (_executeThread != null)
            {
                _executeThread.WaitThreadFinish();
            }
        }

        public override ThreadState GetStatisticState()
        {
            return _statisticState;
        }

        public override bool AlreadyInit()
        {
            return _alreadyInit;
        }
        #endregion

        #region statistic thread
        private void StatisticThrottlingAndUpToStorage()
        {
            int exceptionCountAfterRequestStop = 0;
            while (true)
            {
                ThreadState currentStatisticState;
                lock (_lockObject)
                {
                    currentStatisticState = _statisticState;
                }
                if (currentStatisticState == ThreadState.Stopped)
                {
                    break;
                }

                try
                {
                    RealStatisticAndUploadToStorage(ref currentStatisticState);
                }
                catch (Exception ex)
                {
                    _mLog.Error($"Fail statistic throttling, ex:{ex}");
                    if (currentStatisticState == ThreadState.StopRequested && ++exceptionCountAfterRequestStop > 5)
                    {
                        lock (_lockObject)
                        {
                            _mLog.Error("After reqeust stop, occure more than 5 fail, will exist statistic thread");
                            _statisticState = ThreadState.Stopped;
                            currentStatisticState = _statisticState;
                        }
                    }
                }
                finally
                {
                    if (currentStatisticState != ThreadState.Stopped && currentStatisticState != ThreadState.StopRequested)
                    {
                        Thread.Sleep(1000 * 10);// sleep 10s
                    }
                }
            }
        }

        private void RealStatisticAndUploadToStorage(ref ThreadState nowStatisticState)
        {
            DateTime now = DateTime.UtcNow;
            if (DateOnly.FromDateTime(now) > DateOnly.FromDateTime(_lastUpdateThrottlingDateTime))
            {
                DateTime yesterday = now - new TimeSpan(1, 0, 0, 0);
                RealStatisticDayThrottlingAndUploadToStorage(yesterday);
                _lastUpdateThrottlingDateTime = now;
            }

            if (nowStatisticState == ThreadState.StopRequested)
            {
                if (_lastUpdateThrottlingDateTime <= now)
                {
                    RealStatisticDayThrottlingAndUploadToStorage(now);
                    _lastUpdateThrottlingDateTime = DateTime.MaxValue;
                }
                RealStatisticJobThrottlingAndUploadToStorage(now);
                lock (_lockObject)
                {
                    _statisticState = ThreadState.Stopped;
                    nowStatisticState = _statisticState;
                }
            }
        }

        private void RealStatisticDayThrottlingAndUploadToStorage(DateTime date)
        {
            try
            {
                using DayThrottlingDetailWorker worker = new DayThrottlingDetailWorker(_subJobId, date);
                worker.GetLastestDataBase();
                List<ThrottlingDetails> details = new List<ThrottlingDetails>();
                lock (_lockObject)
                {
                    details.AddRange(GeneThrottlingDeatails(date, StatisticThrottlingType.Hour));
                    details.AddRange(GeneThrottlingDeatails(date, StatisticThrottlingType.Day));
                }
                worker.InsertValueToDB(details.ToArray());
                worker.UploadDatabase();
            }
            catch (Exception ex)
            {
                _mLog.Error($"Fail statistic day throttling , date:{date.ToString()}, ex:{ex}");
                throw;
            }
        }

        private void RealStatisticJobThrottlingAndUploadToStorage(DateTime now)
        {
            try
            {
                using ThrottlingStatisticDBLock dbLock = new ThrottlingStatisticDBLock();
                dbLock.GetLock();
                using JobThrottlingDetailWorker worker = new JobThrottlingDetailWorker();
                worker.GetLastestDataBase();
                IEnumerable<ThrottlingDetails> detail = GeneThrottlingDeatails(now, StatisticThrottlingType.Job);
                if (detail.Any())
                {
                    worker.InsertValueToDB(detail.ToArray());
                    worker.UploadDatabase();
                }
                else
                {
                    _mLog.Warn("The job don't have any detail");
                }
            }
            catch (Exception e)
            {
                _mLog.Error($"Fail statistic job throttling, ex:{e}");
                throw;
            }
        }
        #endregion

        #region gene throttling Details
        private IEnumerable<ThrottlingDetails> GeneThrottlingDeatails(DateTime now, StatisticThrottlingType type)
        {
            List<ThrottlingDetails> res = new ();
            switch (type)
            {
                case StatisticThrottlingType.Job:
                    res.Add(GeneJobThrottlingDeatails(now));
                    break;
                case StatisticThrottlingType.Hour:
                    res.AddRange(GeneHourThrottlingDeatails(now));
                    break;
                case StatisticThrottlingType.Day:
                    res.Add(GeneDayThrottlingDeatails(now));
                    break;
                default:
                    _mLog.Error($"Type {type}, excpetion");
                    break;
            }
            return res;
        }

        private ThrottlingDetails GeneBaseJobThrottlingDeatails(DateTime now)
        {
            return new ThrottlingDetails()
            {
                Id = Guid.NewGuid().ToString(),
                MainJobId = _mainJobId,
                SubJobId = _subJobId,
                TenantId = TenantLocalValue.LogonGroupId,
                MSId = _msId,
                Scope = _scope,
                Day = now.Day,
                Hour = now.Hour,
                JobStartTime = _subJobStartTime.Ticks,
                JobStartTimeStr = _subJobStartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                JobRunTime = now.Ticks - _subJobStartTime.Ticks,
                JobRunHours = new TimeSpan(now.Ticks - _subJobStartTime.Ticks).TotalHours,
            };
        }

        private ThrottlingDetails GeneJobThrottlingDeatails(DateTime now)
        {
            ThrottlingDetails res = GeneBaseJobThrottlingDeatails(now);
            res.JobEndTime = now.Ticks;
            res.JobEndTimeStr = now.ToString("yyyy-MM-dd HH:mm:ss");
            res.TotalRquestCount = AllRequestCount;
            res.ThrottlingRquestCount = ThrottlingRequestCount;
            res.SuccessRquestCount = AllSuccessRequestCount;
            res.ThrottlingCountEachHour = ThrottlingRequestCount / (new TimeSpan(now.Ticks - _subJobStartTime.Ticks).TotalHours);
            res.ThrottlingSleepSumTime = ThrottlingWaitingTime;
            res.Type = StatisticThrottlingType.Job;
            return res;
        }

        private IEnumerable<ThrottlingDetails> GeneHourThrottlingDeatails(DateTime date)
        {
            var allDetails = _appThrottlingCache.Values
                .SelectMany(cache => cache.GetOrDefault(date, new Queue<ThrottlingDetails>()))
                .Where(detail => detail != null);

            var mergedDetails = allDetails
                .GroupBy(detail => detail.Hour)
                .Select(group => new ThrottlingDetails
                {
                    Id = group.First().Id,
                    MainJobId = group.First().MainJobId,
                    SubJobId = group.First().SubJobId,
                    TenantId = group.First().TenantId,
                    MSId = group.First().MSId,
                    Scope = group.First().Scope,
                    Day = group.First().Day,
                    Hour = group.Key,

                    TotalRquestCount = group.Sum(d => d.TotalRquestCount),
                    SuccessRquestCount = group.Sum(d => d.SuccessRquestCount),
                    ThrottlingRquestCount = group.Sum(d => d.ThrottlingRquestCount),
                    ThrottlingSleepSumTime = group.Sum(d => d.ThrottlingSleepSumTime),

                    JobStartTimeStr = group.First().JobStartTimeStr,
                    JobStartTime = group.First().JobStartTime,
                    JobRunHours = group.First().JobRunHours,
                    JobRunTime = group.First().JobRunTime,
                    ThrottlingCountEachHour = group.Sum(d => d.ThrottlingRquestCount),
                    Type = StatisticThrottlingType.Hour
                });

            return mergedDetails.ToList();
        }

        private ThrottlingDetails GetOrCreateHourThrottlingDeatails(DateTime date, string appId)
        {
            lock (_lockObject)
            {
                if (string.IsNullOrWhiteSpace(appId))
                {
                    appId = DEFAULT_APP_ID;
                }
                if (!_appThrottlingCache.ContainsKey(appId))
                {
                    _appThrottlingCache.Add(appId, new());
                }

                ThrottlingStatisticCache cache = _appThrottlingCache[appId];
                if (!cache.ExistKey(date))
                {
                    cache.Add(date, new());
                }

                Queue<ThrottlingDetails> details = cache.Get(date);
                if (!details.Any(detail => detail.Hour == date.Hour))
                {
                    ThrottlingDetails data = GeneBaseJobThrottlingDeatails(date);
                    data.Type = StatisticThrottlingType.Hour;
                    details.Enqueue(data);
                }

                return details.First(detail => detail.Hour == date.Hour);
            }
        }

        private ThrottlingDetails GeneDayThrottlingDeatails(DateTime now)
        {
            IEnumerable<ThrottlingDetails> hourDetails = GeneHourThrottlingDeatails(now).OrderBy(detail => detail.Hour);
            ThrottlingDetails res = GeneBaseJobThrottlingDeatails(now);
            res.JobEndTime = 0;
            res.JobEndTimeStr = string.Empty;
            res.TotalRquestCount = hourDetails.Sum(detail => detail.TotalRquestCount);
            res.ThrottlingRquestCount = hourDetails.Sum(detail => detail.ThrottlingRquestCount);
            res.SuccessRquestCount = hourDetails.Sum(detail => detail.SuccessRquestCount);
            if (hourDetails.Any())
            {
                res.ThrottlingCountEachHour = (double)res.ThrottlingRquestCount / (hourDetails.Last().Hour - hourDetails.First().Hour + 1);
            }
            else
            {
                res.ThrottlingCountEachHour = 0;
            }
            res.ThrottlingSleepSumTime = hourDetails.Sum(detail => detail.ThrottlingSleepSumTime);
            res.Type = StatisticThrottlingType.Day;
            return res;
        }
        #endregion

        #region statistic data
        private void UnRegisterAction()
        {
            ReliableHttpWebRequest.BeforeEachRequestEvent -= IncreaseAllRequestCount;
            ReliableHttpWebRequest.AfterRequestSuccessEvent -= IncreaseAllSuccessRequestCount;
            O365TenantHealthScore.BeforeThrottlingSleepEvent -= AddTooManyRequestError;
            O365TenantHealthScore.AfterThrottlingSleepEvent -= RecordEndAppSleepFor429;
        }

        private void RegisterAction()
        {
            UnRegisterAction();

            ReliableHttpWebRequest.BeforeEachRequestEvent += IncreaseAllRequestCount;
            ReliableHttpWebRequest.AfterRequestSuccessEvent += IncreaseAllSuccessRequestCount;

            O365TenantHealthScore.BeforeThrottlingSleepEvent += AddTooManyRequestError;
            O365TenantHealthScore.AfterThrottlingSleepEvent += RecordEndAppSleepFor429;
        }

        public void IncreaseAllRequestCount(string appId, DateTime time)
        {
            try
            {
                lock (_lockObject)
                {
                    GetOrCreateHourThrottlingDeatails(time, appId).TotalRquestCount++;
                }
            }
            catch (Exception e)
            {
                _mLog.Error($"An error occurred when increase all request count, appId:{appId}, time:{time}, e:{e}");
            }
        }

        public void IncreaseAllSuccessRequestCount(string appId, DateTime time)
        {
            try
            {
                lock (_lockObject)
                {
                    GetOrCreateHourThrottlingDeatails(time, appId).SuccessRquestCount++;
                } 
            }
            catch (Exception e)
            {
                _mLog.Error($"An error occurred when increase all success request count, appId:{appId}, time:{time}, e:{e}");
            }
        }


        public void RecordEndAppSleepFor429()
        {
            try
            {
                lock (_lockObject)
                {
                    if (--_running429ReqeustCount == 0)
                    {
                        _jobAllExist429Times += (DateTime.UtcNow - _running429StartTime).Ticks;
                    }
                }
            }
            catch(Exception e)
            {
                _mLog.Error($"An error occurred when record end app sleep for 429, e:{e}");
            }
        }


        public void AddTooManyRequestError(String error, long throttlingWaitingTime, string appIdOf429)
        {
            try
            {
                lock (_lockObject)
                {
                    DateTime now = DateTime.UtcNow;
                    if (++_running429ReqeustCount == 1)
                    {
                        _running429StartTime = now;
                    }

                    GetOrCreateHourThrottlingDeatails(now, appIdOf429).ThrottlingRquestCount++;
                    GetOrCreateHourThrottlingDeatails(now, appIdOf429).ThrottlingSleepSumTime += throttlingWaitingTime;
                    _tooManyRequestErrors.Add(error);
                }
                if (_tooManyRequestErrors.Count > 5000)
                {
                    WriteTooManyRequestError();
                }
            }
            catch (Exception e)
            {
                _mLog.Error($"An error occurred when add too many request error, e:{e}");
            }
        }


        public void EndAddTooManyRequestError()
        {
            try
            {
                if (AllRequestCount > 0)
                {
                    StringBuilder summaryLog = new StringBuilder();
                    summaryLog.Append($@" All Request Count : {AllRequestCount},Throttling Request Count : {ThrottlingRequestCount}, Success Reqeust Count:{AllSuccessRequestCount}, ");
                    summaryLog.Append($@" Percentage Of Throttling : {(int)(ThrottlingRequestCount / (double)AllRequestCount*100)}%, ");
                    summaryLog.Append($@" Exist 429 Sleep Times : {new TimeSpan(_jobAllExist429Times).ToString(@"d\d\ hh\h\ mm\m\ ss\s")}, ");
                    summaryLog.Append($@" Throttling Total Waiting Time : {new TimeSpan(ThrottlingWaitingTime).ToString(@"d\d\ hh\h\ mm\m\ ss\s")}");
                    _tooManyRequestErrors.Add(summaryLog.ToString());

                    foreach (string appId in _appThrottlingCache.Keys)
                    {
                        List<Queue<ThrottlingDetails>> cache = _appThrottlingCache.GetValueOrDefault(appId, new()).GetValues();
                        long reqeustCount = cache.Sum(details => details.Sum(detail => detail.TotalRquestCount));
                        long successCount = cache.Sum(details => details.Sum(detail => detail.SuccessRquestCount));
                        long throttlingCount = cache.Sum(details => details.Sum(detail => detail.ThrottlingRquestCount));
                        long sleepTime = cache.Sum(details => details.Sum(detail => detail.ThrottlingSleepSumTime));
                        summaryLog = new StringBuilder();
                        summaryLog.Append($@"App: {appId}, reqeust count:{reqeustCount}, Throttling Request Count : {throttlingCount}, Success Reqeust Count:{successCount}, ");
                        summaryLog.Append($@" Percentage Of Throttling : {(reqeustCount > 0 ? ((int)(throttlingCount / (double)reqeustCount) * 100) : 0)}%, ");
                        summaryLog.Append($@"Throttling Total Waiting Time : {new TimeSpan(sleepTime).ToString(@"d\d\ hh\h\ mm\m\ ss\s")}");
                        _tooManyRequestErrors.Add(summaryLog.ToString());
                    }
                }

                if (_tooManyRequestErrors.Count > 0)
                {
                    WriteTooManyRequestError();
                }
            }
            catch (FileNotFoundException ex)
            {
                _mLog.Error("An file not found when generation TooManyRequestError log .Error:{0}", ex);
            }
            catch (Exception ex)
            {
                _mLog.Error("An error occurred when generation TooManyRequestError log .Error:{0}", ex);
            }
        }

        private void WriteTooManyRequestError()
        {
            try
            {
                foreach (string requestError in _tooManyRequestErrors)
                {
                    RACustomLogger.WriteToolManyRequestLog(requestError);
                }
                _tooManyRequestErrors.Clear();
            }
            catch (Exception ex)
            {
                _mLog.Error("An error occurred when write too many request errors to file.Error:{0}", ex);
            }
        }
        #endregion

        private class ThrottlingStatisticDBLock : IDisposable
        {
            public readonly static TimeSpan _timeOutSpan = new TimeSpan(0, 10, 0);

            private const string LOCK_KEY = "ThrottlingStatisticDBLock";

            private readonly Object _innerLock = new Object();

            private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

            private Thread _keepLiveThread;

            private long _deathTime;

            private readonly string _lockId = Guid.NewGuid().ToString();

            private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

            public void GetLock()
            {
                int tryGetLockCount = 1;
                while (!TryGetLock())
                {
                    Thread.Sleep(Math.Max(_timeOutSpan.Milliseconds / 10, 1000 * 50) + new Random().Next(-10 * 1000, 10 * 1000));
                    if (++tryGetLockCount >= 10)
                    {
                        throw new Exception($"After 10 try get lock, unable get lock");
                    }
                }
            }

            public bool TryGetLock()
            {
                lock (_innerLock)
                {
                    if (_keepLiveThread != null && _keepLiveThread.IsAlive)
                    {
                        return true;
                    }

                    if (RealGetLock())
                    {
                        _keepLiveThread = new Thread(KeepLive);
                        _keepLiveThread.IsBackground = true;
                        _keepLiveThread.Start();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            private void KeepLive()
            {
                try
                {
                    while (true)
                    {
                        lock (_innerLock)
                        {
                            if (_cancellationToken.IsCancellationRequested)
                            {
                                break;
                            }
                            if ((DateTime.UtcNow.Ticks + _timeOutSpan.Milliseconds * 0.75) < _deathTime)
                            {
                                if (!RealGetLock())
                                {
                                    _mLog.Error("In keepLive thread unable get db lock, so exist keep live thread");
                                    break;
                                }
                            }
                        }
                        Thread.Sleep(1000);
                    }
                }catch(Exception e)
                {
                    _mLog.Error(@"Have exception occure in keep live thread, e:{e}");
                }
                finally
                {

                }
            }

            public void ReleaseLock()
            {
                lock (_innerLock)
                {
                    try
                    {
                        _cancellationToken?.Cancel();
                        _cancellationToken?.Dispose();
                    }
                    catch(Exception e)
                    {
                        _mLog.Error($"Fail set cancel token status, e:{e}");
                    }
                    try
                    {
                        RMKeyValue jsonLock = RMKeyValueDao.GetValueByKey(LOCK_KEY);
                        ThrottlingDBLockObject oldLock = SerializerHelper.DeserializeByJsonConvert<ThrottlingDBLockObject>(jsonLock.Value);
                        if(oldLock.Id.Equals(_lockId, StringComparison.OrdinalIgnoreCase))
                        {
                            ThrottlingDBLockObject newLock = new(_lockId, DateTime.MinValue.Ticks);
                            RMKeyValueDao.AtomicityUpdate(LOCK_KEY, jsonLock.Value, SerializerHelper.SerializeByJsonConvert(newLock));
                        }
                    }
                    catch (Exception e)
                    {
                        _mLog.Error($"Exception when realease lock, ex:{e}");
                    }
                }
            }

            private bool RealGetLock()
            {
                RMKeyValue jsonLock = RMKeyValueDao.GetValueByKey(LOCK_KEY);
                if(jsonLock == null)
                {
                    if (CreateNewLock())
                    {
                        return true;
                    }
                    else
                    {
                        jsonLock = RMKeyValueDao.GetValueByKey(LOCK_KEY);
                    }
                }

                ThrottlingDBLockObject oldLock;
                try
                {
                    oldLock = SerializerHelper.DeserializeByJsonConvert<ThrottlingDBLockObject>(jsonLock.Value);
                }
                catch(Exception e)
                {
                    _mLog.Error($"Fail parse old lock, old lock str:{jsonLock.Value}, ex:{e}");
                    oldLock = new(_lockId, DateTime.MinValue.Ticks); 
                }
                 
                if (oldLock.DeathTime <= DateTime.UtcNow.Ticks || oldLock.Id.Equals(_lockId, StringComparison.OrdinalIgnoreCase))
                {
                    ThrottlingDBLockObject newLock = new(_lockId, (DateTime.UtcNow + _timeOutSpan).Ticks);
                    return RMKeyValueDao.AtomicityUpdate(LOCK_KEY, jsonLock.Value, SerializerHelper.SerializeByJsonConvert(newLock));
                }
                else
                {
                    return false;
                }
            }

            private bool CreateNewLock()
            {
                try
                {
                    ThrottlingDBLockObject lockObject = new (_lockId, (DateTime.UtcNow + _timeOutSpan).Ticks);
                    return RMKeyValueDao.Save(new RMKeyValue() { Key = LOCK_KEY, Value = SerializerHelper.SerializeByJsonConvert(lockObject) });
                }
                catch(Exception e)
                {
                    _mLog.Error($"Fail create new lock, ex:{e}");
                    return false;
                }
            }

            public void Dispose()
            {
                ReleaseLock();
            }

            private class ThrottlingDBLockObject
            {
                public string Id { get; set; }

                public long DeathTime { get; set; }

                public ThrottlingDBLockObject(string id, long deathTime)
                {
                    Id = id;
                    DeathTime = deathTime;
                }
            }

        }

        private class ThrottlingStatisticCache
        {
            private Dictionary<string, Queue<ThrottlingDetails>> _eachDayCache = new();

            private const string DATE_FORMAT = "yyyy-MM-dd";

            public long AllRequestCount => GetValues().Sum(details => details.Sum(detail => detail.TotalRquestCount));
            public long AllSuccessRequestCount => GetValues().Sum(details => details.Sum(detail => detail.SuccessRquestCount));
            public long ThrottlingRequestCount => GetValues().Sum(details => details.Sum(detail => detail.ThrottlingRquestCount));
            public long ThrottlingWaitingTime => GetValues().Sum(details => details.Sum(detail => detail.ThrottlingSleepSumTime));

            public List<Queue<ThrottlingDetails>> GetValues()
            {
                return _eachDayCache.Values.ToList();
            }

            public List<DateTime> GetKeys()
            {
                return _eachDayCache.Keys.Select(key => DateTime.Parse(key)).ToList();
            }

            public Queue<ThrottlingDetails> Get(DateTime date)
            {
                return GetOrDefault(date, null);
            }

            public Queue<ThrottlingDetails> GetOrDefault(DateTime date, Queue<ThrottlingDetails> defaultValue)
            {
                string dateStr = date.ToString(DATE_FORMAT);
                return _eachDayCache.GetOrDefault(dateStr, defaultValue);
            }

            public bool TryGet(DateTime date, out Queue<ThrottlingDetails> details)
            {
                string dateStr = date.ToString(DATE_FORMAT);
                return _eachDayCache.TryGetValue(dateStr, out details);
            }

            public bool ExistKey(DateTime date) 
            { 
                string dateStr = date.ToString( DATE_FORMAT);
                return _eachDayCache.ContainsKey(dateStr);
            }

            public bool Delete(DateTime date)
            {
                string dateStr = date.ToString(DATE_FORMAT);
                return _eachDayCache.Remove(dateStr);
            }

            public void Add(DateTime date, Queue<ThrottlingDetails> details) 
            {
                string dateStr = date.ToString(DATE_FORMAT);
                _eachDayCache.Add(dateStr, details);
            }
        }
    }
}
