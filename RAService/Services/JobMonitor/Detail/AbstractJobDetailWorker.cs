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
using AvePoint.GCommon.Contract.Tree.Object.Compare;
using AvePoint.RA.Api.Contract;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.JobMonitor.Util;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.JobMonitor.Detail
{
    abstract public class AbstractJobDetailWorker
    {
        public readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        internal string TABLE_NAME { get; set; }
        internal string CREATE_TABLE_SQL { get; set; }
        internal string INSERT_DATA_SQL { get; set; }
        internal string SELECT_DATA_SQL { get; set; }
        internal string SELECT_DETAIL_COUNT_SQL { get; set; }

        public readonly static object createTableLocker= new object();
        public IJobDetailDao JobDetailDao => PlatformWindsorManager.GetService<IJobDetailDao>();
        protected readonly IJobProgressDao _jobProgressDao = PlatformWindsorManager.GetService<IJobProgressDao>();

        private string mExpandedName = ".rpt";
        public string ExpandedName
        {
            get { return mExpandedName; }
            set { mExpandedName = value; }
        }
        abstract public void InsertData(IEnumerable<JMJobDetails> jobDetails, BaseJobDto jobInfo);
        abstract public IEnumerable<JMJobDetails> GetData(int PageSize, int StartPage, ref int totalCount, string conditionFilter, BaseJobDto jobInfo);
        abstract public IEnumerable<JMJobDetails> GetData(int PageSize, int StartPage, string conditionFilter, BaseJobDto jobInfo);

        public virtual IEnumerable<JMJobDetails> GetData(long pageSize, ref long lastRowId, ref long totalCount, string conditionFilter, BaseJobDto jobInfo)
        {
            IEnumerable<JMJobDetails> result = null;
            string reportFilePath = DownloadReports(jobInfo);
            TABLE_NAME = JobMonitorConstants.JOBDETAIL;
            bool isRPTExist = CheckFileExist(reportFilePath);
            bool isTableInRPTExist = JobDetailDao.IsExistTable(reportFilePath, TABLE_NAME);
            if (!isRPTExist || !isTableInRPTExist)
            {
                logger.Debug($"Report file exist: {isRPTExist}, table exist: {isTableInRPTExist}, jobInfo id: {jobInfo.Id}");
                return result;
            }

            InitGetDataSQLString(pageSize, lastRowId, conditionFilter);
            if (totalCount <= 0)
            {
                totalCount = GetCountForDetail(reportFilePath, SELECT_DETAIL_COUNT_SQL, jobInfo);
            }
            result = JobDetailDao.GetData(reportFilePath, SELECT_DATA_SQL, jobInfo, ref lastRowId);

            return result;
        }

        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="jobInfo"></param>
        /// <returns>数据库的路径</returns>
        public virtual string NeedCreateTable(BaseJobDto jobInfo)
        {
            string reportFilePath = GetReportFilePath(jobInfo);    

            lock (createTableLocker)
            {
                if (!CheckFileExist(reportFilePath) || !JobDetailDao.IsExistTable(reportFilePath, TABLE_NAME))    //文件存在  并且  表存在时  不需要新创建表
                {
                    CreateTableNew(reportFilePath);
                }
            }
            return reportFilePath;
        }

        public virtual string NeedCreateDictionary(BaseJobDto jobInfo)
        {
            string reportFilePath = GetReportFilePath(jobInfo);
            CheckAndCreateDirectory(reportFilePath);
            return reportFilePath;
        }


        /// <summary>
        /// 模块名\planId\jobId+ExpandedName来组装路径
        /// </summary>
        /// <param name="Job"></param>
        /// <returns>应该生成文件的路径</returns>
        public virtual string GetReportFilePath(BaseJobDto baseJobDto)
        {
            string rptPath = JobReportUtility.GetJobReportPath(baseJobDto, ExpandedName);
            return rptPath;
        }
        public virtual string GetReportFileDownloadPath(BaseJobDto baseJobDto)
        {
            string rptPath = JobReportUtility.GetJobReportTempPath(baseJobDto, ExpandedName);
            return rptPath;
        }

        /// <summary>
        /// 检查是否存在path路径下的同名文件。
        /// </summary>
        /// <param name="path">.rpt文件的位置</param>
        /// <returns>是否存在</returns>
        public bool CheckFileExist(string path)
        {
            return File.Exists(path);
        }
        /// <summary>
        /// 创建表
        /// </summary>
        protected void CreateTableNew(string reportFilePath)
        {
            try
            {
                CheckAndCreateDirectory(reportFilePath);
                SQLCommond.ExecuteNonQuery(reportFilePath, CREATE_TABLE_SQL);
                logger.Debug("Successfulfull to create table {0}.", TABLE_NAME);
            }
            catch (Exception ex)
            {
                logger.Error($"failed to create table,report file path:{reportFilePath},Table name:{TABLE_NAME}");
                logger.Error(ex.ToString());
            }

        }

        /// <summary>
        /// 检验是否存在目录  不存在时直接创建目录
        /// </summary>
        protected void CheckAndCreateDirectory(string reportFilePath)
        {
            FileInfo reportFile = new FileInfo(reportFilePath);
            if (!reportFile.Directory.Exists)
            {
                reportFile.Directory.Create();
                logger.Debug("Create Directory,Directory Name:",reportFile.Directory.Name);
            }

        }

        public virtual void InitGetDataSQLString(int PageSize, int StartPage, string conditionFilter)
        {
            if (string.IsNullOrEmpty(conditionFilter))
            {
                SELECT_DATA_SQL = string.Format(JobMonitorConstants.SELECT_DATA_FROM_TABLE, TABLE_NAME, PageSize, (StartPage - 1) * PageSize);
                SELECT_DETAIL_COUNT_SQL = string.Format(JobMonitorConstants.SELECT_DETAIL_COUNT_SQL, TABLE_NAME);
            }
            else
            {
                SELECT_DATA_SQL = string.Format(JobMonitorConstants.SELECT_DATA_ON_CONDITION_FROM_TABLE, TABLE_NAME, conditionFilter, PageSize, (StartPage - 1) * PageSize);
                SELECT_DETAIL_COUNT_SQL = string.Format(JobMonitorConstants.SELECT_DETAIL_COUNT_ON_CONDITION_SQL, TABLE_NAME, conditionFilter);
            }
        }

        public virtual void InitGetDataSQLString(long size, long lastRowId, string conditionFilter)
        {
            if (string.IsNullOrEmpty(conditionFilter))
            {
                SELECT_DATA_SQL = string.Format(JobMonitorConstants.PAGE_GET_DATA_BY_CURSOR, TABLE_NAME, lastRowId, size);
                SELECT_DETAIL_COUNT_SQL = string.Format(JobMonitorConstants.SELECT_DETAIL_COUNT_SQL, TABLE_NAME);
            }
            else
            {
                SELECT_DATA_SQL = string.Format(JobMonitorConstants.PAGE_GET_DATA_BY_CURSOR_AND_CONDITION, TABLE_NAME, conditionFilter, lastRowId, size);
                SELECT_DETAIL_COUNT_SQL = string.Format(JobMonitorConstants.SELECT_DETAIL_COUNT_ON_CONDITION_SQL, TABLE_NAME, conditionFilter);
            }
        }

        public int GetCountForDetail(string reportFilePath, string slectDataSql, BaseJobDto jobInfo)
        {
            return JobDetailDao.GetCountForDetail(reportFilePath, slectDataSql, jobInfo);
        }


        public bool MergeJobDetails(BaseJobDto sourceJobInfo, BaseJobDto targetJobIndo)
        {
            string sourceDBPath = DownloadReports(sourceJobInfo);
            string targetDBPath = NeedCreateDictionary(targetJobIndo);
            return JobDetailHelper.MergeJobDetails(JobMonitorConstants.JOBDETAIL, sourceDBPath, targetDBPath);
        }

        public bool InsertMainJobDetails(BaseJobDto sourceJobInfo, BaseJobDto targetJobInfo)
        {
            string sourceDBPath = DownloadReports(sourceJobInfo);
            bool isTeams = JobServiceUtility.IsTeamsJob(targetJobInfo.JobType);
            var jobProgress = _jobProgressDao.GetJobProgressBySubJobIdAsync(sourceJobInfo.Id).ExecuteAsyncTask();
            if (jobProgress is null)
            {
                jobProgress = new RMJobProgress
                {
                    SubJobID = sourceJobInfo.Id,
                    ProgressStatus = (int)JobReportUtility.ConvertJobStatusToProgressStatus((JobStatus)sourceJobInfo.Status),
                    StartTime = sourceJobInfo.StartTime,
                    FinishTime = sourceJobInfo.EndTime,
                    Scope = string.IsNullOrEmpty(sourceJobInfo.ScopeId) ? string.Empty : sourceJobInfo.ScopeId,
                };
                JobDetailHelper.InsertMainJobDetails(sourceDBPath, jobProgress, sourceJobInfo, isTeams);
                jobProgress.LastUpdatedTime = DateTime.UtcNow.Ticks;
                return _jobProgressDao.AddJobProgressAsync(jobProgress).ExecuteAsyncTask();
            }
            else
            {
                JobDetailHelper.InsertMainJobDetails(sourceDBPath, jobProgress, sourceJobInfo, isTeams);
                return _jobProgressDao.UpdateJobProgressAsync(jobProgress).ExecuteAsyncTask();
            }
        }

        public virtual JMJobDetails GetDataForJobSummaryDetails(string conditionFilter, BaseJobDto jobInfo)
        {
            throw new NotImplementedException();
        }

        public virtual void ClearJobSummaryDetails(BaseJobDto jobInfo)
        {
            throw new NotImplementedException();
        }

        public virtual bool UploadReports(BaseJobDto jobInfo)
        {
            string reportFilePath = GetReportFilePath(jobInfo);
            if (!string.IsNullOrEmpty(RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING]) && File.Exists(reportFilePath))
            {
                try
                {
                    logger.Info($"start to upload file");
                    var tenantFolderName = GetBlobFolderUrl(reportFilePath);
                    var blobName = new StringBuilder();
                    if (!string.IsNullOrEmpty(tenantFolderName))
                    {
                        blobName.Append(tenantFolderName).Append("/");
                    }
                    blobName.Append(Path.GetFileName(reportFilePath));
                    RAStorageUtil.UploadReportBlob(blobName.ToString(), reportFilePath);
                    logger.Info($"finish to upload blob name:{blobName}");
                    DeleteFile(reportFilePath);
                    logger.Info($"finish to delete file.");
                    //DeleteFile(zipFile);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error uploading file {reportFilePath}: {ex.Message}");
                    return false;
                }
            }
            else 
            {
                logger.Warn($"Report file path is empty or file does not exist: {reportFilePath}");
            }
            return true;
        }

        public bool UploadReportToTempLocation(BaseJobDto jobInfo)
        {
            string reportFilePath = GetReportFilePath(jobInfo);
            if (!string.IsNullOrEmpty(RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING]) && File.Exists(reportFilePath))
            {
                try
                {
                    logger.Info($"UploadReportToTempLocation.start to upload file");
                    var tenantFolderName = GetBlobFolderUrl(reportFilePath);
                    var blobName = new StringBuilder();
                    if (!string.IsNullOrEmpty(tenantFolderName))
                    {
                        blobName.Append(tenantFolderName).Append("/");
                        blobName.Append("Temp/");
                    }
                    blobName.Append(Path.GetFileName(reportFilePath) + "_" + DateTime.Now.ToString("yyyyMMddHHmm"));
                    RAStorageUtil.UploadReportBlob(blobName.ToString(), reportFilePath);
                    logger.Info($"UploadReportToTempLocation.finish to upload blob name:{blobName}.reportFilePath:{reportFilePath}.");
                    DeleteFile(reportFilePath);
                    logger.Info($"UploadReportToTempLocation.finish to delete file.");
                    //DeleteFile(zipFile);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error uploading file {reportFilePath}: {ex.Message}");
                    return false;
                }
            }
            else
            {
                logger.Warn($"Report file path is empty or file does not exist: {reportFilePath}");
            }
            return true;
        }
        

        public void SendReport(HBReportFileInfo hBReportInfo)
        {
           RAStorageUtil.AppendReport(hBReportInfo);
        }

        public virtual string DownloadReports(BaseJobDto jobInfo)
        {
            string tempPath = string.Empty;
            try
            {
                tempPath = GetReportFileDownloadPath(jobInfo);

                if (SQLCommond.CanConnectToReportFile(tempPath))
                {
                    return tempPath;
                }
                RAStorageUtil.DownloadReport(jobInfo);
            }
            catch (Exception e)
            {
                logger.Error($"download detail file fail,jobInfo id:{jobInfo?.Id}, error:{e}");
            }
            return tempPath;
        }

      

        /* remove 
        private string ZipDetailsFile(string reportFilePath)
        {
            logger.Debug("zip details file:{0}", reportFilePath);
            var file = new FileInfo(reportFilePath);
            //string target = Path.GetFileNameWithoutExtension(reportFilePath);
            string target= Path.ChangeExtension(reportFilePath, "zip");
            //var result = target + "_D.zip";//由于zip没有设置folder，与log的zip同名，上传前会覆盖；
            using (var zipFile = new ZipFile())
            {
                using (var stream = new FileStream(reportFilePath, FileMode.Open))
                {
                    zipFile.AddEntry(Path.GetFileName(reportFilePath), stream);
                    zipFile.Save(target);
                }
            }
            return target;
        }
        */

        internal string GetBlobFolderUrl(string filePath)
        {
            filePath = filePath.Substring(0, filePath.LastIndexOf(Path.DirectorySeparatorChar)).TrimEnd(Path.DirectorySeparatorChar);
            var jobTypeFolder = filePath.Substring(filePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            filePath = filePath.Substring(0, filePath.LastIndexOf(Path.DirectorySeparatorChar));
            var tenantAccountName = filePath.Substring(filePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            return tenantAccountName + "/" + jobTypeFolder.Replace("\\", "/");
        }

        internal void DeleteFile(string file)
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
                logger.Error($"delete file faile,file path:{file},error:{ex}.");
            }
        }
    }
}
