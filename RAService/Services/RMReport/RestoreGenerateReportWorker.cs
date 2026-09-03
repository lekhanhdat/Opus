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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RMReport
{
    public class RestoreGenerateReportWorker : AbstractReportWorker
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(RestoreGenerateReportWorker));
        public RestoreGenerateReportWorker()
        {
            InitCreateTableSQLString();
        }
        public override void SaveReportJobDatas(IEnumerable<BaseReport> jobDetails, BaseJobDto jobInfo)
        {
            InitCreateTableSQLString();
            string reportFilePath = NeedCreateTable(jobInfo);
            ReportCenterDao.SaveReportJobDatas(reportFilePath, jobDetails, this.INSERT_DATA_SQL);
            UploadReports(jobInfo, reportFilePath);
        }

        public override IEnumerable<BaseReport> GetReportJobDatas(int PageSize, int StartPage, ref int totalCount,
            string conditionFilter, BaseJobDto jobInfo, string sortKey = null, bool isAscending = true)
        {
            IEnumerable<BaseReport> result = null;
            InitCreateTableSQLString();
            string reportFilePath = DownloadReports(jobInfo);
            InitGetDataSQLString(PageSize, StartPage, conditionFilter, sortKey, isAscending);
            result = ReportCenterDao.GetReportJobDatas(reportFilePath, base.SELECT_DATA_SQL, jobInfo);
            totalCount = base.GetCountForDetail(conditionFilter, jobInfo);
            return result;
        }

        public void InitCreateTableSQLString()
        {
            TABLE_NAME = ReportConstants.ReportDETAIL;
            CREATE_TABLE_SQL = string.Format(ReportConstants.CREATE_RESTORE_REPORT_TABLE_SQL, TABLE_NAME);
            INSERT_DATA_SQL = string.Format(ReportConstants.INSERT_DATA_INTO_RESTORE_REPORT_TABLE_SQL, TABLE_NAME);
        }
        private bool UploadReports(BaseJobDto jobInfo,string reportFilePath)
        {
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
                    //DeleteFile(reportFilePath);
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

        private string GetBlobFolderUrl(string filePath)
        {
            filePath = filePath.Substring(0, filePath.LastIndexOf(Path.DirectorySeparatorChar)).TrimEnd(Path.DirectorySeparatorChar);
            var jobTypeFolder = filePath.Substring(filePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            filePath = filePath.Substring(0, filePath.LastIndexOf(Path.DirectorySeparatorChar));
            var tenantAccountName = filePath.Substring(filePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            return tenantAccountName + "/" + jobTypeFolder.Replace("\\", "/");
        }
        public override ReportFilter GetReportJobFilterData(BaseJobDto jobInfo)
        {
            throw new NotImplementedException();
        }
    }
}
