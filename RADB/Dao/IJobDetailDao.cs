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
using AvePoint.RA.SharePoint.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IJobDetailDao
    {
        IEnumerable<JMJobDetails> GetData(string reportFilePath, string slectDataSql, BaseJobDto jobInfo);
        IEnumerable<JMJobDetails> GetData(string reportFilePath, string sqlStr, BaseJobDto jobInfo, ref long lastRowId);
        IEnumerable<JMJobDetails> GetDataForTermSelection(string reportFilePath, string slectDataSql, BaseJobDto jobInfo);
        IEnumerable<JMRestoreScDetails> GetDataForSCRestoreDetail(string reportFilePath, string slectDataSql);
        IEnumerable<JMRestoreGDriveDetails> GetDataForGDRestoreDetail(string reportFilePath, string slectDataSql);
        JMJobDetails GetDataForSOSummaryDetails(string reportFilePath, string slectDataSql, BaseJobDto jobInfo); 
        JMJobDetails GetDataForRestoreSummaryDetails(string reportFilePath, string slectDataSql, BaseJobDto jobInfo);
        JMJobDetails GetDataForArchiverDedupReportSummaryDetails(string reportFilePath, string slectDataSql, BaseJobDto jobInfo);
        int GetCountForDetail(string reportFilePath, string slectDataSql, BaseJobDto jobInfo);
        long GetTotalSizeForDetail(string reportFilePath, string slectDataSql, BaseJobDto jobInfo);
        bool SaveDataIntoTable(string reportFilePath, IEnumerable<JMJobDetails> jobDetails, string insertDataSql);
        bool IsExistTable(string reportFilePath, string tableName);
        bool DeleteData(string reportFilePath, string delDataSql);
        public JMSOSummaryDetails StatisticDiscoverPrescanSummaryFromJobDatas(string reportFilePath, string slectDataSql);
    }
}
