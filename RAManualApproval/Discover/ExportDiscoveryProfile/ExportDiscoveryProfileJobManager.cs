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
using System;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Contract.RMWeb.JobMonitor;

namespace RAManualApproval.Discover.ExportDiscoveryProfile
{
    public class ExportDiscoveryProfileJobManager
    {
        private static readonly IRMReportManager ReportManager = ReportMangerFactory.Instance.ReportManager;
        public static bool HasSucceedDetail { get; set; }

        public static bool HasFailedDetail { get; set; }

        public static string JobComment { get; set; }

        public static void Init(string jobId, AvePoint.RA.Contract.JobMonitor.JobType jobType)
        {
            ReportMangerFactory.Instance.Init(jobId, jobType);
            ReportManager.StartUpdateJobProgress(60);
        }

        public static void SetJobFinished()
        {
            var jobFinishStatus = HasSucceedDetail && HasFailedDetail ?
                JobStatus.FinishWithException :
                (
                    HasFailedDetail ?
                    JobStatus.Failed :
                    JobStatus.Finished
                );
            ReportManager.SetJobFinished(jobFinishStatus, JobComment);
        }

        public static void RecordJobDetail(JMDiscoveryExportProfileJobDetails exportJobDetail)
        {
            ReportManager.SendJobDetail(exportJobDetail);
        }
    }
}
