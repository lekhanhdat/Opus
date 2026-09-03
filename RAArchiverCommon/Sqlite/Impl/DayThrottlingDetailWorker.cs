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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Services.JobMonitor.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAArchiverCommon.Sqlite.Impl
{
    public class DayThrottlingDetailWorker : BaseThrottlingDetailWorker
    {
        private const string SUB_JOB_BLOB_CACHE_FOLDER = "SubJobCache";

        private const string MONTH_ALL_JOB_CACHE_DB_NAME = "DayThrottlingStatistic.rpt";

        private const string DAY_CACHE_DB_SPLIT_STR = "__";

        private const string DB_EXTENSION = ".rpt";

        public IJobDetailDao JobDetailDao => PlatformWindsorManager.GetService<IJobDetailDao>();

        public DayThrottlingDetailWorker(string subJobId, DateTime date)
        {
            _dbName = date.ToString("yyyy-MM-dd") + DAY_CACHE_DB_SPLIT_STR + subJobId + DB_EXTENSION;
            _dbdirPath = GetCacheSubJobFolderPath();
            _dbFilePath = GetCacheSubJobDbPath(_dbName);
            blobUri = GetSubJobDBBlobUri(_dbName);
        }

        public override void UploadDatabase()
        {
            UploadDatabase(blobUri, _dbFilePath);
        }

        public override string GetLastestDataBase()
        {
            return GetLastestDataBase(blobUri, _dbFilePath);
        }

        private static string GetMonthJobDbBlobUri(int year, int month)
        {
            return string.Join("/", TenantLocalValue.LogonGroupId, CACHE_FOLDER_NAME, year.ToString(), month.ToString(), MONTH_ALL_JOB_CACHE_DB_NAME);
        }

        private static string GetCacheMonthJobDbPath(int year, int month)
        {
            return SecurityUtils.SafeCombinePath(CACHE_FODER_PATH, CACHE_FOLDER_NAME, year.ToString(), month.ToString(), MONTH_ALL_JOB_CACHE_DB_NAME);
        }

        private static string GetSubJobDbFolderBlobUri()
        {
            return string.Join("/", TenantLocalValue.LogonGroupId, CACHE_FOLDER_NAME, SUB_JOB_BLOB_CACHE_FOLDER);
        }

        private static string GetSubJobDBBlobUri(string dbName)
        {
            return string.Join("/", GetSubJobDbFolderBlobUri(), dbName);
        }

        private static string GetCacheSubJobFolderPath()
        {
            return SecurityUtils.SafeCombinePath(CACHE_FODER_PATH, CACHE_FOLDER_NAME, SUB_JOB_BLOB_CACHE_FOLDER);
        }

        private static string GetCacheSubJobDbPath(string dbName)
        {
            return SecurityUtils.SafeCombinePath(GetCacheSubJobFolderPath(), dbName);
        }

        public static void MergeCacheBlobDayThrottlingDetail()
        {
            logger.Info($"Start merge cache bolb Day throttling for {TenantLocalValue.LogonGroupId}");
            DateTime now = DateTime.UtcNow;
            
            string cacheBlobFolderUri = GetSubJobDbFolderBlobUri();
            List<string> blobs = RAStorageUtil.GetAllReportBlobNames(cacheBlobFolderUri);

            List<string> last3DayBlobs = blobs.Where(
                blob => Path.GetFileName(blob).StartsWith(now.AddDays(-1).ToString("yyyy-MM-dd") + DAY_CACHE_DB_SPLIT_STR)
                || Path.GetFileName(blob).StartsWith(now.AddDays(-2).ToString("yyyy-MM-dd") + DAY_CACHE_DB_SPLIT_STR)
                || Path.GetFileName(blob).StartsWith(now.AddDays(-3).ToString("yyyy-MM-dd") + DAY_CACHE_DB_SPLIT_STR)).ToList();

            var monthAndBlobDic = last3DayBlobs.GroupBy(
                blob =>
                {
                    DateTime time = DateTime.Parse(Path.GetFileName(blob).Split(DAY_CACHE_DB_SPLIT_STR).First());
                    return new DateOnly(time.Year, time.Month, 1);
                });

            foreach (var monthBlobs in monthAndBlobDic)
            {
                try
                {
                    DateOnly desMonth = monthBlobs.Key;
                    string monthAllJobBlobUri = GetMonthJobDbBlobUri(desMonth.Year, desMonth.Month);
                    string monthAllJobCacheDBPath = GetCacheMonthJobDbPath(desMonth.Year, desMonth.Month);
                    if (!monthBlobs.Any())
                    {
                        logger.Info($"TeanantId:{TenantLocalValue.LogonGroupId}, day:{desMonth.ToString("yyyy-MM-dd")} not exist cached throttling statistic");
                        continue;
                    }

                    monthAllJobBlobUri = GetMonthJobDbBlobUri(desMonth.Year, desMonth.Month);
                    monthAllJobCacheDBPath = GetCacheMonthJobDbPath(desMonth.Year, desMonth.Month);
                    DownloadDataBase(monthAllJobBlobUri, monthAllJobCacheDBPath);

                    foreach (string blob in monthBlobs)
                    {
                        string cacheDbPath = GetCacheSubJobDbPath(Path.GetFileName(blob));
                        try
                        {
                            DownloadDataBase(blob, cacheDbPath);
                            if (JobDetailHelper.MergeJobDetails(JobMonitorConstants.JOBDETAIL, cacheDbPath, monthAllJobCacheDBPath))
                            {
                                RAStorageUtil.DeleteReportBlob(blob);
                            }
                        }
                        catch (Exception ex) 
                        {
                            logger.Error($"Fail sync subjob blob, blob:{blob}, e:{ex}");
                        }
                        finally
                        {
                            FileUtility.ForceDelete(cacheDbPath);
                        }
                    }
                    UploadDatabase(monthAllJobBlobUri, monthAllJobCacheDBPath);
                }
                catch(Exception e)
                {
                    logger.Error($"Fail sync month blob,desMonth:{monthBlobs.Key.ToString()}, blob:{GetMonthJobDbBlobUri(monthBlobs.Key.Year, monthBlobs.Key.Month)}, e:{e}");
                }
            }
            logger.Info($"End merge cache bolb Day throttling for {TenantLocalValue.LogonGroupId}, cost:{(DateTime.UtcNow - now).TotalMinutes} m");
        }
    }
}
