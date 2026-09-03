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
using AvePoint.RA.Common.Database;
using System.Collections.Generic;
using System.Data.SQLite;

namespace RAFileSystemCore.ReportSerializer
{
    abstract public class AbstractDetailWorker : AbstractBaseWorker<JMJobDetails>
    {
        public AbstractDetailWorker()
        {
            TABLE_NAME = JobMonitorConstants.JOBDETAIL;
        }

        public abstract void InitCreateTableSQLString();

        public abstract List<List<ReportCell>> ConvertReportData(IEnumerable<JMJobDetails> details);

        public override void SaveReportJobDatas(IEnumerable<JMJobDetails> jobDetails, string location)
        {
            InitCreateTableSQLString();
            string reportFilePath = NeedCreateTable(location);

            List<List<SQLiteParameter>> parameterList = new List<List<SQLiteParameter>>();
            foreach (var detailcells in ConvertReportData(jobDetails))
            {
                List<SQLiteParameter> parameters = new List<SQLiteParameter>();
                foreach (var item in detailcells)
                {
                    parameters.Add(new SQLiteParameter(item.Key, item.Value));
                }
                parameterList.Add(parameters);
            }
            SQLiteUtil.BatchExecuteNonQueryStable(RAlogger, reportFilePath, this.INSERT_DATA_SQL, parameterList);
        }
    }
}
