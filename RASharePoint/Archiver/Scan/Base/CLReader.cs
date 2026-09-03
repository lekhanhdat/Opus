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
using AvePoint.RA.Contract;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Archiver.Scan.Base
{
    internal class CLReader : IEnumerable<ArchiveApproveReport>, IEnumerator<ArchiveApproveReport>, IDisposable
    {
        private ScheduleConfiguration mConfiguration;
        private IBackwardDependencyNodeCache<ArchiveApproveReport> mApprovalReportProxy;
        private ApprovalReportService mApprovalService;
        private PCContainer<ArchiveApproveReport> mPcContainer = null;

        public CLReader(ScheduleConfiguration cfg, PCContainer<ArchiveApproveReport> pcContainer)
        {
            mConfiguration = cfg;
            mPcContainer = pcContainer;
            mApprovalService = new ApprovalReportService(mConfiguration);
        }
        public bool SkipUntilNextType(CacheNodeType type)
        {
            while ((int)Current.NodeType != (int)type)
            {
                if (!this.MoveNext())
                {
                    return false;
                }
            }
            return true;
        }

        private ArchiveApproveReport mCurrent;
        public ArchiveApproveReport Current
        {
            get
            {
                return mCurrent;
            }
        }

        public bool MoveNext()
        {
            return (mCurrent = mPcContainer.Consume()) != null;
        }
        public void Reset()
        {
            //this.mApprovalService.Reset();
        }

        object IEnumerator.Current
        {
            get
            {
                return null;
            }
        }

        public IEnumerator<ArchiveApproveReport> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
        public void Dispose()
        {
            using (mApprovalReportProxy) { }
            using (mApprovalService) { }
            //We must do not Dispose PCContainer here
        }
    }
}
