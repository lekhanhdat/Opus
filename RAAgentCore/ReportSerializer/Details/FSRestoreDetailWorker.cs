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
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.RA.Common.Database;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystemCore.ReportSerializer.Details
{
    public class FSRestoreDetailWorker : AbstractDetailWorker
    {

        internal string CREATE_SUMMAYTABLE_SQL { get; set; }
        internal string INSERT_SUMMAYDATA_SQL { get; set; }
        internal string SUMMAY_TABLE_NAME { get; set; }

        public override List<List<ReportCell>> ConvertReportData(IEnumerable<JMJobDetails> details)
        {
            List<List<ReportCell>> reportCells = new List<List<ReportCell>>();
            foreach (var detail in details)
            {
                if (detail is JMFSRestoreJobDetails)
                {
                    List<ReportCell> reports = new List<ReportCell>();
                    JMFSRestoreJobDetails reportnfo = detail as JMFSRestoreJobDetails;
                    reports.Add(new ReportCell() { Key = "Size", Value = reportnfo.Size });
                    reports.Add(new ReportCell() { Key = "SourceLocation", Value = reportnfo.SourceLocation });
                    reports.Add(new ReportCell() { Key = "FinishTime", Value = reportnfo.FinishTime });
                    reports.Add(new ReportCell() { Key = "Status", Value = reportnfo.Status });
                    reports.Add(new ReportCell() { Key = "Comment", Value = reportnfo.Comment });
                    reportCells.Add(reports);
                }
                else if(detail is JMAgentFSJMRestoreSummaryDetails)
                {
                    List<ReportCell> reports = new List<ReportCell>();
                    reports.Add(new ReportCell() { Key = "Statistics", Value = SerializerHelper.SerializeByJsonSerializer(detail) });
                    reportCells.Add(reports);
                }
            }
            return reportCells;
        }

        public override void InitCreateTableSQLString()
        {
            TABLE_NAME = JobMonitorConstants.JOBDETAIL;
            CREATE_TABLE_SQL = string.Format(JobMonitorConstants.CREATE_TABLE_FileSystem_Restore, TABLE_NAME);
            INSERT_DATA_SQL = string.Format(JobMonitorConstants.INSERT_DATA_FileSystem_Restore, TABLE_NAME);

            SUMMAY_TABLE_NAME = JobMonitorConstants.JOBSUMMAYDETAIL;
            CREATE_SUMMAYTABLE_SQL = string.Format(JobMonitorConstants.CREATE_TABLE_Restore_SUMMARYReport, SUMMAY_TABLE_NAME);
            INSERT_SUMMAYDATA_SQL = string.Format(JobMonitorConstants.INSERT_DATA_Restore_SUMMARYReport, SUMMAY_TABLE_NAME);
        }



        public override void SaveReportJobDatas(IEnumerable<JMJobDetails> jobDetails, string location)
        {
            InitCreateTableSQLString();
            var details = jobDetails.Where(item => !(item is JMAgentFSJMRestoreSummaryDetails));
            if (details.Any())
            {
                base.SaveReportJobDatas(details, location);
            }

            var summaryDetails = jobDetails.Where(item => item is JMAgentFSJMRestoreSummaryDetails);
            if (summaryDetails != null && summaryDetails.Count() > 0)
            {

                string reportFilePath = NeedCreateTable(location, SUMMAY_TABLE_NAME, CREATE_SUMMAYTABLE_SQL);
                List<List<SQLiteParameter>> parameterList = new List<List<SQLiteParameter>>();
                foreach (var detailcells in ConvertReportData(summaryDetails))
                {
                    List<SQLiteParameter> parameters = new List<SQLiteParameter>();
                    foreach (var item in detailcells)
                    {
                        parameters.Add(new SQLiteParameter(item.Key, item.Value));
                    }
                    parameterList.Add(parameters);
                }
                SQLiteUtil.BatchExecuteNonQueryStable(RAlogger, reportFilePath, this.INSERT_SUMMAYDATA_SQL, parameterList);
            }
        }
    }
}
