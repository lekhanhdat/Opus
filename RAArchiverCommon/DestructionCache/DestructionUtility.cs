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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Tenant;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAArchiverCommon.DestructionCache
{
    public class DestructionFactory
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(DestructionFactory));
        private readonly static object mLock = new object();
        private static ConcurrentDictionary<(string nodeId, string jobId), DestructionUtility> concurrentDictionary = new ConcurrentDictionary<(string nodeId, string jobId), DestructionUtility>();
        public static DestructionUtility GetInstance(string nodeId, string jobId)
        {
            if (concurrentDictionary.ContainsKey((nodeId, jobId)))
            {
                return concurrentDictionary[(nodeId, jobId)];
            }
            else
            {
                lock (mLock)
                {
                    if (!concurrentDictionary.ContainsKey((nodeId, jobId)))
                    {
                        var utility = new DestructionUtility(nodeId, jobId);
                        concurrentDictionary.TryAdd((nodeId, jobId), utility);
                    }
                    return concurrentDictionary[(nodeId, jobId)];
                }
            }
        }

        public static void Dispose(string nodeId, string jobId)
        {
            if (concurrentDictionary.ContainsKey((nodeId, jobId)))
            {
                try
                {
                    var utility = concurrentDictionary[(nodeId, jobId)];
                    utility.Dispose();
                    concurrentDictionary.Remove((nodeId, jobId), out DestructionUtility u);
                }
                catch (Exception e)
                {
                    logger.Warn($"Error occurred while dispose. NodeId:{nodeId}");
                }
            }
        }

        public static void UploadToStorage()
        {
            var keys = concurrentDictionary.Keys.ToList();
            foreach (var key in keys)
            {
                try
                {
                    logger.Info($"Start to upload destruction file for node:{key}");
                    concurrentDictionary[key].UploadToStorage();
                    Dispose(key.nodeId, key.jobId);
                    logger.Info($"Finish upload destruction file for node:{key}");
                }
                catch (Exception e)
                {
                    logger.Error($"Error occurred while uploading destruction file for node:{key} Error:{e.ToString()}");
                }
            }
        }
    }
    /// <summary>
    /// 用于记录被Disposal job删除的数据，存储路径为DestructionCache/TenantGroupId/NodeId/Cache收集开始时间_Cache收集结束时间_JobId.rpt
    ///NodeId=>SPO/OneDrive site collection id, Exhange online mailbox id(in place archive)
    ///运行report job时，根据Cache收集开始时间,Cache收集结束时间下载所需的rpt文件
    /// </summary>
    public class DestructionUtility
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(DestructionUtility));
        private string nodeId = string.Empty;
        private string jobId = string.Empty;
        private DestructionSqliteOperation destructionSqliteOperation = null;
        private string DestructionCacheFolder = "DestructionCache";
        //used to write
        public DestructionUtility(string nodeId, string jobId)
        {
            this.nodeId = nodeId;
            this.jobId = jobId;
            destructionSqliteOperation = new DestructionSqliteOperation(GenerateLocalTempFilePath(nodeId), DateTime.UtcNow.Ticks.ToString());
        }

        //used to read
        public DestructionUtility(string rptPath)
        {
            var path = Path.GetDirectoryName(rptPath);
            var name = Path.GetFileName(rptPath);
            destructionSqliteOperation = new DestructionSqliteOperation(path, name);
        }

        public void InsertValueToDB(List<DestructionReport> destructionReports)
        {
            try
            {
                destructionSqliteOperation.InsertValueToDB(destructionReports);
            }
            catch (Exception e)
            {
                logger.Warn($"Error occured while inserting data to destruction report. Error:{e.ToString()}");
            }
        }

        public List<DestructionReport> SelectValuesFromDB(int offset, int pageSize)
        {
            return destructionSqliteOperation.SelectValuesFromDB(offset, pageSize);
        }

        public void UploadToStorage()
        {
            //update destruction file count
            var count = destructionSqliteOperation.GetTotalCount();
            if (count > 0)
            {
                logger.Info($"Found {count} destruction cache, add to dashboard.");
            }
            else
            {
                logger.Info("No destruction cache found.");
            }
            //upload to storage
            string reportFilePath = destructionSqliteOperation.GetFilePath();
            if (!string.IsNullOrEmpty(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING]) && File.Exists(reportFilePath) && count > 0)
            {
                logger.Info($"start to upload destruction cache file");
                var tenantFolderName = GenerateAzureTempFilePath(nodeId);
                var blobName = new StringBuilder();
                if (!string.IsNullOrEmpty(tenantFolderName))
                {
                    blobName.Append(tenantFolderName).Append("/");
                }
                //starttime_endtime_jobId.rpt
                string fileName = Path.GetFileName(reportFilePath) + "_" + DateTime.UtcNow.Ticks.ToString() + "_" + jobId + ".rpt";
                blobName.Append(fileName);
                RAStorageUtil.UploadReportBlob(blobName.ToString(), reportFilePath);
                logger.Info($"finish to upload blob name:{blobName}");
                DeleteFile(reportFilePath);
                logger.Info($"finish to delete destruction cache file.");
            }
            Dispose();
        }

        public string DownloadCacheFromStorage(string nodeId, DateTime startUtcTime, DateTime endUtcTime)
        {
            string reportFilePath = Path.Combine(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.REPORT_TEMP_FOLDER], DestructionCacheFolder, Guid.NewGuid().ToString());
            if (!Directory.Exists(reportFilePath))
            {
                Directory.CreateDirectory(reportFilePath);
            }
            if (!string.IsNullOrEmpty(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING]))
            {
                logger.Info($"start to upload destruction cache file");
                RAStorageUtil.DownloadAllBlobsInContainer(GenerateAzureTempFilePath(nodeId), reportFilePath, startUtcTime, endUtcTime);
                logger.Info($"finish to upload blob name:");
            }
            return reportFilePath;
        }

        private void DeleteFile(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                logger.Error("delete file failed." + ex);
            }
        }


        private string GenerateLocalTempFilePath(string nodeId)
        {
            return SecurityUtils.SafeCombinePath(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.REPORT_TEMP_FOLDER], DestructionCacheFolder, TenantLocalValue.LogonGroupId, nodeId.ToString());
        }

        private string GenerateAzureTempFilePath(string nodeId)
        {
            return SecurityUtils.SafeCombinePath(DestructionCacheFolder, TenantLocalValue.LogonGroupId, nodeId.ToString());
        }

        public void Dispose()
        {
            try
            {
                string reportFilePath = destructionSqliteOperation.GetFilePath();
                if (File.Exists(reportFilePath))
                {
                    File.Delete(reportFilePath);
                }
            }
            catch(Exception e)
            {
                logger.Error($"error occured when Dispose2,error:{e}");
            }            
        }
    }
}
