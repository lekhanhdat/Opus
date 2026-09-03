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
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.FileSystem.Core;
using System.Collections.Generic;

namespace RAFileSystemCore.ReportSerializer.Details
{
    public class FSDisposalDetailWorker : AbstractDetailWorker
    {
        public override List<List<ReportCell>> ConvertReportData(IEnumerable<JMJobDetails> details)
        {
            List<List<ReportCell>> reportCells = new List<List<ReportCell>>();
            foreach (var detail in details)
            {
                if (detail is JMFSDisposalJobDetails)
                {
                    List<ReportCell> reports = new List<ReportCell>();
                    JMFSDisposalJobDetails reportnfo = detail as JMFSDisposalJobDetails;
                    //reports.Add(new ReportCell() { Key = "DetailTab", Value = reportnfo.DetailTab });
                    reports.Add(new ReportCell() { Key = "Type", Value = reportnfo.Type });
                    reports.Add(new ReportCell() { Key = "ObjectName", Value = reportnfo.ObjectName });
                    reports.Add(new ReportCell() { Key = "Size", Value = reportnfo.Size });
                    reports.Add(new ReportCell() { Key = "SourceLocation", Value = reportnfo.SourceLocation });
                    reports.Add(new ReportCell() { Key = "DestinationLocation", Value = reportnfo.DestinationLocation });
                    reports.Add(new ReportCell() { Key = "FinishTime", Value = reportnfo.FinishTime });
                    reports.Add(new ReportCell() { Key = "RuleName", Value = reportnfo.RuleName });
                    reports.Add(new ReportCell() { Key = "Action", Value = reportnfo.Action });
                    reports.Add(new ReportCell() { Key = "AgentName", Value = reportnfo.AgentName });
                    reports.Add(new ReportCell() { Key = "Status", Value = reportnfo.Status });
                    reports.Add(new ReportCell() { Key = "Comment", Value = reportnfo.Comment });
                    if (detail is JMFSDisposalJobDetailV2 detailV2)
                    {
                        reports.Add(new ReportCell() { Key = "Depth", Value = detailV2.Depth });
                        reports.Add(new ReportCell() { Key = "DirPath", Value = detailV2.DirPath });
                        reports.Add(new ReportCell() { Key = "DetailAction", Value = detailV2.DetailAction });
                    }
                    reportCells.Add(reports);
                }
            }
            return reportCells;
        }

        public override void InitCreateTableSQLString()
        {
            TABLE_NAME = JobMonitorConstants.JOBDETAIL;
            if (JobContext.Current.EnableFSHighPerformanceMode)
            {
                CREATE_TABLE_SQL = string.Format(JobMonitorConstants.CREATE_TABLE_FileSystem_DisposalV2, TABLE_NAME);
                INSERT_DATA_SQL = string.Format(JobMonitorConstants.INSERT_DATA_FileSystem_DisposalV2, TABLE_NAME);
            }
            else
            {
                CREATE_TABLE_SQL = string.Format(JobMonitorConstants.CREATE_TABLE_FileSystem_Disposal, TABLE_NAME);
                INSERT_DATA_SQL = string.Format(JobMonitorConstants.INSERT_DATA_FileSystem_Disposal, TABLE_NAME);
            }
        }
    }
}
