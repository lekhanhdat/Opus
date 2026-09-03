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
    internal class ClientSPAuditReportWorker : AbstractReportWorker
    {
        private const string USERNAMECOLUMN = "UserName";

        public ClientSPAuditReportWorker() 
        {
            InitCreateTableSQLString();
        }
        public override IEnumerable<BaseReport> GetReportJobDatas(int PageSize, int StartPage, ref int totalCount, string conditionFilter, BaseJobDto jobInfo, string sortKey = null, bool isAscending = true)
        {
            if (string.IsNullOrEmpty(sortKey))
            {
                sortKey = "Occurred";
                isAscending = true;
            }
            IEnumerable<BaseReport> result = null;
            InitCreateTableSQLString();
            string reportFilePath = DownloadReports(jobInfo);
            TABLE_NAME = ReportConstants.ReportDETAIL;
            InitGetDataSQLString(PageSize, StartPage, conditionFilter, sortKey, isAscending);
            result = ReportCenterDao.GetReportJobDatas(reportFilePath, base.SELECT_DATA_SQL, jobInfo);
            totalCount = base.GetCountForDetail(conditionFilter, jobInfo);
            return result;
        }

        public override void SaveReportJobDatas(IEnumerable<BaseReport> baseJobs, BaseJobDto jobInfo)
        {
            InitCreateTableSQLString();
            string reportFilePath = NeedCreateTable(jobInfo);
            ReportCenterDao.SaveReportJobDatas(reportFilePath, baseJobs, this.INSERT_DATA_SQL);
        }

        public void InitCreateTableSQLString()
        {
            TABLE_NAME = ReportConstants.ReportDETAIL;
            CREATE_TABLE_SQL = string.Format(ReportConstants.CREATE_TABLE_Client_Audit_REPORT, TABLE_NAME);
            INSERT_DATA_SQL = string.Format(ReportConstants.INSERT_DATA_Client_Audit_REPORT, TABLE_NAME);
        }

        public override ReportFilter GetReportJobFilterData(BaseJobDto jobInfo)
        {
            InitCreateTableSQLString();
            string reportFilePath = DownloadReports(jobInfo);
            TABLE_NAME = ReportConstants.ReportDETAIL;
            var users = ReportCenterDao.GetDistinctValues(reportFilePath, TABLE_NAME, USERNAMECOLUMN);
            var userData = new List<ReportFilterData>();
            foreach (var user in users)
            {
                userData.Add(new ReportFilterData() { Name = user, });
            }
            //var actions = ReportCenterDao.GetDistinctValues(reportFilePath, TABLE_NAME, EVENTTYPENAMECOLUMN);
            //var actionData = new List<ReportFilterData>();
            //foreach (var action in actions)
            //{
            //    actionData.Add(new ReportFilterData() { Name = action, });
            //}
            var filters = new Dictionary<ReportFilterType, List<ReportFilterData>>();
            filters.Add(ReportFilterType.User, userData);
            //filters.Add(ReportFilterType.Action, actionData);

            return new ReportFilter()
            {
                Filters = filters,
            };
        }
    }
}
