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
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.JobMonitor.Detail
{
    internal class GoogleRestoreActionDetailWorker : AbstractJobDetailWorker
    {
        internal string CREATE_SUMMAYTABLE_SQL { get; set; }
        internal string INSERT_SUMMAYDATA_SQL { get; set; }
        internal string DELETE_SUMMAYDATA_SQL { get; set; }
        internal string SUMMAY_TABLE_NAME { get; set; }

        public override IEnumerable<JMJobDetails> GetData(int PageSize, int StartPage, ref int totalCount, string conditionFilter, BaseJobDto jobInfo)
        {
            string reportFilePath = DownloadReports(jobInfo);
            TABLE_NAME = JobMonitorConstants.JOBDETAIL;
            InitGetDataSQLString(PageSize, StartPage, conditionFilter);
            totalCount = base.GetCountForDetail(reportFilePath, base.SELECT_DETAIL_COUNT_SQL, jobInfo);
            return GetData(PageSize, StartPage, conditionFilter, jobInfo);
        }
        public override IEnumerable<JMJobDetails> GetData(int PageSize, int StartPage, string conditionFilter, BaseJobDto jobInfo)
        {
            IEnumerable<JMJobDetails> result = null;
            string reportFilePath = DownloadReports(jobInfo);
            TABLE_NAME = JobMonitorConstants.JOBDETAIL;
            InitGetDataSQLString(PageSize, StartPage, conditionFilter);
            bool isRPTExist = CheckFileExist(reportFilePath);
            bool isTableInRPTExist = JobDetailDao.IsExistTable(reportFilePath, TABLE_NAME);
            if (!isRPTExist || !isTableInRPTExist)
            {
                logger.Debug("about {0} database exist:{1},table exist{2}", jobInfo.Id, isRPTExist, isTableInRPTExist);
                return result;
            }
            result = JobDetailDao.GetData(reportFilePath, base.SELECT_DATA_SQL, jobInfo);
            return result;
        }

        public override void InsertData(IEnumerable<JMJobDetails> jobDetails, BaseJobDto jobInfo)
        {
            var details = jobDetails.Where(item => !(item is JMRestoreSummaryDetails));
            if (details != null && details.Count() > 0)
            {
                InitCreateTableSQLString();
                string reportFilePath = NeedCreateTable(jobInfo);
                JobDetailDao.SaveDataIntoTable(reportFilePath, details, this.INSERT_DATA_SQL);
            }

            var summaryDetails = jobDetails.Where(item => item is JMRestoreSummaryDetails);
            if (summaryDetails != null && summaryDetails.Count() > 0)
            {
                InitCreateSummaryTableSQLString();

                string reportFilePath = GetReportFilePath(jobInfo);

                lock (createTableLocker)
                {
                    if (!CheckFileExist(reportFilePath) || !JobDetailDao.IsExistTable(reportFilePath, SUMMAY_TABLE_NAME))
                    {
                        try
                        {
                            CheckAndCreateDirectory(reportFilePath);
                            SQLCommond.ExecuteNonQuery(reportFilePath, CREATE_SUMMAYTABLE_SQL);
                            logger.Debug("Successfulfull to create table {0}.", SUMMAY_TABLE_NAME);
                        }
                        catch (Exception ex)
                        {
                            logger.Error("failed to create table {0}.", SUMMAY_TABLE_NAME);
                            logger.Error(ex.ToString());
                        }
                    }
                }
                JobDetailDao.DeleteData(reportFilePath, DELETE_SUMMAYDATA_SQL);
                JobDetailDao.SaveDataIntoTable(reportFilePath, summaryDetails, this.INSERT_SUMMAYDATA_SQL);
            }
        }
        public void InitCreateTableSQLString()
        {
            TABLE_NAME = JobMonitorConstants.JOBDETAIL;
            CREATE_TABLE_SQL = string.Format(JobMonitorConstants.CREATE_TABLE_Google_Restore_Report, TABLE_NAME);
            INSERT_DATA_SQL = string.Format(JobMonitorConstants.INSERT_DATA_Google_Restore_Report, TABLE_NAME);
        }

        public void InitCreateSummaryTableSQLString()
        {
            SUMMAY_TABLE_NAME = JobMonitorConstants.JOBSUMMAYDETAIL;
            CREATE_SUMMAYTABLE_SQL = string.Format(JobMonitorConstants.CREATE_TABLE_Restore_SUMMARYReport, SUMMAY_TABLE_NAME);
            INSERT_SUMMAYDATA_SQL = string.Format(JobMonitorConstants.INSERT_DATA_Restore_SUMMARYReport, SUMMAY_TABLE_NAME);
            DELETE_SUMMAYDATA_SQL = string.Format(JobMonitorConstants.DELETE_DATA_Restore_SUMMARYReport, SUMMAY_TABLE_NAME);
        }

        public override JMJobDetails GetDataForJobSummaryDetails(string conditionFilter, BaseJobDto jobInfo)
        {
            JMJobDetails result = new JMRestoreSummaryDetails() { ActionStatistics = new List<ActionStatistics>() };
            string reportFilePath = DownloadReports(jobInfo);
            TABLE_NAME = JobMonitorConstants.JOBSUMMAYDETAIL;
            bool isRPTExist = CheckFileExist(reportFilePath);
            logger.Info("filePath:{0},file exist:{1}", reportFilePath, isRPTExist);
            bool isTableInRPTExist = JobDetailDao.IsExistTable(reportFilePath, TABLE_NAME);
            if (!isRPTExist || !isTableInRPTExist)
            {
                logger.Debug("about {0} database exist:{1},table exist{2}", jobInfo.Id, isRPTExist, isTableInRPTExist);
                return result;
            }
            result = JobDetailDao.GetDataForRestoreSummaryDetails(reportFilePath, "select * from JobSummaryDetail", jobInfo);
            return result;
        }
    }
}