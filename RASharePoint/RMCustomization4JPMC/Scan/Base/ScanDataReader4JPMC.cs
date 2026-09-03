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
using AvePoint.RA.SharePoint.Archiver;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.RMCustomization4JPMC.Common.ApprovalService4JPMC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMCustomization4JPMC.Scan.Base
{
    internal class ScanDataReader4JPMC : IScanDataReader4JPMC
    {
        private ApprovalReportService4JPMC mScanDB = null;
        public ScanDataReader4JPMC(ScheduleConfiguration config)
        {
            mScanDB = new ApprovalReportService4JPMC(config);
        }

        public ScanDataEnumer4JPMC GetArchiveApproveReports(string ruleId)
        {
            mScanDB.ResetRuleId(ruleId);
            return new ScanDataEnumer4JPMC(mScanDB);
        }

        public List<Guid> ExistInScanJob(List<Guid> nodeIds)
        {
            return mScanDB.ExistInScanJob(nodeIds);
        }

        public void Dispose()
        {
            if (mScanDB is IDisposable)
            {
                (mScanDB as IDisposable).Dispose();
            }
        }

        public List<ArchiveApproveReport4JPMGroupBy> GetArchiveApproveReportsGroupByColumns(string ruleId)
        {
            return mScanDB.ReadFromApproveDBGroupByColumns(ruleId);
        }

        public List<ArchiveApproveReport4JPMGroupBy> GetArchiveApproveReportsGroupByColumns(string ruleId, string listId)
        {
            return mScanDB.ReadFromApproveDBGroupByColumns(ruleId, listId);
        }
        
        public List<ArchiveApproveReport4JPMTotalSize> GetArchiveApproveReportsTotalSize(string ruleId, string listId = "")
        {
            return mScanDB.ReadFromApproveDBTotalSize(ruleId, listId);
        }
    }
}
