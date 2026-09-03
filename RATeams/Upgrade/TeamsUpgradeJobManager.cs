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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RATeams.Upgrade
{
    public class TeamsUpgradeJobManager
    {
        private static readonly IRMReportManager s_reportManager = ReportMangerFactory.Instance.ReportManager;

        public bool HasSucceedDetail { get; set; }

        public bool HasFailedDetail { get; set; }

        public void Init(string jobId, AvePoint.RA.Contract.JobMonitor.JobType jobType, long total)
        {
            ReportMangerFactory.Instance.Init(jobId, jobType);
            s_reportManager.SetTotal(total);
            s_reportManager.StartUpdateJobProgress();
        }

        public void Init(string jobId, AvePoint.RA.Contract.JobMonitor.JobType jobType)
        {
            ReportMangerFactory.Instance.Init(jobId, jobType);
            s_reportManager.StartUpdateJobProgress();
        }

        public void IncreaseBase(long count)
        {
            s_reportManager.IncreaseBase(count);
        }

        public void Increase(int count)
        {
            s_reportManager.Increase(count);
        }

        public void AddRecordReport(List<JMConvertStubJobDetails> detailList)
        {
            foreach(var detail in detailList)
            {
                s_reportManager.SendJobDetail(detail);
            }
        }

        public void AddRecordReport(JMConvertStubJobDetails detail)
        {
            s_reportManager.SendJobDetail(detail);
        }

        public void AddRecordReport(int action, string fullPath, JobDetailsStatus status, string comment = "")
        {
            var detail = new JMConvertStubJobDetails
            {
                Action = action,
                FullPath = fullPath,
                Status = status,
                FinishTime = DateTime.UtcNow.Ticks,
                Comment = comment
            };

            s_reportManager.SendJobDetail(detail);
        }

        public void SetJobFinished(string jobComment)
        {
            var jobFinishStatus = HasSucceedDetail && HasFailedDetail ?
                JobStatus.FinishWithException :
                (
                    HasFailedDetail ?
                    JobStatus.Failed :
                    JobStatus.Finished
                );
            s_reportManager.SetJobFinished(jobFinishStatus, jobComment);
        }

        public JobStatus SetJobFinished()
        {
            var jobFinishStatus = HasSucceedDetail && HasFailedDetail ?
                JobStatus.FinishWithException :
                (
                    HasFailedDetail ?
                    JobStatus.Failed :
                    JobStatus.Finished
                );
            s_reportManager.SetJobFinished(jobFinishStatus);
            return jobFinishStatus;
        }
    }

    public enum TeamsUpgradeAction
    {
        ILUpgrade = 0,
        SOUpgrade = 1,
        DataUpgrade = 2,
    }
}
