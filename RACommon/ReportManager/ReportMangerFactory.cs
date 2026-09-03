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
using AvePoint.GCommon;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Report
{
    public class ReportMangerFactory
    {
       // private RALogger logger = RALogger.GetInstance(typeof(ReportMangerFactory));

        private static ReportMangerFactory facotry = null;

        private IRMReportManager mReportManager;

        public IRMReportManager ReportManager
        {
            get
            {
                if (mReportManager == null)
                {
                    mReportManager = (IRMReportManager)PlatformWindsorManager.GetService(typeof(IRMReportManager));
                }
                return mReportManager;
            }
        }

        private readonly static object locker = new object();
        
        public static ReportMangerFactory Instance
        {
            get
            {
                //双重锁定提高效率节省CPU
                if (facotry == null)
                {
                    lock (locker)
                    {
                        if (facotry == null)
                        {
                            facotry = new ReportMangerFactory();
                        }
                    }
                }
                return facotry;
            }
        }
        public void Init(string jobId, JobType jobType, bool syncReport = false)
        {
            if (AvePoint.RA.Common.JobService.JobServiceUtility.IsSubJob(jobId))
            {
                mReportManager = new SubJobReportManager(jobId, jobType, syncReport);

            }
            else
            {
                mReportManager = new RMReportManager(jobId, jobType, syncReport);
            }
           
        }

        public void Init(IRMReportManager reportManager)
        {
            mReportManager = reportManager;
        }

    }
}
