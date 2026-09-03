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
using AvePoint.Media.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.DBLocker;
using AvePoint.RA.SharePoint.ArchiverCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading;

namespace AvePoint.RA.RAExchange.Disposal.Common
{
    public static class EXOEnforceRuleActionStatisticStore
    {
        private const string LockerPrefix = "EXOEnforceStatistic";
        private static readonly Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(EXOEnforceRuleActionStatisticStore));
        private static readonly ConcurrentDictionary<string, ActionStatistic> ActionStatistics = new ConcurrentDictionary<string, ActionStatistic>(StringComparer.OrdinalIgnoreCase);
        private static string _currentMainJobId;
        private static string _currentSubJobId;
        private static string _location;
        private static DateTime _subJobStartUtc;

        public static void BeginSubJob(string subJobId, string mainJobId, string mailboxName)
        {
            if (string.IsNullOrWhiteSpace(subJobId))
            {
                return;
            }

            _currentMainJobId = mainJobId;
            _currentSubJobId = subJobId;
            _location = mailboxName;
            _subJobStartUtc = DateTime.UtcNow;
            ActionStatistics.Clear();
        }

        public static void RecordDetail(JobDetailsStatus status, string action)
        {
            var statistic = ActionStatistics.GetOrAdd(action, _ => new ActionStatistic(action, _location));
            statistic.Track(status);
        }

        public static void CompleteSubJob(string mainJobId, string subJobId)
        {
            var records = BuildRecords(ActionStatistics.Values.ToList(), _currentMainJobId, _currentSubJobId, _subJobStartUtc, DateTime.UtcNow);
            if (records.Count == 0)
            {
                logger.Info($"Sub job {subJobId} has no detail statistics.");
                records.Add(new StatisticRecord()
                {
                    JobId = subJobId,
                    MainJobId = mainJobId,
                    Location = _location,
                    SubJobStartTimeUtc = _subJobStartUtc,
                    SubJobEndTimeUtc = DateTime.UtcNow,
                    Action = "RM_EXODisposal_Action_Scan"
                });
            }

            MergeAndUploadStatisticInfo(_currentMainJobId, _currentSubJobId, records);
        }

        private static List<StatisticRecord> BuildRecords(IEnumerable<ActionStatistic> statistics, string mainJobId, string subJobId, DateTime startUtc, DateTime endUtc)
        {
            var records = new List<StatisticRecord>();
            if (statistics == null)
            {
                return records;
            }

            foreach (var statistic in statistics)
            {
                if (!statistic.HasCount)
                {
                    continue;
                }
                records.Add(statistic.ToRecord(mainJobId, subJobId, startUtc, endUtc));
            }
            return records;
        }

        private static void MergeAndUploadStatisticInfo(string mainJobId, string subJobId, IReadOnlyCollection<StatisticRecord> records)
        {
            if (records == null || records.Count == 0)
            {
                return;
            }

            SampleDBLocker locker = null;
            try
            {
                locker = SampleDBLocker.GetAsync(LockerPrefix + mainJobId, subJobId, true, new TimeSpan(0, 10, 0)).GetAwaiter().GetResult();
                using var worker = new StatisticDbWorker(mainJobId);
                worker.PrepareLocalDatabase();
                worker.InsertRecords(records);
                worker.Upload();
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to persist EXO enforce rule statistics for main job {mainJobId}, sub job {subJobId}. {ex}");
            }
            finally
            {
                locker?.DisposeAsync().GetAwaiter().GetResult();
            }
        }

        private sealed class ActionStatistic
        {
            private int _success;
            private int _failed;
            private int _skipped;

            public ActionStatistic(string action, string location)
            {
                Action = action;
                Location = location;
            }

            public string Action { get; }
            public string Location { get; }

            public bool HasCount => _success > 0 || _failed > 0 || _skipped > 0;

            public void Track(JobDetailsStatus status)
            {
                switch (status)
                {
                    case JobDetailsStatus.Successful:
                        Interlocked.Increment(ref _success);
                        break;
                    case JobDetailsStatus.Skipped:
                        Interlocked.Increment(ref _skipped);
                        break;
                    default:
                        Interlocked.Increment(ref _failed);
                        break;
                }
            }

            public StatisticRecord ToRecord(string mainJobId, string subJobId, DateTime startTimeUtc, DateTime endTimeUtc)
            {
                return new StatisticRecord
                {
                    MainJobId = mainJobId,
                    JobId = subJobId,
                    Action = Action,
                    Location = Location,
                    SuccessCount = _success,
                    FailCount = _failed,
                    SkipCount = _skipped,
                    SubJobStartTimeUtc = startTimeUtc,
                    SubJobEndTimeUtc = endTimeUtc
                };
            }
        }

        private sealed class StatisticRecord
        {
            public string JobId { get; set; }
            public string MainJobId { get; set; }
            public string Action { get; set; }
            public int SuccessCount { get; set; }
            public int FailCount { get; set; }
            public int SkipCount { get; set; }
            public string Location { get; set; }
            public DateTime SubJobStartTimeUtc { get; set; }
            public DateTime SubJobEndTimeUtc { get; set; }

