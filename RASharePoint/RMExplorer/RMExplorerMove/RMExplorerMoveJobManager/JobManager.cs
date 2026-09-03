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
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.Job;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Common.Report;
using AvePoint.Records.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    //对IRMReportManager 进行封装，添加了HasErrorNode 属性，标记job状态
    public class JobManagement
    {
        protected static DateTime JobStartTime = DateTime.MinValue;

        public IRMReportManager ReportManager { get; private set; }

        public bool HasErrorNode { get; set; }

        private static JobManagement jobManager = null;
        private readonly static object mLock = new object();
        public static JobManagement GetInstance(RMExplorerMoveJobMessage message)
        {
            if (jobManager == null)
            {
                lock (mLock)
                {
                    if (jobManager == null)
                    {
                        jobManager = new JobManagement(message);
                    }
                }
            }
            return jobManager;
        }

        private JobManagement(RMExplorerMoveJobMessage message)
        {
            JobStartTime = DateTime.UtcNow;
            //ReportMangerFactory.Instance.Init(message.JobID, Contract.JobMonitor.JobType.RecordsExplorerMove);
            ReportManager = ReportMangerFactory.Instance.ReportManager;
            ReportManager.Increase(1);
            ReportManager.StartUpdateJobProgress();
        }


        public void Finish()
        {
            var jobStatus = Contract.RMWeb.JobMonitor.JobStatus.Finished;
            if (HasErrorNode)
            {
                jobStatus = Contract.RMWeb.JobMonitor.JobStatus.FinishWithException;
            }
            ReportManager.SetJobFinished(jobStatus);
        }
    }
}
