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

namespace RAManualApproval.EmailSchedule
{
    public class ManualApprovalEmailScheduleJobManager
    {
        private static readonly IRMReportManager ReportManager = ReportMangerFactory.Instance.ReportManager;

        private static bool HasSucceed { get; set; }

        private static bool HasFailed { get; set; }

        private static readonly Dictionary<SettingAction, string> ActionI18Ns = 
            new Dictionary<SettingAction, string>
        {
                {SettingAction.Notification, "RM_MA_Setting_Email_Notification" },
                {SettingAction.Approved, "RM_MA_Setting_Approved" },
                {SettingAction.Rejected, "RM_MA_Setting_Rejected" },
                {SettingAction.Reassign, "RM_MA_Setting_Reassign" },
        };

        public static void Init(string jobId)
        {
            ReportMangerFactory.Instance.Init(jobId, AvePoint.RA.Contract.JobMonitor.JobType.ManualApprovalEmailSchedule);
            ReportManager.StartUpdateJobProgress(60);
        }

        public static void AddSucceedJobDetail(string titleOrName, SettingAction action)
        {
            HasSucceed = true;
            ReportManager.SendJobDetail(new JMManualApprovalSettingScheduleDetail
            {
                TitleOrName = titleOrName,
                Action = ActionI18Ns[action],
                Status = JobDetailsStatus.Successful,
                Comment = ""
            });
        }

        public static void AddFailedJobDetail(string titleOrName, SettingAction action, string comment)
        {
            HasFailed = true;
            ReportManager.SendJobDetail(new JMManualApprovalSettingScheduleDetail
            {
                TitleOrName = titleOrName,
                Action = ActionI18Ns[action],
                Status = JobDetailsStatus.Failed,
                Comment = comment
            });
        }

        public static void SetJobFailed(string comment)
        {
            ReportManager.SetJobFinished(JobStatus.Failed, comment);
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

            ReportManager.SetJobFinished(status);

        }
    }

    public enum SettingAction
    {
        None = 0,
        Notification = 1,
        Approved = 2,
        Rejected = 3,
        Reassign = 4
    }
}
