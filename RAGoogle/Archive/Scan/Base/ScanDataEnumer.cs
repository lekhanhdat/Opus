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
using RAGoogle.Archive.ApprovalService;
using System.Collections;

namespace RAGoogle.Archive.Scan.Base
{
    public class ScanDataEnumer : IEnumerable<ArchiveApproveReport>, IEnumerator<ArchiveApproveReport>, IDisposable
    {
        private ApprovalReportService mApprovalReportService = null;
        public ScanDataEnumer(ApprovalReportService approvalReportService)
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

        private ArchiveApproveReport mCurrent;
        public ArchiveApproveReport Current
        {
            get
            {
                return mCurrent;
            }
        }

        public void Dispose()
        {
        }

        public IEnumerator<ArchiveApproveReport> GetEnumerator()
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
