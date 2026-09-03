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
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using RAReportCenter.Model;

namespace RAReportCenter.DisposalReport
{
    public class DisposalReportJobManager
    {

        private static IRMReportManager ReportManager;

        private static bool HasSucceedDetail { get; set; }

        private static bool HasFailedDetail { get; set; }

        public static void Init(string jobId)
        {
            ReportMangerFactory.Instance.Init(jobId, AvePoint.RA.Contract.JobMonitor.JobType.DisposalReport, true);
            ReportManager = ReportMangerFactory.Instance.ReportManager;
            ReportManager.StartUpdateJobProgress();
        }

        public static void IncreaseBase(long count)
        {
            ReportManager.IncreaseBase(count);
        }

        public static void Increase()
        {
            ReportManager.Increase();
        }

        public static void AddSucceedJobDetail(SourceNeedReportNode nodeInfo, string typeI18nKey)
        {
            var detail = new JMReportJobDetails
            {
                Type = typeI18nKey,
                TitleOrName = nodeInfo.LeafName,
                Url = nodeInfo.FullPath,
                Status = JobDetailsStatus.Successful
            };
            ReportManager.SendJobDetail(detail);

            HasSucceedDetail = true;
        }

        public static void AddFailedJobDetail(SourceNeedReportNode nodeInfo, string typeI18nKey, string comment)
        {
            var detail = new JMReportJobDetails
            {
                Type = typeI18nKey,
                TitleOrName = nodeInfo.LeafName,
                Url = nodeInfo.FullPath,
                Status = JobDetailsStatus.Successful,
                Comment = comment
            };
            ReportManager.SendJobDetail(detail);

            HasFailedDetail = true;
        }

        public static void AddJobReport(DueDisposalReport report)
        {
            ReportManager.SendJobReport(report);
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
            ReportManager.SetJobFinished(jobFinishStatus);
        }

        public static void SetJobFailed(string comment)
        {
            ReportManager.SetJobFinished(JobStatus.Failed, comment);
        }
    }
}