            public long SubJobStartTime => SubJobStartTimeUtc.Ticks;
            public long SubJobEndTime => SubJobEndTimeUtc.Ticks;
            public string SubJobStartTimeString => SubJobStartTimeUtc.ToString("yyyy-MM-dd HH:mm:ss");
            public string SubJobEndTimeString => SubJobEndTimeUtc.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private sealed class StatisticDbWorker : SqliteDBBase, IDisposable
        {
            private const string CacheFolderName = "EXOEnforceRuleStatistic";
            private const string TableName = "EXOEnforceRuleActionStatistic";
            private readonly string _blobUri;
            private readonly string _mainJobId;

            public StatisticDbWorker(string mainJobId)
            {
                _mainJobId = mainJobId;
                _dbdirPath = SecurityUtils.SafeCombinePath(Environment.CurrentDirectory, CacheFolderName);
                _dbName = _mainJobId + ".rpt";
                _dbFilePath = SecurityUtils.SafeCombinePath(_dbdirPath, _dbName);
                _blobUri = string.Join("/", TenantLocalValue.LogonGroupId, CacheFolderName, _dbName);
            }

            public void PrepareLocalDatabase()
            {
                try
                {
                    DownloadDBFromBlob(_blobUri, _dbFilePath);
                    if (!IsExistTable(_dbFilePath, TableName))
                    {
                        CreateDataBaseIfNotExist(_dbdirPath, _dbName);
                    }
                }
                catch (Exception ex)
                {
                    logger.Info($"Statistic DB for {_mainJobId} does not exist or cannot be downloaded. {ex.Message}");
                    CreateDataBaseIfNotExist(_dbdirPath, _dbName);
                }
            }

            public void InsertRecords(IReadOnlyCollection<StatisticRecord> records)
            {
                if (records == null || records.Count == 0)
                {
                    return;
                }

                ExecuteWithConnectionNoUpateDicLastAccessTime(connection =>
                {
                    using var transaction = connection.BeginTransaction();
                    using var command = connection.CreateCommand();
                    command.CommandText = $@"INSERT INTO {SecurityUtils.SanitizeSQLSchemaName(TableName)} 
(JobId, MainJobId, Action, SuccessCount, FailCount, SkipCount, Location, SubJobStartTime, SubJobStartTimeString, SubJobEndTime, SubJobEndTimeString)
VALUES (@JobId, @MainJobId, @Action, @SuccessCount, @FailCount, @SkipCount, @Location, @SubJobStartTime, @SubJobStartTimeString, @SubJobEndTime, @SubJobEndTimeString);";

                    var parameters = new[]
                    {
                        new SQLiteParameter("@JobId"),
                        new SQLiteParameter("@MainJobId"),
                        new SQLiteParameter("@Action"),
                        new SQLiteParameter("@SuccessCount"),
                        new SQLiteParameter("@FailCount"),
                        new SQLiteParameter("@SkipCount"),
                        new SQLiteParameter("@Location"),
                        new SQLiteParameter("@SubJobStartTime"),
                        new SQLiteParameter("@SubJobStartTimeString"),
                        new SQLiteParameter("@SubJobEndTime"),
                        new SQLiteParameter("@SubJobEndTimeString"),
                    };
                    foreach (var parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }

                    foreach (var record in records)
                    {
                        parameters[0].Value = record.JobId ?? string.Empty;
                        parameters[1].Value = record.MainJobId ?? string.Empty;
                        parameters[2].Value = record.Action ?? string.Empty;
                        parameters[3].Value = record.SuccessCount;
                        parameters[4].Value = record.FailCount;
                        parameters[5].Value = record.SkipCount;
                        parameters[6].Value = record.Location ?? string.Empty;
                        parameters[7].Value = record.SubJobStartTime;
                        parameters[8].Value = record.SubJobStartTimeString;
                        parameters[9].Value = record.SubJobEndTime;
                        parameters[10].Value = record.SubJobEndTimeString;
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                });
            }

            public void Upload()
            {
                UploadDatabase(_blobUri, _dbFilePath);
            }

            public void Dispose()
            {
                FileUtility.ForceDelete(_dbFilePath);
            }

            public override void CreateSchemaIfNotExists(IDbCommand command)
            {
                command.CommandText = $@"CREATE TABLE IF NOT EXISTS {SecurityUtils.SanitizeSQLSchemaName(TableName)} (
    JobId VARCHAR(50) NOT NULL,
    MainJobId VARCHAR(50) NOT NULL,
    Action VARCHAR(50) NOT NULL,
    SuccessCount INT NOT NULL,
    FailCount INT NOT NULL,
    SkipCount INT NOT NULL,
    Location VARCHAR(500),
    SubJobStartTime BIGINT NOT NULL,
    SubJobStartTimeString VARCHAR(50) NOT NULL,
    SubJobEndTime BIGINT NOT NULL,
    SubJobEndTimeString VARCHAR(50) NOT NULL
);";
                command.ExecuteNonQuery();
            }
        }
    }
}
