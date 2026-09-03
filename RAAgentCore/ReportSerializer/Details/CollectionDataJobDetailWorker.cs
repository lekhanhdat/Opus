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
using AvePoint.RA.Contract.Global.RMWeb.JobMonitor;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystemCore.ReportSerializer.Details
{
    public class CollectionDataJobDetailWorker : AbstractDetailWorker
    {
        public override List<List<ReportCell>> ConvertReportData(IEnumerable<JMJobDetails> details)
        {
            List<List<ReportCell>> reportCells = new List<List<ReportCell>>();
            foreach (var detail in details)
            {
                if (detail is JMCollectionDataJobDetails)
                {
                    List<ReportCell> reports = new List<ReportCell>();
                    JMCollectionDataJobDetails reportnfo = detail as JMCollectionDataJobDetails;
                    reports.Add(new ReportCell() { Key = "ObjectName", Value = reportnfo.ObjectName });
                    reports.Add(new ReportCell() { Key = "FullPath", Value = reportnfo.FullPath });
                    reports.Add(new ReportCell() { Key = "AgentName", Value = reportnfo.AgentName });
                    reports.Add(new ReportCell() { Key = "Status", Value = reportnfo.Status });
                    reports.Add(new ReportCell() { Key = "Comment", Value = reportnfo.Comment });
                    reportCells.Add(reports);
                }
            }
            return reportCells;
        }

        public override void InitCreateTableSQLString()
        {
            TABLE_NAME = JobMonitorConstants.JOBDETAIL;
            CREATE_TABLE_SQL = string.Format(JobMonitorConstants.CREATE_TABLE_COLLECTION_DATA_SETTING, TABLE_NAME);
            INSERT_DATA_SQL = string.Format(JobMonitorConstants.INSERT_DATA_COLLECTION_DATA_SETTING, TABLE_NAME);
        }
    }
}
