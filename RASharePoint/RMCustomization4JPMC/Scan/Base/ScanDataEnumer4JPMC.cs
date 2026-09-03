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
using AvePoint.RA.SharePoint.RMCustomization4JPMC.Common.ApprovalService4JPMC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMCustomization4JPMC.Scan.Base
{
    public class ScanDataEnumer4JPMC : IEnumerable<ArchiveApproveReport4JPMC>, IEnumerator<ArchiveApproveReport4JPMC>, IDisposable
    {
        private ApprovalReportService4JPMC mApprovalReportService = null;
        public ScanDataEnumer4JPMC(ApprovalReportService4JPMC approvalReportService)
        {
            mApprovalReportService = approvalReportService;
        }
        object IEnumerator.Current
        {
            get
            {
                return null;
            }
        }

        private ArchiveApproveReport4JPMC mCurrent;
        public ArchiveApproveReport4JPMC Current
        {
            get
            {
                return mCurrent;
            }
        }

        public void Dispose()
        {
            if (mApprovalReportService is IDisposable)
            {
                (mApprovalReportService as IDisposable).Dispose();
            }
        }

        public IEnumerator<ArchiveApproveReport4JPMC> GetEnumerator()
        {
            return this;
        }

        public bool MoveNext()
        {
            var report = mApprovalReportService.FetchNext();
            if (report != null)
            {
                mCurrent = report.Value;
                return true;
            }
            else
            {
                mCurrent = null;
                return false;
            }
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
    }
}
