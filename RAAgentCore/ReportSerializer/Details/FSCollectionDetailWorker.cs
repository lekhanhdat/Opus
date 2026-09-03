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

namespace RAFileSystemCore.ReportSerializer
{
    public class FSCollectionDetailWorker : AbstractDetailWorker
    {
        public override List<List<ReportCell>> ConvertReportData(IEnumerable<JMJobDetails> details)
        {
            List<List<ReportCell>> reportCells = new List<List<ReportCell>>();
            foreach (var detail in details)
            {
                if (detail is FSDataSyncJobReportDetail)
                {
                    List<ReportCell> reports = new List<ReportCell>();
                    FSDataSyncJobReportDetail reportnfo = detail as FSDataSyncJobReportDetail;
                    reports.Add(new ReportCell() { Key = "ObjectName", Value = reportnfo.ObjectName });
                    reports.Add(new ReportCell() { Key = "FullPath", Value = reportnfo.FullPath });
                    reports.Add(new ReportCell() { Key = "Status", Value = reportnfo.Status });
                    reports.Add(new ReportCell() { Key = "AgentName", Value = reportnfo.AgentName });
                    reports.Add(new ReportCell() { Key = "Comment", Value = reportnfo.Comment });
                    if (detail is FSDataSyncJobReportDetailV2 detailV2)
                    {
                        reports.Add(new ReportCell() { Key = "Depth", Value = detailV2.Depth });
                        reports.Add(new ReportCell() { Key = "DirPath", Value = detailV2.DirPath });
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
                CREATE_TABLE_SQL = string.Format(JobMonitorConstants.CREATE_TABLE_FileSystem_DataSyncV2, TABLE_NAME);
                INSERT_DATA_SQL = string.Format(JobMonitorConstants.INSERT_DATA_FileSystem_DataSyncV2, TABLE_NAME);
            }
            else
            {
                CREATE_TABLE_SQL = string.Format(JobMonitorConstants.CREATE_TABLE_FileSystem_DataSync, TABLE_NAME);
                INSERT_DATA_SQL = string.Format(JobMonitorConstants.INSERT_DATA_FileSystem_DataSync, TABLE_NAME);
            }
        }
    }
}
