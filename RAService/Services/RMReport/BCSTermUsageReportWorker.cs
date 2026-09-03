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
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RMReport
{
    public class BCSTermUsageReportWorker : AbstractReportWorker
    {
        public BCSTermUsageReportWorker() 
        {
            InitCreateTableSQLString();
        }
        public override void SaveReportJobDatas(IEnumerable<BaseReport> jobDetails, BaseJobDto jobInfo)
        {
            InitCreateTableSQLString();
            string reportFilePath = NeedCreateTable(jobInfo);
            ReportCenterDao.SaveReportJobDatas(reportFilePath, jobDetails, this.INSERT_DATA_SQL);
        }

        public override IEnumerable<BaseReport> GetReportJobDatas(int PageSize, int StartPage, ref int totalCount, 
            string conditionFilter, BaseJobDto jobInfo, string sortKey = null, bool isAscending = true)
        {
            IEnumerable<BaseReport> result = null;
            InitCreateTableSQLString();
            string reportFilePath = DownloadReports(jobInfo);
            InitGetDataSQLString(PageSize, StartPage, conditionFilter,sortKey,isAscending);
            result = ReportCenterDao.GetReportJobDatas(reportFilePath, base.SELECT_DATA_SQL, jobInfo);
            totalCount = base.GetCountForDetail(conditionFilter, jobInfo);
            return result;
        }

        public void InitCreateTableSQLString()
        {
            TABLE_NAME = ReportConstants.ReportDETAIL;
            CREATE_TABLE_SQL = string.Format(ReportConstants.CREATE_TABLE_BCS_TERM_USAGE_REPORT, TABLE_NAME);
            INSERT_DATA_SQL = string.Format(ReportConstants.INSERT_DATA_BCS_TERM_USAGE_REPORT, TABLE_NAME);
        }

        public override ReportFilter GetReportJobFilterData(BaseJobDto jobInfo)
        {
            throw new NotImplementedException();
        }
    }
}
