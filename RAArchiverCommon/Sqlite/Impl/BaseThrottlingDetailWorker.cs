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
using AvePoint.RA.Api.Contract;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Object;
using RAArchiverCommon.DisposalProgress;
using AvePoint.RA.SharePoint.ArchiverCommon;
using System.Data;
using AvePoint.Media.Common;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Discovery.Model.Query;
using RAArchiverCommon.DiscoveryArchiveJob;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using AvePoint.RA.Contract.Tenant;
using DocumentFormat.OpenXml.VariantTypes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RAArchiverCommon.Sqlite.Impl
{
    public abstract class BaseThrottlingDetailWorker : SqliteDBBase , IDisposable
    {

        protected static AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(BaseThrottlingDetailWorker));

        protected static string CACHE_FODER_PATH = SecurityUtils.SafeCombinePath(Environment.CurrentDirectory, CACHE_FOLDER_NAME);

        protected const string CACHE_FOLDER_NAME = "ThrottlingStatistic";

        protected const string TABLE_NAME = JobMonitorConstants.JOBDETAIL;

        protected DateTime currentTime = DateTime.Now;

        protected string blobUri;

        public BaseThrottlingDetailWorker() : base()
        {
        }

        protected string GetLastestDataBase(string blobUri, string dbFilePath)
        {
            FileUtility.ForceDelete(dbFilePath);
            DownloadDBFromBlob(blobUri, dbFilePath);
            if (!IsExistTable(dbFilePath, TABLE_NAME))
            {
                FileUtility.ForceDelete(dbFilePath);
                CreateDataBaseIfNotExist(Path.GetDirectoryName(dbFilePath), Path.GetFileName(dbFilePath));
            }
            return dbFilePath;
        }

        protected static string DownloadDataBase(string blobUri, string dbFilePath)
        {
            FileUtility.ForceDelete(dbFilePath);
            DownloadDBFromBlob(blobUri, dbFilePath);
            return dbFilePath;
        }

        public abstract void UploadDatabase();

        public abstract string GetLastestDataBase();


        public override void CreateSchemaIfNotExists(IDbCommand command)
        {
            string query = string.Format(
    $@"CREATE TABLE IF NOT EXISTS {TABLE_NAME} (
    Id VARCHAR(500) PRIMARY KEY,
    MainJobId VARCHAR(500),
    SubJobId VARCHAR(500),
    TenantId VARCHAR(500),
    MSId VARCHAR(500),
    Scope VARCHAR(2000),
    Day INT,
    Hour INT,
    JobStartTimeStr VARCHAR(500),
    JobStartTime BIGINT,
    JobEndTimeStr VARCHAR(500),
    JobEndTime BIGINT,
    JobRunHours DOUBLE,
    JobRunTime BIGINT,
    TotalRquestCount BIGINT,
    SuccessRquestCount BIGINT,
    ThrottlingRquestCount BIGINT,
    ThrottlingCountEachHour DOUBLE,
    ThrottlingSleepSumTime BIGINT,
    Type INT
    );");
            command.CommandText = query;
            command.ExecuteNonQuery();
        }


        public void InsertValueToDB(params ThrottlingDetails[] destructionReports)
        {
            using (PerformanceScope pc = new PerformanceScope("ThrottlingDateWorker.InsertValueToDB", addToStatistics: true))
            {
                ExecuteWithConnectionNoUpateDicLastAccessTime(connection =>
                {
                    InternalInsertValueToDB(connection, destructionReports);
                });
            }
        }

        private void InternalInsertValueToDB(SQLiteConnection conn, IEnumerable<ThrottlingDetails> dataInfos)
        {
            using (SQLiteTransaction tr = conn.BeginTransaction())
            {
                using (var command = conn.CreateCommand())
                {
                    foreach (var dataInfo in dataInfos)
                    {
                        // 构建 INSERT 语句
                        var query = new StringBuilder();
                        query.Append($"INSERT INTO {TABLE_NAME} ");
                        query.Append(@"(Id, MainJobId, SubJobId, TenantId, MSId, Scope, Day, Hour, 
    JobStartTimeStr, JobStartTime, JobEndTimeStr, JobEndTime, JobRunHours, JobRunTime, 
    TotalRquestCount, SuccessRquestCount, ThrottlingRquestCount, ThrottlingCountEachHour, 
    ThrottlingSleepSumTime, Type) ");
                        query.Append(@"VALUES (@Id, @MainJobId, @SubJobId, @TenantId, @MSId, @Scope, @Day, @Hour, 
    @JobStartTimeStr, @JobStartTime, @JobEndTimeStr, @JobEndTime, @JobRunHours, @JobRunTime, 
    @TotalRquestCount, @SuccessRquestCount, @ThrottlingRquestCount, @ThrottlingCountEachHour, 
    @ThrottlingSleepSumTime, @Type)");

                        SQLiteParameter[] parameters = {
    new SQLiteParameter("@Id", dataInfo.Id),
    new SQLiteParameter("@MainJobId", dataInfo.MainJobId ?? (object)DBNull.Value),
    new SQLiteParameter("@SubJobId", dataInfo.SubJobId ?? (object)DBNull.Value),
    new SQLiteParameter("@TenantId", dataInfo.TenantId ?? (object)DBNull.Value),
    new SQLiteParameter("@MSId", dataInfo.MSId ?? (object)DBNull.Value),
    new SQLiteParameter("@Scope", dataInfo.Scope ?? (object)DBNull.Value),
    new SQLiteParameter("@Day", dataInfo.Type == StatisticThrottlingType.Job ? (object)DBNull.Value : dataInfo.Day),
    new SQLiteParameter("@Hour", dataInfo.Type == StatisticThrottlingType.Hour ? dataInfo.Hour : (object)DBNull.Value),
    new SQLiteParameter("@JobStartTimeStr", dataInfo.JobStartTimeStr ?? (object)DBNull.Value),
    new SQLiteParameter("@JobStartTime", dataInfo.JobStartTime),
    new SQLiteParameter("@JobEndTimeStr", dataInfo.JobEndTimeStr ?? (object)DBNull.Value),
    new SQLiteParameter("@JobEndTime", dataInfo.JobEndTime),
    new SQLiteParameter("@JobRunHours", dataInfo.JobRunHours),
    new SQLiteParameter("@JobRunTime", dataInfo.JobRunTime),
    new SQLiteParameter("@TotalRquestCount", dataInfo.TotalRquestCount),
    new SQLiteParameter("@SuccessRquestCount", dataInfo.SuccessRquestCount),
    new SQLiteParameter("@ThrottlingRquestCount", dataInfo.ThrottlingRquestCount),
    new SQLiteParameter("@ThrottlingCountEachHour", dataInfo.Type == StatisticThrottlingType.Job ? dataInfo.ThrottlingCountEachHour : (object)DBNull.Value),
    new SQLiteParameter("@ThrottlingSleepSumTime", dataInfo.ThrottlingSleepSumTime),
    new SQLiteParameter("@Type", (int)dataInfo.Type)
};
                        foreach (var para in parameters)
                        {
                            command.Parameters.Add(para);
                        }
                        command.CommandText = query.ToString();
                        command.ExecuteNonQuery();
                    }
                    tr.Commit();
                }
            }
        }

        public void Dispose()
        {
            FileUtility.ForceDelete(_dbFilePath);
        }
    }
}
