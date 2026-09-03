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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.SharePoint.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RAGoogle.Report.RestoreReport
{
    public class GDriveRestoreReportDetailWorker
    {
        private readonly RALogger _logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        internal string TABLE_NAME = GDriveRestoreReportConstant.GD_TABLE_NAME;
        internal string CREATE_TABLE_SQL { get; set; }
        internal string INSERT_DATA_SQL { get; set; }
        internal string SELECT_DATA_SQL { get; set; }
        internal string SELECT_DETAIL_COUNT_SQL { get; set; }
        internal string DELETE_DATA_SQL { get; set; }

        public readonly static object createTableLocker = new object();
        private IJobDetailDao _jobDetailDao => PlatformWindsorManager.GetService<IJobDetailDao>();
        public IEnumerable<JMJobDetails> GetData(int PageSize, int StartPage, ref int totalCount, string conditionFilter, string driveId)
        {
            IEnumerable<JMJobDetails> result = null;
            string reportFilePath = DownloadReports(driveId);
            InitGetDataSQLString(PageSize, StartPage, conditionFilter);
            bool isRPTExist = CheckFileExist(reportFilePath);
            bool isTableInRPTExist = _jobDetailDao.IsExistTable(reportFilePath, TABLE_NAME);
            if (!isRPTExist || !isTableInRPTExist)
            {
                _logger.Debug("about {0} database exist:{1},table exist{2}", driveId, isRPTExist, isTableInRPTExist);
                return result;
            }
            result = _jobDetailDao.GetDataForGDRestoreDetail(reportFilePath, SELECT_DATA_SQL);
            totalCount = GetCountForDetail(reportFilePath, SELECT_DETAIL_COUNT_SQL, new BaseJobDto());
            return result;
        }
        public string DownloadReports(string driveId)
        {
            string tempPath = string.Empty;
            try
            {
                tempPath = GetReportFileDownloadPath(driveId);

                if (!SQLCommond.CanConnectToReportFile(tempPath))
                {
                    RAStorageUtil.DownloadRestoreGDDetail(driveId);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"download detail file,drive Name:{driveId}, error:{e}");
                return string.Empty;
            }
            return tempPath;
        }
        public bool UploadReports(string driveId)
        {
            string reportFilePath = GetReportFilePath(driveId);
            if (!string.IsNullOrEmpty(RMGlobalConfiguration.StorageConfig[AvePoint.RA.Contract.Configurations.RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING]) && File.Exists(reportFilePath))
            {
                _logger.Info($"start to upload file,drive name :{driveId}");
                var blobName = JobReportUtility.GetRestoreReportJobGDDetailUri(driveId);
                RAStorageUtil.UploadReportBlob(blobName, reportFilePath);
                _logger.Info($"finish to upload blob name:{blobName},url :{driveId}");
            }
            return true;
        }
        public string GetReportFileDownloadPath(string driveId)
        {
            return GetReportFilePath(driveId);
        }
        public void DeleteData(string filterCondition, string driveId)
        {
            InitDeleteDataSQLString(filterCondition);
            string reportFilePath = GetReportFileDownloadPath(driveId);
            _jobDetailDao.DeleteData(reportFilePath, DELETE_DATA_SQL);
        }
        public void InitDeleteDataSQLString(string filterCondition)
        {
            if (String.IsNullOrEmpty(filterCondition))
            {
                throw new Exception("delete condition can not be empty");
            }
            TABLE_NAME = GDriveRestoreReportConstant.GD_TABLE_NAME;
            DELETE_DATA_SQL = string.Format(GDriveRestoreReportConstant.DELETE_DATA_FROM_GD_TABLE_SQL, TABLE_NAME, filterCondition);
        }
        public void InsertData(IEnumerable<JMJobDetails> jobDetails, string driveId)
        {
            var details = jobDetails.Where(item => item is JMRestoreGDriveDetails);
            if (details != null && details.Count() > 0)
            {
                InitCreateTableSQLString();
                string reportFilePath = NeedCreateTable(driveId);
                if (!_jobDetailDao.SaveDataIntoTable(reportFilePath, details, INSERT_DATA_SQL))
                {
                    throw new Exception($@"Fail insert data into sqllite, job details :{jobDetails}, drive name:{driveId}, report FilePath:{reportFilePath}");
                }
            }
        }
        public virtual string NeedCreateTable(string driveId)
        {

            _logger.Info("Set report file path.");
            string reportFilePath = GetReportFilePath(driveId);

            lock (createTableLocker)
            {
                if (!CheckFileExist(reportFilePath) || !_jobDetailDao.IsExistTable(reportFilePath, TABLE_NAME))    //文件存在  并且  表存在时  不需要新创建表
                {
                    CreateTableNew(reportFilePath);
                }
            }
            return reportFilePath;
        }
        public void InitCreateTableSQLString()
        {
            TABLE_NAME = GDriveRestoreReportConstant.GD_TABLE_NAME;
            CREATE_TABLE_SQL = string.Format(GDriveRestoreReportConstant.CREATE_GD_TABLE_SQL, TABLE_NAME);
            INSERT_DATA_SQL = string.Format(GDriveRestoreReportConstant.INSERT_DATA_INTO_GD_TABLE_SQL, TABLE_NAME);
        }
        public string GetReportFilePath(string driveId)
        {
            string rptPath = JobReportUtility.GetRestoreReportJobGDDetailPath(driveId);
            return rptPath;
        }
        public void InitGetDataSQLString(int PageSize, int StartPage, string conditionFilter)
        {
            if (string.IsNullOrEmpty(conditionFilter))
            {
                SELECT_DATA_SQL = string.Format(GDriveRestoreReportConstant.GET_DATA_FROM_GD_TABLE_SQL, TABLE_NAME, PageSize, (StartPage - 1) * PageSize);
                SELECT_DETAIL_COUNT_SQL = string.Format(GDriveRestoreReportConstant.GET_COUNT_FROM_GD_TABLE_SQL, TABLE_NAME);
            }
            else
            {
                SELECT_DATA_SQL = string.Format(GDriveRestoreReportConstant.GET_DATA_FROM_GD_TABLE_ON_CONDITION_SQL, TABLE_NAME, conditionFilter, PageSize, (StartPage - 1) * PageSize);
                SELECT_DETAIL_COUNT_SQL = string.Format(GDriveRestoreReportConstant.GET_COUNT_FROM_GD_TABLE_ON_CONDITION_SQL, TABLE_NAME, conditionFilter);
            }
        }
        public bool CheckFileExist(string path)
        {
            return File.Exists(path);
        }
        protected void CreateTableNew(string reportFilePath)
        {
            try
            {
                CheckAndCreateDirectory(reportFilePath);
                SQLCommond.ExecuteNonQuery(reportFilePath, CREATE_TABLE_SQL);
                _logger.Debug($"Successfulfull to create table {TABLE_NAME},report file path:{reportFilePath}.");
            }
            catch (Exception ex)
            {
                _logger.Error($"failed to create table ,report file path:{reportFilePath}.");
                _logger.Error(ex.ToString());
            }

        }
        protected void CheckAndCreateDirectory(string reportFilePath)
        {
            FileInfo reportFile = new FileInfo(reportFilePath);
            if (!reportFile.Directory.Exists)
            {
                reportFile.Directory.Create();
                _logger.Debug($"Create Directory,Directory Name:{reportFile.Directory.Name}, report file path:{reportFilePath}");
            }

        }
        public int GetCountForDetail(string reportFilePath, string slectDataSql, BaseJobDto jobInfo)
        {
            return _jobDetailDao.GetCountForDetail(reportFilePath, slectDataSql, jobInfo);
        }
    }
}
