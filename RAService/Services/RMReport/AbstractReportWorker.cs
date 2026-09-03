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
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AvePoint.RA.Service.Services.RMReport
{
    abstract public class AbstractReportWorker
    {
        private readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        internal string TABLE_NAME { get; set; }
        internal string CREATE_TABLE_SQL { get; set; }
        internal string INSERT_DATA_SQL { get; set; }
        internal string SELECT_DATA_SQL { get; set; }
        internal string SELECT_REPORT_COUNT_SQL { get; set; }

        public IReportCenterDao ReportCenterDao => PlatformWindsorManager.GetService<IReportCenterDao>();
        private string mExpandedName = ".rpt";
        public string ExpandedName
        {
            get { return mExpandedName; }
            set { mExpandedName = value; }
        }
        abstract public void SaveReportJobDatas(IEnumerable<BaseReport> baseJobs, BaseJobDto jobInfo);
        abstract public IEnumerable<BaseReport> GetReportJobDatas(int PageSize, int StartPage, ref int totalCount,
            string conditionFilter, BaseJobDto jobInfo, string sortKey = null, bool isAscending = true);

        abstract public ReportFilter GetReportJobFilterData(BaseJobDto jobInfo);

        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="jobInfo"></param>
        /// <returns>数据库的路径</returns>
        public virtual string NeedCreateTable(BaseJobDto jobInfo)
        {

            logger.Info("Set report file path.");
            string reportFilePath = GetReportFilePath(jobInfo);

            if (!CheckFileExist(reportFilePath) || !ReportCenterDao.IsExistTable(reportFilePath, TABLE_NAME))    //文件存在  并且  表存在时  不需要新创建表
            {

                CreateTableNew(reportFilePath);
            }
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
            FileInfo file = new FileInfo(path);
            return file.Exists;
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
                logger.Error("failed to create table {0}.", TABLE_NAME);
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
                logger.Debug("Create Directory:", reportFile.Directory.Name);
            }

        }

        public void InitGetDataSQLString(int PageSize, int StartPage, string conditionFilter,
            string orderbyKey = null, bool isAscending = true)
        {
            string sortOrderString = isAscending ? SortOrder.ASC.ToString() : SortOrder.DESC.ToString();
            if (string.IsNullOrEmpty(conditionFilter))
            {
                SELECT_REPORT_COUNT_SQL = string.Format(ReportConstants.SELECT_REPORT_COUNT_SQL, TABLE_NAME);
                if (string.IsNullOrEmpty(orderbyKey))
                {
                    SELECT_DATA_SQL = string.Format(ReportConstants.SELECT_DATA_FROM_TABLE,
                        TABLE_NAME, PageSize, (StartPage - 1) * PageSize);
                }
                else
                {
                    if(orderbyKey.Equals("Size", StringComparison.OrdinalIgnoreCase))
                    {
                        SELECT_DATA_SQL = string.Format(ReportConstants.SELECT_DATA_ORDERBY_CAST_INT_FROM_TABLE,
                        TABLE_NAME, orderbyKey, sortOrderString, PageSize, (StartPage - 1) * PageSize);
                    }
                    else
                    {
                        SELECT_DATA_SQL = string.Format(ReportConstants.SELECT_DATA_ORDERBY_LOWER_FROM_TABLE,
                            TABLE_NAME, orderbyKey, sortOrderString, PageSize, (StartPage - 1) * PageSize);
                    }
                }
            }
            else
            {
                SELECT_REPORT_COUNT_SQL = string.Format(ReportConstants.SELECT_REPORT_COUNT_ON_CONDITION_SQL, TABLE_NAME, conditionFilter);
                if (string.IsNullOrEmpty(orderbyKey))
                {
                    SELECT_DATA_SQL = string.Format(ReportConstants.SELECT_DATA_ON_CONDITION_FROM_TABLE,
                        TABLE_NAME, conditionFilter, PageSize, (StartPage - 1) * PageSize);
                }
                else
                {
                    SELECT_DATA_SQL = string.Format(ReportConstants.SELECT_DATA_ON_CONDITION_ORDERBY_FROM_TABLE,
                        TABLE_NAME, conditionFilter, orderbyKey, sortOrderString, PageSize, (StartPage - 1) * PageSize);
                }
            }
        }

        public void InitGetDataCountSQLString(string conditionFilter)
        {
            if (string.IsNullOrEmpty(conditionFilter))
            {
                SELECT_REPORT_COUNT_SQL = string.Format(ReportConstants.SELECT_REPORT_COUNT_SQL, TABLE_NAME);
            }
            else
            {
                SELECT_REPORT_COUNT_SQL = string.Format(ReportConstants.SELECT_REPORT_COUNT_ON_CONDITION_SQL, TABLE_NAME, conditionFilter);
            }
        }

        public int GetCountForDetail(string conditionFilter, BaseJobDto jobInfo)
        {
            int jobReportTotalCount = 0;
            string reportFilePath = DownloadReports(jobInfo);
            InitGetDataCountSQLString(conditionFilter);
            jobReportTotalCount = ReportCenterDao.GetCountForReport(reportFilePath, this.SELECT_REPORT_COUNT_SQL, jobInfo);
            return jobReportTotalCount;
        }

        private enum SortOrder
        {
            ASC = 0,
            DESC
        }

        public string DownloadReports(BaseJobDto jobInfo)
        {
            string tempPath = string.Empty;
            try
            {
                tempPath = GetReportFileDownloadPath(jobInfo);

                if (SQLCommond.CanConnectToReportFile(tempPath))
                {
                    logger.Debug("get report from local cache.");
                    return tempPath;
                }
                RAStorageUtil.DownloadReport(jobInfo);
            }
            catch (Exception e)
            {
                logger.Error("download report file :{0} error:{1},report is :{1}", e.ToString(), tempPath);
            }
            return tempPath;
        }
    }
}
