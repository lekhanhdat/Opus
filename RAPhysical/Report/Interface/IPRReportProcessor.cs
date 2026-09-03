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
using AvePoint.RA.Common.Report;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree;
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.RAPhysical.Tree.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.Report.Interface
{
    public interface IPRReportProcessor
    {
        IPRReportProcessor ConfigRecordAction(Func<IPhysicalRecord,Task> action);
        IPRReportProcessor ConfigTreeAction(Func<IPRTreeService,Task> action);
        IPRReportProcessor ConfigGetTreeFun(Func<string, Task<List<RMLocationProfileNode>>> getTreeFun);
        Task ProcessAsync(ReportOptions options);
        IPRTreeService PRTreeService { get; set; }
        IRMReportManager ReportManager { get; }
        IRMReportService mRMReportService { get; set; }
        void AddJobDetail(JMJobDetails detail);
        void AddJobReport(BaseReport report);
        void BatchAddJobDetail(IEnumerable<JMJobDetails> details);
    }
}
