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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.SharePoint.Object;
using AvePoint.RA.SharePoint.RestoreReport.Constant;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RestoreReport.Worker
{
    public class RestoreReportScDetailWorker
    {

        public readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        internal string TABLE_NAME = RestoreReportConstant.SC_TABLE_NAME;
        internal string CREATE_TABLE_SQL { get; set; }
        internal string INSERT_DATA_SQL { get; set; }
        internal string SELECT_DATA_SQL { get; set; }
        internal string SELECT_DETAIL_COUNT_SQL { get; set; }
        internal string DELETE_DATA_SQL { get; set; }

        public readonly static object createTableLocker = new object();
        public IJobDetailDao JobDetailDao => PlatformWindsorManager.GetService<IJobDetailDao>();

        public IEnumerable<JMJobDetails> GetData(int PageSize, int StartPage, ref int totalCount, string conditionFilter, string scUrl)
        {
            IEnumerable<JMJobDetails> result = null;
            string reportFilePath = DownloadReports(scUrl);
            InitGetDataSQLString(PageSize, StartPage, conditionFilter);
            bool isRPTExist = CheckFileExist(reportFilePath);
            bool isTableInRPTExist = JobDetailDao.IsExistTable(reportFilePath, TABLE_NAME);
            if (!isRPTExist || !isTableInRPTExist)
            {
                logger.Debug("about {0} database exist:{1},table exist{2}", scUrl, isRPTExist, isTableInRPTExist);
                return result;
            }
            result = JobDetailDao.GetDataForSCRestoreDetail(reportFilePath, SELECT_DATA_SQL);
            totalCount = GetCountForDetail(reportFilePath, SELECT_DETAIL_COUNT_SQL, new BaseJobDto());
            return result;
        }

        public void DeleteData(string filterCondition, string scUrl)
        {
            InitDeleteDataSQLString(filterCondition);
            string reportFilePath = GetReportFileDownloadPath(scUrl);
            JobDetailDao.DeleteData(reportFilePath, DELETE_DATA_SQL);
        }

        public void InitDeleteDataSQLString(string filterCondition)
        {
            if (String.IsNullOrEmpty(filterCondition))
            {
                throw new Exception("delete condition can not be empty");
            }
            TABLE_NAME = RestoreReportConstant.SC_TABLE_NAME;
            DELETE_DATA_SQL = string.Format(RestoreReportConstant.DELETE_DATA_FROM_SC_TABLE_SQL, TABLE_NAME, filterCondition);
        }

        public void InsertData(IEnumerable<JMJobDetails> jobDetails, string scUrl)
        {
            var details = jobDetails.Where(item => item is JMRestoreScDetails);
            if (details != null && details.Count() > 0)
            {
                InitCreateTableSQLString();
                string reportFilePath = NeedCreateTable(scUrl);
                if(!JobDetailDao.SaveDataIntoTable(reportFilePath, details, INSERT_DATA_SQL))
                {
                    throw new Exception($@"Fail insert data into sqllite, job details :{jobDetails}, scUrl:{scUrl}, report FilePath:{reportFilePath}");
                }
            }
        }

        public void InitCreateTableSQLString()
        {
            TABLE_NAME = RestoreReportConstant.SC_TABLE_NAME;
            CREATE_TABLE_SQL = string.Format(RestoreReportConstant.CREATE_SC_TABLE_SQL, TABLE_NAME);
            INSERT_DATA_SQL = string.Format(RestoreReportConstant.INSERT_DATA_INTO_SC_TABLE_SQL, TABLE_NAME);
        }

        public string GetReportFilePath(string scUrl)
        {
            string rptPath = JobReportUtility.GetRestoreReportJobScDetailPath(scUrl);
            return rptPath;
        }

        public string GetReportFileDownloadPath(string scUrl)
        {
            return GetReportFilePath(scUrl);
        }

        public string DownloadReports(string scUrl)
        {
            string tempPath = string.Empty;
            try
            {
                tempPath = GetReportFileDownloadPath(scUrl);

                if (!SQLCommond.CanConnectToReportFile(tempPath))
                {
                    RAStorageUtil.DownloadRestoreScDetail(scUrl);
                }
            }
            catch (Exception e)
            {
                logger.Error($"download detail file,site collection url:{scUrl}, error:{e}");
                return string.Empty;
            }
            return tempPath;
        }

        public bool UploadReports(string scUrl)
        {
            string reportFilePath = GetReportFilePath(scUrl);
            if (!string.IsNullOrEmpty(RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING]) && File.Exists(reportFilePath))
            {
                logger.Info($"start to upload file,url :{scUrl}");
                var blobName = JobReportUtility.GetRestoreReportJobScDetailUri(scUrl);
                RAStorageUtil.UploadReportBlob(blobName, reportFilePath);
                logger.Info($"finish to upload blob name:{blobName},url :{scUrl}");
            }
            return true;
        }



        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="jobInfo"></param>
        /// <returns>数据库的路径</returns>
        public virtual string NeedCreateTable(string scUrl)
        {

            logger.Info("Set report file path.");
            string reportFilePath = GetReportFilePath(scUrl);

            lock (createTableLocker)
            {
                if (!CheckFileExist(reportFilePath) || !JobDetailDao.IsExistTable(reportFilePath, TABLE_NAME))    //文件存在  并且  表存在时  不需要新创建表
                {
                    CreateTableNew(reportFilePath);
                }
            }
            return reportFilePath;
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
                logger.Debug($"Successfulfull to create table {TABLE_NAME},report file path:{reportFilePath}.");
            }
            catch (Exception ex)
            {
                logger.Error($"failed to create table ,report file path:{reportFilePath}.");
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
                logger.Debug($"Create Directory,Directory Name:{reportFile.Directory.Name}, report file path:{reportFilePath}");
            }

        }

        public void InitGetDataSQLString(int PageSize, int StartPage, string conditionFilter)
        {
            if (string.IsNullOrEmpty(conditionFilter))
            {
                SELECT_DATA_SQL = string.Format(RestoreReportConstant.GET_DATA_FROM_SC_TABLE_SQL, TABLE_NAME, PageSize, (StartPage - 1) * PageSize);
                SELECT_DETAIL_COUNT_SQL = string.Format(RestoreReportConstant.GET_COUNT_FROM_SC_TABLE_SQL, TABLE_NAME);
            }
            else
            {
                SELECT_DATA_SQL = string.Format(RestoreReportConstant.GET_DATA_FROM_SC_TABLE_ON_CONDITION_SQL, TABLE_NAME, conditionFilter, PageSize, (StartPage - 1) * PageSize);
                SELECT_DETAIL_COUNT_SQL = string.Format(RestoreReportConstant.GET_COUNT_FROM_SC_TABLE_ON_CONDITION_SQL, TABLE_NAME, conditionFilter);
            }
        }

        public int GetCountForDetail(string reportFilePath, string slectDataSql, BaseJobDto jobInfo)
        {
            return JobDetailDao.GetCountForDetail(reportFilePath, slectDataSql, jobInfo);
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
                logger.Error($"delete file,file path :{file}, faile.error:{ex}");
            }
        }

    }
}
