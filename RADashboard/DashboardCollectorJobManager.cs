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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RADashboard
{
    public class DashboardCollectorJobManager
    {

        private static readonly IRMReportManager ReportManager = ReportMangerFactory.Instance.ReportManager;

        public static bool HasError { get; set; }

        public static bool HasSuccess { get; set; }

        private class DashboardCollectorEventJobDetail
        {
            public string EventI18n { get; set; }

            public JobDetailsStatus Status { get; set; } = JobDetailsStatus.Successful;

            public string Comment { get; set; } = string.Empty;

            public DashboardCollectorEventJobDetail(CollectorEventType eventType)
            {
                EventI18n = EventTypeI18ns[eventType];
            }
        }

        private static readonly Dictionary<SourceFlag, string> SourceFlagI18ns = new Dictionary<SourceFlag, string>
        {
            { SourceFlag.SharePoint, "RM_JS_SPS_TabLabel_SP" },
            { SourceFlag.SharePointOnPrem, "RM_JS_SPS_TabLabel_SPLocal" },
            { SourceFlag.Physical, "RM_JS_SPS_TabLabel_Physical" },
            { SourceFlag.OneDrive, "RM_JS_SPS_TabLabel_OneDrive" },
            { SourceFlag.FileSystem, "RM_JS_SPS_TabLabel_FS" },
            { SourceFlag.Exchange, "RM_JS_SPS_TabLabel_EXO" },
            { SourceFlag.AzureFileShare,"RM_JS_SPS_TabLabel_AF"},
            { SourceFlag.Box,"RM_JS_SPS_TabLabel_Box"},
            { SourceFlag.Google,"RM_JS_SPS_TabLabel_Google"},
            { SourceFlag.All, "RM_JS_SPS_TabLabel_All" },
            { SourceFlag.Teams, "RM_JS_SPS_TabLabel_Teams" },
            { SourceFlag.GGControl, "RM_JS_SPS_TabLabel_Google" },
        };

        private static readonly Dictionary<CollectorEventType, string> EventTypeI18ns = new Dictionary<CollectorEventType, string>
        {
            { CollectorEventType.DataUsage, "RM_DSB_ManagedAndDestroyedRecords" },
            { CollectorEventType.DataUsageOfDate, "RM_DSB_DataUsageOfDate" },
            { CollectorEventType.TermApplyRuleUsage, "RM_DSB_TermApplyRuleUsage" },
            { CollectorEventType.TermUsage, "RM_DSB_TermUsage" },
            { CollectorEventType.UserWaitingApprovalCount, "RM_DSB_RecordWaiting" },
            { CollectorEventType.CheckHoldStatus, "RM_DSB_CheckHoldStatus" },
            { CollectorEventType.CollectArchivedSiteInfo, "RM_AR_DSB_CollectArchiverSiteInfo" },
            { CollectorEventType.CollectArchivedTeamsGroupInfo, "RM_AR_DSB_CollectArchiverTeamsInfo" }
        };

        private static readonly Dictionary<SourceFlag, Dictionary<CollectorEventType, DashboardCollectorEventJobDetail>> SourceJobEventStatus 
            = new Dictionary<SourceFlag, Dictionary<CollectorEventType, DashboardCollectorEventJobDetail>>
        {
                {SourceFlag.All, new Dictionary<CollectorEventType, DashboardCollectorEventJobDetail>
                    {
                        { CollectorEventType.CheckHoldStatus, new DashboardCollectorEventJobDetail(CollectorEventType.CheckHoldStatus)}
                    }
                }
        };

        public static void Init(string jobId, IEnumerable<SourceFlag> sourceFlags, bool needCollectTermWithRule)
        {
            ReportMangerFactory.Instance.Init(jobId, AvePoint.RA.Contract.JobMonitor.JobType.Dashboard);
            ReportManager.StartUpdateJobProgress(30);
            foreach (var sourceFlag in sourceFlags)
            {
                SourceJobEventStatus.Add(sourceFlag, InitEventJobStatus());
            }
            if (needCollectTermWithRule)
            {
                SourceJobEventStatus[SourceFlag.All][CollectorEventType.TermApplyRuleUsage] = new DashboardCollectorEventJobDetail(CollectorEventType.TermApplyRuleUsage);
            }
        }

        public static void AddFailedJobDetail(SourceFlag sourceFlag, CollectorEventType eventType, string comment)
        {
            SourceJobEventStatus[sourceFlag][eventType].Status = JobDetailsStatus.Failed;
            SourceJobEventStatus[sourceFlag][eventType].Comment = comment;
            HasError = true;
        }

        public static void AddSOFailedJobDetail(CollectorEventType eventType, string comment, SourceFlag sourceFlag = SourceFlag.All)
        {
            ReportManager.SendJobDetail(new JMDashboardJobDetail
            {
                Action = new DashboardCollectorEventJobDetail(eventType).EventI18n,
                Status = JobDetailsStatus.Failed,
                SourceFlag = SourceFlagI18ns.TryGetValue(sourceFlag, out var value) ? value : SourceFlagI18ns[SourceFlag.All],
                Comment = comment
            });
            HasError = true;
        }

        public static void SetJobFailed(string comment)
        {
            ReportManager.SetJobFinished(JobStatus.Failed, comment);
        }

        public static void AddSuccessJobDetail(CollectorEventType eventType, string comment, SourceFlag sourceFlag = SourceFlag.All)
        {
            ReportManager.SendJobDetail(new JMDashboardJobDetail
            {
                Action = new DashboardCollectorEventJobDetail(eventType).EventI18n,
                Status = JobDetailsStatus.Successful,
                SourceFlag = SourceFlagI18ns.TryGetValue(sourceFlag, out var value) ? value : SourceFlagI18ns[SourceFlag.All],
                Comment = comment
            });
            HasSuccess = true;
        }

        public static void SetJobFinish()
        {
            var hasError = false;
            var hasSuccess = false;

            foreach(var sourceJobEvent in SourceJobEventStatus)
            {
                var eventJobDetails = sourceJobEvent.Value.Values.ToList();
                foreach(var eventJobDetail in eventJobDetails) 
                {
                    
                    ReportManager.SendJobDetail(new JMDashboardJobDetail
                    {
                        Action = eventJobDetail.EventI18n,
                        Status = eventJobDetail.Status,
                        SourceFlag = SourceFlagI18ns[sourceJobEvent.Key],
                        Comment = eventJobDetail.Comment
                    });

                    if (eventJobDetail.Status == JobDetailsStatus.Successful)
                    {
                        hasSuccess = true;
                    }
                    else
                    {
                        hasError = true;
                    }
                }
            }

            if(HasSuccess)
            {
                hasSuccess = HasSuccess;
            }
            if (HasError)
            {
                hasError = HasError;
            }

            var jobFinishStatus = hasSuccess && hasError ?
                JobStatus.FinishWithException :
                (hasSuccess ? JobStatus.Finished : JobStatus.Failed);
            ReportManager.SetJobFinished(jobFinishStatus);
        }

        public static void SetOnlySOJobFinish()
        {
            var jobFinishStatus = HasSuccess && HasError ?
                JobStatus.FinishWithException :
                (HasSuccess ? JobStatus.Finished : JobStatus.Failed);
                ReportManager.SetJobFinished(jobFinishStatus);
        }

        private static Dictionary<CollectorEventType, DashboardCollectorEventJobDetail> InitEventJobStatus()
        {
            return new Dictionary<CollectorEventType, DashboardCollectorEventJobDetail>
            {
                { CollectorEventType.DataUsage, new DashboardCollectorEventJobDetail(CollectorEventType.DataUsage) },
                { CollectorEventType.DataUsageOfDate, new DashboardCollectorEventJobDetail(CollectorEventType.DataUsageOfDate) },
                { CollectorEventType.TermUsage, new DashboardCollectorEventJobDetail(CollectorEventType.TermUsage) },
                { CollectorEventType.UserWaitingApprovalCount, new DashboardCollectorEventJobDetail(CollectorEventType.UserWaitingApprovalCount) }
            };
        }
    }
}
