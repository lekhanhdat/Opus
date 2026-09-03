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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RADownloadCenter
{
    public class GenerateAndUploadFileManager
    {
        private static readonly IRMReportManager ReportManager = ReportMangerFactory.Instance.ReportManager;

        public static bool HasSucceed { get; set; }

        public static bool HasFailed { get; set; }

        public static string? JobComment { get; set; }

        public static List<JMJobDetails> JobDetailList = new();

        public static void Init(string jobId, AvePoint.RA.Contract.JobMonitor.JobType jobType)
        {
            ReportMangerFactory.Instance.Init(jobId, jobType);
            ReportManager.StartUpdateJobProgress(60);
        }
        public static void AddFailedJobDetail(JMJobDetails detail)
        {
            ReportManager.SendJobDetail(detail);

            HasFailed = true;
        }

        public static void AddSucceedJobDetail(JMJobDetails detail)
        {
            ReportManager.SendJobDetail(detail);

            HasSucceed = true;
        }

        public static void SendJobDetail()
        {
            ReportManager.BatchSendJobDetail(JobDetailList);
        }

        public static void SetJobFinished()
        {
            var status = JobStatus.Finished;
            if (HasFailed && HasSucceed)
            {
                status = JobStatus.FinishWithException;
            }
            else if (HasFailed)
            {
                status = JobStatus.Failed;
            }

            ReportManager.SetJobFinished(status, JobComment);

        }
    }
}
