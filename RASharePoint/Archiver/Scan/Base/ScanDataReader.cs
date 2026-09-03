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
using AvePoint.RA.SharePoint.ArchiverCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Archiver
{
    internal class ScanDataReader : IScanDataReader
    {
        private ApprovalReportService mScanDB = null;
        public ScanDataReader(ScheduleConfiguration config) 
        {
            mScanDB = new ApprovalReportService(config);
        }
        public List<string> GetAllRuleIds()
        {
            return mScanDB.GetDataRuleCollection();
        }

        public ScanDataEnumer GetArchiveApproveReports(string ruleId)
        {
            mScanDB.ResetRuleId(ruleId);
            return new ScanDataEnumer(mScanDB);
        }

        public long GetDataCount(int minCacheNodeType = 0)
        {
            return mScanDB.GteDataCount(minCacheNodeType);
        }

        public Dictionary<int, long> GetDataCounts(int minCacheNodeType = 0, string ruleId = "")
        {
            return mScanDB.GetDataCounts(minCacheNodeType, ruleId);
        }

        public List<Guid> ExistInScanJob(List<Guid> nodeIds)
        {
            return mScanDB.ExistInScanJob(nodeIds);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
